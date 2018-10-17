using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 网页配置表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class WebConfig
    {
        public WebConfig()
        { }
        
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public long Id { get; set; }

        /// <summary>
        /// key 最长30个字符
        /// </summary>
        [SqlField]
        public string WebKey { get; set; }

        /// <summary>
        /// value
        /// </summary>
        [SqlField]
        public string WebValue { get; set; }

    }
}
