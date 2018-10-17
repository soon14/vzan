using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BLL.MiniApp.Fds
{
    public static class FoodYiLianYunPrintHelper
    {

        //接口地址  
        private const string IPstr = "open.10ss.net";
        //接口端口
        private const string port = "8888";

        private const string printUrl_yilianyun = "http://open.10ss.net:8888/";

        /// <summary>
        /// 添加打印机
        /// </summary>
        /// <param name="jqType">打印机类型</param>
        /// <param name="deviceNo">打印机编号</param>
        /// <param name="key">密钥</param>
        /// <param name="orderindex">订单索引(orderindex,该值由接口一返回)</param>
        /// <returns></returns>
        public static PrintErrorData addPrinter(string apikey, string partner, string machine_code, string msign, string mobilephone, string username, string printname)
        {
            string url = printUrl_yilianyun + "addprint.php";

            IDictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("partner", partner);
            parameter.Add("machine_code", machine_code);
            parameter.Add("username", username);
            parameter.Add("printname", printname);
            parameter.Add("mobilephone", mobilephone);

            var sign = getSign(parameter, apikey, msign);


            string parameterJson = $"partner={partner}&machine_code={machine_code}&msign={msign}&mobilephone={mobilephone}&username={username}&printname={printname}&sign={sign}";

            var json = DoPostJson(url, parameterJson);
            int result = -999;
            PrintErrorData errorData = new PrintErrorData();
            try
            {
                result = Convert.ToInt32(json);
                errorData.errno = result;
                switch (result)
                {
                    case 1:
                        errorData.error = "添加成功";
                        break;
                    case 2:
                        errorData.error = "重复添加";
                        break;
                    case 3:
                    case 4:
                        errorData.error = "添加失败";
                        break;
                    case 5:
                        errorData.error = "用户验证失败";
                        break;
                    case 6:
                        errorData.error = "非法终端号";
                        break;
                }
            }
            catch
            {
                errorData = JsonConvert.DeserializeObject<PrintErrorData>(json);
                
            }
            return errorData;


        }


        /// <summary>
        /// 删除打印机
        /// </summary>
        /// <param name="apikey">apikey（管理中心系统集成里获取）</param>
        /// <param name="partner">用户id（管理中心系统集成里获取）</param>
        /// <param name="machine_code">打印机终端号</param>
        /// <param name="msign">打印机终端密钥(orderindex,该值由接口一返回)</param>
        /// <returns></returns>
        public static string deletePrinter(string apikey, string partner, string machine_code, string msign)
        {
            string url = printUrl_yilianyun + "removeprint.php";

            IDictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("partner", partner);
            parameter.Add("machine_code", machine_code);


            var sign = getSign(parameter, apikey, msign);
            //string parameterJson = "{";
            //parameterJson += $" 'partner':'{partner}' ,";
            //parameterJson += $" 'machine_code':'{machine_code}' ,";
            //parameterJson += $" 'sign':'{sign}'";
            //parameterJson += " }";
            string parameterJson = $"partner={partner}&machine_code={machine_code}&sign={sign}";
            var json = DoPostJson(url, parameterJson); //1 删除成功 2 没这个设备 3 删除失败 4 认证失败


            //var model = JsonConvert.DeserializeObject<MsgModel365>(json);

            //model.isSuccess = model.responseCode.Equals("0") && model.msg.Equals("在线,工作状态正常");
            return json;
        }


        /// <summary>
        /// 打印机打印内容
        /// </summary>
        /// <param name="apikey">apikey（管理中心系统集成里获取）</param>
        /// <param name="machine_code">打印机终端号</param>
        /// <param name="msign">打印机终端密钥</param>
        /// <param name="partner">用户id（管理中心系统集成里获取）</param>
        /// <param name="time">当前10位数的时间戳（服务器用于验证超时）</param>
        /// <param name="content">打印的内容数据</param>
        /// <param name="isBase64">打印的内容数据是否为base64编码后的</param>
        /// <returns></returns>
        public static string printContent(string apikey, string partner, string machine_code, string msign,string content,bool isBase64 = false)
        {
            string url = printUrl_yilianyun;

            #region 开始生成时间戳
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var time = Convert.ToInt64(ts.TotalSeconds).ToString();     //获取时间戳
            #endregion

            IDictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("machine_code", machine_code);
            parameter.Add("partner", partner);
            parameter.Add("time", time);

            var sign = getSign(parameter, apikey, msign);
            
            if (isBase64)
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        string strDescription = content.Replace(" ", "+");
                        byte[] bytes = Convert.FromBase64String(strDescription);
                        content = System.Text.Encoding.UTF8.GetString(bytes);
                        //byte[] inArray = System.Text.Encoding.UTF8.GetBytes(content);
                        //content = Convert.ToBase64String(inArray);

                        content = System.Web.HttpUtility.UrlEncode(content, System.Text.Encoding.GetEncoding("UTF-8"));
                    }
                    catch
                    {
                    }
                }
                
            }
            else
            {
                content = System.Web.HttpUtility.UrlEncode(content, System.Text.Encoding.GetEncoding("UTF-8"));
            }
            //content = "%3Ccenter%3E8%E5%8F%B7%E6%A1%8C%3C%2Fcenter%3E";
            string parameterJson = $"partner={partner}&machine_code={machine_code}&time={time}&sign={sign}&content={content}";
            var json = DoPostJson(url, parameterJson);
            //var json = DoPostJson("http://o.vzan.com:3972/MiappFoods/FoodPrintList", "{'appId':'2'}");
            return json;
        }

        public static string addConfig(string apikey,string username, string partner, string machine_code, string msign, string printmenu)
        {
            string url = printUrl_yilianyun + "addprintmenu.php";
            var time = DateTime.Now.Ticks;
            IDictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("machine_code", machine_code);
            parameter.Add("partner", partner);
            parameter.Add("time", time.ToString());

            var sign = getSign(parameter, apikey, msign);

            string parameterJson = $"partner={partner}&machine_code={machine_code}&printmenu={printmenu}&time={time}&sign={sign}";

            return DoPostJson(url, parameterJson);
        }


        /// <summary>
        /// 创建本次调用的签名
        /// </summary>       
        /// <param name="parameters">parameters，参数列表</param>
        /// <param name="preKey">preKey，apikey的值</param>
        /// <param name="secKey">secKey，终端密钥的值</param>
        /// <returns>String，签名</returns>
        private static String getSign(IDictionary<string, string> parameters, string apikey, string msign)
        {
            // 第一步：把字典按Key的字母顺序排序
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters);
            IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();
            //IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            // 第二步：把所有参数名和参数值串在一起
            StringBuilder query = new StringBuilder("");

            while (dem.MoveNext())
            {
                string key = dem.Current.Key;
                string value = dem.Current.Value;
                //if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                //{
                    query.Append(key).Append(string.IsNullOrEmpty(value)? "" : value);
                //}
            }
            string source = query.ToString();
            source = apikey + source + msign;
            return GetMD5Hash(source);


        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns>MD5 32位大写</returns>
        public static string GetMD5Hash(String input)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] res = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            string md = BitConverter.ToString(res).Replace("-", "");
            return md;
            //byte[] result = Encoding.Default.GetBytes(input);    //tbPass为输入密码的文本框
            //MD5 md5 = new MD5CryptoServiceProvider();
            //byte[] output = md5.ComputeHash(result);
            //return BitConverter.ToString(output).Replace("-", "");
        }

        /// <summary>
        ///  构造模拟远程HTTP的POST请求，POST数据Json字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static string DoPostJson(string url, string jsonData)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=utf-8";
            using (Stream dataStream = request.GetRequestStream())
            {
                byte[] paramBytes = ASCIIEncoding.UTF8.GetBytes(jsonData);
                dataStream.Write(paramBytes, 0, paramBytes.Length);
                dataStream.Close();
            }
            HttpWebResponse res;
            try
            {
                res = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                res = (HttpWebResponse)ex.Response;
            }
            StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            return sr.ReadToEnd();
        }
    }

    /// <summary>
    /// 打印内容回调
    /// </summary>
    public class FoodYlyReturnModel
    {
       public int state { get; set; }

       public string id { get; set; }
    }

    public class PrintErrorData
    {
        public double errno { get; set; }

        public string error { get; set; }
        public string data { get; set; }
    }

}
