//#region 引用命名空间
//using Api.MiniApp.Controllers;
//using Api.MiniApp.Models;
//using BLL.MiniApp;
//using BLL.MiniApp.Conf;
//using BLL.MiniApp.Fds;
//using Core.MiniApp;
//using DAL.Base;
//using Entity.MiniApp;
//using Entity.MiniApp.Conf;
//using Entity.MiniApp.Fds;
//using Entity.MiniApp.User;
//using MySql.Data.MySqlClient;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Web.Mvc;
//using Utility.AliOss;
//using Utility;

//#endregion

//namespace Api.MiniSNS.Controllers
//{
//    [ExceptionLog]
//    public class apiMiappFoodsController : InheritController
//    {
//        private readonly FoodGoodsTypeBLL _miniappFoodGoodsTypeBll;
//        private readonly FoodBLL _miniappFoodBll;
//        private readonly FoodGoodsCartBLL _miniappFoodGoodsCartBll;
//        private readonly FoodAddressBLL _miniappaddressBll;
//        private readonly FoodLabelBLL _miniappFoodLabelBll;
//        private readonly FoodGoodsOrderBLL _miniappFoodGoodsOrderBll;
//        private readonly FoodGoodsOrderLogBLL _miniappFoodGoodsOrderLogBll;
//        private readonly FoodOrderPrintLogBLL _miniappFoodOrderPrintLogBll;
//        private readonly FoodTableBLL _miniappFoodTableBll;
//        private readonly TemplateMsgBLL _miniapptemplatemsgBll;
//        private readonly TemplateMsg_UserBLL _miniapptemplatemsg_userBll;
//        private readonly TemplateMsg_UserLogBLL _miniapptemplatemsg_userlogBll;
//        private readonly MsnModelHelper _msnModelHelper;
//        private readonly TemplateMsg_UserParamBLL _miniapptemplatemsg_userParamBll;
//        private readonly static object _couponLock = new object();
//        private readonly FoodFreightTemplateBLL _miniappFoodFreighTemplateBll;
//        private readonly FoodGoodsAttrBLL _miniappFoodGoodsAttrBll;
//        private readonly FoodGoodsAttrSpecBLL _miniappFoodGoodsAttrSpecBll;
//        private readonly FoodGoodsBLL _miniappFoodGoodsBll;
//        private readonly FoodGoodsSpecBLL _miniappFoodGoodsSpecBll;



//        public static readonly ConcurrentDictionary<int, object> lockObjectDict_Order = new ConcurrentDictionary<int, object>();
//        public apiMiappFoodsController()
//        {
//            _miniappFoodFreighTemplateBll = new FoodFreightTemplateBLL();
//            _miniappFoodGoodsAttrBll = new FoodGoodsAttrBLL();
//            _miniappFoodGoodsAttrSpecBll = new FoodGoodsAttrSpecBLL();
//            _miniappFoodGoodsBll = new FoodGoodsBLL();
//            _miniappFoodGoodsSpecBll = new FoodGoodsSpecBLL();
//            _miniappFoodGoodsTypeBll = new FoodGoodsTypeBLL();
//            _miniappFoodBll = new FoodBLL();
//            _miniappFoodGoodsCartBll = new FoodGoodsCartBLL();
//            _miniappaddressBll = new FoodAddressBLL();
//            _miniappFoodLabelBll = new FoodLabelBLL();
//            _miniappFoodGoodsOrderLogBll = new FoodGoodsOrderLogBLL();
//            _miniappFoodOrderPrintLogBll = new FoodOrderPrintLogBLL();
//            _miniappFoodGoodsOrderBll = new FoodGoodsOrderBLL();
//            _miniappFoodTableBll = new FoodTableBLL();

//            _msnModelHelper = new MsnModelHelper();
//            _miniapptemplatemsgBll = new TemplateMsgBLL();
//            _miniapptemplatemsg_userBll = new TemplateMsg_UserBLL();
//            _miniapptemplatemsg_userlogBll = new TemplateMsg_UserLogBLL();
//            _miniapptemplatemsg_userParamBll = new TemplateMsg_UserParamBLL();
//        }
        
//        #region 图片上传、删除
//        [HttpPost]
//        [ExceptionLog]
//        public ActionResult uploadImageFromPost(int index = 0, bool isSave = false)
//        {
//            //var mediaId = Guid.NewGuid().ToString();
//            if (Request.Files.Count == 0) { return Json(new BaseResult() { result = false, msg = "请选择一张图片" }, JsonRequestBehavior.AllowGet); }
//            using (Stream stream = Request.Files[0].InputStream)
//            {
//                string outext = "jpg";
//                byte[] imgByteArray = new byte[stream.Length];
//                stream.Read(imgByteArray, 0, imgByteArray.Length);
//                // 设置当前流的位置为流的开始
//                stream.Seek(0, SeekOrigin.Begin);
//                //开始上传图片
//                string aliTempImgKey = string.Empty;
//                var aliTempImgFolder = AliOSSHelper.GetOssImgKey(outext, false, out aliTempImgKey);
//                var putResult = AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, imgByteArray, 1, "." + outext);
//                if (!putResult)
//                {
//                    log4net.LogHelper.WriteInfo(this.GetType(), "图片上传失败！图片同步到Ali失败！mediaId");
//                    return Json(new { result = false, msg = "图片上传失败！图片同步到Ali失败！" }, JsonRequestBehavior.AllowGet);
//                }
//                var thumpath = aliTempImgKey;
//                Attachment model = new Attachment()
//                {
//                    postfix = "." + outext,
//                    filepath = aliTempImgKey,
//                    thumbnail = thumpath
//                };
//                var imgId = 0;
//                if (isSave)
//                {
//                    isSave = int.TryParse(new AttachmentBll().Add(model).ToString(), out imgId);
//                }
//                return Json(new { mediaId = imgId, path = aliTempImgKey, thumpath = thumpath, isSuccessSave = isSave, index = index }, JsonRequestBehavior.AllowGet);
//            }
//        }
//        [ExceptionLog]
//        public JsonResult DeleteStoreImage(int imageId, string openId)
//        {
//            C_UserInfo loginCUser = _userInfoBll.GetModelFromCache(openId);
//            if (loginCUser == null)
//            {
//                return Json(new BaseResult { errcode = 1, result = false, msg = "登录信息过期，刷新试试" }, JsonRequestBehavior.AllowGet);
//            }
//            C_Attachment catt = _attachmentBll.GetModel(imageId);
//            if (catt == null)
//            {
//                return Json(new BaseResult { errcode = 1, result = false, msg = "该图片已不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            bool auth = false;
//            if (_attachmentBll.Delete(catt.id) > 0)
//            {
//                _attachmentBll.RemoveRedis(catt.itemId, catt.itemType);//清除缓存
//                return Json(new BaseResult { errcode = 1, result = true, msg = "删除成功" }, JsonRequestBehavior.AllowGet);
//            }
//            return Json(new BaseResult { errcode = 1, result = false, msg = "系统繁忙db_err" }, JsonRequestBehavior.AllowGet);
//        }
//        #endregion

 

//        /// <summary>
//        /// 获取店铺详情信息接口
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult GetFoodsDetail(string appid)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
//            }
//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
//            }

//            //店铺Logo
//            string Logo = string.Empty;
//            var LogoList = _attachmentBll.GetList("itemid=" + miniappFood.Id + " and itemtype=" + (int)AttachmentItemType.小程序餐饮店铺Logo);
//            if (LogoList != null && LogoList.Count > 0)
//            {
//                Logo = LogoList[0].filepath;
//            }
//            //门店照片
//            var storeImgs = _attachmentBll.GetList($"itemid={ miniappFood.Id} and itemtype={(int)AttachmentItemType.小程序餐饮门店图片}");
//            List<string> storeImgUrls = null;
//            if (storeImgs != null && storeImgs.Count > 0)
//            {
//                storeImgUrls = new List<string>();
//                storeImgs.ForEach(s =>
//                {
//                    storeImgUrls.Add(s.filepath);
//                });
//            }
//            //轮播图
//            var sliderImgs = _attachmentBll.GetList($"itemid={ miniappFood.Id} and itemtype={(int)AttachmentItemType.小程序餐饮店铺轮播图}");
//            List<string> sliderImgUrls = null;
//            if (sliderImgs != null && sliderImgs.Count > 0)
//            {
//                sliderImgUrls = new List<string>();
//                sliderImgs.ForEach(s =>
//                {
//                    sliderImgUrls.Add(s.filepath);
//                });
//            }

//            var postdata = new { Logo = Logo, food = miniappFood, storeImgs = storeImgUrls, sliderImgUrls = sliderImgUrls };

//            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//        }


//        /// <summary>
//        /// 获取菜品列表接口 根据条件
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="typeid">菜品类别</param>
//        /// <param name="goodsName">菜品名称</param>
//        /// <param name="shopType">购买方式(0点餐/1外卖)</param>
//        /// <param name="pageindex">当前页</param>
//        /// <param name="pagesize">每页数量</param>
//        /// <param name="orderbyid">排序</param>
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult GetGoodsList(string appid, int typeid = 0, string goodsName = "", int shopType = 10, int pageindex = 1, int pagesize = 10, int orderbyid = 0)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }

//            var goodslist = _miniappFoodGoodsBll.GetListByParam(miniappFood.Id, typeid, goodsName, pageindex, pagesize, orderbyid, shopType) ?? new List<FoodGoods>();
//            goodslist.ForEach(x =>
//            {
//                if (!string.IsNullOrWhiteSpace(x.labelIdStr))
//                {
//                    var labelIds = x.labelIdStr.Split(',').Where(y => !string.IsNullOrWhiteSpace(y));
//                    if (labelIds != null && labelIds.Any())
//                    {
//                        var labelNames = _miniappFoodLabelBll.GetList($" Id in ({string.Join(",", labelIds)}) ") ?? new List<FoodLabel>();
//                        if (labelNames.Any()) { x.labelNameStr = string.Join(",", labelNames.Select(y => y.LabelName)); }
//                    }
//                }

//            });

//            #region 会员打折
//            int userLevelId = Utility.IO.Context.GetRequestInt("levelid", 0);
//            VipLevel userLevel = _miniappVipLevelBll.GetModel($"id={userLevelId} and state>=0");

//            _miniappVipLevelBll.CalculateVipGoodsPrice(goodslist, userLevel, true);
//            #endregion

//            var dicAttrSpace = new Dictionary<int, List<FoodGoodsAttr>>();
//            if (goodslist.Any())
//            {
//                var goodAttrSpaceList = _miniappFoodGoodsAttrSpecBll.GetList($" FoodGoodsId in ({string.Join(",", goodslist.Select(x => x.Id))}) ") ?? new List<FoodGoodsAttrSpec>();
//                if (goodAttrSpaceList.Any())
//                {
//                    goodslist.Where(x => !string.IsNullOrWhiteSpace(x.AttrDetail)).ToList().ForEach(x =>
//                    {
//                        var goodsAttrList = _miniappFoodGoodsAttrBll.GetList($" Id in ({string.Join(",", goodAttrSpaceList.Where(y => y.FoodGoodsId == x.Id).Select(y => y.AttrId))}) ").OrderBy(y => x.Id).ToList();
//                        var goodsAttrSpacList = _miniappFoodGoodsSpecBll.GetList($" Id in ({string.Join(",", goodAttrSpaceList.Where(y => y.FoodGoodsId == x.Id).Select(y => y.SpecId))}) ").OrderBy(y => x.Id).ToList();

//                        goodsAttrList.ForEach(y => { y.SpecList = goodsAttrSpacList.Where(z => z.AttrId == y.Id).ToList(); });

//                        dicAttrSpace.Add(x.Id, goodsAttrList);
//                    });
//                }


//            }

//            var postdata = new
//            {
//                goodslist = goodslist?.Select(x => new { good = x, labels = x.labelNameStr?.Split(','), attrList = dicAttrSpace?.Where(y => y.Key == x.Id) }),

//            };
//            //log4net.LogHelper.WriteInfo(GetType(), "ccc");
//            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//        }

//        /// <summary>
//        /// 获取店铺类别列表
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult GetGoodsTypeList(string appid)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }

//            var goodsTypeList = _miniappFoodGoodsTypeBll.GetlistByFoodId(miniappFood.Id);

//            var postdata = new
//            {
//                goodsTypeList = goodsTypeList,

//            };
//            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//        }


//        /// <summary>
//        /// 获取菜品列表接口 根据条件
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="typeid">菜品类别</param>
//        /// <param name="goodsName">菜品名称</param>
//        /// <param name="shopType">购买方式(0点餐/1外卖)</param>
//        /// <param name="pageindex">当前页</param>
//        /// <param name="pagesize">每页数量</param>
//        /// <param name="orderbyid">排序</param>
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult GetGoodsDtl(string appid, int goodsid)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }
//            var miniappFoodGoods = _miniappFoodGoodsBll.GetModel(goodsid);
//            var labelIds = miniappFoodGoods.labelIdStr.Split(',').Where(y => !string.IsNullOrWhiteSpace(y));
//            if (labelIds != null && labelIds.Any())
//            {
//                var labelNames = _miniappFoodLabelBll.GetList($" Id in ({string.Join(",", labelIds)}) ");
//                miniappFoodGoods.labelNameStr = string.Join(",", labelNames.Select(y => y.LabelName));
//            }
//            if (miniappFoodGoods == null)
//            {
//                return Json(new { isok = -1, msg = "菜品信息有误" }, JsonRequestBehavior.AllowGet);
//            }

//            #region 会员打折
//            int userLevelId = Utility.IO.Context.GetRequestInt("levelid", 0);
//            VipLevel userLevel = _miniappVipLevelBll.GetModel($"id={userLevelId} and state>=0");

//            _miniappVipLevelBll.CalculateVipGoodPrice(miniappFoodGoods, userLevel);
//            #endregion

//            //规格 / 属性 名称
//            var attrSpecList = _miniappFoodGoodsAttrSpecBll.GetList($" FoodGoodsId = {goodsid} ");
//            var goodsAttrList = new List<FoodGoodsAttr>();
//            var goodsAttrSpacList = new List<FoodGoodsSpec>();
//            if (attrSpecList != null && attrSpecList.Any())
//            {
//                goodsAttrList = _miniappFoodGoodsAttrBll.GetList($" Id in ({string.Join(",", attrSpecList.Distinct().Select(x => x.AttrId))}) ").OrderBy(x => x.Id).ToList();
//                goodsAttrSpacList = _miniappFoodGoodsSpecBll.GetList($" Id in ({string.Join(",", attrSpecList.Distinct().Select(x => x.SpecId))}) ").OrderBy(x => x.Id).ToList();
//                goodsAttrList.ForEach(x =>
//                {
//                    x.SpecList = goodsAttrSpacList.Where(y => x.Id == y.AttrId).ToList();
//                });
//            }

//            var postdata = new
//            {
//                miniappFoodGoods = miniappFoodGoods,
//                goodsAttrList = goodsAttrList,
//                goodsAttrSpacList = goodsAttrSpacList
//            };
//            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//        }

//        #region 收货地址相关接口

//        /// <summary>
//        /// 根据代码获取省市区 默认 北京市
//        /// </summary>
//        /// <param name="parentCode"></param>
//        /// <param name="currentCode"></param>
//        /// <returns></returns>
//        public ActionResult GetRegionJsonList(int parentCode = 110000, int currentCode = 0)
//        {
//            string sql = "SELECT * from c_area where parentCode = " + parentCode;
//            var regionList = _areaBll.GetListBySql(sql).Select(m => new
//            {
//                Value = m.Code,
//                Text = m.Name,
//                Selected = m.Code == currentCode
//            });
//            return Json(regionList, JsonRequestBehavior.AllowGet);
//        }


//        /// <summary>
//        ///  添加/编辑收货地址
//        /// </summary>
//        /// <param name="State"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult AddOrEditMyAddressDefault(string appid, string openid, string addressjson)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }
//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }
//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }

//            log4net.LogHelper.WriteInfo(GetType(), addressjson);
//            var address = JsonConvert.DeserializeObject<FoodAddress>(addressjson);
//            if (address == null)
//            {
//                return Json(new { isok = -1, msg = "地址不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            if (address.Lat <= 0 || address.Lng <= 0)
//            {
//                return Json(new { isok = -1, msg = "定位坐标有误,请重试！" }, JsonRequestBehavior.AllowGet);
//            }
//            if (string.IsNullOrWhiteSpace(address.TelePhone))
//            {
//                return Json(new { isok = -1, msg = "联系人号码不能为空！" }, JsonRequestBehavior.AllowGet);
//            }
//            //if (!Regex.IsMatch(address.TelePhone, @"^[1]+[3-9]+\d{9}$"))
//            //{
//            //    return Json(new { isok = -1, msg = "联系人号码有误！" }, JsonRequestBehavior.AllowGet);
//            //}

//            if (string.IsNullOrWhiteSpace(address.Address))
//            {
//                return Json(new { isok = -1, msg = "请填写详细地址" }, JsonRequestBehavior.AllowGet);
//            }
//            log4net.LogHelper.WriteInfo(GetType(), _miniappaddressBll.BuildAddSql(address));
//            if (address.Id <= 0)
//            {
//                address.FoodId = miniappFood.Id;
//                address.UserId = userInfo.Id;
//                address.CreateDate = DateTime.Now;

//                address.Id = Convert.ToInt32(_miniappaddressBll.Add(address));
//                log4net.LogHelper.WriteInfo(GetType(), _miniappaddressBll.BuildAddSql(address));
//                if (address.Id <= 0)
//                {
//                    return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
//                }
//            }
//            else
//            {
//                var model = _miniappaddressBll.GetModel(address.Id);
//                if (model == null)
//                {
//                    return Json(new { isok = -1, msg = "保存失败" }, JsonRequestBehavior.AllowGet);
//                }
//                model.NickName = address.NickName;
//                model.TelePhone = address.TelePhone;
//                model.Province = address.Province;
//                model.CityCode = address.CityCode;
//                model.AreaCode = address.AreaCode;
//                model.Address = address.Address;
//                model.Lat = address.Lat;
//                model.Lng = address.Lng;
//                if (!_miniappaddressBll.Update(model, "NickName,TelePhone,Province,CityCode,AreaCode,Address,Lat,Lng"))
//                {
//                    return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
//                }
//            }

//            return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//        }

//        /// <summary>
//        ///  获取我的收货地址
//        /// </summary>
//        /// isDefault : 0为否,1 为是
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult GetMyAddress(string appid, string openid, int addressId = 0, int isDefault = 0)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }
//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }
//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }

//            //返回默认地址
//            if (isDefault == 1)
//            {
//                var address = _miniappaddressBll.GetModel($" FoodId = {miniappFood.Id} and UserId = {userInfo.Id} and State = 0 and IsDefault = 1 ");
//                if (address == null)
//                {
//                    return Json(new { isok = -1, msg = "地址信息不存在" }, JsonRequestBehavior.AllowGet);
//                }
//                //拼接地址名称
//                var provinceName = address.Province;
//                var cityName = address.CityCode;
//                var areaName = address.AreaCode;

//                address.Address_Dtl = $"{provinceName} {cityName} {areaName} {address.Address}";

//                var postdata = new { address = address };
//                return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//            }


//            if (addressId == 0)
//            {
//                var addressList = _miniappaddressBll.GetList($" FoodId = {miniappFood.Id} and UserId = {userInfo.Id} and State = 0 ");
//                //拼接地址名称
//                addressList.ForEach(x =>
//                {
//                    var provinceName = x.Province;
//                    var cityName = x.CityCode;
//                    var areaName = x.AreaCode;

//                    x.Address_Dtl = $"{provinceName} {cityName} {areaName} {x.Address}";
//                });



//                var postdata = new { addressList = addressList.Select(s => new { s.Address, s.Id, s.Province, s.CityCode, s.AreaCode, s.NickName, s.TelePhone, IsDefault = (s.IsDefault == 0 ? "正常" : "已删除") }) };

//                return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//            }
//            else
//            {
//                var address = _miniappaddressBll.GetModel(addressId);
//                if (address == null)
//                {
//                    return Json(new { isok = -1, msg = "地址信息不存在" }, JsonRequestBehavior.AllowGet);
//                }
//                //拼接地址名称
//                var provinceName = address.Province;
//                var cityName = address.CityCode;
//                var areaName = address.AreaCode;

//                address.Address_Dtl = $"{provinceName} {cityName} {areaName} {address.Address}";

//                var postdata = new { address = address };
//                return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//            }
//        }

//        /// <summary>
//        ///  设定默认收货地址
//        /// </summary>
//        /// <param name="State"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult setMyAddressDefault(string appid, string openid, int addressId)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }
//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }
//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            var address = _miniappaddressBll.GetModel(addressId);
//            if (address == null)
//            {
//                return Json(new { isok = -1, msg = "地址信息不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            if (!_miniappaddressBll.SetDefault(addressId, userInfo.Id))
//            {
//                return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
//            }
//            return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//        }

//        /// <summary>
//        ///  删除我的收货地址
//        /// </summary>
//        /// <param name="State"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult deleteMyAddress(string appid, string openid, int AddressId)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }
//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }
//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            var address = _miniappaddressBll.GetModel(AddressId);
//            if (address == null)
//            {
//                return Json(new { isok = -1, msg = "地址信息错误" }, JsonRequestBehavior.AllowGet);
//            }
//            address.State = -1;

//            if (!_miniappaddressBll.Update(address, "State"))
//            {
//                return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
//            }
//            return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//        }
//        #endregion

//        /// <summary>
//        /// 根据所选购买菜品 配上 运费模板 计算运费
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="openid"></param>
//        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult getOrderGoodsBuyPriceByCarIds(string appid, string openid, string goodCarIdStr)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }
//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            if (string.IsNullOrEmpty(goodCarIdStr))
//            {
//                return Json(new { isok = -1, msg = "购物车异常" }, JsonRequestBehavior.AllowGet);
//            }
//            var goodCarIdList = goodCarIdStr.Split(',').ToList();
//            if (goodCarIdStr.Substring(goodCarIdStr.Length - 1, 1) == ",")
//            {
//                goodCarIdList = goodCarIdStr.Substring(0, goodCarIdStr.Length - 1).Split(',').ToList();
//            }

//            var store = _miniappFoodBll.GetModel($" appid = {umodel.Id} ");
//            if (store == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }
//            var goodsCar = _miniappFoodGoodsCartBll.GetList($" Id in({string.Join(",", goodCarIdList)}) and UserId = {userInfo.Id} ");
//            if (goodsCar == null || !goodsCar.Any())
//            {
//                return Json(new { isok = -1, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
//            }

//            var fmodelList = _miniappFoodFreighTemplateBll.GetList($" StoreId = {store.Id} ");
//            if (fmodelList == null || !fmodelList.Any())
//            {
//                return Json(new { isok = 1, msg = "商家无设定运费模板" }, JsonRequestBehavior.AllowGet);
//            }
//            //购买价格计算
//            var qtySum = goodsCar.Sum(x => x.Count);
//            fmodelList.ForEach(x =>
//            {
//                var friPrice = 0;
//                if (qtySum <= x.BaseCount)
//                {
//                    friPrice = x.BaseCost;
//                }
//                else
//                {
//                    //初阶费用加上额外费用
//                    friPrice = x.BaseCost + (qtySum - x.BaseCount) * x.ExtraCost;
//                }
//                //临时存放模板所需运费
//                x.sum = (friPrice * 0.01).ToString();
//            });

//            var postdata = fmodelList.Select(x => new { x.Id, x.Name, x.sum });

//            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//        }





//        /// <summary>
//        /// 查询购物车指定记录
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="openid"></param>
//        /// <param name="pageindex"></param>
//        /// <param name="pagesize"></param>
//        /// <param name="orderbyid"></param>
//        /// <returns></returns>
//        public ActionResult getGoodsCarDataByIds(string appid, string openid, List<int> goodsCarList)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }

//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }


//            var myCartList = _miniappFoodGoodsCartBll.GetList($" Id in ({string.Join(",", goodsCarList)}) and UserInfo = {userInfo.Id} ");
//            if (myCartList == null || !myCartList.Any())
//            {
//                return Json(new { isok = -1, msg = "没有找到记录" }, JsonRequestBehavior.AllowGet);
//            }


//            return Json(new { isok = 1, msg = "成功", postdata = myCartList }, JsonRequestBehavior.AllowGet);
//        }


//        /// <summary>
//        /// 查询购物车
//        /// </summary>
//        /// <param name="appid">wx95a525ef2e44492f</param>
//        /// <param name="openid">oaPY9wWfdfUtGTK2xuxoX58QjYmk</param>
//        /// <param name="pageindex"></param>
//        /// <param name="pagesize"></param>
//        /// <returns></returns>
//        public ActionResult getGoodsCarData(string appid, string openid)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }

//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            List<FoodGoods> goodList = new List<FoodGoods>();
//            List<int> typeIdList = new List<int>();
//            List<FoodGoodsType> typeList = new List<FoodGoodsType>();
//            var myCartList = _miniappFoodGoodsCartBll.GetMyCart(miniappFood.Id, userInfo.Id);

//            if (myCartList != null && myCartList.Count > 0)
//            {
//                goodList = _miniappFoodGoodsBll.GetList($" Id in ({string.Join(",", myCartList.Select(x => x.FoodGoodsId))}) ");
//                goodList.ForEach(x =>
//                {
//                    x.Description = "";
//                });
//                myCartList.ForEach(x =>
//                {
//                    x.goodsMsg = goodList.Where(y => y.Id == x.FoodGoodsId).FirstOrDefault();
//                });
//                typeIdList = goodList.Select(x => x.TypeId).Distinct().ToList();
//                typeList = _miniappFoodGoodsTypeBll.GetList($" Id in ({string.Join(",", typeIdList)}) ");
//            }

//            #region 会员打折
//            int userLevelId = Utility.IO.Context.GetRequestInt("levelid", 0);
//            VipLevel userLevel = _miniappVipLevelBll.GetModel($"id={userLevelId} and state>=0");

//            _miniappVipLevelBll.CalculateVipGoodsCartPrice(myCartList, userLevel);
//            #endregion

//            var postdata = typeList.Select(x => new
//            {
//                typeName = x.Name,
//                GoodsCar = myCartList.Where(y => y.goodsMsg.TypeId == x.Id).ToList()
//            });
//            return Json(new { isok = 1, msg = "成功", postdata = postdata, myCartList = myCartList }, JsonRequestBehavior.AllowGet);
//        }

//        /// <summary>
//        /// 添加菜品至购物车
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="openid"></param>
//        /// <param name="goodid"></param>
//        /// <param name="attrSpacStr">1_5;2_7</param>
//        /// <param name="SpecInfo">菜品属性格式:颜色:白色;尺码:M;</param>
//        /// <param name="GoodsNumber"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult addGoodsCarData(string appid, string openid, int goodid, string attrSpacStr, string SpecInfo, int GoodsNumber, int lat = 0, int lng = 0, int newCartRecord = 0)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }
//            if (lat != 0 || lng != 0)
//            {
//                if (GetDistance(miniappFood.Lat, miniappFood.Lng, lat, lng) > miniappFood.DeliveryRange)
//                {
//                    return Json(new { isok = -1, msg = "不在配送范围" }, JsonRequestBehavior.AllowGet);
//                }
//            }
//            if (GoodsNumber <= 0)
//            {
//                return Json(new { isok = -1, msg = "数量必须大于0" }, JsonRequestBehavior.AllowGet);
//            }
//            var good = _miniappFoodGoodsBll.GetModel(goodid);
//            if (good == null)
//            {
//                return Json(new { isok = -1, msg = "未找到该菜品" }, JsonRequestBehavior.AllowGet);
//            }
//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }

//            var dbGoodCar = _miniappFoodGoodsCartBll.GetModel($" UserId={userInfo.Id} and FoodGoodsId={good.Id} and SpecIds='{attrSpacStr}' and State = 0 ");
//            if (dbGoodCar == null || newCartRecord == 1)
//            {
//                var goodsCar = new FoodGoodsCart();
//                goodsCar.FoodId = miniappFood.Id;
//                goodsCar.FoodGoodsId = good.Id;
//                goodsCar.SpecIds = attrSpacStr;
//                goodsCar.Count = GoodsNumber;
//                if (!string.IsNullOrWhiteSpace(attrSpacStr))
//                {
//                    goodsCar.Price = good.GASDetailList.Where(x => x.id.Equals(attrSpacStr)).First().price;
//                }
//                else
//                {
//                    goodsCar.Price = good.Price;
//                }
//                goodsCar.SpecInfo = SpecInfo;
//                goodsCar.UserId = userInfo.Id;
//                goodsCar.CreateDate = DateTime.Now;
//                goodsCar.State = 0;//加入购物车

//                var id = Convert.ToInt32(_miniappFoodGoodsCartBll.Add(goodsCar));
//                if (id > 0)
//                {
//                    return Json(new { isok = 1, msg = "成功", cartid = id }, JsonRequestBehavior.AllowGet);
//                }
//                else
//                {
//                    return Json(new { isok = -1, msg = "失败", cartid = 0 }, JsonRequestBehavior.AllowGet);
//                }
//            }
//            else
//            {
//                dbGoodCar.Count += GoodsNumber;
//                if (_miniappFoodGoodsCartBll.Update(dbGoodCar))
//                {
//                    return Json(new { isok = 1, msg = "成功", cardid = dbGoodCar.Id }, JsonRequestBehavior.AllowGet);
//                }
//                else
//                {
//                    return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
//                }
//            }
//        }

//        /// <summary>
//        /// 删除或更新购物车
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="openid"></param>
//        /// <param name="State">0为更新,-1为删除</param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult updateOrDeleteGoodsCarData(string appid, string openid, List<FoodGoodsCart> goodsCarModel, int State)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }
//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            if (goodsCarModel != null && goodsCarModel.Count > 0)
//            {
//                foreach (var item in goodsCarModel)
//                {
//                    var goodsCar = _miniappFoodGoodsCartBll.GetModel(item.Id);
//                    if (goodsCar == null)
//                    {
//                        return Json(new { isok = -1, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
//                    }
//                    if (goodsCar.UserId != userInfo.Id)
//                    {
//                        return Json(new { isok = -1, msg = "该记录不属于当前用户" }, JsonRequestBehavior.AllowGet);
//                    }

//                    //将记录状态改为删除
//                    if (State == -1)
//                    {
//                        goodsCar.State = -1;
//                    }
//                    else if (State == 0)//根据传入参数更新购物车内容
//                    {
//                        goodsCar.SpecIds = item.SpecIds;
//                        goodsCar.SpecInfo = item.SpecInfo;
//                        goodsCar.Count = item.Count;


//                        //价格因更改规格随之改变
//                        FoodGoods carGoods = _miniappFoodGoodsBll.GetModel(goodsCar.FoodGoodsId);
//                        if (carGoods == null)
//                        {
//                            goodsCar.GoodsState = 2;
//                        }
//                        else
//                        {
//                            if (!string.IsNullOrWhiteSpace(carGoods.AttrDetail))
//                            {
//                                int? price = carGoods.GASDetailList.Where(x => x.id.Equals(goodsCar.SpecIds))?.FirstOrDefault()?.price;
//                                if (price != null)
//                                {
//                                    goodsCar.Price = Convert.ToInt32(price);
//                                }
//                            }
//                        }
//                    }

//                    var success = _miniappFoodGoodsCartBll.Update(goodsCar, "State,SpecIds,SpecInfo,Count,Price,GoodsState");
//                    if (!success)
//                    {
//                        return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
//                    }
//                }
//            }
//            return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//        }



//        #region 餐饮订单
//        /// <summary>
//        /// 获取订单信息列表
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="openid"></param>
//        /// <param name="State">OrderState</param>
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult getMiniappGoodsOrder(string appid, string openid, int State = 10, int pageIndex = 1, int pageSize = 10)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }

//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }


//            var stateSql = (State != 10 ? $" and State = {State} " : "");

//            try
//            {

//                var goodOrderList = _miniappFoodGoodsOrderBll.GetList($" StoreId = {miniappFood.Id} and UserId = {userInfo.Id} { stateSql } and State != {(int)miniAppFoodOrderState.付款中} ", pageSize, pageIndex, "*", "CreateDate desc");
//                //var goodOrderList = _miniappFoodGoodsOrderBll.GetList($" StoreId = {miniappFood.Id} and UserId = {userInfo.Id} ", pageSize, pageIndex, "", "CreateDate desc");
//                if (goodOrderList != null && goodOrderList.Any())
//                {
//                    //订单详情
//                    var goodOrderDtlList = _miniappFoodGoodsCartBll.GetList($" GoodsOrderId in ({string.Join(",", goodOrderList.Select(x => x.Id))}) ");
//                    if (goodOrderDtlList != null && goodOrderList.Any())
//                    {
//                        var goodList = _miniappFoodGoodsBll.GetList($" Id in ({string.Join(",", goodOrderDtlList.Select(x => x.FoodGoodsId))}) ");
//                        goodOrderDtlList.ForEach(x =>
//                        {
//                            x.goodsMsg = goodList.Where(y => y.Id == x.FoodGoodsId).FirstOrDefault();
//                        });
//                        var postdata = goodOrderList.GroupBy(x => x.CreateDate.Year).Select(x =>
//                           new
//                           {
//                               year = x.Key,
//                               orderList = x.OrderByDescending(o => o.CreateDate).Select(y =>
//                                 new
//                                 {
//                                     Id = y.Id,
//                                     Title = goodOrderDtlList.Where(z => z.GoodsOrderId == y.Id).FirstOrDefault()?.goodsMsg?.GoodsName,
//                                     ImgUrl = goodOrderDtlList.Where(z => z.GoodsOrderId == y.Id).FirstOrDefault()?.goodsMsg?.ImgUrl,
//                                     Count = goodOrderDtlList.Where(z => z.GoodsOrderId == y.Id).Count(),
//                                     StateTitle = Enum.GetName(typeof(miniAppFoodOrderState), y.State),
//                                     State = y.State,
//                                     CreateDate = y.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
//                                     //OrderDate = y.CreateDate.ToString("MM-dd"),
//                                     BuyPrice = y.BuyPrice * 0.01,
//                                     cityMorderId = y.OrderId,
//                                     GoodsDtlName = string.Join(" + ", goodOrderDtlList.Where(z => z.GoodsOrderId == y.Id).Select(z => z.goodsMsg.GoodsName)),
//                                     OrderTypeStr = y.OrderType == (int)miniAppFoodOrderType.堂食 ? "堂食" : "外卖",
//                                     BuyMode = y.BuyMode,
//                                 })
//                           });
//                        return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                log4net.LogHelper.WriteInfo(GetType(), ex.Message + "|" + $" StoreId = {miniappFood.Id} and UserId = {userInfo.Id} { stateSql } ");
//                return Json(new { isok = -1, msg = "失败", postdata = "" }, JsonRequestBehavior.AllowGet);
//            }

//            return Json(new { isok = 1, msg = "成功", postdata = "" }, JsonRequestBehavior.AllowGet);
//        }

//        /// <summary>
//        /// 获取订单详情
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="openid"></param>
//        /// <param name="State">OrderState</param>
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult getMiniappGoodsOrderById(string appid, string openid, int orderId)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }

//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            var goodOrder = _miniappFoodGoodsOrderBll.GetModel($" Id = {orderId} and StoreId = {miniappFood.Id} and UserId = {userInfo.Id}");
//            if (goodOrder == null)
//            {
//                return Json(new { isok = -1, msg = "订单信息不存在" }, JsonRequestBehavior.AllowGet);
//            }

//            var goodOrderDtl = _miniappFoodGoodsCartBll.GetList($" GoodsOrderId = {goodOrder.Id} ");
//            var goodList = _miniappFoodGoodsBll.GetList($" Id in ({string.Join(",", goodOrderDtl.Select(x => x.FoodGoodsId))}) ");


//            var postdata = new
//            {
//                buyPrice = goodOrder.BuyPrice * 0.01,
//                freightPrice = goodOrder.FreightPrice * 0.01,
//                stateRemark = Enum.GetName(typeof(miniAppFoodOrderState), goodOrder.State),
//                //orderFriRemark = _miniappfreightemplateBll.GetModel(goodOrder.FreightTemplateId)?.Name,
//                goodOrder = goodOrder,
//                OrderType = goodOrder.OrderType == (int)miniAppFoodOrderType.堂食 ? "堂食" : "外卖",
//                goodOrderDtl = goodOrderDtl.Select(x => new
//                {
//                    priceStr = (x.Price * x.Count * 0.01).ToString("0.00"),
//                    goodImgUrl = goodList.Where(y => y.Id == x.FoodGoodsId).FirstOrDefault()?.ImgUrl,
//                    goodname = goodList.Where(y => y.Id == x.FoodGoodsId).FirstOrDefault()?.GoodsName,
//                    orderDtl = x
//                })
//            };

//            return Json(new { isok = 1, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
//        }


//        /// <summary>
//        /// 生成订单
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="openid"></param>
//        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
//        /// <param name="order"></param>
//        /// <returns></returns>
//        [HttpPost]
//        //public ActionResult f(string appid, string openid, string goodCarIdStr, string orderjson, int buyMode = (int)miniAppBuyMode.微信支付)
//        //{
//        //    string dugmsg = "dugmsg";
//        //    #region  基本验证
//        //    if (string.IsNullOrEmpty(orderjson))
//        //    {
//        //        return Json(new { isok = -1, msg = "订单不能为空" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    if (string.IsNullOrEmpty(appid))
//        //    {
//        //        return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//        //    if (umodel == null)
//        //    {
//        //        return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//        //    }

//        //    var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//        //    if (miniappFood == null)
//        //    {
//        //        return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    if (miniappFood.openState != 1)
//        //    {
//        //        return Json(new { isok = -1, msg = "商家未营业" }, JsonRequestBehavior.AllowGet);
//        //    }

//        //    //添加锁项
//        //    try
//        //    {
//        //        //不同商家，不同的锁,当前商家若还未创建，则创建一个
//        //        if (!lockObjectDict_Order.ContainsKey(miniappFood.Id))
//        //        {
//        //            if (!lockObjectDict_Order.TryAdd(miniappFood.Id, new object()))
//        //            {
//        //                return Json(new { isok = -1, msg = "系统繁忙,请稍候再试！" }, JsonRequestBehavior.AllowGet);
//        //            }
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return Json(new { isok = -1, msg = "系统繁忙,请稍候再试！" }, JsonRequestBehavior.AllowGet);
//        //    }


//        //    dugmsg += "a";
//        //    var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//        //    if (userInfo == null)
//        //    {
//        //        return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    if (string.IsNullOrEmpty(goodCarIdStr))
//        //    {
//        //        return Json(new { isok = -1, msg = "购物车异常" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    var goodCarIdList = goodCarIdStr.Split(',').ToList();
//        //    if (goodCarIdStr.Substring(goodCarIdStr.Length - 1, 1) == ",")
//        //    {
//        //        goodCarIdList = goodCarIdStr.Substring(0, goodCarIdStr.Length - 1).Split(',').ToList();
//        //    }
//        //    var goodsCar = _miniappFoodGoodsCartBll.GetList($" Id in({string.Join(",", goodCarIdList)}) and UserId = {userInfo.Id} and state = 0 ");
//        //    if (goodsCar == null || goodsCar.Count <= 0)
//        //    {
//        //        return Json(new { isok = -1, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    dugmsg += "b";
//        //    //失效菜品
//        //    var carErrorData = goodsCar.Where(x => x.GoodsState > 0).ToList();
//        //    foreach (var x in goodsCar)
//        //    {
//        //        if (x.GoodsState > 0)
//        //        {
//        //            var good = _miniappFoodGoodsBll.GetModel(x.FoodGoodsId);
//        //            if (good == null)
//        //            {
//        //                return Json(new { isok = -1, msg = "菜品信息错误,请重新选择购买菜品！" }, JsonRequestBehavior.AllowGet);
//        //            }
//        //            return Json(new { isok = -1, msg = $"菜品 '{good.GoodsName}' 已经下架或被删除,请重新选择购买菜品！ " }, JsonRequestBehavior.AllowGet);
//        //        }
//        //    }
//        //    if (carErrorData != null && carErrorData.Count > 0)
//        //    {
//        //        return Json(new { isok = -1, msg = $"存在失效菜品 " }, JsonRequestBehavior.AllowGet);
//        //    }

//        //    dugmsg += "c";

//        //    #region 会员打折

//        //    //获取会员信息
//        //    VipRelation vipInfo = _miniappVipRelationBll.GetModel($"uid={userInfo.Id} and state>=0");

//        //    StringBuilder sbUpdateGoodCartSql = null;

//        //    var beforeDiscountPrice = 0;//优惠前商品总价
//        //    var afterDiscountPrice = 0;//优惠后商品总价
//        //    if (vipInfo != null)
//        //    {
//        //        VipLevel levelinfo = _miniappVipLevelBll.GetModel($"id={vipInfo.levelid} and state>=0");
//        //        beforeDiscountPrice = goodsCar.Sum(x => x.Price * x.Count);//优惠前商品总价
//        //        _miniappVipLevelBll.CalculateVipGoodsCartPrice(goodsCar, levelinfo);
//        //        afterDiscountPrice = goodsCar.Sum(x => x.Price * x.Count);//优惠后商品总价

//        //        if (levelinfo != null)
//        //        {
//        //            sbUpdateGoodCartSql = new StringBuilder();
//        //            foreach (var item in goodsCar)
//        //            {
//        //                sbUpdateGoodCartSql.Append(_miniappFoodGoodsCartBll.BuildUpdateSql(item, "Price,originalPrice") + ";");
//        //            }
//        //        }
//        //    }
//        //    #endregion

//        //    try
//        //    {
//        //        //菜品总价格
//        //        var price = goodsCar.Sum(x => x.Price * x.Count);
//        //        if (price <= 0)
//        //        {
//        //            return Json(new { isok = -1, msg = "菜品价格有误" }, JsonRequestBehavior.AllowGet);
//        //        }
//        //        var order = JsonConvert.DeserializeObject<FoodGoodsOrder>(orderjson);
//        //        if (order == null)
//        //        {
//        //            return Json(new { isok = -1, msg = "订单出错" }, JsonRequestBehavior.AllowGet);
//        //        }

//        //        dugmsg += "d";
//        //        FoodAddress address = new FoodAddress();
//        //        if (order.OrderType == (int)miniAppFoodOrderType.外卖)
//        //        {
//        //            if (price < miniappFood.OutSide)
//        //            {
//        //                return Json(new { isok = -1, msg = $"还差{((miniappFood.OutSide - price) * 0.01).ToString("0.00")}元起送,无法提交订单" }, JsonRequestBehavior.AllowGet);
//        //            }

//        //            dugmsg += "1.";

//        //            address = _miniappaddressBll.GetModel(order.AddressId);
//        //            if (address == null)
//        //            {
//        //                return Json(new { isok = -1, msg = "收货地址信息错误" }, JsonRequestBehavior.AllowGet);
//        //            }
//        //            dugmsg += "2.";

//        //            double distance = GetDistance(miniappFood.Lat, miniappFood.Lng, address.Lat, address.Lng);
//        //            if (distance > miniappFood.DeliveryRange)
//        //            {
//        //                return Json(new { isok = -1, msg = "超出配送范围,请更换收货地址" }, JsonRequestBehavior.AllowGet);
//        //            }
//        //            if (miniappFood.TakeOut != 1)
//        //            {
//        //                return Json(new { isok = -1, msg = "商家未开启外卖服务" }, JsonRequestBehavior.AllowGet);
//        //            }
//        //            dugmsg += "3.";
//        //        }
//        //        else
//        //        {
//        //            dugmsg += "4.";
//        //            var table = _miniappFoodTableBll.GetModelByScene(miniappFood.Id, order.TablesNo.ToString());
//        //            dugmsg += "4.";
//        //            if (table == null || table.Id <= 0)
//        //            {
//        //                return Json(new { isok = -1, msg = "桌台号错误" }, JsonRequestBehavior.AllowGet);
//        //            }
//        //            dugmsg += "5.";
//        //            if (miniappFood.TheShop != 1)
//        //            {
//        //                return Json(new { isok = -1, msg = "商家未开启堂食服务" }, JsonRequestBehavior.AllowGet);
//        //            }
//        //        }
//        //        dugmsg += "1";
//        //        //失效菜品 & 异常菜品
//        //        foreach (var x in goodsCar)
//        //        {

//        //            var good = _miniappFoodGoodsBll.GetModel(x.FoodGoodsId);
//        //            if (good == null)
//        //            {
//        //                return Json(new { isok = -1, msg = "菜品信息错误,请重新选择购买菜品！" }, JsonRequestBehavior.AllowGet);
//        //            }
//        //            if (order.OrderType == 1)
//        //            {
//        //                if (good.openTakeOut != 1)
//        //                {
//        //                    return Json(new { isok = -1, msg = $"菜品'{good.GoodsName}' 未开启外卖送餐,请重新选择其他菜品！" }, JsonRequestBehavior.AllowGet);
//        //                }
//        //            }
//        //            else
//        //            {
//        //                if (good.openTheShop != 1)
//        //                {
//        //                    return Json(new { isok = -1, msg = $"菜品'{good.GoodsName}' 未开启堂食,请重新选择其他菜品！" }, JsonRequestBehavior.AllowGet);
//        //                }
//        //            }
//        //            if (x.GoodsState > 0)
//        //            {
//        //                return Json(new { isok = -1, msg = $"菜品 '{good.GoodsName}' 已经下架或被删除,请重新选择购买菜品！ " }, JsonRequestBehavior.AllowGet);
//        //            }
//        //        }
//        //        dugmsg += "2";
//        //        #endregion
//        //        //配送方式
//        //        order.GetWay = order.OrderType == (int)miniAppFoodOrderType.外卖 ?
//        //                         (int)miniAppOrderGetWay.商家配送 :
//        //                                 (int)miniAppOrderGetWay.到店自取;
//        //        order.StoreId = miniappFood.Id;
//        //        order.UserId = userInfo.Id;
//        //        order.CreateDate = DateTime.Now;
//        //        order.State = (int)miniAppFoodOrderState.待付款;
//        //        order.AccepterName = address.NickName;
//        //        order.AccepterTelePhone = address.TelePhone;
//        //        //order.BuyMode = (int)miniAppBuyMode.微信支付;   //支付方式
//        //        order.QtyCount = goodsCar.Sum(x => x.Count);
//        //        order.ReducedPrice = beforeDiscountPrice - afterDiscountPrice;
//        //        dugmsg += "f";
//        //        //购买价格计算
//        //        var friPrice = 0;
//        //        if (order.TablesNo > 0)
//        //        {
//        //            friPrice = 0;
//        //            order.OrderType = 0;
//        //        }
//        //        else
//        //        {
//        //            friPrice = miniappFood.ShippingFee;
//        //            order.OrderType = 1;
//        //        }
//        //        order.FreightPrice = friPrice;
//        //        order.BuyPrice = price + friPrice;
//        //        if (order.BuyPrice > 999999999)
//        //        {
//        //            return Json(new { isok = -1, msg = "单张订单的总金额不可超过9999999.99！" }, JsonRequestBehavior.AllowGet);
//        //        }
//        //        dugmsg += "g";

//        //        order.ZipCode = address.ZipCode;
//        //        order.Address = $"{address.Address}";
//        //        order.BuyMode = buyMode;
//        //        dugmsg += 0;
//        //        lock (lockObjectDict_Order[miniappFood.Id])
//        //        {
//        //            dugmsg += 1;
//        //            //检查当前菜品库存是否足够
//        //            foreach (var x in goodsCar)
//        //            {
//        //                var curGoodQty = _miniappFoodGoodsBll.GetGoodQty(x.FoodGoodsId, x.SpecIds);
//        //                var count = goodsCar.Where(y => y.FoodGoodsId == x.FoodGoodsId && y.SpecIds == x.SpecIds).Sum(y => y.Count);
//        //                if (curGoodQty < x.Count)
//        //                {
//        //                    var curGood = _miniappFoodGoodsBll.GetModel(x.FoodGoodsId);
//        //                    return Json(new { isok = -1, msg = $"菜品: {curGood.GoodsName} {(!string.IsNullOrWhiteSpace(x.SpecInfo) ? "规格:" + x.SpecInfo : "")} 库存不足!" }, JsonRequestBehavior.AllowGet);
//        //                }
//        //            }
//        //            var saveMoneyUser = new SaveMoneySetUser();
//        //            if (buyMode == (int)miniAppBuyMode.储值支付)
//        //            {
//        //                saveMoneyUser = _miniappsavemoneysetuserBll.getModelByUserId(umodel.AppId, userInfo.Id);
//        //                if (saveMoneyUser == null || saveMoneyUser.AccountMoney < order.BuyPrice)
//        //                {
//        //                    return Json(new { isok = -1, msg = $" 储值余额不足,请充值！ " }, JsonRequestBehavior.AllowGet);
//        //                }
//        //            }
//        //            dugmsg += 2;
//        //            if (!_miniappFoodGoodsOrderBll.addGoodsOrder(order, goodsCar, userInfo, sbUpdateGoodCartSql, ref dugmsg))
//        //            //if (!_miniappFoodGoodsOrderBll.addGoodsOrder(order, goodsCar, userInfo, null, ref dugmsg))
//        //            {
//        //                return Json(new { isok = -1, msg = $"订单生成失败！" }, JsonRequestBehavior.AllowGet);
//        //            }
//        //            dugmsg += 3;
//        //            var cartmodel = _miniappFoodGoodsCartBll.GetModel("Id=" + goodsCar[0].Id + " and GoodsOrderId>0");
//        //            if (cartmodel == null)
//        //            {
//        //                cartmodel = _miniappFoodGoodsCartBll.GetModel("Id=" + goodsCar[0].Id + " and GoodsOrderId>0");
//        //            }
//        //            var curGoodOrderId = cartmodel.GoodsOrderId;
//        //            dugmsg += 4;
//        //            dugmsg += "cart:" + goodsCar[0].Id + "-" + curGoodOrderId;
//        //            var dbOrder = _miniappFoodGoodsOrderBll.GetModel(curGoodOrderId);
//        //            if (dbOrder == null)
//        //            {
//        //                return Json(new { isok = -1, msg = $"订单生成失败！" + dugmsg }, JsonRequestBehavior.AllowGet);
//        //            }
//        //            if (buyMode == (int)miniAppBuyMode.微信支付)
//        //            {
//        //                #region CtiyModer 生成
//        //                string no = WxPayApi.GenerateOutTradeNo();

//        //                CityMorders citymorderModel = new CityMorders
//        //                {
//        //                    OrderType = (int)ArticleTypeEnum.MiniappFoodGoods,
//        //                    ActionType = (int)ArticleTypeEnum.MiniappFoodGoods,
//        //                    Addtime = DateTime.Now,
//        //                    payment_free = dbOrder.BuyPrice,
//        //                    trade_no = no,
//        //                    Percent = 99,//不收取服务费
//        //                    userip =  WebHelper.GetIP(),
//        //                    FuserId = userInfo.Id,
//        //                    Fusername = userInfo.NickName,
//        //                    orderno = no,
//        //                    payment_status = 0,
//        //                    Status = 0,
//        //                    Articleid = 0,
//        //                    CommentId = 0,
//        //                    MinisnsId = miniappFood.Id,//商家ID
//        //                    TuserId = dbOrder.Id,//订单的ID
//        //                    ShowNote = $" {umodel.Title}点餐 支付{dbOrder.BuyPrice * 0.01}元",
//        //                    CitySubId = 0,//无分销,默认为0
//        //                    PayRate = 1,
//        //                    buy_num = 0, //无
//        //                    appid = appid,
//        //                };
//        //                dugmsg += 14;
//        //                var orderid = Convert.ToInt32(new CityMordersBLL().Add(citymorderModel));
//        //                dbOrder.OrderId = orderid;
//        //                dugmsg += 15;
//        //                #endregion
//        //            }

//        //            #region 更新对外订单号及对应CityModer的ID
//        //            //对外订单号规则：年月日时分 + 餐饮本地库ID最后3位数字
//        //            var idStr = dbOrder.Id.ToString();
//        //            if (idStr.Length >= 3)
//        //            {
//        //                idStr = idStr.Substring(idStr.Length - 3, 3);
//        //            }
//        //            else
//        //            {
//        //                idStr.PadLeft(3, '0');
//        //            }
//        //            idStr = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{idStr}";
//        //            dbOrder.OrderNum = idStr;
//        //            dugmsg += 16;
//        //            _miniappFoodGoodsOrderBll.Update(dbOrder, "OrderNum,OrderId");
//        //            dugmsg += 17;
//        //            #endregion

//        //            if (buyMode == (int)miniAppBuyMode.储值支付)
//        //            {
//        //                #region 储值支付 扣除预存款金额并生成消费记录
//        //                if (payOrderBySaveMoneyUser(dbOrder, saveMoneyUser, sbUpdateGoodCartSql))
//        //                {
//        //                    return Json(new { isok = 1, msg = "订单生成并支付成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
//        //                }
//        //                else
//        //                {
//        //                    return Json(new { isok = -1, msg = "订单支付失败", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
//        //                }
//        //                #endregion
//        //            }

//        //            //记录订单操作日志(用户下单)
//        //            _miniappFoodGoodsOrderLogBll.Add(new FoodGoodsOrderLog() { GoodsOrderId = dbOrder.Id, UserId = userInfo.Id.ToString(), LogInfo = $" 成功下单,下单金额：{dbOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now });
//        //            dugmsg += 18;
//        //            return Json(new { isok = 1, msg = "订单生成成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return Json(new { isok = -1, msg = ex.Message + "," + dugmsg }, JsonRequestBehavior.AllowGet);
//        //    }

//        //}
//        #endregion


//        /// <summary>
//        /// 餐饮菜品支付
//        /// </summary>
//        /// <param name="orderid"></param>
//        /// <param name="type"></param>
//        /// <param name="openId"></param>
//        /// <returns></returns>
//        [AuthLoginCheckXiaoChenXun]
//        public ActionResult PayOrder(int orderid, int type, string openId)
//        {
//            try
//            {
//                CityMorders order = new CityMordersBLL().GetModel(orderid);

//                if (order == null || order.payment_status != 0)
//                {
//                    return ApiResult(false, "订单已经失效");
//                }

//                if (order.OrderType == (int)ArticleTypeEnum.MiniappFoodGoods)
//                {
//                    //改为付款中,避免付款过程中被自动取消服务取消掉
//                    bool success = SqlMySql.ExecuteNonQuery(_miniappFoodGoodsOrderBll.connName, CommandType.Text, $" update foodgoodsorder set State = {(int)miniAppFoodOrderState.付款中} where orderid = {orderid} and (State in ({(int)miniAppFoodOrderState.待付款},{(int)miniAppFoodOrderState.付款中}) );", null) > 0;
//                    if (!success) return ApiResult(false, "订单已经失效");
//                }

//                var food = _miniappFoodBll.GetModel(order.MinisnsId);
//                if (food == null)
//                {
//                    return ApiResult(false, "商家信息错误");
//                }
//                var app = _xcxappaccountrelationBll.GetModel(food.appId);
//                if (app == null)
//                {
//                    return ApiResult(false, "授权信息错误");
//                }

//                var LoginCUser = LoginData;
//                if (LoginCUser == null)
//                {
//                    return ApiResult(false, "登陆信息异常，请重新支付,Entity_null");
//                }

//                PayCenterSetting setting = null;
//                if (!string.IsNullOrEmpty(order.appid))
//                {
//                    setting = new PayCenterSettingBLL().GetPayCenterSetting(order.appid);
//                }
//                else
//                {
//                    setting = new PayCenterSettingBLL().GetPayCenterSetting((int)PayCenterSettingType.City, order.MinisnsId);
//                }

//                if (openId.IsNullOrWhiteSpace())
//                {
//                    return ApiResult(false, "Openid_null/Empty");
//                }

//                if (type != 2)//JSAPI支付预处理
//                {
//                    JsApiPay jsApiPay = new JsApiPay(HttpContext)
//                    {
//                        total_fee = order.payment_free,
//                        openid = openId
//                    };
//                    //统一下单，获得预支付码
//                    WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResultByCity(setting, order);
//                    log4net.LogHelper.WriteInfo(GetType(), WebConfigBLL.citynotify_url);


//                    //_miniapptemplatemsg_userBll.Delete($" OrderId = {orderid} ");

//                    //var list = _miniapptemplatemsg_userBll.getListByAppId(app.AppId, 8);
//                    //log4net.LogHelper.WriteInfo(GetType(), "list.count" + list.Count);
//                    //list.ForEach(x =>
//                    //{
//                    //    MiniAppTemplateMsg_UserLog userLog = new MiniAppTemplateMsg_UserLog();
//                    //    userLog.OrderId = orderid;
//                    //    userLog.Open_Id = openId;
//                    //    userLog.AddDate = DateTime.Now;
//                    //    userLog.Form_Id = Convert.ToString(unifiedOrderResult.GetValue("prepay_id"));
//                    //    userLog.TmuId = x.Id;
//                    //    userLog.State = 0;

//                    //    _miniapptemplatemsg_userlogBll.Add(userLog);
//                    //});

//                    #region 插入模板消息记录表(拿到form_id为了可以发送模板消息)   --不用
//                    //log4net.LogHelper.WriteInfo(GetType(), "开始加记录");


//                    //var foodOrder = _miniappFoodGoodsOrderBll.GetModel($" OrderId = {orderid}  ") ?? new MiniappFoodGoodsOrder();
//                    //var model = _miniapptemplatemsg_userlogBll.GetModel($" OrderId = {foodOrder.Id} and Ttypeid = 8 ");

//                    //if (model != null && model.Form_Id == Convert.ToString(unifiedOrderResult.GetValue("prepay_id")))
//                    //{

//                    //    //log4net.LogHelper.WriteInfo(GetType(), "开始加记录");
//                    //}
//                    //else
//                    //{
//                    //    //log4net.LogHelper.WriteInfo(GetType(), "开始加记录");
//                    //    _miniapptemplatemsg_userlogBll.Delete($" OrderId = {foodOrder.Id} and  Ttypeid = 8");

//                    //    var list = _miniapptemplatemsg_userBll.getListByAppId(app.AppId, 8);
//                    //    log4net.LogHelper.WriteInfo(GetType(), "list.count" + list.Count);
//                    //    list.ForEach(x =>
//                    //    {
//                    //        MiniAppTemplateMsg_UserLog userLog = new MiniAppTemplateMsg_UserLog();
//                    //        userLog.OrderId = foodOrder.Id;
//                    //        userLog.Open_Id = openId;
//                    //        userLog.AddDate = DateTime.Now;
//                    //        userLog.Form_Id = Convert.ToString(unifiedOrderResult.GetValue("prepay_id"));
//                    //        userLog.TmuId = x.Id;
//                    //        userLog.TmId = x.TmId;
//                    //        userLog.State = 0;
//                    //        userLog.TmgType = x.TmgType;
//                    //        userLog.Ttypeid = 8;
//                    //        _miniapptemplatemsg_userlogBll.Add(userLog);
//                    //    });
//                    //    //log4net.LogHelper.WriteInfo(GetType(), "加完记录");
//                    //}
//                    #endregion


//                    #region 发送消息参数

//                    var foodOrder = _miniappFoodGoodsOrderBll.GetModel($" OrderId = {orderid}  ") ?? new FoodGoodsOrder();
//                    if (!string.IsNullOrWhiteSpace(Convert.ToString(unifiedOrderResult.GetValue("prepay_id"))))
//                    {

//                        //增加发送模板消息参数
//                        TemplateMsg_UserParam userParam = new TemplateMsg_UserParam();
//                        userParam.AppId = app.AppId;
//                        userParam.Form_IdType = 1;//form_id 为prepay_id
//                        userParam.OrderId = foodOrder.Id;
//                        userParam.OrderIdType = (int)TmpType.小程序餐饮模板;
//                        userParam.Open_Id = openId;
//                        userParam.AddDate = DateTime.Now;
//                        userParam.Form_Id = Convert.ToString(unifiedOrderResult.GetValue("prepay_id"));
//                        userParam.State = 1;
//                        userParam.SendCount = 0;
//                        userParam.AddDate = DateTime.Now;
//                        userParam.LoseDateTime = DateTime.Now.AddDays(7);//prepay_id 有效期7天

//                        _miniapptemplatemsg_userParamBll.Add(userParam);
//                    }
//                    #endregion

//                    return ApiResult(true, "下单成功", jsApiPay.GetJsApiParametersnew());
//                }
//                else
//                {
//                    //NativePay native = new NativePay();
//                    //native.openid = openId;
//                    //native.total_fee = order.payment_free;
//                    //string url = native.GetOrderPayUrlByCity(setting, order);
//                    return ApiResult(true, "下单成功");
//                }
//            }
//            catch (Exception ex)
//            {
//                log4net.LogHelper.WriteError(GetType(), ex);
//                return ApiResult(false, "下单异常", ex.Message);
//            }

//        }


//        #region 储值支付方式支付

//        /// <summary>
//        /// 储值支付 扣除预存款金额并生成消费记录(电商)
//        /// </summary>
//        /// <param name="dbOrder"></param>
//        /// <param name="saveMoneyUser"></param>
//        /// <returns></returns>
//        public bool payOrderBySaveMoneyUser(FoodGoodsOrder dbOrder, SaveMoneySetUser saveMoneyUser, StringBuilder updateCarPriceSql)
//        {
//            if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
//            {
//                return false;
//            }
//            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
//            {
//                return false;
//            }
//            MySqlParameter[] _pone = null;
//            TransactionModel _tranModel = new TransactionModel();
//            if (updateCarPriceSql != null)
//            {
//                _tranModel.Add(updateCarPriceSql.ToString());
//            }

//            if (dbOrder != null)
//            {
//                var food = _miniappFoodBll.GetModel(dbOrder.StoreId);
//                if (!food.funJoinModel.canSaveMoneyFunction)
//                {
//                    return false;
//                }

//                dbOrder.PayDate = DateTime.Now;//支付时间
//                dbOrder.State = (int)miniAppFoodOrderState.待接单;

//                //自动接单则跳过待接单状态
//                if (food != null && food.AutoAcceptOrder == 1)
//                {
//                    dbOrder.State = (dbOrder.OrderType == (int)miniAppFoodOrderType.堂食 ? (int)miniAppFoodOrderState.待就餐 : (int)miniAppFoodOrderState.待送餐);
//                    dbOrder.ConfDate = DateTime.Now;//接单时间
//                    if (dbOrder.State == (int)miniAppFoodOrderState.待就餐)
//                    {
//                        dbOrder.DistributeDate = DateTime.Now;
//                    }
//                }

//                dbOrder.GoodsGuid = Guid.NewGuid().ToString().Replace("-", "");//此栏位暂无用处
//                _tranModel.Add(_miniappFoodGoodsOrderBll.BuildUpdateSql(dbOrder, $"State,GoodsGuid,PayDate,ConfDate{(dbOrder.State == (int)miniAppFoodOrderState.待就餐 ? ",DistributeDate" : "")}", out _pone), _pone);

//                _tranModel.Add(_miniappFoodGoodsOrderLogBll.BuildAddSql(new FoodGoodsOrderLog() { GoodsOrderId = dbOrder.Id, UserId = dbOrder.UserId.ToString(), LogInfo = $" 订单成功支付(储值支付)：{dbOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));
//            }
//            _tranModel.Add(_miniappsavemoneysetuserlogBll.BuildAddSql(new SaveMoneySetUserLog()
//            {
//                AppId = saveMoneyUser.AppId,
//                UserId = dbOrder.UserId,
//                MoneySetUserId = saveMoneyUser.Id,
//                Type = -1,
//                BeforeMoney = saveMoneyUser.AccountMoney,
//                AfterMoney = saveMoneyUser.AccountMoney - dbOrder.BuyPrice,
//                ChangeMoney = dbOrder.BuyPrice,
//                ChangeNote = $" 购买商品,订单号:{dbOrder.OrderNum} ",
//                CreateDate = DateTime.Now,
//                State = 1
//            }));

//            _tranModel.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney - {dbOrder.BuyPrice} where id =  {saveMoneyUser.Id} ; ");

//            var isSuccess = _miniappsavemoneysetuserlogBll.ExecuteTransaction(_tranModel.sqlArray, _tranModel.ParameterArray);

//            if (isSuccess)
//            {
//                AfterPaySuccesExecFun(dbOrder);

                
//            }

//            return isSuccess;
//        }

//        /// <summary>
//        /// 储值支付后
//        /// </summary>
//        /// <param name="foodGoodsOrder"></param>
//        public void AfterPaySuccesExecFun(FoodGoodsOrder foodGoodsOrder)
//        {

//            if (foodGoodsOrder == null)
//            {
//                return;
//            }

//            #region 自动打单 + 发送餐饮订单支付成功通知 模板消息


//            XcxAppAccountRelationBLL xapprBll = new XcxAppAccountRelationBLL();
//            AccountBLL accountBll = new AccountBLL();
//            FoodPrintsBLL printBll = new FoodPrintsBLL();
//            FoodGoodsBLL _miniappfoodgoodsBll = new FoodGoodsBLL();
//            FoodOrderPrintLogBLL _miniappfoodorderprintlogBll = new FoodOrderPrintLogBLL();
//            C_UserInfoBLL userInfoBll = new C_UserInfoBLL();


//            MsnModelHelper msnHelper = new MsnModelHelper();
//            TemplateMsgBLL tmgBll = new TemplateMsgBLL();
//            TemplateMsg_UserBLL tmguBll = new TemplateMsg_UserBLL();
//            TemplateMsg_UserLogBLL tmgulBll = new TemplateMsg_UserLogBLL();
//            TemplateMsg_UserParamBLL userParamBll = new TemplateMsg_UserParamBLL();
//            List<FoodGoodsCart> carlist = new List<FoodGoodsCart>();

//            var food = _miniappFoodBll.GetModel(foodGoodsOrder.StoreId);
//            var xappr = xapprBll.GetModel(food.appId);
//            var account = accountBll.GetModel(xappr.AccountId);
//            var foodPrintList = printBll.GetList($" foodstoreid = {food.Id} and accountId = '{account.OpenId}' and state >= 0  and industrytype=1") ?? new List<FoodPrints>();
//            carlist = _miniappFoodGoodsCartBll.GetList($" GoodsOrderId={foodGoodsOrder.Id} and state=1");
//            carlist.ForEach(x =>
//            {
//                x.goodsMsg = _miniappfoodgoodsBll.GetModel($"Id={x.FoodGoodsId}");
//            });

//            //打单
//            _miniappFoodGoodsOrderBll.PrintOrder(food, foodGoodsOrder, carlist, foodPrintList);


//            #region 发送餐饮订单支付成功通知 模板消息
//            var userinfo = userInfoBll.GetModel(foodGoodsOrder.UserId) ?? new C_UserInfo();
//            var tmgup = userParamBll.getParamByAppIdOpenId(xappr.AppId,userinfo.OpenId) ?? new Entity.MiniApp.Conf.TemplateMsg_UserParam();
//            var tmgu = tmguBll.getModelByAppIdTypeId(tmgup.AppId, (int)TmpType.小程序餐饮模板, (int)SendTemplateMessageTypeEnum.餐饮订单支付成功通知) ?? new Entity.MiniApp.Conf.TemplateMsg_User();
//            if (tmgu.State == 1)
//            {
//                string msg = string.Empty;
//                var postData = _miniappFoodGoodsOrderBll.getTemplateMessageData(foodGoodsOrder.Id, SendTemplateMessageTypeEnum.餐饮订单支付成功通知);
//                //msnHelper.sendMyMsn(userinfo.appId, userinfo.OpenId, tmgu.TemplateId, tmgu.PageUrl + $"?scene={foodGoodsOrder.Id}", tmgup.Form_Id, postData, string.Empty, string.Empty, ref msg);
//                msnHelper.sendMyMsn(userinfo.appId, userinfo.OpenId, tmgu.TemplateId, tmgu.PageUrl, tmgup.Form_Id, postData, string.Empty, string.Empty, ref msg);
//                //参数使用次数增加(默认是1)
//                userParamBll.addUsingCount(tmgup);
//            }
//            #endregion

//            #region 发送模板消息通知商家
//            var title = $"您的店铺【{food.FoodsName}】有新的订单！";
//            var remark1 = $" 该订单于 {DateTime.Now.ToString("yyyy-MM-dd HH:mm")} 下单. ";
//            var userBaseInfo = new UserBaseInfoBLL().GetModelByUnionidServerid(account.UnionId, WebSiteConfig.DZ_WxSerId);
//            if (userBaseInfo != null && !string.IsNullOrWhiteSpace(userBaseInfo.openid))
//            {
//                //提醒用户发送消息成功
//                C_TplMsgHelper.SendTplMsgFromVzan(userBaseInfo.openid, "", title, foodGoodsOrder.OrderNum, foodGoodsOrder.BuyPriceStr, foodGoodsOrder.AccepterName, string.Join(",", carlist?.Select(x => x.goodsMsg?.GoodsName)), remark1);

//            }
//            #endregion

//            #endregion
//        }



//        /// <summary>
//        /// 使用储值支付
//        /// </summary>
//        /// <param name="dbOrder"></param>
//        /// <param name="saveMoneyUser"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult buyOrderbySaveMoney(string appid, string openid, int goodsorderid)
//        {

//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }

//            var dbOrder = _miniappFoodGoodsOrderBll.GetModel(goodsorderid);
//            if (dbOrder == null)
//            {
//                return Json(new { isok = -1, msg = "订单不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            if (dbOrder.State != 0)
//            {
//                return Json(new { isok = -1, msg = "此订单不可进行支付" }, JsonRequestBehavior.AllowGet);
//            }
//            if (dbOrder.BuyMode != (int)miniAppBuyMode.储值支付)
//            {
//                return Json(new { isok = -1, msg = "支付方式并非储值支付" }, JsonRequestBehavior.AllowGet);
//            }
//            var saveMoneyUser = _miniappsavemoneysetuserBll.getModelByUserId(umodel.AppId, userInfo.Id);

//            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
//            {
//                return Json(new { isok = -1, msg = "储值金额不足,请更换支付方式" }, JsonRequestBehavior.AllowGet);
//            }

//            //进入支付流程(此处因购物车记录已经更新,故无需再传入更新价格sql,传null)
//            if (payOrderBySaveMoneyUser(dbOrder, saveMoneyUser, null))
//            {
//                AfterPaySuccesExecFun(dbOrder);

//                return Json(new { isok = 1, msg = "支付成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
//            }
//            else
//            {
//                return Json(new { isok = -1, msg = "支付失败", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
//            }
//        }


//        ///// <summary>
//        ///// 使用储值退款
//        ///// </summary>
//        ///// <param name="dbOrder"></param>
//        ///// <param name="saveMoneyUser"></param>
//        ///// <returns></returns>
//        //[HttpPost]
//        //public ActionResult outOrderbySaveMoney(string appid, string openid, int goodsorderid)
//        //{

//        //    if (string.IsNullOrEmpty(appid))
//        //    {
//        //        return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//        //    if (umodel == null)
//        //    {
//        //        return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//        //    }

//        //    var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//        //    if (userInfo == null)
//        //    {
//        //        return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//        //    }

//        //    var dbOrder = _miniappFoodGoodsOrderBll.GetModel(goodsorderid);
//        //    if (userInfo.Id != dbOrder.UserId)
//        //    {
//        //        return Json(new { isok = -1, msg = "无操作此订单权限" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    if (dbOrder == null)
//        //    {
//        //        return Json(new { isok = -1, msg = "订单不存在" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    if (dbOrder.State != 0)
//        //    {
//        //        return Json(new { isok = -1, msg = "此订单不可进行支付" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    if (dbOrder.BuyMode != (int)miniAppBuyMode.储值支付)
//        //    {
//        //        return Json(new { isok = -1, msg = "支付方式并非储值支付" }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    var saveMoneyUser = _miniappsavemoneysetuserBll.getModelByUserId(umodel.AppId, userInfo.Id);

//        //    //进入退款流程
//        //    if (outOrderBySaveMoneyUser(dbOrder, saveMoneyUser))
//        //    {
//        //        return Json(new { isok = 1, msg = "支付成功", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
//        //    }
//        //    else
//        //    {
//        //        return Json(new { isok = -1, msg = "支付失败", postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id }, JsonRequestBehavior.AllowGet);
//        //    }
//        //}





//        //#endregion

//        /// <summary>
//        /// 更新订单状态
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="openid"></param>
//        /// <param name="orderId"></param>
//        /// <param name="State">OrderState</param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult updateMiniappGoodsOrderState(string appid, string openid, int orderId, int State)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }

//            var miniappFoodGoodOrder = _miniappFoodGoodsOrderBll.GetModel(orderId);
//            if (miniappFoodGoodOrder == null)
//            {
//                return Json(new { isok = -1, msg = "找不到订单" }, JsonRequestBehavior.AllowGet);
//            }

//            var userInfo = _userInfoBll.GetModelByAppId_OpenId(appid, openid);
//            if (userInfo == null)
//            {
//                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
//            }
//            if (miniappFoodGoodOrder.UserId != userInfo.Id)
//            {
//                return Json(new { isok = -1, msg = "无此订单操作权限" }, JsonRequestBehavior.AllowGet);
//            }

//            if (miniappFoodGoodOrder.OrderType == (int)miniAppFoodOrderType.堂食)
//            {
//                #region 堂食操作
//                switch (State)
//                {
//                    case (int)miniAppFoodOrderState.已取消:
//                        //当前订单状态是否允许被取消
//                        List<int> allowList1 = new List<int>();
//                        allowList1.Add((int)miniAppFoodOrderState.待付款);
//                        if (!allowList1.Contains(miniappFoodGoodOrder.State))
//                        {
//                            return Json(new { isok = -1, msg = "此订单不允许取消！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        break;
//                    case (int)miniAppFoodOrderState.付款中:
//                        //当前订单状态是否允许被取消
//                        List<int> allowList2 = new List<int>();
//                        allowList2.Add((int)miniAppFoodOrderState.待付款);
//                        if (!allowList2.Contains(miniappFoodGoodOrder.State))
//                        {
//                            return Json(new { isok = -1, msg = "此订单不允许修改为指定状态！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        break;
//                    case (int)miniAppFoodOrderState.待付款:
//                        //当前订单状态是否允许被取消
//                        List<int> allowList3 = new List<int>();
//                        allowList3.Add((int)miniAppFoodOrderState.付款中);
//                        if (!allowList3.Contains(miniappFoodGoodOrder.State))
//                        {
//                            return Json(new { isok = -1, msg = "此订单不允许修改为指定状态！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        break;
//                    default:
//                        return Json(new { isok = -1, msg = "订单状态错误,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
//                        break;
//                }


//                var updateColStr = "State";
//                switch (State)
//                {
//                    case (int)miniAppFoodOrderState.已取消:

//                        break;
//                    default:
//                        break;
//                }
//                var oldState = miniappFoodGoodOrder.State;
//                miniappFoodGoodOrder.State = State;

//                if (State == (int)miniAppFoodOrderState.已取消)
//                {
//                    if (_miniappFoodGoodsOrderBll.updateStock(miniappFoodGoodOrder, oldState))
//                    {
//                        _miniappFoodGoodsOrderLogBll.AddLog(miniappFoodGoodOrder.Id, userInfo.Id.ToString(), $" {Enum.GetName(typeof(miniAppFoodOrderState), State)} ");
//                        return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//                    }
//                }
//                else if (State == (int)miniAppFoodOrderState.已完成)
//                {
//                    TransactionModel tranModel = new TransactionModel();
//                    tranModel.Add(_miniappFoodGoodsOrderBll.BuildUpdateSql(miniappFoodGoodOrder, updateColStr) + $" and state = {oldState}  ");
//                    tranModel = _miniappFoodGoodsOrderBll.addSalesCount(miniappFoodGoodOrder.Id, tranModel);
//                    if (_miniappFoodGoodsOrderBll.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
//                    {
//                        //会员加消费金额
//                        if (!_miniappVipRelationBll.updatelevel(userInfo.Id, "food", miniappFoodGoodOrder.BuyPrice))
//                        {
//                            log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + miniappFoodGoodOrder.Id));
//                        }

//                        return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//                    }
//                }
//                else
//                {
//                    if (_miniappFoodGoodsOrderBll.updateFoodOrderState(miniappFoodGoodOrder, oldState, updateColStr))
//                    {
//                        return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//                    }
//                }
//                return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
//                #endregion
//            }
//            else
//            {
//                #region 外卖
//                switch (State)
//                {
//                    case (int)miniAppFoodOrderState.已取消:
//                        //当前订单状态是否允许被取消
//                        List<int> allowList1 = new List<int>();
//                        allowList1.Add((int)miniAppFoodOrderState.待付款);
//                        if (!allowList1.Contains(miniappFoodGoodOrder.State))
//                        {
//                            return Json(new { isok = -1, msg = "此订单不允许取消！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        break;
//                    case (int)miniAppFoodOrderState.付款中:
//                        //当前订单状态是否允许被取消
//                        List<int> allowList2 = new List<int>();
//                        allowList2.Add((int)miniAppFoodOrderState.待付款);
//                        if (!allowList2.Contains(miniappFoodGoodOrder.State))
//                        {
//                            return Json(new { isok = -1, msg = "此订单不允许修改为指定状态！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        break;
//                    case (int)miniAppFoodOrderState.待付款:
//                        //当前订单状态是否允许被取消
//                        List<int> allowList3 = new List<int>();
//                        allowList3.Add((int)miniAppFoodOrderState.付款中);
//                        if (!allowList3.Contains(miniappFoodGoodOrder.State))
//                        {
//                            return Json(new { isok = -1, msg = "此订单不允许修改为指定状态！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        break;
//                    case (int)miniAppFoodOrderState.退款审核中:
//                        //当前订单状态是否允许被退款
//                        List<int> allowList4 = new List<int>();
//                        allowList4.Add((int)miniAppFoodOrderState.待接单);
//                        allowList4.Add((int)miniAppFoodOrderState.待送餐);
//                        //allowList3.Add((int)C_Enums.miniAppFoodOrderState.待确认送达);
//                        if (!allowList4.Contains(miniappFoodGoodOrder.State))
//                        {
//                            return Json(new { isok = -1, msg = "此订单不允许退款！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        break;
//                    case (int)miniAppFoodOrderState.已完成:
//                        //当前订单状态是否允许完成
//                        List<int> allowList5 = new List<int>();
//                        allowList5.Add((int)miniAppFoodOrderState.待确认送达);
//                        if (!allowList5.Contains(miniappFoodGoodOrder.State))
//                        {
//                            return Json(new { isok = -1, msg = "此订单不允许完成！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        break;
//                    default:
//                        return Json(new { isok = -1, msg = "订单状态错误,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
//                        break;
//                }


//                var updateColStr = "State";
//                switch (State)
//                {
//                    case (int)miniAppFoodOrderState.已取消:

//                        break;
//                    case (int)miniAppFoodOrderState.付款中:

//                        break;
//                    case (int)miniAppFoodOrderState.待付款:

//                        break;
//                    case (int)miniAppFoodOrderState.退款审核中:
//                        miniappFoodGoodOrder.lastState = miniappFoodGoodOrder.State;  //记录订单申请退款前状态,以实现商家拒绝退款后继续走正常配送流程
//                        updateColStr += ",lastState";
//                        break;
//                    case (int)miniAppFoodOrderState.已完成:
//                        miniappFoodGoodOrder.AcceptDate = DateTime.Now;
//                        updateColStr += ",AcceptDate";
//                        break;
//                    default:
//                        break;
//                }
//                var oldState = miniappFoodGoodOrder.State;
//                miniappFoodGoodOrder.State = State;

//                if (State == (int)miniAppFoodOrderState.已取消)
//                {
//                    if (_miniappFoodGoodsOrderBll.updateStock(miniappFoodGoodOrder, oldState))
//                    {
//                        _miniappFoodGoodsOrderLogBll.AddLog(miniappFoodGoodOrder.Id, userInfo.Id.ToString(), $" {Enum.GetName(typeof(miniAppFoodOrderState), State)} ");
//                        return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//                    }
//                }
//                else if (State == (int)miniAppFoodOrderState.已完成)
//                {
//                    TransactionModel tranModel = new TransactionModel();
//                    tranModel.Add(_miniappFoodGoodsOrderBll.BuildUpdateSql(miniappFoodGoodOrder, updateColStr) + $" and state = {oldState}  ");
//                    tranModel = _miniappFoodGoodsOrderBll.addSalesCount(miniappFoodGoodOrder.Id, tranModel);
//                    if (_miniappFoodGoodsOrderBll.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
//                    {
//                        ////模板消息不需再发送
//                        //_miniapptemplatemsg_userlogBll.confTemplateMsg(miniappFoodGoodOrder.Id, 8);
//                        //会员加消费金额
//                        if (!_miniappVipRelationBll.updatelevel(userInfo.Id, "food"))
//                        {
//                            log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常,订单id" + miniappFoodGoodOrder.Id));
//                        }
//                        return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//                    }
//                }
//                else
//                {
//                    if (_miniappFoodGoodsOrderBll.updateFoodOrderState(miniappFoodGoodOrder, oldState, updateColStr))
//                    {
//                        if (State == (int)miniAppFoodOrderState.退款审核中)
//                        {
//                            #region 发送餐饮退款申请通知 模板消息
//                            string msg = string.Empty;
//                            msg += "1";
//                            var userinfo = _userInfoBll.GetModel(miniappFoodGoodOrder.UserId) ?? new C_UserInfo();
//                            var tmgup = _miniapptemplatemsg_userParamBll.getParamByAppIdOpenId(umodel.AppId, userinfo.OpenId) ?? new TemplateMsg_UserParam();
//                            var tmgu = _miniapptemplatemsg_userBll.getModelByAppIdTypeId(tmgup.AppId, (int)TmpType.小程序餐饮模板, (int)SendTemplateMessageTypeEnum.餐饮退款申请通知) ?? new TemplateMsg_User();

//                            msg += "2";
//                            if (tmgu.State == 1)
//                            {
//                                msg += "3";
//                                var postData = _miniappFoodGoodsOrderBll.getTemplateMessageData(miniappFoodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮退款申请通知);
//                                _msnModelHelper.sendMyMsn(userinfo.appId, userinfo.OpenId, tmgu.TemplateId, tmgu.PageUrl, tmgup.Form_Id, postData, string.Empty, string.Empty, ref msg);

//                                //参数使用次数增加(默认是1)
//                                _miniapptemplatemsg_userParamBll.addUsingCount(tmgup);
//                            }
//                            msg += "4";
//                            #endregion

//                            #region 发送模板消息通知商家
//                            var food = _miniappFoodBll.GetModel(miniappFoodGoodOrder.StoreId) ?? new Food();
//                            var app = new XcxAppAccountRelationBLL().GetModel(food.appId) ?? new XcxAppAccountRelation();
//                            var account = _accountBll.GetModel(umodel.AccountId) ?? new Account();
//                            //var paramslist = _miniappparamBll.GetListByRId(Convert.ToInt32(umodel.Id));
//                            //var storeName = paramslist.Where(w => w.Param == "nparam").FirstOrDefault()?.Value;
//                            var goodCartList = _miniappFoodGoodsCartBll.GetList($" GoodsOrderId = {miniappFoodGoodOrder.Id} ") ?? new List<FoodGoodsCart>();
//                            goodCartList.ForEach(x =>
//                            {
//                                x.goodsMsg = _miniappFoodGoodsBll.GetModel(x.GoodsOrderId);
//                            });

//                            var title = $"您的店铺【{food.FoodsName}】有顾客申请退款,请及时处理！";
//                            var remark1 = $" 该订单于 {miniappFoodGoodOrder.CreateDate.ToString("yyyy-MM-dd HH:mm")} 下单. ";
//                            var userBaseInfo = new UserBaseInfoBLL().GetModelByUnionidServerid(account.UnionId, WebSiteConfig.DZ_WxSerId);
//                            //log4net.LogHelper.WriteInfo(GetType(), $" 用户获取到了,{userBaseInfo != null}.公众号,{WebSiteConfig.DZ_WxSerId},OpenId:{userBaseInfo?.openid} ");
//                            if (userBaseInfo != null && !string.IsNullOrWhiteSpace(userBaseInfo.openid))
//                            {
//                                //提醒用户发送消息成功
//                                C_TplMsgHelper.SendTplMsgFromVzan_OutOrderApply(userBaseInfo.openid, "", title, miniappFoodGoodOrder.OrderNum, miniappFoodGoodOrder.BuyPriceStr + " 元", miniappFoodGoodOrder.AccepterName, string.Join(",", goodCartList?.Select(x => x.goodsMsg?.GoodsName)), goodCartList.Sum(x => x.Count), remark1);
//                            }

//                            #endregion
//                        }

//                        return Json(new { isok = 1, msg = "成功" }, JsonRequestBehavior.AllowGet);
//                    }
//                }
//                return Json(new { isok = -1, msg = "失败" }, JsonRequestBehavior.AllowGet);
//                #endregion
//            }
//        }
//        #endregion



//        #region 易联云订单打印状态推送回调
//        [HttpPost]
//        public ActionResult GetPrintStatus()
//        {
//            string sign = Utility.IO.Context.GetRequest("sign", string.Empty);
//            int time = Utility.IO.Context.GetRequestInt("time", 0);
//            string apikey = Utility.IO.Context.GetRequest("apikey", string.Empty);
//            if (string.IsNullOrEmpty(sign) || time == 0 || string.IsNullOrEmpty(apikey)) return View();
//            if (sign != FoodYiLianYunPrintHelper.GetMD5Hash(apikey + time)) return View();
//            string dataid = Utility.IO.Context.GetRequest("dataid", string.Empty);
//            string machine_code = Utility.IO.Context.GetRequest("machine_code", string.Empty);
//            int printtime = Utility.IO.Context.GetRequestInt("printtime", 0);
//            int state = Utility.IO.Context.GetRequestInt("state", 0);
//            string cmd = Utility.IO.Context.GetRequest("cmd", string.Empty);
//            FoodOrderPrintLog log = _miniappFoodOrderPrintLogBll.GetModel($"dataid='{dataid}' and machine_code='{machine_code}'");
//            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
//            DateTime dt = startTime.AddSeconds(printtime);
//            string remark = string.Empty;
//            if (state == 1) remark = "打印成功";
//            else remark = "打印失败";
//            if (log == null)
//            {
//                log = new FoodOrderPrintLog()
//                {
//                    Dataid = dataid,
//                    addtime = dt,
//                    machine_code = machine_code,
//                    state = state,
//                    isupdate = 0,
//                    cmd = cmd,
//                    remark = remark
//                };
//                _miniappFoodOrderPrintLogBll.Add(log);
//            }
//            else
//            {
//                log.Dataid = dataid;
//                log.addtime = dt;
//                log.machine_code = machine_code;
//                log.state = state;
//                log.isupdate = 1;
//                log.cmd = cmd;
//                log.remark = remark;
//                _miniappFoodOrderPrintLogBll.Update(log);
//            }
//            return View();
//        }
//        #endregion

//        /// <summary>
//        /// 判定商家是否配送范围外
//        /// </summary>
//        /// <param name="appid"></param>
//        /// <param name="lat"></param>
//        /// <param name="lng"></param>
//        /// <returns></returns>
//        public ActionResult GetDistanceForFood(string appid, double lat, double lng)
//        {
//            if (string.IsNullOrEmpty(appid))
//            {
//                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
//            }
//            var umodel = _xcxappaccountrelationBll.GetModelByAppid(appid);
//            if (umodel == null)
//            {
//                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
//            }
//            var miniappFood = _miniappFoodBll.GetModel($"appId={umodel.Id}");
//            if (miniappFood == null)
//            {
//                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
//            }
//            var distance = GetDistance(miniappFood.Lat, miniappFood.Lng, lat, lng);

//            if (miniappFood.DeliveryRange >= distance)
//            {
//                return Json(new { isok = 1, msg = "配送范围内", distance = distance, DeliveryRange = miniappFood.DeliveryRange }, JsonRequestBehavior.AllowGet);
//            }
//            else
//            {
//                return Json(new { isok = -1, msg = "配送范围外", distance = distance, DeliveryRange = miniappFood.DeliveryRange }, JsonRequestBehavior.AllowGet);
//            }

//        }

//        /// <summary>
//        /// 根据地址下标,获取地址名称
//        /// </summary>
//        /// <param name="lat"></param>
//        /// <param name="lng"></param>
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult getTXMapAddress(double lat, double lng)
//        {
//            var result = HttpGet($"http://apis.map.qq.com/ws/geocoder/v1/?location={lat},{lng}&key={WebSiteConfig.Tx_MapKey}");

//            var txMap = JsonConvert.DeserializeObject<txMap>(result);
//            return Json(new { isok = 1, msg = "成功", address = txMap.result.address }, JsonRequestBehavior.AllowGet);
//        }

//        public class txMap
//        {
//            public int status { get; set; }

//            public string message { get; set; }

//            public string request_id { get; set; }

//            public txMapResult result { get; set; }


//        }

//        public class txMapResult
//        {

//            public string address { get; set; }
//        }

//        public string HttpGet(string Url, int timeOut = 6000)
//        {
//            try
//            {
//                return  HttpHelper.GetData (Url, timeOut);
//            }
//            catch (Exception ex)
//            {
//                log4net.LogHelper.WriteError(typeof(MsnModelHelper), new Exception(ex.Message + " 请求地址出错：" + Url));
//                return "";
//            }
//        }




//        private double rad(double d)
//        {
//            return d * Math.PI / 180.0;
//        }

//        /// <summary>
//        /// 腾讯地图,返回距离(公里) 
//        /// 有误差,同腾讯地图api 8公里 误差在0.1米以内
//        /// </summary>
//        /// <param name="lat1"></param>
//        /// <param name="lng1"></param>
//        /// <param name="lat2"></param>
//        /// <param name="lng2"></param>
//        /// <returns></returns>
//        public double GetDistance(double lat1, double lng1, double lat2, double lng2)
//        {
//            double EARTH_RADIUS = 6378.137;
//            double radLat1 = rad(lat1);
//            double radLat2 = rad(lat2);
//            double a = radLat1 - radLat2;
//            double b = rad(lng1) - rad(lng2);
//            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
//             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
//            s = s * EARTH_RADIUS;
//            s = Math.Round(s * 10000) / 10000;
//            return s;
//        }


//    }
//}