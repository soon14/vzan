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
    /// 代理分销关联表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentQrCode
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 分销代理商ID
        /// </summary>
        [SqlField]
        public int AgentId { get; set; }
        /// <summary>
        /// 二维码名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 父级分销代理商ID
        /// </summary>
        [SqlField]
        public int NumType { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// -2：永久删除，-1：停用，0或1：正常
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;
        public string StateStr
        {
            get
            {
                switch (State)
                {
                    case -2: return "已删除";
                    case -1: return "停用";
                    default: return "正常";
                }
            }
        }
        /// <summary>
        /// 二维码图片路径
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; }

        /// <summary>
        /// 绑定商户账号
        /// </summary>
        public string LoginId { get; set; }
        public int CustomerRelationId { get; set; }
        public int OpenExtension { get; set; }
        /// <summary>
        /// 客户数量
        /// </summary>
        public int OpenCount { get; set; }
    }
}
