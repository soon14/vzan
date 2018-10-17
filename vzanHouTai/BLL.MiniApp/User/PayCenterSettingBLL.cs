//using BLL.MiniSNS.chat;
using DAL.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    public class PayCenterSettingBLL : BaseMySql<PayCenterSetting>
    {
        #region 单例模式
        private static PayCenterSettingBLL _singleModel;
        private static readonly object SynObject = new object();

        private PayCenterSettingBLL()
        {

        }

        public static PayCenterSettingBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PayCenterSettingBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PayCenterSetting GetPayCenterSettingByappid(string appid)
        {
            return GetModel($"Appid='{appid}'");
        }
        /// <summary>
        /// 根据绑定类型和绑定ID获取缓存的绑定信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="BingdingId"></param>
        /// <returns></returns>
        public string GetPayAppid(PayCenterSettingType type, int BingdingId)
        {
            var center = GetPayCenterSetting((int)type, BingdingId);
            if (null != center)
                return center.Appid;
            else
                return string.Empty;
        }
        /// <summary>
        /// 根据商户appid获取绑定信息
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public PayCenterSetting GetPayCenterSetting(string appid)
        {
            return  SingleModel.GetModel($"Appid='{appid}'");
        }
        /// <summary>
        /// 根据绑定类型和绑定ID获取缓存的绑定信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="BingdingId"></param>
        /// <returns></returns>
        public PayCenterSetting GetPayCenterSetting(int type, int BingdingId)
        {
            string key = string.Format(MemCacheKey.PayCenterSettingKey, type, BingdingId);
            PayCenterSetting setting = RedisUtil.Get<PayCenterSetting>(key);
            if (setting == null)
            {
                setting = GetModel($"BindingType={type} and BindingId={BingdingId}");
                if (setting == null)
                {
                    setting = new PayCenterSetting();
                }
                RedisUtil.Set<PayCenterSetting>(key, setting, TimeSpan.FromHours(12));
            }
            if (setting.Status == -1)
            {
                return new PayCenterSetting();
            }
            return setting;
        }
        public override object Add(PayCenterSetting model)
        {
            var obj = base.Add(model);
            RemoveCache(model);
            return obj;
        }
        public override bool Update(PayCenterSetting model)
        {
            var obj = base.Update(model);
            RemoveCache(model);
            return obj;
        }
        public override bool Update(PayCenterSetting model, string columnFields)
        {
            var obj = base.Update(model, columnFields);
            RemoveCache(model);
            return obj;
        }
        public void RemoveCache(PayCenterSetting model)
        {
            string key = string.Format(MemCacheKey.PayCenterSettingKey, model.BindingType, model.BindingId);
            RedisUtil.Remove(key);
            if (model.BindingType == -998)//清除提现账号列表和appidlist
            {
                key = string.Format(MemCacheKey.PayCenterSettingKey + "List", model.BindingType, model.BindingId);
                RedisUtil.Remove(key);
                key = "CompanyAppidList" + model.BindingType;
                RedisUtil.Remove(key);
            }
        }
        public override int Delete(int id)
        {
            PayCenterSetting model = GetModel(id);
            if (model != null)
            {
                string key = string.Format(MemCacheKey.PayCenterSettingKey, model.BindingType, model.BindingId);
                RedisUtil.Remove(key);
            }
            return base.Delete(id);
        }

        /// <summary>
        /// 根据产品类型获取多账号支付的列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<PayCenterSetting> GetCompanyPaySettingList(PayCenterSettingType type)
        {
            List<PayCenterSetting> list = RedisUtil.Get<List<PayCenterSetting>>(string.Format(MemCacheKey.PayCenterSettingKey + "List", (int)type, "-998"));
            if (list == null)
            {
                list = GetList($"BindingType='{(int)type}' and BindingId=-998");
                if (list == null)
                {
                    list = new List<PayCenterSetting>();
                }
                RedisUtil.Set<List<PayCenterSetting>>(string.Format(MemCacheKey.PayCenterSettingKey + "List", (int)type, "-998"), list, TimeSpan.FromHours(1));
            }
            return list;
        }

        /// <summary>
        /// 根据产品类型，用户ID，用户openid来随机获取一个提现公众号
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userid"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
          
        public string GetCompanyAppidList(PayCenterSettingType type)
        {
            string appidlist = RedisUtil.Get<string>("CompanyAppidList" + (int)type);
            if (appidlist != null && appidlist.Length > 10)
            {
                return appidlist;
            }
            List<PayCenterSetting> list = GetCompanyPaySettingList(type);
            if (list == null || list.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                list = list.FindAll(i => i.Appid != WebSiteConfig.WxAppId);
                if (list == null || list.Count == 0)
                {
                    return string.Empty;
                }
                foreach (PayCenterSetting setting in list)
                {
                    appidlist += (setting.Appid + ",");
                }
                appidlist = appidlist.Substring(0, appidlist.Length - 1);
                RedisUtil.Set<string>("CompanyAppidList" + (int)type, appidlist, TimeSpan.FromHours(1));
                return appidlist;
            }
        }

        public bool UpdateBingding(PayCenterSetting setting)
        {
            var cloumns = "Appid,Mch_id,Key,Status,BindingId";
            PayCenterSetting settingold = GetModel(setting.Id);
            if (settingold == null || settingold.Id == 0)
            {
                return false;
            }
            //在修改绑定的时候要确定是APPID是否是一样的，如果不一样，则要确定表里面存在这个APPID的配置（修改绑定之后用户的退款，提现都要需要用到）
            //一样的appid,直接update
            if (settingold.Appid == setting.Appid)
            {
                return Update(setting, cloumns);
            }
            //不一样，要查询一下是否存在该appid的备份。id=-9999的是备份的
            PayCenterSetting settingbak = GetModel($"Appid='{settingold.Appid}' and BindingId=-9999");
            if (settingbak == null)
            {
                settingold.BindingId = -9999;
                Update(settingold, cloumns);
            }
            setting.Id = 0;
            RemoveCache(setting);
            return Convert.ToInt32(Add(setting)) > 1;
        }
    }
}
