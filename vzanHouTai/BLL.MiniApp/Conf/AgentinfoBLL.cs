using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.FunctionList;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Core.MiniApp;
using Utility;

namespace BLL.MiniApp.Conf
{
    public class AgentinfoBLL : BaseMySql<Agentinfo>
    {
        private readonly string _redis_agentpasswordkey= "agentpasswordkey_{0}";
        private readonly string _redis_AgentOutTimeKey = "AgentOutTimeKey_{0}";//AgentOutTimeKey_9

        #region 单例模式
        private static AgentinfoBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentinfoBLL()
        {

        }

        public static AgentinfoBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentinfoBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public Agentinfo GetModelByAccoundId(string accountid,int state=0)
        {
            if (string.IsNullOrEmpty(accountid))
            {
                return null;
            }
            MySqlParameter[] paras = { new MySqlParameter("@useraccountid", accountid) };
            string sqlwhere = $"useraccountid=@useraccountid ";
            if (state == 0)
            {
                sqlwhere += $" and state>{state}";
            }
            return base.GetModel(sqlwhere, paras);
        }

        public List<Agentinfo> GetListByAccoundIds(string accountids)
        {
            if (string.IsNullOrEmpty(accountids))
            {
                return new List<Agentinfo>();
            }

            return base.GetList($"useraccountid in ({accountids}) and state>0");
        }

        public List<Agentinfo> GetListByIds(string ids)
        {
            if (ids == null || ids.Length <= 0)
                return new List<Agentinfo>();

            return base.GetList($"id in ({ids})");
        }

        public void SavePassword(int agentid,string password)
        {
            RedisUtil.Set<string>(string.Format(_redis_agentpasswordkey, agentid), password, TimeSpan.FromHours(12));
        }

        public bool IsExitePassword(Agentinfo agentinfo)
        {
            if (agentinfo!=null && !string.IsNullOrEmpty(agentinfo.Password))
            {
                string password = RedisUtil.Get<string>(string.Format(_redis_agentpasswordkey, agentinfo.id));
                return string.IsNullOrEmpty(password);
            }

            return false;
        }

        /// <summary>
        /// 根据小程序名称模糊匹配小程序Rid集合
        /// </summary>
        /// <returns></returns>
        public List<int> GetAgentIdByAgentName(string agentName)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            List<int> listAgentId = new List<int>();
            parameters.Add(new MySqlParameter("@agentName", $"%{agentName}%"));
            string strWhere = " name like @agentName";
            List<Agentinfo> list = base.GetListByParam(strWhere, parameters.ToArray());
            if (list != null && list.Count > 0)
            {
                foreach (Agentinfo item in list)
                {
                    listAgentId.Add(item.id);
                }
            }
            return listAgentId;
        }

        public List<Agentinfo> GetListByName(string name,int agentid)
        {
            if (string.IsNullOrEmpty(name))
                return new List<Agentinfo>();

            string sqlwhere = $@"(id in (select agentid from AgentDistributionRelation where  parentagentid = {agentid}) or id ={agentid}) and name like @name";
            MySqlParameter[] parm = new MySqlParameter[] { new MySqlParameter("@name",$"%{name}%") };

            return base.GetListByParam(sqlwhere,parm);
        }
        
        /// <summary>
        /// 检验上级代理预存
        /// </summary>
        /// <param name="agentId">分销商ID</param>
        /// <param name="tids">模板ID</param>
        /// <param name="industryid"></param>
        /// <param name="industr"></param>
        /// <param name="xcxtemplates"></param>
        /// <param name="parent_sum"></param>
        /// <param name="msg"></param>
        public List<XcxTemplate> CheckParentAgentDeposit(int agentId,int industryid,string industr,List<XcxTemplate> xcxtemplates,ref int parent_sum,ref string msg)
        {
            List<XcxTemplate> xcxList = new List<XcxTemplate>();
            if (xcxtemplates==null || xcxtemplates.Count<=0)
            {
                msg = "请选择开通模板";
                return xcxList;
            }
            string tids = string.Join(",", xcxtemplates.Select(s => s.Id));
            Agentinfo agentinfo = base.GetModel(agentId);
            if(agentinfo == null)
            {
                msg = "无效代理";
                return xcxList;
            }
            
            xcxList = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({tids})", agentId);
            if (xcxList != null && xcxList.Count > 0)
            {
                #region 判断代理商合同是否已过期，已过期不给开免费版
                CheckOutTime(ref xcxList, agentinfo, industryid, ref msg);
                #endregion

                foreach (XcxTemplate xcxmodel in xcxList)
                {
                    XcxTemplate tempmodel = xcxtemplates.Where(w => w.Id == xcxmodel.Id).FirstOrDefault();
                    //判断是否为行业小程序模板
                    xcxmodel.industr = xcxmodel.Type == (int)TmpType.小程序专业模板 ? industr : "";
                    xcxmodel.VersionId = industryid;
                    //判断是否为小程序多店铺模板
                    if (xcxmodel.Type == (int)TmpType.小程序多门店模板 || xcxmodel.Type == (int)TmpType.小程序餐饮多门店模板 || xcxmodel.Type == (int)TmpType.智慧餐厅 || xcxmodel.Type == (int)TmpType.企业智推版)
                    {
                        xcxmodel.storecount = tempmodel.storecount;
                        if (xcxmodel.storecount <= 0)
                        {
                            xcxmodel.storecount = xcxmodel.SCount;
                        }
                        //判断开通的多门店的分店是否有超过预定值
                        xcxmodel.sumprice = (xcxmodel.storecount - xcxmodel.SCount) * xcxmodel.SPrice;
                        parent_sum += xcxmodel.sumprice;
                    }

                    //有效期
                    xcxmodel.year = tempmodel != null ? tempmodel.year : 1;
                    xcxmodel.buycount = tempmodel.buycount;
                    if (xcxmodel.Type == (int)TmpType.小程序专业模板)
                    {
                        //重新计算价格专业版版本级别
                        VersionType model = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(xcxmodel.Type, agentinfo.id).FirstOrDefault(x => x.VersionId == xcxmodel.VersionId);
                        xcxmodel.sumprice = Convert.ToInt32(model.VersionPrice) * xcxmodel.year * tempmodel.buycount;
                        parent_sum += xcxmodel.sumprice;
                        xcxmodel.Price = Convert.ToInt32(model.VersionPrice);
                        xcxmodel.LimitCount = model.LimitCount;
                    }
                    else
                    {
                        xcxmodel.sumprice += xcxmodel.Price * xcxmodel.year * tempmodel.buycount;
                        parent_sum += xcxmodel.Price * xcxmodel.year * tempmodel.buycount;
                    }
                }

                if (parent_sum > agentinfo.deposit)
                {
                    msg = $"创建失败，请联系上级代理";
                }
            }
            else
            {
                msg= "数据错误！";
            }

            return xcxList;
        }

        /// <summary>
        /// 导出exel
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Agentinfo> GetListExcelData(int pageIndex, int pageSize)
        {
            List<Agentinfo> list = new List<Agentinfo>();
            string sql = "select a.*,(select sum(cost) from AgentdepositLog where agentid=a.id and type<>1) as sum,w.webState,w.domainType,w.domian from agentinfo a left join AgentWebSiteInfo w on a.useraccountid=w.useraccountid";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName,CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    Agentinfo amodel = new Agentinfo();
                    amodel.id = Convert.ToInt32(dr["id"]);
                    amodel.useraccountid = dr["useraccountid"].ToString();
                    amodel.userLevel = Convert.ToInt32(dr["userLevel"]);
                    amodel.addtime = DateTime.Parse(dr["addtime"].ToString());
                    amodel.updateitme = DateTime.Parse(dr["updateitme"].ToString());
                    amodel.deposit = Convert.ToInt32(dr["deposit"]);
                    amodel.state = Convert.ToInt32(dr["state"]);


                    if (dr["sum"] != DBNull.Value)
                    {
                        amodel.sumcost = (Convert.ToInt32(dr["sum"]) * 0.01).ToString("0.00");
                    }
                    if (dr["webState"] != DBNull.Value)
                    {
                        amodel.webState = Convert.ToInt32(dr["webState"]);
                    }
                    else
                    {
                        amodel.webState = -2;
                    }
                    if (dr["domainType"] != DBNull.Value)
                    {
                        amodel.domainType = Convert.ToInt32(dr["domainType"]);
                    }
                    else
                    {
                        amodel.domainType = -1;
                    }
                    if (dr["domian"] != DBNull.Value)
                    {
                        amodel.domian = dr["domian"].ToString();
                    }
                    else
                    {
                        amodel.domian = "未进行绑定配置";
                    }
                    if (amodel.domainType == 1)
                    {
                        amodel.domian += WebSiteConfig.DzWebSiteDomainExt;
                    }

                    list.Add(amodel);
                }
            }

            return list;
        }

        /// <summary>
        /// 检测代理商合同是否快过期了
        /// 账户退出后进来重新提示，不退出则一天后重新提示
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public Return_Msg CheckOutTime(int agentId)
        {
            Return_Msg data = new Return_Msg();
            Agentinfo model = base.GetModel(agentId);
            string key = string.Format(_redis_AgentOutTimeKey, agentId);
            string code = RedisUtil.Get<string>(key);
            //判断是否已显示过
            if(code!=null && code == "1")
            {
                data.code = "0";
                data.Msg = "已显示过";
                data.isok = true;
                return data;
            }

            if (model == null)
            {
                data.Msg = "请重新登录";
            }
            else
            {
                if(model.userLevel!=0)
                {
                    data.code = "0";
                    data.Msg = "分销代理商";
                    data.isok = true;
                    return data;
                }

                long secondes = DateHelper.GetDataCount(model.OutTime,DateTime.Now,5);
                long[] dateArray = DateHelper.FormatSeconds(secondes);
                int isOutTime = model.OutTime<DateTime.Now?1:0;
                data.dataObj = new {
                    IsOutTime =isOutTime,
                    OutTimeStr = model.OutTime.ToString("yyyy/MM/dd"),
                    DateArray = dateArray,
                };
                data.code = model.OutTime.AddMonths(-1) < DateTime.Now ? "1" : "0";
                data.isok = true;
                //过期时间1天
                RedisUtil.Set<string>(key, "1", TimeSpan.FromDays(1));
            }

            return data;
        }

        /// <summary>
        /// 清除代理合同提示缓存
        /// </summary>
        /// <param name="agentId"></param>
        public void RemoveOutTime(int agentId)
        {
            string key = string.Format(_redis_AgentOutTimeKey, agentId);
            RedisUtil.Remove(key);
        }

        /// <summary>
        /// 判断代理商合同是否已过期，已过期不给开免费版
        /// </summary>
        /// <returns></returns>
        public bool CheckOutTime(ref List<XcxTemplate> xcxList,Agentinfo agentinfo,int industryid, ref string msg)
        {
            if (xcxList == null || xcxList.Count <= 0 || agentinfo == null)
                return false;

            List<XcxTemplate> feeTemplateList = xcxList.Where(w => w.Price <= 0 || (w.Type == (int)TmpType.小程序专业模板 && industryid == 3)).ToList();
            if (agentinfo.userLevel == 0 && agentinfo.OutTime < DateTime.Now && feeTemplateList != null && feeTemplateList.Count > 0)
            {
                //msg = "您的代理合同已过期，请联系客服！";
                //return true;
                foreach (XcxTemplate item in xcxList)
                {
                    item.Price = item.Price <= 0 ? 10000: item.Price;
                }
            }

            return false;
        }

        public string BuildCookie(Agentinfo agentinfo)
        {
            if (agentinfo == null)
                return string.Empty;
            return DESEncryptTools.DESEncrypt($@"{agentinfo.useraccountid}\r\n{agentinfo.updateitme.ToString("yyyy-MM-dd HH:mm:ss")}");

        }
    }
}
