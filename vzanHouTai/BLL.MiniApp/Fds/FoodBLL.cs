using System;
using DAL.Base;
using Entity.MiniApp.Fds;
using Newtonsoft.Json;
using System.Collections.Generic;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System.Linq;

namespace BLL.MiniApp.Fds
{
    public class FoodBLL : BaseMySql<Food>
    {
        #region 单例模式
        private static FoodBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodBLL()
        {

        }

        public static FoodBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 餐饮版缓存图片 存在附件表里的 {餐饮版Id}{AttachmentItemType}
        /// </summary>
        public string foodAttImgKey = "foodAttImg_{0}_{1}";
        public string foodStoreKey = "foodstore_{0}_{1}";

        public ShopInfo GetAddressPoint(int aid)
        {
            ShopInfo shopinfo = new ShopInfo();
            Food food = base.GetModel($"appid={aid}");
            if(food==null)
            {
                return shopinfo;
            }
            shopinfo.ShopAddress = food.Address;
            shopinfo.ShopName = food.FoodsName;
            shopinfo.ShopTag = food.Lng + "," + food.Lat ;
            shopinfo.Lat = food.Lat.ToString();
            shopinfo.Lng = food.Lng.ToString();
            shopinfo.ShopTelphone = food.TelePhone;

            return shopinfo;
        }

        public override object Add(Food model)
        {
            string addSql = BuildAddSql(model);
            addSql += "select last_insert_id(); ";
            return SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, addSql, null);
        }

        public override bool Update(Food model, string columnFields)
        {
            string updateSql = base.BuildUpdateSql(model, columnFields) + ";";
            ClearRedisByAppId(model.appId, model.masterStoreId > 0 ? model.Id : 0);
            return SqlMySql.ExecuteNonQuery(connName, System.Data.CommandType.Text, updateSql, null) > 0;
        }

        public override string BuildUpdateSql(Food model, string columnFields)
        {
            string updateSql = base.BuildUpdateSql(model, columnFields) + ";";
            ClearRedisByAppId(model.appId, model.masterStoreId > 0 ? model.Id : 0);
            return updateSql;
        }


        public override bool Update(Food model)
        {
            string updateSql = BuildUpdateSql(model) + ";";

            return SqlMySql.ExecuteNonQuery(connName, System.Data.CommandType.Text, updateSql, null) > 0;
        }

        /// <summary>
        /// 根据appid获取店铺信息(加了缓存的)
        /// </summary>
        /// <param name="appId">小程序aid</param>
        /// <returns></returns>
        public Food GetModelByAppId(int appId,int storeId = 0)
        {
            string key = string.Format(foodStoreKey, appId, storeId);
            Food model = RedisUtil.Get<Food>(key);
            if (model == null)
            {
                if (storeId > 0)
                {
                    model = GetModel($" Id = {storeId} and appId={appId} ");
                }
                else
                {
                    model = GetModel($" appId={appId} and masterStoreId = 0 ");
                }
                
                if (model != null)
                {
                    RedisUtil.Set(key, model, TimeSpan.FromMinutes(30));
                }
            }
            return model;

        }
        public void ClearRedisByAppId(int appId,int storeId = 0)
        {
            string key = string.Format(foodStoreKey, appId, storeId);
            if (appId > 0)
            {
                RedisUtil.Remove(key);
            }
        }

        public void RemoveCachByKey(string key)
        {
            RedisUtil.Remove(key);
        }

        public bool UpdateConfigJson(Food store, string option, string value)
        {
            FoodConfigModel newConfig = null;
            switch (option.ToLower())
            {
                case "reservation":
                    newConfig = store.funJoinModel;
                    newConfig.reservationSwitch = value == "true";
                    break;
                case "reservationprint":
                    newConfig = store.funJoinModel;
                    newConfig.reservationPrint = value == "true";
                    break;
            }
            if (newConfig != null)
            {
                store.configJson = JsonConvert.SerializeObject(newConfig);
                return Update(store, "configJson,UpdateDate");
            }
            return false;
        }


        public List<string> GetAddFoodSQL(Guid accountid, int scount, int rid, DateTime overtime, int storeid = 0)
        {
            List<string> sqllist = new List<string>();
            Food miniaccount = new Food();
            if (scount > 0)
            {
                if (storeid == 0)
                {
                    //添加总店
                    miniaccount = new Food()
                    {
                        masterStoreId = storeid,
                        appId = rid,
                        state = 1,
                        CreateDate = DateTime.Now,
                        overTime = overtime,
                    };
                    storeid = Convert.ToInt32(Add(miniaccount));

                    //添加总店权限
                    UserRole userrole = new UserRole();
                    userrole.AppId = rid;
                    userrole.CreateDate = DateTime.Now;
                    userrole.RoleId = 1;
                    userrole.State = 1;
                    userrole.StoreId = 0;
                    userrole.UpdateDate = DateTime.Now;
                    userrole.UserId = accountid;
                    UserRoleBLL.SingleModel.Add(userrole);
                }

                //添加分店
                for (int i = 0; i < scount; i++)
                {
                    miniaccount = new Food()
                    {
                        masterStoreId = storeid,
                        appId = rid,
                        CreateDate = DateTime.Now,
                        overTime = overtime,
                    };
                    sqllist.Add(BuildAddSql(miniaccount));
                }
            }

            return sqllist;
        }
    }
}
