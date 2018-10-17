using System;
using System.IO;
using Utility;

namespace Core.MiniApp
{
    public class WxUtils
    {
        /// <summary>
        /// 判断是否出现异常
        /// </summary>
        /// <returns></returns>
       public static bool IsAccessTokenError(string returnText)
        {
            if (!string.IsNullOrEmpty(returnText) && returnText.IndexOf("errcode") > -1 && (returnText.IndexOf("40001") > -1 || returnText.IndexOf("42001") > -1))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断是否出现异常
        /// </summary>
        /// <returns></returns>
        public static bool IsError(string returnText)
        {
            if (!string.IsNullOrEmpty(returnText) && returnText.IndexOf("errcode") > -1)
            {
                log4net.LogHelper.WriteInfo(typeof(WxUtils), "微信错误返回值：" + returnText);
                return true;
            }
            return false;
        }


        /// <summary>
        /// 根据完整文件路径获取FileStream
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileStream GetFileStream(string fileName)
        {
            FileStream fileStream = null;
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                fileStream = new FileStream(fileName, FileMode.Open);
            }
            return fileStream;
        }


        public static DateTime BaseTime = new DateTime(1970, 1, 1);//Unix起始时间

        /// <summary>
        /// 转换微信DateTime时间到C#时间
        /// </summary>
        /// <param name="dateTimeFromXml">微信DateTime</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromXml(long dateTimeFromXml)
        {
            return BaseTime.AddTicks((dateTimeFromXml + 8 * 60 * 60) * 10000000);
        }
        /// <summary>
        /// 转换微信DateTime时间到C#时间
        /// </summary>
        /// <param name="dateTimeFromXml">微信DateTime</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromXml(string dateTimeFromXml)
        {
            return GetDateTimeFromXml(long.Parse(dateTimeFromXml));
        }

        /// <summary>
        /// 获取微信DateTime（UNIX时间戳）
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <returns></returns>
        public static long GetWeixinDateTime(DateTime dateTime)
        {
            return (dateTime.Ticks - BaseTime.Ticks) / 10000000 - 8 * 60 * 60;
        }
        public static string getAttachValue(string attach, string value)
        {
            string regex = "(?:^|\\?|&)" + value.ToLower() + "=(?<value>[\\s\\S]+?)(?:&|$)";
            return  CRegex.GetText(attach.ToLower(), regex, "value");
        }

    }
}
