using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Stores;
using Entity.MiniApp;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Stores;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp
{
    public static class CommonSettingBLL
    {
        /// <summary>
        /// 根据传入aId及storeId去读取店铺资料配置,自动查出默认关联的店铺的配置
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId">店铺ID,可传0,传0默认查出关联的店铺</param>
        /// <param name="errorMsg"></param>
        /// <param name="CommonSetting">公共配置</param>
        /// <returns></returns>
        public static CommonSetting GetCommonSetting(int aId, ref int storeId, ref string errorMsg)
        {
            CommonSetting setting = new CommonSetting();
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aId);
            if (xcxrelation == null)
            {
                errorMsg = "未找到小程序授权资料";
                return setting;
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxTemplate == null)
            {
                errorMsg = "未找到小程序的模板";
                return setting;
            }
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : "")} ");
                    if (store_Food == null)
                    {
                        errorMsg = "还未开通店铺";
                        return setting;
                    }
                    storeId = store_Food.Id;

                    setting.discountRuleSwitch = store_Food.funJoinModel.discountRuleSwitch;
                    setting.newUserFirstOrderDiscountMoney = store_Food.funJoinModel.newUserFirstOrderDiscountMoney;
                    setting.userFirstOrderDiscountMoney = store_Food.funJoinModel.userFirstOrderDiscountMoney;
                    setting.sortQueueSwitch = store_Food.funJoinModel.sortQueueSwitch;
                    setting.sortNo_next = store_Food.funJoinModel.sortNo_next;
                    break;
                case (int)TmpType.小程序多门店模板:
                    FootBath store_MultiStore = FootBathBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : " and HomeId = 0 ")}");
                    if (store_MultiStore == null)
                    {
                        errorMsg = "还未开通店铺";
                        return setting;
                    }
                    storeId = store_MultiStore.Id;

                    //读取配置
                    SwitchModel switchModel = null;
                    try
                    {
                        switchModel = JsonConvert.DeserializeObject<SwitchModel>(store_MultiStore.SwitchConfig);
                    }
                    catch (Exception)
                    {
                        switchModel = new SwitchModel();
                    }
                    setting.discountRuleSwitch = switchModel.discountRuleSwitch;
                    setting.newUserFirstOrderDiscountMoney = switchModel.newUserFirstOrderDiscountMoney;
                    setting.userFirstOrderDiscountMoney = switchModel.userFirstOrderDiscountMoney;
                    break;
                case (int)TmpType.小程序电商模板:
                case (int)TmpType.小程序专业模板:
                    Store store = StoreBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : "")} ");
                    if (store == null)
                    {
                        errorMsg = "还未开通店铺";
                        return setting;
                    }
                    try
                    {
                        store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
                    }
                    catch (Exception)
                    {
                        store.funJoinModel = new StoreConfigModel();
                    }

                    storeId = store.Id;
                    setting.sortQueueSwitch = store.funJoinModel.sortQueueSwitch;
                    setting.sortNo_next = store.funJoinModel.sortNo_next;
                    break;
                default:
                    errorMsg = "还未开通店铺";
                    return setting;
            }
            return setting;
        }


        /// <summary>
        /// 将 CommonSetting 逆推更入数据库
        /// </summary>
        /// <param name="commonSetting"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public static Return_Msg UpdateCommonSetting(CommonSetting commonSetting,int aId, int storeId = 0)
        {
            Return_Msg handingResult = new Return_Msg();
            handingResult.isok = false;

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aId);
            if (xcxrelation == null)
            {
                handingResult.code = "1";
                handingResult.Msg = "未找到小程序授权资料";
                return handingResult;
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxTemplate == null)
            {
                handingResult.code = "2";
                handingResult.Msg = "未找到小程序的模板";
                return handingResult;
            }

            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : "")} ");
                    if (store_Food == null)
                    {
                        handingResult.code = "3";
                        handingResult.Msg = "还未开通店铺";
                        return handingResult;
                    }

                    //修改设定
                    FoodConfigModel foodConfig = store_Food.funJoinModel;
                    foodConfig.discountRuleSwitch = commonSetting.discountRuleSwitch;
                    foodConfig.newUserFirstOrderDiscountMoney = commonSetting.newUserFirstOrderDiscountMoney;
                    foodConfig.userFirstOrderDiscountMoney = commonSetting.userFirstOrderDiscountMoney;
                    foodConfig.sortQueueSwitch = commonSetting.sortQueueSwitch;
                    foodConfig.sortNo_next = commonSetting.sortNo_next;

                    store_Food.configJson = JsonConvert.SerializeObject(foodConfig);
                    store_Food.UpdateDate = DateTime.Now;

                    handingResult.isok = FoodBLL.SingleModel.Update(store_Food, "configJson,UpdateDate");
                    handingResult.Msg = handingResult.isok ? "配置变更成功" : "配置变更失败";
                    return handingResult;

                case (int)TmpType.小程序多门店模板:
                    FootBath store_MultiStore = FootBathBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : " and HomeId = 0 ")}");
                    if (store_MultiStore == null)
                    {
                        handingResult.code = "3";
                        handingResult.Msg = "还未开通店铺";
                        return handingResult;
                    }

                    //读取配置
                    SwitchModel switchModel = null;
                    try
                    {
                        switchModel = JsonConvert.DeserializeObject<SwitchModel>(store_MultiStore.SwitchConfig);
                    }
                    catch (Exception)
                    {
                        switchModel = new SwitchModel();
                    }
                    switchModel.discountRuleSwitch = commonSetting.discountRuleSwitch;
                    switchModel.newUserFirstOrderDiscountMoney = commonSetting.newUserFirstOrderDiscountMoney;
                    switchModel.userFirstOrderDiscountMoney = commonSetting.userFirstOrderDiscountMoney;

                    //修改设定
                    store_MultiStore.SwitchConfig = JsonConvert.SerializeObject(switchModel);
                    store_MultiStore.UpdateDate = DateTime.Now;
                    handingResult.isok = FootBathBLL.SingleModel.Update(store_MultiStore, "SwitchConfig,UpdateDate");
                    handingResult.Msg = handingResult.isok ? "配置变更成功" : "配置变更失败";
                    return handingResult;
                case (int)TmpType.小程序电商模板:
                case (int)TmpType.小程序专业模板:
                    Store store = StoreBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : "")} ");
                    if (store == null)
                    {
                        handingResult.code = "3";
                        handingResult.Msg = "还未开通店铺";
                        return handingResult;
                    }
                    try
                    {
                        store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
                    }
                    catch (Exception)
                    {
                        store.funJoinModel = new StoreConfigModel();
                    }

                    storeId = store.Id;
                    store.funJoinModel.sortQueueSwitch = commonSetting.sortQueueSwitch;
                    store.funJoinModel.sortNo_next = commonSetting.sortNo_next;
                    store.configJson = JsonConvert.SerializeObject(store.funJoinModel);

                    store.UpdateDate = DateTime.Now;
                    handingResult.isok = StoreBLL.SingleModel.Update(store, "configJson,UpdateDate");
                    handingResult.Msg = handingResult.isok ? "配置变更成功" : "配置变更失败";
                    return handingResult;
                default:
                    handingResult.code = "3";
                    handingResult.Msg = "还未开通店铺";
                    return handingResult;
            }
        }



        public static bool RemoveRedis(string key)
        {
             return DAL.Base.RedisUtil.Remove(key);
        }
    }

}
