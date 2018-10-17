using BLL.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// 餐饮多门店登录管理者的Id
        /// </summary>
        protected Int32 dishAdminId
        {
            get
            {
                int dishAdminId = 0;
                string dishAdminIdStr = CookieHelper.GetCookie("dz_DishAdminId");
                if (!string.IsNullOrEmpty(dishAdminIdStr))
                {
                    Int32.TryParse(dishAdminIdStr, out dishAdminId);
                }

                return dishAdminId;
            }
        }

        /// <summary>
        /// 当前餐饮多门店用户
        /// </summary>
        public  Entity.MiniApp.Dish.DishAdminUser dishAdmin
        {
            get
            {
                return DishAdminUserBLL.SingleModel.GetModel(dishAdminId);
            }
        }
    }
}