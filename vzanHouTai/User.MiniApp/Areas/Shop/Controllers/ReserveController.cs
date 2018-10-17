using BLL.MiniApp;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Helper;
using Core.MiniApp;
using Core.MiniApp.Common;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Shop.Filters;
using User.MiniApp.Areas.Shop.Models;
using Utility;


namespace User.MiniApp.Areas.Shop.Controllers
{
    [RouteArea("Shop"), RoutePrefix("Reserve"), Route("{action}/{storeId?}")]
    [LoginFilter(storePara: "store", getAuthStore: true)]
    public class ReserveController : BaseController
    {
        [HttpGet]
        public JsonResult List(DishStore store, int? pageIndex = null, int? pageSize = null)
        {
            List<DishYuDing> printer = DishYuDingBLL.SingleModel.GetListByParams(storeId: store.id, pageIndex :pageIndex, pageSize: pageSize);
            object DTO = new
            {
                page = printer.Select(item => 
                new
                {
                    Id = item.id,
                    SubmitDate = item.addtime.ToString(),
                    ReserveDate = item.yuding_date.ToString(),
                    Name = item.yuding_name,
                    Contact = item.yuding_phone,
                    Seats = item.yuding_renshu,
                    Remark = item.yuding_info, 
                }),
                total = DishYuDingBLL.SingleModel.GetCountByStore(store.id),
            };

            return ApiModel(isok: true, message: "获取成功", data: DTO);
        }
    }
}