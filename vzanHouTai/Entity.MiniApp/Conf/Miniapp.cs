using Entity.Base;
using Entity.MiniApp.Qiye;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 小程序列表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Miniapp
    {
        public Miniapp() { }

        /// <summary>
        /// 小程序ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }


        /// <summary>
        /// 商家名称
        /// </summary>
        [SqlField]
        public string StoreName { get; set; }

        /// <summary>
        /// 链接地址
        /// </summary>
        [SqlField]
        public string Linkurl { get; set; }

        /// <summary>
        /// 图标地址
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [SqlField]
        public string Description { get; set; }

        /// <summary>
        /// 状态（0,1）
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 菜单地址
        /// </summary>
        [SqlField]
        public string MenuLink { get; set; }

        /// <summary>
        /// 所属者
        /// </summary>
        [SqlField]
        public string OpenId { get; set; }
        /// <summary>
        /// 隐藏的模块Id
        /// </summary>
        [SqlField]
        public string hiddenModel { get; set; }
        [SqlField]
        public string ModelId { get; set; }
        /// <summary>
        /// 关联小程序权限表
        /// </summary>
        [SqlField]
        public int xcxRelationId { get; set; }
        /// <summary>
        /// 简述
        /// </summary>
        [SqlField]
        public string Sketch { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        [SqlField]
        public string Phone { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [SqlField]
        public string Address { get; set; }
        /// <summary>
        /// 坐标
        /// </summary>
        [SqlField]
        public string Location { get; set; }

        public List<string> BannersImgUrls { get; set; }

        public List<QiyeKeFu> ListKeFu { get; set; }

    }

    public class QiyeKeFu
    {
        public int EmployeeId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }

        public string Avatar { get; set; }

    }

}
