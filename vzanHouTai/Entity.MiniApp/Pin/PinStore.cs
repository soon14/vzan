using Entity.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Utility;

namespace Entity.MiniApp.Pin
{
    /// <summary>
    /// 拼享惠店铺设置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinStore
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// aid
        /// </summary>
        [SqlField]
        public int aId { get; set; } = 0;

        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int userId { get; set; } = 0;

        [SqlField]
        public string loginName { get; set; } = string.Empty;

        [SqlField]
        public string password { get; set; } = string.Empty;

        /// <summary>
        /// 店铺名称
        /// </summary>
        [SqlField]
        public string storeName { get; set; } = string.Empty;

        /// <summary>
        /// 店铺头像
        /// </summary>
        [SqlField]
        public string logo { get; set; } = string.Empty;

        /// <summary>
        /// 店铺电话
        /// </summary>
        [SqlField]
        public string phone { get; set; } = string.Empty;

        /// <summary>
        /// 状态 1正常 0关闭 -1删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        public string stateName
        {
            get
            {
                switch (state)
                {
                    case 0: return "关闭";
                    case 1: return "正常";
                    default: return "";
                }
            }
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        [SqlField]
        public DateTime startDate { get; set; }

        public string startDateStr
        {
            get
            {
                return startDate.ToShortDateString();
                //return startDate.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        [SqlField]
        public DateTime endDate { get; set; }

        public string endDateStr
        {
            get
            {
                return endDate.ToString("yyyy-MM-dd");
            }
        }
        /// <summary>
        /// 扫店铺码/商品码交易总额 单位：分
        /// </summary>
        [SqlField]
        public int qrcodeMoney { get; set; } = 0;
        public string qrcodeMoneyStr
        {
            get
            {
                return (qrcodeMoney * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 非店内扫码交易总额 单位：分
        /// </summary>
        [SqlField]
        public int money { get; set; } = 0;

        public string moneyStr
        {
            get
            {
                return (money * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 扫店铺码/商品码购买的可提现金额 单位：分
        /// </summary>
        [SqlField]
        public int qrcodeCash { get; set; } = 0;
        public string qrcodeCashStr
        {
            get
            {
                return (qrcodeCash * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 非店内扫码可提现金额 单位：分
        /// </summary>
        [SqlField]
        public int cash { get; set; } = 0;

        public string cashStr
        {
            get
            {
                return (cash * 0.01).ToString("0.00");
            }
        }

        [SqlField]
        public string settingJson { get; set; } = JsonConvert.SerializeObject(new SettingModel());

        /// <summary>
        /// 客服用户id
        /// </summary>
        [SqlField]
        public string kfUserIds { get; set; } = string.Empty;

        public C_UserInfo kfUserInfo { get; set; }

        /// <summary>
        /// 入驻时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;
        public string addtimeStr
        {
            get
            {
                return addtime.ToString("yyyy-MM-dd HH:mm:ss") == "0001-01-01 00:00:00" ? "" : addtime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 商家认证 =0表示待审核，=1表示审核通过
        /// </summary>
        [SqlField]
        public int rz { get; set; } = 0;

        public SettingModel setting
        {
            get
            {
                return JsonConvert.DeserializeObject<SettingModel>(settingJson);
            }
        }

        /// <summary>
        /// 是否可用，已关闭，已过期,审核中 都是不可用
        /// </summary>
        public bool isAvailable
        {
            get
            {
                DateTime now = DateTime.Now;
                if (state == 1)
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// 银行卡账户名
        /// </summary>
        [SqlField]
        public string bankcardName { get; set; } = string.Empty;

        /// <summary>
        /// 银行卡号
        /// </summary>
        [SqlField]
        public string bankcardNum { get; set; }
        /// <summary>
        /// 已上架商品
        /// </summary>
        public int goodsCount { get; set; } = 0;
        /// <summary>
        /// 推广者id
        /// </summary>
        [SqlField]
        public int agentId { get; set; } = 0;
        public PinAgent agentInfo { get; set; }
        public string agentFee { get; set; } = "0.00";
        public string nickName { get; set; }
        /// <summary>
        /// 是否标杆店铺 -1：申请失败， 0：未申请， 1：申请中， 2：申请成功 枚举 pinEnums.BiaoGanState
        /// </summary>
        [SqlField]
        public int biaogan { get; set; } = 0;
        public string biaoganStr
        {
            get
            {
                return Enum.GetName(typeof(PinEnums.BiaoGanState), biaogan);
            }
        }

        public void SetSettingValue(string key, object value)
        {
            Type configType = typeof(SettingModel);
            object newValue = Convert.ChangeType(value, configType.GetProperty(key).PropertyType);
            SettingModel newSetting = setting;
            configType.GetProperty(key).SetValue(newSetting, newValue);
            settingJson = JsonConvert.SerializeObject(newSetting);
        }
        /// <summary>
        /// 店铺发送公众号模板消息用户openId
        /// </summary>
        [SqlField]
        public string wxOpenId { get; set; }
        /// <summary>
        /// 上级代理用户信息
        /// </summary>
        public C_UserInfo fuserInfo { get; set; }
    }


    public class SettingModel
    {
        /// <summary>
        /// 新订单提示音
        /// </summary>
        public int voiceTips { get; set; } = 0;

        /// <summary>
        /// 客服私信
        /// </summary>
        public int openIm { get; set; } = 0;

        /// <summary>
        /// 开启自动问候
        /// </summary>
        public int autoWelcome { get; set; } = 0;

        /// <summary>
        /// 问候语
        /// </summary>
        [MaxLength(length: 25, ErrorMessage = "最多25个字")]
        public string welcome { get; set; } = "欢迎光临";

        /// <summary>
        /// 开启客服电话
        /// </summary>
        public int openKfPhone { get; set; } = 0;

        /// <summary>
        /// 客服号码
        /// </summary>
        [Phone(ErrorMessage = "请输入正确的电话号码")]
        public string kfPhone { get; set; } = string.Empty;

        /// <summary>
        /// 开启到店自取
        /// </summary>
        public int openZq { get; set; } = 0;
        
        /// <summary>
        /// 运费模板开关
        /// </summary>
        public bool freightSwitch { get; set; } = false;
        /// <summary>
        /// 运费合计规则
        /// </summary>
        public int freightSumRule { get; set; } = (int)DeliveryFeeSumMethond.有赞;
        /// <summary>
        /// 退货地址
        /// </summary>
        public string place { get; set; }
        /// <summary>
        /// 退货收件人
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 退货收件号码
        /// </summary>
        public string phone { get; set; }
    }
}