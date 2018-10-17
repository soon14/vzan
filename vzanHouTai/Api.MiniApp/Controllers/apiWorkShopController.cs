using System.Web.Mvc;
using Entity.MiniApp.Workshop;
using BLL.MiniApp.Workshop;
using Utility;
using Utility.IO;
using System.Collections.Generic;
using System;
using Core.MiniApp;
using BLL.MiniApp;
using Entity.MiniApp.Conf;
using Newtonsoft.Json;
using System.Web.Configuration;
using BLL.MiniApp.Conf;
using MySql.Data.MySqlClient;
namespace Api.MiniApp.Controllers
{
    /// <summary>
    /// 小未工坊
    /// </summary>
    [ExceptionLog]
    public class apiWorkShopController : InheritController
    {
        private static readonly CustomPage _customPage=new CustomPage();
        
        
        
        public apiWorkShopController()
        {

        }
        public ActionResult GetAppConfig(string appid="")
        {
            if (string.IsNullOrEmpty(appid))
            {
                return ApiResult(false, "未找到配置");
            }
            ConfParam config = ConfParamBLL.SingleModel.GetModel($"appid='{appid}' and param='appconfig'");
            if (config == null)
            {
                return ApiResult(false, "未找到'appconfig'配置");
            }
            return ApiResult(true,config.Value);
        }
        public ActionResult GetUserPage(int uid=0,int pageSize=10,int pageIndex=1,int state = 1,int filterForm=0)
        {
            if (uid <= 0)
            {
                return ApiResult(false, "非法参数");
            }
            string sql = $"uid={uid} and state={state}";
            if (filterForm == 1)
            {
                sql=$"uid={uid} and state={state} and content like '%\"type\":\"form\",%'";
            }
            List<CustomPage> customPageList = CustomPageBLL.SingleModel.GetList(sql, pageSize, pageIndex,"*","id desc");
            return ApiResult(true, string.Empty, customPageList);
        }
        public ActionResult GetUserPageInfo(int id=0,int uid=0)
        {
            if (id<=0|| uid <= 0)
            {
                return ApiResult(false, "非法参数");
            }
            CustomPage model = CustomPageBLL.SingleModel.GetModel(id);
            if (model == null)
            {
                return ApiResult(false, "数据不存在");
            }
            return ApiResult(true,string.Empty,model);
        }
        public ActionResult SavePage(CustomPage pageModel)
        {
            if (pageModel.uid <= 0)
            {
                return ApiResult(false, "非法参数");
            }
            if (pageModel.content.Length == 0)
            {
                return ApiResult(false, "无法保存空页面");
            }
            if (pageModel.id == 0)
            {
                int pageid = Convert.ToInt32(CustomPageBLL.SingleModel.Add(pageModel));
                pageModel.id = pageid;
                if (pageid > 0)
                {
                    return ApiResult(true, "保存成功",pageModel.id);
                }
                else
                {
                    return ApiResult(false, "保存失败");
                }
            }
            else
            {
                pageModel.updatetime = DateTime.Now;
                bool result= CustomPageBLL.SingleModel.Update(pageModel);
                if (result)
                {
                    return ApiResult(true, "保存成功", pageModel.id);
                }
                else
                {
                    return ApiResult(false, "保存失败");
                }
            }
        }
        public ActionResult DelPage(int id=0)
        {
            if (id <= 0)
            {
                return ApiResult(false, "非法参数");
            }
            CustomPage pageModel = CustomPageBLL.SingleModel.GetModel(id);
            if (pageModel == null)
            {
                return ApiResult(false, "页面不存在");
            }
            pageModel.state = -1;
            if (CustomPageBLL.SingleModel.Update(pageModel, "state"))
            {
                return ApiResult(true, "删除成功");
            }
            else
            {
                return ApiResult(false, "删除失败");
            }
        }
        public ActionResult ChangePageState(int id=0,int state=-2)
        {
            if (id == 0 || (state!=-1&&state!=0&&state!=1))
            {
                return ApiResult(false, "非法参数");
            }
            CustomPage pageModel = CustomPageBLL.SingleModel.GetModel(id);
            if (pageModel == null)
            {
                return ApiResult(false, "页面不存在");
            }
            pageModel.state = state;
            if (CustomPageBLL.SingleModel.Update(pageModel, "state"))
            {
                switch (state)
                {
                    case -1:
                        return ApiResult(true, "删除成功");
                    case 0:
                        return ApiResult(true, "保存成功");
                    case 1:
                        return ApiResult(true, "发布成功");
                    default:
                        return ApiResult(false, "操作失败");
                }
            }
            else
            {
                return ApiResult(false, "操作失败");
            }
        }
        public ActionResult GetShareQRCode(int id=0)
        {
            CustomPage pageModel = CustomPageBLL.SingleModel.GetModel(id);
            if (pageModel == null)
            {
                return ApiResult(false, "页面不存在");
            }
            if (!string.IsNullOrEmpty(pageModel.qrcode))
            {
                return ApiResult(true, pageModel.qrcode);
            }

            string scene = $"{id}";
            string postData = JsonConvert.SerializeObject(new {
                scene = scene,
                page = "pages/index/pagePreview",
                width = 210,
                auto_color = true,
                line_color = new { r = "0", g = "0", b = "0" }
            });
            string appid = WebConfigurationManager.AppSettings["xiaowei_appid"];
            string appsecret = WebConfigurationManager.AppSettings["xiaowei_appsecret"];
            string access_token=  WxHelper.GetToken(appid, appsecret, false);
            string errorMessage = "";
            string qrCode = CommondHelper.HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token, postData,ref errorMessage);
            if (string.IsNullOrEmpty(qrCode))
            {
                return ApiResult(false, $"获取二维码失败！{errorMessage}");

            }
            pageModel.qrcode = qrCode;
            CustomPageBLL.SingleModel.Update(pageModel,"qrcode");
            return ApiResult(true, qrCode);
        }
        public ActionResult SaveFormData(CustomPageFormData formDataModel)
        {
            if (formDataModel == null)
            {
                return ApiResult(false, "非法参数");
            }
            int formDataId= Convert.ToInt32(CustomPageFormDataBLL.SingleModel.Add(formDataModel));
            if (formDataId > 0)
            {
                return ApiResult(true, "提交成功", formDataId);
            }
            else
            {
                return ApiResult(false, "提交失败");
            }
        }

        public ActionResult GetUserForm(int uid = 0,int pageid=0, int pageSize = 10, int pageIndex = 1)
        {
            if (uid <= 0||pageid<=0)
            {
                return ApiResult(false, "非法参数");
            }
            string beginTime = Context.GetRequest("beginTime",string.Empty);
            string endTime = Context.GetRequest("endTime", string.Empty);

            string sql = $"pageid={pageid}";
            List<MySqlParameter> parameter = new List<MySqlParameter>();
            if (beginTime !=""&&endTime!="")
            {
                sql += $" and addtime between @begintime and @endtime";
                parameter.Add(new MySqlParameter("begintime",beginTime));
                parameter.Add(new MySqlParameter("endtime", endTime));
            }
            
            List<CustomPageFormData> list = CustomPageFormDataBLL.SingleModel.GetListByParam(sql, parameter.ToArray(), pageSize, pageIndex, "*", "id desc");
            int count = CustomPageFormDataBLL.SingleModel.GetCount(sql, parameter.ToArray());
            return ApiResult(true, count.ToString(), list);
        }
    }
}