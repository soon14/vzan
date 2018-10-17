using BLL.MiniApp;
using BLL.MiniApp.cityminiapp;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
namespace User.MiniApp.Controllers
{
    public class cityminiappController : baseController
    {
        private Return_Msg result;
        public cityminiappController()
        {
        }

        #region 商家配置
        // GET: cityminiapp
        /// <summary>
        /// 商家配置
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int appId = 0)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });


            CityStoreBanner storebanner = CityStoreBannerBLL.SingleModel.getModelByaid(appId);
            if (storebanner == null)
            {
                //新增一条 初始化

                storebanner = new CityStoreBanner()
                {
                    aid = appId,
                    addTime = DateTime.Now,
                    updateTime = DateTime.Now,
                    ReviewSetting = 0
                };

                int id = Convert.ToInt32(CityStoreBannerBLL.SingleModel.Add(storebanner));
                if (id <= 0)
                    return View("PageError", new Return_Msg() { Msg = "初始化轮播图失败!", code = "500" });

            }
            List<object> listBannersToMsg = new List<object>();
            ViewBag.appId = appId;
            storebanner.MsgCount = CityMsgBLL.SingleModel.GetCountByAId(appId);
            if (!string.IsNullOrEmpty(storebanner.MsgIds) && !string.IsNullOrWhiteSpace(storebanner.MsgIds))
            {
                //获取帖子信息
                List<CityMsg> listCityMsg = CityMsgBLL.SingleModel.GetListByIds(xcx.Id, storebanner.MsgIds);
                foreach(CityMsg item in listCityMsg)
                {
                    listBannersToMsg.Add(new
                    {
                        Id=item.Id,
                        Txt=item.msgDetail.Length>10? item.msgDetail.Substring(0,10)+"...": item.msgDetail
                    });
                }
            }

            ViewBag.listBannersToMsg = listBannersToMsg;

            return View(storebanner);
        }



        /// <summary>
        /// 保存商家配置 轮播图
        /// </summary>
        /// <returns></returns>
        public ActionResult saveBanners()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int RemarkOpenFrm = Utility.IO.Context.GetRequestInt("RemarkOpenFrm", 0);
            int ReviewSetting = Utility.IO.Context.GetRequestInt("ReviewSetting", 0);
            string KeFuPhone = Utility.IO.Context.GetRequest("KeFuPhone", string.Empty);
            string Remark = Utility.IO.Context.GetRequest("Remark", string.Empty);
            int VirtualMsgCount = Utility.IO.Context.GetRequestInt("VirtualMsgCount", 0);
            int VirtualPV = Utility.IO.Context.GetRequestInt("VirtualPV", 0);
            int PV = Utility.IO.Context.GetRequestInt("PV", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }

            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result);
            }


            string imgsStr = Utility.IO.Context.GetRequest("bannerImgs", string.Empty);
            if (string.IsNullOrEmpty(imgsStr))
            {
                result.Msg = "请上传至少一张轮播图";
                return Json(result);
            }


            string[] imgs = imgsStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (imgs == null || imgs.Length == 0)
            {
                result.Msg = "轮播图不能为空";
                return Json(result);
            }
            //校验手机号码  
            if (!Regex.IsMatch(KeFuPhone, @"\d") || KeFuPhone.Length > 11)
            {
                result.Msg = "联系号码错误";
                return Json(result);
            }

            string bannerMsg = Utility.IO.Context.GetRequest("bannerMsg", string.Empty);

            CityStoreBanner storebanner = CityStoreBannerBLL.SingleModel.getModelByaid(appId);
            if (storebanner == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }
            storebanner.RemarkOpenFrm = RemarkOpenFrm;
            storebanner.Remark =HttpUtility.HtmlEncode(Remark);
            storebanner.ReviewSetting = ReviewSetting;
            storebanner.banners = imgsStr;
            storebanner.updateTime = DateTime.Now;
            storebanner.KeFuPhone = KeFuPhone;
            storebanner.VirtualMsgCount = VirtualMsgCount;
            storebanner.VirtualPV = VirtualPV;
            storebanner.PV = PV;
            storebanner.MsgIds = bannerMsg;
            if (!CityStoreBannerBLL.SingleModel.Update(storebanner, "MsgIds,VirtualMsgCount,VirtualPV,PV,RemarkOpenFrm,Remark,banners,ReviewSetting,updateTime,KeFuPhone"))
            {
                result.Msg = "操作失败";
                return Json(result);
            }

            result.isok = true;
            result.Msg = "操作成功";
            return Json(result);


        }

        public ActionResult getMsgRules()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);

            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }


            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int totalCount = 0;
            List<CityStoreMsgRules> list = CityStoreMsgRulesBLL.SingleModel.getListByaid(appId, out totalCount, pageSize, pageIndex);

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
        public ActionResult saveMsgRule(CityStoreMsgRules city_Storemsgrules)
        {
            result = new Return_Msg();
            if (city_Storemsgrules == null)
            {
                result.Msg = "数据不能为空";
                return Json(result);
            }


            if (city_Storemsgrules.aid <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }

            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }

            if (!Regex.IsMatch(city_Storemsgrules.exptimeday.ToString(), @"^\+?[1-9][0-9]*$"))
            {
                result.Msg = "置顶时间天数必须为大于零的整数";
                return Json(result);
            }

            if (!Regex.IsMatch(city_Storemsgrules.price.ToString(), @"^\+?[0-9][0-9]*$"))
            {
                result.Msg = "金额不合法";
                return Json(result);
            }

            int Id = city_Storemsgrules.Id;
            if (Id == 0)
            {
                //表示新增
                Id = Convert.ToInt32(CityStoreMsgRulesBLL.SingleModel.Add(new CityStoreMsgRules()
                {
                    exptimeday = city_Storemsgrules.exptimeday,
                    price = city_Storemsgrules.price,
                    addTime = DateTime.Now,
                    updateTime = DateTime.Now,
                    aid = city_Storemsgrules.aid,
                    state = 0

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
                CityStoreMsgRules model = CityStoreMsgRulesBLL.SingleModel.GetModel(Id);
                if (model == null)
                {
                    result.Msg = "不存在数据库里";
                    return Json(result);
                }

                if (model.aid != city_Storemsgrules.aid)
                {
                    result.Msg = "权限不足";
                    return Json(result);
                }

                model.updateTime = DateTime.Now;
                model.exptimeday = city_Storemsgrules.exptimeday;
                model.price = city_Storemsgrules.price;

                if (CityStoreMsgRulesBLL.SingleModel.Update(model, "updateTime,exptimeday,price"))
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

        public ActionResult delMsgRules()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int ruleId = Utility.IO.Context.GetRequestInt("ruleId", 0);
            if (appId <= 0 || ruleId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }

            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result);
            }

            CityStoreMsgRules model = CityStoreMsgRulesBLL.SingleModel.getCity_StoreMsgRules(appId, ruleId);
            if (model == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }


            model.state = -1;
            model.updateTime = DateTime.Now;
            if (!CityStoreMsgRulesBLL.SingleModel.Update(model, "state,updateTime"))
            {
                result.Msg = "删除异常";
                return Json(result);
            }

            result.isok = true;
            result.Msg = "删除成功";
            return Json(result);

        }


        #endregion

        #region 信息分类

        public ActionResult CityStoreMsgType(int appId = 0)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.appId = appId;
            return View();


        }

        /// <summary>
        /// 编辑或者新增 信息分类
        /// </summary>
        /// <param name="city_Storemsgrules"></param>
        /// <returns></returns>
        public ActionResult saveMsgType(CityStoreMsgType city_StoreMsgType)
        {

            result = new Return_Msg();
            if (city_StoreMsgType == null)
            {
                result.Msg = "数据不能为空";
                return Json(result);
            }

            if (city_StoreMsgType.aid <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }

            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }


            int Id = city_StoreMsgType.Id;
            if (Id == 0)
            {
                //表示新增
                Id = Convert.ToInt32(CityStoreMsgTypeBLL.SingleModel.Add(new CityStoreMsgType()
                {
                    materialPath = city_StoreMsgType.materialPath,
                    name = city_StoreMsgType.name,
                    addTime = DateTime.Now,
                    updateTime = DateTime.Now,
                    aid = city_StoreMsgType.aid,
                    State = 0,
                    sortNumber = city_StoreMsgType.sortNumber

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
                CityStoreMsgType model = CityStoreMsgTypeBLL.SingleModel.GetModel(Id);
                if (model == null)
                {
                    result.Msg = "不存在数据库里";
                    return Json(result);
                }

                if (model.aid != city_StoreMsgType.aid)
                {
                    result.Msg = "权限不足";
                    return Json(result);
                }
                model.updateTime = DateTime.Now;
                model.materialPath = city_StoreMsgType.materialPath;
                model.name = city_StoreMsgType.name;
                model.sortNumber = city_StoreMsgType.sortNumber;
                if (CityStoreMsgTypeBLL.SingleModel.Update(model, "updateTime,materialPath,name,sortNumber"))
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
        public ActionResult saveMsgTypeSort(List<CityStoreMsgType> list)
        {
            result = new Return_Msg();
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);

            }

            if (list == null || list.Count <= 0)
            {
                result.Msg = "数据不能为空";
                return Json(result);

            }
            CityStoreMsgType model = new CityStoreMsgType();
            TransactionModel tranModel = new TransactionModel();

            string msgTypeIds = string.Join(",",list.Select(s=>s.Id));
            List<CityStoreMsgType> cityStoreMsgTypeList = CityStoreMsgTypeBLL.SingleModel.GetListByIds(msgTypeIds);

            foreach (CityStoreMsgType item in list)
            {
                model = cityStoreMsgTypeList?.FirstOrDefault(f=>f.Id==item.Id);
                if (model == null)
                {
                    result.Msg = $"Id={item.Id}不存在数据库里";
                    return Json(result);
                }

                if (model.aid != item.aid)
                {
                    result.Msg = $"Id={item.Id}权限不足";
                    return Json(result);
                }
                
                model.sortNumber = item.sortNumber;
                model.updateTime = DateTime.Now;
                tranModel.Add(CityStoreMsgTypeBLL.SingleModel.BuildUpdateSql(model, "sortNumber,updateTime"));
            }

            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (CityStoreMsgTypeBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
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
        /// 加入用户本地素材文件
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="materialPath"></param>
        /// <returns></returns>
        public ActionResult addCity_Materials(int appId = 0, string materialPath = "", int storeId = 0)
        {
            result = new Return_Msg();
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            //TODO 开发时注释，上线放开
            //if (dzaccount == null)
            //{
            //    result.Msg = "登录信息超时";
            //    return Json(result);
            //}

            string fileExtension = Path.GetExtension(materialPath).ToLower();
            HashSet<string> img = new HashSet<string>() { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            if (!img.Contains(fileExtension))
            {
                result.Msg = $"上传失败！只支持：{string.Join(",", img)}格式的图片！";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            int Id = Convert.ToInt32(CityMaterialsBLL.SingleModel.Add(new CityMaterials()
            {
                aid = appId,
                storeId = storeId,
                materialPath = materialPath,
                materialType = 0,
                addTime = DateTime.Now

            }));
            result.isok = Id > 0;
            result.Msg = Id > 0 ? "ok" : "error";
            result.dataObj = new
            {
                Id = Id
            };

            return Json(result,JsonRequestBehavior.AllowGet);
        }


        public ActionResult getMaterials()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= -2)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);


            //if (dzaccount == null)
            //{
            //    result.Msg = "登录信息超时";
            //    return Json(result, JsonRequestBehavior.AllowGet);
            //}


            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 30);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int totalCount = 0;
            List<CityMaterials> list = CityMaterialsBLL.SingleModel.getListByaid(appId, out totalCount, pageSize, pageIndex, storeId: storeId);

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult getMsgTypes()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            string msgTypeName = Utility.IO.Context.GetRequest("typeName", string.Empty);
            int totalCount = 0;
            List<CityStoreMsgType> list = CityStoreMsgTypeBLL.SingleModel.getListByaid(appId, out totalCount, pageSize, pageIndex, msgTypeName);

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 删除信息分类包括单个批量
        /// </summary>
        /// <returns></returns>
        public ActionResult delMsgTypes()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result);
            }
            if (!Utility.StringHelper.IsNumByStrs(',', ids))
            {
                result.Msg = "非法操作";
                return Json(result);
            }
            //判断是否有权限
            List<CityStoreMsgType> list = CityStoreMsgTypeBLL.SingleModel.GetListByIds(appId, ids);
            TransactionModel tranModel = new TransactionModel();
            foreach (CityStoreMsgType item in list)
            {
                if (appId != item.aid)
                {
                    result.Msg = $"非法操作(无权限对id={item.Id}的类别)";
                    return Json(result);
                }
                if (CityMsgBLL.SingleModel.msgTypeHaveMsg(appId, item.Id))
                {
                    result.Msg = $"{item.name}类别下关联了帖子,请先删除帖子再删除该类别";
                    return Json(result);
                }

                item.State = -1;
                item.updateTime = DateTime.Now;
                tranModel.Add(CityStoreMsgTypeBLL.SingleModel.BuildUpdateSql(item, "State,updateTime"));

            }


            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (CityStoreMsgTypeBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
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


        /// <summary>
        /// 类别名称是否存在
        /// </summary>
        /// <returns></returns>
        public ActionResult msgTypeNameIsExist()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            string msgTypeName = Utility.IO.Context.GetRequest("msgTypeName", string.Empty);
            if (appId <= 0 || string.IsNullOrEmpty(msgTypeName))
            {
                result.Msg = "参数错误";
                return Json(result);
            }

            CityStoreMsgType model = CityStoreMsgTypeBLL.SingleModel.msgTypeNameIsExist(appId, msgTypeName);
            if (model != null && model.Id != Id)
            {
                result.Msg = "类别名称已存在";
                return Json(result);
            }
            result.isok = true;
            result.Msg = "ok";
            return Json(result);
        }


        #endregion

        #region 信息管理

        public ActionResult cityMsg(int appId, int tab = 0)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.appId = appId;
            ViewBag.tab = tab;
            return View();
        }

        /// <summary>
        /// 获取信息列表
        /// </summary>
        /// <returns></returns>
        public ActionResult getMsg()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }


            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            string msgTypeName = Utility.IO.Context.GetRequest("msgTypeName", string.Empty);
            string userName = Utility.IO.Context.GetRequest("userName", string.Empty);
            string userPhone = Utility.IO.Context.GetRequest("userPhone", string.Empty);
            int isTop = Utility.IO.Context.GetRequestInt("isTop", 0);
            int Review = Utility.IO.Context.GetRequestInt("Review", -2);

            int totalCount = 0;
            List<CityMsg> list = CityMsgBLL.SingleModel.getListByaid(appId, out totalCount, isTop, pageSize, pageIndex, msgTypeName, userName, userPhone, "addTime desc", xcx.AppId, Review);

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 删除信息包括单个批量 或者审核通过、不审核通过
        /// </summary>
        /// <returns></returns>
        public ActionResult delMsg()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int actionType = Utility.IO.Context.GetRequestInt("actionType", 0);//默认0为删除 其它为审核的状态
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result);
            }
            if (!Utility.StringHelper.IsNumByStrs(',', ids))
            {
                result.Msg = "非法操作";
                return Json(result);
            }
            //判断是否有权限
            List<CityMsg> list = CityMsgBLL.SingleModel.GetListByIds(appId, ids);
            TransactionModel tranModel = new TransactionModel();
            CityStoreBanner storebanner = CityStoreBannerBLL.SingleModel.getModelByaid(appId);
            if (storebanner == null)
            {
                result.Msg = "商家配置异常";
                return Json(result);
            }

            foreach (CityMsg item in list)
            {
                if (appId != item.aid)
                {
                    result.Msg = $"非法操作(无权限对id={item.Id}的信息)";
                    return Json(result);
                }

                if (actionType == 0)
                {
                    item.state = -1;
                }
                else
                {
                    if (item.Review != 1)
                    {
                        result.Msg = "已经审核了不需再审核";
                        return Json(result);
                    }

                    //审核操作
                    if (item.state == 0)
                    {
                        //表示先审核后发布 置顶时间计算起始为审核通过的时间
                        item.addTime = DateTime.Now;
                    }

                    item.state = actionType == 2 ? 1 : 0;
                    item.Review = actionType;
                }


                item.updateTime = DateTime.Now;
                tranModel.Add(CityMsgBLL.SingleModel.BuildUpdateSql(item, "state,updateTime,Review,addTime"));

            }


            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (CityMsgBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
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

        public ActionResult getMsgDetail()
        {
            result = new Return_Msg();
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
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            CityMsg model = CityMsgBLL.SingleModel.GetModel(Id);
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
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            int ruleId = Utility.IO.Context.GetRequestInt("ruleId", 0);
            if (appId <= 0 || Id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }
            CityMsg model = CityMsgBLL.SingleModel.GetModel(Id);
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

            CityStoreMsgRules msgRule = CityStoreMsgRulesBLL.SingleModel.getCity_StoreMsgRules(appId, ruleId);
            if (msgRule == null)
            {
                result.Msg = "非法操作(置顶时间有误)";
                return Json(result);

            }
            model.addTime = DateTime.Now;
            model.updateTime = DateTime.Now;
            model.topDay = msgRule.exptimeday;
            model.topCostPrice = msgRule.price;
            model.IsDoTop = 1;

            if (CityMsgBLL.SingleModel.Update(model, "addTime,topDay,topCostPrice,IsDoTop,updateTime"))
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
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);

            if (appId <= 0 || Id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }
            CityMsg model = CityMsgBLL.SingleModel.GetModel(Id);
            if (model == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }


            model.topDay = 0;
            model.IsDoNotTop = 1;
            model.updateTime = DateTime.Now;
            if (CityMsgBLL.SingleModel.Update(model, "topDay,IsDoNotTop,updateTime"))
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

        #region 用户管理
        public ActionResult city_Users(int appId)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.appId = appId;
            return View();
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public ActionResult getUsers()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);


            string userName = Utility.IO.Context.GetRequest("userName", string.Empty);
            string userPhone = Utility.IO.Context.GetRequest("userPhone", string.Empty);

            string startTime = Utility.IO.Context.GetRequest("startTime", string.Empty);
            string endTime = Utility.IO.Context.GetRequest("endTime", string.Empty);

            int totalCount = 0;
            List<CityStoreUser> list = CityStoreUserBLL.SingleModel.getListByaid(appId, out totalCount, pageSize, pageIndex, userName, userPhone, startTime, endTime, "addTime desc", xcx.AppId);
            result.Msg = "获取成功";
            result.isok = true;
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);


        }

        public ActionResult UpdateUsersState()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            if (appId <= 0 || id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            CityStoreUser cityStoreUser = CityStoreUserBLL.SingleModel.GetModel(id);
            if (cityStoreUser == null || cityStoreUser.state == -1)
            {
                result.Msg = "数据不存在";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (cityStoreUser.aid != appId)
            {
                result.Msg = "没有权限";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            cityStoreUser.state = cityStoreUser.state == 0 ? 1 : 0;
            cityStoreUser.updateTime = DateTime.Now;
            if (CityStoreUserBLL.SingleModel.Update(cityStoreUser, "state,updateTime"))
            {
                result.isok = true;
                result.Msg = "操作成功";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            result.Msg = "操作异常";
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region 用户举报信息管理

        public ActionResult reportMsg(int appId)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.appId = appId;
            return View();
        }

        public ActionResult getReportMsgList()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int totalCount = 0;
            List<CityMsgReport> list = CityMsgReportBLL.SingleModel.getListByaid(appId, out totalCount, pageSize, pageIndex);

            result.Msg = "获取成功";
            result.isok = true;
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 确认或者删除举报信息
        /// </summary>
        /// <returns></returns>
        public ActionResult delOrConfirmReportMsg()
        {
            result = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int actionType = Utility.IO.Context.GetRequestInt("actionType", 0);//行为类别 默认为0删除 1为确认
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            if (appId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }
            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                result.Msg = "小程序未授权";
                return Json(result);
            }

            if (!Utility.StringHelper.IsNumByStrs(',', ids))
            {
                result.Msg = "非法操作";
                return Json(result);
            }
            //判断是否有权限
            List<CityMsgReport> list = CityMsgReportBLL.SingleModel.GetListByIds(appId, ids);
            TransactionModel tranModel = new TransactionModel();
            foreach (CityMsgReport item in list)
            {
                if (appId != item.aid)
                {
                    result.Msg = $"非法操作(无权限对id={item.Id}的信息)";
                    return Json(result);
                }

                if (actionType == 0)
                {
                    item.state = -1;
                }
                else
                {
                    item.confirmState = 1;//确认已经处理 然后发送一条处理的系统消息给举报者
                    CityUserMsg city_UserMsg = new CityUserMsg();
                    city_UserMsg.addTime = DateTime.Now;
                    city_UserMsg.aid = appId;
                    city_UserMsg.fromUserId = -1;
                    city_UserMsg.toUserId = item.reportUserId;
                    city_UserMsg.updateTime = DateTime.Now;
                    city_UserMsg.msgBody = WebSiteConfig.City_UserReportMsgTemplate;
                    tranModel.Add(CityUserMsgBLL.SingleModel.BuildAddSql(city_UserMsg));

                }

                item.updateTime = DateTime.Now;
                tranModel.Add(CityMsgReportBLL.SingleModel.BuildUpdateSql(item, "confirmState,state,updateTime"));

            }


            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (CityMsgReportBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
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
            result = new Return_Msg();
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
            result = new Return_Msg();
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
            List<CityMsgComment> listComment = CityMsgCommentBLL.SingleModel.GetCityMsgComment(r.Id, out totalCount, keyMsg, pageSize, pageIndex, Id);

            result.dataObj = new { totalCount = totalCount, list = listComment };
            result.isok = true;
            result.Msg = "获取成功";
            return Json(result, JsonRequestBehavior.AllowGet);


        }


        public ActionResult DeleteMsgComment()
        {
            result = new Return_Msg();
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

            List<CityMsgComment> list = CityMsgCommentBLL.SingleModel.GetListByIds(ids);
            if (list == null || list.Count <= 0)
            {
                result.Msg = "没有评论需要删除";
                return Json(result);
            }

            TransactionModel tranModel = new TransactionModel();
            foreach (CityMsgComment item in list)
            {
                if (item == null || item.AId != r.Id)
                {
                    result.Msg = "帖子评论不存在或者没有权限";
                    return Json(result);
                }
                item.State = -1;
                tranModel.Add(CityMsgCommentBLL.SingleModel.BuildUpdateSql(item, "State"));
            }


            if (CityMsgCommentBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
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