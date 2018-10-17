using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class PayTimeOutOrder
    {
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int id { get; set; }
        [SqlField]
        public string transaction_id { get; set; }
        [SqlField]
        public int state { get; set; } = 0;
    }
}
