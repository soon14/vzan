using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Fds;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BLL.MiniApp.Fds
{
    public class FoodGoodsBLL : BaseMySql<FoodGoods>
    {
        #region 单例模式
        private static FoodGoodsBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodGoodsBLL()
        {

        }

        public static FoodGoodsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodGoodsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static readonly bool? isTrue = true;//辅助字段,匹配是否为ture

        /// <summary>
        /// 餐饮版产品列表缓存{餐饮版店铺Id} 
        /// </summary>
        public static readonly string foodGoodsKey = "foodGoods_{0}";


        #region 基础查询

        //删除商品多规格关系表
        public int DelAttrList(int foodgoodsid)
        {
            string sql = $"delete from foodgoodsattrspec where foodgoodsid={foodgoodsid}";
            int result = SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);
            return result;
        }

        public int GetListCount(int foodid,int state)
        {
            return base.GetCount($" foodId = {foodid} and state={state} and IsSell = 1 ");
        }

        public List<FoodGoods> GetPageList(int foodid,int state, int pageSize, int pageIndex, string selectparam, string orderstr)
        {
            return base.GetList($" foodId = {foodid} and state={state} and IsSell = 1 ", pageSize, pageIndex, selectparam, orderstr);
        }
        public List<FoodGoods> GetListByIds(string ids)
        {
            if(string.IsNullOrEmpty(ids))
            {
                return new List<FoodGoods>();
            }
            return base.GetList($"id in ({ids})");
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="typeid"></param>
        /// <param name="goodName"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="orderbyid"></param>
        /// <param name="shopType"> 0 堂食 1外卖</param>
        /// <returns></returns>
        public List<FoodGoods> GetListByParam(int sid, int typeid = 0, string goodName = "", int pageindex = 1, int pagesize = 10, int orderbyid = 0, int shopType = 10,int goodtype=(int)EntGoodsType.普通产品)
        {
            #region 有缓存的写法
            List<FoodGoods> list = GetListFoods(sid, goodName, null, null,goodtype);
            if (list != null)
            {
                list = list.Where(x => x.IsSell == 1).ToList();

                if (typeid > 0)
                {
                    list = list.Where(x => x.TypeId?.Split(',')?.Any(tid => tid.Equals(typeid.ToString())) == isTrue).ToList();
                }

                if (shopType == 0)
                {
                    list = list.Where(x => x.openTheShop == 1).ToList();
                }
                else if (shopType == 1)
                {
                    list = list.Where(x => x.openTakeOut == 1).ToList();
                }


                list = list.OrderByDescending(x => x.Id).OrderByDescending(x => x.Sort).ToList();
                switch (orderbyid)
                {
                    case 1:
                        list = list.OrderByDescending(x => x.Price).ToList();
                        break;
                    case 2:
                        list = list.OrderBy(x => x.Price).ToList();
                        break;
                    case 3:
                        list = list.OrderByDescending(x => x.Inventory).ToList();
                        break;
                    case 4:
                        list = list.OrderBy(x => x.Inventory).ToList();
                        break;
                    case 5:
                        list = list.OrderByDescending(x => x.BrowseCount).ToList();
                        break;
                }
                list = list.Skip((pageindex - 1) * pagesize).Take(pagesize).ToList();

            }
            else
            {
                list = new List<FoodGoods>();
            }

            return list;
            #endregion

            #region 没有缓存的写法

            //string where = $" FoodId={sid} and State>=0 and IsSell = 1 ";
            //if (typeid > 0)
            //{
            //    where += $" and TypeId = {typeid}";
            //}
            //goodName = goodName.Replace("'", "");//去掉 ' 字符,以防拼接错误
            //if (!string.IsNullOrWhiteSpace(goodName))
            //{
            //    where += $" and GoodsName like '%{goodName}%'";
            //}
            //if (shopType == 0)
            //{
            //    where += $" and  openTheShop = 1 ";
            //}
            //else if (shopType == 1)
            //{
            //    where += $" and  openTakeOut = 1 ";
            //}

            //string orderstr = "Id Desc";
            //switch (orderbyid)
            //{
            //    case 0:
            //        orderstr = "Id Desc";
            //        break;
            //    case 1:
            //        orderstr = "Price Desc";
            //        break;
            //    case 2:
            //        orderstr = "Price asc";
            //        break;
            //    case 3:
            //        orderstr = "Inventory Desc";
            //        break;
            //    case 4:
            //        orderstr = "Inventory asc";
            //        break;
            //    case 5:
            //        orderstr = "BrowseCount Desc";
            //        break;
            //}
            //var info = GetList(where, pagesize, pageindex, "*", orderstr);
            //return info; 
            #endregion


        }
        
        /// <summary>
        /// 查询当前商品库存
        /// </summary>
        /// <param name="goodsid"></param>
        /// <param name="attrSpacStr"></param>
        /// <returns></returns>
        public int GetGoodQty(FoodGoods good, string attrSpacStr = "")
        {
            var goodQty = 0;
            if (good == null)
            {
                return 0;
            }
            if (string.IsNullOrWhiteSpace(attrSpacStr))
            {
                goodQty = good.Stock;
            }
            else
            {
                var goodBySpacStr = good.GASDetailList.Where(x => x.id.Equals(attrSpacStr))?.First();
                if (goodBySpacStr != null)
                {
                    goodQty = goodBySpacStr.count;
                }
            }
            return goodQty;
        }



        public List<FoodGoods> getFindPageList(int foodid, string goodname = "", int[] labelids = null, int[] type = null, int pageIndex = 1, int pageSize = 10)
        {
            #region 没有缓存的写法
            //string whereStr = $" foodid = {foodid} and state >= 0 ";
            //if (!string.IsNullOrWhiteSpace(goodname))
            //{
            //    whereStr += $" and GoodsName = '{goodname}' ";
            //}
            //if (labelids != null && labelids.Any())
            //{
            //    var labelwhere = "(";
            //    for (int i = 0; i < labelids.Length; i++)
            //    {
            //        if (i > 0)
            //        {
            //            labelwhere += " or ";
            //        }
            //        labelwhere += $" labelIdstr Like '%,{labelids[i]},%' ";
            //    }
            //    labelwhere += ") ";
            //    whereStr += $" and  {labelwhere} ";
            //}
            //if (type != null && type.Any())
            //{
            //    var typewhere = "(";
            //    for (int i = 0; i < type.Length; i++)
            //    {
            //        if (i > 0)
            //        {
            //            typewhere += " or ";
            //        }
            //        typewhere += $" typeid = {type[i]} ";
            //    }
            //    typewhere += ") ";
            //    whereStr += $" and  {typewhere} ";
            //}
            //var findList = GetList(whereStr, pageSize, pageIndex, "*", " Id Desc ") ?? new List<FoodGoods>();
            //return findList;
            #endregion

            #region 有缓存的写法
            List<FoodGoods> list = GetListFoods(foodid, goodname, labelids, type);
            if (list != null)
                list = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            else
                list = new List<FoodGoods>();

            return list;
            #endregion

        }

        public List<FoodGoods> GetListFoods(int foodid, string goodname = "", int[] labelids = null, int[] type = null,int goodtype=(int)EntGoodsType.普通产品)
        {
            string key = string.Format(foodGoodsKey, foodid);//缓存的key  与餐饮版店铺Id对应
            List<FoodGoods> listFoodGoods = RedisUtil.Get<List<FoodGoods>>(key);//取出该店铺下所有的产品
          //  listFoodGoods = null;
            string whereStr = $" foodid = {foodid} and state >= 0 and goodtype={goodtype}";
            if (goodtype == (int)EntGoodsType.拼团产品)
            {
                whereStr = $" foodid = {foodid} and state > 0 and goodtype={goodtype}";
                listFoodGoods = GetList(whereStr);
            }
            else if (listFoodGoods == null)
            {
                listFoodGoods = GetList(whereStr);
                RedisUtil.Set<List<FoodGoods>>(key, listFoodGoods);
            }

            if (!string.IsNullOrWhiteSpace(goodname))
            {
                listFoodGoods = listFoodGoods.Where(x => x.GoodsName.Contains(goodname)).ToList();
            }
            if (labelids != null && labelids.Length > 0)
            {
                listFoodGoods = listFoodGoods.Where(x => !string.IsNullOrWhiteSpace(x.labelIdStr) && x.labelIdStr.Split(',')?.Any(tid => !string.IsNullOrWhiteSpace(tid) && labelids.Contains(Convert.ToInt32(tid))) == isTrue).ToList();
            }
            if (type != null && type.Length > 0)
            {
                listFoodGoods = listFoodGoods.Where(x => !string.IsNullOrWhiteSpace(x.TypeId) && x.TypeId.Split(',')?.Any(tid => !string.IsNullOrWhiteSpace(tid) && type.Contains(Convert.ToInt32(tid))) == isTrue).ToList();
            }
            
            return listFoodGoods;
        }


        public int getFindPageListCount(int foodid, string goodname = "", int[] labelids = null, int[] type = null)
        {

            #region 没有缓存的写法
            //string whereStr = $" foodid = {foodid} and state >= 0 ";
            //if (!string.IsNullOrWhiteSpace(goodname))
            //{
            //    whereStr += $" and GoodsName = '{goodname}'";
            //}
            //if (labelids != null && labelids.Any())
            //{
            //    var labelwhere = "(";
            //    for (int i = 0; i < labelids.Length; i++)
            //    {
            //        if (i > 0)
            //        {
            //            labelwhere += " or ";
            //        }
            //        labelwhere += $" labelIdstr Like '%,{labelids[i]},%' ";
            //    }
            //    labelwhere += ") ";
            //    whereStr += $" and  {labelwhere} ";
            //}
            //if (type != null && type.Any())
            //{
            //    var typewhere = "(";
            //    for (int i = 0; i < type.Length; i++)
            //    {
            //        if (i > 0)
            //        {
            //            typewhere += " or ";
            //        }
            //        typewhere += $" typeid = {type[i]} ";
            //    }
            //    typewhere += ") ";
            //    whereStr += $" and  {typewhere} ";
            //}
            //var findList = GetCount(whereStr);
            //return findList;
            #endregion

            #region 有缓存的写法
            int count = 0;
            List<FoodGoods> list = GetListFoods(foodid, goodname, labelids, type);
            if (list != null)
                count = list.Count;

            return count;
            #endregion

        }

        public bool UpdateListSort(List<FoodGoods> list,int storeId)
        {
            if (list == null || list.Count <= 0)
            {
                return false;
            }
            DateTime date = DateTime.Now;

            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                string sql = $"update  FoodGoods set Sort={item.Sort},UpdateDate='{date}'  where Id={item.Id};";
                item.UpdateDate = date;
                sb.Append(sql);
            }
            RedisUtil.Remove(string.Format(foodGoodsKey, storeId));
            return ExecuteNonQuery(sb.ToString())>0;

        }
    }
}
