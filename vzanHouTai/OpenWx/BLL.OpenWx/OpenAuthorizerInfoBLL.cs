using Core.OpenWx;
using DAL.Base;
using Entity.OpenWx;
using System;
using static Entity.OpenWx.MiniAppEnum;

namespace BLL.OpenWx
{
    public class OpenAuthorizerInfoBLL : BaseMySql<OpenAuthorizerInfo>
    {
        private object _lockObject = new object();
        private int _minutes = 60;

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

        private static string accestoenkey = "chx_xcx_openauthorizerinfo_{0}";//chx_xcx_openauthorizerinfo_gh_5982b74237c5
        
        public bool UpdateModel(OpenAuthorizerInfo model)
        {
            string key = string.Format(accestoenkey, model.user_name);
            bool result = base.Update(model);
            RedisUtil.Remove(key);
            return result;
        }

        public OpenAuthorizerInfo getCurrentModel(string user_name, bool isreflsh = false)
        {
            string cachekey = string.Format(accestoenkey, user_name);
            OpenAuthorizerInfo model = RedisUtil.Get<OpenAuthorizerInfo>(cachekey);
            if(model!=null)
            {
                if (!(isreflsh || model.refreshtime.AddMinutes(_minutes) < DateTime.Now || string.IsNullOrEmpty(model.authorizer_access_token)))
                {
                    return model;
                }
            }
            model = GetModel(string.Format("user_name='{0}'", user_name));
            if (model == null)
            {
                throw new Exception("没有第三方授权信息:username=" + user_name);
            }
            lock(_lockObject)
            {
                model = RedisUtil.Get<OpenAuthorizerInfo>(cachekey);
                if (model != null)
                {
                    if (!(isreflsh || model.refreshtime.AddMinutes(_minutes) < DateTime.Now || string.IsNullOrEmpty(model.authorizer_access_token)))
                    {
                        return model;
                    }
                }

                model = GetModel(string.Format("user_name='{0}'", user_name));
                if (model == null)
                {
                    throw new Exception("没有第三方授权信息:username=" + user_name);
                }

                if (isreflsh || model.refreshtime.AddMinutes(_minutes) < DateTime.Now || string.IsNullOrEmpty(model.authorizer_access_token))//刷新token
                {
                    var currentmodel = OpenPlatConfigBLL.SingleModel.getCurrentModel();
                    string compenettoken = currentmodel.component_access_token;
                    string compenetappid = currentmodel.component_Appid;
                    try
                    {
                        RefreshAuthorizerTokenResult token = WxRequest.GetAuthoRizerToken(model.authorizer_appid,currentmodel.component_access_token,currentmodel.component_Appid,model.authorizer_refresh_token);
                        if (token != null && !string.IsNullOrEmpty(token.authorizer_access_token))
                        {
                            model.authorizer_access_token = token.authorizer_access_token;
                            model.authorizer_refresh_token = token.authorizer_refresh_token;
                            model.refreshtime = DateTime.Now;
                            UpdateModel(model);
                        }
                        else
                        {
                            if(token.errcode == ReturnCodeEnum.已取消授权)
                            {
                                return null;
                            }
                            log4net.LogHelper.WriteInfo(this.GetType(),  $"{user_name}授权公众号刷新token失败：{token.errcode.ToString()}");
                        }
                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteError(this.GetType(),ex);
                    }
                }
                RedisUtil.Set<OpenAuthorizerInfo>(cachekey, model, TimeSpan.FromMinutes(_minutes));
            }
            
            return model;
        }
        
        public OpenAuthorizerInfo  GetModelByAppId(string appid)
        {
            return GetModel($"authorizer_appid='{appid}'");
        }
    }
}
