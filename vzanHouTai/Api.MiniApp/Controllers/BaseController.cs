//using BLL.MiniSNS.chat;
using Api.MiniApp.Filters;
using Api.MiniApp.Models;
using BLL.MiniApp;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Utility.AliOss;
using BLL.MiniApp.Pin;
using Entity.MiniApp.Alading;
using BLL.MiniApp.Alading;
using Entity.MiniApp.Conf;
using BLL.MiniApp.Conf;
using Entity.MiniApp.Weixin;

namespace Api.MiniApp.Controllers
{

    public class BaseController : ApiController
    {
        /// <summary>
        /// 逻辑锁，处理下单并发
        /// </summary>
        protected static readonly ConcurrentDictionary<string, object> _lockObjectDictOrder = new ConcurrentDictionary<string, object>();
        protected Return_Msg_APP returnObj;
        
        
        public static readonly PinGoodsOrderBLL orderBLL = new PinGoodsOrderBLL();


        /// <summary>
        /// 返回值
        /// </summary>
        public readonly ReturnMsg result;

        public BaseController()
        {
            result = new ReturnMsg();
        }


        #region 获取阿拉丁统计

        /// <summary>
        /// 第一步
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ReturnMsg getCode(string code="")
        {
            if (string.IsNullOrEmpty(code))
            {
                result.msg = "";
            }
            else
            {
                result.code = 1;
                result.obj = code;
            }
            return result;
        }

        /// <summary>
        /// 查询小程序的基本信息：名称，logo
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ReturnMsg getAppInfo(string appid="")
        {
            if (string.IsNullOrEmpty(appid))
            {
                result.msg = "非法请求";
            }
            
            OpenAuthorizerConfig authModel = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(appid);
            if (authModel != null)
            {
                result.code = 1;
                result.obj = authModel;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 通过appid获取aid
        /// </summary>
        /// <param name="appid">appid</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ReturnMsg getAid(string appid = "")
        {
            if (string.IsNullOrEmpty(appid))
            {
                result.msg = "appid不能为空";
                return result;
            }
            XcxAppAccountRelation model = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
            if (model == null)
            {
                result.msg = $"未找到和appid:{appid}相关联的小程序";
                return result;
            }

            //AlaDingAppInfo adlModel= aldAppBLL.GetModel($"appid='{appid}'");
            //if (adlModel == null)
            //{
            //    aldAppBLL
            //}

            result.code = 1;
            result.obj = model.Id;
            return result;
        }

        /// <summary>
        /// 新登陆接口
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public Return_Msg_APP WxLogin(string code = "", string appid = "", int needappsr = 0, int storeId = 0)
        {
            returnObj = new Return_Msg_APP();
            if (string.IsNullOrEmpty(code))
            {
                returnObj.Msg = "登陆凭证不能为空";
                return returnObj;
            }
            if (string.IsNullOrEmpty(appid))
            {
                returnObj.Msg = "appid不能为空";
                return returnObj;
            }
            BaseResult baseResult = CheckLoginClass.WxLogin(code, appid, needappsr, storeId);
            returnObj.Msg = baseResult.msg;
            returnObj.dataObj = baseResult.obj;
            returnObj.code = baseResult.errcode.ToString();
            returnObj.isok = baseResult.result;
            return returnObj;
        }

        /// <summary>
        /// 通过utoken查询用户信息
        /// </summary>
        /// <param name="utoken"></param>
        /// <returns></returns>
        public C_UserInfo GetUserInfo(string utoken)
        {
            C_UserInfo userInfo = null;
            if (string.IsNullOrEmpty(utoken))
            {
                return userInfo;
            }
            string rediskey = $"LoginSessionOpenIdKey_{utoken}";
            UserSession userSession = RedisUtil.Get<UserSession>(rediskey);
            if (userSession == null)
            {
                return userInfo;
            }
            userInfo = C_UserInfoBLL.SingleModel.GetModelFromCache(userSession.openid);
            return userInfo;
        }

        /// <summary>
        /// 用户登录/注册不用秘钥
        /// </summary>
        /// <param name="code">微信授权Code</param>
        /// <param name="iv">初始向量</param>
        /// <param name="data">加密数据</param>
        /// <param name="signature">加密签名</param>
        /// <param name="appid"></param>
        /// <param name="isphonedata">0:不需要获取手机号，1：需要获取手机号</param>
        /// <param name="storeId">门店ID默认=0</param>
        /// <returns>微信用户数据(Json)</returns>
        [AllowAnonymous]
        [HttpGet]
        public BaseResult CheckUserLoginNoappsr(string code, string iv, string data, string signature, string appid, int isphonedata = 0, int storeId = 0)
        {
            return CheckLoginClass.CheckUserLoginNoappsr(storeId, code, iv, data, appid, signature, isphonedata, 0);
        }

        #region 短信验证码

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">
        /// {
        ///     tel:"",
        ///     appid:"",
        ///     type:10
        /// }
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg SendSMS(string utoken, dynamic postData)
        {
            try
            {
                string tel = postData.tel;
                string appid = postData.appid;
                int type = postData.type;
                if (string.IsNullOrEmpty(tel) || !Regex.IsMatch(tel, @"^1[3-9]+\d{9}$"))
                {
                    result.msg = "手机号码格式不正确！";
                    return result;
                }
                if (C_UserInfoBLL.SingleModel.ExistsTelePhone(tel, appid))
                {
                    result.msg = "该手机号已绑定！";
                    return result;
                }

                SendMsgHelper sendMsgHelper = new SendMsgHelper();
                string authCode = RedisUtil.Get<string>(tel);
                if (string.IsNullOrEmpty(authCode))
                    authCode = Utility.EncodeHelper.CreateRandomCode(4);
                bool sendResult = sendMsgHelper.AliSend(tel, "{\"code\":\"" + authCode + "\",\"product\":\" " + Enum.GetName(typeof(SendTypeEnum), type) + "\"}", "小未科技", 401);
                if (sendResult)
                {
                    RedisUtil.Set<string>(tel, authCode, TimeSpan.FromMinutes(5));
                    result.code = 1;
                    result.msg = "验证码发送成功！";
                }
                else
                {
                    result.msg = "验证码发送失败,请稍后再试！";
                }

                result.obj = authCode;
                return result;
            }
            catch (Exception ex)
            {
                result.msg = "系统异常！" + ex.Message;
                return result;
            }
        }

        /// <summary>
        /// 提交验证短信验证码
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">
        /// {
        ///     tel:"",
        ///     code:"",
        ///     appid:""
        /// }
        /// </param>
        /// <returns></returns>
        public ReturnMsg ValidateSMS(string utoken, dynamic postData)
        {
            string code = postData.code,
                   tel = postData.tel,
                   appid = postData.appid;

            if (string.IsNullOrEmpty(code))
            {
                result.msg = "验证码不能为空!";
                return result;
            }

            if (string.IsNullOrEmpty(tel) || !Regex.IsMatch(tel, @"^1[3-9]+\d{9}$"))
            {
                result.msg = "手机格式不正确!";
                return result;
            }

            string serverAuthCode = RedisUtil.Get<string>(tel);
            if (serverAuthCode != code)
            {
                result.msg = "验证码错误!";
                return result;
            }
            
            C_UserInfo user = GetUserInfo(utoken);
            if (user == null)
            {
                result.msg = "非法请求";
                return result;
            }
            if (C_UserInfoBLL.SingleModel.ExistsTelePhone(tel, appid))
            {
                result.msg = "该手机号码已绑定，不可重复使用！";
                return result;
            }
            user.TelePhone = tel;
            user.IsValidTelePhone = 1;
            if (C_UserInfoBLL.SingleModel.Update(user, "TelePhone,IsValidTelePhone"))
            {
                RedisUtil.Remove(tel);
                result.code = 1;
                result.msg = "验证成功";
            }
            else
            {
                result.msg = "验证失败！";
            }

            return result;
        }

        #endregion 短信验证码

        /// <summary>
        /// 上传文件，支持图片，视频
        /// 图片：.jpg, .jpeg, .png, .gif, .bmp
        /// 视频：.mp4", .rmvb, .flv
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="filetype">上传类型 img,video</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg Upload(string utoken,string filetype= "img")
        {
            // 检查是否是 multipart/form-data 
            if (!Request.Content.IsMimeMultipartContent())
            {
                result.msg = "不支持的上传类型";
                return result;
            }

            HttpFileCollection files = HttpContext.Current.Request.Files;
            if (files.Count <= 0)
            {
                result.msg = "请选择要上传的文件";
                return result;
            }
            using (Stream stream = files[0].InputStream)
            {
                string fileExtension = Path.GetExtension(files[0].FileName).ToLower();
                HashSet<string> img = new HashSet<string>() { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                HashSet<string> video = new HashSet<string>() { ".mp4", ".rmvb", ".flv" };
                if (filetype == "img" && !img.Contains(fileExtension))
                {
                    result.msg = $"上传失败！只支持：{string.Join(",", img)}格式的图片！";
                    return result;
                }
                else if (filetype == "video" && !video.Contains(fileExtension))
                {
                    result.msg = $"上传失败！只支持：{string.Join(",", img)}格式的视频！";
                    return result;
                }
                byte[] imgByteArray = new byte[stream.Length];
                stream.Read(imgByteArray, 0, imgByteArray.Length);
                // 设置当前流的位置为流的开始
                stream.Seek(0, SeekOrigin.Begin);
                //开始上传
                string url = string.Empty;
                string ossurl = AliOSSHelper.GetOssImgKey(fileExtension.Replace(".", ""), false, out url);
                bool putResult = AliOSSHelper.PutObjectFromByteArray(ossurl, imgByteArray, 1, fileExtension);
                if (putResult)
                {
                    result.code = 1;
                    result.msg = "上传成功";
                    result.obj = url;
                }
                else
                {
                    result.msg = "上传失败";
                    result.obj ="";

                }
                return result;
            }
        }
        /// <summary>
        /// 提交一个form_id 便于之后发送模板消息
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{appid:appid,formid:formid}</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg commitFormId(string utoken, dynamic postData)
        {
            string appid = postData.appid;
            //string openid, string formid
            if (string.IsNullOrEmpty(appid))
            {
                result.msg = "appid不能为空";
                return result; 
            }
            
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
            if (umodel == null)
            {
                result.msg = "小程序不存在";
                return result;
            }
            C_UserInfo user = GetUserInfo(utoken);
            if (user == null)
            {
                result.msg = "非法请求";
                return result;
            }
            string formid = postData.formid;
            if (string.IsNullOrEmpty(formid))
            {
                result.msg = "formid为空";
                return result;
            }

            if (formid.Equals("the formId is a mock one"))
            {
                result.msg = "formId错误";
                return result;
            }
            
            //增加发送模板消息参数
            TemplateMsg_UserParam userParam = new TemplateMsg_UserParam();
            userParam.AppId = umodel.AppId;
            userParam.Form_IdType = 0;//form_id
            userParam.Open_Id = user.OpenId;
            userParam.AddDate = DateTime.Now;
            userParam.Form_Id = formid;
            userParam.State = 1;
            userParam.SendCount = 0;
            userParam.AddDate = DateTime.Now;
            userParam.LoseDateTime = DateTime.Now.AddDays(7);//form_id 有效期7天

            userParam.Id = Convert.ToInt32(TemplateMsg_UserParamBLL.SingleModel.Add(userParam));
            result.code = 1;
            result.obj = new { FormId = formid };
            return result;
        }
    }
    public class Return_Msg_APP
    {
        /// <summary>
        /// 状态
        /// </summary>
        public bool isok { get; set; } = false;

        /// <summary>
        /// 返回消息
        /// </summary>
        public object Msg { get; set; } = string.Empty;

        /// <summary>
        /// 返回错误编码
        /// </summary>
        public string code { get; set; } = "200";

        /// <summary>
        /// 数据
        /// </summary>
        public object dataObj { get; set; }
    }
}