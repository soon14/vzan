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

namespace BLL.MiniApp
{
    public class FNApi
    {
        #region 单例模式
        private static FNApi _singleModel;
        private static readonly object SynObject = new object();

        private FNApi()
        {

        }

        public static FNApi SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FNApi();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 正式http请求URL: https://open-anubis.ele.me/anubis-webapi/
        /// 联调（测试）http请求URL: https://exam-anubis.ele.me/anubis-webapi/
        /// </summary>
        public static readonly string _fnapi_url = ConfigurationManager.AppSettings["fnapi_url"]; //"https://exam-anubis.ele.me/anubis-webapi/";
        public static readonly string _fnapi_appid = ConfigurationManager.AppSettings["fnapi_appid"]; //"3631052c-d65b-4048-acff-4b353e75aebb";//第三方商户app_id
        public static readonly string _fnapi_secretKey = ConfigurationManager.AppSettings["fnapi_secretKey"]; //"302bf072-be88-46bc-a97b-a482de667072";//第三方商户秘钥
        public static readonly string _callbackurl = ConfigurationManager.AppSettings["fnapi_callback"];//蜂鸟回调链接

        #region 接口链接
        /// <summary>
        /// 添加订单
        /// </summary>
        public static readonly string _addorderapi = $"{ _fnapi_url}v2/order";
        /// <summary>
        /// 取消订单
        /// </summary>
        public static readonly string _cancelorderapi = $"{ _fnapi_url}v2/order/cancel";
        /// <summary>
        /// 查询订单
        /// </summary>
        public static readonly string _queryorderapi = $"{ _fnapi_url}v2/order/query";
        /// <summary>
        /// 添加门店
        /// </summary>
        public static readonly string _addstoreapi = "https://open-anubis.ele.me/anubis-webapi/v2/chain_store";
        /// <summary>
        /// 查询门店
        /// </summary>
        public static readonly string _querystoreapi = $"{ _fnapi_url}v2/chain_store/query";
        /// <summary>
        /// 查询配送服务
        /// </summary>
        public static readonly string _querydeliveryapi = $"{ _fnapi_url}v2/chain_store/delivery/query";
        
        #endregion

        /// <summary>
        /// 获取token
        ///  http请求方式: GET
        ///  正式http请求URL: https://open-anubis.ele.me/anubis-webapi/get_access_token?app_id=APPID&salt=SALT&signature=SIGNATURE
        ///  联调http请求URL: https://exam-anubis.ele.me/anubis-webapi/get_access_token?app_id=APPID&salt=SALT&signature=SIGNATURE
        ///  http请求格式: application/json
        /// </summary>
        /// <returns></returns>
        public string GetTokenApiUrl()
        {
            int salt = RandomNum();// 要求1000-9999内随机数
            string json = $"app_id={_fnapi_appid}&salt={salt}&secret_key={_fnapi_secretKey}";

            //log4net.LogHelper.WriteInfo(this.GetType(), "jsonutf8编码前：" + json);
            string urlencodeData = UrlEncode(json);
            //log4net.LogHelper.WriteInfo(this.GetType(), "jsonutf8编码后：" + urlencodeData);
            string signature = encrypt(urlencodeData);
            //log4net.LogHelper.WriteInfo(this.GetType(), "加密后：" + signature);
            string url = $"{ _fnapi_url}get_access_token?app_id={_fnapi_appid}&salt={salt}&secret_key={_fnapi_secretKey}&signature={signature}";
            return url;
        }

        /// <summary>
        /// 调用蜂鸟配送接口
        /// </summary>
        /// <param name="data"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string UseFNApi(object data,string url)
        {
            DistributionApiConfig model = DistributionApiConfigBLL.SingleModel.GetModelByRedis(_fnapi_appid);
            string postdata = PostParam(model.access_token, data);
            string result = HttpHelper.DoPostJson(url, postdata);

            return result;
        }

        public string PostParam(string token, object model)
        {
            int salt = RandomNum();// 要求1000-9999内随机数
            string json = JsonConvert.SerializeObject(model);
            string urlencodeData = UrlEncode(json);
            string sig = GetSign(token, urlencodeData,salt);

            object postdata = new
            {
                app_id = _fnapi_appid,
                salt = salt,
                data = urlencodeData,
                signature = sig
            };

            return JsonConvert.SerializeObject(postdata); ;
        }

        public string GetSign(string token, string urlencodeData,int salt)
        {
            string postjson = $"app_id={_fnapi_appid}&access_token={token}&data={urlencodeData}&salt={salt}";
            string sig = encrypt(postjson);
            return sig;
        }
        
        /// <summary>
        /// // 要求1000-9999内随机数
        /// </summary>
        /// <returns></returns>
        public int RandomNum()
        {
            Random rd = new Random();
            int salt = rd.Next(1000, 10000);
            return salt;
        }

        /// <summary>
        /// （坑）java的url转码为大写，c#为小写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string UrlEncode(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                if (HttpUtility.UrlEncode(c.ToString()).Length > 1)
                {
                    builder.Append(HttpUtility.UrlEncode(c.ToString()).ToUpper());
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public double GetTimeStamp(int ministes=0)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime dtNow = DateTime.Now.AddMinutes(ministes);
            TimeSpan toNow = dtNow.Subtract(dtStart);
            return toNow.TotalMilliseconds;
        }

        public static string encrypt(string source)
        {
            byte[] sor = Encoding.UTF8.GetBytes(source);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(sor);
            StringBuilder strbul = new StringBuilder(40);
            for (int i = 0; i < result.Length; i++)
            {
                strbul.Append(result[i].ToString("x2"));//加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位

            }
            return strbul.ToString();
        }
    }
}
