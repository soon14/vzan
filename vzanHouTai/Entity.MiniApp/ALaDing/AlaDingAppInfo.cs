using Entity.Base;
using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Entity.MiniApp.Alading
{
    /// <summary>
    /// 属性类型
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AlaDingAppInfo
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        [SqlField]
        public string AppId { get; set; } = string.Empty;

        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;

        [SqlField]
        public string AppKey { get; set; } = string.Empty;

        [SqlField]
        public string Name { get; set; } = string.Empty;

        [SqlField]
        public string Logo { get; set; } = string.Empty;

    }
}
