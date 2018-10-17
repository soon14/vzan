using DAL.Base;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Conf
{
    public class PickPlaceBLL : BaseMySql<PickPlace>
    {
        #region 单例模式
        private static PickPlaceBLL _singleModel;
        private static readonly object SynObject = new object();

        private PickPlaceBLL()
        {

        }

        public static PickPlaceBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PickPlaceBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 根据aid获取有效的数据
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public List<PickPlace> GetListByAid(int aid)
        {
            List<PickPlace> list = new List<PickPlace>();
            if (aid <= 0)
            {
                return list;
            }
            string sqlwhere = $" aid={aid} and state>=0";
            list = GetList(sqlwhere);
            return list;
        }

        
        /// <summary>
        /// 通过经纬度获取到店自取的附近门店,按距离排序最近的优先然后再按时间
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="ws_lat"></param>
        /// <param name="ws_lng"></param>
        /// <returns></returns>
        public List<PickPlace> GetListNearStoreByLocation(int aid, out int totalCount, int pageSize = 10, int pageIndex = 1, double ws_lat = 0, double ws_lng = 0)
        {
            List<PickPlace> list = new List<PickPlace>();
            totalCount = 0;
            
            string strWhere = $"  aid={aid} and state>=0 ";
            string strWhereCount = $" aid={aid} and state>=0 ";
            string sql = $"select *,ROUND(6378.138*2*ASIN(SQRT(POW(SIN(({ws_lat}*PI()/180-lat*PI()/180)/2),2)+COS({ws_lat}*PI()/180)*COS(lat*PI()/180)*POW(SIN(({ws_lng}*PI()/180-lng*PI()/180)/2),2)))*1000) AS distance from PickPlace where { strWhere}  ORDER BY distance asc, addtime desc limit {(pageIndex - 1) * pageSize},{pageSize}";
            using (var dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql,null))
            {
                while (dr.Read())
                {
                    PickPlace pickPlace = base.GetModel(dr);
                    pickPlace.DistanceStr = (Convert.ToInt32(dr["distance"]) * 0.001) < 1 ? $"{dr["distance"].ToString()}m" : $"{Convert.ToInt32(dr["distance"]) * 0.001}km";
                    list.Add(pickPlace);
                }
                if (list.Count > 0)
                {
                    totalCount = base.GetCount(strWhereCount);
                }

                return list;
            }
            

           
        }




        public PickPlace GetModelByAid_Name(int aid, string storeName)
        {
            string whereSql = $"aid = {aid} and name ='{storeName}' and state >= 0";
            return GetModel(whereSql);
        }

        public PickPlace GetModelByAid_Id(int aid, int id)
        {
            PickPlace place = null;
            if (aid <= 0 || id <= 0)
            {
                return place;
            }
            string sqlwhere = $"id={id} and aid={aid} and state>=0";
            place = GetModel(sqlwhere);
            return place;
        }

        public PickPlace GetModelByAid_StoreId_Id(int aid, int storeId, int id)
        {
            PickPlace place = null;
            if (aid <= 0 || storeId <= 0 || id <= 0)
            {
                return place;
            }
            string sqlwhere = $"aid={aid} and storeId={storeId} and id={id} and state>=0";
            place = GetModel(sqlwhere);
            return place;
        }

        public List<PickPlace> GetListByAid_StoreId(int aid, int storeId)
        {
            List<PickPlace> list = new List<PickPlace>();
            if (aid <= 0 || storeId <= 0)
            {
                return list;
            }
            string sqlwhere = $" aid={aid} and storeId={storeId} and state>=0";
            list = GetList(sqlwhere);
            return list;
        }
    }
}