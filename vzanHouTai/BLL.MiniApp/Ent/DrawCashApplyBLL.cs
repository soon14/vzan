using BLL.MiniApp.Pin;
using BLL.MiniApp.Plat;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Pin;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class DrawCashApplyBLL : BaseMySql<DrawCashApply>
    {

        #region 单例模式
        private static DrawCashApplyBLL _singleModel;
        private static readonly object SynObject = new object();

        private DrawCashApplyBLL()
        {

        }

        public static DrawCashApplyBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DrawCashApplyBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion


        public List<DrawCashApply> GetListByAid(int aid, int pageIndex, int pageSize, out int recordCount)
        {
            recordCount = 0;
            List<DrawCashApply> list = new List<DrawCashApply>();
            if (aid <= 0)
            {
                return list;
            }
            string sqlwhere = $" aid={aid} and state>-2 and applytype<>{(int)DrawCashApplyType.拼享惠用户返现}";
            list = GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
            recordCount = GetCount(sqlwhere);
            return list;
        }
        public List<DrawCashApply> GetListByIds(int aid, string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<DrawCashApply>();
            string sqlwhere = $" aid={aid} and id in ({ids})";
            return base.GetList(sqlwhere);
        }

        /// <summary>
        /// 获取分销提现申请记录列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="state"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="telephone"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<DrawCashApply> GetDistributionDrawCashApplys(out int TotalCount, int appId, int state = -2, string startTime = "", string endTime = "", string telephone = "", int pageSize = 10, int pageIndex = 1)
        {
            #region 查找出当前小程序所属分销员的userId 然后再去查询提现申请
            string salesManWhere = $"appId={appId}";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(telephone))
            {
                salesManWhere += $" and TelePhone like @telephone";
                mysqlParams.Add(new MySqlParameter("@telephone", "%" + telephone + "%"));
            }

            List<SalesMan> listSalesMan = SalesManBLL.SingleModel.GetListByParam(salesManWhere, mysqlParams.ToArray());

            List<int> listSalesManUserId = new List<int>();
            foreach (SalesMan item in listSalesMan)
            {
                listSalesManUserId.Add(item.UserId);
            }
            #endregion
            List<DrawCashApply> listDrawCashApply = new List<DrawCashApply>();
            string userIds = string.Join(",", listSalesManUserId);
            TotalCount = 0;
            if (listSalesManUserId != null && listSalesManUserId.Count > 0)
            {


                string strWhere = $"aid={appId} and applyType=0 and userId in({userIds}) ";
                if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
                {
                    strWhere += $" and AddTime>='{startTime}' and AddTime<='{endTime}'";
                }

                if (state != -2)
                {
                    strWhere += $" and state={state}";
                }



                string salesManRecordIds = string.Empty;
                listDrawCashApply = base.GetList(strWhere, pageSize, pageIndex, "*", "addTime desc");
                listDrawCashApply.ForEach(x =>
                {
                    C_UserInfo c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(x.userId);
                    if (c_UserInfo != null)
                    {
                        x.nickName = c_UserInfo.NickName;
                    }

                    SalesMan salesMan = listSalesMan.Find(y => y.UserId == x.userId);
                    if (salesMan != null)
                    {
                        x.phone = salesMan.TelePhone;
                    }

                });



                TotalCount = base.GetList(strWhere) == null ? 0 : base.GetList(strWhere).Count;
            }
            return listDrawCashApply;

        }

        public DrawCashApply GetModelByAid_Id(int aid, int id)
        {
            string sqlwhere = $"aid={aid} and id={id} and state>-2";
            return GetModel(sqlwhere);
        }

        /// <summary>
        /// 获取累积提现金额
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetSumCash(int aId, int userId)
        {
            string sql = $"select sum(cashMoney) from DrawCashApply where aid={aId} and userid={userId} and drawstate={(int)DrawCashState.提现成功}";
            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql);
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        #region 拼享惠提现
        /// <summary>
        /// 拼享惠商家提现
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="account"></param>
        /// <param name="accountName"></param>
        /// <param name="cash"></param>
        /// <param name="drawCashWay"></param>
        /// <returns>0失败 1成功</returns>
        public int PxhAddApply(PinStore store, int userId, string account, string accountName, int cash, int drawCashWay, int serviceFee, int applyType, int xwkjUserId = 0)
        {
            string filed = string.Empty;
            string useCash = string.Empty;
            if (applyType == (int)DrawCashApplyType.拼享惠平台交易)
            {
                filed = "cash";
                store.cash -= cash;
                useCash = store.cashStr;
            }
            else
            {
                filed = "qrcodecash";
                store.qrcodeCash -= cash;
                useCash = store.qrcodeCashStr;
            }
            TransactionModel tran = new TransactionModel();
            MySqlParameter[] pone = null;
            tran.Add(PinStoreBLL.SingleModel.BuildUpdateSql(store, filed, out pone), pone);
            int code = 0;
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(store.aId);
            if (xcx == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"拼享惠申请提现失败：小程序信息错误 aid:{store.aId}"));
                return code;
            }
            string partner_trade_no = WxPayApi.GenerateOutTradeNo();
            DrawCashApply apply = new DrawCashApply()
            {
                Aid = store.aId,
                appId = xwkjUserId == 0 ? xcx.AppId : WebSiteConfig.GongZhongAppId,
                userId = xwkjUserId == 0 ? userId : xwkjUserId,
                partner_trade_no = partner_trade_no,
                applyMoney = cash,
                serviceMoney = serviceFee,
                cashMoney = cash - serviceFee,
                useCash = useCash,
                AddTime = DateTime.Now,
                remark = "拼享惠商户申请提现",
                hostIP = Utility.WebHelper.GetIP(),
                applyType = applyType,
                account = account,
                accountName = accountName,
                drawCashWay = drawCashWay,
                OrderId=userId,
            };
            tran.Add(BuildAddSql(apply));
            code = base.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) ? 1 : 0;
            return code;
        }
        /// <summary>
        /// 更新拼享惠商家提现结果
        /// </summary>
        /// <param name="code"></param>
        /// <param name="item"></param>
        /// <param name="resultmsg"></param>
        /// <returns></returns>
        public string UpdatePinStoreDrawCashResult(int code, DrawCashApply apply, string result)
        {
            //if (apply.drawState != (int)DrawCashState.提现中)
            //{
            //    return $"无效状态 applyId:{apply.Id}";
            //}
            if (code == 1)
            {
                apply.drawState = 2;
                apply.DrawTime = apply.UpdateTime = DateTime.Now;
                apply.remark += $" 本次提现成功{DateTime.Now}";
                
                if (apply.appId == WebSiteConfig.GongZhongAppId)
                {
                    apply.userId = apply.OrderId;
                }
                apply.pinStore = PinStoreBLL.SingleModel.GetModelByAid_UserId(apply.Aid, apply.userId);
                if (apply.pinStore == null)
                {
                    return $"拼享惠商家提现成功，找不到商家信息，累积收益更新失败 aid:{apply.Aid},userid:{apply.userId}";
                }
                if (apply.pinStore.agentId > 0 && apply.applyType == (int)DrawCashApplyType.拼享惠平台交易)
                {
                    
                    //代理提成
                    return PinAgentBLL.SingleModel.AddStoreIncome(apply) ? "" : "提现成功：执行更新代理拼享惠商家提现结果失败";
                }
                return base.Update(apply, "drawstate,drawtime,updatetime,remark") ? "" : "提现成功：执行更新拼享惠商家提现结果失败";

            }
            else
            {
                apply.drawState = -1;
                apply.DrawTime = apply.UpdateTime = DateTime.Now;
                apply.remark = $" 本次提现失败{DateTime.Now},原因{result}";
                return ReturnPinStoreCash(apply) ? "" : "提现失败：执行更新拼享惠商家提现结果失败";
            }
        }
        /// <summary>
        /// 更新拼享惠代理提现结果
        /// </summary>
        /// <param name="code"></param>
        /// <param name="apply"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public string UpdatePinAgentDrawCashResult(int code, DrawCashApply apply, string result)
        {
            //if (apply.drawState != (int)DrawCashState.提现中)
            //{
            //    return $"无效状态 applyId:{apply.Id}";
            //}
            if (code == 1)
            {
                apply.drawState = 2;
                apply.DrawTime = DateTime.Now;
                apply.remark += $" ;本次提现成功{DateTime.Now}";
                return Update(apply, "drawstate,drawtime,remark") ? "" : "执行更新拼享惠代理提现结果失败";
            }
            else
            {
                apply.drawState = -1;
                apply.DrawTime = apply.UpdateTime = DateTime.Now;
                apply.remark = $" ;本次提现失败{DateTime.Now},原因{result}";
                return ReturnPinAgentCash(apply) ? "" : "执行更新拼享惠代理提现结果失败";
            }
        }
        /// <summary>
        /// 拼享惠代理提现：返回提现金额
        /// </summary>
        /// <param name="apply"></param>
        /// <returns></returns>
        public bool ReturnPinAgentCash(DrawCashApply apply, TransactionModel tranModel = null)
        {
            bool isTran = tranModel != null;
            if (!isTran) tranModel = new TransactionModel();
            MySqlParameter[] pone = null;
            tranModel.Add(base.BuildUpdateSql(apply, "drawState,DrawTime,remark,state,UpdateTime", out pone), pone);
            
            if(apply.appId == WebSiteConfig.GongZhongAppId)
            {
                apply.userId = apply.OrderId;
            }
            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(apply.userId);
            if (agent == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($" 拼享惠代理提现：返回提现金额失败，找不到代理userId:{ apply.userId}"));
                return false;
            }
            int money = apply.cashMoney + apply.serviceMoney;
            agent.cash += money * 1000;
            pone = null;
            tranModel.Add(PinAgentBLL.SingleModel.BuildUpdateSql(agent, "cash", out pone), pone);
            if (isTran) return true;
            return base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
        }

        /// <summary>
        /// 拼享惠商家提现：返回提现金额
        /// </summary>
        /// <param name="apply"></param>
        /// <returns></returns>
        public bool ReturnPinStoreCash(DrawCashApply apply, TransactionModel tranModel = null)
        {
            bool isTran = tranModel != null;
            if (!isTran) tranModel = new TransactionModel();
            //MySqlParameter[] pone = null;
            tranModel.Add(base.BuildUpdateSql(apply, "drawState,DrawTime,remark,state,UpdateTime"));
            
            //判断是否是拼享惠公众号提现
            if(apply.appId == WebSiteConfig.GongZhongAppId)
            {
                apply.userId = apply.OrderId;
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(apply.Aid, apply.userId);
            if (store == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($" 拼享惠商家提现：返回提现金额失败，找不到商家 aid:{apply.Aid} userId:{ apply.userId}"));
                return false;
            }
            int money = apply.cashMoney + apply.serviceMoney;
            string fileds = string.Empty;
            switch (apply.applyType)
            {
                case (int)DrawCashApplyType.拼享惠平台交易:
                    store.cash += money;
                    fileds = "cash";
                    break;
                case (int)DrawCashApplyType.拼享惠扫码收益:
                    store.qrcodeCash += money;
                    fileds = "qrcodeCash";
                    break;
                default: return false;
            }
            //pone = null;
            tranModel.Add(PinStoreBLL.SingleModel.BuildUpdateSql(store, fileds));
            if (isTran) return true;
            return base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
        }

        /// <summary>
        /// 拼享惠代理提现
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="userId"></param>
        /// <param name="account"></param>
        /// <param name="accountName"></param>
        /// <param name="cash"></param>
        /// <param name="drawCashWay"></param>
        /// <param name="serviceFee"></param>
        /// <returns></returns>
        public int PxhAddApply(PinAgent agent, int userId, string account, string accountName, int cash, int drawCashWay, int serviceFee, int xwkjUserId = 0)
        {
            agent.cash -= cash * 1000;
            TransactionModel tran = new TransactionModel();
            MySqlParameter[] pone = null;
            tran.Add(PinAgentBLL.SingleModel.BuildUpdateSql(agent, "cash", out pone), pone);
            int code = 0;
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(agent.aId);
            if (xcx == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"拼享惠申请提现失败：小程序信息错误 aid:{agent.aId}"));
                return code;
            }
            string partner_trade_no = WxPayApi.GenerateOutTradeNo();
            DrawCashApply apply = new DrawCashApply()
            {
                Aid = agent.aId,
                appId = xwkjUserId == 0 ? xcx.AppId : WebSiteConfig.GongZhongAppId,
                userId = xwkjUserId == 0 ? userId : xwkjUserId,
                partner_trade_no = partner_trade_no,
                applyMoney = cash,
                serviceMoney = serviceFee,
                cashMoney = cash - serviceFee,
                useCash = agent.cashStr,
                AddTime = DateTime.Now,
                remark = "拼享惠代理商申请提现",
                hostIP = Utility.WebHelper.GetIP(),
                applyType = (int)DrawCashApplyType.拼享惠代理收益,
                account = account,
                accountName = accountName,
                drawCashWay = drawCashWay,
                OrderId = userId,
            };
            tran.Add(BuildAddSql(apply));
            code = base.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) ? 1 : 0;
            return code;
        }

        /// <summary>
        /// 用户拼团成功返现
        /// </summary>
        /// <param name="id">提现账号id(PlatUserCash)</param>
        /// <param name="drawcashmoney">提现金额（分）</param>
        /// <returns></returns>
        public string PxhUserApplyDrawCash(PinGoodsOrder order, int userId, string appid)
        {
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            TransactionModel tran = new TransactionModel();

            if (userId <= 0)
            {
                return "用户ID不能为0";
            }
            if (order.returnMoney <= 0)
            {
                return "返现金额不能为0";
            }
            if (string.IsNullOrEmpty(appid))
            {
                return "appid不能为空";
            }

            int serviceFee = 0;//服务费
            string partner_trade_no = WxPayApi.GenerateOutTradeNo();
            DrawCashApply apply = new DrawCashApply()
            {
                Aid = order.aid,
                appId = appid,
                partner_trade_no = partner_trade_no,
                userId = userId,
                applyMoney = order.returnMoney,
                serviceMoney = serviceFee,
                cashMoney = order.returnMoney - serviceFee,
                useCash = "0",
                BeforeApplyMoney = 0,
                AddTime = DateTime.Now,
                remark = "拼享惠拼团成功用户申请返现",
                hostIP = WebHelper.GetIP(),
                state = 0,
                drawState = (int)DrawCashState.未开始提现,
                applyType = (int)DrawCashApplyType.拼享惠用户返现,
                drawCashWay = (int)DrawCashWay.微信提现,
                OrderId = order.id,
            };
            tran.Add(base.BuildAddSql(apply));

            order.isReturnMoney = 1;
            tran.Add(pinGoodsOrderBLL.BuildUpdateSql(order, "isReturnMoney"));

            if (!base.ExecuteTransactionDataCorect(tran.sqlArray))
            {
                return "申请提现失败";
            }
            return "";
        }

        /// <summary>
        /// 更新用户拼团成功提现结果
        /// </summary>
        /// <param name="code">0提现失败 1提现成功</param>
        /// <param name="drawCashApplyId"></param>
        /// <param name="result">附加信息</param>
        /// <returns></returns>
        public string UpdatePxhUserDrawCashResult(int code, DrawCashApply drawCashApply, string result, int state = 0)
        {
            TransactionModel tranModel = new TransactionModel();
            C_UserInfo usercash = C_UserInfoBLL.SingleModel.GetModel(drawCashApply.userId);
            if (usercash == null)
            {
                return "没有找到用户提现账号";
            }
            //if (drawCashApply.drawState != (int)DrawCashState.提现中)
            //{
            //    return "无效状态";
            //}
            if (code == 1)
            {
                drawCashApply.drawState = 2;
                drawCashApply.DrawTime = DateTime.Now;
                drawCashApply.remark += $" ;本次提现成功{DateTime.Now}";
            }
            else
            {
                PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
                PinGoodsOrder order = pinGoodsOrderBLL.GetModel(drawCashApply.OrderId);
                if (order == null)
                {
                    return "没有找到拼享惠订单";
                }
                order.isReturnMoney = 0;
                tranModel.Add(pinGoodsOrderBLL.BuildUpdateSql(order, "isReturnMoney"));
                //表示提现失败
                //tranModel.Add($"update PinGoodsOrder set isReturnMoney=0 where id={order.id}");

                drawCashApply.drawState = -1;
                if (state == (int)ApplyState.审核不通过)
                {
                    drawCashApply.state = state;
                }

                drawCashApply.DrawTime = DateTime.Now;
                drawCashApply.remark = $" ;本次提现失败{DateTime.Now},原因{result}";
            }

            tranModel.Add(base.BuildUpdateSql(drawCashApply, "state,drawState,DrawTime,remark"));

            bool success = base.ExecuteTransactionDataCorect(tranModel.sqlArray);

            return success ? "" : "执行更新拼享惠用户提现结果失败";
        }

        /// <summary>
        /// 获取拼团返现记录
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="state"></param>
        /// <param name="orderNum"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <param name="excel"></param>
        /// <returns></returns>
        public List<DrawCashApply> GetListPxhUserDrawCash(int aid, int state, string orderNum, string startTime, string endTime, int pageIndex, int pageSize, ref int count, bool excel = false)
        {
            List<DrawCashApply> list = new List<DrawCashApply>();
            List<MySqlParameter> parms = new List<MySqlParameter>();
            string sql = $"select {"{0}"} from drawcashapply d left join pingoodsorder o on d.orderid=o.id ";
            string sqlList = string.Format(sql, "d.*,o.outtradeno,o.addtime orderaddtime");
            string sqlCount = string.Format(sql, "count(*)");
            string sqlWhere = $" where d.aid={aid} and d.applytype = {(int)DrawCashApplyType.拼享惠用户返现} ";
            if (state != -999)
            {
                sqlWhere += $" and d.drawState={state} ";
            }
            if (!string.IsNullOrEmpty(startTime))
            {
                sqlWhere += $" and d.AddTime>='{startTime}'";
            }
            if (!string.IsNullOrEmpty(endTime))
            {
                sqlWhere += $" and d.AddTime<='{endTime}'";
            }
            if(!string.IsNullOrEmpty(orderNum))
            {
                parms.Add(new MySqlParameter("@outtradeno",orderNum));
                sqlWhere += $" and outtradeno=@outtradeno";
            }

            string sqlLimit = $" order by d.addtime desc limit {(pageIndex - 1) * pageSize},{pageSize} ";

            count = base.GetCountBySql(sqlCount + sqlWhere, parms.ToArray());
            if (count <= 0)
                return list;

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sqlList + sqlWhere + (excel ? "" : sqlLimit), parms.ToArray()))
            {
                while (dr.Read())
                {
                    DrawCashApply model = GetModel(dr);
                    if (dr["outtradeno"] != DBNull.Value)
                    {
                        model.nickName = dr["outtradeno"].ToString();
                    }
                    if (dr["orderaddtime"] != DBNull.Value)
                    {
                        model.accountName = dr["orderaddtime"].ToString();
                    }

                    list.Add(model);
                }
            }

            return list;
        }
        #endregion

        public List<DrawCashApply> GetListByAid_UserId(int aid, int userId, int pageIndex, int pageSize, out int recordCount)
        {
            List<DrawCashApply> list = new List<DrawCashApply>();
            recordCount = 0;
            if (aid <= 0 || userId <= 0)
            {
                return list;
            }

            string sqlwhere = $"aid={aid} and userid={userId}";
            list = GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
            recordCount = GetCount(sqlwhere);
            return list;
        }

        #region 平台提现处理
        /// <summary>
        /// 获取平台提现申请记录列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="state"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="telephone"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<DrawCashApply> GetPlatDrawCashApplys(ref int count, int aid, int state, int drawway, int drawstate, string startTime = "", string endTime = "", string storename = "", int pageSize = 10, int pageIndex = 1, int userid = 0)
        {
            List<DrawCashApply> list = new List<DrawCashApply>();
            string sqlwhere = $"aid={aid} and applyType={(int)DrawCashApplyType.平台店铺提现}";
            if (!string.IsNullOrEmpty(startTime))
            {
                sqlwhere += $" and AddTime>='{startTime}'";
            }
            if (!string.IsNullOrEmpty(endTime))
            {
                sqlwhere += $" and AddTime<='{endTime}'";
            }
            //店铺或电话搜索
            if (!string.IsNullOrEmpty(storename))
            {
                List<PlatStore> storelist = PlatStoreBLL.SingleModel.GetListByNameOrPhone(storename, aid);
                if (storelist == null || storelist.Count <= 0)
                    return list;

                string userids = string.Join(",", storelist.Select(s => s.UserId).Distinct());
                sqlwhere += $" and userid in ({userids})";
            }
            if (userid > 0)
            {
                sqlwhere += $" and userid ={userid}";
            }

            if (state != -999)
            {
                sqlwhere += $" and state={state}";
            }
            if (drawway != -999)
            {
                sqlwhere += $" and drawCashWay={drawway}";
            }
            if (drawstate != -999)
            {
                sqlwhere += $" and drawState={drawstate}";
            }
            string salesManRecordIds = string.Empty;
            list = base.GetList(sqlwhere, pageSize, pageIndex, "*", "addTime desc");
            count = base.GetCount(sqlwhere);
            if (count > 0)
            {
                string userids = string.Join(",", list.Select(s => s.userId).Distinct());
                //List<C_UserInfo> userinfolist = C_UserInfoBLL.SingleModel.GetListByIds(userids);
                List<PlatMyCard> mycardlist = PlatMyCardBLL.SingleModel.GetListByUserIds(userids);
                if (mycardlist == null || mycardlist.Count <= 0)
                {
                    return new List<DrawCashApply>();
                }
                string mycardids = string.Join(",", mycardlist.Select(s => s.Id));
                List<PlatStore> storelist = PlatStoreBLL.SingleModel.GetListByCardId(mycardids);
                list.ForEach(x =>
                {
                    PlatMyCard card = mycardlist.FirstOrDefault(f => f.UserId == x.userId);
                    if (card != null)
                    {
                        PlatStore store = storelist.FirstOrDefault(f => f.MyCardId == card.Id);
                        if (store != null)
                        {
                            x.nickName = store.Name;
                            x.phone = card.Phone;
                        }
                    }
                });
            }

            return list;
        }

        /// <summary>
        /// 平台审核提现申请记录 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="state"></param>
        /// <param name="appId"></param>
        /// <param name="listDrawCashApply"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public string UpdatePlatDrawCashApply(string ids, int state, int aid, string accountId)
        {
            
            //1.更新提现申请.如果为标记为审核不通过则需要将提现金额归还并且标记为提现失败,如果标记为审核通过则不需要归还,然后将其标记为提现中2.记录操作日志  两个都成功才算成功
            TransactionModel tranModel = new TransactionModel();
            DrawCashApplyLog drawCashApplyLog = new DrawCashApplyLog();
            List<DrawCashApply> list = GetListByIds(aid, ids);
            if (list == null || list.Count <= 0)
            {
                return "无效申请记录";
            }
            if (string.IsNullOrEmpty(accountId))
            {
                return "无效用户";
            }
            if (state == -1)
            {
                //表示审核不通过  要标记为提现失败  并且需要把提现金额归还  最后记录操作日志
                //标记为审核不通过以及提现失败
                tranModel.Add($"update drawcashapply set state=-1,drawState=-1,updatetime='{DateTime.Now}' where Id in({ids}) and aid={aid}");
                //之前扣的佣金归还
                foreach (DrawCashApply item in list)
                {
                    PlatUserCash usercase = PlatUserCashBLL.SingleModel.GetModelByUserId(aid, item.userId);
                    usercase.UseCash += item.applyMoney;
                    tranModel.Add(PlatUserCashBLL.SingleModel.BuildUpdateSql(usercase, "UseCash"));
                }

                drawCashApplyLog.remark = $"将提现申请记录id为{ids}设置为审核不通过";
            }
            else
            {
                //表示审核通过 
                drawCashApplyLog.remark = $"将提现申请记录id为{ids}设置为审核通过";
                //标记为审核通过以及提现中,只有这样提现服务才会获取去提现
                tranModel.Add($"update drawcashapply set state=1,drawState=1,updatetime='{DateTime.Now}' where Id in({ids}) and aid={aid}");
            }

            drawCashApplyLog.accountid = accountId;
            drawCashApplyLog.appId = aid;
            drawCashApplyLog.AddTime = DateTime.Now;
            drawCashApplyLog.hostIP = Utility.WebHelper.GetIP();
            //记录操作日志
            tranModel.Add(DrawCashApplyLogBLL.SingleModel.BuildAddSql(drawCashApplyLog));

            bool success = DrawCashApplyLogBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray);

            return success ? "" : "操作失败";
        }

        /// <summary>
        /// 更新提现结果
        /// </summary>
        /// <param name="code">0提现失败 1提现成功</param>
        /// <param name="drawCashApplyId"></param>
        /// <param name="result">附加信息</param>
        /// <returns></returns>
        public string UpdatePlayDrawCashResult(int code, DrawCashApply drawCashApply, string result)
        {
            TransactionModel tranModel = new TransactionModel();
            
            PlatUserCash usercash = PlatUserCashBLL.SingleModel.GetModelByUserId(drawCashApply.Aid, drawCashApply.userId);
            if (usercash == null)
            {
                return "没有找到用户提现账号";
            }
            //if (drawCashApply.drawState != (int)DrawCashState.提现中)
            //{
            //    return "无效状态";
            //}
            if (code == 1)
            {
                //表示提现成功  1.将本次提现成功金额加入累计收益 2.更新提现记录为提现成功 3.追加提现记录备注 提现成功
                usercash.UseCashTotal += drawCashApply.cashMoney + drawCashApply.serviceMoney;
                usercash.ServerCash += drawCashApply.serviceMoney;
                drawCashApply.drawState = 2;
                drawCashApply.DrawTime = DateTime.Now;
                drawCashApply.remark += $" ;本次提现成功{DateTime.Now}";
                tranModel.Add(PlatUserCashBLL.SingleModel.BuildUpdateSql(usercash, "UseCashTotal,ServerCash"));
            }
            else
            {
                //表示提现失败 将提现金额返回给分销员
                usercash.UseCash += drawCashApply.applyMoney;
                drawCashApply.drawState = -1;
                drawCashApply.DrawTime = DateTime.Now;
                drawCashApply.remark = $" ;本次提现失败{DateTime.Now},原因{result}";
                tranModel.Add(PlatUserCashBLL.SingleModel.BuildUpdateSql(usercash, "UseCash"));
            }

            tranModel.Add(base.BuildUpdateSql(drawCashApply, "drawState,DrawTime,remark"));

            bool success = base.ExecuteTransactionDataCorect(tranModel.sqlArray);

            return success ? "" : "执行更新平台店铺提现结果失败";
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id">提现账号id(PlatUserCash)</param>
        /// <param name="drawcashmoney">提现金额（分）</param>
        /// <returns></returns>
        public string ApplyDrawCash(int aid = 0, int id = 0, int drawcashmoney = 0)
        {
            
            
            TransactionModel tran = new TransactionModel();
            if (id <= 0)
            {
                return "id不能为0";
            }
            if (drawcashmoney <= 0)
            {
                return "提现金额不能为0";
            }

            PlatUserCash usercash = PlatUserCashBLL.SingleModel.GetModel(id);
            if (usercash == null)
            {
                return "提现账号";
            }
            if (usercash.DrawCashWay == (int)DrawCashWay.银行卡人工提现 && string.IsNullOrEmpty(usercash.AccountBank))
            {
                return "请完善提现账号";
            }
            if(usercash.UseCash<drawcashmoney)
            {
                return "可提现金额不足";
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(usercash.AId);
            if (xcxrelation == null)
            {
                return "提现平台已过期";
            }
            if (string.IsNullOrEmpty(xcxrelation.AppId))
            {
                return "提现平台还未上线";
            }
            PlatDrawConfig drawconfig = PlatDrawConfigBLL.SingleModel.GetModelByAId(usercash.AId);
            if (drawconfig == null)
            {
                return "平台还未开启提现";
            }
            if (string.IsNullOrEmpty(drawconfig.DrawCashWay))
            {
                return "平台未开通提现方式";
            }
            //判断是否支持该提现方式
            DrawCashWayItem tempway = drawconfig.DrawCashWayList.FirstOrDefault(f => f.IsOpen == 1 && f.DrawCashWay == usercash.DrawCashWay);
            if (tempway == null)
            {
                return "平台不支持该提现方式";
            }
            if (drawconfig.MinMoney > drawcashmoney)
            {
                return $"提现金额不能低于{drawconfig.MinMoneyStr}元";
            }
            PlatMyCard mycard = PlatMyCardBLL.SingleModel.GetModelByUserId(usercash.UserId, usercash.AId);
            if (mycard == null)
            {
                return "无效提现账号";
            }

            int serviceFee = Convert.ToInt32((drawconfig.Fee * 0.01 * 0.01) * drawcashmoney);
            string partner_trade_no = WxPayApi.GenerateOutTradeNo();
            DrawCashApply apply = new DrawCashApply()
            {
                Aid = usercash.AId,
                appId = xcxrelation.AppId,
                partner_trade_no = partner_trade_no,
                userId = usercash.UserId,
                applyMoney = drawcashmoney,
                serviceMoney = serviceFee,
                cashMoney = drawcashmoney - serviceFee,
                useCash = (usercash.UseCash - drawcashmoney).ToString(),
                BeforeApplyMoney = usercash.UseCash,
                AddTime = DateTime.Now,
                remark = "平台字模板店铺申请提现",
                hostIP = WebHelper.GetIP(),
                state = 0,
                drawState = (int)DrawCashState.未开始提现,
                applyType = (int)DrawCashApplyType.平台店铺提现,
                account = usercash.DrawCashWay == (int)DrawCashWay.微信提现 ? mycard.Phone : usercash.AccountBank,
                accountName = usercash.DrawCashWay == (int)DrawCashWay.微信提现 ? mycard.Name : usercash.Name,
                drawCashWay = usercash.DrawCashWay,
            };
            tran.Add(base.BuildAddSql(apply));

            usercash.UseCash = usercash.UseCash - drawcashmoney;
            usercash.UpdateTime = DateTime.Now;
            tran.Add(PlatUserCashBLL.SingleModel.BuildUpdateSql(usercash, "UseCash,UpdateTime"));

            if (!base.ExecuteTransactionDataCorect(tran.sqlArray))
            {
                return "申请提现失败";
            }
            return "";
        }
        /// <summary>
        /// 加提现金额
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="aid"></param>
        /// <param name="money"></param>
        public string AddDrawCash(long userid, int aid, int money, ref TransactionModel tran, bool getsql = false)
        {
            
            PlatUserCash usercash = PlatUserCashBLL.SingleModel.GetModelByUserId(aid, userid);
            if (usercash == null)
            {
                return $"店主账号无效【{userid}】";
            }
            usercash.UseCash += money;
            usercash.UpdateTime = DateTime.Now;
            if (getsql)
            {
                tran.Add(PlatUserCashBLL.SingleModel.BuildUpdateSql(usercash, "UseCash,UpdateTime"));
            }
            else
            {
                if (!PlatUserCashBLL.SingleModel.Update(usercash, "UseCash,UpdateTime"))
                {
                    return $"店主提现账号增加提现金额失败【{userid}】";
                }
            }

            return "";
        }
        #endregion

        #region 专业版分销提现


        /// <summary>
        /// 专业版小程序端接口调用申请提现
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="drawCashMoney"></param>
        /// <param name="partner_trade_no"></param>
        /// <returns></returns>
        public int AddDistributionDrawCashApply(int appId, int userId, int drawCashMoney, string partner_trade_no)
        {
            SalesMan salesMan = SalesManBLL.SingleModel.GetModel($"appId={appId} and userId={userId}");
            if (salesMan == null)
                return -1;//表示申请提现的分销员不存在
            if (salesMan.useCash < drawCashMoney)
                return -2;//表示本次申请的提现金额小于账号可提现金额
                          //后续加入提现黑名单

            //int countNowDay = 0;//当天提现申请记录
            // string sql = $"SELECT COUNT(*)  FROM DrawCashApply WHERE  TO_DAYS(AddTime)=TO_DAYS(NOW()) and userId={salesMan.UserId}";
            //object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            //if (obj != null)
            //{
            //    countNowDay = Convert.ToInt32(obj);
            //}

            //if (countNowDay > 3)
            //    return -3;//当天提现次数超标




            DrawCashApply drawCashApply = new DrawCashApply();

            TransactionModel tranModel = new TransactionModel();

            drawCashApply.partner_trade_no = partner_trade_no;
            drawCashApply.userId = userId;
            drawCashApply.AddTime = DateTime.Now;
            drawCashApply.Aid = appId;
            drawCashApply.applyType = 0;
            drawCashApply.cashMoney = drawCashMoney;
            drawCashApply.hostIP = Utility.WebHelper.GetIP();
            drawCashApply.UpdateTime = DateTime.Now;

            //提现金额需要进行审核
            drawCashApply.state = 0;
            drawCashApply.drawState = 0;




            string salesManRecordIds = string.Join(",", SalesManRecordOrderBLL.SingleModel.GetSalesManRecordIds(salesMan.Id, appId));
            OrderSum orderSumPeople = SalesManRecordOrderBLL.SingleModel.GetOrderSum(appId, salesManRecordIds, 0);
            OrderSum orderSumAuto = SalesManRecordOrderBLL.SingleModel.GetOrderSum(appId, salesManRecordIds, 1);

            drawCashApply.orderCount = orderSumPeople.payOrderCount + orderSumAuto.payOrderCount;
            drawCashApply.orderTotalCpsMoney = (Convert.ToDouble(orderSumPeople.payOrderTotalCpsMoney) + Convert.ToDouble(orderSumAuto.payOrderTotalCpsMoney)).ToString();
            drawCashApply.orderTotalMoney = (Convert.ToDouble(orderSumPeople.payOrderTotalPrice) + Convert.ToDouble(orderSumAuto.payOrderTotalPrice)).ToString();
            int afterUseCash = salesMan.useCash - drawCashMoney;
            drawCashApply.useCash = (afterUseCash * 0.01).ToString("0.00");


            salesMan.useCash = afterUseCash;
            salesMan.UpdateTime = DateTime.Now;

            tranModel.Add(base.BuildAddSql(drawCashApply));

            tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(salesMan));

            if (tranModel.sqlArray.Length > 0 && SalesManBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
            {
                return 1;//表示加入申请提现记录成功并且更新了分销员的提现金额
            }
            else
            {
                return 0;//表示执行事务失败
            }

        }

        /// <summary>
        /// 专业版分销后台审核提现申请记录 操作 为true表示操作成功
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="state"></param>
        /// <param name="appId"></param>
        /// <param name="listDrawCashApply"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public bool UpdateDrawCashApply(string ids, int state, int appId, List<DrawCashApply> listDrawCashApply, string accountId)
        {
            //1.更新提现申请.如果为标记为审核不通过则需要将提现金额归还并且标记为提现失败,如果标记为审核通过则不需要归还,然后将其标记为提现中2.记录操作日志  两个都成功才算成功

            TransactionModel tranModel = new TransactionModel();
            DrawCashApplyLog drawCashApplyLog = new DrawCashApplyLog();
            if (state == -1)
            {
                //表示审核不通过  要标记为提现失败  并且需要把提现金额归还  最后记录操作日志

                tranModel.Add($"update drawcashapply set state=-1,drawState=-1 where Id in({ids}) and aid={appId}");//标记为审核不通过以及提现失败

                //之前扣的佣金归还
                foreach (var item in listDrawCashApply)
                {
                    SalesMan salesMan = SalesManBLL.SingleModel.GetModel($"userId={item.userId} and appId={item.Aid}");
                    salesMan.useCash += item.cashMoney;
                    tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(salesMan, "useCash"));
                }

                drawCashApplyLog.remark = $"将提现申请记录id为{ids}设置为审核不通过";

            }
            else
            {
                //表示审核通过 
                drawCashApplyLog.remark = $"将提现申请记录id为{ids}设置为审核通过";
                tranModel.Add($"update drawcashapply set state=1,drawState=1 where Id in({ids}) and aid={appId}");//标记为审核通过以及提现中,只有这样提现服务才会获取去提现

            }


            drawCashApplyLog.accountid = accountId;
            drawCashApplyLog.appId = appId;
            drawCashApplyLog.AddTime = DateTime.Now;
            drawCashApplyLog.hostIP = Utility.WebHelper.GetIP();
            //记录操作日志
            tranModel.Add(DrawCashApplyLogBLL.SingleModel.BuildAddSql(drawCashApplyLog));

            return DrawCashApplyLogBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray);


        }



        /// <summary>
        /// 专业版分销提现服务根据结果更新提现申请记录
        /// </summary>
        /// <param name="code">0提现失败 1提现成功</param>
        /// <param name="drawCashApplyId"></param>
        /// <param name="result">附加信息</param>
        /// <returns></returns>
        public string UpdateDistributionDrawCashApply(int code, DrawCashApply drawCashApply, string result, C_UserInfo userinfo)
        {
            string drawCashRsult = string.Empty;
            TransactionModel tranModel = new TransactionModel();
            SalesMan saleMane = SalesManBLL.SingleModel.GetModel($"appId={drawCashApply.Aid} and userId={drawCashApply.userId}");//获得分销员
            if (code == 1)
            {
                drawCashRsult = "成功";
                //表示提现成功  1.将本次提现成功金额加入累计收益 2.更新提现记录为提现成功 3.追加提现记录备注 提现成功
                saleMane.useCashTotal += drawCashApply.cashMoney;

                drawCashApply.drawState = 2;
                drawCashApply.UpdateTime = DateTime.Now;
                drawCashApply.remark += $" ;本次提现成功{DateTime.Now},{result}";
                tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(saleMane, "useCashTotal"));


            }
            else
            {
                drawCashRsult = "失败";
                //表示提现失败 将提现金额返回给分销员
                saleMane.useCash += drawCashApply.cashMoney;
                drawCashApply.drawState = -1;
                drawCashApply.UpdateTime = DateTime.Now;
                drawCashApply.remark += $" ;本次提现失败{DateTime.Now},原因{result}";
                tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(saleMane, "useCash"));


            }
            tranModel.Add(base.BuildUpdateSql(drawCashApply));
            
            if (SalesManBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
            {
                return $"提现{drawCashRsult}更新提现申请记录成功微信提现返回结果:{result}";
            }
            else
            {
                return $"提现{drawCashRsult}更新提现申请记录失败微信提现返回结果:{result}";
            }


        }

        public List<DrawCashApply> GetListByAid_UserId_applyType(int aid, int userId, int pageIndex, int pageSize, out int recordCount, int applyType)
        {
            List<DrawCashApply> list = new List<DrawCashApply>();
            recordCount = 0;
            if (aid <= 0 || userId <= 0)
            {
                return list;
            }

            string sqlwhere = $"aid={aid} and userid={userId}";
            if (applyType == (int)DrawCashApplyType.拼享惠代理收益)
            {
                sqlwhere += $" and applyType={(int)DrawCashApplyType.拼享惠代理收益}";
            }
            else
            {
                sqlwhere += $" and applyType in ({(int)DrawCashApplyType.拼享惠扫码收益},{(int)DrawCashApplyType.拼享惠平台交易})";
            }
            list = GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
            recordCount = GetCount(sqlwhere);
            return list;
        }

        public DrawCashApply GetModelByOrderId_DrawStates(int orderId, string drawStates)
        {
            if (string.IsNullOrEmpty(drawStates)) return null;
            string sqlwhere = $" orderId={orderId} and drawState in ({drawStates})";
            return GetModel(sqlwhere);
        }


        #endregion
    }


    public class DrawCashApplyLogBLL : BaseMySql<DrawCashApplyLog>
    {
        #region 单例模式
        private static DrawCashApplyLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private DrawCashApplyLogBLL()
        {

        }

        public static DrawCashApplyLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DrawCashApplyLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }



    public class DrawcashResultBLL : BaseMySql<DrawcashResult>
    {
        #region 单例模式
        private static DrawcashResultBLL _singleModel;
        private static readonly object SynObject = new object();

        private DrawcashResultBLL()
        {

        }

        public static DrawcashResultBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DrawcashResultBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }

}
