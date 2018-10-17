using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Fds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.MiniApp.Fds
{

    public class FoodGoodsCartBLL : BaseMySql<FoodGoodsCart>
    {
        #region 单例模式
        private static FoodGoodsCartBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodGoodsCartBLL()
        {

        }

        public static FoodGoodsCartBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodGoodsCartBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public static readonly string cacheKeyList = "MiniappFoodGoodsCart_{0}";

        #region 基础操作
        public List<FoodGoodsCart> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<FoodGoodsCart>();

            return base.GetList($" Id in ({ids}) ");
        }
        public List<FoodGoodsCart> GetListByIds(string ids,int userid,int state)
        {
            return base.GetList($" Id in({string.Join(",", ids)}) and UserId = {userid} and state = {state} ");
        }
        public List<FoodGoodsCart> GetListByOrderIds(string orderids)
        {
            return GetList($" GoodsOrderId in ({orderids}) ");
        }
        #endregion

        /// <summary>
        /// 商品/多规格商品 更新商品时,根据规格,应更新购物车内记录状态
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="specids"></param>
        /// <param name="newGoodState"> 0 正常   1 已下架    2 已删除 </param>
        /// <param name="oldGoodState"> 默认查询所有商品状态的记录 </param>
        /// <param name="returnSql"> 是否返回sql而不执行(用于拼接入tranModel语句) </param>
        public List<string> UpdateCartGoodsStateByGoodsIdSpecids(int gid, string specids, int newGoodState, int oldGoodState = 10, bool returnSql = true)
        {
            var list = GetList($" FoodGoodsId={gid} and IFNULL(Specids,'')='{specids}' and (ISNULL(GoodsOrderId) or GoodsOrderId<=0) and State=0 { (oldGoodState == 10 ? "" : $"and goodsState = {oldGoodState}") }");
            var returnSqlList = new List<string>();
            foreach (var item in list)
            {
                item.GoodsState = newGoodState;
                if (!returnSql)
                {
                    Update(item, "GoodsState");
                }
                else
                {
                    returnSqlList.Add(BuildUpdateSql(item, "GoodsState"));
                }
            }
            return returnSqlList;
        }

        /// <summary>
        /// 商品/多规格商品 上下架/删除时,应更新购物车内记录状态
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="specids"></param>
        /// <param name="newGoodState"> 0 正常   1 已下架    2 已删除 </param>
        /// <param name="oldGoodState"> 默认查询所有商品状态的记录 </param>
        /// <param name="returnSql"> 是否返回sql而不执行(用于拼接入tranModel语句) </param>
        public List<string> UpdateCartGoodsStateByGoodsId(int gid, int newGoodState, int oldGoodState = 10, bool returnSql = true)
        {
            var list = GetList($" FoodGoodsId={gid} and (ISNULL(GoodsOrderId) or GoodsOrderId<=0) and State=0 { (oldGoodState == 10 ? "" : $"and goodsState = {oldGoodState}") }");
            var returnSqlList = new List<string>();
            foreach (var item in list)
            {
                item.GoodsState = newGoodState;
                if (!returnSql)
                {
                    Update(item, "GoodsState");
                }
                else
                {
                    returnSqlList.Add(BuildUpdateSql(item, "GoodsState"));
                }
            }
            return returnSqlList;
        }

        //餐饮商品更新的时候，更新购物车里面有效餐饮商品的价格
        public void UpdateCartByGoodsId(int gid, string specids, int price)
        {
            var list = GetList($" FoodGoodsId={gid} and IFNULL(SpecIds,'')='{specids}' and ISNULL(GoodsOrderId) and GoodsOrderId<=0 and State=0");
            foreach (var item in list)
            {
                item.Price = price;
                Update(item, "Price");
            }
        }

        public void UpdateCartGoodsInfo(ref TransactionModel tran, List<FoodGoodsCart> shopCartItem)
        {
            var goodsId = string.Join(",", shopCartItem.Select(item => item.FoodGoodsId));
            var goods = FoodGoodsBLL.SingleModel.GetList($"Id in({goodsId})");
            foreach(var item in shopCartItem)
            {
                var good = goods.FirstOrDefault(thisGood => thisGood.Id == item.FoodGoodsId);
                if(good == null) { continue; }
                item.GoodName = good.GoodsName;
                item.GoodImg = good.ImgUrl;
                tran.Add(BuildUpdateSql(item, "GoodName,GoodImg"));
            }
        }

        /// <summary>
        /// 从缓存读取配置 ,如果没有，new 一个默认的对象
        /// </summary>
        /// <param>
        /// </param>
        /// <returns></returns>
        public List<FoodGoodsCart> GetMyCart(int FoodId, int UserId)
        {
            string key = string.Format(cacheKeyList, UserId);
            List<FoodGoodsCart> CartList = null/*= RedisUtil.Get<List<MiniappFoodGoodsCart>>(key)*/;
            if (CartList == null)
            {
                CartList = GetList(BuildWhereSql(foodId: FoodId, userId: UserId));
                RedisUtil.Set(key, CartList, TimeSpan.FromDays(1));
            }
            return CartList;
        }

        public List<FoodGoodsCart> GetReserveFromCart(int foodId,int userId,string orderIds = null,int state = 0)
        {
            var whereSql = BuildWhereSql(foodId: foodId, userId: userId, orderIds: orderIds, shopCartType: miniAppShopCartType.预定商品, state: state);
            return GetList(whereSql);
        }

        public string BuildWhereSql(int foodId = 0,int userId = 0,string orderIds = null, miniAppShopCartType shopCartType = 0, int state = 0)
        {
            var whereSql = $"State={state} AND Type = {(int)shopCartType}";
            if(foodId > 0)
            {
                whereSql = $"{whereSql} AND FoodId = {foodId}";
            }
            if(userId > 0)
            {
                whereSql = $"{whereSql} AND UserId = {userId}";
            }
            if(!string.IsNullOrWhiteSpace(orderIds))
            {
                whereSql = $"{whereSql} AND GoodsOrderId in({orderIds})";
            }
            return whereSql;
        }

        public void RemoveCache(int UserId)
        {
            RedisUtil.Remove(string.Format(cacheKeyList, UserId));
        }

        public override object Add(FoodGoodsCart model)
        {
            object o = base.Add(model);
            RemoveCache(model.UserId);
            return o;
        }

        public override bool Update(FoodGoodsCart model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            RemoveCache(model.UserId);
            return b;
        }

        public override bool Update(FoodGoodsCart model)
        {
            bool b = base.Update(model);
            RemoveCache(model.UserId);
            return b;
        }

        public override string BuildUpdateSql(FoodGoodsCart model)
        {
            var b = base.BuildUpdateSql(model);
            RemoveCache(model.UserId);
            return b;
        }

        public override string BuildUpdateSql(FoodGoodsCart model,string fldsList)
        {
            var b = base.BuildUpdateSql(model, fldsList);
            RemoveCache(model.UserId);
            return b;
        }


        /// <summary>
        /// 根据订单Id获取订单详情
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<FoodGoodsCart> GetOrderDetail(int orderId)
        {
            var cartModelList = GetList($" GoodsOrderId = {orderId} ") ?? new List<FoodGoodsCart>();
            if (cartModelList != null && cartModelList.Any())
            {
                var goodList = FoodGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", cartModelList.Select(x => x.FoodGoodsId).Distinct())}) ");
                if (goodList != null && cartModelList.Any())
                {
                    cartModelList.ForEach(x =>
                    {
                        x.goodsMsg = goodList.Where(y => y.Id == x.FoodGoodsId).FirstOrDefault();
                    });
                }
            }
            return cartModelList;
        }
    }
}
