using DAL.Base;
using Entity.MiniApp.MiappTribune;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BLL.MiniApp.MiappTribune
{
    public class MiniappTribunePostTypeBLL : BaseMySql<MiniappTribunePostType>
    {
        /// <summary>
        /// 小程序论坛类别缓存key，父级rid.
        /// </summary>
        public const string cacheKey = "MiniappTribunePostType_{0}";
        
        public List<MiniappTribunePostType> GetListByRelationIdCache(int rid, bool fromCache = true)
        {
            string key = string.Format(cacheKey, rid);
            List<MiniappTribunePostType> tTypeList = RedisUtil.Get<List<MiniappTribunePostType>>(key);
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

        /// <summary>
        /// 返回下拉框数据,并默认全部显示(包括禁用列表的类型)
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="showHide"></param>
        /// <param name="currentId"></param>
        /// <returns></returns>
        public List<SelectListItem> GetPostTypeList(int rid,int showHide = 10, int currentId = 0)
        {
            var list = GetList( $" rid = {rid} {(showHide == 10 ? "" : "and isHide = {showHide}")} ");
            
            var returnList = list.Select(type => new SelectListItem
            {
                Text = type.Name,
                Value = type.Id.ToString(),
                Selected = type.Id == currentId
            }).ToList();

            //returnList.Insert(0, new SelectListItem
            //{
            //    Text = "未选择",
            //    Value = "0",
            //    Selected = true
            //});
            return returnList;
        }


        public void RemoveCache(int rid)
        {
            RedisUtil.Remove(string.Format(cacheKey, rid));
        }
        public override object Add(MiniappTribunePostType model)
        {
            object o = base.Add(model);
            RemoveCache(model.RId);
            return o;
        }
        public override bool Update(MiniappTribunePostType model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            RemoveCache(model.RId);
            return b;
        }
        public override bool Update(MiniappTribunePostType model)
        {
            bool b = base.Update(model);
            RemoveCache(model.RId);
            return b;
        }
    }
}
