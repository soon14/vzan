using BLL.MiniApp.Ent;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entity.MiniApp.Pin.PinEnums;

namespace BLL.MiniApp.Pin
{
    public class PinGroupBLL : BaseMySql<PinGroup>
    {
        #region 单例模式
        private static PinGroupBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinGroupBLL()
        {

        }

        public static PinGroupBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinGroupBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<PinGroup> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PinGroup>();

            return base.GetList($"id in ({ids})");
        }

        /// <summary>
        /// 创建拼团
        /// </summary>
        /// <param name="pinOrder"></param>
        /// <returns></returns>
        public bool CreateGroup(PinGoodsOrder pinOrder, PinGroup group)
        {
            group = new PinGroup()
            {
                aid = pinOrder.aid,
                storeId = pinOrder.storeId,
                goodsId = pinOrder.goodsId,
                groupCount = pinOrder.goodsPhotoModel.groupUserLimit,
                entrantCount = 1,
            };
            group.endTime = group.startTime.AddDays(1);
            pinOrder.groupId = group.id = Convert.ToInt32(Add(group));

            return pinOrder.groupId > 0;
        }
        /// <summary>
        /// 更新参团人数
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool UpdateEntrantCount(int groupId, out string msg, ref PinGroup pinGroup)
        {
            bool result = false;
            pinGroup = GetModel(groupId);
            if (pinGroup == null)
            {
                msg = "拼团不存在";
                return result;
            }
            if (pinGroup.state == (int)PinEnums.GroupState.未成团 && DateTime.Compare(pinGroup.endTime, DateTime.Now) > 0)
            {
                pinGroup.entrantCount++;
                pinGroup.changeTime = DateTime.Now;
                pinGroup.state = pinGroup.entrantCount == pinGroup.groupCount ? (int)PinEnums.GroupState.已成团 : (int)PinEnums.GroupState.未成团;
                result = Update(pinGroup, "changetime,entrantCount,state");
                msg = result ? "" : "系统繁忙 group update error";
                return result;
            }
            else
            {
                msg = "本拼团活动已结束";
                return result;
            }

        }

        /// <summary>
        /// 回滚参团人数
        /// </summary>
        /// <param name="pinOrder"></param>
        /// <returns></returns>
        public bool RollbackEntrantCount(PinGoodsOrder pinOrder, ref string msg, TransactionModel tran = null)
        {
            bool isTran = tran != null;
            if (!isTran) tran = new TransactionModel();
            string sqlwhere = $"id={pinOrder.groupId}";
            PinGroup group = GetModel(sqlwhere);
            if (group == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"拼团回滚失败{pinOrder.groupId}"));
                return false;
            }
            if (group.state == (int)PinEnums.GroupState.已成团 || group.state == (int)PinEnums.GroupState.未成团)
            {
                //交易取消和退款不能重复减去参团人数
                if (pinOrder.state > (int)PinEnums.PinOrderState.交易取消 || pinOrder.payState != (int)PayState.已退款)
                {
                    group.entrantCount--;
                }
                if (group.entrantCount <= 0)
                {
                    group.state = (int)PinEnums.GroupState.拼团失败;
                    //log4net.LogHelper.WriteInfo(GetType(), "1"+group.stateStr);
                }
                else if (group.entrantCount < group.groupCount)
                {
                    //log4net.LogHelper.WriteInfo(GetType(), "2" + group.stateStr);

                    group.state = (int)PinEnums.GroupState.未成团;
                    if (pinOrder.payState == (int)PayState.已付款)
                    {
                        group.endTime = DateTime.Now.AddDays(1);
                    }
                }
                //log4net.LogHelper.WriteInfo(GetType(), "3" + group.stateStr);

                group.changeTime = DateTime.Now;
            }
            MySqlParameter[] parames = null;
            tran.Add(BuildUpdateSql(group, "state,endTime,entrantCount,changeTime", out parames), parames);
            if (isTran) return true;
            return base.ExecuteTransaction(tran.sqlArray, tran.ParameterArray);
        }

        public PinGroup GetModelByAid_StoreId_Id(int aid, int storeId, int id)
        {
            PinGroup group = null;
            if (aid <= 0 || storeId <= 0 || id <= 0)
            {
                return group;
            }
            string sqlwhere = $" aid={aid} and storeId={storeId} and id={id}";
            group = GetModel(sqlwhere);
            return group;
        }

        /// <summary>
        /// 拼团成功，返利提现到零钱
        /// </summary>
        /// <returns></returns>
        public string ReturnMoney(PinGroup groupInfo)
        {
            string msg = string.Empty;
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            TransactionModel tran = new TransactionModel();
            string sqlwhere = $"state={(int)PinEnums.PinOrderState.交易成功} and paystate={(int)PinEnums.PayState.已付款} and groupid ={groupInfo.id} and isReturnMoney=0";
            List<PinGoodsOrder> orderList = pinGoodsOrderBLL.GetList(sqlwhere);
            if (orderList == null || orderList.Count <= 0)
            {
                msg = $"拼团成功，找不到交易成功且已付款的订单 groupids:{groupInfo.id}";
                log4net.LogHelper.WriteError(GetType(), new Exception(msg));
                groupInfo.state = (int)PinEnums.GroupState.返利失败;
                tran.Add(BuildUpdateSql(groupInfo, "state"));
            }
            else
            {
                string aids = string.Join(",",orderList.Select(s=>s.aid).Distinct());
                List<XcxAppAccountRelation> xcxAppAccountRelationList = XcxAppAccountRelationBLL.SingleModel.GetValuableListByIds(aids);

                foreach (var order in orderList)
                {
                    XcxAppAccountRelation xcxAppAccountRelation = xcxAppAccountRelationList?.FirstOrDefault(f=>f.Id == order.aid);
                    if (xcxAppAccountRelation == null)
                    {
                        msg += $"拼享惠返利，找不到平台小程序信息 aid:{order.id}||";
                        log4net.LogHelper.WriteError(GetType(), new Exception($"拼享惠返利，找不到平台小程序信息 aid:{order.id}"));
                        continue;
                    }
                   string str= DrawCashApplyBLL.SingleModel.PxhUserApplyDrawCash(order, order.userId, xcxAppAccountRelation.AppId);
                    if (!string.IsNullOrEmpty(str))
                    {
                        msg += $"拼享惠提现错误，orderId:{order.id}，{str}";
                        log4net.LogHelper.WriteError(GetType(), new Exception($"拼享惠提现错误，orderId:{order.id}，{str}"));
                    }
                }
                groupInfo.state = (int)PinEnums.GroupState.已返利;
                Update(groupInfo, "state");
               // tran.Add(BuildUpdateSql(groupInfo, "state"));
            }
            //if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
            //{
            //    msg = $"拼团成功返利处理失败 groupids:{groupInfo.id}，sql:{JsonConvert.SerializeObject(tran.sqlArray)}";
            //    log4net.LogHelper.WriteError(GetType(), new Exception($"拼团成功返利处理失败 groupids:{groupInfo.id}，sql:{JsonConvert.SerializeObject(tran.sqlArray)}"));
            //}
            return msg;
        }

        public string CheckGroupSuccess()
        {
            try
            {
                //屏蔽自运营拼享惠 aid:6933593
                List<PinGroup> groupList = GetList($" groupcount<=successCount and state={(int)PinEnums.GroupState.拼团成功} and aid!=6933593");
                string msg = string.Empty;
                if (groupList != null && groupList.Count > 0)
                {
                    foreach (var groupInfo in groupList)
                    {
                        msg += ReturnMoney(groupInfo);
                    }
                }
                return msg;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"检查拼团成功失败：{ex.Message}"));
                return ex.Message;
            }

        }

        public string CheckGroupTimeout()
        {
            string msg = string.Empty;
            List<PinGroup> groupList = GetList($"state={(int)PinEnums.GroupState.未成团} and successCount>0 and endtime<'{DateTime.Now}'");
            if (groupList != null && groupList.Count > 0)
            {
                
                foreach (var group in groupList)
                {
                    TransactionModel tran = new TransactionModel();
                    group.state = (int)PinEnums.GroupState.拼团失败;
                    group.changeTime = DateTime.Now;
                    MySqlParameter[] parameter = null;
                    tran.Add(BuildUpdateSql(group, "state,changeTime", out parameter), parameter);
                    PinStoreBLL.SingleModel.UpdateIncome(group, tran);
                    if (!base.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
                    {
                        msg += $"拼团过期处理失败groupId:{group.id}";
                    }
                }
            }
            return msg;
        }

        /// <summary>
        /// 退款修改参团状态
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msg"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        //public bool UpdateGruopState(PinGoodsOrder order, ref string msg, TransactionModel tran = null)
        //{
        //    bool istran = tran != null;
        //    PinGroupBLL pinGroupBLL = new PinGroupBLL();
        //    PinGroup group = pinGroupBLL.GetModel(order.groupId);
        //    if (group != null && order.state != (int)PinEnums.PinOrderState.交易取消)
        //    {
        //        MySqlParameter[] pone = null;
        //        //退款和取消订单不可重复减少参团人数
        //        group.entrantCount--;
        //        if (group.successCount >= group.groupCount)
        //        {
        //            group.state = (int)PinEnums.GroupState.拼团成功;
        //        }
        //        else if (group.entrantCount < group.groupCount)
        //        {
        //            group.state = (int)PinEnums.GroupState.未成团;
        //            group.endTime = DateTime.Now.AddDays(1);
        //        }
        //        else
        //        {
        //            group.state = (int)PinEnums.GroupState.已成团;
        //        }
        //        tran.Add(pinGroupBLL.BuildUpdateSql(group, "entrantCount,state,endTime", out pone), pone);
        //    }
        //    if (istran) return true;
        //    return base.ExecuteTransaction(tran.sqlArray, tran.ParameterArray);

        //}
    }
}