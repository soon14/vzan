using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    [SqlTable(dbEnum.MINIAPP)]
    /// <summary>
    /// 新增订单表单
    /// </summary>
    public class DadaOrder
    {
        public DadaOrder()
        {

        }
        public DadaOrder(string order_id, string citycode, string shopno, float price, string receivername, string receivertel, string address, double lat, double lnt, string desc, string timestamp, string callback)
        {
            this.shop_no = shopno;
            this.origin_id = order_id;
            this.city_code = citycode;
            this.cargo_price = price;
            this.expected_fetch_time = timestamp;
            this.receiver_name = receivername;
            this.receiver_address = address;
            this.receiver_lat = lat;
            this.receiver_lng = lnt;
            this.callback = callback;
            this.receiver_phone = receivertel;
            this.info = desc;
        }

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        #region 必填
        /// <summary>
        /// 门店编号，门店创建后可在门店列表和单页查看
        /// </summary>
        [SqlField]
        public string shop_no { get; set; } = string.Empty;
        /// <summary>
        /// 第三方订单ID
        /// </summary>
        [SqlField]
        public string origin_id { get; set; } = string.Empty;
        /// <summary>
        /// 订单所在城市的code（查看各城市对应的code值）
        /// </summary>
        [SqlField]
        public string city_code { get; set; } = string.Empty;
        /// <summary>
        /// 订单金额
        /// </summary>
        [SqlField]
        public float cargo_price { get; set; }
        /// <summary>
        /// 是否需要垫付 1:是 0:否 (垫付订单金额，非运费)
        /// </summary>
        [SqlField]
        public int is_prepay { get; set; } = 0;
        /// <summary>
        /// 期望取货时间（1.时间戳,以秒计算时间，即unix-timestamp; 2.该字段的设定，不会影响达达正常取货; 3.订单待接单时,该时间往后推半小时后，会自动被系统取消;4.建议取值为当前时间往后推10~15分钟）
        /// </summary>
        [SqlField]
        public string expected_fetch_time { get; set; } = string.Empty;
        /// <summary>
        /// 收货人姓名
        /// </summary>
        [SqlField]
        public string receiver_name { get; set; } = string.Empty;
        /// <summary>
        /// 收货人地址
        /// </summary>
        [SqlField]
        public string receiver_address { get; set; } = string.Empty;
        /// <summary>
        /// 收货人地址维度（高德坐标系）
        /// </summary>
        [SqlField]
        public double receiver_lat { get; set; }
        /// <summary>
        /// 收货人地址经度（高德坐标系）
        /// </summary>
        [SqlField]
        public double receiver_lng { get; set; }
        /// <summary>
        /// 回调URL（查看回调说明）
        /// </summary>
        [SqlField]
        public string callback { get; set; } = string.Empty;
        /// <summary>
        /// 门店编号，门店创建后可在门店列表和单页查看商品保价费(当商品出现损坏，可获取一定金额的赔付)
        /// 保价费分三挡：分别为1元，3元，5元。
        /// 1元保价：最高可获取100元赔付。
        /// 3元保价：最高可获取300元赔付。
        /// 5元保价：最高可获取1000元赔付。
        /// </summary>
        [SqlField]
        public int insurance_fee { get; set; } = 0;
        #endregion


        /// <summary>
        /// 收货人手机号（手机号和座机号必填一项）
        /// </summary>
        [SqlField]
        public string receiver_phone { get; set; } = string.Empty;
        /// <summary>
        /// 收货人座机号（手机号和座机号必填一项）
        /// </summary>
        [SqlField]
        public string receiver_tel { get; set; } = string.Empty;
        /// <summary>
        /// 小费（单位：元，精确小数点后一位）
        /// </summary>
        [SqlField]
        public double tips { get; set; }
        /// <summary>
        /// 订单备注
        /// </summary>
        [SqlField]
        public string info { get; set; } = string.Empty;
        /// <summary>
        /// 订单商品类型：食品小吃-1,饮料-2,鲜花-3,文印票务-8,便利店-9,水果生鲜-13,同城电商-19, 医药-20,蛋糕-21,酒品-24,小商品市场-25,服装-26,汽修零配-27,数码-28,小龙虾-29, 其他-5
        /// </summary>
        [SqlField]
        public int cargo_type { get; set; }
        /// <summary>
        /// 订单重量（单位：Kg）
        /// </summary>
        [SqlField]
        public double cargo_weight { get; set; }
        /// <summary>
        /// 订单商品数量
        /// </summary>
        [SqlField]
        public int cargo_num { get; set; }
        /// <summary>
        /// 发票抬头
        /// </summary>
        [SqlField]
        public string invoice_title { get; set; } = string.Empty;
        /// <summary>
        /// 送货开箱码
        /// </summary>
        [SqlField]
        public string deliver_locker_code { get; set; } = string.Empty;
        /// <summary>
        /// 取货开箱码
        /// </summary>
        [SqlField]
        public string pickup_locker_code { get; set; } = string.Empty;
        /// <summary>
        /// 订单来源标示（该字段可以显示在达达app订单详情页面，只支持字母，最大长度为10）
        /// </summary>
        [SqlField]
        public string origin_mark { get; set; } = string.Empty;
        /// <summary>
        /// 订单来源编号（该字段可以显示在达达app订单详情页面，支持字母和数字，最大长度为30）
        /// </summary>
        [SqlField]
        public string origin_mark_no { get; set; } = string.Empty;
        /// <summary>
        /// 收货码（0：不需要；1：需要。收货码的作用是：骑手必须输入收货码才能完成订单妥投）
        /// </summary>
        [SqlField]
        public int is_finish_code_needed { get; set; }
        /// <summary>
        /// 预约发单时间（预约时间unix时间戳(10位),精确到分;整10分钟为间隔，并且需要至少提前20分钟预约。）
        /// </summary>
        [SqlField]
        public int delay_publish_time { get; set; }

        /// <summary>
        /// 订单状态(0待支付（达达没有该状态），待接单＝1 待取货＝2 配送中＝3 已完成＝4 已取消＝5 已过期＝7 指派单=8 妥投异常之物品返回中=9 
        /// 妥投异常之物品返回完成=10 系统故障订单发布失败=1000 可参考文末的状态说明）(来自枚举：DadaOrderEnum)
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        #region 回调参数
        /// <summary>
        /// 订单取消原因来源(1:达达配送员取消；2:商家主动取消；3:系统或客服取消；0:默认值)
        /// </summary>
        [SqlField]
        public int cancel_from { get; set; }
        /// <summary>
        /// 订单取消原因,其他状态下默认值为空字符串
        /// </summary>
        [SqlField]
        public string cancel_reason { get; set; } = string.Empty;
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime update_time { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 达达配送员id，接单以后会传
        /// </summary>
        [SqlField]
        public int dm_id { get; set; }
        /// <summary>
        /// 配送员姓名，接单以后会传
        /// </summary>
        [SqlField]
        public string dm_name { get; set; } = string.Empty;
        /// <summary>
        /// 配送员手机号，接单以后会传
        /// </summary>
        [SqlField]
        public string dm_mobile { get; set; } = string.Empty;
        #endregion
    }
}
