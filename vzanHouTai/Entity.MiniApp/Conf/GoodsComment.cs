using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 商品评论表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class GoodsComment
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int AId { get; set; }
        /// <summary>
        /// 门店ID，多门店时有用，存分店ID，总店为0
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }
        /// <summary>
        /// 状态-2：删除，-1：失效，1：正常
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;
        /// <summary>
        /// 商品ID
        /// </summary>
        [SqlField]
        public int GoodsId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        [SqlField]
        public string GoodsName { get; set; }
        /// <summary>
        /// 商品规格
        /// </summary>
        [SqlField]
        public string GoodsSpecification { get; set; }
        /// <summary>
        /// 微信昵称
        /// </summary>
        [SqlField]
        public string NickName { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 评论
        /// </summary>
        [SqlField]
        public string Comment { get; set; }
        /// <summary>
        /// 回复
        /// </summary>
        [SqlField]
        public string Replay { get; set; }
        /// <summary>
        /// 好评（2：好评，1：中评，0：差评）
        /// </summary>
        [SqlField]
        public int Praise { get; set; }
        public string PraiseStr { get {
                switch(Praise)
                {
                    case 0:return "差评";
                    case 1:
                        return "中评";
                    default:return "好评";
                }
            } }
        /// <summary>
        /// 物流评分
        /// </summary>
        [SqlField]
        public int LogisticsScore { get; set; }
        /// <summary>
        /// 服务评分
        /// </summary>
        [SqlField]
        public int ServiceScore { get; set; }
        /// <summary>
        /// 描述评分
        /// </summary>
        [SqlField]
        public int DescriptiveScore { get; set; }
        /// <summary>
        /// 是否隐藏（0：否，1：是）
        /// </summary>
        [SqlField]
        public bool Hidden { get; set; } = false;
        public string HiddenStr { get { return Hidden ? "是" : "否"; } }
        /// <summary>
        /// 是否匿名（0：否，1：是）
        /// </summary>
        [SqlField]
        public bool Anonymous { get; set; } = false;
        public string AnonymousStr { get { return Anonymous ? "是" : "否"; } }
        /// <summary>
        /// 商品类型，例如拼团商品，普通商品，砍价商品(EntGoodsType)
        /// </summary>
        [SqlField]
        public int Type { get; set; }
        /// <summary>
        /// (砍价BargainUser表ID，拼团商品、普通车商品用购物车ID)
        /// </summary>
        [SqlField]
        public int OrderId { get; set; }
        
        /// <summary>
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm"); } }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm"); } }
        /// <summary>
        /// 0：无图，1：有图
        /// </summary>
        [SqlField]
        public int HaveImg { get; set; }
        /// <summary>
        /// 商品价格
        /// </summary>
        [SqlField]
        public int GoodsPrice { get; set; }
        /// <summary>
        /// 商品图片
        /// </summary>
        [SqlField]
        public string GoodsImg { get; set; }


        /// <summary>
        /// 点赞数量
        /// </summary>
        [SqlField]
        public int Points { get; set; }
        /// <summary>
        /// 评论图片
        /// </summary>
        public List<C_Attachment> CommentImgs { get; set; } = new List<C_Attachment>();
        /// <summary>
        /// 用户是否已点赞
        /// </summary>
        public bool UserPoints { get; set; } = false;
        /// <summary>
        /// 用户头像
        /// </summary>
        public string HeadImgUrl { get; set; }
        /// <summary>
        /// 是否是默认评论
        /// </summary>
        [SqlField]
        public bool IsDefault { get; set; } = false;
    }
}
