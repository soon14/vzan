using System; 
using Entity.Base; 

using Utility;

/// <summary>
/// Member转移过来的
/// </summary>
namespace Entity.MiniApp.User
{
    /// <summary>
    /// Account数据关联表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public   class AccountRelation
    {
        #region Model
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id{get;set;}
        
        [SqlField]
        public string AccountId{get;set;}
        
		[SqlField]
        public DateTime AddTime { get; set; }
        [SqlField]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 预存款
        /// </summary>
		[SqlField]
        public int Deposit { get; set; }
        #endregion Model 
    }
}