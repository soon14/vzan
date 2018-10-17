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
    /// 代理信息表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentArea
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        [SqlField]
        public int ProvinceCode { get; set; } 
        
        public string ProvinceStr { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        [SqlField]
        public int CityCode { get; set; }

        public string CityStr { get; set; }

        /// <summary>
        /// 区/县
        /// </summary>
        [SqlField]
        public int AreaCode { get; set; }

        public string AreaStr { get; set; }

        /// <summary>
        /// 代理商Id
        /// </summary>
        [SqlField]
        public int AgentId { get; set; } = 0;
    }
}
