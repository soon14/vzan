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
    public class PinAgentProtect
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;
        [SqlField]
        public int aid { get; set; } = 0;
        [SqlField]
        public int userId { get; set; } = 0;

        [SqlField]
        public int fuserId { get; set; } = 0;
        [SqlField]
        public int state { get; set; } = 0;
        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;
        public string addTimeStr
        {
            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}
