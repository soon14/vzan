using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OpenWx
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "Open",
               url: "Open/Callback/{appId}",
               defaults: new { controller = "Open", action = "Callback", appId = UrlParameter.Optional }
           );
           
            routes.MapRoute(
             name: "messgepush",
             url: "pushmessage/{whichmenulong}",
             defaults: new { controller = "cgibin", action = "pushmessage", stype = UrlParameter.Optional }
           );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Default2",
                url: "{controller}/{action}-{id}",
                defaults: new { controller = "f", action = "s", id = UrlParameter.Optional }
            );
        
            routes.MapRoute(
                name: "cgi-bin",
                url: "cgi-bin/{action}/{stype}/{method}",
                defaults: new { controller = "cgibin", action = "message", stype = UrlParameter.Optional, method = UrlParameter.Optional }
            );
          
        }
    }
}
