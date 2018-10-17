using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;


namespace Entity.MiniApp.User
{
    /// <summary>
    /// 绑定模板消息账号
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class MsgAccount
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 管理者Guid
        /// </summary>
        [SqlField]
        public Guid ManagerGuid { get; set; }
        /// <summary>
        /// 代理ID
        /// </summary>
        [SqlField]
        public int Agentid { get; set; }
        [SqlField]
        public string OpenId { get; set; }
        [SqlField]
        public string UnionId { get; set; }
        [SqlField]
        public string NickName { get; set; }
        [SqlField]
        public string HeadImgUrl { get; set; }
        [SqlField]
        public string Sex { get; set; }
        [SqlField]
        public string Address { get; set; }
        /// <summary>
        /// 0:正常,-1:删除
        /// </summary>
        [SqlField]
        public int State { get; set; }
    }
}
