using DAL.Base;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Plat
{
    public class PlatStatisticalFlowBLL : BaseMySql<PlatStatisticalFlow>
    {
        #region 单例模式
        private static PlatStatisticalFlowBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatStatisticalFlowBLL()
        {

        }

        public static PlatStatisticalFlowBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatStatisticalFlowBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static readonly string _redis_PlatVisiteTimeKey = "platvisitetime_{0}";

        public int GetPVCount(int? aid,string appid="",string startime="",string endtime="")
        {
            if (aid==null || aid<=0)
                return 0;

            string sql = $"select sum(VisitPV) from PlatStatisticalFlow where aid={aid} ";
            if(!string.IsNullOrEmpty(appid))
            {
                sql += $" and appid='{appid}'";
            }
            if(!string.IsNullOrEmpty(startime))
            {
                sql += $" and date_format(refdate, '%Y-%m-%d')>='{startime}'";
            }
            if (!string.IsNullOrEmpty(endtime))
            {
                sql += $" and date_format(refdate, '%Y-%m-%d')<'{endtime}'";
            }

            object result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, null);
            if (DBNull.Value != result)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        public List<PlatStatisticalFlow> GetListByAid(int aid, string startime = "", string endtime = "")
        {
            List<PlatStatisticalFlow> list = new List<PlatStatisticalFlow>();
            string sqlwhere = $"aid={aid}";
            if (!string.IsNullOrEmpty(startime))
            {
                sqlwhere += $" and date_format(refdate, '%Y-%m-%d')>='{startime}'";
            }
            if (!string.IsNullOrEmpty(endtime))
            {
                sqlwhere += $" and date_format(refdate, '%Y-%m-%d')<'{endtime}'";
            }
            list = base.GetList(sqlwhere);

            return list;
        }
    }
}
