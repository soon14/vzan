using BLL.MiniApp;
using BLL.MiniApp.Plat;
using Entity.MiniApp.Plat;
using Entity.MiniApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Plat.Filters;

namespace User.MiniApp.Areas.Plat.Controllers
{
    [LoginFilter]
    [MiniApp.Filters.RouteAuthCheck]
    public class ConfigController : User.MiniApp.Controllers.baseController
    {
        private  PlatReturnMsg result;
        
        
        
        
        public ConfigController()
        {
           
            
        }

        public ActionResult Setting(int aid = 0)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.PageType = pageType;
            ViewBag.appId = aid;
            PlatConfig platRemark = PlatConfigBLL.SingleModel.GetPlatConfig(aid, 3);//获取平台公告
            if (platRemark == null)
            {
                platRemark = new PlatConfig() { Aid = aid, AddTime = DateTime.Now, ADImg = string.Empty, ConfigType = 3 };
            }
            ViewBag.PlatRemark = platRemark;

            return View(new PlatConfig() { Aid = aid, AddTime = DateTime.Now, ADImg = "" });
        }

        public ActionResult GetSettingList()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int configType = Utility.IO.Context.GetRequestInt("configType", 0);
            int adImgType = Utility.IO.Context.GetRequestInt("adImgType", 0);
            if (appId <= 0)
            {
                result.Msg = "appId非法";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 50);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);


            int totalCount = 0;
            List<PlatConfig> list = new List<PlatConfig>();


            list.AddRange(PlatConfigBLL.SingleModel.getListByaid(appId, out totalCount, configType, pageSize, pageIndex, "sortNumber desc,addTime desc"));

            result.isok = true;
            result.Msg = "获取成功";
            result.dataObj = new { totalCount = totalCount, list = list };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        public ActionResult SaveConfig(PlatConfig platConfig)
        {
            result = new PlatReturnMsg();
            if (platConfig == null)
            {
                result.Msg = "数据不能为空";
                return Json(result);
            }


            if (platConfig.Aid <= 0)
            {
                result.Msg = "appId非法";
                return Json(result);
            }
            TransactionModel TranModel = new TransactionModel();
            int curCount = PlatConfigBLL.SingleModel.GetCountByType(platConfig.Aid, platConfig.ConfigType);
            switch (platConfig.ConfigType)
            {
                case 0://广告图
                    if (curCount >= 5)
                    {
                        result.Msg = "上限为5条";
                        return Json(result);
                    }
                    break;
                case 1://推荐商家
                    if (curCount >= 12)
                    {
                        result.Msg = "上限为12家";
                        return Json(result);
                    }
                    break;
                case 2://置顶商家
                    if (curCount >= 30)
                    {
                        result.Msg = "上限为30家";
                        return Json(result);
                    }

                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(platConfig.ObjId, platConfig.isStoreID == 0 ? 1 : 0);
                    if (platStore == null)
                    {
                        result.Msg = "操作异常(商家不存在)";
                        return Json(result);
                    }
                    if (platStore.Top == 1)
                    {
                        result.Msg = "商家已置顶";
                        return Json(result);
                    }
                    platStore.Top = 1;
                    TranModel.Add(PlatStoreBLL.SingleModel.BuildUpdateSql(platStore, "Top"));

                    break;
            }




            TranModel.Add(PlatConfigBLL.SingleModel.BuildAddSql(new PlatConfig()
            {
                Aid = platConfig.Aid,
                ObjId = platConfig.ObjId,
                ConfigType = platConfig.ConfigType,
                ADImgType = platConfig.ADImgType,
                ADImg = platConfig.ADImg,
                AddTime = DateTime.Now,
                isStoreID = platConfig.isStoreID,
                Name=platConfig.Name

            }));


            //表示新增

            if (PlatConfigBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray))
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

        /// <summary>
        /// 保存流量广告
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public ActionResult SaveQianRuGuangGaoConfig(List<PlatConfig> list)
        {
            result = new PlatReturnMsg();
            if (list == null || list.Count<=0)
            {
                result.Msg = "数据不能为空";
                return Json(result);
            }
            
            TransactionModel TranModel = new TransactionModel();
            foreach (PlatConfig item in list)
            {
                if(item.Id<=0)
                {
                    TranModel.Add(PlatConfigBLL.SingleModel.BuildAddSql(new PlatConfig()
                    {
                        Aid = item.Aid,
                        ConfigType = item.ConfigType,
                        ADImgType = item.ADImgType,
                        ADImg = item.ADImg,
                        AddTime = DateTime.Now,
                        Name = item.Name,
                    }));
                }
                else
                {
                    TranModel.Add(PlatConfigBLL.SingleModel.GetUpdateAdImgSql(item.ADImg,item.Id));
                }
            }
            
            //表示新增
            if (PlatConfigBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray))
            {
                result.isok = true;
                result.Msg = "保存成功";
                return Json(result);
            }

            result.Msg = "保存失败";
            return Json(result);
        }

        /// <summary>
        /// 平台公告新增或者更新
        /// </summary>
        /// <returns></returns>
        public ActionResult SavePlatRemark()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int type = Utility.IO.Context.GetRequestInt("type", 0);
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            int objId = Utility.IO.Context.GetRequestInt("objId", 0);
           int virtualPV= Utility.IO.Context.GetRequestInt("virtualPV", 0);
            int ADImgType = Utility.IO.Context.GetRequestInt("ADImgType", 0);
            string platRemark = Utility.IO.Context.GetRequest("platRemark", string.Empty);
            if (appId <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);

            }

            PlatConfig platConfig = PlatConfigBLL.SingleModel.GetModel(id);
            if (platConfig == null||platConfig.State==-1)
            {
                //表示新增
                platConfig = new PlatConfig();
                platConfig.Aid = appId;
                platConfig.AddTime = DateTime.Now;
                platConfig.ObjId = objId;
                if (type == 0)
                {
                    //表示公告
                    platConfig.ADImg = platRemark;
                    platConfig.ConfigType = 3;
                    platConfig.ADImgType = 2;
                }else if (type == 2)
                {
                    //表示关注公众号组件
                    platConfig.ConfigType = 6;
                    platConfig.ADImgType = ADImgType;
                }
                else
                {
                    //表示虚拟访问量跟虚拟发帖量
                    platConfig.ConfigType = 5;
                    platConfig.ADImgType = virtualPV;
                }

               


              
                id = Convert.ToInt32(PlatConfigBLL.SingleModel.Add(platConfig));
                if (id > 0)
                {

                    result.isok = true;
                    result.Msg = "新增成功";
                    return Json(result);
                }

                result.Msg = "新增异常";
                return Json(result);
            }
            else
            {
                //表示更新
                string updateFiled = "ADImg,ObjId";
                platConfig.ObjId = objId;
                if (type == 0)
                {
                    platConfig.ADImg = platRemark;
                }
                else if (type == 2)
                {
                    //表示关注公众号组件
                    platConfig.ConfigType = 6;
                    platConfig.ADImgType = ADImgType;
                    updateFiled = "ADImgType,ObjId";
                }
                else
                {
                    platConfig.ADImgType = virtualPV;
                    updateFiled = "ADImgType,ObjId";
                }
               
                if(PlatConfigBLL.SingleModel.Update(platConfig, updateFiled))
                {
                    result.isok = true;
                    result.Msg = "更新成功";
                    return Json(result);
                }

                result.Msg = "更新异常";
                return Json(result);

            }
        }


        /// <summary>
        /// 更新排序/删除
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateConfig()
        {
            result = new PlatReturnMsg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            int actionType = Utility.IO.Context.GetRequestInt("actionType", 0);//默认排序0 -1删除
            int sortNumber = Utility.IO.Context.GetRequestInt("sortNumber", 0);

            if (appId <= 0 || id <= 0)
            {
                result.Msg = "参数错误";
                return Json(result);

            }
            TransactionModel TranModel = new TransactionModel();
            PlatConfig platConfig = PlatConfigBLL.SingleModel.GetModel(id);
            if (platConfig == null || platConfig.Aid != appId)
            {
                result.Msg = "数据不存在";
                return Json(result);
            }

            if (actionType == -1)
            {

                if (platConfig.ConfigType == 2)
                {
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(platConfig.ObjId, platConfig.isStoreID == 0 ? 1 : 0);
                    if (platStore == null)
                    {
                        result.Msg = "操作异常(商家不存在)";
                        return Json(result);
                    }
                    platStore.Top = 0;
                    TranModel.Add(PlatStoreBLL.SingleModel.BuildUpdateSql(platStore, "Top"));
                }




                platConfig.State = -1;
                TranModel.Add(PlatConfigBLL.SingleModel.BuildUpdateSql(platConfig, "State"));
            }
            else
            {
                platConfig.SortNumber = sortNumber;

                TranModel.Add(PlatConfigBLL.SingleModel.BuildUpdateSql(platConfig, "SortNumber"));
            }


            if (PlatConfigBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray))
            {
                result.isok = true;
                result.Msg = "操作成功";
                return Json(result);
            }
            else
            {
                result.Msg = "操作异常";
                return Json(result);
            }




        }


        public ActionResult OtherSetting(int aid)
        {
            if (aid <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "参数错误!", code = "500" });

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);
            if (pageType <= 0)
                return View("PageError", new PlatReturnMsg() { Msg = "小程序模板不存在!", code = "500" });

            ViewBag.PageType = pageType;
            ViewBag.appId = aid;

            #region 平台公告
            PlatConfig platRemark = PlatConfigBLL.SingleModel.GetPlatConfig(aid, 3);//获取平台公告
            if (platRemark == null)
            {
                platRemark = new PlatConfig() { Aid = aid, AddTime = DateTime.Now, ADImg = string.Empty, ConfigType = 3,ObjId=1 };
            }
            ViewBag.PlatRemark = platRemark; 
            #endregion

            #region 发帖量 访问量
            PlatConfig platOther = PlatConfigBLL.SingleModel.GetPlatConfig(aid, 5);//获取平台公告
            if (platOther == null)
            {
                platOther= new PlatConfig() { Aid = aid, AddTime = DateTime.Now, ConfigType = 5 };
            }
            PlatOtherConfig platOtherConfig = new PlatOtherConfig();
            platOtherConfig.VirtualPV = platOther.ADImgType;
            platOtherConfig.VirtualPlatMsgCount = platOther.ObjId;
            platOtherConfig.PlatMsgCount= PlatMsgBLL.SingleModel.GetCountByAId(aid);
            platOtherConfig.PV= PlatStatisticalFlowBLL.SingleModel.GetPVCount(aid);
            platOtherConfig.PlatConfigId = platOther.Id;
            #endregion


            PlatConfig platOfficialAccount = PlatConfigBLL.SingleModel.GetPlatConfig(aid, 6);//获取关注公众号设置
            if (platOfficialAccount == null)
            {
                platOfficialAccount = new PlatConfig() { Aid = aid, AddTime = DateTime.Now, ConfigType = 6};
            }
            ViewBag.PlatOfficialAccount = platOfficialAccount;



            return View(platOtherConfig);
        }




    }
}