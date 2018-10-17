using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using Entity.MiniApp.ViewModel;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Filters;
using User.MiniApp.Model;
using Utility;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class MiappEnterpriseProController : enterpriseproController
    {
    }

    [RouteAuthCheck]
    public class enterpriseproController : enterpriseController
    {
        protected readonly DadaOrderBLL _dadaOrderBLL;
        protected readonly SalesManRecordBLL salesManRecordBLL;
        protected readonly CityMordersBLL _citymordersBLL;
        protected readonly Return_Msg _returnMsg;

        protected Return_Msg result;
        
        public enterpriseproController()
        {
            this.PageType = 22;
            StoreBLL.SingleModel.GetModelByAId(Utility.IO.Context.GetRequestInt("appId", 0));
            
            _dadaOrderBLL = new DadaOrderBLL();
            _citymordersBLL = new CityMordersBLL();
            _returnMsg = new Return_Msg();
            salesManRecordBLL = new SalesManRecordBLL();
        }

        /// <summary>
        /// 店铺配置
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult StoreSetting()
        {
            int appId = Context.GetRequestInt("appId", 0);
            Store storeModel = StoreBLL.SingleModel.GetModelByAId(appId);
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "店铺信息错误!", code = "403" });
            }

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(storeModel.appId);
            if (app == null)
            {
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(app.TId);
            if (xcxTemplate == null)
            {
                return View("PageError", new Return_Msg() { Msg = "找不到模板!", code = "403" });
            }

            int productQrcodeSwitch = 0;
            int switchReceiving = 0;
            int switchSearch = 0;
            int switchReserveShopping = 0;
            int versionId = 0;
            if ((int)TmpType.小程序专业模板 == xcxTemplate.Type)
            {
                FunctionList functionList = new FunctionList();
                versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "此功能未开启!", code = "403" });
                }

                FuncMgr funcMgr = new FuncMgr();
                StoreConfig storeConfig = new StoreConfig();
                ComsConfig comsConfig = new ComsConfig();
                if (!string.IsNullOrEmpty(functionList.FuncMgr))
                {
                    funcMgr = JsonConvert.DeserializeObject<FuncMgr>(functionList.FuncMgr);
                }
                if (!string.IsNullOrEmpty(functionList.StoreConfig))
                {
                    storeConfig = JsonConvert.DeserializeObject<StoreConfig>(functionList.StoreConfig);
                }
                if (!string.IsNullOrEmpty(functionList.ComsConfig))
                {
                    comsConfig = JsonConvert.DeserializeObject<ComsConfig>(functionList.ComsConfig);
                }

                productQrcodeSwitch = funcMgr.ProductQrcodeSwitch;
                switchReceiving = storeConfig.SwitchReceiving;
                switchSearch = comsConfig.Search;
                switchReserveShopping = funcMgr.ReserveShopping;
            }
            ViewBag.versionId = versionId;
            ViewBag.appId = appId;
            ViewBag.productQrcodeSwitch = productQrcodeSwitch;
            ViewBag.switchReceiving = switchReceiving;
            ViewBag.switchSearch = switchSearch;
            ViewBag.switchReserveShopping = switchReserveShopping;
            ViewBag.Attachmentvoicepath = storeModel.VoiceUrl;
            ViewBag.VoiceType = (int)AttachmentItemType.小程序专业版新订单提示语音;
            //json转换可能报错,加try catch
            try
            {
                storeModel.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson) ?? new StoreConfigModel();//若为 null 则new一个新的配置
                if (!string.IsNullOrEmpty(storeModel.funJoinModel.goodsCatIds))
                {
                    storeModel.goodsCatList = EntGoodTypeBLL.SingleModel.GetListByIds(appId, storeModel.funJoinModel.goodsCatIds);
                }
            }
            catch (Exception)
            {
                storeModel.funJoinModel = new StoreConfigModel();
            }
            //获取门店自提地点
            List<PickPlace> placeList = PickPlaceBLL.SingleModel.GetListByAid(appId);
            if ((placeList == null || placeList.Count <= 0) && (storeModel.funJoinModel.openInvite|| storeModel.funJoinModel.openToStoreConsume) && !string.IsNullOrEmpty(storeModel.Address))
            {
                PickPlace pickPlace = new PickPlace() { aid = appId, name = storeModel.name, lat = storeModel.Lat, lng = storeModel.Lng, address = storeModel.Address, addtime = DateTime.Now };
                pickPlace.Id = Convert.ToInt32(PickPlaceBLL.SingleModel.Add(pickPlace));
                placeList = new List<PickPlace>();
                placeList.Add(pickPlace);
            }
            ViewData["placeList"] = placeList;
            ViewBag.StorePayQrcodeAlt = "请点击重新加载支付码";

            //小程序二维码
            qrcodeclass qrcodemodel = new qrcodeclass();
            if (string.IsNullOrEmpty(storeModel.funJoinModel.StorePayQrcode))
            {
                int count = 0;//重试次数
                bool ok = false;

                while (!ok)
                {
                    count++;

                    string token = "";
                    if (XcxApiBLL.SingleModel.GetToken(app, ref token))
                    {
                        qrcodemodel = CommondHelper.GetMiniAppQrcode(token, "pages/my/storePay", "", 500);

                        if (qrcodemodel != null && !string.IsNullOrEmpty(qrcodemodel.url))
                        {
                            storeModel.funJoinModel.StorePayQrcode = qrcodemodel.url;
                            storeModel.configJson = JsonConvert.SerializeObject(storeModel.funJoinModel);
                            StoreBLL.SingleModel.Update(storeModel, "configJson");
                            ok = true;
                        }
                    }
                    
                    if (count > 3)
                    {
                        ok = true;
                    }

                }
            }

            return View(storeModel);
        }

        public ActionResult GetStorePayQrcode(int appId)
        {
            result = new Return_Msg();
            Store model = StoreBLL.SingleModel.GetModelByAId(appId);
            if (model == null)
            {
                result.Msg = $"门店信息不存在";
                return Json(result);
            }
            if (!string.IsNullOrEmpty(model.configJson))
            {
                model.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(model.configJson);
                model.funJoinModel.StorePayQrcode = string.Empty;
                model.configJson = JsonConvert.SerializeObject(model.funJoinModel);
                if(!StoreBLL.SingleModel.Update(model, "configJson"))
                {
                    result.Msg = $"刷新异常";
                    return Json(result);
                }

            }
            result.isok = true;
            result.Msg = $"刷新成功";
            return Json(result);

        }


        /// <summary>
        /// 店铺配置
        /// </summary>
        /// <returns></returns>
        [LoginFilter, HttpPost]
        public ActionResult SaveStoreSetting(Store storeModel)
        {
            result = new Return_Msg();
            if (storeModel == null || storeModel.Id < 0 || storeModel.funJoinModel == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"店铺资料错误 || storeModel.Id ({storeModel?.Id}) || storeModel.funJoinModel: ({JsonConvert.SerializeObject(storeModel?.funJoinModel)})");
                result.Msg = $"店铺资料错误";
                return Json(result);
            }
            Store model = StoreBLL.SingleModel.GetModel(storeModel.Id);
            if (model == null)
            {
                result.Msg = $"门店信息不存在";
                return Json(result);
            }
            if (string.IsNullOrEmpty(model.configJson))
            {
                model.funJoinModel = new StoreConfigModel();
            }
            else
            {
                model.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(model.configJson);
            }

            string action = Context.GetRequest("action", string.Empty);
            switch (action)
            {
                case "savebase":
                    XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(storeModel.appId);
                    if (app == null)
                    {
                        result.Msg = $"小程序未授权";
                        return Json(result);
                    }

                    XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(app.TId);
                    if (xcxTemplate == null)
                    {
                        result.Msg = $"找不到模板";
                        return Json(result);
                    }

                    if ((int)TmpType.小程序专业模板 == xcxTemplate.Type)
                    {
                        FunctionList functionList = new FunctionList();
                        int VersionId = app.VersionId;
                        functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={VersionId}");
                        if (functionList == null)
                        {
                            result.Msg = $"此功能未开启";
                            return Json(result);
                        }

                        FuncMgr funcMgr = new FuncMgr();

                        if (!string.IsNullOrEmpty(functionList.FuncMgr))
                        {
                            funcMgr = JsonConvert.DeserializeObject<FuncMgr>(functionList.FuncMgr);
                        }
                        if (funcMgr.IM == 1 && storeModel.funJoinModel.imSwitch)
                        {
                            result.Msg = $"请升级到更高版本才能开启此功能";
                            return Json(result);
                        }
                        if (funcMgr.ProductQrcodeSwitch == 1 && storeModel.funJoinModel.productQrcodeSwitch)
                        {
                            result.Msg = $"请升级到更高版本才能开启此功能";
                            return Json(result);
                        }
                        if (funcMgr.ReserveShopping == 1 && storeModel.funJoinModel.reserveSwitch)
                        {
                            result.Msg = $"请升级到更高版本才能开启此功能";
                            return Json(result);
                        }
                    }
                   
                    model.Address = storeModel.Address;
                    model.OpenNewOrderPrompt = storeModel.OpenNewOrderPrompt;
                    model.VoiceType = storeModel.VoiceType;
                    model.VoiceUrl = storeModel.VoiceUrl;
                    model.funJoinModel.openSearchKeyword = storeModel.funJoinModel.openSearchKeyword;
                    model.funJoinModel.searchKeyword = storeModel.funJoinModel.searchKeyword;

                    model.funJoinModel.productQrcodeSwitch = storeModel.funJoinModel.productQrcodeSwitch;
                    model.funJoinModel.reserveSwitch = storeModel.funJoinModel.reserveSwitch;
                    model.funJoinModel.reserveClass = storeModel.funJoinModel.reserveClass;
                    model.funJoinModel.imSwitch = storeModel.funJoinModel.imSwitch;
                    model.funJoinModel.sayHello = storeModel.funJoinModel.sayHello;
                    model.funJoinModel.helloWords = storeModel.funJoinModel.helloWords;
                    model.funJoinModel.returnAddress = storeModel.funJoinModel.returnAddress;
                    model.funJoinModel.CashOnDelivery = storeModel.funJoinModel.CashOnDelivery;
                    model.funJoinModel.openExpress = storeModel.funJoinModel.openExpress;



                    model.configJson = JsonConvert.SerializeObject(model.funJoinModel);

                    result.isok = StoreBLL.SingleModel.Update(model, "Address,configJson,OpenNewOrderPrompt,VoiceType,VoiceUrl");
                    break;

                case "savestore":
                    if (string.IsNullOrEmpty(storeModel.name))
                    {
                        result.Msg = "请输入店铺名称";
                        return Json(result);
                    }
                    if (string.IsNullOrEmpty(storeModel.logo))
                    {
                        result.Msg = "请添加店铺logo";
                        return Json(result);
                    }
                    if (string.IsNullOrEmpty(storeModel.funJoinModel.takeoutAddress))
                    {
                        result.Msg = "请选择店铺地址";
                        return Json(result);
                    }
                    if ((storeModel.funJoinModel.Sunday || storeModel.funJoinModel.Tuesday || storeModel.funJoinModel.Wensday || storeModel.funJoinModel.Thursday || storeModel.funJoinModel.Friday || storeModel.funJoinModel.Saturday || storeModel.funJoinModel.Sunday) && (string.IsNullOrEmpty(storeModel.funJoinModel.StartTime) || string.IsNullOrEmpty(storeModel.funJoinModel.EndTime)))
                    {
                        result.Msg = "请选择营业时间";
                        return Json(result);
                    }
                    if (string.IsNullOrEmpty(storeModel.TelePhone))
                    {
                        result.Msg = "请输入联系电话";
                        return Json(result);
                    }
                    model.name = storeModel.name;
                    model.logo = storeModel.logo;
                    model.TelePhone = storeModel.TelePhone;
                    model.notice = storeModel.notice;
                    model.pictures = storeModel.pictures;
                    model.funJoinModel.takeoutAddress = storeModel.funJoinModel.takeoutAddress;
                    model.funJoinModel.StartTime = storeModel.funJoinModel.StartTime;
                    model.funJoinModel.EndTime = storeModel.funJoinModel.EndTime;
                    model.funJoinModel.Monday = storeModel.funJoinModel.Monday;
                    model.funJoinModel.Tuesday = storeModel.funJoinModel.Tuesday;
                    model.funJoinModel.Wensday = storeModel.funJoinModel.Wensday;
                    model.funJoinModel.Thursday = storeModel.funJoinModel.Thursday;
                    model.funJoinModel.Friday = storeModel.funJoinModel.Friday;
                    model.funJoinModel.Saturday = storeModel.funJoinModel.Saturday;
                    model.funJoinModel.Sunday = storeModel.funJoinModel.Sunday;
                    model.configJson = JsonConvert.SerializeObject(model.funJoinModel);

                    result.isok = StoreBLL.SingleModel.Update(model, "name,logo,TelePhone,notice,pictures,configJson");
                    break;

                case "savetakeout":
                    if (storeModel.funJoinModel.DeliveryRange <= 0)
                    {
                        result.Msg = "配送范围错误";
                        return Json(result);
                    }
                    if (storeModel.funJoinModel.OutSide < 0)
                    {
                        result.Msg = "起送价错误";
                        return Json(result);
                    }
                    if (storeModel.funJoinModel.ShippingFee < 0)
                    {
                        result.Msg = "配送费错误";
                        return Json(result);
                    }
                    if (storeModel.funJoinModel.PackinFee < 0)
                    {
                        result.Msg = "餐盒费错误";
                        return Json(result);
                    }
                    model.funJoinModel.takeoutSwitch = storeModel.funJoinModel.takeoutSwitch;
                    model.funJoinModel.DeliveryRange = storeModel.funJoinModel.DeliveryRange;
                    model.funJoinModel.OutSide = storeModel.funJoinModel.OutSide;
                    model.funJoinModel.ShippingFee = storeModel.funJoinModel.ShippingFee;
                    model.funJoinModel.PackinFee = storeModel.funJoinModel.PackinFee;
                    model.funJoinModel.setIndexSwitch = storeModel.funJoinModel.setIndexSwitch;
                    model.funJoinModel.takeoutStyleType = storeModel.funJoinModel.takeoutStyleType;
                    model.funJoinModel.goodsCatIds = storeModel.funJoinModel.goodsCatIds;
                    model.configJson = JsonConvert.SerializeObject(model.funJoinModel);
                    result.isok = StoreBLL.SingleModel.Update(model, "configJson");
                    break;

                case "saveopenInvite":
                case "savestoreConsume":
                    model.funJoinModel.openInvite = storeModel.funJoinModel.openInvite;
                    model.funJoinModel.openToStoreConsume = storeModel.funJoinModel.openToStoreConsume;
                    model.configJson = JsonConvert.SerializeObject(model.funJoinModel);
                    result.isok = StoreBLL.SingleModel.Update(model, "configJson");
                    break;

                case "my":
                    model.funJoinModel.isopen_orderbar = storeModel.funJoinModel.isopen_orderbar;
                    model.funJoinModel.isopen_pintuan = storeModel.funJoinModel.isopen_pintuan;
                    model.funJoinModel.isopen_tuangou = storeModel.funJoinModel.isopen_tuangou;
                    model.funJoinModel.isopen_yuyue = storeModel.funJoinModel.isopen_yuyue;
                    model.funJoinModel.isopen_car = storeModel.funJoinModel.isopen_car;
                    model.funJoinModel.isopen_fenxiao = storeModel.funJoinModel.isopen_fenxiao;
                    model.funJoinModel.isopen_kefu = storeModel.funJoinModel.isopen_kefu;
                    model.funJoinModel.isopen_kanjia = storeModel.funJoinModel.isopen_kanjia;
                    model.funJoinModel.isopen_paidui = storeModel.funJoinModel.isopen_paidui;
                    model.funJoinModel.isopen_jifen = storeModel.funJoinModel.isopen_jifen;

                    model.configJson = JsonConvert.SerializeObject(model.funJoinModel);

                    result.isok = StoreBLL.SingleModel.Update(model, "configJson");
                    break;
                default:
                    result.Msg = "参数错误";
                    break;
            }
            result.Msg = result.isok ? "保存成功" : "保存失败";
            if (result.isok)
            {
                result.dataObj = model;
            }
            return Json(result);
        }

        [LoginFilter]
        public override ActionResult pageset(int appId=0)
        {
            #region 专业版 版本控制

            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            FunctionList functionList = new FunctionList();
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = app.VersionId;

                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
            }
            ViewBag.versionId = versionId;
            ViewBag.FunctionList = functionList;

            #endregion 专业版 版本控制

            int firstTypeCount = EntGoodTypeBLL.SingleModel.GetCount($"aid={appId} and parentId=0 and State=1");
            ViewBag.TypeIndex = firstTypeCount > 0 ? 0 : -1;//如果数量为0则表示没有开启二级分类模式

            InityyForm(appId);
            int templateid = Context.GetRequestInt("templateid", 0);
            EntSetting model = new EntSetting();
            if (templateid > 0)
            {
                CustomModelRelation customrelation = CustomModelRelationBLL.SingleModel.GetModel(templateid);
                if (customrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "该模板已下架，请刷新重试!", code = "403" });
                }
                model = EntSettingBLL.SingleModel.GetModel(customrelation.AId);
                if (model == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "该模板已下架，请刷新重试!", code = "403" });
                }
                model.aid = appId;
            }
            else
            {
                model = EntSettingBLL.SingleModel.GetModel(appId);
            }
            if (model == null)
                model = new EntSetting();
            ViewBag.accountid = dzuserId;

            int freightCount = EntFreightTemplateBLL.SingleModel.GetCount($" aid={app.Id} and state>=0 ");
            ///HACK 若商家无运费模板添加一个默认免费的,以便解决现阶段无运费模板无法下单的问题
            if (freightCount <= 0)
            {
                EntFreightTemplateBLL.SingleModel.Add(new EntFreightTemplate() { BaseCount = 999, aId = app.Id, Name = "免运费", BaseCost = 0, ExtraCost = 0, IsDefault = 1, CreateTime = DateTime.Now });
            }

            Store storeModel = StoreBLL.SingleModel.GetModel($"appid={appId}");
            if (storeModel != null)
            {
                model.storeId = storeModel.Id;

                try
                {
                    storeModel.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson);
                }
                catch (Exception)
                {
                    storeModel.funJoinModel = new StoreConfigModel();
                }
            }
            ViewModel<EntSetting> vm = new ViewModel<EntSetting>();
            vm.PageType = PageType;
            vm.DataModel = model;
            vm.extraConfig = storeModel.funJoinModel;

            #region 判断用户是否代理
            ViewBag.NewsTypeLevel = storeModel.NewsTypeLevel;
            string accountId = Core.MiniApp.Utils.GetBuildCookieId("dz_UserCookieNew").ToString();
            Agentinfo agentInfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(accountId);
            ViewBag.showCopyBtn = agentInfo != null;//是否显示复制小未案例按钮

            #endregion 判断用户是否代理

            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;
            if (PageType == (int)TmpType.小程序专业模板 && souceFrom == "")//&& !WebSiteConfig.CustomerLoginId.Contains(dzaccount.LoginId)
            {
                return Redirect($"/config/functionlist?appId={appId}&type={PageType}");
            }
            return View("../enterprise/pageset", vm);
        }

        [LoginFilter, HttpGet]
        public override ActionResult pedit()
        {
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);

            #region 专业版 版本控制

            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            FunctionList functionList = new FunctionList();
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = app.VersionId;

                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
            }
            ViewBag.versionId = versionId;
            ViewBag.FunctionList = functionList;

            #endregion 专业版 版本控制

            int goodtype = Context.GetRequestInt("goodtype", (int)EntGoodsType.普通产品);
            EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(id);
            ViewBag.goodtype = goodtype;

            if (goodModel == null)
                goodModel = new EntGoods() { goodtype = goodtype, EntGroups = new EntGroupsRelation() };
            else
            {
                EntGroupsRelation entGroups = EntGroupsRelationBLL.SingleModel.GetModel($"EntGoodsId={goodModel.id}");
                if (entGroups != null)
                {
                    goodModel.EntGroups = entGroups;
                }
                else
                {
                    goodModel.EntGroups = new EntGroupsRelation();
                }

                ViewBag.goodtype = goodModel.goodtype;
            }

            int firstTypeCount = 0;
            ViewBag.PageType = xcxTemplate.Type;
            ViewBag.FirstEntGoodTypeList = EntGoodTypeBLL.SingleModel.GetListByCach(appId, 50, 1, ref firstTypeCount, 0);
            ViewBag.SecondTypeSwitch = firstTypeCount;

            return View("../enterprisepro/pedit", goodModel);
        }

        [LoginFilter, HttpPost, ValidateInput(false)]
        public override ActionResult pedit(int appId, EntGoods model)
        {
            try
            {
                string act = Utility.IO.Context.GetRequest("act", "");

                #region 专业版 版本控制

                if (appId <= 0)
                    return Json(new { isok = false, msg = "appId不能小于0" });

                if (dzaccount == null)
                    return Json(new { isok = false, msg = "登录信息过期请重新登录" });

                XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
                if (app == null)
                    return Json(new { isok = false, msg = "小程序未授权" });

                XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
                if (xcxTemplate == null)
                    return Json(new { isok = false, msg = "小程序模板不存在" });

                int curProductCount = EntGoodsBLL.SingleModel.GetCount($"aid={appId} and state=1 and goodtype={(int)EntGoodsType.普通产品}");

                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    if (model.id == 0 || act == "copy")//产品增加时候进行判断
                    {
                        FunctionList functionList = new FunctionList();
                        int industr = app.VersionId;
                        functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                        if (functionList == null)
                        {
                            return Json(new { isok = false, msg = "此功能未开启" });
                        }

                        ProductMgr productMgrModel = new ProductMgr();
                        if (!string.IsNullOrEmpty(functionList.ProductMgr))
                        {
                            productMgrModel = JsonConvert.DeserializeObject<ProductMgr>(functionList.ProductMgr);
                        }
                        if (curProductCount > productMgrModel.ProductMaxCount)
                        {
                            return Json(new { isok = false, msg = $"产品数量达上限{productMgrModel.ProductMaxCount},请先升级更高版本" });
                        }
                    }
                }

                #endregion 专业版 版本控制

                //清除产品列表缓存
                EntGoodsBLL.SingleModel.RemoveEntGoodListCache(appId);

                //复制产品
                if (act == "copy")
                {
                    int pid = Utility.IO.Context.GetRequestInt("id", 0);
                    if (pid == 0)
                    {
                        return Json(new { isok = false, msg = "复制失败，产品不存在" });
                    }

                    model = EntGoodsBLL.SingleModel.GetModel($"id={pid}");
                    if (model == null || model.state == 0)
                    {
                        return Json(new { isok = false, msg = "复制失败，产品不存在或已删除" });
                    }

                    model.id = 0;
                    model.StockNo = string.Empty;
                    if (!model.name.EndsWith("复制"))
                    {
                        model.name = model.name + " 复制";
                        model.updatetime = DateTime.Now;
                    }

                    int newid = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(model));
                    if (newid > 0)
                    {
                        return Json(new { isok = true, msg = newid });
                    }
                }
                //批量操作产品
                else if (act == "batch")
                {
                    int actval = Utility.IO.Context.GetRequestInt("actval", -1);
                    string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
                    if (string.IsNullOrEmpty(ids))
                    {
                        return Json(new { isok = true, msg = "请先选择产品" });
                    }

                    List<MySqlParameter> sqlParams = new List<MySqlParameter>() { new MySqlParameter("@ids", ids) };
                    List<EntGoods> goods = EntGoodsBLL.SingleModel.GetListByParam($" find_in_set(id,@ids) >0 ", sqlParams.ToArray());
                    if (goods == null || !goods.Any())
                    {
                        return Json(new { isok = true, msg = "请先选择有效产品" });
                    }

                    goods.ForEach(g =>
                    {
                        switch (actval)
                        {
                            case 1:
                            case 0:
                                int funState = actval == 0 ? 1 : 0;

                                g.tag = actval;
                                EntGoodsBLL.SingleModel.Update(g, "tag");
                                EntGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(model.id, funState);
                                if (model.tag == 1 && actval == 0)
                                {
                                    EntSettingBLL.SingleModel.SyncData(appId, "$..coms[?(@.type=='good')].items[?(@.id==" + g.id + ")]");
                                }
                                break;

                            case -1:
                                //下架的商品才能删除
                                if (g.tag == 0)
                                {
                                    g.state = 0;
                                    EntGoodsBLL.SingleModel.Update(g, "state");
                                    EntGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(model.id, 2);
                                    EntSettingBLL.SingleModel.SyncData(appId, "$..coms[?(@.type=='good')].items[?(@.id==" + g.id + ")]");
                                }
                                break;
                        }
                    });
                    return Json(new { isok = true, msg = "设置成功" });
                }
                //删除产品
                else if (act == "del")
                {
                    int pid = Utility.IO.Context.GetRequestInt("id", 0);
                    model = EntGoodsBLL.SingleModel.GetModel($"id={pid}");
                    model.state = 0;

                    TransactionModel TranModel = new TransactionModel();
                    //商品状态变动,更新购物车内商品的状态
                    List<EntGoodsAttrDetail> goodsDtlList = model.GASDetailList;
                    List<string> updateGoodsStateSqlList = new List<string>();
                    updateGoodsStateSqlList = EntGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(model.id, 2);
                    updateGoodsStateSqlList.ForEach(x =>
                    {
                        TranModel.Add(x);
                    });

                    TranModel.Add(EntGoodsBLL.SingleModel.BuildUpdateSql(model, "state"));
                    //if (EntGoodsBLL.SingleModel.Update(model, "state"))
                    if (EntGoodsBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
                    {
                        EntSettingBLL.SingleModel.SyncData(appId, "$..coms[?(@.type=='good')].items[?(@.id==" + pid + ")]");

                        return Json(new { isok = true, msg = "删除成功" });
                    }
                    return Json(new { isok = false, msg = "删除失败" });
                }
                //更新产品标签
                else if (act == "tag")
                {
                    int pid = Utility.IO.Context.GetRequestInt("id", 0);
                    int tag = Utility.IO.Context.GetRequestInt("tag", -1);
                    if (pid == 0)
                    {
                        return Json(new { isok = false, msg = "参数错误" });
                    }
                    model = EntGoodsBLL.SingleModel.GetModel($"id={pid}");
                    if (model == null || model.state == 0)
                    {
                        return Json(new { isok = false, msg = "产品不存在或已删除" });
                    }
                    if (tag != -1)
                    {
                        bool isSyncData = false;
                        if (model.tag == 1 && tag == 0)
                        {
                            isSyncData = true;
                        }

                        model.tag = tag;

                        TransactionModel TranModel = new TransactionModel();
                        //商品状态变动,更新购物车内商品的状态
                        List<EntGoodsAttrDetail> goodsDtlList = model.GASDetailList;
                        List<string> updateGoodsStateSqlList = new List<string>();
                        updateGoodsStateSqlList = EntGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(model.id, tag == 1 ? 0 : 1, oldGoodState: tag == 1 ? 1 : 0);
                        updateGoodsStateSqlList.ForEach(x =>
                        {
                            TranModel.Add(x);
                        });
                        TranModel.Add(EntGoodsBLL.SingleModel.BuildUpdateSql(model, "tag"));
                        //if (EntGoodsBLL.SingleModel.Update(model, "tag"))
                        if (EntGoodsBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
                        {
                            if (isSyncData)
                            {
                                EntSettingBLL.SingleModel.SyncData(appId, "$..coms[?(@.type=='good')].items[?(@.id==" + pid + ")]");
                            }
                            return Json(new { isok = true, msg = "" });
                        }
                    }
                    return Json(new { isok = false, msg = "操作失败！" });
                }

                //校验产品编号唯一性
                Func<int, int, string, bool> isRepeat = (aid, goodId, stockNo) =>
                {
                    EntGoods product = EntGoodsBLL.SingleModel.GetByStockNo(aid, stockNo);
                    return product != null && product.id != goodId;
                };
                if (!string.IsNullOrWhiteSpace(model.StockNo) && isRepeat(appId, model.id, model.StockNo))
                {
                    return Json(new { isok = false, msg = "产品编号重复" });
                }
                //新增产品
                if (model.id == 0)
                {
                    int newid = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(model));
                    model.id = newid;
                    if (newid > 0)
                    {
                        //判断是否是添加拼团商品
                        if (model.goodtype == (int)EntGoodsType.拼团产品)
                        {
                            model.EntGroups.EntGoodsId = newid;
                            model.EntGroups.RId = model.aid;
                            int groupid = Convert.ToInt32(EntGroupsRelationBLL.SingleModel.Add(model.EntGroups));
                            if (groupid <= 0)
                            {
                                return Json(new { isok = false, msg = "保存失败" });
                            }
                        }
                        return Json(new { isok = true, msg = "添加成功" });
                    }
                    if (model == null)
                    {
                        return Json(new { isok = false, msg = "复制失败，产品不存在或已删除" });
                    }
                }
                else
                {
                    #region 商品价格加入购物车后,变动需要同步进去

                    //字符串转json串
                    if (!string.IsNullOrEmpty(model.specificationdetail))
                    {
                        List<EntGoodsAttrDetail> specifications = model.GASDetailList;
                        specifications.ForEach(x =>
                        {
                            EntGoodsCartBLL.SingleModel.UpdateCartByGoodsId(model.id, x.id, Convert.ToInt32(x.price * 100));
                        });

                        #region 若多规格商品被删除,更改购物车商品的标识

                        EntGoods dbGood = EntGoodsBLL.SingleModel.GetModel(model.id);
                        if (dbGood == null)
                        {
                            return Json(new { code = -1, msg = "商品不存在！" }, JsonRequestBehavior.AllowGet);
                        }

                        TransactionModel TranModel = new TransactionModel();
                        //更改已被删掉的商品
                        List<string> dbGoodSpacList = dbGood.GASDetailList.Select(x => x.id).ToList();
                        List<string> goodsSpacList = model.GASDetailList.Select(x => x.id).ToList();
                        List<string> updateGoodsStateSqlList = new List<string>();
                        if (dbGoodSpacList.Count > 0)
                        {
                            dbGoodSpacList.ForEach(x =>
                            {
                                if (!goodsSpacList.Contains(x))
                                {
                                    updateGoodsStateSqlList.AddRange(EntGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsIdSpecids(model.id, x, 2));
                                }
                            });
                        }

                        updateGoodsStateSqlList.ForEach(x =>
                        {
                            TranModel.Add(x);
                        });

                        if (!EntGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
                        {
                            return Json(new { code = -1, msg = "更改购物车标识失败！" }, JsonRequestBehavior.AllowGet);
                        }

                        #endregion 若多规格商品被删除,更改购物车商品的标识
                    }
                    //价格更新之后更新购物车价格
                    if (model.id > 0)
                    {
                        EntGoodsCartBLL.SingleModel.UpdateCartByGoodsId(model.id, "", Convert.ToInt32(model.price * 100));
                    }

                    #endregion 商品价格加入购物车后,变动需要同步进去

                    model.updatetime = DateTime.Now;
                    if (EntGoodsBLL.SingleModel.Update(model, "DayStock,originalPrice,name,img,showprice,ptypes,exttypes,exttypesstr,ptypestr,stock,stockLimit,plabels,plabelstr_array,plabelstr,specificationkeys,specification,specificationdetail,pickspecification,price,priceStr,unit,slideimgs,description,updatetime,sort,virtualSalesCount,state,tag,ServiceTime,goodtype,isDistribution,isDefaultCps_Rate,cps_rate,isTakeout,isPackin,TemplateId,Weight,StockNo"))
                    {
 
                        List<SubStoreEntGoods> subGoods = SubStoreEntGoodsBLL.SingleModel.GetList($"pid={model.id} and aid={model.aid}");
                        model.updatetime = DateTime.Now;

                        TransactionModel TranModelSync = new TransactionModel();
                        if (subGoods != null && subGoods.Count > 0)
                        {
                            subGoods.ForEach(subGood =>
                            {
                                TranModelSync.Add(EntGoodsBLL.SingleModel.GetSyncSql(model, subGood));
                            });
                        }
                        bool result = EntGoodsBLL.SingleModel.ExecuteTransactionDataCorect(TranModelSync.sqlArray, TranModelSync.ParameterArray);

                        if (!result)
                        {
                            return Json(new { isok = true, msg = "同步失败！" });
                        }

                        #region 拼团产品

                        if (model.goodtype == (int)EntGoodsType.拼团产品)
                        {
                            EntGroupsRelation entgroupmodel = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(model.id, model.aid, 0, -1);
                            if (entgroupmodel != null)
                            {
                                entgroupmodel.LimitNum = model.EntGroups.LimitNum;
                                entgroupmodel.ValidDateEnd = model.EntGroups.ValidDateEnd;
                                entgroupmodel.ValidDateStart = model.EntGroups.ValidDateStart;
                                entgroupmodel.GroupSize = model.EntGroups.GroupSize;
                                entgroupmodel.InitSaleCount = model.EntGroups.InitSaleCount;
                                entgroupmodel.ValidDateLength = model.EntGroups.ValidDateLength;
                                //  log4net.LogHelper.WriteInfo(this.GetType(),JsonConvert.SerializeObject(entgroupmodel));
                                if (!EntGroupsRelationBLL.SingleModel.Update(entgroupmodel, "LimitNum,ValidDateEnd,ValidDateStart,GroupSize,InitSaleCount,ValidDateLength"))
                                {
                                    return Json(new { isok = true, msg = "保存拼团信息失败！" });
                                }
                            }
                        }

                        #endregion 拼团产品

                        return Json(new { isok = true, msg = "修改成功" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message });
            }

            return Json(new { isok = false, msg = "操作失败" });
        }

        /// <summary>
        /// 规格列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [LoginFilter, HttpGet]
        public ActionResult specificationlist(int appId=0, int pageIndex = 1, int pageSize = 20)
        {
            #region 专业版 版本控制

            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int specificationSwtich = 0;//产品规格开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.ProductMgr))
                {
                    ProductMgr productMgr = JsonConvert.DeserializeObject<ProductMgr>(functionList.ProductMgr);
                    specificationSwtich = productMgr.ProductSpecification;
                }
            }
            ViewBag.versionId = versionId;
            ViewBag.specificationSwtich = specificationSwtich;

            #endregion 专业版 版本控制

            int fid = Utility.IO.Context.GetRequestInt("fid", 0);

            ViewModel<EntSpecification> vm = new ViewModel<EntSpecification>();
            string strwhere = $"aid={appId} and state=1 and parentid={fid}";
            vm.DataList = EntSpecificationBLL.SingleModel.GetList(strwhere, 200, 1, "*", "sort desc");
            vm.PageType = PageType;
            return View("../enterprisepro/specificationlist", vm);
        }

        /// <summary>
        /// 编辑规格
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [LoginFilter, HttpPost]
        public ActionResult specificationlist(int appId=0, EntSpecification model=null)
        {
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId不能小于0" });

            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息过期请重新登录" });

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
                return Json(new { isok = false, msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" });

            string act = Utility.IO.Context.GetRequest("act", string.Empty);
            int id = Utility.IO.Context.GetRequestInt("id", 0);

            #region 删除

            if (act == "del")
            {
                if (id <= 0)
                {
                    return Json(new { isok = false, msg = "非法请求！" });
                }
                model = EntSpecificationBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "该规格不存在！" });
                }
                //检查是否有已经有产品使用了规格或规格值
                int checkcount = 0;
                //如果删除规格
                if (model.parentid == 0)
                {
                    checkcount = EntGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({id},specificationkeys)>0 and aid={appId} and state=1");
                }
                //如果删除规格值
                else
                {
                    checkcount = EntGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({id},specification)>0 and aid={appId} and state=1");
                }

                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = $"该规格下已有{checkcount}个产品，不可删除！" });
                }

                model.state = 0;
                if (EntSpecificationBLL.SingleModel.Update(model, "state"))
                {
                    return Json(new { isok = true, msg = "删除成功！" });
                }
                else
                {
                    return Json(new { isok = true, msg = "删除失败！" });
                }
            }

            #endregion 删除

            #region 添加和修改

            if (model == null || model.id < 0)
            {
                return Json(new { isok = false, msg = "非法请求" });
            }
            if (model.name.Trim() == "" || model.name.Trim().Length > 20)
            {
                return Json(new { isok = false, msg = "规格名称不能为空，且不能超过20个字" });
            }
            else
            {
                int checkcount = EntSpecificationBLL.SingleModel.GetCount($"name=@name and aid={appId} and id not in(0,{model.id}) and parentid={model.parentid} and state=1", new MySql.Data.MySqlClient.MySqlParameter[] { new MySql.Data.MySqlClient.MySqlParameter("name", model.name) });
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = "已存在该规格名称，请重新设置！" });
                }
            }
            //修改
            if (model.id > 0)
            {
                if (EntSpecificationBLL.SingleModel.Update(model))
                {
                    return Json(new { isok = true, msg = model });
                }
            }
            //添加
            else
            {
                #region 专业版 版本控制

                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    int industr = app.VersionId;
                    FunctionList functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                    if (functionList == null)
                    {
                        return Json(new { isok = false, msg = "此功能未开启" });
                    }

                    ProductMgr productMgrModel = new ProductMgr();
                    if (!string.IsNullOrEmpty(functionList.ProductMgr))
                    {
                        productMgrModel = JsonConvert.DeserializeObject<ProductMgr>(functionList.ProductMgr);
                    }
                    if (productMgrModel.ProductSpecification == 1)//表示关闭了添加规格功能
                    {
                        return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                    }
                }

                #endregion 专业版 版本控制

                int checkcount = EntSpecificationBLL.SingleModel.GetCount($"aid={appId} and state=1 and parentid={model.parentid}");
                if (checkcount >= 200)
                {
                    return Json(new { isok = false, msg = "无法新增分类！您已添加了200个产品规格，已达到上限，请编辑已有的分类或删除部分规格后再进行新增。" });
                }
                int newid = Convert.ToInt32(EntSpecificationBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.id = newid;
                    return Json(new { isok = true, msg = model });
                }
            }

            #endregion 添加和修改

            return Json(new { isok = false, msg = "操作失败！" });
        }

        #region 订单管理

        /// <summary>
        /// 订单
        /// </summary>
        /// <param name="cityInfoId"></param>
        /// <param name="name"></param>
        /// <param name="typevalue"></param>
        /// <param name="pageIndex"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        public ActionResult OrderList(int appId = 0, string orderNum = "", int pageIndex = 0, string startdate = "", string enddate = "", string goodsName = "", string accName = "", string accPhone = "", int orderState = -999, bool export = false, int ordertype = 0)
        {
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            StringBuilder wheresql = new StringBuilder();
            wheresql.Append(" 1=1 ");

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            ViewBag.appId = appId;

            wheresql.Append(" and orders.StoreId = " + xcx.Id);

            DateTime? s = null;
            if (!startdate.IsNullOrWhiteSpace())
            {
                s = Convert.ToDateTime(startdate + " 00:00:00 ");
            }

            DateTime? e = null;
            if (!enddate.IsNullOrWhiteSpace())
            {
                e = Convert.ToDateTime(enddate + " 23:59:59 ");
            }

            ViewBag.orderNum = orderNum;
            ViewBag.startdate = startdate;
            ViewBag.enddate = enddate;
            int pageSize = 20;
            pageIndex = pageIndex == 0 ? 1 : pageIndex;
            ViewBag.pageSize = pageSize;
            ViewBag.goodsName = goodsName;
            ViewBag.accName = accName;
            ViewBag.accPhone = accPhone;
            ViewBag.orderState = orderState;

            try
            {
                int totalCount = 0;
                //获得订单记录
                List<EntGoodsOrder> companys = EntGoodsOrderBLL.SingleModel.getModelByWhere(appId, pageSize, pageIndex, out totalCount, goodsName, accName, accPhone, orderState, s, e, orderNum, "", -1, 0, ordertype);
                if (companys == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没有数据!", code = "500" });
                }

                //判断是否有团订单
                EntGroupSponsorBLL.SingleModel.GetSponsorState(ref companys);

                //判断是否有达达订单
                //_dadaOrderBLL.GetDadaOrderState(ref companys,appId);

                //获取店铺二维码名称
                EntStoreCodeBLL.SingleModel.GetStoreCodeName<EntGoodsOrder>(ref companys);

                ViewBag.TotalCount = totalCount;

                return View("../enterprisepro/OrderList", companys);
            }
            catch (Exception)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙!", code = "500" });
            }
        }

        public ActionResult GetOrderDtl()
        {
            int reservationId = Context.GetRequestInt("reservationId", 0);
            EntGoodsOrder goodsOrder = EntGoodsOrderBLL.SingleModel.GetModel($" reserveid = {reservationId} ");
            if (goodsOrder == null)
            {
                _returnMsg.Msg = "订单数据消失了";
                return Json(_returnMsg);
            }

            _returnMsg.isok = true;
            _returnMsg.Msg = "成功";
            _returnMsg.dataObj = new
            {
                goodsOrder = goodsOrder
            };
            return Json(_returnMsg);
        }

        /// <summary>
        /// 订单菜品信息列表
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult getOrderDtlItem(int orderId = 0)
        {
            List<EntGoodsCart> cartModelList = EntGoodsCartBLL.SingleModel.GetOrderDetail(orderId);
            return View("_PartialOrderDtlItem", cartModelList);
        }

        /// <summary>
        /// 更新备注
        /// </summary>
        /// <param name="id"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult OrderRemark(int id = 0, string remark = "")
        {
            StoreGoodsOrder Order = StoreGoodsOrderBLL.SingleModel.GetModel(id);
            if (Order == null)
            {
                return Json(new { isok = false, msg = "找不到该订单！" }, JsonRequestBehavior.AllowGet);
            }
            Order.Remark = remark;
            StoreGoodsOrderBLL.SingleModel.Update(Order, "Remark");
            return Json(new { isok = true, msg = "备注成功！" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 变更订单状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="oldState"></param>
        /// <param name="orderId"></param>
        /// <param name="remark">退款原因</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult updteOrderState(int state, int oldState, int orderId = 0, string attachData = null, double buyPrice = 0.00,string remark="")
        {
            ServiceResult result = new ServiceResult();
            EntGoodsOrder goodsOrder = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
            if (goodsOrder == null)
            {
                return Json(new { isok = false, msg = "订单信息异常！" }, JsonRequestBehavior.AllowGet);
            }

            if (!Enum.IsDefined(typeof(MiniAppEntOrderState), state))
            {
                return Json(new { isok = false, msg = "状态错误,请重新刷新页面！" }, JsonRequestBehavior.AllowGet);
            }

            if(state!=(int)MiniAppEntOrderState.退款失败)
            {
                //变更订单状态
                goodsOrder.State = state;
            }

            bool isSuccess = false;
            switch (state)
            {
                case (int)MiniAppEntOrderState.已取消://取消订单
                    isSuccess = EntGoodsOrderBLL.SingleModel.updateStock(goodsOrder, oldState);
                    if (isSuccess)
                    {
                        //发给用户取消通知
                        object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(goodsOrder, SendTemplateMessageTypeEnum.专业版订单取消通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(goodsOrder.UserId, SendTemplateMessageTypeEnum.专业版订单取消通知, TmpType.小程序专业模板, orderData);
                    }
                    break;

                case (int)MiniAppEntOrderState.退款中://退款
                    int refund_fee = (int)(buyPrice * 100);
                    if (goodsOrder.OrderType != 3)
                    {
                        if (refund_fee <= 0)
                        {
                            return Json(new { isok = false, msg = "请输入退款金额" }, JsonRequestBehavior.AllowGet);
                        }
                        if (refund_fee > goodsOrder.BuyPrice)
                        {
                            return Json(new { isok = false, msg = "退款金额超出实付金额" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        refund_fee = goodsOrder.BuyPrice;
                    }
                    goodsOrder.refundFee = refund_fee;
                    //goodsOrder.BuyPrice = refund_fee;
                    isSuccess = EntGoodsOrderBLL.SingleModel.outOrder(goodsOrder, oldState, goodsOrder.BuyMode, isPartOut: true, remark:remark);
                    break;

                case (int)MiniAppEntOrderState.待收货:
                    goodsOrder.DistributeDate = DateTime.Now;
                    bool isSaveDelivery = !string.IsNullOrWhiteSpace(attachData);
                    if (isSaveDelivery)
                    {
                        //保存物流信息
                        DeliveryUpdatePost DeliveryInfo = System.Web.Helpers.Json.Decode<DeliveryUpdatePost>(attachData);
                        bool isCompleteInfo = (DeliveryInfo.SelfDelivery || (!string.IsNullOrWhiteSpace(DeliveryInfo.CompanyCode) && !string.IsNullOrWhiteSpace(DeliveryInfo.DeliveryNo)))
                                              && !string.IsNullOrWhiteSpace(DeliveryInfo.ContactName)
                                              && !string.IsNullOrWhiteSpace(DeliveryInfo.ContactTel)
                                              && !string.IsNullOrWhiteSpace(DeliveryInfo.Address);
                        if (!isCompleteInfo)
                        {
                            return Json(new { isok = false, msg = "请填写物流信息" }, JsonRequestBehavior.AllowGet);
                        }
                        if (DeliveryInfo.SelfDelivery)
                        {
                            DeliveryInfo.CompanyTitle = "商家自配送";
                            DeliveryInfo.CompanyCode = null;
                        }

                        isSuccess = DeliveryFeedbackBLL.SingleModel.AddEntOrderFeed(goodsOrder.Id, DeliveryInfo) && EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(goodsOrder.Id, oldState, state);
                    }
                    else
                    {
                        isSuccess = EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(goodsOrder.Id, oldState, state);
                    }

                    if (!isSuccess && isSaveDelivery)
                    {
                        LogHelper.WriteInfo(this.GetType(), $"保存物流信息失败！订单ID：{goodsOrder.Id}，物流信息：'{attachData}'");
                    }

                    if (isSuccess)
                    {
                        SendTemplateMessageTypeEnum sendMsgType = SendTemplateMessageTypeEnum.专业版订单发货提醒;
                        if(oldState== (int)MiniAppEntOrderState.申请取消订单)
                        {
                            sendMsgType = SendTemplateMessageTypeEnum.专业版订单强行发货通知;
                        }

                        //发给用户发货通知
                        object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(goodsOrder, sendMsgType);
                        string orderUrl = $"pages/good/goodOlt?dbOrder={goodsOrder.Id}&check=true";
                        TemplateMsg_Miniapp.SendTemplateMessage(goodsOrder.UserId, SendTemplateMessageTypeEnum.专业版订单发货提醒, TmpType.小程序专业模板, orderData,orderUrl);
                    }
                    break;

                case (int)MiniAppEntOrderState.交易成功:
                    isSuccess = EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(goodsOrder.Id, oldState, state);
                    if (isSuccess)
                    {
                        //加销量
                        List<EntGoodsCart> cartList = EntGoodsCartBLL.SingleModel.GetListByGoodsOrderId(goodsOrder.Id);
                        //会员加消费金额
                        if (!VipRelationBLL.SingleModel.updatelevel(goodsOrder.UserId, "entpro", goodsOrder.BuyPrice))
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常(订单发货后 超过10天,系统自动完成订单)" + goodsOrder.Id));
                        }

                        //消费符合积分规则赠送积分

                        if (!ExchangeUserIntegralBLL.SingleModel.AddUserIntegral(goodsOrder.UserId, goodsOrder.aId, 0, goodsOrder.Id))
                            log4net.LogHelper.WriteError(GetType(), new Exception("赠送积分失败(订单发货后 超过10天,系统自动完成订单)" + goodsOrder.Id));

                        //确认收货后 判断该订单购物车里面是否是分销产生的 如果购物车里的产品佣金比例不为零则需要操作分销相关的

                        try
                        {

                            #region 分销相关
                            EntGoodsOrderBLL.SingleModel.PayDistributionMoney(cartList, goodsOrder);

                            #endregion 分销相关
                        }
                        catch (Exception ex)
                        {
                            log4net.LogHelper.WriteError(this.GetType(), ex);
                        }
                    }
                    break;

                case (int)MiniAppEntOrderState.待退货:
                case (int)MiniAppEntOrderState.拒绝退货:
                    isSuccess = EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(goodsOrder.Id, oldState, state);
                    //发送小程序模板消息
                    if (isSuccess)
                    {
                        //log4net.LogHelper.WriteInfo(this.GetType(), "1433");
                        //发给用户审核通知
                        object orderData = TemplateMsg_Miniapp.GetReturnDeliveryTempMsgData(goodsOrder, SendTemplateMessageTypeEnum.退换货订单申请审核);
                        //log4net.LogHelper.WriteInfo(this.GetType(), JsonConvert.SerializeObject(orderData));
                        TemplateMsg_Miniapp.SendTemplateMessage(goodsOrder.UserId, SendTemplateMessageTypeEnum.退换货订单申请审核, TmpType.小程序专业模板, orderData);
                    }
                    break;

                case (int)MiniAppEntOrderState.换货中:
                    if (!string.IsNullOrWhiteSpace(attachData))
                    {
                        //保存物流信息
                        DeliveryUpdatePost DeliveryInfo = System.Web.Helpers.Json.Decode<DeliveryUpdatePost>(attachData);
                        isSuccess = DeliveryFeedbackBLL.SingleModel.AddEntOrderFeedExchange(
                            goodsOrder.Id, DeliveryInfo.ContactName, DeliveryInfo.ContactTel, DeliveryInfo.Address, DeliveryInfo.DeliveryNo, DeliveryInfo.CompanyCode, DeliveryInfo.CompanyTitle, DeliveryInfo.Remark);
                    }
                    else
                    {
                        return Json(new { isok = false, msg = "请填写物流信息！" }, JsonRequestBehavior.AllowGet);
                    }
                    if (!isSuccess)
                    {
                        return Json(new { isok = false, msg = "保存物流信息失败！" }, JsonRequestBehavior.AllowGet);
                    }
                    isSuccess = EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(goodsOrder.Id, oldState, state);
                    //发送小程序模板消息
                    if (isSuccess)
                    {
                        //发给用户发货通知
                        object orderData = TemplateMsg_Miniapp.GetReturnDeliveryTempMsgData(goodsOrder, SendTemplateMessageTypeEnum.退换货订单商家发货);
                        TemplateMsg_Miniapp.SendTemplateMessage(goodsOrder.UserId, SendTemplateMessageTypeEnum.退换货订单商家发货, TmpType.小程序专业模板, orderData);
                    }
                    break;

                case (int)MiniAppEntOrderState.退货退款成功:
                    refund_fee = (int)(buyPrice * 100);
                    if (refund_fee <= 0)
                    {
                        return Json(new { isok = false, msg = "请输入退款金额" }, JsonRequestBehavior.AllowGet);
                    }
                    if (refund_fee > goodsOrder.BuyPrice)
                    {
                        return Json(new { isok = false, msg = "退款金额超出实付金额" }, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(remark))
                    {
                        return Json(new { isok = false, msg = "请输入退款原因" }, JsonRequestBehavior.AllowGet);
                    }
                    goodsOrder.refundFee = refund_fee;
                    //goodsOrder.BuyPrice = refund_fee;
                    isSuccess = EntGoodsOrderBLL.SingleModel.outOrder(goodsOrder, oldState, goodsOrder.BuyMode, (int)MiniAppEntOrderState.退货退款成功, true, remark);
                    break;

                case (int)MiniAppEntOrderState.退款失败://重新退款
                    if (goodsOrder.State != (int)MiniAppEntOrderState.退款失败)
                    {
                        return Json(new { isok = false, msg = "该订单已在退款中！" }, JsonRequestBehavior.AllowGet);
                    }
                    string erromsg = "";
                    EntGoodsOrderBLL.SingleModel.ReturnOrderAgain(goodsOrder, ref erromsg);
                    isSuccess = erromsg.Length <= 0;
                    break;
                default:
                    isSuccess = EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(goodsOrder.Id, oldState, state);
                    break;
            }

            if (!isSuccess)
            {
                return Json(new { isok = false, msg = "操作失败！" }, JsonRequestBehavior.AllowGet);
            }
            EntGoodsOrderLogBLL.SingleModel.AddLog(goodsOrder.Id, 0, $"将订单状态改为：{Enum.GetName(typeof(MiniAppEntOrderState), goodsOrder.State)}");

            return Json(new { isok = true, msg = "操作成功！" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取订单退款失败原因
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult getOutOrderFailRemark(int orderId = 0)
        {
            try
            {
                string msg = "";
                EntGoodsOrder order = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
                CityMorders ctiyMorder = new CityMordersBLL().GetModel(order.OrderId);
                ReFundResult outOrderRsult = RefundResultBLL.SingleModel.GetModel($" transaction_id = '{ctiyMorder.trade_no}' and retype = 1");

                if (outOrderRsult == null)
                {
                    msg = "请检查证书是否已安装，再重新发起退款";
                }
                else
                {
                    msg = outOrderRsult.err_code_des ?? outOrderRsult.return_msg;
                }

                return Content(msg);
            }
            catch (Exception)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙!", code = "500" });
            }
        }

        [LoginFilter]
        public ActionResult UpdateOrderMaterial()
        {
            string orderJson = Context.GetRequest("orderJson", string.Empty);
            string updateOrderCol = Context.GetRequest("updateOrderCol", string.Empty);

            if (orderJson.IsNullOrWhiteSpace())
            {
                _returnMsg.Msg = "服务器繁忙,未接收到订单数据";
                return Json(_returnMsg);
            }
            if (updateOrderCol.IsNullOrWhiteSpace())
            {
                _returnMsg.Msg = "服务器繁忙,未接收到订单处理参数";
                return Json(_returnMsg);
            }

            List<string> updateCol = new List<string>();//最终要更新的字段
            EntGoodsOrder order = null;//要修改成的订单数据
            EntGoodsOrder dbOrder = null; //数据库订单

            #region 读取订单数据

            try
            {
                order = JsonConvert.DeserializeObject<EntGoodsOrder>(orderJson);    //普通订单
                if (order == null || order.Id <= 0)
                {
                    _returnMsg.Msg = $"订单数据出现异常";
                    return Json(_returnMsg);
                }
            }
            catch (Exception)
            {
                _returnMsg.Msg = "订单数据存在异常";
                return Json(_returnMsg);
            }
            dbOrder = EntGoodsOrderBLL.SingleModel.GetModel(order.Id);
            if (dbOrder == null)
            {
                _returnMsg.Msg = "订单数据有异常";
                return Json(_returnMsg);
            }

            #endregion 读取订单数据

            if (updateOrderCol.Split(',').Contains("Address"))
            {
                updateCol.Add("Address");
                dbOrder.Address = order.Address;
            }
            if (updateOrderCol.Contains("AccepterName"))
            {
                updateCol.Add("AccepterName");
                dbOrder.AccepterName = order.AccepterName;
            }
            if (updateOrderCol.Contains("AccepterTelePhone"))
            {
                updateCol.Add("AccepterTelePhone");
                dbOrder.AccepterTelePhone = order.AccepterTelePhone;
            }
            if (updateOrderCol.Contains("BuyPrice"))
            {
                updateCol.Add("BuyPrice");
                updateCol.Add("ReducedPrice");

                if (dbOrder.OrderId > 0 && dbOrder.BuyPrice != order.BuyPrice) //微信支付,金额不等才去重新生成微信订单并关闭原有订单
                {
                    //关闭原微信订单
                    updateCol.Add("OrderId");

                    string errorMsg = string.Empty;
                    dbOrder.OrderId = new JsApiPay(HttpContext).updateWxOrderMoney(dbOrder.OrderId, order.BuyPrice, ref errorMsg);
                    if (dbOrder.OrderId <= 0 || !errorMsg.IsNullOrWhiteSpace())
                    {
                        _returnMsg.Msg = errorMsg;
                        return Json(_returnMsg);
                    }
                }
                dbOrder.ReducedPrice += dbOrder.BuyPrice - order.BuyPrice;//重新累计优惠金额
                dbOrder.BuyPrice = order.BuyPrice;
            }

            if (!updateCol.Any())
            {
                _returnMsg.Msg = "无接收到要修改的资料标识";
                return Json(_returnMsg);
            }

            bool isSuccess = EntGoodsOrderBLL.SingleModel.Update(dbOrder, string.Join(",", updateCol));
            if (isSuccess)
            {
                _returnMsg.isok = true;
                _returnMsg.Msg = "修改订单资料成功";
            }
            else
            {
                _returnMsg.Msg = "修改订单资料失败";
            }
            return Json(_returnMsg);
        }

        [LoginFilter]
        public ActionResult UpdateBargainMaterial()
        {
            string bargainUserJson = Context.GetRequest("bargainUserJson", string.Empty);
            string updateColStr = Context.GetRequest("updateCol", string.Empty);

            if (bargainUserJson.IsNullOrWhiteSpace())
            {
                _returnMsg.Msg = "服务器繁忙,未接收到订单数据";
                return Json(_returnMsg);
            }
            if (updateColStr.IsNullOrWhiteSpace())
            {
                _returnMsg.Msg = "服务器繁忙,未接收到订单处理参数";
                return Json(_returnMsg);
            }

            List<string> updateCol = new List<string>();//最终要更新的字段
            BargainUser curBargainUser = null;//要修改成的订单数据
            BargainUser dbBargainUser = null; //数据库订单

            #region 读取订单数据

            try
            {
                curBargainUser = JsonConvert.DeserializeObject<BargainUser>(bargainUserJson);    //普通订单
                if (curBargainUser == null || curBargainUser.Id <= 0)
                {
                    _returnMsg.Msg = $"订单数据出现异常";
                    return Json(_returnMsg);
                }
            }
            catch (Exception)
            {
                _returnMsg.Msg = "订单数据存在异常";
                return Json(_returnMsg);
            }
            dbBargainUser = BargainUserBLL.SingleModel.GetModel(curBargainUser.Id);
            if (dbBargainUser == null)
            {
                _returnMsg.Msg = "订单数据有异常";
                return Json(_returnMsg);
            }

            #endregion 读取订单数据

            if (updateColStr.Split(',').Contains("Address"))
            {
                updateCol.Add("Address");
                dbBargainUser.Address = curBargainUser.Address;
            }

            if (!updateCol.Any())
            {
                _returnMsg.Msg = "无接收到要修改的资料标识";
                return Json(_returnMsg);
            }

            bool isSuccess = BargainUserBLL.SingleModel.Update(dbBargainUser, string.Join(",", updateCol));
            if (isSuccess)
            {
                _returnMsg.isok = true;
                _returnMsg.Msg = "修改订单资料成功";
            }
            else
            {
                _returnMsg.Msg = "修改订单资料失败";
            }
            return Json(_returnMsg);
        }

        #endregion 订单管理

        #region 分享配置

        public ActionResult ShareSet(int? appId)
        {
            if (appId == null || appId.Value <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId.Value, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "非法请求!", code = "500" });
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={role.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = role.VersionId;
            }

            ViewBag.versionId = versionId;
            int ShareType = 0;
            int itemtypeLogo = (int)AttachmentItemType.小程序行业版分享店铺Logo;
            int itemtypeADImg = (int)AttachmentItemType.小程序行业版分享广告图;
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序专业模板:
                    ShareType = 0;
                    ViewBag.Title = "分享配置";
                    break;

                case (int)TmpType.小程序多门店模板:
                    ShareType = 2;
                    ViewBag.Title = "分享转发";
                    break;
            }
            ViewBag.PageType = xcxTemplate.Type;
            EntShare model = EntShareBLL.SingleModel.GetModel($"aid={appId} and ShareType={ShareType}");

            if (model == null)
            {
                model = new EntShare();
            }
            else
            {
                //表示更新
                
                model.Logo = C_AttachmentBLL.SingleModel.GetListByCache(model.Id, itemtypeLogo);
                model.ADImg = C_AttachmentBLL.SingleModel.GetListByCache(model.Id, itemtypeADImg);

                //店铺Logo
                List<object> LogoList = new List<object>();
                foreach (C_Attachment attachment in model.Logo)
                {
                    LogoList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.LogoList = LogoList;

                //广告图
                List<object> ADImgList = new List<object>();
                foreach (C_Attachment attachment in model.ADImg)
                {
                    ADImgList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.ADImgList = ADImgList;
            }

            ViewBag.appId = appId;

            return View(model);
        }

        [HttpPost]
        public ActionResult ShareSetting(EntShare share, int? appId, string LogoList = "", string ADImgList = "")
        {
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息异常！" });

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId.Value, dzaccount.Id.ToString());
            if (role == null)
            {
                return Json(new { isok = false, msg = "没有权限！" });
            }

            if (string.IsNullOrEmpty(share.StoreName) || share.StoreName.Length > 10)
                return Json(new { isok = false, msg = "店铺名称不能为空或者不能大于10个字符！" });

            if (!string.IsNullOrEmpty(share.ADTitle) && share.ADTitle.Length > 20)
                return Json(new { isok = false, msg = "广告语不能大于20个字符！" });

            bool result = false;
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(role.TId);
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });
            int ShareType = 0;
            int itemtypeLogo = (int)AttachmentItemType.小程序行业版分享店铺Logo;
            int itemtypeADImg = (int)AttachmentItemType.小程序行业版分享广告图;
            switch (xcxTemplate.Type)
            {
                case 22:
                    //专业版
                    ShareType = 0;
                    break;

                case 26:
                    //多门店
                    ShareType = 2;
                    break;
            }

            if (share.Id > 0)
            {
                EntShare model = EntShareBLL.SingleModel.GetModel($"aid={role.Id} and ShareType={ShareType}");
                if (model == null)
                    return Json(new { isok = false, msg = "数据不存在！" });
                share.ShareType = ShareType;
                share.Qrcode = model.Qrcode;
                if (string.IsNullOrEmpty(share.Qrcode))
                {
                    string token = "";
                    if (!XcxApiBLL.SingleModel.GetToken(role, ref token))
                    {
                        return Json(new { isok = false, msg = token });
                    }
                    share.Qrcode = CommondHelper.GetQrcode(token, "pages/index/index");
                }
                //表示修改
                result = EntShareBLL.SingleModel.Update(share);
            }
            else
            {
                if (string.IsNullOrEmpty(LogoList))
                    return Json(new { isok = false, msg = "店铺Logo不能为空！" });

                if (string.IsNullOrEmpty(ADImgList))
                    return Json(new { isok = false, msg = "广告图不能为空！" });

                string token = "";
                if (!XcxApiBLL.SingleModel.GetToken(role, ref token))
                {
                    return Json(new { isok = false, msg = token });
                }
                share.Qrcode = CommondHelper.GetQrcode(token,"pages/index/index");

                share.ShareType = ShareType;
                int id = Convert.ToInt32(EntShareBLL.SingleModel.Add(share));
                share.Id = id;
                //表示新增
                result = id > 0;
            }

            if (result)
            {

                #region Logo

                if (!string.IsNullOrWhiteSpace(LogoList))
                {
                    string[] Imgs = LogoList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Imgs.Length > 0)
                    {
                        C_AttachmentBLL.SingleModel.AddImgList(Imgs, itemtypeLogo, share.Id);
                    }
                }

                #endregion Logo

                #region 广告图

                if (!string.IsNullOrEmpty(ADImgList))
                {
                    string[] imgArray = ADImgList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (imgArray.Length > 0)
                    {
                        C_AttachmentBLL.SingleModel.AddImgList(imgArray, itemtypeADImg, share.Id);
                    }
                }

                #endregion 广告图

                return Json(new { isok = true, msg = "操作成功！", obj = share.Id });
            }

            return Json(new { isok = false, msg = "操作异常！", obj = share.Id });
        }

        #endregion 分享配置

        #region 行业版运费模板栏目操作

        [HttpGet]
        public ActionResult AddFoodFreight(int appId = 0)
        {
            #region 专业版 版本控制

            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int freightSwtich = 0;//运费开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.StoreConfig))
                {
                    StoreConfig storeConfig = JsonConvert.DeserializeObject<StoreConfig>(functionList.StoreConfig);
                    freightSwtich = storeConfig.FreightTemplate;
                }
            }

            ViewBag.freightSwtich = freightSwtich;
            ViewBag.versionId = versionId;

            #endregion 专业版 版本控制

            ViewBag.appId = appId;

            Store storeModel = StoreBLL.SingleModel.GetModel($"appid={appId}");
            StoreConfigModel config = string.IsNullOrWhiteSpace(storeModel.configJson) ? new StoreConfigModel() : JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson);

            return View(config);
        }

        [HttpGet]
        public ActionResult GetFreightTemplate(int appId = 0, int pageIndex = 1, int pageSize = 10, int type = 0, int templateId = 0)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }


            List<EntFreightTemplate> freightTemplates = EntFreightTemplateBLL.SingleModel.GetListByAppId(appId: app.Id, pageIndex: pageIndex, pageSize: pageSize, type: type, templateId: templateId);

            return Json(new { isok = true, msg = "获取成功", data = EntFreightTemplateBLL.SingleModel.ConvertApiModel(freightTemplates) }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加运费模板
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFoodFreight(EntFreightTemplate freightTmplate, int appId = 0)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" });
            }

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" });
            }

            if (string.IsNullOrEmpty(freightTmplate.Name.Trim()))
            {
                return Json(new { isok = false, msg = "模板名称不可为空！" });
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" });

            freightTmplate.aId = appId;


            //添加
            if (freightTmplate.Id == 0)
            {
                #region 专业版 版本控制

                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    int industr = app.VersionId;
                    FunctionList functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                    if (functionList == null)
                    {
                        return Json(new { isok = false, msg = "此功能未开启" });
                    }

                    StoreConfig storeConfig = new StoreConfig();
                    if (!string.IsNullOrEmpty(functionList.ProductMgr))
                    {
                        storeConfig = JsonConvert.DeserializeObject<StoreConfig>(functionList.StoreConfig);
                    }
                    if (storeConfig.FreightTemplate == 1)//表示关闭了添加运费模板功能
                    {
                        return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                    }
                }

                #endregion 专业版 版本控制

                EntFreightTemplate newTemplate = new EntFreightTemplate()
                {
                    Name = freightTmplate.Name,
                    BaseCount = freightTmplate.BaseCount,
                    BaseCost = freightTmplate.BaseCost,
                    ExtraCost = freightTmplate.ExtraCost,
                    FullDiscount = freightTmplate.FullDiscount,
                    AreaCode = freightTmplate.AreaCode,
                    aId = appId,
                    CreateTime = DateTime.Now,
                    /*IsDefault = freightTmplate.IsDefault,*/
                };
                object result = EntFreightTemplateBLL.SingleModel.Add(newTemplate);
                if (int.Parse(result.ToString()) > 0)
                {
                    return Json(new { isok = true, msg = result.ToString() });
                }
            }
            //编辑
            else
            {
                EntFreightTemplate updateModel = EntFreightTemplateBLL.SingleModel.GetModel(freightTmplate.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到模板" });
                }
                else
                {
                    //updateModel.IsDefault = freightTmplate.IsDefault;
                    updateModel.Name = freightTmplate.Name;
                    updateModel.BaseCost = freightTmplate.BaseCost;
                    updateModel.ExtraCost = freightTmplate.ExtraCost;
                    updateModel.BaseCount = freightTmplate.BaseCount;
                    updateModel.FullDiscount = freightTmplate.FullDiscount;
                    updateModel.AreaCode = freightTmplate.AreaCode;
                    EntFreightTemplateBLL.SingleModel.Update(updateModel);
                }
                return Json(new { isok = true, msg = "修改成功" });
            }

            return Json(new { isok = false, msg = "系统错误！" });
        }

        /// <summary>
        /// 设置运费模板
        /// </summary>
        /// <param name="templateId">选中运费模板ID</param>
        /// <param name="onlyFreight">设置统一运费</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SelectFreightTemplate(int appId = 0, int templateId = 0, int onlyFreight = 0)
        {
            if (appId <= 0 || (templateId <= 0 && onlyFreight <= 0))
            {
                return Json(new { isok = false, msg = "参数错误" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" });
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" });
            }

            bool result = false;
            //获取配置
            Store storeModel = StoreBLL.SingleModel.GetModel($"appid={app.Id}");
            StoreConfigModel config = JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson);
            //选用模板
            if (templateId > 0)
            {
                EntFreightTemplate template = EntFreightTemplateBLL.SingleModel.GetModel(templateId);
                if (template?.aId != appId)
                {
                    return Json(new { isok = false, msg = "无效模板" });
                }
                //保存选中模板
                config.FreightTemplateId = config.FreightTemplateId == template.Id ? 0 : template.Id;
                //情况运费
                config.FreightPriceSwitch = false;
                storeModel.configJson = JsonConvert.SerializeObject(config);
                result = StoreBLL.SingleModel.Update(storeModel, "configJson");
            }
            //选用统一运费
            //else if(onlyFreight > 0)
            //{
            //    //清空选中模板
            //    config.FreightTemplateId = 0;
            //    //保存运费
            //    config.FreightPrice = onlyFreight;
            //    storeModel.configJson = JsonConvert.SerializeObject(config);
            //    result = _storeBLL.Update(storeModel, "configJson");
            //}

            return Json(result ? new { isok = true, msg = "操作成功" } : new { isok = false, msg = "操作失败" });
        }

        /// <summary>
        /// 设置统一运费
        /// </summary>
        /// <param name="templateId">选中运费模板ID</param>
        /// <param name="onlyFreight">设置统一运费</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateFreightConfig(int appId = 0, int freight = 0, bool isEnable = false)
        {
            if (appId <= 0 || freight < 0)
            {
                return Json(new { isok = false, msg = "参数错误" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" });
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" });
            }

            bool result = false;
            //获取配置
            Store storeModel = StoreBLL.SingleModel.GetModel($"appid={app.Id}");
            StoreConfigModel config = JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson);
            //选用模板
            config.FreightPrice = freight;
            config.FreightPriceSwitch = isEnable;
            if (config.FreightPriceSwitch)
            {
                config.FreightTemplateId = 0;
            }
            storeModel.configJson = JsonConvert.SerializeObject(config);
            result = StoreBLL.SingleModel.Update(storeModel, "configJson");
            return Json(result ? new { isok = true, msg = "操作成功" } : new { isok = false, msg = "操作失败" });
        }

        /// <summary>
        /// 删除运费模板
        /// </summary>
        /// <param name="id"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult delFoodFreightTemplate(EntFreightTemplate freightTmplate)
        {
            if (freightTmplate.aId <= 0)
            {
                return Json(new { isok = false, msg = "非法参数_aid" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" });
            }

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(freightTmplate.aId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" });
            }


            EntFreightTemplate model = EntFreightTemplateBLL.SingleModel.GetModel(freightTmplate.Id) ?? new EntFreightTemplate();
            if (model.IsDefault == 1)
            {
                return Json(new { isok = true, msg = "默认模板不允许删除！" });
            }

            model.state = -1;
            if (EntFreightTemplateBLL.SingleModel.Update(model, "State"))
            {
                //若无其他运费模板,则添加一个默认的。默认的不允许删除只能编辑
                int freightCount = EntFreightTemplateBLL.SingleModel.GetCount($" aid={model.aId} and state >= 0");
                if (freightCount <= 0)//若商家无运费模板添加一个默认免费的,以便解决现阶段无运费模板无法下单的问题
                {
                    EntFreightTemplateBLL.SingleModel.Add(new EntFreightTemplate() { BaseCount = 999, aId = model.aId, Name = "免运费", BaseCost = 0, ExtraCost = 0, IsDefault = 1, CreateTime = DateTime.Now });
                }
                return Json(new { isok = true, msg = "删除成功！" });
            }
            else
            {
                return Json(new { isok = false, msg = "删除失败！" });
            }
        }

        /// <summary>
        /// 运费模板详情
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        public ActionResult getFoodFreight(int Id = 0)
        {
            return Json(EntFreightTemplateBLL.SingleModel.GetModel(Id), JsonRequestBehavior.AllowGet);
        }

        #endregion 行业版运费模板栏目操作

        /// <summary>
        /// 权限表Id 以及小程序模板类型PageType
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="PageType"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult Bargain(int appId = 0, int PageType = 22, int pageIndex = 1, int pageSize = 10, int actionType = 0)
        {
            #region 专业版 版本控制

            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int bargainSwtich = 0;//砍价开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }

                if (!string.IsNullOrEmpty(functionList.ComsConfig))
                {
                    ComsConfig comsConfig = JsonConvert.DeserializeObject<ComsConfig>(functionList.ComsConfig);
                    bargainSwtich = comsConfig.Cutprice;
                }
            }
            ViewBag.versionId = versionId;
            ViewBag.bargainSwtich = bargainSwtich;

            #endregion 专业版 版本控制

            string strWhere = $"StoreId={appId} and BargainType=1 and IsDel<>-1";

            string searchType = Utility.IO.Context.GetRequest("searchType", string.Empty);
            string BName = string.Empty;
            string StartTime = string.Empty;
            string EndTime = string.Empty;
            int State = 0;
            if (!string.IsNullOrEmpty(searchType))
            {
                BName = Utility.IO.Context.GetRequest("BName", string.Empty);
                BName = Server.UrlDecode(BName);
                BName = Utility.EncodeHelper.ReplaceSqlKey(BName);
                StartTime = Utility.IO.Context.GetRequest("StartTime", string.Empty);
                EndTime = Utility.IO.Context.GetRequest("EndTime", string.Empty);
                State = Utility.IO.Context.GetRequestInt("State", 0);
                if (!string.IsNullOrEmpty(BName))
                {
                    strWhere += $" and BName like '%{BName}%'";
                }
                if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(EndTime))
                {
                    StartTime = Convert.ToDateTime(StartTime).ToString("yyyy-MM-dd HH:mm:ss");
                    EndTime = Convert.ToDateTime(EndTime).ToString("yyyy-MM-dd HH:mm:ss");
                    strWhere += $" and CreateDate between '{StartTime}' and '{EndTime}'";
                }
                if (State > 0)
                {
                    if (State == 1)
                    {
                        //进行中
                        strWhere += $" and (StartDate<'{DateTime.Now.ToString()}' and EndDate>'{DateTime.Now.ToString()}' and RemainNum>0 and IsEnd=0)";
                    }
                    if (State == 2)
                    {
                        //已经下架
                        strWhere += $" and State=-1";
                    }
                    if (State == 3)
                    {
                        //未开始
                        strWhere += $" and StartDate>'{DateTime.Now.ToString()}'";
                    }

                    if (State == 4)
                    {
                        //已过期
                        strWhere += $" and  (EndDate<'{DateTime.Now.ToString()}' or RemainNum=0 or IsEnd=2)";
                    }
                }
            }

            if (actionType > 0)
            {
                strWhere += $" and State<>-1";
            }

            string orderWhere = Utility.EncodeHelper.ReplaceSqlKey(Utility.IO.Context.GetRequest("orderWhere", string.Empty));
            if (string.IsNullOrEmpty(orderWhere))
                orderWhere = "ID desc";

            List<Bargain> list = BargainBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", orderWhere);
            ViewBag.pageSize = pageSize;
            int TotalCount = BargainBLL.SingleModel.GetCount(strWhere);
            ViewBag.TotalCount = TotalCount;
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;

            ViewBag.BName = BName;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.State = State;

            if (actionType > 0)
            {
                ViewModel<Bargain> vm = new ViewModel<Bargain>();
                vm.TotalCount = TotalCount;
                vm.PageCount = Utility.Paging.PageInfo.GetPageCount(TotalCount, pageSize);
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.PageType = PageType;
                vm.DataList = list;

                return Content(JsonConvert.SerializeObject(vm, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
            }

            return View(list == null ? new List<Bargain>() : list);
        }

        /// <summary>
        /// 订单列表
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderTotalList()
        {
            result = new Return_Msg();
            int appId = Context.GetRequestInt("appId", 0);
            int tab = Context.GetRequestInt("tab", 1);  //选中的标签页为哪一页
            
            if (appId == 0)
            {
                result.Msg = "参数错误";
                result.code = "500";
                return View("PageError", result);
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                result.Msg = "没有权限";
                result.code = "500";
                return View("PageError", result);
            }
            ViewBag.appId = appId;
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcx.VersionId;
            }

            ViewBag.versionId = versionId;

            List<VipLevel> levelList = VipLevelBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            ViewBag.tab = tab;

            if (levelList == null) levelList = new List<VipLevel>();

            Store store = StoreBLL.SingleModel.GetModelByRid(xcx.Id);
            if (store != null && !string.IsNullOrEmpty(store.configJson))
            {
                store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
                ViewBag.returnAddress = store.funJoinModel.returnAddress;
            }

            ViewBag.flashDealId = Context.GetRequestInt("flashDealId", 0);  //秒杀活动ID（搜索所属活动订单）
            return View(levelList);
        }

        /// <summary>
        /// 获取砍价订单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="PageType"></param>
        /// <param name="Id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="export"></param>
        /// <returns></returns>
        public JsonResult GetBargainUserList(int appId = 0, int pageIndex = 1, int pageSize = 10)
        {
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            }
            List<int> listBId = new List<int>();

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(BargainBLL.SingleModel.connName, CommandType.Text, $"select Id from Bargain where StoreId={appId}", null))
            {
                while (dr.Read())
                {
                    if (dr["Id"] != DBNull.Value)
                    {
                        listBId.Add(Convert.ToInt32(dr["Id"]));
                    }
                }
            }

            if (listBId.Count <= 0)
                return Json(new { isok = true, msg = "成功", model = new { recordCount = 0, bargainUserList = new List<BargainUser>() } }, JsonRequestBehavior.AllowGet);

            string strWhere = GetBargainOrderStrWhere(listBId);

            List<BargainUser> list = BargainUserBLL.SingleModel.GetJoinList(strWhere, pageSize, pageIndex, "CreateDate desc");
            if (list == null)
                list = new List<BargainUser>();

            list.ForEach(b =>
            {
                b.BuyTimeStr = b.BuyTime.ToString("yyyy-MM-dd HH:mm:ss");
            });

            ViewBag.appId = appId;

            int TotalCount = BargainUserBLL.SingleModel.GetJoinCount(strWhere);
            return Json(new { isok = true, msg = "成功", model = new { bargainRecordCount = TotalCount, bargainUserList = list } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取普通订单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderNum"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="goodsName"></param>
        /// <param name="accName"></param>
        /// <param name="accPhone"></param>
        /// <param name="orderState"></param>
        /// <param name="export"></param>
        /// <returns></returns>
        public JsonResult GetNormalOrderList(int appId = 0, string orderNum = "", int pageIndex = 1, int pageSize = 10, string startdate = "", string enddate = "", string goodsName = "", string accName = "", string accPhone = "", int orderState = -999, bool export = false, string tablesNo = "", int getWay = -1, int ordertype = 0, int flashDealId = 0, string qrCodeName = null)
        {
            if (appId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);

            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息过期!" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权!" }, JsonRequestBehavior.AllowGet);

            ViewBag.appId = appId;
            DateTime? s = null;
            if (!startdate.IsNullOrWhiteSpace())
            {
                s = Convert.ToDateTime(startdate + " 00:00:00 ");
            }

            DateTime? e = null;
            if (!enddate.IsNullOrWhiteSpace())
            {
                e = Convert.ToDateTime(enddate + " 23:59:59 ");
            }
            int qrCodeId = 0;
            if(!string.IsNullOrWhiteSpace(qrCodeName))
            {
                EntStoreCode storeQrCode = EntStoreCodeBLL.SingleModel.GetByName(qrCodeName, xcx.Id);
                qrCodeId = storeQrCode != null ? storeQrCode.Id : 0;
            }

            int totalCount = 0;
            //获得订单记录
            List<EntGoodsOrder> listEntGoodsOrder = EntGoodsOrderBLL.SingleModel.getModelByWhere(appId, pageSize, pageIndex, out totalCount, goodsName, accName, accPhone, orderState, s, e, orderNum, tablesNo, getWay, 0, ordertype, flashDealId: flashDealId, qrCodeId: qrCodeId);
            if (listEntGoodsOrder == null)
                listEntGoodsOrder = new List<EntGoodsOrder>();

            EntFreightTemplate curFreightTemplate = null;
            listEntGoodsOrder.ForEach(order =>
            {
                if (order.FreightTemplateId > 0)
                {
                    if (order.GetWay == (int)miniAppOrderGetWay.商家配送)
                    {
                        curFreightTemplate = EntFreightTemplateBLL.SingleModel.GetModel(order.FreightTemplateId);
                        if (curFreightTemplate != null)
                        {
                            order.FreightTemplateName = $" (选用运费模板:{curFreightTemplate.Name}) ";
                        }
                    }
                }
            });

            //判断是否有团订单
            EntGroupSponsorBLL.SingleModel.GetSponsorState(ref listEntGoodsOrder);

            //判断是否有达达订单
            //_dadaOrderBLL.GetDadaOrderState(ref listEntGoodsOrder, appId);

            //获取店铺二维码名称
            EntStoreCodeBLL.SingleModel.GetStoreCodeName<EntGoodsOrder>(ref listEntGoodsOrder);

            return Json(new { isok = true, msg = "成功", model = new { orderRecordCount = totalCount, listEntGoodsOrder = listEntGoodsOrder } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取查询砍价订单的条件
        /// </summary>
        /// <param name="listBId"></param>
        /// <returns></returns>
        public string GetBargainOrderStrWhere(List<int> listBId)
        {
            string strWhere = $" a.OrderId is not null and a.OrderId!='' and BId in({string.Join(",", listBId)})";
            string orderId = Utility.IO.Context.GetRequest("orderId", string.Empty);
            string bName = Utility.IO.Context.GetRequest("bName", string.Empty);
            string shopName = Utility.IO.Context.GetRequest("shopName", string.Empty);
            string addressUserName = Utility.IO.Context.GetRequest("addressUserName", string.Empty);
            string addressTelNumber = Utility.IO.Context.GetRequest("addressTelNumber", string.Empty);
            string startDate = Utility.IO.Context.GetRequest("startDate", string.Empty);
            string endDate = Utility.IO.Context.GetRequest("endDate", string.Empty);
            int orderState = Utility.IO.Context.GetRequestInt("orderState", -2);
            int levelid = Utility.IO.Context.GetRequestInt("levelid", 0);

            orderId = Utility.EncodeHelper.ReplaceSqlKey(orderId);
            if (!string.IsNullOrEmpty(orderId))
                strWhere += $" and a.orderId like '%{orderId}%'";

            bName = Utility.EncodeHelper.ReplaceSqlKey(bName);
            if (!string.IsNullOrEmpty(bName))
                strWhere += $" and a.BName like '%{bName}%'";

            shopName = Utility.EncodeHelper.ReplaceSqlKey(shopName);
            if (!string.IsNullOrEmpty(shopName))
                strWhere += $" and u.NickName like '%{shopName}%'";

            addressUserName = Utility.EncodeHelper.ReplaceSqlKey(addressUserName);
            if (!string.IsNullOrEmpty(addressUserName))
                strWhere += $" and a.Address like '%{addressUserName}%'";

            addressTelNumber = Utility.EncodeHelper.ReplaceSqlKey(addressTelNumber);
            if (!string.IsNullOrEmpty(addressTelNumber))
                strWhere += $" and a.Address like '%{addressTelNumber}%'";

            if (!string.IsNullOrEmpty(Utility.EncodeHelper.ReplaceSqlKey(startDate)) && !string.IsNullOrEmpty(Utility.EncodeHelper.ReplaceSqlKey(endDate)))
            {
                startDate = Convert.ToDateTime(startDate + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                endDate = Convert.ToDateTime(endDate + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss");
                strWhere += $" and CreateOrderTime between '{startDate}' and '{endDate}'";
            }

            int tempState = orderState;
            if (tempState == -1000)
            {
                orderState = 8;
                strWhere += $" and c.Id is NULL ";
            }

            if (orderState == 7 || orderState == 5 || orderState == 8 || orderState == -1 || orderState == 6)
            {
                strWhere += $" and a.State={orderState}";
            }
            else if (orderState == 2)
            {//退款
                strWhere += $" and (a.State=2 or a.State=3 or a.State=4)";
            }

            if (levelid > 0)
            {
                strWhere += $" and levelid={levelid}";
            }
            return strWhere;
        }

        /// <summary>
        /// 导出砍价商品订单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public void ExportBargainOrderExcel(int appId = 0, int pageIndex = 1, int pageSize = 20)
        {
            if (appId <= 0)
            {
                Response.Write("参数错误");
                return;
            }
            List<int> listBId = new List<int>();

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(BargainBLL.SingleModel.connName, CommandType.Text, $"select Id from Bargain where StoreId={appId}", null))
            {
                while (dr.Read())
                {
                    if (dr["Id"] != DBNull.Value)
                    {
                        listBId.Add(Convert.ToInt32(dr["Id"]));
                    }
                }
            }

            if (listBId.Count <= 0)
            {
                Response.Write("没有数据");
                return;
            }

            DataTable table = new DataTable();
            table.Columns.AddRange(new[]
            {
                        new DataColumn("订单号"),
                        new DataColumn("商品名称"),
                        new DataColumn("会员名称"),
                        new DataColumn("会员级别"),
                         new DataColumn("订单金额(元)"),
                        new DataColumn("支付方式"),
                        new DataColumn("配送地址"),
                        new DataColumn("配送方式"),
                         new DataColumn("快递单号"),
                        new DataColumn("收货人"),
                        new DataColumn("收货电话"),
                        new DataColumn("下单时间"),
                          new DataColumn("支付时间"),
                        new DataColumn("订单状态"),
                          new DataColumn("留言备注")
            });
            int export = Utility.IO.Context.GetRequestInt("export", 0);
            string strWhere = GetBargainOrderStrWhere(listBId);
            List<BargainUser> list = BargainUserBLL.SingleModel.GetJoinList(strWhere, export > 0 ? 10000000 : pageSize, pageIndex, "CreateDate desc", export);

            if (list != null && list.Count > 0)
            {
                foreach (BargainUser item in list)
                {
                    DataRow row = table.NewRow();
                    row["订单号"] = item.OrderId;//item.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
                    row["商品名称"] = item.BName;
                    row["会员名称"] = item.ShopName;
                    row["会员级别"] = item.VipLeve;
                    row["订单金额(元)"] = item.CurrentPriceStr;
                    row["支付方式"] = item.PayTypeStr;
                    row["配送地址"] = item.AddressDetail;
                    row["配送方式"] = item.SendGoodsName;
                    row["快递单号"] = item.WayBillNo;
                    row["收货人"] = item.AddressUserName;
                    row["收货电话"] = item.TelNumber;
                    row["下单时间"] = item.CreateOrderTimeStr;
                    row["支付时间"] = item.BuyTimeStr;
                    row["订单状态"] = item.StateStr;
                    row["留言备注"] = item.Remark;
                    table.Rows.Add(row);
                }
            }
            if (table.Rows.Count <= 0)
            {
                table.Rows.Add(table.NewRow());
            }

            ExcelHelper<C_BargainUser>.Out2Excel(table, "砍价商品订单"); //导出
        }

        /// <summary>
        /// 导出普通商品订单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderNum"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="goodsName"></param>
        /// <param name="accName"></param>
        /// <param name="accPhone"></param>
        /// <param name="orderState"></param>
        /// <param name="export"></param>
        /// <param name="tablesNo"></param>
        /// <param name="getWay"></param>
        public void ExportNormalOrder(int appId = 0, string orderNum = "", int pageIndex = 1, int pageSize = 10, string startdate = "", string enddate = "", string goodsName = "", string accName = "", string accPhone = "", int orderState = -999, string tablesNo = "", int getWay = -1, int ordertype = 0)
        {
            if (appId <= 0)
            {
                Response.Write("参数错误");
                return;
            }

            if (dzaccount == null)
            {
                Response.Write("登录信息过期");
                return;
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                Response.Write("小程序未授权");
                return;
            }

            DateTime? s = null;
            if (startdate != null && !string.IsNullOrEmpty(startdate))
            {
                s = Convert.ToDateTime(startdate + " 00:00:00 ");
            }

            DateTime? e = null;
            if (enddate != null && !string.IsNullOrEmpty(enddate))
            {
                e = Convert.ToDateTime(enddate + " 23:59:59 ");
            }
            int export = Utility.IO.Context.GetRequestInt("export", 0);
            int totalCount = 0;

            DataTable table = new DataTable();
            table.Columns.AddRange(new[]
            {
                new DataColumn("订单号"),
                new DataColumn("提货码"),
                new DataColumn("订单金额(元)"),
                new DataColumn("实收金额(元)"),
                new DataColumn("支付方式"),
                new DataColumn("配送地址"),
                new DataColumn("配送方式"),
                new DataColumn("提/收货人"),
                new DataColumn("提/收货电话"),
                new DataColumn("订单备注"),
                new DataColumn("下单时间"),
                new DataColumn("团号"),
                new DataColumn("订单状态"),
                new DataColumn("商品数量"),
                new DataColumn("订单详情"),
                new DataColumn("二维码名称"),
            });

            if (ordertype != 1)//如果非拼团数据,不需要团号
            {
                table.Columns.Remove("团号");
            }
            //获得订单记录
            List<EntGoodsOrder> list = EntGoodsOrderBLL.SingleModel.getModelByWhere(appId, export > 0 ? 10000000 : pageSize, pageIndex, out totalCount, goodsName, accName, accPhone, orderState, s, e, orderNum, tablesNo, getWay, export, ordertype);
            if (list != null && list.Count > 0)
            {
                //获取店铺二维码名称
                EntStoreCodeBLL.SingleModel.GetStoreCodeName<EntGoodsOrder>(ref list);

                #region 拼接导出订单内容

                StringBuilder sb = new StringBuilder();
                foreach (EntGoodsOrder item in list)
                {
                    DataRow row = table.NewRow();
                    row["订单号"] = item.OrderNum;
                    row["提货码"] = item.TablesNo;
                    row["订单金额(元)"] = item.GoodsMoney;
                    row["实收金额(元)"] = item.BuyPriceStr;
                    row["支付方式"] = item.BuyModeStr;
                    row["配送地址"] = item.Address;
                    row["配送方式"] = item.GetWayStr;
                    row["提/收货人"] = item.AccepterName;
                    row["提/收货电话"] = item.AccepterTelePhone;
                    row["订单备注"] = item.Message;
                    row["下单时间"] = item.CreateDateStr;
                    if (ordertype == 1)//拼团数据才有该列
                    {
                        row["团号"] = item.GroupId;
                    }
                    row["订单状态"] = item.StateStr;
                    row["商品数量"] = item.QtyCount;
                    List<EntGoodsCart> cartModelList = EntGoodsCartBLL.SingleModel.GetOrderDetail(item.Id);
                    foreach (EntGoodsCart x in cartModelList)
                    {
                        sb.AppendLine($"{x.goodsMsg.name}\r\n规格:{x.SpecInfo}\r\n{(x.originalPrice * 0.01).ToString("0.00")}元 ×{x.Count}\r\n原金额:{((x.originalPrice * x.Count) * 0.01).ToString("0.00")}元\r\n实际金额:{((x.Price * x.Count) * 0.01).ToString("0.00")}元\r\n");
                    }
                    sb.AppendLine($"运费:{(item.FreightPrice * 0.01).ToString("0.00")}元");

                    row["订单详情"] = sb.ToString();
                    row["二维码名称"] = item.StoreCodeName;
                    sb.Clear();
                    table.Rows.Add(row);
                }

                #endregion 拼接导出订单内容
            }
            //为空表格就给出默认行,以显示列名
            if (table.Rows.Count <= 0)
            {
                table.Rows.Add(table.NewRow());
            }
            ExcelHelper<C_BargainUser>.Out2Excel(table, ordertype == 0 ? "普通商品订单" : "拼团商品订单"); //导出
        }

        /// <summary>
        /// 选择拼团
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="state">0所有，1进行中，2结束</param>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetGroupListPage()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            if (appId <= 0)
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            //查询数量
            int length = Utility.IO.Context.GetRequestInt("length", 10);

            Store store = StoreBLL.SingleModel.GetModelByRid(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "还未开通店铺" }, JsonRequestBehavior.AllowGet);
            }
            //获取可以参加的拼团数据
            ViewModel<Groups> vm = new ViewModel<Groups>();
            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string sql = $"state=1 and StoreId={store.Id} and ValidDateStart<'{nowtime}' and ValidDateEnd>'{nowtime}'";
            List<Groups> grouplist = GroupsBLL.SingleModel.GetList(sql, pageSize, pageIndex, "*", "Id Desc");
            int _totalcount = GroupsBLL.SingleModel.GetCount(sql);

            if (grouplist?.Count > 0)
            {
                grouplist.ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.ImgUrl) && p.ImgUrl.IndexOf('@') == -1)
                    {
                        p.ImgUrl = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(p.ImgUrl, 640, 360);
                    }
                    //已团数量
                    p.GroupsNum = GroupUserBLL.SingleModel.GetCountPayGroup(p.Id);

                    //判断是否已结束
                    if ((p.ValidDateEnd < DateTime.Now || p.RemainNum <= 0))
                    {
                        p.State = 2;
                        p.StateStr = "已结束";
                    }
                    else if ((p.ValidDateStart < DateTime.Now && p.RemainNum > 0))
                    {
                        //判断是否开始
                        p.State = 1;
                        p.StateStr = "已开始";
                    }
                    else
                    {
                        p.State = -1;
                        p.StateStr = "未开始";
                    }
                });
            }
            vm.DataList = grouplist;
            vm.TotalCount = _totalcount;
            vm.PageCount = Utility.Paging.PageInfo.GetPageCount(_totalcount, pageSize);
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            return Content(JsonConvert.SerializeObject(vm, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
        }

        /// <summary>
        /// 选择专业版拼团
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="state">0所有，1进行中，2结束</param>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetEntGroupListPage()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            if (appId <= 0)
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            //查询数量
            //int length = Utility.IO.Context.GetRequestInt("length", 10);

            Store store = StoreBLL.SingleModel.GetModelByRid(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "还未开通店铺" }, JsonRequestBehavior.AllowGet);
            }
            //获取可以参加的拼团数据

            List<EntGroupsRelation> list = new List<EntGroupsRelation>();
            int totalcount = 0;
            list = EntGroupsRelationBLL.SingleModel.GetListGroups(appId, pageSize, pageIndex, ref totalcount);

            ViewModel<EntGroupsRelation> vm = new ViewModel<EntGroupsRelation>();

            vm.DataList = list;
            vm.TotalCount = totalcount;
            vm.PageCount = Utility.Paging.PageInfo.GetPageCount(totalcount, pageSize);
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;

            return Content(JsonConvert.SerializeObject(vm, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
        }

        /// <summary>
        /// 分销管理 一级分销
        /// </summary>
        /// <returns></returns>
        public ActionResult salesman()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);  //选中的标签页为哪一页
            int tab = Utility.IO.Context.GetRequestInt("tab", 1);  //选中的标签页为哪一页
            ViewBag.tab = tab;
            ViewBag.appId = appId;
            return View();
        }

        public ActionResult CopyTemplate()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid error" });
            }
            XcxAppAccountRelation relation = XcxAppAccountRelationBLL.SingleModel.GetModelById(appId);
            if (relation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation null" });
            }
            if (relation.TId != 37)
            {
                return Json(new { isok = false, msg = "系统繁忙template error" });
            }
            EntSetting templateSetting = EntSettingBLL.SingleModel.GetModel(WebSiteConfig.TemplateAid);
            // EntPage pages = JsonConvert.DeserializeObject<EntPage>(templateSetting.pages);
            return Json(new { isok = true, data = templateSetting.pages });
        }

        #region 功能管理

        [LoginFilter]
        public ActionResult FunManager()
        {
            int aId = Context.GetRequestInt("appId", 0);
            if (aId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(aId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int IMSwtich = 0;//私信功能开关
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
                if (!string.IsNullOrEmpty(functionList.FuncMgr))
                {
                    FuncMgr funcMgr = JsonConvert.DeserializeObject<FuncMgr>(functionList.FuncMgr);
                    IMSwtich = funcMgr.IM;
                }
            }

            ViewBag.IMSwtich = IMSwtich;
            ViewBag.versionId = versionId;
            Store storeModel = StoreBLL.SingleModel.GetModelByAId(aId);
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "店铺信息错误!", code = "500" });
            }

            //json转换可能报错,加try catch
            try
            {
                storeModel.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson) ?? new StoreConfigModel();//若为 null 则new一个新的配置
            }
            catch
            {
                storeModel.funJoinModel = new StoreConfigModel();
            }
            storeModel.kfInfo = C_UserInfoBLL.SingleModel.GetKfInfo(xcx.AppId);

            return View(storeModel);
        }

        public ActionResult SearchUserInfo()
        {
            int aId = Context.GetRequestInt("appId", 0);
            XcxAppAccountRelation xcxRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aId);
            if (xcxRelation == null)
            {
                return Json(new { isok = false, msg = "小程序不存在" });
            }
            Store storeModel = StoreBLL.SingleModel.GetModelByAId(aId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "店铺信息不存在" });
            }
            string nickName = Context.GetRequest("nickName", string.Empty);
            if (string.IsNullOrEmpty(nickName))
            {
                return Json(new { isok = false, msg = "请输入会员昵称" });
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByNickName(xcxRelation.AppId, nickName);
            return Json(new { isok = true, data = userInfo });
        }

        #region 店铺二维码

        public ActionResult ScanCode()
        {
            int appId = Context.GetRequestInt("appId", 0);
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

            int storeCodeSwtich = 0;//扫码购物功能开关
            int PageType = 22;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                PageType = xcxTemplate.Type;
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.FuncMgr))
                {
                    FuncMgr funcMgr = JsonConvert.DeserializeObject<FuncMgr>(functionList.FuncMgr);

                    storeCodeSwtich = funcMgr.StoreCode;
                }
            }

            ViewBag.storeCodeSwtich = storeCodeSwtich;
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;

            return View();
        }

        public ActionResult GetStoreCodeList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (umodel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            Return_Msg data = new Return_Msg();
            RedisModel<EntStoreCode> list = EntStoreCodeBLL.SingleModel.GetListByAid(appId, 0, pageSize, pageIndex);
            data.dataObj = list;
            data.isok = true;
            return Json(data);
        }

        /// <summary>
        /// 添加二维码
        /// </summary>
        [HttpPost]
        public ActionResult AddOrEditStoreCode()
        {
            Return_Msg data = new Return_Msg();
            int appId = Context.GetRequestInt("appId", 0);
            string name = Context.GetRequest("name", string.Empty);
            int codewidth = Context.GetRequestInt("codewidth", 200);
            int id = Context.GetRequestInt("id", 0);
            if (dzaccount == null)
            {
                data.isok = false;
                data.Msg = "登录信息超时";
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                data.isok = false;
                data.Msg = "小程序未授权";
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                data.isok = false;
                data.Msg = "找不到模板";
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if ((int)TmpType.小程序专业模板 == xcxTemplate.Type)
            {
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    data.isok = false;
                    data.Msg = "此功能未开启";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                FuncMgr funcMgr = new FuncMgr();

                if (!string.IsNullOrEmpty(functionList.FuncMgr))
                {
                    funcMgr = JsonConvert.DeserializeObject<FuncMgr>(functionList.FuncMgr);
                }
                if (funcMgr.StoreCode == 1)
                {
                    data.isok = false;
                    data.Msg = "请升级到更高版本才能开启此功能";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            //清除缓存
            EntStoreCodeBLL.SingleModel.RemoveEntStoreCodeListCache(appId);

            //删除
            if (id > 0)
            {
                EntStoreCode model = EntStoreCodeBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    data.Msg = "该二维码不存在，请刷新重试";
                    return Json(data);
                }
                model.State = -1;
                model.UpdateTime = DateTime.Now;
                data.isok = EntStoreCodeBLL.SingleModel.Update(model, "state,updatetime");
                return Json(data);
            }

            if (string.IsNullOrEmpty(name))
            {
                data.Msg = "二维码名称不能为空";
                return Json(data);
            }
            if (EntStoreCodeBLL.SingleModel.ExistData(appId, name))
            {
                data.Msg = "已存在相同的二维码";
                return Json(data);
            }

            EntStoreCode code = new EntStoreCode();
            code.Scene = name;
            code.AId = appId;
            code.AddTime = DateTime.Now;
            code.UpdateTime = DateTime.Now;
            code.Id = Convert.ToInt32(EntStoreCodeBLL.SingleModel.Add(code));

            ConfigViewModel viewmodel = new ConfigViewModel();
            viewmodel.XcxTemplate = xcxTemplate;

            //小程序二维码
            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcx, ref token))
            {
                data.Msg = token;
                return Json(data);
            }

            
            qrcodeclass result = CommondHelper.GetMiniAppQrcode(token, "", code.Id.ToString(), codewidth);

            if (result != null)
            {
                if (result.isok > 0)
                {
                    code.ImgUrl = result.url;
                }
                else
                {
                    code.ImgUrl = result.msg;
                }

                EntStoreCodeBLL.SingleModel.Update(code, "imgurl");
            }

            if (code.Id > 0)
            {
                data.Msg = "添加成功";
                data.isok = true;
                data.dataObj = code;
            }
            else
            {
                data.Msg = "添加失败";
            }

            return Json(data);
        }

        #endregion 店铺二维码

        public ActionResult Reservation(int appId = 0)
        {
            XcxAppAccountRelation xcxRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (xcxRelation == null)
            {
                return Json(new { isok = false, msg = "小程序不存在" });
            }
            ViewBag.appId = xcxRelation.Id;

            Store storeModel = StoreBLL.SingleModel.GetModelByAId(xcxRelation.Id);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "店铺信息不存在" });
            }

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            return View(JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson));
        }

        [LoginFilter]
        public ActionResult GetGoodsType(int appId = 0)
        {
            Return_Msg apiResult = new Return_Msg();
            XcxAppAccountRelation relation = XcxAppAccountRelationBLL.SingleModel.GetModelById(appId);
            if (relation == null)
            {
                apiResult.Msg = "系统繁忙relation null";
                return Json(apiResult);
            }

            Store storeModel = StoreBLL.SingleModel.GetModelByAId(relation.Id);
            if (storeModel == null)
            {
                apiResult.Msg = "店铺信息错误";
                return Json(apiResult);
            }

            int count = 0;
            List<EntGoodType> goodType = EntGoodTypeBLL.SingleModel.GetListByCach(relation.Id, 50, 1, ref count);
            apiResult = new Return_Msg { isok = true, dataObj = new { list = goodType, count = count } };
            return Json(apiResult);
        }

        [LoginFilter]
        public ActionResult UpdateReserveConfig(int appId = 0, string goodTypes = null, bool isEnable = false)
        {
            Return_Msg apiResult = new Return_Msg();
            XcxAppAccountRelation relation = XcxAppAccountRelationBLL.SingleModel.GetModelById(appId);
            if (relation == null)
            {
                apiResult.Msg = "系统繁忙relation null";
                return Json(apiResult);
            }

            Store storeModel = StoreBLL.SingleModel.GetModelByAId(relation.Id);
            if (storeModel == null)
            {
                apiResult.Msg = "店铺信息错误";
                return Json(apiResult);
            }

            bool result = false;

            Dictionary<string, object> updateSet = new Dictionary<string, object>();
            updateSet.Add("reserveClass", goodTypes);
            updateSet.Add("reserveSwitch", isEnable);
            result = StoreBLL.SingleModel.UpdateConfig(storeModel, updateSet);

            apiResult = new Return_Msg { isok = result };
            return Json(apiResult);
        }

        [LoginFilter]
        public JsonResult UpdateReservation(int appId = 0, int reserveId = 0, int updateState = 0)
        {
            Return_Msg apiResult = new Return_Msg();
            if (reserveId <= 0)
            {
                apiResult.Msg = "参数错误";
                return Json(apiResult);
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                apiResult.Msg = "没有权限";
                return Json(apiResult);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(xcxAccountRelation.Id);
            if (store == null)
            {
                apiResult.Msg = "门店不存在";
                return Json(apiResult);
            }

            
            FoodReservation reservation = FoodReservationBLL.SingleModel.GetModel(reserveId);
            if (reservation.AId != xcxAccountRelation.Id)
            {
                apiResult.Msg = "非法请求";
                return Json(apiResult);
            }

            bool result = FoodReservationBLL.SingleModel.UpdateState(reservation, updateState, 0);

            apiResult = new Return_Msg { isok = result, Msg = result ? "操作成功" : "操作失败" };
            return Json(apiResult);
        }

        [HttpGet]
        public JsonResult GetReserveList(int appId = 0, int pageIndex = 1, int pageSize = 10, int state = (int)MiniAppEntOrderState.待接单, string userName = null, string contact = null, DateTime? start = null, DateTime? end = null, DateTime? dinnerStart = null, DateTime? dinnerEnd = null)
        {
            Return_Msg apiResult = new Return_Msg();

            if (appId <= 0)
            {
                apiResult.Msg = "参数错误!";
                return Json(apiResult, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                apiResult.Msg = "请先登录";
                return Json(apiResult, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                apiResult.Msg = "没有权限!";
                return Json(apiResult, JsonRequestBehavior.AllowGet);
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                userName = StringHelper.ReplaceSQLKey(userName).Trim();
            }

            if (!string.IsNullOrWhiteSpace(contact))
            {
                contact = StringHelper.ReplaceSQLKey(contact).Trim();
            }

            
            List<FoodReservation> reserveList = FoodReservationBLL.SingleModel.GetByList(
                 appId: xcxAccountRelation.Id, state: state, type: (int)miniAppReserveType.预约购物_专业版, pageIndex: pageIndex, pageSize: pageSize,
                 userName: userName, contact: contact, start: start, end: end, dinnerStart: dinnerStart, dinnerEnd: dinnerEnd);

            int reserveCount = 0;
            if (pageIndex == 1)
            {
                reserveCount = FoodReservationBLL.SingleModel.GetListCount(appId: xcxAccountRelation.Id, type: (int)miniAppReserveType.预约购物_专业版, state: state, userName: userName, contact: contact, start: start, end: end, dinnerStart: dinnerStart, dinnerEnd: dinnerEnd);
            }

            List<object> result = FoodReservationBLL.SingleModel.ConvertToEntApiModel(reserveList);
            apiResult = new Return_Msg { isok = true, dataObj = new { list = result, count = reserveCount }, Msg = "获取成功" };
            return Json(apiResult, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult test(int id)
        //{
        //    return Json(new FoodReservationBLL().GetModel(id), JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult test1(int id)
        //{
        //    return Json(EntGoodsOrderBLL.SingleModel.GetReserveOrder(id), JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult test3(int id)
        //{
        //    return Json(new EntGoodsCartBLL().GetModel(id), JsonRequestBehavior.AllowGet);
        //}

        #endregion 功能管理

        #region 门店自提信息

        /// <summary>
        /// 保存自提信息
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public ActionResult SavePlaceInfo(PickPlace place)
        {
            
            result = new Return_Msg();
            if (place == null || place.aid <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }
            if (string.IsNullOrEmpty(place.name))
            {
                result.Msg = "请输入店铺名称";
                return Json(result);
            }
            if (string.IsNullOrEmpty(place.address))
            {
                result.Msg = "请选择店铺地址";
                return Json(result);
            }
            place.addtime = DateTime.Now;
            if (place.Id > 0)
            {
                PickPlace model = PickPlaceBLL.SingleModel.GetModelByAid_Id(place.aid, place.Id);
                if (model == null)
                {
                    result.Msg = "数据错误";
                    return Json(result);
                }
                model.name = place.name;
                model.address = place.address;
                model.lat = place.lat;
                model.lng = place.lng;
                model.addtime = place.addtime;
                result.isok = PickPlaceBLL.SingleModel.Update(model);
            }
            else
            {
                place.Id = Convert.ToInt32(PickPlaceBLL.SingleModel.Add(place));
                result.isok = place.Id > 0;
            }
            if (result.isok)
            {
                result.dataObj = PickPlaceBLL.SingleModel.GetListByAid(place.aid);
                result.Msg = "保存成功";
            }
            else
            {
                result.Msg = "保存失败";
            }
            return Json(result);
        }

        public ActionResult updatePickPlaceState(int aid = 0, int id = 0)
        {
            
            result = new Return_Msg();
            if (aid <= 0 || id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }
            PickPlace model = PickPlaceBLL.SingleModel.GetModelByAid_Id(aid, id);
            if (model == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }
            model.state = -1;
            result.isok = PickPlaceBLL.SingleModel.Update(model, "state");
            result.Msg = result.isok ? "操作成功" : "操作失败";
            return Json(result);
        }

        #endregion 门店自提信息

        [LoginFilter(parseAuthDataTo:"authData")]
        public ViewResult Checkout(XcxAppAccountRelation authData)
        {
            return View(authData);
        }
        

        [HttpPost]
        public ActionResult ImportProducts(HttpPostedFileBase file,int aid=0)
        {
            string result = "初始化信息";
            if (file != null&&Request.Files.Count>0)
            {
                var myFile = Request.Files["file"];
                string filetype = file.FileName.Substring(file.FileName.LastIndexOf("."));
                if (EntGoodsBLL.SingleModel.ImportProductsToDB(myFile.InputStream, aid, out result, filetype))
                {
                    return Json(new { isok = true, msg = result });
                }
            }
            
            return Json(new {isok=false,msg= result });
        }

        [LoginFilter]
        public void ExportProductsFromDB(int appId = 0, int pageIndex =0, int pageSize = 1000000)
        {
            try
            {
                if (appId <= 0)
                {
                    Response.Write("参数错误");
                    return;
                }

                if (dzaccount == null)
                {
                    Response.Write("请先登录");
                    return;
                }
                
                string search = Context.GetRequest("search", "");
                int plabels = Context.GetRequestInt("plabels", 0);
                int ptype = Context.GetRequestInt("ptype", 0);
                int ptag = Context.GetRequestInt("ptag", -1);
                string stockNo = Context.GetRequest("stockNo", null);
                string ids = Context.GetRequest("ids", null);

                int count = 0;
                List<EntGoods> list = EntGoodsBLL.SingleModel.GetListByRedis(appId, search, plabels, ptype, ptag, pageIndex, pageSize, ref count, stockNo: stockNo,ids:ids);
                DataTable table = EntGoodsBLL.SingleModel.ExportProductsFromDB(list);

                ExcelHelper<EntGoods>.Out2Excel(table,"专业版普通产品"); //导出


            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
        }
        
        public ActionResult NewIndex()
        {
            return View();
        }
    }
}