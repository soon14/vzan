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
    public class DadaShopBLL : BaseMySql<DadaShop>
    {
        #region 单例模式
        private static DadaShopBLL _singleModel;
        private static readonly object SynObject = new object();

        private DadaShopBLL()
        {

        }

        public static DadaShopBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DadaShopBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public DadaShop GetModelByShopNo(string shopno)
        {
            return GetModel($"origin_shop_id='{shopno}'");
        }

        public DadaShop GetModelByMId(int merchantid)
        {
            return GetModel($"merchantid={merchantid}");
        }

    }
}