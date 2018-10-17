using Api.MiniApp.Filters;
using Api.MiniApp.Models;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using Entity.MiniApp.Weixin;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Utility;
using Utility.AliOss;

namespace Api.MiniApp.Controllers
{
    public class apiStoresController : InheritController
    {
    }
        [ExceptionLog]
    public class apiMiappStoresController : apiStoresController
    {
        private readonly int _maxMoney = 999999999;
        private readonly static object _couponLock = new object();
        public static readonly Dictionary<int, object> lockObjectDict_Order = new Dictionary<int, object>();

        public apiMiappStoresController()
        {
            
        }

        #region 登陆状态授权保存
        /// <summary>
        /// 用户资料(获取)
        /// </summary>
        /// <param name="unionid">UnionID</param>
        /// <returns>用户Json数据</returns>
        [ApiOAuthParameter]
        public ActionResult GetUserInfo(string unionid)
        {
            C_UserInfo loginUser = C_UserInfoBLL.SingleModel.GetModelFromCacheByUnionid(unionid);

            if (loginUser == null)
            {
                return Json(new BaseResult() { result = false, msg = "登陆异常", errcode = -1 }, JsonRequestBehavior.AllowGet);
            }

            return Json(new BaseResult() { result = true, obj = loginUser }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 用户资料(获取)
        /// </summary>
        /// <param name="unionid">UnionID</param>
        /// <returns>用户Json数据</returns>
        [ApiOAuthParameter]
        public ActionResult GetUserInfoByOpenId(string openId)
        {
            C_UserInfo loginUser = C_UserInfoBLL.SingleModel.GetModelFromCache(openId);

            if (loginUser == null)
            {
                return Json(new BaseResult() { result = false, msg = "登陆异常", errcode = -1 }, JsonRequestBehavior.AllowGet);
            }

            return Json(new BaseResult() { result = true, obj = loginUser }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 用户登录/注册
        /// </summary>
        /// <param name="code">微信授权Code</param>
        /// <param name="iv">初始向量</param>
        /// <param name="data">加密数据</param>
        /// <param name="signature">加密签名</param>
        /// <returns>微信用户数据(Json)</returns>
        public ActionResult CheckUserLogin(string code, string iv, string data, string signature, string appid, string appsr = "")
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(iv) || string.IsNullOrWhiteSpace(data) || string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(appid))
            {
                return Json(new { result = false, msg = "参数缺省", errcode = 0 }, JsonRequestBehavior.AllowGet);
            }

            //微信授权Code，调用接口获得session_key
            string JsonResult = new DecryptUserInfo().GetApiJsonStringnew(code, appid, appsr);
            UserSession session = JsonConvert.DeserializeObject<UserSession>(JsonResult);
            session.code = code;
            session.vector = iv;
            session.enData = data;
            session.signature = signature;
            if (!session.verify())
            {
                return CheckUserLoginNoappsr(code, iv, data, signature, appid);
                // return Json(new { result = false, msg = "获取Session_key异常，appsr=" + appsr, errcode = -1, Oject = UserSession }, JsonRequestBehavior.AllowGet);
            }
            //AES解密，委托参数session_key和初始向量
            session.deData = AESDecrypt.Decrypt(session.enData, session.session_key, session.vector);
            C_ApiUserInfo userInfo = JsonConvert.DeserializeObject<C_ApiUserInfo>(session.deData);
            //保存用户会话
            //var SessionId = AESDecrypt.MD5(UserSession.session_key + UserInfo.unionId);
            //返回sessionId
            C_UserInfo userinfopost = C_UserInfoBLL.SingleModel.GetModelFromCache(userInfo.openId);
            if (userinfopost == null)
            {
                userinfopost = C_UserInfoBLL.SingleModel.RegisterByXiaoChenXun(new C_UserInfo() { NickName = userInfo.nickName, HeadImgUrl = userInfo.avatarUrl, UnionId = userInfo.unionId, appId = appid, OpenId = userInfo.openId, Sex = int.Parse(userInfo.gender) });
            }
            userInfo.nickName = userinfopost.NickName;
            userInfo.avatarUrl = userinfopost.HeadImgUrl;
            userInfo.gender = userinfopost.Sex.ToString();
            userInfo.tel = userinfopost.TelePhone;
            userInfo.IsValidTelePhone = userinfopost.IsValidTelePhone;
            return Json(new BaseResult() { result = true, msg = "解密完成", obj = userInfo }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CheckUserState(string sessionid)
        {
            object SessionSav = Session[sessionid];
            BaseResult res = new BaseResult();
            res.result = SessionSav != null ? true : false;
            res.msg = SessionSav != null ? "已登陆" : "未登陆"; ;
            res.errcode = SessionSav != null ? 0 : -1;
            res.obj = SessionSav;
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 图片上传、删除
        [HttpPost]
        [ExceptionLog]
        public ActionResult uploadImageFromPost(int index = 0, bool isSave = false)
        {
            if (Request.Files.Count == 0) { return Json(new BaseResult() { result = false, msg = "请选择一张图片" }, JsonRequestBehavior.AllowGet); }
            using (Stream stream = Request.Files[0].InputStream)
            {
                string outext = "jpg";
                byte[] imgByteArray = new byte[stream.Length];
                stream.Read(imgByteArray, 0, imgByteArray.Length);
                // 设置当前流的位置为流的开始
                stream.Seek(0, SeekOrigin.Begin);
                //开始上传图片
                string aliTempImgKey = string.Empty;
                string aliTempImgFolder = AliOSSHelper.GetOssImgKey(outext, false, out aliTempImgKey);
                bool putResult = AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, imgByteArray, 1, "." + outext);
                if (!putResult)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "图片上传失败！图片同步到Ali失败！mediaId");
                    return Json(new { result = false, msg = "图片上传失败！图片同步到Ali失败！" }, JsonRequestBehavior.AllowGet);
                }
                //添加到附件
                //var pic = new Bitmap(stream);
                //var imgWidth = picWith > 0 ? picWith : pic.Width;//获取图片宽度 
                //var imgHeigth = picHeigth > 0 ? picHeigth : pic.Height; //获取图片高度
                //var imgSize = 0;
                //切割缩略图
                //var thumpath = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(aliTempImgKey, 200, 200);
                string thumpath = aliTempImgKey;
                //aliTempImgKey = aliTempImgKey.Replace("oss.", "img.") + $"@1e_1c_0o_0l_100sh_{imgHeigth}h_{imgWidth}w_90q.src";
                Attachment model = new Attachment()
                {
                    postfix = "." + outext,
                    filepath = aliTempImgKey,
                    thumbnail = thumpath
                    //fid = minisnsId
                    //imgsize = imgSize
                    //userid = usermodel.Id
                };
                int imgId = 0;
                if (isSave)
                {
                    isSave = int.TryParse(AttachmentBll.SingleModel.Add(model).ToString(), out imgId);
                }
                return Json(new { mediaId = imgId, path = aliTempImgKey, thumpath = thumpath, isSuccessSave = isSave, index = index }, JsonRequestBehavior.AllowGet);
            }
        }
        [ExceptionLog]
        public JsonResult DeleteStoreImage(int imageId, string openId)
        {
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModelFromCache(openId);
            if (loginCUser == null)
            {
                return Json(new BaseResult { errcode = 1, result = false, msg = "登录信息过期，刷新试试" }, JsonRequestBehavior.AllowGet);
            }
            C_Attachment catt = C_AttachmentBLL.SingleModel.GetModel(imageId);
            if (catt == null)
            {
                return Json(new BaseResult { errcode = 1, result = false, msg = "该图片已不存在" }, JsonRequestBehavior.AllowGet);
            }
            //bool auth = false;
            //权限验证
            //switch (catt.itemType)
            //{
            //    case (int)C_Enums.AttachmentItemType.信息附件:
            //        C_Post cpost = new C_PostBLL().GetModel(catt.itemId);
            //        if (cpost == null)
            //        {
            //            auth = true;
            //        }
            //        else
            //        {
            //            auth = cpost.OpenId == loginCUser.OpenId;
            //        }
            //        break;
            //    case (int)C_Enums.AttachmentItemType.公司介绍图:
            //        C_Company ccy = new C_CompanyBLL().GetModel(catt.itemId);
            //        if (ccy == null)
            //        {
            //            auth = true;
            //        }
            //        else
            //        {
            //            auth = ccy.OpenId == loginCUser.OpenId;
            //        }
            //        break;
            //    case (int)C_Enums.AttachmentItemType.商户轮播图:
            //    case (int)C_Enums.AttachmentItemType.商户介绍图:
            //    case (int)C_Enums.AttachmentItemType.商户介绍语音:
            //        C_Store cstore = new C_StoreBLL().GetModel(catt.itemId);
            //        if (cstore == null)
            //        {
            //            auth = true;
            //        }
            //        else
            //        {
            //            auth = cstore.OpenId == loginCUser.OpenId;
            //        }

            //        break;
            //    case (int)C_Enums.AttachmentItemType.五折卡服务图片:
            //        C_HalfOffServices chalfs = new C_HalfOffServicesBLL().GetModel(catt.itemId);
            //        if (chalfs == null)
            //        {
            //            auth = true;
            //        }
            //        else
            //        {
            //            auth = chalfs.OpenId == loginCUser.OpenId;
            //        }
            //        break;
            //    case (int)C_Enums.AttachmentItemType.五折卡服务详情图片:
            //        C_HalfOffServices chalfs2 = new C_HalfOffServicesBLL().GetModel(catt.itemId);
            //        if (chalfs2 == null)
            //        {
            //            auth = true;
            //        }
            //        else
            //        {
            //            auth = chalfs2.OpenId == loginCUser.OpenId;
            //        }
            //        break;
            //    case (int)C_Enums.AttachmentItemType.活动图片:
            //        C_Active active = new C_ActiveBLL().GetModel(catt.itemId);
            //        if (active == null)
            //        {
            //            auth = true;
            //        }
            //        else
            //        {
            //            auth = active.Openid == loginCUser.OpenId;
            //        }
            //        break;
            //    case (int)C_Enums.AttachmentItemType.优惠券轮播图:
            //    case (int)C_Enums.AttachmentItemType.优惠券详情图:
            //        C_StoreCoupon coupon = new C_StoreCouponBLL().GetModel(catt.itemId);
            //        if (coupon == null)
            //        {
            //            auth = true;
            //        }
            //        else
            //        {
            //            C_Store cctore = new C_StoreBLL().GetModel(coupon.StoreId);
            //            if (cctore == null)
            //            {
            //                auth = true;
            //            }
            //            else
            //            {
            //                auth = cctore.OpenId == loginCUser.OpenId;
            //            }
            //        }
            //        break;
            //    default:
            //        return Json(new BaseResult { errcode = 1, result = false, msg = "系统繁忙c_param_err" }, JsonRequestBehavior.AllowGet);
            //}
            //if (!auth)
            //{
            //    return Json(new BaseResult { errcode = 1, result = false, msg = "系统繁忙auth_limited" }, JsonRequestBehavior.AllowGet);
            //}
            if (C_AttachmentBLL.SingleModel.Delete(catt.id) > 0)
            {
                C_AttachmentBLL.SingleModel.RemoveRedis(catt.itemId, catt.itemType);//清除缓存
                return Json(new BaseResult { errcode = 1, result = true, msg = "删除成功" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new BaseResult { errcode = 1, result = false, msg = "系统繁忙db_err" }, JsonRequestBehavior.AllowGet);
        }
        #endregion



        #region 小程序商城信息

        public ActionResult GetImg(string appid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(appid))
                {
                    return Json(new { data = "", isok = 0, msg = "模块appId不能为空！" }, JsonRequestBehavior.AllowGet);
                }

                List<ConfParam> confParams = ConfParamBLL.SingleModel.GetModelByappid(appid);
                return Json(new { data = confParams, isok = 1 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "", isok = 0, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        ///  获取首页配置
        /// </summary>
        /// <param name="unionid"></param>
        /// <param name="storeid">店铺ID</param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetHomeConfig(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (app == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(app.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + app.AppId + ",id" + app.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }

            //轮播图链接集合
            List<C_Attachment> imglist = C_AttachmentBLL.SingleModel.GetListByCache(store.Id, (int)AttachmentItemType.小程序商城店铺轮播图);
            //List<string> dataimgs = imglist.Select(s => ImgHelper.ResizeImg(s.filepath,750 ,350)).ToList();
            List<string> dataimgs = imglist.Select(s => s.filepath).ToList();

            //商品分类
            List<StoreGoodsType> types = StoreGoodsTypeBLL.SingleModel.GetlistByStoreId(store.Id);
            types.ForEach(t =>
            {
                t.LogImg = ImgHelper.ResizeImg(t.LogImg, 90, 90);
            });

            object postdata = new
            {
                dataimgs = dataimgs,
                typelist = types
            };
            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取首页配置
        /// </summary>
        /// <param name="unionid"></param>
        /// <param name="storeid">店铺ID</param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetStoreConfig(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (app == null)
            {
                return Json(new { isok = -1, msg = "找不到该程序的授权资料", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(app.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到店铺相关资料", data = "" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
            }
            catch (Exception)
            {
                store.funJoinModel = new StoreConfigModel();
            }
            if (store.funJoinModel.imSwitch)
            {
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetKfInfo(app.AppId);
                store.funJoinModel.imSwitch = userInfo != null;
                if (userInfo != null)
                {
                    store.kfInfo = new { nickName = userInfo.NickName, uid = userInfo.Id, headImgUrl = userInfo.HeadImgUrl };
                }

            }

            object postdata = new
            {
                store = store,
            };
            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        ///  商品详情
        /// </summary>
        /// <param name="unionid"></param>
        /// <param name="storeid">店铺ID</param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetStoreDetail(string appid, int goodsid)
        {
            try
            {
                if (string.IsNullOrEmpty(appid))
                {
                    return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
                }
                XcxAppAccountRelation appRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
                if (appRelation == null)
                {
                    return Json(new { isok = -1, msg = "找不到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
                }
                if (goodsid <= 0)
                {
                    return Json(new { isok = -1, msg = "没有获取到商品标识" }, JsonRequestBehavior.AllowGet);
                }
                StoreGoods good = StoreGoodsBLL.SingleModel.GetModelByCache(goodsid);
                if (good == null)
                {
                    return Json(new { isok = -1, msg = "找不到商品信息" }, JsonRequestBehavior.AllowGet);
                }

                #region 会员折扣显示
                int uid = Utility.IO.Context.GetRequestInt("uid", 0);
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(uid);
                VipRelation vipinfo = null;
                if (userInfo != null)
                {
                    vipinfo = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id} and state>=0");
                }

                VipLevel level = null;
                if (vipinfo != null)
                {
                    level = VipLevelBLL.SingleModel.GetModel($"id={vipinfo.levelid} and state>=0");
                }
                if (level == null)
                {
                    good.discountPrice = good.Price;
                    //多规格处理
                    if (good.GASDetailList != null && good.GASDetailList.Count > 0)
                    {
                        List<StoreGoodsAttrDetail> detaillist = good.GASDetailList.ToList();
                        detaillist.ForEach(g => g.discountPrice = g.price);
                        good.AttrDetail = JsonConvert.SerializeObject(detaillist);
                    }
                }
                else
                {
                    if (level.type == 1)
                    {
                        good.discount = level.discount;
                        good.discountPrice = Convert.ToInt32(good.Price * (level.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(good.Price * (good.discount * 0.01));
                        //多规格处理
                        if (good.GASDetailList != null && good.GASDetailList.Count > 0)
                        {
                            List<StoreGoodsAttrDetail> detaillist = good.GASDetailList.ToList();
                            detaillist.ForEach(g =>
                            {
                                g.discount = level.discount;
                                g.discountPrice = Convert.ToInt32(g.price * (g.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.price * (g.discount * 0.01));
                            });
                            good.AttrDetail = JsonConvert.SerializeObject(detaillist);
                        }
                    }
                    else
                    {
                        if (level.type == 2 && !string.IsNullOrEmpty(level.gids))
                        {
                            List<string> idlist = level.gids.Split(',').ToList();
                            if (idlist == null || idlist.Count <= 0)
                            {
                                good.discountPrice = good.Price;
                            }
                            else
                            {
                                if (idlist.Contains(good.Id.ToString()))
                                {
                                    good.discount = level.discount;
                                    good.discountPrice = Convert.ToInt32(good.Price * (level.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(good.Price * (level.discount * 0.01));
                                    //多规格处理
                                    if (good.GASDetailList != null && good.GASDetailList.Count > 0)
                                    {
                                        List<StoreGoodsAttrDetail> detaillist = good.GASDetailList.ToList();
                                        detaillist.ForEach(g =>
                                        {
                                            g.discount = level.discount;
                                            g.discountPrice = Convert.ToInt32(g.price * (g.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.price * (g.discount * 0.01));
                                        });
                                        good.AttrDetail = JsonConvert.SerializeObject(detaillist);
                                    }
                                }
                                else
                                {
                                    good.discountPrice = good.Price;
                                    //多规格处理
                                    if (good.GASDetailList != null && good.GASDetailList.Count > 0)
                                    {
                                        List<StoreGoodsAttrDetail> detaillist = good.GASDetailList.ToList();
                                        detaillist.ForEach(g => g.discountPrice = g.price);
                                        good.AttrDetail = JsonConvert.SerializeObject(detaillist);
                                    }
                                }
                            }
                        }
                        else
                        {
                            good.discountPrice = good.Price;
                            //多规格处理
                            if (good.GASDetailList != null && good.GASDetailList.Count > 0)
                            {
                                List<StoreGoodsAttrDetail> detaillist = good.GASDetailList.ToList();
                                detaillist.ForEach(g => g.discountPrice = g.price);
                                good.AttrDetail = JsonConvert.SerializeObject(detaillist);
                            }
                        }
                    }
                }

                #endregion

                //详情图
                List<C_Attachment> shopDtlImgs = C_AttachmentBLL.SingleModel.GetListByCache(good.Id, (int)AttachmentItemType.小程序商城商品详情图);
                List<string> shopDtlImgUrls = shopDtlImgs.Select(s => s.filepath).ToList();
                //object shopDtlImgUrls_new = shopDtlImgs.Select(s => new { filepath = s.filepath , filepath_new =ImgHelper.ResizeImg(s.filepath, 750, 600) }).ToList();

                //商品轮播图
                List<C_Attachment> shopSlideshows = C_AttachmentBLL.SingleModel.GetListByCache(good.Id, (int)AttachmentItemType.小程序商城商品轮播图);
                List<string> shopSlideshowUrls = shopSlideshows.Select(s => s.filepath).ToList();
                //object shopSlideshowUrls_new = shopSlideshows.Select(s => new { filepath = s.filepath, filepath_new = ImgHelper.ResizeImg(s.filepath, 750, 600) }).ToList();

                //店铺详情轮播图
                List<C_Attachment> storeDtlImgs = C_AttachmentBLL.SingleModel.GetListByCache(good.Id, (int)AttachmentItemType.小程序商城店铺详情轮播图);
                List<string> storeDtlImgUrls = storeDtlImgs.Select(s => s.filepath).ToList();
                //object storeDtlImgUrls_new = storeDtlImgs.Select(s => new { filepath = s.filepath, filepath_new = ImgHelper.ResizeImg(s.filepath, 750, 600) }).ToList();


                //规格 / 属性 名称
                List<StoreGoodsAttrSpec> attrSpecs = StoreGoodsAttrSpecBLL.SingleModel.GetListByGoodsIdCache(goodsid) ?? new List<StoreGoodsAttrSpec>();//记录商品有哪些规格/规格值的表
                List<StoreGoodsAttr> goodsAttrs = new List<StoreGoodsAttr>();//规格
                List<StoreGoodsSpec> goodsAttrSpacs = new List<StoreGoodsSpec>();//规格值
                if (attrSpecs != null && attrSpecs.Any())
                {
                    foreach (int attrId in attrSpecs.Select(x => x.AttrId).Distinct())
                    {
                        goodsAttrs.Add(StoreGoodsAttrBLL.SingleModel.GetModelByCache(attrId));
                    }
                    foreach (int attrId in attrSpecs.Select(x => x.SpecId).Distinct())
                    {
                        goodsAttrSpacs.Add(StoreGoodsSpecBLL.SingleModel.GetModelByCache(attrId));
                    }

                    //拼接规格
                    goodsAttrs.ForEach(x =>
                    {
                        x.SpecList = goodsAttrSpacs.Where(y => x.Id == y.AttrId).ToList();
                    });
                }

                object postdata = new
                {
                    goodsdetail = good,
                    descImgList = shopDtlImgUrls,
                    descImgList2 = storeDtlImgUrls,
                    imgattent = shopSlideshowUrls,
                    goodsAttrList = goodsAttrs.OrderBy(x => x.Id),
                    //descImgList_new = shopDtlImgUrls_new,
                    //descImgList2_new = storeDtlImgUrls_new,
                    //imgattent_new = shopSlideshowUrls_new
                };
                return Json(new { isok = 1, msg = "成功获取商品信息", postdata = postdata }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
                return Json(new { isok = -1, msg = "请求失败,请重试" }, JsonRequestBehavior.AllowGet);
            }

        }


        /// <summary>
        ///  添加/编辑收货地址
        /// </summary>
        /// <param name="State"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddOrEditMyAddressDefault(string appid, string openid, string addressjson)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (app == null)
            {
                return Json(new { isok = -1, msg = "找不到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(app.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }

            StoreAddress address = JsonConvert.DeserializeObject<StoreAddress>(addressjson);
            if (address == null)
            {
                return Json(new { isok = -1, msg = "没有找到地址信息" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrWhiteSpace(address.Address))
            {
                return Json(new { isok = -1, msg = "没有接收到详细地址" }, JsonRequestBehavior.AllowGet);
            }
            if (address.Id <= 0)
            {
                address.StoreId = store.Id;
                address.UserId = userInfo.Id;
                address.CreateDate = DateTime.Now;

                address.Id = Convert.ToInt32(StoreAddressBLL.SingleModel.Add(address));
                if (address.Id <= 0)
                {
                    return Json(new { isok = -1, msg = "编辑地址失败,请重试" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                StoreAddress db_Address = StoreAddressBLL.SingleModel.GetModel(address.Id);
                if (db_Address == null)
                {
                    return Json(new { isok = -1, msg = "没有找到地址信息" }, JsonRequestBehavior.AllowGet);
                }
                db_Address.NickName = address.NickName;
                db_Address.TelePhone = address.TelePhone;
                db_Address.Province = address.Province;
                db_Address.CityCode = address.CityCode;
                db_Address.AreaCode = address.AreaCode;
                db_Address.Address = address.Address;

                if (!StoreAddressBLL.SingleModel.Update(db_Address))
                {
                    return Json(new { isok = -1, msg = "编辑地址失败,请重试" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isok = 1, msg = "成功编辑地址" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取我的收货地址
        /// </summary>
        /// isDefault : 0为否,1 为是
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMyAddress(string appid, string openid, int addressId = 0, int isDefault = 0)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "找不到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }

            //返回默认地址
            if (isDefault == 1)
            {
                StoreAddress address = StoreAddressBLL.SingleModel.GetModel($" StoreId = {store.Id} and UserId = {userInfo.Id} and State = 0 and IsDefault = 1 ");
                if (address == null)
                {
                    return Json(new { isok = -1, msg = "没有找到地址信息" }, JsonRequestBehavior.AllowGet);
                }
                //拼接地址名称
                string provinceName = address.Province;
                string cityName = address.CityCode;
                string areaName = address.AreaCode;

                address.Address_Dtl = $"{provinceName} {cityName} {areaName} {address.Address}";

                object postdata = new
                {
                    address = address
                };
                return Json(new { isok = 1, msg = "成功获取地址", postdata = postdata }, JsonRequestBehavior.AllowGet);
            }


            if (addressId == 0)
            {
                List<StoreAddress> addressList = StoreAddressBLL.SingleModel.GetList($" StoreId = {store.Id} and UserId = {userInfo.Id} and State =0 ");
                //拼接地址名称
                addressList.ForEach(x =>
                {
                    string provinceName = x.Province;
                    string cityName = x.CityCode;
                    string areaName = x.AreaCode;

                    x.Address_Dtl = $"{provinceName} {cityName} {areaName} {x.Address}";
                });



                object postdata = new
                {
                    addressList = addressList.Select(s => new { s.Address, s.Id, s.Province, s.CityCode, s.AreaCode, s.NickName, s.TelePhone, s.IsDefault })
                };

                return Json(new { isok = 1, msg = "成功获取地址", postdata = postdata }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                StoreAddress address = StoreAddressBLL.SingleModel.GetModel(addressId);
                if (address == null)
                {
                    return Json(new { isok = -1, msg = "没有找到地址信息" }, JsonRequestBehavior.AllowGet);
                }
                //拼接地址名称
                string provinceName = address.Province;
                string cityName = address.CityCode;
                string areaName = address.AreaCode;

                address.Address_Dtl = $"{provinceName} {cityName} {areaName} {address.Address}";

                object postdata = new
                {
                    address = address
                };
                return Json(new { isok = 1, msg = "成功获取地址", postdata = postdata }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///  设定默认收货地址
        /// </summary>
        /// <param name="State"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult setMyAddressDefault(string appid, string openid, int addressId)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "找不到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }
            StoreAddress address = StoreAddressBLL.SingleModel.GetModel(addressId);
            if (address == null)
            {
                return Json(new { isok = -1, msg = "没有找到地址资料" }, JsonRequestBehavior.AllowGet);
            }
            if (!StoreAddressBLL.SingleModel.SetDefault(addressId, userInfo.Id))
            {
                return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  删除我的收货地址
        /// </summary>
        /// <param name="State"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult deleteMyAddress(string appid, string openid, int AddressId)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "找不到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "没有找到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }
            StoreAddress address = StoreAddressBLL.SingleModel.GetModel(AddressId);
            if (address == null)
            {
                return Json(new { isok = -1, msg = "没有找到地址信息" }, JsonRequestBehavior.AllowGet);
            }
            address.State = -1;

            if (!StoreAddressBLL.SingleModel.Update(address))
            {
                return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取商品列表
        /// </summary>
        /// PageType 6 默认 电商版 22专业版
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetGoodsList(string appid, int typeid = 0, int pageindex = 1, int pagesize = 10, int orderbyid = 0, string goodname = "")
        {
            string d = "";
            try
            {

                if (string.IsNullOrEmpty(appid))
                {
                    return Json(new { isok = -1, msg = "读取不到授权信息" }, JsonRequestBehavior.AllowGet);
                }
                XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
                if (umodel == null)
                {
                    return Json(new { isok = -1, msg = "读取不到商家对小程序授权的消息" }, JsonRequestBehavior.AllowGet);
                }
                int levelid = Utility.IO.Context.GetRequestInt("levelid", 0);
                VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");

                object postdata = new object();
                Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
                if (store == null)
                {
                    return Json(new { isok = -1, msg = "找不到店铺相关资料" }, JsonRequestBehavior.AllowGet);
                }

                List<StoreGoodsType> goodsTypeList = StoreGoodsTypeBLL.SingleModel.GetlistByStoreId(store.Id) ?? new List<StoreGoodsType>();
                int typeid_ = typeid;
                if (typeid_ < 0 && goodsTypeList.Any())
                {
                    typeid_ = goodsTypeList[0].Id;
                }
                d += 1;
                List<StoreGoods> goods = StoreGoodsBLL.SingleModel.GetListByStoreId(store.Id, typeid, pageindex, pagesize, orderbyid, goodname);
                goods.ForEach(g =>
                {
                    //商品图片分辨率设定为 320 * 280
                    g.ImgUrl = g.ImgUrl;
                    //g.ImgUrl = ImgHelper.ResizeImg(g.ImgUrl, 320, 280);
                });

                #region 会员打折
                if (goods != null && goods.Count > 0)
                {
                    goods.ForEach(g => g.discountPrice = g.Price);//无折扣
                    if (level != null)
                    {
                        if (level.type == 1)//全场折扣
                        {
                            goods.ForEach(g =>
                            {
                                g.discountPrice = Convert.ToInt32(g.Price * (level.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (level.discount * 0.01));
                                g.discount = level.discount;
                            });

                        }
                        else//部分折扣
                        {
                            List<StoreGoods> list = goods.FindAll(g => level.gids.Split(',').Contains(g.Id.ToString()));
                            if (list != null && list.Count > 0)
                            {
                                list.ForEach(g =>
                                {
                                    g.discount = level.discount;
                                    g.discountPrice = Convert.ToInt32(g.Price * (level.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (level.discount * 0.01));
                                });
                            }
                        }
                    }
                }
                #endregion

                postdata = new
                {
                    goodslist = goods,
                    goodsTypeList = goodsTypeList
                };
                return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = "异常" + ex.Message + "  " + d }, JsonRequestBehavior.AllowGet);
            }

        }



        #region 购物车/订单
        /// <summary>
        /// 查询购物车指定记录 (暂不用)   --不适用新逻辑
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="orderbyid"></param>
        /// <returns></returns>
        public ActionResult getGoodsCarDataByIds(string appid, string openid, List<int> goodsCarList)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "找不到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }


            List<StoreGoodsCart> myCartList = StoreGoodsCartBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCarList)}) and UserInfo = {userInfo.Id} ");
            if (myCartList == null || !myCartList.Any())
            {
                return Json(new { isok = -1, msg = "通信异常,获取购物车资料失败" }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { isok = 1, msg = "成功获取购物车", postdata = myCartList }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 查询购物车
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult getGoodsCarData(string appid, string openid)
        {
            string msg = "";
            try
            {
                if (string.IsNullOrEmpty(appid))
                {
                    return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
                }
                XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
                if (umodel == null)
                {
                    return Json(new { isok = -1, msg = "找不到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
                }
                Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
                if (store == null)
                {
                    return Json(new { isok = -1, msg = "找不到店铺相关资料" }, JsonRequestBehavior.AllowGet);
                }
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
                if (userInfo == null)
                {
                    return Json(new { isok = -1, msg = "没有找到用户资料" }, JsonRequestBehavior.AllowGet);
                }
                List<StoreGoodsCart> myCarts = StoreGoodsCartBLL.SingleModel.GetMyCart(store.Id, userInfo.Id) ?? new List<StoreGoodsCart>();
                List<StoreGoods> goods = new List<StoreGoods>();
                StoreGoods curGood = new StoreGoods();
                myCarts.ForEach(c =>
                {
                    curGood = StoreGoodsBLL.SingleModel.GetModelByCache(c.GoodsId);
                    //购物车页面，图片裁剪为 180*180
                    //curGood.ImgUrl = ImgHelper.ResizeImg(curGood.ImgUrl, 180, 180);
                    //curGood.ImgUrl = curGood.ImgUrl;
                    curGood.Description = "";
                    goods.Add(curGood);

                    c.goodsMsg = curGood;
                });
                #region 会员打折
                int levelid = Utility.IO.Context.GetRequestInt("levelid", 0);
                VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");
                if (goods != null && goods.Count > 0)
                {
                    goods.ForEach(g => g.discountPrice = g.Price);//无折扣
                    if (level.type == 1)//全场折扣
                    {
                        goods.ForEach(g =>
                        {
                            g.discountPrice = Convert.ToInt32(g.Price * (level.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (level.discount * 0.01));
                            g.discount = level.discount;
                            //多规格处理
                            if (g.GASDetailList != null && g.GASDetailList.Count > 0)
                            {
                                List<StoreGoodsAttrDetail> detaillist = g.GASDetailList.ToList();
                                detaillist.ForEach(ga =>
                                {
                                    ga.discount = level.discount;
                                    ga.discountPrice = Convert.ToInt32(ga.price * (ga.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(ga.price * (ga.discount * 0.01));
                                });
                                g.AttrDetail = JsonConvert.SerializeObject(detaillist);
                            }

                        });
                    }
                    else//部分折扣
                    {
                        List<StoreGoods> list = goods.FindAll(g => level.gids.Split(',').Contains(g.Id.ToString()));
                        if (list != null && list.Count > 0)
                        {
                            list.ForEach(g =>
                            {
                                g.discount = level.discount;
                                g.discountPrice = Convert.ToInt32(g.Price * (level.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (level.discount * 0.01));
                                //多规格处理
                                if (g.GASDetailList != null && g.GASDetailList.Count > 0)
                                {
                                    List<StoreGoodsAttrDetail> detaillist = g.GASDetailList.ToList();
                                    detaillist.ForEach(ga =>
                                    {
                                        ga.discount = level.discount;
                                        ga.discountPrice = Convert.ToInt32(ga.price * (ga.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(ga.price * (ga.discount * 0.01));
                                    });
                                    g.AttrDetail = JsonConvert.SerializeObject(detaillist);
                                }

                            });
                        }
                    }
                }

                #endregion

                //根据分类来拆分排序商品
                List<StoreGoodsType> allTypes = StoreGoodsTypeBLL.SingleModel.GetlistByStoreId(store.Id);
                List<StoreGoodsType> types = new List<StoreGoodsType>();
                StoreGoodsType curType = null;

                goods.Select(x => x.TypeId).Distinct().ToList().ForEach(curTypeId =>
                {
                    curType = allTypes?.FirstOrDefault(type => type.Id == curTypeId);
                    if (curType != null && curType.Id > 0)
                    {
                        types.Add(curType);
                    }
                });
                object postdata = types.Select(x => new
                {
                    typeName = x.Name,
                    typeid = x.Id,
                    GoodsCar = myCarts.Where(y => y.goodsMsg.TypeId == x.Id).ToList()
                });
                return Json(new { isok = 1, msg = "成功获取购物车", postdata = postdata }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
                return Json(new { isok = -1, msg = ex.Message + msg }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// 查询购物车   2017-12-13
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult getGoodsCarData_new(string appid, string openid)
        {
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 6);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            object postdata = new { };

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "没有找到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "没有找到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "没有获取到用户资料" }, JsonRequestBehavior.AllowGet);
            }
            List<StoreGoodsCart> myCarts = StoreGoodsCartBLL.SingleModel.GetMyCart(store.Id, userInfo.Id) ?? new List<StoreGoodsCart>();
            if (myCarts == null || !myCarts.Any())
            {
                return Json(new { isok = -1, msg = "购物车数据为空", postdata = postdata }, JsonRequestBehavior.AllowGet);
            }

            List<StoreGoods> goods = new List<StoreGoods>();
            StoreGoods curGood = new StoreGoods();
            string goodsIds = string.Join(",",myCarts.Select(s=>s.GoodsId));
            List<StoreGoods> storeGoodsListTemp = StoreGoodsBLL.SingleModel.GetListByIds(goodsIds);
            myCarts.ForEach(c =>
            {
                curGood = storeGoodsListTemp?.FirstOrDefault(f=>f.Id == c.GoodsId);
                //购物车页面，图片裁剪为 180*180
                //curGood.ImgUrl = ImgHelper.ResizeImg(curGood.ImgUrl, 180, 180);
                curGood.Description = "";
                goods.Add(curGood);

                c.goodsMsg = curGood;
            });

            #region 会员打折
            myCarts.ForEach(g => g.originalPrice = g.Price);
            //获取会员信息
            VipRelation vipInfo = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id} and state>=0");
            if (vipInfo != null)
            {
                VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={vipInfo.levelid} and state>=0");
                if (levelinfo != null)
                {
                    if (levelinfo.type == 1)//全场打折
                    {
                        myCarts.ForEach(g =>
                        {
                            g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01));
                            g.discount = levelinfo.discount;
                        });

                    }
                    else if (levelinfo.type == 2)//部分打折
                    {
                        List<string> gids = levelinfo.gids.Split(',').ToList();
                        myCarts.ForEach(g =>
                        {
                            if (gids.Contains(g.GoodsId.ToString()))
                            {
                                g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01));
                                g.discount = levelinfo.discount;
                            }
                        });
                    }
                }
            }
            int levelid = Utility.IO.Context.GetRequestInt("levelid", 0);
            VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");
            if (goods != null && goods.Count > 0)
            {
                goods.ForEach(g => g.discountPrice = g.Price);//无折扣
                if (level.type == 1)//全场折扣
                {
                    goods.ForEach(g =>
                    {
                        g.discountPrice = Convert.ToInt32(g.Price * (level.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (level.discount * 0.01));
                        g.discount = level.discount;
                        //多规格处理
                        if (g.GASDetailList != null && g.GASDetailList.Count > 0)
                        {
                            List<StoreGoodsAttrDetail> detaillist = g.GASDetailList.ToList();
                            detaillist.ForEach(ga =>
                            {
                                ga.discount = level.discount;
                                ga.discountPrice = Convert.ToInt32(ga.price * (ga.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(ga.price * (ga.discount * 0.01));
                            });
                            g.AttrDetail = JsonConvert.SerializeObject(detaillist);
                        }

                    });
                }
                else//部分折扣
                {
                    List<StoreGoods> list = goods.FindAll(g => level.gids.Split(',').Contains(g.Id.ToString()));
                    if (list != null && list.Count > 0)
                    {
                        list.ForEach(g =>
                        {
                            g.discount = level.discount;
                            g.discountPrice = Convert.ToInt32(g.Price * (level.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (level.discount * 0.01));
                            //多规格处理
                            if (g.GASDetailList != null && g.GASDetailList.Count > 0)
                            {
                                List<StoreGoodsAttrDetail> detaillist = g.GASDetailList.ToList();
                                detaillist.ForEach(ga =>
                                {
                                    ga.discount = level.discount;
                                    ga.discountPrice = Convert.ToInt32(ga.price * (ga.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(ga.price * (ga.discount * 0.01));
                                });
                                g.AttrDetail = JsonConvert.SerializeObject(detaillist);
                            }

                        });
                    }
                }
            }
            #endregion
            //根据分类来拆分排序商品
            List<StoreGoodsType> allTypes = StoreGoodsTypeBLL.SingleModel.GetlistByStoreId(store.Id);
            List<StoreGoodsType> types = new List<StoreGoodsType>();
            StoreGoodsType curType = null;
            goods?.Select(x => x.TypeId)?.Distinct().ToList().ForEach(curTypeId =>
            {
                curType = allTypes?.FirstOrDefault(type => type.Id == curTypeId);
                if (curType != null && curType.Id > 0)
                {
                    types.Add(curType);
                }
            });

            postdata = types.Select(x => new
            {
                typeName = x.Name,
                typeid = x.Id,
                GoodsCar = myCarts.Where(y => y.goodsMsg.TypeId == x.Id).ToList()
            });
            return Json(new { isok = 1, msg = "成功获取购物车", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodid"></param>
        /// <param name="attrSpacStr"></param>
        /// <param name="SpecInfo">商品规格(格式)：规格1：属性1 规格2：属性2 如:（颜色：白色 尺码：M）</param>
        /// <param name="qty"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodsCarData(string appid, string openid, int goodid, string attrSpacStr, string SpecInfo, int qty, int newCartRecord = 0)
        {

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "没有找到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "没有找到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }
            if (qty <= 0)
            {
                return Json(new { isok = -1, msg = "选购商品数量请输入大于0的数字" }, JsonRequestBehavior.AllowGet);
            }
            StoreGoods good = StoreGoodsBLL.SingleModel.GetModel(goodid);
            if (good == null)
            {
                return Json(new { isok = -1, msg = "这个商品消失了" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrWhiteSpace(attrSpacStr))
            {
                if (!good.GASDetailList.Any(x => x.id.Equals(attrSpacStr)))
                {
                    return Json(new { isok = -1, msg = "这个规格的商品消失了" }, JsonRequestBehavior.AllowGet);
                }
            }
            if (!(good.State >= 0 && good.IsSell == 1))
            {
                return Json(new { isok = -1, msg = "这个商品还未启用售卖" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }

            //清除购物车缓存
            StoreGoodsCartBLL.SingleModel.RemoveStoreGoodsCartCache(userInfo.Id);
            StoreGoodsCart dbGoodCar = StoreGoodsCartBLL.SingleModel.GetModel($" UserId={userInfo.Id} and GoodsId={good.Id} and SpecIds='{attrSpacStr}' and State = 0 ");
            if (dbGoodCar == null || newCartRecord == 1)
            {
                StoreGoodsCart goodsCar = new StoreGoodsCart
                {
                    StoreId = store.Id,
                    GoodsId = good.Id,
                    SpecIds = attrSpacStr,
                    Count = qty,
                    Price = !string.IsNullOrWhiteSpace(attrSpacStr) ? good.GASDetailList.First(x => x.id.Equals(attrSpacStr)).price : good.Price,
                    SpecInfo = SpecInfo,
                    UserId = userInfo.Id,
                    CreateDate = DateTime.Now,
                    State = 0
                };
                //加入购物车
                int id = Convert.ToInt32(StoreGoodsCartBLL.SingleModel.Add(goodsCar));
                if (id > 0)
                {
                    return Json(new { isok = 1, msg = "成功添加购物车", cartid = id }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { isok = -1, msg = "添加购物车失败", cartid = 0 }, JsonRequestBehavior.AllowGet);
            }
            dbGoodCar.Count += qty;
            if (StoreGoodsCartBLL.SingleModel.Update(dbGoodCar))
            {
                return Json(new { isok = 1, msg = "成功添加购物车", cardid = dbGoodCar.Id }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = -1, msg = "失败添加购物车" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 从购物车 删除商品
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodsCarId"></param>
        /// <param name="function">0为更新,-1为删除</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult updateOrDeleteGoodsCarData(string appid, string openid, List<StoreGoodsCart> goodsCarModel, int function)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "没有找到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "没有找到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }

            //清除购物车缓存
            StoreGoodsCartBLL.SingleModel.RemoveStoreGoodsCartCache(userInfo.Id);

            if (goodsCarModel != null && goodsCarModel.Count > 0)
            {
                string cartIds = string.Join(",",goodsCarModel.Select(s=>s.Id));
                List<StoreGoodsCart> storeGoodsCartList = StoreGoodsCartBLL.SingleModel.GetListByIds(cartIds);
                foreach (StoreGoodsCart item in goodsCarModel)
                {
                    StoreGoodsCart goodsCar = storeGoodsCartList?.FirstOrDefault(f=>f.Id == item.Id);
                    if (goodsCar == null)
                    {
                        return Json(new { isok = -1, msg = "购物车信息已失效" }, JsonRequestBehavior.AllowGet);
                    }
                    if (goodsCar.UserId != userInfo.Id)
                    {
                        return Json(new { isok = -1, msg = "购物车记录不存在" }, JsonRequestBehavior.AllowGet);
                    }
                    if (goodsCar.State == 1)
                    {
                        return Json(new { isok = -1, msg = "该购物车记录已经下过单啦" }, JsonRequestBehavior.AllowGet);
                    }
                    //将记录状态改为删除
                    if (function == -1)
                    {
                        goodsCar.State = -1;
                    }
                    else if (function == 0)//根据传入参数更新购物车内容
                    {
                        goodsCar.SpecIds = item.SpecIds;
                        goodsCar.SpecInfo = item.SpecInfo;
                        goodsCar.Count = item.Count;


                        //价格因更改规格随之改变
                        StoreGoods carGoods = StoreGoodsBLL.SingleModel.GetModel(goodsCar.GoodsId);
                        if (carGoods == null)
                        {
                            goodsCar.GoodsState = 2;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(carGoods.AttrDetail))
                            {
                                int? price = carGoods.GASDetailList.Where(x => x.id.Equals(goodsCar.SpecIds))?.FirstOrDefault()?.price;
                                if (price != null)
                                {
                                    goodsCar.Price = Convert.ToInt32(price);
                                }
                            }
                        }
                    }

                    bool success = StoreGoodsCartBLL.SingleModel.Update(goodsCar, "State,Count,SpecInfo,SpecIds,Price,GoodsState");
                    if (!success)
                    {
                        return Json(new { isok = -1, msg = "编辑购物车失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { isok = 1, msg = "成功编辑购物车" }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 根据所选购买商品 配上 运费模板 计算运费
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getOrderGoodsBuyPriceByCarIds(string appid, string openid, string goodCarIdStr)
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
            if (string.IsNullOrEmpty(goodCarIdStr))
            {
                return Json(new { isok = -1, msg = "购物车异常" }, JsonRequestBehavior.AllowGet);
            }
            List<string> goodCarIdList = goodCarIdStr.Split(',').ToList();
            if (goodCarIdStr.Substring(goodCarIdStr.Length - 1, 1) == ",")
            {
                goodCarIdList = goodCarIdStr.Substring(0, goodCarIdStr.Length - 1).Split(',').ToList();
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            List<StoreGoodsCart> goodsCar = StoreGoodsCartBLL.SingleModel.GetList($" Id in({string.Join(",", goodCarIdList)}) and UserId = {userInfo.Id} ");
            if (goodsCar == null || !goodsCar.Any())
            {
                return Json(new { isok = -1, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
            }

            List<StoreFreightTemplate> fmodelList = StoreFreightTemplateBLL.SingleModel.GetList($" StoreId = {store.Id} and state >= 0 ");
            if (fmodelList == null || !fmodelList.Any())
            {
                return Json(new { isok = 1, msg = "商家无设定运费模板" }, JsonRequestBehavior.AllowGet);
            }
            //购买价格计算
            int qtySum = goodsCar.Sum(x => x.Count);
            fmodelList.ForEach(x =>
            {
                int friPrice = 0;
                if (qtySum <= x.BaseCount)
                {
                    friPrice = x.BaseCost;
                }
                else
                {
                    //初阶费用加上额外费用
                    friPrice = x.BaseCost + (qtySum - x.BaseCount) * x.ExtraCost;
                }
                //临时存放模板所需运费
                x.sum = (friPrice * 0.01).ToString();
            });

            object postdata = fmodelList.Select(x => new { x.Id, x.Name, x.sum });

            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据所选购买商品 配上 运费模板 计算运费
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getOrderGoodsBuyPriceByGoodsIds(string appid, string openid, int goodId, int qty, string attrSpacStr = "")
        {
            if (qty <= 0)
            {
                return Json(new { isok = -1, msg = "选择商品的数量不可为0" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "没有找到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "没有获取到用户资料" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "没有找到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }

            StoreGoods good = StoreGoodsBLL.SingleModel.GetModel(goodId);
            if (good == null)
            {
                return Json(new { isok = -1, msg = "这个商品消失了" }, JsonRequestBehavior.AllowGet);
            }

            List<StoreFreightTemplate> fmodelList = StoreFreightTemplateBLL.SingleModel.GetList($" StoreId = {store.Id} and State >= 0 ");
            if (fmodelList == null || !fmodelList.Any())
            {
                return Json(new { isok = 1, msg = "商家没有设定运费模板" }, JsonRequestBehavior.AllowGet);
            }

            fmodelList.ForEach(x =>
            {
                int friPrice = 0;
                if (qty <= x.BaseCount)
                {
                    friPrice = x.BaseCost;
                }
                else
                {
                    //初阶费用加上额外费用
                    friPrice = x.BaseCost + (qty - x.BaseCount) * x.ExtraCost;
                }
                //临时存放模板所需运费
                x.sum = (friPrice * 0.01).ToString();
            });

            object postdata = fmodelList.Select(x => new { x.Id, x.Name, x.sum });

            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        #region 订单生成及微信支付方式支付
        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addMiniappGoodsOrder(string appid, string openid, string goodCarIdStr, string orderjson, string wxaddressjson = "", int buyMode = (int)miniAppBuyMode.微信支付)
        {
            string dugmsg = "dugmsg";


            #region  基本验证
            if (string.IsNullOrEmpty(orderjson))
            {
                return Json(new { isok = -1, msg = "没有接收到订单消息,请重试" }, JsonRequestBehavior.AllowGet);
            }
            StoreGoodsOrder order = JsonConvert.DeserializeObject<StoreGoodsOrder>(orderjson);
            if (order == null)
            {
                return Json(new { isok = -1, msg = "订单的资料是无效的" }, JsonRequestBehavior.AllowGet);
            }

            if (!Enum.IsDefined(typeof(miniAppBuyMode), buyMode))
            {
                return Json(new { isok = -1, msg = "支付方式不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "没有找到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "没有找到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }
            //不同商家，不同的锁,当前商家若还未创建，则创建一个
            if (!lockObjectDict_Order.ContainsKey(store.Id))
            {
                lockObjectDict_Order.Add(store.Id, new object());
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(goodCarIdStr))
            {
                return Json(new { isok = -1, msg = "通信异常,没有接收到购物车记录,请重新下单." }, JsonRequestBehavior.AllowGet);
            }
            List<string> goodCarIdList = goodCarIdStr.Split(',').ToList();
            if (goodCarIdStr.Substring(goodCarIdStr.Length - 1, 1) == ",")
            {
                goodCarIdList = goodCarIdStr.Substring(0, goodCarIdStr.Length - 1).Split(',').ToList();
            }
            List<StoreGoodsCart> goodsCar = StoreGoodsCartBLL.SingleModel.GetList($" Id in({string.Join(",", goodCarIdList)}) and UserId = {userInfo.Id} and state = 0 ");
            if (goodsCar == null || goodsCar.Count <= 0)
            {
                return Json(new { isok = -1, msg = "该购物车记录已生成订单." }, JsonRequestBehavior.AllowGet);
            }

            //失效商品
            List<StoreGoodsCart> carErrorData = goodsCar.Where(x => x.GoodsState > 0).ToList();
            foreach (StoreGoodsCart x in goodsCar)
            {
                if (x.GoodsState > 0)
                {
                    StoreGoods good = StoreGoodsBLL.SingleModel.GetModel(x.GoodsId);
                    if (good == null)
                    {
                        return Json(new { isok = -1, msg = "没有找到商品资料,请重新选择购买商品！" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { isok = -1, msg = $"商品 '{good.GoodsName}' 已经下架或被删除,请重新选择购买商品！ " }, JsonRequestBehavior.AllowGet);
                }
            }
            if (carErrorData != null && carErrorData.Count > 0)
            {
                return Json(new { isok = -1, msg = $"选择了不再售卖的商品 " }, JsonRequestBehavior.AllowGet);
            }

            StoreFreightTemplate fmodel = StoreFreightTemplateBLL.SingleModel.GetModel(order.FreightTemplateId);
            if (fmodel == null)
            {
                return Json(new { isok = -1, msg = "运费模板信息错误" }, JsonRequestBehavior.AllowGet);
            }
            //改用微信官方地址信息,故此处不读数据库
            StoreAddress address = StoreAddressBLL.SingleModel.GetModel(order.AddressId);
            if (order.AddressId != 0 && address == null)
            {
                return Json(new { isok = -1, msg = "没有找到收货地址信息" }, JsonRequestBehavior.AllowGet);
            }
            else if (order.AddressId == 0 && string.IsNullOrWhiteSpace(wxaddressjson))
            {
                return Json(new { isok = -1, msg = "没有找到微信的收货地址信息" }, JsonRequestBehavior.AllowGet);
            }

            WxAddress address_wx = null;
            try
            {
                address_wx = JsonConvert.DeserializeObject<WxAddress>(wxaddressjson);
                #region 更新用户微信地址到本地数据库
                UserWxAddress model = new UserWxAddress();
                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(UserWxAddressBLL.SingleModel.connName, CommandType.Text, $"SELECT * from userwxaddress where userid={userInfo.Id} order by id DESC LIMIT 0,1", null))
                {
                    if (dr.Read())
                        model = UserWxAddressBLL.SingleModel.GetModel(dr);
                }
                if (model != null)
                {
                    model.WxAddress = wxaddressjson;
                }
                else
                {
                    model = new UserWxAddress();
                    model.WxAddress = wxaddressjson;
                    model.UserId = userInfo.Id;
                }

                int id = UserWxAddressBLL.SingleModel.UpdateUserWxAddress(model);
                #endregion
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "获取微信地址信息失败" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            try
            {
                #region 订单数据填充及运算
                order.buyMode = buyMode;
                order.StoreId = store.Id;
                order.UserId = userInfo.Id;
                order.CreateDate = DateTime.Now;
                order.State = (int)OrderState.未付款;

                if (address != null)//电商原地址信息 --兼容旧版本
                {
                    order.AccepterName = address.NickName;
                    order.AccepterTelePhone = address.TelePhone;
                    order.ZipCode = address.ZipCode;
                    order.Address = $"{address.Province} {address.CityCode} {address.AreaCode} {address.Address}";
                }
                else //微信地址
                {
                    order.ZipCode = address_wx.postalCode;
                    order.Address = $"{address_wx.provinceName} {address_wx.cityName} {address_wx.countyName} {address_wx.detailInfo}";
                    order.AccepterName = address_wx.userName;
                    order.AccepterTelePhone = address_wx.telNumber;
                }

                //运费
                int qtySum = goodsCar.Sum(x => x.Count);
                int friPrice = 0;
                if (qtySum <= fmodel.BaseCount)
                {
                    friPrice = fmodel.BaseCost;
                }
                else
                {
                    //初阶费用加上额外费用
                    friPrice = fmodel.BaseCost + (qtySum - fmodel.BaseCount) * fmodel.ExtraCost;
                }
                order.FreightPrice = friPrice;
                #region 购买价格

                #region 会员打折
                goodsCar.ForEach(g => g.originalPrice = g.Price);
                //获取会员信息
                VipRelation vipInfo = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id} and state>=0");
                StringBuilder sb = null;
                if (vipInfo != null)
                {
                    VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={vipInfo.levelid} and state>=0");
                    if (levelinfo != null)
                    {
                        if (levelinfo.type == 1)//全场打折
                        {
                            goodsCar.ForEach(g => g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)));

                        }
                        else if (levelinfo.type == 2)//部分打折
                        {
                            List<string> gids = levelinfo.gids.Split(',').ToList();
                            goodsCar.ForEach(g =>
                            {
                                if (gids.Contains(g.GoodsId.ToString()))
                                {
                                    g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01));
                                }
                            });
                        }
                        sb = new StringBuilder();

                        foreach (StoreGoodsCart item in goodsCar)
                        {
                            sb.Append(StoreGoodsCartBLL.SingleModel.BuildUpdateSql(item, "Price,originalPrice") + ";");
                        }

                    }
                }

                #endregion

                //商品总价格
                int price = goodsCar.Sum(x => x.Price * x.Count);
                if (price <= 0)
                {
                    return Json(new { isok = -1, msg = "商品价格有误" }, JsonRequestBehavior.AllowGet);
                }
                order.BuyPrice = price + friPrice;
                if (order.BuyPrice > _maxMoney)
                {
                    return Json(new { isok = -1, msg = "单张订单的总金额不可超过9999999.99！" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                #endregion

                lock (lockObjectDict_Order[store.Id])
                {
                    //检查当前商品库存是否足够
                    foreach (StoreGoodsCart x in goodsCar)
                    {
                        int curGoodQty = StoreGoodsBLL.SingleModel.GetGoodQty(x.GoodsId, x.SpecIds);

                        if (curGoodQty < x.Count)
                        {
                            StoreGoods curGood = StoreGoodsBLL.SingleModel.GetModel(x.GoodsId);
                            return Json(new { isok = -1, msg = $"商品: {curGood.GoodsName} 库存不足!" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    //若为储值支付,判定当前用户储值金额是否充足
                    SaveMoneySetUser saveMoneyUser = new SaveMoneySetUser();
                    if (buyMode == (int)miniAppBuyMode.储值支付)
                    {
                        saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(umodel.AppId, userInfo.Id);
                        if (saveMoneyUser == null || saveMoneyUser.AccountMoney < order.BuyPrice)
                        {
                            return Json(new { isok = -1, msg = $" 储值余额不足,请充值！ " }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    //事务生成订单
                    if (!StoreGoodsOrderBLL.SingleModel.addGoodsOrder(order, goodsCar, userInfo, sb, ref dugmsg))
                    {
                        return Json(new { isok = -1, msg = $"订单生成失败！" }, JsonRequestBehavior.AllowGet);
                    }

                    StoreGoodsCart cartmodel = StoreGoodsCartBLL.SingleModel.GetModel("Id=" + goodsCar[0].Id + " and GoodsOrderId>0");
                    if (cartmodel == null)
                    {
                        cartmodel = StoreGoodsCartBLL.SingleModel.GetModel("Id=" + goodsCar[0].Id + " and GoodsOrderId>0");
                    }
                    int curGoodOrderId = cartmodel.GoodsOrderId;
                    StoreGoodsOrder dbOrder = StoreGoodsOrderBLL.SingleModel.GetModel(curGoodOrderId);
                    if (dbOrder == null)
                    {
                        return Json(new { isok = -1, msg = $"订单生成失败！" + dugmsg }, JsonRequestBehavior.AllowGet);
                    }
                    if (buyMode == (int)miniAppBuyMode.微信支付)
                    {
                        #region CtiyModer 生成
                        string no = WxPayApi.GenerateOutTradeNo();

                        CityMorders citymorderModel = new CityMorders
                        {
                            OrderType = (int)ArticleTypeEnum.MiniappGoods,
                            ActionType = (int)ArticleTypeEnum.MiniappGoods,
                            Addtime = DateTime.Now,
                            payment_free = dbOrder.BuyPrice,
                            trade_no = no,
                            Percent = 99,//不收取服务费
                            userip = WebHelper.GetIP(),
                            FuserId = userInfo.Id,
                            Fusername = userInfo.NickName,
                            orderno = no,
                            payment_status = 0,
                            Status = 0,
                            Articleid = 0,
                            CommentId = 0,
                            MinisnsId = store.Id,//商家ID
                            TuserId = dbOrder.Id,//订单的ID
                            ShowNote = $" {umodel.Title}购买商品支付{dbOrder.BuyPrice * 0.01}元",
                            CitySubId = 0,//无分销,默认为0
                            PayRate = 1,
                            buy_num = 0, //无
                            appid = appid,
                        };
                        dugmsg += 14;
                        int orderid = Convert.ToInt32(new CityMordersBLL().Add(citymorderModel));
                        dugmsg += 15;
                        dbOrder.OrderId = orderid;
                        #endregion
                    }

                    #region 更新对外订单号及对应CityModer的ID
                    //对外订单号规则：年月日时分 + 电商本地库ID最后3位数字
                    string idStr = dbOrder.Id.ToString();
                    if (idStr.Length >= 3)
                    {
                        idStr = idStr.Substring(idStr.Length - 3, 3);
                    }
                    else
                    {
                        idStr.PadLeft(3, '0');
                    }
                    idStr = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{idStr}";
                    dbOrder.OrderNum = idStr;
                    dugmsg += 16;
                    StoreGoodsOrderBLL.SingleModel.Update(dbOrder);
                    dugmsg += 17;
                    #endregion


                    if (buyMode == (int)miniAppBuyMode.储值支付)
                    {
                        #region 储值支付 扣除预存款金额并生成消费记录
                        if (payOrderBySaveMoneyUser(dbOrder, saveMoneyUser, sb,ref dugmsg))
                        {
                            return Json(new { isok = 1, msg = "订单生成并支付成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { isok = -1, msg = "订单支付失败", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
                        }
                        #endregion
                    }

                    //记录订单操作日志(用户下单)
                    StoreGoodsOrderLogBLL.SingleModel.Add(new StoreGoodsOrderLog() { GoodsOrderId = dbOrder.Id, UserId = userInfo.Id, LogInfo = $" 成功下单,下单金额：{dbOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now });
                    return Json(new { isok = 1, msg = "订单生成成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                //log4net.LogHelper.WriteInfo(this.GetType(), dugmsg);
                return Json(new { isok = -1, msg = ex.Message + "," + dugmsg }, JsonRequestBehavior.AllowGet);
            }

        }


        /// <summary>
        /// 商城商品支付
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="type"></param>
        /// <param name="openId"></param>
        /// <returns></returns>
        [HttpPost, AuthLoginCheckXiaoChenXun]
        public ActionResult PayOrder(int orderid, int type, string openId)
        {
            try
            {
                CityMorders order = new CityMordersBLL().GetModel(orderid);
                if (order == null || order.payment_status != 0)
                {
                    return ApiResult(false, "订单已经失效");
                }
                C_UserInfo LoginCUser = LoginData;
                if (LoginCUser == null)
                {
                    return ApiResult(false, "登陆信息异常，请重新支付,Entity_null");
                }
                Store store = StoreBLL.SingleModel.GetModel(order.MinisnsId);
                if (store == null)
                {
                    return ApiResult(false, "商家信息错误");
                }
                XcxAppAccountRelation app = _xcxAppAccountRelationBLL.GetModel(store.appId);
                if (app == null)
                {
                    return ApiResult(false, "授权信息错误");
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

                if (openId.IsNullOrWhiteSpace())
                {
                    return ApiResult(false, "Openid_null/Empty");
                }

                JsApiPay jsApiPay = new JsApiPay(HttpContext)
                {
                    total_fee = order.payment_free,
                    openid = openId
                };
                //统一下单，获得预支付码
                WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResultByCity(setting, order, WebConfigBLL.citynotify_url);

                #region 发送消息参数
                StoreGoodsOrder foodOrder = StoreGoodsOrderBLL.SingleModel.GetModel($" OrderId = {orderid}  ") ?? new StoreGoodsOrder();
                if (!string.IsNullOrWhiteSpace(Convert.ToString(unifiedOrderResult.GetValue("prepay_id"))))
                {

                    //增加发送模板消息参数
                    TemplateMsg_UserParam userParam = new TemplateMsg_UserParam();
                    userParam.AppId = app.AppId;
                    userParam.Form_IdType = 1;//form_id 为prepay_id
                    userParam.OrderId = foodOrder.Id;
                    userParam.OrderIdType = (int)TmpType.小程序电商模板;
                    userParam.Open_Id = openId;
                    userParam.AddDate = DateTime.Now;
                    userParam.Form_Id = Convert.ToString(unifiedOrderResult.GetValue("prepay_id"));
                    userParam.State = 1;
                    userParam.SendCount = 0;
                    userParam.AddDate = DateTime.Now;
                    userParam.LoseDateTime = DateTime.Now.AddDays(7);//prepay_id 有效期7天

                    TemplateMsg_UserParamBLL.SingleModel.Add(userParam);
                }
                #endregion
                return ApiResult(true, "下单成功", jsApiPay.GetJsApiParametersnew(setting));
                //else
                //{
                //    NativePay native = new NativePay();
                //    native.openid = openId;
                //    native.total_fee = order.payment_free;
                //    string url = native.GetOrderPayUrlByCity(setting, order);
                //    return ApiResult(true, "下单成功", url);
                //}
            }
            catch (Exception ex)
            {
                return ApiResult(false, "下单异常", ex.Message);
            }

        }
        #endregion

        /// <summary>
        /// 更改支付方式
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        public ActionResult changeBuyMode(string appid, string openid, int goodsorderid, int buyMode)
        {

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "没有找到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }

            StoreGoodsOrder goodOrder = StoreGoodsOrderBLL.SingleModel.GetModel(goodsorderid);
            if (goodOrder == null)
            {
                return Json(new { isok = -1, msg = "没有找到订单资料" }, JsonRequestBehavior.AllowGet);
            }
            if (!Enum.IsDefined(typeof(miniAppBuyMode), buyMode))
            {
                return Json(new { isok = -1, msg = "这是外太空的支付方式" }, JsonRequestBehavior.AllowGet);
            }
            goodOrder.buyMode = buyMode;
            if (StoreGoodsOrderBLL.SingleModel.Update(goodOrder, "buyMode"))
            {
                return Json(new { isok = 1, msg = "修改成功" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = -1, msg = "修改失败" }, JsonRequestBehavior.AllowGet);
            }
        }

        #region 储值支付方式支付

        /// <summary>
        /// 储值支付 扣除预存款金额并生成消费记录(电商)
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        public bool payOrderBySaveMoneyUser(StoreGoodsOrder dbOrder, SaveMoneySetUser saveMoneyUser, StringBuilder sb,ref string errMsg)
        {
            if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
            {
                return false;
            }
            Store store = StoreBLL.SingleModel.GetModel(dbOrder.StoreId);
            try
            {
                store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
            }
            catch (Exception)
            {
                store.funJoinModel = new StoreConfigModel();
            }
            if (store == null || !store.funJoinModel.canSaveMoneyFunction)
            {
                return false;
            }
            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
            {
                return false;
            }
            TransactionModel tran = new TransactionModel();
            if (sb != null)
            {
                tran.Add(sb.ToString());
            }
            tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
            {
                AppId = saveMoneyUser.AppId,
                UserId = dbOrder.UserId,
                MoneySetUserId = saveMoneyUser.Id,
                Type = -1,
                BeforeMoney = saveMoneyUser.AccountMoney,
                AfterMoney = saveMoneyUser.AccountMoney - dbOrder.BuyPrice,
                ChangeMoney = dbOrder.BuyPrice,
                ChangeNote = $" 购买商品,订单号:{dbOrder.OrderNum} ",
                CreateDate = DateTime.Now,
                State = 1
            }));

            dbOrder.State = (dbOrder.FreightTemplateId > 0 ? (int)OrderState.待发货 : (int)OrderState.待核销);
            dbOrder.PayDate = DateTime.Now;

            tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney - {dbOrder.BuyPrice} where id =  {saveMoneyUser.Id} And (AccountMoney - {dbOrder.BuyPrice}) >= 0; ");
            tran.Add(StoreGoodsOrderBLL.SingleModel.BuildUpdateSql(dbOrder, "State,PayDate") + $" and State = {(int)OrderState.未付款 } ; ");


            //记录订单支付日志
            tran.Add(StoreGoodsOrderLogBLL.SingleModel.BuildAddSql(new StoreGoodsOrderLog() { GoodsOrderId = dbOrder.Id, UserId = dbOrder.UserId, LogInfo = $" 订单使用储值成功支付：{dbOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));
            bool isSuccess = SaveMoneySetUserLogBLL.SingleModel.ExecuteTransaction(tran.sqlArray);
            if (isSuccess)
            {
                AfterPaySuccesExecFun(dbOrder);
            }


            return isSuccess;
        }


        /// <summary>
        /// 储值支付后
        /// </summary>
        /// <param name="foodGoodsOrder"></param>
        public void AfterPaySuccesExecFun(StoreGoodsOrder storeGoodsOrder)
        {
            if (storeGoodsOrder == null)
            {
                return;
            }

            try
            {
                #region 自动打单
                List<FoodPrints> PrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {storeGoodsOrder.StoreId}  and state >= 0  and industrytype=2");
                if (PrintList != null && PrintList.Any())
                {
                    //List<MiniappGoodsCart> goodCartList = goodsCartBll.GetList($"GoodsOrderId={gOrder.Id} and state=1 ");
                    List<StoreGoodsCart> goodCartList = StoreGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={storeGoodsOrder.Id} and state=1 ");
                    if (goodCartList != null && goodCartList.Count > 0)
                    {
                        string content = "<MC>0,00005,0</MC><table><tr><td>商品名称</td><td>数量</td><td>单价(元)</td></tr>";
                        foreach (Entity.MiniApp.Stores.StoreGoodsCart goodCart in goodCartList)
                        {
                            goodCart.goodsMsg = StoreGoodsBLL.SingleModel.GetModel(goodCart.GoodsId);
                            if (goodCart.goodsMsg != null)
                            {
                                content += $"<tr><td>{goodCart.goodsMsg.GoodsName}</td><td>{goodCart.Count}</td><td>{goodCart.Price * 0.01}</td></tr>";
                                if (!string.IsNullOrEmpty(goodCart.SpecInfo))
                                {
                                    content += $"<tr><td>规格:{goodCart.SpecInfo.Replace(' ', '|')}</td></tr>";
                                }
                                else
                                {
                                    content += $"<tr><td></td></tr>";
                                }
                            }
                        }
                        content += "</table>┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄";
                        content += $"<table><tr><td>\r\n总价:</td><td>-</td><td>{(storeGoodsOrder.BuyPrice * 0.01).ToString("0.00")}</td></tr></table>";
                        content += $"收货人：{storeGoodsOrder.AccepterName}\r\n";
                        content += $"联系方式:{storeGoodsOrder.AccepterTelePhone}\r\n";
                        content += $"送货地址:{storeGoodsOrder.Address}\r\n";
                        if (!string.IsNullOrEmpty(storeGoodsOrder.Message))
                        {
                            content += $"买家留言:{storeGoodsOrder.Message}";
                        }
                        //content += $"卖家留言:{gOrder.Remark}";
                        PrintList.ForEach(print =>
                        {
                            string returnMsg = FoodYiLianYunPrintHelper.printContent(print.APIKey, print.UserId, print.PrintNo, print.PrintKey, content);
                            FoodYlyReturnModel returnModel = Utility.SerializeHelper.DesFromJson<FoodYlyReturnModel>(returnMsg);
                            //记录订单打印日志
                            string remark = string.Empty;
                            if (returnModel.state == 2)
                            {
                                remark = "提交时间超时";
                                returnModel.state = -1;
                            }
                            else if (returnModel.state == 3)
                            {
                                remark = "参数有误";
                                returnModel.state = -1;
                            }
                            else if (returnModel.state == 4)
                            {
                                remark = "sign加密验证失败";
                                returnModel.state = -1;
                            }
                            else if (returnModel.state == 1)
                            {
                                remark = "发送成功";
                                returnModel.state = 0;
                            }
                            FoodOrderPrintLog log = new FoodOrderPrintLog()
                            {
                                Dataid = returnModel.id,
                                addtime = DateTime.Now,
                                machine_code = print.PrintNo,
                                state = returnModel.state,
                                isupdate = 0,
                                remark = remark,
                                orderId = storeGoodsOrder.Id,
                                printsId = print.Id
                            };
                            FoodOrderPrintLogBLL.SingleModel.Add(log);
                        });
                    }
                }

                //List<FoodPrints> prints = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {storeGoodsOrder.StoreId}  and state >= 0  and industrytype=2");
                //if (prints != null && prints.Any())
                //{
                //    string printContent = PrinterHelper.storePrintOrderContent(storeGoodsOrder);
                //    PrinterHelper.printContent(prints,printContent, storeGoodsOrder.Id);
                //}
                #endregion

                #region 发送电商订单支付通知 模板消息
                object postData = StoreGoodsOrderBLL.SingleModel.getTemplateMessageData(storeGoodsOrder.Id, SendTemplateMessageTypeEnum.电商订单支付成功通知);
                TemplateMsg_Miniapp.SendTemplateMessage(storeGoodsOrder.UserId, SendTemplateMessageTypeEnum.电商订单支付成功通知, (int)TmpType.小程序电商模板, postData);
                #endregion

                //发送模板消息通知商家
                TemplateMsg_Gzh.SendStorePaySuccessTemplateMessage(storeGoodsOrder);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
            }

        }


        /// <summary>
        /// 使用储值支付
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult buyOrderbySaveMoney(string appid, string openid, int goodsorderid)
        {

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "没有找到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }

            StoreGoodsOrder dbOrder = StoreGoodsOrderBLL.SingleModel.GetModel(goodsorderid);
            if (dbOrder == null)
            {
                return Json(new { isok = -1, msg = "没有找到该订单的资料" }, JsonRequestBehavior.AllowGet);
            }
            if (dbOrder.State != 0)
            {
                return Json(new { isok = -1, msg = "为您拦截了一次重复支付操作" }, JsonRequestBehavior.AllowGet);
            }
            if (dbOrder.buyMode != (int)miniAppBuyMode.储值支付)
            {
                return Json(new { isok = -1, msg = "支付方式并非储值支付" }, JsonRequestBehavior.AllowGet);
            }
            SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(umodel.AppId, userInfo.Id);

            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
            {
                return Json(new { isok = -1, msg = "储值金额不足,请更换支付方式", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
            }

            //进入支付流程
            string error = string.Empty;
            if (payOrderBySaveMoneyUser(dbOrder, saveMoneyUser, null,ref error))
            {
                AfterPaySuccesExecFun(dbOrder);
                return Json(new { isok = 1, msg = "支付成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = -1, msg = "支付失败", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
            }
        }



        #endregion


        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="orderId"></param>
        /// <param name="State">OrderState</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult updateMiniappGoodsOrderState(string appid, string openid, int orderId, int State)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "没有找到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "没有找到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }

            StoreGoodsOrder order = StoreGoodsOrderBLL.SingleModel.GetModel(orderId);
            if (order == null)
            {
                return Json(new { isok = -1, msg = "没有找到订单相关资料" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "没有获取到用户资料" }, JsonRequestBehavior.AllowGet);
            }
            if (State == (int)OrderState.取消订单 && order.State > 0)
            {
                return Json(new { isok = -1, msg = $"订单状态为{Enum.GetName(typeof(OrderState), order.State)},不可取消 " }, JsonRequestBehavior.AllowGet);
            }
            if (State != (int)OrderState.取消订单 && State != (int)OrderState.已收货)
            {
                return Json(new { isok = -1, msg = $"您好,这是外星人的订单" }, JsonRequestBehavior.AllowGet);
            }
            //不同状态相应内容处理
            switch (State)
            {
                case (int)OrderState.取消订单:

                    break;
                case (int)OrderState.已收货:
                    order.AcceptDate = DateTime.Now;
                    break;
                default:
                    return Json(new { isok = -1, msg = $"这是个不可以操作的订单状态" }, JsonRequestBehavior.AllowGet);
            }

            order.State = State;
            if (StoreGoodsOrderBLL.SingleModel.Update(order))
            {
                switch (order.State)
                {
                    case (int)OrderState.取消订单:
                        StoreGoodsOrderLogBLL.SingleModel.Add(new StoreGoodsOrderLog() { GoodsOrderId = order.Id, UserId = userInfo.Id, LogInfo = $" 用户 { userInfo.NickName } 于 {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 取消订单 ", CreateDate = DateTime.Now });
                        break;
                    case (int)OrderState.已收货:
                        StoreGoodsOrderLogBLL.SingleModel.Add(new StoreGoodsOrderLog() { GoodsOrderId = order.Id, UserId = userInfo.Id, LogInfo = $" 用户 { userInfo.NickName } 于 {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 确认收货 ", CreateDate = DateTime.Now });

                        //会员加消费金额
                        if (!VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "store", order.BuyPrice))
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + order.Id));
                        }
                        //加销量
                        List<StoreGoodsCart> list = StoreGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {order.Id} ");
                        string goodsIds = string.Join(",",list?.Select(s=>s.GoodsId).Distinct());
                        List<StoreGoods> storeGoodsList = StoreGoodsBLL.SingleModel.GetListByIds(goodsIds);
                        list.Select(x => x.GoodsId).Distinct().ToList().ForEach(x =>
                        {
                            int salesCount1 = list.Where(y => y.GoodsId == x).Sum(y => y.Count);
                            StoreGoods good = storeGoodsList?.FirstOrDefault(f=>f.Id == x);
                            good.salesCount += salesCount1;
                            good.salesCount_real += salesCount1;
                            StoreGoodsBLL.SingleModel.Update(good, "salesCount,salesCount_real");
                        });

                        break;
                    default:
                        //记录订单操作日志(修改订单状态)
                        StoreGoodsOrderLogBLL.SingleModel.Add(new StoreGoodsOrderLog() { GoodsOrderId = order.Id, UserId = userInfo.Id, LogInfo = $" 用户 { userInfo.NickName } 于 {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 操作订单,订单操作后状态为：{Enum.GetName(typeof(OrderState), order.State)} ", CreateDate = DateTime.Now });
                        break;
                }

                return Json(new { isok = 1, msg = "成功更新订单状态" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = -1, msg = "更新订单状态失败" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 判断是否有无效的商品
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车Id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult checkGood(string appid, string openid, string goodCarIdStr)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "找不到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "获取不到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(goodCarIdStr))
            {
                return Json(new { isok = -1, msg = "购物车Id不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (goodCarIdStr.Substring(goodCarIdStr.Length - 1, 1) == ",")
            {
                goodCarIdStr = goodCarIdStr.Substring(0, goodCarIdStr.Length - 1);
            }

            List<StoreGoodsCart> myCartList = StoreGoodsCartBLL.SingleModel.GetMyCartById(goodCarIdStr);
            List<StoreGoods> goodList = StoreGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", myCartList.Select(x => x.GoodsId))}) and (State<0 || IsSell =0) ");

            if (goodList != null && goodList.Count > 0)
            {
                return Json(new { isok = -1, msg = "存在未开启售卖的商品" }, JsonRequestBehavior.AllowGet);
            }
            //多规格商品 判定无效(处理 商品有效而多规格可能被删除)
            int errorCount = 0;
            goodList.ForEach(x =>
            {
                myCartList.ForEach(y =>
                {
                    if (!x.GASDetailList.Where(z => z.id.Equals(y.SpecIds)).Any())
                    {
                        errorCount++;
                    }
                });
            });
            if (errorCount > 0)
            {
                return Json(new { isok = -1, msg = "存在未开启售卖的商品" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = 1, msg = "选购商品全部可下单" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取订单信息列表
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="State">OrderState</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getMiniappGoodsOrder(string appid, string openid, int State = 10, int pageIndex = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "没有找到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "没有找到店铺" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }
            string stateSql = (State != 10 ?
                                ((State == (int)OrderState.正在配送 || State == (int)OrderState.待收货) ?
                                    $" and State in ({(int)OrderState.待收货},{(int)OrderState.正在配送}) " : $" and State = {State} ")
                                  : "");
            List<StoreGoodsOrder> goodOrderList = StoreGoodsOrderBLL.SingleModel.GetList($" StoreId = {store.Id} and UserId = {userInfo.Id} { stateSql } ", pageSize, pageIndex, "", "CreateDate desc");
            if (goodOrderList != null && goodOrderList.Any())
            {
                List<StoreGoodsCart> goodOrderDtlList = StoreGoodsCartBLL.SingleModel.GetList($" GoodsOrderId in ({string.Join(",", goodOrderList.Select(x => x.Id))}) ");
                if (goodOrderDtlList != null && goodOrderList.Any())
                {
                    goodOrderDtlList.ForEach(x =>
                    {
                        x.goodsMsg = StoreGoodsBLL.SingleModel.GetModelByCache(x.GoodsId);

                        //if (x.goodsMsg != null)
                        //{
                        //    x.goodsMsg.ImgUrl = ImgHelper.ResizeImg(x.goodsMsg.ImgUrl, 280, 280);
                        //}
                    });

                    object postdata = goodOrderList.GroupBy(x => x.CreateDate.Year).Select(x =>
                       new
                       {
                           year = x.Key,
                           orderList = x.OrderByDescending(o => o.CreateDate).Select(y =>
                             new
                             {
                                 Id = y.Id,
                                 Title = goodOrderDtlList.FirstOrDefault(z => z.GoodsOrderId == y.Id).goodsMsg?.GoodsName,
                                 ImgUrl = goodOrderDtlList.FirstOrDefault(z => z.GoodsOrderId == y.Id).goodsMsg?.ImgUrl,
                                 Count = goodOrderDtlList.Count(z => z.GoodsOrderId == y.Id),
                                 StateTitle = Enum.GetName(typeof(OrderState), y.State),
                                 State = y.State,
                                 CreateDate = y.CreateDate.ToString("HH:mm:ss"),
                                 OrderDate = y.CreateDate.ToString("MM-dd"),
                                 BuyPrice = y.BuyPrice * 0.01,
                                 cityMorderId = y.OrderId
                             })
                       });
                    return Json(new { isok = 1, msg = "成功获取订单信息列表", postdata = postdata }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { isok = 1, msg = "成功获取订单信息列表", postdata = "" }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="State">OrderState</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getMiniappGoodsOrderById(string appid, string openid, int orderId)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "没有接收到程序的授权key" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "找不到该程序的授权资料" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "获取不到店铺相关资料" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "获取不到用户资料" }, JsonRequestBehavior.AllowGet);
            }
            StoreGoodsOrder goodOrder = StoreGoodsOrderBLL.SingleModel.GetModel($" Id = {orderId} and StoreId = {store.Id} and UserId = {userInfo.Id}");
            if (goodOrder == null)
            {
                return Json(new { isok = -1, msg = "没有找到订单相关资料" }, JsonRequestBehavior.AllowGet);
            }

            List<StoreGoodsCart> goodOrderDtl = StoreGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {goodOrder.Id} ");
            List<StoreGoods> goodList = StoreGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodOrderDtl.Select(x => x.GoodsId))}) ");


            object postdata = new
            {
                buyPrice = (goodOrder.BuyPrice * 0.01).ToString("0.00"),
                freightPrice = (goodOrder.FreightPrice * 0.01).ToString("0.00"),
                stateRemark = Enum.GetName(typeof(OrderState), goodOrder.State),
                orderFriRemark = StoreFreightTemplateBLL.SingleModel.GetModel(goodOrder.FreightTemplateId)?.Name,
                goodOrder = goodOrder,
                goodOrderDtl = goodOrderDtl.Select(x => new
                {
                    price = x.Price * 0.01,
                    goodImgUrl = goodList.Where(y => y.Id == x.GoodsId).FirstOrDefault()?.ImgUrl,
                    goodname = goodList.Where(y => y.Id == x.GoodsId).FirstOrDefault()?.GoodsName,
                    orderDtl = x
                })
            };

            return Json(new { isok = 1, msg = "成功获取订单详情", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion


        /// <summary>
        /// 获取小程序分享卡片接口
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult GetShareImg(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = false, msg = "获取失败(appid不能为空)" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModel($"AppId='{appId}'");
            if (r == null)
            {
                return Json(new { isok = false, msg = "获取失败(还未进行授权)" }, JsonRequestBehavior.AllowGet);
            }

            PageShare share = PageShareBLL.SingleModel.GetModel($"RelationId={r.Id}");
            if (share == null || string.IsNullOrEmpty(share.ShareImg))
                return Json(new { isok = false, msg = "获取失败(还未进行分享设置)" + r.Id }, JsonRequestBehavior.AllowGet);

            return Json(new { isok = true, msg = "获取成功!", obj = new { IsOpen = share.IsOpen, ShareImg = share.ShareImg } }, JsonRequestBehavior.AllowGet);

        }


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
                return Json(new { isok = -1, msg = "还未支持这样的支付方式" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "找不到该程序的授权信息" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "该程序的授权信息失效" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到这个店铺" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "没有找到用户信息" }, JsonRequestBehavior.AllowGet);
            }
            StoreGoodsOrder goodOrder = StoreGoodsOrderBLL.SingleModel.GetModel($" Id = {orderid} and StoreId = {store.Id} and UserId = {userInfo.Id} and State = {(int)OrderState.未付款} ");
            if (goodOrder == null)
            {
                return Json(new { isok = -1, msg = "订单信息异常,请刷新重试" }, JsonRequestBehavior.AllowGet);
            }
            goodOrder.buyMode = buyMode;
            bool success = StoreGoodsOrderBLL.SingleModel.Update(goodOrder, "buyMode");
            return Json(new { isok = success ? 1 : -1, msg = "修改成功！" }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 支付方式获取
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getBuyModeList(string appid, string openid = "", float Money = -1)
        {
            List<buyMode> buyModeList = new List<buyMode>();
            foreach (miniAppBuyMode buyModel in Enum.GetValues(typeof(miniAppBuyMode)))
            {
                buyModeList.Add(new buyMode() { buyModeId = (int)buyModel, buyModeStr = buyModel.ToString(), State = 1, DefaultSort = 1 });
            }

            int money = Convert.ToInt32(Money * 100);
            if (!string.IsNullOrWhiteSpace(appid) && !string.IsNullOrWhiteSpace(openid) && money >= 0)
            {
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
                XcxAppAccountRelation xcxrModel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
                if (userInfo != null && xcxrModel != null)
                {
                    SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(xcxrModel.AppId, userInfo.Id);
                    if (saveMoneyUser == null || saveMoneyUser.AccountMoney < money)
                    {
                        buyMode buyMode = buyModeList.Where(x => x.buyModeId == (int)miniAppBuyMode.储值支付).FirstOrDefault();
                        buyMode.buyModeStr = "储值支付(余额不足)";
                        buyMode.State = 0;
                        buyMode.DefaultSort -= 1;
                    }
                    else
                    {
                        buyMode buyMode = buyModeList.Where(x => x.buyModeId == (int)miniAppBuyMode.储值支付).FirstOrDefault();
                        buyMode.DefaultSort += 1;
                    }
                }
            }

            return Json(new { buyModeList = buyModeList.OrderByDescending(x => x.DefaultSort) });
        }

        public class buyMode
        {
            public int buyModeId { get; set; }

            public string buyModeStr { get; set; }

            //是否可选择 0可选/1不可选
            public int State { get; set; }

            //排序
            public int DefaultSort { get; set; } = 0;

        }


    }
}