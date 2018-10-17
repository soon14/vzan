using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Ent
{

    /// <summary>
    /// 当前分销员与上级分销员佣金关系
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SalesManRelation
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int Aid { get; set; } = 0;

        /// <summary>
        /// 分销员对应的UserId
        /// </summary>
        [SqlField]
        public int UserId { get; set; } = 0;


        /// <summary>
        /// 上级分销员Id
        /// </summary>
        [SqlField]
        public int ParentSaleManId { get; set; } = 0;




        /// <summary>
        /// 上级分销员所得佣金 渠道佣金
        /// </summary>
        [SqlField]
        public int Price { get; set; } = 0;


        /// <summary>
        /// 佣金结算类型  0表示人工结算 1表示自动结算
        /// </summary>
        [SqlField]
        public int AutoSettle { get; set; } = 0;

        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

    }

    /// <summary>
    /// 我的下级分销展示模型
    /// </summary>
    public class ViewSalesManRelation
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        /// <summary>
        /// 分给上级分销员的佣金
        /// </summary>
        public string CpsPrice { get; set; } = "0.00";

    }

}
