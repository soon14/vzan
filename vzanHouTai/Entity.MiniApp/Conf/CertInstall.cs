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
    /// 商户安装证书表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CertInstall
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public string Mc_Id { get; set; } 
        [SqlField]
        public int Aid { get; set; }
        [SqlField]
        public int State { get; set; }
        [SqlField]
        public string Name { get; set; }
        [SqlField]
        public string Password { get; set; }
        [SqlField]
        public DateTime AddTime { get; set; }
        [SqlField]
        public DateTime UpdateTime { get; set; }
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        [SqlField]
        public string ErrorMsg { get; set; }
        /// <summary>
        /// 枚举ProjectType
        /// </summary>
        [SqlField]
        public int ProjectType { get; set; }
        [SqlField]
        public string AppId { get; set; }
    }
}
