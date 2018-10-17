using BLL.MiniApp;
using BLL.MiniApp.Plat;
using Entity.MiniApp;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Plat.Filters;

namespace User.MiniApp.Areas.Plat.Controllers
{
    /// <summary>
    /// 信息分类里的 信息相关管理
    /// </summary>
    [LoginFilter]
    [MiniApp.Filters.RouteAuthCheck]
    public class MsgController : User.MiniApp.Controllers.baseController
    {
        
        
        
        
        
        

        private PlatReturnMsg result;

        public MsgController()
        {

            
            
            
            
            
        }
        
        #region 信息分类 类别管理
        /// <summary>
        /// 信息类别列表
        /// </summary>
        /// <returns></returns>

        public ActionResult TypeList(int aid = 0)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.PageType = pageType;
            ViewBag.appId = aid;
            return View();
        }

        /// <summary>
        /// 获取信息分类
        /// </summary>
        /// <returns></returns>

        public ActionResult GetMsgTypes()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            string msgTypeName = Utility.IO.Context.GetRequest("typeName", string.Empty);
            int totalCount = 0;
            List<PlatMsgType> list = PlatMsgTypeBLL.SingleModel.getListByaid(appId, out totalCount, pageSize, pageIndex, msgTypeName);

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 类别名称是否存在
        /// </summary>
        /// <returns></returns>

        public ActionResult CheckMsgTypeName()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            string msgTypeName = Utility.IO.Context.GetRequest("msgTypeName", string.Empty);
            if (appId <= 0 || string.IsNullOrEmpty(msgTypeName))
            {
                result.Msg = "参数错误";
                return Json(result);
            }

            PlatMsgType model = PlatMsgTypeBLL.SingleModel.msgTypeNameIsExist(appId, msgTypeName);
            if (model != null && model.Id != Id)
            {
                result.Msg = "类别名称已存在";
                return Json(result);
            }
            result.isok = true;
            result.Msg = "ok";
            return Json(result);
        }


        /// <summary>
        /// 编辑或者新增 信息分类
        /// </summary>
        /// <param name="city_Storemsgrules"></param>
        /// <returns></returns>

        public ActionResult SaveMsgType(PlatMsgType platMsgType)
        {

            result = new PlatReturnMsg();
            if (platMsgType == null)
            {
                result.Msg = "数据不能为空";
                return Json(result);
            }

            if (platMsgType.Aid <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }



            int Id = platMsgType.Id;
            if (Id == 0)
            {
                //表示新增
                Id = Convert.ToInt32(PlatMsgTypeBLL.SingleModel.Add(new PlatMsgType()
                {
                    MaterialPath = platMsgType.MaterialPath,
                    Name = platMsgType.Name,
                    AddTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    Aid = platMsgType.Aid,
                    State = 0,
                    SortNumber = platMsgType.SortNumber

                }));
                if (Id > 0)
                {
                    result.isok = true;
                    result.Msg = "新增成功";
                    return Json(result);

                }
                else
                {
                    result.Msg = "新增失败";
                    return Json(result);
                }

            }
            else
            {
                //表示更新
                PlatMsgType model = PlatMsgTypeBLL.SingleModel.GetModel(Id);
                if (model == null)
                {
                    result.Msg = "不存在数据库里";
                    return Json(result);
                }

                if (model.Aid != platMsgType.Aid)
                {
                    result.Msg = "权限不足";
                    return Json(result);
                }
                model.UpdateTime = DateTime.Now;
                model.MaterialPath = platMsgType.MaterialPath;
                model.Name = platMsgType.Name;
                model.SortNumber = platMsgType.SortNumber;
                if (PlatMsgTypeBLL.SingleModel.Update(model, "updateTime,materialPath,name,sortNumber"))
                {
                    result.isok = true;
                    result.Msg = "更新成功";
                    return Json(result);

                }
                else
                {
                    result.Msg = "更新失败";
                    return Json(result);

                }

            }

        }


        /// <summary>
        /// 批量更新排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>

        public ActionResult SaveMsgTypeSort(List<PlatMsgType> list)
        {
            result = new PlatReturnMsg();

            if (list == null || list.Count <= 0)
            {
                result.Msg = "数据不能为空";
                return Json(result);

            }
            PlatMsgType model = new PlatMsgType();
            TransactionModel tranModel = new TransactionModel();
            string sql = string.Empty;
            string msgTypeIds = string.Join(",",list.Select(s=>s.Id));
            List<PlatMsgType> platMsgTypeList = PlatMsgTypeBLL.SingleModel.GetListByIds(msgTypeIds);
            foreach (PlatMsgType item in list)
            {
                model = platMsgTypeList?.FirstOrDefault(f=>f.Id == item.Id);
                if (model == null)
                {
                    result.Msg = $"Id={item.Id}不存在数据库里";
                    return Json(result);
                }

                if (model.Aid != item.Aid)
                {
                    result.Msg = $"Id={item.Id}权限不足";
                    return Json(result);
                }


                model.SortNumber = item.SortNumber;
                model.UpdateTime = DateTime.Now;
                sql = PlatMsgTypeBLL.SingleModel.BuildUpdateSql(model, "SortNumber,UpdateTime");
                tranModel.Add(sql);

            }

            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatMsgTypeBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    result.isok = true;
                    result.Msg = "操作成功";
                    return Json(result);

                }
                else
                {

                    result.Msg = "操作失败";
                    return Json(result);

                }
            }
            else
            {
                result.Msg = "没有需要更新的数据";
                return Json(result);

            }


        }

        /// <summary>
        /// 删除信息分类包括单个批量
        /// </summary>
        /// <returns></returns>

        public ActionResult DelMsgTypes()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }

            if (!Utility.StringHelper.IsNumByStrs(',', ids))
            {
                result.Msg = "非法操作";
                return Json(result);
            }
            //判断是否有权限
            List<PlatMsgType> list = PlatMsgTypeBLL.SingleModel.GetListByIds(appId, ids);
            TransactionModel tranModel = new TransactionModel();
            foreach (PlatMsgType item in list)
            {
                if (appId != item.Aid)
                {
                    result.Msg = $"非法操作(无权限对id={item.Id}的类别)";
                    return Json(result);
                }

                if (PlatMsgBLL.SingleModel.MsgTypeHaveMsg(appId, item.Id))
                {
                    result.Msg = $"{item.Name}类别下关联了帖子,请先删除帖子再删除该类别";
                    return Json(result);
                }

                item.State = -1;
                item.UpdateTime = DateTime.Now;
                tranModel.Add(PlatMsgTypeBLL.SingleModel.BuildUpdateSql(item, "State,updateTime"));

            }


            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatMsgTypeBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    result.isok = true;
                    result.Msg = "操作成功";
                    return Json(result);

                }
                else
                {
                    result.Msg = "操作失败";
                    return Json(result);

                }
            }
            else
            {
                result.Msg = "没有需要删除的数据";
                return Json(result);

            }
        }

        #endregion

        #region 审核配置

        /// <summary>
        /// 信息审核配置
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>

        public ActionResult Conf(int aid = 0)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.PageType = pageType;

            PlatMsgConf model = PlatMsgConfBLL.SingleModel.GetMsgConf(aid);
            int id = 0;
            if (model == null)
            {
                model = new PlatMsgConf()
                {
                    Aid = aid,
                    ReviewSetting = 0,
                    UpdateTime = DateTime.Now
                };
                id = Convert.ToInt32(PlatMsgConfBLL.SingleModel.Add(model));

                if (id <= 0)
                {
                    return View("PageError", new PlatReturnMsg() { Msg = "初始化审核配置异常!", code = "500" });
                }
            }
            ViewBag.appId = aid;
            return View(model);
        }


        /// <summary>
        /// 保存审核配置
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="reviewSetting"></param>
        /// <returns></returns>

        public ActionResult SaveConf(int aid = 0, int reviewSetting = 0)
        {
            result = new PlatReturnMsg();
            if (aid <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);

            }

            PlatMsgConf model = PlatMsgConfBLL.SingleModel.GetMsgConf(aid);

            if (model == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }

            model.ReviewSetting = reviewSetting;
            model.UpdateTime = DateTime.Now;
            if (PlatMsgConfBLL.SingleModel.Update(model, "ReviewSetting,UpdateTime"))
            {
                result.isok = true;
                result.Msg = "操作成功";
                return Json(result);
            }
            else
            {
                result.Msg = "操作失败";
                return Json(result);
            }


        }
        #endregion

        #region 置顶规则配置

        /// <summary>
        /// 获取置顶规则列表
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>

        public ActionResult GetMsgRules(int aid = 0)
        {
            result = new PlatReturnMsg();


            if (aid <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int totalCount = 0;
            List<PlatMsgRule> list = PlatMsgRuleBLL.SingleModel.GetListByaid(aid, out totalCount, pageSize, pageIndex);

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 编辑或者新增 置顶规则
        /// </summary>
        /// <param name="city_Storemsgrules"></param>
        /// <returns></returns>

        public ActionResult SaveMsgRule(PlatMsgRule platMsgRule)
        {
            result = new PlatReturnMsg();
            if (platMsgRule == null)
            {
                result.Msg = "数据不能为空";
                return Json(result);
            }


            if (platMsgRule.Aid <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }

            if (!Regex.IsMatch(platMsgRule.ExptimeDay.ToString(), @"^\+?[1-9][0-9]*$"))
            {
                result.Msg = "置顶时间天数必须为大于零的整数";
                return Json(result);
            }

            if (!Regex.IsMatch(platMsgRule.Price.ToString(), @"^\+?[0-9][0-9]*$"))
            {
                result.Msg = "金额不合法";
                return Json(result);
            }

            int Id = platMsgRule.Id;
            if (Id == 0)
            {
                //表示新增
                Id = Convert.ToInt32(PlatMsgRuleBLL.SingleModel.Add(new PlatMsgRule()
                {
                    ExptimeDay = platMsgRule.ExptimeDay,
                    Price = platMsgRule.Price,
                    AddTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    Aid = platMsgRule.Aid,
                    State = 0

                }));
                if (Id > 0)
                {
                    result.isok = true;
                    result.Msg = "新增成功";
                    return Json(result);

                }
                else
                {
                    result.Msg = "新增失败";
                    return Json(result);

                }

            }
            else
            {
                //表示更新
                PlatMsgRule model = PlatMsgRuleBLL.SingleModel.GetModel(Id);
                if (model == null)
                {
                    result.Msg = "不存在数据库里";
                    return Json(result);
                }

                if (model.Aid != platMsgRule.Aid)
                {
                    result.Msg = "权限不足";
                    return Json(result);
                }

                model.UpdateTime = DateTime.Now;
                model.ExptimeDay = platMsgRule.ExptimeDay;
                model.Price = platMsgRule.Price;

                if (PlatMsgRuleBLL.SingleModel.Update(model, "UpdateTime,ExptimeDay,Price"))
                {
                    result.isok = true;
                    result.Msg = "更新成功";
                    return Json(result);

                }
                else
                {
                    result.Msg = "更新失败";
                    return Json(result);

                }

            }

        }

        /// <summary>
        /// 删除规则
        /// </summary>
        /// <returns></returns>

        public ActionResult DelMsgRules()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int ruleId = Utility.IO.Context.GetRequestInt("ruleId", 0);
            if (appId <= 0 || ruleId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }

            PlatMsgRule model = PlatMsgRuleBLL.SingleModel.GetMsgRules(appId, ruleId);
            if (model == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }


            model.State = -1;
            model.UpdateTime = DateTime.Now;
            if (!PlatMsgRuleBLL.SingleModel.Update(model, "State,UpdateTime"))
            {
                result.Msg = "删除异常";
                return Json(result);
            }

            result.isok = true;
            result.Msg = "删除成功";
            return Json(result);

        }

        #endregion
        
        #region 信息管理
        public ActionResult Index(int aid, int tab = 0)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.PageType = pageType;

            ViewBag.appId = aid;
            ViewBag.tab = tab;
            return View();
        }

        /// <summary>
        /// 获取信息列表
        /// </summary>
        /// <returns></returns>

        public ActionResult GetMsg()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            string msgTypeName = Utility.IO.Context.GetRequest("msgTypeName", string.Empty);
            string userName = Utility.IO.Context.GetRequest("userName", string.Empty);
            string userPhone = Utility.IO.Context.GetRequest("userPhone", string.Empty);
            int isTop = Utility.IO.Context.GetRequestInt("isTop", 0);
            int Review = Utility.IO.Context.GetRequestInt("Review", -2);
            int isFromStore = Utility.IO.Context.GetRequestInt("isFromStore", -1);
            int totalCount = 0;
            List<PlatMsg> list = PlatMsgBLL.SingleModel.GetListByaid(appId, out totalCount, isTop, pageSize, pageIndex, msgTypeName, userName, userPhone, "addTime desc", Review, isFromStore);

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 删除信息包括单个批量 或者审核通过、不审核通过
        /// </summary>
        /// <returns></returns>

        public ActionResult DelMsg()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            int actionType = Utility.IO.Context.GetRequestInt("actionType", 0);//默认0为删除 其它为审核的状态
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }
            if (!Utility.StringHelper.IsNumByStrs(',', ids))
            {
                result.Msg = "非法操作";
                return Json(result);
            }
            //判断是否有权限
            List<PlatMsg> list = PlatMsgBLL.SingleModel.GetListByIds(appId, ids);
            TransactionModel tranModel = new TransactionModel();


            foreach (PlatMsg item in list)
            {
                if (appId != item.Aid)
                {
                    result.Msg = $"非法操作(无权限对id={item.Id}的信息)";
                    return Json(result);
                }

                if (actionType == 0)
                {
                    item.State = -1;
                }
                else
                {
                    if (item.Review != 1)
                    {
                        result.Msg = "已经审核了不需再审核";
                        return Json(result);
                    }

                    //审核操作
                    if (item.State == 0)
                    {
                        //表示先审核后发布 置顶时间计算起始为审核通过的时间
                        item.ReviewTime = DateTime.Now;
                    }

                    item.State = actionType == 2 ? 1 : 0;
                    item.Review = actionType;
                }


                item.UpdateTime = DateTime.Now;
                tranModel.Add(PlatMsgBLL.SingleModel.BuildUpdateSql(item, "State,UpdateTime,Review,ReviewTime"));

            }


            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatMsgBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    result.isok = true;
                    result.Msg = "操作成功";
                    return Json(result);

                }
                else
                {
                    result.Msg = "操作失败";
                    return Json(result);

                }
            }
            else
            {
                result.Msg = "没有需要操作的数据";
                return Json(result);

            }


        }

        public ActionResult GetMsgDetail()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            if (appId <= 0 || Id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            PlatMsg model = PlatMsgBLL.SingleModel.GetModel(Id);
            if (model == null)
            {
                result.Msg = "数据不存在";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            result.Msg = "获取成功";
            result.isok = true;
            result.dataObj = model;
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 后台手动置顶将未置顶的消息
        /// </summary>
        /// <returns></returns>
        public ActionResult DoTopMsg()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            int ruleId = Utility.IO.Context.GetRequestInt("ruleId", 0);
            if (appId <= 0 || Id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }
            PlatMsg model = PlatMsgBLL.SingleModel.GetModel(Id);
            if (model == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }

           
            if (ruleId <= 0)
            {
                result.Msg = "请选择置顶时间";
                return Json(result);

            }

            PlatMsgRule platMsgRule = PlatMsgRuleBLL.SingleModel.GetMsgRules(appId, ruleId);
            if (platMsgRule == null)
            {
                result.Msg = "非法操作(置顶时间有误)";
                return Json(result);

            }
            //if (model.Review == 2|| model.Review == 0)
            //{
            //    model.ReviewTime = DateTime.Now;
            //}
            model.ReviewTime = DateTime.Now;
            model.TopDay = platMsgRule.ExptimeDay;
            model.TopCostPrice = platMsgRule.Price;
            model.IsTop = 1;
            model.IsDoTop = 1;
            model.UpdateTime = DateTime.Now;
            if (PlatMsgBLL.SingleModel.Update(model, "TopDay,TopCostPrice,IsTop,IsDoTop,UpdateTime,ReviewTime"))
            {
                result.isok = true;
                result.Msg = "操作成功";
                return Json(result);
            }
            else
            {
                result.Msg = "操作失败";
                return Json(result);
            }


        }



        /// <summary>
        /// 后台取消置顶的消息
        /// </summary>
        /// <returns></returns>
        public ActionResult DoNotTopMsg()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            
            if (appId <= 0 || Id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }
            PlatMsg model = PlatMsgBLL.SingleModel.GetModel(Id);
            if (model == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }


            model.TopDay = 0;
           
          //  model.IsTop = 0;
            model.IsDoNotTop = 1;
            model.UpdateTime = DateTime.Now;
            if (PlatMsgBLL.SingleModel.Update(model, "TopDay,IsTop,IsDoNotTop,UpdateTime"))
            {
                result.isok = true;
                result.Msg = "操作成功";
                return Json(result);
            }
            else
            {
                result.Msg = "操作失败";
                return Json(result);
            }


        }



        #endregion

        #region 发帖用户管理

        public ActionResult PostUser(int aid = 0)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.PageType = pageType;
            ViewBag.appId = aid;
            return View();
        }

        /// <summary>
        /// 获取发帖用户列表
        /// </summary>
        /// <returns></returns>

        public ActionResult GetUsers()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            if (appId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);


            string userName = Utility.IO.Context.GetRequest("userName", string.Empty);
            string userPhone = Utility.IO.Context.GetRequest("userPhone", string.Empty);

            string startTime = Utility.IO.Context.GetRequest("startTime", string.Empty);
            string endTime = Utility.IO.Context.GetRequest("endTime", string.Empty);

            int totalCount = 0;
            List<PlatPostUser> list = PlatPostUserBLL.SingleModel.getListByaid(appId, ref totalCount, pageSize, pageIndex, userName, userPhone, startTime, endTime);
            result.Msg = "获取成功";
            result.isok = true;
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);


        }
        #endregion
        
        #region 用户举报信息管理
        public ActionResult ReportMsg(int aid = 0)
        {
            if (aid <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.PageType = pageType;
            ViewBag.appId = aid;
            return View();
        }

        public ActionResult GetReportMsgList()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            if (appId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int totalCount = 0;
            List<PlatMsgReport> list = PlatMsgReportBLL.SingleModel.GetListByaid(appId, out totalCount, pageSize, pageIndex);

            result.Msg = "获取成功";
            result.isok = true;
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 确认或者删除
        /// 确认则删除被举报的帖子
        /// 删除则删除举报记录
        /// </summary>
        /// <returns></returns>

        public ActionResult delOrConfirmReportMsg()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            int actionType = Utility.IO.Context.GetRequestInt("actionType", 0);//行为类别 默认为0删除 1为确认
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            if (appId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }


            if (!Utility.StringHelper.IsNumByStrs(',', ids))
            {
                result.Msg = "非法操作";
                return Json(result);
            }
            //判断是否有权限
            List<PlatMsgReport> list = PlatMsgReportBLL.SingleModel.GetListByIds(appId, ids);
            TransactionModel tranModel = new TransactionModel();
            foreach (PlatMsgReport item in list)
            {
                if (appId != item.Aid)
                {
                    result.Msg = $"非法操作(无权限对id={item.Id}的信息)";
                    return Json(result);
                }

                if (actionType == 0)//删除举报记录
                {
                    item.State = -1;
                }
                else
                {
                    item.ConfirmState = 1;//确认已经处理 后续可能发送模板消息给举报者
                }

                item.UpdateTime = DateTime.Now;
                tranModel.Add(PlatMsgReportBLL.SingleModel.BuildUpdateSql(item, "ConfirmState,State,UpdateTime"));
            }
            
            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatMsgReportBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    result.Msg = "操作成功";
                    result.isok = true;
                    return Json(result);
                }
                else
                {
                    result.Msg = "操作失败";
                    return Json(result);
                }
            }
            else
            {
                result.Msg = "没有合适的数据";
                return Json(result);
            }
        }
        
        public ActionResult MsgCommentMgr()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            if (appId <= 0 || Id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
       

            ViewBag.appId = appId;
            ViewBag.Id = Id;

            return View();

        }


        /// <summary>
        /// 获取指定帖子的评论
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMsgComment()
        {
            result = new PlatReturnMsg();
            result.code = "200";
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);//指定某条帖子的 评论
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);
            string keyMsg = Utility.IO.Context.GetRequest("keyMsg", string.Empty);
            if (appId <= 0 || Id <= 0)
            {

                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (r == null)
            {

                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            int totalCount = 0;
            List<PlatMsgComment> listComment = PlatMsgCommentBLL.SingleModel.GetPlatMsgComment(r.Id, out totalCount,0, keyMsg, pageSize, pageIndex, Id);

            result.dataObj = new { totalCount = totalCount, list = listComment };
            result.isok = true;
            result.Msg = "获取成功";
            return Json(result, JsonRequestBehavior.AllowGet);


        }


        public ActionResult DeleteMsgComment()
        {
            result = new PlatReturnMsg();
            result.code = "200";
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);


            if (appId <= 0 || string.IsNullOrEmpty(ids))
            {

                result.Msg = "参数错误";
                return Json(result);
            }
            XcxAppAccountRelation r = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (r == null)
            {

                result.Msg = "小程序未授权";
                return Json(result);
            }
            if (!Utility.StringHelper.IsNumByStrs(',', ids))
            {
                result.Msg = "非法操作";
                return Json(result);
            }

            List<PlatMsgComment> list = PlatMsgCommentBLL.SingleModel.GetListByIds(ids);
            if (list == null || list.Count <= 0)
            {
                result.Msg = "没有评论需要删除";
                return Json(result);
            }

            TransactionModel tranModel = new TransactionModel();
            foreach (PlatMsgComment item in list)
            {
                if (item == null || item.AId != r.Id)
                {
                    result.Msg = "帖子评论不存在或者没有权限";
                    return Json(result);
                }
                item.State = -1;
                tranModel.Add(PlatMsgCommentBLL.SingleModel.BuildUpdateSql(item, "State"));


            }


            if (PlatMsgCommentBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
            {
                result.isok = true;
                result.Msg = "删除成功";
                return Json(result);
            }
            else
            {
                result.Msg = "删除失败";
                return Json(result);
            }
        }
        #endregion
    }
}