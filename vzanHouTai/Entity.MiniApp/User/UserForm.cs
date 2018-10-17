using System; 
using Entity.Base; 
using Utility;

namespace Entity.MiniApp.User
{
	[Serializable]
    /// <summary>
    ///用户参与的帖子表单
    /// </summary>
    [SqlTable(dbEnum.QLWL)]
    public partial class UserForm
    {
        #region Model
        [SqlField(IsPrimaryKey =true,IsAutoId =true)]
        public int id { get; set; } = 0;
        [SqlField]
        public int uid { get; set; } = 0;
        [SqlField]
        public int minisnsid { get; set; } = 0;
        [SqlField]
        public int formid { get; set; } = 0;
        [SqlField]
        public int aid { get; set; } = 0;
        [SqlField]
        public string content { get; set; } = string.Empty;
        [SqlField]
        public DateTime createtime { get; set; } = DateTime.Now;
        [SqlField]
        public int orderid { get; set; } = 0;
        [SqlField]
        public int orderstatus { get; set; } = 0;
        #endregion Model
    }
}