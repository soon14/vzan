
using BLL.MiniApp.Ent;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Conf
{
    public class SaveMoneySetUserBLL : DAL.Base.BaseMySql<SaveMoneySetUser>
    {
        #region 单例模式
        private static SaveMoneySetUserBLL _singleModel;
        private static readonly object SynObject = new object();

        private SaveMoneySetUserBLL()
        {

        }

        public static SaveMoneySetUserBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SaveMoneySetUserBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 获取用户的累计储值金额不包含赠送的
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetSaveMoneySum(int userId)
        {

            string sql = $" SELECT SUM(ChangeMoney-GiveMoney) from SaveMoneySetUserLog where userId = {userId} and Type = 0  and state=1";
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != null&&obj!=DBNull.Value)
            {
                return Convert.ToInt32(obj);
            }
            return 0;

           
        }


        public SaveMoneySetUser getModelByUserId(int UserId)
        {
            return GetModel($" UserId = {UserId} ");
        }

        public SaveMoneySetUser getModelByUserId(string appId, int UserId)
        {
            return GetModel($" AppId = '{appId}' and UserId = {UserId} ");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId">小程序appid</param>
        /// <param name="UserId">用户ID</param>
        /// <param name="price">价格</param>
        /// <param name="type">0：储值，-1：消费，1：退款</param>
        /// <param name="orderid">订单ID</param>
        /// <param name="orderno">订单号</param>
        /// <returns></returns>
        public List<string> GetCommandCarPriceSql(string appId, int UserId, int price, int type, int orderid,string orderno,string changeLog="")
        {
            var saveMoneyUser = new SaveMoneySetUserBLL().getModelByUserId(appId, UserId);
            if (saveMoneyUser == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("处理失败！没有开通储值支付"));
                return null;
            }
            List<string> sqls = new List<string>();

            int aftermoney = 0;
            if(type==0 || type==1)
            {
                aftermoney = saveMoneyUser.AccountMoney + price;
            }
            else if(type==-1)
            {
                aftermoney = saveMoneyUser.AccountMoney - price;
            }
            sqls.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
            {
                AppId = appId,
                UserId = UserId,
                MoneySetUserId = saveMoneyUser.Id,
                Type = type,
                BeforeMoney = saveMoneyUser.AccountMoney,
                AfterMoney = aftermoney,
                ChangeMoney = price,
                ChangeNote =string.IsNullOrEmpty(changeLog)?$" 参加拼团商品,订单号:{orderno} ": changeLog,
                CreateDate = DateTime.Now,
                State = 1,
                OrderId = orderid,
            }));
            sqls.Add($" update SaveMoneySetUser set AccountMoney = {aftermoney} where id =  {saveMoneyUser.Id} ; ");

            return sqls;
        }

        /// <summary>
        /// 储值支付（足浴版）
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        public bool payOrderBySaveMoneyUser(EntGoodsOrder orderModel, SaveMoneySetUser saveMoneyUser)
        {
            if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
            {
                return false;
            }
            if (saveMoneyUser.AccountMoney < orderModel.BuyPrice)
            {
                return false;
            }
            TransactionModel _tranModel = new TransactionModel();
            if (orderModel != null)
            {

                orderModel.PayDate = DateTime.Now;//支付时间
                orderModel.State = (int)MiniAppEntOrderState.待服务;

                _tranModel.Add(EntGoodsOrderBLL.SingleModel.BuildUpdateSql(orderModel, $"State,PayDate"));

                _tranModel.Add(EntGoodsOrderLogBLL.SingleModel.BuildAddSql(new EntGoodsOrderLog() { GoodsOrderId = orderModel.Id, UserId = orderModel.UserId, LogInfo = $" 订单成功支付(储值支付)：{orderModel.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));
            }
            _tranModel.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
            {
                AppId = saveMoneyUser.AppId,
                UserId = orderModel.UserId,
                MoneySetUserId = saveMoneyUser.Id,
                Type = -1,
                BeforeMoney = saveMoneyUser.AccountMoney,
                AfterMoney = saveMoneyUser.AccountMoney - orderModel.BuyPrice,
                ChangeMoney = orderModel.BuyPrice,
                ChangeNote = $" 购买商品,订单号:{orderModel.OrderNum} ",
                CreateDate = DateTime.Now,
                State = 1
            }));

            _tranModel.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney - {orderModel.BuyPrice} where id =  {saveMoneyUser.Id} ; ");
            var isSuccess = SaveMoneySetUserLogBLL.SingleModel.ExecuteTransaction(_tranModel.sqlArray);

            if (isSuccess)
            {
                AfterPaySuccesExecFun(orderModel);
            }

            return isSuccess;
        }
        /// <summary>
        /// 储值支付后
        /// </summary>
        /// <param name="foodGoodsOrder"></param>
        public void AfterPaySuccesExecFun(EntGoodsOrder orderModel)
        {

            if (orderModel == null)
            {
                return;
            }

            #region 预订成功通知商家 公众号模板消息

            //FootBathBLL _footbathBll = new FootBathBLL();

            //FootBath store = _footbathBll.GetModel(orderModel.StoreId);
            //XcxAppAccountRelation relationInfo = xapprBll.GetModelById(store.Id);
            ////Account account = accountBll.GetModel($"id='{}'")
            ////relationInfo.AccountId
            ////var xappr = xapprBll.GetModel(food.appId);
            
            //#region 发送订单支付成功通知 模板消息
            //#endregion

            //#region 发送模板消息通知商家
            //TemplateMsg_Gzh.SendTemplateMessage(WebSiteConfig.DZ_footbath_ReserveTemplateId,"", .);
            #endregion

        }

        /// <summary>
        /// 储值退款（共用）
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="aid"></param>
        /// <param name="userid"></param>
        /// <param name="type"></param>
        /// <param name="buyprice"></param>
        /// <param name="ordernum"></param>
        /// <param name="tran"></param>
        /// <param name="msg"></param>
        /// <param name="getsql">是否获取sql</param>
        public void ReturnPrice(string appid,int aid,int userid,TmpType type,int buyprice,string ordernum,ref TransactionModel tran,ref string msg,bool getsql=false)
        {
            if(string.IsNullOrEmpty(appid))
            {
                msg = "储值退款：appid不能为空";
                return;
            }
            if(userid<=0)
            {
                msg = "储值退款：用户ID无效";
                return;
            }
            if(aid<=0)
            {
                msg = "储值退款：权限ID无效";
                return;
            }
            
            SaveMoneySetUser saveMoneyUser = getModelByUserId(appid, userid);
            tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
            {
                AppId = appid,
                UserId = userid,
                AId = aid,
                MoneySetUserId = saveMoneyUser.Id,
                Type = 1,
                BeforeMoney = saveMoneyUser.AccountMoney,
                AfterMoney = saveMoneyUser.AccountMoney + buyprice,
                ChangeMoney = buyprice,
                ChangeNote = $"{type.ToString()}购买商品退款,订单号:{ordernum} ",
                CreateDate = DateTime.Now,
                State = 1
            }));
            
            tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney + {buyprice} where id =  {saveMoneyUser.Id} ; ");
           if(!getsql)
            {
                if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
                {
                    msg = "储值退款：操作失败";
                }
            }
            
        }
        /// <summary>
        /// 预约付费储值支付
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        public bool paySubscribeFromOrderBySaveMoneyUser(EntGoodsOrder dbOrder, SaveMoneySetUser saveMoneyUser)
        {
            if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
            {
                return false;
            }
            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
            {
                return false;
            }

          

            TransactionModel tran = new TransactionModel();
            tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
            {
                AppId = saveMoneyUser.AppId,
                UserId = dbOrder.UserId,
                MoneySetUserId = saveMoneyUser.Id,
                Type = -1,
                BeforeMoney = saveMoneyUser.AccountMoney,
                AfterMoney = saveMoneyUser.AccountMoney - dbOrder.BuyPrice,
                ChangeMoney = dbOrder.BuyPrice,
                ChangeNote = $" 预约付费,订单号:{dbOrder.OrderNum} ",
                CreateDate = DateTime.Now,
                State = 1
            }));
            dbOrder.State = (int)MiniAppEntOrderState.交易成功;

            dbOrder.PayDate = DateTime.Now;
            tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney - {dbOrder.BuyPrice} where id =  {saveMoneyUser.Id} ; ");
            tran.Add($" update Entgoodsorder set state = {dbOrder.State},PayDate = '{dbOrder.PayDateStr}',OrderNum = {dbOrder.OrderNum} where Id = {dbOrder.Id} and state = {(int)MiniAppEntOrderState.待付款 } ; ");


            //记录订单支付日志
            tran.Add(EntGoodsOrderLogBLL.SingleModel.BuildAddSql(new EntGoodsOrderLog() { GoodsOrderId = dbOrder.Id, UserId = dbOrder.UserId, LogInfo = $" 专业版预约付费订单使用储值金额成功支付：{dbOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));
            return SaveMoneySetUserLogBLL.SingleModel.ExecuteTransaction(tran.sqlArray);
        }
    }
}
