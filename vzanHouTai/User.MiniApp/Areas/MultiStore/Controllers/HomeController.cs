using Mcx.WebMvcCore.SystemExtents.IdentitySecurity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Linq.Expressions;
using Mcx.Infrastructure.DbFactory;
using Mcx.WebMvcCore.SystemExtents.SystemDb;
using Mcx.WebMvcCore.Model;
using Mcx.WebMvcCore.SystemExtents.IdentitySecurity.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using Mcx.Infrastructure.DbFactory.Repository;

namespace WebUI.MiniAppAdmin.Areas.MultiStore.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class HomeController : WebUI.MiniAppAdmin.Controllers.baseController
    {
        /// <summary>
        /// 
        /// </summary>
        private RepositoryContext _DbContext;

        public RepositoryContext UserContext
        {
            get
            {
                return _DbContext ?? HttpContext.GetOwinContext().Get<IDbContextManager>().SystemUserRepository;
            }
            private set
            {
                _DbContext = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private RepositoryContext _DefaultContext;

        public RepositoryContext DefaultContext
        {
            get
            {
                return _DefaultContext ?? HttpContext.GetOwinContext().Get<IDbContextManager>().DefaultRepository;
            }
            private set
            {
                _DefaultContext = value;
            }
        }


        private SystemUserManager _userManager;

        public SystemUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<SystemUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: MultiStore/Home
        public async Task<ActionResult> Index()
        {
            SystemDbContext<SystemUser> C = (SystemDbContext<SystemUser>)this.UserContext.Db;

            MySqlDefault<Entity.MiniApp.User.MiniAccount> DefaultDb = DefaultContext.UsedOrginalDb<Entity.MiniApp.User.MiniAccount>();


            var Hy = DefaultDb.GetList();

            if (C != null)
            {
                try
                {

                    //var V = this.UserManager.FindByIdAsync<MiniAccount>(
                    //    this.UserManager.Users.ToList().FirstOrDefault().Id,
                    //    HttpContext.GetOwinContext());

                    //SystemUser User = new SystemUser();

                    //User.UserName = "sadf";
                    //User.Email = "df@126.com";
                    //User.MiniAccountList = new List<MiniAccount>();

                    //MiniAccount account1 = C.MiniAccounts.Create();

                    //account1.AccountId = Guid.NewGuid().ToString();

                    //MiniAccount account2 = C.MiniAccounts.Create();

                    //account2.AccountId = Guid.NewGuid().ToString();

                    //User.MiniAccountList.Add(account1);
                    //User.MiniAccountList.Add(account2);

                    //C.SaveChanges();

                    //var result = await UserManager.CreateAsync(User, "Asgsg/12323");

                    //if (result.Succeeded)
                    //{


                    //}
                }
                catch (Exception ex)
                {

                }
            }

            return View();
        }


        public ActionResult test()
        {
            ViewBag.PageType = 6;
            return View();
        }

        public async Task<IdentityResult> Register()
        {
            IdentityResult result = await UserManager.CreateAsync(new SystemUser() { UserName  = "sadf", Email = "df@126.com"},"35235");

            return result;
        }

    }
}