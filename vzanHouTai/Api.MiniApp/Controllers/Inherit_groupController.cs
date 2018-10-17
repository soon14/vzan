using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Stores;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Footbath;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utility;

namespace Api.MiniApp.Controllers
{
    [ExceptionLog]
    public partial class InheritController : AsyncController
    {

        /// <summary>
        /// 下单时判断是否是拼团
        /// </summary>
        /// <param name="isgroup"></param>
        /// <param name="groupid"></param>
        /// <param name="specificationId"></param>
        /// <param name="goodscar"></param>
        /// <param name="grouperprice"></param>
        /// <param name="groupmodel"></param>
        /// <returns></returns>
        protected string CommandEntGroup(int isgroup, int groupid,int userid,int storeid, int goodid, ref int grouperprice, ref EntGroupsRelation groupmodel, int buyCount)
        {
            if (isgroup <= 0 && groupid <= 0)
            {
                return "";
            }

            if (isgroup > 0 && groupid > 0)
            {
                return "拼团参数错误";
            }

            groupmodel = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(goodid,groupmodel.RId, storeid);
            if (groupmodel == null)
            {
                return "产品匹配不到拼团信息";
            }

            #region 判断开团时，库存是否满足成团
            EntGoods entgood = EntGoodsBLL.SingleModel.GetModel(groupmodel.EntGoodsId);
            if (entgood == null)
            {
                return "拼团产品已下架";
            }
            //已团件数
            if (isgroup > 0 && entgood.stockLimit)
            {
                //判定是否当前用户下单后,剩余的数量是否足够成一个团,如果不足够,那么不允许用户再开团
                if (entgood.stock - buyCount < groupmodel.GroupSize - 1)
                {
                    return "商品库存不足，无法成团";
                }
            }
            #endregion
            grouperprice = groupmodel.HeadDeduct;

            //判断是否是团长，团员不减团长优惠价
            if (groupid > 0)
            {
                grouperprice = 0;
            }

            //判断是否已参加该团
            if(groupid>0)
            {
                EntGoodsOrder entgoodorder = EntGoodsOrderBLL.SingleModel.GetModelGroupByGrouId(groupid,userid);
                if(entgoodorder!=null)
                {
                    return "您已经参加过该拼团了";
                }
            }

            return "";
        }

        /// <summary>
        /// 检查购买团商品是否超出限制
        /// </summary>
        /// <param name="userid">用户Id</param>
        /// <param name="qty">购买数量</param>
        /// <param name="good">商品</param>
        /// <param name="attrSpacStr">规格参数</param>
        /// <param name="price">价格</param>
        /// <returns></returns>
        protected string CheckGoodCount(int userid, int qty, EntGoods good, string attrSpacStr, ref int price)
        {
            string msg = "";
            EntGroupsRelation entgroupremodel = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(good.id, good.aid,good.storeId);
            if (entgroupremodel == null)
            {
                msg = "团商品不存在";
            }
            price = entgroupremodel.GroupPrice;
            //用户已团数量
            int buycount = EntGoodsOrderBLL.SingleModel.GetGroupPersonCount(userid, entgroupremodel.EntGoodsId, good.storeId);
            if (entgroupremodel.LimitNum > 0 && qty > entgroupremodel.LimitNum - buycount)
            {
                msg = "超过购买限制";
            }

            //判断是否带有规格参数
            if (!string.IsNullOrWhiteSpace(attrSpacStr))
            {
                EntGoodsAttrDetail curEntGoodsAttrDtl = good.GASDetailList.First(x => x.id.Equals(attrSpacStr));
                if (curEntGoodsAttrDtl == null)
                {
                    msg = $"商品不存在:goodId={good.id}||spec={attrSpacStr}  ";
                }
                price = Convert.ToInt32(curEntGoodsAttrDtl.groupPrice * 100);
            }

            return msg;
        }
        /// <summary>
        /// 检查购买团商品是否超出限制
        /// </summary>
        /// <param name="userid">用户Id</param>
        /// <param name="qty">购买数量</param>
        /// <param name="good">商品</param>
        /// <param name="attrSpacStr">规格参数</param>
        /// <param name="price">价格</param>
        /// <returns></returns>
        protected string CheckGoodCount(int userid, int qty, int goodid,int rid,int storeId, string attrSpacStr,int groupprice, ref int price,int type=(int)TmpType.小程序专业模板)
        {
            string msg = "";
            EntGroupsRelation entgroupremodel = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(goodid, rid, storeId);
            if (entgroupremodel == null)
            {
                msg = "团商品不存在";
            }
            price = entgroupremodel.GroupPrice;
            //用户已团数量
            int buycount = EntGoodsOrderBLL.SingleModel.GetGroupPersonCount(userid, entgroupremodel.EntGoodsId, storeId);
            switch(type)
            {
                case (int)TmpType.小程序餐饮模板:
                    buycount = FoodGoodsOrderBLL.SingleModel.GetGroupPersonCount(userid, entgroupremodel.EntGoodsId);
                    break;
                case (int)TmpType.小程序专业模板:
                    buycount = EntGoodsOrderBLL.SingleModel.GetGroupPersonCount(userid, entgroupremodel.EntGoodsId, storeId);
                    break;
            }
            if (entgroupremodel.LimitNum > 0 && qty > entgroupremodel.LimitNum - buycount)
            {
                msg = "超过购买限制";
            }

            //判断是否带有规格参数
            if (!string.IsNullOrWhiteSpace(attrSpacStr))
            {
                price = groupprice;
            }

            return msg;
        }

        /// <summary>
        /// 开团
        /// </summary>
        /// <param name="isgroup"></param>
        /// <param name="rid"></param>
        /// <param name="buyMode"></param>
        /// <param name="userid"></param>
        /// <param name="groupmodel"></param>
        /// <param name="dbOrder"></param>
        /// <returns></returns>
        protected string OpenGroup(int isgroup,int rid,int buyMode,int userid,EntGroupsRelation groupmodel,ref EntGoodsOrder dbOrder)
        {
            string msg = "";
            //是否开团
            if (isgroup > 0)
            {
                int groupusersum = EntGroupSponsorBLL.SingleModel.GetGroupSponrUserSum(groupmodel.Id);
                EntGroupSponsor groupSponsor = new EntGroupSponsor();
                groupSponsor.EntGoodRId = groupmodel.Id;
                groupSponsor.SponsorUserId = userid;
                groupSponsor.GroupSize = groupmodel.GroupSize;
                groupSponsor.RId = rid;
                groupSponsor.StartDate = DateTime.Now;
                groupSponsor.EndDate = DateTime.Now.AddHours(groupmodel.ValidDateLength);
                groupSponsor.State = buyMode == (int)miniAppBuyMode.储值支付 || dbOrder.BuyPrice <= 0 ? (int)GroupState.开团成功 : (int)GroupState.待付款;  //待付款
                int groupid = Convert.ToInt32(EntGroupSponsorBLL.SingleModel.Add(groupSponsor));
                if (groupid <= 0)
                {
                    msg = $"成团失败！";
                }
                dbOrder.GroupId = groupid;
                EntGoodsOrderBLL.SingleModel.Update(dbOrder, "groupid");
            }

            return msg;
        }

        /// <summary>
        /// 开团
        /// </summary>
        /// <param name="isgroup"></param>
        /// <param name="rid"></param>
        /// <param name="buyMode"></param>
        /// <param name="userid"></param>
        /// <param name="groupmodel"></param>
        /// <param name="dbOrder"></param>
        /// <returns></returns>
        protected string OpenGroup(int isgroup, int rid, int buyMode, int userid, EntGroupsRelation groupmodel,int buyprice, ref int groupid)
        {
            string msg = "";
            //是否开团
            if (isgroup > 0)
            {
                EntGroupSponsor groupSponsor = new EntGroupSponsor();
                groupSponsor.EntGoodRId = groupmodel.Id;
                groupSponsor.SponsorUserId = userid;
                groupSponsor.GroupSize = groupmodel.GroupSize;
                groupSponsor.RId = rid;
                groupSponsor.StartDate = DateTime.Now;
                groupSponsor.EndDate = DateTime.Now.AddHours(groupmodel.ValidDateLength);
                groupSponsor.State = buyMode == (int)miniAppBuyMode.储值支付 || buyprice <= 0 ? (int)GroupState.开团成功 : (int)GroupState.待付款;  //待付款
                groupid = Convert.ToInt32(EntGroupSponsorBLL.SingleModel.Add(groupSponsor));
                if (groupid <= 0)
                {
                    msg = $"成团失败！";
                }
            }

            return msg;
        }
    }
}