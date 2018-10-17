using Entity.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Bargain
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 商品名称，标题
        /// </summary>
        [SqlField]
        public string BName { set; get; }
        /// <summary>
        /// 缩略图
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; }

        public string ImgUrl_thumb { get; set; } = string.Empty;

        // <summary>
        /// 描述
        /// </summary>
        [SqlField]
        public string Description { set; get; }
        /// <summary>
        /// 原价 单位分
        /// </summary>
        [SqlField]
        public int OriginalPrice { set; get; }

        /// <summary>
        /// 原价 单位 元
        /// </summary>
        public string OriginalPriceStr { get { return (OriginalPrice * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 底价 单位分
        /// </summary>
        [SqlField]
        public int FloorPrice { get; set; }

        /// <summary>
        /// 底价 单位 元
        /// </summary>
        public string FloorPriceStr { get { return (FloorPrice * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 最大减价 单位分
        /// </summary>
        [SqlField]
        public int ReduceMax { get; set; }

        /// <summary>
        /// 最大减价 单位 元
        /// </summary>
        public string ReduceMaxStr { get { return (ReduceMax * 0.01).ToString("0.00"); } }
        /// <summary>
        /// <summary>
        /// 最少减价 单位分
        /// </summary>
        [SqlField]
        public int ReduceMin { get; set; }

        /// <summary>
        /// 最小减价 单位 元
        /// </summary>
        public string ReduceMinStr { get { return (ReduceMin * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 生成数量
        /// </summary>
        [SqlField]
        public int CreateNum { get; set; }
        /// <summary>
        /// 剩余数量
        /// </summary>
        [SqlField]
        public int RemainNum { get; set; }
        /// <summary>
        /// 自己砍价间隔小时数
        /// </summary>
        [SqlField]
        public int IntervalHour { get; set; }
        /// <summary>
        /// 开始日期
        /// </summary>
        [SqlField]
        public DateTime StartDate { set; get; }

        public string startDateStr
        {
            get
            {
                return StartDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 结束日期
        /// </summary>
        [SqlField]
        public DateTime EndDate { set; get; }

        public string endDateStr
        {
            get
            {
                return EndDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 领奖时间
        /// </summary>
        [SqlField]
        public DateTime ValidDate { get; set; }
        /// <summary>
        /// 领奖地址
        /// </summary>
        [SqlField]
        public string ValidAddress { get; set; }
        /// <summary>
        /// 领奖电话
        /// </summary>
        [SqlField]
        public string ValidPhone { get; set; }
        /// <summary>
        /// 关联的店铺ID或者专业版Id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;

        /// <summary>
        /// 生成时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        public string createDateStr
        {
            get
            {
                return CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
            }

        }
        /// <summary>
        /// 0正常发布 -1下架
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;


        /// <summary>
        /// 砍价商品状态
        /// </summary>
        public string stateStr
        {
            get
            {
                if (DateTime.Now > EndDate || RemainNum == 0)
                {
                    return "已失效";
                }
                else if (DateTime.Now < StartDate && RemainNum > 0)
                {
                    return "未开始";
                }
                else
                {
                    return "进行中";
                }
            }
           
        }

        /// <summary>
        /// 浏览量
        /// </summary>
        [SqlField]
        public int BrowseCount { get; set; } = 0;
        /// <summary>
        /// 分享数
        /// </summary>
        [SqlField]
        public int ShareCount { get; set; } = 0;

        /// <summary>
        /// 是否结束 已过期或者剩余数量为0  0进行中 1已经结束 2未开始
        /// </summary>
        [SqlField]
        public int IsEnd { get; set; } = 0;
        /// <summary>
        /// 运费 单位为分 
        /// </summary>
        [SqlField]
        public int GoodsFreight { get; set; } = 0;

        /// <summary>
        /// 是否删除  -1删除 0未删除
        /// </summary>
        [SqlField]
        public int IsDel { get; set; } = 0;


        /// <summary>
        /// 砍价产品类型 0→电商版 1→专业版
        /// </summary>
        [SqlField]
        public int BargainType { get; set; } = 0;

        /// <summary>
        /// 初始化销售量
        /// </summary>
        [SqlField]
        public int InitSaleCount { get; set; } = 0;

        /// <summary>
        /// 运费 单位 元
        /// </summary>
        public string GoodsFreightStr { get { return (GoodsFreight * 0.01).ToString("0.00"); } }

        public List<C_Attachment> ImgList { get; set; }
        public List<C_Attachment> DescImgList { get; set; }
        /// <summary>
        /// 商品被领取记录
        /// </summary>
        public List<BargainUser> BargainUserList { get; set; }

        /// <summary>
        /// 该砍价商品已领取人数也就是参与砍价该商品的人数
        /// </summary>
        public int BargainUserNumber { get; set; }

        /// <summary>
        /// 帮砍记录
        /// </summary>
        public List<BargainRecordList> BargainRecordUserList { get; set; }
        

        /// <summary>
        /// 背景音乐
        /// </summary>
        public string VoicePath { get; set; } = string.Empty;
        /// <summary>
        /// 视频
        /// </summary>
        public string VideoPath { get; set; } = string.Empty;

        public bool sel { get; set; } = false;
    }
}
