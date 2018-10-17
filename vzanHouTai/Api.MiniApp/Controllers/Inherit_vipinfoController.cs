using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Plat;
using BLL.MiniApp.Stores;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Plat;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    [ExceptionLog]
    public partial class InheritController : AsyncController
    {
        private static readonly object lockOrder = new object();
        
        
        
        
        
        protected static SalesManRecordBLL salesManRecordBLL = new SalesManRecordBLL();
        
        
        
        
        
        
        

        

        // GET: Inherit_vipinfo
        /// <summary>
        /// 获取会员信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetVipInfo()
        {
            string appid = Utility.IO.Context.GetRequest("appid", string.Empty);
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            int uid = Utility.IO.Context.GetRequestInt("uid", 0);
            if (uid <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel($"id={uid} and appId='{appid}'");
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcx == null)
            {
                return Json(new { isok = false, msg = "小程序不存在" }, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            // MiniappVipLevel def_level = _miniappVipLevelBll

            VipRelation vipRelation = VipRelationBLL.SingleModel.GetVipModel(uid, xcx.AppId, xcxTemplate.Type, aid: xcx.Id);
            int saveMoneySum = SaveMoneySetUserBLL.SingleModel.GetSaveMoneySum(uid);
            if (vipRelation != null)
            {
                vipRelation.SaveMoneySum = saveMoneySum * 0.01;
            }
            //return Json(new { isok = true, model = vipRelation }, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                isok = true,
                model = new
                {
                    vipRelation.AccountMoney,
                    vipRelation.AccountMoneystr,
                    vipRelation.Id,
                    vipRelation.PriceSum,
                    vipRelation.SaveMoneySum,
                    vipRelation.TelePhone,
                    vipRelation.WxVipCode,
                    vipRelation.addtime,
                    vipRelation.appId,
                    vipRelation.headimgurl,
                    levelInfo = new
                    {
                        vipRelation.levelInfo.Id,
                        vipRelation.levelInfo.PlatChildGoodsList,
                        vipRelation.levelInfo.appId,
                        vipRelation.levelInfo.bgcolor,
                        vipRelation.levelInfo.discount,
                        entGoodsList = vipRelation.levelInfo.entGoodsList?.Select(p => new
                        {
                            p.name
                        }),//不需要返回那么多字段，字段太多超出小程序端一次保存数据的最大长度
                        vipRelation.levelInfo.foodgoodslist,
                        vipRelation.levelInfo.gids,
                        vipRelation.levelInfo.goodslist,
                        vipRelation.levelInfo.level,
                        vipRelation.levelInfo.name,
                        vipRelation.levelInfo.showtime,
                        vipRelation.levelInfo.state,
                        vipRelation.levelInfo.type,
                        vipRelation.levelInfo.updatetime,
                    },
                    vipRelation.levelName,
                    vipRelation.levelid,
                    vipRelation.pricestr,
                    vipRelation.reservation,
                    vipRelation.showaddtime,
                    vipRelation.showupdatetime,
                    vipRelation.state,
                    vipRelation.uid,
                    vipRelation.updatetime,
                    vipRelation.userType,
                    vipRelation.userTypeStr,
                    vipRelation.username,
                }
            }, JsonRequestBehavior.AllowGet);
        }

        #region 微信会员卡
        /// <summary>
        /// 获取用户会员卡Code 来判断是否领取过 
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public ActionResult GetWxCardCode(string appid, int UserId, int type = 0)
        {
            if (string.IsNullOrEmpty(appid) || UserId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxrelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            int modelId = 0;

            switch (type)
            {
                case 0:
                    //默认电商版
                    var store = StoreBLL.SingleModel.GetModelByRid(xcxrelation.Id);
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = store.Id;
                    break;
                case 1:
                    //表示餐饮
                    var food = FoodBLL.SingleModel.GetModel($"appId={xcxrelation.Id}");
                    if (food == null)
                    {
                        return Json(new { isok = false, msg = "找不到该餐饮店铺" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = food.Id;
                    break;
                case 2:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(xcxrelation.Id);
                    if (ent == null)
                        return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);

                    modelId = ent.aid;
                    break;
                case 3://表示足浴版
                case 4://表示多门店
                    FootBath footBath = FootBathBLL.SingleModel.GetModel($"appId={xcxrelation.Id}");
                    if (footBath == null)
                        return Json(new { isok = false, msg = "找不到该版本" }, JsonRequestBehavior.AllowGet);
                    modelId = footBath.appId;
                    break;
                case 5://表示小未平台子模板
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(xcxrelation.Id, 2);
                    if (platStore == null)
                        return Json(new { isok = false, msg = "找不到子模板店铺" }, JsonRequestBehavior.AllowGet);
                    modelId = platStore.Aid;
                    break;
            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={modelId} and Type={type}");
            if (_vipWxCard == null)
            {
                return Json(new { isok = false, msg = "还未生成会员卡(请到后台设置同步微信会员卡)" }, JsonRequestBehavior.AllowGet);
            }

            XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
            string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(_vipWxCard.User_Name);
            var authorizer_Access_Token = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);
            if (string.IsNullOrEmpty(authorizer_Access_Token))
            {
                return Json(new { isok = false, msg = "失败(authorizer_Access_Token为空)" });
            }

            var _vipWxCardCode = VipWxCardCodeBLL.SingleModel.GetModel($"CardId='{_vipWxCard.CardId}' and UserId={UserId}");
            if (_vipWxCardCode != null)
            {
                //然后判断微信端卡包里该会员卡有没有被删除
                var codeStateResult = WxHelper.DoPostJson($"https://api.weixin.qq.com/card/code/get?access_token={authorizer_Access_Token}", JsonConvert.SerializeObject(new
                {
                    card_id = _vipWxCardCode.CardId,
                    code = _vipWxCardCode.Code,
                    check_consume = false

                }));

                if (codeStateResult.Contains("DELETE") || codeStateResult.Contains("UNAVAILABLE"))
                {
                    //表示已经删除或失效 然后更新数据库 删掉数据保持一致与微信卡包那边
                    if (VipWxCardCodeBLL.SingleModel.Delete(_vipWxCardCode.Id) > 0)
                    {
                        _vipWxCardCode = null;
                    }
                }
            }

            return Json(new { isok = true, msg = "ok", obj = _vipWxCardCode }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存微信会员卡 用户领卡后也就是同步后得到的code到数据库 下次使用 标示已经同步到卡包
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveWxCardCode(string appid, int UserId, string code, int type = 0)
        {
            if (string.IsNullOrEmpty(appid) || string.IsNullOrEmpty(code) || UserId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" });
            }
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxrelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" });
            }
            int modelId = 0;
            switch (type)
            {
                case 0:
                    var store = StoreBLL.SingleModel.GetModelByRid(xcxrelation.Id);
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = store.Id;
                    break;
                case 1:
                    //表示餐饮
                    var food = FoodBLL.SingleModel.GetModel($"appId={xcxrelation.Id}");
                    if (food == null)
                    {
                        return Json(new { isok = false, msg = "找不到该餐饮店铺" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = food.Id;
                    break;
                case 2:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(xcxrelation.Id);
                    if (ent == null)
                        return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);

                    modelId = ent.aid;
                    break;
                case 3://表示足浴版
                case 4://表示多门店
                    FootBath footBath = FootBathBLL.SingleModel.GetModel($"appId={xcxrelation.Id}");
                    if (footBath == null)
                        return Json(new { isok = false, msg = "找不到该足浴版" }, JsonRequestBehavior.AllowGet);
                    modelId = footBath.appId;
                    break;
                case 5://表示小未平台子模板
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(xcxrelation.Id, 2);
                    if (platStore == null)
                        return Json(new { isok = false, msg = "找不到子模板店铺" }, JsonRequestBehavior.AllowGet);
                    modelId = platStore.Aid;
                    break;
            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={modelId} and Type={type}");
            if (_vipWxCard == null)
            {
                return Json(new { isok = false, msg = "还未生成会员卡(请到后台设置同步微信会员卡)" });
            }
            var _vipWxCardCode = VipWxCardCodeBLL.SingleModel.GetModel($"CardId='{_vipWxCard.CardId}' and UserId={UserId}");
            if (_vipWxCardCode != null)
            {
                return Json(new { isok = false, msg = "非法操作(已经领卡)" });
            }
            XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
            string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(_vipWxCard.User_Name);
            var authorizer_Access_Token = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);
            if (string.IsNullOrEmpty(authorizer_Access_Token))
            {
                return Json(new { isok = false, msg = "失败(authorizer_Access_Token为空)" });
            }


            var desCode = JsonConvert.SerializeObject(new { encrypt_code = code });
            var desCodeResultJson = WxHelper.DoPostJson($"https://api.weixin.qq.com/card/code/decrypt?access_token={authorizer_Access_Token}", desCode);
            if (string.IsNullOrEmpty(desCodeResultJson))
            {
                return Json(new { isok = false, msg = "操作失败(解密Code失败)" });
            }

            var desCodeResult = JsonConvert.DeserializeObject<DesCodeResult>(desCodeResultJson);
            if (desCodeResult.errcode != 0 || desCodeResult.errmsg != "ok")
            {
                return Json(new { isok = false, msg = "操作失败(解密Code失败)" });
            }


            var CodeId = Convert.ToInt32(VipWxCardCodeBLL.SingleModel.Add(
              new VipWxCardCode()
              {
                  CardId = _vipWxCard.CardId,
                  Code = desCodeResult.code,
                  UserId = UserId,
                  AddTime = DateTime.Now
              }
              ));

            if (CodeId > 0)
                return Json(new { isok = true, msg = "ok", obj = CodeId });
            return Json(new { isok = false, msg = "error", obj = CodeId });
        }


        /// <summary>
        /// 更新会员卡的会员信息
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateWxCard(string appid, int UserId, int type = 0)
        {
            if (string.IsNullOrEmpty(appid) || UserId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" });
            }
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxrelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" });
            }
            int modelId = 0;
            int costSum = 0;

            switch (type)
            {
                case 0:
                    var store = StoreBLL.SingleModel.GetModelByRid(xcxrelation.Id);
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = store.Id;
                    costSum = VipRelationBLL.SingleModel.GetVipConsumptionrecord(UserId);//累计消费 分为单位
                    break;
                case 1:
                    //表示餐饮
                    var food = FoodBLL.SingleModel.GetModel($"appId={xcxrelation.Id}");
                    if (food == null)
                    {
                        return Json(new { isok = false, msg = "找不到该餐饮店铺" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = food.Id;
                    costSum = VipRelationBLL.SingleModel.GetFoodVipPriceSum(UserId);//累计消费 分为单位
                    break;
                case 2:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(xcxrelation.Id);
                    if (ent == null)
                        return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);

                    modelId = ent.aid;
                    costSum = VipRelationBLL.SingleModel.GetEntGoodsVipPriceSum(UserId);//累计消费 分为单位
                    break;
                case 3:
                case 4:
                    //表示足浴版
                    FootBath footBath = FootBathBLL.SingleModel.GetModel($"appId={xcxrelation.Id}");
                    if (footBath == null)
                        return Json(new { isok = false, msg = "找不到该版本" }, JsonRequestBehavior.AllowGet);
                    modelId = footBath.appId;
                    if (type == 3)
                        costSum = VipRelationBLL.SingleModel.GetFootbathVipPriceSum(UserId);//累计消费 分为单位 足浴的订单跟专业版同表
                    else
                        costSum = 0;//还未计算
                    break;
                case 5://表示小未平台子模板
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(xcxrelation.Id, 2);
                    if (platStore == null)
                        return Json(new { isok = false, msg = "找不到子模板店铺" }, JsonRequestBehavior.AllowGet);
                    modelId = platStore.Aid;
                    costSum = VipRelationBLL.SingleModel.GetPlatChildGoodsVipPriceSum(UserId);
                    break;
            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={modelId} and Type={type}");


            if (_vipWxCard == null)
            {
                return Json(new { isok = false, msg = "还未生成会员卡(请到后台设置同步微信会员卡)" });
            }
            var _vipWxCardCode = VipWxCardCodeBLL.SingleModel.GetModel($"CardId='{_vipWxCard.CardId}' and UserId={UserId}");
            if (_vipWxCardCode == null)
            {
                return Json(new { isok = false, msg = "请先领卡" });
            }

            XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
            string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(_vipWxCard.User_Name);
            string authorizer_Access_Token = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);
            if (string.IsNullOrEmpty(authorizer_Access_Token))
            {
                return Json(new { isok = false, msg = "失败(authorizer_Access_Token为空)" });
            }

            int saveMoney = 0;
            SaveMoneySetUser m = SaveMoneySetUserBLL.SingleModel.getModelByUserId(UserId);
            if (m != null)
                saveMoney = m.AccountMoney;

            string levelName = "普通会员";
            VipRelation level = VipRelationBLL.SingleModel.GetVipModel(UserId, appid);
            if (level != null)
            {
                if (level.levelInfo != null)
                    levelName = level.levelInfo.name;
            }

            if (string.IsNullOrEmpty(levelName))
            {
                levelName = "未知";
            }
            else
            {
                if (levelName.Length > 4)
                {
                    levelName = levelName.Replace("会员", "").Substring(0, 3);
                }
            }

            var updateCard = new
            {
                code = _vipWxCardCode.Code,
                card_id = _vipWxCardCode.CardId,
                custom_field_value1 = $"￥{float.Parse(saveMoney.ToString()) / 100}",
                custom_field_value2 = $"￥{float.Parse(costSum.ToString()) / 100}",
                custom_field_value3 = levelName
            };

            string updateCardJson = JsonConvert.SerializeObject(updateCard);
            string updateCardResult = Utility.IO.Context.PostData($"https://api.weixin.qq.com/card/membercard/updateuser?access_token={authorizer_Access_Token}", updateCardJson);
            if (updateCardResult.Contains("ok"))
                return Json(new { isok = true, msg = "ok", updateCard = updateCard, modelId = modelId, Type = type });

            return Json(new { isok = false, msg = "error", updateCardResult = updateCardResult });
        }

        /// <summary>
        /// 获取前端领卡数据
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public ActionResult GetCardSign(string appid, int UserId, int type = 0)
        {
            if (string.IsNullOrEmpty(appid) || UserId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxrelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            int modelId = 0;
            switch (type)
            {
                case 0:
                    Store store = StoreBLL.SingleModel.GetModelByRid(xcxrelation.Id);
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = store.Id;
                    break;
                case 1:
                    //表示餐饮
                    Food food = FoodBLL.SingleModel.GetModel($"appId={xcxrelation.Id}");
                    if (food == null)
                    {
                        return Json(new { isok = false, msg = "找不到该餐饮店铺" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = food.Id;
                    break;
                case 2:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(xcxrelation.Id);
                    if (ent == null)
                        return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);

                    modelId = ent.aid;
                    break;
                case 3://表示足浴版
                case 4://表示多门店
                    FootBath footBath = FootBathBLL.SingleModel.GetModel($"appId={xcxrelation.Id}");
                    if (footBath == null)
                        return Json(new { isok = false, msg = "找不到该足浴版" }, JsonRequestBehavior.AllowGet);
                    modelId = footBath.appId;
                    break;
                case 5://表示小未平台子模板
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(xcxrelation.Id, 2);
                    if (platStore == null)
                        return Json(new { isok = false, msg = "找不到子模板店铺" }, JsonRequestBehavior.AllowGet);
                    modelId = platStore.Aid;
                    break;

            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={modelId} and Type={type}");
            if (_vipWxCard == null)
            {
                return Json(new { isok = false, msg = "还未生成会员卡(请到后台设置同步微信会员卡)" }, JsonRequestBehavior.AllowGet);
            }
            var _vipWxCardCode = VipWxCardCodeBLL.SingleModel.GetModel($"CardId='{_vipWxCard.CardId}' and UserId={UserId}");
            if (_vipWxCardCode != null)
            {
                return Json(new { isok = false, msg = "已经领卡" }, JsonRequestBehavior.AllowGet);
            }

            XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
            string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(_vipWxCard.User_Name);
            var ACCESS_TOKEN = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);
            if (string.IsNullOrEmpty(ACCESS_TOKEN))
            {
                return Json(new { isok = false, msg = "失败(authorizer_Access_Token为空)" });
            }

            string timestamp = WxPayApi.GenerateTimeStamp();
            string api_ticket = GetApi_Ticket(ACCESS_TOKEN, _vipWxCard.User_Name);
            string signature = string.Empty;
            string cardId = _vipWxCard.CardId;

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("timestamp", timestamp);
            param.Add("api_ticket", api_ticket);
            param.Add("cardId", cardId);
            List<string> keyList = new List<string>(param.Values);
            keyList.Sort();//排序
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < keyList.Count; i++)
            {
                sb.Append(keyList[i]);
            }
            signature = Utility.EncodeHelper.EncryptToSHA1(sb.ToString());

            return Json(new { isok = true, msg = "ok", obj = new { cardId = cardId, timestamp = timestamp, signature = signature.ToLower() }, info = new { keyList = keyList, sortStr = sb.ToString(), api_ticket = api_ticket } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取Api_Ticket  用于微信会员卡前端调用
        /// </summary>
        /// <param name="ACCESS_TOKEN"></param>
        /// <param name="user_name">公众号原始Id</param>
        /// <returns></returns>
        private string GetApi_Ticket(string ACCESS_TOKEN, string user_name)
        {
            try
            {
                string key = "apiticket_wxcard_" + string.IsNullOrEmpty(user_name);
                string strTicket = string.Empty;
                string jssdkUrl = string.Format(WxSysConfig.Jsapi_ticket(ACCESS_TOKEN), ACCESS_TOKEN, "wx_card");

                string Jsapi_str = HttpHelper.GetData(jssdkUrl);
                Jsapi_ticket obj = SerializeHelper.DesFromJson<Jsapi_ticket>(Jsapi_str);
                if (obj.errcode != 0)
                {
                    log4net.LogHelper.WriteError(typeof(WxHelper), new Exception($"{key}失败1次apiticket_wxcard_ = {Jsapi_str}"));
                    Jsapi_str = HttpHelper.GetData(jssdkUrl);
                    obj = SerializeHelper.DesFromJson<Jsapi_ticket>(Jsapi_str);
                }
                if (obj.errcode != 0)
                {
                    log4net.LogHelper.WriteError(typeof(WxHelper), new Exception($"{key}失败2次apiticket_wxcard_ = {Jsapi_str}"));
                    return string.Empty;
                }
                strTicket = obj.ticket;
                return strTicket.Trim('"');
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(WxHelper), ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 调试用
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllWxCardCode()
        {
            var _vipWxCards = VipWxCardBLL.SingleModel.GetList();
            var codes = VipWxCardCodeBLL.SingleModel.GetList();
            return Json(new { isok = true, msg = "ok", vipWxCards = _vipWxCards, Codes = codes }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region 分享配置
        /// <summary>
        /// 分享配置
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="ShareType">0 专业版 1电商版,2表示多门店总店</param>
        /// <returns></returns>

        public ActionResult GetShare(string appId, int ShareType = 0)
        {
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            string strWhere = string.Empty;
            int itemtypeLogo = 0;
            int itemtypeADImg = 0;
            switch (ShareType)
            {
                case 0:
                case 2:
                    strWhere = $"aid={r.Id} and ShareType={ShareType}";
                    itemtypeLogo = (int)AttachmentItemType.小程序行业版分享店铺Logo;
                    itemtypeADImg = (int)AttachmentItemType.小程序行业版分享广告图;
                    break;
                case 1:
                    Store store = StoreBLL.SingleModel.GetModel($"appId={r.Id}");
                    if (store == null)
                        return Json(new { isok = false, msg = "店铺不存在！" });
                    if (store.appId != r.Id)
                        return Json(new { isok = false, msg = "没有权限" });

                    strWhere = $"aid={store.Id} and ShareType=1";
                    itemtypeLogo = (int)AttachmentItemType.小程序电商版分享店铺Logo;
                    itemtypeADImg = (int)AttachmentItemType.小程序电商版分享广告图;
                    break;
            }

            EntShare m = EntShareBLL.SingleModel.GetModel(strWhere);
            if (m == null)
                return Json(new { isok = false, msg = "未进行分享配置" }, JsonRequestBehavior.AllowGet);

            m.Logo = C_AttachmentBLL.SingleModel.GetListByCache(m.Id, itemtypeLogo);
            m.ADImg = C_AttachmentBLL.SingleModel.GetListByCache(m.Id, itemtypeADImg);

            //店铺Logo
            var LogoList = new List<object>();
            foreach (var attachment in m.Logo)
            {
                LogoList.Add(new { id = attachment.id, url = attachment.filepath });
            }

            //广告图
            var ADImgList = new List<object>();
            foreach (var attachment in m.ADImg)
            {
                ADImgList.Add(new { id = attachment.id, url = attachment.filepath });
            }

            return Json(new { isok = true, msg = "获取成功", obj = new { StoreName = m.StoreName, ADTitle = m.ADTitle, Qrcode = m.Qrcode, StyleType = m.StyleType, Logo = LogoList, ADImg = ADImgList, IsOpen = m.IsOpen, ShareType = m.ShareType } }, JsonRequestBehavior.AllowGet);

        }
        #endregion


        #region 积分商城相关

        /// <summary>
        /// 获取积分商城活动
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type"> 0→积分兑换 1→积分+微信支付</param>
        /// <returns></returns>
        public ActionResult GetExchangeActivityList(string appId, int pageIndex = 1, int pageSize = 10, int type = -1)
        {
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={r.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" }, JsonRequestBehavior.AllowGet);

            string nowTime = DateTime.Now.ToString();
            string strWhere = $"appId={r.Id} and isdel=0 and state=0  and apptype={xcxTemplate.Type} and startdate<='{nowTime}' and enddate>='{nowTime}'";
            if (type != -1)
            {
                strWhere += $" and exchangeway={type} ";
            }
            List<ExchangeActivity> list = ExchangeActivityBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", "SortNumber desc,Id desc");
            int TotalCount = ExchangeActivityBLL.SingleModel.GetCount(strWhere);
            list.ForEach(x =>
            {
                var activityimgList = C_AttachmentBLL.SingleModel.GetListByCache(x.id, (int)AttachmentItemType.小程序积分活动图片);
                if (activityimgList != null && activityimgList.Count > 0)
                {

                    x.activityimg = activityimgList[0].filepath;
                    x.activityimg_fmt = ImgHelper.ResizeImg(activityimgList[0].filepath, 750, 750);
                }
            });

            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, ExchangeActivityList = list } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取单个积分商城活动详情
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult GetExchangeActivity(string appId, int Id)
        {
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={r.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" }, JsonRequestBehavior.AllowGet);

            ExchangeActivity exchangeActivity = ExchangeActivityBLL.SingleModel.GetModel($"appId={r.Id} and Id={Id}  and isdel=0 and state=0");
            if (exchangeActivity == null)
                return Json(new { isok = false, msg = "数据不存在" }, JsonRequestBehavior.AllowGet);

            List<C_Attachment> activityimgList = C_AttachmentBLL.SingleModel.GetListByCache(exchangeActivity.id, (int)AttachmentItemType.小程序积分活动图片);
            if (activityimgList != null && activityimgList.Count > 0)
            {
                exchangeActivity.activityimg = activityimgList[0].filepath;
                exchangeActivity.activityimg_fmt = ImgHelper.ResizeImg(activityimgList[0].filepath, 750, 750);
            }

            List<C_Attachment> imgList = C_AttachmentBLL.SingleModel.GetListByCache(exchangeActivity.id, (int)AttachmentItemType.小程序积分活动轮播图);
            if (imgList != null && imgList.Count > 0)
            {
                imgList.ForEach(x =>
                {
                    exchangeActivity.imgs.Add(x.filepath);
                    exchangeActivity.imgs_fmt.Add(ImgHelper.ResizeImg(x.filepath, 750, 750));
                });
            }

            return Json(new { isok = true, msg = "获取成功", obj = exchangeActivity }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddExchangeActivityOrder(int userId, int activityId, string appId, string address)
        {
            if (string.IsNullOrEmpty(appId) || activityId <= 0 || userId <= 0 || string.IsNullOrEmpty(address))
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" }, JsonRequestBehavior.AllowGet);
            ExchangeActivity exchangeActivity = ExchangeActivityBLL.SingleModel.GetModel($"Id={activityId} and appId={r.Id} and state=0 and isdel=0");
            if (exchangeActivity == null)
                return Json(new { isok = false, msg = "兑换商品不存在" }, JsonRequestBehavior.AllowGet);
            if (exchangeActivity.stock == 0)
                return Json(new { isok = false, msg = "库存不足！" }, JsonRequestBehavior.AllowGet);
            int oldPayCount = ExchangeActivityOrderBLL.SingleModel.GetCount($"userId={userId} and appId={r.Id} and ActivityId={activityId} and state<>0");
            if (oldPayCount >= exchangeActivity.perexgcount)
                return Json(new { isok = false, msg = $"已超出兑换数量" }, JsonRequestBehavior.AllowGet);
            if (DateTime.Now > exchangeActivity.enddate)
                return Json(new { isok = false, msg = "兑换时间已结束" }, JsonRequestBehavior.AllowGet);
            if (DateTime.Now < exchangeActivity.startdate)
                return Json(new { isok = false, msg = "兑换时间未开始" }, JsonRequestBehavior.AllowGet);


            int way = Context.GetRequestInt("way",0);//配送方式

            int orderId = 0;
            lock (lockOrder)
            {
                List<C_Attachment> activityimgList = C_AttachmentBLL.SingleModel.GetListByCache(exchangeActivity.id, (int)AttachmentItemType.小程序积分活动图片);
                if (activityimgList != null && activityimgList.Count > 0)
                {
                    exchangeActivity.activityimg = activityimgList[0].filepath;
                }

                if (exchangeActivity.exchangeway == 0)
                {
                    //表示积分兑换  无需citymodersId  积分扣除成功则表示兑换成功
                    //.先生成订单,生成成功后拿到 订单Id 
                    //1.根据订单Id更新订单状态 2.扣除积分以及3.插入积分记录日志 放在事务里执行

                    orderId = ExchangeUserIntegralBLL.SingleModel.SubUserIntegral(exchangeActivity, userId, r.Id, address,way);
                    if (orderId == -1)
                        return Json(new { isok = false, msg = "积分不足" }, JsonRequestBehavior.AllowGet);
                    if (orderId == -2)
                        return Json(new { isok = false, msg = "生成订单失败" }, JsonRequestBehavior.AllowGet);
                    if (orderId == -3)
                        return Json(new { isok = false, msg = "系统异常" }, JsonRequestBehavior.AllowGet);

                    if (orderId > 0)
                    {
                        return Json(new { isok = true, code = 0, msg = "兑换成功！", obj = orderId }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { isok = false, msg = "兑换失败！", obj = orderId }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ExchangeUserIntegral exchangeUserIntegral = ExchangeUserIntegralBLL.SingleModel.GetModel($"userId={userId}");
                    if (exchangeUserIntegral == null || exchangeUserIntegral.integral < exchangeActivity.integral)
                        return Json(new { isok = false, msg = "积分不足" }, JsonRequestBehavior.AllowGet);

                    //积分+微信支付兑换   1.先生成订单 去微信支付  支付成功后再扣除积分并且写入积分变动日志 否则不扣积分
                    List<string> listSql = new List<string>();
                    ExchangeActivityOrder exchangeActivityOrder = new ExchangeActivityOrder
                    {
                        appId = r.Id,
                        UserId = userId,
                        ActivityId = exchangeActivity.id,
                        PayWay = 1,
                        BuyCount = 1,
                        address = address,
                        state = 0,
                        AddTime = DateTime.Now,
                        activityImg = exchangeActivity.activityimg,
                        activityName = exchangeActivity.activityname,
                        originalPrice = exchangeActivity.originalPrice,
                        Way=way
                    };

                    orderId = Convert.ToInt32(ExchangeActivityOrderBLL.SingleModel.Add(exchangeActivityOrder));
                    if (orderId <= 0)
                        return Json(new { isok = false, msg = "生成预订单失败" }, JsonRequestBehavior.AllowGet);

                    #region CtiyModer 生成
                    string no = WxPayApi.GenerateOutTradeNo();
                    int payMoney = exchangeActivity.price + exchangeActivity.freight;//支付总金额=价格+运费
                    CityMorders citymorderModel = new CityMorders
                    {
                        OrderType = (int)ArticleTypeEnum.MiniappExchangeActivity,
                        ActionType = (int)ArticleTypeEnum.MiniappExchangeActivity,
                        Addtime = DateTime.Now,
                        payment_free = payMoney,
                        trade_no = no,
                        Percent = 99,//不收取服务费
                        userip = WebHelper.GetIP(),
                        FuserId = userId,
                        Fusername = loginCUser.NickName,
                        orderno = no,
                        payment_status = 0,
                        Status = 0,
                        Articleid = exchangeActivity.id,
                        CommentId = orderId,
                        MinisnsId = r.Id,//店铺Id
                        TuserId = orderId,//订单的ID
                        ShowNote = $"小程序商品{payMoney * 0.01}元+{exchangeActivity.integral}积分兑换",
                        CitySubId = 0,//无分销,默认为0
                        PayRate = 1,
                        buy_num = 0, //无
                        appid = appId,
                    };

                    var CityMordersId = Convert.ToInt32(new CityMordersBLL().Add(citymorderModel));
                    if (CityMordersId <= 0)
                        return Json(new { isok = false, msg = "微信下单失败" }, JsonRequestBehavior.AllowGet);

                    //对外订单号规则：年月日时分 + 商品ID最后3位数字
                    var idStr = exchangeActivity.id.ToString();
                    if (idStr.Length >= 3)
                    {
                        idStr = idStr.Substring(idStr.Length - 3, 3);
                    }
                    else
                    {
                        idStr.PadLeft(3, '0');
                    }
                    idStr = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{idStr}";

                    #region 更新订单为待支付状态

                    exchangeActivityOrder.Id = orderId;
                    exchangeActivityOrder.state = 1;//表示已经生成支付订单
                    exchangeActivityOrder.OrderNum = idStr;
                    exchangeActivityOrder.CityMordersId = CityMordersId;
                    exchangeActivityOrder.BuyPrice = payMoney;
                    exchangeActivityOrder.integral = exchangeActivity.integral;

                    // 支付状态在 微信支付后回调进行更新  回调支付成功 则还需要加入一条积分变更记录以及减少库存

                    if (ExchangeActivityOrderBLL.SingleModel.Update(exchangeActivityOrder, "state,OrderNum,CityMordersId,BuyPrice,integral"))
                    {
                        return Json(new { isok = true, code = 1, msg = "ok！", obj = CityMordersId });
                    }
                    else
                    {
                        return Json(new { isok = false, msg = "未知错误" }, JsonRequestBehavior.AllowGet);
                    }

                    #endregion


                    #endregion
                }
            }
        }


        /// <summary>
        /// 获取店铺积分规则列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult GetStoreRules(string appId)
        {
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={r.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" }, JsonRequestBehavior.AllowGet);

            List<string> rules = new List<string>();
            List<ExchangeRule> listRule = ExchangeRuleBLL.SingleModel.GetList($"appId={r.Id} and state=0");

            listRule.ForEach(x =>
            {
                if (x.ruleType == 1 && !string.IsNullOrEmpty(x.goodids))
                {
                    rules.Add($"每单消费满{x.priceStr}元,即可获得{x.integral}积分,{x.goodCount}个商品参与");
                }
                else if (x.ruleType == 0)
                {
                    rules.Add($"每单消费满{x.priceStr}元,即可获得{x.integral}积分,全场商品参与");
                }
                else if (x.ruleType == 2)
                {
                    rules.Add($"每储值充钱满{x.priceStr}元,即可获得{x.integral}积分");
                }

            });
            return Json(new { isok = true, msg = "获取成功", obj = rules }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 根据用户以及appId获取积分订单列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetExchangeActivityOrders(int userId, string appId, int pageIndex = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" }, JsonRequestBehavior.AllowGet);

            string strWhere = $"appId={r.Id} and userId={userId} and state>1";

            List<ExchangeActivityOrder> list = ExchangeActivityOrderBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", " Id desc");
            list.ForEach(x =>
            {
                x.activityimg_fmt = ImgHelper.ResizeImg(x.activityImg, 750, 750);
            });

            int TotalCount = ExchangeActivityOrderBLL.SingleModel.GetCount(strWhere);
            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, listActivity = list } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取用户总积分
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult GetUserIntegral()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" }, JsonRequestBehavior.AllowGet);
            int TotalCount = 0;
            ExchangeUserIntegral model = ExchangeUserIntegralBLL.SingleModel.GetModel($"userId={userId}");
            if (model == null)
            {
                model = new ExchangeUserIntegral { UserId = userId, integral = 0, AddTime = DateTime.Now, UpdateDate = DateTime.Now };
                int id = Convert.ToInt32(ExchangeUserIntegralBLL.SingleModel.Add(model));
                if (id <= 0)
                    return Json(new { isok = false, msg = "初始化失败！" }, JsonRequestBehavior.AllowGet);
                model.Id = id;
            }
            TotalCount = ExchangeActivityOrderBLL.SingleModel.GetCount($"appId={r.Id} and userId={userId} and state=2");
            return Json(new { isok = true, msg = "获取成功！", obj = model, sendCount = TotalCount }, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 获取用户积分变动记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetUserIntegralLogs(int userId, string appId, int pageIndex = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" }, JsonRequestBehavior.AllowGet);

            string strWhere = $"appId={r.Id} and userId={userId}";
            List<ExchangeUserIntegralLog> listLog = ExchangeUserIntegralLogBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", " Id desc");
            int TotalCount = ExchangeUserIntegralLogBLL.SingleModel.GetCount(strWhere);
            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, listLog = listLog } }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 积分物品确认收货
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult ConfirmReciveGood(int userId, string appId, int orderId)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0 || orderId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" }, JsonRequestBehavior.AllowGet);

            ExchangeActivityOrder activityOrder = ExchangeActivityOrderBLL.SingleModel.GetModel($"userId={userId} and appId={r.Id} and Id={orderId}");
            if (activityOrder == null)
                return Json(new { isok = false, msg = "数据不存在！" }, JsonRequestBehavior.AllowGet);
            activityOrder.state = 4;
            if (ExchangeActivityOrderBLL.SingleModel.Update(activityOrder, "state"))
                return Json(new { isok = true, msg = "操作成功！" }, JsonRequestBehavior.AllowGet);
            return Json(new { isok = false, msg = "操作失败！" }, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 获取我的签到信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserPlayCard()
        {
            returnObj = new Return_Msg_APP();
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            int userId = Utility.IO.Context.GetRequestInt("userId", 0);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int resultCode = 200;
            ExchangePlayCardRelation exchangePlayCardRelation = ExchangePlayCardRelationBLL.SingleModel.GetExchangePlayCardRelation(userId, r.Id, out resultCode);
            if (resultCode == 500)
            {
                returnObj.Msg = "获取签到信息异常";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            returnObj.dataObj = exchangePlayCardRelation;
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 打卡签到领积分
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PlayCard()
        {
            returnObj = new Return_Msg_APP();
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            int userId = Utility.IO.Context.GetRequestInt("userId", 0);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }

            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }

            int resultCode = 200;

            ExchangePlayCardRelation exchangePlayCardRelation = ExchangePlayCardRelationBLL.SingleModel.PlayCard(userId, r.Id, out resultCode);
            if (resultCode == 500)
            {
                returnObj.Msg = "店铺配置信息没有找到";
                return Json(returnObj);
            }

            if (resultCode == 403)
            {
                returnObj.Msg = "今天已签到";
                return Json(returnObj);
            }

            if (resultCode == 404)
            {
                returnObj.Msg = "签到配置信息没有找到";
                return Json(returnObj);
            }
            if (resultCode == 302)
            {
                returnObj.Msg = "签到开关关闭";
                return Json(returnObj);
            }
            if (resultCode == 502)
            {
                returnObj.Msg = "签到天数错误";
                return Json(returnObj);
            }
            if (resultCode == 503)
            {
                returnObj.Msg = "签到送积分计算错误";
                return Json(returnObj);
            }
            if (resultCode == -1)
            {
                returnObj.Msg = "签到异常";
                return Json(returnObj);
            }
            returnObj.dataObj = exchangePlayCardRelation;
            returnObj.isok = true;
            returnObj.Msg = "签到成功";
            return Json(returnObj);

        }

        #endregion


        #region 分销相关(营销插件)


        /// <summary>
        /// 获取小程序分销配置以及当前用户是否已经成为分销员
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public ActionResult GetMiniAppSaleManConfig(string appId, int userId)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            SalesManConfig salesManConfig = SalesManConfigBLL.SingleModel.GetModel($"appId={r.Id}");
            if (salesManConfig == null || string.IsNullOrEmpty(salesManConfig.configStr))
                return Json(new { isok = false, msg = "还未进行分销设置" }, JsonRequestBehavior.AllowGet);
            if (salesManConfig.state == 0)
                return Json(new { isok = false, msg = "分销已经关闭" }, JsonRequestBehavior.AllowGet);

            ConfigModel configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);//获取分销配置信息

            SalesMan salesMan = SalesManBLL.SingleModel.GetModel($"UserId={userId} and appId={r.Id}");

            int exp_time_day = configModel.salesManManager.exp_time_day;

            if (configModel.salesManManager.exp_time_type == 0)
            {
                exp_time_day = 15;
            }
            if (configModel.salesManManager.exp_time_type == -1)
            {
                exp_time_day = 1000000;
            }

            //测试改为时间,上线后改为天
            if (salesManConfig.UpdateTime.AddDays(exp_time_day) < DateTime.Now)
            {
                configModel.salesManManager.allow_recruit = 0;
            }

            #region 根据资料判断分销招募页面的显示,如果链接带分销员Id则查询当前二级分销设置以及上级分销员情况来进行显示对应的分销员招募页面
            SalesMan parentSalesMan = new SalesMan();
            bool isSecondSale = false;//当前进入分销招募页面的用户 是否能成为某个分销员的下级
            int secondeSaleState = 0;//默认为关闭状态 后台二级分销开关状态
            int parentSalesManId = Utility.IO.Context.GetRequestInt("parentSalesManId", 0);//大于0则表示来自上级分销员推广的
            if (parentSalesManId > 0 && configModel.secondSalesManConfig.State == 1)
            {
                parentSalesMan = SalesManBLL.SingleModel.GetModel(parentSalesManId);
                if (parentSalesMan != null)
                {
                    C_UserInfo c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(parentSalesMan.UserId);
                    if (c_UserInfo != null)
                    {
                        parentSalesMan.nickName = c_UserInfo.NickName;
                        parentSalesMan.headerImg = c_UserInfo.HeadImgUrl;
                    }
                    if (parentSalesMan.state == 2)
                    {
                        isSecondSale = true;
                    }
                }
            }
            #endregion

            secondeSaleState = configModel.secondSalesManConfig.State;

            if (configModel.recruitPlan != null)
            {
                configModel.recruitPlan.description = HttpUtility.HtmlDecode(configModel.recruitPlan.description);
            }

            return Json(new { isok = true, msg = "获取成功", obj = new { RecruitPlan = configModel.recruitPlan, SalesManManager = configModel.salesManManager, SalesMan = salesMan, SalesManConfigUpdateTime = salesManConfig.UpdateTime, ParentSalesMan = parentSalesMan, IsSecondSale = isSecondSale, secondeSaleState = secondeSaleState } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 申请成为分销员
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ApplySalesman(string appId, int userId, string TelePhone)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0 || string.IsNullOrEmpty(TelePhone))
                return Json(new { isok = false, msg = "参数错误!" });
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" });
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" });
            TelePhone = TelePhone.Trim();
            //校验手机号码  
            if (!Regex.IsMatch(TelePhone, @"^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1})|(17[0-9]{1}))+\d{8})$"))
                return Json(new { isok = false, msg = "手机号码不符合", obj = TelePhone });

            SalesMan salesMan = SalesManBLL.SingleModel.GetModel($"userId={userId} and appId={r.Id}");
            if (salesMan != null)
                return Json(new { isok = false, msg = "已进行申请了" });
            SalesManConfig salesManConfig = SalesManConfigBLL.SingleModel.GetModel($"appId={r.Id}");
            if (salesManConfig == null || string.IsNullOrEmpty(salesManConfig.configStr))
                return Json(new { isok = false, msg = "还未进行分销设置" });
            if (salesManConfig.state == 0)
                return Json(new { isok = false, msg = "分销已经关闭" });

            ConfigModel configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);
            if (configModel == null)
                return Json(new { isok = false, msg = "申请异常(配置错误)" });
            if (configModel.salesManManager.allow_recruit == 0)
                return Json(new { isok = false, msg = "招募已关闭" });

            salesMan = new SalesMan();

            if (configModel.salesManManager.is_verify_on == 0)
            {
                //表示不需要审核
                salesMan.state = 2;
            }
            else if (configModel.salesManManager.is_verify_on == 1)
            {
                //表示需要人工审核
                salesMan.state = 0;
            }
            else if (configModel.salesManManager.is_verify_on == 2)
            {
                //累计消费满指定金额就会自动通过

                int costSum = VipRelationBLL.SingleModel.GetEntGoodsVipPriceSum(userId);//累计消费 分为单位
                if (costSum * 0.01 < configModel.salesManManager.CostMoney)
                {
                    return Json(new { isok = false, msg = $"申请失败(需累计消费满{configModel.salesManManager.CostMoney}元)" });
                }
                salesMan.state = 2;

            }
            else if (configModel.salesManManager.is_verify_on == 3)
            {
                //累计充值满指定金额就会自动通过
                int saveMoney = 0;
                SaveMoneySetUser m = SaveMoneySetUserBLL.SingleModel.getModelByUserId(userId);
                if (m != null)
                {
                    saveMoney = SaveMoneySetUserBLL.SingleModel.GetSaveMoneySum(userId);
                }
                if (saveMoney * 0.01 < configModel.salesManManager.CostMoney)
                {
                    return Json(new { isok = false, msg = $"申请失败(需累计充值满{configModel.salesManManager.CostMoney}元)" });
                }

                salesMan.state = 2;
            }
            else if (configModel.salesManManager.is_verify_on == 4)
            {
                //达到设定标准后自动通过
                int saveMoney = 0;
                SaveMoneySetUser m = SaveMoneySetUserBLL.SingleModel.getModelByUserId(userId);
                if (m != null)
                {
                    saveMoney = SaveMoneySetUserBLL.SingleModel.GetSaveMoneySum(userId);
                }
                int costSum = VipRelationBLL.SingleModel.GetEntGoodsVipPriceSum(userId);//累计消费 分为单位

                if (configModel.salesManManager.Cost_verify_on && configModel.salesManManager.SaveMoney_verify_on)
                {
                    if ((saveMoney * 0.01 < configModel.salesManManager.SaveMoney)||(costSum * 0.01 < configModel.salesManManager.CostMoney))
                    {
                        return Json(new { isok = false, msg = $"申请失败(需累计充值满{configModel.salesManManager.SaveMoney}元,并且累计消费满{configModel.salesManManager.CostMoney}元)" });
                    }
                }

                if (configModel.salesManManager.Cost_verify_on)
                {
                    if (costSum * 0.01 < configModel.salesManManager.CostMoney)
                    {
                        return Json(new { isok = false, msg = $"申请失败(需累计消费满{configModel.salesManManager.CostMoney}元)" });
                    }
                }
                if (configModel.salesManManager.SaveMoney_verify_on)
                {
                    if (saveMoney * 0.01 < configModel.salesManManager.SaveMoney)
                    {
                        return Json(new { isok = false, msg = $"申请失败(需累计充值满{configModel.salesManManager.SaveMoney}元)" });
                    }
                }


                salesMan.state = 2;
            }

            int parentSalesManId = Utility.IO.Context.GetRequestInt("parentSalesManId", 0);//大于0则表示来自上级分销员推广的
            if (parentSalesManId > 0 && configModel.secondSalesManConfig.State == 0)
            {
                //关闭了二级分销
                parentSalesManId = 0;
            }

            SalesMan parentSalesMan = SalesManBLL.SingleModel.GetModel($"Id={parentSalesManId} and state<>-1");
            if (parentSalesMan == null)
            {
                //上级分销被清退了
                parentSalesManId = 0;
            }

            salesMan.AddTime = DateTime.Now;
            salesMan.UpdateTime = DateTime.Now;
            salesMan.TelePhone = TelePhone;
            salesMan.UserId = userId;
            salesMan.appId = r.Id;
            salesMan.ParentSalesmanId = parentSalesManId;

            int id = Convert.ToInt32(SalesManBLL.SingleModel.Add(salesMan));
            if (id > 0)
                return Json(new { isok = true, msg = "申请成功", obj = id });
            return Json(new { isok = false, msg = "申请失败", obj = id });

        }

        /// <summary>
        /// 获取推广分享记录Id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="salesManId"></param>
        /// <returns></returns>

        public ActionResult GetSalesManRecord(string appId, int salesManId, int goodsId)
        {
            if (string.IsNullOrEmpty(appId) || salesManId <= 0 || goodsId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            SalesManConfig salesManConfig = SalesManConfigBLL.SingleModel.GetModel($"appId={r.Id}");
            if (salesManConfig == null || string.IsNullOrEmpty(salesManConfig.configStr))
                return Json(new { isok = false, msg = "还未进行分销设置" }, JsonRequestBehavior.AllowGet);
            if (salesManConfig.state == 0)
                return Json(new { isok = false, msg = "分销已经关闭" }, JsonRequestBehavior.AllowGet);

            SalesMan salesMan = SalesManBLL.SingleModel.GetModel($"Id={salesManId} and state=2 and appId={r.Id}");
            if (salesMan == null)
                return Json(new { isok = false, msg = "分销员未审核通过!" }, JsonRequestBehavior.AllowGet);

            EntGoods entGoods = EntGoodsBLL.SingleModel.GetModel(goodsId);
            if (entGoods == null)
                return Json(new { isok = false, msg = "找不到该分销产品!" }, JsonRequestBehavior.AllowGet);


            ConfigModel configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);

            int salesManRecordId = Convert.ToInt32(salesManRecordBLL.Add(new SalesManRecord()
            {
                appId = r.Id,
                salesManId = salesManId,
                configStr = salesManConfig.configStr,
                salesmanGoodsId = goodsId,
                state = 0,//分享成功后再改变该状态变为1 可用
                addTime = DateTime.Now,
                cps_rate = (entGoods.isDefaultCps_Rate == 0 ? configModel.payMentManager.cps_rate : entGoods.cps_rate)
            }));
            if (salesManRecordId <= 0)
                return Json(new { isok = false, msg = "error!" }, JsonRequestBehavior.AllowGet);

            return Json(new { isok = true, msg = "ok", obj = salesManRecordId }, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 更新推广分享记录状态 默认更新为可用 state=1
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="salesManRecordId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateSalesManRecord(string appId, int salesManRecordId, int state = 1)
        {
            if (string.IsNullOrEmpty(appId) || salesManRecordId <= 0)
                return Json(new { isok = false, msg = "参数错误!" });

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" });

            SalesManRecord salesManRecord = salesManRecordBLL.GetModel(salesManRecordId);
            if (salesManRecord == null)
                return Json(new { isok = false, msg = "推广记录不存在" });
            salesManRecord.state = state;
            if (!salesManRecordBLL.Update(salesManRecord, "state"))
                return Json(new { isok = false, msg = "更新失败分享无效" });

            return Json(new { isok = true, msg = "更新成功分享有效" });

        }


        /// <summary>
        ///  绑定客户-分销员之间的关系
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="salesManRecordId"></param>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        public ActionResult BindRelationShip(string appId, int userId, int salesManRecordId, int goodsId = 0)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0 || salesManRecordId <= 0)
                return Json(new { isok = false, msg = "参数错误!" });

            var r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" });

            SalesManRecord salesManRecord = salesManRecordBLL.GetModel($"Id={salesManRecordId} and state=1 and appId={r.Id}");
            if (salesManRecord == null)
                return Json(new { isok = false, msg = "有效推广记录不存在" });

            // SalesManRecordUser salesManRecordUser = _salesManRecordUserBLL.GetModel($"userId={userId} and goodsId={goodsId} and appId={r.Id}");
            SalesManRecordUser salesManRecordUser = SalesManRecordUserBLL.SingleModel.GetModel($"userId={userId}  and appId={r.Id} and salesmanId={salesManRecord.salesManId}");
           // log4net.LogHelper.WriteInfo(this.GetType(), $"userId={userId}  and appId={r.Id} and salesmanId={salesManRecord.salesManId}");
            ConfigModel configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManRecord.configStr);//获取分享推广时候的分销配置
            SalesMan salesMan3 = SalesManBLL.SingleModel.GetModel($"UserId={userId} and state=2");
            if (salesMan3 != null && salesMan3.Id == salesManRecord.salesManId)
            {
                return Json(new { isok = true, msg = "自己分享的自己点击不用绑定", obj = 0 });
            }

            //TODO 测试使用分钟MINUTE 线上使用天Day 分销
            // SalesManRecordUser  model = _salesManRecordUserBLL.GetModel($" DATE_ADD(UpdateTime,INTERVAL protected_time Day)>now() and userId={userId} and goodsId={goodsId} and appId={r.Id}");//判断 用户-是否在分销员保护期内
            SalesManRecordUser model = SalesManRecordUserBLL.SingleModel.GetModel($" DATE_ADD(UpdateTime,INTERVAL protected_time Day)>now() and userId={userId} and appId={r.Id}");//判断 用户-是否在分销员保护期内
            bool is_protect_seller = configModel.salesManManager.is_protect_seller;//是否开启了分销员保护期

            if (model == null)
            {
                //表示用户-产品 不在任何一个分销员保护期内或者当前设置无保护期限制
                if (salesManRecordUser == null)
                {
                    if (configModel.salesManManager.allow_sellers_related == 0)
                    {
                        //当前设置为 分销员之间不能建立客户关系 判断当前用户是否是分销员
                        SalesMan curSalesMan = SalesManBLL.SingleModel.GetModel($"UserId={userId} and state=2");
                        if (curSalesMan != null && salesManRecord.salesManId != curSalesMan.Id)
                        {
                            //表示当前购买用户是分销员
                            return Json(new { isok = true, msg = "当前设置不允许分销员之间建立关系,当前点击的用户是分销员不用建立关系", obj = 0 });
                        }
                    }

                    salesManRecordUser = new SalesManRecordUser()
                    {
                        appId = r.Id,
                        salesManId = salesManRecord.salesManId,
                        recordId = salesManRecord.Id,
                        goodsId = goodsId,
                        userId = userId,
                        UpdateTime = DateTime.Now,
                        protected_time = is_protect_seller ? configModel.salesManManager.protected_time : 0,
                        addTime = DateTime.Now
                    };
                    int id = Convert.ToInt32(SalesManRecordUserBLL.SingleModel.Add(salesManRecordUser));
                    if (id <= 0)
                    {
                        return Json(new { isok = false, msg = "新增失败1", obj = id });
                    }
                    else
                    {
                        return Json(new { isok = true, msg = "新增成功1", obj = id });
                    }
                }
                else
                {
                    //用户-产品之前存在过绑定关系,需要更新关系保护期以及推广记录Id跟所属分销员Id
                    salesManRecordUser.salesManId = salesManRecord.salesManId;
                    salesManRecordUser.recordId = salesManRecord.Id;
                    salesManRecordUser.UpdateTime = DateTime.Now;
                    salesManRecordUser.protected_time = is_protect_seller ? configModel.salesManManager.protected_time : 0;
                    if (!SalesManRecordUserBLL.SingleModel.Update(salesManRecordUser, "UpdateTime,protected_time,recordId,salesManId"))
                    {
                        return Json(new { isok = false, msg = "绑定关系更新失败", obj = salesManRecordUser.Id });
                    }
                    else
                    {
                        return Json(new { isok = true, msg = "绑定关系更新成功", obj = salesManRecordUser.Id });
                    }
                }
            }
            else
            {
                return Json(new { isok = true, msg = "目前属于某分销员", obj = model.Id });
            }
        }

        /// <summary>
        /// 获取分销产品列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="goodsName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetSalesmanGoodsList(string appId, string goodsName = "", int sortType = 0, int pageIndex = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "appId非法" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={r.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" }, JsonRequestBehavior.AllowGet);


            SalesManConfig salesManConfig = SalesManConfigBLL.SingleModel.GetModel($"appId={r.Id}");
            if (salesManConfig == null || string.IsNullOrEmpty(salesManConfig.configStr))
                return Json(new { isok = false, msg = "还未进行分销设置" }, JsonRequestBehavior.AllowGet);
            if (salesManConfig.state == 0)
                return Json(new { isok = false, msg = "分销已经关闭" }, JsonRequestBehavior.AllowGet);

            ConfigModel configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);//获取分销配置信息


            string sortWhere = " cps_rate desc";//默认 佣金比例降序排序显示
            if (sortType == 1)
            {
                //表示按照分销更新时间降序排序
                sortWhere = " distributionTime desc,Id desc";
            }
            if (sortType == 2)
            {
                //表示按照分销商品价格升序排序
                sortWhere = " price asc,Id desc";
            }


            List<SalesmanGoods> list = SalesManConfigBLL.SingleModel.GetListSalesmanGoods(r.Id, xcxTemplate.Type, goodsName, 1, pageIndex, pageSize, sortWhere);
            int TotalCount = SalesManConfigBLL.SingleModel.GetSalesmanGoodsCount(r.Id, xcxTemplate.Type, goodsName, 1);
            return Json(new { isok = true, msg = "成功", obj = new { RecordCount = TotalCount, SalesmanGoodsList = list, showType = configModel.pageShowWay.is_show_cps_type } }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取分销推广订单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetSalesManRecordOrder(string appId, int userId, int pageIndex = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            string strWhere = $"m.appId={r.Id} and o.UserId={userId} and r.state=1";
            List<EntGoodsCart> goodOrderDtl = new List<EntGoodsCart>();
            EntGoods goods = new EntGoods();
            List<SalesManRecordOrder> listSalesManRecordOrder = SalesManRecordOrderBLL.SingleModel.GetListSalesManRecordOrder(strWhere, pageIndex, pageSize, "Id desc");
            int TotalCount = SalesManRecordOrderBLL.SingleModel.GetSalesManRecordOrderCount(strWhere);
            if (listSalesManRecordOrder != null)
            {
                string orderIds = string.Join(",",listSalesManRecordOrder.Select(s=>s.orderId));
                List<EntGoodsCart> entGoodsCartList = EntGoodsCartBLL.SingleModel.GetListByOrderIds(orderIds);

                string goodsIds = string.Join(",",entGoodsCartList?.Select(s=>s.FoodGoodsId).Distinct());
                List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);

                listSalesManRecordOrder.ForEach(x =>
                {
                    goodOrderDtl = entGoodsCartList?.Where(w=>w.GoodsOrderId == x.orderId).ToList();
                    if (goodOrderDtl != null)
                    {
                        goodOrderDtl.ForEach(item =>
                        {
                            goods = entGoodsList?.FirstOrDefault(f => f.id == item.FoodGoodsId);

                            x.listOrderGoodsDetail.Add(new OrderGoodsDetail
                            {
                                price = Convert.ToDouble((item.Price * 0.01).ToString("0.00")),
                                Count = item.Count,
                                goodImgUrl = goods?.img,
                                goodname = goods?.name,
                                cpsMoney = (int)Math.Ceiling(item.Price * item.Count * item.cps_rate * 0.01),
                                cps_rate = item.cps_rate.ToString("0.00")
                            });
                        });
                    }
                });
            }

            return Json(new { isok = true, msg = "成功", obj = new { RecordCount = TotalCount, List = listSalesManRecordOrder } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取累计客户
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult GetSalesManRecordUser(string appId, int userId, int pageIndex = 1, int pageSize = 10, int state = 0)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);

            var r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            SalesMan salesMan = SalesManBLL.SingleModel.GetModel($"UserId={userId} and appId={r.Id}");
            if (salesMan == null)
                return Json(new { isok = false, msg = "请先申请成为分销员" }, JsonRequestBehavior.AllowGet);

            List<SalesManRecordUser> list = SalesManRecordUserBLL.SingleModel.GetListSalesManRecordUser(r.Id, salesMan.Id, userId, state, pageIndex, pageSize);
            int TotalCount = SalesManRecordUserBLL.SingleModel.GetSalesManRecordUserCount(r.Id, salesMan.Id, userId, state);
            return Json(new { isok = true, msg = "成功", obj = new { RecordCount = TotalCount, SalesManRecordUserList = list } }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取分销员相关信息 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSalesManUserInfo(string appId, int userId)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            var loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" }, JsonRequestBehavior.AllowGet);
            SalesMan salesMan = SalesManBLL.SingleModel.GetModel($"userId={userId} and appId={r.Id}");
            if (salesMan == null)
                return Json(new { isok = false, msg = "还不是分销员" }, JsonRequestBehavior.AllowGet);

            salesMan.nickName = loginCUser.NickName;
            salesMan.headerImg = loginCUser.HeadImgUrl;
            OpenAuthorizerConfig XUserList = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(appId);
            if (XUserList != null)
            {
                salesMan.storeName = XUserList.nick_name;
            }
            salesMan.salesManOrderCount = SalesManBLL.SingleModel.GetListSalesManOrderCount($"appId={r.Id} and salesmanId={salesMan.Id}");
            salesMan.customerNumber = SalesManRecordUserBLL.SingleModel.GetSalesManRecordUserCount(r.Id, salesMan.Id, userId);
            return Json(new { isok = true, msg = "获取成功", obj = salesMan }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 提现申请
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="drawCashMoney"></param>
        /// <returns></returns>
        public ActionResult DrawCashApply(string appId, int userId, double drawCashMoney)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0)
                return Json(new { isok = false, msg = "参数错误!" });
            if (drawCashMoney * 100 < 100)
                return Json(new { isok = false, msg = "单笔提现金额不能低于1元!" });
            if (drawCashMoney * 100 > 20000 * 100)
                return Json(new { isok = false, msg = "单笔提现金额不能大于20000元!" });
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" });
            var loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" });

            string partner_trade_no = WxPayApi.GenerateOutTradeNo();
            int resultCode = DrawCashApplyBLL.SingleModel.AddDistributionDrawCashApply(r.Id, userId, Convert.ToInt32(drawCashMoney * 100), partner_trade_no);
            return Json(new { isok = true, msg = "请求成功!", obj = resultCode });
        }

        /// <summary>
        /// 获取提现记录 根据分销员
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDrawCashApplyList(string appId, int userId, int pageSize = 10, int pageIndex = 1)
        {
            if (string.IsNullOrEmpty(appId) || userId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" }, JsonRequestBehavior.AllowGet);

            string strWhere = $"aid={r.Id} and userId={userId}";
            List<DrawCashApply> listDrawCashApply = DrawCashApplyBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", "addTime desc");
            int totalCount = DrawCashApplyBLL.SingleModel.GetCount(strWhere);

            return Json(new { isok = true, msg = "获取成功！", obj = new { totalCount = totalCount, list = listDrawCashApply } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取我的下级分销员
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="saleManId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult GetSaleManRelationList(string appId = "", int saleManId = 0, int pageSize = 10, int pageIndex = 1)
        {
            if (string.IsNullOrEmpty(appId) || saleManId <= 0)
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            int totalCount = 0;
            List<ViewSalesManRelation> listSalesManRelation = SalesManRelationBLL.SingleModel.GetListBySaleManId(saleManId, out totalCount, pageIndex, pageSize);

            int totalSecondCpsPrice = 0;
            if (totalCount > 0)
            {
                totalSecondCpsPrice = SalesManRelationBLL.SingleModel.GetTotalSecondCpsPrice(saleManId);
            }

            return Json(new { isok = true, msg = "获取成功！", obj = new { totalCount = totalCount, list = listSalesManRelation, totalSecondCpsPrice = (totalSecondCpsPrice * 0.01).ToString("0.00") } }, JsonRequestBehavior.AllowGet);
        }



        #endregion

        /// <summary>
        /// 获取进入产品详情小程序码
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetProductQrcode()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            int pid = Utility.IO.Context.GetRequestInt("pid", 0);
            int productType = Utility.IO.Context.GetRequestInt("productType", 0);//0表示普通 1拼团

            int recordId = Utility.IO.Context.GetRequestInt("recordId", 0); //是否是分销产品
            int saleManId = Utility.IO.Context.GetRequestInt("saleManId", 0); //分销员Id
            int flashItemId = Utility.IO.Context.GetRequestInt("flashItemId", 0); //秒杀商品ID
            if (string.IsNullOrEmpty(appId))
            {
                result.code = "200";
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            var r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                result.code = "200";
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={r.TId}");
            if (xcxTemplate == null)
            {
                result.code = "200";
                result.Msg = "未找到小程序模板";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            //OpenAuthorizerInfo openconfig = _openAuthorizerInfoBLL.GetModelByAppId(appId);
            //if (openconfig == null || string.IsNullOrEmpty(openconfig.user_name))
            //{
            //    result.code = "200";
            //    result.Msg = "OpenAuthorizerInfo没有找到授权信息";
            //    return Json(result, JsonRequestBehavior.AllowGet);
            //}

            //string url = XcxApiBLL.SingleModel.GetOpenAuthodModel(openconfig.user_name);
            //string accessTokenMsgResult = WxHelper.HttpGet(url);
            //if (string.IsNullOrEmpty(accessTokenMsgResult))
            //{
            //    result.code = "200";
            //    result.Msg = "accessTokenMsgResult为NULL";
            //    return Json(result, JsonRequestBehavior.AllowGet);
            //}

            //GetAccessTokenMsg accessTokenMsg = JsonConvert.DeserializeObject<GetAccessTokenMsg>(accessTokenMsgResult);
            //if (accessTokenMsg == null || accessTokenMsg.obj == null)
            //{
            //    result.code = "200";
            //    result.Msg = "授权失败accessTokenMsg异常";
            //    result.dataObj = new { accessTokenMsgResult = accessTokenMsgResult };
            //    return Json(result, JsonRequestBehavior.AllowGet);

            //}



            string access_token = string.Empty;//accessTokenMsg.obj.access_token;
            if (!XcxApiBLL.SingleModel.GetToken(r, ref access_token))
            {
                result.code = "200";
                result.Msg = "获取token异常:"+ access_token;
                return Json(result, JsonRequestBehavior.AllowGet);
              
            }


            string scene = string.Empty;
            string pagePath = string.Empty;
            string postData = string.Empty;
            string qrCode = string.Empty;

            string errorMessage = "";
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序专业模板:

                    if (productType == (int)EntGoodsType.普通产品)
                    {
                        int applySale = Context.GetRequestInt("applySale", 0);//默认0表示不是招募下级分销
                        if (applySale > 0)
                        {
                            scene = $"{saleManId}_{applySale > 0}";
                            pagePath = "pages/sellCenter/sell";
                            postData = Newtonsoft.Json.JsonConvert.SerializeObject(new { scene = scene, page = pagePath, width = 200, auto_color = true, line_color = new { r = "0", g = "0", b = "0" } });
                            
                            qrCode = CommondHelper.HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token, postData,ref errorMessage);

                        }
                        else
                        {

                            int storeSale = Context.GetRequestInt("storeSale", 0);//默认0表示不是分销整店推广
                            if (storeSale > 0)
                            {
                                scene = $"{recordId}_{saleManId}";
                                pagePath = "pages/sellCenter/sellProLst";
                                postData = Newtonsoft.Json.JsonConvert.SerializeObject(new { scene = scene, page = pagePath, width = 200, auto_color = true, line_color = new { r = "0", g = "0", b = "0" } });
                                qrCode =CommondHelper.HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token, postData,ref errorMessage);

                            }
                            else
                            {
                                string typename = Utility.IO.Context.GetRequest("typename", string.Empty);
                                int showprice = Utility.IO.Context.GetRequestInt("showprice", 0);//默认0
                                if (pid <= 0 && string.IsNullOrEmpty(typename))
                                {
                                    result.code = "200";
                                    result.Msg = "参数错误";
                                    return Json(result, JsonRequestBehavior.AllowGet);
                                }

                                EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(pid);
                                if (goodModel == null || goodModel.state == 0)
                                {
                                    result.code = "200";
                                    result.Msg = "产品不存在或者已经删除";
                                    result.dataObj = new { access_token = access_token };
                                    return Json(result, JsonRequestBehavior.AllowGet);
                                }
                                scene = $"{pid}_{recordId}_{typename}_{showprice}_{saleManId}";
                                pagePath = "pages/good/good";
                                postData = Newtonsoft.Json.JsonConvert.SerializeObject(new { scene = scene, page = pagePath, width = 200, auto_color = true, line_color = new { r = "0", g = "0", b = "0" } });
                                qrCode = CommondHelper.HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token, postData,ref errorMessage);

                            }

                        }

                    }

                    if (productType == (int)EntGoodsType.拼团产品)
                    {
                        if (pid <= 0)
                        {
                            result.code = "200";
                            result.Msg = "参数错误222";
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }

                        EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(pid);
                        if (goodModel == null || goodModel.state == 0)
                        {
                            result.code = "200";
                            result.Msg = "产品不存在或者已经删除";
                            result.dataObj = new { access_token = access_token };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }

                        pagePath = $"pages/group2/group2?id={pid}";
                        postData = Newtonsoft.Json.JsonConvert.SerializeObject(new { path = pagePath, width = 200 });
                        qrCode = CommondHelper.HttpPostSaveImg("https://api.weixin.qq.com/cgi-bin/wxaapp/createwxaqrcode?access_token=" + access_token, postData,ref errorMessage);
                    }

                    if (flashItemId > 0 && productType == (int)EntGoodsType.秒杀商品)
                    {
                        if (flashItemId <= 0)
                        {
                            result.code = "200";
                            result.Msg = "参数错误_flashItemId";
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }

                        FlashDealItem flashItem = FlashDealItemBLL.SingleModel.GetModel(flashItemId);
                        if (flashItem == null)
                        {
                            result.code = "200";
                            result.Msg = "产品不存在或者已经删除";
                            result.dataObj = new { access_token = access_token };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }

                        pagePath = $"pages/miaoSha/detail?id={flashItem.Id}";
                        postData = Newtonsoft.Json.JsonConvert.SerializeObject(new { path = pagePath, width = 200 });
                        qrCode = CommondHelper.HttpPostSaveImg("https://api.weixin.qq.com/cgi-bin/wxaapp/createwxaqrcode?access_token=" + access_token, postData,ref errorMessage);
                    }
                    break;
            }

            result.code = "200";
            result.isok = !string.IsNullOrEmpty(qrCode);
            result.Msg = result.isok ? "获取成功" : $"获取失败!{errorMessage}";

            result.dataObj = new { qrCode = qrCode };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取功能列表清单
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetFunctionList()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            result.code = "200";
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            XcxAppAccountRelation xcx = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcx == null)
            {
                result.isok = false;
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                result.isok = false;
                result.Msg = "找不到模板";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if ((int)TmpType.小程序专业模板 == xcxTemplate.Type)
            {
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    result.isok = false;
                    result.Msg = "此功能未开启";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                ComsConfig ComsConfigModel = JsonConvert.DeserializeObject<ComsConfig>(functionList.ComsConfig);
                StoreConfig StoreConfigModel = JsonConvert.DeserializeObject<StoreConfig>(functionList.StoreConfig);
                MarketingPlugin MarketingPluginModel = JsonConvert.DeserializeObject<MarketingPlugin>(functionList.MarketingPlugin);
                OperationMgr OperationMgrModel = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                FuncMgr FuncMgrModel = JsonConvert.DeserializeObject<FuncMgr>(functionList.FuncMgr);
                ProductMgr productMgr = JsonConvert.DeserializeObject<ProductMgr>(functionList.ProductMgr);
                Store store = StoreBLL.SingleModel.GetModelByAId(xcx.Id);
                store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson) ?? new StoreConfigModel();

                List<object> list = new List<object>();
                if (OperationMgrModel.Distribution == 0 && store.funJoinModel.isopen_fenxiao)
                {
                    list.Add(new
                    {
                        title = "分销中心",
                        icon = "dzicon icon-fenxiaozhongxin",
                        url = "/pages/sellCenter/sell",
                        c1 = "#94D972",
                        c2 = "#6AD9B4",
                    });
                }
                if (store.funJoinModel.isopen_car)
                {
                    list.Add(new
                    {
                        title = "我的购物车",
                        icon = "dzicon icon-wodegouwuche",
                        url = "/pages/good/goodShopCar",
                        c1 = "#F76161",
                        c2 = "#FF978F"
                    });
                }

                if (ComsConfigModel.Joingroup == 0 && store.funJoinModel.isopen_tuangou)
                {
                    list.Add(new
                    {
                        title = "我的团购",
                        icon = "dzicon icon-wodetuangou",
                        url = "/pages/group/groupList",
                        c1 = "#FFE63E",
                        c2 = "#F2A244",
                    });
                }

                if (ComsConfigModel.Entjoingroup == 0 && store.funJoinModel.isopen_pintuan)
                {
                    list.Add(new
                    {
                        title = "我的拼团",
                        icon = "dzicon icon-wodepintuan",
                        url = "/pages/group2/group2List",
                        c1 = "#FFE63E",
                        c2 = "#F2A244"
                    });
                }

                if (ComsConfigModel.Cutprice == 0 && store.funJoinModel.isopen_kanjia)
                {
                    list.Add(new
                    {
                        title = "我的砍价单",
                        icon = "dzicon icon-wodekanjiadan",
                        url = "/pages/bargain/bargainList",
                        c1 = "#FFE63E",
                        c2 = "#F2A244"
                    });
                }



                if (productMgr.ProductReservation == 0 && store.funJoinModel.isopen_yuyue)
                {
                    list.Add(new
                    {
                        title = "我的预约单",
                        icon = "dzicon icon-wodeyuyuedan",
                        url = "/pages/good/goodSub?showform=false",
                        c1 = "#61BCF7",
                        c2 = "#5AC5ED"
                    });
                }
                list.Add(new
                {
                    title = "收货地址",
                    icon = "dzicon icon-shouhuodizhi",
                    url = "/pages/my/myaddress",
                    c1 = "#F76161",
                    c2 = "#FF978F"
                });

                if (ComsConfigModel.Coupons == 0)
                {
                    list.Add(new
                    {
                        title = "我的优惠券",
                        icon = "dzicon icon-wodeyouhuiquan",
                        url = "/pages/discount/couponLst",
                        c1 = "#F76161",
                        c2 = "#FF978F"
                    });
                }
                if (OperationMgrModel.Integral == 0 && store.funJoinModel.isopen_jifen)
                {
                    list.Add(new
                    {
                        title = "积分中心",
                        icon = "dzicon icon-jifenzhongxin",
                        url = "/pages/integral/integral",
                        c1 = "#F76161",
                        c2 = "#FF978F"
                    });
                }

                //if (FuncMgrModel.SortShopping == 0 && store.funJoinModel.isopen_paidui)
                //{
                //    list.Add(new
                //    {
                //        title = "拿号排队",
                //        icon = "dzicon icon-nahaopaihao",
                //        url = "/pages/lineup/lineup",
                //        c1 = "#61BCF7",
                //        c2 = "#5AC5ED"
                //    });
                //}

                list.Add(new
                {
                    title = "我的评价",
                    icon = "dzicon icon-wodepingjia",
                    url = "/pages/good/goodValueLst?type=user",
                    c1 = "#61BCF7",
                    c2 = "#5AC5ED"
                });
                if (FuncMgrModel.IM == 0 && store.funJoinModel.isopen_kefu)
                {
                    list.Add(new
                    {
                        title = "客服",
                        icon = "dzicon icon-huodong",
                        url = "/pages/im/contact",
                        c1 = "#94D972",
                        c2 = "#6AD9B4"
                    });
                }

                int firstTypeCount = EntGoodTypeBLL.SingleModel.GetCount($"aid={xcx.Id} and parentId=0 and state=1");

                result.isok = true;
                result.Msg = "获取成功";
                result.dataObj = new
                {
                    list = list,
                    SaveMoney = OperationMgrModel.SaveMoney == 0,
                    SeondTypeOpen = firstTypeCount > 0 ? true : false
                };
                return Json(result, JsonRequestBehavior.AllowGet);

            }

            return Json(result, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取小程序版本级别 0 旗舰版 1尊享版 2高级版 3基础版
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetVersonId()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            result.code = "200";
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            XcxAppAccountRelation xcx = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcx == null)
            {
                result.isok = false;
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = xcx.VersionId;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取所有会员权益
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetAllVipRights()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            result.code = "200";
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            XcxAppAccountRelation xcx = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcx == null)
            {
                result.isok = false;
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            List<VipLevel> levelList = VipLevelBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            result.isok = true;
            result.dataObj = levelList.OrderBy(x=>x.level);
            result.Msg = "获取成功";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}