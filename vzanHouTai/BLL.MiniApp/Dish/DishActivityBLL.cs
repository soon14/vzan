using DAL.Base;
using Entity.MiniApp.Dish;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Dish
{
    public class DishActivityBLL: BaseMySql<DishActivity>
    {
        #region 单例模式
        private static DishActivityBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishActivityBLL()
        {

        }

        public static DishActivityBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishActivityBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 计算当前门店可用代金券优惠的总金额
        /// </summary>
        /// <returns></returns>
        public double GetQuanJiner(int dishId,int userid)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string strSql = $"SELECT sum(q_diyong_jiner) from dishactivity a where storeId={dishId} and q_begin_time<'{now}' and '{now}'< q_end_time " +
                $"and state=1 and q_type=1 " +
                //排除自己已经用过的
                $"and id not in(select quan_id from dishactivityuser where quan_status=1 and dish_id={dishId} and quan_type=1 and user_id={userid})" +
                //排除已经领完了的
                $"and  (q_limit_num=0 or( q_limit_num>0 and q_limit_num>(SELECT count(0) from dishactivityuser where dish_id={dishId} and quan_type=1  and quan_id=a.id)))";
            object result = SqlMySql.ExecuteScalar(connName, CommandType.Text, strSql);
            if (Convert.IsDBNull(result))
                return 0;
            return double.Parse(result.ToString());
        }

        /// <summary>
        /// 查询店铺中所有可用的券
        /// </summary>
        /// <param name="dishId"></param>
        /// <returns></returns>
        public List<DishActivity> GetAvailableQuan(int dishId)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string strSql = $"SELECT * from dishactivity where storeId={dishId} and q_begin_time<'{now}' and '{now}'< q_end_time and state=1 and q_type=1";
            return GetListBySql(strSql);
        }

        /// <summary>
        /// 领取优惠券
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public bool DistributeActivity(int userid, DishActivity activity)
        {
            DateTime now = DateTime.Now;
            //是否可用
            if (now < activity.q_begin_time || now > activity.q_end_time || activity.state <= 0)
                return false;
            //是否已领完

            if (activity.q_limit_num > 0 && DishActivityUserBLL.SingleModel.GetCount($"quan_id={activity.id}")>=activity.q_limit_num)
                return false;
            //是否已领过
            
            if (DishActivityUserBLL.SingleModel.Exists($"quan_id={activity.id} and user_id={userid}"))
                return false;

            DishActivityUser model = new DishActivityUser()
            {
                aId=activity.aId,
                dish_id=activity.storeId,
                quan_add_time=DateTime.Now,
                quan_begin_time=activity.q_begin_time,
                quan_end_time=activity.q_end_time,
                quan_id=activity.id,
                quan_jiner=activity.q_diyong_jiner,
                quan_limit_jiner=activity.q_xiaofei_jiner,
                quan_name=activity.q_name,
                quan_status=0,
                quan_type=1,
                user_id=userid,
            };
            int newid = Convert.ToInt32(DishActivityUserBLL.SingleModel.Add(model));
            return newid > 0;
        }
        /// <summary>
        /// 返回当前消费符合的最大满减活动
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="q_xiaofei_jiner">消费金额</param>
        /// <returns></returns>
        public DishActivity GetMoneyMeetActivity_manjian(int aId,int storeId, double q_xiaofei_jiner)
        {
            string sql = $"select * from DishActivity where aId = @aId and storeId = @storeId and q_type = 2 and state = 1 and q_xiaofei_jiner <=  @q_xiaofei_jiner and  q_begin_time <= @curTime and q_end_time >= @curTime order by q_diyong_jiner desc limit 0,1";
            List<MySqlParameter> sqlParams = new List<MySqlParameter>();
            sqlParams.Add(new MySqlParameter("@aId", aId));
            sqlParams.Add(new MySqlParameter("@storeId", storeId));
            sqlParams.Add(new MySqlParameter("@q_xiaofei_jiner", q_xiaofei_jiner));
            sqlParams.Add(new MySqlParameter("@curTime", DateTime.Now));

            DishActivity maxDiYongJinErActivity = null;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql, sqlParams.ToArray()))
            {
                while (dr.Read())
                {
                    maxDiYongJinErActivity = GetModel(dr);
                }
            }
            return maxDiYongJinErActivity;
        }

    }
}
