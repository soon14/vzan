using DAL.Base;
using Entity.MiniApp;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    public partial class OpenAuthorizerInfoBLL : BaseMySql<OpenAuthorizerInfo>
    {
        #region 单例模式
        private static OpenAuthorizerInfoBLL _singleModel;
        private static readonly object SynObject = new object();

        private OpenAuthorizerInfoBLL()
        {

        }

        public static OpenAuthorizerInfoBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new OpenAuthorizerInfoBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public OpenAuthorizerInfo GetModelByAreaCode(string accountid)
        {
            OpenAuthorizerInfo info = GetModel(string.Format("minisnsid='{0}'", accountid));

            return info;
        }

        public OpenAuthorizerInfo GetModelByAppId(string appid)
        {
            OpenAuthorizerInfo info = GetModel(string.Format("authorizer_appid='{0}'", appid));

            return info;
        }

        public List<OpenAuthorizerInfo> GetListByAppIds(string appids)
        {
            if (string.IsNullOrEmpty(appids))
                return new List<OpenAuthorizerInfo>();

            string sqlwhere = $"authorizer_appid in ({appids})";
            return base.GetList(sqlwhere);
        }
        
    }

    
}