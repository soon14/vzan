using Entity.Base;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.PlatChild;
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
    /// 会员等级表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class VipLevel
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        [SqlField]
        public string appId { get; set; } = string.Empty;
        /// <summary>
        /// 会员等级名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 会员等级 0：默认会员等级，无法删除  1:用户自定义添加
        /// </summary>
        [SqlField]
        public int level { get; set; } = 0;

        /// <summary>
        /// 会员卡封面背景
        /// </summary>
        [SqlField]
        public string bgcolor { get; set; } = string.Empty;

        /// <summary>
        /// 状态 0:正常 -1:删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 优惠类型: 0:无折扣 1:全场折扣 2:部分折扣
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;

        /// <summary>
        /// 部分折扣商品id
        /// </summary>
        [SqlField]
        public string gids { get; set; } = string.Empty;

        /// <summary>
        /// 折扣
        /// </summary>
        [SqlField]
        public int discount { get; set; } = 100;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updatetime { get; set; }
        public string showtime
        {
            get
            {
                return updatetime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 电商版 部分折扣商品列表
        /// </summary>
        public List<StoreGoods> goodslist { get; set; }

        /// <summary>
        /// 餐饮版 部分折扣商品列表
        /// </summary>
        public List<FoodGoods> foodgoodslist { get; set; }


        public List<EntGoods> entGoodsList { get; set; }
        /// <summary>
        /// 平台子模版
        /// </summary>
        public List<PlatChildGoods> PlatChildGoodsList { get; set; }



    }

}
