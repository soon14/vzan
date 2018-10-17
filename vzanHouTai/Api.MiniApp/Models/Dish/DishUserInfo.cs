using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.MiniApp.Models.Dish
{
    public class DishUserInfo
    {
        public string u_name { get; set; } = string.Empty;
        public int u_sex { get; set; } = 0;

        public string u_phone { get; set; } = string.Empty;
        public string u_address { get; set; } = string.Empty;
    }
}