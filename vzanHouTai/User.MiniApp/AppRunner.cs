using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
//using Mcx.Infrastructure.WebBase.StartupConfig;
//using Mcx.WebMvcCore.Configuration;
using Entity.MiniApp.User;
//using Mcx.WebMvcCore.SystemExtents.IdentitySecurity;

[assembly: OwinStartup(typeof(WebUI.MiniAppAdmin.AppRunner))]

namespace WebUI.MiniAppAdmin
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AppRunner 
    {
       
    }
}
