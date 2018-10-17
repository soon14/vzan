using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Qiye
{
    /// <summary>
    /// 企业部门表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class QiyeDepartment
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 小程序appId 
        /// </summary>
        [SqlField]
        public int Aid { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }


        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }


        public string UpdateTimeStr
        {
            get
            {
                return UpdateTime.ToString("yyyy-MM-dd HH:mm");
            }
        }

        /// <summary>
        /// 状态 0为正常 -1删除
        /// </summary>
        [SqlField]
        public int State { get; set; }

    }
}
