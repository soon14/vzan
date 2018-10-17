using BLL.OpenWx;
using DAL.Base;
using Entity.OpenWx;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace BLL.OpenWx
{
    public partial class UserXcxTemplateBLL : BaseMySql<UserXcxTemplate>
    {
        #region 单例模式
        private static UserXcxTemplateBLL _singleModel;
        private static readonly object SynObject = new object();

        private UserXcxTemplateBLL()
        {

        }

        public static UserXcxTemplateBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UserXcxTemplateBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public UserXcxTemplate GetTModel(int tid,string userid,string appid)
        {
            var model = GetModel(string.Format("TId={0} and AppId='{1}' and TuserId='{2}'", tid, appid,userid));
           
            return model;
        }

        public UserXcxTemplate GetModelByUserName(string username)
        {
            var model = GetModel(string.Format("TuserId='{0}'", username));

            return model;
        }

        public UserXcxTemplate GetModelByAppId(string appId)
        {
            var model = GetModel(string.Format("appId='{0}'", appId));

            return model;
        }

        public bool UpdateUserTemplate(string appid,string accountid)
        {
            if (base.ExecuteNonQuery($"Update UserXcxTemplate set State='-2',tid=0,Reason='',Desc='' where AppId = '{appid}' and TuserId='{accountid}'") <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
