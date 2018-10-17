using Api.MiniApp.Filters;
using Api.MiniApp.Models;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Home;
using BLL.MiniApp.Pin;
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

namespace Api.MiniApp.Controllers
{
    public class pxhfangController : Controller
    {
        private readonly static string _pxhAppId = "wxbb2fe3080d04c9b2";
        private readonly static string _appid = WebSiteConfig.GongZhongAppId;
        private readonly static string _secret = WebSiteConfig.GongZhongSecret;
        private readonly static string _returnUrl = WebSiteConfig.GongZhongReturnUrl;
        private readonly static string _redis_PhoneKey = "gongzhonghao_phone_{0}";

        private static PinGoodsOrderBLL _pinGoodsOrderBLL = new PinGoodsOrderBLL();
        
        public ActionResult CheckPhone(string openId = "", int type = 0)
        {
            ViewBag.OpenId = openId;
            ViewBag.type = type;
            return View();
        }

        [AuthPXHFangCheck]
        public ActionResult TiXian(int id = 0, string openId = "", string phone = "",string testappid="")
        {
            if (string.IsNullOrEmpty(testappid))
            {
                testappid = _pxhAppId;
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, testappid);//_pxhAppId
            if (userinfo == null)
            {
                return Content("无效用户");
            }
            if (id <= 0)
            {
                return Content("无效用户id");
            }
            userinfo.zbSiteId = id;

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(userinfo.appId);
            if (xcxrelation == null)
            {
                return Content("无效模板");
            }

            //获取登陆秘钥
            string utoken = CheckLoginClass.GetLoginSessionKey("1", userinfo.OpenId);
            ViewBag.utoken = utoken;

            //是否为代理
            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userinfo.Id);
            ViewBag.isAgent = agent != null ? 1 : 0;

            //店铺
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(xcxrelation.Id, userinfo.Id);
            ViewBag.storeId = store != null ? store.id : 0;

            return View(userinfo);
        }

        [AuthPXHFangCheck]
        public ActionResult TiXianRecord(string openId = "", string phone = "")
        {
           
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appid, openId);
            if (userInfo == null)
            {
                return Content("无效用户");
            }
            userInfo.OpenId = openId;
            userInfo.TelePhone = phone;

            return View(userInfo);
        }

        /// <summary>
        /// 获取提现记录
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost, AuthPXHFangCheck]
        public ActionResult GetTiXianList(int state = 0, int pageIndex = 1, int pageSize = 10, string phone = "")
        {
            Return_Msg returnObj = new Return_Msg();
            string msg = string.Empty;
            int count = 0;
            List<PinGoodsOrder> orderList = _pinGoodsOrderBLL.GetListByDraw(phone, state, pageIndex, pageSize, ref count, ref msg);

            returnObj.dataObj = new { list = orderList, count = count };
            returnObj.isok = msg.Length <= 0;
            returnObj.Msg = msg;

            return Json(returnObj);
        }

        [HttpPost, AuthPXHFangCheck]
        public ActionResult ApplyDrawCase(string orderIds = "", string openId = "")
        {
            Return_Msg returnData = new Return_Msg();
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appid, openId);
            if (userInfo == null)
            {
                returnData.Msg = "无效用户";
                return Json(returnData);
            }
            List<PinGoodsOrder> orderList = _pinGoodsOrderBLL.GetListByIds(orderIds);
            if (orderList != null && orderList.Count > 0)
            {
                foreach (PinGoodsOrder item in orderList)
                {
                    returnData.Msg = DrawCashApplyBLL.SingleModel.PxhUserApplyDrawCash(item, userInfo.Id, _appid);
                    if (returnData.Msg.Length > 0)
                    {
                        return Json(returnData);
                    }
                }
            }

            returnData.isok = true;
            returnData.Msg = "申请成功";
            return Json(returnData);
        }

        #region 公众号授权
        public ActionResult Autho(int type = 0)
        {
            ViewBag.type = type;
            return View();
        }

        public ActionResult GetAuthoUrl(int type = 0)
        {
            Return_Msg returnObj = new Return_Msg();
            returnObj.dataObj = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={_appid}&redirect_uri={_returnUrl + type}&response_type=code&scope=snsapi_base&state=1#wechat_redirect";
            returnObj.isok = true;
            return Json(returnObj);
        }

        public ActionResult GetOpenId(string code = "", int type = 0)
        {
            string grant_type = "authorization_code";
            Return_Msg data = new Return_Msg();
            string url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type={3}";
            url = string.Format(url, _appid, _secret, code, grant_type);
            try
            {
                string resultJson = HttpHelper.GetData(url);
                if (!string.IsNullOrEmpty(resultJson))
                {
                    WxAuthorize result = JsonConvert.DeserializeObject<WxAuthorize>(resultJson);
                    if (result != null && !string.IsNullOrEmpty(result.openid))
                    {
                        data.dataObj = result.openid;
                        C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appid, result.openid);

                        if (userInfo == null)
                        {
                            userInfo = new C_UserInfo();
                            userInfo.StoreId = 0;
                            userInfo.OpenId = result.openid;
                            userInfo.appId = _appid;
                            userInfo.Id = Convert.ToInt32(C_UserInfoBLL.SingleModel.Add(userInfo));
                        }

                        if (!string.IsNullOrEmpty(userInfo.TelePhone))
                        {
                            if (type == 0)
                            {
                                return Redirect($"/pxhfang/TiXianRecord?phone={userInfo.TelePhone}&openId={data.dataObj}&type={type}&appid={_appid}");
                            }
                            else if (type == 99)
                            {
                                return Redirect($"/pxhfang/MyAgentInfo?phone={userInfo.TelePhone}&openId={data.dataObj}&type={type}&appid={_appid}&id={userInfo.Id}");
                            }
                            else
                            {
                                return Redirect($"/pxhfang/TiXian?phone={userInfo.TelePhone}&openId={data.dataObj}&type={type}&appid={_appid}&id={userInfo.Id}");
                            }
                        }
                        return Redirect($"/pxhfang/CheckPhone?openId={data.dataObj}&type={type}");
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }

            return Content("授权失败");
            //return Json(data, JsonRequestBehavior.AllowGet);
        }

        private string GetPxhUserOpenId(string phone)
        {
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, _pxhAppId);
            if (userinfo != null)
            {
                return userinfo.OpenId;
            }

            return "";
        }
        #endregion

        #region 短信验证码

        /// <summary>
        /// 发生短信消息验证
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public ActionResult SendCode(string phone = "")
        {
            SendTypeEnum sendType = SendTypeEnum.提现号码验证;
            Return_Msg returnData = new Return_Msg();
            try
            {
                if (string.IsNullOrEmpty(phone))
                {
                    returnData.Msg = "手机号不能为空！";
                    return Json(returnData);
                }
                C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, _pxhAppId);
                if (userinfo == null)
                {
                    returnData.Msg = "没有找到用户信息！";
                    return Json(returnData);
                }

                string redisKey = string.Format(_redis_PhoneKey, phone);
                SendMsgHelper sendMsgHelper = new SendMsgHelper();
                string authCode = RedisUtil.Get<string>(redisKey);
                if (string.IsNullOrEmpty(authCode))
                    authCode = EncodeHelper.CreateRandomCode(4);
                bool result = sendMsgHelper.AliSend(phone, "{\"code\":\"" + authCode + "\",\"product\":\" " + Enum.GetName(typeof(SendTypeEnum), sendType) + "\"}", "小未科技", 401);
                if (result)
                {
                    RedisUtil.Set<string>(redisKey, authCode, TimeSpan.FromMinutes(5));
                    returnData.isok = true;
                    returnData.Msg = "验证码发送成功！";
                }
                else
                {
                    returnData.Msg = "验证码发送失败,请稍后再试！";
                }
                returnData.dataObj = authCode;
                return Json(returnData);
            }
            catch (Exception ex)
            {
                returnData.Msg = "系统异常！" + ex.Message;
                return Json(returnData);
            }
        }

        /// <summary>
        /// 提交认证码
        /// </summary>
        /// <returns></returns>
        public ActionResult Submitauth(string phone, string openId, string authCode)
        {
            Return_Msg returnData = new Return_Msg();
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelFromCache(openId);
            returnData.isok = false;
            if (userinfo == null)
            {
                returnData.Msg = "登录信息异常!";
                return Json(returnData);
            }
            if (string.IsNullOrEmpty(userinfo.appId))
            {
                returnData.Msg = "登录信息没绑定appId!";
                return Json(returnData);
            }

            //手机电话
            if (string.IsNullOrEmpty(authCode))
            {
                returnData.Msg = "验证码不能为空!";
                return Json(returnData);
            }

            if (string.IsNullOrEmpty(phone))
            {
                returnData.Msg = "手机号不能为空!";
                return Json(returnData);
            }

            string redisKey = string.Format(_redis_PhoneKey, phone);
            string serverAuthCode = RedisUtil.Get<string>(redisKey);
            if (serverAuthCode != authCode)
            {
                returnData.Msg = "手机号码错误或验证码错误!";
                return Json(returnData);
            }
            if (C_UserInfoBLL.SingleModel.ExistsTelePhone(phone, userinfo.appId))
            {
                returnData.Msg = "该手机号码已已被绑定 , 请更换手机号码！";
                return Json(returnData);
            }
            if (serverAuthCode == authCode)
            {
                C_UserInfo pxhUserInfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, _pxhAppId);
                if (pxhUserInfo == null)
                {
                    returnData.Msg = "您不是拼享惠的用户";
                    return Json(returnData);
                }

                userinfo.NickName = pxhUserInfo.NickName;
                userinfo.TelePhone = phone;
                userinfo.HeadImgUrl = pxhUserInfo.HeadImgUrl;
                userinfo.IsValidTelePhone = 1;
                if (C_UserInfoBLL.SingleModel.Update(userinfo, "NickName,TelePhone,IsValidTelePhone,HeadImgUrl"))
                {
                    RedisUtil.Remove(redisKey);
                    returnData.isok = true;
                    returnData.dataObj = userinfo.Id;
                    returnData.Msg = "验证成功！";
                }
                else
                {
                    returnData.Msg = "验证失败！";
                }
            }
            else
            {
                returnData.Msg = "验证码错误！";
            }

            return Json(returnData);
        }

        #endregion 短信验证码

        [AuthPXHFangCheck]
        public ActionResult MyAgentInfo(int id = 0, string openId = "", string phone = "", string testpxhappid = "",string utoken = "")
        {

            if (string.IsNullOrEmpty(testpxhappid))
            {
                testpxhappid = _pxhAppId;
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, testpxhappid);//_pxhAppId
            if (userinfo == null)
            {
                return Content("无效用户");
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(userinfo.appId);
            if (xcxrelation == null)
            {
                return Content("无效模板");
            }

            if (id <= 0)
            {
                return Content("无效用户id");
            }

            //获取登陆秘钥
             utoken = CheckLoginClass.GetLoginSessionKey("1", userinfo.OpenId);
           

            if (string.IsNullOrEmpty(utoken))
            {
                return Content("utoken无效,请重新登录");
            }
            
            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userinfo.Id);
            userinfo.zbSiteId = id;
            agent.userInfo = userinfo;
           
            agent.First = new AgentDistributionDetail()
            {
                AgentCount = PinAgentBLL.SingleModel.GetAgentCount(agent.aId, agent.userId, 0),
                AgentSumMoney = PinAgentIncomeLogBLL.SingleModel.GetIncomeSum(agent.id, 0, 0),
                OrderSum = PinAgentIncomeLogBLL.SingleModel.GetIncomeSum(agent.id, 1, 0),
                StoreCount = PinStoreBLL.SingleModel.GetStoreCount(agent.id, 0)
            };

            agent.Second = new AgentDistributionDetail()
            {
                AgentCount = PinAgentBLL.SingleModel.GetAgentCount(agent.aId, agent.userId, 1),
                AgentSumMoney = PinAgentIncomeLogBLL.SingleModel.GetIncomeSum(agent.id, 0, 1),
                OrderSum = PinAgentIncomeLogBLL.SingleModel.GetIncomeSum(agent.id, 1, 1),
                StoreCount = PinStoreBLL.SingleModel.GetStoreCount(agent.id, 1)
            };
            PinAgentLevelConfig pinAgentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(agent.AgentLevel, agent.aId);
            if (pinAgentLevelConfig != null)
            {
                agent.AgentLevelName = pinAgentLevelConfig.LevelName;//等级名称
            }


            ViewBag.utoken = utoken;
            ViewBag.openId = openId;
            return View(agent);
        }
        [AuthPXHFangCheck]
        public ActionResult MyIncomeList( string phone = "", string testpxhappid = "",int type=0,int extractType=0, string utoken="")
        {

            if (string.IsNullOrEmpty(testpxhappid))
            {
                testpxhappid = _pxhAppId;
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, testpxhappid);
            if (userinfo == null)
            {
                return Content("无效用户");
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(userinfo.appId);
            if (xcxrelation == null)
            {
                return Content("无效模板");
            }

            if (string.IsNullOrEmpty(utoken))
            {
                return Content("utoken无效,请重新登录");
            }
            //获取登陆秘钥

            ViewBag.utoken = utoken;


            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userinfo.Id);
            agent.userInfo = userinfo;
            return View(agent);
        }

        [AuthPXHFangCheck]
        public ActionResult AgentTixianRecord(string openId = "", string phone = "", string testpxhappid = "", string utoken="")
        {

            if (string.IsNullOrEmpty(testpxhappid))
            {
                testpxhappid = _pxhAppId;
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, testpxhappid);//_pxhAppId
            if (userinfo == null)
            {
                return Content("无效用户");
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(userinfo.appId);
            if (xcxrelation == null)
            {
                return Content("无效模板");
            }
            if (string.IsNullOrEmpty(utoken))
            {
                return Content("utoken无效,请重新登录");
            }

            ViewBag.utoken = utoken;

            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userinfo.Id);
            agent.userInfo = userinfo;

            PinStore pinStore = PinStoreBLL.SingleModel.GetModelByAid_UserId(agent.aId, agent.userId);
            ViewBag.storeId = pinStore.id;

         
            return View(agent);
        }
        
        /// <summary>
        /// 测试专用 小程序到代理收益H5页面
        /// </summary>
        /// <param name="phone">拼享惠用户号码=授权给公众号的号码</param>
        /// <param name="testappid">拼享惠模板appid</param>
        /// <returns></returns>
        public ActionResult GetMyAgentInfoUrlByPhone(string phone = "",string testappid= "wx23f8cb7e7700f762",string utoken= "9d86969c-7384-499d-9a4c-ba2aafb9100e")
        {
            ReturnMsg data = new ReturnMsg();
            if (string.IsNullOrEmpty(phone))
            {
                data.msg = "手机号码为空!";
                return Json(data,JsonRequestBehavior.AllowGet);
            }

            C_UserInfo gzhaoUserInfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, _appid);
            if (gzhaoUserInfo == null)
            {
                data.msg = "该用户没有授权给公众号!";
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            data.msg = "获取公众号OpenId成功";
            data.code = 1;
            data.obj = $"http://testwtapi.vzan.com/pxhfang/MyAgentInfo?phone={phone}&openId={gzhaoUserInfo.OpenId}&appid={_appid}&testpxhappid={testappid}&id={gzhaoUserInfo.Id}&utoken={utoken}";
            return Json(data, JsonRequestBehavior.AllowGet);


        }
    }
}