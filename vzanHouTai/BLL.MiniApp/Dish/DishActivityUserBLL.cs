using DAL.Base;
using Entity.MiniApp.Dish;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Dish
{
    public class DishActivityUserBLL : BaseMySql<DishActivityUser>
    {
        #region 单例模式
        private static DishActivityUserBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishActivityUserBLL()
        {

        }

        public static DishActivityUserBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishActivityUserBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 当前店铺用户可用的优惠券
        /// </summary>
        /// <param name="dishId"></param>
        /// <param name="userId"></param>
        /// <returns>
        /// 优惠券列表，
        /// 优惠券总金额
        /// </returns>
        public Tuple<List<DishActivityUser>, double> GetAvailableQuan(int dishId, int userId)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string strSql = $"SELECT * from dishactivityuser where dish_id={dishId} and user_id={userId} and quan_begin_time<'{now}' and '{now}'< quan_end_time and quan_status=0 and quan_type=1";
            List<DishActivityUser> quan_list = GetListBySql(strSql);
            double all_quan_count = quan_list.Sum(p=>p.quan_jiner);
            return Tuple.Create(quan_list, all_quan_count);
        }
    }
}
