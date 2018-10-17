using Entity.Base;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 微信会员卡
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class VipWxCard
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 如果是电商版 则对应店铺Id  如果是餐饮版则对应餐饮的Id
        /// </summary>
        [SqlField]
        public int AppId { get; set; } = 0;
        /// <summary>
        /// 微信会员卡Id 在微信那边创建会员卡后返回的Id
        /// </summary>
        [SqlField]
        public string CardId { get; set; } = string.Empty;


        /// <summary>
        /// 服务号原始Id
        /// </summary>
        [SqlField]
        public string User_Name { get; set; } = string.Empty;


        /// <summary>
        /// 服务号名称
        /// </summary>
        [SqlField]
        public string SerName { get; set; } = string.Empty;


        /// <summary>
        /// 会员卡对应的类型 电商版为0,餐饮版为1 专业版为2,足浴版3,多门店版本4
        /// </summary>
        [SqlField]
        public int Type { get; set; } = 0;


        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }


        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 卡券审核状态
        /// </summary>
        [SqlField]
        public int Status { get; set; }

       

    }



    /// <summary>
    /// 微信会员卡与卡号 领取后会得到卡号 唯一标示对每个用户
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class VipWxCardCode
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;


        [SqlField]
        public string CardId { get; set; } = string.Empty;

        /// <summary>
        /// 卡券Code码 每个用户对应的卡唯一识别
        /// </summary>
        [SqlField]
        public string Code { get; set; }

        /// <summary>
        /// 用户Id 对应不同小程序唯一标示
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

    }


    /// <summary>
    /// 更新微信会员卡审核结果
    /// </summary>
    public class UpdateWxCard
    {
        /// <summary>
        /// 错误码，0为正常。
        /// </summary>
        public int errcode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string errmsg { get; set; }
        /// <summary>
        /// 是否提交审核，false为修改后不会重新提审，true为修改字段后重新提审，该卡券的状态变为审核中。
        /// </summary>
        public bool send_check { get; set; }
    }

    /// <summary>
    /// 微信会员卡 卡套创建后微信审核结果
    /// </summary>
    public  enum WxVipCardStatus
    {
        /// <summary>
        /// 卡券被商户删除
        /// </summary>
        CARD_STATUS_DELETE1 = -1,

        /// <summary>
        /// 待审核
        /// </summary>
        CARD_STATUS_NOT_VERIFY =0,
        /// <summary>
        /// 审核失败
        /// </summary>
        CARD_STATUS_VERIFY_FAIL=1,
        /// <summary>
        /// 通过审核
        /// </summary>
        CARD_STATUS_VERIFY_OK=2,
        /// <summary>
        /// 在公众平台投放过的卡券
        /// </summary>
        CARD_STATUS_DISPATCH = 3


    }


    public class WxCard
    {
      public Card card { get; set; }

    }

    /// <summary>
    /// 微信卡券 官方文档 string(15) 一个中文 占 3 英文 数字 占1
    /// </summary>
    public class Card
    {
        public string card_type { get; } = "MEMBER_CARD";
        public member_card member_card { get; set; }
    }

    /// <summary>
    /// 会员卡
    /// </summary>

    public class member_card
    {
        ///// <summary>
        ///// 背景图上传
        ///// </summary>
        //public string background_pic_url { get; set; }

        /// <summary>
        /// 基本的卡券数据,所有卡券类型通用
        /// </summary>
        public base_info base_info { get; set; }

        /// <summary>
        /// 会员卡特权说明,限制1024汉字
        /// </summary>
        public string prerogative { get; set; }

        /// <summary>
        /// 设置为true时用户领取会员卡后系统自动将其激活，无需调用激活接口
        /// </summary>
        public bool auto_activate { get; set; } = true;
      

        /// <summary>
        /// 显示积分，填写true或false，如填写true，积分相关字段均为必填
        /// 若设置为true则后续不可以被关闭。
        /// </summary>
        public bool supply_bonus { get; } = false;
        /// <summary>
        /// 若设置为true则后续不可以被关闭。该字段须开通储值功能后方可使用
        /// </summary>
        public bool supply_balance { get; } = false;
        /// <summary>
        ///  自定义会员信息类目1，会员卡激活后显示 例如:储值余额
        /// </summary>
        public custom_fieldItem custom_field1 { get; set; }
        /// <summary>
        ///  自定义会员信息类目2，会员卡激活后显示 例如:累计消费
        /// </summary>
        public custom_fieldItem custom_field2 { get; set; }
        /// <summary>
        ///  自定义会员信息类目3，会员卡激活后显示  例如:会员级别
        /// </summary>
        public custom_fieldItem custom_field3 { get; set; }

    
    }




    /// <summary>
    /// 自定义会员信息类目，会员卡激活后显示
    /// 此类目信息 显示在会员卡背景下方 最多3个
    /// </summary>
    public class custom_fieldItem
    {
        /// <summary>
        /// 会员信息类目自定义名称，当开发者变更这类类目信息的value值时
        /// 不会触发系统模板消息通知用户
        /// </summary>
        public string name { get; set; }
       
    }
    public class base_info
    {
        /// <summary>
        /// 卡券的商户logo，建议像素为300*300。    
        /// </summary>
        public string logo_url { get; set; }
        /// <summary>
        /// Code展示类型
        /// </summary>
        public string code_type { get; set; }
        /// <summary>
        /// 商户名字,字数上限为12个汉字。      
        /// </summary>
        public string brand_name { get; set; }
        /// <summary>
        /// 卡券名，字数上限为9个汉字(建议涵盖卡券属性、服务及金额)。
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 券颜色。按色彩规范标注填写Color010-Color100 
        /// </summary>
        public string color { get; set; }

        /// <summary>
        /// 卡券使用提醒，字数上限为16个汉字
        /// </summary>
        public string notice { get; set; }
        /// <summary>
        /// 卡券使用说明，字数上限为1024个汉字
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 商品信息 JSON
        /// </summary>
        public skuItem sku { get; set; } = new skuItem();
       

        /// <summary>
        /// 使用日期，有效期的信息。JSON
        /// </summary>
        public object date_info { get; set; }= new date_infoItem();

        /// <summary>
        /// 顶部居中的标题，仅在卡券激活后且可用状态时显示 储值余额
        /// </summary>
        public string center_title { get; set; }

        /// <summary>
        ///自定义使用入口跳转小程序的user_name，格式为原始id+@app  例如:gh_86a091e50ad4@app
        /// </summary>
        public string center_app_brand_user_name { get; set; }

        /// <summary>
        /// 自定义居中使用入口小程序页面地址
        /// </summary>
        public string center_app_brand_pass { get; set; }

        /// <summary>
        /// 自定义跳转外链的入口名字
        /// </summary>
        public string custom_url_name { get; set; }

        /// <summary>
        ///   自定义使用入口跳转小程序的user_name，格式为原始id+@app  例如:gh_86a091e50ad4@app
        /// </summary>
        public string custom_app_brand_user_name { get; set; }

        /// <summary>
        /// 自定义使用入口小程序页面地址
        /// </summary>
        public string custom_app_brand_pass { get; set; }

        /// <summary>
        /// 显示在入口右侧的提示语
        /// </summary>
        public string custom_url_sub_title { get; set; }

        ///// <summary>
        ///// 营销场景的自定义入口名称
        ///// </summary>
        //public string promotion_url_name { get; set; }

        ///// <summary>
        /////   营销场景使用入口跳转小程序的user_name，格式为原始id+@app  例如:gh_86a091e50ad4@app
        ///// </summary>
        //public string promotion_app_brand_user_name { get; set; }

        ///// <summary>
        ///// 营销场景的自定入口跳转外链的地址链接 小程序页面地址
        ///// </summary>
        //public string promotion_app_brand_pass { get; set; }
        
        /// <summary>
        /// 没人可领取的数量
        /// </summary>
        public int get_limit { get; set; } = 999;



    }

    public class skuItem
    {
        public int quantity { get; set; } = 100000000;
    }

    /// <summary>
    /// 固定日期有效类型
    /// 永久有效类型(DATE_TYPE_PERMANENT)
    /// </summary>
    public class date_infoItem
    {
        /// <summary>
        /// 固定日期有效类型
        /// 永久有效类型(DATE_TYPE_PERMANENT)
        /// </summary>
        public string type { get; set; } = "DATE_TYPE_PERMANENT";

    }

    /// <summary>
    /// DATETYPE FIX_TERM 表示固定时长 （自领取后按天算。
    /// </summary>
    public class Seconddate_infoItem
    {

        /// DATE_TYPE_FIX _TIME_RANGE 表示固定日期区间
        /// ，DATE_TYPE_FIX_TERM 表示固定时长 （自领取后按天算。
        /// </summary>
        public string type { get; set; } = "DATE_TYPE_FIX_TERM";

        /// <summary>
        /// type为DATE_TYPE_FIX_TERM时专用，表示自领取后多少天内有效，不支持填写0。
        /// </summary>
        public int fixed_term { get; set; }

        /// <summary>
        /// type为DATE_TYPE_FIX_TERM时专用，表示自领取后多少天开始生效，领取后当天生效填写0。（单位为天）
        /// </summary>
        public int fixed_begin_term { get; set; }


    }
    /// <summary>
    /// DATE_TYPE_FIX _TIME_RANGE 表示固定日期区间
    /// </summary>
    public class Firstdate_infoItem
    {

        /// DATE_TYPE_FIX _TIME_RANGE 表示固定日期区间
        /// ，DATETYPE FIX_TERM 表示固定时长 （自领取后按天算。
        /// </summary>
        public string type { get; set; } = "DATE_TYPE_FIX_TIME_RANGE";

        /// <summary>
        /// type为DATE_TYPE_FIX_TIME_RANGE时专用，表示起用时间。从1970年1月1日00:00:00至起用时间的秒数，最终需转换为字符串形态传入。（东八区时间,UTC+8，单位为秒）
        /// </summary>
        public long begin_timestamp { get; set; }

        /// <summary>
        /// 表示结束时间 ， 建议设置为截止日期的23:59:59过期 。 （ 东八区时间,UTC+8，单位为秒 ）
        /// </summary>
        public long end_timestamp { get; set; }


    }




    /// <summary>
    /// 微信会员卡创建结果 由微信那边返回
    /// </summary>

    public class CreateCardResult
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int errcode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        /// 微信会员卡id  微信那边返回的
        /// </summary>
        public string card_id { get; set; }
    }


    public class DesCodeResult
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int errcode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        /// 解密后获取的真实Code码
        /// </summary>
        public string code { get; set; }
    }

}
