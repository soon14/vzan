using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Footbath
{
    public class FootbathClientModel
    {
        public int aid { get; set; } = 0;
        public string xcxName { get; set; } = string.Empty;
        public FootbathXcxRelation relation { get; set; }
    }
}
