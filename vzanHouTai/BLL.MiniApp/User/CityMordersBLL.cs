using BLL.MiniApp.cityminiapp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Pin;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Qiye;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using BLL.MiniApp.User;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Pin;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using Entity.MiniApp.Qiye;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp
{
    public class CityMordersBLL : BaseMySql<CityMorders>
    {

        public CityMordersBLL() { }
        public CityMordersBLL(PayResult result, CityMorders order = null)
        {
            Order = order;
            Result = result;
            _tranModel = new TransactionModel();
        }
        public PayResult Result;
        public CityMorders Order;
        private readonly TransactionModel _tranModel;
        private MySqlParameter[] _pone;

        #region 基础操作
        public List<CityMorders> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<CityMorders>();
            return base.GetList($"id in ({ids})");
        }
        #endregion

        /// <summary>
        /// 小程序购买拼团
        /// </summary>
        /// <returns></returns>
        public bool MiniappStoreGroup()
        {
            //是否发消息给商家
            bool sendmsgtostorer = false;

            if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
            {
                WritePayException("支付失败");
                return false;
            }

            //修改状态

            Groups group = GroupsBLL.SingleModel.GetModel(Order.Articleid);
            if (null == group)
            {
                WritePayException("找不到Group，ID为" + Order.Articleid);
                return false;
            }

            Store store = StoreBLL.SingleModel.GetModel(group.StoreId);
            if (store == null)
            {
                WritePayException("购买拼团，店铺找不到");
                return false;
            }



            GroupSponsor groupSponsor = new GroupSponsor();
            int count = 0;
            //判断是否团购
            if (Order.is_group == 1)
            {
                groupSponsor = GroupSponsorBLL.SingleModel.GetModel(Order.groupsponsor_id);
                if (groupSponsor == null)
                {
                    WritePayException("购买拼团，拼团id找不到");
                    return false;
                }
                //判断是否开团，如果是，开团成功
                if (Order.is_group_head == 1)
                {
                    //GroupSponsor.State = 1;
                    string updateGroupSponsorSql = $"update GroupSponsor set State=1 where id={groupSponsor.Id}";
                    _tranModel.Add(updateGroupSponsorSql);
                }
                //判断参团人数是否达标，如果达标 表示拼团成功
                count = GroupUserBLL.SingleModel.GetCount($"GroupId={group.Id} and state={(int)MiniappPayState.待发货} and GroupSponsorId={groupSponsor.Id} and IsGroup=1");
                count++;
                if (groupSponsor.GroupSize == count)
                {
                    //GroupSponsor.State = 2; //拼团成功
                    string updateGroupSponsorSql = $"update GroupSponsor set State=2 where id={groupSponsor.Id}";
                    _tranModel.Add(updateGroupSponsorSql);

                    //拼团成功发消息给商家
                    sendmsgtostorer = true;
                }
            }
            else
            {
                //单买拼团商品发消息给商家
                sendmsgtostorer = true;
            }

            //用户获得团购
            GroupUser userGroup = new GroupUser();
            userGroup = GroupUserBLL.SingleModel.GetModel(Order.CommentId);
            if (userGroup == null)
            {
                userGroup = new GroupUser();
            }

            userGroup.DiscountGuid = Guid.NewGuid().ToString().Replace("-", "");
            userGroup.CreateDate = DateTime.Now;
            userGroup.GroupId = group.Id;
            userGroup.GroupSponsorId = Order.groupsponsor_id;
            userGroup.BuyNum = Order.buy_num;
            userGroup.BuyPrice = Order.payment_free;
            userGroup.IsGroup = Order.is_group;
            userGroup.IsGroupHead = Order.is_group_head;
            userGroup.ObtainUserId = Order.FuserId;
            userGroup.State = (int)MiniappPayState.待发货;
            userGroup.OrderNo = Order.orderno;
            userGroup.OrderId = Order.Id;
            userGroup.PayTime = DateTime.Now;
            userGroup.Address = Order.remark;
            userGroup.PayType = Order.ActionType == 1 ? Order.ActionType : 0;
            userGroup.ValidNumber = new Random().Next(100000, 999999);
            userGroup.AppId = Order.appid;
            userGroup.Phone = Order.AttachPar;
            userGroup.UserName = Order.Tusername;
            userGroup.Note = Order.Note;

            if (userGroup.Id <= 0)
            {
                userGroup.Id = Convert.ToInt32(GroupUserBLL.SingleModel.Add(userGroup));
            }
            else
            {
                GroupUserBLL.SingleModel.Update(userGroup);
            }

            if (userGroup.Id <= 0)
            {
                log4net.LogHelper.WriteInfo(GetType(), "添加拼团记录失败");
                WritePayException("添加拼团记录失败！");
                return false;
            }

            //判断是否是储值支付
            if (Order.ActionType == 1)
            {
                Order.Id = userGroup.Id;

                //储值支付
                _tranModel.Add(SaveMoneySetUserBLL.SingleModel.GetCommandCarPriceSql(Order.appid, Order.FuserId, Order.payment_free, -1, Order.Id, Order.orderno).ToArray());

                if (!ExecuteTransactionDataCorect(_tranModel.sqlArray, _tranModel.ParameterArray))
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception(Newtonsoft.Json.JsonConvert.SerializeObject(_tranModel.sqlArray)));
                    WritePayException("支付成功回调处理失败！");
                    return false;
                }
            }
            else
            {
                if (!MyExecuteTransaction())
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception("MiniappStoreGroup支付回调处理失败！！" + Result.Id));
                    return false;
                }
            }

            //是否发消息给商家
            if (sendmsgtostorer)
            {
                //发送模板消息通知商家
                TemplateMsg_Gzh.SendGroupsPaySuccessTemplateMessage(userGroup);

                #region 发送模板消息通知商家 --防止错误,预留可以代码回滚
                //var xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(userGroup.AppId) ?? new XcxAppAccountRelation();
                //var storemodel = new StoreBLL().GetModelByRid(xcxrelation.Id) ?? new Store();
                //var account = AccountBLL.SingleModel.GetModel(xcxrelation.AccountId) ?? new Account();

                //var title = $"您的拼团商品有新的订单！";
                //var remark1 = $" 该订单于 {DateTime.Now.ToString("yyyy-MM-dd HH:mm")} 下单. ";
                //var userBaseInfo = new UserBaseInfoBLL().GetModelByUnionidServerid(account.UnionId, WebSiteConfig.DZ_WxSerId);
                //if (userBaseInfo != null && !string.IsNullOrWhiteSpace(userBaseInfo.openid))
                //{

                //    //提醒用户发送消息成功
                //    C_TplMsgHelper.SendTplMsgFromVzan(userBaseInfo.openid, "", title, Order.buy_num.ToString(), (Convert.ToDecimal(Order.payment_free) / 100) + " 元", "", group.GroupName, remark1);
                //}
                #endregion
            }


            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(store.appId);
            XcxTemplate xcxtemp = null;
            if (xcx != null)
            {

                //新订单电脑语音提示
                Utils.RemoveIsHaveNewOrder(xcx.Id);

                //支付成功通知
                xcxtemp = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
                if (xcxtemp != null)
                {
                    object groupData = TemplateMsg_Miniapp.GroupGetTemplateMessageData(string.Empty, userGroup, SendTemplateMessageTypeEnum.拼团基础版订单支付成功通知);
                    TemplateMsg_Miniapp.SendTemplateMessage(userGroup.ObtainUserId, SendTemplateMessageTypeEnum.拼团基础版订单支付成功通知, xcxtemp.Type, groupData);
                }
            }



            //拼团成功通知
            if (sendmsgtostorer)
            {
                List<GroupUser> gUsers = GroupUserBLL.SingleModel.GetList($" GroupSponsorId = {userGroup.GroupSponsorId}  and state = {(int)MiniappPayState.待发货} ");
                gUsers?.ForEach(g =>
                {
                    if (xcxtemp != null && userGroup.IsGroup == 1 && userGroup.GroupSponsorId > 0)
                    {
                        object groupData = TemplateMsg_Miniapp.GroupGetTemplateMessageData(string.Empty, g, SendTemplateMessageTypeEnum.拼团拼团成功提醒);
                        TemplateMsg_Miniapp.SendTemplateMessage(g.ObtainUserId, SendTemplateMessageTypeEnum.拼团拼团成功提醒, xcxtemp.Type, groupData);
                    }
                });
            }

            return true;
        }

        public bool PayPinOrder()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException($"拼享惠 {Result.Id}  支付失败");
                    return false;
                }
                string errMsg = string.Empty;//错误提示信息

                PinGoodsOrderBLL orderBLL = new PinGoodsOrderBLL();
                PinGoodsOrder pinOrder = orderBLL.GetModel(Order.CommentId);
                if (pinOrder == null)
                {
                    WritePayException($"拼享惠{Result.Id} 支付回调处理失败,找不到订单 orderId:{Order.CommentId}");
                    return false;
                }
                //支付成功修改订单状态
                if (!orderBLL.UpdateState(pinOrder, _tranModel))
                {
                    //pinGroupBLL.RollbackEntrantCount(pinOrder);
                    WritePayException($"拼享惠{Result.Id} 支付回调处理失败,生成订单状态数据处理失败");
                    return false;
                }
                //如果是代理费，需要修改代理状态


                if (pinOrder.orderType == 1)
                {
                    PinAgent agent = PinAgentBLL.SingleModel.GetModel($"id={pinOrder.goodsId} and state>=0");
                    if (agent == null)
                    {
                        WritePayException($"拼享惠{Result.Id} 支付回调处理:代理状态修改失败!" + Result.Id);
                    }
                    else
                    {
                        agent.state = 1;
                        PinStoreBLL.SingleModel.AddStore(agent);
                        _tranModel.Add(PinAgentBLL.SingleModel.BuildUpdateSql(agent, "state"));
                        if (agent.fuserId > 0)
                        {
                            PinAgentBLL.SingleModel.UpdateIncome(agent, pinOrder, _tranModel);//上级代理获得提成
                        }
                        PinAgentIncomeLogBLL.SingleModel.AddAgentLog(agent, pinOrder, _tranModel);//插入提成日志
                    }
                }

                if (!MyExecuteTransaction())
                {
                    //pinGroupBLL.RollbackEntrantCount(pinOrder);
                    WritePayException($"拼享惠{Result.Id} 支付回调处理失败!" + Result.Id);
                    return false;
                }
                orderBLL.SendTemplateMsg_PaySuccess(pinOrder);
                return true;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("拼享惠支付回调处理失败！！" + Result.Id + ex.Message));
                return false;
            }

        }
        /// <summary>
        /// 生成微信订单
        /// </summary>
        /// <param name="OrderType"></param>
        /// <param name="ActionType"></param>
        /// <param name="buyPrice"></param>
        /// <param name="Percent"></param>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="nickName"></param>
        /// <param name="orderId"></param>
        /// <param name="appId"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public CityMorders CreateCityMorder(int OrderType, int ActionType, int buyPrice, int Percent, int aid, int userId, string nickName, int orderId, string appId, string appName)
        {
            string no = WxPayApi.GenerateOutTradeNo();

            CityMorders citymorderModel = new CityMorders
            {
                OrderType = (int)ArticleTypeEnum.MiniappEnt,
                ActionType = (int)ArticleTypeEnum.MiniappEnt,
                Addtime = DateTime.Now,
                payment_free = buyPrice,
                trade_no = no,
                Percent = 99,//不收取服务费
                userip = WebHelper.GetIP(),
                FuserId = userId,
                Fusername = nickName,
                orderno = no,
                payment_status = 0,
                Status = 0,
                Articleid = 0,
                MinisnsId = aid,// 订单aId
                TuserId = orderId,//订单的ID
                ShowNote = $" {appName}购买商品支付{buyPrice * 0.01}元",
                CitySubId = 0,//无分销,默认为0
                PayRate = 1,
                buy_num = 0, //无
                appid = appId,
            };

            citymorderModel.Id = Convert.ToInt32(Add(citymorderModel));
            return citymorderModel;
        }

        /// <summary>
        /// 多门店微信支付回调
        /// </summary>
        /// <returns></returns>
        public bool MiniappMultiStore()
        {
            EntGoodsOrder goodsOrder = EntGoodsOrderBLL.SingleModel.GetModel($"id={Order.TuserId}");

            //支付后处理
            if (!EntGoodsOrderBLL.SingleModel.HandleAfterPayOrder(goodsOrder, $"cityMorder.Id:{Order.Id},entGoodsOrderId:{Order.TuserId}"))
            {
                return false;
            }

            if (!MyExecuteTransaction())
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(" EntGoods支付回调处理失败！！" + Result.Id));
                return false;
            }
            return true;

        }

        /// <summary>
        /// 足浴版微信支付回调
        /// </summary>
        /// <returns></returns>
        public bool MiniappFootbath()
        {

            EntGoodsOrder goodsOrder = EntGoodsOrderBLL.SingleModel.GetModel($"id={Order.TuserId}");
            goodsOrder.PayDate = DateTime.Now;
            if (goodsOrder == null) return false;
            goodsOrder.State = (int)MiniAppEntOrderState.待服务;
            if (Order.ActionType == (int)ArticleTypeEnum.MiniappFootbathGift)
            {
                goodsOrder.State = (int)MiniAppEntOrderState.交易成功;

            }
            if (EntGoodsOrderBLL.SingleModel.Update(goodsOrder, "state,paydate"))
            {
                VipRelationBLL.SingleModel.updatelevel(goodsOrder.UserId, "footbath");

                if (Order.ActionType == (int)ArticleTypeEnum.MiniappFootbathGift)
                {
                    EntGoods curGift = EntGoodsBLL.SingleModel.GetModel(Convert.ToInt32(goodsOrder.GoodsGuid));
                    if (curGift == null)
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception(" 找不到goodsOrder！！" + goodsOrder.GoodsGuid));
                    }
                    else
                    {
                        //增加收取鲜花的数量
                        TechnicianInfo curTechnicianInfo = TechnicianInfoBLL.SingleModel.GetModel(goodsOrder.FuserId);
                        if (curTechnicianInfo == null)
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception(" 找不到goodsOrder！！" + goodsOrder.FuserId));
                        }
                        curTechnicianInfo.GetItGiftCount += curGift.stock;
                        TechnicianInfoBLL.SingleModel.Update(curTechnicianInfo, "GetItGiftCount");
                    }

                    TemplateMsg_Gzh.SendGiftTemplateMessage(goodsOrder);
                }
                else
                {
                    TemplateMsg_Gzh.SendReserveTemplateMessage(goodsOrder);
                }
                // TemplateMsg_Gzh.SendOrderSuccessTemplateMessage(goodsOrder);
            }
            if (!MyExecuteTransaction())
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(" EntGoods支付回调处理失败！！" + Result.Id));
                return false;
            }
            FootBath store = FootBathBLL.SingleModel.GetModel($"id={Order.MinisnsId}");
            EntGoodsCart goodsCart = EntGoodsCartBLL.SingleModel.GetModel($"GoodsOrderId={Order.TuserId}");
            if (store == null || goodsCart == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 小程序商城商品付费
        /// </summary>
        /// <returns></returns>
        public bool MiniappStoreGoods()
        {
            if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
            {
                WritePayException("支付失败");
                return false;
            }

            #region 实例化BLL










            #endregion

            List<Entity.MiniApp.Stores.StoreGoodsOrder> orderList = StoreGoodsOrderBLL.SingleModel.GetList($"OrderId={Order.Id}");
            if (orderList != null && orderList.Count > 0)
            {
                foreach (Entity.MiniApp.Stores.StoreGoodsOrder gOrder in orderList)
                {
                    gOrder.PayDate = DateTime.Now;
                    gOrder.State = gOrder.FreightTemplateId > 0 ? (int)OrderState.待发货 : (int)OrderState.待核销;
                    gOrder.GoodsGuid = Guid.NewGuid().ToString().Replace("-", "");
                    _tranModel.Add(StoreGoodsOrderBLL.SingleModel.BuildUpdateSql(gOrder, "State,GoodsGuid,PayDate", out _pone), _pone);
                    _tranModel.Add(StoreGoodsOrderLogBLL.SingleModel.BuildAddSql(new Entity.MiniApp.Stores.StoreGoodsOrderLog() { GoodsOrderId = gOrder.Id, UserId = gOrder.UserId, LogInfo = $" 订单成功支付：{gOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));
                }
            }
            else
            {
                WritePayException("小程序商城商品支付后处理失败：实体为空，orderid =" + Order.Id);
                return false;
            }

            if (!MyExecuteTransaction())
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(" StoreGoods支付回调处理失败！！" + Result.Id));
                return false;
            }

            List<StoreGoodsCart> goodCartList = new List<StoreGoodsCart>();

            //自动打单
            foreach (StoreGoodsOrder gOrder in orderList)
            {
                Store food = StoreBLL.SingleModel.GetModel(gOrder.StoreId) ?? new Store();
                XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(food.appId) ?? new XcxAppAccountRelation();
                Account account = AccountBLL.SingleModel.GetModel(app.AccountId) ?? new Account();

                //读取商家绑定的打印机列表
                List<FoodPrints> PrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {gOrder.StoreId}  and state >= 0  and industrytype=2") ?? new List<FoodPrints>();
                goodCartList = StoreGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={gOrder.Id} and state=1 ");//订单的购物车信息
                //打印
                string content = PrinterHelper.storePrintOrderContent(gOrder);
                PrinterHelper.printContent(PrintList, content, gOrder.Id, app.TId, account);


                #region 发送电商订单支付通知 模板消息
                var postData = StoreGoodsOrderBLL.SingleModel.getTemplateMessageData(gOrder.Id, SendTemplateMessageTypeEnum.电商订单支付成功通知);
                TemplateMsg_Miniapp.SendTemplateMessage(gOrder.UserId, SendTemplateMessageTypeEnum.电商订单支付成功通知, (int)TmpType.小程序电商模板, postData);
                #endregion

                //发送模板消息通知商家
                TemplateMsg_Gzh.SendStorePaySuccessTemplateMessage(gOrder);
            }
            return true;
        }

        /// <summary>
        /// 小程序餐饮付费
        /// </summary>
        /// <returns></returns>
        public bool MiniappFoodGoods(int price = 1, FoodGoodsOrder foodGoodsOrder = null)
        {
            try
            {
                if (price > 0 && Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  支付失败");
                    return false;
                }
                if (Order == null)
                {
                    WritePayException("支付失败");
                    return false;
                }

                List<FoodGoodsOrder> orderList = null;
                if (foodGoodsOrder != null)
                {
                    orderList = new List<FoodGoodsOrder>() { foodGoodsOrder };
                }
                else
                {
                    orderList = FoodGoodsOrderBLL.SingleModel.GetList($"OrderId={Order.Id}");
                }

                if (orderList != null && orderList.Count > 0)
                {
                    XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(Order.appid);
                    if (xcxrelation != null)
                    {
                        //新订单电脑语音提示
                        Utils.RemoveIsHaveNewOrder(xcxrelation.Id);
                    }

                    foreach (FoodGoodsOrder gOrder in orderList)
                    {
                        var food = FoodBLL.SingleModel.GetModel(gOrder.StoreId);
                        gOrder.PayDate = DateTime.Now;//支付时间
                        gOrder.State = (int)miniAppFoodOrderState.待接单;

                        //自动接单则跳过待接单状态
                        if (food != null && food.AutoAcceptOrder == 1)
                        {
                            gOrder.State = (gOrder.OrderType == (int)miniAppFoodOrderType.堂食 ? (int)miniAppFoodOrderState.待就餐 : (int)miniAppFoodOrderState.待送餐);
                            gOrder.ConfDate = DateTime.Now;//接单时间
                            gOrder.DistributeDate = DateTime.Now;
                        }

                        //更新预约状态
                        if (gOrder.OrderType == (int)miniAppFoodOrderType.预约 && gOrder.ReserveId > 0)
                        {
                            var reservation = FoodReservationBLL.SingleModel.GetModel(gOrder.ReserveId);
                            _tranModel.Add(FoodReservationBLL.SingleModel.UpdateToPay(reservation));
                        }

                        gOrder.GoodsGuid = Guid.NewGuid().ToString().Replace("-", "");//此栏位暂无用处
                        _tranModel.Add(FoodGoodsOrderBLL.SingleModel.BuildUpdateSql(gOrder, "State,GoodsGuid,PayDate,ConfDate,DistributeDate", out _pone), _pone);

                        List<FoodGoodsCart> orderDetailList = FoodGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={gOrder.Id}");

                        _tranModel.Add(FoodGoodsOrderLogBLL.SingleModel.BuildAddSql(new FoodGoodsOrderLog() { GoodsOrderId = gOrder.Id, UserId = gOrder.UserId.ToString(), LogInfo = $" 订单成功支付：{gOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));

                        //修改拼团状态
                        TransactionModel tran = new TransactionModel();
                        EntGroupSponsorBLL.SingleModel.PayReturnUpdateGroupState(gOrder.GroupId, food.appId, ref tran, 0, (int)TmpType.小程序餐饮模板);
                        if (tran.sqlArray != null && tran.sqlArray.Length > 0)
                        {
                            _tranModel.Add(tran.sqlArray);
                        }

                        //第三方配送
                        TransactionModel temptran = new TransactionModel();
                        string dadamsg = DistributionApiConfigBLL.SingleModel.UpdatePeiSongOrder(gOrder.Id, food.appId, (int)TmpType.小程序餐饮模板, gOrder.GetWay, ref temptran, false);
                        if (!string.IsNullOrEmpty(dadamsg))
                        {
                            LogHelper.WriteInfo(this.GetType(), dadamsg);
                        }
                    }
                }
                else
                {
                    WritePayException("小程序餐饮菜品支付后处理失败：实体为空");
                    return false;
                }
                if (!MyExecuteTransaction())
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception(" FoodGoods支付回调处理失败！！" + Result.Id));
                    return false;
                }

                foreach (FoodGoodsOrder gOrder in orderList)
                {
                    FoodGoodsOrderBLL.SingleModel.AfterPaySuccesExecFun(gOrder);
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(" FoodGoods支付回调处理失败！！" + Result.Id + ex.Message));
            }
            return true;
        }

        /// <summary>
        /// 平台独立模板付费
        /// </summary>
        /// <returns></returns>
        public bool MiniappPlatChildGoods(int price = 1, PlatChildGoodsOrder platChildGoodsOrder = null)
        {
            try
            {
                if (price > 0 && Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  支付失败");
                    return false;
                }
                if (Order == null)
                {
                    WritePayException("支付失败");
                    return false;
                }

                if (platChildGoodsOrder == null)
                {
                    platChildGoodsOrder = PlatChildGoodsOrderBLL.SingleModel.GetModelByOrderId(Order.Id);
                }

                if (platChildGoodsOrder != null)
                {
                    PlatStore store = PlatStoreBLL.SingleModel.GetModel(platChildGoodsOrder.StoreId);
                    if (store == null)
                    {
                        WritePayException("微信支付：店铺失效");
                        return false;
                    }
                    platChildGoodsOrder.PayTime = DateTime.Now;//支付时间
                    switch (platChildGoodsOrder.GetWay)
                    {
                        case (int)miniAppOrderGetWay.商家配送:
                            platChildGoodsOrder.State = (int)PlatChildOrderState.待发货;
                            break;
                        case (int)miniAppOrderGetWay.到店自取:
                            platChildGoodsOrder.State = (int)PlatChildOrderState.待自取;
                            break;
                    }

                    _tranModel.Add(PlatChildGoodsOrderBLL.SingleModel.BuildUpdateSql(platChildGoodsOrder, "State,PayTime", out _pone), _pone);
                }
                else
                {
                    WritePayException("小程序独立模板支付后处理失败：实体为空");
                    return false;
                }
                if (!MyExecuteTransaction())
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception("独立模板支付回调处理失败！！" + Result.Id));
                    return false;
                }

                //发送支付成功通知给用户
                object orderData = TemplateMsg_Miniapp.PlatChildGetTemplateMessageData(platChildGoodsOrder, SendTemplateMessageTypeEnum.独立小程序版订单支付成功通知);
                TemplateMsg_Miniapp.SendTemplateMessage(platChildGoodsOrder.UserId, SendTemplateMessageTypeEnum.独立小程序版订单支付成功通知, TmpType.小未平台子模版, orderData, "pages/my/my-index/index");

                //发送支付成功通知给商家
                TemplateMsg_Gzh.SendPlatChildPaySuccessTemplateMessage(platChildGoodsOrder);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(" 独立模板支付回调处理失败！！" + Result.Id + ex.Message));
            }
            return true;
        }

        /// <summary>
        /// 企业智推版付费
        /// </summary>
        /// <returns></returns>
        public bool QiyePayOrder(int price = 1, QiyeGoodsOrder qiyeGoodsOrder = null)
        {
            try
            {
                if (price > 0 && Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  支付失败");
                    return false;
                }
                if (Order == null)
                {
                    WritePayException("支付失败");
                    return false;
                }

                if (qiyeGoodsOrder == null)
                {
                    qiyeGoodsOrder = QiyeGoodsOrderBLL.SingleModel.GetModelByOrderId(Order.Id);
                }

                if (qiyeGoodsOrder != null)
                {
                    QiyeStore store = QiyeStoreBLL.SingleModel.GetModel(qiyeGoodsOrder.StoreId);
                    if (store == null)
                    {
                        WritePayException("微信支付：店铺失效");
                        return false;
                    }
                    qiyeGoodsOrder.PayTime = DateTime.Now;//支付时间
                    switch (qiyeGoodsOrder.GetWay)
                    {
                        case (int)miniAppOrderGetWay.商家配送:
                            qiyeGoodsOrder.State = (int)PlatChildOrderState.待发货;
                            break;
                        case (int)miniAppOrderGetWay.到店自取:
                            qiyeGoodsOrder.State = (int)PlatChildOrderState.待自取;
                            break;
                    }

                    _tranModel.Add(QiyeGoodsOrderBLL.SingleModel.BuildUpdateSql(qiyeGoodsOrder, "State,PayTime"));
                }
                else
                {
                    WritePayException("小程序独立模板支付后处理失败：实体为空");
                    return false;
                }
                if (!MyExecuteTransaction())
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception("独立模板支付回调处理失败！！" + Result.Id));
                    return false;
                }

                //发送支付成功通知给用户
                object orderData = TemplateMsg_Miniapp.QiyeGetTemplateMessageData(qiyeGoodsOrder, SendTemplateMessageTypeEnum.企业智推版订单支付成功通知);
                TemplateMsg_Miniapp.SendTemplateMessage(qiyeGoodsOrder.UserId, SendTemplateMessageTypeEnum.企业智推版订单支付成功通知, TmpType.企业智推版, orderData);

                //发送支付成功通知给商家
                TemplateMsg_Gzh.SendQiyePaySuccessTemplateMessage(qiyeGoodsOrder);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(" 独立模板支付回调处理失败！！" + Result.Id + ex.Message));
            }
            return true;
        }

        public bool MiniappSaveMoney()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  支付失败");
                    return false;
                }

                var userlog = SaveMoneySetUserLogBLL.SingleModel.getModelByOrderId(Order.Id);
                if (userlog != null)
                {

                    userlog.State = 1;
                    //记录变动并更新
                    var user = SaveMoneySetUserBLL.SingleModel.GetModel(userlog.MoneySetUserId);
                    if (user != null)
                    {
                        userlog.BeforeMoney = user.AccountMoney;
                        userlog.AfterMoney = user.AccountMoney + userlog.ChangeMoney;
                        user.AccountMoney += userlog.ChangeMoney;

                        #region 赠送积分 储值充钱
                        SaveMoneySetUserLogBLL.SingleModel.Update(userlog);
                        SaveMoneySetUserBLL.SingleModel.Update(user, "AccountMoney");

                        XcxAppAccountRelation r = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(user.AppId);
                        if (r != null)
                        {
                            if (!ExchangeUserIntegralBLL.SingleModel.AddUserIntegral(user.UserId, r.Id, Result.total_fee, 0, 1))
                                log4net.LogHelper.WriteError(GetType(), new Exception(" SaveMoney支付回调赠送积分失败！！" + Result.Id));

                        }
                        else
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception(" SaveMoney支付回调赠送积分失败(小程序未授权)" + Result.Id));
                        }
                        #endregion

                        //升级会员 储值充钱
                        VipRelationBLL.SingleModel.updatelevelBySaveMoney(user.UserId, userlog.ChangeMoney - userlog.GiveMoney);
                    }
                    if (!MyExecuteTransaction())
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception(" SaveMoney支付回调处理失败！！" + Result.Id));
                        return false;
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(" SaveMoney支付回调处理失败！！" + Result.Id + ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 小程序专业版商品付费
        /// </summary>
        /// <returns></returns>
        public bool MiniappEntGoods(int price = 1, EntGoodsOrder entGoodsOrder = null)
        {
            if (price > 0 && Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
            {
                WritePayException("支付失败");
                return false;
            }

            if (Order == null)
            {
                WritePayException("支付失败，订单不存在");
                return false;
            }

            List<EntGoodsOrder> orderList = null;
            if (entGoodsOrder != null)
            {
                orderList = new List<EntGoodsOrder>() { entGoodsOrder };
            }
            else
            {
                orderList = EntGoodsOrderBLL.SingleModel.GetList($"OrderId={Order.Id}");
            }

            //新订单电脑语音提示
            Utils.RemoveIsHaveNewOrder(orderList[0].aId);

            if (orderList != null && orderList.Count > 0)
            {
                foreach (Entity.MiniApp.Ent.EntGoodsOrder gOrder in orderList)
                {
                    gOrder.PayDate = DateTime.Now;
                    switch (gOrder.GetWay)
                    {
                        case (int)miniAppOrderGetWay.到店自取:
                        case (int)miniAppOrderGetWay.到店消费:
                            gOrder.State = (int)MiniAppEntOrderState.待自取;
                            //4位随机取物码
                            System.Random Random = new System.Random();
                            gOrder.TablesNo = Random.Next(0, 9999).ToString("0000");
                            break;
                        case (int)miniAppOrderGetWay.商家配送:
                            gOrder.State = (int)MiniAppEntOrderState.待发货;
                            break;
                    }

                    gOrder.GoodsGuid = Guid.NewGuid().ToString("N");
                    //_tranModel.Add(orderBll.BuildUpdateSql(gOrder, "State,GoodsGuid,PayDate,TablesNo", out _pone), _pone);
                    _tranModel.Add($"update  EntGoodsOrder set GoodsGuid='{gOrder.GoodsGuid}',State={gOrder.State},PayDate='{gOrder.PayDate.ToString("yyyy-MM-dd HH:mm:ss")}',TablesNo='{gOrder.TablesNo}'  where Id={gOrder.Id}");
                    //记录订单支付日志
                    _tranModel.Add(EntGoodsOrderLogBLL.SingleModel.BuildAddSql(new EntGoodsOrderLog() { GoodsOrderId = gOrder.Id, UserId = gOrder.UserId, LogInfo = $" 订单成功支付：{gOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));

                    //修改拼团状态
                    TransactionModel tran = new TransactionModel();
                    EntGroupSponsorBLL.SingleModel.PayReturnUpdateGroupState(gOrder.GroupId, gOrder.aId, ref tran);
                    if (tran.sqlArray != null && tran.sqlArray.Length > 0)
                    {
                        _tranModel.Add(tran.sqlArray);
                    }

                    //达达配送，修改订单状态为待接单
                    if (gOrder.GetWay == (int)miniAppOrderGetWay.达达配送)
                    {
                        TransactionModel dadatran = new TransactionModel();
                        string dadamsg = new DadaOrderBLL().GetDadaOrderUpdateSql(gOrder.Id, gOrder.aId, (int)TmpType.小程序专业模板, ref dadatran);
                        if (!string.IsNullOrEmpty(dadamsg))
                        {
                            LogHelper.WriteInfo(this.GetType(), dadamsg);
                            return false;
                        }
                        _tranModel.Add(dadatran.sqlArray);
                    }

                    //采用异步执行,报错也不会中断当前的sql执行
                    EntGoodsOrderBLL.SingleModel.AfterPayOrderBySaveMoney(gOrder);
                }
            }
            else
            {
                WritePayException("小程序专业版商品支付后处理失败：实体为空，orderid =" + Order.Id);
                return false;
            }

            if (!MyExecuteTransaction())
            {
                LogHelper.WriteError(GetType(), new Exception(" EntGoods支付回调处理失败！！" + Result.Id));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 小程序砍价支付
        /// </summary>
        /// <returns></returns>
        public bool MiniappBargainMoney()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  支付失败");
                    return false;
                }

                BargainUser BargainUser = BargainUserBLL.SingleModel.GetModel($"CityMordersId={Order.Id}");
                if (BargainUser != null)
                {
                    //新订单电脑语音提示
                    Utils.RemoveIsHaveNewOrder(BargainUser.aid);

                    BargainUser.State = 7;//支付成功后变成待发货
                    BargainUser.BuyTime = DateTime.Now;
                    BargainUser.BuyTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //log4net.LogHelper.WriteInfo(GetType(), "开始更新MiniappBargainUser");

                    bool updateBargainUser = BargainUserBLL.SingleModel.Update(BargainUser, "State,BuyTime,BuyTimeStr");
                    if (!updateBargainUser)
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception("小程序砍价MiniappBargainMoney更新BargainUser支付回调处理失败！！" + Result.Id));
                        return false;
                    }
                    if (!MyExecuteTransaction())
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception("小程序砍价MiniappBargainMoney更新citymoders支付回调处理失败！！" + Result.Id));
                        return false;
                    }

                    #region 发送 砍价订单支付成功通知 模板消息 => 通知用户
                    object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(BargainUser, SendTemplateMessageTypeEnum.砍价订单支付成功提醒);
                    TemplateMsg_Miniapp.SendTemplateMessage(BargainUser, SendTemplateMessageTypeEnum.砍价订单支付成功提醒, orderData);
                    #endregion

                    #region 发送模板消息通知商家
                    TemplateMsg_Gzh.SendBargainPaySuccessTemplateMessage(BargainUser);
                    #endregion

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("MiniappBargainMoney支付回调处理失败！！" + Result.Id + ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 小程序同城模板发布信息置顶支付
        /// </summary>
        /// <returns></returns>
        public bool cityBuyMsg()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  支付失败");
                    return false;
                }
                //   log4net.LogHelper.WriteInfo(this.GetType(), "cityBuyMsg同城发帖支付进来了"+JsonConvert.SerializeObject(Result));
                CityMsg city_Msg = CityMsgBLL.SingleModel.GetModel($"aid={Order.MinisnsId} and Id={Order.CommentId} and state=0");//拿到置顶消息 , 
                if (city_Msg != null)
                {
                    //  log4net.LogHelper.WriteInfo(this.GetType(),"同城发帖支付进来了");

                    //需要根据商家审核配置进行更新


                    CityStoreBanner storebanner = CityStoreBannerBLL.SingleModel.getModelByaid(Order.MinisnsId);
                    if (storebanner == null)
                    {
                        WritePayException("商家配置异常");
                        log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  同城小程序发帖商家配置异常");
                        return false;

                    }

                    switch (storebanner.ReviewSetting)
                    {
                        case 0://不需要审核
                            city_Msg.state = 1;//支付回成功调后再变更1
                            break;

                        case 1://先审核后发布
                               //支付回调不需要改变city_Msg的状态,在审核的时候改变
                            break;
                        case 2://先发布后审核
                            city_Msg.state = 1;//支付回成功调后再变更为1，审核不通过后变为0
                            break;

                    }



                    city_Msg.updateTime = DateTime.Now;
                    city_Msg.cityModerId = Order.Id;
                    _tranModel.Add(CityMsgBLL.SingleModel.BuildUpdateSql(city_Msg, "state,updateTime,cityModerId,PayState"));
                    if (!MyExecuteTransaction())
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception("小程序同城模板发布信息置顶支付cityBuyMsg更新citymoders以及city_Msg支付回调处理失败！！" + Result.Id));
                        return false;
                    }
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("cityBuyMsg支付回调处理失败！！" + Result.Id + ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 平台版小程序分类信息发帖支付回调
        /// </summary>
        /// <returns></returns>
        public bool PlatMsgPay()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  支付失败");
                    return false;
                }
                // log4net.LogHelper.WriteInfo(this.GetType(), "PlatMsgPay平台版发帖支付进来了"+JsonConvert.SerializeObject(Result));
                PlatMsg platMsg = PlatMsgBLL.SingleModel.GetModel($"aid={Order.MinisnsId} and Id={Order.CommentId} and state=0 and IsTop=1 and TopDay>0");//拿到置顶消息 , 
                if (platMsg != null)
                {
                    //  log4net.LogHelper.WriteInfo(this.GetType(),"平台版发帖支付进来了");

                    //需要根据商家审核配置进行更新


                    PlatMsgConf platMsgConf = PlatMsgConfBLL.SingleModel.GetMsgConf(Order.MinisnsId);
                    if (platMsgConf == null)
                    {
                        WritePayException("商家配置异常");
                        log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  平台版发帖商家配置异常");
                        return false;

                    }
                    switch (platMsgConf.ReviewSetting)
                    {
                        case 0://不需要审核
                        case 2://先发布后审核
                            platMsg.State = 1;
                            break;


                    }
                    platMsg.PayState = 1;



                    platMsg.UpdateTime = DateTime.Now;
                    platMsg.CityModerId = Order.Id;
                    _tranModel.Add(PlatMsgBLL.SingleModel.BuildUpdateSql(platMsg, "State,UpdateTime,CityModerId,PayState"));
                    if (!MyExecuteTransaction())
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception("平台版发帖置顶支付PlatMsgPay更新citymoders以及platMsg支付回调处理失败！！" + Result.Id));
                        return false;
                    }
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("cityBuyMsg支付回调处理失败！！" + Result.Id + ex.Message));
                return false;
            }
        }


        /// <summary>
        /// 平台版小程序店铺收费入驻支付回调
        /// </summary>
        /// <returns></returns>
        public bool PlatAddStorePay()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("平台版店铺入驻支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  平台版店铺入驻支付失败");
                    return false;
                }
                PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(Order.FuserId);
                if (platStore != null)
                {

                    platStore.State = 0;
                    platStore.UpdateTime = DateTime.Now;
                    _tranModel.Add(PlatStoreBLL.SingleModel.BuildUpdateSql(platStore, "State,UpdateTime"));
                    if (!MyExecuteTransaction())
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception("平台版店铺入驻支付PlatAddStorePay更新citymoders以及platStore支付回调处理失败！！" + Result.Id));
                        return false;
                    }
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("平台版店铺入驻支付PlatAddStorePay回调处理失败！！" + Result.Id + ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 平台版小程序店铺续期支付回调
        /// </summary>
        /// <returns></returns>
        public bool PlatStoreAddTimePay()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("平台版店铺续费支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  平台版店铺续费支付失败");
                    return false;
                }
                PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(Order.FuserId);
                if (platStore != null)
                {
                    platStore.YearCount += Order.CommentId;
                    platStore.UpdateTime = DateTime.Now;
                    platStore.CostPrice += Order.payment_free;
                    if (platStore.IsExpired)
                    {
                        platStore.AddTime = DateTime.Now;
                        _tranModel.Add(PlatStoreBLL.SingleModel.BuildUpdateSql(platStore, "AddTime,YearCount,UpdateTime,CostPrice"));
                    }
                    else
                    {
                        _tranModel.Add(PlatStoreBLL.SingleModel.BuildUpdateSql(platStore, "YearCount,UpdateTime,CostPrice"));

                    }



                    if (!MyExecuteTransaction())
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception("平台版店铺续费支付PlatAddStorePay更新citymoders以及platStore支付回调处理失败！！" + Result.Id));
                        return false;
                    }
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("平台版店铺入驻支付PlatAddStorePay回调处理失败！！" + Result.Id + ex.Message));
                return false;
            }
        }




        /// <summary>
        /// 支付订单回调错误日志写入
        /// </summary>
        /// <param name="msg"></param>
        public void WritePayException(string msg = null)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(msg));
            }
            log4net.LogHelper.WriteError(GetType(), new Exception(SerializeHelper.SerToJson(Result)));
        }

        private bool MyExecuteTransaction()
        {
            //修改Order
            if (!UpdateOrder())
            {
                return false;
            }

            //log4net.LogHelper.WriteInfo(this.GetType(), "拼团测试回调2："+JsonConvert.SerializeObject(_tranModel.sqlArray));

            //ExecuteTransactionDataCorect(_tranModel.sqlArray, _tranModel.ParameterArray)
            if (base.ExecuteTransaction(_tranModel.sqlArray, _tranModel.ParameterArray))
            {
                // log4net.LogHelper.WriteInfo(this.GetType(), "支付进来了");
                return true;
            }
            else
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(Newtonsoft.Json.JsonConvert.SerializeObject(_tranModel.sqlArray)));
                WritePayException("支付成功回调处理失败！");
                return false;
            }
        }

        private bool UpdateOrder()
        {
            if (Order == null)
            {
                WritePayException("支付之后根据订单ID找不到订单号");
                return false;
            }
            if (Order.payment_status == 1 || Order.Status == 1)
            {
                WritePayException("订单已经支付");
                return false;
            }
            _tranModel.Add($"update citymorders set confirm_time='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',payment_status=1,Status=1,trade_no='{Result.transaction_id}',payment_free={Result.total_fee} where id={Order.Id} and payment_status=0");
            return true;
        }

        public bool PayByStoredvalue(CouponLog coupon = null)
        {
            if (Order.OrderType == (int)ArticleTypeEnum.MiniappStoredvaluePay)
            {
                //储值支付
                _tranModel.Add(SaveMoneySetUserBLL.SingleModel.GetCommandCarPriceSql(Order.appid, Order.FuserId, Order.payment_free, -1, Order.Id, Order.orderno, $"使用储值付款：{Order.payment_free / 100f}").ToArray());
            }

            if (!VipRelationBLL.SingleModel.updatelevel(Order.FuserId, "entpro", Order.payment_free))
            {
                LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + Order.Id));
            }

            Order.payment_status = 1;
            Order.Status = 1;
            _tranModel.Add(BuildUpdateSql(Order, "payment_status,Status"));
            if (coupon != null)
            {
                coupon.State = 1;
                coupon.OrderId = Order.Id;
                coupon.PayType = 1;
                _tranModel.Add(CouponLogBLL.SingleModel.BuildUpdateSql(coupon, "State,OrderId,PayType"));
            }

            if (base.ExecuteTransaction(_tranModel.sqlArray, _tranModel.ParameterArray))
            {

            }
            else
            {
                LogHelper.WriteError(GetType(), new Exception("储值付款失败\r\n" + JsonConvert.SerializeObject(_tranModel.sqlArray)));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 积分兑换微信支付回调处理
        /// </summary>
        /// <returns></returns>
        public bool PayMiniappExchangeActivity()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException("积分兑换支付失败");
                    log4net.LogHelper.WriteInfo(GetType(), $" {Result.Id}  支付失败");
                    return false;
                }

                ExchangeActivityOrder exchangeActivityOrder = ExchangeActivityOrderBLL.SingleModel.GetModel($"CityMordersId={Order.Id}");
                if (exchangeActivityOrder != null)
                {
                    List<string> listSql = new List<string>();

                    #region 更改订单状态
                    exchangeActivityOrder.state = 2;//支付成功后变为待发货订单
                    exchangeActivityOrder.PayTime = DateTime.Now;
                    listSql.Add(ExchangeActivityOrderBLL.SingleModel.BuildUpdateSql(exchangeActivityOrder, "state,PayTime"));
                    #endregion

                    #region 减积分商品库存
                    ExchangeActivity exchangeActivity = ExchangeActivityBLL.SingleModel.GetModel($"Id={exchangeActivityOrder.ActivityId} and appId={exchangeActivityOrder.appId} and state=0 and isdel=0");
                    if (exchangeActivity == null)
                        log4net.LogHelper.WriteError(GetType(), new Exception("小程序积分兑换微信支付PayMiniappExchangeActivity更新citymoders支付回调处理失败(积分商品不存在)!" + Result.Id));

                    exchangeActivity.stock--;
                    listSql.Add(ExchangeActivityBLL.SingleModel.BuildUpdateSql(exchangeActivity, "stock"));
                    #endregion

                    #region 扣除用户积分操作
                    ExchangeUserIntegral exchangeUserIntegral = ExchangeUserIntegralBLL.SingleModel.GetModel($"userId={exchangeActivityOrder.UserId}");
                    exchangeUserIntegral.integral = exchangeUserIntegral.integral - exchangeActivityOrder.integral;
                    exchangeUserIntegral.UpdateDate = DateTime.Now;
                    listSql.Add(ExchangeUserIntegralBLL.SingleModel.BuildUpdateSql(exchangeUserIntegral, "integral,UpdateDate"));
                    #endregion


                    #region 插入积分兑换商品后扣除积分的日志 
                    ExchangeUserIntegralLog userIntegralLog = new ExchangeUserIntegralLog
                    {
                        ruleId = 0,
                        appId = exchangeActivityOrder.appId,
                        integral = -1,
                        price = 0,
                        ruleType = -1,
                        goodids = "-1",
                        orderId = exchangeActivityOrder.Id,
                        usegoodids = "-1",
                        userId = exchangeActivityOrder.UserId,
                        actiontype = -1,
                        curintegral = exchangeActivityOrder.integral,
                        AddTime = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        ordertype = 1,
                        buyPrice = exchangeActivityOrder.BuyPrice
                    };

                    listSql.Add(ExchangeUserIntegralLogBLL.SingleModel.BuildAddSql(userIntegralLog));
                    #endregion

                    if (listSql.Count > 0)
                    {

                        _tranModel.Add(listSql.ToArray());
                    }
                    if (!MyExecuteTransaction())
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception("小程序积分兑换微信支付PayMiniappExchangeActivity更新citymoders支付回调处理失败!" + Result.Id));
                        return false;
                    }

                    return true;
                }

                LogHelper.WriteError(GetType(), new Exception("小程序积分兑换微信支付PayMiniappExchangeActivity回调异常找不到订单!" + Result.Id));

                return false;

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("PayMiniappExchangeActivity支付回调处理发生异常！！" + Result.Id + ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 获取餐饮版支付订单
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="storeId"></param>
        /// <param name="foodOrderId"></param>
        /// <returns></returns>
        public CityMorders GetModelForFood(int userId, int storeId, int foodOrderId)
        {
            return GetModel($"OrderType = {(int)ArticleTypeEnum.MiniappFoodGoods} AND FuserId = {userId} AND MinisnsId = {storeId} AND TuserId = {foodOrderId}");
        }

        public List<CityMorders> GetCheckOutOrder(string appId, ref int count, int pageIndex = 1, int pageSize = 9999, string tradeNo = null, DateTime? start = null, DateTime? end = null)
        {
            string whereSql = $"appId = '{appId}' AND OrderType IN ({(int)ArticleTypeEnum.MiniappStoredvaluePay},{(int)ArticleTypeEnum.MiniappWXDirectPay})";
            if (!string.IsNullOrWhiteSpace(tradeNo))
            {
                whereSql = $"{whereSql} AND trade_no = '{tradeNo}'";
            }
            if (start.HasValue)
            {
                whereSql = $"{whereSql} AND payment_time >= '{start.Value.ToString()}'";

            }
            if (end.HasValue)
            {
                whereSql = $"{whereSql} AND payment_time <= '{end.Value.AddHours(23).AddMinutes(59).AddMinutes(59).ToString()}'";
            }
            count = GetCount(whereSql);
            return GetList(whereSql, pageSize, pageIndex, "*", "ID DESC");
        }

        /// <summary>
        /// 智慧餐厅支付回调
        /// </summary>
        /// <returns></returns>
        public bool PayDishOrder()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException($"智慧餐厅 {Result.Id}  支付失败");
                    return false;
                }
                string errMsg = string.Empty;//错误提示信息

                DishOrder dishOrder = DishOrderBLL.SingleModel.GetModel(Order.CommentId);
                if (dishOrder == null)
                {
                    WritePayException($"智慧餐厅{Result.Id} 支付回调处理失败,找不到订单");
                    return false;
                }
                dishOrder.pay_id = (int)DishEnums.PayMode.微信支付;
                dishOrder.pay_time = DateTime.Now;
                //支付成功修改订单状态
                if (!DishOrderBLL.SingleModel.PayOrderDisposeDB(dishOrder, ref errMsg, _tranModel))
                {
                    WritePayException(JsonConvert.SerializeObject(dishOrder));
                    WritePayException($"智慧餐厅{Result.Id} 支付回调处理失败,生成订单状态数据处理失败,提示信息{errMsg}");
                    return false;
                }

                //加收益
                if (!DishStoreEarningsBLL.SingleModel.AddStoreEarning(dishOrder.aId, dishOrder.storeId, dishOrder.settlement_total_fee, DishEnums.EarningsType.支付, $" 用户支付,订单号:{dishOrder.order_sn} ", ref errMsg, _tranModel))
                {
                    WritePayException($"智慧餐厅{Result.Id} 支付回调处理失败,生成收益记录处理失败:{errMsg}");
                    return false;
                }
                if (!MyExecuteTransaction())
                {
                    WritePayException($"智慧餐厅{Result.Id} 支付回调处理失败!" + Result.Id);
                    return false;
                }

                DishOrderBLL.SingleModel.AfterPayOrderOperation(dishOrder);
                return true;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("智慧餐厅支付回调处理失败！！" + Result.Id + ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 智慧餐厅支付回调
        /// </summary>
        /// <returns></returns>
        public bool PayDishStorePayTheBill()
        {
            try
            {
                if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
                {
                    WritePayException($"智慧餐厅{Result.Id} 门店买单支付失败");
                    return false;
                }

                string errMsg = string.Empty;


                //加收益
                if (!DishStoreEarningsBLL.SingleModel.AddStoreEarning(Order.MinisnsId, Order.CommentId, float.Parse($"{Order.payment_free * 0.01}"), DishEnums.EarningsType.支付, $" 用户门店买单,消费￥{Order.payment_free * 0.01}元,备注:{Order.remark} ", ref errMsg, _tranModel))
                {
                    WritePayException($"智慧餐厅{Result.Id} 支付回调处理失败,生成收益记录处理失败:{errMsg}");
                    return false;
                }
                if (!MyExecuteTransaction())
                {
                    WritePayException($"智慧餐厅{Result.Id} 门店买单支付回调处理失败!" + Result.Id);
                    return false;
                }
                dynamic obj = JsonConvert.DeserializeObject(Order.AttachPar);
                if (obj.activityId > 0)
                {
                    DishActivityUser quan = DishActivityUserBLL.SingleModel.GetModel((int)obj.activityId);
                    if (quan != null)
                    {
                        quan.quan_status = 1;
                        DishActivityUserBLL.SingleModel.Update(quan, "quan_status");
                    }
                }
                TemplateMsg_Gzh.SendOrderSuccessTemplateMessage(Order.Fusername, Order.payment_free * 0.01, Order.CommentId, Order.Addtime, "微信支付", (double)obj.price, (double)obj.discount);
                return true;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("智慧餐厅门店买单支付回调处理失败！！" + Result.Id + ex.Message));
                return false;
            }
        }

        public bool PayDishCardAccount()
        {
            if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
            {
                WritePayException($"智慧餐厅{Result.Id} 会员充值支付失败");
                return false;
            }
            string errMsg = string.Empty;


            DishCardAccountLog log = DishCardAccountLogBLL.SingleModel.GetModel(Order.Articleid);
            log.state = 1;
            double balance = 0.00;
            List<string> strs = log.account_info.Split('|').ToList();
            log.account_info = strs[0];
            double sumMoney = 0;//充值的总金额=支付金额+赠送金额
            string combinInfo = "";//充值信息
            if (!DishCardAccountLogBLL.SingleModel.Update(log, "state,account_info"))
            {
                WritePayException($"智慧餐厅{Order.Articleid}  会员充值支付修改状态失败");
                return false;
            }
            balance += log.account_money;
            sumMoney += log.account_money;
            combinInfo += log.account_info;
            if (strs.Count > 1)
            {
                DishCardAccountLog song = DishCardAccountLogBLL.SingleModel.GetModel(Convert.ToInt32(strs[1]));

                song.state = 1;
                if (!DishCardAccountLogBLL.SingleModel.Update(song, "state"))
                {
                    WritePayException($"智慧餐厅{Order.Articleid}  会员充值支付赠送金额修改状态失败");
                    return false;
                }
                balance += song.account_money;
                sumMoney += song.account_money;
                combinInfo += "，" + song.account_info;
            }
            DishVipCard card = DishVipCardBLL.SingleModel.GetModelByUid(log.shop_id, log.user_id);
            if (card == null)
            {
                WritePayException($"智慧餐厅{Order.Articleid}  会员充值支付找不到会员卡");
                return false;
            }

            card.account_balance += balance;
            if (!DishVipCardBLL.SingleModel.Update(card, "account_balance"))
            {
                WritePayException($"智慧餐厅{Order.Articleid} 支付回调处理失败,会员充值处理失败");
                return false;
            }
            //加收益
            if (!DishStoreEarningsBLL.SingleModel.AddStoreEarning(log.aId, log.shop_id, log.account_money, DishEnums.EarningsType.支付, $" 用户支付,内部订单号:{Order.Id} ", ref errMsg))
            {
                WritePayException($"智慧餐厅{Result.Id} 支付回调处理失败,生成收益记录处理失败:{errMsg}");
                return false;
            }
            try
            {
                DishStore store = DishStoreBLL.SingleModel.GetModel(log.shop_id);
                log.account_info = combinInfo;
                object curSortQueue_TemplateMsgObj = TemplateMsg_Miniapp.DishMessageData(store.dish_name, card, log, sumMoney, SendTemplateMessageTypeEnum.充值成功通知);
                TemplateMsg_Miniapp.SendTemplateMessage(log.user_id, SendTemplateMessageTypeEnum.充值成功通知, (int)TmpType.智慧餐厅, curSortQueue_TemplateMsgObj, $"pages/restaurant/restaurant-card/index?dish_id={log.shop_id}&savemoney=1");//
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), ex);
            }
            return true;
        }

        public bool PayContentCallBack()
        {
            if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
            {
                WritePayException($"微信支付失败<PayContentCallBack>");
                return false;
            }

            if (Order == null)
            {
                WritePayException("支付订单异常<PayContentCallBack>");
                return false;
            }
            PaidContentRecord updateRecord = PaidContentRecordBLL.SingleModel.GetModel(Order.TuserId);
            if (updateRecord == null)
            {
                WritePayException("付费内容订单异常<PayContentCallBack>");
                return false;
            }

            //更新支付记录事务
            PaidContentRecordBLL.SingleModel.UpdateToPaySql(updateRecord).ForEach((sql) =>
            {
                _tranModel.Add(sql);
            });

            //采用异步执行,报错也不会中断当前的sql执行
            if (!MyExecuteTransaction())
            {
                LogHelper.WriteError(GetType(), new Exception("EntGoods支付回调处理失败！！" + Result.Id));
                return false;
            }

            //执行成功回调
            Task.Factory.StartNew(() => { PaidContentRecordBLL.SingleModel.UpdateSuccessCallBack(updateRecord); });

            return true;
        }

        /// <summary>
        /// 检查订单状态
        /// </summary>
        /// <returns></returns>
        public bool CheckOrderState(CityMorders model, ref string msg)
        {
            if (model == null)
            {
                msg = "无效对象";
                return false;
            }

            switch (model.OrderType)
            {
                case (int)ArticleTypeEnum.MiniappEnt:
                    EntGoodsOrder entOrder = EntGoodsOrderBLL.SingleModel.GetModelByOrderId(model.MinisnsId, model.Id);
                    if (entOrder.State == (int)MiniAppEntOrderState.已删除 || entOrder.State == (int)MiniAppEntOrderState.已取消)
                    {
                        msg = "订单已失效，请刷新重试";
                        return false;
                    }
                    break;
            }

            return true;
        }
        /// <summary>
        /// 专业版预约表单支付回调
        /// </summary>
        /// <returns></returns>
        public bool EntSubscribeFormPay()
        {
            if (Result.result_code != "SUCCESS") //支付成功，支付失败不用改变
            {
                WritePayException("支付失败");
                return false;
            }

            if (Order == null)
            {
                WritePayException("支付失败，订单不存在");
                return false;
            }

            EntGoodsOrder goodsOrder = EntGoodsOrderBLL.SingleModel.GetModel($"OrderId={Order.Id}");
            if (goodsOrder == null)
            {
                WritePayException("专业版预约表单支付处理失败：实体为空，orderid =" + Order.Id);
                return false;
            }
            goodsOrder.PayDate = DateTime.Now;
            goodsOrder.State = (int)MiniAppEntOrderState.交易成功;

            goodsOrder.GoodsGuid = Guid.NewGuid().ToString("N");
            _tranModel.Add($"update  EntGoodsOrder set GoodsGuid='{goodsOrder.GoodsGuid}',State={goodsOrder.State},PayDate='{goodsOrder.PayDate.ToString("yyyy-MM-dd HH:mm:ss")}'  where Id={goodsOrder.Id}");
            //记录订单支付日志
            _tranModel.Add(EntGoodsOrderLogBLL.SingleModel.BuildAddSql(new EntGoodsOrderLog() { GoodsOrderId = goodsOrder.Id, UserId = goodsOrder.UserId, LogInfo = $" 预约付费订单成功支付：{goodsOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));

            if (!MyExecuteTransaction())
            {
                LogHelper.WriteError(GetType(), new Exception(" 专业版预约表单支付回调处理失败！！" + Result.Id));
                return false;
            }
            return true;
        }


    }
}
