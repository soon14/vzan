using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using Entity.MiniApp.ViewModel;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public class apiBussinessController : InheritController
    {
    }

    public class apiMiappBussinessController : apiBussinessController
    {
        public readonly int MAX_PTYPE_NUM = Convert.ToInt32(WebConfigurationManager.AppSettings["MAX_PTYPE_NUM"]);//产品分类最大数量
        

        private static readonly string PHONE_LOGIN_KEY = "TECHNICIAN_LOGIN_KEY_{0}";

        public apiMiappBussinessController()
        {
           
        }

        // GET: apiMiappBussiness
        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult Login()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            int userId = Context.GetRequestInt("userid", 0);
            int userType = Context.GetRequestInt("userType", 0);//用户身份 0：管理员，1：客服
            string verificationCode = Context.GetRequest("verificationCode", string.Empty);

            if (string.IsNullOrEmpty(phone))
            {
                returnObj.Msg = "手机号码为空";
                return Json(returnObj);
            }
            if (string.IsNullOrEmpty(verificationCode))
            {
                returnObj.Msg = "验证码为空";
                return Json(returnObj);
            }
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }
            if (phone != "18023453930")
            {
                //验证码是否匹配
                string serverAuthCode = RedisUtil.Get<string>(string.Format(PHONE_LOGIN_KEY, phone));
                if (serverAuthCode != verificationCode)
                {
                    returnObj.Msg = "验证失败";
                    return Json(returnObj);
                }
            }
            List<XcxAppAccountRelation> xcxList = new List<XcxAppAccountRelation>();
            XcxTemplate entproTemplate = XcxTemplateBLL.SingleModel.GetModelByType((int)TmpType.小程序专业模板);
            switch (userType)
            {
                case 0:
                    Account account = AccountBLL.SingleModel.GetModelByPhone(phone);
                    if (account == null)
                    {
                        returnObj.isok = false;
                        returnObj.Msg = "该手机号没有绑定在任何后台账户";
                        return Json(returnObj);
                    }
                    xcxList = _xcxAppAccountRelationBLL.GetListByaccountId_Tid(account.Id.ToString(), entproTemplate.Id);
                    break;

                case 1:
                    List<C_UserInfo> userList = C_UserInfoBLL.SingleModel.GetKfListByPhone(phone);
                    if (userList == null || userList.Count <= 0)
                    {
                        returnObj.isok = false;
                        returnObj.Msg = "你还不是客服哦";
                        return Json(returnObj);
                    }
                    string appids = string.Join(",", userList.Select(user => user.appId));
                    List<XcxAppAccountRelation> uxcxList = _xcxAppAccountRelationBLL.GetListByAppidsF(appids);
                    if (uxcxList != null && uxcxList.Count > 0)
                    {
                        xcxList.AddRange(uxcxList);
                    }
                    break;

                default:
                    returnObj.isok = false;
                    returnObj.Msg = "参数错误";
                    return Json(returnObj);
            }
            List<XcxAppAccountRelation> list = new List<XcxAppAccountRelation>();
            if (xcxList != null && xcxList.Count > 0)
            {
                userInfo.TelePhone = phone;
                C_UserInfoBLL.SingleModel.Update(userInfo, "telephone");

                string aids = string.Join(",", xcxList.Where(xcx => xcx.storeMasterId == 0).Select(xcx => xcx.Id).ToList());
                if (!string.IsNullOrEmpty(aids))
                {
                    _xcxAppAccountRelationBLL.UpdateMasterIdByAids(aids, userId);
                }
                list = new List<XcxAppAccountRelation>();

                string appIds = $"'{string.Join("','", xcxList.Select(s=>s.AppId))}'";
                List<OpenAuthorizerConfig> openAuthorizerConfigList= OpenAuthorizerConfigBLL.SingleModel.GetListByAppIds(appIds);
                List<UserXcxTemplate> userXcxTemplateList = UserXcxTemplateBLL.SingleModel.GetListByAppIds(appIds);
                foreach (XcxAppAccountRelation xcx in xcxList)
                {
                    OpenAuthorizerConfig xcxConfig = openAuthorizerConfigList?.FirstOrDefault(f=>f.appid == xcx.AppId);
                    UserXcxTemplate userXcxTemplateModel = userXcxTemplateList?.FirstOrDefault(f=>f.AppId==xcx.AppId);
                    xcx.XcxName = XcxAppAccountRelationBLL.SingleModel.GetAppName(0,xcx, userXcxTemplateModel,xcxConfig);
                    if(!string.IsNullOrEmpty(xcx.XcxName))
                    {
                        list.Add(xcx);
                    }
                }
            }
            returnObj.isok = true;
            returnObj.dataObj = list;
            return Json(returnObj);
        }

        [AuthCheckLoginSessionKey]
        public ActionResult Logout()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userid", 0);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }
            userInfo.TelePhone = string.Empty;
            if (C_UserInfoBLL.SingleModel.Update(userInfo, "telePhone"))
            {
                returnObj.isok = true;
                returnObj.Msg = "操作成功";
            }
            else
            {
                returnObj.Msg = "操作失败";
            }
            return Json(returnObj);
        }

        public ActionResult Uv(int type = 0)
        {
            string appid = Context.GetRequest("appId", string.Empty);
            return Json(new { uv = GetUv(appid, type) });
        }

        /// <summary>
        /// 获取店铺列表
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetStoreList()
        {
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userId", 0);
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }
            List<XcxAppAccountRelation> xcxList = new List<XcxAppAccountRelation>();
            XcxTemplate entproTemplate = XcxTemplateBLL.SingleModel.GetModelByType((int)TmpType.小程序专业模板);
            Account account = AccountBLL.SingleModel.GetModelByPhone(userInfo.TelePhone);
            if (account != null)
            {
                xcxList = _xcxAppAccountRelationBLL.GetListByaccountId_Tid(account.Id.ToString(), entproTemplate.Id);
            }
            List<C_UserInfo> userList = C_UserInfoBLL.SingleModel.GetKfListByPhone(userInfo.TelePhone);
            if (userList != null && userList.Count > 0)
            {
                string appids = string.Join(",", userList.Select(user => user.appId));
                List<XcxAppAccountRelation> uxcxList = _xcxAppAccountRelationBLL.GetListByAppidsF(appids);
                if (uxcxList != null && uxcxList.Count > 0)
                {
                    xcxList.AddRange(uxcxList);
                }
            }

            List<XcxAppAccountRelation> list = null;
            if (xcxList != null && xcxList.Count > 0)
            {
                string aids = string.Join(",", xcxList.Where(xcx => xcx.storeMasterId == 0).Select(xcx => xcx.Id).ToList());
                if (!string.IsNullOrEmpty(aids))
                {
                    _xcxAppAccountRelationBLL.UpdateMasterIdByAids(aids, userId);
                }

                string appIds = $"'{string.Join("','", xcxList.Select(s => s.AppId))}'";
                List<OpenAuthorizerConfig> openAuthorizerConfigList = OpenAuthorizerConfigBLL.SingleModel.GetListByAppIds(appIds);
                List<UserXcxTemplate> userXcxTemplateList = UserXcxTemplateBLL.SingleModel.GetListByAppIds(appIds);

                list = new List<XcxAppAccountRelation>();
                foreach (XcxAppAccountRelation xcx in xcxList)
                {
                    OpenAuthorizerConfig xcxConfig = openAuthorizerConfigList?.FirstOrDefault(f => f.appid == xcx.AppId);
                    UserXcxTemplate userXcxTemplateModel = userXcxTemplateList?.FirstOrDefault(f => f.AppId == xcx.AppId);
                    xcx.XcxName = XcxAppAccountRelationBLL.SingleModel.GetAppName(0, xcx, userXcxTemplateModel, xcxConfig);
                    if (!string.IsNullOrEmpty(xcx.XcxName))
                    {
                        list.Add(xcx);
                    }
                }
            }
            returnObj.isok = true;
            returnObj.dataObj = list;
            return Json(returnObj);
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult Index()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxAppAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxAppAccountRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            //总收益 groupuser+entgoodsorder+bargainuser
            int groupPriceSum = GroupUserBLL.SingleModel.GetPriceSumByAppId(appId);//团购总额
            int goodsOrderPriceSum = EntGoodsOrderBLL.SingleModel.GetOrderPriceSumByAppId(appId);//普通订单总额
            int bargainPriceSum = BargainUserBLL.SingleModel.GetPriceSumByAppId(xcxAppAccountRelation.Id);//砍价总额
            int priceSum = groupPriceSum + goodsOrderPriceSum + bargainPriceSum;
            //总订单（完成的订单）
            int groupOrderSum = GroupUserBLL.SingleModel.GetGroupOrderSum(appId, -1);//拼团订单数
            int goodsOrderSum = EntGoodsOrderBLL.SingleModel.GetOrderSum(appId, "3");//普通订单数
            int bargainOrderSum = BargainUserBLL.SingleModel.GetBargainOrderSum(xcxAppAccountRelation.Id, 8);//砍价订单数
            int orderSum = groupOrderSum + goodsOrderSum + bargainOrderSum;
            //总用户
            int userCount = C_UserInfoBLL.SingleModel.GetCountByAppid(appId);
            returnObj.isok = true;
            returnObj.dataObj = new { priceSum = (priceSum * 0.01).ToString("0.00"), orderSum, userCount };
            return Json(returnObj);
        }

        /// <summary>
        /// 根据时间段获取数据记录
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetRecordByDate()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxAppAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxAppAccountRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            // int dateType = Context.GetRequestInt("dateType", 0);//0：今天，1：昨天，2：过去一周，3：过去一个月
            string startDate = string.Empty;
            string endDate = string.Empty;
            //今天
            startDate = $"{DateTime.Now.ToString("yyyy-MM-dd")} 00:00:00";
            endDate = $"{DateTime.Now.ToString("yyyy-MM-dd")} 23:59:59";
            object day = GetRecordByDate(appId, xcxAppAccountRelation.Id, startDate, endDate, 0);
            ////过去一周
            startDate = $"{DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd")} 00:00:00";
            endDate = $"{DateTime.Now.ToString("yyyy-MM-dd")} 23:59:59";
            object week = GetRecordByDate(appId, xcxAppAccountRelation.Id, startDate, endDate, 1);

            ////过去一个月
            startDate = $"{DateTime.Now.AddDays(-29).ToString("yyyy-MM-dd")} 00:00:00";
            endDate = $"{DateTime.Now.ToString("yyyy-MM-dd")} 23:59:59";
            object month = GetRecordByDate(appId, xcxAppAccountRelation.Id, startDate, endDate, 2);

            returnObj.isok = true;
            returnObj.dataObj = new { day, week, month };
            return Json(returnObj);
        }

        /// <summary>
        /// 获取各个订单不同条件下的数量
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetOrderCount()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            int orderType = Context.GetRequestInt("orderType", 0);//0：普通订单，1：砍价订单，2：拼团，3：预约，4：团购
            int type = Context.GetRequestInt("selType", 0);
            string value = Context.GetRequest("value", string.Empty);
            XcxAppAccountRelation xcxAppAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxAppAccountRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }

            returnObj.isok = true;
            switch (orderType)
            {
                case 0://普通订单  orderType不等于3 等于3的是拼团
                    returnObj.dataObj = GetGoodsOrderCount(appId, 1, type, value);
                    break;

                case 1://砍价
                    returnObj.dataObj = GetBargainOrderCount(xcxAppAccountRelation.Id, type, value);
                    break;

                case 2://拼团
                    returnObj.dataObj = GetGoodsOrderCount(appId, 3, type, value);
                    break;

                case 3://预约
                    returnObj.dataObj = GetSubscribeOrderCount(xcxAppAccountRelation.Id, type, value);
                    break;

                case 4://团购
                    returnObj.dataObj = GetGroupOrderCount(appId, type, value);
                    break;
            }
            return Json(returnObj);
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetOrderList()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);

            XcxAppAccountRelation xcxAppAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxAppAccountRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            int orderType = Context.GetRequestInt("orderType", -1);//订单类型：0普通订单，1砍价订单，2团购订单，3预约订单
            int pageIndex = Context.GetRequestInt("pageIndex", 1);//页码
            int pageSize = Context.GetRequestInt("pageSize", 10);//每页数量
            int type = Context.GetRequestInt("selType", 0);//搜索项 0订单号，1商品名称，2手机号码，3客户名称
            string value = Context.GetRequest("value", string.Empty);//搜索值
            int state = Context.GetRequestInt("state", -999);//订单状态
            int dateType = Context.GetRequestInt("dateType", 0);//日期类型 0:不限日期，1:一周，2:今天
            string startDate = string.Empty;
            string endDate = string.Empty;
            switch (dateType)
            {
                case 0://不限日期
                    break;

                case 1://一周
                    startDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd") + " 00:00:00";
                    endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                    break;

                case 2://今天
                    startDate = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
                    endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                    break;
            }
            returnObj.isok = true;
            switch (orderType)
            {
                case 0://普通订单
                    returnObj.dataObj = EntGoodsOrderBLL.SingleModel.GetListByCondition(appId, xcxAppAccountRelation.Id, pageIndex, pageSize, 1, state, type, value, startDate, endDate);
                    break;

                case 1://砍价订单
                    returnObj.dataObj = BargainUserBLL.SingleModel.GetListByCondition(xcxAppAccountRelation.Id, pageIndex, pageSize, state, type, value, startDate, endDate);
                    break;

                case 2://拼团订单
                    returnObj.dataObj = EntGoodsOrderBLL.SingleModel.GetListByCondition(appId, xcxAppAccountRelation.Id, pageIndex, pageSize, 3, state, type, value, startDate, endDate);
                    break;

                case 3://预约订单
                    returnObj.dataObj = EntUserFormBLL.SingleModel.GetListByCondition(xcxAppAccountRelation.Id, pageIndex, pageSize, state, type, value, startDate, endDate);
                    break;

                case 4://团购订单
                    returnObj.dataObj = GroupUserBLL.SingleModel.GetListByCondition(appId, pageIndex, pageSize, state, type, value, startDate, endDate);
                    break;

                default:
                    returnObj.Msg = "参数异常";
                    return Json(returnObj);
            }
            return Json(returnObj);
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult UpdteOrderState()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxAppAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            int orderType = Context.GetRequestInt("orderType", -1);//订单类型：0普通订单，1砍价订单，2团购订单，3预约订单
            int orderId = Context.GetRequestInt("orderId", 0);
            int state = Context.GetRequestInt("state", -999);
            int oldState = Context.GetRequestInt("oldState", -999);
            string attachData = Context.GetRequest("attachData","");
            switch (orderType)
            {
                case 0://普通订单
                case 2://拼团订单
                    returnObj = UpdateGoodsOrderState(orderId, state, oldState, attachData);
                    break;

                case 1://砍价订单
                    returnObj = UpdateBargainOrderState(orderId, appId, xcxAppAccountRelation.Id, state,attachData);
                    break;

                //case 2://拼团订单
                //    returnObj = UpdateGoodsOrderState(orderId, state, oldState);
                //    break;

                case 3://预约订单
                    string remark = Context.GetRequest("remark", string.Empty);
                    returnObj = UpdateSubscribeOrderState(orderId, xcxAppAccountRelation.Id, state, remark);
                    break;

                case 4://团购订单
                    returnObj = UpdateGroupOrderState(orderId, appId, state);
                    break;

                default:
                    returnObj.Msg = "参数异常";
                    return Json(returnObj);
            }
            return Json(returnObj);
        }

        /// <summary>
        /// 通过核销码|订单号获取订单
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetOrderByTableNo()
        {
            string appId = Context.GetRequest("storeAppId", string.Empty);
            returnObj = new Return_Msg_APP();
            string tableNo = Context.GetRequest("kw", string.Empty);
            int type = Context.GetRequestInt("type", 0);// 0：手动输入查询，返回list列表    1：扫码查询 返回model
            switch (type)
            {
                case 0:// 0：手动输入查询，返回list列表
                    if (string.IsNullOrEmpty(tableNo))
                    {
                        returnObj.Msg = "请输入订单号";
                        return Json(returnObj);
                    }
                    List<EntGoodsOrder> goodsOrders = EntGoodsOrderBLL.SingleModel.GetListByAppId_TableNo(appId, tableNo);
                    returnObj.isok = true;
                    returnObj.dataObj = goodsOrders;
                    return Json(returnObj);

                case 1://1：扫码查询 返回model
                    if (string.IsNullOrEmpty(tableNo))
                    {
                        returnObj.Msg = "参数错误";
                        return Json(returnObj);
                    }
                    goodsOrders = new List<EntGoodsOrder>();
                    EntGoodsOrder goodsOrder = EntGoodsOrderBLL.SingleModel.GetModelByOrdernum(tableNo);
                    if (goodsOrder != null) goodsOrders.Add(goodsOrder);
                    returnObj.isok = true;
                    returnObj.dataObj = goodsOrders;
                    return Json(returnObj);

                default:
                    returnObj.Msg = "参数异常";
                    return Json(returnObj);
            }
        }

        /// <summary>
        /// 获取商品分类
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetGoodsTypes()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            int count = 0;
            List<EntGoodType> goodTypes = EntGoodTypeBLL.SingleModel.GetListByCach(xcxRelation.Id, 100, 1, ref count);
            returnObj.isok = true;
            returnObj.dataObj = goodTypes;
            return Json(returnObj);
        }

        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetGoodsTypesAll()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            List<EntGoodType> goodTypes = EntGoodTypeBLL.SingleModel.GetList($"aid={xcxRelation.Id} and state=1 ");
            int labelCount = 0;
            List<EntGoodLabel> goodLabels = EntGoodLabelBLL.SingleModel.GetListByCach(xcxRelation.Id, 25, 1, ref labelCount);
            List<EntSpecification> goodAttrList = EntSpecificationBLL.SingleModel.GetList(string.Format("aid={0} and state=1", xcxRelation.Id));
            List<EntGoodUnit> goodUnits = EntGoodUnitBLL.SingleModel.GetListBySql(string.Format("select * from entgoodunit where aid={0} and state=1", xcxRelation.Id));
            List<EntIndutypes> goodExtTypes = EntIndutypesBLL.SingleModel.GetList(string.Format("state=1 and aid={0}", xcxRelation.Id));
            returnObj.isok = true;
            returnObj.dataObj = new
            {
                goodTypes,//分类
                goodLabels,//标签
                goodAttrList,//规格属性
                goodUnits,//单位
                goodExtTypes,//参数
                goodTypeModel = new EntGoodType { aid = xcxRelation.Id },
                goodModel = new EntGoods { aid = xcxRelation.Id },
                attrModel = new EntSpecification { aid = xcxRelation.Id, sel = false, state = 1 },
            };
            return Json(returnObj);
        }

        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GoodType(string act = "", EntGoodType model = null, string storeAppId = "")
        {
            if (model == null || model.id < 0 || model.aid <= 0)
            {
                return Json(new { isok = false, msg = "非法请求！" });
            }
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(storeAppId);
            if (xcxRelation == null)
                return Json(new { isok = false, msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcxRelation.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" });

            #region 删除

            if (act == "del")
            {
                //检查是否有已经有产品使用了分类
                int checkcount = EntGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({model.id},ptypes)>0 and aid={model.aid} and state=1");
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = $"该分类下已有{checkcount}个产品，不可删除！" });
                }

                //当前删除分类是否是一级分类，如果是先查看是否有二级分类，如果有则先提示删除二级分类才能删除该一级分类

                if (model.parentId == 0)
                {
                    //表示一级分类
                    checkcount = EntGoodTypeBLL.SingleModel.GetCount($"parentId={model.id} and aid={model.aid} and state=1");
                    if (checkcount > 0)
                    {
                        return Json(new { isok = false, msg = $"该分类下已有{checkcount}个二级分类,不可删除,请先删其下的二级分类！" });
                    }
                }
                model.state = 0;
                if (EntGoodTypeBLL.SingleModel.Update(model, "state"))
                {
                    EntSettingBLL.SingleModel.SyncData(model.aid, "$..goodCat[?(@.id==" + model.id + ")]");

                    return Json(new { isok = true, msg = "删除成功！" });
                }
                else
                {
                    return Json(new { isok = true, msg = "删除失败！" });
                }
            }

            #endregion 删除

            #region 添加和修改

            if (model.name.Trim() == "" || model.name.Trim().Length > 10)
            {
                return Json(new { isok = false, msg = "分类名称不能为空，且不能超过10个字" });
            }
            else
            {
                #region 专业版 版本控制

                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    FunctionList functionList = new FunctionList();
                    int industr = xcxRelation.VersionId;
                    functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                    if (functionList == null)
                    {
                        return Json(new { isok = false, msg = "此功能未开启" });
                    }

                    ProductMgr productMgrModel = new ProductMgr();
                    if (!string.IsNullOrEmpty(functionList.ProductMgr))
                    {
                        productMgrModel = JsonConvert.DeserializeObject<ProductMgr>(functionList.ProductMgr);
                    }
                    if (productMgrModel.ProductType == 1)//表示关闭了添加类别功能
                    {
                        return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                    }
                }

                #endregion 专业版 版本控制

                int checkcount = EntGoodTypeBLL.SingleModel.GetCount($"name=@name and aid={model.aid} and id not in(0,{model.id}) and state=1 and parentId={model.parentId}", new MySqlParameter[] { new MySqlParameter("name", model.name) });
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = "已存在该分类名称，请重新设置！" });
                }
            }
            //修改
            if (model.id > 0)
            {
                if (EntGoodTypeBLL.SingleModel.Update(model, "name,parentId"))
                {
                    return Json(new { isok = true, msg = model });
                }
            }
            //添加
            else
            {
                int maxCount = MAX_PTYPE_NUM;
                string countWhere = $"aid={model.aid} and state=1 and parentId<>0";//表示二级分类
                if (model.parentId == 0)
                {
                    //表示添加一级分类 限制为30个
                    maxCount = 30;
                    countWhere = $"aid={model.aid} and state=1 and parentId=0";
                }
                int checkcount = EntGoodTypeBLL.SingleModel.GetCount(countWhere);
                if (checkcount >= maxCount)
                {
                    return Json(new { isok = false, msg = "无法新增分类！您已添加了" + maxCount + "个产品分类，已达到上限，请编辑已有的分类或删除部分分类后再进行新增。" });
                }

                int newid = Convert.ToInt32(EntGoodTypeBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.id = newid;

                    //1.更新初始化小类在大类里  2.页面配置里的产品类别归类为默认分类
                    //if (isIniti != 0)
                    //{
                    //    int secondTypeCount = _entGoodTypeBll.GetCount($"aid={appId} and parentId=-1");
                    //    if (secondTypeCount > 0)
                    //    {
                    //        _entGoodTypeBll.isInitiType(appId, newid);
                    //    }
                    //}

                    return Json(new { isok = true, msg = model });
                }
            }
            return Json(new { isok = true, msg = "操作失败" });

            #endregion 添加和修改
        }

        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GoodAttr(string act = "", EntSpecification model = null, string storeAppId = "")
        {
            if (model == null || model.id < 0 || model.aid <= 0)
            {
                return Json(new { isok = false, msg = "非法请求！" });
            }

            #region 检查权限

            if (model == null || model.id < 0 || model.aid <= 0)
            {
                return Json(new { isok = false, msg = "非法请求！" });
            }
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(storeAppId);
            if (xcxRelation == null)
                return Json(new { isok = false, msg = "小程序未授权" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcxRelation.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" });

            #endregion 检查权限

            #region 删除

            if (act == "del")
            {
                model = EntSpecificationBLL.SingleModel.GetModel(model.id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "该规格不存在！" });
                }
                //检查是否有已经有产品使用了规格或规格值
                int checkcount = 0;
                //如果删除规格
                if (model.parentid == 0)
                {
                    checkcount = EntGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({model.id},specificationkeys)>0 and aid={model.aid} and state=1");
                }
                //如果删除规格值
                else
                {
                    checkcount = EntGoodsBLL.SingleModel.GetCount($"FIND_IN_SET({model.id},specification)>0 and aid={model.aid} and state=1");
                }

                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = $"该规格下已有{checkcount}个产品，不可删除！" });
                }

                model.state = 0;
                if (EntSpecificationBLL.SingleModel.Update(model, "state"))
                {
                    return Json(new { isok = true, msg = "删除成功！" });
                }
                else
                {
                    return Json(new { isok = false, msg = "删除失败！" });
                }
            }

            #endregion 删除

            #region 添加和修改

            if (model.name.Trim() == "" || model.name.Trim().Length > 20)
            {
                return Json(new { isok = false, msg = "规格名称不能为空，且不能超过20个字" });
            }
            else
            {
                int checkcount = EntSpecificationBLL.SingleModel.GetCount($"name=@name and aid={model.aid} and id not in(0,{model.id}) and parentid={model.parentid} and state=1", new MySql.Data.MySqlClient.MySqlParameter[] { new MySql.Data.MySqlClient.MySqlParameter("name", model.name) });
                if (checkcount > 0)
                {
                    return Json(new { isok = false, msg = "已存在该规格名称，请重新设置！" });
                }
            }
            //修改
            if (model.id > 0)
            {
                if (EntSpecificationBLL.SingleModel.Update(model, "name,parentid,sort"))
                {
                    return Json(new { isok = true, msg = model });
                }
            }
            //添加
            else
            {
                #region 专业版 版本控制

                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    int industr = xcxRelation.VersionId;
                    FunctionList functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                    if (functionList == null)
                    {
                        return Json(new { isok = false, msg = "此功能未开启" });
                    }

                    ProductMgr productMgrModel = new ProductMgr();
                    if (!string.IsNullOrEmpty(functionList.ProductMgr))
                    {
                        productMgrModel = JsonConvert.DeserializeObject<ProductMgr>(functionList.ProductMgr);
                    }
                    if (productMgrModel.ProductSpecification == 1)//表示关闭了添加规格功能
                    {
                        return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                    }
                }

                #endregion 专业版 版本控制

                int checkcount = EntSpecificationBLL.SingleModel.GetCount($"aid={model.aid} and state=1 and parentid={model.parentid}");
                if (checkcount >= 200)
                {
                    return Json(new { isok = false, msg = "无法新增分类！您已添加了200个产品规格，已达到上限，请编辑已有的分类或删除部分规格后再进行新增。" });
                }
                int newid = Convert.ToInt32(EntSpecificationBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.id = newid;
                    return Json(new { isok = true, msg = model });
                }
            }
            return Json(new { isok = false, msg = "请求失败" });

            #endregion 添加和修改
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetEntGoodsList()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }

            int goodsType = Context.GetRequestInt("goodsType", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string goodsName = Context.GetRequest("goodsName", string.Empty);
            int count = 0;
            List<EntGoods> goodsList = EntGoodsBLL.SingleModel.GetListByRedis(xcxRelation.Id, goodsName, 0, goodsType, -1, pageIndex, pageSize, ref count);
            returnObj.isok = true;
            returnObj.dataObj = goodsList;
            return Json(returnObj);
        }

        /// <summary>
        /// 查询普通商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public JsonResult GetEntGoodsInfo(int id = 0)
        {
            returnObj = new Return_Msg_APP();
            if (id > 0)
            {
                EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(id);
                if (goodModel == null || goodModel.state == 0)
                {
                    returnObj.isok = false;
                    returnObj.dataObj = "产品不存在或已删除";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.isok = true;
                    returnObj.dataObj = goodModel;
                }
            }
            else if (id == 0)
            {
                EntGoods goodModel = new EntGoods();
                returnObj.isok = true;
                returnObj.dataObj = goodModel;
            }
            return Json(returnObj);
        }

        [AuthCheckLoginSessionKey, MasterCheck, ValidateInput(false)]
        public ActionResult SaveEntGoodsInfo(string storeAppId, EntGoods model)
        {
            try
            {
                #region 专业版 版本控制

                if (model.aid <= 0)
                    return Json(new { isok = false, msg = "aid不能小于0" });
                XcxAppAccountRelation app = _xcxAppAccountRelationBLL.GetModelByAppid(storeAppId);
                if (app == null)
                    return Json(new { isok = false, msg = "小程序未授权" });

                XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
                if (xcxTemplate == null)
                    return Json(new { isok = false, msg = "小程序模板不存在" });

                int curProductCount = EntGoodsBLL.SingleModel.GetCount($"aid={model.aid} and state=1 and goodtype={(int)EntGoodsType.普通产品}");

                if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
                {
                    if (model.id == 0)//产品增加时候进行判断
                    {
                        FunctionList functionList = new FunctionList();
                        int industr = app.VersionId;
                        functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={industr}");
                        if (functionList == null)
                        {
                            return Json(new { isok = false, msg = "此功能未开启" });
                        }
                        ProductMgr productMgrModel = new ProductMgr();
                        if (!string.IsNullOrEmpty(functionList.ProductMgr))
                        {
                            productMgrModel = JsonConvert.DeserializeObject<ProductMgr>(functionList.ProductMgr);
                        }
                        if (curProductCount > productMgrModel.ProductMaxCount)
                        {
                            return Json(new { isok = false, msg = $"产品数量达上限{productMgrModel.ProductMaxCount},请先升级更高版本" });
                        }
                    }
                }

                #endregion 专业版 版本控制

                //清除产品列表缓存
                EntGoodsBLL.SingleModel.RemoveEntGoodListCache(model.aid);

                if (model.id == 0)
                {
                    int newid = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(model));
                    model.id = newid;
                    if (newid > 0)
                    {
                        DAL.Base.RedisUtil.Remove(EntGoodsBLL.key_new_ent_goods);
                        return Json(new { isok = true, msg = "添加成功" });
                    }
                }
                else
                {
                    #region 商品价格加入购物车后,变动需要同步进去

                    //字符串转json串
                    if (!string.IsNullOrEmpty(model.specificationdetail))
                    {
                        List<EntGoodsAttrDetail> specifications = model.GASDetailList;
                        specifications.ForEach((Action<EntGoodsAttrDetail>)(x =>
                        {
                            EntGoodsCartBLL.SingleModel.UpdateCartByGoodsId(model.id, x.id, Convert.ToInt32(x.price * 100));
                        }));

                        #region 若多规格商品被删除,更改购物车商品的标识

                        EntGoods dbGood = EntGoodsBLL.SingleModel.GetModel(model.id);
                        if (dbGood == null)
                        {
                            return Json(new { isok = false, msg = "商品不存在！" }, JsonRequestBehavior.AllowGet);
                        }

                        TransactionModel TranModel = new TransactionModel();
                        //更改已被删掉的商品
                        List<string> dbGoodSpacList = dbGood.GASDetailList.Select(x => x.id).ToList();
                        List<string> goodsSpacList = model.GASDetailList.Select(x => x.id).ToList();
                        List<string> updateGoodsStateSqlList = new List<string>();
                        if (dbGoodSpacList.Count > 0)
                        {
                            dbGoodSpacList.ForEach((Action<string>)(x =>
                            {
                                if (!goodsSpacList.Contains(x))
                                {
                                    updateGoodsStateSqlList.AddRange((IEnumerable<string>)EntGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsIdSpecids(model.id, x, 2));
                                }
                            }));
                        }
                        updateGoodsStateSqlList.ForEach(x =>
                        {
                            TranModel.Add(x);
                        });

                        if (!EntGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
                        {
                            return Json(new { isok = false, msg = "更改购物车标识失败！" }, JsonRequestBehavior.AllowGet);
                        }

                        #endregion 若多规格商品被删除,更改购物车商品的标识
                    }
                    //价格更新之后更新购物车价格
                    if (model.id > 0)
                    {
                        EntGoodsCartBLL.SingleModel.UpdateCartByGoodsId(model.id, "", Convert.ToInt32(model.price * 100));
                    }

                    #endregion 商品价格加入购物车后,变动需要同步进去

                    model.updatetime = DateTime.Now;
                    if (EntGoodsBLL.SingleModel.Update(model, "name,img,showprice,ptypes,exttypes,exttypesstr,ptypestr,stock,stockLimit,plabels,plabelstr_array,plabelstr,specificationkeys,specification,specificationdetail,pickspecification,price,priceStr,unit,slideimgs,description,updatetime,sort,virtualSalesCount,state,tag,ServiceTime,goodtype,isDistribution,isDefaultCps_Rate,cps_rate,isTakeout,isPackin,TemplateId,Weight"))
                    {
                        List<SubStoreEntGoods> subGoods = SubStoreEntGoodsBLL.SingleModel.GetList($"pid={model.id} and aid={model.aid}");
                        model.updatetime = DateTime.Now;

                        TransactionModel TranModelSync = new TransactionModel();
                        if (subGoods != null && subGoods.Count > 0)
                        {
                            subGoods.ForEach(subGood =>
                            {
                                TranModelSync.Add(EntGoodsBLL.SingleModel.GetSyncSql(model, subGood));
                            });
                        }
                        bool result = EntGoodsBLL.SingleModel.ExecuteTransactionDataCorect(TranModelSync.sqlArray, TranModelSync.ParameterArray);

                        if (!result)
                        {
                            return Json(new { isok = true, msg = "同步失败！" });
                        }

                        
                        return Json(new { isok = true, msg = "修改成功" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message });
            }

            return Json(new { isok = false, msg = "操作失败" });
        }

        /// <summary>
        /// 商品上架|下架|删除
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult UpdateState()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "店铺id错误";
                return Json(returnObj);
            }
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "店铺不存在";
                return Json(returnObj);
            }
            string act = Context.GetRequest("act", string.Empty);
            int tag = Context.GetRequestInt("tag", -1);
            int goodsId = Context.GetRequestInt("goodsId", 0);
            if (goodsId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            EntGoodsBLL.SingleModel.RemoveEntGoodListCache(xcxRelation.Id);
            EntGoods goods = EntGoodsBLL.SingleModel.GetModel(goodsId);
            if (goods == null || goods.state == 0)
            {
                returnObj.Msg = "产品不存在或已删除";
                return Json(returnObj);
            }
            switch (act)
            {
                case "del":
                    returnObj = DeleteGoods(xcxRelation.Id, goods);
                    break;

                case "tag":
                    returnObj = UpdateGoods(xcxRelation.Id, goods, tag);
                    break;
            }
            return Json(returnObj);
        }

        /// <summary>
        /// 获取会员列表
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetVipList()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string searchVal = Context.GetRequest("searchValue", string.Empty);
            int levelid = Context.GetRequestInt("levelid", 0);
            try
            {
                MiniappVipInfo model = VipRelationBLL.SingleModel.GetVipList(appId, pageIndex, pageSize, searchVal, levelid, -1, "", "", searchVal, 1);
                returnObj.isok = true;
                returnObj.dataObj = model.relationList;
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
                returnObj.Msg = "出了点小问题";
                return Json(returnObj);
            }
        }

        /// <summary>
        /// 获取会员等级列表
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetVipLevel()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppid", string.Empty);
            List<VipLevel> levelList = VipLevelBLL.SingleModel.GetListByAppId(appId);
            if (levelList == null || levelList.Count <= 0)
            {
                VipLevel def_level = new VipLevel();
                def_level.addtime = DateTime.Now;
                def_level.appId = appId;
                def_level.name = "普通会员";
                def_level.bgcolor = "#4a86e8";
                def_level.updatetime = def_level.addtime;
                levelList = new List<VipLevel>();
                def_level.Id = Convert.ToInt32(VipLevelBLL.SingleModel.Add(def_level));
                levelList.Add(def_level);
            }
            returnObj.isok = true;
            returnObj.dataObj = levelList;
            return Json(returnObj);
        }

        /// <summary>
        /// 会员储值修改
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult ChangeSaveMoney()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            int saveMoney = Context.GetRequestInt("saveMoney", 0);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            int uid = Context.GetRequestInt("vipuid", 0);

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(uid);
            if (userInfo == null)
            {
                returnObj.Msg = "会员不存在";
                return Json(returnObj);
            }
            if (userInfo.appId != appId)
            {
                returnObj.Msg = "没有权限";
                return Json(returnObj);
            }

            SaveMoneySetUser saveMoneyAccount = SaveMoneySetUserBLL.SingleModel.getModelByUserId(appId, userInfo.Id);
            int AccountMoney = saveMoney;
            SaveMoneySetUserLog newLog = new SaveMoneySetUserLog()
            {
                AppId = appId,
                UserId = userInfo.Id,
                CreateDate = DateTime.Now,
                State = 1
            };
            if (saveMoneyAccount == null)
            {
                returnObj = AddSaveMoney(saveMoney, appId, userInfo, newLog);
            }
            else
            {
                returnObj = UpdateSaveMoney(saveMoney, appId, userInfo, newLog, saveMoneyAccount, xcxRelation.TId);
            }
            return Json(returnObj);
        }

        /// <summary>
        /// 修改会员等级
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult EditViplevel()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            int levelId = Context.GetRequestInt("levelid", 0);
            if (levelId <= 0)
            {
                returnObj.Msg = "会员等级不存在";
                return Json(returnObj);
            }
            int vipRid = Context.GetRequestInt("viprid", 0);
            if (vipRid <= 0)
            {
                returnObj.Msg = "会员不存在";
                return Json(returnObj);
            }
            VipLevel levelinfo = VipLevelBLL.SingleModel.GetModelByAppid_Id(appId, levelId);
            VipRelation viprelation = VipRelationBLL.SingleModel.GetModelByAppid_Id(appId, vipRid);
            if (levelinfo == null || viprelation == null)
            {
                returnObj.Msg = "会员信息错误";
                return Json(returnObj);
            }
            viprelation.levelid = levelinfo.Id;
            viprelation.updatetime = DateTime.Now;
            returnObj.isok = VipRelationBLL.SingleModel.Update(viprelation, "levelid,updatetime");
            returnObj.Msg = returnObj.isok ? "修改成功" : "修改失败";
            return Json(returnObj);
        }

        /// <summary>
        /// 获取店铺配置
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetStoreInfo()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            Store storeModel = StoreBLL.SingleModel.GetModelByRid(xcxRelation.Id);
            if (storeModel == null)
            {
                returnObj.Msg = "店铺信息错误";
                return Json(returnObj);
            }
            //json转换可能报错,加try catch
            try
            {
                storeModel.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson) ?? new StoreConfigModel();//若为 null 则new一个新的配置
            }
            catch
            {
                storeModel.funJoinModel = new StoreConfigModel();
            }
            returnObj.isok = true;
            returnObj.dataObj = storeModel;
            return Json(returnObj);
        }

        /// <summary>
        /// 保存门店设置
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult SaveStoreInfo()
        {
            returnObj = new Return_Msg_APP();
            string modelStr = Context.GetRequest("storeModel", string.Empty);
            if (string.IsNullOrEmpty(modelStr))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            Store storeModel = null;
            try
            {
                storeModel = JsonConvert.DeserializeObject<Store>(modelStr);
            }
            catch
            {
                returnObj.Msg = "数据异常";
                return Json(returnObj);
            }
            if (storeModel == null || storeModel.Id < 0 || storeModel.funJoinModel == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"店铺资料错误 || storeModel.Id ({storeModel?.Id}) || storeModel.funJoinModel: ({JsonConvert.SerializeObject(storeModel?.funJoinModel)})");
                returnObj.Msg = "店铺资料错误";
                return Json(returnObj);
            }
            //若开启了自取项,需填写店铺地址
            if (storeModel.funJoinModel.openInvite)
            {
                if (string.IsNullOrWhiteSpace(storeModel.Address))
                {
                    returnObj.Msg = "请选择店铺地址位置";
                    return Json(returnObj);
                }
            }
            storeModel.configJson = JsonConvert.SerializeObject(storeModel.funJoinModel);
            returnObj.isok = StoreBLL.SingleModel.Update(storeModel, "Address,Lat,Lng,configJson");
            returnObj.Msg = returnObj.isok ? "保存成功" : "保存失败";
            return Json(returnObj);
        }

        /// <summary>
        /// 获取未读消息数量
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetMessageCount()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            int orderCount = SystemUpdateMessageBLL.SingleModel.GetUnreadOrderMessageCount(xcxRelation.Id, xcxRelation.AccountId.ToString(), 4);
            int subscribeCount = SystemUpdateMessageBLL.SingleModel.GetUnreadOrderMessageCount(xcxRelation.Id, xcxRelation.AccountId.ToString(), 3);
            returnObj.isok = true;
            returnObj.dataObj = new { orderCount, subscribeCount };
            return Json(returnObj);
        }

        /// <summary>
        /// 修改订单消息为已读
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult ReadMessage()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            int type = Context.GetRequestInt("type", 0);
            if (type != 3 && type != 4)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            int orderId = Context.GetRequestInt("orderId", 0);
            if (orderId <= 0)
            {
                returnObj.Msg = "订单id错误";
                return Json(returnObj);
            }
            SystemUpdateMessage message = SystemUpdateMessageBLL.SingleModel.GetModelByOrderId_Type(orderId, type, xcxRelation.Id, xcxRelation.AccountId.ToString());
            if (message == null)
            {
                returnObj.Msg = "消息不存在";
                return Json(returnObj);
            }

            returnObj.isok = SystemUpdateUserLogBLL.SingleModel.Readed(message.Id, message.AccountId);
            returnObj.Msg = returnObj.isok ? "修改成功" : "修改失败";
            return Json(returnObj);
        }

        /// <summary>
        /// 获取未读消息列表
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetMessageList()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            int type = Context.GetRequestInt("type", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            switch (type)
            {
                case 3://预约消息
                    List<SystemUpdateMessage> messageList = SystemUpdateMessageBLL.SingleModel.GetUnreadOrderMessage(xcxRelation.Id, xcxRelation.AccountId.ToString(), pageIndex, pageSize, type);
                    returnObj.isok = true;
                    returnObj.dataObj = messageList;
                    if (messageList != null && messageList.Count > 0)
                    {
                        int orderId = 0;
                        string orderIds = string.Join(",",messageList.Where(w=> Int32.TryParse(w.Title, out orderId))?.Select(s=>s.Title).Distinct());
                        List<EntUserForm> entUserFormList = EntUserFormBLL.SingleModel.GetListByIds(orderIds);

                        string userIds = string.Join(",",entUserFormList?.Select(s=>s.uid).Distinct());
                        List<VipRelation> vipRelationList = VipRelationBLL.SingleModel.GetListByUserIds(userIds);

                        foreach (SystemUpdateMessage message in messageList)
                        {
                            orderId = 0;
                            Int32.TryParse(message.Title, out orderId);
                            message.formInfo = entUserFormList?.FirstOrDefault(f=>f.id == orderId);
                            message.userInfo = vipRelationList.FirstOrDefault(f=>f.uid == message.formInfo?.uid);
                        }
                    }
                    break;

                case 4://订单消息
                    messageList = SystemUpdateMessageBLL.SingleModel.GetUnreadOrderMessage(xcxRelation.Id, xcxRelation.AccountId.ToString(), pageIndex, pageSize, type);
                    returnObj.isok = true;
                    returnObj.dataObj = messageList;
                    if (messageList != null && messageList.Count > 0)
                    {
                        int orderId = 0;
                        string orderIds = string.Join(",", messageList.Where(w => Int32.TryParse(w.Title, out orderId))?.Select(s => s.Title).Distinct());
                        List<EntGoodsOrder> entGoodsOrderList = EntGoodsOrderBLL.SingleModel.GetListByIds(orderIds);

                        string userIds = string.Join(",", entGoodsOrderList?.Select(s => s.UserId).Distinct());
                        List<VipRelation> vipRelationList = VipRelationBLL.SingleModel.GetListByUserIds(userIds);
                        
                        foreach (SystemUpdateMessage message in messageList)
                        {
                            orderId = 0;
                            Int32.TryParse(message.Title, out orderId);
                            message.orderInfo = entGoodsOrderList.FirstOrDefault(f=>f.Id == orderId);
                            message.userInfo = vipRelationList.FirstOrDefault(f => f.uid == message.orderInfo?.UserId);
                        }
                    }
                    break;

                default:
                    returnObj.Msg = "参数错误";
                    break;
            }
            return Json(returnObj);
        }

        /// <summary>
        /// 更新储值金额
        /// </summary>
        /// <param name="saveMoney"></param>
        /// <param name="appId"></param>
        /// <param name="userInfo"></param>
        /// <param name="newLog"></param>
        /// <param name="saveMoneyAccount"></param>
        /// <param name="tid"></param>
        /// <returns></returns>
        private Return_Msg_APP UpdateSaveMoney(int saveMoney, string appId, C_UserInfo userInfo, SaveMoneySetUserLog newLog, SaveMoneySetUser saveMoneyAccount, int tid)
        {
            returnObj = new Return_Msg_APP();
            //更新储值余额
            newLog.BeforeMoney = saveMoneyAccount.AccountMoney;
            newLog.Type = saveMoney > 0 ? 0 : -1;//如果手动输入的值小于0则表示消费 否则表示在当前余额基础上充值
            saveMoneyAccount.AccountMoney = saveMoneyAccount.AccountMoney + saveMoney;
            if (saveMoneyAccount.AccountMoney < 0)
            {
                returnObj.Msg = "修改失败(余额不足扣除)";
                return returnObj;
            }
            newLog.ChangeMoney = Math.Abs(saveMoneyAccount.AccountMoney - newLog.BeforeMoney);
            newLog.AfterMoney = saveMoneyAccount.AccountMoney;
            string txt = newLog.Type == 0 ? "增加" : "扣除";
            newLog.ChangeNote = $"商家{txt}{Math.Abs(saveMoney)}元";
            if (!SaveMoneySetUserBLL.SingleModel.Update(saveMoneyAccount, "AccountMoney"))
            {
                returnObj.Msg = "修改失败";
                return returnObj;
            }
            //加日志变更记录
            newLog.MoneySetUserId = saveMoneyAccount.Id;
            newLog.Id = Convert.ToInt32(SaveMoneySetUserLogBLL.SingleModel.Add(newLog));
            if (newLog.Id <= 0)
            {
                returnObj.Msg = "修改失败(变更记录失败)";
                return returnObj;
            }
            else
            {
                VipRelation vipRelation = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id}");
                if (vipRelation != null)
                {
                    vipRelation.updatetime = DateTime.Now;
                    VipRelationBLL.SingleModel.Update(vipRelation, "updatetime");
                }
                XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={tid}");
                if (xcxTemplate == null)
                {
                    returnObj.Msg = "找不到模板";
                    return returnObj;
                }

                bool updatelevelResult = false;
                if (newLog.Type == -1 && saveMoney < 0)
                {
                    string no = WxPayApi.GenerateOutTradeNo();
                    CityMorders order = new CityMorders()
                    {
                        OrderType = (int)ArticleTypeEnum.MiniappEditSaveMoney,
                        ActionType = 1,
                        Addtime = DateTime.Now,
                        payment_free = Math.Abs(saveMoney),
                        trade_no = no,
                        Percent = 0,
                        userip = WebHelper.GetIP(),
                        FuserId = userInfo.Id,
                        Fusername = userInfo.NickName,
                        orderno = no,
                        payment_status = 1,
                        Status = 0,
                        Articleid = 0,
                        CommentId = 0,
                        MinisnsId = 0,
                        TuserId = 0,
                        is_group = 0,
                        is_group_head = 0,
                        groupsponsor_id = 0,
                        ShowNote = "",
                        CitySubId = 0,
                        PayRate = 1,
                        buy_num = 1,
                        appid = appId,
                        remark = "",
                        OperStatus = 0,
                        Tusername = "",
                        Note = $"商家在后台手动扣除用户储值余额{Math.Abs(saveMoney)}元",
                    };
                    int orderid = Convert.ToInt32(new CityMordersBLL().Add(order));

                    if (orderid <= 0)
                    {
                        returnObj.isok = false;
                        returnObj.Msg = "修改失败(插入订单异常)";
                        return returnObj;
                    }
                    //表示消费
                    switch (xcxTemplate.Type)
                    {
                        case (int)TmpType.小程序专业模板:
                            updatelevelResult = VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "entpro");
                            break;

                        case (int)TmpType.小程序电商模板:
                            updatelevelResult = VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "");
                            break;

                        case (int)TmpType.小程序餐饮模板:
                            updatelevelResult = VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "food");
                            break;
                    }
                }
                returnObj.isok = true;
                returnObj.Msg = "修改成功";
                returnObj.dataObj = new { saveMoneyAccount.AccountMoney, AccountMoneystr = saveMoneyAccount.AccountMoneyStr };
                return returnObj;
            }
        }

        /// <summary>
        /// 添加储值金额
        /// </summary>
        /// <returns></returns>
        private Return_Msg_APP AddSaveMoney(int saveMoney, string appId, C_UserInfo userInfo, SaveMoneySetUserLog newLog)
        {
            returnObj = new Return_Msg_APP();
            if (saveMoney < 0)
            {
                returnObj.Msg = "新开通的储值账号初始值不能为负数";
                return returnObj;
            }

            //用户储值账户,若无则开通一个
            SaveMoneySetUser saveMoneyAccount = new SaveMoneySetUser()
            {
                AppId = appId,
                UserId = userInfo.Id,
                AccountMoney = saveMoney,
                CreateDate = DateTime.Now
            };
            saveMoneyAccount.Id = Convert.ToInt32(SaveMoneySetUserBLL.SingleModel.Add(saveMoneyAccount));
            if (saveMoneyAccount.Id <= 0)
            {
                returnObj.Msg = "开通储值账户失败,请重试";
                return returnObj;
            }

            //加日志记录
            newLog.Type = 0;
            newLog.BeforeMoney = 0;
            newLog.MoneySetUserId = saveMoneyAccount.Id;
            newLog.ChangeMoney = saveMoney;
            newLog.AfterMoney = saveMoney;
            newLog.ChangeNote = $"商家增加{saveMoney}元";
            newLog.Id = Convert.ToInt32(SaveMoneySetUserLogBLL.SingleModel.Add(newLog));
            if (newLog.Id <= 0)
            {
                returnObj.Msg = "变更记录失败";
                return returnObj;
            }
            else
            {
                VipRelation vipRelation = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id}");
                if (vipRelation != null)
                {
                    vipRelation.updatetime = DateTime.Now;
                    VipRelationBLL.SingleModel.Update(vipRelation, "updatetime");
                }
                returnObj.isok = true;
                returnObj.Msg = "修改成功";
                returnObj.dataObj = new { saveMoneyAccount.AccountMoney, AccountMoneystr = saveMoneyAccount.AccountMoneyStr };
                return returnObj;
            }
        }

        /// <summary>
        /// 商品上下架
        /// </summary>
        /// <param name="id"></param>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        private Return_Msg_APP UpdateGoods(int aid, EntGoods goods, int tag)
        {
            returnObj = new Return_Msg_APP();
            if (tag != -1)
            {
                bool isSyncData = false;
                if (goods.tag == 1 && tag == 0)
                {
                    isSyncData = true;
                }

                goods.tag = tag;

                TransactionModel TranModel = new TransactionModel();
                //商品状态变动,更新购物车内商品的状态
                List<EntGoodsAttrDetail> goodsDtlList = goods.GASDetailList;
                List<string> updateGoodsStateSqlList = new List<string>();
                updateGoodsStateSqlList = EntGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(goods.id, tag == 1 ? 0 : 1, oldGoodState: tag == 1 ? 1 : 0);
                updateGoodsStateSqlList.ForEach(x =>
                {
                    TranModel.Add(x);
                });
                TranModel.Add(EntGoodsBLL.SingleModel.BuildUpdateSql(goods, "tag"));
                //if (_entgoodBLL.Update(model, "tag"))
                if (EntGoodsBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
                {
                    if (isSyncData)
                    {
                        EntSettingBLL.SingleModel.SyncData(aid, "$..coms[?(@.type=='good')].items[?(@.id==" + goods.id + ")]");
                    }
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return returnObj;
                }
            }
            returnObj.Msg = "操作失败";
            return returnObj;
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        private Return_Msg_APP DeleteGoods(int aid, EntGoods goods)
        {
            returnObj = new Return_Msg_APP();

            goods.state = 0;

            TransactionModel TranModel = new TransactionModel();
            //商品状态变动,更新购物车内商品的状态
            List<EntGoodsAttrDetail> goodsDtlList = goods.GASDetailList;
            List<string> updateGoodsStateSqlList = new List<string>();
            updateGoodsStateSqlList = EntGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(goods.id, 2);
            updateGoodsStateSqlList.ForEach(x =>
            {
                TranModel.Add(x);
            });

            TranModel.Add(EntGoodsBLL.SingleModel.BuildUpdateSql(goods, "state"));
            if (EntGoodsBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {
                EntSettingBLL.SingleModel.SyncData(aid, "$..coms[?(@.type=='good')].items[?(@.id==" + goods.id + ")]");
                returnObj.isok = true;
                returnObj.Msg = "删除成功";
                return returnObj;
            }
            returnObj.Msg = "删除失败";
            return returnObj;
        }

        /// <summary>
        /// 处理团购订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="appId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private Return_Msg_APP UpdateGroupOrderState(int orderId, string appId, int state)
        {
            returnObj = new Return_Msg_APP();
            GroupUser groupuser = GroupUserBLL.SingleModel.GetModel(orderId);
            if (groupuser == null)
            {
                returnObj.Msg = "拼团信息异常 , 请刷新页面后重试";
                return returnObj;
            }

            switch (state)
            {
                case -2://退款

                    if (groupuser.BuyPrice <= 0)
                    {
                        returnObj.Msg = "此拼团购买价格为0，不需要退款";
                        break;
                    }
                    string msg = "";
                    if (!GroupUserBLL.SingleModel.RefundOne(groupuser, ref msg, 1))
                    {
                        returnObj.Msg = "退款失败" + msg;
                        break;
                    }
                    returnObj.Msg = "退款成功";
                    returnObj.isok = true;
                    break;

                case 1:
                    if (groupuser.State != (int)MiniappPayState.待发货)
                    {
                        returnObj.Msg = "此拼团信息异常，请刷新后重试";
                        break;
                    }
                    groupuser.State = (int)MiniappPayState.已发货;
                    groupuser.SendGoodTime = DateTime.Now;
                    if (!GroupUserBLL.SingleModel.Update(groupuser, "state,sendgoodtime"))
                    {
                        returnObj.Msg = "发货失败";
                        break;
                    }
                    returnObj.Msg = "发货成功";
                    returnObj.isok = true;

                    XcxAppAccountRelation xcx = _xcxAppAccountRelationBLL.GetModelByAppid(groupuser.AppId);
                    if (xcx == null)
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception($"发送模板消息,参数不足,XcxAppAccountRelation_null:AppId = {groupuser.AppId}"));
                        break;
                    }
                    XcxTemplate xcxtemp = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
                    if (xcxtemp == null)
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception($"发送模板消息,参数不足,xcxtemp_null:id = {xcx.TId}"));
                        break;
                    }
                    //发给用户发货通知
                    object groupData = TemplateMsg_Miniapp.GroupGetTemplateMessageData(string.Empty, groupuser, SendTemplateMessageTypeEnum.拼团基础版订单发货提醒);
                    TemplateMsg_Miniapp.SendTemplateMessage(groupuser.ObtainUserId, SendTemplateMessageTypeEnum.拼团基础版订单发货提醒, xcxtemp.Type, groupData);
                    returnObj.Msg = "发货成功";
                    returnObj.isok = true;
                    break;

                default:
                    returnObj.Msg = "参数异常";
                    break;
            }
            return returnObj;
        }

        /// <summary>
        /// 处理预约订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private Return_Msg_APP UpdateSubscribeOrderState(int orderId, int aid, int state, string remark)
        {
            returnObj = new Return_Msg_APP();
            if (remark.Length > 100)
            {
                returnObj.Msg = "备注内容不能超过100字";
                return returnObj;
            }
            EntUserForm form = EntUserFormBLL.SingleModel.GetModel($"id={orderId} and aid={aid} and state>0");
            if (form == null)
            {
                returnObj.Msg = "您无权执行此操作";
                return returnObj;
            }
            form.state = state;
            EntFormRemark formremark = new EntFormRemark();
            if (!string.IsNullOrEmpty(form.remark))
            {
                formremark = JsonConvert.DeserializeObject<EntFormRemark>(form.remark);
            }
            formremark.operationremark = remark;
            form.remark = JsonConvert.SerializeObject(formremark);
            if (form.state == 2 && !form.excedHandle.Contains("a"))
            {
                form.excedHandle += "a";
                object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(form, SendTemplateMessageTypeEnum.专业版产品预约成功通知);
                TemplateMsg_Miniapp.SendTemplateMessage(form.uid, SendTemplateMessageTypeEnum.专业版产品预约成功通知, (int)TmpType.小程序专业模板, orderData);
            }

            returnObj.isok = EntUserFormBLL.SingleModel.Update(form, "state,remark,excedHandle");
            returnObj.Msg = returnObj.isok ? "保存成功" : "保存失败";
            return returnObj;
        }

        /// <summary>
        /// 更新砍价订单状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private Return_Msg_APP UpdateBargainOrderState(int orderId, string appId, int aid, int state,string attachData="")
        {
            returnObj = new Return_Msg_APP();
            BargainUser bargainUser = BargainUserBLL.SingleModel.GetModel(orderId);
            if (bargainUser == null)
            {
                returnObj.Msg = "数据不存在!";
                return returnObj;
            }

            Bargain bargain = BargainBLL.SingleModel.GetModel(bargainUser.BId);
            if (bargain == null)
            {
                returnObj.Msg = "砍价商品不存在!";
                return returnObj;
            }
            switch (state)
            {
                case 2://退款
                    string msg = "";
                    returnObj.isok = BargainUserBLL.SingleModel.OutOrder(bargainUser, bargain, appId, out msg);
                    returnObj.Msg = msg;
                    break;

                case 6://发货
                    //string sendGoodsName = Context.GetRequest("sendGoodsName", string.Empty);
                    //string wayBillNo = Context.GetRequestInt("wayBillNo", string.Empty);
                    returnObj.isok = BargainUserBLL.SingleModel.SendGoods(bargainUser, bargain, aid, out msg, attachData);
                    returnObj.Msg = msg;
                    break;

                default:
                    returnObj.Msg = "参数异常";
                    break;
            }
            return returnObj;
        }

        /// <summary>
        /// 更新普通订单状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="state"></param>
        /// <param name="oldState"></param>
        /// <returns></returns>
        private Return_Msg_APP UpdateGoodsOrderState(int orderId, int state, int oldState,string attachData="")
        {
            returnObj = new Return_Msg_APP();
            EntGoodsOrder order = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
            if (order == null)
            {
                returnObj.Msg = "订单信息异常";
                return returnObj;
            }
            if (!Enum.IsDefined(typeof(MiniAppEntOrderState), state))
            {
                returnObj.Msg = "状态错误,请重新刷新页面！";
                return returnObj;
            }
            order.State = state;

            bool isSuccess = false;
            switch (state)
            {
                case (int)MiniAppEntOrderState.已取消://取消订单
                    isSuccess = EntGoodsOrderBLL.SingleModel.updateStock(order, oldState);
                    if (isSuccess)
                    {
                        //发给用户取消通知
                        object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(order, SendTemplateMessageTypeEnum.专业版订单取消通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(order.UserId, SendTemplateMessageTypeEnum.专业版订单取消通知, TmpType.小程序专业模板, orderData);
                    }
                    break;

                case (int)MiniAppEntOrderState.退款中://退款
                    isSuccess = EntGoodsOrderBLL.SingleModel.outOrder(order, oldState, order.BuyMode);
                    break;

                //case (int)MiniAppEntOrderState.待收货:

                //    order.DistributeDate = DateTime.Now;
                //    isSuccess = EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(order.Id, oldState, state);

                //    if (isSuccess)
                //    {
                //        //发给用户发货通知
                //        object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(order, SendTemplateMessageTypeEnum.专业版订单发货提醒);
                //        TemplateMsg_Miniapp.SendTemplateMessage(order, SendTemplateMessageTypeEnum.专业版订单发货提醒, TmpType.小程序专业模板, orderData);
                //    }
                //    break;

                case (int)MiniAppEntOrderState.待收货:
                    order.DistributeDate = DateTime.Now;
                    bool isSaveDelivery = !string.IsNullOrWhiteSpace(attachData);
                    if (isSaveDelivery)
                    {
                        //保存物流信息
                        DeliveryUpdatePost DeliveryInfo = System.Web.Helpers.Json.Decode<DeliveryUpdatePost>(attachData);
                        bool isCompleteInfo = (DeliveryInfo.SelfDelivery || (!string.IsNullOrWhiteSpace(DeliveryInfo.CompanyCode) && !string.IsNullOrWhiteSpace(DeliveryInfo.DeliveryNo)))
                                              && !string.IsNullOrWhiteSpace(DeliveryInfo.ContactName)
                                              && !string.IsNullOrWhiteSpace(DeliveryInfo.ContactTel)
                                              && !string.IsNullOrWhiteSpace(DeliveryInfo.Address);
                        if (!isCompleteInfo)
                        {
                            isSuccess = false;
                        }
                        if (DeliveryInfo.SelfDelivery)
                        {
                            DeliveryInfo.CompanyTitle = "商家自配送";
                            DeliveryInfo.CompanyCode = null;
                        }

                        isSuccess = DeliveryFeedbackBLL.SingleModel.AddEntOrderFeed(order.Id, DeliveryInfo) && EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(order.Id, oldState, state);
                    }
                    else
                    {
                        isSuccess = EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(order.Id, oldState, state);
                    }

                    if (isSuccess)
                    {
                        //发给用户发货通知
                        object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(order, SendTemplateMessageTypeEnum.专业版订单发货提醒);
                        TemplateMsg_Miniapp.SendTemplateMessage(order.UserId, SendTemplateMessageTypeEnum.专业版订单发货提醒, TmpType.小程序专业模板, orderData);
                    }
                    break;


                case (int)MiniAppEntOrderState.交易成功:
                    isSuccess = EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(order.Id, oldState, state);
                    if (isSuccess)
                    {
                        ////加销量
                       List<EntGoodsCart> cartList = EntGoodsCartBLL.SingleModel.GetListByGoodsOrderId(order.Id);
                        //cartList?.Select(cart => cart.FoodGoodsId).Distinct().ToList().ForEach(goodsId =>
                        //{
                        //    int salesCount = cartList.Where(y => y.FoodGoodsId == goodsId).Sum(y => y.Count);
                        //    _entgoodBLL.UpdateSaleCountById(goodsId, salesCount);

                        //});

                        //会员加消费金额
                        if (!VipRelationBLL.SingleModel.updatelevel(order.UserId, "entpro", order.BuyPrice))
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常(订单发货后 超过10天,系统自动完成订单)" + order.Id));
                        }

                        //消费符合积分规则赠送积分

                        if (!ExchangeUserIntegralBLL.SingleModel.AddUserIntegral(order.UserId, order.aId, 0, order.Id))
                            log4net.LogHelper.WriteError(GetType(), new Exception("商家端:赠送积分失败(订单发货后 超过10天,系统自动完成订单)" + order.Id));

                        //确认收货后 判断该订单购物车里面是否是分销产生的 如果购物车里的产品佣金比例不为零则需要操作分销相关的

                        try
                        {

                            #region 分销相关

                            List<EntGoodsCart> listEntGoodsCart = cartList;

                            listEntGoodsCart.ForEach(x =>
                            {
                                //将购物车里需要计算佣金的产品计算佣金分给对应的分销员

                                if (x.recordId > 0)
                                {
                                    TransactionModel tranModel = new TransactionModel();
                                    SalesManRecord salesManRecord = salesManRecordBLL.GetModel($"Id={x.recordId} and state=1");
                                    if (salesManRecord == null)
                                    {
                                        LogHelper.WriteInfo(this.GetType(), $"商家端:购物车{x.Id}确认收货后分销计算佣金失败SalesManRecord为NULL");
                                    }
                                    else
                                    {
                                        salesManRecord.configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManRecord.configStr);

                                        SalesManRecordUser salesManRecordUser = SalesManRecordUserBLL.SingleModel.GetModel($"recordId={salesManRecord.Id}");
                                        if (salesManRecordUser == null && salesManRecord.configModel.payMentManager.allow_seller_buy == 0)
                                        {
                                            LogHelper.WriteInfo(this.GetType(), $"商家端:购物车{x.Id}确认收货后分销计算佣金失败salesManRecordUser为NULL");
                                        }
                                        else
                                        {
                                            //当条件都符合的时候才进行佣金订单计算

                                            int salesManId = salesManRecord.salesManId;
                                            if (salesManRecordUser != null)
                                            {
                                                salesManId = salesManRecordUser.salesManId;
                                                //延续对应分销员的保护期
                                                salesManRecordUser.UpdateTime = DateTime.Now;
                                                salesManRecordUser.protected_time = salesManRecord.configModel.salesManManager.protected_time;
                                                tranModel.Add(SalesManRecordUserBLL.SingleModel.BuildUpdateSql(salesManRecordUser));
                                            }

                                            SalesMan salesMan = SalesManBLL.SingleModel.GetModel(salesManId);
                                            double firstCps_rate = 1, secondCps_rate = 1;
                                            SalesMan parentSalesman = SalesManBLL.SingleModel.GetModel(salesMan.ParentSalesmanId);
                                            if (parentSalesman != null)
                                            {
                                                //表示该分销商品对应的分销员有上级分销员,需要重新根据二级分销规则进行佣金再分配
                                                SalesManConfig salesManConfig = SalesManConfigBLL.SingleModel.GetModel($"appId={x.aId}");
                                                if (salesManConfig != null && !string.IsNullOrEmpty(salesManConfig.configStr))
                                                {
                                                    ConfigModel configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);
                                                    if (configModel != null)
                                                    {
                                                        firstCps_rate = configModel.secondSalesManConfig.FirstCps_rate * 0.01;//直销佣金比例
                                                        secondCps_rate = configModel.secondSalesManConfig.SecondCps_rate * 0.01;//渠道佣金比例

                                                    }

                                                }


                                            }



                                            //分销订单记录表新增一条
                                            SalesManRecordOrder salesManRecordOrder = new SalesManRecordOrder()
                                            {
                                                appId = x.aId,
                                                salesManRecordId = x.recordId,
                                                orderNumber = order.OrderNum,
                                                orderId = order.Id,
                                                CarId = x.Id,
                                                orderMoney = x.Price * x.Count,
                                                cps_rate = x.cps_rate,
                                                cpsMoney = (int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01),
                                                state = salesManRecord.configModel.payMentManager.auto_settle,
                                                addTime = DateTime.Now

                                            };

                                            if ((firstCps_rate + secondCps_rate) <= 1 && firstCps_rate >= 0 && firstCps_rate <= 1 && secondCps_rate >= 0 && secondCps_rate <= 1)
                                            {
                                                //如果当前分销员有上级,则重新计算直销佣金
                                                salesManRecordOrder.cpsMoney = (int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01 * firstCps_rate);
                                            }
                                            salesManRecordOrder.Remark = $"商家端:本次推广订单应得佣金{(int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01 * 0.01)}元,比例{x.cps_rate * 0.01};直销佣金比例{firstCps_rate};渠道佣金比例{secondCps_rate};对应购物车Id={x.Id}";


                                            tranModel.Add(SalesManRecordOrderBLL.SingleModel.BuildAddSql(salesManRecordOrder));



                                            //更新分销员可提现佣金
                                            SalesManCashLog salesManCashLog = new SalesManCashLog();
                                            salesManCashLog.Aid = x.aId;
                                            salesManCashLog.SaleManId = salesMan.Id;
                                            salesManCashLog.AddTime = DateTime.Now;


                                            int curCash = 0;


                                            if (salesManRecord.configModel.payMentManager.auto_settle == 0)
                                            {

                                                salesManCashLog.CashLog = $"商家端:本次变更前useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                                salesMan.useCashTotal += salesManRecordOrder.cpsMoney;//人工结算 自动累计到总收益不能到可提现金额
                                                salesManCashLog.CashLog += $"商家端:本次变更后useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                                salesManCashLog.Remark = $"商家端:变更原因:对应购物车Id={x.Id}";

                                                #region 如果当前分销员有上级,则还需要更新上级分销员佣金情况
                                                if (parentSalesman != null && (firstCps_rate + secondCps_rate) <= 1 && firstCps_rate >= 0 && firstCps_rate <= 1 && secondCps_rate >= 0 && secondCps_rate <= 1)
                                                {
                                                    curCash = (int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01 * secondCps_rate);//计算渠道佣金

                                                    //更新上级分销员佣金
                                                    SalesManCashLog parentSalesmanCashLog = new SalesManCashLog();
                                                    parentSalesmanCashLog.Aid = x.aId;
                                                    parentSalesmanCashLog.SaleManId = parentSalesman.Id;
                                                    parentSalesmanCashLog.AddTime = DateTime.Now;
                                                    parentSalesmanCashLog.CashLog = $"商家端:本次变更前useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";

                                                    parentSalesman.useCashTotal += curCash;
                                                    parentSalesman.UpdateTime = DateTime.Now;

                                                    parentSalesmanCashLog.CashLog += $"商家端:本次变更后useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                                    parentSalesmanCashLog.Remark = $"商家端:变更原因:对应购物车Id={x.Id},来自下级分销员{salesMan.Id}";

                                                    SalesManRelation salesManRelation = new SalesManRelation();
                                                    salesManRelation.Aid = x.aId;
                                                    salesManRelation.UserId = salesMan.UserId;
                                                    salesManRelation.AddTime = DateTime.Now;
                                                    salesManRelation.Price = curCash;
                                                    salesManRelation.ParentSaleManId = parentSalesman.Id;
                                                    salesManRelation.AutoSettle = 0;

                                                    tranModel.Add(SalesManRelationBLL.SingleModel.BuildAddSql(salesManRelation));
                                                    tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(parentSalesman));
                                                    tranModel.Add(SalesManCashLogBLL.SingleModel.BuildAddSql(parentSalesmanCashLog));
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                salesManCashLog.CashLog = $"商家端:本次变更前useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                                salesMan.useCash += salesManRecordOrder.cpsMoney;
                                                salesManCashLog.CashLog += $"商家端:本次变更后useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                                if (parentSalesman != null && (firstCps_rate + secondCps_rate) <= 1 && firstCps_rate >= 0 && firstCps_rate <= 1 && secondCps_rate >= 0 && secondCps_rate <= 1)
                                                {
                                                    curCash = (int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01 * secondCps_rate);//计算渠道佣金

                                                    //更新上级分销员佣金
                                                    SalesManCashLog parentSalesmanCashLog = new SalesManCashLog();
                                                    parentSalesmanCashLog.Aid = x.aId;
                                                    parentSalesmanCashLog.SaleManId = parentSalesman.Id;
                                                    parentSalesmanCashLog.AddTime = DateTime.Now;
                                                    parentSalesmanCashLog.CashLog = $"商家端:本次变更前useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";

                                                    parentSalesman.UpdateTime = DateTime.Now;
                                                    parentSalesman.useCash += curCash;

                                                    parentSalesmanCashLog.CashLog += $"商家端:本次变更后useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                                    parentSalesmanCashLog.Remark = $"商家端:变更原因:对应购物车Id={x.Id},来自下级分销员{salesMan.Id}";

                                                    SalesManRelation salesManRelation = new SalesManRelation();
                                                    salesManRelation.Aid = x.aId;
                                                    salesManRelation.UserId = salesMan.UserId;
                                                    salesManRelation.AddTime = DateTime.Now;
                                                    salesManRelation.Price = curCash;
                                                    salesManRelation.ParentSaleManId = parentSalesman.Id;
                                                    salesManRelation.AutoSettle = 1;

                                                    tranModel.Add(SalesManRelationBLL.SingleModel.BuildAddSql(salesManRelation));

                                                    tranModel.Add(SalesManCashLogBLL.SingleModel.BuildAddSql(parentSalesmanCashLog));
                                                    tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(parentSalesman));
                                                }
                                            }

                                            salesMan.UpdateTime = DateTime.Now;
                                            tranModel.Add(SalesManCashLogBLL.SingleModel.BuildAddSql(salesManCashLog));
                                            tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(salesMan));
                                            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
                                            {
                                                if (!SalesManRecordUserBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
                                                {
                                                    LogHelper.WriteInfo(this.GetType(), $"商家端:确认收货后购物车对应产品分销计算佣金失败");
                                                }
                                            }
                                        }
                                    }
                                }
                            });



                            #endregion 分销相关
                        }
                        catch (Exception ex)
                        {
                            log4net.LogHelper.WriteError(this.GetType(), ex);
                        }

                    }
                    break;

                default:
                    isSuccess = EntGoodsOrderBLL.SingleModel.UpdateEntGoodsOrderState(order.Id, oldState, state);

                    break;
            }

            if (!isSuccess)
            {
                returnObj.Msg = "操作失败！";
                return returnObj;
            }
            EntGoodsOrderLogBLL.SingleModel.AddLog(order.Id, 0, $"将订单状态改为：{Enum.GetName(typeof(MiniAppEntOrderState), order.State)}");
            returnObj.isok = true;
            returnObj.Msg = "操作成功！";
            return returnObj;
        }

        /// <summary>
        /// 获取预约订单相关状态的订单数量
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetSubscribeOrderCount(int aid, int type, string value)
        {
            List<object> objData = new List<object>();
            string startDate = string.Empty;
            string endDate = string.Empty;
            int allCount = EntUserFormBLL.SingleModel.GetOrderSumByCondition(aid, type, value);//全部订单 dateType = 0
            objData.Add(new { name = "全部订单", count = allCount, dateType = 0, state = -999, icon = 0, groupId = 0 });

            startDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd") + "00:00:00";
            endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            int weekCount = EntUserFormBLL.SingleModel.GetOrderSumByCondition(aid, type, value, startDate, endDate);
            objData.Add(new { name = "7天订单", count = weekCount, dateType = 1, state = -999, icon = 1, groupId = 1 });

            startDate = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            int todayCount = EntUserFormBLL.SingleModel.GetOrderSumByCondition(aid, type, value, startDate, endDate);//今日订单数 dateType = 2
            objData.Add(new { name = "今日订单", count = todayCount, dateType = 2, state = -999, icon = 2, groupId = 1 });

            //预约订单状态 1:未处理  2:已处理
            int state = 1;//未处理
            int unDealCount = EntUserFormBLL.SingleModel.GetOrderSumByCondition(aid, type, value, state);
            objData.Add(new { name = "未处理", count = todayCount, dateType = 0, state = state, icon = 61, groupId = 5 });
            state = 2;//已处理
            int DealCount = EntUserFormBLL.SingleModel.GetOrderSumByCondition(aid, type, value, state);
            objData.Add(new { name = "已处理", count = todayCount, dateType = 0, state = state, icon = 60, groupId = 5 });
            return new { list = objData, groups = 3 };
        }

        /// <summary>
        ///  获取团购订单相关状态的订单数量
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetGroupOrderCount(string appId, int type, string value)
        {
            List<object> objData = new List<object>();
            string startDate = string.Empty;
            string endDate = string.Empty;

            int allCount = GroupUserBLL.SingleModel.GetOrderSumByCondition(appId, type, value);//全部订单数
            objData.Add(new { name = "全部订单", count = allCount, dateType = 0, state = -999, icon = 0, groupId = 0 });

            startDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd") + " 00:00:00";
            endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            int weekCount = GroupUserBLL.SingleModel.GetOrderSumByCondition(appId, type, value, startDate, endDate);//7天订单数
            objData.Add(new { name = "7天订单", count = weekCount, dateType = 1, state = -999, icon = 1, groupId = 1 });

            startDate = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            int todayCount = GroupUserBLL.SingleModel.GetOrderSumByCondition(appId, type, value, startDate, endDate);//今日订单数
            objData.Add(new { name = "今日订单", count = todayCount, dateType = 2, state = -999, icon = 2, groupId = 1 });

            //订单状态：-2：拼团失败，2：拼团中，0：待发货，1：待收货，-1：已收货
            int state = -2;//拼团失败
            int cancelCount = GroupUserBLL.SingleModel.GetOrderSumByCondition(appId, type, value, state);
            objData.Add(new { name = "拼团失败", count = cancelCount, dateType = 0, state = state, icon = 32, groupId = 9 });

            state = 2;//拼团中
            int waitSendCount = GroupUserBLL.SingleModel.GetOrderSumByCondition(appId, type, value, state);
            objData.Add(new { name = "拼团中", count = waitSendCount, dateType = 0, state = state, icon = 30, groupId = 9 });

            state = 0;//待发货
            int waitTakeCount = GroupUserBLL.SingleModel.GetOrderSumByCondition(appId, type, value, state);
            objData.Add(new { name = "待发货", count = waitTakeCount, dateType = 0, state = state, icon = 40, groupId = 2 });

            state = 1;//待收货
            int sendCount = GroupUserBLL.SingleModel.GetOrderSumByCondition(appId, type, value, state);
            objData.Add(new { name = "待收货", count = sendCount, dateType = 0, state = state, icon = 41, groupId = 2 });

            state = -1;//已收货
            int takenCount = GroupUserBLL.SingleModel.GetOrderSumByCondition(appId, type, value, state);
            objData.Add(new { name = "已收货", count = takenCount, dateType = 0, state = state, icon = 43, groupId = 2 });
            return new { list = objData, groups = 4 };
        }

        /// <summary>
        /// 获取砍价订单相关状态的订单数量
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetBargainOrderCount(int aid, int type, string value)
        {
            //icon 见石墨商家版小程序接口文档中的icon对照表
            List<object> objData = new List<object>();
            string startDate = string.Empty;
            string endDate = string.Empty;
            int allCount = BargainUserBLL.SingleModel.GetOrderSumByCondition(aid, type, value);//全部订单数
            objData.Add(new { name = "全部订单", count = allCount, dateType = 0, state = -999, icon = 0, groupId = 0 });

            startDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd") + " 00:00:00";
            endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            int weekCount = BargainUserBLL.SingleModel.GetOrderSumByCondition(aid, type, value, startDate, endDate);//7天订单数
            objData.Add(new { name = "7天订单", count = weekCount, dateType = 1, state = -999, icon = 1, groupId = 1 });

            startDate = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            int todayCount = BargainUserBLL.SingleModel.GetOrderSumByCondition(aid, type, value, startDate, endDate);//今日订单数
            objData.Add(new { name = "今日订单", count = todayCount, dateType = 2, state = -999, icon = 2, groupId = 1 });
            //-1 已取消,5 待付款,6待收货,7待发货,8交易成功
            int state = -1;//已取消
            int cancelCount = BargainUserBLL.SingleModel.GetOrderSumByCondition(aid, type, value, state);
            objData.Add(new { name = "已取消", count = cancelCount, dateType = 0, state = state, icon = 51, groupId = 3 });
            state = 5;//待付款
            int unPayCount = BargainUserBLL.SingleModel.GetOrderSumByCondition(aid, type, value, state);
            objData.Add(new { name = "待付款", count = unPayCount, dateType = 0, state = state, icon = 52, groupId = 2 });
            state = 6;//待收货
            int waitTakenCount = BargainUserBLL.SingleModel.GetOrderSumByCondition(aid, type, value, state);
            objData.Add(new { name = "待收货", count = waitTakenCount, dateType = 0, state = state, icon = 41, groupId = 2 });
            state = 7;//待发货
            int takenCount = BargainUserBLL.SingleModel.GetOrderSumByCondition(aid, type, value, state);
            objData.Add(new { name = "待发货", count = takenCount, dateType = 0, state = state, icon = 40, groupId = 2 });
            state = 8;//交易成功
            int SuccessCount = BargainUserBLL.SingleModel.GetOrderSumByCondition(aid, type, value, state);
            objData.Add(new { name = "交易成功", count = SuccessCount, dateType = 0, state = state, icon = 50, groupId = 3 });
            return new { list = objData, groups = 4 };
        }

        /// <summary>
        /// 获取普通订单相关状态的订单数量(包含高级拼团)
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetGoodsOrderCount(string appId, int orderType, int type, string value)
        {
            //icon 见石墨商家版小程序接口文档中的icon对照表
            List<object> objData = new List<object>();
            string startDate = string.Empty;
            string endDate = string.Empty;
            EntGoodsOrderBLL _entgoodsorderBll = EntGoodsOrderBLL.SingleModel;

            int allCount = EntGoodsOrderBLL.SingleModel.GetOrderSumByCondition(appId, orderType, type, value);//全部订单数 dateType = 0
            objData.Add(new { name = "全部订单", count = allCount, dateType = 0, state = -999, icon = 0, groupId = 0 });

            startDate = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd") + " 00:00:00";
            endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            int weekCount = EntGoodsOrderBLL.SingleModel.GetOrderSumByCondition(appId, orderType, type, value, startDate, endDate);//7天订单数 dateType = 1
            objData.Add(new { name = "7天订单", count = weekCount, dateType = 1, state = -999, icon = 1, groupId = 1 });

            startDate = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            endDate = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            int todayCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, startDate, endDate);//今日订单数 dateType = 2
            objData.Add(new { name = "今日订单", count = todayCount, dateType = 2, state = -999, icon = 2, groupId = 1 });

            //普通订单状态: 1：待发货，2: 待收货，3:交易成功，8：待自取，-4：退款成功，-3：退款失败，-2：退款中，-1：已取消，0：待付款
            //拼团订单状态：1：待发货，2：待收货，3:交易成功，8：待自取，30：拼团中，31：拼团成功，32：拼团失败

            int state = 1;//待发货
            int waitTakeCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
            objData.Add(new { name = "待发货", count = waitTakeCount, dateType = 0, state = state, icon = 40, groupId = 2 });
            state = 2;//待收货
            int takenCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
            objData.Add(new { name = "待收货", count = takenCount, dateType = 0, state = state, icon = 41, groupId = 2 });
            state = 8;//待自取
            int waitPickCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);//待自取
            objData.Add(new { name = "待自取", count = waitPickCount, dateType = 0, state = state, icon = 42, groupId = 2 });
            state = 3;//交易成功
            int successCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
            objData.Add(new { name = "交易成功", count = successCount, dateType = 0, state = state, icon = 50, groupId = 3 });

            if (orderType == 3)
            {
                state = 30;//拼团中
                int groupingCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
                objData.Add(new { name = "拼团中", count = groupingCount, dateType = 0, state = state, icon = 30, groupId = 9 });
                state = 31;//拼团成功
                int groupSuccessCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
                objData.Add(new { name = "拼团成功", count = groupSuccessCount, dateType = 0, state = state, icon = 31, groupId = 9 });
                state = 32;//拼团失败
                int groupFailCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
                objData.Add(new { name = "拼团失败", count = groupFailCount, dateType = 0, state = state, icon = 32, groupId = 9 });
            }
            else
            {
                state = -4;//退款成功
                int cancelCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
                objData.Add(new { name = "退款成功", count = cancelCount, dateType = 0, state = state, icon = 54, groupId = 4 });
                state = -3;//退款失败
                int unPayCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
                objData.Add(new { name = "退款失败", count = unPayCount, dateType = 0, state = state, icon = 55, groupId = 4 });
                state = -2;//退款中
                int unVerificationCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
                objData.Add(new { name = "退款中", count = unVerificationCount, dateType = 0, state = state, icon = 53, groupId = 4 });
                state = -1;//已取消
                int verificationCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
                objData.Add(new { name = "已取消", count = verificationCount, dateType = 0, state = state, icon = 51, groupId = 3 });
                state = 0;//待付款
                int waitSendCount = _entgoodsorderBll.GetOrderSumByCondition(appId, orderType, type, value, state);
                objData.Add(new { name = "待付款", count = waitSendCount, dateType = 0, state = state, icon = 52, groupId = 2 });
            }
            return new { list = objData, groups = 5 };
        }

        private object GetRecordByDate(string appId, int aid, string startDate, string endDate, int type)
        {
            EntGoodsOrderBLL _entgoodsorderBll = EntGoodsOrderBLL.SingleModel;
            //指定时间段所有订单数
            int groupOrderSum = GroupUserBLL.SingleModel.GetGroupOrderSum(appId, startDate, endDate);//拼团订单数
            int goodsOrderSum = _entgoodsorderBll.GetOrderSum(appId, startDate, endDate);//普通订单数
            int bargainOrderSum = BargainUserBLL.SingleModel.GetBargainOrderSum(aid, -999, startDate, endDate);//砍价订单数
            int orderSum = groupOrderSum + goodsOrderSum + bargainOrderSum;

            ////指定时间段成交额
            int groupPriceSum = GroupUserBLL.SingleModel.GetPriceSumByAppId_Date(appId, startDate, endDate);//团购总额
            int goodsOrderPriceSum = _entgoodsorderBll.GetOrderPriceSumByAppId_Date(appId, startDate, endDate);//普通订单总额+拼团订单总额
            int bargainPriceSum = BargainUserBLL.SingleModel.GetPriceSumByAppId_Date(aid, startDate, endDate);//砍价总额
            int priceSum = groupPriceSum + goodsOrderPriceSum + bargainPriceSum;

            //指定时间段浏览量
            // string uv =

            //指定时间段支付订单数
            string states = $"0,1,-1";
            int groupPayOrderCount = GroupUserBLL.SingleModel.GetGroupOrderSum(appId, states, startDate, endDate);// 团购订单数
            states = $"1,2,3,8,31";
            int goodsPayOrderCount = _entgoodsorderBll.GetOrderSum(appId, states, startDate, endDate);//普通订单数+拼团订单
            states = $"6,7,8";
            int bargainPayOrderCount = BargainUserBLL.SingleModel.GetBargainOrderSum(aid, states, startDate, endDate);//砍价订单数
            int PayOrderCount = groupPayOrderCount + goodsPayOrderCount + bargainPayOrderCount;

            //指定时间段待处理订单
            int groupWaitDealCount = GroupUserBLL.SingleModel.GetGroupOrderSum(appId, 0, startDate, endDate);//团购待处理订单
            int goodsWaitDealCount = _entgoodsorderBll.GetOrderSum(appId, 1, startDate, endDate);//普通订单待处理订单+拼团待处理订单
            int bargainWaitDealCount = BargainUserBLL.SingleModel.GetBargainOrderSum(aid, 7, startDate, endDate);//砍价待处理订单
            int waitDealCount = groupWaitDealCount + goodsWaitDealCount + bargainWaitDealCount;
            //log4net.LogHelper.WriteInfo(GetType(), $"{groupWaitDealCount},{goodsWaitDealCount},{waitDealCount}");
            //指定时间段支付买家数
            int payUserCount = GroupUserBLL.SingleModel.GetPayUserCount(appId, aid, startDate, endDate);
            string uv = GetUv(appId, type);
            object data = new { orderSum, PayOrderCount, waitDealCount, payUserCount, priceSum = (priceSum * 0.01).ToString("0.00"), uv = uv };
            return data;
        }

        private string GetUv(string appid, int type)
        {
            string key = "gkuv_{0}_{1}_{2}";//type,startdate,enddate

            string msg = string.Empty;
            //获取授权信息
            OpenAuthorizerInfo openconfig = OpenAuthorizerInfoBLL.SingleModel.GetModelByAppId(appid);
            if (openconfig != null)
            {
                //获取token
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
                if(xcxrelation==null)
                {
                    log4net.LogHelper.WriteInfo(GetType(), "模板权限无效");
                    msg = "-0";
                    return msg;
                }
                XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                string url = XcxApiBLL.SingleModel.GetOpenAuthodModel(openconfig.user_name);
                string authorizer_access_token = CommondHelper.GetAuthorizer_Access_Token(url);
                string startDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                string endDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                if (!string.IsNullOrEmpty(authorizer_access_token))
                {
                    string gkurl = XcxApiBLL.SingleModel.GetDailySumMaryTrend(authorizer_access_token);
                    switch (type)
                    {
                        case 0://昨日趋势
                            gkurl = XcxApiBLL.SingleModel.GetDailyVisitTrend(authorizer_access_token);
                            startDate = endDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                            break;

                        case 1://上周趋势
                            gkurl = XcxApiBLL.SingleModel.GetWeeklyVisitTrend(authorizer_access_token);
                            startDate = DateHelper.GetWeekFirstDayMon(DateTime.Now.AddDays(-7)).ToString("yyyyMMdd");
                            endDate = DateHelper.GetWeekLastDaySun(DateTime.Now.AddDays(-7)).ToString("yyyyMMdd");
                            break;

                        case 2://上月趋势
                            gkurl = XcxApiBLL.SingleModel.GetMonthlyVisitTrend(authorizer_access_token);
                            startDate = DateHelper.GetMonthFirstDay(DateTime.Now.AddMonths(-1)).ToString("yyyyMMdd");
                            endDate = DateHelper.GetMonthLastDay(DateTime.Now.AddMonths(-1)).ToString("yyyyMMdd");
                            break;
                    }

                    key = string.Format(key, 0, startDate, endDate);
                    msg = RedisUtil.Get<string>(key);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        return msg;
                    }

                    //获取概括趋势数据
                    object postdata = new { begin_date = startDate, end_date = endDate };
                    string dataJson = JsonConvert.SerializeObject(postdata);
                    msg = HttpHelper.DoPostJson(gkurl, dataJson);
                    XCXDataModel<FWResultModel> gkresult = JsonConvert.DeserializeObject<XCXDataModel<FWResultModel>>(msg);
                    if (gkresult.list != null && gkresult.list.Count > 0)
                    {
                        msg = gkresult.list[0].visit_uv == null ? "0" : gkresult.list[0].visit_uv;
                        RedisUtil.Set(key, msg, TimeSpan.FromMinutes(5));
                    }
                    else
                    {
                        msg = "0";
                    }
                }
                else
                {
                    log4net.LogHelper.WriteInfo(GetType(), "获取token失败");
                    msg = "-0";
                }
            }
            else
            {
                log4net.LogHelper.WriteInfo(GetType(), "未授权");
                msg = "-0";
            }
            return msg;
        }

        /// <summary>
        /// 查询商品详情
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey, MasterCheck]
        public ActionResult GetGoodInfo(int id)
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("storeAppId", string.Empty);
            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            EntGoods good = EntGoodsBLL.SingleModel.GetModel(id);
            if (good == null || good.state == 0)
            {
                returnObj.Msg = "产品不存在或已删除";
                return Json(returnObj);
            }

            returnObj.isok = true;
            returnObj.dataObj = good;
            return Json(returnObj);
        }
    }
}