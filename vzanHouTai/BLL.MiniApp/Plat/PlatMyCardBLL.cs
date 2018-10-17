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
using Utility;

namespace BLL.MiniApp.Plat
{
    public class PlatMyCardBLL : BaseMySql<PlatMyCard>
    {
        #region 单例模式
        private static PlatMyCardBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatMyCardBLL()
        {

        }

        public static PlatMyCardBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatMyCardBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static readonly string _redis_mycardhavenewdatakey = "mycardhavenewdata_{0}";
        public static readonly string _redis_mycardhavenewdatav2key = "mycardhavenewdatav2_{0}";
        public static readonly string _redis_havenewdatakey = "havenewdata_{0}_{1}";
        public static readonly string _redis_RenMaiQuanKey = "renmaiquan_{0}_{1}";

        #region 缓存操作

        /// <summary>
        /// 附近人脉
        /// </summary>
        /// <param name="lng">经度</param>
        /// <param name="lat">纬度</param>
        /// <param name="citycode">同城编码</param>
        /// <param name="distance">距离</param>
        /// <param name="userid">登陆用户ID</param>
        /// <param name="aid">权限表ID</param>
        /// <param name="actiontype"></param>
        /// <param name="count"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<PlatMyCard> GetCardApiList(string appid, int industryid, int areacode, string lng, string lat, int citycode, int distance, int userid, int aid, int actiontype, ref int count, int pageSize = 5, int pageIndex = 1)
        {
            string key = string.Format(_redis_RenMaiQuanKey, aid, userid);
            List<PlatMyCard> list = new List<PlatMyCard>();
            RedisModel<PlatMyCard> redisList = RedisUtil.Get<RedisModel<PlatMyCard>>(key);
            if (pageIndex <= 1 || redisList == null || redisList.DataList == null || redisList.DataList.Count <= 0)
            {
                redisList = new RedisModel<PlatMyCard>();
                string sql = $"select {"{0}"} from (select p.*,f.followcount,f.dzcount,(f.viewcount+p.fictitiouscount) viewcount,f.SiXinCount,f.FavoriteCount,GetDistance({lng},{lat},p.lng,p.lat) distance from platmycard p left join platmsgviewfavoriteshare f on p.id = f.msgid and f.aid={aid} and f.datatype={(int)PointsDataType.名片}) d";
                string sqlcount = string.Format(sql, "count(*)");
                string sqllist = string.Format(sql, "*");
                string sqlwhere = $" where d.aid = {aid} and d.appid='{appid}' and d.state>=0";
                string sqlorderby = " order by p.id desc";
                //string sqlpage = $" limit {(pageIndex - 1) * pageSize},{pageSize}";

                //行业
                if (industryid > 0)
                {
                    sqlwhere += $" and IndustryId={industryid} ";
                }
                //区域
                if (areacode > 0)
                {
                    sqlwhere += $" and (citycode={areacode} or ProvinceCode={areacode}) ";
                }

                //人气排序
                switch (actiontype)
                {
                    case (int)PointsActionType.关注: sqlorderby = " order by followcount desc "; break;
                    case (int)PointsActionType.点赞: sqlorderby = " order by dzcount desc "; break;
                    case (int)PointsActionType.看过: sqlorderby = " order by viewcount desc "; break;
                }

                //附近人脉 
                if (distance > 0)
                {
                    sqlwhere += $" and distance<={distance * 1000}";
                    sqlorderby = " order by distance asc ";
                }
                else if (citycode > 0)
                {
                    sqlwhere += $" and citycode={citycode} ";
                    sqlorderby = " order by distance asc ";
                }
                //log4net.LogHelper.WriteInfo(this.GetType(), sqllist + sqlwhere + sqlorderby + sqlpage);
                count = base.GetCountBySql(sqlcount + sqlwhere);
                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqllist + sqlwhere + sqlorderby, null))
                {
                    while (dr.Read())
                    {
                        PlatMyCard model = base.GetModel(dr);
                        if (dr["followcount"] != DBNull.Value)
                        {
                            model.FollowCount = Convert.ToInt32(dr["followcount"]);
                        }
                        if (dr["dzcount"] != DBNull.Value)
                        {
                            model.DzCount = Convert.ToInt32(dr["dzcount"]);
                        }
                        if (dr["viewcount"] != DBNull.Value)
                        {
                            model.ViewCount = Convert.ToInt32(dr["viewcount"]);
                        }
                        if (dr["SiXinCount"] != DBNull.Value)
                        {
                            model.SiXinCount = Convert.ToInt32(dr["SiXinCount"]);
                        }
                        if (dr["FavoriteCount"] != DBNull.Value)
                        {
                            model.FavoriteCount = Convert.ToInt32(dr["FavoriteCount"]);
                        }
                        //PlatMyCard modelitem = GetMyCardData(model.UserId,userid,aid,(int)PointsDataType.名片);
                        if (DBNull.Value != dr["distance"])
                        {
                            int tempdistance = Convert.ToInt32(dr["distance"]);
                            model.Distance = DataHelper.GetDistanceFormat(tempdistance, 1);
                        }
                        else
                        {
                            model.Distance = "0m";
                        }
                        list.Add(model);
                    }
                }
                if (list != null && list.Count > 0)
                {
                    string msgids = string.Join(",", list.Select(s => s.Id).Distinct());
                    List<PlatUserFavoriteMsg> userfavoritelist = PlatUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsgList(aid, msgids, userid, (int)PointsDataType.名片);
                    //入驻店铺
                    List<PlatStore> storelist = PlatStoreBLL.SingleModel.GetListByCardId(msgids);

                    foreach (PlatMyCard item in list)
                    {
                        PlatUserFavoriteMsg userfavorite = userfavoritelist?.FirstOrDefault(f => f.MsgId == item.Id && f.ActionType == (int)PointsActionType.关注);
                        item.IsFavorite = userfavorite?.State >= 0;

                        userfavorite = userfavoritelist?.FirstOrDefault(f => f.MsgId == item.Id && f.ActionType == (int)PointsActionType.关注);
                        item.IsFollow = userfavorite?.State >= 0;

                        userfavorite = userfavoritelist?.FirstOrDefault(f => f.MsgId == item.Id && f.ActionType == (int)PointsActionType.点赞);
                        item.IsDz = userfavorite?.State >= 0;

                        //是否有入驻店铺
                        PlatStore store = storelist?.FirstOrDefault(f => f.MyCardId == item.Id);
                        item.StoreId = store == null ? 0 : store.Id;
                    }
                }
                redisList.DataList = list;
                redisList.Count = count;
                RedisUtil.Set<RedisModel<PlatMyCard>>(key, redisList);
            }
            list = redisList.DataList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            count = redisList.Count;
            return list;
        }
        public void HaveSixin(int tuserId, int fuserId, bool havedata = false)
        {
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(tuserId);
            if (userinfo == null)
                return;

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(userinfo.appId);
            if (xcxrelation == null)
                return;

            //int tmptype = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(xcxrelation.Id);

            switch (xcxrelation.Type)
            {
                case (int)TmpType.小未平台:
                    PlatMyCard card = new PlatMyCardBLL().GetModelByUserId(tuserId);
                    if (card == null)
                        return;
                    RefleshCach(card.Id, (int)PointsActionType.私信, true);
                    break;
                case (int)TmpType.企业智推版:
                    RedisUtil.Set<bool>(string.Format(_redis_havenewdatakey, tuserId, fuserId), true);
                    break;
            }
        }


        /// <summary>
        /// 标识有新消息提示
        /// </summary>
        /// <param name="mycardid"></param>
        /// <param name="actiontype"></param>
        /// <param name="havedata"></param>
        public void RefleshCach(int mycardid, int actiontype, bool havedata = false)
        {
            HaveNewData data = RedisUtil.Get<HaveNewData>(string.Format(_redis_mycardhavenewdatakey, mycardid));
            if (data == null)
            {
                data = new HaveNewData();
            }
            switch (actiontype)
            {
                case (int)PointsActionType.关注: data.FollowData = havedata; break;
                case (int)PointsActionType.点赞: data.DzData = havedata; break;
                case (int)PointsActionType.看过: data.ViewData = havedata; break;
                case (int)PointsActionType.私信: data.SiXinData = havedata; break;
            }
            RedisUtil.Set<HaveNewData>(string.Format(_redis_mycardhavenewdatakey, mycardid, actiontype), data);
        }

        /// <summary>
        /// 标识新消息个数
        /// </summary>
        /// <param name="mycardid"></param>
        /// <param name="actiontype"></param>
        /// <param name="havedata"></param>
        public void RefleshCachV2(int mycardid, int actiontype, int havedata = 0)
        {
            HaveNewDataV2 data = RedisUtil.Get<HaveNewDataV2>(string.Format(_redis_mycardhavenewdatav2key, mycardid));
            if (data == null)
            {
                data = new HaveNewDataV2();
            }
            if (havedata == 0)
            {
                switch (actiontype)
                {
                    case (int)PointsActionType.关注: data.FollowData = havedata; break;
                    case (int)PointsActionType.点赞: data.DzData = havedata; break;
                    case (int)PointsActionType.看过: data.ViewData = havedata; break;
                    case (int)PointsActionType.私信: data.SiXinData = havedata; break;
                }
            }
            else if (havedata > 0)
            {
                switch (actiontype)
                {
                    case (int)PointsActionType.关注: data.FollowData += havedata; break;
                    case (int)PointsActionType.点赞: data.DzData += havedata; break;
                    case (int)PointsActionType.看过: data.ViewData += havedata; break;
                    case (int)PointsActionType.私信: data.SiXinData += havedata; break;
                }
            }

            RedisUtil.Set<HaveNewDataV2>(string.Format(_redis_mycardhavenewdatav2key, mycardid, actiontype), data);
        }
        
        public HaveNewData GetRedisCach(int mycardid)
        {
            HaveNewData data = RedisUtil.Get<HaveNewData>(string.Format(_redis_mycardhavenewdatakey, mycardid));
            if (data == null)
            {
                data = new HaveNewData();
            }
            return data;
        }

        #endregion

        public int GetCountByAId(int aid,string appid)
        {
            return base.GetCount($"aid={aid} and appid = '{appid}'");
        }

        public List<PlatMyCard> GetDataList(int aid, ref int count,string name="",string nickname="", string phone="",string starttime="",string endtime="",int pageSize=10,int pageIndex=1,string loginid="",int storestate=0)
        {
            List<MySqlParameter> parms = new List<MySqlParameter>();
            string sqlwhere = $"aid={aid} and state>=0";
            if(!string.IsNullOrEmpty(name))
            {
                sqlwhere += $" and name like @name";
                parms.Add(new MySqlParameter("@name",$"%{name}%"));
            }
            if (!string.IsNullOrEmpty(nickname))
            {
                sqlwhere += $" and nickname like @nickname";
                parms.Add(new MySqlParameter("@nickname", $"%{nickname}%"));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                sqlwhere += $" and phone like @phone";
                parms.Add(new MySqlParameter("@phone", $"%{phone}%"));
            }
            if (!string.IsNullOrEmpty(starttime))
            {
                sqlwhere += $" and addtime >='{starttime}'";
            }
            if (!string.IsNullOrEmpty(endtime))
            {
                sqlwhere += $" and addtime <='{endtime} 23:59:59'";
            }
            count = base.GetCount(sqlwhere,parms.ToArray());
            return base.GetListByParam(sqlwhere, parms.ToArray(), pageSize,pageIndex);
        }

        public List<PlatMyCard> GetMyCardDataList(string appid,int aid, ref int count, string name = "", string phone = "", int pageSize = 10, int pageIndex = 1, string loginid = "", int storestate = 0)
        {
            List<PlatMyCard> list = new List<PlatMyCard>();
            List<MySqlParameter> parms = new List<MySqlParameter>();
            
            string sql = $"select mc.*,s.name storename from platmycard mc left join platstore s on mc.id=s.mycardid left join PlatStoreRelation sr on s.id = sr.StoreId";
            string sqlcount = "select count(*) from platmycard mc left join platstore s on mc.id=s.mycardid left join PlatStoreRelation sr on s.id = sr.StoreId";
            string sqlwhere = $" where mc.aid={aid} and mc.appid='{appid}'";
            string sqlpage = $" limit {(pageIndex - 1) * pageSize},{pageSize}";
            if (!string.IsNullOrEmpty(name))
            {
                sqlwhere += $" and mc.name like @name";
                parms.Add(new MySqlParameter("@name", $"%{name}%"));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                sqlwhere += $" and mc.phone like @phone ";
                parms.Add(new MySqlParameter("@phone", $"%{phone}%"));
            }
            if (!string.IsNullOrEmpty(loginid))
            {
                sqlwhere += $" and mc.loginid like @loginid ";
                parms.Add(new MySqlParameter("@loginid", $"%{loginid}%"));
            }
            if(storestate>=0)
            {
                sqlwhere += $" and sr.State ={storestate}";
            }

            count = base.GetCountBySql(sqlcount+sqlwhere, parms.ToArray());
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere  + sqlpage,parms.ToArray()))
            {
                while (dr.Read())
                {
                    PlatMyCard model = base.GetModel(dr);
                    model.StoreName = dr["storename"].ToString();
                    PlatPostUserMsgCountViewMolde msgmodel = PlatPostUserBLL.SingleModel.GetCountViewModel(model.Id);
                    if (msgmodel != null)
                    {
                        model.MsgCount = msgmodel.PostMsgCount;
                    }
                    list.Add(model);
                }
            }
            return list;
        }
        
        public PlatMyCard GetModelByUserId(long userid,int aid=0,int state=0)
        {
            string sqlwhere = $" userid={userid} ";
            if(aid>0)
            {
                sqlwhere += $" and aid={aid} ";
            }
            if(state==0)
            {
                sqlwhere += $" and state>=-1";
            }

            return base.GetModel(sqlwhere);
        }

        /// <summary>
        /// 获取名片，并返回我是否点赞了该名片
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="myuserid"></param>
        /// <param name="aid"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public PlatMyCard GetMyCardData(long userid,int myuserid, int aid, int datatype)
        {
            PlatMyCard model = GetModelByUserId(userid,aid);
            if (model == null)
                return new PlatMyCard();
            PlatMsgviewFavoriteShare smodel = PlatMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, model.Id, datatype);
            if (smodel == null)
                return model;
            List<PlatUserFavoriteMsg> userfavoritelist = PlatUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsgList(aid, model.Id.ToString(), myuserid, (int)PointsDataType.名片);
            if(userfavoritelist!=null && userfavoritelist.Count>0)
            {
                PlatUserFavoriteMsg userfavorite = userfavoritelist.Where(w => w.ActionType == (int)PointsActionType.收藏).FirstOrDefault();
                model.IsFavorite = userfavorite?.State >= 0;
                
                userfavorite = userfavoritelist.Where(w => w.ActionType == (int)PointsActionType.关注).FirstOrDefault();
                model.IsFollow = userfavorite?.State >= 0;
                
                userfavorite = userfavoritelist.Where(w => w.ActionType == (int)PointsActionType.点赞).FirstOrDefault();
                model.IsDz = userfavorite?.State >= 0;
            }
            
            model.FollowCount = smodel.FollowCount;
            model.FavoriteCount = smodel.FavoriteCount;
            model.DzCount = smodel.DzCount;
            model.ViewCount = smodel.ViewCount+model.FictitiousCount;
            model.SiXinCount = smodel.SiXinCount;
            model.NewData = GetRedisCach(model.Id);
            return model;
        }

        public PlatMyCard GetMyCardData(int cardid, int aid)
        {
            PlatMyCard model = base.GetModel(cardid);
            if (model == null)
                return new PlatMyCard();
            

            PlatPostUserMsgCountViewMolde msgmodel = PlatPostUserBLL.SingleModel.GetCountViewModel(model.Id);
            if (msgmodel != null)
            {
                model.MsgCount = msgmodel.PostMsgCount;
            }

            model.ContentCount = PlatMsgCommentBLL.SingleModel.GetUserContentCount(model.UserId);
            model.MsgDzCount = PlatUserFavoriteMsgBLL.SingleModel.GetMyMsgCount(model.UserId,(int)PointsActionType.点赞,(int)PointsDataType.帖子);
            PlatMsgviewFavoriteShare smodel = PlatMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, model.Id, (int)PointsDataType.名片);
            if (smodel != null)
            {
                model.FollowCount = smodel.FollowCount;
                model.FavoriteCount = smodel.FavoriteCount;
                model.DzCount = smodel.DzCount;
                model.ViewCount = smodel.ViewCount;
                model.SiXinCount = smodel.SiXinCount;
                model.NewData = GetRedisCach(model.Id);
            }

            //行业
            PlatIndustry industrymodel = PlatIndustryBLL.SingleModel.GetModel(model.IndustryId);
            model.IndustryName = industrymodel?.Name;

            //店铺
            PlatStore store = PlatStoreBLL.SingleModel.GetModelBycardid(model.AId,model.Id);
            if(store!=null)
            {
                smodel = PlatMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, store.Id, (int)PointsDataType.店铺);
                if (smodel != null)
                {
                    model.StoreFavoriteCount = smodel.FavoriteCount;
                    model.StoreViewCount = store.StorePV+store.StoreVirtualPV;
                    model.StoreVistorCount = PlatUserFavoriteMsgBLL.SingleModel.GetUserMsgCount(model.AId,store.Id,(int)PointsActionType.看过, (int)PointsDataType.店铺);
                }
            }
            
            return model;
        }

        public List<PlatMyCard> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return new List<PlatMyCard>();
            }
            return base.GetList($"id in ({ids})");
        }

        public List<PlatMyCard> GetListByUserIds(string userIds)
        {
            if (string.IsNullOrEmpty(userIds))
            {
                return new List<PlatMyCard>();
            }
            return base.GetList($"userid in ({userIds})");
        }

        /// <summary>
        /// 获取小程序正常的名片Id集合字符串
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public string GetCardIds(int aid,string appId)
        {
            List<int> listCardId = new List<int>();
            List<PlatMyCard> list = base.GetList($"aid={aid} and appId='{appId}' and State<>-1");
            if (list != null)
            {
                listCardId.Add(0);//后台手动添加的店铺没有绑定名片
                foreach (PlatMyCard item in list)
                {
                    if (!listCardId.Contains(item.Id))
                    {
                        listCardId.Add(item.Id);
                    }
                }

                return string.Join(",",listCardId);
            }
            return "-1";
        }
    }
}
