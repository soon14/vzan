using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Model;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Utility;
using Utility.IO;
using ent = Entity.MiniApp.Ent;

namespace Api.MiniApp.Controllers
{
    public class apiMultiStoreController : InheritController
    {
    }
        public class apiMiniAppMultiStoreController : apiMultiStoreController
    {
        public static readonly ConcurrentDictionary<int, object> lockObjectDict_Order = new ConcurrentDictionary<int, object>();
        public static readonly Random random_MultiStore = new Random(new Guid().GetHashCode());
        
        //公用参数
        private readonly string _appId = Context.GetRequest("appId", string.Empty);
        private readonly string _openId = Context.GetRequest("openId", string.Empty);
        private readonly int _storeId = Context.GetRequestInt("storeId", 0);

        public apiMiniAppMultiStoreController()
        {
        }

        #region 获取门店相关信息

        /// <summary>
        /// 根据选择地址的经纬度,获取同城配送范围内最近一家的门店
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStores(string appId, double lat = 0, double lng = 0, int type = 0, string actionType = "", int pageindex = 1, int pagesize = 10)
        {
            if (string.IsNullOrEmpty(appId))
                return Json(new { code = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);

            if (lat == 0 && lng == 0)
            {
                //表示没有传坐标 通过客户端IP获取经纬度
                string IP = WebHelper.GetIP();
                
                IPToPoint iPToPoint = CommondHelper.GetLoctionByIP(IP);
                if (iPToPoint != null)
                {
                    lat = iPToPoint.result.location.lat;
                    lng = iPToPoint.result.location.lng;
                }

            }
            List<FootBath> availableStores = FootBathBLL.SingleModel.GetNearUseStore(umodel.Id, lat, lng, type);
            if (availableStores == null || availableStores.Count <= 0)
            {
                var bossStore = FootBathBLL.SingleModel.GetModel($"appId={umodel.Id} and IsDel=0 and HomeId=0");
                if (bossStore == null)
                    return Json(new { code = 3, msg = "未开通多门店" }, JsonRequestBehavior.AllowGet);
                return Json(new { code = 2, msg = $"没有一家合适门店", BoosStoreId = bossStore.Id }, JsonRequestBehavior.AllowGet);
            }

            if (actionType == "list")
                return Json(new { code = 1, msg = "成功匹配", obj = availableStores.Skip((pageindex - 1) * pagesize).Take(pagesize), actionType = actionType }, JsonRequestBehavior.AllowGet);

            return Json(new { code = 1, msg = "成功匹配", obj = availableStores.FirstOrDefault(), actionType = actionType, listStores = availableStores }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取对应总店是否开启快递配送
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult GetStoreExpresState(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { code = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            var store = FootBathBLL.SingleModel.GetModel($"appId={umodel.Id} and HomeId=0");
            if (store == null)
                return Json(new { code = 0, msg = "找不到该总店信息" }, JsonRequestBehavior.AllowGet);
            if (!string.IsNullOrEmpty(store.TakeOutWayConfig))
            {
                store.takeOutWayModel = JsonConvert.DeserializeObject<TakeOutWayModel>(store.TakeOutWayConfig);
            }

            return Json(new { code = 1, msg = "获取成功", isOpen = store.takeOutWayModel.GetExpressdelivery.IsOpen, info = store }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 根据appId以及门店Id 获取门店详情
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ActionResult GetStoreById(string appId, int storeId)
        {
            if (string.IsNullOrEmpty(appId))
                return Json(new { code = -1, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            var mode = FootBathBLL.SingleModel.GetModel($"appId={umodel.Id} and Id={storeId} and IsDel=0 ");
            if (mode == null)
                return Json(new { code = 0, msg = "没有相关门店" }, JsonRequestBehavior.AllowGet);
            if (mode.HomeId > 0)
            {
                //表示门店
                if (mode.State == 0)
                {
                    return Json(new { code = 0, msg = "门店已经下架" }, JsonRequestBehavior.AllowGet);
                }
            }
            if (!string.IsNullOrEmpty(mode.SwitchConfig))
            {
                mode.switchModel = JsonConvert.DeserializeObject<SwitchModel>(mode.SwitchConfig);
                mode.shopDays = FootBathBLL.SingleModel.GetShopDays(mode.switchModel);
            }
            if (!string.IsNullOrEmpty(mode.TakeOutWayConfig))
            {
                mode.takeOutWayModel = JsonConvert.DeserializeObject<TakeOutWayModel>(mode.TakeOutWayConfig);

            }

            return Json(new { code = 1, msg = "获取成功", obj = mode }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 根据用户Id以及用户所在位置的经纬度,返回距离最近的5条收货地址
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        public ActionResult GetNearMyAddress(string appId, int userId, double lat = 0, double lng = 0)
        {
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            var userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                return Json(new { code = 0, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            var addressList = FoodAddressBLL.SingleModel.GetList($"AppId = {umodel.Id} and UserId = {userId} and State = 0 ");
            
            if (addressList != null && addressList.Count > 0)
            {
                if (lat == 0 && lng == 0)
                {
                   
                    //表示没有传坐标 通过客户端IP获取经纬度
                    string IP = WebHelper.GetIP();
                    IPToPoint iPToPoint = CommondHelper.GetLoctionByIP(IP);
                    if (iPToPoint != null)
                    {
                        lat = iPToPoint.result.location.lat;
                        lng = iPToPoint.result.location.lng;
                    }

                }
                addressList.ForEach(x =>
                {
                    x.TakeRangedistance = CommondHelper.GetDistance(x.Lat, x.Lng, lat, lng);
                    x.Address_Dtl = $"{x.Province} {x.CityCode} {x.AreaCode} {x.Address}";
                });

                return Json(new { code = 1, msg = "匹配成功", obj = addressList.Where(x => x.TakeRangedistance <= 2).OrderBy(x => x.TakeRangedistance).Take(5) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { code = 2, msg = "暂无地址" }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 通过IP获取位置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>

        public ActionResult GetLocation()
        {
            //表示没有传坐标 通过客户端IP获取经纬度
            string IP = WebHelper.GetIP();
            IPToPoint iPToPoint = CommondHelper.GetLoctionByIP(IP);
            return Json(new { isok = true, msg = "ok", obj = iPToPoint }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        /// <summary>
        /// 获取所有产品分类
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodTypes(int aid)
        {
            List<EntGoodType> goodTypeList = EntGoodTypeBLL.SingleModel.GetList($"aid={aid} and state=1", 2000, 1, "*", "sort desc,id asc");
            return Json(new { isok = true, msg = "获取成功", data = goodTypeList }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询产品
        /// storeId>0查询门店产品
        /// storeId=0或不传查询主店产品
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="typeid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult GetGoodsList(int aid, string typeid = "", int pageindex = 1, int pagesize = 10)
        {

            string search = Context.GetRequest("search", "");
            int plabels = Context.GetRequestInt("plabels", 0);
            int ptype = Context.GetRequestInt("ptype", 0);
            int ptag = Context.GetRequestInt("ptag", -1);
            int storeid = Context.GetRequestInt("storeId", 0);
            string strwhere = string.Empty;

            string sortstr = " sort desc,id desc";
            if (storeid > 0)
            {
                strwhere = $"aid={aid} and substate=1 and SubTag=1 and state=1 and StoreId={storeid}";
            }
            else
            {
                strwhere = $" aid={aid} and state=1 and tag = 1 ";
            }

            if (!string.IsNullOrEmpty(typeid))
            {
                typeid = Utility.EncodeHelper.ReplaceSqlKey(typeid);
                typeid = Server.UrlDecode(typeid);
                List<string> typeidSplit = typeid.SplitStr(",");
                if (typeidSplit.Count > 0)
                {
                    typeidSplit = typeidSplit.Select(p => p = "FIND_IN_SET('" + p + "',ptypes)").ToList();
                    strwhere += $" and (" + string.Join(" or ", typeidSplit) + ")";
                }
            }

            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (search.Trim() != "")
            {
                strwhere += $" and name like @name ";
                parameters.Add(new MySqlParameter("name", $"%{search}%"));
            }
            if (plabels > 0)
            {
                strwhere += $" and  FIND_IN_SET(@plabels,plabels)";
                parameters.Add(new MySqlParameter("@plabels", plabels));
            }
            if (ptype > 0)
            {
                strwhere += $" and FIND_IN_SET (@ptype,ptypes) ";
                parameters.Add(new MySqlParameter("ptype", ptype));
            }
            if (ptag > -1)
            {
                strwhere += $" and subtag={ptag} ";
            }
            int levelid = Utility.IO.Context.GetRequestInt("levelid", 0);
            VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");//获取会员等级信息
            string selSql = string.Empty;
            List<EntGoods> goodList = new List<EntGoods>();
            if (storeid > 0)
            {
                selSql = $"select * from substoregoodsview where {strwhere} limit {(pageindex - 1) * pagesize},{pagesize}";
                DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, selSql, parameters.ToArray()).Tables[0];
                List<SubStoreGoodsView> subgoodList = DataHelper.ConvertDataTableToList<SubStoreGoodsView>(dt);
                goodList = subgoodList?.Select<SubStoreGoodsView, EntGoods>(s => new EntGoods()
                {
                    id = s.Pid,
                    aid = s.Aid,
                    name = s.name,
                    img = s.img,
                    showprice = s.showprice,
                    ptypes = s.ptypes,
                    exttypes = s.exttypes,
                    exttypesstr = s.exttypesstr,
                    ptypestr = s.ptypestr,
                    stockLimit = s.stockLimit,
                    plabels = s.plabels,
                    plabelstr = s.plabelstr,
                    specificationkeys = s.specificationkeys,
                    specification = s.specification,
                    pickspecification = s.pickspecification,
                    price = s.price,
                    unit = s.unit,
                    slideimgs = s.slideimgs,
                    description = s.description,
                    addtime = s.addtime,
                    sort = s.sort,

                    specificationdetail = s.SubSpecificationdetail,
                    stock = s.SubStock,
                    storeId = s.StoreId,
                }).ToList();
            }
            else
            {
                goodList = EntGoodsBLL.SingleModel.GetListByParam(strwhere, parameters.ToArray(), pagesize, pageindex, "*", sortstr);
            }

            goodList.ForEach(x =>
            {

                VipLevelBLL.SingleModel.CalculateVipGoodsPrice(x, level);
                if (!string.IsNullOrEmpty(x.ptypes))
                {

                    string sql = $"SELECT GROUP_CONCAT(`name` order by sort desc) from entgoodtype where FIND_IN_SET(id,@ptypes)";
                    x.ptypestr = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                        CommandType.Text, sql,
                        new MySqlParameter[] { new MySqlParameter("@ptypes", x.ptypes) }).ToString();
                }

                if (!string.IsNullOrEmpty(x.plabels))
                {

                    string sql = $"SELECT group_concat(name order by sort desc) from entgoodlabel where FIND_IN_SET(id,@plabels)";
                    x.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(),
                        CommandType.Text, sql,
                        new MySqlParameter[] { new MySqlParameter("@plabels", x.plabels) }).ToString();
                    x.plabelstr_array = x.plabelstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            });
            return Json(new { isok = true, msg = goodList }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 查询产品
        /// storeId>0查询门店产品
        /// storeId=0或不传查询主店产品
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="typeid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult GetGoodsList_v2(int aid, string typeid = "", int pageindex = 1, int pagesize = 10)
        {
            string search = Context.GetRequest("search", "");
            int plabels = Context.GetRequestInt("plabels", 0);
            int ptype = Context.GetRequestInt("ptype", 0);
            int ptag = Context.GetRequestInt("ptag", -1);
            int storeid = Context.GetRequestInt("storeId", 0);
            int levelid = Context.GetRequestInt("levelid", 0);
            int goodtype = Context.GetRequestInt("goodtype", (int)EntGoodsType.普通产品);

            List<EntGoods> goodList = new List<EntGoods>();
            if (storeid > 0)
            {
                DataTable dt = EntGoodsBLL.SingleModel.GetMStoreEntGoodsList_Sub(aid, search, plabels, ptype, ptag, storeid, typeid, pageindex, pagesize, goodtype);
                List<SubStoreGoodsView> subgoodList = DataHelper.ConvertDataTableToList<SubStoreGoodsView>(dt);
                goodList = subgoodList?.Select<SubStoreGoodsView, EntGoods>(s => new EntGoods()
                {
                    id = s.Pid,
                    aid = s.Aid,
                    name = s.name,
                    img = s.img,
                    showprice = s.showprice,
                    ptypes = s.ptypes,
                    exttypes = s.exttypes,
                    exttypesstr = s.exttypesstr,
                    ptypestr = s.ptypestr,
                    stockLimit = s.stockLimit,
                    plabels = s.plabels,
                    plabelstr = s.plabelstr,
                    specificationkeys = s.specificationkeys,
                    specification = s.specification,
                    pickspecification = s.pickspecification,
                    price = s.price,
                    unit = s.unit,
                    slideimgs = s.slideimgs,
                    description = s.description,
                    addtime = s.addtime,
                    sort = s.sort,
                    specificationdetail = s.SubSpecificationdetail,
                    stock = s.SubStock,
                    storeId = s.StoreId,
                }).ToList();
            }
            else
            {
                goodList = EntGoodsBLL.SingleModel.GetMStoreEntGoodsList_Store(aid, search, plabels, ptype, ptag, storeid, typeid, pageindex, pagesize, goodtype);
            }

            //获取商品类型和标签
            goodList = EntGoodsBLL.SingleModel.GetTypeAndLabelList(goodList, levelid);

            //获取拼团
            if (goodtype == (int)EntGoodsType.拼团产品 && goodList != null && goodList.Count > 0)
            {
                goodList = EntGroupsRelationBLL.SingleModel.GetEntGoodRelation(goodList,aid, storeid);
            }

            return Json(new { isok = true, msg = goodList }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 产品详情
        /// 如果不传storeId或者storeId=0查询总店产品，否则查询分店产品
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodInfo()
        {
            int pid = Utility.IO.Context.GetRequestInt("pid", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int aid = Context.GetRequestInt("aid", 0);
            if (pid == 0 || storeId < 0 || (storeId > 0 && aid == 0))
            {
                return Json(new { isok = false, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
            }
            ent.EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(pid);
            if (storeId > 0)
            {
                SubStoreEntGoods subGood = SubStoreEntGoodsBLL.SingleModel.GetModelByAppIdStoreIdGoodsId(aid, storeId, pid);
                if (subGood == null)
                {
                    return Json(new { isok = false, msg = "产品不存在或已下架！" }, JsonRequestBehavior.AllowGet);
                }
                goodModel.specificationdetail = subGood.SubSpecificationdetail;
                goodModel.stock = subGood.SubStock;
            }

            if (goodModel == null || goodModel.state == 0)
            {
                return Json(new { isok = false, msg = "产品不存在或已删除！" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrEmpty(goodModel.plabels))
            {
                goodModel.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT group_concat(name order by sort desc) from entgoodlabel where id in ({goodModel.plabels})").ToString();
                goodModel.plabelstr_array = goodModel.plabelstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            
            #region 会员折扣显示
            int levelid = Utility.IO.Context.GetRequestInt("levelid", 0);
            VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");
            VipLevelBLL.SingleModel.CalculateVipGoodsPrice(goodModel, level);
            #endregion
            
            return Json(new { isok = true, msg = goodModel }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 产品详情
        /// 如果不传storeId或者storeId=0查询总店产品，否则查询分店产品
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodStockState()
        {


            int goodId = Context.GetRequestInt("goodId", 0);
            string specId = Context.GetRequest("specId", string.Empty);
            int storeId = Context.GetRequestInt("storeId", 0);


            if (string.IsNullOrEmpty(_appId))
            {
                return Json(new { isok = false, msg = "appid不能为空", storeId = storeId, goodId = goodId, goodState = 0 }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权", storeId = storeId, goodId = goodId, goodState = 0 }, JsonRequestBehavior.AllowGet);
            }
            FootBath store = FootBathBLL.SingleModel.GetModel(storeId);

            ent.EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(goodId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "找不到该店铺", storeId = storeId, goodId = goodId, goodState = 0 }, JsonRequestBehavior.AllowGet);
            }
            int goodState = 0;//说明商品可正常购买
            //分店
            if (store.HomeId > 0)
            {
                SubStoreEntGoods subGood = SubStoreEntGoodsBLL.SingleModel.GetModelByAppIdStoreIdGoodsId(umodel.Id, storeId, goodId);
                if (subGood == null)
                {
                    goodState = 3;//商品在当前店铺无库存;
                }
                else
                {
                    goodModel.specificationdetail = subGood.SubSpecificationdetail;
                    goodModel.stock = subGood.SubStock;

                    if (goodModel.stockLimit && goodModel.stock <= 0)
                    {
                        goodState = 3;//商品在当前店铺无库存;
                    }
                    if (!string.IsNullOrWhiteSpace(specId))
                    {
                        if (goodModel.stockLimit && !subGood.GASDetailList.Any(x => x.id.Equals(specId) && x.stock > 0))
                        {
                            goodState = 3;//商品在当前店铺无库存;
                        }
                    }
                }

            }
            else
            {
                if (goodModel == null)
                {
                    goodState = 3;//商品在当前店铺无库存;
                }
                else
                {
                    if (goodModel.stockLimit && goodModel.stock <= 0)
                    {
                        goodState = 3;//商品在当前店铺无库存;
                    }
                    if (!string.IsNullOrWhiteSpace(specId) && goodModel.stockLimit)
                    {
                        if (!goodModel.GASDetailList.Any(x => x.id.Equals(specId) && x.stock > 0))
                        {
                            goodState = 3;//商品在当前店铺无库存;
                        }
                    }
                }
            }

            #region 会员折扣显示
            int levelid = Utility.IO.Context.GetRequestInt("levelid", 0);
            VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");
            VipLevelBLL.SingleModel.CalculateVipGoodsPrice(goodModel, level);
            #endregion

            return Json(new { isok = true, storeId = storeId, goodId = goodId, goodState = goodState, goodModel = goodModel }, JsonRequestBehavior.AllowGet);
        }

        #region 收货地址

        /// <summary>
        ///  添加/编辑收货地址
        /// </summary>
        /// <param name="State"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddOrEditMyAddress(string appId, string openId, string addressjson)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }

            var address = JsonConvert.DeserializeObject<FoodAddress>(addressjson);
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

            if (string.IsNullOrWhiteSpace(address.Address))
            {
                return Json(new { isok = false, msg = "请填写详细地址" }, JsonRequestBehavior.AllowGet);
            }
            if (address.Id <= 0)
            {
                address.appId = umodel.Id;
                address.UserId = userInfo.Id;
                address.CreateDate = DateTime.Now;
                address.appId = umodel.Id;
                address.Id = Convert.ToInt32(FoodAddressBLL.SingleModel.Add(address));
                if (address.Id <= 0)
                {
                    return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var model = FoodAddressBLL.SingleModel.GetModel(address.Id);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "保存失败" }, JsonRequestBehavior.AllowGet);
                }
                model.NickName = address.NickName;
                model.Name = address.Name;
                model.TelePhone = address.TelePhone;
                model.Address = address.Address;
                model.Lat = address.Lat;
                model.Lng = address.Lng;

                if (!FoodAddressBLL.SingleModel.Update(model))
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
        public ActionResult GetMyAddress(string appId, string openId, int addressId = 0, int isDefault = 0)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }

            //返回默认地址
            if (isDefault == 1)
            {
                var address = FoodAddressBLL.SingleModel.GetModel($" AppId = {umodel.Id} and UserId = {userInfo.Id} and State = 0 and IsDefault = 1 ");
                if (address == null)
                {
                    return Json(new { isok = false, msg = "地址信息不存在" }, JsonRequestBehavior.AllowGet);
                }
                //拼接地址名称
                var provinceName = address.Province;
                var cityName = address.CityCode;
                var areaName = address.AreaCode;

                address.Address_Dtl = $"{provinceName} {cityName} {areaName} {address.Address}";

                var data = new { address = address };
                return Json(new { isok = true, msg = "成功", data = data }, JsonRequestBehavior.AllowGet);
            }


            if (addressId == 0)
            {
                var addressList = FoodAddressBLL.SingleModel.GetList($" AppId = {umodel.Id} and UserId = {userInfo.Id} and State = 0 ");
                //拼接地址名称
                addressList.ForEach(x =>
                {
                    var provinceName = x.Province;
                    var cityName = x.CityCode;
                    var areaName = x.AreaCode;

                    x.Address_Dtl = $"{provinceName} {cityName} {areaName} {x.Address}";
                });



                //var data = new { addressList = addressList.Select(s => new { s.Address, s.Id, s.Province, s.CityCode, s.AreaCode, s.NickName, s.TelePhone, IsDefault = (s.IsDefault == 0 ? "正常" : "已删除") }) };

                return Json(new { isok = true, msg = "成功", data = addressList }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var address = FoodAddressBLL.SingleModel.GetModel(addressId);
                if (address == null)
                {
                    return Json(new { isok = false, msg = "地址信息不存在" }, JsonRequestBehavior.AllowGet);
                }
                //拼接地址名称
                var provinceName = address.Province;
                var cityName = address.CityCode;
                var areaName = address.AreaCode;

                address.Address_Dtl = $"{provinceName} {cityName} {areaName} {address.Address}";

                var data = new { address = address };
                return Json(new { isok = true, msg = "成功", data = data }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        ///  设定默认收货地址
        /// </summary>
        /// <param name="State"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetMyAddressDefault(string appId, string openId, int addressId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            var address = FoodAddressBLL.SingleModel.GetModel(addressId);
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
        public ActionResult DeleteMyAddress(string appId, string openId, int addressId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            var address = FoodAddressBLL.SingleModel.GetModel(addressId);
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

        #region 购物车
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodId"></param>
        /// <param name="attrSpacStr"></param>
        /// <param name="SpecInfo">商品规格(格式)：规格1：属性1 规格2：属性2 如:（颜色：白色 尺码：M）</param>
        /// <param name="qty"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddGoodsCarData(int goodId, string attrSpacStr, string SpecInfo, int qty, int newCartRecord = 0)
        {
            //是否拼团，0：不拼团，1;拼团
            int isgroup = Context.GetRequestInt("isgroup",0);
            //分店必须传，主店不用传或传0
            int storeid = Context.GetRequestInt("storeId", 0);

            #region 基本验证
            if (string.IsNullOrEmpty(_appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            if (qty <= 0)
            {
                return Json(new { isok = false, msg = "数量必须大于0" }, JsonRequestBehavior.AllowGet);
            }
            EntGoods good = EntGoodsBLL.SingleModel.GetModel(goodId);
            if (good == null)
            {
                return Json(new { isok = false, msg = "未找到该商品" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrWhiteSpace(attrSpacStr))
            {
                if (!good.GASDetailList.Any(x => x.id.Equals(attrSpacStr)))
                {
                    return Json(new { isok = false, msg = "未找到该商品" }, JsonRequestBehavior.AllowGet);
                }
            }
            if (good.state != 1)
            {
                return Json(new { isok = false, msg = "无法添加失效商品" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appId, _openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            EntGoodsCart dbGoodCar = EntGoodsCartBLL.SingleModel.GetModel($" UserId={userInfo.Id} and FoodGoodsId={good.id} and SpecIds='{attrSpacStr}' and State = 0 ");
            if (dbGoodCar == null || newCartRecord == 1)
            {
                int price = 0;
                //判断是否是拼团
                if(isgroup>0)
                {
                    good.storeId = storeid;
                    string groupmsg = CheckGoodCount(userInfo.Id,qty,good,attrSpacStr,ref price);
                    if (!string.IsNullOrEmpty(groupmsg))
                    {
                        return Json(new { isok = -1, msg = groupmsg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    price = Convert.ToInt32(!string.IsNullOrWhiteSpace(attrSpacStr) ? good.GASDetailList.First(x => x.id.Equals(attrSpacStr)).price * 100 : good.price * 100);
                }
                
                var goodsCar = new EntGoodsCart
                {
                    //FoodId = store.Id,
                    FoodGoodsId = good.id,
                    SpecIds = attrSpacStr,
                    Count = qty,
                    Price = price,
                    SpecInfo = SpecInfo,
                    UserId = userInfo.Id,
                    CreateDate = DateTime.Now,
                    State = 0,
                    aId = umodel.Id
                };
                //加入购物车

                int id = Convert.ToInt32(EntGoodsCartBLL.SingleModel.Add(goodsCar));
                if (id > 0)
                {
                    return Json(new { isok = true, msg = "成功", cartid = id }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { isok = false, msg = "失败", cartid = 0 }, JsonRequestBehavior.AllowGet);
            }
            dbGoodCar.Count += qty;
            if (EntGoodsCartBLL.SingleModel.Update(dbGoodCar, "Count"))
            {
                return Json(new { isok = true, msg = "成功", cartid = dbGoodCar.Id }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 从购物车 删除商品/更新数量
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="goodsCarId"></param>
        /// <param name="function">0为更新,-1为删除</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateOrDeleteGoodsCarDataBySingle(string appId, string openId, EntGoodsCart item, int function)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (item != null)
            {
                var goodsCar = EntGoodsCartBLL.SingleModel.GetModel(item.Id);
                if (goodsCar == null)
                {
                    return Json(new { isok = false, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
                }
                if (goodsCar.UserId != userInfo.Id)
                {
                    return Json(new { isok = false, msg = "该记录不属于当前用户" }, JsonRequestBehavior.AllowGet);
                }
                if (goodsCar.State == 1)
                {
                    return Json(new { isok = false, msg = "不可修改此记录" }, JsonRequestBehavior.AllowGet);
                }
                //将记录状态改为删除
                if (function == -1)
                {
                    goodsCar.State = -1;
                }
                else if (function == 0)//根据传入参数更新购物车内容
                {
                    goodsCar.SpecIds = item.SpecIds;
                    goodsCar.SpecInfo = item.SpecInfo;
                    goodsCar.Count = item.Count;
                }

                var success = EntGoodsCartBLL.SingleModel.Update(goodsCar, "State,Count,SpecInfo,SpecIds");
                if (!success)
                {
                    return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 从购物车 删除商品/更新商品
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="goodsCarId"></param>
        /// <param name="function">0为更新,-1为删除</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateOrDeleteGoodsCarData(string appId, string openId, List<EntGoodsCart> goodsCarModel, int function)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            var umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }

            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (goodsCarModel != null && goodsCarModel.Count > 0)
            {
                string cartIds = string.Join(",",goodsCarModel.Select(s=>s.Id).Distinct());
                List<EntGoodsCart> entGoodsCartList = EntGoodsCartBLL.SingleModel.GetListByIds(cartIds);
                foreach (var item in goodsCarModel)
                {
                    var goodsCar = entGoodsCartList?.FirstOrDefault(f=>f.Id == item.Id);
                    if (goodsCar == null)
                    {
                        return Json(new { isok = false, msg = "找不到记录" }, JsonRequestBehavior.AllowGet);
                    }
                    if (goodsCar.UserId != userInfo.Id)
                    {
                        return Json(new { isok = false, msg = "该记录不属于当前用户" }, JsonRequestBehavior.AllowGet);
                    }
                    if (goodsCar.State == 1)
                    {
                        return Json(new { isok = false, msg = "不可修改此记录" }, JsonRequestBehavior.AllowGet);
                    }
                    //将记录状态改为删除
                    if (function == -1)
                    {
                        goodsCar.State = -1;
                    }
                    else if (function == 0)//根据传入参数更新购物车内容
                    {
                        goodsCar.SpecIds = item.SpecIds;
                        goodsCar.SpecInfo = item.SpecInfo;
                        goodsCar.Count = item.Count;
                    }

                    var success = EntGoodsCartBLL.SingleModel.Update(goodsCar, "State,Count,SpecInfo,SpecIds,Price,GoodsState");
                    if (!success)
                    {
                        return Json(new { isok = false, msg = "失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取我的购物车信息
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetGoodsCarData()
        {
            string BUG = "";
            try
            {
                int storeId = Context.GetRequestInt("storeId", 0);
                int pageIndex = Context.GetRequestInt("pageIndex", 1);
                int pageSize = Context.GetRequestInt("pageSize", 6);

                #region 数据验证
                if (string.IsNullOrEmpty(_appId))
                {
                    return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
                }
                XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
                if (umodel == null)
                {
                    return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
                }
                if (storeId < 0)
                {
                    return Json(new { isok = false, msg = "storeId不能为空" }, JsonRequestBehavior.AllowGet);
                }
                FootBath storeMaterial = FootBathBLL.SingleModel.GetModel(storeId);
                if (storeMaterial == null)
                {
                    return Json(new { isok = false, msg = "店铺资料错误" }, JsonRequestBehavior.AllowGet);
                }

                var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appId, _openId);
                if (userInfo == null)
                {
                    return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
                }
                int levelid = Context.GetRequestInt("levelid", 0);
                #endregion

                VipLevel level = VipLevelBLL.SingleModel.GetModel($"id={levelid} and state>=0");//获取会员等级信息

                List<EntGoodsCart> myCartList = EntGoodsCartBLL.SingleModel.GetMyCart(umodel.Id, userInfo.Id);
                //计算在当前门店的价格和优惠
                CalculateGoodsCartPrice(myCartList, umodel, storeMaterial, userInfo);
                var data = myCartList;
                return Json(new { isok = true, msg = "成功", data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message + BUG }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// 计算购物车内商品在当前门店的商品价格、库存是否足够
        /// </summary>
        /// <param name="goodCarts"></param>
        /// <param name="xcxRole"></param>
        /// <param name="storeMaterial"></param>
        /// <param name="userInfo"></param>
        public void CalculateGoodsCartPrice(List<EntGoodsCart> goodCarts, XcxAppAccountRelation xcxRole, FootBath storeMaterial, C_UserInfo userInfo)
        {
            StringBuilder sbUpdateCartPriceSql = null;

            CalculateGoodsCartPrice(goodCarts, xcxRole, storeMaterial, out sbUpdateCartPriceSql, userInfo);
        }


        /// <summary>
        /// 计算购物车内商品在当前门店的商品价格、库存是否足够,并生成更新购物车价格及减少商品库存的sql
        /// </summary>
        /// <param name="goodCarts"></param>
        /// <param name="xcxRole"></param>
        /// <param name="storeMaterial"></param>
        /// <param name="beforeDiscountPrice">优惠前金额</param>
        /// <param name="afterDiscountPrice">优惠后金额</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public void CalculateGoodsCartPrice(List<EntGoodsCart> goodCarts, XcxAppAccountRelation xcxRole, FootBath storeMaterial, out StringBuilder sbUpdateCartPriceSql, C_UserInfo userInfo)
        {
            sbUpdateCartPriceSql = null;
            if (goodCarts == null || !goodCarts.Any() || xcxRole == null || storeMaterial == null || userInfo == null)
            {
                return;
            }

            #region 获取店铺内相关商品列表
            List<EntGoods> goods = null;
            if (storeMaterial.HomeId == 0)
            {
                goods = EntGoodsBLL.SingleModel.GetListByParam($" id in ({string.Join(",", goodCarts.Select(x => x.FoodGoodsId))}) and state = 1 and tag = 1 ", null);
            }
            else
            {
                string selSql = $"select * from substoregoodsview where pid in ({string.Join(",", goodCarts.Select(x => x.FoodGoodsId))}) and aid={xcxRole.Id} and substate=1 and SubTag = 1 and state=1 and StoreId={storeMaterial.Id} ";
                DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, selSql).Tables[0];
                List<SubStoreGoodsView> subgoodList = DataHelper.ConvertDataTableToList<SubStoreGoodsView>(dt);
                goods = subgoodList?.Select<SubStoreGoodsView, EntGoods>(s => new EntGoods()
                {
                    id = s.Pid,
                    aid = s.Aid,
                    name = s.name,
                    img = s.img,
                    showprice = s.showprice,
                    ptypes = s.ptypes,
                    exttypes = s.exttypes,
                    exttypesstr = s.exttypesstr,
                    ptypestr = s.ptypestr,
                    stockLimit = s.stockLimit,
                    plabels = s.plabels,
                    plabelstr = s.plabelstr,
                    specificationkeys = s.specificationkeys,
                    specification = s.specification,
                    pickspecification = s.pickspecification,
                    price = s.price,
                    unit = s.unit,
                    slideimgs = s.slideimgs,
                    description = s.description,
                    addtime = s.addtime,
                    sort = s.sort,

                    specificationdetail = s.SubSpecificationdetail,
                    stock = s.SubStock,
                    storeId = s.StoreId,
                    subGoodId = s.Id
                }).ToList();
            }
            #endregion

            #region 获取当前店铺下购物车内商品的价格及是否售罄,库存是否足够
            EntGoods curGood = null;
            EntGoodsAttrDetail curGoodAttrDtl = null;

            //考虑可能存在提交的购物车有重复的商品
            var car = goodCarts.GroupBy(x => new { x.FoodGoodsId, x.SpecIds }).Select(g => new { FoodGoodsId = g.Key.FoodGoodsId, SpecIds = g.Key.SpecIds, Count = g.Sum(x => x.Count) });
            goodCarts.ForEach(x =>
            {
                x.GoodsState = 0;
                if (goods != null)
                {
                    curGood = goods.Where(y => y.id == x.FoodGoodsId)?.FirstOrDefault();
                    if (curGood != null && curGood.id > 0)
                    {
                        x.goodsMsg = curGood;
                        x.GoodName = curGood.name;//商品名
                        x.SubGoodId = curGood.subGoodId;//分店商品ID

                        //规格商品价格获取及库存情况判定
                        if (!string.IsNullOrWhiteSpace(x.SpecIds))
                        {
                            curGoodAttrDtl = curGood.GASDetailList.Where(y => y.id.Equals(x.SpecIds) && y.stock > 0)?.FirstOrDefault();
                            if (curGoodAttrDtl != null && !string.IsNullOrWhiteSpace(curGoodAttrDtl.id))//子店铺商品存在要购买的规格商品
                            {
                                var curCar = car.Where(y => y.FoodGoodsId == x.FoodGoodsId && y.SpecIds == x.SpecIds)?.First(); //当前商品在购物车内需求的数量对象
                                x.Price = Convert.ToInt32(curGoodAttrDtl.price * 100);//获取价格


                                if (curGood.stockLimit)//限制库存才处理库存情况列
                                {
                                    if (curGoodAttrDtl.stock <= 0)
                                    {
                                        x.GoodsState = 3;//当前店铺售罄
                                    }
                                    else if (curCar != null && curGoodAttrDtl.stock < curCar.Count)
                                    {
                                        x.GoodsState = 4; //库存不足
                                    }
                                }
                            }
                            else //子店铺商品不存在要购买的规格商品
                            {
                                if (curGood.stockLimit)
                                {
                                    x.GoodsState = 3;//当前店铺售罄
                                }
                            }
                        }
                        //无规格商品处理
                        else
                        {
                            x.Price = Convert.ToInt32(curGood.price * 100);//获取价格
                            var curCar = car.Where(y => y.FoodGoodsId == x.FoodGoodsId && string.IsNullOrWhiteSpace(y.SpecIds))?.First(); //当前商品在购物车内需求的数量对象


                            if (curGood.stockLimit)//限制库存才处理库存情况列
                            {
                                if (curGood.stock <= 0)
                                {
                                    x.GoodsState = 3;//当前店铺售罄
                                }
                                else if (curCar != null && curGood.stock < curCar.Count)
                                {
                                    x.GoodsState = 4; //库存不足
                                }
                            }
                        }
                    }
                    else
                    {
                        x.GoodsState = 3;//当前店铺售罄
                    }
                }
                else
                {
                    x.GoodsState = 3;//当前店铺售罄
                }
            });

            //查找商品名
            string goodsIds = string.Join(",",goodCarts.Select(s=>s.FoodGoodsId));
            List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);
            foreach (EntGoodsCart goodCart in goodCarts.Where(x => x.GoodName.IsNullOrWhiteSpace()))
            {
                goodCart.GoodName = entGoodsList?.FirstOrDefault(f=>f.id == goodCart.FoodGoodsId)?.name;
            }
            #endregion

            #region 会员处理价格

            #region 默认折后价赋值
            List<EntGoodsAttrDetail> curAttrDetail = null;
            goodCarts.ForEach(g =>
            {
                g.originalPrice = g.Price;
                g.discount = 100;

                if (g.goodsMsg == null)
                {
                    g.goodsMsg = entGoodsList?.FirstOrDefault(f=>f.id == g.FoodGoodsId);
                }
                if (g.goodsMsg != null)
                {
                    g.goodsMsg.discountPrice = g.goodsMsg.price;

                    if (g.goodsMsg.GASDetailList != null && g.goodsMsg.GASDetailList.Count > 0)
                    {
                        curAttrDetail = g.goodsMsg.GASDetailList;
                        curAttrDetail.ForEach(x =>
                        {
                            x.discount = 100;
                            x.discountPrice = x.price;
                        });
                        g.goodsMsg.specificationdetail = JsonConvert.SerializeObject(curAttrDetail);
                    }
                }

            });
            #endregion

            //获取会员信息
            VipRelation vipInfo = VipRelationBLL.SingleModel.GetModel($"uid={userInfo.Id} and state>=0");
            if (vipInfo != null)
            {
                VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={vipInfo.levelid} and state>=0");

                if (levelinfo != null)
                {
                    if (levelinfo.type == 1)//全场打折
                    {
                        goodCarts.ForEach(g =>
                        {
                            g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01));
                            g.discount = levelinfo.discount;
                        });

                    }
                    else if (levelinfo.type == 2)//部分打折
                    {
                        List<string> gids = levelinfo.gids.Split(',').ToList();
                        goodCarts.ForEach(g =>
                        {
                            //处理购物车附带的商品信息折后价格
                            VipLevelBLL.SingleModel.CalculateVipGoodsPrice(g.goodsMsg, levelinfo);
                            if (gids.Contains(g.FoodGoodsId.ToString()))
                            {
                                g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01));
                                g.discount = levelinfo.discount;
                            }
                        });
                    }
                    sbUpdateCartPriceSql = new StringBuilder();

                    foreach (var item in goodCarts)
                    {
                        sbUpdateCartPriceSql.Append(EntGoodsCartBLL.SingleModel.BuildUpdateSql(item, "SubGoodId,Price,originalPrice") + ";");
                    }

                }
            }
            #endregion
        }

        /// <summary>
        /// 获取指定的购物车信息
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetGoodsCarDataByIds()
        {

            try
            {
                int storeId = Context.GetRequestInt("storeId", 0);
                string cartIdStr = Context.GetRequest("cartIds", "");

                List<int> carIds = new List<int>();

                #region 数据验证
                if (string.IsNullOrEmpty(_appId))
                {
                    return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(cartIdStr))
                {
                    return Json(new { isok = false, msg = "没有购物车记录" }, JsonRequestBehavior.AllowGet);
                }
                carIds = cartIdStr.Trim().Split(',').Select(x => Convert.ToInt32(x)).ToList(); ;


                XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
                if (umodel == null)
                {
                    return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
                }
                FootBath storeMaterial = FootBathBLL.SingleModel.GetModel(storeId);
                if (storeMaterial == null)
                {
                    return Json(new { isok = false, msg = "店铺资料错误" }, JsonRequestBehavior.AllowGet);
                }
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appId, _openId);
                if (userInfo == null)
                {
                    return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                List<EntGoodsCart> myCartList = EntGoodsCartBLL.SingleModel.GetList($" id in ({string.Join(",", carIds)})");
                CalculateGoodsCartPrice(myCartList, umodel, storeMaterial, userInfo);//处理价格及获取库存情况

                var data = myCartList;
                return Json(new { isok = true, msg = "成功", data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 判定购物车商品在当前门店是否有库存
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SearchGoodCarStockForStore()
        {

            try
            {
                int storeId = Context.GetRequestInt("storeId", 0);

                #region 数据验证
                if (string.IsNullOrEmpty(_appId))
                {
                    return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
                }
                XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
                if (umodel == null)
                {
                    return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
                }
                if (storeId < 0)
                {
                    return Json(new { isok = false, msg = "storeId不能为空" }, JsonRequestBehavior.AllowGet);
                }
                FootBath storeMaterial = FootBathBLL.SingleModel.GetModel(storeId);
                if (storeMaterial == null)
                {
                    return Json(new { isok = false, msg = "店铺资料错误" }, JsonRequestBehavior.AllowGet);
                }

                var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appId, _openId);
                if (userInfo == null)
                {
                    return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                List<EntGoodsCart> myCartList = EntGoodsCartBLL.SingleModel.GetMyCart(umodel.Id, userInfo.Id);
                //计算在当前门店的价格和优惠
                CalculateGoodsCartPrice(myCartList, umodel, storeMaterial, userInfo);

                int[] stockZeroCartIds = myCartList.Where(x => x.GoodsState == 3).Select(x => x.Id).ToArray();

                return Json(new { isok = true, msg = "成功", data = stockZeroCartIds }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        #region 订单
        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodCarIdStr">购物车ID集合字符串：格式为(id1,id2)</param>
        /// <param name="effGoodCarIdStr">真正要购买下单的购物车ID集合字符串：格式为(id1,id2)</param>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddMiniappGoodsOrder(string goodCarIdStr, string effGoodCarIdStr, string orderjson, int orderType = (int)multiStoreOrderType.到店自取, int buyMode = (int)miniAppBuyMode.微信支付, double lat = 0.00, double lng = 0.00)
        {
            //判断是否发起拼团
            int isgroup = Context.GetRequestInt("isgroup", 0);//开团：1，不开团：0
            int groupid = Context.GetRequestInt("groupid", 0);//团ID
            int goodtype = Context.GetRequestInt("goodtype", 0);//是否是团产品，0：不是，1：是

            string dugmsg = "dugmsg";//无关逻辑,用于码农定位bug

            //用户领取的优惠券记录ID
            int couponLogId = Context.GetRequestInt("couponlogid", 0);
            
            //随订单生成一起执行的sql操作(执行于订单生成前,意味只能执行不会使用到orderId的sql)
            StringBuilder extraSql = new StringBuilder();

            //总店信息(用于判定一些配置是否开启)
            FootBath store_Home = null;

            #region  传入数据 - 基础验证
            if (string.IsNullOrEmpty(_appId))
            {
                return Json(new { isok = false, msg = "店铺未被授权" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "店铺的授权过期" }, JsonRequestBehavior.AllowGet);
            }
            //不同商家，不同的锁,当前商家若还未创建，则创建一个
            if (!lockObjectDict_Order.ContainsKey(umodel.Id))
            {
                if (!lockObjectDict_Order.TryAdd(umodel.Id, new object()))
                {
                    return Json(new { isok = false, msg = "店铺下单场面火热,请您稍候再试" }, JsonRequestBehavior.AllowGet);
                }
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appId, _openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "您的会员身份还未全面识别,请稍候再试" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(orderjson))
            {
                return Json(new { isok = false, msg = "未接收到订单内容" }, JsonRequestBehavior.AllowGet);
            }

            EntGoodsOrder order = null;
            try
            {
                order = JsonConvert.DeserializeObject<EntGoodsOrder>(orderjson);
                if (order == null)
                {
                    return Json(new { isok = false, msg = "订单出现了未知的内容" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = false, msg = "订单出现了异常的内容" }, JsonRequestBehavior.AllowGet);
            }
            if (order.StoreId <= 0)
            {
                return Json(new { isok = false, msg = "没有接收到您传达要下单于哪一家店的意向" }, JsonRequestBehavior.AllowGet);
            }

            //上架且为过期的门店才算有效门店
            FootBath storeMaterial = FootBathBLL.SingleModel.GetModel($" Id = {order.StoreId} and State = 1 and OverTime >= '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' ");
            if (storeMaterial == null)
            {
                return Json(new { isok = false, msg = "当前门店已经过期或者下架了,请您选择其它营业的店铺" }, JsonRequestBehavior.AllowGet);
            }
            store_Home = FootBathBLL.SingleModel.GetModel($" appId = {umodel.Id} and HomeId = 0 and State = 1 ");
            if (store_Home == null)
            {
                return Json(new { isok = false, msg = "总店的信息配置不完善导致订单无法下单,请联系商家" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                store_Home.switchModel = JsonConvert.DeserializeObject<SwitchModel>(store_Home.SwitchConfig);
            }
            catch (Exception)
            {
                store_Home.switchModel = new SwitchModel();
            }

            //快递配送不需要在营业时间段才能下单
            if (orderType != (int)multiStoreOrderType.快递配送)
            {
                bool isShopTime = FootBathBLL.SingleModel.GetStoreShopTime(storeMaterial.Id) == 1;
                if (!isShopTime)
                {
                    return Json(new { isok = false, msg = "当前门店已经打烊了,请您选择其它营业的店铺" }, JsonRequestBehavior.AllowGet);
                }
            }
            FoodAddress address = FoodAddressBLL.SingleModel.GetModel(order.AddressId) ?? new FoodAddress();
            switch (orderType)
            {
                case (int)multiStoreOrderType.到店自取:
                    bool inDistance1 = FootBathBLL.SingleModel.GetTakeWayIsUse(storeMaterial.Id, lat, lng, orderType) == 3;
                    if (!inDistance1)
                    {
                        return Json(new { isok = false, msg = "您所在的地点超出了取货许可范围!" }, JsonRequestBehavior.AllowGet);
                    }
                    break;
                case (int)multiStoreOrderType.同城配送:
                case (int)multiStoreOrderType.快递配送:
                    if (address == null || address.Id <= 0)
                    {
                        return Json(new { isok = false, msg = "您指定的收货地点传达失败" }, JsonRequestBehavior.AllowGet);
                    }
                    bool inDistance = FootBathBLL.SingleModel.GetTakeWayIsUse(storeMaterial.Id, address.Lat, address.Lng, orderType) == 3;
                    if (!inDistance)
                    {
                        return Json(new { isok = false, msg = "您选择的收货地点超出了许可范围!" }, JsonRequestBehavior.AllowGet);
                    }
                    break;
            }

            int int_TryParseId = 0;
            //所有要下单的购物车记录
            List<string> goodCarIds = goodCarIdStr?.Split(',')?.Where(c => !string.IsNullOrWhiteSpace(c)
                                                                                 && Int32.TryParse(c, out int_TryParseId))?.ToList();
            if (goodCarIds == null || !goodCarIds.Any())
            {
                return Json(new { isok = false, msg = "您选择的购物车记录没有传达到购物中心" }, JsonRequestBehavior.AllowGet);
            }
            //需要检索库存的购物车记录
            List<string> effGoodCarIds = effGoodCarIdStr?.Split(',')?.Where(ec => !string.IsNullOrWhiteSpace(ec)
                                                                                  && Int32.TryParse(ec, out int_TryParseId))?.ToList();
            if (effGoodCarIds == null || !effGoodCarIds.Any())
            {
                return Json(new { isok = false, msg = "您要下单的购物车记录没有传达到购物中心" }, JsonRequestBehavior.AllowGet);
            }
            //要检索库存的购物车记录必须都存在于所有下单的购物车中
            if (effGoodCarIds.Any(x => !goodCarIds.Contains(x)))
            {
                return Json(new { isok = false, msg = "您传达的购买意向商品与选择的商品不对应" }, JsonRequestBehavior.AllowGet);
            }
            List<EntGoodsCart> goodsCar = EntGoodsCartBLL.SingleModel.GetList($" Id in({string.Join(",", goodCarIds)}) and UserId = {userInfo.Id} and state = 0 ");
            if (goodsCar == null || goodsCar.Count <= 0)
            {
                return Json(new { isok = false, msg = "您选择的购物车记录消失了" }, JsonRequestBehavior.AllowGet);
            }
            List<EntGoodsCart> effGoodsCar = goodsCar.Where(x => effGoodCarIds.Contains(x.Id.ToString())).ToList();
            if (effGoodsCar == null || effGoodsCar.Count <= 0)
            {
                return Json(new { isok = false, msg = "您要下单的购物车记录消失了" }, JsonRequestBehavior.AllowGet);
            }
            //生成 让页面显示无库存,用户仍选中下单的购物车记录失效  的Sql
            StringBuilder sbCancleCars = new StringBuilder();
            List<EntGoodsCart> goodCars = goodsCar.Where(x => !effGoodsCar.Any(y => x.Id == y.Id)).ToList();
            goodCars.ForEach(x =>
            {
                x.State = -1;
                sbCancleCars.Append(EntGoodsCartBLL.SingleModel.BuildUpdateSql(x, "State") + ";");
            });
            extraSql.Append(sbCancleCars.ToString());//让选择的非可购买商品失效


            order.GoodsGuid = string.Join(",", effGoodsCar.Select(e => e.FoodGoodsId));
            if (!Enum.IsDefined(typeof(multiStoreOrderType), orderType))
            {
                return Json(new { isok = false, msg = $"您选择了不支持的配送方式" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            if (isgroup > 0 && groupid > 0)
            {
                return Json(new { isok = false, msg = "拼团参数错误" }, JsonRequestBehavior.AllowGet);
            }

            //拼团
            int grouperprice = 0;//团长减价
            EntGroupsRelation groupmodel = new EntGroupsRelation() { RId = umodel.Id };
            if (isgroup > 0 || groupid > 0)
            {
                int storeid = storeMaterial.HomeId == 0 ? 0 : storeMaterial.Id;
                //判断是否是拼团，如果是拼团则将产品价格改成拼团价，获取团长优惠价
                string groupmsg = CommandEntGroup(isgroup, groupid,userInfo.Id, storeid, goodsCar[0].FoodGoodsId, ref grouperprice, ref groupmodel, goodsCar[0].Count);
                if (!string.IsNullOrEmpty(groupmsg))
                {
                    return Json(new { isok = -1, msg = groupmsg }, JsonRequestBehavior.AllowGet); ;
                }
            }

            try
            {
                lock (lockObjectDict_Order[umodel.Id])
                {
                    #region 订单内容,参数拿取及计算
                    int beforeDiscountPrice = effGoodsCar.Sum(x => x.Price * x.Count);//优惠前商品总价
                    int afterDiscountPrice = beforeDiscountPrice;//优惠后商品总价

                    #region 计算商品优惠后金额
                    //更新价格sql
                    StringBuilder sbUpdateGoodPriceSql = new StringBuilder();

                    //处理购物车库存情况,及会员价格,方便后续判定及为订单赋值
                    CalculateGoodsCartPrice(effGoodsCar, umodel, storeMaterial, out sbUpdateGoodPriceSql, userInfo);
                    extraSql.Append(sbUpdateGoodPriceSql.ToString()); //更新商品价格
                    afterDiscountPrice = effGoodsCar.Sum(x => x.Price * x.Count); //商品会员打完折后的金额



                    //非拼团的时候才去计算满减优惠 及 优惠券
                    if (!(isgroup > 0 || groupid > 0))
                    {
                        //满减规则优惠金额
                        if (store_Home.switchModel.discountRuleSwitch)//若开启了满减优惠规则
                        {
                            int maxDiscountMoney = DiscountRuleBLL.SingleModel.getMaxDiscountMoney(afterDiscountPrice, umodel.Id);
                            afterDiscountPrice -= maxDiscountMoney;
                        }

                        //首单立减优惠
                        int firstOrderDiscountMoney = DiscountRuleBLL.SingleModel.getFirstOrderDiscountMoney(userInfo.Id, umodel.Id, 0, TmpType.小程序多门店模板);
                        afterDiscountPrice -= firstOrderDiscountMoney;

                        #region  优惠券金额计算
                        //优惠券优惠金额
                        int couponsum = 0;
                        if (couponLogId > 0)
                        {
                            string goodsIds = string.Join(",", effGoodsCar.Select(e => e.FoodGoodsId).Distinct());
                            string goodsInfos = JsonConvert.SerializeObject(effGoodsCar.Select(e => new
                            {
                                goodid = e.FoodGoodsId,
                                totalprice = e.Count * e.Price
                            }));

                            List <CouponLog> couponlist = CouponLogBLL.SingleModel.GetCouponList(0, userInfo.Id, store_Home.Id, umodel.Id, 1, 1, "l.addtime desc",goodsIds,goodsInfos, couponLogId);
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
                        #endregion
                    }

                    #endregion

                    #region 运费计算(必须置于商品价格计算之后)
                    try
                    {
                        storeMaterial.takeOutWayModel = JsonConvert.DeserializeObject<TakeOutWayModel>(storeMaterial.TakeOutWayConfig);
                    }
                    catch (Exception)
                    {
                        storeMaterial.takeOutWayModel = new TakeOutWayModel();
                    }

                    //不同订单类型不同的运费,总金额公式
                    switch (orderType)
                    {
                        case (int)multiStoreOrderType.到店自取:
                            order.FreightPrice = 0;
                            order.BuyPrice = afterDiscountPrice;

                            break;
                        case (int)multiStoreOrderType.同城配送:
                            //是否达到起送价
                            if (Convert.ToInt32(storeMaterial.takeOutWayModel.cityService.TakeStartPrice * 100) > beforeDiscountPrice)
                            {
                                return Json(new { isok = false, msg = "您钦点的商品总额未达到商家的起送价" }, JsonRequestBehavior.AllowGet);
                            }

                            //未达到免运费金额
                            if (Convert.ToInt32(storeMaterial.takeOutWayModel.cityService.FreeFrightCost * 100) > beforeDiscountPrice)
                            {
                                order.FreightPrice = Convert.ToInt32(storeMaterial.takeOutWayModel.cityService.TakeFright * 100);
                                order.BuyPrice = afterDiscountPrice + order.FreightPrice;
                            }
                            else
                            {
                                order.FreightPrice = 0;
                                order.BuyPrice = afterDiscountPrice;
                            }

                            break;
                        case (int)multiStoreOrderType.快递配送:
                            //未达到免运费金额
                            if (Convert.ToInt32(storeMaterial.takeOutWayModel.GetExpressdelivery.FreeFrightCost * 100) > beforeDiscountPrice)
                            {
                                order.FreightPrice = Convert.ToInt32(storeMaterial.takeOutWayModel.GetExpressdelivery.TakeFright * 100);
                                order.BuyPrice = afterDiscountPrice + order.FreightPrice;
                            }
                            else
                            {
                                order.FreightPrice = 0;
                                order.BuyPrice = afterDiscountPrice;
                            }

                            break;
                    }
                    #endregion

                    //验证库存
                    List<string> lowStockGoodNames = effGoodsCar.Where(x => x.GoodsState == 3 || x.GoodsState == 4).Select(x => x.GoodName).ToList();
                    if (lowStockGoodNames.Count > 0)
                    {
                        return Json(new { isok = false, msg = $"您钦点的商品中: {string.Join(",", lowStockGoodNames)} 库存不足" }, JsonRequestBehavior.AllowGet);
                    }

                    if (address != null)
                    {
                        order.AccepterName = address.NickName;
                        order.AccepterTelePhone = address.TelePhone;
                        order.ZipCode = address.ZipCode;
                        order.Address = $"{address.Address}";
                    }

                    order.StoreId = storeMaterial.Id;
                    order.TemplateType = (int)TmpType.小程序多门店模板;
                    order.UserId = userInfo.Id;
                    order.CreateDate = DateTime.Now;
                    order.State = (int)MiniAppEntOrderState.待付款;
                    order.aId = umodel.Id;
                    order.QtyCount = effGoodsCar.Sum(x => x.Count);
                    order.BuyMode = buyMode;
                    order.GetWay = orderType;
                    order.ReducedPrice = beforeDiscountPrice - afterDiscountPrice;
                    order.ReducedPrice += grouperprice;//加上团长优惠价格
                    order.BuyPrice -= grouperprice;//减去团长优惠价格
                    order.BuyPrice = order.BuyPrice <= 0 ? 0 : order.BuyPrice;

                    //对外订单号生成
                    order.OrderNum = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{random_MultiStore.Next(999).ToString().PadLeft(3, '0')}";
                    #endregion
                    //不同支付方式的验证
                    switch (buyMode)
                    {
                        case (int)miniAppBuyMode.微信支付:

                            break;
                        case (int)miniAppBuyMode.储值支付:
                            SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(umodel.AppId, userInfo.Id);
                            if (saveMoneyUser == null || saveMoneyUser.AccountMoney < order.BuyPrice)
                            {
                                return Json(new { isok = false, msg = $" 您的预存款余额不够啦,建议充值后再继续购买！ " }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        default:
                            return Json(new { isok = false, msg = $" 十分抱歉,暂不支持这样的支付方式！ " }, JsonRequestBehavior.AllowGet);
                    }
                    string createOrderErrorMsg = string.Empty;//订单生成过程中的错误节点信息
                    //生成订单
                    if (!EntGoodsOrderBLL.SingleModel.AddGoodsOrder_MultiStore(ref order, effGoodsCar, userInfo, storeMaterial, extraSql, ref createOrderErrorMsg, couponLogId))
                    {
                        return Json(new { isok = false, msg = createOrderErrorMsg }, JsonRequestBehavior.AllowGet);
                    }

                    #region (不参与当前生成逻辑)查找可领取的立减金,立减金无库存时立减金不可领取  --作用于前端判定订单是否要跳转到立减金领取页面
                    //【获取立减金，没有的话就传null】
                    Coupons reductionCart = CouponsBLL.SingleModel.GetOpenedModel(umodel.Id);
                    if (reductionCart != null)
                    {
                        
                        int count = CouponLogBLL.SingleModel.GetCountBySql($"SELECT count(fromorderid) from(select fromorderid from couponlog where CouponId = {reductionCart.Id} group by fromorderid) a");//已领取的份数
                        reductionCart.RemNum = reductionCart.CreateNum-count;

                        ////无库存时立减金不可领取
                        //List<CouponLog> loglist = _couponlogBLL.GetList($"couponid in ({reductionCart.Id}) and state!=4");
                        //List<CouponLog> temploglist = loglist?.Where(w => w.CouponId == reductionCart.Id).ToList();
                        //if (temploglist != null && temploglist.Count > 0)
                        //{
                        //    //已领取份数
                        //    int orderCount = temploglist.GroupBy(g => g.FromOrderId).Count();
                        //    //库存是否足够
                        //    reductionCart.RemNum = reductionCart.CreateNum - orderCount;
                        //    if (reductionCart.RemNum <= 0)
                        //    {
                        //        reductionCart = null;
                        //    }
                        //}
                    }
                    #endregion

                    #region 是否开团
                    string groupmsg = OpenGroup(isgroup, umodel.Id, buyMode, userInfo.Id, groupmodel, order.BuyPrice, ref groupid);
                    if (!string.IsNullOrEmpty(groupmsg))
                    {
                        return Json(new { isok = -1, msg = groupmsg }, JsonRequestBehavior.AllowGet);
                    }
                    if (isgroup > 0 && groupid > 0)
                    {
                        order.OrderType = 3;
                        order.GroupId = groupid;
                        EntGoodsOrderBLL.SingleModel.Update(order, "groupid,OrderType");
                    }
                    #endregion

                    //不同支付方式的结算方式
                    switch (buyMode)
                    {
                        case (int)miniAppBuyMode.微信支付:
                            //订单0元处理 => 无需经过微信支付,直接做已经支付后的处理
                            if (order.BuyPrice <= 0)
                            {
                                if (!EntGoodsOrderBLL.SingleModel.HandleAfterPayOrder(order, $"微信支付0元<entGoodsOrder.Id:{order.Id}>"))
                                {
                                    return Json(new { isok = false, msg = $"您钦点的订单在支付过程中支付失败了,请重新下单！" }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { isok = true, msg = "您钦点的订单支付成功", orderNum = order.OrderNum, orderId = order.OrderId, dbOrderId = order.Id, reductionCart = reductionCart }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                #region CtiyModer 生成并将ID更入当前订单
                                string no = WxPayApi.GenerateOutTradeNo();

                                CityMorders citymorderModel = new CityMorders
                                {
                                    OrderType = (int)ArticleTypeEnum.MiniappMultiStore,
                                    ActionType = (int)ArticleTypeEnum.MiniappMultiStore,
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
                                    MinisnsId = umodel.Id,// 订单aId
                                    TuserId = order.Id,//订单的ID
                                    ShowNote = $" {umodel.Title}购买商品支付{order.BuyPrice * 0.01}元",
                                    CitySubId = 0,//无分销,默认为0
                                    PayRate = 1,
                                    buy_num = 0, //无
                                    appid = umodel.AppId,
                                };
                                order.OrderId = Convert.ToInt32(new CityMordersBLL().Add(citymorderModel));
                                EntGoodsOrderBLL.SingleModel.Update(order, "OrderId");
                                #endregion
                            }

                            break;
                        case (int)miniAppBuyMode.储值支付:

                            #region  //TODO:储值支付 扣除预存款金额并生成消费记录
                            //TODO:储值支付 扣除预存款金额并生成消费记录
                            #endregion

                            break;
                        default:

                            break;
                    }
                    return Json(new { isok = true, msg = "您钦点的订单出单成功", orderNum = order.OrderNum, orderId = order.OrderId, dbOrderId = order.Id, reductionCart = reductionCart }, JsonRequestBehavior.AllowGet);
                    
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message + "," + dugmsg }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderList()
        {
            #region 数据验证
            if (string.IsNullOrEmpty(_appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            int storeId = Context.GetRequestInt("storeId", 0);
            if (storeId < 0)
            {
                return Json(new { isok = false, msg = "storeId不能为空" }, JsonRequestBehavior.AllowGet);
            }
            FootBath storeMaterial = FootBathBLL.SingleModel.GetModel(storeId);
            if (storeMaterial == null)
            {
                return Json(new { isok = false, msg = "店铺资料错误" }, JsonRequestBehavior.AllowGet);
            }

            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appId, _openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            int state = Context.GetRequestInt("state", -999);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string sqlwhere = $" aId={storeMaterial.appId} and userid={userInfo.Id}";
            switch (state)
            {
                case (int)MiniAppEntOrderState.待发货:
                    sqlwhere += $" and state in({(int)MiniAppEntOrderState.待发货},{(int)MiniAppEntOrderState.待接单},{(int)MiniAppEntOrderState.待配送})";
                    break;
                case (int)MiniAppEntOrderState.待收货:
                    sqlwhere += $" and state in({(int)MiniAppEntOrderState.待收货},{(int)MiniAppEntOrderState.待确认送达})";
                    break;
                case (int)MiniAppEntOrderState.待自取:
                    sqlwhere += $" and state={state}";
                    break;
                case 0://已完成或退款的状态订单，跟待付款状态无关
                    sqlwhere += $" and state in({(int)MiniAppEntOrderState.交易成功},{(int)MiniAppEntOrderState.退款中},{(int)MiniAppEntOrderState.退款失败},{(int)MiniAppEntOrderState.退款审核中},{(int)MiniAppEntOrderState.退款成功})";
                    break;
            }
            List<EntGoodsOrder> orderList = EntGoodsOrderBLL.SingleModel.GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
            if (orderList != null && orderList.Count > 0)
            {
                string orderIds = string.Join(",",orderList.Select(s=>s.Id));
                List<EntGoodsCart> entGoodsCartList = EntGoodsCartBLL.SingleModel.GetListByOrderIds(orderIds);

                string goodsIds = string.Join(",",entGoodsCartList?.Select(s=>s.FoodGoodsId).Distinct());
                List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);
                orderList.ForEach(o =>
                {
                    o.goodsCarts = entGoodsCartList?.Where(w=> w.GoodsOrderId == o.Id).ToList();
                    if (o.goodsCarts != null && o.goodsCarts.Count > 0)
                    {
                        o.goodsCarts.ForEach(cart => cart.goodsMsg = entGoodsList?.FirstOrDefault(f=>f.id == cart.FoodGoodsId));
                    }
                });
            }
            return Json(new { isok = true, orderList = orderList });
        }
        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderDetial()
        {
            #region 数据验证
            if (string.IsNullOrEmpty(_appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            int storeId = Context.GetRequestInt("storeId", 0);
            if (storeId < 0)
            {
                return Json(new { isok = false, msg = "storeId不能为空" }, JsonRequestBehavior.AllowGet);
            }
            FootBath storeMaterial = FootBathBLL.SingleModel.GetModel(storeId);
            if (storeMaterial == null)
            {
                return Json(new { isok = false, msg = "店铺资料错误" }, JsonRequestBehavior.AllowGet);
            }

            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appId, _openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            int orderId = Context.GetRequestInt("orderId", 0);
            if (orderId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModel($"aid={storeMaterial.appId} and id={orderId} and userid={userInfo.Id}");
            if (orderInfo == null)
            {
                return Json(new { isok = false, msg = "订单信息错误" }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            
            int groupstate = 0;
            string grouptime = "";//拼团结束时间
            //拼团
            if (orderInfo.GroupId > 0)
            {
                EntGroupSponsor sponsor = EntGroupSponsorBLL.SingleModel.GetModel(orderInfo.GroupId);
                if (sponsor == null)
                {
                    return Json(new { isok = -1, msg = "团信息不存在" }, JsonRequestBehavior.AllowGet);
                }
                groupstate = sponsor.State;
                grouptime = sponsor.EndDate.ToString("yyyy-MM-dd HH:mm:ss");
            }

            FootBath storeInfo = FootBathBLL.SingleModel.GetModel(orderInfo.StoreId);
            if (storeInfo != null)
            {
                storeInfo.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeInfo.SwitchConfig);
                storeInfo.shopDays = FootBathBLL.SingleModel.GetShopDays(storeInfo.switchModel);
            }
            orderInfo.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderInfo.Id}");
            if (orderInfo.goodsCarts != null && orderInfo.goodsCarts.Count > 0)
            {
                string goodsIds = string.Join(",", orderInfo.goodsCarts?.Select(s => s.FoodGoodsId).Distinct());
                List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);
                orderInfo.goodsCarts.ForEach(cart =>
                {
                    cart.goodsMsg = entGoodsList?.FirstOrDefault(f=>f.id == cart.FoodGoodsId);
                });
            }
            return Json(new { isok = true, orderInfo = orderInfo, storeInfo = storeInfo,groupstate= groupstate, grouptime= grouptime });
        }

        /// <summary>
        /// 申请退款
        /// </summary>
        /// <returns></returns>
        public ActionResult OutOrder()
        {
            #region 数据验证
            if (string.IsNullOrEmpty(_appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            int storeId = Context.GetRequestInt("storeId", 0);
            if (storeId < 0)
            {
                return Json(new { isok = false, msg = "storeId不能为空" }, JsonRequestBehavior.AllowGet);
            }
            FootBath storeMaterial = FootBathBLL.SingleModel.GetModel(storeId);
            if (storeMaterial == null)
            {
                return Json(new { isok = false, msg = "店铺资料错误" }, JsonRequestBehavior.AllowGet);
            }

            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appId, _openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            int orderId = Context.GetRequestInt("orderId", 0);
            if (orderId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModel($"aid={storeMaterial.appId} and id={orderId} and userid={userInfo.Id}");
            if (orderInfo == null)
            {
                return Json(new { isok = false, msg = "订单信息错误" }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            orderInfo.State = (int)MiniAppEntOrderState.退款审核中;
            bool isok = EntGoodsOrderBLL.SingleModel.Update(orderInfo, "state");
            if (isok)
            {
                TemplateMsg_Gzh.OutOrderTemplateMessage(orderInfo);
            }
            string msg = isok ? "已向商家提出退款申请，请耐心等待" : "操作失败";
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 修改订单状态
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeOrder()
        {
            #region 数据验证
            if (string.IsNullOrEmpty(_appId))
            {
                return Json(new { isok = false, msg = "appid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(_appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            int storeId = Context.GetRequestInt("storeId", 0);
            if (storeId < 0)
            {
                return Json(new { isok = false, msg = "storeId不能为空" }, JsonRequestBehavior.AllowGet);
            }
            FootBath storeMaterial = FootBathBLL.SingleModel.GetModel(storeId);
            if (storeMaterial == null)
            {
                return Json(new { isok = false, msg = "店铺资料错误" }, JsonRequestBehavior.AllowGet);
            }

            var userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(_appId, _openId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            int orderId = Context.GetRequestInt("orderId", 0);
            if (orderId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModel($"aid={storeMaterial.appId} and id={orderId} and userid={userInfo.Id}");
            if (orderInfo == null)
            {
                return Json(new { isok = false, msg = "订单信息错误" }, JsonRequestBehavior.AllowGet);
            }
            int state = Context.GetRequestInt("state", 0);
            #endregion
            switch (state)
            {
                case (int)MiniAppEntOrderState.已取消:
                    orderInfo.State = (int)MiniAppEntOrderState.已取消;
                    break;
                case (int)MiniAppEntOrderState.交易成功:
                    orderInfo.State = (int)MiniAppEntOrderState.交易成功;
                    orderInfo.AcceptDate = DateTime.Now;
                    break;
                default:
                    return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            bool isok = EntGoodsOrderBLL.SingleModel.Update(orderInfo, "state,AcceptDate");
            if (isok)
            {
                switch (state)
                {
                    case (int)MiniAppEntOrderState.已取消:
                        TemplateMsg_Gzh.OutOrderTemplateMessage(orderInfo);
                        break;
                    case (int)MiniAppEntOrderState.交易成功:
                        VipRelationBLL.SingleModel.updatelevel(orderInfo.UserId, "multistore");
                        break;
                    default:
                        return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
                }
            }
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}