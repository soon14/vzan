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
    /// 平台店铺类别配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatStoreCategoryConfig
    {

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 小程序appId 所属平台小程序
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// 级别
        /// </summary>
        [SqlField]
        public int Level { get; set; }

        /// <summary>
        /// 0 手动同步 1 自动同步 平台商品数据库自动同步铺商品标识开关(包括平台入驻的以及同步的店铺)
        /// </summary>
        [SqlField]
        public int SyncSwitch{ get; set; }


        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }



    }
}
