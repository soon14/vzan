using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 快跑者创建订单表单
    /// </summary>
    [SqlTable(dbEnum.MINIAPP)]
    public class KPZOrder
    {
        public KPZOrder()
        {

        }
        public KPZOrder(int shop_id, string shop_name, string shop_tel, string shop_address, string shop_tag, string order_content, string order_note,string orderno, string customer_name, string customer_tel, string customer_address, string customer_tag, float order_price, float order_origin_price, int pay_status, float pay_fee, int orderid, int aid, int templatetype, int storeid = 0,int ordertype=0)
        {
            this.OrderType = ordertype;
            this.shop_id = shop_id;
            this.shop_name = shop_name;
            this.shop_tel = shop_tel;
            this.shop_address = shop_address;
            this.shop_tag = shop_tag;
            this.order_content = order_content;
            this.order_note = order_note;
            this.order_from = shop_name;
            this.order_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.note = orderno;
            this.customer_name = customer_name;
            this.customer_tel = customer_tel;
            this.customer_address = customer_address;
            this.customer_tag = customer_tag;
            this.order_no = orderno;
            this.OrderId = orderid;
            this.order_price = order_price.ToString();
            this.order_origin_price = order_origin_price.ToString();
            this.pay_status = pay_status;
            this.pay_fee = pay_fee.ToString();
            this.AId = aid;
            this.StoreId = storeid;
            this.TemplateType = templatetype;
        }

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        #region 必填
        /// <summary>
        /// 第三方系统中的商户 ID
        /// </summary>
        [SqlField]
        public int shop_id { get; set; }
        /// <summary>
        /// 第三方系统中的商户名称
        /// </summary>
        [SqlField]
        public string shop_name { get; set; }
        /// <summary>
        /// 第三方系统中的商户电话
        /// </summary>
        [SqlField]
        public string shop_tel { get; set; }
        /// <summary>
        /// 第三方系统中的商户地址
        /// </summary>
        [SqlField]
        public string shop_address { get; set; }
        /// <summary>
        /// 第三方系统中的商户坐标
        /// </summary>
        [SqlField]
        public string shop_tag { get; set; }
        /// <summary>
        /// 订单单号
        /// </summary>
        [SqlField]
        public string order_no { get; set; }
        /// <summary>
        /// 订单支付方式：0 表示已支付、1 表示货到付款
        /// </summary>
        [SqlField]
        public int pay_status { get; set; }
        #endregion

        /// <summary>
        /// 订单订单内容
        /// </summary>
        [SqlField]
        public string order_content { get; set; }
        /// <summary>
        /// 订单备注
        /// </summary>
        [SqlField]
        public string order_note { get; set; }
        /// <summary>
        /// 订单标识
        /// </summary>
        [SqlField]
        public string order_mark { get; set; }
        /// <summary>
        /// 订单来源
        /// </summary>
        [SqlField]
        public string order_from { get; set; }
        /// <summary>
        /// 订单下单时间
        /// </summary>
        [SqlField]
        public string order_time { get; set; }
        /// <summary>
        /// 订单图片路径
        /// </summary>
        [SqlField]
        public string order_photo { get; set; }
        /// <summary>
        /// 自定义参数，快跑者系统调用回调地址时会带上该参数
        /// </summary>
        [SqlField]
        public string note { get; set; }
        /// <summary>
        /// 订单客户对应配送订单的收单人
        /// </summary>
        [SqlField]
        public string customer_name { get; set; }
        /// <summary>
        /// 订单客户电话
        /// </summary>
        [SqlField]
        public string customer_tel { get; set; }
        /// <summary>
        /// 订单客户地址
        /// </summary>
        [SqlField]
        public string customer_address { get; set; }
        /// <summary>
        /// 订单客户坐标火星坐标
        /// </summary>
        [SqlField]
        public string customer_tag { get; set; }
        /// <summary>
        /// 订单的总价
        /// </summary>
        [SqlField]
        public string order_price { get; set; }
        /// <summary>
        /// 订单的原始价格
        /// </summary>
        [SqlField]
        public string order_origin_price { get; set; }
        /// <summary>
        /// 订单的原配送费
        /// </summary>
        [SqlField]
        public string pay_fee { get; set; }
        /// <summary>
        /// 预计送达时间的时间戳（10 位）
        /// </summary>
        [SqlField]
        public string pre_times { get; set; }
        /// <summary>
        /// 配送订单状态：0:待请求,1：待发单，2：待抢单，3：待接单，4：取单中，5：送单中，6：已送达，7：已撤销(KPZOrderEnum)
        /// </summary>
        [SqlField]
        public int status { get; set; } = (int)KPZOrderEnum.待请求;
        /// <summary>
        /// 快跑者创建订单成功后返回的单号，可使用单号查询订单状态及撤销配送订单
        /// </summary>
        [SqlField]
        public string trade_no { get; set; }

        #region 配送员参数
        /// <summary>
        /// 配送员名字
        /// </summary>
        [SqlField]
        public string courier { get; set; }
        /// <summary>
        /// 配送员电话
        /// </summary>
        [SqlField]
        public string tel { get; set; }
        #endregion

        #region 小未科技参数
        /// <summary>
        /// 其他模板订单单号
        /// </summary>
        [SqlField]
        public int OrderId { get; set; }
        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int AId { get; set; }
        /// <summary>
        /// 多门店ID，其他默认0
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        [SqlField]
        public int OrderType { get; set; }
        /// <summary>
        /// 模板类型
        /// </summary>
        [SqlField]
        public int TemplateType { get; set; }
        public string UpdateTime { get; set; }
        #endregion
    }
}
