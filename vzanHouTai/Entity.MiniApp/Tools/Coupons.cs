using Entity.Base;
using Entity.MiniApp.Stores;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Entity.MiniApp.Conf;

namespace Entity.MiniApp.Tools
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Coupons
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 店铺ID
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;
        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;
        /// <summary>
        /// 优惠券名称
        /// </summary>
        [SqlField]
        public string CouponName { set; get; } = string.Empty;
        /// <summary>
        /// 生成数量
        /// </summary>
        [SqlField]
        public int CreateNum { get; set; } = 0;
        /// <summary>
        /// 库存
        /// </summary>
        public int RemNum { get; set; } = 0;
        /// <summary>
        /// 优惠方式(0:指定金额，1：折扣)
        /// </summary>
        [SqlField]
        public int CouponWay { get; set; } = 0;
        // <summary>
        /// 指定金额或折扣（折扣最低为1折，最高为9.9折）
        /// </summary>
        [SqlField]
        public int Money { set; get; } = 0;
        public string MoneyStr {
            get { return (Money / 100.00).ToString(CouponWay==0?"F2":""); }
        }
        /// <summary>
        /// 限领几张
        /// </summary>
        [SqlField]
        public int LimitReceive { set; get; } = 0;
        /// <summary>
        /// 使用门槛（0：不限，大于0：满几元可使用）
        /// </summary>
        [SqlField]
        public int LimitMoney { get; set; } =0;
        public string LimitMoneyStr
        {
            get { return (LimitMoney / 100.00).ToString("F2"); }
        }
        public int LimitMoneyType { get; set; } = 1;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 有效日期开始
        /// </summary>
        [SqlField]
        public DateTime StartUseTime { set; get; }
        public string StartUseTimeStr { get { return StartUseTime.ToString("yyyy.MM.dd HH:mm:ss"); } }
        /// <summary>
        /// 有效日期结束
        /// </summary>
        [SqlField]
        public DateTime EndUseTime { set; get; }
        public string EndUseTimeStr { get { return EndUseTime.ToString("yyyy.MM.dd HH:mm:ss"); } }
        /// <summary>
        /// 有效期方式（0：固定日期，1：领到券次日开始N天内有效，2：领到券当日开始N天内有效
        /// </summary>
        [SqlField]
        public int ValType { get; set; } = 0;
        /// <summary>
        /// N天内有效
        /// </summary>
        [SqlField]
        public int ValDay { get; set; } = 0;
        /// <summary>
        /// 状态：默认0 枚举CouponState
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        public string StateStr { get; set; } = string.Empty;
        /// <summary>
        /// 使用说明
        /// </summary>
        [SqlField]
        public string Desc { get; set; } = string.Empty;
        /// <summary>
        /// 可使用商品ID字符串
        /// </summary>
        [SqlField]
        public string GoodsIdStr { get; set; } = string.Empty;
        public int GoodsType { get { return string.IsNullOrEmpty(GoodsIdStr) ? 0 : 1; } }
        /// <summary>
        /// 限制哪些会员等级可领取（-1：所有会员，大于等于0：对应相应会员等级）
        /// </summary>
        [SqlField]
        public int VipLevel { get; set; } = -1;
        /// <summary>
        /// 票券类型 默认为优惠券 详见枚举类TicketType
        /// </summary>
        [SqlField]
        public int TicketType { get; set; } = 0;

        /// <summary>
        /// 满足领取条件人数，限定领取的份数
        /// </summary>
        [SqlField]
        public int SatisfyNum { get; set; } = 2;
        /// <summary>
        /// 领取人数
        /// </summary>
        public int PersonNum { get; set; } = 0;
        /// <summary>
        /// 领取优惠券数量
        /// </summary>
        public int CouponNum { get; set; } = 0;
        /// <summary>
        /// 优惠券已使用数量
        /// </summary>
        public int UseNum { get; set; } = 0;
        /// <summary>
        /// 是否还能再领取优惠券
        /// </summary>
        public bool CanGet { get; set; } = true;

        public List<object> SelectGoods { get; set; }
        /// <summary>
        /// 领取的立减金来自哪个订单
        /// </summary>
        public int fromOrderId { get; set; } = 0;

        /// <summary>
        /// 是否首页弹窗提示  每个小程序只有三张可以弹窗提示
        /// </summary>
        [SqlField]
        public int IsShowTip { get; set; } = 0;

        /// <summary>
        /// 已设置为首页弹窗提示优惠券数量
        /// </summary>
        /// 
        public int showTipCount { get; set; } = 0;


        /// <summary>
        /// 是否与会员折扣互斥 0默认不互斥 1互斥
        /// </summary>
        [SqlField]
        public int discountType { get; set; } = 0;



        /// <summary>
        /// 微信优惠券ID
        /// </summary>
        [SqlField]
        public string WxCouponsCardId { get; set; }

        /// <summary>
        /// 是否同步开启到微信卡包 0→关闭 1→开启
        /// </summary>
        [SqlField]
        public int WxCouponsCardOpen { get; set; } = 0;

        /// <summary>
        /// 同步到微信卡包处理结果
        /// </summary>
        [SqlField]
        public string WxCouponsCardOpenResult { get; set; }

    }

    public class PlatCoupons
    {
        public string StoreLogo { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int AId { get; set; }
        public List<CouponLog> couponloglist { get; set; }

        public List<Coupons> couponList { get; set; }
    }


    /// <summary>
    /// 微信卡包里的折扣优惠劵  CouponWay=1 折扣
    /// </summary>
    public class WxDiscountCoupons
    {
        public string card_type { get; } = "DISCOUNT";
        public Discount discount { get; set; }
    }


    public class Discount
    {
        /// <summary>
        /// 基本的卡券数据,所有卡券类型通用
        /// </summary>
        public base_info base_info { get; set; }

        /// <summary>
        /// 折扣券专用，表示打折额度（百分比）。填30就是七折。
        /// </summary>
        public int discount { get; set; } = 0;
       

    }


    /// <summary>
    /// 微信卡包里的代金券优惠劵  CouponWay=0 代金券
    /// </summary>
    public class WxCashCoupons
    {
        public string card_type { get; } = "CASH";
        public Cash cash { get; set; }
    }


    public class Cash
    {
        /// <summary>
        /// 基本的卡券数据,所有卡券类型通用
        /// </summary>
        public base_info base_info { get; set; }

        /// <summary>
        /// （卡券高级信息）字段
        /// </summary>
        public Advanced_info advanced_info { get; set; }

        /// <summary>
        /// 代金券专用，表示起用金额（单位为分）,如果无起用门槛则填0。
        /// </summary>
        public int least_cost { get; set; } = 0;
        /// <summary>
        /// 代金券专用，表示减免金额。（单位为分）
        /// </summary>
        public int reduce_cost { get; set; } = 0;

    }





    /// <summary>
    /// （卡券高级信息）字段
    /// </summary>
    public class Advanced_info
    {
        public Use_condition use_condition { get; set; }
    }

 


    /// <summary>
    /// 使用门槛（条件）字段，若不填写使用条件则在券面拼写 ：无最低消费限制，全场通用，不限品类；并在使用说明显示： 可与其他优惠共享
    /// </summary>
    public class Use_condition
    {
        /// <summary>
        /// 指定可用的商品类目，仅用于代金券类型 ，填入后将在券面拼写适用于xxx
        /// </summary>
        public string accept_category { get; set; } = "商品类目进店咨询";
        /// <summary>
        /// 指定不可用的商品类目，仅用于代金券类型 ，填入后将在券面拼写不适用于xxxx
        /// </summary>
        public string reject_category { get; set; } = "商品类目进店咨询";

        /// <summary>
        /// 满减门槛字段，可用于兑换券和代金券 ，填入后将在全面拼写消费满xx元可用。
        /// </summary>
        public int least_cost { get; set; } = 0;

        /// <summary>
        /// 不可以与其他类型共享门槛 ，填写false时系统将在使用须知里 拼写“不可与其他优惠共享”， 填写true时系统将在使用须知里 拼写“可与其他优惠共享”， 默认为true
        /// </summary>
        public bool can_use_with_other_discount = false;

    }





}
