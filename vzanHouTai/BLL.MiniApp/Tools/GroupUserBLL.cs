using BLL.MiniApp.Conf;
using BLL.MiniApp.Helper;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Tools;
using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Tools
{
    public class GroupUserBLL : BaseMySql<GroupUser>
    {
        #region 单例模式
        private static GroupUserBLL _singleModel;
        private static readonly object SynObject = new object();

        private GroupUserBLL()
        {

        }

        public static GroupUserBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new GroupUserBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        #region 基础操作

        /// <summary>
        /// 获取还需要几人才能成团
        /// </summary>
        /// <param name="GroupId">拼团ID</param>
        /// <param name="gsid">参团ID</param>
        /// <param name="groupsize">拼团参与人数</param>
        /// <returns></returns>
        public int GetDCount(int GroupId, int gsid, int groupsize)
        {
            var count = GetCount($"GroupId={GroupId} and GroupSponsorId={gsid} and IsGroup=1 and state not in ({(int)MiniappPayState.取消支付},{(int)MiniappPayState.待支付})");
            return groupsize - count;
        }
        /// <summary>
        /// 获取拼团已团数量
        /// </summary>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        public int GetCountPayGroup(int GroupId)
        {
            return GetCount($"GroupId={GroupId} and state not in ({(int)MiniappPayState.取消支付},{(int)MiniappPayState.待支付})");
        }
        /// <summary>
        /// 获取参团所有用户数据
        /// </summary>
        /// <param name="gsid">参团ID</param>
        /// <returns></returns>
        public List<GroupUser> GetListGroupUserList(int gsid)
        {
            string sql = $"GroupSponsorId={gsid} and state not in ({(int)MiniappPayState.取消支付},{(int)MiniappPayState.待支付})";
            return GetList(sql);
        }
        /// <summary>
        /// 根据订单ID获取微信支付的groupuser数据
        /// </summary>
        /// <param name="sgid"></param>
        /// <returns></returns>
        public GroupUser GetModelByOrderId(int orderid)
        {
            return GetModel($"OrderId={orderid}");
        }

        /// <summary>
        /// 购买该拼团用户记录
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public List<GroupUser> GetListByGroupId(int groupid)
        {
            return base.GetList($"GroupId={groupid} and State>-2 and state not in ({(int)MiniappPayState.取消支付},{(int)MiniappPayState.待支付})");
        }
        #endregion

        #region 逻辑操作

        #region  拼团退款
        /// <summary>
        /// 拼团退款
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type">0：拼团失败退款，1：店主手动退款</param>
        /// <returns></returns>
        public bool RefundOne(GroupUser item, ref TransactionModel tranmodel, ref string msg, int type = 0)
        {
            //0：微信支付，1：储值卡支付
            int paytype = item.PayType;

            //TransactionModel tranmodel = new MiniApp.TransactionModel();
            Groups csg = GroupsBLL.SingleModel.GetModel(item.GroupId);
            if (csg == null)
            {
                msg = "小程序拼团商品不存在啦=" + item.GroupId;
                //item.State = (int)MiniappPayState.已失效;
                //Update(item, "State");
                return false;
            }
            GroupSponsor gsinfo = GroupSponsorBLL.SingleModel.GetModel(item.GroupSponsorId);
            if (gsinfo == null )//&& item.IsGroup == 1)
            {
                msg = "小程序拼团团购不存在啦=" + item.GroupSponsorId;
                //item.State = (int)MiniappPayState.已失效;
                //Update(item, "State");
                return false;
            }

            if (item.BuyPrice <= 0)
            {
                msg = "xxxxxxxxxxxxx小程序拼团价格为0不需要退款=" + item.Id;
                return false;
            }

            if (item.PayState == (int)MiniappPayState.已退款)
            {
                msg = "xxxxxxxxxxxxx小程序拼团状态有误，不能退款=" + item.Id + ",paystate=" + item.PayState + "," + (int)MiniappPayState.已退款;
                return false;
            }

            item.State = (int)MiniappPayState.已退款;
            //更新用户订单状态
            tranmodel.Add($"update GroupUser set State={item.State} where id={item.Id}");

            //判断是否是微信支付
            if (paytype == 0)
            {
                CityMordersBLL mbll = new CityMordersBLL();
                CityMorders order = mbll.GetModel(item.OrderId);
                if (order == null)
                {
                    msg = "xxxxxxxxxxxxxxxxxx小程序拼团退款查不到支付订单 ID=" + item.Id;
                    item.State = (int)MiniappPayState.已失效;
                    Update(item, "State");
                    return false;
                }

                //插入退款队列
                ReFundQueue reModel = new ReFundQueue();
                reModel.minisnsId = -5;
                reModel.money = item.BuyPrice;
                reModel.orderid = item.OrderId;
                reModel.traid = order.trade_no;
                reModel.addtime = DateTime.Now;
                reModel.note = "小程序拼团退款";
                reModel.retype = 1;
                tranmodel.Add(new ReFundQueueBLL().BuildAddSql(reModel));
            }
            else if (paytype == 1)
            {
                //储值卡退款
                tranmodel.Add(SaveMoneySetUserBLL.SingleModel.GetCommandCarPriceSql(item.AppId, item.ObtainUserId, item.BuyPrice, 1, item.OrderId, item.OrderNo).ToArray());
                if (tranmodel.sqlArray.Length <= 0)
                {
                    msg = "xxxxxxxxxxxxxxxxxx拼团储值卡退款失败，ID=" + item.Id;
                    return false;
                }
            }

            //是店主手动退款不加库存 --统一,只要是退款就加库存
            //if (type == 0)
            {
                if (gsinfo.State == 2 && item.IsGroup == 1)
                {
                    msg = "小程序团购成功，不能退款=" + item.GroupSponsorId;
                    return false;
                }

                //退款成功，更新剩余数量
                tranmodel.Add($"update groups set RemainNum ={(csg.RemainNum + item.BuyNum)} where id={csg.Id}");
                //LogHelper.WriteInfo(GetType(), $"修改拼团失败库存：update groups set RemainNum ={(csg.RemainNum + item.BuyNum)} where id={csg.Id}");
            }

            if (tranmodel.sqlArray.Length <= 0)
            {
                msg = "xxxxxxxxxxxxxxxxxx拼团退款失败，ID=" + item.Id;
                return false;
            }

            //if (!ExecuteTransactionDataCorect(tranmodel.sqlArray, tranmodel.ParameterArray))
            //{
            //    msg = "xxxxxxxxxxxxxxxxxx拼团退款事务执行失败，ID=" + item.Id + "sql:" + string.Join(";", tranmodel.sqlArray);
            //    return false;
            //}

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(item.AppId);
            if (xcx == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"发送模板消息,参数不足,XcxAppAccountRelation_null:appId = {item.AppId}"));
                return true;
            }

            //发给用户发货通知
            object groupData = TemplateMsg_Miniapp.GroupGetTemplateMessageData("商家操作退款", item, SendTemplateMessageTypeEnum.拼团基础版订单退款通知);
            TemplateMsg_Miniapp.SendTemplateMessage(item.ObtainUserId, SendTemplateMessageTypeEnum.拼团基础版订单退款通知, xcx.Type, groupData);
            msg = "xxxxxxxxxxxxxxxxxx拼团退款成功，ID=" + item.Id;
            return true;
        }

        public bool RefundOne(GroupUser item, ref string msg, int type = 0)
        {
            //0：微信支付，1：储值卡支付
            int paytype = item.PayType;
            TransactionModel tranmodel = new MiniApp.TransactionModel();
            Groups csg = GroupsBLL.SingleModel.GetModel(item.GroupId);
            if (csg == null)
            {
                msg = "小程序拼团商品不存在啦=" + item.GroupId;
                item.State = (int)MiniappPayState.已失效;
                Update(item, "State");
                return false;
            }
            GroupSponsor gsinfo = GroupSponsorBLL.SingleModel.GetModel(item.GroupSponsorId);
            if (gsinfo == null)//&& item.IsGroup == 1)
            {
                msg = "小程序拼团团购不存在啦=" + item.GroupSponsorId;
                item.State = (int)MiniappPayState.已失效;
                Update(item, "State");
                return false;
            }
            if (item.BuyPrice <= 0)
            {
                msg = "xxxxxxxxxxxxx小程序拼团价格为0不需要退款=" + item.Id;
                return false;
            }
            if (item.PayState == (int)MiniappPayState.已退款)
            {
                msg = "xxxxxxxxxxxxx小程序拼团状态有误，不能退款=" + item.Id + ",paystate=" + item.PayState + "," + (int)MiniappPayState.已退款;
                return false;
            }
            item.State = (int)MiniappPayState.已退款;
            //更新用户订单状态
            tranmodel.Add($"update GroupUser set State={item.State} where id={item.Id}");
            
            //判断是否是微信支付
            if (paytype == 0)
            {
                CityMordersBLL mbll = new CityMordersBLL();
                CityMorders order = mbll.GetModel(item.OrderId);
                if (order == null)
                {
                    msg = "xxxxxxxxxxxxxxxxxx小程序拼团退款查不到支付订单 ID=" + item.Id;
                    item.State = (int)MiniappPayState.已失效;
                    Update(item, "State");
                    return false;
                }
                //插入退款队列
                ReFundQueue reModel = new ReFundQueue();
                reModel.minisnsId = -5;
                reModel.money = item.BuyPrice;
                reModel.orderid = item.OrderId;
                reModel.traid = order.trade_no;
                reModel.addtime = DateTime.Now;
                reModel.note = "小程序拼团退款";
                reModel.retype = 1;
                tranmodel.Add(new ReFundQueueBLL().BuildAddSql(reModel));
            }
            else if (paytype == 1)
            {
                //储值卡退款
                tranmodel.Add(SaveMoneySetUserBLL.SingleModel.GetCommandCarPriceSql(item.AppId, item.ObtainUserId, item.BuyPrice, 1, item.OrderId, item.OrderNo).ToArray());
                if (tranmodel.sqlArray.Length <= 0)
                {
                    msg = "xxxxxxxxxxxxxxxxxx拼团储值卡退款失败，ID=" + item.Id;
                    return false;
                }
            }

            //是店主手动退款不加库存 --统一,只要是退款就加库存
            //if (type == 0)
            {
                if (gsinfo.State == 2 && item.IsGroup == 1)
                {
                    msg = "小程序团购成功，不能退款=" + item.GroupSponsorId;
                    return false;
                }
                
                //退款成功，更新剩余数量
                tranmodel.Add($"update groups set RemainNum ={(csg.RemainNum + item.BuyNum)} where id={csg.Id}");
            }

            if (tranmodel.sqlArray.Length <= 0)
            {
                msg = "xxxxxxxxxxxxxxxxxx拼团退款失败，ID=" + item.Id;
                return false;
            }
            
            if (!base.ExecuteTransaction(tranmodel.sqlArray, tranmodel.ParameterArray))
            {
                msg = "xxxxxxxxxxxxxxxxxx拼团退款事务执行失败，ID=" + item.Id + "sql:" + string.Join(";", tranmodel.sqlArray);
                return false;
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(item.AppId);
            if (xcx == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"发送模板消息,参数不足,XcxAppAccountRelation_null:appId = {item.AppId}"));
                return true;
            }

            //发给用户发货通知
            object groupData = TemplateMsg_Miniapp.GroupGetTemplateMessageData("商家操作退款", item, SendTemplateMessageTypeEnum.拼团基础版订单退款通知);
            TemplateMsg_Miniapp.SendTemplateMessage(item.ObtainUserId, SendTemplateMessageTypeEnum.拼团基础版订单退款通知, xcx.Type, groupData);
            msg = "xxxxxxxxxxxxxxxxxx拼团退款成功，ID=" + item.Id;
            return true;
        }

        #endregion
        /// <summary>
        /// 查我的拼团数据
        /// </summary>
        /// <param name="where"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strOrder"></param>
        /// <returns></returns>
        public List<GroupUser> GetJoinList(string where, int pageSize, int pageIndex, string strOrder)
        {
            var sql = $"select a.Id,a.GroupId,a.GroupSponsorId,a.DiscountGuid,a.BuyNum,a.BuyPrice,a.IsGroup,a.IsGroupHead,a.State,b.ImgUrl,b.GroupName,b.DiscountPrice,b.UnitPrice,b.OriginalPrice,b.GroupSize,b.ValidDateStart,b.ValidDateEnd,b.StoreId,g.EndDate,g.State as PState,b.State as GState,b.CreateNum,b.RemainNum from (groupuser a LEFT join groups b on a.GroupId=b.id) LEFT JOIN store c on b.StoreId=c.id LEFT JOIN groupsponsor g on g.Id=a.GroupSponsorId where {where} order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
                List<GroupUser> list = new List<GroupUser>();
                var gurBLL = new GroupUserBLL();
                while (dr.Read())
                {
                    GroupUser groups = GetModel(dr);
                    groups.Name = dr["GroupName"].ToString();
                    groups.DiscountPrice = Convert.ToInt32(dr["DiscountPrice"]);
                    groups.UnitPrice = Convert.ToInt32(dr["UnitPrice"]);
                    groups.OriginalPrice = Convert.ToInt32(dr["OriginalPrice"]);
                    groups.GroupSize = Convert.ToInt32(dr["GroupSize"]);
                    //已团人数
                    //groups.HaveNum = gurBLL.GetCount($"GroupId={groups.GroupId} and GroupSponsorId={groups.GroupSponsorId} and IsGroup=1");
                    if (DBNull.Value != dr["RemainNum"] && DBNull.Value != dr["CreateNum"])
                    {
                        groups.HaveNum = Convert.ToInt32(dr["CreateNum"]) - Convert.ToInt32(dr["RemainNum"]);
                    }

                    groups.PState = 0;
                    groups.GState = Convert.ToInt32(dr["GState"].ToString());
                    if (!string.IsNullOrEmpty(dr["PState"].ToString()))
                    {
                        groups.PState = Convert.ToInt32(dr["PState"]);
                    }
                    groups.ShopLogoUrl = dr["ImgUrl"].ToString();
                    groups.ShowTime = Convert.ToDateTime(dr["ValidDateEnd"]).ToString("yyyy-MM-dd");
                    groups.StartUseTime = Convert.ToDateTime(dr["ValidDateStart"]);
                    if (!string.IsNullOrEmpty(groups.ShopLogoUrl))
                    {
                        if (groups.ShopLogoUrl.Contains("oss.vzan"))
                        {
                            groups.ShopLogoUrl = groups.ShopLogoUrl.Replace("oss.", "img.") + "@1e_1c_0o_0l_100sh_120h_120w_90q.src";
                        }
                        if (groups.ShopLogoUrl.Contains("i.vzan") && !groups.ShopLogoUrl.Contains("@!"))
                        {
                            groups.ShopLogoUrl = groups.ShopLogoUrl + "@!120x120";
                        }
                    }

                    //参团结束时间
                    groups.ShowDate = "";
                    if (!string.IsNullOrEmpty(dr["EndDate"].ToString()))
                    {
                        groups.ShowDate = Convert.ToDateTime(dr["EndDate"].ToString()).ToString("yyyy-MM-dd HH:mm");
                    }

                    if (groups.PState == 2)
                    {
                        groups.MState = 2;
                    }
                    else if (groups.PState == 1 && !string.IsNullOrEmpty(groups.ShowDate) && Convert.ToDateTime(groups.ShowDate) > DateTime.Now)
                    {
                        groups.MState = 1;
                    }
                    else if (groups.PState == 0)
                    {
                        groups.MState = 0;
                    }
                    else
                    {
                        groups.MState = -1;
                    }

                    list.Add(groups);
                }
                return list;
            }
        }
        //核销/购买记录  后台调用
        public List<GroupUser> getcguRList(string wheresql, int pagesize, int pageindex)
        {
            //购买记录
            string strOrder = "a.CreateDate Desc,b.StartDate Desc";
            var sql = $"select a.Id,a.GroupId,a.GroupSponsorId,a.BuyNum,a.BuyPrice,a.orderno,a.IsGroup,a.note,a.IsGroupHead,a.ObtainUserId,a.ValidTime,a.ValidUserOpenId,a.ValidUserNickName,a.CreateDate,a.PayTime,a.SendGoodTime,a.RecieveGoodTime,a.State,a.Address,a.Phone,a.UserName,b.State as PState,b.StartDate,b.EndDate from groupuser a LEFT join groupsponsor b on a.GroupSponsorId=b.id where {wheresql} order by {strOrder} LIMIT {(pageindex - 1) * pagesize},{pagesize}";
            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
                List<GroupUser> list = new List<GroupUser>();
                while (dr.Read())
                {
                    GroupUser groups = GetModel(dr);
                    groups.ShowDate = groups.CreateDate.ToString("MM-dd");
                    groups.ShowTime = groups.CreateDate.ToString("HH:mm:ss");
                    var user = C_UserInfoBLL.SingleModel.GetModel(groups.ObtainUserId);
                    if (DBNull.Value != dr["PState"])
                    {
                        groups.PState = Convert.ToInt32(dr["PState"].ToString());
                    }
                    if (DBNull.Value != dr["EndDate"])
                    {
                        groups.EndDate = DateTime.Parse(dr["EndDate"].ToString());
                    }

                    groups.NickName = "未知用户";
                    if (user != null)
                    {
                        groups.NickName = user.NickName;
                        groups.ShopLogoUrl = user.HeadImgUrl.IsNullOrWhiteSpace() ? "//j.vzan.cc/content/city/images/voucher/02.png" : user.HeadImgUrl;
                        groups.QrCodeUrl = user.TelePhone;
                    }

                    //判断是否单买商品
                    if (groups.IsGroup == 0)
                    {
                        groups.PState = 3;
                    }
                    else if (groups.PState == 1 && groups.EndDate < DateTime.Now)
                    {
                        groups.PState = -1;
                    }

                    list.Add(groups);
                }
                return list;
            }
        }

        /// <summary>
        /// 获取已确认收货并未评价的购物车数据，每次获取前1000条
        /// </summary>
        /// <returns></returns>
        public List<GroupUser> GetSuccessDataList(int iscomment = 0, int day = -15)
        {
            List<GroupUser> list = new List<GroupUser>();
            string sqlwhere = "";
            //1:已评论,0:未评论
            if (iscomment >= 0)
            {
                sqlwhere = $" and gu.iscommentting={iscomment} ";
            }
            string sql = $"select gu.*,g.groupname,g.imgurl,s.appid aid from groupuser gu left join groups g on gu.groupid = g.id left join store s on g.storeid = s.id where gu.state={(int)MiniappPayState.已收货} and gu.recievegoodtime<='{DateTime.Now.AddDays(day)}' {sqlwhere} LIMIT 100";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    GroupUser amodel = base.GetModel(dr);
                    amodel.GroupName = dr["groupname"].ToString();
                    amodel.GroupImgUrl = dr["imgurl"].ToString();
                    if (dr["aid"] != DBNull.Value)
                    {
                        amodel.AId = Convert.ToInt32(dr["aid"]);
                    }
                    list.Add(amodel);
                }
            }

            return list;
        }
        #endregion

        #region 商家版小程序

        public int GetPriceSumByAppId(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return 0;
            }
            int priceSum = 0;
            List<MySqlParameter> param = new List<MySqlParameter>();
            param.Add(new MySqlParameter("@appId", appId));

            string sql = $"select sum(BuyPrice) pricesum from groupuser where appid=@appId and State=-1";
            using (var dr = SqlMySql.ExecuteDataReader(dbEnum.MINIAPP.ToString(), CommandType.Text, sql, param.ToArray()))
            {
                while (dr.Read())
                {

                    priceSum = dr["pricesum"] == DBNull.Value ? 0 : Convert.ToInt32(dr["pricesum"]);
                }
            }
            return priceSum;
        }
        /// <summary>
        /// 获取已完成拼团总订单数
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public int GetGroupOrderSum(string appId, int state = -1, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            string sqlwhere = $"appid=@appid and state={state}";
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlwhere += " and PayTime>=@startDate and PayTime<=@endDate";
                paramters.Add(new MySqlParameter("@startDate", startDate));
                paramters.Add(new MySqlParameter("@endDate", endDate));
            }
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }
        public int GetGroupOrderSum(string appId, string states, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(states))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            string sqlwhere = $"appid=@appid and state in ({states})";
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                //sqlwhere += " and PayTime>=@startDate and PayTime<=@endDate";
                sqlwhere += " and PayTime between @startDate and @endDate";
                paramters.Add(new MySqlParameter("@startDate", startDate));
                paramters.Add(new MySqlParameter("@endDate", endDate));
            }
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }

        /// <summary>
        /// 获取小程序指定时间段内的订单数
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetGroupOrderSum(string appId, string startDate, string endDate)
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return count;
            }
            //string sqlwhere = $"appid=@appid and PayTime>=@startDate and PayTime<=@endDate";
            string sqlwhere = $"appid=@appid and PayTime between @startDate and @endDate";

            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            paramters.Add(new MySqlParameter("@startDate", startDate));
            paramters.Add(new MySqlParameter("@endDate", endDate));
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }
        /// <summary>
        /// 根据条件获取相应的订单数量
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetOrderSumByCondition(string appId, int type, string value, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            string sql = $"select count(1) from groupuser a left join GroupSponsor b on a.GroupSponsorId=b.Id where ";
            string sqlwhere = GetSqlwhere(paramters, appId, -999, type, value, startDate, endDate);
            sql += sqlwhere;
            if (sqlwhere == null)
            {
                return count;
            }
            count = GetCountBySql(sql, paramters.ToArray());
            return count;
        }

        public object GetListByCondition(string appId, int pageIndex, int pageSize, int state, int type, string value, string startDate, string endDate)
        {


            List<GroupUser> groupUserList = new List<GroupUser>();
            if (string.IsNullOrEmpty(appId))
            {
                return groupUserList;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            string sql = $" select a.* from groupUser a left join GroupSponsor b on a.GroupSponsorId=b.id  where ";
            string sqlwhere = GetSqlwhere(paramters, appId, state, type, value, startDate, endDate);
            sql += $" {sqlwhere} order by id desc limit {(pageIndex - 1) * pageSize},{pageSize}";
            using (var dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql,paramters.ToArray()))
            {
                groupUserList = GetList(dr);
            }
            if (groupUserList != null && groupUserList.Count > 0)
            {
                string sponsorIds = string.Join(",",groupUserList.Select(s=>s.GroupSponsorId).Distinct());
                List<GroupSponsor> groupSponsorList = GroupSponsorBLL.SingleModel.GetListByIds(sponsorIds);

                string groupIds = string.Join(",",groupUserList.Select(s=>s.GroupId).Distinct());
                List<Groups> groupsList = GroupsBLL.SingleModel.GetListByIds(groupIds);

                foreach (GroupUser groupUser in groupUserList)
                {
                    GroupSponsor groupSponsor = groupSponsorList?.FirstOrDefault(f=>f.Id == groupUser.GroupSponsorId);
                    if (groupSponsor != null)
                    {
                        groupUser.PState = groupSponsor.State;
                        groupUser.EndDate = groupSponsor.EndDate;
                    }
                    Groups group = groupsList?.FirstOrDefault(f=>f.Id == groupUser.GroupId);
                    groupUser.GroupName = group.GroupName;
                    if (groupUser.GroupSponsorId == 0)//一键成团
                    {
                        groupUser.PState = 3;
                    }
                    else if (groupUser.PState == 1 && groupUser.EndDate < DateTime.Now)//拼团过期
                    {
                        groupUser.PState = -1;
                    }
                }
            }
            return groupUserList;
        }
        private string GetSqlwhere(List<MySqlParameter> paramters, string appId, int state, int type, string value, string startDate = "", string endDate = "")
        {
            string sqlwhere = $" a.appId=@appid";
            paramters.Add(new MySqlParameter("@appid", appId));
            switch (state)//-2：拼团失败，2：拼团中，0：待发货，1：待收货，-1：已收货  
            {
                case -2://拼团失败（屏蔽了取消支付的订单）
                    sqlwhere += " and ((b.state!=1 and b.state!=2 and b.state!=3) or (b.state=1&& enddate<now())) and a.state!=5";
                    break;
                case 2://拼团中
                    sqlwhere += " and (b.state=1 && enddate>now())";
                    break;
                case 0://待发货
                    sqlwhere += " and (b.state=2 or b.state=3 or a.GroupSponsorId=0) and a.state=0 ";
                    break;
                case 1://待收货
                    sqlwhere += " and (b.state=2 or b.state=3 or a.GroupSponsorId=0) and a.state=1";
                    break;
                case -1://已收货
                    sqlwhere += " and (b.state=2 or b.state=3 or a.GroupSponsorId=0) and a.state=-1";
                    break;
                default://（屏蔽了取消支付的订单）
                    sqlwhere += $" and a.state!=5";
                    break;
            }
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlwhere += " and a.PayTime>=@startDate and a.PayTime<=@endDate";
                paramters.Add(new MySqlParameter("@startDate", startDate));
                paramters.Add(new MySqlParameter("@endDate", endDate));
            }
            if (!string.IsNullOrEmpty(value))
            {
                switch (type)
                {
                    case 0://订单号
                        sqlwhere += " and a.OrderNo like @orderNo";
                        paramters.Add(new MySqlParameter("@orderNo", $"%{value}%"));
                        break;
                    case 1://商品名称
                        List<Groups> groupsList = GroupsBLL.SingleModel.GetList($"a.GroupName like '%{value}%'");
                        if (groupsList == null || groupsList.Count <= 0) return null;
                        sqlwhere += $" and a.id in ({string.Join(",", groupsList.Select(c => c.Id))}) ";
                        break;
                    case 2://收货电话                       
                        sqlwhere += $" and a.phone like @phone";
                        paramters.Add(new MySqlParameter("@phone", $"%{value}%"));
                        break;
                    case 3://收货人
                        sqlwhere += $" and a.username like @username";
                        paramters.Add(new MySqlParameter("@username", $"%{value}%"));
                        break;
                }
            }
            return sqlwhere;
        }

        /// <summary>
        /// 根据条件和订单状态获取相应的订单数
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int GetOrderSumByCondition(string appId, int type, string value, int state)
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            string sql = "select count(1) from groupuser a left join GroupSponsor b on a.GroupSponsorId=b.Id where ";
            string sqlwhere = GetSqlwhere(paramters, appId, state, type, value);
            sql += sqlwhere;
            count = GetCountBySql(sql, paramters.ToArray());
            return count;
        }
        /// <summary>
        /// 获取已支付的用户数量
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetPayUserCount(string appId, int aid, string startDate, string endDate)
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || aid <= 0)
            {
                return count;
            }
            //string sql = $"select count(1) from (select ObtainUserId as userId from groupuser where appid = @appid and State in (-1, 0, 1)  and PayTime>= @startDate and PayTime<= @endDate union select userId from entgoodsorder where appid = @appid and State in (1, 2, 3, 4, 5) and CreateDate >= @startDate and CreateDate <= @endDate union select c.userid as userId from bargainuser c LEFT JOIN bargain d on c.BId = d.id where d.StoreId ={aid} and c.state in (1, 6, 7, 8) and c.BuyTime >= @startDate and c.BuyTime <= @endDate) a";
            string sql = $"select count(1) from (select ObtainUserId as userId from groupuser where appid = @appid and State in (-1, 0, 1)  and PayTime between @startDate and @endDate union select userId from entgoodsorder where appid = @appid and State in (1, 2, 3, 4, 5) and CreateDate between @startDate and @endDate union select c.userid as userId from bargainuser c LEFT JOIN bargain d on c.BId = d.id where d.StoreId ={aid} and c.state in (1, 6, 7, 8) and c.BuyTime between @startDate and @endDate) a";

            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            paramters.Add(new MySqlParameter("@startDate", startDate));
            paramters.Add(new MySqlParameter("@endDate", endDate));
            var result = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(), CommandType.Text, sql, paramters.ToArray());
            if (result != null)
            {
                count = Convert.ToInt32(result);
            }
            return count;
        }

        /// <summary>
        /// 获取小程序指定时间内完成订单总收入
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="payState"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetPriceSumByAppId_Date(string appId, string startDate, string endDate)
        {
            int priceSum = 0;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return priceSum;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appId", appId));
            paramters.Add(new MySqlParameter("@startDate", startDate));
            paramters.Add(new MySqlParameter("@endDate", endDate));

            //string sql = $"select sum(BuyPrice) pricesum from groupuser where appid=@appId and State=-1 and PayTime>=@startDate and PayTime<=@endDate";
            string sql = $"select sum(BuyPrice) pricesum from groupuser where appid=@appId and State=-1 and PayTime between @startDate and @endDate";
            
            using (var dr = SqlMySql.ExecuteDataReader(dbEnum.MINIAPP.ToString(), CommandType.Text, sql, paramters.ToArray()))
            {
                while (dr.Read())
                {

                    priceSum = dr["pricesum"] == DBNull.Value ? 0 : Convert.ToInt32(dr["pricesum"]);
                }
            }
            return priceSum;
        }

        #endregion

    }
}
