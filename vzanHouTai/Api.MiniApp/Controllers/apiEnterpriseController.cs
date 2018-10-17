using Api.MiniApp.Filters;
using Api.MiniApp.Models;
using BLL.MiniApp;
using BLL.MiniApp.Alading;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Home;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using BLL.MiniApp.User;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Alading;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Home;
using Entity.MiniApp.Model;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using Entity.MiniApp.ViewModel;
using Entity.MiniApp.Weixin;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.AliOss;
using Utility.IO;
using ent = Entity.MiniApp.Ent;

namespace Api.MiniApp.Controllers
{
    public class apiEnterpriseController : InheritController
    {
    }

    public class apiMiappEnterpriseController : apiEnterpriseController
    {
        public readonly static object _couponLock = new object();
        public static readonly ConcurrentDictionary<int, object> lockObjectDict_Order = new ConcurrentDictionary<int, object>();

        /// <summary>
        /// 产品|产品列表组件中图片的显示格式
        /// {类型,宽和高}
        /// </summary>
        public readonly Dictionary<string, int> goodImgFormatDic = new Dictionary<string, int>()
        {
            { "big",600},
            { "small",300},
            { "normal",300},
            { "scroll",200}
        };

        /// <summary>
        /// 资讯组件中图片的显示格式
        /// {类型,[宽,高]}
        /// </summary>
        public readonly Dictionary<int, int[]> newsImgFormatDic = new Dictionary<int, int[]>()
        {
            { 2,new int[]{210,150 } },//单行图片列表
            { 3,new int[]{150,150 } },//单行圆角图片列表
            { 4,new int[]{710,360 } },//大图文列表
            { 5,new int[]{200,200 } },//小图文列表
        };

        /// <summary>
        /// 正常图片格式
        /// 包括：
        /// 视频组件的封面
        /// 图片导航组件
        /// 新闻资讯的图片以及轮播图
        /// 产品详情的图片以及轮播图
        /// </summary>
        public readonly Dictionary<string, int[]> imgFormatDic = new Dictionary<string, int[]>()
        {
            { "normal",new int[]{750,750 }}
        };

        public apiMiappEnterpriseController()
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
            UserSession UserSession = JsonConvert.DeserializeObject<UserSession>(JsonResult);
            UserSession.code = code;
            UserSession.vector = iv;
            UserSession.enData = data;
            UserSession.signature = signature;
            if (!UserSession.verify())
            {
                return CheckUserLoginNoappsr(code, iv, data, signature, appid);
                //return Json(new { result = false, msg = "获取Session_key异常，appsr=" + appsr, errcode = -1, Oject = UserSession }, JsonRequestBehavior.AllowGet);
            }
            //AES解密，委托参数session_key和初始向量
            UserSession.deData = AESDecrypt.Decrypt(UserSession.enData, UserSession.session_key, UserSession.vector);
            C_ApiUserInfo userInfo = JsonConvert.DeserializeObject<C_ApiUserInfo>(UserSession.deData);
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

        #endregion 登陆状态授权保存

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
                    return Json(new { result = false, msg = "图片上传失败！图片同步到Ali失败！" }, JsonRequestBehavior.AllowGet);
                }

                string thumpath = aliTempImgKey;

                Attachment model = new Attachment()
                {
                    postfix = "." + outext,
                    filepath = aliTempImgKey,
                    thumbnail = thumpath
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

            if (C_AttachmentBLL.SingleModel.Delete(catt.id) > 0)
            {
                C_AttachmentBLL.SingleModel.RemoveRedis(catt.itemId, catt.itemType);//清除缓存
                return Json(new BaseResult { errcode = 1, result = true, msg = "删除成功" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new BaseResult { errcode = 1, result = false, msg = "系统繁忙db_err" }, JsonRequestBehavior.AllowGet);
        }

        #endregion 图片上传、删除

        public ActionResult req_services_msg(string signature, string timestamp, string nonce, string echostr)
        {
            string hash = "";
            int ret = 0;
            ret = GenarateSinature("pengxuncn", timestamp, nonce, ref hash);
            if (ret != 0)
                return Content(ret.ToString());

            if (hash == signature)
                return Content(echostr);
            else
            {
                return Content("-40001");
            }
        }

        #region 验证签名

        public class DictionarySort : System.Collections.IComparer
        {
            public int Compare(object oLeft, object oRight)
            {
                string sLeft = oLeft as string;
                string sRight = oRight as string;
                int iLeftLength = sLeft.Length;
                int iRightLength = sRight.Length;
                int index = 0;
                while (index < iLeftLength && index < iRightLength)
                {
                    if (sLeft[index] < sRight[index])
                        return -1;
                    else if (sLeft[index] > sRight[index])
                        return 1;
                    else
                        index++;
                }
                return iLeftLength - iRightLength;
            }
        }

        private enum WXBizMsgCryptErrorCode
        {
            WXBizMsgCrypt_OK = 0,
            WXBizMsgCrypt_ValidateSignature_Error = -40001,
            WXBizMsgCrypt_ParseXml_Error = -40002,
            WXBizMsgCrypt_ComputeSignature_Error = -40003,
            WXBizMsgCrypt_IllegalAesKey = -40004,
            WXBizMsgCrypt_ValidateAppid_Error = -40005,
            WXBizMsgCrypt_EncryptAES_Error = -40006,
            WXBizMsgCrypt_DecryptAES_Error = -40007,
            WXBizMsgCrypt_IllegalBuffer = -40008,
            WXBizMsgCrypt_EncodeBase64_Error = -40009,
            WXBizMsgCrypt_DecodeBase64_Error = -40010
        };

        #endregion 验证签名

        public static int GenarateSinature(string sToken, string sTimeStamp, string sNonce, ref string sMsgSignature)
        {
            ArrayList AL = new ArrayList();
            AL.Add(sToken);
            AL.Add(sTimeStamp);
            AL.Add(sNonce);
            AL.Sort(new DictionarySort());
            string raw = "";
            for (int i = 0; i < AL.Count; ++i)
            {
                raw += AL[i];
            }

            SHA1 sha;
            ASCIIEncoding enc;
            string hash = "";
            try
            {
                sha = new SHA1CryptoServiceProvider();
                enc = new ASCIIEncoding();
                byte[] dataToHash = enc.GetBytes(raw);
                byte[] dataHashed = sha.ComputeHash(dataToHash);
                hash = BitConverter.ToString(dataHashed).Replace("-", "");
                hash = hash.ToLower();
            }
            catch (Exception)
            {
                return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_ComputeSignature_Error;
            }
            sMsgSignature = hash;
            return 0;
        }

        #region 产品预约接口

        /// <summary>
        /// 产品预约表单提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveSubscribeForm(int aid=0,int uid=0,int formId=0,string formdatajson="",string remark="",int buyMode = (int)miniAppBuyMode.微信支付)
        {
            if (aid <= 0)
            {
                return Json(new { isok = false, msg = $"aid错误！aid={aid}" }, JsonRequestBehavior.AllowGet);
            }
            if (uid <= 0)
            {
                return Json(new { isok = false, msg = $"uid错误!uid={uid}" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(formdatajson))
            {
                return Json(new { isok = false, msg = "请填写预约信息" }, JsonRequestBehavior.AllowGet);
            }
            DateTime formtime = DateTime.Now;
            try
            {
                EntFormRemark Remarkmodel = JsonConvert.DeserializeObject<EntFormRemark>(remark);
                EntGoods goods = EntGoodsBLL.SingleModel.GetModelById(Remarkmodel.goods.id, 1);
                if (goods == null)
                {
                    return Json(new { isok = false, msg = "预约产品不存在" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(GetType(), ex);
                return Json(new { isok = false, msg = "产品信息异常" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModelByAId(aid);
            if (store == null)
            {
                return Json(new { isok = false, msg = "配置信息错误store is null" }, JsonRequestBehavior.AllowGet);
            }
            EntUserForm form = null;
            if (formId <= 0)
            {
                int citymorderId = 0;
                form = new EntUserForm();
                try
                {
                    store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
                }
                catch
                {
                    store.funJoinModel = new StoreConfigModel();
                }
                //预约付费
                if (store.funJoinModel.OpenYuyuePay)
                {
                    form.isPay = 1;
                    string msg = string.Empty;
                    EntGoodsOrder order = EntUserFormBLL.SingleModel.CreateOrder(aid,uid, store, remark, buyMode,out msg);
                    if (order == null)
                    {
                        return Json(new { isok = false, msg = $"预约失败{msg}" }, JsonRequestBehavior.AllowGet);
                    }
                    form.orderId = order.Id;
                    citymorderId = order.OrderId;
                }
                form.formtime = form.addtime = DateTime.Now;
                form.type = 1;
                form.aid = aid;
                form.uid = uid;
                form.formdatajson = formdatajson;
                form.remark = remark;
                
                
                int id = Convert.ToInt32(EntUserFormBLL.SingleModel.Add(form));
                if (id > 0)
                {
                    TemplateMsg_Gzh.ReserveInformTemplateMessageForEnt(form);

                    //新订单电脑语音提示
                    Utils.RemoveIsHaveNewOrder(aid, 0, "1_" + (int)EntGoodsType.预约商品);
                    SystemUpdateMessageBLL.SingleModel.SendSubscribeMessage(id, form.aid, form.formdatajson);
                    return Json(new { isok = true, msg = $"预约成功{id}",obj=new { citymorderId} }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = false, msg = "预约失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                form = EntUserFormBLL.SingleModel.GetModel(formId);
                if (form == null)
                {
                    return Json(new { isok = false, msg = "预约信息不存在" });
                }
                form.addtime = DateTime.Now;
                form.formdatajson = formdatajson;
                bool isok = EntUserFormBLL.SingleModel.Update(form, "addtime,formdatajson");
                string msg = isok ? "修改成功" : "修改失败";
                return Json(new { isok, msg });
            }
        }

        /// <summary>
        /// 查看预约信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSubscribeFormDetail()
        {
            int aid = Context.GetRequestInt("aid", 0);
            if (aid <= 0)
            {
                return Json(new { isok = false, msg = $"参数错误！{aid}" }, JsonRequestBehavior.AllowGet);
            }
            int uid = Context.GetRequestInt("uid", 0);
            if (uid <= 0)
            {
                return Json(new { isok = false, msg = $"参数错误" }, JsonRequestBehavior.AllowGet);
            }
            int pageindex = Context.GetRequestInt("pageindex", 1);
            int pagesize = Context.GetRequestInt("pagesize", 10);
            List<EntUserForm> formlist = EntUserFormBLL.SingleModel.GetListByAidUid(aid, uid, 1, pagesize, pageindex, "id desc");
            //List<EntUserForm> formlist = EntUserFormBLL.SingleModel.GetList($"aid={aid} and uid={uid} and type=1 and state>0", pagesize, pageindex, "*", "id desc");
            if (formlist != null && formlist.Count > 0)
            {
                foreach (EntUserForm item in formlist)
                {
                    item.formdatajson = item.formdatajson.Replace("{", "").Replace("}", "").Replace("\"", "");
                    item.formremark = JsonConvert.DeserializeObject<EntFormRemark>(item.remark);
                    if (item.isPay > 0 && item.orderId > 0)
                    {
                        EntGoodsOrder order = EntGoodsOrderBLL.SingleModel.GetModel(item.orderId);
                        if (order != null&& order.State== (int)MiniAppEntOrderState.交易成功)
                        {
                            item.money = order.GoodsMoney;
                        }
                    }
                }
            }
            return Json(new { isok = true, list = formlist }, JsonRequestBehavior.AllowGet);
        }

        #endregion 产品预约接口

        #region 企业版小程序接口

        [AcceptVerbs("get", "post")]
        public ActionResult GetPageSetting()
        {
            int aid = Context.GetRequestInt("aid", 0);
            if (aid == 0)
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            //只有专业版才有店铺资料,故若搜索不到就给默认值
            Store store = StoreBLL.SingleModel.GetModelByRid(aid) ?? new Store();
            try
            {
                store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
            }
            catch (Exception)
            {
                store.funJoinModel = new StoreConfigModel();
            }
            //读取用户小程序资料
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModel(aid);
            OpenAuthorizerConfig config = new OpenAuthorizerConfig();
            if (xcxRelation != null)
            {
                config = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxRelation.AppId);
            }

            EntSetting setModel = EntSettingBLL.SingleModel.GetModel(aid);
            if (setModel == null)
            {
                return Json(new { isok = false, msg = "小程序没有设置页面" }, JsonRequestBehavior.AllowGet);
            }

            #region 此逻辑为将产品预约页面放在数组最后一位返回给前端

            List<EntPage> pageModels = JsonConvert.DeserializeObject<List<EntPage>>(setModel.pages);
            if (pageModels != null && pageModels.Count > 0)
            {
                EntPage page = pageModels.Where(p => p.def_name == "产品预约").FirstOrDefault();
                if (page != null)
                {
                    pageModels.Remove(page);
                    pageModels.Add(page);
                }
                setModel.pages = JsonConvert.SerializeObject(pageModels);
            }

            #endregion 此逻辑为将产品预约页面放在数组最后一位返回给前端

            if (setModel.syncmainsite == 1)
            {
                setModel = EntSettingBLL.SingleModel.GetModel(WebSiteConfig.TemplateAid);
            }
            setModel.pages = EntSettingBLL.SingleModel.ResizeComsImage(setModel.pages);

            return new JsonResult()
            {
                Data = new { isok = true, msg = setModel, extraConfig = store?.funJoinModel, appConfig = config },
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetPageSettingUpdateTime()
        {
            int aid = Context.GetRequestInt("aid", 0);
            if (aid == 0)
            {
                return Json(new { isok = false, msg = "非法请求" });
            }
            EntSetting setModel = EntSettingBLL.SingleModel.GetModel(aid);
            if (setModel == null)
            {
                return Json(new { isok = false, msg = "小程序没有设置页面" });
            }
            if (setModel.syncmainsite == 1)
            {
                setModel = EntSettingBLL.SingleModel.GetModel(WebSiteConfig.TemplateAid);
            }
            return Json(new { isok = true, msg = setModel.updatetime });
        }

        /// <summary>
        /// 通过appid获取小程序的id
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult Getaid()
        {
            string appid = Context.GetRequest("appid", string.Empty);
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "参数无效" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation model = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (model == null)
            {
                return Json(new { isok = false, msg = "app不存在" }, JsonRequestBehavior.AllowGet);
            }

            AlaDingAppInfo aldModel = AlaDingAppInfoBLL.SingleModel.GetModel($"appid='{appid}'");

            return Json(new { isok = true, msg = model.Id, appkey = aldModel?.AppKey }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交表单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveUserForm(EntUserForm model)
        {
            if (model.aid <= 0 || model.uid <= 0)
            {
                return Json(new { isok = false, msg = "非法请求" });
            }

            int newid = Convert.ToInt32(EntUserFormBLL.SingleModel.Add(model));
            TemplateMsg_Gzh.ReserveInformTemplateMessageForEnt(model);

            return Json(new { isok = true, msg = "提交成功" });
        }

        /// <summary>
        /// 产品详情
        /// </summary>
        /// <returns></returns>
       // [OutputCache(Duration = 10, Location = System.Web.UI.OutputCacheLocation.Any, VaryByParam = "*")]
        public ActionResult GetGoodInfo()
        {
            int pid = Context.GetRequestInt("pid", 0);

            //接口版本
            int version = Context.GetRequestInt("version", 0);
            if (pid == 0)
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }

            ent.EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(pid);
            if (goodModel == null || goodModel.state == 0)
            {
                return Json(new { isok = false, msg = "产品不存在或已删除！" }, JsonRequestBehavior.AllowGet);
            }
            //获取参数组合
            goodModel.IndutypeList = EntIndutypesBLL.SingleModel.GetIndutypeList(goodModel.aid, goodModel.exttypes);

            //获取该产品对应的可以参团的数据
            List<EntGroupSponsor> GroupSponsorList = new List<EntGroupSponsor>();
            //判断是否是拼团产品
            if (goodModel.goodtype == (int)EntGoodsType.拼团产品)
            {
                goodModel.EntGroups = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(goodModel.id, goodModel.aid);
                if (goodModel.EntGroups == null)
                {
                    return Json(new { isok = false, msg = "该拼团已不存在！" }, JsonRequestBehavior.AllowGet);
                }
                int groupnum = 0;
                goodModel.EntGroups.GroupUserList = EntGroupSponsorBLL.SingleModel.GetGoupsUserImgs(goodModel.EntGroups.Id, ref groupnum, (int)TmpType.小程序专业模板, goodModel.id);
                goodModel.EntGroups.GroupsNum = groupnum + goodModel.EntGroups.InitSaleCount;//加上初始化销售量
                goodModel.EntGroups.GroupSponsorList = EntGroupSponsorBLL.SingleModel.GetHaveSuccessGroup(goodModel.EntGroups.Id, 10, goodModel.id);
            }

            if (!string.IsNullOrEmpty(goodModel.plabels))
            {
                //goodModel.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT group_concat(name order by sort desc) from entgoodlabel where id in ({goodModel.plabels})").ToString();
                goodModel.plabelstr = EntGoodLabelBLL.SingleModel.GetEntGoodsLabelStr(goodModel.plabels);
                goodModel.plabelstr_array = goodModel.plabelstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            #region 会员折扣显示

            int levelid = Context.GetRequestInt("levelid", 0);
            VipLevel level = VipLevelBLL.SingleModel.GetModelById(levelid, -1);
            //VipLevel level = _miniappVipLevelBll.GetModel($"id={levelid} and state>=0");
            goodModel = EntGoodsBLL.SingleModel.GetDiscountPrice(level: level, goodModel: goodModel);

            #endregion 会员折扣显示

            goodModel.img_fmt = ImgHelper.ResizeImg(goodModel.img, imgFormatDic["normal"][0], imgFormatDic["normal"][1]);
            //version2改动:slideimgs增加动态裁剪
            if (!string.IsNullOrEmpty(goodModel.slideimgs) && version > 0)
            {
                goodModel.slideimgs_fmt = string.Join("|", goodModel.slideimgs.SplitStr(",").Select(p => ImgHelper.ResizeImg(p, imgFormatDic["normal"][0], imgFormatDic["normal"][1])).ToArray());
            }
            if (!string.IsNullOrEmpty(goodModel.img))
            {
                goodModel.img = goodModel.img.Replace("http://vzan-img.oss-cn-hangzhou.aliyuncs.com", "https://i.vzan.cc/");
            }

            //goodModel.description= ImgHelper.ResizeContentImg(goodModel.description, false, "lfit", 100, 750);
            string appId = Context.GetRequest("appId", string.Empty);
            List<EntGoods> hotGoods = new List<EntGoods>();
            if (!string.IsNullOrEmpty(appId))
            {
                XcxAppAccountRelation xcxModel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
                if (xcxModel == null)
                {
                    return Json(new { isok = true, msg = goodModel, hotGoods, tips = "小程序不存在" }, JsonRequestBehavior.AllowGet);
                }

                hotGoods = EntGoodsBLL.SingleModel.GetHotGoods(xcxModel.Id, level);
            }

            return Json(new { isok = true, msg = goodModel, hotGoods }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="typeid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult GetGoodsList(int aid, string typeid = "", int pageindex = 1, int pagesize = 10, int userId = 0, int isFirstType = -1)
        {
            string d = "";
            string search = Context.GetRequest("search", "");
            string pricesort = Context.GetRequest("pricesort", "");
            string saleCountSort = Context.GetRequest("saleCountSort", "");
            string exttypes = Context.GetRequest("exttypes", "");
            int goodtype = Context.GetRequestInt("goodtype", (int)EntGoodsType.普通产品);
            //产品图片的裁剪模式
            string goodShowType = Context.GetRequest("goodShowType", "");

            string entGoodTypeIds = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(typeid))
                {
                    typeid = EncodeHelper.ReplaceSqlKey(typeid);
                    typeid = Server.UrlDecode(typeid);
                }
                List<EntGoods> goodslist = EntGoodsBLL.SingleModel.GetListGoods(aid, goodtype, search, typeid, exttypes, pricesort, pagesize, pageindex, isFirstType, saleCountSort);
                if (goodslist != null)
                {
                    //判断是否是拼团产品
                    List<EntGroupsRelation> entgrouplist = new List<EntGroupsRelation>();
                    List<EntGroupSponsor> entgroupsponsorlist = new List<EntGroupSponsor>();
                    string entgoodids = string.Join(",", goodslist.Where(w => w.goodtype == (int)EntGoodsType.拼团产品)?.Select(s => s.id));
                    if (!string.IsNullOrEmpty(entgoodids))
                    {
                        entgrouplist = EntGroupsRelationBLL.SingleModel.GetListByGroupGoodType(aid, entgoodids);
                        string sponsidstr = string.Join(",", entgrouplist.Select(s => s.Id).Distinct());
                        entgroupsponsorlist = EntGroupSponsorBLL.SingleModel.GetListByGoodRids(sponsidstr);
                    }

                    goodslist.ForEach((Action<EntGoods>)(goodModel =>
                    {
                        goodModel.isSubscribe = EntUserFormBLL.SingleModel.IsSubscribe(goodModel.id, userId);
                        //拼团产品
                        if (entgrouplist != null && entgrouplist.Count > 0)
                        {
                            EntGroupsRelation entgroup = entgrouplist.Where(w => goodModel.id == w.EntGoodsId).FirstOrDefault();
                            if (entgroup != null)
                            {
                                string sponsidstr = string.Join(",", entgroupsponsorlist?.Select(s => s.Id).Distinct());
                                //已团数量
                                if (!string.IsNullOrEmpty(sponsidstr))
                                {
                                    entgroup.GroupsNum = EntGoodsOrderBLL.SingleModel.GetGroupCount(sponsidstr, 0);
                                }
                                entgroup.GroupsNum += entgroup.InitSaleCount;//加上初始化销售量
                                goodModel.EntGroups = entgroup;
                            }
                        }

                        if (!string.IsNullOrEmpty(goodModel.ptypes))
                        {
                            goodModel.ptypestr = EntGoodTypeBLL.SingleModel.GetEntGoodTypeName(goodModel.ptypes);
                        }

                        if (!string.IsNullOrEmpty(goodModel.plabels))
                        {
                            goodModel.plabelstr = EntGoodLabelBLL.SingleModel.GetEntGoodsLabel(goodModel.plabels);
                            goodModel.plabelstr_array = goodModel.plabelstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }

                        int levelid = Context.GetRequestInt("levelid", 0);
                        VipLevel level = VipLevelBLL.SingleModel.GetModelById(levelid);
                        goodModel = EntGoodsBLL.SingleModel.GetDiscountPrice(level: level, goodModel: goodModel);

                        if (!string.IsNullOrEmpty(goodShowType))
                        {
                            goodModel.img_fmt = ImgHelper.ResizeImg(goodModel.img, goodImgFormatDic[goodShowType], goodImgFormatDic[goodShowType]);
                        }
                        else
                        {
                            goodModel.img_fmt = ImgHelper.ResizeImg(goodModel.img, 750, 750);
                        }
                    }));
                }
                var postdata = new
                {
                    goodslist = goodslist.Select(goods => new
                    {
                        goods.id,
                        goods.img,
                        goods.img_fmt,
                        goods.name,
                        goods.plabelstr_array,
                        goods.priceStr,
                        goods.discountPricestr,
                        goods.discount,
                        goods.unit,
                        goods.virtualSalesCount,
                        goods.salesCount,
                        goods.price,
                        goods.showprice,
                        goods.originalPrice,
                        goods.sort
                    }),
                };
                ConfigurableJsonResult jsonResult = new ConfigurableJsonResult();
                jsonResult.Data = new { isok = 1, msg = "成功", postdata = postdata, entGoodTypeIds = entGoodTypeIds };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = "异常" + ex.Message + "  " + d }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取自定义分类
        /// </summary>
        /// <returns></returns>
        public ActionResult GetExtTypes()
        {
            int aid = Context.GetRequestInt("aid", 0);
            if (aid == 0)
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation accountrelationModel = _xcxAppAccountRelationBLL.GetModel(aid);
            if (accountrelationModel == null)
            {
                return Json(new { isok = false, msg = "小程序不存在" }, JsonRequestBehavior.AllowGet);
            }

            List<EntIndutypes> types = EntIndutypesBLL.SingleModel.GetList($"state=1 and aid={aid} and industr='{accountrelationModel.Industr}' ");

            return Json(new { isok = true, msg = types }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据ids查询产品
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult GetGoodsByids()
        {
            string ids = Context.GetRequest("ids", string.Empty);
            int storeId = Context.GetRequestInt("storeId", 0);
            int homeId = Context.GetRequestInt("homeId", 0);
            int userId = Context.GetRequestInt("userId", 0);
            string goodShowType = Context.GetRequest("goodShowType", string.Empty);

            if (string.IsNullOrEmpty(ids))
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string wherestr = $" find_in_set(id,@ids)>0 and state=1 and tag=1";
            if (homeId > 0 && storeId > 0)
                wherestr = $" find_in_set(id,@ids)>0 and state=1 ";//表示门店的 不受总店产品下架影响

            parameters.Add(new MySqlParameter("@ids", ids));
            List<ent.EntGoods> goodslist = EntGoodsBLL.SingleModel.GetListByParam(wherestr, parameters.ToArray(), 2000, 1, "*", " find_in_set( id,@ids)");
            if (goodslist != null && goodslist.Count > 0)
            {
                //判断是否是拼团产品
                List<EntGroupsRelation> entgrouplist = new List<EntGroupsRelation>();
                List<EntGroupSponsor> entgroupsponsorlist = new List<EntGroupSponsor>();
                string entgoodids = string.Join(",", goodslist.Where(w => w.goodtype == (int)EntGoodsType.拼团产品)?.Select(s => s.id));
                if (!string.IsNullOrEmpty(entgoodids))
                {
                    entgrouplist = EntGroupsRelationBLL.SingleModel.GetListByGroupGoodType(goodslist[0].aid, entgoodids);
                    string sponsidstr = string.Join(",", entgrouplist.Select(s => s.Id).Distinct());
                    entgroupsponsorlist = EntGroupSponsorBLL.SingleModel.GetList($"entgoodrid in ({sponsidstr})");
                }

                List<EntGoods> listSubGood = new List<EntGoods>();

                goodslist.ForEach((Action<EntGoods>)(x =>
                {
                    x.isSubscribe = EntUserFormBLL.SingleModel.IsSubscribe(x.id, userId);
                    //拼团产品
                    if (entgrouplist != null && entgrouplist.Count > 0)
                    {
                        EntGroupsRelation entgroup = entgrouplist.Where(w => x.id == w.EntGoodsId).FirstOrDefault();
                        if (entgroup != null)
                        {
                            string sponsidstr = string.Join(",", entgroupsponsorlist?.Select(s => s.Id).Distinct());
                            //已团数量
                            if (!string.IsNullOrEmpty(sponsidstr))
                            {
                                entgroup.GroupsNum = EntGoodsOrderBLL.SingleModel.GetCount($"ordertype = 3 and state not in ({(int)MiniAppEntOrderState.已取消},{(int)MiniAppEntOrderState.待付款}) and groupid in ({sponsidstr})");
                            }
                            entgroup.GroupsNum += entgroup.InitSaleCount;//加上初始化销售量
                            x.EntGroups = entgroup;
                        }
                    }

                    if (!string.IsNullOrEmpty(x.ptypes))
                    {
                        string sql = $"SELECT GROUP_CONCAT(`name`) from entgoodtype where FIND_IN_SET(id,@ptypes)";
                        x.ptypestr = DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                            CommandType.Text, sql,
                            new MySqlParameter[] { new MySqlParameter("@ptypes", x.ptypes) }).ToString();
                    }
                    if (!string.IsNullOrEmpty(x.plabels))
                    {
                        x.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT group_concat(name order by sort desc) from entgoodlabel where id in ({x.plabels})").ToString();
                        if (!x.plabelstr.IsNullOrEmpty())
                        {
                            x.plabelstr_array = x.plabelstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }
                    }

                    if (storeId > 0 && homeId > 0)
                    {
                        //表示门店 需要从视图里查找库存与规格 前台需要传入homeId 跟storeId
                        string strWhere = $"  storeId={storeId} and aid={x.aid} and pid={x.id} and subState=1 and SubTag=1";
                        string selSql = $"select * from substoregoodsview where {strWhere}";
                        DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, selSql, parameters.ToArray()).Tables[0];
                        List<SubStoreGoodsView> listSub = DataHelper.ConvertDataTableToList<SubStoreGoodsView>(dt);
                        if (listSub != null && listSub.Count > 0)
                        {
                            x.specificationdetail = listSub[0].SubSpecificationdetail;
                            x.stock = listSub[0].stock;
                            listSubGood.Add(x);
                        }
                    }

                    #region 会员打折

                    int levelid = Context.GetRequestInt("levelid", 0);
                    VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");
                    if (goodslist != null && goodslist.Count > 0)
                    {
                        goodslist.ForEach(g =>
                        {
                            VipLevelBLL.SingleModel.CalculateVipGoodsPrice(g, level);
                        });
                    }

                    #endregion 会员打折

                    if (!string.IsNullOrEmpty(goodShowType))
                    {
                        x.img_fmt = ImgHelper.ResizeImg(x.img, goodImgFormatDic[goodShowType], goodImgFormatDic[goodShowType]);
                    }
                    else
                    {
                        x.img_fmt = ImgHelper.ResizeImg(x.img, 750, 750);
                    }
                }));

                if (storeId > 0 && homeId > 0 && listSubGood.Count > 0)
                {
                    //门店产品
                    listSubGood.ForEach(x =>
                    {
                        #region 会员打折

                        int levelid = Context.GetRequestInt("levelid", 0);
                        VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");
                        if (listSubGood != null && listSubGood.Count > 0)
                        {
                            listSubGood.ForEach(g =>
                            {
                                VipLevelBLL.SingleModel.CalculateVipGoodsPrice(g, level);
                            });
                        }

                        #endregion 会员打折
                    });
                    return Json(new { isok = true, msg = listSubGood }, JsonRequestBehavior.AllowGet);
                }
            }

            object goodsList = goodslist.Select(g => new
            {
                id = g.id,
                img = g.img,
                img_fmt = g.img_fmt,
                name = g.name,
                plabelstr_array = g.plabelstr_array,
                showprice = g.showprice,
                priceStr = g.priceStr,
                price = g.price,
                discountPricestr = g.discountPricestr,
                discount = g.discount,
                unit = g.unit,
                virtualSalesCount = g.virtualSalesCount,
                salesCount = g.salesCount,
                GASDetailList = g.GASDetailList,
                pickspecification = g.pickspecification,
                stockLimit = g.stockLimit,
                stock = g.stock,
                originalPrice = g.originalPrice
            });
            //return Json(new { isok = true, msg = goodslist }, JsonRequestBehavior.AllowGet);
            return Json(new { isok = true, msg = goodsList }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取运费信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="goodCartIds">商品购物车ID</param>
        /// <param name="province">省份</param>
        /// <param name="city">城市</param>
        /// <param name="flashItemId">秒杀商品ID</param>
        /// <param name="isgroup">是否发起拼团</param>
        /// <param name="groupid">拼团ID</param>
        /// <param name="discountType">优惠类型 默认为0 表示可以使用会员折扣 1表示不能使用会员折扣</param>
        /// <param name="couponlogid">用户领取的优惠券记录ID</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetFreightFee(string appId = null, string openId = null, string goodCartIds = null, string province = null, string city = null,
            int? flashItemId = null, int? isgroup = null, int? groupid = null, int? discountType = 0, int? couponlogid = null)
        {
            #region 参数校验
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(goodCartIds) || string.IsNullOrWhiteSpace(province) || string.IsNullOrWhiteSpace(city))
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            if (isgroup > 0 && groupid == 0)
            {
                return Json(new { isok = -1, msg = "拼团参数错误" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation model = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (model == null)
            {
                return Json(new { isok = false, msg = "app不存在" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "app不存在" }, JsonRequestBehavior.AllowGet);
            }

            List<EntGoodsCart> goodCarts = EntGoodsCartBLL.SingleModel.GetMyCartById(goodCartIds);
            if (goodCarts == null)
            {
                return Json(new { isok = false, msg = "购物车数据为空" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            int originalPrice = goodCarts.Sum(x => x.Price * x.Count);//优惠前商品总价

            int groupDiscount = 0;//团长减价
            #region 拼团优惠
            EntGroupsRelation groupmodel = new EntGroupsRelation() { RId = model.Id };
            if (isgroup > 0 || groupid > 0)
            {
                //判断是否是拼团，如果是拼团则将产品价格改成拼团价，获取团长优惠价
                string groupmsg = CommandEntGroup(isgroup.Value, groupid.Value, userInfo.Id, 0, goodCarts[0].FoodGoodsId, ref groupDiscount, ref groupmodel, goodCarts[0].Count);
                if (!string.IsNullOrEmpty(groupmsg))
                {
                    return Json(new { isok = false, msg = groupmsg }, JsonRequestBehavior.AllowGet); ;
                }
            }
            #endregion

            int flashItemDiscount = 0;//秒杀商品打折
            #region 秒杀商品优惠（只能和满包邮叠加）
            //秒杀商品为立即购买（下单固定为1种商品）
            FlashDealPayInfo flashPay = null;
            if (goodCarts?.Count == 1 && flashItemId > 0 && groupDiscount == 0)
            {
                FlashDealItem flashItem = FlashDealItemBLL.SingleModel.GetModel(flashItemId.Value);
                if (flashItem == null)
                {
                    return Json(new { isok = -1, msg = "秒杀商品不存在或已删除" }, JsonRequestBehavior.AllowGet);
                }
                flashPay = FlashDealBLL.SingleModel.GetFlashDealPayment(flashItem, userInfo.Id);
                if (flashPay != null)
                {
                    //是否允许购买
                    if (!flashPay.IsPay)
                    {
                        return Json(new { isok = -1, msg = flashPay.Info }, JsonRequestBehavior.AllowGet);
                    }
                    //格式化购物车价格
                    originalPrice = FlashDealItemBLL.SingleModel.GetFlashDealPrice(goodCarts.First(), flashItem).Price * goodCarts.First().Count;
                }
            }
            #endregion

            int vipDiscount = 0;//会员打折
            #region 会员打折（不能与秒杀叠加）
            //获取会员信息
            VipRelation vipInfo = flashPay == null ? VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id} and state>=0") : null;
            if (vipInfo != null && discountType == 0)
            {
                VipLevelBLL.SingleModel.CalculateVipGoodsCartPrice(goodCarts, userInfo.Id);
                //优惠后商品总价
                vipDiscount = originalPrice - goodCarts.Sum(x => x.Price * x.Count);
            }
            #endregion 会员打折

            int couponDiscount = 0;//优惠券打折
            #region 使用优惠券（不能与秒杀叠加）
            if (flashPay == null && couponlogid > 0)
            {
                string couponmsg = "";
                //优惠金额
                couponDiscount = CouponLogBLL.SingleModel.GetCouponPrice(couponlogid.Value, goodCarts, ref couponmsg);
                if (!string.IsNullOrEmpty(couponmsg))
                {
                    return Json(new { isok = -1, msg = couponmsg }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                couponDiscount = 0;
                couponlogid = 0;
            }
            #endregion 使用优惠券

            int reducetionDiscount= 0;//满减优惠打折
            #region 满减优惠（不能与"优惠券"和"秒杀"叠加）
            //获取满减优惠活动
            FullReduction fullReduction = null;
            //不能与"优惠券"和"秒杀"叠加
            if (couponlogid <= 0 && flashItemId == 0)
            {
                fullReduction = FullReductionBLL.SingleModel.GetFullReduction(model.Id);
            }
            //是否叠加会员折扣 OR 或没有使用会员折扣。
            if ((fullReduction?.VipDiscount == 0 || vipDiscount == 0) && fullReduction != null)
            {
                //满足满级条件 = 原价 >= 满减阈值 ？ 满级折扣 ： 无折扣
                reducetionDiscount = originalPrice >= fullReduction.OrderMoney && originalPrice > fullReduction.ReducetionMoney ? fullReduction.ReducetionMoney : 0;
            }
            #endregion

            bool canpay = false;//是否配送范围内
            int deliveryFee = 0;//配送费用
            string apiMsg = string.Empty;//接口返回信息
            string deliveryMsg = string.Empty;//运费详情
            #region 运费模板
            StoreConfigModel config = StoreBLL.SingleModel.GetStoreConfig(model.Id);
            DeliveryFeeSumMethond sumMethod;
            if (config.enableDeliveryTemplate && Enum.TryParse(config.deliveryFeeSumMethond.ToString(), out sumMethod))
            {
                //新版运费模板
                DeliveryFeeResult deliveryResult = DeliveryTemplateBLL.SingleModel.GetDeliveryFeeSum(goodCarts, province, city, sumMethod);
                deliveryFee = deliveryResult.Fee;
                deliveryMsg = deliveryResult.Message;
                canpay = deliveryResult.InRange;
                apiMsg = "无优惠";
            }
            else
            {
                //旧版运费模板
                FreightInfo freightInfo = EntFreightTemplateBLL.SingleModel.GetFreightFee(config, goodCartIds, province, city);
                deliveryFee = freightInfo.Fee;
                apiMsg = freightInfo.Msg;
                canpay = freightInfo.IsVaild;
            }
            #endregion

            //优惠后购买价
            int discountPrice = originalPrice - (groupDiscount + couponDiscount + flashItemDiscount + vipDiscount + reducetionDiscount);

            //运费满减包邮优惠
            DeliveryDiscount deliveryDiscount = DeliveryTemplateBLL.SingleModel.GetDiscount(model.Id, discountPrice);
            if(deliveryDiscount.HasDiscount)
            {
                deliveryFee = deliveryDiscount.DiscountPrice;
                deliveryMsg = deliveryDiscount.DiscountInfo;
            }

            object result = new
            {
                fee = deliveryFee, //运费金额
                msg = apiMsg,//接口返回信息
                deliveryMsg, //运费详情
                canpay,//在配送范围内
                groupDiscount,// 团长打折
                couponDiscount,//优惠券打折
                flashItemDiscount,//秒杀打折
                vipDiscount,//会员打折
                reducetionDiscount,//满减打折
                discountPrice,//折后价
                originalPrice,//原价
            };
            return Json(new { isok = true, msg = "获取成功", data = result });
        }

        /// <summary>
        /// 获取砍价运费信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetBargainFreightFee(string appId = null, string openId = null,int? buId = null, string province = null, string city = null)
        {
            if (string.IsNullOrWhiteSpace(appId) || !buId.HasValue || string.IsNullOrWhiteSpace(province) || string.IsNullOrWhiteSpace(city))
            {
                return GetJsonResult(Msg: "非法请求");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if(userInfo == null)
            {
                return GetJsonResult(Msg: "登陆异常");
            }

            BargainUser bargainUser = BargainUserBLL.SingleModel.GetModel($"Id={buId.Value} and UserId={userInfo.Id} and State=0");
            if(bargainUser== null)
            {
                return GetJsonResult(Msg: "数据不存在_bargainUserNull");
            }

            XcxAppAccountRelation model = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (model == null)
            {
                return GetJsonResult(Msg: "app不存在");
            }

            DeliveryConfig config = DeliveryConfigBLL.SingleModel.GetConfig(new Bargain { Id = bargainUser.BId });
            if(config== null || !config.Attr.Enable)
            {
                return GetJsonResult(Msg: "未启用运费模板", code: "405");
            }

            StoreConfigModel storeConfig = StoreBLL.SingleModel.GetStoreConfig(model.Id);
            DeliveryFeeSumMethond sumMethod = DeliveryFeeSumMethond.有赞;
            if (!storeConfig.enableDeliveryTemplate && !Enum.TryParse(storeConfig.deliveryFeeSumMethond.ToString(), out sumMethod))
            {
                return GetJsonResult(Msg: "未启用运费模板", code: "405");
            }

            DeliveryFeeResult deliueryResult = DeliveryTemplateBLL.SingleModel.GetDeliveryFee(bargainUser, province, city, sumMethod);
            object result = new
            {
                fee = deliueryResult.Fee,
                msg = deliueryResult.Fee == 0 ? deliueryResult.Message : "无优惠",
                delierymsg = deliueryResult.Message,
                canpay = deliueryResult.InRange,
            };

            return GetJsonResult(isok:true, Msg: "获取成功", dataObj : result);
        }

        #region 资讯

        /// <summary>
        /// 获取资讯列表
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult GetNewsList()
        {
            int aid = Context.GetRequestInt("aid", 0);
            int typeid = Context.GetRequestInt("typeid", 0);
            string openid = Context.GetRequest("openid", "");
            string appid = Context.GetRequest("appid", "");

            if (aid == 0)
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            string keyMsg = Context.GetRequest("keyMsg", string.Empty);
            int pageindex = Context.GetRequestInt("pageindex", 1);
            int pagesize = Context.GetRequestInt("pagesize", 5);
            //根据这个参数来决定使用哪种尺寸来裁剪资讯大图
            int liststyle = Context.GetRequestInt("liststyle", 0);
            List<EntNews> list = new List<EntNews>();
            string wherestr = $"aid={aid}  and state=1";
            if (typeid > 0)
            {
                wherestr += $" and typeid in({typeid})";
            }
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(keyMsg))
            {
                wherestr += $" and (title like @keyMsg or description like @keyMsg) ";
                parameters.Add(new MySqlParameter("keyMsg", $"%{keyMsg}%"));
            }

            int allcount = 0;

            list = EntNewsBLL.SingleModel.GetListByParam(wherestr,parameters.ToArray(), pagesize, pageindex, "*", " SortNumber desc");
            allcount = EntNewsBLL.SingleModel.GetCount(wherestr, parameters.ToArray());

            //获取用户数据
            C_UserInfo userInfo = null;
            if (!string.IsNullOrWhiteSpace(appid) && !string.IsNullOrWhiteSpace(openid))
            {
                userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            }
            //获取付费内容
            list = PayContentBLL.SingleModel.GetContentFormatEnt(list, userInfo?.Id);
            if (liststyle > 1 && list.Count() > 0)
            {
                list.ForEach(item =>
                {
                    item.img_fmt = ImgHelper.ResizeImg(item.img, newsImgFormatDic[liststyle][0], newsImgFormatDic[liststyle][1]);
                });
            }

            list.ForEach(item =>
            {
                item.content = string.Empty;//不需要字段内容太多导致小程序端不显示其实可以改获取数据库时候控制但是不清楚小程序端需要哪些字段
            });

            return Json(new { isok = true, data = list, allcount = allcount }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取单个资讯详情
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult GetNewsInfo()
        {
            int id = Context.GetRequestInt("id", 0);
            int version = Context.GetRequestInt("version", 0);
            string openid = Context.GetRequest("openid", "");
            string appid = Context.GetRequest("appid", "");
            if (id <= 0)
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            EntNews model = EntNewsBLL.SingleModel.GetModel(id);
            if (model == null || model.state == 0)
            {
                return Json(new { isok = false, msg = "内容不存在或已删除！" }, JsonRequestBehavior.AllowGet);
            }
            model.img_fmt = Utility.ImgHelper.ResizeImg(model.img, imgFormatDic["normal"][0], 0, "lfit");
            if (!string.IsNullOrEmpty(model.slideimgs) && version > 0)
            {
                model.slideimgs_fmt = string.Join("|", model.slideimgs.SplitStr(",").Select(p => ImgHelper.ResizeImg(p, imgFormatDic["normal"][0], imgFormatDic["normal"][1])).ToArray());
            }
            if (!string.IsNullOrEmpty(model.content))
            {
                // model.content = ImgHelper.ResizeContentImg(model.content, false, "lfit",100,750);
            }
            //获取用户数据
            C_UserInfo userInfo = null;
            if (!string.IsNullOrWhiteSpace(appid) && !string.IsNullOrWhiteSpace(openid))
            {
                userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            }

            model = PayContentBLL.SingleModel.GetContentFormatEnt(model, userInfo?.Id);
            if (!string.IsNullOrEmpty(model.videocover) && version > 0)
            {
                //视频封面略缩图
                model.videocover = ImgHelper.ResizeImg(model.videocover, 750, 400);
            }
            if (model.ispay && !model.ispaid && userInfo != null)
            {
                PayContentPayment payInfo = PayContentBLL.SingleModel.GetVipDiscountByUserId(PayContentBLL.SingleModel.GetModel(model.paycontent), userInfo.Id);
                if (payInfo.PayAmount > 0 && model.contenttype == (int)PaidContentType.专业版图文)
                {
                    //未支付：屏蔽文章返回数据
                    model.content = null;
                }
                if (payInfo.PayAmount > 0 && model.contenttype == (int)PaidContentType.专业版视频)
                {
                    //未支付：屏蔽视频返回数据
                    model.video = null;
                }
            }
            if (!string.IsNullOrWhiteSpace(model.RecommendedItem))
            {
                model.GoodItem = EntGoodsBLL.SingleModel.GetListByIds(model.RecommendedItem);
            }
            return Json(new { isok = true, msg = model }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 更新资讯访问量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult UpdateNewsPV(int id = 0)
        {
            bool result = false;
            if (id > 0)
            {
                EntNews model = EntNewsBLL.SingleModel.GetModel(id);
                if (model != null)
                {
                    model.PV += 1;
                    result = EntNewsBLL.SingleModel.Update(model, "PV");
                }
            }
            return Json(new { isok = result, msg = "" });
        }
        /// <summary>
        /// 获取多个资讯详情
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult GetNewsInfoByids()
        {
            string ids = Context.GetRequest("ids", string.Empty);
            string openid = Context.GetRequest("openid", "");
            string appid = Context.GetRequest("appid", "");
            //列表显示样式，根据这个值来选择裁剪的尺寸
            int liststyle = Context.GetRequestInt("liststyle", 1);
            if (string.IsNullOrEmpty(ids))
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            int pageindex = Context.GetRequestInt("pageindex", 1);
            int pagesize = Context.GetRequestInt("pagesize", 2000);
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string wherestr = $" find_in_set(id,@ids)>0 and state=1 ";
            parameters.Add(new MySqlParameter("@ids", ids));
            List<EntNews> newslist = EntNewsBLL.SingleModel.GetListByParam(wherestr, parameters.ToArray(), pagesize, pageindex, "*", " find_in_set( id,@ids)");
            if (newslist != null && liststyle > 1 && liststyle <= 5)
            {
                newslist.ForEach(n =>
                {
                    n.img_fmt = ImgHelper.ResizeImg(n.img, newsImgFormatDic[liststyle][0], newsImgFormatDic[liststyle][1]);
                });
            }
            //获取用户数据
            C_UserInfo userInfo = null;
            if (!string.IsNullOrWhiteSpace(appid) && !string.IsNullOrWhiteSpace(openid))
            {
                userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            }
            newslist = PayContentBLL.SingleModel.GetContentFormatEnt(newslist, userInfo?.Id);

            newslist.ForEach(n =>
            {
                n.content = string.Empty;//不需要字段内容太多导致小程序端不显示,其实可以改获取数据库时候控制但是不清楚小程序端需要哪些字段
            });
            return Json(new { isok = true, msg = newslist }, JsonRequestBehavior.AllowGet);
        }

        #endregion 资讯

        /// <summary>
        /// 小程序首页获取拼团
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetGroupByIds()
        {
            string ids = Context.GetRequest("ids", string.Empty);
            int aid = Context.GetRequestInt("aid", 0);
            if (string.IsNullOrEmpty(ids) || aid <= 0)
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            Store store = StoreBLL.SingleModel.GetModelByRid(aid);
            if (store == null)
            {
                return Json(new { isok = false, msg = "还未开通店铺" }, JsonRequestBehavior.AllowGet);
            }
            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string sql = $"find_in_set(id,@ids) and state=1 and StoreId={store.Id} and ValidDateStart<'{nowtime}' and ValidDateEnd>'{nowtime}'";
            parameters.Add(new MySqlParameter("@ids", ids));
            List<Groups> grouplist = GroupsBLL.SingleModel.GetListByParam(sql, parameters.ToArray(), 2000, 1, "*", "Id Desc");
            if (grouplist != null && grouplist.Count > 0)
            {
                grouplist.ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.ImgUrl) && p.ImgUrl.IndexOf('@') == -1)
                    {
                        p.ImgUrl = ImgHelper.ResizeImg(p.ImgUrl, 250, 250);
                    }

                    //判断是否已结束
                    if ((p.ValidDateEnd < DateTime.Now || p.RemainNum <= 0))
                    {
                        p.State = 2;
                    }
                    else if ((p.ValidDateStart < DateTime.Now && p.RemainNum > 0))
                    {
                        //判断是否开始
                        p.State = 1;
                    }
                    else
                    {
                        p.State = -1;
                    }

                    //已团数量
                    p.GroupsNum = GroupUserBLL.SingleModel.GetCountPayGroup(p.Id);
                    //已售数量
                    p.salesCount = GroupUserBLL.SingleModel.GetListByGroupId(p.Id).Sum(m => m.BuyNum);

                    //p.GroupsNum = p.CreateNum - p.RemainNum;
                });
            }
            var postdata = grouplist?.Select(s => new
            {
                s.DiscountPrice,
                s.UnitPrice,
                s.OriginalPrice,
                s.GroupName,
                s.GroupSize,
                s.Id,
                s.State,
                s.ImgUrl,
                s.GroupsNum,
                s.StoreId,
                s.salesCount,
                s.virtualSalesCount,
            });

            return Json(new { isok = true, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 专业拼团
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetEntGroupByIds()
        {
            string ids = Context.GetRequest("ids", string.Empty);
            int aid = Context.GetRequestInt("aid", 0);
            if (string.IsNullOrEmpty(ids) || aid <= 0)
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }

            List<EntGroupsRelation> grouplist = EntGroupsRelationBLL.SingleModel.GetListEntGroups_api(ids, aid);

            var postdata = grouplist?.Select(s => new
            {
                DiscountPrice = s.GroupPriceStr,
                UnitPrice = s.SinglePriceStr,
                OriginalPrice = s.OriginalPriceStr,
                GroupName = s.Name,
                s.GroupSize,
                Id = s.EntGoodsId,
                s.State,
                s.ImgUrl,
                s.GroupsNum,
                s.CreateNum,
                s.salesCount,
                s.virtualSalesCount,
            });

            return Json(new { isok = true, msg = "成功", postdata = postdata, }, JsonRequestBehavior.AllowGet);
        }

        #endregion 企业版小程序接口

        #region 行业版高级版

        /// <summary>
        /// 获取店铺配置
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetStoreInfo(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            XcxTemplate _tempLate = XcxTemplateBLL.SingleModel.GetModel(umodel.TId);
            if (_tempLate == null)
            {
                return Json(new { isok = false, msg = "系统繁忙tempLate_null！" }, JsonRequestBehavior.AllowGet);
            }

            Store storeInfo = StoreBLL.SingleModel.GetModelByAId(umodel.Id);
            if (storeInfo == null)
            {
                return Json(new { isok = false, msg = "找不到店铺信息" }, JsonRequestBehavior.AllowGet);
            }

            //序列化对象可能出错
            try
            {
                storeInfo.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(storeInfo.configJson) ?? new StoreConfigModel();
                if (!string.IsNullOrEmpty(storeInfo.funJoinModel.goodsCatIds))
                {
                    storeInfo.goodsCatList = EntGoodTypeBLL.SingleModel.GetListByIds(umodel.Id, storeInfo.funJoinModel.goodsCatIds);
                }

                if (!string.IsNullOrEmpty(storeInfo.funJoinModel.ExchangePlayCardConfig))
                {
                    storeInfo.funJoinModel.PlayCardConfigModel = JsonConvert.DeserializeObject<ExchangePlayCardConfig>(storeInfo.funJoinModel.ExchangePlayCardConfig) ?? new ExchangePlayCardConfig();
                }

            }
            catch (Exception)
            {
                storeInfo.funJoinModel = new StoreConfigModel();
            }
            if (storeInfo.funJoinModel.imSwitch)
            {
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetKfInfo(umodel.AppId);
                storeInfo.funJoinModel.imSwitch = userInfo != null;
                if (userInfo != null)
                {
                    storeInfo.kfInfo = new { nickName = userInfo.NickName, uid = userInfo.Id, headImgUrl = userInfo.HeadImgUrl };
                }
            }

            if (_tempLate.Type == (int)TmpType.小程序专业模板)
            {
                EntSetting ent = EntSettingBLL.SingleModel.GetModel($"aid={umodel.Id}");
                if (ent == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙store_null！" }, JsonRequestBehavior.AllowGet);
                }

                storeInfo.funJoinModel.canSaveMoneyFunction = ent.funJoinModel.canSaveMoneyFunction;
            }
            
            List<PickPlace> placeList = PickPlaceBLL.SingleModel.GetListByAid(umodel.Id);
            object postData = new
            {
                storeInfo = storeInfo,
                placeList
            };

            return Json(new { isok = true, msg = "成功", postData = postData }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取店铺自提地址列表根据用户位置就近返回
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetStorePickPlace()
        {
            Return_Msg_APP returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex =Context.GetRequestInt("pageIndex", 1);
            string latStr =Context.GetRequest("lat", string.Empty);
            string lngStr =Context.GetRequest("lng", string.Empty);


            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcx = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcx == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            double lat = 0.00;
            double lng = 0.00;
           
            //表示没有传坐标 通过客户端IP获取经纬度
            if (!double.TryParse(latStr, out lat) || !double.TryParse(lngStr, out lng) || lat <= 0 || lng <= 0)
            {
                string IP = WebHelper.GetIP();

                IPToPoint iPToPoint = CommondHelper.GetLoctionByIP(IP);
                if (iPToPoint != null && iPToPoint.result != null)
                {
                    lat = iPToPoint.result.location.lat;
                    lng = iPToPoint.result.location.lng;
                }
            }


            int totalCount = 0;
            List<PickPlace> placeList = PickPlaceBLL.SingleModel.GetListNearStoreByLocation(xcx.Id,out totalCount,pageSize,pageIndex,lat,lng);
            returnObj.isok = true;
            returnObj.dataObj = new { placeList= placeList, totalCount= totalCount };
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }


        #region 购物车/订单

        /// <summary>
        /// 查询购物车指定记录 (暂不用)
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="orderbyid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getGoodsCarDataByIds(string appid, string openid, List<int> goodsCarList)
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

            List<EntGoodsCart> myCartList = EntGoodsCartBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCarList)}) and UserId = {userInfo.Id} ");
            if (myCartList == null || !myCartList.Any())
            {
                return Json(new { isok = -1, msg = "没有找到记录" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = 1, msg = "成功", postdata = myCartList }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询购物车
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getGoodsCarData(string appid, string openid)
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
            List<EntGoodsCart> myCarts = EntGoodsCartBLL.SingleModel.GetMyCart(umodel.Id, userInfo.Id) ?? new List<EntGoodsCart>();
            object postdata = null;
            if (myCarts == null || myCarts.Count <= 0) return Json(new { isok = 1, msg = "成功", postdata = postdata });
            //会员打折
            VipLevelBLL.SingleModel.CalculateVipGoodsCartPrice(myCarts, userInfo.Id);

            //获取商品详细资料
            List<EntGoods> goods = EntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", myCarts.Select(x => x.FoodGoodsId))}) ");
            if (goods == null || goods.Count <= 0) return Json(new { isok = 1, msg = "成功", postdata = postdata });
            EntGoods curGood = new EntGoods();
            myCarts.ForEach(c =>
            {
                curGood = goods.FirstOrDefault(g => g.id == c.FoodGoodsId);
                curGood.description = string.Empty;
                if (curGood?.id > 0)
                {
                    curGood.description = string.Empty;
                    //curGood.img = ImgHelper.ResizeImg(curGood.img, 320, 320); //裁剪图片
                    c.goodsMsg = curGood;
                }
            });

            //获取分类详细资料
            List<string> typeIds = goods.Where(g => !string.IsNullOrWhiteSpace(g.ptypes)).Select(x => x.ptypes).Distinct().ToList();
            List<EntGoodType> types = new List<EntGoodType>();

            if (typeIds == null || typeIds.Count <= 0) return Json(new { isok = 1, msg = "成功", postdata = postdata });

            types = EntGoodTypeBLL.SingleModel.GetList($" Id in ({string.Join(",", typeIds)}) ") ?? new List<EntGoodType>();
            postdata = types.Select(x => new
            {
                typeName = x.name,
                typeid = x.id,
                GoodsCar = myCarts.Where(y => (y.goodsMsg.ptypes + ",").Contains(x.id.ToString() + ",")).ToList()
            });

            return Json(new { isok = 1, msg = "成功", postdata = postdata /*, myCartList = myCartList*/ /*,sotreid = store.Id, userid = userInfo.Id*/}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询购物车
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getGoodsCarData_new(string appid, string openid, bool isGetReserve = false)
        {
            int pageSize = Context.GetRequestInt("pageSize", 6);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);

            try
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
                List<EntGoodsCart> myCarts = null;
                if (isGetReserve)
                {
                    myCarts = EntGoodsCartBLL.SingleModel.GetMyCart(umodel.Id, userInfo.Id, (int)EntGoodCartType.预约);
                }
                else
                {
                    myCarts = EntGoodsCartBLL.SingleModel.GetMyCart(umodel.Id, userInfo.Id);
                }

                if (myCarts == null || !myCarts.Any())
                {
                    return Json(new { isok = 1, msg = "成功", postdata = new { } }, JsonRequestBehavior.AllowGet);
                }

                #region 会员打折 商品加入了原价 这里为了跟商品详情页价格保持一致,并且兼容之前的数据

                myCarts.ForEach(g =>
                {
                    g.originalPrice = g.originalPrice > 0 ? g.originalPrice : g.Price;
                    g.Price = g.NotDiscountPrice > 0 ? g.NotDiscountPrice : g.Price;
                });
                //获取会员信息
                VipRelation vipInfo = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id} and state>=0");
                VipLevel levelinfo = null;
                if (vipInfo != null)
                {
                    levelinfo = VipLevelBLL.SingleModel.GetModel($"id={vipInfo.levelid} and state>=0");
                    if (levelinfo != null)
                    {
                        if (levelinfo.type == 1)//全场打折
                        {
                            myCarts.ForEach(g =>
                            {
                                g.discount = levelinfo.discount;
                                //   g.Price =g.NotDiscountPrice>0?g.NotDiscountPrice:(Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)));
                            });
                        }
                        else if (levelinfo.type == 2)//部分打折
                        {
                            List<string> gids = levelinfo.gids.Split(',').ToList();
                            myCarts.ForEach(g =>
                            {
                                if (gids.Contains(g.FoodGoodsId.ToString()))
                                {
                                    g.discount = levelinfo.discount;
                                    //  g.Price = g.NotDiscountPrice > 0 ? g.NotDiscountPrice : (Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)));
                                }
                            });
                        }
                    }
                }

                #endregion 会员打折

                //获取商品详细资料
                List<EntGoods> goods = new List<EntGoods>();
                if (myCarts != null && myCarts.Any())
                {
                    string goodsIds = string.Join(",", myCarts.Select(x => x.FoodGoodsId));
                    goods = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);
                    if (goods != null && goods.Count>0)
                    {
                        EntGoods curGood = new EntGoods();
                        myCarts.ForEach(c =>
                        {
                            curGood = goods?.FirstOrDefault(g => g.id == c.FoodGoodsId);
                            if (curGood != null && curGood?.id > 0)
                            {
                                //计算会员价格  
                                VipLevelBLL.SingleModel.CalculateVipGoodsPrice(curGood, levelinfo);

                                curGood.description = string.Empty;
                                //curGood.img = ImgHelper.ResizeImg(curGood.img, 320, 320); //裁剪图片

                                c.goodsMsg = curGood;

                                #region 购物车商品规格值是否有失效的
                                if (curGood.price != c.Price || curGood.originalPrice != c.originalPrice)
                                {
                                    //表示该商品价格已经变动，购物车失效，需要重新删除重新添加
                                    c.PriceState = -1;
                                }


                                List<EntGoodsAttrDetail> listEntGoodsAttrDetail = JsonConvert.DeserializeObject<List<EntGoodsAttrDetail>>(curGood.specificationdetail);
                                if (listEntGoodsAttrDetail == null || listEntGoodsAttrDetail.Count <= 0)
                                {
                                    if (!string.IsNullOrEmpty(c.SpecInfo) || !string.IsNullOrWhiteSpace(c.SpecInfo))
                                    {
                                        //表示之前购物车有规格,后面产品改成没有规格的了
                                        c.SpecificationState = -1;
                                    }
                                }
                                else
                                {
                                    //产品规格发生变化
                                    c.SpecificationState = -1;
                                    foreach (EntGoodsAttrDetail item in listEntGoodsAttrDetail)
                                    {
                                        if (item.id == c.SpecIds)
                                        {
                                            c.SpecificationState = 0;
                                            break;
                                        }
                                    }
                                }
                                #endregion
                            }
                        });
                    }
                }

                #region 处理购物车看见拼团商品问题

                List<EntGoodsCart> effCart = new List<EntGoodsCart>();
                //清除拼团商品的购物车记录,防止用拼团商品下单.
                foreach (EntGoodsCart item in myCarts)
                {
                    if (item.goodsMsg?.goodtype == 1)
                    {
                        item.State = -1;
                        EntGoodsCartBLL.SingleModel.Update(item, "state");
                    }
                    else
                    {
                        effCart.Add(item);
                    }
                }
                myCarts = effCart;

                #endregion 处理购物车看见拼团商品问题

                List<EntGoodsCart> postdata = myCarts;

                return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Json(new { isok = 0, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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
        public ActionResult addGoodsCarData(string appid, string openid, int goodid, string attrSpacStr, string SpecInfo, int qty, int newCartRecord = 0, int isgroup = 0, bool isReservation = false, string SpecImg = "")
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

            if (qty <= 0)
            {
                return Json(new { isok = -1, msg = "数量必须大于0" }, JsonRequestBehavior.AllowGet);
            }

            EntGoods good = EntGoodsBLL.SingleModel.GetModel(goodid);
            if (good == null)
            {
                return Json(new { isok = -1, msg = "未找到该商品" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrWhiteSpace(attrSpacStr))
            {
                if (!good.GASDetailList.Any(x => x.id.Equals(attrSpacStr)))
                {
                    return Json(new { isok = -1, msg = "未找到该商品" }, JsonRequestBehavior.AllowGet);
                }
            }
            if (!(good.state == 1 && good.tag == 1))
            {
                return Json(new { isok = -1, msg = "无法添加失效商品" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            
            EntGoodsCart dbGoodCar = EntGoodsCartBLL.SingleModel.GetModel($" UserId={userInfo.Id} and FoodGoodsId={good.id} and SpecIds='{attrSpacStr}' and State = 0 and gotobuy=0");
            if (dbGoodCar == null || newCartRecord == 1)
            {
                //商品价格
                int price = Convert.ToInt32(good.price * 100);
                int originalPrice = Convert.ToInt32(good.originalPrice * 100);
                if (!string.IsNullOrWhiteSpace(attrSpacStr))
                {
                    originalPrice = Convert.ToInt32(good.GASDetailList.First(x => x.id.Equals(attrSpacStr)).originalPrice * 100);
                }
                //判断是否是拼团
                if (isgroup > 0)
                {
                    good.storeId = 0;
                    string groupmsg = CheckGoodCount(userInfo.Id, qty, good, attrSpacStr, ref price);
                    if (!string.IsNullOrEmpty(groupmsg))
                    {
                        return Json(new { isok = -1, msg = groupmsg }, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrWhiteSpace(attrSpacStr))
                    {
                        originalPrice = Convert.ToInt32(EntGroupsRelationBLL.SingleModel.GetModel($"EntGoodsId={good.id}")?.OriginalPrice);
                    }
                }
                else
                {
                    price = Convert.ToInt32(!string.IsNullOrWhiteSpace(attrSpacStr) ? good.GASDetailList.First(x => x.id.Equals(attrSpacStr)).price * 100 : good.price * 100);
                    originalPrice = originalPrice > 0 ? originalPrice : price;
                }
                EntGoodsCart goodsCar = new EntGoodsCart
                {
                    NotDiscountPrice = price,
                    originalPrice = originalPrice,
                    GoodName = good.name,
                    FoodGoodsId = good.id,
                    SpecIds = attrSpacStr,
                    Count = qty,
                    Price = price,
                    SpecInfo = SpecInfo,
                    SpecImg = SpecImg,//规格图片
                    UserId = userInfo.Id,
                    CreateDate = DateTime.Now,
                    State = 0,
                    GoToBuy = newCartRecord,
                    aId = umodel.Id,
                    type = isReservation ? (int)EntGoodCartType.预约 : (int)EntGoodCartType.普通
                };

                //加入购物车
                int id = Convert.ToInt32(EntGoodsCartBLL.SingleModel.Add(goodsCar));
                if (id > 0)
                {
                    return Json(new { isok = 1, msg = "成功", cartid = id }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { isok = -1, msg = "失败", cartid = 0 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                dbGoodCar.Count += qty;
                if (EntGoodsCartBLL.SingleModel.Update(dbGoodCar, "Count"))
                {
                    return Json(new { isok = 1, msg = "成功", cartid = dbGoodCar.Id }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 从购物车 删除商品/更新数量
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodsCarId"></param>
        /// <param name="function">0为更新,-1为删除</param>
        /// <returns></returns>
        public ActionResult updateOrDeleteGoodsCarDataBySingle(string appid, string openid, EntGoodsCart item, int function)
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
            if (item != null)
            {
                EntGoodsCart goodsCar = EntGoodsCartBLL.SingleModel.GetModel(item.Id);
                if (goodsCar == null)
                {
                    return Json(new { isok = -1, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
                }
                if (goodsCar.UserId != userInfo.Id)
                {
                    return Json(new { isok = -1, msg = "该记录不属于当前用户" }, JsonRequestBehavior.AllowGet);
                }
                if (goodsCar.State == 1)
                {
                    return Json(new { isok = -1, msg = "不可修改此记录" }, JsonRequestBehavior.AllowGet);
                }
                //将记录状态改为删除
                if (function == -1)
                {
                    goodsCar.State = -1;
                }
                else if (function == 0)//根据传入参数更新购物车内容
                {
                    //goodsCar.SpecIds = item.SpecIds;
                    //goodsCar.SpecInfo = item.SpecInfo;
                    goodsCar.Count = item.Count;
                }

                bool success = EntGoodsCartBLL.SingleModel.Update(goodsCar, "State,Count,SpecInfo,SpecIds");
                if (!success)
                {
                    return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 从购物车 删除商品
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodsCarId"></param>
        /// <param name="function">0为更新,-1为删除</param>
        /// <returns></returns>
        public ActionResult updateOrDeleteGoodsCarData(string appid, string openid, List<ent.EntGoodsCart> goodsCarModel, int function)
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
            if (goodsCarModel != null && goodsCarModel.Count > 0)
            {
                foreach (EntGoodsCart item in goodsCarModel)
                {
                    EntGoodsCart goodsCar = EntGoodsCartBLL.SingleModel.GetModel(item.Id);
                    if (goodsCar == null)
                    {
                        return Json(new { isok = -1, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
                    }
                    if (goodsCar.UserId != userInfo.Id)
                    {
                        return Json(new { isok = -1, msg = "该记录不属于当前用户" }, JsonRequestBehavior.AllowGet);
                    }
                    if (goodsCar.State == 1)
                    {
                        return Json(new { isok = -1, msg = "不可修改此记录" }, JsonRequestBehavior.AllowGet);
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
                        goodsCar.SpecImg = item.SpecImg;

                        //价格因更改规格随之改变
                        EntGoods carGoods = EntGoodsBLL.SingleModel.GetModel(goodsCar.FoodGoodsId);
                        if (carGoods == null)
                        {
                            goodsCar.GoodsState = 2;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(carGoods.specificationdetail))
                            {
                                float? price = carGoods.GASDetailList.Where(x => x.id.Equals(goodsCar.SpecIds))?.FirstOrDefault()?.price;
                                if (price != null)
                                {
                                    goodsCar.Price = Convert.ToInt32(price * 100);
                                }
                            }
                        }
                    }

                    bool success = EntGoodsCartBLL.SingleModel.Update(goodsCar, "State,Count,SpecInfo,SpecIds,Price,GoodsState,SpecImg");
                    if (!success)
                    {
                        return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
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
            List<EntGoodsCart> goodsCar = EntGoodsCartBLL.SingleModel.GetList($" Id in({string.Join(",", goodCarIdList)}) and UserId = {userInfo.Id} ");
            if (goodsCar == null || !goodsCar.Any())
            {
                return Json(new { isok = -1, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
            }

            List<EntGoodsCart> myCartList = EntGoodsCartBLL.SingleModel.GetMyCart(umodel.Id, userInfo.Id);
            List<EntFreightTemplate> fmodelList = EntFreightTemplateBLL.SingleModel.GetList($" aId = {umodel.Id} and (ISNULL(FullDiscount) or FullDiscount = 0) and state >= 0  ");
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

            var postdata = fmodelList.Select(x => new { x.Id, x.Name, x.sum });

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
                return Json(new { isok = -1, msg = "数量必须大于0" }, JsonRequestBehavior.AllowGet);
            }
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

            EntGoods good = EntGoodsBLL.SingleModel.GetModel(goodId);
            if (good == null)
            {
                return Json(new { isok = -1, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
            }

            List<EntFreightTemplate> fmodelList = EntFreightTemplateBLL.SingleModel.GetList($" aid = {umodel.Id} and state >= 0 ");
            if (fmodelList == null || !fmodelList.Any())
            {
                return Json(new { isok = 1, msg = "商家无设定运费模板" }, JsonRequestBehavior.AllowGet);
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

            var postdata = fmodelList.Select(x => new { x.Id, x.Name, x.sum });

            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
        /// <param name="orderjson">订单JSON</param>
        /// <param name="zqStoreName">自取门店名称</param>
        /// <param name="zqStoreId">自取门店ID</param>
        /// <param name="flashItemId">秒杀商品ID</param>
        /// <returns></returns>
        public ActionResult addMiniappGoodsOrder(string appid, string openid, string goodCarIdStr, string orderjson, string wxaddressjson = "", int buyMode = (int)miniAppBuyMode.微信支付, int aid = 0, int reserveId = 0, string zqStoreName = "", int zqStoreId = 0, int flashItemId = 0)
        {
            #region 达达下单参数

            //是否通过接口物流配送,1:商家配送，2：达达配送
            int getWay = Context.GetRequestInt("getWay", (int)miniAppOrderGetWay.商家配送);
            string cityname = Context.GetRequest("cityname", string.Empty);
            string lat = Context.GetRequest("lat", string.Empty);
            string lnt = Context.GetRequest("lnt", string.Empty);
            int distributionprice = Context.GetRequestInt("distributionprice", 0);//达达运费

            #endregion 达达下单参数

            //店铺二维码
            int storecodeid = Context.GetRequestInt("storecodeid", 0);
            //判断是否发起拼团
            int isgroup = Context.GetRequestInt("isgroup", 0);//是否发起拼团
            int groupid = Context.GetRequestInt("groupid", 0);//团ID
            int goodtype = Context.GetRequestInt("goodtype", 0);//是否是团产品，0：不是，1：是

            int orderType = Context.GetRequestInt("orderType", 0);
            if (isgroup > 0 && groupid > 0)
            {
                return Json(new { isok = -1, msg = "拼团参数错误" }, JsonRequestBehavior.AllowGet);
            }
            //预约订单==到店自取
            if (reserveId > 0)
            {
                getWay = (int)miniAppOrderGetWay.到店自取;
            }

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权 umodel_null" }, JsonRequestBehavior.AllowGet);
            }

            //清除商品缓存
            EntGoodsBLL.SingleModel.RemoveEntGoodListCache(umodel.Id);

            //不同商家，不同的锁,当前商家若还未创建，则创建一个
            if (!lockObjectDict_Order.ContainsKey(umodel.Id))
            {
                if (!lockObjectDict_Order.TryAdd(umodel.Id, new object()))
                {
                    return Json(new { isok = -1, msg = "店铺下单火热,请稍候再试" }, JsonRequestBehavior.AllowGet);
                }
            }

            #region 基本验证

            if (string.IsNullOrEmpty(orderjson))
            {
                return Json(new { isok = -1, msg = "订单不能为空 " }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在 " }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(goodCarIdStr))
            {
                return Json(new { isok = -1, msg = "购物车异常 " }, JsonRequestBehavior.AllowGet);
            }
            List<string> goodCarIdList = goodCarIdStr.Split(',').ToList();
            if (goodCarIdStr.Substring(goodCarIdStr.Length - 1, 1) == ",")
            {
                goodCarIdList = goodCarIdStr.Substring(0, goodCarIdStr.Length - 1).Split(',').ToList();
            }

            List<EntGoodsCart> goodsCar = reserveId > 0 ? EntGoodsCartBLL.SingleModel.GetList($" Id in({string.Join(",", goodCarIdList)}) and UserId = {userInfo.Id}") : EntGoodsCartBLL.SingleModel.GetList($" Id in({string.Join(",", goodCarIdList)}) and UserId = {userInfo.Id} and state = 0 ");
            if (goodsCar == null || goodsCar.Count() <= 0)
            {
                return Json(new { isok = -1, msg = "找不到购物车记录 " }, JsonRequestBehavior.AllowGet);
            }

            //地址验证
            if (string.IsNullOrWhiteSpace(wxaddressjson))
            {
                return Json(new { isok = -1, msg = "未检测到地址信息 " }, JsonRequestBehavior.AllowGet);
            }
            WxAddress address = null;
            try
            {
                address = JsonConvert.DeserializeObject<WxAddress>(wxaddressjson);
                if (address == null)
                {
                    return Json(new { isok = -1, msg = "地址信息不存在 " }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "地址信息错误 " }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrWhiteSpace(address.userName))
            {
                return Json(new { isok = -1, msg = "未输入 收/提货人姓名 " }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrWhiteSpace(address.telNumber))
            {
                return Json(new { isok = -1, msg = "未输入 收/提货电话 或 格式不正确 " }, JsonRequestBehavior.AllowGet);
            }

            //订单
            EntGoodsOrder order = null;
            try
            {
                order = JsonConvert.DeserializeObject<ent.EntGoodsOrder>(orderjson);
                if (order == null)
                {
                    return Json(new { isok = -1, msg = "未检测到订单信息" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "订单信息错误 " }, JsonRequestBehavior.AllowGet);
            }

            #endregion 基本验证

            if (order.GetWay == (int)miniAppOrderGetWay.蜂鸟配送 || order.GetWay == (int)miniAppOrderGetWay.达达配送)
            {
                return Json(new { isok = -1, msg = $"当前不支持此种配送方式({Enum.GetName(typeof(miniAppOrderGetWay), order.GetWay)})" }, JsonRequestBehavior.AllowGet);
            }
            //判定是否存在失效商品
            EntGoods good = null;
            foreach (EntGoodsCart c in goodsCar)
            {

                good = EntGoodsBLL.SingleModel.GetModel(c.FoodGoodsId);

                if (c.GoodsState != 0)
                {
                   
                    if (good == null)
                    {
                        return Json(new { isok = -1, msg = "商品信息错误,请重新选择购买商品！goodsCar_good_null" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { isok = -1, msg = $"商品 '{good.name}' 已经下架或被删除,请重新选择购买商品！ " }, JsonRequestBehavior.AllowGet);
                }

                //判断该商品当天销量是否大于每天限制库存
                if (good != null)
                {
                    if (EntGoodsBLL.SingleModel.LimitDayStock(c.FoodGoodsId, good.aid, c.Count))
                    {
                        return Json(new { isok = -1, msg = $"产品{good.name}今日库存已不足购买数量,请重新选择该商品数量" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            int beforeDiscountPrice = 0;//优惠前商品总价
            int afterDiscountPrice = 0;//优惠后商品总价

            //拼团
            int grouperprice = 0;//团长减价
            EntGroupsRelation groupmodel = new EntGroupsRelation() { RId = umodel.Id };
            if (isgroup > 0 || groupid > 0)
            {
                //判断是否是拼团，如果是拼团则将产品价格改成拼团价，获取团长优惠价
                string groupmsg = CommandEntGroup(isgroup, groupid, userInfo.Id, 0, goodsCar[0].FoodGoodsId, ref grouperprice, ref groupmodel, goodsCar[0].Count);
                if (!string.IsNullOrEmpty(groupmsg))
                {
                    return Json(new { isok = -1, msg = groupmsg }, JsonRequestBehavior.AllowGet); ;
                }
            }

            beforeDiscountPrice = goodsCar.Sum(x => x.Price * x.Count);//优惠前商品总价

            #region 秒杀商品优惠
            //秒杀商品为立即购买（下单固定为1种商品）
            FlashDealPayInfo flashPay = null;
            if (goodsCar?.Count == 1 && flashItemId > 0)
            {
                FlashDealItem flashItem = FlashDealItemBLL.SingleModel.GetModel(flashItemId);
                if (flashItem == null)
                {
                    return Json(new { isok = -1, msg = "秒杀商品不存在或已删除" }, JsonRequestBehavior.AllowGet);
                }
                flashPay = FlashDealBLL.SingleModel.GetFlashDealPayment(flashItem, userInfo.Id);
                if (flashPay != null)
                {
                    //是否允许购买
                    if (!flashPay.IsPay)
                    {
                        return Json(new { isok = -1, msg = flashPay.Info }, JsonRequestBehavior.AllowGet);
                    }
                    //格式化购物车价格
                    EntGoodsCart cartItem = FlashDealItemBLL.SingleModel.GetFlashDealPrice(goodsCar.First(), flashItem);
                    order.attrbuteModel.flashItemId = flashPay.FlashItemId;
                    order.attrbuteModel.flashDealId = flashPay.FlashDealId;
                    order.attribute = JsonConvert.SerializeObject(order.attrbuteModel);
                    //修改优惠价格
                    afterDiscountPrice = cartItem.Price*cartItem.Count;
                    goodsCar = new List<EntGoodsCart> { cartItem };
                }
            }
            #endregion

            #region 会员打折（不能与秒杀共存）
            //获取会员信息
            VipRelation vipInfo = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id} and state>=0");
            StringBuilder updateGoodsPriceSql = null;

            FullReduction fullReduction = FullReductionBLL.SingleModel.GetFullReduction(aid);//获取满减优惠活动
            int fullReductionDiscountType = fullReduction != null ? fullReduction.VipDiscount : 0;
            if (flashPay == null)
            {
                int discountType = Context.GetRequestInt("discountType", 0);//优惠类型 默认为0 表示可以使用会员折扣 1表示不能使用会员折扣
                
                if (vipInfo != null && discountType == 0)
                {
                    VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={vipInfo.levelid} and state>=0");
                    if (levelinfo != null)
                    {
                        if (levelinfo.type == 1)//全场打折
                        {
                            goodsCar.ForEach(g =>
                            {
                                g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01));
                                g.discount = levelinfo.discount;
                            });
                        }
                        else if (levelinfo.type == 2)//部分打折
                        {
                            List<string> gids = levelinfo.gids.Split(',').ToList();
                            goodsCar.ForEach(g =>
                            {
                                if (gids.Contains(g.FoodGoodsId.ToString()))
                                {
                                    g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01));
                                    g.discount = levelinfo.discount;
                                }
                            });
                        }
                        updateGoodsPriceSql = new StringBuilder();

                        foreach (EntGoodsCart item in goodsCar)
                        {
                            updateGoodsPriceSql.Append(EntGoodsCartBLL.SingleModel.BuildUpdateSql(item, "Price,originalPrice,discount") + ";");
                        }
                    }
                }
                afterDiscountPrice = goodsCar.Sum(x => x.Price * x.Count);//优惠后商品总价
            }
            #endregion 会员打折

            #region 使用优惠券（不能与秒杀共存）
            //用户领取的优惠券记录ID
            int couponsum = 0;
            int couponlogid = Context.GetRequestInt("couponlogid", 0);
            if (flashPay == null)
            {
                if (couponlogid > 0)
                {
                    string couponmsg = "";
                    //优惠金额
                    couponsum = CouponLogBLL.SingleModel.GetCouponPrice(couponlogid, goodsCar, ref couponmsg);
                    if (!string.IsNullOrEmpty(couponmsg))
                    {
                        return Json(new { isok = -1, msg = couponsum }, JsonRequestBehavior.AllowGet);
                    }
                }
               
            }
            else
            {
                couponsum = 0;
                couponlogid = 0;
            }
            #endregion 使用优惠券

            #region 绑定扫码下单信息
            if (storecodeid == 0)
            {
                int? scanCodeId = UserTrackBLL.SingleModel.GetLastScanId(userInfo.Id);
                storecodeid = scanCodeId.HasValue ? scanCodeId.Value : storecodeid;
            }
            #endregion

            try
            {
                //商品总价格
                int price = goodsCar.Sum(x => x.Price * x.Count);
                if (price <= 0 || price > 999999999)
                {
                    return Json(new { isok = -1, msg = $"商品价格有误 price:{price}" }, JsonRequestBehavior.AllowGet);
                }

                //运费
                int freightFee = 0;
                Store store = StoreBLL.SingleModel.GetModelByAId(umodel.Id);
                StoreConfigModel config = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);

                if (!config.CashOnDelivery && buyMode == (int)miniAppBuyMode.货到付款)
                {
                    return Json(new { isok = -1, msg = $"商品不支持货到付款" }, JsonRequestBehavior.AllowGet);
                }

                int reducetionMoney = 0;//满减优惠金额
                //表示没有用优惠券和拼团，如果达到满减优惠条件则需要减去优惠
                if (couponlogid <= 0 && isgroup <= 0 && groupid <=0)
                {
                    if (fullReduction != null)
                    {

                        if (price >= fullReduction.OrderMoney && price > fullReduction.ReducetionMoney)
                        {
                            price -= fullReduction.ReducetionMoney;
                            reducetionMoney= fullReduction.ReducetionMoney;
                            fullReduction.UseCount++;
                            FullReductionBLL.SingleModel.Update(fullReduction, "UseCount");
                        }
                    }
                }

                #region 计算运费

                int qtySum = goodsCar.Sum(x => x.Count);
                switch (getWay)
                {
                    case (int)miniAppOrderGetWay.到店自取:
                    case (int)miniAppOrderGetWay.到店消费:
                        break;

                    case (int)miniAppOrderGetWay.商家配送:
                        if (order.FreightTemplateId > 0 && reserveId == 0)
                        {
                            //用户选择运费模板（同城遗留旧功能）
                            EntFreightTemplate fmodel = EntFreightTemplateBLL.SingleModel.GetModel(order.FreightTemplateId);
                            if (fmodel == null)
                            {
                                return Json(new { isok = -1, msg = "运费模板信息错误" }, JsonRequestBehavior.AllowGet);
                            }
                            if (qtySum <= fmodel.BaseCount)
                            {
                                freightFee = fmodel.BaseCost;
                            }
                            else
                            {
                                //初阶费用加上额外费用
                                freightFee = fmodel.BaseCost + (qtySum - fmodel.BaseCount) * fmodel.ExtraCost;
                            }
                        }
                        else if (config.enableDeliveryTemplate && Enum.IsDefined(typeof(DeliveryFeeSumMethond), config.deliveryFeeSumMethond))
                        {
                            //商家选择的商品运费模板（优化第二版）
                            DeliveryFeeSumMethond sumFeeMethod;
                            if (!Enum.TryParse(config.deliveryFeeSumMethond.ToString(), out sumFeeMethod))
                            {
                                return Json(new { isok = -1, msg = "商家在配置运费模板中" }, JsonRequestBehavior.AllowGet);
                            }
                            //获取运费计算结果
                            DeliveryFeeResult deliveryResult = DeliveryTemplateBLL.SingleModel.GetDeliveryFeeSum(goodsCar, provinces: address.provinceName, city: address.cityName, sumMethod: sumFeeMethod);
                            if (!deliveryResult.InRange)
                            {
                                //商品超出配送范围
                                return Json(new { isok = -1, msg = deliveryResult.Message });
                            }
                            //保存运费
                            freightFee = deliveryResult.Fee;
                            //获取运费模板ID
                            string entGoodIds = string.Join(",", goodsCar.Select(cartItem => cartItem.FoodGoodsId));
                            string templateIds = string.Join(",", EntGoodsBLL.SingleModel.GetListByIds(entGoodIds)?.Where(thisGood => thisGood.TemplateId > 0)?.Select(thisGood => thisGood.TemplateId));
                            //保存运费模板ID
                            order.FreightTemplateId = -1;
                            order.FreightTemplateName = templateIds;
                            order.attrbuteModel = !string.IsNullOrEmpty(order.attribute) ? JsonConvert.DeserializeObject<EntGoodsOrderAttr>(order.attribute) : order.attrbuteModel;
                            order.attrbuteModel.FreightInfo = deliveryResult.Message;
                            DeliveryDiscount deliveryDiscount = DeliveryTemplateBLL.SingleModel.GetDiscount(umodel.Id, price - couponsum - grouperprice);
                            if (deliveryDiscount.HasDiscount)
                            {
                                freightFee = deliveryDiscount.DiscountPrice;
                                order.attrbuteModel.FreightInfo = deliveryDiscount.DiscountInfo;
                            }
                            order.attribute = JsonConvert.SerializeObject(order.attrbuteModel);
                        }
                        else
                        {
                            //商家选择的全局运费模板（优化第一版）
                            FreightInfo freightInfo = EntFreightTemplateBLL.SingleModel.GetFreightFee(config: config, cartIds: goodCarIdStr, province: address.provinceName, city: address.cityName);
                            if (!freightInfo.IsVaild)
                            {
                                return Json(new { isok = -1, msg = freightInfo.Msg, fee = freightInfo.Fee });
                            }
                            freightFee = freightInfo.Fee;
                            order.FreightTemplateName = freightInfo.Msg;
                            order.FreightTemplateId = config.FreightTemplateId;
                        }
                        break;
                }

                #endregion 计算运费

                #region 分销相关

                //拿到每个分销产品的推广记录,便于后面用户订单确认收货后计算佣金
                int salesManRecordId = Context.GetRequestInt("salesManRecordId", 0);
                TransactionModel tranGoodsCarCps_rate = new TransactionModel();
                goodsCar.ForEach(x =>
                {
                    CpsRateCar cpsRateCar = salesManRecordBLL.GetCps_rate(userInfo.Id, x.FoodGoodsId, x.aId, salesManRecordId);
                    x.cps_rate = cpsRateCar.cps_rate;
                    x.salesManRecordUserId = cpsRateCar.salesManRecordUserId;
                    x.recordId = cpsRateCar.recordId;
                    tranGoodsCarCps_rate.Add(EntGoodsCartBLL.SingleModel.BuildUpdateSql(x, "cps_rate,salesManRecordUserId,recordId"));
                });

                if (!EntGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(tranGoodsCarCps_rate.sqlArray, tranGoodsCarCps_rate.ParameterArray))
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "购物车里对应的产品佣金比例更新失败");
                }

                #endregion 分销相关

                #region 外卖

                if (orderType == (int)EntOrderType.外卖订单)
                {
                    order.OrderType = orderType;
                    //计算餐盒费
                    List<EntGoods> goodsList = EntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCar.Select(cart => cart.FoodGoodsId))}) ");
                    foreach (var cart in goodsCar)
                    {
                        cart.goodsMsg = goodsList.Where(goods => goods.id == cart.FoodGoodsId).FirstOrDefault();
                        if (cart.goodsMsg.isPackin == 1)
                        {
                            order.PackinPrice += config.PackinFee * cart.Count;
                        }
                    }
                }

                #endregion 外卖

                //地址信息
                if (!string.IsNullOrEmpty(zqStoreName) || zqStoreId > 0)
                {
                    order.attrbuteModel = !string.IsNullOrEmpty(order.attribute) ? JsonConvert.DeserializeObject<EntGoodsOrderAttr>(order.attribute) : order.attrbuteModel;
                    order.attrbuteModel.zqStoreName = zqStoreName;
                    order.attrbuteModel.zqStoreId = zqStoreId;
                    order.attribute = JsonConvert.SerializeObject(order.attrbuteModel);
                }
                order.AccepterName = address.userName;
                order.AccepterTelePhone = address.telNumber;
                order.ZipCode = address.postalCode;
                order.Address = $"{address.provinceName} {address.cityName} {address.countyName} {address.detailInfo}";

                order.TemplateType = (int)TmpType.小程序专业模板;
                order.StoreId = umodel.Id;
                order.UserId = userInfo.Id;
                order.CreateDate = DateTime.Now;
                if (getWay == (int)miniAppOrderGetWay.到店自取)
                {
                    order.State = buyMode == (int)miniAppBuyMode.货到付款 ? (int)MiniAppEntOrderState.待自取 : (int)MiniAppEntOrderState.待付款;
                }
                else
                {
                    order.State = buyMode == (int)miniAppBuyMode.货到付款 ? (int)MiniAppEntOrderState.待发货 : (int)MiniAppEntOrderState.待付款;
                }
                order.aId = umodel.Id;
                order.QtyCount = goodsCar.Sum(x => x.Count);
                order.BuyMode = buyMode;
                order.GetWay = getWay;
                order.ReducedPrice = beforeDiscountPrice - afterDiscountPrice;
                order.ReducedPrice += couponsum;//加上优惠券优惠
                order.ReducedPrice += grouperprice;//加上团长优惠价格
                order.ReducedPrice += reducetionMoney;//加上满减优惠金额
                order.OrderType = goodtype == 1 ? 3 : 0;//是否是拼团订单
                order.GroupId = groupid;//拼团ID
                order.AppId = appid;
                order.StoreCodeId = storecodeid;

                price -= couponsum;//减去优惠费用
                price -= grouperprice;//减去团长优惠价格
                if (price <= 0)
                {
                    price = 0;
                }

                order.FreightPrice = freightFee;
                order.BuyPrice = price + freightFee;
                order.BuyPrice += distributionprice;//达达运费
                order.BuyPrice += order.PackinPrice;
                order.BuyPrice = order.BuyPrice <= 0 ? 0 : order.BuyPrice;

                lock (lockObjectDict_Order[umodel.Id])
                {
                    string goodsIds = string.Join(",",goodsCar.Select(s=>s.FoodGoodsId).Distinct());
                    List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);
                    //检查当前商品库存是否足够
                    foreach (EntGoodsCart x in goodsCar)
                    {
                        EntGoods curGood = entGoodsList?.FirstOrDefault(f=>f.id == x.FoodGoodsId);
                        if (curGood != null && curGood.stockLimit)
                        {
                            int curGoodQty = EntGoodsBLL.SingleModel.GetGoodQtyByModel(curGood, x.SpecIds);
                            if (curGoodQty < x.Count)
                            {
                                return Json(new { isok = -1, msg = $"商品: {curGood.name} 库存不足!" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }

                    SaveMoneySetUser saveMoneyUser = new SaveMoneySetUser();
                    if (buyMode == (int)miniAppBuyMode.储值支付)
                    {
                        saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(umodel.AppId, userInfo.Id);
                        if (saveMoneyUser == null || saveMoneyUser.AccountMoney < order.BuyPrice)
                        {
                            return Json(new { isok = -1, msg = $" 预存款余额不足！ " }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    #region 预约购物

                    //预约购物（配送方式：到店自取，订单类型：预约订单
                    if (reserveId > 0)
                    {
                        order.GetWay = (int)miniAppOrderGetWay.到店自取;
                        order.OrderType = (int)EntOrderType.预约订单;
                        order.ReserveId = reserveId;
                    }

                    #endregion 预约购物

                    //生成订单.
                    string dugmsg = "";
                    if (!EntGoodsOrderBLL.SingleModel.addGoodsOrder(order, goodsCar, userInfo, updateGoodsPriceSql, ref dugmsg))
                    {
                        return Json(new { isok = -1, msg = $"订单生成失败！" + dugmsg }, JsonRequestBehavior.AllowGet);
                    }

                    //扫码购物统计
                    Task.Factory.StartNew(() =>
                    {
                        UserTrackBLL.SingleModel.AddQrCodeOrder(userInfo.Id, storecodeid);
                    });
                    
                    #region 获取到当前订单 dbOrder

                    EntGoodsCart cartmodel = EntGoodsCartBLL.SingleModel.GetModel("Id=" + goodsCar.First().Id + " and GoodsOrderId>0");
                    if (cartmodel == null)
                    {
                        cartmodel = EntGoodsCartBLL.SingleModel.GetModel("Id=" + goodsCar.First().Id + " and GoodsOrderId>0");
                    }
                    int curGoodOrderId = cartmodel.GoodsOrderId;
                    EntGoodsOrder dbOrder = EntGoodsOrderBLL.SingleModel.GetModel(curGoodOrderId);
                    if (dbOrder == null)
                    {
                        return Json(new { isok = -1, msg = $"订单生成失败！" }, JsonRequestBehavior.AllowGet);
                    }

                    #endregion 获取到当前订单 dbOrder

                    #region 修改优惠券状态

                    if (couponlogid > 0)
                    {
                        CouponLogBLL.SingleModel.Update(new CouponLog() { Id = couponlogid, State = 1, OrderId = dbOrder.Id }, "state,orderid");
                    }

                    #endregion 修改优惠券状态

                    CityMorders citymorderModel = new CityMorders();

                    #region 是否开团


                    string groupmsg = EntGroupSponsorBLL.SingleModel.OpenGroup(isgroup, umodel.Id, buyMode, userInfo.Id, groupmodel, dbOrder.BuyPrice, (int)TmpType.小程序专业模板, ref groupid);
                    if (!string.IsNullOrEmpty(groupmsg))
                    {
                        return Json(new { isok = -1, msg = groupmsg }, JsonRequestBehavior.AllowGet);
                    }
                    if (groupid > 0)
                    {
                        dbOrder.GroupId = groupid;
                        EntGoodsOrderBLL.SingleModel.Update(dbOrder, "groupid");
                    }

                    #endregion 是否开团

                    //【获取立减金，没有的话就传null】
                    Coupons reductionCart = CouponsBLL.SingleModel.GetVailtModel(aid);

                    //不同支付方式
                    switch (buyMode)
                    {
                        case (int)miniAppBuyMode.微信支付:
                            //判断订单金额是否小于0，如果小于0则不通过微信接口下单，订单直接支付完成
                            if (dbOrder.BuyPrice <= 0)
                            {
                                PayResult payresult = new PayResult();
                                new CityMordersBLL(payresult, citymorderModel).MiniappEntGoods(0, dbOrder);
                            }
                            else
                            {
                                #region CtiyModer 生成

                                string no = WxPayApi.GenerateOutTradeNo();

                                citymorderModel = new CityMorders
                                {
                                    OrderType = (int)ArticleTypeEnum.MiniappEnt,
                                    ActionType = (int)ArticleTypeEnum.MiniappEnt,
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
                                    CommentId = couponlogid,//用户优惠券记录ID
                                    MinisnsId = umodel.Id,// 订单aId
                                    TuserId = dbOrder.Id,//订单的ID
                                    ShowNote = $" {umodel.Title}购买商品支付{dbOrder.BuyPrice * 0.01}元",
                                    CitySubId = 0,//无分销,默认为0
                                    PayRate = 1,
                                    buy_num = 0, //无
                                    appid = appid,
                                };

                                int orderid = Convert.ToInt32(_cityMordersBLL.Add(citymorderModel));
                                citymorderModel.Id = orderid;
                                dbOrder.OrderId = orderid;
                                EntGoodsOrderBLL.SingleModel.Update(dbOrder, "orderid");

                                #endregion CtiyModer 生成
                            }

                            break;

                        case (int)miniAppBuyMode.储值支付:
                            //储值支付 扣除预存款金额并生成消费记录
                            if (payOrderBySaveMoneyUser(dbOrder, saveMoneyUser))
                            {
                                //新订单电脑语音提示
                                Utils.RemoveIsHaveNewOrder(umodel.Id);
                                //支付后续通知及打印
                                EntGoodsOrderBLL.SingleModel.AfterPayOrderBySaveMoney(dbOrder);
                                return Json(new { isok = 1, msg = "订单生成并支付成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id, TablesNo = dbOrder.TablesNo, reductionCart = reductionCart, reserveId = reserveId }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { isok = -1, msg = "订单支付失败", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id, reserveId = reserveId }, JsonRequestBehavior.AllowGet);
                            }
                        case (int)miniAppBuyMode.货到付款:
                            PayResult afterPayresult = new PayResult();
                            new CityMordersBLL(afterPayresult, citymorderModel).MiniappEntGoods(0, dbOrder);
                            break;
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
                    EntGoodsOrderBLL.SingleModel.Update(dbOrder);

                    #endregion 更新对外订单号及对应CityModer的ID

                    //生成订单操作日志
                    EntGoodsOrderLogBLL.SingleModel.Add(new ent.EntGoodsOrderLog() { GoodsOrderId = dbOrder.Id, UserId = userInfo.Id, LogInfo = $" 成功下单,下单金额：{dbOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now });
                    return Json(new { isok = 1, msg = "订单生成成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id, money = dbOrder.BuyPrice, TablesNo = dbOrder.TablesNo, reductionCart = reductionCart, reserveId = reserveId }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Json(new { isok = -1, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region 储值支付方式支付

        /// <summary>
        /// 储值支付 扣除预存款金额并生成消费记录
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        public bool payOrderBySaveMoneyUser(ent.EntGoodsOrder dbOrder, SaveMoneySetUser saveMoneyUser)
        {
            if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
            {
                return false;
            }
            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
            {
                return false;
            }

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

            TransactionModel tran = new TransactionModel();
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

            switch (dbOrder.GetWay)
            {
                case (int)miniAppOrderGetWay.到店自取:
                case (int)miniAppOrderGetWay.到店消费:
                    dbOrder.State = (int)MiniAppEntOrderState.待自取;

                    //4位随机取物码
                    System.Random Random = new System.Random();
                    dbOrder.TablesNo = Random.Next(0, 9999).ToString("0000");

                    break;

                case (int)miniAppOrderGetWay.商家配送:
                    dbOrder.State = (int)MiniAppEntOrderState.待发货;

                    break;
            }

            dbOrder.PayDate = DateTime.Now;
            tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney - {dbOrder.BuyPrice} where id =  {saveMoneyUser.Id} ; ");
            tran.Add($" update Entgoodsorder set state = {dbOrder.State},PayDate = '{dbOrder.PayDateStr}',TablesNo = '{dbOrder.TablesNo}',OrderNum = {dbOrder.OrderNum} where Id = {dbOrder.Id} and state = {(int)MiniAppEntOrderState.待付款 } ; ");

            //判断是否是拼团订单
            if (!EntGroupSponsorBLL.SingleModel.PayReturnUpdateGroupState(dbOrder.GroupId, dbOrder.aId, ref tran))
            {
                return false;
            }

            //达达配送，修改订单状态为待接单
            if (dbOrder.GetWay == (int)miniAppOrderGetWay.达达配送)
            {
                string dadamsg = _dadaOrderBLL.GetDadaOrderUpdateSql(dbOrder.Id, dbOrder.aId, (int)TmpType.小程序专业模板, ref tran);
                if (!string.IsNullOrEmpty(dadamsg))
                {
                    LogHelper.WriteInfo(this.GetType(), dadamsg);
                    return false;
                }
            }

            //记录订单支付日志
            tran.Add(EntGoodsOrderLogBLL.SingleModel.BuildAddSql(new EntGoodsOrderLog() { GoodsOrderId = dbOrder.Id, UserId = dbOrder.UserId, LogInfo = $" 专业版普通商品订单使用储值金额成功支付：{dbOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));
            return SaveMoneySetUserLogBLL.SingleModel.ExecuteTransaction(tran.sqlArray);
        }

        /// <summary>
        /// 使用储值支付
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult buyOrderbySaveMoney(string appid, string openid, int goodsorderid, int aid = 0)
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

            EntGoodsOrder dbOrder = EntGoodsOrderBLL.SingleModel.GetModel(goodsorderid);
            if (dbOrder == null)
            {
                return Json(new { isok = -1, msg = "订单不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (dbOrder.State != 0)
            {
                return Json(new { isok = -1, msg = "此订单不可进行支付" }, JsonRequestBehavior.AllowGet);
            }
            if (dbOrder.BuyMode != (int)miniAppBuyMode.储值支付)
            {
                return Json(new { isok = -1, msg = "支付方式并非储值支付" }, JsonRequestBehavior.AllowGet);
            }
            SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(umodel.AppId, userInfo.Id);
            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
            {
                return Json(new { isok = -1, msg = "支付金额不足,请更换支付方式" }, JsonRequestBehavior.AllowGet);
            }

            //进入支付流程
            if (payOrderBySaveMoneyUser(dbOrder, saveMoneyUser))
            {
                //【获取立减金，没有的话就传null】
                Coupons reductionCart = CouponsBLL.SingleModel.GetVailtModel(aid);

                //新订单电脑语音提示
                Utils.RemoveIsHaveNewOrder(umodel.Id);
                //成功支付下单后续操作
                EntGoodsOrderBLL.SingleModel.AfterPayOrderBySaveMoney(dbOrder);
                return Json(new { isok = 1, msg = "支付成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id, reductionCart = reductionCart }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = -1, msg = "支付失败", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion 储值支付方式支付

        /// <summary>
        /// 商城商品支付
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="type"></param>
        /// <param name="openId"></param>
        /// <returns></returns>
        [AuthLoginCheckXiaoChenXun]
        public ActionResult PayOrder(int orderid, int type, string openId,int aid=0)
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
                WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResultByCity(setting, order, WebConfigBLL.citynotify_url);

                //增加发送模板消息次数
                TemplateMsg_Miniapp.AddSendTranMessage(order.appid, openId, Convert.ToString(unifiedOrderResult.GetValue("prepay_id")), order.CommentId);

                //【获取立减金，没有的话就传null】
                Coupons reductionCart = CouponsBLL.SingleModel.GetVailtModel(aid);

                return ApiResult(true, "下单成功", jsApiPay.GetJsApiParametersnew(setting), new { reductionCart = reductionCart });
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return ApiResult(false, "下单异常", ex.Message);
            }
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="orderId"></param>
        /// <param name="State">OrderState</param>
        /// <returns></returns>
        public ActionResult updateMiniappGoodsOrderState(string appid, string openid, int orderId, int State, string attchData = null)
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

            EntGoodsOrder order = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
            if (order == null)
            {
                return Json(new { isok = -1, msg = "找不到订单" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (State == (int)MiniAppEntOrderState.已取消 && order.State > 0)
            {
                return Json(new { isok = -1, msg = $"订单状态为{Enum.GetName(typeof(MiniAppEntOrderState), order.State)},不可取消 " }, JsonRequestBehavior.AllowGet);
            }
            //if (State != (int)MiniAppEntOrderState.已取消 && State != (int)MiniAppEntOrderState.交易成功)
            //{
            //    return Json(new { isok = -1, msg = $"无此操作订单权限" }, JsonRequestBehavior.AllowGet);
            //}

            //不同状态相应内容处理
            int oldState = (int)MiniAppEntOrderState.待付款;
            switch (State)
            {
                case (int)MiniAppEntOrderState.已取消:
                    oldState = (int)MiniAppEntOrderState.待付款;
                    break;

                case (int)MiniAppEntOrderState.交易成功:
                    oldState = (int)MiniAppEntOrderState.待收货;
                    order.AcceptDate = DateTime.Now;
                    break;

                case (int)MiniAppEntOrderState.退货审核中:
                    oldState = order.State;
                    //退货订单申请
                    ReturnGoodsPost returnOrderInfo = System.Web.Helpers.Json.Decode<ReturnGoodsPost>(attchData);
                    if (returnOrderInfo == null)
                    {
                        //序列化失败
                        return Json(new { isok = -1, msg = $"请求退货参数格式异常" }, JsonRequestBehavior.AllowGet);
                    }

                    returnOrderInfo.OrderId = order.Id;
                    returnOrderInfo.ReturnAmount = order.BuyPrice;
                    if (!ReturnGoodsBLL.SingleModel.CheckInfoVailded(returnOrderInfo))
                    {
                        //无效枚举
                        return Json(new { isok = -1, msg = $"请提交有效的退货申请" }, JsonRequestBehavior.AllowGet);
                    }
                    if (!ReturnGoodsBLL.SingleModel.AddReturnOrder(returnOrderInfo))
                    {
                        return Json(new { isok = -1, msg = $"保存退货申请失败" }, JsonRequestBehavior.AllowGet);
                    }
                    break;

                case (int)MiniAppEntOrderState.退货中:
                    //退货订单发货
                    DeliveryUpdatePost deliveryInfo = System.Web.Helpers.Json.Decode<DeliveryUpdatePost>(attchData);
                    if (deliveryInfo == null)
                    {
                        //序列化失败
                        return Json(new { isok = -1, msg = $"请求物流信息参数格式异常" }, JsonRequestBehavior.AllowGet);
                    }
                    if (deliveryInfo == null ||
                        (string.IsNullOrWhiteSpace(deliveryInfo.DeliveryNo) ||
                         string.IsNullOrWhiteSpace(deliveryInfo.CompanyCode) ||
                         string.IsNullOrWhiteSpace(deliveryInfo.CompanyTitle) ||
                         string.IsNullOrWhiteSpace(deliveryInfo.ContactName) ||
                         string.IsNullOrWhiteSpace(deliveryInfo.ContactTel) ||
                         string.IsNullOrWhiteSpace(deliveryInfo.Address)))
                    {
                        //物流信息不完整
                        return Json(new { isok = -1, msg = $"请完整填写物流信息" }, JsonRequestBehavior.AllowGet);
                    }
                    //保存物流信息
                    bool result = DeliveryFeedbackBLL.SingleModel.AddEntOrderReturnFeed(order.Id, deliveryInfo.ContactName, deliveryInfo.ContactTel, deliveryInfo.Address, deliveryInfo.DeliveryNo, deliveryInfo.CompanyCode, deliveryInfo.CompanyTitle, deliveryInfo.Remark);
                    if (!result)
                    {
                        return Json(new { isok = -1, msg = $"保存物流信息失败" }, JsonRequestBehavior.AllowGet);
                    }
                    oldState = (int)MiniAppEntOrderState.待退货;
                    break;

                case (int)MiniAppEntOrderState.退换货成功:
                    oldState = (int)MiniAppEntOrderState.换货中;
                    break;

                default:
                    return Json(new { isok = -1, msg = $"无此操作订单权限" }, JsonRequestBehavior.AllowGet);
            }
            order.State = State;
            bool isSuccess = SqlMySql.ExecuteNonQuery(EntGoodsOrderBLL.SingleModel.connName, CommandType.Text, $" update EntGoodsOrder set State = {order.State},AcceptDate = '{order.AcceptDate.ToString("yyyy-MM-dd HH:mm:ss")}' where State = {oldState} and id = {order.Id} ") > 0;

            if (isSuccess)
            {
                switch (order.State)
                {
                    case (int)MiniAppEntOrderState.已取消:
                        EntGoodsOrderLogBLL.SingleModel.Add(new ent.EntGoodsOrderLog() { GoodsOrderId = order.Id, UserId = userInfo.Id, LogInfo = $" 用户 { userInfo.NickName } 于 {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 取消订单 ", CreateDate = DateTime.Now });

                        //发给用户取消通知
                        object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(order, SendTemplateMessageTypeEnum.专业版订单取消通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(order.UserId, SendTemplateMessageTypeEnum.专业版订单取消通知, TmpType.小程序专业模板, orderData);
                        break;

                    case (int)MiniAppEntOrderState.交易成功:
                        EntGoodsOrderLogBLL.SingleModel.Add(new ent.EntGoodsOrderLog() { GoodsOrderId = order.Id, UserId = userInfo.Id, LogInfo = $" 用户 { userInfo.NickName } 于 {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 确认收货 ", CreateDate = DateTime.Now });
                        //会员加消费金额
                        if (!VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "entpro", order.BuyPrice))
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + order.Id));
                        }

                        //加销量 --不在交易成功节点做累计,而是用户付款后即累加销量
                        List<EntGoodsCart> list = EntGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {order.Id} ");

                        //消费符合积分规则赠送积分
                        if (!ExchangeUserIntegralBLL.SingleModel.AddUserIntegral(userInfo.Id, umodel.Id, 0, order.Id))
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception("赠送积分失败" + order.Id));
                        }

                        //确认收货后 判断该订单购物车里面是否是分销产生的 如果购物车里的产品佣金比例不为零则需要操作分销相关的

                        try
                        {

                            #region 分销相关

                            List<EntGoodsCart> listEntGoodsCart = list;
                           EntGoodsOrderBLL.SingleModel.PayDistributionMoney(listEntGoodsCart, order);
                            #endregion 分销相关
                        }
                        catch (Exception ex)
                        {
                            log4net.LogHelper.WriteError(this.GetType(), ex);
                        }

                        break;

                    case (int)MiniAppEntOrderState.退货审核中:
                        //发送"退货请求审核"通知模板消息
                        TemplateMsg_Gzh.ReturnDeliveryTemplateMsgForEnt(order, MiniAppEntOrderState.退货审核中);
                        break;

                    case (int)MiniAppEntOrderState.退货中:
                        //发送"退货订单发货"通知模板消息
                        TemplateMsg_Gzh.ReturnDeliveryTemplateMsgForEnt(order, MiniAppEntOrderState.退货中);
                        break;

                    case (int)MiniAppEntOrderState.退换货成功:
                        //发送"退货订单发货"通知模板消息
                        TemplateMsg_Gzh.ReturnDeliveryTemplateMsgForEnt(order, MiniAppEntOrderState.退换货成功);
                        break;

                    default:
                        //记录订单操作日志(修改订单状态)
                        EntGoodsOrderLogBLL.SingleModel.Add(new EntGoodsOrderLog() { GoodsOrderId = order.Id, UserId = userInfo.Id, LogInfo = $" 用户 { userInfo.NickName } 于 {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 操作订单,订单操作后状态为：{Enum.GetName(typeof(OrderState), order.State)} ", CreateDate = DateTime.Now });
                        break;
                }

                return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
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
                return Json(new { isok = -1, msg = "购物车Id不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (goodCarIdStr.Substring(goodCarIdStr.Length - 1, 1) == ",")
            {
                goodCarIdStr = goodCarIdStr.Substring(0, goodCarIdStr.Length - 1);
            }

            List<EntGoodsCart> myCartList = EntGoodsCartBLL.SingleModel.GetMyCartById(goodCarIdStr);
            List<EntGoods> goodList = EntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", myCartList.Select(x => x.FoodGoodsId))}) and (State<0 || IsSell =0) ");

            if (goodList != null && goodList.Count > 0)
            {
                return Json(new { isok = -1, msg = "有无效商品" }, JsonRequestBehavior.AllowGet);
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
                return Json(new { isok = -1, msg = "有无效商品" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
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

            List<EntGoodsOrder> goodOrderList = EntGoodsOrderBLL.SingleModel.getListEntGoodsOrder(umodel.Id, userInfo.Id, State, pageSize, pageIndex);

            if (goodOrderList != null && goodOrderList.Any())
            {
                List<EntGoodsCart> goodOrderDtlList = EntGoodsCartBLL.SingleModel.GetList($" GoodsOrderId in ({string.Join(",", goodOrderList.Select(x => x.Id))}) ");
                if (goodOrderDtlList != null && goodOrderList.Any())
                {
                    List<EntGoods> goodList = EntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodOrderDtlList.Select(x => x.FoodGoodsId))}) ");
                    //判断商品是否已评论
                    GoodsCommentBLL.SingleModel.DealGoodsCommentState<EntGoodsCart>(ref goodOrderDtlList, umodel.Id, userInfo.Id, (int)EntGoodsType.普通产品, "FoodGoodsId", "GoodsOrderId");

                    goodOrderDtlList.ForEach(x =>
                    {
                        x.goodsMsg = goodList.Where(y => y.id == x.FoodGoodsId).FirstOrDefault();
                    });


                    goodOrderList.ForEach(order =>
                    {
                        goodOrderDtlList.Where(y => y.GoodsOrderId == order.Id).ToList().ForEach(c =>
                       {
                           c.Price = c.NotDiscountPrice > 0 ? c.NotDiscountPrice : c.Price;
                       });
                    });
                    string stateRemark = string.Empty;
                    

                    List<object> postdata = new List<object>();

                    goodOrderList.ForEach(order =>
                    {
                        stateRemark = Enum.GetName(typeof(MiniAppEntOrderState), order.State);
                        if (order.GetWay == 6 && order.State == 8)
                        {
                            stateRemark = "待消费";
                        }

                        postdata.Add(new
                        {
                            order.IsCommentting,
                            ReducedPrice = order.ReducedPrice,
                            orderId = order.Id,
                            orderNum = order.OrderNum,
                            order.GroupId,
                            goodList = goodOrderDtlList.Where(y => y.GoodsOrderId == order.Id).ToList(),
                            citymorderId = order.OrderId,
                            buyPrice = order.BuyPriceStr,
                            StateRemark = stateRemark,
                            reserveId = order.ReserveId,
                            state = order.State,
                            hasDelivery = DeliveryFeedbackBLL.SingleModel.IsDelivery(order),
                            //申请售后限制15天内
                            isCustomerServices = !(order.State == (int)MiniAppEntOrderState.交易成功 && (DateTime.Now - order.AcceptDate).Days > 15)
                        });
                    });
                    return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { isok = 1, msg = "成功", postdata = "" }, JsonRequestBehavior.AllowGet);
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
            EntGoodsOrder goodOrder = EntGoodsOrderBLL.SingleModel.GetModel($" Id = {orderId} and UserId = {userInfo.Id}");
            if (goodOrder == null)
            {
                return Json(new { isok = -1, msg = "订单信息不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrEmpty(goodOrder.attribute))
            {
                goodOrder.attrbuteModel = JsonConvert.DeserializeObject<EntGoodsOrderAttr>(goodOrder.attribute);
            }
            int groupstate = 0;
            string grouptime = "";//拼团结束时间
            //拼团
            if (goodOrder.GroupId > 0)
            {
                EntGroupSponsor sponsor = EntGroupSponsorBLL.SingleModel.GetModel(goodOrder.GroupId);
                if (sponsor == null)
                {
                    return Json(new { isok = -1, msg = "团信息不存在" }, JsonRequestBehavior.AllowGet);
                }
                groupstate = sponsor.State;
                grouptime = sponsor.EndDate.ToString("yyyy-MM-dd HH:mm:ss");
            }

            List<EntGoodsCart> goodOrderDtl = EntGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {goodOrder.Id} ");
            List<EntGoods> goodList = EntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodOrderDtl.Select(x => x.FoodGoodsId))}) ");

            List<object> listObj = new List<object>();
            string img = "https://wx.qlogo.cn/mmopen/vi_32/0CibyLpDpwibAMXGGich7N2twvqAWRPbT1SicnHGtUKtOmu8lQcfwqKkFg2feibMggGuk9Gd7CjQaLvYyNrAplFbu6w/0";
            if (!string.IsNullOrWhiteSpace(goodOrder.attribute))
            {
                try { goodOrder.attrbuteModel = JsonConvert.DeserializeObject<EntGoodsOrderAttr>(goodOrder.attribute); } catch { }
            }
            if (goodOrder.attrbuteModel?.flashDealId > 0 && goodOrder.attrbuteModel?.flashItemId > 0)
            {
                //秒杀订单价格不做特殊处理
            }
            else
            {
                goodOrderDtl.ForEach(x =>
                {
                    x.Price = x.NotDiscountPrice > 0 ? x.NotDiscountPrice : x.Price;
                });
            }
            foreach (var x in goodOrderDtl)
            {
                EntGoods entGoods = goodList.Where(y => y.id == x.FoodGoodsId).FirstOrDefault();
                if (entGoods != null)
                {
                    if (!string.IsNullOrEmpty(x.SpecImg))
                    {
                        img = x.SpecImg;
                    }
                    else
                    {
                        img = entGoods.img;
                    }
                }

                listObj.Add(new
                {
                    price = (x.Price * 0.01).ToString("0.00"),
                    goodImgUrl = img,
                    goodname = goodList.Where(y => y.id == x.FoodGoodsId).FirstOrDefault()?.name,
                    orderDtl = x
                });
            }

            string stateRemark = Enum.GetName(typeof(MiniAppEntOrderState), goodOrder.State);
            if(goodOrder.GetWay == 6 && goodOrder.State == 8)
            {
                stateRemark = "待消费";
            }

            var postdata = new
            {
                buyPrice = (goodOrder.BuyPrice * 0.01).ToString("0.00"),
                freightPrice = (goodOrder.FreightPrice * 0.01).ToString("0.00"),
                stateRemark = stateRemark,
                orderFriRemark = EntFreightTemplateBLL.SingleModel.GetModel(goodOrder.FreightTemplateId)?.Name,
                goodOrder = goodOrder,
                zqStoreName = goodOrder.attrbuteModel.zqStoreName,
                goodOrderDtl = listObj,

                groupstate = groupstate,//4待付款，1开团成功，2团购成功，-1成团失败,-4已过期(GroupState)
                groupendtime = grouptime,
            };

            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        #endregion 购物车/订单

        /// <summary>
        /// 根据话题ID获取直播信息
        /// </summary>
        /// <param name="tpid"></param>
        /// <returns></returns>
        public ActionResult GetLivePlay(int? tpid)
        {
            if (!tpid.HasValue || tpid.Value <= 0)
            {
                return Json(new { isok = false, msg = "请输入正确的话题ID" }, JsonRequestBehavior.AllowGet);
            }
            string result = string.Empty;
            try
            {
                result = HttpHelper.PostData("http://tliveapi.vzan.com/VZLive/GetLivePlay", "tpid=" + tpid);
            }
            catch (Exception ex)
            {
                result = "接口异常";
                LogHelper.WriteError(this.GetType(), ex);
            }
            return Content(result);
        }

        #endregion 行业版高级版

        public ActionResult SaveFeedback()
        {
            string name = Context.GetRequest("username", string.Empty); 
            string phone = Context.GetRequest("phone", string.Empty);
            int datasource = Context.GetRequestInt("source", 0);
            int type = Context.GetRequestInt("type", 5);
            if (string.IsNullOrEmpty(name))
            {
                return ApiResult(false, "请输入您的称呼");
            }
            if (string.IsNullOrEmpty(phone))
            {
                return ApiResult(false, "请输入您的手机号码");
            }
            if (!Regex.IsMatch(phone, @"^1\d{10}$"))
            {
                return ApiResult(false, "手机格式不正确");
            }
            Hfeedback model = new Hfeedback()
            {
                name = name,
                phone = phone,
                datasource = datasource,
                type = type,
                addtime = DateTime.Now
            };
            bool result = Convert.ToInt32(HfeedbackBLL.SingleModel.Add(model)) > 0;
            return ApiResult(result, result ? "发送成功" : "发送失败");
        }

        /// <summary>
        /// 快速支付
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="userId"></param>
        /// <param name="money">单位：分</param>
        /// <returns></returns>
        [AuthLoginCheckXiaoChenXun]
        public ActionResult PayByStoredvalue(string appid = "", int userId = 0, int money = 0, int aid = 0)
        {
            int payway = Context.GetRequestInt("payway", (int)miniAppBuyMode.储值支付);
            int couponid = Context.GetRequestInt("couponid", 0);
            int levelid = Context.GetRequestInt("levelid", 0);
            int discountType = Context.GetRequestInt("discountType", 0);////优惠类型 默认为0 表示可以使用会员折扣 1表示不能使用会员折扣
            if (string.IsNullOrEmpty(appid) || userId <= 0 || money < 0)
            {
                return ApiResult(false, "非法请求");
            }
            C_UserInfo user = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (user == null)
            {
                return ApiResult(false, "用户不存在");
            }

            string no = WxPayApi.GenerateOutTradeNo();
            int ordertype = (int)ArticleTypeEnum.MiniappStoredvaluePay;
            string shownote = "使用储值付款";
            if (payway == (int)miniAppBuyMode.微信支付)
            {
                ordertype = (int)ArticleTypeEnum.MiniappWXDirectPay;
                shownote = "使用微信付款";
            }
            CityMorders order = new CityMorders()
            {
                OrderType = ordertype,
                ActionType = 1,
                Addtime = DateTime.Now,
                payment_free = money,
                trade_no = no,
                Percent = 0,
                userip = WebHelper.GetIP(),
                FuserId = user.Id,
                Fusername = user.NickName,
                orderno = no,
                payment_status = 0,
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
                appid = appid,
                remark = "",
                OperStatus = 0,
                Tusername = "",
                Note = "",
            };
            CouponLog couponlogmodel = null;
            int calMoney = money;
            //【第一步】减去会员减去的部分
            VipLevel levelInfo = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");
            if (levelInfo != null && levelInfo.type == 1&& discountType==0)
            {
                order.payment_free = Convert.ToInt32(calMoney * (levelInfo.discount / 100f));
            }

            //【第二步】从支付金额中减去优惠券 优惠的部分
            if (couponid > 0)
            {
                string msg = "";
                calMoney = CouponLogBLL.SingleModel.GetCouponPrice_Cuzhi(couponid, order.payment_free, userId, ref msg, ref couponlogmodel);
                if (!string.IsNullOrEmpty(msg))
                {
                    return ApiResult(false, msg);
                }
                order.payment_free = calMoney;
            }

            order.ShowNote = $"{shownote}：{order.payment_free * 0.01}元";
            order.AttachPar = JsonConvert.SerializeObject(new
            {
                discountType= discountType,
                phone = user.TelePhone,
                coupon = couponlogmodel,
                payMoney = money,
                money_coupon = Context.GetRequest("money_coupon", ""),
                money_vip = Context.GetRequest("money_vip", ""),
                levelInfo
            });

            if (ordertype == (int)ArticleTypeEnum.MiniappStoredvaluePay)
            {
                SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(appid, userId);
                if (saveMoneyUser == null)
                {
                    return ApiResult(false, "请先充值");
                }
                if (saveMoneyUser.AccountMoney < order.payment_free)
                {
                    return ApiResult(false, "储值卡余额不足");
                }
            }

            int orderid = Convert.ToInt32(_cityMordersBLL.Add(order));
            if (orderid > 0)
            {
                order.Id = orderid;

                if (order.OrderType == (int)ArticleTypeEnum.MiniappStoredvaluePay || order.payment_free <= 0)
                {
                    PayResult result = new PayResult();
                    result.total_fee = order.payment_free;
                    result.result_code = "SUCCESS";
                    CityMordersBLL citybll = new CityMordersBLL(result, order);
                    if (!citybll.PayByStoredvalue(couponlogmodel))
                    {
                        return ApiResult(true, "支付失败", orderid);
                    }
                }

                //【获取立减金，没有的话就传null】
                Coupons reductionCart = CouponsBLL.SingleModel.GetVailtModel(aid);
                return ApiResult(true, "支付成功", new { orderid = orderid }, new { reductionCart = reductionCart });
            }
            else
            {
                return ApiResult(false, "支付失败，订单生成失败");
            }
        }

        [AuthLoginCheckXiaoChenXun]
        public ActionResult StoredvalueOrderInfo(int orderid = 0)
        {
            if (orderid <= 0)
            {
                return ApiResult(false, "非法请求");
            }
            CityMorders order = _cityMordersBLL.GetModel(orderid);
            if (order == null || order.Status != 1)
            {
                return ApiResult(false, "订单不存在！");
            }
            return ApiResult(true, "", new
            {
                order.orderno,
                order.payment_free,
                order.payment_free_fmt,
                order.ShowNote,
                order.payment_time_fmt,
            });
        }

        /// <summary>
        /// 查看预约信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodsSubscribeInfo()
        {
            int userId = Context.GetRequestInt("userId", 0);
            int goodsId = Context.GetRequestInt("goodsId", 0);
            EntUserForm form = EntUserFormBLL.SingleModel.GetGoodsSubscribeInfo(goodsId, userId);
            if (form == null)
            {
                return Json(new { isok = false, msg = "还没有预约此产品" });
            }
            return Json(new { isok = true, data = new { formId = form.id, formData = form.formdatajson, dealState = form.state } });
        }

        [AuthLoginCheckXiaoChenXun]
        public ActionResult AddReservation(string appid, string openid, string reserveTime, int? seats, string note, string userName, string contact, int editReserve = 0,
            string goodCarIdStr = null, string orderjson = null, int buyMode = (int)miniAppBuyMode.微信支付, int isgroup = 0, int groupid = 0, int goodtype = 0)
        {
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" });
            }

            //门店信息
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到店铺" });
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" });
            }

            store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
            if (!store.funJoinModel.reserveSwitch)
            {
                return Json(new { isok = -1, msg = "商家未开启预约服务" });
            }

            if (!seats.HasValue || seats.Value <= 0)
            {
                return Json(new { isok = -1, msg = "请选择预约人数" });
            }

            DateTime reservationTime = DateTime.MinValue;
            if (!DateTime.TryParse(reserveTime, out reservationTime))
            {
                return Json(new { isok = -1, msg = "请选择就餐时间" });
            }

            //过滤
            userName = StringHelper.ReplaceSqlKeyword(userName);
            contact = StringHelper.ReplaceSqlKeyword(contact);
            note = StringHelper.ReplaceSqlKeyword(note);

            if (string.IsNullOrWhiteSpace(userName))
            {
                return Json(new { isok = -1, msg = "请输入预约人姓名" });
            }
            if (string.IsNullOrWhiteSpace(contact))
            {
                return Json(new { isok = -1, msg = "请输入联系方式" });
            }

            //获取选中结算的购物车商品
            List<EntGoodsCart> shopCartItem = new List<EntGoodsCart>();
            if (!string.IsNullOrWhiteSpace(goodCarIdStr.Trim()))
            {
                string[] selelctCartIds = goodCarIdStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (selelctCartIds.Length == 0)
                {
                    return Json(new { isok = -1, msg = "获取商品失败" });
                }
                //获取全部
                shopCartItem = EntGoodsCartBLL.SingleModel.GetMyCartById(goodCarIdStr);
                //获取选中
                shopCartItem = shopCartItem.FindAll(item => selelctCartIds.Contains(item.Id.ToString()));
            }

            int reserveId = 0;

            //获取未完成订单
            FoodReservation unPayReservation = FoodReservationBLL.SingleModel.GetUnPayReservationEnt(umodel.Id, userInfo.Id);
            if (unPayReservation != null)
            {
                editReserve = unPayReservation.Id;
            }
            //更新
            if (editReserve > 0)
            {
                //编辑预约
                bool result = FoodReservationBLL.SingleModel.EditReserve(reserveId: editReserve, dinningTime: reservationTime, userName: userName, contact: contact, seats: seats.Value, note: note);
                reserveId = result ? editReserve : 0;
            }
            else
            {
                //新建预约
                reserveId = FoodReservationBLL.SingleModel.AddReserve(appid: umodel.Id, type: (int)miniAppReserveType.预约购物_专业版, userId: userInfo.Id, dinningTime: reservationTime, seats: seats.Value, note: note, userName: userName, contact: contact);
            }

            //调用购物车下单路由
            string shopCartItemIds = string.Join(",", shopCartItem.Select(item => item.Id));
            string orderJson = System.Web.Helpers.Json.Encode(new EntGoodsOrder
            {
                OrderType = (int)EntOrderType.预约订单,
                AccepterName = userName,
                AccepterTelePhone = contact,
                Address = "预约购物，到店自取",
            });

            string addressJson = JsonConvert.SerializeObject(new WxAddress
            {
                userName = userInfo.NickName,
                telNumber = userInfo.TelePhone,
                postalCode = userInfo.AreaCode.ToString(),
                detailInfo = userInfo.Address,
            });

            return addMiniappGoodsOrder(appid, openid, goodCarIdStr, orderJson, addressJson, buyMode, 0, reserveId);
        }

        [AuthCheckLoginSessionKey]
        public ActionResult GetReserveMenu(string appid, string openid, int reserveId)
        {
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return ApiResult(false, "请先授权");
            }

            //通过缓存获取门店信息
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return ApiResult(false, "用户找不到店铺不存在");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return ApiResult(false, "用户不存在");
            }

            List<EntGoodsCart> menus = FoodReservationBLL.SingleModel.GetEntItem(reserveId: reserveId, userId: userInfo.Id);
            List<object> convertModel = FoodReservationBLL.SingleModel.ConvertEntItemToAPI(menus);

            return ApiResult(true, "获取成功", convertModel);
        }

        [AuthCheckLoginSessionKey]
        public ActionResult GetReservation(string appid, string openid, int reserveId)
        {
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return ApiResult(false, "请先授权");
            }

            //通过缓存获取门店信息
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return ApiResult(false, "找不到店铺");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return ApiResult(false, "用户不存在");
            }

            FoodReservation reservation = new FoodReservation();
            if (reserveId <= 0)
            {
                return ApiResult(false, "非法参数");
            }
            else
            {
                reservation = FoodReservationBLL.SingleModel.GetModel(reserveId);
            }

            if (reservation.UserId != userInfo.Id || reservation.AId != umodel.Id)
            {
                return ApiResult(false, "非法请求");
            }

            return ApiResult(true, "获取成功", FoodReservationBLL.SingleModel.ConvertReservationModel(reservation));
        }

        [AuthCheckLoginSessionKey]
        public ActionResult CancelResevation(string appid, string openid, int reserveId)
        {
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return ApiResult(false, "请先授权");
            }

            //通过缓存获取门店信息
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return ApiResult(false, "找不到店铺");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return ApiResult(false, "用户不存在");
            }

            FoodReservation reservation = new FoodReservation();
            if (reserveId <= 0)
            {
                return ApiResult(false, "非法参数");
            }
            else
            {
                reservation = FoodReservationBLL.SingleModel.GetModel(reserveId);
            }

            if (reservation.UserId != userInfo.Id)
            {
                return ApiResult(false, "非法请求");
            }

            bool result = FoodReservationBLL.SingleModel.UpdateState(reservation, (int)MiniAppEntOrderState.退款审核中);

            return ApiResult(result, result ? "操作成功" : "操作失败");
        }

        [AuthCheckLoginSessionKey]
        public ActionResult GetReserveGoodClass(string appid, string openid)
        {
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return ApiResult(false, "请先授权");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return ApiResult(false, "用户不存在");
            }

            //通过缓存获取门店信息
            Store store = StoreBLL.SingleModel.GetModelByRid(umodel.Id);
            if (store == null)
            {
                return ApiResult(false, "找不到店铺");
            }

            store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);

            List<EntGoodType> goodTypes = EntGoodTypeBLL.SingleModel.GetListByIds(umodel.Id, store.funJoinModel.reserveClass.Trim(','));

            return ApiResult(true, "获取商品分类成功", goodTypes);
        }

        public ActionResult ClearGoodsCart()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {
                result.Msg = "参数错误 appid error";
                return Json(result);
            }
            string openid = Context.GetRequest("openId", string.Empty);
            if (string.IsNullOrEmpty(openid))
            {
                result.Msg = "参数错误 openid error";
                return Json(result);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            List<EntGoodsCart> myCarts = EntGoodsCartBLL.SingleModel.GetMyCart(umodel.Id, userInfo.Id);
            if (myCarts == null && myCarts.Count <= 0)
            {
                result.Msg = "购物车为空";
                return Json(result);
            }
            string ids = string.Join(",", myCarts.Select(cart => cart.Id));
            result.isok = EntGoodsCartBLL.SingleModel.DeleteByIds(ids);
            result.Msg = result.isok ? "操作成功" : "操作失败";
            return Json(result);
        }

        [AuthCheckLoginSessionKey]
        public ActionResult AddGoodsCartList(List<EntGoodsOrderPostData> goodsList)
        {
            Return_Msg_APP result = new Return_Msg_APP();
            string appId = Context.GetRequest("appid", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {
                result.Msg = "appid不能为空";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                result.Msg = "请先授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            string openId = Context.GetRequest("openId", string.Empty);
            if (string.IsNullOrEmpty(openId))
            {
                result.Msg = "参数错误 openId error";
                return Json(result);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsList == null || goodsList.Count <= 0)
            {
                result.Msg = "商品列表为空";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            string gids = string.Join(",", goodsList.Select(goods => goods.goodsId));
            List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(gids);
            if (entGoodsList == null || entGoodsList.Count <= 0)
            {
                result.Msg = "商品列表不存在";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            string goodsNames = string.Join("、", entGoodsList.Where(goods => !(goods.state == 1 && goods.tag == 1)).Select(goods => goods.name));
            if (!string.IsNullOrEmpty(goodsNames))
            {
                result.Msg = $"抱歉，商品：{goodsNames},已下架";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            foreach (var postGoods in goodsList)
            {
                postGoods.goodsInfo = entGoodsList.Where(goods => goods.id == postGoods.goodsId && goods.state == 1 && goods.tag == 1).FirstOrDefault();
                if (postGoods.goodsInfo == null)
                {
                    result.Msg = $"抱歉，商品：{postGoods.goodsName}不存在";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrWhiteSpace(postGoods.attrSpacStr))
                {
                    if (!postGoods.goodsInfo.GASDetailList.Any(x => x.id.Equals(postGoods.attrSpacStr)))
                    {
                        result.Msg = $"未找到商品:{postGoods.goodsName}";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            int orderType = Context.GetRequestInt("orderType", 0);
            List<int> cartIds = null;
            switch (orderType)
            {
                case (int)EntOrderType.外卖订单:
                    cartIds = TakeoutOrderInsertGoodsCart(goodsList, umodel.Id, userInfo.Id);
                    if (cartIds == null || cartIds.Count <= 0)
                    {
                        result.Msg = "商品添加到购物车失败";
                        return Json(result);
                    }
                    result.dataObj = new { cartIds };
                    break;

                default:
                    result.Msg = "参数错误 orderType error";
                    return Json(result);
            }
            result.isok = true;
            return Json(result);
        }

        /// <summary>
        /// 外卖订单加入购物车
        /// </summary>
        /// <param name="entGoodsList"></param>
        /// <param name="goodsList"></param>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        private List<int> TakeoutOrderInsertGoodsCart(List<EntGoodsOrderPostData> goodsList, int aid, int userId)
        {
            List<EntGoodsCart> GoodsCartList = new List<EntGoodsCart>();
            foreach (var data in goodsList)
            {
                int price = Convert.ToInt32(data.goodsInfo.price * 100);
                price = Convert.ToInt32(!string.IsNullOrWhiteSpace(data.attrSpacStr) ? data.goodsInfo.GASDetailList.First(x => x.id.Equals(data.attrSpacStr)).price * 100 : data.goodsInfo.price * 100);
                EntGoodsCart goodsCart = new EntGoodsCart
                {
                    GoodName = data.goodsInfo.name,
                    FoodGoodsId = data.goodsInfo.id,
                    SpecIds = data.attrSpacStr,
                    Count = data.qty,
                    Price = price,
                    SpecInfo = data.SpecInfo,
                    UserId = userId,
                    CreateDate = DateTime.Now,
                    State = 0,
                    aId = aid,
                    type = (int)EntGoodCartType.外卖
                };
                GoodsCartList.Add(goodsCart);
            }
            List<int> cartIds = EntGoodsCartBLL.SingleModel.AddGoodsCartList(GoodsCartList);
            return cartIds;
        }

        /// <summary>
        /// 获取订单物流信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetOrderDeliveryFeed(string appid, string openid, int orderid = 0, int deliverytype = 0)
        {
            if (orderid <= 0)
            {
                return GetJsonResult(Msg: "参数错误");
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return GetJsonResult(Msg: "请先授权");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return GetJsonResult(Msg: "用户不存在");
            }

            EntGoodsOrder order = EntGoodsOrderBLL.SingleModel.GetModelByAppIdAndId(appId: appid, orderId: orderid);
            if (order.UserId != userInfo.Id)
            {
                return GetJsonResult(Msg: "非法操作");
            }

            DeliveryFeedback deliveryFeed = null;
            switch (deliverytype)
            {
                case (int)DeliveryOrderType.专业版订单用户退货:
                    deliveryFeed = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId: order.Id, orderType: DeliveryOrderType.专业版订单用户退货);
                    break;

                case (int)DeliveryOrderType.专业版订单商家换货:
                    deliveryFeed = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId: order.Id, orderType: DeliveryOrderType.专业版订单商家换货);
                    break;

                default:
                    deliveryFeed = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId: order.Id, orderType: DeliveryOrderType.专业版订单商家发货);
                    break;
            }
            return GetJsonResult(isok: true, dataObj: deliveryFeed);
        }

        /// <summary>
        /// 获取订单物流信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetReutrnOrderInfo(string appid, string openid, int orderid = 0)
        {
            if (orderid <= 0)
            {
                return GetJsonResult(Msg: "参数错误");
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return GetJsonResult(Msg: "请先授权");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return GetJsonResult(Msg: "用户不存在");
            }

            EntGoodsOrder order = EntGoodsOrderBLL.SingleModel.GetModelByAppIdAndId(appId: appid, orderId: orderid);
            if (order.UserId != userInfo.Id)
            {
                return GetJsonResult(Msg: "非法操作");
            }

            ReturnGoods returnInfo = ReturnGoodsBLL.SingleModel.GetByOrderId(orderid);
            object infoViewModel = new
            {
                Id = returnInfo.Id,
                ApplyTime = returnInfo.AddTime.ToString(),
                Reason = returnInfo.Reason,
                Remark = returnInfo.Remark,
                ReturnAmount = returnInfo.ReturnAmount * 0.01,
                UploadPics = returnInfo.UploadPics,
                ReturnType = returnInfo.ReturnType,
            };

            List<EntGoodsCart> goods = ReturnGoodsBLL.SingleModel.GetGoodList(returnInfo);
            IEnumerable<object> goodsViewModel = goods.Select(good => new { Img = EntGoodsBLL.SingleModel.GetModel(good.FoodGoodsId)?.img, Name = good.GoodName, Count = good.Count, Attr = good.SpecInfo });
            return GetJsonResult(isok: true, dataObj: new { ReturnInfo = infoViewModel, GoodList = goodsViewModel, OrderState = order.State });
        }

        [AuthCheckLoginSessionKey]
        public ActionResult GetGoodTypeList(string appid = "", string ids = "")
        {
            if (string.IsNullOrEmpty(appid))
            {
                return GetJsonResult(Msg: "appid不能为空");
            }
            if (string.IsNullOrEmpty(ids))
            {
                return GetJsonResult(Msg: "分类不能为空");
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return GetJsonResult(Msg: "请先授权");
            }

            List<GoodTypeRelation> listGoodTypeRelation = EntGoodTypeBLL.SingleModel.GetGoodTypeList(umodel.Id, ids);

            return GetJsonResult(isok: true, Msg: "ok", code: "200", dataObj: listGoodTypeRelation);
        }

        /// <summary>
        /// 根据一级分类ID  parentId获取下面的二级分类
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="appid"></param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetSecondGoodTypeList(int parentId = 0, string appid = "")
        {
            if (string.IsNullOrEmpty(appid))
            {
                return GetJsonResult(Msg: "appid不能为空");
            }
            if (parentId <= 0)
            {
                return GetJsonResult(Msg: "一级分类ID错误");
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return GetJsonResult(Msg: "请先授权");
            }

            List<EntGoodType> listSecondGoodType = EntGoodTypeBLL.SingleModel.GetSecondGoodTypeList(umodel.Id, parentId);

            return GetJsonResult(isok: true, Msg: "ok", code: "200", dataObj: listSecondGoodType);
        }

        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
        /// <param name="order"></param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult addPayContentOrder(string appid = null, string openid = null, int? contentId = null, int? buyMode = (int)miniAppBuyMode.微信支付)
        {
            #region 基本验证

            if (string.IsNullOrEmpty(appid))
            {
                return GetJsonResult(isok: false, Msg: "appid不能为空");
            }

            if (!contentId.HasValue)
            {
                return GetJsonResult(isok: false, Msg: "contentId不能为空");
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return GetJsonResult(isok: false, Msg: "请先授权 umodel_null");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return GetJsonResult(isok: false, Msg: "用户不存在");
            }

            EntNews contentDetail = EntNewsBLL.SingleModel.GetModel(contentId.Value);
            if (contentDetail == null)
            {
                return GetJsonResult(Msg: "您购买的内容不存在，管理员已删除");
            }

            PayContent payContent = PayContentBLL.SingleModel.GetModel(contentDetail.paycontent);
            if (payContent == null)
            {
                return GetJsonResult(Msg: "您购买的内容，管理员没有设置付费");
            }

            #endregion 基本验证

            //会员等级信息
            VipRelation vipInfo = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id} and state>=0");
            VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={vipInfo.levelid} and state>=0");

            //支付信息
            PayContentPayment payInfo = PayContentBLL.SingleModel.GetVipDiscount(payContent, levelinfo);

            //支付方式
            SaveMoneySetUser saveMoneyUser = null;
            if (buyMode == (int)miniAppBuyMode.储值支付 || payInfo.PayAmount == 0)
            {
                saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(umodel.AppId, userInfo.Id);
                if (saveMoneyUser == null || saveMoneyUser.AccountMoney < payInfo.PayAmount)
                {
                    return GetJsonResult(Msg: "生成订单失败");
                }
            }

            //生成订单
            EntGoodsOrder order = EntGoodsOrderBLL.SingleModel.BuildOrderForPayContent(appid, umodel.Id, payContent.Id, userInfo, payInfo);
            //写入订单
            int newOrderId = 0;
            if (!int.TryParse(EntGoodsOrderBLL.SingleModel.Add(order).ToString(), out newOrderId))
            {
                return GetJsonResult(Msg: "生成订单失败");
            }
            order.Id = newOrderId;

            //生成并写入支付记录（未支付状态）
            int payRecordId = PaidContentRecordBLL.SingleModel.AddPayRecordEnt(order, contentDetail, payContent, payInfo);
            if (payRecordId == 0)
            {
                return GetJsonResult(Msg: "生成付费记录失败");
            }

            //生成订单操作日志
            Task.Factory.StartNew(() =>
            {
                EntGoodsOrderLogBLL.SingleModel.Add(new EntGoodsOrderLog()
                {
                    GoodsOrderId = order.Id,
                    UserId = userInfo.Id,
                    LogInfo = $" 成功下单,下单金额：{order.BuyPrice * 0.01} 元 ",
                    CreateDate = DateTime.Now
                });
            });

            //不同支付方式
            switch (buyMode)
            {
                case (int)miniAppBuyMode.微信支付:

                    #region CtiyModer 生成

                    string no = WxPayApi.GenerateOutTradeNo();
                    CityMorders payOrder = new CityMorders
                    {
                        orderno = no,
                        trade_no = no,
                        Addtime = DateTime.Now,
                        payment_free = order.BuyPrice,
                        Percent = 99,//不收取服务费
                        PayRate = 1,
                        OrderType = (int)ArticleTypeEnum.PayContent,
                        ActionType = (int)ArticleTypeEnum.PayContent,
                        payment_status = 0,
                        Status = 0,
                        userip = WebHelper.GetIP(),
                        FuserId = userInfo.Id,
                        Fusername = userInfo.NickName,
                        appid = appid,
                        MinisnsId = umodel.Id,// 订单aId
                        TuserId = payRecordId,//付费记录的ID
                        ShowNote = $"{userInfo.NickName}购买[{umodel.Title}]付费内容，支付{(order.BuyPrice * 0.01).ToString("0.00")}元",
                    };

                    int payOrderid = 0;
                    if (!int.TryParse(_cityMordersBLL.Add(payOrder).ToString(), out payOrderid))
                    {
                        return GetJsonResult(Msg: "生成支付订单失败");
                    }
                    payOrder.Id = payOrderid;
                    order.OrderId = payOrderid;
                    EntGoodsOrderBLL.SingleModel.Update(order, "orderid");

                    #endregion CtiyModer 生成

                    //判断订单金额是否小于0，如果小于0则不通过微信接口下单，订单直接支付完成
                    if (order.BuyPrice <= 0)
                    {
                        goto case (int)miniAppBuyMode.储值支付;
                    }
                    break;

                case (int)miniAppBuyMode.储值支付:
                    //储值支付 扣除预存款金额并生成消费记录
                    if (!payOrderBySaveMoneyUser(order, saveMoneyUser))
                    {
                        return GetJsonResult(Msg: "储值支付失败");
                    }
                    PaidContentRecord payRecord = PaidContentRecordBLL.SingleModel.GetModel(payRecordId);

                    if (PaidContentRecordBLL.SingleModel.UpdateToPay(payRecord))
                    {
                        Task.Factory.StartNew(() => { EntGoodsOrderBLL.SingleModel.UpdatePaidContenSuccess(payRecord); });
                    }
                    break;
            }

            //返回结果格式
            string returnMessage = buyMode == (int)miniAppBuyMode.储值支付 ? "支付成功" : "下单成功";
            object returnData = new
            {
                orderId = order.OrderId,
                dbOrder = order.Id,
                orgAmount = payInfo.OrgAmount * 0.01,
                payAmount = payInfo.PayAmount * 0.01,
                discount = payInfo.DiscountAmount * 0.01,
                discountMsg = payInfo.Info,
            };
            return GetJsonResult(isok: true, Msg: returnMessage, dataObj: returnData);
        }

        /// <summary>
        /// 获取秒杀列表数据
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="flashDealIds">秒杀ID数组</param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getFlashDeal(string appid = null, string openid = null, string flashDealIds = null)
        {
            if (string.IsNullOrWhiteSpace(flashDealIds))
            {
                return GetJsonResult(Msg: "参数为空_flashDealIds");
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return GetJsonResult(isok: false, Msg: "请先授权 umodel_null");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return GetJsonResult(isok: false, Msg: "用户不存在");
            }

            List<FlashDeal> flashDeals = FlashDealBLL.SingleModel.GetByAidAndFlashIds(umodel.Id, flashDealIds);
            object returnData = null;
            if (flashDeals?.Count > 0)
            {
                returnData = FlashDealBLL.SingleModel.FormatForEnt(flashDeals, userInfo.Id);
            }
            return GetJsonResult(isok: true, Msg: "获取成功", dataObj: returnData);
        }

        /// <summary>
        /// 产品详情
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getFlashItem(string appId = null, string openId = null, int? flashItemId = null, int? version = null)
        {
            if (!flashItemId.HasValue || flashItemId.Value <= 0 || string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(openId))
            {
                return GetJsonResult(Msg: "非法请求");
            }

            XcxAppAccountRelation xcxModel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxModel == null)
            {
                return GetJsonResult(Msg: "小程序不存在");
            }

            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userinfo == null)
            {
                return GetJsonResult(Msg: "用户不存在");
            }
            FlashDealItem flashItem = FlashDealItemBLL.SingleModel.GetModel(flashItemId.Value);
            if (flashItem == null)
            {
                return GetJsonResult(Msg: "秒杀商品不存在");
            }

            FlashDealPayInfo flashPay = FlashDealBLL.SingleModel.GetFlashDealPayment(flashItem, userinfo.Id);
            if (flashPay == null)
            {
                return GetJsonResult(Msg: "秒杀商品不存在");
            }

            EntGoods goodInfo = EntGoodsBLL.SingleModel.GetModel(flashItem.SourceId);
            if (goodInfo == null || goodInfo.state == 0)
            {
                return GetJsonResult(Msg: "原产品不存在或已删除");
            }

            //产品标签
            if (!string.IsNullOrEmpty(goodInfo.plabels))
            {
                //goodModel.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT group_concat(name order by sort desc) from entgoodlabel where id in ({goodModel.plabels})").ToString();
                goodInfo.plabelstr = EntGoodLabelBLL.SingleModel.GetEntGoodsLabelStr(goodInfo.plabels);
                goodInfo.plabelstr_array = goodInfo.plabelstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            //产品图片
            goodInfo.img_fmt = ImgHelper.ResizeImg(goodInfo.img, imgFormatDic["normal"][0], imgFormatDic["normal"][1]);
            //version2改动:slideimgs增加动态裁剪
            if (!string.IsNullOrEmpty(goodInfo.slideimgs) && version > 0)
            {
                goodInfo.slideimgs_fmt = string.Join("|", goodInfo.slideimgs.SplitStr(",").Select(p => ImgHelper.ResizeImg(p, imgFormatDic["normal"][0], imgFormatDic["normal"][1])).ToArray());
            }
            if (!string.IsNullOrEmpty(goodInfo.img))
            {
                goodInfo.img = goodInfo.img.Replace("http://vzan-img.oss-cn-hangzhou.aliyuncs.com", "https://i.vzan.cc/");
            }

            goodInfo = FlashDealItemBLL.SingleModel.GetFlashDealPrice(goodInfo, flashItem);
            if (goodInfo.GASDetailList?.Count > 0 && flashItem?.GetSpecs().Count > 0)
            {
                //显示秒杀规格最低价
                FlashItemSpec lowestPrice = flashItem.GetSpecs().OrderBy(spec => spec.DealPrice).First();
                goodInfo.discountPrice = float.Parse((lowestPrice.DealPrice * 0.01).ToString("0.00"));
                //goodInfo.originalPrice = lowestPrice.OrigPrice;
            }

            FlashDeal flashDeal = FlashDealBLL.SingleModel.GetModel(flashItem.DealId);
            object flashPayInfo = null;
            flashPayInfo = new
            {
                Id = flashItem.Id,
                canPay = flashPay.IsPay,
                amountLimit = flashDeal.AmountLimit,
                discount = flashItem.Discount / 100,
                descr = flashPay.Info,
                begin = flashDeal?.Begin,
                end = flashDeal?.End,
                flashDealId = flashPay.FlashDealId,
                isNotify = FlashDealItemBLL.SingleModel.CheckSubscribeMark(flashItem, userinfo.Id)
            };
            return GetJsonResult(isok: true, Msg: "获取成功", dataObj: new { goodInfo, flashPayInfo });
        }

        /// <summary>
        /// 新增秒杀提醒
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="flashItemId"></param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult AddFlashSubscribe(string appId = null, string openId = null, int? flashItemId = null)
        {
            if (!flashItemId.HasValue || flashItemId.Value <= 0)
            {
                return GetJsonResult(Msg: "参数异常");
            }

            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userinfo == null)
            {
                return GetJsonResult(Msg: "用户不存在");
            }
            FlashDealItem flashItem = FlashDealItemBLL.SingleModel.GetModel(flashItemId.Value);
            if (flashItem == null)
            {
                return GetJsonResult(Msg: "秒杀商品不存在");
            }
            bool result = SubscribeMessageBLL.SingleModel.AddByFlashItem(flashItem, userinfo, TmpType.小程序专业模板);
            if (result)
            {
                FlashDealItemBLL.SingleModel.AddSubscribeMark(flashItem, userinfo.Id);
            }
            return GetJsonResult(isok: result, Msg: result ? "订阅成功" : "订阅失败");
        }

        /// <summary>
        /// 新增扫码记录
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="flashItemId"></param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult AddQrCodeScanRecord(string appId = null, string openId = null, int? qrCodeId = null)
        {
            if (!qrCodeId.HasValue || qrCodeId.Value <= 0)
            {
                return GetJsonResult(Msg: "参数异常_qrCodeId");
            }

            XcxAppAccountRelation xcxModel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxModel == null)
            {
                return GetJsonResult(Msg: "小程序不存在");
            }
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userinfo == null)
            {
                return GetJsonResult(Msg: "用户不存在");
            }
            EntStoreCode qrCode = EntStoreCodeBLL.SingleModel.GetModel(qrCodeId.Value);
            if (qrCode == null || qrCode.AId != xcxModel.Id)
            {
                return GetJsonResult(isok: true);
            }
            bool result = UserTrackBLL.SingleModel.AddQrCodeEntry(userinfo.Id, qrCode.Id);
            return GetJsonResult(isok: result);
        }

        [HttpGet]
        public ActionResult GetOrderRecordCount(string appid, string openid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok =false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            int waitPayRecord = EntGoodsOrderBLL.SingleModel.GetEntGoodsOrderRecordCount(umodel.Id, userInfo.Id, (int)MiniAppEntOrderState.待付款);
            int waitSendRecord = EntGoodsOrderBLL.SingleModel.GetEntGoodsOrderRecordCount(umodel.Id, userInfo.Id, (int)MiniAppEntOrderState.待发货);
            int waitReceiveRecord = EntGoodsOrderBLL.SingleModel.GetEntGoodsOrderRecordCount(umodel.Id, userInfo.Id, (int)MiniAppEntOrderState.待收货);
            int afterSale = EntGoodsOrderBLL.SingleModel.GetEntGoodsOrderRecordCount(umodel.Id, userInfo.Id, (int)MiniAppEntOrderState.交易成功);
            return Json(new { isok = true, opostdata = new { waitPayRecord, waitSendRecord, waitReceiveRecord, afterSale } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取消订单申请
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost, AuthCheckLoginSessionKey]
        public ActionResult CancelOrder(int orderId=0)
        {
            Return_Msg_APP returnObj = new Return_Msg_APP();
            string msg = "";
            returnObj.isok = EntGoodsOrderBLL.SingleModel.ApplyReturnOrder(orderId, ref msg);
            returnObj.Msg = msg;

            return Json(returnObj);
        }
        /// <summary>
        /// 获取预约页面配置
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetSubscribePageSetting(int aid)
        {
            if (aid == 0)
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            //只有专业版才有店铺资料,故若搜索不到就给默认值
            Store store = StoreBLL.SingleModel.GetModelByRid(aid) ?? new Store();
            try
            {
                store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
            }
            catch (Exception)
            {
                store.funJoinModel = new StoreConfigModel();
            }
            //读取用户小程序资料
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModel(aid);
            OpenAuthorizerConfig config = new OpenAuthorizerConfig();
            if (xcxRelation != null)
            {
                config = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxRelation.AppId);
            }

            EntSetting setModel = EntSettingBLL.SingleModel.GetModel(aid);
            if (setModel == null)
            {
                return Json(new { isok = false, msg = "小程序没有设置页面" }, JsonRequestBehavior.AllowGet);
            }


            setModel.pages = EntSettingBLL.SingleModel.ResizeComsImage(setModel.pages);

            List<EntPage> pageModels = JsonConvert.DeserializeObject<List<EntPage>>(setModel.pages);
            EntPage page = null;
            if (pageModels != null && pageModels.Count > 0)
            {
                page = pageModels.Where(p => p.def_name == "产品预约").FirstOrDefault();
            }


            return Json(new { isok = true, page= JsonConvert.SerializeObject(page), store.funJoinModel }, JsonRequestBehavior.AllowGet);

        }
    }
}