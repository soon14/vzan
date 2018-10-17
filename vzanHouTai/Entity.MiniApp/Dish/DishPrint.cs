using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 标签
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishPrint
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 状态 1=正常，0=禁用， -1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 平台用户名
        /// </summary>
        [SqlField]
        public string platform_userName { get; set; } = string.Empty;
        /// <summary>
        /// 平台用户id
        /// </summary>
        [SqlField]
        public int platform_userId { get; set; } = 0;

        /// <summary>
        /// api密钥
        /// </summary>
        [SqlField]
        public string apiPrivateKey { get; set; } = string.Empty;
        /// <summary>
        /// 打印机名称
        /// </summary>
        [SqlField]
        public string print_name { get; set; } = string.Empty;

        /// <summary>
        /// 打印方式  1.无需支付下单打印 2.支付后打印
        /// </summary>
        [SqlField]
        public int print_type { get; set; } = 2;

        /// <summary>
        /// 打印机类型  默认0为易联云打印机
        /// </summary>
        [SqlField]
        public int print_name_type { get; set; } =0;

        /// <summary>
        /// 打印机设备编码
        /// </summary>
        [SqlField]
        public string print_bianma { get; set; } = string.Empty;

        /// <summary>
        /// 打印机识别码
        /// </summary>
        [SqlField]
        public string print_shibiema { get; set; } = string.Empty;

        /// <summary>
        /// 是否为整单打印 1=整单，2=分单
        /// </summary>
        [SqlField]
        public int print_d_type { get; set; } = 1;

        /// <summary>
        /// 打印联数
        /// </summary>
        [SqlField]
        public int print_dnum { get; set; } = 1;

        /// <summary>
        /// 可打印的标签
        /// </summary>
        [SqlField]
        public string print_tags { get; set; } = string.Empty;
        public string tags { get; set; } = string.Empty;
        /// <summary>
        /// 字体大小 0=小，1=大
        /// </summary>
        [SqlField]
        public int print_ziti_type { get; set; } =0 ;

        /// <summary>
        /// 商品字体显示大小 0=默认，1=超大
        /// </summary>
        [SqlField]
        public int print_goods_ziti_type { get; set; } = 0;

        /// <summary>
        /// 头部自定义信息
        /// </summary>
        [SqlField]
        public string print_top_copy { get; set; } = string.Empty;

        /// <summary>
        /// 底部自定义信息
        /// </summary>
        [SqlField]
        public string print_bottom_copy { get; set; } = string.Empty;
    }
}
