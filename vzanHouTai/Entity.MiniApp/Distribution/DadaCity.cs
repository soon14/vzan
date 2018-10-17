using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    /// <summary>
    /// 达达城市表
    /// </summary>
    public class DadaCity
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 城市名
        /// </summary>
        [SqlField]
        public string cityName { get; set; }
        /// <summary>
        /// 城市code
        /// </summary>
        [SqlField]
        public string cityCode { get; set; }
    }
}
