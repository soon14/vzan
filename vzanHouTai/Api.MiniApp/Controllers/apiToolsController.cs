using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.CoreHelper;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utility;

namespace Api.MiniApp.Controllers
{
    public class apiToolsController : InheritController
    {
    }
    public class apiMiniAppToolsController : apiToolsController
    {
        private static readonly object CutPriceLocker = new object();


        private static readonly object lockOrder = new object();
        /// <summary>
        /// 实例化对象
        /// </summary>
        public apiMiniAppToolsController()
        {

        }


        /// <summary>
        /// 获取店铺是否开启砍价
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        public ActionResult GetBargainOpenState(string AppId = "")
        {
            if (string.IsNullOrEmpty(AppId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);


            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(AppId);
            if (r == null)
            {
                return Json(new { isok = false, msg = "未授权模板!" }, JsonRequestBehavior.AllowGet);
            }


            Store Store = StoreBLL.SingleModel.GetModelByRid(r.Id);
            if (Store == null)
                return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);

            int StoreId = Store.Id;
            var m = ToolsConfigBLL.SingleModel.GetModel($"StoreId={StoreId}");

            if (m != null)
            {
                return Json(new { isok = true, msg = "获取成功!", obj = m }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = false, msg = "请到后台开启砍价开关!" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 查找一个店铺的砍价活动 根据appId
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="IsEnd"></param>
        /// <param name="BargainType">0→电商版(默认) 1→专业版</param>
        /// ids 砍价商品id组合逗号分隔
        /// <returns></returns>
        public ActionResult GetBargainList(string appId = "", int pageIndex = 1, int pageSize = 10, int IsEnd = -1, int BargainType = 0)
        {
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            int StoreId = 0;
            if (BargainType > 0)
            {
                EntSetting ent = EntSettingBLL.SingleModel.GetModel(r.Id);
                if (ent == null)
                    return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);

                StoreId = ent.aid;
            }
            else
            {
                var Store = StoreBLL.SingleModel.GetModelByRid(r.Id);
                if (Store == null)
                    return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
                StoreId = Store.Id;
            }

            List<Bargain> listBargainTotal = new List<Bargain>();
            List<Bargain> listBargainNotEnd = new List<Bargain>();
            List<Bargain> listBargainNotStart = new List<Bargain>();
            List<Bargain> listBargainEnd = new List<Bargain>();
            List<Bargain> listBargain = new List<Bargain>();

            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            if (!string.IsNullOrEmpty(ids))
            {

                List<MySqlParameter> parameters = new List<MySqlParameter>();
                string wherestr = $" find_in_set(id,@ids)>0 and State<>-1 and IsDel<>-1 ";
                parameters.Add(new MySqlParameter("@ids", ids));
                listBargain = BargainBLL.SingleModel.GetListByParam(wherestr, parameters.ToArray(), 2000, 1, "*", " find_in_set( id,@ids)");
            }
            else
            {
                listBargain = BargainBLL.SingleModel.GetListByStoreId(StoreId, pageSize, pageIndex, IsEnd, BargainType);
            }

            if (listBargain?.Count > 0)
            {
                listBargain.ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.ImgUrl) && p.ImgUrl.IndexOf('@') == -1)
                    {
                        p.ImgUrl = p.ImgUrl;
                        p.ImgUrl_thumb = ImgHelper.ResizeImg(p.ImgUrl, 750, 400);
                    }

                    //判断是否已结束
                    if ((p.EndDate < DateTime.Now || p.RemainNum <= 0))
                    {
                        p.IsEnd = 1;
                        listBargainEnd.Add(p);
                    }
                    else if ((p.StartDate > DateTime.Now && p.RemainNum > 0))
                    {

                        p.IsEnd = 2;
                        listBargainNotStart.Add(p);
                    }
                    else
                    {

                        p.IsEnd = 0;
                        listBargainNotEnd.Add(p);
                    }
                    BargainBLL.SingleModel.Update(p, "IsEnd");

                });

                listBargainTotal.AddRange(listBargainNotEnd);
                listBargainTotal.AddRange(listBargainNotStart);
                listBargainTotal.AddRange(listBargainEnd);
            }
            else
            {
                return Json(new List<Bargain>(), JsonRequestBehavior.AllowGet);
            }

            return Json(listBargainTotal, JsonRequestBehavior.AllowGet);


        }

        /// <summary>
        ///  获取指定的砍价商品详情 根据商品砍价商品Id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="UserId">当前登录用户Id -1表示帮朋友砍价后进来的</param>
        /// <param name="Id">砍价商品Id</param>
        ///  /// <param name="BargainType">0→电商版(默认) 1→专业版</param>
        /// <returns></returns>
        public ActionResult GetBargain(string appId = "", int Id = 0, int UserId = 0, int BargainType = 0)
        {
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            int storeId = 0;
            if (BargainType > 0)
            {
                EntSetting ent = EntSettingBLL.SingleModel.GetModel(r.Id);
                if (ent == null)
                    return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);

                storeId = ent.aid;
            }
            else
            {
                Store store = StoreBLL.SingleModel.GetModelByRid(r.Id);
                if (store == null)
                    return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
                storeId = store.Id;
            }

            Bargain model = BargainBLL.SingleModel.GetModel($"StoreId={storeId} and Id={Id} and IsDel<>-1 and State<>-1");
            if (model == null)
            {
                return Json(new { isok = false, msg = "该活动已下架或者删除" }, JsonRequestBehavior.AllowGet);
            }
            model.ImgList = C_AttachmentBLL.SingleModel.GetListByCache(Id, (int)AttachmentItemType.小程序店铺砍价轮播图);
            model.DescImgList = C_AttachmentBLL.SingleModel.GetListByCache(Id, (int)AttachmentItemType.小程序店铺砍价详情图);

            //背景音乐
            var voiceModel = C_AttachmentBLL.SingleModel.GetModelByType(model.Id, (int)AttachmentItemType.小程序砍价背景音乐);
            if (voiceModel != null && !string.IsNullOrEmpty(voiceModel.thumbnail))
            {
                model.VoicePath = voiceModel.thumbnail;
            }

            //视频
            var attvideolist = C_AttachmentVideoBLL.SingleModel.GetListByCache(Id, (int)AttachmentVideoType.小程序砍价商品视频);
            if (attvideolist.Count > 0)
            {
                model.VideoPath = attvideolist[0].convertFilePath;
            }

            model.BargainUserNumber = BargainUserBLL.SingleModel.GetCount($"BId={Id}");//参与砍价人数

            //  该砍价商品领取记录排行
            List<BargainUser> listmsgr = new List<BargainUser>();
            bool haveCreatOrder = false;//表示当前用户对该产品是下个单未完成  下单后未完成的不能再进行自砍或者分享好友砍 要等该单完成交易后才可
            var curUserBargain = BargainUserBLL.SingleModel.GetModel($"BId={Id} and UserId={UserId} and state in(0,2,5,6,7)");
            if (curUserBargain != null)
            {
                listmsgr = BargainUserBLL.SingleModel.GetList($"BId={Id} and state>=0 and Id<>{curUserBargain.Id}", 9, 1, "*", "state desc,CurrentPrice asc,BuyTime asc");
                listmsgr.Add(curUserBargain);
                if (curUserBargain.CreateOrderTime != Convert.ToDateTime("0001-01-01 00:00:00"))
                {
                    haveCreatOrder = true;
                }
            }
            else
            {
                listmsgr = BargainUserBLL.SingleModel.GetList($"BId={Id} and state>=0", 10, 1, "*", "state desc,CurrentPrice asc,BuyTime asc");
            }

            if (listmsgr != null && listmsgr.Count > 0)
            {
                listmsgr = listmsgr.OrderByDescending(i => i.State).ThenBy(i => i.CurrentPrice).ThenBy(i => i.BuyTime).ToList();
                var userList = C_UserInfoBLL.SingleModel.GetList($"id in ({string.Join(",", listmsgr.Select(x => x.UserId))})");
                foreach (var item in listmsgr)
                {
                    var user = userList.FirstOrDefault(p => p.Id == item.UserId);//_userInfoBll.GetModel(item.UserId);
                    if (user != null)
                    {
                        item.ShopLogoUrl = user.HeadImgUrl;
                        item.ShopName = user.NickName;
                    }
                }
            }

            model.BargainUserList = listmsgr;

            var curLoginBargainUser = BargainUserBLL.SingleModel.GetModel($"UserId={UserId} and BId={Id} and State not in(-1,1,3,8)");
            List<BargainRecordList> resultList = new List<BargainRecordList>();

            //获取当前用户当前商品帮砍记录
            if (curLoginBargainUser != null)
            {
                resultList = BargainRecordListBLL.SingleModel.GetList($"BUId={curLoginBargainUser.Id} and BId={Id}", int.MaxValue, 1, "*", "Id desc");
                if (resultList != null && resultList.Count > 0)
                {
                    List<C_UserInfo> listUser = C_UserInfoBLL.SingleModel.GetList($" id in ({(string.Join(",", resultList.Select(x => x.BargainUserId)))})");
                    foreach (BargainRecordList item in resultList)
                    {
                        C_UserInfo user = listUser?.FirstOrDefault(p => p.Id == item.BargainUserId);   //_userInfoBll.GetModel(item.BargainUserId);
                        if (user != null)
                        {
                            item.UserLogo = user.HeadImgUrl;
                            item.UserName = user.NickName;
                        }
                    }
                }
            }

            model.BargainRecordUserList = resultList;
            model.ImgUrl_thumb = model.ImgUrl;// ImgHelper.ResizeImg(model.ImgUrl, 750, 750);
            return Json(new { isok = true, msg = "数据获取成功", obj = model, haveCreatOrder = haveCreatOrder }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 领取砍价商品 往砍价商品参与记录里添加一条记录
        /// </summary>
        /// <param name="UserId">砍价用户Id</param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddBargainUser(int UserId = 0, int Id = 0, string UserName = "", int BargainType = 0)
        {

            if (UserId <= 0 || Id <= 0)
                return Json(new { isok = false, msg = "参数错误" });

            Bargain bargain = BargainBLL.SingleModel.GetModel(Id);
            if (bargain == null)
            {
                return Json(new { isok = false, msg = "商品不存在" });
            }

            if (BargainType == 0)
            {
                ToolsConfig c = ToolsConfigBLL.SingleModel.GetModel($"StoreId={bargain.StoreId}");
                if (c.IsBargainOpen == 0)
                {
                    return Json(new { isok = false, msg = "未开启砍价！" });
                }
            }
            if (bargain.IsDel == -1)
            {
                return Json(new { isok = false, msg = "商品已经删除！" });
            }
            if (bargain.State < 0)
            {
                return Json(new { isok = false, msg = "商品状态有误！" });
            }
            if (bargain.StartDate > DateTime.Now)
            {
                return Json(new { isok = false, msg = "活动未开始！" });
            }
            if (bargain.EndDate < DateTime.Now)
            {
                return Json(new { isok = false, msg = "活动已结束！" });
            }
            if (bargain.RemainNum <= 0)
            {
                return Json(new { isok = false, msg = "商品已售罄！" });
            }

            //查找我是否有参与中的此砍价
            BargainUser myBargainUser = BargainUserBLL.SingleModel.GetModel($"BId={Id} and UserId={UserId} and State not in(-1,1,3,8)");
            if (myBargainUser != null)
            {
                return Json(new { isok = true, msg = "砍价单里已存在！", buid = myBargainUser.Id });
            }

            BargainUser BargainUser = new BargainUser();
            BargainUser.BId = Id;
            BargainUser.BName = bargain.BName;
            BargainUser.UserId = UserId;
            BargainUser.CurrentPrice = bargain.OriginalPrice;
            BargainUser.HelpNum = 0;
            BargainUser.StartDate = bargain.StartDate;
            BargainUser.EndDate = bargain.EndDate;
            BargainUser.CreateDate = DateTime.Now;
            BargainUser.State = 0;
            BargainUser.Name = UserName;
            BargainUser.aid = bargain.StoreId;
            var buid = Convert.ToInt32(BargainUserBLL.SingleModel.Add(BargainUser));
            if (buid <= 0)
            {
                return Json(new { isok = false, msg = "报名失败！" });
            }

            return Json(new { isok = true, msg = "请求成功", buid = buid });
        }


        /// <summary>
        /// 砍价
        /// </summary>
        /// <param name="buid">参与砍价记录Id 也就是领取砍价商品记录Id</param>
        /// <param name="BargainType">0→电商版(默认) 1→专业版</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult cutprice(int UserId = 0, int buid = 0, int BargainType = 0)
        {
            if (UserId <= 0 || buid <= 0)
                return Json(new { code = -1, msg = "参数错误" });
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(UserId);
            if (loginCUser == null)
                return Json(new { code = -1, msg = "登录过期！" });

            lock (CutPriceLocker)
            {
                BargainUser BUser = BargainUserBLL.SingleModel.GetModel(buid);
                if (BUser == null)
                {
                    return Json(new { code = -1, msg = "砍价信息有误！" });
                }
                Bargain Bargian = BargainBLL.SingleModel.GetModel(BUser.BId);
                if (Bargian == null || Bargian.IsDel == -1 || Bargian.State == -1)
                {
                    return Json(new { code = -1, msg = "商品不存在！" });
                }

                if (BargainType == 0)
                {
                    //电商版才判断是否开启 专业版在其它入口
                    ToolsConfig c = ToolsConfigBLL.SingleModel.GetModel($"StoreId={Bargian.StoreId}");
                    if (c.IsBargainOpen == 0)
                    {
                        return Json(new { isok = -1, msg = "未开启砍价！" });
                    }
                }

                if (BUser.State < 0)
                {
                    return Json(new { code = -1, msg = "砍价状态有误！" });
                }
                if (BUser.StartDate > DateTime.Now)
                {
                    return Json(new { code = -1, msg = "活动未开始！" });
                }
                if (BUser.EndDate < DateTime.Now)
                {
                    return Json(new { code = -1, msg = "活动已结束！" });
                }
                if (Bargian.RemainNum <= 0)
                {
                    return Json(new { code = -1, msg = "商品已售罄！" });
                }
                if (BUser.CurrentPrice <= Bargian.FloorPrice)
                {
                    return Json(new { code = -1, msg = "已砍至底价！" });
                }


                int cutType = 0;

                //自砍一刀
                if (BUser.UserId == UserId)
                {
                    var record = BargainRecordListBLL.SingleModel.GetModel($"BUId={BUser.Id} and BargainUserId={UserId} and CreateDate>NOW()-INTERVAL {Bargian.IntervalHour} HOUR");//SECOND 
                    if (record != null)
                    {

                        TimeSpan ts = record.CreateDate.AddHours(Bargian.IntervalHour).Subtract(DateTime.Now).Duration();
                        return Json(new { code = 0, msg = "自砍间隔时间为" + Bargian.IntervalHour + "秒，请耐心等候！", obj = Math.Round(ts.TotalHours, 2) });
                    }
                }
                else
                {
                    var record = BargainRecordListBLL.SingleModel.Exists($"BUId={BUser.Id} and BargainUserId={UserId}");
                    if (record)
                    {
                        return Json(new { code = 1, msg = "已帮他砍过了" });
                    }

                    cutType = 1;
                }

                // 生成支付订单后不能在砍
                BargainUser morders = BargainUserBLL.SingleModel.GetModel(BUser.Id);
                if (morders != null && morders.CreateOrderTime != Convert.ToDateTime("0001-01-01 00:00:00"))//下单了，不能帮砍了
                {
                    return Json(new { code = 1, msg = "已下单不能再砍" });
                }

                //随机减价
                Random ran = new Random();
                int amount = ran.Next(Bargian.ReduceMin, Bargian.ReduceMax);
                //是否减到底价
                if (BUser.CurrentPrice - amount <= Bargian.FloorPrice)
                {
                    amount = BUser.CurrentPrice - Bargian.FloorPrice;
                }
                BargainRecordList UserRecord = new BargainRecordList();
                UserRecord.BId = Bargian.Id;
                UserRecord.BUId = BUser.Id;
                UserRecord.BargainUserId = UserId;
                UserRecord.BargainUserName = loginCUser.NickName;
                UserRecord.Amount = amount;
                UserRecord.CreateDate = DateTime.Now;
                UserRecord.IpAddress = WebHelper.GetIP();
                if (Convert.ToInt32(BargainRecordListBLL.SingleModel.Add(UserRecord)) <= 0)
                {
                    return Json(new { code = -1, msg = "砍价异常" });
                }

                //更新当前价格
                BUser.CurrentPrice = BUser.CurrentPrice - amount;
                BUser.HelpNum++;
                var result = BargainUserBLL.SingleModel.Update(BUser, "CurrentPrice,HelpNum");
                if (!result)
                {
                    return Json(new { code = -1, msg = "砍价失败" });
                }

                return Json(new { code = 2, msg = "请求成功", price = (BUser.CurrentPrice * 0.01).ToString("0.00"), num = BUser.HelpNum, cutprice = UserRecord.AmountStr, isFriend = cutType, BargainedUserName = BUser.Name });
            }

        }

        /// <summary>
        /// 获取邀请砍价分享小程序码
        /// </summary>
        /// <param name="UserId">被帮助者</param>
        /// <param name="buid">砍价参与记录ID 也就是要帮砍的ID</param>
        /// <param name="AppId"></param>
        /// <returns></returns>
        public ActionResult GetShareCutPrice(int UserId = 0, int buid = 0, string AppId = "", int bId = 0)
        {
            if (UserId <= 0 || buid <= 0 || string.IsNullOrEmpty(AppId) || bId <= 0)
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(UserId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(AppId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            //OpenAuthorizerInfo openconfig = _openAuthorizerInfoBLL.GetModelByAppId(AppId);
            //if (openconfig == null)
            //{
            //    return Json(new { isok = false, msg = "未授权小程序" }, JsonRequestBehavior.AllowGet);
            //}
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={r.TId}");
            if (xcxTemplate == null)
            {
                return Json(new { isok = false, msg = "未找到小程序模板" }, JsonRequestBehavior.AllowGet);
            }

            string access_token = string.Empty;
            if (!XcxApiBLL.SingleModel.GetToken(r, ref access_token))
            {
                return Json(new { isok = false, msg = access_token });
            }
            //表示新增

            string page = "pages/bargaindetail/bargaindetail";
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                page = "pages/bargain/bargain";
            }

            string scene = $"{buid}_{bId}_1";
            string postData = Newtonsoft.Json.JsonConvert.SerializeObject(new { scene = scene, page = page, width = 200, auto_color = true, line_color = new { r = "0", g = "0", b = "0" } });
            string errorMessage = "";
            string qrCode = CommondHelper.HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token, postData,ref errorMessage);
            if (string.IsNullOrEmpty(qrCode))
            {
                return Json(new { isok = false, msg = $"获取二维码失败!{errorMessage}", access_token = access_token, qrcode = qrCode }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { isok = true, msg = "获取二维码成功!", qrcode = qrCode }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 测试调试用
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="buid"></param>
        /// <param name="AppId"></param>
        /// <param name="bId"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public ActionResult GetTestShareCutPrice(int UserId = 0, int buid = 0, string AppId = "", int bId = 0, string access_token = "")
        {
            string page = "pages/bargain/bargain";
            
            string scene = $"{buid}_{bId}_1";
            string postData = Newtonsoft.Json.JsonConvert.SerializeObject(new { scene = scene, page = page, width = 200, auto_color = true, line_color = new { r = "0", g = "0", b = "0" } });
            string errorMessage = "";
            string qrCode = CommondHelper.HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token, postData,ref errorMessage);
            if (string.IsNullOrEmpty(qrCode))
            {
                return Json(new { isok = false, msg = $"获取二维码失败!{errorMessage}", qrcode = qrCode }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = true, msg = "获取二维码成功!", qrcode = qrCode }, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 根据用户ID跟小程序AppId获取砍价记录
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="AppId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="BargainType">0→电商版(默认) 1→专业版</param>
        /// <returns></returns>
        public ActionResult GetBargainUserList(int UserId = 0, string AppId = "", int pageSize = 10, int pageIndex = 1, int State = -2, int BargainType = 0)
        {
            if (UserId <= 0 || string.IsNullOrEmpty(AppId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(UserId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(AppId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            int StoreId = 0;
            if (BargainType > 0)
            {
                EntSetting ent = EntSettingBLL.SingleModel.GetModel(r.Id);
                if (ent == null)
                    return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);

                StoreId = ent.aid;
            }
            else
            {
                Store store = StoreBLL.SingleModel.GetModelByRid(r.Id);
                if (store == null)
                    return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
                StoreId = store.Id;
            }

            string strWhere = $"b.StoreId={StoreId} and a.UserId={UserId}";
            if (State > -2)
            {
                if (State == 0 || State == 5)
                {
                    strWhere += $" and (a.State=0 or a.State=5)";
                }
                else if (State == 8 || State == -1 || State == 2 || State == 3 || State == 4)
                {
                    strWhere += $" and (a.State=8 or a.State=-1 or a.State=2 or a.State=3 or a.State=4)";
                }
                else
                {
                    strWhere += $" and a.State={State}";
                }
            }
            List<BargainUser> list = BargainUserBLL.SingleModel.GetJoinList(strWhere, pageSize, pageIndex, "CreateDate desc");
            //判断商品是否已评论
            GoodsCommentBLL.SingleModel.DealGoodsCommentState<BargainUser>(ref list, r.Id, loginCUser.Id, (int)EntGoodsType.砍价产品, "BId", "Id");
            return Json(new { isok = true, msg = "获取成功", obj = list }, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 根据砍价商品记录ID 获取帮砍记录
        /// </summary>
        /// <param name="buid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetBargainRecordList(int buid = 0, int pageSize = 10, int pageIndex = 1)
        {
            if (buid <= 0)
                return Json(new { isok = false, msg = "参数错误" });

            List<BargainRecordList> list = BargainRecordListBLL.SingleModel.GetList($"BUId={buid}", pageSize, pageIndex, "*", "CreateDate desc");
            if (list?.Count > 0)
            {
                List<C_UserInfo> listUser = C_UserInfoBLL.SingleModel.GetList($" id in ({(string.Join(",", list.Select(x => x.BargainUserId)))})");
                foreach (var item in list)
                {
                    C_UserInfo user = listUser?.FirstOrDefault(p => p.Id == item.BargainUserId);   //_userInfoBll.GetModel(item.BargainUserId);
                    if (user != null)
                    {
                        item.UserLogo = user.HeadImgUrl;
                        item.UserName = user.NickName;
                    }
                }
            }
            else
            {
                list = new List<BargainRecordList>();
            }
            return Json(new { isok = true, msg = "获取成功", obj = list }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 砍价单确认信息
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="buid">砍价领取记录Id</param>
        /// <returns></returns>
        public ActionResult GetBargainUser(int UserId = 0, int buid = 0)
        {
            if (UserId <= 0 || buid <= 0)
                return Json(new { isok = false, msg = "参数错误" });
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(UserId);
            if (loginCUser == null)
                return Json(new { code = 0, msg = "登录过期" });
            BargainUser _bargainUser = BargainUserBLL.SingleModel.GetModel($"Id={buid} and UserId={UserId} and State=0");
            if (_bargainUser == null)
            {
                return Json(new { isok = false, msg = "领取信息有误" }, JsonRequestBehavior.AllowGet);
            }
            Bargain bargain = BargainBLL.SingleModel.GetModel(_bargainUser.BId);
            if (bargain == null)
            {
                return Json(new { isok = false, msg = "商品信息有误" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = true, msg = "获取成功", obj = new { ImgUrl = bargain.ImgUrl, BName = bargain.BName, Freight = bargain.GoodsFreightStr, curPrcie = _bargainUser.CurrentPriceStr } }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 生成支付订单
        /// </summary>
        /// <param name="UserId">当前用户Id</param>
        /// <param name="buid">砍价商品领取记录Id</param>
        /// <param name="AppId">小程序AppId</param>
        /// <param name="address">收货地址</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddBargainOrder()
        {
            int UserId = Utility.IO.Context.GetRequestInt("UserId", 0);
            int buid = Utility.IO.Context.GetRequestInt("buid", 0);
            int PayType = Utility.IO.Context.GetRequestInt("PayType", 0);
            int BargainType = Utility.IO.Context.GetRequestInt("BargainType", 0);

            string AppId = Utility.IO.Context.GetRequest("AppId",string.Empty);
            string address = Utility.IO.Context.GetRequest("address", string.Empty);
            string Remark = Utility.IO.Context.GetRequest("Remark", string.Empty);

            int getWay = Utility.IO.Context.GetRequestInt("getWay", 0);//0表示快递配送 1表示到店自取 2到店消费
            string storeName = Utility.IO.Context.GetRequest("storeName", string.Empty);//到店消费或者自取的店铺名称

            if (UserId <= 0 || buid <= 0 || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(AppId))
                return Json(new { isok = false, msg = "参数错误" });
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(UserId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！" });

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(AppId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" });

            BargainUser _bargainUser = BargainUserBLL.SingleModel.GetModel($"Id={buid} and UserId={UserId} and State=0");
            if (_bargainUser == null)
            {
                return Json(new { isok = false, msg = "领取信息有误" });
            }


            int StoreId = 0;
            if (BargainType > 0)
            {
                EntSetting ent = EntSettingBLL.SingleModel.GetModel(r.Id);
                if (ent == null)
                    return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);

                StoreId = ent.aid;
            }
            else
            {
                Store Store = StoreBLL.SingleModel.GetModelByRid(r.Id);
                if (Store == null)
                    return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
                StoreId = Store.Id;
            }

            Bargain _bargain = BargainBLL.SingleModel.GetModel($"StoreId={StoreId} and Id={_bargainUser.BId}");
            if (_bargain == null)
            {
                return Json(new { isok = false, msg = "该活动已下架或者删除" });
            }
            if (_bargain.RemainNum <= 0)
            {
                return Json(new { isok = false, msg = "库存不足!" });
            }

            if (DateTime.Now > _bargainUser.EndDate)
            {
                return Json(new { isok = false, msg = "活动过期!" });
            }
            UserWxAddress addr = new UserWxAddress();
            if (getWay == 0)
            {
                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(UserWxAddressBLL.SingleModel.connName, CommandType.Text, $"SELECT * from userwxaddress where userid={UserId} order by id DESC LIMIT 0,1", null))
                {
                    if (dr.Read())
                        addr = UserWxAddressBLL.SingleModel.GetModel(dr);
                }
                if (addr != null)
                {
                    addr.WxAddress = address;
                }
                else
                {
                    addr = new UserWxAddress() { UserId = UserId, WxAddress = address };
                }

                if (UserWxAddressBLL.SingleModel.UpdateUserWxAddress(addr) <= 0)
                    return Json(new { isok = false, msg = "下单异常(地址出错)" });


            }

            if (!string.IsNullOrEmpty(StringHelper.ReplaceSqlKeyword(StringHelper.ReplaceSQLKey(Remark))))
                _bargainUser.Remark = Remark;

            //运费模板
            int freightFee = _bargain.GoodsFreight;
            string deliveryInfo = string.Empty;
            Store store = StoreBLL.SingleModel.GetModelByRid(r.Id);
            DeliveryConfig config = DeliveryConfigBLL.SingleModel.GetConfig(_bargain) ?? new DeliveryConfig();
            if (store != null && BargainType > 0 && config.Attr.Enable&&getWay==0)
            {
                //地址
                WxAddress addressFormat = null;
                try { addressFormat = JsonConvert.DeserializeObject<WxAddress>(addr.WxAddress); } catch { addressFormat = new WxAddress(); }
                //店铺配置
                StoreConfigModel storeConfig = null;
                try { storeConfig = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson); } catch { storeConfig = new StoreConfigModel(); }
                DeliveryFeeSumMethond sumRule = DeliveryFeeSumMethond.有赞;
                Enum.TryParse(storeConfig.deliveryFeeSumMethond.ToString(), out sumRule);
                //获取运费
                DeliveryFeeResult deliveryFee = deliveryFee = DeliveryTemplateBLL.SingleModel.GetDeliveryFee(_bargainUser, addressFormat.provinceName, addressFormat.cityName, sumRule, config);
                if (!deliveryFee.InRange)
                    return Json(new { isok = false, msg = deliveryFee.Message });
                freightFee = deliveryFee.Fee;
                deliveryInfo = $",{deliveryFee.Message}";
            }
            else
            {
                deliveryInfo = $",运费{freightFee * 0.01}元";
            }

            int payMoney = _bargainUser.CurrentPrice + freightFee;//实际支付费用=商品价格+运费

            lock (lockOrder)
            {
                //对外订单号规则：年月日时分 + 餐饮本地库ID最后3位数字
                string idStr = _bargainUser.Id.ToString();
                if (idStr.Length >= 3)
                {
                    idStr = idStr.Substring(idStr.Length - 3, 3);
                }
                else
                {
                    idStr.PadLeft(3, '0');
                }
                idStr = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{idStr}";
                string flied = string.Empty;
                if (PayType == (int)miniAppBuyMode.储值支付 || payMoney == 0)
                {
                    //储值支付
                    SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(r.AppId, UserId);
                    if (saveMoneyUser == null)
                    {
                        //用户储值账户,若无则开通一个
                        saveMoneyUser = new SaveMoneySetUser()
                        {
                            AppId = r.AppId,
                            UserId = loginCUser.Id,
                            AccountMoney = 0,
                            CreateDate = DateTime.Now
                        };
                        saveMoneyUser.Id = Convert.ToInt32(SaveMoneySetUserBLL.SingleModel.Add(saveMoneyUser));
                    }
                    if (saveMoneyUser.Id == 0)
                    {
                        return Json(new { isok = false, msg = "创建储值账号失败！ " });
                    }
                    if (saveMoneyUser == null || saveMoneyUser.AccountMoney < payMoney)
                    {
                        return Json(new { isok = -1, msg = $" 预存款余额不足！ " }, JsonRequestBehavior.AllowGet);
                    }
                   
                    TransactionModel tran = new TransactionModel();
                    tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
                    {
                        AppId = r.AppId,
                        UserId = UserId,
                        MoneySetUserId = saveMoneyUser.Id,
                        Type = -1,
                        BeforeMoney = saveMoneyUser.AccountMoney,
                        AfterMoney = saveMoneyUser.AccountMoney - payMoney,
                        ChangeMoney = payMoney,
                        ChangeNote = $"小程序砍价购买商品[{_bargainUser.BName}],订单号:{idStr}{deliveryInfo}",
                        CreateDate = DateTime.Now,
                        State = 1
                    }));

                    tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney - {payMoney} where id =  {saveMoneyUser.Id} ; ");/*  _miniappsavemoneysetuserBll.BuildUpdateSql(saveMoneyUser, "AccountMoney"));*/

                    _bargainUser.State = 7;//表示已经生成支付订单
                    _bargainUser.Address = address;
                    _bargainUser.OrderId = idStr;
                    _bargainUser.CreateOrderTime = DateTime.Now;
                    _bargainUser.PayType = (int)miniAppBuyMode.储值支付;
                    _bargainUser.BuyTime = DateTime.Now;
                    _bargainUser.FreightFee = freightFee;
                    _bargainUser.GetWay = getWay;
                    _bargainUser.StoreName = storeName;
                    //减砍价商品库存
                    _bargain.RemainNum--;
                    string cutRemainNum = BargainBLL.SingleModel.BuildUpdateSql(_bargain, "RemainNum");

                    flied = "GetWay,State,Address,OrderId,CreateOrderTime,PayType,FreightFee,BuyTime,Remark";
                    if (!string.IsNullOrEmpty(storeName) && getWay != 0)
                    {
                        flied += ",StoreName";
                    }
                    string updateBargainUser = BargainUserBLL.SingleModel.BuildUpdateSql(_bargainUser,flied);

                    tran.Add(cutRemainNum);
                    tran.Add(updateBargainUser);

                    //foreach (var x in tran.sqlArray)
                    //{
                    //    log4net.LogHelper.WriteInfo(GetType(), x);
                    //}
                    bool isok = BargainBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray);
                    if (isok)
                    {
                        //新订单电脑语音提示
                        Utils.RemoveIsHaveNewOrder(r.Id);

                        #region 发送 砍价订单支付成功通知 模板消息 => 通知用户
                        object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(_bargainUser, SendTemplateMessageTypeEnum.砍价订单支付成功提醒);
                        TemplateMsg_Miniapp.SendTemplateMessage(_bargainUser, SendTemplateMessageTypeEnum.砍价订单支付成功提醒, orderData);
                        #endregion

                        #region 发送砍价订单支付成功通知 模板消息
                        TemplateMsg_Gzh.SendBargainPaySuccessTemplateMessage(_bargainUser);
                        #endregion

                        return Json(new { isok = true, msg = "下单成功!" });//返回订单信息
                    }
                    else
                    {
                        return Json(new { isok = false, msg = "下单异常!" });//返回订单信息
                    }
                }
                else
                {
                    #region CtiyModer 生成
                    string no = WxPayApi.GenerateOutTradeNo();

                    CityMorders citymorderModel = new CityMorders
                    {
                        OrderType = (int)ArticleTypeEnum.MiniappBargain,
                        ActionType = (int)ArticleTypeEnum.MiniappBargain,
                        Addtime = DateTime.Now,
                        payment_free = payMoney,
                        trade_no = no,
                        Percent = 99,//不收取服务费
                        userip = WebHelper.GetIP(),
                        FuserId = UserId,
                        Fusername = loginCUser.NickName,
                        orderno = no,
                        payment_status = 0,
                        Status = 0,
                        Articleid = _bargainUser.BId,
                        CommentId = _bargainUser.Id,
                        MinisnsId = StoreId,//店铺Id
                        TuserId = _bargainUser.Id,//订单的ID
                                                  // ShowNote = $"[{_bargain.BName}]小程序商品砍价支付{payMoney * 0.01}元",
                        CitySubId = 0,//无分销,默认为0
                        PayRate = 1,
                        buy_num = 0, //无
                        appid = AppId,
                    };

                    var orderid = Convert.ToInt32(_cityMordersBLL.Add(citymorderModel));
                    if (orderid > 0)
                    {
                        #region 更新对外订单号及对应CityModer的ID

                        _bargainUser.State = 5;//表示已经生成支付订单
                        _bargainUser.Address = address;
                        _bargainUser.OrderId = idStr;
                        _bargainUser.CityMordersId = orderid;
                        _bargainUser.CreateOrderTime = DateTime.Now;
                        _bargainUser.PayType = (int)miniAppBuyMode.微信支付;
                        _bargainUser.FreightFee = freightFee;
                        _bargainUser.GetWay = getWay;
                        _bargainUser.StoreName = storeName;
                        //减砍价商品库存
                        _bargain.RemainNum--;

                        TransactionModel TranModel = new TransactionModel();
                        flied = "GetWay,State,Address,OrderId,CityMordersId,CreateOrderTime,PayType,FreightFee";
                        if (!string.IsNullOrEmpty(storeName) && getWay != 0)
                        {
                            flied += ",StoreName";
                        }
                        string cutRemainNum = BargainBLL.SingleModel.BuildUpdateSql(_bargain, "RemainNum");
                        string updateBargainUser = BargainUserBLL.SingleModel.BuildUpdateSql(_bargainUser, flied);

                        TranModel.Add(cutRemainNum);
                        TranModel.Add(updateBargainUser);

                        //foreach (var x in TranModel.sqlArray)
                        //{
                        //    log4net.LogHelper.WriteInfo(GetType(), x);
                        //}
                        bool isok = BargainBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray);
                        if (isok)
                        {
                            return Json(new { isok = true, msg = "下单成功!", orderId = orderid });//返回订单信息
                        }
                        else
                        {
                            return Json(new { isok = false, msg = "下单异常!", orderId = orderid });//返回订单信息
                        }

                        #endregion
                    }
                    else
                    {
                        return Json(new { isok = false, msg = "下单失败!" });
                    }


                    #endregion
                }
            }
        }


        /// <summary>
        /// 买家确认收货
        /// </summary>
        /// <param name="RId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ConfirmReceive(int buid = 0, int UserId = 0, string AppId = "")
        {
            if (UserId <= 0 || buid <= 0 || string.IsNullOrEmpty(AppId))
                return Json(new { isok = false, msg = "参数错误" });

            BargainUser BargainUser = BargainUserBLL.SingleModel.GetModel(buid);
            if (BargainUser == null)
            {
                return Json(new { isok = false, msg = "该活动已下架或者删除!" });
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(AppId);
            if (r == null)
            {
                return Json(new { isok = false, msg = "未授权小程序!" });
            }

            Bargain bargain = BargainBLL.SingleModel.GetModel($"Id={BargainUser.BId}");
            if (bargain == null)
            {
                return Json(new { isok = false, msg = "商品不存在!" });
            }

            int storeId = 0;
            string updateLeveType = string.Empty;
            switch (bargain.BargainType)
            {
                case 0:
                    Store store = StoreBLL.SingleModel.GetModelByRid(r.Id);
                    if (store == null)
                        return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
                    storeId = store.Id;
                    updateLeveType = "store";
                    break;
                case 1:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(r.Id);
                    if (ent == null)
                        return Json(new { isok = false, msg = "找不到该专业版" }, JsonRequestBehavior.AllowGet);
                    storeId = ent.aid;
                    updateLeveType = "entpro";
                    break;
            }

            if (BargainUser.UserId != UserId || bargain.StoreId != storeId)
            {
                return Json(new { isok = false, msg = "没有权限!" });
            }
            BargainUser.State = 8;//确认收货后,交易成功
            BargainUser.ConfirmReceiveGoodsTime = DateTime.Now;
            if (BargainUserBLL.SingleModel.Update(BargainUser, "State,ConfirmReceiveGoodsTime"))
            {


                if (!VipRelationBLL.SingleModel.updatelevel(UserId, updateLeveType, BargainUser.CurrentPrice))
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + buid));
                }

                return Json(new { isok = true, msg = "操作成功!" });
            }
            else
            {
                return Json(new { isok = false, msg = "操作异常!" });
            }
        }


        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="buid"></param>
        /// <param name="UserId"></param>
        /// <param name="AppId"></param>
        /// <returns></returns>
        public ActionResult GetOrderDetail(int buid = 0, int UserId = 0, string AppId = "")
        {
            if (UserId <= 0 || buid <= 0 || string.IsNullOrEmpty(AppId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);

            BargainUser bargainUser = BargainUserBLL.SingleModel.GetModel(buid);
            if (bargainUser == null)
                return Json(new { isok = false, msg = "该活动已下架或者删除" }, JsonRequestBehavior.AllowGet);

            Bargain bargain = BargainBLL.SingleModel.GetModel(bargainUser.BId);
            if (bargain == null)
                return Json(new { isok = false, msg = "商品不存在" }, JsonRequestBehavior.AllowGet);

            bargainUser.GoodsFreightStr = bargain.GoodsFreightStr;
            bargainUser.ImgUrl = bargain.ImgUrl;

            UserWxAddress WxAddress = new UserWxAddress() { UserId = bargainUser.UserId, WxAddress = bargainUser.Address };

            if (bargainUser.PayType == (int)miniAppBuyMode.储值支付)
            {
                bargainUser.BuyTimeStr = bargainUser.BuyTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                var _CityMorders = _cityMordersBLL.GetModel(bargainUser.CityMordersId);
                if (_CityMorders != null)
                {
                    bargainUser.CreatCityModerTimeStr = _CityMorders.Addtime.ToString("yyyy-MM-dd HH:mm:ss");
                    if (_CityMorders.payment_status == 1)
                    {
                        bargainUser.BuyTimeStr = bargainUser.BuyTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            }
            
            return Json(new { isok = true, msg = "获取成功", obj = new { OrderDetail = bargainUser, WxAddress = WxAddress } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取用户地址
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="AppId"></param>
        /// <returns></returns>
        public ActionResult GetUserWxAddress(int UserId = 0, string AppId = "")
        {
            if (UserId <= 0 || string.IsNullOrEmpty(AppId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);

            UserWxAddress model = new UserWxAddress();
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(UserWxAddressBLL.SingleModel.connName, CommandType.Text, $"SELECT * from userwxaddress where userid={UserId} order by id DESC LIMIT 0,1", null))
            {
                if (dr.Read())
                    model = UserWxAddressBLL.SingleModel.GetModel(dr);
            }
            
            return Json(new { isok = true, msg = "获取成功", obj = new { WxAddress = model } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 更新用户微信地址副本到到本地数据库
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="WxAddress"></param>
        /// <returns></returns>
        public ActionResult UpdateUserWxAddress(int UserId = 0, string WxAddress = "")
        {
            if (UserId <= 0 || string.IsNullOrEmpty(WxAddress))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);

            UserWxAddress model = UserWxAddressBLL.SingleModel.GetModel($"userid={UserId}");
            if (model == null)
            {
                model = new UserWxAddress
                {
                    UserId = UserId,
                    WxAddress = WxAddress,
                };
            }
            else
            {
                model.WxAddress = WxAddress;
            }

            int id = UserWxAddressBLL.SingleModel.UpdateUserWxAddress(model);

            return Json(new { isok = id > 0, msg = id > 0 ? "成功" : "失败", obj = id }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取物流公司列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDeliveryCompany()
        {
            List<DeliveryCompany> companys = DeliveryCompanyBLL.SingleModel.GetCompanys();
            return GetJsonResult(isok: true, Msg: "获取成功", dataObj: companys);
        }

        public ActionResult GetAddress(double u_lat = 0, double u_lng = 0)
        {
            AddressApi addressinfo = AddressHelper.GetAddressByApi(u_lng.ToString(), u_lat.ToString());
            return Json(new { addressinfo = addressinfo }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取小程序当前可用的优惠活动,只能有一个同时间段
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult GetFullReductionByAid(int aid = 0)
        {
            returnObj = new Return_Msg_APP();
            returnObj.isok = true;
            returnObj.code = "200";
            returnObj.dataObj = FullReductionBLL.SingleModel.GetFullReduction(aid);
            return Json(returnObj, JsonRequestBehavior.AllowGet);


        }


    }
}