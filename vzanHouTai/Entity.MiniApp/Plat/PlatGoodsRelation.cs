using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    /// <summary>
    /// 平台版店铺产品关系表 同步店铺里的产品
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatGoodsRelation
    {

        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId  所属平台
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        [SqlField]
        public int GoodsId { get; set; }



        /// <summary>
        /// 是否同步到平台状态 0 没有同步 1同步
        /// </summary>
        [SqlField]
        public int Synchronized { get; set; } = 0;

        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }


    }
}
