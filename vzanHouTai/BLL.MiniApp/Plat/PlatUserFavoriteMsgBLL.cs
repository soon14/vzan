using BLL.MiniApp.Im;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Im;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
    public class PlatUserFavoriteMsgBLL : BaseMySql<PlatUserFavoriteMsg>
    {
        #region 单例模式
        private static PlatUserFavoriteMsgBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatUserFavoriteMsgBLL()
        {

        }

        public static PlatUserFavoriteMsgBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatUserFavoriteMsgBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        
        /// <summary>
        /// 我点赞，关注，收藏总数量
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="actiontype"></param>
        /// <returns></returns>
        public int GetMyMsgCount(long userid, int actiontype, int datatype)
        {
            string sqlwhere = $"userid={userid} and state>=0 and actiontype={actiontype} and datatype={datatype}";
            return base.GetCount(sqlwhere);
        }
        /// <summary>
        /// 我的名片数据分析
        /// </summary>
        /// <param name="msgid"></param>
        /// <param name="aid"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public List<PlatUserFavoriteMsg> GetReportData(int msgid, int aid, int datatype, string starttime, string endtime)
        {
            List<PlatUserFavoriteMsg> list = new List<PlatUserFavoriteMsg>();
            string sql = $"select count(*) count,actiontype from PlatUserFavoriteMsg where aid={aid} and msgid={msgid} and datatype={datatype} and state>=0 GROUP BY actiontype";
            if (!string.IsNullOrEmpty(starttime))
            {
                sql += $" and addtime >='{starttime}'";
            }
            if (!string.IsNullOrEmpty(endtime))
            {
                sql += $" and addtime <'{endtime}'";
            }

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    if (dr["actiontype"] != DBNull.Value)
                    {
                        if (dr["count"] != DBNull.Value)
                        {
                            PlatUserFavoriteMsg tempmodel = new PlatUserFavoriteMsg();
                            tempmodel.ActionType = Convert.ToInt32(dr["actiontype"]);
                            tempmodel.Count = Convert.ToInt32(dr["count"]);
                            list.Add(tempmodel);
                        }
                    }
                }
            }

            return list;
        }
        /// <summary>
        /// 获取访客人数
        /// </summary>
        /// <returns></returns>
        public int GetVisitorCount(int msgid, int aid, int datatype, string starttime, string endtime)
        {
            string sql = $"select Count(DISTINCT(userid)) from PlatUserFavoriteMsg where aid={aid} and msgid={msgid} and datatype={datatype}  and actiontype=3 and state>=0";
            if (!string.IsNullOrEmpty(starttime))
            {
                sql += $" and addtime >='{starttime}'";
            }
            if (!string.IsNullOrEmpty(endtime))
            {
                sql += $" and addtime <'{endtime}'";
            }

            object result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, null);
            if (result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        /// <summary>
        /// 店铺，帖子，名片点赞，关注，收藏总量
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="actiontype"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public int GetUserMsgCount(int aid, int msgid, int actiontype, int datatype, int userid = 0)
        {
            string sqlwhere = $"aid={aid} and state>=0 and actiontype={actiontype} and datatype={datatype}";
            if (msgid > 0)
            {
                sqlwhere += $" and msgid={msgid}";
            }
            if (userid > 0)
            {
                sqlwhere += $" and userid={userid}";
            }
            return base.GetCount(sqlwhere);
        }

        /// <summary>
        /// 互相关注
        /// </summary>
        /// <returns></returns>
        public int GetMutualFollowCount(int aid, int userid, int datatype = (int)PointsDataType.名片)
        {
            string sql = $@"select count(*) from (select m.userid,c.userid carduserid from PlatUserFavoriteMsg m left join               platmycard c on m.msgid = c.id 
                        where m.aid = {aid} and m.datatype = {datatype} and m.actiontype={(int)PointsActionType.关注} and m.state=0 and m.userid={userid}) t1
                          INNER JOIN
                          (select m.userid, c.userid carduserid from PlatUserFavoriteMsg m left
                                                                join platmycard c on m.msgid = c.id
                          where m.aid = {aid} and m.datatype = {datatype} and m.actiontype={(int)PointsActionType.关注} and m.state=0) t2
                           where t1.userid = t2.carduserid and t1.carduserid = t2.userid";
            object result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, null);
            if (result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        /// <summary>
        /// 获取一条收藏或者点赞记录
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="msgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public PlatUserFavoriteMsg GetUserFavoriteMsg(int aid, int msgId, int userId, int actionType = 0, int datatype = (int)PointsDataType.帖子)
        {
            PlatUserFavoriteMsg model = base.GetModel($"aid={aid} and userId={userId} and msgId={msgId} and actionType={actionType} and datatype={datatype}");
            return model;
        }
        public List<PlatUserFavoriteMsg> GetUserFavoriteMsgList(int aid, string msgIds, int userId, int datatype = (int)PointsDataType.帖子)
        {
            return base.GetList($"aid={aid} and userId={userId} and msgId in ({msgIds})  and datatype={datatype}");
        }

        public List<PlatUserFavoriteMsg> GetMyCardUserFavoriteList(string appid, string lng, string lat, int myuserid, int type, int aid, int msgid, int actiontype, int datatype, int pageindex, int pagesize, ref int count)
        {
            PlatMyCardBLL.SingleModel.RefleshCach(msgid, actiontype, false);
            string sql = $"select f.*,m.imgurl,m.jobtype,m.Name cardname,m.Address cardaddress,m.Lat cardlat,m.Lng cardlng,m.Phone cardphone,m.CompanyName cardcompanyname,m.Job cardjob,m.Department carddepartment,m.IndustryId cardindustryid,m.HiddenPhone cardhiddenphone,f.userid carduserid,GetDistance({lng},{lat},m.lng,m.lat) distance from PlatUserFavoriteMsg f left join platmycard m on f.userid=m.userid ";
            string sqlcount = $"select Count(*) from PlatUserFavoriteMsg f left join platmycard m on f.userid=m.userid";
            string sqlwhere = $" where f.aid={aid}  and f.state=0 and f.datatype={datatype} and f.actiontype={actiontype} and m.appid='{appid}'";
            string sqllimit = $" LIMIT {(pageindex - 1) * pagesize},{pagesize} ";
            string sqlorderby = " order by f.addtime desc ";

            if (type == 0)
            {
                sqlwhere += $" and f.msgid={msgid}";
            }
            else
            {
                sqlwhere += $" and f.userid={myuserid}";
            }

            List<PlatUserFavoriteMsg> list = new List<PlatUserFavoriteMsg>();
            count = base.GetCountBySql(sqlcount + sqlwhere);
            if (count <= 0)
            {
                return list;
            }
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqlorderby + sqllimit, null))
            {
                while (dr.Read())
                {
                    PlatUserFavoriteMsg model = base.GetModel(dr);
                    model.AddTimeStr = DateHelper.GetDateTimeFormat(model.AddTime);

                    PlatMyCard mycard = new PlatMyCard();
                    if (type == 1)
                    {
                        if (dr["msgid"] != DBNull.Value)
                        {
                            mycard = PlatMyCardBLL.SingleModel.GetModel(Convert.ToInt32(dr["msgid"].ToString()));
                        }
                        if (mycard == null)
                            continue;
                    }
                    else
                    {
                        mycard.UserId = model.UserId;
                    }

                    mycard = PlatMyCardBLL.SingleModel.GetMyCardData(mycard.UserId, myuserid, aid, datatype);
                    if (DBNull.Value != dr["distance"])
                    {
                        int tempdistance = Convert.ToInt32(dr["distance"]);
                        mycard.Distance = DataHelper.GetDistanceFormat(tempdistance, 1);
                    }
                    else
                    {
                        mycard.Distance = "0m";
                    }
                    model.MyCardModel = mycard;

                    list.Add(model);
                }
            }

            if (list == null || list.Count <= 0)
            {
                return list;
            }

            //行业
            string industryids = string.Join(",", list.Select(s => s.MyCardModel.IndustryId).Distinct());
            List<PlatIndustry> platIndustrylist = PlatIndustryBLL.SingleModel.GetListByIds(industryids);
            string cardids = string.Join(",", list.Select(s => s.MyCardModel.Id).Distinct());
            List<PlatStore> platstorelist = PlatStoreBLL.SingleModel.GetListByCardId(cardids);

            foreach (PlatUserFavoriteMsg item in list)
            {
                PlatStore store = platstorelist?.FirstOrDefault(f => f.MyCardId == item.MyCardModel.Id);
                item.MyCardModel.StoreId = store != null ? store.Id : 0;
                PlatIndustry industryitem = platIndustrylist?.FirstOrDefault(f => f.Id == item.MyCardModel.IndustryId);
                if (industryitem != null)
                {
                    item.MyCardModel.IndustryName = industryitem.Name;
                }
            }

            return list;
        }

        public List<PlatUserFavoriteMsg> GetMyCardUserFavoriteList(int ver, int cardId, int myuserid, int aid, int pageindex, int pagesize, ref int count)
        {
            PlatMyCardBLL.SingleModel.RefleshCach(cardId, (int)PointsActionType.私信, false);
            List<PlatUserFavoriteMsg> list = new List<PlatUserFavoriteMsg>();
            List<ImMessage> immessagelist = new List<ImMessage>();
            //版本控制
            if (ver > 0)
            {
                C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(myuserid);
                if (userinfo == null)
                    return list;
                immessagelist = ImMessageBLL.SingleModel.GetListCardByTuseridV2(userinfo.appId, myuserid, pagesize, pageindex, ref count);
            }
            else
            {
                immessagelist = ImMessageBLL.SingleModel.GetListCardByTuserid(myuserid, pagesize, pageindex, ref count);
                if (count <= 0)
                {
                    return list;
                }
            }

            if (immessagelist == null || immessagelist.Count <= 0)
                return list;
            foreach (ImMessage item in immessagelist)
            {
                PlatUserFavoriteMsg model = new PlatUserFavoriteMsg();
                model.Id = item.Id;
                model.MsgId = item.tuserId;
                model.UserId = item.fuserId;
                // model.AddTime = DateTime.Parse(item.createDate);
                //model.AddTimeStr = DateHelper.GetDateTimeFormat(model.AddTime);
                model.ImMessage = item;
                list.Add(model);
            }

            if (list == null || list.Count <= 0)
                return list;

            return list;
        }
        public List<PlatUserFavoriteMsg> GetMsgUserFavoriteList(int type, int aid, int msgid, int actiontype, int datatype, int pageindex, int pagesize, ref int count)
        {
            string sql = $"select f.*,c.imgurl,c.name,c.jobtype from PlatUserFavoriteMsg f left join platmsg m on f.userid=m.id left join platmycard c on f.userId=c.userid";
            string sqlcount = $"select Count(*) from PlatUserFavoriteMsg f left join platmsg m on f.userid=m.id left join platmycard c on f.userId=c.userid";
            string sqlwhere = $" where f.aid={aid}  and f.state=0 and f.datatype={datatype} and f.actiontype={actiontype} ";
            string sqllimit = $" LIMIT {(pageindex - 1) * pagesize},{pagesize} ";

            if (type == 0)
            {
                sqlwhere += $" and f.msgid={msgid}";
            }
            else
            {
                sqlwhere += $" and f.userid={msgid}";
            }

            List<PlatUserFavoriteMsg> list = new List<PlatUserFavoriteMsg>();
            count = base.GetCountBySql(sqlcount + sqlwhere);
            if (count <= 0)
            {
                return list;
            }


            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqllimit, null))
            {
                while (dr.Read())
                {
                    PlatUserFavoriteMsg model = base.GetModel(dr);
                    PlatMyCard mycard = new PlatMyCard();
                    C_UserInfo c_UserInfo = new C_UserInfo();
                    if (dr["imgurl"] != DBNull.Value&&!string.IsNullOrEmpty(dr["imgurl"].ToString()))
                    {
                        mycard.ImgUrl = dr["imgurl"].ToString();
                    }
                    else
                    {
                        c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(model.UserId);
                        if (c_UserInfo != null)
                        {
                            mycard.ImgUrl = c_UserInfo.HeadImgUrl;
                        }
                    }
                    if (dr["name"] != DBNull.Value && !string.IsNullOrEmpty(dr["name"].ToString()))
                    {
                        mycard.NickName = dr["name"].ToString();
                    }
                    else
                    {
                        c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(model.UserId);
                        if (c_UserInfo != null)
                        {
                            mycard.NickName = c_UserInfo.NickName;
                        }
                           
                    }

                    if (dr["jobtype"] != DBNull.Value)
                    {
                        mycard.JobType = Convert.ToInt32(dr["jobtype"]);
                    }
                    model.MyCardModel = mycard;
                    list.Add(model);
                }
            }

            return list;
        }

        public string CommondFavoriteMsg(int aid, int othercardid, int userid, int actiontype, int datatype, ref int curState)
        {
            int count = 1;
            
            PlatUserFavoriteMsg platUserFavoriteMsgModel = GetUserFavoriteMsg(aid, othercardid, userid, actiontype, datatype);
            if (platUserFavoriteMsgModel == null)
            {
                platUserFavoriteMsgModel = new PlatUserFavoriteMsg();
                platUserFavoriteMsgModel.ActionType = actiontype;
                platUserFavoriteMsgModel.AddTime = DateTime.Now;
                platUserFavoriteMsgModel.AId = aid;
                platUserFavoriteMsgModel.Datatype = datatype;
                platUserFavoriteMsgModel.MsgId = othercardid;
                platUserFavoriteMsgModel.State = 0;
                platUserFavoriteMsgModel.UserId = userid;
            }
            else if (platUserFavoriteMsgModel.State == -1)
            {
                platUserFavoriteMsgModel.State = 0;
            }
            else if (platUserFavoriteMsgModel.State == 0)//取消
            {
                platUserFavoriteMsgModel.State = -1;
                count = -1;
            }
            curState = platUserFavoriteMsgModel.State;
            PlatMsgviewFavoriteShare platmsgviewModel = PlatMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, othercardid, datatype);
            if (platmsgviewModel == null)
            {
                platmsgviewModel = new PlatMsgviewFavoriteShare();
                platmsgviewModel.AddTime = DateTime.Now;
                platmsgviewModel.AId = aid;
                platmsgviewModel.DataType = datatype;
                platmsgviewModel.DzCount = 0;
                platmsgviewModel.FavoriteCount = 0;
                platmsgviewModel.FollowCount = 0;
                platmsgviewModel.MsgId = othercardid;
                platmsgviewModel.ShareCount = 0;
                platmsgviewModel.SiXinCount = 0;
                platmsgviewModel.ViewCount = 0;
                platmsgviewModel.Id = Convert.ToInt32(PlatMsgviewFavoriteShareBLL.SingleModel.Add(platmsgviewModel));
            }

            switch (actiontype)
            {
                case (int)PointsActionType.关注: platmsgviewModel.FollowCount += count; break;
                case (int)PointsActionType.收藏: platmsgviewModel.FavoriteCount += count; break;
                case (int)PointsActionType.点赞: platmsgviewModel.DzCount += count; break;
                case (int)PointsActionType.看过: platmsgviewModel.ViewCount += 1; break;
                case (int)PointsActionType.私信: platmsgviewModel.SiXinCount += 1; break;
                case (int)PointsActionType.转发: platmsgviewModel.ShareCount += 1; break;
            }

            PlatMsgviewFavoriteShareBLL.SingleModel.Update(platmsgviewModel);
            if (platUserFavoriteMsgModel.Id > 0)
            {
                if (actiontype != (int)PointsActionType.转发 && actiontype != (int)PointsActionType.看过 && actiontype != (int)PointsActionType.私信)
                {
                    platUserFavoriteMsgModel.AddTime = DateTime.Now;
                    base.Update(platUserFavoriteMsgModel, "State,AddTime");
                }
            }
            else
            {
                platUserFavoriteMsgModel.Id = Convert.ToInt32(base.Add(platUserFavoriteMsgModel));
                if (platUserFavoriteMsgModel.Id <= 0)
                {
                    return "操作失效";
                }
            }

            if (datatype == (int)PointsDataType.名片 && platUserFavoriteMsgModel.State >= 0)
            {
                PlatMyCardBLL.SingleModel.RefleshCach(othercardid, actiontype, true);
            }

            if (datatype == (int)PointsDataType.名片 || datatype == (int)PointsDataType.帖子)
            {
                //添加活动轨迹
                PlatActivityTrajectoryBLL.SingleModel.AddData(aid, userid, othercardid, actiontype, datatype);
            }

            return "";
        }


        /// <summary>
        /// 获取用户收藏帖子列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<PlatMsg> GetListMyFavoriteMsg(int aid, int myCardId, out int totalCount, int pageSize = 10, int pageIndex = 1)
        {
            
            string strWhere = $"aid={aid} and userId={myCardId} and state=0 and actionType={(int)PointsActionType.收藏} and Datatype={(int)PointsDataType.帖子}";
            totalCount = base.GetCount(strWhere);
            List<PlatMsg> listPlatMsg = new List<PlatMsg>();
            List<PlatUserFavoriteMsg> listPlatUserFavoriteMsg = base.GetList(strWhere, pageSize, pageIndex, "*", "addTime desc");
            if (listPlatUserFavoriteMsg != null && listPlatUserFavoriteMsg.Count > 0)
            {
                List<string> listMsgId = new List<string>();
                foreach (var item in listPlatUserFavoriteMsg)
                {
                    listMsgId.Add(item.MsgId.ToString());//获取到需要获取帖子的Id
                }
                if (listMsgId != null && listMsgId.Count > 0)
                {
                    listPlatMsg = PlatMsgBLL.SingleModel.GetListByIds(aid, string.Join(",", listMsgId.ToArray()));
                    if (listPlatMsg != null && listPlatMsg.Count > 0)
                    {
                        string cardIds = string.Join(",",listPlatMsg.Select(s=>s.MyCardId).Distinct());
                        List<PlatMyCard> platMyCardList = PlatMyCardBLL.SingleModel.GetListByIds(cardIds);

                        string msgTypeIds = string.Join(",",listPlatMsg.Select(s=>s.MsgTypeId).Distinct());
                        List<PlatMsgType> platMsgTypeList = PlatMsgTypeBLL.SingleModel.GetListByIds(msgTypeIds);

                        listPlatMsg.ForEach(x =>
                        {
                            //获取用户头像
                            PlatMyCard platMyCard = platMyCardList?.FirstOrDefault(f=>f.Id == x.MyCardId);
                            if (platMyCard != null)
                            {
                                x.UserName = platMyCard.Name;
                                x.UserHeaderImg = platMyCard.ImgUrl;
                            }

                            //获取帖子类别
                            PlatMsgType platMsgType = platMsgTypeList?.FirstOrDefault(f=>f.Id == x.MsgTypeId);
                            if (platMsgType != null)
                            {
                                x.MsgTypeName = platMsgType.Name;
                            }

                            //根据帖子ID获取其浏览量-收藏量-分享量数据
                            PlatMsgviewFavoriteShare _platMsgviewFavoriteShare = PlatMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(x.Aid, x.Id, (int)PointsDataType.帖子);
                            if (_platMsgviewFavoriteShare != null)
                            {
                                x.ViewCount = _platMsgviewFavoriteShare.ViewCount;
                                x.FavoriteCount = _platMsgviewFavoriteShare.FavoriteCount;
                                x.ShareCount = _platMsgviewFavoriteShare.ShareCount;
                                x.DzCount = _platMsgviewFavoriteShare.DzCount;
                            }

                            x.ShowTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.AddTime);
                            x.FavoriteId = listPlatUserFavoriteMsg.FirstOrDefault(y => y.MsgId == x.Id).Id;//收藏记录ID
                        });
                    }
                }

            }

            return listPlatMsg;
        }


        /// <summary>
        /// 获取用户收藏店铺列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<PlatStore> GetListMyFavoriteStore(int aid, int myCardId, out int totalCount, int pageSize = 10, int pageIndex = 1)
        {

            string strWhere = $"aid={aid} and userId={myCardId} and state=0 and actionType={(int)PointsActionType.收藏} and Datatype={(int)PointsDataType.店铺}";
            totalCount = base.GetCount(strWhere);
            List<PlatStore> listPlatStore = new List<PlatStore>();
            //    log4net.LogHelper.WriteInfo(this.GetType(), $"{pageSize},{pageIndex},{strWhere}");
            List<PlatUserFavoriteMsg> listPlatUserFavoriteMsg = base.GetList(strWhere, pageSize, pageIndex, "*", "addTime desc");
            if (listPlatUserFavoriteMsg != null && listPlatUserFavoriteMsg.Count > 0)
            {
                //  log4net.LogHelper.WriteInfo(this.GetType(), $"进来了{listPlatUserFavoriteMsg.Count}:"+strWhere);
                List<string> listPlatStoreId = new List<string>();
                foreach (var item in listPlatUserFavoriteMsg)
                {
                    listPlatStoreId.Add(item.MsgId.ToString());//获取到需要获取店铺的Id
                }
                if (listPlatStoreId != null && listPlatStoreId.Count > 0)
                {
                    listPlatStore = PlatStoreBLL.SingleModel.GetList($"bindplataid={aid} and Id in({string.Join(",", listPlatStoreId.ToArray())})");
                }

            }

            return listPlatStore;
        }

        /// <summary>
        /// 获取店铺收藏数量
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public int GetStoreFavoriteCount(int aid, int storeId)
        {
            string strWhere = $"aid={aid} and MsgId={storeId} and state=0 and actionType={(int)PointsActionType.收藏} and Datatype={(int)PointsDataType.店铺}";
            return base.GetCount(strWhere);
        }




        /// <summary>
        /// 删除用户收藏的记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public bool DelMolde(int id, int userId, int appId)
        {
            PlatUserFavoriteMsg model = base.GetModel($"Id={id} and userId={userId} and aid={appId} and actionType={(int)PointsActionType.收藏}");
            if (model == null)
                return false;
            //  log4net.LogHelper.WriteInfo(this.GetType(),"删除进来了"+ $"Id={id} and userId={userId} and aid={appId} and actionType=0");
            model.State = -1;
            return base.Update(model, "State");
        }

    }
}
