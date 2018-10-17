using BLL.MiniApp;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Tools;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using User.MiniApp.Areas.PlatChild.Filters;
using Utility.IO;

namespace User.MiniApp.Areas.PlatChild.Controllers
{
    [LoginFilter]
    public class SurveyController : User.MiniApp.Controllers.baseController
    {
        public ActionResult Index()
        {
            int aid = Context.GetRequestInt("aid", 0);
            return View();
        }

        public ActionResult GetData()
        {
            Return_Msg returnData = new Return_Msg();
            int aid = Context.GetRequestInt("aid", 0);
            if (aid <= 0)
            {
                returnData.Msg = "参数错误";
                return Json(returnData);
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                returnData.Msg = "无效模板";
                return Json(returnData);
            }
            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(aid);
            if (store == null)
            {
                returnData.Msg = "无效店铺";
                return Json(returnData);
            }
            PlatMyCard myCard = PlatMyCardBLL.SingleModel.GetModel(store.MyCardId);
            if (myCard == null)
            {
                returnData.Msg = "无效用户数据";
                return Json(returnData);
            }
            PlatUserCash userCash = PlatUserCashBLL.SingleModel.GetModelByUserId(store.BindPlatAid, myCard.UserId);
            if (userCash == null)
            {
                return Redirect("/base/PageError?type=2");
            }
            PlatDrawConfig platDrawConfig = PlatDrawConfigBLL.SingleModel.GetModelByAId(store.BindPlatAid);
            if (platDrawConfig == null)
            {
                platDrawConfig = new PlatDrawConfig();
            }

            //总访问量
            int sumPV = PlatStatisticalFlowBLL.SingleModel.GetPVCount(aid);
            //店铺概况
            //当天订单
            int newOrderCount = PlatChildGoodsOrderBLL.SingleModel.GetTodayOrderCount(aid);
            //总订单
            int orderCount = PlatChildGoodsOrderBLL.SingleModel.GetOrderSum(aid);
            //会员人数
            int userCount = C_UserInfoBLL.SingleModel.GetCountByAppid(xcxrelation.AppId);
            //领券人数
            int couponCount = CouponLogBLL.SingleModel.GetUserCount(aid);
            //上次访问时间
            string visiteTime = RedisUtil.Get<string>(string.Format(PlatStatisticalFlowBLL._redis_PlatVisiteTimeKey, aid));
            //平台订单数
            int platOrderNum = PlatChildGoodsOrderBLL.SingleModel.GetPlatOrderNum(store.BindPlatAid, store.Id);
            //平台交易总额
            int platSumPrice = PlatChildGoodsOrderBLL.SingleModel.GetPlatOrderSumPrice(store.BindPlatAid, store.Id);
            //已提现金额
            int drawedCashPrice = userCash.UseCashTotal;
            //可提现金额
            int canDrawCashPrice = userCash.UseCash;
            //提现手续费
            int drawCashServer = platDrawConfig.Fee;

            DateTime nowTime = DateTime.Now;
            //访问量（PV）
            //当天，昨天，近七天，近30天，总累计
            int todayPV = 0;
            int yeastodayPV = PlatStatisticalFlowBLL.SingleModel.GetPVCount(aid, "", nowTime.AddDays(-1).ToString("yyyy-MM-dd"), nowTime.ToString("yyyy-MM-dd"));
            int sevenDayPV = PlatStatisticalFlowBLL.SingleModel.GetPVCount(aid, "", nowTime.AddDays(-7).ToString("yyyy-MM-dd"), nowTime.ToString("yyyy-MM-dd"));
            int thirthDayPV = PlatStatisticalFlowBLL.SingleModel.GetPVCount(aid, "", nowTime.AddDays(-30).ToString("yyyy-MM-dd"), nowTime.ToString("yyyy-MM-dd"));
            //近30天访问趋势统计
            List<PlatStatisticalFlow> list = PlatStatisticalFlowBLL.SingleModel.GetListByAid(aid, nowTime.AddDays(-30).ToString("yyyy-MM-dd"), nowTime.ToString("yyyy-MM-dd"));
            List<string> labels = new List<string>();
            List<int> datas = new List<int>();
            if (list != null && list.Count > 0)
            {
                foreach (PlatStatisticalFlow item in list)
                {
                    labels.Add(item.RefDate);
                    datas.Add(item.VisitPV);
                }
            }

            returnData.dataObj = new
            {
                sumpv = sumPV,
                newordercount = newOrderCount,
                usercount = userCount,
                ordercount = orderCount,
                couponcount = couponCount,
                todaypv = todayPV,
                yeastodaypv = yeastodayPV,
                sevendaypv = sevenDayPV,
                thirthdaypv = thirthDayPV,
                labels = labels,
                datas = datas,
                visitetime = visiteTime,

                platordernum = platOrderNum,
                platsumprice = (platSumPrice * 0.01).ToString("0.00"),
                drawedcashprice = (drawedCashPrice * 0.01).ToString("0.00"),
                candrawcashprice = (canDrawCashPrice * 0.01).ToString("0.00"),
                isopendistribution = userCash.IsOpenDistribution,
                drawcashserver = (drawCashServer * 0.01) + "%",
            };

            return Json(returnData);
        }

        public ActionResult OpenDistribution(int aid = 0, int isOpen = 0)
        {
            
            Return_Msg returnData = new Return_Msg();
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                returnData.Msg = "无效模板";
                return Json(returnData);
            }
            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(aid);
            if (store == null)
            {
                returnData.Msg = "无效店铺";
                return Json(returnData);
            }
            PlatMyCard myCard = PlatMyCardBLL.SingleModel.GetModel(store.MyCardId);
            if (myCard == null)
            {
                returnData.Msg = "无效用户数据";
                return Json(returnData);
            }
            PlatUserCash userCash = PlatUserCashBLL.SingleModel.GetModelByUserId(store.BindPlatAid, myCard.UserId);
            if (userCash == null)
            {
                returnData.Msg = "无效账号";
                return Json(returnData);
            }
            userCash.IsOpenDistribution = isOpen;
            userCash.UpdateTime = DateTime.Now;
            returnData.isok = PlatUserCashBLL.SingleModel.Update(userCash, "IsOpenDistribution,UpdateTime");
            returnData.Msg = returnData.isok ? "保存成功" : "保存失败";

            return Json(returnData);
        }
    }
}