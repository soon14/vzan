using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.Base;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class log4netsyslog
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        [SqlField]
        public string ThreadId { get; set; } = string.Empty;

        [SqlField]
        public string Level { get; set; } = string.Empty;

        [SqlField]
        public string Logger { get; set; } = string.Empty;

        [SqlField]
        public string MsgBody { get; set; } = string.Empty;

        [SqlField]
        public string MsgException { get; set; } = string.Empty;

        [SqlField]
        public string Host { get; set; } = string.Empty;
    }
}
