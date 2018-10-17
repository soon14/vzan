using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    /// <summary>
    /// 权限表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Role
    {
        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true,IsAutoId = true)]
        public int ID { get; set; }

        /// <summary>
        /// 小程序权限ID  - 不设定为公共权限
        /// </summary>
        [SqlField]
        public int AppId { get; set; }

        /// <summary>
        /// 门店号  - 不设定为公共权限
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

        /// <summary>
        /// 权限说明
        /// </summary>
        [SqlField]
        public string RoleName { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }
    }
}
