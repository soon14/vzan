using BLL.MiniApp.Conf;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
    public class PlatStoreBLL : BaseMySql<PlatStore>
    {
        #region 单例模式
        private static PlatStoreBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatStoreBLL()
        {

        }

        public static PlatStoreBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatStoreBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public static readonly string _redis_PlatStoreCodeUrlKey = "redis_PlatStoreCodeUrl_{0}";
        
        public PlatStore GetModelBycardid(int bindPlatAid, long myCardId)
        {
            return base.GetModel($"BindPlatAid={bindPlatAid} and MyCardId={myCardId} and State<>-1");
        }

        public PlatStore GetModelByAId(int aid)
        {
            return base.GetModel($"aid={aid}");
        }
        
        public PlatStore GetPlatStore(int id, int type = 0)
        {

            if (type == 1)
            {
                //根据名片Id查找店铺
                return base.GetModel($"MyCardId={id} and State<>-1");
            }
            else if (type == 2)
            {
                //根据绑定的小程序来找店铺 从子模板小程序查看绑定的店铺
                return base.GetModel($"Aid={id}  and State<>-1");
            }
            else
            {
                return base.GetModel($"Id={id}  and State<>-1");
            }

        }

        public List<PlatStore> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PlatStore>();

            return base.GetList($"id in ({ids})");
        }

        public int GetCountByAId(int aid,string appId="")
        {
            string sqlwhere = $"BindPlatAid={aid} and state>=0 and MyCardId in({PlatMyCardBLL.SingleModel.GetCardIds(aid, appId)})";
            return base.GetCount(sqlwhere);
        }

        public List<PlatStore> GetListByBindAids(string bindPlatAids)
        {
            if (string.IsNullOrEmpty(bindPlatAids))
                return new List<PlatStore>();

            return base.GetList($"BindPlatAid in({bindPlatAids}) and State<>-1");
        }

        public List<PlatStore> GetListByBindAid(string bindPlatAids)
        {
            if (string.IsNullOrEmpty(bindPlatAids))
                return new List<PlatStore>();

            return base.GetList($"BindPlatAid in({bindPlatAids}) and State<>-1");
        }
        
        /// <summary>
        /// 运费模板设置
        /// </summary>
        /// <param name="model"></param>
        /// <param name="updateSet"></param>
        /// <returns></returns>
        public bool UpdateConfig(PlatStore model, Dictionary<string, object> updateSet)
        {
            if (model == null || updateSet == null || updateSet.Count == 0)
            {
                return false;
            }

            PlatStoreSwitchModel config = string.IsNullOrWhiteSpace(model.SwitchConfig) ? new PlatStoreSwitchModel() : JsonConvert.DeserializeObject<PlatStoreSwitchModel>(model.SwitchConfig);

            try
            {
                config = SetConfigValue(updateSet, config);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return false;
            }

            model.SwitchConfig = JsonConvert.SerializeObject(config);
            return Update(model, "SwitchConfig");
        }
        public PlatStoreSwitchModel SetConfigValue(Dictionary<string, object> updateList, PlatStoreSwitchModel config)
        {
            Type configType = typeof(PlatStoreSwitchModel);
            foreach (var set in updateList)
            {
                object newValue = Convert.ChangeType(set.Value, configType.GetProperty(set.Key).PropertyType);
                configType.GetProperty(set.Key).SetValue(config, newValue, null);
            }
            return config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aid">平台小程序appId</param>
        /// <param name="totalCount"></param>
        /// <param name="storeName">店铺名称</param>
        /// <param name="appName">绑定的小程序名称</param>
        /// <param name="agentName">分销代理商名称</param>
        /// <param name="categoryName">原平台类别名称</param>
        /// <param name="storeState">店铺状态</param>
        /// <param name="haveAid">是否绑定了小程序0 未绑定 1绑定了</param>
        /// <param name="fromType">数据来源0 店铺入驻 1代理分销商同步</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="orderWhere"></param>
        /// <returns></returns>
        public List<PlatStoreRelation> GetListStore(int aid, out int totalCount, string storeName = "", string appName = "", string agentName = "", string categoryName = "", int storeState = -1, int haveAid = -1, int fromType = -1, int pageSize = 10, int pageIndex = 1, string orderWhere = "AddTime desc", string storeOwnerPhone = "", int isFirstType = 0, int categoryId = 0,string appId="")
        {
            List<PlatStore> list = new List<PlatStore>();
            List<PlatStoreRelation> listPlatStoreRelation = new List<PlatStoreRelation>();
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string strWhere = $" psr.Aid={aid} and s.Id>0 and s.state<>-1 and s.MyCardId in({PlatMyCardBLL.SingleModel.GetCardIds(aid, appId)})";
            if (storeState != -1)
            {
                strWhere += $" and psr.State={storeState}";
            }
            if (haveAid != -1)
            {
                if (haveAid == 0)
                {
                    strWhere += $" and s.Aid=0";
                }
                else
                {
                    strWhere += $" and s.Aid<>0"; //表示店铺已经绑定独立小程序
                }

            }

            if (fromType != -1)
            {
                strWhere += $" and psr.FromType={fromType}";
            }

            if (!string.IsNullOrEmpty(storeOwnerPhone))
            {
                parameters.Add(new MySqlParameter("@storeOwnerPhone", $"%{storeOwnerPhone}%"));
                strWhere += " and c.Phone like @storeOwnerPhone";
            }

            if (!string.IsNullOrEmpty(storeName))
            {
                parameters.Add(new MySqlParameter("@storeName", $"%{storeName}%"));
                strWhere += " and s.Name like @storeName";
            }
            if (!string.IsNullOrEmpty(appName))//根据店铺绑定的小程序名称模糊查询
            {
                List<int> listRids = OpenAuthorizerConfigBLL.SingleModel.GetRidByAppName(appName);
                if (listRids == null || listRids.Count <= 0)
                {
                    listRids.Add(0);
                }
                strWhere += $" and s.Aid in ({string.Join(",", listRids)})";
            }
            if (!string.IsNullOrEmpty(agentName))//根据分销代理商名称模糊查询
            {
                List<int> listAgentId = AgentinfoBLL.SingleModel.GetAgentIdByAgentName(agentName);
                if (listAgentId == null || listAgentId.Count <= 0)
                {
                    listAgentId.Add(0);
                }
                strWhere += $" and psr.AgentId in ({string.Join(",", listAgentId)})";

            }

            if (!string.IsNullOrEmpty(categoryName))//根据类别名称(小类 因为店铺最终属于某个小类)模糊查询
            {
                List<int> listCategoryId = PlatStoreCategoryBLL.SingleModel.GetCategoryIdName(categoryName);
                if (listCategoryId == null || listCategoryId.Count <= 0)
                {
                    listCategoryId.Add(0);
                }

                strWhere += $" and s.Category in ({string.Join(",", listCategoryId)})";
            }

            if (isFirstType == 1 && categoryId > 0)
            {
                //表示传过来的是店铺大类 先查询出其下面的小类id
                strWhere += $" and s.Category in ({PlatStoreCategoryBLL.SingleModel.GetChildIds(aid, categoryId)})";
            }

            if (isFirstType == 2 && categoryId > 0)
            {
                //表示传过来的是店铺大类 先查询出其下面的小类id
                strWhere += $" and s.Category in ({categoryId})";
            }

            string sql = $"select psr.*,s.YearCount,s.CostPrice,s.Id as sId,s.Aid as sAid,s.Category as sCategory,s.MyCardId as sMyCardId,s.StorePV,s.StoreVirtualPV,s.Name,c.Phone as StoreOwnerPhone from PlatStoreRelation psr left join PlatStore s on psr.StoreId=s.Id left join PlatMyCard c on s.MyCardId=c.Id  where {strWhere} order by {orderWhere} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, parameters.ToArray()))
            {
                listPlatStoreRelation = new List<PlatStoreRelation>();
                while (dr.Read())
                {
                    PlatStoreRelation platStoreRelation = PlatStoreRelationBLL.SingleModel.GetModel(dr);

                    if (platStoreRelation != null)
                    {
                        if (dr["sCategory"] != DBNull.Value)
                        {
                            PlatStoreCategory OwnerPlatStoreCategory = PlatStoreCategoryBLL.SingleModel.GetModel(Convert.ToInt32(dr["sCategory"]));
                            if (OwnerPlatStoreCategory != null)
                            {
                                platStoreRelation.OwnerSecondCategoryName = OwnerPlatStoreCategory.Name;
                                PlatStoreCategory firstplatStoreCategory = PlatStoreCategoryBLL.SingleModel.GetModel(OwnerPlatStoreCategory.ParentId);
                                if (firstplatStoreCategory != null)
                                {
                                    platStoreRelation.OwnerFirstCategoryName = firstplatStoreCategory.Name;
                                }
                            }
                        }

                        PlatStoreCategory CurPlatStoreCategory = PlatStoreCategoryBLL.SingleModel.GetModel(platStoreRelation.Category);
                        if (CurPlatStoreCategory != null)
                        {
                            platStoreRelation.CurSecondCategoryName = CurPlatStoreCategory.Name;
                            PlatStoreCategory firstplatStoreCategory = PlatStoreCategoryBLL.SingleModel.GetModel(CurPlatStoreCategory.ParentId);
                            if (firstplatStoreCategory != null)
                            {
                                platStoreRelation.CurFirstCategoryName = firstplatStoreCategory.Name;
                                platStoreRelation.FirstCategory = CurPlatStoreCategory.ParentId;
                            }
                        }


                        platStoreRelation.StoreOwnerPhone = dr["StoreOwnerPhone"].ToString();
                        if (dr["sMyCardId"] != DBNull.Value)
                        {
                            platStoreRelation.MyCardId = Convert.ToInt32(dr["sMyCardId"]);
                        }

                        Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModel(platStoreRelation.AgentId);
                        platStoreRelation.FromTypeStr = platStoreRelation.FromType == 0 ? "平台入驻" : (agentinfo != null ? agentinfo.name : "分销代理商");

                        if (dr["sAid"] != DBNull.Value)
                        {
                            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(Convert.ToInt32(dr["sAid"]));
                            if (xcxAppAccountRelation == null)
                            {
                                platStoreRelation.BindAppIdName = "无";
                            }
                            else
                            {
                                OpenAuthorizerConfig openAuthorizerConfig = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxAppAccountRelation.AppId, xcxAppAccountRelation.Id);

                                platStoreRelation.BindAppIdName = openAuthorizerConfig?.nick_name + "id:" + xcxAppAccountRelation.Id;

                            }
                        }

                        platStoreRelation.StoreName = Convert.ToString(dr["Name"]);
                        if (dr["StorePV"] != DBNull.Value)
                        {
                            platStoreRelation.StorePV = Convert.ToInt32(dr["StorePV"]);
                        }
                        if (dr["StoreVirtualPV"] != DBNull.Value)
                        {
                            platStoreRelation.StorePV += Convert.ToInt32(dr["StoreVirtualPV"]);
                        }
                       
                        if (dr["sId"] != DBNull.Value)
                        {
                            platStoreRelation.StoreId = Convert.ToInt32(dr["sId"]);
                        }
                        if (dr["sAid"] != DBNull.Value)
                        {
                            platStoreRelation.StoreAid = Convert.ToInt32(dr["sAid"]);
                        }

                        platStoreRelation.YearCount = Convert.ToInt32(dr["YearCount"]);
                        platStoreRelation.CostPrice = Convert.ToInt32(dr["CostPrice"]);

                        listPlatStoreRelation.Add(platStoreRelation);
                    }
                }

            }


            totalCount = 0;
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"select Count(psr.Id) from PlatStoreRelation psr left join PlatStore s on psr.StoreId=s.Id left join PlatMyCard c on s.MyCardId=c.Id  where {strWhere}", parameters.ToArray());
            if (obj != null)
            {
                totalCount = Convert.ToInt32(obj);
            };



            return listPlatStoreRelation;
        }

        public List<PlatStore> GetListStore(int aid, out int totalCount, int categoryId = 0, string storeName = "", double ws_lat = 0, double ws_lng = 0, int cityCode = 0, int orderType = 0, int pageSize = 10, int pageIndex = 1, int isBigType = 0, string appId = "")
        {
            string orderWhere = " top desc, storepv+StoreVirtualPV DESC";//TODO 上线改为月MONTH//默认表示按照人气降序
            string strWhere = $" psr.aid = {aid} and s.id is not null AND psr.FromType = 0 and psr.state = 1 and s.State=0 and (DATE_ADD(s.addtime, INTERVAL s.YearCount MONTH)-now())>0 and s.MyCardId in({PlatMyCardBLL.SingleModel.GetCardIds(aid,appId)})";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (categoryId > 0)
            {
                string categoryIds = categoryId.ToString();
                if (isBigType > 0)
                {
                    //先找出该大类下的子类Id
                    categoryIds = PlatStoreCategoryBLL.SingleModel.GetChildIds(aid, categoryId);
                }

                //表示筛选某个类别下的店铺
                strWhere += $" and psr.Category in ({categoryIds}) ";
            }

            if (cityCode > 0)
            {
                //表示选择指定区域
                strWhere += $" and s.CityCode={cityCode} ";
            }

            if (!string.IsNullOrEmpty(storeName))
            {
                //表示筛选某个店铺名称关键词
                parameters.Add(new MySqlParameter("@storeName", $"%{storeName}%"));
                strWhere += "  and (s.Name like @storeName or s.Location like @storeName) ";
            }


            if (orderType == 1)
            {
                //按照距离由近到远
                orderWhere = "  distance asc";
            }

            if (orderType == 2)
            {
                //表示按照人气pv降序排列
                orderWhere = "  StorePV+StoreVirtualPV desc";
            }
            string sql = $"SELECT s.*, ROUND(6378.138*2*ASIN(SQRT(POW(SIN(({ws_lat}*PI()/180-lat*PI()/180)/2),2)+COS({ws_lat}*PI()/180)*COS(lat*PI()/180)*POW(SIN(({ws_lng}*PI()/180-lng*PI()/180)/2),2)))*1000) AS distance from platstorerelation  psr  LEFT JOIN platstore s on psr.storeid=s.id where  {strWhere}  ORDER BY {orderWhere} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";
            
            List<PlatStore> list = new List<PlatStore>();
            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, parameters.ToArray()))
            {
                while (dr.Read())
                {
                    PlatStore platStore = base.GetModel(dr);
                    if (platStore.Aid > 0)
                    {
                        //表示店铺绑定了独立小程序 是否有可用优惠券
                        platStore.HaveCoupons = CouponsBLL.SingleModel.GetAbleCountByAppId(platStore.Aid, TicketType.优惠券)>0;

                    }


                    if (DBNull.Value != dr["distance"])
                    {
                        platStore.Distance = (Convert.ToInt32(dr["distance"]) * 0.001) < 1 ? $"{dr["distance"].ToString()}m" : $"{Convert.ToInt32(dr["distance"]) * 0.001}km";
                    }
                    platStore.StorePV += platStore.StoreVirtualPV;




                    list.Add(platStore);
                }
            }

            totalCount = 0;
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT count(psr.Id) from platstorerelation  psr left join agentdistributionrelation ar on psr.agentid = ar.agentid LEFT JOIN platstore s on psr.storeid=s.id where {strWhere}", parameters.ToArray());
            if (obj != null)
            {
                totalCount = Convert.ToInt32(obj);
            };

            return list;
        }

        public List<PlatStore> GetListByCardId(string cardids)
        {
            if (string.IsNullOrEmpty(cardids))
                return new List<PlatStore>();

            return base.GetList($"MyCardId in ({cardids}) and state>=0");
        }

        public List<PlatChildGoods> GetSyncGoods(int aid, out int totalCount, string storeName = "", string goodsName = "", int goodsSync = -1, string categoryName = "", int pageSize = 10, int pageIndex = 1, int isFirstType = 0, int categoryId = 0,string appId="")
        {
            List<PlatChildGoods> list = new List<PlatChildGoods>();
            int count = 0;
            List<int> listStoreAid = new List<int>();
            //从中间表里获取该平台的店铺包括本平台入驻的跟代理商同步过来的
            List<PlatStoreRelation> listPlatStoreRelation = GetListStore(aid, out count, storeName, "", "", categoryName, 1, -1, -1, 1000, 1, "AddTime desc", string.Empty, isFirstType, categoryId,appId);

            foreach (PlatStoreRelation item in listPlatStoreRelation)
            {
                if (!listStoreAid.Contains(item.StoreAid))
                {
                    listStoreAid.Add(item.StoreAid);
                }
            }

            totalCount = 0;
            if (listStoreAid.Count > 0)
            {


                //从中间表里获取该平台的产品包括本平台入驻店铺的产品以及跟代理商同步过来的
                string goodsIds = PlatGoodsRelationBLL.SingleModel.GetGoodsIdsByAids(aid, goodsSync);

                if (!string.IsNullOrEmpty(goodsIds))
                {
                    List<MySqlParameter> parameters = new List<MySqlParameter>();
                    string strWhere = $" Id in({goodsIds}) and Tag=1 and State=1 and Aid in({string.Join(",", listStoreAid)}) ";


                    if (!string.IsNullOrEmpty(goodsName))
                    {
                        parameters.Add(new MySqlParameter("@goodsName", $"%{goodsName}%"));
                        strWhere += " and Name like @goodsName";
                    }
                    list = PlatChildGoodsBLL.SingleModel.GetListByParam(strWhere, parameters.ToArray(), pageSize, pageIndex, "Id,AId,Name,Price,VirtualSalesCount,SalesCount,Stock,Img,Unit", " sort desc, id desc ");
                    if (list != null)
                    {
                        list.ForEach(x =>
                        {
                            PlatGoodsRelation goodsRelation = PlatGoodsRelationBLL.SingleModel.GetPlatGoodsRelation(aid, x.Id);
                            if (goodsRelation != null)
                            {
                                x.Synchronized = goodsRelation.Synchronized;
                            }

                            PlatStoreRelation platStoreRelation = listPlatStoreRelation.FirstOrDefault(y => y.StoreAid == x.AId);
                            if (platStoreRelation != null)
                            {
                                x.StoreName = platStoreRelation.StoreName;
                                x.CurFirstCategoryName = platStoreRelation.CurFirstCategoryName;
                                x.CurSecondCategoryName = platStoreRelation.CurSecondCategoryName;
                            }


                        });

                        totalCount = PlatChildGoodsBLL.SingleModel.GetCount(strWhere, parameters.ToArray());
                    }
                }


            }

            return list;


        }

        /// <summary>
        /// 子模版获取小程序二维码
        /// </summary>
        /// <param name="storeid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string GetStoreCode(int storeid, ref string msg)
        {
            
            PlatStore store = base.GetModel(storeid);
            if (store == null)
            {
                msg = "店铺二维码：找不到店铺数据";
                return "";
            }

            string url = RedisUtil.Get<string>(string.Format(_redis_PlatStoreCodeUrlKey, storeid));
            if (!string.IsNullOrEmpty(url))
            {
                return url;
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(store.Aid);
            if (xcxrelation == null || string.IsNullOrEmpty(xcxrelation.AppId))
            {
                msg = "店铺二维码：店铺没绑定小程序";
                return "";
            }
            //OpenAuthorizerConfig XUserList = openAuthorizerConfigBLL.GetModelByAppids(xcxrelation.AppId);
            //if (XUserList == null)
            //{
            //    msg = "店铺二维码：店铺小程序还未授权";
            //    return "";
            //}

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxTemplate == null)
            {
                msg = "店铺二维码：无效模板";
                return "";
            }

            string token = "";
                if(!XcxApiBLL.SingleModel.GetToken(xcxrelation,ref token))
            {
                msg = token;
                return "";
            }

            //小程序二维码
            
            qrcodeclass result = CommondHelper.GetMiniAppQrcode(token, xcxTemplate.Address);
            if (result != null && result.isok > 0)
            {
                url = result.url;
                if (string.IsNullOrEmpty(url))
                {
                    return "";
                }
                url = url.Replace("http:", "https:");
                RedisUtil.Set<string>(string.Format(_redis_PlatStoreCodeUrlKey, storeid), url, TimeSpan.FromSeconds(5));
                return url;
            }

            msg = "店铺二维码：获取失败";
            return "";
        }

        /// <summary>
        /// 获取平台店铺二维码
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageurl"></param>
        /// <param name="scene"></param>
        /// <returns></returns>
        public string GetStoreCode(int storeid, string pageurl, ref string msg)
        {
            PlatStore store = base.GetModel(storeid);
            if (store == null)
            {
                msg = "店铺二维码：找不到店铺数据";
                return "";
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(store.BindPlatAid);
            if (xcxrelation == null)
            {
                msg = "平台店铺二维码：模板过期";
                return "";
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxTemplate == null)
            {
                msg = "无效模板";
                return "";
            }

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcxrelation, ref token))
            {
                msg = token;
                return "";
            }

            //店铺没有开通小程序
            //平台小程序二维码
            string scen = $"{storeid}";
            qrcodeclass qrcodemodel = CommondHelper.GetMiniAppQrcode(token,pageurl,scen);
            if (qrcodemodel == null || string.IsNullOrEmpty(qrcodemodel.url))
            {
                msg = qrcodemodel != null ? qrcodemodel.msg : "生成名片码失败";
                return "";
            }

            return qrcodemodel.url;
        }

        /// <summary>
        /// 获取可以绑定店铺的名片
        /// </summary>
        /// <param name="bindplataid"></param>
        /// <returns></returns>
        public List<object> GetBindMyCard(int bindplataid=0)
        {
            List<object> listObj = new List<object>();
            List<int> listMyCardId = new List<int>();

            List<PlatStore> listStore = base.GetList($"bindplataid={bindplataid}", 100000,1,"mycardid");
            if (listStore != null && listStore.Count > 0)
            {
                foreach (PlatStore item in listStore)
                {
                    listMyCardId.Add(item.MyCardId);
                }
            }
            else
            {
                listMyCardId.Add(0);
            }
            
            List<PlatMyCard> list = PlatMyCardBLL.SingleModel.GetList($"Id not in({string.Join(",",listMyCardId)}) and aid={bindplataid}",100000,1,"Id,Name");
            listObj.Add(new { Id = 0, Name = "请选择" });
            foreach (PlatMyCard item in list)
            {
                listObj.Add(new {Id=item.Id,Name=item.Name });
            }
            return listObj;
        }
        
        /// <summary>
        /// 输入店铺名称或者手机号模糊查询店铺
        /// </summary>
        /// <param name="name"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        public List<PlatStore> GetListByNameOrPhone(string name,int aid)
        {
            List<PlatStore> list = new List<PlatStore>();
            MySqlParameter[] parms = new MySqlParameter[] {
                new MySqlParameter("@name",$"%{name}%"),
                new MySqlParameter("@phone",$"%{name}%")
            };

            string sql = $"select s.*,c.userid from platstore s left join platmycard c on s.MyCardId=c.id where s.bindplataid={aid} and (s.Name like @name or c.Phone like @phone)";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, parms.ToArray()))
            {
                while (dr.Read())
                {
                    PlatStore model = base.GetModel(dr);
                    if(dr["userid"]!=DBNull.Value)
                    {
                        model.UserId = Convert.ToInt32(dr["userid"]);
                    }
                    list.Add(model);
                }
            }

            return list;
        }
        
        /// <summary>
        /// 获取优选商城商品总数量 没有任何搜索条件
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public int GetSyncGoodsTotalCount(int aid,string appId="")
        {
            List<PlatChildGoods> list = new List<PlatChildGoods>();
            int count = 0;
            List<int> listStoreAid = new List<int>();
            //从中间表里获取该平台的店铺包括本平台入驻的跟代理商同步过来的
            List<PlatStoreRelation> listPlatStoreRelation = GetListStore(aid, out count, string.Empty, "", "", string.Empty, 1, -1, -1, 1000, 1, "AddTime desc", string.Empty, 0, 0,appId);

            foreach (PlatStoreRelation item in listPlatStoreRelation)
            {
                if (!listStoreAid.Contains(item.StoreAid))
                {
                    listStoreAid.Add(item.StoreAid);
                }
            }

            int totalCount = 0;
            if (listStoreAid.Count > 0)
            {
                //从中间表里获取该平台的产品包括本平台入驻店铺的产品以及跟代理商同步过来的
                string goodsIds = PlatGoodsRelationBLL.SingleModel.GetGoodsIdsByAids(aid, -1);

                if (!string.IsNullOrEmpty(goodsIds))
                {

                    string strWhere = $" Id in({goodsIds}) and Tag=1 and State=1 and Aid in({string.Join(",", listStoreAid)}) ";
                    totalCount = PlatChildGoodsBLL.SingleModel.GetCount(strWhere);
                }

            }
            return totalCount;
        }

        /// <summary>
        /// 跟进平台aid获取平台店铺绑定的appid
        /// </summary>
        /// <param name="bindplataid"></param>
        /// <returns></returns>
        public List<PlatStore> GetXcxRelationAppids(int bindplataid)
        {
            string sql = $"select x.appid,p.aid from PlatStore p left join XcxAppAccountRelation x on p.aid = x.id where x.appid is not null and x.appid <>'' and p.BindPlatAid={bindplataid}";
            List<PlatStore> list = new List<PlatStore>();
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    PlatStore model = base.GetModel(dr);
                    model.AppId = dr["appid"].ToString();
                    list.Add(model);
                }
            }
            
            return list;
        }


        public override string BuildUpdateSql(PlatStore model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update  " + TableName + " set ");
            PropertyInfo[] pis = UpdateArrProperty;

            for (int i = 1; i < pis.Length; i++)
            {
                if(pis[i].GetValue(model, null) != null)
                {
                    strSql.Append(string.Format("{0}={1},", pis[i].Name, TypeConvert.GetPropValueField(pis[i].PropertyType.Name, pis[i].GetValue(model, null).ToString())));

                }
            }
            strSql = strSql.Replace(",", " ", strSql.Length - 1, 1);
            if (pis[0].GetValue(model, null) != null)
            {
                strSql.Append(string.Format(" where {0}={1}", PrimaryKey, TypeConvert.GetPropValueField(pis[0].PropertyType.Name, pis[0].GetValue(model, null).ToString())));
            }
            return strSql.ToString();
        }

        /// <summary>
        /// 获取快速支付码
        /// </summary>
        /// <param name="xcxrelation"></param>
        /// <param name="platStore"></param>
        public void GetQuicklyPayQrCode(XcxAppAccountRelation xcxrelation,ref PlatStore platStore)
        {
            if (string.IsNullOrEmpty(platStore.SwitchModel.StorePayQrcode))
            {
                string token = "";
                if (XcxApiBLL.SingleModel.GetToken(xcxrelation, ref token))
                {
                    qrcodeclass qrcodeModel = CommondHelper.GetMiniAppQrcode(token, "pages/home/shop-detail/pay", "", 500);
                    if (!string.IsNullOrEmpty(qrcodeModel.url))
                    {
                        platStore.SwitchModel.StorePayQrcode = qrcodeModel.url;
                        platStore.SwitchConfig = JsonConvert.SerializeObject(platStore.SwitchModel);
                        base.Update(platStore, "SwitchConfig");
                    }
                    else
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), $"生成快速支付码失败:msg:{qrcodeModel.msg}，aid:{xcxrelation.Id}");
                    }
                }
                else
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), $"获取生成快速支付码的token失败:msg:{token}，aid:{xcxrelation.Id}");
                }
            }
        }
    }
}
