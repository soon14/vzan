using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.FunctionList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLL.MiniApp.Conf
{
    public class AgentdepositLogBLL : BaseMySql<AgentdepositLog>
    {
        #region 单例模式
        private static AgentdepositLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentdepositLogBLL()
        {

        }

        public static AgentdepositLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentdepositLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public bool ExitLogByacid(int acid)
        {
            string sqlwhere = $"acid={acid}";
            return base.Exists(sqlwhere);
        }
        /// <summary>
        /// 是否是第一次开通
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="type"></param>
        /// <param name="acid"></param>
        /// <returns></returns>
        public bool IsFirstOpen(int agentId,int type,int acid)
        {
            string sqlwhere = $"agentid={agentId} and type={type}";
            if(type==13)
            {
                sqlwhere = $"acid={acid} and type={type}";
            }
            return !base.Exists(sqlwhere);
        }

        public List<AgentdepositLog> GetList(int agentId, string reason, int type, string startTime, string endTime, int pageSize, int pageIndex, out int count,int acid=0)
        {
            string sqlwhere = $"agentid={agentId} and type not in (13,14,15)";
            if(acid>0)
            {
                sqlwhere = $"acid={acid} and type in (13,14,15)";
            }
            if (!string.IsNullOrEmpty(reason))
            {
                sqlwhere += $" and costdetail like '%{reason}%'";
            }
            if (type > 0)
            {
                sqlwhere += $" and type={type}";
            }
            if (!string.IsNullOrEmpty(startTime))
            {
                sqlwhere += $" and addtime>='{startTime} 00:00:00'";
            }
            if (!string.IsNullOrEmpty(endTime))
            {
                sqlwhere += $" and addtime<='{endTime} 23:59:59'";
            }
            List<AgentdepositLog> list = base.GetList(sqlwhere, pageSize, pageIndex, "*", "addtime desc, id desc");
            //if (list != null && list.Count > 0)
            //{
            //    list.ForEach(l => l.showbeforeDeposit = (l.beforeDeposit * 0.01).ToString());
            //    list.ForEach(l => l.showafterDeposit = (l.afterDeposit * 0.01).ToString());
            //    list.ForEach(l => l.showcost = (l.cost * 0.01).ToString());
            //    list.ForEach(l => l.showaddtime = l.addtime.ToString("yyyy-MM-dd HH:mm:ss"));
            //}
            count = base.GetCount(sqlwhere);
            return list;
        }

        public int GetOpenOrderCount(string customerId,int agentId=-1,int type=-1)
        {
            string sqlwhere = "1=1";
            if(!string.IsNullOrEmpty(customerId))
            {
                sqlwhere += $" and customerid='{customerId}' ";
            }
            if(agentId>0)
            {
                sqlwhere += $" and agentid={agentId} ";
            }
            if(type>0)
            {
                sqlwhere += $" and type={type} ";
            }
            return base.GetCount(sqlwhere);
        }

        public void AddagentinfoLog(Agentinfo agentinfo, List<XcxTemplate> xcxList, int deposit, int parentDeposit, string userName, string userId, int openType = 0, int aid = 0)
        {
            if (xcxList == null || xcxList.Count <= 0) return;
            StringBuilder sb = new StringBuilder();
            AgentdepositLog agentLog = new AgentdepositLog();
            DateTime date = DateTime.Now;
            agentLog.agentid = agentinfo.id;
            agentLog.addtime = date;
            agentLog.templateCount = 1;
            agentLog.customerid = userId;
            agentLog.Rid = aid;
            foreach (XcxTemplate xcx in xcxList)
            {
                int cost = xcx.Price * xcx.year * xcx.buycount;
                if(xcx.Type == (int)TmpType.小程序专业模板)
                {
                    xcx.TName += GetVerName(xcx.VersionId);
                }
                
                string desc = $"客户:{userName}  开通模板:{xcx.TName} 开通数量：" + xcx.buycount;
                //整理消费日志
                GetCommondLog(ref openType, ref cost, ref desc, ref agentLog, xcx, userName, "", 0);

                agentLog.tid = xcx.Id;
                agentLog.type = openType > 0 ? openType : 2;
                agentLog.templateCount = openType > 0 ? xcx.storecount : xcx.buycount;
                agentLog.beforeDeposit = deposit;
                agentLog.cost = cost;
                agentLog.afterDeposit = deposit - cost;
                deposit = deposit - cost;
                agentLog.costdetail = desc;
                sb.Append(base.BuildAddSql(agentLog));
            }
            if (agentinfo.userLevel == 1)
            {
                try
                {
                    Distribution distribution = DistributionBLL.SingleModel.GetModel($"agentid={agentinfo.id}");
                    string tids = string.Join(",", xcxList.Select(x => x.Id).ToArray());
                    List<XcxTemplate> parent_xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({tids})", distribution.parentAgentId);
                    AgentdepositLog parent_agentLog = new AgentdepositLog();
                    parent_agentLog.agentid = distribution.parentAgentId;
                    parent_agentLog.addtime = date;
                    parent_agentLog.templateCount = 1;
                    parent_agentLog.customerid = agentinfo.id.ToString();
                    foreach (XcxTemplate xcx in parent_xcxlist)
                    {
                        XcxTemplate model = xcxList.Where(w => w.Id == xcx.Id).FirstOrDefault();
                        xcx.year = model != null ? model.year : 1;
                        xcx.storecount = model.storecount;
                        xcx.buycount = model.buycount;
                        xcx.VersionId = model.VersionId;
                        xcx.SCount = model.SCount;
                        if (xcx.Type == (int)TmpType.小程序专业模板)
                        {
                            xcx.TName += GetVerName(xcx.VersionId);
                        }

                        int cost = xcx.Price * xcx.year * xcx.buycount;
                        if (xcx.Type == (int)TmpType.小程序专业模板)
                        {
                            //重新计算价格专业版版本级别

                            VersionType m = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(xcx.Type, distribution.parentAgentId).FirstOrDefault(x => x.VersionId == xcx.VersionId);
                            cost = Convert.ToInt32(m.VersionPrice) * xcx.year * xcx.buycount;
                        }

                        string desc = $"分销商:{distribution.name}  开通模板:{xcx.TName} 开通数量：" + xcx.buycount;
                        //整理消费日志
                        GetCommondLog(ref openType, ref cost, ref desc, ref agentLog, xcx, userName, distribution.name, 1);

                        parent_agentLog.tid = xcx.Id;
                        parent_agentLog.type = openType > 0 ? openType : 3;
                        parent_agentLog.templateCount = xcx.buycount;
                        parent_agentLog.beforeDeposit = parentDeposit;
                        parent_agentLog.cost = cost;
                        parent_agentLog.afterDeposit = parentDeposit - cost;
                        parentDeposit = parentDeposit - cost;
                        parent_agentLog.costdetail = desc;
                        sb.Append(base.BuildAddSql(parent_agentLog));
                    }
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), ex.Message);
                }

            }
            base.ExecuteNonQuery(sb.ToString());
        }

        public bool AddagentinfoLogV2(Agentinfo agentinfo, Distribution distribution, List<XcxTemplate> xcxList, string userName, string userId,ref TransactionModel trans, int openType = 0, int aid = 0,int isDistribut=0)
        {
            if (xcxList == null || xcxList.Count <= 0)
                return false;

            AgentdepositLog agentLog = new AgentdepositLog();
            DateTime date = DateTime.Now;
            agentLog.agentid = agentinfo.id;
            agentLog.addtime = date;
            agentLog.templateCount = 1;
            agentLog.customerid = userId;
            agentLog.Rid = aid;
            string desc = "";
            int cost = 0;
            int tempOpenType = openType;
            foreach (XcxTemplate xcx in xcxList)
            {
                openType = tempOpenType;
                cost = 0;
                desc = $"客户:{userName}  开通模板:{xcx.TName} 开通数量：" + xcx.buycount;
                if(isDistribut ==1)
                {
                    desc = $"分销商:{distribution.name}  开通模板:{xcx.TName} 开通数量：" + xcx.buycount;
                }
                //整理消费日志
                GetCommondLog(ref openType, ref cost, ref desc, ref agentLog, xcx, userName, "", isDistribut);
                
                agentLog.tid = xcx.Id;
                agentLog.type = openType > 0 ? openType : 2;
                agentLog.templateCount = openType > 0 ? xcx.storecount : xcx.buycount;
                agentLog.beforeDeposit = agentinfo.deposit;
                agentLog.cost = xcx.sumprice;
                agentLog.afterDeposit = agentinfo.deposit - xcx.sumprice;
                agentLog.costdetail = desc;
                trans.Add(base.BuildAddSql(agentLog));

                agentinfo.deposit -= xcx.sumprice;
            }

            return true;
        }

        public bool OpenCustomBotton(Agentinfo agentInfo,Agentinfo parentInfo, string userName,XcxAppAccountRelation xcxrelation, int cost,string parm)
        {
            bool success = false;
            if (agentInfo == null)
                return success;
            TransactionModel tran = new TransactionModel();
            DateTime date = DateTime.Now;
            //代理商扣费
            tran.Add($"UPDATE Agentinfo set deposit={agentInfo.deposit-cost} ,updateitme='{date}' where id={agentInfo.id}");
            //添加自定义水印数据
            ConfParam model = new ConfParam();
            model.AppId = xcxrelation.AppId;
            model.Param = parm;
            model.Value = "";
            model.State = 0;
            model.UpdateTime = date;
            model.AddTime = date;
            model.RId = xcxrelation.Id;
            tran.Add(ConfParamBLL.SingleModel.BuildAddSql(model));
            //扣费记录
            AgentdepositLog agentLog = new AgentdepositLog();
            agentLog.agentid = agentInfo.id;
            agentLog.addtime = date;
            agentLog.templateCount = 1;
            agentLog.customerid = xcxrelation.AccountId.ToString();
            agentLog.Rid = xcxrelation.Id;
            agentLog.OutTime = date;
            agentLog.tid = 0;
            agentLog.type = (int)AgentDepositLogType.开启水印;
            agentLog.templateCount = 0;
            agentLog.beforeDeposit = agentInfo.deposit;
            agentLog.cost = cost;
            agentLog.afterDeposit = agentInfo.deposit - cost;
            agentLog.costdetail = $"客户:{userName}开启自定义小程序水印";
            tran.Add(base.BuildAddSql(agentLog));

            //判断是否是二级分销商
            if (agentInfo.userLevel > 0)
            {
                if (parentInfo == null)
                    return success;
                //上级代理商扣费
                tran.Add($"UPDATE Agentinfo set deposit={parentInfo.deposit - cost} ,updateitme='{date}' where id={parentInfo.id}");

                agentLog = new AgentdepositLog();
                agentLog.agentid = parentInfo.id;
                agentLog.addtime = date;
                agentLog.templateCount = 1;
                agentLog.customerid = xcxrelation.AccountId.ToString();
                agentLog.Rid = xcxrelation.Id;
                agentLog.OutTime = date;
                agentLog.tid = 0;
                agentLog.type = (int)AgentDepositLogType.开启水印;
                agentLog.templateCount = 0;
                agentLog.beforeDeposit = parentInfo.deposit;
                agentLog.cost = cost;
                agentLog.afterDeposit = parentInfo.deposit - cost;
                agentLog.costdetail = $"分销商{parentInfo.name}为客户{userName}开启自定义小程序水印";
                tran.Add(base.BuildAddSql(agentLog));
            }

            success = base.ExecuteTransactionDataCorect(tran.sqlArray);
            return success;
        }

        /// <summary>
        ////整理消费日志
        /// </summary>
        /// <param name="openType"></param>
        /// <param name="cost"></param>
        /// <param name="desc"></param>
        /// <param name="agentLog"></param>
        /// <param name="xcx"></param>
        /// <param name="userName"></param>
        /// <param name="dName"></param>
        /// <param name="isDistribution"></param>
        public void GetCommondLog(ref int openType, ref int cost, ref string desc, ref AgentdepositLog agentLog, XcxTemplate xcx, string userName, string dName, int isDistribution)
        {
            switch(xcx.Type)
            {
                case (int)TmpType.小程序餐饮多门店模板:
                case (int)TmpType.小程序多门店模板:
                    GetOpenMulStoreInfo(openType, ref cost, ref desc, ref agentLog, xcx, userName, dName, isDistribution);
                    break;
                case (int)TmpType.智慧餐厅:
                    GetOpenZHStoreInfo(openType, ref cost, ref desc, xcx, userName, dName, isDistribution);
                    if(isDistribution==1 && openType!=(int)AgentDepositLogType.分销商开通客户模板 && openType != (int)AgentDepositLogType.开通客户模板)
                    {
                        openType = (int)AgentDepositLogType.分销商变更智慧餐厅门店;
                    }
                    break;
                case (int)TmpType.企业智推版:
                    GetOpenQYStoreInfo(openType, ref cost, ref desc, xcx, userName, dName, isDistribution);
                    if (isDistribution == 1 && openType != (int)AgentDepositLogType.分销商开通客户模板 && openType != (int)AgentDepositLogType.开通客户模板)
                    {
                        openType = (int)AgentDepositLogType.分销商变更企业智推版员工;
                    }
                    break;
            }
        }

        /// <summary>
        /// 多门店版开通日志整理
        /// </summary>
        /// <param name="openType"></param>
        /// <param name="cost"></param>
        /// <param name="desc"></param>
        /// <param name="agentLog"></param>
        /// <param name="xcx"></param>
        /// <param name="userName"></param>
        /// <param name="dName"></param>
        /// <param name="isDistribution"></param>
        public void GetOpenMulStoreInfo(int openType,ref int cost,ref string desc,ref AgentdepositLog agentLog,XcxTemplate xcx,string userName,string dName,int isDistribution)
        {
            switch (openType)
            {
                case (int)AgentDepositLogType.开通新门店:
                    //帮客户开通分店
                    cost = xcx.sumprice;
                    agentLog.OutTime = DateTime.Now.AddYears(1);
                    desc = isDistribution==0?($"客户:{userName}  新开通门店数量：{xcx.storecount}"):($"分销商:{dName} 为客户{userName}新开通门店数量：{xcx.storecount}");
                    break;
                case 7:
                    //帮客户续费门店
                    cost = xcx.sumprice;
                    agentLog.OutTime = DateTime.Parse(xcx.outtime);
                    desc = $"客户:{userName}  {xcx.TName}门店续期至{xcx.outtime}";
                    break;
                default:
                    //开通多门店模板
                    cost += (xcx.storecount - xcx.SCount) * xcx.SPrice;
                    desc = isDistribution == 0 ? ($"客户:{userName}  开通模板:{xcx.TName} 开通数量：{xcx.buycount} 门店数量：{xcx.storecount}"):($"分销商:{dName}  开通模板:{xcx.TName} 开通数量：{xcx.buycount} 门店数量：{xcx.storecount}");
                    break;
            }
        }
        
        /// <summary>
        /// 智慧餐厅开通日志整理
        /// </summary>
        /// <param name="openType"></param>
        /// <param name="cost"></param>
        /// <param name="desc"></param>
        /// <param name="xcx"></param>
        /// <param name="userName"></param>
        /// <param name="dname"></param>
        /// <param name="isDistribution"></param>
        public void GetOpenZHStoreInfo(int openType, ref int cost, ref string desc, XcxTemplate xcx, string userName, string dname, int isDistribution)
        {
            switch (openType)
            {
                case (int)AgentDepositLogType.变更智慧餐厅门店:
                    //开通智慧餐厅模板
                    cost = 0;
                    if (xcx.buycount > xcx.SCount)
                    {
                        cost += (xcx.buycount - xcx.SCount) * xcx.SPrice;
                        desc = isDistribution == 0 ? ($"客户:{userName}  开通{xcx.TName}门店数量：{xcx.buycount - xcx.SCount}") : ($"分销商:{dname} 为客户{userName}新开通{xcx.TName}门店数量：{xcx.buycount - xcx.SCount}");
                    }
                    else
                    {
                        desc = isDistribution == 0 ? ($"客户:{userName}  减少了{xcx.TName}门店数量：{xcx.SCount - xcx.buycount}") : ($"分销商:{dname} 为客户{userName}减少{xcx.TName}门店数量：{xcx.SCount - xcx.buycount}");
                    }
                    break;
                default:
                    //开通智慧餐厅模板
                    cost += (xcx.storecount - xcx.SCount) * xcx.SPrice * xcx.buycount;
                    desc = isDistribution == 0 ? ($"客户:{userName}  开通模板:{xcx.TName} 开通数量：" + xcx.buycount + " 门店数量：" + xcx.storecount) : (desc = $"分销商:{dname}  开通模板:{xcx.TName} 开通数量：" + xcx.buycount + " 门店数量：" + xcx.storecount);
                    break;
            }
        }

        /// <summary>
        /// 企业智推版开通日志整理
        /// </summary>
        /// <param name="openType"></param>
        /// <param name="cost"></param>
        /// <param name="desc"></param>
        /// <param name="xcx"></param>
        /// <param name="userName"></param>
        /// <param name="dname"></param>
        /// <param name="isDistribution"></param>
        public void GetOpenQYStoreInfo(int openType, ref int cost, ref string desc, XcxTemplate xcx, string userName, string dname, int isDistribution)
        {
            switch(openType)
            {
                case (int)AgentDepositLogType.变更企业智推版员工:
                    //企业智推版员工数量
                    cost = 0;
                    if (xcx.buycount > xcx.SCount)
                    {
                        cost += (xcx.buycount - xcx.SCount) * xcx.SPrice;
                        desc = isDistribution==0?($"客户:{userName}  开通{xcx.TName}员工数量：{xcx.buycount - xcx.SCount}"):($"分销商:{dname} 为客户{userName}新开通{xcx.TName}员工数量：{xcx.buycount - xcx.SCount}");
                    }
                    else
                    {
                        desc = isDistribution == 0 ? ($"客户:{userName}  减少了{xcx.TName}员工数量：{xcx.SCount - xcx.buycount}"):($"分销商:{dname} 为客户{userName}减少{xcx.TName}员工数量：{xcx.SCount - xcx.buycount}");
                    }
                    break;
                default:
                    //开通企业智推版
                    cost += (xcx.storecount - xcx.SCount) * xcx.SPrice * xcx.buycount;
                    desc = isDistribution == 0 ? ($"客户:{userName}  开通模板:{xcx.TName} 开通数量：" + xcx.buycount + " 员工数量：" + xcx.storecount):(desc = $"分销商:{dname}  开通模板:{xcx.TName} 开通数量：" + xcx.buycount + " 员工数量：" + xcx.storecount);
                    break;
            }
        }

        /// <summary>
        /// 获取专业版升级版本名称
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public string GetVerName(int versionId)
        {
            switch (versionId)
            {
                case 3:
                    return "基础版";
                case 2:
                    return "高级版";
                case 1:
                    return "尊享版";
                default:
                    return "旗舰版";
            }
        }
    }
}
