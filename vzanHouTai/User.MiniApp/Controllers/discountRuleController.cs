using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.User;
using BLL.MiniApp;
using BLL.MiniApp.Tools;
using Entity.MiniApp.Tools;
using Utility.IO;
using User.MiniApp.Filters;
using BLL.MiniApp.Footbath;
using Entity.MiniApp.Fds;
using BLL.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Stores;
using BLL.MiniApp.Stores;
using Newtonsoft.Json;

namespace User.MiniApp.Controllers
{
    public class discountRuleController : baseController
    {
        

        private readonly int maxRulesCount = 20;//最多可设定多少条

        public int _appId = Context.GetRequestInt("appId", 0);
        public int _storeId = Context.GetRequestInt("storeId", 0);
        public discountRuleController()
        {
            
        }

        /// <summary>
        /// 满减优惠
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult DiscountRuleManager()
        {
            int pageType = Context.GetRequestInt("pageType", 0);

            //根据aid,storeId读取不同模板下的开关配置
            string errorMsg = string.Empty;
            CommonSetting commonSetting = CommonSettingBLL.GetCommonSetting(_appId, ref _storeId, ref errorMsg);
            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                return Redirect($"/base/PageErrorMsg?code=500&Msg={errorMsg}"); //提示报错页
            }

            ViewBag.aId = ViewBag.appId = _appId;
            ViewBag.storeId = _storeId;
            ViewBag.pageType = pageType;
            ViewBag.maxRulesCount = maxRulesCount;
            return View(commonSetting);
        }

        /// <summary>
        /// 获取满减规则
        /// </summary> 
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult GetDiscountRule(int discountRuleId)
        {
            Return_Msg handingResult = new Return_Msg();
            handingResult.isok = true;
            handingResult.code = "0";
            handingResult.Msg = "成功查询";

            DiscountRule discountRules = DiscountRuleBLL.SingleModel.GetModel(discountRuleId);
            //找不到记录时
            if (discountRules == null)
            {
                handingResult.isok = false;
                handingResult.code = "1";
                handingResult.Msg = "找不到指定满减规则";

                return Json(handingResult, JsonRequestBehavior.AllowGet);
            }
            else
            {
                handingResult.dataObj = new
                {
                    discountRules = discountRules
                };
                return Json(handingResult, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 获取满减规则集合
        /// </summary> 
        /// <param name="appId"></param>
        /// <returns></returns>
        //[LoginFilter]
        public ActionResult GetDiscountRules(int appId, int storeId = 0)
        {
            Return_Msg handingResult = new Return_Msg();
            handingResult.isok = true;
            handingResult.code = "0";
            handingResult.Msg = "成功查询";

            List<DiscountRule> discountRules = DiscountRuleBLL.SingleModel.GetListByAId(appId, storeId);

            handingResult.dataObj = new
            {
                discountRules = discountRules,
                recordCount = discountRules.Count
            };
            return Json(handingResult, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 编辑满减规则
        /// </summary> 
        /// <param name="appId"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult EditDiscountRules(DiscountRule curDiscountRule)
        {
            Return_Msg handingResult = new Return_Msg();
            handingResult.isok = true;
            handingResult.code = "0";
            handingResult.Msg = "保存成功";

            if (curDiscountRule.meetMoney <= 0 || curDiscountRule.meetMoney > 99999.99f)
            {
                handingResult.isok = false;
                handingResult.code = "300";
                handingResult.Msg = "满足金额请设定 0.01 ~ 99999.99 区间的值";
            }
            if (curDiscountRule.discountMoney <= 0 || curDiscountRule.discountMoney > 99999.99f)
            {
                handingResult.isok = false;
                handingResult.code = "301";
                handingResult.Msg = "优惠额请设定 0.01 ~ 99999.99 区间的值";
            }
            if (curDiscountRule.meetMoney < curDiscountRule.discountMoney)
            {
                handingResult.isok = false;
                handingResult.code = "302";
                handingResult.Msg = "优惠金额不可设定大于满足金额.";
            }

            //数据库的满减规则
            List<DiscountRule> dbDiscountRules = DiscountRuleBLL.SingleModel.GetListByAId(curDiscountRule.aId);
            //是否有重复的规则
            bool havRepe = dbDiscountRules.Any(d =>
                d.id != curDiscountRule.id
                    && d.meetMoney == curDiscountRule.meetMoney
                    && d.discountMoney == curDiscountRule.discountMoney
            );
            if (havRepe)
            {
                handingResult.isok = false;
                handingResult.code = "303";
                handingResult.Msg = "已有重复的规则.";
            }

            //总数不可超过 N 条
            int ruleCount = dbDiscountRules.Count(d =>
                d.id != curDiscountRule.id
            );
            if (ruleCount >= maxRulesCount)
            {
                handingResult.isok = false;
                handingResult.code = "304";
                handingResult.Msg = $"规则数量已达到上限{maxRulesCount}条.";
            }

            //编辑
            if (curDiscountRule.id > 0)
            {
                DiscountRule dbDiscountRule = DiscountRuleBLL.SingleModel.GetModel(curDiscountRule.id);
                if (dbDiscountRule == null)
                {
                    handingResult.isok = false;
                    handingResult.code = "1";
                    handingResult.Msg = "找不到指定的规则记录";

                    return Json(handingResult, JsonRequestBehavior.AllowGet);
                }

                dbDiscountRule.meetMoney = curDiscountRule.meetMoney;
                dbDiscountRule.discountMoney = curDiscountRule.discountMoney;
                dbDiscountRule.updateDate = DateTime.Now;

                bool isUpdateSuccess = DiscountRuleBLL.SingleModel.Update(dbDiscountRule, "meetMoney,discountMoney,updateDate");
                if (!isUpdateSuccess)
                {
                    handingResult.isok = false;
                    handingResult.code = "2";
                    handingResult.Msg = "修改失败";

                    return Json(handingResult, JsonRequestBehavior.AllowGet);
                }
            }
            //添加
            else
            {
                curDiscountRule.state = 0;
                curDiscountRule.createDate = curDiscountRule.updateDate = DateTime.Now;

                int id = Convert.ToInt32(DiscountRuleBLL.SingleModel.Add(curDiscountRule));
                if (id <= 0)
                {
                    handingResult.isok = false;
                    handingResult.code = "3";
                    handingResult.Msg = "保存失败";

                    return Json(handingResult, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(handingResult, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 删除满减规则
        /// </summary> 
        /// <param name="appId"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult DeleteDiscountRules(int id, int appId, int storeId = 0)
        {
            Return_Msg handingResult = new Return_Msg();
            handingResult.isok = true;
            handingResult.code = "0";
            handingResult.Msg = "删除成功";

            DiscountRule discountRule = DiscountRuleBLL.SingleModel.GetModel(id);
            discountRule.state = -1;
            discountRule.updateDate = DateTime.Now;

            bool isUpdateSuccess = DiscountRuleBLL.SingleModel.Update(discountRule, "state,updateDate");
            if (!isUpdateSuccess)
            {
                handingResult.isok = false;
                handingResult.code = "-1";
                handingResult.Msg = "修改失败,请重试!";
            }
            return Json(handingResult, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 修改满减规则开关设置
        /// </summary> 
        /// <param name="appId"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult ModifyMeetMoneySwitchState(int appId, int storeId = 0,bool discountRuleSwitch = false)
        {
            Return_Msg handingResult = new Return_Msg();
            handingResult.isok = true;
            handingResult.code = "0";
            handingResult.Msg = discountRuleSwitch ? "已开启" : "已关闭";

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (xcxrelation == null)
            {
                handingResult.code = "1";
                handingResult.Msg = "未找到小程序授权资料";
                return Json(handingResult, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxTemplate == null)
            {
                handingResult.code = "2";
                handingResult.Msg = "未找到小程序的模板";
                return Json(handingResult, JsonRequestBehavior.AllowGet);
            }

            bool isUpdateSuccess = false;
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : "")} ");
                    if (store_Food == null)
                    {
                        handingResult.code = "3";
                        handingResult.Msg = "还未开通店铺";
                        return Json(handingResult, JsonRequestBehavior.AllowGet);
                    }

                    //修改设定
                    FoodConfigModel foodConfig = store_Food.funJoinModel;
                    foodConfig.discountRuleSwitch = discountRuleSwitch;

                    store_Food.configJson = JsonConvert.SerializeObject(foodConfig);
                    store_Food.UpdateDate = DateTime.Now;

                    isUpdateSuccess = FoodBLL.SingleModel.Update(store_Food, "configJson,UpdateDate");
                    break;

                case (int)TmpType.小程序多门店模板:
                    FootBath store_MultiStore = FootBathBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : " and HomeId = 0 ")}");
                    if (store_MultiStore == null)
                    {
                        handingResult.code = "3";
                        handingResult.Msg = "还未开通店铺";
                        return Json(handingResult, JsonRequestBehavior.AllowGet);
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
                    switchModel.discountRuleSwitch = discountRuleSwitch;

                    //修改设定
                    store_MultiStore.SwitchConfig = JsonConvert.SerializeObject(switchModel);
                    store_MultiStore.UpdateDate = DateTime.Now;
                    isUpdateSuccess = FootBathBLL.SingleModel.Update(store_MultiStore, "SwitchConfig,UpdateDate");
                    break;
                default:
                    handingResult.code = "3";
                    handingResult.Msg = "还未开通店铺";
                    return Json(handingResult, JsonRequestBehavior.AllowGet);
            }
            
            if (!isUpdateSuccess)
            {
                handingResult.isok = false;
                handingResult.code = "-1";
                handingResult.Msg = "修改失败,请重试!";
            }
            return Json(handingResult, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 修改用户首单立减金额设定
        /// </summary> 
        /// <param name="appId"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult ModifyFirstOrderDiscountMoneySetting(int appId, int storeId = 0, float newUserFirstOrderDiscountMoney = 0.00f,float userFirstOrderDiscountMoney = 0.00f)
        {
            Return_Msg handingResult = new Return_Msg();
            handingResult.isok = true;
            handingResult.code = "0";
            handingResult.Msg = "保存成功";

            if (newUserFirstOrderDiscountMoney < 0 || newUserFirstOrderDiscountMoney > 99999.99)
            {
                handingResult.code = "201";
                handingResult.Msg = "新用户首单立减金额请设定 0.00 ~ 99999.99 区间的值";
                return Json(handingResult, JsonRequestBehavior.AllowGet);
            }
            if (userFirstOrderDiscountMoney < 0 || userFirstOrderDiscountMoney > 99999.99)
            {
                handingResult.code = "202";
                handingResult.Msg = "用户首单立减金额请设定 0.00 ~ 99999.99 区间的值";
                return Json(handingResult, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (xcxrelation == null)
            {
                handingResult.code = "1";
                handingResult.Msg = "未找到小程序授权资料";
                return Json(handingResult, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxTemplate == null)
            {
                handingResult.code = "2";
                handingResult.Msg = "未找到小程序的模板";
                return Json(handingResult, JsonRequestBehavior.AllowGet);
            }

            bool isUpdateSuccess = false;
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : "")} ");
                    if (store_Food == null)
                    {
                        handingResult.code = "3";
                        handingResult.Msg = "还未开通店铺";
                        return Json(handingResult, JsonRequestBehavior.AllowGet);
                    }

                    //修改设定
                    FoodConfigModel foodConfig = store_Food.funJoinModel;
                    foodConfig.newUserFirstOrderDiscountMoney = newUserFirstOrderDiscountMoney;
                    foodConfig.userFirstOrderDiscountMoney = userFirstOrderDiscountMoney;

                    store_Food.configJson = JsonConvert.SerializeObject(foodConfig);
                    store_Food.UpdateDate = DateTime.Now;

                    isUpdateSuccess = FoodBLL.SingleModel.Update(store_Food, "configJson,UpdateDate");
                    break;

                case (int)TmpType.小程序多门店模板:
                    FootBath store_MultiStore = FootBathBLL.SingleModel.GetModel($" appId = {xcxrelation.Id} {(storeId > 0 ? $" and Id = {storeId}" : " and HomeId = 0 ")}");
                    if (store_MultiStore == null)
                    {
                        handingResult.code = "3";
                        handingResult.Msg = "还未开通店铺";
                        return Json(handingResult, JsonRequestBehavior.AllowGet);
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
                    //修改设定
                    switchModel.newUserFirstOrderDiscountMoney = newUserFirstOrderDiscountMoney;
                    switchModel.userFirstOrderDiscountMoney = userFirstOrderDiscountMoney;

                    store_MultiStore.SwitchConfig = JsonConvert.SerializeObject(switchModel);
                    store_MultiStore.UpdateDate = DateTime.Now;

                    isUpdateSuccess = FootBathBLL.SingleModel.Update(store_MultiStore, "SwitchConfig,UpdateDate");
                    break;
                default:
                    handingResult.code = "3";
                    handingResult.Msg = "还未开通店铺";
                    return Json(handingResult, JsonRequestBehavior.AllowGet);
            }

            if (!isUpdateSuccess)
            {
                handingResult.isok = false;
                handingResult.code = "-1";
                handingResult.Msg = "修改失败,请重试!";
            }
            return Json(handingResult, JsonRequestBehavior.AllowGet);
        }
    }
}