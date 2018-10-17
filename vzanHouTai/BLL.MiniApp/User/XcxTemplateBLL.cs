using BLL.MiniApp.Conf;
using BLL.MiniApp.FunList;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.FunctionList;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp
{
    public class XcxTemplateBLL : BaseMySql<XcxTemplate>
    {
        private static string _redis_XcxTemplateModelKey = "XcxTemplateModelKey_{0}";

        #region 单例模式
        private static XcxTemplateBLL _singleModel;
        private static readonly object SynObject = new object();

        private XcxTemplateBLL()
        {

        }

        public static XcxTemplateBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new XcxTemplateBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        #region 缓存操作
        public void RemoveRedis(int id)
        {
            string key = string.Format(_redis_XcxTemplateModelKey, id);
            RedisUtil.Remove(key);
        }

        public override XcxTemplate GetModel(int Id)
        {
            string key = string.Format(_redis_XcxTemplateModelKey, Id);
            XcxTemplate model = RedisUtil.Get<XcxTemplate>(key);
            if(model==null)
            {
                model = base.GetModel(Id);
                RedisUtil.Set<XcxTemplate>(key,model);
            }

            return model;
        }
        public override bool Update(XcxTemplate model)
        {
            RemoveRedis(model.Id);
            return base.Update(model);
        }
        #endregion

        public XcxTemplate GetTModel(int tid)
        {
            return base.GetModel(string.Format("Id={0} and State>=0", tid));
        }

        public List<XcxTemplate> GetListByIds(string tids)
        {
            if (string.IsNullOrEmpty(tids))
                return new List<XcxTemplate>();

            return base.GetList($"Id in ({tids})");
        }

        /// <returns></returns>
        /// <summary>
        /// 获取需要付费的模板
        /// </summary>
        /// <param name="type">0：所有模板，1：付费模板，2：免费模板</param>
        /// <returns></returns>
        public List<XcxTemplate> GetListByPriceType(int type)
        {
            string sqlWhere = $"State >=0 and ProjectType={(int)ProjectType.小程序} ";
            switch(type)
            {
                case 1:sqlWhere+=" and price>0"; break;
                case 2: sqlWhere += " and price=0"; break;
            }
            return base.GetList(sqlWhere);
        }

        /// <summary>
        /// 根据项目类型获取小程序模板
        /// </summary>
        /// <param name="ProjectType"></param>
        /// <returns></returns>
        public List<XcxTemplate> GetListByIdsandProjectType(int ProjectType)
        {
            return base.GetList($"ProjectType={ProjectType} and state>=0", 2000, 1, "", "sort asc");
        }
        public XcxTemplate GetModelByType(int typeid)
        {
            return GetModel($"Type={typeid} and state>=0");
        }
        public List<XcxTemplate> GetListByTypes(string typeids)
        {
            if (string.IsNullOrEmpty(typeids))
                return new List<XcxTemplate>();
            return base.GetList($"Type in({typeids}) and state>=0");
        }
        /// <summary>
        /// 获取专业版里的版本代理商自定义价格
        /// </summary>
        /// <param name="TemplateType"></param>
        /// <param name="agentid"></param>
        /// <returns></returns>
        public List<VersionType> GetRealPriceVersionTemplateList(int TemplateType, int agentid, int agenttype = 0)
        {

            List<VersionType> listVersionType = FunctionListBLL.SingleModel.GetVersionTypeList(TemplateType, agenttype);
            List<VersionType> list = new List<VersionType>();
            int tid = 0;
            XcxTemplate xcxTemplate = base.GetModel($"Type={TemplateType}");
            if (xcxTemplate != null)
            {
                tid = xcxTemplate.Id;
            }
            if (listVersionType != null && listVersionType.Count > 0)
            {

                foreach (var item in listVersionType)
                {
                    Xcxtemplate_Price template_Price = Xcxtemplate_PriceBLL.SingleModel.GetModel($"tid={tid} and agentid='{agentid}' and state=0 and VersionId={item.VersionId}");

                    if (template_Price != null)
                    {
                        if (template_Price.price > 0 || item.VersionId == 3)
                        {
                            list.Add(new
                            VersionType
                            {
                                VersionId = item.VersionId,
                                VersionName = item.VersionName,
                                VersionPrice = template_Price.price.ToString(),
                                LimitCount = template_Price.LimitCount
                            });
                        }
                        else
                        {
                            list.Add(item);
                        }


                    }
                    else
                    {
                        list.Add(item);
                    }


                }
            }
            return list;

        }

        public List<XcxTemplate> GetRealPriceTemplateListV2(int agentid, string ceshitype, int agentType)
        {
            string sqlwhere = $" projectType={(int)ProjectType.小程序} and state>=0";
            if (ceshitype.Length > 0)
            {
                sqlwhere += $" or type in({ceshitype}) ";
            }
            if (agentType == 1)
            {
                sqlwhere += $" and (price<=0 or type={(int)TmpType.小程序专业模板})";
            }

            return GetRealPriceTemplateList(sqlwhere, agentid);
        }

        public List<XcxTemplate> GetRealPriceTemplateList(string sqlwhere, int agentid)
        {

            List<XcxTemplate> templateList = GetList(sqlwhere, 2000, 1, "", "sort asc");
            List<XcxTemplate> list = null;
            if (templateList != null && templateList.Count > 0)
            {
                list = new List<XcxTemplate>();
                foreach (XcxTemplate xcxmodel in templateList)
                {
                    Xcxtemplate_Price template_Price = Xcxtemplate_PriceBLL.SingleModel.GetModel($"tid={xcxmodel.Id} and agentid='{agentid}' and state>=0");
                    if (template_Price != null)
                    {
                        if (xcxmodel.Type == (int)TmpType.小未平台子模版)
                        {
                            //自定义代理开通模板价格
                            xcxmodel.Price = template_Price.price >= 0 ? template_Price.price : xcxmodel.Price;
                        }
                        else
                        {
                            //自定义代理开通模板价格
                            xcxmodel.Price = template_Price.price > 0 ? template_Price.price : xcxmodel.Price;
                        }
                        //自定义代理开通多门店模板增加分店价格
                        xcxmodel.SPrice = template_Price.SPrice > 0 ? template_Price.SPrice : xcxmodel.SPrice;
                        //自定义代理开通模板价格
                        xcxmodel.SCount = template_Price.SCount > 0 ? template_Price.SCount : xcxmodel.SCount;
                        //免费版限制开通数量
                        if (xcxmodel.Price <= 0)
                        {
                            xcxmodel.LimitCount = template_Price.LimitCount <= 0 ? 1000 : template_Price.LimitCount;
                        }
                    }

                    list.Add(xcxmodel);
                }
            }
            return list;
        }

        public List<XcxTemplateDetail> GetSaleDetailList(int agentId, int agenttype, string ceshitype = "")
        {
            List<XcxTemplateDetail> list = null;
            // string sql = $"select Tname,a.* from xcxtemplate LEFT JOIN (select Tid, count(*) as count ,sum(price) as sum from xcxappaccountrelation where agentid={agentId} group by Tid ) as a on xcxtemplate.Id = a.tid where xcxtemplate.state = 0 and xcxtemplate.ProjectType = 2";
            string sql = $"select a.*, xcxtemplate.TName from  xcxtemplate LEFT JOIN (select sum(cost) as sum,type, count(1) as count,tid from agentdepositlog where customerid='{agentId}' and type=3 group by tid ) as a  on a.tid = xcxtemplate.Id where xcxtemplate.state = 0 and (xcxtemplate.ProjectType in({(int)ProjectType.小程序},{(int)ProjectType.测试}) {(ceshitype.Length > 0 ? $" or xcxtemplate.type in ({ceshitype})" : "")}) and xcxtemplate.state>=0";
            if (agenttype == 1)
            {
                sql += $" and (xcxtemplate.price<=0 or xcxtemplate.type={(int)TmpType.小程序专业模板})";
            }
            DataSet ds = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (ds.Tables.Count <= 0) return list;
            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows.Count <= 0) return list;
            list = new List<XcxTemplateDetail>();
            foreach (DataRow dr in dt.Rows)
            {
                XcxTemplateDetail detail = new XcxTemplateDetail();
                if (dr["Tname"] != DBNull.Value)
                {
                    detail.name = dr["Tname"].ToString();
                }
                else
                {
                    detail.name = string.Empty;
                }

                if (dr["sum"] != DBNull.Value)
                {
                    detail.sum = Convert.ToInt32(dr["sum"]);
                }
                else
                {
                    detail.sum = 0;
                }

                if (dr["count"] != DBNull.Value)
                {
                    detail.Count = Convert.ToInt32(dr["count"]);
                }
                else
                {
                    detail.Count = 0;
                }
                detail.showsum = (detail.sum * 0.01).ToString("0.00");
                list.Add(detail);
            }
            return list;
        }

        /// <summary>
        /// 获取代理开通企业智推版的信息
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public XcxTemplate GetAgentTemplate(int agentId)
        {
            XcxTemplate xcxtemp = GetModelByType((int)TmpType.企业智推版);
            if (xcxtemp != null)
            {
                List<XcxTemplate> xcxList = GetRealPriceTemplateList($" id in ({xcxtemp.Id})", agentId);
                if (xcxList != null && xcxList.Count > 0)
                {
                    return xcxList[0];
                }
            }

            return new XcxTemplate();
        }

        /// <summary>
        /// 获取代理为用户开通模板数据
        /// </summary>
        /// <param name="accountIds"></param>
        /// <param name="ceshiType"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public List<XcxTemplate> GetAgentOpenXcxTemplateList(string accountIds, string ceshiType, int agentId)
        {
            List<XcxTemplate> list = new List<XcxTemplate>();
            string sql = $"select t.tname,t.Type,x.price,x.accountid from XcxTemplate t right join XcxAppAccountRelation x on t.id=x.tid where accountId in ({accountIds}) and (projectType in({(int)ProjectType.小程序},{(int)ProjectType.测试}) {(ceshiType.Length > 0 ? $" or type in({ceshiType})" : "")}) and agentId={agentId} and x.state>=0";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    XcxTemplate model = base.GetModel(dr);
                    model.industr = dr["accountid"].ToString();

                    list.Add(model);
                }
            }
            
            return list;
        }
    }
}
