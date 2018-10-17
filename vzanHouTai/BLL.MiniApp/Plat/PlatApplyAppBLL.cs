using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
using Entity.MiniApp.User;
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
    public class PlatApplyAppBLL : BaseMySql<PlatApplyApp>
    {
        #region 单例模式
        private static PlatApplyAppBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatApplyAppBLL()
        {

        }

        public static PlatApplyAppBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatApplyAppBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 根据店铺Id获取申请开通的小程序记录
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public PlatApplyApp GetPlatApplyAppByStoreId(int storeId)
        {
            return base.GetModel($"StoreId={storeId}");
        }

        public List<PlatApplyApp> GetListByAid(int aid)
        {
            string sql = $"bindaid={aid}";
            return base.GetList(sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customername">客户名称</param>
        /// <param name="loginid">会员账号</param>
        /// <param name="storename">店铺名称</param>
        /// <param name="openstate">是否开通，0：未开通，1：开通</param>
        /// <param name="xcxappstate">状态</param>
        /// <param name="bindaid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<PlatApplyApp> GetDataList(string appid, int daylength, string customername, string loginid, string storename, int openstate, int xcxappstate, int bindaid, int pageSize, int pageIndex, ref int count)
        {
            List<PlatApplyApp> list = new List<PlatApplyApp>();
            List<MySqlParameter> parms = new List<MySqlParameter>();

            string sql = $@"select {"{0}"}  from platapplyapp app left join 
                            platstore s on app.storeid = s.id
                            left join xcxappaccountrelation x on s.aid = x.id
                            left join platmycard c on app.mycardid = c.id";
            string sqllilst = string.Format(sql, "app.*,s.name storename,c.name customername,c.loginid,x.outtime,x.State xcxappstate");
            string sqlcount = string.Format(sql, "count(*)");
            string sqlwhere = $" where bindaid = {bindaid} and c.appid='{appid}' ";
            string sqllimit = $" LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

            if (!string.IsNullOrEmpty(customername))
            {
                parms.Add(new MySqlParameter("@customername", $"%{customername}%"));
                sqlwhere += $" and c.name like @customername";
            }
            if (!string.IsNullOrEmpty(loginid))
            {
                parms.Add(new MySqlParameter("@loginid", $"%{loginid}%"));
                sqlwhere += $" and c.loginid like @loginid";
            }
            if (!string.IsNullOrEmpty(storename))
            {
                parms.Add(new MySqlParameter("@storename", $"%{storename}%"));
                sqlwhere += $" and s.name like @storename";
            }
            if (openstate > -1)
            {
                sqlwhere += $" and app.openstate ={openstate}";
            }
            if (xcxappstate > -2)
            {
                sqlwhere += $" and x.state ={xcxappstate}";
            }
            if (daylength > 0)
            {
                sqlwhere += $" and app.openstate =1 and x.outtime <= (NOW()+INTERVAL {daylength} DAY)";
            }

            count = base.GetCountBySql(sqlcount + sqlwhere, parms.ToArray());
            if (count <= 0)
                return list;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqllilst + sqlwhere + sqllimit, parms.ToArray()))
            {
                while (dr.Read())
                {
                    PlatApplyApp amodel = base.GetModel(dr);
                    amodel.CustomerName = dr["customername"].ToString();
                    amodel.StoreName = dr["storename"].ToString();
                    amodel.LoginId = dr["loginid"].ToString();
                    if (dr["outtime"] != DBNull.Value)
                    {
                        amodel.OutTime = DateTime.Parse(dr["outtime"].ToString());

                        if (amodel.OutTime <= DateTime.Now)
                        {
                            amodel.ValDayLength = 0;
                        }
                        else
                        {
                            TimeSpan sp = amodel.OutTime.Subtract(DateTime.Now);
                            amodel.ValDayLength = sp.Days <= 0 ? 1 : sp.Days;
                        }
                    }
                    if (dr["xcxappstate"] != DBNull.Value)
                    {
                        amodel.XcxAppState = Convert.ToInt32(dr["xcxappstate"]);
                    }
                    list.Add(amodel);
                }
            }

            return list;
        }

        /// <summary>
        /// 开通独立小程序
        /// </summary>
        /// <param name="platApplyAppModel"></param>
        /// <param name="accountid"></param>
        /// <param name="uselength"></param>
        /// <param name="tid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool OpenStore(PlatApplyApp platApplyAppModel, string accountid, int uselength, int tid, ref string msg)
        {
            DateTime nowtime = DateTime.Now;
            TransactionModel tran = new TransactionModel();
            

            bool success = false;
            if (platApplyAppModel == null)
            {
                msg = "申请记录为空";
                return false;
            }

            //用户基础数据，获取普通预存款
            
            AccountRelation accountrelation = AccountRelationBLL.SingleModel.GetModelByAccountId(accountid);
            
            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(accountid);

            #region 模板跟模板价格
            XcxTemplate tempinfo = XcxTemplateBLL.SingleModel.GetModelByType((int)TmpType.小未平台子模版);
            if (tempinfo != null && agentinfo != null)
            {
                List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({tempinfo.Id})", agentinfo.id);
                if (xcxlist != null && xcxlist.Count > 0)
                {
                    //代理过期检验
                    AgentinfoBLL.SingleModel.CheckOutTime(ref xcxlist,agentinfo,0,ref msg);
                    tempinfo = xcxlist[0];
                }
            }
            if (tempinfo == null)
            {
                msg = "模板数据为空";
                return false;
            }
            #endregion

            #region 扣费
            //扣除总费用
            int sum = tempinfo.Price * uselength;
            //变更前金额
            int deposit = 0;
            //变更后的金额
            int afterdeposit = 0;
            //扣除代理费用
            if (agentinfo != null)
            {
                //首次开通免费
                if (AgentdepositLogBLL.SingleModel.IsFirstOpen(agentinfo.id, 2, 0))
                {
                    sum = 0;
                    uselength = 1;
                }
                //变更前金额
                deposit = agentinfo.deposit;
                //判断余额是否满足扣费
                if (deposit + accountrelation.Deposit < sum)
                {
                    msg = "余额不足";
                    return false;
                }

                //变更后金额，先扣除普通用户账号预存款
                if (accountrelation.Deposit>0)
                {
                    if(accountrelation.Deposit>=sum)
                    {
                        afterdeposit = accountrelation.Deposit - sum;
                        tran.Add($"UPDATE AccountRelation set deposit={afterdeposit} ,updatetime='{nowtime}' where id={accountrelation.Id}");
                    }
                    else
                    {
                        //先扣除普通用户预存，再扣除代理商预存
                        afterdeposit = agentinfo.deposit - (sum - accountrelation.Deposit);
                        tran.Add($"UPDATE Agentinfo set deposit={afterdeposit} ,updateitme='{nowtime}' where id={agentinfo.id}");
                        tran.Add($"UPDATE AccountRelation set deposit=0 ,updatetime='{nowtime}' where id={accountrelation.Id}");
                    }
                }
                else
                {
                    afterdeposit = deposit - sum;
                    tran.Add($"UPDATE Agentinfo set deposit={afterdeposit} ,updateitme='{nowtime}' where id={agentinfo.id}");
                }
            }
            //扣除普通账号预存款
            else
            {
                //首次开通免费
                if (AgentdepositLogBLL.SingleModel.IsFirstOpen(0, 13, accountrelation.Id))
                {
                    sum = 0;
                }
                else
                {
                    sum = 100* uselength;
                }
                //变更前金额
                deposit = accountrelation.Deposit;
                //判断余额是否满足扣费
                if (deposit < sum)
                {
                    msg = "余额不足";
                    return false;
                }
                //变更后金额
                afterdeposit = deposit - sum;
                tran.Add($"UPDATE AccountRelation set deposit={afterdeposit} ,updatetime='{nowtime}' where id={accountrelation.Id}");
            }
            #endregion

            //名片管理登陆账号
            
            PlatMyCard mycardmodel = PlatMyCardBLL.SingleModel.GetModel(platApplyAppModel.MycardId);
            if (mycardmodel == null)
            {
                msg = "名片过期";
                return false;
            }
            //判断是否已开通
            if (!string.IsNullOrEmpty(mycardmodel.LoginId))
            {
                msg = "该用户已开通过，请刷新看看";
                return false;
            }

            //开通后台账号
            Account account = AccountBLL.SingleModel.WeiXinRegister("", 0, "", true, "", "", "小未平台开通独立小程序");
            if (account == null)
            {
                msg = "注册后台账号失败";
                return false;
            }

            mycardmodel.LoginId = account.LoginId;
            mycardmodel.UpdateTime = nowtime;
            tran.Add(PlatMyCardBLL.SingleModel.BuildUpdateSql(mycardmodel, "LoginId,UpdateTime"));

            //申请开通记录
            platApplyAppModel.UserId = mycardmodel.UserId;
            platApplyAppModel.OpenState = 1;
            platApplyAppModel.OpenTime = nowtime;
            platApplyAppModel.UpdateTime = nowtime;
            tran.Add(base.BuildUpdateSql(platApplyAppModel, "OpenState,OpenTime,UpdateTime,UserId"));

            //开通独立小程序模板
            tran.Add($@"insert into XcxAppAccountRelation(TId,AccountId,AddTime,Url,price,outtime,agentid) 
            values({tempinfo.Id}, '{account.Id}', '{nowtime}', '{tempinfo.Link}', {tempinfo.Price}, '{nowtime.AddYears(uselength)}',{(agentinfo != null ? agentinfo.id : 0)})");

            //绑定店铺
            
            PlatStore platstoremodel = PlatStoreBLL.SingleModel.GetModel(platApplyAppModel.StoreId);
            if (platstoremodel == null)
            {
                msg = "店铺已过期，请刷新重试";
                return false;
            }
            tran.Add($"update PlatStore set aid=(select last_insert_id()),UpdateTime='{nowtime}' where id={platstoremodel.Id}");

            #region 开通流水
            AgentdepositLog agentLog = new AgentdepositLog();
            agentLog.agentid = agentinfo != null ? agentinfo.id : 0;
            agentLog.addtime = nowtime;
            agentLog.templateCount = 1;
            agentLog.customerid = account.Id.ToString();
            agentLog.tid = tempinfo.Id;
            agentLog.type = agentinfo != null ? (int)AgentDepositLogType.开通客户模板 : (int)AgentDepositLogType.普通用户开通模板;
            agentLog.templateCount = 1;
            agentLog.beforeDeposit = deposit;
            agentLog.cost = sum;
            agentLog.Rid = platApplyAppModel.BindAId;
            agentLog.acid = agentinfo != null ? 0 : accountrelation.Id;
            agentLog.afterDeposit = afterdeposit;
            string desc = $"客户:{mycardmodel.Name}  开通模板:{tempinfo.TName} 开通数量：1";
            agentLog.costdetail = desc;
            tran.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLog));
            #endregion

            //执行事务
            success = base.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);

            return success;
        }
        /// <summary>
        /// 续期
        /// </summary>
        /// <param name="platApplyAppModel"></param>
        /// <param name="accountid"></param>
        /// <param name="uselength"></param>
        /// <param name="tid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool AddTimeLength(PlatApplyApp platApplyAppModel, string accountid, int uselength, int tid, ref string msg)
        {
            DateTime nowtime = DateTime.Now;
            TransactionModel tran = new TransactionModel();

            #region 基础验证
            bool success = false;
            if (platApplyAppModel == null)
            {
                msg = "申请记录为空";
                return false;
            }
            //名片管理登陆账号
            
            PlatMyCard mycardmodel = PlatMyCardBLL.SingleModel.GetModel(platApplyAppModel.MycardId);
            if (mycardmodel == null)
            {
                msg = "名片过期";
                return false;
            }
            //判断是否已开通
            if (string.IsNullOrEmpty(mycardmodel.LoginId))
            {
                msg = "该用户还没开通小程序";
                return false;
            }

            //店铺数据
            
            PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(platApplyAppModel.StoreId);
            if (platStore == null)
            {
                msg = "店铺过期，请刷新重试";
                return false;
            }
            #endregion

            #region 使用中的模板数据
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(platStore.Aid);
            if (xcxrelation == null)
            {
                msg = "模板数据过期";
                return false;
            }
            if (xcxrelation.outtime < nowtime)
            {
                xcxrelation.outtime = nowtime.AddYears(uselength);
            }
            else
            {
                xcxrelation.outtime = xcxrelation.outtime.AddYears(uselength);
            }
            tran.Add($"update XcxAppAccountRelation set outtime='{xcxrelation.outtime}',state=1 where id={xcxrelation.Id}");
            #endregion

            //用户基础数据，获取普通预存款
            
            AccountRelation accountrelation = AccountRelationBLL.SingleModel.GetModelByAccountId(accountid);

            //代理数据
            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(accountid);

            #region 模板跟模板价格
            XcxTemplate tempinfo = XcxTemplateBLL.SingleModel.GetModelByType((int)TmpType.小未平台子模版);
            if (tempinfo != null && agentinfo != null)
            {
                List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({tempinfo.Id})", agentinfo.id);
                if (xcxlist != null && xcxlist.Count > 0)
                {
                    //代理过期检验
                    AgentinfoBLL.SingleModel.CheckOutTime(ref xcxlist, agentinfo, 0, ref msg);
                    tempinfo = xcxlist[0];
                }
            }
            if (tempinfo == null)
            {
                msg = "模板数据为空";
                return false;
            }
            #endregion

            #region 扣费
            //扣除总费用
            int sum = tempinfo.Price * uselength;
            //变更前金额
            int deposit = 0;
            //变更后的金额
            int afterdeposit = 0;
            //扣除代理费用
            if (agentinfo != null)
            {
                //变更前金额
                deposit = agentinfo.deposit;
                //判断余额是否满足扣费
                if (deposit + accountrelation.Deposit < sum)
                {
                    msg = "余额不足";
                    return false;
                }
                //变更后金额
                if (deposit >= sum)
                {
                    afterdeposit = deposit - sum;
                    tran.Add($"UPDATE Agentinfo set deposit={afterdeposit} ,updateitme='{nowtime}' where id={agentinfo.id}");
                }
                else if (accountrelation.Deposit >= sum)
                {
                    afterdeposit = accountrelation.Deposit - sum;
                    tran.Add($"UPDATE AccountRelation set deposit={afterdeposit} ,updatetime='{nowtime}' where id={accountrelation.Id}");
                }
                else
                {
                    //先扣除普通用户预存，再扣除代理商预存
                    afterdeposit = agentinfo.deposit - (sum - accountrelation.Deposit);
                    tran.Add($"UPDATE Agentinfo set deposit={afterdeposit} ,updateitme='{nowtime}' where id={agentinfo.id}");
                    tran.Add($"UPDATE AccountRelation set deposit=0 ,updatetime='{nowtime}' where id={accountrelation.Id}");
                }
            }
            //扣除普通账号预存款
            else
            {
                sum = 100* uselength;
                //变更前金额
                deposit = accountrelation.Deposit;
                //判断余额是否满足扣费
                if (deposit < sum)
                {
                    msg = "余额不足";
                    return false;
                }
                //变更后金额
                afterdeposit = deposit - sum;
                tran.Add($"UPDATE AccountRelation set deposit={afterdeposit} ,updatetime='{nowtime}' where id={accountrelation.Id}");
            }
            #endregion

            #region 开通流水
            
            AgentdepositLog agentLog = new AgentdepositLog();
            agentLog.agentid = agentinfo != null ? agentinfo.id : 0;
            agentLog.addtime = nowtime;
            agentLog.templateCount = 1;
            agentLog.customerid = xcxrelation.AccountId.ToString();
            agentLog.tid = tempinfo.Id;
            agentLog.type = agentinfo != null ? (int)AgentDepositLogType.代理商续费 : (int)AgentDepositLogType.普通用户续费模板;
            agentLog.templateCount = 1;
            agentLog.beforeDeposit = deposit;
            agentLog.cost = sum;
            agentLog.acid = agentinfo != null ? 0 : accountrelation.Id;
            agentLog.Rid = platApplyAppModel.BindAId;
            agentLog.afterDeposit = afterdeposit;
            string desc = $"客户:{mycardmodel.Name}  续期模板:{tempinfo.TName} 续期年限：{uselength}年";
            agentLog.costdetail = desc;
            tran.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentLog));
            #endregion

            success = base.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);

            //清除缓存
            XcxAppAccountRelationBLL.SingleModel.RemoveRedis(xcxrelation.Id);
            return success;
        }
    }
}
