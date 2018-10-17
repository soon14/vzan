using Mcx.WebMvcCore.Configuration;
using Mcx.WebMvcCore.SystemExtents.IdentitySecurity;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entity.MiniApp.User;

namespace WebUI.MiniAppAdmin
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AppRunner : SystemConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        protected override void InvokeConfigure(IAppBuilder app)
        {
            base.InvokeConfigure(app);
        }
    }
}