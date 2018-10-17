using BLL.MiniApp;
using BLL.MiniApp.cityminiapp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Pin;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Qiye;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using Core.MiniApp.Common;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Pin;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using Entity.MiniApp.Qiye;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Filters;
using Utility;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class MiniAppToolsController : toolsController
    {

    }
    public class toolsController : baseController
    {
        private Return_Msg result;


        /// <summary>
        /// 实例化对象
        /// </summary>
        public toolsController()
        {


        }

        /// <summary>
        /// 权限表Id 以及小程序模板类型PageType
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="PageType"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult Bargain(int appId = 0, int PageType = 0, int pageIndex = 1, int pageSize = 10)
        {

            if (appId <= 0 || PageType <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            int StoreId = StoreBLL.SingleModel.GetModelByRid(appId).Id;

            string strWhere = $"StoreId={StoreId}";
            string orderWhere = "IsEnd asc,CreateDate desc";
            List<Bargain> list = BargainBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", orderWhere);
            ViewBag.pageSize = pageSize;
            ViewBag.TotalCount = BargainBLL.SingleModel.GetCount(strWhere);
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            ViewBag.StoreId = StoreId;

            //砍价状态
            ToolsConfig BargainConfig = ToolsConfigBLL.SingleModel.GetModel($"StoreId={StoreId}");
            if (BargainConfig == null)
            {
                ToolsConfigBLL.SingleModel.Add(new ToolsConfig() { StoreId = StoreId, IsBargainOpen = 0 });
                ViewBag.IsBargainOpen = 0;
            }
            else
            {
                ViewBag.IsBargainOpen = BargainConfig.IsBargainOpen;
            }
            return View(list == null ? new List<Bargain>() : list);
        }

        [RouteAuthCheck]
        public ActionResult AddOrEditBargain(int appId = 0, int PageType = 0, int Id = 0, int BargainType = 0)
        {
            if (appId <= 0 || PageType <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            }
            int StoreId = 0;
            if (BargainType > 0)
            {
                EntSetting ent = EntSettingBLL.SingleModel.GetModel(appId);
                if (ent == null)
                    return View("PageError", new Return_Msg() { Msg = "找不到该专业版!", code = "403" });
                StoreId = ent.aid;
            }
            else
            {
                Store store = StoreBLL.SingleModel.GetModelByRid(appId);
                if (store == null)
                    return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "403" });
                StoreId = store.Id;
            }

            ViewBag.VoiceId = 0;
            ViewBag.Attachmentvoicepath = "";
            ViewBag.Vid = 0;
            ViewBag.convertFilePath = "";
            ViewBag.videoPosterPath = "";
            ViewBag.BargainType = BargainType;

            Bargain model = new Bargain();
            if (Id > 0)
            {
                //表示新增
                model = BargainBLL.SingleModel.GetModel(Id);
                if (model == null)
                    return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "404" });

                model.ImgList = C_AttachmentBLL.SingleModel.GetListByCache(Id, (int)AttachmentItemType.小程序店铺砍价轮播图);
                model.DescImgList = C_AttachmentBLL.SingleModel.GetListByCache(Id, (int)AttachmentItemType.小程序店铺砍价详情图);


                //轮播图 也就是主图
                string CarouselList = "未知";
                foreach (C_Attachment attachment in model.ImgList)
                {
                    //  CarouselList.Add(new { id = attachment.id, url = attachment.filepath });
                    CarouselList = attachment.filepath;
                }
                ViewBag.CarouselList = CarouselList;

                //详情图片
                List<object> DescImgList = new List<object>();
                foreach (C_Attachment attachment in model.DescImgList)
                {
                    DescImgList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.DescImgList = DescImgList;
                ViewBag.DeliveryConfig = DeliveryConfigBLL.SingleModel.GetConfig(model);
            }

            ViewBag.DeliveryTemplate = DeliveryTemplateBLL.SingleModel.GetByAid(appId);
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            ViewBag.StoreId = StoreId;
            return View(model ?? new Bargain());
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddOrUpdateBargain(Bargain bargain, string ImgList, string DescImgList = "", int oldhvid = 0, string videopath = "", string voicepath = "", bool enableTemplate = false, int? templateId = null, float? weight = null)
        {
            int rid = 0;
            if (bargain.BargainType > 0)
            {
                EntSetting ent = EntSettingBLL.SingleModel.GetModel(bargain.StoreId);
                if (ent == null) return View("PageError", new Return_Msg() { Msg = "找不到该专业版!", code = "403" });
                rid = ent.aid;

            }
            else
            {
                Store store = StoreBLL.SingleModel.GetModel(bargain.StoreId);
                if (store == null)
                    return Json(new { code = -1, msg = "找不到店铺！" }, JsonRequestBehavior.AllowGet);
                rid = store.appId;

            }

            if (dzaccount == null)
            {
                return Json(new { code = -1, msg = "登录信息超时！" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(rid, dzaccount.Id.ToString());
            if (role == null)
            {
                return Json(new { code = -1, msg = "没有权限！" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(bargain.BName))
            {
                return Json(new { code = -1, msg = "砍价名称不能为空！" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrEmpty(bargain.BName) && bargain.BName.Length > 40)
            {
                return Json(new { code = -1, msg = "砍价名称不能超过40字符！" }, JsonRequestBehavior.AllowGet);
            }

            if (bargain.CreateNum > 999999 || bargain.CreateNum < 1)
            {
                return Json(new { code = -1, msg = "砍价的生成数量为1-999999！" }, JsonRequestBehavior.AllowGet);
            }


            if (bargain.ReduceMax < bargain.ReduceMin)
            {
                return Json(new { code = -1, msg = "减价最大金额必须大于减价最小金额！" }, JsonRequestBehavior.AllowGet);
            }
            if (bargain.OriginalPrice < bargain.FloorPrice)
            {
                return Json(new { code = -1, msg = "原价金额必须大于底价金额！" }, JsonRequestBehavior.AllowGet);
            }

            if (!string.IsNullOrEmpty(bargain.Description))
            {


                ArrayList DescriptionImglist = FilterHandler.GetImgUrlfromhtml(bargain.Description);
                if (DescriptionImglist.Count > 0)
                {
                    return Json(new { code = -1, msg = "编辑器不能有图片！" }, JsonRequestBehavior.AllowGet);
                }
            }

            bool result = false;




            if (bargain.Id > 0)
            {
                int groupUserNum = BargainUserBLL.SingleModel.GetCount("BId=" + bargain.Id + " and state>0");
                if (groupUserNum > bargain.CreateNum)
                {
                    return Json(new { code = -1, msg = $"当前砍价已售出{groupUserNum}份 , 生成数量不能小于已售出数量！" }, JsonRequestBehavior.AllowGet);
                }
                bargain.RemainNum = bargain.CreateNum - groupUserNum;

                //表示修改

                result = BargainBLL.SingleModel.Update(bargain);
                if (result)
                {
                    BargainUserBLL.SingleModel.BargainUser_UpdateBacth(bargain.StartDate.ToString("yyyy-MM-dd HH:mm"), bargain.EndDate.ToString("yyyy-MM-dd HH:mm"), bargain.BName, bargain.Id);
                }

            }
            else
            {
                bargain.RemainNum = bargain.CreateNum;//新增的时候 生成数量等于剩余数量
                int id = bargain.Id = Convert.ToInt32(BargainBLL.SingleModel.Add(bargain));
                result = id > 0;

                //新增
            }

            if (templateId.HasValue && weight.HasValue)
            {
                result = DeliveryConfigBLL.SingleModel.UpdateConfig(bargain, new DeliveryConfigAttr { Enable = enableTemplate, DeliveryTemplateId = templateId.Value, Weight = (int)(weight.Value * 1000) });
            }

            if (result)
            {
                #region 轮播图也就是主图
                if (!string.IsNullOrWhiteSpace(ImgList))
                {
                    string[] Imgs = ImgList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Imgs.Length > 0)
                    {
                        foreach (string img in Imgs)
                        {
                            //判断上传图片是否以http开头，不然为破图-蔡华兴
                            if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http") == 0)
                            {
                                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                                {
                                    itemId = bargain.Id,
                                    createDate = DateTime.Now,
                                    filepath = img,
                                    itemType = (int)AttachmentItemType.小程序店铺砍价轮播图,
                                    thumbnail = img,
                                    status = 0
                                });
                            }

                        }
                        if (Imgs.Length > 0)
                        {
                            bargain.ImgUrl = Imgs.First();
                            BargainBLL.SingleModel.Update(bargain, "ImgUrl");

                        }
                    }
                }
                #endregion

                #region 详情图

                if (!string.IsNullOrEmpty(DescImgList))
                {
                    string[] imgArray = DescImgList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (imgArray.Length > 0)
                    {
                        foreach (string img in imgArray)
                        {
                            //判断上传图片是否以http开头，不然为破图-蔡华兴
                            if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http://") == 0)
                            {
                                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                                {
                                    itemId = bargain.Id,
                                    createDate = DateTime.Now,
                                    filepath = img,
                                    itemType = (int)AttachmentItemType.小程序店铺砍价详情图,
                                    thumbnail = img,
                                    status = 0
                                });
                            }

                        }
                    }
                }
                #endregion


                return Json(new { code = 1, msg = "操作成功！", obj = bargain.GoodsFreight }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { code = -1, msg = "系统错误！" }, JsonRequestBehavior.AllowGet);


        }


        /// <summary>
        /// 开启或者关闭 是否砍价
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateToolsConfigBargain(int StoreId = 0)
        {
            if (StoreId <= 0)
                return Json(new { code = -1, msg = "参数错误！" }, JsonRequestBehavior.AllowGet);
            Store store = StoreBLL.SingleModel.GetModel(StoreId);
            if (store == null)
                return Json(new { code = -1, msg = "店铺不存在！" }, JsonRequestBehavior.AllowGet);

            ToolsConfig model = ToolsConfigBLL.SingleModel.GetModel($"StoreId={store.Id}");
            if (model == null)
                return Json(new { code = -1, msg = "数据不存在！" }, JsonRequestBehavior.AllowGet);
            model.IsBargainOpen = model.IsBargainOpen > 0 ? 0 : 1;
            if (ToolsConfigBLL.SingleModel.Update(model, "IsBargainOpen"))
                return Json(new { code = 1, msg = "操作成功！" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { code = -1, msg = "操作失败！" }, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 参与砍价记录
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="PageType"></param>
        /// <param name="bid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="export"></param>
        /// <returns></returns>
        public ActionResult BargainUserList(int appId = 0, int PageType = 0, int bid = 0, int pageIndex = 1, int pageSize = 10, bool export = false)
        {
            if (appId <= 0 || PageType <= 0 || bid <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            string strWhere = $"BId={bid}";
            string orderWhere = "CreateDate desc";
            if (export)
            {//导出，查询全部的
                pageIndex = 1;
            }


            List<BargainUser> list = BargainUserBLL.SingleModel.GetList(strWhere, export ? int.MaxValue : pageSize, pageIndex, "*", orderWhere);
            ViewBag.pageSize = pageSize;
            ViewBag.TotalCount = BargainUserBLL.SingleModel.GetCount(strWhere);
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            Bargain Bargain = BargainBLL.SingleModel.GetModel(bid);
            ViewBag.Bargain = Bargain;
            //导出Excel
            if (export)
            {
                if (list != null && list.Count > 0)
                {
                    DataTable table = new DataTable();
                    table.Columns.AddRange(new[]
                    {
                        new DataColumn("报名时间"),
                        new DataColumn("当前价格"),
                        new DataColumn("状态"),
                        new DataColumn("用户名"),
                        new DataColumn("留言备注")
                    });

                    foreach (BargainUser item in list)
                    {
                        DataRow row = table.NewRow();
                        row["报名时间"] = item.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
                        row["当前价格"] = item.CurrentPrice * 0.01;


                        if (item.State == 1)
                        {
                            row["状态"] = "已付款";

                        }
                        else if (item.State == 5)
                        {
                            row["状态"] = "等待付款(已下单)";

                        }
                        else if (item.State == 2)
                        {
                            row["状态"] = "退款中";

                        }
                        else if (item.State == 3)
                        {
                            row["状态"] = "退款成功";

                        }
                        else if (item.State == 3)
                        {
                            row["状态"] = "退款失败";

                        }
                        else if (item.State == -1)
                        {
                            row["状态"] = "已关闭(过期或售罄)";

                        }
                        else if (item.State == 7)
                        {
                            row["状态"] = "待发货";

                        }
                        else if (item.State == 6)
                        {
                            row["状态"] = "等待确认收货";
                        }
                        else if (item.State == 8)
                        {
                            row["状态"] = "交易成功";
                        }
                        else
                        {
                            row["状态"] = "未付款";
                        }





                        row["用户名"] = item.Name;
                        row["留言备注"] = item.Remark;
                        table.Rows.Add(row);
                    }

                    ExcelHelper<C_BargainUser>.Out2Excel(table, $"{Bargain.Id}砍价活动参与记录"); //导出
                }

            }

            return View(list == null ? new List<BargainUser>() : list);
        }


        /// <summary>
        /// 删除或者上架下架操作
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="Id"></param>
        /// <param name="BargainType">0→电商版(默认) 1→餐饮版</param>
        /// <param name="actionType">0→上架或下架(默认操作) 1→删除</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelBargain(int appId = 0, int Id = 0, int BargainType = 0, int actionType = 0)
        {
            if (appId <= 0 || Id <= 0)
                return Json(new { code = -1, msg = "参数错误！" }, JsonRequestBehavior.AllowGet);
            Bargain model = BargainBLL.SingleModel.GetModel(Id);
            if (model == null)
                return Json(new { code = -1, msg = "数据不存在！" }, JsonRequestBehavior.AllowGet);


            if (BargainType > 0)
            {
                EntSetting ent = EntSettingBLL.SingleModel.GetModel(appId);
                if (ent == null) return View("PageError", new Return_Msg() { Msg = "找不到该专业版!", code = "403" });

                if (model.StoreId != ent.aid)
                    return Json(new { code = -1, msg = "没有权限！" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                Store store = StoreBLL.SingleModel.GetModelByRid(appId);
                if (store == null)
                    return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "403" });
                if (model.StoreId != store.Id)
                    return Json(new { code = -1, msg = "没有权限！" }, JsonRequestBehavior.AllowGet);
            }

            string upFiled = "State";

            if (actionType > 0)
            {
                if (model.State == 0)
                {
                    return Json(new { code = -1, msg = "未下架的砍价商品不能删除！" }, JsonRequestBehavior.AllowGet);
                }
                upFiled = "IsDel";
                model.IsDel = -1;
                //表示删除产品
            }
            else
            {
                //表示上架或下架
                if (model.State == 0)
                {
                    model.State = -1;
                    model.IsEnd = 1;
                }
                else
                {
                    model.State = 0;
                    model.IsEnd = 0;
                }
            }




            if (BargainBLL.SingleModel.Update(model, upFiled))
                return Json(new { code = 1, msg = "操作成功！" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { code = -1, msg = "操作失败！" }, JsonRequestBehavior.AllowGet);

        }



        //砍价帮砍记录
        public ActionResult BargainRecordList(int appId = 0, int PageType = 0, int buid = 0, int pageIndex = 1, int pageSize = 10)
        {
            if (appId <= 0 || PageType <= 0 || buid <= 0)
            {
                View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            string strWhere = $"BUId={buid}";
            string orderWhere = "CreateDate desc";


            List<BargainRecordList> list = BargainRecordListBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", orderWhere);
            ViewBag.pageSize = pageSize;
            ViewBag.TotalCount = BargainRecordListBLL.SingleModel.GetCount(strWhere);
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;

            return View(list == null ? new List<BargainRecordList>() : list);
        }

        [RouteAuthCheck]
        public ActionResult GiveGoods(int appId = 0, int PageType = 0, int buid = 0, int masterPage = 1)
        {
            if (appId <= 0 || PageType <= 0 || buid <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            BargainUser _BargainUser = BargainUserBLL.SingleModel.GetModel(buid);
            if (_BargainUser == null)
            {
                return View("PageError", new Return_Msg() { Msg = "砍价领取记录不存在!", code = "404" });
            }

            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            ViewBag.addr = _BargainUser.Address;
            ViewBag.masterPage = masterPage;

            return View(_BargainUser);
        }



        /// <summary>
        /// 发货操作
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="buid"></param>
        /// <param name="SendGoodsName"></param>
        /// <param name="WayBillNo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateGiveGoods(int appId = 0, int buid = 0, string SendGoodsName = "", string WayBillNo = "",string attachData="")
        {
            if (appId <= 0 || buid <= 0)
                return Json(new { code = -1, msg = "参数错误！" });
            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return Json(new { code = -1, msg = "暂无权限！请先登录" });
            }
            BargainUser _BargainUser = BargainUserBLL.SingleModel.GetModel(buid);
            if (_BargainUser == null)
            {
                return Json(new { code = -1, msg = "砍价领取记录不存在！" });
            }

            Bargain Bargain = BargainBLL.SingleModel.GetModel(_BargainUser.BId);
            if (Bargain == null)
            {
                return Json(new { code = -1, msg = "砍价商品不存在！" });
            }

            if (Bargain.BargainType > 0)
            {
                EntSetting ent = EntSettingBLL.SingleModel.GetModel(role.Id);
                if (ent == null)
                    return View("PageError", new Return_Msg() { Msg = "找不到该专业版!", code = "403" });

                if (Bargain.StoreId != ent.aid)
                    return Json(new { code = -1, msg = "没有权限！" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                Store store = StoreBLL.SingleModel.GetModelByRid(appId);
                if (store == null)
                    return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "403" });
                if (Bargain.StoreId != store.Id)
                    return Json(new { code = -1, msg = "没有权限！" }, JsonRequestBehavior.AllowGet);
            }





            if (!string.IsNullOrEmpty(_BargainUser.WayBillNo) || !string.IsNullOrEmpty(_BargainUser.SendGoodsName))
            {
                return Json(new { code = -1, msg = "已经发货了,不能修改！" });
            }
            if (string.IsNullOrEmpty(WayBillNo) || string.IsNullOrEmpty(SendGoodsName))
            {
                return Json(new { code = -1, msg = "快递单号或者名称不能为空！" });
            }
            if (SendGoodsName.Length > 8)
            {
                return Json(new { code = -1, msg = "快递名称过长" });
            }

            string msg = string.Intern("");
            bool addExpressResult = false;
            if (!string.IsNullOrEmpty(attachData))
            {
                DeliveryUpdatePost DeliveryInfo = System.Web.Helpers.Json.Decode<DeliveryUpdatePost>(attachData);
                if (DeliveryInfo != null)
                {
                    WayBillNo = DeliveryInfo.DeliveryNo;
                    SendGoodsName = DeliveryInfo.CompanyTitle;

                    addExpressResult = DeliveryFeedbackBLL.SingleModel.AddOrderFeed(_BargainUser.Id, DeliveryInfo, DeliveryOrderType.专业版砍价发货);
                    if (!addExpressResult)
                    {
                        return Json(new { code = -1, msg = "物流信息添加失败，发货失败！" });
                    }
                }
            }

            _BargainUser.State = 6;
            _BargainUser.WayBillNo = WayBillNo;
            _BargainUser.SendGoodsName = SendGoodsName;
            _BargainUser.SendGoodsTime = DateTime.Now;
            if (BargainUserBLL.SingleModel.Update(_BargainUser, "SendGoodsName,WayBillNo,SendGoodsTime,State"))
            {
                string storeName = "";
                if (Bargain != null)
                {
                    switch (Bargain.BargainType)
                    {
                        case 0:
                            Store store = StoreBLL.SingleModel.GetModel(Bargain.StoreId);
                            if (store != null)
                            {
                                List<ConfParam> paramslist = ConfParamBLL.SingleModel.GetListByRId(Convert.ToInt32(appId)) ?? new List<ConfParam>();
                                storeName = paramslist.Where(w => w.Param == "nparam").FirstOrDefault()?.Value;
                            }
                            break;
                        case 1:
                            storeName = OpenAuthorizerConfigBLL.SingleModel.GetModel($" rid = {appId} ")?.nick_name;
                            break;
                        default:
                            storeName = "";
                            break;

                    }
                }

                #region 模板消息
                try
                {
                    XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
                    if (app == null)
                    {
                        throw new Exception($"发送砍价发货模板消息参数错误 app_null :appId = {appId}");
                    }
                    XcxTemplate xcxTemp = XcxTemplateBLL.SingleModel.GetModel(app.TId);
                    if (xcxTemp == null)
                    {
                        throw new Exception($"发送砍价发货模板消息参数错误 xcxTemp_null :xcxTempId = {app.TId}");
                    }
                    Account account = AccountBLL.SingleModel.GetModel(app.AccountId);
                    if (account == null)
                    {
                        throw new Exception($"发送砍价发货模板消息参数错误 account_null :accountId = {app.AccountId}");
                    }
                    //电商为旧做法,兼容电商
                    switch (xcxTemp.Type)
                    {
                        case (int)TmpType.小程序电商模板:
                            #region 购买者模板消息
                            object postData = BargainUserBLL.SingleModel.GetTemplateMessageData_SendGoods(_BargainUser.Id, storeName);
                            TemplateMsg_Miniapp.SendTemplateMessage(_BargainUser.UserId, SendTemplateMessageTypeEnum.电商订单配送通知, (int)TmpType.小程序电商模板, postData);
                            #endregion
                            break;
                        default:
                            object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(_BargainUser, SendTemplateMessageTypeEnum.砍价订单发货提醒);
                            TemplateMsg_Miniapp.SendTemplateMessage(_BargainUser.UserId, SendTemplateMessageTypeEnum.砍价订单发货提醒, xcxTemp.Type,orderData);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteError(GetType(), ex);
                }
                #endregion

                return Json(new { code = 1, msg = "发货成功！" });
            }
            else
            {
                return Json(new { code = -1, msg = "发货异常！" });
            }


        }



        /// <summary>
        /// 订单退款 后台操作
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult outOrder(int buid = 0, int AppId = 0, int buyPrice = 0, string remark = "")
        {
            if (buid <= 0 || AppId <= 0)
                return Json(new { isok = false, msg = "参数错误" });

            BargainUser BargainUser = BargainUserBLL.SingleModel.GetModel(buid);
            if (BargainUser == null)
            {
                return Json(new { isok = false, msg = "数据不存在!" });
            }

            Bargain _Bargain = BargainBLL.SingleModel.GetModel($"Id={BargainUser.BId}");
            if (_Bargain == null)
            {
                return Json(new { isok = false, msg = "砍价商品不存在!" });
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(AppId, dzaccount.Id.ToString());
            if (role == null)
            {
                return Json(new { isok = false, msg = "无权限!" });
            }

            XcxTemplate xcxTempLateModel = XcxTemplateBLL.SingleModel.GetModel(role.TId);
            if(xcxTempLateModel==null)
            {
                return Json(new { isok = false, msg = "小程序模板为空!" });
            }

            if (buyPrice <= 0)
            {
                return Json(new { isok = false, msg = "请输入退款金额!" });
            }
            int money = BargainUser.CurrentPrice + _Bargain.GoodsFreight;//实付金额=当前价格+运费
            if (money < buyPrice)
            {
                return Json(new { isok = false, msg = "退款金额不能高于实际付款金额!" });
            }
            BargainUser.State = 2;
            BargainUser.outOrderDate = DateTime.Now;
            BargainUser.refundFee = buyPrice;

            if (BargainUser.PayType == (int)miniAppBuyMode.储值支付)
            {
                BargainUser.State = 3;
                SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(role.AppId, BargainUser.UserId);
                TransactionModel tran = new TransactionModel();
                tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
                {
                    AppId = role.AppId,
                    UserId = BargainUser.UserId,
                    MoneySetUserId = saveMoneyUser.Id,
                    Type = 1,
                    BeforeMoney = saveMoneyUser.AccountMoney,
                    AfterMoney = saveMoneyUser.AccountMoney + buyPrice,
                    ChangeMoney = buyPrice,
                    ChangeNote = $"小程序砍价购买商品[{BargainUser.BName}]退款,订单号:{BargainUser.OrderId} ",
                    CreateDate = DateTime.Now,
                    State = 1
                }));

                tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney + {buyPrice} where id =  {saveMoneyUser.Id} ; ");/*  _miniappsavemoneysetuserBll.BuildUpdateSql(saveMoneyUser, "AccountMoney"));*/


                ////减砍价商品库存 砍价退款不需要回退库存
                //_Bargain.RemainNum++;
                //string cutRemainNum = _miniappBargainBLL.BuildUpdateSql(_Bargain, "RemainNum");

                string updateBargainUser = BargainUserBLL.SingleModel.BuildUpdateSql(BargainUser, "State,outOrderDate,refundFee");

                //tran.Add(cutRemainNum);
                tran.Add(updateBargainUser);

                //foreach (var x in tran.sqlArray)
                //{
                //    log4net.LogHelper.WriteInfo(GetType(), x);
                //}
                bool isok = BargainBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray);
                if (isok)
                {
                    object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(BargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, remark);
                    TemplateMsg_Miniapp.SendTemplateMessage(BargainUser.UserId, SendTemplateMessageTypeEnum.砍价订单退款通知, xcxTempLateModel.Type, orderData);
                    return Json(new { isok = true, msg = "退款成功,请查看账户余额!" });//返回订单信息
                }
                else
                {
                    return Json(new { isok = false, msg = "退款异常!" });//返回订单信息
                }

            }
            else
            {
                CityMorders order = new CityMordersBLL().GetModel(BargainUser.CityMordersId);
                if (order == null)
                {
                    return Json(new { isok = false, msg = "订单信息有误!" });
                }

                if (BargainUserBLL.SingleModel.Update(BargainUser, "State,outOrderDate,refundFee"))
                {
                    ReFundQueue reModel = new ReFundQueue
                    {
                        minisnsId = -5,
                        money = buyPrice,
                        orderid = order.Id,
                        traid = order.trade_no,
                        addtime = DateTime.Now,
                        note = "小程序砍价订单退款",
                        retype = 1
                    };
                    try
                    {
                        int funid = Convert.ToInt32(new ReFundQueueBLL().Add(reModel));
                        if (funid > 0)
                        {
                            object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(BargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, remark);
                            TemplateMsg_Miniapp.SendTemplateMessage(BargainUser.UserId, SendTemplateMessageTypeEnum.砍价订单退款通知, xcxTempLateModel.Type,orderData);

                            return Json(new { isok = true, msg = "操作成功,已提交退款申请!", obj = funid });
                        }
                        else
                        {
                            return Json(new { isok = false, msg = "退款异常插入队列小于0!" });
                        }

                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteInfo(GetType(), $"{ex.Message} xxxxxxxxxxxxxxxx小程序砍价退款订单插入队列失败 ID={order.Id}");
                        return Json(new { isok = false, msg = "退款异常(插入队列失败)!" });
                    }



                }
                else
                {
                    return Json(new { isok = false, msg = "退款异常!" });
                }
            }




        }


        public ActionResult ConfirmBargainOrder(int buid = 0, int appId = 0)
        {
            if (buid <= 0 || appId <= 0)
                return Json(new { isok = false, msg = "参数错误" });

            BargainUser BargainUser = BargainUserBLL.SingleModel.GetModel(buid);
            if (BargainUser == null)
            {
                return Json(new { isok = false, msg = "数据不存在!" });
            }
            XcxAppAccountRelation r = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (r == null)
            {
                return Json(new { isok = false, msg = "未授权小程序模板!" });
            }

            Bargain _Bargain = BargainBLL.SingleModel.GetModel($"Id={BargainUser.BId}");
            if (_Bargain == null)
            {
                return Json(new { isok = false, msg = "砍价商品不存在!" });
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return Json(new { isok = false, msg = "无权限!" });
            }
            BargainUser.State = 8;
           if(!BargainUserBLL.SingleModel.Update(BargainUser, "State"))
            {
                return Json(new { isok = false, msg = "操作异常!" });
            }
            return Json(new { isok = true, msg = "操作成功!" });
        }



        /// <summary>
        /// 删除视频
        /// </summary>
        /// <param name="vid">视频的id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult delAttVideo(int vid = 0)
        {

            C_AttachmentVideo cattV = C_AttachmentVideoBLL.SingleModel.GetModel(vid);
            if (cattV == null)
            {
                return Json(new { code = 1, msg = "该视频已不存在" });
            }

            if (C_AttachmentVideoBLL.SingleModel.Delete(cattV.id) > 0)
            {
                C_AttachmentVideoBLL.SingleModel.RemoveRedis(cattV.itemId, cattV.itemType);//清除缓存
                return Json(new { code = 1, msg = "删除成功" });
            }
            return Json(new { code = 0, msg = "系统繁忙db_err" });
        }



        public ActionResult DeleteVoice(int id = 0)
        {
            C_Attachment model = C_AttachmentBLL.SingleModel.GetModel(id);
            if (model != null)
            {
                if (C_AttachmentBLL.SingleModel.Delete(id) > 0)
                {
                    return Json(new { code = 1, msg = "删除成功!" });
                }
                else
                {
                    return Json(new { code = -1, msg = "删除失败!" });
                }

            }
            else
            {
                return Json(new { code = 1, msg = "删除成功(音乐不存在)!" });
            }
        }

        public ActionResult DeleteImg(int id = 0)
        {
            try
            {
                C_AttachmentBLL.SingleModel.DeleteImg(id);
                return Json(new { Success = true, Msg = "删除成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Msg = "删除失败" }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetData(int BargainType)
        {
            List<object> list = new List<object>();
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(BargainBLL.SingleModel.connName, CommandType.Text, $"SELECT * from Bargain where BargainType={BargainType}", null))
            {
                while (dr.Read())
                {
                    Bargain model = BargainBLL.SingleModel.GetModel(dr);
                    list.Add(model);
                }

            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult DeliveryInfo(int appId = 0, int orderId = 0, int orderType = 0, string address = null, int deliveryOrderType = 0)
        {
            ViewBag.OrderType = orderType;
            ViewBag.DeliveryOrderType = deliveryOrderType;
            if (appId <= 0 || orderId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return View("PageError", new Return_Msg() { Msg = "暂无权限！请先登录!", code = "500" });
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "暂无权限！请先登录!", code = "500" });
            }

            EntGoodsOrder order = new EntGoodsOrder();
            if (deliveryOrderType == 0)
            {
                order = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
                switch (orderType)
                {
                    case (int)TmpType.小未平台子模版:
                        order = new EntGoodsOrder();
                        PlatChildGoodsOrder platChildOrder = PlatChildGoodsOrderBLL.SingleModel.GetModel(orderId);
                        if (platChildOrder == null)
                        {
                            return View("PageError", new Return_Msg() { Msg = "找不到订单数据!", code = "500" });
                        }
                        order.Id = platChildOrder.Id;
                        order.AccepterName = platChildOrder.AccepterName;
                        order.AccepterTelePhone = platChildOrder.AccepterTelePhone;
                        order.Address = platChildOrder.Address;
                        break;
                    case (int)TmpType.企业智推版:
                        order = new EntGoodsOrder();
                        QiyeGoodsOrder qiyeOrder = QiyeGoodsOrderBLL.SingleModel.GetModel(orderId);
                        if (qiyeOrder == null)
                        {
                            return View("PageError", new Return_Msg() { Msg = "找不到订单数据!", code = "500" });
                        }
                        order.Id = qiyeOrder.Id;
                        order.AccepterName = qiyeOrder.AccepterName;
                        order.AccepterTelePhone = qiyeOrder.AccepterTelePhone;
                        order.Address = qiyeOrder.Address;
                        break;
                }
            }
            else
            {
                switch (deliveryOrderType)
                {
                    case (int)DeliveryOrderType.专业版砍价发货:
                        order = new EntGoodsOrder();
                        BargainUser bargainUser = BargainUserBLL.SingleModel.GetModel(orderId);
                        if (bargainUser != null)
                        {
                            order.AccepterName = bargainUser.AddressUserName;
                            order.AccepterTelePhone = bargainUser.TelNumber;
                            order.Address = bargainUser.AddressDetail;
                        }
                        break;
                }
            }

            return View(order);
        }

        public ActionResult GetDeliveryCompany(int appId = 0)
        {
            if (appId <= 0)
            {
                return ReturnJson(message: "参数错误", code: "403");
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return ReturnJson(message: "暂无权限！请先登录!", code: "403");
            }

            List<DeliveryCompany> companys = DeliveryCompanyBLL.SingleModel.GetCompanys();
            return ReturnJson(isOk: true, message: "获取成功", code: "200", data: companys);
        }

        [HttpGet]
        public ActionResult ViewDeliveryFeed(int appId = 0, int orderId = 0, int orderType = -1)
        {

            if (appId <= 0 || !DeliveryFeedbackBLL.SingleModel.IsVaildType(orderType))
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            //XcxAppAccountRelation role = _xcxappaccountrelationBll.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            //if (role == null)
            //{
            //    return View("PageError", new Return_Msg() { Msg = "暂无权限！请先登录!", code = "500" });
            //}
            DeliveryFeedback deliveryInfo = null;
            switch (orderType)
            {
                case (int)DeliveryOrderType.专业版订单商家发货:
                    deliveryInfo = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId, DeliveryOrderType.专业版订单商家发货);
                    break;
                case (int)DeliveryOrderType.专业版订单用户退货:
                    deliveryInfo = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId, DeliveryOrderType.专业版订单用户退货);
                    break;
                case (int)DeliveryOrderType.专业版订单商家换货:
                    deliveryInfo = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId, DeliveryOrderType.专业版订单商家换货);
                    break;
                case (int)DeliveryOrderType.独立小程序订单商家发货:
                    deliveryInfo = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId, DeliveryOrderType.独立小程序订单商家发货);
                    break;
                case (int)DeliveryOrderType.拼享惠订单商家发货:
                    deliveryInfo = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId, DeliveryOrderType.拼享惠订单商家发货);
                    break;
            }
            return View(deliveryInfo);
        }

        [HttpGet]
        public ActionResult GetDeliveryFeed(int appId = 0, int orderId = 0, int orderType = -1)
        {
            if (appId <= 0 || orderId <= 0 || orderType < 0)
            {
                return ReturnJson(message: "参数错误", code: "403");
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return ReturnJson(message: "暂无权限！请先登录!", code: "403");
            }

            DeliveryFeedback deliveryFeed = null;


            switch (orderType)
            {
                case (int)DeliveryOrderType.专业版订单商家发货:
                    deliveryFeed = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId, DeliveryOrderType.专业版订单商家发货);
                    break;
                case (int)DeliveryOrderType.专业版订单用户退货:
                    deliveryFeed = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId, DeliveryOrderType.专业版订单用户退货);
                    break;
                case (int)DeliveryOrderType.专业版订单商家换货:
                    deliveryFeed = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId, DeliveryOrderType.专业版订单商家换货);
                    break;
            }

            return ReturnJson(isOk: true, code: "200", data: deliveryFeed);
        }

        public ActionResult UpLoadImgFrm(int appId, int multi_selection = 0, int maxImgSize = 1, int storeId = 0)
        {
            ViewBag.appId = appId;
            ViewBag.storeId = storeId;
            ViewBag.multi_selection = multi_selection;
            ViewBag.maxImgSize = maxImgSize;
            return View();
        }

        /// <summary>
        /// 删除素材
        /// </summary>
        /// <returns></returns>
        public ActionResult DelMaterials()
        {
            result = new Return_Msg();
            result.code = "200";
            int appId = Context.GetRequestInt("appId", -2);
            if (appId <= -2)
            {
                result.Msg = "appId非法";
                return Json(result);
            }
            int storeId = Context.GetRequestInt("storeId", 0);

            string materialsStr = Context.GetRequest("materialsStr", string.Empty);
            if (string.IsNullOrEmpty(materialsStr))
            {
                result.Msg = "请先选择需要删除的图片";
                return Json(result);
            }

            List<MaterialsItem> list = JsonConvert.DeserializeObject<List<MaterialsItem>>(materialsStr);
            if (list != null && list.Count > 0)
            {
                string materialsIds = string.Join(",",list.Select(s=>s.Id));
                List<CityMaterials> cityMaterialsList = CityMaterialsBLL.SingleModel.GetListByIds(materialsIds);
                TransactionModel transactionModel = new TransactionModel();
                foreach (var item in list)
                {
                    CityMaterials materials = cityMaterialsList?.FirstOrDefault(f=>f.Id == item.Id);
                    if (materials == null || materials.aid != appId)
                    {
                        result.Msg = "暂无权限";
                        return Json(result);
                    }
                    materials.State = -1;
                    transactionModel.Add(CityMaterialsBLL.SingleModel.BuildUpdateSql(materials, "State"));
                }
                if (CityMaterialsBLL.SingleModel.ExecuteTransactionDataCorect(transactionModel.sqlArray))
                {
                    result.isok = true;
                    result.Msg = "删除成功";
                    return Json(result);
                }
                else
                {
                    result.Msg = "删除失败";
                    return Json(result);
                }

            }
            result.Msg = "没有需要删除的图片";
            return Json(result);

        }

        public ActionResult GetReturnOrderInfo(int appId = 0, int orderId = 0)
        {
            if (appId <= 0 || orderId <= 0)
            {
                return ReturnJson(message: "参数错误!");
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return ReturnJson(message: "授权异常");
            }

            ReturnGoods returnInfo = ReturnGoodsBLL.SingleModel.GetByOrderId(orderId);

            return ReturnJson(isOk: true, data: returnInfo);
        }

        [HttpGet]
        public ActionResult ViewReturnOrderInfo(int appId = 0, int orderId = 0)
        {

            if (appId <= 0 || orderId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (role == null)
            {
                return View("PageError", new Return_Msg() { Msg = "暂无权限！请先登录!", code = "500" });
            }

            ReturnGoods returnInfo = ReturnGoodsBLL.SingleModel.GetByOrderId(orderId);
            List<EntGoodsCart> goods = ReturnGoodsBLL.SingleModel.GetGoodList(returnInfo);
            Tuple<ReturnGoods, List<EntGoodsCart>> viewModel = Tuple.Create(returnInfo, goods);
            return View(viewModel);
        }

        /// <summary>
        /// 获取省市区列表（引用：DeliveryTemplate.cshtml）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult GetAreaList(int appId = 0, string areaId = "0")
        {
            List<C_Area> areas = C_AreaBLL.SingleModel.GetByIds(areaId);

            return ReturnJson(isOk: true, data: areas, message: "获取成功");
        }

        /// <summary>
        /// 运费模板管理页
        /// </summary>
        /// <param name="authData"></param>
        /// <param name="appId"></param>
        /// <param name="pageType"></param>
        /// <returns></returns>
        [LoginFilter(parseAuthDataTo: "authData")]
        [RouteAuthCheck]
        public ActionResult DeliveryTemplate(XcxAppAccountRelation authData, int appId = 0, int? pageType = null, int? storeId = null)
        {
            if (!pageType.HasValue)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={authData.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int freightSwtich = 0;//运费开关
            int versionId = 0;

            switch (pageType)
            {
                case (int)TmpType.小程序专业模板:
                    #region 判断该专业版是否能使用运费模板功能
                    FunctionList functionList = new FunctionList();
                    versionId = authData.VersionId;
                    functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                    if (functionList == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                    }
                    if (!string.IsNullOrEmpty(functionList.StoreConfig))
                    {
                        StoreConfig storeConfig = JsonConvert.DeserializeObject<StoreConfig>(functionList.StoreConfig);
                        freightSwtich = storeConfig.FreightTemplate;

                    }
                    #endregion

                    Store store = StoreBLL.SingleModel.GetModelByRid(authData.Id);
                    if (!string.IsNullOrEmpty(store?.configJson))
                    {
                        StoreConfigModel config = System.Web.Helpers.Json.Decode<StoreConfigModel>(store?.configJson);
                        ViewBag.Rule = config?.deliveryFeeSumMethond;
                        ViewBag.Enable = config?.enableDeliveryTemplate != null && config.enableDeliveryTemplate ? 1 : 0;
                    }
                    break;
                case (int)TmpType.小未平台子模版:
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetModelByAId(authData.Id);
                    if (!string.IsNullOrEmpty(platStore?.SwitchConfig))
                    {
                        PlatStoreSwitchModel config = System.Web.Helpers.Json.Decode<PlatStoreSwitchModel>(platStore?.SwitchConfig);
                        ViewBag.Rule = config?.deliveryFeeSumMethond;
                        ViewBag.Enable = config?.enableDeliveryTemplate != null && config.enableDeliveryTemplate ? 1 : 0;
                    }
                    break;
                case (int)TmpType.企业智推版:
                    QiyeStore qiyeStore = QiyeStoreBLL.SingleModel.GetModelByAId(authData.Id);
                    if (!string.IsNullOrEmpty(qiyeStore?.SwitchConfig))
                    {
                        QiyeStoreSwitchModel config = System.Web.Helpers.Json.Decode<QiyeStoreSwitchModel>(qiyeStore?.SwitchConfig);
                        ViewBag.Rule = config?.deliveryFeeSumMethond;
                        ViewBag.Enable = config?.enableDeliveryTemplate != null && config.enableDeliveryTemplate ? 1 : 0;
                    }
                    break;
                case (int)TmpType.拼享惠:
                    PinStore pinStore = PinStoreBLL.SingleModel.GetModelByAid_Id(authData.Id, storeId.Value);
                    if (pinStore != null)
                    {
                        ViewBag.Rule = pinStore.setting.freightSumRule;
                        ViewBag.Enable = pinStore.setting.freightSwitch ? 1 : 0;
                        ViewBag.AppendQueryPara = JsonConvert.SerializeObject(new { storeId = pinStore.id });
                    }
                    break;
            }

            if (freightSwtich == 1)
            {
                //表示没有该使用功能
                ViewBag.Enable = 0;
            }

            ViewBag.freightSwtich = freightSwtich;
            ViewBag.versionId = versionId;
            ViewBag.PageType = pageType.Value;
            ViewBag.config = DeliveryConfigBLL.SingleModel.GetStoreConfig(authData.Id) ?? new DeliveryConfig();
            return View(authData);
        }

        /// <summary>
        /// 获取运费模板列表（引用：DeliveryTemplate.cshtml, Pedit.cshtml, GoodEdit.cshtml）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="unitType">单位类型</param>
        /// <param name="pagenIdex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageType">小程序类型</param>
        /// <param name="parentId">母模板ID</param>
        /// <returns></returns>
        [LoginFilter(parseAuthDataTo: "authData")]
        public ActionResult GetDeliveryTemplate(XcxAppAccountRelation authData, int appId = 0, int? unitType = null, int pagenIdex = 1, int pageSize = 99, int? pageType = null, int? parentId = null, int? storeId = null)
        {
            List<DeliveryTemplate> templates = null;
            if (parentId > 0)
            {
                //查询配送区域
                templates = DeliveryTemplateBLL.SingleModel.GetRegionTemplate(parentId.Value);
            }
            else
            {
                //查询运费模板
                templates = DeliveryTemplateBLL.SingleModel.GetByAid(appId: authData.Id, storeId: storeId, pageIndex: pagenIdex, pageSize: pageSize, unitType: unitType);
            }

            if (parentId > 0 && templates.Count == 0)
            {
                //兼容旧模板数据
                templates.Add(DeliveryTemplateBLL.SingleModel.GetModel(parentId.Value));
            }

            if (pageType.HasValue && pageType.Value == (int)TmpType.小程序专业模板)
            {
                //统计使用运费模板中的商品数
                foreach (DeliveryTemplate template in templates)
                {
                    template.ApplyCount = EntGoodsBLL.SingleModel.GetCount($"TemplateId = {template.Id}");
                }
            }
            else if (pageType.HasValue && pageType.Value == (int)TmpType.小未平台子模版)
            {
                //统计使用运费模板中的商品数

                foreach (DeliveryTemplate template in templates)
                {
                    template.ApplyCount = PlatChildGoodsBLL.SingleModel.GetCountByTemplateId(template.Id);
                }
            }
            else if (pageType.HasValue && pageType.Value == (int)TmpType.企业智推版)
            {
                //统计使用运费模板中的商品数

                foreach (DeliveryTemplate template in templates)
                {
                    template.ApplyCount = QiyeGoodsBLL.SingleModel.GetCountByTemplateId(template.Id);
                }
            }
            else if (pageType.HasValue && pageType.Value == (int)TmpType.拼享惠 && storeId.HasValue)
            {
                //统计使用运费模板中的商品数

                foreach (DeliveryTemplate template in templates)
                {
                    template.ApplyCount = PinGoodsBLL.SingleModel.GetCountByPara(storeId: storeId.Value, aid: authData.Id, freightTemplate: template.Id);
                }
            }

            return ReturnJson(isOk: true, data: parentId.HasValue ? templates.OrderBy(template => template.Id) : templates.OrderByDescending(template => template.UpdateTime));
        }

        /// <summary>
        /// 更新运费模板（引用：DeliveryTemplate.cshtml）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="newTemplate"></param>
        /// <param name="templateId"></param>
        /// <param name="regionTemplate"></param>
        /// <returns></returns>
        [LoginFilter(parseAuthDataTo: "authData")]
        public ActionResult UpdateDeliveryTemplate(XcxAppAccountRelation authData, int appId = 0, int? storeId = null, DeliveryTemplate newTemplate = null, int? templateId = null, List<DeliveryTemplate> regionTemplate = null)
        {
            if (newTemplate == null && regionTemplate == null)
            {
                return ReturnJson(message: "参数错误");
            }

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={authData.TId}");

            if (xcxTemplate == null)
            {
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });
            }

            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                #region 判断该专业版是否能使用运费模板功能
                FunctionList functionList = new FunctionList();

                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={authData.VersionId}");
                if (functionList == null && string.IsNullOrEmpty(functionList.StoreConfig))
                {
                    return ReturnJson(message: "功能权限未设置!");

                }

                StoreConfig storeConfig = JsonConvert.DeserializeObject<StoreConfig>(functionList.StoreConfig);
                if (storeConfig.FreightTemplate == 1)
                {
                    return ReturnJson(message: "请升级到更高版本才能开启此功能!");
                }
                #endregion
            }

            //校验输入"运费模板"&"配送区域"

            Func<DeliveryTemplate, JsonResult> checkTemplate = (template) =>
             {
                 if (!Enum.IsDefined(typeof(DeliveryUnit), template.UnitType) || template.Base <= 0 || template.BaseCost < 0 || template.Extra < 0 || template.ExtraCost < 0)
                 {
                     return ReturnJson(message: "非法单位");
                 }
                 if (!string.IsNullOrWhiteSpace(template.DeliveryRegion) && !DeliveryTemplateBLL.SingleModel.CheckRegionCode(template.DeliveryRegion))
                 {
                     return ReturnJson(message: "非法配送区域码");
                 }
                 //if(template.EnableDiscount && template.Discount <= 0)
                 //{
                 //    return ReturnJson(message: "满减金额不能为零");
                 //}
                 return null;
             };
            JsonResult errorResult = null;
            foreach (var template in regionTemplate ?? new List<DeliveryTemplate>())
            {
                template.DeliveryRegion = string.Join(",", template.DeliveryRegion?.SplitStr(",").Where(code => StringHelper.IsNumber(code) && int.Parse(code) > 0));
                errorResult = checkTemplate(template);
            }
            if (newTemplate != null)
            {
                errorResult = checkTemplate(newTemplate);
                if (string.IsNullOrWhiteSpace(newTemplate.Name))
                {
                    return ReturnJson(message: "请填写运费模板名称");
                }
            }

            //返回校验错误
            if (errorResult != null)
            {
                return errorResult;
            }

            //开始保存模板
            bool result = false;
            if (newTemplate != null && templateId.HasValue && templateId.Value > 0)
            {
                //校验编辑运费模板
                DeliveryTemplate targetTemplate = DeliveryTemplateBLL.SingleModel.GetModel(templateId.Value);
                if (targetTemplate.Aid != authData.Id)
                {
                    return ReturnJson(message: "非法操作");
                }
                //校验编辑配送区域
                List<DeliveryTemplate> currRegions = DeliveryTemplateBLL.SingleModel.GetRegionTemplate(templateId.Value);
                if (regionTemplate?.Count > 0)
                {
                    currRegions = DeliveryTemplateBLL.SingleModel.GetRegionTemplate(templateId.Value);
                    if (regionTemplate.Exists(newRegion => currRegions.Exists(currRegion => DeliveryTemplateBLL.SingleModel.HasRepeatRegion(currRegion, newRegion))))
                    {
                        return ReturnJson(message: "非法数据");
                    }
                }
                if (targetTemplate.UnitType != newTemplate.UnitType && Enum.IsDefined(typeof(DeliveryUnit), newTemplate.UnitType) && currRegions?.Count > 0)
                {
                    //更新配送单位
                    DeliveryUnit newUnitType = new DeliveryUnit();
                    if (Enum.TryParse(newTemplate.UnitType.ToString(), out newUnitType))
                    {
                        currRegions.ForEach((region) =>
                        {
                            region = DeliveryTemplateBLL.SingleModel.ConvertUnitType(region, newUnitType);
                        });
                    }
                    regionTemplate = currRegions;
                }
                //更新保存
                result = DeliveryTemplateBLL.SingleModel.UpdateTemplate(templateId.Value, newTemplate);
                if (!result)
                {
                    //保存失败
                    return ReturnJson(isOk: false);
                }
            }
            else if (newTemplate != null)
            {
                newTemplate.StoreId = storeId.HasValue ? storeId.Value : 0;
                //新增
                templateId = DeliveryTemplateBLL.SingleModel.AddTemplate(authData.Id, newTemplate);
                result = templateId > 0;
            }
            if (regionTemplate?.Count > 0 && templateId.HasValue && templateId.Value > 0)
            {
                //更新配送区域
                int CompatData = regionTemplate.FindIndex(region => region.Id == templateId);
                if (CompatData > -1)
                {
                    //兼容转换旧模板
                    regionTemplate[CompatData].Id = 0;
                }
                result = DeliveryTemplateBLL.SingleModel.UpdateRegionTemplate(templateId.Value, authData.Id, regionTemplate);
            }

            return ReturnJson(isOk: result);
        }

        /// <summary>
        /// 删除运费模板（引用：DeliveryTemplate.cshtml）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="templateId">运费模板ID</param>
        /// <returns></returns>
        [LoginFilter(parseAuthDataTo: "authData")]
        public ActionResult DeleteDeliveryTemplate(XcxAppAccountRelation authData, int appId = 0, int templateId = 0)
        {
            if (templateId <= 0)
            {
                return ReturnJson(message: "参数错误");
            }


            DeliveryTemplate deltemplate = DeliveryTemplateBLL.SingleModel.GetModel(templateId);
            if (deltemplate.Aid != authData.Id)
            {
                return ReturnJson(message: "非法操作");
            }

            bool result = DeliveryTemplateBLL.SingleModel.DeleteTemplate(deltemplate);
            return ReturnJson(isOk: result, message: result ? "操作成功" : "操作失败");
        }

        /// <summary>
        /// 更新运费模板功能配置（接口引用：DeliveryTemplate.cshtml）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageType"></param>
        /// <param name="storeId">拼享惠店铺ID</param>
        /// <param name="rule">运费模板结算规则</param>
        /// <param name="enable">开启运费模板</param>
        /// <param name="enableTrack">开启物流轨迹跟踪（未使用）</param>
        /// <param name="trackKey">物流轨迹跟踪密钥</param>
        /// <param name="enableDiscount">开启运费满减</param>
        /// <param name="discount">运费满级阈值</param>
        /// <returns></returns>
        [LoginFilter(parseAuthDataTo: "authData")]
        public ActionResult UpdateDeliveryConfig(XcxAppAccountRelation authData, int appId = 0, int pageType = 0,
            int? storeId = 0, int? rule = null, bool? enable = null, bool? enableDiscount = null, float? discount = null,
            bool? enableTrack = null, string trackKey = null)
        {
            if (pageType <= 0 || (!enable.HasValue && !enableTrack.HasValue))
            {
                return ReturnJson(message: "参数错误");
            }

            if (enable.HasValue && enable.Value && !rule.HasValue && !Enum.IsDefined(typeof(DeliveryFeeSumMethond), rule.Value))
            {
                return ReturnJson(message: "无效参数");
            }

            bool result = false;
            Dictionary<string, object> updateSet = new Dictionary<string, object>(2);
            if (enable.HasValue)
            {
                updateSet.Add("enableDeliveryTemplate", enable.Value);
                updateSet.Add("deliveryFeeSumMethond", rule.Value);
            }
            if (enableTrack.HasValue)
            {
                updateSet.Add("trackDelivery", enableTrack.Value);
                updateSet.Add("trackDeliveryKey", trackKey);
            }
            if (updateSet.Count == 0)
            {
                ReturnJson(isOk: false, message: "保存失败");
            }
            switch (pageType)
            {
                case (int)TmpType.小程序专业模板:
                    Store store = StoreBLL.SingleModel.GetModelByRid(authData.Id);
                    result = StoreBLL.SingleModel.UpdateConfig(store, updateSet);
                    break;
                case (int)TmpType.小未平台子模版:
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetModelByAId(appId);
                    if (platStore == null)
                    {
                        return ReturnJson(message: "还未绑定独立小程序");
                    }
                    result = PlatStoreBLL.SingleModel.UpdateConfig(platStore, updateSet);
                    break;
                case (int)TmpType.企业智推版:
                    QiyeStore qiyeStore = QiyeStoreBLL.SingleModel.GetModelByAId(appId);
                    if (qiyeStore == null)
                    {
                        return ReturnJson(message: "还未绑定独立小程序");
                    }
                    result = QiyeStoreBLL.SingleModel.UpdateConfig(qiyeStore, updateSet);
                    break;
                case (int)TmpType.拼享惠:

                    PinStore pinStore = PinStoreBLL.SingleModel.GetModelByAid_Id(authData.Id, storeId.Value);
                    pinStore.SetSettingValue("freightSwitch", enable);
                    pinStore.SetSettingValue("freightSumRule", rule);
                    result = PinStoreBLL.SingleModel.Update(pinStore, "SettingJson");
                    break;
            }

            if (discount.HasValue && enableDiscount.HasValue)
            {
                result = DeliveryConfigBLL.SingleModel.UpdateConfig(authData, enableDiscount.Value, (int)(discount.Value * 100));
            }

            return ReturnJson(isOk: result, message: result ? "保存成功" : "保存失败");
        }

        /// <summary>
        /// 专业版商品列表（接口引用：DeliveryTemplate.cshtml,/Enterprise/NewsEdit.cshtml）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="productType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [LoginFilter(parseAuthDataTo: "authData")]
        public ActionResult GetProductList(XcxAppAccountRelation authData, int pageIndex = 1, int pageSize = 9999, string search = null, int templateId = 0, int storeId = 0)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = StringHelper.ReplaceSqlKeyword(search);
            }
            //if (templateId > 0)
            //{
            //    pageSize = 9999;
            //}

            object products = null;
            int count = 0;
            int? pageType = XcxTemplateBLL.SingleModel.GetModel(authData.TId)?.Type;
            switch (pageType)
            {
                case (int)TmpType.小程序专业模板:
                    List<EntGoods> entProducts = EntGoodsBLL.SingleModel.GetListByTemplateId(appId: authData.Id, pageIndex: pageIndex, pageSize: pageSize, templateId: templateId);
                    products = entProducts.Select(item => new { Id = item.id, Name = item.name }).ToList();
                    if (pageIndex == 1)
                    {
                        count = EntGoodsBLL.SingleModel.GetCountByTemplateId(appId: authData.Id, templateId: templateId);
                    }
                    break;
                case (int)TmpType.小未平台子模版:
                    List<PlatChildGoods> platChildGoodsList = PlatChildGoodsBLL.SingleModel.GetListByRedis(authData.Id, ref count, pageIndex: pageIndex, pageSize: pageSize, templateid: templateId);
                    products = platChildGoodsList?.Select(item => new { Id = item.Id, Name = item.Name }).ToList();
                    break;
                case (int)TmpType.企业智推版:
                    List<QiyeGoods> qiyeGoodsList = QiyeGoodsBLL.SingleModel.GetListByRedis(authData.Id, ref count, pageIndex: pageIndex, pageSize: pageSize, templateid: templateId);
                    products = qiyeGoodsList?.Select(item => new { Id = item.Id, Name = item.Name }).ToList();
                    break;
                case (int)TmpType.拼享惠:
                    List<PinGoods> pinGoods = PinGoodsBLL.SingleModel.GetListByStoreId(storeId: storeId, aid: authData.Id, freightTemplate: templateId);
                    products = pinGoods?.Select(item => new { Id = item.id, Name = item.name }).ToList();
                    break;
            }
            object result = new { products, count };
            return ReturnJson(isOk: true, message: "获取成功", data: result);
        }

        /// <summary>
        /// 批量更新商品运费模板（接口引用：DeliveryTemplate.cshtml）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="templateId">模板ID</param>
        /// <param name="productIds">商品ID</param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult UpdateProductTemplate(int appId, int templateId, string productIds)
        {
            if (templateId <= 0)
            {
                return ReturnJson(message: "参数错误");
            }

            //模板类型
            int type = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(appId);

            DeliveryTemplate template = DeliveryTemplateBLL.SingleModel.GetModel(templateId);
            if (template?.Aid != appId)
            {
                return ReturnJson(message: "非法操作模板");
            }
            bool result = false;
            switch (type)
            {
                case (int)TmpType.小未平台子模版:

                    result = PlatChildGoodsBLL.SingleModel.UpdateTemplateByIds(appId, template.Id, productIds);
                    if (result)
                    {
                        //清除产品列表缓存
                        PlatChildGoodsBLL.SingleModel.RemoveEntGoodListCache(appId);
                    }
                    break;
                case (int)TmpType.企业智推版:

                    result = QiyeGoodsBLL.SingleModel.UpdateTemplateByIds(appId, template.Id, productIds);
                    if (result)
                    {
                        //清除产品列表缓存
                        QiyeGoodsBLL.SingleModel.RemoveEntGoodListCache(appId);
                    }
                    break;
                case (int)TmpType.拼享惠:

                    result = PinGoodsBLL.SingleModel.UpdateFreightTemplate(appId: appId, goodIds: productIds, templateId: template.Id);
                    break;
                default:
                    result = EntGoodsBLL.SingleModel.UpdateTemplateByIds(appId, template.Id, productIds);
                    if (result)
                    {
                        //清除产品列表缓存
                        EntGoodsBLL.SingleModel.RemoveEntGoodListCache(appId);
                    }
                    break;
            }

            return ReturnJson(isOk: result, message: result ? "保存成功" : "保存失败");
        }

        [LoginFilter(parseAuthDataTo: "authData")]
        public ActionResult Printer(XcxAppAccountRelation authData, int appId, int? printType = null, string content = null, string pageTitle = null)
        {
            if (!printType.HasValue)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            ViewBag.printType = printType.Value;
            if (!string.IsNullOrWhiteSpace(pageTitle))
            {
                ViewBag.title = pageTitle;
            }
            object printContent = null;
            switch (printType)
            {
                case (int)PrintContentType.打印专业版订单:
                    //订单ID
                    List<int> orderIds = content.ConvertToIntList(',');
                    //专业版订单
                    List<EntGoodsOrder> orders = EntGoodsOrderBLL.SingleModel.GetByIds(orderIds);
                    //订单详情
                    List<EntGoodsCart> buyProducts = EntGoodsCartBLL.SingleModel.GetListByOrderIds(string.Join(",", orderIds));
                    //产品详情
                    List<EntGoods> orgProducts = EntGoodsBLL.SingleModel.GetListByIds(string.Join(",", buyProducts.Select(thisPro => thisPro.FoodGoodsId)));
                    //获取拼团状态
                    EntGroupSponsorBLL.SingleModel.GetSponsorState(ref orders);
                    //格式化打印数据
                    printContent = orders.Select((order) =>
                    {
                        //格式化产品
                        IEnumerable<object> orderProduct = buyProducts
                        .Where(thisPro => thisPro.GoodsOrderId == order.Id)
                        .Select(thisPro => new
                        {
                            name = thisPro.GoodName,
                            attr = thisPro.SpecInfo,
                            count = thisPro.Count,
                            no = orgProducts.FirstOrDefault(item => item.id == thisPro.FoodGoodsId)?.StockNo,
                            price = ((thisPro.NotDiscountPrice > 0 ? thisPro.NotDiscountPrice : thisPro.Price) * 0.01).ToString("0.00"),
                            total = ((thisPro.NotDiscountPrice > 0 ? thisPro.NotDiscountPrice : thisPro.Price) * thisPro.Count * 0.01).ToString("0.00"),
                        });
                        //获取店铺名（兼容旧数据）
                        string storeName = string.Empty;
                        //产品总数量
                        int buyCount = buyProducts.Where(thisPro => thisPro.GoodsOrderId == order.Id).Sum(thisPro => thisPro.Count);
                        if (!string.IsNullOrWhiteSpace(order.attribute))
                        {
                            try { order.attrbuteModel = JsonConvert.DeserializeObject<EntGoodsOrderAttr>(order.attribute); } catch { }
                        }
                        storeName = order.attrbuteModel.zqStoreName;
                        if (string.IsNullOrWhiteSpace(storeName))
                        {
                            storeName = StoreBLL.SingleModel.GetStoreNameEnt(StoreBLL.SingleModel.GetModelByRid(order.StoreId));
                        }
                        //格式化订单
                        return new
                        {
                            orderno = order.OrderNum,
                            date = order.CreateDate.ToString(),
                            getWay = order.GetWayStr,
                            state = order.StateStr,
                            store = storeName,
                            buyer = order.AccepterName,
                            contact = order.AccepterTelePhone,
                            address = order.Address,
                            mark = order.Message,
                            products = orderProduct,
                            discount = order.ReducedPriceStr,
                            total = order.OnlyGoodsMoney,
                            freight = order.FreightPriceStr,
                            pay = order.BuyPriceStr,
                            buycount = buyCount
                        };
                    });
                    break;
                case (int)PrintContentType.打印专业版砍价订单:
                    //砍价订单ID
                    List<int> bargainIds = content.ConvertToIntList(',');
                    //砍价订单
                    List<BargainUser> bargainOrder = BargainUserBLL.SingleModel.GetJoinList($"a.Id in ({string.Join(",", bargainIds)})", 99, 1, "ID DESC");
                    //砍价详情
                    List<Bargain> bargains = bargainOrder?.Count > 0 ? BargainBLL.SingleModel.GetList($"id in ({string.Join(",", bargainOrder.Select(order => order.BId))})") : null;
                    //格式化打印数据
                    string getWayStr = "快递配送";
                    string storeNameStr = string.Empty;
                    printContent = bargainOrder?.Select((item) =>
                    {
                        Bargain bargainInfo = bargains.FirstOrDefault(thisItem => thisItem.Id == item.BId);
                        //格式化砍价详情
                        object product = new
                        {
                            name = bargainInfo.BName,
                            attr = "无",
                            count = 1,
                            price = bargainInfo.OriginalPriceStr,
                            total = bargainInfo.OriginalPriceStr,
                        };
                        //格式化砍价订单
                        storeNameStr = StoreBLL.SingleModel.GetStoreNameEnt(StoreBLL.SingleModel.GetModelByRid(bargainInfo.StoreId));
                        if (item.GetWay != 0)
                        {
                            storeNameStr = item.StoreName;
                        }
                        if (item.GetWay == 1)
                        {
                            getWayStr = "到店自取";
                        }
                        if (item.GetWay == 2)
                        {
                            getWayStr = "到店消费";
                        }
                        return new
                        {
                            orderno = item.OrderId,
                            date = item.CreateOrderTime.ToString(),
                            getWay = getWayStr,
                            state = item.StateStr,
                            buyer = item.AddressUserName,
                            contact = item.TelNumber,
                            address = item.AddressDetail,
                            mark = item.Remark,
                            store = storeNameStr,
                            products = new object[] { product },
                            discount = ((bargainInfo.OriginalPrice - item.CurrentPrice) * 0.01).ToString("0.00"),
                            total = item.CurrentPriceStr,
                            freight = item.FreightFee * 0.01,
                            pay = item.PayAmount,
                            buycount = 1
                        };
                    });
                    break;
            }

            return View(printContent);
        }

        [LoginFilter(parseAuthDataTo: "authData")]
        [RouteAuthCheck]
        public ActionResult PaidContentRecord(int appId = 0, XcxAppAccountRelation authData = null)
        {
            ViewBag.appId = authData.Id;
            ViewBag.PageType = XcxTemplateBLL.SingleModel.GetModel(authData.TId)?.Type;
            return View();
        }

        [HttpPost]
        [LoginFilter(parseAuthDataTo: "authData")]
        public ActionResult PaidContentRecord(int appId = 0, XcxAppAccountRelation authData = null,
            DateTime? queryStart = null, DateTime? queryEnd = null, string contact = null, string title = null, int pageIndex = 1, int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(contact))
            {
                contact = StringHelper.ReplaceSqlKeyword(contact);
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                title = StringHelper.ReplaceSqlKeyword(title);
            }
            List<PaidContentRecord> records = PaidContentRecordBLL.SingleModel.GetListByPara(appId: authData.Id, title: title, contact: contact, queryStart: queryStart, queryEnd: queryEnd, pageIndex: pageIndex, pageSize: pageSize);
            int recordCount = 0;
            if (pageIndex == 1)
            {
                recordCount = PaidContentRecordBLL.SingleModel.GetCountByPara(appId: authData.Id, title: title, contact: contact, queryStart: queryStart, queryEnd: queryEnd);
            }
            return ReturnJson(isOk: true, message: "获取成功", data: new { records = records, total = recordCount });
        }

        [HttpGet, LoginFilter(parseAuthDataTo: "authData"), RouteAuthCheck]
        public ActionResult Account(int appId = 0, XcxAppAccountRelation authData = null)
        {
            return View();
        }



        [RouteAuthCheck]
        public ActionResult FullReduction(int appId = 0, int pageType = 22)
        {
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });


            ViewBag.appId = appId;
            ViewBag.PageType = pageType;
            return View(new FullReduction()
            {
                Aid = appId,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(8)

            });

        }

        [HttpPost]
        public ActionResult SaveFullReduction(FullReduction fullReduction)
        {
            Return_Msg returnObj = new Return_Msg();
            if (fullReduction.Aid <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            int actionType = Context.GetRequestInt("actionType", 0);
            if (actionType == -1)
            {
                if (fullReduction.Id <= 0)
                {
                    returnObj.Msg = "数据不存在";
                    return Json(returnObj);
                }
                fullReduction = FullReductionBLL.SingleModel.GetModel(fullReduction.Id);
                if (fullReduction == null)
                {
                    returnObj.Msg = "数据库里不存在";
                    return Json(returnObj);
                }
                fullReduction.IsDel = -1;
                //表示删除
                if (FullReductionBLL.SingleModel.Update(fullReduction, "IsDel"))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "删除成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "删除异常";
                    return Json(returnObj);
                }



            }

            if (DateTime.Compare(fullReduction.StartTime, fullReduction.EndTime) > 0)
            {
                returnObj.Msg = "开始时间不能晚于结束时间";
                return Json(returnObj);
            }

            if (FullReductionBLL.SingleModel.HaveFullReductionByTime(fullReduction.StartTime, fullReduction.EndTime, fullReduction.Id, fullReduction.Aid))
            {
                returnObj.Msg = "已存在该时间段范围内的优惠活动了";
                return Json(returnObj);
            }

            if (fullReduction.Id > 0)
            {
                //表示修改
                if (FullReductionBLL.SingleModel.Update(fullReduction))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "修改成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "更新异常";
                    return Json(returnObj);
                }
            }
            else
            {
                //表示新增
                int id = Convert.ToInt32(FullReductionBLL.SingleModel.Add(fullReduction));
                if (id > 0)
                {
                    returnObj.isok = true;
                    returnObj.Msg = "新增成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "新增异常";
                    return Json(returnObj);
                }
            }


        }


        public ActionResult GetListFullReduction()
        {
            Return_Msg returnObj = new Return_Msg();
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);

            if (appId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int totalCount = 0;
            List<FullReduction> list = FullReductionBLL.SingleModel.GetListFullReduction(appId, out totalCount, pageIndex, pageSize);
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj);


        }

        [LoginFilter(parseAuthDataTo: "authData")]
        public ActionResult Checkout(XcxAppAccountRelation authData, int pageIndex = 1, int pageSize = 15, string tradeNo = null, DateTime? start = null, DateTime? end = null)
        {
            int recordCount = 0;
            List<CityMorders> orders = new CityMordersBLL().GetCheckOutOrder(authData.AppId, ref recordCount, pageIndex, pageSize, tradeNo: tradeNo, start: start, end: end);
            return View(Tuple.Create(recordCount, orders));
        }

        [LoginFilter(parseAuthDataTo: "authData")]
        public ActionResult LabelPrinter(XcxAppAccountRelation authData, int appId, string content = null)
        {
            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(authData.Id);
            Dictionary<int, int> printSwitch = new Dictionary<int, int>
            {
                { (int)TmpType.小程序餐饮模板, (int)PrintContentType.打印餐饮版粘贴标签 }
            };
            int printType;
            if (!printSwitch.TryGetValue(pageType, out printType) || string.IsNullOrWhiteSpace(content))
            {
                return View("Error");
            }

            object printContent = null;
            switch (printType)
            {
                case (int)PrintContentType.打印餐饮版粘贴标签:
                    List<FoodGoodsCart> cartModelList = FoodGoodsCartBLL.SingleModel.GetOrderDetail(int.Parse(content));
                    FoodGoodsOrder order = FoodGoodsOrderBLL.SingleModel.GetModel(int.Parse(content));
                    printContent = new
                    {
                        OrderId = order.OrderNum,
                        Item = cartModelList.Select(item =>
                        {
                            List<string> Spec = item.SpecInfo.SplitStr(" ").Select(spec => spec.SubstringSpecial(spec.IndexOf(":") + 1, spec.Length)).ToList();
                            return new
                            {
                                Title = item.goodsMsg.GoodsName,
                                Spec = Spec,
                                Price = item.Price,
                                OrigPrice = item.originalPrice,
                                Amount = item.Count,
                                Date = item.CreateDate.ToString("MM-dd HH:MM")
                            };
                        }),
                    };
                    break;
            }

            return View(printContent);
        }
    }
}