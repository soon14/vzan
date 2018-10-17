using DAL.Base;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Pin
{
    public class PinCategoryBLL : BaseMySql<PinCategory>
    {
        #region 单例模式
        private static PinCategoryBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinCategoryBLL()
        {

        }

        public static PinCategoryBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinCategoryBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public static string listKey = "PinCategoryListKey_{0}";//aid
        public static string modelKey = "PinCategory_{0}";//id
        public bool UpdateSortBatch(string sortData,int aId)
        {
            if (string.IsNullOrEmpty(sortData))
                return false;

            string[] sortDataArray = sortData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (sortDataArray.Length <= 0)
                return false;
            List<string> sql = new List<string>();
            List<MySqlParameter[]> sqlParameters = new List<MySqlParameter[]>();
            for (int i = 0; i < sortDataArray.Length; i++)
            {
                string[] idSortArray = sortDataArray[i].Split('_');
                sql.Add($"update {TableName} set sort=@sort where id=@id");
                sqlParameters.Add(new MySqlParameter[] {
                    new MySqlParameter("@sort",idSortArray[1]),
                    new MySqlParameter("@id",idSortArray[0])
                });
            }
            return ExecuteTransaction(sql.ToArray(), sqlParameters.ToArray());
        }


        public List<PinCategory> GetAllCategory(int aid)
        {
            //string key = string.Format(listKey, aid);
            //List<PinCategory> list = RedisUtil.Get<List<PinCategory>>(key);
            //if (list == null)
            //    list = GetList($"aid={aid} and storeid=0 and state=1",1000,1,"*","sort desc");

            //if (list != null)
            //    RedisUtil.Set(key, list, TimeSpan.FromHours(4));
            //return list;
            return GetList($"aid={aid} and storeid=0 and state=1", 1000, 1, "*", "sort desc"); 
        }
        public List<PinCategory> GetStoreCategory(int aid, int storeId)
        {
            return GetListBySql($@" SELECT * from pincategory where id in (
                                SELECT DISTINCT p.cateId as id from pingoods p  where p.storeId = {storeId} and p.aId = {aid}
                                union
                                SELECT DISTINCT p.cateIdOne as id from pingoods p  where p.storeId = {storeId} and p.aId = {aid}
                            ) order by sort desc");
        }


        public override PinCategory GetModel(int Id)
        {
            //string key = string.Format(modelKey, Id);
            //PinCategory model = RedisUtil.Get<PinCategory>(key);

            //if (model == null)
            //    model = base.GetModel(Id);

            //if (model != null)
            //    RedisUtil.Set(key, model, TimeSpan.FromHours(4));
            //return model;
            return base.GetModel(Id);
        }
        public override object Add(PinCategory model)
        {
            //RemoveCache(model);
            return base.Add(model);
        }

        public override bool Update(PinCategory model)
        {
            //RemoveCache(model);
            return base.Update(model);
        }

        public override bool Update(PinCategory model, string columnFields)
        {
            //RemoveCache(model);
            return base.Update(model, columnFields);
        }

        public void RemoveCache(PinCategory model)
        {
            if (model != null)
            {
                RedisUtil.Remove(string.Format(modelKey, model.id));
                RedisUtil.Remove(string.Format(listKey, model.aId));
            }
        }
    }
}
