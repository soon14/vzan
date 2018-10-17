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
using User.MiniApp.Areas.DishAdmin.Models;
using User.MiniApp.Areas.Shop.Filters;
using User.MiniApp.Areas.Shop.Models;
using Utility;


namespace User.MiniApp.Areas.Shop.Controllers
{
    [RouteArea("Shop"), RoutePrefix("Checkout"), Route("{action}/{storeId?}")]
    [LoginFilter(storePara: "store", getAuthStore: true)]
    public class CheckoutController : BaseController
    {
        [HttpGet]
        public JsonResult List(DishStore store, int pageIndex = 1, int pageSize = 10, int payType = 0, DateTime? queryBegin =null, DateTime? queryEnd = null)
        {
            int total;
            List<PayRecordModel> record = PayRecordBLL.SingleModel.GetList(store.aid, store.id, pageIndex - 1, pageSize, out total, payType, queryBegin, queryEnd);
            object DTO = new
            {
                page = record.Select(item => 
                new
                {
                    NickName = item.NickName,
                    HeadImg = item.HeadImgUrl,
                    PayType = item.PayType,
                    Amount = item.Money,
                    Detail = item.Info,
                    PayDate = item.AddTime.ToString(),
                }),
                total = total,
            };

            return ApiModel(isok: true, message: "获取成功", data: DTO);
        }
    }
}