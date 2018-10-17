using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp;
using System;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Utility.MemberLogin;

namespace User.MiniApp.Filters
{
    public class ValidateAttribute : ActionFilterAttribute 
    {
        /// <summary>
        /// Action 执行之前先执行此方法
        /// </summary>
        /// <param name="filterContext">过滤器上下文</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            int id = Utility.EncodeHelper.ConvertInt(GetPath(3), 0);
            if (id == 0) { int.TryParse(filterContext.ActionParameters["id"]?.ToString(), out id); }
            ViewResult vi = new ViewResult();
            vi.ViewName = "Error";
        }

        /// <summary>
        /// string
        /// </summary>
        private string GetPath(int index)
        {
            string strVal = string.Empty;
            string path = System.Web.HttpContext.Current.Request.FilePath;
            if (!string.IsNullOrEmpty(path))
            {
                string[] array = path.Split('/');
                if (array != null && array.Length > index)
                {
                    strVal = array[index].ToLower();
                }
            }
            return strVal;
        }
    }
}