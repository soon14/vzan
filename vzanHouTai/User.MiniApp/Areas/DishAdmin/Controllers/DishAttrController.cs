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
    public class DishAttrController : Controller
    {
        private readonly DishReturnMsg _result;
        
        
        public DishAttrController()
        {
            _result = new DishReturnMsg();
            
            
        }
        /// <summary>
        /// 属性类型管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishAttrType> vm = new ViewModel<DishAttrType>();
                vm.DataList = DishAttrTypeBLL.SingleModel.GetListFromTable(pageIndex, pageSize,aId, storeId);
                vm.TotalCount = DishAttrTypeBLL.SingleModel.GetCount($"state<>-1 and aid={aId} and storeid={storeId}");
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
                        _result.msg = "参数错误";
                    else
                    {
                        DishAttrType updateModel = DishAttrTypeBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishAttrTypeBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                                _result.msg = "删除失败";
                        }
                        else
                            _result.msg = "删除失败,分类不存在";
                    }

                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 编辑属性类型
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(string act = "", int id = 0, int aId = 0, int storeId = 0, DishAttrType model = null)
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
                    model = new DishAttrType();
                else
                {
                    model = DishAttrTypeBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("分类不存在");
                }
                EditModel<DishAttrType> em = new EditModel<DishAttrType>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    if (id == 0)
                    {
                        int newid = Convert.ToInt32(DishAttrTypeBLL.SingleModel.Add(model));
                        _result.msg = newid > 0 ? "添加成功" : "添加失败";
                        _result.code = newid > 0 ? 1 : 0;
                    }
                    else
                    {
                        bool updateResult = DishAttrTypeBLL.SingleModel.Update(model);
                        _result.msg = updateResult ? "修改成功" : "修改失败";
                        _result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 属性值管理
        /// </summary>
        /// <returns></returns>

        public ActionResult AttrValue(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "", int fid = 0)
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishAttr> vm = new ViewModel<DishAttr>();
                vm.DataList = DishAttrBLL.SingleModel.GetListFromTable(pageIndex, pageSize, fid);
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.aId = aId;
                vm.storeId = storeId;
                //类型ID
                ViewBag.fid = fid;
                return View(vm);
            }
            else
            {
                //删除
                if (act == "del")
                {
                    if (id <= 0)
                        _result.msg = "参数错误";
                    else
                    {
                        DishAttr updateModel = DishAttrBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishAttrBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                                _result.msg = "删除失败";
                        }
                        else
                            _result.msg = "删除失败,分类不存在";
                    }

                }
            }
            return Json(_result);
        }
        /// <summary>
        /// 编辑属性值
        /// </summary>
        /// <returns></returns>
        public ActionResult AttrValueEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, DishAttr model = null, int fid = 0)
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
                    model = new DishAttr();
                else
                {
                    model = DishAttrBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("分类不存在");
                }
                EditModel<DishAttr> em = new EditModel<DishAttr>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                ViewBag.dishAttrTypeList = DishAttrTypeBLL.SingleModel.GetList($"state=1 and aid={aId} and storeId={storeId}");
                ViewBag.fid = fid;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    //去除首位多余的换行，保证两个值之间只有一个换行
                    if (model != null && !string.IsNullOrEmpty(model.attr_values))
                        model.attr_values = string.Join("\n", model.attr_values.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Distinct());

                    if (id == 0)
                    {
                        int newid = Convert.ToInt32(DishAttrBLL.SingleModel.Add(model));
                        _result.msg = newid > 0 ? "添加成功" : "添加失败";
                        _result.code = newid > 0 ? 1 : 0;
                    }
                    else
                    {

                        bool updateResult = DishAttrBLL.SingleModel.Update(model);
                        _result.msg = updateResult ? "修改成功" : "修改失败";
                        _result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(_result);
        }
    }
}