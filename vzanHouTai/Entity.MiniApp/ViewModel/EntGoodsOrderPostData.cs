using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.ViewModel
{
    /// <summary>
    /// 订单提交商品参数实体
    /// </summary>
    public class EntGoodsOrderPostData
    {
        public int goodsId { get; set; } = 0;

        public string goodsName { get; set; } = string.Empty;

        public string attrSpacStr { get; set; } = string.Empty;

        public string SpecInfo { get; set; } = string.Empty;

        public int qty { get; set; } = 0;

        public EntGoods goodsInfo { get; set; }
    }
}
