using Entity.MiniApp.Conf;
using Entity.MiniApp.Fds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.MiniApp.Fds
{
    public class FoodOrderPostModel
    {
        public string cityname { get; set; }
        public string lat { get; set; }
        public string lnt { get; set; }
        public int distributionprice { get; set; }
        public int couponlogid { get; set; }
        public string goodCarIdStr { get; set; }
        public int isgroup { get; set; }
        public int groupid { get; set; }
        public int goodtype { get; set; }
        public FoodGoodsOrder FoodModel { get; set; }
    }
}