using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Plat;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Model;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utility;

namespace BLL.MiniApp.PlatChild
{
    public class PlatChildGoodsBLL : BaseMySql<PlatChildGoods>
    {
        #region 单例模式
        private static PlatChildGoodsBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatChildGoodsBLL()
        {

        }

        public static PlatChildGoodsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatChildGoodsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private static readonly string _redis_PlatChildGoodsCodeUrlKey = "PlatChildGoodsCodeUrl_{0}_{1}";
        private static readonly string Redis_PlatChildProductList = "PlatChildProduct_{0}_{1}_{2}";
        private static readonly string Redis_PlatChildProductList_version = "PlatChildProduct_version_{0}";
        
        


        /// <summary>
        /// 获取当前推荐产品的数量 不包括当前设置产品
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="curId"></param>
        /// <returns></returns>
        public int GetTopCount(int aid,int curId)
        {
            return base.GetCount($"id<>{curId} and state=1 and aid={aid} and TopState=1");
        }

        public string GetPlabelStr(string plabels)
        {
            return SqlMySql.ExecuteScalar(connName, CommandType.Text, $"SELECT group_concat(name order by sort desc) from PlatChildGoodsLabel where id in ({plabels})").ToString();
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
            string sql = $" update PlatChildGoods set salesCount = salesCount + {salesCount} where id = {goodsId} ";
            SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql);
        }
        
        public PlatChildGoods GetModelById(int id ,int state)
        {
            return GetModel($"id={id} and state={state}");
        }
       
        public List<PlatChildGoods> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PlatChildGoods>();
            return base.GetList($"id in ({ids})");
        }

        public List<PlatChildGoods> GetListByRedis(int appId , ref int count, string search ="", int plabels=0, int ptype=0, int ptag=-1, int pageIndex=1, int pageSize=10,string orderWhere= " TopState desc,sort desc, id desc ", int templateid=-1)
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
            if (templateid !=-1)
            {
                if(templateid>0)
                {
                    strwhere += $" and TemplateId={templateid} ";
                }
                
                openredis = false;
            }

            string key = string.Format(Redis_PlatChildProductList, appId, pageSize, pageIndex);
            string version_key = string.Format(Redis_PlatChildProductList_version, appId);

            int version = RedisUtil.GetVersion(version_key);
            RedisModel<PlatChildGoods> list = RedisUtil.Get<RedisModel<PlatChildGoods>>(key);
            //RedisModel<PlatChildProduct> list = new RedisModel<PlatChildProduct>();
            if (!openredis || list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || version != list.DataVersion)
            {
                list = new RedisModel<PlatChildGoods>();
                #region 重新查询数据
              //  log4net.LogHelper.WriteInfo(this.GetType(),$"{strwhere},{pageSize},{pageIndex},{orderWhere}");
                List<PlatChildGoods> DataList = GetListByParam(strwhere, parameters.ToArray(), pageSize, pageIndex, "*", orderWhere);
                count = GetCount(strwhere, parameters.ToArray());
                DataList.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.Categorys))
                    {

                        string sql = $"SELECT GROUP_CONCAT(`name`) from PlatChildGoodsCategory where FIND_IN_SET(id,@ptypes)";
                        x.CategorysStr = DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                            CommandType.Text, sql,
                            new MySqlParameter[] { new MySqlParameter("@ptypes", x.Categorys) }).ToString();
                    }

                    if (!string.IsNullOrEmpty(x.Plabels))
                    {
                        string sql = $"SELECT group_concat(name) from PlatChildGoodsLabel where FIND_IN_SET(id,@plabels)";
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
                    RedisUtil.Set<RedisModel<PlatChildGoods>>(key, list, TimeSpan.FromHours(12));
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
                RedisUtil.SetVersion(string.Format(Redis_PlatChildProductList_version, appid));
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

            PlatChildGoods good = GetModel(goodsid);
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
        public int GetGoodQtyByModel(PlatChildGoods goods, string attrSpacStr = "")
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

        public List<PlatChildGoods> GetListGoods(int aid, string search, string typeid, string pricesort, int pagesize, int pageindex, int isFirstType = -1)
        {
            string wherestr = $" aid={aid} and state=1 and tag = 1";
            string sortstr = " sort desc,id desc ";//id asc
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string entGoodTypeIds = string.Empty;

            if (!string.IsNullOrEmpty(search))
            {
             //   log4net.LogHelper.WriteInfo(this.GetType(), $"wherestr111={wherestr};{sortstr};");
                //将该关键词当做类别进行模糊匹配 查询类别id
                entGoodTypeIds =PlatChildGoodsCategoryBLL.SingleModel.GetGoodCategoryIds(aid, 100, 1, search);
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
              //  log4net.LogHelper.WriteInfo(this.GetType(), $"wherestr222={wherestr};{sortstr};");
            }

            if (!string.IsNullOrEmpty(typeid))
            {
                //判断是否是大类,如果是大类先转换为下面的小类

                if (isFirstType == 0)
                {
                    List<string> listids = new List<string>();

                    List<PlatChildGoodsCategory> listGoodType = PlatChildGoodsCategoryBLL.SingleModel.GetList($"aid={aid} and state=0 and parentid in({typeid})");
                     //log4net.LogHelper.WriteInfo(this.GetType(), $"aid={aid} and state=1 and parentid in({typeid});{listGoodType.Count}");
                    foreach (PlatChildGoodsCategory item in listGoodType)
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

                    // log4net.LogHelper.WriteInfo(this.GetType(),$"typeid={typeid};isFirstType={isFirstType}");
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

            List<PlatChildGoods> goodslist = base.GetListByParam(wherestr, parameters.ToArray(), pagesize, pageindex, "*", sortstr);
            return goodslist;
        }
        
        public bool UpdateProductSort(int appId, string datajson)
        {

            //获取修改排序的sql语句
            RemoveEntGoodListCache(appId);
            string sql = "update PlatChildGoods set Sort = {0} where id={1}";
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
                //log4net.LogHelper.WriteInfo(this.GetType(),Newtonsoft.Json.JsonConvert.SerializeObject(tran.sqlArray));
                if (base.ExecuteTransaction(tran.sqlArray))
                {
                   
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// 同步产品到 店铺所属平台 以及 所属平台的一级 二级代理商平台
        /// </summary>
        /// <param name="id"></param>
        /// <param name="platStore"></param>
        /// <param name="agentinfo"></param>
        /// <returns></returns>
        public bool SyncProduct(int id, PlatStore platStore, Agentinfo agentinfo)
        {
            TransactionModel tramModelGoodsRelation = new TransactionModel();
            #region 当前新增产品插入关系表
            PlatGoodsRelation platGoodsRelation = new PlatGoodsRelation();
            platGoodsRelation.AddTime = DateTime.Now;
            platGoodsRelation.Aid = platStore.BindPlatAid;
            platGoodsRelation.GoodsId = id;
            PlatStoreCategoryConfig platStoreCategoryConfig = PlatStoreCategoryConfigBLL.SingleModel.GetModelByAid(platStore.BindPlatAid);
            if (platStoreCategoryConfig != null)
            {
                platGoodsRelation.Synchronized = platStoreCategoryConfig.SyncSwitch;
            }
            tramModelGoodsRelation.Add(PlatGoodsRelationBLL.SingleModel.BuildAddSql(platGoodsRelation));
            #endregion




            #region  //2.查找上级代理 暂时先屏蔽
            //AgentDistributionRelation agentDistributionRelationFirst = new AgentDistributionRelationBLL().GetModel(agentinfo.id);
            //if (agentDistributionRelationFirst != null)
            //{
            //    Agentinfo agentinfoFirst = _agentinfoBll.GetModel(agentDistributionRelationFirst.ParentAgentId);
            //    if (agentinfoFirst != null)
            //    {
            //        XcxAppAccountRelation xcxAppAccountRelationFirst = _xcxappaccountrelationBll.GetModelByaccountidAndTid(agentinfoFirst.useraccountid, (int)TmpType.小未平台);
            //        if (xcxAppAccountRelationFirst != null)
            //        {
            //            PlatGoodsRelation platGoodsRelationFist = new PlatGoodsRelation();
            //            platGoodsRelationFist.GoodsId = id;
            //            platGoodsRelationFist.AddTime = DateTime.Now;
            //            platGoodsRelationFist.Aid = xcxAppAccountRelationFirst.Id;
            //            PlatStoreCategoryConfig platStoreCategoryConfigFist = _platStoreCategoryConfigBLL.GetModelByAid(xcxAppAccountRelationFirst.Id);
            //            if (platStoreCategoryConfigFist != null)
            //            {
            //                platGoodsRelationFist.Synchronized = platStoreCategoryConfigFist.SyncSwitch;
            //            }
            //            tramModelGoodsRelation.Add(_platGoodsRelationBLL.BuildAddSql(platGoodsRelationFist));



            //            //3.查找上级的上级代理
            //            AgentDistributionRelation agentDistributionRelationSecond = new AgentDistributionRelationBLL().GetModel(agentinfoFirst.id);
            //            if (agentDistributionRelationSecond != null)
            //            {
            //                Agentinfo agentinfoSecond = _agentinfoBll.GetModel(agentDistributionRelationSecond.ParentAgentId);
            //                if (agentinfoSecond != null)
            //                {
            //                    XcxAppAccountRelation xcxAppAccountRelationSecond = _xcxappaccountrelationBll.GetModelByaccountidAndTid(agentinfoSecond.useraccountid, (int)TmpType.小未平台);
            //                    if (xcxAppAccountRelationSecond != null)
            //                    {
            //                        PlatGoodsRelation platGoodsRelationSecond = new PlatGoodsRelation();
            //                        platGoodsRelationSecond.GoodsId = id;
            //                        platGoodsRelationSecond.AddTime = DateTime.Now;
            //                        platGoodsRelationSecond.Aid = xcxAppAccountRelationSecond.Id;
            //                        PlatStoreCategoryConfig platStoreCategoryConfigSecond = _platStoreCategoryConfigBLL.GetModelByAid(xcxAppAccountRelationSecond.Id);
            //                        if (platStoreCategoryConfigSecond != null)
            //                        {
            //                            platGoodsRelationSecond.Synchronized = platStoreCategoryConfigSecond.SyncSwitch;
            //                        }
            //                        tramModelGoodsRelation.Add(_platGoodsRelationBLL.BuildAddSql(platGoodsRelationSecond));
            //                    }


            //                }
            //            }


            //        }
            //    }
            //} 
            #endregion

            return base.ExecuteTransactionDataCorect(tramModelGoodsRelation.sqlArray);


        }

        public bool UpdateTemplateByIds(int appId, int templateId, string productIds)
        {
            TransactionModel tran = new TransactionModel();
            

            tran.Add($"update PlatChildGoods set TemplateId=0 where TemplateId={templateId}");
            if (!string.IsNullOrWhiteSpace(productIds))
            {
                tran.Add($"update PlatChildGoods set TemplateId={templateId} where id in ({productIds})");
            }
            
            //返回事务执行结果
            return base.ExecuteTransaction(tran.sqlArray, tran.ParameterArray);
        }


        /// <summary>
        /// 商品一物一码
        /// </summary>
        /// <param name="goodsId">商品ID</param>
        /// <param name="pageUrl">小程序商品详情页路径</param>
        /// <param name="type">0：子模板小程序，1：平台版小程序</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string GetGoodsCodeUrl(int goodsId, string pageUrl,int type, ref string msg)
        {
            
            PlatChildGoods goods = base.GetModel(goodsId);
            if (goods == null)
            {
                msg = "商品二维码：找不到商品数据";
                return "";
            }

            string url = RedisUtil.Get<string>(string.Format(_redis_PlatChildGoodsCodeUrlKey, type,goodsId));
            if (!string.IsNullOrEmpty(url))
            {
                return url;
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(goods.AId);
            if (xcxrelation == null || string.IsNullOrEmpty(xcxrelation.AppId))
            {
                msg = "商品二维码：店铺没绑定小程序";
                return "";
            }
            
            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcxrelation, ref token))
            {
                msg = token;
                return "";
            }
            
            //小程序二维码
            string scen = $"goodsId={goodsId}";
            qrcodeclass result = CommondHelper.GetMiniAppQrcode(token,pageUrl, scen);
            if (result != null && result.isok > 0)
            {
                url = result.url;
                if (string.IsNullOrEmpty(url))
                {
                    return "";
                }
                url = url.Replace("http:", "https:");
                RedisUtil.Set<string>(string.Format(_redis_PlatChildGoodsCodeUrlKey, type,goodsId), url, TimeSpan.FromDays(1));
                return url;
            }

            msg = "商品二维码：获取失败";
            return "";
        }
    }
}
