using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Friend;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.MiniApp
{
    /// <summary>
    /// 区域
    /// </summary>
    public class AreaRegionBLL : BaseMySql<AreaRegion>
    {
        #region 单例模式
        private static AreaRegionBLL _singleModel;
        private static readonly object SynObject = new object();

        private AreaRegionBLL()
        {

        }

        public static AreaRegionBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AreaRegionBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 获取省市区
        /// </summary>
        public List<AreaRegion> listAreaRegion
        {
            get
            {
                List<AreaRegion> _list = RedisUtil.Get<List<AreaRegion>>(FCacheKey.FAreaRegion.ToString());
                if (_list == null)
                {
                    _list = base.GetList(string.Empty, 10000, 1);
                    RedisUtil.Set<List<AreaRegion>>(FCacheKey.FAreaRegion.ToString(), _list, TimeSpan.FromHours(1));
                }
                return _list;
            }
        }

        //public Dictionary<int, AreaRegion> dicAreaRegion
        //{
        //    get
        //    {
        //        return listAreaRegion.ToDictionary(p => p.AreaCode);
        //    }
        //}

        /// <summary>
        /// 获取省份列表
        /// </summary>
        /// <returns></returns>
        public List<AreaRegion> GetProvinceList()
        {
            return GetChildAreaList(0);
        }

        /// <summary>
        /// 获取子区域
        /// </summary>
        /// <param name="parentCode">父区域Code</param>
        /// <returns></returns>
        public List<AreaRegion> GetChildAreaList(int parentCode)
        {
            return listAreaRegion.Where(p => p.ParentCode == parentCode).ToList();
        }

        /// <summary>
        /// 根据areaCode获取全路径
        /// </summary>
        /// <param name="areaCode"></param>
        /// <returns></returns>
        //public string GetFullAddress(int areaCode)
        //{
        //    if (dicAreaRegion.ContainsKey(areaCode))
        //    {
        //        return GetFullAddress(dicAreaRegion[areaCode].ParentCode) + " " + dicAreaRegion[areaCode].AreaName;
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        /// <summary>
        /// 根据areaCode获取全路径
        /// </summary>
        /// <param name="areaCode"></param>
        /// <returns></returns>
        //public string GetFullAddress(int proCode,int cityCode)
        //{
        //    if (dicAreaRegion.ContainsKey(proCode))
        //    {
        //        return dicAreaRegion[proCode].AreaName + " " + GetCityName(cityCode);
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        /// <summary>
        /// 获取城市名
        /// </summary>
        /// <returns></returns>
        //public string GetCityName(int areaCode)
        //{
        //    string _AreaName = string.Empty;
        //    if (dicAreaRegion.ContainsKey(areaCode))
        //        _AreaName = dicAreaRegion[areaCode].AreaName;
        //    return _AreaName;
        //}


        /// <summary>
        /// 根据areaCode获取全路径
        /// </summary>
        /// <param name="areaCode"></param>
        /// <returns></returns>
        //public string GetFullAddressd(int proCode, int cityCode)
        //{
        //    if (dicAreaRegion.ContainsKey(proCode))
        //    {
        //        return dicAreaRegion[proCode].AreaName + (string.IsNullOrEmpty(GetCityName(cityCode))?"":","+ GetCityName(cityCode));
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}


        /// <summary>
        /// 根据code获取组合地址
        /// </summary>
        /// <param name="pcode"></param>
        /// <param name="ccode"></param>
        /// <param name="acode"></param>
        /// <returns></returns>
        //public string GetAddress(int pcode,int ccode,int acode,string separator=".")
        //{
        //    string _AreaName = string.Empty;
        //    if (dicAreaRegion.ContainsKey(pcode))
        //    {
        //        _AreaName = dicAreaRegion[pcode].AreaName;
        //    }
        //    else
        //    {
        //        return _AreaName;
        //    }
        //    if (dicAreaRegion.ContainsKey(ccode))
        //    {
        //        _AreaName += separator + dicAreaRegion[ccode].AreaName;
        //    }
        //    else
        //    {
        //        return _AreaName;
        //    }
        //    if (dicAreaRegion.ContainsKey(acode))
        //    {
        //        _AreaName += separator + dicAreaRegion[acode].AreaName;
        //    }            
        //    return _AreaName;

        //}

    }
}