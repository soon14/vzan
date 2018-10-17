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
    /// 代理商网站提交过来的咨询问题表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentWebSiteQuestion
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 代理accountid
        /// </summary>
        [SqlField]
        public string useraccountid { get; set; } = string.Empty;


        /// <summary>
        /// 咨询者电话号码
        /// </summary>
        [SqlField]
        public string telephone { get; set; } = string.Empty;


        /// <summary>
        /// 咨询者用户名
        /// </summary>
        [SqlField]
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 咨询问题内容
        /// </summary>
        [SqlField]
        public string question { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }

        public string addTimeStr
        {
            get
            {
                return addtime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

       
        
    }

   
}
