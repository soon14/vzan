using DAL.Base;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entity.MiniApp.Pin.PinEnums;

namespace BLL.MiniApp.Pin
{
    public class PinRefundApplyBLL : BaseMySql<PinRefundApply>
    {
        #region 单例模式
        private static PinRefundApplyBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinRefundApplyBLL()
        {

        }

        public static PinRefundApplyBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinRefundApplyBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<PinRefundApply> GetListByCondition(int aid, int storeId, string goodsName, int state, string nickName, string consignee, string phone, int sendWay, int type, string serviceNo, int pageIndex, int pageSize, out int recordCount)
        {
            List<PinRefundApply> list = new List<PinRefundApply>();
            recordCount = 0;
            string sql = $" select a.* from pinrefundapply a left join pingoodsorder b on a.orderid=b.id left join c_userInfo c on a.userid=c.id";
            string sqlwhere = $" where a.aid={aid} and a.storeid={storeId}";
            if (state != -999)
            {
                sqlwhere += $" and a.state={state}";
            }
            else
            {
                sqlwhere += $" and a.state>{(int)RefundApplyState.删除}";
            }
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(serviceNo))
            {
                sqlwhere += $" and a.serviceNo like @serviceNo";
                parameters.Add(new MySqlParameter("@serviceNo", $"%{serviceNo}%"));
            }
            if (type != -999)
            {
                sqlwhere += $" and a.type={type}";
            }
            if (!string.IsNullOrEmpty(goodsName))
            {
                List<MySqlParameter> paras = new List<MySqlParameter>();
                paras.Add(new MySqlParameter("@goodsName", $"%{goodsName}%"));
                List<PinGoods> goodsList = PinGoodsBLL.SingleModel.GetList($" aid={aid} and storeid={storeId} and name like @goodsName", paras.ToArray());
                if (goodsList == null || goodsList.Count <= 0)
                {
                    return list;
                }
                string goodsIds = string.Join(",", goodsList.Select(goods => goods.id));
                sqlwhere += $" and b.goodsId in ({goodsIds})";
            }
            if (!string.IsNullOrEmpty(nickName))
            {
                sqlwhere += $" and c.nickname like @nickName";
                parameters.Add(new MySqlParameter("@nickName", $"%{nickName}%"));
            }
            if (!string.IsNullOrEmpty(consignee))
            {
                sqlwhere += $" and b.consignee like @consignee";
                parameters.Add(new MySqlParameter("@consignee", $"%{consignee}%"));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                sqlwhere += $" and b.phone like @phone";
                parameters.Add(new MySqlParameter("@phone", $"%{phone}%"));
            }
            if (sendWay != -999)
            {
                sqlwhere += $" and b.sendWay={sendWay}";
                parameters.Add(new MySqlParameter("@sendWay", $"%{sendWay}%"));
            }
            sql = $"{sql}{sqlwhere} order by a.id desc limit {(pageIndex - 1) * pageSize},{pageSize}";
            recordCount = GetCountBySql(sql, parameters.ToArray());
            list = GetListBySql(sql, parameters.ToArray());
            return list;
        }

        public List<PinRefundApply> GetListByAid_UserId(int aid, int userId, int pageIndex, int pageSize, out int recordCount)
        {
            string sqlwhere = $"aid={aid} and userid={userId} and state >{(int)RefundApplyState.删除}";
            recordCount = GetCount(sqlwhere);
            return GetList(sqlwhere, pageSize, pageIndex);
        }

        public int GetCountByOrderId(int orderId)
        {
            string sqlwhere = $" orderId={orderId} and state>{(int)RefundApplyState.删除}";
            return GetCount(sqlwhere);
        }
        /// <summary>
        /// 生成服务单号
        /// </summary>
        /// <param name="refundApply"></param>
        /// <returns></returns>
        public string CreateServiceNo(PinRefundApply refundApply,string outTradeNo)
        {
            int count = PinRefundApplyBLL.SingleModel.GetCountByOrderId(refundApply.orderId);
            return outTradeNo + (count + 1);
        }

        public int GetCountByOrderId_State(int orderId, int state)
        {
            string sqlwhere = $" orderId={orderId} and state={state}";
            return GetCount(sqlwhere);
        }
        /// <summary>
        /// 申请不通过处理
        /// </summary>
        /// <param name="apply"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool ApplyFail(PinRefundApply apply, ref string msg)
        {
            bool result = false;
            PinGoodsOrderBLL orderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = orderBLL.GetModel(apply.orderId);
            if (order == null)
            {
                msg = "订单不存在";
                return result;
            }
            order.state = apply.orderState;
            orderBLL.Update(order, "state");
            result = Update(apply, "state,updatetime");
            return result;
        }
        /// <summary>
        /// 申请通过处理
        /// </summary>
        /// <param name="apply"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public bool ApplySuccess(PinRefundApply apply, PinStore store, ref string msg)
        {
            bool result = false;
            if (apply.type == 2 || apply.type == 3)
            {
                if (string.IsNullOrEmpty(store.setting.place) || string.IsNullOrEmpty(store.setting.phone) || string.IsNullOrEmpty(store.setting.name))
                {
                    msg = "请前往店铺配置设置您的退换货寄件信息";
                    return result;
                }
            }
            PinGoodsOrderBLL orderBLL = new PinGoodsOrderBLL();
            
            PinGoodsOrder order = orderBLL.GetModel(apply.orderId);
            if (order == null)
            {
                msg = "订单不存在";
                return result;
            }
            order.state = apply.orderState;
            order.returnCount += apply.count;
            order.state = order.returnCount == order.count ? order.state = (int)PinOrderState.交易取消 : order.state = apply.orderState;
            orderBLL.Update(order, "state,returncount,state");
            PinGroupBLL.SingleModel.RollbackEntrantCount(order, ref msg);
            result = Update(apply, "state,updatetime");
            return result;
        }
       

    }
}
