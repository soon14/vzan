using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public class apiEntGroupController : InheritController
    {
    }
        [ExceptionLog]
    public class apiMiniAppEntGroupController : apiEntGroupController
    {
        private static readonly object CutPriceLocker = new object();
        private static readonly object GroupLocker = new object();
        private static readonly object lockOrder = new object();
        

        /// <summary>
        /// 实例化对象
        /// </summary>
        public apiMiniAppEntGroupController()
        {
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
            int state = Context.GetRequestInt("state", 0);
            int storeId = Context.GetRequestInt("storeId", 0);//多门店分店ID，其他的都为0
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = 10;

            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (userId <= 0)
            {
                return Json(new { isok = -1, msg = "用户ID不能小于0" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            int xtype = _xcxAppAccountRelationBLL.GetXcxTemplateType(umodel.Id);
            if (xtype == 0)
            {
                return Json(new { isok = -1, msg = "小程序没授权" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            object postdata = new object();
            switch(xtype)
            {
                case (int)TmpType.小程序专业模板:
                    postdata = EntGoodsOrderBLL.SingleModel.GetMyGroupPostData(state, umodel.Id, userId, pageIndex, pageSize, storeId);
                    break;
                case (int)TmpType.小程序餐饮模板:
                    postdata = FoodGoodsOrderBLL.SingleModel.GetMyGroupPostData(state, umodel.Id, userId, pageIndex, pageSize);
                    break;
            }
            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取正在拼团的团列表
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetJoinGroupList()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int entgoodid = Context.GetRequestInt("entgoodid", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int storeId = Context.GetRequestInt("storeId", 0);//多门店分店ID，其他都为0
            int pageSize = 10;

            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            int xtype = _xcxAppAccountRelationBLL.GetXcxTemplateType(umodel.Id);
            if (xtype == 0)
            {
                return Json(new { isok = -1, msg = "小程序没授权" }, JsonRequestBehavior.AllowGet);
            }

            EntGroupsRelation grouprmodel = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(entgoodid, umodel.Id, storeId); //_entgrouprelationBll.GetModel($"entgoodsid={entgoodid}");
            if (grouprmodel == null)
            {
                return Json(new { isok = -1, msg = "匹配不到拼团"}, JsonRequestBehavior.AllowGet);
            }
            List<EntGroupSponsor> grouplist = EntGroupSponsorBLL.SingleModel.GetPageList(grouprmodel.Id, pageIndex, pageSize);
            if (grouplist?.Count > 0)
            {
                string userids = string.Join(",",grouplist.Select(s=>s.SponsorUserId).Distinct());
                List<C_UserInfo> groupusers = C_UserInfoBLL.SingleModel.GetListByIds(userids); 

                //拼团订单记录
                string groupids = string.Join(",",grouplist.Select(s=>s.Id));
                switch(xtype)
                {
                    case (int)TmpType.小程序专业模板:
                        List<EntGoodsOrder> entgoodsorderlist = EntGoodsOrderBLL.SingleModel.GetListByGoodsState(umodel.Id, groupids, MiniAppEntOrderState.待发货);

                        grouplist.ForEach(p =>
                        {
                            C_UserInfo user = groupusers.Where(w => w.Id == p.SponsorUserId).FirstOrDefault();
                            if (user != null && !string.IsNullOrEmpty(user.HeadImgUrl))
                            {
                                p.UserImg = Utility.ImgHelper.ResizeImg(user.HeadImgUrl, 640, 360); ;
                            }

                            int? tempgrouplist = entgoodsorderlist?.Where(w => w.GroupId == p.Id)?.Sum(s=>s.QtyCount);
                            //已团数量
                            p.GroupNum = Convert.ToInt32(tempgrouplist);
                        });
                        break;
                    case (int)TmpType.小程序餐饮模板:
                        Food food = FoodBLL.SingleModel.GetModelByAppId(umodel.Id);
                        if(food==null)
                        {
                            return Json(new { isok = -1, msg = "餐厅还未营业，请稍后再试"}, JsonRequestBehavior.AllowGet);
                        }
                        List<FoodGoodsOrder> foodgoodsorderlist = FoodGoodsOrderBLL.SingleModel.GetListByGoodsState(food.Id, groupids, miniAppFoodOrderState.待就餐);

                        grouplist.ForEach(p =>
                        {
                            C_UserInfo user = groupusers.Where(w => w.Id == p.SponsorUserId).FirstOrDefault();
                            if (user != null && !string.IsNullOrEmpty(user.HeadImgUrl))
                            {
                                p.UserImg = Utility.ImgHelper.ResizeImg(user.HeadImgUrl, 640, 360); ;
                            }

                            int? tempgrouplist = foodgoodsorderlist?.Where(w => w.GroupId == p.Id).Sum(s=>s.QtyCount);
                            //已团数量
                            p.GroupNum = Convert.ToInt32(tempgrouplist);
                        });
                        break;
                }
                
            }

            return Json(new { isok = 1, msg = "成功", postdata = grouplist }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  查看团详情
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post")]
        public ActionResult GetGroupDetail()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int groupid = Context.GetRequestInt("groupid", 0);

            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            int xtype = _xcxAppAccountRelationBLL.GetXcxTemplateType(umodel.Id);
            if (xtype == 0)
            {
                return Json(new { isok = -1, msg = "小程序没授权" }, JsonRequestBehavior.AllowGet);
            }

            EntGroupSponsor group = EntGroupSponsorBLL.SingleModel.GetGroupDetail(groupid, xtype);
            if (group == null)
            {
                return Json(new { isok = -1, msg = "团已失效" }, JsonRequestBehavior.AllowGet);
            }

            //获取该产品对应的可以参团的数据
            List<EntGroupSponsor> GroupSponsorList = EntGroupSponsorBLL.SingleModel.GetHaveSuccessGroup(group.EntGoodRId, 10,group.GoodId,xtype);

            return Json(new { isok = 1, msg = "成功", postdata = group, GroupSponsorList= GroupSponsorList }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询团商品列表
        /// storeId>0查询门店产品
        /// storeId=0或不传查询主店产品
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetGroupGoodsList()
        {
            returnObj = new Return_Msg_APP();
            returnObj.isok = false;

            int pagesize = Context.GetRequestInt("pagesize", 10);
            int pageindex = Context.GetRequestInt("pageindex", 0);
            string typeid = Context.GetRequest("typeid", "");
            int aid= Context.GetRequestInt("aid", 0);
            string search = Context.GetRequest("search", "");
            int plabels = Context.GetRequestInt("plabels", 0);
            int ptype = Context.GetRequestInt("ptype", 0);
            int ptag = Context.GetRequestInt("ptag", -1);
            int storeid = Context.GetRequestInt("storeId", 0);
            int levelid = Context.GetRequestInt("levelid", 0);
            int goodtype = Context.GetRequestInt("goodtype", (int)EntGoodsType.拼团产品);

            int xtype = _xcxAppAccountRelationBLL.GetXcxTemplateType(aid);
            if (xtype == 0)
            {
                returnObj.Msg = "小程序没授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            
            switch(xtype)
            {
                case (int)TmpType.小程序多门店模板:
                    List<EntGoods> goodList = new List<EntGoods>();
                    if (storeid > 0)
                    {
                        DataTable dt = EntGoodsBLL.SingleModel.GetMStoreEntGoodsList_Sub(aid, search, plabels, ptype, ptag, storeid, typeid, pageindex, pagesize, goodtype);
                        List<SubStoreGoodsView> subgoodList = DataHelper.ConvertDataTableToList<SubStoreGoodsView>(dt);
                        goodList = subgoodList?.Select<SubStoreGoodsView, EntGoods>(s => new EntGoods()
                        {
                            id = s.Pid,
                            aid = s.Aid,
                            name = s.name,
                            img = s.img,
                            showprice = s.showprice,
                            ptypes = s.ptypes,
                            exttypes = s.exttypes,
                            exttypesstr = s.exttypesstr,
                            ptypestr = s.ptypestr,
                            stockLimit = s.stockLimit,
                            plabels = s.plabels,
                            plabelstr = s.plabelstr,
                            specificationkeys = s.specificationkeys,
                            specification = s.specification,
                            pickspecification = s.pickspecification,
                            price = s.price,
                            unit = s.unit,
                            slideimgs = s.slideimgs,
                            description = s.description,
                            addtime = s.addtime,
                            sort = s.sort,
                            specificationdetail = s.SubSpecificationdetail,
                            stock = s.SubStock,
                            storeId = s.StoreId,
                        }).ToList();
                    }
                    else
                    {
                        goodList = EntGoodsBLL.SingleModel.GetMStoreEntGoodsList_Store(aid, search, plabels, ptype, ptag, storeid, typeid, pageindex, pagesize, goodtype);
                    }
                    
                    //获取商品类型和标签
                    goodList = EntGoodsBLL.SingleModel.GetTypeAndLabelList(goodList, levelid);

                    //获取拼团
                    if (goodtype == (int)EntGoodsType.拼团产品 && goodList != null && goodList.Count > 0)
                    {
                        goodList = EntGroupsRelationBLL.SingleModel.GetEntGoodRelation(goodList, aid, storeid);
                    }
                    returnObj.dataObj = goodList;
                    break;
            }


            returnObj.isok = true;
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 团商品详情
        /// 如果不传storeId或者storeId=0查询总店产品，否则查询分店产品
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetGoodInfo()
        {
            returnObj = new Return_Msg_APP();
            returnObj.isok = false;

            int goodid = Context.GetRequestInt("goodid", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int aid = Context.GetRequestInt("aid", 0);
            if (goodid == 0 || storeId < 0 || (storeId > 0 && aid == 0))
            {
                returnObj.Msg = "非法请求";
                return Json(returnObj , JsonRequestBehavior.AllowGet);
            }

            int xtype= _xcxAppAccountRelationBLL.GetXcxTemplateType(aid);
            if (xtype == 0)
            {
                returnObj.Msg = "小程序没授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            
            EntGroupsRelation relationmodel = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(goodid, aid, storeId);
            if (relationmodel == null)
            {
                returnObj.Msg = "该拼团已不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            //已团数量
            int groupnum = 0;
            List<object> userlist = EntGroupSponsorBLL.SingleModel.GetGoupsUserImgs(relationmodel.Id, ref groupnum,xtype,relationmodel.EntGoodsId);
            List<EntGroupSponsor> groupSponsorList = EntGroupSponsorBLL.SingleModel.GetHaveSuccessGroup(relationmodel.Id, 10, goodid);

            switch (xtype)
            {
                case (int)TmpType.小程序专业模板:break;
                case (int)TmpType.小程序多门店模板:
                    EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(goodid);
                    if (goodModel == null || goodModel.state == 0)
                    {
                        returnObj.Msg = "产品不存在或已删除";
                        return Json(returnObj, JsonRequestBehavior.AllowGet);
                    }

                    if (storeId > 0)
                    {
                        SubStoreEntGoods subGood = SubStoreEntGoodsBLL.SingleModel.GetModelByAppIdStoreIdGoodsId(aid, storeId, goodid);
                        if (subGood == null)
                        {
                            returnObj.Msg = "产品不存在或已下架";
                            return Json(returnObj, JsonRequestBehavior.AllowGet);
                        }
                        goodModel.specificationdetail = subGood.SubSpecificationdetail;
                        goodModel.stock = subGood.SubStock;
                    }
                    
                    if (!string.IsNullOrEmpty(goodModel.plabels))
                    {
                        goodModel.plabelstr = EntGoodsBLL.SingleModel.GetPlabelStr(goodModel.plabels); 
                        goodModel.plabelstr_array = goodModel.plabelstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }

                    goodModel.EntGroups.GroupUserList = userlist;
                    goodModel.EntGroups.GroupsNum = groupnum;
                    goodModel.EntGroups.GroupSponsorList = groupSponsorList;

                    returnObj.dataObj = goodModel;
                    break;
            }
            
            returnObj.isok = true;
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }
    }
}