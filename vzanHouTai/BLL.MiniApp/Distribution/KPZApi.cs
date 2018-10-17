using Entity.MiniApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Utility;
using System.Configuration;
using System.Text.RegularExpressions;
using DAL.Base;
using Core.MiniApp;

namespace BLL.MiniApp
{
    /// <summary>
    /// 快跑者配送接口
    /// </summary>
    public static class KPZApi
    {
        public static string _host = ConfigurationManager.AppSettings["kpzapi_url"];//"https://open.keloop.cn";
        public static string _version = ConfigurationManager.AppSettings["kpzapi_version"];// "1";
        public static string _devKey = ConfigurationManager.AppSettings["kpzapi_kevkey"];//"A5UHBOH0EUBUHMHKLQLI60P60MD9BOQ5";     // 开发密钥
        public static string _devSecret = ConfigurationManager.AppSettings["kpzapi_devsecret"];//"U194HFWJWZL6PVS4J2MUVIVB182JR2VX";  // 签名密钥
        //public static string _teamToken = "KGLET6FFATP6GU7L";  // 团队 Token
        // ********************** 订单相关接口 ********************** //
        public static string _getFeeApiUrl = "/open/order/getFee?";//配送费
        public static string _createOrderApiUrl = "/open/order/createOrder";//下单
        public static string _getOrderInfoApiUrl = "/open/order/getOrderInfo?";//获取订单信息
        public static string _getOrderLogApiUrl = "/open/order/getOrderLog?";//获取订单进程
        public static string _getCourierTagApiUrl = "/open/order/getCourierTag?";//获取配送员最新坐标
        public static string _cancelOrderApiUrl = "/open/order/cancelOrder";//取消订单
        public static string _getTeamInfoApiUrl = "/open/team/getTeamInfo?";//获取团队信息

        /// <summary>
        /// 生成订单
        /// </summary>
        public static KPZResult<OrderTradeNo> CreateOrder(KPZOrder data,string teamToken)
        {
            //log4net.LogHelper.WriteInfo(typeof(KPZApi),"快跑者订单："+JsonConvert.SerializeObject(data));
            //string orderId = "2654849875465125499";  // 生成一个第三方订单 ID
            ////string preTime = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");   // 预计一个小时之后送达
            //string ordertime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //KPZOrder data = new KPZOrder()
            //{
            //    shop_id = 1,
            //    shop_name = "小未商家",
            //    shop_tel = "18718463809",
            //    shop_address = "广东省天河区天河公园",
            //    shop_tag = "113.366326,23.128052",
            //    order_content = "2份烧白开(100x1),2份拉面(18x1)",
            //    order_note = "不要太辣了",
            //    //order_mark = "12",
            //    order_from = "小未科技",
            //    order_time = ordertime,
            //    //order_photo = "http://a4.att.hudong.com/38/47/19300001391844134804474917734_950.png",
            //    note = orderId,
            //    customer_name = "张三丰",
            //    customer_tel = "18288888888",
            //    customer_address = "广东省天河区天河公园",
            //    customer_tag = "113.366326,23.128052",
            //    order_no = orderId,
            //    order_price = 99.99f,
            //    order_origin_price = 100.00f,
            //    pay_status = 1,
            //    pay_fee = 1.66f,
            //    //pre_times = preTime,
            //};

            Dictionary<string, string> param = KPZApi.GetParamApi(data,teamToken);
            param["sign"] = KPZApi.GetSign(param, KPZApi._devSecret);

            return KPZApi.KPZRequest<OrderTradeNo>(KPZApi._createOrderApiUrl, param);
        }
        /// <summary>
        /// 获取订单详情
        /// </summary>
        public static KPZResult<object> GetOrderInfo(string orderid, string teamToken)
        {
            //string orderId = "18061516542500002";  // 生成一个第三方订单 ID
            object data = new { trade_no = orderid };
            Dictionary<string, string> param = KPZApi.GetParamApi(data, teamToken);
            param["sign"] = KPZApi.GetSign(param, KPZApi._devSecret);

            return KPZApi.KPZRequest<object>(KPZApi._getOrderInfoApiUrl, param, "get");
        }
        /// <summary>
        /// 获取订单详情
        /// </summary>
        public static KPZResult<object> GetOrderLog(string orderid, string teamToken)
        {
            //string orderId = "18061516542500002";  // 生成一个第三方订单 ID
            object data = new { trade_no = orderid };
            Dictionary<string, string> param = KPZApi.GetParamApi(data, teamToken);
            param["sign"] = KPZApi.GetSign(param, KPZApi._devSecret);

            return KPZApi.KPZRequest<object>(KPZApi._getOrderLogApiUrl, param, "get");
        }
        /// <summary>
        /// 取消订单
        /// </summary>
        public static KPZResult<object> CancelOrder(string orderId, string reason, string teamToken)
        {
            //string orderId = "18061516542500002";  // 生成一个第三方订单 ID
            object data = new { trade_no = orderId, reason };
            Dictionary<string, string> param = KPZApi.GetParamApi(data, teamToken);
            param["sign"] = KPZApi.GetSign(param, KPZApi._devSecret);

            return KPZApi.KPZRequest<object>(KPZApi._cancelOrderApiUrl, param);
        }
        /// <summary>
        /// 获取配送员最新坐标
        /// </summary>
        public static KPZResult<object> GetCourierTag(string orderId, string teamToken)
        {
            //string orderId = "2654849875465125498";  // 生成一个第三方订单 ID
            object data = new { trade_no = orderId };
            Dictionary<string, string> param = KPZApi.GetParamApi(data, teamToken);
            param["sign"] = KPZApi.GetSign(param, KPZApi._devSecret);

            return KPZApi.KPZRequest<object>(KPZApi._getCourierTagApiUrl, param, "get");
        }
        /// <summary>
        /// 获取运费
        /// </summary>
        /// <param name="shopId">第三方商户ID</param>
        /// <param name="sendTag">送达坐标</param>
        /// <param name="getTag">取单坐标</param>
        /// <param name="orderPrice">第三方订单总价</param>
        /// <param name="payFee">第三方订单原配送费</param>
        public static KPZResult<KPZFee> GetFee(int shopId, string sendTag, string getTag, string orderPrice, string payFee, string teamToken)
        {
            KPZFee data = new KPZFee();
            data.shop_id = shopId;//1;
            data.customer_tag = sendTag; //"23.15098980049273,113.3218529820442";
            data.get_tag = getTag;//"23.15098980049273,113.3218529820442";
            data.order_price = orderPrice;//"1";
            data.pay_fee = payFee;//"1";
            Dictionary<string, string> param = KPZApi.GetParamApi(data, teamToken);
            param["sign"] = KPZApi.GetSign(param, KPZApi._devSecret);

            return KPZApi.KPZRequest<KPZFee>(KPZApi._getFeeApiUrl, param, "get");
        }

        /// <summary>
        /// 获取团队信息
        /// </summary>
        public static KPZResult<object> GetTeamInfo(string teamToken)
        {
            object data = new object();
            Dictionary<string, string> param = KPZApi.GetParamApi(data, teamToken);
            param["sign"] = KPZApi.GetSign(param, KPZApi._devSecret);

            return KPZApi.KPZRequest<object>(KPZApi._getTeamInfoApiUrl, param, "get");
        }

        public static KPZResult<T> KPZRequest<T>(string apiUrl, Dictionary<string, string> param, string type = "post")
        {
            KPZResult<T> dataResult = new KPZResult<T>();
            string url = _host + apiUrl;
            string result = "";
            try
            {
                switch (type)
                {
                    case "post":
                        string json = SortParamString(param);
                        result = CommonCore.HttpPost(url, json);
                        break;
                    case "get":
                        url += SortParamString(param);
                        result = HttpHelper.GetData(url);
                        break;
                }
                KPZResult<object> tempResult = JsonConvert.DeserializeObject<KPZResult<object>>(result);
                if(tempResult.code == 200)
                {
                    dataResult = JsonConvert.DeserializeObject<KPZResult<T>>(result);
                }
                else
                {
                    dataResult.code = tempResult.code;
                    dataResult.message = tempResult.message;
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(typeof(KPZApi),$"快跑者接口请求异常：{ex.Message},result={result}");
            }

            return dataResult;
        }

        #region 签名
        public static Dictionary<string, string> GetParamApi(object data,string teamToken)
        {
            string expire_time = KPZApi.GetTimeStamp().ToString().Substring(0, 10) + 60;
            Dictionary<string, string> param = new Dictionary<string, string>();
            param["dev_key"] = KPZApi._devKey;
            param["version"] = KPZApi._version;
            param["ticket"] = Guid.NewGuid().ToString();
            param["team_token"] = teamToken;
            param["body"] = JsonConvert.SerializeObject(data);
            param["timestamp"] = expire_time;

            return param;
        }
        /// <summary>
        /// 除去数组中的空值和签名参数
        /// </summary>
        /// <param name="param">签名参数组</param>
        /// <returns>获取去掉空值与签名参数后的新签名参数组</returns>
        public static Dictionary<string, string> ParaFilter(Dictionary<string, string> param)
        {
            Dictionary<string, string> filterParam = new Dictionary<string, string>();
            foreach (var item in param)
            {
                //去掉 '',null,保留数字0
                if (item.Key == "sign" || item.Key == "sign_type" || item.Key == "key" || string.IsNullOrEmpty(item.Value))
                {
                    continue;
                }
                else
                {
                    filterParam[item.Key] = item.Value;
                }
            }

            return filterParam;
        }

        /// <summary>
        /// 把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串
        /// </summary>
        /// <param name="param">需要拼接的数组</param>
        /// <returns>拼接完成以后的字符串</returns>
        public static string SortParamString(Dictionary<string, string> param)
        {
            //对数组排序
            var myParameters = param.OrderBy(c => c.Key);
            StringBuilder data = new StringBuilder();
            foreach (var par in myParameters)
            {
                data.AppendFormat("{0}={1}&", par.Key, par.Value);
            }
            //去掉最后一个&字符,并转码
            return data.ToString().TrimEnd('&');
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="param">密的参数数组</param>
        /// <param name="sessionkey">加密的key</param>
        /// <returns>生产的签名</returns>
        public static string GetSign(Dictionary<string, string> param, string sessionkey)
        {
            if (param == null || string.IsNullOrEmpty(sessionkey))
            {
                return "";
            }

            // 除去待签名参数数组中的空值和签名参数
            param = ParaFilter(param);
            string sortStr = SortParamString(param);
            //string temp = encrypt(sortstr + _devSecret);
            //string temp1 = DESEncryptTools.GetMd5Base32(sortstr + _devSecret);
            return Encrypt(sortStr + _devSecret);
        }
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(int ministes = 0)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime dtNow = DateTime.Now.AddMinutes(ministes);
            TimeSpan toNow = dtNow.Subtract(dtStart);
            return toNow.TotalMilliseconds.ToString().Substring(0, 10) + 60;
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Encrypt(string source)
        {
            byte[] sor = Encoding.UTF8.GetBytes(source);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(sor);
            StringBuilder strBuilder = new StringBuilder(40);
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));//加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位

            }
            return strBuilder.ToString();
        }

        #endregion
    }
}
