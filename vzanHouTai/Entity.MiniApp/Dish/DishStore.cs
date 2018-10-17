using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 餐饮多门店配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishStore
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;

        /// <summary>
        /// 小程序aid
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;

        /// <summary>
        /// 门店分类id
        /// </summary>
        [SqlField]
        public int dish_cate_id { get; set; } = 0;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string cat_name { get; set; } = string.Empty;

        /// <summary>
        /// 门店名称
        /// </summary>
        [SqlField]
        public string dish_name { get; set; } = string.Empty;

        /// <summary>
        /// 是否主店，如果设置为1小程序自动开启单店模式
        /// </summary>
        [SqlField]
        public int ismain { get; set; } = 0;
        /// <summary>
        /// logo  
        /// </summary>
        [SqlField]
        public string dish_logo { get; set; } = string.Empty;

        /// <summary>
        /// 门店手机
        /// </summary>
        [SqlField]
        public string dish_con_mobile { get; set; } = string.Empty;

        /// <summary>
        /// 门店电话
        /// </summary>
        [SqlField]
        public string dish_con_phone { get; set; } = string.Empty;


        /// <summary>
        /// 门店有效时间 - 开始
        /// </summary>
        [SqlField]
        public DateTime dish_begin_time { get; set; } = DateTime.Now;

        public string dish_begin_time_Str
        {
            get { return dish_begin_time.ToString("yyyy-MM-dd"); }
        }

        /// <summary>
        /// 门店有效时间 - 结束
        /// </summary>
        [SqlField]
        public DateTime dish_end_time { get; set; } = DateTime.Now.AddYears(1);

        public string dish_end_time_Str
        {
            get { return dish_end_time.ToString("yyyy-MM-dd"); }
        }

        /// <summary>
        /// 提现费率
        /// </summary>
        [SqlField]
        public double cash_fee { get; set; } = 0.00d;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }

        /// <summary>
        /// 状态 1为正常 0关闭 -1为删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 基本设置
        /// </summary>
        [SqlField]
        public string baseConfigJson { get; set; } = string.Empty;
        public DishBaseConfig baseConfig { get; set; } = new DishBaseConfig();

        /// <summary>
        /// 高级设置
        /// </summary>
        [SqlField]
        public string gaojiConfigJson { get; set; } = string.Empty;
        public DishGaojiConfig gaojiConfig { get; set; } = new DishGaojiConfig();

        /// <summary>
        /// 店内设置
        /// </summary>
        [SqlField]
        public string dianneiConfigJson { get; set; } = string.Empty;
        public DishDianneiConfig dianneiConfig { get; set; } = new DishDianneiConfig();

        /// <summary>
        /// 外卖设置
        /// </summary>
        [SqlField]
        public string takeoutConfigJson { get; set; } = string.Empty;
        public DishTakeoutConfig takeoutConfig { get; set; } = new DishTakeoutConfig();

        /// <summary>
        /// 跳转设置
        /// </summary>
        [SqlField]
        public string tiaozhuanConfigJson { get; set; } = string.Empty;

        public DishTiaoZhuanConfig tiaozhuanConfig { get; set; } = new DishTiaoZhuanConfig();

        /// <summary>
        /// 支付设置
        /// </summary>
        [SqlField]
        public string paySettingJson { get; set; } = string.Empty;

        public DishPaySetting paySetting { get; set; }

        /// <summary>
        /// 短信设置
        /// </summary>
        [SqlField]
        public string smsSettingJson { get; set; } = string.Empty;
        public DishSmsSetting smsSetting { get; set; }


        /// <summary>
        /// 店铺首页_小程序码
        /// </summary>
        [SqlField]
        public string storeHome_qrcode { get; set; } = string.Empty;

        /// <summary>
        /// 付款页面_小程序码
        /// </summary>
        [SqlField]
        public string payPage_qrcode { get; set; } = string.Empty;

        /// <summary>
        /// 是否开启配送
        /// </summary>
        [SqlField]
        public int ps_open_status { get; set; } = 0;

        /// <summary>
        /// 配送方式
        /// </summary>
        [SqlField]
        public int ps_type { get; set; } = 1;

        /// <summary>
        /// 管理员账号
        /// </summary>
        [SqlField]
        public string login_username { get; set; } = string.Empty;

        /// <summary>
        /// 管理员密码
        /// </summary>
        [SqlField]

        public string login_userpass { get; set; } = string.Empty;

        [SqlField]
        public string txSettingJson { get; set; } = string.Empty;

        /// <summary>
        /// 纬度
        /// </summary>
        [SqlField]
        public double ws_lat { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        [SqlField]
        public double ws_lng { get; set; }

        /// <summary>
        /// 客户当前距离
        /// </summary>
        public string dish_julimi { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        [SqlField]
        public int commentCount { get; set; } = 0;

        /// <summary>
        /// 是否营业
        /// </summary>
        public int is_yingye_status { get { return StoreIsOpen(baseConfig.open_time,baseConfig.dish_open_status); } }
        private int StoreIsOpen(List<DishOpenTime> open_time,int dish_open_status)
        {
            int code = 0;//0:未营业 1:营业中
            if (open_time == null || open_time.Count <= 0 || dish_open_status==0) return code;
            string date = DateTime.Now.ToShortDateString();
            foreach (var item in open_time)
            {
                DateTime startTime = Convert.ToDateTime($"{date} {item.dish_open_btime}");
                DateTime endTime = DateTime.Now;
                if (item.dish_open_etime == "24:00")
                {
                    date = DateTime.Now.AddDays(1).ToShortDateString();
                    endTime = Convert.ToDateTime($"{date} 00:00");
                }
                else
                {
                    endTime = Convert.ToDateTime($"{date} {item.dish_open_etime}");
                }
                if (DateTime.Compare(DateTime.Now, startTime) > 0 && DateTime.Compare(DateTime.Now, endTime) < 0)
                {
                    code = 1;
                    return code;
                }
            }
            return code;
        }

        /// <summary>
        /// 评分
        /// </summary>
        public double stars { get; set; } = 0;

        /// <summary>
        /// 月销量
        /// </summary>
        public int dish_yue_xiaoliang { get; set; } = 0;

        /// <summary>
        /// 活动
        /// </summary>
        public Dictionary<string, huodong_item> huodong_list { get; set; } = new Dictionary<string, huodong_item>();
        /// <summary>
        /// 接收通知用户openid
        /// </summary>
        [SqlField]
        public string notifyOpenId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 基本设置
    /// </summary>
    public class DishBaseConfig
    {
        /// <summary>
        /// 门店状态
        /// </summary>
        public int dish_open_status { get; set; } = 0;

        /// <summary>
        /// 前台显示
        /// </summary>
        public int dish_show_status { get; set; } = 0;

        /// <summary>
        /// 分享
        /// </summary>
        public int dish_share { get; set; } = 0;

        /// <summary>
        /// 门店logo
        /// </summary>
       // public string dish_logo { get; set; }

        /// <summary>
        /// 门店公告
        /// </summary>
        public string dish_gonggao { get; set; } = string.Empty;

        /// <summary>
        /// 预定公告
        /// </summary>
        public string dish_yuding_gonggao { get; set; } = string.Empty;

        /// <summary>
        /// 所属区域
        /// </summary>
        public string dish_quyu { get; set; } = string.Empty;

        /// <summary>
        /// 商家地址
        /// </summary>
        public string dish_address { get; set; } = string.Empty;

        /// <summary>
        /// 人均消费
        /// </summary>
        public double dish_pingjunxiaofei { get; set; } = 0.00d;

        /// <summary>
        /// 营业时间
        /// </summary>
        public List<DishOpenTime> open_time { get; set; } = new List<DishOpenTime>();

        /// <summary>
        /// 外卖营业时间
        /// </summary>
        public List<DishOpenWmTime> wm_time { get; set; } = new List<DishOpenWmTime>();

        /// <summary>
        ///  门店介绍
        /// </summary>
        public string dish_jieshao { get; set; } = string.Empty;

        /// <summary>
        /// 门店手机号码
        /// </summary>
        public string dish_con_mobile { get; set; } = string.Empty;
        /// <summary>
        /// 门店电话
        /// </summary>
        public string dish_con_phone { get; set; } = string.Empty;

        /// <summary>
        ///  提供服务
        /// </summary>
        public string dish_fuwu { get; set; } = string.Empty;

        public List<string> dish_fuwu_arr
        {
            get
            {
                if (string.IsNullOrEmpty(dish_fuwu))
                    return new List<string>() { };
                return dish_fuwu.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
        /// <summary>
        /// 门店实景
        /// </summary>
        public List<string> dish_shijing { get; set; }

        /// <summary>
        /// 资质证书
        /// </summary>
        public List<string> dish_zizhi { get; set; }

    }

    /// <summary>
    /// 高级设置
    /// </summary>
    public class DishGaojiConfig
    {
        /// <summary>
        /// 菜单样式 0：样式1，1：样式2
        /// </summary>
        public int menu_style { get; set; } = 0;
        /// <summary>
        /// 支持预定
        /// </summary>
        public int dish_is_yuding { get; set; } = 0;

        /// <summary>
        /// 支持店内
        /// </summary>
        public int dish_is_diannei { get; set; } = 0;

        /// <summary>
        /// 支持自取
        /// </summary>
        public int dish_is_ziqu { get; set; } = 0;

        /// <summary>
        /// 支持外卖
        /// </summary>
        public int dish_is_waimai { get; set; } = 0;

        /// <summary>
        /// 支持排队
        /// </summary>
        public int dish_is_paidui { get; set; } = 0;

        /// <summary>
        /// 预订文本
        /// </summary>
        public string dish_yuding_text { get; set; } = string.Empty;

        /// <summary>
        /// 店内文本
        /// </summary>
        public string dish_diannei_text { get; set; } = string.Empty;

        /// <summary>
        /// 自取文本
        /// </summary>
        public string dish_ziqu_text { get; set; } = string.Empty;

        /// <summary>
        /// 外卖文本
        /// </summary>
        public string dish_waimai_text { get; set; } = string.Empty;

        /// <summary>
        /// 排队文本
        /// </summary>
        public string dish_paidui_text { get; set; } = string.Empty;

        /// <summary>
        /// 支付时间限制
        /// </summary>
        public int dish_pay_limit_time { get; set; } = 0;

        /// <summary>
        /// 首次下单短信验证
        /// </summary>
        public int dish_is_sms_check { get; set; } = 0;

        /// <summary>
        /// 活动管理-首单立减-是否开启
        /// </summary>
        public int huodong_shou_isopen { get; set; } = 0;

        /// <summary>
        /// 活动管理-首单立减-立减金额
        /// </summary>
        public double huodong_shou_jiner { get; set; } = 0.00d;


        /// <summary>
        /// 是否支持开发票
        /// </summary>
        public int dish_is_fapiao { get; set; } = 0;

        /// <summary>
        /// 是否开启WebView功能
        /// </summary>
        public int dish_is_webview_open { get; set; } = 0;

        /// <summary>
        /// webview标题
        /// </summary>
        public string dish_webview_text { get; set; } = string.Empty;

        /// <summary>
        /// webview地址
        /// </summary>
        public string dish_webview_url { get; set; } = string.Empty;

        /// <summary>
        /// 点餐备注
        /// </summary>
        public string dish_beizhu_info { get; set; } = string.Empty;

    }
    /// <summary>
    /// 店内设置
    /// </summary>
    public class DishDianneiConfig
    {
        /// <summary>
        /// 就餐方式
        /// </summary>
        public int dish_diannei_fangshi { get; set; } = 1;

        /// <summary>
        /// 是否显示就餐提示
        /// </summary>
        public int dish_diannei_tips_show { get; set; } = 1;

        /// <summary>
        /// 先付款后就餐提
        /// </summary>
        public string dish_diannei_tips_one { get; set; } = "下单付款后，订单才能下送后厨";

        /// <summary>
        /// 先就餐后付款提示
        /// </summary>
        public string dish_diannei_tips_two { get; set; } = "下单后，订单将下送到厨房";

        /// <summary>
        /// 扫码点餐
        /// </summary>
        public int dish_is_rcode_open { get; set; } = 0;

        /// <summary>
        /// 是否支持更换桌号
        /// </summary>
        public int dish_is_zhuohao_change { get; set; } = 1;

        /// <summary>
        /// 自取天数限制
        /// </summary>
        public int dish_ziqu_day { get; set; } = 0;

        /// <summary>
        /// 自取时间限制
        /// </summary>
        public List<DishZqTime> zq_time { get; set; } = new List<DishZqTime>();

    }
    /// <summary>
    /// 外卖设置
    /// </summary>
    public class DishTakeoutConfig
    {
        /// <summary>
        /// 自动接单
        /// </summary>
        public int dish_waimai_auto_post { get; set; } = 0;


        /// <summary>
        /// 基础配送费
        /// </summary>
        public double waimai_peisong_jiner { get; set; } = 0.00d;

        /// <summary>
        /// 基础公里数
        /// </summary>
        public int waimai_peisong_base_juli { get; set; } = 0;

        /// <summary>
        /// 超过每公里增加费用
        /// </summary>
        public int waimai_peisong_base_step { get; set; } = 0;

        /// <summary>
        /// 外卖起送价格
        /// </summary>
        public int waimai_limit_jiner { get; set; } = 0;

        /// <summary>
        /// 配送半径
        /// </summary>
        public int waimai_limit_juli { get; set; } = 0;

        /// <summary>
        /// 配送规则
        /// </summary>
        public List<DishPsRule> ps_rule { get; set; } = new List<DishPsRule>();

    }

    /// <summary>
    /// 跳转设置
    /// </summary>
    public class DishTiaoZhuanConfig
    {
        /// <summary>
        /// 是否开启小程序跳转
        /// </summary>
        public int dish_is_open_tomini { get; set; } = 0;

        /// <summary>
        /// 跳转文本提示
        /// </summary>
        public string dish_tomini_apptext { get; set; } = string.Empty;

        /// <summary>
        /// 小程序appid
        /// </summary>
        public string dish_tomini_appid { get; set; } = string.Empty;

        /// <summary>
        /// 小程序页面路径
        /// </summary>
        public string dish_tomini_appurl { get; set; } = string.Empty;
    }

    /// <summary>
    /// 支付设置
    /// </summary>
    public class DishPaySetting
    {
        /// <summary>
        /// 微信支付
        /// </summary>
        public int pay_weixin_isopen { get; set; } = 1;

        /// <summary>
        /// 现金支付
        /// </summary>
        public int pay_xianjin_isopen { get; set; } = 0;

        /// <summary>
        /// 货到支付
        /// </summary>
        public int pay_daofu_isopen { get; set; } = 0;

        /// <summary>
        /// 余额支付
        /// </summary>
        public int pay_yuer_isopen { get; set; } = 0;
        /// <summary>
        /// 门店买单是否使用优惠券
        /// </summary>
        public int pay_is_useCoupon { get; set; } = 0;
    }

    /// <summary>
    /// 短信设置
    /// </summary>
    public class DishSmsSetting
    {
        /// <summary>
        /// 短信通知号码
        /// </summary>
        public string msg_phone { get; set; } = string.Empty;

        /// <summary>
        /// 点餐提醒
        /// </summary>
        public int msg_diancan_status { get; set; } = 0;

        /// <summary>
        /// 外卖提醒
        /// </summary>
        public int msg_waimai_status { get; set; } = 0;

        /// <summary>
        /// 预定提醒
        /// </summary>
        public int msg_yuding_status { get; set; } = 0;
    }

    /// <summary>
    /// 外卖营业时间
    /// </summary>
    public class DishOpenWmTime
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public string dish_open_wm_btime { get; set; } = "00:00";

        /// <summary>
        /// 结束时间
        /// </summary>
        public string dish_open_wm_etime { get; set; } = "24:00";

    }

    /// <summary>
    /// 店内营业时间
    /// </summary>
    public class DishOpenTime
    {

        public string dish_open_btime { get; set; } = "00:00";

        public string dish_open_etime { get; set; } = "24:00";
    }

    public class DishZqTime
    {
        public string dish_zq_btime { get; set; } = "00:00";
        public string dish_zq_etime { get; set; } = "24:00";
    }

    public class DishPsRule
    {
        /// <summary>
        /// 配送开始时间
        /// </summary>
        public string ps_time_b { get; set; } = "00:00";
        /// <summary>
        /// 配送结束时间
        /// </summary>
        public string ps_time_e { get; set; } = "24:00";
        /// <summary>
        /// 配送费
        /// </summary>
        public double ps_time_jiner { get; set; } = 0.00d;

    }

    /// <summary>
    /// 用于getDishList接口的index_dish_list[].huodong_list
    /// </summary>
    public class huodong_item
    {
        public string hd_info { get; set; } = string.Empty;
        public string hd_style { get; set; } = string.Empty;
        public string hd_title { get; set; } = string.Empty;
        public string s_hd_style { get; set; } = string.Empty;

    }
}
