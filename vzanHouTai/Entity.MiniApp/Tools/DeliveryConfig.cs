using Entity.Base;
using Entity.MiniApp.common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    [SqlTable(dbEnum.MINIAPP)]
    public class DeliveryConfig : BaseCustom
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public int BindId { get; set; }
        [SqlField]
        public int Type { get; set; }
        [SqlField]
        private string attr { get; set; }
        public DeliveryConfigAttr Attr => GetJsonModel<DeliveryConfigAttr>(attr);
        public void SetAttrbute(DeliveryConfigAttr attrbute)
        {
            attr = JsonConvert.SerializeObject(attrbute);
        }

        public string GetAttrbute()
        {
            return attr;
        }
    }

    public class DeliveryConfigAttr
    {
        public bool Enable { get; set; }
        public int FeeRule { get; set; }
        public int DeliveryTemplateId { get; set; }
        public int Weight { get; set; }
        public bool DiscountEnable { get; set; }
        public int Discount { get; set; }
    }
}
