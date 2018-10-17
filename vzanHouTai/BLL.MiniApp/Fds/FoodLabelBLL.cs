using DAL.Base;
using Entity.MiniApp.Fds;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.MiniApp.Fds
{
    public class FoodLabelBLL : BaseMySql<FoodLabel>
    {
        /// <summary>
        /// 餐饮版产品标签缓存{餐饮版店铺Id} 
        /// </summary>
        public static readonly string foodLabelKey = "foodLabelKey_{0}";

        #region 单例模式
        private static FoodLabelBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodLabelBLL()
        {

        }

        public static FoodLabelBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodLabelBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<FoodLabel> GetListByLabelIds(string labelids)
        {
            if (string.IsNullOrEmpty(labelids))
                return new List<FoodLabel>();

            return base.GetList($" Id in ({labelids})");
        }
        /// <summary>
        /// 根据餐饮版店铺Id获取标签
        /// </summary>
        /// <param name="foodId"></param>
        /// <returns></returns>
        public List<FoodLabel> GetFoodLabelByFoodId(int foodId)
        {
            string key = string.Format(foodLabelKey, foodId);
            List<FoodLabel> listLabel = RedisUtil.Get<List<FoodLabel>>(key);
            if (listLabel == null)
            {
                listLabel = GetList($" foodStoreId={foodId} and State>=0");
                RedisUtil.Set(key, listLabel);
            }

            return listLabel;
        }

        public bool UpdateListSort(List<FoodLabel> list, int storeId)
        {
            if (list == null || list.Count <= 0)
            {
                return false;
            }
            DateTime date = DateTime.Now;

            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                string sql = $"update  FoodLabel set Sort={item.Sort},UpdateDate='{date}'  where Id={item.Id};";
                item.UpdateDate = date;
                sb.Append(sql);
            }
            RedisUtil.Remove(string.Format(foodLabelKey, storeId));
            return ExecuteNonQuery(sb.ToString()) > 0;
        }
    }
}
