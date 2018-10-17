using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.cityminiapp
{
    /// <summary>
    /// 同城模板 信息类别表
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CityStoreMsgType
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
        public int aid { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        [SqlField]
        public string materialPath { get; set; } = string.Empty;


        /// <summary>
        /// 类别名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 排序字段
        /// </summary>
        [SqlField]
        public int sortNumber { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }


        public string addTimeStr
        {
            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }
        public string updateTimeStr
        {
            get
            {
                return updateTime.ToString("yyyy-MM-dd HH:mm:ss");
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
        public bool isShowEditSort { get; set; } = false;

    }
}
