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

namespace BLL.MiniApp
{
    /// <summary>
    /// API调用规范
    /// 所有接口都采用HTTP协议
    /// 所有接口都采用POST方式（包括查询接口）
    /// 请求和响应的数据都为JSON格式。注：请设置Header的Content-Type为application/json
    /// 采用UTF-8字符编码
    /// 所有的接口必须app_secret进行接口签名，规则见接口签名规则
    /// 在测试环境，使用统一商户和门店进行发单。其中，商户id：73753，门店编号：11047059。
    /// </summary>
    public class DadaApi
    {
        /// <summary>
        /// 线上域名：newopen.imdada.cn
        /// 测试域名：newopen.qa.imdada.cn
        /// </summary>
        public readonly static string _dadaapihosturl = ConfigurationManager.AppSettings["dadaapi_url"];
        //第三方开发者app_id
        public readonly static string _appkey = ConfigurationManager.AppSettings["dadaapi_appid"]; 
        //第三方开发者秘钥
        public readonly string _appsecret = ConfigurationManager.AppSettings["dadaapi_secretKey"];
        //达达订单回调地址
        public readonly string _ordercallback = ConfigurationManager.AppSettings["dadaapi_ordercallback"];
        //达达取消订单缓存KEY
        public readonly string _cancelorderkey = "dada_cancelorder_"+ _appkey;

        #region 接口参数配置
        /// <summary>
        /// 商户ID
        /// </summary>
        public string _sourceid = "73753";
        /// <summary>
        /// 门店编号
        /// </summary>
        public string _shop_no = "11047059";
        //接口版本
        public const string _VERSION = "1.0";
        //接口格式
        public const string _FORMAT = "json";
        #endregion

        #region 接口链接
        //注册商户
        public readonly string _merchantapi= $"{ _dadaapihosturl}/merchantApi/merchant/add";

        //新增门店
        public readonly string _shopapi = $"{ _dadaapihosturl}/api/shop/add";

        //订单查询
        public readonly string _orderqueryapi = $"{ _dadaapihosturl}/api/order/status/query";

        //新增订单
        public readonly string _addorderapi = $"{ _dadaapihosturl}/api/order/addOrder";

        //重发订单
        public readonly string _readdorderapi = $"{ _dadaapihosturl}/api/order/reAddOrder";

        //订单预发布
        //查询订单运费接口
        public readonly string _querydeliverfeeorderapi = $"{ _dadaapihosturl}/api/order/queryDeliverFee";
        //查询运费后发单接口
        public readonly string _addafterqueryorderapi = $"{ _dadaapihosturl}/api/order/addAfterQuery";

        //查询城市
        public readonly string _citycodelistapi = $"{ _dadaapihosturl}/api/cityCode/list";

        //取消订单(线上环境)
        public readonly string _cancelorderapi = $"{ _dadaapihosturl}/api/order/formalCancel";

        //获取取消原因
        public readonly string _cancelorderreasonsapi = $"{ _dadaapihosturl}/api/order/cancel/reasons";

        //接受订单
        public readonly string _acceptorderapi = $"{ _dadaapihosturl}/api/order/accept";

        //完成取货
        public readonly string _fetchgoodapi = $"{ _dadaapihosturl}/api/order/fetch";

        //完成订单
        public readonly string _finishorderapi = $"{ _dadaapihosturl}/api/order/finish";

        //订单过期
        public readonly string _expireorderapi = $"{ _dadaapihosturl}/api/order/expire";

        //追加订单
        public readonly string _appendorderapi = $"{ _dadaapihosturl}/api/order/appoint/exist";

        //查询追加配送员
        public readonly string _transporterapi = $"{ _dadaapihosturl}/api/order/appoint/list/transporter";

        //取消追加订单
        public readonly string _cancelappendorderapi = $"{ _dadaapihosturl}/api/order/appoint/cancel";
        
        //添加消费
        public readonly string _addtip = $"{ _dadaapihosturl}/api/order/addTip";
        #endregion

        #region 实现接口
        /// <summary>
        /// 获取取消订单原因列表
        /// </summary>
        /// <returns></returns>
        public List<ResultReponseModel> GetCancelOrderReasonList()
        {
            List<ResultReponseModel> cancellist = RedisUtil.Get<List<ResultReponseModel>>(_cancelorderkey);
            if(cancellist!=null)
            {
                return cancellist;
            }

            object data = "";
            string json = PostParamJson(data);
            string url = _cancelorderreasonsapi;
            string result = HttpHelper.DoPostJson(url, json);
            if(!string.IsNullOrEmpty(result))
            {
                DadaApiReponseModel<List<ResultReponseModel>> resultmodel = JsonConvert.DeserializeObject<DadaApiReponseModel<List<ResultReponseModel>>>(result);
                if(resultmodel.status=="success" && resultmodel.result!=null && resultmodel.result.Count>0)
                {
                    cancellist = resultmodel.result;
                    RedisUtil.Set<List<ResultReponseModel>>(_cancelorderkey, cancellist, TimeSpan.FromHours(12));
                }
            }
            else{
                cancellist = new List<ResultReponseModel>();
            }

            log4net.LogHelper.WriteInfo(this.GetType(), "获取达达取消订单原因列表出错：" + result);
            return cancellist;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 请求参数处理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string PostParamJson(object model)
        {
            string json = JsonConvert.SerializeObject(model);
            
            DadaApiRequestModel data = new DadaApiRequestModel();
            data.app_key = _appkey;
            data.body = json;
            data.format = _FORMAT;
            data.source_id = _sourceid;
            data.v = _VERSION;
            data.timestamp = GetTimeStamp();
            data.signature = GetSignature(data);

            string postjson = JsonConvert.SerializeObject(data);
            return postjson;
        }
        
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
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
        
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public string GetTimeStamp()
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime dtNow = DateTime.Parse(DateTime.Now.ToString());
            TimeSpan toNow = dtNow.Subtract(dtStart);
            string timeStamp = toNow.Ticks.ToString();
            timeStamp = timeStamp.Substring(0, timeStamp.Length - 7);
            return timeStamp;
        }

        /// <summary>
        /// 时间戳转时间
        /// </summary>
        /// <param name="timstamp"></param>
        /// <returns></returns>
        public DateTime GetDateTimeByStamp(long timstamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timstamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);

            //DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
            //DateTime dt = startTime.AddMilliseconds(timstamp);
            //return dt;
        }
        
        /// <summary>
        /// 请求签名规则
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetSignature(object obj)
        {
            string signature = "";
            PropertyInfo[] pros = obj.GetType().GetProperties();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if(pros!=null && pros.Length>0)
            {
                foreach (PropertyInfo pro in pros)
                {
                    if(pro.Name == "signature")
                    {
                        continue;
                    }

                    Object value = pro.GetValue(obj);
                    if(value == null ||value == DBNull.Value || string.IsNullOrEmpty(value.ToString()))
                    {
                        dic.Add(pro.Name, "");
                        continue;
                    }
                    
                    dic.Add(pro.Name, value.ToString());
                }

                if(dic.Count>0)
                {
                    //第一步：将参与签名的参数按照键值(key)进行字典排序
                    //第二步：将排序过后的参数，进行key和value字符串拼接
                    signature = string.Join("",dic.OrderBy(x => x.Key,new KeyComparer() ).Select(x => x.Key + x.Value));
                    //log4net.LogHelper.WriteInfo(this.GetType(), "排序后拼接：" + signature);
                    //第三步：将拼接后的字符串首尾加上app_secret秘钥，合成签名字符串
                    signature = _appsecret + signature + _appsecret;
                    //signature = "appSecret" + signature + "appSecret";
                    //log4net.LogHelper.WriteInfo(this.GetType(), "首尾加上秘钥：" + signature);
                    //第四步：对签名字符串进行MD5加密，生成32位的字符串
                    signature = encrypt(signature);
                    //signature = DESEncryptTools.GetMd5Base32(signature);
                    //log4net.LogHelper.WriteInfo(this.GetType(), "加密后：" + signature);
                    //第五步：将签名生成的32位字符串转换为大写
                    signature = signature.ToUpper();
                    //log4net.LogHelper.WriteInfo(this.GetType(), "转大写后：" + signature);
                }
            }

            return signature;
        }

        /// <summary>
        /// 回调签名规则
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetSignatureR(object obj)
        {
            string signature = "";
            PropertyInfo[] pros = obj.GetType().GetProperties();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (pros != null && pros.Length > 0)
            {
                foreach (PropertyInfo pro in pros)
                {
                    if (pro.Name == "signature")
                    {
                        continue;
                    }

                    Object value = pro.GetValue(obj);
                    if (value == null || value == DBNull.Value || string.IsNullOrEmpty(value.ToString()))
                    {
                        dic.Add(pro.Name, "");
                        continue;
                    }
                    
                    dic.Add(pro.Name, value.ToString());
                }

                if (dic.Count > 0)
                {
                    //第一步：将参与签名的参数按照键值(value)进行字典排序
                    //第二步：将排序过后的参数，进行value字符串拼接
                    signature = string.Join("", dic.OrderBy(x => x.Value).Select(x => x.Value));
                    //第四步：对签名字符串进行MD5加密，生成32位的字符串
                    signature = DESEncryptTools.GetMd5Base32(signature);
                }
            }

            return signature;
        }
        #endregion
    }

    public class KeyComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return String.Compare(x, y, StringComparison.Ordinal);
        }
    }
}
