using Entity.Base;
using Entity.MiniApp.Stores;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 拼团产品关联表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntGroupsRelation
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 产品ID
        /// </summary>
        [SqlField]
        public int EntGoodsId { get; set; } = 0;

        [SqlField]
        public int RId { get; set; } = 0;
        /// <summary>
        /// 团购价 单位分
        /// </summary>
        [SqlField]
        public int GroupPrice { set; get; } = 0;
        public string GroupPriceStr { get { return (GroupPrice / 100.00).ToString("0.00"); } }
        /// <summary>
        /// 单买价格 单位分
        /// </summary>
        public float SinglePrice { get; set; } = 0;
        public string SinglePriceStr { get { return SinglePrice.ToString("0.00"); } }
        /// <summary>
        /// 成团人数
        /// </summary>
        [SqlField]
        public int GroupSize { get; set; } = 2;
        /// <summary>
        /// 有效日期开始
        /// </summary>
        [SqlField]
        public DateTime ValidDateStart { set; get; }
        public string ValidDateStartStr { get { return ValidDateStart.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 有效日期结束
        /// </summary>
        [SqlField]
        public DateTime ValidDateEnd { set; get; }
        public string ValidDateEndStr { get { return ValidDateEnd.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 每人限购数量
        /// </summary>
        [SqlField]
        public int LimitNum { get; set; } = 0;
        /// <summary>
        /// 生成时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        ///// <summary>
        ///// 团长优惠金额
        ///// </summary>
        [SqlField]
        public int HeadDeduct { get; set; } = 0;
        
        public string HeadDeductStr { get { return (HeadDeduct / 100.00).ToString("0.00"); } }
        /// <summary>
        /// 状态，0：已下架，1：已上架，-1：已失效
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;
        /// <summary>
        /// 原价
        /// </summary>
        [SqlField]
        public int OriginalPrice { get; set; } = 0;

        /// <summary>
        /// 店铺ID（多门店）
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;

        public string OriginalPriceStr { get { return (OriginalPrice / 100.00).ToString("0.00"); } }

        /// <summary>
        /// 初始化销售量
        /// </summary>
        [SqlField]
        public int InitSaleCount { get; set; } = 0;

        /// <summary>
        /// 团有效时长（单位：时）
        /// </summary>
        [SqlField]
        public int ValidDateLength { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 拼团是否立即开始
        /// </summary>
        public bool IsConcern { get; set; }
        /// <summary>
        /// 生成数量
        /// </summary>
        public int CreateNum { get; set; } = 0;
        /// <summary>
        /// 已团数量
        /// </summary>
        public int GroupsNum { get; set; } = 0;
        public bool sel { get; set; } = false;
        public string ImgUrl { get; set; } = string.Empty;
        /// <summary>
        /// 已团几件
        /// </summary>
        public int SaleNum { get; set; } = 0;

        public int salesCount { get; set; } = 0;
        //虚拟销量
        public int virtualSalesCount { get; set; } = 0;

        /// <summary>
        /// 参加过该拼团的用户信息
        /// </summary>
        public List<object> GroupUserList { get; set; }
        public List<EntGroupSponsor> GroupSponsorList { get; set; }
    }
}
