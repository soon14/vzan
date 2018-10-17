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
    [RouteArea("Shop"), RoutePrefix("Category")]
    [LoginFilter(storePara: "store", getAuthStore: true)]
    public class CategoryController : BaseController
    {
        [HttpGet, Route("List/{storeId?}")]
        public JsonResult List(DishStore store, int pageIndex = 1, int pageSize = 10)
        {
            List<DishCategory> category = DishCategoryBLL.SingleModel.GetListByStore(storeId: store.id, type: DishEnums.CategoryEnums.菜品分类, pageIdex: pageIndex, pageSize: pageSize);
            int total = DishCategoryBLL.SingleModel.GetCountByStore(storeId: store.id, type: DishEnums.CategoryEnums.菜品分类);

            object formatCategory = category.Select(item => new
            {
                Id = item.id,
                Name = item.title,
                Description = item.name_info,
                Icon = item.img,
                Display = item.is_show,
                Sort = item.is_order,
            });

            return ApiModel(isok: true, message: "获取成功", data: new { page = formatCategory, total });
        }

        [HttpPost, Route("Add/{storeId?}"), FormValidate]
        public JsonResult Add(DishStore store, [System.Web.Http.FromBody]EditCategory edit)
        {
            DishCategory newCategory = new DishCategory
            {
                title = edit.Name,
                img = edit.Icon,
                name_info = edit.Description,
                type = (int)DishEnums.CategoryEnums.菜品分类,
                is_show = edit.Display ? 1 : 0,
                is_order = edit.Sort,
                aId = store.aid,
                storeId = store.id,
                state = 1,
                addTime = DateTime.Now,
                updateTime = DateTime.Now,
            };

            int newId;
            bool success = int.TryParse(DishCategoryBLL.SingleModel.Add(newCategory)?.ToString(), out newId) && newId > 0;
            return ApiModel(isok: success, message: success ? "新增成功" : "新增失败");
        }

        [HttpPost, Route("Edit/{categoryId?}"), FormValidate]
        public JsonResult Edit(DishStore store, int? categoryId, [System.Web.Http.FromBody]EditCategory edit)
        {
            if (!categoryId.HasValue)
            {
                return ApiModel(message: "参数不能为空[categoryId]");
            }

            DishCategory category = DishCategoryBLL.SingleModel.GetModel(categoryId.Value);
            if (category == null || category.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            category.title = edit.Name;
            category.img = edit.Icon;
            category.name_info = edit.Description;
            category.is_show = edit.Display ? 1 : 0;
            category.is_order = edit.Sort;
            category.updateTime = DateTime.Now;

            bool success = DishCategoryBLL.SingleModel.Update(category, "title,img,name_info,is_show,is_order,updateTime");
            return ApiModel(isok: success, message: success ? "编辑成功" : "编辑失败");
        }

        [HttpPost, Route("Delete/{categoryId?}")]
        public JsonResult Delete(DishStore store, int? categoryId)
        {
            if(!categoryId.HasValue)
            {
                return ApiModel(message: "参数不能为空[categoryId]");
            }

            DishCategory category = DishCategoryBLL.SingleModel.GetModel(categoryId.Value);
            if (category == null || category.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            bool success = DishCategoryBLL.SingleModel.DeleteCategory(category);
            return ApiModel(isok: success, message: success ? "删除成功" : "删除失败");
        }
    }
}