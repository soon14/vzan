using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Home
{
    /// <summary>
    ///Hfeedback:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class Hfeedback
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 用户名
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 公司名
        /// </summary>
        [SqlField]
        public string company { get; set; } = string.Empty;

        /// <summary>
        /// 手机号
        /// </summary>
        [SqlField]
        public string phone { get; set; } = string.Empty;

        /// <summary>
        /// 需求类型 1：直播，2：同城，3：论坛，4：有约，5：小程序
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;

        /// <summary>
        /// 详细需求
        /// </summary>
        [SqlField]
        public string descInfo { get; set; } = string.Empty;

        /// <summary>
        /// 回访状态
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 处理时间
        /// </summary>
        [SqlField]
        public DateTime dealdate { get; set; }

        /// <summary>
        /// 处理人
        /// </summary>
        [SqlField]
        public string dealperson { get; set; } = string.Empty;
        /// <summary>
        /// 数据来源 0：pc端  1：手机端
        /// </summary>
        [SqlField]
        public int datasource { get; set; } = 0;
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string remark { get; set; } = string.Empty;
    }
}
