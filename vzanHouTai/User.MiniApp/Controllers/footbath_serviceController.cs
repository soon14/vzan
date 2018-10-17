using Entity.MiniApp.Footbath;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Entity.MiniApp.Ent;
using Entity.MiniApp;
using Utility.IO;
using MySql.Data.MySqlClient;
using BLL.MiniApp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Footbath;

namespace User.MiniApp.Controllers
{
    public partial class footbathController : configController
    {
        // GET: footbath_service：足浴版小程序-服务项目
        #region 服务项目
        /// <summary>
        /// 项目列表 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult ServiceList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            ViewBag.appId = appId;
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有数据!", code = "500" });
            }
            List<EntGoodType> goodTypeList = EntGoodTypeBLL.SingleModel.GetServiceItemList(appId,storeModel.Id,(int)GoodProjectType.足浴版服务项目分类);

            return View(goodTypeList);
        }

        /// <summary>
        /// 获取项目列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetServiceList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int ptypes = Context.GetRequestInt("ptypes", 0);
            string name = Context.GetRequest("name", string.Empty);
            int tag = Context.GetRequestInt("tag", -1);

            int recordCount = 0;
            List<EntGoods> serviceList = EntGoodsBLL.SingleModel.GetServiceList(appId, (int)GoodsType.足浴版服务, ptypes, name, tag, pageSize, pageIndex,out recordCount);
                
            if (serviceList != null && serviceList.Count > 0)
            {
                serviceList.ForEach(s =>
                {
                    if (!string.IsNullOrEmpty(s.ptypes))
                    {
                        List<EntGoodType> typeList = EntGoodTypeBLL.SingleModel.GetList($"id in ({s.ptypes}) and type = {(int)GoodProjectType.足浴版服务项目分类} and state> 0");
                        if (typeList != null && typeList.Count > 0)
                        {
                            foreach (EntGoodType typeInfo in typeList)
                            {
                                s.ptypestr += typeInfo.name + ",";
                            }
                        }
                        s.ptypestr = s.ptypestr.TrimEnd(',');
                    }

                });
            }
            return Json(new { isok = true, serviceList = serviceList, recordCount = recordCount });
        }

        /// <summary>
        /// 保存添加编辑服务
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SaveServiceInfo(EntGoods serviceInfo)
        {
            if (serviceInfo.aid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(serviceInfo.aid, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={serviceInfo.aid}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            if (string.IsNullOrEmpty(serviceInfo.name))
            {
                return Json(new { isok = false, msg = "请输入项目名称" });
            }
            if (serviceInfo.name.Length > 30)
            {
                return Json(new { isok = false, msg = "项目名称字数不可超过30字" });
            }
            if (string.IsNullOrEmpty(serviceInfo.ptypes))
            {
                return Json(new { isok = false, msg = "请选择项目分类" });
            }
            if (serviceInfo.price < 0 || serviceInfo.price > 999999999)
            {
                return Json(new { isok = false, msg = "价格范围：0~999999999，最多两位小数" });
            }
            if (serviceInfo.ServiceTime < 0 || serviceInfo.ServiceTime > 999)
            {
                return Json(new { isok = false, msg = "服务时长：0~999分钟之间" });
            }
            if (string.IsNullOrEmpty(serviceInfo.img))
            {
                return Json(new { isok = false, msg = "请上传项目主图" });
            }
            if (serviceInfo.stock < 0 || serviceInfo.stock > 999)
            {
                return Json(new { isok = false, msg = "已订人数范围：0~999" });
            }
            if (serviceInfo.specificationkeys != null && serviceInfo.specificationkeys.Length > 100)
            {
                return Json(new { isok = false, msg = "服务流程字数不能超过100字" });
            }
            bool isok = false;
            if (serviceInfo.id > 0)//编辑
            {
                EntGoods model = EntGoodsBLL.SingleModel.GetServiceById(serviceInfo.aid, serviceInfo.id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙info_null" });
                }
                serviceInfo.updatetime = DateTime.Now;
                isok = EntGoodsBLL.SingleModel.Update(serviceInfo, "updatetime,name,ptypes,price,ServiceTime,img,stock,specificationkeys,description");

            }
            else//添加
            {
                serviceInfo.addtime = serviceInfo.updatetime = DateTime.Now;
                serviceInfo.exttypes = ((int)GoodsType.足浴版服务).ToString();
                isok = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(serviceInfo)) > 0;
            }
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, msg = msg });
        }
        /// <summary>
        /// 上架\下架
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult UpdateTagOrDelete(EntGoods serviceInfo)
        {
            string action = Context.GetRequest("action", string.Empty);
            if (serviceInfo.aid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(serviceInfo.aid, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={serviceInfo.aid}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            EntGoods model = EntGoodsBLL.SingleModel.GetServiceById(serviceInfo.aid,serviceInfo.id);
            if (model == null)
            {
                return Json(new { isok = false, msg = "系统繁忙info_null" });
            }
            bool isok = false;
            model.updatetime = DateTime.Now;
            switch (action)
            {
                case "updateTag":
                    if (model.tag == 1)
                    {
                        model.tag = 0;
                    }
                    else
                    {
                        model.tag = 1;
                    }
                    isok = EntGoodsBLL.SingleModel.Update(model, "tag,updatetime");
                    break;

                case "delete":
                    model.state = 0;
                    isok = EntGoodsBLL.SingleModel.Update(model, "state,updatetime");
                    break;
                default: return Json(new { isok = false, msg = "参数错误" });

            }
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg });
        }


        /// <summary>
        /// 项目分类
        /// </summary>
        /// <returns></returns>
        public ActionResult ServiceType()
        {

            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            ViewBag.appId = appId;
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            }
            List<EntGoodType> goodTypeList = EntGoodTypeBLL.SingleModel.GetServiceItemList(appId, storeModel.Id, (int)GoodProjectType.足浴版服务项目分类);

            return View(goodTypeList);
        }

        /// <summary>
        /// 保存（添加|编辑的）分类
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveType()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            int id = Context.GetRequestInt("id", -1);
            if (id < 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" });
            }
            string name = Context.GetRequest("name", string.Empty);
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { isok = false, msg = "请填写分类名称" });
            }
            if (name.Length > 8)
            {
                return Json(new { isok = false, msg = "分类名称不能超过8字" });
            }
            int sort = Context.GetRequestInt("sort", 0);
            if (sort < 1 || sort > 99)
            {
                return Json(new { isok = false, msg = "排序请填写1~99之间的整数" });
            }
            if (EntGoodTypeBLL.SingleModel.ValidItemName(appId, storeModel.Id, (int)GoodProjectType.足浴版服务项目分类, id, name))
            {
                return Json(new { isok = false, msg = "该分类名称已存在！" });

            }
            EntGoodType typeInfo = null;
            bool isok = false;
            if (id > 0) //保存编辑
            {
                typeInfo = EntGoodTypeBLL.SingleModel.GetServiceItem(appId, storeModel.Id, (int)GoodProjectType.足浴版服务项目分类, id);
                if (typeInfo == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙info_null" });
                }
                typeInfo.name = name;
                typeInfo.sort = sort;
                isok = EntGoodTypeBLL.SingleModel.Update(typeInfo, "name,sort");
            }
            else //保存添加
            {
                typeInfo = new EntGoodType();
                typeInfo.aid = appId;
                typeInfo.storeId = storeModel.Id;
                typeInfo.name = name;
                typeInfo.sort = sort;
                typeInfo.type = (int)GoodProjectType.足浴版服务项目分类;
                typeInfo.state = 1;
                isok = Convert.ToInt32(EntGoodTypeBLL.SingleModel.Add(typeInfo)) > 0;
            }
            if (isok)
            {
                List<EntGoodType> goodTypeList = EntGoodTypeBLL.SingleModel.GetServiceItemList(appId,storeModel.Id,(int)GoodProjectType.足浴版服务项目分类);
                return Json(new { isok = isok, msg = "保存成功", list = goodTypeList });
            }
            else
            {
                return Json(new { isok = isok, msg = "保存失败" });
            }
        }

        /// <summary>
        /// 删除项目分类
        /// </summary>
        /// <returns></returns>
        public ActionResult DelType()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            int id = Context.GetRequestInt("id", -1);
            if (id < 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" });
            }
            EntGoodType typeInfo = EntGoodTypeBLL.SingleModel.GetServiceItem(appId, storeModel.Id, (int)GoodProjectType.足浴版服务项目分类, id);
            if (typeInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙info_null" });
            }
            //获取该分类下的项目数目
            int serviceCount = EntGoodsBLL.SingleModel.GetServiceCountById(appId,typeInfo.id);

            if (serviceCount > 0)
            {
                return Json(new { isok = false, msg = $"不可删除：该分类下已关联{serviceCount}个项目" });
            }




            typeInfo.state = 0;
            bool isok = EntGoodTypeBLL.SingleModel.Update(typeInfo, "state");
            string msg = isok ? "已删除" : "删除失败";
            return Json(new { isok = isok, msg = msg });
        }
        #endregion
    }
}