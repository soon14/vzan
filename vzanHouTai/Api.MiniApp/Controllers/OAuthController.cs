using Core.MiniApp.WeiXin;
using DAL.Base;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Api.MiniApp.Controllers
{
    public class OAuthController : Controller
    {
        public ActionResult CallBack(string code, string state, string returnUrl)
        {
            if(!WxOAuth.SingleModel.CheckToken(state))
            {
                return new ContentResult() { Content = "token已过期，请重新授权认证" };
            }

            if(!WxOAuth.SingleModel.ExpireToken(state))
            {
                return new ContentResult() { Content = "token设置过期失败" };
            }

            //通过回调函数返回的code来获取令牌
            var accessToken = WxOAuth.SingleModel.GetAccessToken(code);
            if (accessToken.errcode != ReturnCode.请求成功)
            {
                //如果令牌的错误信息不等于请求成功，则需要重新返回授权界面
                return Redirect(WxOAuth.SingleModel.GetSuccessCallBack(state));
            }

            try
            {
                OAuthUserInfo userInfo = WxOAuth.SingleModel.GetOAuthUserInfo(accessToken);
                if(!WxOAuth.SingleModel.WriteOAuthInfo(state, userInfo))
                {
                    return new ContentResult { Content = "写入授权失败" };
                }
                return Redirect(WxOAuth.SingleModel.GetSuccessCallBack(state));
            }
            catch(Exception ex)
            {
                return new ContentResult() { Content = ex.Message };
            }
        }
    }
}