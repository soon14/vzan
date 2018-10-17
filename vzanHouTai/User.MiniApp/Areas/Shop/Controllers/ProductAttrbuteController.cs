using BLL.MiniApp.Dish;
using Core.MiniApp.DTO;
using Entity.MiniApp.Dish;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Areas.Shop.Filters;
using User.MiniApp.Areas.Shop.Models;

namespace User.MiniApp.Areas.Shop.Controllers
{
    [RouteArea("Shop"), RoutePrefix("ProductAttrbute")]
    [LoginFilter(storePara: "store", getAuthStore: true)]
    public class ProductAttrbuteController : BaseController
    {
        [HttpGet, Route("Type/{storeId?}")]
        public JsonResult Type(DishStore store, int pageIndex = 1, int pageSize = 10)
        {
            List<DishAttrType> attrType = DishAttrTypeBLL.SingleModel.GetListFromTable(pageIndex - 1, pageSize, store.aid, store.id);
            if (attrType.Exists(item => item.storeId != store.id))
            {
                return ApiModel(message: "非法操作");
            }

            int total = DishAttrTypeBLL.SingleModel.GetCountByStore(store.aid, store.id);
            object formatAttrType = attrType.Select(item => new
            {
                Id = item.id,
                Name = item.cat_name,
                Enable = item.enabled,
                Count = item.attrCount,
            });

            return ApiModel(isok: true, message: "获取成功", data: new { page = formatAttrType, total });
        }

        [HttpPost, Route("AddType/{storeId?}"), FormValidate]
        public JsonResult AddType(DishStore store, [System.Web.Http.FromBody]EditAttrType edit)
        {
            DishAttrType newAttrType = new DishAttrType
            {
                aid = store.aid,
                cat_name = edit.Name,
                enabled = edit.Enable.Value ? 1 : 0,
                storeId = store.id,
                state = 1,
            };

            int newId;
            bool success = int.TryParse(DishAttrTypeBLL.SingleModel.Add(newAttrType)?.ToString(), out newId) && newId > 0;
            return ApiModel(isok: success, message: success ? "新增成功" : "新增失败");
        }

        [HttpPost, Route("EditType/{attrTypeId?}"), FormValidate]
        public JsonResult EditType(DishStore store, int? attrTypeId, [System.Web.Http.FromBody]EditAttrType edit)
        {
            if (!attrTypeId.HasValue)
            {
                return ApiModel(message: "参数不能为空[attrTypeId]");
            }

            DishAttrType attrType = DishAttrTypeBLL.SingleModel.GetModel(attrTypeId.Value);
            if (attrType == null || attrType.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }


            attrType.enabled = edit.Enable.Value ? 1 : 0;
            attrType.cat_name = edit.Name;

            bool success = DishAttrTypeBLL.SingleModel.Update(attrType, "enabled,cat_name");

            return ApiModel(isok: success, message: success ? "编辑成功" : "编辑失败");
        }

        [HttpPost, Route("DeleteType/{attrTypeId?}")]
        public JsonResult DeleteType(DishStore store, int? attrTypeId)
        {
            if (!attrTypeId.HasValue)
            {
                return ApiModel(message: "参数不能为空[attrTypeId]");
            }

            DishAttrType attrType = DishAttrTypeBLL.SingleModel.GetModel(attrTypeId.Value);
            if (attrType == null || attrType.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            bool success = DishAttrTypeBLL.SingleModel.DeleteAttrType(attrType);
            return ApiModel(isok: success, message: success ? "删除成功" : "删除失败");
        }

        [HttpGet, Route("Group/{attrTypeId?}")]
        public JsonResult Group(DishStore store, int? attrTypeId, int pageIndex = 1, int pageSize = 10)
        {
            List<DishAttr> attrbute = DishAttrBLL.SingleModel.GetListFromTable(pageIndex - 1, pageSize, attrTypeId.Value);
            if (attrbute.Exists(item => item.storeId != store.id))
            {
                return ApiModel(message: "非法操作");
            }

            int total = DishAttrBLL.SingleModel.GetCountByType(attrTypeId.Value);
            object formatAttrbute = attrbute.Select(item =>
            new
            {
                Id = item.id,
                AttrTypeId = item.cat_id,
                Name = item.attr_name,
                Type = item.cat_name,
                Item = item.attr_values_arr,
                Sort = item.sort,
            });

            return ApiModel(isok: true, message: "获取成功", data: new { page = formatAttrbute, total });
        }

        [HttpPost, Route("Add/{attrTypeId?}"), FormValidate]
        public JsonResult Add(DishStore store, int? attrTypeId, [System.Web.Http.FromBody]EditAttbute edit)
        {
            if (!attrTypeId.HasValue)
            {
                return ApiModel(message: "参数不能为空[attrTypeId]");
            }

            DishAttrType attrType = DishAttrTypeBLL.SingleModel.GetModel(attrTypeId.Value);
            if (attrType == null || attrType.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            DishAttr newAttrbute = new DishAttr
            {
                attr_name = edit.Name,
                attr_values = string.Join("\n", edit.Option),
                aid = store.aid,
                cat_id = edit.AttrTypeId,
                storeId = store.id,
                sort = edit.Sort,
                state = 1,
            };

            int newId;
            bool success = int.TryParse(DishAttrBLL.SingleModel.Add(newAttrbute)?.ToString(), out newId) && newId > 0;
            return ApiModel(isok: success, message: success ? "新增成功" : "新增失败");
        }

        [HttpPost, Route("Edit/{attrbuteId?}"), FormValidate]
        public JsonResult Edit(DishStore store, int? attrbuteId, [System.Web.Http.FromBody]EditAttbute edit)
        {
            if (!attrbuteId.HasValue)
            {
                return ApiModel(message: "参数不能为空[attrbuteId]");
            }

            DishAttr attrbute = DishAttrBLL.SingleModel.GetModel(attrbuteId.Value);
            if (attrbute == null || attrbute.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            attrbute.attr_name = edit.Name;
            attrbute.cat_id = edit.AttrTypeId;
            attrbute.attr_values = string.Join("\n", edit.Option);
            attrbute.sort = edit.Sort;

            bool success = DishAttrBLL.SingleModel.Update(attrbute, "cat_id,attr_name,sort,attr_values");

            return ApiModel(isok: success, message: success ? "编辑成功" : "编辑失败");
        }

        [HttpPost, Route("Delete/{attrbuteId?}")]
        public JsonResult Delete(DishStore store, int? attrbuteId)
        {
            if(!attrbuteId.HasValue)
            {
                return ApiModel(message: "参数不能为空[attrbuteId]");
            }

            DishAttr attrbute = DishAttrBLL.SingleModel.GetModel(attrbuteId.Value);
            if (attrbute == null || attrbute.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            bool success = DishAttrBLL.SingleModel.DeleteAttr(attrbute);
            return ApiModel(isok: success, message: success ? "删除成功" : "删除失败");
        }
    }
}