using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Qiye;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Qiye;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BLL.MiniApp.Qiye
{
    public class QiyeGoodsOrderBLL : BaseMySql<QiyeGoodsOrder>
    {
        #region 单例模式
        private static QiyeGoodsOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeGoodsOrderBLL()
        {

        }

        public static QiyeGoodsOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeGoodsOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        #region 基础操作
        public QiyeGoodsOrder GetModelByOrderId(int cityOrderId)
        {
            return base.GetModel($"orderid ={cityOrderId}");
        }

        public QiyeGoodsOrder GetModelByOrderNum(string orderNum)
        {
            return base.GetModel($"ordernum ='{orderNum}'");
        }

        public QiyeGoodsOrder GetModelByUserId(int aid, int userId, int id)
        {
            string sqlWhere = $"id = {id} and aid={aid} and userid={userId}";
            return base.GetModel(sqlWhere);
        }

        /// <summary>
        /// 获取超过时限待付款的订单
        /// </summary>
        /// <returns></returns>
        public List<QiyeGoodsOrder> GetListByNoPayOutTimeOrder(int state, DateTime outTime, int pageSize, int pageIndex)
        {
            string sqlWhere = $"state={(int)QiyeOrderState.待付款} and addtime <= '{outTime}'";
            return base.GetList(sqlWhere, pageSize, pageIndex);
        }

        /// <summary>
        /// 退款失败或退款中的订单
        /// </summary>
        /// <returns></returns>
        public List<QiyeGoodsOrder> GetListByReturnOrder(int pageSize, int pageIndex)
        {
            return base.GetList($" State in ({(int)QiyeOrderState.退款中},{(int)QiyeOrderState.退款失败}) and RefundTime <= (NOW()-interval 17 second) and BuyMode = {(int)miniAppBuyMode.微信支付} ", pageSize, pageIndex);
        }
        
        #endregion

        /// <summary>
        /// 统计客户下单数量和下单总金额
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public List<QiyeGoodsOrder> GetOrderResult(int aid,string userIds)
        {
            List<QiyeGoodsOrder> list = new List<QiyeGoodsOrder>();
            if (string.IsNullOrEmpty(userIds))
                return list;

            if (aid<=0)
                return list;

            string sql = $"select userid,Count(*) count,sum(buyprice) sumprice from qiyegoodsorder where aid = {aid} and userid in ({userIds}) and state in ({(int)QiyeOrderState.已完成},{(int)QiyeOrderState.退款成功}) GROUP BY userid ";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    QiyeGoodsOrder model = base.GetModel(dr);
                    if(dr["sumprice"] !=DBNull.Value)
                    {
                        model.BuyPrice = Convert.ToInt32(dr["sumprice"]);
                    }
                    if (dr["count"] != DBNull.Value)
                    {
                        model.QtyCount = Convert.ToInt32(dr["count"]);
                    }

                    list.Add(model);
                }
            }

            return list;
        }

        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
        /// <param name="order"></param>
        /// <returns></returns>
        public string AddOrder(int userId, string jsonData, ref TransactionModel tran, ref object orderObj, int orderType)
        {
            string msg = "";
            //基本验证
            QiyeGoodsOrder postData = new QiyeGoodsOrder();
            C_UserInfo userInfo = new C_UserInfo();
            List<QiyeGoodsCart> cartList = new List<QiyeGoodsCart>();
            XcxAppAccountRelation xcxRelation = new XcxAppAccountRelation();
            WxAddress address = new WxAddress();
            QiyeStore store = new QiyeStore();
            CheckBaseData(jsonData, userId, ref postData, ref userInfo, ref cartList, ref xcxRelation, ref address,ref store,ref msg, orderType);
            if (msg.Length > 0)
                return msg;

            //检测库存
            CheckGoods(cartList, ref msg);
            if (msg.Length > 0)
                return msg;

            //优惠价格处理
            int afterDiscountPrice = 0;
            int beforeDiscountPrice = 0;
            int vipDiscountPrice = 0;
            int couponPrice = 0;
            DiscountPrice(postData.CouponLogId, ref cartList, userInfo.Id, ref beforeDiscountPrice, ref afterDiscountPrice, ref vipDiscountPrice, ref couponPrice, ref msg);
            if (msg.Length > 0)
                return msg;
            if (afterDiscountPrice > 999999999)
                return "商品价格有误";

            //运费
            int friPrice = GetFreight(store, cartList, postData.GetWay, postData.CartIds, store.Aid, address, ref msg);
            if (msg.Length > 0)
                return msg;

            //订单
            QiyeGoodsOrder order = new QiyeGoodsOrder();
            order.AccepterName = postData.AccepterName;
            order.AccepterTelePhone = postData.AccepterTelePhone;
            order.ZipCode = address.postalCode;
            order.Address = postData.Address;
            order.Message = postData.Message;
            order.TemplateType = (int)TmpType.企业智推版;
            order.StoreId = store.Id;
            order.UserId = userInfo.Id;
            order.AddTime = DateTime.Now;
            order.AId = xcxRelation.Id;
            order.QtyCount = cartList.Sum(x => x.Count);
            order.BuyMode = postData.BuyMode;
            order.GetWay = postData.GetWay;
            order.AppId = userInfo.appId;
            order.SumPrice = beforeDiscountPrice;
            order.VipReducedPrice = vipDiscountPrice;
            order.ReducedPrice = beforeDiscountPrice - afterDiscountPrice;
            order.FreightPrice = friPrice;
            order.CouponPrice = couponPrice;
            order.BuyPrice = afterDiscountPrice + friPrice;
            order.BuyPrice = order.BuyPrice <= 0 ? 0 : order.BuyPrice;
            order.OrderNum = CommonCore.CreateOrderNum(cartList[0].Id.ToString());

            if (order.BuyMode == (int)miniAppBuyMode.微信支付)
            {
                order.State = (int)QiyeOrderState.待付款;
                switch (order.GetWay)
                {
                    case (int)miniAppOrderGetWay.到店自取:
                        //4位随机取物码
                        System.Random Random = new System.Random();
                        order.LadingCode = Random.Next(0, 9999).ToString("0000");
                        break;
                }
            }
            else if (order.BuyMode == (int)miniAppBuyMode.储值支付)
            {
                order.PayTime = DateTime.Now;
                switch (order.GetWay)
                {
                    case (int)miniAppOrderGetWay.到店自取:
                        order.State = (int)QiyeOrderState.待自取;
                        //4位随机取物码
                        System.Random Random = new System.Random();
                        order.LadingCode = Random.Next(0, 9999).ToString("0000");
                        break;
                    case (int)miniAppOrderGetWay.商家配送:
                        order.State = (int)QiyeOrderState.待发货;
                        break;
                }
            }

            //新增订单sql
            tran.Add(Utils.BuildAddSqlS(order, "OrderId", "(select LAST_INSERT_ID())", base.TableName, base.arrProperty));
            //修改优惠券状态
            if (postData.CouponLogId > 0)
            {
                tran.Add($" update CouponLog set OrderId = (select last_insert_id()),State = 1 where id ={postData.CouponLogId}");
            }
            //tran.Add(base.BuildAddSql(order));
            //修改购物车订单
            foreach (QiyeGoodsCart cartitem in cartList)
            {
                tran.Add($" update QiyeGoodsCart set OrderId = (select last_insert_id()),State = 1,Discount = {cartitem.Discount},price={cartitem.Price},OriginalPrice={cartitem.OriginalPrice} where id ={cartitem.Id} and state = 0; ");
            }

            //修改库存
            UpdateGoodsStock(cartList, ref tran, ref msg, true);
            orderObj = order;
            return msg;
        }

        /// <summary>
        /// 微信支付
        /// </summary>
        /// <param name="orderObj"></param>
        /// <param name="cityMorder"></param>
        /// <returns></returns>
        public object PayOrder(object orderObj, CityMorders cityMorder, TransactionModel tran, ref int orderId)
        {
            CityMordersBLL cityMordersBLL = new CityMordersBLL();
            QiyeGoodsOrder order = (QiyeGoodsOrder)orderObj;
            cityMorder.payment_free = order.BuyPrice;
            if (tran == null || tran.sqlArray.Count() <= 0)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "独立模板生成订单失败，sql为空");
                return "";
            }
            TransactionModel tranModel = new TransactionModel();
            tranModel.Add(cityMordersBLL.BuildAddSql(cityMorder));
            foreach (string sqlitem in tran.sqlArray)
            {
                tranModel.Add(sqlitem);
            }
            if (!ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "独立模板生成订单失败" + JsonConvert.SerializeObject(tran));
                return "";
            }

            QiyeStore store = QiyeStoreBLL.SingleModel.GetModel(order.StoreId);
            if (store != null)
            {
                //清除商品缓存
                QiyeGoodsBLL.SingleModel.RemoveEntGoodListCache(store.Aid);
            }

            order = GetModelByOrderNum(order.OrderNum);
            if (order == null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), $"独立模板生成订单失败，获取不到生成的订单【{order.OrderNum}】");
                return "";
            }
            if (order.OrderId <= 0)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "独立模板生成订单OrderId失败" + JsonConvert.SerializeObject(tranModel));
                return "";
            }
            cityMorder.Id = order.OrderId;
            orderId = order.OrderId;
            //为0不需进入生成微信预支付订单的流程（免费订单）
            if (order.BuyPrice == 0)
            {
                PayResult payresult = new PayResult();
                new CityMordersBLL(payresult, cityMorder).QiyePayOrder(0, order);
                return new { orderid = order.Id, dbOrder = order.Id };
            }
            else //生成微信预支付订单
            {
                return new { orderid = order.OrderId, dbOrder = order.Id };
            }
        }

        /// <summary>
        /// 储值支付
        /// </summary>
        /// <param name="orderObj"></param>
        /// <param name="order"></param>
        /// <param name="aid"></param>
        /// <param name="saveMoneyUser"></param>
        /// <param name="sbUpdateGoodCartSql"></param>
        /// <param name="reductionCart"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object PayOrderByChuzhi(object orderObj, int aid, SaveMoneySetUser saveMoneyUser, TransactionModel tran, ref int orderId)
        {

            QiyeGoodsOrder order = (QiyeGoodsOrder)orderObj;
            //储值支付 扣除预存款金额并生成消费记录
            //添加储值日志
            SaveMoneySetUserLog userLog = new SaveMoneySetUserLog()
            {
                AppId = saveMoneyUser.AppId,
                UserId = order.UserId,
                MoneySetUserId = saveMoneyUser.Id,
                Type = -1,
                BeforeMoney = saveMoneyUser.AccountMoney,
                AfterMoney = saveMoneyUser.AccountMoney - order.BuyPrice,
                ChangeMoney = order.BuyPrice,
                ChangeNote = $" 购买商品,订单号:{order.OrderNum} ",
                CreateDate = DateTime.Now,
                State = 1,
                AId = order.AId,
            };
            tran.Add(Utils.BuildAddSqlS(userLog, "OrderId", "(select LAST_INSERT_ID())", SaveMoneySetUserLogBLL.SingleModel.TableName, SaveMoneySetUserLogBLL.SingleModel.arrProperty));

            //储值扣费
            tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney - {order.BuyPrice} where id =  {saveMoneyUser.Id} ; ");

            if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "储值支付：操作失败" + Newtonsoft.Json.JsonConvert.SerializeObject(tran));
                return "";
            }

            QiyeStore store = QiyeStoreBLL.SingleModel.GetModel(order.StoreId);
            if (store != null)
            {
                //清除商品缓存
                QiyeGoodsBLL.SingleModel.RemoveEntGoodListCache(store.Aid);
            }
            orderId = 1;
            
            order = GetModelByOrderNum(order.OrderNum);
            if (order == null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "储值支付：生成订单失败");
                return "";
            }

            //发送支付成功通知给用户
            object orderData = TemplateMsg_Miniapp.QiyeGetTemplateMessageData(order, SendTemplateMessageTypeEnum.企业智推版订单支付成功通知);
            TemplateMsg_Miniapp.SendTemplateMessage(order.UserId, SendTemplateMessageTypeEnum.企业智推版订单支付成功通知, TmpType.企业智推版, orderData);

            //发送支付成功公众号通知给商家
            TemplateMsg_Gzh.SendQiyePaySuccessTemplateMessage(order);

            return new { postdata = order.OrderNum, orderid = 0, dbOrder = order.Id };
        }

        /// <summary>
        /// 基本验证
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="wxaddressjson"></param>
        /// <param name="userInfo"></param>
        /// <param name="xcxRelation"></param>
        /// <param name="address"></param>
        /// <param name="msg"></param>
        private void CheckBaseData(string jsonData, int userId, ref QiyeGoodsOrder postData, ref C_UserInfo userInfo, ref List<QiyeGoodsCart> cartList, ref XcxAppAccountRelation xcxRelation, ref WxAddress address, ref QiyeStore store, ref string msg, int orderType)
        {
            postData = JsonConvert.DeserializeObject<QiyeGoodsOrder>(jsonData);
            if (postData == null)
            {
                msg = "基本验证：订单请求数据不能为空";
                return;
            }
            userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                msg = "基本验证：无效用户";
                return;
            }
            xcxRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(userInfo.appId);
            if (xcxRelation == null)
            {
                msg = "基本验证：无效模板";
                return;
            }
            store = QiyeStoreBLL.SingleModel.GetModelByAId(xcxRelation.Id);
            if (store == null)
            {
                msg = $"基本验证：无效店铺";
                return;
            }
            if (postData.GetWay == (int)miniAppOrderGetWay.到店自取 )
            {
                if(store.SwitchConfigModel != null && !store.SwitchConfigModel.SwitchReceiving)
                {
                    msg = $"基本验证：店铺暂不支持到店自取";
                    return;
                }
            }

            if (string.IsNullOrEmpty(postData.CartIds))
            {
                msg = "基本验证：无效购物车数据";
                return;
            }
            List<string> goodCarIdList = postData.CartIds.Split(',').ToList();
            if (postData.CartIds.Substring(postData.CartIds.Length - 1, 1) == ",")
            {
                postData.CartIds = postData.CartIds.Substring(0, postData.CartIds.Length - 1);
            }

            cartList = QiyeGoodsCartBLL.SingleModel.GetListByIds(postData.CartIds);
            if (cartList == null || cartList.Count <= 0)
            {
                msg = "基本验证：找不到购物车记录";
                return;
            }

            //地址验证
            if (string.IsNullOrWhiteSpace(postData.WxAddresJson))
            {
                msg = "基本验证：无效地址";
                return;
            }
            address = JsonConvert.DeserializeObject<WxAddress>(postData.WxAddresJson);
            if (address == null)
            {
                msg = "基本验证：地址信息不存在";
                return;
            }

            if (string.IsNullOrWhiteSpace(postData.AccepterName))
            {
                msg = "基本验证：未输入收/提货人姓名";
                return;
            }
            if (string.IsNullOrWhiteSpace(postData.AccepterTelePhone))
            {
                msg = "基本验证：未输入收/提货电话或格式不正确";
                return;
            }
        }

        /// <summary>
        /// 检验商品库存
        /// </summary>
        /// <param name="cartList"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool CheckGoods(List<QiyeGoodsCart> cartList, ref string msg)
        {
            //判定是否存在失效商品
            QiyeGoods good = null;
            string goodsId = string.Join(",", cartList.Select(s => s.GoodsId).Distinct());
            List<QiyeGoods> goodslist = QiyeGoodsBLL.SingleModel.GetListByIds(goodsId);
            if (goodslist == null || goodslist.Count <= 0)
            {
                msg = "商品：找不到商品";
                return false;
            }
            foreach (QiyeGoodsCart c in cartList)
            {
                good = goodslist.FirstOrDefault(f => f.Id == c.GoodsId);
                if (good == null)
                {
                    msg = "商品：购物车商品失效";
                    return false;
                }

                if (c.State != 0)
                {
                    msg = "商品：购物车商品已失效";
                    return false;
                }
                //检查当前商品库存是否足够
                if (good.StockLimit)
                {
                    int curGoodQty = 0;
                    if (string.IsNullOrWhiteSpace(c.SpecIds))
                    {
                        curGoodQty = good.Stock;
                    }
                    else
                    {
                        List<GoodsSpecDetail> goodList = good.GASDetailList.Where(x => x.Id.Equals(c.SpecIds)).ToList();
                        if (goodList == null || goodList.Count <= 0)
                        {
                            msg = $"商品: {good.Name} 库存不足!";
                            return false;
                        }
                        curGoodQty = goodList[0].Stock;
                    }
                    if (curGoodQty < c.Count)
                    {
                        msg = $"商品: {good.Name} 库存不足，请重新选购！";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 计算运费
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        private int GetFreight(QiyeStore store, List<QiyeGoodsCart> cartList, int getWay, string goodCarIdStr, int aid, WxAddress address, ref string msg)
        {
            int friPrice = 0;
            QiyeStoreSwitchModel config = JsonConvert.DeserializeObject<QiyeStoreSwitchModel>(store.SwitchConfig);

            int qtySum = cartList.Sum(x => x.Count);
            switch (getWay)
            {
                case (int)miniAppOrderGetWay.到店自取:
                    break;
                case (int)miniAppOrderGetWay.商家配送:
                    DeliveryFeeResult deliueryResult = DeliveryTemplateBLL.SingleModel.GetQiyeFee(goodCarIdStr, aid, address.provinceName, address.cityName, ref msg);
                    if (msg.Length > 0)
                    {
                        return 0;
                    }

                    friPrice = deliueryResult.Fee;
                    break;
            }

            return friPrice;
        }

        /// <summary>
        /// 优惠金额处理
        /// </summary>
        /// <param name="couponLogId">用户领取优惠券id</param>
        /// <param name="cartList">购物车</param>
        /// <param name="userId">用户ID</param>
        /// <param name="beforeDiscountPrice">优惠前商品总价</param>
        /// <param name="afterDiscountPrice">优惠后商品总价</param>
        /// <param name="vipDiscountPrice">会员优惠价</param>
        /// <param name="couponPrice">优惠券优惠价格</param>
        public void DiscountPrice(int couponLogId, ref List<QiyeGoodsCart> cartList, int userId, ref int beforeDiscountPrice, ref int afterDiscountPrice, ref int vipDiscountPrice, ref int couponPrice, ref string msg)
        {
            beforeDiscountPrice = cartList.Sum(x => x.Price * x.Count);

            #region 会员打折
            cartList.ForEach(g => g.OriginalPrice = g.Price);
            //获取会员信息
            VipRelation vipInfo = VipRelationBLL.SingleModel.GetModelByUserid(userId);
            VipLevel levelInfo = vipInfo != null ? VipLevelBLL.SingleModel.GetModel(vipInfo.levelid) : null;
            VipLevelBLL.SingleModel.GetVipDiscount(ref cartList, vipInfo, levelInfo, userId, "Discount", "Price");
            #endregion 会员打折

            //折后总价
            afterDiscountPrice = cartList.Sum(x => x.Price * x.Count);
            //会员优惠金额
            vipDiscountPrice = beforeDiscountPrice - afterDiscountPrice;

            //优惠金额
            couponPrice = CouponLogBLL.SingleModel.GetCouponPrice<QiyeGoodsCart>(couponLogId, cartList, "GoodsId", "Price", "Count", ref msg);

            afterDiscountPrice = afterDiscountPrice - couponPrice;
            afterDiscountPrice = afterDiscountPrice < 0 ? 0 : afterDiscountPrice;
        }

        /// <summary>
        /// 修改商品库存
        /// </summary>
        /// <param name="cartList">购物车</param>
        /// <param name="userInfo"></param>
        /// <param name="tran"></param>
        /// <param name="msg"></param>
        /// <param name="getSql">是否获取sql</param>
        /// <param name="type">是否加库存</param>
        public void UpdateGoodsStock(List<QiyeGoodsCart> cartList, ref TransactionModel tran, ref string msg, bool getSql = false, bool type = false)
        {
            if (cartList == null || cartList.Count <= 0)
            {
                msg = "库存：购物车数据为空";
                return;
            }

            string goodsId = string.Join(",", cartList.Select(s => s.GoodsId));
            //根据订单内记录数量减库存,加销量
            List<QiyeGoods> goodsList = QiyeGoodsBLL.SingleModel.GetListByIds(goodsId);
            if (goodsList == null || goodsList.Count <= 0)
            {
                msg = "库存：商品已过期";
                return;
            }

            Utility.Easyui.EasyuiHelper<GoodsSpecDetail> goodDtlJsonHelper = new Utility.Easyui.EasyuiHelper<GoodsSpecDetail>();
            foreach (QiyeGoodsCart cartitem in cartList)
            {
                QiyeGoods goods = goodsList.FirstOrDefault(f => f.Id == cartitem.GoodsId);
                goods.SalesCount = type ? goods.SalesCount - cartitem.Count : goods.SalesCount + cartitem.Count;
                goods.SalesCount = goods.SalesCount < 0 ? 0 : goods.SalesCount;
                if (goods.StockLimit) //限制库存时才去操作库存
                {
                    if (string.IsNullOrWhiteSpace(cartitem.SpecIds))
                    {
                        goods.Stock = type ? goods.Stock + cartitem.Count : goods.Stock - cartitem.Count;
                    }
                    else
                    {
                        goods.Stock = type ? goods.Stock + cartitem.Count : goods.Stock - cartitem.Count;
                        List<GoodsSpecDetail> entGoodsAttrDtls = new List<GoodsSpecDetail>();
                        goods.GASDetailList.ForEach(y =>
                        {
                            if (y.Id.Equals(cartitem.SpecIds))
                            {
                                y.Stock = type ? y.Stock + cartitem.Count : y.Stock - cartitem.Count;
                            }
                            entGoodsAttrDtls.Add(y);
                        });
                        //规格库存详情重新赋值
                        goods.SpecDetail = goodDtlJsonHelper.SToJsonArray(entGoodsAttrDtls);
                    }
                }
                //更新商品库存
                tran.Add(QiyeGoodsBLL.SingleModel.BuildUpdateSql(goods, $"Stock,SpecDetail,SalesCount"));
            }

            //是否执行
            if (!getSql)
            {
                if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
                {
                    msg = "库存：修改库存失败";
                }
            }

            return;
        }

        /// <summary>
        /// 订单接口：获取订单列表
        /// </summary>
        /// <param name="state"></param>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<QiyeGoodsOrder> GetList_Api(int state, int aid, int userId, int pageSize, int pageIndex, ref int count)
        {
            string sqlwhere = $"aid={aid} and userid={userId}";
            if (state != -999)
            {
                sqlwhere += $" and state={state}";
            }

            List<QiyeGoodsOrder> list = base.GetList(sqlwhere, pageSize, pageIndex, "", "id desc");
            count = base.GetCount(sqlwhere);

            if (list != null && list.Any())
            {
                string orderids = string.Join(",", list.Select(s => s.Id));
                List<QiyeGoodsCart> cartlist = QiyeGoodsCartBLL.SingleModel.GetListByOrderIds(orderids);
                if (cartlist == null || cartlist.Count <= 0)
                {
                    return list;
                }
                
                foreach (QiyeGoodsOrder item in list)
                {
                    item.CartList = cartlist.Where(w => w.OrderId == item.Id).ToList();
                }
            }

            return list;
        }

        /// <summary>
        /// 订单接口：获取订单详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public QiyeGoodsOrder GetModel_Api(int id)
        {
            QiyeGoodsOrder order = base.GetModel(id);
            if (order != null)
            {
                List<QiyeGoodsCart> cartlist = QiyeGoodsCartBLL.SingleModel.GetListByOrderIds(order.Id.ToString());
                if (cartlist == null || cartlist.Count <= 0)
                {
                    return order;
                }
                order.CartList = cartlist;
            }

            return order;
        }

        /// <summary>
        /// 后台获取订单列表
        /// </summary>
        /// <returns></returns>
        public List<QiyeGoodsOrder> GetDataList(string accepterTelephone, string accepterName, string ladingCode, string orderNum, int getWay, int state, int aid, int pageSize, int pageIndex, ref int count)
        {
            List<MySqlParameter> parms = new List<MySqlParameter>();
            string sqlWhere = $"aid={aid}";
            if (state != -999)
            {
                sqlWhere += $" and state={state}";
            }
            if (getWay != -1)
            {
                sqlWhere += $" and getway={getWay}";
            }
            if (!string.IsNullOrEmpty(accepterName))
            {
                sqlWhere += $" and acceptername like @acceptername";
                parms.Add(new MySqlParameter("@acceptername", $"%{accepterName}%"));
            }
            if (!string.IsNullOrEmpty(accepterTelephone))
            {
                sqlWhere += $" and acceptertelephone like @acceptertelephone";
                parms.Add(new MySqlParameter("@acceptertelephone", $"%{accepterTelephone}%"));
            }
            if (!string.IsNullOrEmpty(ladingCode))
            {
                sqlWhere += $" and ladingcode=@ladingcode";
                parms.Add(new MySqlParameter("@ladingcode", $"{ladingCode}"));
            }
            if (!string.IsNullOrEmpty(orderNum))
            {
                sqlWhere += $" and ordernum like @ordernum";
                parms.Add(new MySqlParameter("@ordernum", $"%{orderNum}%"));
            }

            List<QiyeGoodsOrder> list = base.GetListByParam(sqlWhere, parms.ToArray(), pageSize, pageIndex, "", "id desc");
            count = base.GetCount(sqlWhere, parms.ToArray());
            if (list != null && list.Any())
            {
                string orderids = string.Join(",", list.Select(s => s.Id));
                List<QiyeGoodsCart> cartList = QiyeGoodsCartBLL.SingleModel.GetListByOrderIds(orderids);
                if (cartList == null || cartList.Count <= 0)
                {
                    return list;
                }

                foreach (QiyeGoodsOrder item in list)
                {
                    item.CartList = cartList.Where(w => w.OrderId == item.Id).ToList();
                }
            }
            return list;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msg"></param>
        public void CancelOrder(QiyeGoodsOrder order, ref string msg)
        {
            if (order == null)
            {
                msg = "取消订单：找不到订单";
                return;
            }
            order.State = (int)QiyeOrderState.已取消;
            TransactionModel tranModel = new TransactionModel();
            tranModel.Add($"update QiyeGoodsOrder set state={order.State} where id={order.Id}");

            //订单明细
            List<QiyeGoodsCart> cartlist = QiyeGoodsCartBLL.SingleModel.GetListByOrderIds(order.Id.ToString());
            if (cartlist == null || cartlist.Count <= 0)
            {
                msg = "取消订单：找不到订单明细";
                return;
            }

            //更改库存
            UpdateGoodsStock(cartlist, ref tranModel, ref msg, true, true);
            if (msg.Length > 0)
                return;
            if (tranModel == null || tranModel.sqlArray.Count() <= 0)
            {
                msg = "取消订单：无效执行";
                return;
            }

            if (ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
            {
                //清除商品缓存
                QiyeGoodsBLL.SingleModel.RemoveEntGoodListCache(order.AId);
                //发给用户取消通知
                object orderData = TemplateMsg_Miniapp.QiyeGetTemplateMessageData(order, SendTemplateMessageTypeEnum.企业智推版订单取消通知);
                TemplateMsg_Miniapp.SendTemplateMessage(order.UserId, SendTemplateMessageTypeEnum.企业智推版订单取消通知, TmpType.企业智推版, orderData);
            }
            else
            {
                msg = "取消订单：操作失败";
                return;
            }
        }

        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msg"></param>
        public void SendGoods(QiyeGoodsOrder order, string attachData, ref string msg)
        {
            bool success = false;
            if (string.IsNullOrWhiteSpace(attachData))
            {
                msg = "发货：物流信息不能为空";
                return;
            }
            order.State = (int)QiyeOrderState.待收货;
            order.SendTime = DateTime.Now;
            //保存物流信息
            DeliveryUpdatePost deliveryInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<DeliveryUpdatePost>(attachData);
            bool isCompleteInfo = (deliveryInfo.SelfDelivery || (!string.IsNullOrWhiteSpace(deliveryInfo.CompanyCode) && !string.IsNullOrWhiteSpace(deliveryInfo.DeliveryNo)))
                                  && !string.IsNullOrWhiteSpace(deliveryInfo.ContactName)
                                  && !string.IsNullOrWhiteSpace(deliveryInfo.ContactTel)
                                  && !string.IsNullOrWhiteSpace(deliveryInfo.Address);
            //物流配送
            if (!deliveryInfo.SelfDelivery)
            {
                if (string.IsNullOrWhiteSpace(deliveryInfo.CompanyCode) || string.IsNullOrWhiteSpace(deliveryInfo.DeliveryNo) || string.IsNullOrWhiteSpace(deliveryInfo.ContactName) || string.IsNullOrWhiteSpace(deliveryInfo.ContactTel) || string.IsNullOrWhiteSpace(deliveryInfo.Address))
                {
                    msg = "发货：物流信息不完整";
                    return;
                }
                success = DeliveryFeedbackBLL.SingleModel.AddPlatOrderFeed(order.Id, deliveryInfo) && base.Update(order, "state,SendTime");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(deliveryInfo.ContactName) || string.IsNullOrWhiteSpace(deliveryInfo.ContactTel) || string.IsNullOrWhiteSpace(deliveryInfo.Address))
                {
                    msg = "发货：商家自配信息不完整";
                    return;
                }

                order.AccepterName = deliveryInfo.ContactName;
                order.AccepterTelePhone = deliveryInfo.ContactTel;
                order.Address = deliveryInfo.Address;
                order.Remark = deliveryInfo.Remark;
                success = base.Update(order, "state,SendTime,AccepterName,AccepterTelePhone,Address,Remark");
            }

            if (success)
            {
                //发给用户取消通知
                object orderData = TemplateMsg_Miniapp.QiyeGetTemplateMessageData(order, SendTemplateMessageTypeEnum.企业智推版订单发货提醒);
                TemplateMsg_Miniapp.SendTemplateMessage(order.UserId, SendTemplateMessageTypeEnum.企业智推版订单发货提醒, TmpType.企业智推版, orderData);
            }
            else
            {
                msg = "发货：操作失败";
            }
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        public void ReceiptGoods(QiyeGoodsOrder order, ref string msg)
        {
            TransactionModel tran = new TransactionModel();
            order.State = (int)QiyeOrderState.已完成;
            order.AcceptTime = DateTime.Now;
            tran.Add(base.BuildUpdateSql(order, "State,AcceptTime"));
            //会员加消费金额
            if (!VipRelationBLL.SingleModel.updatelevel(order.UserId, "qiye", order.BuyPrice))
            {
                log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常(订单发货后 超过10天,系统自动完成订单)" + order.Id));
            }

            if (!base.ExecuteTransactionDataCorect(tran.sqlArray))
            {
                msg = "收货：操作失败";
                return;
            }
        }

        /// <summary>
        ///  退款
        /// </summary>
        /// <param name="item"></param>
        /// <param name="oldState"></param>
        /// <param name="BuyMode">默认微信支付</param>
        /// <returns></returns>
        public void ReturnOrder(QiyeGoodsOrder order, ref string msg)
        {
            TransactionModel tranModel = new TransactionModel();
            order.State = (int)QiyeOrderState.退款成功;
            order.RefundTime = DateTime.Now;

            if (order.BuyMode == (int)miniAppBuyMode.微信支付)
            {
                if (order.BuyPrice > 0)
                {
                    order.State = (int)QiyeOrderState.退款中;
                    CityMorders citymorder = new CityMordersBLL().GetModel(order.OrderId);
                    if (citymorder == null)
                    {
                        msg = "退款：退款数据繁忙";
                        return;
                    }
                    //微信支付
                    ReFundQueue reModel = new ReFundQueue
                    {
                        minisnsId = -5,
                        money = citymorder.payment_free,
                        orderid = citymorder.Id,
                        traid = citymorder.trade_no,
                        addtime = DateTime.Now,
                        note = "企业智推版小程序退款",
                        retype = 1
                    };
                    tranModel.Add(new ReFundQueueBLL().BuildAddSql(reModel));
                }
            }
            else if (order.BuyMode == (int)miniAppBuyMode.储值支付)
            {
                SaveMoneySetUserBLL.SingleModel.ReturnPrice(order.AppId, order.AId, order.UserId, TmpType.小未平台子模版, order.BuyPrice, order.OrderNum, ref tranModel, ref msg, true);

                if (msg.Length > 0)
                    return;
            }
            else
            {
                msg = "退款：无效支付类型";
                return;
            }
            tranModel.Add($"update QiyeGoodsOrder set state={order.State},RefundTime='{order.RefundTime}' where id={order.Id}");
            //订单明细
            List<QiyeGoodsCart> cartList = QiyeGoodsCartBLL.SingleModel.GetListByOrderIds($"'{order.Id}'");
            if (cartList == null || cartList.Count <= 0)
            {
                msg = "退款：找不到订单明细";
                return;
            }
            //更改库存
            UpdateGoodsStock(cartList, ref tranModel, ref msg, true, true);
            if (msg.Length > 0)
                return;

            if (!ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
            {
                msg = "退款：操作失败";
            }

            //清除商品缓存
            QiyeGoodsBLL.SingleModel.RemoveEntGoodListCache(order.AId);
        }

        /// <summary>
        ///  重新退款
        /// </summary>
        /// <param name="item"></param>
        /// <param name="oldState"></param>
        /// <param name="BuyMode">默认微信支付</param>
        /// <returns></returns>
        public void ReturnOrderAgain(QiyeGoodsOrder order, ref string msg)
        {
            if (order.State != (int)QiyeOrderState.退款失败)
            {
                msg = "该订单已在退款中";
                return;
            }
            TransactionModel tranModel = new TransactionModel();
            ReFundQueueBLL reFundQueueBLL = new ReFundQueueBLL();
            order.State = (int)QiyeOrderState.退款成功;
            order.RefundTime = DateTime.Now;

            if (order.BuyMode == (int)miniAppBuyMode.微信支付)
            {
                if (order.BuyPrice > 0)
                {
                    order.State = (int)QiyeOrderState.退款中;

                    ReFundQueue reFundQueue = reFundQueueBLL.GetModelByOrderId(order.OrderId);
                    if (reFundQueue == null)
                    {
                        msg = "退款：退款队列数据不存在";
                        return;
                    }

                    reFundQueue.state = 0;
                    tranModel.Add(reFundQueueBLL.BuildUpdateSql(reFundQueue, "state"));
                }
            }
            else if (order.BuyMode == (int)miniAppBuyMode.储值支付)
            {
                SaveMoneySetUserBLL.SingleModel.ReturnPrice(order.AppId, order.AId, order.UserId, TmpType.企业智推版, order.BuyPrice, order.OrderNum, ref tranModel, ref msg, true);

                if (msg.Length > 0)
                    return;
            }
            else
            {
                msg = "退款：无效支付类型";
                return;
            }
            tranModel.Add($"update QiyeGoodsOrder set state={order.State},RefundTime='{order.RefundTime}' where id={order.Id}");
            if (!ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
            {
                msg = "退款：操作失败";
            }
        }

        /// <summary>
        /// 取消订单服务
        /// </summary>
        /// <param name="timeLength">1小时候自动取消待付款状态订单</param>
        public void StartCancelServer(int timeLength)
        {
            int state = (int)QiyeOrderState.待付款;
            DateTime outTime = DateTime.Now.AddHours(timeLength);
            List<QiyeGoodsOrder> orderList = GetListByNoPayOutTimeOrder(state, outTime, 500, 1);
            if (orderList == null || orderList.Count <= 0)
                return;

            string msg = "";
            foreach (QiyeGoodsOrder item in orderList)
            {
                //取消订单
                CancelOrder(item, ref msg);
                if (msg.Length > 0)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "订单取消服务：" + msg);
                    item.State = (int)QiyeOrderState.操作失败;
                    base.Update(item, "state");
                }
            }
        }

        /// <summary>
        /// 确认收货服务
        /// </summary>
        /// <param name="timeLength">7天过后自动确认收货</param>
        public void StartReceiptGoodsServer(int timeLength)
        {
            int state = (int)QiyeOrderState.待收货;
            DateTime outTime = DateTime.Now.AddDays(timeLength);
            List<QiyeGoodsOrder> orderList = GetListByNoPayOutTimeOrder(state, outTime, 500, 1);
            if (orderList == null || orderList.Count <= 0)
                return;

            string msg = "";
            foreach (QiyeGoodsOrder item in orderList)
            {
                //自动确认收货
                ReceiptGoods(item, ref msg);
                if (msg.Length > 0)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "订单确认收货服务：" + msg);
                    item.State = (int)QiyeOrderState.操作失败;
                    base.Update(item, "state");
                }
            }
        }

        /// <summary>
        /// 跟进 退款状态 (退款是否成功)
        /// </summary>
        /// <returns></returns>
        public void StartOutOrderStateServer()
        {
            TransactionModel tranModel = new TransactionModel();
            string sql = $@"select eo.*,r.result_code as refundcode from QiyeGoodsOrder eo left join citymorders co on eo.orderid = co.id left join ReFundResult r on r.transaction_id = co.trade_no where eo.State in ({(int)QiyeOrderState.退款中}) and eo.RefundTime <= (NOW() - interval 17 second) and eo.BuyMode = 1";//and r.result_code = 'SUCCESS'  ,{(int)QiyeOrderState.退款失败}
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    QiyeGoodsOrder model = base.GetModel(dr);
                    if (dr["refundcode"].ToString() == "SUCCESS")
                    {
                        model.State = (int)QiyeOrderState.退款成功;
                        if (base.Update(model, "state"))
                        {
                            //发给用户退款成功通知
                            object orderData = TemplateMsg_Miniapp.QiyeGetTemplateMessageData(model, SendTemplateMessageTypeEnum.企业智推版订单退款通知);
                            TemplateMsg_Miniapp.SendTemplateMessage(model.UserId, SendTemplateMessageTypeEnum.企业智推版订单退款通知, TmpType.企业智推版, orderData);
                        }
                    }
                    else
                    {
                        model.State = (int)QiyeOrderState.退款失败;
                        base.Update(model, "state");
                    }
                }
            }
        }
    }
}