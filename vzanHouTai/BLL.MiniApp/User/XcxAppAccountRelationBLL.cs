using BLL.MiniApp.Conf;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Plat;
using BLL.MiniApp.Stores;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Plat;
using Entity.MiniApp.Stores;
using Entity.MiniApp.User;
using Entity.MiniApp.ViewModel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp
{
    public sealed class XcxAppAccountRelationBLL : BaseMySql<XcxAppAccountRelation>
    {
        private static string _redis_XcxAppAccountRelationModelKey = "XcxAppAccountRelationModelKey_{0}";
        private static string _redis_XcxAppAccountRelationModelVersion = "XcxAppAccountRelationModelVersion_{0}";

        #region 单例模式
        private static XcxAppAccountRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private XcxAppAccountRelationBLL()
        {

        }

        public static XcxAppAccountRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new XcxAppAccountRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        #region 基础操作
        public bool ExitModelByAppIdAndId(int id,string appid)
        {
            if(string.IsNullOrEmpty(appid))
            {
                return false;
            }
            List<MySqlParameter> parms = new List<MySqlParameter>();
            string sqlWhere = $"id<>{id} and appid=@appid";
            parms.Add(new MySqlParameter("@appid",appid));

            return base.Exists(sqlWhere,parms.ToArray());
        }
        public bool EsitPlatChild(int tid,string accountId)
        {
            return base.Exists($"accountid = '{accountId}' and tid={tid}");
        }

        public bool UpdateMasterIdByAids(string aids, int masterId)
        {
            if (string.IsNullOrEmpty(aids))
                return false;

            string sql = $"update xcxappaccountrelation set storemasterid ={masterId} where id in ({aids})";
            bool success = SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql) > 0;

            //清除缓存
            foreach (string item in aids.Split(','))
            {
                RemoveRedis(Convert.ToInt32(item));
            }

            return success;
        }

        public int UpdateModelByAppId(string appid)
        {
            string sql = $"update XcxAppAccountRelation set appid='' where appid='{appid}'";
            return SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);
        }

        public bool IsOpenExperience(string tids,string accountId)
        {
            MySqlParameter[] parm = { new MySqlParameter("@AccountId", accountId) };
            return base.Exists($"accountId=@AccountId and tid in ({tids}) and IsExperience=1",parm);
        }

        /// <summary>
        /// 查询用户开通板的最大数量
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="tid"></param>
        /// <returns></returns>
        public int GetOpenTemplateMaxCount(string accountId, int tid)
        {
            return GetCount($" AccountId='{accountId}' and tid={tid}");
        }

        /// <summary>
        /// 商家版小程序验证用户权限
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="storeMasterId"></param>
        /// <returns></returns>
        public bool CheckMaster(string appid, int storeMasterId)
        {
            string sqlwhere = $"appid='{appid}' and storemasterid={storeMasterId} and  state>=0 and outtime>now()";
            XcxAppAccountRelation model = GetModel(sqlwhere);
            return model != null;
        }

        /// <summary>
        /// 根据rid获取模板类型
        /// </summary>
        /// <param name="rid"></param>
        /// <returns></returns>
        public int GetXcxTemplateType(int rid)
        {
            string sql = $"select x.type from XcxAppAccountRelation r left join xcxtemplate x on r.tid = x.id  where r.id={rid} LIMIT 1";
            object result = SqlMySql.ExecuteScalar(connName, CommandType.Text,sql, null);
            if(result!=DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }
        
        /// <summary>
        /// 小程序模板可用并未过期
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public XcxAppAccountRelation GetModelById(int id)
        {
            return base.GetModel($" Id={id} and state>=0 and outtime>'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'");
        }

        public List<XcxAppAccountRelation> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<XcxAppAccountRelation>();

            return base.GetList($"id in ({ids})");
        }

        /// <summary>
        /// 获取未过期的权限数据集合
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<XcxAppAccountRelation> GetValuableListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<XcxAppAccountRelation>();

            return base.GetList($" Id in ({ids} ) and state>=0 and outtime>'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'");
        }

        public XcxAppAccountRelation GetModelByaccound(string accountId)
        {
            return base.GetModel($"AccountId='{accountId}' and state>=0 and outtime>'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'");
        }
        
        public XcxAppAccountRelation GetModelByaccountidAndAppid(int Id, string accountId)
        {
            return base.GetModel($" Id={Id} and accountid  = '{accountId}' and state>=0 and outtime>now()");
        }

        /// <summary>
        /// 是否是代理商开发的用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public XcxAppAccountRelation CheckAgentOpenCustomer(string accountId)
        {
            return GetModel($" AccountId='{accountId}' and agentid>0 ");
        }
        
        public List<XcxAppAccountRelation> GetListByaccountId_Tid(string accountId, int tid)
        {
            string sqlwhere = $"AccountId='{accountId}' and TId={tid} and state>=0 and outtime>'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return GetList(sqlwhere);
        }

        public List<XcxAppAccountRelation> GetListByTidAccountId(string accountIds, int tid)
        {
            if (string.IsNullOrEmpty(accountIds))
                return new List<XcxAppAccountRelation>();
            string sqlwhere = $"AccountId in({accountIds}) and TId={tid}";
            return GetList(sqlwhere);
        }

        /// <summary>
        /// 商家版小程序获取用户所有专业版店铺
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<XcxAppAccountRelation> GetListByStoreMasterId_Tid(int storeMasterId, int tid)
        {
            string sqlwhere = $"tid={tid} and storemasterid={storeMasterId} and  state>=0 and outtime>now()";
            return GetList(sqlwhere);
        }

        public List<XcxAppAccountRelation> GetListByAppidsF(string appids)
        {
            List<XcxAppAccountRelation> list = null;
            if (string.IsNullOrEmpty(appids))
            {
                return list;
            }
            string sqlwhere = $" find_in_set(appid,@appids)";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@appids", appids));
            list = GetListByParam(sqlwhere, parameters.ToArray());
            return list;
        }

        public List<XcxAppAccountRelation> GetListByAppids(string appids)
        {
            List<XcxAppAccountRelation> list = null;
            if (string.IsNullOrEmpty(appids))
            {
                return list;
            }
            string sqlwhere = $" appid in ({appids})";
            list = base.GetList(sqlwhere);
            return list;
        }
        
        /// <summary>
        /// 足浴版获取客户端小程序列
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public List<XcxAppAccountRelation> GetClientList(string accountId)
        {
            return base.GetList($"accountid='{accountId}' and tid=41");
        }
        
        public List<XcxAppAccountRelation> GetListByAccountId(string accountId,int agentId,int isExperience=0)
        {
            MySqlParameter[] parm = { new MySqlParameter("@AccountId", accountId) };
            string sqlwhere = $"accountid=@AccountId ";
            if(agentId>=0)
            {
                sqlwhere += $" and agentid={agentId} ";
            }
            if(isExperience>0)
            {
                sqlwhere += $" and IsExperience={isExperience} ";
            }
            return GetListByParam(sqlwhere, parm);
        }
        #endregion

        #region 逻辑操作
        /// <summary>
        /// 获取用户开通模板最早时间列表
        /// </summary>
        /// <returns></returns>
        public string GetListXcxappListSQL(string accountId)
        {
            string tids = "";
            List<XcxAppAccountRelation> list = new List<XcxAppAccountRelation>();
            string sql = $"select t.id tid,(select min(addtime) from xcxappaccountrelation where tid=t.id and accountid='{accountId}') addtime from xcxtemplate t where projecttype in ({(int)ProjectType.小程序},{(int)ProjectType.测试})";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                list = GetList(dr);
            }

            if (list != null && list.Count > 0)
            {
                //获取用户对应模板更新日志，更新日志时间小于用户最早开通模板时间的不显示
                foreach (XcxAppAccountRelation item in list)
                {
                    if (item.AddTime.ToString("yyyy-MM-dd HH:mm:ss") != "0001-01-01 00:00:00")
                    {
                        tids += $" or ( type=1 and tid={item.TId} and updatetime>='{item.AddTime.ToString("yyyy-MM-dd HH:mm:ss")}') ";
                    }
                }
            }

            return tids;
        }

        /// <summary>
        /// 根据aId取对应模板的店铺表的storeId
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public int ReturnStoreIdByAId(int aId, ref string errorMsg,ref string aids,ref string userIds,string telephone="")
        {
            aids = aId.ToString();
            //店铺ID
            int storeId = 0;
            XcxAppAccountRelation xcxrelation = base.GetModel(aId);
            if (xcxrelation == null)
            {
                errorMsg = "未找到小程序授权资料";
                return storeId;
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxTemplate == null)
            {
                errorMsg = "未找到小程序的模板";
                return storeId;
            }

            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} ");
                    if (store_Food == null)
                    {
                        errorMsg = "还未开通店铺";
                        return storeId;
                    }
                    storeId = store_Food.Id;
                    break;
                case (int)TmpType.小程序多门店模板:
                    FootBath store_MultiStore = FootBathBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} and HomeId = 0 ");
                    if (store_MultiStore == null)
                    {
                        errorMsg = "还未开通店铺";
                        return storeId;
                    }
                    storeId = store_MultiStore.Id;
                    break;
                case (int)TmpType.小程序电商模板:
                case (int)TmpType.小程序专业模板:
                    Store store = StoreBLL.SingleModel.GetModelByRid(xcxrelation.Id);
                    if (store == null)
                    {
                        errorMsg = "还未开通店铺";
                        return storeId;
                    }
                    storeId = store.Id;
                    break;

                case (int)TmpType.小未平台子模版:
                    PlatStore platstore = PlatStoreBLL.SingleModel.GetModelByAId(xcxrelation.Id);
                    if (platstore == null)
                    {
                        errorMsg = "还未开通店铺";
                        return storeId;
                    }
                    storeId = platstore.Id;
                    break;
                case (int)TmpType.小未平台:
                    storeId = -999;
                    List<PlatStore>  storeList = PlatStoreBLL.SingleModel.GetXcxRelationAppids(xcxrelation.Id);
                    if(storeList!=null && storeList.Count>0)
                    {
                        aids =aId+ ","+string.Join(",", storeList.Select(s => s.Aid));
                        string appids = "'" + string.Join("','", storeList.Select(s => s.AppId)) + "'";
                        List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByAppIds(telephone, appids);
                        if (userInfoList == null || userInfoList.Count <= 0)
                            return storeId;
                        userIds += "," + string.Join(",", userInfoList.Select(s => s.Id));
                    }
                    
                    break;
                default:
                    errorMsg = "还未开通店铺";
                    return storeId;
            }
            return storeId;
        }
        
        public List<XcxAppAccountRelation> GetListByaccountId(string accountId, int isCanuse = 0)
        {
            string sql = $"AccountId='{accountId}' and state>=0 and outtime>'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'";
            if (isCanuse > 0)
            {
                sql = $"AccountId='{accountId}'";
            }
            return base.GetList(sql);
        }

        public List<XcxAppAccountRelation> GetListByaccountIdandAgentId(string accountId, int agentId, int isCanuse = 0, int pageSize = 20, int pageIndex = 1)
        {
            string sql = $"AccountId='{accountId}' and state>=0 and outtime>'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'";
            if (agentId > 0)
            {
                sql += $" and agentId={agentId} ";
            }

            if (isCanuse > 0)
            {
                sql = $"AccountId='{accountId}'";
            }

            return base.GetList(sql, pageSize, pageIndex, "", "id desc");
        }

        public XcxAppAccountRelation GetModelByaccountidAndTid(string accountId, int tId, int isCanuse = 0)
        {
            string sql = $"AccountId='{accountId}' and TId={tId} and state>=0 and outtime>'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'";
            if (isCanuse > 0)
            {
                sql = $"AccountId='{accountId}' and TId={tId}";
            }
            return base.GetModel(sql);
        }

        public List<XcxAppAccountRelation> GetListByTemplateType(string accountId, TmpType templateType)
        {
            XcxTemplate template = XcxTemplateBLL.SingleModel.GetModelByType((int)templateType);
            string whereSql = $"AccountId='{accountId}' and TId={template.Id} and state>=0 and outtime>'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return base.GetList(whereSql);
        }

        public IndexViewModel GetAgentTemplateInfon(int agentId)
        {
            IndexViewModel miniappindexmodel = new IndexViewModel();
            string dsql = $"select sum(cost) as templatepricesum, sum(templatecount) as count from agentdepositlog where agentid={agentId} and type<>1";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, dsql, null))
            {
                while (dr.Read())
                {
                    ////分销商开通的模板数量
                    //List<Distribution> distributionlist = new DistributionBLL().GetListByAgentId(agentid);
                    //string cagentids = string.Join(",", distributionlist?.Select(s => s.AgentId));
                    //int cagentxcxcount = GetCountByAgentIds(cagentids);
                    miniappindexmodel.TemplateCount = Convert.ToInt32(string.IsNullOrEmpty(dr["count"].ToString()) ? "0" : dr["count"].ToString());
                    miniappindexmodel.TemplateSum = Convert.ToInt32(string.IsNullOrEmpty(dr["templatepricesum"].ToString()) ? "0" : dr["templatepricesum"].ToString());
                }
            }
            return miniappindexmodel;
        }

        public List<XcxAppAccountRelationGroupInfo> GetAgentTemplateInfonGroup(int agentId, ref int allCount)
        {
            List<Distribution> distributionList = DistributionBLL.SingleModel.GetList($"parentagentid={agentId}");
            string sqlwhere = $"agentId={agentId}";
            if (distributionList != null && distributionList.Count > 0)
            {
                string agentids = string.Join(",", distributionList.Select(x => x.AgentId).ToList());
                sqlwhere = $"agentId in ({agentId},{agentids})";
            }

            string sql = $"select Count(*) count,TId,(select TName from xcxtemplate where id=r.TId) TName from xcxappaccountrelation r where {sqlwhere}  group by TId";

            List<XcxAppAccountRelationGroupInfo> miniappindexmodel = new List<XcxAppAccountRelationGroupInfo>();
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    XcxAppAccountRelationGroupInfo model = new XcxAppAccountRelationGroupInfo();
                    model.TName = dr["TName"].ToString();
                    model.TId = Convert.ToInt32(string.IsNullOrEmpty(dr["TId"].ToString()) ? "0" : dr["TId"].ToString());
                    model.Count = Convert.ToInt32(string.IsNullOrEmpty(dr["Count"].ToString()) ? "0" : dr["Count"].ToString());
                    allCount += model.Count;
                    miniappindexmodel.Add(model);
                }
            }
            
            return miniappindexmodel;
        }

        /// <summary>
        /// 代理商开通体验版
        /// </summary>
        /// <param name="agentInfo"></param>
        /// <param name="tIds"></param>
        /// <param name="userAccount"></param>
        /// <param name="erroMsg"></param>
        /// <param name="dayLength"></param>
        /// <returns></returns>
        public bool OpenExperience(Agentinfo agentInfo, string tIds, Account userAccount, ref string erroMsg,int dayLength)
        {
            TransactionModel tranModel = new TransactionModel();
            
            //判断要开通的模板是否还没体验过
            bool canopen = IsOpenExperience(tIds, userAccount.Id.ToString());
            if (canopen)
            {
                erroMsg = "模板已开通过，请刷新重试";
                return false;
            }
            
            //添加客户
            AgentCustomerRelation agentcustomerrelation = AgentCustomerRelationBLL.SingleModel.GetModelByAccountId(agentInfo.id,userAccount.Id.ToString());
            if (agentcustomerrelation == null)
            {
                agentcustomerrelation = new AgentCustomerRelation()
                {
                    username = userAccount.LoginId,
                    addtime = DateTime.Now,
                    agentid = agentInfo.id,
                    useraccountid = userAccount.Id.ToString(),
                    state = 1,
                };
                agentcustomerrelation.id = Convert.ToInt32(AgentCustomerRelationBLL.SingleModel.Add(agentcustomerrelation));
                if(agentcustomerrelation.id<=0)
                {
                    erroMsg = "添加客户关联失败";
                    return false;
                }
            }

            List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetListByIds(tIds);
            //添加模板记录
            foreach (XcxTemplate xcx in xcxlist)
            {
                AgentdepositLog agentLog = new AgentdepositLog();
                agentLog.agentid = agentInfo.id;
                agentLog.addtime = DateTime.Now;
                agentLog.templateCount = 1;
                agentLog.customerid = agentcustomerrelation.id.ToString();
                agentLog.tid = xcx.Id;
                agentLog.type = 2;
                agentLog.templateCount = 1;
                agentLog.costdetail = $"开通体验{xcx.TName}模板";
                tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLog));

                XcxAppAccountRelation xcxappaccountrelation = new XcxAppAccountRelation()
                {
                    TId = xcx.Id,
                    AccountId = userAccount.Id,
                    AddTime = DateTime.Now,
                    agentId = agentInfo.id,
                    Url = xcx.Link,
                    price = xcx.Price,
                    TimeLength = xcx.year,
                    outtime = DateTime.Now.AddDays(dayLength),
                    SCount = xcx.SCount,
                    IsExperience = true,
                };
                xcxappaccountrelation.Id = Convert.ToInt32(base.Add(xcxappaccountrelation));

                if (xcx.Type == (int)TmpType.小程序多门店模板)
                {
                    tranModel.Add(FootBathBLL.SingleModel.GetAddFootbathSQL(xcxappaccountrelation.AccountId, xcx.SCount - 1, xcxappaccountrelation.Id, xcxappaccountrelation.outtime).ToArray());
                }
            }

            //执行事务
            bool success = false;
            try
            {
                success = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception ex)
            {
                erroMsg = ex.Message;
            }
            return success;
        }

        /// <summary>
        /// 普通用户开通体验版
        /// </summary>
        /// <param name="agentInfo"></param>
        /// <param name="tIds"></param>
        /// <param name="userAccount"></param>
        /// <param name="erroMsg"></param>
        /// <param name="dayLength"></param>
        /// <returns></returns>
        public bool OpenExperience(string tIds, Account userAccount, ref string erroMsg, int dayLength=3)
        {
            TransactionModel tranModel = new TransactionModel();

            //判断要开通的模板是否还没体验过
            bool canOpen = IsOpenExperience(tIds, userAccount.Id.ToString());
            if (canOpen)
            {
                erroMsg = "模板已开通过，请刷新重试";
                return false;
            }
            
            List<XcxTemplate> xcxList = XcxTemplateBLL.SingleModel.GetListByIds(tIds);
            //添加模板记录
            foreach (XcxTemplate xcx in xcxList)
            {
                AgentdepositLog agentLog = new AgentdepositLog();
                agentLog.agentid = 0;
                agentLog.addtime = DateTime.Now;
                agentLog.templateCount = 1;
                agentLog.customerid = userAccount.Id.ToString();
                agentLog.tid = xcx.Id;
                agentLog.type = 2;
                agentLog.templateCount = 1;
                agentLog.costdetail = $"开通体验{xcx.TName}模板";
                tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLog));

                XcxAppAccountRelation xcxappaccountrelation = new XcxAppAccountRelation()
                {
                    TId = xcx.Id,
                    AccountId = userAccount.Id,
                    AddTime = DateTime.Now,
                    agentId = 0,
                    Url = xcx.Link,
                    price = xcx.Price,
                    TimeLength = xcx.year,
                    outtime = DateTime.Now.AddDays(dayLength),
                    SCount = xcx.SCount,
                    IsExperience = true,
                };
                xcxappaccountrelation.Id = Convert.ToInt32(base.Add(xcxappaccountrelation));

                if (xcx.Type == (int)TmpType.小程序多门店模板)
                {
                    tranModel.Add(FootBathBLL.SingleModel.GetAddFootbathSQL(xcxappaccountrelation.AccountId, xcx.SCount - 1, xcxappaccountrelation.Id, xcxappaccountrelation.outtime).ToArray());
                }
            }

            //执行事务
            bool success = false;
            try
            {
                success = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception ex)
            {
                erroMsg = ex.Message;
            }
            return success;
        }

        /// <summary>
        /// 获取带类型的模板信息，
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public XcxAppAccountRelation GetModelByAppid(string appid,bool isOutTime=true)
        {
            XcxAppAccountRelation model = new XcxAppAccountRelation();
            string sql = $"select *,(select type from xcxtemplate where id=x.tid) xcxtype from XcxAppAccountRelation x where appid ='{appid}' and state>=0";
            if(isOutTime)
            {
                sql += $" and outtime>'{DateTime.Now.ToString("yyyy - MM - dd HH: mm: ss")}'";
            }
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    model = base.GetModel(dr);
                    if(dr["xcxtype"]!=DBNull.Value)
                    {
                        model.Type = Convert.ToInt32(dr["xcxtype"]);
                    }
                }
            }
            return model;
        }

        /// <summary>
        /// 注册赠送模板
        /// </summary>
        /// <param name="accountModel"></param>
        public void AddFreeTemplate(Account accountModel)
        {
            string typeids = $"{(int)TmpType.小程序单页模板},{(int)TmpType.小程序企业模板},{(int)TmpType.小程序专业模板}";
            List<XcxTemplate> templist = XcxTemplateBLL.SingleModel.GetListByTypes(typeids);
            if (templist == null || templist.Count <= 0 || accountModel == null)
                return;

            XcxAppAccountRelation usertemplate = GetModelByaccound(accountModel.Id.ToString());
            if (usertemplate != null)
            {
                return;
            }

            TransactionModel tran = new TransactionModel();
            DateTime nowtime = DateTime.Now;
            int version = 0;
            int month = 100*12;
            foreach (XcxTemplate item in templist)
            {
                version = 0;
                if (item.Type == (int)TmpType.小程序专业模板)
                {
                    version = 3;
                    month = 3;
                }
                else
                {
                    month = 100 * 12;
                }
                tran.Add($@"insert into XcxAppAccountRelation(TId,AccountId,AddTime,Url,price,outtime,agentid,VersionId) 
            values({item.Id}, '{accountModel.Id}', '{nowtime}', '{item.Link}', {item.Price}, '{nowtime.AddMonths(month)}',0,{version})");

                AgentdepositLog pricemodellog = new AgentdepositLog();
                pricemodellog.addtime = DateTime.Now;
                pricemodellog.afterDeposit = 0;
                pricemodellog.agentid = 0;
                pricemodellog.beforeDeposit = 0;
                pricemodellog.cost = 0;
                pricemodellog.costdetail = $"客户免费使用小程序模板：{(version == 3 ? "专业基础版" : item.TName)}";
                pricemodellog.type = 0;
                pricemodellog.tid = item.Id;

                tran.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(pricemodellog));
            }

            if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "赠送免费版：失败，" + Newtonsoft.Json.JsonConvert.SerializeObject(tran));
            }
        }

        /// <summary>
        /// 赠送代理平台模板
        /// </summary>
        /// <param name="accountModel"></param>
        public void AddTemplate(Account accountModel, Agentinfo agentInfo)
        {
            if (agentInfo == null && agentInfo.userLevel==0)
                return;

            string typeids = $"{(int)TmpType.小未平台}";
            List<XcxTemplate> templist = XcxTemplateBLL.SingleModel.GetListByTypes(typeids);
            if (templist == null || templist.Count <= 0 || accountModel == null)
                return;

            List<XcxAppAccountRelation> usertemplate = GetListByTidAccountId($"'{accountModel.Id.ToString()}'",templist[0].Id);
            if (usertemplate != null && usertemplate.Count>0)
            {
                return;
            }

            TransactionModel tran = new TransactionModel();
            DateTime nowtime = DateTime.Now;
            foreach (XcxTemplate item in templist)
            {
                tran.Add($@"insert into XcxAppAccountRelation(TId,AccountId,AddTime,Url,price,outtime,agentid) 
            values({item.Id}, '{accountModel.Id}', '{nowtime}', '{item.Link}', {item.Price}, '{agentInfo.addtime.AddYears(1)}',0)");

                AgentdepositLog pricemodellog = new AgentdepositLog();
                pricemodellog.addtime = DateTime.Now;
                pricemodellog.afterDeposit = 0;
                pricemodellog.agentid = 0;
                pricemodellog.beforeDeposit = 0;
                pricemodellog.cost = 0;
                pricemodellog.costdetail = $"客户免费使用小程序模板：" + item.TName;
                pricemodellog.type = 0;
                pricemodellog.tid = item.Id;

                tran.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(pricemodellog));
            }

            if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "赠送免费版：失败，" + Newtonsoft.Json.JsonConvert.SerializeObject(tran));
            }
        }

        /// <summary>
        /// 获取小程序名称
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public string GetAppName(int aid,XcxAppAccountRelation model=null, UserXcxTemplate userxcxTmpelate=null,OpenAuthorizerConfig openModel=null)
        {
            if(model==null)
            {
                if (aid <= 0)
                    return "";

                model = base.GetModel(aid);
            }

            if (model == null)
                return "";
            
            if (model.AuthoAppType == 0)
            {
                if(userxcxTmpelate==null)
                    userxcxTmpelate = UserXcxTemplateBLL.SingleModel.GetModelByAppId(model.AppId);
                return userxcxTmpelate?.Name;
            }
            else if (model.AuthoAppType == 1)
            {
                if (openModel == null)
                    openModel = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(model.AppId);
                return openModel?.nick_name;
            }

            return "";
        }
        
        public string GetStoreName(string name, XcxAppAccountRelation relationInfo)
        {
            //获取店铺名（兼容旧数据）
            if (string.IsNullOrEmpty(name))
            {
                return GetAppName(0, relationInfo);
            }

            return name;
        }
        #endregion

        #region 新订单提示处理
        public string CommandNewOrder(int xcxType,int aid, ref string path,ref string voicePath,ref int orderType)
        {
            switch (xcxType)
            {
                case (int)TmpType.小程序餐饮模板:
                    path = "/foods/FoodGoodsOrderList?appId=" + aid;
                    Food food = FoodBLL.SingleModel.GetModelByAppId(aid);
                    if (food == null)
                        return "";
                    if (!food.OpenNewOrderPrompt)
                        return "";
                    if (food.VoiceType == 1)
                    {
                        voicePath = food.VoiceUrl;
                    }
                    break;

                case (int)TmpType.小程序专业模板:
                    path = $"/enterprisepro/OrderTotalList?appId={aid}&tab=1";
                    Store entstore = StoreBLL.SingleModel.GetModelByAId(aid);
                    if (entstore == null)
                        return "店铺不存在";

                    if (!entstore.OpenNewOrderPrompt)
                        return "未开启提示音";

                    if (entstore.VoiceType == 1)
                    {
                        voicePath = entstore.VoiceUrl;
                    }
                    break;
            }

            return "";
        }
        public string IsHaveNewOrder(int aid,ref string content,ref int orderType)
        {
            string havestr = Utils.IsHaveNewOrder(aid, 0);
            if (!string.IsNullOrEmpty(havestr))
            {
                string[] msgattr = havestr.Split('_');
                if (msgattr.Length > 1)
                {
                    switch (Convert.ToInt32(msgattr[1]))
                    {
                        case (int)EntGoodsType.普通产品: content = "您有一个新的订单，请及时处理"; break;
                        case (int)EntGoodsType.预约商品: content = "您有一个新的产品预约单，请及时处理"; break;
                    }
                }
                
                Utils.RemoveIsHaveNewOrder(aid, 0, "");
                return "";
            }
            else
            {
                return "没有新订单";
            }
        }
        #endregion

        #region 缓存操作
        public void RemoveVersion(int agentId)
        {
            string key = string.Format(_redis_XcxAppAccountRelationModelVersion, agentId);
            RedisUtil.SetVersion(key);
        }
        public void RemoveRedis(int id)
        {
            string key = string.Format(_redis_XcxAppAccountRelationModelKey, id);
            RedisUtil.Remove(key);
        }

        public override XcxAppAccountRelation GetModel(int Id)
        {
            string key = string.Format(_redis_XcxAppAccountRelationModelKey, Id);
            XcxAppAccountRelation model = RedisUtil.Get<XcxAppAccountRelation>(key);
            if (model == null)
            {
                model = base.GetModel(Id);
                
                RedisUtil.Set<XcxAppAccountRelation>(key, model, TimeSpan.FromHours(1));
            }
            else
            {
                if(model.agentId>0)
                {
                    string versionKey = string.Format(_redis_XcxAppAccountRelationModelVersion, model.agentId);
                    int version = RedisUtil.GetVersion(versionKey);
                    if (version != model.Version)
                    {
                        model = base.GetModel(Id);
                        model.Version = version;
                        RedisUtil.Set<XcxAppAccountRelation>(key, model, TimeSpan.FromHours(1));
                    }
                }
            }

            return model;
        }

        public override bool Update(XcxAppAccountRelation model)
        {
            bool success = base.Update(model); 
            if(success)
            {
                RemoveRedis(model.Id);
            }

            return success;
        }

        public override bool Update(XcxAppAccountRelation model, string columnFields)
        {
            bool success = base.Update(model, columnFields);
            if (success)
            {
                RemoveRedis(model.Id);
            }

            return success;
        }
        #endregion
    }
}

