using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Stores;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using Entity.MiniApp.Stores;
using Entity.MiniApp.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Filters;
using Utility;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public partial class commonController : baseController
    {
        #region 会员管理

        /// <summary>
        /// 后台手动修改用户储值余额
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="uid"></param>
        /// <param name="saveMoney"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditUserSaveMoney(int appId = 0, int uid = 0, double saveMoney = 0.00)
        {

            if (appId <= 0 || uid <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(uid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "会员不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (userInfo.appId != xcx.AppId)
            {
                return Json(new { isok = false, msg = "没有权限!" }, JsonRequestBehavior.AllowGet);
            }

            SaveMoneySetUser saveMoneyAccount = SaveMoneySetUserBLL.SingleModel.getModelByUserId(xcx.AppId, userInfo.Id);
            int AccountMoney = Convert.ToInt32(saveMoney * 100);
            SaveMoneySetUserLog newLog = new SaveMoneySetUserLog()
            {
                AppId = xcx.AppId,
                UserId = userInfo.Id,
                CreateDate = DateTime.Now,
                State = 1
            };
            if (saveMoneyAccount == null)
            {
                if (AccountMoney < 0)
                {
                    return Json(new { isok = false, msg = "新开通的储值账号初始值不能为负数" }, JsonRequestBehavior.AllowGet);
                }

                //用户储值账户,若无则开通一个
                saveMoneyAccount = new SaveMoneySetUser()
                {
                    AppId = xcx.AppId,
                    UserId = userInfo.Id,
                    AccountMoney = AccountMoney,
                    CreateDate = DateTime.Now
                };
                saveMoneyAccount.Id = Convert.ToInt32(SaveMoneySetUserBLL.SingleModel.Add(saveMoneyAccount));
                if (saveMoneyAccount.Id <= 0)
                {
                    return Json(new { isok = false, msg = "开通储值账户失败,请重试" }, JsonRequestBehavior.AllowGet);
                }

                //加日志记录
                newLog.Type = 0;
                newLog.BeforeMoney = 0;
                newLog.MoneySetUserId = saveMoneyAccount.Id;
                newLog.ChangeMoney = AccountMoney;
                newLog.AfterMoney = AccountMoney;
                newLog.ChangeNote = $"商家增加{saveMoney}元";
                newLog.Id = Convert.ToInt32(SaveMoneySetUserLogBLL.SingleModel.Add(newLog));
                if (newLog.Id <= 0)
                {
                    return Json(new { isok = false, msg = "变更记录失败!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    VipRelation vipRelation = VipRelationBLL.SingleModel.GetModel($"uid={uid}");
                    if (vipRelation != null)
                    {
                        vipRelation.updatetime = DateTime.Now;
                        VipRelationBLL.SingleModel.Update(vipRelation, "updatetime");
                    }
                    VipRelationBLL.SingleModel.updatelevelBySaveMoney(uid, newLog.ChangeMoney);
                    return Json(new { isok = true, msg = "修改成功!" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //更新储值余额



                newLog.BeforeMoney = saveMoneyAccount.AccountMoney;
                newLog.Type = AccountMoney > 0 ? 0 : -1;//如果手动输入的值小于0则表示消费 否则表示在当前余额基础上充值
                saveMoneyAccount.AccountMoney = saveMoneyAccount.AccountMoney + AccountMoney;
                if (saveMoneyAccount.AccountMoney < 0)
                {
                    return Json(new { isok = false, msg = "修改失败(余额不足扣除)!" }, JsonRequestBehavior.AllowGet);
                }
                newLog.ChangeMoney = Math.Abs(saveMoneyAccount.AccountMoney - newLog.BeforeMoney);
                newLog.AfterMoney = saveMoneyAccount.AccountMoney;
                string txt = newLog.Type == 0 ? "增加" : "扣除";
                newLog.ChangeNote = $"商家{txt}{Math.Abs(saveMoney)}元";
                if (!SaveMoneySetUserBLL.SingleModel.Update(saveMoneyAccount, "AccountMoney"))
                {
                    return Json(new { isok = false, msg = "修改失败!" }, JsonRequestBehavior.AllowGet);
                }



                //加日志变更记录
                newLog.MoneySetUserId = saveMoneyAccount.Id;
                newLog.Id = Convert.ToInt32(SaveMoneySetUserLogBLL.SingleModel.Add(newLog));
                if (newLog.Id <= 0)
                {
                    return Json(new { isok = false, msg = "变更记录失败!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    VipRelation vipRelation = VipRelationBLL.SingleModel.GetModel($"uid={uid}");
                    if (vipRelation != null)
                    {
                        vipRelation.updatetime = DateTime.Now;
                        VipRelationBLL.SingleModel.Update(vipRelation, "updatetime");
                    }
                    if (newLog.Type == 0)
                    {
                        //表示储值
                        VipRelationBLL.SingleModel.updatelevelBySaveMoney(uid, newLog.ChangeMoney);
                    }
                    

                    XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
                    if (xcxTemplate == null)
                    {
                        return Json(new { isok = false, msg = "找不到模板!" }, JsonRequestBehavior.AllowGet);
                    }


                    bool updatelevelResult = false;
                    if (newLog.Type == -1 && AccountMoney < 0)
                    {
                        string no = WxPayApi.GenerateOutTradeNo();
                        CityMorders order = new CityMorders()
                        {
                            OrderType = (int)ArticleTypeEnum.MiniappEditSaveMoney,
                            ActionType = 1,
                            Addtime = DateTime.Now,
                            payment_free = Math.Abs(AccountMoney),
                            trade_no = no,
                            Percent = 0,
                            userip = WebHelper.GetIP(),
                            FuserId = userInfo.Id,
                            Fusername = userInfo.NickName,
                            orderno = no,
                            payment_status = 1,
                            Status = 0,
                            Articleid = 0,
                            CommentId = 0,
                            MinisnsId = 0,
                            TuserId = 0,
                            is_group = 0,
                            is_group_head = 0,
                            groupsponsor_id = 0,
                            ShowNote = "",
                            CitySubId = 0,
                            PayRate = 1,
                            buy_num = 1,
                            appid = xcx.AppId,
                            remark = "",
                            OperStatus = 0,
                            Tusername = "",
                            Note = $"商家在后台手动扣除用户储值余额{Math.Abs(AccountMoney)}元",
                        };
                        int orderid = Convert.ToInt32(new CityMordersBLL().Add(order));

                        if (orderid <= 0)
                            return Json(new { isok = false, msg = "修改失败(插入订单异常)!" }, JsonRequestBehavior.AllowGet);
                        //表示消费
                        switch (xcxTemplate.Type)
                        {
                            case (int)TmpType.小程序专业模板:
                                updatelevelResult = VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "entpro");
                                break;
                            case (int)TmpType.小程序电商模板:
                                updatelevelResult = VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "");
                                break;
                            case (int)TmpType.小程序餐饮模板:
                                updatelevelResult = VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "food");
                                break;
                            case (int)TmpType.小未平台子模版:
                                updatelevelResult = VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "platchild");
                                break;
                            case (int)TmpType.小程序多门店模板:
                                updatelevelResult = VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "multistore");
                                break;
                            case (int)TmpType.小程序足浴模板:
                                updatelevelResult = VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "footbath");
                                break;


                        }
                    }




                    return Json(new { isok = true, msg = "修改成功!" }, JsonRequestBehavior.AllowGet);
                }
            }





        }




        //商品列表
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="ispost"></param>
        /// <returns></returns>
        public ActionResult GoodsList(int appId, int pageIndex = 1, int pageSize = 10, int PageType = 22)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没权限！", code = "503" });
            }
            ViewBag.appId = appId;
            object obj = new object();
            switch (PageType)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModelByAppId(appId);
                    if (store_Food == null)
                    {
                        return Json(obj);
                    }

                    List<FoodGoods> goods_Food = FoodGoodsBLL.SingleModel.GetPageList(store_Food.Id, 1, pageSize, pageIndex, "Id,ImgUrl,GoodsName,CreateDate", "sort desc,id desc");
                    int recordCount_Food = FoodGoodsBLL.SingleModel.GetListCount(store_Food.Id, 1);
                    List<object> listGoods_Food = new List<object>();
                    foreach (FoodGoods item in goods_Food)
                    {
                        listGoods_Food.Add(new
                        {
                            Id = item.Id,
                            ImgUrl = item.ImgUrl,
                            GoodsName = item.GoodsName,
                            showtime = item.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            sel = false
                        });
                    }
                    obj = new
                    {
                        recordCount = recordCount_Food,
                        list = listGoods_Food
                    };

                    break;
                case (int)TmpType.小程序多门店模板:
                case (int)TmpType.小程序足浴模板:
                case (int)TmpType.小程序专业模板:
                    List<EntGoods> goodslist = EntGoodsBLL.SingleModel.GetList($" aid={appId} and state=1 and tag = 1 ", pageSize, pageIndex, "id,img,name,addtime", " id desc ");
                    int recordCount = EntGoodsBLL.SingleModel.GetCount($"aid={appId} and state=1 and tag = 1 ");
                    List<object> listGoods = new List<object>();
                    foreach (EntGoods item in goodslist)
                    {
                        listGoods.Add(new
                        {
                            Id = item.id,
                            ImgUrl = item.img,
                            GoodsName = item.name,
                            showtime = item.addtime.ToString("yyyy-MM-dd HH:mm:ss"),
                            sel = false
                        });
                    }
                    obj = new
                    {
                        recordCount = recordCount,
                        list = listGoods
                    };

                    break;
                case (int)TmpType.小程序电商模板:
                    //默认返回电商版

                    Store store = StoreBLL.SingleModel.GetModelByAId(appId);
                    if (store == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "请先配置店铺信息!", code = "403" });
                    }
                    string strwhere = $"StoreId={store.Id} and State>=0";

                    List<StoreGoods> list = StoreGoodsBLL.SingleModel.GetList(strwhere, pageSize, pageIndex, "*", " Id Desc ") ?? new List<StoreGoods>();

                    int TotalCount = StoreGoodsBLL.SingleModel.GetCount(strwhere);
                    list.ForEach(g => g.showtime = g.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    obj = new
                    {
                        recordCount = TotalCount,
                        list = list
                    };

                    break;

                case (int)TmpType.小未平台子模版:
                    recordCount = 0;
                    List<PlatChildGoods> platChildGoodsList = PlatChildGoodsBLL.SingleModel.GetListByRedis(appId, ref recordCount, string.Empty, 0, 0, 1, pageIndex, pageSize);

                    listGoods = new List<object>();
                    foreach (PlatChildGoods item in platChildGoodsList)
                    {
                        listGoods.Add(new
                        {
                            Id = item.Id,
                            ImgUrl = item.Img,
                            GoodsName = item.Name,
                            showtime = item.Addtime.ToString("yyyy-MM-dd HH:mm:ss"),
                            sel = false
                        });
                    }
                    obj = new
                    {
                        recordCount = recordCount,
                        list = listGoods
                    };

                    break;



            }
            return Json(obj);



        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="PageType">以此区分不同模板，即模板id</param>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult VipSetting(int appId, int PageType = 22)
        {
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }
            XcxApiBLL.SingleModel._openType = xcx.ThirdOpenType;

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                return View("PageError", new Return_Msg() { Msg = "找不到模板!", code = "403" });

            }

            int VIPSwtich = 0;//会员开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                versionId = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    OperationMgr operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                    VIPSwtich = operationMgr.VIP;
                }
            }
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;
            ViewBag.VIPSwtich = VIPSwtich;
            ViewBag.versionId = versionId;
            ViewBag.appId = appId;

            int modelId = 0;
            PageType = xcxTemplate.Type;
            ViewBag.PageType = PageType;

            switch (PageType)
            {
                case (int)TmpType.小程序专业模板:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(appId);
                    if (ent == null)
                        return View("PageError", new Return_Msg() { Msg = "找不到该专业版!", code = "500" });
                    modelId = ent.aid;

                    break;
                case (int)TmpType.小程序电商模板:
                    Store store = StoreBLL.SingleModel.GetModelByAId(appId);
                    if (store == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "店铺不存在!", code = "500" });
                    }
                    modelId = store.Id;
                    break;
                case (int)TmpType.小程序餐饮模板:
                    Food miAppfood = FoodBLL.SingleModel.GetModel($"appId={appId}");
                    if (miAppfood == null)
                    {
                        return Content("系统繁忙miAppfood_null！");
                    }
                    modelId = miAppfood.Id;
                    break;
                case (int)TmpType.小程序足浴模板:
                case (int)TmpType.小程序多门店模板:
                    FootBath footBath = FootBathBLL.SingleModel.GetModel($"appId={xcx.Id}");
                    if (footBath == null)
                        return View("PageError", new Return_Msg() { Msg = "找不到该版本!", code = "500" });
                    modelId = footBath.appId;
                    break;
                case (int)TmpType.小未平台子模版:
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(xcx.Id, 2);

                    if (platStore == null)
                    {
                        return Json(new { isok = false, msg = "店铺不存在！" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = platStore.Aid;
                    break;
            }


            VipViewModel model = new VipViewModel();
            model.config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (model.config == null)
            {
                model.config = new VipConfig();
                model.config.addtime = model.config.updatetime = DateTime.Now;
                model.config.appId = xcx.AppId;
                VipConfigBLL.SingleModel.Add(model.config);
            }
            model.levelList = VipLevelBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            if (model.levelList == null || model.levelList.Count <= 0)
            {
                VipLevel def_level = new VipLevel();
                def_level.addtime = DateTime.Now;
                def_level.appId = xcx.AppId;
                def_level.name = "普通会员";
                def_level.bgcolor = "#4a86e8";
                def_level.updatetime = def_level.addtime;
                model.levelList = new List<VipLevel>();
                def_level.Id = Convert.ToInt32(VipLevelBLL.SingleModel.Add(def_level));
                model.levelList.Add(def_level);
            }
            else
            {
                foreach (VipLevel info in model.levelList)
                {
                    if (info.type == 2 && !string.IsNullOrEmpty(info.gids))
                    {

                        switch (PageType)
                        {
                            case (int)TmpType.小程序专业模板:
                            case (int)TmpType.小程序足浴模板:
                            case (int)TmpType.小程序多门店模板:
                                //专业版
                                var goodslist = EntGoodsBLL.SingleModel.GetList($"id in ({info.gids})").Where(item => item.aid == appId);
                                List<StoreGoods> listGoods = new List<StoreGoods>();
                                foreach (EntGoods item in goodslist)
                                {
                                    listGoods.Add(new StoreGoods()
                                    {
                                        Id = item.id,
                                        GoodsName = item.name,
                                        ImgUrl = item.img

                                    });
                                }
                                info.goodslist = listGoods;

                                break;
                            case (int)TmpType.小程序电商模板:
                                info.goodslist = StoreGoodsBLL.SingleModel.GetList($"id in ({info.gids})");
                                break;

                            case (int)TmpType.小程序餐饮模板:
                                List<FoodGoods> foodGoodslist = FoodGoodsBLL.SingleModel.GetList($"id in ({info.gids})");
                                List<StoreGoods> listFoodGoods = new List<StoreGoods>();
                                foreach (FoodGoods item in foodGoodslist)
                                {
                                    listFoodGoods.Add(new StoreGoods()
                                    {
                                        Id = item.Id,
                                        GoodsName = item.GoodsName,
                                        ImgUrl = item.ImgUrl

                                    });
                                }
                                info.goodslist = listFoodGoods;
                                break;

                            case (int)TmpType.小未平台子模版:
                                List<PlatChildGoods> Goodslist = PlatChildGoodsBLL.SingleModel.GetListByIds(info.gids);
                                List<StoreGoods> listPlatChildGoods = new List<StoreGoods>();
                                foreach (PlatChildGoods item in Goodslist)
                                {
                                    listPlatChildGoods.Add(new StoreGoods()
                                    {
                                        Id = item.Id,
                                        GoodsName = item.Name,
                                        ImgUrl = item.Img

                                    });
                                }
                                info.goodslist = listPlatChildGoods;

                                break;

                        }
                    }
                }
            }
            VipLevel dflevel = model.levelList.Where(l => l.level == 0).FirstOrDefault();
            if (dflevel == null)
            {
                dflevel = new VipLevel();
                dflevel.addtime = DateTime.Now;
                dflevel.appId = xcx.AppId;
                dflevel.name = "普通会员";
                dflevel.bgcolor = "#4a86e8";
                dflevel.updatetime = dflevel.addtime;
                dflevel.Id = Convert.ToInt32(VipLevelBLL.SingleModel.Add(dflevel));
            }

            model.ruleList = VipRuleBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0 and RuleType=0");
            if (model.ruleList == null || model.ruleList.Count <= 0)
            {
                VipRule rule = new VipRule();
                rule.addtime = rule.updatetime = DateTime.Now;
                rule.appId = xcx.AppId;
                rule.minMoney = 0;
                rule.maxMoney = 0;
                rule.levelid = dflevel.Id;
                rule.id = Convert.ToInt32(VipRuleBLL.SingleModel.Add(rule));
                //rule.levelinfo = dflevel;
                model.ruleList.Add(rule);
            }

            //储值规则升级
            model.SaveMoneyRuleList = VipRuleBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0 and RuleType=1");
            if (model.SaveMoneyRuleList == null || model.SaveMoneyRuleList.Count <= 0)
            {
                VipRule rule = new VipRule();
                rule.addtime = rule.updatetime = DateTime.Now;
                rule.appId = xcx.AppId;
                rule.minMoney = 0;
                rule.maxMoney = 0;
                rule.levelid = dflevel.Id;
                rule.RuleType = 1;
                rule.id = Convert.ToInt32(VipRuleBLL.SingleModel.Add(rule));

                model.SaveMoneyRuleList.Add(rule);
            }

            string authorizer_access_token = string.Empty;
            int isAuth = 0;
            int isAuthCard = 0;
            int haveCard = 0;
            string SerName = string.Empty;
            string cardStaus = string.Empty;
            int cardType = 0;

            VipWxCard card = null;

            switch (PageType)
            {
                case (int)TmpType.小未平台子模版:
                    cardType = 5;//表示小未平台子模版 
                    break;
                case (int)TmpType.小程序多门店模板:
                    cardType = 4;//表示多门店版本 多门店版本只有总店
                    break;
                case (int)TmpType.小程序足浴模板:
                    cardType = 3;
                    break;
                case (int)TmpType.小程序专业模板:
                    //Type=2 表示专业版的微信会员卡
                    cardType = 2;
                    break;
                case (int)TmpType.小程序餐饮模板:
                    //Type=1 表示小程序餐饮模板的微信会员卡
                    cardType = 1;
                    break;
                case (int)TmpType.小程序电商模板:
                    cardType = 0;
                    break;
            }
            card = VipWxCardBLL.SingleModel.GetModel($"Type={cardType} and AppId={modelId}");
            if (card != null)
            {
                string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(card.User_Name);
                authorizer_access_token = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);

                isAuth = 1;
                isAuthCard = 1;
                haveCard = 1;
                string cardResult = Context.PostData($"https://api.weixin.qq.com/card/get?access_token={authorizer_access_token}", JsonConvert.SerializeObject(new { card_id = card.CardId }));
                if (cardResult.Contains("CARD_STATUS_VERIFY_FAIL"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_VERIFY_FAIL;
                }
                if (cardResult.Contains("CARD_STATUS_VERIFY_OK"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_VERIFY_OK;
                }
                if (cardResult.Contains("CARD_STATUS_NOT_VERIFY"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_NOT_VERIFY;
                }
                if (cardResult.Contains("CARD_STATUS_DELETE1"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_DELETE1;
                }
                if (cardResult.Contains("CARD_STATUS_DISPATCH"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_DISPATCH;
                }

                switch (card.Status)
                {
                    case -1:
                        cardStaus = "已删除";
                        break;
                    case 0:
                        cardStaus = "待审核";
                        break;
                    case 1:
                        cardStaus = "审核失败";
                        break;
                    case 2:
                        cardStaus = "通过审核";
                        break;
                    case 3:
                        cardStaus = "已投放";
                        break;
                    default:
                        cardStaus = "待审核";
                        break;
                }

                card.UpdateTime = DateTime.Now;
                VipWxCardBLL.SingleModel.Update(card, "UpdateTime,Status");

                if (card.Status == 2)
                    cardStaus = "已同步到微信卡包";

                SerName = card.SerName;
            }
            else
            {
                OpenAuthorizerConfig umodel = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), appId, 4).FirstOrDefault();
                if (umodel != null)
                {
                    isAuth = 1;
                    isAuthCard = 1;
                    SerName = umodel.nick_name;
                    string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(umodel.user_name);
                    authorizer_access_token = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);
                }
            }

            string returnurl = AsUrlData($"{WebSiteConfig.XcxAppReturnUrl}/common/VipSetting?appId={appId}&PageType={PageType}");

            if (card != null)
            {
                ViewBag.AuthUrl =
              $"{WebSiteConfig.GoToGetAuthoUrl}index?userId={dzaccount.Id}&newtype=4&rid={xcx.Id}&user_name={card.User_Name}&returnurl={returnurl}";

            }
            else
            {
                ViewBag.AuthUrl =
              $"{WebSiteConfig.GoToGetAuthoUrl}index?userId={dzaccount.Id}&newtype=4&rid={xcx.Id}&returnurl={returnurl}";

            }


            ViewBag.IsAuth = isAuth;
            ViewBag.IsAuthCard = isAuthCard;
            ViewBag.HaveCard = haveCard;
            ViewBag.CardStaus = cardStaus;
            ViewBag.SerName = SerName;
            ViewBag.token = authorizer_access_token;
            //   string ak = WxHelper.GetToken();
            //if (string.IsNullOrEmpty(ak))
            //{
            //    ak = WxHelper.GetToken("wx64f161aa79a6801b", "65d0158981a05224eab3d690c36f035e", true);
            //}
            ViewBag.ak = "ak";
            model.levelList= model.levelList.OrderBy(x => x.level).ToList();



            return View(model);
        }
        private string AsUrlData(string data)
        {
            return Uri.EscapeDataString(data);
        }

        /// <summary>
        /// 添加编辑会员级别
        /// </summary>
        public ActionResult SavelevelInfo(int PageType = 22)
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }

            #region 专业版版本控制功能
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                return Json(new { isok = false, msg = $"找不到模板" }, JsonRequestBehavior.AllowGet);

            }
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    return Json(new { isok = false, msg = $"功能权限未设置" }, JsonRequestBehavior.AllowGet);

                }
                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }

                if (operationMgr.VIP == 1)
                {
                    return Json(new { isok = false, msg = $"请先升级到更高版本才能开启此功能" }, JsonRequestBehavior.AllowGet);
                }

            }
            #endregion


            int id = Context.GetRequestInt("id", -1);
            if (id < 0)
            {
                return Json(new { isok = false, msg = "参数错误id_error" + id }, JsonRequestBehavior.AllowGet);
            }
            string name = Context.GetRequest("name", string.Empty);
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { isok = false, msg = "请填写级别名称" }, JsonRequestBehavior.AllowGet);
            }
            if (name.Length > 5)
            {
                return Json(new { isok = false, msg = "级别名称长度不能超过5个字" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel model = VipLevelBLL.SingleModel.GetModel($" name='{name}' and state>=0 and appId='{xcx.AppId}'");
            if (id <= 0 && model != null)
            {
                return Json(new { isok = false, msg = "该级别名称已存在" }, JsonRequestBehavior.AllowGet);
            }
            int type = Context.GetRequestInt("type", -1);
            if (type < 0)
            {
                return Json(new { isok = false, msg = "参数错误type_error" }, JsonRequestBehavior.AllowGet);
            }
            string gids = Context.GetRequest("gids", string.Empty);
            if (type == 2 && string.IsNullOrEmpty(gids))
            {
                return Json(new { isok = false, msg = "请选择折扣商品" }, JsonRequestBehavior.AllowGet);
            }

            //会员等级
            int level = Context.GetRequestInt("level", -1);
            if (level <= -1)
            {
                return Json(new { isok = false, msg = "请选择等级" }, JsonRequestBehavior.AllowGet);
            }
            #region 
            VipLevel levelmodel = VipLevelBLL.SingleModel.GetModel($" (level={level} or (level=0 and {level}=1)) and state>=0 and appId='{xcx.AppId}'");
            //判断会员卡等级是否存在
            if (levelmodel != null)
            {
                if (levelmodel.Id != id)
                {
                    return Json(new { isok = false, msg = "会员卡等级已存在" }, JsonRequestBehavior.AllowGet);
                }
            }
            #endregion




            int discount = Context.GetRequestInt("discount", 0);
            if (type != 0) //type:0-无折扣 1-全场折扣 2-部分折扣
            {
                if (discount <= 0)
                {
                    return Json(new { isok = false, msg = "请填写商品折扣" }, JsonRequestBehavior.AllowGet);
                }
                if (discount < 0 || discount >= 100)
                {
                    return Json(new { isok = false, msg = "请填写0~10之间的数字，最多保留一位小数" }, JsonRequestBehavior.AllowGet);
                }
            }
            string bgcolor = Context.GetRequest("bgcolor", string.Empty);
            if (string.IsNullOrEmpty(bgcolor))
            {
                return Json(new { isok = false, msg = "请选择会员封面" }, JsonRequestBehavior.AllowGet);
            }
            if (id > 0)
            {
                model = VipLevelBLL.SingleModel.GetModel($"id={id} and appId='{xcx.AppId}' and state>=0");
                if (model == null)
                {
                    return Json(new { isok = false, msg = "数据异常" }, JsonRequestBehavior.AllowGet);
                }
                model.name = name;
                model.gids = gids;
                model.level = level == 1 ? 0 : level;//普通会员不能删除，所以会员等级为1的改成0
                model.type = type;
                model.updatetime = DateTime.Now;
                model.bgcolor = bgcolor;
                if (type != 0)
                {
                    model.discount = discount;
                }
                if (type == 2 && !string.IsNullOrEmpty(gids))
                {
                    switch (PageType)
                    {
                        case (int)TmpType.小程序多门店模板:
                        case (int)TmpType.小程序足浴模板:
                        case (int)TmpType.小程序专业模板:
                            List<EntGoods> goodslist = EntGoodsBLL.SingleModel.GetList($"id in ({gids})");
                            List<StoreGoods> listGoods = new List<StoreGoods>();
                            foreach (EntGoods item in goodslist)
                            {
                                listGoods.Add(new StoreGoods()
                                {
                                    Id = item.id,
                                    GoodsName = item.name,
                                    ImgUrl = item.img

                                });
                            }
                            model.goodslist = listGoods;
                            break;
                        case (int)TmpType.小程序电商模板:
                            model.goodslist = StoreGoodsBLL.SingleModel.GetList($"id in ({gids})");
                            break;

                        case (int)TmpType.小未平台子模版:
                            List<PlatChildGoods> Goodslist = PlatChildGoodsBLL.SingleModel.GetListByIds(gids);
                            List<StoreGoods> listPlatChildGoods = new List<StoreGoods>();
                            foreach (PlatChildGoods item in Goodslist)
                            {
                                listPlatChildGoods.Add(new StoreGoods()
                                {
                                    Id = item.Id,
                                    GoodsName = item.Name,
                                    ImgUrl = item.Img

                                });
                            }
                            model.goodslist = listPlatChildGoods;

                            break;

                    }

                }
                bool isok = VipLevelBLL.SingleModel.Update(model, "name,gids,type,bgcolor,updatetime,discount,level");
                string msg = isok ? "保存成功" : "保存失败";
                return Json(new { isok = isok, msg = msg, model = model }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                model = new VipLevel();
                model.name = name;
                model.gids = gids;
                model.type = type;
                model.addtime = model.updatetime = DateTime.Now;
                model.level = level == 1 ? 0 : level;//普通会员不能删除，所以会员等级为1的改成0
                model.appId = xcx.AppId;
                model.bgcolor = bgcolor;
                if (type != 0)
                {
                    model.discount = discount;
                }
                if (type == 2 && !string.IsNullOrEmpty(gids))
                {
                    switch (PageType)
                    {
                        case (int)TmpType.小程序多门店模板:
                        case (int)TmpType.小程序足浴模板:
                        case (int)TmpType.小程序专业模板:
                            List<EntGoods> goodslist = EntGoodsBLL.SingleModel.GetList($"id in ({gids})");
                            List<StoreGoods> listGoods = new List<StoreGoods>();
                            foreach (EntGoods item in goodslist)
                            {
                                listGoods.Add(new StoreGoods()
                                {
                                    Id = item.id,
                                    GoodsName = item.name,
                                    ImgUrl = item.img

                                });
                            }
                            model.goodslist = listGoods;
                            break;
                        case (int)TmpType.小程序电商模板:
                            model.goodslist = StoreGoodsBLL.SingleModel.GetList($"id in ({gids})");
                            break;
                        case (int)TmpType.小未平台子模版:
                            List<PlatChildGoods> Goodslist = PlatChildGoodsBLL.SingleModel.GetListByIds(gids);
                            List<StoreGoods> listPlatChildGoods = new List<StoreGoods>();
                            foreach (PlatChildGoods item in Goodslist)
                            {
                                listPlatChildGoods.Add(new StoreGoods()
                                {
                                    Id = item.Id,
                                    GoodsName = item.Name,
                                    ImgUrl = item.Img

                                });
                            }
                            model.goodslist = listPlatChildGoods;

                            break;
                    }

                    model.goodslist = StoreGoodsBLL.SingleModel.GetList($"id in ({gids})");
                }
                model.Id = Convert.ToInt32(VipLevelBLL.SingleModel.Add(model));
                bool isok = model.Id > 0;
                string msg = isok ? "保存成功" : "保存失败";
                return Json(new { isok = isok, msg = msg, model = model }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 删除会员等级时验证该等级下还有多少会员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult validviplist(int id)
        {
            if (id <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }

            string countsql = $"select count(*) as count from viprelation left join c_userinfo on viprelation.uid=c_userinfo.id  where viprelation.levelid={id} and viprelation.state>=0  and c_userinfo.nickname is not NULL and c_userinfo.headimgurl is not NULL";

            int count = 0;
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, countsql);
            if (obj != DBNull.Value)
            {
                count = Convert.ToInt32(obj);
            }
            //   int count = _miniappVipRelationBll.GetCount($"levelid={id} and state>=0");
            string msg = string.Empty;
            if (count > 0)
            {
                return Json(new { isok = false, msg = $"该会员等级下还有{count}个会员" }, JsonRequestBehavior.AllowGet);
            }
            count = VipRuleBLL.SingleModel.GetCount($"levelid={id} and state>=0");
            if (count > 0)
            {
                return Json(new { isok = false, msg = $"自动升级规则设定中还有此会员等级，请先对升级规则做出修改" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = true }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 删除会员等级
        /// </summary>
        /// <returns></returns>
        public ActionResult delLevel()
        {
            int id = Context.GetRequestInt("id", 0);
            int appId = Context.GetRequestInt("appId", 0);
            if (id <= 0 || appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={id} and appid='{xcx.AppId}' and state>=0");
            if (levelinfo == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }
            levelinfo.state = -1;
            levelinfo.updatetime = DateTime.Now;
            bool isok = VipLevelBLL.SingleModel.Update(levelinfo, "state,updatetime");
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 开启\关闭会员自动升级
        /// </summary>
        /// <returns></returns>
        public ActionResult saveConfig()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int autoswitch = Context.GetRequestInt("switchState", -1);
            int ruleType = Context.GetRequestInt("ruleType", 0);
           
            if (appId <= 0 || autoswitch < 0 || ruleType < 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }

            #region 专业版版本控制功能
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                return Json(new { isok = false, msg = $"找不到模板" }, JsonRequestBehavior.AllowGet);

            }
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    return Json(new { isok = false, msg = $"功能权限未设置" }, JsonRequestBehavior.AllowGet);

                }
                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }

                if (operationMgr.VIP == 1)
                {
                    return Json(new { isok = false, msg = $"请先升级到更高版本才能开启此功能" }, JsonRequestBehavior.AllowGet);
                }

            }
            #endregion

            VipConfig config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (config == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }

            string updateFiled = "autoupdate";
            if (ruleType == 0)
            {
                config.autoupdate = autoswitch;
                
            }
            else
            {
                config.SaveMoneyAutoUpdate = autoswitch;
                updateFiled = "SaveMoneyAutoUpdate";
            }
           
            config.updatetime = DateTime.Now;
            bool isok = VipConfigBLL.SingleModel.Update(config, $"{updateFiled},updatetime");
            string msg = string.Empty;
            if (ruleType == 0)
            {
               msg= config.autoswitch ? "自动升级已开启" : "自动升级已关闭";
            }
            else
            {
                msg = config.SaveMoneyAutoUpdateSwtich ? "自动升级已开启" : "自动升级已关闭";
            }

            if (!isok)
            {
                msg = "操作失败";
            }
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 开启\关闭会员卡自动同步 创建会员卡卡套
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveSyncCard(int PageType = 22)
        {
            int appId = Context.GetRequestInt("appId", 0);

            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxApiBLL.SingleModel._openType = xcx.ThirdOpenType;

            #region 专业版版本控制功能
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                return Json(new { isok = false, msg = $"找不到模板" }, JsonRequestBehavior.AllowGet);

            }
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    return Json(new { isok = false, msg = $"功能权限未设置" }, JsonRequestBehavior.AllowGet);

                }
                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }

                if (operationMgr.VIP == 1)
                {
                    return Json(new { isok = false, msg = $"请先升级到更高版本才能开启此功能" }, JsonRequestBehavior.AllowGet);
                }

            }
            #endregion

            int cardType = 2;
            int modelId = 0;
            string logo = string.Empty, name = string.Empty;
            switch (PageType)
            {
                case (int)TmpType.小程序专业模板:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(appId);
                    if (ent == null)
                        return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);

                    modelId = ent.aid;
                    ConfParam imginfo = ConfParamBLL.SingleModel.GetModelByParamappid("logoimg", xcx.AppId);
                    if (imginfo == null)
                    {
                        return Json(new { isok = false, msg = "请先到小程序管理配置底部Logo" }, JsonRequestBehavior.AllowGet);
                    }
                    logo = imginfo.Value;

                    OpenAuthorizerConfig XUserList = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcx.AppId);
                    if (XUserList == null)
                    {
                        return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
                    }
                    name = XUserList.nick_name;
                    cardType = 2;
                    break;
                case (int)TmpType.小程序电商模板:
                    //表示电商版
                    Store store = StoreBLL.SingleModel.GetModelByAId(appId);
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = store.Id;
                    logo = store.logo;
                    name = store.name;
                    cardType = 0;
                    break;

                case (int)TmpType.小程序餐饮模板:
                    //小程序餐饮模板
                    Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
                    if (miAppFood == null)
                    {
                        return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = miAppFood.Id;
                    logo = miAppFood.Logo;
                    name = miAppFood.FoodsName;
                    cardType = 1;
                    break;

                case (int)TmpType.小程序足浴模板://表示足浴版
                    FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
                    if (storeModel == null)
                    {
                        return Json(new { isok = false, msg = "找不到该版本" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = storeModel.appId;
                    name = storeModel.StoreName;
                    cardType = 3;
                    logo = GetStoreLogo(storeModel.Id, (int)AttachmentItemType.小程序足浴版店铺logo);
                    if (string.IsNullOrEmpty(logo) || string.IsNullOrEmpty(name))
                    {
                        return Json(new { isok = false, msg = "请先到门店信息配置店铺名称以及Logo" }, JsonRequestBehavior.AllowGet);
                    }
                    break;
                case (int)TmpType.小程序多门店模板://表示多门店
                    storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
                    if (storeModel == null)
                    {
                        return Json(new { isok = false, msg = "找不到该版本" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = storeModel.appId;
                    name = storeModel.StoreName;
                    cardType = 4;
                    logo = GetStoreLogo(storeModel.Id, (int)AttachmentItemType.小程序多门店版门店logo);
                    if (string.IsNullOrEmpty(logo) || string.IsNullOrEmpty(name))
                    {
                        return Json(new { isok = false, msg = "请先到门店信息配置店铺名称以及Logo" }, JsonRequestBehavior.AllowGet);
                    }
                    break;
                case (int)TmpType.小未平台子模版:
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(appId, 2);
                    if (platStore == null)
                        return Json(new { isok = false, msg = "找不到该店铺" }, JsonRequestBehavior.AllowGet);

                    modelId = platStore.Aid;
                    name = platStore.Name;
                    logo = platStore.StoreHeaderImg;
                    cardType = 5;

                    break;
            }
            
            VipConfig config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (config == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={modelId}");
            if (_vipWxCard != null)
            {
                return Json(new { isok = true, msg = "操作成功", obj = _vipWxCard }, JsonRequestBehavior.AllowGet);
            }
            _vipWxCard = new VipWxCard();

            OpenAuthorizerConfig umodel = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), appId, 4).FirstOrDefault();
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "操作失败(请先绑定认证服务号或者申请代制)" }, JsonRequestBehavior.AllowGet);
            }

            //log4net.LogHelper.WriteInfo(this.GetType(), dzaccount.Id.ToString() + "&" + appId);
            OpenAuthorizerConfig app = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), appId).FirstOrDefault();
            if (app == null)
            {
                return Json(new { isok = false, msg = "请先到小程序管理绑定小程序" }, JsonRequestBehavior.AllowGet);
            }
            //这里可能会出现token失效 个人发布的未授权给我们第三方平台的卡券会生成不了
            string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(umodel.user_name);
            string authorizer_access_token = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);
            if (string.IsNullOrEmpty(logo) || string.IsNullOrEmpty(name))
            {
                return Json(new { isok = false, msg = "操作失败(请先配置店铺信息Logo以及店铺名称)" }, JsonRequestBehavior.AllowGet);
            }
            string uploadImgResult = CommondHelper.WxUploadImg(authorizer_access_token, logo);
            if (!uploadImgResult.Contains("url"))
            {
                return Json(new { isok = false, msg = "操作失败", obj = uploadImgResult }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (name.Length >= 12)
                {
                    name = name.Substring(0, 12);
                }
                string carTitle = name;
                if (carTitle.Length >= 6)
                {
                    carTitle = carTitle.Substring(0, 6);
                }


                WxUploadImgResult r = JsonConvert.DeserializeObject<WxUploadImgResult>(uploadImgResult);
                CreateCardResult _createCardResult = CommondHelper.AddVipWxCard(r.url, name, carTitle + "会员卡", app.user_name, authorizer_access_token, PageType);

                if (_createCardResult.errcode != 0)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), $"errorCode:{_createCardResult.errcode},errmsg:{_createCardResult.errmsg}");
                    return Json(new { isok = false, msg = "操作失败", obj = _createCardResult }, JsonRequestBehavior.AllowGet);
                }

                _vipWxCard.CardId = _createCardResult.card_id;
                _vipWxCard.AppId = modelId;
                _vipWxCard.Type = cardType;
                _vipWxCard.User_Name = umodel.user_name;
                _vipWxCard.SerName = umodel.nick_name;
                _vipWxCard.Status = (int)WxVipCardStatus.CARD_STATUS_NOT_VERIFY;
                _vipWxCard.AddTime = DateTime.Now;
                int VipWxCardId = Convert.ToInt32(VipWxCardBLL.SingleModel.Add(_vipWxCard));

                return Json(new { isok = true, msg = VipWxCardId > 0 ? "操作成功" : "操作失败", CreateCardResult = _createCardResult, VipWxCardId = VipWxCardId, authorizer_access_token = authorizer_access_token }, JsonRequestBehavior.AllowGet);

            }


        }

        private string GetStoreLogo(int storeId, int itemType)
        {
            string logo = string.Empty;
            List<C_Attachment> LogoList = C_AttachmentBLL.SingleModel.GetListByCache(storeId, itemType);
            if (LogoList != null && LogoList.Count > 0)
            {
                logo = LogoList[0].filepath;
            }
            return logo;
        }


        /// <summary>
        /// 更新微信会员卡
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateWxCard(int PageType = 22)
        {
            int appId = Context.GetRequestInt("appId", 0);

            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxApiBLL.SingleModel._openType = xcx.ThirdOpenType;

            #region 专业版版本控制功能
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                return Json(new { isok = false, msg = $"找不到模板" }, JsonRequestBehavior.AllowGet);

            }
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    return Json(new { isok = false, msg = $"功能权限未设置" }, JsonRequestBehavior.AllowGet);

                }
                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }

                if (operationMgr.VIP == 1)
                {
                    return Json(new { isok = false, msg = $"请先升级到更高版本才能开启此功能" }, JsonRequestBehavior.AllowGet);
                }

            }
            #endregion

            int cardType = 0;
            int modelId = 0;
            string logo = string.Empty, name = string.Empty;
            //默认专业版的
            string center_app_brand_pass = "pages/my/myInfo";//专业版 个人中心
            string custom_app_brand_pass = "pages/index/index";//首页 专业版
            switch (PageType)
            {
                case (int)TmpType.小程序专业模板:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(appId);
                    if (ent == null)
                        return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);
                    modelId = ent.aid;
                    ConfParam imginfo = ConfParamBLL.SingleModel.GetModelByParamappid("logoimg", xcx.AppId);
                    if (imginfo == null)
                    {
                        return Json(new { isok = false, msg = "请先到小程序管理配置底部Logo" }, JsonRequestBehavior.AllowGet);
                    }
                    logo = imginfo.Value;

                    OpenAuthorizerConfig XUserList = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcx.AppId);
                    if (XUserList == null)
                    {
                        return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
                    }

                    name = XUserList.nick_name;
                    cardType = 2;
                    break;
                case (int)TmpType.小程序电商模板:
                    //表示电商版
                    Store store = StoreBLL.SingleModel.GetModelByAId(appId);
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = store.Id;
                    logo = store.logo;
                    name = store.name;
                    center_app_brand_pass = "pages/me/me";
                    break;
                case (int)TmpType.小程序餐饮模板:
                    //小程序餐饮模板
                    Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
                    if (miAppFood == null)
                    {
                        return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = miAppFood.Id;
                    logo = miAppFood.Logo;
                    name = miAppFood.FoodsName;
                    cardType = 1;
                    center_app_brand_pass = "pages/me/me";//个人中心页面
                    custom_app_brand_pass = "pages/home/home";//首页
                    break;
                case (int)TmpType.小程序足浴模板:
                    FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
                    if (storeModel == null)
                    {
                        return Json(new { isok = false, msg = "找不到该版本" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = storeModel.appId;
                    name = storeModel.StoreName;
                    cardType = 3;
                    logo = GetStoreLogo(storeModel.Id, (int)AttachmentItemType.小程序足浴版店铺logo);
                    if (string.IsNullOrEmpty(logo) || string.IsNullOrEmpty(name))
                    {
                        return Json(new { isok = false, msg = "请先到门店信息配置店铺名称以及Logo" }, JsonRequestBehavior.AllowGet);
                    }
                    center_app_brand_pass = "pages/me/me";//个人中心页面
                    custom_app_brand_pass = "pages/book/book";//首页
                    break;
                case (int)TmpType.小程序多门店模板:
                    storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
                    if (storeModel == null)
                    {
                        return Json(new { isok = false, msg = "找不到该版本" }, JsonRequestBehavior.AllowGet);
                    }
                    modelId = storeModel.appId;
                    name = storeModel.StoreName;
                    cardType = 4;
                    logo = GetStoreLogo(storeModel.Id, (int)AttachmentItemType.小程序多门店版门店logo);
                    if (string.IsNullOrEmpty(logo) || string.IsNullOrEmpty(name))
                    {
                        return Json(new { isok = false, msg = "请先到门店信息配置店铺名称以及Logo" }, JsonRequestBehavior.AllowGet);
                    }
                    break;
                case (int)TmpType.小未平台子模版:
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(appId, 2);
                    if (platStore == null)
                        return Json(new { isok = false, msg = "找不到该店铺" }, JsonRequestBehavior.AllowGet);

                    modelId = platStore.Aid;
                    imginfo = ConfParamBLL.SingleModel.GetModelByParamappid("logoimg", xcx.AppId);
                    if (imginfo == null)
                    {
                        return Json(new { isok = false, msg = "请先到小程序管理配置底部Logo" }, JsonRequestBehavior.AllowGet);
                    }
                    logo = imginfo.Value;

                    XUserList = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcx.AppId);
                    if (XUserList == null)
                    {
                        return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
                    }
                    name = XUserList.nick_name;
                    cardType = 5;
                    center_app_brand_pass = "pages/my/my-index/index";//个人中心页面
                    custom_app_brand_pass = "pages/home/shop-detail/index";//首页

                    break;
            }

            VipConfig config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (config == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(logo) || string.IsNullOrEmpty(name))
            {
                return Json(new { isok = false, msg = "操作失败(请先配置店铺信息Logo以及店铺名称)" }, JsonRequestBehavior.AllowGet);
            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={modelId} and Type={cardType}");
            if (_vipWxCard == null)
                return Json(new { isok = false, msg = "非法操作(请先创建卡套)" }, JsonRequestBehavior.AllowGet);



            string carTitle = name;
            if (carTitle.Length > 6)
            {
                carTitle = carTitle.Substring(0, 6);
            }
            var updateCard = new
            {
                card_id = _vipWxCard.CardId,
                member_card = new
                {
                    base_info = new
                    {
                        title = carTitle + "会员卡",
                        logo_url = logo,
                        center_app_brand_pass = center_app_brand_pass,
                        custom_app_brand_pass = custom_app_brand_pass
                    }
                }

            };

            string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(_vipWxCard.User_Name);
            string updateCardJson = JsonConvert.SerializeObject(updateCard);
            string authorizer_Access_Token = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);
            string updateResult = Context.PostData($"https://api.weixin.qq.com/card/update?access_token={authorizer_Access_Token}", updateCardJson);

            UpdateWxCard _updateWxCard = JsonConvert.DeserializeObject<UpdateWxCard>(updateResult);
            if (_updateWxCard.errcode == 0)
                return Json(new { isok = true, msg = "更新成功", obj = _updateWxCard }, JsonRequestBehavior.AllowGet);
            return Json(new { isok = false, msg = "更新失败", obj = _updateWxCard }, JsonRequestBehavior.AllowGet);

        }
        
        /// <summary>
        /// 删除规则
        /// </summary>
        /// <returns></returns>
        public ActionResult delRule()
        {
            int id = Context.GetRequestInt("id", 0);
            int appid = Context.GetRequestInt("appId", 0);
            if (id <= 0 || appid <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appid, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            VipRule rule = VipRuleBLL.SingleModel.GetModel($"id={id} and appid='{xcx.AppId}' and state>=0");
            if (rule == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }
            rule.state = -1;
            rule.updatetime = DateTime.Now;
            bool isok = VipRuleBLL.SingleModel.Update(rule, "state,updatetime");
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 保存规则
        /// </summary>
        /// <returns></returns>
        public ActionResult saveRuleList()
        {
            string ruleliststr = Context.GetRequest("rulelist", string.Empty);
            int appid = Context.GetRequestInt("appId", 0);
            int ruleType = Context.GetRequestInt("ruleType", 0);
            if (string.IsNullOrEmpty(ruleliststr) || appid <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appid, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            #region 专业版版本控制功能
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                return Json(new { isok = false, msg = $"找不到模板" }, JsonRequestBehavior.AllowGet);

            }
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                int industr = xcx.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                if (functionList == null)
                {
                    return Json(new { isok = false, msg = $"功能权限未设置" }, JsonRequestBehavior.AllowGet);

                }
                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }

                if (operationMgr.VIP == 1)
                {
                    return Json(new { isok = false, msg = $"请先升级到更高版本才能开启此功能" }, JsonRequestBehavior.AllowGet);
                }

            }
            #endregion

            List<VipRule> ruleList = null;
            try
            {
                ruleList = JsonConvert.DeserializeObject<List<VipRule>>(ruleliststr);
            }
            catch
            {
                return Json(new { isok = false, msg = "参数错误！" }, JsonRequestBehavior.AllowGet);
            }
            if (ruleList == null || ruleList.Count <= 0)
            {
                return Json(new { isok = false, msg = "参数错误error" }, JsonRequestBehavior.AllowGet);
            }
            if (ruleList.Where(r => r.appId != xcx.AppId).ToList().Count > 0)
            {
                return Json(new { isok = false, msg = "数据异常" }, JsonRequestBehavior.AllowGet);
            }
            foreach (VipRule rule in ruleList)
            {
                if (rule.minMoney > rule.maxMoney)
                {
                    return Json(new { isok = false, msg = "规则设置消费金额范围错误" }, JsonRequestBehavior.AllowGet);
                }
                int count = ruleList.Where(r => r.levelid == rule.levelid).ToList().Count;
                if (count > 1)
                {
                    return Json(new { isok = false, msg = "有多条规则包含同一会员级别" }, JsonRequestBehavior.AllowGet);
                }
            }
            bool isok = VipRuleBLL.SingleModel.saveRuleList(ruleList);
            string msg = isok ? "保存成功" : "保存失败";
            if (isok)
            {
                int saveMoneyType = Context.GetRequestInt("saveMoneyType", -1);
                if (ruleType == 1 && saveMoneyType > -1)
                {

                    VipConfig vipConfig = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
                    if (vipConfig != null)
                    {
                        vipConfig.SaveMoneyType = saveMoneyType;
                        VipConfigBLL.SingleModel.Update(vipConfig, "SaveMoneyType");
                    }
                }

                ruleList = VipRuleBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0 and RuleType={ruleType}");
            }
            return Json(new { isok = isok, msg = msg, ruleList = ruleList }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 会员列表
        /// </summary>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult VipList(int PageType = 22)
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });


            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;

            int versionId = 0;
            int VIPSwitch = 0;
            int saveMoneySwitch = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcx.VersionId;
                FunctionList functionList = new FunctionList();
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });

                }
                OperationMgr operationMgr = new OperationMgr();
                if (!string.IsNullOrEmpty(functionList.OperationMgr))
                {
                    operationMgr = JsonConvert.DeserializeObject<OperationMgr>(functionList.OperationMgr);
                }

                VIPSwitch = operationMgr.VIP;
                saveMoneySwitch = operationMgr.SaveMoney;


            }
            ViewBag.versionId = versionId;
            ViewBag.VIPSwitch = VIPSwitch;
            ViewBag.saveMoneySwitch = saveMoneySwitch;



            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            List<VipLevel> levelList = VipLevelBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            //log4net.LogHelper.WriteInfo(this.GetType(), $"appid='{xcx.AppId}' and state>=0");
            if (levelList == null) levelList = new List<VipLevel>();
            return View(levelList);
        }

        /// <summary>
        /// 获取会员列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetVipList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string username = Context.GetRequest("username", string.Empty);
            int levelid = Context.GetRequestInt("levelid", 0);
            int leveltype = Context.GetRequestInt("leveltype", -1);
            string startDate = Context.GetRequest("startDate", string.Empty);
            string endDate = Context.GetRequest("endDate", string.Empty);
            string telePhone = Context.GetRequest("telePhone", string.Empty);
            try
            {
                MiniappVipInfo model = VipRelationBLL.SingleModel.GetVipList(xcx.AppId, pageIndex, pageSize, username, levelid, leveltype, startDate, endDate, telePhone);
                return Json(new { isok = true, model = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 删除会员信息
        /// </summary>
        /// <returns></returns>
        public ActionResult DelVipInfo()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            int vid = Context.GetRequestInt("vid", 0);
            if (vid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙vid_null" }, JsonRequestBehavior.AllowGet);
            }
            VipRelation viprelation = VipRelationBLL.SingleModel.GetModel($"id={vid} and state>=0 and appid='{xcx.AppId}'");
            if (viprelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙data_null" }, JsonRequestBehavior.AllowGet);
            }
            bool isok = VipRelationBLL.SingleModel.DelModel(viprelation);
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 修改会员等级
        /// </summary>
        /// <returns></returns>
        public ActionResult saveEdit()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int actionType = Context.GetRequestInt("actionType", 0);//表示默认
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            
            int vid = Context.GetRequestInt("vid", 0);
            if (vid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙vid_null" }, JsonRequestBehavior.AllowGet);
            }
            VipRelation viprelation = VipRelationBLL.SingleModel.GetModel($"id={vid} and appid='{xcx.AppId}' and state>=0");
            if (viprelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙viprelation" }, JsonRequestBehavior.AllowGet);
            }
            if (actionType > 0)
            {
                string remark = Context.GetRequest("remark",string.Empty);
                C_UserInfo c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(viprelation.uid);
                if (c_UserInfo == null)
                {
                    return Json(new { isok = false, msg = "用户信息不存在" }, JsonRequestBehavior.AllowGet);
                }
                c_UserInfo.Remark = remark;
                if(C_UserInfoBLL.SingleModel.Update(c_UserInfo, "Remark"))
                {
                    return Json(new { isok = true, msg = "更新用户备注成功" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = false, msg = "更新用户备注异常" }, JsonRequestBehavior.AllowGet);
                }

                //表示更新备注
            }
            int levelid = Context.GetRequestInt("levelid", 0);
            if (levelid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙levelid_null" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={levelid} and appid='{xcx.AppId}' and state>=0");
           
            if (levelinfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙levelinfo" }, JsonRequestBehavior.AllowGet);
            }
            viprelation.levelid = levelinfo.Id;
            viprelation.updatetime = DateTime.Now;
            bool isok = VipRelationBLL.SingleModel.Update(viprelation, "levelid,updatetime");
            string msg = isok ? "修改成功" : "修改失败";
            return Json(new { isok = isok, msg = msg, levelinfo = levelinfo }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult InsteadCardAuth(int appId = 0, int PageType = 22)
        {
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }
            ViewBag.appId = appId;

            int modelId = 0;
            ViewBag.PageType = PageType;
            int cardType = 0;
            switch (PageType)
            {
                case (int)TmpType.小程序专业模板:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(appId);
                    if (ent == null)
                        return View("PageError", new Return_Msg() { Msg = "找不到该专业版!", code = "500" });
                    modelId = ent.aid;
                    cardType = 2;
                    break;
                case (int)TmpType.小程序电商模板:
                    //表示电商版
                    Store store = StoreBLL.SingleModel.GetModelByAId(appId);
                    if (store == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "500" });
                    }
                    modelId = store.Id;
                    ViewBag.Logo = store.logo;
                    ViewBag.FoodsName = store.name;
                    break;
            }

            string ak = string.Empty;


            List<PrimaryCategoryItem> listCategory = new List<PrimaryCategoryItem>();
            List<object> PrimaryCategorys = new List<object>();
            List<object> Categorys = new List<object>();
            bool akIsUse = false;
            int getCategoryCount = 0;
            while (!akIsUse && getCategoryCount <= 3)
            {
                WxHelper.GetToken("wx64f161aa79a6801b", "65d0158981a05224eab3d690c36f035e", false);
                if (string.IsNullOrEmpty(ak))
                {
                    ak = WxHelper.GetToken("wx64f161aa79a6801b", "65d0158981a05224eab3d690c36f035e", true);
                }

                listCategory = WxHelper.GetMiniAppCategory(ak, appId).category;
                ViewBag.listCategoryJson = JsonConvert.SerializeObject(listCategory);
                if (listCategory != null && listCategory.Count > 0)
                {
                    akIsUse = true;
                }
                getCategoryCount++;//尝试3次,如果还获取不到则放弃

            }
            if (listCategory != null && listCategory.Count > 0)
            {
                foreach (PrimaryCategoryItem item in listCategory)
                {
                    PrimaryCategorys.Add(new { id = item.primary_category_id, name = item.category_name });
                    Categorys.Add(new { id = item.primary_category_id, secondCategory = item.secondary_category });
                }
            }


            ViewBag.getCategoryCount = getCategoryCount;
            ViewBag.ak = ak;
            ViewBag.PrimaryCategorys = PrimaryCategorys;
            ViewBag.Categorys = Categorys;
            
            List<C_Attachment> AuthLetters = C_AttachmentBLL.SingleModel.GetListByCache(modelId, (int)AttachmentItemType.小程序微信会员卡授权函图片);
            if (AuthLetters != null && AuthLetters.Count == 1)
            {
                ViewBag.AuthLetterObj = new { id = AuthLetters[0].id, url = AuthLetters[0].filepath };
                ViewBag.AuthLetter = AuthLetters[0].filepath;
            }

            List<C_Attachment> AgreementFiles = C_AttachmentBLL.SingleModel.GetListByCache(modelId, (int)AttachmentItemType.小程序微信会员卡营业执照图片);
            if (AgreementFiles != null && AgreementFiles.Count == 1)
            {
                ViewBag.AgreementFileObj = new { id = AgreementFiles[0].id, url = AgreementFiles[0].filepath };
                ViewBag.AgreementFile = AgreementFiles[0].filepath;
            }
            List<C_Attachment> OperatorFiles = C_AttachmentBLL.SingleModel.GetListByCache(modelId, (int)AttachmentItemType.小程序微信会员卡个体身份证图片);
            if (OperatorFiles != null && OperatorFiles.Count == 1)
            {
                ViewBag.OperatorFileObj = new { id = OperatorFiles[0].id, url = OperatorFiles[0].filepath };
                ViewBag.OperatorFile = OperatorFiles[0].filepath;
            }

            VipInsteadCardAuth vipInsteadCard = VipInsteadCardAuthBLL.SingleModel.GetModel($"type={cardType} and AppId={modelId}");
            if (vipInsteadCard == null)
                vipInsteadCard = new VipInsteadCardAuth();

            return View(vipInsteadCard);
        }

        public ActionResult SaveInsteadCardAuth(int appId = 0, int PageType = 22, int Id = 0)
        {
            if (appId == 0)
            {
                return Json(new { isok = false, msg = "appId非法" });
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "登录信息超时" });
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙!" });
            }

            string FoodsName = Context.GetRequest("FoodsName", string.Empty);
            string Logo = Context.GetRequest("Logo", string.Empty);
            int primary_category_id = Context.GetRequestInt("primary_category_id", -1);
            int secondary_category_id = Context.GetRequestInt("secondary_category_id", -1);
            string AuthLetter = Context.GetRequest("AuthLetter", string.Empty);
            string AgreementFile = Context.GetRequest("AgreementFile", string.Empty);
            string OperatorFile = Context.GetRequest("OperatorFile", string.Empty);
            int authorize_type = Context.GetRequestInt("authorize_type", -1);
            string End_Time = Context.GetRequest("End_Time", string.Empty);

            if (FoodsName == "" || Logo == "")
                return Json(new { isok = false, msg = "请先到店铺信息配置店铺名称和Logo!" });
            if (primary_category_id < 0 || secondary_category_id < 0)
                return Json(new { isok = false, msg = "请选择类目信息!" });
            if (authorize_type == -1)
                return Json(new { isok = false, msg = "请选择认证类型!" });
            if (AuthLetter == "")
                return Json(new { isok = false, msg = "请先上传授权函!" });
            if (string.IsNullOrEmpty(End_Time))
                return Json(new { isok = false, msg = "请先选择授权函截止时间并且与上传的授权函上的日期一致!" });


            if (authorize_type == 0)
            {
                //无公章授权函
                if (AgreementFile == "" || OperatorFile == "")
                    return Json(new { isok = false, msg = "你选择的是无公章的授权函,请先上传身份证以及营业执照!" });

            }



            VipInsteadCardAuth _VipInsteadCardAuth = new VipInsteadCardAuth();
            if (Id > 0)
            {
                _VipInsteadCardAuth = VipInsteadCardAuthBLL.SingleModel.GetModel(Id);
                if (_VipInsteadCardAuth == null)
                    return Json(new { isok = false, msg = "数据不存在!" });
                //表示修改更新 重新提交审核

            }
            else
            {
                //表示新增 第一次提交审核
                int modelId = 0;
                switch (PageType)
                {
                    case (int)TmpType.小程序专业模板:
                        EntSetting ent = EntSettingBLL.SingleModel.GetModel(appId);
                        if (ent == null)
                            return View("PageError", new Return_Msg() { Msg = "找不到该专业版!", code = "500" });
                        modelId = ent.aid;
                        _VipInsteadCardAuth.Type = 2;
                        break;
                    case (int)TmpType.小程序电商模板:
                        //表示电商版
                        Store store = StoreBLL.SingleModel.GetModelByAId(appId);
                        if (store == null)
                        {
                            return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "500" });
                        }
                        modelId = store.Id;
                        _VipInsteadCardAuth.Type = 0;
                        break;
                }
                _VipInsteadCardAuth.AppId = modelId;
            }

            string ak = WxHelper.GetToken("wx64f161aa79a6801b", "65d0158981a05224eab3d690c36f035e", false);
            //if (string.IsNullOrEmpty(ak))
            //{
            //    ak = WxHelper.GetToken("wx64f161aa79a6801b", "65d0158981a05224eab3d690c36f035e", true);
            //}

            #region 上传Logo到微信 得到微信那边的链接
            string uploadImgResult = CommondHelper.WxUploadImg(ak, Logo);
            if (!uploadImgResult.Contains("url"))
            {
                return Json(new { isok = false, msg = "操作失败(上传Logo到微信Error)", obj = uploadImgResult, ak = ak, Logo = Logo }, JsonRequestBehavior.AllowGet);
            }
            WxUploadImgResult r = JsonConvert.DeserializeObject<WxUploadImgResult>(uploadImgResult);
            _VipInsteadCardAuth.Logo_Url = r.url;
            #endregion

            #region 上传授权函到微信临时素材 得到微信那边返回临时素材Id  媒体文件在微信后台保存时间为3天，即3天后media_id失效。
            uploadImgResult = CommondHelper.WxUploadTemImg(ak, AuthLetter);

            if (uploadImgResult.Contains("errcode"))
            {
                return Json(new { isok = false, msg = "操作失败(上传授权函到临时素材Error)" + uploadImgResult, obj = uploadImgResult }, JsonRequestBehavior.AllowGet);
            }
            WxUploadTemImgResult wxUploadTemImgResult = JsonConvert.DeserializeObject<WxUploadTemImgResult>(uploadImgResult);
            _VipInsteadCardAuth.Protocol = wxUploadTemImgResult.media_id;
            #endregion

            object submitObj = new object();
            _VipInsteadCardAuth.Brand_Name = FoodsName;
            _VipInsteadCardAuth.End_Time = WxHelper.TimeToUnix(Convert.ToDateTime(End_Time));
            _VipInsteadCardAuth.Primary_Category_Id = primary_category_id;
            _VipInsteadCardAuth.Secondary_Category_Id = secondary_category_id;
            _VipInsteadCardAuth.AuthLetterType = authorize_type;

            if (authorize_type == 0)
            {
                //授权函无公章的需要上传营业执照以及身份证到微信的临时素材
                uploadImgResult = CommondHelper.WxUploadTemImg(ak, AgreementFile);
                if (uploadImgResult.Contains("errcode"))
                {
                    return Json(new { isok = false, msg = "操作失败(上传营业执照到临时素材Error)" + uploadImgResult, obj = uploadImgResult }, JsonRequestBehavior.AllowGet);
                }
                wxUploadTemImgResult = JsonConvert.DeserializeObject<WxUploadTemImgResult>(uploadImgResult);
                _VipInsteadCardAuth.Agreement_Media_Id = wxUploadTemImgResult.media_id;

                uploadImgResult = CommondHelper.WxUploadTemImg(ak, OperatorFile);
                if (uploadImgResult.Contains("errcode"))
                {
                    return Json(new { isok = false, msg = "操作失败(上传身份证到临时素材Error)" + uploadImgResult, obj = uploadImgResult }, JsonRequestBehavior.AllowGet);
                }
                wxUploadTemImgResult = JsonConvert.DeserializeObject<WxUploadTemImgResult>(uploadImgResult);
                _VipInsteadCardAuth.Operator_Media_Id = wxUploadTemImgResult.media_id;


                submitObj = new
                {
                    info = new
                    {
                        brand_name = _VipInsteadCardAuth.Brand_Name,
                        logo_url = _VipInsteadCardAuth.Logo_Url,
                        protocol = _VipInsteadCardAuth.Protocol,
                        agreement_media_id = _VipInsteadCardAuth.Agreement_Media_Id,
                        operator_media_id = _VipInsteadCardAuth.Operator_Media_Id,
                        end_time = _VipInsteadCardAuth.End_Time,
                        primary_category_id = _VipInsteadCardAuth.Primary_Category_Id,
                        secondary_category_id = _VipInsteadCardAuth.Secondary_Category_Id
                    }
                };

            }
            else
            {
                submitObj = new
                {
                    info = new
                    {
                        brand_name = _VipInsteadCardAuth.Brand_Name,
                        logo_url = _VipInsteadCardAuth.Logo_Url,
                        protocol = _VipInsteadCardAuth.Protocol,
                        end_time = _VipInsteadCardAuth.End_Time,
                        primary_category_id = _VipInsteadCardAuth.Primary_Category_Id,
                        secondary_category_id = _VipInsteadCardAuth.Secondary_Category_Id
                    }
                };
            }





            string submitJson = JsonConvert.SerializeObject(submitObj);//提交创建子商户的数据
            string resultJson = WxHelper.DoPostJson($"https://api.weixin.qq.com/card/submerchant/submit?access_token={ak}", submitJson);
            if (!resultJson.Contains("merchant_id"))
            {
                return Json(new { isok = false, msg = "创建子商户失败", resultJson = resultJson, submitObj = submitJson });
            }

            VipInsteadCardAuthResult vipInsteadCardAuthResult = JsonConvert.DeserializeObject<VipInsteadCardAuthResult>(resultJson);
            if (vipInsteadCardAuthResult == null)
                return Json(new { isok = false, msg = "创建子商户失败(反序列化失败)", obj = resultJson });

            _VipInsteadCardAuth.CreateDate = WxHelper.UnixToTime(vipInsteadCardAuthResult.info.create_time);
            _VipInsteadCardAuth.UpdateDate = WxHelper.UnixToTime(vipInsteadCardAuthResult.info.update_time);
            _VipInsteadCardAuth.Status = vipInsteadCardAuthResult.info.status;

            if (Id > 0)
            {
                if (VipInsteadCardAuthBLL.SingleModel.Update(_VipInsteadCardAuth))
                {
                    return Json(new { isok = true, msg = "提交成功(等待更新审核)", obj = resultJson });
                }
                else
                {
                    return Json(new { isok = false, msg = "更新到数据库失败", obj = resultJson });
                }
                //表示更新
            }
            else
            {
                if (Convert.ToInt32(VipInsteadCardAuthBLL.SingleModel.Add(_VipInsteadCardAuth)) > 0)
                {
                    return Json(new { isok = true, msg = "提交成功(等待审核)", obj = resultJson });
                }
                else
                {
                    return Json(new { isok = false, msg = "插入到数据库失败", obj = resultJson });
                }
                //表示新增
            }



        }






        #endregion
    }
}