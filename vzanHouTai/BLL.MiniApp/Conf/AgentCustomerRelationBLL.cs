using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.FunList;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Fds;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.User;
using Entity.MiniApp.ViewModel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Conf
{
    public class AgentCustomerRelationBLL : BaseMySql<AgentCustomerRelation>
    {
        #region 单例模式
        private static AgentCustomerRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentCustomerRelationBLL()
        {

        }

        public static AgentCustomerRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentCustomerRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<AgentCustomerRelation> GetListByAgentId(int agentId)
        {
            return base.GetList($"agentid={agentId}");
        }

        public List<AgentCustomerRelation> GetListByQrCodeId(string qrcodeId)
        {
            return base.GetList($"QrcodeId in ({qrcodeId})");
        }

        public List<AgentCustomerRelation> GetListByAccountId(string accountId)
        {
            return base.GetList($"useraccountid ='{accountId}'");
        }

        public List<AgentCustomerRelation> GetListByAccountIds(string accountIds)
        {
            if (accountIds == null || accountIds == "")
                return new List<AgentCustomerRelation>();

            return base.GetList($"useraccountid in ({accountIds})");
        }

        public AgentCustomerRelation GetModelByUsername(string userName)
        {
            MySqlParameter[] paras = { new MySqlParameter("@username", userName) };
            return GetModel(" username=@username", paras);
        }

        public AgentCustomerRelation GetModelByIdAndAgentId(int id,int agentId)
        {
            return base.GetModel($"id={id} and agentid='{agentId}' and state=1");
        }

        public AgentCustomerRelation GetModelByAccountId(int agentid, string accountid)
        {
            string sqlwhere = $" useraccountid=@AccountId and agentid={agentid}";
            List<MySqlParameter> parm = new List<MySqlParameter>();
            parm.Add(new MySqlParameter("@AccountId", accountid));
            return base.GetModel(sqlwhere, parm.ToArray());
        }

        public AgentCustomerRelation GetModelByAccountId(int agentId, string accountId, string userName)
        {
            string sqlWhere = $"agentid={agentId} ";

            string otherSqlWhere = "";
            List<MySqlParameter> parm = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(accountId))
            {
                otherSqlWhere += "useraccountid=@AccountId";
                parm.Add(new MySqlParameter("@AccountId", accountId));
            }
            if (!string.IsNullOrEmpty(userName))
            {
                if (otherSqlWhere.Length > 0)
                {
                    otherSqlWhere += " or username=@username";
                }
                else
                {
                    otherSqlWhere = " username=@username";
                }
                parm.Add(new MySqlParameter("@username", userName));
            }

            if (otherSqlWhere.Length > 0)
            {
                sqlWhere += $" and ({otherSqlWhere})";
            }

            return base.GetModel(sqlWhere, parm.ToArray());
        }

        public List<AgentCustomerRelation> GetListByAccountId(string agentIds, string accountId, string userName)
        {
            string sqlWhere = $"agentid in ({agentIds}) ";

            string otherSqlWhere = "";
            List<MySqlParameter> parm = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(accountId))
            {
                otherSqlWhere += "useraccountid=@AccountId";
                parm.Add(new MySqlParameter("@AccountId", accountId));
            }
            if (!string.IsNullOrEmpty(userName))
            {
                if (otherSqlWhere.Length > 0)
                {
                    otherSqlWhere += " or username=@username";
                }
                else
                {
                    otherSqlWhere = " username=@username";
                }
                parm.Add(new MySqlParameter("@username", userName));
            }

            if (otherSqlWhere.Length > 0)
            {
                sqlWhere += $" and ({otherSqlWhere})";
            }

            return base.GetListByParam(sqlWhere, parm.ToArray());
        }

        /// <summary>
        /// 代理商修改模板绑定用户账号
        /// </summary>
        /// <param name="accountModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public bool UpdateCustomerLoginInfo(Account accountModel, AgentCustomerRelation customer, string oldAccountId, ref string erroMsg)
        {
            TransactionModel tran = new TransactionModel();

            string aids = "";
            //代理商操作更换账号
            if (customer != null)
            {
                tran.Add($"update AgentCustomerRelation set useraccountid='{accountModel.Id.ToString()}' where id={customer.id}");
                List<XcxAppAccountRelation> xList = XcxAppAccountRelationBLL.SingleModel.GetListByAccountId(customer.useraccountid, customer.agentid);
                if (xList != null && xList.Count > 0)
                {
                    tran.Add($"update xcxappaccountrelation set accountid = '{accountModel.Id.ToString()}' where accountid='{customer.useraccountid}' and agentId={customer.agentid}");
                    aids = string.Join(",", xList.Select(s => s.Id));
                    tran.Add($"update UserRole set userId = '{accountModel.Id.ToString()}' where  appid in ({aids})");
                }
            }
            else//用户自己操作更换账号
            {
                Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(accountModel.Id.ToString());
                if (agentinfo != null)
                {
                    erroMsg = "换绑的的用户不能是代理商";
                    return false;
                }
                tran.Add($"update AgentCustomerRelation set useraccountid='{accountModel.Id.ToString()}' where useraccountid='{oldAccountId}'");
                tran.Add($"update agentinfo set useraccountid='{accountModel.Id.ToString()}' where useraccountid='{oldAccountId}'");
                tran.Add($"update distribution set useraccountid='{accountModel.Id.ToString()}' where useraccountid='{oldAccountId}'");
                tran.Add($"update agentwebsiteinfo set userAccountId='{accountModel.Id.ToString()}' where userAccountId='{oldAccountId}'");
                tran.Add($"update agentwebsitequestion set userAccountId='{accountModel.Id.ToString()}' where userAccountId='{oldAccountId}'");
                List<XcxAppAccountRelation> xList = XcxAppAccountRelationBLL.SingleModel.GetListByAccountId(oldAccountId, -1);
                if (xList != null && xList.Count > 0)
                {
                    tran.Add($"update xcxappaccountrelation set accountid = '{accountModel.Id.ToString()}' where accountid='{oldAccountId}'");
                    aids = string.Join(",", xList.Select(s => s.Id));
                    tran.Add($"update UserRole set userId = '{accountModel.Id.ToString()}' where  appid in ({aids})");
                }
            }

            if (base.ExecuteTransaction(tran.sqlArray))
            {
                if (!string.IsNullOrEmpty(aids))
                {
                    return OpenAuthorizerConfigBLL.SingleModel.UpdateBindUserInfo(accountModel.Id.ToString(), oldAccountId, aids);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 客户管理-获取客户列表信息
        /// </summary>
        /// <param name="sqlwhere"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<CustomerModel> GetCustomerList(int agentId, string LoginId, string userName, string startTime, string endTime, string tids, int state, int pageSize, int pageIndex, ref int count, ref string msg, string ceshiType)
        {
            List<MySqlParameter> param = new List<MySqlParameter>();
            List<CustomerModel> list = new List<CustomerModel>();
            string sqlWhere = $" a.agentid = '{agentId}'";
            List<Account> userAccount = null;
            if (!string.IsNullOrEmpty(userName))
            {
                sqlWhere += $" and username like @username ";
                param.Add(new MySqlParameter("@username", "%" + userName + "%"));
            }
            if (!string.IsNullOrEmpty(startTime))
            {
                sqlWhere += $" and a.addtime>='{startTime} 00:00:00'";
            }
            if (!string.IsNullOrEmpty(endTime))
            {
                sqlWhere += $" and a.addtime<='{endTime} 23:59:59'";
            }
            if (state != 0)
            {
                sqlWhere += $" and a.state={state}";
            }
            if (!string.IsNullOrEmpty(tids))
            {
                sqlWhere += $" and tid in ({tids}) and b.state<>-2";
            }
            if (!string.IsNullOrEmpty(LoginId))
            {
                userAccount = AccountBLL.SingleModel.GetModelByLoginidL(LoginId);

                if (userAccount == null)
                    return null;
                string userIds = "'" + string.Join("','", userAccount.Select(s => s.Id).Distinct()) + "'";
                sqlWhere += $" and useraccountid in ({userIds})";
            }
            try
            {
                string sql = $"select a.* from AgentCustomerRelation a left join XcxAppAccountRelation b on a.useraccountid = b.accountid and a.agentid=b.agentid  where {sqlWhere} group by useraccountid order by id desc";
                List<AgentCustomerRelation> customerList = base.GetListBySql($"{sql} limit {(pageIndex - 1) * pageSize},{pageSize}", param.ToArray());
                count = base.GetCountBySql($"select count(*) from ({sql}) as c", param.ToArray());
                if (customerList == null || customerList.Count <= 0)
                    return list;

                string accountIds = $"'{string.Join("','", customerList.Select(s => s.useraccountid))}'";
                List<Account> accountList = AccountBLL.SingleModel.GetListByAccountids(accountIds);

                List<XcxTemplate> xcxtempList = XcxTemplateBLL.SingleModel.GetAgentOpenXcxTemplateList(accountIds, ceshiType, agentId);

                foreach (AgentCustomerRelation info in customerList)
                {
                    Account account = accountList?.FirstOrDefault(f => f.Id.ToString() == info.useraccountid);
                    if (account == null)
                    {
                        continue;
                    }

                    CustomerModel model = new CustomerModel();
                    model.addtimeTostring = info.addtime.ToString("yyyy-MM-dd HH:mm:ss");
                    model.agentid = info.agentid;
                    model.id = info.id;
                    model.LoginId = account.LoginId;
                    model.state = info.state;

                    List<XcxTemplate> xcxRelationlist = xcxtempList?.Where(w => w.industr == info.useraccountid).ToList();
                    List<string> TNameList = new List<string>();
                    foreach (var item in xcxRelationlist)
                    {
                        TNameList.Add(item.TName);
                    }

                    model.templates = string.Join(",", TNameList.ToList());
                    model.Totalcost = xcxRelationlist.Sum(x => x.Price * x.year);
                    model.useraccountid = info.useraccountid;
                    model.username = info.username;
                    list.Add(model);
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
            return list;
        }

        /// <summary>
        /// 客户管理-后去客户已开通模板信息
        /// </summary>
        /// <param name="sqlwhere"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<OpenTemplateView> GetCustomerOpenTemplateList(int id, int agentId, string LoginId, string userName, string outTime, string templateName, int pageSize, int pageIndex, ref int count,string ceshiType)
        {
            List<OpenTemplateView> list = new List<OpenTemplateView>();
            List<MySqlParameter> param = new List<MySqlParameter>();
            string sqlWhere = "";
            if (id > 0)
            {
                sqlWhere += $" and xr.id={id}";
            }
            //string sqlwhere2 = "";
            if (!string.IsNullOrEmpty(userName))
            {
                AgentCustomerRelation dmodel = GetModelByUsername(userName);
                if (dmodel != null)
                {
                    param.Add(new MySqlParameter("@accountid2", dmodel.useraccountid));
                    sqlWhere += $" and xr.accountid=@accountid2";
                }
                else
                {
                    return null;
                }
            }
            if (!string.IsNullOrEmpty(outTime))
            {
                param.Add(new MySqlParameter("@outtime", outTime));
                sqlWhere += $" and xr.outtime<@outtime";
            }
            if (!string.IsNullOrEmpty(templateName))
            {
                param.Add(new MySqlParameter("@templatename", templateName));
                sqlWhere += $" and xc.tname=@templatename";
            }
            
            if (!string.IsNullOrEmpty(LoginId))
            {
                Account accountmodel = AccountBLL.SingleModel.GetModelByLoginid(LoginId);
                if (accountmodel != null)
                {
                    param.Add(new MySqlParameter("@accountid", accountmodel.Id.ToString()));
                    sqlWhere += $" and xr.accountid=@accountid";
                }
                else
                {
                    return null;
                }
            }
            
            string sql = string.Empty;
            try
            {
                sql = $@"select xr.id, 
                            (select username from agentcustomerrelation where useraccountid=xr.accountid and agentid={agentId} LIMIT 1) username,xr.scount,
                            xr.accountid useraccountid,
                            xr.outtime,xr.addtime,xr.price,xc.tname,xr.state,xr.IsExperience,xc.price as singleprice,xc.id as tid,xc.type,xc.sprice,xr.versionid,xr.appid,xr.Id as xcxRelationId from xcxappaccountrelation xr
                            left join xcxtemplate xc on xc.id = xr.tid
                            where xr.state<>-2 and xr.agentid = {agentId} and (xc.projecttype in ({(int)ProjectType.小程序},{(int)ProjectType.测试}) {(ceshiType.Length>0? $" or xc.type in({ceshiType})" : "")}){sqlWhere} order by xr.AddTime desc";

                count = base.GetCountBySql($"select count(*) from ({sql}) as c", param.ToArray());

                sql = $"{sql} limit {(pageIndex - 1) * pageSize},{pageSize}";
                using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql, param.ToArray()))
                {
                    while (dr.Read())
                    {
                        OpenTemplateView model = new OpenTemplateView();
                        model.username = dr["username"].ToString();
                        model.outtime = DateTime.Parse(dr["outtime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                        model.addtime = dr["addtime"].ToString();
                        model.price = Convert.ToInt32(dr["price"].ToString());
                        model.type = Convert.ToInt32(dr["type"].ToString());
                        string TName = string.Empty;
                        model.XcxRelationId = Convert.ToInt32(dr["xcxRelationId"]);
                        if (model.type == 22)
                        {
                            model.VersionId = Convert.ToInt32(dr["versionid"]);
                            TName = AgentdepositLogBLL.SingleModel.GetVerName(model.VersionId);
                        }
                        if(dr["appid"]!=DBNull.Value)
                        {
                            model.AppId = dr["appid"].ToString();
                        }
                        model.tname = dr["tname"].ToString() + TName;
                        model.state = Convert.ToInt32(dr["state"].ToString());
                        model.singleprice = Convert.ToInt32(dr["singleprice"].ToString());
                        model.tid = Convert.ToInt32(dr["tid"].ToString());
                        model.IsExperience = Convert.ToBoolean(dr["IsExperience"]);
                        
                        DateTime tempdate;
                        if (DateTime.TryParse(model.outtime, out tempdate))
                        {
                            if (model.state >= 0)
                            {
                                if (DateTime.Now.CompareTo(tempdate) > 0)
                                {
                                    model.state = 2;
                                }
                                else
                                {
                                    model.state = 1;
                                }
                            }

                            //有效天数
                            model.validday = DateHelper.GetDays(tempdate, DateTime.Now);
                        }
                        else
                        {
                            model.state = 3;
                        }
                        model.id = Convert.ToInt32(dr["id"].ToString());
                        model.accountid = dr["useraccountid"].ToString();
                        if (DBNull.Value != dr["sprice"])
                        {
                            model.sprice = Convert.ToInt32(dr["sprice"]);
                        }
                        if (DBNull.Value != dr["scount"])
                        {
                            model.scount = Convert.ToInt32(dr["scount"]);
                        }

                        list.Add(model);
                    }
                }

                if (list != null && list.Count > 0)
                {
                    //是否已开启水印
                    string aids = string.Join(",",list.Select(s=>s.XcxRelationId));
                    List<ConfParam> parmList = ConfParamBLL.SingleModel.GetListByRIdAndParam(aids, "'agentcustomlogo','agentcustomtitle','agentcustomhost','agentsystemlogo'");

                    //已授权小程序信息
                    string appids = "'" + string.Join("','",list.Where(w=>w.AppId.Length>0)?.Select(s=>s.AppId))+"'";
                    List<OpenAuthorizerConfig> openAuthorizerList = OpenAuthorizerConfigBLL.SingleModel.GetListByAppIds(appids);

                    string accountIds = "'" + string.Join("','", list.Select(s => s.accountid).Distinct()) + "'";
                    List<Account> accountList = AccountBLL.SingleModel.GetListByAccountids(accountIds);
                    List<Xcxtemplate_Price> templatePrice = Xcxtemplate_PriceBLL.SingleModel.GetListByAgentId(agentId);
                    foreach (OpenTemplateView item in list)
                    {
                        //小程序昵称
                        OpenAuthorizerConfig tempInfo = openAuthorizerList?.FirstOrDefault(f=>f.appid == item.AppId);
                        item.NickName = tempInfo==null?"":$"【{tempInfo.nick_name}】";

                        //是否开启水印
                        ConfParam tempConfParam = parmList?.Where(w=>w.RId == item.XcxRelationId).FirstOrDefault();
                        item.OpenCustomBottom = tempConfParam == null ? 0 : 1;
                        item.ConfParamList = parmList?.Where(w => w.RId == item.XcxRelationId).ToList();

                        Account model = accountList?.Where(w => w.Id.ToString() == item.accountid).FirstOrDefault();
                        item.LoginId = model?.LoginId;

                        Xcxtemplate_Price priceModel = templatePrice?.Where(w => w.tid == item.tid.ToString()).FirstOrDefault();
                        if (priceModel != null)
                        {
                            item.singleprice = priceModel.price;
                            item.sprice = priceModel.SPrice;
                        }
                        //开通单个门店价格
                        if (item.type == (int)TmpType.小程序多门店模板)
                        {
                            int scount = FootBathBLL.SingleModel.GetCount($"appId={item.id}");
                            item.scount = scount;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
            return list;
        }
        
        /// <summary>
        /// 首页-获取累计客户
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public int GetCustomerCount(int agentId)
        {
            string where = $"agentid='{agentId}'";
            return GetCount(where);
        }
        
        public bool AddCustomer(string userName, int province, int city, int area, int industryId, int companyscaleId, string remark, Agentinfo agentinfo, List<XcxTemplate> xcxList, int cost, int parentCost, Account userAccount, ref string errorMsg)
        {
            TransactionModel tranModel = new TransactionModel();
            MySqlParameter[] pone = null;
            Agentinfo parentAgentinfo = null;
            int beforeDeposit = agentinfo.deposit;
            int pbeforeDeposit = 0;
            //扣费
            agentinfo.deposit = agentinfo.deposit - cost;
            tranModel.Add($"UPDATE Agentinfo set deposit={agentinfo.deposit} ,updateitme='{DateTime.Now}' where id={agentinfo.id}");
            if (agentinfo.userLevel == 1)
            {
                Distribution distribution = DistributionBLL.SingleModel.GetModel($"agentId={agentinfo.id}");
                parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                pbeforeDeposit = parentAgentinfo.deposit;
                parentAgentinfo.deposit = parentAgentinfo.deposit - parentCost;
                tranModel.Add($"UPDATE Agentinfo set deposit={parentAgentinfo.deposit} ,updateitme='{DateTime.Now}' where id={parentAgentinfo.id}");
            }

            //添加客户
            AgentCustomerRelation customerRelation = base.GetModel($"useraccountid='{userAccount.Id}' and agentid='{agentinfo.id}'");
            if (customerRelation == null)
            {
                customerRelation = new AgentCustomerRelation()
                {
                    username = userName,
                    provincecode = province,
                    citycode = city,
                    areacode = area,
                    companyscaleid = companyscaleId,
                    remark = remark,
                    addtime = DateTime.Now,
                    agentid = agentinfo.id,
                    useraccountid = userAccount.Id.ToString(),
                    state = 1,
                };
                tranModel.Add(base.BuildAddSql(customerRelation, out pone), pone);
            }
            else
            {
                tranModel.Add(base.BuildUpdateSql(customerRelation));
            }

            //添加模板记录
            foreach (var xcx in xcxList)
            {
                //开通免费版不能超过超过限制数量
                if (xcx.Price <= 0)
                {
                    if (xcx.LimitCount > 0 && xcx.buycount > xcx.LimitCount)
                    {
                        errorMsg = $"免费开通模板已超过{xcx.LimitCount}，请联系客服！";
                        return false;
                    }

                    int maxCount = XcxAppAccountRelationBLL.SingleModel.GetOpenTemplateMaxCount(userAccount.Id.ToString(), xcx.Id);
                    //免费模板开通不能超过限制数量
                    if (xcx.LimitCount > 0 && maxCount + xcx.buycount > xcx.LimitCount)
                    {
                        errorMsg = $"免费开通模板已超过{xcx.LimitCount}，请联系客服！";
                        return false;
                    }
                    //专业基础版默认可开通最大数量 xcx.LimitCount=0表示不限制
                    int maxVersionCount = xcx.LimitCount > 0 ? xcx.LimitCount : 100;
                    
                    if (maxCount + xcx.buycount > maxVersionCount && xcx.VersionId == 3)
                    {
                        errorMsg = $"专业基础版免费开通模板已超过{maxVersionCount}个，请联系客服！";
                        return false;
                    }
                }

                for (int i = 0; i < xcx.buycount; i++)
                {
                    XcxAppAccountRelation xcxAppaccountRelation = new XcxAppAccountRelation()
                    {
                        TId = xcx.Id,
                        AccountId = userAccount.Id,
                        AddTime = DateTime.Now,
                        agentId = agentinfo.id,
                        Url = xcx.Link,
                        price = xcx.Price,
                        TimeLength = xcx.year,
                        outtime = DateTime.Now.AddYears(xcx.year),
                        Industr = xcx.industr,
                        SCount = xcx.storecount,
                        VersionId = xcx.VersionId,
                    };

                    xcxAppaccountRelation.Id = Convert.ToInt32(XcxAppAccountRelationBLL.SingleModel.Add(xcxAppaccountRelation));
                    if (xcx.Type == (int)TmpType.小程序多门店模板)
                    {
                        tranModel.Add(FootBathBLL.SingleModel.GetAddFootbathSQL(xcxAppaccountRelation.AccountId, xcx.storecount - 1, xcxAppaccountRelation.Id, xcxAppaccountRelation.outtime).ToArray());
                    }
                    else if (xcx.Type == (int)TmpType.小程序餐饮多门店模板)
                    {
                        tranModel.Add(FoodBLL.SingleModel.GetAddFoodSQL(xcxAppaccountRelation.AccountId, xcx.storecount - 1, xcxAppaccountRelation.Id, xcxAppaccountRelation.outtime).ToArray());
                    }
                    else if(xcx.Type== (int)TmpType.小程序足浴模板)
                    {
                        XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModelByType((int)TmpType.小程序足浴技师端模板);
                        if(xcxtemplate!=null)
                        {
                            xcxAppaccountRelation = new XcxAppAccountRelation()
                            {
                                TId = xcxtemplate.Id,
                                AccountId = userAccount.Id,
                                AddTime = DateTime.Now,
                                agentId = agentinfo.id,
                                Url = xcxtemplate.Link,
                                price = 0,
                                TimeLength = 100,
                                outtime = DateTime.Now.AddYears(100),
                            };
                            xcxAppaccountRelation.Id = Convert.ToInt32(XcxAppAccountRelationBLL.SingleModel.Add(xcxAppaccountRelation));
                        }
                    }
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
                throw new Exception(ex.Message);
            }

            if (success)
            {
                //记录流水
                AgentdepositLogBLL.SingleModel.AddagentinfoLog(agentinfo, xcxList, beforeDeposit, pbeforeDeposit, userName, userAccount.Id.ToString());
            }

            return success;
        }

        public bool AddCustomerV2(string userName, int province, int city, int area,int companyscaleId, string remark, Agentinfo agentInfo, List<XcxTemplate> xcxList, int cost, int parentCost, Account userAccount, Distribution distribution, Agentinfo pagentInfo, List<XcxTemplate> pxcxList, ref string errorMsg)
        {
            TransactionModel tranModel = new TransactionModel();
            MySqlParameter[] pone = null;
            
            //添加客户
            AgentCustomerRelation customerRelation = base.GetModel($"useraccountid='{userAccount.Id}' and agentid='{agentInfo.id}'");
            if (customerRelation == null)
            {
                customerRelation = new AgentCustomerRelation()
                {
                    username = userName,
                    provincecode = province,
                    citycode = city,
                    areacode = area,
                    companyscaleid = companyscaleId,
                    remark = remark,
                    addtime = DateTime.Now,
                    agentid = agentInfo.id,
                    useraccountid = userAccount.Id.ToString(),
                    state = 1,
                };
                tranModel.Add(base.BuildAddSql(customerRelation, out pone), pone);
            }
            else
            {
                tranModel.Add(base.BuildUpdateSql(customerRelation));
            }

            //添加模板记录
            foreach (var xcx in xcxList)
            {
                //开通免费版不能超过超过限制数量
                if (xcx.Price <= 0)
                {
                    if (xcx.LimitCount > 0 && xcx.buycount > xcx.LimitCount)
                    {
                        errorMsg = $"免费开通模板已超过{xcx.LimitCount}，请联系客服！";
                        return false;
                    }

                    int maxCount = XcxAppAccountRelationBLL.SingleModel.GetOpenTemplateMaxCount(userAccount.Id.ToString(), xcx.Id);
                    //免费模板开通不能超过限制数量
                    if (xcx.LimitCount > 0 && maxCount + xcx.buycount > xcx.LimitCount)
                    {
                        errorMsg = $"免费开通模板已超过{xcx.LimitCount}，请联系客服！";
                        return false;
                    }
                    //专业基础版默认可开通最大数量 xcx.LimitCount=0表示不限制
                    int maxVersionCount = xcx.LimitCount > 0 ? xcx.LimitCount : 100;

                    if (maxCount + xcx.buycount > maxVersionCount && xcx.VersionId == 3)
                    {
                        errorMsg = $"专业基础版免费开通模板已超过{maxVersionCount}个，请联系客服！";
                        return false;
                    }
                }

                for (int i = 0; i < xcx.buycount; i++)
                {
                    XcxAppAccountRelation xcxAppaccountRelation = new XcxAppAccountRelation()
                    {
                        TId = xcx.Id,
                        AccountId = userAccount.Id,
                        AddTime = DateTime.Now,
                        agentId = agentInfo.id,
                        Url = xcx.Link,
                        price = xcx.Price,
                        TimeLength = xcx.year,
                        outtime = DateTime.Now.AddYears(xcx.year),
                        Industr = xcx.industr,
                        SCount = xcx.storecount,
                        VersionId = xcx.VersionId,
                    };

                    xcxAppaccountRelation.Id = Convert.ToInt32(XcxAppAccountRelationBLL.SingleModel.Add(xcxAppaccountRelation));
                    if (xcx.Type == (int)TmpType.小程序多门店模板)
                    {
                        tranModel.Add(FootBathBLL.SingleModel.GetAddFootbathSQL(xcxAppaccountRelation.AccountId, xcx.storecount - 1, xcxAppaccountRelation.Id, xcxAppaccountRelation.outtime).ToArray());
                    }
                    else if (xcx.Type == (int)TmpType.小程序餐饮多门店模板)
                    {
                        tranModel.Add(FoodBLL.SingleModel.GetAddFoodSQL(xcxAppaccountRelation.AccountId, xcx.storecount - 1, xcxAppaccountRelation.Id, xcxAppaccountRelation.outtime).ToArray());
                    }
                    else if (xcx.Type == (int)TmpType.小程序足浴模板)
                    {
                        XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModelByType((int)TmpType.小程序足浴技师端模板);
                        if (xcxtemplate != null)
                        {
                            xcxAppaccountRelation = new XcxAppAccountRelation()
                            {
                                TId = xcxtemplate.Id,
                                AccountId = userAccount.Id,
                                AddTime = DateTime.Now,
                                agentId = agentInfo.id,
                                Url = xcxtemplate.Link,
                                price = 0,
                                TimeLength = 100,
                                outtime = DateTime.Now.AddYears(100),
                            };
                            xcxAppaccountRelation.Id = Convert.ToInt32(XcxAppAccountRelationBLL.SingleModel.Add(xcxAppaccountRelation));
                        }
                    }
                }
            }
            
            //扣费
            tranModel.Add($"UPDATE Agentinfo set deposit={agentInfo.deposit - cost} ,updateitme='{DateTime.Now}' where id={agentInfo.id}");
            //记录流水
            if (!AgentdepositLogBLL.SingleModel.AddagentinfoLogV2(agentInfo, distribution, xcxList, userName, userAccount.Id.ToString(), ref tranModel, (int)AgentDepositLogType.开通客户模板))
            {
                errorMsg = "记录流水失败";
                return false;
            }

            //分销商扣费
            if (agentInfo.userLevel == 1)
            {
                tranModel.Add($"UPDATE Agentinfo set deposit={pagentInfo.deposit - parentCost} ,updateitme='{DateTime.Now}' where id={pagentInfo.id}");

                if (!AgentdepositLogBLL.SingleModel.AddagentinfoLogV2(pagentInfo, distribution, pxcxList, userName, userAccount.Id.ToString(), ref tranModel,(int)AgentDepositLogType.分销商开通客户模板, 0,1))
                {
                    errorMsg = "记录代理消费流水失败";
                    return false;
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
                throw new Exception(ex.Message);
            }
            
            return success;
        }

        public bool EditCustomer(string userName, int province, int city, int area, int industryId, int companyscaleId, string remark, Agentinfo agentinfo, List<XcxTemplate> list, int cost, int parentCost, AgentCustomerRelation customer, Account account, ref string erromsg)
        {
            TransactionModel tranModel = new TransactionModel();
            MySqlParameter[] pone = null;
            bool noChange = true;
            int beforeDeposit = agentinfo.deposit;
            int pbeforeDeposit = 0;
            //扣费
            if (cost > 0)
            {
                noChange = false;
                agentinfo.deposit = agentinfo.deposit - cost;
                tranModel.Add($"UPDATE Agentinfo set deposit={agentinfo.deposit} ,updateitme='{DateTime.Now}' where id={agentinfo.id}");
            }
            if (agentinfo.userLevel == 1)
            {
                noChange = false;
                Distribution distribution = DistributionBLL.SingleModel.GetModel($"agentId={agentinfo.id}");
                Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                pbeforeDeposit = parentAgentinfo.deposit;
                parentAgentinfo.deposit = parentAgentinfo.deposit - parentCost;
                tranModel.Add($"UPDATE Agentinfo set deposit={parentAgentinfo.deposit} ,updateitme='{DateTime.Now}' where id={parentAgentinfo.id}");
            }
            //更新用户数据
            if (customer.username != userName || customer.provincecode != province || customer.citycode != city || customer.areacode != area || customer.industryid != industryId || customer.companyscaleid != companyscaleId || customer.remark != remark)
            {
                noChange = false;
                customer.username = userName;
                customer.provincecode = province;
                customer.citycode = city;
                customer.areacode = area;
                customer.industryid = industryId;
                customer.companyscaleid = companyscaleId;
                customer.remark = remark;
                customer.updatetime = DateTime.Now;
                tranModel.Add(base.BuildUpdateSql(customer));
            }
            //添加模板记录
            if (list != null && list.Count > 0)
            {
                noChange = false;
                foreach (XcxTemplate xcx in list)
                {
                    //开通免费版不能超过超过限制数量
                    if (xcx.Price <= 0)
                    {
                        if (xcx.buycount > xcx.LimitCount)
                        {
                            erromsg = $"免费开通模板已超过{xcx.LimitCount}，请联系客服！";
                            return false;
                        }

                        int maxcount = XcxAppAccountRelationBLL.SingleModel.GetOpenTemplateMaxCount(account.Id.ToString(), xcx.Id);
                        //免费模板开通不能超过限制数量
                        if (maxcount + xcx.buycount > xcx.LimitCount)
                        {
                            erromsg = $"免费开通模板已超过{xcx.LimitCount}，请联系客服！";
                            return false;
                        }
                        //专业基础版默认可开通最大数量
                        int maxVersionCount = xcx.LimitCount > 0 ? xcx.LimitCount : 100;
                        if (maxcount + xcx.buycount > maxVersionCount && xcx.VersionId == 3)
                        {
                            erromsg = $"专业基础版免费开通模板已超过{maxVersionCount}个，请联系客服！";
                            return false;
                        }
                    }

                    for (int i = 0; i < xcx.buycount; i++)
                    {
                        XcxAppAccountRelation xcxappaccountrelation = new XcxAppAccountRelation()
                        {
                            TId = xcx.Id,
                            AccountId = account.Id,
                            AddTime = DateTime.Now,
                            agentId = agentinfo.id,
                            Url = xcx.Link,
                            price = xcx.Price,
                            TimeLength = xcx.year,
                            outtime = DateTime.Now.AddYears(xcx.year),
                            Industr = xcx.industr,
                        };
                        tranModel.Add(XcxAppAccountRelationBLL.SingleModel.BuildAddSql(xcxappaccountrelation, out pone), pone);
                    }
                }
            }
            if (noChange) return true;//没有数据修改
            //执行事务
            bool success = false;
            try
            {
                success = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
            }
            if (success)
            {
                //记录流水
                AgentdepositLogBLL.SingleModel.AddagentinfoLog(agentinfo, list, beforeDeposit, pbeforeDeposit, userName, account.Id.ToString());
            }
            return success;
        }
        
        /// <summary>
        /// 根据代理id获取代理创建的客户信息
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public List<CustomerModel> GetCustomerByAgentId(int agentId, int pageIndex, int pageSize, out int count)
        {
            List<CustomerModel> list = null;
            List<XcxAppAccountRelation> relationList = XcxAppAccountRelationBLL.SingleModel.GetList($"agentId={agentId}", pageSize, pageIndex, "*", "id desc");
            count = XcxAppAccountRelationBLL.SingleModel.GetCount($"agentId={agentId}");
            if (relationList == null || relationList.Count <= 0)
                return list;

            string tids = string.Join(",",relationList.Select(s=>s.TId).Distinct());
            List<XcxTemplate> xcxTemplateList = XcxTemplateBLL.SingleModel.GetListByIds(tids);

            string accountIds = $"'{string.Join("','", relationList.Select(s => s.AccountId).Distinct())}'";
            List<Account> accountList = AccountBLL.SingleModel.GetListByAccountids(accountIds);

            List<AgentCustomerRelation> agentCustomerRelationList = GetListByAccountIds(accountIds);

            list = new List<CustomerModel>();
            foreach (XcxAppAccountRelation relation in relationList)
            {
                XcxTemplate template = xcxTemplateList?.FirstOrDefault(f=>f.Id == relation.TId);
                if (template == null)
                    template = new XcxTemplate();
                Account account = accountList?.FirstOrDefault(f=>f.Id == relation.AccountId);
                AgentCustomerRelation agentCustomerRelation = agentCustomerRelationList?.FirstOrDefault(f=>f.useraccountid == relation.AccountId.ToString());

                CustomerModel model = new CustomerModel();
                model.LoginId = account?.LoginId;
                model.username = agentCustomerRelation?.username;
                model.templates = template.TName;
                model.price = relation.price;
                model.showPrice = (relation.price * 0.01).ToString("0.00");
                list.Add(model);
            }
            return list;
        }
        
        #region 续期
        public bool AddTimeLength(int id, int agentId, int years, string userName, string tname, ref string msg)
        {
            TransactionModel tranModel = new TransactionModel();

            //修改代理商预存款
            Agentinfo agentModel = AgentinfoBLL.SingleModel.GetModel(agentId);
            if (agentModel == null)
            {
                msg = "代理商信息异常";
                return false;
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
            if (xcxrelation == null)
            {
                msg = "用户模板不存在";
                return false;
            }
            xcxrelation.TimeLength += years;
            if (xcxrelation.outtime != null)
            {
                if (xcxrelation.outtime < DateTime.Now)
                {
                    xcxrelation.outtime = DateTime.Now;
                }
            }
            else
            {
                xcxrelation.outtime = DateTime.Now;
            }
            xcxrelation.outtime = xcxrelation.outtime.AddYears(years);
            xcxrelation.State = 1;
            xcxrelation.IsExperience = false;
            //修改用户权限模板记录
            tranModel.Add($"UPDATE XcxAppAccountRelation set outtime='{xcxrelation.outtime}',State={xcxrelation.State},TimeLength={xcxrelation.TimeLength},IsExperience=0 where Id={xcxrelation.Id}");

            //查询模板信息
            List<XcxTemplate> xcxtemplateList = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xcxrelation.TId})", agentModel.id);
            //var xcxtemplatemodel = _xcxtemplate_priceBll.GetModel($"tid={xrelationmodel.TId} and agentid={agentmodel.id}");

            if (xcxtemplateList == null && xcxtemplateList.Count <= 0)
            {
                msg = "找不到模板";
                return false;
            }
            #region 判断代理商合同是否已过期，已过期不给开免费版
            if (AgentinfoBLL.SingleModel.CheckOutTime(ref xcxtemplateList, agentModel, xcxrelation.VersionId, ref msg))
            {
                return false;
            }
            #endregion
            Xcxtemplate_Price xcxtemplateModel = new Xcxtemplate_Price();
            xcxtemplateModel.agentid = agentModel.id.ToString();
            if (xcxtemplateList[0].Type == 22)
            {
                //表示专业版里的级别 重新计算价格
                FunctionList model = FunctionListBLL.SingleModel.GetModelBytid(agentId, xcxtemplateList[0].Type, xcxrelation.TId, xcxrelation.VersionId);
                if (model == null)
                {
                    msg = $"专业模板设置加出错_{xcxrelation.TId}_{agentId}_{xcxrelation.VersionId}";
                    return false;
                }
                xcxtemplateModel.price = Convert.ToInt32(model.Price);
                xcxtemplateModel.tid = xcxrelation.TId.ToString();
            }
            else
            {
                xcxtemplateModel.price = xcxtemplateList[0].Price;
                xcxtemplateModel.tid = xcxtemplateList[0].Id.ToString();
            }
            var sum = (years * xcxtemplateModel.price);
            if (agentModel.deposit < sum)
            {
                msg = "预存款不足";
                return false;
            }

            //添加流水
            string dlog = $"客户:{userName}  续费模板:{tname} 续费时长{years}年";
            AgentdepositLog agentLog = new AgentdepositLog(Convert.ToInt32(xcxtemplateModel.tid), sum,agentModel, dlog,xcxrelation.AccountId.ToString(), agentModel.userLevel == 0 ? (int)AgentDepositLogType.代理商续费 : (int)AgentDepositLogType.分销商续费);
            tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLog));

            //扣费
            agentModel.deposit = agentModel.deposit - sum;
            tranModel.Add($"UPDATE Agentinfo set deposit={agentModel.deposit} ,updateitme='{DateTime.Now}' where id={agentModel.id}");
            //判断是否为分销商
            if (agentModel.userLevel == 1)
            {
                //扣除代理预存款
                Distribution distribution = DistributionBLL.SingleModel.GetModel($"agentId={agentModel.id}");
                if (distribution == null)
                {
                    msg = "分销商代理关系错误";
                    return false;
                }
                var parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                if (parentAgentinfo == null)
                {
                    msg = "分销商代理没找到";
                    return false;
                }

                //查询模板信息分销商的代理商模板设定价格
                List<XcxTemplate> pxcxtemplatelist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xcxrelation.TId})", parentAgentinfo.id);
                if (pxcxtemplatelist == null || pxcxtemplatelist.Count <= 0)
                {
                    msg = "没有找到代理设定的模板价格";
                    return false;
                }
                int psum = pxcxtemplatelist[0].Price * years;
                if (pxcxtemplatelist[0].Type == 22)
                {
                    //表示专业版里的级别 重新计算价格
                    Xcxtemplate_Price model = Xcxtemplate_PriceBLL.SingleModel.GetModelByAgentIdAndTid(parentAgentinfo.id, xcxrelation.TId, xcxrelation.VersionId);
                    if (model != null)
                    {
                        psum = Convert.ToInt32(model.price) * years;
                    }
                    else
                    {
                        FunctionList modelFunc = FunctionListBLL.SingleModel.GetModelBytid(parentAgentinfo.id, xcxtemplateList[0].Type, xcxrelation.TId, xcxrelation.VersionId);
                        if (modelFunc == null)
                        {
                            msg = $"(代理分销商)专业模板设置加出错_{xcxrelation.TId}_{parentAgentinfo.id}_{xcxrelation.VersionId}";
                            return false;
                        }
                        psum = Convert.ToInt32(modelFunc.Price) * years;
                    }
                }

                //代理商流水
                dlog = $"客户:{userName}  续费模板:{tname} 续费时长{years}年";
                AgentdepositLog agentLogmodel = new AgentdepositLog(Convert.ToInt32(xcxtemplateModel.tid),psum,parentAgentinfo,dlog,xcxrelation.AccountId.ToString(), (int)AgentDepositLogType.分销商续费);
                tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLogmodel));

                parentAgentinfo.deposit = parentAgentinfo.deposit - psum;
                tranModel.Add($"UPDATE Agentinfo set deposit={parentAgentinfo.deposit} ,updateitme='{DateTime.Now}' where id={parentAgentinfo.id}");
            }

            //执行事务
            bool success = false;
            try
            {
                success = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
                throw new Exception(ex.Message);
            }

            //清除缓存
            XcxAppAccountRelationBLL.SingleModel.RemoveRedis(xcxrelation.Id);
            msg = "续费成功";
            return success;
        }
        public bool AddTimeLengthV2(List<XcxTemplate> xcxtemplateList, Distribution distributionModel, int sum,int parentSum, XcxAppAccountRelation xcxrelation, int agentId, string userName, string tname, ref string msg)
        {
            TransactionModel tranModel = new TransactionModel();

            //修改代理商预存款
            Agentinfo agentModel = AgentinfoBLL.SingleModel.GetModel(agentId);
            if (agentModel == null)
            {
                msg = "代理商信息异常";
                return false;
            }
            
            xcxrelation.TimeLength += xcxtemplateList[0].year;
            if (xcxrelation.outtime != null)
            {
                if (xcxrelation.outtime < DateTime.Now)
                {
                    xcxrelation.outtime = DateTime.Now;
                }
            }
            else
            {
                xcxrelation.outtime = DateTime.Now;
            }
            xcxrelation.outtime = xcxrelation.outtime.AddYears(xcxtemplateList[0].year);
            xcxrelation.State = 1;
            xcxrelation.IsExperience = false;
            //修改用户权限模板记录
            tranModel.Add($"UPDATE XcxAppAccountRelation set outtime='{xcxrelation.outtime}',State={xcxrelation.State},TimeLength={xcxrelation.TimeLength},IsExperience=0 where Id={xcxrelation.Id}");
            
            if (agentModel.deposit < sum)
            {
                msg = "预存款不足";
                return false;
            }

            //添加流水
            string dlog = $"客户:{userName}  续费模板:{tname} 续费时长{xcxtemplateList[0].year}年";
            AgentdepositLog agentLog = new AgentdepositLog(Convert.ToInt32(xcxtemplateList[0].Id), sum, agentModel, dlog, xcxrelation.AccountId.ToString(), agentModel.userLevel == 0 ? (int)AgentDepositLogType.代理商续费 : (int)AgentDepositLogType.分销商续费);
            tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLog));

            //扣费
            agentModel.deposit = agentModel.deposit - sum;
            tranModel.Add($"UPDATE Agentinfo set deposit={agentModel.deposit} ,updateitme='{DateTime.Now}' where id={agentModel.id}");
            //判断是否为分销商
            if (agentModel.userLevel == 1)
            {
                Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distributionModel.parentAgentId);
                //代理商流水
                dlog = $"客户:{userName}  续费模板:{tname} 续费时长{xcxtemplateList[0].year}年";
                AgentdepositLog agentLogmodel = new AgentdepositLog(Convert.ToInt32(xcxtemplateList[0].Id), parentSum, parentAgentinfo, dlog, xcxrelation.AccountId.ToString(), (int)AgentDepositLogType.分销商续费);
                tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLogmodel));

                parentAgentinfo.deposit = parentAgentinfo.deposit - parentSum;
                tranModel.Add($"UPDATE Agentinfo set deposit={parentAgentinfo.deposit} ,updateitme='{DateTime.Now}' where id={parentAgentinfo.id}");
            }

            //执行事务
            bool success = false;
            try
            {
                success = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
                throw new Exception(ex.Message);
            }

            //清除缓存
            XcxAppAccountRelationBLL.SingleModel.RemoveRedis(xcxrelation.Id);
            msg = "续费成功";
            return success;
        }
        #endregion

        #region 专业版升级版本
        public bool UpEntProVersion(int id, int agentId, string userName, int newVersionId, string tname, ref string msg)
        {
            TransactionModel tranModel = new TransactionModel();

            //修改代理商预存款
            var agentModel = AgentinfoBLL.SingleModel.GetModel(agentId);
            if (agentModel == null)
            {
                msg = "代理商信息异常";
                return false;
            }

            XcxAppAccountRelation xrelationModel = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
            if (xrelationModel == null)
            {
                msg = "用户模板不存在";
                return false;
            }

            int oldVersionId = xrelationModel.VersionId;
            if (newVersionId > oldVersionId)  //3基础版 2 高级版 1尊享版 0旗舰版
            {
                msg = "只允许升级更高的版本";
                return false;
            }
            
            xrelationModel.outtime = DateTime.Now.AddYears(xrelationModel.TimeLength);
            xrelationModel.AddTime = DateTime.Now;//升级后新增时间为升级时间
            int oldPrice = xrelationModel.price;//之前开通该模板付出的费用
            VersionType model = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(22, agentId).FirstOrDefault(x => x.VersionId == newVersionId);
            //升级需要从代理商扣除的的钱 升级模板一年的价格*之前开通的年限 然后过期时间从升级当天往后推之前的年限
            int difference = Convert.ToInt32(model.VersionPrice) * xrelationModel.TimeLength;

            int newPrice = difference;
            if (agentModel.deposit < difference)
            {
                msg = "预存款不足";
                return false;
            }

            Dictionary<int, string> dictVersion = new Dictionary<int, string>
            {
                [0] = "旗舰版",
                [1] = "尊享版",
                [2] = "高级版",
                [3] = "基础版"
            };

            tname = tname.Replace("专业版", "");
            //添加流水
            string dlog = $"客户:{userName}  模板升级:由{tname}升级为{dictVersion[newVersionId]},升级需{difference * 0.01}元";
            AgentdepositLog agentLog = new AgentdepositLog(xrelationModel.TId,difference,agentModel,dlog,xrelationModel.AccountId.ToString(), agentModel.userLevel == 0 ? (int)AgentDepositLogType.代理商升级专业版版本 : (int)AgentDepositLogType.分销商升级专业版版本);

            tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLog));

            //扣费
            agentModel.deposit = agentModel.deposit - difference;
            tranModel.Add($"UPDATE Agentinfo set deposit={agentModel.deposit} ,updateitme='{DateTime.Now}' where id={agentModel.id}");
            //判断是否为分销商
            if (agentModel.userLevel == 1)
            {
                //扣除代理预存款
                Distribution distribution = DistributionBLL.SingleModel.GetModel($"agentId={agentModel.id}");
                if (distribution == null)
                {
                    msg = "分销商代理关系错误";
                    return false;
                }
                Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                if (parentAgentinfo == null)
                {
                    msg = "分销商代理没找到";
                    return false;
                }

                model = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(22, parentAgentinfo.id).FirstOrDefault(x => x.VersionId == newVersionId);
                //升级需要从分销商扣除的价
                difference = Convert.ToInt32(model.VersionPrice) * xrelationModel.TimeLength;
                newPrice = difference;
                if (parentAgentinfo.deposit < difference)
                {
                    msg = "分销商预存款不足";
                    return false;
                }
                
                //代理商流水
                dlog = $"客户:{userName}  模板升级:由{tname}升级为{dictVersion[newVersionId]},升级再需{difference * 0.01}元";
                AgentdepositLog agentLogmodel = new AgentdepositLog(xrelationModel.TId,difference,parentAgentinfo,dlog,xrelationModel.AccountId.ToString(),(int)AgentDepositLogType.分销商升级专业版版本);
                tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLogmodel));

                parentAgentinfo.deposit = parentAgentinfo.deposit - difference;
                tranModel.Add($"UPDATE Agentinfo set deposit={parentAgentinfo.deposit} ,updateitme='{DateTime.Now}' where id={parentAgentinfo.id}");
            }
            //修改用户权限模板记录
            tranModel.Add($"UPDATE XcxAppAccountRelation set price={newPrice},VersionId={newVersionId},outtime='{xrelationModel.outtime}',AddTime='{xrelationModel.AddTime}',IsExperience=0 where Id={xrelationModel.Id}");

            //执行事务
            bool success = false;
            try
            {
                success = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception ex)
            {
                //str = ex.Message + "|" + JsonConvert.SerializeObject(tranModel.sqlArray);
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
                throw new Exception(ex.Message);
            }

            //清除缓存
            XcxAppAccountRelationBLL.SingleModel.RemoveRedis(xrelationModel.Id);
            msg = "升级成功";
            return success;
        }
        #endregion
        
        #region 开分店
        public bool AddStore(string userName, XcxAppAccountRelation xcx, Agentinfo agentInfo, XcxTemplate xcxList, int cost, int parentCost, Account userAccount, ref string remsg, int storeId = 0)
        {
            TransactionModel tranModel = new TransactionModel();
            Agentinfo parentAgentinfo = null;
            int beforedeposit = agentInfo.deposit;
            int pbeforedeposit = 0;

            //扣费
            agentInfo.deposit = agentInfo.deposit - cost;
            tranModel.Add($"UPDATE Agentinfo set deposit={agentInfo.deposit} ,updateitme='{DateTime.Now}' where id={agentInfo.id}");
            //分销商扣费
            if (agentInfo.userLevel == 1)
            {
                Distribution distribution = DistributionBLL.SingleModel.GetModel($"agentId={agentInfo.id}");
                parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                pbeforedeposit = parentAgentinfo.deposit;
                parentAgentinfo.deposit = parentAgentinfo.deposit - parentCost;
                tranModel.Add($"UPDATE Agentinfo set deposit={parentAgentinfo.deposit} ,updateitme='{DateTime.Now}' where id={parentAgentinfo.id}");
            }

            //如果开通分店则把体验版去除
            if (xcx.IsExperience)
            {
                tranModel.Add($"UPDATE XcxAppAccountRelation set IsExperience=0 where id={xcx.Id}");
            }

            if (storeId == 0)
            {
                switch (xcxList.Type)
                {
                    case (int)TmpType.小程序多门店模板:
                        if (storeId == 0)
                        {
                            //添加分店记录
                            var ministore = FootBathBLL.SingleModel.GetModel($"appId={xcx.Id} and HomeId=0 and TemplateType={(int)TmpType.小程序多门店模板}");
                            if (ministore == null)
                            {
                                remsg = "没有找到总店";
                                return false;
                            }

                            //判断模板过期时间是否比门店过期时间还小，如果小则开通的门店过期时间与模板过期时间一致
                            var overtime = DateTime.Now.AddYears(1);
                            if (xcx.outtime < overtime)
                            {
                                overtime = xcx.outtime;
                            }

                            tranModel.Add(FootBathBLL.SingleModel.GetAddFootbathSQL(xcx.AccountId, xcxList.storecount, xcx.Id, overtime, ministore.Id).ToArray());
                        }
                        else
                        {
                            tranModel.Add($"UPDATE footbath set OverTime='{xcx.outtime.ToString("yyyy-MM-dd HH:mm:ss")}' ,UpdateDate='{DateTime.Now}' where id={storeId}");
                        }
                        break;
                    case (int)TmpType.小程序餐饮多门店模板:
                        if (storeId == 0)
                        {
                            //添加分店记录
                            Food store_Food = FoodBLL.SingleModel.GetModel($" appId={xcx.Id} and masterStoreId=0 ");
                            if (store_Food == null)
                            {
                                remsg = "没有找到总店";
                                return false;
                            }

                            //判断模板过期时间是否比门店过期时间还小，如果小则开通的门店过期时间与模板过期时间一致
                            var overtime = DateTime.Now.AddYears(1);
                            if (xcx.outtime < overtime)
                            {
                                overtime = xcx.outtime;
                            }

                            tranModel.Add(FoodBLL.SingleModel.GetAddFoodSQL(xcx.AccountId, xcxList.storecount, xcx.Id, overtime, store_Food.Id).ToArray());
                        }
                        else
                        {
                            tranModel.Add($"UPDATE Food set overTime='{xcx.outtime.ToString("yyyy-MM-dd HH:mm:ss")}' ,UpdateDate='{DateTime.Now}' where id={storeId}");
                        }
                        break;
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
                remsg = ex.Message;
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
                throw new Exception(ex.Message);
            }

            if (success)
            {
                //清除缓存
                XcxAppAccountRelationBLL.SingleModel.RemoveRedis(xcx.Id);
                //记录流水
                AgentdepositLogBLL.SingleModel.AddagentinfoLog(agentInfo, new List<XcxTemplate>() { xcxList }, beforedeposit, pbeforedeposit, userName, userAccount.Id.ToString(), storeId > 0 ? 7 : 6, xcx.Id);
            }
            else
            {
                remsg = "系统繁忙";
            }

            return success;
        }

        public bool AddStoreV2(string userName, XcxAppAccountRelation xcx, Agentinfo agentInfo,Agentinfo pagentInfo,Distribution distribution, XcxTemplate xcxList, XcxTemplate pxcxList, int cost, int parentCost, Account userAccount, ref string remsg, int storeId = 0)
        {
            TransactionModel tranModel = new TransactionModel();
            int beforedeposit = agentInfo.deposit;

            //扣费
            tranModel.Add($"UPDATE Agentinfo set deposit={agentInfo.deposit - cost} ,updateitme='{DateTime.Now}' where id={agentInfo.id}");
            //记录流水
            if(!AgentdepositLogBLL.SingleModel.AddagentinfoLogV2(agentInfo, distribution, new List<XcxTemplate>() { xcxList }, userName, userAccount.Id.ToString(),ref tranModel, storeId > 0 ? 7 : (int)AgentDepositLogType.开通新门店, xcx.Id))
            {
                remsg = "记录流水失败";
                return false;
            }

            //分销商扣费
            if (agentInfo.userLevel == 1)
            {
                tranModel.Add($"UPDATE Agentinfo set deposit={pagentInfo.deposit - parentCost} ,updateitme='{DateTime.Now}' where id={pagentInfo.id}");

                if (!AgentdepositLogBLL.SingleModel.AddagentinfoLogV2(pagentInfo, distribution, new List<XcxTemplate>() { pxcxList }, userName, userAccount.Id.ToString(), ref tranModel, storeId > 0 ? 7 : (int)AgentDepositLogType.开通新门店, xcx.Id,1))
                {
                    remsg = "记录代理消费流水失败";
                    return false;
                }
            }

            //如果开通分店则把体验版去除
            if (xcx.IsExperience)
            {
                tranModel.Add($"UPDATE XcxAppAccountRelation set IsExperience=0 where id={xcx.Id}");
            }

            if (storeId == 0)
            {
                switch (xcxList.Type)
                {
                    case (int)TmpType.小程序多门店模板:
                        if (storeId == 0)
                        {
                            //添加分店记录
                            var ministore = FootBathBLL.SingleModel.GetModel($"appId={xcx.Id} and HomeId=0 and TemplateType={(int)TmpType.小程序多门店模板}");
                            if (ministore == null)
                            {
                                remsg = "没有找到总店";
                                return false;
                            }

                            //判断模板过期时间是否比门店过期时间还小，如果小则开通的门店过期时间与模板过期时间一致
                            var overtime = DateTime.Now.AddYears(1);
                            if (xcx.outtime < overtime)
                            {
                                overtime = xcx.outtime;
                            }

                            tranModel.Add(FootBathBLL.SingleModel.GetAddFootbathSQL(xcx.AccountId, xcxList.storecount, xcx.Id, overtime, ministore.Id).ToArray());
                        }
                        else
                        {
                            tranModel.Add($"UPDATE footbath set OverTime='{xcx.outtime.ToString("yyyy-MM-dd HH:mm:ss")}' ,UpdateDate='{DateTime.Now}' where id={storeId}");
                        }
                        break;
                }
            }

            //执行事务
            bool success = false;
            try
            {
                //清除缓存
                XcxAppAccountRelationBLL.SingleModel.RemoveRedis(xcx.Id);
                success = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception ex)
            {
                remsg = ex.Message;
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
                throw new Exception(ex.Message);
            }
            
            return success;
        }
        #endregion

        public List<CustomerModel> GetCustomerData(int agentId, string loginId)
        {
            List<CustomerModel> list = new List<CustomerModel>();
            List<AgentCustomerRelation> customerRelation = GetListByAgentId(agentId);
            if (customerRelation == null || customerRelation.Count <= 0)
                return list;
            string userId = "'" + string.Join("','", customerRelation.Select(s => s.useraccountid).Distinct()) + "'";

            List<Account> accountList = AccountBLL.SingleModel.GetListByAccountids(userId, loginId);
            if (accountList == null)
                return list;

            foreach (Account item in accountList)
            {
                AgentCustomerRelation relationModel = customerRelation.FirstOrDefault(f => f.useraccountid == item.Id.ToString());
                if (relationModel == null)
                    continue;
                CustomerModel model = new CustomerModel();
                model.LoginId = item.LoginId;
                model.agentName = relationModel.username;
                list.Add(model);
            }

            return list;
        }

        /// <summary>
        /// 智慧餐厅开门店
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="xcx"></param>
        /// <param name="agentInfo"></param>
        /// <param name="xcxList"></param>
        /// <param name="cost"></param>
        /// <param name="parentCost"></param>
        /// <param name="userAccount"></param>
        /// <param name="remsg"></param>
        /// <param name="storeid"></param>
        /// <returns></returns>
        public bool OpenZHStore(string userName, XcxAppAccountRelation xcx, int scount, Agentinfo agentInfo, XcxTemplate xcxList, int cost, int parentCost, Account userAccount, ref string remsg)
        {
            
            TransactionModel tranModel = new TransactionModel();
            Agentinfo parentAgentinfo = null;
            int beforeDeposit = agentInfo.deposit;
            int pbeforeDeposit = 0;

            //扣费
            agentInfo.deposit = agentInfo.deposit - cost;
            tranModel.Add($"UPDATE Agentinfo set deposit={agentInfo.deposit} ,updateitme='{DateTime.Now}' where id={agentInfo.id}");
            //分销商扣费
            if (agentInfo.userLevel == 1)
            {
                Distribution distribution = DistributionBLL.SingleModel.GetModel($"agentId={agentInfo.id}");
                parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                pbeforeDeposit = parentAgentinfo.deposit;
                parentAgentinfo.deposit = parentAgentinfo.deposit - parentCost;
                tranModel.Add($"UPDATE Agentinfo set deposit={parentAgentinfo.deposit} ,updateitme='{DateTime.Now}' where id={parentAgentinfo.id}");
            }

            tranModel.Add($"UPDATE XcxAppAccountRelation set IsExperience=0,SCount={scount} where id={xcx.Id}");

            //执行事务
            bool _sec = false;
            try
            {
                _sec = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception ex)
            {
                remsg = ex.Message;
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
                throw new Exception(ex.Message);
            }

            if (_sec)
            {
                int openType = (int)AgentDepositLogType.变更智慧餐厅门店;
                if(xcxList.Type==(int)TmpType.企业智推版)
                {
                    openType = (int)AgentDepositLogType.变更企业智推版员工;
                }
                //记录流水
                AgentdepositLogBLL.SingleModel.AddagentinfoLog(agentInfo, new List<XcxTemplate>() { xcxList }, beforeDeposit, pbeforeDeposit, userName, userAccount.Id.ToString(), openType, xcx.Id);
                //清除缓存
                XcxAppAccountRelationBLL.SingleModel.RemoveRedis(xcx.Id);
            }
            else
            {
                remsg = "系统繁忙";
            }

            return _sec;
        }

        /// <summary>
        /// 企业版升级为企业智推版
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="aid"></param>
        /// <param name="userName"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool UpQieVersion(int agentId,int aid,string userName,ref string msg)
        {
            Agentinfo agentModel = AgentinfoBLL.SingleModel.GetModel(agentId);
            if (agentModel == null)
            {
                msg = "无效代理";
                return false;
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                msg = "无效模板";
                return false;
            }
            if (xcxrelation.agentId != agentModel.id)
            {
                msg= "权限不够";
                return false;
            }

            List<XcxTemplate> xcxtemplateList = XcxTemplateBLL.SingleModel.GetListByTypes($"{(int)TmpType.小程序企业模板},{(int)TmpType.企业智推版}");
            XcxTemplate officialTemplate = xcxtemplateList?.FirstOrDefault(f => f.Type == (int)TmpType.小程序企业模板);
            XcxTemplate qiyeTemplate = xcxtemplateList?.FirstOrDefault(f => f.Type == (int)TmpType.企业智推版);
            if (qiyeTemplate == null || qiyeTemplate == null)
            {
                msg = "暂不支持升级";
                return false;
            }
            qiyeTemplate.year = 1;
            qiyeTemplate.buycount = 1;
            List<XcxTemplate> xcxtemplates = new List<XcxTemplate>() { qiyeTemplate };

            if (xcxrelation.TId != officialTemplate.Id || xcxrelation.TId == qiyeTemplate.Id)
            {
                msg = "该模板已不是企业版";
                return false;
            }
            
            string errorMsg = "";
            int sum = 0;
            //检验代理预存
            List<XcxTemplate> xcxList = AgentinfoBLL.SingleModel.CheckParentAgentDeposit(agentId, 0, "", xcxtemplates, ref sum, ref errorMsg);
            if (errorMsg.Length > 0)
            {
                msg = errorMsg;
                return false;
            }
            
            //升级模板，更新模板信息
            xcxrelation.AddTime = DateTime.Now;
            xcxrelation.Url = xcxList[0].Link;
            xcxrelation.State = 1;
            xcxrelation.TId = xcxList[0].Id;
            xcxrelation.price = xcxList[0].Price;
            xcxrelation.SCount = xcxList[0].SCount;
            xcxrelation.outtime = DateTime.Now.AddYears(1);

            TransactionModel tran = new TransactionModel();

            //添加流水
            int logType = agentModel.userLevel==0?(int)AgentDepositLogType.升级企业版: (int)AgentDepositLogType.分销商升级企业版;
            string dlog = $"客户:{userName} 企业版升级为企业智推版";
            AgentdepositLog agentLogmodel = new AgentdepositLog(xcxrelation.TId, sum, agentModel, dlog, xcxrelation.AccountId.ToString(), logType);
            tran.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLogmodel));

            //扣费
            agentModel.deposit = agentModel.deposit - sum;
            tran.Add($"UPDATE Agentinfo set deposit={agentModel.deposit} ,updateitme='{DateTime.Now}' where id={agentModel.id}");

            //判断是否为分销商
            int parent_sum = 0;
            if (agentModel.userLevel == 1)
            {
                Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentModel.id);
                if (distribution == null)
                {
                    msg = "分销商代理关系错误";
                    return false;
                }
                //检验上级代理预存
                AgentinfoBLL.SingleModel.CheckParentAgentDeposit(distribution.parentAgentId, 0, "", xcxtemplates, ref parent_sum, ref errorMsg);
                if (errorMsg.Length > 0)
                {
                    msg = errorMsg;
                    return false;
                }

                //代理商流水
                Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                dlog = $"客户:{userName} 企业版升级为企业智推版";
                logType = (int)AgentDepositLogType.分销商升级企业版;
                agentLogmodel = new AgentdepositLog(xcxrelation.TId,parent_sum,parentAgentinfo, dlog, xcxrelation.AccountId.ToString(), logType);
                tran.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLogmodel));

                parentAgentinfo.deposit = parentAgentinfo.deposit - parent_sum;
                tran.Add($"UPDATE Agentinfo set deposit={parentAgentinfo.deposit} ,updateitme='{DateTime.Now}' where id={parentAgentinfo.id}");
            }

            //修改用户权限模板记录
            tran.Add($"UPDATE XcxAppAccountRelation set tid={xcxrelation.TId},price={xcxrelation.price},outtime='{xcxrelation.outtime}',AddTime='{xcxrelation.AddTime}',url='{xcxrelation.Url}',scount={xcxrelation.SCount},IsExperience=0 where Id={xcxrelation.Id}");

            //执行事务
            bool success = false;
            try
            {
                success = base.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
                //清除缓存
                XcxAppAccountRelationBLL.SingleModel.RemoveRedis(xcxrelation.Id);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return success;
        }
    }
}
