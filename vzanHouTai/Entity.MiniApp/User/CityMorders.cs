using Entity.Base;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
	/// orders:付款订单--实体类(属性说明自动提取数据库字段的描述信息)
	/// </summary>
	[Serializable]
    [SqlTable(dbEnum.MINIAPP, UseMaster = true)]

    public class CityMorders
    {
        public CityMorders()
        { }
        #region Model
        private int _id = 0;
        private string _trade_no = string.Empty;
        private string _orderno = string.Empty;
        private int _ordertype = 0;
        private int _fuserid = 0;
        private string _fusername = string.Empty;
        private int _tuserid = 0;
        private string _tuserguid = string.Empty;
        private string _tusername = string.Empty;
        private int _payment_free = 0;
        private int _payment_status = 0;
        private DateTime _payment_time = DateTime.Now;
        private int _minisnsid = 0;
        private int _articleid = 0;
        private int _status = 0;
        private int _actiontype = 0;
        private int _percent = 0;
        private int _commentid = 0;
        private int _operstatus = 0;
        private string _remark = string.Empty;
        private DateTime _confirm_time = DateTime.Now.AddYears(-1);
        private DateTime _addtime = DateTime.Now;
        private string _shownote = string.Empty;

        /// <summary>
        /// 附件参数类型
        /// </summary>
        [SqlField]
        public string AttachPar { get; set; } = string.Empty;

        /// <summary>
        /// 主键Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 外部交易订单号
        /// </summary>
        [SqlField]
        public string trade_no
        {
            set { _trade_no = value; }
            get { return _trade_no; }
        }
        /// <summary>
        /// 内部订单编号
        /// </summary>
        [SqlField]
        public string orderno
        {
            set { _orderno = value; }
            get { return _orderno; }
        }
        /// <summary>
        /// 订单类型
        /// </summary>
        [SqlField]
        public int OrderType
        {
            set { _ordertype = value; }
            get { return _ordertype; }
        }
        /// <summary>
        /// 订单来源用户Id
        /// </summary>
        [SqlField]
        public int FuserId
        {
            set { _fuserid = value; }
            get { return _fuserid; }
        }
        /// <summary>
        /// 订单来源用户名
        /// </summary>
        [SqlField]
        public string Fusername
        {
            set { _fusername = value; }
            get { return _fusername; }
        }
        /// <summary>
        /// 订单接收用户Id --不知道从什么时候开始,这个字段用来存储订单ID
        /// </summary>
        [SqlField]
        public int TuserId
        {
            set { _tuserid = value; }
            get { return _tuserid; }
        }
        /// <summary>
        /// 订单接收用户GUID
        /// </summary>
        [SqlField]
        public string TuserGuid
        {
            set { _tuserguid = value; }
            get { return _tuserguid; }
        }
        /// <summary>
        /// 订单接收用户名
        /// </summary>
        [SqlField]
        public string Tusername
        {
            set { _tusername = value; }
            get { return _tusername; }
        }
        /// <summary>
        /// 付款金额,单位：分
        /// </summary>
        [SqlField]
        public int payment_free
        {
            set { _payment_free = value; }
            get { return _payment_free; }
        }

        public string payment_free_fmt
        {
            get { return string.Format("{0:C}", payment_free * 0.01); }
        }
        /// <summary>
        /// 付款状态  0==未成功  1==付款成功
        /// </summary>
        [SqlField]
        public int payment_status
        {
            set { _payment_status = value; }
            get { return _payment_status; }
        }
        /// <summary>
        /// 付款时间
        /// </summary>
        [SqlField]
        public DateTime payment_time
        {
            set { _payment_time = value; }
            get { return _payment_time; }
        }

        public string payment_time_fmt { get { return payment_time.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 商家区域代码
        /// </summary>
        [SqlField]
        public int MinisnsId
        {
            set { _minisnsid = value; }
            get { return _minisnsid; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int Articleid
        {
            set { _articleid = value; }
            get { return _articleid; }
        }
        /// <summary>
        /// 状态 -1已关闭 0未付款 1已付款
        /// </summary>
        [SqlField]
        public int Status
        {
            set { _status = value; }
            get { return _status; }
        }
        public string StatusStr
        {
            get
            {
                switch (Status)
                {
                    case 0: return "未付款";
                    case 1: return "已付款";
                    case -1: return "已关闭";
                    default: return "无效状态";
                }
            }
        }
        /// <summary>
        /// 打赏或者其他操作类型
        /// </summary>
        [SqlField]
        public int ActionType
        {
            set { _actiontype = value; }
            get { return _actiontype; }
        }
        /// <summary>
        /// 提成百分比
        /// </summary>
        [SqlField]
        public int Percent
        {
            set { _percent = value; }
            get { return _percent; }
        }
        /// <summary>
        /// 回帖Id /数量 /砍价id /竞拍报名id
        /// </summary>
        [SqlField]
        public int CommentId
        {
            set { _commentid = value; }
            get { return _commentid; }
        }
        /// <summary>
        /// 店铺的ID，用于统计店铺的收入
        /// </summary>
        [SqlField]
        public int OperStatus
        {
            set { _operstatus = value; }
            get { return _operstatus; }
        }
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string remark
        {
            set { _remark = value; }
            get { return _remark; }
        }
        /// <summary>
        /// 审核确认时间
        /// </summary>
        [SqlField]
        public DateTime confirm_time
        {
            set { _confirm_time = value; }
            get { return _confirm_time; }
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime Addtime
        {
            set { _addtime = value; }
            get { return _addtime; }
        }
        /// <summary>
        /// 付款显示内容
        /// </summary>
        [SqlField]
        public string ShowNote
        {
            set { _shownote = value; }
            get { return _shownote; }
        }


        /// <summary>
        /// 支付到公共号的appid
        /// </summary>
        [SqlField]
        public string appid { get; set; }
        /// <summary>
        /// 支付到的商户平台
        /// </summary>
        [SqlField]
        public string mch_id { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [SqlField]
        public string userip { get; set; }

        /// <summary>
        /// 购买数量 / 店铺入驻，开通 高级版（VIP） 的时长小时
        /// </summary>
        [SqlField]
        public int buy_num { get; set; } = 0;
        /// <summary>
        /// 是否团购
        /// </summary>
        [SqlField]
        public int is_group { get; set; }
        /// <summary>
        /// 是否开团团长
        /// </summary>
        [SqlField]
        public int is_group_head { get; set; }
        /// <summary>
        /// 拼团参团id /砍价id
        /// </summary>
        [SqlField]
        public int groupsponsor_id { get; set; }

        /// <summary>
        /// 是否分销订单，分销同城id
        /// </summary>
        [SqlField]
        public int CitySubId { get; set; }

        /// <summary>
        /// 支付费率 默认0.994
        /// </summary>
        [SqlField]
        public double PayRate { get; set; } = 0.994;
        /// <summary>
        /// 店铺Id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

        public string Phone { get; set; }
        /// <summary>
        /// 留言
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 拼团用户购买记录ID
        /// </summary>
        public int guid { get; set; }
        #endregion Model
    }

    /// <summary>
	/// 小计实体
	/// </summary>
	[Serializable]
    //[SqlTable(dbEnum.MINIAPP, UseMaster = true)]

    public class CityMordersAmount
    {
        //[SqlField]
        public double day_amount { get; set; } = 0;
        //[SqlField]
        public double week_amount { get; set; } = 0;
        //[SqlField]
        public double month_amount { get; set; } = 0;
    }

    public class CityMordersAttach
    {
        public int discountType { get; set; }
        public string phone { get; set; }
        public CouponLog coupon { get; set; }
        public int payMoney { get; set; }
        public string money_coupon { get; set; }
        public string money_vip { get; set; }
        public VipLevel levelInfo { get; set; }
    }
}
