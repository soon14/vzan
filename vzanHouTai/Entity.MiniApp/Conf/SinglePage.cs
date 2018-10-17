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
    public class SinglePage
    {
        public SinglePage() { }
        /// <summary>
        /// ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 关联Id
        /// </summary>
        [SqlField]
        public int RelationId { get; set; }
        /// <summary>
        /// 状态（0,1）
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [SqlField]
        public string Phone { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [SqlField]
        public string Desc { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [SqlField]
        public string Content { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        [SqlField]
        public string CompnayName { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 预约时间
        /// </summary>
        [SqlField]
        public DateTime VisitorTime { get; set; }
        /// <summary>
        /// 预约人数
        /// </summary>
        [SqlField]
        public int PersonCount{ get; set; }
        /// <summary>
        /// 预约用户
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 预约用户名
        /// </summary>
        public string UserName { get; set; }
    }
}
