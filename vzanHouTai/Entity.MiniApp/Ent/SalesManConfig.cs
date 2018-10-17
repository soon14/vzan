using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 小程序专业版-分销配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SalesManConfig
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;

        /// <summary>
        /// 设置
        /// </summary>
        [SqlField]
        public string configStr { get; set; } = string.Empty;

        /// <summary>
        /// 分销开关 0→关闭 1→开启
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        ///增加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        public string UpdateTimeStr
        {
            get
            {
                return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }


        public ConfigModel configModel { get; set; }
    }


    /// <summary>
    /// 分销设置模型
    /// </summary>
    public class ConfigModel
    {
        /// <summary>
        /// 分销员招募与管理
        /// </summary>
        public SalesManManager salesManManager { get; set; } = new SalesManManager();
        /// <summary>
        /// 佣金管理与发放
        /// </summary>
        public PayMentManager payMentManager { get; set; } = new PayMentManager();
        /// <summary>
        /// 页面信息展示
        /// </summary>
        public PageShowWay pageShowWay { get; set; } = new PageShowWay();



        /// <summary>
        /// 二级分销设置
        /// </summary>
        public SecondSalesManConfig secondSalesManConfig { get; set; } = new SecondSalesManConfig();

        /// <summary>
        /// 招募计划
        /// </summary>
        public RecruitPlan recruitPlan { get; set; } = new RecruitPlan();




    }


    /// <summary>
    /// 分销员招募与管理
    /// </summary>
    public class SalesManManager
    {
        /// <summary>
        /// 分销员招募 0关闭 1开启
        /// </summary>
        public int allow_recruit { get; set; } = 0;
        /// <summary>
        /// 分销员招募审核  1→开启分销员审核功能后，消费者申请成为本店分销员需要经过商家审核 0→不需要审核
        /// 2/3表示其它设置 累计消费额满N元通过或者累计充值满N元通过
        /// </summary>
        public int is_verify_on { get; set; } = 0;


        /// <summary>
        /// 累计消费额满N元通过 
        /// </summary>
        public bool Cost_verify_on { get; set; }

        /// <summary>
        /// 累计消费多少元
        /// </summary>
        public float CostMoney { get; set; }

        /// <summary>
        /// 累计充值满N元通过 
        /// </summary>
        public bool SaveMoney_verify_on { get; set; }

        /// <summary>
        /// 累计充值多少元
        /// </summary>
        public float SaveMoney { get; set; }

        /// <summary>
        /// 分销员招募有效期 前端分销员招募按钮显示时间
        /// 0表示短期 15天  -1 表示 永久  1为自定义天数  
        /// </summary>
        public int exp_time_type { get; set; } = 0;

        /// <summary>
        /// 有效期的天数
        /// </summary>
        public int exp_time_day { get; set; } = 15;


        /// <summary>
        /// true开启分销员保护期  false 关闭   商家开启分销员保护期设置后，在保护期内，分销员发展的客户不会变更绑定关系。
        /// </summary>
        public bool is_protect_seller { get; set; } = false;

        /// <summary>
        /// 多少天内，分销员发展的客户不会变更绑定关系
        /// </summary>
        public int protected_time { get; set; } = 0;

        /// <summary>
        /// 分销员建立客户关系.设置允许后，商家允许分销员之间建立客户关系。 1表示允许 0表示不允许
        /// </summary>
        public int allow_sellers_related { get; set; } = 0;


    }

    /// <summary>
    /// 佣金管理与发放
    /// </summary>
    public class PayMentManager
    {
        /// <summary>
        /// 分销员购买权限,
        /// 商家开启分销员购买权限，成为分销员后将立即与自己绑定客户关系，不受保护期限制。 
        /// 0表示关闭 1表示开启
        /// </summary>
        public int allow_seller_buy { get; set; } = 0;

        /// <summary>
        /// 结算方式 0表示人工结算 1表示自动结算
        /// 1.自动结算：系统根据你设置的佣金比例，自动与分销员进行结算。
        /// 可同时设置店铺和单商品佣金比例，单商品的佣金比例优先级高于店铺佣金比例。
        /// 2.人工结算：需要你自行与分销员进行业绩结算，通过线下转账方式发放佣金，系统不参与过程。
        /// </summary>
        public int auto_settle { get; set; } = 0;



        /// <summary>
        /// 佣金比例方式结算时候 设置的佣金比例
        /// </summary>
        public double cps_rate { get; set; } = 20.00;


        /// <summary>
        /// 结算时间
        /// 1表示 交易完成结算   
        /// 一般情况下发货后7天内（含7天）给分销员结算佣金，期间发生的退款会自动扣除（微信支付－自有除外）风险提醒：
        /// 若您选择交易完成结算的方式，则交易完成后发生的维权退款不影响已结算的佣金，可能会造成佣金亏损。    
        /// 2.售后维权处理期结束后再结算  交易完成后需要再等15天，直到不会产生售后退款或处理完售后退款再给分销员结算佣金。
        /// </summary>
        public int settle_time_type { get; set; } = 0;


    }


    /// <summary>
    /// 页面信息展示  前端 我的分销中心里的产品页面显示
    /// </summary>
    public class PageShowWay
    {
        /// <summary>
        /// 显示类型 0表示显示佣金 1表示显示佣金比例
        /// </summary>
        public int is_show_cps_type { get; set; }


    }


    /// <summary>
    /// 招募计划模型
    /// </summary>
    public class RecruitPlan
    {
        public string title { get; set; } = "";
        public string description { get; set; } = "";
    }


    /// <summary>
    /// 二级分销设置
    /// </summary>
    public class SecondSalesManConfig
    {
        /// <summary>
        /// 二级分销开关状态 0→关闭 1→开启
        /// </summary>
        public int State { get; set; } = 0;

        /// <summary>
        /// 直销佣金比例
        /// </summary>
        public double FirstCps_rate { get; set; } = 80.00;

        /// <summary>
        /// 渠道佣金比例
        /// </summary>
        public double SecondCps_rate
        {
            get
            {
                return 100 - FirstCps_rate;
            }
        }


    }




    /// <summary>
    /// 分销商品显示列表
    /// </summary>
    public class SalesmanGoods
    {
        public int goodsId { get; set; } = 0;
        public string goodsImg { get; set; } = string.Empty;
        public string goodsName { get; set; } = string.Empty;
        public float goodsPrice { get; set; } = 0f;
        public int goodsStock { get; set; } = 0;
        public bool stockLimit { get; set; } = true;

        public int salesCount { get; set; } = 0;

        /// <summary>
        /// 上架下架
        /// </summary>
        public int goodsState { get; set; } = 0;

        /// <summary>
        /// 是否属于分销商品 0→不参与 1→参与
        /// </summary>
        public int isDistribution { get; set; } = 0;

        /// <summary>
        /// 是否默认店铺佣金比例 0→默认 1→自定义
        /// </summary>
        public int isDefaultCps_Rate { get; set; } = 0;

        /// <summary>
        /// 佣金比例
        /// </summary>
        public double cps_rate { get; set; } = 00.00;

        public string cps_Money { get; set; }


        public string distributionTime { get; set; } = string.Empty;



        /// <summary>
        /// 显示类型 0表示显示佣金 1表示显示佣金比例
        /// </summary>
        public int is_show_cps_type { get; set; }


    }


}
