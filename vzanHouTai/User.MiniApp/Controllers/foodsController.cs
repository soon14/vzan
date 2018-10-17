using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Helper;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.User;
using Entity.MiniApp.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using User.MiniApp.Filters;
using Utility;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class MiappFoodsController : foodsController
    {

    }
    public class foodsController : configController
    {

        #region 私有BLL成员
        
        

        
        
        
        

        
        

        
        
        

        
        
        
        
        
        
        
        

        
        
        






     
        
        protected readonly DadaOrderBLL _dadaOrderBLL;


        

        protected Return_Msg result;

        #endregion

        public foodsController()
        {
            _dadaOrderBLL = new DadaOrderBLL();
            result = new Return_Msg();
            result.isok = false;
            result.Msg = "请求失败";
        }

        #region 新接口

        /// <summary>
        /// 餐馆配置
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult Index_Food()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int pageType = Context.GetRequestInt("pageType", 8);

            Food storeModel = FoodBLL.SingleModel.GetModelByAppId(appId, storeId) ?? new Food() { appId = appId };


            ViewBag.appId = appId;
            ViewBag.storeId = storeModel.Id;
            ViewBag.PageType = pageType;

            ViewBag.havMasterStoreRole = UserRoleBLL.SingleModel.HavingRole(dzuserId, RoleType.总店管理员, appId); //总店管理员权限

            #region 扫码绑定门店管理员二维码参数设定
            var sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["qrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = true;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
            }
            Session["appId"] = storeModel.appId;
            Session["storeId"] = storeModel.Id;
            #endregion

            return View(storeModel);
        }

        /// <summary>
        /// 添加编辑店铺信息
        /// </summary>
        /// <param name="food"></param>
        /// <param name="storeImgs"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        [LoginFilter, HttpPost, ValidateInput(false)]
        public ActionResult SaveFoodInfo(Food storeModel, List<C_Attachment> storeImgs, string colName = "")
        {
            if (storeModel == null)
            {
                return Json(result);
            }

            if (storeModel.Id > 0) //编辑
            {
                //解决空字符串  FoodBLL.SingleModel.BuildUpdateSql(food, cloumns) 报错
                if (string.IsNullOrWhiteSpace(storeModel.Notice))
                {
                    storeModel.Notice = " ";
                }
                if (colName.IsNullOrWhiteSpace())
                {

                    colName = "FoodsName,Province,CityCode,AreaCode,Address,TelePhone,UpdateDate,Notice,StartShopTime,EndShopTime,GiveWays,Lng,Lat,DeliveryRange,OpenDateStr,TakeOut,TheShop,OutSide,ShippingFee,OpenTimeJson,AutoAcceptOrder,Logo";
                }

                storeModel.UpdateDate = DateTime.Now;
                bool isSuccess = FoodBLL.SingleModel.Update(storeModel, colName);
                if (!isSuccess)
                {
                    result.Msg = "更新失败 !";
                    return Json(result);
                }
            }
            else //添加
            {
                storeModel.CreateDate = DateTime.Now;
                storeModel.UpdateDate = DateTime.Now;

                storeModel.Id = Convert.ToInt32(FoodBLL.SingleModel.Add(storeModel));
            }

            #region logo 存入附件表
            //判断上传图片是否以http开头，不然为破图
            if (!string.IsNullOrEmpty(storeModel.Logo) && storeModel.Logo.IndexOf("http://", StringComparison.Ordinal) == 0)
            {
                //门店照片
                List<C_Attachment> logo = C_AttachmentBLL.SingleModel.GetListByCache(storeModel.Id, (int)AttachmentItemType.小程序餐饮店铺Logo) ?? new List<C_Attachment>();
                if (!logo.Any(l => !l.filepath.Equals(storeModel.Logo)))
                {
                    C_AttachmentBLL.SingleModel.Add(new C_Attachment
                    {
                        itemId = storeModel.Id,
                        createDate = DateTime.Now,
                        filepath = storeModel.Logo,
                        itemType = (int)AttachmentItemType.小程序餐饮店铺Logo,
                        #region 小程序餐饮店铺Logo尺寸改成300*300   /*640*360*/
                        thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(storeModel.Logo, 300, 300),
                        #endregion
                        status = 0
                    });

                    //删除旧附件
                    logo.FindAll(l => !l.filepath.Equals(logo)).ForEach(l =>
                    {
                        l.status = -1;
                        C_AttachmentBLL.SingleModel.Update(l, "status");
                    });

                    //清除门店logo缓存
                    C_AttachmentBLL.SingleModel.RemoveRedis(storeModel.Id, (int)AttachmentItemType.小程序餐饮店铺Logo);
                    string key = string.Format(FoodBLL.SingleModel.foodAttImgKey, storeModel.Id, (int)AttachmentItemType.小程序餐饮店铺Logo);
                    RedisUtil.Remove(key);
                }
            }
            #endregion

            #region 门店图片
            //门店照片
            List<C_Attachment> imgs = C_AttachmentBLL.SingleModel.GetListByCache(storeModel.Id, (int)AttachmentItemType.小程序餐饮门店图片) ?? new List<C_Attachment>();
            //删除旧附件
            if (storeImgs?.Count > 0)
            {
                storeImgs.ForEach(i =>
                {
                    if (i.id == 0) //新加的数据
                    {
                        C_AttachmentBLL.SingleModel.Add(new C_Attachment
                        {
                            itemId = storeModel.Id,
                            createDate = DateTime.Now,
                            filepath = i.filepath,
                            itemType = (int)AttachmentItemType.小程序餐饮门店图片,
                            //小程序餐饮店铺轮播图尺寸700*400
                            thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(i.filepath, 700, 400),
                            status = 0
                        });
                    }
                });

                //已有的数据被删除
                imgs.ForEach(i =>
                {
                    if (!storeImgs.Any(si => si.id == i.id))
                    {
                        i.status = -1;
                        C_AttachmentBLL.SingleModel.Update(i, "status");
                    }
                });
            }
            else //全部删除
            {
                imgs.ForEach(i =>
                {
                    i.status = -1;
                    C_AttachmentBLL.SingleModel.Update(i, "status");
                });
            }

            //清除门店图片缓存
            C_AttachmentBLL.SingleModel.RemoveRedis(storeModel.Id, (int)AttachmentItemType.小程序餐饮门店图片);
            string key_ = string.Format(FoodBLL.SingleModel.foodAttImgKey, storeModel.Id, (int)AttachmentItemType.小程序餐饮门店图片);
            RedisUtil.Remove(key_);
            #endregion



            result.isok = true;
            result.Msg = "更新成功";
            return Json(result);
        }


        #endregion

        #region 分店管理员设定及解绑
        /// <summary>
        /// 门店管理者绑定微信
        /// </summary>
        /// <param name="wxkey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult StoreManagerBindWx(string wxkey = "")
        {
            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
            if (string.IsNullOrEmpty(wxkey))
            {
                return Json(new { success = false, msg = "没有wxkey！" }, JsonRequestBehavior.AllowGet);
            }
            if (lcode == null)
            {
                return Json(new { success = false, msg = "lcode为NULL！" }, JsonRequestBehavior.AllowGet);
            }
            if (!lcode.IsLogin)
            {
                return Json(new { success = false, msg = "未登录！" }, JsonRequestBehavior.AllowGet);
            }

            if (lcode.WxUser != null)
            {
                //已扫描
                Account accountmodel = null;
                if (!string.IsNullOrEmpty(lcode.WxUser.openid))
                {
                    
                    UserBaseInfo userInfo = UserBaseInfoBLL.SingleModel.GetModelByOpenId(lcode.WxUser.openid, lcode.WxUser.serverid);
                    if (userInfo == null)
                    {
                        userInfo = new UserBaseInfo();
                        userInfo.openid = lcode.WxUser.openid;
                        userInfo.nickname = lcode.WxUser.nickname;
                        userInfo.headimgurl = lcode.WxUser.headimgurl;
                        userInfo.sex = lcode.WxUser.sex;
                        userInfo.country = lcode.WxUser.country;
                        userInfo.city = lcode.WxUser.city;
                        userInfo.province = lcode.WxUser.province;
                        userInfo.unionid = lcode.WxUser.unionid;
                        UserBaseInfoBLL.SingleModel.Add(userInfo);
                    }
                    accountmodel = AccountBLL.SingleModel.GetAccountByWeixinUser(lcode.WxUser);
                    if (accountmodel == null)
                    {
                        return Json(new { success = true, msg = "找不到绑定账号！" }, JsonRequestBehavior.AllowGet);
                    }
                    
                    Member member = MemberBLL.SingleModel.GetModel(string.Format("AccountId ='{0}'", accountmodel.Id.ToString()));
                    member.LastModified = DateTime.Now;//记录登录时间
                    MemberBLL.SingleModel.Update(member);
                    RedisUtil.Remove("SessionID:" + wxkey);

                    Int32 appId = Convert.ToInt32(Session["appId"] ?? "0");
                    Int32 storeId = Convert.ToInt32(Session["storeId"] ?? "0");

                    bool havingRole = UserRoleBLL.SingleModel.HavingRole(RoleType.分店管理员, appId, storeId);
                    if (havingRole)
                    {
                        return Json(new { success = true, msg = "该店铺已经有管理者,请解绑后再重新绑定！", isBind = true }, JsonRequestBehavior.AllowGet);
                    }

                    //添加当前扫码用户于当前店铺的管理权限
                    TransactionModel tran = new TransactionModel();
                    tran.Add(UserRoleBLL.SingleModel.BuildAddSql(new UserRole()
                    {
                        RoleId = (int)RoleType.分店管理员,
                        AppId = appId,
                        StoreId = storeId,
                        State = 1,
                        UserId = accountmodel.Id,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    }));
                    Food food = FoodBLL.SingleModel.GetModel(storeId);
                    if (food == null)
                    {
                        return Json(new { success = true, msg = "没有找到店铺信息,绑定失败！" }, JsonRequestBehavior.AllowGet);
                    }
                    food.masterNickName = lcode.WxUser.nickname;
                    tran.Add(FoodBLL.SingleModel.BuildUpdateSql(food, "masterNickName"));
                    bool isSuccess = UserRoleBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray);
                    if (!isSuccess)
                    {
                        return Json(new { success = true, msg = "加入数据库失败！" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = true, msg = "绑定成功！", ShopManagerName = lcode.WxUser.nickname, ShopManager = accountmodel.LoginId }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { success = false, msg = "找不到openid！" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
            }
        }



        /// <summary>
        /// 解除店铺权限
        /// </summary>
        /// <returns></returns>
        [LoginFilter, HttpPost]
        public ActionResult RemoveUserStoreManagerRole()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);

            TransactionModel tran = new TransactionModel();
            //删除用户在这个模板里的权限
            var deleteRoleSql = $" update userRole set State = 0 Where Appid = {appId} and storeId = {storeId} and RoleId = {(int)RoleType.分店管理员} and State = 1 ";
            tran.Add(deleteRoleSql);
            Food food = FoodBLL.SingleModel.GetModel(storeId);
            food.masterNickName = string.Empty;
            tran.Add(FoodBLL.SingleModel.BuildUpdateSql(food, "masterNickName"));
            bool isSuccess = UserRoleBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray);
            return Json(new { success = isSuccess, msg = isSuccess ? "解除权限成功!" : "解除权限失败!" });
        }


        #endregion
        
        #region 餐饮店铺栏目管理操作
        // GET: MiappFoods
        /// <summary>
        /// 餐馆配置
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int? appId)
        {
            if (appId.Value <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            ViewBag.appId = appId;
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId.Value, dzaccount.Id.ToString());
            if (umodel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }

            ViewBag.Attachmentvoicepath = "";
            ViewBag.VoiceType = (int)AttachmentItemType.小程序餐饮新订单提示语音;
            Food model = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (model != null)
            {
                //店铺Logo

                List<C_Attachment> imgs = C_AttachmentBLL.SingleModel.GetListByCache(model.Id, (int)AttachmentItemType.小程序餐饮店铺Logo);

                if (imgs != null && imgs.Count > 0)
                {
                    ViewBag.Logo = new List<object>() { new { id = imgs[0].id, url = imgs[0].filepath } };
                }



                //门店图片

                List<object> storePicObjList = new List<object>();

                imgs = C_AttachmentBLL.SingleModel.GetListByCache(model.Id, (int)AttachmentItemType.小程序餐饮门店图片);
                if (imgs != null && imgs.Count > 0)
                {

                    imgs.ForEach(x =>
                    {
                        storePicObjList.Add(new { id = x.id, url = x.filepath });
                    });
                }


                ViewBag.storePicList = storePicObjList;
                ViewBag.Attachmentvoicepath = model.VoiceUrl;


            }
            else
            {
                model = new Food();
                model.appId = appId.Value;
                int Id = Convert.ToInt32(FoodBLL.SingleModel.Add(model));
                if (Id > 0)
                    model.Id = Id;
                else
                    return View("PageError", new Return_Msg() { Msg = "后台初始化中!", code = "500" });
            }

            //ViewBag.XcxName = "";
            return View(model);
        }

        public ActionResult delAppQrcordNo()
        {
            string noId = Context.GetRequest("tablenoid", "0");
            FoodTable footTable = FoodTableBLL.SingleModel.GetModel(Convert.ToInt32(noId));
            if (footTable == null)
            {
                return Json(new { isok = -1, msg = "找不到此桌台号！" }, JsonRequestBehavior.AllowGet);
            }
            footTable.State = -1;
            FoodTableBLL.SingleModel.Update(footTable, "State");
            return Json(new { isok = 1, msg = "修改成功！" }, JsonRequestBehavior.AllowGet);
        }

        //添加编辑店铺信息
        [HttpPost, ValidateInput(false)]
        public ActionResult IndexAddOrEdit(Food food, string ImgList, string aptitudeImgList, string CouponList, string colName = "", int rid = 0, string StartShopTime = "8:00", string EndShopTime = "20:00")
        {
            ServiceResult result = new ServiceResult();
            try
            {
                if (dzaccount == null)
                {
                    return Json(result.IsFailed("系统繁忙auth_null !"));
                }
                if (rid <= 0)
                {
                    return Json(result.IsFailed("系统繁忙rid !"));
                }
                XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(food.appId, dzaccount.Id.ToString());
                if (umodel == null)
                {
                    return Json(result.IsFailed("系统繁忙umodel_null"));
                }
                if (string.IsNullOrWhiteSpace(food.FoodsName))
                {
                    return Json(result.IsFailed("请输入店铺名称 !"));
                }
                if (food.Id > 0)
                {
                    Food store = FoodBLL.SingleModel.GetModel(food.Id);
                    if (store == null)
                    {
                        return Json(new { isok = false, msg = "门店不存在" });
                    }
                    string colNames = "FoodsName,TelePhone,TelePhone,Address,Notice,OpenDateStr,Lng,Lat,AutoAcceptOrder,OpenTimeJson,OpenNewOrderPrompt,VoiceType,Logo";
                    if (food.VoiceType == 1)
                    {
                        colNames += ",VoiceUrl";
                        store.VoiceUrl = food.VoiceUrl;
                    }
                    List<FoodOpenTimeModel> openTimeList = store.getOpenTimeList;
                    openTimeList[0].StartTime = StartShopTime;
                    openTimeList[0].EndTime = EndShopTime;
                    store.OpenTimeJson = JsonConvert.SerializeObject(openTimeList);
                    if (string.IsNullOrWhiteSpace(food.Notice))
                    {
                        food.Notice = " ";
                    }
                   
                    store.UpdateDate = DateTime.Now;
                    store.FoodsName = food.FoodsName;
                    store.TelePhone = food.TelePhone;
                    store.Address = food.Address;
                    store.Notice = food.Notice;
                    store.OpenDateStr = food.OpenDateStr;
                    store.Lng = food.Lng;
                    store.Lat = food.Lat;
                    store.AutoAcceptOrder = food.AutoAcceptOrder;
                    store.OpenNewOrderPrompt = food.OpenNewOrderPrompt;
                    store.VoiceType = food.VoiceType;
                    if (!string.IsNullOrEmpty(food.Logo))
                    {
                        store.Logo = food.Logo;
                        colName += ",Logo";
                    }
                    log4net.LogHelper.WriteError(GetType(), new Exception($"{JsonConvert.SerializeObject(store)},{colName}"));
                    bool isSuccess = FoodBLL.SingleModel.Update(store, colName);
                  
                    if (!isSuccess)
                    {
                        return Json(result.IsFailed("更新失败 !"));
                    }

                    result.IsSucceed("更新成功 !");
                }
                else
                {
                    food.CreateDate = DateTime.Now;
                    food.UpdateDate = DateTime.Now;

                    int id = Convert.ToInt32(FoodBLL.SingleModel.Add(food));
                    food.Id = id;

                    result.IsSucceed("添加成功 !");
                }


                //判断上传图片是否以http开头，不然为破图
                if (!string.IsNullOrEmpty(food.Logo) && food.Logo.IndexOf("http://", StringComparison.Ordinal) == 0)
                {
                    C_AttachmentBLL.SingleModel.Add(new C_Attachment
                    {
                        itemId = food.Id,
                        createDate = DateTime.Now,
                        filepath = food.Logo,
                        itemType = (int)AttachmentItemType.小程序餐饮店铺Logo,
                        #region 小程序餐饮店铺Logo尺寸改成300*300   /*640*360*/
                        thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(food.Logo, 300, 300),
                        #endregion
                        status = 0
                    });
                }

                //门店图片
                string imgsStr = Context.GetRequest("imgs", string.Empty);
                List<string> storeImgs = new List<string>();
                if (!string.IsNullOrEmpty(imgsStr))
                {
                    storeImgs = imgsStr.Split(',').ToList();
                }
                if (storeImgs.Count > 6)
                {
                    return Json(new { isok = false, msg = "图片数量不能超过6张" }, JsonRequestBehavior.AllowGet);
                }
                //插入门店图片
                foreach (string imgUrl in storeImgs)
                {
                    C_AttachmentBLL.SingleModel.Add(new C_Attachment
                    {
                        itemId = food.Id,
                        createDate = DateTime.Now,
                        filepath = imgUrl,
                        itemType = (int)AttachmentItemType.小程序餐饮门店图片,
                        //小程序餐饮店铺轮播图尺寸700*400
                        thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(imgUrl, 700, 400),
                        status = 0
                    });
                }
                //清除门店图片缓存
                C_AttachmentBLL.SingleModel.RemoveRedis(food.Id, (int)AttachmentItemType.小程序餐饮门店图片);

                //清除门店logo缓存
                C_AttachmentBLL.SingleModel.RemoveRedis(food.Id, (int)AttachmentItemType.小程序餐饮店铺Logo);

                result.Data = new Dictionary<string, object> { { "Id", food.Id } };

                string key = string.Format(FoodBLL.SingleModel.foodAttImgKey, food.Id, (int)AttachmentItemType.小程序餐饮店铺Logo);
                RedisUtil.Remove(key);
                key = string.Format(FoodBLL.SingleModel.foodAttImgKey, food.Id, (int)AttachmentItemType.小程序餐饮门店图片);
                RedisUtil.Remove(key);
                return Json(result);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
                return Json(result.IsFailed("服务器出错 , 请重试 !"));
            }
        }


        //public ActionResult DelBangding(int Id)
        //{
        //    //string dmsg = "";
        //    if (dzaccount == null)
        //    {
        //        return Redirect("/dzhome/login");
        //    }

        //    if (Id <= 0)
        //    {
        //        return Json(new { isok = -1, msg = "系统繁忙id" }, JsonRequestBehavior.AllowGet);
        //    }
        //    XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
        //    if (xcxrelation == null)
        //    {
        //        return Json(new { isok = -1, msg = "系统繁忙xcxrelation_null" }, JsonRequestBehavior.AllowGet);
        //    }

        //    try
        //    {
        //        OpenAuthorizerConfig model = _openauthorizerconfigBll.GetModelByAppids(xcxrelation.AppId);
        //        if (model != null)
        //        {
        //            if (_openauthorizerconfigBll.Delete(model.id) <= 0)
        //            {
        //                return Json(new { isok = -1, msg = "解除绑定失败" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        xcxrelation.AppId = "";
        //        if (!XcxAppAccountRelationBLL.SingleModel.Update(xcxrelation, "AppId"))
        //        {
        //            return Json(new { isok = -1, msg = "解除绑定失败" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { isok = -1, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
        //    }

        //    return Json(new { isok = 1, msg = "解除绑定成功" }, JsonRequestBehavior.AllowGet);
        //}


        /// <summary>
        /// 店铺装修
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult Renovation()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);

            Food model = FoodBLL.SingleModel.GetModelByAppId(appId, storeId);
            if (model != null)
            {
                //店铺Logo

                List<C_Attachment> imgs = C_AttachmentBLL.SingleModel.GetListByCache(model.Id, (int)AttachmentItemType.小程序多门店版门店logo);
                if (imgs != null && imgs.Count > 0)
                {
                    ViewBag.Logo = new List<object>() { new { id = imgs[0].id, url = imgs[0].filepath } };
                }



                //门店图片
                List<object> storePicObjList = new List<object>();

                imgs = C_AttachmentBLL.SingleModel.GetListByCache(model.Id, (int)AttachmentItemType.小程序餐饮门店图片);
                if (imgs != null && imgs.Count > 0)
                {
                    //从缓存里获取
                    imgs.ForEach(x =>
                    {
                        storePicObjList.Add(new { id = x.id, url = x.filepath });
                    });
                }

                ViewBag.storePicList = storePicObjList;

                //轮播图
                List<object> sliderPicObjList = new List<object>();
                imgs = C_AttachmentBLL.SingleModel.GetListByCache(model.Id, (int)AttachmentItemType.小程序餐饮店铺轮播图);
                if (imgs != null && imgs.Count > 0)
                {
                    //从缓存里获取

                    imgs.ForEach(x =>
                    {
                        sliderPicObjList.Add(new { id = x.id, url = x.filepath });
                    });
                }

                ViewBag.sliderPicList = sliderPicObjList;
            }
            else
            {
                model = new Food();
                model.appId = appId;
                int Id = Convert.ToInt32(FoodBLL.SingleModel.Add(model));
                if (Id > 0)
                    model.Id = Id;
                else
                    return View("PageError", new Return_Msg() { Msg = "后台初始化中!", code = "500" });
            }


            ViewBag.appId = appId;
            ViewBag.storeId = storeId;
            return View(model);
        }

        /// <summary>
        /// 店铺装修保存
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveRenovation()
        {
            string imgsStr = Context.GetRequest("imgs", string.Empty);
            string infoStr = Context.GetRequest("info", string.Empty);
            if (string.IsNullOrEmpty(infoStr))
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }

            Food postInfo = null;
            try
            {
                postInfo = JsonConvert.DeserializeObject<Food>(infoStr);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Json(new { isok = false, msg = "系统繁忙json_error" }, JsonRequestBehavior.AllowGet);
            }
            if (postInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙Info_null" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={postInfo.appId}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" }, JsonRequestBehavior.AllowGet);
            }

            //轮播图
            List<string> sliderImgs = new List<string>();
            if (!string.IsNullOrEmpty(imgsStr))
            {
                sliderImgs = imgsStr.Split(',').ToList();
            }
            if (sliderImgs.Count > 4)
            {
                return Json(new { isok = false, msg = "图片数量不能超过4张" }, JsonRequestBehavior.AllowGet);
            }
            //插入轮播图
            foreach (string imgUrl in sliderImgs)
            {
                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                {
                    itemId = postInfo.Id,
                    createDate = DateTime.Now,
                    filepath = imgUrl,
                    itemType = (int)AttachmentItemType.小程序餐饮店铺轮播图,
                    //小程序餐饮店铺轮播图尺寸700*400
                    thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(imgUrl, 700, 400),
                    status = 0
                });
            }
            foodStore.configJson = postInfo.configJson;
            foodStore.UpdateDate = DateTime.Now;
            bool isok = FoodBLL.SingleModel.Update(foodStore, "configJson,updatedate");
            //清除轮播图缓存
            C_AttachmentBLL.SingleModel.RemoveRedis(foodStore.Id, (int)AttachmentItemType.小程序餐饮店铺轮播图);
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 外卖
        /// </summary>
        /// <returns></returns>
        public ActionResult TakeoutSetting()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (xcxrelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "500" });
            }

            Food model = FoodBLL.SingleModel.GetModelByAppId(appId, storeId);
            if (model == null)
            {
                model = new Food();
                model.appId = appId;
                int Id = Convert.ToInt32(FoodBLL.SingleModel.Add(model));
                if (Id > 0)
                    model.Id = Id;
                else
                    return View("PageError", new Return_Msg() { Msg = "后台初始化中!", code = "500" });
            }

            //达达配送
            DadaMerchant dadamerchant = DadaMerchantBLL.SingleModel.GetModelByRId(xcxrelation.Id);
            if (dadamerchant != null && dadamerchant.id > 0)
            {
                ViewBag.OpenDadaTake = 1;
            }
            else
            {
                ViewBag.OpenDadaTake = 0;
            }
            //快跑者
            KPZStoreRelation kpzstore = KPZStoreRelationBLL.SingleModel.GetModelBySidAndAid(appId,0);
            if(kpzstore!=null)
            {
                model.KPZPhone = kpzstore.TelePhone;
                model.KPZTeamToken = kpzstore.TeamToken;
            }
            List<FoodOpenTimeModel> openTimeList = model.getOpenTimeList;
            if (openTimeList.Count == 1)
            {
                openTimeList.Add(openTimeList[0]);
                openTimeList.Add(openTimeList[0]);
                model.OpenTimeJson = JsonConvert.SerializeObject(openTimeList);
                FoodBLL.SingleModel.Update(model, "OpenTimeJson");
            }
            else if (openTimeList.Count == 2)
            {
                openTimeList.Add(model.getOpenTimeList[0]);
                model.OpenTimeJson = JsonConvert.SerializeObject(openTimeList);
                FoodBLL.SingleModel.Update(model, "OpenTimeJson");
            }
            //uu配送
            UUCustomerRelation uuModel = UUCustomerRelationBLL.SingleModel.GetModelByAid(appId, 0, 0);
            if (uuModel == null)
            {
                uuModel = new UUCustomerRelation();
                uuModel.AId = appId;
            }
            model.UUModel = uuModel;

            ViewBag.appId = appId;
            ViewBag.storeId = storeId;
            return View(model);
        }


        public ActionResult ScanCode()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);

            Food model = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (model == null)
            {
                model = new Food();
                model.appId = appId;
                int Id = Convert.ToInt32(FoodBLL.SingleModel.Add(model));
                if (Id > 0)
                    model.Id = Id;
                else
                    return View("PageError", new Return_Msg() { Msg = "后台初始化中!", code = "500" });
            }
            ViewBag.TablesNoList = FoodTableBLL.SingleModel.GetList($" FoodId = {model.Id} and State >= 0 ") ?? new List<FoodTable>();
            ViewBag.appId = appId;
            ViewBag.storeId = storeId;
            List<FoodOpenTimeModel> openTimeList = model.getOpenTimeList;
            if (openTimeList.Count == 1)
            {
                openTimeList.Add(openTimeList[0]);
                openTimeList.Add(openTimeList[0]);
                model.OpenTimeJson = JsonConvert.SerializeObject(openTimeList);
                FoodBLL.SingleModel.Update(model, "OpenTimeJson");
            }
            else if (openTimeList.Count == 2)
            {
                openTimeList.Add(model.getOpenTimeList[0]);
                model.OpenTimeJson = JsonConvert.SerializeObject(openTimeList);
                FoodBLL.SingleModel.Update(model, "OpenTimeJson");
            }
            return View(model);
        }
        #endregion

        #region 删除图片
        //删除图片
        public ActionResult DeleteImg(int id=0)
        {
            try
            {
                return C_AttachmentBLL.SingleModel.Delete(id) > 0 ? Json(new { Success = true, Msg = "删除成功" }, JsonRequestBehavior.AllowGet)
                     : Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
            }
        }

        ////删除店铺logo图片
        //public ActionResult DeleteImgForStoreLogo(int id)
        //{
        //    try
        //    {
        //        return C_AttachmentBLL.SingleModel.Delete(id) > 0 ? Json(new { Success = true, Msg = "删除成功" }, JsonRequestBehavior.AllowGet)
        //             : Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        #endregion

        #region 小程序授权新
        /// <summary>
        /// 添加桌台号
        /// </summary>
        [HttpPost]
        public ActionResult addTables(int appId, string tablesNo)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null！" }, JsonRequestBehavior.AllowGet);
            }

            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "找不到店铺！" }, JsonRequestBehavior.AllowGet);
            }
            if (FoodTableBLL.SingleModel.Exists($" foodid = {miAppFood.Id} and Scene = '{tablesNo}' and State >= 0 "))
            {
                return Json(new { isok = false, msg = "已存在相同桌台号！" }, JsonRequestBehavior.AllowGet);
            }

            FoodTable newTable = new FoodTable();
            newTable.Scene = tablesNo;
            newTable.State = 0;
            newTable.FoodId = miAppFood.Id;
            newTable.AddTime = DateTime.Now;
            newTable.Id = Convert.ToInt32(FoodTableBLL.SingleModel.Add(newTable));
            if (newTable.Id <= 0)
            {
                return Json(new { isok = false, msg = "添加失败！" }, JsonRequestBehavior.AllowGet);
            }
            ConfigViewModel viewmodel = new ConfigViewModel();
            viewmodel.XcxTemplate = XcxTemplateBLL.SingleModel.GetModel(app.TId);
            //小程序二维码
            if (viewmodel.XcxTemplate != null)
            {
                string token = "";
                if (!XcxApiBLL.SingleModel.GetToken(app, ref token))
                {
                    return Json(new { isok = false, msg = token });
                }

                qrcodeclass result = CommondHelper.GetMiniAppQrcode(token, "pages/index/index", $"0#{newTable.Id.ToString()}",520);//0|用来区分新旧版二维码，给前端做不同逻辑处理
                if (result != null)
                {
                    if (result.isok > 0)
                    {
                        newTable.ImgUrl = result.url;
                    }
                    else
                    {
                        newTable.ImgUrl = result.msg;
                    }
                    //log4net.LogHelper.WriteInfo(GetType(), JsonConvert.SerializeObject(result));
                    //log4net.LogHelper.WriteError(GetType(), new Exception(JsonConvert.SerializeObject(result)));
                }
            }
            bool isok = FoodTableBLL.SingleModel.Update(newTable, "ImgUrl");
            string msg = isok ? "添加成功" : "添加失败";
            return Json(new { isok, msg, model = newTable }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 餐饮菜品分类栏目操作

        [HttpGet]
        public ActionResult AddFoodGoodsType(int appId=0)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.appId = appId;

            Food miAppfood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppfood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.FoodId = miAppfood.Id;
            ViewBag.StoreId = miAppfood.Id;
            List<FoodGoodsType> foodgoodsTypeList = FoodGoodsTypeBLL.SingleModel.GetlistByFoodId(miAppfood.Id);
            return View(foodgoodsTypeList);
        }

        /// <summary>
        /// 添加菜品分类
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFoodGoodType(int appId, FoodGoodsType goodsTypeModel)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsTypeModel.State != -1 && string.IsNullOrEmpty(goodsTypeModel.Name.Trim()))
            {
                return Json(new { isok = false, msg = "菜品分类名称不能为空！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsTypeModel.FoodId == 0)
            {
                return Json(new { isok = false, msg = "餐饮店铺id不能为0！" }, JsonRequestBehavior.AllowGet);
            }
            if (!Regex.IsMatch(goodsTypeModel.SortVal.ToString(), @"^[0-9]*$"))
                return Json(new { isok = false, msg = "请填写正确的排序数" });
            Food miFood = FoodBLL.SingleModel.GetModel(goodsTypeModel.FoodId);
            if (miFood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            int resultInt = 0;//
            List<FoodGoodsType> goodsTypeList = FoodGoodsTypeBLL.SingleModel.GetlistByFoodId(goodsTypeModel.FoodId);
            if (goodsTypeModel.Id == 0)//添加
            {

                if (goodsTypeList.Count >= 50)
                {
                    return Json(new { isok = false, msg = "目前最多能添加50个分类哦" });
                }
                if (goodsTypeList.Any(m => m.Name == goodsTypeModel.Name))
                {
                    return Json(new { isok = false, msg = "分类名称已存在 , 请重新添加" });
                }
                object result = FoodGoodsTypeBLL.SingleModel.Add(new FoodGoodsType() { SortVal = goodsTypeModel.SortVal, FoodId = goodsTypeModel.FoodId, Name = goodsTypeModel.Name, LogImg = goodsTypeModel.LogImg, CreateDate = DateTime.Now, UpdateDate = DateTime.Now });
                if (int.Parse(result.ToString()) > 0)
                {
                    string key = string.Format(FoodGoodsTypeBLL.foodGoodsTypeKey, goodsTypeModel.FoodId);
                    RedisUtil.Remove(key);
                    return Json(new { isok = true, msg = "添加成功!", newid = result, dataObj = goodsTypeModel }, JsonRequestBehavior.AllowGet);
                }
            }
            else//编辑
            {
                if (goodsTypeList.Any(m => m.Name == goodsTypeModel.Name && m.Id != goodsTypeModel.Id))
                {
                    return Json(new { isok = false, msg = "分类名称已存在 , 请重新添加" });
                }
                FoodGoodsType updateModel = FoodGoodsTypeBLL.SingleModel.GetModel(goodsTypeModel.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到分类" });
                }
                //删除，做数量验证
                if (goodsTypeModel.State == -1)
                {
                    int typeGoodsCount = FoodGoodsBLL.SingleModel.GetCount($"TypeId={updateModel.Id} and State >= 0");//从菜品里找出属于该类别的菜品
                    if (typeGoodsCount > 0)
                    {
                        return Json(new { isok = false, msg = $"该分类下有{typeGoodsCount}个菜品，不能删除" });
                    }
                    updateModel.State = -1;
                    updateModel.UpdateDate = DateTime.Now;
                    //只修改名称，排序，修改时间
                    resultInt = FoodGoodsTypeBLL.SingleModel.Update(updateModel, "State,UpdateDate") ? 1 : 0;
                }
                else
                {

                    updateModel.UpdateDate = DateTime.Now;
                    updateModel.Name = goodsTypeModel.Name;
                    updateModel.LogImg = goodsTypeModel.LogImg;
                    updateModel.SortVal = goodsTypeModel.SortVal;
                    //只修改名称，排序，修改时间
                    resultInt = FoodGoodsTypeBLL.SingleModel.Update(updateModel, "UpdateDate,SortVal,Name,LogImg") ? 1 : 0;
                }
                string key = string.Format(FoodGoodsTypeBLL.foodGoodsTypeKey, goodsTypeModel.FoodId);
                RedisUtil.Remove(key);
                return Json(new { isok = true, msg = "修改成功", dataObj = updateModel });
            }


            return Json(new { isok = false, msg = "系统错误！" });
        }


        /// <summary>
        /// 是否可以添加菜品分类
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetFoodGoodTypeCanAdd(int storeid)
        {
            //var typeList = _miAppFoodGoodsTypeBll.GetList($"  ");
            int count = FoodGoodsTypeBLL.SingleModel.GetCount($" foodid = {storeid} and State >= 0 ");
            if (count >= 50)
            {
                return Json(new { isok = false, msg = "无法新增分类！您已添加了50个菜品分类，已达到上限，请编辑已有的分类或删除部分分类后再进行新增。" });
            }

            return Json(new { isok = true, msg = "可以添加！" });
        }

        public ActionResult FoodGoodTypeEditPartail(int id)
        {
            FoodGoodsType foodgoodstype = FoodGoodsTypeBLL.SingleModel.GetModel(id);
            return PartialView("_PartialTypeItem", foodgoodstype);
        }
        #endregion


        #region 餐饮菜品的规格属性栏目操作
        /// <summary>
        /// 是否可以添加菜品规格
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetFoodGoodAttrCanAdd(int storeid)
        {
            int count = FoodGoodsAttrBLL.SingleModel.GetCount($" foodid = {storeid} and State >= 0 ");
            if (count >= 10)
            {
                return Json(new { isok = false, msg = "规格值已达到上限，请编辑已有的规格值或删除部分规格值后再进行新增。" });
            }

            return Json(new { isok = true, msg = "可以添加！" });
        }


        public ActionResult AddFoodAttrSpecList(int appId)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.appId = appId;

            Food _miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (_miAppFood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.FoodId = _miAppFood.Id;

            List<FoodGoodsAttr> goodsAttrList = FoodGoodsAttrBLL.SingleModel.GetlistAttrByFoodId(_miAppFood.Id);
            foreach (FoodGoodsAttr item in goodsAttrList)
            {
                List<FoodGoodsSpec> speclist = FoodGoodsSpecBLL.SingleModel.GetlistSpecByAttrId(item.Id);
                item.SpecList = speclist;
            }
            return View(goodsAttrList);
        }

        [HttpPost]
        public ActionResult AddFoodAttrList(int appId, FoodGoodsAttr goodsAttr)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsAttr.State != -1 && string.IsNullOrEmpty(goodsAttr.AttrName.Trim()))
            {
                return Json(new { isok = false, msg = "规格名称不能为空！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsAttr.FoodId == 0)
            {
                return Json(new { isok = false, msg = "餐饮店铺id不能为0！" }, JsonRequestBehavior.AllowGet);
            }

            Food miAppFood = FoodBLL.SingleModel.GetModel(goodsAttr.FoodId);
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            int resultInt = 0;//
            List<FoodGoodsAttr> goodsAttrList = FoodGoodsAttrBLL.SingleModel.GetlistAttrByFoodId(goodsAttr.FoodId);
            string key = string.Format(FoodGoodsAttrBLL.foodGoodsAttrKey, goodsAttr.FoodId);
            if (goodsAttr.Id == 0)//添加
            {

                if (goodsAttrList.Count >= 20)
                {
                    return Json(new { isok = false, msg = "目前最多能添加20个规格哦" });
                }
                if (goodsAttrList.Any(m => m.AttrName == goodsAttr.AttrName))
                {
                    return Json(new { isok = false, msg = "规格名称已存在 , 请重新添加" });
                }
                object result = FoodGoodsAttrBLL.SingleModel.Add(new FoodGoodsAttr() { FoodId = goodsAttr.FoodId, AttrName = goodsAttr.AttrName, State = 0 });
                if (int.Parse(result.ToString()) > 0)
                {
                    RedisUtil.Remove(key);//移除缓存
                    return Json(new { isok = true, msg = "新增成功!", obj = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else//编辑
            {
                if (goodsAttrList.Any(m => m.AttrName == goodsAttr.AttrName && m.Id != goodsAttr.Id))
                {
                    return Json(new { isok = false, msg = "规格名称已存在 , 请重新添加" });
                }
                FoodGoodsAttr updateModel = FoodGoodsAttrBLL.SingleModel.GetModel(goodsAttr.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到规格" });
                }
                //删除，做数量验证
                if (goodsAttr.State == -1)
                {
                    List<FoodGoodsAttrSpec> attrlist = FoodGoodsAttrSpecBLL.SingleModel.GetList($"AttrId={goodsAttr.Id}");//从菜品-规格-属性关系表里找出与当前规格关联的记录
                    if (attrlist.Any())
                    {

                        //菜品实现后在来
                        int attrGoodsCount = FoodGoodsBLL.SingleModel.GetCount($"Id in ({string.Join(",", attrlist.Select(m => m.FoodGoodsId).Distinct())}) and State>=0");
                        if (attrGoodsCount > 0)
                        {
                            return Json(new { isok = false, msg = $"有{attrGoodsCount}个菜品使用了该规格，不能删除" });
                        }
                        //  int attrGoodsCount = 1;

                    }

                    updateModel.State = -1;
                    resultInt = FoodGoodsAttrBLL.SingleModel.Update(updateModel, "State") ? 1 : 0;
                }
                else
                {
                    updateModel.AttrName = goodsAttr.AttrName;
                    resultInt = FoodGoodsAttrBLL.SingleModel.Update(updateModel, "AttrName") ? 1 : 0;
                }
                RedisUtil.Remove(key);//移除缓存
                return Json(new { isok = true, msg = "修改成功" });
            }


            return Json(new { isok = false, msg = "系统错误！" });
        }

        /// <summary>
        /// 添加/编辑/删除菜品规格属性
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFoodSpecList(int appId, FoodGoodsSpec goodsSpec)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsSpec.State != -1 && string.IsNullOrEmpty(goodsSpec.SpecName.Trim()))
            {
                return Json(new { isok = false, msg = "属性名称不能为空！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsSpec.AttrId == 0)
            {
                return Json(new { isok = false, msg = "所属规格id不能为0！" }, JsonRequestBehavior.AllowGet);
            }

            FoodGoodsAttr attr = FoodGoodsAttrBLL.SingleModel.GetModel(goodsSpec.AttrId);
            if (attr == null)
            {
                return Json(new { isok = false, msg = "找不到所属规格！" }, JsonRequestBehavior.AllowGet);
            }
            Food miAppFood = FoodBLL.SingleModel.GetModel(attr.FoodId);
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            int resultInt = 0;//
            List<FoodGoodsSpec> goodsSpecList = FoodGoodsSpecBLL.SingleModel.GetlistSpecByAttrId(goodsSpec.AttrId);
            string key = string.Format(FoodGoodsSpecBLL.foodGoodsSpecKey, goodsSpec.AttrId);
            if (goodsSpec.Id == 0)//添加
            {
                if (goodsSpecList.Count >= 10)
                {
                    return Json(new { isok = false, msg = "每个规格最多能添加10个属性哦" });
                }
                if (goodsSpecList.Any(m => m.SpecName == goodsSpec.SpecName))
                {
                    return Json(new { isok = false, msg = "属性名称已存在 , 请重新添加" });
                }
                object result = FoodGoodsSpecBLL.SingleModel.Add(new FoodGoodsSpec() { AttrId = goodsSpec.AttrId, SpecName = goodsSpec.SpecName, State = 0 });
                if (int.Parse(result.ToString()) > 0)
                {
                    RedisUtil.Remove(key);
                    return Json(new { isok = true, msg = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else//编辑
            {
                if (goodsSpecList.Any(m => m.SpecName == goodsSpec.SpecName && m.Id != goodsSpec.Id))
                {
                    return Json(new { isok = false, msg = "属性名称已存在 , 请重新添加" });
                }
                FoodGoodsSpec updateModel = FoodGoodsSpecBLL.SingleModel.GetModel(goodsSpec.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到该属性" });
                }
                //删除，做数量验证
                if (goodsSpec.State == -1)
                {
                    List<FoodGoodsAttrSpec> speclist = FoodGoodsAttrSpecBLL.SingleModel.GetList($"SpecId={goodsSpec.Id}");
                    if (speclist.Any())
                    {

                        // 有菜品实现在来
                        int specGoodsCount = FoodGoodsBLL.SingleModel.GetCount($"Id in ({string.Join(",", speclist.Select(m => m.FoodGoodsId).Distinct())}) and State>=0");
                        if (specGoodsCount > 0)
                        {
                            return Json(new { isok = false, msg = $"有{specGoodsCount}个菜品使用了该属性，不能删除" });
                        }

                    }

                    updateModel.State = -1;
                    resultInt = FoodGoodsSpecBLL.SingleModel.Update(updateModel, "State") ? 1 : 0;
                }
                else
                {
                    updateModel.SpecName = goodsSpec.SpecName;
                    resultInt = FoodGoodsSpecBLL.SingleModel.Update(updateModel, "SpecName") ? 1 : 0;
                }
                RedisUtil.Remove(key);
                return Json(new { isok = true, msg = "修改成功" });
            }


            return Json(new { isok = false, msg = "系统错误！" });
        }


        //返回菜品规格属性编辑框数据
        [HttpPost]
        public ActionResult editFoodAttrSpecFrom(int attrId)
        {
            FoodGoodsAttr attrModel = FoodGoodsAttrBLL.SingleModel.GetModel(attrId);
            if (attrModel == null)
            {
                return Json(new { isok = false, msg = "找不到该规格！" });
            }
            attrModel.SpecList = FoodGoodsSpecBLL.SingleModel.GetlistSpecByAttrId(attrModel.Id);

            return PartialView("_PartialAttrItem", attrModel);
        }

        //返回菜品规格属性编辑框数据
        [HttpPost]
        public ActionResult foodSpacCanDelete(int spacId)
        {
            //var count = _foodGoodsattrspecBll.GetList($" SpecId = {spacId} ");
            List<FoodGoodsAttrSpec> speclist = FoodGoodsAttrSpecBLL.SingleModel.GetList($"SpecId={spacId}");
            if (speclist.Any())
            {

                // 有菜品实现在来
                int specGoodsCount = FoodGoodsBLL.SingleModel.GetCount($"Id in ({string.Join(",", speclist.Select(m => m.FoodGoodsId).Distinct())}) and State>=0");
                if (specGoodsCount > 0)
                {
                    return Json(new { isok = false, msg = $"有{specGoodsCount}个菜品使用了该属性，不能删除" });
                }

            }
            return Json(new { isok = true, msg = $"可以删除" });
            //var attrModel = _foodGoodsattrBll.GetModel(attrId);
            //if (attrModel == null)
            //{
            //    return Json(new { isok = false, msg = "找不到该规格！" });
            //}
            //attrModel.SpecList = _foodGoodsspecBll.GetList($" attrid = {attrModel.Id} and State >= 0 ");

            //return PartialView("_PartialAttrItem", attrModel);
        }


        /// <summary>
        /// 规格,多属性一次编辑
        /// </summary>
        /// <param name="attrId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult editFoodAttrSpec(int attrid, string attrname, int sort, string spacJsonData)
        {
            TransactionModel TranModel = new TransactionModel();
            FoodGoodsAttr attrModel = FoodGoodsAttrBLL.SingleModel.GetModel(attrid);
            if (attrModel == null)
            {
                return Json(new { isok = false, msg = "找不到规格" });
            }
            attrModel.AttrName = attrname;
            attrModel.Sort = sort;
            List<FoodGoodsAttr> goodsAttrList = FoodGoodsAttrBLL.SingleModel.GetlistAttrByFoodId(attrModel.FoodId);
            if (goodsAttrList.Any(m => m.AttrName == attrModel.AttrName && m.Id != attrModel.Id))
            {
                return Json(new { isok = false, msg = "规格名称已存在 , 请重新编辑" });
            }
            //转model
            //var speclist = Utility.Serialize.SerializeHelper.DesFromJson<List<MiniappFoodGoodsSpec>>(spacJsonData);
            List<FoodGoodsSpec> speclist = JsonConvert.DeserializeObject<List<FoodGoodsSpec>>(spacJsonData);
            List<FoodGoodsSpec> dbSpecList = FoodGoodsSpecBLL.SingleModel.GetlistSpecByAttrId(attrModel.Id);

            int namecount = speclist.Select(x => x.SpecName).Distinct().Count();
            if (speclist.Count != namecount)
            {
                return Json(new { isok = false, msg = "编辑的数据存在输入相同属性名称,请修改后重新保存" });
            }

            if (speclist.Any(x => x.SpecName.Length > 8))
            {
                return Json(new { isok = false, msg = "编辑的数据存在属性名称超过8个字符的数据,请修改后重新保存" });
            }


            //验证完,拼接sql
            TranModel.Add(FoodGoodsAttrBLL.SingleModel.BuildUpdateSql(attrModel));

            foreach (FoodGoodsSpec x in speclist)
            {
                if (x.Id > 0 || dbSpecList.Any(y => y.Id == x.Id))
                {
                    FoodGoodsSpec model = dbSpecList.Where(y => y.Id == x.Id).FirstOrDefault();
                    model.SpecName = x.SpecName;
                    model.State = x.State;

                    TranModel.Add(FoodGoodsSpecBLL.SingleModel.BuildUpdateSql(model, "SpecName,State"));
                }
                else
                {
                    x.State = 0;
                    x.AttrId = attrModel.Id;

                    TranModel.Add(FoodGoodsSpecBLL.SingleModel.BuildAddSql(x));

                }
            }

            //验证数据添加update 语句
            if (!FoodGoodsAttrBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {
                return Json(new { isok = false, msg = "系统忙,请稍候再试！" });
            };

            string key = string.Format(FoodGoodsAttrBLL.foodGoodsAttrKey, attrModel.FoodId);
            RedisUtil.Remove(key);
            key = string.Format(FoodGoodsSpecBLL.foodGoodsSpecKey, attrModel.Id);
            RedisUtil.Remove(key);

            return Json(new { isok = true, msg = "修改成功！" });
        }
        #endregion

        #region 餐饮菜品的标签

        //标签列表

        [HttpGet]
        public ActionResult FoodLabelList(int appId)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.appId = appId;

            Food miAppfood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppfood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.FoodId = miAppfood.Id;
            ViewBag.StoreId = miAppfood.Id;
            List<FoodLabel> foodgoodsLabelList = FoodLabelBLL.SingleModel.GetFoodLabelByFoodId(miAppfood.Id);
            return View(foodgoodsLabelList);
        }

        /// <summary>
        /// 添加菜品标签
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFoodLabel(int appId, FoodLabel goodslabelModel)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodslabelModel.State != -1 && string.IsNullOrEmpty(goodslabelModel.LabelName.Trim()))
            {
                return Json(new { isok = false, msg = "菜品标签名称不能为空！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodslabelModel.FoodStoreId == 0)
            {
                return Json(new { isok = false, msg = "餐饮店铺id不能为0！" }, JsonRequestBehavior.AllowGet);
            }

            Food miFood = FoodBLL.SingleModel.GetModel(goodslabelModel.FoodStoreId);
            if (miFood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            int resultInt = 0;//
            List<FoodLabel> foodLabelList = FoodLabelBLL.SingleModel.GetFoodLabelByFoodId(miFood.Id);
            string key = string.Format(FoodLabelBLL.foodLabelKey, miFood.Id);
            if (goodslabelModel.Id == 0)//添加
            {

                if (foodLabelList.Count >= 25)
                {
                    return Json(new { isok = false, msg = "目前最多能添加25个标签哦" });
                }
                if (foodLabelList.Any(m => m.LabelName == goodslabelModel.LabelName))
                {
                    return Json(new { isok = false, msg = "标签名称已存在 , 请重新添加" });
                }
                object result = FoodLabelBLL.SingleModel.Add(new FoodLabel() { LabelName = goodslabelModel.LabelName, FoodStoreId = miFood.Id, CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Sort = goodslabelModel.Sort });
                if (int.Parse(result.ToString()) > 0)
                {

                    RedisUtil.Remove(key);
                    return Json(new { isok = true, msg = "添加成功!", id = Convert.ToInt32(result), labelName = goodslabelModel.LabelName, dataObj = goodslabelModel }, JsonRequestBehavior.AllowGet);
                }
            }
            else//编辑
            {
                if (foodLabelList.Any(m => m.LabelName == goodslabelModel.LabelName && m.Id != goodslabelModel.Id))
                {
                    return Json(new { isok = false, msg = "标签名称已存在 , 请重新添加" });
                }
                FoodLabel updateModel = FoodLabelBLL.SingleModel.GetModel(goodslabelModel.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到标签" });
                }
                //删除，做数量验证
                if (goodslabelModel.State == -1)
                {
                    int typeGoodsCount = FoodGoodsLabelBLL.SingleModel.GetCount($" LabelId={updateModel.Id} ");//从菜品里找出属于该类别的菜品
                    if (typeGoodsCount > 0)
                    {
                        return Json(new { isok = false, msg = $"该标签下有{typeGoodsCount}个菜品，不能删除" });
                    }
                    updateModel.State = -1;
                    updateModel.UpdateDate = DateTime.Now;
                    //只修改名称，排序，修改时间
                    resultInt = FoodLabelBLL.SingleModel.Update(updateModel, "State,UpdateDate") ? 1 : 0;

                }
                else
                {

                    updateModel.UpdateDate = DateTime.Now;
                    updateModel.LabelName = goodslabelModel.LabelName;
                    updateModel.Sort = goodslabelModel.Sort;
                    //updateModel.FoodStoreId = goodslabelModel.FoodStoreId;
                    //updateModel. = goodslabelModel.SortVal;
                    //只修改名称，排序，修改时间
                    resultInt = FoodLabelBLL.SingleModel.Update(updateModel, "UpdateDate,LabelName,Sort") ? 1 : 0;

                }
                RedisUtil.Remove(key);
                return Json(new { isok = true, msg = "修改成功", dataObj = updateModel });
            }


            return Json(new { isok = false, msg = "系统错误！" });
        }


        /// <summary>
        /// 是否可以添加菜品标签
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetFoodLabelTypeCanAdd(int storeid)
        {
            //var typeList = _miAppFoodGoodsTypeBll.GetList($"  ");
            int count = FoodLabelBLL.SingleModel.GetCount($" foodStoreid = {storeid} and State >= 0 ");
            if (count >= 25)
            {
                return Json(new { isok = false, msg = "无法新增标签！您已添加了25个菜品标签，已达到上限，请编辑已有的标签或删除部分标签后再进行新增。" });
            }

            return Json(new { isok = true, msg = "可以添加！" });
        }

        public ActionResult FoodLabelEditPartail(int id)
        {
            FoodLabel FoodLabel = FoodLabelBLL.SingleModel.GetModel(id);
            return PartialView("_PartialLabelItem", FoodLabel);
        }

        //标签保存

        #endregion


        #region 菜品管理操作

        //菜品列表
        //[OutputCache(VaryByCustom = "type", VaryByParam = "type", Duration = 20)]
        public ActionResult FoodGoodsList(int appId=0, string goodname = "", int goodlabel = 0, string type = "", int pageIndex = 1, int pageSize = 10)
        {
            ViewBag.appId = appId;
            if (dzaccount == null)
            {
                return View("PageError", new Return_Msg() { Msg = "登录过期!", code = "403" });
            }
            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (foodStore == null)
            {
                return View("PageError", new Return_Msg() { Msg = "请先配置餐饮店铺信息!", code = "500" });
            }
            List<int> typeList = new List<int>();
            if (type.Length > 0)
            {
                type = type.Substring(0, type.Length - 1);
            }
            if (type.Split(',') != null && type.Split(',').Any())
            {
                string[] typeArr = type.Split(',');
                if (typeArr.Any(val => !string.IsNullOrWhiteSpace(val)))
                {
                    typeList = typeArr.Where(val => !string.IsNullOrWhiteSpace(val)).Select(val => Convert.ToInt32(val)).ToList();
                }
            }

            List<FoodGoods> list = FoodGoodsBLL.SingleModel.getFindPageList(foodStore.Id, goodname, goodlabel == 0 ? null : new int[] { goodlabel }, typeList.ToArray(), pageIndex, pageSize);

            List<string> typeIdStrs = list.Select(x => x.TypeId)?.ToList();
            List<string> typeIds = new List<string>();
            typeIdStrs?.ForEach(ts =>
            {
                typeIds.AddRange(ts?.Split(','));
            });
          //  log4net.LogHelper.WriteInfo(this.GetType(), $"list={JsonConvert.SerializeObject(list)}");

           // log4net.LogHelper.WriteInfo(this.GetType(),$"typeIdStrs={JsonConvert.SerializeObject(typeIdStrs)};ts={JsonConvert.SerializeObject(typeIds)}");

            List<FoodGoodsType> typeNameList = typeIds.Count <= 0 ? new List<FoodGoodsType>() : FoodGoodsTypeBLL.SingleModel.GetList($" Id in ({string.Join(",", typeIds.Distinct())}) ");

            List<FoodGoodsAttrSpec> attrList = FoodGoodsAttrSpecBLL.SingleModel.GetList($" FoodGoodsId in ({(list.Any() ? string.Join(",", list.Select(x => x.Id).Distinct()) : "0")}) ");
            List<FoodGoodsAttr> attrNameList = new List<FoodGoodsAttr>();
            if (attrList != null && attrList.Any())
            {
                attrNameList = FoodGoodsAttrBLL.SingleModel.GetList($" Id in ({string.Join(",", attrList.Select(x => x.AttrId).Distinct())}) ");
            }

            //拼接必要数据
            list.ForEach(x =>
            {
                //拼接分类名称
                List<FoodGoodsType> curTypes = typeNameList.FindAll(y => x.TypeId?.Split(',')?.Any(z => z.Equals(y.Id.ToString())) == isTrue);
                x.TypeName = curTypes != null ? string.Join(",", curTypes.Select(c => c.Name)) : string.Empty;

                //拼接标签名称
                var labelIds = x.labelIdStr.Split(',').Where(y => !string.IsNullOrWhiteSpace(y));
                if (labelIds != null && labelIds.Any())
                {
                    List<FoodLabel> labelNames = FoodLabelBLL.SingleModel.GetList($" Id in ({string.Join(",", labelIds)}) ");
                    x.labelNameStr = string.Join(",", labelNames.Select(y => y.LabelName));
                }

                //拼接规格
                if (attrList.Any(y => y.FoodGoodsId == x.Id))
                {
                    var attrIds = attrList.Where(y => y.FoodGoodsId == x.Id).Select(y => y.AttrId);
                    if (attrIds != null && attrIds.Any())
                    {
                        var attrNames = attrNameList.Where(y => attrIds.Contains(y.Id));
                        if (attrNames != null && attrIds.Any())
                        {
                            x.AttrStr = string.Join(",", attrNames.Select(y => y.AttrName));
                        }
                    }
                }
            });

            ViewBag.TotalCount = FoodGoodsBLL.SingleModel.getFindPageListCount(foodStore.Id, goodname, goodlabel == 0 ? null : new int[] { goodlabel }, typeList.ToArray());
            ViewBag.pageSize = pageSize;

            ViewBag.foodlabellist = FoodLabelBLL.SingleModel.GetFoodLabelByFoodId(foodStore.Id);
            ViewBag.typeList = FoodGoodsTypeBLL.SingleModel.GetlistByFoodId(foodStore.Id);
            ViewBag.goodname = goodname;
            ViewBag.goodlabel = goodlabel;
            ViewBag.checkType = typeList.ToList();
            return View(list);
        }


        /// <summary>
        /// add goods添加菜品
        /// </summary>
        /// <returns></returns>
        public ActionResult addFoodGoods(FoodGoods goods, string ImgList = "", string DescImgList = "", string goods_spec_ids = "", int oldhvid = 0, string videopath = "", int copy = 0)
        {
            int copyGoodsId = goods.Id;
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            #region Base64解密
            //if (goods != null)
            //{
            //    try
            //    {
            //        string strDescription = goods.Description.Replace(" ", "+");
            //        byte[] bytes = Convert.FromBase64String(strDescription);
            //        goods.Description = System.Text.Encoding.UTF8.GetString(bytes);
            //    }
            //    catch
            //    {
            //        return Json(new { code = -1, msg = "菜品描述Base64解密失败" });
            //    }
            //}
            #endregion

            //if (!string.IsNullOrEmpty(coupon.Description))
            //{
            //    coupon.Description = coupon.Description.Replace("\r\n", "</br>").Replace("\n", "</br>");
            //}
            List<FoodGoodsSpec> speclist = new List<FoodGoodsSpec>();
            if (!string.IsNullOrWhiteSpace(goods_spec_ids))
            {
                string sids = goods_spec_ids.Substring(0, goods_spec_ids.Length - 1);
                speclist = FoodGoodsSpecBLL.SingleModel.GetList($"Id in ({sids})");
                var deleteStateSpac = speclist.Where(x => x.State == -1);
                if (deleteStateSpac != null && deleteStateSpac.Any())
                {

                    return Json(new { code = -1, msg = $"菜品属性('{string.Join("','", deleteStateSpac.Select(x => x.SpecName))}')被删除,请选择其他属性！" }, JsonRequestBehavior.AllowGet);
                }
            }

            var labels = goods.labelIdStr?.Split(',').ToList().Where(x => !string.IsNullOrWhiteSpace(x));
            List<FoodLabel> foodLabels = new List<FoodLabel>();
            if (labels != null && labels.Any())
            {
                if (labels.Count() > 5)
                {
                    return Json(new { code = -1, msg = $"单个菜品最多添加5个标签！" }, JsonRequestBehavior.AllowGet);
                }


                foodLabels = FoodLabelBLL.SingleModel.GetList($" Id in ({string.Join(",", labels)}) ") ?? new List<FoodLabel>();
                if (foodLabels != null && foodLabels.Any(x => x.State == -1))
                {
                    return Json(new { code = -1, msg = $"添加了不存在的标签，请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
                }
            }


            bool result;
            int addresult = 0;
            Food miAppFood = FoodBLL.SingleModel.GetModel(goods.FoodId);
            if (miAppFood == null)
            {
                return Json(new { code = -1, msg = "餐饮店铺信息有误！" }, JsonRequestBehavior.AllowGet);
            }
            //字符串转json串
            if (!string.IsNullOrEmpty(goods.AttrDetail))
            {
                string[] attrstr = goods.AttrDetail.Split(';');
                if (attrstr.Length > 0)
                {
                    List<FoodGoodsAttrDetail> attrdetaillist = new List<FoodGoodsAttrDetail>();
                    foreach (string attr in attrstr)
                    {
                        string[] detail = attr.Split(',');
                        if (detail.Length > 1)
                        {
                            FoodGoodsAttrDetail model = new FoodGoodsAttrDetail();
                            model.id = detail[0].ToString();
                            model.count = Convert.ToInt32(detail[1]);
                            model.price = Convert.ToInt32(Convert.ToDouble(detail[2]) * 100);
                            if (goods.goodtype == (int)EntGoodsType.拼团产品)
                            {
                                model.originalPrice = Convert.ToInt32(Convert.ToDouble(detail[3]) * 100);
                                model.groupPrice = Convert.ToInt32(Convert.ToDouble(detail[4]) * 100);
                            }
                            attrdetaillist.Add(model);
                            //价格更新之后更新购物车价格
                            if (goods.Id > 0)
                            {
                                FoodGoodsCartBLL.SingleModel.UpdateCartByGoodsId(goods.Id, model.id, model.price);
                            }
                        }
                    }
                    JavaScriptSerializer jsc = new JavaScriptSerializer();
                    StringBuilder jsonData = new StringBuilder();
                    jsc.Serialize(attrdetaillist, jsonData);
                    goods.AttrDetail = jsonData.ToString();
                }
            }
            //价格更新之后更新购物车价格
            if (goods.Id > 0)
            {
                FoodGoodsCartBLL.SingleModel.UpdateCartByGoodsId(goods.Id, "", goods.Price);
            }
            if (goods.Id > 0)
            {
                List<C_Attachment> imglist = C_AttachmentBLL.SingleModel.GetListByCache(goods.Id, (int)AttachmentItemType.小程序餐饮菜品介绍轮播图片);
                if (imglist != null && imglist.Count > 0)
                {
                    goods.ImgUrl = imglist[0].filepath;
                }
                FoodGoods oldnews = FoodGoodsBLL.SingleModel.GetModel(goods.Id);
                if (oldnews == null)
                {
                    return Json(new { code = -1, msg = "编辑出错！" }, JsonRequestBehavior.AllowGet);
                }
                goods.UpdateDate = DateTime.Now;

                addresult = 1;
                if (copy == 1)
                {
                    goods.Id = 0;
                    goods.IsSell = 1;
                    goods.State = (int)MiniappState.通过;
                    addresult = goods.Id = Convert.ToInt32(FoodGoodsBLL.SingleModel.Add(goods));
                    result = addresult > 0;
                }
                else
                {
                    string columnField = "Inventory,Stock,Price,OriginalPrice,TypeId,Description,GoodsName,UpdateDate,AttrDetail,ImgUrl,FreightIds,Introduction,IsHot,labelIdStr,openTakeOut,openTheShop,Sort,ispackin";

                    #region 若多规格商品被删除,更改购物车商品的标识
                    FoodGoods dbGood = FoodGoodsBLL.SingleModel.GetModel(goods.Id);
                    if (dbGood == null)
                    {
                        return Json(new { code = -1, msg = "商品不存在！" }, JsonRequestBehavior.AllowGet);
                    }

                    TransactionModel TranModel = new TransactionModel();
                    
                    //更改已被删掉的商品
                    List<string> dbGoodSpacList = dbGood.GASDetailList.Select(x => x.id).ToList();
                    List<string> goodsSpacList = goods.GASDetailList.Select(x => x.id).ToList();
                    List<string> updateGoodsStateSqlList = new List<string>();
                    if (dbGoodSpacList.Count > 0)
                    {
                        dbGoodSpacList.ForEach(x =>
                        {
                            if (!goodsSpacList.Contains(x))
                            {
                                updateGoodsStateSqlList.AddRange(FoodGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsIdSpecids(goods.Id, x, 2));
                            }
                        });
                    }

                    updateGoodsStateSqlList.ForEach(x =>
                    {
                        TranModel.Add(x);
                        //log4net.LogHelper.WriteInfo(GetType(),x);
                    });

                    if (!FoodGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
                    {
                        return Json(new { code = -1, msg = "更改购物车标识失败！" }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion

                    result = FoodGoodsBLL.SingleModel.Update(goods, columnField);
                }
                //addresult = 1;

                #region 拼团产品
                if (goods.goodtype == (int)EntGoodsType.拼团产品)
                {
                    EntGroupsRelation entgroupmodel = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(goods.Id, goods.EntGroups.RId);
                    if (entgroupmodel != null)
                    {

                        entgroupmodel.LimitNum = goods.EntGroups.LimitNum;
                        entgroupmodel.GroupSize = goods.EntGroups.GroupSize;
                        entgroupmodel.InitSaleCount = goods.EntGroups.InitSaleCount;
                        if (!EntGroupsRelationBLL.SingleModel.Update(entgroupmodel, "LimitNum,GroupSize,InitSaleCount"))
                        {
                            return Json(new { isok = true, msg = "保存拼团信息失败！" });
                        }
                    }
                }
                #endregion
            }
            else
            {
                goods.CreateDate = DateTime.Now;
                goods.UpdateDate = DateTime.Now;
                goods.State = (int)MiniappState.通过;
                goods.IsSell = 1;

                int id = goods.Id = Convert.ToInt32(FoodGoodsBLL.SingleModel.Add(goods));
                if (id > 0)
                {
                    //判断是否是添加拼团商品
                    if (goods.goodtype == (int)EntGoodsType.拼团产品)
                    {
                        goods.EntGroups.EntGoodsId = id;
                        int groupid = Convert.ToInt32(EntGroupsRelationBLL.SingleModel.Add(goods.EntGroups));
                        if (groupid <= 0)
                        {
                            return Json(new { isok = false, msg = "保存失败" });
                        }
                    }
                }
                goods.Id = id;
                addresult = id;
            }
            if (addresult > 0)
            {
                #region 轮播图
                if (!string.IsNullOrWhiteSpace(ImgList))
                {
                    string[] imgs = ImgList.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string img in imgs)
                    {
                        //判断上传图片是否以http开头，不然为破图-蔡华兴
                        if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http", StringComparison.Ordinal) == 0)
                        {
                            C_AttachmentBLL.SingleModel.Add(new C_Attachment
                            {
                                itemId = goods.Id,
                                createDate = DateTime.Now,
                                filepath = img,
                                itemType = (int)AttachmentItemType.小程序餐饮菜品介绍轮播图片,
                                thumbnail = img,
                                status = 0
                            });
                        }
                    }
                }

                //复制则复制一份
                if (copy == 1)
                {
                    List<C_Attachment> imgList = C_AttachmentBLL.SingleModel.GetListByCache(copyGoodsId, (int)AttachmentItemType.小程序餐饮菜品介绍轮播图片);
                    imgList.ForEach(x =>
                    {
                        x.id = 0;
                        x.itemId = addresult;
                        C_AttachmentBLL.SingleModel.Add(x);
                    });
                }
                #endregion

                #region 详情图

                //if (!string.IsNullOrEmpty(DescImgList))
                //{
                //    string[] imgArray = DescImgList.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                //    if (imgArray.Length > 0)
                //    {
                //        foreach (var img in imgArray)
                //        {
                //            //判断上传图片是否以http开头，不然为破图-蔡华兴
                //            if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http://", StringComparison.Ordinal) == 0)
                //            {
                //                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                //                {
                //                    itemId = goods.Id,
                //                    createDate = DateTime.Now,
                //                    filepath = img,
                //                    itemType = (int)AttachmentItemType.小程序餐饮菜品详情图,
                //                    thumbnail = img,
                //                    status = 0
                //                });
                //            }

                //        }
                //    }
                //}
                #endregion

                #region 视频
                //C_AttachmentVideoBLL.SingleModel.HandleVideoLogicStrategy(videopath, oldhvid, goods.Id, (int)AttachmentVideoType.小程序餐饮菜品视频);
                #endregion

                //删除旧关系
                int ids = FoodGoodsBLL.SingleModel.DelAttrList(goods.Id);
                #region 更新菜品规格关系表
                if (!string.IsNullOrEmpty(goods_spec_ids))
                {
                    foreach (FoodGoodsSpec item in speclist)
                    {
                        FoodGoodsAttrSpecBLL.SingleModel.Add(new FoodGoodsAttrSpec
                        {

                            FoodGoodsId = goods.Id,
                            AttrId = item.AttrId,
                            SpecId = item.Id
                        });
                    }
                }
                #endregion


                //删除旧关系
                bool success = FoodGoodsLabelBLL.SingleModel.Delete($" goodid = {goods.Id} ") > 0;
                if (foodLabels.Any())
                {
                    foreach (FoodLabel item in foodLabels)
                    {
                        FoodGoodsLabelBLL.SingleModel.Add(new FoodGoodsLabel
                        {
                            FoodStoreId = miAppFood.Id,
                            GoodId = goods.Id,
                            labelId = item.Id,
                        });
                    }
                }

                //System.Collections.ArrayList Imglist = FilterHandler.GetImgUrlfromhtml(goods.Description);
                ////自动抓取图片和图文混排图片处理开启一个线程去处理
                //if (Imglist.Count > 0)
                //{
                //    System.Threading.ThreadPool.QueueUserWorkItem(delegate
                //    {
                //        var content = goods.Description;
                //        if (Imglist.Count > 0)
                //            CityCommonUtils.downloadImgs(0, ref content, Imglist);
                //        var updateModel = _miniappfoodgoodsBll.GetModel(addresult);
                //        updateModel.Description = content;
                //        _miniappfoodgoodsBll.Update(updateModel, "Description");
                //    });

                //}

                string key = string.Format(FoodGoodsBLL.foodGoodsKey, miAppFood.Id);
                RedisUtil.Remove(key);//清除产品缓存列表

                return Json(new { code = 1, msg = "操作成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { code = -1, msg = "系统错误！" }, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// add goods添加菜品
        /// </summary>
        /// <returns></returns>
        //[LoginFilter]
        public ActionResult SaveFoodGood(FoodGoods goods, List<FoodGoodsAttr> useGoodsAttr, int copy = 0)
        {
            Return_Msg resultMsg = new Return_Msg();
            resultMsg.isok = false;

            Food miAppFood = FoodBLL.SingleModel.GetModel(goods.FoodId);
            if (miAppFood == null)
            {
                resultMsg.Msg = "餐饮店铺信息有误！";
                return Json(resultMsg);
            }

            //检测选用的规格是否都是有效的
            List<FoodGoodsSpec> speclist = new List<FoodGoodsSpec>();
            useGoodsAttr?.ForEach(a =>
            {
                speclist.AddRange(a.SpecList);
            });
            if (speclist?.Count > 0)
            {
                speclist = FoodGoodsSpecBLL.SingleModel.GetList($"Id in ({string.Join(",", speclist.Select(s => s.Id))})");
                IEnumerable<FoodGoodsSpec> deleteStateSpac = speclist.Where(x => x.State == -1);
                if (deleteStateSpac != null && deleteStateSpac.Any())
                {
                    resultMsg.Msg = $"菜品属性('{string.Join("','", deleteStateSpac.Select(x => x.SpecName))}')被删除,请选择其他属性！";
                    return Json(resultMsg);
                }
            }
            //检测选用的标签是否都是有效的
            IEnumerable<string> labels = goods.labelIdStr?.Split(',').ToList().Where(x => !string.IsNullOrWhiteSpace(x));
            List<FoodLabel> foodLabels = new List<FoodLabel>();
            if (labels != null && labels.Any())
            {
                if (labels.Count() > 5)
                {
                    resultMsg.Msg = $"单个菜品最多添加5个标签！";
                    return Json(resultMsg);

                }

                foodLabels = FoodLabelBLL.SingleModel.GetList($" Id in ({string.Join(",", labels)}) ") ?? new List<FoodLabel>();
                if (foodLabels != null && foodLabels.Any(x => x.State == -1))
                {
                    resultMsg.Msg = $"添加了不存在的标签，请刷新页面重试！";
                    return Json(resultMsg);
                }
            }

            //编辑
            if (goods.Id > 0)
            {
                FoodGoods dbGood = FoodGoodsBLL.SingleModel.GetModel(goods.Id);
                if (dbGood == null)
                {
                    resultMsg.isok = false;
                    resultMsg.Msg = "商品不存在";
                    return Json(resultMsg);
                }
                goods.UpdateDate = DateTime.Now;
                if (copy == 1) //复制商品
                {
                    goods.Id = 0;
                    goods.IsSell = 1;
                    goods.State = (int)MiniappState.通过;
                    goods.Id = Convert.ToInt32(FoodGoodsBLL.SingleModel.Add(goods));
                    resultMsg.isok = goods.Id > 0;
                }
                else
                {
                    #region 若多规格商品被删除,更改购物车商品的标识
                    FoodGoodsCartBLL.SingleModel.UpdateCartByGoodsId(goods.Id, "", goods.Price); //价格更新之后更新购物车价格

                    TransactionModel TranModel = new TransactionModel();
                    //更改已被删掉的商品
                    List<string> dbGoodSpacList = dbGood.GASDetailList.Select(x => x.id)?.ToList() ?? new List<string>();
                    List<string> goodsSpacList = goods.GASDetailList.Select(x => x.id).ToList();
                    List<string> updateGoodsStateSqlList = new List<string>();
                    dbGoodSpacList.ForEach(x =>
                    {
                        FoodGoodsCartBLL.SingleModel.UpdateCartByGoodsId(goods.Id, x, goods.Price); //更新购物车内的商品价格
                        if (!goodsSpacList.Contains(x))
                        {
                            updateGoodsStateSqlList.AddRange(FoodGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsIdSpecids(goods.Id, x, 2));
                        }
                    });
                    updateGoodsStateSqlList.ForEach(x =>
                    {
                        TranModel.Add(x);
                    });

                    if (!FoodGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
                    {
                        resultMsg.isok = false;
                        resultMsg.Msg = "更改购物车标识失败";
                        return Json(resultMsg);
                    }
                    #endregion

                    string columnField = "Inventory,Stock,Price,OriginalPrice,TypeId,Description,GoodsName,UpdateDate,AttrDetail,ImgUrl,FreightIds,Introduction,IsHot,labelIdStr,openTakeOut,openTheShop,Sort,isPackin";
                    resultMsg.isok = FoodGoodsBLL.SingleModel.Update(goods, columnField);
                }

                //拼团产品 则需要往拼团表更新数据
                if (goods.goodtype == (int)EntGoodsType.拼团产品)
                {
                    EntGroupsRelation entgroupmodel = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(goods.Id, goods.EntGroups.RId, 0, -1);
                    if (entgroupmodel != null)
                    {
                        entgroupmodel.OriginalPrice = goods.EntGroups.OriginalPrice;
                        entgroupmodel.LimitNum = goods.EntGroups.LimitNum;
                        entgroupmodel.GroupSize = goods.EntGroups.GroupSize;
                        entgroupmodel.ValidDateLength = goods.EntGroups.ValidDateLength;
                        if (!EntGroupsRelationBLL.SingleModel.Update(entgroupmodel, "OriginalPrice,LimitNum,GroupSize,ValidDateLength"))
                        {
                            resultMsg.isok = false;
                            resultMsg.Msg = "保存拼团信息失败！";
                            return Json(resultMsg);
                        }
                    }
                }
            }
            else
            {
                goods.CreateDate = DateTime.Now;
                goods.UpdateDate = DateTime.Now;
                goods.State = (int)MiniappState.通过;
                goods.IsSell = 1;

                goods.Id = Convert.ToInt32(FoodGoodsBLL.SingleModel.Add(goods));
                resultMsg.isok = goods.Id > 0;

                if (resultMsg.isok)
                {
                    //拼团产品 则需要往拼团表添加数据
                    if (goods.goodtype == (int)EntGoodsType.拼团产品)
                    {
                        goods.EntGroups.EntGoodsId = goods.Id;
                        int groupid = Convert.ToInt32(EntGroupsRelationBLL.SingleModel.Add(goods.EntGroups));
                        if (groupid <= 0)
                        {
                            resultMsg.isok = false;
                            resultMsg.Msg = "拼团商品保存失败";
                            return Json(resultMsg);
                        }
                    }
                }
            }

            if (resultMsg.isok)
            {

                #region 将菜品图片放入附件表
                List<C_Attachment> imgs = C_AttachmentBLL.SingleModel.GetListByCache(goods.Id, (int)AttachmentItemType.小程序餐饮菜品介绍轮播图片) ?? new List<C_Attachment>();
                //不存在该地址就删除旧附件并重新上传这张菜品图片
                if (!imgs.Any(i => i.filepath.Equals(goods.ImgUrl)))
                {
                    //清空该菜品的logo附件
                    imgs.ForEach(i =>
                    {
                        i.status = -1;
                        C_AttachmentBLL.SingleModel.Update(i, "status");
                    });


                    //判断上传图片是否以http开头，不然为破图-蔡华兴
                    if (!string.IsNullOrWhiteSpace(goods.ImgUrl) && goods.ImgUrl.IndexOf("http", StringComparison.Ordinal) == 0)
                    {
                        C_AttachmentBLL.SingleModel.Add(new C_Attachment
                        {
                            itemId = goods.Id,
                            createDate = DateTime.Now,
                            filepath = goods.ImgUrl,
                            itemType = (int)AttachmentItemType.小程序餐饮菜品介绍轮播图片,
                            thumbnail = goods.ImgUrl,
                            status = 0
                        });
                    }
                }



                #endregion

                #region 更新菜品规格关系表,标签关系表
                //删除旧关系
                FoodGoodsBLL.SingleModel.DelAttrList(goods.Id);
                if (speclist?.Count > 0)
                {
                    foreach (FoodGoodsSpec item in speclist)
                    {
                        FoodGoodsAttrSpecBLL.SingleModel.Add(new FoodGoodsAttrSpec
                        {

                            FoodGoodsId = goods.Id,
                            AttrId = item.AttrId,
                            SpecId = item.Id
                        });
                    }
                }

                //删除旧关系
                FoodGoodsLabelBLL.SingleModel.Delete($" goodid = {goods.Id} ");
                if (foodLabels.Any())
                {
                    foreach (FoodLabel item in foodLabels)
                    {
                        FoodGoodsLabelBLL.SingleModel.Add(new FoodGoodsLabel
                        {
                            FoodStoreId = miAppFood.Id,
                            GoodId = goods.Id,
                            labelId = item.Id,
                        });
                    }
                }
                #endregion

                string key = string.Format(FoodGoodsBLL.foodGoodsKey, miAppFood.Id);
                RedisUtil.Remove(key);//清除产品缓存列表

                resultMsg.Msg = "操作成功!";
                return Json(resultMsg);
            }

            resultMsg.Msg = "系统错误!";
            return Json(resultMsg);
        }


        //添加/编辑菜品
        public ActionResult GoodEdit(int appId=0, int gid = 0, int copy = 0)
        {
            int goodtype = Context.GetRequestInt("goodtype", (int)EntGoodsType.普通产品);
            ViewBag.Vid = 0;
            FoodGoods viewModel = null;
            ViewBag.appId = appId;
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                return View("PageError", new Return_Msg() { Msg = "请先配置餐饮店铺信息!", code = "500" });
            }
            if (gid > 0)
            {
                viewModel = FoodGoodsBLL.SingleModel.GetModel(gid);
                if (null == viewModel)
                    return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

                //拼团
                if (viewModel.goodtype == (int)EntGoodsType.拼团产品)
                {
                    viewModel.EntGroups = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(viewModel.Id, appId, 0, -1);
                    if (viewModel.EntGroups == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "该拼团不可用!", code = "500" });
                    }
                }

                ViewBag.Copy = copy;
                ViewBag.goodtype = viewModel.goodtype;
                return View(viewModel);
            }
            ViewBag.goodtype = goodtype;
            ViewBag.Copy = copy;
            ViewBag.LabelList = new List<FoodLabel>();
            return View(new FoodGoods() { FoodId = miAppFood.Id, GoodsName = "" });
        }

        //添加/编辑菜品
        public ActionResult FoodGoodsAddOrEdit(int appId, int gid = 0, int copy = 0)
        {
            int goodtype = Context.GetRequestInt("goodtype", (int)EntGoodsType.普通产品);
            ViewBag.Vid = 0;
            FoodGoods viewModel = null;
            ViewBag.appId = appId;
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                return View("PageError", new Return_Msg() { Msg = "请先配置餐饮店铺信息!", code = "500" });
            }


            //菜品分类
            List<FoodGoodsType> goodtylelist = FoodGoodsTypeBLL.SingleModel.GetlistByFoodId(miAppFood.Id);//  GetList("StoreId=" + storeId);
            ViewBag.GoodTypeList = goodtylelist?.Count > 0 ? goodtylelist : new List<FoodGoodsType>();
            //运费模板
            List<FoodFreightTemplate> freightList = FoodFreightTemplateBLL.SingleModel.GetList($"FoodId={miAppFood.Id}");
            ViewBag.FreightList = freightList;
            ViewBag.FreightText = "";

            if (gid > 0)
            {
                viewModel = FoodGoodsBLL.SingleModel.GetModel(gid);
                if (null == viewModel)
                    return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

                List<C_Attachment> ImgList = C_AttachmentBLL.SingleModel.GetListByCache(viewModel.Id, (int)AttachmentItemType.小程序餐饮菜品介绍轮播图片);
                List<object> CarouselList = new List<object>();
                foreach (C_Attachment attachment in ImgList)
                {
                    CarouselList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.CarouselList = CarouselList;

                List<C_Attachment> ImgList2 = C_AttachmentBLL.SingleModel.GetListByCache(viewModel.Id, (int)AttachmentItemType.小程序餐饮菜品详情图);
                List<object> DescImgList = new List<object>();
                foreach (C_Attachment attachment in ImgList2)
                {
                    DescImgList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.DescImgList = DescImgList;

                List<C_AttachmentVideo> attvideolist = C_AttachmentVideoBLL.SingleModel.GetListByCache(viewModel.Id, (int)AttachmentVideoType.小程序餐饮菜品视频);
                if (attvideolist.Count > 0)
                {
                    ViewBag.Vid = attvideolist[0].id;
                    ViewBag.convertFilePath = attvideolist[0].convertFilePath;
                    ViewBag.videoPosterPath = attvideolist[0].videoPosterPath;
                }
                List<FoodLabel> LabelList = new List<FoodLabel>();
                if (!string.IsNullOrWhiteSpace(viewModel.labelIdStr))
                {
                    var labelIds = viewModel.labelIdStr.Split(',').Where(x => !string.IsNullOrWhiteSpace(x));
                    if (labelIds != null && labelIds.Any())
                    {
                        LabelList = FoodLabelBLL.SingleModel.GetList($" Id in ({string.Join(",", labelIds) }) ");
                    }
                }

                //拼团
                if (viewModel.goodtype == (int)EntGoodsType.拼团产品)
                {
                    viewModel.EntGroups = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(viewModel.Id, appId, 0, -1);
                    if (viewModel.EntGroups == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "该拼团不可用!", code = "500" });
                    }
                }

                ViewBag.LabelList = LabelList ?? new List<FoodLabel>();
                ViewBag.Copy = copy;
                ViewBag.goodtype = viewModel.goodtype;
                return View(viewModel);
            }
            ViewBag.goodtype = goodtype;
            ViewBag.Copy = copy;
            ViewBag.LabelList = new List<FoodLabel>();
            return View(new FoodGoods() { FoodId = miAppFood.Id, GoodsName = "" });
        }

        //添加/编辑菜品
        public ActionResult checkLabelCount(int storeId)
        {
            Food store = FoodBLL.SingleModel.GetModel(storeId);
            int labelCount = FoodLabelBLL.SingleModel.GetCount($" foodStoreId = {storeId}  and State >=0 ");

            return Json(new { Success = labelCount > 0, Msg = $"标签数量为{labelCount}!" });
        }
        //添加/编辑菜品
        public ActionResult PartialLabelItemCheckForm(int storeId, int[] labelIds = null)
        {
            Food store = FoodBLL.SingleModel.GetModel(storeId);

            List<FoodLabel> labelList = FoodLabelBLL.SingleModel.GetList($" foodStoreId = {storeId}  and State >=0 ");
            if (labelList != null && labelList.Any() && labelIds != null)
            {
                labelList.ForEach(x =>
               {
                   x.isCheck = labelIds.Contains(x.Id);
               });
            }

            return PartialView("_PartialLabelItemCheckForm", labelList);
        }

        /// <summary>
        /// 删除菜品
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteFoodGoods(int type, int id, int appId = 0)
        {
            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");

            FoodGoods goods = FoodGoodsBLL.SingleModel.GetModel(id);

            if (goods.FoodId != miAppFood.Id)
            {
                return Json(new { Success = false, Msg = "非法操作!该菜品不属于此店铺" });
            }
            if (goods.IsSell == 1)
            {
                return Json(new { Success = false, Msg = "上架菜品不可以被编辑/删除!" });
            }

            TransactionModel TranModel = new TransactionModel();

            //更新购物车内商品的状态
            
            List<FoodGoodsAttrDetail> goodsDtlList = goods.GASDetailList;
            List<string> updateGoodsStateSqlList = new List<string>();
            updateGoodsStateSqlList = FoodGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(goods.Id, 2);
            updateGoodsStateSqlList.ForEach(x =>
            {
                TranModel.Add(x);
            });

            goods.State = -1;

            //更新商品状态
            TranModel.Add(FoodGoodsBLL.SingleModel.BuildUpdateSql(goods, "State"));

            if (FoodGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {
                bool success = FoodGoodsLabelBLL.SingleModel.Delete($" goodid = {goods.Id} ") > 0;
                string key = string.Format(FoodGoodsBLL.foodGoodsKey, miAppFood.Id);
                RedisUtil.Remove(key);//清除产品缓存列表
                return Json(new { Success = true, Msg = "成功" });
            }
            else
                return Json(new { Success = false, Msg = "失败" });
        }


        /// <summary>
        /// 上下架菜品
        /// </summary>
        /// <returns></returns>
        public ActionResult IsSellFoodGoods(int type, int id, int appid = 0, int foodId = 0)
        {
            if (appid == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appid}");
            if (miAppFood == null)
            {
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            }
            FoodGoods good = FoodGoodsBLL.SingleModel.GetModel(id);
            if (good == null)
            {
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            }
            if (good.State != (int)MiniappState.通过)
            {
                return View("PageError", new Return_Msg() { Msg = "状态繁忙!", code = "500" });
            }
            TransactionModel TranModel = new TransactionModel();
            //更新购物车内商品的状态
            
            List<FoodGoodsAttrDetail> goodsDtlList = good.GASDetailList;
            List<string> updateGoodsStateSqlList = new List<string>();
            updateGoodsStateSqlList = FoodGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(good.Id, type == 1 ? 0 : 1, oldGoodState: type == 1 ? 1 : 0);
            updateGoodsStateSqlList.ForEach(x =>
            {
                TranModel.Add(x);
            });

            //更新商品上下架状态
            good.IsSell = type;
            TranModel.Add(FoodGoodsBLL.SingleModel.BuildUpdateSql(good, "IsSell"));
            if (FoodGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {
                string key = string.Format(FoodGoodsBLL.foodGoodsKey, miAppFood.Id);
                RedisUtil.Remove(key);//清除产品缓存列表
                return Json(new { Success = true, Msg = "成功" });
            }
            else
                return Json(new { Success = false, Msg = "失败" });
        }

        /// <summary>
        /// 上下架菜品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BatchIsSellFoodGoods()
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            int type = Context.GetRequestInt("type", 0);
            if (type < 0 || type > 1)
            {
                return Json(new { Success = false, Msg = "状态错误" });
            }

            string foodGoodsIdString = Context.GetRequest("foodGoodsIds", "");
            List<int> foodIds = new List<int>();

            try
            {
                if (string.IsNullOrWhiteSpace(foodGoodsIdString))
                {
                    return Json(new { Success = false, Msg = "商品信息错误" });
                }
                string[] foodStrIdArray = foodGoodsIdString.Split(',');
                foodIds = foodStrIdArray.Select(x => Convert.ToInt32(x)).ToList<int>();
            }
            catch (Exception)
            {
                return Json(new { Success = false, Msg = "商品信息错误" });
            }

            TransactionModel TranModel = new TransactionModel();
            //更新购物车内商品的状态
            
            int miAppFood = 0;
            foreach (int id in foodIds)
            {
                FoodGoods good = FoodGoodsBLL.SingleModel.GetModel(id);
                if (good == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
                }
                if (good.State != (int)MiniappState.通过)
                {
                    return View("PageError", new Return_Msg() { Msg = "状态繁忙!", code = "500" });
                }
                miAppFood = good.FoodId;
                List<FoodGoodsAttrDetail> goodsDtlList = good.GASDetailList;
                List<string> updateGoodsStateSqlList = new List<string>();
                updateGoodsStateSqlList = FoodGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(good.Id, type == 1 ? 0 : 1, oldGoodState: type == 1 ? 1 : 0);
                updateGoodsStateSqlList.ForEach(x =>
                {
                    TranModel.Add(x);
                });

                //更新商品上下架状态
                good.IsSell = type;
                TranModel.Add(FoodGoodsBLL.SingleModel.BuildUpdateSql(good, "IsSell"));
            }


            if (FoodGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {
                string key = string.Format(FoodGoodsBLL.foodGoodsKey, miAppFood);
                RedisUtil.Remove(key);//清除产品缓存列表
                return Json(new { Success = true, Msg = "成功" });
            }
            else
                return Json(new { Success = false, Msg = "失败" });
        }




        /// <summary>
        /// 查找规格列表根据餐饮店铺Id
        /// </summary>
        public ActionResult GetAttrByFoodId(int foodId)
        {
            Food miAppFood = FoodBLL.SingleModel.GetModel(foodId);
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "店铺信息有误" }, JsonRequestBehavior.AllowGet);
            }
            List<FoodGoodsAttr> list = FoodGoodsAttrBLL.SingleModel.GetList($"FoodId={miAppFood.Id} and State>=0");
            return Json(new { isok = true, datas = list }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 添加规格
        /// </summary>
        public ActionResult AddFoodAttr(int foodId, string AttrName)
        {
            Food miAppFood = FoodBLL.SingleModel.GetModel(foodId);
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "店铺信息有误" }, JsonRequestBehavior.AllowGet);
            }
            List<FoodGoodsAttr> list = FoodGoodsAttrBLL.SingleModel.GetList($" FoodId = {miAppFood.Id} and State >=0 ");
            if (list.Count >= 10)
            {
                return Json(new { isok = false, msg = "规格最多只能添加10个" }, JsonRequestBehavior.AllowGet);
            }
            if (list.Any(x => x.AttrName.Equals(AttrName)))
            {
                return Json(new { isok = false, msg = "存在相同规格名,请重新输入！" }, JsonRequestBehavior.AllowGet);
            }

            FoodGoodsAttr attr = new FoodGoodsAttr();
            attr.FoodId = miAppFood.Id;
            attr.AttrName = AttrName;
            attr.State = 0;
            FoodGoodsAttrBLL.SingleModel.Add(attr);
            return Json(new { isok = true }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 查找属性列表
        /// </summary>
        public ActionResult GetFoodSpecByAttrId(int gs_id)
        {
            FoodGoodsAttr attr = FoodGoodsAttrBLL.SingleModel.GetModel(gs_id);
            if (attr == null)
            {
                return Json(new { isok = false, msg = "规格信息有误" }, JsonRequestBehavior.AllowGet);
            }
            List<FoodGoodsSpec> list = FoodGoodsSpecBLL.SingleModel.GetList($"AttrId={attr.Id} and State>=0");
            return Json(new { isok = true, datas = list }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 添加属性
        /// </summary>
        public ActionResult AddFoodSpec(int gs_id, string SpecName)
        {
            FoodGoodsAttr attr = FoodGoodsAttrBLL.SingleModel.GetModel(gs_id);
            if (attr == null)
            {
                return Json(new { isok = false, msg = "规格信息有误" }, JsonRequestBehavior.AllowGet);
            }
            List<FoodGoodsSpec> goodsSpecList = FoodGoodsSpecBLL.SingleModel.GetList($"AttrId={gs_id} and State>=0");
            if (goodsSpecList.Count >= 10)
            {
                return Json(new { isok = false, msg = "每个规格最多能添加10个属性哦" });
            }
            if (goodsSpecList.Any(m => m.SpecName == SpecName))
            {
                return Json(new { isok = false, msg = "属性名称已存在 , 请重新添加" });
            }

            FoodGoodsSpec spec = new FoodGoodsSpec();
            spec.AttrId = attr.Id;
            spec.SpecName = SpecName;
            spec.State = 0;
            FoodGoodsSpecBLL.SingleModel.Add(spec);
            return Json(new { isok = true }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 添加或编辑菜品规格
        /// </summary>
        public ActionResult FoodAttrSpec_detail(int goods_id = 0, int gs_id = 0, string AttrName = "", int FoodId = 0, int spec_id = 0, string SpecName = "", int remove_gs_id = 0, int remove_spec_id = 0, int goodtype = 0)
        {
            string inputdisabel = "";
            if (goods_id > 0 && goodtype == (int)EntGoodsType.拼团产品)
            {
                inputdisabel = "disabled";
            }

            StringBuilder Str = new StringBuilder();
            //查询菜品规格属性  页面加载结果
            string specids = "";
            FoodGoodsAttr attrM = new FoodGoodsAttr();
            List<FoodGoodsAttrDetail> AttrDetailList = new List<FoodGoodsAttrDetail>();
            if (goods_id > 0)
            {
                FoodGoods GoodsM = FoodGoodsBLL.SingleModel.GetModel(goods_id);
                if (GoodsM != null)
                {
                    AttrDetailList = GoodsM.GASDetailList;
                }
                List<FoodGoodsAttrSpec> gslist = FoodGoodsAttrSpecBLL.SingleModel.GetList($"FoodGoodsId={goods_id}");
                specids = string.Join(",", gslist.Select(m => m.SpecId).Distinct());
            }
            else if (spec_id > 0 || SpecName != "") //根据属性id或者名称以及规格id生成表格  添加规格属性结果
            {
                string ids = "";
                if (Session["miniappfoodgoodsspecids"] != null)
                {
                    ids = Session["miniappfoodgoodsspecids"].ToString();
                }
                if (spec_id > 0)
                {
                    if (ids == "")
                    {
                        specids = spec_id.ToString();
                    }
                    else
                    {
                        specids = ids + "," + spec_id;
                    }
                }
                else
                {
                    FoodGoodsSpec spec = FoodGoodsSpecBLL.SingleModel.GetModel($"AttrId={gs_id} and SpecName='{SpecName}' and State>=0");
                    if (spec == null)
                    {
                        return Content("");
                    }
                    if (ids == "")
                    {
                        specids = spec.Id.ToString();
                    }
                    else
                    {
                        specids = ids + "," + spec.Id;
                    }
                }
            }
            else if (gs_id > 0 || AttrName != "") //根据规格id或者名称生成表格 添加规格结果
            {
                string ids = "";
                if (Session["miniappfoodgoodsspecids"] != null)
                {
                    ids = Session["miniappfoodgoodsspecids"].ToString();
                }
                specids = ids;
                if (gs_id > 0)
                {
                    attrM = FoodGoodsAttrBLL.SingleModel.GetModel(gs_id);
                }
                else
                {
                    attrM = FoodGoodsAttrBLL.SingleModel.GetModel($"FoodId={FoodId} and AttrName='{AttrName}' and State>=0");
                }
                if (attrM == null)
                {
                    return Content("");
                }
            }
            else if (remove_gs_id > 0) //删除规格结果
            {
                string ids = "";
                if (Session["miniappfoodgoodsspecids"] != null)
                {
                    ids = Session["miniappfoodgoodsspecids"].ToString();
                }
                if (ids != "")
                {
                    List<FoodGoodsSpec> specnewlist = FoodGoodsSpecBLL.SingleModel.GetList($"Id in ({ids}) and AttrId<>{remove_gs_id}");
                    specids = string.Join(",", specnewlist.Select(m => m.Id).Distinct());
                }
                else
                {
                    specids = "";
                }
            }
            else if (remove_spec_id > 0) //删除属性结果
            {
                string ids = "";
                if (Session["miniappfoodgoodsspecids"] != null)
                {
                    ids = Session["miniappfoodgoodsspecids"].ToString();
                }
                string[] strids = ids.Split(',');
                List<string> list = new List<string>(strids);
                if (list.IndexOf(remove_spec_id.ToString()) >= 0)
                {
                    list.Remove(remove_spec_id.ToString());
                }
                strids = list.ToArray();
                specids = string.Join(",", strids);
            }
            else
            {
                specids = "";
            }
            //更新集合
            Session["miniappfoodgoodsspecids"] = specids;

            //获取所有属性集合
            List<FoodGoodsSpec> specalllist = new List<FoodGoodsSpec>();
            List<FoodGoodsAttr> attralllist = new List<FoodGoodsAttr>();
            if (specids != "")
            {
                specalllist = FoodGoodsSpecBLL.SingleModel.GetList($"Id in ({specids})");
                attralllist = FoodGoodsAttrBLL.SingleModel.GetList($"Id in ({string.Join(",", specalllist.Select(m => m.AttrId).Distinct())})");
            }
            if (attrM.Id > 0)
            {
                attralllist.Add(attrM);
            }
            #region 打印规格与属性
            Str.Append("<!--规格与规格值-->\n");
            Str.Append("<input name=\"add_spec\" type=\"hidden\"/>\n");
            foreach (FoodGoodsAttr attr in attralllist)
            {
                Str.Append("<ul class=\"removespec\">\n");
                Str.Append("<li class=\"clearfix mt-spacing guige-li\">\n");
                Str.Append($"<div class=\"fl\">{attr.AttrName}</div>\n");
                if (string.IsNullOrEmpty(inputdisabel))
                {
                    Str.Append($"<div class=\"fr pointer\" style=\"color: #27f;\" gs_id=\"{attr.Id}\">删除</div>\n");
                }
                Str.Append("</li>\n");
                Str.Append("<li class=\"clearfix\" style=\"padding: 10px; \">\n");
                var speclist = specalllist.Where(m => m.AttrId == attr.Id);
                int i = 0;
                foreach (FoodGoodsSpec spec in speclist)
                {
                    i++;
                    Str.Append("<div class=\"fl\">\n");
                    Str.Append("<div class=\"guige-zhi\">\n");
                    Str.Append($"<span>{spec.SpecName}</span>\n");
                    //if (string.IsNullOrEmpty(inputdisabel))
                    //{
                    //    Str.Append($"<a class=\"guige-zhi-close\" spec_id=\"{spec.Id}\" guige_index=\"{i}\"></a>\n");
                    //}
                    Str.Append($"<a class=\"guige-zhi-close\" spec_id=\"{spec.Id}\" guige_index=\"{i}\"></a>\n");
                    Str.Append("</div>\n");
                    Str.Append("</div>\n");
                }
                Str.Append("<div class=\"guige-tianjia\" >\n");
                if (string.IsNullOrEmpty(inputdisabel))
                {
                    Str.Append($"<a class=\"gsps\" gs_id=\"{attr.Id}\">+添加</a>\n");
                }
                Str.Append("</div>\n");
                Str.Append("</li>\n");
                Str.Append("</ul>\n");
            }

            #endregion

            #region 打印表格
            if (specalllist.Count > 0)
            {
                //重新获取规格集合
                List<FoodGoodsAttr> attrlist = FoodGoodsAttrBLL.SingleModel.GetList($"Id in ({string.Join(",", specalllist.Select(m => m.AttrId).Distinct())})");

                //开始打印表格
                Str.Append("<!--多规格表格-->\n");
                Str.Append("<table class=\"mt-spacing spgl-spgg-table more\" id=\"table_spec\" >\n");
                //打印抬头
                Str.Append("<tr>\n");
                foreach (FoodGoodsAttr attr in attrlist)
                {
                    Str.Append($"<th style=\"text-align: center;\">{attr.AttrName}</th>\n");
                }
                //判断是否是拼团
                if (goodtype == (int)EntGoodsType.拼团产品)
                {
                    Str.Append("<th style=\"text-align: center;\">原价</th>\n");
                    Str.Append("<th style=\"text-align: center;\">拼团价</th>\n");
                }
                Str.Append("<th style=\"text-align: center;\">" + (goodtype == (int)EntGoodsType.拼团产品 ? "单买价" : "价格") + "</th>\n");
                Str.Append("<th style=\"text-align: center;\">库存</th>\n");
                Str.Append("</tr>\n");
                //打印内容
                List<FoodGoodsSpec> speclist1 = specalllist.Where(m => m.AttrId == attrlist[0].Id).ToList<FoodGoodsSpec>();
                List<FoodGoodsSpec> speclist2 = new List<FoodGoodsSpec>();
                List<FoodGoodsSpec> speclist3 = new List<FoodGoodsSpec>();
                int rowspan1 = 1, rowspan2 = 1;
                if (attrlist.Count > 1)
                {
                    speclist2 = specalllist.Where(m => m.AttrId == attrlist[1].Id).ToList<FoodGoodsSpec>();
                    rowspan1 = speclist2.Count;
                }
                if (attrlist.Count > 2)
                {
                    speclist3 = specalllist.Where(m => m.AttrId == attrlist[2].Id).ToList<FoodGoodsSpec>();
                    rowspan1 = speclist2.Count * speclist3.Count;
                    rowspan2 = speclist3.Count;
                }
                for (int i = 0; i < speclist1.Count(); i++)
                {
                    Str.Append("<tr>\n");
                    Str.Append($"<td rowspan=\"{rowspan1}\">{speclist1[i].SpecName}</td>\n");
                    if (speclist2.Count() <= 0)
                    {
                        string gsp_ids = speclist1[i].Id + "_";
                        string price = "";
                        string count = "";
                        string groupPrice = "";
                        string originalPrice = "";
                        //编辑菜品 有价格与数量
                        if (AttrDetailList.Count > 0)
                        {
                            FoodGoodsAttrDetail attrdetail = AttrDetailList.Where(m => m.id == gsp_ids).FirstOrDefault();
                            if (attrdetail.price > 0)
                            {
                                originalPrice = (attrdetail.originalPrice * 0.01).ToString("0.00");//原价
                                price = (attrdetail.price * 0.01).ToString("0.00");//单买价
                                groupPrice = (attrdetail.groupPrice * 0.01).ToString("0.00");//团购价
                                count = attrdetail.count.ToString();
                            }
                        }
                        //判断是否是拼团
                        if (goodtype == (int)EntGoodsType.拼团产品)
                        {
                            Str.Append($"<td><input class=\"spec_originalPrice\" {inputdisabel} name=\"originalPrice_{speclist1[i].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{originalPrice}\" /></td>\n");
                            Str.Append($"<td><input class=\"spec_groupPrice\" {inputdisabel}  name=\"groupPrice_{speclist1[i].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{groupPrice}\" /></td>\n");
                        }
                        Str.Append($"<td><input class=\"spec_price\" {inputdisabel} name=\"price_{speclist1[i].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{price}\" /></td>\n");
                        Str.Append($"<td><input class=\"spec_count\" name=\"count_{speclist1[i].Id}\" type=\"text\" value=\"{count}\" /></td>\n");
                        Str.Append("</tr>\n");
                    }
                    else
                    {
                        for (int j = 0; j < speclist2.Count(); j++)
                        {
                            if (j > 0)
                            {
                                Str.Append("<tr>\n");
                            }
                            Str.Append($"<td rowspan=\"{rowspan2}\">{speclist2[j].SpecName}</td>\n");
                            if (speclist3.Count() <= 0)
                            {
                                string gsp_ids = speclist1[i].Id + "_" + speclist2[j].Id + "_";
                                string price = "";
                                string count = "";
                                string groupPrice = "";
                                string originalPrice = "";
                                //编辑菜品 有价格与数量
                                if (AttrDetailList.Count > 0)
                                {
                                    FoodGoodsAttrDetail attrdetail = AttrDetailList.Where(m => m.id == gsp_ids).FirstOrDefault();
                                    if (attrdetail.price > 0)
                                    {
                                        originalPrice = (attrdetail.originalPrice * 0.01).ToString("0.00");//原价
                                        price = (attrdetail.price * 0.01).ToString();
                                        groupPrice = (attrdetail.groupPrice * 0.01).ToString("0.00");//团购价
                                        count = attrdetail.count.ToString();
                                    }
                                }

                                //判断是否是拼团
                                if (goodtype == (int)EntGoodsType.拼团产品)
                                {
                                    Str.Append($"<td><input class=\"spec_originalPrice\" {inputdisabel}  name=\"originalPrice_{speclist1[i].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{originalPrice}\" /></td>\n");
                                    Str.Append($"<td><input class=\"spec_groupPrice\" {inputdisabel}  name=\"groupPrice_{speclist1[i].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{groupPrice}\" /></td>\n");
                                }

                                Str.Append($"<td><input class=\"spec_price\" {inputdisabel}  name=\"price_{speclist1[i].Id}_{speclist2[j].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{price}\" /></td>\n");
                                Str.Append($"<td><input class=\"spec_count\" name=\"count_{speclist1[i].Id}_{speclist2[j].Id}\" type=\"text\" value=\"{count}\" /></td>\n");
                                Str.Append("</tr>\n");
                            }
                            else
                            {
                                for (int k = 0; k < speclist3.Count(); k++)
                                {
                                    if (k > 0)
                                    {
                                        Str.Append("<tr>\n");
                                    }
                                    string gsp_ids = speclist1[i].Id + "_" + speclist2[j].Id + "_" + speclist3[k].Id + "_";
                                    string price = "";
                                    string count = "";
                                    string groupPrice = "";
                                    string originalPrice = "";
                                    //编辑菜品 有价格与数量
                                    if (AttrDetailList.Count > 0)
                                    {
                                        FoodGoodsAttrDetail attrdetail = AttrDetailList.Where(m => m.id == gsp_ids).FirstOrDefault();
                                        if (attrdetail.price > 0)
                                        {
                                            originalPrice = (attrdetail.originalPrice * 0.01).ToString("0.00");//原价
                                            price = (attrdetail.price * 0.01).ToString();
                                            groupPrice = (attrdetail.groupPrice * 0.01).ToString("0.00");//团购价
                                            count = attrdetail.count.ToString();
                                        }
                                    }

                                    Str.Append($"<td rowspan=\"1\">{speclist3[k].SpecName}</td>\n");
                                    //判断是否是拼团
                                    if (goodtype == (int)EntGoodsType.拼团产品)
                                    {
                                        Str.Append($"<td><input class=\"spec_originalPrice\" {inputdisabel}  name=\"originalPrice_{speclist1[i].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{originalPrice}\" /></td>\n");
                                        Str.Append($"<td><input class=\"spec_groupPrice\" {inputdisabel}  name=\"groupPrice_{speclist1[i].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{groupPrice}\" /></td>\n");
                                    }
                                    Str.Append($"<td><input class=\"spec_price\" {inputdisabel}  name=\"price_{speclist1[i].Id}_{speclist2[j].Id}_{speclist3[k].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{price}\" /></td>\n");
                                    Str.Append($"<td><input class=\"spec_count\" name=\"count_{speclist1[i].Id}_{speclist2[j].Id}_{speclist3[k].Id}\" type=\"text\" value=\"{count}\" /></td>\n");
                                    Str.Append("</tr>\n");
                                }
                            }
                        }
                    }
                }
                //底部批量设置
                Str.Append("<tr>\n");
                //判断是否是拼团
                if (goodtype == (int)EntGoodsType.拼团产品)
                {
                    Str.Append($"<td colspan=\"{attrlist.Count + 4}\">\n");
                }
                else
                {
                    Str.Append($"<td colspan=\"{attrlist.Count + 2}\">\n");
                }
                Str.Append("<div style=\"float:left;\">\n批量设置:\n");
                Str.Append("<span style=\"display: none;\" id=\"bt_inp\">\n");
                //判断是否是拼团
                if (goodtype == (int)EntGoodsType.拼团产品)
                {
                    Str.Append("<input placeholder=\"输入原价\" class=\"batch_all\" name=\"batch_originalPrice\"/>\n");
                    Str.Append("<input placeholder=\"输入团购价\" class=\"batch_all\" name=\"batch_groupPrice\"/>\n");
                }
                Str.Append("<input placeholder=\"输入价格\" class=\"batch_all\" name=\"batch_price\"/>\n");
                Str.Append("<input placeholder=\"输入库存\" class=\"batch_all\" name=\"batch_count\"/>\n");
                Str.Append("<a style=\"color: #07d;\" id=\"batch_save\">保存</a>\n");
                Str.Append("<a style=\"color: #07d;\" id=\"batch_cancel\">取消</a>\n");
                Str.Append("</span>\n");
                Str.Append("<span id=\"bt_btn\">\n");
                //判断是否是拼团
                if (goodtype == (int)EntGoodsType.拼团产品 && string.IsNullOrEmpty(inputdisabel))
                {
                    Str.Append("<a style=\"color: #07d;\" class=\"batch_all_click\" id=\"batch_originalPrice\">原价</a>\n");
                    Str.Append("<a style=\"color: #07d;\" class=\"batch_all_click\" id=\"batch_groupPrice\">团购价</a>\n");
                }
                if (string.IsNullOrEmpty(inputdisabel))
                {
                    Str.Append("<a style=\"color: #07d;\" class=\"batch_all_click\" id=\"batch_price\">价格</a>\n");
                }
                Str.Append("<a style=\"color: #07d;\" class=\"batch_all_click\" id=\"batch_count\">库存</a>\n");
                Str.Append("</span>\n");
                Str.Append("</div>\n");
                Str.Append("</td>\n");
                Str.Append("</tr>\n");
                Str.Append("<table>");
            }
            #endregion

            return Content(Str.ToString());
        }

        #endregion

        #region 餐饮运费模板栏目操作
        [HttpGet]
        public ActionResult AddFoodFreight(int appId)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.appId = appId;

            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.FoodId = miAppFood.Id;

            List<FoodFreightTemplate> feight = FoodFreightTemplateBLL.SingleModel.GetList($"FoodId = {miAppFood.Id}");
            return View(feight);
        }
        /// <summary>
        /// 添加运费模板
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFoodFreight(int appId, FoodFreightTemplate freightTmplate)
        {
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(freightTmplate.Name.Trim()))
            {
                return Json(new { isok = false, msg = "模板名称不可为空！" }, JsonRequestBehavior.AllowGet);
            }

            if (freightTmplate.FoodId == 0)
            {
                return Json(new { isok = false, msg = "餐饮店铺id不能为0！" }, JsonRequestBehavior.AllowGet);
            }

            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            //var freightList = new C_FreightTemplateBLL().GetList();
            int resultInt = 0;//
            if (freightTmplate.Id == 0)//添加
            {
                object result = FoodFreightTemplateBLL.SingleModel.Add(new FoodFreightTemplate() { BaseCount = freightTmplate.BaseCount, FoodId = freightTmplate.FoodId, Name = freightTmplate.Name, BaseCost = freightTmplate.BaseCost, ExtraCost = freightTmplate.ExtraCost,/*IsDefault = freightTmplate.IsDefault,*/CreateTime = DateTime.Now });
                if (int.Parse(result.ToString()) > 0)
                {
                    return Json(new { isok = true, msg = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else//编辑
            {
                FoodFreightTemplate updateModel = FoodFreightTemplateBLL.SingleModel.GetModel(freightTmplate.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到模板" });
                }
                else
                {
                    updateModel.IsDefault = freightTmplate.IsDefault;
                    updateModel.Name = freightTmplate.Name;
                    updateModel.BaseCost = freightTmplate.BaseCost;
                    updateModel.ExtraCost = freightTmplate.ExtraCost;
                    updateModel.BaseCount = freightTmplate.BaseCount;
                    //只修改名称，排序，freightTmplate
                    resultInt = FoodFreightTemplateBLL.SingleModel.Update(updateModel) ? 1 : 0;
                }
                return Json(new { isok = true, msg = "修改成功" });
            }

            //IsReferred(int ftid, int storeId)
            return Json(new { isok = false, msg = "系统错误！" });
        }

        /// <summary>
        /// 删除运费模板
        /// </summary>
        /// <param name="id"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult delFoodFreightTemplate(FoodFreightTemplate freightTmplate)
        {
            
            if (FoodFreightTemplateBLL.SingleModel.IsReferred(freightTmplate.Id, freightTmplate.FoodId))
            {
                return Json(new { isok = false, msg = "该模板有菜品在使用,暂时不可删除！" });
            }
            if (FoodFreightTemplateBLL.SingleModel.Delete(freightTmplate.Id) > 0)
            {
                return Json(new { isok = true, msg = "删除成功！" });
            }
            else
            {
                return Json(new { isok = false, msg = "删除失败！" });
            }
        }
        /// <summary>
        /// 运费模板详情
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>

        public ActionResult getFoodFreight(int Id)
        {
            return Json(FoodFreightTemplateBLL.SingleModel.GetModel(Id), JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region 打印机管理
        /// <summary>
        /// 打印机管理列表
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult FoodPrintList(int appid)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appid, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            Food foodStore = FoodBLL.SingleModel.GetModel($" appId = {app.Id}  ");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            List<FoodPrints> foodPrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {foodStore.Id} and accountId = '{dzaccount.OpenId}' and state >= 0 and industrytype=1") ?? new List<FoodPrints>();
            ViewBag.appId = appid;
            ViewBag.FoodId = foodStore.Id;
            return View(foodPrintList);
        }

        /// <summary>
        /// 查看打印机终端
        /// </summary>
        /// <param name="print"></param>
        /// <returns></returns>
        public ActionResult getPrintForm(int appId, int printId, int edit = 0)
        {
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_error！" }, JsonRequestBehavior.AllowGet);
            }
            Food foodStore = FoodBLL.SingleModel.GetModel($" appid = {appId} ");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_null！" }, JsonRequestBehavior.AllowGet);
            }
            FoodPrints print = FoodPrintsBLL.SingleModel.GetModel(printId) ?? new FoodPrints();
            //先访问易连云接口添加,成功后才在系统内添加记录

            ViewBag.edit = edit;
            return PartialView("_PartialPrintItem", print);
        }

        /// <summary>
        /// 添加打印机终端
        /// </summary>
        /// <param name="print"></param>
        /// <returns></returns>
        public ActionResult addPrint(FoodPrints print)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            if (print.FoodStoreId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_error！" }, JsonRequestBehavior.AllowGet);
            }
            Food foodStore = FoodBLL.SingleModel.GetModel(print.FoodStoreId);
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_null！" }, JsonRequestBehavior.AllowGet);
            }
            //先访问易连云接口添加,成功后才在系统内添加记录
            PrintErrorData returnMsg = FoodYiLianYunPrintHelper.addPrinter(print.APIKey, print.UserId, print.PrintNo, print.PrintKey, print.Telphone, print.UserName, print.Name);
            if (returnMsg.errno != 1)
            {
                return Json(new { isok = false, msg = returnMsg.error }, JsonRequestBehavior.AllowGet);
            }
            print.industrytype = 1;
            print.accountId = dzaccount.OpenId;
            print.State = 0;
            print.CreateDate = DateTime.Now;
            int id = Convert.ToInt32(FoodPrintsBLL.SingleModel.Add(print));
            if (id > 0)
            {
                return Json(new { isok = true, msg = "添加成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false, msg = "添加失败！" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除打印机
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="printId"></param>
        /// <returns></returns>
        public ActionResult deletePrint(int appid, int printId = 0)
        {
            Food foodStore = FoodBLL.SingleModel.GetModel($" appid = {appid} ");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_null！" }, JsonRequestBehavior.AllowGet);
            }
            FoodPrints print = FoodPrintsBLL.SingleModel.GetModel(printId);
            if (print == null)
            {
                Json(new { isok = false, msg = "系统繁忙printModel_null！" }, JsonRequestBehavior.AllowGet);
            }

            //无论打印机解绑失败与否都让用户删除记录,避免用户在第三方删除导致流程异常删除不了打印机
            ////先访问易连云接口删除,成功后才在系统内操作记录
            string returnMsg = FoodYiLianYunPrintHelper.deletePrinter(print.APIKey, print.UserId, print.PrintNo, print.PrintKey);
            //if (Convert.ToInt32(returnMsg) == 4)
            //{
            //    return Json(new { isok = false, msg = "删除失败！" }, JsonRequestBehavior.AllowGet);
            //}
            print.State = -1;
            print.UpdateDate = DateTime.Now;
            bool result = FoodPrintsBLL.SingleModel.Update(print, "State,UpdateDate");
            if (result)
            {
                return Json(new { isok = true, msg = "删除成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false, msg = "删除失败！" }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        /// <summary>
        /// 拼接地址
        /// </summary>
        /// <param name="areaCodeList"></param>
        /// <returns></returns>
        public List<SelectListItem> GetConcatAddressByAreaCode(List<string> areaCodeList)
        {
            string codes = areaCodeList.Count == 0 ? "0" : string.Join(",", areaCodeList);
            string sql = "select c.code,c.parentCode,c.level,CONCAT(a.name,b.name,c.name) name from c_area a LEFT JOIN c_area b on a.code = b.parentCode LEFT JOIN c_area c on b.code = c.parentCode where c.level = 3 and c.code in (" + codes + ") ";
            List<C_Area> areaList = C_AreaBLL.SingleModel.GetListBySql(sql);
            return areaList.Select(area => new SelectListItem
            {
                Value = area.Code.ToString(),
                Text = area.Name
            }).ToList();
        }


        #region 订单

        public ActionResult FoodGoodsOrderList(int appId, int type = (int)miniAppFoodOrderType.堂食, string orderNum = "", int orderState = -999, int foodGoodsType = -999, int pageIndex = 1, int pageSize = 10)
        {
            //配送方式
            int sentway = Context.GetRequestInt("SendWay", -999);
            //提货码
            string verificationNum = Context.GetRequest("verificationNum", string.Empty);

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            ViewBag.appId = appId;

            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                return View("PageError", new Return_Msg() { Msg = "找不到该餐饮店铺!", code = "500" });
            }
            string nickName = Context.GetRequest("nickName", string.Empty);

            int totleCount = 0;
            string startTime = Context.GetRequest("startTime", string.Empty);
            string endTime = Context.GetRequest("endTime", string.Empty);
            string tableNo = Context.GetRequest("tableNo", string.Empty);
            int searchToday = Context.GetRequestInt("searchToday", 0);//1:搜索今日订单
            if (searchToday == 1)
            {
                startTime = $"{DateTime.Now.ToString("yyyy-MM-dd")} 00:00:00";
                endTime = $"{DateTime.Now.ToString("yyyy-MM-dd")} 23:59:59";
            }
            //var orderList = _miniappfoodgoodsorderBll.GetOrderByParames(miAppFood.Id, type, orderNum, orderState, foodGoodsType, pageIndex, pageSize);
            List<FoodAdminGoodsOrder> goodOrders = new MiniappAdminGoodsOrderBLL().GetAdminList(sentway, miAppFood.Id, out totleCount, type, orderNum, orderState, foodGoodsType, pageIndex, pageSize, nickName, 0, verificationNum,startTime,endTime,tableNo);
            if (goodOrders != null && goodOrders.Count > 0)
            {
                FoodTable table = new FoodTable();
                foreach (var goodsOrder in goodOrders)
                {
                    goodsOrder.TableName = goodsOrder.TablesNo.ToString();
                    if (!string.IsNullOrEmpty(goodsOrder.attribute))
                    {
                        goodsOrder.attrbuteModel = JsonConvert.DeserializeObject<FoodGoodsOrderAttr>(goodsOrder.attribute);
                        if (goodsOrder.attrbuteModel.isNewTableNo)
                        {
                            table = FoodTableBLL.SingleModel.GetModelById(goodsOrder.TablesNo);
                            if (table != null)
                            {
                                goodsOrder.TableName = table.Scene;
                            }
                        }
                    }


                }
            }
            ViewBag.StartTime = startTime;
            ViewBag.EndTime = endTime;
            ViewBag.TableNo = tableNo;
            ViewBag.TotalCount = totleCount;
            ViewBag.GoodsTypeList = FoodGoodsTypeBLL.SingleModel.GetList($" FoodId = {miAppFood.Id} and state >= 0") ?? new List<FoodGoodsType>();
            ViewBag.pageSize = pageSize;
            ViewBag.orderNum = orderNum;
            ViewBag.orderState = orderState;
            ViewBag.foodGoodsType = foodGoodsType;
            ViewBag.type = type;
            ViewBag.nickName = nickName;
            ViewBag.verificationNum = verificationNum;
            ViewBag.pageIndex = pageIndex;
            ViewBag.SendWay = sentway;
            //var orderList = _miniappfoodGoodsOrderBll.GetList($" StoreId = {miAppFood.Id}" +
            //                            $"{ ((type == 10 || !Enum.IsDefined(typeof(miniAppFoodOrderType),type)) ? "" : $" and OrderType = {type} ")}");
            ViewBag.foodlabellist = FoodLabelBLL.SingleModel.GetList($" foodstoreid = {miAppFood.Id} and State >= 0") ?? new List<FoodLabel>();

            //判断是否有达达订单
            _dadaOrderBLL.GetFoodDadaOrderState<FoodAdminGoodsOrder>(ref goodOrders, appId);

            //判断是否有团订单
            EntGroupSponsorBLL.SingleModel.GetFoodSponsorState(ref goodOrders);

            return View(goodOrders);
        }


        /// <summary>
        /// 导出订单数据
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="orderNum"></param>
        /// <param name="orderState"></param>
        /// <param name="foodGoodsType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public void ExportOrders(int appId, int type = (int)miniAppFoodOrderType.堂食, string orderNum = "", int orderState = -999, int foodGoodsType = -999, int pageIndex = 1, int pageSize = 10)
        {
            //配送方式
            int sentway = Context.GetRequestInt("SendWay", -999);

            if (dzaccount == null)
            {
                Response.Write("系统繁忙auth_null");
                return;
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                Response.Write("非法操作(无权限)");
                return;
            }
            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                Response.Write("找不到店铺");
                return;
            }
            string nickName = Context.GetRequest("nickName", string.Empty);

            int totleCount = 0;
            int export = Context.GetRequestInt("export", 0);
            List<FoodAdminGoodsOrder> goodOrders = new MiniappAdminGoodsOrderBLL().GetAdminList(sentway, miAppFood.Id, out totleCount, type, orderNum, orderState, foodGoodsType, pageIndex, export > 0 ? 10000000 : pageSize, nickName, export);
            DataTable exportTable = new DataTable();
            exportTable.Columns.AddRange(new DataColumn[] {
                        new DataColumn("会员名称"),
                        new DataColumn("订单号"),
                        new DataColumn("订单类型"),
                         new DataColumn("菜品数量"),
                        new DataColumn("订单金额(元)"),
                        new DataColumn("实收金额(元)"),
                         new DataColumn("支付方式"),
                           new DataColumn("下单时间"),
                        new DataColumn("订单状态"),
                         new DataColumn("订单详情"),
                          new DataColumn("订单备注")
                    });

            if (type == 0)
            {
                //表示堂食
                exportTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("桌台号")
                });
            }
            else
            {
                //表示外卖
                exportTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("配送地址"),
                  new DataColumn("配送方式"),
                  new DataColumn("收货人"),
                  new DataColumn("收货电话")
                });
            }

            string filename = (type == 0 ? "堂食" : "外卖") + "订单导出";

            if (goodOrders != null && goodOrders.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (FoodAdminGoodsOrder item in goodOrders)
                {
                    DataRow dr = exportTable.NewRow();
                    dr[0] = item.NickName;
                    dr[1] = item.OrderNum;
                    dr[2] = Enum.GetName(typeof(miniAppFoodOrderType), item.OrderType);
                    dr[3] = item.QtyCount;
                    dr[4] = ((item.BuyPrice + item.ReducedPrice) * 0.01).ToString("0.00");
                    dr[5] = ((item.BuyPrice * 0.01).ToString("0.00"));
                    dr[6] = item.PayDate != Convert.ToDateTime("0001-01-01 00:00:00") ? Enum.GetName(typeof(miniAppBuyMode), item.BuyMode) : string.Empty;
                    dr[7] = item.CreateDateStr;
                    dr[8] = Enum.GetName(typeof(miniAppFoodOrderState), item.State);
                    List<FoodGoodsCart> cartModelList = FoodGoodsCartBLL.SingleModel.GetOrderDetail(item.Id);

                    foreach (FoodGoodsCart x in cartModelList)
                    {

                        sb.AppendLine($"{x.goodsMsg.GoodsName}\r\n规格:{x.SpecInfo}\r\n{(x.originalPrice * 0.01).ToString("0.00")}元 ×{x.Count}\r\n原金额:{((x.originalPrice * x.Count) * 0.01).ToString("0.00")}元\r\n实际金额:{((x.Price * x.Count) * 0.01).ToString("0.00")}元\r\n");
                    }
                    sb.AppendLine($"运费:{(item.FreightPrice * 0.01).ToString("0.00")}元");
                    dr[9] = sb.ToString();
                    sb.Clear();
                    dr[10] = item.Message;
                    if (type == 0)
                    {
                        FoodTable table = FoodTableBLL.SingleModel.GetModelById(item.TablesNo);
                        dr[11] = table == null ? item.TablesNo.ToString() : table.Scene;

                    }
                    else
                    {
                        dr[11] = item.Address;
                        dr[12] = Enum.GetName(typeof(miniAppOrderGetWay), item.GetWay);
                        dr[13] = item.AccepterName;
                        dr[14] = item.AccepterTelePhone;
                    }
                    exportTable.Rows.Add(dr);
                }

            }
            if (exportTable.Rows.Count <= 0)
            {
                Response.Write("没有数据");
                return;
            }

            ExcelHelper<Entity.MiniApp.User.UserForm>.Out2Excel(exportTable, filename);//导出

        }



        /// <summary>
        /// 订单菜品信息列表
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult getOrderDtlItem(int orderId)
        {
            List<FoodGoodsCart> cartModelList = FoodGoodsCartBLL.SingleModel.GetOrderDetail(orderId);
            return View("_PartialOrderDtlItem", cartModelList);
        }






        /// <summary>
        /// 订单状态变更
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderid"></param>
        /// <param name="state">订单新的状态</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult updateTheShopOrderState(int appId, int orderid, int state, string cancleRemark = "")
        {

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }


            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.appId = appId;

            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                FoodGoodsOrder miniappFoodGoodOrder = FoodGoodsOrderBLL.SingleModel.GetModel($" Id = {orderid} ");// and  OrderType ={(int)miniAppFoodOrderType.店内点餐 } ");
                if (miniappFoodGoodOrder == null)
                {
                    return Json(new { isok = false, msg = "找不到订单！" }, JsonRequestBehavior.AllowGet);

                }
                if (miniappFoodGoodOrder.OrderType == (int)miniAppFoodOrderType.堂食)
                {
                    #region 堂食
                    switch (state)
                    {
                        case (int)miniAppFoodOrderState.已取消:
                            //当前订单状态是否允许被取消
                            List<int> allowList1 = new List<int>();
                            allowList1.Add((int)miniAppFoodOrderState.待付款);
                            if (!allowList1.Contains(miniappFoodGoodOrder.State))
                            {
                                return Json(new { isok = false, msg = "此订单不允许取消！" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case (int)miniAppFoodOrderState.待就餐:
                            //当前订单状态是否允许被取消
                            List<int> allowList2 = new List<int>();
                            allowList2.Add((int)miniAppFoodOrderState.待接单);
                            if (!allowList2.Contains(miniappFoodGoodOrder.State))
                            {
                                return Json(new { isok = false, msg = "此订单不允许确认接单！" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case (int)miniAppFoodOrderState.退款中:
                            //当前订单状态是否允许被退款
                            List<int> allowList3 = new List<int>();
                            allowList3.Add((int)miniAppFoodOrderState.待接单);
                            allowList3.Add((int)miniAppFoodOrderState.待就餐);
                            if (!allowList3.Contains(miniappFoodGoodOrder.State))
                            {
                                return Json(new { isok = false, msg = "此订单不允许退款！" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case (int)miniAppFoodOrderState.已完成:
                            //当前订单状态是否允许完成
                            List<int> allowList4 = new List<int>();
                            allowList4.Add((int)miniAppFoodOrderState.待就餐);
                            if (!allowList4.Contains(miniappFoodGoodOrder.State) && miniappFoodGoodOrder.BuyMode != (int)miniAppBuyMode.线下支付)
                            {
                                return Json(new { isok = false, msg = "此订单不允许完成！" }, JsonRequestBehavior.AllowGet);
                            }

                            break;
                        default:
                            return Json(new { isok = false, msg = "订单状态错误,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
                            //break;
                    }


                    string updateColStr = "State";
                    switch (state)
                    {
                        case (int)miniAppFoodOrderState.已取消:

                            break;
                        case (int)miniAppFoodOrderState.待就餐:
                            miniappFoodGoodOrder.ConfDate = DateTime.Now;
                            miniappFoodGoodOrder.DistributeDate = DateTime.Now;
                            updateColStr += ",ConfDate,DistributeDate";
                            break;
                        case (int)miniAppFoodOrderState.退款中:
                            miniappFoodGoodOrder.outOrderDate = DateTime.Now;
                            miniappFoodGoodOrder.Remark = cancleRemark;
                            updateColStr += ",outOrderDate,Remark";
                            break;
                        case (int)miniAppFoodOrderState.已完成:
                            miniappFoodGoodOrder.AcceptDate = DateTime.Now;
                            updateColStr += ",AcceptDate";
                            break;
                        default:
                            break;
                    }
                    bool isSuccess = false;
                    int oldState = miniappFoodGoodOrder.State;
                    miniappFoodGoodOrder.State = state;
                    if (state == (int)miniAppFoodOrderState.已取消)
                    {
                        isSuccess = FoodGoodsOrderBLL.SingleModel.updateStock(miniappFoodGoodOrder, oldState);
                    }
                    else if (state == (int)miniAppFoodOrderState.退款中)
                    {
                        //log4net.LogHelper.WriteInfo(GetType(), "调用了退款BLL");
                        //退款接口 abel
                        if (miniappFoodGoodOrder.BuyMode == (int)miniAppBuyMode.微信支付)
                        {
                            isSuccess = FoodGoodsOrderBLL.SingleModel.outOrder(miniappFoodGoodOrder, oldState);
                        }
                        else if (miniappFoodGoodOrder.BuyMode == (int)miniAppBuyMode.储值支付)
                        {
                            SaveMoneySetUser userSaveMoney = SaveMoneySetUserBLL.SingleModel.getModelByUserId(miniappFoodGoodOrder.UserId) ?? new SaveMoneySetUser();
                            isSuccess = FoodGoodsOrderBLL.SingleModel.outOrderBySaveMoneyUser(miniappFoodGoodOrder, userSaveMoney, oldState);
                        }
                    }
                    else if (state == (int)miniAppFoodOrderState.已完成)
                    {
                        TransactionModel tranModel = new TransactionModel();
                        tranModel.Add(FoodGoodsOrderBLL.SingleModel.BuildUpdateSql(miniappFoodGoodOrder, updateColStr) + $" and state = {oldState}  ");
                        tranModel = FoodGoodsOrderBLL.SingleModel.addSalesCount(miniappFoodGoodOrder.Id, tranModel);
                        isSuccess = FoodGoodsOrderBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
                    }
                    else
                    {
                        isSuccess = FoodGoodsOrderBLL.SingleModel.Update(miniappFoodGoodOrder, updateColStr);
                    }

                    if (isSuccess)
                    {
                        if (state == (int)miniAppFoodOrderState.待就餐) //接单
                        {
                            //打印机列表
                            List<FoodPrints> foodPrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {miAppFood.Id} and appId = {app.Id} and state >= 0 ") ?? new List<FoodPrints>();
                            List<FoodGoodsCart> carlist = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId={miniappFoodGoodOrder.Id} and state=1");
                            FoodGoodsOrderBLL.SingleModel.PrintOrder(miAppFood, miniappFoodGoodOrder, carlist, foodPrintList, dzaccount);
                        }
                        else if (state == (int)miniAppFoodOrderState.退款中 && (oldState == (int)miniAppFoodOrderState.待接单 || oldState == (int)miniAppFoodOrderState.待就餐))
                        {
                            #region 发送餐饮订单拒接通知 模板消息
                            object postData = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(miniappFoodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮订单拒绝通知);
                            TemplateMsg_Miniapp.SendTemplateMessage(miniappFoodGoodOrder.UserId, SendTemplateMessageTypeEnum.餐饮订单拒绝通知, (int)TmpType.小程序餐饮模板, postData);
                            #endregion
                        }
                        else if (state == (int)miniAppFoodOrderState.退款中)
                        {
                            #region 餐饮退款成功通知 模板消息
                            object postData = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(miniappFoodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮退款成功通知);
                            TemplateMsg_Miniapp.SendTemplateMessage(miniappFoodGoodOrder.UserId, SendTemplateMessageTypeEnum.餐饮退款成功通知, (int)TmpType.小程序餐饮模板, postData);
                            #endregion
                        }
                        else if (state == (int)miniAppFoodOrderState.已完成)
                        {
                            //会员加消费金额
                            if (!VipRelationBLL.SingleModel.updatelevel(miniappFoodGoodOrder.UserId, "food"))
                            {
                                log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + miniappFoodGoodOrder.Id));
                            }
                        }
                        FoodGoodsOrderLogBLL.SingleModel.AddLog(miniappFoodGoodOrder.Id, dzaccount.OpenId, $" {Enum.GetName(typeof(miniAppFoodOrderState), state)} ");
                        return Json(new { isok = true, msg = "修改订单状态成功！" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { isok = false, msg = "修改订单状态失败！" + isSuccess }, JsonRequestBehavior.AllowGet);
                    //return Json(new { isok = true, msg = "修改订单状态成功！" }, JsonRequestBehavior.AllowGet);
                    #endregion
                }
                else
                {
                    #region 外卖
                    switch (state)
                    {
                        case (int)miniAppFoodOrderState.已取消:
                            //当前订单状态是否允许被取消
                            List<int> allowList1 = new List<int>();
                            allowList1.Add((int)miniAppFoodOrderState.待付款);
                            if (!allowList1.Contains(miniappFoodGoodOrder.State))
                            {
                                return Json(new { isok = false, msg = "此订单不允许取消！" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case (int)miniAppFoodOrderState.待送餐:
                            //当前订单状态是否允许被接单
                            List<int> allowList2 = new List<int>();
                            allowList2.Add((int)miniAppFoodOrderState.待接单);
                            allowList2.Add((int)miniAppFoodOrderState.拒绝退款);
                            if (!allowList2.Contains(miniappFoodGoodOrder.State))
                            {
                                return Json(new { isok = false, msg = "此订单不允许确认接单！" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case (int)miniAppFoodOrderState.退款中:
                            //当前订单状态是否允许被退款
                            List<int> allowList3 = new List<int>();
                            allowList3.Add((int)miniAppFoodOrderState.待接单);
                            allowList3.Add((int)miniAppFoodOrderState.待送餐);
                            allowList3.Add((int)miniAppFoodOrderState.待确认送达);
                            allowList3.Add((int)miniAppFoodOrderState.退款审核中);
                            allowList3.Add((int)miniAppFoodOrderState.拒绝退款);
                            if (!allowList3.Contains(miniappFoodGoodOrder.State))
                            {
                                return Json(new { isok = false, msg = "此订单不允许退款！" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case (int)miniAppFoodOrderState.待确认送达:
                            //当前订单状态是否允许被退款
                            List<int> allowList4 = new List<int>();
                            allowList4.Add((int)miniAppFoodOrderState.待送餐);
                            allowList4.Add((int)miniAppFoodOrderState.拒绝退款);
                            if (!allowList4.Contains(miniappFoodGoodOrder.State))
                            {
                                return Json(new { isok = false, msg = "此订单状态不允许改为确认送达！" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case (int)miniAppFoodOrderState.已完成:
                            //当前订单状态是否允许完成
                            List<int> allowList5 = new List<int>();
                            allowList5.Add((int)miniAppFoodOrderState.待确认送达);
                            if (!allowList5.Contains(miniappFoodGoodOrder.State))
                            {
                                return Json(new { isok = false, msg = "此订单不允许完成！" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case (int)miniAppFoodOrderState.拒绝退款:
                            //当前订单状态是否允许被接单
                            List<int> allowList6 = new List<int>();
                            allowList6.Add((int)miniAppFoodOrderState.退款审核中);
                            if (!allowList6.Contains(miniappFoodGoodOrder.State))
                            {
                                return Json(new { isok = false, msg = "此订单不允许退款失败！" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        default:
                            return Json(new { isok = false, msg = "订单状态错误,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
                            //break;
                    }


                    string updateColStr = "State";
                    switch (state)
                    {
                        case (int)miniAppFoodOrderState.已取消:
                            break;
                        case (int)miniAppFoodOrderState.待送餐:
                            miniappFoodGoodOrder.ConfDate = DateTime.Now;
                            updateColStr += ",ConfDate";
                            break;
                        case (int)miniAppFoodOrderState.退款中:
                            //调用退款接口 abel
                            miniappFoodGoodOrder.outOrderDate = DateTime.Now;
                            miniappFoodGoodOrder.Remark = cancleRemark;
                            updateColStr += ",outOrderDate,Remark";
                            break;
                        case (int)miniAppFoodOrderState.待确认送达:
                            miniappFoodGoodOrder.DistributeDate = DateTime.Now;
                            updateColStr += ",DistributeDate";
                            break;
                        case (int)miniAppFoodOrderState.已完成:
                            miniappFoodGoodOrder.AcceptDate = DateTime.Now;
                            updateColStr += ",AcceptDate";
                            break;
                        case (int)miniAppFoodOrderState.拒绝退款:

                            break;
                        default:
                            break;
                    }
                    int oldState = miniappFoodGoodOrder.State;
                    miniappFoodGoodOrder.State = state;


                    bool isSuccess = false;
                    if (state == (int)miniAppFoodOrderState.已取消)
                    {
                        isSuccess = FoodGoodsOrderBLL.SingleModel.updateStock(miniappFoodGoodOrder, oldState);
                    }
                    else if (state == (int)miniAppFoodOrderState.退款中)
                    {
                        if(miniappFoodGoodOrder.GetWay == (int)miniAppOrderGetWay.快跑者配送)
                        {
                            KPZOrderBLL.SingleModel.CancelOrder(appId, miniappFoodGoodOrder.StoreId, miniappFoodGoodOrder.Id);
                        }
                        
                        //退款接口 abel
                        //log4net.LogHelper.WriteInfo(GetType(), "调用了退款BLL");
                        //isSuccess = _miniappfoodgoodsorderBll.outOrder(miniappFoodGoodOrder, oldState);

                        if (miniappFoodGoodOrder.BuyMode == (int)miniAppBuyMode.微信支付)
                        {
                            isSuccess = FoodGoodsOrderBLL.SingleModel.outOrder(miniappFoodGoodOrder, oldState);
                        }
                        else if (miniappFoodGoodOrder.BuyMode == (int)miniAppBuyMode.储值支付)
                        {
                            SaveMoneySetUser userSaveMoney = SaveMoneySetUserBLL.SingleModel.getModelByUserId(miniappFoodGoodOrder.UserId) ?? new SaveMoneySetUser();
                            isSuccess = FoodGoodsOrderBLL.SingleModel.outOrderBySaveMoneyUser(miniappFoodGoodOrder, userSaveMoney, oldState);
                        }
                    }
                    else if (state == (int)miniAppFoodOrderState.已完成)
                    {
                        TransactionModel tranModel = new TransactionModel();
                        tranModel.Add(FoodGoodsOrderBLL.SingleModel.BuildUpdateSql(miniappFoodGoodOrder, updateColStr) + $" and state = {oldState}  ");
                        tranModel = FoodGoodsOrderBLL.SingleModel.addSalesCount(miniappFoodGoodOrder.Id, tranModel);
                        isSuccess = FoodGoodsOrderBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
                    }
                    else //if (_miniappfoodGoodsOrderBll.updateFoodOrderState(miniappFoodGoodOrder, oldState, updateColStr))
                    {
                        isSuccess = FoodGoodsOrderBLL.SingleModel.updateFoodOrderState(miniappFoodGoodOrder, oldState, updateColStr);
                    }

                    if (isSuccess)
                    {
                        List<FoodGoodsCart> carlist = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId={miniappFoodGoodOrder.Id} and state=1");
                        if (state == (int)miniAppFoodOrderState.退款中 && (oldState == (int)miniAppFoodOrderState.待接单))
                        {
                            #region 发送餐饮订单拒接通知 模板消息
                            object postData = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(miniappFoodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮订单拒绝通知);
                            TemplateMsg_Miniapp.SendTemplateMessage(miniappFoodGoodOrder.UserId, SendTemplateMessageTypeEnum.餐饮订单拒绝通知, (int)TmpType.小程序餐饮模板, postData);
                            #endregion

                        }
                        else if (state == (int)miniAppFoodOrderState.待确认送达)
                        {
                            #region 发送餐饮订单配送通知 模板消息
                            object postData = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(miniappFoodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮订单配送通知);
                            TemplateMsg_Miniapp.SendTemplateMessage(miniappFoodGoodOrder.UserId, SendTemplateMessageTypeEnum.餐饮订单配送通知, (int)TmpType.小程序餐饮模板, postData);
                            #endregion
                        }
                        else if (state == (int)miniAppFoodOrderState.退款中 &&
                                                   (oldState == (int)miniAppFoodOrderState.退款审核中 || oldState == (int)miniAppFoodOrderState.待接单 ||
                                                       oldState == (int)miniAppFoodOrderState.待送餐 || oldState == (int)miniAppFoodOrderState.待确认送达))
                        {
                            #region 发送餐饮退款成功通知 模板消息
                            object postData = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(miniappFoodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮退款成功通知);
                            TemplateMsg_Miniapp.SendTemplateMessage(miniappFoodGoodOrder.UserId, SendTemplateMessageTypeEnum.餐饮退款成功通知, (int)TmpType.小程序餐饮模板, postData);
                            #endregion
                        }
                        else if (state == (int)miniAppFoodOrderState.待送餐)
                        {
                            //打印机列表
                            List<FoodPrints> foodPrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {miAppFood.Id} and appId = {app.Id} and state >= 0 ") ?? new List<FoodPrints>();
                            FoodGoodsOrderBLL.SingleModel.PrintOrder(miAppFood, miniappFoodGoodOrder, carlist, foodPrintList, dzaccount);
                        }
                        else if (state == (int)miniAppFoodOrderState.已完成)
                        {
                            //会员加消费金额
                            if (!VipRelationBLL.SingleModel.updatelevel(miniappFoodGoodOrder.UserId, "food"))
                            {
                                log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + miniappFoodGoodOrder.Id));
                            }
                        }

                        return Json(new { isok = true, msg = "修改订单状态成功！" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { isok = false, msg = "修改订单状态失败！请刷新页面重试！" }, JsonRequestBehavior.AllowGet);

                    #endregion
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Json(new { isok = false, msg = "修改订单状态失败！请刷新页面重试！" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取订单退款失败原因
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult getOutOrderFailRemark(int orderId)
        {
            try
            {
                FoodGoodsOrder order = FoodGoodsOrderBLL.SingleModel.GetModel(orderId);
                CityMorders ctiyMorder = new CityMordersBLL().GetModel(order.OrderId);
                ReFundResult outOrderRsult = RefundResultBLL.SingleModel.GetModel($" transaction_id = '{ctiyMorder.trade_no}' and retype = 1");

                if (outOrderRsult == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "未知原因!", code = "500" });
                }

                return Content(outOrderRsult.err_code_des ?? outOrderRsult.return_msg);
            }
            catch (Exception)
            {
                return View("PageError", new Return_Msg() { Msg = "未知原因!", code = "500" });
            }
        }

        #region 模板消息

        /// <summary>
        /// 模板消息使用管理
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult TemplateMsgManager(int appId=0)
        {

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有授权!", code = "403" });
            }
            if (app.AppId.IsNullOrWhiteSpace())
            {
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            }

            //var miAppfood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            //if (miAppfood == null)
            //{
            //    return Content("找不到该餐饮店铺！");
            //}

            List<TemplateMsg> miniapptemplatemsg = TemplateMsgBLL.SingleModel.GetListByType((int)TmpType.小程序餐饮模板) ?? new List<TemplateMsg>();
            string msg = "";
            miniapptemplatemsg.ForEach(x =>
            {
                TemplateMsg_User newUserMsg = TemplateMsg_UserBLL.SingleModel.getModelByAppId(app.AppId, (int)TmpType.小程序餐饮模板, x.Id);
                if (newUserMsg == null)
                {
                    //往微信appid加模板消息
                    
                    addResultModel _addResult = MsnModelHelper.addMsnToMy(app.AppId, x.TitileId, x.ColNums.Split(','), ref msg);

                    newUserMsg = new TemplateMsg_User();
                    newUserMsg.AppId = app.AppId;
                    //newUserMsg.TmpId = miAppfood.Id;
                    newUserMsg.Ttypeid = x.Ttypeid;
                    newUserMsg.TmId = x.Id;
                    newUserMsg.ColNums = x.ColNums;
                    newUserMsg.TitleId = x.TitileId;
                    newUserMsg.State = 0;//关闭
                    newUserMsg.CreateDate = DateTime.Now;
                    newUserMsg.TemplateId = _addResult.template_id;//微信公众号内的模板Id
                    newUserMsg.TmgType = x.TmgType;

                    int result = Convert.ToInt32(TemplateMsg_UserBLL.SingleModel.Add(newUserMsg));
                }

                if (newUserMsg != null)
                {
                    x.openState = newUserMsg.State;
                }
            });
            return View(miniapptemplatemsg);
        }

        /// <summary>
        /// 启用模板
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult startTmg(int appId, int TempMsgId)
        {
            try
            {
                if (dzaccount == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
                }
                XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
                if (app == null)
                {
                    return Json(new { isok = false, msg = "未授权！" }, JsonRequestBehavior.AllowGet);
                }
                if (app.AppId.IsNullOrWhiteSpace())
                {
                    return Json(new { isok = false, msg = "未授权！" }, JsonRequestBehavior.AllowGet);
                }
                Food miAppfood = FoodBLL.SingleModel.GetModel($"appId={appId}");
                if (miAppfood == null)
                {
                    return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
                }
                TemplateMsg miniapptemplatemsg = TemplateMsgBLL.SingleModel.GetModel(TempMsgId);
                if (miniapptemplatemsg == null)
                {
                    return Json(new { isok = false, msg = "系统内置的模板Id错误！" }, JsonRequestBehavior.AllowGet);
                }
                string msg = "";
                TemplateMsg_User newUserMsg = TemplateMsg_UserBLL.SingleModel.getModelByAppId(app.AppId, (int)TmpType.小程序餐饮模板, miniapptemplatemsg.Id);
                if (newUserMsg == null)
                {
                    //往微信appid加模板消息
                    
                    addResultModel _addResult = MsnModelHelper.addMsnToMy(app.AppId, miniapptemplatemsg.TitileId, miniapptemplatemsg.ColNums.Split(','), ref msg);

                    if (_addResult.errcode != 0)
                    {
                        return Json(new { isok = false, msg = _addResult.errmsg, newUserMsg = newUserMsg }, JsonRequestBehavior.AllowGet);
                    }

                    newUserMsg = new TemplateMsg_User();
                    newUserMsg.AppId = app.AppId;
                    newUserMsg.TmpId = miAppfood.Id;
                    newUserMsg.Ttypeid = miniapptemplatemsg.Ttypeid;
                    newUserMsg.TmId = miniapptemplatemsg.Id;
                    newUserMsg.ColNums = miniapptemplatemsg.ColNums;
                    newUserMsg.TitleId = miniapptemplatemsg.TitileId;
                    newUserMsg.State = 1;//启用
                    newUserMsg.CreateDate = DateTime.Now;
                    newUserMsg.TemplateId = _addResult.template_id;//微信公众号内的模板Id
                    newUserMsg.TmgType = miniapptemplatemsg.TmgType;

                    int result = Convert.ToInt32(TemplateMsg_UserBLL.SingleModel.Add(newUserMsg));
                    //var result = Convert.ToInt32(_miniapptemplatemsg_userBll.startTemplate(newUserMsg));

                    if (result <= 0)
                    {
                        return Json(new { isok = false, msg = "启用失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    newUserMsg.State = 1;
                    bool isSuccess = TemplateMsg_UserBLL.SingleModel.Update(newUserMsg, "State");

                    if (!isSuccess)
                    {
                        return Json(new { isok = false, msg = "启用失败" }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { isok = true, msg = "启用成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { isok = false, msg = "网络忙,请重试" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 停用模板
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult stopTmg(int appId, int TempMsgId)
        {
            try
            {
                if (dzaccount == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
                }
                XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
                if (app == null)
                {
                    return Json(new { isok = false, msg = "未授权！" }, JsonRequestBehavior.AllowGet);
                }
                if (app.AppId.IsNullOrWhiteSpace())
                {
                    return Json(new { isok = false, msg = "未授权！" }, JsonRequestBehavior.AllowGet);
                }
                Food miAppfood = FoodBLL.SingleModel.GetModel($"appId={appId}");
                if (miAppfood == null)
                {
                    return Json(new { isok = false, msg = "找不到该餐饮店铺！" }, JsonRequestBehavior.AllowGet);
                }
                TemplateMsg miniapptemplatemsg = TemplateMsgBLL.SingleModel.GetModel(TempMsgId);
                if (miniapptemplatemsg == null)
                {
                    return Json(new { isok = false, msg = "系统内置的模板Id错误！" }, JsonRequestBehavior.AllowGet);
                }
                //string msg = "";
                TemplateMsg_User newUserMsg = TemplateMsg_UserBLL.SingleModel.getModelByAppId(app.AppId, (int)TmpType.小程序餐饮模板, miniapptemplatemsg.Id);
                if (newUserMsg == null)
                {
                    return Json(new { isok = false, msg = "未找到模板" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    
                    newUserMsg.State = 0;//0为停用
                    bool isSuccess = TemplateMsg_UserBLL.SingleModel.Update(newUserMsg, "State");

                    //var isSuccess = _miniapptemplatemsg_userBll.stopTemplate(newUserMsg);
                    if (!isSuccess)
                    {
                        return Json(new { isok = false, msg = "停用失败" }, JsonRequestBehavior.AllowGet);
                    }

                    //var _deleteResult = _msnModelHelper.deleteMyMsn(app.AppId, newUserMsg.TemplateId, ref msg);
                    //if (_deleteResult.errcode != 0)
                    //{
                    //    return Json(new { isok = false, msg = _deleteResult.errmsg }, JsonRequestBehavior.AllowGet);
                    //}
                }

                return Json(new { isok = true, msg = "停用成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { isok = false, msg = "网络忙,请重试" }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion
        ///// <summary>
        ///// 请求打印
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <param name="orderid"></param>
        ///// <param name="state"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult printContent(int printId,int orderId)
        //{
        //    var print = _miniappfoodsprintsBll.GetModel(printId);
        //    var order = _miniappfoodgoodsorderBll.GetModel(orderId);

        //    //拼接订单内容排版
        //    var content = ""; 
        //    var returnMsg =  _yilianyunPrintHelper.printContent(print.APIKey, print.UserId, print.PrintNo, print.PrintKey, content);
        //    var returnModel = Utility.Serialize.SerializeHelper.DesFromJson<ylyReturnModel>(returnMsg);

        //    //更新打印机官网订单号至对应本地订单记录
        //    order.PrintId = returnModel.id;
        //    order.PrintSuccess = 0;
        //    _miniappfoodgoodsorderBll.Update(order, "PrintId");

        //    return Json(new { isok = true, msg = "成功！" }, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region 百度地图弹框

        [HttpPost]
        public ActionResult _PartailMapPoint(string lat, string lng, string address, string area)
        {
            // 根据坐标查位置
            if (!lat.IsNullOrWhiteSpace() && !lng.IsNullOrWhiteSpace() && lat != "0" && lng != "0")
            {
                ViewBag.Condition = new Dictionary<string, string> { { "Type", "1" }, { "Data", lng + ',' + lat } };
            }
            else if (!address.IsNullOrWhiteSpace())  // 地址
            {
                ViewBag.Condition = new Dictionary<string, string> { { "Type", "2" }, { "Data", address } };
                //new ResultObject { Type = 2, Data = address };
            }
            else if (!area.IsNullOrWhiteSpace())  //区域
            {
                string areaName = area;// C_AreaBLL.SingleModel.GetNameByAreaCode(area);
                ViewBag.Condition = new Dictionary<string, string> { { "Type", "3" }, { "Data", areaName } };
            }
            else  // IP
            {
                ViewBag.Condition = new Dictionary<string, string> { { "Type", "4" }, { "Data", "广州市" } };
            }
            return PartialView("_PartailMapPoint");
        }



        #region 分享配置

        public ActionResult ShareSet(int? appId)
        {
            if (appId == null || appId.Value <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId.Value, dzaccount.Id.ToString());
            if (role == null)
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            Food food = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (food == null)
                return View("PageError", new Return_Msg() { Msg = "店铺不存在!", code = "500" });
            FoodShare model = food.funJoinModel.shareConfig;
            if (model == null)
            {
                model = new FoodShare();
            }
            else
            {
                List<C_Attachment> logoAttachment = C_AttachmentBLL.SingleModel.GetListByCache(food.Id, (int)AttachmentItemType.小程序餐饮版分享店铺Logo);
                List<C_Attachment> adImgAttachment = C_AttachmentBLL.SingleModel.GetListByCache(food.Id, (int)AttachmentItemType.小程序餐饮版分享广告图);

                //店铺Logo
                List<object> LogoList = new List<object>();

                foreach (C_Attachment attachment in logoAttachment)
                {
                    LogoList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.LogoList = LogoList;

                //广告图
                List<object> ADImgList = new List<object>();
                foreach (C_Attachment attachment in adImgAttachment)
                {
                    ADImgList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.ADImgList = ADImgList;

                FoodConfigModel funJoinModel = food.funJoinModel;
                if (LogoList.Count <= 0)
                    funJoinModel.shareConfig.Logo.Clear();
                if (ADImgList.Count <= 0)
                    funJoinModel.shareConfig.ADImg.Clear();


                food.configJson = JsonConvert.SerializeObject(funJoinModel);

                FoodBLL.SingleModel.Update(food, "configJson");



            }



            ViewBag.appId = appId;

            return View(model);
        }


        [HttpPost]
        public ActionResult ShareSetting(FoodShare share, int? appId, string LogoList = "", string ADImgList = "")
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "登录信息过期！" });
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId.Value, dzaccount.Id.ToString());
            if (role == null)
            {
                return Json(new { isok = false, msg = "没有权限！" });
            }

            if (string.IsNullOrEmpty(share.StoreName) || share.StoreName.Length > 10)
                return Json(new { isok = false, msg = "店铺名称不能为空或者不能大于10个字符！" });

            if (!string.IsNullOrEmpty(share.ADTitle) && share.ADTitle.Length > 20)
                return Json(new { isok = false, msg = "广告语不能大于20个字符！" });

            Food food = FoodBLL.SingleModel.GetModel($"appId={appId.Value}");
            if (food == null)
                return Json(new { isok = false, msg = "店铺不存在！" });
            
            share.FoodId = food.Id;

            #region Logo

            FoodConfigModel funJoinModel = new FoodConfigModel();

            if (food.funJoinModel != null)
            {
                funJoinModel = food.funJoinModel;
            }

            if (funJoinModel.shareConfig == null)
            {
                funJoinModel.shareConfig = new FoodShare();
            }

            if (!string.IsNullOrWhiteSpace(LogoList))
            {
                string[] Imgs = LogoList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (Imgs != null && Imgs.Length > 0)
                {

                    foreach (string img in Imgs)
                    {
                        //判断上传图片是否以http开头，不然为破图
                        if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http") == 0)
                        {
                            C_Attachment c_att = new C_Attachment
                            {
                                itemId = share.FoodId,
                                createDate = DateTime.Now,
                                filepath = img,
                                itemType = (int)AttachmentItemType.小程序餐饮版分享店铺Logo,
                                thumbnail = img,
                                status = 0
                            };
                            int tid = Convert.ToInt32(C_AttachmentBLL.SingleModel.Add(c_att));
                            share.Logo.Add(new { id = tid, url = img });
                        }

                    }

                }
            }
            else
            {
                List<C_Attachment> adImgAttachment = C_AttachmentBLL.SingleModel.GetListByCache(food.Id, (int)AttachmentItemType.小程序餐饮版分享店铺Logo);

                funJoinModel.shareConfig.Logo.Clear();
                foreach (C_Attachment attachment in adImgAttachment)
                {
                    funJoinModel.shareConfig.Logo.Add(new { id = attachment.id, url = attachment.filepath });
                }
                share.Logo = funJoinModel.shareConfig.Logo;
            }

            #endregion

            #region 广告图

            if (!string.IsNullOrEmpty(ADImgList))
            {
                string[] imgArray = ADImgList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (imgArray != null && imgArray.Length > 0)
                {
                    share.ADImg = new List<string>();
                    foreach (string img in imgArray)
                    {
                        //判断上传图片是否以http开头，不然为破图
                        if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http://") == 0)
                        {
                            C_Attachment c_att = new C_Attachment
                            {
                                itemId = share.FoodId,
                                createDate = DateTime.Now,
                                filepath = img,
                                itemType = (int)AttachmentItemType.小程序餐饮版分享广告图,
                                thumbnail = img,
                                status = 0
                            };
                            C_AttachmentBLL.SingleModel.Add(c_att);
                            share.ADImg.Add(img);
                        }

                    }
                }
            }
            else
            {
                List<C_Attachment> adImgAttachment = C_AttachmentBLL.SingleModel.GetListByCache(food.Id, (int)AttachmentItemType.小程序餐饮版分享广告图);
                //广告图
                funJoinModel.shareConfig.ADImg.Clear();
                foreach (C_Attachment attachment in adImgAttachment)
                {
                    funJoinModel.shareConfig.ADImg.Add(attachment.filepath);

                }
                share.ADImg = funJoinModel.shareConfig.ADImg;

            }
            #endregion


            if (string.IsNullOrEmpty(funJoinModel.shareConfig.Qrcode) || !funJoinModel.shareConfig.Qrcode.Contains("."))
            {
                //  获取二维码
                XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(role.TId);
                if (xcxTemplate == null)
                {
                    return Json(new { isok = false, msg = "无效模板！" });
                }

                string token = "";
                if (!XcxApiBLL.SingleModel.GetToken(role, ref token))
                {
                    return Json(new { isok = false, msg = token });
                }

                qrcodeclass resultQcode = CommondHelper.GetMiniAppQrcode(token,"pages/index/index");
                if (resultQcode != null)
                {
                    if (resultQcode.isok > 0)
                    {
                        share.Qrcode = resultQcode.url;
                    }
                    else
                    {
                        share.Qrcode = resultQcode.msg;
                    }

                }
            }
            else
            {
                share.Qrcode = funJoinModel.shareConfig.Qrcode;
            }


            funJoinModel.shareConfig = share;
            food.configJson = JsonConvert.SerializeObject(funJoinModel);

            if (FoodBLL.SingleModel.Update(food, "configJson"))
            {
                return Json(new { isok = true, msg = "操作成功！", obj = share.FoodId });
            }
            else
            {
                return Json(new { isok = false, msg = "操作失败！", obj = share.FoodId });
            }


        }

        /// <summary>
        ///更新小程序码
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateShareQrcode(int? appId)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "登录信息过期！" });
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId.Value, dzaccount.Id.ToString());
            if (role == null)
            {
                return Json(new { isok = false, msg = "没有权限！" });
            }


            Food food = FoodBLL.SingleModel.GetModel($"appId={appId.Value}");
            if (food == null)
                return Json(new { isok = false, msg = "店铺不存在！" });


            FoodConfigModel funJoinModel = new FoodConfigModel();

            if (food.funJoinModel == null || food.funJoinModel.shareConfig == null)
            {
                return Json(new { isok = false, msg = "请先进行配置保存再获取" });
            }
            
            funJoinModel = food.funJoinModel;

            //获取二维码
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(role.TId);
            if (xcxTemplate == null)
            {
                return Json(new { isok = false, msg = "无效模板" });
            }

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(role, ref token))
            {
                return Json(new { isok = false, msg = token });
            }

            qrcodeclass resultQcode = CommondHelper.GetMiniAppQrcode(token,"pages/home/home");
            if (resultQcode != null)
            {
                if (resultQcode.isok > 0)
                {
                    funJoinModel.shareConfig.Qrcode = resultQcode.url;
                }
                else
                {
                    funJoinModel.shareConfig.Qrcode = resultQcode.msg;
                }
            }

            food.configJson = JsonConvert.SerializeObject(funJoinModel);

            if (!FoodBLL.SingleModel.Update(food, "configJson"))
            {
                return Json(new { isok = false, msg = "操作失败！", obj = food.Id });
            }

            return Json(new { isok = true, msg = "操作成功！", obj = food.Id });

        }

        #endregion

        #endregion

        #region 会员管理
        public ActionResult VipSetting(int appId = 0)
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
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            ViewBag.appId = appId;

            Food miAppfood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppfood == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有找到餐饮店铺!", code = "500" });

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
                //初始化默认会员等级
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
                    //获取部分打折的商品
                    if (info.type == 2 && !string.IsNullOrEmpty(info.gids))
                    {
                        info.foodgoodslist = FoodGoodsBLL.SingleModel.GetList($"id in ({info.gids}) and IsSell=1 and state=1");
                    }
                }
            }
            VipLevel dflevel = model.levelList.Where(l => l.level == 0).FirstOrDefault();
            model.ruleList = VipRuleBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            if (model.ruleList == null || model.ruleList.Count <= 0)
            {
                //初始化自动升级规则
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

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcx, ref token))
            {
                return View("PageError", new Return_Msg() { Msg = token, code = "500" });
            }

            int isAuth = 0;
            int isAuthCard = 0;
            int haveCard = 0;
            string SerName = string.Empty;
            string cardStaus = string.Empty;

            VipWxCard card = VipWxCardBLL.SingleModel.GetModel($"Type=1 and AppId={miAppfood.Id}");
            if (card != null)
            {
                
                isAuth = 1;
                isAuthCard = 1;
                haveCard = 1;
                string cardResult = Context.PostData($"https://api.weixin.qq.com/card/get?access_token={token}", JsonConvert.SerializeObject(new { card_id = card.CardId }));
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
                }
            }

            string returnurl = AsUrlData($"{WebSiteConfig.XcxAppReturnUrl}/foods/VipSetting?appId={appId}");
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
            ViewBag.token = token;
            //string ak = WxHelper.GetToken("wx64f161aa79a6801b", "65d0158981a05224eab3d690c36f035e", false);
            //if (string.IsNullOrEmpty(ak))
            //{
            //    ak = WxHelper.GetToken("wx64f161aa79a6801b", "65d0158981a05224eab3d690c36f035e", true);
            //}
            ViewBag.ak = "ak";

            return View(model);
        }

        private string AsUrlData(string data)
        {
            return Uri.EscapeDataString(data);
        }

        /// <summary>
        /// 添加编辑会员级别
        /// </summary>
        public ActionResult SavelevelInfo()
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
                model.type = type;
                model.updatetime = DateTime.Now;
                model.bgcolor = bgcolor;
                if (type != 0)
                {
                    model.discount = discount;
                }
                if (type == 2 && !string.IsNullOrEmpty(gids))
                {
                    model.foodgoodslist = FoodGoodsBLL.SingleModel.GetList($"id in ({gids})  and IsSell=1 and state=1");
                }
                bool isok = VipLevelBLL.SingleModel.Update(model, "name,gids,type,bgcolor,updatetime,discount");
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
                model.level = 1;
                model.appId = xcx.AppId;
                model.bgcolor = bgcolor;
                if (type != 0)
                {
                    model.discount = discount;
                }
                if (type == 2 && !string.IsNullOrEmpty(gids))
                {
                    model.foodgoodslist = FoodGoodsBLL.SingleModel.GetList($"id in ({gids})  and IsSell=1 and state=1");
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
        public ActionResult validviplist(int id = 0)
        {
            if (id <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            int count = VipRelationBLL.SingleModel.GetCount($"levelid={id} and state>=0");
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
            int autoswitch = Context.GetRequestInt("switch", -1);
            if (appId <= 0 || autoswitch < 0)
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
            VipConfig config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (config == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }
            config.autoupdate = autoswitch;
            config.updatetime = DateTime.Now;
            bool isok = VipConfigBLL.SingleModel.Update(config, "autoupdate,updatetime");
            string msg = config.autoswitch ? "自动升级已开启" : "自动升级已关闭";
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
        public ActionResult SaveSyncCard()
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

            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
            }

            VipConfig config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (config == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={miAppFood.Id}");
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
            
            OpenAuthorizerConfig app = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), appId).FirstOrDefault();
            if (app == null)
            {
                return Json(new { isok = false, msg = "请先到小程序管理绑定小程序" }, JsonRequestBehavior.AllowGet);
            }

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcx, ref token))
            {
                return Json(new { isok = false, msg = token }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(miAppFood.Logo) || string.IsNullOrEmpty(miAppFood.FoodsName))
            {
                return Json(new { isok = false, msg = "操作失败(请先配置店铺信息Logo以及店铺名称)" }, JsonRequestBehavior.AllowGet);
            }
            string uploadImgResult = CommondHelper.WxUploadImg(token, miAppFood.Logo);
            if (!uploadImgResult.Contains("url"))
            {
                return Json(new { isok = false, msg = "操作失败", obj = uploadImgResult }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string name = miAppFood.FoodsName;
                if (name.Length > 12)
                {
                    name = name.Substring(0, 12);
                }
                string carTitle = name;
                if (carTitle.Length > 6)
                {
                    carTitle = carTitle.Substring(0, 6);
                }

                WxUploadImgResult r = JsonConvert.DeserializeObject<WxUploadImgResult>(uploadImgResult);
                CreateCardResult _createCardResult = CommondHelper.AddVipWxCard(r.url, name, carTitle + "会员卡", app.user_name, token);

                if (_createCardResult.errcode != 0)
                {
                    //log4net.LogHelper.WriteInfo(this.GetType(), $"errorCode:{_createCardResult.errcode},errmsg:{_createCardResult.errmsg}");
                    return Json(new { isok = false, msg = "操作失败", obj = _createCardResult }, JsonRequestBehavior.AllowGet);
                }

                _vipWxCard.CardId = _createCardResult.card_id;
                _vipWxCard.AppId = miAppFood.Id;
                _vipWxCard.Type = 1;//表示餐饮
                _vipWxCard.User_Name = umodel.user_name;
                _vipWxCard.SerName = umodel.nick_name;
                _vipWxCard.Status = (int)WxVipCardStatus.CARD_STATUS_NOT_VERIFY;
                _vipWxCard.AddTime = DateTime.Now;
                int VipWxCardId = Convert.ToInt32(VipWxCardBLL.SingleModel.Add(_vipWxCard));

                return Json(new { isok = true, msg = VipWxCardId > 0 ? "操作成功" : "操作失败", CreateCardResult = _createCardResult, VipWxCardId = VipWxCardId, authorizer_access_token = token }, JsonRequestBehavior.AllowGet);

            }
        }


        /// <summary>
        /// 更新微信会员卡
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateWxCard()
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

            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
            }

            VipConfig config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (config == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(miAppFood.Logo) || string.IsNullOrEmpty(miAppFood.FoodsName))
            {
                return Json(new { isok = false, msg = "操作失败(请先配置店铺信息Logo以及店铺名称)" }, JsonRequestBehavior.AllowGet);
            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={miAppFood.Id}");
            if (_vipWxCard == null)
                return Json(new { isok = false, msg = "非法操作(请先创建卡套)" }, JsonRequestBehavior.AllowGet);

            string carTitle = miAppFood.FoodsName;
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
                        logo_url = miAppFood.Logo
                    }
                }

            };
            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcx, ref token))
            {
                return Json(new { isok = false, msg = token }, JsonRequestBehavior.AllowGet);
            }
            string updateCardJson = JsonConvert.SerializeObject(updateCard);
            string updateResult = Context.PostData($"https://api.weixin.qq.com/card/update?access_token={token}", updateCardJson);

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
                ruleList = VipRuleBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            }
            return Json(new { isok = isok, msg = msg, ruleList = ruleList }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 会员列表
        /// </summary>
        /// <returns></returns>
        public ActionResult VipList()
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
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
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

        public ActionResult saveEdit()
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
            int levelid = Context.GetRequestInt("levelid", 0);
            if (levelid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙levelid_null" }, JsonRequestBehavior.AllowGet);
            }
            int vid = Context.GetRequestInt("vid", 0);
            if (vid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙vid_null" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={levelid} and appid='{xcx.AppId}' and state>=0");
            VipRelation viprelation = VipRelationBLL.SingleModel.GetModel($"id={vid} and appid='{xcx.AppId}' and state>=0");
            if (levelinfo == null || viprelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙data_null" }, JsonRequestBehavior.AllowGet);
            }
            viprelation.levelid = levelinfo.Id;
            viprelation.updatetime = DateTime.Now;
            bool isok = VipRelationBLL.SingleModel.Update(viprelation, "levelid,updatetime");
            string msg = isok ? "修改成功" : "修改失败";
            return Json(new { isok = isok, msg = msg, levelinfo = levelinfo }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 部分折扣时选择餐品列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodsList()
        {
            int appid = Context.GetRequestInt("appid", 0);
            if (appid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" }, JsonRequestBehavior.AllowGet);
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
            Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={appid}");
            if (miAppFood == null)
            {
                return Json(new { isok = false, msg = "请先配置餐饮店铺信息！" }, JsonRequestBehavior.AllowGet);
            }
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            List<FoodGoods> goodlist = FoodGoodsBLL.SingleModel.getFindPageList(miAppFood.Id, string.Empty, null, null, pageIndex, pageSize);// _miniappfoodgoodsBll.GetList($" foodid = {miAppFood.Id} and state >= 0 ", pageSize, pageIndex, "*", "id desc");
            int count = FoodGoodsBLL.SingleModel.getFindPageListCount(miAppFood.Id, string.Empty, null, null);//_miniappfoodgoodsBll.GetCount($" foodid = {miAppFood.Id} and state >= 0 ");
            return Json(new { isok = true, goodlist = goodlist, count = count }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        
        #region 储值支付
        /// <summary>
        /// 支付方式更改
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult updateOrderBuyMode(string appid, string openid, int orderid, int buyMode)
        {
            if (!Enum.IsDefined(typeof(OrderState), buyMode))
            {
                return Json(new { isok = -1, msg = "支付方式有误" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food store = FoodBLL.SingleModel.GetModel($" appid = {umodel.Id} ");
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            FoodGoodsOrder goodOrder = FoodGoodsOrderBLL.SingleModel.GetModel($" Id = {orderid} and StoreId = {store.Id} and UserId = {userInfo.Id} and State = {(int)miniAppFoodOrderState.待付款} ");
            if (goodOrder == null)
            {
                return Json(new { isok = -1, msg = "订单信息异常,请刷新重试" }, JsonRequestBehavior.AllowGet);
            }
            goodOrder.BuyMode = buyMode;
            bool success = FoodGoodsOrderBLL.SingleModel.Update(goodOrder, "buyMode");
            return Json(new { isok = success ? 1 : -1, msg = "修改成功！" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        
        public ActionResult SetUserInfoTelePhone()
        {
            int userInfoId = Context.GetRequestInt("userInfoId", 0);
            string encryptedData = Context.GetRequest("encryptedData", "");
            string iv = Context.GetRequest("iv", "");
            string session_key = Context.GetRequest("session_key", "");

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

        public ActionResult SendMsg()
        {
            string a = "";
            EntGoodsOrder e = EntGoodsOrderBLL.SingleModel.GetModel(1867);
            TemplateMsg_Gzh.SendReserveTimeOutTemplateMessage(e);

            return Json(new { isok = 1, msg = true ? "成功" + a : "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 批量保存菜品排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public ActionResult SaveSort(List<FoodGoods> list)
        {
            if (list == null || list.Count <= 0)
            {
                return Json(new { isok = false, msg = "数据错误" });
            }
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" });
            }

            Food store = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (store == null)
            {
                return Json(new { isok = false, msg = "门店不存在" });
            }

            bool isok = FoodGoodsBLL.SingleModel.UpdateListSort(list, store.Id);
            string msg = isok ? "保存成功" : "保存失败";

            return Json(new { isok = isok, msg = msg });
        }
        /// <summary>
        /// 批量保存菜品分类排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public ActionResult SaveGoodsTypeSort(List<FoodGoodsType> list)
        {
            if (list == null || list.Count <= 0)
            {
                return Json(new { isok = false, msg = "数据错误" });
            }
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" });
            }

            Food store = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (store == null)
            {
                return Json(new { isok = false, msg = "门店不存在" });
            }

            bool isok = FoodGoodsTypeBLL.SingleModel.UpdateListSort(list, store.Id);
            string msg = isok ? "保存成功" : "保存失败";

            return Json(new { isok = isok, msg = msg });
        }

        /// <summary>
        /// 批量保存菜品标签排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public ActionResult SaveLabelSort(List<FoodLabel> list)
        {
            if (list == null || list.Count <= 0)
            {
                return Json(new { isok = false, msg = "数据错误" });
            }
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" });
            }

            Food store = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (store == null)
            {
                return Json(new { isok = false, msg = "门店不存在" });
            }

            bool isok = FoodLabelBLL.SingleModel.UpdateListSort(list, store.Id);
            string msg = isok ? "保存成功" : "保存失败";

            return Json(new { isok = isok, msg = msg });
        }
        public ActionResult SaveAttrSpecSort(List<FoodGoodsAttr> list)
        {
            if (list == null || list.Count <= 0)
            {
                return Json(new { isok = false, msg = "数据错误" });
            }
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" });
            }

            Food store = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (store == null)
            {
                return Json(new { isok = false, msg = "门店不存在" });
            }

            bool isok = FoodGoodsAttrBLL.SingleModel.UpdateListSort(list, store.Id);
            string msg = isok ? "保存成功" : "保存失败";

            return Json(new { isok = isok, msg = msg });
        }
        
        [LoginFilter]
        public ActionResult Reservation(int appId = 0)
        {
            int storeId = Context.GetRequestInt("storeId", 0);
            int pageType = Context.GetRequestInt("PageType", 0);

            FoodConfigModel foodSetting = new FoodConfigModel();
            Food store_Food = FoodBLL.SingleModel.GetModelByAppId(appId, storeId);
            if (store_Food == null)
            {
                return Redirect("/base/PageError?type=3");
            }
            foodSetting = store_Food.funJoinModel;

            ViewBag.appId = appId;
            ViewBag.storeId = storeId;
            ViewBag.PageType = pageType;
            return View(foodSetting);
        }

        [HttpGet]
        public JsonResult GetReserveList(int appId = 0, int pageIndex = 1, int pageSize = 10, int state = (int)miniAppFoodOrderState.待接单, string userName = null, string contact = null, DateTime? start = null, DateTime? end = null, DateTime? dinnerStart = null, DateTime? dinnerEnd = null)
        {
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" }, JsonRequestBehavior.AllowGet);
            }

            Food store = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (store == null)
            {
                return Json(new { isok = false, msg = "门店不存在" }, JsonRequestBehavior.AllowGet);
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                userName = StringHelper.ReplaceSQLKey(userName).Trim();
            }

            if (!string.IsNullOrWhiteSpace(contact))
            {
                contact = StringHelper.ReplaceSQLKey(contact).Trim();
            }

            
            List<FoodReservation> reserveList = FoodReservationBLL.SingleModel.GetByList(pageIndex: pageIndex, pageSize: pageSize, foodId: store.Id, state: state, userName: userName, contact: contact, start: start, end: end, dinnerStart: dinnerStart, dinnerEnd: dinnerEnd);
            List<object> result = FoodReservationBLL.SingleModel.ConvertToAPIModel(reserveList);
            int count = pageIndex == 1 ? FoodReservationBLL.SingleModel.GetListCount(foodId: store.Id, state: state, userName: userName, contact: contact, start: start, end: end, dinnerStart: dinnerStart, dinnerEnd: dinnerEnd) : 0;
            return Json(new { isok = true, data = result, count = count }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateReservation(int appId = 0, int reserveId = 0, int updateState = 0, int tableId = 0)
        {
            if (appId <= 0 || reserveId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" });
            }

            Food store = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (store == null)
            {
                return Json(new { isok = false, msg = "门店不存在" });
            }

            
            FoodReservation reservation = FoodReservationBLL.SingleModel.GetModel(reserveId);
            if (reservation.FoodId != store.Id)
            {
                return Json(new { isok = false, msg = "非法请求" });
            }

            bool result = FoodReservationBLL.SingleModel.UpdateState(reservation, updateState, tableId);

            return Json(result ? new { isok = true, msg = "操作成功" } : new { isok = false, msg = "操作失败" });
        }

        [HttpGet]
        public JsonResult GetTableList(int appId = 0)
        {
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" }, JsonRequestBehavior.AllowGet);
            }

            Food store = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (store == null)
            {
                return Json(new { isok = false, msg = "门店不存在" }, JsonRequestBehavior.AllowGet);
            }

            List<FoodTable> tableList = FoodTableBLL.SingleModel.GetTablesByStore(store.Id);

            return Json(new { isok = true, data = tableList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateStoreConfig(int appId = 0, string option = null, string value = null)
        {
            if (appId <= 0 || string.IsNullOrWhiteSpace(option) || string.IsNullOrWhiteSpace(value))
            {
                return Json(new { isok = false, msg = "参数错误!" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" });
            }

            Food store = FoodBLL.SingleModel.GetModel($"appId={appId}");
            if (store == null)
            {
                return Json(new { isok = false, msg = "门店不存在" });
            }

            bool result = FoodBLL.SingleModel.UpdateConfigJson(store: store, option: option, value: value);

            return Json(result ? new { isok = true, msg = "保存成功" } : new { isok = false, msg = $"保存失败:{option}-{value}" });
        }


        public ActionResult SaveTakeoutSetting(int id = 0, int appid = 0, int DeliveryRange = 0, int TakeOut = 0, int OutSide = 0, int DistributionWay = 0, int ShippingFee = 0, int PackinFee = 0, string StartShopTime = "8:00", string EndShopTime = "20:00",string teamtoken="",string kpzphone="")
        {
            result = new Return_Msg();
            if (id <= 0 || appid <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }
            if (dzaccount == null)
            {
                result.Msg = "系统繁忙auth_null !";
                return Json(result);
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appid, dzaccount.Id.ToString());
            if (umodel == null)
            {
                result.Msg = "系统繁忙umodel_null";
                return Json(result);
            }
            Food store = FoodBLL.SingleModel.GetModel(id);
            if (store == null)
            {
                result.Msg = "找不到门店";
                return Json(result);
            }

            if (DistributionWay == (int)miniAppOrderGetWay.快跑者配送)
            {
                if (string.IsNullOrEmpty(teamtoken.Trim()))
                {
                    result.Msg = "请输入团队token !";
                    return Json(result);
                }
                if(string.IsNullOrEmpty(kpzphone))
                {
                    result.Msg = "请输入手机号码!";
                    return Json(result);
                }
                //快跑者配送配置
                KPZStoreRelationBLL.SingleModel.AddStore(store.appId,0,teamtoken.Trim(), kpzphone.Trim());
            }
            store.DeliveryRange = DeliveryRange;
            store.TakeOut = TakeOut;
            store.OutSide = OutSide;
            store.DistributionWay = DistributionWay;
            store.ShippingFee = ShippingFee;
            store.PackinFee = PackinFee;
            List<FoodOpenTimeModel> openTimeList = store.getOpenTimeList;
            openTimeList[1].StartTime = StartShopTime;
            openTimeList[1].EndTime = EndShopTime;
            store.OpenTimeJson = JsonConvert.SerializeObject(openTimeList);
            result.isok = FoodBLL.SingleModel.Update(store, "DeliveryRange,TakeOut,OutSide,DistributionWay,ShippingFee,PackinFee,OpenTimeJson");
            result.Msg = result.isok ? "保存成功" : "保存失败";
            return Json(result);
        }


        /// <summary>
        /// UU跑腿配置
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveUUConfig(UUCustomerRelation model = null)
        {
            result = new Return_Msg();
            if (model == null)
            {
                result.Msg = "参数出错";
                return Json(result);
            }
            if (string.IsNullOrEmpty(model.Phone))
            {
                result.Msg = "请输入商户手机号";
                return Json(result);
            }
            if (string.IsNullOrEmpty(model.PhoneCode))
            {
                result.Msg = "请输入验证码";
                return Json(result);
            }
            UUBaseResult resultUU = UUApi.BindUserSubmit(model.Phone, model.PhoneCode, model.CityName, model.CountyName);
            if (resultUU != null)
            {
                if(resultUU.return_code == "ok" && !string.IsNullOrEmpty(resultUU.openid))
                {
                    model.OpenId = resultUU.openid;
                    model.UpdateTime = DateTime.Now;
                    if (model.Id <= 0)
                    {
                        model.State = 0;
                        model.AddTime = DateTime.Now;
                        UUCustomerRelationBLL.SingleModel.Add(model);
                    }
                    else
                    {
                        UUCustomerRelationBLL.SingleModel.Update(model);
                    }
                    result.Msg = "注册成功";
                    result.isok = true;
                    return Json(result);
                }
                else
                {
                    result.Msg = resultUU.return_msg;
                }
            }
            else
            {
                result.Msg = "注册失败";
            }

            return Json(result);
        }

        /// <summary>
        /// UU跑腿发送验证码
        /// </summary>
        /// <param name="aId"></param>
        /// <returns></returns>
        public ActionResult UUSendPhoneCode(int aId = 0, string phone = "", int storeId = 0)
        {
            result = new Return_Msg();
            //参数验证
            if (aId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                result.Msg = "请输入手机号码";
                return Json(result);
            }

            UUBaseResult uuResult = UUApi.BindUserApply(phone);
            if (uuResult != null)
            {
                result.Msg = uuResult.return_msg;
                result.isok = uuResult.return_code == "ok";
            }
            else
            {
                result.Msg = "uu接口异常";
            }

            return Json(result);
        }


        /// <summary>
        /// 解绑UU跑腿
        /// </summary>
        /// <param name="aId"></param>
        /// <returns></returns>
        public ActionResult UUCancelBind(int aId = 0, int storeId = 0, int id = 0)
        {
            result = new Return_Msg();
            //参数验证
            if (aId <= 0 || id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }

            UUCustomerRelation model = UUCustomerRelationBLL.SingleModel.GetModel(id);
            if (model == null)
            {
                result.Msg = "请刷新重试";
                return Json(result);
            }
            if (model.AId != aId || model.StoreId != storeId)
            {
                result.Msg = "无效权限";
                return Json(result);
            }

            UUBaseResult uuResult = UUApi.CancelBind(model.OpenId);
            if (uuResult != null)
            {
                result.Msg = uuResult.return_msg;
                if (uuResult.return_code == "ok")
                {
                    model.State = -1;
                    UUCustomerRelationBLL.SingleModel.Update(model, "state");
                    result.isok = true;
                }
            }
            else
            {
                result.Msg = "uu接口异常";
            }

            return Json(result);
        }

        public ActionResult SaveScanCodeInfo(int id = 0, int appid = 0, int TheShop = 0, int UnderlinePay = 0, string StartShopTime = "8:00", string EndShopTime = "20:00")
        {
            result = new Return_Msg();
            if (id <= 0 || appid <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }
            if (dzaccount == null)
            {
                result.Msg = "系统繁忙auth_null !";
                return Json(result);
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appid, dzaccount.Id.ToString());
            if (umodel == null)
            {
                result.Msg = "系统繁忙umodel_null";
                return Json(result);
            }
            Food store = FoodBLL.SingleModel.GetModel(id);
            if (store == null)
            {
                result.Msg = "找不到门店";
                return Json(result);
            }
            store.TheShop = TheShop;
            store.underlinePay = UnderlinePay;
            List<FoodOpenTimeModel> openTimeList = store.getOpenTimeList;
            openTimeList[2].StartTime = StartShopTime;
            openTimeList[2].EndTime = EndShopTime;
            store.OpenTimeJson = JsonConvert.SerializeObject(openTimeList);
            result.isok = FoodBLL.SingleModel.Update(store, "TheShop,underlinePay,OpenTimeJson");
            result.Msg = result.isok ? "保存成功" : "保存失败";
            return Json(result);
        }

    }
}
