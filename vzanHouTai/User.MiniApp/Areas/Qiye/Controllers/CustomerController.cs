using BLL.MiniApp;
using BLL.MiniApp.Conf;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Web.Mvc;
using System.Web.UI;
using User.MiniApp.Comment;
using User.MiniApp.Model;
using Utility;
using User.MiniApp.Filters;
using User.MiniApp.Areas.Qiye.Model;
using BLL.MiniApp.Plat;
using Entity.MiniApp.Qiye;
using Entity.MiniApp.Im;
using BLL.MiniApp.Im;
using BLL.MiniApp.Qiye;

namespace User.MiniApp.Areas.Qiye.Controllers
{
    public class CustomerController : User.MiniApp.Controllers.baseController
    {
        
        
        
        /// <summary>
        /// 实例化对象
        /// </summary>
        public CustomerController()
        {

        }
        
        public ActionResult Index(int aid=0)
        {
            ViewBag.appId = aid;
            return View();
        }

        public ActionResult GetDataList(int aid=0,string name = "",int pageSize = 10,int pageIndex=1)
        {
            Return_Msg returnData = new Return_Msg();
            int count = 0;
            List<QiyeCustomer> list = QiyeCustomerBLL.SingleModel.GetDataList(aid,name,pageSize,pageIndex,ref count);

            returnData.dataObj = new { data = list, count = count };
            returnData.isok = true;
            return Json(returnData);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DelData(int aid = 0, int id = 0)
        {
            Return_Msg returnData = new Return_Msg();
            QiyeCustomer customer = QiyeCustomerBLL.SingleModel.GetModel(id);
            if(customer==null)
            {
                returnData.Msg = "无效数据";
                return Json(returnData);
            }
            if (customer.Aid != aid)
            {
                returnData.Msg = "权限不足";
                return Json(returnData);
            }
            customer.State = -1;
            customer.UpdateTime = DateTime.Now;
            returnData.isok = QiyeCustomerBLL.SingleModel.Update(customer, "state,UpdateTime");
            returnData.Msg = returnData.isok ? "删除成功" : "删除失败";

            return Json(returnData);
        }

        /// <summary>
        /// 保存客户备注
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="desc"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SaveDesc(int aid = 0,string desc="",string phoneDesc="",int id=0,int employeeId=0)
        {
            Return_Msg returnData = new Return_Msg();
            QiyeCustomer model = QiyeCustomerBLL.SingleModel.GetModel(id);
            if(model==null)
            {
                returnData.Msg = "请刷新重试";
                return Json(returnData);
            }
            if(model.Aid !=aid)
            {
                returnData.Msg = "权限不足";
                return Json(returnData);
            }
            if (employeeId > 0)
            {
                model.StaffId = employeeId;
            }
            else
            {
                model.StaffId = 0;
            }
            model.Desc = desc;
            model.PhoneDesc = phoneDesc;
            model.UpdateTime = DateTime.Now;
            returnData.isok = QiyeCustomerBLL.SingleModel.Update(model, "StaffId,PhoneDesc,desc,updatetime");
            returnData.Msg = returnData.isok ? "保存成功" : "保存失败";

            return Json(returnData);
        }

        /// <summary>
        /// 获取员工列表
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult GetEmployeeDataList(int aid=0)
        {
            Return_Msg returnData = new Return_Msg();
            List<QiyeEmployee> list = QiyeEmployeeBLL.SingleModel.GetListByAid(aid);
            returnData.isok = true;
            returnData.dataObj = list;

            return Json(returnData);
        }

        /// <summary>
        /// 批量修改绑定客服
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public ActionResult SaveBindEmployee(int aid=0,int id=0,string customerId="")
        {
            Return_Msg returnData = new Return_Msg();
            if(id<=0)
            {
                returnData.isok = false;
                returnData.Msg = "无效参数";
                return Json(returnData);
            }

            if(string.IsNullOrEmpty(customerId))
            {
                returnData.isok = false;
                returnData.Msg = "无效客户参数";
                return Json(returnData);
            }
            customerId = customerId.Substring(0, customerId.Length-1);

            QiyeEmployee employeeModel = QiyeEmployeeBLL.SingleModel.GetModel(id);
            if(employeeModel==null)
            {
                returnData.isok = false;
                returnData.Msg = "无效员工，请刷新重试！";
                return Json(returnData);
            }

            returnData.isok = QiyeCustomerBLL.SingleModel.UpdateBindEmployee(id,customerId);
            returnData.Msg = returnData.isok ? "保存成功" : "保存失败";

            return Json(returnData);
        }

        public ActionResult Imessage(int aid=0,int customerId=0)
        {
            ViewBag.CustomerId = customerId;
            ViewBag.appId = aid;
            return View();
        }

        #region 私信记录
        public ActionResult GetImessageUser(int aid=0,int customerId = 0)
        {
            Return_Msg returnData = new Return_Msg();
            if(customerId<=0)
            {
                returnData.Msg = "无效参数";
                return Json(returnData);
            }
            QiyeCustomer customer = QiyeCustomerBLL.SingleModel.GetModel(customerId);
            if(customer==null)
            {
                returnData.Msg = "无效客户，请刷新重试！";
                return Json(returnData);
            }
            List<ImMessage> messageList = ImMessageBLL.SingleModel.GetListByTuserid(customer.UserId);
            if(messageList != null && messageList.Count>0)
            {
                string userIds = string.Join(",", messageList.Where(w=>w.tuserId!=customer.UserId).Select(s=>s.tuserId).Distinct());
                //List<C_UserInfo> userList = _userInfoBLL.GetListByIds(userIds);
                List<QiyeEmployee> employeeList = QiyeEmployeeBLL.SingleModel.GetListByUserIds(userIds);
                
                returnData.dataObj = new { data= employeeList, customer = customer} ;
                returnData.isok = true;
            }
            return Json(returnData);
        }

        public ActionResult GetImMessageDataList(int aid = 0, int tuserId = 0,int fuserId=0,int pageIndex=1,int pageSize=10)
        {
            Return_Msg returnData = new Return_Msg();
            int count = 0;
            List<ImMessage> list = ImMessageBLL.SingleModel.GetListByTFUserId(tuserId,fuserId,pageIndex,pageSize,ref count);
            returnData.dataObj = new { data = list,count = count };
            return Json(returnData);
        }
        #endregion
    }
}