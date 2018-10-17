using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace User.MiniApp.Areas.MultiStore.Models.Manage
{
    /// <summary>
    /// 
    /// </summary>
    public class ManageLoginsViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }
}