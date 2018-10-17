using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 页面设置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CustomModelEntSetting
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        public int AId { get; set; } = 0;
        /// <summary>
        /// 页面配置
        /// </summary>
        [SqlField]
        public string Pages { get; set; } = string.Empty;
        /// <summary>
        /// 页面创建时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 页面更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; } =DateTime.Now;

        /// <summary>
        /// 配置json string 串
        /// </summary>
        [SqlField]
        public string ConfigJson { get; set; }

        /// <summary>
        /// 同步小未案例 0:不同步 1：同步
        /// </summary>
        [SqlField]
        public int Syncmainsite { get; set; } = 0;
        
        [SqlField]
        public int StoreMateriaId { get; set; } = 0;

        public string MeConfigJson { get; set; } = Newtonsoft.Json.JsonConvert.SerializeObject(new EntNavItem { name = "我的", icon = "icon-personal4-33", img = "" });

    }
    
}
