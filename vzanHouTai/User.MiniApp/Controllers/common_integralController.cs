using BLL.MiniApp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Stores;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Stores;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using User.MiniApp.Filters;
using Utility;
using Utility.IO;
using BLL.MiniApp.FunList;
using System.Linq;

namespace User.MiniApp.Controllers
{
    public partial class commonController : baseController
    {
        protected Return_Msg result;
        #region 积分商城

        /// <summary>
        /// 获取积分活动列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns
        [RouteAuthCheck]
        public ActionResult ExchangeActivityList(int appId=0)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int integralSwtich = 0;//积分开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                versionId = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    OperationMgr operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                    integralSwtich = operationMgr.Integral;
                }

            }

            ViewBag.integralSwtich = integralSwtich;
            ViewBag.versionId = versionId;
            ViewBag.PageType = xcxTemplate.Type;
            ViewBag.appId = appId;

            return View();

        }


        /// <summary>
        /// 获取积分活动列表数据
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetExchangeActivityList(int appId, int pageIndex = 1, int pageSize = 10)
        {
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            string activityname = Context.GetRequest("activityname", string.Empty);
            string strWhere = $"appId={appId} and isdel=0";
            int state = Context.GetRequestInt("state", -1);
            activityname = StringHelper.ReplaceSqlKeyword(activityname);
            if (state > -1)
                strWhere += $" and state={state}";
            if (!string.IsNullOrEmpty(activityname))
                strWhere += $" and activityname like '%{activityname}%'";

            List<ExchangeActivity> list = ExchangeActivityBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", "SortNumber desc,Id desc");
            int TotalCount = ExchangeActivityBLL.SingleModel.GetCount(strWhere);
            list.ForEach(x =>
            {
                List<C_Attachment> activityimgList = C_AttachmentBLL.SingleModel.GetListByCache(x.id, (int)AttachmentItemType.小程序积分活动图片);
                if (activityimgList != null && activityimgList.Count > 0)
                {

                    x.activityimg = Utility.ImgHelper.ResizeImg(activityimgList[0].filepath, 50, 50);

                }

            });

            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, ExchangeActivityList = list } }, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 新增或者修改积分产品数据展示页面
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult ExchangeActivitySet(int appId, int Id = 0)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });


            ViewBag.PageType = xcxTemplate.Type;
            ExchangeActivity exchange = new ExchangeActivity();

            if (Id > 0)
            {
                //表示更新
                exchange = ExchangeActivityBLL.SingleModel.GetModel(Id);
                if (exchange == null)
                    return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
                if (exchange.appId != appId)
                    return View("PageError", new Return_Msg() { Msg = "暂无权限!", code = "403" });

                List<C_Attachment> activityimgList = C_AttachmentBLL.SingleModel.GetListByCache(exchange.id, (int)AttachmentItemType.小程序积分活动图片);
                if (activityimgList != null && activityimgList.Count > 0)
                {
                    ViewBag.activityimg = new List<object>() { new { id = activityimgList[0].id, url = activityimgList[0].filepath } };
                }

                List<object> imgs = new List<object>();
                List<C_Attachment> imgList = C_AttachmentBLL.SingleModel.GetListByCache(exchange.id, (int)AttachmentItemType.小程序积分活动轮播图);
                if (imgList != null && imgList.Count > 0)
                {
                    imgList.ForEach(x =>
                    {
                        imgs.Add(new { id = x.id, url = x.filepath });

                    });
                    ViewBag.imgs = imgs;
                }


            }
            else
            {
                //表示新增
                exchange = new ExchangeActivity();
            }
            ViewBag.appId = appId;
            return View(exchange);

        }

        /// <summary>
        /// 新增或者修改积分产品数据保存数据
        /// </summary>
        /// <param name="exChangeModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ExchangeActivitySaveSet(ExchangeActivity exChangeModel)
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "找不到该小程序模板" }, JsonRequestBehavior.AllowGet);





            if (exChangeModel.activityname.Length <= 0 || exChangeModel.activityname.Length > 100)
                return Json(new { isok = false, msg = "活动名称不能为空并且小于100字符" }, JsonRequestBehavior.AllowGet);

            if (!Regex.IsMatch(exChangeModel.integral.ToString(), @"^\+?[0-9]*$"))
                return Json(new { isok = false, msg = "所需积分必须为整数" }, JsonRequestBehavior.AllowGet);
            if (!Regex.IsMatch(exChangeModel.stock.ToString(), @"^\+?[0-9]*$"))
                return Json(new { isok = false, msg = "库存必须为正整数" }, JsonRequestBehavior.AllowGet);
            if (!Regex.IsMatch(exChangeModel.perexgcount.ToString(), @"^\+?[1-9][0-9]*$"))
                return Json(new { isok = false, msg = "每人可兑换数量必须为正整数且大于0" }, JsonRequestBehavior.AllowGet);

            if (!Regex.IsMatch(exChangeModel.freight.ToString(), @"^\+?[0-9]*$"))
                return Json(new { isok = false, msg = "运费不合法!" }, JsonRequestBehavior.AllowGet);

            if (exChangeModel.exchangeway == 1)
            {

                //表示积分+金额兑换需要验证金额
                if (!Regex.IsMatch(exChangeModel.price.ToString(), @"^\+?[1-9][0-9]*$") || exChangeModel.price <= 0)
                    return Json(new { isok = false, msg = "金额不合法!" }, JsonRequestBehavior.AllowGet);
            }

            //图片
            string activityimg = Context.GetRequest("activityimg", string.Empty);
            string imgs = Context.GetRequest("imgs", string.Empty);

            bool result = false;

            string[] Imgs = imgs.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (exChangeModel.id > 0)
            {
                //表示更新
                ExchangeActivity model = ExchangeActivityBLL.SingleModel.GetModel(exChangeModel.id);
                if (model == null)
                    return Json(new { isok = false, msg = "数据不存在!" }, JsonRequestBehavior.AllowGet);
                exChangeModel.UpdateDate = DateTime.Now;

                result = ExchangeActivityBLL.SingleModel.Update(exChangeModel);
            }
            else
            {

                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    FunctionList functionList = new FunctionList();
                    int industr = xcx.VersionId;
                    functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                    if (functionList == null)
                    {
                        return Json(new { isok = false, msg = $"功能权限未设置" }, JsonRequestBehavior.AllowGet);

                    }
                    OperationMgr operationMgr = new OperationMgr();
                    if (!string.IsNullOrEmpty(functionList.OperationMgr))
                    {
                        operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                    }

                    if (operationMgr.Integral == 1)
                    {
                        return Json(new { isok = false, msg = $"请先升级到更高版本才能开启此功能" }, JsonRequestBehavior.AllowGet);
                    }

                }


                //表示新增
                if (activityimg.Length <= 0)
                    return Json(new { isok = false, msg = "请上传活动图片!" }, JsonRequestBehavior.AllowGet);

                if (Imgs.Length <= 0 || Imgs.Length > 5)
                    return Json(new { isok = false, msg = "轮播图至少一张最多5张!" }, JsonRequestBehavior.AllowGet);
                exChangeModel.appId = appId;
                exChangeModel.apptype = xcxTemplate.Type;
                exChangeModel.AddTime = DateTime.Now;
                exChangeModel.UpdateDate = DateTime.Now;
                int id = Convert.ToInt32(ExchangeActivityBLL.SingleModel.Add(exChangeModel));
                result = id > 0;
                exChangeModel.id = id;

            }

            if (result)
            {
                if (!string.IsNullOrEmpty(activityimg))
                {
                    //添加店铺Logo
                    C_AttachmentBLL.SingleModel.Add(new C_Attachment
                    {
                        itemId = exChangeModel.id,
                        createDate = DateTime.Now,
                        filepath = activityimg,
                        itemType = (int)AttachmentItemType.小程序积分活动图片,
                        thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(activityimg, 300, 300),
                        status = 0
                    });
                }


                if (Imgs.Length > 0)
                {
                    foreach (string img in Imgs)
                    {
                        //判断上传图片是否以http开头，不然为破图-蔡华兴
                        if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http") == 0)
                        {
                            C_AttachmentBLL.SingleModel.Add(new C_Attachment
                            {
                                itemId = exChangeModel.id,
                                createDate = DateTime.Now,
                                filepath = img,
                                itemType = (int)AttachmentItemType.小程序积分活动轮播图,
                                thumbnail = img,
                                status = 0
                            });
                        }

                    }

                }

                return Json(new { isok = true, msg = "操作成功！", obj = exChangeModel.id }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = true, msg = "操作失败！" }, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 删除或者下架积分产品
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelOrDownExchangeActivity(int appId, int Id)
        {
            int actionType = Context.GetRequestInt("actionType", 0);
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            ExchangeActivity model = ExchangeActivityBLL.SingleModel.GetModel(Id);
            if (model == null)
                return Json(new { isok = false, msg = "数据不存在!" }, JsonRequestBehavior.AllowGet);
            if (model.appId != appId)
                return Json(new { isok = false, msg = "没有权限!" }, JsonRequestBehavior.AllowGet);

            string field = "UpdateDate,";
            if (actionType == 0)
            {
                //表示 上架或者下架
                model.state = model.state == 0 ? 1 : 0;
                model.UpdateDate = DateTime.Now;
                field += "state";
            }
            else
            {
                //表示删除
                model.isdel = 1;
                model.UpdateDate = DateTime.Now;
                field += "isdel";
            }


            return Json(new { isok = true, msg = ExchangeActivityBLL.SingleModel.Update(model, field) ? "操作成功" : "操作失败" }, JsonRequestBehavior.AllowGet);

        }



        /// <summary>
        /// 积分规则列表
        /// </summary>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult ExchangeRuleList(int appId = 0)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            Store storeModel = StoreBLL.SingleModel.GetModelByAId(appId);
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "店铺信息错误!", code = "403" });
            }

            int integralSwtich = 0;//积分开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                versionId = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    OperationMgr operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                    integralSwtich = operationMgr.Integral;
                }

            }

            ViewBag.integralSwtich = integralSwtich;
            ViewBag.versionId = versionId;

            ViewBag.PageType = xcxTemplate.Type;
            ViewBag.appId = appId;

            List<ExchangeRule> listRule = ExchangeRuleBLL.SingleModel.GetList($"appId={appId} and state=0");
            
            listRule.ForEach(x =>
            {
                if (x.ruleType == 1 && !string.IsNullOrEmpty(x.goodids))
                {
                    List<EntGoods> listgoods = EntGoodsBLL.SingleModel.GetList($" id in({x.goodids})");
                    listgoods.ForEach(y =>
                    {
                        x.goodslist.Add(new PickGood { Id = y.id, GoodsName = y.name, ImgUrl = y.img, sel = true, showtime = y.showUpdateTime });

                    });
                }


            });
            ExchangePlayCardConfig exchangePlayCardConfig = new ExchangePlayCardConfig();
            if (string.IsNullOrEmpty(storeModel.configJson))
            {
                storeModel.funJoinModel = new StoreConfigModel();
            }
            else
            {
                storeModel.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson);
            }
            if (!string.IsNullOrEmpty(storeModel.funJoinModel.ExchangePlayCardConfig))
            {
                exchangePlayCardConfig = JsonConvert.DeserializeObject<ExchangePlayCardConfig>(storeModel.funJoinModel.ExchangePlayCardConfig);
            }

            ViewBag.ExchangePlayCardConfig = exchangePlayCardConfig;

            return View(listRule);
        }


        /// <summary>
        /// 新增或者修改积分规则
        /// </summary>
        /// <param name="exchangeRule"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveExchangeRule(ExchangeRule exchangeRule)
        {
            if (exchangeRule.appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(exchangeRule.appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "找不到小程序模板" }, JsonRequestBehavior.AllowGet);




            int ruleTotalCount = ExchangeRuleBLL.SingleModel.GetCount($"appId={exchangeRule.appId} and state=0");
            if (ruleTotalCount >= 10)
                return Json(new { isok = false, msg = "最多设置10条规则" }, JsonRequestBehavior.AllowGet);

            if (!Regex.IsMatch(exchangeRule.integral.ToString(), @"^\+?[1-9][0-9]*$"))
                return Json(new { isok = false, msg = "积分必须为大于零的整数" }, JsonRequestBehavior.AllowGet);

            if (!Regex.IsMatch(exchangeRule.price.ToString(), @"^\+?[0-9][0-9]*$"))
                return Json(new { isok = false, msg = "金额不合法!" }, JsonRequestBehavior.AllowGet);

            if (exchangeRule.ruleType == 1)
            {
                //表示选择部分商品消费赠送积分
                if (!StringHelper.IsNumByStrs(',', exchangeRule.goodids))
                    return Json(new { isok = false, msg = "请选择消费赠送金额的商品!" }, JsonRequestBehavior.AllowGet);
            }

            if (exchangeRule.Id > 0)
            {
                ExchangeRule model = ExchangeRuleBLL.SingleModel.GetModel(exchangeRule.Id);
                if (model == null)
                    return Json(new { isok = false, msg = "数据不存在!" }, JsonRequestBehavior.AllowGet);
                //表示更新
                exchangeRule.UpdateDate = DateTime.Now;
                if (ExchangeRuleBLL.SingleModel.Update(exchangeRule))
                    return Json(new { isok = true, msg = "更新成功!" }, JsonRequestBehavior.AllowGet);
                return Json(new { isok = false, msg = "更新失败!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    FunctionList functionList = new FunctionList();
                    int industr = xcx.VersionId;
                    functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                    if (functionList == null)
                    {
                        return Json(new { isok = false, msg = $"功能权限未设置" }, JsonRequestBehavior.AllowGet);

                    }
                    OperationMgr operationMgr = new OperationMgr();
                    if (!string.IsNullOrEmpty(functionList.OperationMgr))
                    {
                        operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                    }

                    if (operationMgr.Integral == 1)
                    {
                        return Json(new { isok = false, msg = $"请先升级到更高版本才能开启此功能" }, JsonRequestBehavior.AllowGet);
                    }

                }


                //表示新增
                exchangeRule.AddTime = DateTime.Now;
                exchangeRule.UpdateDate = DateTime.Now;
                int ruleId = Convert.ToInt32(ExchangeRuleBLL.SingleModel.Add(exchangeRule));
                if (ruleId > 0)
                    return Json(new { isok = true, msg = "新增成功!", obj = ruleId }, JsonRequestBehavior.AllowGet);
                return Json(new { isok = false, msg = "新增失败!" }, JsonRequestBehavior.AllowGet);
            }




        }

        /// <summary>
        /// 删除积分规则
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelExchangeRule(int appId, int Id)
        {
            if (appId <= 0 || Id <= 0)
                return Json(new { isok = false, msg = "删除失败!" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            ExchangeRule model = ExchangeRuleBLL.SingleModel.GetModel($"appId={appId} and Id={Id}");
            if (model == null)
                return Json(new { isok = false, msg = "数据不存在" }, JsonRequestBehavior.AllowGet);
            model.state = 1;
            model.UpdateDate = DateTime.Now;
            bool result = ExchangeRuleBLL.SingleModel.Update(model, "state,UpdateDate");
            return Json(new { isok = result, msg = result ? "删除成功" : "删除失败" }, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 用户兑换列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult JoinExchangeActivityList(int appId=0)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcx.VersionId;
            }
            ViewBag.versionId = versionId;

            ViewBag.PageType = xcxTemplate.Type;
            ViewBag.appId = appId;
            return View();

        }

        public ActionResult GetExchangeActivityOrders(int appId, int pageIndex = 1, int pageSize = 10)
        {
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            string activityname = Context.GetRequest("activityname", string.Empty);
            string nickname = Context.GetRequest("nickname", string.Empty);

            string strWhere = $"a.appId={appId} and a.state>1";
            activityname = StringHelper.ReplaceSqlKeyword(activityname);

            if (!string.IsNullOrEmpty(activityname))
                strWhere += $" and activityname like '%{activityname}%'";

            if (!string.IsNullOrEmpty(nickname))
                strWhere += $" and NickName like '%{nickname}%'";

            List<ExchangeActivityOrder> list = ExchangeActivityOrderBLL.SingleModel.GetJoinList(strWhere, pageSize, pageIndex, "Id desc");
            int TotalCount = ExchangeActivityOrderBLL.SingleModel.GetJoinCount(strWhere);

            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, JoinExchangeActivitys = list } }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 发货 积分兑换商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult giveGoods(int appId, int orderId)
        {
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            ExchangeActivityOrder model = ExchangeActivityOrderBLL.SingleModel.GetModel($"Id={orderId} and appId={appId}");
            if (model == null)
                return Json(new { isok = false, msg = "数据不存在" }, JsonRequestBehavior.AllowGet);
            if (model.state != 2)
                return Json(new { isok = false, msg = "状态错误" }, JsonRequestBehavior.AllowGet);

            if (model.Way != 0)
            {
                model.state = 4;
            }
            else
            {
                model.state = 3;
            }

            
            if (ExchangeActivityOrderBLL.SingleModel.Update(model, "state"))
                return Json(new { isok = true, msg = "发货成功!" }, JsonRequestBehavior.AllowGet);
            return Json(new { isok = false, msg = "发货异常" }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult SaveExchangePlayCardConfig(ExchangePlayCardConfig model)
        {
            result = new Return_Msg();
            int appId = Context.GetRequestInt("appId",0);
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }
              
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result);
            }
          

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
            {
                result.Msg = "找不到小程序模板";
                return Json(result);
            }
           
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    result.Msg = "功能权限未设置";
                    return Json(result);
                }
               
                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }

                if (operationMgr.Integral == 1)
                {
                    result.Msg = "请先升级到更高版本才能开启此功能";
                    return Json(result);
                }
               

            }

            Store store = StoreBLL.SingleModel.GetModelByRid(appId);
            if (store == null)
            {
                result.Msg = $"店铺配置不存在";
                return Json(result);
            }
            ExchangePlayCardConfig exchangePlayCardConfig = new ExchangePlayCardConfig();
            store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson) ?? new StoreConfigModel();//若为 null 则new一个新的配置
            if (store.funJoinModel != null)
            {
                store.funJoinModel.ExchangePlayCardConfig = JsonConvert.SerializeObject(model);
                store.configJson= JsonConvert.SerializeObject(store.funJoinModel);
            }
            
            if(StoreBLL.SingleModel.Update(store, "configJson"))
            {
                result.Msg = $"保存成功";
                return Json(result);
            }
            else
            {
                result.Msg = $"保存失败";
                return Json(result);
            }



        }


        /// <summary>
        /// 获取用户签到送的积分列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserInfoPoints(int appId, int pageIndex = 1, int pageSize = 10,string nickName="")
        {
            result = new Return_Msg();
            int totalCount = 0;
            List<UserPointsInfo> list = ExchangePlayCardRelationBLL.SingleModel.GetUserPointsInfoList(appId,out totalCount, pageIndex, pageSize, nickName);

            result.dataObj = new { list = list, recordCount=totalCount };
            result.isok = true;
            result.Msg = "获取成功";
            return Json(result);

        }

        public ActionResult UpdateSortExchangeActivity(List<ExchangeActivity> list)
        {
            result = new Return_Msg();

            if (list == null || list.Count <= 0)
            {
                result.Msg = "数据不能为空";
                return Json(result);

            }
            ExchangeActivity model = new ExchangeActivity();
            TransactionModel tranModel = new TransactionModel();
            string sql = string.Empty;
            foreach (ExchangeActivity item in list)
            {
                model = ExchangeActivityBLL.SingleModel.GetModel(item.id);
                if (model == null)
                {
                    result.Msg = $"Id={item.id}不存在数据库里";
                    return Json(result);
                }

                if (model.appId != item.appId)
                {
                    result.Msg = $"Id={item.id}权限不足";
                    return Json(result);
                }


                model.SortNumber = item.SortNumber;
                model.UpdateDate = DateTime.Now;
                sql = ExchangeActivityBLL.SingleModel.BuildUpdateSql(model, "SortNumber,UpdateDate");
                tranModel.Add(sql);

            }

            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (ExchangeActivityBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    result.isok = true;
                    result.Msg = "操作成功";
                    return Json(result);

                }
                else
                {

                    result.Msg = "操作失败";
                    return Json(result);

                }
            }
            else
            {
                result.Msg = "没有需要更新的数据";
                return Json(result);

            }
        }

        [RouteAuthCheck]
        public ActionResult UserIntegralDetail(int appId=0,int pageType=22)
        {
            ViewBag.PageType = pageType;
            ViewBag.appId = appId;
            return View();
        }





        /// <summary>
        /// 获取积分榜单
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public ActionResult GetUserIntegralDetail(int aid, int pageIndex = 1, int pageSize = 10, string nickName = "")
        {
            result = new Return_Msg();
            int totalCount = 0;
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(aid, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.isok = false;
                result.Msg = "小程序未授权";
                return Json(result);
            }


            List<UserIntegralDetail> list = ExchangeUserIntegralBLL.SingleModel.GetUserIntegralDetailList(xcx.AppId,aid,out totalCount, pageIndex, pageSize, nickName);

            result.dataObj = new { list = list, recordCount = totalCount };
            result.isok = true;
            result.Msg = "获取成功";
            return Json(result);

        }


        /// <summary>
        /// 手动修改用户积分
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="points"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult ChangeUserIntegral(int appId, int points,int userId)
        {
            result = new Return_Msg();
            if (appId <= 0)
            {
            
                result.Msg = "参数错误";
                return Json(result);
            }
               
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }
               
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result);
            }

            if (points > 99999)
            {
                result.Msg = "赠送的积分不能大于99999";
                return Json(result);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                result.Msg = "会员不存在";
                return Json(result);

            }

                ExchangeUserIntegral exchangeUserIntegral = ExchangeUserIntegralBLL.SingleModel.GetModel($"userId={userId}");
            if (exchangeUserIntegral == null)
            {
                int curUserIntegralId = Convert.ToInt32(ExchangeUserIntegralBLL.SingleModel.Add(new ExchangeUserIntegral()
                {
                    UserId = userId,
                    AddTime = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    integral = 0

                }));
                if (curUserIntegralId <= 0)
                {
                    result.Msg = "初始化用户积分信息异常";
                    return Json(result);
                }
            }


            if (!ExchangeUserIntegralBLL.SingleModel.ChangeUserPoints(exchangeUserIntegral, appId, points))
            {
            
                result.Msg = "修改失败";
                return Json(result);
            }


            result.isok = true;
            result.Msg = "修改成功";
            return Json(result);
        }






        #endregion
    }
}