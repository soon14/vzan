using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Model;
using Entity.MiniApp.Tools;
using Entity.MiniApp.PlatChild;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utility;
using Core.MiniApp.Common;
using System.IO;
using Core.MiniApp;
using BLL.MiniApp.Tools;
using System.Diagnostics;

namespace BLL.MiniApp.Ent
{
    public class EntGoodsBLL : BaseMySql<EntGoods>
    {
        #region 单例模式
        private static EntGoodsBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntGoodsBLL()
        {

        }

        public static EntGoodsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntGoodsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string Redis_EntGoodsList = "EntGoods_{0}_{1}_{2}";
        private readonly string Redis_EntGoodsList_version = "EntGoods_version_{0}";
        public static readonly string key_new_ent_goods = "temp_p_description_0";




        /// <summary>
        /// 查询指定商品当天的购买数量是否大于当天限制的购买数量
        /// </summary>
        /// <param name="goodsId"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        public bool LimitDayStock(int goodsId, int aid,int buyCount)
        {
           // log4net.LogHelper.WriteInfo(this.GetType(), $"每日库存{goodsId},{aid},{buyCount}");

            EntGoods entGoods = base.GetModel(goodsId);
            if (entGoods == null)
                return true;


            if (entGoods.DayStock <= 0)//表示不限制每日库存
                return false;

            string sql = $@"SELECT SUM(count) from entgoodscart where foodgoodsId={goodsId} and goodsorderId in(

SELECT Id from entgoodsorder where Aid = {aid} and to_days(CreateDate) = to_days(now()) and State<>0

) and Aid = {aid} and to_days(CreateDate) = to_days(now())";
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            int daySaleCount = 0;
            if (obj != DBNull.Value)
            {
                daySaleCount = Convert.ToInt32(obj);
            }

          //  log4net.LogHelper.WriteInfo(this.GetType(), $"今天购买数量{daySaleCount},今日库存{entGoods.DayStock}");
            if (entGoods.DayStock <daySaleCount + buyCount)
            {
                return true;
            }

            return false;


        }


        #region 基本操作

        public string GetPlabelStr(string plabels)
        {
            return SqlMySql.ExecuteScalar(connName, CommandType.Text, $"SELECT group_concat(name order by sort desc) from entgoodlabel where id in ({plabels})").ToString();
        }

        /// <summary>
        /// 获取该分类下的项目数目
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetServiceCountById(int aid, int id)
        {
            return GetCount($"aid ={aid} and state> 0 and FIND_IN_SET('{id}',ptypes)");
        }

        /// <summary>
        /// 获取多门店分店商品
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="search"></param>
        /// <param name="plabels"></param>
        /// <param name="ptype"></param>
        /// <param name="ptag"></param>
        /// <param name="storeid"></param>
        /// <param name="typeid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public DataTable GetMStoreEntGoodsList_Sub(int aid, string search, int plabels, int ptype, int ptag, int storeid, string typeid, int pageindex, int pagesize, int goodtype)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string selSql = GetMStoreEntGoodsListSqlWhere(aid, search, plabels, ptype, ptag, storeid, ref parameters, typeid, pageindex, pagesize, goodtype);
            DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, selSql, parameters.ToArray()).Tables[0];
            return dt;
        }

        public void UpdateSaleCountById(int goodsId, int salesCount)
        {
            string sql = $" update entgoods set salesCount = salesCount + {salesCount} where id = {goodsId} ";
            SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql);
        }

        public EntGoods GetModelById(int id, int state)
        {
            return GetModel($"id={id} and state={state}");
        }

        /// <summary>
        /// 根据礼物id获取礼物信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="id"></param>
        /// <param name="exttypes"></param>
        /// <returns></returns>
        public EntGoods GetGiftInfo(int appId, int id, int exttypes)
        {
            string sqlwhere = $"id={id} and aid ={appId} and state> 0 and exttypes='{exttypes}'";
            return GetModel(sqlwhere);
        }

        /// <summary>
        /// 根据礼物名称获取礼物信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="exttypes"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public EntGoods GetGiftInfoByName(int appId, int exttypes, string name)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string sqlwhere = $"aid={appId} and state> 0 and exttypes='{exttypes}' and name=@name";
            parameters.Add(new MySqlParameter("@name", $"{name}"));
            return GetModel(sqlwhere, parameters.ToArray());
        }

        public List<EntGoods> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<EntGoods>();
            return base.GetList($"id in ({ids})");
        }
        
        /// <summary>
        /// 足浴版获取礼物列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="exttypes"></param>
        /// <returns></returns>
        public List<EntGoods> GetGiftPackages(int appId, int exttypes)
        {
            string sqlwhere = $"aid ={appId} and state> 0 and exttypes='{(int)GoodsType.足浴版送花套餐}'";
            return GetList(sqlwhere, 10, 1, "*", "id asc");
        }

        /// <summary>
        /// 足浴版根据项目名称获取服务项目
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="exttype"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public List<EntGoods> GetServiceByName(int appId, int exttype, string serviceName)
        {
            string serviceSqlwhere = $" aid ={appId} and state> 0 and exttypes='{(int)GoodsType.足浴版服务}' and name like @name";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@name", $"%{serviceName}%"));
            return GetListByParam(serviceSqlwhere, parameters.ToArray());
        }

        /// <summary>
        /// 获取多门店总店商品数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="search"></param>
        /// <param name="plabels"></param>
        /// <param name="ptype"></param>
        /// <param name="ptag"></param>
        /// <param name="storeid"></param>
        /// <param name="typeid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public List<EntGoods> GetMStoreEntGoodsList_Store(int aid, string search, int plabels, int ptype, int ptag, int storeid, string typeid, int pageindex, int pagesize, int goodtype)
        {
            string sortstr = " sort desc,id desc";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string strwhere = GetMStoreEntGoodsListSqlWhere(aid, search, plabels, ptype, ptag, storeid, ref parameters, typeid, pageindex, pagesize, goodtype);
            return GetListByParam(strwhere, parameters.ToArray(), pagesize, pageindex, "*", sortstr);
        }

        #endregion 基本操作

        #region 缓存优化

        public List<EntGoods> GetListByRedis(int appId, string search, int plabels, int ptype, int ptag, int pageIndex, int pageSize, ref int count, string stockNo = null, string ids = null)
        {
            string strwhere = $"aid={appId} and state=1 and goodtype={(int)EntGoodsType.普通产品}";
            bool openredis = true;

            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrWhiteSpace(ids))
            {
                strwhere += $" and id in({ids}) ";
                //  parameters.Add(new MySqlParameter("@ids", ids));
                openredis = false;
            }
            if (search.Trim() != "")
            {
                strwhere += $" and name like @name ";
                parameters.Add(new MySqlParameter("@name", $"%{search}%"));
                openredis = false;
            }
            if (plabels > 0)
            {
                strwhere += $" and  FIND_IN_SET(@plabels,plabels)";
                parameters.Add(new MySqlParameter("@plabels", plabels));
                openredis = false;
            }
            if (ptype > 0)
            {
                strwhere += $" and FIND_IN_SET (@ptype,ptypes) ";
                parameters.Add(new MySqlParameter("ptype", ptype));
                openredis = false;
            }
            if (ptag > -1)
            {
                strwhere += $" and tag={ptag} ";
                openredis = false;
            }

            if (!string.IsNullOrWhiteSpace(stockNo))
            {
                strwhere += $" and StockNo='{stockNo}' ";
                openredis = false;
            }

            string key = string.Format(Redis_EntGoodsList, appId, pageSize, pageIndex);
            string version_key = string.Format(Redis_EntGoodsList_version, appId);

            int version = RedisUtil.GetVersion(version_key);
            RedisModel<EntGoods> list = RedisUtil.Get<RedisModel<EntGoods>>(key);
            // list = null;
            //RedisModel<EntGoods> list = new RedisModel<EntGoods>();
            if (!openredis || list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || version != list.DataVersion)
            {
                list = new RedisModel<EntGoods>();

                #region 重新查询数据

                List<EntGoods> DataList = GetListByParam(strwhere, parameters.ToArray(), pageSize, pageIndex, "*", "sort desc, id desc ");
                count = GetCount(strwhere, parameters.ToArray());
                DataList.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.ptypes))
                    {
                        string sql = $"SELECT GROUP_CONCAT(`name`) from entgoodtype where FIND_IN_SET(id,@ptypes)";
                        x.ptypestr = DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                            CommandType.Text, sql,
                            new MySqlParameter[] { new MySqlParameter("@ptypes", x.ptypes) }).ToString();
                    }

                    if (!string.IsNullOrEmpty(x.plabels))
                    {
                        string sql = $"SELECT group_concat(name) from entgoodlabel where FIND_IN_SET(id,@plabels)";
                        x.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(),
                            CommandType.Text, sql,
                            new MySqlParameter[] { new MySqlParameter("@plabels", x.plabels) }).ToString();
                    }
                });

                #endregion 重新查询数据

                #region 缓存

                list.DataList = DataList;
                list.Count = count;
                list.DataVersion = version;

                if (openredis) //只缓存默认条件的数据,若有条件则不缓存
                {
                    RedisUtil.Set<RedisModel<EntGoods>>(key, list, TimeSpan.FromHours(12));
                }

                #endregion 缓存
            }
            else
            {
                count = list.Count;
            }

            return list.DataList;
        }

        /// <summary>
        /// 清除行业类型的列表缓存
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveEntGoodListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_EntGoodsList_version, appid));
            }
        }

        #endregion 缓存优化

        #region 逻辑代码

        /// <summary>
        /// 查询当前商品库存
        /// </summary>
        /// <param name="goodsid"></param>
        /// <param name="attrSpacStr"></param>
        /// <returns></returns>
        public int GetGoodQty(int goodsid, string attrSpacStr = "")
        {
            int goodQty = 0;

            EntGoods good = GetModel(goodsid);
            if (string.IsNullOrWhiteSpace(attrSpacStr))
            {
                goodQty = good.stock;
            }
            else
            {
                EntGoodsAttrDetail goodBySpacStr = good.GASDetailList.Where(x => x.id.Equals(attrSpacStr))?.First();
                if (goodBySpacStr != null)
                {
                    goodQty = goodBySpacStr.stock;
                }
            }
            return goodQty;
        }

        /// <summary>
        /// 查询当前商品库存
        /// </summary>
        /// <param name="goodsid"></param>
        /// <param name="attrSpacStr"></param>
        /// <returns></returns>
        public int GetGoodQtyByModel(EntGoods goods, string attrSpacStr = "")
        {
            int goodQty = 0;

            if (string.IsNullOrWhiteSpace(attrSpacStr))
            {
                goodQty = goods.stock;
            }
            else
            {
                List<EntGoodsAttrDetail> goodList = goods.GASDetailList.Where(x => x.id.Equals(attrSpacStr)).ToList();
                if (goodList != null && goodList.Any())
                {
                    EntGoodsAttrDetail goodBySpacStr = goodList.First();
                    if (goodBySpacStr != null)
                    {
                        goodQty = goodBySpacStr.stock;
                    }
                }
            }
            return goodQty;
        }

        /// <summary>
        /// 同步门店产品和总店产品并更新门店产品
        /// </summary>
        /// <param name="entGood"></param>
        /// <param name="subGood"></param>
        /// <returns></returns>
        public string GetSyncSql(EntGoods entGoodOld, EntGoods entGood, SubStoreEntGoods subGood)
        {
            //如果编辑门店产品的时候主店产品发生了变化
            if (entGood.specificationdetail != subGood.SubSpecificationdetail)
            {
                List<EntGoodsAttrDetail> newList = new List<EntGoodsAttrDetail>();
                //以总店产品信息为准
                entGoodOld.GASDetailList.ForEach(p =>
                {
                    EntGoodsAttrDetail commonAttr = entGood.GASDetailList.Find(subp => p.id == subp.id);
                    if (commonAttr != null)
                    {
                        //分店只能修改库存
                        p.stock = commonAttr.stock;
                        newList.Add(p);
                    }
                });
                subGood.SubSpecificationdetail = JsonConvert.SerializeObject(newList);
                subGood.SubStock = newList.Sum(p => p.stock);
            }
            else
            {
                subGood.SubStock = entGood.stock;
            }
            subGood.SubUpdateTime = DateTime.Now;
            subGood.SubSort = entGood.sort;

            return SubStoreEntGoodsBLL.SingleModel.BuildUpdateSql(subGood);
        }

        /// <summary>
        /// 修改总店产品的时候，同步所有门店产品
        /// </summary>
        /// <param name="entGood"></param>
        /// <param name="subGood"></param>
        /// <returns></returns>
        public string GetSyncSql(EntGoods entGood, SubStoreEntGoods subGood)
        {
            List<EntGoodsAttrDetail> newList = new List<EntGoodsAttrDetail>();
            //以总店产品信息为准
            entGood.GASDetailList.ForEach(p =>
            {
                EntGoodsAttrDetail commonAttr = subGood.GASDetailList.Find(subp => p.id == subp.id);
                if (commonAttr != null)
                {
                    //只保留分店的库存
                    p.stock = commonAttr.stock;
                    newList.Add(p);
                }
            });
            subGood.SubSpecificationdetail = JsonConvert.SerializeObject(newList);
            subGood.SubStock = newList.Sum(p => p.stock);

            subGood.SubUpdateTime = DateTime.Now;

            return SubStoreEntGoodsBLL.SingleModel.BuildUpdateSql(subGood);
        }

        /// <summary>
        /// 多门店接口获取总店或分店商品数据的条件语句
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="search"></param>
        /// <param name="plabels"></param>
        /// <param name="ptype"></param>
        /// <param name="ptag"></param>
        /// <param name="storeid"></param>
        /// <param name="parameters"></param>
        /// <param name="typeid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        private string GetMStoreEntGoodsListSqlWhere(int aid, string search, int plabels, int ptype, int ptag, int storeid, ref List<MySqlParameter> parameters, string typeid = "", int pageindex = 1, int pagesize = 10, int goodtype = (int)EntGoodsType.普通产品)
        {
            string strwhere = string.Empty;

            if (storeid > 0)
            {
                strwhere = $"aid={aid} and substate=1 and SubTag=1 and state=1 and StoreId={storeid} and goodtype={goodtype}";
            }
            else
            {
                strwhere = $" aid={aid} and state=1 and tag = 1  and goodtype={goodtype}";
            }

            if (!string.IsNullOrEmpty(typeid))
            {
                List<string> typeidSplit = typeid.SplitStr(",");
                if (typeidSplit.Count > 0)
                {
                    typeidSplit = typeidSplit.Select(p => p = "FIND_IN_SET('" + p + "',ptypes)").ToList();
                    strwhere += $" and (" + string.Join(" or ", typeidSplit) + ")";
                }
            }

            if (search.Trim() != "")
            {
                strwhere += $" and name like @name ";
                parameters.Add(new MySqlParameter("name", $"%{search}%"));
            }
            if (plabels > 0)
            {
                strwhere += $" and  FIND_IN_SET(@plabels,plabels)";
                parameters.Add(new MySqlParameter("@plabels", plabels));
            }
            if (ptype > 0)
            {
                strwhere += $" and FIND_IN_SET (@ptype,ptypes) ";
                parameters.Add(new MySqlParameter("ptype", ptype));
            }
            if (ptag > -1)
            {
                strwhere += $" and subtag={ptag} ";
            }

            string selSql = string.Empty;

            if (storeid > 0)
            {
                selSql = $"select * from substoregoodsview where {strwhere} limit {(pageindex - 1) * pagesize},{pagesize}";
                return selSql;
            }
            else
            {
                return strwhere;
            }
        }

        /// <summary>
        /// 足浴版根据aid，id获取服务项目
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public EntGoods GetServiceById(int aid, int id, int tag = 0)
        {
            if (tag > 0)
            {
                return GetModel($"aid={aid} and id={id} and state>0 and tag>0  and exttypes='{(int)GoodsType.足浴版服务}'");
            }
            else
            {
                return GetModel($"aid={aid} and id={id} and state>0  and exttypes='{(int)GoodsType.足浴版服务}'");
            }
        }

        /// <summary>
        /// 足浴版根据条件获取服务项目列表
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="type"></param>
        /// <param name="ptypes"></param>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<EntGoods> GetServiceList(int aId, int exttype, int ptypes, string name, int tag, int pageSize, int pageIndex, out int recordCount)
        {
            string sqlwhere = $"aid ={aId} and state> 0 and exttypes='{exttype}'";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (ptypes > 0)
            {
                sqlwhere += $" and FIND_IN_SET('{ptypes}',ptypes)";
            }
            if (!string.IsNullOrEmpty(name))
            {
                sqlwhere += " and name like @name";
                parameters.Add(new MySqlParameter("@name", $"%{name}%"));
            }
            if (tag > -1)
            {
                sqlwhere += $" and tag={tag}";
            }
            recordCount = GetCount(sqlwhere, parameters.ToArray());
            return GetListByParam(sqlwhere, parameters.ToArray(), pageSize, pageIndex, "*", "id desc");
        }

        /// <summary>
        /// 足浴版获取服务项目
        /// </summary>
        /// <param name="aId"></param>
        /// <returns></returns>
        public List<EntGoods> GetServicList(int aId)
        {
            if (aId <= 0)
            {
                return null;
            }
            return GetList($"aid ={aId} and state> 0 and tag> 0");
        }

        /// <summary>
        /// 足浴版根据项目Id获取服务列表
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public List<EntGoods> GetServerListByIds(int aId, string Ids)
        {
            try
            {
                if (string.IsNullOrEmpty(Ids))
                {
                    return null;
                }
                return GetList($"id in ({Ids}) and state>0 and tag>0 and aid={aId}");
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return null;
            }
        }

        /// <summary>
        /// 获取商品类型和标签
        /// </summary>
        /// <param name="goodList"></param>
        /// <param name="levelid"></param>
        /// <returns></returns>
        public List<EntGoods> GetTypeAndLabelList(List<EntGoods> goodList, int levelid)
        {
            if (goodList == null || goodList.Count <= 0)
                return goodList;

            VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");//获取会员等级信息
            goodList.ForEach(x =>
            {
                VipLevelBLL.SingleModel.CalculateVipGoodsPrice(x, level);
                if (!string.IsNullOrEmpty(x.ptypes))
                {
                    string sql = $"SELECT GROUP_CONCAT(`name`) from entgoodtype where FIND_IN_SET(id,@ptypes)";
                    x.ptypestr = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                        CommandType.Text, sql,
                        new MySqlParameter[] { new MySqlParameter("@ptypes", x.ptypes) }).ToString();
                }

                if (!string.IsNullOrEmpty(x.plabels))
                {
                    string sql = $"SELECT group_concat(name) from entgoodlabel where FIND_IN_SET(id,@plabels)";
                    x.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(),
                        CommandType.Text, sql,
                        new MySqlParameter[] { new MySqlParameter("@plabels", x.plabels) }).ToString();
                    x.plabelstr_array = x.plabelstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            });

            return goodList;
        }

        /// <summary>
        /// 足浴
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<EntGoods> GetFootbathGoodsList(int aId, int orderByPrice, int count, int type, int pageSize, int pageIndex)
        {
            string sql = $"select *,(stock+salesCount) as sum from entgoods where aid ={aId} and state> 0 and tag>0  and exttypes='{(int)GoodsType.足浴版服务}'";
            string orderstr = " order by 1=1,";
            switch (orderByPrice)
            {
                case 1:
                    orderstr += $" price asc,";
                    break;

                case 2:
                    orderstr += $" price desc,";
                    break;
            }
            switch (count)
            {
                case 1:
                    orderstr += $" sum asc,";
                    break;

                case 2:
                    orderstr += $" sum desc,";
                    break;
            }
            if (type > 0)
            {
                sql += $" and FIND_IN_SET('{type}',ptypes)";
            }
            sql += $"{orderstr.TrimEnd(',')} limit {(pageIndex - 1) * pageSize},{pageSize}";
            List<EntGoods> list = new List<EntGoods>();
            DataSet ds = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (ds.Tables.Count <= 0)
                return list;
            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows.Count <= 0)
                return list;
            EntGoods model = null;
            foreach (DataRow row in dt.Rows)
            {
                model = new EntGoods();
                model.id = Convert.ToInt32(row["id"]);
                model.aid = Convert.ToInt32(row["aid"]);
                model.salesCount = Convert.ToInt32(row["sum"]);
                model.name = row["name"].ToString();
                model.specificationkeys = row["specificationkeys"].ToString();
                model.img = row["img"].ToString();
                model.ServiceTime = Convert.ToInt32(row["ServiceTime"]);
                model.description = row["description"].ToString(); //Convert.ToDateTime(row["birthday"]);
                model.price = (float)row["price"];
                model.salesCount = Convert.ToInt32(row["salesCount"]);
                model.stock = Convert.ToInt32(row["stock"]);
                list.Add(model);
            }
            return list;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="goodtype"></param>
        /// <param name="search"></param>
        /// <param name="typeid"></param>
        /// <param name="exttypes"></param>
        /// <param name="pricesort"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <param name="isFirstType">是否是一级类别,0表示是一级类别,-1表示不是</param>
        /// <returns></returns>

        public List<EntGoods> GetListGoods(int aid, int goodtype, string search, string typeid, string exttypes, string pricesort, int pagesize, int pageindex, int isFirstType = -1, string saleCountSort = "")
        {
            string wherestr = $" aid={aid} and state=1 and tag = 1 and goodtype={goodtype}";
            string sortstr = " sort desc,id desc ";//id asc
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string entGoodTypeIds = string.Empty;

            if (!string.IsNullOrEmpty(search))
            {
                //将该关键词当做类别进行模糊匹配 查询类别id
                entGoodTypeIds = EntGoodTypeBLL.SingleModel.GetEntGoodTypeIds(aid, 100, 1, search);
                if (!string.IsNullOrEmpty(entGoodTypeIds))
                {
                    wherestr += $" and( (name like @name) or (ptypes like'%{entGoodTypeIds}%'))";//产品与小类关联
                    parameters.Add(new MySqlParameter("@name", $"%{search}%"));
                }
                else
                {
                    wherestr += $" and name like @name";
                    parameters.Add(new MySqlParameter("@name", $"%{search}%"));
                }
            }

            if (!string.IsNullOrEmpty(typeid))
            {
                //判断是否是大类,如果是大类先转换为下面的小类

                if (isFirstType == 0)
                {
                    List<string> listids = new List<string>();

                    List<EntGoodType> listGoodType = EntGoodTypeBLL.SingleModel.GetList($"aid={aid} and state=1 and parentid in({typeid})");
                    foreach (EntGoodType item in listGoodType)
                    {
                        listids.Add(item.id.ToString());
                    }
                    if (listids.Count <= 0)
                    {
                        typeid = "0";
                    }
                    else
                    {
                        typeid = string.Join(",", listids);
                    }
                }

                List<string> typeidSplit = typeid.SplitStr(",");
                if (typeidSplit.Count > 0)
                {
                    typeidSplit = typeidSplit.Select(p => p = "FIND_IN_SET('" + p + "',ptypes)").ToList();
                    wherestr += $" and (" + string.Join(" or ", typeidSplit) + ")";
                }
            }

            if (!string.IsNullOrEmpty(exttypes))
            {
                string[] exttypes_list = exttypes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < exttypes_list.Length; i++)
                {
                    string parameterName = "@exttypeitem" + i;
                    wherestr += $" and FIND_IN_SET({parameterName},exttypes)>0 ";
                    parameters.Add(new MySqlParameter(parameterName, exttypes_list[i]));
                }
            }
            if (!string.IsNullOrEmpty(pricesort) && (pricesort == "asc" || pricesort == "desc"))
            {
                sortstr = " price " + pricesort;
            }

            if (!string.IsNullOrEmpty(saleCountSort) && (saleCountSort == "asc" || saleCountSort == "desc"))
            {
                sortstr = " virtualSalesCount+salesCount " + saleCountSort;
            }

            List<EntGoods> goodslist = base.GetListByParam(wherestr, parameters.ToArray(), pagesize, pageindex, "*", sortstr);
            return goodslist;
        }

        /// <summary>
        /// 获取热门推荐（实际销量+虚拟销量）只取前6条 做了图片裁剪 350*350
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public List<EntGoods> GetHotGoods(int aid, VipLevel level)
        {
            List<EntGoods> goodsList = new List<EntGoods>();
            if (aid <= 0)
            {
                return goodsList;
            }
            string sqlwhere = $"aid={aid} and state=1 and tag=1 and goodtype={(int)EntGoodsType.普通产品}";
            goodsList = GetList(sqlwhere, 6, 1, "*", "virtualSalesCount+salesCount desc");
            if (goodsList != null && goodsList.Count > 0)
            {
                goodsList.ForEach(goods =>
                {
                    goods.img_fmt = ImgHelper.ResizeImg(goods.img, 350, 350);
                    goods = GetDiscountPrice(level, goods);
                });
            }
            return goodsList;
        }

        public EntGoods GetDiscountPrice(VipLevel level, EntGoods goodModel)
        {
            if (level == null)
            {
                goodModel.discountPrice = float.Parse(goodModel.price.ToString("0.00"));
                //拼团折后价
                goodModel.discountGroupPrice = float.Parse((goodModel.EntGroups.GroupPrice * 0.01).ToString("0.00"));
                //多规格处理
                if (goodModel.GASDetailList != null && goodModel.GASDetailList.Count > 0)
                {
                    List<EntGoodsAttrDetail> detaillist = goodModel.GASDetailList.ToList();
                    detaillist.ForEach(g => g.discountPrice = float.Parse(g.price.ToString("0.00")));
                    //拼团折后价
                    detaillist.ForEach(g => g.discountGroupPrice = g.groupPrice);
                    goodModel.specificationdetail = JsonConvert.SerializeObject(detaillist);
                }
            }
            else
            {
                if (level.type == 1)
                {
                    goodModel.discount = level.discount;
                    goodModel.discountPrice = float.Parse(((goodModel.price * (level.discount * 0.01)) < 0.01 ? 0.01 : goodModel.price * (goodModel.discount * 0.01)).ToString("0.00"));
                    //拼团折后价
                    goodModel.discountGroupPrice = (float)((goodModel.EntGroups.GroupPrice * 0.01 * (level.discount * 0.01)) < 0.01 ? 0.01 : goodModel.EntGroups.GroupPrice * 0.01 * (goodModel.discount * 0.01));
                    //多规格处理
                    if (goodModel.GASDetailList != null && goodModel.GASDetailList.Count > 0)
                    {
                        List<EntGoodsAttrDetail> detaillist = goodModel.GASDetailList.ToList();
                        detaillist.ForEach(g =>
                        {
                            g.discount = level.discount;
                            g.discountPrice = float.Parse(((g.price * (level.discount * 0.01)) < 0.01 ? 0.01 : g.price * (level.discount * 0.01)).ToString("0.00"));
                            //拼团折后价
                            g.discountGroupPrice = float.Parse(((g.groupPrice * (level.discount * 0.01)) < 0.01 ? 0.01 : g.groupPrice * (level.discount * 0.01)).ToString("0.00"));
                        });
                        goodModel.specificationdetail = JsonConvert.SerializeObject(detaillist);
                    }
                }
                else
                {
                    if (level.type == 2 && !string.IsNullOrEmpty(level.gids))
                    {
                        List<string> idlist = level.gids.Split(',').ToList();
                        if (idlist == null || idlist.Count <= 0)
                        {
                            goodModel.discountPrice = float.Parse(goodModel.price.ToString("0.00"));
                            //拼团折后价
                            goodModel.discountGroupPrice = float.Parse((goodModel.EntGroups.GroupPrice * 0.01).ToString("0.00"));
                        }
                        else
                        {
                            if (idlist.Contains(goodModel.id.ToString()))
                            {
                                goodModel.discount = level.discount;
                                goodModel.discountPrice = float.Parse(((goodModel.price * (level.discount * 0.01)) < 0.01 ? 0.01 : goodModel.price * (goodModel.discount * 0.01)).ToString("0.00"));
                                //拼团折后价
                                goodModel.discountGroupPrice = float.Parse(((goodModel.EntGroups.GroupPrice * 0.01 * (level.discount * 0.01)) < 0.01 ? 0.01 : goodModel.EntGroups.GroupPrice * 0.01 * (goodModel.discount * 0.01)).ToString("0.00"));
                                //多规格处理
                                if (goodModel.GASDetailList != null && goodModel.GASDetailList.Count > 0)
                                {
                                    List<EntGoodsAttrDetail> detaillist = goodModel.GASDetailList.ToList();
                                    detaillist.ForEach(g =>
                                    {
                                        g.discount = level.discount;
                                        g.discountPrice = float.Parse(((g.price * (level.discount * 0.01)) < 0.01 ? 0.01 : g.price * (level.discount * 0.01)).ToString("0.00"));
                                        //拼团折后价
                                        g.discountGroupPrice = float.Parse(((g.groupPrice * (level.discount * 0.01)) < 0.01 ? 0.01 : g.groupPrice * (level.discount * 0.01)).ToString("0.00"));
                                    });
                                    goodModel.specificationdetail = JsonConvert.SerializeObject(detaillist);
                                }
                            }
                            else
                            {
                                goodModel.discountPrice = float.Parse(goodModel.price.ToString("0.00"));
                                //拼团折后价
                                goodModel.discountGroupPrice = float.Parse((goodModel.EntGroups.GroupPrice * 0.01).ToString("0.00"));
                                //多规格处理
                                if (goodModel.GASDetailList != null && goodModel.GASDetailList.Count > 0)
                                {
                                    List<EntGoodsAttrDetail> detaillist = goodModel.GASDetailList.ToList();
                                    detaillist.ForEach(g => g.discountPrice = float.Parse(g.price.ToString("0.00")));
                                    //拼团折后价
                                    detaillist.ForEach(g => g.discountGroupPrice = float.Parse(g.groupPrice.ToString("0.00")));
                                    goodModel.specificationdetail = JsonConvert.SerializeObject(detaillist);
                                }
                            }
                        }
                    }
                    else
                    {
                        goodModel.discountPrice = float.Parse(goodModel.price.ToString("0.00"));
                        //拼团折后价
                        goodModel.discountGroupPrice = float.Parse((goodModel.EntGroups.GroupPrice * 0.01).ToString("0.00"));
                        //多规格处理
                        if (goodModel.GASDetailList != null && goodModel.GASDetailList.Count > 0)
                        {
                            List<EntGoodsAttrDetail> detaillist = goodModel.GASDetailList.ToList();
                            detaillist.ForEach(g => g.discountPrice = float.Parse(g.price.ToString("0.00")));
                            //拼团折后价
                            detaillist.ForEach(g => g.discountGroupPrice = float.Parse(g.groupPrice.ToString("0.00")));
                            goodModel.specificationdetail = JsonConvert.SerializeObject(detaillist);
                        }
                    }
                }
            }
            return goodModel;
        }

        public List<EntGoods> GetListByTemplateId(int appId, int templateId, int pageIndex = 1, int pageSize = 10)
        {
            //string whereSql = BuildWhereSql(appId: appId, templateId: templateId, type: EntGoodsType.普通产品);
            //if (WebSiteConfig.Environment == "dev")
            //{
            string whereSql = BuildWhereSql(appId: appId, templateId: templateId);
            //}
            return GetList(whereSql, pageSize, pageIndex, "ID,Name", "ID DESC");
        }

        public List<EntGoods> GetListByFlashDealId(int appId, int flashDealId, int pageIndex = 1, int pageSize = 10)
        {
            string entGoodIds = string.Join(",", FlashDealItemBLL.SingleModel.GetByDealId(flashDealId).Select(item => item.SourceId));
            string whereSql = BuildWhereSql(appId: appId, ids: entGoodIds, type: EntGoodsType.普通产品);

            return GetList(whereSql, pageSize, pageIndex, "ID,Name,Price,specificationdetail,pickspecification", "ID DESC");
        }

        public int GetCountByTemplateId(int appId, int templateId)
        {
            string whereSql = BuildWhereSql(appId: appId, templateId: templateId, type: EntGoodsType.普通产品);
            return GetCount(whereSql);
        }

        public bool UpdateTemplateByIds(int appId, int templateId, string productIds)
        {
            TransactionModel tran = new TransactionModel();

            //更新商品的ID
            List<int> updateProductId = productIds.ConvertToIntList(',');
            //当前已绑定模板的商品
            List<EntGoods> currProducts = GetListByTemplateId(appId: appId, templateId: templateId, pageIndex: 1, pageSize: 9999);

            //新增绑定商品ID
            List<int> newProductId = new List<int>();
            if (!string.IsNullOrWhiteSpace(productIds))
            {
                newProductId = updateProductId.FindAll(newId => !currProducts.Exists(product => product.id == newId));
            }
            //新增绑定商品列表
            List<EntGoods> newProducts = new List<EntGoods>();
            if (newProductId.Count > 0)
            {
                newProducts = GetListByIds(string.Join(",", newProductId)).FindAll(product => product.aid == appId);
            }

            //取消绑定商品列表
            List<EntGoods> updateProducts = currProducts.Where(product => !updateProductId.Contains(product.id)).ToList();

            //插入事务
            updateProducts?.ForEach((product) =>
            {
                product.TemplateId = 0;
                tran.Add(BuildUpdateSql(product, "TemplateId"));
            });
            newProducts?.ForEach((product) =>
            {
                product.TemplateId = templateId;
                tran.Add(BuildUpdateSql(product, "TemplateId"));
            });
            if (tran.sqlArray.Length == 0)
            {
                //无数据更改，跳出
                return true;
            }
            //返回事务执行结果
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        public EntGoods GetByStockNo(int appId, string stockNo)
        {
            string whereSql = BuildWhereSql(appId: appId, stockNo: stockNo);
            return GetModel(whereSql);
        }

        /// <summary>
        /// 序列化规格：秒杀设置价格使用
        /// </summary>
        /// <param name="good"></param>
        /// <returns></returns>
        public object FormatSpecFlashDeal(EntGoods good)
        {
            List<EntGoodsAttrDetail> specsConf = good.GASDetailList;
            List<pickspecification> specs = !string.IsNullOrWhiteSpace(good.pickspecification) ? JsonConvert.DeserializeObject<List<pickspecification>>(good.pickspecification) : null;
            if (specsConf.Count == 0)
            {
                return null;
            }
            return specs?.Select(spec => spec.items).Cartesian().Select(combo =>
            {
                string confId = string.Join("_", combo.Select(item => item.id));
                EntGoodsAttrDetail conf = specsConf.FirstOrDefault(item => item.id.Trim() == confId);
                return conf != null ? new FlashItemSpec
                {
                    Id = conf.id,//属性ID_属性ID_属性ID
                    Name = string.Join("", combo.Select(item => $"[{item.name}]")),//[属性1][属性2][属性3]
                    OrigPrice = (int)(conf.price * 100),//原价
                    DealPrice = (int)(conf.price * 100),//秒杀价（初始）
                } : null;
            }).Where(item => item != null);
        }

        public string BuildWhereSql(int appId = 0, string ids = null, int templateId = 0, string stockNo = null, EntGoodsType? type = null)
        {
            string whereSql = "state > 0";
            if (appId > 0)
            {
                whereSql = $"{whereSql} AND aid = {appId}";
            }
            if (type.HasValue)
            {
                whereSql = $"{whereSql} AND goodtype = {(int)type.Value}";
            }
            if (templateId > 0)
            {
                whereSql = $"{whereSql} AND templateId = {templateId}";
            }
            if (!string.IsNullOrWhiteSpace(stockNo))
            {
                whereSql = $"{whereSql} AND stockNo = '{stockNo}'";
            }
            if (!string.IsNullOrWhiteSpace(ids))
            {
                whereSql = $"({whereSql}) OR Id IN ({ids})";
            }
            return whereSql;
        }

        #endregion 逻辑代码

        /// <summary>
        /// 产品导入
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="aid"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool ImportProductsToDB(Stream stream, int aid, out string result, string fileType)
        {

            result = "导入异常,";

            try
            {


                DataTable dt = fileType == ".xlsx" ? ExcelHelper<EntGoods>.Excel2007ToDataTable(stream) : ExcelHelper<EntGoods>.Excel2003ToDataTable(stream);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows.Count > 300)
                    {
                        result += $"一次最多只能导入300条数据";
                        return false;
                    }

                    TransactionModel transactionModel = new TransactionModel();

                    int i = 0;

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    foreach (DataRow item in dt.Rows)
                    {

                        List<string> listTypeId = new List<string>();//分类Id集合 产品小类对应后台
                        List<string> listType = new List<string>();//分类名称集合 产品小类对应后台
                        string typeId = string.Empty;

                        List<string> listLabelId = new List<string>();//标签Id集合 对应后台
                        List<string> listLabel = new List<string>();//标签名称集合 对应后台
                        string labelId = string.Empty;


                        List<string> listSKUName = new List<string>();//规格名称组合集合
                        List<string> listSKUVule = new List<string>();//规格值组合集合
                        List<string> SKUItemTotal = new List<string>();//单个规格
                        List<string> SKUItemVule = new List<string>();//单个规格值集合
                        string skuId = string.Empty;
                        int parentSKUId = 0;



                        List<string> listSKUDetail = new List<string>();//具体规格值集合
                        List<string> SKUDetailItemTotal = new List<string>();//单个规格名称
                        List<string> SKUDetailItemVule = new List<string>();//单个规格值集合
                        List<string> detailItem = new List<string>();
                        List<string> detailItemName = new List<string>();
                        List<string> detailItemVaule = new List<string>();
                        List<EntGoodsAttrDetail> listEntGoodsAttrDetail = new List<EntGoodsAttrDetail>();
                        List<pickspecification> listPickspecificationParnet = new List<pickspecification>();

                        string detailItemVauleId = string.Empty;


                        List<string> listExtTypes = new List<string>();//产品参数
                        List<string> listItemExtTypes = new List<string>();
                        List<string> listExtTypesDB = new List<string>();
                        string extTypeId = string.Empty;

                        int deliveryTemplateId = 0;//运费模板ID


                        i++;
                        EntGoods entGoods = new EntGoods();
                        if (item["产品名称"] == null || string.IsNullOrEmpty(item["产品名称"].ToString()) || string.IsNullOrWhiteSpace(item["产品名称"].ToString()))
                        {
                            result += $"第{i}个产品名称不能为空";
                            return false;
                        }
                        else
                        {
                            entGoods.name = item["产品名称"].ToString();
                        }

                        if (item["产品图片"] == null || string.IsNullOrEmpty(item["产品图片"].ToString()) || string.IsNullOrWhiteSpace(item["产品图片"].ToString()))
                        {
                            result += $"第{i}个产品图片不能为空";
                            return false;
                        }
                        else
                        {
                            entGoods.img = item["产品图片"].ToString();
                        }

                        if (item["产品分类"] == null || string.IsNullOrEmpty(item["产品分类"].ToString()) || string.IsNullOrWhiteSpace(item["产品分类"].ToString()))
                        {
                            result += $"第{i}个产品分类不能为空";
                            return false;
                        }
                        else
                        {
                            listType = item["产品分类"].ToString().Split(',').ToList();
                            foreach (string typeItem in listType)
                            {
                                typeId = EntGoodTypeBLL.SingleModel.GetEntGoodTypeId(aid, typeItem);
                                if (string.IsNullOrEmpty(typeId))
                                {
                                    result += $"第{i}个产品分类{typeItem}在后台不存在";
                                    return false;
                                }

                                listTypeId.Add(typeId);
                            }
                            entGoods.ptypes = string.Join(",", listTypeId);

                        }


                        if (item["产品标签"] != null && !string.IsNullOrEmpty(item["产品标签"].ToString()) && !string.IsNullOrWhiteSpace(item["产品标签"].ToString()))
                        {
                            listLabel = item["产品标签"].ToString().Split(',').ToList();
                            foreach (string labelItem in listLabel)
                            {
                                labelId = EntGoodLabelBLL.SingleModel.GetEntGoodLabelId(aid, labelItem);
                                if (string.IsNullOrEmpty(labelId))
                                {
                                    result += $"第{i}个产品标签{labelItem}在后台不存在";
                                    return false;
                                }

                                listLabelId.Add(labelId);
                            }
                            entGoods.plabels = string.Join(",", listLabelId);
                        }
                        if (item["货号"] != null && !string.IsNullOrEmpty(item["货号"].ToString()) && !string.IsNullOrWhiteSpace(item["货号"].ToString()))
                        {
                            entGoods.StockNo = item["货号"].ToString();
                        }

                        if (item["限制库存"] != null && !string.IsNullOrEmpty(item["限制库存"].ToString()) && !string.IsNullOrWhiteSpace(item["限制库存"].ToString()))
                        {
                            entGoods.stockLimit = Convert.ToInt32(item["限制库存"]) > 0;
                        }

                        if (item["产品规格"] != null && !string.IsNullOrEmpty(item["产品规格"].ToString()) && !string.IsNullOrWhiteSpace(item["产品规格"].ToString()))
                        {
                            //颜色:红色,蓝色,黄色;尺码:S,M,L
                            listSKUName = item["产品规格"].ToString().Split(';').ToList();
                            if (listSKUName == null || listSKUName.Count <= 0)
                            {
                                result += $"第{i}个产品规格填写格式不对";
                                return false;
                            }
                            int skuVauleCount = 1;
                            List<string> listSpecificationVule = new List<string>();//规格值名称集合
                            List<string> listSpecificationkeys = new List<string>();
                            List<string> listSpecification = new List<string>();

                            foreach (string skuItem in listSKUName)
                            {
                                #region 检查格式
                                SKUItemTotal = skuItem.Split(':').ToList(); //颜色:红色,蓝色,黄色
                                if (SKUItemTotal == null || SKUItemTotal.Count != 2)
                                {
                                    result += $"第{i}个产品规格{skuItem}格式不对";
                                    return false;
                                }

                                SKUItemVule = SKUItemTotal[1].Split(',').ToList(); //红色,蓝色,黄色
                                if (SKUItemVule == null || SKUItemVule.Count <= 0)
                                {
                                    result += $"第{i}个产品规格{SKUItemTotal[0]}的值{SKUItemVule}格式不对";
                                    return false;
                                }
                                #endregion

                                //判断是否存在后台
                                skuId = EntSpecificationBLL.SingleModel.GetSKUId(aid, SKUItemTotal[0], "0");

                                if (string.IsNullOrEmpty(skuId))
                                {
                                    result += $"第{i}个产品规格{SKUItemTotal[0]}在后台不存在";
                                    return false;
                                }

                                List<pickspecification> listPickspecificationChild = new List<pickspecification>();

                                pickspecification pickspecificationParent = new pickspecification();
                                pickspecificationParent.id = Convert.ToInt32(skuId);
                                pickspecificationParent.name = SKUItemTotal[0];

                                listSpecificationkeys.Add(skuId);

                                parentSKUId = Convert.ToInt32(skuId);
                                foreach (string skuVule in SKUItemVule)
                                {
                                    skuId = EntSpecificationBLL.SingleModel.GetSKUId(aid, skuVule);
                                    if (string.IsNullOrEmpty(skuId))
                                    {
                                        result += $"第{i}个产品规格{SKUItemTotal[0]}的规格值{skuVule}在后台不存在";
                                        return false;
                                    }

                                    //检测规格值是否属于该规格
                                    skuId = EntSpecificationBLL.SingleModel.GetSKUId(aid, skuVule, parentSKUId.ToString());
                                    if (string.IsNullOrEmpty(skuId))
                                    {
                                        result += $"第{i}个产品规格值{skuVule}在后台不属于填写的规格{SKUItemTotal[0]}";
                                        return false;
                                    }

                                    listPickspecificationChild.Add(new pickspecification()
                                    {
                                        id = Convert.ToInt32(skuId),
                                        name = skuVule,
                                        parentid = parentSKUId
                                    });

                                    listSpecification.Add(skuId);
                                    listSpecificationVule.Add(skuVule);

                                }
                                pickspecificationParent.items = listPickspecificationChild;

                                listPickspecificationParnet.Add(pickspecificationParent);
                                skuVauleCount *= SKUItemVule.Count;
                            }

                            entGoods.pickspecification = JsonConvert.SerializeObject(listPickspecificationParnet);
                            entGoods.specificationkeys = string.Join(",", listSpecificationkeys);
                            entGoods.specification = string.Join(",", listSpecification);


                            //填写了规格则检测具体规格值是否填写正确
                            if (item["具体规格"] == null || string.IsNullOrEmpty(item["具体规格"].ToString()))
                            {
                                result += $"第{i}个产品规格填写了,具体规格必须填写";
                                return false;
                            }

                            listSKUDetail = item["具体规格"].ToString().Split(';').ToList();

                            if (listSKUDetail == null || listSKUDetail.Count <= 0)
                            {
                                result += $"第{i}个产品具体规格格式错误";
                                return false;
                            }
                            if (listSKUDetail.Count != skuVauleCount)
                            {
                                result += $"第{i}个产品具体规格值错误,应该包含{skuVauleCount}条具体规格";
                                return false;
                            }

                            foreach (string detail in listSKUDetail)
                            {
                                detailItem = detail.Split('|').ToList();
                                if (detailItem == null || detailItem.Count != 2)
                                {
                                    result += $"第{i}个产品具体规格值错误,错误数据{detail}";
                                    return false;
                                }

                                #region 获取单条规格值Id字符串
                                detailItemName = detailItem[0].Split('_').ToList();
                                if (detailItemName == null || detailItemName.Count <= 0)
                                {
                                    result += $"第{i}个产品具体规格值错误{detail}";
                                    return false;
                                }

                                List<string> listDetailItemVauleId = new List<string>();
                                foreach (string detailName in detailItemName)
                                {
                                    if (!listSpecificationVule.Contains(detailName.Trim()))
                                    {
                                        result += $"第{i}个产品具体规格值错误,{detailName}规格值不在所写规格里";
                                        return false;
                                    }
                                    detailItemVauleId = EntSpecificationBLL.SingleModel.GetSKUId(aid, detailName);
                                    if (string.IsNullOrEmpty(detailItemVauleId))
                                    {
                                        result += $"第{i}个产品规格值{detailName}在后台不存在";
                                        return false;
                                    }
                                    listDetailItemVauleId.Add(detailItemVauleId);

                                }
                                EntGoodsAttrDetail entGoodsAttrDetail = new EntGoodsAttrDetail();
                                entGoodsAttrDetail.id = string.Join("_", listDetailItemVauleId);
                                #endregion


                                #region 获取单条规格值，原价,购买价,库存,图片
                                detailItemVaule = detailItem[1].Split(',').ToList();//原价,购买价,库存,图片
                                if (detailItemVaule == null || detailItemVaule.Count != 4)
                                {
                                    result += $"第{i}个产品具体规格值价格错误{detail}";
                                    return false;
                                }
                                if (Convert.ToDouble(detailItemVaule[0]) < 0 || Convert.ToDouble(detailItemVaule[1]) <= 0 || Convert.ToInt32(detailItemVaule[2]) < 0)
                                {
                                    result += $"第{i}个产品具体规格值价格或者库存填写错误{detailItem[1]}";
                                    return false;
                                }

                                if (Convert.ToDouble(detailItemVaule[0]) != 0 && Convert.ToDouble(detailItemVaule[0]) < Convert.ToDouble(detailItemVaule[1]))
                                {
                                    result += $"第{i}个产品具体规格值价格填写错误,原价不能小于购买价{detailItem[1]}";
                                    return false;
                                }


                                entGoodsAttrDetail.originalPrice = (float)Convert.ToDouble(detailItemVaule[0]);
                                entGoodsAttrDetail.price = (float)Convert.ToDouble(detailItemVaule[1]);
                                if (entGoods.stockLimit)
                                {
                                    entGoodsAttrDetail.stock = Convert.ToInt32(detailItemVaule[2]);
                                    entGoods.stock += entGoodsAttrDetail.stock;
                                }

                                entGoodsAttrDetail.imgUrl = detailItemVaule[3];
                                #endregion


                                listEntGoodsAttrDetail.Add(entGoodsAttrDetail);
                            }


                            entGoods.specificationdetail = JsonConvert.SerializeObject(listEntGoodsAttrDetail);



                        }

                        if (item["产品参数"] != null && !string.IsNullOrEmpty(item["产品参数"].ToString()) && !string.IsNullOrWhiteSpace(item["产品参数"].ToString()))
                        {
                            listExtTypes = item["产品参数"].ToString().Split(',').ToList();
                            foreach (string extTypeItem in listExtTypes)
                            {

                                string s = string.Empty;
                                listItemExtTypes = extTypeItem.Split('-').ToList();
                                if (listItemExtTypes == null || listItemExtTypes.Count != 2)
                                {
                                    result += $"第{i}个产品参数格式填写错误{listItemExtTypes}";
                                    return false;
                                }


                                EntIndutypes extType = EntIndutypesBLL.SingleModel.GetExType(aid, listItemExtTypes[0], 0);
                                if (extType == null)
                                {
                                    result += $"第{i}个产品参数{listItemExtTypes[0]}在后台不存在";
                                    return false;
                                }
                                s += $"{extType.TypeId}";

                                extType = EntIndutypesBLL.SingleModel.GetExType(aid, listItemExtTypes[1], extType.TypeId);
                                if (extType == null)
                                {
                                    result += $"第{i}个产品参数值{listItemExtTypes[1]}在后台不存在或者不属于填写的参数{listItemExtTypes[0]}";
                                    return false;
                                }
                                s += $"-{extType.TypeId}";

                                listExtTypesDB.Add(s);

                            }
                            entGoods.exttypes = string.Join(",", listExtTypesDB);
                        }



                        if (item["虚拟销量"] != null && !string.IsNullOrEmpty(item["虚拟销量"].ToString()) && !string.IsNullOrWhiteSpace(item["虚拟销量"].ToString()))
                        {
                            entGoods.virtualSalesCount = Convert.ToInt32(item["虚拟销量"]);
                        }
                        if (item["单位"] != null && !string.IsNullOrEmpty(item["单位"].ToString()) && !string.IsNullOrWhiteSpace(item["单位"].ToString()))
                        {

                            if (EntGoodUnitBLL.SingleModel.GetEntGoodUnitId(aid, item["单位"].ToString()) <= 0)
                            {
                                result += $"第{i}个产品单位不存在于后台";
                                return false;
                            }
                            entGoods.unit = item["单位"].ToString();
                        }

                        if (item["物流重量(kg)"] != null && !string.IsNullOrEmpty(item["物流重量(kg)"].ToString()) && !string.IsNullOrWhiteSpace(item["物流重量(kg)"].ToString()))
                        {
                            entGoods.Weight = (int)Convert.ToDouble(item["物流重量(kg)"]) * 1000;
                        }
                        if (item["运费模板"] != null && !string.IsNullOrEmpty(item["运费模板"].ToString()) && !string.IsNullOrWhiteSpace(item["运费模板"].ToString()))
                        {
                            deliveryTemplateId = DeliveryTemplateBLL.SingleModel.GetDeliveryTemplateId(aid, item["运费模板"].ToString());
                            if (deliveryTemplateId <= 0)
                            {
                                result += $"第{i}个产品运费模板不存在于后台";
                                return false;
                            }
                            entGoods.TemplateId = deliveryTemplateId;
                        }
                        if (item["产品轮播图"] != null && !string.IsNullOrEmpty(item["产品轮播图"].ToString()) && !string.IsNullOrWhiteSpace(item["产品轮播图"].ToString()))
                        {
                            entGoods.slideimgs = item["产品轮播图"].ToString();
                        }
                        if (item["产品描述"] != null && !string.IsNullOrEmpty(item["产品描述"].ToString()) && !string.IsNullOrWhiteSpace(item["产品描述"].ToString()))
                        {
                            entGoods.description = item["产品描述"].ToString();
                        }
                        if (item["产品排序"] != null && !string.IsNullOrEmpty(item["产品排序"].ToString()) && !string.IsNullOrWhiteSpace(item["产品排序"].ToString()))
                        {
                            entGoods.sort = Convert.ToInt32(item["产品排序"]);
                        }
                        if (item["是否上架销售"] != null && !string.IsNullOrEmpty(item["是否上架销售"].ToString()) && !string.IsNullOrWhiteSpace(item["是否上架销售"].ToString()))
                        {
                            entGoods.tag = Convert.ToInt32(item["是否上架销售"]);
                        }

                        if (!string.IsNullOrEmpty(entGoods.specificationkeys) && !string.IsNullOrWhiteSpace(entGoods.specificationkeys))
                        {
                            //表示选择了多规格 则将产品原价购买价变为多规格中最小的值
                            listEntGoodsAttrDetail = listEntGoodsAttrDetail.OrderByDescending(x => x.price).ToList();
                            entGoods.price = listEntGoodsAttrDetail.FirstOrDefault().price;
                            entGoods.originalPrice = listEntGoodsAttrDetail.FirstOrDefault().originalPrice;
                        }
                        else
                        {
                            //表示没有多规格

                            if (entGoods.stockLimit)
                            {
                                //限制库存
                                if (item["库存"] != null && !string.IsNullOrEmpty(item["库存"].ToString()) && !string.IsNullOrWhiteSpace(item["库存"].ToString()))
                                {
                                    int stock = 0;
                                    if (!int.TryParse(item["库存"].ToString(), out stock))
                                    {
                                        result += $"第{i}个产品选择限制库存并且无规格请填写正确的库存";
                                        return false;
                                    }
                                    entGoods.stock = Convert.ToInt32(item["库存"]);
                                }
                            }

                            float p = 0;
                            if (item["产品原价"] != null && !string.IsNullOrEmpty(item["产品原价"].ToString()) && !string.IsNullOrWhiteSpace(item["产品原价"].ToString()))
                            {

                                if (!float.TryParse(item["产品原价"].ToString(), out p))
                                {
                                    result += $"第{i}个产品无规格请填写正确的原价";
                                    return false;
                                }
                                entGoods.originalPrice = p;
                            }
                            if (item["产品购买价"] != null && !string.IsNullOrEmpty(item["产品购买价"].ToString()) && !string.IsNullOrWhiteSpace(item["产品购买价"].ToString()))
                            {

                                if (!float.TryParse(item["产品购买价"].ToString(), out p))
                                {
                                    result += $"第{i}个产品无规格请填写正确的产品购买价";
                                    return false;
                                }
                                if (p > entGoods.originalPrice && entGoods.originalPrice > 0)
                                {
                                    result += $"第{i}个产品产品购买价不能大于原价";
                                    return false;
                                }

                                entGoods.price = p;
                            }

                        }

                        entGoods.aid = aid;
                        entGoods.addtime = DateTime.Now;
                        entGoods.updatetime = DateTime.Now;
                        entGoods.FromSource = 1;
                        transactionModel.Add(base.BuildAddSql(entGoods));


                    }
                    sw.Stop();
                    TimeSpan ts2 = sw.Elapsed;
                    if (base.ExecuteTransactionDataCorect(transactionModel.sqlArray))
                    {

                        //清除产品列表缓存
                        RemoveEntGoodListCache(aid);
                        result = $"导入成功,总计{i}个产品,总共花费{ts2.TotalSeconds}秒";
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    result += $"没有数据需要导入";
                    return true;
                }

            }
            catch (Exception ex)
            {
                result += $"{ex.Message}";
                return false;
            }
            finally
            {
                stream.Dispose();
                stream.Close();
            }


        }

        /// <summary>
        /// 产品导出
        /// </summary>
        /// <returns></returns>
        public DataTable ExportProductsFromDB(List<EntGoods> list)
        {

            DataTable table = new DataTable();
            table.Columns.AddRange(new[]
            {
                new DataColumn("产品名称"),
                new DataColumn("产品图片"),
                new DataColumn("产品分类"),
                new DataColumn("产品标签"),
                new DataColumn("货号"),
                new DataColumn("产品规格"),
                new DataColumn("具体规格"),
                new DataColumn("产品参数"),
                new DataColumn("限制库存"),
                new DataColumn("库存"),
                new DataColumn("虚拟销量"),
                new DataColumn("单位"),
                new DataColumn("物流重量(kg)"),
                new DataColumn("运费模板"),
                new DataColumn("产品轮播图"),
                new DataColumn("产品描述"),
                 new DataColumn("产品原价"),
                new DataColumn("产品购买价"),
                new DataColumn("产品排序"),
                new DataColumn("是否上架销售")
            });

            try
            {


                foreach (EntGoods item in list)
                {
                    DataRow row = table.NewRow();
                    row["产品名称"] = item.name;
                    row["产品图片"] = item.img;
                    row["产品分类"] = EntGoodTypeBLL.SingleModel.GetEntGoodTypeName(item.ptypes);
                    if (!string.IsNullOrEmpty(item.plabels))
                    {
                        row["产品标签"] = EntGoodLabelBLL.SingleModel.GetEntGoodsLabelStr(item.plabels);

                    }
                    row["货号"] = item.StockNo;
                    if (!string.IsNullOrEmpty(item.pickspecification) && !string.IsNullOrEmpty(item.specificationkeys))
                    {
                        //表示有多规格
                        string skuParentName = string.Empty;
                        List<pickspecification> listPickspecification = JsonConvert.DeserializeObject<List<pickspecification>>(item.pickspecification);
                        foreach (pickspecification sku in listPickspecification)
                        {

                            skuParentName += sku.name + ":";
                            foreach (pickspecification skuChild in sku.items)
                            {
                                skuParentName += skuChild.name + ",";
                            }
                            skuParentName = skuParentName.TrimEnd(',');
                            skuParentName += ";";
                        }

                        row["产品规格"] = skuParentName.TrimEnd(';');

                        StringBuilder sbAttrDetail = new StringBuilder();
                        List<EntGoodsAttrDetail> listEntGoodsAttrDetail = JsonConvert.DeserializeObject<List<EntGoodsAttrDetail>>(item.specificationdetail);
                        foreach (EntGoodsAttrDetail attrDetail in listEntGoodsAttrDetail)
                        {
                            sbAttrDetail.Append(EntSpecificationBLL.SingleModel.GetEntSpecificationName(attrDetail.id.Replace("_", ",")).Replace(",", "_"))
                                        .Append("|")
                                        .Append(attrDetail.originalPrice).Append(",")
                                        .Append(attrDetail.price).Append(",")
                                        .Append(attrDetail.stock).Append(",")
                                        .Append(attrDetail.imgUrl)
                                        .Append(";");
                        }
                        row["具体规格"] = sbAttrDetail.ToString().TrimEnd(';');

                    }

                    if (!string.IsNullOrEmpty(item.exttypes))
                    {
                        List<string> listExttypes = item.exttypes.Split(',').ToList();
                        StringBuilder sbExttypes = new StringBuilder();
                        foreach (string s in listExttypes)
                        {
                            sbExttypes.Append(EntIndutypesBLL.SingleModel.GetEntIndutypesName(item.aid, s.Replace("-", ",")).Replace(",", "-"))
                                      .Append(",");
                        }
                        row["产品参数"] = sbExttypes.ToString().TrimEnd(',');
                    }

                    row["限制库存"] = item.stockLimit ? 1 : 0;
                    row["库存"] = item.stock;
                    row["虚拟销量"] = item.virtualSalesCount;
                    row["单位"] = item.unit;
                    row["物流重量(kg)"] = item.Weight * 0.001;
                    row["运费模板"] = DeliveryTemplateBLL.SingleModel.GetDeliveryTemplateName(item.TemplateId);
                    row["产品轮播图"] = item.slideimgs;
                    row["产品描述"] = item.description;
                    row["产品原价"] = item.originalPrice;
                    row["产品购买价"] = item.price;
                    row["产品排序"] = item.sort;
                    row["是否上架销售"] = item.tag;
                    table.Rows.Add(row);

                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
            return table;

        }



    }
}