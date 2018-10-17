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
    /// <summary>
    /// 全局管理员
    /// </summary>
    [SqlTable(dbEnum.QLWL)]
    public class WholeAdmin
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id
        {
            get; set;
        }

        /// <summary>
        /// Openid
        /// </summary>
        [SqlField]
        public string Openid
        {
            get; set;
        }

        [SqlField]
        public string NickName
        {
            get; set;
        }

        [SqlField]
        public string HeadImgUrl
        {
            get; set;
        }
    }
}
