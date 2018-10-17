using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.CoreHelper
{
    public class AddressApi
    {
        public int status { get; set; }
        public string message { get; set; }
        public string request_id { get; set; }
        public List<LocationData> locations { get; set; }
        public AddressApilocation result { get; set; }
    }
    public class AddressApilocation
    {
        public LocationData location { get; set; }
        public object address { get; set; }
        public object formatted_addresses { get; set; }
        public AddressApiAddressComponent address_component { get; set; }
    }
    public class AddressApiAddressComponent
    {
        /// <summary>
        /// 国家
        /// </summary>
        public string nation { get; set; }
        /// <summary>
        /// 省 / 直辖市
        /// </summary>
        public string province { get; set; }
        /// <summary>
        /// 市 / 地级区 及同级行政区划
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 区，可能为空字串
        /// </summary>
        public string district { get; set; }
        /// <summary>
        /// 街道，可能为空字串
        /// </summary>
        public string street { get; set; }
        /// <summary>
        /// 门牌，可能为空字串
        /// </summary>
        public string street_number { get; set; }
    }
    public class LocationData
    {
        /// <summary>
        /// 纬度
        /// </summary>
        public double lat { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public double lng { get; set; }
    }
}
