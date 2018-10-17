using BLL.MiniApp.Conf;
using BLL.MiniApp.PlatChild;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Qiye
{
    public class QiyeStoreBLL : BaseMySql<QiyeStore>
    {
        #region 单例模式
        private static QiyeStoreBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeStoreBLL()
        {

        }

        public static QiyeStoreBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeStoreBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public QiyeStore GetModelBycardid(int bindPlatAid, long myCardId)
        {
            return base.GetModel($"BindPlatAid={bindPlatAid} and MyCardId={myCardId} and State<>-1");
        }

        public QiyeStore GetModelByAId(int aid)
        {
            return base.GetModel($"aid={aid}");
        }
        
        public List<QiyeStore> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<QiyeStore>();

            return base.GetList($"id in ({ids})");
        }

        public int GetCountByAId(int aid)
        {
            string sqlwhere = $"BindPlatAid={aid} and state>=0";
            return base.GetCount(sqlwhere);
        }

        public List<QiyeStore> GetListByBindAids(string bindPlatAids)
        {
            if (string.IsNullOrEmpty(bindPlatAids))
                return new List<QiyeStore>();

            return base.GetList($"BindPlatAid in({bindPlatAids}) and State<>-1");
        }

        public List<QiyeStore> GetListByBindAid(string bindPlatAids)
        {
            if (string.IsNullOrEmpty(bindPlatAids))
                return new List<QiyeStore>();

            return base.GetList($"BindPlatAid in({bindPlatAids}) and State<>-1");
        }
        
        /// <summary>
        /// 运费模板设置
        /// </summary>
        /// <param name="model"></param>
        /// <param name="updateSet"></param>
        /// <returns></returns>
        public bool UpdateConfig(QiyeStore model, Dictionary<string, object> updateSet)
        {
            if (model == null || updateSet == null || updateSet.Count == 0)
            {
                return false;
            }

            QiyeStoreSwitchModel config = string.IsNullOrWhiteSpace(model.SwitchConfig) ? new QiyeStoreSwitchModel() : JsonConvert.DeserializeObject<QiyeStoreSwitchModel>(model.SwitchConfig);

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
        public QiyeStoreSwitchModel SetConfigValue(Dictionary<string, object> updateList, QiyeStoreSwitchModel config)
        {
            Type configType = typeof(QiyeStoreSwitchModel);
            foreach (var set in updateList)
            {
                object newValue = Convert.ChangeType(set.Value, configType.GetProperty(set.Key).PropertyType);
                configType.GetProperty(set.Key).SetValue(config, newValue, null);
            }
            return config;
        }
    }
}
