using Entity.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodAddress
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 餐饮店铺Id
        /// </summary>
        [SqlField]
        public int FoodId { get; set; } = 0;

        /// <summary>
        /// 地址名称
        /// </summary>
        [SqlField]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 省id
        /// </summary>
        [SqlField]
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string CityCode { get; set; } = string.Empty;

        /// <summary>
        /// 区域编码
        /// </summary>
        [SqlField]
        public string AreaCode { get; set; } = string.Empty;

        /// <summary>
        /// 详细地址
        /// </summary>
        [SqlField]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 邮政编码
        /// </summary>
        [SqlField]
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// 收货人姓名
        /// </summary>
        [SqlField]
        public string NickName { get; set; } = string.Empty;

        /// <summary>
        /// 手机号码
        /// </summary>
        [SqlField]
        public string TelePhone { get; set; } = string.Empty;

        /// <summary>
        /// 是否默认地址
        /// </summary>
        [SqlField]
        public int IsDefault { get; set; } = 0;
        //public C_Enums.IsDefault IsDefault { get; set; } = C_Enums.IsDefault.否;

        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        //public C_Enums.AddressState State { get; set; } = C_Enums.AddressState.正常;

        /// <summary>
        /// UserId
        /// </summary>
        [SqlField]
        public int UserId { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;


        public string Address_Dtl { get; set; } = string.Empty;

        /// <summary>
        /// 距离对比地点差值
        /// </summary>
        public double TakeRangedistance { get; set; } = 0.00;


        /// <summary>
        /// 纬度
        /// </summary>
        [SqlField]
        public double Lat { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        [SqlField]
        public double Lng { get; set; }

        /// <summary>
        /// appId
        /// </summary>
        [SqlField]
        public int appId { get; set; }

    }
}
