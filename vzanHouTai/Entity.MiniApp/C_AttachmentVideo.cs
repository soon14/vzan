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
    /// 视频附件
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class C_AttachmentVideo
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        [SqlField]
        public int itemId { get; set; }
        /// <summary>
        ///  来源类型   店铺介绍视频 = 0
        /// </summary>
        [SqlField]
        public int itemType { get; set; }
        [SqlField]
        public int videoWith { get; set; }
        [SqlField]
        public int videoHeight { get; set; }
        [SqlField]
        public int videoSize { get; set; }
        //封面图
        [SqlField]
        public string videoPosterPath { get; set; }
        //转码地址
        [SqlField]
        public string convertFilePath { get; set; }
        //物理来源地址
        [SqlField]
        public string sourceFilePath { get; set; }
        //网络地址
        [SqlField]
        public string networkFilePath { get; set; }
        [SqlField]
        public string videoTime { get; set; }
        [SqlField]
        public int cityCode { get; set; }
        [SqlField]
        public DateTime createDate { get; set; }
        [SqlField]
        public int status { get; set; }
        
    }
}
