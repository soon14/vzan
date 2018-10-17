using Api.MiniApp.Models;
using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using BLL.MiniApp.Conf;

namespace Api.MiniApp.Controllers
{
    public class apiOfficalController : InheritController
    {
    }
    public class apiMiappController : apiOfficalController
    {
        private readonly Regex _regex = new Regex("<(?!img).+?>");//正则表达式匹配除了img标签的所有标签
        private readonly Regex _regex2 = new Regex("<(?!img|span|/span).+?>");//正则表达式匹配除了img和span标签的所有标签1
        private readonly Regex _regex3 = new Regex("<(?!img|p|/p).+?>");//正则表达式匹配除了img和p标签的所有标签
        private readonly Regex _regex4 = new Regex("<.+?>");//正则表达式匹配html标签


        public apiMiappController()
        {

        }

        /// <summary>
        /// 获得板块数据
        /// </summary>
        /// <param name="appid">模块ID</param>
        /// <param name="level">板块号</param>
        /// <returns></returns>
        public ActionResult GetModelData(string appid, int level, string pageIndex, int pageSize = 5)
        {
            MiappJSModel<Moduls> strjson = new MiappJSModel<Moduls>();
            strjson.Code = 0;
            strjson.Message = "Success";
            XcxAppAccountRelation relationmodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (relationmodel == null)
            {
                strjson.Code = 500;
                strjson.Message = "模板已过期！";
                return Json(strjson, JsonRequestBehavior.AllowGet);
            }

            XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModel(relationmodel.TId);
            if (xcxtemplate == null || xcxtemplate.Type != (int)TmpType.小程序企业模板)
            {
                strjson.Code = 500;
                strjson.Message = "模板不存在！";
                return Json(strjson, JsonRequestBehavior.AllowGet);
            }
            try
            {
                //wx61575c2a72a69def
                if (string.IsNullOrWhiteSpace(appid))
                {
                    strjson.Code = 500;
                    strjson.Message = "模块appId不能为空！";
                    return Json(strjson, JsonRequestBehavior.AllowGet);
                }
                if (level < 1 || level > (int)Miapp_Miniappmoduls_Level.EightModel)
                {
                    strjson.Code = 500;
                    strjson.Message = "板块号不能为" + level;
                    return Json(strjson, JsonRequestBehavior.AllowGet);
                }

                int pageIndexInt = 1;
                int.TryParse(pageIndex, out pageIndexInt);

                List<Miniapp> appmodel = MiniappBLL.SingleModel.GetList("ModelId = '" + appid + "'");
                if (appmodel == null || appmodel.Count <= 0)
                {
                    strjson.Code = 500;
                    strjson.Message = "模块中没有存在ModelId=" + appid;
                    return Json(strjson, JsonRequestBehavior.AllowGet);
                }
                string wheresql = $"Level = {level} and State = 1 and appId = {appmodel[0].Id}";

                List<Moduls> data = ModulsBLL.SingleModel.GetList(wheresql, pageSize, pageIndexInt, "", "Createdate DESC");
                strjson.data = data;
                //判断版块
                switch (level)
                {
                    case (int)Miapp_Miniappmoduls_Level.ModelData: return GetTopModelData(data);//首页
                    case (int)Miapp_Miniappmoduls_Level.FirstModel: return GetProductModelData(data);
                    case (int)Miapp_Miniappmoduls_Level.TwoModel: return GetProductModelData(data);
                    case (int)Miapp_Miniappmoduls_Level.ThreeModel: return GetProductModelData(data);
                    case (int)Miapp_Miniappmoduls_Level.FourModel: return GetProductModelData2(data);//产品展示
                    case (int)Miapp_Miniappmoduls_Level.FiveModel: return GetDevelopmentModelData(data, level, pageSize, pageIndexInt);//发展历程
                    case (int)Miapp_Miniappmoduls_Level.SixModel: return GetTopModelData(data);//尾页
                    case (int)Miapp_Miniappmoduls_Level.EightModel: return GetNewsList(data);//新闻
                }
            }
            catch (Exception ex)
            {
                strjson.Code = 500;
                strjson.Message = ex.Message;
            }

            return Json(strjson, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetImg(string appid)
        {
            MiappJSModel<Moduls> strjson = new MiappJSModel<Moduls>();
            strjson.Code = 0;
            strjson.Message = "Success";
            try
            {
                XcxAppAccountRelation xcxrelationmodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
                if (xcxrelationmodel == null)
                {
                    strjson.Code = 500;
                    strjson.Message = "请先开通企业版";
                }
                else
                {

                    if (string.IsNullOrWhiteSpace(appid))
                    {
                        strjson.Code = 500;
                        strjson.Message = "模块appId不能为空！";
                        return Json(new { data = "", isok = 0, msg = strjson.Message }, JsonRequestBehavior.AllowGet);
                    }

                    List<ConfParam> model = ConfParamBLL.SingleModel.GetListByRId(xcxrelationmodel.Id);
                    return Json(new { data = model, isok = 1 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                strjson.Code = 500;
                strjson.Message = ex.Message;
            }

            return Json(new { data = "", isok = 0, msg = strjson.Message }, JsonRequestBehavior.AllowGet);
        }

        #region 首页
        public ActionResult GetTopModelData(List<Moduls> data)
        {
            MiappJSModel<Moduls> strjson = new MiappJSModel<Moduls>();
            strjson.Code = 0;
            strjson.Message = "Success";

            if (data != null && data.Count > 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].Level == 1)
                    {
                        List<C_Attachment> imgs = C_AttachmentBLL.SingleModel.GetListByCache(data[i].Id, (int)AttachmentItemType.小程序官网首页轮播图);
                        if (imgs != null && imgs.Count > 0)
                        {
                            string imgurls = string.Join(",", imgs.Select(s => s.filepath));
                            data[i].ImgUrl = imgurls;
                        }
                    }

                    //data[i].Content = ReturnStr3(data[i].Content);
                }
            }

            strjson.data = data;

            return Json(strjson, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult GetProductModelData(List<Moduls> data)
        {
            MiappJSModel<Moduls> strjson = new MiappJSModel<Moduls>();
            strjson.Code = 0;
            strjson.Message = "Success";

            if (data != null && data.Count > 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    data[i].Content = ReturnStr(data[i].Content);
                }
            }

            strjson.data = data;

            return Json(strjson, JsonRequestBehavior.AllowGet);
        }

        #region 产品展示
        public ActionResult GetProductModelData2(List<Moduls> data)
        {
            MiappJSModel<Moduls> strjson = new MiappJSModel<Moduls>();
            strjson.Code = 0;
            strjson.Message = "Success";

            if (data != null && data.Count > 0)
            {

                for (int i = 0; i < data.Count; i++)
                {
                    data[i].Title = data[i].Title;
                    //data[i].Content = ReturnStr(data[i].Content);
                    data[i].Content = _regex4.Replace(data[i].Content.Replace("&nbsp;", ""), "").Trim();
                }
            }

            strjson.data = data;

            return Json(strjson, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 发展历程
        public ActionResult GetDevelopmentModelData(List<Moduls> data, int level, int pageSize, int pageIndexInt)
        {
            MiappJSModel<Moduls> strjson = new MiappJSModel<Moduls>();
            strjson.Code = 0;
            strjson.Message = "Success";

            if (data != null && data.Count > 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    data[i].Content = SubHtml(data[i].Content);
                }

                //获取发展历程
                string wheresql2 = "appId = '" + data[0].appId + "' and Level = " + level + " and State = 1";
                List<Development> data2 = DevelopmentBLL.SingleModel.GetList(wheresql2, pageSize, pageIndexInt, "", "Year DESC,Month DESC");
                if (data2 != null && data2.Count > 0)
                {
                    data[0].MiniappdevelopmentList = data2;
                }
            }

            strjson.data = data;

            return Json(strjson, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 企业动态
        /// <summary>
        /// 获取新闻列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ActionResult GetNewsList(List<Moduls> data)
        {
            MiappJSModel<Moduls> strjson = new MiappJSModel<Moduls>();
            strjson.Code = 0;
            strjson.Message = "Success";

            if (data != null && data.Count > 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (string.IsNullOrEmpty(data[i].Content2))
                    {
                        //字数长于100截取前100个字
                        string tempstr = _regex4.Replace(data[i].Content, "").Trim().Replace("&nbsp;", "");
                        data[i].Content = tempstr.Length <= 45 ? tempstr : tempstr.Substring(0, 45) + "...";
                    }
                    else
                    {
                        data[i].Content = data[i].Content2;
                    }
                }
            }
            else
            {
                data = new List<Moduls>();
            }

            strjson.data = data;

            return Json(strjson, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 新闻列表根据Id获取详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetModelInfoById(int id)
        {
            MiappJSModel<Moduls> strjson = new MiappJSModel<Moduls>();
            strjson.Code = 0;
            strjson.Message = "Success";

            try
            {
                if (id <= 0)
                {
                    strjson.Code = 500;
                    strjson.Message = "Id不能小于等于0";
                    return Json(strjson, JsonRequestBehavior.AllowGet);
                }
                string wheresql = "Id = " + id + " and State = 1";
                List<Moduls> data = ModulsBLL.SingleModel.GetList(wheresql);

                strjson.data = data;
            }
            catch (Exception ex)
            {
                strjson.Code = 500;
                strjson.Message = ex.Message;
            }

            return Json(strjson, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 从html中提取文本
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string SubHtml(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "";
            }

            return _regex.Replace(ReturnStr(content), "");
        }

        /// <summary>
        /// 把p标签用代替\n代替
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReturnStr(string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.IndexOf("</p>") <= 0)
            {
                return content;
            }

            string temp = content.Substring(0, content.IndexOf("</p>")) + "</p>";
            //判断p标签是否包含内容，没有则去除该标签
            if (string.IsNullOrWhiteSpace(_regex.Replace(temp, "")))
            {
                return ReturnStr(content.Replace(temp, ""));
            }

            return temp + "\n" + ReturnStr(content.Replace(temp, ""));
        }

        /// <summary>
        /// 把 &nbsp;标识用p标签样式text-indent:2rem;代替
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReturnStr2(string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.IndexOf("</p>") <= 0)
            {
                return content;
            }

            string temp = content.Substring(0, content.IndexOf("</p>")) + "</p>";
            string temp1 = _regex.Replace(temp, "");
            if (temp1.IndexOf("&nbsp;") == 0)
            {
                temp1 = temp.Replace("<p>", "<p style='text-indent:2em;'>").Replace("&nbsp;", "");
                return temp1 + ReturnStr2(content.Replace(temp, ""));
            }

            return temp + ReturnStr2(content.Replace(temp, ""));
        }

        /// <summary>
        /// 纯文本中把空格标识用p标签样式text-indent:2rem;代替
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReturnStr3(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return content;
            }

            if (content.IndexOf("\n") <= 0)
            {
                if (content.IndexOf(" ") == 0)
                {
                    content = "<p style='text-indent:2em;'>" + content + "</p>";
                }
                else
                {
                    content = "<p>" + content + "</p>";
                }
                return content;
            }

            string temp = content.Substring(0, content.IndexOf("\n"));
            if (temp.IndexOf(" ") == 0)
            {
                return "<p style='text-indent:2em;'>" + temp + "</p>\n" + ReturnStr3(content.Replace(temp + "\n", ""));
            }
            else
            {
                return "<p>" + temp + "</p>\n" + ReturnStr3(content.Replace(temp + "\n", ""));
            }
        }
        #endregion
    }
}