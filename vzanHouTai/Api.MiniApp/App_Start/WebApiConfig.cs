using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
//using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Api.MiniApp.Filters;
namespace Api.MiniApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务
            // 将 Web API 配置为仅使用不记名令牌身份验证。
            //config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //新增全局JSON格式化DateTime设置
            GlobalJsonFormatConfig();
        }

        /// <summary>
        /// 设置全局JSON格式化设置（修复dateTime转换）
        ///（2018-04-20T15:49:44 改为 '2018-04-20 15:49:44'）
        /// </summary>
        public static void GlobalJsonFormatConfig()
        {
            var jsonConfig = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            jsonConfig.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
            jsonConfig.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
            jsonConfig.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
        }
    }
}
