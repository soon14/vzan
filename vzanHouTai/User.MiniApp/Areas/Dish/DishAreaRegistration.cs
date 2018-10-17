using System.Web.Mvc;

namespace User.MiniApp.Areas.Dish
{
    public class DishAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Dish";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Dish_default",
                "Dish/{controller}/{action}/{id}",
                new { controller = "admin", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}