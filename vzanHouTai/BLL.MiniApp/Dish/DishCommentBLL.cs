using DAL.Base;
using System.Data;
using Entity.MiniApp.Dish;
using System.Collections.Generic;
using Utility;
using Core.MiniApp;
using System;

namespace BLL.MiniApp.Dish
{
    public class DishCommentBLL : BaseMySql<DishComment>
    {
        #region 单例模式
        private static DishCommentBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishCommentBLL()
        {

        }

        public static DishCommentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishCommentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public float GetStars(int id)
        {
            float stars = 0;
            if (id <= 0)
            {
                return stars;
            }
            string sql = $"select sum(star) as score,count(1) as count from dishcomment where storeid={id}";
            using (var dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    float score = dr["score"] == DBNull.Value ? 0 : float.Parse(dr["score"].ToString());
                    int count = Convert.ToInt32(dr["count"]);
                    stars = count <= 0 ? 0 : float.Parse(Math.Round(score / count, 1).ToString());
                }
                // return list;
                return stars;
            }
        }

        public bool DeleteComment(DishComment comment)
        {
            comment.state = -1;
            return base.Update(comment, "state");
        }

        public List<DishComment> GetListByStore(int storeId,int pageIndex = 1, int pageSize = 10)
        {
            string whereSql = BuildWhereSql(storeId: storeId);
            return GetList(whereSql,pageSize,pageIndex);
        }

        public int GetCountByStore(int storeId)
        {
            string whereSql = BuildWhereSql(storeId: storeId);
            return GetCount(whereSql);
        }

        public string BuildWhereSql(int? storeId, bool isGetDel = false)
        {
            List<string> whereSql = new List<string>();
            if(storeId.HasValue)
            {
                whereSql.Add($"StoreId = {storeId}");
            }
            if(!isGetDel)
            {
                whereSql.Add("State = 1");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
