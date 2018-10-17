using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Plat;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Model;
using Entity.MiniApp.Plat;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utility;

namespace BLL.MiniApp.Qiye
{
    public class QiyeGoodsBLL : BaseMySql<QiyeGoods>
    {
        #region 单例模式
        private static QiyeGoodsBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeGoodsBLL()
        {

        }

        public static QiyeGoodsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeGoodsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static readonly string Redis_QiyeProductList = "QiyeProduct_{0}_{1}_{2}";
        public static readonly string Redis_QiyeProductList_version = "QiyeProduct_version_{0}";
        
        
        public string GetPlabelStr(string plabels)
        {
            return SqlMySql.ExecuteScalar(connName, CommandType.Text, $"SELECT group_concat(name order by sort desc) from QiyeGoodsLabel where id in ({plabels})").ToString();
        }

        public int GetCountByTemplateId(int templateid)
        {
            return base.GetCount($"TemplateId={templateid}");
        }

        /// <summary>
        /// 获取该分类下的项目数目
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetServiceCountById(int aid, int id)
        {
            return GetCount($"aid ={aid} and state> 0 and FIND_IN_SET('{id}',Categorys)");
        }

        public void UpdateSaleCountById(int goodsId, int salesCount)
        {
            string sql = $" update QiyeGoods set salesCount = salesCount + {salesCount} where id = {goodsId} ";
            SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql);
        }

        public QiyeGoods GetModelById(int id, int state)
        {
            return GetModel($"id={id} and state={state}");
        }

        public List<QiyeGoods> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<QiyeGoods>();
            return base.GetList($"id in ({ids})");
        }

        public List<QiyeGoods> GetListByRedis(int appId, ref int count, string search = "", int plabels = 0, int ptype = 0, int ptag = -1, int pageIndex = 1, int pageSize = 10, string orderWhere = " sort desc, id desc ", int templateid = -1)
        {
            string strwhere = $"aid={appId} and state=1";
            bool openredis = true;

            List<MySqlParameter> parameters = new List<MySqlParameter>();
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
                strwhere += $" and FIND_IN_SET (@Categorys,Categorys) ";
                parameters.Add(new MySqlParameter("Categorys", ptype));
                openredis = false;
            }
            if (ptag > -1)
            {
                strwhere += $" and tag={ptag} ";
                openredis = false;
            }
            if (templateid != -1)
            {
                if (templateid > 0)
                {
                    strwhere += $" and TemplateId={templateid} ";
                }

                openredis = false;
            }

            string key = string.Format(Redis_QiyeProductList, appId, pageSize, pageIndex);
            string version_key = string.Format(Redis_QiyeProductList_version, appId);

            int version = RedisUtil.GetVersion(version_key);
            RedisModel<QiyeGoods> list = RedisUtil.Get<RedisModel<QiyeGoods>>(key);
            //RedisModel<QiyeProduct> list = new RedisModel<QiyeProduct>();
            if (!openredis || list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || version != list.DataVersion)
            {
                list = new RedisModel<QiyeGoods>();
                #region 重新查询数据
                //  log4net.LogHelper.WriteInfo(this.GetType(),$"{strwhere},{pageSize},{pageIndex},{orderWhere}");
                List<QiyeGoods> DataList = GetListByParam(strwhere, parameters.ToArray(), pageSize, pageIndex, "*", orderWhere);
                count = GetCount(strwhere, parameters.ToArray());
                DataList.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.Categorys))
                    {

                        string sql = $"SELECT GROUP_CONCAT(`name`) from QiyeGoodsCategory where FIND_IN_SET(id,@ptypes)";
                        x.CategorysStr = DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                            CommandType.Text, sql,
                            new MySqlParameter[] { new MySqlParameter("@ptypes", x.Categorys) }).ToString();
                    }

                    if (!string.IsNullOrEmpty(x.Plabels))
                    {
                        string sql = $"SELECT group_concat(name) from QiyeGoodsLabel where FIND_IN_SET(id,@plabels)";
                        x.PlabelStr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(),
                            CommandType.Text, sql,
                            new MySqlParameter[] { new MySqlParameter("@plabels", x.Plabels) }).ToString();
                    }

                });
                #endregion

                #region 缓存
                list.DataList = DataList;
                list.Count = count;
                list.DataVersion = version;

                if (openredis && templateid == -1) //只缓存默认条件的数据,若有条件则不缓存
                {
                    RedisUtil.Set<RedisModel<QiyeGoods>>(key, list, TimeSpan.FromHours(12));
                }
                #endregion
            }
            else
            {
                count = list.Count;
            }

            return list.DataList;
        }

        /// <summary>
        /// 清除商品的列表缓存
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveEntGoodListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_QiyeProductList_version, appid));
            }
        }

        /// <summary>
        /// 查询当前商品库存
        /// </summary>
        /// <param name="goodsid"></param>
        /// <param name="attrSpacStr"></param>
        /// <returns></returns>
        public int GetGoodQty(int goodsid, string attrSpacStr = "")
        {
            int goodQty = 0;

            QiyeGoods good = GetModel(goodsid);
            if (string.IsNullOrWhiteSpace(attrSpacStr))
            {
                goodQty = good.Stock;
            }
            else
            {
                GoodsSpecDetail goodBySpacStr = good.GASDetailList.Where(x => x.Id.Equals(attrSpacStr))?.First();
                if (goodBySpacStr != null)
                {
                    goodQty = goodBySpacStr.Stock;
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
        public int GetGoodQtyByModel(QiyeGoods goods, string attrSpacStr = "")
        {
            int goodQty = 0;

            if (string.IsNullOrWhiteSpace(attrSpacStr))
            {
                goodQty = goods.Stock;
            }
            else
            {
                List<GoodsSpecDetail> goodList = goods.GASDetailList.Where(x => x.Id.Equals(attrSpacStr)).ToList();
                if (goodList != null && goodList.Any())
                {
                    GoodsSpecDetail goodBySpacStr = goodList.First();
                    if (goodBySpacStr != null)
                    {
                        goodQty = goodBySpacStr.Stock;
                    }
                }

            }
            return goodQty;
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

        public List<QiyeGoods> GetListGoods(int aid, string search, string typeid, string pricesort, int pagesize, int pageindex, int isFirstType = -1,string saleCountSort="")
        {
            string wherestr = $" aid={aid} and state=1 and tag = 1";
            string sortstr = " sort desc,id desc ";//id asc
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string entGoodTypeIds = string.Empty;

            if (!string.IsNullOrEmpty(search))
            {
                //将该关键词当做类别进行模糊匹配 查询类别id
                entGoodTypeIds = QiyeGoodsCategoryBLL.SingleModel.GetGoodCategoryIds(aid, 100, 1, search);
                if (!string.IsNullOrEmpty(entGoodTypeIds))
                {
                    wherestr += $" and( (name like @name) or (Categorys like'%{entGoodTypeIds}%'))";//产品与小类关联
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

                    List<QiyeGoodsCategory> listGoodType = QiyeGoodsCategoryBLL.SingleModel.GetList($"aid={aid} and state=0 and parentid in({typeid})");

                    foreach (QiyeGoodsCategory item in listGoodType)
                    {
                        listids.Add(item.Id.ToString());
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
                    typeidSplit = typeidSplit.Select(p => p = "FIND_IN_SET('" + p + "',Categorys)").ToList();
                    wherestr += $" and (" + string.Join(" or ", typeidSplit) + ")";
                }
            }


            if (!string.IsNullOrEmpty(pricesort) && (pricesort == "asc" || pricesort == "desc"))
            {
                sortstr = " price " + pricesort;
            }
            if (!string.IsNullOrEmpty(saleCountSort) && (saleCountSort == "asc" || saleCountSort == "desc"))
            {
                sortstr = " SalesCount+VirtualSalesCount " + saleCountSort;
            }

            List<QiyeGoods> goodslist = base.GetListByParam(wherestr, parameters.ToArray(), pagesize, pageindex, "*", sortstr);
            //log4net.LogHelper.WriteInfo(this.GetType(), $"wherestr={wherestr}; {pagesize};  {pageindex};{goodslist.Count}");
            return goodslist;
        }

        public bool UpdateProductSort(int appId, string datajson)
        {

            //获取修改排序的sql语句
            RemoveEntGoodListCache(appId);
            string sql = "update QiyeGoods set Sort = {0} where id={1}";
            if (string.IsNullOrEmpty(sql))
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
                    tran.Add(string.Format(sql, sort, dataid));
                }

                if (base.ExecuteTransaction(tran.sqlArray))
                {

                    return true;
                }
            }

            return false;
        }

        public bool UpdateTemplateByIds(int appId, int templateId, string productIds)
        {
            TransactionModel tran = new TransactionModel();


            tran.Add($"update QiyeGoods set TemplateId=0 where TemplateId={templateId}");
            if (!string.IsNullOrWhiteSpace(productIds))
            {
                tran.Add($"update QiyeGoods set TemplateId={templateId} where id in ({productIds})");
            }

            //返回事务执行结果
            return base.ExecuteTransaction(tran.sqlArray, tran.ParameterArray);
        }
    }
}
