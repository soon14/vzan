using System.Web.Mvc;

namespace User.MiniApp.Areas.Plat
{
    public class PlatAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Plat";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Plat_default",
                "Plat/{controller}/{action}/{id}",
                new { controller = "admin", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}