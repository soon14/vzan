using DAL.Base;
using Entity.MiniApp.Fds;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.MiniApp.Fds
{
    public class FoodGoodsTypeBLL : BaseMySql<FoodGoodsType>
    {
        #region 单例模式
        private static FoodGoodsTypeBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodGoodsTypeBLL()
        {

        }

        public static FoodGoodsTypeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodGoodsTypeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 餐饮版产品类别缓存{餐饮版店铺Id} 
        /// </summary>
        public static readonly string foodGoodsTypeKey = "foodGoodsType_{0}";


        /// <summary>
        /// 查询餐饮店铺的分类，只需要 "Id,Name,SortVal" 3个字段
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public List<FoodGoodsType> GetlistByFoodId(int foodId)
        {
            string key = string.Format(foodGoodsTypeKey,foodId);
            List<FoodGoodsType> listType = RedisUtil.Get<List<FoodGoodsType>>(key);
            if (listType == null)
            {
                listType = GetList($"FoodId={foodId} and State>=0", 100, 1, "Id,Name,SortVal", "SortVal desc,Id desc");
                RedisUtil.Set(key, listType);
            }
          
            return listType;
        }

        public bool UpdateListSort(List<FoodGoodsType> list, int storeId)
        {
            if (list == null || list.Count <= 0)
            {
                return false;
            }
            DateTime date = DateTime.Now;

            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                string sql = $"update  FoodGoodsType set SortVal={item.SortVal},UpdateDate='{date}'  where Id={item.Id};";
                item.UpdateDate = date;
                sb.Append(sql);
            }
            RedisUtil.Remove(string.Format(foodGoodsTypeKey, storeId));
            return ExecuteNonQuery(sb.ToString()) > 0;
        }
    }
}
