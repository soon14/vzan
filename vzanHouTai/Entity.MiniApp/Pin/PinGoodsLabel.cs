using Entity.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Utility;

namespace Entity.MiniApp.Pin
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinGoodsLabel
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "名称不能为空")]
        [MinLength(length: 1, ErrorMessage = "名称最少1个字")]
        [MaxLength(length: 5, ErrorMessage = "名称最多5个字")]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 排序，倒序排
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 99;

        /// <summary>
        /// 状态
        /// 1=启用，0=禁用，-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        public bool sel { get; set; } = false;
    }
}
