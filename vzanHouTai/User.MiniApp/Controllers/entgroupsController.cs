using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BLL.MiniApp;
using Core.MiniApp;
using Newtonsoft.Json;
using User.MiniApp.Comment;
using User.MiniApp.Model;
using Entity.MiniApp;
using System.IO;
using System.Text;
using Utility.AliOss;
using System.Web.Script.Serialization;
using Entity.MiniApp.Tools;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using DAL.Base;
using User.MiniApp.Filters;
using MySql.Data.MySqlClient;
using BLL.MiniApp.Ent;
using Entity.MiniApp.Ent;
using Utility.IO;
using BLL.MiniApp.FunList;
using Entity.MiniApp.FunctionList;

namespace User.MiniApp.Controllers
{
    public class entgroupsController : baseController
    {



        public entgroupsController()
        {



        }
        //拼团列表管理
        // GET: CityCoupon

        [LoginFilter][RouteAuthCheck]
        public ActionResult MiniappStoreGroupsManager()
        {
            int appId = Context.GetRequestInt("appId",0);
                int PageType =Context.GetRequestInt("PageType", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string title = Context.GetRequest("Title", string.Empty);
            int State = Context.GetRequestInt("State", 0);

            if (appId<=0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (PageType <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            #region 专业版 版本控制

            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int EntjoingroupSwtich = 0;//拼团开关
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
                    EntjoingroupSwtich = comsConfig.Entjoingroup;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.EntjoingroupSwtich = EntjoingroupSwtich;
            #endregion
            
            int count = 0;
            List<EntGroupsRelation> list = new List<EntGroupsRelation>();
            list = EntGroupsRelationBLL.SingleModel.GetListGroups(PageType,appId, title, State,ref count, pageSize,pageIndex);

            ViewBag.TotalCount = count;
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            ViewBag.pageSize = pageSize;

            return View(list);
        }


        //拼团购买记录
        [LoginFilter]
        public ActionResult GroupList()
        {
            int PageType = Context.GetRequestInt("PageType", 0);
            int appId = Context.GetRequestInt("appId", 0);
            int groupid = Context.GetRequestInt("id", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 20);
            int groupstate = Context.GetRequestInt("groupstate", 999);

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if(xcxrelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "还未授权!", code = "500" });
            }

            int count = 0;
            List<EntGroupSponsor> grouplist = EntGroupSponsorBLL.SingleModel.GetListByGroupId(groupid,groupstate,pageIndex,pageSize,ref count);

            ViewBag.TotalCount = count;
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            ViewBag.pageSize = pageSize;
            ViewBag.groupstate = groupstate;

            return View(grouplist);
        }
        

        /// <summary>
        /// 修改拼团状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult UpdateState()
        {
            int id = Context.GetRequestInt("id",0);
            int state = Context.GetRequestInt("state", 0);
            int type = Context.GetRequestInt("type", 0);

            string result = EntGroupsRelationBLL.SingleModel.UpdateGroupState(id,state,type);

            return Json(new { isok = true, msg = result });
        }
    }
}