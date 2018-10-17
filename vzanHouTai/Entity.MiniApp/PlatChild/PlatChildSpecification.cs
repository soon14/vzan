using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;

namespace Entity.MiniApp.PlatChild
{
    /// <summary>
    /// 产品规格
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class  PlatChildSpecification
    {
        /// <summary>
        /// 产品
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        
        /// <summary>
        /// ID
        /// </summary>
        [SqlField]
        public int AId { get; set; } = 0;
        
        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string Name { get; set; } = string.Empty;
        
     
        /// <summary>
        /// 父级ID
        /// </summary>
        [SqlField]
        public int ParentId { get; set; } = 0;

       
        /// <summary>
        /// 状态 1正常 0删除
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;


        public bool Sel { get; set; } = false;

    }
}
