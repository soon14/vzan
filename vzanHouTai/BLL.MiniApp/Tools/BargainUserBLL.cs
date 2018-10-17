using BLL.MiniApp.Conf;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Stores;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Tools
{
    public class BargainUserBLL : BaseMySql<BargainUser>
    {
        #region 单例模式
        private static BargainUserBLL _singleModel;
        private static readonly object SynObject = new object();

        private BargainUserBLL()
        {

        }

        public static BargainUserBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new BargainUserBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 更新砍价开始结束时间（批量）
        /// </summary>
        /// <returns></returns>
        public bool BargainUser_UpdateBacth(string startdate, string enddate, string BName, int bid)
        {
            string sql = $"update BargainUser set StartDate='{startdate}',EndDate='{enddate}',BName='{BName}' where BId = {bid}";
            int result = SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);

            return result > 0;
        }

        /// <summary>
        /// 获取参与记录条数
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public int GetJoinCount(int bid)
        {
            return GetCount("BId=" + bid);
        }

        /// <summary>
        /// 获取参与记录条数
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public int GetJoinCount(string where)
        {
            var sql = $"select Count(a.Id) from BargainUser a LEFT join Bargain b on a.BId=b.id  LEFT join C_UserInfo u on a.UserId=u.Id left join viprelation vr on u.id=vr.uid left JOIN GoodsComment c on c.orderid = a.Id where {where}";
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != null)
            {
                return Convert.ToInt32(obj);
            }
            return 0;
        }

        /// <summary>
        /// 获取指定用户的参与记录
        /// </summary>
        /// <param name="bId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public BargainUser GetModelByUser(int bId, int userId)
        {
            return GetModel($"BId={bId} and UserId={userId}");
        }

        /// <summary>
        /// 根据条件获取砍价商品的参与记录
        /// </summary>
        /// <param name="where"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strOrder"></param>
        /// <returns></returns>
        public List<BargainUser> GetJoinList(string where, int pageSize, int pageIndex, string strOrder, int export = 0, List<MySqlParameter> parameter = null)
        {
            if (parameter == null)
            {
                parameter = new List<MySqlParameter>();
            }
            string sql = $"select a.Id,a.BId,a.BName,a.CurrentPrice,a.FreightFee,a.HelpNum,a.BuyTime,a.PayType,a.CreateDate,a.State,a.StartDate,a.EndDate,a.CityMordersId,a.Address,a.CreateOrderTime,a.OrderId,b.StoreId,a.SendGoodsName,a.WayBillNo,a.Remark,a.GetWay,a.StoreName,b.ImgUrl,b.OriginalPrice,b.FloorPrice,b.RemainNum,b.IsEnd,u.NickName,vL.name as leveName from BargainUser a LEFT join Bargain b on a.BId=b.id  LEFT join C_UserInfo u on a.UserId=u.Id left join viprelation vr on u.id=vr.uid left join viplevel vL on vr.levelid=vL.id left JOIN GoodsComment c on c.orderid = a.Id  where {where} order by {strOrder} LIMIT {(export > 0 ? 0 : (pageIndex - 1) * pageSize)},{pageSize}";

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, parameter.ToArray()))
            {
                List<BargainUser> list = new List<BargainUser>();
                while (dr.Read())
                {
                    BargainUser bargainUser = GetModel(dr);
                    bargainUser.FloorPrice = (Convert.ToInt32(dr["FloorPrice"]) * 0.01).ToString("0.00");
                    bargainUser.OriginalPrice = (Convert.ToInt32(dr["OriginalPrice"]) * 0.01).ToString("0.00");
                    bargainUser.RemainNum = Convert.ToInt32(dr["RemainNum"]);
                    bargainUser.CityMordersId = Convert.ToInt32(dr["CityMordersId"]);
                    bargainUser.ShopLogoUrl = dr["ImgUrl"].ToString();
                    bargainUser.IsEnd = Convert.ToInt32(dr["IsEnd"]);
                    bargainUser.ShopName = dr["NickName"].ToString();
                    bargainUser.VipLeve = dr["leveName"].ToString();

                    if (!string.IsNullOrEmpty(bargainUser.ShopLogoUrl))
                    {
                        if (bargainUser.ShopLogoUrl.Contains("oss.vzan"))
                        {
                            bargainUser.ShopLogoUrl = bargainUser.ShopLogoUrl.Replace("oss.", "img.") + "@1e_1c_0o_0l_100sh_120h_120w_90q.src";
                        }
                        if (bargainUser.ShopLogoUrl.Contains("i.vzan") && !bargainUser.ShopLogoUrl.Contains("@!"))
                        {
                            bargainUser.ShopLogoUrl = bargainUser.ShopLogoUrl + "@!120x120";
                        }
                    }

                    list.Add(bargainUser);
                }
                return list;
            }
        }

        /// <summary>
        /// 跟进 退款状态 (退款是否成功)用于微信支付跟进
        /// </summary>
        /// <returns></returns>
        public bool updateOutOrderState()
        {
            TransactionModel tranModel = new TransactionModel();
            List<BargainUser> itemList = GetList($" State = {2} and outOrderDate <= (NOW()-interval 17 second) ", 1000, 1) ?? new List<BargainUser>();
            List<CityMorders> orderList = new List<CityMorders>();
            List<Entity.MiniApp.ReFundResult> outOrderList = new List<Entity.MiniApp.ReFundResult>();
            if (itemList.Any())
            {
                orderList = new CityMordersBLL().GetList($" Id in ({string.Join(",", itemList.Select(x => x.CityMordersId))}) ", 1000, 1) ?? new List<CityMorders>();
                if (orderList.Any())
                {
                    outOrderList = RefundResultBLL.SingleModel.GetList($" transaction_id in ('{string.Join("','", orderList.Select(x => x.trade_no))}') and retype = 1") ?? new List<ReFundResult>();
                    itemList.ForEach(x =>
                    {
                        var curOrder = orderList.Where(y => y.Id == x.CityMordersId).FirstOrDefault();
                        if (curOrder != null)
                        {
                            //退款是排程处理,故无法确定何时执行退款,而现阶段退款操作成败与否都会记录在系统内
                            var curOutOrder = outOrderList.Where(y => y.transaction_id == curOrder.trade_no).FirstOrDefault();
                            if (curOutOrder != null && curOutOrder.result_code.Equals("SUCCESS"))
                            {
                                x.State = 3;//表示退款成功
                                tranModel.Add(BuildUpdateSql(x, "State"));
                            }
                            else if (curOutOrder != null && !curOutOrder.result_code.Equals("SUCCESS"))
                            {
                                x.State = 4;// 退款失败;
                                tranModel.Add(BuildUpdateSql(x, "State"));
                            }
                            //tranModel.Add(BuildUpdateSql(x, "State"));
                        }
                    });
                }
            }
            var isSuccess = ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);

            return isSuccess;
        }

        /// <summary>
        /// 指定时间内未支付 取消订单变为已经关闭
        /// </summary>
        /// <param name="timeoutlength"></param>
        public void updateOrderStateForCancle(int timeoutlength = -1)
        {
            TransactionModel tranModel = new TransactionModel();

            //找出15分钟内未支付的订单

            tranModel = new TransactionModel();
            //从参与砍价商品领取记录里筛选已经生成订单并且1小时未支付的的订单
            List<BargainUser> orderList = GetList($"(State=0 or State=5) and CreateOrderTime <= (NOW()+INTERVAL {timeoutlength} HOUR) and CreateOrderTime!='0001-01-01 00:00:00'");
            if (orderList != null && orderList.Count > 0)
            {
                //找出需要增加库存的砍价商品
                List<Bargain> goodlist = BargainBLL.SingleModel.GetList($"Id in ({string.Join(",", orderList.Select(s => s.BId).Distinct())})");

                if (goodlist != null && goodlist.Count > 0)
                {
                    foreach (var item in orderList)
                    {
                        BargainUser _BargainUser = item;
                        _BargainUser.State = -1;//订单超过时间未支付

                        Bargain Bargain = goodlist.Where(x => x.Id == _BargainUser.BId).FirstOrDefault();
                        Bargain.RemainNum++;//库存加1

                        tranModel.Add(BuildUpdateSql(_BargainUser, "State"));
                        tranModel.Add(BargainBLL.SingleModel.BuildUpdateSql(Bargain, "RemainNum"));

                        //事务内某行sql执行受影响行数为0,会回滚整个事务
                        if (ExecuteTransactionDataCorect(tranModel.sqlArray))
                        {
                            log4net.LogHelper.WriteInfo(GetType(), item.Id + "CityMorders订单超过15分钟未支付取消成功");
                        }
                        else
                        {
                            log4net.LogHelper.WriteInfo(GetType(), item.Id + "CityMorders订单超过15分钟未支付取消失败");
                        }

                        #region 发送 砍价订单取消通知 模板消息 => 通知用户

                        object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(item, SendTemplateMessageTypeEnum.砍价订单取消通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(item, SendTemplateMessageTypeEnum.砍价订单取消通知, orderData);

                        #endregion 发送 砍价订单取消通知 模板消息 => 通知用户
                    }
                }
            }
        }

        /// <summary>
        /// 根据模板类型返回填充模板内容
        /// </summary>
        /// <param name="bargainId">砍价ID</param>
        /// <returns></returns>
        public object GetTemplateMessageData_PaySuccess(int bargainId)
        {
            var model = GetModel(bargainId) ?? new BargainUser();
            //string modelDtlName = "";

            var postData = new
            {
                keyword1 = new { value = model.OrderId, color = "#000000" },
                keyword2 = new { value = model.CreateOrderTimeStr, color = "#000000" },
                keyword3 = new { value = model.BName, color = "#000000" },
                keyword4 = new { value = model.BuyTimeStr, color = "#000000" },
                keyword5 = new { value = Enum.GetName(typeof(miniAppBuyMode), model.PayType), color = "#000000" },
                keyword6 = new { value = model.CurrentPriceStr, color = "#000000" },
                keyword7 = new { value = "待发货".ToString(), color = "#000000" },
            };
            return postData;
        }

        /// <summary>
        /// 根据模板类型返回填充模板内容
        /// </summary>
        /// <param name="bargainId">砍价ID</param>
        /// <returns></returns>
        public object GetTemplateMessageData_SendGoods(int bargainId, string storeName)
        {
            var model = GetModel(bargainId) ?? new BargainUser();

            var postData = new
            {
                keyword1 = new { value = model.CreateOrderTimeStr, color = "#000000" },
                keyword2 = new { value = storeName, color = "#000000" },
                keyword3 = new { value = model.OrderId, color = "#000000" },
                keyword4 = new { value = model.AddressDetail, color = "#000000" },
                keyword5 = new { value = model.SendGoodsTime.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                keyword6 = new { value = model.BName, color = "#000000" },
                keyword7 = new { value = "待收货", color = "#000000" },
            };
            return postData;
        }

        /// <summary>
        /// 获取已确认收货并未评价的购物车数据，每次获取前1000条
        /// </summary>
        /// <returns></returns>
        public List<BargainUser> GetSuccessDataList(int iscomment = 0, int day = -15)
        {
            List<BargainUser> list = new List<BargainUser>();
            string sqlwhere = "";
            //1:已评论,0:未评论
            if (iscomment >= 0)
            {
                sqlwhere = $" and a.iscommentting={iscomment} ";
            }
            string sql = $"select a.* from bargainuser a left join bargain b on a.bid = b.Id where a.state = 8 and a.ConfirmReceiveGoodsTime<= '{DateTime.Now.AddDays(day)}' {sqlwhere} LIMIT 100";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                list = base.GetList(dr);
            }

            return list;
        }

        #region 商家小程序

        /// <summary>
        ///
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int GetPriceSumByAppId(int aid)
        {
            int priceSum = 0;
            if (aid <= 0)
            {
                return priceSum;
            }
            string sql = $"select sum(currentprice) pricesum from bargainuser a where a.aid = {aid} and a.state = 8";
            var result = SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, sql);
            if (result != DBNull.Value)
            {
                priceSum = Convert.ToInt32(result);
            }
            return priceSum;
        }

        /// <summary>
        /// 根据条件获取小程序订单数量
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetBargainOrderSum(int aid, int state = -999, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (aid <= 0 || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return count;
            }
            string sql = $"select count(1) from bargainuser where aid = {aid}  ";
            string sqlwhere = string.Empty;
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                paramters.Add(new MySqlParameter("@startDate", startDate));
                paramters.Add(new MySqlParameter("@endDate", endDate));
                // sqlwhere = " and BuyTime>=@startDate and BuyTime<=@endDate";
                sql += " and BuyTime  between @startDate and @endDate";
            }
            if (state != -999)
            {
                sql += $" and state={state}";
            }
            //log4net.LogHelper.WriteInfo(GetType(), sql);
            count = GetCountBySql(sql, paramters.ToArray());
            return count;
        }

        /// <summary>
        /// 砍价发货
        /// </summary>
        /// <param name="bargainUser"></param>
        /// <param name="bargain"></param>
        /// <param name="appId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SendGoods(BargainUser bargainUser, Bargain bargain, int aid, out string msg, string attachData = "")
        {
            bool result = false;
            bool addExpressResult = false;
            if (bargainUser.State == 6)
            {
                msg = "已经发货了,不能修改！";
                return result;
            }
            //if (!string.IsNullOrEmpty(bargainUser.WayBillNo) || !string.IsNullOrEmpty(bargainUser.SendGoodsName))
            //{
            //    msg = "已经发货了,不能修改！";
            //    return result;
            //}
            //if (string.IsNullOrEmpty(WayBillNo) || string.IsNullOrEmpty(SendGoodsName))
            //{
            //    return Json(new { code = -1, msg = "快递单号或者名称不能为空！" });
            //}
            //if (SendGoodsName.Length > 8)
            //{
            //    return Json(new { code = -1, msg = "快递名称过长" });
            //}

            if (!string.IsNullOrEmpty(attachData))
            {
                DeliveryUpdatePost DeliveryInfo = System.Web.Helpers.Json.Decode<DeliveryUpdatePost>(attachData);
                if (DeliveryInfo != null)
                {
                    addExpressResult = DeliveryFeedbackBLL.SingleModel.AddOrderFeed(bargainUser.Id, DeliveryInfo, DeliveryOrderType.专业版砍价发货);
                    if (!addExpressResult)
                    {
                        msg = "物流信息添加失败，发货失败！";
                        return result;
                    }
                }
            }

            bargainUser.State = 6;
            //bargainUser.WayBillNo = WayBillNo;
            //bargainUser.SendGoodsName = SendGoodsName;
            bargainUser.SendGoodsTime = DateTime.Now;
            if (Update(bargainUser, "SendGoodsTime,State"))
            {
                var storeName = "";
                if (bargain != null)
                {
                    switch (bargain.BargainType)
                    {
                        case 0:
                            var store = StoreBLL.SingleModel.GetModel(bargain.StoreId);
                            if (store != null)
                            {
                                var paramslist = ConfParamBLL.SingleModel.GetListByRId(aid) ?? new List<ConfParam>();
                                storeName = paramslist.Where(w => w.Param == "nparam").FirstOrDefault()?.Value;
                            }
                            break;

                        case 1:
                            storeName = OpenAuthorizerConfigBLL.SingleModel.GetModel($" rid = {aid} ")?.nick_name;
                            break;

                        default:
                            storeName = "";
                            break;
                    }
                }

                #region 模板消息

                try
                {
                    XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
                    if (app == null)
                    {
                        throw new Exception($"发送砍价发货模板消息参数错误 app_null :aid = {aid}");
                    }
                    XcxTemplate xcxTemp = XcxTemplateBLL.SingleModel.GetModel(app.TId);
                    if (xcxTemp == null)
                    {
                        throw new Exception($"发送砍价发货模板消息参数错误 xcxTemp_null :xcxTempId = {app.TId}");
                    }
                    Account account = AccountBLL.SingleModel.GetModel(app.AccountId);
                    if (account == null)
                    {
                        throw new Exception($"发送砍价发货模板消息参数错误 account_null :accountId = {app.AccountId}");
                    }
                    //电商为旧做法,兼容电商
                    switch (xcxTemp.Type)
                    {
                        case (int)TmpType.小程序电商模板:

                            #region 购买者模板消息

                            var postData = new BargainUserBLL().GetTemplateMessageData_SendGoods(bargainUser.Id, storeName);
                            TemplateMsg_Miniapp.SendTemplateMessage(bargainUser.UserId, SendTemplateMessageTypeEnum.电商订单配送通知, (int)TmpType.小程序电商模板, postData);

                            #endregion 购买者模板消息

                            break;

                        default:
                            object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(bargainUser, SendTemplateMessageTypeEnum.砍价订单发货提醒);
                            TemplateMsg_Miniapp.SendTemplateMessage(bargainUser.UserId, SendTemplateMessageTypeEnum.砍价订单发货提醒,xcxTemp.Type,orderData);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteError(GetType(), ex);
                }

                #endregion 模板消息

                result = true;
                msg = "发货成功！";
                return result;
            }
            else
            {
                result = false;
                msg = "发货异常！";
                return result;
            }
        }

        /// <summary>
        /// 砍价退款（照搬后台的）
        /// </summary>
        /// <param name="bargainUser"></param>
        /// <param name="bargain"></param>
        /// <param name="appId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool OutOrder(BargainUser bargainUser, Bargain bargain, string appId, out string msg)
        {

            bargainUser.State = 2;
            bargainUser.outOrderDate = DateTime.Now;

            if (bargainUser.PayType == (int)miniAppBuyMode.储值支付)
            {
                bargainUser.refundFee = bargainUser.CurrentPrice + bargain.GoodsFreight;
                bargainUser.State = 3;
                var saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(appId, bargainUser.UserId);
                TransactionModel tran = new TransactionModel();
                tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
                {
                    AppId = appId,
                    UserId = bargainUser.UserId,
                    MoneySetUserId = saveMoneyUser.Id,
                    Type = 1,
                    BeforeMoney = saveMoneyUser.AccountMoney,
                    AfterMoney = saveMoneyUser.AccountMoney + bargainUser.refundFee,
                    ChangeMoney = bargainUser.refundFee,
                    ChangeNote = $"小程序砍价购买商品[{bargainUser.BName}]退款,订单号:{bargainUser.OrderId} ",
                    CreateDate = DateTime.Now,
                    State = 1
                }));

                tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney + {bargainUser.refundFee} where id =  {saveMoneyUser.Id} ; ");

                string updateBargainUser = BuildUpdateSql(bargainUser, "State,outOrderDate,refundFee");

                tran.Add(updateBargainUser);

                bool isok = BargainBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray);
                if (isok)
                {
                    object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(bargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, "商家操作退款");
                    TemplateMsg_Miniapp.SendTemplateMessage(bargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, orderData);
                    msg = "退款成功,请查看账户余额!";
                }
                else
                {
                    msg = "退款异常!";//返回订单信息
                }
                return isok;
            }
            else
            {
                bool isok = false;

                CityMorders order = new CityMordersBLL().GetModel(bargainUser.CityMordersId);
                if (order == null)
                {
                    msg = "订单信息有误!";
                    return isok;
                }
                bargainUser.refundFee = bargainUser.CurrentPrice + bargain.GoodsFreight;
                if (new BargainUserBLL().Update(bargainUser, "State,outOrderDate,refundFee"))
                {
                    ReFundQueue reModel = new ReFundQueue
                    {
                        minisnsId = -5,
                        money = bargainUser.refundFee,
                        orderid = order.Id,
                        traid = order.trade_no,
                        addtime = DateTime.Now,
                        note = "小程序砍价订单退款",
                        retype = 1
                    };
                    try
                    {
                        int funid = Convert.ToInt32(new ReFundQueueBLL().Add(reModel));
                        if (funid > 0)
                        {
                            object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(bargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, "商家操作退款");
                            TemplateMsg_Miniapp.SendTemplateMessage(bargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, orderData);
                            isok = true;
                            msg = "操作成功,已提交退款申请!";
                            return isok;
                            // return Json(new { isok = true, msg = "", obj = funid });
                        }
                        else
                        {
                            isok = false;
                            msg = "退款异常插入队列小于0!";
                            return isok;
                        }
                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteInfo(GetType(), $"{ex.Message} xxxxxxxxxxxxxxxx小程序砍价退款订单插入队列失败 ID={order.Id}");
                        isok = false;
                        msg = "退款异常(插入队列失败)!";
                        return isok;
                    }
                }
                else
                {
                    isok = false;
                    msg = "退款异常!";
                    return isok;
                }
            }
        }

        public int GetPriceSumByAppId_Date(int aid, string startDate, string endDate)
        {
            int priceSum = 0;
            if (aid <= 0 || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return priceSum;
            }
            //string sql = $"select sum(currentprice) pricesum from bargainuser where aid = {aid} and state = 8 and BuyTime>=@startDate and BuyTime<=@endDate";
            string sql = $"select sum(currentprice) pricesum from bargainuser where aid = {aid} and state = 8 and BuyTime  between @startDate and @endDate";
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@startDate", startDate));
            paramters.Add(new MySqlParameter("@endDate", endDate));
            var result = SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, sql, paramters.ToArray());
            if (result != DBNull.Value)
            {
                priceSum = Convert.ToInt32(result);
            }
            return priceSum;
        }

        public int GetBargainOrderSum(int aid, string states, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (aid <= 0)
            {
                return count;
            }
            string str = string.Empty;
            List<MySqlParameter> paramters = new List<MySqlParameter>();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                paramters.Add(new MySqlParameter("@startDate", startDate));
                paramters.Add(new MySqlParameter("@endDate", endDate));
                str = " and BuyTime>=@startDate and BuyTime<=@endDate";
            }
            string sql = $"select count(1) count from bargainuser where aid = {aid} and state in ({states}) {str}";
            count = GetCountBySql(sql, paramters.ToArray());
            return count;
        }

        public int GetPayUserCount(int aid, string startDate, string endDate)
        {
            int count = 0;
            if (aid <= 0 || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return count;
            }
            string sql = $"select count(1) count from bargainuser aid = {aid} and state in (1,6,7,8) and BuyTime>=@startDate and BuyTime<=@endDate group by userid";
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@startDate", startDate));
            paramters.Add(new MySqlParameter("@endDate", endDate));
            count = GetCountBySql(sql, paramters.ToArray());
            return count;
        }

        public int GetOrderSumByCondition(int aid, int type, string value, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (aid <= 0)
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            string sqlwhere = GetSqlwhere(paramters, type, value, startDate, endDate);
            if (sqlwhere == null)
            {
                return count;
            }
            string sql = $"select count(1) count from bargainuser a where a.aid = {aid}{sqlwhere} ";

            count = GetCountBySql(sql, paramters.ToArray());
            return count;
        }

        public object GetListByCondition(int aid, int pageIndex, int pageSize, int state, int type, string value, string startDate, string endDate)
        {

            List<BargainUser> bargainUserList = new List<BargainUser>();
            if (aid <= 0)
            {
                return bargainUserList;
            }

            List<int> listBId = BargainBLL.SingleModel.GetBidsByStoreId(aid);

            if (listBId == null || listBId.Count <= 0)
            {
                return bargainUserList;
            }
            string sqlwhere = $" a.BId in({string.Join(",", listBId)})";
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            sqlwhere += GetSqlwhere(paramters, type, value, startDate, endDate);
            if (sqlwhere == null)
            {
                return bargainUserList;
            }
            if (state != -999)
            {
                sqlwhere += $" and a.state={state} ";
            }

            bargainUserList = GetJoinList(sqlwhere, pageSize, pageIndex, "a.CreateDate desc", 0, paramters);
            return bargainUserList;
        }

        public int GetOrderSumByCondition(int aid, int type, string value, int state)
        {
            int count = 0;
            if (aid <= 0)
            {
                return count;
            }
            string sqlwhere = string.Empty;
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            sqlwhere = GetSqlwhere(paramters, type, value);

            string sql = $"select count(1) count from bargainuser a where a.aid = {aid} and a.state={state}{sqlwhere}";
            count = GetCountBySql(sql, paramters.ToArray());
            return count;
        }

        private string GetSqlwhere(List<MySqlParameter> paramters, int type, string value, string startDate = "", string endDate = "")
        {
            string sqlwhere = " and a.OrderId is not null and a.OrderId!=''";
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlwhere += $" and a.BuyTime>=@startDate and a.BuyTime<=@endDate";
                paramters.Add(new MySqlParameter("@startDate", startDate));
                paramters.Add(new MySqlParameter("@endDate", endDate));
            }
            if (!string.IsNullOrEmpty(value))
            {
                switch (type)
                {
                    case 0://订单号
                        sqlwhere += " and a.OrderId like @orderId";
                        paramters.Add(new MySqlParameter("@orderId", $"%{value}%"));
                        break;

                    case 1://商品名称
                        List<Bargain> bargainList = BargainBLL.SingleModel.GetList($" BName like '%{value}%'");
                        if (bargainList == null || bargainList.Count <= 0) return null;
                        sqlwhere += $" and a.Bid in ({string.Join(",", bargainList.Select(b => b.Id))})";
                        break;

                    case 2://手机号码
                        sqlwhere += $" and a.address like @phone";
                        paramters.Add(new MySqlParameter("@phone", $"%{value}%"));
                        break;

                    case 3://客户名
                        sqlwhere += $" and a.address like @name";
                        paramters.Add(new MySqlParameter("@name", $"%{value}%"));
                        break;
                }
            }
            return sqlwhere;
        }

        #endregion 商家小程序
    }
}