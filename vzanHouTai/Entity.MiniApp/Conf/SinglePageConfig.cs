using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 单页小程序模板
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SinglePageConfig
    {
        public SinglePageConfig() { }
        /// <summary>
        /// ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 是否填写姓名
        /// </summary>
        [SqlField]
        public int IsName { get; set; }
        /// <summary>
        /// 是否填写联系方式
        /// </summary>
        [SqlField]
        public int IsPhone { get; set; } 
        /// <summary>
        /// 是否填写预约时间
        /// </summary>
        [SqlField]
        public int IsTime { get; set; } 

        /// <summary>
        /// 是否填写人数
        /// </summary>
        [SqlField]
        public int IsPersonCount { get; set; }
        /// <summary>
        /// 是否填写备注
        /// </summary>
        [SqlField]
        public int IsDesc { get; set; }
        /// <summary>
        /// 首页富文本备注
        /// </summary>
        [SqlField]
        public string PageStyle { get; set; }
        /// <summary>
        /// 关联权限表id
        /// </summary>
        [SqlField]
        public int Rid { get; set; }
        /// <summary>
        /// 客服电话
        /// </summary>
        [SqlField]
        public string CustomerPhone { get; set; }
    }
}
