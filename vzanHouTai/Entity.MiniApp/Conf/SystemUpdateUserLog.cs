using Entity.Base;
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
    public class SystemUpdateUserLog
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
        public DateTime AddTime { get; set; } = DateTime.Now;
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd"); } }
        /// <summary>
        /// 更新详情
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd"); } }
        /// <summary>
        /// 状态，-1：删除，0：正常，1：已阅读
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 用户accountId
        /// </summary>
        [SqlField]
        public string AccountId { get; set; } = string.Empty;
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public int UpdateMessageId { get; set; } =0;
    }
}