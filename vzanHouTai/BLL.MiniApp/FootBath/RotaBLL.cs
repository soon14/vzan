using DAL.Base;
using Entity.MiniApp.Footbath;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Footbath
{
    public class RotaBLL : BaseMySql<Rota>
    {
        #region 单例模式
        private static RotaBLL _singleModel;
        private static readonly object SynObject = new object();

        private RotaBLL()
        {

        }

        public static RotaBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new RotaBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 获取技师某一天的json格式值班状态
        /// </summary>
        /// <param name="monday"></param>
        /// <returns></returns>
        public WorkTimeState GetDayState(string daystr)
        {
            WorkTimeState result = new WorkTimeState();
            if (daystr.Contains("1"))
            {
                result.morning = true;
            }
            if (daystr.Contains("2"))
            {
                result.noon = true;
            }
            if (daystr.Contains("3"))
            {
                result.evening = true;
            }
            return result;
        }

        /// <summary>
        /// 根据技师json格式值班状态得出存入数据表的值班状态字符串
        /// </summary>
        /// <param name="rotaInfo"></param>
        public string GetDayStr(WorkTimeState workTime)
        {
            string result = string.Empty;
            if (workTime == null)
            {
                return result;
            }
            if (workTime.morning)
            {
                result += "1,";
            }
            if (workTime.noon)
            {
                result += "2,";
            }
            if (workTime.evening)
            {
                result += "3,";
            }
            result = result.TrimEnd(',');
            return result;
        }
        /// <summary>
        /// 删除排班表
        /// </summary>
        /// <param name="tid"></param>
        public void deleteRotaByTid(int tid)
        {
            if (tid > 0)
            {
                string sql = $"update rota set state=-1 where tid={tid}";
                SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);
            }
        }
        /// <summary>
        /// 获取排班表
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="tids"></param>
        /// <param name="daytype"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<Rota> GetRotaList(int aId, string tids, int daytype, int pageSize, int pageIndex, out int recordCount)
        {
            try
            {
                string sqlwhere = $"aid={aId} and state>-1";
                if (!string.IsNullOrEmpty(tids))
                {
                    sqlwhere += $" and tid in ({tids})";
                }
                if (daytype > -1)
                {
                    sqlwhere += $" and daytype={daytype}";
                }
                recordCount = GetCount(sqlwhere);
                return GetList(sqlwhere, pageSize, pageIndex, "*", "tid desc,daytype asc");
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                recordCount = 0;
                return null;
            }
        }
        /// <summary>
        /// 根据aid，id获取值班信息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Rota GetModelById(int aid, int id)
        {
            return GetModel($"id={id} and state>=0 and aid={aid}");
        }
    }
}
