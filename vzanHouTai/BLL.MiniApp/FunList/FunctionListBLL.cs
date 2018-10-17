using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp.Conf;
using Entity.MiniApp.FunctionList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.FunList
{
    public class FunctionListBLL : BaseMySql<FunctionList>
    {
        #region 单例模式
        private static FunctionListBLL _singleModel;
        private static readonly object SynObject = new object();

        private FunctionListBLL()
        {

        }

        public static FunctionListBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FunctionListBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<VersionType> GetVersionTypeList(int TemplateType = 22,int agenttype=0)
        {
            string sqlwhere = $"TemplateType={TemplateType}";
            if(agenttype==1)
            {
                sqlwhere += " and Price<=0";
            }
            List<FunctionList> list = base.GetList(sqlwhere);
            List<VersionType> listVersionType = new List<VersionType>();
            foreach (var item in list)
            {
                listVersionType.Add(new VersionType()
                {
                    VersionId = item.VersionId,
                    VersionName = item.VersionName,
                    VersionPrice = item.Price.ToString()

                });
            }
            return listVersionType.OrderByDescending(x => x.VersionId).ToList();
        }

        /// <summary>
        /// 获取专业版各个版本的价格
        /// </summary>
        /// <param name="agentid"></param>
        /// <param name="templateType"></param>
        /// <param name="tid"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public FunctionList GetModelBytid(int agentid,int templateType,int tid,int versionId)
        {
            FunctionList model = base.GetModel($"TemplateType={templateType} and VersionId={versionId}");
            Xcxtemplate_Price Xcxtemplate_Pricemodel = Xcxtemplate_PriceBLL.SingleModel.GetModelByAgentIdAndTid(agentid, tid, versionId);
            if(Xcxtemplate_Pricemodel!=null)
            {
                model.Price = Xcxtemplate_Pricemodel.price;
            }

            return model;
        }

        public FunctionList GetModelByTypeAndVId(int tmptype,int versionId)
        {
            return base.GetModel($"TemplateType={tmptype} and VersionId={versionId}");
        }
    }
}
