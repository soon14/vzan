using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace User.MiniApp.Areas.PlatChild.Model
{
    public class TableItemModel
    {
        public string Name { get; set; }
        public string SpecInfo { get; set; }
        public int Price { get; set; }
        public string PriceStr { get { return (Price * 0.01).ToString("0.00"); } }
        public int originalPrice { get; set; }
        public int Count { get; set; }
    }
}