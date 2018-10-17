using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Stores;
using Entity.MiniApp.ViewModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Stores
{
    public class StoreBLL : BaseMySql<Store>
    {
        private static readonly string key_store_aid = "dz_store_aid_{0}";
        private static readonly string key_store_id = "dz_store_id_{0}";

        #region 单例模式
        private static StoreBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreBLL()
        {

        }

        public static StoreBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<Store> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<Store>();

            return base.GetList($"id in ({ids})");
        }

        public override Store GetModel(int id)
        {
            if (id <= 0)
                return null;

            string model_key = string.Format(key_store_id, id);
            Store model = RedisUtil.Get<Store>(model_key);
            if (model == null)
                model = base.GetModel(id);

            if (model != null)
            {
                RedisUtil.Set(model_key, model, TimeSpan.FromHours(6));
                RedisUtil.Set(string.Format(key_store_aid, model.appId), model, TimeSpan.FromHours(6));
            }

            return model;
        }
        public override bool Update(Store model)
        {
            RemoveCache(model);
            return base.Update(model);
        }
        public override bool Update(Store model, string columnFields)
        {
            RemoveCache(model);
            return base.Update(model, columnFields);
        }
        public void RemoveCache(Store model)
        {
            RedisUtil.Remove(string.Format(key_store_id, model.Id));
            RedisUtil.Remove(string.Format(key_store_aid, model.appId));
        }

        public Store GetModelByRid(int rid)
        {
            if (rid <= 0)
                return null;

            string model_key = string.Format(key_store_aid, rid);
            Store model = RedisUtil.Get<Store>(model_key);
            // model = null;
            if (model == null)
            {
                model = base.GetModel($"appId={rid}");
                //如果缓存数据库都没有就创建一个
                if (model == null)
                {
                    model = new Store
                    {
                        appId = rid,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                    };
                    model.Id = Convert.ToInt32(Add(model));
                }
            }
            if (model != null && model.Id > 0)
                RedisUtil.Set(model_key, model, TimeSpan.FromHours(6));

            return model;
        }

        public Store GetModelByAId(int aid)
        {
            return GetModelByRid(aid);
        }

        /// <summary>
        /// 获取店铺销售成交金额
        /// </summary>
        /// <param name="storeid">店铺ID</param>
        /// <param name="startdate">开始时间</param>
        /// <param name="enddate">结束时间</param>
        /// <returns></returns>
        public int GetStoreInCome(int storeid, string startdate, string enddate)
        {
            string sql = $@"select (select sum(buyprice) from storegoodsorder where storeid = {storeid} and state ={(int)OrderState.已收货} and paydate>='{startdate}' and paydate<='{enddate}') as goodprice,
                            (select sum(gu.BuyPrice) from groupuser gu
                            left join groups g on gu.GroupId = g.Id
                             where g.storeid = {storeid} and gu.state = {(int)MiniappPayState.已收货} and gu.RecieveGoodTime>='{startdate}' and gu.RecieveGoodTime<='{enddate}') as groupprice,
                            (select sum(bu.CurrentPrice) from bargainuser bu left join bargain b on bu.BId = b.Id where b.storeid = {storeid} and bu.state = 8 and bu.ConfirmReceiveGoodsTime>='{startdate}' and ConfirmReceiveGoodsTime<='{enddate}') as bargainprice";
            int sum = 0;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    if (DBNull.Value != dr["goodprice"])
                    {
                        sum += Convert.ToInt32(dr["goodprice"]);
                    }
                    if (DBNull.Value != dr["groupprice"])
                    {
                        sum += Convert.ToInt32(dr["groupprice"]);
                    }
                    if (DBNull.Value != dr["bargainprice"])
                    {
                        sum += Convert.ToInt32(dr["bargainprice"]);
                    }
                }
            }

            return sum;
        }

        /// <summary>
        /// 获取销售前十商品
        /// </summary>
        /// <param name="storeid">店铺ID</param>
        /// <param name="odbtype">排序类型，1：销售类排序，0：销售额排序</param>
        /// <param name="limit">获取数量</param>
        /// <returns></returns>
        public List<MiniAppStoreGoods> GetStoreGroupsDescData(int storeid, int odbtype, string starttime, string endtime, int limit = 10)
        {
            string orderby = "goods.xprice";
            string wheresql = " goods.xprice>0";
            string limitsql = " LIMIT " + limit;
            if (odbtype == 1)
            {
                orderby = "goods.buynum";
                wheresql = " goods.buynum>0";
            }
            if (limit == 0)
            {
                limitsql = "";
            }

            List<MiniAppStoreGoods> groupslist = new List<MiniAppStoreGoods>();
            string sql = $@"select * from (SELECT gs.id,gs.groupname,gs.storeid,
                            (select sum(gu.buyprice) from groupuser gu where gu.groupid = gs.id and gu.state = {(int)MiniappPayState.已收货} and gu.recievegoodtime>='{starttime}' and gu.recievegoodtime<='{endtime}') xprice,
                            (select sum(gu.buynum) from groupuser gu where gu.groupid = gs.id and gu.state = {(int)MiniappPayState.已收货} and gu.recievegoodtime>='{starttime}' and gu.recievegoodtime<='{endtime}') buynum,1 as goodtype
                            from groups gs
                            where gs.storeid = {storeid}
                            UNION
                            select sg.id,sg.goodsname,sg.storeid,
                            (select sum(sc.price * sc.count) from storegoodscart sc left join storegoodsorder so on sc.goodsorderid = so.id where so.state = {(int)OrderState.已收货} and sc.goodsid = sg.id and sc.state = 1 and so.paydate>='{starttime}' and so.paydate<='{endtime}') xprice,
                            (select sum(sc.count) from storegoodscart sc left join storegoodsorder so on sc.goodsorderid = so.id where so.state = {(int)OrderState.已收货} and sc.goodsid = sg.id and sc.state = 1 and so.paydate>='{starttime}' and so.paydate<='{endtime}') buynum,2 as goodtype
                                                            from storegoods sg
                                                        where sg.storeid = {storeid}
                            UNION
                            select b.id,b.bname,storeid,
                            (select sum(CurrentPrice) from bargainuser bu where bu.BId=b.id and bu.state = 8 and ConfirmReceiveGoodsTime>='{starttime}' and ConfirmReceiveGoodsTime<='{endtime}') xprice,
                            (select count(*) from bargainuser bu where bu.BId=b.id and bu.state = 8 and ConfirmReceiveGoodsTime>='{starttime}' and ConfirmReceiveGoodsTime<='{endtime}') buynum,3 as goodtype
                             from bargain b
                            where b.StoreId={storeid}
                            ) goods where {wheresql}
                            order by {orderby} DESC
                            {limitsql}";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    MiniAppStoreGoods model = new MiniAppStoreGoods();
                    model.Id = Convert.ToInt32(dr["id"]);
                    model.StoreId = Convert.ToInt32(dr["storeid"]);
                    model.GoodsName = dr["groupname"].ToString();
                    model.IsSell = Convert.ToInt32(dr["goodtype"]);
                    if (DBNull.Value != dr["xprice"])
                    {
                        model.Price = Convert.ToInt32(dr["xprice"]);
                    }
                    if (DBNull.Value != dr["buynum"])
                    {
                        model.salesCount = Convert.ToInt32(dr["buynum"]);
                    }

                    groupslist.Add(model);
                }
            }

            return groupslist;
        }

        /// <summary>
        /// 获取获取商品分类销售情况
        /// </summary>
        /// <param name="storeid">店铺ID</param>
        public List<MiniAppStoreGoods> GetGoodTypeInCome(int storeid, string starttime, string endtime, ref int salesum)
        {
            string sql = $@"select *,sum(goodtypes.xprice) as salesum from (
                            select st.id,st.name,st.storeid,st.State,
                            (select sum(sc.price * sc.count) from storegoodscart sc left join storegoodsorder so on sc.goodsorderid = so.id where so.state = {(int)OrderState.已收货} and sc.goodsid = sg.id and sc.state = 1 and so.paydate>='{starttime}' and so.paydate<='{endtime}' ) xprice
                            from storegoodstype st left join storegoods sg on st.id = sg.TypeId
                            where st.storeid = {storeid} and st.state>-1
                            ) goodtypes
                            group by goodtypes.id";
            List<MiniAppStoreGoods> groupslist = new List<MiniAppStoreGoods>();
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    MiniAppStoreGoods model = new MiniAppStoreGoods();
                    model.TypeId = Convert.ToInt32(dr["id"]);
                    model.StoreId = Convert.ToInt32(dr["storeid"]);
                    model.GoodsName = dr["name"].ToString();
                    if (DBNull.Value != dr["salesum"])
                    {
                        model.Price = Convert.ToInt32(dr["salesum"]);
                        salesum += model.Price;
                    }

                    groupslist.Add(model);
                }
            }

            return groupslist;
        }

        public bool UpdateConfig(Store model, string field, object value)
        {
            if (model == null || string.IsNullOrWhiteSpace(field))
            {
                return false;
            }

            Dictionary<string, object> updateSet = new Dictionary<string, object>();
            updateSet.Add(field, value);
            return UpdateConfig(model, updateSet);
        }

        public bool UpdateConfig(Store model, Dictionary<string, object> updateSet)
        {
            if (model == null || updateSet == null || updateSet.Count == 0)
            {
                return false;
            }

            StoreConfigModel config = string.IsNullOrWhiteSpace(model.configJson) ? new StoreConfigModel() : JsonConvert.DeserializeObject<StoreConfigModel>(model.configJson);

            try
            {
                config = SetConfigValue(updateSet, config);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return false;
            }

            model.configJson = JsonConvert.SerializeObject(config);
            return Update(model, "configJson");
        }

        public StoreConfigModel SetConfigValue(Dictionary<string, object> updateList, StoreConfigModel config)
        {
            Type configType = typeof(StoreConfigModel);
            foreach (var set in updateList)
            {
                object newValue = Convert.ChangeType(set.Value, configType.GetProperty(set.Key).PropertyType);
                configType.GetProperty(set.Key).SetValue(config, newValue, null);
            }
            return config;
        }

        public string GetStoreNameEnt(Store store)
        {
            //if (store == null)
            //{
            //    return string.Empty;
            //}
            if (!string.IsNullOrWhiteSpace(store.name))
            {
                return store.name;
            }
            OpenAuthorizerConfig XUserList = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(string.Empty, store.appId);
            return XUserList?.nick_name;
        }

        public StoreConfigModel GetStoreConfig(int aid)
        {
            Store store = GetModelByAId(aid);
            StoreConfigModel config = null;
            if (!string.IsNullOrWhiteSpace(store.configJson))
            {
                try { config = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson); } catch { }
            }
            return config ?? new StoreConfigModel();
        }
    }
}
