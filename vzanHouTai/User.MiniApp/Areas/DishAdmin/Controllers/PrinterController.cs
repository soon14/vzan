using BLL.MiniApp.Dish;
using BLL.MiniApp.Fds;
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
    public class PrinterController : Controller
    {
        private readonly DishReturnMsg _result;
        
        
        


        public PrinterController()
        {
            _result = new DishReturnMsg();
            
            
            
        }

        public ActionResult Index(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20)
        {

            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishPrint> vm = new ViewModel<DishPrint>();
                vm.DataList = DishPrintBLL.SingleModel.GetListBySql($"select * from dishprint where state >-1 and aid={aId} and storeid={storeId}");
                if (vm.DataList != null && vm.DataList.Count > 0)
                {
                    foreach (var item in vm.DataList)
                    {
                        if (!string.IsNullOrEmpty(item.print_tags))
                        {
                            List<DishTag> tagList = DishTagBLL.SingleModel.GetList($" id in ({item.print_tags})");
                            item.tags = string.Join(",", tagList.Select(tag => tag.name));
                        }
                    }

                }
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.aId = aId;
                vm.storeId = storeId;
                return View(vm);
            }
            else
            {
                if (id <= 0)
                    _result.msg = "参数错误";
                else
                {
                    DishPrint updateModel = DishPrintBLL.SingleModel.GetModel(id);
                    if (updateModel != null)
                    {
                        bool updateResult = false;
                        switch (act)
                        {
                            case "del": //删除
                                switch(updateModel.print_name_type)
                                {
                                    case 0:
                                        //先访问易连云接口删除,成功后才在系统内操作记录
                                        string returnMsg = FoodYiLianYunPrintHelper.deletePrinter(updateModel.apiPrivateKey, updateModel.platform_userId.ToString(), updateModel.print_bianma, updateModel.print_shibiema);
                                        break;
                                    default:
                                        break;
                                }

                                //if (isDeletePrintByDiSanFang_Success)
                                //{
                                //无论打印机解绑失败与否,都让用户删除打印机。避免用户在第三方平台上删除后，此打印机删除不了的情况
                                    updateModel.state = -1;
                                    updateResult = DishPrintBLL.SingleModel.Update(updateModel);
                                    if (updateResult)
                                    {
                                        _result.code = 1;
                                        _result.msg = "删除成功";
                                    }
                                    else
                                        _result.msg = "删除失败";
                                //}
                                //else
                                //{
                                //    _result.msg = "解绑第三方平台打印机绑定失败,删除失败";
                                //}
                                break;
                            case "ban":
                                updateModel.state = 0;
                                updateResult = DishPrintBLL.SingleModel.Update(updateModel);
                                if (updateResult)
                                {
                                    _result.code = 1;
                                    _result.msg = "操作成功";
                                }
                                else
                                    _result.msg = "操作失败";
                                break;
                            case "start":
                                updateModel.state = 1;
                                updateResult = DishPrintBLL.SingleModel.Update(updateModel);
                                if (updateResult)
                                {
                                    _result.code = 1;
                                    _result.msg = "操作成功";
                                }
                                else
                                    _result.msg = "操作失败";
                                break;
                        }
                    }
                    else
                        _result.msg = "删除失败,数据不存在或已删除";
                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(string act = "", int id = 0, int aId = 0, int storeId = 0, DishPrint model = null, int fid = 0, List<string> printtags = null)
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
                    model = new DishPrint();
                else
                {
                    model = DishPrintBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("打印机不存在");
                }
                EditModel<DishPrint> em = new EditModel<DishPrint>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                ViewBag.dishPrintTagList = DishTagBLL.SingleModel.GetList($"state=1 and aid={aId} and storeId={storeId}");
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    model.print_tags = printtags != null?string.Join(",", printtags):"";
                    if (id == 0)
                    {
                        //先访问易连云接口添加,成功后才在系统内添加记录
                        PrintErrorData returnMsg = FoodYiLianYunPrintHelper.addPrinter(model.apiPrivateKey, model.platform_userId.ToString(), model.print_bianma, model.print_shibiema,"", model.platform_userName,model.print_name);
                        if (returnMsg.errno != 1)//returnMsg.errno>2 建议这里大于2
                        {
                            _result.msg = returnMsg.error;
                            return Json(_result);
                        }
                        int newid = Convert.ToInt32(DishPrintBLL.SingleModel.Add(model));
                        _result.msg = newid > 0 ? "添加成功" : "添加失败";
                        _result.code = newid > 0 ? 1 : 0;
                    }
                    else
                    {
                        bool updateResult = DishPrintBLL.SingleModel.Update(model, "print_name,print_type,print_d_type,print_dnum,print_ziti_type,print_goods_ziti_type,print_top_copy,print_bottom_copy,print_tags,state");
                        _result.msg = updateResult ? "修改成功" : "修改失败";
                        _result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 打印标签
        /// </summary>
        /// <param name="act"></param>
        /// <param name="id"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortData"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public ActionResult Label(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "", int fid = 0)
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                string filterSql = $"type=1 and state=1 and aid={aId} and storeid={storeId}";
                ViewModel<DishTag> vm = new ViewModel<DishTag>();
                vm.DataList = DishTagBLL.SingleModel.GetList(filterSql,pageSize,pageIndex,"*", "sort desc");
                vm.TotalCount = DishTagBLL.SingleModel.GetCount(filterSql);
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
                        DishTag updateModel = DishTagBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishTagBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                //同步设定了该标签的数据
                                DishGoodBLL.SingleModel.SyncGoodByPrintTag(updateModel.id);
                                DishPrintBLL.SingleModel.SyncPrintByPrintTag(updateModel.aId, updateModel.storeId, updateModel.id);

                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                                _result.msg = "删除失败";
                        }
                        else
                            _result.msg = "删除失败,标签不存在或已删除";
                    }

                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 编辑标签
        /// </summary>
        /// <returns></returns>
        public ActionResult LabelEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, DishTag model = null, int fid = 0)
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
                    model = new DishTag();
                else
                {
                    model = DishTagBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("分类不存在");
                }
                EditModel<DishTag> em = new EditModel<DishTag>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                ViewBag.dishAttrTypeList = DishTagBLL.SingleModel.GetList($"state=1 and aid={aId} and storeId={storeId}");
                ViewBag.fid = fid;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    if (id == 0)
                    {
                        model.type = 1;
                        int newid = Convert.ToInt32(DishTagBLL.SingleModel.Add(model));
                        _result.msg = newid > 0 ? "添加成功" : "添加失败";
                        _result.code = newid > 0 ? 1 : 0;
                    }
                    else
                    {
                        bool updateResult = DishTagBLL.SingleModel.Update(model, "name,sort");
                        _result.msg = updateResult ? "修改成功" : "修改失败";
                        _result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(_result);
        }
    }
}