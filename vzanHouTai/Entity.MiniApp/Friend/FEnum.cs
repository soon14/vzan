using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Friend
{
    /// <summary>
    /// 缓存Key
    /// </summary>
    public class FCacheKey
    {
        /// <summary>
        /// 用户缓存Key
        /// </summary>
        public static string FpUserKey = "FpUserInfo{0}";

        /// <summary>
        /// 平台站点Key
        /// </summary>
        public static string FpSiteKey = "FpSiteInfof{0}";


        public static string FGift = "FGift_{0}";

        /// <summary>
        /// 区域Key
        /// </summary>
        public static string FAreaRegion = "FAreaRegion";

        /// <summary>
        /// 数据词典KEY
        /// </summary>
        public static string FDataDictory = "FDataDictory";

        /// <summary>
        /// 用户首页浏览量
        /// </summary>
        public static string FuserIndexViews = "FUserIndexViews{0}";

        /// <summary>
        /// 平台用户总数
        /// </summary>
        public static string FpUserCounts = "FpUserCounts{0}";

        /// <summary>
        /// 用户搜索列表
        /// </summary>
        public static string FpSearchUserList = "FpSearchUserList{0}";

        /// <summary>
        /// 用户搜索列表
        /// </summary>
        public static string xq_IndexUserList = "xq_UserList{0}";

        /// <summary>
        /// 首页帖子列表
        /// </summary>
        public static string FpIndexArticleList = "FpIndexArticleList{0}";

        /// <summary>
        /// 交友首页最新动态列表
        /// </summary>
        public static string FhIndexArticleListByCreateDate = "FhIndexArticleList{0}";


        /// <summary>
        /// 用户资料完善度
        /// </summary>
        public static string FpUserInfoPerfectDegree = "FpUserInfoPerfectDegree_{0}_{1}";

        /// <summary>
        /// 用户的相片数（以openID为key）
        /// </summary>
        public static string FpUserPhotos = "FpUserPhotos_{0}_{1}";

        /// <summary>
        /// 用户的VIP还有几天（userId_MinisnsId）
        /// </summary>
        public static string FpUserVipDays = "FpUserVipDays_{0}_{1}";

        /// <summary>
        /// 交友用户任务日志列表
        /// </summary>
        public static string FUserTaskLogList = "FUserTaskLogList_{0}_{1}";

        /// <summary>
        /// 首页推荐用户列表{0}:平台Id
        /// </summary>
        public static string FpIndexUserList = "FpIndexUserList{0}";


        /// <summary>
        /// 排行榜邀请排名列表
        /// </summary>
        public static string keys_Ranklist = "ranklist_{0}_{1}";//0-minisnsId,1-type

        /// <summary>
        /// 交友访客总量
        /// </summary>
        public static string keys_Visitors = "visitor_{0}_{1}";//0-minisnsId,1-userId

        /// <summary>
        /// 交友访客记录 0-minisnsId,1-userId,2-tuserId,3-types,4-time
        /// </summary>
        public static string keys_VisitorLog = "visitor_{0}_{1}_{2}_{3}_{4}";

        /// <summary>
        /// 交友最新访客的状态 0:无1：有
        /// </summary>
        public static string keys_IsNewVisitor = "isnewvisitor_{0}_{1}";//0-minisnsId,1-userId

        /// <summary>
        /// 相亲用户缓存Key
        /// </summary>
        public static string FBlindUserKey = "FBlindUserInfo{0}";

        /// <summary>
        /// 相亲-我看过谁0-minisnsId,1-userId,2-tuserId，3-types
        /// </summary>
        public static string Xq_ViewLog = "FBlindUserInfo_{0}_{1}_{2}";
    }
}
