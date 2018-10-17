using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 小程序更新任务表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class MiniappUploadTask
    {
        ///<summary>
        /// auto_increment
        /// </summary>		
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        ///<summary>
        /// 小程序模板类型名称
        /// </summary>		
        [SqlField]
        public string Name { get; set; }

        [SqlField]
        public int TId { get; set; }

        [SqlField]
        public string Desc { get; set; }

        [SqlField]
        public DateTime AddTime { get; set; }


        [SqlField]
        public DateTime UpdateTime { get; set; }


        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 要跟新的状态
        /// </summary>
        [SqlField]
        public int UState { get; set; }
        /// <summary>
        /// 要跟新的版本
        /// </summary>
        [SqlField]
        public string version { get; set; }
        
    }
}
