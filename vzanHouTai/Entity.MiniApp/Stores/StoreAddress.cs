using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Entity.MiniApp;

namespace Entity.MiniApp.Stores
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class StoreAddress
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

        /// <summary>
        /// 地址名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 省id
        /// </summary>
        [SqlField]
        public string Province { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string CityCode { get; set; }

        /// <summary>
        /// 区域编码
        /// </summary>
        [SqlField]
        public string AreaCode { get; set; }

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
        public string NickName { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [SqlField]
        public string TelePhone { get; set; }

        /// <summary>
        /// 是否默认地址
        /// </summary>
        [SqlField]
        public int IsDefault { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public  int State { get; set; } =  0;

        /// <summary>
        /// UserId
        /// </summary>
        [SqlField]
        public int UserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;


        public string Address_Dtl { get; set; } = string.Empty;
    }
}
