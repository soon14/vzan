using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.Base;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 附件
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class C_Attachment
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        [SqlField]
        public int itemId { get; set; }
        /// <summary>
        ///  来源类型   商户轮播图 = 0,商户Logo图 = 1,公司介绍图3
        /// </summary>
        [SqlField]
        public int itemType { get; set; }
        [SqlField]
        public int imgwith { get; set; }
        [SqlField]
        public int imgheight { get; set; }
        //图片大小/语音时长
        [SqlField]
        public int imgsize { get; set; }
        [SqlField]
        public string postfix { get; set; }
        [SqlField]
        public string filepath { get; set; }
        [SqlField]
        public string thumbnail { get; set; }
        [SqlField]
        public DateTime createDate { get; set; }
        /// <summary>
        /// 状态 0正常 -1 删除
        /// </summary>
        [SqlField]
        public int status { get; set; }
        //语音本地id
        [SqlField]
        public string VoiceServerId { get; set; }
        /// <summary>
        /// 分销同城Id
        /// </summary>
        [SqlField]
        public int CitySubId { get; set; } = 0;
    }
}
