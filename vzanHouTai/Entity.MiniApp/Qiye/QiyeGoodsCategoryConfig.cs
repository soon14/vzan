using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Qiye
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class QiyeGoodsCategoryConfig
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId   小程序
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// 开关设置
        /// </summary>
        [SqlField]
        public string SwitchConfig { get; set; } = string.Empty;


        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

    }

    public class QiyeSwitchModel
    {
       
        /// <summary>
        /// 产品类别级别 1表示1级只显示小类 样式按照小类样式 
        /// 2表示2级 按照二级类别样式显示 先显示大类的 然后再显示小类
        /// </summary>
        public int ProductCategoryLevel { get; set; } = 1;


    }


}
