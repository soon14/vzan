using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Model;
using Utility.IO;
using User.MiniApp.Areas.PlatChild.Filters;
using Entity.MiniApp.CoreHelper;
using Newtonsoft.Json;

namespace User.MiniApp.Areas.PlatChild.Controllers
{
    /// <summary>
    /// 大部分是平台版子模板后台功能
    /// </summary>
    [LoginFilter]
    public class StoreController : User.MiniApp.Controllers.baseController
    {
        private PlatReturnMsg returnObj;
        
        public StoreController()
        {

        }

        #region 店铺首页编辑

        // GET: PlatChild/Store
        public ActionResult Index()
        {
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);

            if (appId <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(appId, 2);
            if (platStore == null)
                return View("PageError", new PlatReturnMsg() { Msg = "店铺不存在!", code = "404" });

            if (!string.IsNullOrEmpty(platStore.SwitchConfig))
            {
                platStore.SwitchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
            }
            else
            {
                platStore.SwitchModel = new PlatStoreSwitchModel();
            }


            if (!string.IsNullOrEmpty(platStore.StoreService) && platStore.StoreService.Contains("ServiceState"))
            {
                platStore.StoreServiceModelList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StoreServiceModel>>(platStore.StoreService);
            }
            else
            {
                List<StoreServiceModel> list = new List<StoreServiceModel>();
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "WIFI" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "停车位" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "支付宝支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "微信支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "刷卡支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "空调雅座" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "付费停车" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "接送服务" });
                platStore.StoreServiceModelList = list;
            }




            return View(platStore);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveStore(PlatStore store)
        {

            returnObj = new PlatReturnMsg();
            if (string.IsNullOrEmpty(store.Banners) || store.Banners.Split(',').Length <= 0 || store.Banners.Split(',').Length > 5)
            {
                returnObj.Msg = "轮播图至少一张最多5张";
                return Json(returnObj);
            }

            if (string.IsNullOrEmpty(store.Name) || store.Name.Length >= 20)
            {
                returnObj.Msg = "店铺名称不能为空并且不能超过20字符";
                return Json(returnObj);
            }

            if (string.IsNullOrEmpty(store.Location))
            {
                returnObj.Msg = "地址不能为空";
                return Json(returnObj);
            }
            
            if (string.IsNullOrEmpty(store.OpenTime))
            {
                returnObj.Msg = "营业时间不能为空";
                return Json(returnObj);
            }
            //TODO 需要验证手机+电话

            if (string.IsNullOrEmpty(store.Phone))
            {
                returnObj.Msg = "请填写正确的号码";
                return Json(returnObj);
            }
            
            if (string.IsNullOrEmpty(store.BusinessDescription))
            {
                returnObj.Msg = "业务简述不能为空";
                return Json(returnObj);
            }
            
            if (store.SwitchModel != null)
            {
                store.SwitchConfig = Newtonsoft.Json.JsonConvert.SerializeObject(store.SwitchModel);
            }
            else
            {
                if (string.IsNullOrEmpty(store.SwitchConfig))
                {
                    store.SwitchConfig = Newtonsoft.Json.JsonConvert.SerializeObject(new PlatStoreSwitchModel());

                }
            }
            AddressApi addressinfo = AddressHelper.GetAddressByApi(store.Lng.ToString(), store.Lat.ToString());
           // log4net.LogHelper.WriteInfo(this.GetType(),Newtonsoft.Json.JsonConvert.SerializeObject(addressinfo));
            string provinceName = addressinfo.result.address_component.province;
            string cityName = addressinfo.result.address_component.city;
            string countryName = addressinfo.result.address_component.district;
            store.ProvinceCode = C_AreaBLL.SingleModel.GetCodeByName(provinceName);
            store.CityCode = C_AreaBLL.SingleModel.GetCodeByName(cityName);
            store.CountryCode = C_AreaBLL.SingleModel.GetCodeByName(countryName);

            if (store.StoreServiceModelList == null && store.StoreServiceModelList.Count <= 0)
            {
                //表示旧版本的数据,设施服务给初始化新值

                store.StoreServiceModelList.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "WIFI" });
                store.StoreServiceModelList.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "停车位" });
                store.StoreServiceModelList.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "支付宝支付" });
                store.StoreServiceModelList.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "微信支付" });
                store.StoreServiceModelList.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "刷卡支付" });
                store.StoreServiceModelList.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "空调雅座" });
                store.StoreServiceModelList.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "付费停车" });
                store.StoreServiceModelList.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "接送服务" });

            }

            store.StoreService = Newtonsoft.Json.JsonConvert.SerializeObject(store.StoreServiceModelList);
           // log4net.LogHelper.WriteInfo(this.GetType(), "店铺" + Newtonsoft.Json.JsonConvert.SerializeObject(store));

            if (store.Id > 0)
            {
                //表示更新
                TransactionModel TranModel = new TransactionModel();
                store.UpdateTime = DateTime.Now;
                TranModel.Add(PlatStoreBLL.SingleModel.BuildUpdateSql(store));
              //  log4net.LogHelper.WriteInfo(this.GetType(),"店铺2"+Newtonsoft.Json.JsonConvert.SerializeObject(store));
                XcxAppAccountRelation r = XcxAppAccountRelationBLL.SingleModel.GetModel(store.BindPlatAid);
                if (r == null)
                {
                    returnObj.Msg = "更新异常(所属平台小程序未授权)";
                    return Json(returnObj);
                }

                Agentinfo agent = AgentinfoBLL.SingleModel.GetModelByAccoundId(r.AccountId.ToString());

              //  log4net.LogHelper.WriteInfo(this.GetType(), "agent=" + Newtonsoft.Json.JsonConvert.SerializeObject(agent));

                PlatStoreRelation platStoreRelation = PlatStoreRelationBLL.SingleModel.GetPlatStoreRelationOwner(store.BindPlatAid, store.Id);
                if (platStoreRelation == null)
                {
                    returnObj.Msg = "更新异常(店铺关系找不到数据)";
                    return Json(returnObj);
                }
               // log4net.LogHelper.WriteInfo(this.GetType(), "platStoreRelation=" + Newtonsoft.Json.JsonConvert.SerializeObject(store));

                platStoreRelation.Category = store.Category;
                platStoreRelation.UpdateTime = DateTime.Now;
                TranModel.Add(PlatStoreRelationBLL.SingleModel.BuildUpdateSql(platStoreRelation, "Category,UpdateTime"));
              //  log4net.LogHelper.WriteInfo(this.GetType(), "sql=" + Newtonsoft.Json.JsonConvert.SerializeObject(TranModel.sqlArray));

                if (TranModel.sqlArray != null && TranModel.sqlArray.Length > 0 && PlatStoreRelationBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "更新失败";
                    return Json(returnObj);
                }
            }
            else
            {
                //表示新增

                store.AddTime = DateTime.Now;
                store.UpdateTime = DateTime.Now;
                int storeId = Convert.ToInt32(PlatStoreBLL.SingleModel.Add(store));
                if (storeId > 0)
                {
                    int appId = Context.GetRequestInt("appId", 0);
                    int agentinfoId = Context.GetRequestInt("agentinfoId", 0);
                    //插入关系表  才算成功 店铺数据都是从关系表获取才算有效的
                    //1.先将入驻到平台的插入 
                    PlatStoreRelation platStoreRelation = new PlatStoreRelation();
                    platStoreRelation.StoreId = storeId;
                    platStoreRelation.State = 1;
                    platStoreRelation.FromType = 0;
                    platStoreRelation.Aid = appId;
                    platStoreRelation.Category = store.Category;
                    platStoreRelation.AgentId = agentinfoId;
                    platStoreRelation.AddTime = DateTime.Now;
                    platStoreRelation.UpdateTime = DateTime.Now;

                    int platStoreRelationId = Convert.ToInt32(PlatStoreRelationBLL.SingleModel.Add(platStoreRelation));
                    if (platStoreRelationId > 0)
                    {
                        returnObj.dataObj = new { storeId = storeId, platStoreRelationId = platStoreRelationId };
                        returnObj.isok = true;
                        returnObj.Msg = "新增成功";
                        return Json(returnObj);
                    }
                    else
                    {
                        returnObj.Msg = "新增异常";
                        return Json(returnObj);
                    }

                }
                else
                {
                    returnObj.Msg = "新增失败";
                    return Json(returnObj);
                }


            }







        }

        /// <summary>
        /// 将平台后台添加的没有绑定名片的店铺绑定上名片
        /// </summary>
        /// <returns></returns>
        public ActionResult BindMyCard()
        {
            int platMyCardId = Context.GetRequestInt("platMyCardId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            returnObj = new PlatReturnMsg();
          
            if (platMyCardId <= 0 || storeId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(storeId);
            if (platStore == null)
            {
                returnObj.Msg = "店铺不存在";
                return Json(returnObj);
            }
            if (platStore.MyCardId>0)
            {
                returnObj.Msg = "该店铺已经有主人了!";
                return Json(returnObj);
            }
            platStore.MyCardId = platMyCardId;
            if(PlatStoreBLL.SingleModel.Update(platStore, "MyCardId"))
            {
                returnObj.isok = true;
                returnObj.Msg = "绑定成功!";
                return Json(returnObj);
            }
            else
            {
                returnObj.Msg = "绑定失败!";
                return Json(returnObj);
            }


        }


        #endregion
        
        #region 分享配置

        /// <summary>
        /// 分享配置
        /// </summary>
        /// <returns></returns>
        public ActionResult Share(int aid = 0)
        {
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            if (appId <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            PlatChildShare platChildShare = PlatChildShareBLL.SingleModel.GetPlatChildShare(aid);
            if (platChildShare == null)
            {
                platChildShare = new PlatChildShare()
                {
                    Aid = aid,
                    AddTime = DateTime.Now
                };
                int id = Convert.ToInt32(PlatChildShareBLL.SingleModel.Add(platChildShare));
                if (id <= 0)
                {
                    return View("PageError", new PlatReturnMsg() { Msg = "初始化页面失败!", code = "500" });
                }
            }



            return View(platChildShare);
        }


        /// <summary>
        /// 保存分享配置
        /// </summary>
        /// <param name="platChildShare"></param>
        /// <returns></returns>
        public ActionResult SetShare(PlatChildShare platChildShare)
        {
            returnObj = new PlatReturnMsg();
            if (platChildShare.Aid <= 0 && platChildShare.Id <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }


            if (PlatChildShareBLL.SingleModel.GetModel(platChildShare.Id) == null)
            {
                returnObj.Msg = "数据不存在";
                return Json(returnObj);
            }

            if (string.IsNullOrEmpty(platChildShare.StoreName) || platChildShare.StoreName.Length > 10)
            {
                returnObj.Msg = "店铺名称不能为空或者不能大于10个字符";
                return Json(returnObj);
            }


            if (!string.IsNullOrEmpty(platChildShare.ADTitle) && platChildShare.ADTitle.Length > 20)
            {
                returnObj.Msg = "广告语不能大于20个字符";
                return Json(returnObj);
            }

            if (string.IsNullOrEmpty(platChildShare.StoreLogo))
            {
                returnObj.Msg = "店铺Logo不能为空";
                return Json(returnObj);
            }

            if (string.IsNullOrEmpty(platChildShare.ADImg))
            {
                returnObj.Msg = "广告图不能为空";
                return Json(returnObj);
            }

            if (PlatChildShareBLL.SingleModel.Update(platChildShare))
            {
                returnObj.isok = true;
                returnObj.Msg = "设置成功";
                return Json(returnObj);
            }
            else
            {
                returnObj.Msg = "操作异常";
                return Json(returnObj);
            }




        }



        /// <summary>
        /// 获取小程序分二维码
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult GetQrcode(int aid = 0)
        {

            returnObj = new PlatReturnMsg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(aid, dzaccount.Id.ToString());
            if (role == null)
            {
                returnObj.Msg = "系统繁忙";
                return Json(returnObj);
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(role.TId);
            if (xcxTemplate == null)
            {
                returnObj.Msg = "小程序模板不存在";
                return Json(returnObj);
            }
            PlatChildShare platChildShare = PlatChildShareBLL.SingleModel.GetPlatChildShare(aid);
            
            if (platChildShare == null)
            {
                returnObj.Msg = "数据不存在请先进行分享配置";
                return Json(returnObj);
            }
            
            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(role, ref token))
            {
                returnObj.Msg = token;
                return Json(returnObj);
            }
            string qrcodeImg = CommondHelper.GetQrcode(token,"pages/index/index");

            if (string.IsNullOrEmpty(qrcodeImg))
            {
                returnObj.Msg = "获取失败";
                return Json(returnObj);
            }


            platChildShare.Qrcode = qrcodeImg;
            if (!PlatChildShareBLL.SingleModel.Update(platChildShare, "Qrcode"))
            {
                returnObj.Msg = "获取异常";
                return Json(returnObj);
            }
            returnObj.isok = true;
            returnObj.dataObj = qrcodeImg;
            returnObj.Msg = "获取成功";
            return Json(returnObj);


        }
        #endregion

        #region 微信在线客服工具使用说明
        /// <summary>
        /// 微信在线客服工具使用说明
        /// </summary>
        public ActionResult WebChatService(int aid = 0)
        {
            //if (dzaccount == null)
            //    return View("PageError", new PlatReturnMsg() { Msg = "请先登录!", code = "403" });
            //XcxAppAccountRelation app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(aid, dzaccount.Id.ToString());

            //if (app == null)
            //    return View("PageError", new PlatReturnMsg() { Msg = "小程序未授权!", code = "403" });
            //XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(app.TId);
            //if (xcxTemplate == null)
            //    return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "404" });
            return View();
        }
        #endregion
        
        #region 店铺产品类别管理

        public ActionResult ProductCategroyMgr(int aid = 0, int isFirstType = 2)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });



            PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(aid, 2);

            if (platStore == null)
            {
                return View("PageError", new PlatReturnMsg() { Msg = "数据不存在!", code = "404" });
            }
            PlatStoreSwitchModel switchModel = new PlatStoreSwitchModel();
            ViewBag.ProductCategoryLevel = 1;
            if (!string.IsNullOrEmpty(platStore.SwitchConfig))
            {
                switchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
                ViewBag.ProductCategoryLevel = switchModel.ProductCategoryLevel;
            }


            int totalCount = 0;
            List<PlatChildGoodsCategory> list = new List<PlatChildGoodsCategory>();
            list.Add(new PlatChildGoodsCategory()
            {
                Id = 0,
                Name = "请选择"
            });
            list.AddRange(PlatChildGoodsCategoryBLL.SingleModel.getListByaid(aid, out totalCount, 0, 100, 1));

            ViewBag.isFirstType = isFirstType;

            ViewBag.appId = aid;
            return View(list);
        }


        public ActionResult UpdateProductCategoryLevel(int aid = 0)
        {
            returnObj = new PlatReturnMsg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
            {
                returnObj.Msg = "小程序模板不存在";
                return Json(returnObj);
            }

            PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(aid, 2);

            if (platStore == null)
            {
                returnObj.Msg = "数据不存在";
                return Json(returnObj);

            }
            PlatStoreSwitchModel switchModel = new PlatStoreSwitchModel();

            if (!string.IsNullOrEmpty(platStore.SwitchConfig))
            {
                switchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
                switchModel.ProductCategoryLevel = switchModel.ProductCategoryLevel == 1 ? 2 : 1;
            }

            platStore.SwitchConfig = Newtonsoft.Json.JsonConvert.SerializeObject(switchModel);

            if (PlatStoreBLL.SingleModel.Update(platStore, "SwitchConfig"))
            {
                returnObj.isok = true;
                returnObj.Msg = "操作成功";
                return Json(returnObj);
            }
            else
            {

                returnObj.Msg = "操作失败";
                return Json(returnObj);
            }



        }



        /// <summary>
        /// 获取店铺产品分类
        /// </summary>
        /// <returns></returns>

        public ActionResult GetCategoryList()
        {
            returnObj = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int isFirstType = Utility.IO.Context.GetRequestInt("isFirstType", 1);
            int parentId = Utility.IO.Context.GetRequestInt("parentId", 0);
            if (appId <= 0)
            {
                returnObj.Msg = "appId非法";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int isList = Utility.IO.Context.GetRequestInt("isList", 0);
            int totalCount = 0;
            List<PlatChildGoodsCategory> list = new List<PlatChildGoodsCategory>();
            if (isList != 0)
            {
                list.Add(new PlatChildGoodsCategory()
                {
                    Id = 0,
                    Name = "请选择"
                });
            }

            list.AddRange(PlatChildGoodsCategoryBLL.SingleModel.getListByaid(appId, out totalCount, isFirstType, pageSize, pageIndex, "sortNumber desc,addTime desc", parentId));

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 类别名称是否存在
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckCategoryName()
        {
            returnObj = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            int isFirstType = Utility.IO.Context.GetRequestInt("isFirstType", 0);
            string categoryName = Utility.IO.Context.GetRequest("categoryName", string.Empty);
            if (appId <= 0 || string.IsNullOrEmpty(categoryName))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            PlatChildGoodsCategory model = PlatChildGoodsCategoryBLL.SingleModel.msgTypeNameIsExist(appId, categoryName);
            if (model != null && model.Id != Id)
            {
                returnObj.Msg = "类别名称已存在";
                return Json(returnObj);
            }
            returnObj.isok = true;
            returnObj.Msg = "ok";
            return Json(returnObj);
        }



        /// <summary>
        /// 编辑或者新增 信息分类
        /// </summary>
        /// <param name="city_Storemsgrules"></param>
        /// <returns></returns>

        public ActionResult SaveCategory(PlatChildGoodsCategory PlatChildProductCategory)
        {

            returnObj = new PlatReturnMsg();
            if (PlatChildProductCategory == null)
            {
                returnObj.Msg = "数据不能为空";
                return Json(returnObj);
            }

            if (PlatChildProductCategory.AId <= 0)
            {
                returnObj.Msg = "appId非法";
                return Json(returnObj);
            }



            int Id = PlatChildProductCategory.Id;
            if (Id == 0)
            {
                PlatChildGoodsCategory model = new PlatChildGoodsCategory()
                {
                    MaterialPath = PlatChildProductCategory.MaterialPath,
                    Name = PlatChildProductCategory.Name,
                    AddTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    AId = PlatChildProductCategory.AId,
                    State = 0,
                    SortNumber = PlatChildProductCategory.SortNumber,
                    ParentId = PlatChildProductCategory.ParentId

                };
                //表示新增
                Id = Convert.ToInt32(PlatChildGoodsCategoryBLL.SingleModel.Add(model));
                if (Id > 0)
                {
                    returnObj.dataObj = model;
                    returnObj.isok = true;
                    returnObj.Msg = "新增成功";
                    return Json(returnObj);

                }
                else
                {
                    returnObj.Msg = "新增失败";
                    return Json(returnObj);
                }

            }
            else
            {
                //表示更新
                PlatChildGoodsCategory model = PlatChildGoodsCategoryBLL.SingleModel.GetModel(Id);
                if (model == null)
                {
                    returnObj.Msg = "不存在数据库里";
                    return Json(returnObj);
                }

                if (model.AId != PlatChildProductCategory.AId)
                {
                    returnObj.Msg = "权限不足";
                    return Json(returnObj);
                }
                model.UpdateTime = DateTime.Now;
                model.MaterialPath = PlatChildProductCategory.MaterialPath;
                model.Name = PlatChildProductCategory.Name;
                model.SortNumber = PlatChildProductCategory.SortNumber;
                model.ParentId = PlatChildProductCategory.ParentId;
                if (PlatChildGoodsCategoryBLL.SingleModel.Update(model, "UpdateTime,MaterialPath,Name,SortNumber,ParentId"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "更新成功";
                    return Json(returnObj);

                }
                else
                {
                    returnObj.Msg = "更新失败";
                    return Json(returnObj);

                }

            }

        }


        /// <summary>
        /// 删除产品分类包括单个批量
        /// </summary>
        /// <returns></returns>

        public ActionResult DelCategory()
        {
            returnObj = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            if (appId <= 0)
            {
                returnObj.Msg = "appId非法";
                return Json(returnObj);
            }

            if (!Utility.StringHelper.IsNumByStrs(',', ids))
            {
                returnObj.Msg = "非法操作";
                return Json(returnObj);
            }
            //判断是否有权限
            List<PlatChildGoodsCategory> list = PlatChildGoodsCategoryBLL.SingleModel.GetListByIds(appId, ids);
            TransactionModel tranModel = new TransactionModel();
            foreach (PlatChildGoodsCategory item in list)
            {
                if (appId != item.AId)
                {
                    returnObj.Msg = $"非法操作(无权限对id={item.Id}的类别)";
                    return Json(returnObj);
                }
                int count = 0;
                if (item.ParentId == 0)
                {
                    //表示删除的是大类,要检测有没有子类,没有子类才能删除
                    count = PlatChildGoodsCategoryBLL.SingleModel.GetSecondCategoryCount(item.Id);
                    if (count > 0)
                    {
                        returnObj.Msg = $"{item.Name}类别下包含{count}子类别,请先删除子类别再删除)";
                        return Json(returnObj);
                    }
                }
                else
                {

                    //表示删除的是子类,要检测该类别下有没有关联的产品,没有才能删除
                    count = PlatChildGoodsBLL.SingleModel.GetServiceCountById(appId,item.Id);
                    if (count > 0)
                    {
                        returnObj.Msg = $"{item.Name}类别下包含{count}个产品,请先删除该类别下的产品再删除)";
                        return Json(returnObj);
                    }
                }


                item.State = -1;
                item.UpdateTime = DateTime.Now;
                tranModel.Add(PlatChildGoodsCategoryBLL.SingleModel.BuildUpdateSql(item, "State,UpdateTime"));

            }


            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatChildGoodsCategoryBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
                else
                {
                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            else
            {
                returnObj.Msg = "没有需要删除的数据";
                return Json(returnObj);

            }
        }


        /// <summary>
        /// 批量更新排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>

        public ActionResult SaveCategorySort(List<PlatChildGoodsCategory> list)
        {
            returnObj = new PlatReturnMsg();

            if (list == null || list.Count <= 0)
            {
                returnObj.Msg = "数据不能为空";
                return Json(returnObj);

            }
            PlatChildGoodsCategory model = new PlatChildGoodsCategory();
            TransactionModel tranModel = new TransactionModel();
            string sql = string.Empty;

            string categoryIds = string.Join(",",list.Select(s=>s.Id));
            List<PlatChildGoodsCategory> platChildGoodsCategoryList = PlatChildGoodsCategoryBLL.SingleModel.GetListByIds(categoryIds);
            foreach (PlatChildGoodsCategory item in list)
            {
                model = platChildGoodsCategoryList?.FirstOrDefault(f=>f.Id == item.Id);
                if (model == null)
                {
                    returnObj.Msg = $"Id={item.Id}不存在数据库里";
                    return Json(returnObj);
                }

                if (model.AId != item.AId)
                {
                    returnObj.Msg = $"Id={item.Id}权限不足";
                    return Json(returnObj);
                }


                model.SortNumber = item.SortNumber;
                model.UpdateTime = DateTime.Now;
                sql = PlatChildGoodsCategoryBLL.SingleModel.BuildUpdateSql(model, "SortNumber,UpdateTime");
                tranModel.Add(sql);

            }

            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatChildGoodsCategoryBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
                else
                {

                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            else
            {
                returnObj.Msg = "没有需要更新的数据";
                return Json(returnObj);

            }


        }
        #endregion
        
        #region 店铺产品标签
        public ActionResult ProductLabelMgr(int aid, int pageIndex = 1, int pageSize = 100)
        {

            if (aid <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            //if (dzaccount == null)
            //    return Redirect("/dzhome/login");
            //XcxAppAccountRelation app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            //if (app == null)
            //    return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            //XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            //if (xcxTemplate == null)
            //    return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });



            ViewModel<PlatChildGoodsLabel> vm = new ViewModel<PlatChildGoodsLabel>();
            int count = 0;
            vm.DataList = PlatChildGoodsLabelBLL.SingleModel.GetListByCach(aid, pageSize, pageIndex, ref count);
            vm.TotalCount = count;
            return View(vm);
        }

        [HttpPost]
        public ActionResult SaveProductLabel(int aid, PlatChildGoodsLabel model)
        {
            returnObj = new PlatReturnMsg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            //if (dzaccount == null)
            //    return Json(new { isok = false, msg = "登录信息过期请重新登录" });

            //XcxAppAccountRelation app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            //if (app == null)
            //    return Json(new { isok = false, msg = "小程序未授权" });

            //XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            //if (xcxTemplate == null)
            //    return Json(new { isok = false, msg = "小程序模板不存在" });






            //清除缓存
            PlatChildGoodsLabelBLL.SingleModel.RemovePlatChildProductLabelListCache(aid);

            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);


            #region 添加和修改
            if (model == null || model.Id < 0)
            {
                returnObj.Msg = "非法请求";
                return Json(returnObj);
            }
            if (model.Name.Trim() == "" || model.Name.Trim().Length > 10)
            {
                returnObj.Msg = "分类名称不能为空，且不能超过10个字";
                return Json(returnObj);

            }
            PlatChildGoodsLabel platChildProductLabel = PlatChildGoodsLabelBLL.SingleModel.GetLabelByName(aid, model.Name);
            //修改
            if (model.Id > 0)
            {

                if (platChildProductLabel != null && platChildProductLabel.Id != model.Id)
                {
                    returnObj.Msg = "已存在该分类名称，请重新设置！";
                    return Json(returnObj);

                }

                if (PlatChildGoodsLabelBLL.SingleModel.Update(model))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "更新成功";
                    return Json(returnObj);
                }
            }
            //添加
            else
            {
                if (platChildProductLabel != null)
                {
                    returnObj.Msg = "已存在该分类名称，请重新设置！";
                    return Json(returnObj);

                }


                //不能超过100个
                int checkcount = PlatChildGoodsLabelBLL.SingleModel.GetProductLabelCount(aid);
                if (checkcount >= 100)
                {
                    returnObj.Msg = "无法新增标签！您已添加了100个标签分类，已达到上限，请编辑已有的标签或删除部分标签后再进行新增。";
                    return Json(returnObj);

                }
                int newid = Convert.ToInt32(PlatChildGoodsLabelBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    returnObj.isok = true;
                    returnObj.dataObj = model;
                    returnObj.Msg = "新增成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "操作失败！";
                    return Json(returnObj);
                }
            }
            #endregion

            return Json(new { isok = false, msg = "操作异常！" });
        }


        /// <summary>
        /// 批量更新排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>

        public ActionResult SaveProductLabelSort(List<PlatChildGoodsLabel> list, int aid = 0, int actionType = 0)
        {
            returnObj = new PlatReturnMsg();

            if (list == null || list.Count <= 0)
            {
                returnObj.Msg = "数据不能为空";
                return Json(returnObj);

            }
            PlatChildGoodsLabel model = new PlatChildGoodsLabel();
            TransactionModel tranModel = new TransactionModel();
            string sql = string.Empty;

            string labelIds = string.Join(",",list.Select(s=>s.Id));
            List<PlatChildGoodsLabel> platChildGoodsLabelList = PlatChildGoodsLabelBLL.SingleModel.GetListByIds(labelIds);
            foreach (PlatChildGoodsLabel item in list)
            {
                model = platChildGoodsLabelList?.FirstOrDefault(f=>f.Id == item.Id);
                if (model == null)
                {
                    returnObj.Msg = $"Id={item.Id}不存在数据库里";
                    return Json(returnObj);
                }

                if (model.AId != item.AId)
                {
                    returnObj.Msg = $"Id={item.Id}权限不足";
                    return Json(returnObj);
                }
                string filed = "Sort";

                if (actionType > 0)
                {
                    int checkcount = PlatChildGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({model.Id},plabels)>0 and aid={aid} and state=1");
                    if (checkcount > 0)
                    {
                        returnObj.Msg = $"该标签下已有{checkcount}个产品，不可删除！";
                        return Json(returnObj);
                    }

                    //表示删除
                    filed = "State";
                    model.State = 0;
                }
                else
                {
                    //表示排序
                    model.Sort = item.Editsort;
                }


                sql = PlatChildGoodsLabelBLL.SingleModel.BuildUpdateSql(model, filed);
                tranModel.Add(sql);

            }

            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatChildGoodsLabelBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    PlatChildGoodsLabelBLL.SingleModel.RemovePlatChildProductLabelListCache(aid);
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
                else
                {

                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            else
            {
                returnObj.Msg = "没有需要操作的数据";
                return Json(returnObj);

            }


        }

        #endregion

        #region 产品规格管理

        /// <summary>
        /// 规格列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult ProductSKUMgr(int aid, int pageIndex = 1, int pageSize = 20)
        {


            if (aid <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            //if (dzaccount == null)
            //    return Redirect("/dzhome/login");
            //XcxAppAccountRelation app = _xcxappaccountrelationBll.GetModelByaccountidAndaid(aid, dzaccount.Id.ToString());

            //if (app == null)
            //    return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            //XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            //if (xcxTemplate == null)
            //    return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });







            int fid = Utility.IO.Context.GetRequestInt("fid", 0);

            ViewModel<PlatChildSpecification> vm = new ViewModel<PlatChildSpecification>();
            string strwhere = $"aid={aid} and state=1 and parentid={fid}";
            vm.DataList = PlatChildSpecificationBLL.SingleModel.GetList(strwhere, 100, 1);
            return View(vm);
        }
        /// <summary>
        /// 编辑规格
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveProductSKU(int aid, PlatChildSpecification model)
        {
            returnObj = new PlatReturnMsg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            //if (dzaccount == null)
            //    return Json(new { isok = false, msg = "登录信息过期请重新登录" });

            //XcxAppAccountRelation app = _xcxappaccountrelationBll.GetModelByaccountidAndaid(aid, dzaccount.Id.ToString());
            //if (app == null)
            //    return Json(new { isok = false, msg = "小程序未授权" });

            //XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            //if (xcxTemplate == null)
            //    return Json(new { isok = false, msg = "小程序模板不存在" });


            string act = Utility.IO.Context.GetRequest("act", string.Empty);
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            #region 删除
            if (act == "del")
            {
                if (id <= 0)
                {
                    returnObj.Msg = "非法请求";
                    return Json(returnObj);

                }
                model = PlatChildSpecificationBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    returnObj.Msg = "该规格不存在";
                    return Json(returnObj);

                }
                // TODO 检查是否有已经有产品使用了规格或规格值
                int checkcount = 0;
                //如果删除规格
                if (model.ParentId == 0)
                {
                    checkcount = PlatChildGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({id},SpecName)>0 and aid={aid} and state=1");
                }
                //如果删除规格值
                else
                {
                    checkcount = PlatChildGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({id},SpecValue)>0 and aid={aid} and state=1");
                }

                if (checkcount > 0)
                {
                    returnObj.Msg = $"该规格下已有{checkcount}个产品，不可删除！";
                    return Json(returnObj);
                }


                model.State = 0;
                if (PlatChildSpecificationBLL.SingleModel.Update(model, "State"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            #endregion

            #region 添加和修改
            if (model == null || model.Id < 0)
            {
                returnObj.Msg = "非法请求";
                return Json(returnObj);

            }
            if (model.Name.Trim() == "" || model.Name.Trim().Length > 20)
            {
                returnObj.Msg = "规格名称不能为空，且不能超过20个字";
                return Json(returnObj);

            }
            PlatChildSpecification platChildProductSKU = PlatChildSpecificationBLL.SingleModel.GetSKUByName(aid, model.Name);


            //修改
            if (model.Id > 0)
            {
                if (platChildProductSKU != null && platChildProductSKU.Id != model.Id)
                {
                    returnObj.Msg = "已存在该规格名称，请重新设置！";
                    return Json(returnObj);
                }
                if (PlatChildSpecificationBLL.SingleModel.Update(model))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
            }
            //添加
            else
            {

                int checkcount = PlatChildSpecificationBLL.SingleModel.GetCountPlatChildProductSKU(aid, model.ParentId);
                if (checkcount >= 200)
                {
                    return Json(new { isok = false, msg = "无法新增规格！您已添加了200个产品规格，已达到上限，请编辑已有的分类或删除部分规格后再进行新增。" });
                }
                int newid = Convert.ToInt32(PlatChildSpecificationBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.Id = newid;
                    returnObj.dataObj = model;
                    returnObj.isok = true;
                    returnObj.Msg = "添加成功";
                    return Json(returnObj);

                }
            }
            #endregion
            returnObj.Msg = "操作异常";
            return Json(returnObj);
        }

        #endregion

        public ActionResult ProductMgr(int aid, int pageIndex = 1, int pageSize = 20)
        {
            ViewModel<PlatChildGoods> vm = new ViewModel<PlatChildGoods>();
            try
            {

                string search = Context.GetRequest("search", "");
                int plabels = Context.GetRequestInt("plabels", 0);
                int ptype = Context.GetRequestInt("ptype", 0);
                int ptag = Context.GetRequestInt("ptag", -1);


                int count = 0;
                vm.DataList = PlatChildGoodsBLL.SingleModel.GetListByRedis(aid, ref count, search, plabels, ptype, ptag, pageIndex, pageSize);
                vm.TotalCount = count;
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }

            return View(vm);
        }
        
        public ActionResult ProductEdit(int aid, int id = 0)
        {
            if (aid <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(aid, 2);

            if (platStore == null)
                return View("PageError", new PlatReturnMsg() { Msg = "数据不存在!", code = "404" });

            PlatStoreSwitchModel switchModel = new PlatStoreSwitchModel();
            ViewBag.ProductCategoryLevel = 1;
            if (!string.IsNullOrEmpty(platStore.SwitchConfig))
            {
                switchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
                ViewBag.ProductCategoryLevel = switchModel.ProductCategoryLevel;
            }


            PlatChildGoods platChildProduct = PlatChildGoodsBLL.SingleModel.GetModel(id);
            if (platChildProduct == null)
            {
                platChildProduct = new PlatChildGoods();
                platChildProduct.AId = aid;
            }

            return View(platChildProduct);



        }

        [HttpPost, ValidateInput(false)]
        public ActionResult SaveProduct(PlatChildGoods platChildProduct, int aid = 0)
        {
            returnObj = new PlatReturnMsg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            PlatStore platStore = PlatStoreBLL.SingleModel.GetModelByAId(aid);
            if (platStore == null)
            {
                returnObj.Msg = "店铺不存在";
                return Json(returnObj);
            }

            XcxAppAccountRelation agentinfoXcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelById(platStore.BindPlatAid);
            if (agentinfoXcxAppAccountRelation == null)
            {
                returnObj.Msg = "产品所属店铺的平台没有进行小程序授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(agentinfoXcxAppAccountRelation.AccountId.ToString());
            //if (agentinfo == null)//本平台代理信息  查找上级需要以及上上级需要
            //{
            //    returnObj.Msg = "产品所属店铺的平台不是代理商";
            //    return Json(returnObj, JsonRequestBehavior.AllowGet);
            //}



            PlatChildGoodsBLL.SingleModel.RemoveEntGoodListCache(aid);


            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            PlatChildGoods platChildGoods = new PlatChildGoods();


            //查找店铺所属平台
            //   TransactionModel tramModelGoodsRelation = new TransactionModel();



            if (act == "copy")
            {
                if (id <= 0)
                {
                    returnObj.Msg = "请选择需要复制的产品";
                    return Json(returnObj);
                }
                platChildGoods = PlatChildGoodsBLL.SingleModel.GetModel(id);
                if (platChildGoods == null || platChildGoods.AId != aid)
                {
                    returnObj.Msg = "复制的产品不存在";
                    return Json(returnObj);
                }
                platChildGoods.Name += "-复制";
                platChildGoods.Addtime = DateTime.Now;
                platChildGoods.Id = 0;


                id = Convert.ToInt32(PlatChildGoodsBLL.SingleModel.Add(platChildGoods));
                if (id > 0)
                {

                    if (PlatChildGoodsBLL.SingleModel.SyncProduct(id, platStore, agentinfo))
                    {

                        returnObj.isok = true;
                        returnObj.Msg = "复制成功";
                        return Json(returnObj);
                    }
                    else
                    {
                        platChildGoods.State = 0;
                        platChildGoods.Updatetime = DateTime.Now;
                        PlatChildGoodsBLL.SingleModel.Update(platChildGoods, "State,Updatetime");
                        returnObj.Msg = "复制异常";
                        return Json(returnObj);
                    }

                }
                else
                {
                    returnObj.Msg = "复制失败";
                    return Json(returnObj);
                }
            }
            else if (act == "tag")
            {
                //单个产品上架或者下架
                //上架或者下架
                if (id <= 0)
                {
                    returnObj.Msg = "请选择需要操作的产品";
                    return Json(returnObj);
                }

                int tag = Context.GetRequestInt("tag", 0);
                platChildGoods = PlatChildGoodsBLL.SingleModel.GetModel(id);
                if (platChildGoods == null || platChildGoods.AId != aid)
                {
                    returnObj.Msg = "产品不存在";
                    return Json(returnObj);
                }

                platChildGoods.Tag = tag;
                platChildGoods.Updatetime = DateTime.Now;
                if (PlatChildGoodsBLL.SingleModel.Update(platChildProduct, "Tag,Updatetime"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = $"操作成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            else if (act == "top")
            {
                //单个产品推荐或者取消推荐
                //上架或者下架
                if (id <= 0)
                {
                    returnObj.Msg = "请选择需要操作的产品";
                    return Json(returnObj);
                }

                int topState = Context.GetRequestInt("topState", 0);
                platChildGoods = PlatChildGoodsBLL.SingleModel.GetModel(id);
                if (platChildGoods == null || platChildGoods.AId != aid)
                {
                    returnObj.Msg = "产品不存在";
                    return Json(returnObj);
                }
                if (topState > 0)
                {
                    if (platChildGoods.Tag == 0)
                    {
                        returnObj.Msg = "该产品已下架，请先上架再推荐";
                        return Json(returnObj);
                    }


                    int curTopCount = PlatChildGoodsBLL.SingleModel.GetTopCount(aid, id);
                    if (curTopCount >= 4)
                    {
                        returnObj.Msg = "推荐商品最多4个已达上限，请先取消部分再推荐";
                        return Json(returnObj);
                    }
                }

                platChildGoods.TopState = topState;
                platChildGoods.Updatetime = DateTime.Now;
                if (PlatChildGoodsBLL.SingleModel.Update(platChildProduct, "TopState,Updatetime"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = $"操作成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            else if (act == "batch")
            {
                //批量操作 上架或者下架 删除 

                int actval = Utility.IO.Context.GetRequestInt("actval", -1);
                string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
                if (string.IsNullOrEmpty(ids))
                {
                    returnObj.Msg = "请先选择产品";
                    return Json(returnObj);
                }

                List<PlatChildGoods> goods = PlatChildGoodsBLL.SingleModel.GetListByIds(ids);
                if (goods == null || !goods.Any())
                {
                    returnObj.Msg = "选择的产品不存在";
                    return Json(returnObj);
                }

                TransactionModel transactionModel = new TransactionModel();
                foreach (PlatChildGoods item in goods)
                {
                    if (actval != -1)
                    {
                        //表示批量上架或者下架
                        item.Updatetime = DateTime.Now;
                        item.Tag = actval;
                        transactionModel.Add(PlatChildGoodsBLL.SingleModel.BuildUpdateSql(item, "Tag,Updatetime"));
                        // TODO 更新购物车
                    }
                    else
                    {
                        //表示批量删除
                        if (item.Tag == 0)
                        {
                            item.Updatetime = DateTime.Now;
                            item.State = 0;
                            transactionModel.Add(PlatChildGoodsBLL.SingleModel.BuildUpdateSql(item, "State,Updatetime"));
                            // TODO 更新购物车
                        }
                        else
                        {

                            returnObj.Msg = "操作异常,请先下架商品才能删除";
                            return Json(returnObj);
                        }

                    }
                }

                if (transactionModel.sqlArray != null && transactionModel.sqlArray.Length > 0 && PlatChildGoodsBLL.SingleModel.ExecuteTransactionDataCorect(transactionModel.sqlArray))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "操作失败";
                    return Json(returnObj);
                }







            }
            else if (act == "del")
            {
                PlatChildGoods model = PlatChildGoodsBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    returnObj.Msg = "请选择需要删除的产品";
                    return Json(returnObj);
                }
                if (model.Tag == 1)
                {
                    returnObj.Msg = "请先下架该产品才能删除";
                    return Json(returnObj);
                }
                model.Updatetime = DateTime.Now;
                model.State = 0;
                if (PlatChildGoodsBLL.SingleModel.Update(model, "State,Updatetime"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "删除成功";
                    return Json(returnObj);

                }
                returnObj.Msg = "删除失败";
                return Json(returnObj);
            }
            else
            {
                //新增或者更新
                if (platChildProduct.Id <= 0)
                {
                    //表示新增
                    platChildProduct.Addtime = DateTime.Now;
                    id = Convert.ToInt32(PlatChildGoodsBLL.SingleModel.Add(platChildProduct));
                    if (id > 0)
                    {
                        platChildProduct.Id = id;
                        if (PlatChildGoodsBLL.SingleModel.SyncProduct(id, platStore, agentinfo))
                        {
                            returnObj.isok = true;
                            returnObj.Msg = "新增成功";
                            return Json(returnObj);
                        }
                        else
                        {
                            platChildProduct.State = 0;
                            platChildProduct.Updatetime = DateTime.Now;
                            PlatChildGoodsBLL.SingleModel.Update(platChildProduct, "State,Updatetime");
                            returnObj.Msg = "新增异常";
                            return Json(returnObj);
                        }


                    }
                    else
                    {
                        returnObj.Msg = "新增失败";
                        return Json(returnObj);
                    }
                }
                else
                {
                    platChildGoods = PlatChildGoodsBLL.SingleModel.GetModel(platChildProduct.Id);
                    if (platChildGoods == null || platChildGoods.AId != aid)
                    {
                        returnObj.Msg = "数据不存在";
                        return Json(returnObj);
                    }
                    platChildProduct.Updatetime = DateTime.Now;
                    if (PlatChildGoodsBLL.SingleModel.Update(platChildProduct))
                    {
                        returnObj.isok = true;
                        returnObj.Msg = "更新成功";
                        return Json(returnObj);
                    }
                    else
                    {
                        returnObj.Msg = "更新失败";
                        return Json(returnObj);

                    }

                }
            }




        }
        
        /// <summary>
        /// 产品排序
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateProductSort()
        {
            int appId = Context.GetRequestInt("appId", 0);
            string datajson = Context.GetRequest("datajson", string.Empty);

            if (string.IsNullOrEmpty(datajson))
            {
                return Json(new { isok = false, msg = "参数错误！" });
            }

            if (PlatChildGoodsBLL.SingleModel.UpdateProductSort(appId, datajson))
            {
                return Json(new { isok = true, msg = "操作成功！" });
            }

            return Json(new { isok = false, msg = "操作失败！" });
        }
        
        /// <summary>
        /// 店铺配置
        /// </summary>
        /// <returns></returns>
        public ActionResult StoreConfig()
        {
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);

            if (appId <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);

            PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(appId, 2);
            if (platStore == null)
                return View("PageError", new PlatReturnMsg() { Msg = "店铺不存在!", code = "404" });

            if (!string.IsNullOrEmpty(platStore.SwitchConfig))
            {
                platStore.SwitchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
            }
            else
            {
                platStore.SwitchModel = new PlatStoreSwitchModel();
            }


            if (!string.IsNullOrEmpty(platStore.StoreService) && platStore.StoreService.Contains("ServiceState"))
            {
                platStore.StoreServiceModelList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StoreServiceModel>>(platStore.StoreService);
            }
            else
            {
                List<StoreServiceModel> list = new List<StoreServiceModel>();
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "WIFI" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "停车位" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "支付宝支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "微信支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "刷卡支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "空调雅座" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "付费停车" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "接送服务" });
                platStore.StoreServiceModelList = list;
            }

            //快速支付
            PlatStoreBLL.SingleModel.GetQuicklyPayQrCode(xcxrelation,ref platStore);
            
            return View(platStore);
        }
        
        [HttpGet]
        public ActionResult Punit(int aid, int pageIndex = 1, int pageSize = 100)
        {


            string strwhere = $"aid={aid} and state=1";
            ViewModel<PlatChildGoodsUnit> vm = new ViewModel<PlatChildGoodsUnit>();

            vm.DataList = PlatChildGoodsUnitBLL.SingleModel.GetList(strwhere, pageSize, pageIndex, "*", "sort desc,id asc");
            vm.TotalCount = PlatChildGoodsUnitBLL.SingleModel.GetCount(strwhere);

            return View(vm);
        }
        [HttpPost]
        public ActionResult Punit(int aid, PlatChildGoodsUnit model)
        {
            returnObj = new PlatReturnMsg();
            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            #region 删除
            if (act == "del")
            {
                if (id <= 0)
                {
                    returnObj.Msg = "请选择需要删除的单位";
                    return Json(returnObj);
                }

                model = PlatChildGoodsUnitBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    returnObj.Msg = "数据不存在";
                    return Json(returnObj);

                }
                model.State = 0;
                if (PlatChildGoodsUnitBLL.SingleModel.Update(model, "State"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "删除成功";
                    return Json(returnObj);

                }
                else
                {
                    returnObj.Msg = "删除失败";
                    return Json(returnObj);
                }
            }
            #endregion

            #region 添加和修改

            if (model.Name.Trim() == "" || model.Name.Trim().Length > 10)
            {
                returnObj.Msg = "单位名称不能为空，且不能超过10个字";
                return Json(returnObj);

            }
            else
            {
                int checkcount = PlatChildGoodsUnitBLL.SingleModel.GetCount($"name=@name and aid={aid} and id not in(0,{model.Id}) and state=1", new MySqlParameter[] { new MySqlParameter("name", model.Name) });
                if (checkcount > 0)
                {
                    returnObj.Msg = "已存在该单位名称，请重新设置！";
                    return Json(returnObj);

                }
            }
            //修改
            if (model.Id > 0)
            {
                if (PlatChildGoodsUnitBLL.SingleModel.Update(model))
                {

                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
            }
            //添加
            else
            {

                int newid = Convert.ToInt32(PlatChildGoodsUnitBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.Id = newid;
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    returnObj.dataObj = model;
                    return Json(returnObj);
                }
            }
            #endregion

            returnObj.Msg = "操作异常";
            return Json(returnObj);
        }

        /// <summary>
        /// 刷新快速支付码
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetStorePayQrcode(int aid=0)
        {
            returnObj = new PlatReturnMsg();
            PlatStore model = PlatStoreBLL.SingleModel.GetModelByAId(aid);
            if (model == null)
            {
                returnObj.Msg = $"门店信息不存在";
                return Json(returnObj);
            }
            if (!string.IsNullOrEmpty(model.SwitchConfig))
            {
                model.SwitchModel = JsonConvert.DeserializeObject<PlatStoreSwitchModel>(model.SwitchConfig);
                model.SwitchModel.StorePayQrcode = string.Empty;
                model.SwitchConfig = JsonConvert.SerializeObject(model.SwitchModel);
                if (!PlatStoreBLL.SingleModel.Update(model, "SwitchConfig"))
                {
                    returnObj.Msg = $"刷新异常";
                    return Json(returnObj);
                }
            }
            returnObj.isok = true;
            returnObj.Msg = $"刷新成功";
            return Json(returnObj);
        }
    }
}