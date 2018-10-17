using System.Web.Mvc;

namespace User.MiniApp.Areas.PlatChild
{
    public class PlatChildAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PlatChild";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PlatChild_default",
                "PlatChild/{controller}/{action}/{id}",
                new { controller = "admin", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}