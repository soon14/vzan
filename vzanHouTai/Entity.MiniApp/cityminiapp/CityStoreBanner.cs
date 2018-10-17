using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.cityminiapp
{
    /// <summary>
    /// 同城版-商家配置轮播图
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
   public class CityStoreBanner
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int aid { get; set; }
        /// <summary>
        /// 轮播图路径逗号分隔
        /// </summary>
        [SqlField]
        public string banners { get; set; } = string.Empty;


        /// <summary>
        /// 轮播图跳转到的帖子Id逗号分隔
        /// </summary>
        [SqlField]
        public string MsgIds { get; set; } = string.Empty;

        /// <summary>
        /// 审核设置 发帖时是否需要审核
        /// 0 → 不需要审核  1 →先审核后发布 2 →先发布后审核
        /// </summary>
        [SqlField]
        public int ReviewSetting { get; set; } = 0;//不需要审核 默认


        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }


        /// <summary>
        /// 客服联系号码
        /// </summary>
        [SqlField]
        public string KeFuPhone { get; set; }


        /// <summary>
        /// 公告
        /// </summary>
        [SqlField]
        public string Remark { get; set; }

        /// <summary>
        /// 是否弹窗
        /// </summary>
        [SqlField]
        public int RemarkOpenFrm { get; set; }

        /// <summary>
        /// 真实浏览量
        /// </summary>
        [SqlField]
        public int PV { get; set; }


        /// <summary>
        /// 虚拟浏览量
        /// </summary>
        [SqlField]
        public int VirtualPV { get; set; }

        /// <summary>
        /// 虚拟发帖量
        /// </summary>
        [SqlField]
        public int VirtualMsgCount { get; set; }

        public int MsgCount { get; set; }


    }
}
