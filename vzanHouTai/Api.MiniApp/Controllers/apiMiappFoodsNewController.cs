#region 引用命名空间
using Api.MiniApp.Controllers;
using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Utility;
using Utility.IO;

#endregion

namespace Api.MiniSNS.Controllers
{
    /// <summary>
    /// 餐饮版版本号：xf1.1.3之后用此接口 增加了缓存，返回值做了规范
    /// </summary>
    [ExceptionLog]
    public class apiMiappFoodsNewController : InheritController
    {
        private readonly static object _couponLock = new object();
        public static readonly ConcurrentDictionary<int, object> lockObjectDict_Order = new ConcurrentDictionary<int, object>();
        public apiMiappFoodsNewController()
        {
            
        }

        /// <summary>
        /// 获取店铺详情信息接口
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetFoodsDetail(string appid, string openid = null)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = $"没有数据{xcxAccountRelation.AppId},id{xcxAccountRelation.Id}" }, JsonRequestBehavior.AllowGet);
            }

            //店铺Logo
            string Logo = string.Empty;
            string key = string.Format(FoodBLL.SingleModel.foodAttImgKey, foodStore.Id, (int)AttachmentItemType.小程序餐饮店铺Logo);
            List<C_Attachment> imgs = RedisUtil.Get<List<C_Attachment>>(key);
            //优先从缓存里获取图片
            if (imgs != null && imgs.Count > 0)
            {
                Logo = imgs[0].filepath;
            }
            else
            {
                List<C_Attachment> LogoList = C_AttachmentBLL.SingleModel.GetListByCache(foodStore.Id, (int)AttachmentItemType.小程序餐饮店铺Logo);
                if (LogoList != null && LogoList.Count > 0)
                {
                    Logo = LogoList[0].filepath;
                    RedisUtil.Set(key, LogoList);
                }
            }

            //店铺Logo
            string logo = string.Empty;
            List<C_Attachment> logoList = C_AttachmentBLL.SingleModel.GetListByCache(foodStore.Id, (int)AttachmentItemType.小程序餐饮店铺Logo);
            if (logoList != null && logoList.Count > 0)
            {
                logo = logoList[0].filepath;
            }

            //门店照片
            List<string> storeImgUrls = new List<string>();
            List<C_Attachment> storeImgs = C_AttachmentBLL.SingleModel.GetListByCache(foodStore.Id, (int)AttachmentItemType.小程序餐饮门店图片);
            if (storeImgs != null && storeImgs.Count > 0)
            {
                storeImgs.ForEach(img =>
                {
                    storeImgUrls.Add(img.filepath);
                });
            }

            //轮播图
            List<string> sliderImgUrls = new List<string>();
            List<C_Attachment> sliderImgs = C_AttachmentBLL.SingleModel.GetListByCache(foodStore.Id, (int)AttachmentItemType.小程序餐饮店铺轮播图);
            if (sliderImgs != null && sliderImgs.Count > 0)
            {
                sliderImgs.ForEach(img =>
                {
                    sliderImgUrls.Add(img.filepath);
                });
            }


            #region 图片裁剪  由于裁剪导致前端图片显示不全 先去掉
            //logo
            //if (!string.IsNullOrEmpty(logo))
            //{
            //    logo = ImgHelper.ResizeImg(logo, 120, 120);
            //}
            ////门店照片
            //if (storeImgUrls != null && storeImgUrls.Count > 0)
            //{
            //    storeImgUrls.ForEach(storeImg => { ImgHelper.ResizeImg(storeImg, 640, 0); });
            //}
            ////轮播图
            //if (sliderImgUrls != null && sliderImgUrls.Count > 0)
            //{
            //    sliderImgUrls.ForEach(sliderImg => { ImgHelper.ResizeImg(sliderImg, 640, 0); });
            //}
            #endregion

            var reserveId = 0;
            if (!string.IsNullOrWhiteSpace(openid))
            {
                var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
                var reservation = FoodReservationBLL.SingleModel.GetUnfinishReservation(foodId: foodStore.Id, userId: userInfo.Id);
                reserveId = reservation != null ? reservation.Id : 0;
            }

            var postdata = new { Logo = Logo, food = foodStore, storeImgs = storeImgUrls, sliderImgUrls = sliderImgUrls, reserveId = reserveId };

            return Json(new { isok = true, msg = "成功", data = postdata }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取菜品列表接口 根据条件
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="typeid">菜品类别</param>
        /// <param name="goodsName">菜品名称</param>
        /// <param name="shopType">购买方式(0点餐/1外卖)</param>
        /// <param name="pageindex">当前页</param>
        /// <param name="pagesize">每页数量</param>
        /// <param name="orderbyid">排序</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetGoodsList(string appid, int typeid = 0, string goodsName = "", int shopType = 10, int pageindex = 1, int pagesize = 10, int orderbyid = 0)
        {
            #region 数据校验
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }

            List<FoodGoods> goodslist = FoodGoodsBLL.SingleModel.GetListByParam(foodStore.Id, typeid, goodsName, pageindex, pagesize, orderbyid, shopType);
            if (goodslist == null || goodslist.Count <= 0)
            {
                return Json(new { isok = true, msg = "成功", data = new { goodslist = goodslist } }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            
            //获取商品标签
            goodslist.ForEach(goods =>
            {
                if (!string.IsNullOrWhiteSpace(goods.labelIdStr))
                {
                    string[] labelIds = goods.labelIdStr.Split(',').Where(labelId => !string.IsNullOrWhiteSpace(labelId)).ToArray();
                    if (labelIds != null && labelIds.Any())
                    {
                        List<FoodLabel> labels = FoodLabelBLL.SingleModel.GetListByLabelIds(string.Join(",", labelIds));
                        if (labels != null && labels.Count > 0)
                        {
                            goods.labelNameStr = string.Join(",", labels.Select(label => label.LabelName));
                        }
                    }
                }
                //图片裁剪
                goods.ImgUrl = ImgHelper.ResizeImg(goods.ImgUrl, 600, 0);
            });

            //计算会员折扣
            int userLevelId = Context.GetRequestInt("levelid", 0);
            VipLevel userLevel = VipLevelBLL.SingleModel.GetModel($"id={userLevelId} and state>=0");

            VipLevelBLL.SingleModel.CalculateVipGoodsPrice(goodslist, userLevel, true);

            //拼装组合数据
            var postdata = new
            {
                goodslist = FoodGoodsSpecBLL.SingleModel.GetGoodsAttrSpaceByGoodsIds(goodslist)

            };
            return Json(new { isok = true, msg = "成功", data = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取店铺类别列表
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetGoodsTypeList(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }

            List<FoodGoodsType> goodsTypeList = FoodGoodsTypeBLL.SingleModel.GetlistByFoodId(foodStore.Id);

            var postdata = new
            {
                goodsTypeList = goodsTypeList,

            };
            return Json(new { isok = true, msg = "成功", data = postdata }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取菜品列表接口 根据条件
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="typeid">菜品类别</param>
        /// <param name="goodsName">菜品名称</param>
        /// <param name="shopType">购买方式(0点餐/1外卖)</param>
        /// <param name="pageindex">当前页</param>
        /// <param name="pagesize">每页数量</param>
        /// <param name="orderbyid">排序</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetGoodsDtl(string appid, int goodsid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            FoodGoods goods = FoodGoodsBLL.SingleModel.GetModel(goodsid);
            List<string> labelIds = goods.labelIdStr.Split(',').Where(labelId => !string.IsNullOrWhiteSpace(labelId)).ToList();
            if (labelIds != null && labelIds.Any())
            {
                List<FoodLabel> labelNames = FoodLabelBLL.SingleModel.GetList($" Id in ({string.Join(",", labelIds)}) ");
                goods.labelNameStr = string.Join(",", labelNames.Select(labelName => labelName.LabelName));
            }
            if (goods == null)
            {
                return Json(new { isok = false, msg = "菜品信息有误" }, JsonRequestBehavior.AllowGet);
            }

            // 会员打折
            int userLevelId = Utility.IO.Context.GetRequestInt("levelid", 0);
            VipLevel userLevel = VipLevelBLL.SingleModel.GetModel($"id={userLevelId} and state>=0");

            VipLevelBLL.SingleModel.CalculateVipGoodPrice(goods, userLevel);

            List<FoodGoods> goodslist = new List<FoodGoods>();
            goodslist.Add(goods);
            //规格 / 属性 名称
            var postdata = new
            {
                goodslist = FoodGoodsSpecBLL.SingleModel.GetGoodsAttrSpaceByGoodsIds(goodslist)
            };
            return Json(new { isok = true, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        #region 收货地址相关接口

        /// <summary>
        /// 根据代码获取省市区 默认 北京市
        /// </summary>
        /// <param name="parentCode"></param>
        /// <param name="currentCode"></param>
        /// <returns></returns>
        public ActionResult GetRegionJsonList(int parentCode = 110000, int currentCode = 0)
        {
            string sql = "SELECT * from c_area where parentCode = " + parentCode;
            var regionList = C_AreaBLL.SingleModel.GetListBySql(sql).Select(m => new
            {
                Value = m.Code,
                Text = m.Name,
                Selected = m.Code == currentCode
            });
            return Json(new { data = regionList }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        ///  添加/编辑收货地址
        /// </summary>
        /// <param name="State"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddOrEditMyAddressDefault(string appid, string openid, string addressjson)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }

            FoodAddress address = JsonConvert.DeserializeObject<FoodAddress>(addressjson);
            if (address == null)
            {
                return Json(new { isok = false, msg = "地址不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (address.Lat <= 0 || address.Lng <= 0)
            {
                return Json(new { isok = false, msg = "定位坐标有误,请重试！" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrWhiteSpace(address.TelePhone))
            {
                return Json(new { isok = false, msg = "联系人号码不能为空！" }, JsonRequestBehavior.AllowGet);
            }
            if (!Regex.IsMatch(address.TelePhone, @"^[1]+[3-9]+\d{9}$"))
            {
                return Json(new { isok = false, msg = "联系人号码有误！" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(address.Address))
            {
                return Json(new { isok = false, msg = "请填写详细地址" }, JsonRequestBehavior.AllowGet);
            }
            if (address.Id <= 0)
            {
                address.FoodId = foodStore.Id;
                address.UserId = userInfo.Id;
                address.CreateDate = DateTime.Now;

                address.Id = Convert.ToInt32(FoodAddressBLL.SingleModel.Add(address));
                if (address.Id <= 0)
                {
                    return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                FoodAddress model = FoodAddressBLL.SingleModel.GetModel(address.Id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "保存失败" }, JsonRequestBehavior.AllowGet);
                }
                model.NickName = address.NickName;
                model.TelePhone = address.TelePhone;
                model.Province = address.Province;
                model.CityCode = address.CityCode;
                model.AreaCode = address.AreaCode;
                model.Address = address.Address;
                model.Lat = address.Lat;
                model.Lng = address.Lng;
                if (!FoodAddressBLL.SingleModel.Update(model, "NickName,TelePhone,Province,CityCode,AreaCode,Address,Lat,Lng"))
                {
                    return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  获取我的收货地址
        /// </summary>
        /// isDefault : 0为否,1 为是
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMyAddress(string appid, string openid, int addressId = 0, int isDefault = 0)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }

            //返回默认地址
            if (isDefault == 1)
            {
                FoodAddress address = FoodAddressBLL.SingleModel.GetModel($" FoodId = {foodStore.Id} and UserId = {userInfo.Id} and State = 0 and IsDefault = 1 ");
                if (address == null)
                {
                    return Json(new { isok = false, msg = "地址信息不存在" }, JsonRequestBehavior.AllowGet);
                }
                //拼接地址名称
                address.Address_Dtl = $"{address.Province} {address.CityCode} {address.AreaCode} {address.Address}";

                var postdata = new { address = address };
                return Json(new { isok = true, msg = "成功", data = postdata }, JsonRequestBehavior.AllowGet);
            }


            if (addressId == 0)
            {
                List<FoodAddress> addressList = FoodAddressBLL.SingleModel.GetList($" FoodId = {foodStore.Id} and UserId = {userInfo.Id} and State = 0 ");
                //拼接地址名称
                addressList.ForEach(address =>
                {
                    address.Address_Dtl = $"{address.Province} {address.CityCode} {address.AreaCode} {address.Address}";
                });



                var postdata = new { addressList = addressList.Select(s => new { s.Address, s.Id, s.Province, s.CityCode, s.AreaCode, s.NickName, s.TelePhone, IsDefault = (s.IsDefault == 0 ? "正常" : "已删除") }) };

                return Json(new { isok = true, msg = "成功", data = postdata }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                FoodAddress address = FoodAddressBLL.SingleModel.GetModel(addressId);
                if (address == null)
                {
                    return Json(new { isok = false, msg = "地址信息不存在" }, JsonRequestBehavior.AllowGet);
                }
                //拼接地址名称
                address.Address_Dtl = $"{address.Province} {address.CityCode} {address.AreaCode} {address.Address}";

                var postdata = new { address = address };
                return Json(new { isok = true, msg = "成功", data = postdata }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///  设定默认收货地址
        /// </summary>
        /// <param name="State"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult setMyAddressDefault(string appid, string openid, int addressId)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            FoodAddress address = FoodAddressBLL.SingleModel.GetModel(addressId);
            if (address == null)
            {
                return Json(new { isok = false, msg = "地址信息不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (!FoodAddressBLL.SingleModel.SetDefault(addressId, userInfo.Id))
            {
                return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///  删除我的收货地址
        /// </summary>
        /// <param name="State"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteMyAddress(string appid, string openid, int AddressId)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            FoodAddress address = FoodAddressBLL.SingleModel.GetModel(AddressId);
            if (address == null)
            {
                return Json(new { isok = false, msg = "地址信息错误" }, JsonRequestBehavior.AllowGet);
            }
            address.State = -1;

            if (!FoodAddressBLL.SingleModel.Update(address, "State"))
            {
                return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// 根据所选购买菜品 配上 运费模板 计算运费
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getOrderGoodsBuyPriceByCarIds(string appid, string openid, string goodCarIdStr)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(goodCarIdStr))
            {
                return Json(new { isok = false, msg = "购物车异常" }, JsonRequestBehavior.AllowGet);
            }
            List<string> goodCarIdList = goodCarIdStr.Split(',').ToList();
            if (goodCarIdStr.Substring(goodCarIdStr.Length - 1, 1) == ",")
            {
                goodCarIdList = goodCarIdStr.Substring(0, goodCarIdStr.Length - 1).Split(',').ToList();
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($" appid = {xcxAccountRelation.Id} ");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            List<FoodGoodsCart> goodsCars = FoodGoodsCartBLL.SingleModel.GetList($" Id in({string.Join(",", goodCarIdList)}) and UserId = {userInfo.Id} ");
            if (goodsCars == null || !goodsCars.Any())
            {
                return Json(new { isok = false, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
            }

            List<FoodFreightTemplate> freightTemplates = FoodFreightTemplateBLL.SingleModel.GetList($" StoreId = {foodStore.Id} ");
            if (freightTemplates == null || !freightTemplates.Any())
            {
                return Json(new { isok = true, msg = "商家无设定运费模板" }, JsonRequestBehavior.AllowGet);
            }
            //购买价格计算
            int qtySum = goodsCars.Sum(goodsCar => goodsCar.Count);
            freightTemplates.ForEach(template =>
            {
                int friPrice = 0;
                if (qtySum <= template.BaseCount)
                {
                    friPrice = template.BaseCost;
                }
                else
                {
                    //初阶费用加上额外费用
                    friPrice = template.BaseCost + (qtySum - template.BaseCount) * template.ExtraCost;
                }
                //临时存放模板所需运费
                template.sum = (friPrice * 0.01).ToString();
            });

            var postdata = freightTemplates.Select(template => new { template.Id, template.Name, template.sum });

            return Json(new { isok = true, msg = "成功", data = postdata }, JsonRequestBehavior.AllowGet);
        }





        /// <summary>
        /// 查询购物车指定记录
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="orderbyid"></param>
        /// <returns></returns>
        public ActionResult getGoodsCarDataByIds(string appid, string openid, List<int> goodsCarList)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }


            List<FoodGoodsCart> myCartList = FoodGoodsCartBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCarList)}) and UserInfo = {userInfo.Id} ");
            if (myCartList == null || !myCartList.Any())
            {
                return Json(new { isok = false, msg = "没有找到记录" }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { isok = true, msg = "成功", data = myCartList }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 查询购物车
        /// </summary>
        /// <param name="appid">wx95a525ef2e44492f</param>
        /// <param name="openid">oaPY9wWfdfUtGTK2xuxoX58QjYmk</param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult getGoodsCarData(string appid, string openid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            List<FoodGoods> goodsList = new List<FoodGoods>();
            List<int> typeIds = new List<int>();
            List<FoodGoodsType> typeList = new List<FoodGoodsType>();
            List<FoodGoodsCart> myCartList = FoodGoodsCartBLL.SingleModel.GetMyCart(foodStore.Id, userInfo.Id);

            //会员打折
            int userLevelId = Utility.IO.Context.GetRequestInt("levelid", 0);
            VipLevel userLevel = VipLevelBLL.SingleModel.GetModel($"id={userLevelId} and state>=0");
            VipLevelBLL.SingleModel.CalculateVipGoodsCartPrice(myCartList, userLevel);

            var postdata = new { GoodsCar = myCartList };
            return Json(new { isok = true, msg = "成功", data = new { postdata = postdata, myCartList = myCartList } }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 添加菜品至购物车
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodid"></param>
        /// <param name="attrSpacStr">1_5;2_7</param>
        /// <param name="SpecInfo">菜品属性格式:颜色:白色;尺码:M;</param>
        /// <param name="GoodsNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodsCarData(string appid, string openid, int goodid, string attrSpacStr, string SpecInfo, int GoodsNumber, int lat = 0, int lng = 0, int newCartRecord = 0)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            if (lat != 0 || lng != 0)
            {
                if (GetDistance(foodStore.Lat, foodStore.Lng, lat, lng) > foodStore.DeliveryRange)
                {
                    return Json(new { isok = false, msg = "不在配送范围" }, JsonRequestBehavior.AllowGet);
                }
            }
            if (GoodsNumber <= 0)
            {
                return Json(new { isok = false, msg = "数量必须大于0" }, JsonRequestBehavior.AllowGet);
            }
            FoodGoods goods = FoodGoodsBLL.SingleModel.GetModel(goodid);
            if (goods == null)
            {
                return Json(new { isok = false, msg = "未找到该菜品" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }

            FoodGoodsCart dbGoodCar = FoodGoodsCartBLL.SingleModel.GetModel($" UserId={userInfo.Id} and FoodGoodsId={goods.Id} and SpecIds='{attrSpacStr}' and State = 0 ");
            if (dbGoodCar == null || newCartRecord == 1)
            {
                FoodGoodsCart goodsCar = new FoodGoodsCart();
                goodsCar.FoodId = foodStore.Id;
                goodsCar.FoodGoodsId = goods.Id;
                goodsCar.SpecIds = attrSpacStr;
                goodsCar.Count = GoodsNumber;
                if (!string.IsNullOrWhiteSpace(attrSpacStr))
                {
                    goodsCar.Price = goods.GASDetailList.Where(GASDetail => GASDetail.id.Equals(attrSpacStr)).First().price;
                }
                else
                {
                    goodsCar.Price = goods.Price;
                }
                goodsCar.SpecInfo = SpecInfo;
                goodsCar.UserId = userInfo.Id;
                goodsCar.CreateDate = DateTime.Now;
                goodsCar.State = 0;//加入购物车

                goodsCar.Id = Convert.ToInt32(FoodGoodsCartBLL.SingleModel.Add(goodsCar));
                if (goodsCar.Id > 0)
                {
                    return Json(new { isok = true, msg = "成功", data = new { cartid = goodsCar.Id } }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = false, msg = "失败", data = new { cartid = 0 } }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                dbGoodCar.Count += GoodsNumber;
                if (FoodGoodsCartBLL.SingleModel.Update(dbGoodCar))
                {
                    return Json(new { isok = true, msg = "成功", data = new { cardid = dbGoodCar.Id } }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// 删除或更新购物车
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="State">0为更新,-1为删除</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult updateOrDeleteGoodsCarData(string appid, string openid, List<FoodGoodsCart> goodsCarModel, int State)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (goodsCarModel != null && goodsCarModel.Count > 0)
            {
                string cartIds = string.Join(",",goodsCarModel.Select(s=>s.Id));
                List<FoodGoodsCart> foodGoodsCartList = FoodGoodsCartBLL.SingleModel.GetListByIds(cartIds);
                foreach (FoodGoodsCart item in goodsCarModel)
                {
                    FoodGoodsCart goodsCar = foodGoodsCartList?.FirstOrDefault(f=>f.Id == item.Id);
                    if (goodsCar == null)
                    {
                        return Json(new { isok = false, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
                    }
                    if (goodsCar.UserId != userInfo.Id)
                    {
                        return Json(new { isok = false, msg = "该记录不属于当前用户" }, JsonRequestBehavior.AllowGet);
                    }

                    //将记录状态改为删除
                    if (State == -1)
                    {
                        goodsCar.State = -1;
                    }
                    else if (State == 0)//根据传入参数更新购物车内容
                    {
                        goodsCar.SpecIds = item.SpecIds;
                        goodsCar.SpecInfo = item.SpecInfo;
                        goodsCar.Count = item.Count;


                        //价格因更改规格随之改变
                        FoodGoods carGoods = FoodGoodsBLL.SingleModel.GetModel(goodsCar.FoodGoodsId);
                        if (carGoods == null)
                        {
                            goodsCar.GoodsState = 2;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(carGoods.AttrDetail))
                            {
                                int? price = carGoods.GASDetailList.Where(x => x.id.Equals(goodsCar.SpecIds))?.FirstOrDefault()?.price;
                                if (price != null)
                                {
                                    goodsCar.Price = Convert.ToInt32(price);
                                }
                            }
                        }
                    }

                    bool success = FoodGoodsCartBLL.SingleModel.Update(goodsCar, "State,SpecIds,SpecInfo,Count,Price,GoodsState");
                    if (!success)
                    {
                        return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }



        #region 餐饮订单
        /// <summary>
        /// 获取订单信息列表
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="State">OrderState</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getMiniappGoodsOrder(string appid, string openid, int State = 10, int pageIndex = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }


            string stateSql = (State != 10 ? $" and State = {State} " : "");

            try
            {

                List<FoodGoodsOrder> goodsOrderList = FoodGoodsOrderBLL.SingleModel.GetList($" StoreId = {foodStore.Id} and UserId = {userInfo.Id} { stateSql } and State != {(int)miniAppFoodOrderState.付款中} ", pageSize, pageIndex, "*", "CreateDate desc");

                if (goodsOrderList != null && goodsOrderList.Any())
                {
                    //获取达达订单状态
                    _dadaOrderBLL.GetFoodDadaOrderState<FoodGoodsOrder>(ref goodsOrderList, xcxAccountRelation.Id);

                    //订单详情
                    List<FoodGoodsCart> goodsOrderDtlList = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId in ({string.Join(",", goodsOrderList.Select(goodsOrder => goodsOrder.Id))}) ");
                    if (goodsOrderDtlList != null && goodsOrderList.Any())
                    {
                        List<FoodGoods> goodsList = FoodGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsOrderDtlList.Select(x => x.FoodGoodsId))}) ");
                        goodsOrderDtlList.ForEach(goodsOrderDtl =>
                        {
                            goodsOrderDtl.goodsMsg = goodsList.Where(goods => goods.Id == goodsOrderDtl.FoodGoodsId).FirstOrDefault();
                        });
                        var postdata = goodsOrderList.GroupBy(goodsOrder => goodsOrder.CreateDate.Year).Select(goodsOrder =>
                           new
                           {
                               year = goodsOrder.Key,
                               orderList = goodsOrder.OrderByDescending(o => o.CreateDate).Select(order =>
                                 new
                                 {
                                     Id = order.Id,
                                     Title = goodsOrderDtlList.Where(goodsOrderDtl => goodsOrderDtl.GoodsOrderId == order.Id).FirstOrDefault()?.goodsMsg?.GoodsName,
                                     ImgUrl = goodsOrderDtlList.Where(goodsOrderDtl => goodsOrderDtl.GoodsOrderId == order.Id).FirstOrDefault()?.goodsMsg?.ImgUrl,
                                     Count = goodsOrderDtlList.Where(goodsOrderDtl => goodsOrderDtl.GoodsOrderId == order.Id).Count(),
                                     StateTitle = Enum.GetName(typeof(miniAppFoodOrderState), order.State),
                                     State = order.State,
                                     CreateDate = order.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                     BuyPrice = order.BuyPrice * 0.01,
                                     cityMorderId = order.OrderId,
                                     GoodsDtlName = string.Join(" + ", goodsOrderDtlList.Where(goodsOrderDtl => goodsOrderDtl.GoodsOrderId == order.Id).Select(goodsOrderDtl => goodsOrderDtl.goodsMsg.GoodsName)),
                                     OrderTypeStr = order.OrderType == (int)miniAppFoodOrderType.堂食 ? "堂食" : "外卖",
                                     BuyMode = order.BuyMode,
                                     order.GetWay,
                                     order.DadaOrderStateStr,
                                 })
                           });
                        return Json(new { isok = true, msg = "成功", data = postdata }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
                log4net.LogHelper.WriteInfo(GetType(), ex.Message + "|" + $" StoreId = {foodStore.Id} and UserId = {userInfo.Id} { stateSql } ");
                return Json(new { isok = false, msg = "失败", data = "" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = true, msg = "成功", data = "" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="State">OrderState</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getMiniappGoodsOrderById(string appid, string openid, int orderId)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            FoodGoodsOrder goodsOrder = FoodGoodsOrderBLL.SingleModel.GetModel($" Id = {orderId} and StoreId = {foodStore.Id} and UserId = {userInfo.Id}");
            if (goodsOrder == null)
            {
                return Json(new { isok = false, msg = "订单信息不存在" }, JsonRequestBehavior.AllowGet);
            }

            //达达订单状态查询
            _dadaOrderBLL.GetFoodDadaOrderDetailState<FoodGoodsOrder>(ref goodsOrder, xcxAccountRelation.Id);

            List<FoodGoodsCart> goodsOrderDtl = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {goodsOrder.Id} ");
            List<FoodGoods> goodsList = FoodGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsOrderDtl.Select(order => order.FoodGoodsId))}) ");
            if (string.IsNullOrEmpty(goodsOrder.attribute))
            {
                goodsOrder.attrbuteModel = new FoodGoodsOrderAttr();
            }
            else
            {
                goodsOrder.attrbuteModel = JsonConvert.DeserializeObject<FoodGoodsOrderAttr>(goodsOrder.attribute);
            }
            string tableName = goodsOrder.TablesNo.ToString();
            if (goodsOrder.attrbuteModel.isNewTableNo)
            {
                FoodTable table = FoodTableBLL.SingleModel.GetModelById(goodsOrder.TablesNo);
                if (table == null)
                {
                    tableName = goodsOrder.TablesNo.ToString();
                }
                else
                {
                    tableName = table.Scene;
                }

            }
            int groupstate = 0;
            string grouptime = "";//拼团结束时间
            //拼团
            if (goodsOrder.GroupId > 0)
            {
                EntGroupSponsor sponsor = EntGroupSponsorBLL.SingleModel.GetModel(goodsOrder.GroupId);
                if (sponsor == null)
                {
                    return Json(new { isok = -1, msg = "团信息不存在" }, JsonRequestBehavior.AllowGet);
                }
                groupstate = sponsor.State;
                grouptime = sponsor.EndDate.ToString("yyyy-MM-dd HH:mm:ss");
            }

            var postdata = new
            {
                buyPrice = goodsOrder.BuyPrice * 0.01,
                freightPrice = goodsOrder.FreightPrice * 0.01,
                stateRemark = Enum.GetName(typeof(miniAppFoodOrderState), goodsOrder.State),
                goodsOrder = goodsOrder,
                OrderType = goodsOrder.OrderType == (int)miniAppFoodOrderType.堂食 ? "堂食" : "外卖",
                goodOrderDtl = goodsOrderDtl.Select(order => new
                {
                    priceStr = (order.Price * order.Count * 0.01).ToString("0.00"),
                    goodImgUrl = goodsList.Where(goods => goods.Id == order.FoodGoodsId).FirstOrDefault()?.ImgUrl,
                    goodname = goodsList.Where(goods => goods.Id == order.FoodGoodsId).FirstOrDefault()?.GoodsName,
                    orderDtl = order
                }),
                tabelName = tableName,
                groupstate = groupstate,//4待付款，1开团成功，2团购成功，-1成团失败,-4已过期(GroupState)
                groupendtime = grouptime,
                goodsOrder.GoodType,
                goodsOrder.GroupId,

            };

            return Json(new { isok = true, msg = "成功", data = postdata }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 生成订单（该接口已停止维护，新接口在InheritController/AddPayOrder）
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addMiniappGoodsOrder(string appid, string openid, string goodCarIdStr, string orderjson, int buyMode = (int)miniAppBuyMode.微信支付, bool ispay = true, bool isNewTableNo = false)
        {
            #region 物流下单参数
            //是否通过接口物流配送,1:商家配送，2：调起达达推单接口，3：蜂鸟配送
            int getWay = Context.GetRequestInt("getWay", (int)miniAppOrderGetWay.商家配送);
            string cityname = Context.GetRequest("cityname", string.Empty);
            string lat = Context.GetRequest("lat", string.Empty);
            string lnt = Context.GetRequest("lnt", string.Empty);
            int distributionprice = Context.GetRequestInt("distributionprice", 0);//物流运费
            #endregion

            //无关逻辑,方便码农定位bug
            string dugmsg = "dugmsg";

            //用户领取的优惠券记录ID
            int couponLogId = Context.GetRequestInt("couponlogid", 0);

            #region  数据基础验证
            if (string.IsNullOrEmpty(orderjson))
            {
                return Json(new { isok = -1, msg = "订单不能为空" }, JsonRequestBehavior.AllowGet);
            }
            FoodGoodsOrder order = null;
            try
            {
                order = JsonConvert.DeserializeObject<FoodGoodsOrder>(orderjson);
                if (order == null)
                {
                    return Json(new { isok = -1, msg = "没有接收到您提交的订单内容" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "订单内容异常,无法识别" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            //通过缓存获取门店信息
            Food store = FoodBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (store == null)
            {
                return Json(new { isok = -1, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            if (store.openState != 1)
            {
                return Json(new { isok = -1, msg = "商家未营业" }, JsonRequestBehavior.AllowGet);
            }
            //添加锁项
            try
            {
                //不同商家，不同的锁,当前商家若还未创建，则创建一个
                if (!lockObjectDict_Order.ContainsKey(store.Id))
                {
                    if (!lockObjectDict_Order.TryAdd(store.Id, new object()))
                    {
                        return Json(new { isok = -1, msg = "系统繁忙,请稍候再试！" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "系统繁忙,请稍候再试！" }, JsonRequestBehavior.AllowGet);
            }


            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(goodCarIdStr))
            {
                return Json(new { isok = -1, msg = "购物车异常" }, JsonRequestBehavior.AllowGet);
            }
            int int_TryParseId = 0;
            //所有要下单的购物车记录
            List<string> goodCarIds = goodCarIdStr?.Split(',')?.Where(c => !string.IsNullOrWhiteSpace(c)
                                                                                 && Int32.TryParse(c, out int_TryParseId))?.ToList();
            if (goodCarIds == null || !goodCarIds.Any())
            {
                return Json(new { isok = false, msg = "请选择要下单的购物车内容" }, JsonRequestBehavior.AllowGet);
            }

            List<FoodGoodsCart> goodsCar = FoodGoodsCartBLL.SingleModel.GetList($" Id in({string.Join(",", goodCarIds)}) and UserId = {userInfo.Id} and state = 0 ");
            if (goodsCar == null || goodsCar.Count <= 0)
            {
                return Json(new { isok = -1, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
            }

            int orderType = order.TablesNo > 0 ? (int)miniAppFoodOrderType.堂食 : (int)miniAppFoodOrderType.外卖;

            #endregion

            try
            {
                #region 订单数据的处理及价格计算
                int beforeDiscountPrice = goodsCar.Sum(x => x.Price * x.Count);//优惠前商品总价
                int afterDiscountPrice = beforeDiscountPrice;//优惠后商品总价(默认没有优惠)

                #region 会员打折,计算  beforeDiscountPrice 
                StringBuilder sbUpdateGoodCartSql = null;

                //获取会员信息
                VipRelation vipInfo = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id} and state>=0");
                if (vipInfo != null)
                {
                    VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={vipInfo.levelid} and state>=0");

                    VipLevelBLL.SingleModel.CalculateVipGoodsCartPrice(goodsCar, levelinfo);
                    afterDiscountPrice = goodsCar.Sum(x => x.Price * x.Count);//优惠后商品总价

                    if (levelinfo != null)
                    {
                        sbUpdateGoodCartSql = new StringBuilder();
                        foreach (var item in goodsCar)
                        {
                            sbUpdateGoodCartSql.Append(FoodGoodsCartBLL.SingleModel.BuildUpdateSql(item, "Price,originalPrice") + ";");
                        }
                    }
                }


                //满减规则优惠金额
                if (store.funJoinModel.discountRuleSwitch)//若开启了满减优惠规则
                {
                    int maxDiscountMoney = DiscountRuleBLL.SingleModel.getMaxDiscountMoney(afterDiscountPrice, umodel.Id);
                    afterDiscountPrice -= maxDiscountMoney;
                }

                //首单立减优惠
                int firstOrderDiscountMoney = DiscountRuleBLL.SingleModel.getFirstOrderDiscountMoney(userInfo.Id, umodel.Id, 0, TmpType.小程序多门店模板);
                afterDiscountPrice -= firstOrderDiscountMoney;


                #region  金额计算

                //优惠券优惠金额
                int couponsum = 0;
                if (couponLogId > 0)
                {
                    List<CouponLog> couponlist = CouponLogBLL.SingleModel.GetCouponList(0, userInfo.Id, store.Id, umodel.Id, 1, 1, "l.addtime desc",
                        string.Join(",", goodsCar.Select(e => e.FoodGoodsId).Distinct()),
                        JsonConvert.SerializeObject(goodsCar.Select(e => new
                        {
                            goodid = e.FoodGoodsId,
                            totalprice = e.Count * e.Price
                        })), couponLogId);
                    //满足优惠券使用条件才去计算优惠券优惠金额
                    if (couponlist?.Count >= 1 && couponlist[0].CanUse)
                    {
                        string couponmsg = "";
                        //优惠金额
                        couponsum = CouponLogBLL.SingleModel.GetCouponPrice(couponLogId, goodsCar, ref couponmsg);
                        if (!string.IsNullOrEmpty(couponmsg))
                        {
                            return Json(new { isok = false, msg = couponsum }, JsonRequestBehavior.AllowGet);
                        }
                        afterDiscountPrice -= couponsum;
                    }
                    else
                    {
                        return Json(new { isok = false, msg = $" 优惠券失效 或 当前订单未满足优惠券使用条件！" }, JsonRequestBehavior.AllowGet);
                    }
                }
                order.CouponPrice = couponsum;
                #endregion

                #endregion

                switch (orderType)
                {
                    case (int)miniAppFoodOrderType.外卖:
                        if (store.TakeOut != 1)
                        {
                            return Json(new { isok = -1, msg = "商家未开启外卖服务" }, JsonRequestBehavior.AllowGet);
                        }
                        if (beforeDiscountPrice < store.OutSide && (getWay == (int)miniAppOrderGetWay.商家配送 || getWay == (int)miniAppOrderGetWay.快跑者配送))
                        {
                            return Json(new { isok = -1, msg = $"还差{((store.OutSide - afterDiscountPrice) * 0.01).ToString("0.00")}元起送,无法提交订单" }, JsonRequestBehavior.AllowGet);
                        }

                        FoodAddress address = FoodAddressBLL.SingleModel.GetModel(order.AddressId);
                        if (address == null)
                        {
                            return Json(new { isok = -1, msg = "收货地址信息错误" }, JsonRequestBehavior.AllowGet);
                        }

                        double distance = GetDistance(store.Lat, store.Lng, address.Lat, address.Lng);
                        if (distance > store.DeliveryRange)
                        {
                            return Json(new { isok = -1, msg = "超出配送范围,请更换收货地址" }, JsonRequestBehavior.AllowGet);
                        }

                        //地址信息录入
                        order.AccepterName = address.NickName;
                        order.AccepterTelePhone = address.TelePhone;
                        order.ZipCode = address.ZipCode;
                        order.Address = $"{address.Address}";
                        //计算餐盒费
                        List<FoodGoods> goodsList = FoodGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCar.Select(cart => cart.FoodGoodsId))}) ");
                        foreach (var cart in goodsCar)
                        {
                            cart.goodsMsg = goodsList.Where(goods => goods.Id == cart.FoodGoodsId).FirstOrDefault();
                            if (cart.goodsMsg.isPackin == 1)
                            {
                                order.PackinPrice += store.PackinFee * cart.Count;
                            }
                        }

                        //order.DistributionType = distributiontype;//物流接口配送
                        break;
                    case (int)miniAppFoodOrderType.堂食:
                        if (store.TheShop != 1)
                        {
                            return Json(new { isok = -1, msg = "商家未开启堂食服务" }, JsonRequestBehavior.AllowGet);
                        }
                        FoodTable table = FoodTableBLL.SingleModel.GetModelByScene(store.Id, order.TablesNo.ToString(), isNewTableNo);
                        if (table == null || table.Id <= 0)
                        {
                            return Json(new { isok = -1, msg = "桌台号错误" }, JsonRequestBehavior.AllowGet);
                        }
                        order.attrbuteModel.isNewTableNo = isNewTableNo;
                        break;
                    default:
                        return Json(new { isok = -1, msg = "未知的配送方式" }, JsonRequestBehavior.AllowGet);
                }

                order.attribute = JsonConvert.SerializeObject(order.attrbuteModel);
                order.OrderType = orderType;
                order.GetWay = orderType == (int)miniAppFoodOrderType.外卖 ? getWay : (int)miniAppOrderGetWay.到店自取;
                order.StoreId = store.Id;
                order.UserId = userInfo.Id;
                order.CreateDate = DateTime.Now;
                order.State = (int)miniAppFoodOrderState.待付款;
                order.QtyCount = goodsCar.Sum(x => x.Count);
                order.ReducedPrice = beforeDiscountPrice - afterDiscountPrice;
                order.BuyMode = buyMode;
                order.FreightPrice = getWay > 1 ? distributionprice : (order.TablesNo > 0 ? 0 : store.ShippingFee); //堂食免运费
                order.BuyPrice = afterDiscountPrice + order.FreightPrice + order.PackinPrice;
                //对外订单号生成
                order.OrderNum = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{random_Food.Next(999).ToString().PadLeft(3, '0')}";
                #endregion

                lock (lockObjectDict_Order[store.Id])
                {

                    #region  购物车商品是否可下单 - 验证
                    //失效菜品 & 异常菜品
                    string goodsIds = string.Join(",",goodsCar.Select(s=>s.FoodGoodsId).Distinct());
                    List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(goodsIds);
                    foreach (FoodGoodsCart car in goodsCar)
                    {
                        FoodGoods good = foodGoodsList?.FirstOrDefault(f=>f.Id == car.FoodGoodsId);
                        if (good == null)
                        {
                            return Json(new { isok = -1, msg = "菜品信息错误,请重新选择购买菜品！" }, JsonRequestBehavior.AllowGet);
                        }
                        if (car.GoodsState > 0)
                        {
                            return Json(new { isok = -1, msg = $"菜品 '{good.GoodsName}' 已经下架或被删除,请重新选择购买菜品！ " }, JsonRequestBehavior.AllowGet);
                        }
                        switch (order.OrderType)
                        {
                            case (int)miniAppFoodOrderType.外卖:
                                if (good.openTakeOut != 1)
                                {
                                    return Json(new { isok = -1, msg = $"菜品'{good.GoodsName}' 未开启外卖送餐,请重新选择其他菜品！" }, JsonRequestBehavior.AllowGet);
                                }
                                break;
                            case (int)miniAppFoodOrderType.堂食:
                                if (good.openTheShop != 1)
                                {
                                    return Json(new { isok = -1, msg = $"菜品'{good.GoodsName}' 未开启堂食,请重新选择其他菜品！" }, JsonRequestBehavior.AllowGet);
                                }
                                break;
                        }

                        //判定当前商品数量是否足够此次下单
                        int curGoodQty = FoodGoodsBLL.SingleModel.GetGoodQty(good, car.SpecIds);
                        int count = goodsCar.Where(y => y.FoodGoodsId == car.FoodGoodsId && y.SpecIds == car.SpecIds).Sum(y => y.Count);
                        if (curGoodQty < count)
                        {
                            FoodGoods curGood = FoodGoodsBLL.SingleModel.GetModel(car.FoodGoodsId);
                            return Json(new { isok = -1, msg = $"菜品: {curGood.GoodsName} {(!string.IsNullOrWhiteSpace(car.SpecInfo) ? "规格:" + car.SpecInfo : "")} 库存不足!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    #endregion

                    SaveMoneySetUser saveMoneyUser = null;
                    switch (buyMode)
                    {
                        case (int)miniAppBuyMode.储值支付:
                            //储值支付判定预存款是否足够
                            saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(umodel.AppId, userInfo.Id);
                            if (saveMoneyUser == null || saveMoneyUser.AccountMoney < order.BuyPrice)
                            {
                                return Json(new { isok = -1, msg = $" 储值余额不足,请充值！ " }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                    }

                    //生成订单
                    if (!FoodGoodsOrderBLL.SingleModel.addGoodsOrder(ref order, goodsCar, userInfo.Id, sbUpdateGoodCartSql, ref dugmsg))
                    {
                        return Json(new { isok = -1, msg = $"订单生成失败！" }, JsonRequestBehavior.AllowGet);
                    }

                    //对接物流平台
                    dugmsg = DistributionApiConfigBLL.SingleModel.AddDistributionOrder(umodel.Id, userInfo.Id, cityname, lat, lnt, getWay, order, goodsCar, (int)TmpType.小程序餐饮模板,0,distributionprice);
                    if (!string.IsNullOrEmpty(dugmsg))
                    {
                        return Json(new { isok = -1, msg = dugmsg }, JsonRequestBehavior.AllowGet);
                    }

                    //若使用了优惠券将优惠券标记为已使用
                    if (couponLogId > 0)
                    {
                        CouponLogBLL.SingleModel.Update(new CouponLog()
                        {
                            Id = couponLogId,
                            State = 1,
                            OrderId = order.Id
                        }, "state,orderid");
                    }

                    #region (不参与当前生成逻辑)查找可领取的立减金  --作用于前端判定订单是否要跳转到立减金领取页面
                    //【获取立减金，没有的话就传null】
                    Coupons reductionCart = CouponsBLL.SingleModel.GetOpenedModel(umodel.Id);
                    if (reductionCart != null)
                    {
                        int count = CouponLogBLL.SingleModel.GetCountBySql($"select count(*) from couponlog where CouponId={reductionCart.Id} group by fromorderid");//已领取的份数
                        reductionCart.RemNum = reductionCart.CreateNum - count;
                    }
                    #endregion

                    //不同支付方式,走不同流程,其中储值支付直接扣费,微信支付则为生成预支付订单
                    switch (buyMode)
                    {
                        case (int)miniAppBuyMode.微信支付:
                        case (int)miniAppBuyMode.线下支付:
                            //为0不需进入生成微信预支付订单的流程（免费订单）
                            if (order.BuyPrice == 0)
                            {
                                #region 直接更改订单状态并执行支付后的后续操作
                                order.PayDate = DateTime.Now;//支付时间
                                order.State = (int)miniAppFoodOrderState.待接单;

                                //自动接单则跳过待接单状态
                                if (store != null && store.AutoAcceptOrder == 1)
                                {
                                    order.State = (order.OrderType == (int)miniAppFoodOrderType.堂食 ? (int)miniAppFoodOrderState.待就餐 : (int)miniAppFoodOrderState.待送餐);
                                    order.ConfDate = DateTime.Now;//接单时间
                                    if (order.State == (int)miniAppFoodOrderState.待就餐)
                                    {
                                        order.DistributeDate = DateTime.Now;
                                    }
                                }
                                FoodGoodsOrderBLL.SingleModel.Update(order);

                                //更新预约状态
                                if (order.OrderType == (int)miniAppFoodOrderType.预约 && order.ReserveId > 0)
                                {
                                    var reservation = FoodReservationBLL.SingleModel.GetModel(order.ReserveId);
                                    var updateReservationSql = FoodReservationBLL.SingleModel.UpdateToPay(reservation: reservation, order: order, cartItem: goodsCar);
                                    if (!string.IsNullOrWhiteSpace(updateReservationSql))
                                    {
                                        var tran = new TransactionModel();
                                        tran.Add(updateReservationSql);
                                        FoodReservationBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
                                    }
                                }

                                //添加订单操作日志记录
                                FoodGoodsOrderLog curLog = new FoodGoodsOrderLog()
                                {
                                    GoodsOrderId = order.Id,
                                    UserId = order.UserId.ToString(),
                                    LogInfo = $" 订单成功支付：{order.BuyPrice * 0.01} 元 ",
                                    CreateDate = DateTime.Now
                                };
                                FoodGoodsOrderLogBLL.SingleModel.Add(curLog);

                                FoodGoodsOrderBLL.SingleModel.AfterPaySuccesExecFun(order);

                                TransactionModel temptran = new TransactionModel();
                                string dadamsg = DistributionApiConfigBLL.SingleModel.UpdatePeiSongOrder(order.Id, store.appId, (int)TmpType.小程序餐饮模板, order.GetWay, ref temptran, true);
                                if (!string.IsNullOrEmpty(dadamsg))
                                {
                                    LogHelper.WriteInfo(this.GetType(), dadamsg);
                                }
                                //达达配送，修改订单状态为待接单
                                //if (order.GetWay == (int)miniAppOrderGetWay.达达配送)
                                //{
                                //    TransactionModel tran = new TransactionModel();
                                //    string dadamsg = _dadaOrderBLL.GetDadaOrderUpdateSql(order.Id, store.appId, (int)TmpType.小程序餐饮模板, ref tran, true);
                                //    if (!string.IsNullOrEmpty(dadamsg))
                                //    {
                                //        LogHelper.WriteInfo(this.GetType(), dadamsg);
                                //    }
                                //}
                                ////达达配送，修改订单状态为待接单
                                //else if (order.GetWay == (int)miniAppOrderGetWay.蜂鸟配送)
                                //{
                                //    TransactionModel tran = new TransactionModel();
                                //    string fnamsg = _fnOrderBLL.GetFNOrderUpdateSql(order.Id, store.appId, (int)TmpType.小程序餐饮模板, ref tran, true);
                                //    if (!string.IsNullOrEmpty(fnamsg))
                                //    {
                                //        LogHelper.WriteInfo(this.GetType(), fnamsg);
                                //    }
                                //}
                                //新订单电脑语音提示
                                Utils.RemoveIsHaveNewOrder(umodel.Id);
                                return Json(new { isok = 1, msg = "订单生成并支付成功", postdata = order.OrderNum, orderid = order.OrderId, dbOrder = order.Id, reductionCart = reductionCart }, JsonRequestBehavior.AllowGet);
                                #endregion
                            }
                            else //生成微信预支付订单
                            {
                                #region CtiyModer 生成
                                string no = WxPayApi.GenerateOutTradeNo();

                                CityMorders citymorderModel = new CityMorders
                                {
                                    OrderType = (int)ArticleTypeEnum.MiniappFoodGoods,
                                    ActionType = (int)ArticleTypeEnum.MiniappFoodGoods,
                                    Addtime = DateTime.Now,
                                    payment_free = order.BuyPrice,
                                    trade_no = no,
                                    Percent = 99,//不收取服务费
                                    userip = WebHelper.GetIP(),
                                    FuserId = userInfo.Id,
                                    Fusername = userInfo.NickName,
                                    orderno = no,
                                    payment_status = 0,
                                    Status = 0,
                                    Articleid = 0,
                                    CommentId = 0,
                                    MinisnsId = store.Id,//商家ID
                                    TuserId = order.Id,//订单的ID
                                    ShowNote = $" {umodel.Title}点餐 支付{order.BuyPrice * 0.01}元",
                                    CitySubId = 0,//无分销,默认为0
                                    PayRate = 1,
                                    buy_num = 0, //无
                                    appid = appid,
                                };

                                order.OrderId = Convert.ToInt32(new CityMordersBLL().Add(citymorderModel));
                                FoodGoodsOrderBLL.SingleModel.Update(order, "OrderId");
                                #endregion
                            }

                            break;
                        case (int)miniAppBuyMode.储值支付:
                            #region 储值支付 扣除预存款金额并生成消费记录
                            if (payOrderBySaveMoneyUser(order, saveMoneyUser, sbUpdateGoodCartSql,ref dugmsg))
                            {
                                //新订单电脑语音提示
                                Utils.RemoveIsHaveNewOrder(umodel.Id);
                                return Json(new { isok = 1, msg = "订单生成并支付成功", postdata = order.OrderNum, orderid = order.OrderId, dbOrder = order.Id, reductionCart = reductionCart }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { isok = -1, msg = "订单支付失败:"+ dugmsg, postdata = order.OrderNum, orderid = order.OrderId, dbOrder = order.Id, reductionCart = reductionCart }, JsonRequestBehavior.AllowGet);
                            }
                        #endregion


                        default:
                            break;
                    }
                    if (buyMode== (int)miniAppBuyMode.线下支付)
                    {
                        //新订单电脑语音提示
                        Utils.RemoveIsHaveNewOrder(umodel.Id);
                        FoodGoodsOrderBLL.SingleModel.AfterUnPaySuccesExecFun(order);
                    }
                    return Json(new { isok = 1, msg = "订单生成成功", postdata = order.OrderNum, orderid = order.OrderId, dbOrder = order.Id, reductionCart = reductionCart }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = ex.Message + "," + dugmsg }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        /// <summary>
        /// 餐饮菜品支付
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="type"></param>
        /// <param name="openId"></param>
        /// <returns></returns>
        [HttpPost, AuthLoginCheckXiaoChenXun]
        public ActionResult PayOrder(int orderid, int type, string openId)
        {
            try
            {
                CityMorders order = new CityMordersBLL().GetModel(orderid);

                if (order == null || order.payment_status != 0)
                {
                    return Json(new { isok = false, msg = "订单已经失效 errCode:1576" });
                }

                if (order.OrderType == (int)ArticleTypeEnum.MiniappFoodGoods)
                {
                    string sql = $" update foodgoodsorder set State = {(int)miniAppFoodOrderState.付款中} where orderid = {orderid} and (State in ({(int)miniAppFoodOrderState.待付款},{(int)miniAppFoodOrderState.付款中}) );";
                    //改为付款中,避免付款过程中被自动取消服务取消掉
                    bool success = SqlMySql.ExecuteNonQuery(FoodGoodsOrderBLL.SingleModel.connName, CommandType.Text, sql, null) > 0;
                    if (!success)
                    {
                        return Json(new { isok = false, msg = "订单已经失效 errCode:1586" });
                    }
                }

                Food foodStore = FoodBLL.SingleModel.GetModel(order.MinisnsId);
                if (foodStore == null)
                {
                    return Json(new { isok = false, msg = "商家信息错误" });
                }
                XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModel(foodStore.appId);
                if (xcxAccountRelation == null)
                {
                    return Json(new { isok = false, msg = "授权信息错误" });
                }

                C_UserInfo LoginCUser = LoginData;
                if (LoginCUser == null)
                {
                    return Json(new { isok = false, msg = "登陆信息异常，请重新支付,Entity_null" });
                }

                PayCenterSetting setting = null;
                if (!string.IsNullOrEmpty(order.appid))
                {
                    setting = PayCenterSettingBLL.SingleModel.GetPayCenterSetting(order.appid);
                }
                else
                {
                    setting = PayCenterSettingBLL.SingleModel.GetPayCenterSetting((int)PayCenterSettingType.City, order.MinisnsId);
                }

                if (openId.IsNullOrWhiteSpace())
                {
                    return Json(new { isok = false, msg = "Openid_null/Empty" });
                }

                if (type != 2)//JSAPI支付预处理
                {
                    JsApiPay jsApiPay = new JsApiPay(HttpContext)
                    {
                        total_fee = order.payment_free,
                        openid = openId
                    };
                    //统一下单，获得预支付码
                    WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResultByCity(setting, order, WebConfigBLL.citynotify_url);



                    #region 发送消息参数

                    FoodGoodsOrder foodOrder = FoodGoodsOrderBLL.SingleModel.GetModel($" OrderId = {orderid}  ") ?? new FoodGoodsOrder();
                    if (!string.IsNullOrWhiteSpace(Convert.ToString(unifiedOrderResult.GetValue("prepay_id"))))
                    {

                        //增加发送模板消息参数
                        TemplateMsg_UserParam userParam = new TemplateMsg_UserParam();
                        userParam.AppId = xcxAccountRelation.AppId;
                        userParam.Form_IdType = 1;//form_id 为prepay_id
                        userParam.OrderId = foodOrder.Id;
                        userParam.OrderIdType = (int)TmpType.小程序餐饮模板;
                        userParam.Open_Id = openId;
                        userParam.AddDate = DateTime.Now;
                        userParam.Form_Id = Convert.ToString(unifiedOrderResult.GetValue("prepay_id"));
                        userParam.State = 1;
                        userParam.SendCount = 0;
                        userParam.AddDate = DateTime.Now;
                        userParam.LoseDateTime = DateTime.Now.AddDays(7);//prepay_id 有效期7天

                        TemplateMsg_UserParamBLL.SingleModel.Add(userParam);
                    }
                    #endregion

                    return Json(new { isok = true, msg = "下单成功", data = jsApiPay.GetJsApiParametersnew(setting) });
                }
                else
                {
                    return Json(new { isok = true, msg = "下单成功" });
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
                return Json(new { isok = false, msg = "下单异常 errCode:1668" });
            }

        }


        #region 储值支付方式支付

        /// <summary>
        /// 储值支付 扣除预存款金额并生成消费记录(电商)
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        public bool payOrderBySaveMoneyUser(FoodGoodsOrder dbOrder, SaveMoneySetUser saveMoneyUser, StringBuilder updateCarPriceSql, ref string erroMsg)
        {
            if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
            {
                return false;
            }
            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
            {
                return false;
            }
            MySqlParameter[] _pone = null;
            TransactionModel _tranModel = new TransactionModel();
            if (updateCarPriceSql != null)
            {
                _tranModel.Add(updateCarPriceSql.ToString());
            }

            if (dbOrder != null)
            {
                Food foodStore = FoodBLL.SingleModel.GetModel(dbOrder.StoreId);
                if (!foodStore.funJoinModel.canSaveMoneyFunction)
                {
                    return false;
                }

                dbOrder.PayDate = DateTime.Now;//支付时间
                dbOrder.State = (int)miniAppFoodOrderState.待接单;

                //自动接单则跳过待接单状态
                if (foodStore != null && foodStore.AutoAcceptOrder == 1)
                {
                    dbOrder.State = (dbOrder.OrderType == (int)miniAppFoodOrderType.堂食 ? (int)miniAppFoodOrderState.待就餐 : (int)miniAppFoodOrderState.待送餐);
                    dbOrder.ConfDate = DateTime.Now;//接单时间
                    if (dbOrder.State == (int)miniAppFoodOrderState.待就餐)
                    {
                        dbOrder.DistributeDate = DateTime.Now;
                    }
                }

                string dadamsg = DistributionApiConfigBLL.SingleModel.UpdatePeiSongOrder(dbOrder.Id, foodStore.appId, (int)TmpType.小程序餐饮模板, dbOrder.GetWay, ref _tranModel, true);
                if (!string.IsNullOrEmpty(dadamsg))
                {
                    LogHelper.WriteInfo(this.GetType(), dadamsg);
                    erroMsg = dadamsg;
                    return false;
                }
                
                dbOrder.GoodsGuid = Guid.NewGuid().ToString().Replace("-", "");//此栏位暂无用处

                string sql = $"State,GoodsGuid,PayDate,ConfDate{(dbOrder.State == (int)miniAppFoodOrderState.待就餐 ? ",DistributeDate" : "")}";
                _tranModel.Add(FoodGoodsOrderBLL.SingleModel.BuildUpdateSql(dbOrder, sql, out _pone), _pone);

                //添加订单日志
                FoodGoodsOrderLog orderLog = new FoodGoodsOrderLog()
                {
                    GoodsOrderId = dbOrder.Id,
                    UserId = dbOrder.UserId.ToString(),
                    LogInfo = $" 订单成功支付(储值支付)：{dbOrder.BuyPrice * 0.01} 元 ",
                    CreateDate = DateTime.Now
                };
                _tranModel.Add(FoodGoodsOrderLogBLL.SingleModel.BuildAddSql(orderLog));
            }
            //添加储值日志
            SaveMoneySetUserLog userLog = new SaveMoneySetUserLog()
            {
                AppId = saveMoneyUser.AppId,
                UserId = dbOrder.UserId,
                MoneySetUserId = saveMoneyUser.Id,
                Type = -1,
                BeforeMoney = saveMoneyUser.AccountMoney,
                AfterMoney = saveMoneyUser.AccountMoney - dbOrder.BuyPrice,
                ChangeMoney = dbOrder.BuyPrice,
                ChangeNote = $" 购买商品,订单号:{dbOrder.OrderNum} ",
                CreateDate = DateTime.Now,
                State = 1
            };
            _tranModel.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(userLog));

            //储值扣费
            _tranModel.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney - {dbOrder.BuyPrice} where id =  {saveMoneyUser.Id} ; ");

            //执行sql
            bool isSuccess = SaveMoneySetUserLogBLL.SingleModel.ExecuteTransaction(_tranModel.sqlArray, _tranModel.ParameterArray);

            //操作成功
            if (isSuccess)
            {
                AfterPaySuccesExecFun(dbOrder);
            }

            return isSuccess;
        }

        /// <summary>
        /// 储值支付后
        /// </summary>
        /// <param name="foodGoodsOrder"></param>
        public void AfterPaySuccesExecFun(FoodGoodsOrder foodGoodsOrder)
        {

            if (foodGoodsOrder == null)
            {
                return;
            }

            #region 自动打单 + 发送餐饮订单支付成功通知 模板消息


            
            


            
            

            
            

            Food foodStore = FoodBLL.SingleModel.GetModel(foodGoodsOrder.StoreId);
            if (foodStore == null)
            {
                return;
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModel(foodStore.appId);
            if (xcxAccountRelation == null)
            {
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel(xcxAccountRelation.AccountId);
            if (account == null)
            {
                return;
            }
            List<FoodPrints> foodPrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {foodStore.Id} and accountId = '{account.OpenId}' and state >= 0 ");
            List<FoodGoodsCart> cartlist = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId={foodGoodsOrder.Id} and state=1");

            string goodsIds = string.Join(",",cartlist?.Select(s=>s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(goodsIds);
            cartlist.ForEach(cart =>
            {
                cart.goodsMsg = foodGoodsList?.FirstOrDefault(f=>f.Id == cart.FoodGoodsId);
            });

            //打单
            FoodGoodsOrderBLL.SingleModel.PrintOrder(foodStore, foodGoodsOrder, cartlist, foodPrintList, account);


            //餐饮订单支付成功-发送商家模板消息
            TemplateMsg_Gzh.SendOrderSuccessTemplateMessage(foodGoodsOrder, account, foodStore);
            //餐饮订单支付成功-发送客户模板消息
            object orderData = TemplateMsg_Miniapp.FoodGetTemplateMessageData(foodStore, foodGoodsOrder, SendTemplateMessageTypeEnum.餐饮订单支付成功通知);
            TemplateMsg_Miniapp.SendTemplateMessage(foodGoodsOrder.UserId, SendTemplateMessageTypeEnum.餐饮订单支付成功通知, TmpType.小程序餐饮模板, orderData);

            #endregion
        }



        /// <summary>
        /// 使用储值支付
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult buyOrderbySaveMoney(string appid, string openid, int goodsorderid)
        {

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }

            FoodGoodsOrder dbOrder = FoodGoodsOrderBLL.SingleModel.GetModel(goodsorderid);
            if (dbOrder == null)
            {
                return Json(new { isok = false, msg = "订单不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (dbOrder.State != 0)
            {
                return Json(new { isok = false, msg = "此订单不可进行支付" }, JsonRequestBehavior.AllowGet);
            }
            if (dbOrder.BuyMode != (int)miniAppBuyMode.储值支付)
            {
                return Json(new { isok = false, msg = "支付方式并非储值支付" }, JsonRequestBehavior.AllowGet);
            }
            SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(xcxAccountRelation.AppId, userInfo.Id);

            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
            {
                return Json(new { isok = false, msg = "储值金额不足,请更换支付方式" }, JsonRequestBehavior.AllowGet);
            }

            string errorMsg = "";
            //进入支付流程(此处因购物车记录已经更新,故无需再传入更新价格sql,传null)
            if (payOrderBySaveMoneyUser(dbOrder, saveMoneyUser, null,ref errorMsg))
            {
                AfterPaySuccesExecFun(dbOrder);

                return Json(new { isok = true, msg = "支付成功", data = new { postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id } }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = false, msg = "支付失败:"+ errorMsg, data = new { postdata = dbOrder.OrderNum, orderid = dbOrder.OrderId, dbOrder = dbOrder.Id } }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="orderId"></param>
        /// <param name="State">OrderState</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult updateMiniappGoodsOrderState(string appid, string openid, int orderId, int State)
        {
            #region 数据校验
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }

            FoodGoodsOrder miniappFoodGoodOrder = FoodGoodsOrderBLL.SingleModel.GetModel(orderId);
            if (miniappFoodGoodOrder == null)
            {
                return Json(new { isok = false, msg = "找不到订单" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (miniappFoodGoodOrder.UserId != userInfo.Id)
            {
                return Json(new { isok = false, msg = "无此订单操作权限" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            if (miniappFoodGoodOrder.OrderType == (int)miniAppFoodOrderType.堂食)
            {
                #region 堂食操作
                switch (State)
                {
                    case (int)miniAppFoodOrderState.已取消:

                        List<int> allowList = new List<int>(); //当前订单状态不允许被取消的状态
                        allowList.Add((int)miniAppFoodOrderState.待付款);

                        if (!allowList.Contains(miniappFoodGoodOrder.State))
                        {
                            return Json(new { isok = false, msg = "此订单不允许取消！" }, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    case (int)miniAppFoodOrderState.付款中:

                        allowList = new List<int>();//当前订单状态不允许被取消的状态
                        allowList.Add((int)miniAppFoodOrderState.待付款);

                        if (!allowList.Contains(miniappFoodGoodOrder.State))
                        {
                            return Json(new { isok = false, msg = "此订单不允许修改为指定状态！" }, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    case (int)miniAppFoodOrderState.待付款:

                        allowList = new List<int>();//当前订单状态不允许被取消的状态
                        allowList.Add((int)miniAppFoodOrderState.付款中);

                        if (!allowList.Contains(miniappFoodGoodOrder.State))
                        {
                            return Json(new { isok = false, msg = "此订单不允许修改为指定状态！" }, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    default:
                        return Json(new { isok = false, msg = "订单状态错误,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
                }

                //未更改前的状态
                int oldState = miniappFoodGoodOrder.State;
                //更改状态
                miniappFoodGoodOrder.State = State;

                if (State == (int)miniAppFoodOrderState.已取消)
                {
                    if (FoodGoodsOrderBLL.SingleModel.updateStock(miniappFoodGoodOrder, oldState))
                    {
                        FoodGoodsOrderLogBLL.SingleModel.AddLog(miniappFoodGoodOrder.Id, userInfo.Id.ToString(), $" {Enum.GetName(typeof(miniAppFoodOrderState), State)} ");
                        return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (State == (int)miniAppFoodOrderState.已完成)
                {
                    TransactionModel tranModel = new TransactionModel();
                    tranModel.Add($"{FoodGoodsOrderBLL.SingleModel.BuildUpdateSql(miniappFoodGoodOrder, "State")} and state = {oldState}  ");

                    tranModel = FoodGoodsOrderBLL.SingleModel.addSalesCount(miniappFoodGoodOrder.Id, tranModel);

                    if (FoodGoodsOrderBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
                    {
                        //会员加消费金额
                        if (!VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "food", miniappFoodGoodOrder.BuyPrice))
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception("餐饮版-用户自动升级逻辑异常 行数:2002" + miniappFoodGoodOrder.Id));
                        }

                        return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (FoodGoodsOrderBLL.SingleModel.updateFoodOrderState(miniappFoodGoodOrder, oldState, "State"))
                    {
                        if (State == (int)miniAppFoodOrderState.退款审核中)
                        {
                            // 发送餐饮退款申请通知给客户
                            object orderData = TemplateMsg_Miniapp.FoodGetTemplateMessageData(foodStore, miniappFoodGoodOrder, SendTemplateMessageTypeEnum.餐饮退款申请通知);
                            TemplateMsg_Miniapp.SendTemplateMessage(miniappFoodGoodOrder.UserId, SendTemplateMessageTypeEnum.餐饮退款申请通知, TmpType.小程序餐饮模板, orderData);
                            //发送餐饮退款申请通知给商家
                            TemplateMsg_Gzh.OutOrderTemplateMessage(miniappFoodGoodOrder);
                        }

                        return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
                #endregion
            }
            else
            {
                #region 外卖
                switch (State)
                {
                    case (int)miniAppFoodOrderState.已取消:

                        List<int> allowList = new List<int>();//当前订单状态不允许被取消的状态
                        allowList.Add((int)miniAppFoodOrderState.待付款);

                        if (!allowList.Contains(miniappFoodGoodOrder.State))
                        {
                            return Json(new { isok = false, msg = "此订单不允许取消！" }, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    case (int)miniAppFoodOrderState.付款中:

                        allowList = new List<int>();//当前订单状态不允许被取消的状态
                        allowList.Add((int)miniAppFoodOrderState.待付款);

                        if (!allowList.Contains(miniappFoodGoodOrder.State))
                        {
                            return Json(new { isok = false, msg = "此订单不允许修改为指定状态！" }, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    case (int)miniAppFoodOrderState.待付款:

                        allowList = new List<int>(); //当前订单状态不允许被取消的状态
                        allowList.Add((int)miniAppFoodOrderState.付款中);

                        if (!allowList.Contains(miniappFoodGoodOrder.State))
                        {
                            return Json(new { isok = false, msg = "此订单不允许修改为指定状态！" }, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    case (int)miniAppFoodOrderState.退款审核中:

                        allowList = new List<int>();//当前订单状态不允许被取消的状态
                        allowList.Add((int)miniAppFoodOrderState.待接单);
                        allowList.Add((int)miniAppFoodOrderState.待送餐);

                        if (!allowList.Contains(miniappFoodGoodOrder.State))
                        {
                            return Json(new { isok = false, msg = "此订单不允许退款！" }, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    case (int)miniAppFoodOrderState.已完成:

                        allowList = new List<int>();//当前订单状态不允许被取消的状态
                        allowList.Add((int)miniAppFoodOrderState.待确认送达);

                        if (!allowList.Contains(miniappFoodGoodOrder.State))
                        {
                            return Json(new { isok = false, msg = "此订单不允许完成！" }, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    default:
                        return Json(new { isok = false, msg = "订单状态错误,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
                }


                string updateColStr = "State";
                switch (State)
                {

                    case (int)miniAppFoodOrderState.退款审核中:
                        miniappFoodGoodOrder.lastState = miniappFoodGoodOrder.State;  //记录订单申请退款前状态,以实现商家拒绝退款后继续走正常配送流程
                        updateColStr += ",lastState";
                        break;

                    case (int)miniAppFoodOrderState.已完成:
                        miniappFoodGoodOrder.AcceptDate = DateTime.Now;
                        updateColStr += ",AcceptDate";
                        break;

                    case (int)miniAppFoodOrderState.已取消:
                    case (int)miniAppFoodOrderState.付款中:
                    case (int)miniAppFoodOrderState.待付款:
                    default:
                        break;
                }
                //更改前的状态
                int oldState = miniappFoodGoodOrder.State;
                miniappFoodGoodOrder.State = State;

                if (State == (int)miniAppFoodOrderState.已取消)
                {
                    if (FoodGoodsOrderBLL.SingleModel.updateStock(miniappFoodGoodOrder, oldState))
                    {
                        FoodGoodsOrderLogBLL.SingleModel.AddLog(miniappFoodGoodOrder.Id, userInfo.Id.ToString(), $" {Enum.GetName(typeof(miniAppFoodOrderState), State)} ");
                        return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (State == (int)miniAppFoodOrderState.已完成)
                {
                    TransactionModel tranModel = new TransactionModel();
                    tranModel.Add($"{FoodGoodsOrderBLL.SingleModel.BuildUpdateSql(miniappFoodGoodOrder, updateColStr)} and state = {oldState} ");
                    tranModel = FoodGoodsOrderBLL.SingleModel.addSalesCount(miniappFoodGoodOrder.Id, tranModel);

                    if (FoodGoodsOrderBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
                    {
                        //会员加消费金额
                        if (!VipRelationBLL.SingleModel.updatelevel(userInfo.Id, "food"))
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常,行数:2131  订单id" + miniappFoodGoodOrder.Id));
                        }
                        return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (FoodGoodsOrderBLL.SingleModel.updateFoodOrderState(miniappFoodGoodOrder, oldState, updateColStr))
                    {
                        if (State == (int)miniAppFoodOrderState.退款审核中)
                        {
                            // 发送餐饮退款申请通知给客户
                            object orderData = TemplateMsg_Miniapp.FoodGetTemplateMessageData(foodStore, miniappFoodGoodOrder, SendTemplateMessageTypeEnum.餐饮退款申请通知);
                            TemplateMsg_Miniapp.SendTemplateMessage(miniappFoodGoodOrder.UserId, SendTemplateMessageTypeEnum.餐饮订单支付成功通知, TmpType.小程序餐饮模板, orderData);
                            //发送餐饮退款申请通知给商家
                            TemplateMsg_Gzh.OutOrderTemplateMessage(miniappFoodGoodOrder);

                        }

                        return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
                #endregion
            }
        }
        #endregion



        #region 易联云订单打印状态推送回调

        /// <summary>
        /// 返回打印机打印状态 (收费接口,用不了,也还没有接完)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPrintStatus()
        {
            string sign = Context.GetRequest("sign", string.Empty);
            int time = Context.GetRequestInt("time", 0);
            string apikey = Context.GetRequest("apikey", string.Empty);
            if (string.IsNullOrEmpty(sign) || time == 0 || string.IsNullOrEmpty(apikey)) return View();
            if (sign != FoodYiLianYunPrintHelper.GetMD5Hash(apikey + time)) return View();
            string dataid = Context.GetRequest("dataid", string.Empty);
            string machine_code = Context.GetRequest("machine_code", string.Empty);
            int printtime = Context.GetRequestInt("printtime", 0);
            int state = Context.GetRequestInt("state", 0);
            string cmd = Context.GetRequest("cmd", string.Empty);
            FoodOrderPrintLog log = FoodOrderPrintLogBLL.SingleModel.GetModel($"dataid='{dataid}' and machine_code='{machine_code}'");
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(printtime);
            string remark = string.Empty;
            if (state == 1) remark = "打印成功";
            else remark = "打印失败";
            if (log == null)
            {
                log = new FoodOrderPrintLog()
                {
                    Dataid = dataid,
                    addtime = dt,
                    machine_code = machine_code,
                    state = state,
                    isupdate = 0,
                    cmd = cmd,
                    remark = remark
                };
                FoodOrderPrintLogBLL.SingleModel.Add(log);
            }
            else
            {
                log.Dataid = dataid;
                log.addtime = dt;
                log.machine_code = machine_code;
                log.state = state;
                log.isupdate = 1;
                log.cmd = cmd;
                log.remark = remark;
                FoodOrderPrintLogBLL.SingleModel.Update(log);
            }
            return View();
        }
        #endregion

        /// <summary>
        /// 判定商家是否配送范围外
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        public ActionResult GetDistanceForFood(string appid, double lat, double lng)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            Food foodStore = FoodBLL.SingleModel.GetModel($"appId={xcxAccountRelation.Id}");
            if (foodStore == null)
            {
                return Json(new { isok = false, msg = "找不到店铺" }, JsonRequestBehavior.AllowGet);
            }
            double distance = GetDistance(foodStore.Lat, foodStore.Lng, lat, lng);

            if (foodStore.DeliveryRange >= distance)
            {
                return Json(new { isok = true, msg = "配送范围内", data = new { distance = distance, DeliveryRange = foodStore.DeliveryRange } }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = false, msg = "配送范围外", data = new { distance = distance, DeliveryRange = foodStore.DeliveryRange } }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// 根据地址下标,获取地址名称
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getTXMapAddress(double lat, double lng)
        {
            string result = HttpGet($"http://apis.map.qq.com/ws/geocoder/v1/?location={lat},{lng}&key={WebSiteConfig.Tx_MapKey}");

            txMap txMap = JsonConvert.DeserializeObject<txMap>(result);
            return Json(new { isok = true, msg = "成功", data = new { address = txMap.result.address } }, JsonRequestBehavior.AllowGet);
        }

        public class txMap
        {
            public int status { get; set; }

            public string message { get; set; }

            public string request_id { get; set; }

            public txMapResult result { get; set; }


        }

        public class txMapResult
        {

            public string address { get; set; }
        }

        public string HttpGet(string Url, int timeOut = 6000)
        {
            try
            {
                return HttpHelper.GetData(Url, timeOut);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(MsnModelHelper), new Exception(ex.Message + " 请求地址出错：" + Url));
                return "";
            }
        }




        private double rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        /// <summary>
        /// 腾讯地图,返回距离(公里) 
        /// 有误差,同腾讯地图api 8公里 误差在0.1米以内
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double EARTH_RADIUS = 6378.137;
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

    }
}