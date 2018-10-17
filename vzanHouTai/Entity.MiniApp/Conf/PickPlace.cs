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
    /// 自提地点配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PickPlace
    {
        /// <summary>
        /// 唯一标识符，自增
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序id
        /// </summary>
        [SqlField]
        public int aid { get; set; }

        /// <summary>
        /// 门店id
        /// </summary>
        [SqlField]
        public int storeId { get; set; }

        /// <summary>
        /// 自提地点名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 自提地点logo
        /// </summary>
        [SqlField]
        public string logo { get; set; } = string.Empty;

        /// <summary>
        /// 自提地点地址
        /// </summary>
        [SqlField]
        public string address { get; set; } = string.Empty;

        /// <summary>
        /// 纬度
        /// </summary>
        [SqlField]
        public double lat { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        [SqlField]
        public double lng { get; set; }

        /// <summary>
        /// 状态 0：正常   -1：删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }


        /// <summary>
        /// 距离
        /// </summary>
        public string DistanceStr { get; set; } = "0.00";

    }
}