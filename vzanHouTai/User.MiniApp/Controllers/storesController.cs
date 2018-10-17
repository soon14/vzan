using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Stores;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Stores;
using Entity.MiniApp.User;
using Entity.MiniApp.ViewModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using User.MiniApp.Filters;
using User.MiniApp.Model;
using Utility;
using Utility.IO;
using store = Entity.MiniApp.Stores;

namespace User.MiniApp.Controllers
{
    public class MiappStoresController : storesController
    {

    }

    public class storesController : baseController
    {

        /// <summary>
        /// 实例化对象
        /// </summary>
        public storesController()
        {

  
        }
        public ActionResult DeleteImg(int? id)
        {
            try
            {
                if (id != null)
                {
                    C_AttachmentBLL.SingleModel.DeleteImg(Convert.ToInt32(id));
                }
                return Json(new { Success = true, Msg = "删除成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
            }
        }
        #region 店铺配置
        //店铺配置
        [LoginFilter]
        public ActionResult Index(int appId)
        {
            if (dzaccount ==null)
            {
                return View("PageError", new Return_Msg() { Msg = "登录过期!", code = "500" });
            }

            ViewBag.appId = appId;
            
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            BusinessTimes businessTimes = new BusinessTimes();
            if (store != null)
            {
                #region  若为后续编辑,读取关联数据
                //轮播图
                List<C_Attachment> slideShows = C_AttachmentBLL.SingleModel.GetListByCache( store.Id, (int)AttachmentItemType.小程序商城店铺轮播图);
                List<object> slideShowCheckedModels = new List<object>();
                foreach (C_Attachment attachment in slideShows)
                {
                    slideShowCheckedModels.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.CarouselList = slideShowCheckedModels;

                if (!store.businesstimes.IsNullOrWhiteSpace())
                {
                    businessTimes = JsonConvert.DeserializeObject<BusinessTimes>(store.businesstimes);
                    if (businessTimes.StartTime.IsNullOrWhiteSpace())
                    {
                        businessTimes.StartTime = "08:00";
                    }
                    if (businessTimes.EndTime.IsNullOrWhiteSpace())
                    {
                        businessTimes.EndTime = "18:00";
                    }
                }
                else
                {
                    businessTimes.StartTime = "08:00";
                    businessTimes.EndTime = "18:00";

                }
                #endregion
            }
            else
            {
                //初次编辑则新增
                store = new Store();
                store.appId = appId;
                store.Id = Convert.ToInt32(StoreBLL.SingleModel.Add(store));
            }


            //若无默认运费模板,则添加一个
            int freightCount = StoreFreightTemplateBLL.SingleModel.GetCount($" StoreId={store.Id} and state >= 0");
            if (freightCount <= 0)//若商家无运费模板添加一个默认免费的,以便解决现阶段无运费模板无法下单的问题
            {
                StoreFreightTemplateBLL.SingleModel.Add(new store.StoreFreightTemplate() { BaseCount = 999, StoreId = store.Id, Name = "免运费", BaseCost = 0, ExtraCost = 0, IsDefault = 1, CreateTime = DateTime.Now });
            }


            //初始化
            if (store.AreaCode <= 0)
            {
                store.AreaCode = 110101;
            }
            if (store.CityCode <= 0)
            {
                store.CityCode = 110100;
            }
            if (store.Province <= 0)
            {
                store.Province = 110000;
            }
            //区域
            ViewBag.ProvinceList = C_AreaBLL.SingleModel.GetStreetItemsInChina(store.Province);
            ViewBag.CityList = C_AreaBLL.SingleModel.GetStreetItems(store.Province, store.CityCode);
            ViewBag.AreaList = C_AreaBLL.SingleModel.GetStreetItems(store.CityCode, store.AreaCode);

            //小程序配置
            ViewBag.shouquan = 0;
            ViewBag.XcxName = "";
            List<ConfParam> paramslist = ConfParamBLL.SingleModel.GetListByRId(appId);
            List<OpenAuthorizerConfig> umodel = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), appId);
            if (umodel != null && umodel.Count > 0)
            {
                ViewBag.shouquan = 1;
                if (paramslist != null && paramslist.Count > 0)
                {
                    ConfParam cinfo = paramslist.Where(w => w.Param == "nparam").FirstOrDefault();
                    if (cinfo != null)
                    {
                        ViewBag.XcxName = cinfo.Value;
                    }
                }
            }
            ViewBag.BusinessTimes = businessTimes;
            return View(store);
        }

        [HttpGet]
        public ActionResult getDefaultFrieghtTempLate()
        {
            List<Store> storeList = StoreBLL.SingleModel.GetList();
            storeList.ForEach(x =>
            {
                if (StoreFreightTemplateBLL.SingleModel.GetCount($" StoreId={x.Id} and state >= 0") <= 0)
                {
                    StoreFreightTemplateBLL.SingleModel.Add(new store.StoreFreightTemplate() { BaseCount = 999, StoreId = x.Id, Name = "免运费", BaseCost = 0, ExtraCost = 0, IsDefault = 1, CreateTime = DateTime.Now });
                }
            });
            return Content("执行完了");
        }


        //添加编辑店铺信息
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult IndexAddOrEdit(Store store, string ImgList, string datajson = "", int rid = 0)
        {
            //var abc = "";
            ServiceResult result = new ServiceResult();
            try
            {
                if (dzaccount == null)
                {
                    return Json(result.IsFailed("系统繁忙auth_null !"));
                }
                if (rid <= 0)
                {
                    return Json(result.IsFailed("系统繁忙rid !"));
                }
                XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(rid);
                if (umodel == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
                }

                if (store.Id != 0)
                {
                    string cloumns = "Province,CityCode,AreaCode,Address,TelePhone,UpdateDate,name,logo,businesstimes,notice";

                    store.UpdateDate = DateTime.Now;

                    bool isSuccess = StoreBLL.SingleModel.Update(store, cloumns);

                    if (!isSuccess)
                    {
                        return Json(result.IsFailed("更新商户失败 !"));
                    }

                    result.IsSucceed("更新成功 !");
                }
                else
                {
                    store.CreateDate = DateTime.Now;
                    store.UpdateDate = DateTime.Now;
                    int id = Convert.ToInt32(StoreBLL.SingleModel.Add(store));
                    store.Id = id;


                    result.IsSucceed("添加成功 !");
                }

                if (!ImgList.IsNullOrWhiteSpace())
                {

                    string[] imgArray = ImgList.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (imgArray.Length > 0)
                    {
                        foreach (string item in imgArray)
                        {
                            //判断上传图片是否以http开头，不然为破图-蔡华兴
                            if (!string.IsNullOrEmpty(item) && item.IndexOf("http://", StringComparison.Ordinal) == 0)
                            {
                                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                                {
                                    itemId = store.Id,
                                    createDate = DateTime.Now,
                                    filepath = item,
                                    itemType = (int)AttachmentItemType.小程序商城店铺轮播图,
                                    #region 店铺详情页的轮播图尺寸改成640*360-蔡华兴
                                    thumbnail = Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(item, 640, 360),
                                    #endregion
                                    status = 0
                                });
                            }
                        }
                    }
                }

                #region 小程序配置

                List<ConfParam> paramslist = ConfParamBLL.SingleModel.GetListByRId(rid);
                if (paramslist != null && paramslist.Count > 0)
                {
                    List<ConfParam> data = JsonConvert.DeserializeObject<List<ConfParam>>(datajson);
                    if (data != null && data.Count > 0)
                    {
                        //abc += "1";
                        foreach (ConfParam item in data)
                        {
                            ConfParam list = paramslist.FirstOrDefault(f => f.Param == item.Param);
                            if (list != null)
                            {
                                //abc += "2";
                                if (ConfParamBLL.SingleModel.UpdateList(rid, item.Value, item.Param, umodel.AppId) <= 0)
                                {
                                    return Json(new { isok = -1, msg = "修改失败_" + item.Param }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                //abc += "3";
                                ConfParam model = new ConfParam();
                                model.AppId = umodel.AppId;
                                model.Param = item.Param;
                                model.Value = item.Value;
                                model.State = 0;
                                model.UpdateTime = DateTime.Now;
                                model.AddTime = DateTime.Now;
                                model.RId = rid;
                                if (Convert.ToInt32(ConfParamBLL.SingleModel.Add(model)) <= 0)
                                {
                                    return Json(new { isok = -1, msg = "添加失败" }, JsonRequestBehavior.AllowGet);
                                }
                                //abc += "4";
                            }
                        }
                        //abc += "5";
                    }
                }
                else
                {
                    //abc += "6";
                    List<ConfParam> data = JsonConvert.DeserializeObject<List<ConfParam>>(datajson);
                    if (data != null && data.Count > 0)
                    {
                        foreach (ConfParam item in data)
                        {
                            //abc += "7";
                            ConfParam model = new ConfParam();
                            model.AppId = umodel.AppId;
                            model.Param = item.Param;
                            model.Value = item.Value;
                            model.State = 0;
                            model.UpdateTime = DateTime.Now;
                            model.AddTime = DateTime.Now;
                            model.RId = rid;
                            if (Convert.ToInt32(ConfParamBLL.SingleModel.Add(model)) <= 0)
                            {
                                return Json(new { isok = -1, msg = "添加失败" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        //abc += "8";
                    }
                }
                #endregion

                result.Data = new Dictionary<string, object> { { "Id", store.Id } };
                return Json(result);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
                return Json(result.IsFailed("服务器出错 , 请重试 !" + ex.Message));
                //return Json(result.IsFailed("服务器出错 , 请重试 !"));
            }
        }

        //获取子区域
        public ActionResult GetRegionJsonList(int parentCode=0, int currentCode = 0)
        {
            string sql = "SELECT * from c_area where parentCode = " + parentCode;
            var regionList = C_AreaBLL.SingleModel.GetListBySql(sql).Select(m => new
            {
                Value = m.Code,
                Text = m.Name,
                Selected = m.Code == currentCode
            });
            return Json(regionList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 商品管理
        #region 商品管理
        //商品列表
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="ispost"></param>
        /// <param name="versionType">0电商版 1专业版</param>
        /// <returns></returns>
        public ActionResult GoodsList(int appId,string goodsName = "",int goodsType = 0,int isSell = -999,  int pageIndex = 1, int pageSize = 10, string ispost = "", int versionType = 0,string goodIdStr ="", bool export = false)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            ViewBag.appId = appId;
            if (versionType > 0)
            {

                List<EntGoods> goodslist = EntGoodsBLL.SingleModel.GetList($" aid={appId} and state=1 and tag = 1 ", pageSize, pageIndex, "id,img,name,addtime", " id desc ");
                int recordCount = EntGoodsBLL.SingleModel.GetCount($" aid={appId} and state=1 and tag = 1 ");
                List<object> listGoods = new List<object>();
               foreach(EntGoods item in goodslist)
                {
                    listGoods.Add(new {
                        Id=item.id,
                        ImgUrl=item.img,
                        GoodsName =item.name,
                        showtime=item.addtime.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
                object obj = new
                {
                    recordCount = ViewBag.TotalCount,
                    list = listGoods
                };
                return Json(obj);

            }

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (store == null)
            {
                return View("PageError", new Return_Msg() { Msg = "请先配置店铺信息!", code = "500" });
            }
            string strwhere = $" StoreId={store.Id} and State>=0 ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            if (!string.IsNullOrWhiteSpace(goodsName))
            {
                strwhere += $" and GoodsName like @goodsName ";
                mysqlParams.Add(new MySqlParameter("@goodsName", "%" + goodsName +"%"));
            }
            if (goodsType != 0)
            {
                strwhere += $" and TypeId = {goodsType}";
            }

            if (isSell != -999)
            {
                strwhere += $" and IsSell = {isSell}";
            }

            //目前导出生成excel 只支持选择ID导出
            if (export && !string.IsNullOrWhiteSpace(goodIdStr))
            {
                List<int> goodIds = new List<int>();
                string[] goodIdArray = goodIdStr.Split(',');
                goodIds = goodIdArray.Select(x => Convert.ToInt32(x)).ToList<int>();

                if (goodIds != null && goodIds.Any())
                {
                    strwhere = $" Id in ({string.Join(",", goodIds)}) ";
                }
            }
            List<StoreGoods> list = StoreGoodsBLL.SingleModel.GetListByParam(strwhere, mysqlParams.ToArray(), pageSize, pageIndex, "*", " Sort Desc,Id Desc ") ?? new List<StoreGoods>();

            string goodsTypeIds = string.Join(",",list?.Select(s=>s.TypeId).Distinct());
            List<StoreGoodsType> storeGoodsTypeList = StoreGoodsTypeBLL.SingleModel.GetListByIds(goodsTypeIds);

            list?.ForEach(x =>
            {
                x.TypeStr = storeGoodsTypeList?.FirstOrDefault(f=>f.Id == x.TypeId)?.Name;
            });


            //导出Excel
            if (export)
            {
                if (list != null && list.Count > 0)
                {
                    DataTable table = new DataTable();
                    table.Columns.AddRange(new[]
                    {
                        new DataColumn("商品名称"),
                        new DataColumn("是否多规格"),
                        new DataColumn("价格"),
                        new DataColumn("销量"),
                        new DataColumn("剩余库存"),
                        new DataColumn("总库存"),
                        new DataColumn("商品类型"),
                        new DataColumn("发布时间"),
                        new DataColumn("更新时间"),
                        new DataColumn("状态")
                    });

                    foreach (StoreGoods item in list)
                    {
                        DataRow row = table.NewRow();
                        row["商品名称"] = item.GoodsName;
                        row["是否多规格"] = string.IsNullOrWhiteSpace(item.AttrDetail) ? "是" : "否";
                        row["价格"] = item.PriceStr;
                        row["销量"] = item.salesCount_real;
                        row["剩余库存"] = item.Stock;
                        row["总库存"] = item.Inventory;
                        row["商品类型"] = item.TypeStr;
                        row["发布时间"] = item.CreateDate.ToString("yyyy-MM-dd HH:mm");
                        row["更新时间"] = item.UpdateDate.ToString("yyyy-MM-dd HH:mm");
                        row["状态"] = item.IsSell == 1 ? "上架": "下架" ;
                        table.Rows.Add(row);
                    }


                    ExcelHelper<UserForm>.Out2Excel(table, role.Title + "商品记录"); //导出
                }
                return Content("导出流水成功");
            }
            List<StoreGoodsType> goodsTypes = StoreGoodsTypeBLL.SingleModel.GetlistByStoreId(store.Id);

            ViewBag.TotalCount = StoreGoodsBLL.SingleModel.GetCount(strwhere,mysqlParams.ToArray());
            ViewBag.isSell = isSell;
            ViewBag.goodsType = goodsType;
            ViewBag.goodsName = goodsName;
            ViewBag.pageSize = pageSize;
            ViewBag.goodsTypes = goodsTypes;
            //ViewBag.State = Utility.GetEnumOpt(typeof(ActiveState)).Where(x => Convert.ToInt32(x[1]) != (int)ActiveState.删除 && Convert.ToInt32(x[1]) != (int)ActiveState.彻底删除); 
            //原页面不再显示删除及彻底删除的数据
            //会员管理获取商品列表数据
            if (ispost == "post")
            {
                list.ForEach(g => g.showtime = g.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
                object obj = new
                {
                    recordCount = ViewBag.TotalCount,
                    list = list
                };
                return Json(obj);
            }
            return View(list);
        }


        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteGoods(int type, int id, int appId = 0)
        {
            //清除商品相关缓存
            StoreGoodsAttrSpecBLL.SingleModel.RemoveStoreGoodsAttrSpecsCache(id);

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);

            StoreGoods goods = StoreGoodsBLL.SingleModel.GetModel(id);
            if (goods == null)
            {
                return Json(new { Success = false, Msg = "找不到该商品" });
            }
            if (goods.StoreId != store.Id)
            {
                return Json(new { Success = false, Msg = "该商品不属于此店铺" });
            }
            if (goods.IsSell == 1)
            {
                return Json(new { Success = false, Msg = "必须下架的商品才可以删除" });
            }
            TransactionModel TranModel = new TransactionModel();

            //更新购物车内商品的状态
            
            List<StoreGoodsAttrDetail> goodsDtlList = goods.GASDetailList;
            List<string> updateGoodsStateSqlList = new List<string>();
            updateGoodsStateSqlList = StoreGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(goods.Id, 2);
            updateGoodsStateSqlList.ForEach(x =>
            {
                TranModel.Add(x);
            });
            goods.State = -1;

            //更新商品状态
            TranModel.Add(StoreGoodsBLL.SingleModel.BuildUpdateSql(goods, "State"));

            if (StoreGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {

                return Json(new { Success = true, Msg = "成功" });
            }
            else
                return Json(new { Success = false, Msg = "失败" });

            //if (_goodsBll.Update(goods, "State"))
            //{

            //    return Json(new { Success = true, Msg = "成功" });
            //}
            //else
            //    return Json(new { Success = false, Msg = "失败" });
        }

        //添加/编辑商品
        public ActionResult GoodsAddOrEdit(int appId, int gid = 0)
        {
            //清除商品相关缓存
            StoreGoodsBLL.SingleModel.RemoveStoreGoodsCache(gid);
            StoreGoodsAttrSpecBLL.SingleModel.RemoveStoreGoodsAttrSpecsCache(gid);

            ViewBag.Vid = 0;
            store.StoreGoods good = null;
            ViewBag.appId = appId;
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            Store store = StoreBLL.SingleModel.GetModelByAId(appId);

            //商品分类
            List<StoreGoodsType> goodtylelist = StoreGoodsTypeBLL.SingleModel.GetlistByStoreId(store.Id);//  GetList("StoreId=" + storeId);
            ViewBag.GoodTypeList = goodtylelist.Any() ? goodtylelist : new List<StoreGoodsType>();
            //运费模板
            List<StoreFreightTemplate> freightList = StoreFreightTemplateBLL.SingleModel.GetList($"StoreId={store.Id} and state >= 0");
            ViewBag.FreightList = freightList;
            ViewBag.FreightText = "";

            if (gid > 0)
            {
                good = StoreGoodsBLL.SingleModel.GetModel(gid);
                if (null == good)
                    return View("PageError", new Return_Msg() { Msg = "没有数据!", code = "500" });
                
                List<C_Attachment> ImgList = C_AttachmentBLL.SingleModel.GetListByCache(good.Id , (int)AttachmentItemType.小程序商城商品轮播图);
                List<object> CarouselList = new List<object>();
                foreach (C_Attachment attachment in ImgList)
                {
                    CarouselList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.CarouselList = CarouselList;


                //详情轮播图
                List<C_Attachment> DescImgList2 = C_AttachmentBLL.SingleModel.GetListByCache( good.Id , (int)AttachmentItemType.小程序商城店铺详情轮播图);
                List<object> DescCarouselList = new List<object>();
                foreach (C_Attachment attachment in DescImgList2)
                {
                    DescCarouselList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.descCount = DescCarouselList.Count;
                ViewBag.DescCarouselList = DescCarouselList;
                
                List<C_Attachment> ImgList2 = C_AttachmentBLL.SingleModel.GetListByCache(good.Id ,(int)AttachmentItemType.小程序商城商品详情图);
                List<object> DescImgList = new List<object>();
                foreach (C_Attachment attachment in ImgList2)
                {
                    DescImgList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.DescImgList = DescImgList;

                List<C_AttachmentVideo> attvideolist = C_AttachmentVideoBLL.SingleModel.GetListByCache(good.Id, (int)AttachmentVideoType.小程序商城商品视频);
                if (attvideolist.Count > 0)
                {
                    ViewBag.Vid = attvideolist[0].id;
                    ViewBag.convertFilePath = attvideolist[0].convertFilePath;
                    ViewBag.videoPosterPath = attvideolist[0].videoPosterPath;
                }

                return View(good);
            }

            return View(new store.StoreGoods() { StoreId = store.Id, GoodsName = "" });
        }

        /// <summary>
        /// add goods添加商品
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> addGoodsAsync(store.StoreGoods goods, string ImgList = "", string DescImgList = "", string goods_spec_ids = "", int oldhvid = 0, string videopath = "", string descdoimglist = "")
        {
            if (dzaccount == null)
            {
                return Json(new { code = -1, msg = "系统繁忙auth_null" });
            }

            #region Base64解密
            if (goods != null)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(goods.Description))
                    {
                        //描述
                        string strDescription = goods.Description.Replace(" ", "+");
                        byte[] bytes = Convert.FromBase64String(strDescription);
                        goods.Description = System.Text.Encoding.UTF8.GetString(bytes);
                    }

                    ////多规格商品信息
                    //string strAttrDetail = goods.AttrDetail.Replace(" ", "+");
                    //byte[] bytes2 = Convert.FromBase64String(strAttrDetail);
                    //goods.AttrDetail = System.Text.Encoding.UTF8.GetString(bytes2);
                }
                catch
                {
                }
            }

            #endregion

            bool result;
            int addresult = 0;
            Store store = StoreBLL.SingleModel.GetModel(goods.StoreId);
            if (store == null)
            {
                return Json(new { code = -1, msg = "店铺信息有误！" }, JsonRequestBehavior.AllowGet);
            }
            //字符串转json串
            if (!string.IsNullOrEmpty(goods.AttrDetail))
            {
                string[] attrstr = goods.AttrDetail.Split(';');
                if (attrstr.Length > 0)
                {
                    List<StoreGoodsAttrDetail> attrdetaillist = new List<StoreGoodsAttrDetail>();
                    foreach (string attr in attrstr)
                    {
                        string[] detail = attr.Split(',');
                        if (detail.Length > 1)
                        {
                            StoreGoodsAttrDetail storeGoodsAttrDtl = new StoreGoodsAttrDetail();
                            storeGoodsAttrDtl.id = detail[0].ToString();
                            storeGoodsAttrDtl.count = Convert.ToInt32(detail[1]);
                            storeGoodsAttrDtl.price = Convert.ToInt32(Convert.ToDouble(detail[2]) * 100);
                            attrdetaillist.Add(storeGoodsAttrDtl);
                            //价格更新之后更新购物车价格
                            if (goods.Id > 0)
                            {
                                StoreGoodsCartBLL.SingleModel.UpdateCartByGoodsId(goods.Id, storeGoodsAttrDtl.id, storeGoodsAttrDtl.price);
                            }
                        }
                    }
                    JavaScriptSerializer jsc = new JavaScriptSerializer();
                    StringBuilder jsonData = new StringBuilder();
                    jsc.Serialize(attrdetaillist, jsonData);
                    goods.AttrDetail = jsonData.ToString();
                }
            }
            if (goods.Id > 0)
            {
                StoreGoodsCartBLL.SingleModel.UpdateCartByGoodsId(goods.Id, "", goods.Price);
            }

            if (goods.Id > 0)
            {
                List<C_Attachment> imglist = C_AttachmentBLL.SingleModel.GetListByCache(+ goods.Id ,(int)AttachmentItemType.小程序商城商品轮播图);
                if (imglist != null && imglist.Count > 0)
                {
                    goods.ImgUrl = imglist[0].filepath;
                }
                StoreGoods oldnews = StoreGoodsBLL.SingleModel.GetModel(goods.Id);
                if (oldnews == null)
                {
                    return Json(new { code = -1, msg = "编辑出错！" }, JsonRequestBehavior.AllowGet);
                }
                goods.UpdateDate = DateTime.Now;
                
                string columnField = "Inventory,Stock,Price,OriginalPrice,TypeId,Description,GoodsName,UpdateDate,AttrDetail,ImgUrl,FreightIds,Introduction,salesCount,Sort";


                #region 若多规格商品被删除,更改购物车商品的标识
                StoreGoods dbGood = StoreGoodsBLL.SingleModel.GetModel(goods.Id);
                if (dbGood == null)
                {
                    return Json(new { code = -1, msg = "商品不存在！" }, JsonRequestBehavior.AllowGet);
                }

                TransactionModel TranModel = new TransactionModel();
                
                //更改已被删掉的商品
                List<string> dbGoodSpacList = dbGood.GASDetailList.Select(x => x.id).ToList();
                List<string> goodsSpacList = goods.GASDetailList.Select(x => x.id).ToList();
                List<string> updateGoodsStateSqlList = new List<string>();
                if (dbGoodSpacList.Count > 0)
                {
                    dbGoodSpacList.ForEach(x =>
                    {
                        if (!goodsSpacList.Contains(x))
                        {
                            updateGoodsStateSqlList.AddRange(StoreGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsIdSpecids(goods.Id, x, 2));
                        }
                    });
                }

                updateGoodsStateSqlList.ForEach(x =>
                {
                    TranModel.Add(x);
                    //log4net.LogHelper.WriteInfo(GetType(),x);
                });

                if (!StoreGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
                {
                    return Json(new { code = -1, msg = "更改购物车标识失败！" }, JsonRequestBehavior.AllowGet);
                }
                #endregion

                result = StoreGoodsBLL.SingleModel.Update(goods, columnField);
                addresult = 1;
            }
            else
            {
                goods.CreateDate = DateTime.Now;
                goods.UpdateDate = DateTime.Now;
                goods.State = (int)MiniappState.通过;
                goods.IsSell = 1;

                int id = goods.Id = Convert.ToInt32(StoreGoodsBLL.SingleModel.Add(goods));
                goods.Id = id;
                addresult = id;
            }
            if (addresult > 0)
            {
                #region 轮播图
                if (!string.IsNullOrWhiteSpace(ImgList))
                {
                    string[] imgs = ImgList.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string img in imgs)
                    {
                        //判断上传图片是否以http开头，不然为破图-蔡华兴
                        if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http", StringComparison.Ordinal) == 0)
                        {
                            C_AttachmentBLL.SingleModel.Add(new C_Attachment
                            {
                                itemId = goods.Id,
                                createDate = DateTime.Now,
                                filepath = img,
                                itemType = (int)AttachmentItemType.小程序商城商品轮播图,
                                thumbnail = img,
                                status = 0
                            });
                        }
                    }
                }
                #endregion

                #region 详情图

                if (!string.IsNullOrEmpty(DescImgList))
                {
                    string[] imgArray = DescImgList.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (imgArray.Length > 0)
                    {
                        foreach (string img in imgArray)
                        {
                            //判断上传图片是否以http开头，不然为破图-蔡华兴
                            if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http://", StringComparison.Ordinal) == 0)
                            {
                                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                                {
                                    itemId = goods.Id,
                                    createDate = DateTime.Now,
                                    filepath = img,
                                    itemType = (int)AttachmentItemType.小程序商城商品详情图,
                                    thumbnail = img,
                                    status = 0
                                });
                            }

                        }
                    }
                }
                #endregion

                #region 详情图

                if (!string.IsNullOrEmpty(descdoimglist))
                {
                    string[] imgArray = descdoimglist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (imgArray.Length > 0)
                    {
                        foreach (string img in imgArray)
                        {
                            //判断上传图片是否以http开头，不然为破图-蔡华兴
                            if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http://", StringComparison.Ordinal) == 0)
                            {
                                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                                {
                                    itemId = goods.Id,
                                    createDate = DateTime.Now,
                                    filepath = img,
                                    itemType = (int)AttachmentItemType.小程序商城店铺详情轮播图,
                                    thumbnail = img,
                                    status = 0
                                });
                            }

                        }
                    }
                }
                #endregion

                #region 视频
                await C_AttachmentVideoBLL.SingleModel.HandleVideoLogicStrategyAsync(videopath, oldhvid, goods.Id, (int)AttachmentVideoType.小程序商城商品视频);
                #endregion

                //删除旧关系
                int ids = StoreGoodsBLL.SingleModel.DelAttrList(goods.Id);
                #region 更新商品规格关系表
                if (!string.IsNullOrEmpty(goods_spec_ids))
                {
                    string sids = goods_spec_ids.Substring(0, goods_spec_ids.Length - 1);
                    List<StoreGoodsSpec> speclist = StoreGoodsSpecBLL.SingleModel.GetList($"Id in ({sids})");
                    foreach (StoreGoodsSpec item in speclist)
                    {
                        StoreGoodsAttrSpecBLL.SingleModel.Add(new StoreGoodsAttrSpec
                        {
                            GoodsId = goods.Id,
                            AttrId = item.AttrId,
                            SpecId = item.Id
                        });
                    }
                }
                #endregion

                if (!string.IsNullOrWhiteSpace(goods.Description))
                {
                    System.Collections.ArrayList Imglist = FilterHandler.GetImgUrlfromhtml(goods.Description);
                    //自动抓取图片和图文混排图片处理开启一个线程去处理
                    if (Imglist.Count > 0)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(delegate
                        {
                            string content = goods.Description;
                            if (Imglist.Count > 0)
                                CityCommonUtils.downloadImgs(0, ref content, Imglist);
                            StoreGoods updateModel = StoreGoodsBLL.SingleModel.GetModel(addresult);
                            updateModel.Description = content;
                            StoreGoodsBLL.SingleModel.Update(updateModel, "Description");
                        });

                    }
                }
                return Json(new { code = 1, msg = "操作成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { code = -1, msg = "系统错误！" }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 上下架商品
        /// </summary>
        /// <returns></returns>
        public ActionResult IsSellStoreGoods(int type, int id, int appid = 0, int storeid = 0)
        {
            //清除商品相关缓存
            StoreGoodsBLL.SingleModel.RemoveStoreGoodsCache(id);
            StoreGoodsAttrSpecBLL.SingleModel.RemoveStoreGoodsAttrSpecsCache(id);

            if (appid == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            Store store = StoreBLL.SingleModel.GetModelByAId(appid);
            if (store == null)
            {
                return View("PageError", new Return_Msg() { Msg = "店铺不存在!", code = "500" });
            }
            StoreGoods good = StoreGoodsBLL.SingleModel.GetModel(id);
            if (good == null)
            {
                return View("PageError", new Return_Msg() { Msg = "商品不存在!", code = "500" });
            }
            if (good.State != (int)MiniappState.通过)
            {
                return View("PageError", new Return_Msg() { Msg = "状态繁忙!", code = "500" });
            }

            TransactionModel TranModel = new TransactionModel();
            //更新购物车内商品的状态
            
            List<StoreGoodsAttrDetail> goodsDtlList = good.GASDetailList;
            List<string> updateGoodsStateSqlList = new List<string>();
            updateGoodsStateSqlList = StoreGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(good.Id, type == 1 ? 0 : 1, oldGoodState: type == 1 ? 1 : 0);
            updateGoodsStateSqlList.ForEach(x =>
            {
                TranModel.Add(x);
            });

            //更新商品上下架状态
            good.IsSell = type;
            TranModel.Add(StoreGoodsBLL.SingleModel.BuildUpdateSql(good, "IsSell"));
            if (StoreGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {
                return Json(new { Success = true, Msg = "成功" });
            }
            else
                return Json(new { Success = false, Msg = "失败" });
        }


        /// <summary>
        /// 批量上下架菜品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BatchIsSellFoodGoods()
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            int type = Context.GetRequestInt("type", 0);
            if (type < 0 || type > 1)
            {
                return Json(new { Success = false, Msg = "状态错误" });
            }

            string storeGoodsIdString = Context.GetRequest("storeGoodsIds", "");
            List<int> goodIds = new List<int>();

            try
            {
                if (string.IsNullOrWhiteSpace(storeGoodsIdString))
                {
                    return Json(new { Success = false, Msg = "商品信息错误" });
                }
                string[] foodStrIdArray = storeGoodsIdString.Split(',');
                goodIds = foodStrIdArray.Select(x => Convert.ToInt32(x)).ToList<int>();
            }
            catch (Exception)
            {
                return Json(new { Success = false, Msg = "商品信息错误" });
            }

            TransactionModel TranModel = new TransactionModel();
            //更新购物车内商品的状态
            

            foreach (int id in goodIds)
            {
                //清除商品相关缓存
                StoreGoodsBLL.SingleModel.RemoveStoreGoodsCache(id);

                StoreGoods good = StoreGoodsBLL.SingleModel.GetModel(id);
                if (good == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "商品不存在!", code = "500" });
                }
                if (good.State != (int)MiniappState.通过)
                {
                    return View("PageError", new Return_Msg() { Msg = "状态繁忙!", code = "500" });
                }

                List<StoreGoodsAttrDetail> goodsDtlList = good.GASDetailList;
                List<string> updateGoodsStateSqlList = new List<string>();
                updateGoodsStateSqlList = StoreGoodsCartBLL.SingleModel.UpdateCartGoodsStateByGoodsId(good.Id, type == 1 ? 0 : 1, oldGoodState: type == 1 ? 1 : 0);
                updateGoodsStateSqlList.ForEach(x =>
                {
                    TranModel.Add(x);
                });

                //更新商品上下架状态
                good.IsSell = type;
                TranModel.Add(StoreGoodsBLL.SingleModel.BuildUpdateSql(good, "IsSell"));
            }


            if (StoreGoodsCartBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {
                return Json(new { Success = true, Msg = "成功" });
            }
            else
                return Json(new { Success = false, Msg = "失败" });
        }


        /// <summary>
        /// 商品排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public ActionResult UpdateStoreCouponSort(int appId, int id, int sort)
        {
            //清除商品相关缓存
            StoreGoodsBLL.SingleModel.RemoveStoreGoodsCache(id);

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }

            if (id <= 0)
            {
                return Json(new { code = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
            }

            StoreGoods good = StoreGoodsBLL.SingleModel.GetModel(id);
            if (good != null)
            {
                good.Sort = sort;
                bool result = StoreGoodsBLL.SingleModel.Update(good, "Sort");
                if (result)
                {
                    return Json(new { code = 1, msg = "排序成功 !" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { code = -1, msg = "排序失败 !" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { code = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 商品规格编辑
        /// <summary>
        /// 查找规格列表
        /// </summary>
        public ActionResult GetAttrByStoreId(int storeid)
        {
            Store store = StoreBLL.SingleModel.GetModel(storeid);
            if (store == null)
            {
                return Json(new { isok = false, msg = "店铺信息有误" }, JsonRequestBehavior.AllowGet);
            }
            List<StoreGoodsAttr> list = StoreGoodsAttrBLL.SingleModel.GetList($"StoreId={store.Id} and State>=0");
            return Json(new { isok = true, datas = list }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 添加规格
        /// </summary>
        public ActionResult AddAttr(int storeid, string AttrName)
        {
            Store store = StoreBLL.SingleModel.GetModel(storeid);
            if (store == null)
            {
                return Json(new { isok = false, msg = "店铺信息有误" }, JsonRequestBehavior.AllowGet);
            }
            List<StoreGoodsAttr> goodsAttrList = StoreGoodsAttrBLL.SingleModel.GetList($"StoreId={storeid} and State>=0");
            
            if (goodsAttrList.Count >= 100)
            {
                return Json(new { isok = false, msg = "目前最多能添加100个规格哦" });
            }
            if (goodsAttrList.Any(m => m.AttrName.Equals(AttrName)))
            {
                return Json(new { isok = false, msg = "规格名称已存在 , 请重新添加" });
            }
            object result = StoreGoodsAttrBLL.SingleModel.Add(new StoreGoodsAttr() { StoreId = storeid, AttrName = AttrName, State = 0 });
            if (int.Parse(result.ToString()) > 0)
            {
                return Json(new { isok = true, msg = result.ToString() }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 查找属性列表
        /// </summary>
        public ActionResult GetSpecByAttrId(int gs_id)
        {
            StoreGoodsAttr attr = StoreGoodsAttrBLL.SingleModel.GetModel(gs_id);
            if (attr == null)
            {
                return Json(new { isok = false, msg = "规格信息有误" }, JsonRequestBehavior.AllowGet);
            }
            List<StoreGoodsSpec> list = StoreGoodsSpecBLL.SingleModel.GetList($"AttrId={attr.Id} and State>=0");
            return Json(new { isok = true, datas = list }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 添加属性
        /// </summary>
        public ActionResult AddSpec(int gs_id, string SpecName)
        {
            StoreGoodsAttr attr = StoreGoodsAttrBLL.SingleModel.GetModel(gs_id);
            if (attr == null)
            {
                return Json(new { isok = false, msg = "规格信息有误" }, JsonRequestBehavior.AllowGet);
            }

            List<StoreGoodsSpec> goodsSpecList = StoreGoodsSpecBLL.SingleModel.GetList($"AttrId={gs_id} and State>=0");
            
            if (goodsSpecList.Any(m => m.SpecName == SpecName))
            {
                return Json(new { isok = false, msg = "属性名称已存在 , 请重新添加" });
            }
            object result = StoreGoodsSpecBLL.SingleModel.Add(new StoreGoodsSpec() { AttrId = gs_id, SpecName = SpecName, State = 0 });
            if (int.Parse(result.ToString()) > 0)
            {
                return Json(new { isok = true, msg = result.ToString() }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加或编辑商品规格
        /// </summary>
        public ActionResult AttrSpec_detail(int goods_id = 0, int gs_id = 0, string AttrName = "", int storeid = 0, int spec_id = 0, string SpecName = "", int remove_gs_id = 0, int remove_spec_id = 0)
        {
            StringBuilder Str = new StringBuilder();
            //查询商品规格属性  页面加载结果
            string specids = "";
            StoreGoodsAttr attrM = new StoreGoodsAttr();
            List<StoreGoodsAttrDetail> AttrDetailList = new List<StoreGoodsAttrDetail>();
            if (goods_id > 0)
            {
                StoreGoods GoodsM = StoreGoodsBLL.SingleModel.GetModel(goods_id);
                if (GoodsM != null)
                {
                    AttrDetailList = GoodsM.GASDetailList;
                }
                List<StoreGoodsAttrSpec> gslist = StoreGoodsAttrSpecBLL.SingleModel.GetList($"GoodsId={goods_id}");
                specids = string.Join(",", gslist.Select(m => m.SpecId).Distinct());
            }
            else if (spec_id > 0 || SpecName != "") //根据属性id或者名称以及规格id生成表格  添加规格属性结果
            {
                string ids = "";
                if (Session["miniappgoodsspecids"] != null)
                {
                    ids = Session["miniappgoodsspecids"].ToString();
                }
                if (spec_id > 0)
                {
                    if (ids == "")
                    {
                        specids = spec_id.ToString();
                    }
                    else
                    {
                        specids = ids + "," + spec_id;
                    }
                }
                else
                {
                    StoreGoodsSpec spec = StoreGoodsSpecBLL.SingleModel.GetModel($"AttrId={gs_id} and SpecName='{SpecName}' and State>=0");
                    if (spec == null)
                    {
                        return Content("");
                    }
                    if (ids == "")
                    {
                        specids = spec.Id.ToString();
                    }
                    else
                    {
                        specids = ids + "," + spec.Id;
                    }
                }
            }
            else if (gs_id > 0 || AttrName != "") //根据规格id或者名称生成表格 添加规格结果
            {
                string ids = "";
                if (Session["miniappgoodsspecids"] != null)
                {
                    ids = Session["miniappgoodsspecids"].ToString();
                }
                specids = ids;
                if (gs_id > 0)
                {
                    attrM = StoreGoodsAttrBLL.SingleModel.GetModel(gs_id);
                }
                else
                {
                    attrM = StoreGoodsAttrBLL.SingleModel.GetModel($"StoreId={storeid} and AttrName='{AttrName}' and State>=0");
                }
                if (attrM == null)
                {
                    return Content("");
                }
            }
            else if (remove_gs_id > 0) //删除规格结果
            {
                string ids = "";
                if (Session["miniappgoodsspecids"] != null)
                {
                    ids = Session["miniappgoodsspecids"].ToString();
                }
                if (ids != "")
                {
                    List<StoreGoodsSpec> specnewlist = StoreGoodsSpecBLL.SingleModel.GetList($"Id in ({ids}) and AttrId<>{remove_gs_id}");
                    specids = string.Join(",", specnewlist.Select(m => m.Id).Distinct());
                }
                else
                {
                    specids = "";
                }
            }
            else if (remove_spec_id > 0) //删除属性结果
            {
                string ids = "";
                if (Session["miniappgoodsspecids"] != null)
                {
                    ids = Session["miniappgoodsspecids"].ToString();
                }
                string[] strids = ids.Split(',');
                List<string> list = new List<string>(strids);
                if (list.IndexOf(remove_spec_id.ToString()) >= 0)
                {
                    list.Remove(remove_spec_id.ToString());
                }
                strids = list.ToArray();
                specids = string.Join(",", strids);
            }
            else
            {
                specids = "";
            }
            //更新集合
            Session["miniappgoodsspecids"] = specids;

            //获取所有属性集合
            List<StoreGoodsSpec> specalllist = new List<StoreGoodsSpec>();
            List<StoreGoodsAttr> attralllist = new List<StoreGoodsAttr>();
            if (specids != "")
            {
                specalllist = StoreGoodsSpecBLL.SingleModel.GetList($"Id in ({specids})");
                attralllist = StoreGoodsAttrBLL.SingleModel.GetList($"Id in ({string.Join(",", specalllist.Select(m => m.AttrId).Distinct())})");
            }
            if (attrM.Id > 0)
            {
                attralllist.Add(attrM);
            }
            #region 打印规格与属性
            Str.Append("<!--规格与规格值-->\n");
            Str.Append("<input name=\"add_spec\" type=\"hidden\"/>\n");
            foreach (StoreGoodsAttr attr in attralllist)
            {
                Str.Append("<ul class=\"removespec\">\n");
                Str.Append("<li class=\"clearfix mt-spacing guige-li\">\n");
                Str.Append($"<div class=\"fl\">{attr.AttrName}</div>\n");
                Str.Append($"<div class=\"fr pointer\" style=\"color: #27f;\" gs_id=\"{attr.Id}\">删除</div>\n");
                Str.Append("</li>\n");
                Str.Append("<li class=\"clearfix\" style=\"padding: 10px; \">\n");
                var speclist = specalllist.Where(m => m.AttrId == attr.Id);
                int i = 0;
                foreach (StoreGoodsSpec spec in speclist)
                {
                    i++;
                    Str.Append("<div class=\"fl\">\n");
                    Str.Append("<div class=\"guige-zhi\">\n");
                    Str.Append($"<span>{spec.SpecName}</span>\n");
                    Str.Append($"<a class=\"guige-zhi-close\" spec_id=\"{spec.Id}\" guige_index=\"{i}\"></a>\n");
                    Str.Append("</div>\n");
                    Str.Append("</div>\n");
                }
                Str.Append("<div class=\"guige-tianjia\" >\n");
                Str.Append($"<a class=\"gsps\" gs_id=\"{attr.Id}\">+添加</a>\n");
                Str.Append("</div>\n");
                Str.Append("</li>\n");
                Str.Append("</ul>\n");
            }

            #endregion

            #region 打印表格
            if (specalllist.Count > 0)
            {
                //重新获取规格集合
                List<StoreGoodsAttr> attrlist = StoreGoodsAttrBLL.SingleModel.GetList($"Id in ({string.Join(",", specalllist.Select(m => m.AttrId).Distinct())})");

                //开始打印表格
                Str.Append("<!--多规格表格-->\n");
                Str.Append("<table class=\"mt-spacing spgl-spgg-table more\" id=\"table_spec\" >\n");
                //打印抬头
                Str.Append("<tr>\n");
                foreach (StoreGoodsAttr attr in attrlist)
                {
                    Str.Append($"<th style=\"text-align: center;\">{attr.AttrName}</th>\n");
                }
                Str.Append("<th style=\"text-align: center;\">价格</th>\n");
                Str.Append("<th style=\"text-align: center;\">库存</th>\n");
                Str.Append("</tr>\n");
                //打印内容
                List<StoreGoodsSpec> speclist1 = specalllist.Where(m => m.AttrId == attrlist[0].Id).ToList<StoreGoodsSpec>();
                List<StoreGoodsSpec> speclist2 = new List<StoreGoodsSpec>();
                List<StoreGoodsSpec> speclist3 = new List<StoreGoodsSpec>();
                int rowspan1 = 1, rowspan2 = 1;
                if (attrlist.Count > 1)
                {
                    speclist2 = specalllist.Where(m => m.AttrId == attrlist[1].Id).ToList<StoreGoodsSpec>();
                    rowspan1 = speclist2.Count;
                }
                if (attrlist.Count > 2)
                {
                    speclist3 = specalllist.Where(m => m.AttrId == attrlist[2].Id).ToList<StoreGoodsSpec>();
                    rowspan1 = speclist2.Count * speclist3.Count;
                    rowspan2 = speclist3.Count;
                }
                for (int i = 0; i < speclist1.Count(); i++)
                {
                    Str.Append("<tr>\n");
                    Str.Append($"<td rowspan=\"{rowspan1}\">{speclist1[i].SpecName}</td>\n");
                    if (speclist2.Count() <= 0)
                    {
                        string gsp_ids = speclist1[i].Id + "_";
                        string price = "";
                        string count = "";
                        //编辑商品 有价格与数量
                        if (AttrDetailList.Count > 0)
                        {
                            StoreGoodsAttrDetail attrdetail = AttrDetailList.Where(m => m.id == gsp_ids).FirstOrDefault();
                            if (attrdetail.price > 0)
                            {
                                price = (attrdetail.price * 0.01).ToString();
                                count = attrdetail.count.ToString();
                            }
                        }
                        Str.Append($"<td><input class=\"spec_price\" name=\"price_{speclist1[i].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{price}\" /></td>\n");
                        Str.Append($"<td><input class=\"spec_count\" name=\"count_{speclist1[i].Id}\" type=\"text\" value=\"{count}\" /></td>\n");
                        Str.Append("</tr>\n");
                    }
                    else
                    {
                        for (int j = 0; j < speclist2.Count(); j++)
                        {
                            if (j > 0)
                            {
                                Str.Append("<tr>\n");
                            }
                            Str.Append($"<td rowspan=\"{rowspan2}\">{speclist2[j].SpecName}</td>\n");
                            if (speclist3.Count() <= 0)
                            {
                                string gsp_ids = speclist1[i].Id + "_" + speclist2[j].Id + "_";
                                string price = "";
                                string count = "";
                                //编辑商品 有价格与数量
                                if (AttrDetailList.Count > 0)
                                {
                                    StoreGoodsAttrDetail attrdetail = AttrDetailList.Where(m => m.id == gsp_ids).FirstOrDefault();
                                    if (attrdetail.price > 0)
                                    {
                                        price = (attrdetail.price * 0.01).ToString();
                                        count = attrdetail.count.ToString();
                                    }
                                }
                                Str.Append($"<td><input class=\"spec_price\" name=\"price_{speclist1[i].Id}_{speclist2[j].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{price}\" /></td>\n");
                                Str.Append($"<td><input class=\"spec_count\" name=\"count_{speclist1[i].Id}_{speclist2[j].Id}\" type=\"text\" value=\"{count}\" /></td>\n");
                                Str.Append("</tr>\n");
                            }
                            else
                            {
                                for (int k = 0; k < speclist3.Count(); k++)
                                {
                                    if (k > 0)
                                    {
                                        Str.Append("<tr>\n");
                                    }
                                    string gsp_ids = speclist1[i].Id + "_" + speclist2[j].Id + "_" + speclist3[k].Id + "_";
                                    string price = "";
                                    string count = "";
                                    //编辑商品 有价格与数量
                                    if (AttrDetailList.Count > 0)
                                    {
                                        StoreGoodsAttrDetail attrdetail = AttrDetailList.Where(m => m.id == gsp_ids).FirstOrDefault();
                                        if (attrdetail.price > 0)
                                        {
                                            price = (attrdetail.price * 0.01).ToString();
                                            count = attrdetail.count.ToString();
                                        }
                                    }
                                    Str.Append($"<td rowspan=\"1\">{speclist3[k].SpecName}</td>\n");
                                    Str.Append($"<td><input class=\"spec_price\" name=\"price_{speclist1[i].Id}_{speclist2[j].Id}_{speclist3[k].Id}\" type=\"text\" gsp_ids=\"{gsp_ids}\" value=\"{price}\" /></td>\n");
                                    Str.Append($"<td><input class=\"spec_count\" name=\"count_{speclist1[i].Id}_{speclist2[j].Id}_{speclist3[k].Id}\" type=\"text\" value=\"{count}\" /></td>\n");
                                    Str.Append("</tr>\n");
                                }
                            }
                        }
                    }

                }
                //底部批量设置
                Str.Append("<tr>\n");
                Str.Append($"<td colspan=\"{attrlist.Count + 2}\">\n");
                Str.Append("<div style=\"float:left;\">\n批量设置:\n");
                Str.Append("<span style=\"display: none;\" id=\"bt_inp\">\n");
                Str.Append("<input placeholder=\"输入价格\" name=\"batch_price\"/>\n");
                Str.Append("<input placeholder=\"输入库存\" name=\"batch_count\"/>\n");
                Str.Append("<a style=\"color: #07d;\" id=\"batch_save\">保存</a>\n");
                Str.Append("<a style=\"color: #07d;\" id=\"batch_cancel\">取消</a>\n");
                Str.Append("</span>\n");
                Str.Append("<span id=\"bt_btn\">\n");
                Str.Append("<a style=\"color: #07d;\" id=\"batch_price\">价格</a>\n");
                Str.Append("<a style=\"color: #07d;\" id=\"batch_count\">库存</a>\n");
                Str.Append("</span>\n");
                Str.Append("</div>\n");
                Str.Append("</td>\n");
                Str.Append("</tr>\n");
                Str.Append("<table>");
            }
            #endregion

            return Content(Str.ToString());
        }


        /// <summary>
        /// 批量排序 产品类别
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public ActionResult SaveGoodsTypeSort(List<StoreGoodsType> list)
        {
            if (list == null || list.Count <= 0)
            {
                return Json(new { isok = false, msg = "数据错误" });
            }
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" });
            }

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "店铺不存在" });
            }

            bool isok = StoreGoodsTypeBLL.SingleModel.UpdateListSort(list, store.Id);
            string msg = isok ? "保存成功" : "保存失败";

            return Json(new { isok = isok, msg = msg });
        }

        
        /// <summary>
        /// 批量保存商品排序
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public ActionResult SaveGoodsSort(List<StoreGoods> list)
        {
            if (list == null || list.Count <= 0)
            {
                return Json(new { isok = false, msg = "数据错误" });
            }
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误!" });
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "没有权限" });
            }

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "店铺不存在" });
            }

            bool isok = StoreGoodsBLL.SingleModel.UpdateListSort(list, store.Id);
            string msg = isok ? "保存成功" : "保存失败";

            return Json(new { isok = isok, msg = msg });
        }


        #endregion

        #endregion

        #region 商品规格管理
        public ActionResult AddAttrSpecList(int appId)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.appId = appId;

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "找不到该店铺！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.StoreId = store.Id;

            List<StoreGoodsAttr> goodsAttrList = StoreGoodsAttrBLL.SingleModel.GetList($"StoreId={store.Id} and State>=0 ");
            foreach (StoreGoodsAttr item in goodsAttrList)
            {
                List<StoreGoodsSpec> speclist = StoreGoodsSpecBLL.SingleModel.GetList($"AttrId={item.Id} and State>=0 ");
                item.SpecList = speclist;
            }
            return View(goodsAttrList);
        }

        /// <summary>
        /// 添加/编辑/删除商品规格
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddAttrList(int appId, StoreGoodsAttr goodsAttr)
        {
            //清除规格缓存
            StoreGoodsAttrBLL.SingleModel.RemoveStoreGoodsAttrCache(goodsAttr.Id);


            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsAttr.State != -1 && string.IsNullOrEmpty(goodsAttr.AttrName.Trim()))
            {
                return Json(new { isok = false, msg = "规格名称不能为空！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsAttr.StoreId == 0)
            {
                return Json(new { isok = false, msg = "店铺id不能为0！" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModel(goodsAttr.StoreId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "找不到该店铺！" }, JsonRequestBehavior.AllowGet);
            }
            int resultInt = 0;//
            List<StoreGoodsAttr> goodsAttrs = StoreGoodsAttrBLL.SingleModel.GetList($"StoreId={goodsAttr.StoreId} and State>=0");
            if (goodsAttr.Id == 0)//添加
            {
                if (goodsAttrs.Count >= 100)
                {
                    return Json(new { isok = false, msg = "目前最多能添加100个规格哦" });
                }
                if (goodsAttrs.Any(m => m.AttrName == goodsAttr.AttrName))
                {
                    return Json(new { isok = false, msg = "规格名称已存在 , 请重新添加" });
                }
                object result = StoreGoodsAttrBLL.SingleModel.Add(new StoreGoodsAttr() { StoreId = goodsAttr.StoreId, AttrName = goodsAttr.AttrName, State = 0 });
                if (int.Parse(result.ToString()) > 0)
                {
                    return Json(new { isok = true, msg = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else//编辑
            {
                if (goodsAttrs.Any(m => m.AttrName == goodsAttr.AttrName && m.Id != goodsAttr.Id))
                {
                    return Json(new { isok = false, msg = "规格名称已存在 , 请重新添加" });
                }
                StoreGoodsAttr updateModel = StoreGoodsAttrBLL.SingleModel.GetModel(goodsAttr.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到规格" });
                }
                //删除，做数量验证
                if (goodsAttr.State == -1)
                {
                    List<StoreGoodsAttrSpec> attrlist = StoreGoodsAttrSpecBLL.SingleModel.GetList($"AttrId={goodsAttr.Id}");
                    if (attrlist.Any())
                    {
                        int attrGoodsCount = StoreGoodsBLL.SingleModel.GetCount($"Id in ({string.Join(",", attrlist.Select(m => m.GoodsId).Distinct())}) and State>=0");
                        if (attrGoodsCount > 0)
                        {
                            return Json(new { isok = false, msg = $"有{attrGoodsCount}个商品使用了该规格，不能删除" });
                        }
                    }

                    updateModel.State = -1;
                    resultInt = StoreGoodsAttrBLL.SingleModel.Update(updateModel, "State") ? 1 : 0;
                }
                else
                {
                    updateModel.AttrName = goodsAttr.AttrName;
                    resultInt = StoreGoodsAttrBLL.SingleModel.Update(updateModel, "AttrName") ? 1 : 0;
                }
                return Json(new { isok = true, msg = "修改成功" });
            }


            return Json(new { isok = false, msg = "系统错误！" });
        }

        /// <summary>
        /// 添加/编辑/删除商品规格属性
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSpecList(int appId, StoreGoodsSpec goodsSpec)
        {
            //清除规格缓存
            StoreGoodsSpecBLL.SingleModel.RemoveStoreGoodsSpecCache(goodsSpec.Id);

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsSpec.State != -1 && string.IsNullOrEmpty(goodsSpec.SpecName.Trim()))
            {
                return Json(new { isok = false, msg = "属性名称不能为空！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsSpec.AttrId == 0)
            {
                return Json(new { isok = false, msg = "所属规格id不能为0！" }, JsonRequestBehavior.AllowGet);
            }

            StoreGoodsAttr attr = StoreGoodsAttrBLL.SingleModel.GetModel(goodsSpec.AttrId);
            if (attr == null)
            {
                return Json(new { isok = false, msg = "找不到所属规格！" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModel(attr.StoreId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "找不到该店铺！" }, JsonRequestBehavior.AllowGet);
            }
            int resultInt = 0;//
            List<StoreGoodsSpec> goodsSpecList = StoreGoodsSpecBLL.SingleModel.GetList($"AttrId={goodsSpec.AttrId} and State>=0");
            if (goodsSpec.Id == 0)//添加
            {
                //if (goodsSpecList.Count >= 10)
                //{
                //    return Json(new { isok = false, msg = "每个规格最多能添加10个属性哦" });
                //}
                if (goodsSpecList.Any(m => m.SpecName == goodsSpec.SpecName))
                {
                    return Json(new { isok = false, msg = "属性名称已存在 , 请重新添加" });
                }
                object result = StoreGoodsSpecBLL.SingleModel.Add(new StoreGoodsSpec() { AttrId = goodsSpec.AttrId, SpecName = goodsSpec.SpecName, State = 0 });
                if (int.Parse(result.ToString()) > 0)
                {
                    return Json(new { isok = true, msg = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else//编辑
            {
                if (goodsSpecList.Any(m => m.SpecName == goodsSpec.SpecName && m.Id != goodsSpec.Id))
                {
                    return Json(new { isok = false, msg = "属性名称已存在 , 请重新添加" });
                }
                StoreGoodsSpec updateModel = StoreGoodsSpecBLL.SingleModel.GetModel(goodsSpec.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到该属性" });
                }
                //删除，做数量验证
                if (goodsSpec.State == -1)
                {
                    List<StoreGoodsAttrSpec> speclist = StoreGoodsAttrSpecBLL.SingleModel.GetList($"SpecId={goodsSpec.Id}");
                    if (speclist.Any())
                    {
                        int specGoodsCount = StoreGoodsBLL.SingleModel.GetCount($"Id in ({string.Join(",", speclist.Select(m => m.GoodsId).Distinct())}) and State>=0");
                        if (specGoodsCount > 0)
                        {
                            return Json(new { isok = false, msg = $"有{specGoodsCount}个商品使用了该属性，不能删除" });
                        }
                    }

                    updateModel.State = -1;
                    resultInt = StoreGoodsSpecBLL.SingleModel.Update(updateModel, "State") ? 1 : 0;
                }
                else
                {
                    updateModel.SpecName = goodsSpec.SpecName;
                    resultInt = StoreGoodsSpecBLL.SingleModel.Update(updateModel, "SpecName") ? 1 : 0;
                }
                return Json(new { isok = true, msg = "修改成功" });
            }


            return Json(new { isok = false, msg = "系统错误！" });
        }
        #endregion

        #region 商品类型
        [HttpGet]
        public ActionResult AddGoodType(int appId)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.appId = appId;

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "找不到该店铺！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.StoreId = store.Id;

            List<StoreGoodsType> goodsTypeList = StoreGoodsTypeBLL.SingleModel.GetList($"StoreId={store.Id} and State>=0",100,1,"*", "SortVal desc,Id desc");
            return View(goodsTypeList);
        }

        /// <summary>
        /// 添加商品分类
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddGoodType(int appId, StoreGoodsType goodsTypeModel)
        {

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsTypeModel.State != -1 && string.IsNullOrEmpty(goodsTypeModel.Name.Trim()))
            {
                return Json(new { isok = false, msg = "商品分类名称不能为空！" }, JsonRequestBehavior.AllowGet);
            }

            if (goodsTypeModel.StoreId == 0)
            {
                return Json(new { isok = false, msg = "店铺id不能为0！" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModel(goodsTypeModel.StoreId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "找不到该店铺！" }, JsonRequestBehavior.AllowGet);
            }
            int resultInt = 0;//
            List<StoreGoodsType> goodsTypeList = StoreGoodsTypeBLL.SingleModel.GetlistByStoreId(goodsTypeModel.StoreId);

            //清除缓存
            StoreGoodsTypeBLL.SingleModel.RemoveStoreGoodsTypeCache(goodsTypeModel.StoreId);
            if (goodsTypeModel.Id == 0)//添加
            {
                //int totalCount = _goodsTypeConfigBll.GetCountByStoreId(goodsTypeModel.StoreId);
                if (goodsTypeList.Count >= 50)
                {
                    return Json(new { isok = false, msg = "目前最多能添加50个分类哦" });
                }
                if (goodsTypeList.Any(m => m.Name == goodsTypeModel.Name))
                {
                    return Json(new { isok = false, msg = "分类名称已存在 , 请重新添加" });
                }
                object result = StoreGoodsTypeBLL.SingleModel.Add(new StoreGoodsType() { SortVal = goodsTypeModel.SortVal, StoreId = goodsTypeModel.StoreId, Name = goodsTypeModel.Name, LogImg = goodsTypeModel.LogImg, CreateDate = DateTime.Now, UpdateDate = DateTime.Now });
                if (int.Parse(result.ToString()) > 0)
                {
                    return Json(new { isok = true, msg = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else//编辑
            {
                if (goodsTypeList.Any(m => m.Name == goodsTypeModel.Name && m.Id != goodsTypeModel.Id))
                {
                    return Json(new { isok = false, msg = "分类名称已存在 , 请重新添加" });
                }
                StoreGoodsType updateModel = StoreGoodsTypeBLL.SingleModel.GetModel(goodsTypeModel.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到分类" });
                }
                //删除，做数量验证
                if (goodsTypeModel.State == -1)
                {
                    int typeGoodsCount = StoreGoodsBLL.SingleModel.GetCountByTypeId(updateModel.Id);
                    if (typeGoodsCount > 0)
                    {
                        return Json(new { isok = false, msg = $"该分类下有{typeGoodsCount}个商品，不能删除" });
                    }
                    updateModel.State = -1;
                    updateModel.UpdateDate = DateTime.Now;
                    //只修改名称，排序，修改时间
                    resultInt = StoreGoodsTypeBLL.SingleModel.Update(updateModel, "State,UpdateDate") ? 1 : 0;
                }
                else
                {
                    updateModel.UpdateDate = DateTime.Now;
                    updateModel.Name = goodsTypeModel.Name;
                    updateModel.LogImg = goodsTypeModel.LogImg;
                    updateModel.SortVal = goodsTypeModel.SortVal;
                    //只修改名称，排序，修改时间
                    resultInt = StoreGoodsTypeBLL.SingleModel.Update(updateModel, "UpdateDate,SortVal,Name,LogImg") ? 1 : 0;
                }
                return Json(new { isok = true, msg = "修改成功" });
            }


            return Json(new { isok = false, msg = "系统错误！" });
        }
        public ActionResult CityCarouselEditPartail(int id)
        {
            StoreGoodsType goodstype = StoreGoodsTypeBLL.SingleModel.GetModel(id);
            return PartialView("CityCarouselEdit", goodstype);
        }
        #endregion

        #region 运费模板
        [HttpGet]
        public ActionResult existsFreight(int appId)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.appId = appId;

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "找不到该店铺！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.StoreId = store.Id;

            bool feight = StoreFreightTemplateBLL.SingleModel.Exists($"storeid = {store.Id} and state >= 0");
            return Json(new { isok = feight, msg = "请先添加运费模板, 否则不可体验/发布！" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [LoginFilter]
        public ActionResult AddFreight(int appId)
        {
            ViewBag.appId = appId;

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "找不到该店铺！" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.StoreId = store.Id;

            List<StoreFreightTemplate> feight = StoreFreightTemplateBLL.SingleModel.GetList($"storeid = {store.Id} and state >= 0 ");
            return View(feight);
        }
        /// <summary>
        /// 添加运费模板
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        [LoginFilter]
        public ActionResult AddFreight(int appId, store.StoreFreightTemplate freightTmplate)
        {
            if (string.IsNullOrEmpty(freightTmplate.Name.Trim()))
            {
                return Json(new { isok = false, msg = "模板名称不可为空！" }, JsonRequestBehavior.AllowGet);
            }

            if (freightTmplate.StoreId == 0)
            {
                return Json(new { isok = false, msg = "店铺id不能为0！" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModel(freightTmplate.StoreId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "找不到该店铺！" }, JsonRequestBehavior.AllowGet);
            }
            //var freightList = new C_FreightTemplateBLL().GetList();
            int resultInt = 0;//
            if (freightTmplate.Id == 0)//添加
            {
                object result = StoreFreightTemplateBLL.SingleModel.Add(new Entity.MiniApp.Stores.StoreFreightTemplate() { BaseCount = freightTmplate.BaseCount, StoreId = freightTmplate.StoreId, Name = freightTmplate.Name, BaseCost = freightTmplate.BaseCost, ExtraCost = freightTmplate.ExtraCost,/*IsDefault = freightTmplate.IsDefault,*/CreateTime = DateTime.Now });
                if (int.Parse(result.ToString()) > 0)
                {
                    return Json(new { isok = true, msg = result.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else//编辑
            {
                store.StoreFreightTemplate updateModel = StoreFreightTemplateBLL.SingleModel.GetModel(freightTmplate.Id);
                if (updateModel == null)
                {
                    return Json(new { isok = false, msg = "找不到模板" });
                }
                else
                {
                    updateModel.IsDefault = freightTmplate.IsDefault;
                    updateModel.Name = freightTmplate.Name;
                    updateModel.BaseCost = freightTmplate.BaseCost;
                    updateModel.ExtraCost = freightTmplate.ExtraCost;
                    updateModel.BaseCount = freightTmplate.BaseCount;
                    //只修改名称，排序，freightTmplate
                    resultInt = StoreFreightTemplateBLL.SingleModel.Update(updateModel) ? 1 : 0;
                }
                return Json(new { isok = true, msg = "修改成功" });
            }

            //IsReferred(int ftid, int storeId)
            return Json(new { isok = false, msg = "系统错误！" });
        }

        /// <summary>
        /// 删除运费模板
        /// </summary>
        /// <param name="id"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult delFreightTemplate(store.StoreFreightTemplate freightTmplate)
        {
            
            if (StoreFreightTemplateBLL.SingleModel.IsReferred(freightTmplate.Id, freightTmplate.StoreId))
            {
                return Json(new { isok = false, msg = "该模板有商品在使用,暂时不可删除！" });
            }
            StoreFreightTemplate model = StoreFreightTemplateBLL.SingleModel.GetModel(freightTmplate.Id) ?? new store.StoreFreightTemplate();
            if (model.IsDefault == 1)
            {
                return Json(new { isok = true, msg = "默认模板不允许删除！" });
            }
            model.State = -1;
            if (StoreFreightTemplateBLL.SingleModel.Update(model, "state"))
            {
                //若无其他运费模板,则添加一个默认的。默认的不允许删除只能编辑
                int freightCount = StoreFreightTemplateBLL.SingleModel.GetCount($" StoreId={model.StoreId} and state >= 0 ");
                if (freightCount <= 0)//若商家无运费模板添加一个默认免费的,以便解决现阶段无运费模板无法下单的问题
                {
                    StoreFreightTemplateBLL.SingleModel.Add(new store.StoreFreightTemplate() { BaseCount = 999, StoreId = model.StoreId, Name = "免运费", BaseCost = 0, ExtraCost = 0, IsDefault = 1, CreateTime = DateTime.Now });
                }
                return Json(new { isok = true, msg = "删除成功！" });
            }
            else
            {
                return Json(new { isok = false, msg = "删除失败！" });
            }
        }
        /// <summary>
        /// 运费模板详情
        /// </summary>
        /// <param name="CityInfoId"></param>
        /// <param name="goodtypename"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>

        public ActionResult getFreight(int Id)
        {
            return Json(StoreFreightTemplateBLL.SingleModel.GetModel(Id), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 订单管理
        /// <summary>
        /// 同城流水报表
        /// </summary>
        /// <param name="cityInfoId"></param>
        /// <param name="name"></param>
        /// <param name="typevalue"></param>
        /// <param name="pageIndex"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        public ActionResult OrderList(int appId, string orderNum = "", int pageIndex = 0, string startdate = "", string enddate = "", bool export = false, string userName = "", int vipLevelId = 0, int state = -999)
        {
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            StringBuilder wheresql = new StringBuilder();
            wheresql.Append(" 1=1 ");

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            ViewBag.appId = appId;

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (store == null)
            {
                return View("PageError", new Return_Msg() { Msg = "店铺不存在!", code = "500" });
            }

            wheresql.Append(" and orders.StoreId = " + store.Id);

            ViewBag.orderNum = orderNum == "" ? "" : orderNum.ToString();
            ViewBag.startdate = startdate;
            ViewBag.enddate = enddate;
            int pageSize = 20;
            pageIndex = pageIndex == 0 ? 1 : pageIndex;
            ViewBag.pageSize = pageSize;
            ViewBag.curState = state;

            List<MySqlParameter> sqlparams = new List<MySqlParameter>();

            StringBuilder orderWhere = new StringBuilder();
            //订单号筛选
            if (!string.IsNullOrWhiteSpace(orderNum))
            {
                wheresql.Append(" and orders.OrderNum = " + orderNum);
            }

            try
            {
                //时间段筛选，限制时间间隔最大为一个月
                if (!string.IsNullOrEmpty(startdate) && !string.IsNullOrEmpty(enddate) &&
                    DateTime.Parse(startdate).AddMonths(1) >= DateTime.Parse(enddate))
                {
                    wheresql.Append(" and orders.CreateDate >= '" + startdate + "'");
                    wheresql.Append(" and orders.CreateDate <= '" + enddate + " 23:59:59'");
                }
                //else if (string.IsNullOrEmpty(startdate) && string.IsNullOrEmpty(enddate))
                //{
                //    ViewBag.startdate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                //    wheresql.Append(" and orders.CreateDate >= '" + DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") + "'");

                //    wheresql.Append(" and orders.CreateDate <= '" + DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59'");
                //    ViewBag.enddate = DateTime.Now.ToString("yyyy-MM-dd");
                //}
                //else
                //{
                //    return View("PageError", new Return_Msg() { Msg = "筛选时长不能超过一个月哦!", code = "500" });
                //}

                if (state != -999)
                {
                    wheresql.Append($" and orders.state = {state} ");
                }

                //会员名称
                if (!userName.IsNullOrWhiteSpace())
                {
                    wheresql.Append(" and c.NickName = @NickName");
                    sqlparams.Add(new MySql.Data.MySqlClient.MySqlParameter("@NickName", userName));
                }
                //会员级别
                if (vipLevelId > 0)
                {
                    wheresql.Append($" and v.levelid = {vipLevelId} ");
                }

                int totalCount = 0;
                //获得订单记录
                List<StoreAdminGoodsOrder> companys = new StoreAdminGoodsOrderBLL().GetAdminListForStores(wheresql.ToString(), pageSize, pageIndex, out totalCount, export, sqlparams.ToArray());

                if (companys == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
                }


                ViewBag.TotalCount = totalCount;

                //导出Excel
                if (export)
                {
                    //记录已经输出过的订单号
                    List<string> orderNums = new List<string>();

                    DataTable table = new DataTable();
                    table.Columns.AddRange(new[]
                    {
                            new DataColumn("订单号"),
                            new DataColumn("商品名称"),
                            new DataColumn("数量"),
                            new DataColumn("单价(元)"),
                            new DataColumn("日期"),
                            new DataColumn("地址"),
                            new DataColumn("买家"),
                            new DataColumn("手机号"),
                            new DataColumn("会员"),
                            new DataColumn("会员级别"),
                            new DataColumn("支付状态"),
                            new DataColumn("支付方式"),
                            //new DataColumn("留言")
                            new DataColumn("订单金额（元）"),
                            new DataColumn("备注")
                    });
                    if (companys != null && companys.Count > 0)
                    {
                        //当前数据行是否当前订单的第一条记录,否则订单号,金额不重复输出
                        bool isCurOrdersDtlFirst = true;
                        foreach (StoreAdminGoodsOrder item in companys.OrderByDescending(c => c.OrderNum))
                        {
                            isCurOrdersDtlFirst = !orderNums.Any(o => o.Equals(item.OrderNum));
                            DataRow row = table.NewRow();
                            row["订单号"] = isCurOrdersDtlFirst ? item.OrderNum : string.Empty;
                            row["商品名称"] = item.goodsName;
                            row["数量"] = item.count;
                            row["单价(元)"] = item.priceStr;
                            row["日期"] = item.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
                            row["地址"] = item.Address;
                            row["买家"] = item.NickName;
                            row["手机号"] = item.TelePhone;
                            row["会员"] = item.userName;
                            row["会员级别"] = item.levelname;
                            row["支付状态"] = Enum.GetName(typeof(OrderState), item.State);
                            row["支付方式"] = Enum.GetName(typeof(miniAppBuyMode), item.buyMode);
                            //row["留言"] = item.Message;
                            row["订单金额（元）"] = isCurOrdersDtlFirst ? (item.BuyPrice * 0.01).ToString() : "";
                            row["备注"] = isCurOrdersDtlFirst ? item.Remark : string.Empty; 
                            table.Rows.Add(row);

                            orderNums.Add(item.OrderNum);
                        }
                    }
                    ExcelHelper<UserForm>.Out2Excel(table, xcx.Title + "订单记录"); //导出
                }


                List<VipLevel> levelList = VipLevelBLL.SingleModel.GetList($" appid='{xcx.AppId}' and state >= 0") ?? new List<VipLevel>();
                ViewBag.levelList = levelList;
                ViewBag.curLevel = vipLevelId;
                ViewBag.userName = userName;

                #region 总收益

                //int remoney = 0;//退款的金额
                //var _cityModersBll = new CityMordersBLL();
                //int smoney = _cityModersBll.GetSumMoneyNew(wheresql.ToString(), storeIds);
                //List<CityMorders> tradenos = _cityModersBll.GetTradeNoNew(wheresql.ToString(), storeIds);
                //if (tradenos.Count > 0)
                //{
                //    remoney = _refundResultBll.GetSumMoney($"'{tradenos.JoinStrings("','")}'");
                //}

                //ViewBag.SumShouyi = smoney;
                //ViewBag.RefundCount = remoney;
                #endregion

                return View(companys);
            }
            catch (Exception)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙!", code = "500" });
            }
        }

        //更新备注
        [HttpPost]
        public ActionResult OrderRemark(int id, string remark)
        {
            StoreGoodsOrder Order = StoreGoodsOrderBLL.SingleModel.GetModel(id);
            if (Order == null)
            {
                return Json(new { isok = false, msg = "找不到该订单！" }, JsonRequestBehavior.AllowGet);
            }
            Order.Remark = remark;
            StoreGoodsOrderBLL.SingleModel.Update(Order, "Remark");
            return Json(new { isok = true, msg = "备注成功！" }, JsonRequestBehavior.AllowGet);
        }

        //调整价格
        //public ActionResult order_adjust_fee_save(int adjustoid, double goods_amount, double ship_price, double totalPrice)
        public ActionResult order_adjust_fee_save(int adjustoid, int goods_amount, int ship_price, int totalPrice)
        {
            //goods_amount = Convert.ToDouble(decimal.Round(decimal.Parse(goods_amount.ToString()),2));
            //ship_price = Convert.ToDouble(decimal.Round(decimal.Parse(ship_price.ToString()), 2));
            //totalPrice = Convert.ToDouble(decimal.Round(decimal.Parse(totalPrice.ToString()), 2));
            if (goods_amount < 1)
            {
                return Json(new { isok = false, msg = "商品金额最少0.01元！" }, JsonRequestBehavior.AllowGet);
            }

            if (goods_amount * 0.01 > 100000)
            {
                return Json(new { isok = false, msg = "商品总金额最多10万！" }, JsonRequestBehavior.AllowGet);
            }
            if (ship_price * 0.01 > 100000)
            {
                return Json(new { isok = false, msg = "运费最多10万！" }, JsonRequestBehavior.AllowGet);
            }
            //var loginer = C_UserInfoBLL.SingleModel.GetModelByOpenId(dzaccount.OpenId);/* LoginUser.GetCUserFromCookie();*/
            //if (loginer == null)
            //{
            //    return Json(new { isok = false, msg = "登录信息异常！" }, JsonRequestBehavior.AllowGet);
            //}
            
            StoreGoodsOrder Order = StoreGoodsOrderBLL.SingleModel.GetModel(adjustoid);
            if (Order == null)
            {
                return Json(new { isok = false, msg = "找不到该订单！" }, JsonRequestBehavior.AllowGet);
            }
            int bprice = Convert.ToInt32((goods_amount + ship_price));
            if (bprice != totalPrice)
            {
                return Json(new { isok = false, msg = $"订单调整金额有误，请刷新后再试！" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModel(Order.StoreId);
            if (store == null)
            {
                return Json(new { isok = false, msg = $"订单异常！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(store.appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = $"订单异常！" }, JsonRequestBehavior.AllowGet);
            }
            string oldbprice = (Order.BuyPrice * 0.01).ToString("0.00");
            string oldfprice = (Order.FreightPrice * 0.01).ToString("0.00");
            Order.BuyPrice = Convert.ToInt32(totalPrice);
            Order.FreightPrice = Convert.ToInt32(ship_price);

            CityMordersBLL citymorderBLL = new CityMordersBLL();
            CityMorders morder = citymorderBLL.GetModel(Order.OrderId);
            string no = WxPayApi.GenerateOutTradeNo();
            morder.orderno = no;
            morder.payment_free = Order.BuyPrice;
            morder.ShowNote = $" {umodel.Title}购买商品支付{morder.payment_free * 0.01}元";
            if (!citymorderBLL.Update(morder))
            {
                return Json(new { isok = false, msg = "订单金额调整失败！" }, JsonRequestBehavior.AllowGet);
            }
            if (StoreGoodsOrderBLL.SingleModel.Update(Order, "BuyPrice,FreightPrice,OrderId"))
            {
                //new MiniappGoodsOrderLogBLL().AddLog(Order.Id, loginer.Id, $"调整商品总价从¥{oldbprice}改为¥{totalPrice * 0.01},配送金额从¥{oldfprice}改为¥{ship_price * 0.01}。");
                StoreGoodsOrderLogBLL.SingleModel.AddLog(Order.Id, 0, $"调整商品总价从¥{oldbprice}改为¥{totalPrice * 0.01},配送金额从¥{oldfprice}改为¥{ship_price * 0.01}。");
            }
            else
            {
                return Json(new { isok = false, msg = "订单金额调整失败！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = true, msg = "订单金额调整成功！" }, JsonRequestBehavior.AllowGet);
        }

        //取消订单
        [HttpPost]
        public ActionResult ordercancel(int oidcancel, string state_info, string other_state_info = "")
        {
            ServiceResult result = new ServiceResult();
            //var loginer = C_UserInfoBLL.SingleModel.GetModelByOpenId(dzaccount.OpenId);/* LoginUser.GetCUserFromCookie();*/
            //if (loginer == null)
            //{
            //    return Json(new { isok = false, msg = "登录信息异常！" }, JsonRequestBehavior.AllowGet);
            //}

            
            StoreGoodsOrder goodsOrder = StoreGoodsOrderBLL.SingleModel.GetModel(oidcancel);
            if (goodsOrder == null)
            {
                return Json(new { isok = false, msg = "订单信息异常！" }, JsonRequestBehavior.AllowGet);
            }
            Store store = StoreBLL.SingleModel.GetModel(goodsOrder.StoreId);

            //取消订单
            goodsOrder.State = (int)OrderState.取消订单;
            if (StoreGoodsOrderBLL.SingleModel.Update(goodsOrder, "State"))
            {
                //记录日志
                if (other_state_info == "")
                {
                    //new MiniappGoodsOrderLogBLL().AddLog(goodsOrder.Id, loginer.Id, "取消订单 操作说明：" + state_info);
                    StoreGoodsOrderLogBLL.SingleModel.AddLog(goodsOrder.Id, 0, "取消订单 操作说明：" + state_info);
                }
                else
                {
                    //new MiniappGoodsOrderLogBLL().AddLog(goodsOrder.Id, loginer.Id, "取消订单 操作说明：" + other_state_info);
                    StoreGoodsOrderLogBLL.SingleModel.AddLog(goodsOrder.Id, 0, "取消订单 操作说明：" + other_state_info);
                }
            }

            return Json(new { isok = true, msg = "取消成功！" }, JsonRequestBehavior.AllowGet);
        }

        //配送信息
        [HttpPost]
        public ActionResult getdistributioninfo(int id)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                
                StoreGoodsOrder goodsOrder = StoreGoodsOrderBLL.SingleModel.GetModel(id);
                if (goodsOrder == null)
                {
                    return Json(result.IsFailed("订单信息异常！"));
                }
                StoreAddress address = StoreAddressBLL.SingleModel.GetModel(goodsOrder.AddressId);
                StoreFreightTemplate freightTemplate = StoreFreightTemplateBLL.SingleModel.GetModel(goodsOrder.FreightTemplateId);
                result.Data.Add("DistributionInfo", new
                {
                    //NickName = address?.NickName,
                    //TelePhone = address?.TelePhone,
                    //Address = address?.Address,
                    NickName = goodsOrder.AccepterName,
                    TelePhone = goodsOrder.AccepterTelePhone,
                    Address = goodsOrder.Address,
                    DistributeName = freightTemplate == null ? "到店自提" : freightTemplate.Name,
                    DistributeOrderNo = goodsOrder.OrderNum,
                    DistributeTime = goodsOrder.CreateDate.ToString("yyyy-MM-dd HH:mm:ss")
                });
                return Json(result.IsSucceed());
            }
            catch (Exception)
            {
                return Json(result.IsFailed("数据异常 !"));
            }
        }

        //买家确认发货
        [HttpPost]
        public ActionResult confirmdistribute(int id, int state = (int)OrderState.待发货)
        {
            ServiceResult result = new ServiceResult();
            if (state != (int)OrderState.待发货 && state != (int)OrderState.正在配送)
            {
                return Json(result.IsFailed("参数错误！"));
            }
            //var loginer = C_UserInfoBLL.SingleModel.GetModelByOpenId(dzaccount.OpenId);/* LoginUser.GetCUserFromCookie();*/
            //if (loginer == null)
            //{
            //    return Json(result.IsFailed("登录信息异常！"));
            //}
            
            StoreGoodsOrder goodsOrder = StoreGoodsOrderBLL.SingleModel.GetModel(id);
            if (goodsOrder == null)
            {
                return Json(result.IsFailed("订单信息异常！"));
            }
            Store store = StoreBLL.SingleModel.GetModel(goodsOrder.StoreId);
            if (store == null)
            {
                return Json(result.IsFailed("订单信息异常！"));
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(store.appId);
            if (app == null)
            {
                return Json(result.IsFailed("订单信息异常！"));
            }

            //状态向下一阶段流动
            goodsOrder.State = state + 1;
            goodsOrder.DistributeDate = DateTime.Now;
            if (StoreGoodsOrderBLL.SingleModel.Update(goodsOrder, "State,DistributeDate"))
            {
                //记录日志
                if (state == (int)OrderState.待发货)
                {
                    //new MiniappGoodsOrderLogBLL().AddLog(goodsOrder.Id, loginer.Id, "确认发货成功");
                    StoreGoodsOrderLogBLL.SingleModel.AddLog(goodsOrder.Id, 0, "确认发货成功");

                    #region 发送电商订单发货通知 模板消息
                    object postData = StoreGoodsOrderBLL.SingleModel.getTemplateMessageData(goodsOrder.Id, SendTemplateMessageTypeEnum.电商订单配送通知);
                    TemplateMsg_Miniapp.SendTemplateMessage(goodsOrder.UserId, SendTemplateMessageTypeEnum.电商订单配送通知, (int)TmpType.小程序电商模板, postData);
                    #endregion
                }
                else if (state == (int)OrderState.正在配送)
                {
                    //new MiniappGoodsOrderLogBLL().AddLog(goodsOrder.Id, loginer.Id, "确认配送完成成功");
                    StoreGoodsOrderLogBLL.SingleModel.AddLog(goodsOrder.Id, 0, "确认配送完成成功");
                }
            }

            result = state == (int)OrderState.正在配送 ? result.IsSucceed(" 确认配送完成成功!") : result.IsSucceed("确认发货成功 !");
            return Json(result);
        }
        #endregion

        #region 订单详情页
        public ActionResult OrderDetail(int appId, int Oid)
        {
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            ViewBag.appId = appId;

            Store store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (store == null)
            {
                return View("PageError", new Return_Msg() { Msg = "店铺不存在!", code = "500" });
            }

            StoreGoodsOrder order = StoreGoodsOrderBLL.SingleModel.GetModel(Oid);
            if (order == null)
            {
                return View("PageError", new Return_Msg() { Msg = "订单不存在!", code = "500" });
            }

            //订单明细
            List<StoreGoodsCart> cartlist = StoreGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={order.Id}");
            List<StoreOrderCardDetail> newcartlist = new List<StoreOrderCardDetail>();

            string goodsIds = string.Join(",",cartlist?.Select(s=>s.GoodsId).Distinct());
            List<StoreGoods> storeGoodsList = StoreGoodsBLL.SingleModel.GetListByIds(goodsIds);

            foreach (StoreGoodsCart item in cartlist)
            {
                StoreOrderCardDetail cartM = new StoreOrderCardDetail();
                cartM.Id = item.Id;
                StoreGoods goods = storeGoodsList?.FirstOrDefault(f=>f.Id == item.GoodsId);
                if (goods != null)
                {
                    cartM.ImgUrl = goods.ImgUrl;
                    cartM.GoodsName = goods.GoodsName;
                }
                cartM.SpecInfo = item.SpecInfo;
                cartM.Price = item.Price;
                cartM.Count = item.Count;
                newcartlist.Add(cartM);
            }

            ViewBag.CartList = newcartlist;

            //运费模板
            if (order.FreightTemplateId == 0)
            {
                ViewBag.FreightT = "[卖家承担[0.0]]";
            }
            else
            {
                StoreFreightTemplate FT = StoreFreightTemplateBLL.SingleModel.GetModel(order.FreightTemplateId);
                if (FT != null)
                {
                    int price = FT.BaseCost;
                    if (cartlist.Count > FT.BaseCount)
                    {
                        price += (cartlist.Count - FT.BaseCount) * FT.ExtraCost;
                    }
                    //ViewBag.FreightT = $"[{FT.Name}[{(price * 0.01).ToString("0.00")}]]";
                    ViewBag.FreightT = $"[{FT.Name}]";
                }
            }

            //操作历史
            List<StoreGoodsOrderLog> loglist = StoreGoodsOrderLogBLL.SingleModel.GetList($"GoodsOrderId={order.Id}");

            string userIds = string.Join(",",loglist?.Select(s=>s.UserId).Distinct());
            List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

            loglist.ForEach(m =>
            {
                C_UserInfo user = userInfoList?.FirstOrDefault(f=>f.Id == m.UserId);
                m.UserName = user != null ? user.NickName : "卖家";
            });
            ViewBag.OrderLogList = loglist;
            return View(order);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 纯文本中把空格标识用p标签样式text-indent:2rem;代替
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReturnStr3(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return content;
            }

            if (content.IndexOf("\n") <= 0)
            {
                if (content.IndexOf(" ") == 0)
                {
                    content = "<p style='text-indent:2em;'>" + content + "</p>";
                }
                else
                {
                    content = "<p>" + content + "</p>";
                }
                return content;
            }

            string temp = content.Substring(0, content.IndexOf("\n"));
            if (temp.IndexOf(" ") == 0)
            {
                return "<p style='text-indent:2em;'>" + temp + "</p>\n" + ReturnStr3(content.Replace(temp + "\n", ""));
            }
            else
            {
                return "<p>" + temp + "</p>\n" + ReturnStr3(content.Replace(temp + "\n", ""));
            }
        }

        #region 删除图片
        //删除图片
        //public ActionResult DeleteImg(int id)
        //{
        //    try
        //    {
        //        return C_AttachmentBLL.SingleModel.Delete(id) > 0 ? Json(new { Success = true, Msg = "删除成功" }, JsonRequestBehavior.AllowGet)
        //             : Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        #endregion
        #endregion
            
        #region 自定义分享设置

        /// <summary>
        /// 页面分享设置
        /// </summary>
        /// <returns></returns>
        public ActionResult ShareSet(int? appId)
        {
            if (appId == null || appId.Value <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId.Value, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }

            Store store = StoreBLL.SingleModel.GetModelByAId(appId.Value);
            if (store == null) return View("PageError", new Return_Msg() { Msg = "店铺不存在!", code = "500" });


            EntShare model = EntShareBLL.SingleModel.GetModel($"aid={store.Id} and ShareType=1");



            if (model == null)
            {
                model = new EntShare();

            }
            else
            {
                //表示更新
                model.Logo = C_AttachmentBLL.SingleModel.GetListByCache( model.Id , (int)AttachmentItemType.小程序电商版分享店铺Logo);
                model.ADImg = C_AttachmentBLL.SingleModel.GetListByCache( model.Id , (int)AttachmentItemType.小程序电商版分享广告图);

                //店铺Logo
                List<object> LogoList = new List<object>();
                foreach (C_Attachment attachment in model.Logo)
                {
                    LogoList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.LogoList = LogoList;

                //广告图
                List<object> ADImgList = new List<object>();
                foreach (C_Attachment attachment in model.ADImg)
                {
                    ADImgList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.ADImgList = ADImgList;
            }


            ViewBag.appId = appId;
            ViewBag.aid = store.Id;
            return View(model);
        }


        /// <summary>
        /// 页面分享设置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ShareSetting(EntShare share, int? appId, string LogoList = "", string ADImgList = "")
        {
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息异常！" });

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId.Value, dzaccount.Id.ToString());
            if (role == null)
            {
                return Json(new { isok = false, msg = "没有权限！" });
            }

            Store store = StoreBLL.SingleModel.GetModelByAId(appId.Value);
            if (store == null)
                return Json(new { isok = false, msg = "店铺不存在！" });

            if (string.IsNullOrEmpty(share.StoreName) || share.StoreName.Length > 10)
                return Json(new { isok = false, msg = "店铺名称不能为空或者不能大于10个字符！" });

            if (!string.IsNullOrEmpty(share.ADTitle) && share.ADTitle.Length > 20)
                return Json(new { isok = false, msg = "广告语不能大于20个字符！" });

            bool result = false;
            if (share.Id > 0)
            {
                EntShare model = EntShareBLL.SingleModel.GetModel($"aid={store.Id} and ShareType=1");
                if (model == null)
                    return Json(new { isok = false, msg = "数据不存在！" });

                share.Qrcode = model.Qrcode;
                if (string.IsNullOrEmpty(share.Qrcode))
                {
                    XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(role.TId);
                    if (xcxTemplate == null)
                    {
                        return Json(new { isok = false, msg = "小程序模板不存在！" });
                    }

                    string token = "";
                    if (!XcxApiBLL.SingleModel.GetToken(role, ref token))
                    {
                        return Json(new { isok = false, msg = token });
                    }
                    share.Qrcode = CommondHelper.GetQrcode(token, "pages/index/index");
                }
                //表示修改
                result = EntShareBLL.SingleModel.Update(share);
            }
            else
            {
                if (string.IsNullOrEmpty(LogoList))
                    return Json(new { isok = false, msg = "店铺Logo不能为空！" });

                XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(role.TId);
                if (xcxTemplate == null)
                {
                    return Json(new { isok = false, msg = "小程序模板不存在！" });
                }

                string token = "";
                if (!XcxApiBLL.SingleModel.GetToken(role, ref token))
                {
                    return Json(new { isok = false, msg = token });
                }
                share.Qrcode = CommondHelper.GetQrcode(token,"pages/index/index");

                share.ShareType = 1;
                int id = Convert.ToInt32(EntShareBLL.SingleModel.Add(share));
                share.Id = id;
                //表示新增
                result = id > 0;
            }

            if (result)
            {

                #region Logo
                if (!string.IsNullOrWhiteSpace(LogoList))
                {
                    string[] Imgs = LogoList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Imgs.Length > 0)
                    {
                        C_AttachmentBLL.SingleModel.AddImgList(Imgs, (int)AttachmentItemType.小程序电商版分享店铺Logo, share.Id);

                    }
                }
                #endregion

                #region 广告图

                if (!string.IsNullOrEmpty(ADImgList))
                {
                    string[] imgArray = ADImgList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (imgArray.Length > 0)
                    {
                        C_AttachmentBLL.SingleModel.AddImgList(imgArray, (int)AttachmentItemType.小程序电商版分享广告图, share.Id);
                    }
                }
                #endregion

                return Json(new { isok = true, msg = "操作成功！", obj = share.Id });

            }

            return Json(new { isok = false, msg = "操作异常！", obj = share.Id });
        }


        /// <summary>
        /// 获取小程序分二维码
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ActionResult GetQrcode(int appId = 0, int ShareType = 0)
        {
            if (appId <= 0)
                return Json(new { isok = false, msg = "参数错误!" });
            if (dzaccount == null)
                return Json(new { isok = false, msg = "登录信息异常!" });
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
                return Json(new { isok = false, msg = "系统繁忙 role_null!" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={role.TId}");
            if (xcxTemplate == null)
                Json(new { isok = false, msg = "小程序模板不存在" });

            int modeId = role.Id;
            EntShare model = new EntShare();
            switch (xcxTemplate.Type)
            {
                case 22:
                    //专业版
                    ShareType = 0;
                    break;
                case 26:
                    //多门店版本
                    ShareType = 2;
                    break;
                case 6:
                    //电商版
                    ShareType = 1;
                    Store store = StoreBLL.SingleModel.GetModelByAId(appId);
                    if (store == null)
                        return Json(new { isok = false, msg = "店铺不存在！" });
                    modeId = store.Id;
                    break;
            }


            model = EntShareBLL.SingleModel.GetModel($"aid={modeId} and ShareType={ShareType}");


            if (model == null)
                return Json(new { isok = false, msg = "数据不存在请先进行分享配置" });

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(role, ref token))
            {
                return Json(new { isok = false, msg = token });
            }

            string qrcodeImg = CommondHelper.GetQrcode(token,"pages/index/index");

            if (string.IsNullOrEmpty(qrcodeImg))
                return Json(new { isok = false, msg = "获取失败" });

            model.Qrcode = qrcodeImg;
            if (!EntShareBLL.SingleModel.Update(model, "Qrcode"))
                return Json(new { isok = false, msg = "获取异常" });

            return Json(new { isok = true, msg = "获取成功", obj = qrcodeImg });

        }
        
        #endregion

        #region 打印机管理
        /// <summary>
        /// 打印机管理列表
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult PrintList(int appid)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appid, dzaccount.Id.ToString());
            if (app == null)
            {
                return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
            }
            Store Store = StoreBLL.SingleModel.GetModelByAId(appid);
            if (Store == null)
            {
                return Json(new { isok = false, msg = "系统繁忙Storenull！" }, JsonRequestBehavior.AllowGet);
            }
            List<FoodPrints> PrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {Store.Id} and accountId = '{dzaccount.OpenId}' and state >= 0 ") ?? new List<FoodPrints>();
            ViewBag.appId = appid;
            ViewBag.FoodId = Store.Id;
            return View(PrintList);
        }

        /// <summary>
        /// 查看打印机终端
        /// </summary>
        /// <param name="print"></param>
        /// <returns></returns>
        public ActionResult getPrintForm(int appId, int printId, int edit = 0)
        {
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_error！" }, JsonRequestBehavior.AllowGet);
            }
            Store Store = StoreBLL.SingleModel.GetModelByAId(appId);
            if (Store == null)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_null！" }, JsonRequestBehavior.AllowGet);
            }
            FoodPrints print = FoodPrintsBLL.SingleModel.GetModel(printId) ?? new Entity.MiniApp.Fds.FoodPrints();
            //先访问易连云接口添加,成功后才在系统内添加记录

            ViewBag.edit = edit;
            return PartialView("_PartialPrintItem", print);
        }

        /// <summary>
        /// 添加打印机终端
        /// </summary>
        /// <param name="print"></param>
        /// <returns></returns>
        public ActionResult addPrint(Entity.MiniApp.Fds.FoodPrints print)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
            }
            if (print.FoodStoreId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_error！" }, JsonRequestBehavior.AllowGet);
            }
            Store Store = StoreBLL.SingleModel.GetModel(print.FoodStoreId);
            if (Store == null)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_null！" }, JsonRequestBehavior.AllowGet);
            }
            //先访问易连云接口添加,成功后才在系统内添加记录
            PrintErrorData returnMsg = FoodYiLianYunPrintHelper.addPrinter(print.APIKey, print.UserId, print.PrintNo, print.PrintKey, print.Telphone, print.UserName, print.Name);
            if (returnMsg.errno != 1)
            {
                return Json(new { isok = false, msg = returnMsg.error }, JsonRequestBehavior.AllowGet);
            }
            print.industrytype = 2;
            print.accountId = dzaccount.OpenId;
            print.State = 0;
            print.CreateDate = DateTime.Now;
            int id = Convert.ToInt32(FoodPrintsBLL.SingleModel.Add(print));
            if (id > 0)
            {
                return Json(new { isok = true, msg = "添加成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false, msg = "添加失败！" }, JsonRequestBehavior.AllowGet);
        }

        ///// <summary>
        ///// 删除打印机
        ///// </summary>
        ///// <param name="appid"></param>
        ///// <param name="printId"></param>
        ///// <returns></returns>
        public ActionResult deletePrint(int appid, int printId = 0)
        {
            Store Store = StoreBLL.SingleModel.GetModelByAId(appid);
            if (Store == null)
            {
                return Json(new { isok = false, msg = "系统繁忙Id_null！" }, JsonRequestBehavior.AllowGet);
            }
            FoodPrints print = FoodPrintsBLL.SingleModel.GetModel(printId);
            if (print == null)
            {
                Json(new { isok = false, msg = "系统繁忙printModel_null！" }, JsonRequestBehavior.AllowGet);
            }
            //先访问易连云接口删除,成功后才在系统内操作记录
            string returnMsg = FoodYiLianYunPrintHelper.deletePrinter(print.APIKey, print.UserId, print.PrintNo, print.PrintKey);
            if (Convert.ToInt32(returnMsg) == 4)
            {
                return Json(new { isok = false, msg = "删除失败!！" }, JsonRequestBehavior.AllowGet);
            }
            print.State = -1;
            print.UpdateDate = DateTime.Now;
            bool result = FoodPrintsBLL.SingleModel.Update(print, "State,UpdateDate");
            if (result)
            {
                return Json(new { isok = true, msg = "删除成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = false, msg = "删除失败！" }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region 储值
        ///// <summary>
        ///// 获取储值项目资料
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult MiniAppSaveMoneySetManager(int appId)
        //{
        //    if (dzaccount == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
        //    }
        //    var app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
        //    if (app == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
        //    }
        //    ViewBag.appId = appId;

        //    //object dataObj = new
        //    //{
        //    //    list = _miniAppSaveMoneySetBLL.getListByAppId(app.Id, pageIndex, pageSize),
        //    //    recordCount = _miniAppSaveMoneySetBLL.getCountByAppId(app.Id, pageIndex, pageSize)
        //    //};

        //    return View();
        //}



        ///// <summary>
        ///// 储值项目管理
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <returns></returns>
        ////[HttpGet]
        //public ActionResult getMiniAppSaveMoneySet(int appId, int State = -999, int pageIndex = 1, int pageSize = 6)
        //{
        //    if (dzaccount == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
        //    }
        //    var app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
        //    if (app == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
        //    }
        //    ViewBag.appId = appId;

        //    object dataObj = new
        //    {
        //        list = _miniAppSaveMoneySetBLL.getListByAppId(app.Id, State, pageIndex, pageSize),
        //        recordCount = _miniAppSaveMoneySetBLL.getCountByAppId(app.Id, State, pageIndex, pageSize)
        //    };

        //    return Json(new { isok = true, dataObj = dataObj }, JsonRequestBehavior.AllowGet);
        //}

        /////// <summary>
        /////// 储值项目管理
        /////// </summary>
        /////// <param name="appId"></param>
        /////// <returns></returns>
        ////[HttpGet]
        ////public ActionResult AddMiniAppSaveMoneySet(int appId,int State = -999)
        ////{
        ////    if (dzaccount == null)
        ////    {
        ////        return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
        ////    }
        ////    var app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
        ////    if (app == null)
        ////    {
        ////        return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
        ////    }
        ////    ViewBag.appId = appId;
        ////    var List = _miniAppSaveMoneySetBLL.getListByAppId(app.Id);

        ////    return View(List);
        ////}


        ///// <summary>
        ///// 添加储值项目
        ///// </summary>
        ///// <param name="CityInfoId"></param>
        ///// <param name="goodtypename"></param>
        ///// <param name="StoreId"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult AddMiniAppSaveMoneySet(int appId, MiniAppSaveMoneySet model)
        //{
        //    var app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
        //    if (app == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
        //    }

        //    if (dzaccount == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
        //    }

        //    if (model.JoinMoney <= 0 || model.JoinMoney > 9999999)
        //    {
        //        return Json(new { isok = false, msg = "充值金额请设定0.01 ~ 99999.99！" }, JsonRequestBehavior.AllowGet);
        //    }

        //    if (model.GiveMoney <= 0 || model.GiveMoney > 9999999)
        //    {
        //        return Json(new { isok = false, msg = "赠送金额请设定0.01 ~ 99999.99！" }, JsonRequestBehavior.AllowGet);
        //    }
        //    //int resultInt = 0;//
        //    var List = _miniAppSaveMoneySetBLL.getListByAppId(app.Id);
        //    if (model.Id == 0)//添加
        //    {
        //        if (List.Count >= 20)
        //        {
        //            return Json(new { isok = false, msg = "目前最多能添加20个储值项目" });
        //        }
        //        if (List.Any(m => m.JoinMoney == model.JoinMoney && m.GiveMoney == model.GiveMoney))
        //        {
        //            return Json(new { isok = false, msg = "已存在相同储值项目 , 请重新添加" });
        //        }

        //        model.SetName = $"充{model.JoinMoneyStr}送{model.GiveMoneyStr}";
        //        model.AmountMoney = model.JoinMoney + model.GiveMoney;
        //        model.CreateDate = DateTime.Now;
        //        model.State = 1;
        //        var result = _miniAppSaveMoneySetBLL.Add(model);
        //        if (int.Parse(result.ToString()) > 0)
        //        {
        //            return Json(new { isok = true, msg = "添加成功!", newid = result }, JsonRequestBehavior.AllowGet);
        //        }
        //    }


        //    return Json(new { isok = false, msg = "系统错误！" });
        //}


        ///// <summary>
        ///// 上下架/删除 储值项目
        ///// </summary>
        ///// <param name="CityInfoId"></param>
        ///// <param name="goodtypename"></param>
        ///// <param name="StoreId"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult updateMiniAppSaveMoneySetState(int appId, int saveMoneySetId, int State)
        //{
        //    var app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
        //    if (app == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
        //    }

        //    if (dzaccount == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
        //    }
        //    var saveMoneySet = _miniAppSaveMoneySetBLL.GetModel(saveMoneySetId);
        //    if (saveMoneySet == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙saveMoney_null！" }, JsonRequestBehavior.AllowGet);
        //    }
        //    saveMoneySet.State = State;
        //    if (!_miniAppSaveMoneySetBLL.Update(saveMoneySet, "State"))
        //    {
        //        return Json(new { isok = false, msg = "状态更新失败！" });
        //    }

        //    return Json(new { isok = true, msg = "状态更新成功！" });
        //}

        ///// <summary>
        ///// 是否可以添加储值项目
        ///// </summary>
        ///// <param name="CityInfoId"></param>
        ///// <param name="goodtypename"></param>
        ///// <param name="StoreId"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult GetMiniAppSaveMoneySetCanAdd(int appId)
        //{
        //    //int resultInt = 0;//
        //    var count = _miniAppSaveMoneySetBLL.getCountByAppId(appId);

        //    if (count >= 20)
        //    {
        //        return Json(new { isok = false, msg = "无法新增储值项目！您已添加了20个储值项目，已达到上限，请编辑已有的储值项目或删除部分储值项目后再进行新增。" });
        //    }

        //    return Json(new { isok = true, msg = "可以添加！" });
        //}


        ///// <summary>
        ///// 储值项目管理
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <returns></returns>
        ////[HttpGet]
        //public ActionResult getMiniAppSaveMoneySetUserLog(int appId, DateTime? changeStartDate, DateTime? changeEndDate, int State = -999, int pageIndex = 1, int pageSize = 6, string nickName = "", int changeType = 0, string changeNote = "")
        //{
        //    if (dzaccount == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
        //    }
        //    var app = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
        //    if (app == null)
        //    {
        //        return Json(new { isok = false, msg = "系统繁忙null！" }, JsonRequestBehavior.AllowGet);
        //    }

        //    MiniAppSaveMoneySetUserLogViewBLL userLogViewBLL = new MiniAppSaveMoneySetUserLogViewBLL();

        //    ViewBag.appId = appId;

        //    object dataObj = new
        //    {
        //        list = userLogViewBLL.getListByCondition(app.Id, changeStartDate, changeEndDate, nickName, changeType, changeNote, pageIndex, pageSize),
        //        recordCount = userLogViewBLL.getCountByCondition(app.Id, changeStartDate, changeEndDate, nickName, changeType, changeNote, pageIndex, pageSize)
        //    };

        //    return Json(new { isok = true, dataObj = dataObj }, JsonRequestBehavior.AllowGet);
        //}

        #endregion

        #region 会员管理
            /// <summary>
            /// 
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="versionType">版本0 电商版 1专业版</param>
            /// <returns></returns>
        public ActionResult VipSetting(int appId,int versionType=0)
        {
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            ViewBag.appId = appId;
            ViewBag.versionType = versionType;
            int modelId = 0;
            ViewBag.PageType = 6;
            if (versionType > 0)
            {
                //表示专业版
                ViewBag.PageType = 22;
                EntSetting ent = EntSettingBLL.SingleModel.GetModel(appId);
                if (ent == null)
                    return View("PageError", new Return_Msg() { Msg = "找不到该专业版!", code = "500" });
                modelId = ent.aid;
            }
            else
            {
                //表示电商版
                Store store = StoreBLL.SingleModel.GetModelByAId(appId);
                if (store == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "店铺不存在!", code = "500" });
                }
                modelId = store.Id;
            }

            
            VipViewModel model = new VipViewModel();
            model.config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (model.config == null)
            {
                model.config = new VipConfig();
                model.config.addtime = model.config.updatetime = DateTime.Now;
                model.config.appId = xcx.AppId;
                VipConfigBLL.SingleModel.Add(model.config);
            }
            model.levelList = VipLevelBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            if (model.levelList == null || model.levelList.Count <= 0)
            {
                VipLevel def_level = new VipLevel();
                def_level.addtime = DateTime.Now;
                def_level.appId = xcx.AppId;
                def_level.name = "普通会员";
                def_level.bgcolor = "#4a86e8";
                def_level.updatetime = def_level.addtime;
                model.levelList = new List<VipLevel>();
                def_level.Id = Convert.ToInt32(VipLevelBLL.SingleModel.Add(def_level));
                model.levelList.Add(def_level);
            }
            else
            {
                //获取部分折扣类型选择的产品列表
                foreach (VipLevel info in model.levelList)
                {
                    if (info.type == 2 && !string.IsNullOrEmpty(info.gids))
                    {

                        if (versionType > 0)
                        {
                            List<EntGoods> goodslist = EntGoodsBLL.SingleModel.GetList($"id in ({info.gids})");
                            List<StoreGoods> listGoods = new List<StoreGoods>();
                            foreach (EntGoods item in goodslist)
                            {
                                listGoods.Add(new StoreGoods()
                                {
                                    Id=item.id,
                                    GoodsName=item.name,
                                    ImgUrl=item.img

                                });
                            }
                            info.goodslist = listGoods;
                        }
                        else
                        {
                            info.goodslist = StoreGoodsBLL.SingleModel.GetList($"id in ({info.gids})");
                        }
                    }
                }
            }
            VipLevel dflevel = model.levelList.Where(l => l.level == 0).FirstOrDefault();
            model.ruleList = VipRuleBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            if (model.ruleList == null || model.ruleList.Count <= 0)
            {
                VipRule rule = new VipRule();
                rule.addtime = rule.updatetime = DateTime.Now;
                rule.appId = xcx.AppId;
                rule.minMoney = 0;
                rule.maxMoney = 0;
                rule.levelid = dflevel.Id;
                rule.id = Convert.ToInt32(VipRuleBLL.SingleModel.Add(rule));
                //rule.levelinfo = dflevel;
                model.ruleList.Add(rule);
            }
            
            int isAuth = 0;
            int isAuthCard = 0;
            int haveCard = 0;
            string SerName = string.Empty;
            string cardStaus = string.Empty;

            VipWxCard card = new VipWxCard();
            if (versionType > 0)
            {
                //Type=2 表示专业版的微信会员卡
                card= VipWxCardBLL.SingleModel.GetModel($"Type=2 and AppId={modelId}");
            }
            else
            {
                card= VipWxCardBLL.SingleModel.GetModel($"Type=0 and AppId={modelId}");
            }

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcx, ref token))
            {
                return Json(new { isok = false, msg = token }, JsonRequestBehavior.AllowGet);
            }
            if (card != null)
            {
                isAuth = 1;
                isAuthCard = 1;
                haveCard = 1;
                string cardResult = Utility.IO.Context.PostData($"https://api.weixin.qq.com/card/get?access_token={token}", JsonConvert.SerializeObject(new { card_id = card.CardId }));
                if (cardResult.Contains("CARD_STATUS_VERIFY_FAIL"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_VERIFY_FAIL;
                }
                if (cardResult.Contains("CARD_STATUS_VERIFY_OK"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_VERIFY_OK;
                }
                if (cardResult.Contains("CARD_STATUS_NOT_VERIFY"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_NOT_VERIFY;
                }
                if (cardResult.Contains("CARD_STATUS_DELETE1"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_DELETE1;
                }
                if (cardResult.Contains("CARD_STATUS_DISPATCH"))
                {
                    card.Status = (int)WxVipCardStatus.CARD_STATUS_DISPATCH;
                }

                switch (card.Status)
                {
                    case -1:
                        cardStaus = "已删除";
                        break;
                    case 0:
                        cardStaus = "待审核";
                        break;
                    case 1:
                        cardStaus = "审核失败";
                        break;
                    case 2:
                        cardStaus = "通过审核";
                        break;
                    case 3:
                        cardStaus = "已投放";
                        break;
                    default:
                        cardStaus = "待审核";
                        break;
                }

                card.UpdateTime = DateTime.Now;
                VipWxCardBLL.SingleModel.Update(card, "UpdateTime,Status");

                if (card.Status == 2)
                    cardStaus = "已同步到微信卡包";

                SerName = card.SerName;
            }
            else
            {
               
                OpenAuthorizerConfig umodel = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), appId, 4).FirstOrDefault();
                if (umodel != null)
                {
                    isAuth = 1;
                    isAuthCard = 1;
                    SerName = umodel.nick_name;
                }
            }

            string returnurl =AsUrlData($"{WebSiteConfig.XcxAppReturnUrl}/stores/VipSetting?appId={appId}");
            if (versionType > 0)
            {
                returnurl= AsUrlData($"{WebSiteConfig.XcxAppReturnUrl}/stores/VipSetting?appId={appId}&versionType=1");
            }
            if (card != null)
            {
                ViewBag.AuthUrl =
              $"{WebSiteConfig.GoToGetAuthoUrl}index?userId={dzaccount.Id}&newtype=4&rid={xcx.Id}&user_name={card.User_Name}&returnurl={returnurl}";

            }
            else
            {
                ViewBag.AuthUrl =
              $"h{WebSiteConfig.GoToGetAuthoUrl}index?userId={dzaccount.Id}&newtype=4&rid={xcx.Id}&returnurl={returnurl}";

            }


            ViewBag.IsAuth = isAuth;
            ViewBag.IsAuthCard = isAuthCard;
            ViewBag.HaveCard = haveCard;
            ViewBag.CardStaus = cardStaus;
            ViewBag.SerName = SerName;
            ViewBag.token = token;
            //string ak = WxHelper.GetToken("wx64f161aa79a6801b", "65d0158981a05224eab3d690c36f035e", false);
            //if (string.IsNullOrEmpty(ak))
            //{
            //    ak = WxHelper.GetToken("wx64f161aa79a6801b", "65d0158981a05224eab3d690c36f035e", true);
            //}
            ViewBag.ak = "ak";
            return View(model);
        }
        private string AsUrlData(string data)
        {
            return Uri.EscapeDataString(data);
        }

        /// <summary>
        /// 添加编辑会员级别
        /// </summary>
        public ActionResult SavelevelInfo()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            int id = Utility.IO.Context.GetRequestInt("id", -1);
            if (id < 0)
            {
                return Json(new { isok = false, msg = "参数错误id_error" + id }, JsonRequestBehavior.AllowGet);
            }
            string name = Utility.IO.Context.GetRequest("name", string.Empty);
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { isok = false, msg = "请填写级别名称" }, JsonRequestBehavior.AllowGet);
            }
            if (name.Length > 5)
            {
                return Json(new { isok = false, msg = "级别名称长度不能超过5个字" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel model = VipLevelBLL.SingleModel.GetModel($" name='{name}' and state>=0 and appId='{xcx.AppId}'");
            if (id <= 0 && model != null)
            {
                return Json(new { isok = false, msg = "该级别名称已存在" }, JsonRequestBehavior.AllowGet);
            }
            int type = Utility.IO.Context.GetRequestInt("type", -1);
            if (type < 0)
            {
                return Json(new { isok = false, msg = "参数错误type_error" }, JsonRequestBehavior.AllowGet);
            }
            string gids = Utility.IO.Context.GetRequest("gids", string.Empty);
            if (type == 2 && string.IsNullOrEmpty(gids))
            {
                return Json(new { isok = false, msg = "请选择折扣商品" }, JsonRequestBehavior.AllowGet);
            }
            int discount = Utility.IO.Context.GetRequestInt("discount", 0);
            if (type != 0) //type:0-无折扣 1-全场折扣 2-部分折扣
            {
                if (discount <= 0)
                {
                    return Json(new { isok = false, msg = "请填写商品折扣" }, JsonRequestBehavior.AllowGet);
                }
                if (discount < 0 || discount >= 100)
                {
                    return Json(new { isok = false, msg = "请填写0~10之间的数字，最多保留一位小数" }, JsonRequestBehavior.AllowGet);
                }
            }
            string bgcolor = Utility.IO.Context.GetRequest("bgcolor", string.Empty);
            if (string.IsNullOrEmpty(bgcolor))
            {
                return Json(new { isok = false, msg = "请选择会员封面" }, JsonRequestBehavior.AllowGet);
            }
            if (id > 0)
            {
                model = VipLevelBLL.SingleModel.GetModel($"id={id} and appId='{xcx.AppId}' and state>=0");
                if (model == null)
                {
                    return Json(new { isok = false, msg = "数据异常" }, JsonRequestBehavior.AllowGet);
                }
                model.name = name;
                model.gids = gids;
                model.type = type;
                model.updatetime = DateTime.Now;
                model.bgcolor = bgcolor;
                if (type != 0)
                {
                    model.discount = discount;
                }
                if (type == 2 && !string.IsNullOrEmpty(gids))
                {
                    model.goodslist = StoreGoodsBLL.SingleModel.GetList($"id in ({gids})");
                }
                bool isok = VipLevelBLL.SingleModel.Update(model, "name,gids,type,bgcolor,updatetime,discount");
                string msg = isok ? "保存成功" : "保存失败";
                return Json(new { isok = isok, msg = msg, model = model }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                model = new VipLevel();
                model.name = name;
                model.gids = gids;
                model.type = type;
                model.addtime = model.updatetime = DateTime.Now;
                model.level = 1;
                model.appId = xcx.AppId;
                model.bgcolor = bgcolor;
                if (type != 0)
                {
                    model.discount = discount;
                }
                if (type == 2 && !string.IsNullOrEmpty(gids))
                {
                    model.goodslist = StoreGoodsBLL.SingleModel.GetList($"id in ({gids})");
                }
                model.Id = Convert.ToInt32(VipLevelBLL.SingleModel.Add(model));
                bool isok = model.Id > 0;
                string msg = isok ? "保存成功" : "保存失败";
                return Json(new { isok = isok, msg = msg, model = model }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 删除会员等级时验证该等级下还有多少会员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult validviplist(int id)
        {
            if (id <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            int count = VipRelationBLL.SingleModel.GetCount($"levelid={id} and state>=0");
            string msg = string.Empty;
            if (count > 0)
            {
                return Json(new { isok = false, msg = $"该会员等级下还有{count}个会员" }, JsonRequestBehavior.AllowGet);
            }
            count = VipRuleBLL.SingleModel.GetCount($"levelid={id} and state>=0");
            if (count > 0)
            {
                return Json(new { isok = false, msg = $"自动升级规则设定中还有此会员等级，请先对升级规则做出修改" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = true }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 删除会员等级
        /// </summary>
        /// <returns></returns>
        public ActionResult delLevel()
        {
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (id <= 0 || appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={id} and appid='{xcx.AppId}' and state>=0");
            if (levelinfo == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }
            levelinfo.state = -1;
            levelinfo.updatetime = DateTime.Now;
            bool isok = VipLevelBLL.SingleModel.Update(levelinfo, "state,updatetime");
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 开启\关闭会员自动升级
        /// </summary>
        /// <returns></returns>
        public ActionResult saveConfig()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int autoswitch = Utility.IO.Context.GetRequestInt("switch", -1);
            if (appId <= 0 || autoswitch < 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            VipConfig config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (config == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }
            config.autoupdate = autoswitch;
            config.updatetime = DateTime.Now;
            bool isok = VipConfigBLL.SingleModel.Update(config, "autoupdate,updatetime");
            string msg = config.autoswitch ? "自动升级已开启" : "自动升级已关闭";
            if (!isok)
            {
                msg = "操作失败";
            }
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 开启\关闭会员卡自动同步 创建会员卡卡套
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveSyncCard()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);

            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
            }

            VipConfig config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (config == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={store.Id}");
            if (_vipWxCard != null)
            {
                return Json(new { isok = true, msg = "操作成功", obj = _vipWxCard }, JsonRequestBehavior.AllowGet);
            }
            _vipWxCard = new VipWxCard();

            OpenAuthorizerConfig umodel = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), appId, 4).FirstOrDefault();
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "操作失败(请先绑定认证服务号或者申请代制)" }, JsonRequestBehavior.AllowGet);
            }


            OpenAuthorizerConfig app = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(dzaccount.Id.ToString(), appId).FirstOrDefault();
            if (app == null)
            {
                return Json(new { isok = false, msg = "请先到小程序管理绑定小程序" }, JsonRequestBehavior.AllowGet);
            }

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcx, ref token))
            {
                return Json(new { isok = false, msg = token }, JsonRequestBehavior.AllowGet);
            }
            if(string.IsNullOrEmpty(store.logo)|| string.IsNullOrEmpty(store.name))
            {
                return Json(new { isok = false, msg = "操作失败(请先配置店铺信息Logo以及店铺名称)"}, JsonRequestBehavior.AllowGet);
            }
            string uploadImgResult = CommondHelper.WxUploadImg(token, store.logo);
            if (!uploadImgResult.Contains("url"))
            {
                return Json(new { isok = false, msg = "操作失败", obj = uploadImgResult }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                WxUploadImgResult r = JsonConvert.DeserializeObject<WxUploadImgResult>(uploadImgResult);
                CreateCardResult _createCardResult = CommondHelper.AddVipWxCard(r.url, store.name, store.name + "会员卡", app.user_name, token);

                if (_createCardResult.errcode != 0)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), $"errorCode:{_createCardResult.errcode},errmsg:{_createCardResult.errmsg}");
                    return Json(new { isok = false, msg = "操作失败", obj = _createCardResult }, JsonRequestBehavior.AllowGet);
                }

                _vipWxCard.CardId = _createCardResult.card_id;
                _vipWxCard.AppId = store.Id;
                _vipWxCard.Type = 0;
                _vipWxCard.User_Name = umodel.user_name;
                _vipWxCard.SerName = umodel.nick_name;
                _vipWxCard.Status = (int)WxVipCardStatus.CARD_STATUS_NOT_VERIFY;
                _vipWxCard.AddTime = DateTime.Now;
                int VipWxCardId = Convert.ToInt32(VipWxCardBLL.SingleModel.Add(_vipWxCard));

                return Json(new { isok = true, msg = VipWxCardId > 0 ? "操作成功" : "操作失败", CreateCardResult = _createCardResult, VipWxCardId = VipWxCardId, authorizer_access_token = token }, JsonRequestBehavior.AllowGet);

            }
        }


        /// <summary>
        /// 更新微信会员卡
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateWxCard()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);

            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(appId);
            if (store == null)
            {
                return Json(new { isok = false, msg = "店铺不存在" }, JsonRequestBehavior.AllowGet);
            }

            VipConfig config = VipConfigBLL.SingleModel.GetModel($"appid='{xcx.AppId}' and state>=0");
            if (config == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(store.logo) || string.IsNullOrEmpty(store.name))
            {
                return Json(new { isok = false, msg = "操作失败(请先配置店铺信息Logo以及店铺名称)" }, JsonRequestBehavior.AllowGet);
            }

            VipWxCard _vipWxCard = VipWxCardBLL.SingleModel.GetModel($"AppId={store.Id}");
            if (_vipWxCard == null)
                return Json(new { isok = false, msg = "非法操作(请先创建卡套)" }, JsonRequestBehavior.AllowGet);
            var updateCard = new
            {
                card_id = _vipWxCard.CardId,
                member_card = new
                {
                    base_info = new
                    {
                        title = store.name + "会员卡",
                        logo_url = store.logo
                    }
                }

            };

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcx, ref token))
            {
                return Json(new { isok = false, msg = token }, JsonRequestBehavior.AllowGet);
            }
            string updateCardJson = JsonConvert.SerializeObject(updateCard);
            string updateResult = Utility.IO.Context.PostData($"https://api.weixin.qq.com/card/update?access_token={token}", updateCardJson);

            UpdateWxCard _updateWxCard = JsonConvert.DeserializeObject<UpdateWxCard>(updateResult);
            if (_updateWxCard.errcode == 0)
                return Json(new { isok = true, msg = "更新成功", obj = _updateWxCard }, JsonRequestBehavior.AllowGet);
            return Json(new { isok = false, msg = "更新失败", obj = _updateWxCard }, JsonRequestBehavior.AllowGet);

        }





        /// <summary>
        /// 删除规则
        /// </summary>
        /// <returns></returns>
        public ActionResult delRule()
        {
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            int appid = Utility.IO.Context.GetRequestInt("appId", 0);
            if (id <= 0 || appid <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appid, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            VipRule rule = VipRuleBLL.SingleModel.GetModel($"id={id} and appid='{xcx.AppId}' and state>=0");
            if (rule == null)
            {
                return Json(new { isok = false, msg = "非法操作" }, JsonRequestBehavior.AllowGet);
            }
            rule.state = -1;
            rule.updatetime = DateTime.Now;
            bool isok = VipRuleBLL.SingleModel.Update(rule, "state,updatetime");
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 保存规则
        /// </summary>
        /// <returns></returns>
        public ActionResult saveRuleList()
        {
            string ruleliststr = Utility.IO.Context.GetRequest("rulelist", string.Empty);
            int appid = Utility.IO.Context.GetRequestInt("appId", 0);
            if (string.IsNullOrEmpty(ruleliststr) || appid <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appid, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            List<VipRule> ruleList = null;
            try
            {
                ruleList = JsonConvert.DeserializeObject<List<VipRule>>(ruleliststr);
            }
            catch
            {
                return Json(new { isok = false, msg = "参数错误！" }, JsonRequestBehavior.AllowGet);
            }
            if (ruleList == null || ruleList.Count <= 0)
            {
                return Json(new { isok = false, msg = "参数错误error" }, JsonRequestBehavior.AllowGet);
            }
            if (ruleList.Where(r => r.appId != xcx.AppId).ToList().Count > 0)
            {
                return Json(new { isok = false, msg = "数据异常" }, JsonRequestBehavior.AllowGet);
            }
            foreach (VipRule rule in ruleList)
            {
                if (rule.minMoney > rule.maxMoney)
                {
                    return Json(new { isok = false, msg = "规则设置消费金额范围错误" }, JsonRequestBehavior.AllowGet);
                }
                int count = ruleList.Where(r => r.levelid == rule.levelid).ToList().Count;
                if (count > 1)
                {
                    return Json(new { isok = false, msg = "有多条规则包含同一会员级别" }, JsonRequestBehavior.AllowGet);
                }
            }
            bool isok = VipRuleBLL.SingleModel.saveRuleList(ruleList);
            string msg = isok ? "保存成功" : "保存失败";
            if (isok)
            {
                ruleList = VipRuleBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            }
            return Json(new { isok = isok, msg = msg, ruleList = ruleList }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 会员列表
        /// </summary>
        /// <returns></returns>
        public ActionResult VipList()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            ViewBag.appId = appId;
            List<VipLevel> levelList = VipLevelBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            //log4net.LogHelper.WriteInfo(this.GetType(), $"appid='{xcx.AppId}' and state>=0");
            if (levelList == null) levelList = new List<VipLevel>();
            return View(levelList);
        }

        /// <summary>
        /// 获取会员列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetVipList()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            string username = Utility.IO.Context.GetRequest("username", string.Empty);
            int levelid = Utility.IO.Context.GetRequestInt("levelid", 0);
            int leveltype = Utility.IO.Context.GetRequestInt("leveltype", -1);
            string startDate = Utility.IO.Context.GetRequest("startDate", string.Empty);
            string endDate = Utility.IO.Context.GetRequest("endDate", string.Empty);
            string telePhone = Utility.IO.Context.GetRequest("telePhone", string.Empty);
            try
            {
                MiniappVipInfo model = VipRelationBLL.SingleModel.GetVipList(xcx.AppId, pageIndex, pageSize, username, levelid, leveltype, startDate, endDate, telePhone);
                return Json(new { isok = true, model = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 删除会员信息
        /// </summary>
        /// <returns></returns>
        public ActionResult DelVipInfo()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            int vid = Utility.IO.Context.GetRequestInt("vid", 0);
            if (vid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙vid_null" }, JsonRequestBehavior.AllowGet);
            }
            VipRelation viprelation = VipRelationBLL.SingleModel.GetModel($"id={vid} and state>=0 and appid='{xcx.AppId}'");
            if (viprelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙data_null" }, JsonRequestBehavior.AllowGet);
            }
            bool isok = VipRelationBLL.SingleModel.DelModel(viprelation);
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult saveEdit()
        {
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcx == null)
            {
                return Json(new { isok = false, msg = "系统繁忙app_null" }, JsonRequestBehavior.AllowGet);
            }
            int levelid = Utility.IO.Context.GetRequestInt("levelid", 0);
            if (levelid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙levelid_null" }, JsonRequestBehavior.AllowGet);
            }
            int vid = Utility.IO.Context.GetRequestInt("vid", 0);
            if (vid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙vid_null" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel levelinfo = VipLevelBLL.SingleModel.GetModel($"id={levelid} and appid='{xcx.AppId}' and state>=0");
            VipRelation viprelation = VipRelationBLL.SingleModel.GetModel($"id={vid} and appid='{xcx.AppId}' and state>=0");
            if (levelinfo == null || viprelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙data_null" }, JsonRequestBehavior.AllowGet);
            }
            viprelation.levelid = levelinfo.Id;
            viprelation.updatetime = DateTime.Now;
            bool isok = VipRelationBLL.SingleModel.Update(viprelation, "levelid,updatetime");
            string msg = isok ? "修改成功" : "修改失败";
            return Json(new { isok = isok, msg = msg, levelinfo = levelinfo }, JsonRequestBehavior.AllowGet);
        }



   
        #endregion
    }
}
