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
    [RouteArea("Shop"), RoutePrefix("Product"), Route("{action}/{productId?}")]
    [LoginFilter(storePara: "store", getAuthStore: true)]
    public class ProductController : BaseController
    {
        [HttpGet, Route("List/{storeId?}")]
        public JsonResult List(DishStore store, int pageIndex = 1, int pageSize = 10, string searchName = null, int classId = 0)
        {
            var tupleResult = DishGoodBLL.SingleModel.GetListFromTable(store.aid, store.id, pageIndex, pageSize, searchName, classId);

            List<DishGood> product = tupleResult.Item1;
            int total = tupleResult.Item2;

            //string classIds = string.Join(",", product.Select(item => item.cate_id));
            //List<DishCategory> productClass = null;
            //if(!string.IsNullOrWhiteSpace(classIds))
            //{
            //    productClass = DishCategoryBLL.SingleModel.GetListById(classIds);
            //}

            object formatProduct = product.Select(item => new
            {
                Id = item.id,
                Name = item.g_name,
                Img = item.img,
                Class = item.cat_name,
                //Class = productClass.First(thisItem => thisItem.id == item.cate_id)?.title,
                Sort = item.is_order,
                Price = item.shop_price,
                DailySupply = item.day_kucun,
                Display = item.state == 1,
            });

            return ApiModel(isok: true, message: "获取成功", data: new { page = formatProduct, total });
        }

        [HttpGet]
        public JsonResult Detail(DishStore store, int? productId)
        {
            if(!productId.HasValue)
            {
                return ApiModel(message: "参数不能为空[productId]");
            }

            DishGood product = DishGoodBLL.SingleModel.GetModel(productId.Value);
            if(product == null || product.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            EditProduct editProduct = Common.SingleModel.ConvertToDTO(product);

            List<DishGoodAttr> productAttr = DishGoodAttrBLL.SingleModel.GetListByProduct(product.id);
            editProduct.Attrs = Common.SingleModel.ConvertToDTO(productAttr);

            return ApiModel(isok: true, message: "获取成功", data: editProduct);
        }

        [HttpPost, Route("Add/{storeId?}"), FormValidate]
        public JsonResult Add(DishStore store, [System.Web.Http.FromBody]EditProduct edit)
        {
            DishGood newProduct = Common.SingleModel.ConvertToDAO(edit);
            newProduct.storeId = store.id;
            newProduct.aId = store.aid;

            int newId;
            bool success = int.TryParse(DishGoodBLL.SingleModel.Add(newProduct)?.ToString(), out newId) && newId > 0;
            if (success && edit.Attrs?.Count > 0)
            {
                List<DishGoodAttr> newAttr = Common.SingleModel.ConvertToDAO(edit.Attrs);
                newAttr.ForEach(attr => attr.goods_id = newId);
                if (!DishGoodAttrBLL.SingleModel.UpdateAttr(newAttr: newAttr))
                {
                    Delete(store, newId);
                    return ApiModel(message: "写入商品属性失败");
                }
            }

            return ApiModel(isok: success, message: success ? "新增成功" : "新增失败");
        }

        [HttpPost, FormValidate]
        public JsonResult Edit(DishStore store, int? productId, [System.Web.Http.FromBody]EditProduct edit)
        {
            if(!productId.HasValue)
            {
                return ApiModel(message: "参数不能为空[productId]");
            }

            if(DishGoodBLL.SingleModel.GetModel(productId.Value)?.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            //保存编辑商品
            DishGood editProduct = Common.SingleModel.ConvertToDAO(edit);
            editProduct.id = productId.Value;
            bool success = DishGoodBLL.SingleModel.Update(editProduct, "img,g_name,g_description,g_danwei,g_renqi,g_print_tag,shop_price,market_price,dabao_price,day_kucun,is_order,state,cate_id,yue_xiaoliang,update_time");
            if(!success)
            {
                return ApiModel(message: "编辑失败", data: editProduct);
            }

            //当前商品属性
            List<DishGoodAttr> currentAttr = DishGoodAttrBLL.SingleModel.GetListByProduct(productId.Value);
            //更新商品属性
            List<DishGoodAttr> updateAttr = Common.SingleModel.ConvertToDAO(edit.Attrs?.FindAll(item => item.AttrId > 0 && currentAttr.Exists(thisItem => thisItem.id == item.Id)));
            //删除商品属性
            List<DishGoodAttr> deleteAttr = currentAttr.FindAll(curr => edit.Attrs?.Exists(editAttr => curr.id == editAttr.Id) == false);
            //新增商品属性
            List<DishGoodAttr> newAttr = Common.SingleModel.ConvertToDAO(edit.Attrs?.FindAll(item => item.Id == 0));
            newAttr.ForEach(item => item.goods_id = productId.Value);

            if (!DishGoodAttrBLL.SingleModel.UpdateAttr(newAttr, updateAttr, deleteAttr))
            {
                return ApiModel(message: "保存商品属性失败");
            }

            return ApiModel(isok: success, message: success ? "编辑成功" : "编辑失败");
        }

        [HttpPost]
        public JsonResult Delete(DishStore store, int? productId)
        {
            if(!productId.HasValue)
            {
                return ApiModel(message: $"参数不能为空[productId]");
            }

            DishGood product = DishGoodBLL.SingleModel.GetModel(productId.Value);
            if (product == null || product.storeId != store.id)
            {
                return ApiModel(message: $"非法操作[{product?.id}]");
            }

            bool success = DishGoodBLL.SingleModel.DeleteProduct(product);

            return ApiModel(isok: success, message: success ? "删除成功" : "删除失败");
        }
    }
}