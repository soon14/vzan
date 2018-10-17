using DAL.Base;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Core.MiniApp;
using System.Data;
using Utility;
using Newtonsoft.Json;
using Entity.MiniApp;

namespace BLL.MiniApp.Dish
{
    public class DishStoreBLL : BaseMySql<DishStore>
    {
        #region 单例模式
        private static DishStoreBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishStoreBLL()
        {

        }

        public static DishStoreBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishStoreBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public ShopInfo GetAddressPoint(int id)
        {
            ShopInfo shopinfo = new ShopInfo();
            DishStore dishstore = base.GetModel(id);
            if (dishstore == null)
            {
                return shopinfo;
            }
            dishstore.baseConfig = JsonConvert.DeserializeObject<DishBaseConfig>(dishstore.baseConfigJson) ?? new DishBaseConfig();
            shopinfo.ShopAddress = dishstore.baseConfig.dish_address;
            shopinfo.ShopName = dishstore.dish_name;
            shopinfo.ShopTag = dishstore.ws_lng + "," + dishstore.ws_lat;
            shopinfo.Lat = dishstore.ws_lat.ToString();
            shopinfo.Lng = dishstore.ws_lng.ToString();
            shopinfo.ShopTelphone = dishstore.baseConfig.dish_con_phone;
            
            return shopinfo;
        }
        public List<DishStore> GetListFromTable(int pageIndex, int pageSize, string kw = "", int aId = 0)
        {
            List<MySqlParameter> parameters = null;
            StringBuilder sqlFilter = new StringBuilder();

            if (!string.IsNullOrEmpty(kw))
            {
                if (parameters == null)
                    parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter("@kw", Utils.FuzzyQuery(kw)));
                sqlFilter.Append(" and dish_name like @kw ");
            }
            DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT s.*,(SELECT title from dishcategory c where c.id=s.dish_cate_id and c.type=1) as cat_name from dishstore s where s.state<>-1 and s.aid={aId} {sqlFilter}  limit {pageSize * pageIndex},{pageSize}", parameters?.ToArray()).Tables[0];
            return DataHelper.ConvertDataTableToList<DishStore>(dt);
        }

        /// <summary>
        /// 查询正常的门店
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="dish_name"></param>
        /// <param name="dish_con_mobile"></param>
        /// <returns></returns>
        public List<DishStore> GetAvailableStore(int aid, string dish_name = "", string dish_con_mobile = "", int state = 1)
        {
            StringBuilder sqlFilter = new StringBuilder();
            sqlFilter.Append($" aid = {aid} and state = {state} ");
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();

            if (!string.IsNullOrEmpty(dish_name))
            {
                sqlFilter.Append($" and dish_name like @dish_name ");
                mysqlParams.Add(new MySqlParameter("@dish_name", Utils.FuzzyQuery(dish_name)));
            }
            if (!string.IsNullOrEmpty(dish_con_mobile))
            {
                sqlFilter.Append(" and dish_con_mobile like @dish_con_mobile ");
                mysqlParams.Add(new MySqlParameter("@dish_con_mobile", Utils.FuzzyQuery(dish_con_mobile)));
            }
            DateTime now = DateTime.Now;
            sqlFilter.Append($" and ( dish_begin_time<= @now and @now <=dish_end_time) ");
            mysqlParams.Add(new MySqlParameter("@now", now));
            return base.GetListByParam(sqlFilter.ToString(), mysqlParams.ToArray());
        }

        public DishStore GetModelByAid_Id(int aid, int storeId)
        {
            string sqlwhere = $" aid = {aid} and state != -1 and Id={storeId}";
            return GetModel(sqlwhere);
        }

        public bool CheckExistLoginName(int id, int aId, string login_username)
        {
            string whereSql = $" login_username = @login_username and id != @id ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@id", id));
            mysqlParams.Add(new MySqlParameter("@login_username", login_username));

            return base.Exists(whereSql, mysqlParams.ToArray());
        }

        public DishStore GetAdminByLoginParams(string login_username, string login_userpass)
        {
            try
            {
                MySqlParameter[] paras = new MySqlParameter[]{
                                 new MySqlParameter("@login_username",login_username),
                                 new MySqlParameter("@login_userpass",DESEncryptTools.GetMd5Base32(login_userpass)),
                };
                DishStore admin = base.GetModel(" login_username = @login_username and login_userpass = @login_userpass  ", paras);
                return admin;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<DishStore> GetListByCondition(int aid, double ws_lat, double ws_lng, int sort_type, string dish_name, int cate_id, int pageIndex, int pageSize)
        {
            string sqlwhere = $"a.aid={aid} and a.state>=0 ";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            List<DishStore> list = new List<DishStore>();
            if (!string.IsNullOrEmpty(dish_name))
            {
                sqlwhere += " and a.dish_name like @dish_name ";
                parameters.Add(new MySqlParameter("@dish_name", Utils.FuzzyQuery(dish_name)));
            }
            if (cate_id > 0)
            {
                sqlwhere += $" and a.dish_cate_id={cate_id}";
            }
            string orderByStr = string.Empty;
            string sql = string.Empty;
            switch (sort_type)
            {
                //综合排序
                case 1:
                    orderByStr = "order by count desc,weight asc ";
                    sql = $"select a.*,count(b.id) as count,(count(a.id)/a.commentcount) as weight,ROUND(6378.138*2*ASIN(SQRT(POW(SIN(({ws_lat}*PI()/180-ws_lat*PI()/180)/2),2)+COS({ws_lat}*PI()/180)*COS(ws_lat*PI()/180)*POW(SIN(({ws_lng}*PI()/180-ws_lng*PI()/180)/2),2)))*1000) AS juli  from dishstore as a left join  (select * from dishorder where is_delete=0 and (order_status=1 or order_status=2))as b  on a.id=b.storeId where {sqlwhere} group by a.id {orderByStr}";
                    break;
                //销量最高
                case 2:
                    orderByStr = "order by count desc";
                    sql = $"select a.*,count(b.id) as count,ROUND(6378.138*2*ASIN(SQRT(POW(SIN(({ws_lat}*PI()/180-ws_lat*PI()/180)/2),2)+COS({ws_lat}*PI()/180)*COS(ws_lat*PI()/180)*POW(SIN(({ws_lng}*PI()/180-ws_lng*PI()/180)/2),2)))*1000) AS juli  from dishstore as a left join (select * from dishorder where is_delete=0 and (order_status=1 or order_status=2))as b on a.id=b.storeId where {sqlwhere} group by a.id {orderByStr}";
                    break;
                //距离最近
                case 3:
                    orderByStr = "order by juli asc";
                    sql = $"select *,ROUND(6378.138*2*ASIN(SQRT(POW(SIN(({ws_lat}*PI()/180-ws_lat*PI()/180)/2),2)+COS({ws_lat}*PI()/180)*COS(ws_lat*PI()/180)*POW(SIN(({ws_lng}*PI()/180-ws_lng*PI()/180)/2),2)))*1000) AS juli  from dishstore as a where {sqlwhere} {orderByStr}";
                    break;
            }
            using (var dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql, parameters.ToArray()))
            {
                while (dr.Read())
                {
                    DishStore store = GetModel(dr);
                    store.dish_julimi = (Convert.ToInt32(dr["juli"]) * 0.001) < 1 ? $"{dr["juli"].ToString()}m" : $"{Convert.ToInt32(dr["juli"]) * 0.001}km";
                    list.Add(store);
                }
                return list;
            }
        }

        public void GetDishActivityInfo(ref DishStore model,int userid=0)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //如果开启首单立减并且立减金额>0
            if (model.gaojiConfig.huodong_shou_isopen == 1 && model.gaojiConfig.huodong_shou_jiner > 0)
            {
                huodong_item huodong_shou = new huodong_item
                {
                    hd_info = $"本店新用户立减{model.gaojiConfig.huodong_shou_jiner}元",
                    hd_style = "greenbg",
                    hd_title = "新",
                    s_hd_style = "zhct-bggreen"
                };
                model.huodong_list.Add("huodong_shou", huodong_shou);
            }
            //查询当前店铺的活动：满减，代金券
            List<DishActivity> storeActivity = DishActivityBLL.SingleModel.GetList($"aid={model.aid} and storeId={model.id} and state=1 and q_begin_time<'{now}' and '{now}'<q_end_time and q_diyong_jiner>0");
            List<DishActivity> huodong_man_list = storeActivity?.Where(p => p.q_type == 2)?.ToList();
            //满减
            if (huodong_man_list != null && huodong_man_list.Count > 0)
            {
                huodong_item huodong_man = new huodong_item
                {
                    hd_info = string.Join(",", huodong_man_list.Select(p => $"满{p.q_xiaofei_jiner}减{p.q_diyong_jiner}")),
                    hd_style = "orangebg",
                    hd_title = "减",
                    s_hd_style = "zhct-bggreen",
                };
                model.huodong_list.Add("huodong_man", huodong_man);

            }
            //代金券
            double quan_money_all = DishActivityBLL.SingleModel.GetQuanJiner(model.id, userid);
            if (quan_money_all>0)
            {
                huodong_item huodong_quan = new huodong_item
                {
                    hd_info = $"可领{quan_money_all}元代金券",
                    hd_style = "redbg",
                    hd_title = "领",
                    s_hd_style = "zhct-bgred",
                };
                model.huodong_list.Add("huodong_quan", huodong_quan);
            }
        }

        public override DishStore GetModel(int id)
        {
            DishStore store = base.GetModel(id);
            if(store != null)
            {
                DisposeStoreConfig(store);
            }

            return store;
        }

        public bool DelStore(DishStore store)
        {
            store.state = -1;
            return base.Update(store, "state");
        }

        public void DisposeStoreConfig(DishStore store)
        {
            try { store.baseConfig = JsonConvert.DeserializeObject<DishBaseConfig>(store.baseConfigJson) ?? new DishBaseConfig(); }
            catch (Exception) { store.baseConfig = new DishBaseConfig(); }
            try { store.gaojiConfig = JsonConvert.DeserializeObject<DishGaojiConfig>(store.gaojiConfigJson) ?? new DishGaojiConfig(); }
            catch (Exception) { store.gaojiConfig = new DishGaojiConfig(); }
            try { store.takeoutConfig = JsonConvert.DeserializeObject<DishTakeoutConfig>(store.takeoutConfigJson) ?? new DishTakeoutConfig(); }
            catch (Exception) { store.takeoutConfig = new DishTakeoutConfig(); }
            try { store.dianneiConfig = JsonConvert.DeserializeObject<DishDianneiConfig>(store.dianneiConfigJson) ?? new DishDianneiConfig(); }
            catch (Exception) { store.dianneiConfig = new DishDianneiConfig(); }
            try { store.paySetting = JsonConvert.DeserializeObject<DishPaySetting>(store.paySettingJson) ?? new DishPaySetting(); }
            catch (Exception) { store.paySetting = new DishPaySetting(); }

        }


        public List<DishStore> GetStoreByIds(int[] ids)
        {
            if (ids == null || ids.Length == 0) return null;


            return GetList($" id in ({string.Join(",", ids)}) ");
        }

        public List<DishStore> GetListByAids(string aId, int pageIndex, int pageSize)
        {
            return GetList($"aid in ({aId}) AND State >= 0", pageSize, pageIndex);
        }

        public List<DishStore> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<DishStore>();

            return base.GetList($"id in ({ids})");
        }

        public int GetCountByAids(string aId)
        {
            return GetCount($"aid in ({aId})");
        }

        public bool SetMainStore(int isMain, int aId, int storeId)
        {
            return base.ExecuteTransaction(new string[] 
            {
                $" update dishstore set ismain=0 where aid={aId}",
                $" update dishstore set ismain={isMain} where id={storeId}"
            });
        }
    }
}
