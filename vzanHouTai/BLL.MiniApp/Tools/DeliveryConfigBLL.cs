using Core.MiniApp.Common;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Tools
{
    public class DeliveryConfigBLL : BaseMySql<DeliveryConfig>
    {
        #region 单例模式
        private static DeliveryConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private DeliveryConfigBLL()
        {

        }

        public static DeliveryConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DeliveryConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public bool UpdateConfig(Bargain bargain, DeliveryConfigAttr configDetail)
        {
            return UpdateConfig(bargain.Id.ToString(), configDetail, DeliveryConfigType.砍价产品);
        }

        public bool UpdateConfig(XcxAppAccountRelation appInfo, bool enableDiscount, int discount)
        {
            return UpdateConfig(appInfo.Id.ToString(), new DeliveryConfigAttr { DiscountEnable = enableDiscount, Discount = discount }, DeliveryConfigType.运费模板);
        }

        public bool UpdateConfig(string bindIds, DeliveryConfigAttr attr, DeliveryConfigType configType)
        {
            List<DeliveryConfig> config = GetConfig(bindIds, configType);
            List<DeliveryConfig> newConfig = bindIds.ConvertToIntList(',')?
                                                    .Where(id => !config.Exists(item => item.BindId == id))
                                                    .Select(id => new DeliveryConfig() { BindId = id, Type = (int)configType })
                                                    .ToList();
            TransactionModel tran = new TransactionModel();
            config?.ForEach(item =>
            {
                item.SetAttrbute(attr);
                tran.Add(BuildUpdateSql(item, "attr"));
            });
            newConfig?.ForEach(item =>
            {
                item.SetAttrbute(attr);
                tran.Add(BuildAddSql(item));
            });
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        public DeliveryConfig GetConfig(Bargain bargain)
        {
            return GetConfig(bargain.Id, DeliveryConfigType.砍价产品);
        }

        public DeliveryConfig GetStoreConfig(int aid)
        {
            return GetConfig(aid, DeliveryConfigType.运费模板);
        }

        public DeliveryConfig GetConfig(int bindId, DeliveryConfigType configType)
        {
            return GetConfig(bindId.ToString(), configType).FirstOrDefault();
        }

        public List<DeliveryConfig> GetConfig(string bindIds, DeliveryConfigType configType)
        {
            string whereSql = BuildWhereSql(bindIds: bindIds, configType: configType);
            return GetList(whereSql);
        }

        public string BuildWhereSql(string bindIds = null, DeliveryConfigType? configType = null)
        {
            List<string> whereSql = new List<string>();
            if(!string.IsNullOrWhiteSpace(bindIds))
            {
                whereSql.Add($"BindId IN ({bindIds})");
            }
            if (configType.HasValue)
            {
                whereSql.Add($"Type = {(int)configType.Value}");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
