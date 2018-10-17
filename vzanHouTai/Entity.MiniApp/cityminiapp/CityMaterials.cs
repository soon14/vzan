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
    /// 同城模板 素材库
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CityMaterials
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
        public int aid { get; set; } = -1;//表示系统提供的 系统提供的叫素材库 用户上传的叫本地文件

        /// <summary>
        /// 小程序storeId
        /// </summary>
        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 文件路径
        /// </summary>
        [SqlField]
        public string materialPath { get; set; } = string.Empty;

        /// <summary>
        /// 素材文件类型 0→图片 1→视频 2→音频 3→文件
        /// </summary>
        [SqlField]
        public int materialType { get; set; } = 0;


        /// <summary>
        /// 状态 0→正常  -1→删除
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }

        public bool sel { get; set; } = false;
    }

    public class MaterialsItem
    {
        public int Id { get; set; }
        public string materialPath { get; set; }
    }

}
