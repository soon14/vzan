using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace User.MiniApp.Areas.DishVipCardDish.Models
{
    /// <summary>
    /// 轮播图编辑视图model
    /// </summary>
    public class LunboEditViewModel
    {
        public DishPicture picture { get; set; } = new DishPicture();

        public List<DishStore> storeList { get; set; } = new List<DishStore>();

    }
}