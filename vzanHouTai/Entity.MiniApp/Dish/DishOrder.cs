using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Entity.Base;
using Utility;
using Newtonsoft.Json;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 订单
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishOrder
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 订单编号
        /// </summary>
        [SqlField]
        public string order_sn { get; set; } = string.Empty;


        /// <summary>
        /// 用户编号
        /// </summary>
        [SqlField]
        public int user_id { get; set; } = 0;

        /// <summary>
        /// 用户微信昵称
        /// </summary>
        [SqlField]
        public string user_name { get; set; } = string.Empty;

        /// <summary>
        /// 订单状态 枚举:DishEnums.OrderState
        /// </summary>
        [SqlField]
        public int order_status { get; set; } = 0;

        /// <summary>
        /// 订单类型 枚举:DishEnums.OrderType
        /// </summary>
        [SqlField]
        public int order_type { get; set; } = 0;

        /// <summary>
        /// 订单类型文案
        /// </summary>
        public string order_type_txt
        {
            get
            {
                if (order_type > 0)
                {
                    if (order_type == (int)DishEnums.OrderType.店内 && is_ziqu == 1) return "店内自提";
                    return Enum.GetName(typeof(DishEnums.OrderType), order_type);
                }
                else
                {
                    return "-";
                }
            }
        }

        /// <summary>
        /// 订单就餐类型  店内单据有2种:1.先付款后就餐,2.先就餐后付款
        /// </summary>
        [SqlField]
        public int order_jiucan_type { get; set; } = 0;

        /// <summary>
        /// 运送状态
        /// </summary>
        [SqlField]
        public int shipping_status { get; set; } = 0;

        /// <summary>
        /// 支付状态 枚举:DishEnums.PayState
        /// </summary>
        [SqlField]
        public int pay_status { get; set; } = 0;

        /// <summary>
        /// 支付状态文案 -!db
        /// </summary>
        public string pay_status_txt
        {
            get
            {
                return Enum.GetName(typeof(DishEnums.PayState), pay_status);
            }
        }

        /// <summary>
        /// 客户给商家的备注
        /// </summary>
        [SqlField]
        public string post_info { get; set; } = string.Empty;

        /// <summary>
        /// 用餐人数
        /// </summary>
        [SqlField]
        public string yongcan_renshu { get; set; } = string.Empty;

        /// <summary>
        /// 配送方式
        /// </summary>
        [SqlField]
        public int shipping_id { get; set; } = 0;

        /// <summary>
        /// 配送方式文案 -!db
        /// </summary>
        public string shipping_name { get; set; } = string.Empty;

        /// <summary>
        /// 支付类型 枚举：DishEnums.PayMode
        /// </summary>
        [SqlField]
        public int pay_id { get; set; } = 0;

        /// <summary>
        /// 支付方式 -!db
        /// </summary>
        public string pay_name
        {
            get
            {
                if (pay_id > 0)
                {
                    return Enum.GetName(typeof(DishEnums.PayMode), pay_id);
                }
                else
                {
                    return "未支付";
                }
            }
        }

        /// <summary>
        /// 商品总金额
        /// </summary>
        [SqlField]
        public double goods_amount { get; set; } = 0.00;

        /// <summary>
        /// 配送费/运费
        /// </summary>
        [SqlField]
        public double shipping_fee { get; set; } = 0.00;

        /// <summary>
        /// 打包费
        /// </summary>
        [SqlField]
        public double dabao_fee { get; set; } = 0.00;

        /// <summary>
        /// 订单金额
        /// </summary>
        [SqlField]
        public double order_amount { get; set; } = 0.00;

        /// <summary>
        /// 订单金额_old<未知作用>
        /// </summary>
        [SqlField]
        public double order_amount_old { get; set; } = 0.00;

        /// <summary>
        /// 订单最后一次添加菜品的时间
        /// </summary>
        [SqlField]
        public DateTime add_time { get; set; } = DateTime.Now;


        public string add_time_txt
        {
            get
            {
                if (add_time != null)
                {
                    return add_time.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 确认时间
        /// </summary>
        [SqlField]
        public DateTime confirm_time { get; set; } = Convert.ToDateTime("0001-01-01 00:00:00");

        /// <summary>
        /// 支付时间 若商家手动修改,那么这个栏位不会改值,用于区分标识订单是否真的付了款
        /// </summary>
        [SqlField]
        public DateTime pay_time { get; set; } = Convert.ToDateTime("0001-01-01 00:00:00");

        /// <summary>
        /// 支付时间 格式化string -!db
        /// </summary>
        public string pay_time_txt
        {
            get
            {
                if (pay_time != null)
                {
                    return pay_time.ToString("yyyy-MM-dd HH:mm:ss");
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 最晚支付时间
        /// </summary>
        [SqlField]
        public DateTime pay_end_time { get; set; } = Convert.ToDateTime("0001-01-01 00:00:00");

        /// <summary>
        /// 配送时间
        /// </summary>
        [SqlField]
        public DateTime shipping_time { get; set; } = Convert.ToDateTime("0001-01-01 00:00:00");

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string express_name { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string express_code { get; set; } = string.Empty;

        /// <summary>
        /// 是否已评论
        /// </summary>
        [SqlField]
        public int is_comment { get; set; } = 0;

        /// <summary>
        /// 是否要求开发票
        /// </summary>
        [SqlField]
        public int is_fapiao { get; set; } = 0;

        /// <summary>
        /// 是否已通知<模板消息>
        /// </summary>
        [SqlField]
        public int is_tongzhi { get; set; } = 0;


        /// <summary>
        /// 订单排号 (今天的第几单)
        /// </summary>
        [SqlField]
        public string order_haoma { get; set; } = string.Empty;

        /// <summary>
        /// 桌号文案
        /// </summary>
        [SqlField]
        public string order_table_id { get; set; } = string.Empty;

        /// <summary>
        /// 活动:首单立减金额
        /// </summary>
        [SqlField]
        public double huodong_shou_jiner { get; set; } = 0;

        /// <summary>
        /// 活动, 优惠券id
        /// </summary>
        [SqlField]
        public int huodong_quan_id { get; set; } = 0;

        /// <summary>
        /// 活动, 优惠金额
        /// </summary>
        [SqlField]
        public double huodong_quan_jiner { get; set; } = 0.00;

        /// <summary>
        /// 满减ID
        /// </summary>
        [SqlField]
        public int huodong_manjian_id { get; set; } = 0;

        /// <summary>
        /// 活动, 满减金额
        /// </summary>
        [SqlField]
        public double huodong_manjin_jiner { get; set; } = 0.00;

        /// <summary>
        /// 优惠券类型 0:优惠券 1：立减金
        /// </summary>
        [SqlField]
        public int coupon_type { get; set; } = 0;

        /// <summary>
        /// 记录是否删除
        /// </summary>
        [SqlField]
        public int is_delete { get; set; } = 0;

        /// <summary>
        /// 现金??
        /// </summary>
        [SqlField]
        public int is_auto_cash { get; set; } = 0;

        /// <summary>
        /// 开始有配送员跟进此单(取货/送货)
        /// </summary>
        [SqlField]
        public int peisong_open { get; set; } = 0;


        /// <summary>
        /// 配送类型
        /// </summary>
        [SqlField]
        public int peisong_type { get; set; } = 0;
        public string PeiSongName { get { return Enum.GetName(typeof(miniAppOrderGetWay), peisong_type); } }

        /// <summary>
        /// 配送状态 枚举:DeliveryState
        /// </summary>
        [SqlField]
        public int peisong_status { get; set; } = 0;

        /// <summary>
        /// 配送人名称
        /// </summary>
        [SqlField]
        public string peisong_user_name { get; set; } = string.Empty;

        /// <summary>
        /// 配送人电话
        /// </summary>
        [SqlField]
        public string peisong_user_phone { get; set; } = string.Empty;

        /// <summary>
        /// 配送费
        /// </summary>
        [SqlField]
        public double peisong_amount { get; set; } = 0.00f;

        /// <summary>
        /// 是否自取 1是0不是
        /// </summary>
        [SqlField]
        public int is_ziqu { get; set; } = 0;


        /// <summary>
        /// 自取用户名
        /// </summary>
        [SqlField]
        public string ziqu_username { get; set; } = string.Empty;

        /// <summary>
        /// 联系电话
        /// </summary>
        [SqlField]
        public string ziqu_userphone { get; set; } = string.Empty;

        /// <summary>
        /// 取货时间
        /// </summary>
        [SqlField]
        public string ziqu_time { get; set; } = string.Empty;

        /// <summary>
        /// 最终金额
        /// </summary>
        [SqlField]
        public double settlement_total_fee { get; set; } = 0.00;

        /// <summary>
        /// 桌号号
        /// </summary>
        [SqlField]
        public int order_table_id_zhen { get; set; } = 0;


        /// <summary>
        /// 订单支付提示文案, eg:请在15:04之前完成支付，超时订单将自动取消
        /// </summary>
        public string pay_end_time_text
        {
            get
            {
                return $"请在 {pay_end_time.ToString("HH:mm")} 之前完成支付，超时订单将自动取消";
            }
        }

        /// <summary>
        /// 订单的创建时间
        /// </summary>
        [SqlField]
        public DateTime ctime { get; set; } = DateTime.Now;

        /// <summary>
        /// 订单状态文案 -!db
        /// </summary>
        public string order_status_txt
        {
            get
            {
                return Enum.GetName(typeof(DishEnums.OrderState), order_status);
            }
        }

        /// <summary>
        /// 配送文案 -!db
        /// </summary>
        public string peisong_name { get; set; } = string.Empty;

        /// <summary>
        /// 配送状态文案 -!db
        /// </summary>
        public string peisong_status_text
        {
            get
            {
                return Enum.GetName(typeof(DishEnums.DeliveryState), peisong_status);
            }
        }

        /// <summary>
        /// 发票类型文案
        /// </summary>
        [SqlField]
        public string fapiao_leixing_txt { get; set; } = string.Empty;

        /// <summary>
        /// 发票抬头
        /// </summary>
        [SqlField]
        public string fapiao_text { get; set; } = string.Empty;

        /// <summary>
        /// 发票税号
        /// </summary>
        [SqlField]
        public string fapiao_no { get; set; } = string.Empty;

        /// <summary>
        /// 收货人姓名
        /// </summary>
        [SqlField]
        public string consignee { get; set; } = string.Empty;

        /// <summary>
        /// 收货人手机号码
        /// </summary>
        [SqlField]
        public string mobile { get; set; } = string.Empty;

        /// <summary>
        /// 国家
        /// </summary>
        [SqlField]
        public string country { get; set; } = string.Empty;

        /// <summary>
        /// 省份
        /// </summary>
        [SqlField]
        public string province { get; set; } = string.Empty;

        /// <summary>
        /// 城市
        /// </summary>
        [SqlField]
        public string city { get; set; } = string.Empty;

        /// <summary>
        /// 区域
        /// </summary>
        [SqlField]
        public string area { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>
        [SqlField]
        public string address { get; set; } = string.Empty;

        /// <summary>
        /// 编码
        /// </summary>
        [SqlField]
        public string zipcode { get; set; } = string.Empty;


        /// <summary>
        /// 邮件
        /// </summary>
        [SqlField]
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// 订单坐标
        /// </summary>
        [SqlField]
        public double u_lat { get; set; } = 0.00;

        /// <summary>
        /// 订单坐标
        /// </summary>
        [SqlField]
        public double u_lng { get; set; } = 0.00;

        /// <summary>
        /// 购物车记录
        /// </summary>
        public List<DishShoppingCart> carts = new List<DishShoppingCart>();

        /// <summary>
        /// cityMorders表ID
        /// </summary>
        [SqlField]
        public int cityMordersId { get; set; } = 0;


        /// <summary>
        /// 已经退款的金额
        /// </summary>
        [SqlField]
        public double refundMoney { get; set; } = 0.00;

        [SqlField]
        public string attrbute { get; set;}
        public DishOrderAttrbute GetAttrbute()
        {
            if (string.IsNullOrWhiteSpace(attrbute))
            {
                return new DishOrderAttrbute();
            }
            try
            {
                return JsonConvert.DeserializeObject<DishOrderAttrbute>(attrbute);
            }
            catch
            {
                return new DishOrderAttrbute();
            }
        }

        /// <summary>
        /// 退款原因
        /// </summary>
        [SqlField]
        public string refundReason { get; set; } = string.Empty;
    }

    public class DishOrderAttrbute
    {
        /// <summary>
        /// 订单备注
        /// </summary>
        public string mark { get; set; }
    }

    /// <summary>
    /// 此结构仅用于小程序端提交订单
    /// </summary>
    public class PostOrderInfo
    {
        /// <summary>
        /// 桌台号
        /// </summary>
        public int dish_table_id { get; set; } 

        /// <summary>
        /// 门店ID
        /// </summary>
        public int dish_id { get; set; }

        /// <summary>
        /// 订单类型 枚举 DishEnums.OrderType
        /// </summary>
        public int order_type { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        public int order_id { get; set; }

        /// <summary>
        /// 自取
        /// </summary>
        public int is_ziqu { get; set; }

        /// <summary>
        /// 自取时间文案
        /// </summary>
        public string ziqu_time { get; set; }

        /// <summary>
        /// 优惠券ID
        /// </summary>
        public int quan_id { get; set; }

        /// <summary>
        /// 满减Id
        /// </summary>
        public int manjian_id { get; set; }

        /// <summary>
        /// 是否满足首单立减,满足传1,否则为0
        /// </summary>
        public int shou_id { get; set; }

        /// <summary>
        /// 订单资料
        /// </summary>
        public postOrderAddress wx_address { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string this_beizhu_info { get; set; }

        /// <summary>
        /// 用餐人数
        /// </summary>
        public string this_yongcan_renshu { get; set; }

        /// <summary>
        /// 使用的发票ID
        /// </summary>
        public int this_fapiao_id { get; set; }

        /// <summary>
        /// 自取人姓名
        /// </summary>
        public string ziqu_username { get; set; }

        /// <summary>
        /// 自取人电话
        /// </summary>
        public string ziqu_userphone { get; set; }
        /// <summary>
        /// 判断优惠券是否立减金
        /// </summary>
        public bool isReductionCard { get; set; } = false;
    }

    /// <summary>
    /// 结构仅用于小程序端提交订单方便处理数据
    /// </summary>
    public class postOrderAddress
    {
        /// <summary>
        /// 收件人姓名
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string telNumber { get; set; }

        /// <summary>
        /// 收货地址详情
        /// </summary>
        public string detailInfo { get; set; }

        /// <summary>
        /// 坐标
        /// </summary>
        public double u_lat { get; set; }

        /// <summary>
        /// 坐标
        /// </summary>
        public double u_lng { get; set; }
    }
}
