using BLL.MiniApp.Stores;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Tools
{
    public class GroupsBLL : BaseMySql<Groups>
    {
        #region 单例模式
        private static GroupsBLL _singleModel;
        private static readonly object SynObject = new object();

        private GroupsBLL()
        {

        }

        public static GroupsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new GroupsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public static readonly string CacheKeyCGroupsVer = "vzan_groupsver_{0}";

        private static readonly object GroupLocker = new object();

        #region 基础操作

        public List<Groups> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<Groups>();

            return base.GetList($"id in ({ids})");
        }

        /// <summary>
        /// 获取首页拼团数据
        /// </summary>
        /// <param name="storeid">店铺ID</param>
        /// <param name="state">状态，0-待审核 1 审核通过 -1审核不通过</param>
        /// <returns></returns>
        public List<Groups> GetPageList(int storeid, int state, int length)
        {
            var nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return GetList($"state={state} and StoreId={storeid} and ValidDateStart<'{nowtime}' and ValidDateEnd>'{nowtime}'", length, 1, "", "Id Desc");
        }
        #endregion

        #region 逻辑操作

        /// <summary>
        /// 获取拼团列表
        /// </summary>
        /// <param name="storeid">店铺Id</param>
        /// <param name="pageSize">页数</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="state">状态，0所有，1进行中，2结束</param>
        /// <returns></returns>
        public List<Groups> GetStoreGroupsList(int storeid, int pageSize, int pageIndex, int state = 0)
        {
            List<Groups> storediscountList = new List<Groups>();

            string sqlWhere = $"State=1 ";

            if (storeid > 0)
            {
                sqlWhere += " and StoreId = " + storeid;
            }
            string orderFile = "Id desc";
            if (state == 1)
            {
                sqlWhere += " and ValidDateStart < now()  and ValidDateEnd > now() ";
            }
            else if (state == 2)
            {
                sqlWhere += " and ValidDateEnd <now() ";
            }

            storediscountList = GetList(sqlWhere, pageSize, pageIndex, "", orderFile);
            return storediscountList;
        }

        /// <summary>
        /// 用户参加拼团
        /// </summary>
        /// <param name="groupjson">拼团信息</param>
        /// <returns></returns>
        public int AddGroup(ref AddGroupModel groupjson)
        {
            string appId = groupjson.appId;//小程序appId
            int userId = groupjson.userId;//用户ID
            int groupId = groupjson.groupId;//拼团商品ID
            int num = groupjson.num;//数量
            int isGroup = groupjson.isGroup;//是否团购
            int isGHead = groupjson.isGHead;//是否团长
            int gsid = groupjson.gsid;//参团ID

            if (string.IsNullOrEmpty(appId))
            {
                groupjson.msg = "小程序appId不能为空！";
                return -1;
            }
            XcxAppAccountRelation relationmodel = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appId);
            if (relationmodel == null)
            {
                groupjson.msg = "非法请求！";
                return -1;
            }
            if (userId <= 0)
            {
                groupjson.msg = "用户ID不能为0！";
                return -1;
            }
            C_UserInfo loginCUser = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (loginCUser == null)
            {
                groupjson.msg = "找不到用户！";
                return -1;
            }
            if (groupId <= 0)
            {
                groupjson.msg = "拼团商品ID不能为0！";
                return -1;
            }
            if (num <= 0)
            {
                groupjson.msg = "拼团商品数量不能为0！";
                return -1;
            }
            if (string.IsNullOrEmpty(groupjson.addres))
            {
                groupjson.msg = "收货地址不能为空！";
                return -1;
            }
            Groups group = base.GetModel(groupId);
            if (null == group)
            {
                groupjson.msg = "不存在该拼团商品！";
                return -1;
            }

            List<GroupUser> buyerlist = GroupUserBLL.SingleModel.GetList($"GroupId={groupId} && ObtainUserId={loginCUser.Id} and state not in({(int)MiniappPayState.取消支付},{(int)MiniappPayState.待支付})");
            var userCouponLCount = buyerlist?.Sum(s => s.BuyNum);
            //var record = userCouponList.Where(m => m.ObtainUserId == loginCUser.Id && m.State !=  && m.State != );

            if (group.LimitNum > 0 && userCouponLCount + num > group.LimitNum)
            {
                groupjson.msg = "您已达到此拼团限购数量！";
                return -1;
            }

            //判断是否已售罄
            if (group.RemainNum <= 0)
            {
                groupjson.msg = "你手慢了，商品已售罄！";
                return -1;
            }

            int totalPrice = group.UnitPrice * num;
            if (isGroup == 1)
            {
                totalPrice = group.DiscountPrice * num;
                //新开团团长减价
                if (isGHead == 1)
                {
                    //判断剩余数量是否达到团的人数
                    if (group.RemainNum < group.GroupSize)
                    {
                        groupjson.msg = "商品的剩余数量已经不够开一个新团哦！";
                        return -1;
                    }
                    totalPrice = totalPrice - group.HeadDeduct;
                }
                else
                {

                }
            }
            //判断价格和数量是否正常
            if (totalPrice <= 0 || num <= 0)
            {
                groupjson.msg = "价格或数量有误，请刷新后重试！";
                return -1;
            }
            //判断参团的团状态
            if (gsid > 0)
            {
                GroupSponsor gSinfo = GroupSponsorBLL.SingleModel.GetModel(gsid);
                if (gSinfo == null)
                {
                    groupjson.msg = "参团信息有误，请刷新后重试！";
                    return -1;
                }
                if (gSinfo.State != 1 || gSinfo.EndDate < DateTime.Now)
                {
                    groupjson.msg = "此团信息状态已改变，请刷新后重试！";
                    return -1;
                }
            }

            lock (GroupLocker)
            {
                if (group.RemainNum < num)
                {
                    groupjson.msg = "库存已不足，请减少购买数量后重试！";
                    return -1;
                }

                if (isGHead == 0 && isGroup == 1) // 参团
                {
                    var groupsponcount = GroupUserBLL.SingleModel.GetCount($"GroupSponsorId={gsid} and state ={(int)MiniappPayState.待发货}");
                    //var groupsponcount = sporslist?.Sum(s=>s.BuyNum);
                    if (group.GroupSize < groupsponcount + 1)
                    {
                        groupjson.msg = "拼团场面太火爆，请稍后重试！";
                        return -1;
                    }
                }

                //发起开团
                GroupSponsor groupSponsor = new GroupSponsor();
                int result = gsid;
                if (isGHead == 1 && isGroup == 1)
                {
                    if (group.RemainNum / group.GroupSize <= 0)
                    {
                        groupjson.msg = "剩余商品已不足成团！";
                        return -1;
                    }
                    groupSponsor.GroupId = group.Id;
                    groupSponsor.GroupName = group.GroupName;
                    groupSponsor.SponsorUserId = loginCUser.Id;
                    groupSponsor.GroupSize = group.GroupSize;
                    groupSponsor.StartDate = DateTime.Now;
                    groupSponsor.EndDate = DateTime.Now.AddHours(24);
                    if (group.ValidDateEnd <= DateTime.Now.AddHours(24))
                    {
                        groupSponsor.EndDate = group.ValidDateEnd;
                    }
                    groupSponsor.State = (int)GroupState.待付款;  //待付款
                    result = Convert.ToInt32(GroupSponsorBLL.SingleModel.Add(groupSponsor));
                }

                //用户获得团购
                GroupUser userGroup = new GroupUser();
                userGroup.DiscountGuid = Guid.NewGuid().ToString().Replace("-", "");
                userGroup.CreateDate = DateTime.Now;
                userGroup.GroupId = group.Id;
                userGroup.GroupSponsorId = result;
                userGroup.BuyNum = num;
                userGroup.BuyPrice = totalPrice;
                userGroup.IsGroup = isGroup;
                userGroup.IsGroupHead = isGHead;
                userGroup.ObtainUserId = userId;
                userGroup.State = (int)MiniappPayState.待支付;
                //userGroup.OrderNo = Order.orderno;
                //userGroup.OrderId = Order.Id;
                //userGroup.PayTime = DateTime.Now;
                userGroup.Address = groupjson.addres;
                //userGroup.PayType = groupj == 1 ? Order.ActionType : 0;
                userGroup.ValidNumber = new Random().Next(100000, 999999);
                userGroup.AppId = groupjson.appId;
                userGroup.Phone = groupjson.phone;
                userGroup.UserName = groupjson.username;
                userGroup.Note = groupjson.note;
                userGroup.Id = Convert.ToInt32(GroupUserBLL.SingleModel.Add(userGroup));

                group.RemainNum = group.RemainNum - num;
                if (!base.Update(group, "RemainNum"))
                {
                    log4net.LogHelper.WriteInfo(GetType(), "更改拼团库存失败");
                    return -1;
                }

                groupjson.msg = "开团成功！";
                groupjson.gsid = result;
                groupjson.guid = userGroup.Id;
                groupjson.payprice = totalPrice;
                return result;
            }
        }

        /// <summary>
        /// 拼团支付逻辑处理
        /// </summary>
        /// <param name="jsondata"></param>
        /// <param name="ordertype"></param>
        /// <param name="paytype"></param>
        /// <param name="appid"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public string CommandGroupPay(string jsondata, int ordertype, int paytype, string appid, ref CityMorders order)
        {
            if (string.IsNullOrEmpty(jsondata))
            {
                return "json参数错误";
            }

            AddGroupModel groupjson = JsonConvert.DeserializeObject<AddGroupModel>(jsondata);
            if (groupjson == null)
            {
                return "null参数错误";
            }

            Groups group = base.GetModel(groupjson.groupId);
            if (group == null)
            {
                return "拼团商品不存在";
            }

            if (groupjson.groupId <= 0)
            {
                return "拼团参数错误";
            }

            Store store = StoreBLL.SingleModel.GetModel(group.StoreId);
            if (store == null)
            {
                return "店铺不存在";
            }

            if (groupjson.guid <= 0)
            {
                return "用户拼团记录ID不能小于0";
            }

            order.payment_free = groupjson.payprice;
            order.Articleid = groupjson.groupId;
            order.CommentId = groupjson.guid;
            order.MinisnsId = store.Id;
            order.is_group = groupjson.isGroup;
            order.is_group_head = groupjson.isGHead;
            order.groupsponsor_id = groupjson.gsid;
            order.ShowNote = $"小程序拼团付款{order.payment_free * 0.01}元";
            order.buy_num = groupjson.num;
            order.remark = groupjson.addres;
            order.OperStatus = store.Id;
            order.Tusername = groupjson.username;
            order.AttachPar = groupjson.phone;
            order.Note = groupjson.note;

            return "";
        }
        #endregion

        #region 缓存操作

        public override object Add(Groups model)
        {
            string artCheckSum = Utility.CheckSum.ComputeCheckSum(model.CreateUserId + model.GroupName + model.Description + model.DiscountPrice + model.OriginalPrice + model.UnitPrice + model.ValidDateStart + model.ValidDateEnd + model.CreateNum).ToString();
            int exitsValue = RedisUtil.Get<int>(artCheckSum);
            if (exitsValue == 1)//短时间内重复提交数据
            {
                return -4;
            }
            object o = base.Add(model);
            Store store = StoreBLL.SingleModel.GetModel(model.StoreId);
            if (store != null)
            {
                RemoveCache(store.Id);
                RemoveCache(store.CityCode);
            }
            RedisUtil.Set(artCheckSum, 1, TimeSpan.FromMinutes(3));
            return o;
        }

        public override bool Update(Groups model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            Store store = StoreBLL.SingleModel.GetModel(model.StoreId);
            if (store != null)
            {
                RemoveCache(store.Id);
                RemoveCache(store.CityCode);
            }
            return b;
        }
        /// <summary>
        /// 删除优惠拼团（批量）
        /// </summary>
        /// <param name="state"></param>
        /// <param name="id"></param>
        /// <param name="cityAreaCode"></param>
        /// <returns></returns>
        public bool StoreDiscount_DeleteBacth(MiniappState state, string[] id, int cityAreaCode = 0)
        {
            string sql = $"update groups set State={(int)state} where Id in ({string.Join(",", id)})";
            int result = SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);

            if (cityAreaCode > 0)
            {
                RemoveCache(cityAreaCode);
            }
            return result > 0;
        }
        /// <summary>
        /// 清楚缓存
        /// </summary>
        /// <param name="areacode"></param>
        public void RemoveCache(int areacode)
        {
            RedisUtil.SetVersion(string.Format(CacheKeyCGroupsVer, areacode));
        }
        #endregion

    }
}
