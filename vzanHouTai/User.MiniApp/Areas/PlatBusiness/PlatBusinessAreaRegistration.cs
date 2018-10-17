using System.Web.Mvc;

namespace User.MiniApp.Areas.PlatBusiness
{
    public class PlatBusinessAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PlatBusiness";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PlatBusiness_default",
                "PlatBusiness/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}