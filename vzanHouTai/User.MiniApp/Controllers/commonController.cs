using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp;
using Entity.MiniApp;
using System.Web.Configuration;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Fds;
using Entity.MiniApp.Ent;
using Entity.MiniApp.ViewModel;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Stores;
using Newtonsoft.Json;
using Utility;
using Core.MiniApp;
using User.MiniApp.Model;
using BLL.MiniApp.Tools;
using DAL.Base;
using User.MiniApp.Filters;
using Entity.MiniApp.Footbath;
using BLL.MiniApp.Footbath;
using System.Text.RegularExpressions;
using Utility.IO;
using MySql.Data.MySqlClient;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Tools;
using Entity.MiniApp.FunctionList;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using BLL.MiniApp.PlatChild;

namespace User.MiniApp.Controllers
{
    public partial class commonController : baseController
    {
        
        
        
        
        
        
        
        
        
        
        
        




        
        
        

        
        

        private readonly CityMordersBLL _citymordersBll;

        

        //多门店
        
        
        
        
        
        
        


        
        
        
        
        
        

        
        


        //小未平台子程序
        /// <summary>
        /// 实例化对象
        /// </summary>
        public commonController()
        {
            
            
            
            
            
            
            
            
            
            _citymordersBll = new CityMordersBLL();
            
            
            
            
            
            
            
            
            
            
            
            
        }

        // GET: common
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 微信在线客服工具使用说明
        /// </summary>
        [RouteAuthCheck]
        public ActionResult wxconcat(int appId, int? type = null, int? pageType = null)
        {
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
            if(type.HasValue)
            {
                ViewBag.PageType = type.Value;
            }
            if (pageType.HasValue)
            {
                ViewBag.PageType = pageType.Value;
            }
            ViewBag.appId = appId;
            return View();
        }

        [RouteAuthCheck]
        public ActionResult AladingTongJi(int appId, int pageType)
        {
            ViewBag.PageType = pageType;
            ViewBag.appId = appId;
            return View();
        }

        #region 店铺首页数据分析
        /// <summary>
        /// 小程序后台主页
        /// </summary>
        [LoginFilter]
        public ActionResult dashboard()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int dstoreid = Context.GetRequestInt("dstoreid", 0);
            int storeid = Context.GetRequestInt("storeid", 0);

            ViewBag.appId = appId;
            ViewBag.PageType = 0;
            ViewBag.storeId = dstoreid;

            DashBoardViewModel viewmodel = new DashBoardViewModel();
            
            try
            {
                XcxAppAccountRelation relationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
                if (relationmodel != null)
                {
                    viewmodel.AppId = relationmodel.AppId;
                    viewmodel.RId = relationmodel.Id;
                    //小程序统计数据
                    XcxApiBLL.SingleModel._openType = relationmodel.ThirdOpenType;
                    viewmodel = GetMiniappManageData(viewmodel, relationmodel);

                    //小程序二维码
                    XcxTemplate templatemodel = XcxTemplateBLL.SingleModel.GetModel(relationmodel.TId);
                    if (templatemodel != null)
                    {
                        ViewBag.PageType = templatemodel.Type;

                        string token = "";
                        if (XcxApiBLL.SingleModel.GetToken(relationmodel, ref token))
                        {
                            qrcodeclass result = CommondHelper.GetMiniAppQrcode(token, templatemodel.Address);
                            if (result != null)
                            {
                                if (result.isok > 0)
                                {
                                    viewmodel.XcxCodeImgUrl = result.url;
                                }
                                else
                                {
                                    viewmodel.XcxCodeImgUrl = result.msg;
                                }
                            }
                        }
                    }

                    switch ((int)ViewBag.PageType)
                    {
                        case (int)TmpType.小程序多门店模板:
                            viewmodel.StoreId = dstoreid;
                            List<FootBath> footbath = FootBathBLL.SingleModel.GetList($"appid={relationmodel.Id}");
                            viewmodel.Footbathlist = footbath;
                            FootBath storemodel = footbath.Where(s => s.Id == storeid).FirstOrDefault();
                            #region 判断是否是分店
                            ViewBag.IsHome = 0;
                            if (storemodel != null)
                            {
                                viewmodel.StoreId = storemodel.Id;
                                ViewBag.storeId = storemodel.Id;
                                ViewBag.IsHome = storemodel.HomeId;
                            }
                            #endregion
                            break;
                        case (int)TmpType.小程序电商模板:
                            Store store = StoreBLL.SingleModel.GetModelByRid(relationmodel.Id);
                            if (store != null)
                            {
                                viewmodel.StoreId = store.Id;
                            }
                            break;
                    }
                }
                else
                {
                    viewmodel.IsOk = false;
                    viewmodel.Msg = "还没绑定该小程序模板";
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }


            return View(viewmodel);
        }

        /// <summary>
        /// 获取经营概况
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetStoreInCome()
        {
            int storeId = Context.GetRequestInt("storeId", 0);
            int datetype = Context.GetRequestInt("datetype", 0);
            string appId = Context.GetRequest("appId", string.Empty);
            int rid = Context.GetRequestInt("rid", 0);
            string startdate = Context.GetRequest("startTime", string.Empty);
            string enddate = Context.GetRequest("endTime", string.Empty);
            int orderby = Context.GetRequestInt("orderby", -1);
            int templatetype = Context.GetRequestInt("templatetype", -1);

            if (string.IsNullOrEmpty(startdate))
            {
                startdate = DateTime.Now.ToShortDateString();
            }
            if (string.IsNullOrEmpty(enddate))
            {
                enddate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            }

            if (datetype == -1)//昨天
            {
                startdate = DateTime.Now.AddDays(-1).ToShortDateString();
                enddate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + " 23:59:59";
            }
            else if (datetype >= 7)//七天内或30天内
            {
                startdate = DateTime.Now.AddDays(-datetype).ToShortDateString();
            }

            MiniAppStoreData data = new MiniAppStoreData();
            //商品销售排行
            if (orderby != -1)
            {
                switch (templatetype)
                {
                    case (int)TmpType.小程序多门店模板:
                        List<MiniAppStoreGoods> salegoodsorderby = EntGoodsOrderBLL.SingleModel.GetOrderGoodsSaleLog(storeId, rid, orderby, startdate, enddate);
                        data.salegoodsorderby = salegoodsorderby;
                        return Json(new { obj = data }, JsonRequestBehavior.AllowGet);
                    case (int)TmpType.小程序电商模板:
                        //商品销售排行
                        List<MiniAppStoreGoods> goodsorderby = StoreBLL.SingleModel.GetStoreGroupsDescData(storeId, orderby, startdate, enddate);
                        data.salegoodsorderby = goodsorderby;
                        return Json(new { obj = data }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                orderby = 0;
            }

            switch (templatetype)
            {
                case (int)TmpType.小程序多门店模板:
                    data = GetMultiStoreData(appId, datetype, storeId, startdate, enddate, orderby, rid, data);
                    break;
                case (int)TmpType.小程序电商模板:
                    data = GetStoreData(appId, datetype, storeId, startdate, enddate, orderby, data);
                    break;
            }

            return Json(new { obj = data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 电商统计分析
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="orderby"></param>
        /// <param name="data"></param>
        [NonAction]
        public MiniAppStoreData GetStoreData(string appId, int datetype, int storeId, string startdate, string enddate, int orderby, MiniAppStoreData data)
        {
            //累计会员数量
            int vipsum = 0;
            string vipsumsql = $"appid='{appId}'";
            vipsumsql += datetype == -1 ? $" and AddTime<='{enddate}'" : datetype >= 7 ? $" and AddTime>='{startdate}' and AddTime<='{enddate}'" : datetype == -2 ? $" and AddTime>='{startdate}' and AddTime<='{enddate}'" : $" and AddTime<='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'";

            //会员充值
            int vippaysum = SaveMoneySetUserLogBLL.SingleModel.GetVipPayLog(appId, startdate, enddate);
            //新增会员数量
            int usersum = C_UserInfoBLL.SingleModel.GetCount($"appid='{appId}' and AddTime>='{startdate}' and AddTime<='{enddate}'");
            //累计会员数量
            vipsum = C_UserInfoBLL.SingleModel.GetCount(vipsumsql);
            data.vippaysum = vippaysum;
            data.usersum = usersum;
            data.vipsum = vipsum;

            //电商微信支付销售金额
            int sum = StoreBLL.SingleModel.GetStoreInCome(storeId, startdate, enddate);
            //商品分类销售情况
            int salesum = 0;
            List<MiniAppStoreGoods> salegoods = StoreBLL.SingleModel.GetGoodTypeInCome(storeId, startdate, enddate, ref salesum);
            //商品销售排行
            List<MiniAppStoreGoods> salegoodsorderby = StoreBLL.SingleModel.GetStoreGroupsDescData(storeId, orderby, startdate, enddate);
            data.storeincome = sum;
            data.salegoodsorderby = salegoodsorderby;
            data.salegoods = salegoods;
            data.salesum = salesum;
            return data;
        }

        /// <summary>
        /// 多门统计分析
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="orderby"></param>
        /// <param name="data"></param>
        public MiniAppStoreData GetMultiStoreData(string appId, int datetype, int storeId, string startdate, string enddate, int orderby, int rid, MiniAppStoreData data)
        {
            try
            {
                //累计会员数量
                int vipsum = 0;
                string vipsumsql = $"appid='{appId}'";
                if (storeId > 0)
                {
                    vipsumsql += $" and storeid={storeId}";
                }
                vipsumsql += datetype == -1 ? $" and AddTime<='{enddate}'" : datetype >= 7 ? $" and AddTime>='{startdate}' and AddTime<='{enddate}'" : datetype == -2 ? $" and AddTime>='{startdate}' and AddTime<='{enddate}'" : $" and AddTime<='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'";

                //会员充值
                int vippaysum = SaveMoneySetUserLogBLL.SingleModel.GetVipPayLog(appId, startdate, enddate, storeId);
                //新增会员数量
                int usersum = C_UserInfoBLL.SingleModel.GetCount($"appid='{appId}' {(storeId > 0 ? " and storeid=" + storeId : "")} and AddTime>='{startdate}' and AddTime<='{enddate}'");
                //累计会员数量
                vipsum = C_UserInfoBLL.SingleModel.GetCount(vipsumsql);
                data.vippaysum = vippaysum;
                data.usersum = usersum;
                data.vipsum = vipsum;


                //商品支付销售金额
                int sum = EntGoodsOrderBLL.SingleModel.GetOrderPriceSum(storeId, rid, startdate, enddate);
                //商品分类销售情况
                int salesum = 0;
                List<MiniAppStoreGoods> salegoods = EntGoodsOrderBLL.SingleModel.GetOrderGoodsTypeSaleLog(storeId, rid, startdate, enddate, ref salesum);
                //商品销售排行
                List<MiniAppStoreGoods> salegoodsorderby = EntGoodsOrderBLL.SingleModel.GetOrderGoodsSaleLog(storeId, rid, orderby, startdate, enddate);
                data.storeincome = sum;
                data.salegoodsorderby = salegoodsorderby;
                data.salegoods = salegoods;
                data.salesum = salesum;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
            return data;
        }

        #region 获取小程序统计数据
        public DashBoardViewModel GetMiniappManageData(DashBoardViewModel viewmodel, XcxAppAccountRelation relationmodel)
        {
            //获取授权信息
            OpenAuthorizerInfo openconfig = OpenAuthorizerInfoBLL.SingleModel.GetModelByAppId(relationmodel.AppId);
            if (openconfig != null)
            {
                string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(openconfig.user_name);
                //获取token
                string authorizer_access_token = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);

                if (!string.IsNullOrEmpty(authorizer_access_token))
                {
                    DateTime nowtime = DateTime.Now.AddDays(1);
                    //获取概括趋势数据
                    string gkurl = XcxApiBLL.SingleModel.GetDailySumMaryTrend(authorizer_access_token);
                    Return_Msg gkresult = XcxApiBLL.SingleModel.GetDataInfo<XCXDataModel<GKResultModel>>(gkurl, nowtime, nowtime);
                    viewmodel.GKData = (XCXDataModel<GKResultModel>)gkresult.dataObj;

                    //获取访问数据
                    string fwurl = XcxApiBLL.SingleModel.GetDailyVisitTrend(authorizer_access_token);
                    Return_Msg fwresult = XcxApiBLL.SingleModel.GetDataInfo<XCXDataModel<FWResultModel>>(fwurl, nowtime, nowtime);
                    viewmodel.FWData = (XCXDataModel<FWResultModel>)fwresult.dataObj;

                    viewmodel.IsOk = true;
                    viewmodel.Msg = "成功";

                    //log4net.LogHelper.WriteInfo(this.GetType(), JsonConvert.SerializeObject(viewmodel));
                }
                else
                {
                    viewmodel.IsOk = false;
                    viewmodel.Msg = "获取token失败，请重新绑定小程序";
                }
            }
            else
            {
                viewmodel.IsOk = false;
                viewmodel.Msg = "未授权";
            }


            return viewmodel;
        }
        /// <summary>
        /// 获取小程序统计数据
        /// </summary>
        /// <param name="user_name"></param>
        /// <returns></returns>
        //public Return_Msg GetDataInfo<T>(string url)
        //{
        //    Return_Msg msg = new Return_Msg();

        //    //获取概括起始日期，只能获取一天内的数据 
        //    string datestr = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    //end_date允许设置的最大值为昨日
        //    object postdata = new { begin_date = datestr, end_date = datestr + " 23:59:59" };
        //    string result = HttpHelper.PostData(url, postdata);
        //    //log4net.LogHelper.WriteInfo(this.GetType(), result);
        //    T gkresult = JsonConvert.DeserializeObject<T>(result);

        //    msg.isok = true;
        //    msg.dataObj = gkresult;
        //    return msg;
        //}
        #endregion
        #endregion

        #region 打印机管理
        /// <summary>
        /// 打印机管理列表
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult PrintList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int pageType = Context.GetRequestInt("pageType", 0);

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appId_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxRole = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (xcxRole == null)
            {
                return Json(new { isok = false, msg = "系统繁忙xcxRole_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcxRole.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcxRole.VersionId;
            }

            ViewBag.versionId = versionId;
            //不同模板不同验证
            switch (pageType)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store = FoodBLL.SingleModel.GetModel($" appId = {appId} ");
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                    }
                    storeId = store.Id;//找出店铺Id

                    if (xcxRole.AccountId != dzaccount.Id)
                    {
                        return Json(new { isok = false, msg = "权限不足accountId_notHaving！" }, JsonRequestBehavior.AllowGet);
                    }

                    break;
                case (int)TmpType.小程序专业模板:
                    if (xcxRole.AccountId != dzaccount.Id)
                    {
                        return Json(new { isok = false, msg = "权限不足accountId_notHaving！" }, JsonRequestBehavior.AllowGet);
                    }

                    break;
            }

            List<FoodPrints> PrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {storeId} and appId = {appId} and state >= 0 ") ?? new List<Entity.MiniApp.Fds.FoodPrints>();
            ViewBag.PageType = pageType;
            ViewBag.appId = appId;
            ViewBag.storeId = storeId;
            return View(PrintList);
        }

        /// <summary>
        /// 查看打印机终端
        /// </summary>
        /// <param name="print"></param>
        /// <returns></returns>
        public ActionResult getPrintForm(int appId, int printId, int edit = 0)
        {
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_error！" }, JsonRequestBehavior.AllowGet);
            }
            FoodPrints print = FoodPrintsBLL.SingleModel.GetModel(printId) ?? new Entity.MiniApp.Fds.FoodPrints();
            //先访问易连云接口添加,成功后才在系统内添加记录

            ViewBag.edit = edit;
            return PartialView("_PartialPrintItem", print);
        }

        /// <summary>
        /// 添加打印机终端
        /// </summary>
        /// <param name="print"></param>
        /// <returns></returns>
        public ActionResult addPrint(Entity.MiniApp.Fds.FoodPrints print)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            //先访问易连云接口添加,成功后才在系统内添加记录
            PrintErrorData returnMsg = FoodYiLianYunPrintHelper.addPrinter(print.APIKey, print.UserId, print.PrintNo, print.PrintKey, print.Telphone, print.UserName, print.Name);
            if (returnMsg.errno != 1)
            {
                return Json(new { isok = false, msg = returnMsg.error }, JsonRequestBehavior.AllowGet);
            }

            print.accountId = dzaccount.OpenId;
            print.State = 0;
            print.CreateDate = DateTime.Now;
            int id = Convert.ToInt32(FoodPrintsBLL.SingleModel.Add(print));
            if (id > 0)
            {
                return Json(new { isok = true, msg = "添加成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false, msg = "添加失败！" }, JsonRequestBehavior.AllowGet);
        }

        ///// <summary>
        ///// 删除打印机
        ///// </summary>
        ///// <param name="appid"></param>
        ///// <param name="printId"></param>
        ///// <returns></returns>
        public ActionResult deletePrint()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int printId = Context.GetRequestInt("printId", 0);

            FoodPrints print = FoodPrintsBLL.SingleModel.GetModel(printId);
            if (print == null)
            {
                Json(new { isok = false, msg = "系统繁忙printModel_null！" }, JsonRequestBehavior.AllowGet);
            }
            //先访问易连云接口删除,成功后才在系统内操作记录
            string returnMsg = FoodYiLianYunPrintHelper.deletePrinter(print.APIKey, print.UserId, print.PrintNo, print.PrintKey);
            if (Convert.ToInt32(returnMsg) == 4)
            {
                return Json(new { isok = false, msg = "删除失败!！" }, JsonRequestBehavior.AllowGet);
            }
            print.State = -1;
            print.UpdateDate = DateTime.Now;
            bool result = FoodPrintsBLL.SingleModel.Update(print, "State,UpdateDate");
            if (result)
            {
                return Json(new { isok = true, msg = "删除成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false, msg = "删除失败！" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult deletePrintByAid(string apikey = "", string userid = "", string num = "", string key = "")
        {

            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0 || userid == "" || num == "" || key == "")
            {
                return Json(new { isok = false, msg = "非法请求！" });
            }
            FoodPrints model = FoodPrintsBLL.SingleModel.GetModel($" state=0 and Type=1 and PrintNo=@num and PrintKey=@key and APIKey=@apikey and UserId=@userid", new MySqlParameter[] {
                new MySqlParameter("@apikey",apikey),
                new MySqlParameter("@num",num),
                new MySqlParameter("@key",key),
                new MySqlParameter("@userid",userid)
            });

            if (model != null)
            {
                FoodPrintsBLL.SingleModel.Delete(model.Id);
            }


            //先访问易连云接口删除,成功后才在系统内操作记录
            string returnMsg = FoodYiLianYunPrintHelper.deletePrinter(apikey, userid, num, key);
            if (Convert.ToInt32(returnMsg) == 4)
            {
                return Json(new { isok = false, msg = "删除失败！" }, JsonRequestBehavior.AllowGet);
            }

            //bool result = _miniappfoodsprintsBll.Update(print, "State,UpdateDate");
            //if (result)
            //{
            //    return Json(new { isok = true, msg = "删除成功！" }, JsonRequestBehavior.AllowGet);
            //}
            return Json(new { isok = true, msg = "删除成功！" }, JsonRequestBehavior.AllowGet);
        }


        ///// <summary>
        ///// 设定 总票/单票 打印机
        ///// </summary>
        ///// <param name="appid"></param>
        ///// <param name="printId"></param>
        ///// <returns></returns>
        public ActionResult settingPrints()
        {
            int printId = Context.GetRequestInt("printId", 0);
            int printType = Context.GetRequestInt("printType", 0);

            FoodPrints print = FoodPrintsBLL.SingleModel.GetModel(printId);
            if (print == null)
            {
                Json(new { isok = false, msg = "系统繁忙printModel_null！" }, JsonRequestBehavior.AllowGet);
            }
            print.printType = printType;
            print.UpdateDate = DateTime.Now;

            bool result = FoodPrintsBLL.SingleModel.Update(print, "printType,UpdateDate");
            if (result)
            {
                return Json(new { isok = true, msg = "设定成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false, msg = "设定失败！" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 后台一级分销(营销插件)

        /// <summary>
        /// 分销管理 一级分销
        /// </summary> 
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult salesman(int appId)
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

            int distributionSwtich = 0;//分销功能开关
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
                    distributionSwtich = operationMgr.Distribution;
                }

            }

            ViewBag.distributionSwtich = distributionSwtich;
            ViewBag.versionId = versionId;
            ViewBag.PageType = xcxTemplate.Type;

            SalesManConfig salesManConfig = SalesManConfigBLL.SingleModel.GetModel($"appId={appId}");
            if (salesManConfig == null)
            {
                salesManConfig = new SalesManConfig();
                ConfigModel configMolde = new ConfigModel();
                salesManConfig.configStr = JsonConvert.SerializeObject(configMolde);
                salesManConfig.AddTime = DateTime.Now;
                salesManConfig.UpdateTime = DateTime.Now;
                salesManConfig.appId = xcx.Id;
                int id = Convert.ToInt32(SalesManConfigBLL.SingleModel.Add(salesManConfig));
                if (id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "初始化异常!", code = "500" });
                }
                salesManConfig.Id = id;
                salesManConfig.configModel = configMolde;
            }
            else
            {
                salesManConfig.configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);
            }
            if(salesManConfig.configModel.recruitPlan!=null&& salesManConfig.configModel.recruitPlan.description != null)
            {
                salesManConfig.configModel.recruitPlan.description = HttpUtility.HtmlDecode(salesManConfig.configModel.recruitPlan.description);
            }



            #region 兼容分销员申请审核设置数据,累计消费与累计储值由原来的单选变为复选
            if (salesManConfig.configModel != null && salesManConfig.configModel.salesManManager != null)
            {
                if (salesManConfig.configModel.salesManManager.is_verify_on == 2)
                {
                    //表示之前的累计消费通过审核
                    salesManConfig.configModel.salesManManager.Cost_verify_on = true;
                    salesManConfig.configModel.salesManManager.is_verify_on = 4;//重新赋值为4
                }
                if (salesManConfig.configModel.salesManManager.is_verify_on == 3)
                {
                    //表示之前的累计储值通过审核
                    salesManConfig.configModel.salesManManager.SaveMoney_verify_on = true;
                    salesManConfig.configModel.salesManManager.SaveMoney = salesManConfig.configModel.salesManManager.CostMoney;
                    salesManConfig.configModel.salesManManager.is_verify_on = 4;//重新赋值为4
                }


            } 
            #endregion


            ViewBag.appId = appId;
            return View(salesManConfig);
        }


        /// <summary>
        /// 保存分销设置的信息
        /// </summary>
        /// <param name="salesManConfig"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult salesmanSet(SalesManConfig salesManConfig)
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" });
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" });
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "找不到该小程序模板" });

            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                int versionId = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return Json(new { isok = false, msg = $"功能权限未设置" }, JsonRequestBehavior.AllowGet);

                }
                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }

                if (operationMgr.Distribution == 1)
                {
                    return Json(new { isok = false, msg = $"请先升级到更高版本才能开启此功能" }, JsonRequestBehavior.AllowGet);
                }

            }

            if (salesManConfig != null)
            {
                SalesManConfig model = SalesManConfigBLL.SingleModel.GetModel($"Id={salesManConfig.Id} and appId={appId}");
                if (model == null)
                    return Json(new { isok = false, msg = "非法操作" });
                model.UpdateTime = DateTime.Now;
                salesManConfig.configModel.recruitPlan.description = HttpUtility.HtmlEncode(salesManConfig.configModel.recruitPlan.description);
                model.configStr = JsonConvert.SerializeObject(salesManConfig.configModel);
                model.state = salesManConfig.state;
                if (!salesManConfig.configModel.salesManManager.is_protect_seller)
                {
                    salesManConfig.configModel.salesManManager.protected_time = 0;
                }

                TransactionModel transactionModel = new TransactionModel();
                transactionModel.Add(SalesManConfigBLL.SingleModel.BuildUpdateSql(model));
                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    int count = EntGoodsBLL.SingleModel.GetCount($"aid={appId} and isDistribution=1 and isDefaultCps_Rate=0");
                    if (count > 0)
                    {
                        transactionModel.Add($"update EntGoods set distributionTime='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',cps_rate={salesManConfig.configModel.payMentManager.cps_rate} where isDistribution=1 and isDefaultCps_Rate=0 and aid={appId}");
                    }
                }


                //  log4net.LogHelper.WriteInfo(this.GetType(),JsonConvert.SerializeObject(transactionModel.sqlArray));
                if (SalesManConfigBLL.SingleModel.ExecuteTransactionDataCorect(transactionModel.sqlArray))
                    return Json(new { isok = true, msg = "操作成功" });
                return Json(new { isok = false, msg = "操作失败" });

            }
            return Json(new { isok = false, msg = "数据不能为NULL" });

        }


        /// <summary>
        /// 分销产品列表页
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult salesmanGoods(int appId)
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
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序模板不存在" });

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
            }

            ViewBag.versionId = versionId;
            ViewBag.appId = appId;
            ViewBag.PageType = xcxTemplate.Type;
            return View();
        }


        /// <summary>
        /// 获取分销产品列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="goodsName"></param>
        /// <param name="isDistribution"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetSalesmanGoodsList(int appId, string goodsName = "", int isDistribution = -1, int pageIndex = 1, int pageSize = 10)
        {
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" }, JsonRequestBehavior.AllowGet);

            List<SalesmanGoods> list = new List<SalesmanGoods>();
            int TotalCount = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                int versionId = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    OperationMgr operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                    if (operationMgr.Distribution == 1)
                    {
                        return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, SalesmanGoodsList = list } }, JsonRequestBehavior.AllowGet);
                    }
                }

            }



            list = SalesManConfigBLL.SingleModel.GetListSalesmanGoods(appId, xcxTemplate.Type, goodsName, isDistribution, pageIndex, pageSize);
            TotalCount = SalesManConfigBLL.SingleModel.GetSalesmanGoodsCount(appId, xcxTemplate.Type, goodsName, isDistribution);
            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, SalesmanGoodsList = list } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 更新分销产品分销相关信息
        /// </summary>
        /// <param name="salesmanGoods"></param>
        /// <returns></returns>
        public ActionResult SaveSalesmanGoods(SalesmanGoods salesmanGoods)
        {
            int appId = Context.GetRequestInt("appId", 0);
            string ids = StringHelper.ReplaceSqlKeyword(Context.GetRequest("ids", string.Empty));
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" }, JsonRequestBehavior.AllowGet);
            if (salesmanGoods == null)
                return Json(new { isok = false, msg = "数据不存在" }, JsonRequestBehavior.AllowGet);

            int code = -888;
            if (!string.IsNullOrEmpty(ids))
            {
                //表示批量设置
                if (!StringHelper.IsNumByStrs(',', ids))
                    return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
                code = SalesManConfigBLL.SingleModel.UpdateSalesmanGoods(salesmanGoods, appId, xcxTemplate.Type, ids);

            }
            else
            {
                //表示单个设置
                code = SalesManConfigBLL.SingleModel.UpdateSalesmanGoods(salesmanGoods, appId, xcxTemplate.Type, ids);
            }

            if (code > 0)
            {
                return Json(new { isok = true, msg = "操作成功", code = code }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = false, msg = "操作失败", code = code, ids = ids }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 分销员列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult SalesmanList(int appId=0)
        {
            if (appId <= 0)
                return View("ErrorMsg", new Return_Msg() { Msg = "appId非法" });
            if (dzaccount == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "登录信息超时" });
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序模板不存在" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcx.VersionId;
            }
            ViewBag.versionId = versionId;

            int state = Context.GetRequestInt("state", 0);

            if (state != 2)
            {
                ViewBag.Title = "待审核";
                if (state == -1)
                {
                    ViewBag.Title = "已被清退";
                }
            }
            else
            {
                ViewBag.Title = "通过审核";
            }
            ViewBag.state = state;
            ViewBag.appId = appId;
            ViewBag.PageType = xcxTemplate.Type;
            return View();
        }

        /// <summary>
        /// 获取分销员列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="TelePhone"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult GetSalesmanList(int appId, string TelePhone = "", int pageIndex = 1, int pageSize = 10, int state = -1)
        {
            string strWhere = $"m.appId={appId} and m.state=2 ";
            if (state == 0)
                strWhere = $"m.appId={appId} and m.state in(0,1)";
            if (state == -1)
                strWhere = $"m.appId={appId} and m.state=-1";

            string startTime = Context.GetRequest("startTime", string.Empty);
            string endTime = Context.GetRequest("endTime", string.Empty);
            if (!string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(startTime)) && !string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(startTime)))
                strWhere += $" and m.AddTime>='{startTime}' and m.AddTime<='{endTime}'";
            if (!string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(TelePhone)))
                strWhere += $" and m.TelePhone like '%{TelePhone}%'";

            string parentSaleManKeyMsg= Context.GetRequest("parentSaleManKeyMsg", string.Empty);
            if (!string.IsNullOrEmpty(parentSaleManKeyMsg))
            {
              string ids=  SalesManBLL.SingleModel.GetSaleManIdsByPhoneName(parentSaleManKeyMsg,appId);
                strWhere += $" and m.ParentSalesmanId in({ids}) ";
            }
                



            List<SalesMan> listSalesman = SalesManBLL.SingleModel.GetListSalesMan(strWhere, pageIndex, pageSize, "m.Id desc");

            int TotalCount = SalesManBLL.SingleModel.GetListSalesManCount(strWhere);
            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, SalesmanList = listSalesman } }, JsonRequestBehavior.AllowGet);


        }


        public ActionResult UpdateSaleMan(SalesMan salesMan)
        {
            int appId = Context.GetRequestInt("appId", 0);
            int type = Context.GetRequestInt("type", 0);
            string ids = StringHelper.ReplaceSqlKeyword(Context.GetRequest("ids", string.Empty));
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            if (salesMan == null)
                return Json(new { isok = false, msg = "没有更新的数据" }, JsonRequestBehavior.AllowGet);
            if (type == 0)
            {
                //表示更新备注信息 只是单个更新
                SalesMan mode = SalesManBLL.SingleModel.GetModel($"appId={appId} and Id={salesMan.Id}");
                if (mode == null)
                    return Json(new { isok = false, msg = "数据不存在" }, JsonRequestBehavior.AllowGet);
                mode.Remark = salesMan.Remark;
                if (SalesManBLL.SingleModel.Update(mode))
                    return Json(new { isok = true, msg = "操作成功" }, JsonRequestBehavior.AllowGet);
                return Json(new { isok = false, msg = "操作异常" }, JsonRequestBehavior.AllowGet);

            }

            if (type == 1 && string.IsNullOrEmpty(ids))
            {
                //表示单个更新分销员状态
                SalesMan mode = SalesManBLL.SingleModel.GetModel($"appId={appId} and Id={salesMan.Id}");
                if (mode == null)
                    return Json(new { isok = false, msg = "数据不存在" }, JsonRequestBehavior.AllowGet);
                mode.state = salesMan.state;
                if (SalesManBLL.SingleModel.Update(mode))
                    return Json(new { isok = true, msg = "状态更新成功" }, JsonRequestBehavior.AllowGet);
                return Json(new { isok = false, msg = "状态更新异常" }, JsonRequestBehavior.AllowGet);
            }

            if (type == 1 && !string.IsNullOrEmpty(ids))
            {
                //表示批量更新分销员状态
                if (!StringHelper.IsNumByStrs(',', ids))
                    return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
                int result = SalesManBLL.SingleModel.ExecuteNonQuery($"update SalesMan set state={salesMan.state} where Id in({ids})");
                if (result > 0)
                    return Json(new { isok = true, msg = "批量设置成功" }, JsonRequestBehavior.AllowGet);
                return Json(new { isok = false, msg = "批量设置失败" }, JsonRequestBehavior.AllowGet);



            }
            return Json(new { isok = true, msg = "请选择操作" }, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 推广效果列表页面
        /// </summary>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult PromotionEffect(int appId)
        {
            if (appId <= 0)
                return View("ErrorMsg", new Return_Msg() { Msg = "appId非法" });
            if (dzaccount == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "登录信息超时" });
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序模板不存在" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcx.VersionId;
            }
            ViewBag.versionId = versionId;
            ViewBag.Title = "推广效果";
            ViewBag.appId = appId;
            ViewBag.PageType = xcxTemplate.Type;
            return View();
        }

        /// <summary>
        /// 获取推广效果列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPromotionEffectList(int appId, string TelePhone = "", string orderNumber = "", int pageIndex = 1, int pageSize = 10)
        {
            string strWhere = $"m.appId={appId} and r.state=1";
            string startTime = Context.GetRequest("startTime", string.Empty);
            string endTime = Context.GetRequest("endTime", string.Empty);
            if (!string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(startTime)) && !string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(startTime)))
                strWhere += $" and m.AddTime>='{startTime}' and m.AddTime<='{endTime}'";
            if (!string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(TelePhone)))
                strWhere += $" and o.TelePhone like '%{TelePhone}%'";

            if (!string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(orderNumber)))
                strWhere += $" and m.orderNumber='{orderNumber}'";


            List<SalesManRecordOrder> listSalesManRecordOrder = SalesManRecordOrderBLL.SingleModel.GetListSalesManRecordOrder(strWhere, pageIndex, pageSize, "Id desc",0);

            int TotalCount = SalesManRecordOrderBLL.SingleModel.GetSalesManRecordOrderCount(strWhere,0);
            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, List = listSalesManRecordOrder } });

        }

        /// <summary>
        /// 将未结算的 人工结算方式推广订单标记为已结算
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdatePromotionEffect()
        {
          int Id=  Context.GetRequestInt("Id", 0);
            int appId = Context.GetRequestInt("appId", 0);
            if (Id <= 0 || appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误"},JsonRequestBehavior.AllowGet);
            }
            SalesManRecordOrder salesManRecordOrder = SalesManRecordOrderBLL.SingleModel.GetModel(Id);
            if (salesManRecordOrder ==null|| salesManRecordOrder.appId != appId)
            {
                return Json(new { isok = false, msg = "数据不存在" }, JsonRequestBehavior.AllowGet);
            }

            salesManRecordOrder.PayState = 0;
            if(SalesManRecordOrderBLL.SingleModel.Update(salesManRecordOrder, "PayState"))
            {
                return Json(new { isok = true, msg = "操作成功" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = false, msg = "操作异常" }, JsonRequestBehavior.AllowGet);

        }



        /// <summary>
        /// 分销员推广业绩列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="TelePhone"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult Performance(int appId)
        {
            if (appId <= 0)
                return View("ErrorMsg", new Return_Msg() { Msg = "appId非法" });
            if (dzaccount == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "登录信息超时" });
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序模板不存在" });
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcx.VersionId;
            }
            ViewBag.versionId = versionId;

            ViewBag.Title = "业绩统计";
            ViewBag.appId = appId;
            ViewBag.PageType = xcxTemplate.Type;
            return View();
        }


        /// <summary>
        /// 获取分销员业绩推广
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="TelePhone"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetPerformanceList(int appId, string TelePhone = "", int pageIndex = 1, int pageSize = 10)
        {
            string strWhere = $"o.appId={appId}";
            string startTime = Context.GetRequest("startTime", string.Empty);
            string endTime = Context.GetRequest("endTime", string.Empty);
            if (!string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(startTime)) && !string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(startTime)))
                strWhere += $" and o.AddTime>='{startTime}' and o.AddTime<='{endTime}'";
            if (!string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(TelePhone)))
                strWhere += $" and o.TelePhone like '%{TelePhone}%'";


            List<SalesMan> listSalesManRecordOrder = SalesManRecordOrderBLL.SingleModel.GetSalesManRecordOrdersGroupBySalesMan(appId, strWhere, pageIndex, pageSize, "Id desc");

            int TotalCount = SalesManRecordOrderBLL.SingleModel.GetSalesManRecordOrdersGroupBySalesManCount(strWhere);
            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, List = listSalesManRecordOrder } });

        }


        /// <summary>
        /// 分销员与客户的关系查询
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="TelePhone"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult Relationship(int appId, string TelePhone = "", int pageIndex = 1, int pageSize = 10)
        {
            if (appId <= 0)
                return View("ErrorMsg", new Return_Msg() { Msg = "appId非法" });
            if (dzaccount == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "登录信息超时" });
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序模板不存在" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcx.VersionId;
            }
            ViewBag.versionId = versionId;
            ViewBag.Title = "关系查询";
            ViewBag.appId = appId;
            ViewBag.PageType = xcxTemplate.Type;
            return View();
        }


        /// <summary>
        ///  关系查询
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult RelationshipSearch(int appId)
        {
            if (appId <= 0)
                return View("ErrorMsg", new Return_Msg() { Msg = "appId非法" });
            if (dzaccount == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "登录信息超时" });
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序模板不存在" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcx.VersionId;
            }
            ViewBag.versionId = versionId;

            ViewBag.Title = "关系查询";
            ViewBag.appId = appId;
            ViewBag.PageType = xcxTemplate.Type;
            return View();
        }


        /// <summary>
        /// 获取分销员与客户的关系查询
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="TelePhone"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetRelationship(int appId, string orderNumber)
        {
            if (appId <= 0 || string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(orderNumber)))
            {
                return Json(new { isok = false, msg = "参数错误" });
            }
            EntGoodsOrder entGoodsOrder = EntGoodsOrderBLL.SingleModel.GetModel($"OrderNum={orderNumber}");
            if (entGoodsOrder == null)
            {
                return Json(new { isok = false, msg = "订单不存在" });
            }
            List<RelationViewModel> listRelationViewModel = SalesManRecordUserBLL.SingleModel.GetRelationSearch(entGoodsOrder.Id);
            return Json(new { isok = true, msg = "成功", model = new { RecordCount = listRelationViewModel.Count, List = listRelationViewModel } });


        }



        /// <summary>
        ///  提现申请
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [MiniApp.Filters.RouteAuthCheck]
        public ActionResult DrawCashApply(int appId)
        {
            if (appId <= 0)
                return View("ErrorMsg", new Return_Msg() { Msg = "appId非法" });
            if (dzaccount == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "登录信息超时" });
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "小程序模板不存在" });


            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcx.VersionId;
            }
            ViewBag.versionId = versionId;

            ViewBag.Title = "提现申请";
            ViewBag.appId = appId;
            ViewBag.PageType = xcxTemplate.Type;
            return View();
        }

        /// <summary>
        /// 获取提现申请列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="TelePhone"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetDrawCashApply(int appId, string TelePhone = "", int state = -2, int pageIndex = 1, int pageSize = 10)
        {

            string startTime = Context.GetRequest("startTime", string.Empty);
            string endTime = Context.GetRequest("endTime", string.Empty);
            int TotalCount = 0;
            List<DrawCashApply> list = DrawCashApplyBLL.SingleModel.GetDistributionDrawCashApplys(out TotalCount, appId, state, startTime, endTime, TelePhone, pageSize, pageIndex);

            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, List = list } });

        }


        /// <summary>
        /// 变更提现申请记录状态
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateDrawCashApply()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int state = Context.GetRequestInt("state", 0);
            if (state != -1 && state != 1)
                return Json(new { isok = false, msg = "审核状态错误" }, JsonRequestBehavior.AllowGet);
            string ids = StringHelper.ReplaceSqlKeyword(Context.GetRequest("ids", string.Empty));
            if (appId <= 0)
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息超时" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            if (!StringHelper.IsNumByStrs(',', ids))
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);

            //验证是否有权限
            List<DrawCashApply> listDrawCashApply = DrawCashApplyBLL.SingleModel.GetList($"Id in({ids})");
            foreach (DrawCashApply item in listDrawCashApply)
            {
                if (xcx.Id != item.Aid || item.state != 0)//表示不属于该小程序或者记录已经操作过
                {
                    return Json(new { isok = false, msg = $"非法操作(没有操作记录{item.Id})的权限" }, JsonRequestBehavior.AllowGet);
                }


            }

            bool result = DrawCashApplyBLL.SingleModel.UpdateDrawCashApply(ids, state, appId, listDrawCashApply, dzaccount.Id.ToString());
            if (result)
                return Json(new { isok = true, msg = "操作成功" }, JsonRequestBehavior.AllowGet);
            return Json(new { isok = false, msg = "操作失败" }, JsonRequestBehavior.AllowGet);




        }



        #endregion

        #region 代理商证书查询

        /// <summary>
        /// 代理证书查询
        /// </summary>
        /// <returns></returns>
        public ActionResult FindAgentareaData()
        {
            Return_Msg returnMsg = new Return_Msg();
            returnMsg.isok = false;
            returnMsg.Msg = "没有找到匹配的相关代理资料";

            string authorCode = Context.GetRequest("authorCode", string.Empty);
            if (string.IsNullOrWhiteSpace(authorCode))//授权码
            {
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@authCode", authorCode));

            //代理商信息
            Agentinfo agentInfo = AgentinfoBLL.SingleModel.GetModel($" AuthCode = @authCode ", mysqlParams.ToArray());
            if (agentInfo == null)
            {
                returnMsg.code = "1";
                returnMsg.Msg = "没有找到匹配的相关代理资料";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }

            //代理商隶属区域信息
            List<AgentArea> agentAreas = AgentAreaBLL.SingleModel.GetList($" agentId = {agentInfo.id} ");
            if (agentAreas == null || !agentAreas.Any())
            {
                returnMsg.code = "2";
                returnMsg.Msg = "该代理的证书正在制作中,请稍候再查询";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }

            //取得 省,市,区 名称
            agentAreas.ForEach(a =>
            {
                a.ProvinceStr = C_AreaBLL.SingleModel.GetNameByAreaCode(a.ProvinceCode) ?? string.Empty;
                a.CityStr = C_AreaBLL.SingleModel.GetNameByAreaCode(a.CityCode) ?? string.Empty;
                a.AreaStr = C_AreaBLL.SingleModel.GetNameByAreaCode(a.AreaCode) ?? string.Empty;
            });

            //返回一张或多张证书的资料
            object postData = agentAreas.Select(a => new
            {
                companyName = agentInfo.name,
                agentAreaName = $"{a.ProvinceStr}{a.CityStr}{a.AreaStr}",
                agentData = $"{agentInfo.addtime.ToString("yyyy.MM.dd")} - {agentInfo.OutTime.ToString("yyyy.MM.dd")}",
                agentAuthCode = agentInfo.AuthCode,
                agentStartData = $" {agentInfo.addtime.ToString("yyyy年MM月dd日起")} "
            });

            returnMsg.isok = true;
            returnMsg.Msg = $"匹配到{agentAreas.Count}份证书";
            returnMsg.dataObj = postData;
            returnMsg.code = agentAreas.Count.ToString();
            return Json(returnMsg, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 排队
        /// <summary>
        /// 排队管理页
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult SortQueueManager()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int pageType = Context.GetRequestInt("PageType", 0);

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

            pageType = xcxTemplate.Type;
            int sortShoppingSwtich = 0;//排队购物功能开关

            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
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

                    sortShoppingSwtich = funcMgr.SortShopping;

                }

            }

            ViewBag.sortShoppingSwtich = sortShoppingSwtich;

            //读取配置
            string errMsg = string.Empty;
            CommonSetting commonSetting = CommonSettingBLL.GetCommonSetting(appId, ref storeId, ref errMsg);

            ViewBag.appId = appId;
            ViewBag.storeId = storeId;
            ViewBag.PageType = pageType;
            return View(commonSetting);
        }

        /// <summary>
        /// 排队列表
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult GetSortQueues()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int sortNo = Context.GetRequestInt("sortNo", 0);
            string startTimeStr = Context.GetRequest("startTime", string.Empty);
            string endTimeStr = Context.GetRequest("endTime", string.Empty);
            string telephone = Context.GetRequest("telephone", string.Empty);
            int state = Context.GetRequestInt("state", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 0);
            int pageSize = Context.GetRequestInt("pageSize", 0);

            #region 时间处理 - 安全转换,避免报错
            DateTime defaultTime = Convert.ToDateTime("0001-01-01 00:00:00");
            DateTime startTime = defaultTime;
            DateTime endTime = defaultTime;

            DateTime.TryParse(startTimeStr, out startTime);
            DateTime.TryParse(endTimeStr, out endTime);
            #endregion

            int recordCount = 0;
            List<SortQueue> sortQueues = SortQueueBLL.SingleModel.GetListByWhere(ref recordCount, appId, storeId, state, pageIndex, pageSize, startTime, endTime, telephone, sortNo);

            Return_Msg result = new Return_Msg();
            result.isok = true;
            result.dataObj = new
            {
                sortQueues = sortQueues,
                recordCount = recordCount
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 操作队列排队
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult UpdateSortQueuesState()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int sortId = Context.GetRequestInt("sortId", 0);
            int state = Context.GetRequestInt("state", 0);

            Return_Msg result = new Return_Msg();
            result.isok = false;

            if (sortId <= 0)
            {
                result.Msg = "没有接收到队列记录标识";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            List<SortQueue> all_sortQueueing = SortQueueBLL.SingleModel.GetListByQueueing(appId, storeId);
            if (all_sortQueueing == null)
            {
                result.Msg = "更新失败";
            }

            SortQueue curSortQueue = all_sortQueueing.FirstOrDefault(s => s.id == sortId);
            if (curSortQueue == null || curSortQueue.id < 0 || curSortQueue.state != 0)
            {
                result.Msg = "该排队记录已失效或已经处理";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            curSortQueue.state = state;
            curSortQueue.updateDate = DateTime.Now;
            bool isSuccess = SortQueueBLL.SingleModel.Update(curSortQueue, "state,updateDate");

            result.isok = isSuccess;
            result.Msg = isSuccess ? "更新成功" : "更新失败";
            if (isSuccess)
            {
                string errMsg = string.Empty;
                //发送模板消息通知到号或即将到号用户
                SortQueueBLL.SingleModel.SendTemplateMsgToNextUser(ref errMsg, curSortQueue.aId, curSortQueue.storeId, all_sortQueueing);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改排队状态开关
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult ModifySortQueueSwitch()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            string sortQueueSwitchStr = Context.GetRequest("sortQueueSwitch", "false");

            Return_Msg result = new Return_Msg();
            if (dzaccount == null)
            {
                result.isok = false;
                result.Msg = "登录信息超时";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.isok = false;
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                result.isok = false;
                result.Msg = "找不到模板";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if ((int)TmpType.小程序专业模板 == xcxTemplate.Type)
            {
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    result.isok = false;
                    result.Msg = "此功能未开启";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                FuncMgr funcMgr = new FuncMgr();

                if (!string.IsNullOrEmpty(functionList.FuncMgr))
                {
                    funcMgr = JsonConvert.DeserializeObject<FuncMgr>(functionList.FuncMgr);
                }
                if (funcMgr.SortShopping == 1)
                {
                    result.isok = false;
                    result.Msg = "请升级到更高版本才能开启此功能";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }


            }



            string errMsg = string.Empty;
            bool sortQueueSwitch = false;
            if (!bool.TryParse(sortQueueSwitchStr, out sortQueueSwitch))
            {
                result.isok = false;
                result.Msg = "没有接收到开关的设定";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            //读取配置
            CommonSetting commonSetting = CommonSettingBLL.GetCommonSetting(appId, ref storeId, ref errMsg);
            commonSetting.sortQueueSwitch = sortQueueSwitch;

            result = CommonSettingBLL.UpdateCommonSetting(commonSetting, appId, storeId);
            result.Msg = (sortQueueSwitch ? "开启队列" : "关闭队列") + (result.isok ? "成功" : "失败");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 重置排队号
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult ResetSortNo()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);

            Return_Msg result = new Return_Msg();
            result.isok = false;


            List<SortQueue> curSortQueues = SortQueueBLL.SingleModel.GetListByQueueing(appId, storeId);
            if (curSortQueues != null && curSortQueues.Any())
            {
                result.Msg = "无队列数据时才能重置";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            //读取配置
            string errMsg = string.Empty;
            CommonSetting commonSetting = CommonSettingBLL.GetCommonSetting(appId, ref storeId, ref errMsg);
            if (!string.IsNullOrWhiteSpace(errMsg))
            {
                result.Msg = errMsg;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            //更新配置
            commonSetting.sortNo_next = 1;
            result = CommonSettingBLL.UpdateCommonSetting(commonSetting, appId, storeId);
            result.Msg = result.isok ? "重置队列成功" : "重置队列失败";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 订单单号查询

        /// <summary>
        /// 微信订单查询页面
        /// </summary>
        /// <returns></returns>
        [LoginFilter][RouteAuthCheck]
        public ActionResult FindWxOrderMsgForm()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int pageType = Context.GetRequestInt("pageType", 0);

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

            ViewBag.appId = appId;
            ViewBag.PageType = pageType;
            return View();
        }

        /// <summary>
        /// 根据系统单号查询微信订单资料
        /// </summary>
        /// <returns></returns>
        public ActionResult FindWxOrderNum()
        {
            Return_Msg returnMsg = new Return_Msg();
            returnMsg.isok = false;

            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                returnMsg.Msg = "未找到系统标识 => appId";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }

            string orderNum = Context.GetRequest("orderNum", string.Empty);
            if (string.IsNullOrWhiteSpace(orderNum))
            {
                returnMsg.Msg = "没有接收到请求查询的订单号";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }


            List<MySqlParameter> sqlParams = new List<MySqlParameter>();
            sqlParams.Add(new MySqlParameter("@orderNum", orderNum));

            List<wxOrderNumMsg> orderMsgs = new List<wxOrderNumMsg>();

            #region 找到对应的订单数据
            //电商版商品订单
            Store store = StoreBLL.SingleModel.GetModelByRid(appId);
            if (store != null)
            {
                List<StoreGoodsOrder> order_Store = StoreGoodsOrderBLL.SingleModel.GetListByParam($" storeId = {store.Id} and orderNum = @orderNum ", sqlParams.ToArray());
                if (order_Store != null)
                {
                    foreach (StoreGoodsOrder order in order_Store)
                    {
                        orderMsgs.Add(new wxOrderNumMsg()
                        {
                            system_orderId = order.OrderNum,
                            cityMorderId = order.OrderId,
                            orderPrice = order.BuyPrice,
                            userId = order.UserId,
                            orderSource = "普通订单",
                        });
                    }
                }
            }




            //餐饮版商品订单
            Food store_Food = FoodBLL.SingleModel.GetModelByAppId(appId);
            if (store_Food != null)
            {
                List<FoodGoodsOrder> order_Food = FoodGoodsOrderBLL.SingleModel.GetListByParam($" storeId = {store_Food.Id} and orderNum = @orderNum ", sqlParams.ToArray());
                if (order_Food != null)
                {
                    foreach (FoodGoodsOrder order in order_Food)
                    {
                        orderMsgs.Add(new wxOrderNumMsg()
                        {
                            system_orderId = order.OrderNum,
                            cityMorderId = order.OrderId,
                            orderPrice = order.BuyPrice,
                            userId = order.UserId,
                            orderSource = "普通订单"
                        });
                    }
                }
            }


            //专业版,多门店商品订单   ||  拼团(高级)订单
            List<EntGoodsOrder> order_Ent = EntGoodsOrderBLL.SingleModel.GetListByParam($" aId = {appId} and orderNum = @orderNum ", sqlParams.ToArray());
            if (order_Ent != null)
            {
                foreach (EntGoodsOrder order in order_Ent)
                {
                    orderMsgs.Add(new wxOrderNumMsg()
                    {
                        system_orderId = order.OrderNum,
                        cityMorderId = order.OrderId,
                        orderPrice = order.BuyPrice,
                        userId = order.UserId,
                        orderSource = returnEntOrderSource(order)
                    });
                }
            }

            //砍价订单
            List<BargainUser> order_Bar = BargainUserBLL.SingleModel.GetListByParam($" orderId = @orderNum ", sqlParams.ToArray());
            if (order_Bar != null)
            {
                foreach (BargainUser order in order_Bar)
                {
                    orderMsgs.Add(new wxOrderNumMsg()
                    {
                        system_orderId = order.OrderId,
                        cityMorderId = order.CityMordersId,
                        orderPrice = order.CurrentPrice,
                        userId = order.UserId,
                        orderSource = "砍价订单",
                    });
                }
            }


            //拼团订单
            List<GroupUser> order_Group = GroupUserBLL.SingleModel.GetListByParam($" OrderNo = @orderNum ", sqlParams.ToArray());
            if (order_Group != null)
            {
                foreach (GroupUser order in order_Group)
                {
                    orderMsgs.Add(new wxOrderNumMsg()
                    {
                        system_orderId = order.OrderNo,
                        cityMorderId = order.OrderId,
                        orderPrice = order.BuyPrice,
                        userId = order.ObtainUserId,
                        orderSource = "拼团订单",
                    });
                }
            }
            #endregion

            //补充信息
            C_UserInfo curUserInfo = null;
            CityMorders curCityMorder = null;

            string userIds = string.Join(",",orderMsgs?.Select(s=>s.userId).Distinct());
            List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

            foreach (wxOrderNumMsg curOrder in orderMsgs)
            {
                //付款者微信昵称
                curUserInfo = userInfoList?.FirstOrDefault(f=>f.Id == curOrder.userId);
                curOrder.userName = curUserInfo?.NickName;

                //移除没有微信订单的订单
                if (curOrder.cityMorderId <= 0)
                {
                    curOrder.stateMsg = "此订单并无生成微信订单";
                    continue;
                }
                else
                {
                    curCityMorder = _citymordersBll.GetModel(curOrder.cityMorderId);
                    //找不到相应订单
                    if (curCityMorder == null)
                    {
                        curOrder.stateMsg = "当前订单没有生成微信订单";
                        continue;
                    }

                    //订单未支付
                    if (curCityMorder.payment_status != 1 || curCityMorder.payment_status != 1)
                    {
                        curOrder.stateMsg = "当前订单还未付款,未生成有效的微信订单号";
                        continue;
                    }

                    curOrder.wx_orderId = curCityMorder.trade_no;
                    curOrder.entity_orderId = curCityMorder.orderno;
                }
            }

            //有效
            returnMsg.isok = true;
            returnMsg.Msg = orderMsgs.Any() ? "成功查询" : "无匹配订单";
            returnMsg.dataObj = orderMsgs;
            return Json(returnMsg, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 根据微信订单号查询系统订单号资料
        /// </summary>
        /// <returns></returns>
        public ActionResult FindSystemOrderNum()
        {
            Return_Msg returnMsg = new Return_Msg();
            returnMsg.isok = false;

            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                returnMsg.Msg = "未找到系统标识 => appId";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }

            string wxPayNum = Context.GetRequest("wxPayNum", string.Empty);
            if (string.IsNullOrWhiteSpace(wxPayNum))
            {
                returnMsg.Msg = "没有接收到请求查询的微信订单号";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }


            List<MySqlParameter> sqlParams = new List<MySqlParameter>();
            sqlParams.Add(new MySqlParameter("@orderNum", wxPayNum));

            CityMorders cityMorders = _citymordersBll.GetModel($"trade_no = @orderNum", sqlParams.ToArray());
            if (cityMorders == null)
            {
                returnMsg.Msg = "无匹配订单";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }


            sqlParams = new List<MySqlParameter>();
            sqlParams.Add(new MySqlParameter("@orderNum", cityMorders.Id));

            List<wxOrderNumMsg> orderMsgs = new List<wxOrderNumMsg>();

            #region 找到对应的订单数据
            //电商版商品订单
            Store store = StoreBLL.SingleModel.GetModelByRid(appId);
            if (store != null)
            {
                List<StoreGoodsOrder> order_Store = StoreGoodsOrderBLL.SingleModel.GetListByParam($" storeId = {store.Id} and OrderId = @orderNum ", sqlParams.ToArray());
                if (order_Store != null)
                {
                    foreach (StoreGoodsOrder order in order_Store)
                    {
                        orderMsgs.Add(new wxOrderNumMsg()
                        {
                            system_orderId = order.OrderNum,
                            cityMorderId = order.OrderId,
                            orderPrice = order.BuyPrice,
                            userId = order.UserId,
                            orderSource = "普通订单"
                        });
                    }
                }
            }




            //餐饮版商品订单
            Food store_Food = FoodBLL.SingleModel.GetModelByAppId(appId);
            if (store_Food != null)
            {
                List<FoodGoodsOrder> order_Food = FoodGoodsOrderBLL.SingleModel.GetListByParam($" storeId = {store_Food.Id} and OrderId = @orderNum ", sqlParams.ToArray());
                if (order_Food != null)
                {
                    foreach (FoodGoodsOrder order in order_Food)
                    {
                        orderMsgs.Add(new wxOrderNumMsg()
                        {
                            system_orderId = order.OrderNum,
                            cityMorderId = order.OrderId,
                            orderPrice = order.BuyPrice,
                            userId = order.UserId,
                            orderSource = "普通订单"
                        });
                    }
                }
            }


            //专业版,多门店商品订单   ||  拼团(高级)订单
            List<EntGoodsOrder> order_Ent = EntGoodsOrderBLL.SingleModel.GetListByParam($" aId = {appId} and OrderId = @orderNum ", sqlParams.ToArray());
            if (order_Ent != null)
            {
                foreach (EntGoodsOrder order in order_Ent)
                {
                    orderMsgs.Add(new wxOrderNumMsg()
                    {
                        system_orderId = order.OrderNum,
                        cityMorderId = order.OrderId,
                        orderPrice = order.BuyPrice,
                        userId = order.UserId,
                        orderSource = returnEntOrderSource(order)
                    });
                }
            }

            //砍价订单
            List<BargainUser> order_Bar = BargainUserBLL.SingleModel.GetListByParam($" CityMordersId = @orderNum ", sqlParams.ToArray());
            if (order_Bar != null)
            {
                foreach (BargainUser order in order_Bar)
                {
                    orderMsgs.Add(new wxOrderNumMsg()
                    {
                        system_orderId = order.OrderId,
                        cityMorderId = order.CityMordersId,
                        orderPrice = order.CurrentPrice,
                        userId = order.UserId,
                        orderSource = "砍价订单",
                    });
                }
            }


            //拼团订单
            List<GroupUser> order_Group = GroupUserBLL.SingleModel.GetListByParam($" OrderId = @orderNum ", sqlParams.ToArray());
            if (order_Group != null)
            {
                foreach (GroupUser order in order_Group)
                {
                    orderMsgs.Add(new wxOrderNumMsg()
                    {
                        system_orderId = order.OrderNo,
                        cityMorderId = order.OrderId,
                        orderPrice = order.BuyPrice,
                        userId = order.ObtainUserId,
                        orderSource = "拼团订单",
                    });
                }
            }
            #endregion

            //补充信息
            C_UserInfo curUserInfo = null;
            CityMorders curCityMorder = null;

            string userIds = string.Join(",", orderMsgs?.Select(s => s.userId).Distinct());
            List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

            foreach (wxOrderNumMsg curOrder in orderMsgs)
            {
                //付款者微信昵称
                curUserInfo = userInfoList?.FirstOrDefault(f=>f.Id==curOrder.userId);
                curOrder.userName = curUserInfo?.NickName;

                //移除没有微信订单的订单
                if (curOrder.cityMorderId <= 0)
                {
                    curOrder.stateMsg = "此订单并无生成微信订单";
                    continue;
                }
                else
                {
                    curCityMorder = cityMorders;
                    //找不到相应订单
                    if (curCityMorder == null)
                    {
                        curOrder.stateMsg = "当前订单没有生成微信订单";
                        continue;
                    }

                    //订单未支付
                    if (curCityMorder.payment_status != 1 || curCityMorder.payment_status != 1)
                    {
                        curOrder.stateMsg = "当前订单还未付款,未生成有效的微信订单号";
                        continue;
                    }

                    curOrder.wx_orderId = curCityMorder.trade_no;
                    curOrder.entity_orderId = curCityMorder.orderno;
                }
            }

            //有效
            returnMsg.isok = true;
            returnMsg.Msg = orderMsgs.Any() ? "成功查询" : "无匹配订单";
            returnMsg.dataObj = orderMsgs;
            return Json(returnMsg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// entGoodsOrder表数据源
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public string returnEntOrderSource(EntGoodsOrder order)
        {
            string orderSource = string.Empty;
            switch (order.OrderType)
            {
                case (int)EntOrderType.订单:
                    orderSource = "普通订单";
                    break;
                default:
                    orderSource = Enum.GetName(typeof(EntOrderType), order.OrderType);
                    break;
            }
            return orderSource;
        }


        //微信订单查询订单资料信息
        protected class wxOrderNumMsg
        {
            //ctiryMorder Id
            public int cityMorderId { get; set; }

            //商户订单号
            public string entity_orderId { get; set; }

            //微信支付订单号
            public string wx_orderId { get; set; }

            //系统内订单号
            public string system_orderId { get; set; }

            // 订单金额
            public int orderPrice { get; set; }
            public string orderPriceStr
            {
                get
                {
                    return (orderPrice * 0.01).ToString("0.00");
                }
            }

            //付款人c_userInfo Id
            public int userId { get; set; }

            //付款人微信昵称
            public string userName { get; set; }

            //状态信息
            public string stateMsg { get; set; }

            /// <summary>
            /// 订单来源
            /// </summary>
            public string orderSource { get; set; }
        }
        #endregion

        //根据aId获取小程序头像
        public ActionResult GetShoperLogoUrl()
        {
            int aId = Context.GetRequestInt("appId", 0);

            Return_Msg returnMsg = new Return_Msg();
            returnMsg.isok = false;

            if (aId <= 0)
            {
                returnMsg.Msg = "获取失败,没有接收到必要参数";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(aId);
            if (xcx == null)
            {
                returnMsg.Msg = "获取失败,没有接收到有效的必要参数";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }

            OpenAuthorizerConfig config = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcx.AppId);
            if (config == null)
            {
                returnMsg.Msg = "请先进行小程序授权";
                return Json(returnMsg, JsonRequestBehavior.AllowGet);
            }

            returnMsg.isok = true;
            returnMsg.Msg = "成功获取头像";
            returnMsg.dataObj = new
            {
                headImg = config.head_img
            };
            return Json(returnMsg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditUserType()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int uid = Context.GetRequestInt("userId", 0);
            int userType = Context.GetRequestInt("userType", 0);
            string phone = Context.GetRequest("phone", string.Empty);
            if (appId <= 0 || uid <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcxRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(uid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "会员不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (userType == 1 && (string.IsNullOrEmpty(phone) || phone.Length != 11))
            {
                return Json(new { isok = false, msg = "手机号码错误" }, JsonRequestBehavior.AllowGet);
            }
            string columnFields = "usertype";
            userInfo.userType = userType;
            if (userType == 1)
            {
                columnFields += ",telephone";
                userInfo.TelePhone = phone;
            }
            bool isok = C_UserInfoBLL.SingleModel.Update(userInfo, columnFields);
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok, msg });
        }

        #region 商品评论

        /// <summary>
        /// 商品管理列表
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult GoodsCommentList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int type = Context.GetRequestInt("type", 0);
            string goodsname = Context.GetRequest("goodsname", string.Empty);
            int pageSize = Context.GetRequestInt("pageSize", 20);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙appId_null!", code = "500" });
            }
            XcxAppAccountRelation xcxRole = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (xcxRole == null || xcxRole.AccountId != dzaccount.Id)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙xcxRole_null!", code = "500" });
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxRole.TId);
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcxRole.VersionId;
            }

            int count = 0;
            ViewModel<GoodsComment> viewmodel = new ViewModel<GoodsComment>();
            viewmodel.DataList = GoodsCommentBLL.SingleModel.GetGoodsCommentList(goodsname, appId, storeId, pageIndex, pageSize, ref count, true);
            viewmodel.TotalCount = count;
            viewmodel.PageIndex = pageIndex;
            viewmodel.PageSize = pageSize;
            ViewBag.GoodsName = goodsname;
            ViewBag.appId = appId;
            ViewBag.StoreId = storeId;
            ViewBag.Type = type;
            ViewBag.PageType = xcxTemplate.Type;

            return View(viewmodel);
        }

        public ActionResult DelComment()
        {
            Return_Msg returndata = new Return_Msg();
            int id = Context.GetRequestInt("id", 0);
            int state = Context.GetRequestInt("state", 1);
            int appId = Context.GetRequestInt("appId", 0);
            if (dzaccount == null)
            {
                returndata.Msg = "用户未登录!";
                return Json(returndata);
            }
            if (id <= 0)
            {
                returndata.Msg = "系统繁忙id_null!";
                return Json(returndata);
            }
            if (appId <= 0)
            {
                returndata.Msg = "系统繁忙appId_null!";
                return Json(returndata);
            }
            XcxAppAccountRelation xcxRole = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (xcxRole == null || xcxRole.AccountId != dzaccount.Id)
            {
                returndata.Msg = "系统繁忙xcxRole_null!";
                return Json(returndata);
            }

            GoodsComment model = GoodsCommentBLL.SingleModel.GetModel(id);
            if (model == null)
            {
                returndata.Msg = "系统繁忙model_null!";
                return Json(returndata);
            }
            model.State = state;
            model.UpdateTime = DateTime.Now;

            returndata.isok = GoodsCommentBLL.SingleModel.Update(model, "state,updatetime");
            returndata.Msg = returndata.isok ? "删除成功" : "删除失败!";
            return Json(returndata);
        }

        [RouteAuthCheck]
        public ActionResult AddOrEditGoodsComment()
        {
            int id = Context.GetRequestInt("id", 0);
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (id <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙Id_null!", code = "500" });
            }
            GoodsComment model = GoodsCommentBLL.SingleModel.GetModelByIdState(id, 1);
            if (model == null)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙model_null!", code = "500" });
            }
            model.CommentImgs = C_AttachmentBLL.SingleModel.GetListByCache(model.Id, (int)AttachmentItemType.小程序商品评论轮播图);
            XcxAppAccountRelation xcxRole = XcxAppAccountRelationBLL.SingleModel.GetModel(model.AId);
            if (xcxRole == null || xcxRole.AccountId != dzaccount.Id)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙xcxRole_null!", code = "500" });
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxRole.TId);
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcxRole.VersionId;
            }

            ViewBag.appId = model.AId;
            ViewBag.PageType = xcxTemplate.Type;

            return View(model);
        }

        public ActionResult SaveGoodsComment(GoodsComment model)
        {
            Return_Msg returndata = new Return_Msg();
            if (dzaccount == null)
            {
                returndata.Msg = "用户未登录!";
                return Json(returndata);
            }
            if (model == null)
            {
                returndata.Msg = "系统繁忙model_null!";
                return Json(returndata);
            }
            if (model.Id <= 0)
            {
                returndata.Msg = "系统繁忙id_null!";
                return Json(returndata);
            }
            if (model.AId <= 0)
            {
                returndata.Msg = "系统繁忙appId_null!";
                return Json(returndata);
            }
            XcxAppAccountRelation xcxRole = XcxAppAccountRelationBLL.SingleModel.GetModel(model.AId);
            if (xcxRole == null || xcxRole.AccountId != dzaccount.Id)
            {
                returndata.Msg = "系统繁忙xcxRole_null!";
                return Json(returndata);
            }

            returndata.isok = GoodsCommentBLL.SingleModel.Update(model, "updatetime,Hidden,Replay");
            returndata.Msg = returndata.isok ? "保存成功" : "保存失败";
            return Json(returndata);
        }
        #endregion
    }
}