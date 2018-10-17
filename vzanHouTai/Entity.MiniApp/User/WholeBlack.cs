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
    /// 全局黑名单
    /// </summary>
    [SqlTable(dbEnum.QLWL)]
    public class WholeBlack
    {
        [SqlField(IsPrimaryKey = true,IsAutoId =true)]
        public int id
        {
            get;
            set;
        }
        /// <summary>
        /// 黑名单类型
        /// </summary>
        [SqlField]
        public int BlackType
        {
            get;
            set;
        }
        /// <summary>
        /// Openid
        /// </summary>
        [SqlField]
        public string Openid
        {
            get;
            set;
        }
        /// <summary>
        /// 黑名单备注
        /// </summary>
        [SqlField]
        public string Note
        {
            get;
            set;
        }

        [SqlField]
        public string NickName
        {
            get;
            set;
        }
        [SqlField]
        public DateTime AddTime { get; set; }
    }
}
