using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    [Serializable]
    /// <summary>
    ///ManagerInfo:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [SqlTable(dbEnum.QLWL)]
    public class ManagerInfo
    {
        [SqlField(IsPrimaryKey = true)]
        public int Id { get; set; }
        [SqlField]
        public string Forums { get; set; }
        [SqlField]
        public string ForumName { get; set; }
        [SqlField]
        public string OpenId { get; set; }
        [SqlField]
        public string UnionId { get; set; }
        [SqlField]
        public DateTime CreateDate { get; set; }
        [SqlField]
        public string NickName { get; set; }
        [SqlField]
        public string PhoneNum { get; set; }
        [SqlField]
        public int State { get; set; }
        public string Forumer { get; set; } = string.Empty;
    }
}
