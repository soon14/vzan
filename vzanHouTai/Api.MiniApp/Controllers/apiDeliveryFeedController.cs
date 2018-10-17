using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using Newtonsoft.Json;
using BLL.MiniApp.Tools;
using BLL.MiniApp;
using Core.MiniApp;

namespace Api.MiniApp.Controllers
{
    /// <summary>
    /// 快鸟服务接受推送接口
    /// </summary>
    public class apiDeliveryFeedController : Controller
    {
        /// <summary>
        /// 接收订阅轨迹
        /// </summary>
        /// <param name="EBusinessID"></param>
        /// <returns></returns>
        public ActionResult Subscribe(string DataSign = null, string RequestType = null, string RequestData = null)
        {
            if(string.IsNullOrWhiteSpace(DataSign) || string.IsNullOrWhiteSpace(RequestType) || string.IsNullOrWhiteSpace(RequestData))
            {
                //返回安全404
                return new HttpStatusCodeResult(404);
            }

            Func<bool, string, object> feedBackResult = (success, reason) =>
            {
                return new { EBusinessID = WebConfigBLL.MerchIdForDeliveryAPi, UpdateTime = DateTime.Now.ToString(), Success = success, Reason = reason };
            };

            if(RequestType != "101")
            {
                //非订阅推送动作
                return Json(feedBackResult(false, "非订阅动作"));
            }

            //停用：签名加密算法异常
            //DeliveryFeedbackBLL feedBLL = new DeliveryFeedbackBLL();
            //if (!feedBLL.CompareSign(RequestData, DataSign))
            //{
            //    //非法签名
            //    return Json(feedBackResult(false, "非法签名"));
            //}

            DeliverySubscribePush feedBack = JsonConvert.DeserializeObject<DeliverySubscribePush>(RequestData);
            if(feedBack == null)
            {
                //异常数据结构
                return Json(feedBackResult(false, "序列化失败：RequestData"));
            }

            int proessCount = 0;
            if(!int.TryParse(feedBack.Count, out proessCount) || proessCount == 0)
            {
                //处理数量为零，不插入队列
                return Json(feedBackResult(true, $"推送队列接受，共处理{feedBack.Count}条物流推送"));
            }

            //插入数据库队列
            
            bool result = DeliverySubscribeBLL.SingleModel.AddFromAPI(feedBack);

            return Json(feedBackResult(result, $"推送队列接受，共处理{feedBack.Count}条物流推送"));
        }

#if DEBUG
        public ActionResult SendSubscribe(string no,string code,string fid)
        {
            return Json(DeliveryAPI.RequestSubscribe(WebConfigBLL.MerchIdForDeliveryAPi, WebConfigBLL.AuthKeyForDeliveryAPI, no, code, callBack: fid));
        }

        public ActionResult GetRealTime(string no,string code)
        {
            return Json(JsonConvert.DeserializeObject<DeliveryData>(DeliveryAPI.RequestRealTime(WebConfigBLL.MerchIdForDeliveryAPi, WebConfigBLL.AuthKeyForDeliveryAPI, no, code).ToString()), JsonRequestBehavior.AllowGet);
        }
#endif
    }
}