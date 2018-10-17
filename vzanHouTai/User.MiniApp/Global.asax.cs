using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Core.MiniApp;
using log4net;
using log4net.Config;
using User.MiniApp.Controllers;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Tools;
using BLL.MiniApp.Ent;
using BLL.MiniApp;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin.Entities;
using Senparc.Weixin;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Containers;
using System.Configuration;

namespace User.MiniApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            XmlConfigurator.Configure();
            //App_Start.AutofacConfig.Register();
            AutoMapperCore.Configure();
            #region 盛派SDK配置
            /* 
             * CO2NET 全局注册开始
             * 建议按照以下顺序进行注册
             */

            /*
             * CO2NET 是从 Senparc.Weixin 分离的底层公共基础模块，经过了长达 6 年的迭代优化。
             * 关于 CO2NET 在所有项目中的通用设置可参考 CO2NET 的 Sample：
             * https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
             */


            //设置全局 Debug 状态
            var isGLobalDebug = true;
            //全局设置参数，将被储存到 Senparc.CO2NET.Config.SenparcSetting
            var senparcSetting = SenparcSetting.BuildFromWebConfig(isGLobalDebug);
            //也可以通过这种方法在程序任意位置设置全局 Debug 状态：
            //Senparc.CO2NET.Config.IsDebug = isGLobalDebug;


            //CO2NET 全局注册，必须！！
            IRegisterService register = RegisterService.Start(senparcSetting).UseSenparcGlobal();

            /* 微信配置开始
             * 建议按照以下顺序进行注册
             */

            //设置微信 Debug 状态
            var isWeixinDebug = true;
            //全局设置参数，将被储存到 Senparc.Weixin.Config.SenparcWeixinSetting
            var senparcWeixinSetting = SenparcWeixinSetting.BuildFromWebConfig(isWeixinDebug);
            //也可以通过这种方法在程序任意位置设置微信的 Debug 状态：
            //Senparc.Weixin.Config.IsDebug = isWeixinDebug;

            //微信全局注册，必须！！
            register.UseSenparcWeixin(senparcWeixinSetting, senparcSetting)


            #region 注册公众号或小程序（按需）

                //注册公众号（可注册多个）
                .RegisterMpAccount(senparcWeixinSetting, "【拼享惠】公众号")
                //注册多个公众号或小程序（可注册多个）
                // .RegisterWxOpenAccount(senparcWeixinSetting, "【盛派网络小助手】小程序")

                //除此以外，仍然可以在程序任意地方注册公众号或小程序：
                //AccessTokenContainer.Register(appId, appSecret, name);//命名空间：Senparc.Weixin.MP.Containers
            #endregion
            ;

            AccessTokenContainer.Register(ConfigurationManager.AppSettings["webview_appid"], ConfigurationManager.AppSettings["webview_appsecret"]);
            /* 微信配置结束 */
            #endregion

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            AppHelper.Log(this);
        }
    }
}
