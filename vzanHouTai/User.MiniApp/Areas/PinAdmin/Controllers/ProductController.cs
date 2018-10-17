using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entity.MiniApp.Pin;
using BLL.MiniApp.Pin;
using User.MiniApp.Model;
using Core.MiniApp;
using User.MiniApp.Areas.PinAdmin.Filters;
using static Entity.MiniApp.Pin.PinEnums;

namespace User.MiniApp.Areas.PinAdmin.Controllers
{
    /// <summary>
    /// 产品
    /// 标签
    /// 规格
    /// 单位
    /// </summary>
    [LoginFilter]
    public class ProductController : BaseController
    {
        #region 产品
        public ActionResult Good(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string kw = "", int cateIdOne = 0, int cateId = 0, string sortData = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<PinGoods> vm = new ViewModel<PinGoods>();
                var tupleResult = PinGoodsBLL.SingleModel.GetListFromTable(aId, storeId, pageIndex, pageSize, kw, cateIdOne, cateId);
                vm.DataList = tupleResult.Item1;
                vm.TotalCount = tupleResult.Item2;
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.aId = aId;
                vm.storeId = storeId;
                ViewBag.categoryList = PinCategoryBLL.SingleModel.GetList($"aid={aId} and storeid=0 and state=1");
                return View(vm);
            }
            else
            {
                //删除
                if (act == "del")
                {
                    if (id <= 0)
                        result.msg = "参数错误";
                    else
                    {
                        PinGoods updateModel = PinGoodsBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = PinGoodsBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                result.code = 1;
                                result.msg = "删除成功";
                            }
                            else
                                result.msg = "删除失败";
                        }
                        else
                            result.msg = "删除失败,菜品不存在";
                    }

                }
                else if (act == "sort")
                {
                    bool updateResult = PinGoodsBLL.SingleModel.UpdateSortBatch(sortData);
                    result.code = updateResult ? 1 : 0;
                    result.msg = updateResult ? "排序成功" : "排序失败";
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GoodEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, PinGoods model = null)
        {
            //参数验证
            if (id < 0 || aId <= 0 || storeId <= 0)
            {
                result.msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (id == 0)
                    model = new PinGoods();
                else
                {
                    model = PinGoodsBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("菜品不存在");
                }
                EditModel<PinGoods> em = new EditModel<PinGoods>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                ViewBag.categoryList = PinCategoryBLL.SingleModel.GetList($"aid={aId} and storeid=0 and state=1");
                ViewBag.attrTypeList = PinAttrBLL.SingleModel.GetList($"aid={aId} and storeid={storeId} and state=1");
                ViewBag.labelList = PinGoodsLabelBLL.SingleModel.GetList($"aid={aId} and storeid={storeId} and state=1");
                return View(em);
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    result.code = 0;
                    result.msg = this.ErrorMsg();
                    return Json(result);
                }
                if (act == "edit")
                {
                    if (model.groupPrice > 0)
                    {
                        if (model.groupPrice < 1 * 100 || model.groupPrice > 20000 * 100 || model.groupPrice > model.price)
                        {
                            result.code = 0;
                            result.msg = "返现金额不能小于1元，不能大于2万元，且不能高于产品价格";
                            return Json(result);
                        }
                    }

                    if (id == 0)
                    {


                        //添加产品
                        int newid = Convert.ToInt32(PinGoodsBLL.SingleModel.Add(model));
                        result.msg = newid > 0 ? "添加成功" : "添加失败";
                        result.code = newid > 0 ? 1 : 0;


                    }
                    else
                    {
                        if (model.state == 0)
                            model.auditState = (int)GoodsAuditState.待审核;

                        bool updateResult = PinGoodsBLL.SingleModel.Update(model);
                        result.msg = updateResult ? "修改成功" : "修改失败";
                        result.code = updateResult ? 1 : 0;
                    }
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        #region 标签
        public ActionResult Label(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "", int fid = 0)
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                string filterSql = $"state=1 and aid={aId} and storeid={storeId}";
                ViewModel<PinGoodsLabel> vm = new ViewModel<PinGoodsLabel>();
                vm.DataList = PinGoodsLabelBLL.SingleModel.GetList(filterSql, pageSize, pageIndex, "*", "sort desc");
                vm.TotalCount = PinGoodsLabelBLL.SingleModel.GetCount(filterSql);
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
                        result.msg = "参数错误";
                    else
                    {
                        PinGoodsLabel updateModel = PinGoodsLabelBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = PinGoodsLabelBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                result.code = 1;
                                result.msg = "删除成功";
                            }
                            else
                                result.msg = "删除失败";
                        }
                        else
                            result.msg = "删除失败,对象不存在或已删除";
                    }

                }
                else if (act == "sort")
                {
                    bool updateResult = PinGoodsLabelBLL.SingleModel.UpdateSortBatch(sortData);
                    result.code = updateResult ? 1 : 0;
                    result.msg = updateResult ? "排序成功" : "排序失败";
                }
            }
            return Json(result);
        }

        public ActionResult LabelEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, PinGoodsLabel model = null, int fid = 0)
        {
            //参数验证
            if (id < 0 || aId <= 0 || storeId <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (id == 0)
                    model = new PinGoodsLabel();
                else
                {
                    model = PinGoodsLabelBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("对象不存在");
                }
                EditModel<PinGoodsLabel> em = new EditModel<PinGoodsLabel>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    if (!ModelState.IsValid)
                    {
                        result.code = 0;
                        result.msg = this.ErrorMsg();
                        return Json(result);
                    }
                    if (id == 0)
                    {
                        if (PinGoodsLabelBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                        {
                            result.code = 0;
                            result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                            return Json(result);
                        }
                        int newid = Convert.ToInt32(PinGoodsLabelBLL.SingleModel.Add(model));
                        result.msg = newid > 0 ? "添加成功" : "添加失败";
                        result.code = newid > 0 ? 1 : 0;
                    }
                    else
                    {
                        if (PinGoodsLabelBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name and id<>{id}", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                        {
                            result.code = 0;
                            result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                            return Json(result);
                        }
                        bool updateResult = PinGoodsLabelBLL.SingleModel.Update(model, "name,sort");
                        result.msg = updateResult ? "修改成功" : "修改失败";
                        result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(result);
        }
        #endregion

        #region 规格
        public ActionResult Attr(string act = "", int id = 0, int aId = 0, int storeId = 0, int fid = 0, int pageIndex = 0, int pageSize = 20, string sortData = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                string filterSql = $"state=1 and aid={aId} and storeid={storeId} and fid={fid}";
                ViewModel<PinAttr> vm = new ViewModel<PinAttr>();
                vm.DataList = PinAttrBLL.SingleModel.GetList(filterSql, pageSize, pageIndex, "*", "sort desc");
                vm.TotalCount = PinAttrBLL.SingleModel.GetCount(filterSql);
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
                        result.msg = "参数错误";
                    else
                    {
                        PinAttr updateModel = PinAttrBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = PinAttrBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                result.code = 1;
                                result.msg = "删除成功";
                            }
                            else
                                result.msg = "删除失败";
                        }
                        else
                            result.msg = "删除失败,对象不存在";
                    }

                }
                else if (act == "sort")
                {
                    bool updateResult = PinAttrBLL.SingleModel.UpdateSortBatch(sortData);
                    result.code = updateResult ? 1 : 0;
                    result.msg = updateResult ? "排序成功" : "排序失败";
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 编辑规格和规格值
        /// </summary>
        /// <returns></returns>
        public ActionResult AttrEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, PinAttr model = null)
        {
            //参数验证
            if (id < 0 || aId <= 0 || storeId <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (id == 0)
                    model = new PinAttr();
                else
                {
                    model = PinAttrBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("对象不存在");
                }
                EditModel<PinAttr> em = new EditModel<PinAttr>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                return View(em);
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    result.code = 0;
                    result.msg = this.ErrorMsg();
                    return Json(result);
                }
                if (act == "edit")
                {
                    if (id == 0)
                    {
                        if (PinAttrBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and fid={model.fId} and BINARY name=@name", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                        {
                            result.code = 0;
                            result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                            return Json(result);
                        }
                        int newid = Convert.ToInt32(PinAttrBLL.SingleModel.Add(model));
                        result.msg = newid > 0 ? "添加成功" : "添加失败";
                        result.code = newid > 0 ? 1 : 0;
                    }
                    else
                    {
                        if (PinAttrBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and fid={model.fId} and BINARY name=@name and id<>{id}", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                        {
                            result.code = 0;
                            result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                            return Json(result);
                        }
                        bool updateResult = PinAttrBLL.SingleModel.Update(model, "name");
                        result.msg = updateResult ? "修改成功" : "修改失败";
                        result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(result);
        }


        #endregion

        #region 单位
        public ActionResult Unit(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "", int fid = 0)
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                string filterSql = $"state=1 and aid={aId} and storeid={storeId}";
                ViewModel<PinGoodsUnit> vm = new ViewModel<PinGoodsUnit>();
                vm.DataList = PinGoodsUnitBLL.SingleModel.GetList(filterSql, pageSize, pageIndex, "*", "sort desc");
                vm.TotalCount = PinGoodsUnitBLL.SingleModel.GetCount(filterSql);
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
                        result.msg = "参数错误";
                    else
                    {
                        PinGoodsUnit updateModel = PinGoodsUnitBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = PinGoodsUnitBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                result.code = 1;
                                result.msg = "删除成功";
                            }
                            else
                                result.msg = "删除失败";
                        }
                        else
                            result.msg = "删除失败,对象不存在或已删除";
                    }

                }
                else if (act == "sort")
                {
                    bool updateResult = PinGoodsUnitBLL.SingleModel.UpdateSortBatch(sortData);
                    result.code = updateResult ? 1 : 0;
                    result.msg = updateResult ? "排序成功" : "排序失败";
                }
            }
            return Json(result);
        }

        public ActionResult UnitEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, PinGoodsUnit model = null, int fid = 0)
        {
            //参数验证
            if (id < 0 || aId <= 0 || storeId <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (id == 0)
                    model = new PinGoodsUnit();
                else
                {
                    model = PinGoodsUnitBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("对象不存在");
                }
                EditModel<PinGoodsUnit> em = new EditModel<PinGoodsUnit>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    if (!ModelState.IsValid)
                    {
                        result.code = 0;
                        result.msg = this.ErrorMsg();
                        return Json(result);
                    }
                    if (id == 0)
                    {
                        if (PinGoodsUnitBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                        {
                            result.code = 0;
                            result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                            return Json(result);
                        }
                        int newid = Convert.ToInt32(PinGoodsUnitBLL.SingleModel.Add(model));
                        result.msg = newid > 0 ? "添加成功" : "添加失败";
                        result.code = newid > 0 ? 1 : 0;
                    }
                    else
                    {
                        if (PinGoodsUnitBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name and id<>{id}", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                        {
                            result.code = 0;
                            result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                            return Json(result);
                        }
                        bool updateResult = PinGoodsUnitBLL.SingleModel.Update(model, "name,sort");
                        result.msg = updateResult ? "修改成功" : "修改失败";
                        result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(result);
        }


        #endregion


    }
}