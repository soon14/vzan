using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entity.MiniApp.Dish;
using BLL.MiniApp.Dish;
using BLL.MiniApp.cityminiapp;
using Utility.IO;
using User.MiniApp.Model;
using User.MiniApp.Areas.Dish.Filters;

namespace User.MiniApp.Areas.Dish.Controllers
{
    [LoginFilter]
    public class CategoryController :Controller
    {
        protected readonly DishReturnMsg _result;
        
        public CategoryController()
        {
            _result = new DishReturnMsg();
            
        }
        public ActionResult Index(int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 0)
        {
            //显示
            ViewModel<DishCategory> vm = new ViewModel<DishCategory>();
            vm.DataList = DishCategoryBLL.SingleModel.GetDishCategorys(DishEnums.CategoryEnums.店铺分类, aId, storeId, pageIndex, pageSize, true) ?? new List<DishCategory>();

            vm.DataModel = new DishCategory();
            vm.DataModel.aId = aId;
            vm.DataModel.storeId = storeId;
            return View(vm);
        }

        public ActionResult Edit(DishCategory model,string act = "",string updateCols = "")
        {
            //参数验证
            if (model.id < 0 || model.aId <= 0)
            {
                if (string.IsNullOrWhiteSpace(act))
                {
                    _result.code = 500;
                    _result.msg = "参数错误";
                    return View("PageError", _result);
                }
                else
                {
                    _result.code = 0;
                    _result.msg = "参数错误";
                    return Json(_result);
                }
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (model.id > 0)
                {
                    model = DishCategoryBLL.SingleModel.GetModel(model.id);
                    if (model == null)
                    {
                        _result.code = 500;
                        _result.msg = "分类不存在";
                        return View("PageError", _result);
                    }
                }
                EditModel<DishCategory> em = new EditModel<DishCategory>();
                em.DataModel = model;
                em.appId = model.aId;
                em.storeId = model.storeId;
                return View(em);
            }
            else
            {
                if (act == "save")
                {
                    //添加
                    if (model.id == 0)
                    {
                        int newid = Convert.ToInt32(DishCategoryBLL.SingleModel.Add(model));
                        _result.msg = newid > 0 ? "添加成功" : "添加失败";
                        _result.code = newid > 0 ? 1 : 0;

                    }
                    //修改
                    else
                    {
                        bool updateResult = false;
                        if (!string.IsNullOrWhiteSpace(updateCols))
                        {
                            updateResult = DishCategoryBLL.SingleModel.Update(model, updateCols);
                        }
                        else
                        {
                            updateResult = DishCategoryBLL.SingleModel.Update(model, "title,img,is_show,is_order,state,name_info");
                        }

                        _result.msg = updateResult ? "修改成功" : "修改失败";
                        _result.code = updateResult ? 1 : 0;
                    }
                }
                else if (act == "del")
                {
                    bool isSuccess = false;

                    model.state = -1;
                    isSuccess = DishCategoryBLL.SingleModel.Update(model, "state");

                    _result.msg = isSuccess ? "删除成功" : "删除失败";
                    _result.code = isSuccess ? 1 : 0;
                }
            }
            return Json(_result);
        }

    }
}