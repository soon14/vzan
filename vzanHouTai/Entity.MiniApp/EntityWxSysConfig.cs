using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    public class EntityWxSysConfig
    {
        private static string DomainName(string accessToken)
        {
            string domainName = "https://api.weixin.qq.com";
            if (!string.IsNullOrEmpty(accessToken) && accessToken.Length < 30)
            {
                return domainName = WebSiteConfig.WxApiUrl;//ConfigurationManager.AppSettings["transferruleUrl"].ToString();
            }
            return domainName;
        }

        #region  群发url
        /// <summary>
        /// 群发接口  根据分组进行群发
        /// </summary>
        public static string FSendGroupUrl(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/message/mass/sendall?access_token={0}";
        }

        /// <summary>
        /// 群发接口 根据OpenId进行群发
        /// </summary>
        public static string FSendOpenIdUrl(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/message/mass/send?access_token={0}";
        }

        /// <summary>
        /// 群发状态
        /// </summary>
        public static string FSendResultUrls(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/message/mass/get?access_token={0}";
        }

        /// <summary>
        /// 删除群发
        /// </summary>
        public static string FSendDeleteUrls(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/message/mass/delete?access_token={0}";
        }

        /// <summary>
        /// 预览群发
        /// </summary>
        public static string FSendPreviewUrls(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/message/mass/preview?access_token={0}";
        }
        #endregion

        #region 分组
        /// <summary>
        /// 创建分组的Url
        /// </summary>
        public static string GroupCreateUrls(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/groups/create?access_token={0}";
        }

        /// <summary>
        /// 获取所有分组
        /// </summary>
        public static string GroupGetUrls(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/groups/get?access_token={0}";
        }

        /// <summary>
        /// 获取单个用户的分组
        /// </summary>
        public static string GroupUserUrls(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/groups/getid?access_token={0}";
        }

        /// <summary>
        /// 修改分组
        /// </summary>
        public static string GroupUpdateUrls(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/groups/update?access_token={0}";
        }

        /// <summary>
        /// 移动分组
        /// </summary>
        public static string GroupMembersUrls(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/groups/members/update?access_token={0}";
        }
        #endregion

        #region 多媒体操作

        /// <summary>
        /// 上传临时多媒体
        /// </summary>
        public static string MediaUpload(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/media/upload?access_token={0}&type={1}";
        }

        /// <summary>
        /// 下载临时多媒体
        /// </summary>
        public static string MediaDownload(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/media/get?access_token={0}&media_id={1}";
        }

        /// <summary>
        /// 上传永久多媒体
        /// </summary>
        public static string MediaYJUpload(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/material/add_material?access_token={0}";
        }

        /// <summary>
        /// 永久图文消息上传
        /// </summary>
        public static string NewsYJUpload(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/material/add_news?access_token={0}";
        }

        /// <summary>
        /// 临时图文消息上传
        /// </summary>
        public static string NewsUpload(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/media/uploadnews?access_token={0}";
        }

        /// <summary>
        /// 多媒体img上传
        /// </summary>
        public static string ImgUpload(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/media/uploadimg?access_token={0}";
        }

        #endregion

        #region 微信菜单

        /// <summary>
        /// 创建菜单
        /// </summary>
        public static string MenuCreateUrls(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/menu/create?access_token={0}";
        }

        /// <summary>
        /// 微信删除菜单Url
        /// </summary>
        public static string MenuDeleteUrl(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/menu/delete?access_token={0}";
        }

        /// <summary>
        /// 获取通过API调用设置的菜单
        /// </summary>
        public static string MenuGetUrl(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/menu/get?access_token={0}";
        }

        /// <summary>
        /// 本接口将会提供公众号当前使用的自定义菜单的配置，如果公众号是通过API调用设置的菜单，则返回菜单的开发配置，而如果公众号是在公众平台官网通过网站功能发布菜单，则本接口返回运营者设置的菜单配置。
        /// </summary>
        public static string MenuSelfInfoUrl(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/get_current_selfmenu_info?access_token={0}";
        }
        #endregion

        #region 微信自动回复
        public static string AmAway(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/get_current_autoreply_info?access_token={0}";
        }
        #endregion

        #region 客服发送消息接口
        /// <summary>
        /// 客服发送消息接口
        /// </summary>
        public static string CustomSendMsg(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/message/custom/send?access_token={0}";
        }
        #endregion

        #region 获取jsapi_ticket

        /// <summary>
        /// 微信JSSDK jsapi_ticket
        /// </summary>
        public static string Jsapi_ticket(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/ticket/getticket?access_token={0}&type={1}";
        }
        #endregion

        #region 用户信息
        /// <summary>
        ///获取用户信息
        /// </summary>
        public static string User_infoURL(string accessToken, bool isopenbingding = false)
        {
            if (isopenbingding)
                return WebSiteConfig.OpenDomain+"/cgibin/userinfo/{0}?openid={1}&lang=zh_CN";
            else
                return DomainName(accessToken) + "/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN";
        }
        #endregion

        #region 模板消息接口
        public static string MsgTtemplate(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/message/template/send?access_token={0}";
        }
        #endregion


        #region 网页授权
        /// <summary>
        /// 获取验证地址
        /// </summary>
        public static string OauthGetUrl
        {
            get
            {
                return "https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type={2}&scope={3}&state={4}#wechat_redirect";
            }
        }

        /// <summary>
        /// 获取网页授权的AccessToken
        /// </summary>
        public static string OauthAccessToken
        {
            get
            {
                return "https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type={3}";
            }
        }

        /// <summary>
        /// 刷新access_token（如果需要）
        /// </summary>
        public static string OauthRefreshToken
        {
            get
            {
                return "https://api.weixin.qq.com/sns/oauth2/refresh_token?appid={0}&grant_type={1}&refresh_token={2}";
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public static string OauthGetUserInfo
        {
            get
            {
                return "https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN";
            }
        }

        /// <summary>
        /// 检验授权凭证（access_token）是否有效
        /// </summary>
        public static string AuthIsAuth
        {
            get
            {
                return "https://api.weixin.qq.com/sns/auth?access_token={0}&openid={1}";
            }
        }

        #endregion

        #region 获取access_tokenURL
        /// <summary>
        /// 获取access_tokenURL http请求方式: GET
        /// </summary>
        public static string access_tokenURL = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";


        #endregion


        #region 二维码
        public static string QrCode_Url(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/qrcode/create?access_token={0}";
        }
        #endregion
        public static string Short_Url(string accessToken)
        {
            return DomainName(accessToken) + "/cgi-bin/shorturl?access_token={0}";
        }

    }
}
