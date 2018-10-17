using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entity.MiniApp.Dish;
using Entity.MiniApp.cityminiapp;

namespace User.MiniApp.Areas.Dish.Models
{
    [Serializable]
    public class StoreInfoView
    {
        public DishStore store { get; set; }

        //列名字符串,暂用于标识哪些列需要更新
        public string storeColNames { get; set; }

        public Entity.MiniApp.Dish.DishAdminUser admin { get; set; }
    }
}