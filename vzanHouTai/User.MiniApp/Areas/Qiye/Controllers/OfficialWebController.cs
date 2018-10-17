using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Qiye;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Model;
using Utility.IO;

namespace User.MiniApp.Areas.Qiye.Controllers
{
    public class OfficialWebController : User.MiniApp.Controllers.baseController
    {
        
        public static readonly ReturnMsg result = new ReturnMsg();

        /// <summary>
        /// 实例化对象
        /// </summary>
        public OfficialWebController()
        { 
        }

        #region 企业信息
        public ActionResult Index(int aid = 0)
        {
            ViewBag.appId = aid;
            Miniapp listTemp = MiniappBLL.SingleModel.GetModelByRelationId(aid);

            if (listTemp == null)
            {
                XcxAppAccountRelation xrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
                listTemp = new Miniapp();
                listTemp.CreateDate = DateTime.Now;
                listTemp.Description = "官网小程序";
                listTemp.OpenId = dzaccount.ToString();
                listTemp.xcxRelationId = aid;
                listTemp.State = 1;
                listTemp.ModelId = xrelationmodel.AppId;
                listTemp.Id = Convert.ToInt32(MiniappBLL.SingleModel.Add(listTemp));
                if (listTemp.Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "添加出错!", code = "500" });
                }
            }

            if (listTemp.Description == "官网小程序")
            {
                listTemp.Description = "";
            }

            //首页数据
            Moduls moduls = ModulsBLL.SingleModel.GetModelByAppidandLevel(listTemp.Id, (int)Miapp_Miniappmoduls_Level.ModelData);
            if (moduls != null)
            {
                listTemp.StoreName = string.IsNullOrEmpty(listTemp.StoreName) ? moduls.Name : listTemp.StoreName;
            }
            List<C_Attachment> imgs = C_AttachmentBLL.SingleModel.GetListByCache(listTemp.Id, (int)AttachmentItemType.小程序智慧官网形象图, true);

            listTemp.BannersImgUrls = new List<string>();
            if (imgs != null && imgs.Count > 0)
            {
                foreach (C_Attachment item in imgs)
                {
                    listTemp.BannersImgUrls.Add(item.filepath);
                }
            }

            //联系我数据
            moduls = ModulsBLL.SingleModel.GetModelByAppidandLevel(listTemp.Id, (int)Miapp_Miniappmoduls_Level.SixModel);
            if (moduls != null)
            {
                listTemp.Phone = string.IsNullOrEmpty(listTemp.Phone) ? moduls.mobile : listTemp.Phone;
                listTemp.Address = string.IsNullOrEmpty(listTemp.Address) ? moduls.Address : listTemp.Address;
                listTemp.Location = string.IsNullOrEmpty(listTemp.Location) ? moduls.AddressPoint : listTemp.Location;
            }

            return View(listTemp);
        }

        [ValidateInput(false)]
        public ActionResult SaveDataInfo(int aid = 0, Miniapp model = null, string[] Banners = null)
        {
            Return_Msg returnData = new Return_Msg();
            if (model == null || model.Id <= 0)
            {
                returnData.Msg = "无效数据";
                return Json(returnData);
            }
            if (Banners != null && Banners.Length > 0)
            {
                C_AttachmentBLL.SingleModel.DeleteALL(model.Id, (int)AttachmentItemType.小程序智慧官网形象图);
                C_AttachmentBLL.SingleModel.AddImgList(Banners, (int)AttachmentItemType.小程序智慧官网形象图, model.Id);
            }
            returnData.isok = MiniappBLL.SingleModel.Update(model);
            returnData.Msg = returnData.isok ? "保存成功" : "保存失败";
            return Json(returnData);
        }
        #endregion

        #region 发展历程
        public ActionResult Development(int aid = 0)
        {
            ViewBag.appId = aid;

            return View();
        }

        /// <summary>
        /// 获取发展历程数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult GetDevelopmentDataList(int aid = 0, int pageSize = 10, int pageIndex = 1)
        {
            Return_Msg returnData = new Return_Msg();
            Miniapp model = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (model == null)
            {
                returnData.Msg = "无效数据";
                return Json(returnData);
            }

            List<Development> list = DevelopmentBLL.SingleModel.GetListByAppid(model.Id, pageIndex, pageSize);
            int count = DevelopmentBLL.SingleModel.GetListByAppidCount(model.Id);
            returnData.dataObj = new { data = list, count = count };
            returnData.isok = true;
            return Json(returnData);
        }

        /// <summary>
        /// 保存发展历程数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public ActionResult SaveDevelopmentData(int aid = 0, int id = 0, string year = "", string month = "", string content = "")
        {
            Return_Msg returnData = new Return_Msg();
            Development model = new Development();
            model.Year = year;
            model.Month = month;
            model.Content = content;
            model.Id = id;
            Miniapp bmodel = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (bmodel == null)
            {
                returnData.Msg = "无效数据";
                return Json(returnData);
            }
            if (model.Id <= 0)
            {
                model.Level = (int)Miapp_Miniappmoduls_Level.FiveModel;
                model.Createdate = DateTime.Now;
                model.State = 1;
                model.appId = bmodel.Id;
                model.Lastdate = DateTime.Now;
                model.Id = Convert.ToInt32(DevelopmentBLL.SingleModel.Add(model));
                returnData.isok = model.Id > 0;
            }
            else
            {
                model.Lastdate = DateTime.Now;
                returnData.isok = DevelopmentBLL.SingleModel.Update(model, "lastdate,year,month,content");
            }
            returnData.Msg = returnData.isok ? "保存成功" : "保存失败";
            return Json(returnData);
        }

        /// <summary>
        /// 删除发展历程数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DelDevelopmentData(int aid = 0, int id = 0)
        {
            Return_Msg returnData = new Return_Msg();
            Development model = DevelopmentBLL.SingleModel.GetModel(id);
            Miniapp bmodel = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (bmodel == null)
            {
                returnData.Msg = "无效数据";
                return Json(returnData);
            }
            if (model == null)
            {
                returnData.Msg = "请刷新重试";
                return Json(returnData);
            }
            if (model.appId != bmodel.Id)
            {
                returnData.Msg = "系统繁忙";
                return Json(returnData);
            }
            model.State = -1;
            model.Lastdate = DateTime.Now;
            returnData.isok = DevelopmentBLL.SingleModel.Update(model, "state,Lastdate");
            returnData.Msg = returnData.isok ? "删除成功" : "删除失败";

            return Json(returnData);
        }
        #endregion

        #region 企业资讯
        public ActionResult CompanyNews(int aid = 0)
        {
            ViewBag.appId = aid;
            return View();
        }

        /// <summary>
        /// 获取资讯数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult GetCompanyNewsDataList(int aid = 0, int pageSize = 10, int pageIndex = 1)
        {
            Return_Msg returnData = new Return_Msg();
            Miniapp bmodel = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (bmodel == null)
            {
                returnData.Msg = "无效数据";
                return Json(returnData);
            }
            List<Moduls> list = ModulsBLL.SingleModel.GetListByAppidandLevel(bmodel.Id, (int)Miapp_Miniappmoduls_Level.EightModel, pageIndex, pageSize);
            int count = ModulsBLL.SingleModel.GetListByAppidandLevelCount(bmodel.Id, (int)Miapp_Miniappmoduls_Level.EightModel);
            returnData.dataObj = new { data = list, count = count };
            returnData.isok = true;
            return Json(returnData);
        }

        [ValidateInput(false)]
        public ActionResult AddOrEditCompanyNews(string act = "add", int aid = 0, int id = 0, Moduls model = null)
        {
            ViewBag.appId = aid;
            Miniapp bmodel = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (bmodel == null)
            {
                return Redirect("/base/PageError?type=1");
            }

            if (act == "add")
            {
                return View(new Moduls());
            }
            else if (act == "edite")
            {
                model = ModulsBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    return Redirect("/base/PageError?type=1");
                }
                return View(model);
            }
            else if (act == "save")
            {
                Return_Msg returnData = new Return_Msg();
                if (model == null)
                {
                    return Redirect("/base/PageError?type=2");
                }
                if (model.Id <= 0)
                {
                    model.Createdate = DateTime.Now;
                    model.Lastdate = DateTime.Now;
                    model.State = 1;
                    model.Level = (int)Miapp_Miniappmoduls_Level.EightModel;
                    model.appId = bmodel.Id;
                    model.Id = Convert.ToInt32(ModulsBLL.SingleModel.Add(model));
                    returnData.isok = model.Id > 0;
                }
                else
                {
                    model.Lastdate = DateTime.Now;
                    model.State = 1;
                    returnData.isok = ModulsBLL.SingleModel.Update(model, "VideoUrl,State,Lastdate,Title,Content,Content2,ImgUrl,Sort,VideoType");
                }
                returnData.Msg = returnData.isok ? "保存成功" : "保存失败";
                return Json(returnData);
            }

            return Redirect("/base/PageError?type=2");
        }

        /// <summary>
        /// 删除资讯数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DelCompanyData(int aid = 0, int id = 0)
        {
            Return_Msg returnData = new Return_Msg();
            Moduls model = ModulsBLL.SingleModel.GetModel(id);
            Miniapp bmodel = MiniappBLL.SingleModel.GetModelByRelationId(aid);
            if (bmodel == null)
            {
                returnData.Msg = "无效数据";
                return Json(returnData);
            }
            if (model == null)
            {
                returnData.Msg = "请刷新重试";
                return Json(returnData);
            }
            if (model.appId != bmodel.Id)
            {
                returnData.Msg = "系统繁忙";
                return Json(returnData);
            }
            model.State = -1;
            model.Lastdate = DateTime.Now;
            returnData.isok = ModulsBLL.SingleModel.Update(model, "state,Lastdate");
            returnData.Msg = returnData.isok ? "删除成功" : "删除失败";

            return Json(returnData);
        }
        #endregion

        #region 企业员工与部门管理
        public ActionResult DepartmentMgr(int aid = 0)
        {
            ViewBag.appId = aid;
            return View();
        }

        public ActionResult GetDepartmentList(int aid = 0, int pageSize = 10, int pageIndex = 1)
        {
            Return_Msg returnData = new Return_Msg();

            int totalCount = 0;
            List<QiyeDepartment> list = QiyeDepartmentBLL.SingleModel.GetQiyeDepartmentList(aid, out totalCount, pageSize, pageIndex);
            returnData.dataObj = new { data = list, count = totalCount };
            returnData.isok = true;
            return Json(returnData);
        }

        public ActionResult SaveDepartment(int aid, QiyeDepartment model)
        {
            Return_Msg returnObj = new Return_Msg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            if (dzaccount == null)
            {
                returnObj.Msg = "登录信息过期请重新登录";
                return Json(returnObj);
            }

            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);


            #region 添加和修改
            if (model == null || model.Id < 0)
            {
                returnObj.Msg = "非法请求";
                return Json(returnObj);
            }
            if (model.Name.Trim() == "" || model.Name.Trim().Length > 10)
            {
                returnObj.Msg = "名称不能为空，且不能超过10个字";
                return Json(returnObj);

            }
            QiyeDepartment department = QiyeDepartmentBLL.SingleModel.GetDepartmentByName(aid, model.Name);
            //修改
            if (model.Id > 0)
            {
                if (act == "del")
                {
                    int employeeCount = QiyeDepartmentBLL.SingleModel.GetEmployeeCount(model.Id);
                    if (employeeCount > 0)
                    {
                        returnObj.Msg = $"删除失败,该部门下还有{employeeCount}个员工！";
                        return Json(returnObj);
                    }

                    model.State = -1;
                }
                model.UpdateTime = DateTime.Now;
                if (department != null && department.Id != model.Id && act != "del")
                {
                    returnObj.Msg = "已存在该部门，请重新设置！";
                    return Json(returnObj);

                }

                if (QiyeDepartmentBLL.SingleModel.Update(model))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);
                }
            }
            //添加
            else
            {
                if (department != null)
                {
                    returnObj.Msg = "已存在该部门，请重新设置！";
                    return Json(returnObj);
                }
                model.AddTime = DateTime.Now;
                model.UpdateTime = DateTime.Now;

                int newid = Convert.ToInt32(QiyeDepartmentBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    returnObj.isok = true;
                    returnObj.dataObj = model;
                    returnObj.Msg = "新增成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "新增失败！";
                    return Json(returnObj);
                }
            }
            #endregion

            returnObj.Msg = "操作异常！";
            return Json(returnObj);
        }

        /// <summary>
        /// 员工管理
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult QiyeEmployeeMgr(int aid = 0)
        {
            ViewBag.appId = aid;
            int totalCountDepart = 0;
            List<QiyeDepartment> listDepart = QiyeDepartmentBLL.SingleModel.GetQiyeDepartmentList(aid, out totalCountDepart, 1000, 1);
            if (listDepart == null)
            {
                listDepart = new List<QiyeDepartment>();
            }
            listDepart.Insert(0, new QiyeDepartment() { Id = -1, Name = "全部" });
            ViewBag.listDepart = listDepart;



            return View();
        }

        public ActionResult GetEmployeeList(int aid = 0, int pageSize = 10, int pageIndex = 1)
        {
            Return_Msg returnData = new Return_Msg();
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcx == null)
            {
                returnData.Msg = "小程序未授权";
                return Json(returnData);
            }

            int wxBindState = Context.GetRequestInt("wxBindState", -1);//微信绑定状态 -1表示全部
            int departMentId = Context.GetRequestInt("departMentId", -1);//部门ID
            int workState = Context.GetRequestInt("workState", -2);//在职状态 -1表示全部
            string searchKey = Context.GetRequest("searchKey", string.Empty);//搜索的关键词 姓名或者手机号码
            int totalCount = 0;
            int totalCountDepart = 0;
            List<QiyeDepartment> listDepart = QiyeDepartmentBLL.SingleModel.GetQiyeDepartmentList(aid, out totalCountDepart, 1000, 1);
            List<QiyeEmployee> list = QiyeEmployeeBLL.SingleModel.GetListQiyeEmployee(aid, xcx.AppId, out totalCount, searchKey, wxBindState, departMentId, workState, pageSize, pageIndex);
            if (listDepart != null && list != null)
            {
                string userIds = string.Join(",",list.Where(w=>w.UserId>0)?.Select(s=>s.UserId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                list.ForEach(x =>
                {
                    QiyeDepartment department = listDepart.Find(y => y.Id == x.DepartmentId);
                    if (department != null)
                    {
                        x.DepartMentName = department.Name;
                    }

                    if (x.UserId > 0)
                    {
                        //表示绑定了微信号
                        C_UserInfo c_UserInfo = userInfoList?.FirstOrDefault(f=>f.Id == x.UserId);
                        if (c_UserInfo != null)
                        {
                            x.WxBindUserInfo.UserId = c_UserInfo.Id;
                            x.WxBindUserInfo.UserName = c_UserInfo.NickName;
                            x.WxBindUserInfo.Avatar = c_UserInfo.HeadImgUrl;
                        }
                    }

                    QiyeMsgviewFavoriteShare qiyeMsgviewFavoriteShare = QiyeMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, x.Id, (int)PointsDataType.名片);
                    if (qiyeMsgviewFavoriteShare != null)
                    {
                        x.PV = qiyeMsgviewFavoriteShare.ViewCount;
                    }

                    x.CustomerNum= QiyeCustomerBLL.SingleModel.GetEmployeeCustomerCount(x.Id);

                });
            }

            int curKefuCount = QiyeEmployeeBLL.SingleModel.GetQiyeEmployeeKefuCount(aid, xcx.AppId);
            returnData.dataObj = new { list = list, count = totalCount, curKefuCount = curKefuCount };
            returnData.isok = true;
            return Json(returnData);
        }

        [HttpPost]
        public ActionResult SaveEmployee(QiyeEmployee employee)
        {
            Return_Msg returnData = new Return_Msg();
            int aid = Context.GetRequestInt("aid",0);
            if (employee == null)
            {
                returnData.Msg = "数据不能为空";
                return Json(returnData);
            }

            if (aid != employee.Aid)
            {
                returnData.Msg = "暂无权限";
                return Json(returnData);
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcx == null)
            {
                returnData.Msg = "小程序未授权";
                return Json(returnData);
            }
            int curKefuCount = QiyeEmployeeBLL.SingleModel.GetQiyeEmployeeKefuCount(aid, xcx.AppId, employee.Id);
            if (employee.Kefu == 1 && curKefuCount >= 6)
            {
                returnData.Msg = "最多可设置6个客服";
                return Json(returnData);
            }

            QiyeEmployee tempQiyeEmployee = QiyeEmployeeBLL.SingleModel.GetQiyeEmployeeByWorkId(aid, employee.WorkID, xcx.AppId);
            if (tempQiyeEmployee != null && tempQiyeEmployee.Id != employee.Id)
            {
                returnData.Msg = "该员工码已经存在了!请重新设置";
                return Json(returnData);
            }

          

            if (employee.Id <= 0)
            {
                //TODO 进行数量的判断  看是否允许添加
                //表示新增
                int curEmployeeCount = QiyeEmployeeBLL.SingleModel.GetQiyeEmployeeCount(aid, xcx.AppId);
             //   log4net.LogHelper.WriteInfo(this.GetType(),$"curEmployeeCount={curEmployeeCount.ToString()};aid={aid};appId={xcx.AppId};employee.appId={employee.AppId}");
                if (xcx.SCount <=curEmployeeCount)
                {
                    returnData.Msg = $"添加数量已达上限{xcx.SCount}，请联系客服";
                    return Json(returnData);
                }
                employee.AppId = xcx.AppId;
                employee.AddTime = DateTime.Now;
                employee.UpdateTime = DateTime.Now;
                int id = Convert.ToInt32(QiyeEmployeeBLL.SingleModel.Add(employee));
                if (id > 0)
                {
                    if (employee.Kefu == 1)
                    {
                        QiyeEmployeeBLL.SingleModel.SetKeFuListToRedis(aid, xcx.AppId);
                    }

                    returnData.isok = true;
                    returnData.Msg = "添加成功";
                    return Json(returnData);
                }
                else
                {
                    returnData.Msg = "添加失败";
                    return Json(returnData);
                }


            }
            else
            {
                tempQiyeEmployee = QiyeEmployeeBLL.SingleModel.GetModel(employee.Id);
                if (tempQiyeEmployee == null || tempQiyeEmployee.State == -1)
                {
                    returnData.Msg = "修改的数据不存在";
                    return Json(returnData);
                }


                employee.UpdateTime = DateTime.Now;
                if (QiyeEmployeeBLL.SingleModel.Update(employee))
                {
                    if (tempQiyeEmployee.Kefu != employee.Kefu)
                    {
                        QiyeEmployeeBLL.SingleModel.SetKeFuListToRedis(aid, xcx.AppId);
                    }

                    returnData.isok = true;
                    returnData.Msg = "修改成功";
                    return Json(returnData);
                }
                else
                {
                    returnData.Msg = "修改失败";
                    return Json(returnData);
                }
                //表示修改
            }



        }

        public ActionResult DelEmployee(int aid = 0, int Id = 0)
        {
            Return_Msg returnData = new Return_Msg();
            if (aid <= 0 || Id <= 0)
            {
                returnData.Msg = "参数错误";
                return Json(returnData);
            }

            QiyeEmployee employee = QiyeEmployeeBLL.SingleModel.GetModel(Id);
            if (employee == null)
            {
                returnData.Msg = "数据不存在";
                return Json(returnData);
            }

            if (employee.Aid != aid)
            {
                returnData.Msg = "暂无权限";
                return Json(returnData);
            }

            employee.State = -1;
            employee.UpdateTime = DateTime.Now;
            if (!QiyeEmployeeBLL.SingleModel.Update(employee, "State,UpdateTime"))
            {
                returnData.Msg = "删除失败";
                return Json(returnData);
            }


            returnData.isok = true;
            returnData.Msg = "删除成功";
            return Json(returnData);
        }
        #endregion

        #region 产品类别管理

        public ActionResult ProductCategroyMgr(int aid = 0, int isFirstType = 2)
        {
            if (aid <= 0)
                return View("PageError", new QiyeReturnMsg() { Msg = "参数错误!", code = "500" });

            //int pageType = _xcxappaccountrelationBll.GetXcxTemplateType(aid);
            //if (pageType <= 0)
            //    return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });



            QiyeGoodsCategoryConfig qiyeGoodsCategoryConfig = QiyeGoodsCategoryConfigBLL.SingleModel.GetModelByAid(aid);
            QiyeSwitchModel switchModel = new QiyeSwitchModel();

            if (qiyeGoodsCategoryConfig == null)
            {

                qiyeGoodsCategoryConfig = new QiyeGoodsCategoryConfig() { Aid = aid, AddTime = DateTime.Now, UpdateTime = DateTime.Now };
                qiyeGoodsCategoryConfig.SwitchConfig = JsonConvert.SerializeObject(switchModel);
                if (Convert.ToInt32(QiyeGoodsCategoryConfigBLL.SingleModel.Add(qiyeGoodsCategoryConfig)) <= 0)
                {
                    return View("PageError", new QiyeReturnMsg() { Msg = "初始化失败!", code = "500" });
                }

            }

            ViewBag.ProductCategoryLevel = 1;
            if (!string.IsNullOrEmpty(qiyeGoodsCategoryConfig.SwitchConfig))
            {
                switchModel = JsonConvert.DeserializeObject<QiyeSwitchModel>(qiyeGoodsCategoryConfig.SwitchConfig);
                ViewBag.ProductCategoryLevel = switchModel.ProductCategoryLevel;
            }


            int totalCount = 0;
            List<QiyeGoodsCategory> list = new List<QiyeGoodsCategory>();
            list.Add(new QiyeGoodsCategory()
            {
                Id = 0,
                Name = "请选择"
            });
            list.AddRange(QiyeGoodsCategoryBLL.SingleModel.getListByaid(aid, out totalCount, 0, 100, 1));

            ViewBag.isFirstType = isFirstType;

            ViewBag.appId = aid;
            return View(list);
        }


        public ActionResult UpdateProductCategoryLevel(int aid = 0)
        {
            Return_Msg returnObj = new Return_Msg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            //int pageType = _xcxappaccountrelationBll.GetXcxTemplateType(aid);
            //if (pageType <= 0)
            //{
            //    returnObj.Msg = "小程序模板不存在";
            //    return Json(returnObj);
            //}

            QiyeGoodsCategoryConfig qiyeGoodsCategoryConfig = QiyeGoodsCategoryConfigBLL.SingleModel.GetModelByAid(aid);

            if (qiyeGoodsCategoryConfig == null)
            {
                returnObj.Msg = "数据不存在";
                return Json(returnObj);

            }
            QiyeSwitchModel switchModel = new QiyeSwitchModel();

            if (!string.IsNullOrEmpty(qiyeGoodsCategoryConfig.SwitchConfig))
            {
                switchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<QiyeSwitchModel>(qiyeGoodsCategoryConfig.SwitchConfig);
                switchModel.ProductCategoryLevel = switchModel.ProductCategoryLevel == 1 ? 2 : 1;
            }

            qiyeGoodsCategoryConfig.SwitchConfig = Newtonsoft.Json.JsonConvert.SerializeObject(switchModel);

            if (QiyeGoodsCategoryConfigBLL.SingleModel.Update(qiyeGoodsCategoryConfig, "SwitchConfig"))
            {
                returnObj.isok = true;
                returnObj.Msg = "操作成功";
                return Json(returnObj);
            }
            else
            {

                returnObj.Msg = "操作失败";
                return Json(returnObj);
            }



        }



        /// <summary>
        /// 获取店铺产品分类
        /// </summary>
        /// <returns></returns>

        public ActionResult GetCategoryList()
        {
            Return_Msg returnObj = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int isFirstType = Utility.IO.Context.GetRequestInt("isFirstType", 1);
            int parentId = Utility.IO.Context.GetRequestInt("parentId", 0);
            if (appId <= 0)
            {
                returnObj.Msg = "appId非法";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int isList = Utility.IO.Context.GetRequestInt("isList", 0);
            int totalCount = 0;
            List<QiyeGoodsCategory> list = new List<QiyeGoodsCategory>();
            if (isList != 0)
            {
                list.Add(new QiyeGoodsCategory()
                {
                    Id = 0,
                    Name = "请选择"
                });
            }

            list.AddRange(QiyeGoodsCategoryBLL.SingleModel.getListByaid(appId, out totalCount, isFirstType, pageSize, pageIndex, "sortNumber desc,addTime desc", parentId));

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 类别名称是否存在
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckCategoryName()
        {
            Return_Msg returnObj = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            int isFirstType = Utility.IO.Context.GetRequestInt("isFirstType", 0);
            string categoryName = Utility.IO.Context.GetRequest("categoryName", string.Empty);
            if (appId <= 0 || string.IsNullOrEmpty(categoryName))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            QiyeGoodsCategory model = QiyeGoodsCategoryBLL.SingleModel.msgTypeNameIsExist(appId, categoryName);
            if (model != null && model.Id != Id)
            {
                returnObj.Msg = "类别名称已存在";
                return Json(returnObj);
            }
            returnObj.isok = true;
            returnObj.Msg = "ok";
            return Json(returnObj);
        }



        /// <summary>
        /// 编辑或者新增 信息分类
        /// </summary>
        /// <param name="city_Storemsgrules"></param>
        /// <returns></returns>

        public ActionResult SaveCategory(QiyeGoodsCategory QiyeProductCategory)
        {

            Return_Msg returnObj = new Return_Msg();
            if (QiyeProductCategory == null)
            {
                returnObj.Msg = "数据不能为空";
                return Json(returnObj);
            }

            if (QiyeProductCategory.AId <= 0)
            {
                returnObj.Msg = "appId非法";
                return Json(returnObj);
            }



            int Id = QiyeProductCategory.Id;
            if (Id == 0)
            {
                QiyeGoodsCategory model = new QiyeGoodsCategory()
                {
                    MaterialPath = QiyeProductCategory.MaterialPath,
                    Name = QiyeProductCategory.Name,
                    AddTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    AId = QiyeProductCategory.AId,
                    State = 0,
                    SortNumber = QiyeProductCategory.SortNumber,
                    ParentId = QiyeProductCategory.ParentId

                };
                //表示新增
                Id = Convert.ToInt32(QiyeGoodsCategoryBLL.SingleModel.Add(model));
                if (Id > 0)
                {
                    model.Id = Id;
                    returnObj.dataObj = model;
                    returnObj.isok = true;
                    returnObj.Msg = "新增成功";
                    return Json(returnObj);

                }
                else
                {
                    returnObj.Msg = "新增失败";
                    return Json(returnObj);
                }

            }
            else
            {
                //表示更新
                QiyeGoodsCategory model = QiyeGoodsCategoryBLL.SingleModel.GetModel(Id);
                if (model == null)
                {
                    returnObj.Msg = "不存在数据库里";
                    return Json(returnObj);
                }

                if (model.AId != QiyeProductCategory.AId)
                {
                    returnObj.Msg = "权限不足";
                    return Json(returnObj);
                }
                model.UpdateTime = DateTime.Now;
                model.MaterialPath = QiyeProductCategory.MaterialPath;
                model.Name = QiyeProductCategory.Name;
                model.SortNumber = QiyeProductCategory.SortNumber;
                model.ParentId = QiyeProductCategory.ParentId;
                if (QiyeGoodsCategoryBLL.SingleModel.Update(model, "UpdateTime,MaterialPath,Name,SortNumber,ParentId"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "更新成功";
                    return Json(returnObj);

                }
                else
                {
                    returnObj.Msg = "更新失败";
                    return Json(returnObj);

                }

            }

        }


        /// <summary>
        /// 删除信息分类包括单个批量
        /// </summary>
        /// <returns></returns>

        public ActionResult DelCategory()
        {
            Return_Msg returnObj = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            if (appId <= 0)
            {
                returnObj.Msg = "appId非法";
                return Json(returnObj);
            }

            if (!Utility.StringHelper.IsNumByStrs(',', ids))
            {
                returnObj.Msg = "非法操作";
                return Json(returnObj);
            }
            //判断是否有权限
            List<QiyeGoodsCategory> list = QiyeGoodsCategoryBLL.SingleModel.GetListByIds(appId, ids);
            TransactionModel tranModel = new TransactionModel();
            foreach (QiyeGoodsCategory item in list)
            {
                if (appId != item.AId)
                {
                    returnObj.Msg = $"非法操作(无权限对id={item.Id}的类别)";
                    return Json(returnObj);
                }
                int count = 0;
                if (item.ParentId == 0)
                {
                    //表示删除的是大类,要检测有没有子类,没有子类才能删除
                    count = QiyeGoodsCategoryBLL.SingleModel.GetSecondCategoryCount(item.Id);
                    if (count > 0)
                    {
                        returnObj.Msg = $"{item.Name}类别下包含{count}子类别,请先删除子类别再删除)";
                        return Json(returnObj);
                    }
                }
                else
                {
                    //TODO 店铺完善后再检测
                    //表示删除的是子类,要检测该类别下有没有关联的产品,没有产品才能删除
                }





                item.State = -1;
                item.UpdateTime = DateTime.Now;
                tranModel.Add(QiyeGoodsCategoryBLL.SingleModel.BuildUpdateSql(item, "State,UpdateTime"));

            }


            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (QiyeGoodsCategoryBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
                else
                {
                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            else
            {
                returnObj.Msg = "没有需要删除的数据";
                return Json(returnObj);

            }
        }


        /// <summary>
        /// 批量更新排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>

        public ActionResult SaveCategorySort(List<QiyeGoodsCategory> list)
        {
            Return_Msg returnObj = new Return_Msg();

            if (list == null || list.Count <= 0)
            {
                returnObj.Msg = "数据不能为空";
                return Json(returnObj);

            }
            QiyeGoodsCategory model = new QiyeGoodsCategory();
            TransactionModel tranModel = new TransactionModel();
            string sql = string.Empty;

            string categoryIds = string.Join(",",list.Select(s=>s.Id));
            List<QiyeGoodsCategory> qiyeGoodsCategoryList = QiyeGoodsCategoryBLL.SingleModel.GetListByIds(categoryIds);
            foreach (QiyeGoodsCategory item in list)
            {
                model = qiyeGoodsCategoryList?.FirstOrDefault(f=>f.Id == item.Id);
                if (model == null)
                {
                    returnObj.Msg = $"Id={item.Id}不存在数据库里";
                    return Json(returnObj);
                }

                if (model.AId != item.AId)
                {
                    returnObj.Msg = $"Id={item.Id}权限不足";
                    return Json(returnObj);
                }


                model.SortNumber = item.SortNumber;
                model.UpdateTime = DateTime.Now;
                sql = QiyeGoodsCategoryBLL.SingleModel.BuildUpdateSql(model, "SortNumber,UpdateTime");
                tranModel.Add(sql);

            }

            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (QiyeGoodsCategoryBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
                else
                {

                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            else
            {
                returnObj.Msg = "没有需要更新的数据";
                return Json(returnObj);

            }


        }
        #endregion

        #region 店铺产品标签
        public ActionResult ProductLabelMgr(int aid, int pageIndex = 1, int pageSize = 100)
        {

            if (aid <= 0)
                return View("PageError", new QiyeReturnMsg() { Msg = "参数错误!", code = "500" });

            //if (dzaccount == null)
            //    return Redirect("/dzhome/login");
            //XcxAppAccountRelation app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            //if (app == null)
            //    return View("PageError", new QiyeReturn_Msg() { Msg = "小程序未授权!", code = "403" });
            //XcxTemplate xcxTemplate = _xcxtemplateBll.GetModel($"id={app.TId}");
            //if (xcxTemplate == null)
            //    return View("PageError", new QiyeReturn_Msg() { Msg = "小程序模板不存在!", code = "500" });



            ViewModel<QiyeGoodsLabel> vm = new ViewModel<QiyeGoodsLabel>();
            int count = 0;
            vm.DataList = QiyeGoodsLabelBLL.SingleModel.GetListByCach(aid, pageSize, pageIndex, ref count);
            vm.TotalCount = count;
            return View(vm);
        }

        [HttpPost]
        public ActionResult SaveProductLabel(int aid, QiyeGoodsLabel model)
        {
            QiyeReturnMsg returnObj = new QiyeReturnMsg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            //if (dzaccount == null)
            //    return Json(new { isok = false, msg = "登录信息过期请重新登录" });

            //XcxAppAccountRelation app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            //if (app == null)
            //    return Json(new { isok = false, msg = "小程序未授权" });

            //XcxTemplate xcxTemplate = _xcxtemplateBll.GetModel($"id={app.TId}");
            //if (xcxTemplate == null)
            //    return Json(new { isok = false, msg = "小程序模板不存在" });






            //清除缓存
            QiyeGoodsLabelBLL.SingleModel.RemoveQiyeProductLabelListCache(aid);

            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);


            #region 添加和修改
            if (model == null || model.Id < 0)
            {
                returnObj.Msg = "非法请求";
                return Json(returnObj);
            }
            if (model.Name.Trim() == "" || model.Name.Trim().Length > 10)
            {
                returnObj.Msg = "分类名称不能为空，且不能超过10个字";
                return Json(returnObj);

            }
            QiyeGoodsLabel QiyeProductLabel = QiyeGoodsLabelBLL.SingleModel.GetLabelByName(aid, model.Name);
            //修改
            if (model.Id > 0)
            {

                if (QiyeProductLabel != null && QiyeProductLabel.Id != model.Id)
                {
                    returnObj.Msg = "已存在该分类名称，请重新设置！";
                    return Json(returnObj);

                }

                if (QiyeGoodsLabelBLL.SingleModel.Update(model))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "更新成功";
                    return Json(returnObj);
                }
            }
            //添加
            else
            {
                if (QiyeProductLabel != null)
                {
                    returnObj.Msg = "已存在该分类名称，请重新设置！";
                    return Json(returnObj);

                }


                //不能超过100个
                int checkcount = QiyeGoodsLabelBLL.SingleModel.GetProductLabelCount(aid);
                if (checkcount >= 100)
                {
                    returnObj.Msg = "无法新增标签！您已添加了100个标签分类，已达到上限，请编辑已有的标签或删除部分标签后再进行新增。";
                    return Json(returnObj);

                }
                int newid = Convert.ToInt32(QiyeGoodsLabelBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.Id = newid;
                    returnObj.isok = true;
                    returnObj.dataObj = model;
                    returnObj.Msg = "新增成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "操作失败！";
                    return Json(returnObj);
                }
            }
            #endregion

            return Json(new { isok = false, msg = "操作异常！" });
        }


        /// <summary>
        /// 批量更新排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>

        public ActionResult SaveProductLabelSort(List<QiyeGoodsLabel> list, int aid = 0, int actionType = 0)
        {
            QiyeReturnMsg returnObj = new QiyeReturnMsg();

            if (list == null || list.Count <= 0)
            {
                returnObj.Msg = "数据不能为空";
                return Json(returnObj);

            }
            QiyeGoodsLabel model = new QiyeGoodsLabel();
            TransactionModel tranModel = new TransactionModel();
            string sql = string.Empty;
            string labelIds = string.Join(",",list.Select(s=>s.Id));
            List<QiyeGoodsLabel> qiyeGoodsLabelList = QiyeGoodsLabelBLL.SingleModel.GetListByIds(labelIds);
            foreach (QiyeGoodsLabel item in list)
            {
                model = qiyeGoodsLabelList?.FirstOrDefault(f=>f.Id == item.Id);
                if (model == null)
                {
                    returnObj.Msg = $"Id={item.Id}不存在数据库里";
                    return Json(returnObj);
                }

                if (model.AId != item.AId)
                {
                    returnObj.Msg = $"Id={item.Id}权限不足";
                    return Json(returnObj);
                }
                string filed = "Sort";

                if (actionType > 0)
                {
                    //int checkcount = _qiye.GetCount($"FIND_IN_SET({model.Id},plabels)>0 and aid={aid} and state=1");
                    //if (checkcount > 0)
                    //{
                    //    returnObj.Msg = $"该标签下已有{checkcount}个产品，不可删除！";
                    //    return Json(returnObj);
                    //}

                    //表示删除
                    filed = "State";
                    model.State = 0;
                }
                else
                {
                    //表示排序
                    model.Sort = item.Editsort;
                }


                sql = QiyeGoodsLabelBLL.SingleModel.BuildUpdateSql(model, filed);
                tranModel.Add(sql);

            }

            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (QiyeGoodsLabelBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    QiyeGoodsLabelBLL.SingleModel.RemoveQiyeProductLabelListCache(aid);
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
                else
                {

                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            else
            {
                returnObj.Msg = "没有需要操作的数据";
                return Json(returnObj);

            }


        }

        #endregion

        #region 产品规格管理

        /// <summary>
        /// 规格列表
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult ProductSKUMgr(int aid, int pageIndex = 1, int pageSize = 20)
        {


            if (aid <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            //if (dzaccount == null)
            //    return Redirect("/dzhome/login");
            //XcxAppAccountRelation app = _xcxappaccountrelationBll.GetModelByaccountidAndaid(aid, dzaccount.Id.ToString());

            //if (app == null)
            //    return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            //XcxTemplate xcxTemplate = _xcxtemplateBll.GetModel($"id={app.TId}");
            //if (xcxTemplate == null)
            //    return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });







            int fid = Utility.IO.Context.GetRequestInt("fid", 0);

            ViewModel<QiyeSpecification> vm = new ViewModel<QiyeSpecification>();
            string strwhere = $"aid={aid} and state=1 and parentid={fid}";
            vm.DataList = QiyeSpecificationBLL.SingleModel.GetList(strwhere, 100, 1);
            return View(vm);
        }
        /// <summary>
        /// 编辑规格
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveProductSKU(int aid, QiyeSpecification model)
        {
            QiyeReturnMsg returnObj = new QiyeReturnMsg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            //if (dzaccount == null)
            //    return Json(new { isok = false, msg = "登录信息过期请重新登录" });

            //XcxAppAccountRelation app = _xcxappaccountrelationBll.GetModelByaccountidAndaid(aid, dzaccount.Id.ToString());
            //if (app == null)
            //    return Json(new { isok = false, msg = "小程序未授权" });

            //XcxTemplate xcxTemplate = _xcxtemplateBll.GetModel($"id={app.TId}");
            //if (xcxTemplate == null)
            //    return Json(new { isok = false, msg = "小程序模板不存在" });


            string act = Utility.IO.Context.GetRequest("act", string.Empty);
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            #region 删除
            if (act == "del")
            {
                if (id <= 0)
                {
                    returnObj.Msg = "非法请求";
                    return Json(returnObj);

                }
                model = QiyeSpecificationBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    returnObj.Msg = "该规格不存在";
                    return Json(returnObj);

                }
                // TODO 检查是否有已经有产品使用了规格或规格值
                int checkcount = 0;
                //如果删除规格
                //if (model.ParentId == 0)
                //{
                //    checkcount = _qiyeGoodsBLL.GetCount($"FIND_IN_SET({id},SpecName)>0 and aid={aid} and state=1");
                //}
                ////如果删除规格值
                //else
                //{
                //    checkcount = _qiyeGoodsBLL.GetCount($"FIND_IN_SET({id},SpecValue)>0 and aid={aid} and state=1");
                //}

                if (checkcount > 0)
                {
                    returnObj.Msg = $"该规格下已有{checkcount}个产品，不可删除！";
                    return Json(returnObj);
                }


                model.State = 0;
                if (QiyeSpecificationBLL.SingleModel.Update(model, "State"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            #endregion

            #region 添加和修改
            if (model == null || model.Id < 0)
            {
                returnObj.Msg = "非法请求";
                return Json(returnObj);

            }
            if (model.Name.Trim() == "" || model.Name.Trim().Length > 20)
            {
                returnObj.Msg = "规格名称不能为空，且不能超过20个字";
                return Json(returnObj);

            }
            QiyeSpecification QiyeProductSKU = QiyeSpecificationBLL.SingleModel.GetSKUByName(aid, model.Name);


            //修改
            if (model.Id > 0)
            {
                if (QiyeProductSKU != null && QiyeProductSKU.Id != model.Id)
                {
                    returnObj.Msg = "已存在该规格名称，请重新设置！";
                    return Json(returnObj);
                }
                if (QiyeSpecificationBLL.SingleModel.Update(model))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
            }
            //添加
            else
            {

                int checkcount = QiyeSpecificationBLL.SingleModel.GetCountQiyeProductSKU(aid, model.ParentId);
                if (checkcount >= 200)
                {
                    return Json(new { isok = false, msg = "无法新增规格！您已添加了200个产品规格，已达到上限，请编辑已有的分类或删除部分规格后再进行新增。" });
                }
                int newid = Convert.ToInt32(QiyeSpecificationBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.Id = newid;
                    returnObj.dataObj = model;
                    returnObj.isok = true;
                    returnObj.Msg = "添加成功";
                    return Json(returnObj);

                }
            }
            #endregion
            returnObj.Msg = "操作异常";
            return Json(returnObj);
        }

        #endregion

        #region 产品单位管理
        [HttpGet]
        public ActionResult Punit(int aid, int pageIndex = 1, int pageSize = 100)
        {


            string strwhere = $"aid={aid} and state=1";
            ViewModel<QiyeGoodsUnit> vm = new ViewModel<QiyeGoodsUnit>();

            vm.DataList = QiyeGoodsUnitBLL.SingleModel.GetList(strwhere, pageSize, pageIndex, "*", "sort desc,id asc");
            vm.TotalCount = QiyeGoodsUnitBLL.SingleModel.GetCount(strwhere);

            return View(vm);
        }
        [HttpPost]
        public ActionResult Punit(int aid, QiyeGoodsUnit model)
        {
            QiyeReturnMsg returnObj = new QiyeReturnMsg();
            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            #region 删除
            if (act == "del")
            {
                if (id <= 0)
                {
                    returnObj.Msg = "请选择需要删除的单位";
                    return Json(returnObj);
                }

                model = QiyeGoodsUnitBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    returnObj.Msg = "数据不存在";
                    return Json(returnObj);

                }
                model.State = 0;
                if (QiyeGoodsUnitBLL.SingleModel.Update(model, "State"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "删除成功";
                    return Json(returnObj);

                }
                else
                {
                    returnObj.Msg = "删除失败";
                    return Json(returnObj);
                }
            }
            #endregion

            #region 添加和修改

            if (model.Name.Trim() == "" || model.Name.Trim().Length > 10)
            {
                returnObj.Msg = "单位名称不能为空，且不能超过10个字";
                return Json(returnObj);

            }
            else
            {
                int checkcount = QiyeGoodsUnitBLL.SingleModel.GetCount($"name=@name and aid={aid} and id not in(0,{model.Id}) and state=1", new MySqlParameter[] { new MySqlParameter("name", model.Name) });
                if (checkcount > 0)
                {
                    returnObj.Msg = "已存在该单位名称，请重新设置！";
                    return Json(returnObj);

                }
            }
            //修改
            if (model.Id > 0)
            {
                if (QiyeGoodsUnitBLL.SingleModel.Update(model))
                {

                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);

                }
            }
            //添加
            else
            {

                int newid = Convert.ToInt32(QiyeGoodsUnitBLL.SingleModel.Add(model));
                if (newid > 0)
                {
                    model.Id = newid;
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    returnObj.dataObj = model;
                    return Json(returnObj);
                }
            }
            #endregion

            returnObj.Msg = "操作异常";
            return Json(returnObj);
        }
        #endregion

        #region 产品管理

        public ActionResult ProductMgr(int aid, int pageIndex = 1, int pageSize = 20)
        {
            ViewModel<QiyeGoods> vm = new ViewModel<QiyeGoods>();
            try
            {

                string search = Context.GetRequest("search", "");
                int plabels = Context.GetRequestInt("plabels", 0);
                int ptype = Context.GetRequestInt("ptype", 0);
                int ptag = Context.GetRequestInt("ptag", -1);


                int count = 0;
                vm.DataList = QiyeGoodsBLL.SingleModel.GetListByRedis(aid, ref count, search, plabels, ptype, ptag, pageIndex, pageSize);
                vm.TotalCount = count;
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }

            return View(vm);
        }



        public ActionResult ProductEdit(int aid, int id = 0)
        {
            if (aid <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            QiyeGoodsCategoryConfig qiyeGoodsCategoryConfig = QiyeGoodsCategoryConfigBLL.SingleModel.GetModelByAid(aid);
            QiyeSwitchModel switchModel = new QiyeSwitchModel();
            if (qiyeGoodsCategoryConfig == null)
            {

                qiyeGoodsCategoryConfig = new QiyeGoodsCategoryConfig() { Aid = aid, AddTime = DateTime.Now, UpdateTime = DateTime.Now };
                qiyeGoodsCategoryConfig.SwitchConfig = JsonConvert.SerializeObject(switchModel);
                if (Convert.ToInt32(QiyeGoodsCategoryConfigBLL.SingleModel.Add(qiyeGoodsCategoryConfig)) <= 0)
                {
                    return View("PageError", new QiyeReturnMsg() { Msg = "初始化失败!", code = "500" });
                }

            }


            ViewBag.ProductCategoryLevel = 1;
            if (!string.IsNullOrEmpty(qiyeGoodsCategoryConfig.SwitchConfig))
            {
                switchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<QiyeSwitchModel>(qiyeGoodsCategoryConfig.SwitchConfig);
                ViewBag.ProductCategoryLevel = switchModel.ProductCategoryLevel;
            }


            QiyeGoods QiyeProduct = QiyeGoodsBLL.SingleModel.GetModel(id);
            if (QiyeProduct == null)
            {
                QiyeProduct = new QiyeGoods();
                QiyeProduct.AId = aid;
            }

            return View(QiyeProduct);



        }

        [HttpPost, ValidateInput(false)]
        public ActionResult SaveProduct(QiyeGoods QiyeProduct, int aid = 0)
        {
            QiyeReturnMsg returnObj = new QiyeReturnMsg();
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }


            //XcxAppAccountRelation agentinfoXcxAppAccountRelation = _xcxappaccountrelationBll.GetModelById(platStore.BindPlatAid);
            //if (agentinfoXcxAppAccountRelation == null)
            //{
            //    returnObj.Msg = "产品所属店铺的平台没有进行小程序授权";
            //    return Json(returnObj, JsonRequestBehavior.AllowGet);
            //}


            QiyeGoodsBLL.SingleModel.RemoveEntGoodListCache(aid);


            string act = Context.GetRequest("act", string.Empty);
            int id = Context.GetRequestInt("id", 0);
            QiyeGoods QiyeGoods = new QiyeGoods();


            //查找店铺所属平台
            //   TransactionModel tramModelGoodsRelation = new TransactionModel();



            if (act == "copy")
            {
                if (id <= 0)
                {
                    returnObj.Msg = "请选择需要复制的产品";
                    return Json(returnObj);
                }
                QiyeGoods = QiyeGoodsBLL.SingleModel.GetModel(id);
                if (QiyeGoods == null || QiyeGoods.AId != aid)
                {
                    returnObj.Msg = "复制的产品不存在";
                    return Json(returnObj);
                }
                QiyeGoods.Name += "-复制";
                QiyeGoods.Addtime = DateTime.Now;
                QiyeGoods.Id = 0;


                id = Convert.ToInt32(QiyeGoodsBLL.SingleModel.Add(QiyeGoods));
                if (id > 0)
                {

                    returnObj.isok = true;
                    returnObj.Msg = "复制成功";
                    return Json(returnObj);

                }
                else
                {
                    returnObj.Msg = "复制失败";
                    return Json(returnObj);
                }
            }
            else if (act == "tag")
            {
                //单个产品上架或者下架
                //上架或者下架
                if (id <= 0)
                {
                    returnObj.Msg = "请选择需要操作的产品";
                    return Json(returnObj);
                }

                int tag = Context.GetRequestInt("tag", 0);
                QiyeGoods = QiyeGoodsBLL.SingleModel.GetModel(id);
                if (QiyeGoods == null || QiyeGoods.AId != aid)
                {
                    returnObj.Msg = "产品不存在";
                    return Json(returnObj);
                }

                QiyeGoods.Tag = tag;
                QiyeGoods.Updatetime = DateTime.Now;
                if (QiyeGoodsBLL.SingleModel.Update(QiyeProduct, "Tag,Updatetime"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = $"操作成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "操作失败";
                    return Json(returnObj);

                }
            }
            else if (act == "batch")
            {
                //批量操作 上架或者下架 删除 

                int actval = Utility.IO.Context.GetRequestInt("actval", -1);
                string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
                if (string.IsNullOrEmpty(ids))
                {
                    returnObj.Msg = "请先选择产品";
                    return Json(returnObj);
                }

                List<QiyeGoods> goods = QiyeGoodsBLL.SingleModel.GetListByIds(ids);
                if (goods == null || !goods.Any())
                {
                    returnObj.Msg = "选择的产品不存在";
                    return Json(returnObj);
                }

                TransactionModel transactionModel = new TransactionModel();
                foreach (QiyeGoods item in goods)
                {
                    if (actval != -1)
                    {
                        //表示批量上架或者下架
                        item.Updatetime = DateTime.Now;
                        item.Tag = actval;
                        transactionModel.Add(QiyeGoodsBLL.SingleModel.BuildUpdateSql(item, "Tag,Updatetime"));
                        // TODO 更新购物车
                    }
                    else
                    {
                        //表示批量删除
                        if (item.Tag == 0)
                        {
                            item.Updatetime = DateTime.Now;
                            item.State = 0;
                            transactionModel.Add(QiyeGoodsBLL.SingleModel.BuildUpdateSql(item, "State,Updatetime"));
                            // TODO 更新购物车
                        }
                        else
                        {

                            returnObj.Msg = "操作异常,请先下架商品才能删除";
                            return Json(returnObj);
                        }

                    }
                }

                if (transactionModel.sqlArray != null && transactionModel.sqlArray.Length > 0 && QiyeGoodsBLL.SingleModel.ExecuteTransactionDataCorect(transactionModel.sqlArray))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "操作失败";
                    return Json(returnObj);
                }







            }
            else if (act == "del")
            {
                QiyeGoods model = QiyeGoodsBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    returnObj.Msg = "请选择需要删除的产品";
                    return Json(returnObj);
                }
                if (model.Tag == 1)
                {
                    returnObj.Msg = "请先下架该产品才能删除";
                    return Json(returnObj);
                }
                model.Updatetime = DateTime.Now;
                model.State = 0;
                if (QiyeGoodsBLL.SingleModel.Update(model, "State,Updatetime"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "删除成功";
                    return Json(returnObj);

                }
                returnObj.Msg = "删除失败";
                return Json(returnObj);
            }
            else
            {
                //新增或者更新
                if (QiyeProduct.Id <= 0)
                {
                    //表示新增
                    QiyeProduct.Addtime = DateTime.Now;
                    id = Convert.ToInt32(QiyeGoodsBLL.SingleModel.Add(QiyeProduct));
                    if (id > 0)
                    {
                        returnObj.isok = true;
                        returnObj.Msg = "新增成功";
                        return Json(returnObj);


                    }
                    else
                    {
                        returnObj.Msg = "新增失败";
                        return Json(returnObj);
                    }
                }
                else
                {
                    QiyeGoods = QiyeGoodsBLL.SingleModel.GetModel(QiyeProduct.Id);
                    if (QiyeGoods == null || QiyeGoods.AId != aid)
                    {
                        returnObj.Msg = "数据不存在";
                        return Json(returnObj);
                    }
                    QiyeProduct.Updatetime = DateTime.Now;
                    if (QiyeGoodsBLL.SingleModel.Update(QiyeProduct))
                    {
                        returnObj.isok = true;
                        returnObj.Msg = "更新成功";
                        return Json(returnObj);
                    }
                    else
                    {
                        returnObj.Msg = "更新失败";
                        return Json(returnObj);

                    }

                }
            }




        }


        /// <summary>
        /// 产品排序
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateProductSort()
        {
            int appId = Context.GetRequestInt("appId", 0);
            string datajson = Context.GetRequest("datajson", string.Empty);

            if (string.IsNullOrEmpty(datajson))
            {
                return Json(new { isok = false, msg = "参数错误！" });
            }

            if (QiyeGoodsBLL.SingleModel.UpdateProductSort(appId, datajson))
            {
                return Json(new { isok = true, msg = "操作成功！" });
            }

            return Json(new { isok = false, msg = "操作失败！" });
        }
        #endregion

        /// <summary>
        /// 店铺配置
        /// </summary>
        /// <returns></returns>
        public ActionResult Store(int aid = 0)
        {

            if (aid <= 0)
                return View("PageError", new QiyeReturnMsg() { Msg = "参数错误!", code = "500" });
            QiyeStore qiyeStore = QiyeStoreBLL.SingleModel.GetModelByAId(aid);
            if (qiyeStore == null)
            {
                qiyeStore = new QiyeStore();
                qiyeStore.Aid = aid;
                qiyeStore.AddTime = DateTime.Now;
                qiyeStore.UpdateTime = DateTime.Now;
                qiyeStore.SwitchModel = new QiyeStoreSwitchModel();
                qiyeStore.SwitchConfig = JsonConvert.SerializeObject(qiyeStore.SwitchModel);
            }

            if (!string.IsNullOrEmpty(qiyeStore.SwitchConfig))
            {
                qiyeStore.SwitchModel = JsonConvert.DeserializeObject<QiyeStoreSwitchModel>(qiyeStore.SwitchConfig);
            }

            ViewBag.appId = aid;

            return View(qiyeStore);
        }

        /// <summary>
        /// 店铺配置保存
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveStore(QiyeStore qiyeStore)
        {
            QiyeReturnMsg returnObj = new QiyeReturnMsg();
            if (qiyeStore == null || qiyeStore.SwitchModel == null)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            qiyeStore.UpdateTime = DateTime.Now;
            qiyeStore.SwitchConfig = JsonConvert.SerializeObject(qiyeStore.SwitchModel);
            if (qiyeStore.Id > 0)
            {
                //表示更新
                if (QiyeStoreBLL.SingleModel.Update(qiyeStore))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "修改成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "修改失败";
                    return Json(returnObj);
                }
            }
            else
            {
                qiyeStore.AddTime = DateTime.Now;
                int id = Convert.ToInt32(QiyeStoreBLL.SingleModel.Add(qiyeStore));
                if (id > 0)
                {
                    returnObj.isok = true;
                    returnObj.Msg = "新增成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "新增失败";
                    return Json(returnObj);
                }
            }




        }

        #region 业绩管理
        public ActionResult YeJi(int aId, int pageIndex = 1, int pageSize = 20, string act = "", string kw = "", int id = 0, int did = 0, int typeid = 0)
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<Entity.MiniApp.Qiye.QiyeYeJi> vm = new ViewModel<Entity.MiniApp.Qiye.QiyeYeJi>();
                var tupleResult = QiyeYeJiBLL.SingleModel.GetListFromTable(aId, did, pageIndex, pageSize, kw, typeid);
                vm.DataList = tupleResult.Item1;
                vm.TotalCount = tupleResult.Item2;
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.aId = aId;
                ViewBag.DepartmentList = QiyeDepartmentBLL.SingleModel.GetList($"aid={aId} and state=0");
                return View(vm);
            }
            return Json(result);
        }

        [HttpGet]
        public void YeJiExport(int aId, int pageIndex = 1, string act = "", string kw = "", int id = 0, int did = 0, int typeid = 0)
        {
            if (act == "export")
            {
                string filename = "业绩导出" + "-" + DateTime.Now.ToString("yyyy-MM-dd");
                DataTable exportTable = QiyeYeJiBLL.SingleModel.GetDataTable(aId, did, pageIndex, -1, kw, typeid);
                if (exportTable == null || exportTable.Rows.Count <= 0)
                {
                    Response.Write("没有数据");
                    Response.End();
                    return;
                }
                DataTable formatTable = new DataTable();
                formatTable.Columns.AddRange(new DataColumn[] {
                    new DataColumn("姓名"),
                    new DataColumn("电话"),
                    new DataColumn("部门"),
                    new DataColumn("名片访问"),
                    new DataColumn("名片转发"),
                    new DataColumn("客户总数"),
                    new DataColumn("客户点赞"),
                    new DataColumn("咨询次数"),
                    new DataColumn("订单量"),
                    new DataColumn("销售金额"),
                    new DataColumn("销售排名")
                });
                int rank = 1;
                foreach (DataRow item in exportTable.Rows)
                {
                    DataRow dr = formatTable.NewRow();
                    dr["姓名"] = item["UserName"].ToString();
                    dr["电话"] = item["Phone"].ToString();
                    dr["部门"] = item["DepartmentName"].ToString();
                    dr["名片访问"] = item["CardViewedCount"].ToString();
                    dr["名片转发"] = item["CardRepostCount"].ToString();
                    dr["客户总数"] = item["CustomerCount"].ToString();
                    dr["客户点赞"] = item["CustomerLikeCount"].ToString();
                    dr["咨询次数"] = item["CustomerConsultCount"].ToString();
                    dr["订单量"] = item["OrderCount"].ToString();
                    dr["销售金额"] = item["Sales"].ToString();
                    dr["销售排名"] = rank;
                    formatTable.Rows.Add(dr);
                    rank++;
                }
                ExcelHelper<QiyeYeJi>.Out2Excel(formatTable, filename);//导出
            }
            Response.Write("无效参数");
            Response.End();
            return;
        }
        #endregion
    }
}