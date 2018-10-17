using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Footbath
{
    /// <summary>
    /// 技师服务时间被选表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ServiceTime
    {
        /// <summary>
        /// 自增id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 小程序id
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;

        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 技师id
        /// </summary>
        [SqlField]
        public int tid { get; set; } = 0;

        /// <summary>
        /// 已选服务日期
        /// </summary>
        [SqlField]
        public DateTime date { get; set; }

        public string datestr
        {
            get
            {
                return date.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 已选服务时间
        /// </summary>
        [SqlField]
        public string time { get; set; } = string.Empty;

        [SqlField]
        public DateTime createTime { get; set; }
        public string createTimeStr
        {
            get
            {
                return createTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}
