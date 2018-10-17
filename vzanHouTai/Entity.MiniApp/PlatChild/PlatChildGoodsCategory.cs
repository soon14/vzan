using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.PlatChild
{
    /// <summary>
    ///平台子小程序 店铺产品类别
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatChildGoodsCategory
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
        public int AId { get; set; }

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
        /// 父级Id
        /// </summary>
        [SqlField]
        public int ParentId { get; set; }

        public string ParentName { get; set; }
        /// <summary>
        /// 是否显示编辑排序输入框
        /// </summary>
        public bool IsShowEditSort { get; set; } = false;

        /// <summary>
        /// 是否选中 前端用
        /// </summary>
        public bool Sel { get; set; }

    }
}
