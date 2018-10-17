using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.IO;

namespace OpenWx
{
    public class MvcApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            this.StartLog4net();
        }
        
        /// <summary>
        /// 启动Log4Net  通过程序启动Log
        /// </summary>
        private void StartLog4net()
        {
            string filePath = HttpRuntime.AppDomainAppPath.ToString() + "Config\\Log4net.config";
            if (!File.Exists(filePath))
            {
                return;
            }
            FileInfo fileInfo = new FileInfo(filePath);
            log4net.Config.XmlConfigurator.ConfigureAndWatch(fileInfo);
        }
    }
}
