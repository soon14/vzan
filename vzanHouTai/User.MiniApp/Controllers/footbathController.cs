using BLL.MiniApp;
using Entity.MiniApp.Footbath;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Entity.MiniApp.Ent;
using BLL.MiniApp.Ent;
using Entity.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.User;
using Utility.IO;
using MySql.Data.MySqlClient;
using Entity.MiniApp.Conf;

namespace User.MiniApp.Controllers
{
    public partial class footbathController : configController
    {
        
        
        
        
        

        
        

        public footbathController()
        {
            

            
        }


        #region 店铺管理
        /// <summary>
        /// 门店信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
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
            if (storeModel != null)
            {
                if (!string.IsNullOrEmpty(storeModel.SwitchConfig))
                {
                    try
                    {
                        storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteError(this.GetType(), ex);
                        return View("PageError", new Return_Msg() { Msg = "系统繁忙!", code = "500" });
                    }
                }
                else
                {
                    storeModel.switchModel = new SwitchModel();
                }
                //店铺Logo
                List<C_Attachment> LogoList = C_AttachmentBLL.SingleModel.GetListByCache(storeModel.Id, (int)AttachmentItemType.小程序足浴版店铺logo);
                if (LogoList != null && LogoList.Count > 0)
                {
                    ViewBag.Logo = new List<object>() { new { id = LogoList[0].id, filepath = LogoList[0].filepath } };
                }
                //门店图片
                List<C_Attachment> storePicList = C_AttachmentBLL.SingleModel.GetListByCache(storeModel.Id, (int)AttachmentItemType.小程序足浴版门店图片);
                if (storePicList != null && storePicList.Count > 0)
                {
                    List<object> storePicObjList = new List<object>();
                    storePicList.ForEach(x =>
                    {
                        storePicObjList.Add(new { id = x.id, filepath = x.filepath });
                    });
                    ViewBag.storePicList = storePicObjList;
                }

            }
            else
            {
                storeModel = new FootBath();
                storeModel.GiftPrice = 200;//默认一朵花2块钱
                storeModel.switchModel = new SwitchModel();
                storeModel.SwitchConfig = JsonConvert.SerializeObject(storeModel.switchModel);
                storeModel.appId = appId;
                int Id = Convert.ToInt32(FootBathBLL.SingleModel.Add(storeModel));
                if (Id > 0)
                    storeModel.Id = Id;
                else
                    return View("PageError", new Return_Msg() { Msg = "后台初始化异常!", code = "500" });
            }
            //小程序配置
            ViewBag.isAuthorize = 0;
            ViewBag.XcxName = "";
            List<ConfParam> paramslist = ConfParamBLL.SingleModel.GetListByRId(Convert.ToInt32(appId));
            if (appAcountRelation != null && !string.IsNullOrWhiteSpace(appAcountRelation.AppId))
            {
                ViewBag.isAuthorize = 1;
                if (paramslist != null && paramslist.Count > 0)
                {
                    ConfParam cinfo = paramslist.Where(w => w.Param == "nparam").FirstOrDefault();
                    if (cinfo != null)
                    {
                        ViewBag.XcxName = cinfo.Value;
                    }
                }
            }


            return View(storeModel);
        }
        /// <summary>
        /// 保存门店信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ActionResult SaveIndex(List<C_Attachment> logo, List<C_Attachment> storeImgs, FootBath info)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }

            if (info == null)
            {
                return Json(new { isok = false, msg = "参数错误" });
            }

            if (info.appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" });
            }

            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(info.appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }

            if (info.Notice != null && info.Notice.Length > 200)
            {
                return Json(new { isok = false, msg = "公告内容不能超过200字" });
            }

            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={info.appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }

            //店铺Logo
            if (logo == null || logo.Count <= 0)
            {
                return Json(new { isok = false, msg = "请选择店铺logo" });
            }
            //等于0说明有修改过
            if (logo[0].id == 0)
            {
                List<C_Attachment> beforLogoList = C_AttachmentBLL.SingleModel.GetListByCache(storeModel.Id, (int)AttachmentItemType.小程序足浴版店铺logo);
                logo[0].itemId = storeModel.Id;
                logo[0].createDate = DateTime.Now;
                logo[0].itemType = (int)AttachmentItemType.小程序足浴版店铺logo;
                logo[0].thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(logo[0].filepath, 300, 300);
                logo[0].status = 0;
                //添加成功删除旧照片
                if (Convert.ToInt32(C_AttachmentBLL.SingleModel.Add(logo[0])) > 0 && beforLogoList != null && beforLogoList.Count > 0)
                {
                    string ids = string.Join(",", beforLogoList.Select(l => l.id));
                    if (!string.IsNullOrEmpty(ids))
                    {
                        //C_AttachmentBLL.SingleModel.Delete($"id in ('{ids}')");
                        string[] idStrs = ids.Split(',');
                        if (idStrs != null && idStrs.Any())
                        {

                            foreach (string curId in idStrs)
                            {
                                C_AttachmentBLL.SingleModel.Delete(Convert.ToInt32(curId));
                            }
                        }
                    }
                }
            }

            ////门店照片
            string photoIds = storeImgs != null ? string.Join(",", storeImgs.Where(s => s.id > 0).Select(s => s.id)) : string.Empty;
            List<C_Attachment> beforeStorePicList = C_AttachmentBLL.SingleModel.GetFootbathStorePic(storeModel.Id, (int)AttachmentItemType.小程序足浴版门店图片, photoIds);

            photoIds = string.Join(",", beforeStorePicList.Select(l => l.id));
            if (!string.IsNullOrEmpty(photoIds))
            {
                string[] photoIdStrs = photoIds.Split(',');
                if (photoIdStrs != null && photoIdStrs.Any())
                {
                    foreach (string curId in photoIdStrs)
                    {
                        C_AttachmentBLL.SingleModel.Delete(Convert.ToInt32(curId));
                    }
                }

            }
            if (storeImgs != null && storeImgs.Count > 0)
            {
                foreach (C_Attachment img in storeImgs)
                {
                    if (img.id == 0)
                    {
                        img.itemId = storeModel.Id;
                        img.createDate = DateTime.Now;
                        img.itemType = (int)AttachmentItemType.小程序足浴版门店图片;
                        img.thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(img.filepath, 300, 300);
                        img.status = 0;
                        C_AttachmentBLL.SingleModel.Add(img);
                    }
                }
            }

            //门店信息修改
            storeModel.GiftPrice = 200;//默认一朵花2块钱
            storeModel.ShopTime = info.ShopTime;
            storeModel.StoreName = info.StoreName;
            storeModel.Address = info.Address;
            storeModel.SwitchConfig = JsonConvert.SerializeObject(info.switchModel);
            storeModel.TelePhone = info.TelePhone;
            storeModel.Notice = info.Notice;
            storeModel.Lat = info.Lat;
            storeModel.Lng = info.Lng;
            storeModel.GiftPrice = 200;//鲜花价格固定2块钱
            storeModel.UpdateDate = DateTime.Now;
            bool isok = FootBathBLL.SingleModel.Update(storeModel, "ShopTime,StoreName,Address,Lng,Lat,SwitchConfig,TelePhone,Notice,UpdateDate,GiftPrice");
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, msg = msg });
        }

        /// <summary>
        /// 支付方式
        /// </summary>
        /// <returns></returns>
        public ActionResult PayWaySetting()
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
            if (!string.IsNullOrEmpty(storeModel.SwitchConfig))
            {
                try
                {
                    storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteError(this.GetType(), ex);
                    return View("PageError", new Return_Msg() { Msg = "系统繁忙!", code = "500" });
                }
            }
            else
            {
                storeModel.switchModel = new SwitchModel();
            }

            return View(storeModel);
        }
        /// <summary>
        /// 保存支付方式
        /// </summary>
        /// <returns></returns>
        public ActionResult SavePayWay(SwitchModel paySetting)
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
            if (paySetting == null)
            {
                return Json(new { isok = false, msg = "系统繁忙switch_null" });
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
            storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            storeModel.switchModel.Alipay = paySetting.Alipay;
            storeModel.switchModel.BankCardPay = paySetting.BankCardPay;
            storeModel.switchModel.CashPay = paySetting.CashPay;
            storeModel.switchModel.OtherPay = paySetting.OtherPay;
            storeModel.switchModel.SaveMoneyPay = paySetting.SaveMoneyPay;
            storeModel.switchModel.WeChatPay = paySetting.WeChatPay;
            storeModel.SwitchConfig = JsonConvert.SerializeObject(storeModel.switchModel);
            storeModel.UpdateDate = DateTime.Now;
            bool isok = FootBathBLL.SingleModel.Update(storeModel, "switchconfig,updatedate");
            string msg = isok ? "修改成功" : "修改失败";
            return Json(new { isok = isok, msg = msg });
        }
        #endregion


        #region 包间管理
        /// <summary>
        /// 包间管理界面
        /// </summary>
        /// <returns></returns>
        public ActionResult PrivateRoom()
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
            return View();
        }
        /// <summary>
        /// 获取包间列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRoomList()
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
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" });
            }
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 8);

            int recordCount = 0;
            List<EntGoodType> roomList = EntGoodTypeBLL.SingleModel.GetRoomList(appId, storeModel.Id, (int)GoodProjectType.足浴版包间分类, pageSize, pageIndex, out recordCount);

            return Json(new { isok = true, roomList = roomList, recordCount = recordCount });
        }

        /// <summary>
        /// 添加编辑包间信息
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveRoomInfo()
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
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙store_null" });
            }
            int id = Context.GetRequestInt("id", 0);
            string name = Context.GetRequest("name", string.Empty);
            int count = Context.GetRequestInt("count", 0);
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { isok = false, msg = "请输入包间名称" });
            }
            if (name.Length > 20)
            {
                return Json(new { isok = false, msg = "包间名称不能超过20字" });
            }
            if (count < 1 || count > 999)
            {
                return Json(new { isok = false, msg = "容纳人数:请输入0~999之间的整数" });
            }

            EntGoodType roomInfo = EntGoodTypeBLL.SingleModel.GetModelByName(appId, storeModel.Id, (int)GoodProjectType.足浴版包间分类, name);
            bool isok = false;
            if (roomInfo != null && roomInfo.state > 0 && id <= 0)
            {
                return Json(new { isok = false, msg = "该包间名称已存在" });
            }
            if (roomInfo != null && roomInfo.state > 0 && id > 0 && roomInfo.id != id)
            {
                return Json(new { isok = false, msg = "该包间名称已存在" });
            }
            //添加
            if (id <= 0)
            {
                roomInfo = new EntGoodType()
                {
                    aid = appId,
                    type = (int)GoodProjectType.足浴版包间分类,
                    name = name,
                    count = count,
                    state = 1,
                };
                isok = Convert.ToInt32(EntGoodTypeBLL.SingleModel.Add(roomInfo)) > 0;
            }
            else
            {
                //编辑保存
                roomInfo = EntGoodTypeBLL.SingleModel.GetModel(id);
                if (roomInfo == null||roomInfo.type!= (int)GoodProjectType.足浴版包间分类|| roomInfo.state<=0 || roomInfo.aid!=appId)
                {
                    return Json(new { isok = false, msg = "数据错误" });
                }
                roomInfo.count = count;
                roomInfo.name = name;
                isok = EntGoodTypeBLL.SingleModel.Update(roomInfo, "name,count");
            }
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, msg = msg });
        }
        #endregion


    }
}