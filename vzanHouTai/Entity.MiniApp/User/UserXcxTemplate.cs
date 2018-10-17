using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 用户上传小程序代码表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class UserXcxTemplate
    {
        ///<summary>
        /// auto_increment
        /// </summary>		
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        ///<summary>
        /// 小程序模板Id
        /// </summary>		
        [SqlField]
        public int TId { get; set; }
        ///<summary>
        /// 小程序名称
        /// </summary>		
        [SqlField]
        public string Name { get; set; }
        ///<summary>
        /// 小程序原始Id
        /// </summary>		
        [SqlField]
        public string TuserId { get; set; }
        ///<summary>
        /// 同城code
        /// </summary>		
        [SqlField]
        public int AreaCode { get; set; }
        ///<summary>
        /// 小程序Appid
        /// </summary>		
        [SqlField]
        public string AppId { get; set; }
        ///<summary>
        /// 小程序秘钥
        /// </summary>		
        [SqlField]
        public string Appsr { get; set; }
        ///<summary>
        /// 状态
        /// </summary>		
        [SqlField]
        public int State { get; set; }
        ///<summary>
        /// 修改时间
        /// </summary>		
        [SqlField]
        public DateTime UpdateTime { get; set; }
        ///<summary>
        /// 添加时间
        /// </summary>		
        [SqlField]
        public DateTime AddTime { get; set; }
        
        ///<summary>
        /// 版本号
        /// </summary>		
        [SqlField]
        public string Version { get; set; }
        ///<summary>
        /// 小程序审核Id
        /// </summary>		
        [SqlField]
        public int Auditid { get; set; }
        ///<summary>
        /// 原因
        /// </summary>		
        [SqlField]
        public string Reason { get; set; }
        ///<summary>
        /// 
        /// </summary>		
        [SqlField]
        public string access_token { get; set; }
        ///<summary>
        /// 过期时间
        /// </summary>		
        [SqlField]
        public DateTime token_time { get; set; }
        ///<summary>
        /// 有效时间
        /// </summary>		
        [SqlField]
        public int expires_in { get; set; }



        public string XName { get; set; }
        public string TName { get; set; }
        public string Desc { get; set; }

    }
}
