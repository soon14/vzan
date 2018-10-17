using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace BLL.MiniApp.Footbath
{
    public class FootBathBLL : BaseMySql<FootBath>
    {
        #region 单例模式
        private static FootBathBLL _singleModel;
        private static readonly object SynObject = new object();

        private FootBathBLL()
        {

        }

        public static FootBathBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FootBathBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<FootBath> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<FootBath>();

            return base.GetList(ids);
        }

        /// <summary>
        /// 根据AccountId查看有权限管理的店铺
        /// </summary>
        /// <param name="shopManagerId"></param>
        /// <returns></returns>
        public List<FootBath> GetListByAccountId(Guid AccountId, int AppId = 0)
        {
            List<UserRole> userRoles = UserRoleBLL.SingleModel.GetList($" {(AppId == 0 ? "" : $" AppId = {AppId} And ")} UserId = '{AccountId.ToString()}' And  State = 1 ") ?? new List<UserRole>();
            List<FootBath> stores = new List<FootBath>();
            FootBath store = null;
            foreach (UserRole userRole in userRoles)
            {
                store = GetModel(userRole.StoreId);
                if (store != null)
                {
                    stores.Add(store);
                }
            }
            return stores;
        }

        /// <summary>
        /// 根据 appId,storeId,及 模板类型返回店铺资料model
        /// </summary>
        /// <param name="appId">小程序权限表ID</param>
        /// <param name="storeId">店铺ID</param>
        /// <param name="tempType">模板类型</param>
        /// <returns></returns>
        public FootBath GetModelByParam(int appId, int storeId, int tempType)
        {
            return GetModel($" AppId = {appId} and storeId = {storeId} and TemplateType = {tempType} ");
        }


        /// <summary>
        /// 排列营业时间
        /// </summary>
        /// <param name="switchModel"></param>
        /// <returns></returns>
        public string GetShopDays(SwitchModel switchModel)
        {
            string result = string.Empty;
            List<string> fullarr = new List<string> { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };
            List<string> unselArr = new List<string>();
            if (!switchModel.Monday)
            {
                unselArr.Add("周一");
            }
            if (!switchModel.Tuesday)
            {
                unselArr.Add("周二");
            }
            if (!switchModel.Wensday)
            {
                unselArr.Add("周三");
            }
            if (!switchModel.Thursday)
            {
                unselArr.Add("周四");
            }
            if (!switchModel.Friday)
            {
                unselArr.Add("周五");
            }
            if (!switchModel.Saturday)
            {
                unselArr.Add("周六");
            }
            if (!switchModel.Sunday)
            {
                unselArr.Add("周日");
            }
            if (unselArr.Count == 0)
            {
                result = "周一至周日";
            }
            else
            {
                string fullstr = string.Join("|", fullarr);
                List<string> selarr = new List<string>();
                //过滤不要的，重新拼接数组
                foreach (var item in unselArr)
                {
                    selarr = Regex.Split(fullstr, $"{item}", RegexOptions.IgnoreCase).ToList();
                    for (int i = 0; i < selarr.Count; i++)
                    {
                        selarr[i] = selarr[i].Trim('|').Trim(',');
                    }
                    fullstr = string.Join(",", selarr);
                }
                //得出每个连续的字符串
                List<string> list = fullstr.Split(',').ToList();
                foreach (var str in list)
                {
                    var vals = str.Trim('|').Split('|');
                    if (vals.Length == 1)
                    {
                        result += vals[0] + ",";
                    }
                    else if (vals.Length > 1)
                    {
                        result += $"{vals[0]}至{vals[vals.Length - 1]},";
                    }
                }
                result = result.Trim(',');
            }
            return result;
        }
        /// <summary>
        /// 根据appid获取门店信息
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public FootBath GetModelByAppId(int appId)
        {
            if (appId <= 0)
            {
                return null;
            }
            return GetModel($"appId={appId}");
        }

        /// <summary>
        ///获取预订日期
        /// </summary>
        /// <param name="presetTime"></param>
        /// <returns></returns>
        public List<object> GetReservationTime(int presetTime)
        {
            List<object> result = new List<object>();
            string[] Day = new string[] { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;
            DateTime date = DateTime.Now;
            var obj = new { date = $"{date.ToString("yyyy-MM-dd")} ", dayStr = $"今天 {month}.{day}" };
            result.Add(obj);
            for (int i = 1; i <= presetTime; i++)
            {
                obj = new
                {
                    date = $"{date.AddDays(i).ToString("yyyy-MM-dd")} ",
                    dayStr = $"{Day[Convert.ToInt16(date.AddDays(i).DayOfWeek)]} {date.AddDays(i).Month}.{date.AddDays(i).Day}"
                };
                result.Add(obj);
            }
            return result;
        }


        #region 多门店
        /// <summary>
        /// 开通多门店时初始化店铺数据
        /// </summary>
        /// <param name="scount">开通门店数量</param>
        /// <param name="accountid">用户accountid</param>
        /// <param name="rid">模板权限ID</param>
        /// <returns></returns>
        public List<string> GetAddFootbathSQL(Guid accountid, int scount, int rid, DateTime overtime, int storeid = 0)
        {
            List<string> sqllist = new List<string>();
            FootBath miniaccount = new FootBath();
            if (scount > 0)
            {
                if (storeid == 0)
                {
                    //添加总店
                    miniaccount = new FootBath()
                    {
                        HomeId = storeid,
                        appId = rid,
                        State=1,
                        TemplateType = (int)TmpType.小程序多门店模板,
                        CreateDate = DateTime.Now,
                        OverTime = overtime,
                    };
                    storeid = Convert.ToInt32(Add(miniaccount));

                    //添加总店权限
                    UserRole userrole = new UserRole();
                    userrole.AppId = rid;
                    userrole.CreateDate = DateTime.Now;
                    userrole.RoleId = 1;
                    userrole.State = 1;
                    userrole.StoreId = 0;
                    userrole.UpdateDate = DateTime.Now;
                    userrole.UserId = accountid;
                    UserRoleBLL.SingleModel.Add(userrole);
                }

                //添加分店
                for (int i = 0; i < scount; i++)
                {
                    miniaccount = new FootBath()
                    {
                        HomeId = storeid,
                        appId = rid,
                        TemplateType = (int)TmpType.小程序多门店模板,
                        CreateDate = DateTime.Now,
                        OverTime = overtime,
                    };

                    sqllist.Add(BuildAddSql(miniaccount));
                }
            }

            return sqllist;
        }
        #endregion

        #region 多门店 判断店铺是否打烊以及当前位置选择的配送方式是否在有效范围内主要是用于订单

        /// <summary>
        /// 判断当前时间是否在店铺营业时间 只有返回为1的时候表示当前店铺未打烊
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="takeType"></param>
        /// <returns></returns>
        public int GetStoreShopTime(int storeId)
        {

            FootBath model = GetModel(storeId);
            if (model == null || model.State == 0 || model.IsDel == -1)
                return -1;
            if (string.IsNullOrEmpty(model.SwitchConfig))
                return 0;
            SwitchModel switchModel = JsonConvert.DeserializeObject<SwitchModel>(model.SwitchConfig);
            string curWeek = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(DateTime.Now.DayOfWeek);
            //string shopDays = GetShopDays(switchModel);//营业时间 星期 必须选择的

            bool isOpenShop = false;
            switch (curWeek) //根据当前星期几去判定当天开关是否开启
            {
                case "星期一":
                    isOpenShop = switchModel.Monday;
                    break;
                case "星期二":
                    isOpenShop = switchModel.Tuesday;
                    break;
                case "星期三":
                    isOpenShop = switchModel.Wensday;
                    break;
                case "星期四":
                    isOpenShop = switchModel.Thursday;
                    break;
                case "星期五":
                    isOpenShop = switchModel.Friday;
                    break;
                case "星期六":
                    isOpenShop = switchModel.Saturday;
                    break;
                case "星期日":
                    isOpenShop = switchModel.Sunday;
                    break;
                default:
                    isOpenShop = false;
                    break;
            }
            //if (!shopDays.Contains(curWeek))//当前时间不在店铺选择的营业星期
            //    return 0;
            if (!isOpenShop)//当前时间不在店铺选择的营业星期
                return 0;
            if (!switchModel.OpenAllDay)
            {
                //不是24小时营业再进行时间判断
                string[] shopTimes = model.ShopTime.Split('-');
                if (shopTimes.Length == 2)
                {
                    //当前时间不在店铺选择的营业时间段
                    if (!getTimeIsUse(DateTime.Now.ToString(), shopTimes[0], shopTimes[1]))
                        return 0;
                }

            }
            return 1;
        }


        /// <summary>
        /// 针对指定门店查询,当前所在位置选择的配送方式是否可用 只有返回为3的时候才表示可用  默认同城配送方式
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="takeType">multiStoreOrderType 枚举值,默认 (int)multiStoreOrderType.同城配送</param>
        /// <returns></returns>
        public int GetTakeWayIsUse(int storeId, double lat=0, double lng=0, int takeType = (int)multiStoreOrderType.同城配送)
        {
            int codeResult = 3;
            FootBath model = GetModel(storeId);
            if (model == null || model.State == 0 || model.IsDel == -1)
                codeResult = -2;
            if (string.IsNullOrEmpty(model.TakeOutWayConfig))
                codeResult = -1;
            //再进行是否在配送范围内
            TakeOutWayModel takeOutWayModel = JsonConvert.DeserializeObject<TakeOutWayModel>(model.TakeOutWayConfig);
            double distance = GetDistance(model.Lat, model.Lng, lat, lng);//当前客户坐标离店铺距离
            if (distance <= 0.1)
                distance = 0.00;
            //订单选择的配送方式
            switch (takeType)
            {
                case (int)multiStoreOrderType.同城配送:
                    //选择的为同城配送

                    if (!takeOutWayModel.cityService.IsOpen || takeOutWayModel.cityService.TakeRange < distance)//超过才算范围外
                        codeResult = 0;
                    break;
                case (int)multiStoreOrderType.到店自取:
                    //选择为到店自取
                    if (!takeOutWayModel.selfTake.IsOpen || distance > takeOutWayModel.selfTake.TakeRange)//到店自取超过自定义范围公里,断定超过范围(原需求没有提及)
                        codeResult = 1;
                    break;
                case (int)multiStoreOrderType.快递配送:
                    //快递配送
                    if (!takeOutWayModel.GetExpressdelivery.IsOpen)
                        codeResult = 2;
                    break;
            }
            return codeResult;
        }


        /// <summary>
        /// 通过当前位置以及选择的配送方式 按照由近到远的距离返回
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="type">0 同城配送 1到店自取</param>
        /// <returns></returns>
        public List<FootBath> GetNearUseStore(int appId,double lat, double lng, int type = 0)
        {
            var stores = GetList($"appId={appId} and State=1 and IsDel=0 and  HomeId<>0");//门店
            var bossStore = GetModel($"appId={appId} and  HomeId=0");//总店
            if (bossStore != null)
            {
                stores.Add(bossStore);
            }
            
            double distance = 0.00;
            List<FootBath> availableStores = new List<FootBath>();
            foreach (var item in stores)
            {
                if (!string.IsNullOrEmpty(item.SwitchConfig))
                {
                    item.switchModel = JsonConvert.DeserializeObject<SwitchModel>(item.SwitchConfig);
                    item.shopDays = GetShopDays(item.switchModel);
                }

                

                distance = GetDistance(item.Lat, item.Lng, lat, lng);
                if (distance <= 0.1)
                    distance = 0.00;//如果相差小于0.1公里就表示在同一个地方
                if (!string.IsNullOrEmpty(item.TakeOutWayConfig)&& GetStoreShopTime(item.Id)==1)
                {
                    item.takeOutWayModel = JsonConvert.DeserializeObject<TakeOutWayModel>(item.TakeOutWayConfig);

                    switch (type)
                    {
                        case 0:
                            //表示获取同城配送
                            if (item.takeOutWayModel.cityService.IsOpen && item.takeOutWayModel.cityService.TakeRange >= distance)
                            {
                                item.takeOutWayModel.TakeRangedistance = distance;
                                if (distance <= 1)
                                {
                                    item.takeOutWayModel.TakeRangedistanceStr = (distance * 1000).ToString("0.00") + "m";
                                }
                                else
                                {
                                    item.takeOutWayModel.TakeRangedistanceStr = (distance).ToString("0.00") + "km";
                                }
                                availableStores.Add(item);//加入在同城配送范围内列表里
                            }
                            break;
                        case 1:
                            //表示获取自取
                            if (item.takeOutWayModel.selfTake.IsOpen && distance <= item.takeOutWayModel.selfTake.TakeRange)// && distance <= item.takeOutWayModel.selfTake.TakeRange 去掉到店自取范围限制
                            {
                                //开启了到店自取的门店
                                item.takeOutWayModel.TakeRangedistance = distance;
                                if (distance <= 1)
                                {
                                    item.takeOutWayModel.TakeRangedistanceStr = (distance * 1000).ToString("0.00") + "m";
                                }
                                else
                                {
                                    item.takeOutWayModel.TakeRangedistanceStr = (distance).ToString("0.00") + "km";
                                }
                                availableStores.Add(item);//加入在到店自取送范围内列表里
                            }
                            break;
                    }

                }

            }

            return availableStores.OrderBy(s => s.takeOutWayModel.TakeRangedistance).ToList();

        }

        /// <summary>
        /// 根据收货地址坐标自动匹配最近的已开启同城配送的且在配送范围内的一家门店，
        ///若没有符合同城配送条件的门店，但快递配送已开启，则默认返回总店信息 否则直接返回NULL
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public FootBath GetNearCityStore(int appId, double lat, double lng)
        {
            List<FootBath> availableStores = GetNearUseStore(appId, lat, lng, 0);
            if (availableStores.Count > 0)
            {
                return availableStores.FirstOrDefault();
            }
            else
            {
                //表示没有符合同城配送条件的门店，但总店快递配送已开启，则默认显示总店名称
                var bossStore = GetModel($"appId={appId} and  HomeId=0");
                if (!string.IsNullOrEmpty(bossStore.TakeOutWayConfig))
                {
                    bossStore.takeOutWayModel = JsonConvert.DeserializeObject<TakeOutWayModel>(bossStore.TakeOutWayConfig);
                }
                if (bossStore.takeOutWayModel.GetExpressdelivery.IsOpen)
                    return bossStore;
                return null;
            }
        }



        #endregion
        /// <summary>
        /// 判断timeStr 时间 是否在startTime与endTime时间之内
        /// </summary>
        /// <param name="timeStr"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public bool getTimeIsUse(string timeStr, string startTime, string endTime)
        {

            TimeSpan dspWorkingDayAM = DateTime.Parse(startTime).TimeOfDay;
            TimeSpan dspWorkingDayPM = DateTime.Parse(endTime).TimeOfDay;

            //string time1 = "2017-2-17 8:10:00";
            DateTime t1 = Convert.ToDateTime(timeStr);

            TimeSpan dspNow = t1.TimeOfDay;
            if (dspNow > dspWorkingDayAM && dspNow < dspWorkingDayPM)
            {
                return true;
            }
            return false;
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

    }
}
