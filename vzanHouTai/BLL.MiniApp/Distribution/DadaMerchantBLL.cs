using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Utility;

namespace BLL.MiniApp
{
    public class DadaMerchantBLL : BaseMySql<DadaMerchant>
    {
        #region 单例模式
        private static DadaMerchantBLL _singleModel;
        private static readonly object SynObject = new object();

        private DadaMerchantBLL()
        {

        }

        public static DadaMerchantBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DadaMerchantBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public DadaMerchant GetModelByMId(string merchantid)
        {
            return GetModel($"sourceid='{merchantid}'");
        }

        public DadaMerchant GetModelByRId(int rid,int storeid=0)
        {
            DadaRelation relation = DadaRelationBLL.SingleModel.GetModelByRid(rid,storeid);
            if (relation == null)
            {
                return new DadaMerchant();
            }

            DadaMerchant merchant = GetModel(relation.merchantid);

            return merchant;
        }

        public bool AddDadaMerchant(DadaMerchant model,int aid,int storeid=0)
        {
            
            
            TransactionModel tran = new TransactionModel();
            DadaMerchant merchantmodel = new DadaMerchant();
            merchantmodel.city_name = model.city_name;
            merchantmodel.enterprise_name = model.enterprise_name;
            merchantmodel.mobile = model.mobile;
            merchantmodel.sourceid = model.sourceid.ToString();
            tran.Add(base.BuildAddSql(merchantmodel));

            DadaRelation dadarelationmodel = new DadaRelation();
            dadarelationmodel.addtime = DateTime.Now;
            dadarelationmodel.rid = aid;
            dadarelationmodel.StoreId = storeid;
            tran.Add(Utils.BuildAddSqlS(dadarelationmodel, "merchantid", "(select LAST_INSERT_ID())", DadaRelationBLL.SingleModel.TableName, DadaRelationBLL.SingleModel.arrProperty));

            DadaShop dadashopmodel = new DadaShop();
            dadashopmodel.origin_shop_id = model.origin_shop_id;
            tran.Add(Utils.BuildAddSqlS(dadashopmodel, "merchantid", $"(select merchantid from DadaRelation where rid={aid} and storeid={storeid} LIMIT 1)", DadaShopBLL.SingleModel.TableName, DadaShopBLL.SingleModel.arrProperty));
            
            bool success = base.ExecuteTransaction(tran.sqlArray);

            return success;
        }

        public bool UpdateDadaMerchant(DadaMerchant model, ref string msg)
        {
            
            TransactionModel tran = new TransactionModel();
            DadaMerchant merchantmodel = base.GetModel(model.id);
            if (merchantmodel == null)
            {
                msg = "商户号数据失效了，请刷新重试";
                return false;
            }
            merchantmodel.city_name = model.city_name;
            merchantmodel.enterprise_name = model.enterprise_name;
            merchantmodel.mobile = model.mobile;
            merchantmodel.sourceid = model.sourceid.ToString();
            tran.Add(base.BuildUpdateSql(merchantmodel, "city_name,enterprise_name,mobile,sourceid"));

            DadaShop dadashopmodel = DadaShopBLL.SingleModel.GetModelByMId(model.id);
            if (dadashopmodel == null)
            {
                msg = "达达店铺数据失效了，请刷新重试";
                return false;
            }
            dadashopmodel.origin_shop_id = model.origin_shop_id;
            tran.Add(DadaShopBLL.SingleModel.BuildUpdateSql(dadashopmodel, "origin_shop_id"));
            bool success = base.ExecuteTransaction(tran.sqlArray);

            return success;
        }
    }
}