using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;
using Entity.MiniApp.Tools;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 资讯
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntNews
    {
        /// <summary>
        /// 资讯
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 标题
        /// </summary>
        [SqlField]
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        [SqlField]
        public string description { get; set; } = string.Empty;
        /// <summary>
        /// 图片
        /// </summary>
        [SqlField]
        public string img { get; set; } = string.Empty;
        /// <summary>
        /// 用来显示格式化的图片
        /// </summary>
        public string img_fmt { get; set; } = string.Empty;
        /// <summary>
        /// 视频
        /// </summary>
        [SqlField]
        public string video { get; set; } = string.Empty;
        public string audio { get; set; } = string.Empty;
        /// <summary>
        /// 分类
        /// </summary>
        [SqlField]
        public int typeid { get; set; } = 0;

        public string typename { get; set; } = string.Empty;
        
        /// <summary>
        /// 轮播图
        /// </summary>
        [SqlField]
        public string slideimgs { get; set; } = string.Empty;

        /// <summary>
        /// 格式化的轮播图
        /// </summary>
        public string slideimgs_fmt { get; set; } = string.Empty;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 状态
        /// 1：正常
        /// 0：删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        [SqlField]
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 是否付费内容
        /// </summary>
        [SqlField]
        public bool ispay { get; set; }
        /// <summary>
        /// 付费内容相关
        /// </summary>
        [SqlField]
        public int paycontent { get; set; }
        /// <summary>
        /// 内容类型
        /// </summary>
        [SqlField]
        public int contenttype { get; set; }
        /// <summary>
        /// 视频封面
        /// </summary>
        public string videocover { get; set; }
        
        /// <summary>
        /// 是否已支付购买
        /// </summary>
        public bool ispaid { get; set; }
        /// <summary>
        /// 前端辅助字段后台可以忽略
        /// </summary>
        public bool sel { get; set; } = false;

        /// <summary>
        /// 更新付费相关
        /// </summary>
        public PayContent updatecontent { get; set; }

        public PayContentPayment payinfo { get; set; }
        
        /// <summary>
        /// 支付价格
        /// </summary>
        public double amount { get; set; }

        /// <summary>
        /// 浏览量
        /// </summary>
        [SqlField]
        public int PV { get; set; } = 0;

        /// <summary>
        /// 虚拟访问量
        /// </summary>
        [SqlField]
        public int VirtualPV { get; set; } = 0;

        /// <summary>
        /// 推荐商品
        /// </summary>
        [SqlField]
        public string RecommendedItem { get; set; } = string.Empty;

        public List<EntGoods> GoodItem { get; set; }


        /// <summary>
        /// 排序字段
        /// </summary>
        [SqlField]
        public int SortNumber { get; set; }

        public bool IsShowEditSort = false;

    }
}
