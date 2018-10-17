using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Utility;

namespace BLL.MiniApp
{
    public class C_AreaBLL : BaseMySql<C_Area>
    {
        #region 单例模式
        private static C_AreaBLL _singleModel;
        private static readonly object SynObject = new object();

        private C_AreaBLL()
        {

        }

        public static C_AreaBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new C_AreaBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 城市级别列表，pid，一级是0
        /// </summary>
        private const string cAreaKey = "carea_{0}";
        private const string cCityListKey = "carea_citylist";
        private readonly string _redis_areakey = "dz_arealist";
        //private readonly string _ak;
        //public C_AreaBLL()
        //{
        //    _ak = ConfigurationManager.AppSettings["BaiduAk"];
        //}

        public List<C_Area> GetListByCodes(string codes)
        {
            string sqlWhere = $"code in ({codes})";
            return base.GetList(sqlWhere);
        }

        /// <summary>
        /// 读取缓存区域数据
        /// </summary>
        /// <returns></returns>
        public List<C_Area> GetAreaRedisData()
        {
            List<C_Area> list = RedisUtil.Get<List<C_Area>>(_redis_areakey);
            if(list==null || list.Count<=0)
            {
                list = base.GetList("state>=0");
                if(list!=null && list.Count>0)
                {
                    RedisUtil.Set<List<C_Area>>(_redis_areakey, list);
                }
            }

            return list;
        }

        /// <summary>
        /// 根据区域码找区域名称
        /// </summary>
        /// <param name="areaCode"></param>
        /// <returns></returns>
        public string GetNameByAreaCode(int areaCode)
        {
            var area = GetModel(areaCode);
            return area?.Name;
        }
        public string GetFullNameByAreaCode(int areaCode)
        {
            //
            var result = string.Empty;
            var area = GetModel(areaCode);
            if (null == area) return result;
            switch (area.Level)
            {
                case 1:
                    result = area.Name;
                    break;
                case 2:
                    var parent = GetModel(area.ParentCode);
                    if (null == parent) return result;
                    result = parent.Name + area.Name;
                    break;
                case 3:
                    var parent1 = GetModel(area.ParentCode);
                    var parent2 = GetModel(parent1.ParentCode);
                    if (null == parent1 || null == parent2) return result;
                    result = parent2.Name + parent1.Name + area.Name;
                    break;
                default:
                    break;
            }
            return result;
        }
        /// <summary>
        /// 获取父集code
        /// </summary>
        /// <param name="areaCode"></param>
        /// <returns></returns>
        public int GetParentCityCode(int areaCode)
        {
            int citycode = 0;
            var area = GetModel(areaCode);
            if (area != null)
                citycode = area.ParentCode;
            return citycode;
        }


        /// <summary>
        /// 根据区域名称找区域码
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int? GetAreaCodeByName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var model = new C_Area { Name = name };
                var area = GetModel(model);
                if (area != null)
                {
                    return area.Code;
                }
            }
            return null;
        }

        public List<C_Area> GetByIds(string subAreas)
        {
            return GetList($"ParentCode in ({subAreas})");
        }

        /// <summary>
        /// 根据经纬度获取区域码
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        public int GetAreaCodeByPoint(double lat, double lng)
        {
            string ak = ConfigurationManager.AppSettings["BaiduAk"];
            var areaCode = 0;
            var url = "http://api.map.baidu.com/geocoder/v2/?ak=" + ak + "&location=" + lat + "," +
                      lng + "&output=json&pois=0";
            var result =  HttpHelper.GetData(url);
            var resultObj = JsonConvert.DeserializeObject<BaiduPoiGeocoderModel>(result);
            if (resultObj?.status == 0 && resultObj.result?.addressComponent?.adcode != null)
            {
                if (resultObj.result.cityCode.Trim().Length > 0 && resultObj.result.addressComponent.adcode != "0")
                {
                    return Convert.ToInt32(resultObj.result.addressComponent.adcode);
                }
            }
            else if (resultObj?.status == 302)
            {
                // {"status":302,"message":"天配额超限，限制访问"}
            }
            return areaCode;
        }


        /// <summary>
        /// 查找该区域的子区域  
        /// </summary>
        /// <param name="pCode">一级区域，输入0</param>
        /// <returns></returns>
        public List<C_Area> GetSubArea(int pCode)
        {
            string key = string.Format(cAreaKey, pCode);
            var sbuAreaList = RedisUtil.Get<List<C_Area>>(key);
            if (sbuAreaList == null || sbuAreaList.Count == 0)
            {
                string sqlWhere = "ParentCode=@ParentCode";
                var param = new List<MySqlParameter> {new MySqlParameter("@ParentCode", pCode)};
                sbuAreaList = GetListByParam(sqlWhere, param.ToArray(), 500, 1, "*", "sort desc");
                RedisUtil.Set(key, sbuAreaList);
            }
            return sbuAreaList;
        }

        private const double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
        /// <summary>
        /// 中国正常坐标系GCJ02协议的坐标，转到 百度地图对应的 BD09 协议坐标
        /// </summary>
        /// <param name="lat">维度</param>
        /// <param name="lng">经度</param>
        public static void Convert_GCJ02_To_BD09(ref double lat, ref double lng)
        {
            double x = lng, y = lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            lng = z * Math.Cos(theta) + 0.0065;
            lat = z * Math.Sin(theta) + 0.006;
        }

        /// <summary>
        /// 百度地图对应的 BD09 协议坐标，转到 中国正常坐标系GCJ02协议的坐标
        /// </summary>
        /// <param name="lat">维度</param>
        /// <param name="lng">经度</param>
        public static void Convert_BD09_To_GCJ02(ref double lat, ref double lng)
        {
            double x = lng - 0.0065, y = lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            lng = z * Math.Cos(theta);
            lat = z * Math.Sin(theta);
        }
         


        ///// <summary>
        ///// 获取可管理区域
        ///// </summary>
        ///// <param name="cityInfoId"></param>
        ///// <param name="currentCode"></param>
        ///// <returns></returns>
        //public List<SelectListItem> GetCanManageAreaList(string openId, int cityInfoId, int currentCode = 0)
        //{
        //    string sql = "select * from c_area where code in (select AreaCode from c_citysubarea where CityInfoId in (select id from c_cityinfo where id = " + cityInfoId + " and openid = '" + openId + "')) and LEVEL = 3";
        //    var areaList = GetListBySql(sql);
        //    return areaList.Select(area => new SelectListItem()
        //    {
        //        Selected = area.Code == currentCode,
        //        Value = area.Code.ToString(),
        //        Text = area.Name
        //    }).ToList();
        //}

        

        public List<SelectListItem> GetStreetItems(int parentCode, int currentCode = 0)
        {
            var streetList = GetSubArea(parentCode).ToList();
            return streetList.Select(m => new SelectListItem
            {
                Selected = m.Code == currentCode,
                Value = m.Code.ToString(),
                Text = m.Name
            }).ToList();
        }

        /// <summary>
        /// 过滤国外的省数据并返回下拉
        /// </summary>
        /// <param name="currentCode"></param>
        /// <returns></returns>
        public List<SelectListItem> GetStreetItemsInChina(int currentCode = 0)
        {
            //过滤国外省
            var streetList = GetSubArea(0).Where(x => x.Code < 900000).ToList();
            return streetList.Select(m => new SelectListItem
            {
                Selected = m.Code == currentCode,
                Value = m.Code.ToString(),
                Text = m.Name
            }).ToList();
        }

        /// <summary>
        /// 生成新增街道areacode
        /// </summary>
        /// <returns></returns>
        public int GetStreetCode(int ParentCode)
        {
            int streetcode = 0;
            for (int i = 1; i <= 99; i++)
            {
                streetcode = ParentCode * 100 + i;
                if (base.GetModel(streetcode) == null)
                {
                    return streetcode;
                }
                else
                {
                    continue;
                }
            }
            return 0;
        }


        /// <summary>
        /// 根据名称查找区域代码
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetCodeByName(string name)
        {
            C_Area c_Area = base.GetModel($"name ='{name}'");
            if (c_Area != null)
            {
                return c_Area.Code;
            }
            return 0;

        }

        /// <summary>
        /// 根据名称查找区域代码
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<C_Area> GetListByName(string provincename,string cityname,string countryName)
        {
            string sqlwhere = "";
            List<MySqlParameter> param = new List<MySqlParameter>();
            List<string> sqlarray = new List<string>();
            if(!string.IsNullOrEmpty(provincename))
            {
                param.Add(new MySqlParameter("@provincename", provincename));
                sqlarray.Add($" name like @provincename ");
            }
            if (!string.IsNullOrEmpty(cityname))
            {
                param.Add(new MySqlParameter("@cityname", cityname));
                sqlarray.Add($" name like @cityname ");
            }
            if (!string.IsNullOrEmpty(countryName))
            {
                param.Add(new MySqlParameter("@countryName", countryName));
                sqlarray.Add($" name like @countryName ");
            }

            sqlwhere = string.Join(" or ",sqlarray);
            if (string.IsNullOrEmpty(sqlwhere))
                return new List<C_Area>();

            List<C_Area> list = base.GetListByParam(sqlwhere,param.ToArray());
            return list;
        }

        public Dictionary<string,int> GetAddressCode(string provincename, string cityname, string countryName)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic["provincename"] = 0;
            dic["cityname"] = 0;
            dic["countryName"] = 0;
            List<C_Area> arealist = GetListByName(provincename, cityname, countryName);
            if(arealist!=null && arealist.Count>0)
            {
                C_Area province = arealist.Where(w=>w.Name.Contains(provincename))?.FirstOrDefault();
                C_Area city = arealist.Where(w => w.Name.Contains(cityname))?.FirstOrDefault();
                C_Area country = arealist.Where(w => w.Name.Contains(countryName))?.FirstOrDefault();
                
                dic["provincename"] = province!=null?province.Code:0;
                dic["cityname"] = city != null ? city.Code : 0;
                dic["countryName"] = country != null ? country.Code : 0;
            }

            return dic;
        }


        public override object Add(C_Area model)
        {
            object o = base.Add(model);
            RedisUtil.Set(cAreaKey, TimeSpan.FromHours(3));
            RemoveCache(model);
            return o;
        }
        public override bool Update(C_Area model)
        {
            bool b = base.Update(model);
            RemoveCache(model);
            return b;
        }
        public override bool Update(C_Area model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            RemoveCache(model);
            return b;
        }
        public override int Delete(int id)
        {
            C_Area area = base.GetModel(id);
            int result = base.Delete(id);
            RemoveCache(area);
            return result;
        }
        public void RemoveCache(C_Area model)
        {
            RedisUtil.Remove(string.Format(cAreaKey, model.ParentCode));
        }
    }
}
