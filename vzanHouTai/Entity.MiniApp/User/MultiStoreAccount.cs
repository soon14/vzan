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
    /// 多门店分店登录表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class MultiStoreAccount
    {
        public MultiStoreAccount() { }
        /// <summary>
        /// 多门店分店登录自增长ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 权限表Id
        /// </summary>
        [SqlField]
        public int AppId { get; set; } = 0;


        /// <summary>
        /// 商户号
        /// </summary>
        [SqlField]
        public string MerNo { get; set; } = string.Empty;

        /// <summary>
        /// 登录名
        /// </summary>
        [SqlField]
        public string LoginName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        [SqlField]
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 对应Account表的ID
        /// </summary>
        [SqlField]
        public Guid AccountId { get; set; } = Guid.Empty;

        /// <summary>
        /// 该用户创建者的Account表ID
        /// </summary>
        [SqlField]
        public Guid MasterAccountId { get; set; } = Guid.Empty;

        /// <summary>
        /// 记录创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 记录创建时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 记录是否有效 1激活  0未激活  -1删除
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

    }
}
