using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using User.MiniApp.Areas.Dish.Models;
using System.Web.Mvc;
using Utility.IO;
using BLL.MiniApp.cityminiapp;
using Entity.MiniApp;
using BLL.MiniApp.Dish;
using Entity.MiniApp.Dish;
using User.MiniApp.Model;
using Utility;
using User.MiniApp.Areas.Dish.Filters;
using BLL.MiniApp;

namespace User.MiniApp.Areas.Dish.Controllers.Admin
{
    [LoginFilter]
    public class ConfigController : User.MiniApp.Controllers.baseController
    {
        //默认返回值
        protected readonly DishReturnMsg _result;
        
        

        public ConfigController()
        {
            //default params
            _result = new DishReturnMsg();
            _result.msg = "执行失败";
            
            
        }

        public ActionResult Index(string act = "", int pageIndex = 0, int pageSize = 100, int aId = 0, string kw = "", int id = 0)
        {
            XcxAppAccountRelation xcx = (XcxAppAccountRelation)Request.RequestContext.RouteData.Values["xcx"];
            if (xcx.SCount <= 1)
                xcx.SCount = 1;
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishStore> vm = new ViewModel<DishStore>();
                vm.DataList = DishStoreBLL.SingleModel.GetListFromTable(pageIndex, xcx.SCount, kw, aId);
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.aId = aId;
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
                        DishStore updateModel = DishStoreBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishStoreBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                                _result.msg = "删除失败";
                        }
                        else
                            _result.msg = "删除失败,数据不存在";
                    }

                }
                else if (act == "changemode")
                {
                    int ismain = Context.GetRequestInt("ismain", -1);
                    if (ismain > -1)
                    {
                        ismain = ismain == 1 ? 0 : 1;
                        bool rsult = DishStoreBLL.SingleModel.SetMainStore(ismain, aId, id);
                        _result.code = rsult ? 1 : 0;
                        _result.msg = rsult ? "设置成功" : "设置失败";
                        return Json(_result);
                    }
                }
            }
            return Json(_result);
        }

        public ActionResult Edit(string act = "", int id = 0, int aId = 0, int storeId = 0, DishStore model = null, string pwd = "")
        {
            XcxAppAccountRelation xcx = (XcxAppAccountRelation)Request.RequestContext.RouteData.Values["xcx"];
            //参数验证
            if (aId <= 0)
            {
                _result.msg = "参数错误";
                return Json(_result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (id == 0)
                    model = new DishStore();
                else
                {
                    model = DishStoreBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("分类不存在");
                }
                EditModel<DishStore> em = new EditModel<DishStore>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                ViewBag.dishCategoryList = DishCategoryBLL.SingleModel.GetList($"state=1 and type=1 and aid={aId}");
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    if (string.IsNullOrEmpty(model.login_username))
                    {
                        _result.code = 0;
                        _result.msg = "请输入门店管理员账号";
                        return Json(_result);
                    }

                    if (DishStoreBLL.SingleModel.CheckExistLoginName(id, aId, model.login_username))
                    {
                        _result.code = 0;
                        _result.msg = "存在同名的管理者账号,请修改！";
                        return Json(_result);
                    }

                    if (id == 0)
                    {
                        int count = DishStoreBLL.SingleModel.GetCount($"aid={xcx.Id} and state<>-1");
                        if (count >= xcx.SCount)
                        {
                            _result.msg = $"您最多只能创建{xcx.SCount}个门店！";
                            return Json(_result);
                        }
                        //如果只能创建一个门店，默认设置为主店
                        if (xcx.SCount <= 1)
                            model.ismain = 1;
                        if (string.IsNullOrEmpty(pwd))
                        {
                            _result.msg = "密码不能为空！";
                            return Json(_result);
                        }
                        model.login_userpass = DESEncryptTools.GetMd5Base32(pwd);
                        model.id = Convert.ToInt32(DishStoreBLL.SingleModel.Add(model));
                    }
                    else
                    {
                        string updateColumns = "dish_cate_id,dish_name,dish_logo,dish_con_mobile,dish_con_phone,dish_begin_time,dish_end_time,cash_fee,login_username";
                        if (!string.IsNullOrEmpty(pwd))
                        {
                            updateColumns += ",login_userpass";
                            model.login_userpass = DESEncryptTools.GetMd5Base32(pwd);
                        }
                        model.updateTime = DateTime.Now;
                        DishStoreBLL.SingleModel.Update(model, updateColumns);
                    }


                    _result.code = 1;
                    _result.msg = "保存成功";
                    return Json(_result);
                }
            }
            return Json(_result);
        }

        public ActionResult Join()
        {
            return View();
        }

        public ActionResult Setting()
        {
            int aid = Context.GetRequestInt("aId", 0);
            if (aid <= 0)
            {
                _result.code = 500;
                _result.msg = "参数错误";
                return View("PageError", _result);
            }
            DishSetting setting = DishSettingBLL.SingleModel.GetModelByAid(aid);
            if (setting == null)
            {
                setting = new DishSetting();
                setting.aid = aid;
                setting.Id = Convert.ToInt32(DishSettingBLL.SingleModel.Add(setting));
                if (setting.Id <= 0)
                {
                    _result.code = 500;
                    _result.msg = "数据错误";
                    return View("PageError", _result);
                }
            }
            EditModel<DishSetting> model = new EditModel<DishSetting>()
            {
                DataModel = setting
            };
            return View(model);
        }

        public ActionResult SaveSetting(DishSetting setting = null)
        {
            if (setting == null)
            {
                _result.msg = "参数错误";
                return Json(_result);
            }
            if (setting.aid <= 0 || setting.Id <= 0)
            {
                _result.msg = "参数错误!";
                return View(_result);
            }
            DishSetting model = DishSettingBLL.SingleModel.GetModelByAid(setting.aid);
            if (model == null)
            {
                _result.msg = "数据错误!";
                return View(_result);
            }
            if (setting.cash_limit_jiner > 0 && setting.cash_limit_jiner < 1)
            {
                _result.msg = "最低提现金额不能小于1元";
                return Json(_result);
            }
            setting.Id = model.Id;
            _result.code = DishSettingBLL.SingleModel.Update(setting) ? 1 : 0;
            _result.msg = _result.code > 0 ? "保存成功" : "保存失败";
            return Json(_result);
        }

        public ActionResult EnterStoreManage(int aId = 0, int id = 0)
        {
            _result.code = 1;
            _result.msg = Utility.DESEncryptTools.DESEncrypt(id.ToString());
            return Json(_result);
        }
    }
}