using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Plat.Filters;
namespace User.MiniApp.Areas.Plat.Controllers
{
    [LoginFilter]
    [MiniApp.Filters.RouteAuthCheck]
    public class StoreController : User.MiniApp.Controllers.baseController
    {
        private PlatReturnMsg result;

        
        
        
        
        
        
        

        public StoreController()
        {
            
            
            
            
            
            
        }


        #region 店铺类别操作
        /// <summary>
        /// 店铺类别
        /// </summary>
        /// <returns></returns>
        public ActionResult Category(int aid = 0, int isFirstType = 2)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });

            int totalCount = 0;
            List<PlatStoreCategory> list = new List<PlatStoreCategory>();
            list.Add(new PlatStoreCategory()
            {
                Id = 0,
                Name = "请选择"
            });
            list.AddRange(PlatStoreCategoryBLL.SingleModel.getListByaid(aid, out totalCount, 0, 100, 1));

            PlatStoreCategoryConfig platStoreCategoryConfig = PlatStoreCategoryConfigBLL.SingleModel.GetModelByAid(aid);
            if (platStoreCategoryConfig == null)
            {
                platStoreCategoryConfig = new PlatStoreCategoryConfig()
                {

                    Aid = aid,
                    AddTime = DateTime.Now,
                    Level = 1,
                    SyncSwitch = 0

                };
                int id = Convert.ToInt32(PlatStoreCategoryConfigBLL.SingleModel.Add(platStoreCategoryConfig));
                if (id <= 0)
                {
                    return View("PageError", new PlatReturnMsg() { Msg = "初始化数据异常!", code = "500" });
                }

            }

            ViewBag.PlatStoreCategoryConfig = platStoreCategoryConfig;
            ViewBag.isFirstType = isFirstType;
            ViewBag.PageType = pageType;
            ViewBag.appId = aid;
            return View(list);
        }



        /// <summary>
        /// 获取店铺分类
        /// </summary>
        /// <returns></returns>

        public ActionResult GetCategoryList()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int isFirstType = Utility.IO.Context.GetRequestInt("isFirstType", 1);
            int parentId = Utility.IO.Context.GetRequestInt("parentId", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int isList = Utility.IO.Context.GetRequestInt("isList", 0);
            int totalCount = 0;
            List<PlatStoreCategory> list = new List<PlatStoreCategory>();
            if (isList != 0)
            {
                list.Add(new PlatStoreCategory()
                {
                    Id = 0,
                    Name = "请选择"
                });
            }

            list.AddRange(PlatStoreCategoryBLL.SingleModel.getListByaid(appId, out totalCount, isFirstType, pageSize, pageIndex, "sortNumber desc,addTime desc", parentId));

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 类别名称是否存在
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckCategoryName()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int Id = Utility.IO.Context.GetRequestInt("Id", 0);
            int isFirstType = Utility.IO.Context.GetRequestInt("isFirstType", 0);
            string categoryName = Utility.IO.Context.GetRequest("categoryName", string.Empty);
            if (appId <= 0 || string.IsNullOrEmpty(categoryName))
            {
                result.Msg = "参数错误";
                return Json(result);
            }

            PlatStoreCategory model = PlatStoreCategoryBLL.SingleModel.msgTypeNameIsExist(appId, categoryName, isFirstType);
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

        public ActionResult SaveCategory(PlatStoreCategory platStoreCategory)
        {

            result = new PlatReturnMsg();
            if (platStoreCategory == null)
            {
                result.Msg = "数据不能为空";
                return Json(result);
            }

            if (platStoreCategory.Aid <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }



            int Id = platStoreCategory.Id;
            if (Id == 0)
            {
                //表示新增
                Id = Convert.ToInt32(PlatStoreCategoryBLL.SingleModel.Add(new PlatStoreCategory()
                {
                    MaterialPath = platStoreCategory.MaterialPath,
                    Name = platStoreCategory.Name,
                    AddTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    Aid = platStoreCategory.Aid,
                    State = 0,
                    SortNumber = platStoreCategory.SortNumber,
                    ParentId = platStoreCategory.ParentId

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
                PlatStoreCategory model = PlatStoreCategoryBLL.SingleModel.GetModel(Id);
                if (model == null)
                {
                    result.Msg = "不存在数据库里";
                    return Json(result);
                }

                if (model.Aid != platStoreCategory.Aid)
                {
                    result.Msg = "权限不足";
                    return Json(result);
                }
                model.UpdateTime = DateTime.Now;
                model.MaterialPath = platStoreCategory.MaterialPath;
                model.Name = platStoreCategory.Name;
                model.SortNumber = platStoreCategory.SortNumber;
                model.ParentId = platStoreCategory.ParentId;
                if (PlatStoreCategoryBLL.SingleModel.Update(model, "UpdateTime,MaterialPath,Name,SortNumber,ParentId"))
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
        /// 删除店铺类别包括单个批量
        /// </summary>
        /// <returns></returns>

        public ActionResult DelCategory()
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
            List<PlatStoreCategory> list = PlatStoreCategoryBLL.SingleModel.GetListByIds(appId, ids);
            TransactionModel tranModel = new TransactionModel();
            foreach (PlatStoreCategory item in list)
            {
                if (appId != item.Aid)
                {
                    result.Msg = $"非法操作(无权限对id={item.Id}的类别)";
                    return Json(result);
                }
                int count = 0;
                if (item.ParentId == 0)
                {
                    //表示删除的是大类,要检测有没有子类,没有子类才能删除
                    count = PlatStoreCategoryBLL.SingleModel.GetSecondCategoryCount(item.Id);
                    if (count > 0)
                    {
                        result.Msg = $"{item.Name}类别下包含{count}子类别,请先删除子类别再删除)";
                        return Json(result);
                    }
                }
                //else
                //{

                //    //表示删除的是子类,要检测该类别下有没有关联的店铺,没有店铺才能删除
                //    count = _platStoreBLL.
                //    if (count > 0)
                //    {
                //        result.Msg = $"{item.Name}类别下包含{count}子类别,请先删除子类别再删除)";
                //        return Json(result);
                //    }

                //}






                item.State = -1;
                item.UpdateTime = DateTime.Now;
                tranModel.Add(PlatStoreCategoryBLL.SingleModel.BuildUpdateSql(item, "State,UpdateTime"));

            }


            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatStoreCategoryBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
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
        /// 批量更新排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>

        public ActionResult SaveCategorySort(List<PlatStoreCategory> list)
        {
            result = new PlatReturnMsg();

            if (list == null || list.Count <= 0)
            {
                result.Msg = "数据不能为空";
                return Json(result);

            }
            PlatStoreCategory model = new PlatStoreCategory();
            TransactionModel tranModel = new TransactionModel();
            string sql = string.Empty;

            string categoryIds = string.Join(",",list.Select(s=>s.Id));
            List<PlatStoreCategory> platStoreCategoryList = PlatStoreCategoryBLL.SingleModel.GetListByIds(categoryIds);
            foreach (PlatStoreCategory item in list)
            {
                model = platStoreCategoryList?.FirstOrDefault(f=>f.Id == item.Id);
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
                sql = PlatStoreCategoryBLL.SingleModel.BuildUpdateSql(model, "SortNumber,UpdateTime");
                tranModel.Add(sql);

            }

            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatStoreCategoryBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
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
        /// 开启或者关闭平台店铺分类二级模式
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateStoreCategoryLevel()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int id = Utility.IO.Context.GetRequestInt("Id", 0);
            int act = Utility.IO.Context.GetRequestInt("act", 0);//默认平台店铺级别配置 1表示 优选商城商品数据同步配置
            if (appId <= 0 || id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);
            }

            PlatStoreCategoryConfig platStoreCategoryConfig = PlatStoreCategoryConfigBLL.SingleModel.GetModel(id);

            if (platStoreCategoryConfig.Aid != appId)
            {
                result.Msg = "暂无权限";
                return Json(result);
            }
            string filed = "Level";
            if (act == 0)
            {
                platStoreCategoryConfig.Level = platStoreCategoryConfig.Level == 1 ? 2 : 1;
            }
            else
            {
                filed = "SyncSwitch";
                platStoreCategoryConfig.SyncSwitch = platStoreCategoryConfig.SyncSwitch == 0 ? 1 : 0;
            }

            platStoreCategoryConfig.UpdateTime = DateTime.Now;

            if (PlatStoreCategoryConfigBLL.SingleModel.Update(platStoreCategoryConfig, $"{filed},UpdateTime"))
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


        /// <summary>
        /// 店铺列表
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult Index(int aid)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });
            ViewBag.appId = aid;

            int totalCount = 0;
            List<PlatStoreCategory> list = new List<PlatStoreCategory>();
            list.Add(new PlatStoreCategory()
            {
                Id = 0,
                Name = "请选择"
            });
            list.AddRange(PlatStoreCategoryBLL.SingleModel.getListByaid(aid, out totalCount, 0, 100, 1));

            List<object> listBindMyCard = PlatStoreBLL.SingleModel.GetBindMyCard(aid);
            ViewBag.listBindMyCard = listBindMyCard;

            return View(list);
        }


        /// <summary>
        /// 获取店铺列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStoreList()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (r == null)
            {
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }


            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            string storeName = Utility.IO.Context.GetRequest("storeName", string.Empty);
            string appName = Utility.IO.Context.GetRequest("appName", string.Empty);
            string agentName = Utility.IO.Context.GetRequest("agentName", string.Empty);
            int storeState = Utility.IO.Context.GetRequestInt("storeState", -1);
            string categoryName = Utility.IO.Context.GetRequest("categoryName", string.Empty);
            int haveAid = Utility.IO.Context.GetRequestInt("haveAid", -1);//是否绑定了小程序 0则表示没有否则表示绑定了
            int fromType = Utility.IO.Context.GetRequestInt("fromType", -1);//数据来源 0表示通过平台入驻  1表示同步其它平台建立的关系
            string storeOwnerPhone = Utility.IO.Context.GetRequest("userPhone", string.Empty);//会员账号
            int totalCount = 0;
            List<PlatStoreRelation> list = PlatStoreBLL.SingleModel.GetListStore(appId, out totalCount, storeName, appName, agentName, categoryName, storeState, haveAid, fromType, pageSize, pageIndex, "AddTime desc", storeOwnerPhone,appId:r.AppId);

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 更改店铺在平台的显示类别
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateStoreRelationCategory()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int storeRelationId = Utility.IO.Context.GetRequestInt("storeRelationId", 0);
            int categoryId = Utility.IO.Context.GetRequestInt("Id", 0);
            if (appId <= 0 || storeRelationId <= 0 || categoryId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            PlatStoreRelation platStoreRelation = PlatStoreRelationBLL.SingleModel.GetModel(storeRelationId);
            if (platStoreRelation == null || platStoreRelation.Aid != appId)
            {
                result.Msg = "操作异常";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            TransactionModel tranModel = new TransactionModel();

            platStoreRelation.Category = categoryId;
            platStoreRelation.UpdateTime = DateTime.Now;
            tranModel.Add(PlatStoreRelationBLL.SingleModel.BuildUpdateSql(platStoreRelation, "Category,UpdateTime"));
            if (platStoreRelation.FromType == 0)
            {
                //表示店铺入驻过来的，也就是本平台的 还需要更改店铺类别
                //否则没有权限更改店铺里的类别 只能更改在本平台显示的店铺类别 因为是同步其它代理商的
                PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(platStoreRelation.StoreId);
                if (platStore == null && platStore.BindPlatAid != appId)
                {
                    result.Msg = "暂无权限操作";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                platStore.Category = categoryId;
                platStore.UpdateTime = DateTime.Now;
                tranModel.Add(PlatStoreBLL.SingleModel.BuildUpdateSql(platStore, "Category,UpdateTime"));
            }

            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
            {
                if (PlatStoreRelationBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
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
        /// 更改店铺在平台的显示状态
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateStoreRelationState()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int act = Utility.IO.Context.GetRequestInt("act", 0);//默认为屏蔽操作
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);

            if (appId <= 0 || string.IsNullOrEmpty(ids))
            {
                result.Msg = "参数错误";
                return Json(result, JsonRequestBehavior.AllowGet);
            }


            TransactionModel transactionModel = new TransactionModel();

            List<PlatStoreRelation> list = PlatStoreRelationBLL.SingleModel.GetListPlatStoreRelation(ids);
            if (list != null && list.Count <= 0)
            {
                result.Msg = "请先选择需要操作的数据";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            foreach (PlatStoreRelation item in list)
            {
                if (item.Aid != appId)
                {
                    result.Msg = "操作异常(暂无权限)";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                item.State = act;
                item.UpdateTime = DateTime.Now;
                transactionModel.Add(PlatStoreRelationBLL.SingleModel.BuildUpdateSql(item, "State,UpdateTime"));

            }




            if (PlatStoreRelationBLL.SingleModel.ExecuteTransactionDataCorect(transactionModel.sqlArray))
            {
                result.isok = true;
                result.Msg = "操作成功";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                result.Msg = "操作失败(请联系客服)";
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }


        /// <summary>
        /// 查看店铺详情
        /// </summary>
        /// <returns></returns>
        public ActionResult StoreDetail()
        {
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            int storeRelationId = Utility.IO.Context.GetRequestInt("storeRelationId", 0);
            if (appId <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            PlatStoreRelation platStoreRelation = PlatStoreRelationBLL.SingleModel.GetModel(storeRelationId);
            if (platStoreRelation == null)
                return View("PageError", new PlatReturnMsg() { Msg = "数据异常!", code = "404" });

            PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(platStoreRelation.StoreId);
            if (platStore == null)
                return View("PageError", new PlatReturnMsg() { Msg = "店铺不存在!", code = "404" });


            return View(platStore);


        }


        /// <summary>
        /// 优选商城栏目的商品数据库
        /// </summary>
        /// <returns></returns>
        public ActionResult GoodsDB(int aid = 0)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });

            PlatStoreCategoryConfig platStoreCategoryConfig = PlatStoreCategoryConfigBLL.SingleModel.GetModelByAid(aid);
            if (platStoreCategoryConfig == null)
            {
                platStoreCategoryConfig = new PlatStoreCategoryConfig()
                {

                    Aid = aid,
                    AddTime = DateTime.Now,
                    Level = 1,
                    SyncSwitch = 0

                };
                int id = Convert.ToInt32(PlatStoreCategoryConfigBLL.SingleModel.Add(platStoreCategoryConfig));
                if (id <= 0)
                {
                    return View("PageError", new PlatReturnMsg() { Msg = "初始化数据异常!", code = "500" });
                }

            }


            int totalCount = 0;
            List<PlatStoreCategory> list = new List<PlatStoreCategory>();
            list.Add(new PlatStoreCategory()
            {
                Id = 0,
                Name = "请选择"
            });
            list.AddRange(PlatStoreCategoryBLL.SingleModel.getListByaid(aid, out totalCount, 0, 100, 1));

            ViewBag.FirstCategory = list;
            ViewBag.appId = aid;
            return View(platStoreCategoryConfig);
        }


        public ActionResult GetGoodsDB()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (r == null)
            {
                result.Msg = "小程序未授权";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            string storeName = Utility.IO.Context.GetRequest("storeName", string.Empty);
            string goodsName = Utility.IO.Context.GetRequest("goodsName", string.Empty);
            int goodsSync = Utility.IO.Context.GetRequestInt("goodsSync", -1);//表示全部 0表示未同步的 1表示已经同步了的
            string categoryName = Utility.IO.Context.GetRequest("categoryName", string.Empty);//产品所绑定的小类名称

            int totalCount = 0;
            List<PlatChildGoods> listGoods = PlatStoreBLL.SingleModel.GetSyncGoods(appId, out totalCount, storeName, goodsName, goodsSync, categoryName, pageSize, pageIndex,appId:r.AppId);

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = listGoods };
            return Json(result, JsonRequestBehavior.AllowGet);


        }


        /// <summary>
        /// 同步或者取消商品到平台商品数据库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SyncGoods()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("aid", 0);
            int act = Utility.IO.Context.GetRequestInt("act", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            TransactionModel transactionModel = new TransactionModel();
            string ids = Utility.IO.Context.GetRequest("ids", string.Empty);
            List<PlatGoodsRelation> list = PlatGoodsRelationBLL.SingleModel.GetPlatGoodsRelationListByGoodsId(appId, ids);
            foreach (PlatGoodsRelation item in list)
            {
                item.Synchronized = act;
                transactionModel.Add(PlatGoodsRelationBLL.SingleModel.BuildUpdateSql(item, "Synchronized"));

            }

            if (transactionModel.sqlArray.Length > 0 && PlatGoodsRelationBLL.SingleModel.ExecuteTransactionDataCorect(transactionModel.sqlArray))
            {
                result.Msg = "操作成功";
                result.isok = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            result.dataObj = transactionModel.sqlArray;
            result.Msg = "操作失败";
            return Json(result, JsonRequestBehavior.AllowGet);

        }




        /// <summary>
        /// 在平台店铺列表里新增没有主人的店铺
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateStore(int aid = 0)
        {
            int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);
            PlatStoreCategoryConfig platStoreCategoryConfig = PlatStoreCategoryConfigBLL.SingleModel.GetModelByAid(aid);
            if (platStoreCategoryConfig == null)
            {
                platStoreCategoryConfig = new PlatStoreCategoryConfig()
                {

                    Aid = aid,
                    AddTime = DateTime.Now,
                    Level = 1,
                    SyncSwitch = 0

                };
                int id = Convert.ToInt32(PlatStoreCategoryConfigBLL.SingleModel.Add(platStoreCategoryConfig));
                if (id <= 0)
                {
                    return View("PageError", new PlatReturnMsg() { Msg = "初始化数据异常!", code = "500" });
                }

            }

            List<PlatStoreCategory> list = new List<PlatStoreCategory>();
            list.Add(new PlatStoreCategory()
            {
                Id = 0,
                Name = "请选择"
            });
            int totalCount = 0;
            ViewBag.firstCategoryId = 0;
            ViewBag.appId = aid;
            list.AddRange(PlatStoreCategoryBLL.SingleModel.getListByaid(aid, out totalCount, platStoreCategoryConfig.Level == 1 ? 2 : 1, 100, 1));
            ViewBag.CategoryList = list;
            ViewBag.CategoryLevel = platStoreCategoryConfig.Level;
            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(dzuserId.ToString());
            ViewBag.AgentinfoId = agentinfo == null ? 0 : agentinfo.id;
            PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(storeId);
            bool isHave = false;
            if (platStore == null)
            {

                platStore = new PlatStore();
                platStore.BindPlatAid = aid;
                isHave = true;
            }
            else
            {
                PlatStoreCategory platStoreCategory = PlatStoreCategoryBLL.SingleModel.GetModel(platStore.Category);
                if (platStoreCategory != null)
                {
                    ViewBag.firstCategoryId = platStoreCategory.ParentId;
                }

                if (!string.IsNullOrEmpty(platStore.StoreService))
                {
                    platStore.StoreServiceModelList = JsonConvert.DeserializeObject<List<StoreServiceModel>>(platStore.StoreService);
                }
                else
                {
                    isHave = true;
                }
            }

            if (isHave)
            {
                List<StoreServiceModel> listService = new List<StoreServiceModel>();
                listService.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "WIFI" });
                listService.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "停车位" });
                listService.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "支付宝支付" });
                listService.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "微信支付" });
                listService.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "刷卡支付" });
                listService.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "空调雅座" });
                listService.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "付费停车" });
                listService.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "接送服务" });
                platStore.StoreServiceModelList = listService;
            }

            if (!string.IsNullOrEmpty(platStore.SwitchConfig))
            {
                platStore.SwitchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
            }
            else
            {
                platStore.SwitchModel = new PlatStoreSwitchModel();
            }


            return View(platStore);
        }




        public ActionResult AddSetting(int aid = 0)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            PlatStoreAddSetting platStoreAddSetting = PlatStoreAddSettingBLL.SingleModel.GetPlatStoreAddSetting(aid);
            if (platStoreAddSetting == null)
            {
                platStoreAddSetting = new PlatStoreAddSetting()
                {
                    Aid = aid,
                    AddWay = 0,
                    AddTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };
                int id = Convert.ToInt32(PlatStoreAddSettingBLL.SingleModel.Add(platStoreAddSetting));
            }
            else
            {
                if (platStoreAddSetting.AddWay == 1&&PlatStoreAddRulesBLL.SingleModel.GetRuleCount(aid)<=0)
                {
                    platStoreAddSetting.AddWay = 0;
                    platStoreAddSetting.UpdateTime = DateTime.Now;
                    if (!PlatStoreAddSettingBLL.SingleModel.Update(platStoreAddSetting, "AddWay,UpdateTime"))
                    {
                        return View("PageError", new PlatReturnMsg() { Msg = "初始化异常!", code = "500" });
                    }
                }
            }
            ViewBag.AddWay = platStoreAddSetting.AddWay;
            ViewBag.AddRule = new PlatStoreAddRules()
            {
                Aid = aid,
                AddTime = DateTime.Now,
                YearCount = 1,
                CostPrice = 1000,
                UpdateTime = DateTime.Now
            };
            ViewBag.appId = aid;
            return View();
        }

        [HttpPost]
        public ActionResult SaveAddSetting()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int addWay = Utility.IO.Context.GetRequestInt("addWay", 0);
            if (appId <= 0 || addWay <0)
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

            PlatStoreAddSetting platStoreAddSetting = PlatStoreAddSettingBLL.SingleModel.GetPlatStoreAddSetting(appId);
            if (platStoreAddSetting == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }


            platStoreAddSetting.AddWay = addWay;
            platStoreAddSetting.UpdateTime = DateTime.Now;
            if (!PlatStoreAddSettingBLL.SingleModel.Update(platStoreAddSetting, "AddWay,UpdateTime"))
            {
                result.Msg = "设置异常";
                return Json(result);
            }

            result.isok = true;
            result.Msg = "设置成功";
            return Json(result);

        }



        public ActionResult GetRules()
        {
            result = new PlatReturnMsg();
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
            List<PlatStoreAddRules> list = PlatStoreAddRulesBLL.SingleModel.getListByaid(appId, out totalCount, pageSize, pageIndex);

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        public ActionResult SaveRule(PlatStoreAddRules rule)
        {
            result = new PlatReturnMsg();
            if (rule == null)
            {
                result.Msg = "数据不能为空";
                return Json(result);
            }


            if (rule.Aid <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }

            if (dzaccount == null)
            {
                result.Msg = "登录信息超时";
                return Json(result);
            }

            if (!Regex.IsMatch(rule.YearCount.ToString(), @"^\+?[1-9][0-9]*$"))
            {
                result.Msg = "使用期限必须为大于零的整数";
                return Json(result);
            }

            if (!Regex.IsMatch(rule.CostPrice.ToString(), @"^\+?[0-9][0-9]*$"))
            {
                result.Msg = "金额不合法";
                return Json(result);
            }

            if (PlatStoreAddRulesBLL.SingleModel.GetRuleByYearCount(rule.Aid, rule.YearCount,rule.Id) != null)
            {
               
                result.Msg = "已存在相同使用年限规则";
                return Json(result);
            }


            int Id = rule.Id;
            if (Id == 0)
            {
                //表示新增
                Id = Convert.ToInt32(PlatStoreAddRulesBLL.SingleModel.Add(new PlatStoreAddRules()
                {
                    YearCount = rule.YearCount,
                    CostPrice = rule.CostPrice,
                    AddTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    Aid = rule.Aid,
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
                PlatStoreAddRules model = PlatStoreAddRulesBLL.SingleModel.GetModel(Id);
                if (model == null)
                {
                    result.Msg = "不存在数据库里";
                    return Json(result);
                }

                if (model.Aid != rule.Aid)
                {
                    result.Msg = "权限不足";
                    return Json(result);
                }

                model.UpdateTime = DateTime.Now;
                model.YearCount = rule.YearCount;
                model.CostPrice = rule.CostPrice;

                if (PlatStoreAddRulesBLL.SingleModel.Update(model, "UpdateTime,YearCount,CostPrice"))
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

        public ActionResult DelRule()
        {
            result = new PlatReturnMsg();
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

            PlatStoreAddRules model = PlatStoreAddRulesBLL.SingleModel.getRule(appId, ruleId);
            if (model == null)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }


            model.State = -1;
            model.UpdateTime = DateTime.Now;
            if (!PlatStoreAddRulesBLL.SingleModel.Update(model, "State,UpdateTime"))
            {
                result.Msg = "删除异常";
                return Json(result);
            }

            result.isok = true;
            result.Msg = "删除成功";
            return Json(result);

        }




    }
}