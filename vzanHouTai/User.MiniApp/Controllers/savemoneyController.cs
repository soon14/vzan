using BLL.MiniApp.Conf;
using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Entity.MiniApp;
using BLL.MiniApp.Fds;
using Entity.MiniApp.Fds;
using Newtonsoft.Json;
using Entity.MiniApp.Stores;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Ent;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Footbath;
using BLL.MiniApp.Footbath;
using Entity.MiniApp.FunctionList;
using BLL.MiniApp.FunList;
using Entity.MiniApp.Plat;
using BLL.MiniApp.Plat;
using Utility.IO;
using User.MiniApp.Filters;

namespace User.MiniApp.Controllers
{

    public class  MiappSaveMoneyController: savemoneyController
    {

    }
    public class savemoneyController : baseController
    {


        
        public savemoneyController()
        {


            
        }

        
        #region 储值
        /// <summary>
        /// 获取储值项目资料
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        //[HttpGet]
        [RouteAuthCheck]
        public ActionResult MiniAppSaveMoneySetManager(int appId,int? projectType = null,int? pageType = null)
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
            XcxTemplate _tempLate = XcxTemplateBLL.SingleModel.GetModel(app.TId);
            if (_tempLate == null)
            {
                return Json(new { isok = false, msg = "系统繁忙tempLate_null！" }, JsonRequestBehavior.AllowGet);
            }

            int saveMoneySwtich = 0;//储值开关
            int versionId = 0;
            if (_tempLate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={_tempLate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.ProductMgr))
                {
                    OperationMgr operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                    saveMoneySwtich = operationMgr.SaveMoney;
                }

            }
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;
            ViewBag.saveMoneySwtich = saveMoneySwtich;
            ViewBag.versionId = versionId;

            bool canSaveMoneyFunction = true;
            switch (_tempLate.Type)
            {
                case (int)TmpType.小程序电商模板:
                    Store store = StoreBLL.SingleModel.GetModel($"appId={appId}");
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    try
                    {
                        store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
                    }
                    catch (Exception)
                    {
                        store.funJoinModel = new StoreConfigModel();
                    }
                    canSaveMoneyFunction = store.funJoinModel.canSaveMoneyFunction;
                    break;
                case (int)TmpType.小程序餐饮模板:
                    Food food = FoodBLL.SingleModel.GetModel($"appId={appId}");
                    if (food == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    canSaveMoneyFunction = food.funJoinModel.canSaveMoneyFunction;
                    break;
                case (int)TmpType.小程序专业模板:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel($"aid={appId}");
                    if (ent == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                   
                    canSaveMoneyFunction = ent.funJoinModel.canSaveMoneyFunction;
                    break;
                case (int)TmpType.小程序足浴模板:
                    FootBath footbath = FootBathBLL.SingleModel.GetModel($"appId={appId}");
                    if (footbath == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    footbath.switchModel = JsonConvert.DeserializeObject<SwitchModel>(footbath.SwitchConfig);
                    canSaveMoneyFunction = footbath.switchModel.canSaveMoneyFunction;
                    break;
                case (int)TmpType.小程序多门店模板:
                    FootBath multiStore = FootBathBLL.SingleModel.GetModel($"appId={appId} and HomeId = 0 ");
                    if (multiStore == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    multiStore.switchModel = JsonConvert.DeserializeObject<SwitchModel>(multiStore.SwitchConfig) ?? new SwitchModel();
                    canSaveMoneyFunction = multiStore.switchModel.canSaveMoneyFunction;
                    break;
                case (int)TmpType.小未平台子模版:
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(app.Id,2);

                    if (platStore == null)
                    {
                        return Json(new { isok = false, msg = "店铺不存在！" }, JsonRequestBehavior.AllowGet);
                    }
                    PlatStoreSwitchModel switchModel = new PlatStoreSwitchModel();
                   
                    if (!string.IsNullOrEmpty(platStore.SwitchConfig))
                    {
                        switchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
                        
                    }
                    canSaveMoneyFunction = switchModel.SaveMoneyPay;
                    break;
            }

            ViewBag.canSaveMoneyFuntion = canSaveMoneyFunction;
            ViewBag.appId = appId;
            if(projectType.HasValue)
            {
                ViewBag.typeId = projectType.Value;
            }
            if (pageType.HasValue)
            {
                ViewBag.typeId = pageType.Value;
            }

            return View();
        }

        /// <summary>
        /// 修改会员储值功能开关
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult changeCanSaveMoenySetting(int appId, bool canSaveMoneyFunction)
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
            XcxTemplate _tempLate = XcxTemplateBLL.SingleModel.GetModel(app.TId);
            if (_tempLate == null)
            {
                return Json(new { isok = false, msg = "系统繁忙tempLate_null！" });
            }

            #region 专业版 版本控制
            if (_tempLate.Type == (int)TmpType.小程序专业模板)
            {

                FunctionList functionList = new FunctionList();
                int industr = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={_tempLate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    return Json(new { isok = false, msg = "此功能未开启" });
                }

                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }
                if (operationMgr.SaveMoney == 1)//表示关闭了储值使用功能
                {
                    return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                }

            }
            #endregion

            bool isSuccess = true;

            switch (_tempLate.Type)
            {
                case (int)TmpType.小程序电商模板:
                    Store store = StoreBLL.SingleModel.GetModel($"appId={appId}");
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    StoreConfigModel newConfig = null;
                    try
                    {
                        newConfig = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
                    }
                    catch (Exception)
                    {
                        newConfig = new StoreConfigModel();
                    }

                    newConfig.canSaveMoneyFunction = canSaveMoneyFunction;
                    store.configJson = JsonConvert.SerializeObject(newConfig);

                    isSuccess = StoreBLL.SingleModel.Update(store, "configJson");
                    break;
                case (int)TmpType.小程序餐饮模板:
                    Food food = FoodBLL.SingleModel.GetModel($"appId={appId}");
                    if (food == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    FoodConfigModel foodConfig = food.funJoinModel;
                    foodConfig.canSaveMoneyFunction = canSaveMoneyFunction;
                    food.configJson = JsonConvert.SerializeObject(foodConfig);

                    isSuccess = FoodBLL.SingleModel.Update(food, "configJson");
                    break;
                case (int)TmpType.小程序专业模板:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel($"aid={appId}");
                    if (ent == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    

                    EntConfigModel entConfig = ent.funJoinModel;
                    entConfig.canSaveMoneyFunction = canSaveMoneyFunction;
                    ent.configJson = JsonConvert.SerializeObject(entConfig);

                    isSuccess = EntSettingBLL.SingleModel.Update(ent, "configJson");
                    break;
                case (int)TmpType.小程序足浴模板:
                    FootBath footbath = FootBathBLL.SingleModel.GetModel($"appId={appId}");
                    if (footbath == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    footbath.switchModel = JsonConvert.DeserializeObject<SwitchModel>(footbath.SwitchConfig);
                    footbath.switchModel.canSaveMoneyFunction= canSaveMoneyFunction;
                    footbath.SwitchConfig = JsonConvert.SerializeObject(footbath.switchModel);
                    isSuccess = FootBathBLL.SingleModel.Update(footbath, "SwitchConfig");
                    break;
                case (int)TmpType.小程序多门店模板:
                    FootBath multiStore = FootBathBLL.SingleModel.GetModel($"appId={appId} and HomeId = 0 ");
                    if (multiStore == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    multiStore.switchModel = JsonConvert.DeserializeObject<SwitchModel>(multiStore.SwitchConfig) ?? new SwitchModel();
                    multiStore.switchModel.canSaveMoneyFunction = canSaveMoneyFunction;
                    multiStore.SwitchConfig = JsonConvert.SerializeObject(multiStore.switchModel);
                    isSuccess = FootBathBLL.SingleModel.Update(multiStore, "SwitchConfig");
                    break;
                case (int)TmpType.小未平台子模版:
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(app.Id, 2);

                    if (platStore == null)
                    {
                        return Json(new { isok = false, msg = "店铺不存在！" }, JsonRequestBehavior.AllowGet);
                    }
                    PlatStoreSwitchModel switchModel = new PlatStoreSwitchModel();

                    if (!string.IsNullOrEmpty(platStore.SwitchConfig))
                    {
                        switchModel =JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
                        
                    }
                    switchModel.SaveMoneyPay = canSaveMoneyFunction;
                    platStore.SwitchConfig= JsonConvert.SerializeObject(switchModel);
                    platStore.UpdateTime = DateTime.Now;
                    isSuccess = PlatStoreBLL.SingleModel.Update(platStore, "UpdateTime,SwitchConfig");
                    break;
            }           
            return Json(new { isok = isSuccess, msg = isSuccess ? "修改成功" : "修改失败" });
        }



        /// <summary>
        /// 储值项目管理
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        //[HttpGet]
        public ActionResult getMiniAppSaveMoneySet(int appId, int State = -999 ,int pageIndex = 1 ,int pageSize = 6)
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
            ViewBag.appId = appId;
            List<SaveMoneySet> list = SaveMoneySetBLL.SingleModel.getListByAppId(app.AppId, State, pageIndex, pageSize);
            if (list == null || !list.Any())
            {
                if (pageIndex > 1)
                {
                    list = SaveMoneySetBLL.SingleModel.getListByAppId(app.AppId, State, pageIndex - 1, pageSize);
                }
            }
            object dataObj = new
            {
                list = list,
                recordCount = SaveMoneySetBLL.SingleModel.getCountByAppId(app.AppId, State, pageIndex, pageSize)
            };
            
            return Json(new { isok = true, dataObj = dataObj}, JsonRequestBehavior.AllowGet); 
        }
        

        /// <summary>
        /// 添加储值项目
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddMiniAppSaveMoneySet(int appId, SaveMoneySet model)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            XcxTemplate _tempLate = XcxTemplateBLL.SingleModel.GetModel(app.TId);
            if (_tempLate == null)
            {
                return Json(new { isok = false, msg = "系统繁忙tempLate_null！" });
            }
            if(_tempLate.Type== (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                int industr = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={_tempLate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    return Json(new { isok = false, msg = "此功能未开启" });
                }

                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }
                if (operationMgr.SaveMoney == 1)//表示关闭了储值使用功能
                {
                    return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                }
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }

            if (model.JoinMoney <= 0 || model.JoinMoney > 9999999)
            {
                return Json(new { isok = false, msg = "充值金额请设定0.01 ~ 99999.99！" }, JsonRequestBehavior.AllowGet);
            }

            if (model.GiveMoney <0 || model.GiveMoney > 9999999)
            {
                return Json(new { isok = false, msg = "赠送金额请设定0.00 ~ 99999.99！" }, JsonRequestBehavior.AllowGet);
            }
            //int resultInt = 0;//
            List<SaveMoneySet> List = SaveMoneySetBLL.SingleModel.getListByAppId(app.AppId);
            if (model.Id == 0)//添加
            {
                if (List.Count >= 20)
                {
                    return Json(new { isok = false, msg = "目前最多能添加20个储值项目" });
                }
                if (List.Any(m => m.JoinMoney == model.JoinMoney && m.GiveMoney == model.GiveMoney))
                {
                    return Json(new { isok = false, msg = "已存在相同储值项目 , 请重新添加" });
                }
                model.AppId = app.AppId;
                model.SetName = $"充{model.JoinMoneyStr}送{model.GiveMoneyStr}";
                model.AmountMoney = model.JoinMoney + model.GiveMoney;
                model.CreateDate = DateTime.Now;
                model.State = 1;
                object result = SaveMoneySetBLL.SingleModel.Add(model);
                if (int.Parse(result.ToString()) > 0)
                {
                    return Json(new { isok = true, msg = "添加成功!", newid = result }, JsonRequestBehavior.AllowGet);
                }
            }


            return Json(new { isok = false, msg = "系统错误！" });
        }


        /// <summary>
        /// 上下架/删除 储值项目
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult updateMiniAppSaveMoneySetState(int appId,int saveMoneySetId,int State)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            SaveMoneySet saveMoneySet = SaveMoneySetBLL.SingleModel.GetModel(saveMoneySetId);
            if(saveMoneySet == null)
            {
                return Json(new { isok = false, msg = "系统繁忙saveMoney_null！" }, JsonRequestBehavior.AllowGet);
            }
            saveMoneySet.State = State;
            if (!SaveMoneySetBLL.SingleModel.Update(saveMoneySet, "State"))
            {
                return Json(new { isok = false, msg = "状态更新失败！" });
            }
            
            return Json(new { isok = true, msg = "状态更新成功！" });
        }

        /// <summary>
        /// 是否可以添加储值项目
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetMiniAppSaveMoneySetCanAdd(int appId)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            //int resultInt = 0;//
            int count = SaveMoneySetBLL.SingleModel.getCountByAppId(app.AppId);
            
            if (count >= 20)
            {
                return Json(new { isok = false, msg = "无法新增储值项目！您已添加了20个储值项目，已达到上限，请编辑已有的储值项目或删除部分储值项目后再进行新增。" });
            }

            return Json(new { isok = true, msg = "可以添加！" });
        }


        /// <summary>
        /// 储值充值记录查看
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        //[HttpGet]
        public ActionResult getMiniAppSaveMoneySetUserLog(int appId, DateTime? changeStartDate, DateTime? changeEndDate, int State = -999, int pageIndex = 1, int pageSize = 6, string nickName = "", int changeType = 0, string changeNote = "")
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

             SaveMoneySetUserLogViewBLL userLogViewBLL = new  SaveMoneySetUserLogViewBLL();

            ViewBag.appId = appId;

            object dataObj = new
            {
                list = userLogViewBLL.getListByCondition(app.AppId, changeStartDate, changeEndDate, nickName, changeType, changeNote, pageIndex, pageSize),
                recordCount = userLogViewBLL.getCountByCondition(app.AppId, changeStartDate, changeEndDate, nickName, changeType, changeNote, pageIndex, pageSize)
            };

            return Json(new { isok = true, dataObj = dataObj }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        /// <summary>
        /// 储值消费记录
        /// </summary>
        /// <returns></returns>
        public ActionResult getMiniAppSaveMoneyLoglist()
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            int appid = Utility.IO.Context.GetRequestInt("appid", 0);
            if (appid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙aid_null！" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appid, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            int uid = Utility.IO.Context.GetRequestInt("uid", 0);
            if (uid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙uid_null！" }, JsonRequestBehavior.AllowGet);
            }
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            List<SaveMoneySetUserLog> list = SaveMoneySetUserLogBLL.SingleModel.GetList($"UserId={uid} and state>0", pageSize, pageIndex, "*", "id desc");
            int count = SaveMoneySetUserLogBLL.SingleModel.GetCount($"UserId={uid} and state>0");
            return Json(new { isok = true, list=list, count = count }, JsonRequestBehavior.AllowGet);
        }


    }
}