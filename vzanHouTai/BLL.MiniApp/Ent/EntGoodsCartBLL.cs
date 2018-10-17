using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Ent
{

    public class EntGoodsCartBLL : BaseMySql<EntGoodsCart>
    {
        #region 单例模式
        private static EntGoodsCartBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntGoodsCartBLL()
        {

        }

        public static EntGoodsCartBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntGoodsCartBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private string cacheKeyList = "MiniappFoodGoodsCart_{0}";

        public List<EntGoodsCart> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<EntGoodsCart>();

            return base.GetList($"id in ({ids})");
        }

        public List<EntGoodsCart> GetListByOrderIds(string orderids)
        {
            if (string.IsNullOrEmpty(orderids))
                return new List<EntGoodsCart>();

            List<EntGoodsCart> list= GetList($" GoodsOrderId in ({orderids}) ");
            string goodsIds = string.Join(",",list?.Select(s=>s.FoodGoodsId).Distinct());
            List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);
            
            list.ForEach(x=>{
                if (string.IsNullOrEmpty(x.GoodName))
                {
                    EntGoods entGoods= entGoodsList?.FirstOrDefault(f=>f.id==x.FoodGoodsId);
                    if (entGoods != null)
                    {
                        x.GoodName = entGoods.name;
                    }
                }
            });
            return list;
        }

        public EntGoodsCart GetModelByGoodsId(int orderid,int goodsid)
        {
            return base.GetModel($" GoodsOrderId ={orderid} and FoodGoodsId={goodsid}");
        }


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
            List<EntGoodsCart> list = GetList($" FoodGoodsId={gid} and IFNULL(Specids,'')='{specids}' and (ISNULL(GoodsOrderId) or GoodsOrderId<=0) and State=0 { (oldGoodState == 10 ? "" : $"and goodsState = {oldGoodState}") }");
            List<string> returnSqlList = new List<string>();
            foreach (EntGoodsCart item in list)
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


        public List<EntGoodsCart> GetMyCartById(string ids, int? state = 0)
        {
            return state.HasValue ? GetList($" Id in ({ids}) and state = {state.Value}") : GetList($" Id in ({ids})");
        }
        /// <summary>
        /// 商品/多规格商品 上下架/删除时,应更新购物车内记录状态
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="specids"></param>
        /// <param name="newGoodState"> 0 正常   1 已下架    2 已删除 </param>
        /// <param name="oldGoodState"> 默认查询所有商品状态的记录 </param>
        /// <param name="returnSql"> 是否返回sql而不执行(用于拼接入tranModel语句) </param>
        public List<string> UpdateCartGoodsStateByGoodsId(int gid, int newGoodState, int oldGoodState = 10, bool returnSql = true, int storeId = 0)
        {
            List<EntGoodsCart> list = GetList($" FoodGoodsId={gid} and (ISNULL(GoodsOrderId) or GoodsOrderId<=0) and State=0 { (oldGoodState == 10 ? "" : $" and goodsState = {oldGoodState}")}{(storeId <= 0 ? "" : $" and storeId={storeId}")}");
            List<string> returnSqlList = new List<string>();
            foreach (EntGoodsCart item in list)
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
            List<EntGoodsCart> list = GetList($" FoodGoodsId={gid} and SpecIds='{specids}' and ISNULL(GoodsOrderId) and GoodsOrderId<=0 and State=0");
            foreach (EntGoodsCart item in list)
            {
                item.Price = price;
                Update(item, "Price");
            }
        }



        /// <summary>
        /// 从缓存读取配置 ,如果没有，new 一个默认的对象
        /// </summary>
        /// <param>
        /// </param>
        /// <returns></returns>
        public List<EntGoodsCart> GetMyCart(int aId, int UserId)
        {
            string key = string.Format(cacheKeyList, UserId);
            List<EntGoodsCart> CartList = GetList($" aId = {aId} and UserId={UserId} and State=0 and gotobuy=0");
            return CartList;
        }

        /// <summary>
        /// 从缓存读取配置 ,如果没有，new 一个默认的对象
        /// </summary>
        /// <param>
        /// </param>
        /// <returns></returns>
        public List<EntGoodsCart> GetMyCart(int aId, int UserId, int pageIndex, int pageSize)
        {
            string key = string.Format(cacheKeyList, UserId);
            List<EntGoodsCart> Carts = GetList($" aId = {aId} and UserId={UserId} and State=0", pageSize, pageIndex);
            return Carts;
        }

        public List<EntGoodsCart> GetMyCart(int aId, int UserId, int type)
        {
            string key = string.Format(cacheKeyList, UserId);
            List<EntGoodsCart> Carts = GetList($" aId = {aId} and UserId={UserId} and State=0 and gotobuy=0 and Type = {type}");
            return Carts;
        }

        public void RemoveCache(int UserId)
        {
            RedisUtil.Remove(string.Format(cacheKeyList, UserId));
        }
        public override object Add(EntGoodsCart model)
        {
            object o = base.Add(model);
            return o;
        }
        public override bool Update(EntGoodsCart model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            RemoveCache(model.UserId);
            return b;
        }
        public override bool Update(EntGoodsCart model)
        {
            bool b = base.Update(model);
            RemoveCache(model.UserId);
            return b;
        }

        /// <summary>
        /// 获取订单详情从购物车里
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<EntGoodsCart> GetOrderDetail(int orderId)
        {
            List<EntGoodsCart> cartModelList = GetList($" GoodsOrderId = {orderId} ") ?? new List<EntGoodsCart>();
            try
            {
                if (cartModelList != null && cartModelList.Any())
                {
                    List<EntGoods> goodList = EntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", cartModelList.Select(x => x.FoodGoodsId).Distinct())}) ");
                    if (goodList != null && cartModelList.Any())
                    {
                        cartModelList.ForEach(x =>
                        {
                            x.goodsMsg = goodList.Where(y => y.id == x.FoodGoodsId).FirstOrDefault();
                        });
                    }
                }


            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
            return cartModelList == null ? new List<EntGoodsCart>() : cartModelList;

        }
        /// <summary>
        /// 足浴版根据订单号获取订单明细
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EntGoodsCart GetModelByGoodsOrderId(int id, int state = 1)
        {
            if (state < 1)
            {
                return GetModel($"GoodsOrderId={id}");
            }
            else//获取已提交的订单明细
            {
                return GetModel($"GoodsOrderId={id} and state=1");
            }
        }

        public List<EntGoodsCart> GetListByGoodsOrderId(int goodsOrderId)
        {
            return GetList($"GoodsOrderId={goodsOrderId}");
        }
        
        public bool DeleteByIds(string ids)
        {
            bool result = false;
            if (string.IsNullOrWhiteSpace(ids))
            {
                return result;
            }
            string sql = $" update entgoodsCart set state=-1 where id in ({ids})";
            result = SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql) > 0;
            return result;
        }

        public List<int> AddGoodsCartList(List<EntGoodsCart> goodsCartList)
        {
            List<int> result = null;
            if (goodsCartList == null || goodsCartList.Count <= 0)
            {
                return result;
            }
            List<int> ids = new List<int>();
            foreach (EntGoodsCart goodsCart in goodsCartList)
            {
                ids.Add(Convert.ToInt32(Add(goodsCart)));
            }
            if (ids.Any(id => id <= 0))
            {
                string idsStr = string.Join(",", ids.Select(id => id > 0));
                string sql = $"update entgoodscart set state=-1 where id in({idsStr})";
                SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql);
                result = null;
            }
            result = ids;
            return result;
        }

        /// <summary>
        /// 获取已确认收货并未评价的购物车数据，每次获取前1000条
        /// </summary>
        /// <returns></returns>
        public List<EntGoodsCart> GetSuccessDataList(int iscomment=0,int day=-15)
        {
            List<EntGoodsCart> list = new List<EntGoodsCart>();
            string sqlwhere = "";
            //1:已评论,0:未评论
            if(iscomment>=0)
            {
                sqlwhere = $" and c.iscommentting={iscomment} ";
            }
            string sql = $"select c.* from entgoodscart c right JOIN entgoodsorder o on c.GoodsOrderId=o.id where o.State={(int)MiniAppEntOrderState.交易成功} and o.AcceptDate<='{DateTime.Now.AddDays(day)}' {sqlwhere} LIMIT 100";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                list = base.GetList(dr);
            }

            return list;
        }

        public void UpdateDataList(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return;
            base.ExecuteNonQuery($"update EntGoodsCart set iscommentting=0 where id in ({ids})");
        }
    }
}
