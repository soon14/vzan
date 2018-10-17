using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.PinAdmin.Filters;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using BLL.MiniApp.Pin;
using BLL.MiniApp;
using Core.MiniApp;
using System.Text.RegularExpressions;
using DAL.Base;

namespace User.MiniApp.Areas.PinAdmin.Controllers
{
    [LoginFilter]
    public class BaseController : Controller
    {
        public static readonly ReturnMsg result = new ReturnMsg();
        
        
        
        
        
        
        public static readonly List<FunModel> funList = new List<FunModel> {
                new FunModel(1,"拼团商品",""),
                new FunModel(2,"推广活动",""),
                new FunModel(3,"入驻申请","")
            };
        
        
        public static readonly PinGoodsOrderBLL orderBLL = new PinGoodsOrderBLL();

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
        public ActionResult SendSMS(int aid=0,int storeId=0)
        {
            try
            {
                PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    result.msg = "店铺不存在！";
                    return Json(result);
                }
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(store.userId);
                if (userInfo == null)
                {
                    result.msg = "用户信息错误！";
                    return Json(result);
                }
                if (string.IsNullOrEmpty(userInfo.TelePhone) || !Regex.IsMatch(userInfo.TelePhone, @"^1[3-9]+\d{9}$"))
                {
                    result.msg = "手机号码格式不正确！";
                    return Json(result);
                }
                XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
                if (xcx == null)
                {
                    result.msg = "小程序信息错误！";
                    return Json(result);

                }

                SendMsgHelper sendMsgHelper = new SendMsgHelper();
                string authCode = RedisUtil.Get<string>(userInfo.TelePhone);
                if (string.IsNullOrEmpty(authCode))
                    authCode = Utility.EncodeHelper.CreateRandomCode(4);
                bool sendResult = sendMsgHelper.AliSend(userInfo.TelePhone, "{\"code\":\"" + authCode + "\",\"product\":\" " + Enum.GetName(typeof(SendTypeEnum), 11) + "\"}", "小未科技", 401);
                if (sendResult)
                {
                    RedisUtil.Set<string>(userInfo.TelePhone, authCode, TimeSpan.FromMinutes(5));
                    result.code = 1;
                    result.msg = "验证码发送成功！";
                }
                else
                {
                    result.msg = "验证码发送失败,请稍后再试！";
                }

                result.obj = authCode;
                return Json(result);
            }
            catch (Exception ex)
            {
                result.msg = "系统异常！" + ex.Message;
                return Json(result); ;
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
        public ReturnMsg ValidateSMS(PinStore store, string code = "")
        {
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(store.userId);
            if (userInfo == null)
            {
                result.msg = "用户信息错误！";
                return result;
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(store.aId);
            if (xcx == null)
            {
                result.msg = "小程序信息错误！";
                return result;

            }
            if (string.IsNullOrEmpty(code))
            {
                result.msg = "验证码不能为空!";
                return result;
            }

            string serverAuthCode = RedisUtil.Get<string>(userInfo.TelePhone);
            if (serverAuthCode != code)
            {
                result.msg = "验证码错误!";
                return result;
            }
            RedisUtil.Remove(userInfo.TelePhone);
            result.code = 1;
            result.msg = "验证成功";
            return result;
        }

        #endregion 短信验证码
    }
}