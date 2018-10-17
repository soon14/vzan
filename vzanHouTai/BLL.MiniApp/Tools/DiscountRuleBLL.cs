using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Tools
{
    public class DiscountRuleBLL : BaseMySql<DiscountRule>
    {
        #region 单例模式
        private static DiscountRuleBLL _singleModel;
        private static readonly object SynObject = new object();

        private DiscountRuleBLL()
        {

        }

        public static DiscountRuleBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DiscountRuleBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// model_key  
        /// </summary>
        private static readonly string Redis_DiscountRule = "Miniapp_DiscountRule_{0}_{1}";

        /// <summary>
        /// model_version_key  
        /// </summary>
        private static readonly string Redis_DiscountRule_version = "Miniapp_DiscountRule_version_{0}";

        /// <summary>
        /// 根据消费金额得出当前appId,storeId得出最大优惠力度金额(单位:分)
        /// </summary>
        /// <param name="curOrderMoney"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public int getMaxDiscountMoney(int curOrderMoney, int aId, int storeId = 0)
        {
            //0元金额不再做处理
            if (curOrderMoney == 0)
            {
                return 0;
            }

            //最大优惠力度金额
            float maxDiscountMoney = 0;

            List<DiscountRule> discountRoles = GetListByAId(aId, storeId);
            //得出最大优惠力度金额
            foreach (DiscountRule curRole in discountRoles)
            {
                //如果当前金额满足满减金额,则规则有效
                if (curOrderMoney >= curRole.meetMoney)
                {
                    if (curRole.discountMoney > maxDiscountMoney)
                    {
                        maxDiscountMoney = curRole.discountMoney;
                    }
                }
            }

            return Convert.ToInt32(maxDiscountMoney * 100);
        }

        /// <summary>
        /// 获取当前订单用户可以得到首单立减优惠的金额（单位：分）
        /// </summary>
        /// <param name="curOrderMoney"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public int getFirstOrderDiscountMoney(int userId, int aId, int storeId, TmpType tmpType)
        {
            float discountMoneySum = 0f;//总的优惠金额
            float newUserFirstOrderDiscountMoney = 0f;//新用户首单立减
            float userFirstOrderDiscountMoney = 0f;//用户首单立减

            bool isNewUserFirstOrder = false; //新下单用户为新用户首单
            bool isTodayFirstOrder = false; //用户每天下的首单为用户首单

            #region 获取 新用户首单立减,用户首单立减 
            switch (tmpType)
            {
                case TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModel($" appId = {aId} {(storeId > 0 ? $" and Id = {storeId}" : "")} ");
                    if (store_Food == null)
                    {
                        return 0;
                    }

                    newUserFirstOrderDiscountMoney = store_Food.funJoinModel.newUserFirstOrderDiscountMoney;
                    userFirstOrderDiscountMoney = store_Food.funJoinModel.userFirstOrderDiscountMoney;
                    break;
                case TmpType.小程序多门店模板:
                    FootBath store_MultiStore = FootBathBLL.SingleModel.GetModel($" appId = {aId} {(storeId > 0 ? $" and Id = {storeId}" : " and HomeId = 0 ")}");
                    if (store_MultiStore == null)
                    {
                        return 0;
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

                    newUserFirstOrderDiscountMoney = switchModel.newUserFirstOrderDiscountMoney;
                    userFirstOrderDiscountMoney = switchModel.userFirstOrderDiscountMoney;
                    break;
                default:
                    return 0;
            }
            #endregion

            if (newUserFirstOrderDiscountMoney == 0f && userFirstOrderDiscountMoney == 0f)
            {
                return 0;
            }

            #region 查询 是否满足优惠条件
            switch (tmpType)
            {
                case TmpType.小程序餐饮模板:
                    
                    isNewUserFirstOrder = !FoodGoodsOrderBLL.SingleModel.Exists($" userId = {userId} and  storeId ={storeId} ");
                    isTodayFirstOrder = !FoodGoodsOrderBLL.SingleModel.Exists($" userId = {userId} and  storeId ={storeId} and CreateDate >= '{DateTime.Now.ToString("yyyy-MM-dd")}' ");
                    break;
                case TmpType.小程序多门店模板:
                    isNewUserFirstOrder = !EntGoodsOrderBLL.SingleModel.Exists($" userId = {userId} and aId = {aId} and  storeId ={storeId} ");
                    isTodayFirstOrder = !EntGoodsOrderBLL.SingleModel.Exists($" userId = {userId} and aId = {aId} and  storeId ={storeId} and CreateDate >= '{DateTime.Now.ToString("yyyy-MM-dd")}' ");
                    break;
                default:
                    return 0;
            }
            #endregion

            //二者不可累加,取最大值
            if (isNewUserFirstOrder && discountMoneySum < newUserFirstOrderDiscountMoney)
            {
                discountMoneySum = newUserFirstOrderDiscountMoney;
            }
            if (isTodayFirstOrder && discountMoneySum < userFirstOrderDiscountMoney)
            {
                discountMoneySum = userFirstOrderDiscountMoney;
            }

            return Convert.ToInt32(discountMoneySum * 100);
        }


        /// <summary>
        /// 查询aId下的优惠规则（加缓存）
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public List<DiscountRule> GetListByAId(int aId, int storeId = 0)
        {
            string model_key = string.Format(Redis_DiscountRule, aId, storeId);
            string version_key = string.Format(Redis_DiscountRule_version, aId);

            int version = RedisUtil.GetVersion(version_key);
            RedisModel<DiscountRule> redisModel_DiscountRule = RedisUtil.Get<RedisModel<DiscountRule>>(model_key);
            if (redisModel_DiscountRule == null || redisModel_DiscountRule.DataList == null
                    || redisModel_DiscountRule.DataList.Count <= 0 || redisModel_DiscountRule.DataVersion != version)
            {
                redisModel_DiscountRule = new RedisModel<DiscountRule>();

                List<DiscountRule> newModels = GetList($" aId = {aId} and State = 0 {(storeId > 0 ? $"and storeId = {storeId}" : "")} ");
                redisModel_DiscountRule.DataList = newModels;
                redisModel_DiscountRule.Count = newModels.Count;
                redisModel_DiscountRule.DataVersion = version;

                RedisUtil.Set<RedisModel<DiscountRule>>(model_key, redisModel_DiscountRule, TimeSpan.FromHours(12));
            }

            return redisModel_DiscountRule.DataList;
        }


        #region 底层方法加入清除缓存节点
        public override object Add(DiscountRule model)
        {
            object id = base.Add(model);
            RedisUtil.SetVersion(string.Format(Redis_DiscountRule_version, model?.aId));
            return id;
        }

        public override bool Update(DiscountRule model)
        {
            bool isSuccess = base.Update(model);
            RedisUtil.SetVersion(string.Format(Redis_DiscountRule_version, model?.aId));
            return isSuccess;
        }

        public override bool Update(DiscountRule model, string columnFields)
        {
            bool isSuccess = base.Update(model, columnFields);
            RedisUtil.SetVersion(string.Format(Redis_DiscountRule_version, model?.aId));
            return isSuccess;
        }

        //删除缓存
        public void RemoveRedis(int aId)
        {
            if (aId > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_DiscountRule_version, aId));
            }
        }
        #endregion

    }
}
