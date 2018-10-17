using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.OpenWx
{
    /// <summary>
    /// 获取（刷新）授权公众号的令牌返回结果
    /// </summary>
    [Serializable]
    public class RefreshAuthorizerTokenResult : WxJsonResult
    {
        /// <summary>
        /// 授权方令牌
        /// </summary>
        public string authorizer_access_token { get; set; }
        /// <summary>
        /// 有效期，为2小时
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string authorizer_refresh_token { get; set; }

        public RefreshAuthorizerTokenResult()
        {
        }

        public RefreshAuthorizerTokenResult(string accessToken, string refreshToken, int expiresIn)
        {
            authorizer_access_token = accessToken;
            authorizer_refresh_token = refreshToken;
            expires_in = expiresIn;
        }
    }
}
