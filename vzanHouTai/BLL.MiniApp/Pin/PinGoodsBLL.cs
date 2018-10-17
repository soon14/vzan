using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utility;

namespace BLL.MiniApp.Pin
{
    public class PinGoodsBLL : BaseMySql<PinGoods>
    {
        #region 单例模式
        private static PinGoodsBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinGoodsBLL()
        {

        }

        public static PinGoodsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinGoodsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static string modelKey = "PinGoodKey_{0}";//id
        public static string key_new_pin_goods = "temp_p_pin_description_0";//用于临时保存webview中id=0的产品详情，当添加成功时清空

        public List<PinGoods> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PinGoods>();

            return base.GetList($"id in ({ids})");
        }

        public Tuple<List<PinGoods>, int> GetListFromTable(int aId, int storeId, int pageIndex, int pageSize, string kw = "",int cateIdOne=0, int cateId = 0)
        {
            List<MySqlParameter> parameters = null;
            StringBuilder sqlFilter = new StringBuilder();
            StringBuilder sqlFilter2 = new StringBuilder();
            pageIndex = pageIndex - 1;
            if (pageIndex < 0)
                pageIndex = 0;

            if (!string.IsNullOrEmpty(kw))
            {
                if (parameters == null)
                    parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter("@kw", Utils.FuzzyQuery(kw)));
                sqlFilter.Append(" and g.name like @kw ");
                sqlFilter2.Append(" and name like @kw ");
            }
            if (cateIdOne > 0)
            {
                if (parameters == null)
                    parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter("@cateIdOne", cateIdOne));
                sqlFilter.Append(" and g.cateIdOne=@cateIdOne ");
                sqlFilter2.Append(" and cateIdOne=@cateIdOne ");
            }
            if (cateId > 0)
            {
                if (parameters == null)
                    parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter("@cateId", cateId));
                sqlFilter.Append(" and g.cateId=@cateId ");
                sqlFilter2.Append(" and cateId=@cateId ");
            }
            
            DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT g.*,(SELECT name from PinCategory c where c.id=g.cateId) as cateName,(SELECT name from PinCategory c where c.id=g.cateIdOne) as cateNameOne from {TableName} g where g.aid={aId} and g.storeid={storeId} and g.state<>-1 {sqlFilter}  order by g.sort desc limit {pageSize * pageIndex},{pageSize}", parameters?.ToArray()).Tables[0];
            int totalCount = GetCount($"aid={aId} and storeid={storeId} and state<>-1 {sqlFilter2}", parameters?.ToArray());
            return Tuple.Create(DataHelper.ConvertDataTableToList<PinGoods>(dt), totalCount);
        }

        /// <summary>
        /// 批量更新排序
        /// </summary>
        /// <param name="sortData">
        /// id_is_order,id_is_order,id_is_order
        /// </param>
        /// <returns></returns>
        public bool UpdateSortBatch(string sortData)
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

        public bool UpdateRankBatch(string sortData)
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
                sql.Add($"update {TableName} set indexrank=@indexrank where id=@id");
                sqlParameters.Add(new MySqlParameter[] {
                    new MySqlParameter("@indexrank",idSortArray[1]),
                    new MySqlParameter("@id",idSortArray[0])
                });
            }
            return ExecuteTransaction(sql.ToArray(), sqlParameters.ToArray());
        }

        public override PinGoods GetModel(int Id)
        {
            //string key = string.Format(modelKey, Id);
            //PinGoods model = RedisUtil.Get<PinGoods>(key);

            //if (model == null)
            //{
            //    model = base.GetModel(Id);
            //    if (model != null)
            //    {
            //        RedisUtil.Set(key, model, TimeSpan.FromHours(4));
            //    }
            //}
            //return model;
            return base.GetModel(Id);
        }

        public override bool Update(PinGoods model)
        {
            RemoveCache(model);
            model.updateTime = DateTime.Now;
            return base.Update(model);
        }

        public void RemoveCache(PinGoods model)
        {
            if (model != null)
            {
                string key = string.Format(modelKey, model.id);
                RedisUtil.Remove(key);
            }
        }
        public void RemoveCache(List<PinGoods> models)
        {
            models?.ForEach(model => RemoveCache(model));
        }

        public PinGoods GetModelById_State(int goodsId, int state)
        {
            PinGoods model = GetModel(goodsId);
            if (model != null && model.state != state)
            {
                model = null;
            }
            return model;
        }

        public bool UpdateFreightTemplate(int appId, string goodIds, int templateId)
        {
            TransactionModel tran = new TransactionModel();
            List<PinGoods> goodList = GetListByPara(freightTemplate: templateId);
            //goodList.ForEach(good =>
            //{
            //    good.aId = appId;
            //    good.FreightTemplate = templateId;
            //    tran.Add(BuildUpdateSql(good, "FreightTemplate"));
            //});
            tran.Add($"UPDATE PinGoods SET FreightTemplate=0 WHERE FreightTemplate={templateId}");
            if (!string.IsNullOrWhiteSpace(goodIds))
            {
                tran.Add($"UPDATE PinGoods SET FreightTemplate={templateId} WHERE Id IN ({goodIds})");
            }
            bool result = base.ExecuteTransaction(tran.sqlArray, tran.ParameterArray);
            if (result)
            {
                RemoveCache(goodList);
            }
            //返回事务执行结果
            return result;
        }

        public bool UpdateStock(PinGoods goods, string specificationId, int count,ref SpecificationDetailModel specificationDetail, out string msg)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(specificationId))
            {
                specificationDetail = goods.SpecificationDetailList.Find(s => s.id == specificationId);
                if (specificationDetail == null)
                {
                    msg = "所选规格已失效";
                    return result;
                }
                if (specificationDetail.stock - count < 0 && goods.stockLimit)
                {
                    msg = "库存不足";
                    return result;
                }
                else if (goods.stockLimit)
                {
                    List<SpecificationDetailModel> list = goods.SpecificationDetailList;
                    list.ForEach(spec =>
                    {
                        if (spec.id == specificationId)
                        {
                            spec.stock = spec.stock - count;
                        }
                    });
                    goods.specificationdetail = JsonConvert.SerializeObject(list);
                    result = Update(goods, "specificationdetail");//减库存
                }
                else
                {
                    result = true;
                }
            }
            else
            {
                if (goods.stockLimit && goods.stock - count < 0)
                {
                    msg = "库存不足";
                    return result;
                }
                else if (goods.stockLimit)
                {
                    goods.stock = goods.stock - count;
                    result = Update(goods, "stock");//减库存
                }
                else
                {
                    result = true;
                }
            }
            msg = result ? "" : "系统繁忙";//去库存失败
            return result;
        }
        /// <summary>
        /// 回滚库存
        /// </summary>
        /// <param name="order"></param>
        /// <param name="tran"></param>
        public bool RollbackGoodsStock(PinGoodsOrder order, TransactionModel tran = null)
        {
            bool isTran = tran != null;
            if (!isTran) tran = new TransactionModel();
            PinGoods goods = GetModel(order.goodsId);
            if (goods == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"回滚库存失败 商品不存在 orderId:{order.id} goodsId:{order.goodsId}"));
                return false;
            }
            if (!string.IsNullOrEmpty(order.specificationId))
            {
                List<SpecificationDetailModel> specList = goods.SpecificationDetailList;
                specList.ForEach(spec =>
                {
                    if (spec.id == order.specificationId)
                    {
                        spec.stock += order.count;
                    }
                });
                goods.specificationdetail = JsonConvert.SerializeObject(specList);
                tran.Add(BuildUpdateSql(goods, "specificationdetail"));
            }
            else
            {
                goods.stock += order.count;
                tran.Add(BuildUpdateSql(goods, "stock"));
            }
            if (isTran) return true;
            return base.ExecuteTransaction(tran.sqlArray, tran.ParameterArray);
        }
        /// <summary>
        /// 获取店铺已上架商品数量,已上架且已审核的
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public int GetCountByStoreId(int storeId)
        {
            int total = 0;
            if (storeId <= 0)
            {
                return total;
            }
            string sql = $"select count(1) from pingoods where storeid={storeId} and state=1 and auditState=1";
            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql);
            total = result == DBNull.Value ? 0 : Convert.ToInt32(result);
            return total;
        }

        public List<PinGoods> GetListByAid_State(int aid, int state,int pageIndex,int pageSize,out int recordCount)
        {
            List<PinGoods> list = new List<PinGoods>();
            recordCount = 0;
            if (aid <= 0)
            {
                return list;
            }
            string sqlwhere = $" aid={aid} and state={state}";
            list = GetList(sqlwhere, pageSize, pageIndex);
            recordCount = GetCount(sqlwhere);
            return list;
        }

        public List<PinGoods> GetListByAid(int aid, bool isGetDelete = false)
        {
            string whereSql = BuildWhereSql(aId: aid, isGetDelete: isGetDelete);
            return GetList(whereSql);
        }

        public List<PinGoods> GetListByStoreId(int storeId, int aid, bool isGetDelete = false, int freightTemplate = 0)
        {
            string whereSql = BuildWhereSql(aId: aid, storeId: storeId, isGetDelete: isGetDelete, freightTemplateId: freightTemplate);
            return GetList(whereSql);
        }

        public List<PinGoods> GetListByPara(int? storeId = null, int? aid = null, bool? isGetDelete = null, int? freightTemplate = null)
        {
            string whereSql = BuildWhereSql(aId: aid, storeId: storeId, isGetDelete: isGetDelete, freightTemplateId: freightTemplate);
            return GetList(whereSql);
        }

        public int GetCountByPara(int storeId, int aid, bool isGetDelete = false, int freightTemplate = 0)
        {
            string whereSql = BuildWhereSql(aId: aid, storeId: storeId, isGetDelete: isGetDelete, freightTemplateId: freightTemplate);
            return GetCount(whereSql);
        }

        public string BuildWhereSql(int? aId = null, int? storeId = null, int? state = null, int? freightTemplateId = null, bool? isGetDelete = null)
        {
            List<string> whereSql = new List<string>();
            if (aId.HasValue)
            {
                whereSql.Add($"aId = {aId.Value}");
            }
            if (isGetDelete.HasValue)
            {
                whereSql.Add(isGetDelete.Value ? $"State = -1" : $"State != -1");
            }
            else if (state.HasValue)
            {
                whereSql.Add($"State = {state.Value}");
            }
            if (storeId.HasValue)
            {
                whereSql.Add($"StoreId = {storeId.Value}");
            }
            if (freightTemplateId.HasValue && freightTemplateId.Value > 0)
            {
                whereSql.Add($"FreightTemplate = {freightTemplateId.Value}");
            }
            return string.Join(" AND ", whereSql);
        }
    }
        
}