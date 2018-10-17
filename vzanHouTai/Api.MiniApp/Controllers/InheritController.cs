using Api.MiniApp.Filters;
using Api.MiniApp.Models;
using BLL.MiniApp;
using BLL.MiniApp.cityminiapp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Qiye;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using Entity.MiniApp.Weixin;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.AliOss;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public partial class InheritController : AsyncController
    {
        protected Return_Msg_APP returnObj;
        protected static readonly object _goodsMommentLocker = new object();
        protected static readonly object _sortQueueLockObject = new object();
        protected static readonly Random random_Food = new Random(new Guid().GetHashCode());
        protected static readonly ConcurrentDictionary<string, object> _lockObjectDictOrder = new ConcurrentDictionary<string, object>();

        protected C_UserInfo LoginData { get { return (C_UserInfo)ViewData["LoginData"]; } }

        #region 其他
        protected static CityMordersBLL _cityMordersBLL = new CityMordersBLL();
        protected static XcxAppAccountRelationBLL _xcxAppAccountRelationBLL = XcxAppAccountRelationBLL.SingleModel;
        #endregion

        #region 配送物流
        //达达配送
        protected static DadaApi _dadaApi = new DadaApi();
        protected static DadaOrderBLL _dadaOrderBLL = new DadaOrderBLL();

        #endregion

        #region 短信验证码

        /// <summary>
        /// 发生短信消息验证
        /// </summary>
        /// <param name="tel"></param>
        /// <param name="sendType"></param>
        /// <param name="isNeedCheckBind">是否检查手机已被绑定</param>
        /// <returns></returns>
        public ActionResult senduserauth(string tel, string openId, SendTypeEnum sendType, string appid)
        {
            Return_Msg msg = new Return_Msg();
            msg.isok = false;
            try
            {
                if (string.IsNullOrEmpty(openId))
                {
                    msg.Msg = "登录信息异常！openid";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                C_UserInfo loginer = C_UserInfoBLL.SingleModel.GetModelFromCache(openId);
                if (loginer == null)
                {
                    msg.Msg = "登录信息异常！";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(tel) || !Regex.IsMatch(tel, @"^[1]+[3-9]+\d{9}$"))
                {
                    msg.Msg = "手机格式不正确！";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                if (C_UserInfoBLL.SingleModel.ExistsTelePhone(tel, appid))
                {
                    msg.Msg = "该手机号已绑定！";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }

                SendMsgHelper sendMsgHelper = new SendMsgHelper();
                string authCode = RedisUtil.Get<string>(tel);
                if (string.IsNullOrEmpty(authCode))
                    authCode = Utility.EncodeHelper.CreateRandomCode(4);
                bool result = sendMsgHelper.AliSend(tel, "{\"code\":\"" + authCode + "\",\"product\":\" " + Enum.GetName(typeof(SendTypeEnum), sendType) + "\"}", "小未科技", 401);
                if (result)
                //if (new SendMsgHelper().Send($"【{cityinfo.CName}】验证码：{authCode}，5分钟内使用有效", tel))
                {
                    RedisUtil.Set<string>(tel, authCode, TimeSpan.FromMinutes(5));
                    msg.isok = true;
                    msg.Msg = "验证码发送成功！";
                }
                else
                {
                    msg.Msg = "验证码发送失败,请稍后再试！";
                }
                msg.dataObj = authCode;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msg.Msg = "系统异常！" + ex.Message;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 提交认证码
        /// </summary>
        /// <returns></returns>
        public ActionResult Submitauth(string tel, string openId, string authCode)
        {
            Return_Msg msg = new Return_Msg();
            C_UserInfo loginer = C_UserInfoBLL.SingleModel.GetModelFromCache(openId);
            msg.isok = false;
            if (loginer == null)
            {
                msg.Msg = "登录信息异常!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(loginer.appId))
            {
                msg.Msg = "登录信息没绑定小程序appId!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            //判断是否为固定电话
            if (!string.IsNullOrEmpty(tel) && tel.Length == 8)
            {
                loginer.TelePhone = tel;
                loginer.IsValidTelePhone = 1;
                if (C_UserInfoBLL.SingleModel.Update(loginer, "TelePhone,IsValidTelePhone"))
                {
                    RedisUtil.Remove(tel);
                    msg.isok = true;
                    msg.Msg = "电话保存成功！";
                }
                else
                {
                    msg.Msg = "电话保存失败！";
                }

                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            //手机电话
            if (string.IsNullOrEmpty(authCode))
            {
                msg.Msg = "认证码不能为空!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(tel) || !Regex.IsMatch(tel, @"^[1]+[3-9]+\d{9}$"))
            {
                msg.Msg = "手机格式不正确!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string serverAuthCode = RedisUtil.Get<string>(tel);
            if (serverAuthCode != authCode)
            {
                msg.Msg = "手机号码错误或验证码错误!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            if (C_UserInfoBLL.SingleModel.ExistsTelePhone(tel, loginer.appId))
            {
                msg.Msg = "该手机号码已已被绑定 , 请更换手机号码！";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            if (serverAuthCode == authCode)
            {
                loginer.TelePhone = tel;
                loginer.IsValidTelePhone = 1;
                if (C_UserInfoBLL.SingleModel.Update(loginer, "TelePhone,IsValidTelePhone"))
                {
                    RedisUtil.Remove(tel);
                    msg.isok = true;
                    msg.Msg = "验证成功！";
                }
                else
                {
                    msg.Msg = "验证失败！";
                }
            }
            else
            {
                msg.Msg = "验证失败！";
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        #endregion 短信验证码

        #region 小程序底部水印
        protected static string _bottomLogTest = ConfigurationManager.AppSettings["bottomlogtest"].ToString();
        protected static string _bottomLogHost = ConfigurationManager.AppSettings["bottomloghost"].ToString();
        protected static string _bottomLogTitle = ConfigurationManager.AppSettings["bottomlogtitle"].ToString();
        protected static string _bottomLogImg = ConfigurationManager.AppSettings["bottomlogimg"].ToString();
        protected static string _bottomLogChildTitle = ConfigurationManager.AppSettings["bottomlogchildtitle"].ToString();
        #endregion

        public InheritController()
        {

        }

        protected JsonResult ApiResult(bool Result, string Message, object obj = null, object extdata = null)
        {
            if (obj != null)
            {
                string tempmsg = JsonConvert.SerializeObject(obj);
                if (tempmsg.Contains("FAIL"))
                {
                    Message = "商家未完成支付配置";
                }
            }
            return new JsonResult()
            {
                Data = new { result = Result, msg = Message, obj = obj, extdata = extdata },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult BaseApiResult(bool isok, string msg = "", object data = null)
        {
            return Json(new { isok, msg, data });
        }

        public DateTime convertIntDatetime(int utc)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            startTime = startTime.AddSeconds(utc);
            return startTime;
        }

        /// <summary>
        /// 用户登录/注册不用秘钥
        /// </summary>
        /// <param name="code">微信授权Code</param>
        /// <param name="iv">初始向量</param>
        /// <param name="data">加密数据</param>
        /// <param name="signature">加密签名</param>
        /// <param name="appsr">小程序秘钥</param>
        /// <param name="needappsr">是否需要秘钥：0：不需要，通过第三方登陆，1：需要秘钥，不通过第三方登陆，参数appsr不为空</param>
        /// <returns>微信用户数据(Json)</returns>
        public ActionResult CheckUserLoginNoappsr(string code, string iv, string data, string signature, string appid, string appsr = "", int needappsr = 0)
        {
            //0:不需要获取手机号，1：需要获取手机号
            int isphonedata = Context.GetRequestInt("isphonedata", 0);
            //店铺ID
            int storeId = Context.GetRequestInt("storeId", 0);
            BaseResult result = CheckLoginClass.CheckUserLoginNoappsr(storeId, code, iv, data, appid, signature, isphonedata, needappsr);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新登陆接口
        /// </summary>
        /// <returns></returns>
        public ActionResult WxLogin()
        {
            returnObj = new Return_Msg_APP();
            string code = Context.GetRequest("code", string.Empty);
            string appid = Context.GetRequest("appid", string.Empty);
            int needappsr = Context.GetRequestInt("needappsr", 0);
            int storeId = Context.GetRequestInt("storeid", 0);
            if (string.IsNullOrEmpty(code))
            {
                returnObj.Msg = "登陆凭证不能为空";
                return Json(returnObj);
            }
            if (string.IsNullOrEmpty(appid))
            {
                returnObj.Msg = "appid不能为空";
                return Json(returnObj);
            }
            BaseResult result = CheckLoginClass.WxLogin(code, appid, needappsr, storeId);
            returnObj.Msg = result.msg;
            returnObj.dataObj = result.obj;
            returnObj.code = result.errcode.ToString();
            returnObj.isok = result.result;
            return Json(returnObj);
        }

        #region 新版登录
        //1. 小程序请求login，拿到code 然后传给服务端；
        //2.服务端拿到code 到微信服务器拿到sessionKey ；
        //3.然后小程序调用getuserinfo接口拿到encryptedData，iv,然后给服务端；
        //4.服务端拿到客户端的encryptedData，vi还有之前的sessionKey去解密得到 unionId等用户信息；
        //不然就会出现你这样的问题，你这种情况偶然出现的原因就是 你在服务端还未去获取sessionKey的时候你就去调用了getuserinfo，有时候你会比服务端快，有时候你会比服务端慢，所以就出现了偶然性
        /// <summary>
        /// 获取sesionkey
        /// </summary>
        /// <param name="code"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult GetSesionKey(int storeId = 0, string code = "", string appId = "", int needAppsr = 0)
        {
            returnObj = new Return_Msg_APP();
            if (string.IsNullOrEmpty(code))
            {
                returnObj.Msg = "code不能为空";
                return Json(returnObj);
            }
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "appId不能为空";
                return Json(returnObj);
            }

            string msg = "";
            UserSession session = XcxApiBLL.SingleModel.GetAppSessionInfo(needAppsr, code, appId, ref msg);
            if (msg.Length > 0)
            {
                returnObj.isok = false;
                returnObj.Msg = msg;
            }
            else
            {
                //查询用户信息
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, session.openid);
                if (userInfo == null)
                {
                    userInfo = new C_UserInfo() { appId = appId, OpenId = session.openid, StoreId = storeId, };
                    userInfo = C_UserInfoBLL.SingleModel.RegisterByXiaoChenXun(userInfo);
                }

                //获取登陆秘钥
                string loginsessionkey = CheckLoginClass.GetLoginSessionKey(session.session_key, session.openid);
                userInfo.loginSessionKey = loginsessionkey;
                //userInfo.SessionKey = session.session_key;
                C_UserInfoBLL.SingleModel.RefleshUserInfoSessionKey(userInfo.Id, session.session_key);
                returnObj.isok = true;
                returnObj.dataObj = userInfo;
            }

            return Json(returnObj);
        }

        /// <summary>
        /// 获取解密后用户的数据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        /// <param name="iv"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult GetAppUserInfo(string iv = "", string data = "", int userId = 0)
        {
            returnObj = new Return_Msg_APP();
            if (string.IsNullOrEmpty(data))
            {
                returnObj.Msg = "data不能为空";
                return Json(returnObj);
            }
            if (string.IsNullOrEmpty(iv))
            {
                returnObj.Msg = "iv不能为空";
                return Json(returnObj);
            }
            if (userId <= 0)
            {
                returnObj.Msg = "userId不能为0";
                return Json(returnObj);
            }
            try
            {
                //查询用户信息
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
                if (userInfo == null)
                {
                    returnObj.Msg = "获取用户信息失败";
                    return Json(returnObj);
                }
                string sessionKey = C_UserInfoBLL.SingleModel.GetUserInfoSessionKey(userId);
                if (string.IsNullOrEmpty(sessionKey))
                {
                    returnObj.Msg = "sessionKey不能为空";
                    return Json(returnObj);
                }
                string msg = "";
                //解密后的用户数据
                C_ApiUserInfo apiUserInfo = AESDecrypt.GetUserInfo(sessionKey, iv, data, ref msg);
                if (msg.Length > 0)
                {
                    returnObj.Msg = msg;
                    return Json(returnObj);
                }

                if (apiUserInfo == null)
                {
                    returnObj.Msg = "获取用户信息失败";
                    return Json(returnObj);
                }

                //更新小程序用户信息
                userInfo = C_UserInfoBLL.SingleModel.UpdateUserInfo(apiUserInfo, userInfo);

                returnObj.dataObj = userInfo;
                returnObj.isok = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(this.GetType(), ex);
                returnObj.Msg = "系统繁忙";
            }

            return Json(returnObj);
        }
        #endregion

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateUserInfo()
        {
            returnObj = new Return_Msg_APP();
            int userid = Context.GetRequestInt("userid", 0);
            string imgurl = Context.GetRequest("imgurl", "");
            string nickname = Context.GetRequest("nickname", "");
            if (userid <= 0)
            {
                returnObj.Msg = "用户ID不能为空";
                return Json(returnObj);
            }

            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userid);
            if (userinfo == null)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }

            StringBuilder column = new StringBuilder();
            if (imgurl != userinfo.HeadImgUrl)
            {
                userinfo.HeadImgUrl = imgurl;
                column.Append("HeadImgUrl,");
            }
            if (nickname != userinfo.NickName)
            {
                userinfo.NickName = nickname;
                column.Append("NickName,");
            }
            if (!string.IsNullOrEmpty(column.ToString()))
            {
                returnObj.isok = C_UserInfoBLL.SingleModel.Update(userinfo, column.ToString().TrimEnd(','));
            }

            returnObj.Msg = returnObj.isok ? "成功" : "失败";
            return Json(returnObj);
        }

        /// <summary>
        /// 获取代理配置
        /// </summary>
        /// <param name="appid">小程序appid</param>
        public ActionResult GetAgentConfigInfo(string appid)
        {
            AgentConfig agentConfig = new AgentConfig { IsOpenAdv = 1, LogoText = _bottomLogTest, LogoHost = _bottomLogHost, LogoTitle = _bottomLogTitle };

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcxrelationModel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxrelationModel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModel(xcxrelationModel.agentId);
            if (agentinfo != null)
            {
                //判断是否是分销代理
                if (agentinfo.userLevel > 0)
                {
                    Distribution fenxiaomodel = DistributionBLL.SingleModel.GetModel("AgentId=" + agentinfo.id);
                    if (fenxiaomodel != null)
                    {
                        agentinfo = AgentinfoBLL.SingleModel.GetModel(fenxiaomodel.parentAgentId);
                        if (agentinfo == null)
                        {
                            agentinfo = new Agentinfo();
                        }
                    }
                }
            }

            //如果是子模板，则另外处理，所有免费版都挂上公司自己的logo
            if (ConfParamBLL.SingleModel.CheckFeeTemplateShuiying(xcxrelationModel, agentinfo, ref agentConfig, _bottomLogHost, _bottomLogImg, _bottomLogChildTitle, _bottomLogTitle))
            {
                return Json(new { isok = 1, msg = "成功", AgentConfig = agentConfig }, JsonRequestBehavior.AllowGet);
            }

            if (agentinfo != null)
            {
                //判断代理是否有自定义水印
                if (!string.IsNullOrEmpty(agentinfo.configjson))
                {
                    agentConfig = SerializeHelper.DesFromJson<AgentConfig>(agentinfo.configjson);
                    agentConfig.isdefaul = 1;
                    agentConfig.LogoTitle = agentConfig.LogoText;
                }

                //判断是否有加入代理推广分销
                AgentCustomerRelation customerrelation = AgentCustomerRelationBLL.SingleModel.GetModelByAccountId(agentinfo.id, xcxrelationModel.AccountId.ToString());
                if (customerrelation != null)
                {
                    agentConfig.OpenExtension = customerrelation.OpenExtension;
                    agentConfig.QrcodeId = customerrelation.QrcodeId;
                }

                //代理是否付费单独模板开启水印
                if (ConfParamBLL.SingleModel.CheckPayOpenShuiying(xcxrelationModel.Id, ref agentConfig))
                {
                    return Json(new { isok = 1, msg = "成功", AgentConfig = agentConfig }, JsonRequestBehavior.AllowGet);
                }
            }

            //判断用户有没有自定义logo，没有就用系统默认
            if (string.IsNullOrEmpty(agentConfig.LogoImgUrl))
            {
                ConfParam userconfig = ConfParamBLL.SingleModel.GetModelByParamappid("logoimg", appid);
                if (userconfig != null && !string.IsNullOrEmpty(userconfig.Value))
                {
                    agentConfig.LogoImgUrl = userconfig.Value;
                }
                else
                {
                    agentConfig.LogoImgUrl = _bottomLogImg;
                }
            }

            if (string.IsNullOrEmpty(agentConfig.LogoText))
            {
                agentConfig.LogoText = _bottomLogTest;
                agentConfig.isdefaul = 0;
            }
            //agentConfig.LogoHost = string.IsNullOrEmpty(agentConfig.LogoHost) ? _bottomLogHost : agentConfig.LogoHost;
            //agentConfig.LogoTitle = _bottomLogTitle;

            return Json(new { isok = 1, msg = "成功", AgentConfig = agentConfig }, JsonRequestBehavior.AllowGet);
        }

        #region 支付相关

        //支付回调
        public void paynotify()
        {
            ResultNotify resultNotify = new ResultNotify(HttpContext);
            resultNotify.ProcessNotify();
        }

        //支付下单（添加）
        /// <summary>
        /// 小程序下单接口
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="appid">小程序appId</param>
        /// <param name="ordertype">下单类型</param>
        /// <param name="paytype">支付类型，储值卡支付</param>
        /// <param name="payprice">下单金额</param>
        /// <param name="itemid"></param>
        /// <param name="quantity">数量</param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult AddPayOrder(int userId, string appid, int ordertype, int paytype, string jsondata)
        {
            int buyprice = Context.GetRequestInt("buyprice", 0);
            returnObj = new Return_Msg_APP();
            object orderobj = new object();
            Coupons reductionCart = new Coupons();

            C_UserInfo LoginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (ordertype <= 0)
            {
                returnObj.Msg = "用户信息登录失败";
                return Json(returnObj);
            }

            if (string.IsNullOrEmpty(appid))
            {
                returnObj.Msg = "小程序AppId参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                returnObj.Msg = "请先授权";
                return Json(returnObj);
            }

            //添加锁项
            //不同商家，不同的锁,当前商家若还未创建，则创建一个
            string lockkey = umodel.Id + "_" + ordertype;
            if (!_lockObjectDictOrder.ContainsKey(lockkey))
            {
                if (!_lockObjectDictOrder.TryAdd(lockkey, new object()))
                {
                    returnObj.Msg = "系统繁忙,请稍候再试！";
                    return Json(returnObj);
                }
            }

            TransactionModel tran = new TransactionModel();
            StringBuilder sbUpdateGoodCartSql = null;
            SaveMoneySetUser saveMoneyUser = new SaveMoneySetUser();
            if (paytype == (int)miniAppBuyMode.储值支付)
            {
                saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(appid, userId);
                if (saveMoneyUser == null)
                {
                    returnObj.Msg = "您还没开通储值支付";
                    return Json(returnObj);
                }
                if (saveMoneyUser.AccountMoney < buyprice)
                {
                    returnObj.Msg = "储值卡余额不足";
                    return Json(returnObj);
                }
            }

            int orderid = 0;
            CityMorders order = new CityMorders()
            {
                FuserId = LoginCUser.Id,
                Fusername = LoginCUser.NickName,
                TuserId = 0,
                OrderType = ordertype,
                ActionType = paytype == (int)miniAppBuyMode.储值支付 ? paytype : ordertype,
                Addtime = DateTime.Now,
                Percent = 99,//不收取服务费
                userip = WebHelper.GetIP(),
                payment_status = 0,
                Status = 0,
                CitySubId = 0,//无分销,默认为0
                PayRate = 1,
                appid = appid,
            };
            lock (_lockObjectDictOrder[lockkey])
            {
                switch (ordertype)
                {
                    case (int)ArticleTypeEnum.QiyeOrderPay:
                        //生成订单
                        returnObj.Msg = QiyeGoodsOrderBLL.SingleModel.AddOrder(LoginCUser.Id, jsondata, ref tran, ref orderobj, ordertype);
                        break;
                    case (int)ArticleTypeEnum.PlatChildOrderInPlatPay:
                    case (int)ArticleTypeEnum.PlatChildOrderPay:
                        //生成订单
                        returnObj.Msg = PlatChildGoodsOrderBLL.SingleModel.AddOrder(LoginCUser.Id, jsondata, ref tran, ref orderobj, ordertype);
                        break;
                    case (int)ArticleTypeEnum.MiniappGroups:
                        returnObj.Msg = GroupsBLL.SingleModel.CommandGroupPay(jsondata, ordertype, paytype, appid, ref order);
                        break;

                    case (int)ArticleTypeEnum.MiniappFoodGoods:
                        //生成订单
                        returnObj.Msg = FoodGoodsOrderBLL.SingleModel.CommandFoodOrder(jsondata, umodel.Id, LoginCUser.Id, paytype, ref orderobj, ref reductionCart, ref sbUpdateGoodCartSql, ref order);
                        break;

                    case (int)ArticleTypeEnum.MiniappEnt:

                        break;

                    case (int)ArticleTypeEnum.City_StoreBuyMsg://小程序同城模板发布消息
                        returnObj.Msg = CityMsgBLL.SingleModel.saveMsg(jsondata, ordertype, paytype, umodel.Id, ref order, userId);
                        if (!string.IsNullOrEmpty(returnObj.Msg.ToString()))
                        {
                            //表示发布失败返回失败结果
                            returnObj.isok = false;
                            return Json(returnObj);
                        }
                        //  LogHelper.WriteInfo(this.GetType(), "order.Articleid=" + order.Articleid);
                        if (order.Articleid == 0)//表示不是置顶信息 不需要生成CityMorders
                        {
                            // LogHelper.WriteInfo(this.GetType(), "同城发帖不需支付进来了"+JsonConvert.SerializeObject(order));
                            returnObj.isok = true;
                            return Json(returnObj);
                        }
                        break;

                    case (int)ArticleTypeEnum.PlatMsgPay://平台版小程序分类信息 发布消息
                        returnObj.Msg = PlatMsgBLL.SingleModel.saveMsg(jsondata, ordertype, paytype, umodel.Id, ref order, userId);
                        if (!string.IsNullOrEmpty(returnObj.Msg.ToString()))
                        {
                            //表示发布失败返回失败结果
                            returnObj.isok = false;
                            return Json(returnObj);
                        }
                        //  LogHelper.WriteInfo(this.GetType(), "order.Articleid=" + order.Articleid);
                        if (order.Articleid == 0)//表示不是置顶信息 不需要生成CityMorders
                        {
                            // LogHelper.WriteInfo(this.GetType(), "平台版小程序分类信息不需支付进来了"+JsonConvert.SerializeObject(order));
                            returnObj.isok = true;
                            return Json(returnObj);
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(returnObj.Msg.ToString()))
                {
                    return Json(returnObj);
                }

                string no = WxPayApi.GenerateOutTradeNo();
                order.orderno = no;
                order.trade_no = no;

                switch (paytype)
                {
                    case (int)miniAppBuyMode.微信支付:
                        switch (ordertype)
                        {
                            case (int)ArticleTypeEnum.QiyeOrderPay:
                                returnObj.dataObj = QiyeGoodsOrderBLL.SingleModel.PayOrder(orderobj, order, tran, ref orderid);
                                break;
                            case (int)ArticleTypeEnum.PlatChildOrderInPlatPay:
                            case (int)ArticleTypeEnum.PlatChildOrderPay:
                                returnObj.dataObj = PlatChildGoodsOrderBLL.SingleModel.PayOrder(orderobj, order, tran, ref orderid);
                                break;
                            case (int)ArticleTypeEnum.MiniappGroups:
                                orderid = Convert.ToInt32(_cityMordersBLL.Add(order));
                                returnObj.dataObj = new { orderid = orderid };
                                break;
                            case (int)ArticleTypeEnum.MiniappFoodGoods:
                                returnObj.dataObj = FoodGoodsOrderBLL.SingleModel.PayOrder(orderobj, order, ref orderid);
                                break;
                            case (int)ArticleTypeEnum.City_StoreBuyMsg://小程序同城模板发布消息
                            case (int)ArticleTypeEnum.PlatMsgPay://平台版小程序同城模板发布消息
                                orderid = Convert.ToInt32(_cityMordersBLL.Add(order));
                                returnObj.dataObj = new { orderid = orderid };
                                break;
                        }
                        break;
                    case (int)miniAppBuyMode.储值支付:
                        PayResult result = new PayResult();
                        result.total_fee = order.payment_free;
                        result.result_code = "SUCCESS";
                        switch (ordertype)
                        {
                            case (int)ArticleTypeEnum.QiyeOrderPay:
                                returnObj.dataObj = QiyeGoodsOrderBLL.SingleModel.PayOrderByChuzhi(orderobj, umodel.Id, saveMoneyUser, tran, ref orderid);
                                break;
                            case (int)ArticleTypeEnum.PlatChildOrderInPlatPay:
                            case (int)ArticleTypeEnum.PlatChildOrderPay:
                                returnObj.dataObj = PlatChildGoodsOrderBLL.SingleModel.PayOrderByChuzhi(orderobj, umodel.Id, saveMoneyUser, tran, ref orderid);
                                break;
                            case (int)ArticleTypeEnum.MiniappGroups:
                                CityMordersBLL citybll = new CityMordersBLL(result, order);
                                if (!citybll.MiniappStoreGroup())
                                {
                                    returnObj.Msg = "请求失败";
                                    return Json(returnObj);
                                }
                                orderid = citybll.Order.Id;
                                returnObj.dataObj = new { orderid = orderid };
                                break;
                            case (int)ArticleTypeEnum.MiniappFoodGoods:
                                returnObj.dataObj = FoodGoodsOrderBLL.SingleModel.PayOrderByChuzhi(orderobj, order, umodel.Id, saveMoneyUser, sbUpdateGoodCartSql, reductionCart, ref orderid);
                                break;
                        }
                        break;
                }
            }

            returnObj.isok = orderid > 0;
            returnObj.Msg = returnObj.isok ? "支付成功" : "请求失败，订单生成失败";
            return Json(returnObj);
        }

        //支付下单（添加）
        /// <summary>
        /// 小程序下单接口
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="appid">小程序appId</param>
        /// <param name="ordertype">下单类型</param>
        /// <param name="paytype">支付类型，储值卡支付</param>
        /// <param name="payprice">下单金额</param>
        /// <param name="itemid"></param>
        /// <param name="quantity">数量</param>
        /// <returns></returns>
        [HttpPost, AuthLoginCheckXiaoChenXun]
        public ActionResult AddPayOrderNew(int userId, string appid, int ordertype, int paytype, string jsondata = "")
        {
            int PayAmout = 0;
            int Articleid = 0;
            int itemid = 0;
            int quantity = 1;
            int CommentId = 0;
            int MinisnsId = 0;
            string remark = "";
            int OperStatus = 0;
            string Tusername = "";//收货人姓名
            string phone = "";//收货人电话
            string note = "";//留言

            //拼团
            int is_group = 0;
            int groupsponsor_id = 0;
            int is_group_head = 0;

            C_UserInfo LoginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (LoginCUser == null)
            {
                return ApiResult(false, "用户信息登录失败");
            }
            if (ordertype <= 0)
            {
                return ApiResult(false, "支付参数错误");
            }

            if (string.IsNullOrEmpty(appid))
            {
                return ApiResult(false, "小程序AppId参数错误");
            }

            string shownote = "";
            switch (ordertype)
            {
                case (int)ArticleTypeEnum.MiniappGroups:

                    if (string.IsNullOrEmpty(jsondata))
                    {
                        return ApiResult(false, "json参数错误");
                    }
                    AddGroupModel groupjson = JsonConvert.DeserializeObject<AddGroupModel>(jsondata);
                    if (groupjson == null)
                    {
                        return ApiResult(false, "null参数错误");
                    }

                    Groups group = GroupsBLL.SingleModel.GetModel(groupjson.groupId);
                    if (group == null)
                    {
                        return ApiResult(false, "拼团商品不存在");
                    }

                    is_group = groupjson.isGroup;
                    is_group_head = groupjson.isGHead;
                    CommentId = groupjson.guid;
                    quantity = groupjson.num;
                    Articleid = groupjson.groupId;
                    if (Articleid <= 0)
                    {
                        return ApiResult(false, "拼团参数错误");
                    }
                    PayAmout = groupjson.payprice;
                    groupsponsor_id = groupjson.gsid;
                    remark = groupjson.addres;
                    Store store = StoreBLL.SingleModel.GetModel(group.StoreId);
                    if (store == null)
                    {
                        return ApiResult(false, "店铺不存在");
                    }
                    OperStatus = store.Id;
                    MinisnsId = store.Id;
                    shownote = $"小程序拼团付款{PayAmout * 0.01}元";
                    Tusername = groupjson.username;
                    phone = groupjson.phone;
                    note = groupjson.note;
                    if (CommentId <= 0)
                    {
                        return ApiResult(false, "用户拼团记录ID不能小于0");
                    }

                    break;
            }
            if (paytype == 1)
            {
                SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(appid, userId);
                if (saveMoneyUser == null)
                {
                    return ApiResult(false, "您还没开通储值支付");
                }
                if (saveMoneyUser.AccountMoney < PayAmout)
                {
                    return ApiResult(false, "储值卡余额不足");
                }
            }
            if (PayAmout <= 0)
            {
                return ApiResult(false, "系统错误!PayAmout_error");
            }
            string no = WxPayApi.GenerateOutTradeNo();

            CityMorders order = new CityMorders()
            {
                OrderType = ordertype,
                ActionType = paytype > 0 ? paytype : ordertype,
                Addtime = DateTime.Now,
                payment_free = PayAmout,
                trade_no = no,
                Percent = 99,//不收取服务费
                userip = WebHelper.GetIP(),
                FuserId = LoginCUser.Id,
                Fusername = LoginCUser.NickName,
                orderno = no,
                payment_status = 0,
                Status = 0,
                Articleid = Articleid,
                CommentId = CommentId,
                MinisnsId = MinisnsId,//商家ID
                TuserId = itemid,//订单的ID
                is_group = is_group,
                is_group_head = is_group_head,
                groupsponsor_id = groupsponsor_id,
                ShowNote = shownote,
                CitySubId = 0,//无分销,默认为0
                PayRate = 1,
                buy_num = quantity, //无
                appid = appid,
                remark = remark,
                OperStatus = OperStatus,
                Tusername = Tusername,
                AttachPar = phone,
                Note = note,
            };

            int orderid = 0;
            if (paytype == 1)
            {
                PayResult result = new PayResult();
                result.total_fee = order.payment_free;
                result.result_code = "SUCCESS";

                CityMordersBLL citybll = new CityMordersBLL(result, order);
                switch (ordertype)
                {
                    case (int)ArticleTypeEnum.MiniappGroups://拼团储值支付
                        if (!citybll.MiniappStoreGroup())
                        {
                            orderid = citybll.Order.Id;
                            return ApiResult(false, "请求失败", orderid);
                        }
                        orderid = citybll.Order.Id;
                        break;
                }
            }
            else
            {
                orderid = Convert.ToInt32(_cityMordersBLL.Add(order));
            }

            if (orderid > 0)
            {
                return ApiResult(true, "请求成功", orderid);
            }
            else
            {
                return ApiResult(false, "请求失败，订单生成失败");
            }
        }

        [HttpPost, AuthLoginCheckXiaoChenXun]
        public ActionResult PayOrderNew(int orderid, string openId, int type = 0)
        {
            try
            {
                CityMorders order = _cityMordersBLL.GetModel(orderid);
                if (order == null || order.payment_status != 0)
                {
                    return ApiResult(false, "订单已经失效");
                }

                //检查订单状态
                string errorMsg = "";
                if (!_cityMordersBLL.CheckOrderState(order, ref errorMsg))
                {
                    return ApiResult(false, errorMsg);
                }

                PayCenterSetting setting = null;
                if (!string.IsNullOrEmpty(order.appid))
                {
                    setting = PayCenterSettingBLL.SingleModel.GetPayCenterSetting(order.appid);
                }
                else
                {
                    setting = PayCenterSettingBLL.SingleModel.GetPayCenterSetting((int)PayCenterSettingType.City, order.MinisnsId);
                }

                JsApiPay jsApiPay = new JsApiPay(HttpContext)
                {
                    total_fee = order.payment_free,
                    openid = openId
                };

                //统一下单，获得预支付码
                WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResult(setting, order, WebConfigBLL.citynotify_url, ref errorMsg);
                if (errorMsg.Length > 0)
                {
                    return ApiResult(false, errorMsg);
                }

                //增加发送模板消息次数
                TemplateMsg_Miniapp.AddSendTranMessage(order.appid, openId, Convert.ToString(unifiedOrderResult.GetValue("prepay_id")), order.CommentId);

                //获取立减金，没有的话就传null
                Coupons reductionCart = CouponsBLL.SingleModel.GetVailtModelByAppId(order.appid);

                return ApiResult(true, "下单成功", jsApiPay.GetJsApiParametersnew(setting), new { reductionCart = reductionCart });
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return ApiResult(false, "下单异常", ex.Message);
            }
        }

        /// <summary>
        /// 提交一个form_id 便于之后发送模板消息
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="formid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult commitFormId(string appid, string openid, string formid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(formid))
            {
                return Json(new { isok = -1, msg = "formId为空" }, JsonRequestBehavior.AllowGet);
            }
            if (formid.Equals("the formId is a mock one"))
            {
                return Json(new { isok = -1, msg = "formId错误" }, JsonRequestBehavior.AllowGet);
            }

            //增加发送模板消息参数
            TemplateMsg_UserParam userParam = new TemplateMsg_UserParam();
            userParam.AppId = umodel.AppId;
            userParam.Form_IdType = 0;//form_id
            userParam.Open_Id = openid;
            userParam.AddDate = DateTime.Now;
            userParam.Form_Id = formid;
            userParam.State = 1;
            userParam.SendCount = 0;
            userParam.AddDate = DateTime.Now;
            userParam.LoseDateTime = DateTime.Now.AddDays(7);//form_id 有效期7天

            TemplateMsg_UserParamBLL.SingleModel.Add(userParam);
            return Json(new { isok = true, FormId = formid }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除插入的最后一个FormId
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteLastFormId(string appid, string openid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            TemplateMsg_UserParam delFormId = TemplateMsg_UserParamBLL.SingleModel.getLastParamByAppIdOpenId(appid, openid);
            if (delFormId != null)
            {
                TemplateMsg_UserParamBLL.SingleModel.Delete(delFormId.Id);
            }
            return Json(new { isok = true, FormId = delFormId?.Form_Id }, JsonRequestBehavior.AllowGet);
        }
        #endregion 支付相关

        /// <summary>
        /// 根据url 请求获取图片
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public string HttpPostSaveImg(string Url, string postDataStr)
        {
            string aliTempImgKey = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                CookieContainer cookie = new CookieContainer();
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
                request.CookieContainer = cookie;
                using (Stream myRequestStream = request.GetRequestStream())
                {
                    using (StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312")))
                    {
                        myStreamWriter.Write(postDataStr);
                    }
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        //  log4net.LogHelper.WriteInfo(GetType(), JsonConvert.SerializeObject(response));
                        response.Cookies = cookie.GetCookies(response.ResponseUri);
                        Stream myResponseStream = response.GetResponseStream();

                        byte[] byteData = StreamHelpers.ReadFully(myResponseStream);
                        myRequestStream.Close();
                        SaveImageToAliOSS(byteData, out aliTempImgKey);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
            }

            return aliTempImgKey;
        }

        public bool SaveImageToAliOSS(byte[] byteArray, out string aliTempImgKey)
        {
            string aliTempImgFolder = AliOSSHelper.GetOssImgKey("jpg", false, out aliTempImgKey);
            return AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, byteArray, 1, ".jpg");
        }

        [HttpPost]
        public ActionResult SetUserInfoTelePhone()
        {
            int userInfoId = Utility.IO.Context.GetRequestInt("userInfoId", 0);
            string encryptedData = Utility.IO.Context.GetRequest("encryptedData", "");
            string iv = Utility.IO.Context.GetRequest("iv", "");
            string session_key = Utility.IO.Context.GetRequest("session_key", "");

            string result = WxHelper.AESDecrypt(encryptedData, session_key, iv);
            GetPhoneNumberReturnJson phoneNumberModel = JsonConvert.DeserializeObject<GetPhoneNumberReturnJson>(result);
            if (phoneNumberModel == null || string.IsNullOrWhiteSpace(phoneNumberModel.purePhoneNumber))
            {
                return Json(new { isok = false, msg = "传入号码不可为空" + JsonConvert.SerializeObject(phoneNumberModel) });
            }

            if (userInfoId <= 0)
            {
                return Json(new { isok = false, msg = "用户不存在" });
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userInfoId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" });
            }

            userInfo.TelePhone = phoneNumberModel.purePhoneNumber;

            bool isSuccess = C_UserInfoBLL.SingleModel.Update(userInfo, "TelePhone");

            return Json(new { isok = isSuccess, msg = isSuccess ? "成功" : "失败" });
        }

        #region 上传

        [HttpPost]
        public ActionResult Upload(string filetype = "img")
        {
            if (Request.Files.Count == 0)
            {
                return this.ApiResult(false, "请选择要上传的图片！");
            }
            using (Stream stream = Request.Files[0].InputStream)
            {
                string fileExtension = Path.GetExtension(Request.Files[0].FileName).ToLower();
                HashSet<string> img = new HashSet<string>() { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                HashSet<string> video = new HashSet<string>() { ".mp4", ".rmvb", ".flv" };
                if (filetype == "img" && !img.Contains(fileExtension))
                {
                    return this.ApiResult(false, $"上传失败！只支持：{string.Join(",", img)}格式的图片！");
                }
                else if (filetype == "video" && !video.Contains(fileExtension))
                {
                    return this.ApiResult(false, $"上传失败！只支持：{string.Join(",", img)}格式的视频！");
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
                    return this.ApiResult(true, url);
                }
                else
                {
                    return this.ApiResult(false, "上传失败！");
                }
            }
        }

        #endregion 上传

        #region Common

        /// <summary>
        /// 使用腾讯地图API根据IP获取地址
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAddressByIp()
        {
            string key = ConfigurationManager.AppSettings["TXMAPKEY"];
            string ip = WebHelper.GetIP();
            string result = HttpHelper.GetData($"http://apis.map.qq.com/ws/location/v1/ip?ip={ip}&key={key}");
            return Content(result);
        }

        /// <summary>
        /// 获取用户的收货地址
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public ActionResult GetUserAddress(int? userid)
        {
            if (!userid.HasValue)
            {
                return BaseApiResult(false, "非法请求");
            }
            List<UserAddress> list = UserAddressBLL.SingleModel.GetList($"userid={userid.Value}", 20, 1, "*", "isdefault desc,updatetime desc");
            return BaseApiResult(true, "", list);
        }

        /// <summary>
        /// 编辑收货地址，包括：修改，添加，
        /// 当id=0时添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult EditUserAddress(UserAddress model)
        {
            if (model == null || model.id < 0)
                return BaseApiResult(false, "非法请求");

            if (model.contact.IsNullOrEmpty())
                return BaseApiResult(false, "联系人不能空");

            if (model.phone.IsNullOrEmpty())
                return BaseApiResult(false, "手机号码不能空");

            if (!new Regex(@"^1[\d]{10}$").IsMatch(model.phone))
                return BaseApiResult(false, "手机号码格式不正确");

            if (model.province.IsNullOrEmpty() ||
                model.city.IsNullOrEmpty() ||
                model.district.IsNullOrEmpty() ||
                model.street.IsNullOrEmpty())
                return BaseApiResult(false, "地区和详细地址不能空");

            if (UserAddressBLL.SingleModel.Exists(model))
            {
                return BaseApiResult(false, "该地址已存在");
            }

            if (model.id == 0)
            {
                int newId = Convert.ToInt32(UserAddressBLL.SingleModel.Add(model));
                if (newId > 0)
                    return BaseApiResult(true, "添加成功", newId);
                else
                    return BaseApiResult(false, "添加失败");
            }
            else
            {
                model.updatetime = DateTime.Now;
                bool result = UserAddressBLL.SingleModel.Update(model);

                if (result)
                    return BaseApiResult(true, "修改成功");
                else
                    return BaseApiResult(false, "修改失败");
            }
        }

        /// <summary>
        /// 设置默认，取消默认
        /// 当act=changeDefault时 isdefault=1设置默认，isdefault=0时取消默认
        /// </summary>
        /// <returns></returns>
        public ActionResult changeUserAddressState(int id = 0, int userid = 0, int isdefault = 0)
        {
            if (id == 0 || userid == 0)
                return BaseApiResult(false, "非法请求");

            bool result = UserAddressBLL.SingleModel.changeUserAddressState(id, userid, isdefault);

            if (result)
                return BaseApiResult(true, "设置成功");
            else
                return BaseApiResult(false, "设置失败");
        }

        /// <summary>
        /// 删除收货地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteUserAddress(int id)
        {
            int reault = UserAddressBLL.SingleModel.Delete(id);
            return BaseApiResult(reault > 0, reault > 0 ? "删除成功" : "删除失败");
        }

        #endregion Common

        #region 减免营销

        /// <summary>
        /// 获取优惠规则列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDiscountRules()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            result.isok = false;
            result.Msg = "获取失败";

            int aId = Context.GetRequestInt("aId", 0);
            if (aId <= 0)
            {
                result.Msg = "识别不到店铺标识";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModel(aId);
            if (xcxAccountRelation == null)
            {
                result.Msg = "店铺标识无效";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate template = XcxTemplateBLL.SingleModel.GetModel(xcxAccountRelation.TId);
            if (template == null)
            {
                result.Msg = "店铺类型无效";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            //规则文案 int: 1新用户首单立减,2用户每日首单立减,3满减规则
            Dictionary<string, int> rulesRemark = new Dictionary<string, int>();
            List<DiscountRule> rules = new List<DiscountRule>();

            //是否开启满减规则
            bool isOpenDiscountRuls = false;
            switch (template.Type)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModelByAppId(xcxAccountRelation.Id);
                    if (store_Food == null)
                    {
                        result.Msg = "找不到指定店铺";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                    isOpenDiscountRuls = store_Food.funJoinModel.discountRuleSwitch;

                    if (store_Food.funJoinModel.newUserFirstOrderDiscountMoney > 0.00f)
                    {
                        rulesRemark.Add($"新用户首单立减 {store_Food.funJoinModel.newUserFirstOrderDiscountMoney} 元", 1);
                    }
                    if (store_Food.funJoinModel.userFirstOrderDiscountMoney > 0.00f)
                    {
                        rulesRemark.Add($"用户每日首单立减 {store_Food.funJoinModel.userFirstOrderDiscountMoney} 元", 2);
                    }
                    break;

                case (int)TmpType.小程序多门店模板:
                    FootBath store_MultiStore = FootBathBLL.SingleModel.GetModel($" appId = {aId} and HomeId = 0 ");
                    if (store_MultiStore == null)
                    {
                        result.Msg = "找不到指定店铺";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                    //读取配置
                    SwitchModel switchModel = null;
                    try
                    {
                        switchModel = JsonConvert.DeserializeObject<SwitchModel>(store_MultiStore.SwitchConfig);
                    }
                    catch (Exception)
                    {
                        switchModel = new SwitchModel();
                    }

                    isOpenDiscountRuls = switchModel.discountRuleSwitch;
                    if (switchModel.newUserFirstOrderDiscountMoney > 0.00f)
                    {
                        rulesRemark.Add($"新用户首单立减 {switchModel.newUserFirstOrderDiscountMoney} 元", 1);
                    }
                    if (switchModel.userFirstOrderDiscountMoney > 0.00f)
                    {
                        rulesRemark.Add($"用户每日首单立减 {switchModel.userFirstOrderDiscountMoney} 元", 2);
                    }
                    break;
            }
            if (isOpenDiscountRuls)
            {
                rules = DiscountRuleBLL.SingleModel.GetListByAId(aId) ?? new List<DiscountRule>();
                rules.ForEach(r =>
                {
                    rulesRemark.Add($"满 {r.meetMoney} 元,减 {r.discountMoney} 元 ", 3);
                });
            }

            result.isok = true;
            result.Msg = "成功获取";
            result.dataObj = new
            {
                rulesRemark = rulesRemark.Select(r => new
                {
                    type = r.Value,
                    remark = r.Key
                }),
                rules = rules
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取用户下一订单能够直接减免的金额
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLastOrderDiscountMoney()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            result.isok = false;
            result.Msg = "获取失败";

            int aId = Context.GetRequestInt("aId", 0);
            if (aId <= 0)
            {
                result.Msg = "识别不到店铺标识";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModel(aId);
            if (xcxAccountRelation == null)
            {
                result.Msg = "店铺标识无效";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate template = XcxTemplateBLL.SingleModel.GetModel(xcxAccountRelation.TId);
            if (template == null)
            {
                result.Msg = "店铺类型无效";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            int userId = Context.GetRequestInt("userId", 0);
            if (userId <= 0)
            {
                result.Msg = "识别不到店铺标识";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                result.Msg = "找不到有效的用户标识";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            TmpType type = (TmpType)Enum.Parse(typeof(TmpType), $"{template.Type}");
            int discountMoney = DiscountRuleBLL.SingleModel.getFirstOrderDiscountMoney(userInfo.Id, aId, 0, type);

            result.isok = true;
            result.Msg = "成功获取";
            result.dataObj = new
            {
                discountMoney = discountMoney
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion 减免营销

        #region 排队接口

        /// <summary>
        /// 当前是否开启排队功能
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetSortQueueSwitch()
        {
            int aId = Context.GetRequestInt("aId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);

            Return_Msg_APP result = new Return_Msg_APP();
            result.isok = false;

            string errMsg = string.Empty;
            CommonSetting curSetting = CommonSettingBLL.GetCommonSetting(aId, ref storeId, ref errMsg);
            if (!string.IsNullOrWhiteSpace(errMsg))
            {
                result.Msg = errMsg;
                return Json(result);
            }

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new
            {
                sortQueueSwitch = curSetting.sortQueueSwitch
            };
            return Json(result);
        }

        /// <summary>
        /// 排队拿号接口
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult PutSortQueueMsg()
        {
            int aId = Context.GetRequestInt("aId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int userId = Context.GetRequestInt("userId", 0);
            int pCount = Context.GetRequestInt("pCount", 0);
            int pageType = Context.GetRequestInt("pageType", 8); //默认餐饮模板
            string telePhone = Context.GetRequest("telePhone", string.Empty);

            lock (_sortQueueLockObject)
            {
                Return_Msg_APP result = new Return_Msg_APP();
                result.isok = false;

                string errMsg = string.Empty;
                CommonSetting curSetting = CommonSettingBLL.GetCommonSetting(aId, ref storeId, ref errMsg);
                if (!string.IsNullOrWhiteSpace(errMsg))
                {
                    result.Msg = errMsg;
                    return Json(result);
                }
                if (!curSetting.sortQueueSwitch)
                {
                    result.Msg = "当前商家未开启排队功能!";
                    return Json(result);
                }

                if (pageType == 8 && pCount <= 0)
                {
                    result.Msg = "请正确填写就餐人数!";
                    return Json(result);
                }
                if (string.IsNullOrWhiteSpace(telePhone))
                {
                    result.Msg = "请正确填写手机号码!";
                    return Json(result);
                }
                List<SortQueue> sortQueue = SortQueueBLL.SingleModel.GetListByQueueing(aId, storeId);
                if (sortQueue.Any(s => s.userId == userId))
                {
                    result.Msg = "您已在队列中!";
                    return Json(result);
                }

                //拿号前队列情况
                List<SortQueue> all_sortQueueing = SortQueueBLL.SingleModel.GetListByQueueing(aId, storeId);

                SortQueue newSort = new SortQueue();
                newSort.aId = aId;
                newSort.storeId = storeId;
                newSort.userId = userId;
                newSort.pCount = pCount;
                newSort.telephone = telePhone;
                newSort.createDate = DateTime.Now;
                newSort.sortNo = curSetting.sortNo_next;
                newSort.pageType = pageType;

                newSort.id = Convert.ToInt32(SortQueueBLL.SingleModel.Add(newSort));
                if (newSort.id <= 0)
                {
                    result.Msg = "加入排队队列失败,请重试!";
                    return Json(result);
                }
                else
                {
                    //更新下一个队列号的号码
                    curSetting.sortNo_next++;
                    CommonSettingBLL.UpdateCommonSetting(curSetting, aId, storeId);

                    errMsg = string.Empty;
                    //ps: 前端提供formId的节点不够,去掉拿号成功通知
                    ////发送模板消息通知用户拿号成功
                    //_sortQueueBLL.SendTemplateMsgToGetNoSuccessUser(ref errMsg, newSort);
                    //if (!string.IsNullOrWhiteSpace(errMsg))
                    //{
                    //    result.Msg = errMsg;
                    //    return Json(result, JsonRequestBehavior.AllowGet);
                    //}

                    //发送模板消息通知到号或即将到号用户
                    SortQueueBLL.SingleModel.SendTemplateMsgToNextUser(ref errMsg, newSort.aId, newSort.storeId, all_sortQueueing);
                    if (!string.IsNullOrWhiteSpace(errMsg))
                    {
                        result.Msg = errMsg;
                        return Json(result);
                    }

                    result.isok = true;
                    result.Msg = "加入队列成功,请注意队列情况,以免过号!";
                    return Json(result);
                }
            }
        }

        /// <summary>
        /// 获取当前队列位置信息
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetUserInSortQueuesPlanMsg()
        {
            int aId = Context.GetRequestInt("aId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int userId = Context.GetRequestInt("userId", 0);

            Return_Msg_APP result = new Return_Msg_APP();
            result.isok = false;

            //前面有几桌/人
            int befterSortCount = 0;
            List<SortQueue> sortQueues = SortQueueBLL.SingleModel.GetListByQueueing(aId, storeId);
            SortQueue curSort = sortQueues?.FirstOrDefault(s => s.userId == userId);
            if (sortQueues == null || curSort == null) //排队队伍中不包含当前用户时
            {
                befterSortCount = (sortQueues == null ? 0 : sortQueues.Count); //当前队列有几桌/人

                result.isok = true;
                result.code = "0";
                result.Msg = "获取信息成功!";
                result.dataObj = new
                {
                    befterSortCount = befterSortCount,
                    sortQueue = curSort
                };
                return Json(result);
            }

            befterSortCount = sortQueues.Count(s => s.id < curSort.id);

            result.isok = true;
            result.code = "1";
            result.Msg = "获取信息成功!";
            result.dataObj = new
            {
                befterSortCount = befterSortCount,
                sortQueue = curSort
            };
            return Json(result);
        }

        /// <summary>
        /// 取消排队
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult CancelSortQueue()
        {
            int aId = Context.GetRequestInt("aId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int sortId = Context.GetRequestInt("sortId", 0);

            Return_Msg_APP result = new Return_Msg_APP();
            result.isok = false;

            List<SortQueue> all_sortQueueing = SortQueueBLL.SingleModel.GetListByQueueing(aId, storeId);
            if (all_sortQueueing == null)
            {
                result.Msg = "更新失败";
            }

            SortQueue curSort = all_sortQueueing.FirstOrDefault(s => s.id == sortId);
            if (curSort == null || curSort.id < 0 || curSort.state != 0)
            {
                result.Msg = "该排队记录已失效或已经处理";
                return Json(result);
            }
            if (curSort.state != 0)
            {
                result.Msg = "当前排队已不可操作!";
                return Json(result);
            }

            curSort.state = -1;
            curSort.updateDate = DateTime.Now;
            bool isUpdateSuccess = SortQueueBLL.SingleModel.Update(curSort, "state,updateDate");

            result.isok = isUpdateSuccess;
            result.Msg = isUpdateSuccess ? "取消成功!" : "取消失败,请重试!";
            if (isUpdateSuccess)
            {
                string errMsg = string.Empty;
                //发送模板消息
                SortQueueBLL.SingleModel.SendTemplateMsgToNextUser(ref errMsg, curSort.aId, curSort.storeId, all_sortQueueing);
            }
            return Json(result);
        }

        #endregion 排队接口

        #region 获取提货码二维码

        public ActionResult GetTableNoQrCode()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            int orderId = Context.GetRequestInt("orderId", 0);
            string appId = Context.GetRequest("appId", string.Empty);
            string bussinessAppid = Context.GetRequest("bussinessAppid", string.Empty);
            if (orderId <= 0 || string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(bussinessAppid))
            {
                result.Msg = $"参数错误 order：{orderId}||appId:{appId}||bussinessAppid:{bussinessAppid}";
                return Json(result);
            }
            EntGoodsOrder goodsOrder = EntGoodsOrderBLL.SingleModel.GetModelByAppIdAndId(appId, orderId);
            if (goodsOrder == null)
            {
                result.Msg = "找不到该订单";
                return Json(result);
            }
            string msg = string.Empty;
            //必须用商家端的appid去生成商家端的小程序二维码，不能用别的小程序appid去生成该小程序不存在页面路径的二维码
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(bussinessAppid);
            if (xcx == null)
            {
                result.Msg = "没有找到权限信息";
                return Json(result);
            }
            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcx, ref token))
            {
                result.Msg = token;
                return Json(result);
            }
            //获取二维码
            qrcodeclass resultQcode = CommondHelper.GetWxQrcode(token, $"{WebSiteConfig.StoreOrderPath}", $"1_{goodsOrder.OrderNum}");
            if (resultQcode != null)
            {
                if (resultQcode.isok > 0)
                {
                    result.isok = true;
                    result.Msg = resultQcode.url;
                }
                else
                {
                    result.Msg = resultQcode.msg + $"{WebSiteConfig.StoreOrderPath}" + $"1_{goodsOrder.OrderNum}";
                }
            }

            return Json(result);
        }

        #endregion 获取提货码二维码

        [NonAction]
        public JsonResult GetJsonResult(bool isok = false, string Msg = null, string code = null, object dataObj = null, bool allowGet = true)
        {
            Return_Msg_APP jsonModel = new Return_Msg_APP
            {
                Msg = Msg,
                code = code,
                dataObj = dataObj,
                isok = isok
            };
            return new JsonResultFormat() { Data = jsonModel, DateFormat = "yyyy/MM/dd HH:mm:ss", JsonRequestBehavior = allowGet ? JsonRequestBehavior.AllowGet : JsonRequestBehavior.DenyGet };
        }

        #region 商品评论

        /// <summary>
        /// 添加评论
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult AddGoodsComment()
        {
            string appId = Context.GetRequest("appid", string.Empty);
            int userid = Context.GetRequestInt("userid", 0);
            int orderid = Context.GetRequestInt("orderid", 0);
            int goodsid = Context.GetRequestInt("goodsid", 0);
            int praise = Context.GetRequestInt("praise", 0);
            int logisticsscore = Context.GetRequestInt("logisticsscore", 0);
            int servicescore = Context.GetRequestInt("servicescore", 0);
            int descriptivescore = Context.GetRequestInt("descriptivescore", 0);
            int goodstype = Context.GetRequestInt("goodstype", 0);
            int anonymous = Context.GetRequestInt("anonymous", 0);
            string comment = Context.GetRequest("comment", string.Empty);
            string imgurl = Context.GetRequest("imgurl", string.Empty);
            string goodsspecification = Context.GetRequest("goodsspecification", string.Empty);
            string goodsimg = Context.GetRequest("goodsimg", string.Empty);
            int goodsprice = Context.GetRequestInt("goodsprice", 0);

            returnObj = new Return_Msg_APP();
            if (userid <= 0)
            {
                returnObj.Msg = "用户ID不能为空";
                return Json(returnObj);
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userid);
            if (userinfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "请先授权";
                return Json(returnObj);
            }
            XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxtemplate == null)
            {
                returnObj.Msg = "找不到模板数据";
                return Json(returnObj);
            }
            if (goodsid <= 0)
            {
                returnObj.Msg = "商品ID不能为空";
                return Json(returnObj);
            }

            string msg = "";
            //获取商品名称
            string goodsname = GoodsCommentBLL.SingleModel.GetGoodsName(xcxtemplate.Type, goodstype, goodsid, ref msg);
            if (msg != "")
            {
                returnObj.Msg = msg;
                return Json(returnObj);
            }

            //判断是否评论过
            bool iscomment = GoodsCommentBLL.SingleModel.IsComment(xcxtemplate.Type, goodstype, goodsid, orderid, xcxrelation.Id);
            if (iscomment)
            {
                returnObj.isok = false;
                returnObj.Msg = "已评论，请刷新重试！";
                return Json(returnObj);
            }

            GoodsComment commentmodel = new GoodsComment();
            commentmodel.AddTime = DateTime.Now;
            commentmodel.AId = xcxrelation.Id;
            commentmodel.Anonymous = anonymous == 1;
            commentmodel.Comment = comment;
            commentmodel.GoodsId = goodsid;
            commentmodel.GoodsName = goodsname;
            commentmodel.Hidden = false;
            commentmodel.NickName = userinfo.NickName;
            commentmodel.UserId = userinfo.Id;
            commentmodel.Praise = praise;
            commentmodel.LogisticsScore = logisticsscore;
            commentmodel.ServiceScore = servicescore;
            commentmodel.DescriptiveScore = descriptivescore;
            commentmodel.State = 1;
            commentmodel.Type = goodstype;
            commentmodel.Points = 1;
            commentmodel.UpdateTime = DateTime.Now;
            commentmodel.GoodsPrice = goodsprice;
            commentmodel.GoodsImg = goodsimg;
            commentmodel.OrderId = orderid;
            commentmodel.GoodsSpecification = goodsspecification;
            if (imgurl != null && imgurl.Length > 0)
            {
                commentmodel.HaveImg = 1;
            }
            commentmodel.Id = Convert.ToInt32(GoodsCommentBLL.SingleModel.Add(commentmodel));
            if (commentmodel.HaveImg==1)
            {
                string[] imgArray = imgurl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (imgArray.Length > 0)
                {
                    foreach (string item in imgArray)
                    {
                        C_AttachmentBLL.SingleModel.Add(new C_Attachment
                        {
                            itemId = commentmodel.Id,
                            createDate = DateTime.Now,
                            filepath = item,
                            itemType = (int)AttachmentItemType.小程序商品评论轮播图,
                            thumbnail = item,
                            status = 0
                        });
                    }
                }
            }

            returnObj.isok = commentmodel.Id > 0;
            returnObj.Msg = returnObj.isok ? "成功" : "失败";
            return Json(returnObj);
        }
        
        /// <summary>
        /// 多件商品添加评论
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey,HttpPost]
        public ActionResult AddManyGoodsComment(string appId="",int userId=0,int orderId=0,int goodsType=0,string listJson = "")
        {
            returnObj = new Return_Msg_APP();
            if(string.IsNullOrEmpty(listJson))
            {
                returnObj.Msg = "评论内容Json不能为空";
                return Json(returnObj);
            }
            List<GoodsComment> list = JsonConvert.DeserializeObject<List<GoodsComment>>(listJson);
            if (list==null || list.Count<=0)
            {
                returnObj.Msg = "评论内容不能为空";
                return Json(returnObj);
            }
            if (userId <= 0)
            {
                returnObj.Msg = "用户ID不能为空";
                return Json(returnObj);
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userinfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "请先授权";
                return Json(returnObj);
            }
            XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxtemplate == null)
            {
                returnObj.Msg = "找不到模板数据";
                return Json(returnObj);
            }
            if(!GoodsCommentBLL.SingleModel.OrderCommenting(xcxtemplate.Type,goodsType,orderId))
            {
                returnObj.Msg = "订单评论失败";
                return Json(returnObj);
            }
            foreach (GoodsComment item in list)
            {
                item.AddTime = DateTime.Now;
                item.AId = xcxrelation.Id;
                item.NickName = userinfo.NickName;
                item.UserId = userinfo.Id;
                item.State = 1;
                item.Type = goodsType;
                item.Points = 1;
                item.UpdateTime = DateTime.Now;
                item.OrderId = orderId;
                if (item.HeadImgUrl != null && item.HeadImgUrl.Length > 0)
                {
                    item.HaveImg = 1;
                }
                item.Id = Convert.ToInt32(GoodsCommentBLL.SingleModel.Add(item));
                if (item.HaveImg==1)
                {
                    string[] imgArray = item.HeadImgUrl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (imgArray.Length > 0)
                    {
                        foreach (string itemImg in imgArray)
                        {
                            C_AttachmentBLL.SingleModel.Add(new C_Attachment
                            {
                                itemId = item.Id,
                                createDate = DateTime.Now,
                                filepath = itemImg,
                                itemType = (int)AttachmentItemType.小程序商品评论轮播图,
                                thumbnail = itemImg,
                                status = 0
                            });
                        }
                    }
                }
            }
            
            returnObj.isok = true;
            returnObj.Msg = "成功";
            return Json(returnObj);
        }

        /// <summary>
        /// 获取商品评论
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetGoodsCommentList()
        {
            string appId = Context.GetRequest("appid", string.Empty);
            int goodsid = Context.GetRequestInt("goodsid", 0);
            int userid = Context.GetRequestInt("userid", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int haveimg = Context.GetRequestInt("haveimg", -1);//-1：全部，0：无图，1：有图

            returnObj = new Return_Msg_APP();
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "请先授权";
                return Json(returnObj);
            }
            if (goodsid <= 0)
            {
                returnObj.Msg = "商品ID不能为空";
                return Json(returnObj);
            }

            int count = 0;
            List<GoodsComment> list = GoodsCommentBLL.SingleModel.GetGoodsCommentListApi(xcxrelation.Id, goodsid, userid, 0, pageIndex, pageSize, haveimg, ref count);
            returnObj.dataObj = new { count = count, list = list };
            returnObj.isok = true;
            returnObj.Msg = returnObj.isok ? "成功" : "失败";

            return Json(returnObj);
        }

        /// <summary>
        /// 获取个人评论
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetUserGoodsCommentList()
        {
            string appId = Context.GetRequest("appid", string.Empty);
            int userid = Context.GetRequestInt("userid", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int haveimg = Context.GetRequestInt("haveimg", -1);//-1：全部，0：无图，1：有图

            returnObj = new Return_Msg_APP();
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "请先授权";
                return Json(returnObj);
            }
            if (userid <= 0)
            {
                returnObj.Msg = "用户ID不能为空";
                return Json(returnObj);
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userid);
            if (userinfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }

            int count = 0;
            List<GoodsComment> list = GoodsCommentBLL.SingleModel.GetUserGoodsCommentListApi(xcxrelation.Id, userid, haveimg, pageIndex, pageSize, ref count);
            returnObj.dataObj = new { count = count, list = list };
            returnObj.isok = true;
            returnObj.Msg = returnObj.isok ? "成功" : "失败";

            return Json(returnObj);
        }

        /// <summary>
        /// 评论点赞
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult PointsGoodsComment()
        {
            string appId = Context.GetRequest("appid", string.Empty);
            int commentid = Context.GetRequestInt("id", 0);
            int userid = Context.GetRequestInt("userid", 0);

            returnObj = new Return_Msg_APP();
            if (userid <= 0)
            {
                returnObj.Msg = "用户ID不能为空";
                return Json(returnObj);
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userid);
            if (userinfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "请先授权";
                return Json(returnObj);
            }

            if (commentid <= 0)
            {
                returnObj.Msg = "评论ID不能为0";
                return Json(returnObj);
            }
            lock (_goodsMommentLocker)
            {
                CityUserFavoriteMsg pmodel = CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(xcxrelation.Id, commentid, userid, 1, (int)PointsDataType.评论);
                if (pmodel == null)
                {
                    pmodel = new CityUserFavoriteMsg();
                    pmodel.actionType = 1;
                    pmodel.addTime = DateTime.Now;
                    pmodel.aid = xcxrelation.Id;
                    pmodel.Datatype = (int)PointsDataType.评论;
                    pmodel.msgId = commentid;
                    pmodel.state = 0;
                    pmodel.userId = userid;
                    pmodel.Id = Convert.ToInt32(CityUserFavoriteMsgBLL.SingleModel.Add(pmodel));
                    if (pmodel.Id <= 0)
                    {
                        returnObj.Msg = "点赞失效";
                        return Json(returnObj);
                    }
                }
                else if (pmodel.state == -1)
                {
                    pmodel.state = 0;
                    returnObj.isok = CityUserFavoriteMsgBLL.SingleModel.Update(pmodel, "state");
                }
                else if (pmodel.state == 0)//取消点赞
                {
                    pmodel.state = -1;
                    returnObj.isok = CityUserFavoriteMsgBLL.SingleModel.Update(pmodel, "state");
                }

                GoodsComment model = GoodsCommentBLL.SingleModel.GetModel(commentid);
                if (model == null)
                {
                    returnObj.Msg = "该评论已被删除";
                    return Json(returnObj);
                }

                model.Points = model.Points + (pmodel.state == -1 ? -1 : 1);
                model.UpdateTime = DateTime.Now;
                returnObj.isok = GoodsCommentBLL.SingleModel.Update(model, "points,updatetime");
            }

            returnObj.Msg = returnObj.isok ? "成功" : "失败";
            return Json(returnObj);
        }


        public ActionResult GetServerTime()
        {
            return Content(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        #endregion 商品评论
    }

    /// <summary>
    /// 处理并记录错误
    /// </summary>
    public class ExceptionLog : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            //log4net.LogHelper.WriteError(this.GetType(), filterContext.Exception);

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = ExceptionLogFormat(filterContext);
            }
            else
            {
                filterContext.Result = ExceptionLogFormat(filterContext);
            }
            filterContext.ExceptionHandled = true;
            base.OnException(filterContext);
        }

        private JsonResult ExceptionLogFormat(ExceptionContext filterContext)
        {
            string errorTraceInfo = filterContext.Exception.StackTrace;
            int errorIndex = errorTraceInfo.IndexOf("位置");
            string errorLine = errorIndex > -1 ?
                            errorTraceInfo.Substring(errorTraceInfo.IndexOf("位置"), errorTraceInfo.Length - errorTraceInfo.IndexOf("位置")).Replace("位置", "") :
                            string.Empty;

            int errorEnterIndex = errorLine.IndexOf("\r\n");
            errorLine = errorEnterIndex > -1 ?
                        errorLine.Substring(0, errorLine.IndexOf("\r\n")) :
                        string.Empty;

            //#region 保存到数据库异常信息
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<p>客户端IP:{HttpContext.Current.Request.UserHostAddress}</p>")
                .AppendLine($"<p style=\"color:red\">请求URL:{HttpContext.Current.Request.Url.ToString()}</p>")
                .AppendLine($"<p>客户端浏览器版本:{HttpContext.Current.Request.UserAgent}</p>")
                .AppendLine($"<p>异常对象：{filterContext.Exception.Source}</p>")
                .AppendLine($"<p>异常实例{filterContext.Exception.TargetSite}</p>")
                .AppendLine($"<p>异常位置：{errorLine}</p>")
                .AppendLine($"<p>详细信息：{filterContext.Exception.Message}</p>")
                .AppendLine($"<p>调用堆栈:{filterContext.Exception.StackTrace}");

            Entity.MiniApp.CommandExceptionLog exLog = new Entity.MiniApp.CommandExceptionLog
            {
                WebAddress = HttpContext.Current.Request.UserHostAddress,
                Version = "小程序接口",
                SourcePath = "OnException全局错误捕获",
                ExceptionMsg = sb.ToString()
            };
            CommandExceptionLogBLL.SingleModel.Add(exLog);

            //#endregion

            JsonResult result = new JsonResult();
            result.Data = new BaseResult()
            {
                result = false,
                msg = filterContext.Exception.Message,
                obj = $@"异常对象：{filterContext.Exception.Source}
                       ；异常实例{filterContext.Exception.TargetSite}
                       ；异常位置：({errorLine}
                      )；查询参数：({HttpContext.Current.Request.QueryString.ToString()})",
                errcode = filterContext.Exception.HResult
            };
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;
        }
    }
}