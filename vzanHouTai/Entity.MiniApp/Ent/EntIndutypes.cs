using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 行业类型表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntIndutypes
    {
        public EntIndutypes() { }

        /// <summary>
        /// ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 父类ID
        /// </summary>
        [SqlField]
        public int ParentId { get; set; } = 0;
        /// <summary>
        /// 类型ID
        /// </summary>
        [SqlField]
        public int TypeId { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        [SqlField]
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 状态：大于等于0可用，小于0不可用
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// miniappentgoods表id
        /// </summary>
        [SqlField]
        public int AId { get; set; } = 0;

        /// <summary>
        /// 行业类型
        /// </summary>
        [SqlField]
        public string Industr { get; set; } = string.Empty;

        /// <summary>
        /// 显示类型(0：文字:1：图片:2：文字加图片)
        /// </summary>
        [SqlField]
        public int ShowType { get; set; }
        /// <summary>
        /// 图片路径
        /// </summary>
        [SqlField]
        public string Imgurl { get; set; } = string.Empty;
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; } = 1;
        /// <summary>
        /// 级别
        /// </summary>
        [SqlField]
        public int Level { get; set; } = 1;
        public int isopen { get; set; } = 0;
        /// <summary>
        /// 是否子参数
        /// </summary>
        public int childcount { get; set; } = 0;
        public bool sel { get; set; } = false;
    }

    public class IndutypesItem
    {
        public int PTypeId { get; set; }
        public int PVTypeId { get; set; }
        public string PKey { get; set; }
        public string PValue { get; set; }
    }
}
