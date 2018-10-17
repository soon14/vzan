using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Home;
using BLL.MiniApp.User;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Home;
using Entity.MiniApp.User;
using Entity.MiniApp.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ThoughtWorks.QRCode;
using User.MiniApp.Model;
using Utility;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class MiappAgentManagerController : agentmanagerController
    {

    }

    public class agentmanagerController : baseController
    {
        private static readonly AgentCustomerRelationBLL _agentCustomerRelationBLL = AgentCustomerRelationBLL.SingleModel;
        private static readonly AgentDistributionRelationBLL _agentDistributionRelationBLL = new AgentDistributionRelationBLL();

        protected Return_Msg msg;
        private readonly string _dzsavekey = "dz_savekey_";
        //测试模板时填测试类型模板类型ID
        private readonly string ceshitype = "";

        /// <summary>
        /// 实例化对象
        /// </summary>
        public agentmanagerController()
        {
            msg = new Return_Msg();
            ceshitype = agentinfo != null ? (agentinfo.TemplateTypeId == null ? "" : agentinfo.TemplateTypeId) : "";
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //判断是不是代理
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            int CustomerCount = _agentCustomerRelationBLL.GetCustomerCount(agentinfo.id);

            IndexViewModel info = XcxAppAccountRelationBLL.SingleModel.GetAgentTemplateInfon(agentinfo.id);
            if (info == null)
            {
                info = new IndexViewModel();
            }

            int tempcount = 0;
            List<XcxAppAccountRelationGroupInfo> tempinfo = XcxAppAccountRelationBLL.SingleModel.GetAgentTemplateInfonGroup(agentinfo.id, ref tempcount);

            info.TemplateCount = tempcount;
            info.agentinfo = agentinfo;
            info.CustomerCount = CustomerCount;
            ViewBag.Title = "首页";
            ViewBag.username = agentaccount.LoginId;
            OpenConfig(agentinfo);
            return View(info);
        }

        /// <summary>
        /// 判断代理商合同是否已过期
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckOutTime()
        {
            //判断是不是代理
            if (agentinfo == null)
            {
                msg.Msg = "无效代理";
                return Json(msg);
            }

            msg = AgentinfoBLL.SingleModel.CheckOutTime(agentinfo.id);
            return Json(msg);
        }

        [HttpGet]
        public ActionResult getTemplateInfo()
        {
            try
            {
                if (agentinfo == null)
                {
                    return Json(new { isok = 0, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
                }
                int count = 0;
                List<XcxAppAccountRelationGroupInfo> info = XcxAppAccountRelationBLL.SingleModel.GetAgentTemplateInfonGroup(agentinfo.id, ref count);

                return Json(new { isok = 1, data = info, count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = 0, msg = "系统繁忙" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 帮助中心
        /// </summary>
        /// <returns></returns>
        public ActionResult HelpCenter()
        {
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }

            OpenConfig(agentinfo);
            return View(agentinfo);
        }

        #region 客户管理
        /// <summary>
        /// 客户管理
        /// </summary>
        /// <returns></returns>
        public ActionResult UserList()
        {
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "用户不存在,user:" + CookieHelper.GetCookie("agent_UserCookieNew!"), code = "403" });
            }
            ViewBag.username = agentaccount.LoginId;
            #region 获取专业版级别 基础版3,高级版2,尊享版1,旗舰版0
            List<object> listXqrVersion = new List<object>();
            List<VersionType> listVersionType = FunctionListBLL.SingleModel.GetVersionTypeList((int)TmpType.小程序专业模板, agentinfo.AgentType);

            if (listVersionType != null && listVersionType.Count > 0)
            {
                foreach (VersionType item in listVersionType)
                {
                    listXqrVersion.Add(new
                    {
                        value = item.VersionId,
                        name = item.VersionName
                    });
                }

                ViewBag.listXqrVersion = JsonConvert.SerializeObject(listXqrVersion);
            }
            #endregion

            OpenConfig(agentinfo);
            return View(agentinfo);
        }

        /// <summary>
        /// 获取客户列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserList()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int pagesize = Context.GetRequestInt("pagesize", 10);
            int pageindex = Context.GetRequestInt("pageindex", 1);
            string LoginId = Context.GetRequest("loginname", string.Empty);
            string username = Context.GetRequest("username", string.Empty);
            string startTime = Context.GetRequest("starttime", string.Empty);
            string endtime = Context.GetRequest("endtime", string.Empty);
            int state = Context.GetRequestInt("state", 0);
            string tids = Context.GetRequest("tids", string.Empty);


            string errormsg = string.Empty;
            int count = 0;
            List<CustomerModel> customerList = _agentCustomerRelationBLL.GetCustomerList(agentinfo.id, LoginId, username, startTime, endtime, tids, state, pagesize, pageindex, ref count, ref errormsg, ceshitype);
            msg.isok = count >= 0;
            msg.Msg = errormsg;
            msg.dataObj = new
            {
                recordCount = count,
                customerList = customerList
            };
            return Json(msg);
        }

        /// <summary>
        /// 获取小程序模板
        /// </summary>
        /// <returns></returns>
        public ActionResult GetXcxTemplate()
        {
            if (agentinfo == null)
            {
                return Json(null);
            }
            int versionId = Context.GetRequestInt("versionId", -1);
            int templateType = Context.GetRequestInt("templateType", 0);
            int actionType = Context.GetRequestInt("actionType", 0);
            if (versionId > -1 && templateType == 22)
            {
                msg.isok = true;
                //表示专业版选择了版本级别过来
                VersionType model = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(templateType, agentinfo.id).FirstOrDefault(x => x.VersionId == versionId);
                msg.dataObj = model;
            }
            else if (actionType == -1)
            {
                string sqlwhere = $" (projectType={(int)ProjectType.小程序} {(ceshitype.Length > 0 ? $" or type in ({ceshitype})" : "")}) and state>=0";
                if (agentinfo.AgentType == 1)
                {
                    sqlwhere += $" and (price<=0 or type={(int)TmpType.小程序专业模板})";
                }
                List<XcxTemplate> list = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList(sqlwhere, agentinfo.id);

                List<VersionType> listVersionType = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList((int)TmpType.小程序专业模板, agentinfo.id, agentinfo.AgentType);
                list?.ForEach(x =>
                {
                    if (x.Type == (int)TmpType.小程序专业模板 && listVersionType != null && x.VersionId == 0)
                    {
                        VersionType m = listVersionType.Find(y => y.VersionId == 0);
                        if (m != null)
                        {
                            x.TName += m.VersionName;
                            x.Title += m.VersionName;
                            x.Price = Convert.ToInt32(m.VersionPrice);
                        }
                    }

                    x.ShowPrice = (x.Price * 0.01).ToString("0.00");
                });

                if (listVersionType != null && listVersionType.Count > 0)
                {
                    foreach (VersionType item in listVersionType)
                    {
                        if (item.VersionId != 0)
                        {
                            XcxTemplate xcxTemplateTemp = new XcxTemplate();

                            if (list != null)
                            {
                                xcxTemplateTemp.VersionId = item.VersionId;
                                xcxTemplateTemp.TName = "专业版" + item.VersionName;
                                xcxTemplateTemp.Title = "专业版" + item.VersionName;
                                xcxTemplateTemp.Price = Convert.ToInt32(item.VersionPrice);
                                xcxTemplateTemp.Type = 22;
                                XcxTemplate tempxcxmodel = list.Find(x => x.VersionId == 0 && x.Type == 22);
                                xcxTemplateTemp.Id = tempxcxmodel != null ? tempxcxmodel.Id : 0;
                            }

                            list.Add(xcxTemplateTemp);
                        }
                    }
                }

                msg.isok = true;
                list.ForEach(x => x.ShowPrice = (x.Price * 0.01).ToString("0.00"));
                if (agentinfo.AgentType == 1)
                {
                    XcxTemplate tempmodel = list.FirstOrDefault(f => f.Type == (int)TmpType.小程序专业模板 && f.VersionId == 0);
                    list.Remove(tempmodel);
                }
                msg.dataObj = list;
            }
            else
            {
                string sqlwhere = $" projectType={(int)ProjectType.小程序} and state>=0";
                if (ceshitype!=null&&ceshitype.Length > 0)
                {
                    sqlwhere += $" or type in ({ceshitype} )";
                }
                if (agentinfo.AgentType == 1)
                {
                    sqlwhere += $" and (price<=0 or type={(int)TmpType.小程序专业模板})";
                }
                List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList(sqlwhere, agentinfo.id);
                #region 判断代理商合同是否已过期，已过期不给开免费版
                string errMsg = "";
                AgentinfoBLL.SingleModel.CheckOutTime(ref xcxlist, agentinfo, 0, ref errMsg);
                #endregion
                xcxlist.ForEach(x => x.ShowPrice = (x.Price * 0.01).ToString("0.00"));
                msg.dataObj = xcxlist;
            }
            return Json(msg);
        }

        /// <summary>
        /// 获取可以升级的专业版版本级别
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUpEntProVersionXcxTemplate()
        {
            if (agentinfo == null)
            {
                msg.Msg = "agentinfo不能为NULL";
                return Json(msg);
            }
            int curVersionId = Context.GetRequestInt("oldVersionId", -1);

            List<VersionType> listVersionType = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList((int)TmpType.小程序专业模板, agentinfo.id);
            listVersionType = listVersionType.FindAll(x => x.VersionId < curVersionId);
            listVersionType.ForEach(x =>
            {
                x.VersionPrice = (Convert.ToInt32(x.VersionPrice) * 0.01).ToString("0.00");

            });
            msg.isok = true;
            msg.dataObj = listVersionType;
            return Json(msg);
        }

        /// <summary>
        /// 创建客户界面
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerCreate()
        {
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }

            //扫码登陆代码
            string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["qrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = false;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
            }

            #region 获取专业版级别 基础版3,高级版2,尊享版1,旗舰版0
            List<object> listXqrVersion = new List<object>();
            List<VersionType> listVersionType = FunctionListBLL.SingleModel.GetVersionTypeList((int)TmpType.小程序专业模板, agentinfo.AgentType);

            if (listVersionType != null && listVersionType.Count > 0)
            {
                foreach (VersionType item in listVersionType)
                {
                    listXqrVersion.Add(new
                    {
                        value = item.VersionId,
                        name = item.VersionName
                    });
                }

                ViewBag.listXqrVersion = JsonConvert.SerializeObject(listXqrVersion);
                #endregion
            }
            OpenConfig(agentinfo);
            return View(agentinfo);
        }

        /// <summary>
        /// 获取地区列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAreaCode()
        {
            int areacode = Context.GetRequestInt("areacode", -1);
            if (areacode < 0)
            {
                return Json(null);
            }
            List<AreaRegion> list = AreaRegionBLL.SingleModel.GetChildAreaList(areacode);
            return Json(list);
        }

        /// <summary>
        /// 保存客户添加
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCustomer()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            //账户信息不从缓存中读取，避免用户数据更新了缓存没有更新导致金额对不上
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModel(agentinfo.id);
            if (agentmodel == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            string username = Context.GetRequest("username", string.Empty);
            string industr = Context.GetRequest("industrselect", string.Empty);
            string tids = Context.GetRequest("tids", string.Empty);
            //多门店分店数量
            int scount = Context.GetRequestInt("scount", 0);
            int zscount = Context.GetRequestInt("zscount", 0);
            //模板有效期
            string years = Context.GetRequest("years", string.Empty);
            //同一个模板购买数量
            string buycount = Context.GetRequest("buycount", string.Empty);
            int province = Context.GetRequestInt("province", 0);
            int city = Context.GetRequestInt("city", 0);
            int area = Context.GetRequestInt("area", 0);
            int industryid = string.IsNullOrEmpty(industr) ? 0 : Convert.ToInt32(industr);
            int companyscaleid = Context.GetRequestInt("companyscaleid", 0);
            string remark = Context.GetRequest("remark", string.Empty);
            string useraccountid = Context.GetRequest("useraccountid", string.Empty);

            if (string.IsNullOrEmpty(username))
            {
                msg.Msg = "请填写客户名称";
                return Json(msg);
            }

            if (username.Length > 28)
            {
                msg.Msg = "用户名称过长";
                return Json(msg);
            }

            if (!Regex.IsMatch(username, @"^[^\s]+$"))
            {
                msg.Msg = "客户名称不能有空格";
                return Json(msg);
            }

            if (string.IsNullOrEmpty(tids))
            {
                msg.Msg = "请选择模板";
                return Json(msg);
            }

            List<XcxTemplate> xcxtemplates = new List<XcxTemplate>();
            string[] tidsarray = tids.Split(',');
            string[] yearsarray = years?.Split(',');
            string[] buycountarray = buycount?.Split(',');

            for (int i = 0; i < tidsarray.Length; i++)
            {
                XcxTemplate model = new XcxTemplate();
                model.Id = Convert.ToInt32(tidsarray[i]);
                model.year = yearsarray.Any() ? Convert.ToInt32(yearsarray.Length > i ? yearsarray[i] : "1") : 1;
                model.buycount = buycountarray.Any() ? Convert.ToInt32(buycountarray.Length > i ? buycountarray[i] : "1") : 1;
                xcxtemplates.Add(model);
            }

            if (string.IsNullOrEmpty(useraccountid))
            {
                msg.Msg = "参数错误，用户账号不存在";
                return Json(msg);
            }

            Account useraccount = AccountBLL.SingleModel.GetModel($"id='{useraccountid}'");
            if (useraccount == null)
            {
                msg.Msg = "用户不存在!";
                return Json(msg);
            }

            if (remark.Length > 200)
            {
                msg.Msg = "备注内容已超过200字";
                return Json(msg);
            }

            List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({tids})", agentmodel.id);
            int sum = 0;
            int parent_sum = 0;
            if (xcxlist != null && xcxlist.Count > 0)
            {
                foreach (XcxTemplate xcxmodel in xcxlist)
                {
                    //判断是否为行业小程序模板
                    xcxmodel.industr = xcxmodel.Type == (int)TmpType.小程序专业模板 ? industr : "";
                    xcxmodel.VersionId = industryid;

                    XcxTemplate tempmodel = xcxtemplates.FirstOrDefault(w => w.Id == xcxmodel.Id);

                    //判断是否为小程序多店铺模板
                    if (xcxmodel.Type == (int)TmpType.小程序多门店模板 || xcxmodel.Type == (int)TmpType.小程序餐饮多门店模板)
                    {
                        xcxmodel.storecount = scount;
                        if (scount <= 0)
                        {
                            xcxmodel.storecount = xcxmodel.SCount;
                        }
                        //判断开通的多门店的分店是否有超过预定值
                        sum += (xcxmodel.storecount - xcxmodel.SCount) * xcxmodel.SPrice * tempmodel.buycount;
                    }
                    else if (xcxmodel.Type == (int)TmpType.智慧餐厅)
                    {
                        xcxmodel.storecount = zscount;
                        if (zscount <= 0)
                        {
                            xcxmodel.storecount = xcxmodel.SCount;
                        }
                        //判断开通的多门店的分店是否有超过预定值
                        sum += (xcxmodel.storecount - xcxmodel.SCount) * xcxmodel.SPrice * tempmodel.buycount;
                    }

                    //有效期
                    xcxmodel.year = tempmodel != null ? tempmodel.year : 1;
                    xcxmodel.buycount = tempmodel.buycount;
                    if (xcxmodel.Type == (int)TmpType.小程序专业模板)
                    {
                        //重新计算价格专业版版本级别
                        //log4net.LogHelper.WriteInfo(this.GetType(), $"代理商=xcxmodel.VersionId=" + xcxmodel.VersionId);
                        VersionType model = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(xcxmodel.Type, agentinfo.id).FirstOrDefault(x => x.VersionId == xcxmodel.VersionId);
                        sum += Convert.ToInt32(model.VersionPrice) * xcxmodel.year * tempmodel.buycount;
                        xcxmodel.Price = Convert.ToInt32(model.VersionPrice);
                        xcxmodel.LimitCount = model.LimitCount;
                    }
                    else
                    {
                        sum += xcxmodel.Price * xcxmodel.year * tempmodel.buycount;
                    }
                }

                if (sum > agentmodel.deposit)
                {
                    msg.Msg = "您的预存款不足";
                    return Json(msg);
                }

                if (agentmodel.userLevel == 1)
                {
                    Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentmodel.id);
                    Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                    List<XcxTemplate> parent_xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({tids})", distribution.parentAgentId);
                    if (parent_xcxlist != null && parent_xcxlist.Count > 0)
                    {
                        foreach (XcxTemplate xcxmodel in parent_xcxlist)
                        {
                            //判断是否为行业小程序模板
                            xcxmodel.industr = xcxmodel.Type == (int)TmpType.小程序专业模板 ? industr : "";
                            xcxmodel.VersionId = industryid;
                            //判断是否为小程序多店铺模板
                            if (xcxmodel.Type == (int)TmpType.小程序多门店模板 || xcxmodel.Type == (int)TmpType.小程序餐饮多门店模板 || xcxmodel.Type == (int)TmpType.智慧餐厅)
                            {
                                xcxmodel.storecount = scount;
                                if (scount <= 0)
                                {
                                    xcxmodel.storecount = xcxmodel.SCount;
                                }
                                //判断开通的多门店的分店是否有超过预定值
                                parent_sum += (xcxmodel.storecount - xcxmodel.SCount) * xcxmodel.SPrice;
                            }

                            XcxTemplate tempmodel = xcxtemplates.FirstOrDefault(f => f.Id == xcxmodel.Id);
                            //有效期
                            xcxmodel.year = tempmodel != null ? tempmodel.year : 1;
                            xcxmodel.buycount = tempmodel.buycount;
                            if (xcxmodel.Type == (int)TmpType.小程序专业模板)
                            {
                                //重新计算价格专业版版本级别
                                //log4net.LogHelper.WriteInfo(this.GetType(), $"分销商=xcxmodel.VersionId=" + xcxmodel.VersionId);
                                VersionType model = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(xcxmodel.Type, distribution.parentAgentId).FirstOrDefault(x => x.VersionId == xcxmodel.VersionId);
                                parent_sum += Convert.ToInt32(model.VersionPrice) * xcxmodel.year * tempmodel.buycount;
                            }
                            else
                            {
                                parent_sum += xcxmodel.Price * xcxmodel.year * tempmodel.buycount;
                            }
                        }

                        if (parent_sum > parentAgentinfo.deposit)
                        {
                            msg.Msg = $"创建失败，请联系上级代理";
                            return Json(msg);
                        }
                    }
                    else
                    {
                        msg.Msg = "数据错误！";
                        return Json(msg);
                    }
                }
                try
                {
                    string erromsg = "";
                    msg.isok = _agentCustomerRelationBLL.AddCustomer(username, province, city, area, industryid, companyscaleid, remark, agentmodel, xcxlist, sum, parent_sum, useraccount, ref erromsg);
                    msg.Msg = string.IsNullOrEmpty(erromsg) ? (msg.isok ? "添加成功" : "添加失败") : erromsg;
                }
                catch (Exception ex)
                {
                    msg.Msg = ex.Message;
                }
                return Json(msg);
            }
            else
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
        }

        public ActionResult AddCustomerV2(string username = "", string industr = "", int province = 0, int city = 0, int area = 0, int companyscaleid = 0, string remark = "", string useraccountid = "", List<XcxTemplate> xcxtemplates = null)
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            //账户信息不从缓存中读取，避免用户数据更新了缓存没有更新导致金额对不上
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModel(agentinfo.id);
            if (agentmodel == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            int industryid = string.IsNullOrEmpty(industr) ? 0 : Convert.ToInt32(industr);

            if (string.IsNullOrEmpty(username))
            {
                msg.Msg = "请填写客户名称";
                return Json(msg);
            }
            if (username.Length > 28)
            {
                msg.Msg = "用户名称过长";
                return Json(msg);
            }
            if (!Regex.IsMatch(username, @"^[^\s]+$"))
            {
                msg.Msg = "客户名称不能有空格";
                return Json(msg);
            }

            if (string.IsNullOrEmpty(useraccountid))
            {
                msg.Msg = "参数错误，用户账号不存在";
                return Json(msg);
            }

            Account useraccount = AccountBLL.SingleModel.GetModel($"id='{useraccountid}'");
            if (useraccount == null)
            {
                msg.Msg = "用户不存在!";
                return Json(msg);
            }

            if (remark.Length > 200)
            {
                msg.Msg = "备注内容已超过200字";
                return Json(msg);
            }

            if (xcxtemplates == null || xcxtemplates.Count <= 0)
            {
                msg.Msg = "请选择模板";
                return Json(msg);
            }
            string tids = string.Join(",", xcxtemplates.Select(s => s.Id));
            List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({tids})", agentmodel.id);
            int sum = 0;
            int parent_sum = 0;
            if (xcxlist != null && xcxlist.Count > 0)
            {
                foreach (XcxTemplate xcxmodel in xcxlist)
                {
                    //判断是否为行业小程序模板
                    xcxmodel.industr = xcxmodel.Type == (int)TmpType.小程序专业模板 ? industr : "";
                    xcxmodel.VersionId = industryid;

                    XcxTemplate tempmodel = xcxtemplates.FirstOrDefault(f => f.Id == xcxmodel.Id);

                    //判断是否为小程序多店铺模板
                    if (xcxmodel.Type == (int)TmpType.小程序多门店模板 || xcxmodel.Type == (int)TmpType.小程序餐饮多门店模板 || xcxmodel.Type == (int)TmpType.智慧餐厅 || xcxmodel.Type == (int)TmpType.企业智推版)
                    {
                        xcxmodel.storecount = tempmodel.storecount;
                        if (tempmodel.storecount <= 0)
                        {
                            xcxmodel.storecount = xcxmodel.SCount;
                        }
                        //判断开通的多门店的分店是否有超过预定值
                        sum += (xcxmodel.storecount - xcxmodel.SCount) * xcxmodel.SPrice * tempmodel.buycount;
                    }

                    //有效期
                    xcxmodel.year = tempmodel != null ? tempmodel.year : 1;
                    xcxmodel.buycount = tempmodel.buycount;
                    if (xcxmodel.Type == (int)TmpType.小程序专业模板)
                    {
                        //重新计算价格专业版版本级别
                        //log4net.LogHelper.WriteInfo(this.GetType(), $"代理商=xcxmodel.VersionId=" + xcxmodel.VersionId);
                        VersionType model = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(xcxmodel.Type, agentinfo.id).FirstOrDefault(x => x.VersionId == xcxmodel.VersionId);
                        sum += Convert.ToInt32(model.VersionPrice) * xcxmodel.year * tempmodel.buycount;
                        xcxmodel.Price = Convert.ToInt32(model.VersionPrice);
                        xcxmodel.LimitCount = model.LimitCount;
                    }
                    else
                    {
                        sum += xcxmodel.Price * xcxmodel.year * tempmodel.buycount;
                    }
                }

                if (sum > agentmodel.deposit)
                {
                    msg.Msg = "您的预存款不足";
                    return Json(msg);
                }

                if (agentmodel.userLevel == 1)
                {
                    Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentmodel.id);
                    Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                    List<XcxTemplate> parent_xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({tids})", distribution.parentAgentId);
                    if (parent_xcxlist != null && parent_xcxlist.Count > 0)
                    {
                        foreach (XcxTemplate xcxmodel in parent_xcxlist)
                        {
                            XcxTemplate tempmodel = xcxtemplates.FirstOrDefault(f => f.Id == xcxmodel.Id);
                            //判断是否为行业小程序模板
                            xcxmodel.industr = xcxmodel.Type == (int)TmpType.小程序专业模板 ? industr : "";
                            xcxmodel.VersionId = industryid;
                            //判断是否为小程序多店铺模板
                            if (xcxmodel.Type == (int)TmpType.小程序多门店模板 || xcxmodel.Type == (int)TmpType.小程序餐饮多门店模板 || xcxmodel.Type == (int)TmpType.智慧餐厅 || xcxmodel.Type == (int)TmpType.企业智推版)
                            {
                                xcxmodel.storecount = tempmodel.storecount;
                                if (xcxmodel.storecount <= 0)
                                {
                                    xcxmodel.storecount = xcxmodel.SCount;
                                }
                                //判断开通的多门店的分店是否有超过预定值
                                parent_sum += (xcxmodel.storecount - xcxmodel.SCount) * xcxmodel.SPrice;
                            }

                            //有效期
                            xcxmodel.year = tempmodel != null ? tempmodel.year : 1;
                            xcxmodel.buycount = tempmodel.buycount;
                            if (xcxmodel.Type == (int)TmpType.小程序专业模板)
                            {
                                //重新计算价格专业版版本级别
                                //log4net.LogHelper.WriteInfo(this.GetType(), $"分销商=xcxmodel.VersionId=" + xcxmodel.VersionId);
                                VersionType model = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(xcxmodel.Type, distribution.parentAgentId).FirstOrDefault(x => x.VersionId == xcxmodel.VersionId);
                                parent_sum += Convert.ToInt32(model.VersionPrice) * xcxmodel.year * tempmodel.buycount;
                            }
                            else
                            {
                                parent_sum += xcxmodel.Price * xcxmodel.year * tempmodel.buycount;
                            }
                        }

                        if (parent_sum > parentAgentinfo.deposit)
                        {
                            msg.Msg = $"创建失败，请联系上级代理";
                            return Json(msg);
                        }
                    }
                    else
                    {
                        msg.Msg = "数据错误！";
                        return Json(msg);
                    }
                }
                try
                {
                    string erromsg = "";
                    msg.isok = _agentCustomerRelationBLL.AddCustomer(username, province, city, area, industryid, companyscaleid, remark, agentmodel, xcxlist, sum, parent_sum, useraccount, ref erromsg);
                    msg.Msg = string.IsNullOrEmpty(erromsg) ? (msg.isok ? "添加成功" : "添加失败") : erromsg;
                }
                catch (Exception ex)
                {
                    msg.Msg = ex.Message;
                }
                return Json(msg);
            }
            else
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
        }

        public ActionResult AddCustomerV3(string username = "", string industr = "", int province = 0, int city = 0, int area = 0, int companyscaleid = 0, string remark = "", string useraccountid = "", List<XcxTemplate> xcxtemplates = null)
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            //账户信息不从缓存中读取，避免用户数据更新了缓存没有更新导致金额对不上
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModel(agentinfo.id);
            if (agentmodel == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            int industryid = string.IsNullOrEmpty(industr) ? 0 : Convert.ToInt32(industr);

            if (string.IsNullOrEmpty(username))
            {
                msg.Msg = "请填写客户名称";
                return Json(msg);
            }
            if (username.Length > 28)
            {
                msg.Msg = "用户名称过长";
                return Json(msg);
            }
            if (!Regex.IsMatch(username, @"^[^\s]+$"))
            {
                msg.Msg = "客户名称不能有空格";
                return Json(msg);
            }

            if (string.IsNullOrEmpty(useraccountid))
            {
                msg.Msg = "参数错误，用户账号不存在";
                return Json(msg);
            }

            Account useraccount = AccountBLL.SingleModel.GetModel($"id='{useraccountid}'");
            if (useraccount == null)
            {
                msg.Msg = "用户不存在!";
                return Json(msg);
            }

            if (remark.Length > 200)
            {
                msg.Msg = "备注内容已超过200字";
                return Json(msg);
            }

            if (xcxtemplates == null || xcxtemplates.Count <= 0)
            {
                msg.Msg = "请选择模板";
                return Json(msg);
            }

            int sum = 0;
            int parent_sum = 0;
            string errorMsg = "";
            //检验代理预存
            List<XcxTemplate> xcxlist = AgentinfoBLL.SingleModel.CheckParentAgentDeposit(agentmodel.id, industryid, industr, xcxtemplates, ref sum, ref errorMsg);
            if (errorMsg.Length > 0)
            {
                msg.Msg = errorMsg;
                return Json(msg);
            }

            Distribution distribution = null;
            Agentinfo pagentInfo = new Agentinfo();
            List<XcxTemplate> pxcxList = new List<XcxTemplate>();
            //分销商：检验上级代理预存
            if (agentmodel.userLevel == 1)
            {
                distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentmodel.id);
                pagentInfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                pxcxList = AgentinfoBLL.SingleModel.CheckParentAgentDeposit(distribution.parentAgentId, industryid, industr, xcxtemplates, ref parent_sum, ref errorMsg);
                if (errorMsg.Length > 0)
                {
                    msg.Msg = errorMsg;
                    return Json(msg);
                }
            }

            //开通模板
            try
            {
                string erromsg = "";
                //msg.isok = _agentCustomerRelationBLL.AddCustomer(username, province, city, area, industryid, companyscaleid, remark, agentmodel, xcxlist, sum, parent_sum, useraccount, ref erromsg);
                msg.isok = _agentCustomerRelationBLL.AddCustomerV2(username, province, city, area,  companyscaleid, remark, agentmodel, xcxlist, sum, parent_sum, useraccount, distribution, pagentInfo, pxcxList,ref erromsg);
                msg.Msg = string.IsNullOrEmpty(erromsg) ? (msg.isok ? "添加成功" : "添加失败") : erromsg;
            }
            catch (Exception ex)
            {
                msg.Msg = ex.Message;
            }

            return Json(msg);
        }

        /// <summary>
        /// 修改客户资料界面
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerEdit()
        {
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                return Redirect("/dzhome/login");
            }
            AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModelByIdAndAgentId(id,agentinfo.id);
            if (customer == null)
            {
                return Redirect("/dzhome/login");
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{customer.useraccountid}'");
            if (account == null)
            {
                return Redirect("/dzhome/login");
            }
            ViewBag.type = Context.GetRequest("type", string.Empty);//根据type显示编辑界面不同标签页
            List<XcxAppAccountRelation> appAccountRelationList = XcxAppAccountRelationBLL.SingleModel.GetListByAccountId(customer.useraccountid,agentinfo.id);
            List<XcxTemplate> sel_templateList = null;
            string ids = string.Empty;
            if (appAccountRelationList != null && appAccountRelationList.Count > 0)
            {
                sel_templateList = new List<XcxTemplate>();
                foreach (XcxAppAccountRelation appAccount in appAccountRelationList)
                {
                    XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($" id = {appAccount.TId} and (projectType in ({(int)ProjectType.小程序},{(int)ProjectType.测试}) {(ceshitype.Length > 0 ? $" or type in ({ceshitype})" : "")})");
                    if (xcxTemplate != null)
                    {
                        if (xcxTemplate.Type == 22)
                        {
                            xcxTemplate.TName += AgentdepositLogBLL.SingleModel.GetVerName(appAccount.VersionId);
                        }

                        xcxTemplate.Price = appAccount.price;
                        xcxTemplate.year = appAccount.TimeLength;
                        xcxTemplate.industr = appAccount.Industr;
                        xcxTemplate.AddTimeStr = appAccount.AddTime.ToString("yyyy-MM-dd HH:mm:ss");
                        xcxTemplate.outtime = appAccount.outtime.ToString("yyyy-MM-dd HH:mm:ss");
                        xcxTemplate.State = appAccount.State;
                        xcxTemplate.statename = appAccount.State < 0 ? "禁用" : "正常";
                        xcxTemplate.VersionId = appAccount.VersionId;
                        sel_templateList.Add(xcxTemplate);
                        ids += xcxTemplate.Id + ",";
                    }
                }
            }
            //获取未开通模板列表
            string sqlwhere = $"state=0 and projectType={(int)ProjectType.小程序} {(ceshitype.Length > 0 ? $" or type in({ceshitype} )" : "")}";
            if (!string.IsNullOrEmpty(ids))
            {
                sqlwhere += $" and id not in ({ids.TrimEnd(',')})";
            }
            List<XcxTemplate> templateList = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList(sqlwhere, agentinfo.id);

            CustomerModel model = new CustomerModel()
            {
                //addtimeTostring = info.addtime.ToString("yyyy-MM-dd HH:mm:ss"),
                agentid = customer.agentid,
                agentName = agentinfo.name,
                id = customer.id,
                LoginId = account.LoginId,
                //model.phone = account.ConsigneePhone;
                state = customer.state,
                //Totalcost = xcxRelationlist.Sum(x => x.Price),
                useraccountid = customer.useraccountid,
                username = customer.username,
                //pwd = LixSecurity.DESDecrypt(account.Password),
                sel_templateList = sel_templateList,
                templateList = templateList,
                provincecode = customer.provincecode,
                citycode = customer.citycode,
                areacode = customer.areacode,
                industryid = customer.industryid,
                companyscaleid = customer.companyscaleid,
                remark = customer.remark
            };

            //绑定模板消息接收人
            model.MsgAccounts = MsgAccountBLL.SingleModel.GetListByManagerAgent(agentinfo.id, account.Id);

            //扫码绑定代码
            string sessonid = string.Empty;
            if (model.MsgAccounts?.Count < 5)
            {
                sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
                Session["qrcodekey"] = sessonid;
                if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
                {
                    LoginQrCode wxkey = new LoginQrCode();
                    wxkey.SessionId = sessonid;
                    wxkey.IsLogin = false;
                    RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
                }
                model.SessionId = sessonid;
            }

            //return Content(JsonConvert.SerializeObject(model)+"||||||"+sqlwhere);
            return View(model);
        }

        /// <summary>
        /// 保存修改客户资料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult saveEdit()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int id = Context.GetRequestInt("id", 0);
            string industr = Context.GetRequest("industrselect", string.Empty);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModelByIdAndAgentId(id, agentinfo.id);
            if (customer == null)
            {
                msg.Msg = "用户不存在";
                return Json(msg);
            }
            Account account = AccountBLL.SingleModel.GetModel($"id ='{customer.useraccountid}'");
            if (account == null)
            {
                msg.Msg = "用户不存在!";
                return Json(msg);
            }
            string username = Context.GetRequest("username", string.Empty);
            if (string.IsNullOrEmpty(username))
            {
                msg.Msg = "请填写客户名称";
                return Json(msg);
            }
            if (username.Length > 28)
            {
                msg.Msg = "客户名称过长";
                return Json(msg);
            }
            if (!Regex.IsMatch(username, @"^[^\s]+$"))
            {
                msg.Msg = "客户名称不能有空格";
                return Json(msg);
            }
            //string pwd = Context.GetRequest("pwd", string.Empty);

            string tids = Context.GetRequest("tids", string.Empty);
            string years = Context.GetRequest("years", string.Empty);
            //有效期
            string[] tidsarray = tids?.Split(',');
            //同一个模板购买数量
            string buycount = Context.GetRequest("buycount", string.Empty);
            List<XcxTemplate> xcxtemplates = new List<XcxTemplate>();
            string[] yearsarray = years?.Split(',');
            string[] buycountarray = buycount?.Split(',');
            for (int i = 0; i < tidsarray.Length; i++)
            {
                if (string.IsNullOrEmpty(tidsarray[i]))
                {
                    continue;
                }
                XcxTemplate model = new XcxTemplate();
                model.Id = Convert.ToInt32(tidsarray[i]);
                model.year = yearsarray.Any() ? Convert.ToInt32(yearsarray.Length > i ? yearsarray[i] : "1") : 1;
                model.buycount = buycountarray.Any() ? Convert.ToInt32(buycountarray.Length > i ? buycountarray[i] : "1") : 1;
                xcxtemplates.Add(model);
            }
            int province = Context.GetRequestInt("province", 0);
            int city = Context.GetRequestInt("city", 0);
            int area = Context.GetRequestInt("area", 0);
            int industryid = Context.GetRequestInt("industryid", 0);
            int companyscaleid = Context.GetRequestInt("companyscaleid", 0);
            string remark = Context.GetRequest("remark", string.Empty);
            if (remark.Length > 200)
            {
                msg.Msg = "备注内容已超过200字";
                return Json(msg);
            }
            List<XcxTemplate> xcxlist = null;
            int sum = 0;
            int parent_sum = 0;
            string errorMsg = "";
            if (!string.IsNullOrEmpty(tids))
            {
                //检验代理预存
                xcxlist = AgentinfoBLL.SingleModel.CheckParentAgentDeposit(agentinfo.id, industryid, industr, xcxtemplates, ref sum, ref errorMsg);
                if (errorMsg.Length > 0)
                {
                    msg.Msg = errorMsg;
                    return Json(msg);
                }
                //如果是二级代理（分销商）,扣费同时要扣一级代理的预存款
                if (agentinfo.userLevel == 1)
                {
                    Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentinfo.id);
                    AgentinfoBLL.SingleModel.CheckParentAgentDeposit(distribution.parentAgentId, industryid, industr, xcxtemplates, ref parent_sum, ref errorMsg);
                    if (errorMsg.Length > 0)
                    {
                        msg.Msg = errorMsg;
                        return Json(msg);
                    }
                }
            }
            string erromsg = "";
            msg.isok = _agentCustomerRelationBLL.EditCustomer(username, province, city, area, industryid, companyscaleid, remark, agentinfo, xcxlist, sum, parent_sum, customer, account, ref erromsg);
            msg.Msg = string.IsNullOrEmpty(erromsg) ? (msg.isok ? "保存成功" : "保存失败") : erromsg;
            return Json(msg);
        }

        /// <summary>
        /// 修改账号状态
        /// </summary>
        /// <returns></returns>
        public ActionResult updatestate()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int id = Context.GetRequestInt("id", 0);
            int state = Context.GetRequestInt("state", 0);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModelByIdAndAgentId(id, agentinfo.id);
            if (customer == null)
            {
                msg.Msg = "用户不存在";
                return Json(msg);
            }
            customer.state = state;
            customer.updatetime = DateTime.Now;

            string updateSql = $"update XcxAppAccountRelation set state={state} where accountid='{customer.useraccountid}' and agentid={agentinfo.id}";

            msg.isok = _agentCustomerRelationBLL.Update(customer) && _agentCustomerRelationBLL.ExecuteNonQuery(updateSql) > 0;
            //删除缓存
            XcxAppAccountRelationBLL.SingleModel.RemoveVersion(agentinfo.id);
            msg.Msg = msg.isok ? "操作成功" : "操作失败";
            return Json(msg);
        }

        /// <summary>
        /// 修改密码界面
        /// </summary>
        /// <returns></returns>
        public ActionResult updatePwd()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModelByIdAndAgentId(id, agentinfo.id);
            if (customer == null)
            {
                msg.Msg = "用户不存在";
                return Json(msg);
            }
            return View(customer);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult savePwd()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModelByIdAndAgentId(id, agentinfo.id);
            if (customer == null)
            {
                msg.Msg = "用户不存在";
                return Json(msg);
            }
            Account account = AccountBLL.SingleModel.GetModel($" id ='{customer.useraccountid}'");
            if (account == null)
            {
                msg.Msg = "用户不存在!";
                return Json(msg);
            }
            string pwd = Context.GetRequest("pwd", string.Empty);
            if (string.IsNullOrEmpty(pwd))
            {
                msg.Msg = "请输入密码";
                return Json(msg);
            }
            if (pwd.Length < 6)
            {
                msg.Msg = "密码不能少于6位";
                return Json(msg);
            }
            if (pwd.Length > 20)
            {
                msg.Msg = "密码太长啦!";
                return Json(msg);
            }
            if (!Regex.IsMatch(pwd, @"^[a-zA-Z\d]+$"))
            {
                msg.Msg = "只能用数字或者字母作为密码";
                return Json(msg);
            }
            string pwd_again = Context.GetRequest("pwd_again", string.Empty);
            if (pwd != pwd_again)
            {
                msg.Msg = "密码不一致";
                return Json(msg);
            }
            if (account.Password == DESEncryptTools.GetMd5Base32(pwd))
            {
                msg.isok = true;
                msg.Msg = "修改成功";
                return Json(msg);
            }
            else
            {
                account.Password = DESEncryptTools.GetMd5Base32(pwd);
                msg.isok = AccountBLL.SingleModel.Update(account);
                msg.Msg = msg.isok ? "修改成功" : "修改失败";
                return Json(msg);
            }
        }

        #region 换绑账号
        /// <summary>
        /// 换绑账号
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeUserInfo()
        {
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0 && dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            else if (id > 0 && agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }
            ViewBag.customerid = id;
            //微信二维码
            GetQrdCode();

            return View();
        }

        /// <summary>
        /// 保存换绑账号
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveUserInfo()
        {
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0 && dzaccount == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            else if (id > 0 && agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            string savekey = Context.GetRequest("savekey", string.Empty);
            if (savekey == null || savekey.Length <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }

            try
            {
                string accountid = RedisUtil.Get<string>(_dzsavekey + savekey);
                if (accountid == null || accountid.Length == 0)
                {
                    msg.Msg = "已过期，请刷新重试";
                    return Json(msg);
                }
                Account accountmodel = AccountBLL.SingleModel.GetModelByStringId(accountid);
                if (accountmodel == null)
                {
                    msg.Msg = "找不到用户，请换一个账号试试";
                    return Json(msg);
                }

                string oldaccountid = "";
                AgentCustomerRelation customer = null;
                if (id > 0)
                {
                    customer = _agentCustomerRelationBLL.GetModel(id);
                    if (customer == null)
                    {
                        msg.Msg = "找不到关联用户，请刷新重试";
                        return Json(msg);
                    }
                    oldaccountid = customer.useraccountid;
                }
                else
                {
                    oldaccountid = dzaccount.Id.ToString();
                }

                string erromsg = "";
                msg.isok = _agentCustomerRelationBLL.UpdateCustomerLoginInfo(accountmodel, customer, oldaccountid, ref erromsg);
                if (erromsg != "")
                {
                    msg.Msg = erromsg;
                    msg.isok = false;
                }
                msg.Msg = msg.isok ? "换绑成功" : "换绑失败";
            }
            catch (Exception ex)
            {
                msg.Msg = ex.Message;
            }

            return Json(msg);
        }

        /// <summary>
        /// 客户扫描二维码
        /// </summary>
        /// <param name="wxkey"></param>
        /// <returns></returns>
        public ActionResult ChangeUserlogin(string wxkey)
        {
            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
            int type = Context.GetRequestInt("type", 0);//1:第一步，旧帐号扫描，2：第二步，新账号扫描
            int id = Context.GetRequestInt("id", 0);//id大于0为代理商操作更换账号，等于0为用户操作更换账号

            msg.code = "-1";
            if (string.IsNullOrEmpty(wxkey))
            {
                return Json(msg);
            }
            if (lcode == null)
            {
                return Json(msg);
            }
            if (!lcode.IsLogin)
            {
                return Json(msg);
            }
            if (lcode.WxUser == null)
            {
                return Json(msg);
            }

            Account accountmodel = null;
            if (string.IsNullOrEmpty(lcode.WxUser.openid) || string.IsNullOrEmpty(lcode.WxUser.unionid))
            {
                msg.code = "-2";
                msg.Msg = "无法获取微信信息，请刷新重试";
                return Json(msg);
            }
            
            UserBaseInfo userInfo = UserBaseInfoBLL.SingleModel.GetModelByOpenId(lcode.WxUser.openid, lcode.WxUser.serverid);
            if (userInfo == null)
            {
                userInfo = new UserBaseInfo();
                userInfo.openid = lcode.WxUser.openid;
                userInfo.nickname = lcode.WxUser.nickname;
                userInfo.headimgurl = lcode.WxUser.headimgurl;
                userInfo.sex = lcode.WxUser.sex;
                userInfo.country = lcode.WxUser.country;
                userInfo.city = lcode.WxUser.city;
                userInfo.province = lcode.WxUser.province;
                userInfo.unionid = lcode.WxUser.unionid;
                userInfo.serverid = lcode.WxUser.serverid;
                UserBaseInfoBLL.SingleModel.Add(userInfo);
            }
            accountmodel = AccountBLL.SingleModel.GetAccountByWeixinUser(lcode.WxUser);
            if (accountmodel == null)
            {
                return Json(msg);
            }
            
            Member member = MemberBLL.SingleModel.GetMemberByAccountId(accountmodel.Id.ToString());
            member.LastModified = DateTime.Now;//记录登录时间
            MemberBLL.SingleModel.Update(member);
            RedisUtil.Remove("SessionID:" + wxkey);

            AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModel(id);
            if (id > 0 && customer == null)
            {
                msg.code = "-2";
                msg.Msg = "找不到用户";
                return Json(msg);
            }
            //微信二维码
            string sessionkey = GetQrdCode();
            msg.dataObj = sessionkey;

            if (type == 1)//老账号
            {
                if ((id > 0 && accountmodel.Id.ToString() != customer.useraccountid) || (id <= 0 && dzaccount != null && dzaccount.Id.ToString() != accountmodel.Id.ToString()))
                {
                    msg.code = "-2";
                    msg.Msg = "账号不对";
                    return Json(msg);
                }
                else
                {
                    msg.isok = true;
                    return Json(msg);
                }
            }
            else if (type == 2)//新账号
            {
                msg.isok = true;
                string key = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
                RedisUtil.Set<string>(_dzsavekey + key, accountmodel.Id.ToString(), TimeSpan.FromHours(1));
                msg.dataObj = new { savelogin = accountmodel.LoginId, savekey = key };
                msg.Msg = msg.isok ? "换绑成功" : "换绑失败";
            }

            return Json(msg);
        }
        #endregion

        [HttpPost]
        public ActionResult BindMsgAccount(string code = null, int id = 0)
        {
            if (string.IsNullOrWhiteSpace(code) || id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }

            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + code);
            if (lcode == null)
            {
                return Json(new { success = false, msg = "-2" });
            }
            if (!lcode.IsLogin)
            {
                return Json(new { success = false, msg = "-3" });
            }

            if (lcode.WxUser == null)
            {
                return Json(new { success = false, msg = "-5" });
            }

            Agentinfo agentInfo = agentinfo;
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModelByIdAndAgentId(id, agentinfo.id);
            if (customer == null)
            {
                msg.Msg = "无效ID";
                return Json(msg);
            }

            Account account = AccountBLL.SingleModel.GetModel($"id='{customer.useraccountid}'");
            if (account == null)
            {
                msg.Msg = "无效客户账号";
                return Json(msg);
            }
            
            bool result = MsgAccountBLL.SingleModel.UpdateMsgAccount(agentInfo.id, account.Id, lcode.WxUser);
            if (result)
            {
                RedisUtil.Remove($"SessionID:{code}");
            }
            msg.isok = result;
            return Json(msg);
        }

        [HttpPost]
        public ActionResult UpdateMsgAccount(int id = 0, int msgId = 0)
        {
            if (id <= 0 || msgId <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }

            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModelByIdAndAgentId(id, agentinfo.id);
            if (customer == null)
            {
                msg.Msg = "无效ID";
                return Json(msg);
            }

            Account account = AccountBLL.SingleModel.GetModel($"id='{customer.useraccountid}'");
            if (account == null)
            {
                msg.Msg = "无效客户账号";
                return Json(msg);
            }

            bool result = false;
            
            MsgAccount msgAccount = MsgAccountBLL.SingleModel.GetModel(msgId);
            if (msgAccount.Agentid == agentinfo.id && msgAccount.ManagerGuid == account.Id && msgAccount.State == 0)
            {
                result = MsgAccountBLL.SingleModel.UpdateToUnBind(msgAccount);
            }

            msg.isok = result;
            return Json(msg);
        }

        /// <summary>
        /// 管理小程序
        /// </summary>
        /// <returns></returns>
        public ActionResult TurnToMiniApp()
        {
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                return Redirect("/dzhome/login");
            }
            AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModelByIdAndAgentId(id, agentinfo.id);
            if (customer == null)
            {
                return Redirect("/dzhome/login");
            }
            Account account = AccountBLL.SingleModel.GetModel($"id ='{customer.useraccountid}'");
            if (account == null)
            {
                return Redirect("/dzhome/login");
            }
            return Redirect("/dzhome/casetemplate");
        }

        /// <summary>
        /// 客户扫描二维码
        /// </summary>
        /// <param name="wxkey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult wxlogin(string wxkey)
        {
            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
            int type = Context.GetRequestInt("type", 0);//0:小程序客户扫码  1:分销商扫码 

            if (string.IsNullOrEmpty(wxkey))
            {
                msg.Msg = "-1";
                return Json(msg);
            }
            if (lcode == null)
            {
                msg.Msg = "-1";
                return Json(msg);
            }
            if (!lcode.IsLogin)
            {
                msg.Msg = "-1";
                return Json(msg);
            }

            if (lcode.WxUser != null)
            {
                Account accountmodel = null;
                if (!string.IsNullOrEmpty(lcode.WxUser.openid) && !string.IsNullOrEmpty(lcode.WxUser.unionid))
                {
                    
                    UserBaseInfo userInfo = UserBaseInfoBLL.SingleModel.GetModelByOpenId(lcode.WxUser.openid, lcode.WxUser.serverid);
                    if (userInfo == null)
                    {
                        userInfo = new UserBaseInfo();
                        userInfo.openid = lcode.WxUser.openid;
                        userInfo.nickname = lcode.WxUser.nickname;
                        userInfo.headimgurl = lcode.WxUser.headimgurl;
                        userInfo.sex = lcode.WxUser.sex;
                        userInfo.country = lcode.WxUser.country;
                        userInfo.city = lcode.WxUser.city;
                        userInfo.province = lcode.WxUser.province;
                        userInfo.unionid = lcode.WxUser.unionid;
                        userInfo.serverid = lcode.WxUser.serverid;
                        UserBaseInfoBLL.SingleModel.Add(userInfo);
                    }
                    accountmodel = AccountBLL.SingleModel.GetAccountByWeixinUser(lcode.WxUser);
                    if (accountmodel == null)
                    {
                        msg.Msg = "-1";
                        return Json(msg);
                    }
                    
                    Member member = MemberBLL.SingleModel.GetModel(string.Format("AccountId ='{0}'", accountmodel.Id.ToString()));
                    member.LastModified = DateTime.Now;//记录登录时间
                    MemberBLL.SingleModel.Update(member);
                    RedisUtil.Remove("SessionID:" + wxkey);
                    if (type == 0)//验证是否老客户
                    {
                        AgentCustomerRelation customer = _agentCustomerRelationBLL.GetModel($" useraccountid='{accountmodel.Id}' and agentid={agentinfo.id}");
                        if (customer == null)
                        {
                            customer = new AgentCustomerRelation() { username = "" };
                        }
                        msg.Msg = accountmodel.Id.ToString();
                        msg.dataObj = customer;
                    }
                    else//验证是否老分销商
                    {
                        Distribution distribution = DistributionBLL.SingleModel.GetModel($"useraccountid='{accountmodel.Id}' and parentAgentId={agentinfo.id}");
                        if (distribution != null)
                        {
                            msg.Msg = "olddistribution";

                        }
                        else
                        {
                            msg.Msg = accountmodel.Id.ToString();
                        }
                    }
                    msg.isok = true;
                    return Json(msg);
                }
            }
            else
            {
                msg.Msg = "-1";
                return Json(msg);
            }
            msg.Msg = "-1";
            return Json(msg);
        }

        #region 已开通模板
        /// <summary>
        /// 已开通模板
        /// </summary>
        /// <returns></returns>
        public ActionResult OpenTemplate()
        {
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }
            OpenConfig(agentinfo);
            return View(agentinfo);
        }

        public ActionResult GetOpenTemplateList()
        {
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }

            int pagesize = Context.GetRequestInt("pagesize", 10);
            int pageindex = Context.GetRequestInt("pageindex", 1);
            string LoginId = Context.GetRequest("loginname", string.Empty);
            string username = Context.GetRequest("username", string.Empty);
            string outtime = Context.GetRequest("outtime", string.Empty);
            string templatename = Context.GetRequest("templatename", string.Empty);
            if (!string.IsNullOrEmpty(outtime))
            {
                outtime = DateTime.Now.AddDays(Convert.ToInt32(outtime)).ToString("yyyy-MM-dd HH:mm:ss");
            }
            int aid = CustomModelRelationBLL.SingleModel.GetUpGradeAid(agentinfo.id);

            string errormsg = string.Empty;
            int count = 0;
            List<OpenTemplateView> customerList = _agentCustomerRelationBLL.GetCustomerOpenTemplateList(aid, agentinfo.id, LoginId, username, outtime, templatename, pagesize, pageindex, ref count, ceshitype);

            msg.isok = count >= 0;
            msg.Msg = errormsg;
            msg.dataObj = new
            {
                recordCount = count,
                customerList = customerList,
                aid = aid,
            };
            return Json(msg);
        }

        /// <summary>
        /// 修改账号状态
        /// </summary>
        /// <returns></returns>
        public ActionResult updatetemplatestate()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int id = Context.GetRequestInt("id", 0);
            int state = Context.GetRequestInt("state", 0);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel($"id={id} and agentid={agentinfo.id}");
            if (xrelationmodel == null)
            {
                msg.Msg = "用户模板不存在";
                return Json(msg);
            }
            xrelationmodel.State = state;
            msg.isok = XcxAppAccountRelationBLL.SingleModel.Update(xrelationmodel);
            msg.Msg = msg.isok ? "操作成功" : "操作失败";
            return Json(msg);
        }

        /// <summary>
        /// 续费
        /// </summary>
        /// <returns></returns>
        public ActionResult addtimelengthV1()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int id = Context.GetRequestInt("id", 0);
            int years = Context.GetRequestInt("years", 0);
            int tid = Context.GetRequestInt("tid", 0);
            string username = Context.GetRequest("username", string.Empty);
            string tname = Context.GetRequest("tname", string.Empty);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            if (years <= 0)
            {
                msg.Msg = "年限错误";
                return Json(msg);
            }
            
            if (agentinfo.userLevel == 1)
            {
                XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
                if (xrelationmodel == null)
                {
                    msg.isok = false;
                    msg.Msg = $"分销商的用户模板不存在";
                    return Json(msg);
                }
                Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentinfo.id);
                Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                List<XcxTemplate> parent_xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", distribution.parentAgentId);
                if (parent_xcxlist != null && parent_xcxlist.Count > 0)
                {
                    #region 判断代理商合同是否已过期，已过期不给开免费版
                    if (AgentinfoBLL.SingleModel.CheckOutTime(ref parent_xcxlist, parentAgentinfo, xrelationmodel.VersionId, ref tname))
                    {
                        msg.Msg = tname;
                        return Json(msg);
                    }
                    #endregion

                    int parent_sum = 0;
                    foreach (XcxTemplate xcxmodel in parent_xcxlist)
                    {
                        parent_sum += xcxmodel.Price * years;
                    }
                    if (parent_sum > parentAgentinfo.deposit)
                    {
                        msg.isok = false;
                        msg.Msg = $"创建失败，请联系上级代理";
                        return Json(msg);
                    }
                }
                else
                {
                    msg.isok = false;
                    msg.Msg = "没有找到模板！";
                    return Json(msg);
                }
            }
            string remsg = "";
            try
            {
                msg.isok = _agentCustomerRelationBLL.AddTimeLength(id, agentinfo.id, years, username, tname, ref remsg);
                msg.Msg = remsg;
            }
            catch (Exception ex)
            {
                msg.Msg = remsg + ex.Message;
            }

            return Json(msg);
        }
        public ActionResult addtimelength()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int id = Context.GetRequestInt("id", 0);
            int years = Context.GetRequestInt("years", 0);
            int tid = Context.GetRequestInt("tid", 0);
            string username = Context.GetRequest("username", string.Empty);
            string tname = Context.GetRequest("tname", string.Empty);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            if (years <= 0)
            {
                msg.Msg = "年限错误";
                return Json(msg);
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
            if (xcxrelation == null)
            {
                msg.Msg = "无效授权数据，请刷新重试";
                return Json(msg);
            }

            XcxTemplate tempModel = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if(tempModel==null)
            {
                msg.Msg = "模板不存在，请刷新重试";
                return Json(msg);
            }

            
            try
            {
                int sum = 0;
                int parent_sum = 0;
                string errorMsg = "";
                //检验代理预存
                List<XcxTemplate> tempXcxlist = new List<XcxTemplate>() { new XcxTemplate() { Id = tempModel.Id, buycount = 1, year = years } };
                List<XcxTemplate> xcxlist = AgentinfoBLL.SingleModel.CheckParentAgentDeposit(agentinfo.id, tempModel.VersionId, "", tempXcxlist, ref sum, ref errorMsg);
                if (errorMsg.Length > 0)
                {
                    msg.Msg = errorMsg;
                    return Json(msg);
                }

                //分销商：检验上级代理预存
                Distribution distribution = null;
                if (agentinfo.userLevel == 1)
                {
                    distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentinfo.id);
                    AgentinfoBLL.SingleModel.CheckParentAgentDeposit(distribution.parentAgentId, tempModel.VersionId, "", tempXcxlist, ref parent_sum, ref errorMsg);
                    if (errorMsg.Length > 0)
                    {
                        msg.Msg = errorMsg;
                        return Json(msg);
                    }
                }
                msg.isok = _agentCustomerRelationBLL.AddTimeLengthV2(xcxlist,distribution,sum,parent_sum, xcxrelation, agentinfo.id, username, tname, ref errorMsg);
                msg.Msg = errorMsg;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(),ex);
                msg.Msg = ex.Message;
            }

            return Json(msg);
        }

        /// <summary>
        /// 升级
        /// </summary>
        /// <returns></returns>
        public ActionResult UpEntProVersion()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int id = Context.GetRequestInt("XcxRelationId", 0);
            string username = Context.GetRequest("username", string.Empty);
            string tname = Context.GetRequest("tname", string.Empty);
            int newVersionId = Context.GetRequestInt("newVersionId", -1);
            int oldVersionId = Context.GetRequestInt("oldVersionId", -1);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            if (newVersionId < 0)
            {
                msg.Msg = "升级的版本不存在";
                return Json(msg);
            }
            if (oldVersionId == 0)
            {
                msg.Msg = "当前是最高版本";
                return Json(msg);
            }

            if (agentinfo.userLevel == 1)
            {
                XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
                if (xrelationmodel == null)
                {
                    msg.isok = false;
                    msg.Msg = $"分销商的用户模板不存在";
                    return Json(msg);
                }
                Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentinfo.id);
                Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                List<XcxTemplate> parent_xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", distribution.parentAgentId);
                if (parent_xcxlist == null && parent_xcxlist.Count <= 0)
                {
                    msg.isok = false;
                    msg.Msg = "没有找到模板！";
                    return Json(msg);
                }

            }
            string remsg = "";
            msg.isok = _agentCustomerRelationBLL.UpEntProVersion(id, agentinfo.id, username, newVersionId, tname, ref remsg);
            msg.Msg = remsg;
            return Json(msg);
        }

        /// <summary>
        /// 开通分店
        /// </summary>
        /// <returns></returns>
        public ActionResult AddStore()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            //账户信息不从缓存中读取，避免用户数据更新了缓存没有更新导致金额对不上
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModel(agentinfo.id);
            if (agentmodel == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            string username = Context.GetRequest("username", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            int scount = Context.GetRequestInt("scount", 0);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }

            XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
            if (xrelationmodel == null)
            {
                msg.Msg = "还没有权限";
                return Json(msg);
            }

            if (scount <= 0)
            {
                msg.Msg = "请输入开通数量";
                return Json(msg);
            }

            Account account = AccountBLL.SingleModel.GetModel($"id ='{xrelationmodel.AccountId}'");
            if (account == null)
            {
                msg.Msg = "用户不存在!";
                return Json(msg);
            }

            DateTime outtime = DateTime.Now;
            double per = CountDate(xrelationmodel, ref outtime);

            int sum = 0;
            int parent_sum = 0;
            List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", agentmodel.id);
            if (xcxlist != null && xcxlist.Count > 0)
            {
                foreach (XcxTemplate xcxmodel in xcxlist)
                {
                    xcxmodel.storecount = scount;
                    xcxmodel.sumprice = Convert.ToInt32(xcxmodel.SPrice * scount * per);
                    sum += xcxmodel.sumprice;
                }
                if (sum > agentmodel.deposit)
                {
                    msg.isok = false;
                    msg.Msg = $"您的预存款不足";
                    return Json(msg);
                }
            }

            //是否是分销代理
            if (agentmodel.userLevel == 1)
            {
                if (xrelationmodel == null)
                {
                    msg.isok = false;
                    msg.Msg = $"分销商的用户模板不存在";
                    return Json(msg);
                }
                Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentmodel.id);
                Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                List<XcxTemplate> parent_xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", distribution.parentAgentId);
                if (parent_xcxlist != null && parent_xcxlist.Count > 0)
                {
                    foreach (XcxTemplate xcxmodel in parent_xcxlist)
                    {
                        xcxmodel.storecount = scount;
                        xcxmodel.sumprice = Convert.ToInt32(xcxmodel.SPrice * scount * per);
                        parent_sum += xcxmodel.sumprice;
                    }
                    if (parent_sum > parentAgentinfo.deposit)
                    {
                        msg.isok = false;
                        msg.Msg = $"创建失败，请联系上级代理";
                        return Json(msg);
                    }
                }
                else
                {
                    msg.isok = false;
                    msg.Msg = "没有找到模板！";
                    return Json(msg);
                }
            }
            string remsg = "保存成功";
            msg.isok = _agentCustomerRelationBLL.AddStore(username, xrelationmodel, agentmodel, xcxlist[0], sum, parent_sum, account, ref remsg);
            msg.Msg = remsg;
            return Json(msg);
        }
        public ActionResult AddStoreV2()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            //账户信息不从缓存中读取，避免用户数据更新了缓存没有更新导致金额对不上
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModel(agentinfo.id);
            if (agentmodel == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            string username = Context.GetRequest("username", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            int scount = Context.GetRequestInt("scount", 0);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }

            XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
            if (xrelationmodel == null)
            {
                msg.Msg = "还没有权限";
                return Json(msg);
            }

            if (scount <= 0)
            {
                msg.Msg = "请输入开通数量";
                return Json(msg);
            }

            Account account = AccountBLL.SingleModel.GetModel($"id ='{xrelationmodel.AccountId}'");
            if (account == null)
            {
                msg.Msg = "用户不存在!";
                return Json(msg);
            }

            DateTime outtime = DateTime.Now;
            double per = CountDate(xrelationmodel, ref outtime);

            int sum = 0;
            int parent_sum = 0;
            List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", agentmodel.id);
            if (xcxlist != null && xcxlist.Count > 0)
            {
                foreach (XcxTemplate xcxmodel in xcxlist)
                {
                    xcxmodel.storecount = scount;
                    xcxmodel.sumprice = Convert.ToInt32(xcxmodel.SPrice * scount * per);
                    sum += xcxmodel.sumprice;
                }
                if (sum > agentmodel.deposit)
                {
                    msg.isok = false;
                    msg.Msg = $"您的预存款不足";
                    return Json(msg);
                }
            }

            //是否是分销代理
            List<XcxTemplate> parent_xcxlist = new List<XcxTemplate>();
            Agentinfo parentAgentinfo = new Agentinfo();
            Distribution distribution = new Distribution();
            if (agentmodel.userLevel == 1)
            {
                if (xrelationmodel == null)
                {
                    msg.isok = false;
                    msg.Msg = $"分销商的用户模板不存在";
                    return Json(msg);
                }
                distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentmodel.id);
                parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                parent_xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", distribution.parentAgentId);
                if (parent_xcxlist != null && parent_xcxlist.Count > 0)
                {
                    foreach (XcxTemplate xcxmodel in parent_xcxlist)
                    {
                        xcxmodel.storecount = scount;
                        xcxmodel.sumprice = Convert.ToInt32(xcxmodel.SPrice * scount * per);
                        parent_sum += xcxmodel.sumprice;
                    }
                    if (parent_sum > parentAgentinfo.deposit)
                    {
                        msg.isok = false;
                        msg.Msg = $"创建失败，请联系上级代理";
                        return Json(msg);
                    }
                }
                else
                {
                    msg.isok = false;
                    msg.Msg = "没有找到模板！";
                    return Json(msg);
                }
            }
            string remsg = "保存成功";
            msg.isok = _agentCustomerRelationBLL.AddStoreV2(username, xrelationmodel, agentmodel, parentAgentinfo, distribution, xcxlist[0], parent_xcxlist[0], sum, parent_sum, account, ref remsg);
            msg.Msg = remsg;
            return Json(msg);
        }

        /// <summary>
        /// 当前模板过期时间是否比新开门店过期时间还长，如果比较短，
        /// 则门店开通费用为当前模板有效时间占一年费用内的百分比，
        /// 门店有效期与模板保持一致，如果模板有效期超过1年，则门店费用为1年，有效期为1年
        /// </summary>
        /// <param name="xrelationmodel"></param>
        /// <param name="outtime"></param>
        /// <param name="years"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private double CountDate(XcxAppAccountRelation xrelationmodel, ref DateTime outtime, int years = 1, int type = 0)
        {
            //是否是添加门店
            if (type == 0)
            {
                TimeSpan ts1 = new TimeSpan(DateTime.Parse(xrelationmodel.outtime.ToString("yyyy-MM-dd")).Ticks);
                TimeSpan ts2 = new TimeSpan(DateTime.Now.AddYears(years).Ticks);
                TimeSpan ts3 = new TimeSpan(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).Ticks);
                TimeSpan ts = ts1.Subtract(ts3).Duration();
                int tday = ts1.Days;
                int t2day = ts2.Days;
                //相差天数
                double days = ts.Days;
                double per = tday < t2day ? days / 365.00 : 1;

                return per;
            }
            else
            {
                //门店续期
                //模板过期时间
                TimeSpan ts1 = new TimeSpan(DateTime.Parse(xrelationmodel.outtime.ToString("yyyy-MM-dd")).Ticks);
                //门店过期时间
                DateTime t2 = xrelationmodel.AddTime;
                //判断门店过期时间是否比现在时间小
                if (xrelationmodel.AddTime < DateTime.Now)
                {
                    t2 = DateTime.Now;
                }

                if (xrelationmodel.outtime < t2.AddYears(years))
                {
                    outtime = xrelationmodel.outtime;
                    TimeSpan ts3 = new TimeSpan(DateTime.Parse(t2.ToString("yyyy-MM-dd")).Ticks);

                    TimeSpan ts = ts1.Subtract(ts3).Duration();
                    //相差天数
                    double days = ts.Days;
                    //log4net.LogHelper.WriteInfo(this.GetType(), days.ToString()+";"+ xrelationmodel.outtime.ToString("yyyy-MM-dd")+";"+ t2.ToString("yyyy-MM-dd"));
                    double per = days / 365.00;
                    return per;
                }
                else
                {
                    outtime = t2.AddYears(years);
                    return years;
                }
            }
        }

        /// <summary>
        /// 获取开通门店记录
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStoreLog()
        {
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }

            int id = Context.GetRequestInt("id", 0);
            List<AgentdepositLog> storeloglist = AgentdepositLogBLL.SingleModel.GetList($"rid={id} and agentid={agentinfo.id}", 1000, 1, "", "addtime desc");
            if (storeloglist != null && storeloglist.Count > 0)
            {
                foreach (AgentdepositLog item in storeloglist)
                {
                    item.costdetail = item.OutTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            msg.isok = true;
            msg.Msg = "成功";
            msg.dataObj = storeloglist;
            return Json(msg);
        }

        /// <summary>
        /// 门店续费
        /// </summary>
        /// <returns></returns>
        public ActionResult AddStoreTimeLength()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int id = Context.GetRequestInt("id", 0);
            int rid = Context.GetRequestInt("rid", 0);
            int years = Context.GetRequestInt("years", 0);
            string username = Context.GetRequest("username", string.Empty);//用户名称
            string tname = Context.GetRequest("tname", string.Empty);//门店名称
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            if (rid <= 0)
            {
                msg.Msg = "参数错误author";
                return Json(msg);
            }
            if (years <= 0)
            {
                msg.Msg = "年限错误";
                return Json(msg);
            }
            XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(rid);
            if (xrelationmodel == null)
            {
                msg.isok = false;
                msg.Msg = $"分销商的用户模板不存在";
                return Json(msg);
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xrelationmodel.TId);
            if (xcxTemplate == null)
            {
                msg.isok = false;
                msg.Msg = $"分销商的用户模板类型不存在";
                return Json(msg);
            }


            DateTime overTime = DateTime.MinValue;
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序餐饮多门店模板:
                    Food store_Foods = FoodBLL.SingleModel.GetModel(id);
                    if (store_Foods == null)
                    {
                        msg.Msg = "找不到门店";
                        return Json(msg);
                    }
                    overTime = store_Foods.overTime;
                    break;
                default://默认多门店
                    FootBath footbathmodel = FootBathBLL.SingleModel.GetModel(id);
                    if (footbathmodel == null)
                    {
                        msg.Msg = "找不到门店";
                        return Json(msg);
                    }
                    overTime = footbathmodel.OverTime;
                    break;
            }

            //账户信息不从缓存中读取，避免用户数据更新了缓存没有更新导致金额对不上
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModel(agentinfo.id);
            if (agentmodel == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            xrelationmodel.AddTime = overTime;
            DateTime outtime = DateTime.Now;
            double per = CountDate(xrelationmodel, ref outtime, years, 1);
            //到期时间
            xrelationmodel.outtime = outtime;

            int sum = 0;
            int parent_sum = 0;
            List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", agentmodel.id);
            if (xcxlist != null && xcxlist.Count > 0)
            {
                foreach (XcxTemplate xcxmodel in xcxlist)
                {
                    xcxmodel.sumprice = Convert.ToInt32(xcxmodel.SPrice * per);
                    xcxmodel.outtime = outtime.ToString("yyyy-MM-dd HH:mm:ss");
                    xcxmodel.TName = tname;
                    sum += xcxmodel.sumprice;
                }
                if (sum <= 0)
                {
                    msg.isok = false;
                    msg.Msg = $"该门店无法续期，该门店的有效期不能超过模板有效期";
                    return Json(msg);
                }
                if (sum > agentmodel.deposit || agentmodel.deposit <= 0)
                {
                    msg.isok = false;
                    msg.Msg = $"您的预存款不足";
                    return Json(msg);
                }
            }

            //是否是分销代理
            if (agentmodel.userLevel == 1)
            {
                Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentmodel.id);
                Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                List<XcxTemplate> parent_xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", distribution.parentAgentId);
                if (parent_xcxlist != null && parent_xcxlist.Count > 0)
                {
                    foreach (XcxTemplate xcxmodel in parent_xcxlist)
                    {
                        xcxmodel.sumprice = Convert.ToInt32(xcxmodel.SPrice * per);
                        xcxmodel.outtime = outtime.ToString("yyyy-MM-dd HH:mm:ss");
                        xcxmodel.TName = tname;
                        parent_sum += xcxmodel.sumprice;
                    }
                    if (parent_sum > parentAgentinfo.deposit)
                    {
                        msg.isok = false;
                        msg.Msg = $"创建失败，请联系上级代理";
                        return Json(msg);
                    }
                }
                else
                {
                    msg.isok = false;
                    msg.Msg = "没有找到模板！";
                    return Json(msg);
                }
            }
            string remsg = "保存成功";

            msg.isok = _agentCustomerRelationBLL.AddStore(username, xrelationmodel, agentmodel, xcxlist[0], sum, parent_sum, dzaccount, ref remsg, id);
            msg.Msg = remsg;
            return Json(msg);
        }

        /// <summary>
        /// 获取门店集合
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStoreMenDianList()
        {
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }
            msg.isok = true;
            msg.Msg = "成功";

            int id = Context.GetRequestInt("id", 0);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
            if (xcx == null)
            {
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (xcxTemplate == null)
            {
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            object storelist = null;
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序餐饮多门店模板:
                    List<Food> store_Foods = FoodBLL.SingleModel.GetList($"appId = {id}");
                    storelist = store_Foods?.Select(s => new
                    {
                        Id = s.Id,
                        StoreName = s.FoodsName,
                        OverTimeStr = s.overTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        CreateDateStr = s.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    });
                    break;
                default://默认多门店版
                    storelist = FootBathBLL.SingleModel.GetList($"appid={id}");
                    break;
            }
            msg.dataObj = storelist;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改智慧餐厅门店数量
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateStoreCount()
        {
            msg = new Return_Msg();
            int scount = Context.GetRequestInt("scount", 0);
            int aid = Context.GetRequestInt("id", 0);
            string username = Context.GetRequest("username", string.Empty);//用户名称

            if (agentinfo == null)
            {
                msg.Msg = "登陆已过期，请重新登陆!";
                return Json(msg);
            }
            XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xrelationmodel == null)
            {
                msg.Msg = "数据已过期，请刷新重试";
                return Json(msg);
            }
            if (agentinfo.id != xrelationmodel.agentId)
            {
                msg.Msg = "权限已过期，请刷新重试";
                return Json(msg);
            }
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModel(agentinfo.id);
            if (agentmodel == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int sum = 0;
            int parent_sum = 0;
            List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", agentmodel.id);
            if (xcxlist != null && xcxlist.Count > 0)
            {
                if (xrelationmodel.SCount < scount)
                {
                    xcxlist[0].sumprice = Convert.ToInt32(xcxlist[0].SPrice * (scount - xrelationmodel.SCount));
                }
                xcxlist[0].buycount = scount;
                xcxlist[0].SCount = xrelationmodel.SCount;
                sum += xcxlist[0].sumprice;

                if (sum > agentmodel.deposit || agentmodel.deposit <= 0)
                {
                    msg.isok = false;
                    msg.Msg = $"您的预存款不足";
                    return Json(msg);
                }
            }

            //是否是分销代理
            if (agentmodel.userLevel == 1)
            {
                if (xrelationmodel == null)
                {
                    msg.isok = false;
                    msg.Msg = $"分销商的用户模板不存在";
                    return Json(msg);
                }
                Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentmodel.id);
                Agentinfo parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                List<XcxTemplate> parent_xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({xrelationmodel.TId})", distribution.parentAgentId);
                if (parent_xcxlist != null && parent_xcxlist.Count > 0)
                {
                    if (xrelationmodel.SCount < scount)
                    {
                        parent_xcxlist[0].sumprice = Convert.ToInt32(parent_xcxlist[0].SPrice * (scount - xrelationmodel.SCount));
                    }
                    parent_sum += parent_xcxlist[0].sumprice;
                    parent_xcxlist[0].buycount = scount;
                    parent_xcxlist[0].SCount = xrelationmodel.SCount;
                    if (parent_sum > parentAgentinfo.deposit)
                    {
                        msg.isok = false;
                        msg.Msg = $"您的预存款不足以扣费，请联系上级代理";
                        return Json(msg);
                    }
                }
                else
                {
                    msg.isok = false;
                    msg.Msg = "没有找到模板！";
                    return Json(msg);
                }
            }

            string remsg = "保存成功";
            msg.isok = _agentCustomerRelationBLL.OpenZHStore(username, xrelationmodel, scount, agentmodel, xcxlist[0], sum, parent_sum, dzaccount, ref remsg);
            msg.Msg = remsg;
            return Json(msg);
        }

        /// <summary>
        /// 获取代理升级企业版到企业智推版的模板信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOpenQiyeTemplateInfo()
        {
            msg = new Return_Msg();
            if (agentinfo == null)
            {
                msg.Msg = "登陆已过期，请重新登陆!";
                return Json(msg);
            }
            XcxTemplate qiyeTemplate = XcxTemplateBLL.SingleModel.GetAgentTemplate(agentinfo.id);
            msg.isok = true;
            msg.dataObj = qiyeTemplate;
            return Json(msg);
        }
        /// <summary>
        /// 企业智推版升级
        /// </summary>
        /// <returns></returns>
        public ActionResult UpQiyeVersion(int XcxRelationId = 0, string username = "")
        {
            msg = new Return_Msg();
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }

            if (XcxRelationId <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            string errorMsg = "";
            msg.isok = _agentCustomerRelationBLL.UpQieVersion(agentinfo.id, XcxRelationId, username, ref errorMsg);
            msg.Msg = msg.isok ? "操作成功" : errorMsg;

            return Json(msg);
        }
        #endregion
        #endregion

        #region 分销代理管理
        /// <summary>
        /// 分销商管理界面
        /// </summary>
        /// <returns></returns>
        public ActionResult DistributionList()
        {
            if (agentinfo == null)
            {
                return Content("用户不存在");
            }
            ViewBag.username = agentaccount.LoginId;
            OpenConfig(agentinfo);
            return View(agentinfo);
        }
        /// <summary>
        /// 获取分销商列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDistributionList()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int pagesize = Context.GetRequestInt("pagesize", 10);
            int pageindex = Context.GetRequestInt("pageindex", 1);
            string LoginId = Context.GetRequest("loginname", string.Empty);
            string username = Context.GetRequest("username", string.Empty);
            int state = Context.GetRequestInt("state", -2);
            string errormsg = string.Empty;
            int count = 0;
            List<DistributionModel> distributionList = DistributionBLL.SingleModel.GetDistributionList(agentinfo.id, LoginId, username, state, pagesize, pageindex, out count, out errormsg);
            msg.isok = count >= 0;
            msg.Msg = errormsg;
            msg.dataObj = new
            {
                recordCount = count,
                distributionList = distributionList
            };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 添加分销商界面
        /// </summary>
        /// <returns></returns>
        public ActionResult DistributionCreate()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            //扫码登陆代码
            string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["DistributionQrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = false;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
            }
            return View();
        }
        /// <summary>
        /// 保存添加分销商
        /// </summary>
        /// <returns></returns>
        public ActionResult AddDistribution()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            string useraccountid = Context.GetRequest("useraccountid", string.Empty);
            if (string.IsNullOrEmpty(useraccountid))
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{useraccountid}'");
            if (account == null)
            {
                msg.Msg = "该用户不存在";
                return Json(msg);
            }
            string username = Context.GetRequest("username", string.Empty);
            if (string.IsNullOrEmpty(username))
            {
                msg.Msg = "请填写分销商名称";
                return Json(msg);
            }
            if (username.Length > 28)
            {
                msg.Msg = "分销商名称不能超过28字";
                return Json(msg);
            }
            if (!Regex.IsMatch(username, @"^[^\s]+$"))
            {
                msg.Msg = "分销商名称不能有空格";
                return Json(msg);
            }
            int deposit = Context.GetRequestInt("deposit", 0);
            if (deposit < 0)
            {
                msg.Msg = "预存款错误，预存款范围：不小于零，最多两位小数";
                return Json(msg);
            }
            string remark = Context.GetRequest("remark", string.Empty);
            if (remark.Length > 100)
            {
                msg.Msg = "备注内容不能超过100字";
                return Json(msg);
            }
            string templateList = Context.GetRequest("templateList", string.Empty);
            if (string.IsNullOrEmpty(templateList))
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            List<XcxTemplate> list = JsonConvert.DeserializeObject<List<XcxTemplate>>(templateList);
            if (list == null || list.Count <= 0)
            {
                msg.Msg = "参数错误!";
                return Json(msg);
            }

            foreach (XcxTemplate template in list)
            {
                if (template.Price < 0)
                {
                    msg.Msg = "模板价格错误，价格范围：不小于零，最多两位小数";
                    return Json(msg);
                }
            }
            Agentinfo _agentinfo = AgentinfoBLL.SingleModel.GetModel($"useraccountid='{account.Id}'");
            if (_agentinfo != null)
            {
                msg.Msg = "该用户已成为代理";
                return Json(msg);
            }
            string errormsg = "";
            msg.dataObj = list;
            msg.isok = DistributionBLL.SingleModel.AddDistribution(account, agentinfo.id, agentinfo.AgentType, username, list, deposit, remark, ref errormsg);
            msg.Msg = string.IsNullOrEmpty(errormsg) ? (msg.isok ? "添加成功" : "添加失败") : errormsg;
            return Json(msg);
        }

        /// <summary>
        /// 修改客户资料界面
        /// </summary>
        /// <returns></returns>
        public ActionResult DistributionEdit()
        {
            if (agentinfo == null)
            {
                return Content("您还未登录");
            }
            int agentid = Context.GetRequestInt("id", 0);
            if (agentid <= 0)
            {
                return Content("参数错误");
            }
            Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentIdAndParentId(agentid,agentinfo.id);
            if (distribution == null)
            {
                return Content("用户不存在");
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{distribution.useraccountid}'");
            if (account == null)
            {
                return Content("用户不存在!");
            }
            Agentinfo userAgent = AgentinfoBLL.SingleModel.GetModel($"id={distribution.AgentId} and userLevel=1");
            if (userAgent == null)
            {
                return Content("该分销商不存在");
            }
            ViewBag.type = Context.GetRequestInt("type", 0);//根据type显示编辑界面不同标签页

            DistributionModel viewmodel = new DistributionModel();
            viewmodel.AgentId = distribution.AgentId;
            viewmodel.LoginId = account.LoginId;
            viewmodel.name = distribution.name;
            viewmodel.remark = distribution.remark;
            viewmodel.deposit = userAgent.deposit;
            viewmodel.showDeposit = (userAgent.deposit * 0.01).ToString("0.00");
            viewmodel.detailList = XcxTemplateBLL.SingleModel.GetSaleDetailList(distribution.AgentId, agentinfo.AgentType, ceshitype);

            return View(viewmodel);
        }
        /// <summary>
        /// 获取分销商各个模板价格
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDistributionTemplate()
        {
            if (agentinfo == null)
            {
                msg.Msg = "代理商账号过期";
                return Json(msg);
            }
            int agentId = Context.GetRequestInt("id", 0);
            if (agentId <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }

            string sqlwhere = $"state=0 and (projectType={(int)ProjectType.小程序} {(ceshitype.Length > 0 ? $" or type in({ceshitype})": "")})";
            if (agentinfo.AgentType == 1)
            {
                sqlwhere += $" and (price<=0 or type={(int)TmpType.小程序专业模板})";
            }

            List<XcxTemplate> list = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList(sqlwhere, agentId);
            List<VersionType> listVersionType = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(22, agentId, agentinfo.AgentType);
            list.ForEach(x =>
            {
                if (x.Type == 22 && listVersionType != null && x.VersionId == 0)
                {
                    VersionType m = listVersionType.Find(y => y.VersionId == 0);
                    if (m != null)
                    {
                        x.TName += m.VersionName;
                        x.Title += m.VersionName;
                        x.Price = Convert.ToInt32(m.VersionPrice);
                    }
                }

                x.ShowPrice = (x.Price * 0.01).ToString("0.00");
            });

            List<XcxTemplate> datalist = new List<XcxTemplate>();
            if (list != null && list.Count > 0)
            {
                foreach (VersionType item in listVersionType)
                {
                    if (item.VersionId != 0)
                    {
                        XcxTemplate xcxTemplateTemp = new XcxTemplate();
                        xcxTemplateTemp.VersionId = item.VersionId;
                        xcxTemplateTemp.TName = "专业版" + item.VersionName;
                        xcxTemplateTemp.Title = "专业版" + item.VersionName;
                        xcxTemplateTemp.Price = Convert.ToInt32(item.VersionPrice);
                        xcxTemplateTemp.Type = 22;
                        XcxTemplate tempxcxmodel = list.Find(x => x.VersionId == 0 && x.Type == 22);
                        xcxTemplateTemp.Id = tempxcxmodel != null ? tempxcxmodel.Id : 0;

                        list.Add(xcxTemplateTemp);

                    }
                }

                foreach (XcxTemplate item in list)
                {

                    item.ShowPrice = (item.Price * 0.01).ToString("0.00");
                    item.statename = (item.SPrice * 0.01).ToString("0.00");
                    item.storecount = item.SCount;
                    if (agentinfo.AgentType == 1 && item.Type == (int)TmpType.小程序专业模板 && item.VersionId == 0)
                    {
                        continue;
                    }
                    datalist.Add(item);
                }
            }

            msg.isok = true;
            msg.dataObj = datalist;
            return Json(msg);
        }
        /// <summary>
        /// 获取分销商已创建用户
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCreatedCustomer()
        {
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 5);
            int agentId = Context.GetRequestInt("id", 0);
            if (agentId <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            int count = 0;
            List<CustomerModel> customerList = _agentCustomerRelationBLL.GetCustomerByAgentId(agentId, pageIndex, pageSize, out count);
            msg.isok = true;
            msg.dataObj = new
            {
                list = customerList,
                Count = count
            };
            return Json(msg);
        }
        /// <summary>
        /// 分销商编辑保存
        /// </summary>
        /// <returns></returns>
        public ActionResult DistributionSaveEdit()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int agentId = Context.GetRequestInt("id", 0);
            if (agentId <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentIdAndParentId(agentId,agentinfo.id);
            if (distribution == null)
            {
                msg.Msg = "用户不存在";
                return Json(msg);
            }

            string username = Context.GetRequest("username", string.Empty);
            if (string.IsNullOrEmpty(username))
            {
                msg.Msg = "请填写分销商名称";
                return Json(msg);
            }
            if (username.Length > 28)
            {
                msg.Msg = "分销商名称不能超过28字";
                return Json(msg);
            }
            if (!Regex.IsMatch(username, @"^[^\s]+$"))
            {
                msg.Msg = "分销商名称不能有空格";
                return Json(msg);
            }
            string showdeposit = Context.GetRequest("deposit", string.Empty);
            int deposit = (int)(Convert.ToDouble(showdeposit) * 100);
            if (deposit < 0)
            {
                msg.Msg = $"预存款错误，预存款范围：0~9999999.99，最多两位小数{deposit}";
                return Json(msg);
            }
            string remark = Context.GetRequest("remark", string.Empty);
            if (remark.Length > 100)
            {
                msg.Msg = "备注内容不能超过100字";
                return Json(msg);
            }
            string templateList = Context.GetRequest("templateList", string.Empty);
            if (string.IsNullOrEmpty(templateList))
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            List<XcxTemplate> list = JsonConvert.DeserializeObject<List<XcxTemplate>>(templateList);
            if (list == null || list.Count <= 0)
            {
                msg.Msg = "参数错误!";
                return Json(msg);
            }

            //list.ForEach(l => l.Price = (int)(Convert.ToDouble(l.ShowPrice) * 100));
            foreach (XcxTemplate template in list)
            {
                //模板价格
                template.Price = (int)(Convert.ToDouble(template.ShowPrice) * 100);
                msg.Msg = template.Price < 0 ? "模板价格错误，价格范围：0~9999999.99，最多两位小数|" : "";
                //单门店价格
                template.SPrice = (template.Type == (int)TmpType.小程序多门店模板 || template.Type == (int)TmpType.小程序餐饮多门店模板 || template.Type == (int)TmpType.智慧餐厅) ? (int)(Convert.ToDouble(template.statename) * 100) : 0;
                msg.Msg += (template.Type == (int)TmpType.小程序多门店模板 || template.Type == (int)TmpType.小程序餐饮多门店模板 || template.Type == (int)TmpType.智慧餐厅) && template.SPrice < 0 ? "单门店价格错误，价格范围：0~9999999.99，最多两位小数|" : "";
                //门店数量
                msg.Msg += (template.Type == (int)TmpType.小程序多门店模板 || template.Type == (int)TmpType.小程序餐饮多门店模板 || template.Type == (int)TmpType.智慧餐厅) && template.storecount < template.SCount ? "开通分店数量限制范围：" + template.SCount + "~9999|" : "";
                template.SCount = (template.Type == (int)TmpType.小程序多门店模板 || template.Type == (int)TmpType.小程序餐饮多门店模板 || template.Type == (int)TmpType.智慧餐厅) ? template.storecount : 0;

                if (!string.IsNullOrEmpty(msg.Msg))
                {
                    msg.Msg = msg.Msg.Split('|')[0];
                    return Json(msg);
                }
            }
            distribution.name = username;
            distribution.remark = remark;
            string errormsg = string.Empty;
            msg.isok = DistributionBLL.SingleModel.UpdateDistribution(distribution, deposit, list, out errormsg);
            msg.Msg = msg.isok ? "保存成功" : "保存失败," + errormsg;



            return Json(msg);
        }

        /// <summary>
        /// 更新分销商状态
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateDistributionState()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int AgentId = Context.GetRequestInt("id", 0);
            int state = Context.GetRequestInt("state", 0);
            if (AgentId <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentIdAndParentId(AgentId,agentinfo.id);
            if (distribution == null)
            {
                msg.Msg = "用户不存在";
                return Json(msg);
            }
            Agentinfo dagentInfo = AgentinfoBLL.SingleModel.GetModel(distribution.AgentId);
            if (dagentInfo == null)
            {
                msg.Msg = "代理分销商不存在";
                return Json(msg);
            }
            distribution.state = state;
            distribution.modifyDate = DateTime.Now;
            dagentInfo.state = state;
            dagentInfo.updateitme = distribution.modifyDate;
            msg.isok = DistributionBLL.SingleModel.UpdateState(distribution, dagentInfo);
            msg.Msg = msg.isok ? "操作成功" : "操作失败";
            return Json(msg);
        }

        /// <summary>
        /// 修改分销商密码界面
        /// </summary>
        /// <returns></returns>
        public ActionResult updateDistributionPwd()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int agentId = Context.GetRequestInt("id", 0);
            if (agentId <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentIdAndParentId(agentId,agentinfo.id);
            if (distribution == null)
            {
                msg.Msg = "用户不存在";
                return Json(msg);
            }
            return View(distribution);
        }
        /// <summary>
        /// 保存分销商密码
        /// </summary>
        /// <returns></returns>
        public ActionResult saveDistributionPwd()
        {
            if (agentinfo == null)
            {
                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int agentId = Context.GetRequestInt("id", 0);
            if (agentId <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentIdAndParentId(agentId,agentinfo.id);
            if (distribution == null)
            {
                msg.Msg = "用户不存在";
                return Json(msg);
            }
            Account account = AccountBLL.SingleModel.GetModel($" id ='{distribution.useraccountid}'");
            if (account == null)
            {
                msg.Msg = "用户不存在!";
                return Json(msg);
            }
            string pwd = Context.GetRequest("pwd", string.Empty);
            if (string.IsNullOrEmpty(pwd))
            {
                msg.Msg = "请输入密码";
                return Json(msg);
            }
            if (pwd.Length < 6)
            {
                msg.Msg = "密码不能少于6位";
                return Json(msg);
            }
            if (pwd.Length > 20)
            {
                msg.Msg = "密码太长啦!";
                return Json(msg);
            }
            if (!Regex.IsMatch(pwd, @"^[a-zA-Z\d]+$"))
            {
                msg.Msg = "只能用数字或者字母作为密码";
                return Json(msg);
            }
            string pwd_again = Context.GetRequest("pwd_again", string.Empty);
            if (pwd != pwd_again)
            {
                msg.Msg = "密码不一致";
                return Json(msg);
            }
            if (account.Password == DESEncryptTools.GetMd5Base32(pwd))
            {
                msg.isok = true;
                msg.Msg = "修改成功";
                return Json(msg);
            }
            else
            {
                account.Password = DESEncryptTools.GetMd5Base32(pwd);
                msg.isok = AccountBLL.SingleModel.Update(account, "Password");
                msg.Msg = msg.isok ? "修改成功" : "修改失败";
                return Json(msg);
            }
        }

        #endregion

        #region 预存款明细
        /// <summary>
        /// 列表界面
        /// </summary>
        /// <returns></returns>
        public ActionResult DepositDetail()
        {
            if (agentinfo == null)
            {
                return Content("您还未登录");
            }
            ViewBag.username = agentaccount.LoginId;
            OpenConfig(agentinfo);
            return View(agentinfo);
        }
        /// <summary>
        /// 获取明细列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLogList()
        {
            if (agentinfo == null)
            {

                msg.Msg = "您还未登录";
                return Json(msg);
            }
            int pagesize = Context.GetRequestInt("pagesize", 10);
            int pageindex = Context.GetRequestInt("pageIndex", 1);
            string reason = Context.GetRequest("reason", string.Empty);
            int type = Context.GetRequestInt("type", 0);
            string startTime = Context.GetRequest("starttime", string.Empty);
            string endtime = Context.GetRequest("endtime", string.Empty);
            int count = 0;
            string errormsg = string.Empty;
            List<AgentdepositLog> list = AgentdepositLogBLL.SingleModel.GetList(agentinfo.id, reason, type, startTime, endtime, pagesize, pageindex, out count);
            msg.isok = count >= 0;
            msg.dataObj = new
            {
                count = count,
                list = list
            };
            return Json(msg);
        }
        #endregion

        #region 代理商网站相关

        /// <summary>
        /// 代理商网站基础设置
        /// </summary>
        /// <returns></returns>
        public ActionResult WebSiteIndex()
        {
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }
            AgentWebSiteInfo agentWebSiteInfo = AgentWebSiteInfoBLL.SingleModel.GetModel($"userAccountId='{agentinfo.useraccountid}'");
            if (agentWebSiteInfo == null)
            {
                agentWebSiteInfo = new AgentWebSiteInfo();
                agentWebSiteInfo.seoConfig = JsonConvert.SerializeObject(agentWebSiteInfo.seoConfigModel);
                agentWebSiteInfo.pageMsgConfigModel.bannerImgs = "http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/img/20180201/15145031fbbd4d838a2bcf3b9c19165e.png,http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/img/20180201/ae6ab2fe6b15454e93664b93e7697ea7.png,http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/img/20180201/74b92f8da3e24cc6888bb4d1b15755bd.png";//新增的时候给默认的轮播图
                agentWebSiteInfo.pageMsgConfigModel.MobileBannerImgs = "http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/img/20180418/135a3a79373e49008b1bfee0c2923750.png,http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/img/20180418/5afc62d8ff7943a0a39f782f301e48ab.png,http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/img/20180418/2db5230a452445a3b014cf6ae7d6e905.png";//新增的时候给默认的移动端轮播图
                agentWebSiteInfo.pageMsgConfigModel.listCustomModel.Add(new CustomModel()
                {
                    modelName = "关于我们",
                    modelContent = "我们是国内优秀的小程序技术服务商，也是最早进入小程序行业的专家。目前，我们研发人员已超30人，项目经验丰富。研发团队实力强大，顶尖人才精心打造.我们为用户提供全方位小程序行业解决方案，帮助企业更快捷的实现传统业务向移动互联网的迁移，用技术驱动商业进化。",
                    modelDescription = "小程序行业领先服务商",
                    modelBanners = "http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/img/20180209/9bd16392dcbd4e5686b635b5cb2b3914.png"

                });
                agentWebSiteInfo.pageMsgConfig = JsonConvert.SerializeObject(agentWebSiteInfo.pageMsgConfigModel);
                agentWebSiteInfo.userAccountId = agentinfo.useraccountid;


                int agentWebSiteInfoId = Convert.ToInt32(AgentWebSiteInfoBLL.SingleModel.Add(agentWebSiteInfo));
                if (agentWebSiteInfoId <= 0)
                    return View("PageError", new Return_Msg() { Msg = "初始化失败!", code = "500" });

                agentWebSiteInfo.Id = agentWebSiteInfoId;
            }
            else
            {
                if (string.IsNullOrEmpty(agentWebSiteInfo.seoConfig))
                    return View("PageError", new Return_Msg() { Msg = "数据异常!", code = "500" });

                agentWebSiteInfo.seoConfigModel = JsonConvert.DeserializeObject<SeoConfigModel>(agentWebSiteInfo.seoConfig);
            }
            OpenConfig(agentinfo);
            return View(agentWebSiteInfo);
        }

        /// <summary>
        /// 保存网站配置
        /// </summary>
        /// <param name="agentWebSiteInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetWebSiteIndex(AgentWebSiteInfo agentWebSiteInfo)
        {
            if (agentinfo == null)
            {
                return Json(new { isok = false, msg = "登录信息过期" }, JsonRequestBehavior.AllowGet);
            }

            AgentWebSiteInfo model = AgentWebSiteInfoBLL.SingleModel.GetModel(agentWebSiteInfo.Id);
            if (model == null)
                return Json(new { isok = false, msg = "数据不存在" }, JsonRequestBehavior.AllowGet);

            if (agentWebSiteInfo.seoConfigModel == null)
                return Json(new { isok = false, msg = "基础配置信息不能为NULL" }, JsonRequestBehavior.AllowGet);

            List<string> listDomain = new List<string>();
            List<string> listClearDomin = new List<string>();
            if (string.IsNullOrEmpty(agentWebSiteInfo.domian))
                return Json(new { isok = false, msg = "请输入需要绑定的域名" }, JsonRequestBehavior.AllowGet);

            listDomain.AddRange(agentWebSiteInfo.domian.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList());
            if (listDomain == null || listDomain.Count <= 0)
                return Json(new { isok = false, msg = "绑定的域名不能为空" }, JsonRequestBehavior.AllowGet);


            if (agentWebSiteInfo.domainType == 1)
            {
                //表示选择的是小未程序二级域名
                if (listDomain.Count > 1)
                {
                    return Json(new { isok = false, msg = "二级域名只能绑定一个" }, JsonRequestBehavior.AllowGet);
                }
                listDomain[0] += WebSiteConfig.DzWebSiteDomainExt;

            }
            
            foreach (string item in listDomain)
            {
                if (item.StartsWith("xn--"))
                {
                    return Json(new { isok = false, msg = "中文域名punycode编码后的,请联系客服" }, JsonRequestBehavior.AllowGet);
                }

                if (!Regex.IsMatch(item, @"^([a-zA-Z0-9-\u4E00-\u9FA5]([a-zA-Z0-9\-\u4E00-\u9FA5]{0,61}[a-zA-Z0-9-\u4E00-\u9FA5])?\.)+[a-zA-Z]{2,6}$"))
                    return Json(new { isok = false, msg = "请填写正确的域名" }, JsonRequestBehavior.AllowGet);

                string[] domianArry = WebSiteConfig.DzWebSiteDomain.Split(';');
                if (domianArry.Contains(item.ToLower()))
                    return Json(new { isok = false, msg = "填写的域名存在已被占用" }, JsonRequestBehavior.AllowGet);

                AgentWebSiteInfo domianOwer = AgentWebSiteInfoBLL.SingleModel.GetModelByDomian($"{(agentWebSiteInfo.domainType == 1 ? item.Replace(WebSiteConfig.DzWebSiteDomainExt, "") : item)}");
                if (domianOwer != null && domianOwer.Id != model.Id)
                {
                    //表示在此之前已经有其它代理商使用该域名创建了网站并且绑定 提示当前代理商  因为一个域名对应一个代理商
                    return Json(new { isok = false, msg = item + "域名已经被其它代理商占用,请先核对" }, JsonRequestBehavior.AllowGet);
                }

            }


            if (model.isCreate > 0 && !string.IsNullOrEmpty(model.domian))
            {
                //表示之前已经绑定过域名了 将之前的加入解绑域名集合
                listClearDomin.AddRange(model.domian.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }

            if (!AgentWebSiteInfoBLL.SingleModel.BindDominInWebSite(listDomain, listClearDomin, agentWebSiteInfo.domainType))
                return Json(new { isok = false, msg = "创建网站异常" }, JsonRequestBehavior.AllowGet);

            model.updateitme = DateTime.Now;
            model.domian = agentWebSiteInfo.domian;
            model.domainType = agentWebSiteInfo.domainType;
            model.isCreate = 1;
            model.seoConfig = JsonConvert.SerializeObject(agentWebSiteInfo.seoConfigModel);
            if (AgentWebSiteInfoBLL.SingleModel.Update(model, "updateitme,domian,domainType,isCreate,seoConfig"))
            {
                return Json(new { isok = true, msg = "操作成功" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = false, msg = "操作失败" }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// 网站内容管理
        /// </summary>
        /// <returns></returns>
        public ActionResult WebSiteContent()
        {
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }
            AgentWebSiteInfo agentWebSiteInfo = AgentWebSiteInfoBLL.SingleModel.GetModel($"userAccountId='{agentinfo.useraccountid}'");
            if (agentWebSiteInfo == null)
                return View("PageError", new Return_Msg() { Msg = "请先到基础设置进行网站配置!", code = "403" });
            if (string.IsNullOrEmpty(agentWebSiteInfo.pageMsgConfig))
                return View("PageError", new Return_Msg() { Msg = "数据异常!", code = "500" });
            agentWebSiteInfo.pageMsgConfigModel = JsonConvert.DeserializeObject<PageMsgConfigModel>(agentWebSiteInfo.pageMsgConfig);

            OpenConfig(agentinfo);
            return View(agentWebSiteInfo);
        }

        /// <summary>
        /// 保存网站内容
        /// </summary>
        /// <param name="agentWebSiteInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetWebSiteContent(AgentWebSiteInfo agentWebSiteInfo)
        {
            if (agentinfo == null)
            {
                return Json(new { isok = false, msg = "登录信息过期" }, JsonRequestBehavior.AllowGet);
            }

            AgentWebSiteInfo model = AgentWebSiteInfoBLL.SingleModel.GetModel(agentWebSiteInfo.Id);
            if (model == null)
                return Json(new { isok = false, msg = "数据不存在" }, JsonRequestBehavior.AllowGet);

            if (agentWebSiteInfo.pageMsgConfigModel == null)
                return Json(new { isok = false, msg = "网站内容信息不能为NULL" }, JsonRequestBehavior.AllowGet);
            model.updateitme = DateTime.Now;
            model.pageMsgConfig = JsonConvert.SerializeObject(agentWebSiteInfo.pageMsgConfigModel);

            if (AgentWebSiteInfoBLL.SingleModel.Update(model, "updateitme,pageMsgConfig"))
            {
                return Json(new { isok = true, msg = "操作成功" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = false, msg = "操作失败" }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// 咨询表单列表
        /// </summary>
        /// <returns></returns>
        public ActionResult QuestionList()
        {
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            OpenConfig(agentinfo);
            return View();

        }

        /// <summary>
        /// 获取代理商网站页面咨询内容列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public JsonResult GetListGetQuestion(int pageIndex = 1, int pageSize = 20)
        {
            if (agentinfo == null)
            {
                return Json(new { isok = false, msg = "登录信息过期" }, JsonRequestBehavior.AllowGet);
            }

            string startDate = Context.GetRequest("startDate", string.Empty);
            string endDate = Context.GetRequest("endDate", string.Empty);
            string strWhere = $"useraccountid='{agentinfo.useraccountid}'";
            if (!string.IsNullOrEmpty(Utility.EncodeHelper.ReplaceSqlKey(startDate)) && !string.IsNullOrEmpty(Utility.EncodeHelper.ReplaceSqlKey(endDate)))
            {
                startDate = Convert.ToDateTime(startDate + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                endDate = Convert.ToDateTime(endDate + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss");
                strWhere += $"  and addtime between '{startDate}' and '{endDate}'";
            }

            List<AgentWebSiteQuestion> list = AgentWebSiteQuestionBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", " addtime desc");
            int totalCount = AgentWebSiteQuestionBLL.SingleModel.GetCount(strWhere);

            return Json(new { isok = true, msg = "获取成功", model = new { RecordCount = totalCount, List = list } }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 导出代理商网站咨询列表
        /// </summary>

        public void ExPortQuestion()
        {
            if (agentinfo == null)
            {
                Response.Write("登录信息过期");
                return;
            }


            string startDate = Context.GetRequest("startDate", string.Empty);
            string endDate = Context.GetRequest("endDate", string.Empty);
            string strWhere = $"useraccountid='{agentinfo.useraccountid}'";
            string ids = Context.GetRequest("ids", string.Empty);
            if (!string.IsNullOrEmpty(ids))
            {
                if (!StringHelper.IsNumByStrs(',', ids))
                {
                    Response.Write("非法输入");
                    return;
                }
                else
                {
                    strWhere += $" and Id in({ids})";
                }
            }
            if (!string.IsNullOrEmpty(Utility.EncodeHelper.ReplaceSqlKey(startDate)) && !string.IsNullOrEmpty(Utility.EncodeHelper.ReplaceSqlKey(endDate)))
            {
                startDate = Convert.ToDateTime(startDate + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                endDate = Convert.ToDateTime(endDate + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss");
                strWhere += $"  and addtime between '{startDate}' and '{endDate}'";
            }
            List<AgentWebSiteQuestion> list = AgentWebSiteQuestionBLL.SingleModel.GetList(strWhere, 10000000, 1, "*", " addtime desc");
            if (list == null || list.Count <= 0)
            {
                Response.Write("非法输入");
                return;
            }
            else
            {
                DataTable table = new DataTable();
                table.Columns.AddRange(new[]
                {
                        new DataColumn("手机号码"),
                        new DataColumn("姓名"),
                        new DataColumn("咨询内容"),
                        new DataColumn("提交时间")
                    });
                StringBuilder sb = new StringBuilder();
                foreach (AgentWebSiteQuestion item in list)
                {
                    DataRow row = table.NewRow();
                    row[0] = item.telephone;
                    row[1] = item.userName;
                    row[2] = item.question;
                    row[3] = item.addTimeStr;
                    table.Rows.Add(row);
                }

                ExcelHelper<C_BargainUser>.Out2Excel(table, "咨询问题列表"); //导出
            }
        }

        /// <summary>
        /// 根据当前请求的域名 找到绑定的代理商网站信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAgentWebSiteInfo()
        {
            string domian = System.Web.HttpContext.Current.Request.Url.Host;
            if (string.IsNullOrEmpty(domian))
                return Json(new { isok = false, msg = "请求异常(域名不合法)" }, JsonRequestBehavior.AllowGet);
            if (domian.Contains(WebSiteConfig.DzWebSiteDomainExt))
            {
                //表示二级域名
                domian = domian.Replace(WebSiteConfig.DzWebSiteDomainExt, "");
            }
            AgentWebSiteInfo agentWebSiteInfo = AgentWebSiteInfoBLL.SingleModel.GetModelByDomian(domian);
            if (agentWebSiteInfo == null)
                return Json(new { isok = false, msg = "请求异常(没有数据)" }, JsonRequestBehavior.AllowGet);

            if (!string.IsNullOrEmpty(agentWebSiteInfo.seoConfig))
            {
                agentWebSiteInfo.seoConfigModel = JsonConvert.DeserializeObject<SeoConfigModel>(agentWebSiteInfo.seoConfig);
            }
            if (!string.IsNullOrEmpty(agentWebSiteInfo.pageMsgConfig))
            {
                agentWebSiteInfo.pageMsgConfigModel = JsonConvert.DeserializeObject<PageMsgConfigModel>(agentWebSiteInfo.pageMsgConfig);
            }
            return Json(new { isok = true, msg = "成功", obj = agentWebSiteInfo }, JsonRequestBehavior.AllowGet); ;

        }

        /// <summary>
        /// 代理商网站页面的咨询问题
        /// </summary>
        /// <returns></returns>
        public ActionResult SetAgentWebSiteQuetion()
        {
            string domian = System.Web.HttpContext.Current.Request.Url.Host;
            if (string.IsNullOrEmpty(domian))
                return Json(new { isok = false, msg = "请求异常(域名不合法)" }, JsonRequestBehavior.AllowGet);
            if (domian.Contains(WebSiteConfig.DzWebSiteDomainExt))
            {
                domian = domian.Replace(WebSiteConfig.DzWebSiteDomainExt, "");
            }
            AgentWebSiteInfo agentWebSiteInfo = AgentWebSiteInfoBLL.SingleModel.GetModelByDomian(domian);
            if (agentWebSiteInfo == null)
                return Json(new { isok = false, msg = "请求异常(代理商数据异常)" }, JsonRequestBehavior.AllowGet);

            string telephone = Context.GetRequest("telephone", string.Empty);
            if (!Regex.IsMatch(telephone, @"^(0?(13[0-9]|15[012356789]|17[013678]|18[0-9]|14[57])[0-9]{8})|(400|800)([0-9\\-]{7,10})|(([0-9]{4}|[0-9]{3})(-|)?)?([0-9]{7,8})((-||转)*([0-9]{1,4}))?$"))
                return Json(new { isok = false, msg = "请填写正确的联系方式" }, JsonRequestBehavior.AllowGet);
            string userName = Context.GetRequest("userName", string.Empty);
            userName = StringHelper.ReplaceSqlKeyword(StringHelper.ReplaceSQLKey(userName));
            if (string.IsNullOrEmpty(userName))
                return Json(new { isok = false, msg = "非法操作(联系人填写非法)" }, JsonRequestBehavior.AllowGet);
            string question = Context.GetRequest("question", string.Empty);
            question = StringHelper.ReplaceSqlKeyword(question);
            if (string.IsNullOrEmpty(question))
                return Json(new { isok = false, msg = "非法操作(填写内容非法)" }, JsonRequestBehavior.AllowGet);

            int id = Convert.ToInt32(AgentWebSiteQuestionBLL.SingleModel.Add(
                   new AgentWebSiteQuestion()
                   {
                       useraccountid = agentWebSiteInfo.userAccountId,
                       userName = userName,
                       telephone = telephone,
                       question = question,
                       addtime = DateTime.Now
                   }
                   ));
            if (id <= 0)
                return Json(new { isok = false, msg = "提交失败" }, JsonRequestBehavior.AllowGet);


            return Json(new { isok = true, msg = "提交成功" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult friendPage()
        {
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }
            OpenConfig(agentinfo);
            return View();
        }

        #endregion

        #region 案例管理

        /// <summary>
        /// 案例管理
        /// </summary>
        /// <returns></returns>
        public ActionResult AgentinfoCase()
        {
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            OpenConfig(agentinfo);
            return View();
        }

        public ActionResult GetAgentinfoCaseList(string tagName="",string userAccountId="",int tid = -999, string name = "", int pageIndex = 1, int pageSize = 10)
        {
            msg = new Return_Msg();
            int agentId = 0;
            int state = -999;
            if (agentinfo == null)
            {
                if (string.IsNullOrEmpty(userAccountId))
                {
                    msg.Msg = "您还不是代理商";
                    return Json(msg);
                }
                else
                {
                    Agentinfo webAgentInfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(userAccountId);
                    if (webAgentInfo == null)
                    {
                        msg.Msg = "无效代理";
                        return Json(msg);
                    }
                    agentId = webAgentInfo.id;
                    state = 1;
                }
            }
            else
            {
                agentId = agentinfo.id;
            }

            int count = 0;
            List<AgentinfoCase> list = AgentinfoCaseBLL.SingleModel.GetCaseList(tagName,state, name, agentId, tid, pageIndex, pageSize, ref count);

            msg.dataObj = new { list = list, count = count };
            msg.isok = true;
            return Json(msg);
        }

        public ActionResult UpdateAgentCaseState(int id=0,int state=0)
        {
            msg = new Return_Msg();
            if (agentinfo == null)
            {
                msg.Msg = "无效代理商";
                return Json(msg);
            }

            AgentinfoCase model = AgentinfoCaseBLL.SingleModel.GetModel(id);
            if(model==null)
            {
                msg.Msg = "案例不存在，请刷新重试";
                return Json(msg);
            }

            if(model.AgentId!=agentinfo.id)
            {
                msg.Msg = "无效案例，请刷新重试";
                return Json(msg);
            }

            model.State = state;
            model.UpdateTime = DateTime.Now;
            msg.isok = AgentinfoCaseBLL.SingleModel.Update(model);
            msg.Msg = msg.isok ? "保存成功" : "保存失败";

            return Json(msg);
        }

        /// <summary>
        /// 编辑案例
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddOrEditeCase(int id = 0)
        {
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            AgentinfoCase agentCase = AgentinfoCaseBLL.SingleModel.GetModel(id);
            if (agentCase == null)
            {
                agentCase = new AgentinfoCase();
                agentCase.AgentId = agentinfo.id;
            }

            OpenConfig(agentinfo);

            return View(agentCase);
        }

        public ActionResult SaveCaseInfo(AgentinfoCase model=null)
        {
            msg = new Return_Msg();
            if (agentinfo == null)
            {
                msg.Msg = "无效代理商";
                return Json(msg);
            }
            if(model==null)
            {
                msg.Msg = "无效案例";
                return Json(msg);
            }
            if(model.AgentId !=agentinfo.id)
            {
                msg.Msg = "请刷新重试";
                return Json(msg);
            }
            
            model.UpdateTime = DateTime.Now;
            if (model.Id<=0)
            {
                model.AddTime = DateTime.Now;
                model.Id = Convert.ToInt32(AgentinfoCaseBLL.SingleModel.Add(model));
                msg.isok = model.Id > 0;
            }
            else
            {
                msg.isok = AgentinfoCaseBLL.SingleModel.Update(model);
            }

            msg.Msg = msg.isok ? "保存成功" : "保存失败";

            return Json(msg);
        }

        public ActionResult GetCaseTagList(string name = "", int tid = 0, int pageSize = 20, int pageIndex = 1)
        {
            msg = new Return_Msg();
            if (agentinfo == null)
            {
                msg.Msg = "无效代理商";
                return Json(msg);
            }
            int count = 0;
            List<Miniapptag> list = MiniapptagBLL.SingleModel.GetListByAgentIdAndTid(name, agentinfo.id, tid, ref count, pageSize, pageIndex);

            msg.dataObj = new { list, count };
            msg.isok = true;
            return Json(msg);
        }

        /// <summary>
        /// 获取案例模板
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCaseXcxTemplate(string userAccountId = "")
        {
            msg = new Return_Msg();
            int agentType = 0;
            int agentId = 0;
            if (agentinfo == null)
            {
                if (string.IsNullOrEmpty(userAccountId))
                {
                    msg.Msg = "您还不是代理商";
                    return Json(msg);
                }
                else
                {
                    Agentinfo webAgentInfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(userAccountId);
                    if (webAgentInfo == null)
                    {
                        msg.Msg = "无效代理";
                        return Json(msg);
                    }
                    agentType = webAgentInfo.AgentType;
                    agentId = webAgentInfo.id;
                }
            }
            else
            {
                agentType = agentinfo.AgentType;
                agentId = agentinfo.id;
            }
            
            List<XcxTemplate> xcxlist = XcxTemplateBLL.SingleModel.GetRealPriceTemplateListV2(agentId, ceshitype, agentType);
            xcxlist?.ForEach(x => x.ShowPrice = (x.Price * 0.01).ToString("0.00"));
            msg.dataObj = xcxlist;
            msg.isok = true;
            return Json(msg);
        }

        /// <summary>
        /// 标签管理
        /// </summary>
        /// <returns></returns>
        public ActionResult AgentinfoCaseTag()
        {
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            OpenConfig(agentinfo);
            return View();
        }

        [HttpPost]
        public ActionResult SaveCaseTag(int id = 0, int tid = 0, string name = "")
        {
            msg = new Return_Msg();
            if (agentinfo == null)
            {
                msg.Msg = "无效代理商";
                return Json(msg);
            }

            if (tid <= 0)
            {
                msg.Msg = "无效的标签归属";
                return Json(msg);
            }
            if (string.IsNullOrEmpty(name))
            {
                msg.Msg = "标签名称不能为空";
                return Json(msg);
            }

            if (id <= 0)
            {
                Miniapptag model = new Miniapptag();
                model.addtime = DateTime.Now;
                model.AgentId = agentinfo.id;
                model.tagname = name;
                model.tid = tid;
                model.updatetime = model.addtime;
                model.id = Convert.ToInt32(MiniapptagBLL.SingleModel.Add(model));
                msg.isok = model.id > 0;
            }
            else
            {
                Miniapptag model = MiniapptagBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    msg.Msg = "无效标签";
                    return Json(msg);
                }
                model.tagname = name;
                model.tid = tid;
                model.updatetime = DateTime.Now;
                msg.isok = MiniapptagBLL.SingleModel.Update(model, "tagname,tid,updatetime");
            }

            msg.Msg = msg.isok ? "保存成功" : "保存失败";
            return Json(msg);
        }

        [HttpPost]
        public ActionResult DeleteCaseTag(int id=0)
        {
            if (agentinfo == null)
            {
                msg.Msg = "无效代理商";
                return Json(msg);
            }
            Miniapptag model = MiniapptagBLL.SingleModel.GetModel(id);
            if(model==null)
            {
                msg.Msg = "无效标签";
                return Json(msg);
            }
            if(model.AgentId !=agentinfo.id)
            {
                msg.Msg = "请刷新重试";
                return Json(msg);
            }

            model.state = -1;
            model.updatetime = DateTime.Now;
            msg.isok = MiniapptagBLL.SingleModel.Update(model,"state,updatetime");
            msg.Msg = msg.isok ? "保存成功" : "保存失败";

            return Json(msg);
        }
        
        #endregion

        #region 代理分销推广

        #region 推广二维码
        public ActionResult AgentQrCodeList()
        {
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            //判断是不是代理
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }
            if (agentinfo.IsOpenDistribution == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            int count = 0;
            ViewModel<AgentQrCode> viewmodel = new ViewModel<AgentQrCode>();
            viewmodel.DataList = AgentQrCodeBLL.SingleModel.GetAgentQrCodeList(agentinfo.id, pageIndex, pageSize, ref count);
            viewmodel.TotalCount = count;
            viewmodel.PageIndex = pageIndex;
            viewmodel.PageSize = pageSize;

            OpenConfig(agentinfo);
            return View(viewmodel);
        }

        public ActionResult AddAgentQrCode()
        {
            string qrcodename = Context.GetRequest("qrcodename", string.Empty);
            string customerloginid = Context.GetRequest("customerloginid", string.Empty);
            int type = Context.GetRequestInt("type", 0);
            //判断是不是代理
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }
            if (string.IsNullOrEmpty(qrcodename))
            {
                msg.Msg = "请输入二维码名称";
                return Json(msg);
            }

            if (AgentQrCodeBLL.SingleModel.ExitModel(qrcodename, agentinfo.id))
            {
                msg.Msg = "该二维码名称已存在";
                return Json(msg);
            }

            AgentCustomerRelation customerrelation = null;
            if (type == 1)
            {
                if (string.IsNullOrEmpty(customerloginid))
                {
                    msg.Msg = "请输入客户账号";
                    return Json(msg);
                }
                Account accountmodel = AccountBLL.SingleModel.GetModelByLoginid(customerloginid);
                if (accountmodel == null)
                {
                    msg.Msg = "客户账号不正确";
                    return Json(msg);
                }
                customerrelation = _agentCustomerRelationBLL.GetModelByAccountId(agentinfo.id, accountmodel.Id.ToString());
                if (customerrelation == null)
                {
                    msg.Msg = "客户不存在";
                    return Json(msg);
                }
                if (customerrelation.QrcodeId > 0)
                {
                    msg.Msg = "该客户已存在推广二维码";
                    return Json(msg);
                }
            }

            AgentQrCode qrcodemodel = new AgentQrCode();
            qrcodemodel.AddTime = DateTime.Now;
            qrcodemodel.AgentId = agentinfo.id;
            qrcodemodel.State = -2;
            qrcodemodel.Name = qrcodename;
            qrcodemodel.UpdateTime = DateTime.Now;
            qrcodemodel.Id = Convert.ToInt32(AgentQrCodeBLL.SingleModel.Add(qrcodemodel));
            if (qrcodemodel.Id <= 0)
            {
                return Json(msg);
            }

            string domian = System.Web.HttpContext.Current.Request.Url.Host;
            string url = $"http://{domian}/mobile/mobileReg?agentqrcodeid={qrcodemodel.Id}";
            string Logo = "http:" + WebSiteConfig.MiniappZyUrl + "/img/green_logo.jpg";
            MemoryStream ms = new MemoryStream();
            Bitmap bmp = QRCodeHelp.Instance.GetQrCodeImg(Logo, url);
            bmp.Save(ms, ImageFormat.Jpeg);
            bmp.Dispose();

            byte[] byteData = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(byteData, 0, (int)ms.Length);
            ms.Close();
            string ext = string.Empty;
            ImgHelper.IsImgageType(byteData, "jpg", out ext);
            string aliTempImgKey = "";
            CommondHelper.SaveImageToAliOSS(byteData, out aliTempImgKey);
            msg.dataObj = aliTempImgKey;

            if (string.IsNullOrEmpty(aliTempImgKey))
            {
                msg.Msg = "生成二维码图pain链接失败";
                return Json(msg);
            }

            qrcodemodel.ImgUrl = aliTempImgKey;
            qrcodemodel.State = 1;
            if (!AgentQrCodeBLL.SingleModel.Update(qrcodemodel, "imgurl,state"))
            {
                msg.Msg = "保存二维码图失败";
                return Json(msg);
            }
            AgentQrCodeBLL.SingleModel.RemoveCache(agentinfo.id);
            if (type == 1)
            {
                customerrelation.QrcodeId = qrcodemodel.Id;
                customerrelation.OpenExtension = 1;
                _agentCustomerRelationBLL.Update(customerrelation, "QrcodeId,OpeneEtension");
            }
            msg.isok = true;
            return Json(msg);
        }

        public ActionResult DeleQrcode()
        {
            int id = Context.GetRequestInt("id", 0);
            int state = Context.GetRequestInt("state", 1);
            if (id <= 0)
            {
                msg.Msg = "id不能为0";
                return Json(msg);
            }
            //判断是不是代理
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            AgentQrCode agentcodemodel = AgentQrCodeBLL.SingleModel.GetModelById(agentinfo.id, id);
            if (agentcodemodel == null)
            {
                msg.Msg = "系统繁忙，请刷新重试";
                return Json(msg);
            }

            agentcodemodel.State = state;
            agentcodemodel.UpdateTime = DateTime.Now;
            if (!AgentQrCodeBLL.SingleModel.Update(agentcodemodel, "state,UpdateTime"))
            {
                return Json(msg);
            }
            msg.isok = true;
            AgentQrCodeBLL.SingleModel.RemoveCache(agentinfo.id);

            return Json(msg);
        }

        /// <summary>
        /// 商家搜索
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCustomerData()
        {
            msg = new Return_Msg();
            string loginid = Context.GetRequest("loginid", "");
            if (agentinfo == null)
            {
                msg.Msg = "请刷新重试";
                return Json(msg);
            }
            if (string.IsNullOrEmpty(loginid))
            {
                msg.Msg = "请输入商户账号";
                return Json(msg);
            }
            List<CustomerModel> list = _agentCustomerRelationBLL.GetCustomerData(agentinfo.id, loginid);

            msg.dataObj = list;
            msg.isok = true;
            return Json(msg);
        }

        public ActionResult DelBindCustomer()
        {
            msg = new Return_Msg();
            int id = Context.GetRequestInt("id", 0);
            int state = Context.GetRequestInt("state", 0);
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            //判断是不是代理
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            AgentCustomerRelation customerrelation = _agentCustomerRelationBLL.GetModel(id);
            if (customerrelation == null)
            {
                msg.Msg = "请刷新重试";
                return Json(msg);
            }

            AgentQrCodeBLL.SingleModel.RemoveCache(agentinfo.id);
            customerrelation.OpenExtension = state;
            customerrelation.updatetime = DateTime.Now;
            msg.isok = _agentCustomerRelationBLL.Update(customerrelation, "OpenExtension,updatetime");
            msg.Msg = msg.isok ? "保存成功" : "保存失败";

            return Json(msg);
        }
        #endregion

        #region 开通情况
        public ActionResult AgentDistributionUser()
        {
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int agentqrcodeid = Context.GetRequestInt("agentqrcodeid", 0);
            string loginid = Context.GetRequest("loginid", "");
            string starttime = Context.GetRequest("starttime", "");
            string endtime = Context.GetRequest("endtime", "");
            if (agentqrcodeid <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数出错!", code = "403" });
            }
            //判断是不是代理
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            if (agentinfo.IsOpenDistribution == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }


            int count = 0;
            ViewModel<AgentDistributionRelation> viewmodel = new ViewModel<AgentDistributionRelation>();
            viewmodel.DataList = _agentDistributionRelationBLL.GetAgentDistributionRelationList(starttime, endtime, loginid, agentqrcodeid, agentinfo.id, pageIndex, pageSize, ref count);
            viewmodel.TotalCount = count;
            viewmodel.PageIndex = pageIndex;
            viewmodel.PageSize = pageSize;
            ViewBag.agentqrcodeid = agentqrcodeid;
            ViewBag.Loginid = loginid;
            ViewBag.starttime = starttime;
            ViewBag.endtime = endtime;

            OpenConfig(agentinfo);
            return View(viewmodel);
        }

        public ActionResult GiveXiaowei()
        {
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                msg.Msg = "参数不能为0";
                return Json(msg);
            }

            //判断是不是代理
            if (agentinfo == null)
            {
                msg.Msg = "您还不是代理商";
                return Json(msg);
            }

            AgentDistributionRelation model = _agentDistributionRelationBLL.GetModel(id);
            if (model == null || model.ParentAgentId != agentinfo.id)
            {
                msg.Msg = "您不具有管理该代理的权限";
                return Json(msg);
            }

            model.FollowState = 1;
            model.SubmitTime = DateTime.Now;
            msg.isok = _agentDistributionRelationBLL.Update(model, "followstate,SubmitTime");
            _agentDistributionRelationBLL.RemoveCache(agentinfo.id);
            msg.Msg = msg.isok ? "保存成功" : "保存失败";

            return Json(msg);
        }

        public ActionResult UpdateCustomerName()
        {
            msg = new Return_Msg();
            int id = Context.GetRequestInt("id", 0);
            string customername = Context.GetRequest("customername", "");
            //判断是不是代理
            if (agentinfo == null)
            {
                msg.Msg = "您还不是代理商";
                return Json(msg);
            }
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            if (string.IsNullOrEmpty(customername))
            {
                msg.Msg = "请输入客户姓名";
                return Json(msg);
            }

            AgentDistributionRelation agentrelation = _agentDistributionRelationBLL.GetModel(id);
            if (agentrelation == null || agentrelation.ParentAgentId != agentinfo.id)
            {
                msg.Msg = "您不具有管理该代理的权限";
                return Json(msg);
            }
            Agentinfo model = AgentinfoBLL.SingleModel.GetModel(agentrelation.AgentId);
            if (model == null)
            {
                msg.Msg = "客户已过期，请刷新重试";
                return Json(msg);
            }

            model.name = customername;
            model.updateitme = DateTime.Now;
            msg.isok = AgentinfoBLL.SingleModel.Update(model, "name,updateitme");
            _agentDistributionRelationBLL.RemoveCache(agentinfo.id);
            msg.Msg = msg.isok ? "保存成功" : "保存失败";
            return Json(msg);
        }
        #endregion

        #region 跟进记录
        public ActionResult AgentFollowLogList()
        {
            int agentdistributionrid = Context.GetRequestInt("agentdistributionrid", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }

            AgentDistributionRelation agentrelationmodel = _agentDistributionRelationBLL.GetModel(agentdistributionrid);
            if (agentrelationmodel == null || agentrelationmodel.ParentAgentId != agentinfo.id || agentinfo.IsOpenDistribution == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            int count = 0;
            ViewModel<AgentFollowLog> viewmodel = new ViewModel<AgentFollowLog>();
            viewmodel.DataList = AgentFollowLogBLL.SingleModel.GetAgentFollowLogList(agentdistributionrid, pageIndex, pageSize, ref count);
            viewmodel.TotalCount = count;
            viewmodel.PageIndex = pageIndex;
            viewmodel.PageSize = pageSize;
            ViewBag.AgentDistributionRid = agentdistributionrid;
            ViewBag.AgentQrcodeId = agentrelationmodel.QrCodeId;

            OpenConfig(agentinfo);
            return View(viewmodel);
        }
        public ActionResult SaveAgentFollowLog()
        {
            int agentdistributionrid = Context.GetRequestInt("agentdistributionrid", 0);
            string desc = Context.GetRequest("desc", string.Empty);

            if (agentdistributionrid <= 0)
            {
                msg.Msg = "参数错误d_null";
                return Json(msg);
            }
            if (string.IsNullOrEmpty(desc))
            {
                msg.Msg = "备注不能为空";
                return Json(msg);
            }
            #region Base64解密
            try
            {
                string strDescription = desc.Replace(" ", "+");
                byte[] bytes = Convert.FromBase64String(strDescription);
                desc = Encoding.UTF8.GetString(bytes);
            }
            catch
            {

            }

            desc = StringHelper.NoHtml(desc.Replace("&nbsp;", ""));
            #endregion

            //判断是不是代理
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            AgentDistributionRelation agentrelationmodel = _agentDistributionRelationBLL.GetModel(agentdistributionrid);
            if (agentrelationmodel == null || agentrelationmodel.ParentAgentId != agentinfo.id)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            AgentFollowLog agentlogmodel = new AgentFollowLog();
            agentlogmodel.Desc = desc.Replace("\n", "<br/>");
            agentlogmodel.AgentDistributionRelatioinId = agentdistributionrid;
            agentlogmodel.AddTime = DateTime.Now;
            agentlogmodel.UpdateTime = DateTime.Now;
            agentlogmodel.Type = 0;
            agentlogmodel.State = 1;
            agentlogmodel.Id = Convert.ToInt32(AgentFollowLogBLL.SingleModel.Add(agentlogmodel));

            msg.isok = agentlogmodel.Id > 0;
            msg.Msg = msg.isok ? "保存成功" : "保存失败";
            AgentFollowLogBLL.SingleModel.RemoveCache(agentdistributionrid, agentrelationmodel.ParentAgentId);
            return Json(msg);
        }

        public ActionResult DelAgentFollowLog()
        {
            int agentdistributionrid = Context.GetRequestInt("agentdistributionrid", 0);
            int id = Context.GetRequestInt("id", 0);

            if (agentdistributionrid <= 0)
            {
                msg.Msg = "参数错误d_null";
                return Json(msg);
            }

            //判断是不是代理
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            AgentDistributionRelation agentrelationmodel = _agentDistributionRelationBLL.GetModel(agentdistributionrid);
            if (agentrelationmodel == null || agentrelationmodel.ParentAgentId != agentinfo.id)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            AgentFollowLog agentlogmodel = AgentFollowLogBLL.SingleModel.GetModel(id);
            if (agentlogmodel == null)
            {
                msg.Msg = "请刷新重试";
                return Json(msg);
            }

            agentlogmodel.UpdateTime = DateTime.Now;
            agentlogmodel.State = -2;
            msg.isok = AgentFollowLogBLL.SingleModel.Update(agentlogmodel, "UpdateTime,State");
            msg.Msg = msg.isok ? "删除成功" : "删除失败";
            AgentFollowLogBLL.SingleModel.RemoveCache(agentdistributionrid, agentrelationmodel.ParentAgentId);
            return Json(msg);
        }
        #endregion

        #region 提交返款

        public ActionResult AgentCaseBackList()
        {
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string starttime = Context.GetRequest("starttime", "");
            string endtime = Context.GetRequest("endtime", "");
            string soucefrom = Context.GetRequest("soucefrom", "");
            string agentname = Context.GetRequest("agentname", "");
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }

            if (agentinfo.IsOpenDistribution == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            int count = 0;
            ViewModel<AgentCaseBack> viewmodel = new ViewModel<AgentCaseBack>();
            viewmodel.DataList = AgentCaseBackBLL.SingleModel.GetAgentCaseBackList(agentname, soucefrom, starttime, endtime, agentinfo.id.ToString(), pageIndex, pageSize, ref count);
            viewmodel.TotalCount = count;
            viewmodel.PageIndex = pageIndex;
            viewmodel.PageSize = pageSize;
            ViewBag.starttime = starttime;
            ViewBag.endtime = endtime;

            OpenConfig(agentinfo);
            return View(viewmodel);
        }
        public ActionResult AddAgentCaseBack()
        {
            int agentdistributionrid = Context.GetRequestInt("agentdistributionrid", 0);
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }

            AgentDistributionRelation agentrelationmodel = _agentDistributionRelationBLL.GetModel(agentdistributionrid);
            if (agentrelationmodel == null || agentrelationmodel.ParentAgentId != agentinfo.id || agentinfo.IsOpenDistribution == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            ViewBag.AgentDistributionRid = agentdistributionrid;
            ViewBag.AgentQrcodeId = agentrelationmodel.QrCodeId;
            OpenConfig(agentinfo);

            return View();
        }


        public ActionResult SaveAgentCaseBack()
        {
            int agentdistributionrid = Context.GetRequestInt("agentdistributionrid", 0);
            int invoice = Context.GetRequestInt("invoice", 0);
            string imgUrl = Context.GetRequest("imgurl", string.Empty);
            string bankAccount = Context.GetRequest("bankaccount", string.Empty);
            string alipayAccount = Context.GetRequest("alipayaccount", string.Empty);
            string courierNumber = Context.GetRequest("couriernumber", string.Empty);

            if (agentdistributionrid <= 0)
            {
                msg.Msg = "参数错误d_null";
                return Json(msg);
            }
            if (string.IsNullOrEmpty(bankAccount))
            {
                msg.Msg = "银行账号不能为空";
                return Json(msg);
            }
            if (string.IsNullOrEmpty(alipayAccount))
            {
                msg.Msg = "支付宝账号不能为空";
                return Json(msg);
            }
            if (invoice == 0)
            {
                if (string.IsNullOrEmpty(courierNumber))
                {
                    msg.Msg = "快递单号不能为空";
                    return Json(msg);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(imgUrl))
                {
                    msg.Msg = "请上传电子发票图片";
                    return Json(msg);
                }
            }

            //判断是不是代理
            if (agentinfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "您还不是代理商!", code = "403" });
            }

            AgentDistributionRelation agentrelationmodel = _agentDistributionRelationBLL.GetModel(agentdistributionrid);
            if (agentrelationmodel == null || agentrelationmodel.ParentAgentId != agentinfo.id)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            AgentCaseBack agentcasebackgmodel = new AgentCaseBack();
            agentcasebackgmodel.AgentDistributionRelatioinId = agentdistributionrid;
            agentcasebackgmodel.AddTime = DateTime.Now;
            agentcasebackgmodel.UpdateTime = DateTime.Now;
            agentcasebackgmodel.Invoice = invoice;
            agentcasebackgmodel.AlipayAccount = alipayAccount;
            agentcasebackgmodel.ImgUrl = imgUrl;
            agentcasebackgmodel.BankAccount = bankAccount;
            agentcasebackgmodel.CourierNumber = courierNumber;
            agentcasebackgmodel.State = 0;
            //分销用户信息
            List<DistributionUserInfo> duserinfo = _agentDistributionRelationBLL.GetDistributionUserInfo(agentdistributionrid);
            if (duserinfo != null && duserinfo.Count > 0)
            {
                agentcasebackgmodel.DUserInfo = JsonConvert.SerializeObject(duserinfo);
            }

            agentcasebackgmodel.Id = Convert.ToInt32(AgentCaseBackBLL.SingleModel.Add(agentcasebackgmodel));

            msg.isok = agentcasebackgmodel.Id > 0;
            msg.Msg = msg.isok ? "保存成功" : "保存失败";
            AgentCaseBackBLL.SingleModel.RemoveCache(agentinfo.id);
            return Json(msg);
        }
        #endregion

        #region 商家返款处理
        public ActionResult AgentCustomerCaseBack()
        {
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string starttime = Context.GetRequest("starttime", "");
            string endtime = Context.GetRequest("endtime", "");
            string soucefrom = Context.GetRequest("soucefrom", "");
            string agentname = Context.GetRequest("agentname", "");
            if (agentinfo == null)
            {
                return Redirect("/dzhome/login");
            }

            if (agentinfo.IsOpenDistribution == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            int count = 0;
            ViewModel<AgentCaseBack> viewmodel = new ViewModel<AgentCaseBack>();
            viewmodel.DataList = AgentCaseBackBLL.SingleModel.GetAgentCaseBackList(agentname, soucefrom, starttime, endtime, agentinfo.id.ToString(), pageIndex, pageSize, ref count, false, 1);
            viewmodel.TotalCount = count;
            viewmodel.PageIndex = pageIndex;
            viewmodel.PageSize = pageSize;
            ViewBag.starttime = starttime;
            ViewBag.endtime = endtime;
            ViewBag.SouceFrom = soucefrom;
            ViewBag.AgentName = agentname;

            OpenConfig(agentinfo);
            return View(viewmodel);
        }

        public ActionResult CommandRequest()
        {
            msg = new Return_Msg();
            int id = Context.GetRequestInt("id", 0);
            int state = Context.GetRequestInt("state", 0);
            string desc = Context.GetRequest("desc", "");
            string imgurl = Context.GetRequest("imgurl", "");
            if (agentinfo == null)
            {
                msg.Msg = "登陆过期，请重新登陆";
                return Json(msg);
            }
            if (id <= 0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            if (string.IsNullOrEmpty(desc))
            {
                msg.Msg = "请输入驳回理由";
                return Json(msg);
            }
            AgentCaseBack casebackmodel = AgentCaseBackBLL.SingleModel.GetModel(id);
            if (casebackmodel == null)
            {
                msg.Msg = "返款请求数据过期，请刷新重试";
                return Json(msg);
            }

            casebackmodel.State = state;
            casebackmodel.CaseBackImgUrl = imgurl;
            casebackmodel.UpdateTime = DateTime.Now;
            casebackmodel.Desc = desc;
            msg.isok = AgentCaseBackBLL.SingleModel.Update(casebackmodel, "CaseBackImgUrl,State,Desc,state,updatetime");
            msg.Msg = msg.isok ? "保存成功" : "保存失败";
            AgentCaseBackBLL.SingleModel.RemoveCache(agentinfo.id);

            return Json(msg);
        }
        #endregion

        #endregion

        #region 扣费提示
        public ActionResult GetCostMsg(int type=0,List<XcxTemplate> list=null)
        {
            msg = new Return_Msg();
            if (list==null || list.Count<=0)
            {
                msg.Msg = "数据错误";
                return Json(msg);
            }

            int sum = 0;
            int parent_sum = 0;
            string errorMsg = "";
            switch (type)
            {
                case 0://模板续费
                    //检验代理预存
                    List<XcxTemplate> xcxlist = AgentinfoBLL.SingleModel.CheckParentAgentDeposit(agentinfo.id, list[0].VersionId, "", list, ref sum, ref errorMsg);
                    if (errorMsg.Length > 0)
                    {
                        msg.Msg = errorMsg;
                        return Json(msg);
                    }

                    //分销商：检验上级代理预存
                    Distribution distribution = null;
                    if (agentinfo.userLevel == 1)
                    {
                        distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentinfo.id);
                        AgentinfoBLL.SingleModel.CheckParentAgentDeposit(distribution.parentAgentId, list[0].VersionId, "", list, ref parent_sum, ref errorMsg);
                        if (errorMsg.Length > 0)
                        {
                            msg.Msg = errorMsg;
                            return Json(msg);
                        }
                    }
                    break;
            }

            msg.dataObj = (sum*0.01).ToString("0.00");
            msg.isok = true;

            return Json(msg);
        }
        #endregion

        /// <summary>
        /// 检测代理密码
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckAgentPassowrd()
        {
            Return_Msg returndata = new Return_Msg();
            string password = Context.GetRequest("password", "");
            if (string.IsNullOrEmpty(password))
            {
                returndata.Msg = "请输入密码";
                return Json(returndata);
            }
            if (agentinfo == null)
            {
                returndata.Msg = "代理账号过期，请重新登陆";
                return Json(returndata);
            }
            if (!string.IsNullOrEmpty(agentinfo.Password) && agentinfo.Password != password)
            {
                returndata.Msg = "密码错误";
                return Json(returndata);
            }
            AgentinfoBLL.SingleModel.SavePassword(agentinfo.id, agentinfo.Password);
            returndata.isok = true;
            return Json(returndata);
        }

        /// <summary>
        /// 开启配置
        /// </summary>
        private void OpenConfig(Agentinfo agentinfo)
        {
            ViewBag.IsOpenDistribution = agentinfo.IsOpenDistribution;
            ViewBag.OpenFenXiao = agentinfo.OpenFenXiao;
            ViewBag.CanOpenDistribution = false;
            ViewBag.AgentType = agentinfo.AgentType;
            ViewBag.level = agentinfo.userLevel;

            ViewBag.Password = AgentinfoBLL.SingleModel.IsExitePassword(agentinfo);
            ViewBag.appId = 0;
            if (agentinfo.userLevel == 0)
            {
                if (DistributionBLL.SingleModel.ExiteDistribution(agentinfo.id))
                {
                    ViewBag.CanOpenDistribution = true;
                }
            }
        }
        
        #region 自定义水印
        /// <summary>
        /// 保存编辑水印数据
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveCustomBottom()
        {
            Return_Msg returndata = new Return_Msg();
            int aid = Context.GetRequestInt("aid", 0);
            string datajson = Context.GetRequest("datajson", "");
            if (aid <= 0 || string.IsNullOrEmpty(datajson))
            {
                returndata.Msg = "参数错误";
                return Json(returndata);
            }
            if (agentinfo == null)
            {
                returndata.Msg = "代理信息过期，请重新登陆";
                return Json(returndata);
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null || xcxrelation.agentId != agentinfo.id)
            {
                returndata.Msg = "客户信息过期，请刷新重试";
                return Json(returndata);
            }

            List<ConfParam> parmlist = ConfParamBLL.SingleModel.GetListByRId(aid, "'agentcustomlogo'");
            if (parmlist == null || parmlist.Count <= 0)
            {
                returndata.Msg = "操作过期，请刷新重试";
                return Json(returndata);
            }

            string msg = "";
            returndata.isok = ConfParamBLL.SingleModel.SaveData(xcxrelation, datajson, ref msg);
            returndata.Msg = returndata.isok ? "保存成功" : msg;
            return Json(returndata);
        }
        /// <summary>
        /// 开启水印
        /// </summary>
        /// <returns></returns>
        public ActionResult OpenCustomBottom()
        {
            Return_Msg returndata = new Return_Msg();
            int aid = Context.GetRequestInt("aid", 0);
            string username = Context.GetRequest("username", "");
            string parm = "agentcustomlogo";
            int cost = 300 * 100;//单位分
            if (aid <= 0)
            {
                returndata.Msg = "参数错误";
                return Json(returndata);
            }
            if (agentinfo == null)
            {
                returndata.Msg = "代理信息过期，请重新登陆";
                return Json(returndata);
            }
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModel(agentinfo.id);
            if (agentmodel == null || agentmodel.state < 0)
            {
                returndata.Msg = "代理过期";
                return Json(returndata);
            }
            if (agentmodel.deposit < cost)
            {
                returndata.Msg = "您的预存款不足，请联系客服充值！";
                return Json(returndata);
            }
            Agentinfo parentAgentinfo = new Agentinfo();
            if (agentmodel.userLevel > 0)
            {
                Distribution distribution = DistributionBLL.SingleModel.GetModelByAgentId(agentmodel.id);
                parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                if (parentAgentinfo == null)
                {
                    returndata.Msg = "您的上级代理账号已过期，请联系您的上级代理";
                    return Json(returndata);
                }
                if (parentAgentinfo.deposit < cost)
                {
                    returndata.Msg = "您的上级代理预存款不足，请联系您的上级代理！";
                    return Json(returndata);
                }
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null || xcxrelation.agentId != agentmodel.id)
            {
                returndata.Msg = "客户信息过期，请刷新重试";
                return Json(returndata);
            }
            List<ConfParam> parmlist = ConfParamBLL.SingleModel.GetListByRId(aid, $"'{parm}'");
            if (parmlist != null && parmlist.Count > 0)
            {
                returndata.Msg = "操作频繁，请刷新重试";
                return Json(returndata);
            }

            string msg = "";
            returndata.isok = AgentdepositLogBLL.SingleModel.OpenCustomBotton(agentmodel, parentAgentinfo, username, xcxrelation, cost, parm);
            returndata.Msg = returndata.isok ? "保存成功" : msg;
            return Json(returndata);
        }
        #endregion
    }
}