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
    /// --用户信息表实体类
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class OAuthUser_Master
    {
        public OAuthUser_Master()
        { }
        /// <summary>
        /// --用户的唯一标识
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { set; get; }

        [SqlField]
        public string OpenId { get; set; }

        [SqlField]
        public int MinisnsId { get; set; }

        [SqlField]
        public int UserLevel { get; set; }

        [SqlField]
        public string unionid { get; set; } = string.Empty;//微信用户Unionid


        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;

    }
}
