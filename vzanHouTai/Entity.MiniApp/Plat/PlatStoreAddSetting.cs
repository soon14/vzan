using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatStoreAddSetting
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId   小程序
        /// </summary>
        [SqlField]
        public int Aid { get; set; }



        /// <summary>
        /// 入驻模式0表示免费 1表示收费
        /// </summary>
        [SqlField]
        public int AddWay { get; set; }


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
