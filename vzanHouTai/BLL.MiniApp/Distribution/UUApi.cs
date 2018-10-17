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
using System.Net;
using System.IO;

namespace BLL.MiniApp
{
    public static class UUApi
    {
        public static string _host = ConfigurationManager.AppSettings["uuapi_url"];
        public static string _notisUrl = ConfigurationManager.AppSettings["uuapi_notisurl"];//回调链接
        public static string _appid = ConfigurationManager.AppSettings["uuapi_appid"];// 开发appid
        public static string _appsecret = ConfigurationManager.AppSettings["uuapi_appsecret"];// 签名密钥
        public static string _openid = "a824e5d3650b4aee822e32526c732022";  // 商户openid
        // ********************** 相关接口 ********************** //
        public static string _getOrderPriceApiUrl = "/getorderprice.ashx";//获取订单价格
        public static string _addOrderApiUrl = "/addorder.ashx";//下单
        public static string _cancelOrderApiUrl = "/cancelorder.ashx";//取消订单
        public static string _getOrderDetailApiUrl = "/getorderdetail.ashx";//查看订单详情
        public static string _bindUserApplyApiUrl = "/binduserapply.ashx";//用户申请绑定
        public static string _bindUserSubmitApiUrl = "/bindusersubmit.ashx";//用户提交绑定
        public static string _cancelBindApiUrl = "/cancelbind.ashx";//用户解除绑定

        #region 订单相关
        /// <summary>
        /// 获取运费
        /// </summary>
        /// <param name="openId">商户注册时返回的OpenId</param>
        /// <param name="fromAddress">起始地址</param>
        /// <param name="fromNote">起始详细地址</param>
        /// <param name="toAddress">送达地址</param>
        /// <param name="toNote">送达详细地址</param>
        /// <param name="cityName">城市，必须带上'市'</param>
        /// <param name="countyName">区域</param>
        /// <returns></returns>
        public static UUGetPriceResult GetOrderPrice(string openId,string fromAddress,string fromNote,string toAddress,string toNote,string cityName,string countyName)
        {
            UUGetPrice model = new UUGetPrice();
            model.appid = _appid;
            model.nonce_str = Guid.NewGuid().ToString().Replace("-", "");
            model.timestamp = GetTimestamp();
            model.openid = openId;
            model.origin_id = DateTime.Now.ToString("yyyyMMddHHmmss");
            model.from_address = fromAddress;
            model.from_usernote = fromNote;
            model.to_address = toAddress;
            model.to_usernote = toNote;
            model.city_name = cityName;
            model.county_name = countyName;
            model.from_lng = "0";
            model.from_lat = "0";
            model.to_lng = "0";
            model.to_lat = "0";
            Dictionary<string, string> mydic = GetDictionary(model);
            model.sign = CreateMd5Sign(mydic, _appsecret);
            mydic.Add("sign", model.sign);
            string url = _host + _getOrderPriceApiUrl;
            string resultJson = HttpPost(url, mydic);
            UUGetPriceResult result = JsonConvert.DeserializeObject<UUGetPriceResult>(resultJson);
            return result;
            //{"price_token":"a69de77bf4a24065868c4cc8d58b32d2","total_money":"7.00","need_paymoney":"0.00","total_priceoff":"7.00","distance":"0","freight_money":"7.00","business_priceoff":"0.00","couponid":"80087951","coupon_amount":"7.00","addfee":"0.00","goods_insurancemoney":"0.00","expires_in":"1200","origin_id":"20180725173348","return_code":"ok","return_msg":"价格计算成功","appid":"d2fe3af49ce544c4b5c0731aa5ab28df","nonce_str":"63d34d12f1104fbfabc78dce6477908c","sign":"1F26C26E2E089BCDFA96391326E4179E"}
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static UUBaseResult AddOrder(UUOrder model)
        {
            model.nonce_str = Guid.NewGuid().ToString().Replace("-", "");
            model.timestamp = GetTimestamp();
            Dictionary<string, string> mydic = GetDictionary(model);
            model.sign = CreateMd5Sign(mydic, _appsecret);
            mydic.Add("sign", model.sign);
            string url = _host + _addOrderApiUrl;
            string resultJson = HttpPost(url, mydic);
            UUBaseResult result = JsonConvert.DeserializeObject<UUBaseResult>(resultJson);
            return result;
            //{"ordercode":"U23108001807251739989279322","origin_id":"20180725173348","return_code":"ok","return_msg":"订单发布成功","appid":"d2fe3af49ce544c4b5c0731aa5ab28df","nonce_str":"29d8c95be2fe4b73bb648211b94fee0b","sign":"EDDF8CDC25DB95E3CF98CFCA13EA6F5A"}
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="openId">商户openid</param>
        /// <param name="orderCode">uu平台返回的order_code</param>
        /// <param name="reason">取消的原因，非必填</param>
        /// <returns></returns>
        public static UUBaseResult CanecelOrder(string openId,string orderCode,string reason)
        {
            UUCancelOrder model = new UUCancelOrder();
            model.appid = _appid;
            model.nonce_str = Guid.NewGuid().ToString().Replace("-", "");
            model.timestamp = GetTimestamp();
            model.openid = openId;
            model.order_code = orderCode;
            model.reason = reason;
            Dictionary<string, string> mydic = GetDictionary(model);
            model.sign = CreateMd5Sign(mydic, _appsecret);
            mydic.Add("sign", model.sign);

            string url = _host + _cancelOrderApiUrl;
            string resultJson = HttpPost(url, mydic);
            UUBaseResult result = JsonConvert.DeserializeObject<UUBaseResult>(resultJson);
            return result;
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="openId">商户openid</param>
        /// <param name="orderCode">uu平台返回的订单order_code</param>
        /// <returns></returns>
        public static UUBaseResult GetOrderDetail(string openId,string orderCode)
        {
            UUBase model = new UUBase();
            model.appid = _appid;
            model.nonce_str = Guid.NewGuid().ToString().Replace("-", "");
            model.timestamp = GetTimestamp();
            model.openid = openId;
            model.order_code = orderCode;
            Dictionary<string, string> mydic = GetDictionary(model);
            model.sign = CreateMd5Sign(mydic, _appsecret);
            mydic.Add("sign", model.sign);

            string url = _host + _getOrderDetailApiUrl;
            string resultJson = HttpPost(url, mydic);
            UUBaseResult result = JsonConvert.DeserializeObject<UUBaseResult>(resultJson);
            return result;
        }
        #endregion

        #region 用户相关
        /// <summary>
        /// 用户申请，发送验证码
        /// </summary>
        /// <param name="mobile">注册的手机号</param>
        /// <returns></returns>
        public static UUBaseResult BindUserApply(string mobile)
        {
            UUBindUserApply model = new UUBindUserApply();
            model.appid = _appid;
            model.nonce_str = Guid.NewGuid().ToString().Replace("-", "");
            model.timestamp = GetTimestamp();
            model.user_mobile = mobile;
            model.user_ip = WebHelper.GetIP();
            Dictionary<string, string> mydic = GetDictionary(model);
            model.sign = CreateMd5Sign(mydic, _appsecret);
            mydic.Add("sign", model.sign);

            string url = _host + _bindUserApplyApiUrl;
            string resultJson = HttpPost(url, mydic);
            UUBaseResult result = JsonConvert.DeserializeObject<UUBaseResult>(resultJson);
            return result;
        }

        /// <summary>
        /// 用户绑定
        /// </summary>
        /// <param name="mobile">商户注册的手机号</param>
        /// <param name="valCode">验证码</param>
        /// <param name="cityName">城市</param>
        /// <param name="countyName">区域</param>
        /// <returns></returns>
        public static UUBaseResult BindUserSubmit(string mobile,string valCode,string cityName,string countyName)
        {
            UUUserSubmit model = new UUUserSubmit();
            model.appid = _appid;
            model.nonce_str = Guid.NewGuid().ToString().Replace("-", "");
            model.timestamp = GetTimestamp();
            model.openid = _openid;
            model.user_mobile = mobile;
            model.validate_code = valCode;
            model.city_name = cityName;
            model.county_name = countyName;
            model.reg_ip = WebHelper.GetIP();
            Dictionary<string, string> mydic = GetDictionary(model);
            model.sign = CreateMd5Sign(mydic, _appsecret);
            mydic.Add("sign", model.sign);

            string url = _host + _bindUserSubmitApiUrl;
            string resultJson = HttpPost(url, mydic);

            UUBaseResult result = JsonConvert.DeserializeObject<UUBaseResult>(resultJson);
            return result;
            //{"openid":"eff85e7142304bae9acd45cf1563eaf2","return_code":"ok","return_msg":"用户绑定成功","appid":"d2fe3af49ce544c4b5c0731aa5ab28df","nonce_str":"15757e727a2141bf8017c30dbaec7cd8","sign":"6697C9EB3A2C290A5C253A8CDAA654BE"}
        }

        /// <summary>
        /// 取消绑定
        /// </summary>
        /// <param name="openId">商户openid</param>
        /// <returns></returns>
        public static UUBaseResult CancelBind(string openId)
        {
            UUUserSubmit model = new UUUserSubmit();
            model.appid = _appid;
            model.nonce_str = Guid.NewGuid().ToString().Replace("-", "");
            model.timestamp = GetTimestamp();
            model.openid = openId;
            Dictionary<string, string> mydic = GetDictionary(model);
            model.sign = CreateMd5Sign(mydic, _appsecret);
            mydic.Add("sign", model.sign);

            string url = _host + _cancelBindApiUrl;
            string resultJson = HttpPost(url, mydic);
            UUBaseResult result = JsonConvert.DeserializeObject<UUBaseResult>(resultJson);
            return result;
            //{"return_code":"ok","return_msg":"解除绑定成功","appid":"d2fe3af49ce544c4b5c0731aa5ab28df","nonce_str":"217fb4e25f6c4f1cabe53e7ad6545508","sign":"36D551A0B0E71A42FFBE12C21F006145"}
        }
        #endregion

        public static Dictionary<string, string> GetDictionary<T>(T obj)
        {
            Dictionary<string, string> mydic = new Dictionary<string, string>();
            if (obj == null)
                return mydic;
            PropertyInfo[] pros = obj.GetType().GetProperties();
            foreach (PropertyInfo item in pros)
            {
                if (item.GetCustomAttribute<NoJoinField>() != null)
                    continue;
                object value = item.GetValue(obj);
                if (value == null || value == DBNull.Value)
                    continue;
                mydic.Add(item.Name, value.ToString());
            }

            return mydic;
        }

        public static string GetTimestamp()
        {
            return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }

        public static string HttpPost(string Url, IDictionary<string, string> parameters)
        {
            try
            {
                StringBuilder postDataStr = new StringBuilder();
                if (parameters != null && parameters.Count > 0)
                {
                    int i = 0;
                    foreach (string key in parameters.Keys)
                    {
                        if (i > 0)
                        {
                            postDataStr.AppendFormat("&{0}={1}", key, parameters[key]);
                        }
                        else
                        {
                            postDataStr.AppendFormat("{0}={1}", key, parameters[key]);
                        }
                        i++;
                    }
                }
                byte[] dataArray = Encoding.UTF8.GetBytes(postDataStr.ToString());
                //创建请求
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
                request.Method = "POST";
                request.ContentLength = dataArray.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                //创建输入流
                Stream dataStream = null;
                dataStream = request.GetRequestStream();
                //发送请求
                dataStream.Write(dataArray, 0, dataArray.Length);
                dataStream.Close();
                //读取返回消息
                string res = string.Empty;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                res = reader.ReadToEnd();
                reader.Close();
                return res;
            }
            catch
            {
                return "";
            }
        }

        #region 签名
        public static string CreateMd5Sign(Dictionary<string, string> Parameters, string AppKey)
        {
            var myParameters = Parameters.OrderBy(c => c.Key);
            StringBuilder data = new StringBuilder();
            foreach (var par in myParameters)
            {
                if (par.Key.ToUpper() != "SIGN" && !string.IsNullOrEmpty(par.Value))
                {
                    data.AppendFormat("{0}={1}&", par.Key, par.Value);
                }
            }
            data.Append("key=" + AppKey);
            var sing = MD5(data.ToString().ToUpper()).ToUpper();
            return sing;
        }

        public static string MD5(string inputStr)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hashByte = md5.ComputeHash(Encoding.UTF8.GetBytes(inputStr));
            StringBuilder sb = new StringBuilder();
            foreach (byte item in hashByte)
                sb.Append(item.ToString("x").PadLeft(2, '0'));
            return sb.ToString();
        }
        #endregion
    }
}
