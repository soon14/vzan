using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Controllers;
using User.MiniApp.Areas.MultiStore.Filters;
using BLL.MiniApp.Footbath;
using BLL.MiniApp;
using Entity.MiniApp.Footbath;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Entity.MiniApp;
using Utility.IO;
using Entity.MiniApp.Ent;
using BLL.MiniApp.Ent;
using User.MiniApp.Model;
using Entity.MiniApp.User;
using DAL.Base;

namespace User.MiniApp.Areas.MultiStore.Controllers
{
    public class StoresManagerController : baseController
    {
        
        
        public StoresManagerController()
        {
            
            
        }

        // GET: MultiStore/StoreManager
        [LoginFilter]
        public ActionResult Index(int appId = 0, int storeId = 0)
        {
            if (appId <= 0)
                return View("ErrorMsg", new Return_Msg() { Msg = "非法操作!" });

            FootBath storeModel = new FootBath();
            if (storeId > 0)
            {
                //表示门店
                storeModel= FootBathBLL.SingleModel.GetModel(storeId);
            }
            else
            {
                //表示总店
                storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId} and HomeId=0");
            }
            
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
                        return View("ErrorMsg", new Return_Msg() { Msg = "系统繁忙Config_error!" });
                    }
                }
                else
                {
                    storeModel.switchModel = new SwitchModel();
                }
                //店铺Logo
                var LogoList = C_AttachmentBLL.SingleModel.GetListByCache(storeModel.Id,(int)AttachmentItemType.小程序多门店版门店logo); 
                if (LogoList != null && LogoList.Count > 0)
                {
                    ViewBag.Logo = new List<object>() { new { id = LogoList[0].id, url = LogoList[0].filepath } };
                }
                //门店图片
                //if (storePicList != null && storePicList.Count > 0)
                //{
                //    var storePicObjList = new List<object>();
                //    storePicList.ForEach(x =>
                //    {
                //        storePicObjList.Add(new { id = x.id, url = x.filepath });
                //    });
                //    ViewBag.storePicList = storePicObjList;
                //}

            }
            else
            {
                
                return View("ErrorMsg", new Return_Msg() { Msg = "请先开通多门店!" });
            }

            ViewBag.storeId = storeModel.Id;
            ViewBag.appId = appId;
            ViewBag.SetStoreMaterialPages = string.IsNullOrEmpty(storeModel.StoreMaterialPages);
            ViewBag.HaveRole = UserRoleBLL.SingleModel.HavingRole(dzuserId, RoleType.总店管理员,appId);


            #region 扫码绑定门店管理员

            var sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["qrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = true;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
            }
            Session["appId"] = storeModel.appId;
            Session["storeId"] = storeModel.Id; 
            #endregion

            return View(storeModel);
        }



        /// <summary>
        /// 保存门店信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [LoginFilter]
        [ValidateInput(false)]
        public ActionResult SaveStoreMateria(FootBath info)
        {


            if (info == null)
                return Json(new { isok = false, msg = "参数错误" });
            if (info.appId <= 0)
                return Json(new { isok = false, msg = "系统繁忙appId不能为空" });
            if (info.StoreName != null && info.StoreName.Length > 16)
                return Json(new { isok = false, msg = "名称不能超过16个字符" });
            if (info.Notice != null && info.Notice.Length > 200)
                return Json(new { isok = false, msg = "公告内容不能超过200字" });
            if (!Regex.IsMatch(info.TelePhone, @"^(\d{3,4}-)?\d{6,8}$") && !Regex.IsMatch(info.TelePhone, @"^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1})|(17[0-9]{1}))+\d{8})$"))
                return Json(new { isok = false, msg = "请填写正确的电话号码" });

            //店铺Logo
            string logo = Context.GetRequest("logo", string.Empty);
            //门店图片
            //  string storeImgs = Context.GetRequest("storeImgs", string.Empty);
         //   List<string> storeImgUrl = new List<string>();
            bool result = false;
            //  storeImgUrl = storeImgs.Split(',').ToList();
            //if (storeImgUrl.Count > 10)
            //    return Json(new { isok = false, msg = "门店照片不能超过10张" });
            if (info.Id > 0)
            {
                if(string.IsNullOrEmpty(info.StoreMaterialPages))
                    return Json(new { isok = false, msg = "请先到店铺装修页面进行装修" });
                var storeModel = FootBathBLL.SingleModel.GetModel(info.Id);
                if (storeModel == null)
                    return Json(new { isok = false, msg = "数据不存在!" });

                //表示更新
                info.UpdateDate = DateTime.Now;
                info.SwitchConfig = JsonConvert.SerializeObject(info.switchModel);

                result = FootBathBLL.SingleModel.Update(info);
            }
            else
            {
                ////表示新增
                //if (string.IsNullOrEmpty(logo))
                //    return Json(new { isok = false, msg = "请上传店铺Logo" });



                //info.TemplateType = (int)TmpType.小程序多门店模板;
                //info.SwitchConfig = JsonConvert.SerializeObject(info.switchModel);
                //int id = Convert.ToInt32(_storeMaterialBLL.Add(info));
                //result = id > 0;
                //info.Id = id;
                return Json(new { isok = false, msg = "非法操作！"}, JsonRequestBehavior.AllowGet);

            }



            if (result)
            {
                if (!string.IsNullOrEmpty(logo))
                {
                    //添加店铺Logo
                    C_AttachmentBLL.SingleModel.Add(new C_Attachment
                    {
                        itemId = info.Id,
                        createDate = DateTime.Now,
                        filepath = logo,
                        itemType = (int)AttachmentItemType.小程序多门店版门店logo,
                        thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(logo, 300, 300),
                        status = 0
                    });
                }

                //if (!string.IsNullOrEmpty(storeImgs))
                //{
                //    //插入门店照片
                //    foreach (var imgUrl in storeImgUrl)
                //    {
                //        C_AttachmentBLL.SingleModel.Add(new C_Attachment
                //        {
                //            itemId = info.Id,
                //            createDate = DateTime.Now,
                //            filepath = imgUrl,
                //            itemType = (int)AttachmentItemType.小程序多门店版门店图片,
                //            thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(imgUrl, 700, 400),
                //            status = 0
                //        });
                //    }
                //}
                return Json(new { isok = true, msg = "操作成功！", obj = info.Id }, JsonRequestBehavior.AllowGet);

            }



            return Json(new { isok = false, msg = "系统错误！" }, JsonRequestBehavior.AllowGet);
        }

        [LoginFilter]
        public ActionResult pageset(int appId = 0, int storeId=0 )
        {
            if (appId <= 0)
                return View("ErrorMsg", new Return_Msg() { Msg = "非法操作!" });

            FootBath model = new FootBath();
            if (storeId > 0)
            {
                //表示门店
                model = FootBathBLL.SingleModel.GetModel(storeId);
            }
            else
            {
                //表示总店
                model = FootBathBLL.SingleModel.GetModel($"appId={appId} and HomeId=0");
            }
            if (model == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "非法操作(请先开通多店铺)!" });
            ViewBag.accountid = dzuserId;
           
            ViewModel<EntSetting> vm = new ViewModel<EntSetting>();
            vm.PageType = 26;
            vm.DataModel = new EntSetting()
            {
                aid=model.appId,
                pages=model.StoreMaterialPages,
                storeId=model.Id,
                HomeId=model.HomeId
            };
            ViewBag.storeId = storeId;
            ViewBag.TypeIndex = 0;
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;
            if (souceFrom == "")//&& !WebSiteConfig.CustomerLoginId.Contains(dzaccount.LoginId)
            {
                return Redirect($"/config/functionlist?appId={appId}&type=26");
            }
            return View("../enterprise/pageset", vm);
        }

        [LoginFilter]
        
        public ActionResult TakeoutSetting(int appId = 0, int storeId = 0)
        {
            if (appId <= 0)
                return View("ErrorMsg", new Return_Msg() { Msg = "非法操作!" });

            FootBath storeModel = new FootBath();
            if (storeId > 0)
            {
                //表示门店
                storeModel = FootBathBLL.SingleModel.GetModel(storeId);
            }
            else
            {
                //表示总店
                storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId} and HomeId=0");
            }

            if (storeModel == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "非法操作(请先开通多店铺)!" });
            

                if (!string.IsNullOrEmpty(storeModel.SwitchConfig))
                {
                    try
                    {
                        storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);

                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteError(this.GetType(), ex);
                        return Content("系统繁忙Config_error");
                    }
                }
                else
                {
                    storeModel.switchModel = new SwitchModel();
                }


                if (!string.IsNullOrEmpty(storeModel.TakeOutWayConfig))
                {
                    try
                    {
                        storeModel.takeOutWayModel = JsonConvert.DeserializeObject<TakeOutWayModel>(storeModel.TakeOutWayConfig);
                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteError(this.GetType(), ex);
                        return Content("系统繁忙TakeOutWayConfig");
                    }
                }
                else
                {
                    storeModel.takeOutWayModel = new TakeOutWayModel();
                }



            ViewBag.appId = appId;
            ViewBag.isAuthorize = 0;
            ViewBag.storeId = storeId;
            return View(storeModel);
        }

        [LoginFilter]
        [ValidateInput(false)]
        public ActionResult SaveTakeoutSetting(FootBath takeOut)
        {
            FootBath storeModel = FootBathBLL.SingleModel.GetModel(takeOut.Id);

            if (storeModel == null)
                return Json(new { isok = false, msg = "数据不存在！" }, JsonRequestBehavior.AllowGet);

            if (takeOut.takeOutWayModel != null)
            {
                if (!Regex.IsMatch(takeOut.takeOutWayModel.cityService.TakeFright.ToString(), @"^\d+\.?\d*$"))
                    return Json(new { isok = false, msg = "请填写正确的配送费(同城配送)" });
                if (!Regex.IsMatch(takeOut.takeOutWayModel.cityService.TakeRange.ToString(), @"^\d+\.?\d*$"))
                    return Json(new { isok = false, msg = "请填写正确的配送范围(同城配送)" });
                if (!Regex.IsMatch(takeOut.takeOutWayModel.selfTake.TakeRange.ToString(), @"^\d+\.?\d*$"))
                    return Json(new { isok = false, msg = "请填写正确的自取范围(同城配送)" });
                if (!Regex.IsMatch(takeOut.takeOutWayModel.cityService.TakeStartPrice.ToString(), @"^\d+\.?\d*$"))
                    return Json(new { isok = false, msg = "请填写正确的起送价格(同城配送)" });
                if (!Regex.IsMatch(takeOut.takeOutWayModel.cityService.FreeFrightCost.ToString(), @"^\d+\.?\d*$"))
                    return Json(new { isok = false, msg = "请填写正确的消费免邮金额(同城配送)" });
                if (storeModel.HomeId == 0)
                {
                    if (!Regex.IsMatch(takeOut.takeOutWayModel.GetExpressdelivery.TakeFright.ToString(), @"^\d+\.?\d*$"))
                        return Json(new { isok = false, msg = "请填写正确的统一配送费(快递配送)" });
                    if (!Regex.IsMatch(takeOut.takeOutWayModel.GetExpressdelivery.FreeFrightCost.ToString(), @"^\d+\.?\d*$"))
                        return Json(new { isok = false, msg = "请填写正确的消费免邮金额(同城配送)" });
                }


                storeModel.TakeOutWayConfig = JsonConvert.SerializeObject(takeOut.takeOutWayModel);
                if (FootBathBLL.SingleModel.Update(storeModel, "TakeOutWayConfig"))
                    return Json(new { isok = true, msg = "操作成功！" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { isok = false, msg = "操作失败！" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = false, msg = "配送设置异常！" }, JsonRequestBehavior.AllowGet);
            }

        }

        [LoginFilter]
        /// <summary>
        /// 门店列表 只有总店进入才能看到该列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult StoreList(int appId)
        {
            ViewBag.appId = appId;
            FootBath model = FootBathBLL.SingleModel.GetModel($"appId={appId} and HomeId=0");
            if (model == null)
                return View("ErrorMsg", new Return_Msg() { Msg = "请先开通多门店!" });

            ViewBag.HomeId = model.Id;//用于查询该总店下的所有门店
            ViewBag.storeId = model.Id;
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (xcx != null)
            {
                var paycenter = PayCenterSettingBLL.SingleModel.GetPayCenterSettingByappid(xcx.AppId);
                if (paycenter != null)
                    ViewBag.Mch_id = paycenter.Mch_id;
            }
            

            return View();
        }
        [LoginFilter]
        /// <summary>
        /// 获取门店列表 根据总店appId 以及总店Id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="HomeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetStoreList(int appId = 0, int HomeId = 0, int pageIndex = 1, int pageSize = 10)
        {
            string strWhere = $"appId={appId} and HomeId={HomeId} and IsDel<>-1";
            List<FootBath> list = FootBathBLL.SingleModel.GetList(strWhere, pageSize, pageIndex);
            int TotalCount = FootBathBLL.SingleModel.GetCount(strWhere);
            return Json(new { isok = true, msg = "成功", model = new { RecordCount = TotalCount, StoreList = list } }, JsonRequestBehavior.AllowGet);
        }



    }
}