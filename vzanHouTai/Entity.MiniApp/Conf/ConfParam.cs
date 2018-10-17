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
    /// 小程序模板数据表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ConfParam
    {
        /// <summary>
        /// 小程序配置参数表
        /// </summary>
        public ConfParam() { }
        /// <summary>
        /// 小程序ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 商家名称Id
        /// </summary>
        [SqlField]
        public string AppId { get; set; }
        /// <summary>
        /// 状态（0,1）
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        [SqlField]
        public string Title { get; set; }
        /// <summary>
        /// 参数名
        /// </summary>
        [SqlField]
        public string Param { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        [SqlField]
        public string Value { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [SqlField]
        public string Desc { get; set; }
        /// <summary>
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        
        [SqlField]
        public int RId { get; set; }
    }
}
