using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Configuration;

namespace User.MiniApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //routes.IgnoreRoute("{resource}.apk");// 加入此行
            routes.MapMvcAttributeRoutes();
            //AreaRegistration.RegisterAllAreas();

            //routes.MapRoute(
            //"error",
            //"{controller}",
            //new { action = "error" });

            //routes.MapRoute(
            //"articles",
            //"articles/{id}-{typeId}-{page}",
            //new { controller = "Article", action = "Topics" });

            ////Topics
            //routes.MapRoute(
            //"Topics",
            //"Topics/{id}-{typeId}-{page}",
            //new { controller = "friend", action = "Topics" });

            //routes.MapRoute(
            //    name: "cgi-bin",
            //    url: "cgi-bin/{action}",
            //    defaults: new { controller = "cgibin", action = "message", stype = UrlParameter.Optional, method = UrlParameter.Optional },
            //    namespaces: new string[] { "User.MiniApp.Controllers.cgibin" }
            //);
            string pxhWebSiteUrl = WebConfigurationManager.AppSettings["pxhWebSiteUrl"] ?? "";
            if (!string.IsNullOrEmpty(pxhWebSiteUrl))
            {
                string[] urls = pxhWebSiteUrl.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in urls)
                {
                    routes.Add(item, new DomainRoute(
                    item,
                    "{controller}/{action}",
                    new { controller = "pxh", action = "Index", id = "" }
                ));
                }
                
            }

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
              defaults: new { controller = "dzhome", action = "newHome", id = UrlParameter.Optional },
                namespaces: new string[] { "User.MiniApp.Controllers" }
            );
            routes.MapRoute(
                name: "index",
                url: "{controller}/{action}",
              defaults: new { controller = "dzhome", action = "newHome" }
            );
            routes.MapRoute(
                name: "agency",
                url: "{controller}/{action}",
              defaults: new { controller = "dzhome", action = "newHome" }
            );

            routes.MapRoute(
               name: "DefaultMultiStore",
               url: "MultiStore/{controller}/{action}/{id}",
               defaults: new { controller = "UserLogin", action = "Login", id = UrlParameter.Optional },
               namespaces: new string[] { "User.MiniApp.Areas.MultiStore.Controllers" }
           );

        }
    }
}
