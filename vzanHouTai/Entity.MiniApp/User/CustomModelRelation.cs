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
    /// 自定义模型关联表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CustomModelRelation
    {
        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true,IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 装修模板名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int AId { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [SqlField]
        public string Desc { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 修改日期
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }

        /// <summary>
        /// -1：删除，0：未发布，1：已发布
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        [SqlField]
        public int DataType { get; set; } = 0;

        public int CustommodelId { get; set; }
        public int VersionId { get; set; }
        public string VersionIdImg { get; set; }
    }
}
