using AutoMapper;
using BLL.MiniApp;
using BLL.MiniApp.Dish;
using Core.MiniApp;
using Core.MiniApp.Common;
using Core.MiniApp.DTO;
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
    [RouteArea("Shop"), RoutePrefix("Setting"), Route("{action}/{storeId?}")]
    [LoginFilter(storePara: "store", getAuthStore: true)]
    public class SettingController : BaseController
    {
        [HttpGet]
        public JsonResult Base(DishStore store)
        {
            EditBaseConfig config = JsonConvert.DeserializeObject<EditBaseConfig>(store.baseConfigJson);
            config.logo = store.dish_logo;
            config.storename = store.dish_name;
            config.lat = store.ws_lat;
            config.lng = store.ws_lng;
            return ApiModel(isok: true, message: "获取成功", data: config);
        }

        [HttpPost, Route("Base/Save"), FormValidate]
        public JsonResult Base(DishStore store, [System.Web.Http.FromBody]EditBaseConfig config)
        {
            store.dish_name = config.storename;
            store.dish_logo = config.logo;
            store.ws_lat = config.lat;
            store.ws_lng = config.lng;
            if (string.IsNullOrWhiteSpace(config.dish_address))
            {
                return ApiModel(message: "请填写商家地址");
            }
            if (string.IsNullOrWhiteSpace(config.dish_jieshao))
            {
                return ApiModel(message: "请填写门店介绍");
            }
            if (string.IsNullOrWhiteSpace(config.dish_con_mobile))
            {
                return ApiModel(message: "请填写门店手机号码");
            }
            if (string.IsNullOrWhiteSpace(config.dish_fuwu))
            {
                return ApiModel(message: "请填写提供服务");
            }

            store.baseConfigJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<DishBaseConfig>(JsonConvert.SerializeObject(config)));
            bool success = DishStoreBLL.SingleModel.Update(store, "baseConfigJson");
            return ApiModel(isok: success, message: "保存成功");
        }

        [HttpGet]
        public JsonResult Advance(DishStore store)
        {
            return ApiModel(isok: true, message: "获取成功", data: JsonConvert.DeserializeObject<DishGaojiConfig>(store.gaojiConfigJson));
        }

        [HttpPost, Route("Advance/Save")]
        public JsonResult Advance(DishStore store, [System.Web.Http.FromBody]DishGaojiConfig config)
        {
            if(string.IsNullOrWhiteSpace(config.dish_beizhu_info))
            {
                return ApiModel(message: "请填写点餐备注");
            }

            store.gaojiConfig = JsonConvert.DeserializeObject<DishGaojiConfig>(store.gaojiConfigJson);
            //不更改WebView设置
            config.dish_is_webview_open = store.gaojiConfig.dish_is_webview_open;
            config.dish_webview_text = store.gaojiConfig.dish_webview_text;
            config.dish_webview_url= store.gaojiConfig.dish_webview_url;
            //保存其它设置
            store.gaojiConfigJson = JsonConvert.SerializeObject(config);
            bool success = DishStoreBLL.SingleModel.Update(store, "gaojiConfigJson");
            return ApiModel(isok: success, message: "保存成功");
        }

        [HttpGet]
        public JsonResult DineIn(DishStore store)
        {
            return ApiModel(isok: true, message: "获取成功", data: JsonConvert.DeserializeObject<DishDianneiConfig>(store.dianneiConfigJson));
        }

        [HttpPost, Route("DineIn/Save")]
        public JsonResult DineIn(DishStore store, [System.Web.Http.FromBody]DishDianneiConfig config)
        {
            store.dianneiConfigJson = JsonConvert.SerializeObject(config);
            bool success = DishStoreBLL.SingleModel.Update(store, "dianneiConfigJson");
            return ApiModel(isok: success, message: "保存成功");
        }

        [HttpGet]
        public JsonResult TakeOut(DishStore store)
        {
            return ApiModel(isok: true, message: "获取成功", data: JsonConvert.DeserializeObject<DishTakeoutConfig>(store.takeoutConfigJson));
        }

        [HttpPost, Route("TakeOut/Save")]
        public JsonResult TakeOut(DishStore store, [System.Web.Http.FromBody]DishTakeoutConfig config)
        {
            store.takeoutConfigJson = JsonConvert.SerializeObject(config);
            bool success = DishStoreBLL.SingleModel.Update(store, "takeoutConfigJson");
            return ApiModel(isok: success, message: "保存成功");
        }

        [HttpGet]
        public JsonResult Payment(DishStore store)
        {
            return ApiModel(isok: true, message: "获取成功", data: JsonConvert.DeserializeObject<DishPaySetting>(store.paySettingJson));
        }

        [HttpPost, Route("Payment/Save")]
        public JsonResult Payment(DishStore store, [System.Web.Http.FromBody]DishPaySetting config)
        {
            store.paySettingJson = JsonConvert.SerializeObject(config);
            bool success = DishStoreBLL.SingleModel.Update(store, "paySettingJson");
            return ApiModel(isok: success, message: "保存成功");
        }
    }
}