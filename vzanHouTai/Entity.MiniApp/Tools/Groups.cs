using Entity.Base;
using Entity.MiniApp.Stores;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 普通拼团商品
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Groups
    {

        public string Qrcode_Logo { get; set; }
        public string Qrcode_Url { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 拼团名称，标题
        /// </summary>
        [SqlField]
        public string GroupName { set; get; } = string.Empty;
        /// <summary>
        /// 缩略图
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; } = string.Empty;
        [SqlField]
        public int ImageCount { get; set; }
        // <summary>
        /// 描述
        /// </summary>
        [SqlField]
        public string Description { set; get; } = string.Empty;
        /// <summary>
        /// 团购价 单位分
        /// </summary>
        [SqlField]
        public int DiscountPrice { set; get; }
        public string DiscountPriceStr { get { return (DiscountPrice*0.01).ToString("0.00"); } }
        /// <summary>
        /// 购买价格 单位分
        /// </summary>
        [SqlField]
        public int UnitPrice { get; set; }
        public string UnitPriceStr { get { return (UnitPrice*0.01).ToString("0.00"); } }
        /// <summary>
        /// 原价 单位分
        /// </summary>
        [SqlField]
        public int OriginalPrice { get; set; }
        public string OriginalPriceStr { get { return (OriginalPrice*0.01).ToString("0.00"); } }
        /// <summary>
        /// 成团人数
        /// </summary>
        [SqlField]
        public int GroupSize { get; set; }
        /// <summary>
        /// 生成数量
        /// </summary>
        [SqlField]
        public int CreateNum { get; set; } = 0;
        /// <summary>
        /// 剩余数量
        /// </summary>
        [SqlField]
        public int RemainNum { get; set; }


        /// <summary>
        /// 有效日期开始
        /// </summary>
        [SqlField]
        public DateTime ValidDateStart { set; get; }
        /// <summary>
        /// 有效日期结束
        /// </summary>
        [SqlField]
        public DateTime ValidDateEnd { set; get; }

        /// <summary>
        /// 使用截止时间
        /// </summary>
        [SqlField]
        public DateTime UserDateEnd { get; set; }
        /// <summary>
        /// 每人限购数量
        /// </summary>
        [SqlField]
        public int LimitNum { get; set; }
        /// <summary>
        /// 是否模拟成团
        /// </summary>
        [SqlField]
        public int Simulation { get; set; }
        /// <summary>
        /// 店铺Id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;
        /// <summary>
        /// 发起优惠的用户
        /// </summary>
        [SqlField]
        public int CreateUserId { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 0-待审核 1 审核通过 -1审核不通过
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 状态的格式化显示
        /// </summary>
        public string StateStr { get; set; } = string.Empty;
        /// <summary>
        /// 需要填写手机号码
        /// </summary>
        [SqlField]
        public bool NeedPhone { get; set; } = true;
        ///// <summary>
        ///// 团长抵扣/折扣/优惠金额
        ///// </summary>
        [SqlField]
        public int HeadDeduct { get; set; }
        [SqlField]
        public string specificationdetail { get; set; } = string.Empty;
        [SqlField]
        public string pickspecification { get; set; } = string.Empty;

        /// <summary>
        /// 虚拟销量
        /// </summary>
        [SqlField]
        public int virtualSalesCount { get; set; } = 0;

        /// <summary>
        /// 已售数量 --自行赋值
        /// </summary>
        public int salesCount { get; set; } = 0;

        public string HeadDeductStr { get { return (HeadDeduct*0.01).ToString("0.00"); } }

        public string ShowTime { get; set; }
        public string ShowTimeS { get; set; }
        public string ShowUseTimeEnd { get; set; }
        /// <summary>
        /// 商家地址
        /// </summary>
        public string StoreAddress { get; set; }
        /// <summary>
        /// 商家电话
        /// </summary>
        public string StoreTel { get; set; }
        

        /// <summary>
        /// 店铺名称（用于后台审核）
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// 发布用户的昵称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 店铺类型
        /// </summary>
        public string StoreType { get; set; }

        /// <summary>
        /// 店铺logo
        /// </summary>
        public string LogoUrl { get; set; }
        

        /// <summary>
        /// 是否结束
        /// </summary>
        public int IsEnd { get; set; }
        /// <summary>
        /// 拼团视频
        /// </summary>
        public string VideoPath { get; set; }
        public Store Store { get; set; }
        public bool IsConcern { get; set; } = false;
        public List<C_Attachment> ImgList { get; set; }
        public List<C_Attachment> DescImgList { get; set; }
        public List<GroupUser> GroupUserList { get; set; }

        public List<GroupSponsor> GroupSponsorList { get; set; }
        //自己已购买此团总量
        public int GroupsNum { get; set; } = 0;
        //猜你喜欢
        public List<Groups> GroupsList { get; set; }

        public bool sel { get; set; } = false;
    }
    public class CacheGroupsModel
    {
        public int Version { get; set; }

        public List<Groups> GroupsList { get; set; }
    }
}
