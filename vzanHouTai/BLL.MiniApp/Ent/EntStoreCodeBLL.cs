using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Ent
{
    public class EntStoreCodeBLL : BaseMySql<EntStoreCode>
    {
        #region 单例模式
        private static EntStoreCodeBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntStoreCodeBLL()
        {

        }

        public static EntStoreCodeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntStoreCodeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string _redis_EntStoreCodeList = "EntStoreCodeList_{0}_{1}_{2}";
        private readonly string _redis_EntStoreCodeList_version = "EntStoreCodeList_version_{0}";

        public List<EntStoreCode> GetListByIds(string ids)
        {
            return base.GetList($"id in ({ids})");
        }

        public RedisModel<EntStoreCode> GetListByAid(int aid,int state,int pageSize,int pageIndex)
        {
            string key = string.Format(_redis_EntStoreCodeList, aid, pageSize, pageIndex);
            string version_key = string.Format(_redis_EntStoreCodeList_version, aid);
            int version = RedisUtil.GetVersion(version_key);
            RedisModel<EntStoreCode> list = RedisUtil.Get<RedisModel<EntStoreCode>>(key);
            if (list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || version != list.DataVersion)
            {
                list = new RedisModel<EntStoreCode>();
                string sqlwhere = $"aid = {aid} and state>{state}";

                #region 缓存
                list.DataList = base.GetList(sqlwhere, pageSize, pageIndex); ;
                list.Count = base.GetCount(sqlwhere); ;
                list.DataVersion = version;
                #endregion
            }
            return list;
        }

        /// <summary>
        /// 清除列表缓存
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveEntStoreCodeListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(_redis_EntStoreCodeList_version, appid));
            }
        }

        public bool ExistData(int aid ,string name)
        {
            MySqlParameter[] param = new []{ new MySqlParameter("@Scene",name) };
            return base.Exists($" aid = {aid} and Scene = @Scene and State > 0 ",param);
        }

        public EntStoreCode GetByName(string name,int aid)
        {
            MySqlParameter[] param = new[] { new MySqlParameter("@Scene", name) };
            return GetModel("Scene = @Scene AND State > 0", param);
        }

        /// <summary>
        /// 获取二维码名称
        /// </summary>
        /// <param name="list"></param>
        public void GetStoreCodeName<T>(ref List<T> list)
        {
            if(list==null || list.Count<=0)
                return;

            List<string> scids = new List<string>();
            foreach (T item in list)
            {
                int StoreCodeId = Convert.ToInt32(item.GetType().GetProperty("StoreCodeId").GetValue(item));
                if (StoreCodeId >0)
                {
                    scids.Add(StoreCodeId.ToString());
                }
            }

            string storecodeids = string.Join(",", scids.Distinct());
            if(storecodeids==null || storecodeids.Length==0)
                return;
            
            List<EntStoreCode> codelist = GetListByIds(storecodeids);
            if (codelist == null || codelist.Count <= 0)
                return;
            
            foreach (T item in list)
            {
                int storeCodeId = Convert.ToInt32(item.GetType().GetProperty("StoreCodeId").GetValue(item));
                if (storeCodeId <= 0)
                    continue;
                EntStoreCode model = codelist.Where(w => w.Id == storeCodeId).FirstOrDefault();
                if (model == null)
                    continue;

                item.GetType().GetProperty("StoreCodeName").SetValue(item, model.Scene);
            }
        }
    }
}
