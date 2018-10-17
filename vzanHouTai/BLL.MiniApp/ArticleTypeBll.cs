using DAL.Base;
using Entity.MiniSNS;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Utility;
using System.Linq;
using System.Runtime.Serialization;

namespace BLL.MiniSNS
{
    public class ArticleTypeBll : BaseMySql<ArticleType>
    {
        /// <summary>
        /// 显示在顶部的版块的缓存key
        /// </summary>
        private const string articleTypeTopKey = "narticleTypeTopKey_{0}";

        /// <summary>
        /// 同城所有公共的版块   minisnsid 和 父级id
        /// </summary>
        private const string cityAllTypes = "city_types_{0}_{1}";

        /// <summary>
        /// 论坛的版块版本
        /// </summary>
        private const string minisnsTypeVer = "articletype_ver_{0}";


        public override object Add(ArticleType model)
        {
            object o = base.Add(model);
            RemoveArticleTypeList(model.MinisnsId, model.PId);
            return o;
        }
        public override bool Update(ArticleType model)
        {
            bool b = base.Update(model);
            RemoveArticleTypeList(model.MinisnsId, model.PId);
            return b;
        }
        public  bool RemoveFromTopNav(int typeid,int fid)
        {
            string sql = string.Format("UPDATE ArticleType set IsPushTop=0 where Id={0}", typeid);
            bool b = base.ExecuteTransaction(sql);
            RemoveArticleTypeList(fid);
            return b;
        }
        public bool AddTopNav(int typeid, int fid)
        {
            string sql = string.Format("UPDATE ArticleType set IsPushTop=1 where Id={0}", typeid);
            bool b = base.ExecuteTransaction(sql);
            RemoveArticleTypeList(fid);
            return b;
        }
        /// <summary>
        /// 获取第二级板块
        /// </summary>
        /// <returns></returns>
        public List<ArticleType> GetL2Types(int minisnsId)
        {
            string ids = string.Empty;
            List<ArticleType> parentlist = GetList("pid=0 and minisnsId=" + minisnsId);
            foreach (var item in parentlist)
            {
                ids += item.Id + ",";
            }
            var strWhere = string.Format("pid in ({0})", ids.Substring(0, ids.Length - 1));
            return GetList(strWhere);
        }

        ///// <summary>
        ///// 是否锁定版块
        ///// </summary>
        ///// <param name="arttype"></param>
        ///// <param name="lockstate"></param>
        ///// <returns></returns>
        //public bool updateLockState(ArticleType arttype, int lockstate)
        //{
        //    string ids = GetSubTypeIds(arttype.Id, arttype.MinisnsId);
        //    string sql = string.Format("UPDATE ArticleType set LockStatus={0} where Id in ({1})", lockstate, ids);
        //    bool b =  base.ExecuteTransaction(sql);
        //    RemoveArticleTypeList(arttype.MinisnsId, arttype.PId);
        //    return b;
        //}

        /// <summary>
        /// 处理板块变更的缓存
        /// </summary>
        /// <param name="minisnsId"></param>
        public void RemoveArticleTypeList(int minisnsId,int pid=0)
        {
            RedisUtil.Remove(string.Format(MemCacheKey.ArtTypeToatalCount));
            RedisUtil.Remove(string.Format(MemCacheKey.Model_ArtTypeList_Key, minisnsId));
            RedisUtil.Remove(string.Format(MemCacheKey.Small_ArtTypeList_Key, minisnsId));
            RedisUtil.SetVersion(string.Format(minisnsTypeVer, minisnsId));
            RedisUtil.Remove(string.Format(articleTypeTopKey, minisnsId));
        }
        /// <summary>
        /// 重置默认版块
        /// </summary>
        /// <param name="fId"></param>
        /// <returns></returns>
        public bool ReSetDefaultType(int fId)
        {
            string sql = string.Format("UPDATE ArticleType set IsDefault=0 where MinisnsId={0}", fId);
            bool b = base.ExecuteTransaction(sql);
            RemoveArticleTypeList(fId);
            return b;
        }
        /// <summary>
        /// 板块排序批量修改
        /// </summary>
        /// <param name="fId"></param>
        /// <returns></returns>
        public bool ChangeSort(string [] ids,IOrderedEnumerable<string> sorts,int fId)
        {
            StringBuilder sqls = new StringBuilder();
            for (int i = 0; i < ids.Length; i++)
            {
                sqls.Append($"UPDATE ArticleType set Sort={sorts.ElementAt(i)} where Id={ids[i]};");
            }
            RemoveArticleTypeList(fId);
            return  base.ExecuteTransaction(sqls.ToString());
        }
        /// <summary>
        /// 该状态删除版块，连同子版块一起删除
        /// </summary>
        /// <param name="state"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckOut(int state, int id)
        {
            StringBuilder strSql = new StringBuilder();
            string ids = id.ToString();
            ArticleType _Type = base.GetModel(id);
            if (state == 0)
                ids = GetSubTypeIdsNoState(id, _Type.MinisnsId);  
            strSql.AppendFormat("UPDATE ArticleType set state={0} where Id in ({1})", state, ids);
            new ArticleBll().ClearArticleType(_Type.MinisnsId, ids);
           bool b= base.ExecuteTransaction(strSql.ToString());
            RemoveArticleTypeList(_Type.MinisnsId, _Type.PId);
            return b;
        }

        /// <summary>
        /// 物理删除版块，将下面的子版块也删除
        /// </summary>
        /// <param name="art"></param>
        /// <returns></returns>
        public bool delete(ArticleType art)
        {
            try
            {
                if (art == null)
                    return false;
                StringBuilder strSql = new StringBuilder();
                string ids = GetSubTypeIdsNoState(art.Id,art.MinisnsId);
                strSql.AppendFormat("delete from ArticleType  where Id in ({0})", ids);
                new ArticleBll().ClearArticleType(art.MinisnsId, ids);
                //return base.ExecuteTransaction(strSql.ToString());
                bool b = base.ExecuteTransaction(strSql.ToString());
                RemoveArticleTypeList(art.MinisnsId,art.PId);
                return b;
            }
            catch { return false; }            
        }
        
        /// <summary>
        /// 获取版块的名字
        /// </summary>
        /// <param name="articleTypeId"></param>
        /// <param name="minisnsId"></param>
        /// <returns></returns>
        public string GetArticleTypeName(int articleTypeId, int minisnsId)
        {
            string typeName = string.Empty;
            ArticleType atmodel = GetModel(articleTypeId);//GetArticleTypeList(minisnsId).Find(p => p.Id == articleTypeId);//articleTypebll.GetModel(Convert.ToInt32(typeId));
            if (atmodel != null)
            {
                typeName = atmodel.Title;
            }
            return typeName;
        }
        public int GetTplCount(int fid)
        {
           var key=string.Format(MemCacheKey.TplCountKey, fid);
           int result = RedisUtil.Get<int>(key);
            if (0 == result)
            {
                var where= string.Format("MinisnsId={0} and ArtTempletId>=0", fid);
                var count = GetCount(where);
                RedisUtil.Set<int>(key,count,TimeSpan.FromDays(30));
                result = count;
            }
            return result;
        }
        /// <summary>
        /// 论坛下的所有版块
        /// </summary>
        /// <param name="minisnsId"></param>
        /// <returns></returns>
        public List<ArticleType> GetMinisnsType(int minisnsId)
        {
            return GetList(string.Format(" minisnsId={0} and AppType=0", minisnsId));
        }

        public string GetSubTypeIds(int artTypeId, int siteId,bool fromCache=true)
        {
            try
            {
                string typeIds = string.Empty;
                if (artTypeId > 0)
                {
                    typeIds = artTypeId.ToString();
                    List<ArticleType> vlist = GetArticleTypeList(siteId, fromCache);
                    List<ArticleType> lv1 = vlist.Where(a => a.PId == artTypeId).ToList();
                    foreach (ArticleType item in lv1)
                    {
                        typeIds += "," + item.Id;
                        List<ArticleType> lv2 = vlist.Where(a => a.PId == item.Id).ToList();
                        if (lv2 != null && lv2.Count > 0)
                        {
                            foreach (ArticleType items in lv2)
                            {
                                typeIds += "," + items.Id;
                            }
                        }
                    }
                    return typeIds;
                }
                return artTypeId.ToString();
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), new Exception(ex.Message + "找子版块出错了：artTypeId=" + artTypeId));
                return artTypeId.ToString();
            }
        }
        /// <summary>
        /// 获取版块下的所有子版块
        /// </summary>
        /// <param name="artTypeId"></param>
        /// <param name="siteId"></param>
        /// <returns>子版块的id连接</returns>
        public string GetSubTypeIdsNoState(int artTypeId, int siteId)
        {
            try
            {
                string typeIds = string.Empty;
                if (artTypeId > 0)
                {
                    typeIds = artTypeId.ToString();
                    List<ArticleType> vlist = GetMinisnsType(siteId);
                    List<ArticleType> lv1 = vlist.Where(a => a.PId == artTypeId).ToList();
                    foreach (ArticleType item in lv1)
                    {
                        typeIds += "," + item.Id;
                        List<ArticleType> lv2 = vlist.Where(a => a.PId == item.Id).ToList();
                        if (lv2 != null && lv2.Count > 0)
                        {
                            foreach (ArticleType items in lv2)
                            {
                                typeIds += "," + items.Id;
                            }
                        }
                    }
                    return typeIds;
                }
                return artTypeId.ToString();
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), new Exception(ex.Message + "找子版块出错了：artTypeId=" + artTypeId));
                return artTypeId.ToString();
            }
        }

        public List<ArticleType> GetSubTypeList(int artTypeId,int minisnsId)
        {
            if (artTypeId > 0)
            {
                try
                {
                    List<ArticleType> allSubTypelist = new List<ArticleType>();
                    string ids = GetSubTypeIds(artTypeId, minisnsId);
                    string sql = string.Format("select * from articletype where id in ({0});", ids);
                    allSubTypelist = base.GetListBySql(sql);
                    return allSubTypelist;
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteError(this.GetType(), ex);
                    return GetList("id=" + artTypeId);
                }
            }
            return null;
        }       
        public string GetParentIds(int artTypeId)
        {
            string typeIds = artTypeId.ToString();
            if (artTypeId > 0)
            {
                ArticleType model3 = GetModel(artTypeId);
                if (model3 == null)
                    return typeIds;
                if (model3.PId > 0)
                {
                    typeIds += "," + model3.PId;
                    ArticleType model2 = GetModel(model3.PId);
                    if (model2 == null)
                        return typeIds;
                    if (model2.PId > 0)
                        typeIds += "," + model2.PId;
                }
                typeIds += ",0";
            }
            return typeIds;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="minisnsId"></param>
        /// <param name="ShowType">1,设置了在【全部】显示，0，隐藏</param>
        /// <returns></returns>
        public string GetTopShowTypeIds(int minisnsId,int ShowType)
        {
            List<ArticleType> typeList = GetArticleTypeList(minisnsId);
            if (typeList != null && typeList.Count > 0)
            {
                List<ArticleType> ShowTypeList = typeList.FindAll(p => p.IsAllShow == ShowType);//--设置了显示的版块
                if (ShowTypeList!=null && ShowTypeList.Count>0)
                {
                    string ids = string.Empty;
                    foreach (ArticleType type in ShowTypeList)
                    {
                        ids += (type.Id.ToString() + ",");
                    }
                    return ids.Trim(',');
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取论坛下的所有版块
        /// </summary>
        /// <param name="minisnsId"></param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        public List<ArticleType> GetArticleTypeList(int minisnsId, bool fromCache = true)
        {
            string key = string.Format(MemCacheKey.Model_ArtTypeList_Key, minisnsId);
            List<ArticleType> _list = RedisUtil.Get<List<ArticleType>>(key);
            if (null == _list || !fromCache)
            {
                _list = new ArticleTypeBll().GetList(string.Format("minisnsId={0} and state=1 and AppType=0", minisnsId), 0, 0, "", " Sort DESC ");
                if(_list!=null && _list.Count > 0)
                {
                    RedisUtil.Set<List<ArticleType>>(key, _list, TimeSpan.FromHours(24));
                }
            }
            return _list;
        }
        public List<ArticleType> SmallGetArticleTypeList(int minisnsId)
        {
            string key = string.Format(MemCacheKey.Small_ArtTypeList_Key, minisnsId);
            List<ArticleType> _list = RedisUtil.Get<List<ArticleType>>(key);
            if (null == _list )
            {
                _list = new ArticleTypeBll().GetList(string.Format(" minisnsId={0} and state=1 and AppType=0 and PId=0", minisnsId), 0, 0, "", " Sort DESC ");
                if (_list != null && _list.Count > 0)
                {
                    RedisUtil.Set<List<ArticleType>>(key, _list, TimeSpan.FromHours(24));
                }
            }
            return _list;
        }
        /// <summary>
        /// 获取论坛下的版块
        /// </summary>
        /// <param name="minisnsId"></param>
        /// <param name="pId">父级ID，0是一级版块</param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        public List<ArticleType> GetArticleTypeList(int minisnsId, int pId, bool fromCache = true)
        {
            string key = string.Format(MemCacheKey.Model_ArtTypeListLevel_Key, minisnsId, pId);
            int ver = RedisUtil.GetVersion(string.Format(minisnsTypeVer, minisnsId));
            ArticleTypeCache _list = null;
            try
            {
                _list = RedisUtil.Get<ArticleTypeCache>(key);
            }
            catch(SerializationException)
            {
                _list = null;
            }
            if (_list == null || _list.Version != ver || null == _list._ArticleTypeList || _list._ArticleTypeList.Count == 0 || !fromCache)
            {
                _list = new ArticleTypeCache();
                _list._ArticleTypeList = new ArticleTypeBll().GetList(string.Format("minisnsId={0} and pid={1} and state=1 and AppType=0", minisnsId, pId), 0, 0, "", " Sort DESC ");
                _list.Version = ver;
                if (_list._ArticleTypeList != null && _list._ArticleTypeList.Count > 0)
                    RedisUtil.Set(key, _list, TimeSpan.FromHours(24));
            }
            return _list._ArticleTypeList;
        }

        /// <summary>
        /// 获取设置了在顶部的版块
        /// </summary>
        /// <param name="minisnsId"></param>
        /// <returns></returns>
        public List<ArticleType> GetArticleTypeTop(int minisnsId)
        {
            string key = string.Format(articleTypeTopKey,minisnsId);
            List<ArticleType> _typeList = RedisUtil.Get<List<ArticleType>>(key);
            if (_typeList == null || _typeList.Count == 0)
            {
                _typeList = GetList($"minisnsId={minisnsId} and state=1 and IsPushTop=1", 10, 1,"*", " Sort DESC ");
                RedisUtil.Set(key, _typeList);
            }
            return _typeList;
        }


        /// <summary>
        /// 更新论坛的配置，是否将版块下的帖子显示在全部
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="isAllShow"></param>
        /// <param name="minisnsId"></param>
        public void SetShowAll(int typeId,int isAllShow,int minisnsId)
        {
            string subIds = GetSubTypeIdsNoState(typeId, minisnsId);
            if (!string.IsNullOrEmpty(subIds))
            {
                string sql = string.Format(" UPDATE ArticleType set IsAllShow={0} where Id in ({1})", isAllShow, subIds);
                base.ExecuteTransaction(sql);
                RemoveArticleTypeList(minisnsId);
            }
        }

        /// <summary>
        /// 获取发帖默认选种版块的名称
        /// 一级菜单,二级菜单,三级菜单
        /// </summary>
        /// <returns></returns>
        public string GetDefaultCategoryName(int minisnsId)
        {
            //List<ArticleType> list = GetArticleTypeList(minisnsId);
            StringBuilder sb = new StringBuilder();
            ArticleType defaultType = GetModel($"MinisnsId={minisnsId} and IsDefault=1 and state>0");//list.SingleOrDefault(x => x.IsDefault == 1);
            if (defaultType == null)
            {
                return string.Empty;
            }
            else
            {
                ArticleType sType = GetModel(defaultType.PId);//list.SingleOrDefault(x => x.Id == defaultType.PId);
                string secontNmae = string.Empty;
                string firstName = string.Empty;
                if (sType != null)
                {
                    secontNmae = sType.Title;
                    ArticleType fType = GetModel(sType.PId); //list.SingleOrDefault(x => x.Id == sType.PId);
                    firstName = fType == null ? string.Empty : fType.Title;
                }
                string strName = string.Format("{0},{1},{2}", firstName, secontNmae, defaultType.Title);
                return strName.TrimStart(',');
            }            
        }



        /// <summary>
        /// 查找同城下所有官方版块
        /// </summary>
        /// <param name="minisnsId">0,代表所有同城一样的版块</param>
        /// <param name="pId"></param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        public List<ArticleType> GetCityAllTypes(int minisnsId, int pId, bool fromCache = true)
        {
            string key = string.Format(MemCacheKey.Model_ArtTypeListLevel_Key, minisnsId, pId);
            int ver = RedisUtil.GetVersion(string.Format(minisnsTypeVer, minisnsId));
            ArticleTypeCache _list = RedisUtil.Get<ArticleTypeCache>(key);
            if (_list == null || _list.Version != ver || null == _list._ArticleTypeList || _list._ArticleTypeList.Count == 0 || !fromCache)
            {
                _list = new ArticleTypeCache();
                _list._ArticleTypeList = GetList(string.Format("minisnsId={0} and pid={1} and state=1 and AppType=1", minisnsId, pId), 0, 0, "*", " Sort DESC ");
                _list.Version = ver;
                RedisUtil.Set(key, _list, TimeSpan.FromHours(24));
            }
            return _list._ArticleTypeList;
        }
        /// <summary>
        /// 获取导航版块链
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="typeid"></param>
        /// <returns></returns>
        public List<ArticleType> GetTypeLinkCache(int fid, int typeid)
        {
            List<ArticleType> list = new List<ArticleType>();
            var artTypeList = GetArticleTypeList(fid);
            var model = artTypeList.FirstOrDefault(x => x.Id == typeid);
            for (int i = 0; i < 2; i++)
            {
                if (null != model)
                {
                    list.Add(model);
                    var parent = artTypeList.Find(x => x.Id == model.PId);
                    model = parent;
                }
                continue;
            }
            list.Reverse();
            return list;
        }
        public int GetLevel(int typeid)
        {
            var model = GetModel(typeid);
            if (model.PId == 0)
                return 0;
            else {
                var pmodel = GetModel(model.PId);
                if (pmodel.PId == 0)
                    return 1;
                else
                    return 2;
            }
        }
    }
}
