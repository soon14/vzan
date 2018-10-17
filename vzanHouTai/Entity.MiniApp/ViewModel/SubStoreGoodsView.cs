using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.MiniApp.Model
{
    public class SubStoreGoodsView
    {
        public int Id { get; set; }=0;
        public int Aid { get; set; } = 0;
        public int Pid { get; set; } = 0;
        public int StoreId { get; set; } = 0;
        public string SubSpecificationdetail { get; set; } = string.Empty;
        public int SubState { get; set; } = 0;
        public int SubStock { get; set; } = 0;
        public int SubTag { get; set; } = 0;
        public int SubSort { get; set; } = 0;
        public int SubsalesCount { get; set; } = 0;
        public int goodid { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public string img { get; set; } = string.Empty;
        public Boolean showprice { get; set; } = true;
        public string ptypes { get; set; } = string.Empty;
        public string exttypes { get; set; } = string.Empty;
        public string exttypesstr { get; set; } = string.Empty;
        public string ptypestr { get; set; } = string.Empty;
        public int stock { get; set; } = 0;
        public bool stockLimit { get; set; } = true;
        public string plabels { get; set; } = string.Empty;
        public string plabelstr { get; set; } = string.Empty;
        public string specificationkeys { get; set; } = string.Empty;
        public string specification { get; set; } = string.Empty;
        public string specificationdetail { get; set; } = string.Empty;
        public string pickspecification { get; set; } = string.Empty;
        public int goodtype { get; set; } = 0;
        public float price { get; set; } = 0;
        public string unit { get; set; } = string.Empty;
        public string slideimgs { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public DateTime addtime { get; set; }
        public DateTime updatetime { get; set; }
        public Boolean showExtTypes { get; set; } = false;
        public int sort { get; set; } = 99;
        public int state { get; set; } = 1;
        public int tag { get; set; } = 1;
        public Boolean sel = false;
        /// <summary>
        /// 团购价
        /// </summary>
        public int groupPrice = 0;
        /// <summary>
        /// 原价
        /// </summary>
        public int originalPrice = 0;

    }
}