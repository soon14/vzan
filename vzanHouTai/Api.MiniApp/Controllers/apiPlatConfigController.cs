using BLL.MiniApp;
using BLL.MiniApp.Plat;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.CoreHelper;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public partial class apiPlatController : InheritController
    {
        /// <summary>
        /// 获取首页轮播图以及推荐商家
        /// </summary>
        /// <returns></returns>
        public ActionResult GetConfig()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);

            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            int pageIndexAdImg = Context.GetRequestInt("pageIndexAdImg", 1);
            int pageSizeAdImg = Context.GetRequestInt("pageSizeAdImg", 10);
            int pageIndexTjStore = Context.GetRequestInt("pageIndexTjStore", 1);
            int pageSizeTjStore = Context.GetRequestInt("pageSizeTjStore", 20);

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            string appname = "";
            OpenAuthorizerConfig openonfigmodel = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(r.AppId);
            if(openonfigmodel!=null)
            {
                appname = openonfigmodel.nick_name;
            }
            else
            {
                UserXcxTemplate userXcxModel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(r.AppId);
                appname = userXcxModel?.Name;
            }
            int adImgCount, tjStoreCount = 0;
            List<PlatConfig> listADImg = PlatConfigBLL.SingleModel.getListByaid(r.Id, out adImgCount, 0, pageSizeAdImg, pageIndexAdImg);
            List<PlatConfig> listTjStore = PlatConfigBLL.SingleModel.getListByaid(r.Id, out tjStoreCount, 1, pageSizeTjStore, pageIndexTjStore);

            string lngStr = Context.GetRequest("lng", string.Empty);
            string latStr = Context.GetRequest("lat", string.Empty);
            double lng = 0.00;
            double lat = 0.00;
            string curLocation = "未知城市" ;
            int curCityCode = 0;
            if (!double.TryParse(lngStr, out lng) || !double.TryParse(latStr, out lat))
            {
                string IP = WebHelper.GetIP();
                
                IPToPoint iPToPoint = CommondHelper.GetLoctionByIP(IP);
                if (iPToPoint != null)
                {

                    lat = iPToPoint.result.location.lat;
                    lng = iPToPoint.result.location.lng;
                    lngStr = lng.ToString();
                    latStr = lat.ToString();
                    //log4net.LogHelper.WriteInfo(this.GetType(), $"IP={IP};{lat},{lng}");
                }
            }
            AddressApi addressinfo = AddressHelper.GetAddressByApi(lngStr, latStr);
           // log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(addressinfo));
            if (addressinfo != null && addressinfo.result != null && addressinfo.result.address_component != null)
            {
                curLocation = addressinfo.result.address_component.city;
                curCityCode = C_AreaBLL.SingleModel.GetCodeByName(curLocation);
            }


            listTjStore.ForEach(x =>
            {
                x.ObjName = x.ObjName.Length > 6 ? (x.ObjName.Substring(0, 5) + "...") : x.ObjName;
            });

            PlatConfig platConfigRemark = PlatConfigBLL.SingleModel.GetPlatConfig(r.Id,3);
            var remarkObj = new { haveRemark = false, remark =string.Empty, remarkOpenFrm=false };
            if(platConfigRemark != null && !string.IsNullOrEmpty(platConfigRemark.ADImg))
            {
                remarkObj = new { haveRemark=true, remark= platConfigRemark.ADImg, remarkOpenFrm=platConfigRemark.ObjId==0 };
            }

            PlatConfig platOther = PlatConfigBLL.SingleModel.GetPlatConfig(r.Id, 5);//获取平台其它设置
            if (platOther == null)
            {
                platOther = new PlatConfig() { Aid = r.Id, AddTime = DateTime.Now, ConfigType = 5 };
            }
            PlatOtherConfig platOtherConfig = new PlatOtherConfig();
            platOtherConfig.VirtualPV = platOther.ADImgType;
            platOtherConfig.VirtualPlatMsgCount = platOther.ObjId;
            platOtherConfig.PlatMsgCount = PlatMsgBLL.SingleModel.GetCountByAId(r.Id);
            platOtherConfig.PV = PlatStatisticalFlowBLL.SingleModel.GetPVCount(r.Id);

            int totalPV = platOtherConfig.VirtualPV + platOtherConfig.PV;
            int totalMsgCout = platOtherConfig.PlatMsgCount + platOtherConfig.VirtualPlatMsgCount;

            PlatConfig platOfficialAccount = PlatConfigBLL.SingleModel.GetPlatConfig(r.Id, 6);//获取关注公众号设置
            if (platOfficialAccount == null)
            {
                platOfficialAccount = new PlatConfig() { Aid = r.Id, AddTime = DateTime.Now, ConfigType = 6 };
            }


            returnObj.dataObj = new
            {
                appname = appname,
                ADImgs = new { totalCount = adImgCount, list = listADImg },
                TjStores = new { totalCount = tjStoreCount, list = listTjStore },
                Location = new { curLocation = curLocation, curCityCode = curCityCode },
                platConfigRemark = remarkObj,
                platOtherConfig= new {TotalPV= totalPV>100000?Convert.ToInt32(totalPV*0.0001).ToString()+"万+": totalPV.ToString(), TotalMsgCount = totalMsgCout>100000? Convert.ToInt32(totalMsgCout *0.0001).ToString() + "万+": totalMsgCout.ToString() },
                platOfficialAccount= platOfficialAccount
            };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取流量广告配置
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="configType"></param>
        /// <returns></returns>
        public ActionResult GetAdvertisementConfig(int aid=0,int configType=0)
        {
            returnObj = new Return_Msg_APP();
            if(configType<0 || aid<=0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            List<PlatConfig> list = PlatConfigBLL.SingleModel.GetListByConfigType(aid,configType);
            returnObj.dataObj = list;
            returnObj.isok = true;
            return Json(returnObj);
        }


        /// <summary>
        /// 获取平台入驻模式配置
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="configType"></param>
        /// <returns></returns>
        public ActionResult GetAddStoreConfig(int aid = 0)
        {
            returnObj = new Return_Msg_APP();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            List<PlatStoreAddRules> list = PlatStoreAddRulesBLL.SingleModel.getListByaid(aid, out totalCount, 1000, 1);
            PlatStoreAddSetting platStoreAddSetting = PlatStoreAddSettingBLL.SingleModel.GetPlatStoreAddSetting(aid);

            returnObj.dataObj = new {AddWay=(platStoreAddSetting!=null? platStoreAddSetting.AddWay:0),rules=list };
            returnObj.isok = true;
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据城市名称获取百度天气
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public ActionResult GetBaiduWeather(string location)
        {
            returnObj = new Return_Msg_APP();
            if (string.IsNullOrEmpty(location))
            {
                
                returnObj.Msg = "请选择城市";
                return Json(returnObj,JsonRequestBehavior.AllowGet);
            }

            string result = HttpHelper.GetData($"http://api.map.baidu.com/telematics/v3/weather?location={location}&output=json&ak={ConfigurationManager.AppSettings["BaiduAk"]}");
            returnObj.Msg = "获取成功";
            returnObj.dataObj = result;
            returnObj.isok = true;
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }
    }
}