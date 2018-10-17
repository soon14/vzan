namespace Entity.MiniApp
{
    public class BaiduPoiGeocoderModel
    {
        public int status { get; set; }
        public BaiduPoiGeocoderl result { get; set; }
    }

    public class BaiduPoiGeocoderl
    {
        //public BaiduLocation location { get; set; }
        public string formatted_address { get; set; }
        public string business { get; set; }
        public BaiduAddressComponent addressComponent { get; set; }
        public string sematic_description { get; set; }
        public string cityCode { get; set; }
    }
    public class BaiduAddressComponent
    {
        public string country { get; set; }
        public int country_code { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string adcode { get; set; }
        public string street { get; set; }
        public string street_number { get; set; }
        public string direction { get; set; }
        public string distance { get; set; }
    }
}