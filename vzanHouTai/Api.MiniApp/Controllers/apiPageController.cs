using BLL.MiniApp;
using BLL.MiniApp.Conf;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Web.Mvc;

namespace Api.MiniApp.Controllers
{
    public class apiPageController : InheritController
    {
    }
    public class apiMiappPageController : apiPageController
    {
        /// <summary>
        /// 自定义页面接口
        /// </summary>
        public apiMiappPageController()
        {
        }

        #region 单页版相关
        /// <summary>
        /// 获取自定义页面配置信息
        /// </summary>
        /// <param name="AppId"></param>
        /// <param name="PageId"></param>
        /// <returns></returns>
        public ActionResult GetPageMsg(string AppId)
        {
            if (string.IsNullOrEmpty(AppId))
            {
                return Json(new { isok = false, msg = "AppId不能为空!" }, JsonRequestBehavior.AllowGet);
            }

            var umodel = _xcxAppAccountRelationBLL.GetModel($"AppId='{AppId}'");
            if (umodel == null)
            {
                return Content("系统繁忙umodel_null");
            }
            string strWhere = $"RelationId={umodel.Id}";

            PageIndexControl model = PageIndexControlBLL.SingleModel.GetModel(strWhere);
            if (model == null)
            {
                Json(new { isok = false, msg = "首页未进行配置!" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = true, msg = "请求成功", obj = model }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 保存自定义页面表单数据
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetForm(PageFormMsg m, string openId, string AppId, string FormTitle)
        {

            if (string.IsNullOrEmpty(openId))
                return Json(new { isok = false, msg = "提交异常(openId不能为空)" });

            C_UserInfo u = C_UserInfoBLL.SingleModel.GetModel($"appId='{AppId}' and OpenId='{openId}'");
            if (u == null)
                return Json(new { isok = false, msg = "提交异常(找不到对应的用户)" });

            var rmodel = _xcxAppAccountRelationBLL.GetModelByAppid(AppId);
            if (rmodel == null)
                return Json(new { isok = false, msg = "提交异常(未进行授权)" });

            m.Rid = rmodel.Id;
            //表示新增
            m.UserId = u.Id;
            m.FormTitle = FormTitle;
            int id = Convert.ToInt32(PageFormMsgBLL.SingleModel.Add(m));
            if (id > 0)
                return Json(new { isok = true, msg = "提交成功", obj = id });
            else
                return Json(new { isok = false, msg = "提交异常" });
        }
        #endregion
    }
}