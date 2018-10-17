using Entity.Base;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 代理信息表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SystemUpdateMessage
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        [SqlField]
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// 更新详情
        /// </summary>
        [SqlField]
        public string Content { get; set; } = string.Empty;


        /// <summary>
        /// 状态：-1：删除，0：正常
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd"); } }
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd"); } }

        /// <summary>
        /// 更新类型，0：系统更新，1：小程序更新，2：用户自己的消息 ,3:预约消息,4:订单消息
        /// </summary>
        [SqlField]
        public int Type { get; set; } = 0;
        /// <summary>
        /// 模板ID
        /// </summary>
        [SqlField]
        public int TId { get; set; } = 0;
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public string AccountId { get; set; } = string.Empty;
        /// <summary>
        /// 发布者
        /// </summary>
        [SqlField]
        public string PublishUser { get; set; } = string.Empty;
        /// <summary>
        /// 更新年份
        /// </summary>
        [SqlField]
        public int Year { get; set; } = 0;
        /// <summary>
        /// 更新月份
        /// </summary>
        [SqlField]
        public int Month { get; set; } = 0;
        /// <summary>
        /// 更新日
        /// </summary>
        [SqlField]
        public int Day { get; set; } = 0;

        /// <summary>
        /// 小程序aid
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 是否已读：0：未读，1：已读
        /// </summary>

        public int IsRead { get; set; } = 0;
        #region 下单消息通知属性
        /// <summary>
        /// 会员名称
        /// </summary>
        public VipRelation userInfo { get; set; }

        /// <summary>
        /// 会员等级名称
        /// </summary>
        public string levelName { get; set; }

        public EntGoodsOrder orderInfo { get; set; }
        public EntUserForm formInfo { get; set; }
        #endregion
    }
}