using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntGoodTypeBLL : BaseMySql<EntGoodType>
    {
        #region 单例模式
        private static EntGoodTypeBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntGoodTypeBLL()
        {

        }

        public static EntGoodTypeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntGoodTypeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string Redis_EntGoodTypeList = "EntGoodType_{0}_{1}_{2}_{3}";
        private readonly string Redis_EntGoodTypeList_version = "EntGoodType_version_{0}_{1}";

        /// <summary>
        /// 根据类别名称获取类别Id
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public string GetEntGoodTypeId(int aid,string typeName)
        {
            string sql = $"SELECT id from entgoodtype where aid={aid} and name=@typeName and state=1";
          object id=  SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                CommandType.Text, sql,
                new MySqlParameter[] { new MySqlParameter("@typeName", typeName) });
            if (id != DBNull.Value)
            {
                return Convert.ToString(id);
            }
            return string.Empty;
        }
        
        /// <summary>
        /// 获取一级分类以及其下的二级分类
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public List<GoodTypeRelation> GetGoodTypeList(int appId,string ids)
        {
           // int firtCount = 0;
            string sql = $"aid={appId} and state=1 and Id in ({ids})";
            List<EntGoodType> listFirstType =base.GetList(sql,200,1,"*"," sort desc,id desc");// GetListByCach(appId, 100, 1,ref firtCount, 0);
            if (listFirstType != null && listFirstType.Count>0)
            {
        
                List<GoodTypeRelation> listGoodTypeRelation = new List<GoodTypeRelation>();
                foreach(EntGoodType item in listFirstType)
                {
                    listGoodTypeRelation.Add(new GoodTypeRelation()
                    {
                        FirstGoodType = item,
                        SecondGoodTypes = base.GetList($"aid={appId} and state=1 and parentId={item.id}", 500, 1, "*", " sort desc,id desc")

                    });
                }
                return listGoodTypeRelation;
            }
            return null;
        }

        /// <summary>
        /// 根据一级分类ID  parentId获取下面的二级分类
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public List<EntGoodType> GetSecondGoodTypeList(int appId,int parentId)
        {
      
            List<EntGoodType> list = base.GetList($"aid={appId} and state=1 and parentId={parentId}");
            return list;
        }
        
        public string GetEntGoodTypeName(string ptypes)
        {
            string sql = $"SELECT GROUP_CONCAT(`name`) from entgoodtype where FIND_IN_SET(id,@ptypes)";
            return DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                CommandType.Text, sql,
                new MySqlParameter[] { new MySqlParameter("@ptypes", ptypes) }).ToString();
        }

        public int isInitiType(int appId,int parentId)
        {
            string sql = $"update entgoodtype set parentId={parentId} where aid={appId} and parentId=-1";
            return DAL.Base.SqlMySql.ExecuteNonQuery(dbEnum.MINIAPP.ToString(),
                CommandType.Text, sql);

        }

        /// <summary>
        /// 根据类别名称获取id集合
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public string GetEntGoodTypeIds(int appId, int pageSize, int pageIndex,string typeName)
        {
            int count = 0;
           
            List<string> listIds = new List<string>();
            List<EntGoodType> goodTypeList = new List<EntGoodType>();
            List<EntGoodType> goodTypes = GetListByCach(appId, 100, 1, ref count);//获取小类的
            if (goodTypes != null && goodTypes.Count > 0)
            {
                goodTypeList = goodTypes.FindAll(x => x.name.Contains(typeName));
                foreach(EntGoodType item in goodTypeList)
                {
                    listIds.Add(item.id.ToString());
                }
            }

            if (listIds.Count > 0)
            {
                return string.Join(",", listIds);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取产品分类（加缓存）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<EntGoodType> GetListByCach(int appId,int pageSize,int pageIndex,ref int count, int typeIndex = -1)
        {
            string strwhere = $"aid={appId} and state=1 ";
            if (typeIndex == 0)
            {
                strwhere += " and parentId=0";
            }
            else
            {
                strwhere += " and parentId<>0";
            }
            string key = string.Format(Redis_EntGoodTypeList,appId,pageSize,pageIndex, typeIndex);
            string version_key = string.Format(Redis_EntGoodTypeList_version, appId, typeIndex);
            int entGoodTypeList_version = RedisUtil.GetVersion(version_key);

            RedisModel<EntGoodType> list = RedisUtil.Get<RedisModel<EntGoodType>>(key);
            list = null;
            if (list==null || list.DataList==null || list.DataList.Count<=0 || list.Count<=0 || entGoodTypeList_version!= list.DataVersion)
            {
                list = new RedisModel<EntGoodType>();
                list.DataList = GetList(strwhere, pageSize, pageIndex, "*", "sort desc, id asc ");
                list.DataList.ForEach(x =>
                {
                    x.parentName = GetEntGoodTypeName(x.parentId.ToString());
                });
                count = GetCount(strwhere);
                list.Count = count;
                list.DataVersion = entGoodTypeList_version;

                RedisUtil.Set<RedisModel<EntGoodType>>(key,list, TimeSpan.FromHours(12));
            }
            else
            {
                count = list.Count;
            }
            
            return list.DataList;
        }

        /// <summary>
        /// 足浴版获取服务项目分类列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="id"></param>
        /// <param name="足浴版服务项目分类"></param>
        /// <returns></returns>
        public List<EntGoodType> GetServiceItemList(int aId, int storeId, int type)
        {
            return GetList($"aid ={aId} and storeid = {storeId} and type = {type} and state> 0",1000,1,"*","sort desc");
        }

        /// <summary>
        /// 清除行业类型的列表缓存
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveEntGoodTypeListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_EntGoodTypeList_version, appid, -1));
                RedisUtil.SetVersion(string.Format(Redis_EntGoodTypeList_version, appid, 0));
            }
        }

        /// <summary>
        /// 修改分类排序
        /// </summary>
        /// <param name="datajson"></param>
        /// <returns></returns>

        public bool UpdateSort(int appId,string datajson,int type)
        {
            //DateTime start = DateTime.Now;
            //获取修改排序的sql语句
            string sql = GetUpdateSortSql(type,appId);
            
            if(string.IsNullOrEmpty(sql))
            {
                return false;
            }

            TransactionModel tran = new TransactionModel();
            JArray pageArray = JArray.Parse(datajson);
            if (pageArray != null && pageArray.Count > 0)
            {
                foreach (JObject oitem in pageArray)
                {
                    int dataid = Convert.ToInt32(oitem["id"]);
                    int sort = Convert.ToInt32(oitem["sort"]);
                    tran.Add(string.Format(sql,sort, dataid));
                }
                if (base.ExecuteTransaction(tran.sqlArray))
                {
                    //DateTime end = DateTime.Now;
                    //TimeSpan span = end - start;
                    //double seconds = span.TotalSeconds;
                    //log4net.LogHelper.WriteInfo(this.GetType(), "性能测试：" + seconds + "秒");
                    return true;
                }
            }
            
            return false;
        }

        public string GetUpdateSortSql(int type,int appId)
        {
            string sql = "";
            switch (type)
            {
                case 1:
                    //清除列表缓存
                    RemoveEntGoodTypeListCache(appId);
                    sql = "update entgoodtype set sort = {0} where id={1}";
                    break;
                case 2:
                    //清除列表缓存
                    EntGoodLabelBLL.SingleModel.RemoveEntGoodLabelListCache(appId);
                    sql = "update entgoodlabel set sort = {0} where id={1}";
                    break;
                case 3:
                    //清除列表缓存
                    EntGoodsBLL.SingleModel.RemoveEntGoodListCache(appId);
                    sql = "update entgoods set sort = {0} where id={1}";
                    break;
                case 4:
                    sql = "update EntSpecification set sort = {0} where id={1}";
                    break;
                case 5:
                    sql = "update entgoodunit set sort = {0} where id={1}";
                    break;
                case 6:
                    sql = "update EntNews set SortNumber = {0} where id={1}";
                    break;
            }

            return sql;
        }

        /// <summary>
        /// 足浴版获取包间列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<EntGoodType> GetRoomList(int aId, int storeId, int type)
        {
            return GetList($"aid={aId} and storeid={storeId} and type={type} and state>0");
        }

        /// <summary>
        /// 足浴版获取包间列表
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="type"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public List<EntGoodType> GetRoomList(int aId, int storeId, int type, int pageSize, int pageIndex, out int recordCount)
        {
            string sqlwhere = $"aid={aId} and storeid={storeId} and type={type} and state>0";
            recordCount = GetCount(sqlwhere);
            return GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
        }
        /// <summary>
        /// 足浴版根据包间名称获取包间信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="足浴版包间分类"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public EntGoodType GetModelByName(int aId, int storeId, int type, string name)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string sqlwhere = $"aid={aId} and storeid={storeId} and type={type} and name=@name";
            parameters.Add(new MySqlParameter("@name", $"{name}"));
            return GetModel(sqlwhere, parameters.ToArray());
        }
        /// <summary>
        /// 足浴版获取项目分类
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public EntGoodType GetServiceItem(int aId, int storeId, int type, int id)
        {
            return GetModel($"aid ={aId} and storeid = {storeId} and type = {(int)GoodProjectType.足浴版服务项目分类} and state> 0 and id={id}");
        }
        /// <summary>
        /// 足浴版验证该分类名称是否已存在
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ValidItemName(int aId, int storeId, int type, int id, string name)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string sqlwhere = $"aid ={aId} and storeid = {storeId} and type = {(int)GoodProjectType.足浴版服务项目分类} and state> 0 and id!={id} and name=@name";
            parameters.Add(new MySqlParameter("@name", name));
            EntGoodType model = GetModel(sqlwhere, parameters.ToArray());
            return model != null;
        }
        /// <summary>
        /// 足浴版根据包间名称获取包间
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<EntGoodType> GetRoomListByName(int aId, int storeId, int type,string name)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@name", $"%{name}%"));
            string sqlwhere = $" aid={aId} and storeid={storeId} and type={type} and state>0 and name like @name";
            return GetListByParam(sqlwhere, parameters.ToArray());
        }
        /// <summary>
        /// 足浴版根据ids获取项目分类
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<EntGoodType> GetServiceItemListByIds(string ids)
        {
            return GetList($"id in ({ids}) and type = {(int)GoodProjectType.足浴版服务项目分类} and state> 0");
        }

        public List<EntGoodType> GetListByIds(int appId,string typeIds)
        {
            return GetList($"id in({typeIds}) and aid = {appId}");
        }

        public List<EntGoodType> GetListByAidParentId(int aid,int pageIndex,int pageSize,bool isParentData)
        {
            string sqlWhere = $"aid={aid} and state=1 ";
            if(isParentData)
            {
                sqlWhere += " and parentId=0";
            }
            else
            {
                sqlWhere += " and parentId<>0";
            }

            return base.GetList(sqlWhere, pageSize, pageIndex, "*", "sort desc,id asc");
        }

        public int GetCountByAid(int aid)
        {
            return base.GetCount($"aid={aid} and parentId=0 and State=1");
        }
    }
}
