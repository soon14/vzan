using BLL.MiniApp.Dish;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Areas.DishAdmin.Filters;
using User.MiniApp.Model;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{

    [LoginFilter]
    public class DishController : Controller
    {
        private readonly DishReturnMsg _result;




        public DishController()
        {
            _result = new DishReturnMsg();


        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Goods(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string kw = "", int cate_id = 0, string sortData = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishGood> vm = new ViewModel<DishGood>();
                var tupleResult = DishGoodBLL.SingleModel.GetListFromTable(aId, storeId, pageIndex, pageSize, kw, cate_id);
                vm.DataList = tupleResult.Item1;
                vm.TotalCount = tupleResult.Item2;
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.aId = aId;
                vm.storeId = storeId;
                ViewBag.dishCategoryList = DishCategoryBLL.SingleModel.GetList($"aid={aId} and storeid={storeId} and state=1");
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
                        DishGood updateModel = DishGoodBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishGoodBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                                _result.msg = "删除失败";
                        }
                        else
                            _result.msg = "删除失败,菜品不存在";
                    }

                }
                else if (act == "sort")
                {
                    bool updateResult = DishGoodBLL.SingleModel.UpdateSortBatch(sortData);
                    _result.code = updateResult ? 1 : 0;
                    _result.msg = updateResult ? "排序成功" : "排序失败";
                }
            }
            return Json(_result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GoodEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, DishGood model = null)
        {
            //参数验证
            if (id < 0 || aId <= 0 || storeId <= 0)
            {
                _result.msg = "参数错误";
                return Json(_result, JsonRequestBehavior.AllowGet);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (id == 0)
                    model = new DishGood();
                else
                {
                    model = DishGoodBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("菜品不存在");
                }
                EditModel<DishGood> em = new EditModel<DishGood>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                ViewBag.dishCategoryList = DishCategoryBLL.SingleModel.GetList($"aid={aId} and storeid={storeId} and state=1");
                ViewBag.dishAttrTypeList = DishAttrTypeBLL.SingleModel.GetList($"aid={aId} and storeid={storeId} and state=1 and enabled=1");
                ViewBag.dishPrintTagList = DishTagBLL.SingleModel.GetTagByParams(aId, storeId, 1);
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    //去除重复属性，如果有重复属性只取第一个
                    model.attr = model.attr?.GroupBy(p => new { p.goods_id, p.attr_id, p.value }).Select(groups => groups.First()).OrderBy(p => p.attr_id).ToList();
                    if (id == 0)
                    {
                        //添加产品
                        int newid = Convert.ToInt32(DishGoodBLL.SingleModel.Add(model));
                        _result.msg = newid > 0 ? "添加成功" : "添加失败";
                        _result.code = newid > 0 ? 1 : 0;

                        //添加产品属性
                        model.attr?.ForEach(item =>
                        {
                            if (!string.IsNullOrEmpty(item.value))
                            {
                                item.goods_id = newid;
                                DishGoodAttrBLL.SingleModel.Add(item);
                            }
                        });
                    }
                    else
                    {
                        bool updateResult = DishGoodBLL.SingleModel.Update(model);
                        _result.msg = updateResult ? "修改成功" : "修改失败";
                        _result.code = updateResult ? 1 : 0;
                        EditDishGoodAttr(model);
                    }
                }
                return Json(_result, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 修改商品属性
        /// </summary>
        /// <param name="model"></param>
        private void EditDishGoodAttr(DishGood model)
        {

            List<DishGoodAttr> goodAttrList = DishGoodAttrBLL.SingleModel.GetList($"goods_id={model.id} and attr_type_id={model.goods_type}");
            if (model.attr != null)
            {
                List<DishGoodAttr> tempGoodAttrList = model.attr.Where(w => w.id > 0).ToList();
                //取差集，删除差集
                List<DishGoodAttr> diffGoodAttr = goodAttrList?.Except(model.attr, new DishGoodAttrComparer2()).ToList();
                diffGoodAttr?.ForEach(p =>
                {
                    if (p.id > 0)
                        DishGoodAttrBLL.SingleModel.Delete(p.id);
                });
                tempGoodAttrList?.ForEach(item =>
                {
                    DishGoodAttrBLL.SingleModel.Update(item, "price,value");
                });

                tempGoodAttrList = model.attr.Where(w => w.id == 0).ToList();
                tempGoodAttrList?.ForEach(item =>
                {
                    item.id = Convert.ToInt32(DishGoodAttrBLL.SingleModel.Add(item));
                });

            }

        }

        /// <summary>
        /// 获取商品属性
        /// </summary>
        /// <param name="goods_type">分类ID</param>
        /// <param name="goods_id">产品ID</param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ActionResult GetAttrList(int goods_type, int goods_id, int aId = 0, int storeId = 0)
        {
            ViewBag.goods_id = goods_id;
            ViewBag.goods_type = goods_type;
            List<DishAttr> disAttrList = DishAttrBLL.SingleModel.GetList($"aid={aId} and storeid={storeId} and state=1 and cat_id={goods_type}");
            List<DishGoodAttr> goodAttrList = new List<DishGoodAttr>();
            if (goods_id > 0 && goods_type != 0)
            {
                goodAttrList = DishGoodAttrBLL.SingleModel.GetList($"goods_id={goods_id} and attr_type_id ={goods_type}");
                //如果没有属性，生成默认属性
                if (goodAttrList == null || goodAttrList.Count <= 0)
                {
                    foreach (var item in disAttrList)
                    {
                        goodAttrList.Add(new DishGoodAttr { id = 0, goods_id = goods_id, attr_id = item.id, price = 0, value = "" });
                    }
                }

            }
            ViewBag.goodAttrList = goodAttrList;
            ViewBag.dishAttrList = disAttrList;
            return View();
        }
    }
}