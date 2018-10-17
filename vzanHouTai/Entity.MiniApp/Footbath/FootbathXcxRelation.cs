using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Footbath
{
    //足浴版客户端技师端小程序关系表
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FootbathXcxRelation
    {
        /// <summary>
        /// 自增id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 用户accountId
        /// </summary>
        [SqlField]
        public string accountId { get; set; } = string.Empty;
        /// <summary>
        /// 客户端小程序aid
        /// </summary>
        [SqlField]
        public int clientAid { get; set; } = 0;
       /// <summary>
       /// 技师端小程序aid
       /// </summary>
        [SqlField]
        public int technicianAid { get; set; } = 0;
        /// <summary>
        /// 状态 默认值：0
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        /// <summary>
        /// 关联的小程序名称
        /// </summary>
        public string clientXcxName { get; set; } = string.Empty;
    }
}
