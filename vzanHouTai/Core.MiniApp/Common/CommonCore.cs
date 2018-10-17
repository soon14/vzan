using DAL.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Utility;

namespace Core.MiniApp
{
    public class CommonCore
    {
        /// <summary>
        /// 将手机号码加*号
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public string PhoneToNickName(string phone)
        {
            try
            {
                if (StringHelper.IsMobile(phone))
                    return phone.Substring(0, 3) + "*****" + phone.Substring(8, 3);
                return phone;
            }
            catch { return phone; }
        }


        /// <summary>
        /// 将邮箱号码加*号
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public string EMaliToNickName(string email)
        {
            try
            {
                if (StringHelper.IsEmail(email))
                {
                    int idx = email.IndexOf('@');
                    int len = email.Length;
                    string preEmail = email.Substring(0, idx);//---获取@前面的字符
                    string etrEmail = email.Substring(idx, len - idx);//---获取@后面的字符
                    return preEmail.First() + "***" + preEmail.Last() + etrEmail;
                }
                return email;
            }
            catch { return email; }
        }

        public static string HttpPost(string Url, string postdata)
        {
            try
            {
                byte[] dataArray = Encoding.UTF8.GetBytes(postdata);
                //创建请求
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
                request.Method = "POST";
                request.ContentLength = dataArray.Length;
                //request.ContentType = "application/json";
                request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

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
        public static string HttpGet(string URL)
        {
            WebRequest wrt;
            wrt = WebRequest.Create(URL);
            wrt.Credentials = CredentialCache.DefaultCredentials;
            WebResponse wrp;
            wrp = wrt.GetResponse();
            string reader = new StreamReader(wrp.GetResponseStream(), Encoding.GetEncoding("gb2312")).ReadToEnd();
            try
            {
                wrt.GetResponse().Close();
            }
            catch (WebException ex)
            {
                throw ex;
            }
            return reader;
        }

        /// <summary>
        /// 订单号生成，规则
        /// 长度：24
        /// 年月日时分 + 购物车第一个ID
        /// </summary>
        /// <returns></returns>
        public static string CreateOrderNum(string id)
        {
            string ordernum = id;
            ordernum = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{(ordernum.Length >= 12 ? ordernum.Substring(ordernum.Length - 12, 12) : ordernum.PadLeft(12, '0'))}";

            return ordernum;
        }

        /// <summary>
        /// 订单号生成，规则
        /// 年月日时分秒加用户ID
        /// </summary>
        /// <returns></returns>
        public static string GetOrderNumByUserId(string userid)
        {
            string ordernum = userid;
            ordernum = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}{(ordernum.Length >= 10 ? ordernum.Substring(ordernum.Length - 12, 12) : ordernum.PadLeft(12, '0'))}";

            return ordernum;
        }
    }
}
