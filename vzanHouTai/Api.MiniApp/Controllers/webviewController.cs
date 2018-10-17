using DAL.Base;
using Entity.MiniApp.Ent;
using BLL.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Senparc.Weixin;
using Senparc.Weixin.MP.Helpers;
using System.Configuration;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.Entities;
using System.IO;
using Utility.AliOss;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using Entity.MiniApp.Plat;
using BLL.MiniApp.Plat;
using Senparc.Weixin.MP.CommonAPIs;
using Utility.IO;
using Entity.MiniApp.Conf;
using BLL.MiniApp.Conf;
using Entity.MiniApp;
using Senparc.Weixin.MP.Containers;
using Entity.MiniApp.User;
using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp.Pin;
using BLL.MiniApp.Pin;
using Utility;

namespace Api.MiniApp.Controllers
{
    public class webviewController : Controller
    {
        public static string webview_appid = ConfigurationManager.AppSettings["webview_appid"];
        public static string webview_appsecret = ConfigurationManager.AppSettings["webview_appsecret"];
        
        /// <summary>
        /// 当前页面的url，由于代理转发后会 https可能会变成htpp 这里做一下纠正
        /// </summary>
        public string GetPageUrl()
        {
            string host = Request.Url.Host;
            string pageUrl = Request.Url.ToString().Split('#')[0];
            if (host.ToLower() == "wtapi.vzan.com")
            {
                pageUrl = pageUrl.Replace("http://", "https://");
            }
            return pageUrl;
        }
        //修改密码发送验证码缓存
        private readonly string _resetPasswordkey = "dz_phone_{0}_bindvalidatecode_resetpwd";
        public ActionResult RichText(int id = 0)
        {
            //string token = AccessTokenContainer.GetAccessToken(AccessTokenContainer.GetFirstOrDefaultAppId());
            string token = WxHelper.GetToken(webview_appid, webview_appsecret, false);
            ViewBag.appid = AccessTokenContainer.GetFirstOrDefaultAppId();
            ViewBag.ticket = JsApiTicketContainer.GetJsApiTicket(JsApiTicketContainer.GetFirstOrDefaultAppId());
            ViewBag.timestamp = JSSDKHelper.GetTimestamp();
            ViewBag.nonceStr = JSSDKHelper.GetNoncestr();

            ViewBag.signature = JSSDKHelper.GetSignature(ViewBag.ticket, ViewBag.nonceStr, ViewBag.timestamp, GetPageUrl());
            ViewBag.pageUrl = GetPageUrl();
            if (id == 0)
            {
                ViewBag.description = "";
            }
            else
            {
                EntGoods good = EntGoodsBLL.SingleModel.GetModel(id);
                if (good == null )
                {
                    ViewBag.description = "";
                    return Content("产品不存在或已删除");
                }
                ViewBag.description = good.description;
            }
            return View();
        }


        /// <summary>
        /// 拼享惠产品描述编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult PinRichText(int id = 0)
        {
            string token = WxHelper.GetToken(webview_appid, webview_appsecret, false);
            ViewBag.appid = AccessTokenContainer.GetFirstOrDefaultAppId();
            ViewBag.ticket = JsApiTicketContainer.GetJsApiTicket(JsApiTicketContainer.GetFirstOrDefaultAppId());
            ViewBag.timestamp = JSSDKHelper.GetTimestamp();
            ViewBag.nonceStr = JSSDKHelper.GetNoncestr();

            ViewBag.signature = JSSDKHelper.GetSignature(ViewBag.ticket, ViewBag.nonceStr, ViewBag.timestamp, Server.UrlDecode(GetPageUrl()));
            ViewBag.pageUrl = GetPageUrl();
            
            if (id == 0)
            {
                //string temp = RedisUtil.Get<string>(PinGoodsBLL.key_new_pin_goods);
                ViewBag.description = ""; //temp ?? "";
            }
            else
            {
                PinGoods good = PinGoodsBLL.SingleModel.GetModel(id);
                if (good == null)
                {
                    ViewBag.description = "";
                    return Content("产品不存在或已删除");
                }
                ViewBag.description = good.description;
            }
            return View("RichText");
        }
        /// <summary>
        /// 保存产品详情富文本
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SaveRichText(int id, string content)
        {
            if (id > 0)
            {
                EntGoods good = EntGoodsBLL.SingleModel.GetModel(id);
                if (good == null)
                {
                    return Json(new { isok = false, msg = "产品不存在或已删除" });
                }
                if (content.Trim() == "")
                {
                    return Json(new { isok = false, msg = "产品详情不能为空" });
                }

                good.description = content;
                if (EntGoodsBLL.SingleModel.Update(good, "description"))
                {
                    return Json(new { isok = true, msg = "保存成功！" });
                }
                else
                {
                    return Json(new { isok = false, msg = "保存失败！" });
                }
            }
            else
            {
                //如果是新增的产品 没有id用一个缓存来保存
                if (RedisUtil.Set(EntGoodsBLL.key_new_ent_goods, content))
                {
                    return Json(new { isok = true, msg = "保存成功！" });
                }
                else
                {
                    return Json(new { isok = false, msg = "保存失败！" });
                }
            }
        }

        /// <summary>
        /// 保存拼享惠产品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SavePinRichText(int id, string content)
        {
            
            if (id > 0)
            {
                PinGoods good = PinGoodsBLL.SingleModel.GetModel(id);
                if (good == null)
                {
                    return Json(new { isok = false, msg = "产品不存在或已删除" });
                }
                if (content.Trim() == "")
                {
                    return Json(new { isok = false, msg = "产品详情不能为空" });
                }

                good.description = content;
                if (PinGoodsBLL.SingleModel.Update(good, "description"))
                {
                    return Json(new { isok = true, msg = "保存成功！" });
                }
                else
                {
                    return Json(new { isok = false, msg = "保存失败！" });
                }
            }
            else
            {
                //如果是新增的产品 没有id用一个缓存来保存
                if (RedisUtil.Set(PinGoodsBLL.key_new_pin_goods, content))
                {
                    return Json(new { isok = true, msg = "保存成功！" });
                }
                else
                {
                    return Json(new { isok = false, msg = "保存失败！" });
                }
            }
        }

        public ActionResult SaveImgByServerId(string media_id = "")
        {
            int times = 1;
            reGetToken:
            string token = WxHelper.GetToken(webview_appid, webview_appsecret, false);
            string url = string.Empty;
            var posturl = string.Format("http://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, media_id);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(posturl);
            byte[] result = null;
            using (WebResponse response = req.GetResponse())
            {
                //在这里对接收到的页面内容进行处理 
                Stream stream = response.GetResponseStream();
                List<byte> bytes = new List<byte>();
                int temp = stream.ReadByte();
                while (temp != -1)
                {
                    bytes.Add((byte)temp);
                    temp = stream.ReadByte();
                }

                if (response.ContentType == "image/jpeg")
                {

                    result = bytes.ToArray();
                }
                else
                {
                    string errorResult = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
                    if (errorResult.IndexOf("40001") != -1 && times < 3)
                    {
                        WxHelper.GetToken(webview_appid, webview_appsecret, true);
                        times += 1;
                        goto reGetToken;
                    }
                }


            }
            string fileExtension = ".jpg";
            if (result != null && result.Length > 0)
            {
                string ossurl = AliOSSHelper.GetOssImgKey(fileExtension.Replace(".", ""), false, out url);
                bool putResult = AliOSSHelper.PutObjectFromByteArray(ossurl, result, 1, fileExtension);
                if (putResult)
                {
                    return Json(new { isok = true, msg = url });
                }
                else
                {
                    return Json(new { isok = false, msg = "保存失败" });
                }
            }

            return Json(new { isok = false, msg = "上传失败" });
        }


        public ActionResult PlatStoreDescriptionRichText(int id = 0)
        {
            //string token = AccessTokenContainer.GetAccessToken(AccessTokenContainer.GetFirstOrDefaultAppId());
            string token = WxHelper.GetToken(webview_appid, webview_appsecret, false);
            ViewBag.appid = AccessTokenContainer.GetFirstOrDefaultAppId();
            ViewBag.ticket = JsApiTicketContainer.GetJsApiTicket(JsApiTicketContainer.GetFirstOrDefaultAppId());
            ViewBag.timestamp = JSSDKHelper.GetTimestamp();
            ViewBag.nonceStr = JSSDKHelper.GetNoncestr();
            ViewBag.signature = JSSDKHelper.GetSignature(ViewBag.ticket, ViewBag.nonceStr, ViewBag.timestamp, GetPageUrl());
            log4net.LogHelper.WriteInfo(this.GetType(), $"token={token};appid={ViewBag.appid};ticket={ViewBag.ticket};timestamp={ ViewBag.timestamp};nonceStr={ViewBag.nonceStr};signature={ViewBag.signature};pageUrl={GetPageUrl()}");

            if (id == 0)
            {
                // string temp = RedisUtil.Get<string>("temp_psd_description_0");
                ViewBag.description = "";//temp ?? "";
            }
            else
            {
                PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(id);
                if (platStore == null || platStore.State == -1)
                {
                    ViewBag.description = "";
                    return Content("店铺不存在或已删除");
                }
                ViewBag.description = platStore.StoreDescription;
            }
            return View();
        }

        /// <summary>
        /// 保存产品详情富文本
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SavePlatStoreDescriptionRichText(int id, string content)
        {
            
            if (id > 0)
            {
                PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(id);
                if (platStore == null || platStore.State == -1)
                {
                    return Json(new { isok = false, msg = "店铺不存在或已删除" });
                }
                if (content.Trim() == "")
                {
                    return Json(new { isok = false, msg = "店铺描述不能为空" });
                }

                platStore.StoreDescription = content;
                if (PlatStoreBLL.SingleModel.Update(platStore, "StoreDescription"))
                {
                    return Json(new { isok = true, msg = "保存成功！" });
                }
                else
                {
                    return Json(new { isok = false, msg = "保存失败！" });
                }
            }
            else
            {
                //如果是新增的产品 没有id用一个缓存来保存
                if (RedisUtil.Set<string>("temp_psd_description_0", content))
                {
                    return Json(new { isok = true, msg = "保存成功！" });
                }
                else
                {
                    return Json(new { isok = false, msg = "保存失败！" });
                }
            }
        }

        #region 手机号注册
        public ActionResult mobileReg()
        {
            
            //代理分销，判断是否已开通过代理，开通过代理就不给他开通
            int agentqrcodeid = Context.GetRequestInt("agentqrcodeid", 0);
            if (agentqrcodeid > 0)
            {
                AgentQrCode agentqrmodel = AgentQrCodeBLL.SingleModel.GetModelById(agentqrcodeid);
                if (agentqrmodel == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "二维码已失效!", code = "403" });
                }
            }
            ViewBag.AgentQrCodeId = agentqrcodeid;

            return View();
        }

        /// <summary>
        /// 发送验证码修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetVaildCode()
        {
            Return_Msg data = new Return_Msg();
            string phoneNum = Context.GetRequest("phonenum", "");
            //修改密码 type=0  注册type=1
            int type = Context.GetRequestInt("type", 0);
            Account account = AccountBLL.SingleModel.GetModelByPhone(phoneNum);

            //代理分销，判断是否已开通过代理，开通过代理就不给他开通
            int agentqrcodeid = Context.GetRequestInt("agentqrcodeid", 0);
            if (agentqrcodeid > 0 && account != null)
            {
                Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModelByAccoundId(account.Id.ToString(), -1);
                if (agentmodel != null)
                {
                    data.Msg = "该手机号已经绑定了代理商账号";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            data = CommondHelper.GetVaildCode(agentqrcodeid, phoneNum, account, type);

            return Json(data);
        }
        /// <summary>
        /// 保存注册信息
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveUserInfo()
        {
            Return_Msg data = new Return_Msg();
            string password = Utility.IO.Context.GetRequest("password", string.Empty);
            string phone = Utility.IO.Context.GetRequest("phone", string.Empty);
            string code = Utility.IO.Context.GetRequest("code", string.Empty);
            string address = Utility.IO.Context.GetRequest("address", string.Empty);
            string sourcefrom = Utility.IO.Context.GetRequest("sourcefrom", "代理分销推广");
            int agentqrcodeid = Utility.IO.Context.GetRequestInt("agentqrcodeid", 0);
            int opentype = Utility.IO.Context.GetRequestInt("opentype", 0);
            string username = Utility.IO.Context.GetRequest("username", "");
            string appid = Utility.IO.Context.GetRequest("appid", "");
            data.isok = false;
            //if (agentqrcodeid>0 && string.IsNullOrEmpty(username))
            //{
            //    _msg.Msg = "请输入姓名";
            //    return Json(_msg, JsonRequestBehavior.AllowGet);
            //}
            if (string.IsNullOrEmpty(phone))
            {
                data.Msg = "请输入手机号";
                return Json(data);
            }
            if (string.IsNullOrEmpty(code))
            {
                data.Msg = "请输入验证码";
                return Json(data);
            }
            if (string.IsNullOrEmpty(password))
            {
                data.Msg = "请输入密码";
                return Json(data);
            }
            if (agentqrcodeid <= 0)
            {
                data.Msg = "参数错误";
                return Json(data);
            }

            //是否校验手机号,0检验，1不校验
            //校验验证码
            data = CommondHelper.CheckVaildCode(phone, code);
            if (!data.isok)
            {
                return Json(data);
            }
            Account account = AccountBLL.SingleModel.GetModelByPhone(phone);
            //如果是代理分销扫描注册，则判断绑定的手机号是否已经注册过账号，如果没有则注册一个账号
            if (account == null)
            {
                account = AccountBLL.SingleModel.WeiXinRegister("", 0, "", true, address, phone, sourcefrom, password);
            }
            else
            {
                //修改已经注册过的用户信息
                AccountBLL.SingleModel.UpdateUserInfo(account.Id.ToString(), phone, password, address);
            }

            if (account != null)
            {
                //用户已绑定手机号，判断是否有单页版
                XcxAppAccountRelation usertemplate = XcxAppAccountRelationBLL.SingleModel.GetModelByaccound(account.Id.ToString());
                if (usertemplate == null)
                {
                    //免费开通单页版
                    XcxAppAccountRelationBLL.SingleModel.AddFreeTemplate(account);
                }

                //如果是扫分销代理码注册，则开通代理，
                AgentDistributionRelationBLL agentDistributionRelationBLL = new AgentDistributionRelationBLL();
                data.Msg = agentDistributionRelationBLL.CreateDistributionAgent(account.Id.ToString(), agentqrcodeid, opentype, username);
                if (data.Msg != "")
                {
                    return Json(data);
                }

                data.isok = true;
                data.Msg = "注册成功";
            }
            else
            {
                data.isok = false;
                data.Msg = "注册失败";
            }
            string key = string.Format(_resetPasswordkey, phone);
            RedisUtil.Remove(key);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}