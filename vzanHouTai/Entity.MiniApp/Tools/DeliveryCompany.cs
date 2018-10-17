using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 物流公司组件
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DeliveryCompany
    {
        [SqlField(IsPrimaryKey =true,IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 公司代码
        /// </summary>
        [SqlField]
        public string Code { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        [SqlField]
        public string Title { get; set; }
        /// <summary>
        /// 分类排序（按拼音首字母）
        /// </summary>
        [SqlField]
        public string Sort { get; set; }
    }
}
