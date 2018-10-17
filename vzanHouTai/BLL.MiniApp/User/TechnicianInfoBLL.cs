using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.User
{

    public class TechnicianInfoBLL : BaseMySql<TechnicianInfo>
    {
        #region 单例模式
        private static TechnicianInfoBLL _singleModel;
        private static readonly object SynObject = new object();

        private TechnicianInfoBLL()
        {

        }

        public static TechnicianInfoBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new TechnicianInfoBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<TechnicianInfo> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<TechnicianInfo>();

            return base.GetList($"id in ({ids})");
        }

        /// <summary>
        /// 根据id获取技师
        /// </summary>
        /// <param name="tuserId"></param>
        /// <returns></returns>
        public TechnicianInfo GetModelById(int userId)
        {
            string sqlwhere = $"id={userId} and state>{(int)TechnicianState.删除}";
            return GetModel(sqlwhere);
        }
        /// <summary>
        /// 根据条件获取技师列表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="jobNumber"></param>
        /// <param name="name"></param>
        /// <param name="sex"></param>
        /// <param name="phone"></param>
        /// <param name="itemId"></param>
        /// <param name="isShow"></param>
        /// <param name="state"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public List<TechnicianInfo> GetTechnicianList(int storeId, string jobNumber, string name, int sex, string phone, int itemId, int isShow, int state, int pageIndex, int pageSize, out int recordCount)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string sqlwhere = $"storeid={storeId} and state>=0";
            if (!string.IsNullOrEmpty(jobNumber))
            {
                sqlwhere += " and jobnumber like @jobnumber";
                parameters.Add(new MySqlParameter("@jobnumber", $"%{jobNumber}%"));
            }
            if (!string.IsNullOrEmpty(name))
            {
                sqlwhere += " and name like @name";
                parameters.Add(new MySqlParameter("@name", $"%{name}%"));
            }
            if (sex >= 0)
            {
                sqlwhere += $" and sex={sex}";
            }
            if (!string.IsNullOrEmpty(phone))
            {
                sqlwhere += " and phone like @phone";
                parameters.Add(new MySqlParameter("@phone", $"%{phone}%"));
            }
            if (itemId > 0)
            {
                sqlwhere += $" and FIND_IN_SET('{itemId}',itemid)";
            }
            if (isShow >= 0)
            {
                switch (isShow)
                {
                    case 0: sqlwhere += $" and switchConfig like '%showIndex:false%'"; break;
                    case 1: sqlwhere += $" and switchConfig like '%showIndex:true%'"; break;
                }
            }
            if (state > -1)
            {
                sqlwhere += $" and state={state}";
            }
            recordCount = GetCount(sqlwhere, parameters.ToArray());
            return GetListByParam(sqlwhere, parameters.ToArray(), pageSize, pageIndex, "*", "id desc");
        }
        /// <summary>
        /// 验证手机号是否已被其他人使用
        /// </summary>
        /// <param name="postdata"></param>
        /// <returns></returns>
        public bool ValidPhone(TechnicianInfo postdata)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@TelPhone", postdata.TelPhone));
            TechnicianInfo model = GetModel($" appid={postdata.appId} and storeid={postdata.storeId} and id!={postdata.id} and state>-1 and TelPhone = @TelPhone", parameters.ToArray());
            return model != null;
        }
        /// <summary>
        /// 验证工号是否已存在
        /// </summary>
        /// <param name="postdata"></param>
        /// <returns></returns>
        public bool ValidJobNumber(TechnicianInfo postdata)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@jobNumber", postdata.jobNumber));
            TechnicianInfo checkData = GetModel($"appid={postdata.appId} and storeid={postdata.storeId} and id!={postdata.id} and state>-1 and jobNumber=@jobNumber", parameters.ToArray());
            return checkData != null;
        }
        /// <summary>
        /// 验证微信号是否已被使用
        /// </summary>
        /// <param name="postdata"></param>
        /// <returns></returns>
        public bool ValidWeChat(TechnicianInfo postdata)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@unionId", postdata.unionId));
            TechnicianInfo checkData = GetModel($"appid={postdata.appId} and id!={postdata.id} and state>-1 and unionId=@unionId", parameters.ToArray());
            return checkData != null;
        }

        public List<TechnicianInfo> GetTechnicianList(int storeId, int sex, int orderByAge, int orderByCount, int pageSize, int pageIndex)
        {
            string sql = $"select *,(servicecount+basecount) as sum from technicianinfo where storeid={storeId} and state>=0";
            string orderstr = " order by 1=1,";
            if (sex != 0)
            {
                sql += $" and sex={sex}";
            }
            switch (orderByAge)
            {
                case 1: orderstr += " birthday desc,"; break;
                case 2: orderstr += " birthday asc,"; break;
            }

            switch (orderByCount)
            {
                case 1: orderstr += " sum asc,"; break;
                case 2: orderstr += " sum desc,"; break;
            }
            sql += $"{orderstr.TrimEnd(',')} limit {(pageIndex - 1) * pageSize},{pageSize}";
            List<TechnicianInfo> list = new List<TechnicianInfo>();
            DataSet ds = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (ds.Tables.Count <= 0)
                return list;
            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows.Count <= 0)
                return list;
            TechnicianInfo model = null;
            foreach (DataRow row in dt.Rows)
            {
                model = new TechnicianInfo();
                model.id = Convert.ToInt32(row["id"]);
                model.appId = Convert.ToInt32(row["appId"]);
                model.storeId = Convert.ToInt32(row["storeId"]);
                model.unionId = row["unionid"].ToString();
                model.jobNumber = row["jobNumber"].ToString();
                model.sex = Convert.ToInt32(row["sex"]);
                model.birthday = Convert.ToDateTime(row["birthday"]);
                model.headImg = row["headImg"].ToString();
                model.serviceCount = Convert.ToInt32(row["sum"]);
                model.photo = row["photo"].ToString();
                model.state = Convert.ToInt32(row["state"]);
                model.desc = row["desc"].ToString();
                model.switchModel = JsonConvert.DeserializeObject<TechnicianSwitch>(row["switchconfig"].ToString());
                list.Add(model);
            }
            return list;
        }

        /// <summary>
        /// 根据工号或状态获取技师列表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="jobNumber"></param>
        /// <param name="workType"></param>
        /// <returns></returns>
        public List<TechnicianInfo> GetTechnicianListByJobNumeberOrWorkType(int storeId, string jobNumber, int workType)
        {
            string sqlwhere = $"storeid={storeId} and state>=0";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(jobNumber))
            {
                sqlwhere += " and jobnumber like @jobnumber";
                parameters.Add(new MySqlParameter("@jobnumber", $"%{jobNumber}%"));
            }
            if (workType > -1)
            {
                sqlwhere += $" and state={workType}";
            }
           return GetListByParam(sqlwhere, parameters.ToArray());
        }
        /// <summary>
        /// 根据技师可服务项目id获取技师列表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<TechnicianInfo> GetListByServiceId(int storeId, int serviceId)
        {
            string sqlwhere = $" storeid={storeId} and state>=0 and FIND_IN_SET('{serviceId}', itemid)";
            return GetList(sqlwhere);
        }
        /// <summary>
        /// 根据姓名，工号，性别查询技师
        /// </summary>
        /// <param name="name"></param>
        /// <param name="jobNumber"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        public List<TechnicianInfo> GetTechnicianList(string name, string jobNumber, int sex)
        {
            string sqlWhere = "1=1";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(name))
            {
                sqlWhere += " and name like @name";
                parameters.Add(new MySqlParameter("@name", $"%{name}%"));
            }
            if (!string.IsNullOrEmpty(jobNumber))
            {
                sqlWhere += " and jobNumber like @jobNumber";
                parameters.Add(new MySqlParameter("@jobNumber", $"%{jobNumber}%"));
            }
            if (sex > 0)
            {
                sqlWhere += $" and sex ={sex}";
            }
            return GetListByParam(sqlWhere, parameters.ToArray());
        }
        /// <summary>
        /// 根据服务项目id获取技师列表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<TechnicianInfo> GetTechnicianListByServiceId(int storeId, int serviceId)
        {
            return GetList($" storeid={storeId} and state>=0 and FIND_IN_SET('{serviceId}', itemid)");
        }

        public TechnicianInfo GetTechnicianListByAid_JobNumeber(int aid, string telePhoneNumber)
        {
            return GetModel($" TelPhone = '{telePhoneNumber}' and State != {(int)TechnicianState.删除} and appId={aid}");
        }

        public List<TechnicianInfo> GetTechnicianListByStoreId(string storeId)
        {
            return GetList($" storeId = {storeId} and state >= 0 ");
        }
    }
}
