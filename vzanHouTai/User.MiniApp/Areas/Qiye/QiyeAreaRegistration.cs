using System.Web.Mvc;

namespace User.MiniApp.Areas.Qiye
{
    public class QiyeAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Qiye";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Qiye_default",
                "Qiye/{controller}/{action}/{id}",
                new { controller = "admin", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}