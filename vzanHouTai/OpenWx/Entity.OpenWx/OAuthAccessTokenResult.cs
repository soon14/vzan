using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.OpenWx
{
    /// <summary>
    /// 获取OAuth AccessToken的结果
    /// 如果错误，返回结果{"errcode":40029,"errmsg":"invalid code"}
    /// </summary>
    public class OAuthAccessTokenResult : WxJsonResult
    {
        /// <summary>
        /// 接口调用凭证
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// access_token接口调用凭证超时时间，单位（秒）
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        public string refresh_token { get; set; }
        /// <summary>
        /// 授权用户唯一标识
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 用户授权的作用域，使用逗号（,）分隔
        /// </summary>
        public string scope { get; set; }

        /// <summary>
        /// 审核状态，其中0为审核成功，1为审核失败，2为审核中 
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 当status=1，审核被拒绝时，返回的拒绝原因
        /// </summary>
        public string reason { get; set; }
        /// <summary>
        /// 会话密钥
        /// </summary>
        public string session_key { get; set; }
        /// <summary>
        /// 审核Id
        /// </summary>
        public int auditid { get; set; }
        public string authorizer_refresh_token { get; set; }
        public string authorizer_access_token { get; set; }

        public List<membersdata> members { get; set; }
        public List<string> page_list { get; set; }
    }

    public class membersdata
    {
        public string userstr { get; set; }
    }
}
