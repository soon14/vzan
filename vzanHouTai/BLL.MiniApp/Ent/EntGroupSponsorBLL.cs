
using BLL.MiniApp.Fds;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntGroupSponsorBLL : BaseMySql<EntGroupSponsor>
    {

        #region 单例模式
        private static EntGroupSponsorBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntGroupSponsorBLL()
        {

        }

        public static EntGroupSponsorBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntGroupSponsorBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        #region 基本操作
        /// <summary>
        /// 获取团商品已开团所需总人数
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public int GetGroupSponrUserSum(int entgoodrid)
        {
            object result = SqlMySql.ExecuteScalar(connName, CommandType.Text, $"select sum(GroupSize) from EntGroupSponsor where entgoodrid = {entgoodrid}");
            if (result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }
            return 0;
        }
        public List<EntGroupSponsor> GetListByGoodRids(string sponsidstr)
        {
            return GetList($"entgoodrid in ({sponsidstr})");
        }
        public List<EntGroupSponsor> GetListGroupSponsorByRid(int rid, string state)
        {
            return GetList($"rid = {rid} and state in ({state})");
        }
        public List<EntGroupSponsor> GetListById(string ids)
        {
            return GetList($"id in ({ids})");
        }
        #endregion
        
        #region 专业/多门店拼团

        /// <summary>
        /// 获取订单团状态
        /// </summary>
        /// <param name="orderlist"></param>
        public void GetSponsorState(ref List<EntGoodsOrder> orderlist)
        {
            if (orderlist == null || orderlist.Count <= 0)
                return;

            string sporids = string.Join(",", orderlist.Where(w => w.OrderType == 3 && w.GroupId > 0).Select(s => s.GroupId).Distinct());
            if (string.IsNullOrEmpty(sporids))
            {
                return;
            }
            List<EntGroupSponsor> sponlist = GetListById(sporids);
            if (sponlist == null || sponlist.Count <= 0)
                return;

            foreach (EntGoodsOrder order in orderlist)
            {
                EntGroupSponsor model = sponlist.FirstOrDefault(f => f.Id == order.GroupId);
                if (model == null)
                    continue;

                order.GroupState = model.State;
            }
        }
        /// <summary>
        /// 支付成功回调修改团状态
        /// </summary>
        /// <param name="gOrder"></param>
        /// <param name="tran">是否返回sql语句添加到事务,空为不进行事务处理，不为空则返回sql语句</param>
        /// <param name="updatemethod">0：添加sql语句进行事务修改，1：不通过事务修改</param>
        public bool PayReturnUpdateGroupState(int groupid, int rid, ref TransactionModel tran, int updatemethod = 0,int type=(int)TmpType.小程序专业模板)
        {
            //拼团状态
            if (groupid > 0)
            {
                EntGroupSponsor groupspor = GetModel(groupid);
                if (groupspor != null)
                {
                    //是否开团成功
                    if (groupspor.State == (int)GroupState.待付款)
                    {
                        groupspor.State = (int)GroupState.开团成功;
                        if (updatemethod == 1)
                        {
                            Update(groupspor, "State");
                        }
                        else
                        {
                            tran.Add(base.BuildUpdateSql(groupspor, "State"));
                        }
                    }

                    #region 团购是否成功
                    int groupordercount = 0;
                    switch (type)
                    {
                        case (int)TmpType.小程序专业模板:
                            groupordercount = EntGoodsOrderBLL.SingleModel.GetGroupCount(rid, groupspor.Id);
                            break;
                        case (int)TmpType.小程序餐饮模板:
                            groupordercount = FoodGoodsOrderBLL.SingleModel.GetGroupCount(groupspor.Id);
                            break;
                    }
                    //判断是否开团成功
                    if (groupordercount == groupspor?.GroupSize - 1)
                    {
                        groupspor.State = (int)GroupState.团购成功;
                        if (updatemethod == 1)
                        {
                            Update(groupspor, "State");
                        }
                        else
                        {
                            tran.Add(base.BuildUpdateSql(groupspor, "State"));
                        }
                    }
                    #endregion
                }
            }
            return true;
        }
        #endregion

        #region 餐饮拼团

        /// <summary>
        /// 获取订单团状态
        /// </summary>
        /// <param name="orderlist"></param>
        public void GetFoodSponsorState(ref List<FoodAdminGoodsOrder> orderlist)
        {
            if (orderlist == null || orderlist.Count <= 0)
                return;

            string sporids = string.Join(",", orderlist.Where(w => w.GoodType == (int)EntGoodsType.拼团产品 && w.GroupId > 0).Select(s => s.GroupId).Distinct());
            if (string.IsNullOrEmpty(sporids))
            {
                return;
            }
            List<EntGroupSponsor> sponlist = GetListById(sporids);
            if (sponlist == null || sponlist.Count <= 0)
                return;

            foreach (FoodGoodsOrder order in orderlist)
            {
                EntGroupSponsor model = sponlist.FirstOrDefault(f => f.Id == order.GroupId);
                if (model == null)
                    continue;

                order.GroupState = model.State;
            }
        }
        #endregion

        #region 逻辑操作

        /// <summary>
        /// 获取同一个拼团商品还能参团的拼团数据，按最少需要参团人数和快参团结束日期排序
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public List<EntGroupSponsor> GetListByGroupId(int entgoodrid, int groupstate, int pageIndex, int pageSize,ref int count)
        {
            List<EntGroupSponsor> storediscountList = new List<EntGroupSponsor>();
            string wheresql = $"EntGoodRId={entgoodrid}";
            if(groupstate!=999)
            {
                wheresql += $" and State={groupstate}";
            }

            count = base.GetCount(wheresql);

            return base.GetList(wheresql,pageSize,pageIndex,"*","id desc");
        }
        
        public List<EntGroupSponsor> GetGroupList(int type)
        {
            string entstrWhere = $"type={type} and State={(int)GroupState.开团成功} and EndDate < NOW()";
            List<EntGroupSponsor> entlistPost = base.GetList(entstrWhere, 500, 1);
            return entlistPost;
        }

        /// <summary>
        /// 获取拼团数据
        /// </summary>
        /// <param name="storeid">产品关联表ID</param>
        /// <returns></returns>
        public List<EntGroupSponsor> GetPageList(int entgoodrid, int pageindex, int pagesize)
        {
            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return base.GetList($"state={(int)GroupState.开团成功} and entgoodrid={entgoodrid} and StartDate<'{nowtime}' and EndDate>'{nowtime}'", pagesize, pageindex, "*", "EndDate Desc");
        }

        public EntGroupSponsor GetGroupDetail(int groupid, int type)
        {
            EntGroupSponsor model = GetModel(groupid);
            if (model == null)
            {
                return model;
            }

            EntGroupsRelation entgroup = EntGroupsRelationBLL.SingleModel.GetModel(model.EntGoodRId);
            if (entgroup == null)
            {
                return new EntGroupSponsor();
            }
            
            int groupnum = 0;
            model.GroupPrice = entgroup.GroupPriceStr;
            model.OriginalPrice = entgroup.OriginalPriceStr;
            switch (type)
            {
                case (int)TmpType.小程序专业模板:
                    EntGoods entgood = EntGoodsBLL.SingleModel.GetModel(entgroup.EntGoodsId);
                    if (entgood == null)
                    {
                        return new EntGroupSponsor();
                    }

                    model.GroupName = entgood.name;
                    model.GoodId = entgood.id;
                    EntGoodsOrder order = EntGoodsOrderBLL.SingleModel.GetModel($"ordertype = 3 and groupid = {groupid}");
                    model.GroupImage = ImgHelper.ResizeImg(entgood.img, 220, 220);
                    if (order != null)
                    {
                        EntGoodsCart goodOrderDtl = EntGoodsCartBLL.SingleModel.GetModelByGoodsOrderId(order.Id);
                        if (goodOrderDtl != null && !string.IsNullOrEmpty(goodOrderDtl.SpecImg))
                        {
                            model.GroupImage = ImgHelper.ResizeImg(goodOrderDtl.SpecImg, 220, 220);
                        }
                    }

                    ;
                    model.GroupUserList = EntGoodsOrderBLL.SingleModel.GetPersonByGroup(groupid.ToString(), ref groupnum);
                    break;
                case (int)TmpType.小程序餐饮模板:
                    groupnum = FoodGoodsOrderBLL.SingleModel.GetGroupPersonCount(0, entgroup.EntGoodsId);
                    model.GroupUserList = FoodGoodsOrderBLL.SingleModel.GetPersonByGroup(groupid.ToString());
                    FoodGoods foodgood = FoodGoodsBLL.SingleModel.GetModel(entgroup.EntGoodsId);
                    if (foodgood == null)
                    {
                        return new EntGroupSponsor();
                    }

                    model.GroupName = foodgood.GoodsName;
                    model.GoodId = foodgood.Id;
                    model.GroupImage = ImgHelper.ResizeImg(foodgood.ImgUrl, 220, 220); ;
                    break;
            }
            model.GroupNum = groupnum+ entgroup.InitSaleCount;//加上初始化销售量
           
            return model;
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
        public string OpenGroup(int isgroup, int rid, int buyMode, int userid, EntGroupsRelation groupmodel, int buyprice, int type,ref int groupid)
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
                groupSponsor.Type = type;
                groupSponsor.StartDate = DateTime.Now;
                groupSponsor.EndDate = DateTime.Now.AddHours(groupmodel.ValidDateLength);
                groupSponsor.State = buyMode == (int)miniAppBuyMode.储值支付 || buyprice <= 0 ? (int)GroupState.开团成功 : (int)GroupState.待付款;  //待付款
                groupid = Convert.ToInt32(base.Add(groupSponsor));
                if (groupid <= 0)
                {
                    msg = $"成团失败！";
                }
            }

            return msg;
        }

        /// <summary>
        /// 获取开团成功数据
        /// </summary>
        /// <param name="entgoodrid"></param>
        /// <returns></returns>
        public List<EntGroupSponsor> GetListByGoodRid(int entgoodrid,int pagesize,int pageindex)
        {
            string sqlwhere = $"EntGoodRId={entgoodrid} and state in ({(int)GroupState.团购成功},{(int)GroupState.开团成功})";
            if(pagesize>0 && pageindex>0)
            {
                return base.GetList(sqlwhere,pagesize,pageindex, "*","EndDate desc");
            }
            return base.GetList(sqlwhere);
        }
        /// <summary>
        /// 获取指定产品已拼团的用户头像数组
        /// </summary>
        /// <returns></returns>
        public List<object> GetGoupsUserImgs(int entgoodrid, ref int groupnum, int type,int goodid)
        {
            List<object> userimglist = new List<object>();
            //查询该产品已开了多少团
            List<EntGroupSponsor> entsponsorlist = GetListByGoodRid(entgoodrid,0,0);
            if (entsponsorlist == null || entsponsorlist.Count <= 0)
            {
                return userimglist;
            }

            //团ID
            string groupids = string.Join(",", entsponsorlist.Select(s => s.Id).Distinct());
            switch (type)
            {
                case (int)TmpType.小程序专业模板:
                    userimglist = EntGoodsOrderBLL.SingleModel.GetPersonByGroup(groupids, ref groupnum);
                    break;
                case (int)TmpType.小程序餐饮模板:
                    groupnum = FoodGoodsOrderBLL.SingleModel.GetGroupPersonCount(0, goodid);
                    userimglist = FoodGoodsOrderBLL.SingleModel.GetPersonByGroup(groupids);
                    break;
            }
            
            return userimglist;
        }

        /// <summary>
        /// 获取当前可以参加的团
        /// </summary>
        /// <param name="groupid">拼团产品ID</param>
        /// <param name="length">获取数据数量</param>
        /// <returns></returns>
        public List<EntGroupSponsor> GetHaveSuccessGroup(int entgoodrid, int length, int goodid, int type = (int)TmpType.小程序专业模板)
        {
            List<EntGroupSponsor> gsList = GetPageList(entgoodrid, 1, length);
            if (gsList != null && gsList.Count > 0)
            {
                string userIds = string.Join(",",gsList.Select(s=>s.SponsorUserId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                foreach (EntGroupSponsor gsInfo in gsList)
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

                    int groupnum = 0;
                    //开始以及结束时间
                    gsInfo.ShowStartTime = gsInfo.StartDate.ToString("yyyy/MM/dd HH:mm:ss");
                    gsInfo.ShowEndTime = gsInfo.EndDate.ToString("yyyy/MM/dd HH:mm:ss");
                    gsInfo.GoodId = goodid;
                    switch (type)
                    {
                        case (int)TmpType.小程序专业模板:
                            gsInfo.GroupUserList = EntGoodsOrderBLL.SingleModel.GetPersonByGroup(gsInfo.Id.ToString(), ref groupnum);
                            break;
                        case (int)TmpType.小程序餐饮模板:

                            groupnum = FoodGoodsOrderBLL.SingleModel.GetGroupPersonCount(0, goodid);
                            gsInfo.GroupUserList = FoodGoodsOrderBLL.SingleModel.GetPersonByGroup(gsInfo.Id.ToString());
                            break;
                    }
                    gsInfo.GroupNum = groupnum;
                }
            }

            return gsList;
        }
        #endregion
    }
}