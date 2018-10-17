using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    /// <summary>
    /// 蜂鸟配送推单表单
    /// </summary>
    public class FNOrder
    {
        public FNOrder()
        {

        }

        public FNOrder(string storecode,int goodcount,string callbackurl,float userpayprice,long addtimestamp,float orderprice,string orderno,string remark,List<elegood> fngoods,FNReceiverInfo reciverinfo, FNStoreInfo storeinfo)
        {
            this.chain_store_code = storecode;
            this.goods_count = goodcount;
            this.items_json = fngoods;
            this.notify_url = callbackurl;
            this.order_actual_amount = userpayprice;
            this.order_add_time = addtimestamp;
            this.order_total_amount = orderprice;
            this.partner_order_code = orderno;
            this.receiver_info = reciverinfo;
            this.transport_info = storeinfo;
            this.order_remark = remark;
        }

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 商户备注信息
        /// </summary>
        [SqlField]
        public string partner_remark { get; set; }
        /// <summary>
        /// 商户订单号，要求唯一（支持数字、大小写字母、#,必填）
        /// </summary>
        [SqlField]
        public string partner_order_code { get; set; }
        /// <summary>
        /// 回调地址,订单状态变更时会调用此接口传递状态信息(必填)
        /// </summary>
        [SqlField]
        public string notify_url { get; set; }
        /// <summary>
        /// 订单类型 1: 蜂鸟配送，支持90分钟内送达(必填)
        /// </summary>
        [SqlField]
        public int order_type { get; set; } = 1;
        /// <summary>
        /// 门店编号（支持数字、字母的组合）
        /// </summary>
        [SqlField]
        public string chain_store_code { get; set; }
        /// <summary>
        /// 门店信息
        /// </summary>
        public FNStoreInfo transport_info { get; set; }
        /// <summary>
        /// 门店信息
        /// </summary>
        [SqlField]
        public string transport_infojson { get; set; }
        /// <summary>
        /// 下单时间(毫秒)
        /// </summary>
        [SqlField]
        public long order_add_time { get; set; }
        /// <summary>
        /// 订单总金额（不包含商家的任何活动以及折扣的金额）(必填)
        /// </summary>
        [SqlField]
        public float order_total_amount { get; set; }
        /// <summary>
        /// 客户需要支付的金额(必填)
        /// </summary>
        [SqlField]
        public float order_actual_amount { get; set; }
        /// <summary>
        /// 订单总重量（kg），营业类型选定为果蔬生鲜、商店超市、其他三类时必填，大于0kg并且小于等于15kg
        /// </summary>
        [SqlField]
        public float order_weight { get; set; } = 1;
        /// <summary>
        /// 用户备注
        /// </summary>
        [SqlField]
        public string order_remark { get; set; }
        /// <summary>
        /// 是否需要发票, 0:不需要, 1:需要(必填)
        /// </summary>
        [SqlField]
        public int is_invoiced { get; set; } = 0;
        /// <summary>
        /// 发票抬头, 如果需要发票, 此项必填
        /// </summary>
        [SqlField]
        public string invoice { get; set; }
        /// <summary>
        /// 订单支付状态 0:未支付 1:已支付(必填)
        /// </summary>
        [SqlField]
        public int order_payment_status { get; set; } = 1;
        /// <summary>
        /// 订单支付方式 1:在线支付(必填)
        /// </summary>
        [SqlField]
        public int order_payment_method { get; set; } = 1;
        /// <summary>
        /// 是否需要ele代收 0:否(必填)
        /// </summary>
        [SqlField]
        public int is_agent_payment { get; set; } = 0;
        /// <summary>
        /// 需要代收时客户应付金额, 如果需要ele代收 此项必填
        /// </summary>
        [SqlField]
        public float require_payment_pay { get; set; }
        /// <summary>
        /// 订单货物件数(必填)
        /// </summary>
        [SqlField]
        public int goods_count { get; set; }
        /// <summary>
        /// 需要送达时间（毫秒).
        /// </summary>
        [SqlField]
        public long require_receive_time { get; set; }
        /// <summary>
        /// 商家订单流水号, 方便配送骑手到店取货, 支持数字,字母及#等常见字符. 如不填写, 蜂鸟将截取商家订单号后4位作为流水号.
        /// </summary>
        [SqlField]
        public string serial_number { get; set; }
        /// <summary>
        /// 收货人信息(必填)
        /// </summary>
        public FNReceiverInfo receiver_info { get; set; }
        /// <summary>
        /// 收货人信息(必填)
        /// </summary>
        [SqlField]
        public string receiver_infojson { get; set; }
        /// <summary>
        /// 商品信息(必填)
        /// </summary>
        public List<elegood> items_json { get; set; }
        /// <summary>
        /// 商品信息(必填)
        /// </summary>
        [SqlField]
        public string items_json_str { get; set; }

        #region 系统字段
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int state { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updatetime{get;set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }
        #endregion

        #region 回调参数值
        /// <summary>
        /// 配送员姓名
        /// </summary>
        [SqlField]
        public string carrier_driver_name { get; set; }
        /// <summary>
        /// 配送员手机
        /// </summary>
        [SqlField]
        public string carrier_driver_phone { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [SqlField]
        public string description { get; set; }
        #endregion

        #region 取消订单返回值
        /// <summary>
        /// 订单取消原因代码(2:商家取消)
        /// </summary>
        [SqlField]
        public int order_cancel_reason_code { get; set; }
        /// <summary>
        /// 订单取消编码（0:其他, 1:联系不上商户, 2:商品已经售完, 3:用户申请取消, 4:运力告知不配送 让取消订单, 5:订单长时间未分配, 6:接单后骑手未取件）
        /// </summary>
        [SqlField]
        public int order_cancel_code { get; set; }
        /// <summary>
        /// 订单取消描述（order_cancel_code为0时必填）
        /// </summary>
        [SqlField]
        public string order_cancel_description { get; set; }
        /// <summary>
        /// 订单取消时间（毫秒）
        /// </summary>
        [SqlField]
        public string order_cancel_time { get; set; }
        #endregion
    }

    public class FNStoreInfo
    {
        /// <summary>
        /// 门店名称（支持汉字、符号、字母的组合），后期此参数将预留另用(必填)
        /// </summary>
        public string transport_name { get; set; }
        /// <summary>
        /// 取货点地址，后期此参数将预留另用(必填)
        /// </summary>
        public string transport_address { get; set; }
        /// <summary>
        /// 取货点经度，取值范围0～180，后期此参数将预留另用(必填)
        /// </summary>
        public double transport_longitude { get; set; }
        /// <summary>
        /// 取货点纬度，取值范围0～90，后期此参数将预留另用(必填)
        /// </summary>
        public double transport_latitude { get; set; }
        /// <summary>
        /// 取货点经纬度来源, 1:腾讯地图, 2:百度地图, 3:高德地图(必填)
        /// </summary>
        public int position_source { get; set; } = 1;
        /// <summary>
        /// 取货点联系方式, 只支持手机号,400开头电话以及座机号码(必填)
        /// </summary>
        public string transport_tel { get; set; }
        /// <summary>
        /// 取货点备注
        /// </summary>
        public string transport_remark { get; set; }
        
    }

    public class FNReceiverInfo
    {
        /// <summary>
        /// 收货人姓名(必填)
        /// </summary>
        public string receiver_name { get; set; }
        /// <summary>
        /// 收货人联系方式，只支持手机号，400开头电话，座机号码以及95013开头、长度13位的虚拟电话(必填)
        /// </summary>
        public string receiver_primary_phone { get; set; }
        /// <summary>
        /// 收货人备用联系方式，只支持手机号，400开头电话，座机号码以及95013开头、长度13位的虚拟电话
        /// </summary>
        public string receiver_second_phone { get; set; }
        /// <summary>
        /// 收货人地址(必填)
        /// </summary>
        public string receiver_address { get; set; }
        /// <summary>
        /// 收货人经度，取值范围0～180(必填)
        /// </summary>
        public double receiver_longitude { get; set; }
        /// <summary>
        /// 收货人纬度，取值范围0～90(必填)
        /// </summary>
        public double receiver_latitude { get; set; }
        /// <summary>
        /// 收货人经纬度来源, 1:腾讯地图, 2:百度地图, 3:高德地图(必填)
        /// </summary>
        public int position_source { get; set; } = 1;
    }
    public class elegood
    {
        /// <summary>
        /// 商品编号
        /// </summary>
        public string item_id { get; set; }
        /// <summary>
        /// 商品名称(必填)
        /// </summary>
        public string item_name { get; set; }
        /// <summary>
        /// 商品数量(必填)
        /// </summary>
        public int item_quantity { get; set; }
        /// <summary>
        /// 商品原价(必填)
        /// </summary>
        public float item_price { get; set; }
        /// <summary>
        /// 商品实际支付金额(必填)
        /// </summary>
        public float item_actual_price { get; set; }
        /// <summary>
        /// 商品尺寸
        /// </summary>
        public int item_size { get; set; }
        /// <summary>
        /// 商品备注
        /// </summary>
        public string item_remark { get; set; }
        /// <summary>
        /// 是否需要ele打包 0:否 1:是(必填)
        /// </summary>
        public int is_need_package { get; set; } = 0;
        /// <summary>
        /// 是否代购 0:否(必填)
        /// </summary>
        public int is_agent_purchase { get; set; } = 0;
        /// <summary>
        /// 代购进价, 如果需要代购 此项必填
        /// </summary>
        public float agent_purchase_price { get; set; }
    }
}
