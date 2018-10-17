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
    public class CustomModelUserRelation
    {
        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true,IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 模型ID
        /// </summary>
        [SqlField]
        public int CustomModelId { get; set; }

        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int AId { get; set; }

        /// <summary>
        /// 上一个模型ID
        /// </summary>
        [SqlField]
        public int PreviousId { get; set; }
        /// <summary>
        /// setting数据
        /// </summary>
        [SqlField]
        public int SettingId { get; set; }
        
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
        
    }
}
