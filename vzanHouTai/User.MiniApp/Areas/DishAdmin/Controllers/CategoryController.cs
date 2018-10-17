using BLL.MiniApp.Dish;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.DishAdmin.Filters;
using User.MiniApp.Model;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{
    [LoginFilter]
    public class CategoryController : Controller
    {
        //private readonly IDishCategoryRepository _iDishCategory;
        //public CategoryController()
        //{

        //}
        //public CategoryController(IDishCategoryRepository iDishCategory)
        //{
        //    _iDishCategory = iDishCategory;
        //}
        protected readonly DishReturnMsg _result;
        
        public CategoryController()
        {
            _result = new DishReturnMsg();
            
        }
        public ActionResult Index(string act="",int id=0, int aId = 0, int storeId = 0, int pageIndex = 1, int pageSize = 20,string sortData="")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishCategory> vm = new ViewModel<DishCategory>();
                string filterSql = $"type = 2 and aid = {aId} and storeId = {storeId} and state<>-1";
                vm.DataList = DishCategoryBLL.SingleModel.GetList(filterSql, pageSize, pageIndex, "*", "is_order desc");
                vm.TotalCount = DishCategoryBLL.SingleModel.GetCount(filterSql);
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.aId = aId;
                vm.storeId = storeId;
                return View(vm);
            }
            else
            {
                //删除
                if (act == "del")
                {
                    if (id <= 0)
                    {
                        _result.code = 0;
                        _result.msg = "参数错误";
                    }
                    else
                    {
                        DishCategory updateModel = DishCategoryBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishCategoryBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                            {
                                _result.code = 0;
                                _result.msg = "删除失败";
                            }
                        }
                        else
                        {
                            _result.code = 0;
                            _result.msg = "删除失败,分类不存在";
                        }
                    }

                }
                else if (act == "sort")
                {
                    bool updateResult = DishCategoryBLL.SingleModel.UpdateSortBatch(sortData);
                    _result.code = updateResult ? 1 : 0;
                    _result.msg = updateResult ? "修改成功" : "修改失败";
                }
            }
            return Json(_result);
        }

        public ActionResult Edit(string act = "", int id = 0, int aId = 0, int storeId = 0, DishCategory model = null)
        {
            //参数验证
            if (id < 0 || aId <= 0 || storeId <= 0)
            {
                _result.msg = "参数错误";
                return Json(_result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (id == 0)
                {
                    model = new DishCategory();
                }
                else
                {
                    model = DishCategoryBLL.SingleModel.GetModel(id);
                    if (model == null)
                    {
                        return Content("分类不存在");
                    }
                }
                EditModel<DishCategory> em = new EditModel<DishCategory>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    //添加
                    if (id == 0)
                    {
                        int newid = Convert.ToInt32(DishCategoryBLL.SingleModel.Add(model));
                        _result.msg = newid > 0 ? "添加成功" : "添加失败";
                        _result.code = newid > 0 ? 1 : 0;
                        
                    }
                    //修改
                    else
                    {
                        bool updateResult = DishCategoryBLL.SingleModel.Update(model);
                        _result.msg = updateResult ? "修改成功" : "修改失败";
                        _result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(_result);
        }
    }
}