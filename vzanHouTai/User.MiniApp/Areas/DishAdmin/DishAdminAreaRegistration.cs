using System.Web.Mvc;

namespace User.MiniApp.Areas.DishAdmin
{
    public class DishAdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DishAdmin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DishAdmin_default",
                "DishAdmin/{controller}/{action}/{id}",
                new { controller = "main", action = "Index", id = UrlParameter.Optional },
                new string[] { "User.MiniApp.Areas.DishAdmin.Controllers" }
            );
        }
    }
}