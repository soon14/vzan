using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Stores;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Stores;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public class apiSaveMoneyController : InheritController
    {
    }
    [ExceptionLog]
    public class apiMiappSaveMoneyController : apiSaveMoneyController
    {
        public apiMiappSaveMoneyController()
        {

        }

        #region 储值项目
        /// <summary>
        /// 充值项目列表获取
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getSaveMoneySetList(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "获取失败(appid不能为空)" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModel($"AppId='{appid}'");
            if (r == null)
            {
                return Json(new { isok = false, msg = "获取失败(还未进行授权)" }, JsonRequestBehavior.AllowGet);
            }

            var list = SaveMoneySetBLL.SingleModel.getListByAppId(r.AppId, 1);

            return Json(new { isok = true, msg = "获取成功!", saveMoneySetList = list }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 请求预充值
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addSaveMoneySet(string appid, string openid, int saveMoneySetId)
        {

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "获取失败(appid不能为空)" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModel($"AppId='{appid}'");
            if (r == null)
            {
                return Json(new { isok = false, msg = "获取失败(还未进行授权)" }, JsonRequestBehavior.AllowGet);
            }
            var minisnsId = 0;
            //获取当前小程序模板类型
            var typeSql = $" select type from xcxtemplate where id = {r.TId} ";
            var temp = Convert.ToInt32(SqlMySql.ExecuteScalar(_xcxAppAccountRelationBLL.connName, CommandType.Text, typeSql, null));
            if (temp == (int)TmpType.小程序电商模板 || temp == (int)TmpType.小程序电商模板测试)
            {
                var store = StoreBLL.SingleModel.GetModelByRid(r.Id) ?? new Store();
                minisnsId = store.Id;
                try
                {
                    store.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(store.configJson);
                }
                catch (Exception)
                {
                    store.funJoinModel = new StoreConfigModel();
                }
                if (!store.funJoinModel.canSaveMoneyFunction)
                {
                    return Json(new { isok = false, msg = "商家未开启储值优惠功能" }, JsonRequestBehavior.AllowGet);
                }
            }
            else if (temp == (int)TmpType.小程序餐饮模板)
            {
                var food = FoodBLL.SingleModel.GetModel($" appId = {r.Id} ") ?? new Food();
                minisnsId = food.Id;
                if (!food.funJoinModel.canSaveMoneyFunction)
                {
                    return Json(new { isok = false, msg = "商家未开启储值优惠功能" }, JsonRequestBehavior.AllowGet);
                }
            }

            var model = SaveMoneySetBLL.SingleModel.GetModel(saveMoneySetId, r.AppId);
            if (model == null || model.State != 1)//要是上架状态的储值项目
            {
                return Json(new { isok = false, msg = "找不到储值项目" }, JsonRequestBehavior.AllowGet);
            }
            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (model == null || model.State != 1)//要是上架状态的储值项目
            {
                return Json(new { isok = false, msg = "找不到储值项目" }, JsonRequestBehavior.AllowGet);
            }
            var user = SaveMoneySetUserBLL.SingleModel.getModelByUserId(r.AppId, userInfo.Id);
            if (user == null)
            {
                //用户储值账户,若无则开通一个
                user = new SaveMoneySetUser()
                {
                    AppId = r.AppId,
                    UserId = userInfo.Id,
                    AccountMoney = 0,
                    CreateDate = DateTime.Now
                };
                user.Id = Convert.ToInt32(SaveMoneySetUserBLL.SingleModel.Add(user));
                if (user.Id <= 0)
                {
                    return Json(new { isok = -1, msg = "开通储值账户失败,请重试" }, JsonRequestBehavior.AllowGet);
                }
            }
            //充值记录
            var newLog = new SaveMoneySetUserLog()
            {
                AppId = r.AppId,
                UserId = userInfo.Id,
                MoneySetUserId = user.Id,
                Type = 0,
                BeforeMoney = user.AccountMoney,
                AfterMoney = user.AccountMoney + model.AmountMoney,
                ChangeMoney = model.AmountMoney,
                ChangeNote = model.SetName,
                CreateDate = DateTime.Now,
                State = 0,
                GiveMoney = model.GiveMoney
            };
            //付款后才去累计预充值金额
            //user.AccountMoney += model.AmountMoney;
            //_miniappsavemoneysetuserBll.Update(user, "AccountMoney");
            newLog.Id = Convert.ToInt32(SaveMoneySetUserLogBLL.SingleModel.Add(newLog));
            //小程序信息
            var xcxconfig = OpenAuthorizerConfigBLL.SingleModel.GetModel($" appid = '{r.AppId}' ");

            #region cityMorder 
            string no = WxPayApi.GenerateOutTradeNo();

            CityMorders citymorderModel = new CityMorders
            {
                OrderType = (int)ArticleTypeEnum.MiniappSaveMoneySet,
                ActionType = (int)ArticleTypeEnum.MiniappSaveMoneySet,
                Addtime = DateTime.Now,
                payment_free = model.JoinMoney,
                trade_no = no,
                Percent = 99,//不收取服务费
                userip = WebHelper.GetIP(),
                FuserId = userInfo.Id,
                Fusername = userInfo.NickName,
                orderno = no,
                payment_status = 0,
                Status = 0,
                Articleid = 0,
                CommentId = 0,
                MinisnsId = minisnsId, //r.Id,//模板权限表Id
                TuserId = newLog.Id,//充值记录的ID
                ShowNote = $" {xcxconfig?.nick_name} 储值项目[{model.SetName}]购买商品支付{(model.JoinMoney * 0.01).ToString("0.00")}元",
                CitySubId = 0,//无分销,默认为0
                PayRate = 1,
                buy_num = 0, //无
                appid = appid,
            };
            var orderid = Convert.ToInt32(_cityMordersBLL.Add(citymorderModel));
            if (orderid <= 0)
            {
                return Json(new { isok = false, msg = "生成订单失败!" }, JsonRequestBehavior.AllowGet);
            }
            newLog.OrderId = orderid;
            SaveMoneySetUserLogBLL.SingleModel.Update(newLog, "OrderId");
            #endregion

            return Json(new { isok = true, msg = "获取成功!", orderid = newLog.OrderId }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取储值记录列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getSaveMoneySetUserLogList(string appid, string openid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "获取失败(appid不能为空)" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModel($"AppId='{appid}'");
            if (r == null)
            {
                return Json(new { isok = false, msg = "获取失败(还未进行授权)" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            var list = SaveMoneySetUserLogBLL.SingleModel.getListByUserId(userInfo.Id, r.AppId);

            list.ForEach(x =>
            {
                if (x.Type == 0)
                {
                    x.ChangeNote = "【充值】" + x.ChangeNote;
                }
                else if (x.Type == -1)
                {
                    x.ChangeNote = "【消费】" + x.ChangeNote;
                }
                else if (x.Type == 1)
                {
                    x.ChangeNote = "【退款】" + x.ChangeNote;
                }

            });
            return Json(new { isok = 1, msg = "获取预存款记录成功", saveMoneyUserLogList = list.OrderByDescending(x => x.Id) }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取消费记录列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetPayLogList()
        {
            returnObj = new Return_Msg_APP();
            string appid = Context.GetRequest("appid", string.Empty);
            string openid = Context.GetRequest("openid", string.Empty);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            if (string.IsNullOrEmpty(appid))
            {
                returnObj.Msg = "获取失败(appid不能为空)";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (r == null)
            {
                returnObj.Msg = "获取失败(还未进行授权)";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }

            List<SaveMoneySetUserLog> list = SaveMoneySetUserLogBLL.SingleModel.GetListByUserId(userInfo.Id, r.AppId, pageSize, pageIndex);
            list.ForEach(x =>
            {
                if (x.Type == 0)
                {
                    x.ChangeNote = "【充值】" + x.ChangeNote;
                }
                else if (x.Type == -1)
                {
                    x.ChangeNote = "【储值支付】" + x.ChangeNote;
                }
                else if (x.Type == -2)
                {
                    x.ChangeNote = "【微信支付】" + x.ChangeNote;
                }
                else if (x.Type == 1)
                {
                    x.ChangeNote = "【退款】" + x.ChangeNote;
                }

            });

            returnObj.Msg = "获取预存款记录成功";
            returnObj.dataObj = list;
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 获取储值总额
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getSaveMoneySetUser(string appid, string openid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "获取失败(appid不能为空)" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModel($"AppId='{appid}'");
            if (r == null)
            {
                return Json(new { isok = false, msg = "获取失败(还未进行授权)" }, JsonRequestBehavior.AllowGet);
            }
            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            var saveMoneySetUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(r.AppId, userInfo.Id) ?? new SaveMoneySetUser();

            return Json(new { isok = true, msg = "获取成功!", saveMoneySetUser = saveMoneySetUser }, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}