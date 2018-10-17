using DAL.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    public class WebSiteConfig
    {
        /// <summary>
        /// 微社区全地址
        /// </summary>
        public static string WsqUrl
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["WsqUrl"]; }
        }

        /// <summary>
        /// 微社区后台
        /// </summary>
        public static string WsqAdminUrl
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["WsqAdminUrl"]; }
        }

        /// <summary>
        /// 图片js,css 地址
        /// </summary>
        public static string SourceContent
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["SourceContent"]; }
        }
        /// <summary>
        /// 小程序资源url
        /// </summary>
        public static string MiniappZyUrl
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["MiniappZyUrl"]; }
        }

        /// <summary>
        /// 用于网页授权的微信帐号 小未科技公众号
        /// </summary>
        public static string WxSerId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["WxSerId"]; }
        }
        /// <summary>
        /// 用于网页授权的微信帐号 小未公司公众号
        /// </summary>
        public static string XWGS_WxSerId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XWGS_WxSerId"]; }
        }
        /// <summary>
        /// 用于点赞小程序后台用户接受消息的公众号  --小未科技
        /// </summary>
        public static string DZ_WxSerId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["DZ_WxSerId"]; }
        }

        /// <summary>
        /// 点赞后台商家接收 用户 下单使用的模板消息
        /// </summary>
        public static string DZ_paySuccessTemplateId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["DZ_paySuccessTemplateId"]; }
        }

        /// <summary>
        /// 点赞后台商家接收 用户 下单使用的模板消息
        /// </summary>
        public static string DZ_outOrderTemplateId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["DZ_outOrderTemplateId"]; }
        }

        /// <summary>
        /// 腾讯url api使用key
        /// </summary>
        public static string Tx_MapKey
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["Tx_MapKey"]; }
        }

        /// <summary>
        /// 用于网页授权的微信帐号
        /// </summary>
        public static string WxAppId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["WxAppId"]; }
        }

        /// <summary>
        /// 用于JSSDK的微信帐号
        /// </summary>
        public static string JsSDKSerId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["JsSDKSerId"]; }
        }
        public static string XWGS_JsSDKSerId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XWGS_JsSDKSerId"]; }
        }

        /// <summary>
        /// cookie 域
        /// </summary>
        public static string DefaultForumDomain
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["DefaultForumDomain"] ?? string.Empty; }
        }
        /// <summary>
        /// 网络URL
        /// </summary>
        public static string ImageUrl
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["ImageUrl"]; }
        }



        /// <summary>
        /// 转发器地址
        /// </summary>
        public static string WxApiUrl
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["transferruleUrl"]; }
        }
        public static string OpenDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["OpenDomain"] ?? "http://open.vzan.com";
            }
        }

        public static string XcxAPI
        {
            get
            {
                return ConfigurationManager.AppSettings["XcxAPI"] ?? "http://openapp.vzan.com/XcxApi/";
            }
        }

        public static string XcxAPIDzOpen
        {
            get
            {
                return ConfigurationManager.AppSettings["XcxAPIDzOpen"] ?? "http://dzopen.vzan.com/XcxApi/";
            }
        }
        public static string GoToGetAuthoUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["GoToGetAuthoUrl"];
            }
        }
        

        /// <summary>
        /// 小程序授权回调链接
        /// </summary>
        public static string XcxAppReturnUrl
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XcxAppReturnUrl"]; }
        }


        #region 小程序异常信息邮件推送相关配置

        /// <summary>
        /// 推送开关 1 开启 0 关闭
        /// </summary>
        public static string XcxAppSmtpState
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XcxAppSmtpState"]; }
        }

        /// <summary>
        /// 小程序发邮件SmtpHost 主机
        /// </summary>
        public static string XcxAppSmtpHost
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XcxAppSmtpHost"]; }
        }

        /// <summary>
        /// 小程序发邮件SmtpPort   端口
        /// </summary>
        public static string XcxAppSmtpPort
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XcxAppSmtpPort"]; }
        }

        /// <summary>
        /// 小程序发邮件账户昵称 
        /// </summary>
        public static string XcxAppSmtpNickName
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XcxAppSmtpNickName"]; }
        }

        /// <summary>
        /// 小程序发邮件账户 SmtpUser
        /// </summary>
        public static string XcxAppSmtpUser
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XcxAppSmtpUser"]; }
        }

        /// <summary>
        /// 小程序发邮件账户 SmtpPwd  授权码或者密码
        /// </summary>
        public static string XcxAppSmtpPwd
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XcxAppSmtpPwd"]; }
        }


        /// <summary>
        /// 小程序接收异常信息推送邮箱
        /// </summary>
        public static string XcxAppReceiveEmail
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["XcxAppReceiveEmail"]; }
        }
        #endregion

        #region 足浴模板消息-模板id
        /// <summary>
        /// 顾客预约提醒模板消息
        /// </summary>
        public static string DZ_footbath_ReserveTemplateId
        {
            get { return ConfigurationManager.AppSettings["DZ_footbath_ReserveTemplateId"]; }
        }

        /// <summary>
        /// 预订单超时提醒模板消息
        /// </summary>
        public static string DZ_footbath_ReserveTimeOutTemplateId
        {
            get { return ConfigurationManager.AppSettings["DZ_footbath_ReserveTimeOutTemplateId"]; }
        }
        #endregion

        #region 多门店模板消息-模板id
        public static string DZ_multiStore_paySuccessTemplateId
        {
            get { return ConfigurationManager.AppSettings["DZ_multiStore_paySuccessTemplateId"]; }
        }
        public static string DZ_multiStore_outOrderTemplateId
        {
            get { return ConfigurationManager.AppSettings["DZ_multiStore_outOrderTemplateId"]; }
        }
        #endregion

        /// <summary>
        /// 客户预约通知
        /// </summary>
        public static string DZ_ReserveInformTemplateId
        {
            get { return ConfigurationManager.AppSettings["DZ_ReserveInformTemplateId"]; }
        }

        public static string DZ_paySuccessTemplateId_new
        {
            get { return ConfigurationManager.AppSettings["DZ_paySuccessTemplateId_new"]; }
        }

        /// <summary>
        /// 小程序官网IIS绑定配置(IP:端口:主机名)
        /// </summary>
        public static string DzWebSiteBindString
        {
            get { return ConfigurationManager.AppSettings["DzWebSiteBindString"]; }
        }

        /// <summary>
        /// 小程序官网IIS里的网站名称
        /// </summary>
        public static string DzWebSiteName
        {
            get { return ConfigurationManager.AppSettings["DzWebSiteName"]; }
        }


        /// <summary>
        /// 小程序官网域名带www
        /// </summary>
        public static string DzWebSiteDomain
        {
            get { return ConfigurationManager.AppSettings["DzWebSiteDomain"]; }
        }

        /// <summary>
        /// 小程序官网域名后缀
        /// </summary>
        public static string DzWebSiteDomainExt
        {
            get { return ConfigurationManager.AppSettings["DzWebSiteDomainExt"]; }
        }
        public static int TemplateAid
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["TemplateAid"]); }
        }
        /// <summary>
        /// 商家版小程序 appid
        /// </summary>
        public static string StoreAppid
        {
            get { return ConfigurationManager.AppSettings["store_appid"]; }
        }
        public static string StoreOrderPath
        {
            get { return ConfigurationManager.AppSettings["store_order_path"]; }
        }


        /// <summary>
        /// 小程序同城版  举报信息处理消息通知模板
        /// </summary>
        public static string City_UserReportMsgTemplate
        {
            get { return ConfigurationManager.AppSettings["City_UserReportMsgTemplate"]; }
        }
        /// <summary>
        /// 小程序同城版  点赞信息消息通知模板
        /// </summary>
        public static string City_UserDzMsgTemplate
        {
            get { return ConfigurationManager.AppSettings["City_UserDzMsgTemplate"]; }
        }

        /// <summary>
        /// 小程序同城版  帖子举报原因 每条原因逗号分隔
        /// </summary>
        public static string City_ReportReasonTemplate
        {
            get { return ConfigurationManager.AppSettings["City_ReportReasonTemplate"]; }
        }

        /// <summary>
        /// 小程序cdn地址
        /// </summary>
        public static string cdnurl
        {
            get { return ConfigurationManager.AppSettings["cdnurl"]; }
        }

        /// <summary>
        /// 小程序cdn版本
        /// </summary>
        public static string cdnVersion
        {
            get { return ConfigurationManager.AppSettings["cdnVersion"]; }
        }

        /// <summary>
        /// 小程序是否要经过客服审核，1：是，0：否
        /// </summary>
        public static string ServiceShenHe
        {
            get { return ConfigurationManager.AppSettings["serviceshenhe"]; }
        }
        /// <summary>
        /// 小程序是否要经过客服审核，1：是，0：否
        /// </summary>
        public static string AuthoAppType
        {
            get { return ConfigurationManager.AppSettings["AuthoAppType"]; }
        }
        

        /// <summary>
        /// 代理商开通体验模板有效天数
        /// </summary>
        public static int ExperienceDayLength
        {
            get
            {
                int daylength = 0;
                int.TryParse(ConfigurationManager.AppSettings["ExperienceDayLength"], out daylength);
                return daylength;
            }
        }

        /// <summary>
        /// 装修模型用户登陆ID，只有该用账号登陆上去才能编辑装修模板
        /// </summary>
        public static string CustomerLoginId
        {
            get { return ConfigurationManager.AppSettings["CustomerLoginId"] ?? string.Empty; }
        }

        public static string WebDomain
        {
            get { return ConfigurationManager.AppSettings["WebDomain"] ?? string.Empty; }
        }

        /// <summary>
        /// 是否使用阿拉丁统计
        /// </summary>
        public static bool UseALaDing
        {

            get
            {
                string value = ConfigurationManager.AppSettings["UseALaDing"] ?? "";
                bool result = false;
                Boolean.TryParse(value, out result);
                return result;
            }
        }

        /// <summary>
        /// 运行环境：dev=开发环境，pro=生产环境，test=测试环境
        /// </summary>
        public static string Environment
        {
            get { return ConfigurationManager.AppSettings["Environment"] ?? string.Empty; }
        }

        #region 拼享惠公众号提现配置
        public static string GongZhongAppId
        {
            get { return ConfigurationManager.AppSettings["gongzhong_appid"] ?? string.Empty; }
        }
        public static string GongZhongSecret
        {
            get { return ConfigurationManager.AppSettings["gongzhong_secret"] ?? string.Empty; }
        }
        public static string GongZhongReturnUrl
        {
            get { return ConfigurationManager.AppSettings["gongzhong_returnUrl"] ?? string.Empty; }
        }
        #endregion

        #region 小程序服务器域名设置
        /// <summary>
        /// 请求域名
        /// </summary>
        public static string requesthost
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["requesthost"]; }
        }
        /// <summary>
        /// 上传域名
        /// </summary>
        public static string uploadFilehost
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["uploadFilehost"]; }
        }
        /// <summary>
        /// 下载域名
        /// </summary>
        public static string downloadFilehost
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["downloadFilehost"]; }
        }
        /// <summary>
        /// socket域名
        /// </summary>
        public static string sokethost
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["sokethost"]; }
        }
        /// <summary>
        /// 业务域名
        /// </summary>
        public static string WebviewHost
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["WebviewHost"]; }
        }
        #endregion

        /// <summary>
        /// 上传校验业务域名txt文件保存路径
        /// </summary>
        public static string CheckWebViewHost
        {
            get
            {
                return ConfigurationManager.AppSettings["CheckWebViewHost"] ?? string.Empty;
            }
        }

        #region crm系统对接配置
        public static string CrmApiHost
        {
            get { return ConfigurationManager.AppSettings["CrmApiHost"] ?? string.Empty; }
        }
        public static string CrmVersionCode
        {
            get { return ConfigurationManager.AppSettings["CrmVersionCode"] ?? string.Empty; }
        }
        public static string CrmDevice
        {
            get { return ConfigurationManager.AppSettings["CrmDevice"] ?? string.Empty; }
        }
        public static string CrmLogin
        {
            get { return ConfigurationManager.AppSettings["CrmLogin"] ?? string.Empty; }
        }

        public static string CrmPassword
        {
            get { return ConfigurationManager.AppSettings["CrmPassword"] ?? string.Empty; }
        }
        #endregion

        public static string DZ_payTipsTemplateId
        {
            get
            {
                return ConfigurationManager.AppSettings["DZ_payTipsTemplateId"] ?? string.Empty;
            }
        }

        /// <summary>
        /// 支付回调链接
        /// </summary>

        public static string WebConfigMiappNotifyUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["WebConfigMiappNotifyUrl"] ?? string.Empty;
            }
        }
    }
}
