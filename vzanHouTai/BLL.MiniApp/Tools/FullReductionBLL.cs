using DAL.Base;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Tools
{
    public class FullReductionBLL : BaseMySql<FullReduction>
    {
        #region 单例模式
        private static FullReductionBLL _singleModel;
        private static readonly object SynObject = new object();

        private FullReductionBLL()
        {

        }

        public static FullReductionBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FullReductionBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 获取数据数据库时间里是否有交集与当前时间段
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public bool HaveFullReductionByTime(DateTime startTime, DateTime endTime,int curId,int aid)
        {
            List<MySqlParameter> mysqlPameter = new List<MySqlParameter>();

            string sqlWhere = $"Aid={aid} and Id<>{curId} and IsDel<>-1 and ((StartTime BETWEEN @startTime and @endTime) or (EndTime BETWEEN @startTime and @endTime) or (StartTime<@startTime and EndTime>@endTime))";
            mysqlPameter.Add(new MySqlParameter("@startTime", startTime));
            mysqlPameter.Add(new MySqlParameter("@endTime", endTime));
            return base.GetCount(sqlWhere, mysqlPameter.ToArray())>0;
        }

        public List<FullReduction> GetListFullReduction(int aid,out int totalCount,int pageIndex=1,int pageSize=10)
        {
            totalCount = 0;
            string strWhere = $"aid={aid} and IsDel<>-1";
            List<FullReduction> list= base.GetList(strWhere, pageSize, pageIndex);
            if (list != null && list.Count > 0)
            {
                totalCount = base.GetCount(strWhere);
            }
            return list;
        }


        /// <summary>
        /// 获取小程序当前时间段优惠活动
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public FullReduction GetFullReduction(int aid)
        {
            List<MySqlParameter> mysqlPameter = new List<MySqlParameter>();

            string sqlWhere = $"Aid={aid}  and (StartTime<=@startTime and EndTime>@startTime) and IsDel<>-1";
            mysqlPameter.Add(new MySqlParameter("@startTime", DateTime.Now));
            return base.GetModel(sqlWhere, mysqlPameter.ToArray()) ;
        }


    }
}
