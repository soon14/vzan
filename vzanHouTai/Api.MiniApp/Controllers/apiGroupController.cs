using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public class apiGroupController : InheritController
    {
    }
        [ExceptionLog]
    public class apiMiniAppGroupController : apiGroupController
    {
        private static readonly object lockOrder = new object();
        private static readonly object GroupLocker = new object();
        private static readonly object lockcancelpay = new object();
        private static readonly object CutPriceLocker = new object();
        
        /// <summary>
        /// 实例化对象
        /// </summary>
        public apiMiniAppGroupController()
        {
           
        }

        /// <summary>
        /// 获取首页拼团数据
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="state">0所有，1进行中，2结束</param>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetGroupListPage()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            //查询数量
            int length = Context.GetRequestInt("length", 10);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            Store store = StoreBLL.SingleModel.GetModelByRid(r.Id);
            if (store == null)
            {
                return Json(new { isok = false, msg = "还未开通店铺" }, JsonRequestBehavior.AllowGet);
            }
            //获取可以参加的拼团数据
            List<Groups> grouplist = GetCanJoinGroup(store.Id, length);
            if (grouplist?.Count > 0)
            {
                grouplist.ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.ImgUrl) && p.ImgUrl.IndexOf('@') == -1)
                    {
                        p.ImgUrl = Utility.ImgHelper.ResizeImg(p.ImgUrl, 750, 750);
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
                    //p.GroupsNum = p.CreateNum - p.RemainNum;
                });
            }
            else
            {
                return Json(new { isok = true, msg = "成功", postdata = new List<Object>() }, JsonRequestBehavior.AllowGet);
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
            });

            return Json(new { isok = true, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取店铺拼团列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="state">0所有，1进行中，2结束</param>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetGroupList()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            int state = Context.GetRequestInt("state", 1);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            Store store = StoreBLL.SingleModel.GetModelByRid(r.Id);
            if (store == null)
            {
                return Json(new { isok = false, msg = "还未开通店铺" }, JsonRequestBehavior.AllowGet);
            }
            List<Groups> grouplist = GroupsBLL.SingleModel.GetStoreGroupsList(store.Id, pageSize, pageIndex, state);
            if (grouplist?.Count > 0)
            {
                grouplist.ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.ImgUrl) && p.ImgUrl.IndexOf('@') == -1)
                    {
                        p.ImgUrl = Utility.ImgHelper.ResizeImg(p.ImgUrl,750,750);
                    }
                    
                    if ((p.ValidDateEnd < DateTime.Now || p.RemainNum <= 0))
                    {
                        //已结束
                        p.State = 2;
                    }
                    else if ((p.ValidDateStart < DateTime.Now && p.RemainNum > 0))
                    {
                        //进行中
                        p.State = 1;
                    }
                    else
                    {
                        p.State = -1;
                    }

                    //已团数量
                    p.GroupsNum = GroupUserBLL.SingleModel.GetCountPayGroup(p.Id);
                });
            }
            else
            {
                return Json(new { isok = true, msg = "成功", postdata = new List<Object>() }, JsonRequestBehavior.AllowGet);
            }

            var postgroupdata = grouplist.Select(s => new
            {
                s.CreateNum,
                s.RemainNum,
                s.DiscountPrice,
                s.UnitPrice,
                s.OriginalPrice,
                s.GroupName,
                s.GroupSize,
                s.GroupsNum,
                s.Id,
                s.ImgUrl,
                s.State,
                s.StoreId,
                ValidDateEnd = s.ValidDateEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                ValidDateStart = s.ValidDateStart.ToString("yyyy-MM-dd HH:mm:ss"),
            });

            return Json(new { isok = true, msg = "成功", postdata = postgroupdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取指定的拼团商品详情
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetGroupDetail()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int groupid = Context.GetRequestInt("groupId", 0);
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            if (groupid <= 0)
                return Json(new { isok = false, msg = "拼团商品Id不能小于0" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            Store store = StoreBLL.SingleModel.GetModelByRid(r.Id);
            if (store == null)
            {
                return Json(new { isok = false, msg = "还未开通店铺" }, JsonRequestBehavior.AllowGet);
            }

            Groups model = GroupsBLL.SingleModel.GetModel(groupid);
            if (model == null)
            {
                return Json(new { isok = false, msg = "拼团数据不存在" }, JsonRequestBehavior.AllowGet);
            }

            model.salesCount = GroupUserBLL.SingleModel.GetListByGroupId(model.Id).Sum(m => m.BuyNum);
            if (model.RemainNum > model.CreateNum)
            {
                int groupUserNum = model.salesCount;
                if (groupUserNum > model.CreateNum)
                {
                    groupUserNum = model.CreateNum;
                }
                model.RemainNum = model.CreateNum - groupUserNum;
                GroupsBLL.SingleModel.Update(model, "RemainNum");
            }

            List<string> userlogimg = new List<string>();
            //判断是否已结束
            if ((model.ValidDateEnd < DateTime.Now || model.RemainNum <= 0))
            {
                model.IsEnd = 1;
            }
            else if ((model.ValidDateStart < DateTime.Now && model.RemainNum > 0))
            {
                //判断是否开始
                model.IsEnd = 2;

                //购买该拼团用户记录
                model.GroupUserList = GroupUserBLL.SingleModel.GetListByGroupId(groupid);

                if (model.GroupUserList != null && model.GroupUserList.Count > 0)
                {
                    string userIds = string.Join(",",model.GroupUserList.Select(s=>s.ObtainUserId).Distinct());
                    List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                    foreach (GroupUser item in model.GroupUserList)
                    {
                        C_UserInfo userinfo = userInfoList?.FirstOrDefault(f=>f.Id == item.ObtainUserId);
                        if (userinfo != null)
                        {
                            userlogimg.Add(userinfo.HeadImgUrl == null ? "//j.vzan.cc/content/city/images/voucher/02.png" : userinfo.HeadImgUrl);
                        }
                        //取20条购买记录
                        if (userlogimg.Count >= 20)
                        {
                            break;
                        }
                    }
                }

                //获取当前可以参加的团
                model.GroupSponsorList = GetHaveSuccessGroup(groupid, 2);
            }
            else
            {
                model.IsEnd = 0;
            }

            model.ImgList = C_AttachmentBLL.SingleModel.GetListByCache( groupid , (int)AttachmentItemType.小程序拼团轮播图);
            model.ImgList.ForEach(p=> {
                p.thumbnail = Utility.ImgHelper.ResizeImg(p.filepath, 750, 750);
            });
            model.DescImgList = C_AttachmentBLL.SingleModel.GetListByCache( groupid, (int)AttachmentItemType.小程序拼团详情图);

            //视频
            List<C_AttachmentVideo> attvideolist = C_AttachmentVideoBLL.SingleModel.GetListByCache(groupid, (int)AttachmentVideoType.小程序拼团视频);
            if (attvideolist.Count > 0)
            {
                model.VideoPath = attvideolist[0].convertFilePath;
            }

            //可以参加的团
            var goupsponsordata = model.GroupSponsorList != null ? model.GroupSponsorList.Select(s => new {
                s.NeedNum,
                s.ShowEndTime,
                s.ShowStartTime,
                s.UserLogo,
                s.UserName,
                MState = s.State,
                s.GroupId,
                s.Id
            }) : null;
            
            var groupdetail = new
            {
                model.Id,
                model.CreateNum,
                model.RemainNum,
                model.LimitNum,
                model.DiscountPrice,
                model.UnitPrice,
                model.OriginalPrice,
                model.HeadDeduct,
                model.GroupName,
                model.GroupSize,
                UserDateEnd = model.UserDateEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                ValidDateEnd = model.ValidDateEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                ValidDateStart = model.ValidDateStart.ToString("yyyy-MM-dd HH:mm:ss"),
                model.VideoPath,
                model.State,
                model.IsEnd,
                GroupUserList = userlogimg,
                model.ImgUrl,
                model.ImgList,
                model.Description,
                ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                model.DescImgList,
                GroupSponsorList = goupsponsordata,
                model.salesCount,
                model.virtualSalesCount,
            };

            return Json(new { isok = true, msg = "数据获取成功", groupdetail = groupdetail }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取支付成功拼团详情
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetPaySuccessGroupDetail()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int gsid = Context.GetRequestInt("gsid", 0);
            int orderid = Context.GetRequestInt("orderid", 0);
            int paytype = Context.GetRequestInt("paytype", 0);
            Groups groupmodel = new Groups();

            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
           
            if (orderid <= 0)
                return Json(new { isok = false, msg = "订单Id不能小于0" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            //付款价格
            int payprice = 0;
            //判断是否是储值支付，储值支付没有在citymorders表中生成数据，而是直接在groupuser生成一条数据
            if (paytype != 1)
            {
                var ordermodel = _cityMordersBLL.GetModel(orderid);
                if (ordermodel == null)
                {
                    return Json(new { isok = false, msg = "订单不存在" }, JsonRequestBehavior.AllowGet);
                }
                payprice = ordermodel.payment_free;

                //单买
                if(gsid<=0)
                {
                    var guser = GroupUserBLL.SingleModel.GetModelByOrderId(orderid);
                    if (guser == null)
                    {
                        return Json(new { isok = false, msg = "拼团订单不存在" }, JsonRequestBehavior.AllowGet);
                    }
                    orderid = guser.Id;
                    groupmodel = GroupsBLL.SingleModel.GetModel(guser.GroupId);
                }
            }
            else
            {
                //储值支付
                var guser = GroupUserBLL.SingleModel.GetModel(orderid);
                if (guser == null)
                {
                    return Json(new { isok = false, msg = "订单不存在" }, JsonRequestBehavior.AllowGet);
                }
                payprice = guser.BuyPrice;
            }

            //判断是否为单买
            if (gsid <= 0)
            {
                if (groupmodel == null)
                {
                    return Json(new { isok = false, msg = "拼团商品不存在" }, JsonRequestBehavior.AllowGet);
                }

                var postdatasingel = new
                {
                    payprice = payprice,
                    Id= orderid,
                    groupmodel.ImgUrl,
                    groupmodel.OriginalPrice,
                };

                return Json(new { isok = true, msg = "数据获取成功", postdata = postdatasingel }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return GoToPintuan(gsid, paytype, orderid, payprice);
            }
        }
        private ActionResult GoToPintuan(int gsid,int paytype, int orderid,int payprice)
        {
            //拼团
            var groupsmodel = GroupSponsorBLL.SingleModel.GetModel(gsid);
            if (groupsmodel == null)
            {
                return Json(new { isok = false, msg = "参团数据不存在" }, JsonRequestBehavior.AllowGet);
            }

            //拼团商品
            Groups groupmodel = GroupsBLL.SingleModel.GetModel(groupsmodel.GroupId);
            //获取还需要几人才能成团
            groupsmodel.NeedNum = GroupUserBLL.SingleModel.GetDCount(groupsmodel.GroupId, groupsmodel.Id, groupsmodel.GroupSize);

            //用户购买拼团记录
            List<GroupUser> usergrouplog = GroupUserBLL.SingleModel.GetListGroupUserList(gsid);
            if (usergrouplog == null || usergrouplog.Count <= 0)
            {
                return Json(new { isok = false, msg = "参团用户数据不存在" }, JsonRequestBehavior.AllowGet);
            }

            //获取用户头像和下单用户id
            int userid = 0;
            List<C_UserInfo> userinfolist = GetUserInfo(usergrouplog, paytype, orderid, ref userid);
            if (userid <= 0)
            {
                return Json(new { isok = false, msg = "找不到下单用户" }, JsonRequestBehavior.AllowGet);
            }

            if (groupmodel == null)
            {
                return Json(new { isok = false, msg = "拼团商品不存在" }, JsonRequestBehavior.AllowGet);
            }

            var postdata = new
            {
                Id = userid,
                orderid= orderid,
                groupsmodel.GroupName,
                groupsmodel.NeedNum,
                GroupSponsorId = gsid,
                groupmodel.DiscountPrice,
                groupmodel.ImgUrl,
                payprice = payprice,
                ValidDateEnd = groupsmodel.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ValidDateStart = groupsmodel.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                GroupUserList = userinfolist?.Select(s => new {
                    s.Id,
                    s.HeadImgUrl,
                    s.NickName,
                    IsGroupHeader=s.zbSiteId,
                }),
            };

            return Json(new { isok = true, msg = "数据获取成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        ///  获取我的拼团列表
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetMyGroupList()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int t = Context.GetRequestInt("t", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);

            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            Store store = StoreBLL.SingleModel.GetModelByRid(r.Id);
            if (store == null)
            {
                return Json(new { isok = false, msg = "还未开通店铺" }, JsonRequestBehavior.AllowGet);
            }

            string where = $"a.ObtainUserId={userId} and c.id={store.Id} and a.appid='{r.AppId}' and a.state not in ({(int)MiniappPayState.取消支付},{(int)MiniappPayState.待支付})";
            if (0 == t)
            {
                where += $" and (g.state <> {(int)GroupState.已过期} or a.isgroup=0)";
            }//进行中
            else if (1 == t)
            {
                where += $" and g.State={(int)GroupState.开团成功} and g.EndDate>now()";
            }//已成功
            else if (2 == t)
            {
                where += $" and  (g.State={(int)GroupState.团购成功} or a.state={(int)MiniappPayState.已发货} or a.state={(int)MiniappPayState.已收货})";
            }//未成团
            else if (-1 == t)
            {
                where += $" and (g.State={(int)GroupState.开团成功} or g.State={(int)GroupState.成团失败}) and g.EndDate<now()";
            }
            List<GroupUser> resultList = GroupUserBLL.SingleModel.GetJoinList(where, 10, pageIndex, "Id desc");
            //判断团购是否已评论
            GoodsCommentBLL.SingleModel.DealGoodsCommentState<GroupUser>(ref resultList, r.Id, userId, (int)EntGoodsType.团购商品, "GroupId", "Id");
            var postdata = resultList?.Select(s => new
            {
                s.Id,
                ImgUrl = string.IsNullOrEmpty(s.ShopLogoUrl) ? "//j.vzan.cc/content/city/images/noshoplogo.png" : s.ShopLogoUrl,
                s.IsGroup,
                GroupName=s.Name,
                s.GroupSize,
                s.HaveNum,
                s.UnitPrice,
                s.DiscountPrice,
                s.OriginalPrice,
                s.BuyPrice,
                s.ShowDate,
                s.GroupSponsorId,
                s.GroupId,
                s.PState,
                s.State,
                s.MState,
                s.IsCommentting,
            });

            return Json(new { isok = true, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取我的拼团商品详情
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="UserId">当前登录用户Id -1表示帮朋友砍价后进来的</param>
        /// <param name="Id">砍价商品Id</param>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetMyGroupDetail()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int groupsponId = Context.GetRequestInt("groupsponId", 0);
            int userId = Context.GetRequestInt("userId", 0);

            if (userId <= 0)
            {
                return Json(new { isok = false, msg = "用户ID不能为0" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo loginuserinfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginuserinfo == null)
            {
                return Json(new { isok = false, msg = "用户不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            if (groupsponId <= 0)
                return Json(new { isok = false, msg = "拼团Id不能小于0" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);
            Store store = StoreBLL.SingleModel.GetModelByRid(r.Id);
            if (store == null)
            {
                return Json(new { isok = false, msg = "还未开通店铺" }, JsonRequestBehavior.AllowGet);
            }

            GroupSponsor smodel = GroupSponsorBLL.SingleModel.GetModel(groupsponId);
            if (smodel == null)
            {
                return Json(new { isok = false, msg = "参拼团数据不存在" }, JsonRequestBehavior.AllowGet);
            }

            Groups model = GroupsBLL.SingleModel.GetModel(smodel.GroupId);
            if (model == null)
            {
                return Json(new { isok = false, msg = "拼团数据不存在" }, JsonRequestBehavior.AllowGet);
            }
            model.ImgList = C_AttachmentBLL.SingleModel.GetListByCache(smodel.GroupId,(int)AttachmentItemType.小程序拼团轮播图);
            model.DescImgList = C_AttachmentBLL.SingleModel.GetListByCache(smodel.GroupId , (int)AttachmentItemType.小程序拼团详情图);

            //视频
            List<C_AttachmentVideo> attvideolist = C_AttachmentVideoBLL.SingleModel.GetListByCache(smodel.GroupId, (int)AttachmentVideoType.小程序拼团视频);
            if (attvideolist.Count > 0)
            {
                model.VideoPath = attvideolist[0].convertFilePath;
            }
            //获取当前团的所有成员
            List<GroupUser> groupUserList = GroupUserBLL.SingleModel.GetList($"GroupId={smodel.GroupId} and GroupSponsorId={smodel.Id} and IsGroup=1 and state not in ({(int)MiniappPayState.取消支付},{(int)MiniappPayState.待支付})");


            //获取还需要几人才能成团
            int NeedNum = GroupUserBLL.SingleModel.GetDCount(smodel.GroupId, smodel.Id, smodel.GroupSize);

            int mState = 0;
            List<C_UserInfo> userinfolist = new List<C_UserInfo>();
            if (groupUserList != null && groupUserList.Count > 0)
            {
                string userIds = string.Join(",",groupUserList.Select(s=>s.ObtainUserId).Distinct());
                List<C_UserInfo> userInfoListTemp = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                foreach (GroupUser GUInfo in groupUserList)
                {
                    //获取头像和昵称
                    C_UserInfo User = userInfoListTemp?.FirstOrDefault(f=>f.Id == GUInfo.ObtainUserId);
                    if (User != null)
                    {
                        User.zbSiteId = GUInfo.IsGroupHead;
                        if (string.IsNullOrEmpty(User.HeadImgUrl))
                        {
                            User.HeadImgUrl = GUInfo.HeadImgUrl;
                        }
                        if (User.Id == userId)
                        {
                            if (smodel.State == 2)
                            {
                                mState = 2;
                            }
                            else if (smodel.State == 1 && smodel.EndDate > DateTime.Now)
                            {
                                mState = 1;
                            }
                            else if (smodel.State == 0)
                            {
                                mState = 0;
                            }
                            else
                            {
                                mState = -1;
                            }
                        }

                        userinfolist.Add(User);
                    }
                }
            }

            //判断是否已结束
            if ((model.ValidDateEnd < DateTime.Now || model.RemainNum <= 0))
            {
                model.IsEnd = 1;
            }
            else if ((model.ValidDateStart > DateTime.Now && model.RemainNum > 0))
            {
                //判断是否开始
                model.IsEnd = 2;
            }
            else
            {
                model.IsEnd = 0;
            }
            var groupdetail = new
            {
                model.Id,
                model.CreateNum,
                model.RemainNum,
                model.LimitNum,
                model.DiscountPrice,
                model.UnitPrice,
                model.OriginalPrice,
                model.HeadDeduct,
                model.GroupName,
                ValidDateEnd = smodel.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ValidDateStart = smodel.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                smodel.GroupSize,
                model.VideoPath,
                smodel.State,
                mState,
                model.ImgUrl,
                model.ImgList,
                model.Description,
                model.DescImgList,
                NeedNum,
                GroupUserList = userinfolist?.Select(s => new {
                    s.Id,
                    s.HeadImgUrl,
                    s.NickName,
                    IsGroupHeader=s.zbSiteId,
                }),
            };

            return Json(new { isok = true, msg = "数据获取成功", groupdetail = groupdetail }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  添加拼团
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="UserId">当前登录用户Id -1表示帮朋友砍价后进来的</param>
        /// <param name="Id">砍价商品Id</param>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult AddGroup()
        {
            string jsondata = Context.GetRequest("jsondata", string.Empty);
            if (string.IsNullOrEmpty(jsondata))
            {
                return Json(new { isok = false, msg = "json参数错误", groupsponsor_id = -1 }, JsonRequestBehavior.AllowGet);
            }
            AddGroupModel groupjson = JsonConvert.DeserializeObject<AddGroupModel>(jsondata);
            if (groupjson == null)
            {
                return Json(new { isok = false, msg = "null参数错误", groupsponsor_id = -1 }, JsonRequestBehavior.AllowGet);
            }

            string msg = "";
            int groupsponsor_id = GroupsBLL.SingleModel.AddGroup(ref groupjson);

            return Json(new { isok = groupsponsor_id != -1, msg = msg, postdata = groupjson }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  拼团失败或取消支付(该方法取消调用，由服务执行，要不然出现冲突)
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="UserId">当前登录用户Id -1表示帮朋友砍价后进来的</param>
        /// <param name="Id">砍价商品Id</param>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult CancelPay()
        {
            return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);

            //int guid = Context.GetRequestInt("guid", 0);
            //string appId = Context.GetRequest("appId", string.Empty);
            //XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            //if (r == null)
            //    return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            //if (guid <= 0)
            //{
            //    return Json(new { isok = false, msg = "用户拼团记录ID不能为0" }, JsonRequestBehavior.AllowGet);
            //}

            //lock(lockcancelpay)
            //{
            //    GroupUser groupuser = _miniappgroupuserBll.GetModel(guid);
            //    if (groupuser == null)
            //    {
            //        return Json(new { isok = false, msg = "找不到用户拼团记录" }, JsonRequestBehavior.AllowGet);
            //    }

            //    if (groupuser.State == (int)MiniappPayState.取消支付)
            //    {
            //        return Json(new { isok = false, msg = "已取消支付" }, JsonRequestBehavior.AllowGet);
            //    }

            //    groupuser.State = (int)MiniappPayState.取消支付;
            //    if (!_miniappgroupuserBll.Update(groupuser, "State"))
            //    {
            //        return Json(new { isok = false, msg = "用户拼团记录状态更改失败" }, JsonRequestBehavior.AllowGet);
            //    }

            //    Groups group = _groupBLL.GetModel(groupuser.GroupId);
            //    if (group == null)
            //    {
            //        return Json(new { isok = false, msg = "找不到拼团商品" }, JsonRequestBehavior.AllowGet);
            //    }

            //    group.RemainNum = group.RemainNum + groupuser.BuyNum;
            //    if (!_groupBLL.Update(group, "RemainNum"))
            //    {
            //        return Json(new { isok = false, msg = "拼团商品库存更改失败" }, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        XcxTemplate xcxtemp = XcxTemplateBLL.SingleModel.GetModel(r.TId);
            //        if (xcxtemp != null)
            //        {
            //            //发给用户退款通知
            //            object groupData = TemplateMsg_Miniapp.GroupGetTemplateMessageData(string.Empty, groupuser, SendTemplateMessageTypeEnum.拼团基础版订单取消通知);
            //            TemplateMsg_Miniapp.SendTemplateMessage(groupuser, SendTemplateMessageTypeEnum.拼团基础版订单取消通知, xcxtemp.Type, groupData);
            //        }
            //    }
            //}
            
            
            //return Json(new { isok = true, msg = "成功"}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取获取邀请页面数据
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="UserId">当前登录用户Id -1表示帮朋友砍价后进来的</param>
        /// <param name="Id">砍价商品Id</param>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetInvitePageData()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int gsid = Context.GetRequestInt("gsid", 0);
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            if (gsid <= 0)
                return Json(new { isok = false, msg = "参团Id不能小于0" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            GroupSponsor groupsmodel = GroupSponsorBLL.SingleModel.GetModel(gsid);
            if (groupsmodel == null)
            {
                return Json(new { isok = false, msg = "参团数据不存在" }, JsonRequestBehavior.AllowGet);
            }
            //获取还需要几人才能成团
            groupsmodel.NeedNum = GroupUserBLL.SingleModel.GetDCount(groupsmodel.GroupId, groupsmodel.Id, groupsmodel.GroupSize);

            List<GroupUser> usergrouplog = GroupUserBLL.SingleModel.GetListGroupUserList(gsid);
            if (usergrouplog == null || usergrouplog.Count <= 0)
            {
                return Json(new { isok = false, msg = "参团用户数据不存在" }, JsonRequestBehavior.AllowGet);
            }

            List<C_UserInfo> userinfolist = new List<C_UserInfo>();
            string userIds = string.Join(",",usergrouplog.Select(s=>s.ObtainUserId).Distinct());
            List<C_UserInfo> userInfoListTemp = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
            foreach (GroupUser item in usergrouplog)
            {
                //获取头像和昵称
                C_UserInfo user = userInfoListTemp?.FirstOrDefault(f=>f.Id == item.ObtainUserId);
                if (user != null)
                {
                    user.zbSiteId = item.IsGroupHead;
                    if (string.IsNullOrEmpty(user.HeadImgUrl))
                    {
                        user.HeadImgUrl = "j.vzan.cc/content/city/images/voucher/10.jpg";
                    }

                    userinfolist.Add(user);
                }
            }

            Groups model = GroupsBLL.SingleModel.GetModel(groupsmodel.GroupId);
            if (model == null)
            {
                return Json(new { isok = false, msg = "拼团数据不存在" }, JsonRequestBehavior.AllowGet);
            }
            model.ImgList = C_AttachmentBLL.SingleModel.GetListByCache(groupsmodel.GroupId ,(int)AttachmentItemType.小程序拼团轮播图);
            model.DescImgList = C_AttachmentBLL.SingleModel.GetListByCache(groupsmodel.GroupId ,(int)AttachmentItemType.小程序拼团详情图);
            model.GroupsNum = GroupUserBLL.SingleModel.GetCountPayGroup(model.Id);
         
            //开团成功
            if (groupsmodel.State == 1)
            {

            }
            else if (groupsmodel.State == 2)
            {
                //团购成功
                //获取当前可以参加的团
                model.GroupSponsorList = GetHaveSuccessGroup(model.Id, 2);
            }
            else if (groupsmodel.State == -1 || groupsmodel.EndDate < DateTime.Now)
            {
                //拼团失败,超过24小时未拼团成功
                groupsmodel.State = -1;
                //获取当前可以参加的团
                model.GroupSponsorList = GetHaveSuccessGroup(model.Id, 2);
            }

            //拼团结束
            if (model.ValidDateEnd < DateTime.Now)
            {
                groupsmodel.State = -2;
                //获取可以参加的拼团数据
                model.GroupsList = GetCanJoinGroup(model.StoreId, 2);

            }

            //可以参加的团
            var goupsponsordata = model.GroupSponsorList?.Select(s => new {
                s.NeedNum,
                s.ShowEndTime,
                s.ShowStartTime,
                s.UserLogo,
                s.UserName,
                s.GroupId
            });

            //其他拼团商品数据
            var canjoindata = model.GroupsList?.Select(s => new
            {
                s.DiscountPrice,
                s.UnitPrice,
                s.OriginalPrice,
                s.GroupName,
                s.GroupSize,
                s.Id,
                s.ImgUrl,
                s.GroupsNum,
                s.StoreId,
            });

            var postdata = new
            {
                model.ImgList,
                model.ImgUrl,
                model.GroupName,
                model.GroupSize,
                model.GroupsNum,
                model.DiscountPrice,
                model.UnitPrice,
                model.OriginalPrice,
                model.Description,
                model.DescImgList,
                GroupSponsorList = goupsponsordata,
                Canjoindata = canjoindata,
                GroupSponsorId = groupsmodel.Id,
                MState = groupsmodel.State,
                groupsmodel.NeedNum,
                Id = groupsmodel.GroupId,
                ValidDateEnd = groupsmodel.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ValidDateStart = groupsmodel.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                GroupUserList = userinfolist?.Select(s => new {
                    s.Id,
                    s.HeadImgUrl,
                    s.NickName,
                    IsGroupHeader=s.zbSiteId,
                }),
            };

            return Json(new { isok = true, msg = "数据获取成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取订单详情
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetOrderDetail()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int guid = Context.GetRequestInt("guid", 0);
            if (string.IsNullOrEmpty(appId))
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            if (guid <= 0)
                return Json(new { isok = false, msg = "订单Id不能小于0" }, JsonRequestBehavior.AllowGet);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
                return Json(new { isok = false, msg = "小程序未授权" }, JsonRequestBehavior.AllowGet);

            GroupUser groupumodel = GroupUserBLL.SingleModel.GetModel(guid);
            if (groupumodel == null)
            {
                return Json(new { isok = false, msg = "订单数据不存在" }, JsonRequestBehavior.AllowGet);
            }

            Groups group = GroupsBLL.SingleModel.GetModel(groupumodel.GroupId);
            if (group == null)
            {
                return Json(new { isok = false, msg = "拼团商品不存在" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModel(group.StoreId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
            }

            string orderNo = "";
            if (groupumodel.PayType==0)
            {
                CityMorders citymorder = _cityMordersBLL.GetModel(groupumodel.OrderId);
                if (citymorder == null)
                {
                    return Json(new { isok = false, msg = "订单不存在" }, JsonRequestBehavior.AllowGet);
                }
                orderNo = citymorder.orderno;
            }
            else
            {
                orderNo = groupumodel.OrderNo;
            }

            GroupSponsor groupspormodel = new GroupSponsor();
            //如果是拼团查找拼团信息
            if (groupumodel.IsGroup==1)
            {
                groupspormodel = GroupSponsorBLL.SingleModel.GetModel(groupumodel.GroupSponsorId);
                if(groupspormodel==null)
                {
                    return Json(new { isok = false, msg = "拼团不存在" }, JsonRequestBehavior.AllowGet);
                }
            }

            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(groupumodel.ObtainUserId);
            var postdata = new
            {
                groupumodel.Address,
                groupumodel.BuyPrice,
                groupumodel.PayType,
                groupumodel.Note,
                isheader = groupspormodel.SponsorUserId == groupumodel.ObtainUserId,
                CreateDate = groupumodel.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                SendGoodTime = groupumodel.SendGoodTime.ToString("yyyy-MM-dd HH:mm:ss"),
                RecieveGoodTime = groupumodel.RecieveGoodTime.ToString("yyyy-MM-dd HH:mm:ss"),
                PayTime = groupumodel.PayTime.ToString("yyyy-MM-dd HH:mm:ss"),
                groupumodel.StorerRemark,
                groupumodel.State,
                isGroup = groupumodel.IsGroup,
                num = groupumodel.BuyNum,
                username = groupumodel.UserName,
                phone = groupumodel.Phone,
                group.HeadDeduct,
                group.ImgUrl,
                group.DiscountPrice,
                group.OriginalPrice,
                group.GroupName,
                storemobile = store.TelePhone,
                orderno = orderNo,
            };

            return Json(new { isok = true, msg = "数据获取成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取邀请拼团分享小程序码
        /// </summary>
        /// <param name="buid">参团ID</param>
        /// <param name="AppId"></param>
        /// <param name="UserId">分享者ID</param>
        /// <returns></returns>
        public ActionResult GetShareCutPrice(int guid, string AppId, int UserId)
        {
            if (guid <= 0 || string.IsNullOrEmpty(AppId) || UserId <= 0)
                return Json(new { isok = false, msg = "参数错误", JsonRequestBehavior.AllowGet });
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(UserId);
            if (loginCUser == null)
                return Json(new { isok = false, msg = "登录过期！", JsonRequestBehavior.AllowGet });
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(AppId); 
            if(xcxrelation==null)
            {
                return Json(new { isok = false, msg = "模板权限无效！", JsonRequestBehavior.AllowGet });
            }
            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcxrelation, ref token))
            {
                return Json(new { isok = false, msg = token, JsonRequestBehavior.AllowGet });
            }
            //  log4net.LogHelper.WriteInfo(GetType(), r.AppId+"===token:"+access_token);
            //表示新增


            string scene = $"{guid}";
            string postData = Newtonsoft.Json.JsonConvert.SerializeObject(new { scene = scene, path = "pages/bargaindetail/bargaindetail", width = 200, auto_color = true, line_color = new { r = "0", g = "0", b = "0" } });
            string errorMessage = "";
            string qrCode = CommondHelper.HttpPostSaveImg("https://api.weixin.qq.com/wxa/getwxacode?access_token=" + token, postData,ref errorMessage);
            if (string.IsNullOrEmpty(qrCode))
            {
                return Json(new { isok = false, msg = $"获取失败!{errorMessage}", JsonRequestBehavior.AllowGet });

            }
            return Json(new { isok = true, msg = "获取二维码成功!", qrcode = qrCode, JsonRequestBehavior.AllowGet });

        }

        /// <summary>
        /// 收货
        /// </summary>
        /// <param name="buid">参团ID</param>
        /// <param name="AppId"></param>
        /// <param name="UserId">分享者ID</param>
        /// <returns></returns>
        public ActionResult RecieveGoods(int guid)
        {
            GroupUser groupuser = GroupUserBLL.SingleModel.GetModel(guid);
            if(groupuser==null)
            {
                return Json(new { isok=false,msg="没找到拼团订单"},JsonRequestBehavior.AllowGet);
            }

            groupuser.State = (int)MiniappPayState.已收货;
            groupuser.RecieveGoodTime = DateTime.Now;
            Groups groupgoods = GroupsBLL.SingleModel.GetModel(groupuser.GroupId);
            if(groupgoods==null)
            {
                return Json(new { isok = false, msg = "没有找到拼团商品,请刷新重试" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModel(groupgoods.StoreId);
            if(store==null)
            {
                return Json(new { isok = false, msg = "没有找到店铺,请刷新重试" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxmodel = _xcxAppAccountRelationBLL.GetModel(store.appId);
            if(xcxmodel==null)
            {
                return Json(new { isok = false, msg = "还没授权,请刷新重试" }, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModel(xcxmodel.TId);
            if(xcxtemplate==null)
            {
                return Json(new { isok = false, msg = "找不到模板,请刷新重试" }, JsonRequestBehavior.AllowGet);
            }
            string updateLeveType = string.Empty;
            if (xcxtemplate.Type== (int)TmpType.小程序专业模板)
            {
                updateLeveType = "entpro";
            }
            else if(xcxtemplate.Type==(int)TmpType.小程序多门店模板)
            {
                updateLeveType = "store";
            }

            if (GroupUserBLL.SingleModel.Update(groupuser, "State,RecieveGoodTime"))
            {
                if (!VipRelationBLL.SingleModel.updatelevel(groupuser.ObtainUserId, updateLeveType, groupuser.BuyPrice))
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + guid));
                }
                return Json(new { isok = true, msg = "已确认收货" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false, msg = "已确认收货失败" }, JsonRequestBehavior.AllowGet);
        }

        #region 公共方法

        /// <summary>
        /// 获取当前可以参加的团
        /// </summary>
        /// <param name="groupid">拼团商品ID</param>
        /// <param name="length">获取数据数量</param>
        /// <returns></returns>
        private List<GroupSponsor> GetHaveSuccessGroup(int groupid, int length)
        {
            List<GroupSponsor> gsList = GroupSponsorBLL.SingleModel.GetListJoiningGroup(groupid, length);
            if (gsList.Count > 0)
            {
                string userIds = string.Join(",", gsList.Select(s => s.SponsorUserId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                foreach (GroupSponsor gsInfo in gsList)
                {
                    //获取头像和昵称
                    C_UserInfo user = userInfoList?.FirstOrDefault(f=>f.Id == gsInfo.SponsorUserId);
                    if (user != null)
                    {
                        gsInfo.UserName = user.NickName;
                        gsInfo.UserLogo = "//j.vzan.cc/content/city/images/voucher/10.jpg";
                        if (!string.IsNullOrEmpty(user.HeadImgUrl))
                        {
                            gsInfo.UserLogo = user.HeadImgUrl;
                        }
                    }

                    //开始以及结束时间
                    gsInfo.ShowStartTime = gsInfo.StartDate.ToString("yyyy/MM/dd HH:mm:ss");
                    gsInfo.ShowEndTime = gsInfo.EndDate.ToString("yyyy/MM/dd HH:mm:ss");
                }
            }

            return gsList;
        }

        /// <summary>
        /// 获取当前可以参加的团
        /// </summary>
        /// <param name="groupid">拼团商品ID</param>
        /// <param name="length">获取数据数量</param>
        /// <returns></returns>
        private List<Groups> GetCanJoinGroup(int storeid, int length)
        {
            List<Groups> grouplist = GroupsBLL.SingleModel.GetPageList(storeid, 1, length);
            if (grouplist?.Count > 0)
            {
                grouplist.ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.ImgUrl) && p.ImgUrl.IndexOf('@') == -1)
                    {
                        p.ImgUrl = Utility.ImgHelper.ResizeImg(p.ImgUrl, 640, 360); ;
                    }
                    //已团数量
                    p.GroupsNum = GroupUserBLL.SingleModel.GetCountPayGroup(p.Id);
                });
            }

            return grouplist;
        }

        /// <summary>
        /// 获取用户数据
        /// </summary>
        /// <param name="usergrouplog">拼团订单</param>
        /// <param name="paytype">支付类型，0：微信支付，1储值卡支付</param>
        /// <param name="orderid">订单id</param>
        /// <param name="userid">用户ID</param>
        /// <returns></returns>
        private List<C_UserInfo> GetUserInfo(List<GroupUser> usergrouplog,int paytype,int orderid,ref int userid)
        {
            List<C_UserInfo> userinfolist = new List<C_UserInfo>();
            string userIds = string.Join(",", usergrouplog.Select(s => s.ObtainUserId).Distinct());
            List<C_UserInfo> userInfoListTemp = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
            foreach (GroupUser item in usergrouplog)
            {
                //获取头像和昵称
                C_UserInfo user = userInfoListTemp?.FirstOrDefault(f=>f.Id == item.ObtainUserId);
                if (user != null)
                {
                    user.zbSiteId = item.IsGroupHead;
                    if (string.IsNullOrEmpty(user.HeadImgUrl))
                    {
                        user.HeadImgUrl = "j.vzan.cc/content/city/images/voucher/10.jpg";
                    }

                    userinfolist.Add(user);
                }

                //判断是否为储值支付
                if (paytype == 1)
                {
                    //下单用户
                    if (item.Id == orderid)
                    {
                        userid = item.Id;
                    }
                }
                else
                {
                    //下单用户
                    if (item.OrderId == orderid)
                    {
                        userid = item.Id;
                    }
                }
            }

            return userinfolist;
        }
        #endregion

    }
}