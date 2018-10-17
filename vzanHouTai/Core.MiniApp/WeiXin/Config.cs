using System;
using System.Collections.Generic;
using System.Web;

namespace Core.MiniApp
{
    /**
    * 	配置账号信息
    */
    public class WxPayConfig
    {
        //=======【基本信息设置】=====================================
        /* 微信公众号信息配置
        * APPID：绑定支付的APPID（必须配置）
        * MCHID：商户号（必须配置）
        * KEY：商户支付密钥，参考开户邮件设置（必须配置）
        * APPSECRET：公众帐号secert（仅JSAPI支付的时候需要配置）
        */
        //public static string APPID = BLL.MiniSNS.WebConfigBLL.WzLivePay ? BLL.MiniSNS.WebConfigBLL.WxPayAppId : System.Configuration.ConfigurationManager.AppSettings["WxAppId"];
        //public static string MCHID = BLL.MiniSNS.WebConfigBLL.WzLivePay ? BLL.MiniSNS.WebConfigBLL.MCHID : System.Configuration.ConfigurationManager.AppSettings["MCHID"];
        //public static string KEY = BLL.MiniSNS.WebConfigBLL.WzLivePay ? BLL.MiniSNS.WebConfigBLL.KEY : System.Configuration.ConfigurationManager.AppSettings["KEY"];
        //public static string APPSECRET = BLL.MiniSNS.WebConfigBLL.WzLivePay ? BLL.MiniSNS.WebConfigBLL.WxAppSecret : System.Configuration.ConfigurationManager.AppSettings["WxAppSecret"];

        public static string APPID
        {
            get
            {
               return BLL.MiniApp.WebConfigBLL.WzLivePay? BLL.MiniApp.WebConfigBLL.WxPayAppId : System.Configuration.ConfigurationManager.AppSettings["WxAppId"];
            }
        }
        public static string MCHID
        {
            get
            {
                return BLL.MiniApp.WebConfigBLL.WzLivePay ? BLL.MiniApp.WebConfigBLL.MCHID : System.Configuration.ConfigurationManager.AppSettings["MCHID"];
            }
        }
        public static string KEY
        {
            get
            {
                return BLL.MiniApp.WebConfigBLL.WzLivePay ? BLL.MiniApp.WebConfigBLL.KEY : System.Configuration.ConfigurationManager.AppSettings["KEY"];
            }
        }
        public static string APPSECRET
        {
            get
            {
                return BLL.MiniApp.WebConfigBLL.WzLivePay ? BLL.MiniApp.WebConfigBLL.WxAppSecret : System.Configuration.ConfigurationManager.AppSettings["WxAppSecret"];
            }
        }
        #region 暂时屏蔽 2016-11-05 10:03:12 xiaowei
        //public static string livePayAPPID
        //{
        //    get
        //    {
        //       return BLL.MiniSNS.WebConfigBLL.WzLivePay? BLL.MiniSNS.WebConfigBLL.livePayWxPayAppId : System.Configuration.ConfigurationManager.AppSettings["WxAppId"];
        //    }
        //}
        //public static string livePayMCHID
        //{
        //    get
        //    {
        //        return BLL.MiniSNS.WebConfigBLL.WzLivePay ? BLL.MiniSNS.WebConfigBLL.livePayMCHID : System.Configuration.ConfigurationManager.AppSettings["MCHID"];
        //    }
        //}
        //public static string livePayKEY
        //{
        //    get
        //    {
        //        return BLL.MiniSNS.WebConfigBLL.WzLivePay ? BLL.MiniSNS.WebConfigBLL.livePayKEY : System.Configuration.ConfigurationManager.AppSettings["KEY"];
        //    }
        //}
        //public static string livePayAPPSECRET
        //{
        //    get
        //    {
        //        return BLL.MiniSNS.WebConfigBLL.WzLivePay ? BLL.MiniSNS.WebConfigBLL.livePayWxAppSecret : System.Configuration.ConfigurationManager.AppSettings["WxAppSecret"];
        //    }
        //}

        //=======【证书路径设置】===================================== 
        #endregion
        /* 证书路径,注意应该填写绝对路径（仅退款、撤销订单时需要）
        */
        public static readonly string SSLCERT_PATH = System.Configuration.ConfigurationManager.AppSettings["SSLCERT_PATH"];
        public static readonly string SSLCERT_PASSWORD = System.Configuration.ConfigurationManager.AppSettings["SSLCERT_PASSWORD"];



        //=======【支付结果通知url】===================================== 
        /* 支付结果通知回调url，用于商户接收支付结果
        */
        public static readonly string NOTIFY_URL = System.Configuration.ConfigurationManager.AppSettings["NOTIFY_URL"];

        //=======【商户系统后台机器IP】===================================== 
        /* 此参数可手动配置也可在程序中自动获取
        */
        public static readonly string IP = System.Configuration.ConfigurationManager.AppSettings["IP"];


        //=======【代理服务器设置】===================================
        /* 默认IP和端口号分别为0.0.0.0和0，此时不开启代理（如有需要才设置）
        */
        //public const string PROXY_URL = "http://10.152.18.220:8080";

        //=======【上报信息配置】===================================
        /* 测速上报等级，0.关闭上报; 1.仅错误时上报; 2.全量上报
        */
        public static readonly int REPORT_LEVENL = 1;

    }
}