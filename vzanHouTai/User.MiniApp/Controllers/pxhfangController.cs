using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Home;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Home;
using Entity.MiniApp.Pin;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class pxhfangController : Controller
    {
        public ActionResult Autho()
        {
            return View();
        }
        public ActionResult GetAuthoUrl()
        {
            string returnUrl = "https://testwtapi.vzan.com/webview/GetOpenId";
            string appid = "wx64f161aa79a6801b";
            string scope = "snsapi_base";
            Return_Msg returnObj = new Return_Msg();
            returnObj.dataObj = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appid}&redirect_uri={returnUrl}&response_type=code&scope={scope}&state=1#wechat_redirect";
            returnObj.isok = true;
            return Json(returnObj);
        }

        public ActionResult Index(string openId="")
        {
            ViewBag.OpenId = openId;
            return View();
        }
        public ActionResult userinfo()
        {
            return View();
        }
        public ActionResult mobileVideoList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetUserCaseList(int type=0,int pageIndex=1,int pageSize=10,string code="",string phone="")
        {
            Return_Msg returnObj = new Return_Msg();

            //List<PinGroup> groupList = GetList($" groupcount<=successCount and state={(int)PinEnums.GroupState.拼团成功}");
            //string msg = string.Empty;
            //if (groupList != null && groupList.Count > 0)
            //{
            //    foreach (var groupInfo in groupList)
            //    {
            //        msg += ReturnMoney(groupInfo);
            //    }
            //}

            return Json(returnObj);
        }
    }
}