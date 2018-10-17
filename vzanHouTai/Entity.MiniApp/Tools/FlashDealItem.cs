using Entity.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 秒杀物品（限时抢购）
    /// </summary>
    [SqlTable(dbEnum.MINIAPP)]
    public class FlashDealItem
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public int Aid { get; set; }
        /// <summary>
        /// 秒杀活动关联ID
        /// </summary>
        [SqlField]
        public int DealId { get; set; }
        /// <summary>
        /// 秒杀物品关联ID（产品ID等）
        /// </summary>
        [SqlField]
        public int SourceId { get; set; }
        /// <summary>
        /// 秒杀物品标题
        /// </summary>
        [SqlField]
        public string Title { get; set; }
        /// <summary>
        /// 秒杀物品原价
        /// </summary>
        [SqlField]
        public int OrigPrice { get; set; }
        /// <summary>
        /// 秒杀价格
        /// </summary>
        [SqlField]
        public int DealPrice { get; set; }
        /// <summary>
        /// 秒杀库存
        /// </summary>
        [SqlField]
        public int Stock { get; set; }
        /// <summary>
        /// 秒杀折扣（百分比）
        /// </summary>
        [SqlField]
        public int Discount { get; set; }
        /// <summary>
        /// 秒杀多规格
        /// </summary>
        [SqlField]
        public string Specs { get; set; }
        public List<FlashItemSpec> GetSpecs()
        {
            if (string.IsNullOrWhiteSpace(Specs))
            {
                return new List<FlashItemSpec>();
            }
            try
            {
                return JsonConvert.DeserializeObject<List<FlashItemSpec>>(Specs);
            }
            catch
            {
                return new List<FlashItemSpec>();
            }
        }
        /// <summary>
        /// -1：删除，0：停用（活动已结束），1：启用
        /// </summary>
        [SqlField]
        public int State { get; set; }
    }

    public class FlashItemForSelect
    {
        public int Id { get; set; } //主键唯一ID 
        public int SourceId { get; set; }//来源ID：比如产品表ID（必须）
        public string Title { get; set; }//标题（必须）
        public float OrigPrice { get; set; }//原价（必须）
        public float DealPrice { get; set; }//秒杀价（必须）
        public List<FlashItemSpec> Specs { get; set; }//多规格
        public bool disabled { get; set; }//是否可选（必须）
    }

    public class FlashItemSpec
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int DealPrice { get; set; }
        public int OrigPrice { get; set; }
    }

    /// <summary>
    /// 秒杀支付信息
    /// </summary>
    public class FlashDealPayInfo
    {
        /// <summary>
        /// 是否可支付
        /// </summary>
        public bool IsPay { get; set; }
        /// <summary>
        /// 支付信息
        /// </summary>
        public string Info { get; set; }
        /// <summary>
        /// 秒杀活动物品ID
        /// </summary>
        public int FlashItemId { get; set; } = 0;
        /// <summary>
        /// 秒杀活动ID
        /// </summary>
        public int FlashDealId { get; set; } = 0;
    }
}
