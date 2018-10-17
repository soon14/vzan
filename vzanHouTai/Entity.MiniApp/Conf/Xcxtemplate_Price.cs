using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 自定义小程序模板信息表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Xcxtemplate_Price
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 小程序模板id
        /// </summary>
        [SqlField]
        public string tid { get; set; } = string.Empty;

        /// <summary>
        /// 代理agentid
        /// </summary>
        [SqlField]
        public string agentid { get; set; } = string.Empty;
        [SqlField]
        /// <summary>
        /// 自定义价格
        /// </summary>
        public int price { get; set; } = 0;

        /// <summary>
        /// 模板状态
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        ///<summary>
        /// 多门店每增加一门店所付价格
        /// </summary>		
        [SqlField]
        public int SPrice { get; set; }
        ///<summary>
        /// 没开通一个多门店附带开通几家分门店
        /// </summary>		
        [SqlField]
        public int SCount { get; set; }
        ///<summary>
        /// 0:不限制添加数量，大于0：限制添加的数量
        /// </summary>		
        [SqlField]
        public int LimitCount { get; set; }


        /// <summary>
        /// 版本Id 目前点赞小程序专业版此字段有意义 0 旗舰版,1尊享版 ,2高级版,3基础版
        /// </summary>
        [SqlField]
        public int VersionId { get; set; }
    }
}
