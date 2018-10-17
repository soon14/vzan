using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BLL.MiniApp.Plat
{
    public class PlatMsgBLL : BaseMySql<PlatMsg>
    {

        #region 单例模式
        private static PlatMsgBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatMsgBLL()
        {

        }

        public static PlatMsgBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatMsgBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion




        public int GetCountByAId(int aid)
        {
            return base.GetCount($"aid={aid} and state<>-1 and PayState=1");
        }

        /// <summary>
        /// 获取帖子列表 后台数据用
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="isTop"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="msgTypeName"></param>
        /// <param name="userName"></param>
        /// <param name="userPhone"></param>
        /// <param name="orderWhere"></param>
        /// <param name="Review">-2表示全部</param>
        /// <returns></returns>
        public List<PlatMsg> GetListByaid(int aid, out int totalCount, int isTop = 0, int pageSize = 10, int pageIndex = 1, string msgTypeName = "", string userName = "", string userPhone = "", string orderWhere = "addTime desc", int Review = -2,int isFromStore=-1)
        {
            string strWhere = $"aid={aid} and state<>-1 and PayState=1";

            if (isTop == 0)
            {
                //表示普通信息 非置顶信息
                strWhere += " and topDay=0 and isTop=0";
            }
            if (isTop == 1)
            {
                strWhere += $" and (topDay>0 or isTop=1) ";//置顶信息 
            }


            if (Review != -2)
            {
                strWhere += $" and Review={Review} ";
            }



            #region 根据类别名称查询类别
            List<PlatMsgType> listMsgType = new List<PlatMsgType>();

            if (!string.IsNullOrEmpty(msgTypeName))
            {
                //类别
                int count = 0;
                string typeIds = string.Empty;
                List<int> listIds = new List<int>();
                listMsgType = PlatMsgTypeBLL.SingleModel.getListByaid(aid, out count, 1000, 1, msgTypeName);
                if (listMsgType != null && listMsgType.Count > 0)
                {
                    listIds.AddRange(listMsgType.Select(x => x.Id));

                }
                else
                {
                    listIds.Add(0);
                }
                typeIds = string.Join(",", listIds);
                if (!string.IsNullOrEmpty(typeIds))
                {
                    strWhere += $" and msgTypeId in ({typeIds})";
                }
            }
            #endregion

            #region 根据用户昵称模糊匹配用户列表
            List<PlatMyCard> listPlatMyCard = new List<PlatMyCard>();
            if (!string.IsNullOrEmpty(userName))
            {
                string platMyCardIds = string.Empty;
                List<int> listIds = new List<int>();
                int platMyCardCount = 0;
                listPlatMyCard = PlatMyCardBLL.SingleModel.GetDataList(aid, ref platMyCardCount, userName);
                if (listPlatMyCard != null && listPlatMyCard.Count > 0)
                {
                    //  log4net.LogHelper.WriteInfo(this.GetType(), $"listUserInfo={listUserInfo.Count}");
                    listIds.AddRange(listPlatMyCard.Select(x => x.Id));

                }
                else
                {
                    listIds.Add(0);
                }

                platMyCardIds = string.Join(",", listIds);
                if (!string.IsNullOrEmpty(platMyCardIds))
                {
                    strWhere += $" and mycardid in ({platMyCardIds})";
                }
            }
            #endregion

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            if (!string.IsNullOrEmpty(userPhone))
            {
                parameters.Add(new MySqlParameter("@userPhone", $"%{userPhone}%"));
                strWhere += " and phone like @userPhone";
            }

            totalCount = base.GetCount(strWhere, parameters.ToArray());
            // log4net.LogHelper.WriteInfo(this.GetType(), strWhere);
            List<PlatMsg> listPlatMsg = base.GetListByParam(strWhere, parameters.ToArray(), pageSize, pageIndex, "*", orderWhere);
            if (listPlatMsg != null && listPlatMsg.Count > 0)
            {
                listPlatMsg.ForEach(x =>
                {
                    if (listPlatMyCard != null && listPlatMyCard.Count > 0)
                    {
                        x.UserName = listPlatMyCard.FirstOrDefault(u => u.Id == x.MyCardId).Name;
                        x.UserHeaderImg = listPlatMyCard.FirstOrDefault(u => u.Id == x.MyCardId).ImgUrl;
                    }
                    else
                    {
                        PlatMyCard platMyCard = PlatMyCardBLL.SingleModel.GetModel(x.MyCardId);
                        if (platMyCard != null)
                        {
                            x.UserName = platMyCard.Name;
                            x.UserHeaderImg = platMyCard.ImgUrl;
                        }
                    }
                    
                    if (listMsgType != null && listMsgType.Count > 0)
                    {
                        x.MsgTypeName = listMsgType.FirstOrDefault(t => t.Id == x.MsgTypeId).Name;
                    }
                    else
                    {
                        PlatMsgType platMsgType = PlatMsgTypeBLL.SingleModel.GetModel(x.MsgTypeId);
                        if (platMsgType != null)
                        {
                            x.MsgTypeName = platMsgType.Name;
                        }
                    }
                    x.MsgDetail = HttpUtility.HtmlDecode(x.MsgDetail);

                    if (isFromStore != -1)
                    {
                        x.MsgDetail = x.MsgDetail.Length > 10 ? x.MsgDetail.Substring(0, 10)+"..." : x.MsgDetail;
                    }
                });
            }

            return listPlatMsg;
        }

        public List<PlatMsg> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PlatMsg>();

            string strWhere = $"Id in({ids})";
            return base.GetList(strWhere);
        }
        
        /// <summary>
        /// 根据id集合获取platMsg列表数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<PlatMsg> GetListByIds(int aid, string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PlatMsg>();

            string strWhere = $"aid={aid} and Id in({ids})";
            return base.GetList(strWhere);
        }

        /// <summary>
        /// 获取指定帖子详情 包含了 点赞数 分享数 收藏数 浏览量等...
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public PlatMsg GetMsg(int aid, int msgId)
        {
            PlatMsg x = base.GetModel($"aid={aid} and Id={msgId}");
            if (x != null)
            {
                //获取用户头像
                PlatMyCard platMyCard = PlatMyCardBLL.SingleModel.GetModel(x.MyCardId);
                if (platMyCard != null)
                {
                    x.UserName = platMyCard.Name;
                    x.UserHeaderImg = platMyCard.ImgUrl;
                    x.UserId = platMyCard.UserId;
                }

                //获取帖子类别
                PlatMsgType platMsgType = PlatMsgTypeBLL.SingleModel.GetModel(x.MsgTypeId);
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
                x.MsgDetail = HttpUtility.HtmlDecode(x.MsgDetail);
                x.ShowTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.AddTime);
            }


            return x;
        }



        /// <summary>
        /// 判断类别下是否关联了消息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="msgTypeId"></param>
        /// <returns></returns>
        public bool MsgTypeHaveMsg(int aid, int msgTypeId)
        {

            string strWhere = $"aid={aid} and msgTypeId={msgTypeId} and state=1";

            return base.Exists(strWhere);
        }


        /// <summary>
        /// 发帖
        /// </summary>
        /// <param name="jsondata"></param>
        /// <param name="ordertype"></param>
        /// <param name="paytype"></param>
        /// <param name="aid"></param>
        /// <param name="order"></param>
        /// <param name="MyCardId"></param>
        /// <returns></returns>
        public string saveMsg(string jsondata, int ordertype, int paytype, int aid, ref CityMorders order, int userId)
        {
            if (string.IsNullOrEmpty(jsondata))
                return "json参数错误";

            SavePlatMsgModel savePlatMsgModelJson = JsonConvert.DeserializeObject<SavePlatMsgModel>(jsondata);
            if (savePlatMsgModelJson == null)
                return "null参数错误";

            double lng = 0.00;
            double lat = 0.00;

            //校验手机号码  
            if (!Regex.IsMatch(savePlatMsgModelJson.Phone, @"\d")|| savePlatMsgModelJson.Phone.Length>11)
                return "联系号码不符合";

            if (string.IsNullOrEmpty(savePlatMsgModelJson.Location))
                return "请填写地址";

            if (string.IsNullOrEmpty(savePlatMsgModelJson.MsgDetail))
                return "发布信息不能为空";

            if (!double.TryParse(savePlatMsgModelJson.Lng, out lng) || !double.TryParse(savePlatMsgModelJson.Lat, out lat))
                return "地址坐标错误";

            if (savePlatMsgModelJson.MsgType <= 0)
                return "请选择信息类别";

            PlatMyCard platMyCard = PlatMyCardBLL.SingleModel.GetModelByUserId(userId, aid);
            if (platMyCard == null)
                return "请先注册";



            PlatMsg platMsg = new PlatMsg();
            platMsg.AddTime = DateTime.Now;
            platMsg.Aid = aid;
            platMsg.Lat = savePlatMsgModelJson.Lat;
            platMsg.Lng = savePlatMsgModelJson.Lng;
            platMsg.Location = savePlatMsgModelJson.Location;
            platMsg.MsgDetail = HttpUtility.HtmlEncode(savePlatMsgModelJson.MsgDetail);
            platMsg.MsgTypeId = savePlatMsgModelJson.MsgType;
            platMsg.Phone = savePlatMsgModelJson.Phone;
            platMsg.MyCardId = platMyCard.Id;
            platMsg.Imgs = savePlatMsgModelJson.Imgs;

            PlatMsgConf platMsgConf = PlatMsgConfBLL.SingleModel.GetMsgConf(aid);
            if (platMsgConf == null)
            {
                return "商家配置异常";
            }

            switch (platMsgConf.ReviewSetting)
            {
                case 0://不需要审核
                    if (savePlatMsgModelJson.IsTop == 0)
                    {
                        //不置顶消息
                        platMsg.State = 1;
                        platMsg.Review = 0;
                        platMsg.PayState = 1;
                        platMsg.ReviewTime = DateTime.Now;
                    }
                    else
                    {
                        platMsg.State = 0;//支付回成功调后再变更1
                        platMsg.PayState = 0; //支付回成功调后再变更1
                        platMsg.ReviewTime = DateTime.Now;
                        platMsg.Review = 0;

                    }
                    break;

                case 1://先审核后发布
                    platMsg.State = 0;//审核通过后变为1
                    platMsg.Review = 1;//审核通过后变为2  审核不通过变为-1
                    if (savePlatMsgModelJson.IsTop == 0)
                    {
                        //不置顶消息
                        platMsg.PayState = 1;
                    }
                    else
                    {
                        platMsg.PayState = 0;//支付回成功调后再变更为1
                    }

                    break;
                case 2://先发布后审核
                    platMsg.ReviewTime = DateTime.Now;
                    if (savePlatMsgModelJson.IsTop == 0)
                    {
                        //不置顶消息
                        platMsg.State = 1;
                        platMsg.PayState = 1;
                        platMsg.Review = 1;//审核通过后变为2  审核不通过变为-1

                    }
                    else
                    {
                        platMsg.State = 0;//支付回成功调后再变更为1
                        platMsg.PayState = 0;//支付回成功调后再变更为1
                        platMsg.Review = 1;//审核通过后变为2  审核不通过变为-1

                    }
                    break;



            }

            if (savePlatMsgModelJson.IsTop == 0)
            {
                //表示不置顶的消息
                platMsg.TopDay = 0;
                platMsg.IsTop = 0;
                platMsg.TopCostPrice = 0;

                int msgId = Convert.ToInt32(base.Add(platMsg));
                if (msgId <= 0)
                {
                    return "发布信息异常";

                }
                else
                {
                    return string.Empty;
                }

            }
            else
            {
                //表示置顶消息 需要 生成微信订单，然后微信支付，支付成功后回调将状态变为1可用
                if (savePlatMsgModelJson.RuleId <= 0)
                {
                    return "请选择置顶时间";
                }

                PlatMsgRule platMsgRule = PlatMsgRuleBLL.SingleModel.GetMsgRules(aid, savePlatMsgModelJson.RuleId);
                if (platMsgRule == null)
                {
                    return "非法操作(置顶时间有误)";
                }

                platMsg.TopDay = platMsgRule.ExptimeDay;
                platMsg.TopCostPrice = platMsgRule.Price;
                platMsg.IsTop = 1;
                int msgId = Convert.ToInt32(base.Add(platMsg));
                if (msgId <= 0)
                {
                    return "发布信息异常";
                }
                else
                {
                    order.Articleid = platMsgRule.Id;
                    order.CommentId = msgId;
                    order.MinisnsId = aid;
                    order.payment_free = platMsg.TopCostPrice;
                    order.ShowNote = $"平台版小程序分类信息发置顶帖付款{order.payment_free * 0.01}元";
                    return string.Empty;
                }


            }




        }


        /// <summary>
        /// 根据用户Id也就是myCardId 获取该用户发布的帖子列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="myCardId"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<PlatMsg> GetListByUserId(int aid, int myCardId, out int totalCount, int pageSize = 10, int pageIndex = 1)
        {
            string strWhere = $"aid={aid} and state<>-1 and PayState=1 and MyCardId={myCardId}";
            List<PlatMsg> listPlatMsg = base.GetList(strWhere, pageSize, pageIndex, "*", " addTime desc");
            int commentTotalCount = 0;
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
                    x.Comments = PlatMsgCommentBLL.SingleModel.GetPlatMsgComment(x.Aid, out commentTotalCount, 0, string.Empty, 1000, 1, x.Id, 0);
                    x.ShowTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.AddTime);
                    x.MsgDetail = HttpUtility.HtmlDecode(x.MsgDetail);

                });
            }

            totalCount = base.GetCount(strWhere);

            return listPlatMsg;
        }


        /// <summary>
        /// 获取帖子列表 排列顺序为 置顶未失效>时间降序 
        /// 然后再到前端进行排序 如果时间按照最近则不需要排序
        /// 如果是按距离则再排一次先是置顶的然后 距离有限 再按时间降序
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>

        public List<PlatMsg> GetListMsg(int aid, out int totalCount, string keyMsg = "", int msgTypeId = 0, int pageSize = 10, int pageIndex = 1,int orderType=0, double ws_lat = 0, double ws_lng = 0)
        {
            string strWhere = $" aid={aid} and state=1 and PayState=1 ";
            if (msgTypeId > 0)
            {
                strWhere += $"  and MsgTypeId={msgTypeId} ";
            }

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            if (!string.IsNullOrEmpty(keyMsg))
            {
                strWhere += $" and msgDetail like @keyMsg ";
                parameters.Add(new MySqlParameter("keyMsg", $"%{keyMsg}%"));
            }

            totalCount = base.GetCount(strWhere, parameters.ToArray());

            string sql = $@"SELECT * FROM( SELECT * FROM (SELECT *,(DATE_ADD(ReviewTime,INTERVAL topDay DAY)-now()) as tspan from platmsg where (DATE_ADD(ReviewTime,INTERVAL topDay DAY)-now())>0 and { strWhere} ORDER BY addtime desc LIMIT 100000)t1 
UNION
 SELECT* FROM(
SELECT*, (DATE_ADD(ReviewTime, INTERVAL topDay DAY)-now()) as tspan from platmsg where (DATE_ADD(ReviewTime, INTERVAL topDay DAY) - now()) <= 0 and { strWhere}  ORDER BY addtime desc LIMIT 100000) t2
) t3 limit {(pageIndex - 1) * pageSize},{pageSize}";
            if (orderType > 0)
            {//表示附近
                sql = $@"SELECT * FROM( SELECT * FROM (SELECT *,(DATE_ADD(ReviewTime,INTERVAL topDay DAY)-now()) as tspan,ROUND(6378.138*2*ASIN(SQRT(POW(SIN(({ws_lat}*PI()/180-lat*PI()/180)/2),2)+COS({ws_lat}*PI()/180)*COS(lat*PI()/180)*POW(SIN(({ws_lng}*PI()/180-lng*PI()/180)/2),2)))*1000) AS distance from platmsg where (DATE_ADD(ReviewTime,INTERVAL topDay DAY)-now())>0 and { strWhere} ORDER BY distance asc,addtime desc LIMIT 100000)t1 
UNION
 SELECT* FROM(
SELECT*, (DATE_ADD(ReviewTime, INTERVAL topDay DAY)-now()) as tspan,ROUND(6378.138*2*ASIN(SQRT(POW(SIN(({ws_lat}*PI()/180-lat*PI()/180)/2),2)+COS({ws_lat}*PI()/180)*COS(lat*PI()/180)*POW(SIN(({ws_lng}*PI()/180-lng*PI()/180)/2),2)))*1000) AS distance from platmsg where (DATE_ADD(ReviewTime, INTERVAL topDay DAY) - now()) <= 0 and { strWhere}  ORDER BY distance asc, addtime desc LIMIT 100000) t2
) t3 limit {(pageIndex - 1) * pageSize},{pageSize}";
            }

         //   log4net.LogHelper.WriteInfo(this.GetType(),sql);
            List<PlatMsg> listPlatMsg = base.GetListBySql(sql, parameters.ToArray());
            if (listPlatMsg != null && listPlatMsg.Count > 0)
            {
                string cardIds = string.Join(",",listPlatMsg.Select(s=>s.MyCardId).Distinct());
                List<PlatMyCard> platMyCardList = PlatMyCardBLL.SingleModel.GetListByIds(cardIds);

                string msgTypeIds = string.Join(",", listPlatMsg.Select(s => s.MsgTypeId).Distinct());
                List<PlatMsgType> platMsgTypeList = PlatMsgTypeBLL.SingleModel.GetListByIds(msgTypeIds);

                listPlatMsg.ForEach(x =>
                {
                    //获取用户头像
                    PlatMyCard platMyCard = platMyCardList?.FirstOrDefault(f=>f.Id == x.MyCardId);
                    if (platMyCard != null)
                    {
                        x.UserName = platMyCard.Name;
                        x.UserHeaderImg = platMyCard.ImgUrl;
                        x.UserId = platMyCard.UserId;
                    }

                    //获取帖子类别
                    PlatMsgType platMsgType = platMsgTypeList?.FirstOrDefault(f => f.Id == x.MsgTypeId);
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
                    x.MsgDetail = HttpUtility.HtmlDecode(x.MsgDetail);

                });
            }
            
            totalCount = base.GetCount(strWhere, parameters.ToArray());
            return listPlatMsg;
        }


        /// <summary>
        /// 小程序端用户删除发布的帖子
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public bool DelMsg(int id, int userId, int appId)
        {
            PlatMsg model = base.GetModel($"Id={id} and MyCardId={userId} and aid={appId}");
            if (model == null)
                return false;
            model.State = -1;
            return base.Update(model, "State");
        }

    }
}
