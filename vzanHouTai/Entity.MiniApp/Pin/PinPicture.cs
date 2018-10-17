using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Pin
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinPicture
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        [SqlField]
        public int aid { get; set; } = 0;

        /// <summary>
        /// 图片路径
        /// </summary>
        [SqlField]
        public string img { get; set; } = string.Empty;

        /// <summary>
        /// 跳转功能链接 1:拼团商品 2:入驻申请
        /// </summary>
        [SqlField]
        public int funId { get; set; } = 1;

        /// <summary>
        /// 指向目标
        /// </summary>
        [SqlField]
        public string target { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 99;

        /// <summary>
        /// 图片类型：0：首页广告
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;

        /// <summary>
        /// 状态 -1:删除 0:正常
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;

        public string addTimeStr
        {
            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public FunModel funModel { get; set; }
    }

    public class FunModel
    {
        public FunModel(int id, string name, string page)
        {
            this.id = id;
            this.name = name;
            this.page = page;
        }

        public int id { get; set; } = 0;

        /// <summary>
        /// 功能名称
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 页面路径
        /// </summary>
        public string page { get; set; } = string.Empty;
    }
}