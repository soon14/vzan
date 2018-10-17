using Entity.MiniApp.Conf ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.ViewModel
{
    public class MiniAppStoreGoods
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public string GoodsName { get; set; }
        public int IsSell { get; set; }
        public int Price { get; set; }
        public string PriceStr {
            get
            {
                return (Price * 0.01).ToString("0.00");
            }
        }
    public int salesCount { get; set; }
        public int TypeId { get; set; }
    }
}
