using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity.MiniApp
{
    public class MemCacheKey
    {
        /// <summary>
        /// redis用户缓存
        /// </summary>
        public const string User_key = "minisns_user_new_{0}";

        //用户缓存(opengId+minisnsId)
        /// <summary>
        /// redis用户缓存
        /// </summary>
        public const string User_key_OpenId_MinisnsId = "minisns_user_Key_{0}_{1}";//用户缓存

        /// <summary>
        /// 微社区资料
        /// </summary>
        public const string MinisnsDetailKey = "MinisnsDetail_{0}";
        /// <summary>
        /// 全局黑名单缓存
        /// </summary>
        //public const string WholeBlackListKey = "wholeblacklist"; 
        public const string WholeBlackHashKey = "wholeblackhash_key";

        /// <summary>
        /// 语音缓存，语音ID
        /// </summary>
        public const string Voice_Key = "minisns_Voice_Key_key_{0}";
        /// <summary>
        /// 是否以接收到微信支付回调
        /// </summary>
        public const string ProcessNotify = "dz_transaction_id{0}";
        /// <summary>
        /// 第三方直播设置
        /// </summary>
        public const string PayCenterSettingKey = "PayCenterSetting_{0}_{1}";
    }
}
