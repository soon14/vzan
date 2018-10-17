using BLL.MiniApp;
using BLL.MiniApp.Plat;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Plat.Filters;
using Utility.IO;

namespace User.MiniApp.Areas.Plat.Controllers
{
    [LoginFilter]
    [MiniApp.Filters.RouteAuthCheck]
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
            int aid = Context.GetRequestInt("aid",0);
            if(aid<=0)
            {
                returnData.Msg = "参数错误";
                return Json(returnData);
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            //总访问量
            int sumpv = PlatStatisticalFlowBLL.SingleModel.GetPVCount(aid);

            //平台概况
            //入驻店铺
            int storeCount = PlatStoreBLL.SingleModel.GetCountByAId(aid,xcxrelation.AppId);
            //会员人数
            int userCount = PlatMyCardBLL.SingleModel.GetCountByAId(aid, xcxrelation.AppId);
            //发帖总数
            int msgCount = PlatMsgBLL.SingleModel.GetCountByAId(aid);
            //优选商品
            int goodsCount = PlatStoreBLL.SingleModel.GetSyncGoodsTotalCount(aid,xcxrelation.AppId);
            //上次访问时间
            string visiteTime = RedisUtil.Get<string>(string.Format(PlatStatisticalFlowBLL._redis_PlatVisiteTimeKey, aid));

            DateTime nowTime = DateTime.Now;
            //访问量（PV）
            //当天，昨天，近七天，近30天，总累计
            int todayPV = 0;
            int yeastodayPV = PlatStatisticalFlowBLL.SingleModel.GetPVCount(aid,"",nowTime.AddDays(-1).ToString("yyyy-MM-dd"), nowTime.ToString("yyyy-MM-dd"));
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
                    //item.RefDate = DateTime.Parse(item.RefDate).ToShortDateString();
                    labels.Add(item.RefDate);
                    datas.Add(item.VisitPV);
                }
            }

            //XCXDataModel<FWResultModel> tempdata = new XCXDataModel<FWResultModel>();
            ////获取第三方平台用户数据
            //OpenAuthorizerInfo openconfig = new OpenAuthorizerInfoBLL().GetModelByAppId("wx9cb1d8be83da075b");
            //if (openconfig != null) {
            //    string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(openconfig.user_name);
            //    string authorizer_access_token = new CommondHelper().GetAuthorizer_Access_Token(xcxapiurl);
            //    //获取日概况数据
            //    string fwurl = XcxApiBLL.SingleModel.GetDailyVisitTrend(authorizer_access_token);
            //    Return_Msg fwresult = XcxApiBLL.SingleModel.GetDataInfo<XCXDataModel<FWResultModel>>(fwurl, nowtime, nowtime);
            //    tempdata = (XCXDataModel<FWResultModel>)fwresult.dataObj;
            //}


            returnData.dataObj = new {
                sumpv = sumpv,
                storecount = storeCount,
                usercount = userCount,
                msgcount = msgCount,
                goodscount= goodsCount,
                todaypv= todayPV,
                yeastodaypv= yeastodayPV,
                sevendaypv= sevenDayPV,
                thirthdaypv= thirthDayPV,
                labels=labels,
                datas=datas,
                visitetime= visiteTime,
            };

            return Json(returnData);
        }
    }
}