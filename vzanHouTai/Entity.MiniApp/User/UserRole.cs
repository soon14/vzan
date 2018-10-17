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
    public class UserRole
    {
        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int ID { get; set; }

        /// <summary>
        /// 小程序权限表ID  - 为 0/不填 代表作用所有appId
        /// </summary>
        [SqlField]
        public int AppId { get; set; }

        /// <summary>
        /// 店铺Id - 为 0/不填 代表作用所有StoreId
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

        /// <summary>
        /// 用户ID   - 关联 Account 表ID
        /// </summary>
        [SqlField]
        public Guid UserId { get; set; }

        /// <summary>
        /// 权限号
        /// </summary>
        [SqlField]
        public int RoleId { get; set; }

        /// <summary>
        /// 新增日期
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// 状态 0无效 1有效
        /// </summary>
        [SqlField]
        public int State { get; set; }

    }
}
