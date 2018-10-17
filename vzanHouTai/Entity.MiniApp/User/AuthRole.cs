using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    [SqlTable(dbEnum.MINIAPP)]
    public class AuthRole
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public int AId { get; set; }
        [SqlField]
        public int CreateUserId { get; set; }
        [SqlField]
        public int GroupId { get; set; }
        [SqlField]
        public int State { get; set; }
        [SqlField]
        public DateTime AddTime { get; set; }
        [SqlField]
        public string Name { get; set; } = string.Empty;
        [SqlField]
        public string LoginName { get; set; } = string.Empty;
        [SqlField]
        public string Password { get; set; } = string.Empty;
        [SqlField]
        public string Remark { get; set; } = string.Empty;
        [SqlField]
        public DateTime LastLogin { get; set; }
        public string GroupName { get; set; }
        public string CreateUserName { get; set; }
    }
}
