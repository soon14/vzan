using BLL.MiniApp;
using Entity.MiniApp.Footbath;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Entity.MiniApp.Ent;
using Entity.MiniApp;
using Entity.MiniApp.User;
using Utility.IO;
using DAL.Base;
using MySql.Data.MySqlClient;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.User;

namespace User.MiniApp.Controllers
{
    public partial class footbathController : configController
    {
        // GET: footbath_gift：足浴版小程序-送花管理
        #region 送花管理
        /// <summary>
        /// 送花配置界面
        /// </summary>
        /// <returns></returns>
        public ActionResult GiftSetting()
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
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有数据!", code = "500" });
            }


            ViewBag.GiftPrice = storeModel.ShowGiftPrice;
            SwitchModel switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            ViewBag.ShowPhotoByGift = switchModel.ShowPhotoByGift;
            
            List<EntGoods> giftPackages = EntGoodsBLL.SingleModel.GetGiftPackages(appId, (int)GoodsType.足浴版送花套餐);
            if (giftPackages == null || giftPackages.Count <= 0)
            {
                EntGoods giftPackage = new EntGoods()
                {
                    aid = appId,
                    exttypes = ((int)GoodsType.足浴版送花套餐).ToString(),
                    stock = 2,
                    name = "看相册",
                    state = 1,
                };
                giftPackage.id = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(giftPackage));
                giftPackages = new List<EntGoods>();
                giftPackages.Add(giftPackage);
            }
            return View(giftPackages);
        }
        /// <summary>
        /// 开启|关闭 送花看相册功能
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveGiftConfig()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" });
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙account_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" });
            }
            int isOpen = Context.GetRequestInt("isOpen", -1);
            if (isOpen < 0)
            {
                return Json(new { isok = false, msg = "系统繁忙isOpen_null" });
            }
            SwitchModel switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            switchModel.ShowPhotoByGift = isOpen > 0;
            storeModel.SwitchConfig = JsonConvert.SerializeObject(switchModel);
            storeModel.UpdateDate = DateTime.Now;
            bool isok = FootBathBLL.SingleModel.Update(storeModel, "switchconfig,updatedate");
            return Json(new { isok = isok });
        }
        /// <summary>
        /// 保存 花朵价格
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveGiftPrice()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" });
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙account_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" });
            }
            int GiftPrice = Context.GetRequestInt("GiftPrice", -1);
            if (GiftPrice < 0 || GiftPrice > 99900)
            {
                return Json(new { isok = false, msg = "价格范围：0~999，最多两位小数" });
            }
            storeModel.GiftPrice = GiftPrice;
            storeModel.UpdateDate = DateTime.Now;
            bool isok = FootBathBLL.SingleModel.Update(storeModel, "giftprice,updatedate");
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, msg = msg });
        }

        /// <summary>
        /// 添加编辑 礼物
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveGiftInfo()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" });
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙account_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" });
            }
            int id = Context.GetRequestInt("id", 0);
            string name = Context.GetRequest("name", string.Empty);
            int count = Context.GetRequestInt("stock", 0);
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { isok = false, msg = "请输入套餐名称" });
            }
            if (name.Length > 20)
            {
                return Json(new { isok = false, msg = "套餐名称不能超过5字" });
            }
            if (count < 1 || count > 999)
            {
                return Json(new { isok = false, msg = "鲜花数量范围:0~999之间" });
            }


            EntGoods giftInfo = EntGoodsBLL.SingleModel.GetGiftInfoByName(appId, (int)GoodsType.足浴版送花套餐, name);
            bool isok = false;
            if (giftInfo != null && giftInfo.state > 0 && id <= 0)
            {
                return Json(new { isok = false, msg = "该套餐名称已存在" });
            }
            if (giftInfo != null && giftInfo.state > 0 && id > 0 && giftInfo.id != id)
            {
                return Json(new { isok = false, msg = "该套餐名称已存在" });
            }

            //添加
            if (id <= 0)
            {
                giftInfo = new EntGoods()
                {
                    aid = appId,
                    exttypes = ((int)GoodsType.足浴版送花套餐).ToString(),
                    stock = count,
                    name = name,
                    state = 1,
                };
                giftInfo.id = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(giftInfo));
                isok = giftInfo.id > 0;

            }
            //$"aid ={appId} and state> 0 and exttypes='{(int)GoodsType.足浴版送花套餐}'";
            else
            {
                //编辑保存
                giftInfo = EntGoodsBLL.SingleModel.GetGiftInfo(appId,id, (int)GoodsType.足浴版送花套餐);
                if (giftInfo == null)
                {
                    return Json(new { isok = false, msg = "数据错误" });
                }
                giftInfo.stock = count;
                giftInfo.name = name;
                isok = EntGoodsBLL.SingleModel.Update(giftInfo, "name,stock");
            }
            string msg = isok ? "保存成功" : "保存失败";
            if (isok)
            {
                return Json(new { isok = isok, msg = msg, info = giftInfo });
            }
            else
            {
                return Json(new { isok = isok, msg = msg });
            }
        }
        /// <summary>
        /// 删除套餐
        /// </summary>
        /// <returns></returns>
        public ActionResult DelGift()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" });
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙account_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" });
            }
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" });
            }
            EntGoods giftInfo = EntGoodsBLL.SingleModel.GetGiftInfo(appId,id, (int)GoodsType.足浴版送花套餐);
            if (giftInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙giftInfo_null" });
            }
            giftInfo.state = 0;
            bool isok = EntGoodsBLL.SingleModel.Update(giftInfo, "state");
            string msg = isok ? "删除成功" : "删除失败";
            return Json(new { isok = isok, msg = msg });
        }

        /// <summary>
        /// 送花记录（视图）
        /// </summary>
        /// <returns></returns>
        public ActionResult GiftRecord()
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
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            }
            SwitchModel switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            ViewBag.ShowPhotoByGift = switchModel.ShowPhotoByGift;
            return View();
        }

        /// <summary>
        /// 获取送花记录
        /// </summary>
        /// <returns></returns>
       
        public ActionResult GetGiftList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" });
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙account_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" });
            }
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string name = Context.GetRequest("name", string.Empty);
            string jobNumber = Context.GetRequest("jobNumber", string.Empty);
            string createtime = Context.GetRequest("createtime", string.Empty);
            string nickName = Context.GetRequest("nickName", string.Empty);
            int sex = Context.GetRequestInt("sex", 0);
           
            List<FootbathGiftModel> giftList = new List<FootbathGiftModel>();

            List<TechnicianInfo> technicianList = TechnicianInfoBLL.SingleModel.GetTechnicianList(name, jobNumber, sex);
            if(technicianList==null || technicianList.Count <= 0)
            {
                return Json(new { isok = true, list= giftList, recordCount=0,sumPrice=0.00 });
            }
            List<C_UserInfo> userList = new List<C_UserInfo>();
            if (!string.IsNullOrEmpty(nickName))
            {

                userList = C_UserInfoBLL.SingleModel.GetUserListByNickName(nickName);

                if (userList == null || userList.Count <= 0)
                {
                    return Json(new { isok = true, list = giftList, recordCount = 0, sumPrice = 0.00 });
                }
            }

            int recordCount = 0;
            int sumPrice = 0;
            List<EntGoodsOrder> giftOrderList = EntGoodsOrderBLL.SingleModel.GetGiftRecord(storeModel.appId, (int)MiniAppEntOrderState.交易成功, (int)TmpType.小程序足浴模板, name, jobNumber, sex, createtime, technicianList,userList,pageSize, pageIndex, out recordCount,out sumPrice);
                
            if(giftOrderList!=null && giftOrderList.Count > 0)
            {
                string technicianIds = string.Join(",",giftOrderList.Select(s=>s.FuserId).Distinct());
                List<TechnicianInfo> technicianInfoList = TechnicianInfoBLL.SingleModel.GetListByIds(technicianIds);

                string userIds = string.Join(",",giftOrderList.Select(s=>s.UserId));
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                foreach(EntGoodsOrder orderInfo in giftOrderList)
                {
                    FootbathGiftModel gift = new FootbathGiftModel();
                    TechnicianInfo technician = technicianInfoList?.FirstOrDefault(f=>f.id == orderInfo.FuserId) ;
                    gift.name = technician.name;
                    gift.jobNumber = technician.jobNumber;
                    gift.sex = technician.sex;
                    gift.createTime = orderInfo.CreateDate;
                    C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id == orderInfo.UserId);
                    gift.nickName = userInfo.NickName;
                    gift.price = orderInfo.BuyPrice;
                    giftList.Add(gift);
                }
            }
            return Json(new { isok = true, list = giftList, recordCount = recordCount, sumPrice = (sumPrice*0.01).ToString("0.00")});

        }
        #endregion
    }
}