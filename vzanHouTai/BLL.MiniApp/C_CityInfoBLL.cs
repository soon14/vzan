using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp
{
    public class C_CityInfoBLL : BaseMySql<C_CityInfo>
    {
        /// <summary>
        /// 同城系统缓存key， ｛区域ID｝
        /// </summary>
        private const string CityInfoCacheKey = "vzancityinfo_{0}";
        private const string F_CityInfoCacheKey = "f_vzancityinfo_{0}";
        /// <summary>
        /// serverid作为key
        /// </summary>
        private const string CityServerKey = "cityinfo_{0}";

        public C_CityInfo GetModelFromCache(int areaId, bool fromCache = true)
        {
            string key = string.Format(CityInfoCacheKey, areaId);
            C_CityInfo cityInfo;
            if (fromCache)
            {
                cityInfo = RedisUtil.Get<C_CityInfo>(key);
                if (cityInfo != null)
                {
                    if(cityInfo.PostCount==0 || cityInfo.StoreCount == 0)
                    {
                        
                    }
                    return cityInfo;
                }
            }
            cityInfo = GetModel("AreaCode=" + areaId) ?? GetModel("cityinfotag in (select cityinfotag from c_citysubarea where areacode = " + areaId + " and cityinfotag < 0)");
            if (cityInfo != null)
            {
                RedisUtil.Set(key, cityInfo, TimeSpan.FromDays(1));
            }
            //找不到，寻找父级
            //if (null == cityInfo)
            //{
            //    var bllArea = new C_AreaBLL();
            //    var currentArea = bllArea.GetModel(areaId);
            //    if (null != currentArea)
            //    {
            //        var parent = bllArea.GetModel(currentArea.ParentCode);
            //        cityInfo = GetModel("AreaCode=" + parent.Code);
            //        if (cityInfo != null)
            //        {
            //            RedisUtil.Set(key, cityInfo, TimeSpan.FromDays(1));
            //        }
            //    }
            //}
            return cityInfo;
        }
        public C_CityInfo GetModelByAreaCode(int areacode)
        {
            return GetModel("AreaCode=" + areacode) ?? GetModel("cityinfotag in (select cityinfotag from c_citysubarea where areacode = " + areacode + " and cityinfotag < 0)");
        }
        public C_CityInfo GetModelFromCacheByMinisnsId(int fid, bool fromCache = true)
        {
            string key = string.Format(F_CityInfoCacheKey, fid);
            C_CityInfo cityInfo;
            if (fromCache)
            {
                cityInfo = RedisUtil.Get<C_CityInfo>(key);
                if (cityInfo != null)
                {
                    if (cityInfo.PostCount == 0 || cityInfo.StoreCount == 0)
                    {
                        //RemoveCache(cityInfo);
                    }
                    return cityInfo;
                }
            }
            cityInfo =base.GetList("MiniSnsId=" + fid,1,1,"*","Id desc").Where(x=>x.AreaCode>0).FirstOrDefault() ;
            if (cityInfo != null)
            {
                RedisUtil.Set(key, cityInfo, TimeSpan.FromDays(1));
            }
            //找不到，寻找父级
            //if (null == cityInfo)
            //{
            //    var bllArea = new C_AreaBLL();
            //    var currentArea = bllArea.GetModel(areaId);
            //    if (null != currentArea)
            //    {
            //        var parent = bllArea.GetModel(currentArea.ParentCode);
            //        cityInfo = GetModel("AreaCode=" + parent.Code);
            //        if (cityInfo != null)
            //        {
            //            RedisUtil.Set(key, cityInfo, TimeSpan.FromDays(1));
            //        }
            //    }
            //}
            return cityInfo;
        }
        public C_CityInfo GetModelByDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return null;
            }
            List<MySqlParameter> listParam = new List<MySqlParameter>();
            listParam.Add(new MySqlParameter("@CusDomain", domain));
            return GetModel("CusDomain=@CusDomain", listParam.ToArray());
        }

        public override bool Update(C_CityInfo model)
        {
            bool b = base.Update(model);
            //RemoveCache(model);
            return b;
        }
        public override bool Update(C_CityInfo model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            //RemoveCache(model);
            return b;
        }
         
        //public bool DeletCityInfo(int cityinfoid, Guid accid, string nick)
        //{
        //    var city = GetModel(cityinfoid);
        //    if (null == city)
        //        return false;

        //    Delete(cityinfoid);
        //    RedisUtil.Remove(string.Format(CityInfoCacheKey, city.AreaCode));
        //    // 添加操作日志
        //    var log = new C_OperateLog
        //    {
        //        AccountId = accid,
        //        CreateDate = DateTime.Now,
        //        Remark = nick + "删除同城,Id=" + cityinfoid
        //    };
        //    //new C_OperateLogBLL().Add(log);
        //    //var bllSubCityInfo = new C_CitySubAreaBLL();
        //    //子区域
        //    //bllSubCityInfo.Delete($"CityInfoId={cityinfoid}");
        //    //角色
        //    //new C_UserRoleBLL().Delete($"CityInfoId={cityinfoid}");
        //    //论坛
        //    var bllforum = new MinisnsBll();
        //    var forum = bllforum.GetModel(city.MiniSnsId);
        //    forum.mprices = 0;
        //    bllforum.Update(forum, "mprices");
        //    return true;

        //}
         
        //public string AddICityInfo(C_UserInfo userInfo, DateTime endTime,int regionCode)
        //{
        //    try
        //    {
        //        var bllCityInfo = new C_CityInfoBLL();
        //        int areaCode = 100001001;
        //        var where = $"SELECT max(areacode) areacode from c_cityinfo";
        //        var newst = bllCityInfo.GetListBySql(where).FirstOrDefault();
        //        if (null != newst)
        //        {
        //            areaCode = newst.AreaCode + 1;
        //        }
        //        var cityforuminfo = new C_CityInfo
        //        {
        //            AreaCode = areaCode,
        //            IsThirdTools = 2,
        //            CName = userInfo.NickName + "-分类信息小程序",
        //            CreateDate = DateTime.Now,
        //            EndDate = endTime,
        //            LogoUrl = userInfo.HeadImgUrl,
        //            OpenId = userInfo.OpenId,
        //            UnionId = userInfo.UnionId,
        //            State = 0,
        //            RegionCode = regionCode
        //        };
        //        var cityinfoid = Convert.ToInt32(bllCityInfo.Add(cityforuminfo));

        //        //添加权限
        //        var role = new C_UserRole
        //        {
        //            CityInfoId = cityinfoid,
        //            AreaCode = cityforuminfo.AreaCode,
        //            OpenId = cityforuminfo.OpenId,
        //            UnionId = cityforuminfo.UnionId,
        //            UserState = 0,
        //            RoleId = 1
        //        };
        //        //new C_UserRoleBLL().Add(role);
        //    }
        //    catch (Exception ex)
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), ex);
        //        return "内部错误，请查看错误日志";
        //    }
        //    return string.Empty;
        //}
             

        #region 城市切换-蔡华兴
        /// <summary>
        /// 获取有同城信息城市数据
        /// </summary>
        /// <returns></returns>
        public List<C_CityInfo> GetCityInfo()
        {
            List<C_CityInfo> resultList = new List<C_CityInfo>();
            string strSql = @"select * from (
	select area.level,area.name as areaname
		,area.pingyin as py,area.code
,(select cityinfo.id from c_cityinfo cityinfo where (area.level=2 and left(cityinfo.AreaCode,4) = left(area.code,4)) or left(cityinfo.AreaCode,6)=left(area.code,6) LIMIT 0,1) as areaCount
	from c_area area where area.level BETWEEN 2 and 3
) result where result.areaCount>0 ORDER BY result.code;";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, strSql, null))
            {
                while (dr.Read())
                {
                    var model = new C_CityInfo { CName = dr["areaname"].ToString() };
                    if (dr["code"] != DBNull.Value)
                    {
                        model.AreaCode = int.Parse(dr["code"].ToString());
                    }

                    model.ConfigJson = dr["py"].ToString();
                    resultList.Add(model);
                }
            }
            return resultList;
        }
        /// <summary>
        /// 获取所有同城的区域信息,要排除行业版的
        /// </summary>
        /// <returns></returns>
        public List<C_Area> GetCityInfo3()
        {
            List<C_Area> resultList = new List<C_Area>();
            string strSql = @"SELECT area.`code`,area.`name`,area.`level`,area.pingyin 
from c_cityinfo cityinfo LEFT JOIN c_area area on  cityinfo.AreaCode = area.`code` 
where  area.`code`>0 and cityinfo.IsThirdTools=0 ";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, strSql, null))
            {
                while (dr.Read())
                {
                    var model = new C_Area();
                    if (dr["level"] != DBNull.Value)
                    {
                        model.Level = Convert.ToInt32(dr["level"]);
                    }
                    if (dr["name"] != DBNull.Value)
                    {
                        model.Name = dr["name"].ToString();
                    }
                    if (dr["code"] != DBNull.Value)
                    {
                        model.Code = Convert.ToInt32(dr["code"]);
                    }
                    if (dr["pingyin"] != DBNull.Value)
                    {
                        model.PingYin = dr["pingyin"].ToString();
                    }
                    resultList.Add(model);
                }
            }
            return resultList;
        }

        /// <summary>
        /// 根据市id获取市内所有同城信息
        /// </summary>
        /// <param name="areacode"></param>
        /// <returns></returns>
        public List<C_CityInfo> GetCityInfoList(int areacode)
        {
            List<C_CityInfo> resultList = new List<C_CityInfo>();
            List<C_Area> listArea = new C_AreaBLL().GetListBySql($"select code from c_area where parentCode = '{areacode}' or parentCode in (select code from c_area where parentCode='{areacode}')");
            string areaCodeJoins = listArea.Count > 0 ? string.Join(",", listArea.Select(x => x.Code)) : "";
            if (string.IsNullOrEmpty(areaCodeJoins))
                return new List<C_CityInfo>();
            string strSql = $"select cityinfo.AreaCode,cityinfo.CName,cityinfo.id from c_cityinfo cityinfo where cityinfo.AreaCode='{areacode}' or cityinfo.AreaCode in ({areaCodeJoins}) ORDER BY cityinfo.AreaCode";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, strSql, null))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    resultList.Add(model);
                }
            }
            return resultList;
        }
        #endregion

        #region 浏览量
        //public int ReadClickCountByRedis(long itemId, bool fromCache = true)
        //{
        //    string readKey = string.Format(MemCacheKey.CityClickCount_Key, itemId, "city");
        //    var clickcount = RedisUtil.Get<int>(readKey);

        //    CityCityClickSingle model;
        //    BaseMySql<CityCityClickSingle> dalClick = new BaseMySql<CityCityClickSingle> { TableName = "c_cityInfo" };
        //    if (clickcount <= 0)
        //    {
        //        model = dalClick.GetModel(itemId);
        //        if (model != null)
        //        {
        //            clickcount = model.ViewCount;
        //        }
        //        RedisUtil.Set(readKey, clickcount, TimeSpan.FromDays(3650));
        //    }
        //    else if (!fromCache)
        //    {
        //        model = dalClick.GetModel(itemId);
        //        if (model != null)
        //        {
        //            clickcount = model.ViewCount;
        //        }
        //        RedisUtil.Set(readKey, clickcount, TimeSpan.FromDays(3650));
        //    }
        //    return clickcount;
        //}
        //public int UpdateClickLogByRedis(int itemId)
        //{
        //    int vClickCount = ReadClickCountByRedis(itemId) + 1;
        //    RedisUtil.Set(string.Format(MemCacheKey.CityClickCount_Key, itemId, "city"), vClickCount, TimeSpan.FromDays(3650));
        //    return vClickCount;
        //}
        public void DeleteClickCountByRedis(long itemId)
        {
            string readKey = string.Format(MemCacheKey.CityClickCount_Key, itemId, "city");
            RedisUtil.Remove(readKey);
        } 
        #endregion
    }
}
