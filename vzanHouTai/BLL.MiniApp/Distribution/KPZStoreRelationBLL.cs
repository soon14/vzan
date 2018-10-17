using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Utility;

namespace BLL.MiniApp
{
    public class KPZStoreRelationBLL : BaseMySql<KPZStoreRelation>
    {
        #region 单例模式
        private static KPZStoreRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private KPZStoreRelationBLL()
        {

        }

        public static KPZStoreRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new KPZStoreRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public KPZStoreRelation GetModelBySidAndAid(int aid,int storeid)
        {
            string sqlwhere = $"aid={aid} and storeid={storeid} and state=0";
            return base.GetModel(sqlwhere);
        }

        /// <summary>
        /// 建立快跑者与系统店铺关联
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeid"></param>
        /// <param name="teamtoken"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        public bool AddStore(int aid,int storeid,string teamtoken, string phone)
        {
            KPZStoreRelation model = GetModelBySidAndAid(aid, storeid);
            if(model==null || model.TeamToken!=teamtoken || model.TelePhone != phone)
            {
                if(model==null)
                {
                    model = new KPZStoreRelation();
                }
                else
                {
                    model.State = -1;
                    model.UpdateTime = DateTime.Now;
                    base.Update(model, "state,UpdateTime");
                }
               
                model.TeamToken = teamtoken;
                model.TelePhone = phone;
                model.AddTime = DateTime.Now;
                model.UpdateTime = DateTime.Now;
                model.StoreId = storeid;
                model.AId = aid;
                model.State = 0;
                base.Add(model);
            }
            
            return true;
        }
    }
}