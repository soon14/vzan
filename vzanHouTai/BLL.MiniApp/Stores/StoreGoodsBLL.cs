using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BLL.MiniApp.Stores
{
    public class StoreGoodsBLL : BaseMySql<StoreGoods>
    {
        #region 单例模式
        private static StoreGoodsBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreGoodsBLL()
        {

        }

        public static StoreGoodsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreGoodsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// model_key  
        /// </summary>
        public static readonly string Redis_StoreGoods = "Miniapp_StoreGoods_{0}";

        /// <summary>
        /// model_version_key  
        /// </summary>
        public static readonly string Redis_StoreGoods_version = "Miniapp_StoreGoods_version_{0}";

        /// <summary>
        /// model_key list_根据storeId
        /// </summary>
        public static readonly string Redis_StoreGoodsList = "Miniapp_StoreGoodsList_{0}_{1}_{2}_{3}";

        /// <summary>
        /// model_key list_根据storeId
        /// </summary>
        public static readonly string Redis_StoreGoodsList_version = "Miniapp_StoreGoodsList_version_{0}";

        #region 基本查询
        public List<StoreGoods> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<StoreGoods>();
            return base.GetList($"id in ({ids})");
        }
        #endregion


        public StoreGoods GetModelByCache(int id)
        {
            string model_key = string.Format(Redis_StoreGoods, id);
            string version_key = string.Format(Redis_StoreGoods_version, id);

            int version = RedisUtil.GetVersion(version_key);
            RedisModel<StoreGoods> redisModel_StoreGood = RedisUtil.Get<RedisModel<StoreGoods>>(model_key);

            //if (redisModel_StoreGood == null || redisModel_StoreGood.DataList == null 
            //        || redisModel_StoreGood.DataList.Count <= 0 || redisModel_StoreGood.DataVersion != version)
            //{
                redisModel_StoreGood = new RedisModel<StoreGoods>();
                List<StoreGoods> StoreGoods = new List<StoreGoods>() { GetModel(id) };

                redisModel_StoreGood.DataList = StoreGoods;
                redisModel_StoreGood.DataVersion = version;
                redisModel_StoreGood.Count = StoreGoods.Count;

                RedisUtil.Set<RedisModel<StoreGoods>>(model_key, redisModel_StoreGood, TimeSpan.FromHours(12));
            //}
            return redisModel_StoreGood.DataList[0];
        }

        /// <summary>
        /// 更新此列表缓存最新版本号,达到加载覆盖旧数据
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveStoreGoodsCache(int id)
        {
            if (id > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_StoreGoods_version, id));

                StoreGoods curGoods = GetModel(id);
                if (curGoods != null)
                {
                    RedisUtil.SetVersion(string.Format(Redis_StoreGoodsList_version, curGoods.StoreId)); 
                }
            }
        }


        //删除商品多规格关系表
        public int DelAttrList(int goodsid)
        {
            string sql = $"delete from StoreGoodsAttrSpec where goodsid={goodsid}";
            int result = SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);
            return result;
        }

        public List<StoreGoods> GetListByStoreId(int sid,int pageindex=1,int pagesize=10)
        {
            var info = GetList($"StoreId={sid} and State>=0", pagesize, pageindex, "", "Id Desc");
            return info;
        }

        public List<StoreGoods> GetListByStoreId(int sid, int typeid ,int pageindex = 1, int pagesize = 10,int orderbyid=0,string goodname="")
        {
            List<MySqlParameter> listParam = new List<MySqlParameter>();

            string models_key = string.Format(Redis_StoreGoodsList, sid, typeid, pageindex, pagesize);
            string version_key = string.Format(Redis_StoreGoodsList_version, sid);
            int version = RedisUtil.GetVersion(version_key);
            bool noWhereOrderby = true;//是否默认条件及排序

            string where = $" StoreId=@StoreId and State>=0 and IsSell = 1 ";
            listParam.Add(new MySqlParameter("@StoreId", sid));
            if (typeid>0)
            {
               where += $" and TypeId = @TypeId";
                listParam.Add(new MySqlParameter("@TypeId", typeid));

                noWhereOrderby = false;
            }
            if(!string.IsNullOrEmpty(goodname))
            {
                where += $" and GoodsName like @GoodsName";
                listParam.Add(new MySqlParameter("@GoodsName", "%"+goodname+"%"));

                noWhereOrderby = false;
            }
            string orderstr = "Sort Desc,Id Desc";
            switch(orderbyid)
            {
                case 0:
                    orderstr = "Sort Desc,Id Desc";
                    break;
                case 1:
                    orderstr = "Price Desc";

                    noWhereOrderby = false;
                    break;
                case 2:
                    orderstr = "Price asc";

                    noWhereOrderby = false;
                    break;
                case 3:
                    orderstr = "Inventory Desc";

                    noWhereOrderby = false;
                    break;
                case 4:
                    orderstr = "Inventory asc";

                    noWhereOrderby = false;
                    break;
                case 5:
                    orderstr = "(Inventory - Stock) Desc";

                    noWhereOrderby = false;
                    break;
                case 6:
                    orderstr = "(Inventory - Stock) asc";

                    noWhereOrderby = false;
                    break;
            }
            listParam.Add(new MySqlParameter("@pageindex", (pageindex-1)* pagesize));
            listParam.Add(new MySqlParameter("@pagesize", pagesize));

            
            RedisModel<StoreGoods> redisModel_StoreGoods = RedisUtil.Get<RedisModel<StoreGoods>>(models_key);
            //if (!noWhereOrderby || redisModel_StoreGoods == null 
            //        || redisModel_StoreGoods.DataList == null || redisModel_StoreGoods.DataList.Count <= 0 || redisModel_StoreGoods.DataVersion != version)
            //{
                redisModel_StoreGoods = new RedisModel<StoreGoods>();

                string sql = $@"select * from StoreGoods
                           where {where} ORDER BY {orderstr}  LIMIT @pageindex,@pagesize";

                List<StoreGoods> list = new List<StoreGoods>();
                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, listParam.ToArray()))
                {
                    while (dr.Read())
                    {
                        var model = GetModel(dr);
                        list.Add(model);
                    }
                }

                redisModel_StoreGoods.DataList = list;
                redisModel_StoreGoods.DataVersion = version;
                redisModel_StoreGoods.Count = list.Count;

                if (noWhereOrderby)//只写默认条件的缓存
                {
                    RedisUtil.Set<RedisModel<StoreGoods>>(models_key, redisModel_StoreGoods, TimeSpan.FromHours(12));
                }
            //}
            
            return redisModel_StoreGoods.DataList;
        }

        /// <summary>
        /// 查询当前商品库存
        /// </summary>
        /// <param name="goodsid"></param>
        /// <param name="attrSpacStr"></param>
        /// <returns></returns>
        public int GetGoodQty(int goodsid,string attrSpacStr = "")
        {
            var goodQty = 0;

            var good = GetModel(goodsid);
            if (string.IsNullOrWhiteSpace(attrSpacStr))
            {
                goodQty = good.Stock;
            }
            else
            {
                var goodBySpacStr = good.GASDetailList.Where(x => x.id.Equals(attrSpacStr))?.First();
                if (goodBySpacStr != null)
                {
                    goodQty = goodBySpacStr.count;
                }
            }
            return goodQty;
        }

        /// <summary>
        /// 引用某分类的商品记录总数
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public int GetCountByTypeId(int typeId)
        {
            return GetCount($" TypeId={typeId} And  State >=0 ");
        }

        /// <summary>
        /// 获取电商销售前十普通商品
        /// </summary>
        /// <param name="aid"></param>
        public List<StoreGoods> GetStoreGoodsDescData(int storeid)
        {
            List<StoreGoods> goodlist = new List<StoreGoods>();
            string sql = $@"select sg.id,sg.storeid,sg.goodsname,
                            (select sum(sc.price * sc.count) from storegoodscart sc left
                                                             join storegoodsorder so on sc.goodsorderid = so.id
                            where so.state = 5 and sc.goodsid = sg.id and sc.state = 1) xprice
                                from storegoods sg
                            where sg.storeid = {storeid}
                            ORDER BY xprice DESC
                            LIMIT 10";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    StoreGoods model = new StoreGoods();
                    model.Id = Convert.ToInt32(dr["id"]);
                    model.StoreId = Convert.ToInt32(dr["storeid"]);
                    model.GoodsName = dr["goodsname"].ToString();
                    model.Price = Convert.ToInt32(dr["xprice"]);

                    goodlist.Add(model);
                }
            }

            return goodlist;
        }


        /// <summary>
        /// 电商版商品批量更新排序
        /// </summary>
        /// <param name="list"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public bool UpdateListSort(List<StoreGoods> list, int storeId)
        {
            if (list == null || list.Count <= 0)
            {
                return false;
            }
            DateTime date = DateTime.Now;

            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                string sql = $"update  StoreGoods set Sort={item.Sort},UpdateDate='{date}'  where Id={item.Id};";
                sb.Append(sql);
            }
            if (list != null && list.Count > 0)
            {
                this.RemoveStoreGoodsCache(list[0].Id);
            }
           
            return ExecuteNonQuery(sb.ToString()) > 0;
        }

    }
}
