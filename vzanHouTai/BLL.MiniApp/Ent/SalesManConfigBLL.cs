using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Ent
{
    public class SalesManConfigBLL : BaseMySql<SalesManConfig>
    {
        #region 单例模式
        private static SalesManConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private SalesManConfigBLL()
        {

        }

        public static SalesManConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SalesManConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 更新分销产品分销相关信息 只有返回大于0才表示成功
        /// </summary>
        /// <param name="salesmanGoods"></param>
        /// <param name="appId"></param>
        /// <param name="pageType"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public int UpdateSalesmanGoods(SalesmanGoods salesmanGoods, int appId, int pageType, string ids = "")
        {


            int code = -999;
            string strWhere = string.Empty;
            switch (pageType)
            {
                case (int)TmpType.小程序专业模板:
                    SalesManConfig salesManConfig = base.GetModel($"appId={appId}");
                    if (!string.IsNullOrEmpty(salesManConfig.configStr))
                    {
                        salesManConfig.configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);
                    }
                    double cps_rate = 0.00;
                    if (salesmanGoods.isDefaultCps_Rate == 0 && salesManConfig.configModel != null)
                    {
                        //默认佣金比例后台设置的
                        cps_rate = salesManConfig.configModel.payMentManager.cps_rate;
                    }
                    else
                    {
                        //自定义佣金比例
                        cps_rate = salesmanGoods.cps_rate;
                    }
                    if (string.IsNullOrEmpty(ids))
                    {
                        //单个设置
                        strWhere = $"aid={appId} and Id={salesmanGoods.goodsId} and state=1 and goodtype={(int)EntGoodsType.普通产品}";
                        EntGoods entGood = EntGoodsBLL.SingleModel.GetModel(strWhere);
                        if (entGood == null)
                        {
                            code = -1;
                        }
                        else
                        {
                            entGood.distributionTime = DateTime.Now;
                            entGood.isDefaultCps_Rate = salesmanGoods.isDefaultCps_Rate;
                            entGood.isDistribution = salesmanGoods.isDistribution;

                            entGood.cps_rate = cps_rate;


                            if (EntGoodsBLL.SingleModel.Update(entGood, "distributionTime,isDefaultCps_Rate,isDistribution,cps_rate"))
                            {
                                code = 1;
                            }
                            else
                            {
                                code = -2;
                            }
                        }


                    }
                    else
                    {
                        //批量设置
                        code = EntGoodsBLL.SingleModel.ExecuteNonQuery($"update EntGoods set distributionTime='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',isDefaultCps_Rate={salesmanGoods.isDefaultCps_Rate},isDistribution={salesmanGoods.isDistribution},cps_rate={cps_rate} where Id in({ids})");
                    }
                    break;
            }

            return code;


        }

        /// <summary>
        /// 获取分销产品
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="goodsName"></param>
        /// <param name="isDistribution"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<SalesmanGoods> GetListSalesmanGoods(int appId, int pageType, string goodsName = "", int isDistribution = -1, int pageIndex = 1, int pageSize = 10, string sortWhere = "sort desc, id desc")
        {
            List<SalesmanGoods> listSalesmanGoods = new List<SalesmanGoods>();
            string strWhere = string.Empty;
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            switch (pageType)
            {
                case (int)TmpType.小程序专业模板:
                    strWhere = $"aid={appId} and state=1 and goodtype={(int)EntGoodsType.普通产品}";
                    if (!string.IsNullOrEmpty(goodsName.Trim()))
                    {
                        strWhere += $" and name like @name ";
                        parameters.Add(new MySqlParameter("name", $"%{goodsName}%"));
                    }

                    if (isDistribution > -1)//表示选择了是否参与分销的状态 -1表示选择全部产品
                    {
                        strWhere += $" and isDistribution=@isDistribution ";
                        parameters.Add(new MySqlParameter("isDistribution", $"{isDistribution}"));
                    }
                    SalesManConfig salesManConfig = this.GetModel($"appId={appId}");
                    int is_show_cps_type = 0;
                    double cps_rate = 0.00;
                    if (salesManConfig != null)
                    {
                        if (!string.IsNullOrEmpty(salesManConfig.configStr))
                        {
                            ConfigModel configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);
                            if (configModel != null)
                            {
                                is_show_cps_type = configModel.pageShowWay.is_show_cps_type;
                                cps_rate = configModel.payMentManager.cps_rate;//默认佣金比例
                            }
                               
                        }
                    }
                    List<EntGoods> listEntGoods = EntGoodsBLL.SingleModel.GetListByParam(strWhere, parameters.ToArray(), pageSize, pageIndex, "*", sortWhere);
                    foreach (var item in listEntGoods)
                    {
                        

                        listSalesmanGoods.Add(new SalesmanGoods
                        {
                            goodsId = item.id,
                            goodsName = item.name,
                            goodsImg = item.img,
                            goodsPrice = item.price,
                            goodsStock = item.stock,
                            stockLimit = item.stockLimit,
                            goodsState = item.tag,
                            cps_rate = (item.isDefaultCps_Rate==0?cps_rate:item.cps_rate),
                            isDefaultCps_Rate = item.isDefaultCps_Rate,
                            isDistribution = item.isDistribution,
                            distributionTime = item.distributionTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            is_show_cps_type = is_show_cps_type,
                            cps_Money=Convert.ToDouble( 0.01*(item.isDefaultCps_Rate == 0 ? cps_rate*item.price : item.cps_rate* item.price)).ToString("0.00"),
                            salesCount=item.salesCount+item.virtualSalesCount
                        });
                    }

                    break;
            }
            
            return listSalesmanGoods;
        }

        /// <summary>
        /// 获取分销产品条数
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="goodsName"></param>
        /// <param name="isDistribution"></param>
        /// <returns></returns>
        public int GetSalesmanGoodsCount(int appId, int pageType, string goodsName = "", int isDistribution = -1)
        {
            string strWhere = string.Empty;
            int totalCount = 0;
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            switch (pageType)
            {
                case (int)TmpType.小程序专业模板:
                    strWhere = $"aid={appId} and state=1 and goodtype={(int)EntGoodsType.普通产品}";
                    if (!string.IsNullOrEmpty(goodsName.Trim()))
                    {
                        strWhere += $" and name like @name ";
                        parameters.Add(new MySqlParameter("name", $"%{goodsName}%"));
                    }

                    if (isDistribution > -1)//表示选择了是否参与分销的状态 -1表示选择全部产品
                    {
                        strWhere += $" and isDistribution=@isDistribution ";
                        parameters.Add(new MySqlParameter("isDistribution", $"{isDistribution}"));
                    }

                    totalCount = EntGoodsBLL.SingleModel.GetCount(strWhere, parameters.ToArray());

                    break;
            }

            return totalCount;
        }


    }
}
