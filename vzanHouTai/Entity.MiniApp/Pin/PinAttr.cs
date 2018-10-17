using Entity.Base;
using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Entity.MiniApp.Pin
{
    /// <summary>
    /// 属性类型
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinAttr
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 属性类型名称
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "名称不能为空")]
        [MinLength(length: 1, ErrorMessage = "名称最少1个字")]
        [MaxLength(length: 10, ErrorMessage = "名称最多10个字")]
        public string name { get; set; } = string.Empty;


        /// <summary>
        /// 状态 1=启用，0=禁用，-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        [SqlField]
        public int sort { get; set; } = 99;
        /// <summary>
        /// 属性个数
        /// </summary>

        [SqlField]
        public string img { get; set; } = string.Empty;

        [SqlField]
        public int fId { get; set; } = 0;

        public bool sel { get; set; } = false;
    }
}
