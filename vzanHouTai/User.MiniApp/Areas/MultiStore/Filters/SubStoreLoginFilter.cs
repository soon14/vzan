//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using BLL.MiniApp.Ent;
//using Entity.MiniApp.Ent;
//using BLL.MiniApp;
//using Core.MiniApp;
//using Entity.MiniApp;
//using Entity.MiniApp.User;
//using Newtonsoft.Json;
//using Utility;
//using Entity.MiniApp.Footbath;
//using BLL.MiniApp.Footbath;

//namespace User.MiniApp.Areas.MultiStore.Filters
//{
//    /// <summary>
//    /// 分店管理权限验证
//    /// </summary>
//    public class SubStoreLoginFilterAttribute : AuthorizeAttribute
//    {

//        /// <summary>
//        /// 一定验证店铺权限(默认true,传false则获取不到storeId就不去验证)
//        /// </summary>
//        public bool certainlyVerifyStoreRole = true;
        
//        private FootBathBLL _storeMateriaBLL = new FootBathBLL();
//        private MultiStoreAccountBLL _multiStoreAccountBLL = new MultiStoreAccountBLL();


//        public override void OnAuthorization(AuthorizationContext filterContext)
//        {
//            //当前登录的分店用户
//            Int32 multiStoreAccountId = 0;
//            Int32.TryParse(CookieHelper.GetCookie("dz_MultiStoreUserCookie"), out multiStoreAccountId);
//            MultiStoreAccount user = _multiStoreAccountBLL.GetMultiStoreAccountByCache(multiStoreAccountId);
//            if (user == null || user.State != 1) //找不到或者用户已失效
//            {
//                //如果是ajax请求则返回json
//                if (filterContext.HttpContext.Request.IsAjaxRequest())
//                {
//                    filterContext.Result = new System.Web.Mvc.JsonResult() { Data = new { isok = -1, msg = "未检测到登录账户,请先登录" }, ContentEncoding = System.Text.Encoding.UTF8, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "json" };
//                }
//                else
//                {
//                    filterContext.Result = new ContentResult() { Content = "未检测到登录账户,请先登录" };
//                }
//            }

//            //总店长Account
//            Account masterUserAccount = _accountBLL.GetModel(user.MasterAccountId);
//            if (masterUserAccount == null)
//            {
//                //如果是ajax请求则返回json
//                if (filterContext.HttpContext.Request.IsAjaxRequest())
//                {
//                    filterContext.Result = new System.Web.Mvc.JsonResult() { Data = new { isok = -1, msg = "权限不足 masterUserAccount_null" }, ContentEncoding = System.Text.Encoding.UTF8, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "json" };
//                }
//                else
//                {
//                    filterContext.Result = new ContentResult() { Content = "权限不足 masterUserAccount_null" };
//                }
//            }



//            #region 验证操作小程序权限
//            int appId = Utility.IO.Context.GetRequestInt("appId",0);
//            XcxAppAccountRelation role = relationBLL.GetModel(appId);
//            //判定总店长权限是否有效
//            if (role == null)
//            {  
//                //如果是ajax请求则返回json
//                if (filterContext.HttpContext.Request.IsAjaxRequest())
//                {
//                    filterContext.Result = new System.Web.Mvc.JsonResult() { Data = new { isok = -1, msg = "权限不足 role_null" }, ContentEncoding = System.Text.Encoding.UTF8, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "json" };
//                }
//                else
//                {
//                    filterContext.Result = new ContentResult() { Content = "权限不足 role_null" };
//                }
//            }

//            //检查总店长是否是这个小程序的管理者
//            if (!masterUserAccount.Id.Equals(role.AccountId))
//            {
//                //如果是ajax请求则返回json
//                if (filterContext.HttpContext.Request.IsAjaxRequest())
//                {
//                    filterContext.Result = new System.Web.Mvc.JsonResult() { Data = new { isok = -1, msg = "无操作权限 role_misfit" }, ContentEncoding = System.Text.Encoding.UTF8, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "json" };
//                }
//                else
//                {
//                    filterContext.Result = new ContentResult() { Content = "无操作权限 role_misfit" };
//                }
//            }

//            #endregion

//            #region 验证店铺管理者权限
//            int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);

//            //certainlyVerifyStoreRole设定一定要验证时,storeId没传也要验证
//            if (certainlyVerifyStoreRole || storeId != 0)
//            {
//                FootBath storeMateria = _storeMateriaBLL.GetModelByParam(appId, storeId, (int)TmpType.小程序多门店模板);
//                if (storeMateria == null)
//                {
//                    //如果是ajax请求则返回json
//                    if (filterContext.HttpContext.Request.IsAjaxRequest())
//                    {
//                        filterContext.Result = new System.Web.Mvc.JsonResult() { Data = new { isok = -1, msg = "找不到这个门店 storeMateria_null" }, ContentEncoding = System.Text.Encoding.UTF8, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "json" };
//                    }
//                    else
//                    {
//                        filterContext.Result = new ContentResult() { Content = "找不到这个门店 storeMateria_null" };
//                    }
//                }
//                else
//                {
//                    //店长需要判定是否当前店铺管理者
//                    if (!user.AccountId.Equals(user.MasterAccountId)    //非总店长
//                            && !string.IsNullOrWhiteSpace(storeMateria.ShopManager)   //设定了管理者
//                                && Convert.ToInt32(storeMateria.ShopManager) != user.Id)   //是否当前登录用户
//                    {
//                        //如果是ajax请求则返回json
//                        if (filterContext.HttpContext.Request.IsAjaxRequest())
//                        {
//                            filterContext.Result = new System.Web.Mvc.JsonResult() { Data = new { isok = -1, msg = "无操作权限,门店不属于当前登录用户管辖 role_error" }, ContentEncoding = System.Text.Encoding.UTF8, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "json" };
//                        }
//                        else
//                        {
//                            filterContext.Result = new ContentResult() { Content = "无操作权限,门店不属于当前登录用户管辖 role_error" };
//                        }
//                    }
//                }
//            }
//            else if (certainlyVerifyStoreRole && storeId == 0)
//            {
//                //如果是ajax请求则返回json
//                if (filterContext.HttpContext.Request.IsAjaxRequest())
//                {
//                    filterContext.Result = new System.Web.Mvc.JsonResult() { Data = new { isok = -1, msg = "找不到这个门店 storeId_null" }, ContentEncoding = System.Text.Encoding.UTF8, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "json" };
//                }
//                else
//                {
//                    filterContext.Result = new ContentResult() { Content = "找不到这个门店 storeId_null" };
//                }
//            }

//            #endregion
//        }
//    }
//}