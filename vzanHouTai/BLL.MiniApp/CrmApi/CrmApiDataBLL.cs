using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.CrmApi;
using Entity.MiniApp.Dish;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.CrmApi
{
    public class CrmApiDataBLL : BaseMySql<CrmApiData>
    {
        #region 单例模式
        private static CrmApiDataBLL _singleModel;
        private static readonly object SynObject = new object();

        private CrmApiDataBLL()
        {

        }

        public static CrmApiDataBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CrmApiDataBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public string _crmApiHost = WebSiteConfig.CrmApiHost; //"https://api.ikcrm.com";
        public string _versionCode = WebSiteConfig.CrmVersionCode;//"3.13.0";
        public string _device = WebSiteConfig.CrmDevice;//"dingtalk";
        public string _login = WebSiteConfig.CrmLogin;//"15766604268";
        public string _password = WebSiteConfig.CrmPassword;//"huaxing123";

        public string _loginUrl = "/api/v2/auth/login";
        public string _newIndexUrl = "/api/v2/revisit_logs/new_index";
        public string _leadsUrl = "/api/v2/leads";
        public string _customersUrl = "/api/v2/customers";

        public CrmBaseData<CRMUserData> Login()
        {
            CrmBaseData<CRMUserData> returndata = new CrmBaseData<CRMUserData>();
            string url = _crmApiHost + _loginUrl;
            object data = new { device = _device, version_code = _versionCode, login = _login, password = _password };
            string postdatastr = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, postdatastr);
            if (!string.IsNullOrEmpty(result))
            {
                returndata = Newtonsoft.Json.JsonConvert.DeserializeObject<CrmBaseData<CRMUserData>>(result);
            }
            return returndata;
        }

        public void CrmService()
        {
            CrmBaseData<CRMUserData> logininfo = Login();
            if (logininfo != null && logininfo.data != null && !string.IsNullOrEmpty(logininfo.data.user_token))
            {
                GetCrmLeadsList(logininfo.data.user_token);
                CrmApiData modelConfig = base.GetModel(1);
                DateTime nowtime = DateTime.Parse(DateTime.Now.ToShortDateString());
                DateTime currenttime = DateTime.Parse(modelConfig.CurrentTime);
                if (nowtime.CompareTo(currenttime) > 0)
                {
                    List<CrmData> crmdata = GetNewIndex(logininfo.data.user_token, modelConfig);
                    SaveLog(crmdata, modelConfig);
                }
            }
            else
            {
                log4net.LogHelper.WriteInfo(this.GetType(),"获取crm服务登陆信息失败"+Newtonsoft.Json.JsonConvert.SerializeObject(logininfo));
            }
        }

        public bool SaveLog(List<CrmData> crmdata, CrmApiData modelConfig)
        {
            TransactionModel tran = new TransactionModel();
            AgentDistributionRelationBLL agentDistributionRelationBLL = new AgentDistributionRelationBLL();
            
            if (modelConfig == null)
                return false;

            if (crmdata == null || crmdata.Count <= 0)
            {
                UpdateCrmApiData(modelConfig);
                return false;
            }

            string phones = "'" + string.Join("','", crmdata.Select(s => s.phone).Distinct()) + "'";
            List<Account> accountList = AccountBLL.SingleModel.GetListByPhones(phones);
            if (accountList == null || accountList.Count <= 0)
            {
                UpdateCrmApiData(modelConfig);
                return false;
            }

            string accountids = "'" + string.Join("','", accountList.Select(s => s.Id).Distinct()) + "'";
            List<AgentDistributionRelation> agentdistributionlist = agentDistributionRelationBLL.GetListByAgent(accountids);
            if (agentdistributionlist == null || agentdistributionlist.Count <= 0)
            {
                UpdateCrmApiData(modelConfig);
                return false;
            }

            foreach (CrmData item in crmdata)
            {
                Account accountitem = accountList.FirstOrDefault(f => f.ConsigneePhone == item.phone);
                if (accountitem == null)
                    continue;
                AgentDistributionRelation adbitem = agentdistributionlist.FirstOrDefault(f => f.UserAccountId == accountitem.Id.ToString());
                if (adbitem == null)
                    continue;

                AgentFollowLog followlog = new AgentFollowLog();
                followlog.AddTime = item.AddTime;
                followlog.UpdateTime = item.AddTime;
                followlog.State = 1;
                followlog.Type = 1;
                followlog.SouceFrom = 1;
                followlog.AgentDistributionRelatioinId = adbitem.Id;
                followlog.Desc = "业务员：" + item.name + "，跟进状态：" + item.statecontent + ",备注：" + item.content;
                tran.Add($"insert into AgentFollowLog(agentdistributionrelatioinid,`desc`,state,addtime,updatetime,type,writer,soucefrom) values ({followlog.AgentDistributionRelatioinId},'{followlog.Desc}',{followlog.State},'{followlog.AddTime.ToString("yyyy-MM-dd HH:mm:ss")}','{followlog.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")}',{followlog.Type},'',{followlog.SouceFrom})");
            }

            if (tran.sqlArray.Length > 0)
            {
                bool isok = base.ExecuteTransaction(tran.sqlArray);
                if (isok)
                {
                    UpdateCrmApiData(modelConfig);
                }
                return isok;
            }
            //else
            //{
            //    if (DateTime.Now.CompareTo(DateTime.Parse(modelConfig.CurrentTime).AddDays(1)) > 0)
            //    {
            //        string columns = "pageindex,TotalPageSize,CurrentTime";
            //        modelConfig.PageIndex = 1;
            //        modelConfig.TotalPageSize = 0;
            //        modelConfig.CurrentTime = DateTime.Parse(modelConfig.CurrentTime).AddDays(1).ToShortDateString();

            //        base.Update(modelConfig, columns);
            //    }
            //}

            return false;
        }

        public void UpdateCrmApiData(CrmApiData modelConfig)
        {
            string columns = "pageindex";
            modelConfig.PageIndex += 1;
            if (modelConfig.PageIndex > modelConfig.TotalPageSize)
            {
                modelConfig.PageIndex = 1;
                modelConfig.TotalPageSize = 0;
                modelConfig.CurrentTime = DateTime.Parse(modelConfig.CurrentTime).AddDays(1).ToShortDateString();
                columns += ",TotalPageSize,CurrentTime";
            }
            base.Update(modelConfig, columns);
        }

        /// <summary>
        /// 跟进模块
        /// </summary>
        /// <param name="usertoken"></param>
        /// <param name="modelConfig"></param>
        /// <returns></returns>
        public List<CrmData> GetNewIndex(string usertoken, CrmApiData modelConfig)
        {
            List<CrmData> crmdata = new List<CrmData>();
            if (modelConfig == null)
            {
                return crmdata;
            }
            string starttime = modelConfig.CurrentTime;
            string endtime = modelConfig.CurrentTime;
            int pagesize = modelConfig.PageSize;
            int pageindex = modelConfig.PageIndex;
            string url = _crmApiHost + _newIndexUrl + $"?user_token={usertoken}&version_code={_versionCode}&device={_device}&start_date={starttime}&end_date={endtime}&per_page={pagesize}&page={pageindex}&date=other";

            string result = HttpHelper.GetData(url);
            if (!string.IsNullOrEmpty(result))
            {
                CrmBaseData<CRMNewIndexBoxData> returndata = Newtonsoft.Json.JsonConvert.DeserializeObject<CrmBaseData<CRMNewIndexBoxData>>(result);
                if (returndata != null && returndata.data.revisit_logs != null && returndata.data.revisit_logs.Count > 0)
                {
                    if (modelConfig.TotalPageSize <= 0)
                    {
                        modelConfig.TotalPageSize = Convert.ToInt32(Math.Ceiling(returndata.data.total_count / Convert.ToDecimal(returndata.data.per_page)));
                        base.Update(modelConfig, "totalpagesize");
                    }

                    foreach (CRMNewIndexData item in returndata.data.revisit_logs)
                    {
                        CrmBaseData<CRMLeadsData> leaddata = GetCrmfilterLeads(usertoken, item.loggable_id);
                        if (leaddata == null || leaddata.data == null || leaddata.data.address == null)
                        {
                            continue;
                        }
                        CrmData data = new CrmData();
                        data.phone = leaddata.data.address.tel;
                        data.content = item.content;
                        data.statecontent = leaddata.data.status_mapped;
                        data.name = item.user.name;
                        data.AddTime = item.real_revisit_at;
                        crmdata.Add(data);
                    }
                }
            }

            return crmdata;
        }

        public CrmBaseData<CRMLeadsData> GetCrmfilterLeads(string usertoken, int id)
        {
            CrmLeads model = CrmLeadsBLL.SingleModel.GetModelLeadId(id);
            if (model != null)
            {
                CrmBaseData<CRMLeadsData> returndata = new CrmBaseData<CRMLeadsData>();
                returndata.data = new CRMLeadsData();
                returndata.data.address = new CRMAddressData();
                returndata.data.address.tel = model.Phone;
                returndata.data.status_mapped = model.StateContent;
                return returndata;
            }

            //string url = _crmApiHost + _leadsUrl + $"/{id}?user_token={usertoken}&version_code={_versionCode}&device={_device}";
            //string result = HttpHelper.GetData(url);
            //if (!string.IsNullOrEmpty(result) && result != "Retry later\n")
            //{
            //    returndata = Newtonsoft.Json.JsonConvert.DeserializeObject<CrmBaseData<CRMLeadsData>>(result);
            //}

            return new CrmBaseData<CRMLeadsData>();
        }

        public void GetCrmLeadsList(string usertoken)
        {
            TransactionModel tran = new TransactionModel();
            CrmApiData modelConfig = base.GetModel(2);
            if (modelConfig == null) return;
            string url = _crmApiHost + _leadsUrl + $"?user_token={usertoken}&version_code={_versionCode}&device={_device}&source=4264341&per_page={modelConfig.PageSize}&page={modelConfig.PageIndex}&updated_at={modelConfig.CurrentTime}";

            string result = HttpHelper.GetData(url);
            if (string.IsNullOrEmpty(result)) return;
            CrmBaseData<CRMLeadsBoxData> datalist = new CrmBaseData<CRMLeadsBoxData>();
            try
            {
                datalist = Newtonsoft.Json.JsonConvert.DeserializeObject<CrmBaseData<CRMLeadsBoxData>>(result);
            }
            catch (Exception ex)
            {
                return;
            }
            
            if (datalist == null || datalist.data == null || datalist.data.leads == null || datalist.data.total_count <= 0) return;

            if (modelConfig.TotalPageSize <= 0)
            {
                modelConfig.TotalPageSize = Convert.ToInt32(Math.Ceiling(datalist.data.total_count / Convert.ToDecimal(datalist.data.per_page)));
                base.Update(modelConfig, "totalpagesize");
            }

            foreach (CRMLeadsData item in datalist.data.leads)
            {
                CrmLeads model = CrmLeadsBLL.SingleModel.GetModelLeadId(item.id);
                if (model == null)
                {
                    model = new CrmLeads();
                    model.AddTime = item.created_at;
                    model.UpdateTime = item.updated_at;
                    model.Phone = item.address.tel;
                    model.LeadId = item.id;
                    model.Name = item.name;
                    int state = 0;
                    int.TryParse(item.status,out state);
                    model.State = state;
                    model.StateContent = item.status_mapped;
                    //CrmLeadsBLL.SingleModel.Add(model);
                    tran.Add(CrmLeadsBLL.SingleModel.BuildAddSql(model));
                }
                else
                {
                    model.AddTime = item.created_at;
                    model.UpdateTime = item.updated_at;
                    model.Phone = item.address.tel;
                    model.LeadId = item.id;
                    model.Name = item.name;
                    int state = 0;
                    int.TryParse(item.status, out state);
                    model.State = state;
                    model.StateContent = item.status_mapped;
                    //CrmLeadsBLL.SingleModel.Update(model);

                    tran.Add(CrmLeadsBLL.SingleModel.BuildUpdateSql(model));
                }
            }

            if (tran.sqlArray.Length > 0)
            {
                bool isok = base.ExecuteTransaction(tran.sqlArray);
                if (!isok)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "更新crm系统线索数据失败");
                }
            }

            modelConfig.PageIndex += 1;
            if (modelConfig.PageIndex > modelConfig.TotalPageSize)
            {
                modelConfig.PageIndex = 1;
                modelConfig.TotalPageSize = 0;
                base.Update(modelConfig, "pageindex,TotalPageSize");
            }
            else
            {
                base.Update(modelConfig, "pageindex");
            }

        }

        /// <summary>
        /// 客户模块
        /// </summary>
        /// <param name="usertoken"></param>
        /// <param name="modelConfig"></param>
        /// <returns></returns>
        public List<CrmData> GetCustomerList(string usertoken, CrmApiData modelConfig)
        {
            List<CrmData> crmdata = new List<CrmData>();
            if (modelConfig == null)
            {
                return crmdata;
            }
            string starttime = modelConfig.CurrentTime;
            string endtime = modelConfig.CurrentTime;
            int pagesize = modelConfig.PageSize;
            int pageindex = modelConfig.PageIndex;
            string url = _crmApiHost + _customersUrl + $"?user_token={usertoken}&version_code={_versionCode}&device={_device}&start_date={starttime}&end_date={endtime}&per_page={pagesize}&page={pageindex}&date=other";

            string result = HttpHelper.GetData(url);
            if (!string.IsNullOrEmpty(result))
            {
                CrmBaseData<CRMNewIndexBoxData> returndata = Newtonsoft.Json.JsonConvert.DeserializeObject<CrmBaseData<CRMNewIndexBoxData>>(result);
                if (returndata != null && returndata.data.revisit_logs != null && returndata.data.revisit_logs.Count > 0)
                {
                    if (modelConfig.TotalPageSize <= 0)
                    {
                        modelConfig.TotalPageSize = Convert.ToInt32(Math.Ceiling(returndata.data.total_count / Convert.ToDecimal(returndata.data.per_page)));
                        base.Update(modelConfig, "totalpagesize");
                    }

                    foreach (CRMNewIndexData item in returndata.data.revisit_logs)
                    {
                        CrmBaseData<CRMLeadsData> leaddata = GetCrmfilterLeads(usertoken, item.loggable_id);
                        if (leaddata == null || leaddata.data == null || leaddata.data.address == null)
                        {
                            continue;
                        }
                        CrmData data = new CrmData();
                        data.phone = leaddata.data.address.tel;
                        data.content = item.content;
                        data.statecontent = leaddata.data.status_mapped;
                        data.name = item.user.name;
                        data.AddTime = item.real_revisit_at;
                        crmdata.Add(data);
                    }
                }
            }

            return crmdata;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">目标连接字符</param>
        /// <param name="TableName">目标表</param>
        /// <param name="dt">源数据</param>
        private void SqlBulkCopyByDatatable(string connectionString, string TableName, DataTable dt)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlBulkCopy sqlbulkcopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction))
                {
                    try
                    {
                        sqlbulkcopy.DestinationTableName = TableName;
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            sqlbulkcopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                        }
                        sqlbulkcopy.WriteToServer(dt);
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }
    }
}
