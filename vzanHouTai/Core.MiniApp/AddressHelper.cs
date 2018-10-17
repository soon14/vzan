using DAL.Base;
using Entity.MiniApp.CoreHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Core.MiniApp
{
    public static class AddressHelper
    {
        private static string GetLocationApiCountKey = "TXMAPKEY_API";
        private static string GetLocationCacheCountKey = "TXMAPKEY_CACHE";
        public static AddressApi GetAddressByApi(string lng,string lat)
        {
            string key = ConfigurationManager.AppSettings["TXMAPKEY"];
            string location = lat + "," + lng;
            int count = 0;
            string result= RedisUtil.Get<string>(location);
            if (string.IsNullOrEmpty(result))
            {
                count = RedisUtil.Get<int>(GetLocationApiCountKey);
                count++;
                RedisUtil.Set<int>(GetLocationApiCountKey, count, TimeSpan.FromHours(24));
                result = HttpHelper.GetData($"http://apis.map.qq.com/ws/geocoder/v1/?location={location}&key={key}");
                if (!string.IsNullOrEmpty(result)&&result.Contains("query ok"))
                {

                    RedisUtil.Set<string>(location, result, TimeSpan.FromHours(24));
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<AddressApi>(result);
                }
                else
                {
                    return new AddressApi();
                }
            }
            else
            {
                count= RedisUtil.Get<int>(GetLocationCacheCountKey);
                count++;
                RedisUtil.Set<int>(GetLocationCacheCountKey, count, TimeSpan.FromHours(24));
                return Newtonsoft.Json.JsonConvert.DeserializeObject<AddressApi>(result);
            }
        }

        public static AddressApi GetLngAndLatByAddress(string address)
        {
            string key = ConfigurationManager.AppSettings["TXMAPKEY"];
            string result = HttpHelper.GetData($"http://apis.map.qq.com/ws/geocoder/v1/?address={address}&key={key}");
            if (!string.IsNullOrEmpty(result))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<AddressApi>(result);
            }
            return new AddressApi();
        }

        /// <summary>
        /// 坐标转换
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static AddressApi GetTranslate(string lat_lngs)
        {
            string key = ConfigurationManager.AppSettings["TXMAPKEY"];
            string result = HttpHelper.GetData($"https://apis.map.qq.com/ws/coord/v1/translate?locations={lat_lngs}&type=3&key ={key}");
            if (!string.IsNullOrEmpty(result))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<AddressApi>(result);
            }
            return new AddressApi();
        }
    }
}
