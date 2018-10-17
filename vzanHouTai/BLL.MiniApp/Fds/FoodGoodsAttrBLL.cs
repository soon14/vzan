using DAL.Base;
using Entity.MiniApp.Fds;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.MiniApp.Fds
{
    public class FoodGoodsAttrBLL : BaseMySql<FoodGoodsAttr>
    {
        #region 单例模式
        private static FoodGoodsAttrBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodGoodsAttrBLL()
        {

        }

        public static FoodGoodsAttrBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodGoodsAttrBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 餐饮版产品属性缓存{餐饮版店铺Id} 
        /// </summary>
        public static readonly string foodGoodsAttrKey = "foodGoodsAttr_{0}";

        public List<FoodGoodsAttr> GetListByAttrId(string attrids)
        {
            return base.GetList($" Id in ({attrids}) ");
        }


        /// <summary>
        /// 根据店铺Id获取对应的产品规格列表 已经加缓存处理
        /// </summary>
        /// <param name="foodId"></param>
        /// <returns></returns>
        public List<FoodGoodsAttr> GetlistAttrByFoodId(int foodId)
        {
            string key = string.Format(foodGoodsAttrKey, foodId);
            List<FoodGoodsAttr> listAttr = RedisUtil.Get<List<FoodGoodsAttr>>(key);
            if (listAttr == null)
            {
                listAttr = GetList($"FoodId={foodId} and State>=0");
                RedisUtil.Set(key, listAttr);
            }

            return listAttr;
        }

        public bool UpdateListSort(List<FoodGoodsAttr> list, int storeId)
        {
            if (list == null || list.Count <= 0)
            {
                return false;
            }
            DateTime date = DateTime.Now;

            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                string sql = $"update  FoodGoodsAttr set Sort={item.Sort}  where Id={item.Id};";
                sb.Append(sql);
            }
            RedisUtil.Remove(string.Format(foodGoodsAttrKey, storeId));
            return ExecuteNonQuery(sb.ToString()) > 0;
        }
    }
}
