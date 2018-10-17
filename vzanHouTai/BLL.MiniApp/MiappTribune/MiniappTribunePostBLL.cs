using DAL.Base;
using Entity.MiniApp.MiappTribune;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entity.MiniApp.C_Enums;

namespace BLL.MiniApp.MiappTribune
{
    public class MiniappTribunePostBLL : BaseMySql<MiniappTribunePost>
    {
        /// <summary>
        /// 小程序论坛帖子缓存key，父级rid.
        /// </summary>
        public const string cacheKey = "MiniappTribunePost_{0}";


        public List<MiniappTribunePost> GetListByRidPostId(int rid,  int postType = -999, int pageIndex = 1, int pageSize = 10, string OrderByStr = "Id desc", int State = 10, string Title = "", int TopState = -1)
        {
            string sqlWhere = "RId=@RId";
            List<MySqlParameter> param = new List<MySqlParameter>();
            param.Add(new MySqlParameter("@RId", rid));
            if (postType != -1)
            {
                sqlWhere += " and  postType=@postType ";
                param.Add(new MySqlParameter("@postType", postType));
            }
            if (State != 10)
            {
                sqlWhere += " and  State=@State ";
                param.Add(new MySqlParameter("@State", State));
            }
            else
            {
                sqlWhere += " and (state>=0 or state=" + (int)PostState.待支付 + ") ";//默认不查询已经删除的数据
            }
            if (!string.IsNullOrWhiteSpace(Title))
            {
                sqlWhere += " and Title = @Title";//默认不查询已经删除的数据
                param.Add(new MySqlParameter("@Title", Title));
            }
            if (TopState != -1)
            {
                sqlWhere += " and TopState = @TopState";
                param.Add(new MySqlParameter("@TopState", TopState));
            }
            var postList = GetListByParam(sqlWhere, param.ToArray(), pageSize, pageIndex, "*", OrderByStr);
            if (postList == null)
            {
                postList = new List<MiniappTribunePost>();
            }
            return postList;
        }

        public int GetListByRidPostIdCount(int rid, int postType = -1, int pageIndex = 1, int pageSize = 10, string OrderByStr = "Id desc", int State = 10, string Title = "", int TopState = -1)
        {
            string sqlWhere = "RId=@RId";
            List<MySqlParameter> param = new List<MySqlParameter>();
            param.Add(new MySqlParameter("@RId", rid));
            if (postType != -1)
            {
                sqlWhere += " and  postType=@postType ";
                param.Add(new MySqlParameter("@postType", postType));
            }
            if (State != 10)
            {
                sqlWhere += " and  State=@State ";
                param.Add(new MySqlParameter("@State", State));
            }
            else
            {
                sqlWhere += " and (state>=0 or state=" + (int)PostState.待支付 + ") ";//默认不查询已经删除的数据
            }
            if (!string.IsNullOrWhiteSpace(Title))
            {
                sqlWhere += " and Title = @Title";//默认不查询已经删除的数据
                param.Add(new MySqlParameter("@Title", Title));
            }
            if (TopState != -1)
            {
                sqlWhere += " and TopState = @TopState";
                param.Add(new MySqlParameter("@TopState", TopState));
            }
            var count = GetCount(sqlWhere, param.ToArray());
            return count;
        }


        public List<MiniappTribunePost> GetListByRelationIdCache(int rid, bool fromCache = true)
        {
            string key = string.Format(cacheKey, rid);
            List<MiniappTribunePost> tTypeList = RedisUtil.Get<List<MiniappTribunePost>>(key);
            if (tTypeList == null || tTypeList.Count == 0 || !fromCache)
            {
                string sqlWhere = "RId=@RId";

                List<MySqlParameter> param = new List<MySqlParameter>();
                param.Add(new MySqlParameter("@RId", rid));
                tTypeList = GetListByParam(sqlWhere, param.ToArray(), 100, 1, "*", "ShowSort desc,Id desc");
                RedisUtil.Set(key, tTypeList);
            }
            return tTypeList;
        }
        public void RemoveCache(int rid)
        {
            RedisUtil.Remove(string.Format(cacheKey, rid));
        }
        public override object Add(MiniappTribunePost model)
        {
            object o = base.Add(model);
            RemoveCache(model.Rid);
            return o;
        }
        public override bool Update(MiniappTribunePost model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            RemoveCache(model.Rid);
            return b;
        }
        public override bool Update(MiniappTribunePost model)
        {
            bool b = base.Update(model);
            RemoveCache(model.Rid);
            return b;
        }
    }
}
