using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Utility;

namespace BLL.MiniApp.cityminiapp
{
    public class CityMsgBLL : BaseMySql<CityMsg>
    {
        #region 单例模式
        private static CityMsgBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityMsgBLL()
        {

        }

        public static CityMsgBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityMsgBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        

        public CityMsg getMsg(int aid, int msgId)
        {
            CityMsg x = base.GetModel($"aid={aid} and Id={msgId}");
            if (x != null)
            {
                //获取用户头像
                C_UserInfo c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(x.userId);
                if (c_UserInfo != null)
                {
                    x.userName = c_UserInfo.NickName;
                    x.userHeaderImg = c_UserInfo.HeadImgUrl;
                }

                //获取帖子类别
                CityStoreMsgType city_StoreMsgType = CityStoreMsgTypeBLL.SingleModel.GetModel(x.msgTypeId);
                if (city_StoreMsgType != null)
                {
                    x.msgTypeName = city_StoreMsgType.name;
                }

                //根据帖子ID获取其浏览量-收藏量-分享量数据
                CityMsgViewFavoriteShare city_MsgViewFavoriteShare = CityMsgViewFavoriteShareBLL.SingleModel.getModelByMsgId(x.Id);
                if (city_MsgViewFavoriteShare != null)
                {
                    x.ViewCount = city_MsgViewFavoriteShare.ViewCount;
                    x.FavoriteCount = city_MsgViewFavoriteShare.FavoriteCount;
                    x.ShareCount = city_MsgViewFavoriteShare.ShareCount;
                    x.DzCount = city_MsgViewFavoriteShare.DzCount;
                }

                x.showTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.addTime);
                x.msgDetail = HttpUtility.HtmlDecode(x.msgDetail);
            }


            return x;
        }

        public List<CityMsg> getListByaid(int aid, out int totalCount, int isTop = 0, int pageSize = 10, int pageIndex = 1, string msgTypeName = "", string userName = "", string userPhone = "", string orderWhere = "addTime desc",string appId="",int Review=-2)
        {
            string strWhere = $"aid={aid} and state<>-1";

            if (isTop == 0)
            {
                //表示普通信息 非置顶信息
                strWhere += " and topDay=0 and IsDoNotTop<>1";
            }
            else if(isTop==1)
            {
                List<string> listMsgId = new List<string>();
                
                List<CityMorders> listCityMorder = new CityMordersBLL().GetList($"MinisnsId={aid} and payment_status=1 and Status=1");
                if(listCityMorder!=null&& listCityMorder.Count > 0)
                {
                    foreach(CityMorders item in listCityMorder)
                    {
                        listMsgId.Add(item.CommentId.ToString());
                    }

                }
                else
                {
                    listMsgId.Add("0");
                }

                strWhere += $" and  (topDay>0 or IsDoNotTop=1) and (Id in({string.Join(",",listMsgId)}) or IsDoTop=1) ";//置顶信息 去CityMorders表找出已经付款成功的帖子信息ID


            }


            if (Review != -2)
            {
                strWhere += $" and Review={Review} ";
            }



            #region 根据类别名称查询类别
            List<CityStoreMsgType> listMsgType = new List<CityStoreMsgType>();

            if (!string.IsNullOrEmpty(msgTypeName))
            {
                //类别
                int count = 0;
                string typeIds = string.Empty;
                List<int> listIds = new List<int>();
                listMsgType = CityStoreMsgTypeBLL.SingleModel.getListByaid(aid, out count, 1000, 1, msgTypeName);
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
            List<C_UserInfo> listUserInfo = new List<C_UserInfo>();
            if (!string.IsNullOrEmpty(userName))
            {
                //用户
                string userIds = string.Empty;
                List<int> listIds = new List<int>();
                listUserInfo = C_UserInfoBLL.SingleModel.GetUserListByNickName(userName,appId);
                if (listUserInfo != null && listUserInfo.Count > 0)
                {
                    listIds.AddRange(listUserInfo.Select(x => x.Id));
                   
                }
                else
                {
                    listIds.Add(0);
                }

                userIds = string.Join(",", listIds);
                if (!string.IsNullOrEmpty(userIds))
                {
                    strWhere += $" and userId in ({userIds})";
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
            List<CityMsg> listCity_Msg = base.GetListByParam(strWhere, parameters.ToArray(), pageSize, pageIndex, "*", orderWhere);
            if (listCity_Msg != null && listCity_Msg.Count > 0)
            {
                listCity_Msg.ForEach(x =>
                {

                    if (listUserInfo != null && listUserInfo.Count > 0)
                    {
                        x.userName = listUserInfo.FirstOrDefault(u => u.Id == x.userId).NickName;
                        x.userHeaderImg = listUserInfo.FirstOrDefault(u => u.Id == x.userId).HeadImgUrl;
                    }
                    else
                    {
                        C_UserInfo c_UserInfo = C_UserInfoBLL.SingleModel.GetModel(x.userId);
                        if (c_UserInfo != null)
                        {
                            x.userName = c_UserInfo.NickName;
                            x.userHeaderImg = c_UserInfo.HeadImgUrl;
                        }
                    }


                    if (listMsgType != null && listMsgType.Count > 0)
                    {
                        x.msgTypeName = listMsgType.FirstOrDefault(t => t.Id == x.msgTypeId).name;
                    }
                    else
                    {
                        CityStoreMsgType city_StoreMsgType = CityStoreMsgTypeBLL.SingleModel.GetModel(x.msgTypeId);
                        if (city_StoreMsgType != null)
                        {
                            x.msgTypeName = city_StoreMsgType.name;
                        }
                    }

                    x.msgDetail = HttpUtility.HtmlDecode(x.msgDetail);

                });
            }

            return listCity_Msg;
        }


        /// <summary>
        /// 根据用户Id获取该用户的帖子信息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="userId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="orderType">帖子排序字段 默认为0表示按照时间降序排列 1表示先按照置顶排序 然后再按照时间</param>
        /// <returns></returns>
        public List<CityMsg> getListByUserId(int aid, out int totalCount, int userId, int pageSize = 10, int pageIndex = 1, int orderType = 0)
        {
            List<string> listMsgId = new List<string>();

            List<CityMorders> listCityMorder = new CityMordersBLL().GetList($"MinisnsId={aid} and payment_status=1 and Status=1");
            if (listCityMorder != null && listCityMorder.Count > 0)
            {
                foreach (CityMorders item in listCityMorder)
                {
                    listMsgId.Add(item.CommentId.ToString());
                }

            }
            else
            {
                listMsgId.Add("0");
            }

            string strWhere = $"aid={aid} and userId={userId} and ( (state<>-1 and topDay=0) or (state<>-1 and Id in({string.Join(",",listMsgId)}))) ";
            string orderWhere = " addTime desc";//默认按照时间降序排序
                                                //if (orderType == 1)
                                                //{
                                                //    orderWhere = " topDay desc,addTime desc";//先按照置顶排序 然后再按照时间




            //    strWhere += "";
            //}

            
            totalCount = base.GetCount(strWhere);

            List<CityMsg> listCity_Msg = base.GetList(strWhere, pageSize, pageIndex, "*", orderWhere);
            if (listCity_Msg != null && listCity_Msg.Count > 0)
            {
                string userIds = string.Join(",", listCity_Msg.Select(s=>s.userId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                string cityMsgIds = string.Join(",",listCity_Msg.Select(s=>s.msgTypeId));
                List<CityStoreMsgType> cityStoreMsgTypeList = CityStoreMsgTypeBLL.SingleModel.GetListByIds(cityMsgIds);

                string msgShareIds = string.Join(",",listCity_Msg.Select(s=>s.Id));
                List<CityMsgViewFavoriteShare> cityMsgViewFavoriteShareList = CityMsgViewFavoriteShareBLL.SingleModel.GetListByMsgIds(msgShareIds);

                listCity_Msg.ForEach(x =>
                {
                    //获取用户头像
                    C_UserInfo c_UserInfo = userInfoList?.FirstOrDefault(f=>f.Id == x.userId);
                    if (c_UserInfo != null)
                    {
                        x.userName = c_UserInfo.NickName;
                        x.userHeaderImg = c_UserInfo.HeadImgUrl;
                    }

                    //获取帖子类别
                    CityStoreMsgType city_StoreMsgType = cityStoreMsgTypeList?.FirstOrDefault(f=>f.Id == x.msgTypeId);
                    if (city_StoreMsgType != null)
                    {
                        x.msgTypeName = city_StoreMsgType.name;
                    }

                    //根据帖子ID获取其浏览量-收藏量-分享量数据
                    CityMsgViewFavoriteShare city_MsgViewFavoriteShare = cityMsgViewFavoriteShareList?.FirstOrDefault(f=>f.Id == x.Id);
                    if (city_MsgViewFavoriteShare != null)
                    {
                        x.ViewCount = city_MsgViewFavoriteShare.ViewCount;
                        x.FavoriteCount = city_MsgViewFavoriteShare.FavoriteCount;
                        x.ShareCount = city_MsgViewFavoriteShare.ShareCount;
                        x.DzCount = city_MsgViewFavoriteShare.DzCount;
                    }

                    x.showTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.addTime);
                    x.msgDetail = HttpUtility.HtmlDecode(x.msgDetail);
                });
            }

            return listCity_Msg;
        }


        public List<CityMsg> getListCity_Msg(int aid, out int totalCount, int pageSize = 10, int pageIndex = 1, int msgTypeId = 0,string keyMsg="", int orderType = 0, double ws_lat = 0, double ws_lng = 0)
        {
           
            string strWhere = $"  aid={aid} and state=1 ";
            string strWhereCount = $" aid={aid} and state=1 ";
            if (msgTypeId > 0)
            {
                strWhere += $"  and msgTypeId={msgTypeId}";
                strWhereCount += $"  and msgTypeId={msgTypeId}";
            }
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(keyMsg))
            {
                strWhere += $" and msgDetail like @keyMsg ";
                strWhereCount += $" and msgDetail like @keyMsg ";
                parameters.Add(new MySqlParameter("keyMsg", $"%{keyMsg}%"));
            }
            //TODO 测试时的时间单位为分钟MINUTE  线上为天DAY
            string sql = $@"SELECT * FROM( SELECT * FROM (SELECT *,(DATE_ADD(addtime,INTERVAL topDay DAY)-now()) as tspan from CityMsg where (DATE_ADD(addtime,INTERVAL topDay DAY)-now())>0 and { strWhere} ORDER BY addtime desc LIMIT 100000)t1 
UNION
 SELECT* FROM(
SELECT*, (DATE_ADD(addtime, INTERVAL topDay DAY)-now()) as tspan from CityMsg where (DATE_ADD(addtime, INTERVAL topDay DAY) - now()) <= 0 and { strWhere}  ORDER BY addtime desc LIMIT 100000) t2
) t3 limit {(pageIndex - 1) * pageSize},{pageSize}";
            if (orderType > 0)
            {//表示附近
                sql = $@"SELECT * FROM( SELECT * FROM (SELECT *,(DATE_ADD(addtime,INTERVAL topDay DAY)-now()) as tspan,ROUND(6378.138*2*ASIN(SQRT(POW(SIN(({ws_lat}*PI()/180-lat*PI()/180)/2),2)+COS({ws_lat}*PI()/180)*COS(lat*PI()/180)*POW(SIN(({ws_lng}*PI()/180-lng*PI()/180)/2),2)))*1000) AS distance from CityMsg where (DATE_ADD(addtime,INTERVAL topDay DAY)-now())>0 and { strWhere} ORDER BY distance asc,addtime desc LIMIT 100000)t1 
UNION
 SELECT* FROM(
SELECT*, (DATE_ADD(addtime, INTERVAL topDay DAY)-now()) as tspan,ROUND(6378.138*2*ASIN(SQRT(POW(SIN(({ws_lat}*PI()/180-lat*PI()/180)/2),2)+COS({ws_lat}*PI()/180)*COS(lat*PI()/180)*POW(SIN(({ws_lng}*PI()/180-lng*PI()/180)/2),2)))*1000) AS distance from CityMsg where (DATE_ADD(addtime, INTERVAL topDay DAY) - now()) <= 0 and { strWhere}  ORDER BY distance asc, addtime desc LIMIT 100000) t2
) t3 limit {(pageIndex - 1) * pageSize},{pageSize}";
            }     
            List<CityMsg> listCity_Msg = base.GetListBySql(sql,parameters.ToArray());
            if (listCity_Msg != null && listCity_Msg.Count > 0)
            {
                string userIds = string.Join(",", listCity_Msg.Select(s => s.userId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                string cityMsgIds = string.Join(",", listCity_Msg.Select(s => s.msgTypeId));
                List<CityStoreMsgType> cityStoreMsgTypeList = CityStoreMsgTypeBLL.SingleModel.GetListByIds(cityMsgIds);

                string msgShareIds = string.Join(",", listCity_Msg.Select(s => s.Id));
                List<CityMsgViewFavoriteShare> cityMsgViewFavoriteShareList = CityMsgViewFavoriteShareBLL.SingleModel.GetListByMsgIds(msgShareIds);

                listCity_Msg.ForEach(x =>
                {
                    //获取用户头像 昵称
                    C_UserInfo c_UserInfo = userInfoList?.FirstOrDefault(f=>f.Id == x.userId);
                    if (c_UserInfo != null)
                    {
                        x.userName = c_UserInfo.NickName;
                        x.userHeaderImg = c_UserInfo.HeadImgUrl;
                    }

                    //获取帖子类别
                    CityStoreMsgType city_StoreMsgType = cityStoreMsgTypeList?.FirstOrDefault(f=>f.Id
                    == x.msgTypeId);
                    if (city_StoreMsgType != null)
                    {
                        x.msgTypeName = city_StoreMsgType.name;
                    }

                    //根据帖子ID获取其浏览量-收藏量-分享量数据
                    CityMsgViewFavoriteShare city_MsgViewFavoriteShare = cityMsgViewFavoriteShareList?.FirstOrDefault(f=>f.Id == x.Id);
                    if (city_MsgViewFavoriteShare != null)
                    {
                        x.ViewCount = city_MsgViewFavoriteShare.ViewCount;
                        x.FavoriteCount = city_MsgViewFavoriteShare.FavoriteCount;
                        x.ShareCount = city_MsgViewFavoriteShare.ShareCount;
                        x.DzCount = city_MsgViewFavoriteShare.DzCount;
                    }

                    x.showTimeStr = CommondHelper.GetTimeSpan(DateTime.Now - x.addTime);
                    x.msgDetail = HttpUtility.HtmlDecode(x.msgDetail);
                });
            }



            totalCount = base.GetCount(strWhereCount, parameters.ToArray());


            return listCity_Msg;
        }


        public string saveMsg(string jsondata, int ordertype, int paytype, int aid, ref CityMorders order,int userId)
        {
            if (string.IsNullOrEmpty(jsondata))
                return "json参数错误";

            SaveCityMsgModel saveCityMsgModelJson = JsonConvert.DeserializeObject<SaveCityMsgModel>(jsondata);
            if (saveCityMsgModelJson == null)
                return "null参数错误";

            double lng = 0.00;
            double lat = 0.00;

            //校验手机号码  
            //if (!Regex.IsMatch(saveCityMsgModelJson.phone, @"\d")|| saveCityMsgModelJson.phone.Length>11)
            //    return "联系号码不符合";

            if (string.IsNullOrEmpty(saveCityMsgModelJson.location))
                return "请填写地址";

            if (string.IsNullOrEmpty(saveCityMsgModelJson.msgDetail))
                return "发布信息不能为空";

            if (!double.TryParse(saveCityMsgModelJson.lng, out lng) || !double.TryParse(saveCityMsgModelJson.lat, out lat))
                return "地址坐标错误";

            if (saveCityMsgModelJson.msgType <= 0)
                return "请选择信息类别";

            CityStoreBanner storebanner = CityStoreBannerBLL.SingleModel.getModelByaid(aid);
            if (storebanner == null)
            {
                return "商家配置异常";
            }


            CityStoreUser cityStoreUser = CityStoreUserBLL.SingleModel.getCity_StoreUser(aid, userId);
            
            if (cityStoreUser != null && cityStoreUser.state != 0)
            {
                if(string.IsNullOrEmpty(storebanner.KeFuPhone))
                {
                    return "账户异常";
                }
                else
                {
                    return $"账户异常,请拨打电话({storebanner.KeFuPhone})联系客服";
                }
                
            }

            CityMsg city_Msg = new CityMsg();
            city_Msg.addTime = DateTime.Now;
            city_Msg.aid = aid;
            city_Msg.lat = saveCityMsgModelJson.lat;
            city_Msg.lng = saveCityMsgModelJson.lng;
            city_Msg.location = saveCityMsgModelJson.location;
            city_Msg.msgDetail = HttpUtility.HtmlEncode(saveCityMsgModelJson.msgDetail);
           
            city_Msg.msgTypeId = saveCityMsgModelJson.msgType;
            city_Msg.phone = saveCityMsgModelJson.phone;
            city_Msg.userId = userId;
            city_Msg.imgs = saveCityMsgModelJson.imgs;

            
            
            switch (storebanner.ReviewSetting)
            {
                case 0://不需要审核
                    if (saveCityMsgModelJson.isTop == 0)
                    {
                        //不置顶消息
                        city_Msg.state = 1;
                        city_Msg.Review = 0;
                       
                    }
                    else
                    {
                        city_Msg.state = 0;//支付回成功调后再变更1
                        city_Msg.Review = 0;
                       
                    }
                    break;

                case 1://先审核后发布
                    city_Msg.state = 0;//审核通过后变为1
                    city_Msg.Review = 1;//审核通过后变为2  审核不通过变为-1
                    break;
                case 2://先发布后审核
                    if (saveCityMsgModelJson.isTop == 0)
                    {
                        //不置顶消息
                        city_Msg.state = 1;
                        city_Msg.Review = 1;//审核通过后变为2  审核不通过变为-1
                        
                    }
                    else
                    {
                        city_Msg.state = 0;//支付回成功调后再变更为1
                        city_Msg.Review = 1;//审核通过后变为2  审核不通过变为-1
                       
                    }
                    break;
            }

            if (saveCityMsgModelJson.isTop == 0)
            {
                //表示不置顶的消息
                city_Msg.topDay = 0;
                city_Msg.topCostPrice = 0;
               
                int msgId = Convert.ToInt32(base.Add(city_Msg));
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
                if (saveCityMsgModelJson.ruleId <= 0)
                {
                    return "请选择置顶时间";
                }

                CityStoreMsgRules _city_StoreMsgRules = CityStoreMsgRulesBLL.SingleModel.getCity_StoreMsgRules(aid, saveCityMsgModelJson.ruleId);
                if (_city_StoreMsgRules == null)
                {
                    return "非法操作(置顶时间有误)";
                }

                city_Msg.topDay = _city_StoreMsgRules.exptimeday;
                city_Msg.topCostPrice = _city_StoreMsgRules.price;
               
                int msgId = Convert.ToInt32(base.Add(city_Msg));
                if (msgId <= 0)
                {
                    return "发布信息异常";
                }
                else
                {
                    order.Articleid = _city_StoreMsgRules.Id;
                    order.CommentId = msgId;
                    order.MinisnsId = aid;
                    order.payment_free = city_Msg.topCostPrice;
                    order.ShowNote= $"小程序同城发布消息付款{order.payment_free * 0.01}元";
                    return string.Empty;
                }
            }
        }
        
        public bool delMsg(int id,int userId,int appId)
        {
            CityMsg model = base.GetModel($"Id={id} and userId={userId} and aid={appId}");
            if (model == null)
                return false;
            model.state = -1;
            return base.Update(model, "state");
        }


        /// <summary>
        /// 判断类别下是否关联了消息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="msgTypeId"></param>
        /// <returns></returns>
        public bool msgTypeHaveMsg(int aid,int msgTypeId)
        {

            string strWhere = $"aid={aid} and msgTypeId={msgTypeId} and state=1";

            return base.Exists(strWhere);
        }


        /// <summary>
        /// 腾讯地图,返回距离(公里) 
        /// 有误差,同腾讯地图api 8公里 误差在0.1米以内
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double EARTH_RADIUS = 6378.137;
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

        private double rad(double d)
        {
            return d * Math.PI / 180.0;
        }
        
        public List<CityMsg> GetListByIds(int aid, string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<CityMsg>();

            string strWhere = $"aid={aid} and Id in({ids})";
            return base.GetList(strWhere);
        }

        /// <summary>
        /// 根据id集合获取City_Msg列表数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<CityMsg> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<CityMsg>();

            string strWhere = $"Id in({ids})";
            return base.GetList(strWhere);
        }
        
        public int GetCountByAId(int aid)
        {
            return base.GetCount($"aid={aid} and state<>-1");
        }
    }
}
