using Api.MiniApp.Models;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.User;
using Entity.MiniApp.Weixin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Utility.AliOss;

namespace Api.MiniApp.Controllers
{
    [ExceptionLog]
    public class apiSinglePageController : InheritController
    {
        private readonly static object _couponLock = new object();
        
        
        public apiSinglePageController()
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
            var loginUser = C_UserInfoBLL.SingleModel.GetModelFromCacheByUnionid(unionid);

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
            var loginUser = C_UserInfoBLL.SingleModel.GetModelFromCache(openId);

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
            var JsonResult = new DecryptUserInfo().GetApiJsonStringnew(code, appid, appsr);
            var session = JsonConvert.DeserializeObject<UserSession>(JsonResult);
            session.code = code;
            session.vector = iv;
            session.enData = data;
            session.signature = signature;
            if (!session.verify())
            {
                return CheckUserLoginNoappsr(code, iv, data, signature, appid);
                //return Json(new { result = false, msg = "获取Session_key异常，appsr=" + appsr, errcode = -1, Oject = UserSession }, JsonRequestBehavior.AllowGet);
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
            var SessionSav = Session[sessionid];
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
            //var mediaId = Guid.NewGuid().ToString();
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
                var aliTempImgFolder = AliOSSHelper.GetOssImgKey(outext, false, out aliTempImgKey);
                var putResult = AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, imgByteArray, 1, "." + outext);
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
                var thumpath = aliTempImgKey;
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
                var imgId = 0;
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

        #region 小程序同城信息
        public ActionResult GetImg(string appid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(appid))
                {
                    return Json(new { data = "", isok = 0, msg = "模块appId不能为空！" }, JsonRequestBehavior.AllowGet);
                }

                var model = ConfParamBLL.SingleModel.GetModelByappid(appid);
                return Json(new { data = model, isok = 1 }, JsonRequestBehavior.AllowGet);
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
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if(umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            SinglePageConfig data = SinglePageConfigBLL.SingleModel.GetModelByRid(umodel.Id);
            if(data == null)
            {
                return Json(new { isok = -1, msg = "没有数据"+ umodel.AppId+",id"+umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }

            List<C_Attachment> imglist = C_AttachmentBLL.SingleModel.GetListByCache(data.Id, (int)AttachmentItemType.小程序单页轮播图);
            var dataimgs = imglist.Select(s => s.filepath);

            return Json(new { isok = 1, msg = "成功", data = data, dataimgs= dataimgs }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取个人发布的预约信息
        /// </summary>
        /// <param name="unionid"></param>
        /// <param name="storeid">店铺ID</param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetUserPublicData(string appid,string openid,int pageindex=1,int pagesize=10)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", data = "" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(openid))
            {
                return Json(new { isok = -1, msg = "openid不能为空", data = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", data = "" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelFromCache(openid);
            if(userinfo==null)
            {
                return Json(new { isok = -1, msg = "请先登陆", data = "" }, JsonRequestBehavior.AllowGet);
            }

            List<SinglePage> data = SinglePageBLL.SingleModel.GetListByUserIdandRid(userinfo.Id,umodel.Id,pageindex,pagesize);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据", data = "" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = 1, msg = "成功", data = data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  添加预约信息
        /// </summary>
        /// <param name="unionid"></param>
        /// <param name="storeid">店铺ID</param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddPublicData(string appid, string openid,string datajson)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空"}, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(openid))
            {
                return Json(new { isok = -1, msg = "openid不能为空"}, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(datajson))
            {
                return Json(new { isok = -1, msg = "datajson为空"}, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权"}, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelFromCache(openid);
            if (userinfo == null)
            {
                return Json(new { isok = -1, msg = "请先登陆", data = "" }, JsonRequestBehavior.AllowGet);
            }

            SinglePage model = JsonConvert.DeserializeObject< SinglePage>(datajson);
            model.RelationId = umodel.Id;
            model.UserId = userinfo.Id;
            model.AddTime = DateTime.Now;

            var result = SinglePageBLL.SingleModel.Add(model);
            if(Convert.ToInt32(result)>0)
            {
                return Json(new { isok = 1, msg = "预约成功"}, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = -1, msg = "预约失败" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}