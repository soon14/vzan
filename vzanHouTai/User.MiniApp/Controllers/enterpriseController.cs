using BLL.MiniApp;
using BLL.MiniApp.Ent;
using Core.MiniApp;
using Entity.MiniApp.Ent;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Filters;
using User.MiniApp.Model;
using ent = Entity.MiniApp.Ent;
using Utility;
using Utility.IO;
using MySql.Data.MySqlClient;
using System.Web.Configuration;
using DAL.Base;
using BLL.MiniApp.Footbath;
using Entity.MiniApp.Footbath;
using Entity.MiniApp;
using Entity.MiniApp.Tools;
using Entity.MiniApp.Stores;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Helper;
using Entity.MiniApp.FunctionList;
using BLL.MiniApp.FunList;
using Entity.MiniApp.User;
using BLL.MiniApp.Tools;
using BLL.MiniApp.Conf;

namespace User.MiniApp.Controllers
{


    public class MiappEnterpriseController : enterpriseController
    {

    }
    public class enterpriseController : baseController
    {
        public readonly int MAX_PTYPE_NUM = Convert.ToInt32(WebConfigurationManager.AppSettings["MAX_PTYPE_NUM"]);//产品分类最大数量

        /// <summary>
        ///获取小程序编辑页数据
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult GetPageSetData(int aid = 0)
        {
            Return_Msg returnData = new Return_Msg();

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(aid, dzuserId.ToString());
            if (role == null)
            {
                returnData.Msg = "权限模板不存在";
                return Json(returnData);
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(role.TId);
            if (xcxTemplate == null)
            {
                returnData.Msg = "小程序模板不存在";
                return Json(returnData);
            }

            PageConfig pageConfigModel = new PageConfig();
            ComsConfig comsConfigModel = new ComsConfig();
            ProductMgr productMgrModel = new ProductMgr();
            OperationMgr operationMgrModel = new OperationMgr();
            FuncMgr funcMgrModel = new FuncMgr();
            string tName = "";
            int typeIndex = 0;

            Store store = StoreBLL.SingleModel.GetModelByAId(aid);
            if (store == null)
            {
                returnData.Msg = "store不存在";
                return Json(returnData);
            }
            StoreConfigModel storeConfig = new StoreConfigModel();
            if (!string.IsNullOrEmpty(store.configJson))
            {
                storeConfig = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
            }

            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                //如果数量为0则表示没有开启二级分类模式
                int firstTypeCount = EntGoodTypeBLL.SingleModel.GetCountByAid(role.Id);
                typeIndex = firstTypeCount > 0 ? 0 : -1;

                FunctionList functionList = FunctionListBLL.SingleModel.GetModelByTypeAndVId(xcxTemplate.Type,role.VersionId);
                if (functionList == null)
                {
                    returnData.Msg = "功能权限未设置";
                    return Json(returnData);
                }
                if (!string.IsNullOrEmpty(functionList.PageConfig))
                {
                    pageConfigModel = JsonConvert.DeserializeObject<PageConfig>(functionList.PageConfig);
                }
                if (!string.IsNullOrEmpty(functionList.ComsConfig))
                {
                    comsConfigModel = JsonConvert.DeserializeObject<ComsConfig>(functionList.ComsConfig);
                }
                if (!string.IsNullOrEmpty(functionList.ProductMgr))
                {
                    productMgrModel = JsonConvert.DeserializeObject<ProductMgr>(functionList.ProductMgr);
                }
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgrModel = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }
                if (!string.IsNullOrEmpty(functionList.FuncMgr))
                {
                    funcMgrModel = JsonConvert.DeserializeObject<FuncMgr>(functionList.FuncMgr);
                }
                tName += AgentdepositLogBLL.SingleModel.GetVerName(role.VersionId);
            }

            EntSetting pageModel = EntSettingBLL.SingleModel.GetModel(aid);
            if (pageModel == null)
                pageModel = new EntSetting();
            if (pageModel.pages == "")
            {
                pageModel.pages = "[]";
            }

            //小类
            List<EntGoodType> pmintypes = EntGoodTypeBLL.SingleModel.GetListByAidParentId(aid, 1, 200, false);
            List<EntGoodType> ptypes = pmintypes;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                ptypes = EntGoodTypeBLL.SingleModel.GetListByAidParentId(aid, 1, 200, true);
            }

            List<EntNewsType> newstypes = EntNewsTypeBLL.SingleModel.GetListData(aid, 50, 1, false);
            List<EntNewsType> news2types = newstypes;
            if (store.NewsTypeLevel != 0)
            {
                //二级分类模式 资讯列表组件选择的是大类
                news2types = EntNewsTypeBLL.SingleModel.GetListData(aid, 1, 50, true);
            }

            List<EntIndutypes> pexttypes = EntIndutypesBLL.SingleModel.GetListByAidIndutye(aid, role.Industr);

            returnData.dataObj = new
            {
                typeIndex,
                store.funJoinModel,
                storeConfig,
                pageConfigModel,
                comsConfigModel,
                productMgrModel,
                operationMgrModel,
                funcMgrModel,
                tName,
                pageModel,
                pmintypes,
                ptypes,
                newstypes,
                news2types,
                pexttypes,
            };

            return Json(returnData);
        }


        public int PageType { get; set; }
        public enterpriseController()
        {
            PageType = 12;

        }

        [LoginFilter]
        public virtual ActionResult pageset(int appId = 0)
        {
            InityyForm(appId);
            EntSetting model = EntSettingBLL.SingleModel.GetModel(appId);
            if (model == null)
                model = new EntSetting();
            ViewBag.accountid = dzuserId;

            ViewModel<EntSetting> vm = new ViewModel<EntSetting>();
            vm.PageType = PageType;
            vm.DataModel = model;
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;
            int firstTypeCount = EntGoodTypeBLL.SingleModel.GetCountByAid(appId);
            ViewBag.TypeIndex = firstTypeCount > 0 ? 0 : -1;//如果数量为0则表示没有开启二级分类模式


            if (PageType == (int)TmpType.小程序行业模板 && souceFrom == "")//&& !WebSiteConfig.CustomerLoginId.Contains(dzaccount.LoginId)
            {
                return Redirect($"/config/functionlist?appId={appId}&type={PageType}");
            }
            return View("../enterprise/pageset", vm);
        }

        [LoginFilter, HttpPost, ValidateInput(false)]
        public ActionResult pageset(int appId, EntSetting model)
        {
            string act = Context.GetRequest("act", "");
            int pagetype = Context.GetRequestInt("pagetype", -1);
            int storeId = Context.GetRequestInt("storeId", -1);
            int syncmainsite = Context.GetRequestInt("syncmainsite", 0);
            string extraConfig = Context.GetRequest("extraConfig", "");
            int templateid = Context.GetRequestInt("templateid", 0);

            model.aid = appId;
            model.syncmainsite = syncmainsite;
            //保存
            if (act == "save")
            {
                try
                {
                    List<EntPage> pageModels = JsonConvert.DeserializeObject<List<EntPage>>(model.pages);
                    if (pageModels == null || pageModels.Count <= 0)
                    {
                        return Json(new { isok = false, msg = "数据不存在，保存失败" });
                    }
                }
                catch (Exception)
                {
                    return Json(new { isok = false, msg = "数据错误，保存失败" });
                }
                if (pagetype == 26)
                {
                    //表示多门店
                    FootBath footBath = FootBathBLL.SingleModel.GetModel(storeId);
                    if (footBath == null)
                        return Json(new { isok = false, msg = "保存失败(请先开通多门店)" });
                    footBath.UpdateDate = DateTime.Now;
                    footBath.StoreMaterialPages = model.pages;
                    if (!FootBathBLL.SingleModel.Update(footBath, "StoreMaterialPages,UpdateDate"))
                        return Json(new { isok = false, msg = "保存失败" });
                    return Json(new { isok = true, msg = "保存成功！" });
                }

                if (EntSettingBLL.SingleModel.Exists($"aid={appId}"))
                {
                    //保存旧装修模板数据
                    if (templateid > 0)
                    {
                        EntSetting pageSetting = EntSettingBLL.SingleModel.GetModel(appId);
                        //保存旧的配置数据
                        CustomModelEntSetting oldentsetting = new CustomModelEntSetting();
                        oldentsetting.AddTime = DateTime.Now;
                        oldentsetting.AId = pageSetting.aid;
                        oldentsetting.ConfigJson = pageSetting.configJson;
                        oldentsetting.Pages = pageSetting.pages;
                        oldentsetting.MeConfigJson = pageSetting.MeConfigJson;
                        oldentsetting.Syncmainsite = pageSetting.syncmainsite;
                        oldentsetting.UpdateTime = DateTime.Now;
                        oldentsetting.Id = Convert.ToInt32(CustomModelEntSettingBLL.SingleModel.Add(oldentsetting));

                        CustomModelUserRelation crelation = CustomModelUserRelationBLL.SingleModel.GetModelByAId(appId);
                        if (crelation == null)
                        {
                            crelation = new CustomModelUserRelation();
                            crelation.AId = appId;
                            crelation.PreviousId = 0;
                            crelation.AddTime = DateTime.Now;
                            crelation.CustomModelId = templateid;
                            crelation.UpdateTime = DateTime.Now;
                            CustomModelUserRelationBLL.SingleModel.Add(crelation);
                        }
                        else
                        {
                            crelation.SettingId = oldentsetting.Id;
                            crelation.PreviousId = crelation.CustomModelId;
                            crelation.CustomModelId = templateid;
                            crelation.UpdateTime = DateTime.Now;
                            CustomModelUserRelationBLL.SingleModel.Update(crelation, "PreviousId,CustomModelId,UpdateTime,SettingId");
                        }
                        CustomModelRelationBLL.SingleModel.RemoveCache();
                    }
                    model.updatetime = DateTime.Now;
                    if (!EntSettingBLL.SingleModel.Update(model, "pages,updatetime,meconfigjson,syncmainsite"))
                    {
                        return Json(new { isok = false, msg = "保存失败" });
                    }
                }
                else
                {
                    int newid = Convert.ToInt32(EntSettingBLL.SingleModel.Add(model));
                    if (newid <= 0)
                    {
                        return Json(new { isok = false, msg = "保存失败" });
                    }
                }

                //若有传入额外配置更新内容,则更新配置
                if (!string.IsNullOrWhiteSpace(extraConfig))
                {
                    Store store = StoreBLL.SingleModel.GetModelByRid(appId);
                    if (store != null)
                    {
                        #region 处理更新店铺配置
                        try
                        {
                            store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
                        }
                        catch (Exception)
                        {
                            store.funJoinModel = new StoreConfigModel();
                        }

                        StoreConfigModel config = null;
                        try
                        {
                            config = JsonConvert.DeserializeObject<StoreConfigModel>(extraConfig);
                        }
                        catch (Exception)
                        {
                        }
                        store.funJoinModel.openWxShopMessage = config.openWxShopMessage;
                        store.configJson = JsonConvert.SerializeObject(store.funJoinModel);
                        #endregion

                        store.UpdateDate = DateTime.Now;
                        StoreBLL.SingleModel.Update(store, "configJson,UpdateDate");
                    }
                }

                return Json(new { isok = true, msg = "保存成功！" });
            }
            return View();
        }

        [LoginFilter]
        public ActionResult p(int appId = 0, int pageIndex = 1, int pageSize = 20)
        {



            ViewModel<ent.EntGoods> vm = new ViewModel<ent.EntGoods>();
            try
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

                string strwhere = $"aid={appId} and state=1 and goodtype={(int)EntGoodsType.普通产品}";
                int curProductCount = EntGoodsBLL.SingleModel.GetCount(strwhere);
                bool notAdd = false;
                int versionId = 0;
                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    FunctionList functionList = new FunctionList();
                    versionId = app.VersionId;
                    functionList = FunctionListBLL.SingleModel.GetModelByTypeAndVId(xcxTemplate.Type,versionId);
                    if (functionList == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "此功能未开启!", code = "500" });
                    }

                    ProductMgr productMgrModel = new ProductMgr();
                    if (!string.IsNullOrEmpty(functionList.ProductMgr))
                    {
                        productMgrModel = JsonConvert.DeserializeObject<ProductMgr>(functionList.ProductMgr);
                    }
                    notAdd = curProductCount > productMgrModel.ProductMaxCount;

                }
                ViewBag.versionId = versionId;
                ViewBag.notAdd = notAdd;
                #endregion


                string search = Context.GetRequest("search", "");
                int plabels = Context.GetRequestInt("plabels", 0);
                int ptype = Context.GetRequestInt("ptype", 0);
                int ptag = Context.GetRequestInt("ptag", -1);
                string stockNo = Context.GetRequest("stockNo", null);

                int count = 0;
                vm.DataList = EntGoodsBLL.SingleModel.GetListByRedis(appId, search, plabels, ptype, ptag, pageIndex, pageSize, ref count, stockNo: stockNo);
                vm.TotalCount = count;
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;

                ViewBag.goodTypeList = EntGoodTypeBLL.SingleModel.GetList(string.Format("aid={0}", appId));
                ViewBag.goodLabelList = EntGoodLabelBLL.SingleModel.GetList(string.Format("aid={0}", appId));

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }

            return View(vm);
        }

        [LoginFilter, HttpPost]
        public ActionResult plist(int appId, int pageIndex = 1, int pageSize = 20, int goodType = 0, int grouptype = 0)
        {
            int storeId = Context.GetRequestInt("storeId", 0);
            ViewModel<ent.EntGoods> vm = new ViewModel<ent.EntGoods>();
            string strwhere = $"aid={appId} and state=1 and tag=1 and goodtype={grouptype}";
            string strSort = " sort desc, id desc ";
            if (PageType == 14)
            {
                strwhere += " and tag=1 ";
            }

            int firstTypeCount = 0;
            if (PageType == 22)
            {
                firstTypeCount = EntGoodTypeBLL.SingleModel.GetCountByAid(appId);//查询是否开启二级分类
            }


            if (goodType > 0)
            {
                if (firstTypeCount <= 0)
                {
                    strwhere += $" and FIND_IN_SET('{goodType}',ptypes) ";
                }
                else
                {
                    List<string> listids = new List<string>();
                    string typeid = goodType.ToString();
                    List<EntGoodType> listGoodType = EntGoodTypeBLL.SingleModel.GetList($"aid={appId} and state=1 and parentid in({goodType})");
                    // log4net.LogHelper.WriteInfo(this.GetType(), $"aid={aid} and state=1 and parentid in({typeid});{listGoodType.Count}");
                    foreach (EntGoodType item in listGoodType)
                    {
                        listids.Add(item.id.ToString());
                    }
                    if (listids.Count <= 0)
                    {
                        typeid = "0";
                    }
                    else
                    {
                        typeid = string.Join(",", listids);
                    }

                    List<string> typeidSplit = typeid.SplitStr(",");
                    if (typeidSplit.Count > 0)
                    {
                        typeidSplit = typeidSplit.Select(p => p = "FIND_IN_SET('" + p + "',ptypes)").ToList();
                        strwhere += $" and (" + string.Join(" or ", typeidSplit) + ")";
                    }
                }

            }
            string search = Context.GetRequest("search", "");
            if (search.Trim() == "")
            {
                vm.DataList = EntGoodsBLL.SingleModel.GetList(strwhere, pageSize, pageIndex, "*", strSort);
            }
            else
            {
                strwhere += $" and name like @name";

                log4net.LogHelper.WriteInfo(GetType(), strwhere);
                vm.DataList = EntGoodsBLL.SingleModel.GetListByParam(strwhere,
                    new MySqlParameter[] { new MySqlParameter("@name", $"%{search}%") },
                    pageSize, pageIndex, "*", strSort);
            }
            int _totalcount = EntGoodsBLL.SingleModel.GetCount(strwhere,
                    new MySqlParameter[] { new MySqlParameter("@name", $"%{search}%") });

            vm.TotalCount = _totalcount;
            vm.PageCount = Utility.Paging.PageInfo.GetPageCount(_totalcount, pageSize);
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            vm.DataList.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.ptypes))
                {
                    string sql = $"SELECT GROUP_CONCAT(`name`) from entgoodtype where FIND_IN_SET(id,@ptypes)";
                    x.ptypestr = DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                        CommandType.Text, sql,
                        new MySqlParameter[] { new MySqlParameter("@ptypes", x.ptypes) }).ToString();
                }


                if (!string.IsNullOrEmpty(x.plabels))
                {
                    x.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT group_concat(name) from entgoodlabel where id in ({x.plabels})").ToString();
                    x.plabelstr_array = x.plabelstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                if (SubStoreEntGoodsBLL.SingleModel.Exists($"Pid={x.id} and aid={appId} and storeid={storeId} and SubState=1"))
                {
                    x.enable = false;
                }
                else
                {
                    x.enable = true;
                }
            });

            return Content(JsonConvert.SerializeObject(vm, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
        }

        [HttpPost]
        public ActionResult coupinlist(int appId, int pageIndex = 1, int pageSize = 20, int storeId = 0)
        {
            string couponname = Context.GetRequest("couponname", "");

            ViewModel<Coupons> vm = new ViewModel<Coupons>();
            List<Coupons> couponlist = CouponsBLL.SingleModel.GetCouponList(couponname, 2, storeId, appId, TicketType.优惠券, pageSize, pageIndex, "id desc");

            vm.TotalCount = CouponsBLL.SingleModel.GetCouponListCount(couponname, 2, storeId, appId, TicketType.优惠券);
            vm.PageCount = Utility.Paging.PageInfo.GetPageCount(vm.TotalCount, pageSize);
            vm.DataList = couponlist;
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;

            return Content(JsonConvert.SerializeObject(vm, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
        }


        [LoginFilter, HttpGet]
        public virtual ActionResult pedit()
        {
            int id = Context.GetRequestInt("id", 0);
            EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(id);
            if (goodModel == null)
                goodModel = new EntGoods();

            int appid = Utility.IO.Context.GetRequestInt("appId", 0);
            int firstTypeCount = 0;
            ViewBag.PageType = 12;
            ViewBag.FirstEntGoodTypeList = EntGoodTypeBLL.SingleModel.GetListByCach(appid, 50, 1, ref firstTypeCount, 0);
            ViewBag.SecondTypeSwitch = firstTypeCount;

            return View("../enterprise/pedit", goodModel);
        }

        [LoginFilter, HttpPost, ValidateInput(false)]
        public virtual ActionResult pedit(int appId, ent.EntGoods model)
        {
            //清除产品列表缓存
            EntGoodsBLL.SingleModel.RemoveEntGoodListCache(appId);

            string act = Context.GetRequest("act", "");
            if (act == "copy")
            {
                int pid = Context.GetRequestInt("id", 0);
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
                if (!model.name.EndsWith("复制"))
                {
                    model.name = model.name + " 复制";
                }

                int newid = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    return Json(new { isok = true, msg = newid });
                }

            }
            else if (act == "del")
            {
                int pid = Context.GetRequestInt("id", 0);
                model = EntGoodsBLL.SingleModel.GetModel($"id={pid}");
                model.state = 0;
                if (EntGoodsBLL.SingleModel.Update(model, "state"))
                {

                    EntSettingBLL.SingleModel.SyncData(appId, "$..coms[?(@.type=='good')].items[?(@.id==" + pid + ")]");

                    return Json(new { isok = true, msg = "删除成功" });
                }
                return Json(new { isok = false, msg = "删除失败" });
            }

            if (model.id == 0)
            {
                int newid = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(model));
                model.id = newid;
                if (newid > 0)
                {
                    return Json(new { isok = true, msg = "添加成功" });
                }
                if (model == null)
                {
                    return Json(new { isok = false, msg = "复制失败，产品不存在或已删除" });
                }
            }
            else
            {
                if (EntGoodsBLL.SingleModel.Update(model))
                {
                    return Json(new { isok = true, msg = "修改成功" });
                }
            }
            return Json(new { isok = false, msg = "操作失败" });
        }

        [LoginFilter, HttpGet]
        public ActionResult ptype(int appId = 0, int pageIndex = 1, int pageSize = 200, int typeIndex = -1)
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

            int productTypeSwtich = 0;//产品类别开关
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
                    productTypeSwtich = productMgr.ProductType;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.productTypeSwtich = productTypeSwtich;
            #endregion

            ViewModel<EntGoodType> vm = new ViewModel<EntGoodType>();

            int count = 0;
            int firstTypeCount = 0;
            vm.DataList = EntGoodTypeBLL.SingleModel.GetListByCach(appId, pageSize, pageIndex, ref count, typeIndex);
            vm.FirstDataList = EntGoodTypeBLL.SingleModel.GetListByCach(appId, pageSize, pageIndex, ref firstTypeCount, 0);
            vm.SecondTypeSwitch = firstTypeCount;
            vm.TotalCount = count;
            vm.PageType = PageType;
            vm.TypeIndex = typeIndex;

            EntSetting entSetting = EntSettingBLL.SingleModel.GetModel(appId);
            if (entSetting != null)
            {
                vm.pageSetting = entSetting.pages;
            }

            return View("../enterprise/ptype", vm);
        }

        [LoginFilter, HttpPost]
        public ActionResult ptype(int appId = 0, EntGoodType model = null, int isIniti = 0)
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


            //清除列表缓存
            EntGoodTypeBLL.SingleModel.RemoveEntGoodTypeListCache(appId);


            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            #region 删除
            if (act == "del")
            {
                if (id <= 0)
                {
                    return Json(new { isok = false, msg = "非法请求！" });
                }
                //检查是否有已经有产品使用了分类
                int checkcount = EntGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({id},ptypes)>0 and aid={appId} and state=1");
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = $"该分类下已有{checkcount}个产品，不可删除！" });
                }

                model = EntGoodTypeBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "该分类不存在！" });
                }

                //当前删除分类是否是一级分类，如果是先查看是否有二级分类，如果有则先提示删除二级分类才能删除该一级分类

                if (model.parentId == 0)
                {
                    //表示一级分类
                    checkcount = EntGoodTypeBLL.SingleModel.GetCount($"parentId={model.id} and aid={appId} and state=1");
                    if (checkcount > 0)
                    {
                        return Json(new { isok = false, msg = $"该分类下已有{checkcount}个二级分类,不可删除,请先删其下的二级分类！" });
                    }
                }



                model.state = 0;

                if (EntGoodTypeBLL.SingleModel.Update(model, "state"))
                {
                    EntSettingBLL.SingleModel.SyncData(appId, "$..goodCat[?(@.id==" + id + ")]");

                    return Json(new { isok = true, msg = "删除成功！" });
                }
                else
                {
                    return Json(new { isok = true, msg = "删除失败！" });
                }
            }
            #endregion

            #region 添加和修改
            if (model == null || model.id < 0)
            {
                return Json(new { isok = false, msg = "非法请求" });
            }
            if (model.name.Trim() == "" || model.name.Trim().Length > 10)
            {
                return Json(new { isok = false, msg = "分类名称不能为空，且不能超过10个字" });
            }
            else
            {
                #region 专业版 版本控制
                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
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
                    if (productMgrModel.ProductType == 1)//表示关闭了添加类别功能
                    {
                        return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                    }

                }
                #endregion

                int checkcount = EntGoodTypeBLL.SingleModel.GetCount($"name=@name and aid={appId} and id not in(0,{model.id}) and state=1 and parentId={model.parentId}", new MySqlParameter[] { new MySqlParameter("name", model.name) });
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = "已存在该分类名称，请重新设置！" });
                }



            }
            //修改
            if (model.id > 0)
            {
                if (EntGoodTypeBLL.SingleModel.Update(model))
                {
                    return Json(new { isok = true, msg = model });
                }
            }
            //添加
            else
            {

                int maxCount = MAX_PTYPE_NUM;
                string countWhere = $"aid={appId} and state=1 and parentId<>0";//表示二级分类
                if (model.parentId == 0)
                {
                    //表示添加一级分类 限制为30个
                    maxCount = 30;
                    countWhere = $"aid={appId} and state=1 and parentId=0";
                }
                int checkcount = EntGoodTypeBLL.SingleModel.GetCount(countWhere);
                if (checkcount >= maxCount)
                {
                    return Json(new { isok = false, msg = "无法新增分类！您已添加了" + maxCount + "个产品分类，已达到上限，请编辑已有的分类或删除部分分类后再进行新增。" });
                }

                int newid = Convert.ToInt32(EntGoodTypeBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.id = newid;

                    //1.更新初始化小类在大类里  2.页面配置里的产品类别归类为默认分类
                    if (isIniti != 0)
                    {
                        int secondTypeCount = EntGoodTypeBLL.SingleModel.GetCount($"aid={appId} and parentId=-1");
                        if (secondTypeCount > 0)
                        {
                            EntGoodTypeBLL.SingleModel.isInitiType(appId, newid);
                        }
                    }

                    return Json(new { isok = true, msg = model });
                }
            }
            #endregion

            return Json(new { isok = false, msg = "操作失败！" });
        }

        [LoginFilter, HttpPost]
        public ActionResult updatesort()
        {
            int appId = Context.GetRequestInt("appId", 0);
            string datajson = Context.GetRequest("datajson", string.Empty);
            int type = Context.GetRequestInt("type", 0);

            if (string.IsNullOrEmpty(datajson))
            {
                return Json(new { isok = false, msg = "参数错误！" });
            }

            if (EntGoodTypeBLL.SingleModel.UpdateSort(appId, datajson, type))
            {
                return Json(new { isok = true, msg = "操作成功！" });
            }

            return Json(new { isok = false, msg = "操作失败！" });
        }

        [LoginFilter, HttpGet]
        public ActionResult plabel(int appId, int pageIndex = 1, int pageSize = 100)
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

            int labelSwtich = 0;//产品标签开关
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
                    labelSwtich = productMgr.ProductLabel;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.labelSwtich = labelSwtich;
            #endregion

            ViewModel<EntGoodLabel> vm = new ViewModel<EntGoodLabel>();
            int count = 0;
            vm.DataList = EntGoodLabelBLL.SingleModel.GetListByCach(appId, pageSize, pageIndex, ref count);
            vm.TotalCount = count;
            vm.PageType = PageType;
            return View("../enterprise/plabel", vm);
        }

        [LoginFilter, HttpPost]
        public ActionResult plabel(int appId, EntGoodLabel model)
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






            //清除缓存
            EntGoodLabelBLL.SingleModel.RemoveEntGoodLabelListCache(appId);

            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            #region 删除
            if (act == "del")
            {
                if (id <= 0)
                {
                    return Json(new { isok = false, msg = "非法请求！" });
                }
                //检查是否有已经有产品使用了标签
                int checkcount = EntGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({id},plabels)>0 and aid={appId} and state=1");
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = $"该标签下已有{checkcount}个产品，不可删除！" });
                }

                model = EntGoodLabelBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "该标签不存在！" });
                }
                model.state = 0;
                if (EntGoodLabelBLL.SingleModel.Update(model, "state"))
                {
                    return Json(new { isok = true, msg = "删除成功！" });
                }
                else
                {
                    return Json(new { isok = true, msg = "删除失败！" });
                }
            }
            #endregion

            #region 添加和修改
            if (model == null || model.id < 0)
            {
                return Json(new { isok = false, msg = "非法请求" });
            }
            if (model.name.Trim() == "" || model.name.Trim().Length > 10)
            {
                return Json(new { isok = false, msg = "分类名称不能为空，且不能超过10个字" });
            }
            else
            {
                int checkcount = EntGoodLabelBLL.SingleModel.GetCount($"name=@name and aid={appId} and id not in(0,{model.id}) and state=1", new MySqlParameter[] { new MySqlParameter("name", model.name) });
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = "已存在该分类名称，请重新设置！" });
                }
            }
            //修改
            if (model.id > 0)
            {
                if (EntGoodLabelBLL.SingleModel.Update(model))
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
                    if (productMgrModel.ProductLabel == 1)//表示关闭了添加标签功能
                    {
                        return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                    }

                }
                #endregion

                //不能超过100个
                int checkcount = EntGoodLabelBLL.SingleModel.GetCount($"aid={appId} and state=1");
                if (checkcount >= 100)
                {
                    return Json(new { isok = false, msg = "无法新增标签！您已添加了100个标签分类，已达到上限，请编辑已有的标签或删除部分标签后再进行新增。" });
                }
                int newid = Convert.ToInt32(EntGoodLabelBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.id = newid;
                    return Json(new { isok = true, msg = model });
                }
            }
            #endregion

            return Json(new { isok = false, msg = "操作失败！" });
        }


        [LoginFilter, HttpGet]
        public ActionResult punit(int appId, int pageIndex = 1, int pageSize = 25)
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


            }
            ViewBag.versionId = versionId;

            #endregion



            string strwhere = $"aid={appId} and state=1";
            ViewModel<EntGoodUnit> vm = new ViewModel<EntGoodUnit>();

            vm.DataList = EntGoodUnitBLL.SingleModel.GetList(strwhere, pageSize, pageIndex, "*", "sort desc,id asc");
            vm.TotalCount = EntGoodUnitBLL.SingleModel.GetCount(strwhere);
            vm.PageType = PageType;
            return View("../enterprise/punit", vm);
        }
        [LoginFilter, HttpPost]
        public ActionResult punit(int appId, EntGoodUnit model)
        {

            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            #region 删除
            if (act == "del")
            {
                if (id <= 0)
                {
                    return Json(new { isok = false, msg = "非法请求！" });
                }

                model = EntGoodUnitBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "该单位不存在！" });
                }
                model.state = 0;
                if (EntGoodUnitBLL.SingleModel.Update(model, "state"))
                {
                    return Json(new { isok = true, msg = "删除成功！" });
                }
                else
                {
                    return Json(new { isok = true, msg = "删除失败！" });
                }
            }
            #endregion

            #region 添加和修改
            if (model == null || model.id < 0)
            {
                return Json(new { isok = false, msg = "非法请求" });
            }
            if (model.name.Trim() == "" || model.name.Trim().Length > 10)
            {
                return Json(new { isok = false, msg = "单位名称不能为空，且不能超过10个字" });
            }
            else
            {
                int checkcount = EntGoodUnitBLL.SingleModel.GetCount($"name=@name and aid={appId} and id not in(0,{model.id}) and state=1", new MySqlParameter[] { new MySqlParameter("name", model.name) });
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = "已存在该单位名称，请重新设置！" });
                }
            }
            //修改
            if (model.id > 0)
            {
                if (EntGoodUnitBLL.SingleModel.Update(model))
                {
                    return Json(new { isok = true, msg = model });
                }
            }
            //添加
            else
            {
                //不能超过12个
                //int checkcount = EntGoodLabelBLL.SingleModel.GetCount($"aid={appId} and state=1");
                //if (checkcount >= 25)
                //{
                //    return Json(new { isok = false, msg = "无法新增标签！您已添加了25个标签分类，已达到上限，请编辑已有的标签或删除部分标签后再进行新增。" });
                //}
                int newid = Convert.ToInt32(EntGoodUnitBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.id = newid;
                    return Json(new { isok = true, msg = model });
                }
            }
            #endregion

            return Json(new { isok = false, msg = "操作失败！" });
        }

        [LoginFilter]
        public ActionResult appform(int appId, int pageIndex = 1, int pageSize = 20)
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

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = app.VersionId;
            }

            ViewBag.versionId = versionId;
            #endregion


            ViewModel<EntUserForm> vm = new ViewModel<EntUserForm>();
            string strwhere = $"aid={appId} and state!=0 and Type=0";
            string act = Context.GetRequest("act", "");
            int state = Context.GetRequestInt("state", -999);
            if (state != -999)
            {
                strwhere += $" and state={state} ";
            }
            string start = Context.GetRequest("start", string.Empty);
            string end = Context.GetRequest("end", string.Empty);
            if (act == "search" && !string.IsNullOrEmpty(start))
            {

                if (string.IsNullOrEmpty(end))
                {
                    end = DateTime.Now.ToString("yyyy-MM-dd");
                }
                end = end + " 23:59:59";

                strwhere += $" and addtime between @start and @end ";
                vm.DataList = EntUserFormBLL.SingleModel.GetListByParam(strwhere, new MySqlParameter[] {
                        new MySqlParameter("@start",start),
                        new MySqlParameter("@end",end)
                    }, pageSize, pageIndex, "*", " id desc ");
            }
            else
            {
                vm.DataList = EntUserFormBLL.SingleModel.GetList(strwhere, pageSize, pageIndex, "*", " id desc ");
                vm.TotalCount = EntUserFormBLL.SingleModel.GetCount(strwhere);
            }

            //获取店铺二维码名称
            List<EntUserForm> userformlist = vm.DataList;
            EntStoreCodeBLL.SingleModel.GetStoreCodeName<EntUserForm>(ref userformlist);
            userformlist.ForEach(form => form.formremark = string.IsNullOrEmpty(form.remark) ? new EntFormRemark() : JsonConvert.DeserializeObject<EntFormRemark>(form.remark));
            vm.DataList = userformlist;

            vm.PageType = PageType;
            return View("../enterprise/appform", vm);
        }

        [LoginFilter, HttpGet]
        public void appformexport(int appId)
        {
            string act = Context.GetRequest("act", "");
            DataTable exportTable = new DataTable();
            exportTable.Columns.AddRange(new DataColumn[] {
                        new DataColumn("表单名称"),
                        new DataColumn("表单详情"),
                        new DataColumn("提交时间"),
                    });
            string filename = "表单导出" + "-" + DateTime.Now.ToString("yyyy-MM-dd");
            if (act == "all")
            {
                string sql = $"select * from EntUserForm where  aid={appId} and state=1 order by id desc ";
                DataSet ds = DAL.Base.SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow item in ds.Tables[0].Rows)
                    {
                        DataRow dr = exportTable.NewRow();
                        dr[0] = item["comename"].ToString();
                        dr[1] = item["formdatajson"].ToString().Replace("{", "").Replace("}", "").Replace("\"", "");
                        dr[2] = item["addtime"].ToString();
                        exportTable.Rows.Add(dr);
                    }

                }
            }
            else if (act == "sel")
            {
                string ids = Context.GetRequest("ids", "");
                ids = Server.UrlDecode(ids);
                ids = Utility.EncodeHelper.ReplaceSqlKey(ids);
                if (ids.IsNullOrEmpty())
                {
                    Response.Write("请选择要导出的数据");
                    return;
                }
                string sql = $"select * from EntUserForm where  aid={appId} and id in({ids}) and state=1 order by id desc ";
                DataSet ds = DAL.Base.SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow item in ds.Tables[0].Rows)
                    {
                        DataRow dr = exportTable.NewRow();
                        dr[0] = item["comename"].ToString();
                        dr[1] = item["formdatajson"].ToString().Replace("{", "").Replace("}", "").Replace("\"", "");
                        dr[2] = item["addtime"].ToString();
                        exportTable.Rows.Add(dr);
                    }

                }
            }
            else if (act == "range")
            {
                string start = Context.GetRequest("start", string.Empty);
                string end = Context.GetRequest("end", string.Empty);
                int state = Context.GetRequestInt("state", -999);
                if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                {
                    Response.Write("请选择开始时间和结束时间");
                    return;
                }
                end = end + " 23:59:59";
                string sqlwhere = " and state!=0";
                if (state != -999)
                {
                    sqlwhere = $" and state={state}";
                }
                string sql = $"select * from EntUserForm where  aid={appId} {sqlwhere} and addtime between @start and @end order by id desc";
                DataSet ds = DAL.Base.SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, new MySqlParameter[] {
                        new MySqlParameter("@start",start),
                        new MySqlParameter("@end",end)
                    });
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow item in ds.Tables[0].Rows)
                    {
                        DataRow dr = exportTable.NewRow();
                        dr[0] = item["comename"].ToString();
                        dr[1] = item["formdatajson"].ToString().Replace("{", "").Replace("}", "").Replace("\"", "");
                        dr[2] = item["addtime"].ToString();
                        exportTable.Rows.Add(dr);
                    }

                }
                filename = "表单导出" + start + "到" + end;
            }
            if (exportTable.Rows.Count <= 0)
            {
                Response.Write("没有数据");
                return;
            }

            ExcelHelper<EntUserForm>.Out2Excel(exportTable, filename);//导出
        }

        [LoginFilter, HttpPost]
        public ActionResult appform(int appId)
        {
            string act = Context.GetRequest("act", "");
            int id = Context.GetRequestInt("id", 0);
            if (act == "" || id == 0)
            {
                return Json(new { isok = false, msg = "非法请求！" });
            }
            if (act == "del")
            {
                EntUserForm userFormModel = EntUserFormBLL.SingleModel.GetModel(id);
                if (userFormModel == null)
                {
                    return Json(new { isok = false, msg = "数据不存在！" });
                }
                userFormModel.state = 0;
                if (EntUserFormBLL.SingleModel.Update(userFormModel, "state"))
                {
                    return Json(new { isok = true, msg = "删除成功！" });
                }

                return Json(new { isok = false, msg = "删除失败！" });
            }
            return Json(new { isok = false, msg = "" });
        }

        [LoginFilter, HttpPost]
        public ActionResult UpdateNewsTypeLevel(int appId = 0)
        {
            Return_Msg returnObj = new Return_Msg();
            if (appId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            Store storeModel = StoreBLL.SingleModel.GetModelByAId(appId);
            if (storeModel == null)
            {
                returnObj.Msg = "店铺信息错误";
                return Json(returnObj);
             
            }
            storeModel.NewsTypeLevel = storeModel.NewsTypeLevel == 0 ? 1 : 0;




            if (StoreBLL.SingleModel.Update(storeModel, "NewsTypeLevel"))
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



        [LoginFilter, HttpGet]
        public ActionResult newstype(int appId, int pageIndex = 1, int pageSize = 20)
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

            Store storeModel = StoreBLL.SingleModel.GetModelByAId(appId);
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "店铺信息错误!", code = "403" });
            }

            int newsTypeSwtich = 0;//产品类别开关
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
                if (!string.IsNullOrEmpty(functionList.NewsMgr))
                {
                    NewsMgr NewsMgr = JsonConvert.DeserializeObject<NewsMgr>(functionList.NewsMgr);
                    newsTypeSwtich = NewsMgr.NewsType;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.newsTypeSwtich = newsTypeSwtich;
            #endregion

            ViewBag.NewsTypeLevel = storeModel.NewsTypeLevel;//新闻资讯分类级别开关 默认为0表示小类 1表示大类
            int typeIndex = Context.GetRequestInt("typeIndex", 1);
            ViewModel<EntNewsType> vm = new ViewModel<EntNewsType>();
            string strwhere = $"aid={appId} and state=1";//默认获取小类的数据

            //if (typeIndex == 0)
            //{
            //    strwhere = $"aid={appId} and state=1 and parentId=0";
            //}

           

            vm.DataList = EntNewsTypeBLL.SingleModel.GetList(strwhere, pageSize, pageIndex, "*", " id asc ");
            vm.DataList.ForEach(x =>
            {
                x.ParentName = EntNewsTypeBLL.SingleModel.GetEntNewsTypeName(x.ParentId.ToString());
            });
            vm.TotalCount = EntNewsTypeBLL.SingleModel.GetCount(strwhere);
            vm.PageType = PageType;
            vm.FirstDataList = EntNewsTypeBLL.SingleModel.GetList($"aid={appId} and state=1 and parentId=0", pageSize, pageIndex, "*", " id asc ");
            //if (EntNewsTypeBLL.SingleModel.GetCountByType(appId, 0) <= 0)
            //{
            //    //表示还未初始化数据  则新增一条默认大类,其它的归属该大类
            //    int parentId = Convert.ToInt32(EntNewsTypeBLL.SingleModel.Add(new EntNewsType()
            //    {
            //        aid = appId,
            //        name = "默认大类",
            //        sort = 99,
            //        state = 1,
            //        ParentId = 0
            //    }));

            //    if (parentId <= 0)
            //    {
            //        return View("PageError", new Return_Msg() { Msg = "初始化大类失败!", code = "500" });
            //    }
            //    EntNewsTypeBLL.SingleModel.InitiType(appId, parentId);
            //}

            vm.TypeIndex = typeIndex;

            return View("../enterprise/newstype", vm);
        }

        [LoginFilter, HttpPost]
        public ActionResult newstype(int appId, EntNewsType model)
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

            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            #region 删除
            if (act == "del")
            {
                if (id <= 0)
                {
                    return Json(new { isok = false, msg = "非法请求！" });
                }
                //检查是否有已经有产品使用了分类
                int checkcount = EntNewsBLL.SingleModel.GetCount($"typeid={id} and aid={appId} and state=1");
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = $"该分类下已有{checkcount}个资讯，不可删除！" });
                }

                model = EntNewsTypeBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "该分类不存在！" });
                }
                model.state = 0;
                if (EntNewsTypeBLL.SingleModel.Update(model, "state"))
                {
                    return Json(new { isok = true, msg = "删除成功！" });
                }
                else
                {
                    return Json(new { isok = true, msg = "删除失败！" });
                }
            }
            #endregion

            #region 添加和修改





            if (model == null || model.id < 0)
            {
                return Json(new { isok = false, msg = "非法请求" });
            }
            if (model.name.Trim() == "" || model.name.Trim().Length > 10)
            {
                return Json(new { isok = false, msg = "分类名称不能为空，且不能超过10个字" });
            }
            else
            {
                int checkcount = EntNewsTypeBLL.SingleModel.GetCount($"name=@name and aid={appId} and id not in(0,{model.id}) and state=1", new MySqlParameter[] { new MySqlParameter("name", model.name) });
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = "已存在该分类名称，请重新设置！" });
                }
            }
            //修改
            if (model.id > 0)
            {
                if (EntNewsTypeBLL.SingleModel.Update(model))
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

                    FunctionList functionList = new FunctionList();
                    int industr = app.VersionId;
                    functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                    if (functionList == null)
                    {
                        return Json(new { isok = false, msg = "此功能未开启" });
                    }

                    NewsMgr newsMgr = new NewsMgr();
                    if (!string.IsNullOrEmpty(functionList.ProductMgr))
                    {
                        newsMgr = JsonConvert.DeserializeObject<NewsMgr>(functionList.NewsMgr);
                    }
                    if (newsMgr.NewsType == 1)//表示关闭了添加类别功能
                    {
                        return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                    }

                }
                #endregion

                //不能超过12个

                int MaxCount = 12;
                string countWhere = $"aid={appId} and state=1";
                //if (model.ParentId == 0)
                //{
                //    MaxCount = 6;
                //    countWhere = $"aid={appId} and state=1 and parentId=0";
                //}

                int checkcount = EntNewsTypeBLL.SingleModel.GetCount(countWhere);
                if (checkcount >= MaxCount)
                {
                    return Json(new { isok = false, msg = $"无法新增分类！您已添加了{MaxCount}个分类，已达到上限，请编辑已有的分类或删除部分分类后再进行新增。" });
                }
                int newid = Convert.ToInt32(EntNewsTypeBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.id = newid;
                    return Json(new { isok = true, msg = model });
                }
            }
            #endregion

            return Json(new { isok = false, msg = "操作失败！" });
        }

        [LoginFilter]
        //[OutputCache(VaryByCustom = "*", VaryByParam = "*", Duration = 20)]
        public ActionResult news(int appId, int pageIndex = 1, int pageSize = 20, int? contentType = null, bool? isPay = null)
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

            int newsContentSwtich = 0;//新闻内容开关
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
                if (!string.IsNullOrEmpty(functionList.NewsMgr))
                {
                    NewsMgr NewsMgr = JsonConvert.DeserializeObject<NewsMgr>(functionList.NewsMgr);
                    newsContentSwtich = NewsMgr.NewsContent;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.newsContentSwtich = newsContentSwtich;
            #endregion


            ViewModel<EntNews> vm = new ViewModel<EntNews>();
            string strwhere = $"aid={appId} and state=1 ";
            string search = Context.GetRequest("search", "");
            int typeid = Context.GetRequestInt("typeid", 0);
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (typeid > 0)
            {
                strwhere += $" and FIND_IN_SET({typeid},typeid) ";
            }
            if (search.Trim() != "")
            {
                strwhere += $" and title like @title ";
                parameters.Add(new MySqlParameter("@title", $"%{search}%"));
            }
            if (contentType.HasValue)
            {
                strwhere += contentType.Value == (int)PaidContentType.专业版图文 ? $" and ( contentType = {contentType.Value} OR contentType is null)" : $" and contentType = {contentType.Value}";
            }
            if (isPay.HasValue)
            {
                strwhere += $" and ispay = {isPay.Value} ";
            }
            vm.DataList = EntNewsBLL.SingleModel.GetListByParam(strwhere, parameters.ToArray(), pageSize, pageIndex, "*", "SortNumber desc, id desc ");
            vm.DataList.ForEach(x =>
            {
                if (x.typeid != 0)
                {
                    x.typename = EntNewsTypeBLL.SingleModel.GetModel(Convert.ToInt32(x.typeid))?.name;
                }
            });

            vm.TotalCount = EntNewsBLL.SingleModel.GetCount(strwhere, parameters.ToArray());
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            vm.PageType = PageType;
            return View("../enterprise/news", vm);
        }

        [LoginFilter, HttpPost]
        public ActionResult newslist(int appId, int pageIndex = 1, int pageSize = 20)
        {
            int typeid = Context.GetRequestInt("typeid", 0);
            string search = Context.GetRequest("search", "");

            ViewModel<EntNews> vm = new ViewModel<EntNews>();
            string strwhere = $"aid={appId} and state=1";
            int totalcount = 0;
            if (typeid != 0)
            {
                strwhere += " and typeid=" + typeid;
            }


            if (search.Trim() == "")
            {
                vm.DataList = EntNewsBLL.SingleModel.GetList(strwhere, pageSize, pageIndex, "*", "SortNumber desc, id desc ");
                totalcount = EntNewsBLL.SingleModel.GetCount(strwhere);
            }
            else
            {
                string sql = $"aid={appId} and state=1 and title like @title";
                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("title", $"%{search}%")
                };
                vm.DataList = EntNewsBLL.SingleModel.GetListByParam(sql, parameters, pageSize, pageIndex, "*", "SortNumber desc, id desc ");
                totalcount = EntNewsBLL.SingleModel.GetCount(strwhere, parameters);
            }

            vm.DataList?.ForEach(x =>
            {
                if (x.typeid != 0)
                {
                    x.typename = EntNewsTypeBLL.SingleModel.GetModel(Convert.ToInt32(x.typeid))?.name;
                }
            });

            vm.TotalCount = totalcount;
            vm.PageCount = Utility.Paging.PageInfo.GetPageCount(totalcount, pageSize);
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            vm.PageType = PageType;

            return Content(JsonConvert.SerializeObject(vm, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
        }

        [LoginFilter(parseAuthDataTo: "authData"), HttpGet]
        public ActionResult newsedit(XcxAppAccountRelation authData)
        {
            int id = Context.GetRequestInt("id", 0);
            EntNews model = EntNewsBLL.SingleModel.GetModel(id);
            if (model == null)
                model = new EntNews();
            ViewModel<EntNews> vm = new ViewModel<EntNews>();
            vm.PageType = PageType;
            vm.DataModel = model;
            vm.DataModel.updatecontent = PayContentBLL.SingleModel.GetModel(model.paycontent) ?? new PayContent();
            ViewBag.userClass = VipLevelBLL.SingleModel.GetList($"appid='{authData.AppId}' and state>=0");
            return View("../enterprise/newsedit", vm);
        }

        [LoginFilter, HttpPost, ValidateInput(false)]
        public ActionResult newsedit(int appId, EntNews model)
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


            string act = Context.GetRequest("act", "");

            #region 专业版 版本控制
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                if (model.id == 0 || act == "copy")
                {

                    FunctionList functionList = new FunctionList();
                    int industr = app.VersionId;
                    functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                    if (functionList == null)
                    {
                        return Json(new { isok = false, msg = "此功能未开启" });
                    }

                    NewsMgr newsMgr = new NewsMgr();
                    if (!string.IsNullOrEmpty(functionList.NewsMgr))
                    {
                        newsMgr = JsonConvert.DeserializeObject<NewsMgr>(functionList.NewsMgr);
                    }
                    if (newsMgr.NewsContent == 1)//表示关闭了添加新闻功能
                    {
                        return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                    }

                }
            }
            #endregion


            if (act == "del")
            {
                int pid = Context.GetRequestInt("id", 0);
                model = EntNewsBLL.SingleModel.GetModel($"id={pid}");
                model.state = 0;
                if (EntNewsBLL.SingleModel.Update(model, "state"))
                {
                    EntSettingBLL.SingleModel.SyncData(appId, "$..coms[?(@.type=='news')].list[?(@.id==" + pid + ")]");

                    return Json(new { isok = true, msg = "删除成功" });
                }
                return Json(new { isok = false, msg = "删除失败" });
            }


            if (act == "copy")
            {
                int pid = Context.GetRequestInt("id", 0);
                if (pid == 0)
                {
                    return Json(new { isok = false, msg = "复制失败，资讯不存在" });
                }
                model = EntNewsBLL.SingleModel.GetModel($"id={pid}");
                if (model == null || model.state == 0)
                {
                    return Json(new { isok = false, msg = "复制失败，资讯不存在或已删除" });
                }
                model.id = 0;
                model.addtime = DateTime.Now;
                if (!model.title.EndsWith("复制"))
                {
                    model.title = model.title + " 复制";
                }

                int payId = 0;
                if (model.paycontent > 0)
                {
                    PayContent payInfo = PayContentBLL.SingleModel.GetModel(model.paycontent);
                    if (payInfo != null)
                    {
                        payInfo.Id = 0;
                        int.TryParse(PayContentBLL.SingleModel.Add(payInfo).ToString(), out payId);
                    }
                }

                if (payId > 0)
                {
                    model.paycontent = payId;
                }

                int newid = Convert.ToInt32(EntNewsBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    return Json(new { isok = true, msg = newid });
                }

            }
            else if (act == "batch_del")
            {
                string pid = Context.GetRequest("id", string.Empty);
                if (pid.IsNullOrWhiteSpace())
                {
                    return Json(new { isok = false, msg = "无选择要删除的咨询内容" });
                }

                List<EntNews> entNews = EntNewsBLL.SingleModel.GetList($" id in ({pid}) ");
                if (entNews == null || !entNews.Any())
                {
                    return Json(new { isok = false, msg = "无选择要删除的有效的咨询内容" });
                }

                entNews.ForEach(n =>
                {
                    n.state = 0;
                    if (EntNewsBLL.SingleModel.Update(n, "state"))
                    {
                        EntSettingBLL.SingleModel.SyncData(appId, "$..coms[?(@.type=='news')].list[?(@.id==" + n.id + ")]");
                    }
                });
                return Json(new { isok = true, msg = "删除成功" });
            }

            if (model.ispay && model.updatecontent.Amount <= 0)
            {
                return Json(new { isok = false, msg = "付费价格设置必须大于零" });
            }
            if (model.updatecontent?.ContentType == (int)PaidContentType.专业版视频 && string.IsNullOrWhiteSpace(model.updatecontent?.VideoURL))
            {
                return Json(new { isok = false, msg = "您还没有上传视频" });
            }

            if (model.id == 0)
            {
                int payId = 0;
                if (model.updatecontent != null && !int.TryParse(PayContentBLL.SingleModel.Add(model.updatecontent).ToString(), out payId))
                {
                    return Json(new { isok = true, msg = "添加失败:1700" });
                }
                if (payId > 0)
                {
                    model.contenttype = model.updatecontent.ContentType;
                }
                model.paycontent = payId;
                int newid = Convert.ToInt32(EntNewsBLL.SingleModel.Add(model));
                model.id = newid;
                if (newid > 0)
                {
                    return Json(new { isok = true, msg = "添加成功" });
                }
                if (model == null)
                {
                    return Json(new { isok = false, msg = "复制失败，产品不存在或已删除" });
                }
            }
            else
            {
                int payId = 0;
                //if (model.updatecontent == null)
                //{
                //    return Json(new { isok = false, msg = "操作失败" });
                //}
                if (model.paycontent > 0)
                {
                    model.updatecontent.Id = model.paycontent;
                    model.contenttype = model.updatecontent.ContentType;
                    payId = PayContentBLL.SingleModel.UpdateContent(model.updatecontent) ? model.paycontent : 0;
                }
                else if (model.updatecontent != null)
                {
                    int.TryParse(PayContentBLL.SingleModel.Add(model.updatecontent).ToString(), out payId);
                    model.contenttype = model.updatecontent.ContentType;
                }
                if (payId > 0)
                {
                    model.paycontent = payId;
                }
                bool result = EntNewsBLL.SingleModel.Update(model);
                if (result)
                {
                    return Json(new { isok = true, msg = "修改成功" });
                }
            }
            return Json(new { isok = false, msg = "操作失败" });
        }



        #region 产品预约
        /// <summary>
        /// 产品预约界面
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult subscribeForm(int appId = 0)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数出错!", code = "500" });
            }
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
            }

            if (role == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={role.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = role.VersionId;
            }
            Store store = StoreBLL.SingleModel.GetModelByAId(role.Id);
            store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
            ViewData["FunJoinModel"] = store.funJoinModel;
            ViewBag.versionId = versionId;
            InityyForm(appId);
            EntSetting model = EntSettingBLL.SingleModel.GetModel(appId);
            ViewModel<EntSetting> vm = new ViewModel<EntSetting>();
            vm.PageType = PageType;
            vm.DataModel = model;
            return View("../enterprise/subscribeForm", vm);
        }

        /// <summary>
        /// 获取产品预约数据列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult GetSubscribeList(int appId = 0)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (appId <= 0)
            {

                return View("PageError", new Return_Msg() { Msg = "参数出错!", code = "500" });
            }
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
            }
            int pagesize = Context.GetRequestInt("pagesize", 12);
            int pageindex = Context.GetRequestInt("pageIndex", 1);
            string start = Context.GetRequest("start", string.Empty);
            string end = Context.GetRequest("end", string.Empty);
            int state = Context.GetRequestInt("state", 0);
            string name = Context.GetRequest("name", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            string goods = Context.GetRequest("goods", string.Empty);
            string sqlwhere = $"aid={appId} and type=1";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                sqlwhere += $" and formtime between @start and @end ";
                parameters.Add(new MySqlParameter("@start", $"{start} 00:00:00"));
                parameters.Add(new MySqlParameter("@end", $"{end} 23:59:59"));
            }
            if (!string.IsNullOrEmpty(name))
            {
                sqlwhere += $" and formdatajson like @name";
                parameters.Add(new MySqlParameter("@name", $"%\"姓名\":\"{name}%\"%"));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                sqlwhere += $" and formdatajson like @phone";
                parameters.Add(new MySqlParameter("@phone", $"%\"手机号码\":\"{phone}%\"%"));
            }
            if (!string.IsNullOrEmpty(goods))
            {
                sqlwhere += $" and remark like @goods";
                parameters.Add(new MySqlParameter("@goods", $"%\"goods\":{{\"name\":\"{goods}%\"}}%"));
            }
            if (state > 0)
            {
                sqlwhere += $" and state={state}";
            }
            else
            {
                sqlwhere += $" and state>0";
            }
            List<EntUserForm> list = EntUserFormBLL.SingleModel.GetListByParam(sqlwhere, parameters.ToArray(), pagesize, pageindex, "*", "id desc");
            int count = EntUserFormBLL.SingleModel.GetCount(sqlwhere, parameters.ToArray());
            if (list != null && list.Count > 0)
            {
                foreach (EntUserForm item in list)
                {
                    item.formdatajson = item.formdatajson.Replace("{", "").Replace("}", "").Replace("\"", "").Replace(",", "<br/>");
                    item.formremark = JsonConvert.DeserializeObject<EntFormRemark>(item.remark);

                }
            }
            object obj = new
            {
                recordCount = count,
                list = list
            };
            return Json(new { isok = true, dataObj = obj, msg = sqlwhere });
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult SettingyyForm(EntSetting setting, StoreConfigModel funModel)
        {
            int appId = Context.GetRequestInt("appid", 0);
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "您还未登录" });
            }
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误id_null" });
            }
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return Json(new { isok = false, msg = "参数错误role_null" });
            }
            if (string.IsNullOrEmpty(setting.pages))
            {
                return Json(new { isok = false, msg = "参数错误pages_null" });
            }
            EntSetting model = EntSettingBLL.SingleModel.GetModel(appId);
            if (model == null)
            {
                return Json(new { isok = false, msg = "数据异常" });
            }
            Store store = StoreBLL.SingleModel.GetModelByAId(role.Id);
            store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
            store.funJoinModel.OpenYuyuePay = funModel.OpenYuyuePay;
            store.funJoinModel.YuyuePayType = funModel.YuyuePayType;
            store.funJoinModel.YuyuePayCount = funModel.YuyuePayCount;
            store.configJson = JsonConvert.SerializeObject(store.funJoinModel);
            model.pages = setting.pages;
            model.updatetime = DateTime.Now;
            bool isok = EntSettingBLL.SingleModel.Update(model, "pages,updatetime") && StoreBLL.SingleModel.Update(store, "configJson"); ;
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, msg = msg });
        }

        /// <summary>
        /// 导出
        /// XXX 要改成带参数的查询
        /// </summary>
        public void SubscribeListExport(int appId = 0, string Ids = "")
        {
            if (dzaccount == null)
            {
                Response.Write("您还未登录");
                return;
            }
            if (appId <= 0)
            {
                Response.Write("参数错误id_null");
                return;
            }
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                Response.Write("参数错误role_null");
                return;
            }

            string act = Context.GetRequest("act", string.Empty);
            int pagesize = Context.GetRequestInt("pagesize", 12);
            int pageindex = Context.GetRequestInt("pageIndex", 1);
            string start = Context.GetRequest("start", string.Empty);
            string end = Context.GetRequest("end", string.Empty);
            int state = Context.GetRequestInt("state", 0);
            string name = Context.GetRequest("name", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            string goods = Context.GetRequest("goods", string.Empty);
            string sqlwhere = $"aid={appId} and type=1 and state>0";
            string filename = "表单导出" + "-" + DateTime.Now.ToString("yyyy-MM-dd");
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            List<EntUserForm> list = null;
            if (act == "search")
            {
                if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
                {
                    sqlwhere += $" and addtime between @start and @end ";
                    parameters.Add(new MySqlParameter("@start", $"{start} 00:00:00"));
                    parameters.Add(new MySqlParameter("@end", $"{end} 23:59:59"));
                }
                if (!string.IsNullOrEmpty(name))
                {
                    sqlwhere += $" and formdatajson like @name";
                    parameters.Add(new MySqlParameter("@name", $"%\"姓名\":\"{name}%\"%"));
                }
                if (!string.IsNullOrEmpty(phone))
                {
                    sqlwhere += $" and formdatajson like @phone";
                    parameters.Add(new MySqlParameter("@phone", $"%\"手机号\":\"{phone}%\"%"));

                }
                if (!string.IsNullOrEmpty(goods))
                {
                    sqlwhere += $" and remark like @goods";
                    parameters.Add(new MySqlParameter("@goods", $"%\"goods\":{{\"name\":\"{goods}%\"}}%"));
                }

                if (!string.IsNullOrEmpty(Ids))
                {
                    if (!Utility.StringHelper.IsNumByStrs(',', Ids))
                    {
                        Response.Write("<script>alert('非法操作!');window.opener=null;window.close();</script>");
                        return;
                    }

                    sqlwhere += $" and Id in ({Ids}) ";

                }


                if (state > 0)
                {
                    sqlwhere += $" and state={state}";

                }
                filename = $"表单导出" + start + "到" + end;
            }
            int count = EntUserFormBLL.SingleModel.GetCount(sqlwhere, parameters.ToArray());
            list = EntUserFormBLL.SingleModel.GetListByParam(sqlwhere, parameters.ToArray(), count, 1, "*", "id desc");

            if (list != null && list.Count > 0)
            {



                DataTable exportTable = new DataTable();
                exportTable.Columns.AddRange(new DataColumn[] {
                        new DataColumn("预约产品"),
                        new DataColumn("预约详情"),
                        new DataColumn("提交时间"),
                        new DataColumn("处理状态"),
                        new DataColumn("操作人备注")
                    });
                foreach (EntUserForm item in list)
                {
                    item.formremark = JsonConvert.DeserializeObject<EntFormRemark>(item.remark);
                    DataRow dr = exportTable.NewRow();
                    dr[0] = item.formremark.goods.name;
                    dr[1] = item.formdatajson.Replace("{", "").Replace("}", "").Replace("\"", "").Replace(",", "\r\n");
                    dr[2] = item.formtime.ToString();
                    dr[3] = item.state == 1 ? "未处理" : item.state == 2 ? "已处理" : "未知";
                    dr[4] = item.formremark.operationremark;
                    exportTable.Rows.Add(dr);
                }
                ExcelHelper<EntUserForm>.Out2Excel(exportTable, filename);//导出
            }
            else
            {
                Response.Write("<script>alert('查无数据');window.opener=null;window.close();</script>");
                return;
            }
        }

        /// <summary>
        /// 处理预约
        /// </summary>
        /// <returns></returns>
        public ActionResult saveyyForm()
        {
            int appId = Context.GetRequestInt("appid", 0);
            int state = Context.GetRequestInt("state", 0);
            string remark = Context.GetRequest("remark", string.Empty);
            int id = Context.GetRequestInt("formId", 0);
            if (appId <= 0 || state <= 0 || id <= 0)
            {
                return Json(new { isok = false, Msg = "参数错误" });
            }
            EntUserForm model = EntUserFormBLL.SingleModel.GetModel($"id={id} and aid={appId} and state>0");
            if (model == null)
            {
                return Json(new { isok = false, Msg = "您无权执行此操作" });
            }
            model.state = state;
            if (!string.IsNullOrEmpty(remark))
            {
                if (remark.Length > 100)
                {
                    return Json(new { isok = false, Msg = "备注内容不能超过100字" });
                }
                EntFormRemark formremark = JsonConvert.DeserializeObject<EntFormRemark>(model.remark) ?? new EntFormRemark();
                formremark.operationremark = remark;
                model.remark = JsonConvert.SerializeObject(formremark);
            }

            //改为已处理则发送预约成功通知通知用户
            if (model.type == 1 && model.state == 2 && !model.excedHandle.Contains("a"))
            {
                model.excedHandle += "a";
                object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(model, SendTemplateMessageTypeEnum.专业版产品预约成功通知);
                TemplateMsg_Miniapp.SendTemplateMessage(model.uid, SendTemplateMessageTypeEnum.专业版产品预约成功通知, (int)TmpType.小程序专业模板, orderData);
            }

            bool isok = EntUserFormBLL.SingleModel.Update(model, "state,remark,excedHandle");
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, Msg = msg });
        }
        #endregion


        #region 汽车行业
        /// <summary>
        /// 汽车分类
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [LoginFilter, HttpGet]
        public ActionResult cartypelist(int appId = 0, int pageIndex = 1, int pageSize = 10)
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

            int parasSwtich = 0;//产品参数开关
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
                    parasSwtich = productMgr.ProductParas;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.parasSwtich = parasSwtich;
            #endregion

            string msg = "";
            ViewModel<EntIndutypes> vm = new ViewModel<EntIndutypes>();
            //var templist = new List<Miniappindutypes>();
            vm.PageSize = pageSize;
            vm.PageIndex = pageIndex;
            vm.TotalCount = EntIndutypesBLL.SingleModel.GetCountByIndustr(appId, app.Industr, 1);

            if (EntIndutypesBLL.SingleModel.CopySystemTypes(appId, app.Industr, ref msg))
            {
                vm.DataList = EntIndutypesBLL.SingleModel.GetListByAIdandLevel(appId, app.Industr, 1, ref msg, vm.PageIndex, vm.PageSize);
                if (vm.DataList != null && vm.DataList.Count > 0)
                {
                    string typeids = string.Join(",", vm.DataList.Select(s => s.TypeId).Distinct());
                    List<EntIndutypes> childtypes = EntIndutypesBLL.SingleModel.GetList($"ParentId in ({typeids}) and aid={app.Id} and state>=0 and Industr='{app.Industr}'", 2000, 1, "*", "sort desc,id asc");
                    if (childtypes != null && childtypes.Count > 0)
                    {
                        foreach (EntIndutypes item in vm.DataList)
                        {
                            List<EntIndutypes> clist = childtypes.Where(w => w.ParentId == item.TypeId).ToList();
                            if (clist != null && clist.Count > 0)
                            {
                                item.childcount = clist.Count;
                            }
                        }
                    }
                }

            }

            vm.Msg = msg;
            vm.PageType = PageType;
            return View("../enterprise/cartypelist", vm);
        }

        [LoginFilter, HttpPost]
        public ActionResult getcartypelist()
        {
            int typeid = Context.GetRequestInt("typeid", 0);
            int aid = Context.GetRequestInt("aid", 0);
            int level = Context.GetRequestInt("level", 0);

            XcxAppAccountRelation xcxrelationmoel = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelationmoel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
            }

            string msg = "";
            ViewModel<EntIndutypes> vm = new ViewModel<EntIndutypes>();
            //var templist = new List<Miniappindutypes>();
            vm.DataList = EntIndutypesBLL.SingleModel.GetListByIndustr(aid, xcxrelationmoel.Industr, level, typeid);
            if (vm.DataList != null && vm.DataList.Count > 0)
            {
                string typeids = string.Join(",", vm.DataList.Select(s => s.TypeId).Distinct());
                List<EntIndutypes> childtypes = EntIndutypesBLL.SingleModel.GetList($"ParentId in ({typeids}) and aid={xcxrelationmoel.Id}  and state>=0 and industr='{xcxrelationmoel.Industr}'");

                if (childtypes != null && childtypes.Count > 0)
                {
                    foreach (EntIndutypes item in vm.DataList)
                    {
                        List<EntIndutypes> clist = childtypes.Where(w => w.ParentId == item.TypeId).ToList();
                        if (clist != null && clist.Count > 0)
                        {
                            item.childcount = clist.Count;
                        }
                    }
                }

            }
            //vm.list = templist;
            vm.Msg = msg;
            return Json(new { isok = true, msg = msg, data = vm.DataList });
        }

        [LoginFilter, HttpPost]
        public ActionResult savecartype(int appId, EntIndutypes model)
        {

            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            int appid = Context.GetRequestInt("appId", 0);
            if (model.AId == 0)
            {
                model.AId = appid;
            }

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
            
            model.Industr = app.Industr;
            #region 删除
            if (act == "del")
            {
                if (id <= 0)
                {
                    return Json(new { isok = false, msg = "非法请求！" });
                }

                model = EntIndutypesBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "该参数不存在！" });
                }
                //检查该分类是否有子类
                int checkcountp = EntIndutypesBLL.SingleModel.GetCountParent(model.AId, app.Industr, model.TypeId);
                if (checkcountp > 0)
                {
                    return Json(new { isok = false, msg = $"该参数下已有{checkcountp}个子参数，不可删除！" });
                }
                //检查是否有已经有产品使用了分类
                int checkcount = EntGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({id},exttypes)>0 and aid={appId} and state=1");
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = $"该参数下已有{checkcount}个产品，不可删除！" });
                }
                model.State = -1;
                if (EntIndutypesBLL.SingleModel.Update(model, "state"))
                {
                    return Json(new { isok = true, msg = "删除成功！" });
                }
                else
                {
                    return Json(new { isok = true, msg = "删除失败！" });
                }
            }
            #endregion

            #region 添加和修改
            if (model == null || model.Id < 0)
            {
                return Json(new { isok = false, msg = "非法请求" });
            }
            if (model.ShowType != 1)
            {
                if (model.TypeName.Trim() == "" || model.TypeName.Trim().Length > 20)
                {
                    return Json(new { isok = false, msg = "参数名称不能为空，且不能超过20个字" });
                }
                else
                {
                    int checkcount = EntIndutypesBLL.SingleModel.GetCount($"TypeName=@TypeName and aid={appId} and id not in(0,{model.Id}) and state>=0", new MySqlParameter[] { new MySqlParameter("TypeName", model.TypeName) });
                    if (checkcount > 0)
                    {
                        return Json(new { isok = false, msg = "已存在该参数名称，请重新设置！" });
                    }
                }
            }

            //修改
            if (model.Id > 0)
            {
                if (EntIndutypesBLL.SingleModel.Update(model))
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
                    if (productMgrModel.ProductParas == 1)//表示关闭了添加参数功能
                    {
                        return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                    }

                }
                #endregion

                string msg = "";
                //不能超过12个
                //int checkcount = EntGoodTypeBLL.SingleModel.GetCount($"aid={appId} and state=1");
                //if (checkcount >= 12)
                //{
                //    return Json(new { isok = false, msg = "无法新增分类！您已添加了12个产品分类，已达到上限，请编辑已有的分类或删除部分分类后再进行新增。" });
                //}
                model.Industr = app.Industr;
                model = EntIndutypesBLL.SingleModel.AddIndustrType(model, ref msg);
                if (model.Id > 0)
                {
                    return Json(new { isok = true, data = model, msg = msg });
                }
                return Json(new { isok = false, msg = msg });
            }
            #endregion

            return Json(new { isok = false, msg = "操作失败！" });
        }
        #endregion
    }
}