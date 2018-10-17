using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 小程序企业版-产品信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SubStoreEntGoods 
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        [SqlField]
        public int Aid { get; set; } = 0;
        /// <summary>
        /// 产品ID
        /// </summary>
        [SqlField]
        public int Pid { get; set; } = 0;


        [SqlField]
        public int StoreId { get; set; } = 0;

        /// <summary>
        /// 产品规格，详情 保存规格+价格+库存
        /// </summary>
        [SqlField]
        public string SubSpecificationdetail { get; set; } = string.Empty;
        
        /// <summary>
        /// 状态
        /// 1：正常
        /// 0：删除
        /// </summary>
        [SqlField]
        public int SubState { get; set; } = 1;

        /// <summary>
        /// 0：下架
        /// 1：上架
        /// </summary>
        [SqlField]
        public int SubTag { get; set; } = 1;

        /// <summary>
        /// 门店库存，如果总店设置了不限库存此字段无效
        /// </summary>
        [SqlField]
        public int SubStock { get; set; } = 0;

        [SqlField]
        public DateTime SubUpdateTime { get; set; }

        [SqlField]
        public int SubSort { get; set; }
        /// <summary>
        /// 是否选中，前端辅助字段
        /// </summary>
        public bool sel { get; set; } = false;

        /// <summary>
        /// 商品销量 （足浴版用户实际销量）
        /// </summary>
        [SqlField]
        public int SubsalesCount { get; set; } = 0;

        public List<EntGoodsAttrDetail> GASDetailList => !string.IsNullOrEmpty(SubSpecificationdetail) ?JsonConvert.DeserializeObject<List<EntGoodsAttrDetail>>(SubSpecificationdetail) : new List<EntGoodsAttrDetail>();

    }

}
