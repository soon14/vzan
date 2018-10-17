using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    /// <summary>
    ///平台 信息分类
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatMsgType
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        [SqlField]
        public string MaterialPath { get; set; } = string.Empty;


        /// <summary>
        /// 类别名称
        /// </summary>
        [SqlField]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 排序字段
        /// </summary>
        [SqlField]
        public int SortNumber { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }


        public string AddTimeStr
        {
            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        public string UpdateTimeStr
        {
            get
            {
                return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 状态 0→正常 -1已经删除
        /// </summary>
        [SqlField]
        public int State { get; set; }


        /// <summary>
        /// 是否显示编辑排序输入框
        /// </summary>
        public bool IsShowEditSort { get; set; } = false;

    }
}
