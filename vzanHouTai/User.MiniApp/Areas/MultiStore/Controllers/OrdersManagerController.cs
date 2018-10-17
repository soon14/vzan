using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Controllers;
using User.MiniApp.Areas.MultiStore.Filters;
using Entity.MiniApp.Conf;
using Utility.IO;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using Entity.MiniApp.Footbath;
using BLL.MiniApp.Footbath;
using Entity.MiniApp.Ent;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using BLL.MiniApp.Ent;
using Utility;
using DAL.Base;
using System.Data;
using BLL.MiniApp.Helper;
using Core.MiniApp;
using Newtonsoft.Json;
using Entity.MiniApp.User;

namespace User.MiniApp.Areas.MultiStore.Controllers
{
    public class OrdersManagerController : baseController
    {
        


        public OrdersManagerController()
        {
            

        }
        // GET: MultiStore/OrdersManager
        public ActionResult Index()
        {
            return View();
        }
        [LoginFilter]
        public ActionResult OrderTotalList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            string sqlwhere = $"appid={appId}";
            if (storeId > 0)
            {
                sqlwhere = $"id={storeId}";
            }
            if (appId == 0)
            {
                return Content("系统繁忙id_null");
            }
            if (dzaccount == null)
            {
                return Content("系统繁忙auth_null");
            }
            var xcx = XcxAppAccountRelationBLL.SingleModel.GetModel($"id ={appId}");

            if (xcx == null)
            {
                return Content("系统繁忙app_null");
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel(sqlwhere);
            if (storeModel == null)
            {
                return Content("系统繁忙store_null");
            }
            ViewBag.storeId = storeModel.Id;
            List<FootBath> storeList = new List<FootBath>();
            storeList.Add(storeModel);
            if (storeModel.HomeId == 0)
            {
                storeList.AddRange(FootBathBLL.SingleModel.GetList($"HomeId={storeModel.Id} and state>0 and IsDel>-1"));
            }
            List<VipLevel> levelList = VipLevelBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            ViewBag.storeList = storeList;
            return View(levelList);
        }

        /// <summary>
        /// 获取订单列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderList()
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
            var appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel($"id ={appId}");
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            int storeId = Context.GetRequestInt("storeid", -1);
            if (storeId < 0)
            {
                return Json(new { isok = false, msg = "系统繁忙storeId_null" });
            }
            string storeSqlwhere = $"appid={appId} and homeid=0";
            if (storeId > 0)
            {
                storeSqlwhere = $"id={storeId}";
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel(storeSqlwhere);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            int tab = Context.GetRequestInt("tab", 0);
            if (tab <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙tab_null" });
            }
            string orderNum = Context.GetRequest("orderNum", string.Empty);
            string nickName = Context.GetRequest("accName", string.Empty);
            string phone = Context.GetRequest("accPhone", string.Empty);
            int buyMode = Context.GetRequestInt("buyMode", 0);
            string goodsName = Context.GetRequest("goodsName", string.Empty);
            int levelid = Context.GetRequestInt("levelid", 0);
            int orderState = Context.GetRequestInt("orderState", -999);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string startdate = Context.GetRequest("startdate", string.Empty);
            string enddate = Context.GetRequest("enddate", string.Empty);

            List<EntGoodsOrder> OrderList = null;
            List<MySqlParameter> parameters = null;
            string sqlwhere = $" getway={tab}";
            string storeids = string.Empty;
            if (storeId > 0)
            {
                storeids = storeId.ToString();
                sqlwhere += $" and storeId={storeId}";
            }
            else
            {
                List<FootBath> storeList = FootBathBLL.SingleModel.GetList($"HomeId={storeModel.Id} and state>0 and IsDel>-1");
                if (storeList == null || storeList.Count <= 0)
                {
                    storeids = storeModel.Id.ToString();
                    sqlwhere += $" and storeId={storeModel.Id}";
                }
                else
                {
                    string ids = string.Join(",", storeList.Select(s => s.Id));
                    storeids = $"{storeModel.Id},{ids}";
                    sqlwhere += $" and storeId in ({storeModel.Id},{ids})";
                }

            }

            int recordCount = 0;
            List<string> userUidList = new List<string>();
            if (!string.IsNullOrEmpty(nickName))
            {
                parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter("@NickName", $"%{nickName}%"));

                List<C_UserInfo> userList = C_UserInfoBLL.SingleModel.GetListByParam($"nickname like @NickName and appId='{appAcountRelation.AppId}'", parameters.ToArray());
                if (userList == null || userList.Count <= 0)
                {
                    return Json(new { isok = true, OrderList = OrderList, recordCount = recordCount });
                }
                userUidList = userList.Select(s => s.Id.ToString()).ToList();
                if (userUidList == null || userUidList.Count <= 0)
                {
                    return Json(new { isok = true, OrderList = OrderList, recordCount = recordCount });
                }
            }
            List<string> levelUidList = new List<string>();
            if (levelid > 0)
            {
                List<VipRelation> relationList = VipRelationBLL.SingleModel.GetList($"levelid={levelid}");
                if (relationList == null || relationList.Count <= 0)
                {
                    return Json(new { isok = true, OrderList = OrderList, recordCount = recordCount });
                }
                levelUidList = relationList.Select(s => s.uid.ToString()).ToList();
                if (levelUidList == null || levelUidList.Count <= 0)
                {
                    return Json(new { isok = true, OrderList = OrderList, recordCount = recordCount });
                }
            }
            List<string> uidList = new List<string>();

            if (levelUidList.Count > 0 || userUidList.Count > 0)
            {
                if (levelUidList.Count > 0 && levelUidList.Count > 0)
                {
                    uidList = levelUidList.Intersect(levelUidList).ToList();
                }
                else if (levelUidList.Count > 0)
                {
                    uidList = levelUidList;
                }
                else if (userUidList.Count > 0)
                {
                    uidList = userUidList;
                }
            }
            if (uidList.Count > 0)
            {

                sqlwhere += $" and UserId in ({string.Join(",", uidList)})";
            }

            if (!string.IsNullOrEmpty(goodsName))
            {
                List<FootBath> stores = FootBathBLL.SingleModel.GetList($" id in ({storeids})");
                string aids = string.Join(",", stores.Select(s => s.appId).ToList());
                parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter("@name", $"%{goodsName}%"));
                List<EntGoods> goodsList = EntGoodsBLL.SingleModel.GetListByParam($" aid in ({aids}) and state> 0 and name like @name", parameters.ToArray());
                if (goodsList == null || goodsList.Count <= 0)
                {
                    return Json(new { isok = true, OrderList = OrderList, recordCount = recordCount, msg = "name_null" });
                }
                List<string> typeidSplit = goodsList.Select(g => g.id.ToString()).ToList();
                if (typeidSplit.Count > 0)
                {
                    typeidSplit = typeidSplit.Select(p => p = "FIND_IN_SET('" + p + "',GoodsGuid)").ToList();
                    sqlwhere += $" and (" + string.Join(" or ", typeidSplit) + ")";
                }
            }


            parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(orderNum))
            {
                sqlwhere += $" and orderNum like @orderNum";
                parameters.Add(new MySqlParameter("@orderNum", $"%{orderNum}%"));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                sqlwhere += $" and AccepterTelePhone like @phone";
                parameters.Add(new MySqlParameter("@phone", $"%{phone}%"));
            }
            if (!string.IsNullOrEmpty(startdate))
            {
                sqlwhere += $" and CreateDate >= @startdate";
                parameters.Add(new MySqlParameter("@startdate", $"{startdate} 00:00:00"));
            }
            if (!string.IsNullOrEmpty(enddate))
            {
                sqlwhere += $" and CreateDate <= @enddate";
                parameters.Add(new MySqlParameter("@enddate", $"{enddate} 23:59:59"));
            }
            if (buyMode > 0)
            {
                sqlwhere += $" and buyMode={buyMode}";
            }
            if (orderState > -999)
            {
                if (orderState == -2)
                {
                    sqlwhere += $" and State in({(int)MiniAppEntOrderState.退款中},{(int)MiniAppEntOrderState.退款成功},{(int)MiniAppEntOrderState.退款失败})";
                }
                else
                {
                    sqlwhere += $" and State={orderState}";
                }
            }
            List<EntGoodsOrder> orderList = EntGoodsOrderBLL.SingleModel.GetListByParam(sqlwhere, parameters.ToArray(), pageSize, pageIndex,"*","id desc");
            //log4net.LogHelper.WriteInfo(this.GetType(), sqlwhere);
            if (orderList != null && orderList.Count > 0)
            {
                string bathIds = string.Join(",",orderList.Select(s=>s.StoreId).Distinct());
                List<FootBath> footBathList = FootBathBLL.SingleModel.GetListByIds(bathIds);

                string userIds = string.Join(",",orderList.Select(s=>s.UserId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                string orderIds = string.Join(",", orderList.Select(s=>s.Id));
                List<EntGoodsCart> entGoodsCartList = EntGoodsCartBLL.SingleModel.GetListByOrderIds(orderIds);

                string goodsIds = string.Join(",",entGoodsCartList?.Select(s=>s.FoodGoodsId).Distinct());
                List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);

                orderList.ForEach(o =>
                {
                    FootBath store = footBathList?.FirstOrDefault(f=>f.Id == o.StoreId);
                    if (store != null)
                    {
                        UserRole userrole = UserRoleBLL.SingleModel.GetModel($"storeid={store.Id} and appid={store.appId} and state>0");
                        if (userrole != null)
                        {
                            o.isSelfOrder = userrole.UserId == dzaccount.Id;
                        }
                        o.storeName = store.StoreName;
                    }
                    C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id == o.UserId);
                    string sql = $"select name from viprelation as a left join viplevel as b on a.levelid = b.id where a.uid={o.UserId}";
                    o.vipLevelName = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(), CommandType.Text, sql).ToString();//获取会员等级
                    if (userInfo != null)
                    {
                        o.nickName = userInfo.NickName;
                    }
                    //获取购买的商品
                    o.goodsCarts = entGoodsCartList?.Where(w=>w.GoodsOrderId == o.Id).ToList();
                    if (o.goodsCarts != null && o.goodsCarts.Count > 0)
                    {
                        o.goodsNames = string.Empty;
                        foreach (var goodsCart in o.goodsCarts)
                        {
                            EntGoods goods = entGoodsList?.FirstOrDefault(f=>f.id == goodsCart.FoodGoodsId);
                            if (goods == null)
                            {
                                continue;
                            }
                            goodsCart.GoodName = goods.name;
                            if (!string.IsNullOrEmpty(goodsCart.SpecInfo))
                            {
                                o.goodsNames += $"{goods.name}({goodsCart.SpecInfo}),";
                            }
                            else
                            {
                                o.goodsNames += $"{goods.name},";
                            }
                            o.goodsNames = o.goodsNames.TrimEnd(',');
                        }
                    }
                });
            }
            
            //判断是否有团订单
            EntGroupSponsorBLL.SingleModel.GetSponsorState(ref orderList);

            recordCount = EntGoodsOrderBLL.SingleModel.GetCount(sqlwhere, parameters.ToArray());
            return Json(new { isok = true, orderList = orderList, recordCount = recordCount });
        }

        /// <summary>
        /// 修改订单状态
        /// </summary>
        /// <returns></returns>
        public ActionResult updateState()
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
            var appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel($"id ={appId}");
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            int storeId = Context.GetRequestInt("storeid", -1);
            if (storeId < 0)
            {
                return Json(new { isok = false, msg = "系统繁忙storeId_null" });
            }
            string storeSqlwhere = $"appid={appId}";
            if (storeId > 0)
            {
                storeSqlwhere = $"id={storeId}";
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel(storeSqlwhere);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            string msg = "操作失败";
            bool isok = false;
            int state = Context.GetRequestInt("state", -999);
            int orderId = Context.GetRequestInt("id", 0);
            if (orderId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙orderId_null" });
            }
            EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModel($"aId={appId} and id={orderId}");
            if (orderInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙order_null" });
            }
            SendTemplateMessageTypeEnum templateType = new SendTemplateMessageTypeEnum();
            switch (state)
            {
                case (int)MiniAppEntOrderState.已取消:
                    templateType = SendTemplateMessageTypeEnum.多门店订单取消通知;
                    orderInfo.State = (int)MiniAppEntOrderState.已取消;
                    break;
                case (int)MiniAppEntOrderState.待接单:
                    templateType = SendTemplateMessageTypeEnum.多门店反馈处理结果通知;
                    orderInfo.State = (int)MiniAppEntOrderState.待接单;
                    break;
                case (int)MiniAppEntOrderState.待配送:
                    templateType = SendTemplateMessageTypeEnum.多门店订单确认通知;
                    orderInfo.State = (int)MiniAppEntOrderState.待配送;
                    break;
                case (int)MiniAppEntOrderState.待确认送达:
                    templateType = SendTemplateMessageTypeEnum.多门店订单配送通知;
                    orderInfo.State = (int)MiniAppEntOrderState.待确认送达;
                    break;
                case (int)MiniAppEntOrderState.交易成功:
                    if (orderInfo.GetWay == (int)multiStoreOrderType.同城配送)
                    {
                        templateType = SendTemplateMessageTypeEnum.多门店订单配送通知;
                    }
                    orderInfo.AcceptDate = DateTime.Now;
                    orderInfo.State = (int)MiniAppEntOrderState.交易成功;
                    break;
                case (int)MiniAppEntOrderState.待收货:
                    templateType = SendTemplateMessageTypeEnum.多门店订单发货提醒;
                    orderInfo.State = (int)MiniAppEntOrderState.待收货;
                    break;
            }
            isok = EntGoodsOrderBLL.SingleModel.Update(orderInfo, "state,AcceptDate");
            if (isok)
            {
                if (state == (int)MiniAppEntOrderState.交易成功)
                {
                    VipRelationBLL.SingleModel.updatelevel(orderInfo.UserId, "multistore");
                }
                msg = "操作成功";
                var data = TemplateMsg_Miniapp.MutilStoreGetTemplateMessageData(orderInfo, templateType);
                TemplateMsg_Miniapp.SendTemplateMessage(orderInfo.UserId, templateType, TmpType.小程序多门店模板, data);
            }
            return Json(new { isok = isok, msg = msg });
        }
        /// <summary>
        /// 发起退款
        /// </summary>
        /// <returns></returns>
        public ActionResult OutOrder()
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
            var appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel($"id ={appId}");
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            int storeId = Context.GetRequestInt("storeid", -1);
            if (storeId < 0)
            {
                return Json(new { isok = false, msg = "系统繁忙storeId_null" });
            }
            string storeSqlwhere = $"appid={appId}";
            if (storeId > 0)
            {
                storeSqlwhere = $"id={storeId}";
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel(storeSqlwhere);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            int orderId = Context.GetRequestInt("orderId", 0);
            if (orderId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙orderId_null" });
            }
            EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModel($"aId={storeModel.appId} and id={orderId}");
            if (orderInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙order_null" });
            }
            List<EntGoodsCart> goodscarts = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderInfo.Id}");
            bool isok = EntGoodsOrderBLL.SingleModel.outOrder(goodscarts, storeModel, orderInfo);
            string msg = "操作失败";
            if (isok)
            {
                msg = "操作成功";
                var data = TemplateMsg_Miniapp.MutilStoreGetTemplateMessageData(orderInfo, SendTemplateMessageTypeEnum.多门店退款通知);
                TemplateMsg_Miniapp.SendTemplateMessage(orderInfo.UserId, SendTemplateMessageTypeEnum.多门店退款通知, TmpType.小程序多门店模板, data);
            }
            return Json(new { isok = isok, msg = msg });
        }

        public void ExportOrder()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                Response.Write("<script>alert('系统繁忙appid_null');window.opener=null;window.close();</script>");
                return;
            }
            if (dzaccount == null)
            {
                Response.Write("<script>alert('系统繁忙auth_null');window.opener=null;window.close();</script>");
                return;
            }
            var appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel($"id ={appId}");
            if (appAcountRelation == null)
            {
                Response.Write("<script>alert('系统繁忙relation_null');window.opener=null;window.close();</script>");
                return;
            }
            int storeId = Context.GetRequestInt("storeid", -1);
            if (storeId < 0)
            {
                Response.Write("<script>alert('系统繁忙storeId_null');window.opener=null;window.close();</script>");
                return;
            }
            string storeSqlwhere = $"appid={appId}";
            if (storeId > 0)
            {
                storeSqlwhere = $"id={storeId}";
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModel(storeSqlwhere);
            if (storeModel == null)
            {
                Response.Write("<script>alert('系统繁忙model_null');window.opener=null;window.close();</script>");
                return;
            }
            string startdate = Context.GetRequest("startdate", string.Empty);
            string enddate = Context.GetRequest("enddate", string.Empty);
            int orderType = Context.GetRequestInt("orderType", 0);
            if (orderType <= 0)
            {
                Response.Write("<script>alert('系统繁忙orderType_null');window.opener=null;window.close();</script>");
                return;
            }
            if (string.IsNullOrEmpty(startdate) || string.IsNullOrEmpty(enddate))
            {
                Response.Write("<script>alert('请选择导出订单时间段');window.opener=null;window.close();</script>");
                return;
            }
            string filename = $"表单导出-{storeModel.StoreName}-{startdate}至{enddate}";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@startdate", $"{startdate} 00:00:00"));
            parameters.Add(new MySqlParameter("@enddate", $"{enddate} 23:59:59"));
            string sqlwhere = $"aid={storeModel.appId} and getway={orderType} and CreateDate>=@startdate and CreateDate<=@enddate";
            if (storeId > 0)
            {
                sqlwhere = $"aid={storeModel.appId} and StoreId={storeModel.Id} and getway={orderType}";
            }
            List<EntGoodsOrder> orderList = EntGoodsOrderBLL.SingleModel.GetListByParam(sqlwhere, parameters.ToArray());
            if (orderList != null && orderList.Count > 0)
            {
                string bathIds = string.Join(",", orderList.Select(s => s.StoreId).Distinct());
                List<FootBath> footBathList = FootBathBLL.SingleModel.GetListByIds(bathIds);

                string userIds = string.Join(",", orderList.Select(s => s.UserId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                string orderIds = string.Join(",", orderList.Select(s => s.Id));
                List<EntGoodsCart> entGoodsCartList = EntGoodsCartBLL.SingleModel.GetListByOrderIds(orderIds);

                string goodsIds = string.Join(",", entGoodsCartList?.Select(s => s.FoodGoodsId).Distinct());
                List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);
                orderList.ForEach(o =>
                {
                    FootBath store = footBathList?.FirstOrDefault(f=>f.Id == o.StoreId);
                    if (store != null)
                    {
                        o.storeName = store.StoreName;
                    }
                    C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id == o.UserId);
                    string sql = $"select * from viprelation as a left join viplevel as b on a.levelid = b.id where a.uid={o.UserId}";
                    o.vipLevelName = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(), CommandType.Text, sql).ToString();//获取会员等级
                    if (userInfo != null)
                    {
                        o.nickName = userInfo.NickName;
                    }
                    //获取购买的商品
                    o.goodsCarts = entGoodsCartList?.Where(w=>w.GoodsOrderId == o.Id).ToList();
                    if (o.goodsCarts != null && o.goodsCarts.Count > 0)
                    {
                        o.goodsNames = string.Empty;
                        foreach (var goodsCart in o.goodsCarts)
                        {
                            EntGoods goods = entGoodsList?.FirstOrDefault(f=>f.id == goodsCart.FoodGoodsId);
                            if (goods == null)
                            {
                                continue;
                            }
                            if (!string.IsNullOrEmpty(goodsCart.SpecInfo))
                            {
                                o.goodsNames += $"{goods.name}({goodsCart.SpecInfo}),";
                            }
                            else
                            {
                                o.goodsNames += $"{goods.name}";
                            }
                        }
                    }
                });
                DataTable exportTable = new DataTable();
                switch (orderType)
                {
                    case (int)multiStoreOrderType.到店自取:
                        exportTable = ExportExcelBLL.GetDaoDianZiquData(orderList);
                        break;
                    case (int)multiStoreOrderType.同城配送:
                        exportTable = ExportExcelBLL.GetTongChengPeisongData(orderList);
                        break;
                    case (int)multiStoreOrderType.快递配送:
                        exportTable = ExportExcelBLL.GetKuaiDiPeisongData(orderList);
                        break;
                }
                ExcelHelper<EntUserForm>.Out2Excel(exportTable, filename);//导出
            }
            else
            {
                Response.Write("<script>alert('查无数据');window.opener=null;window.close();</script>");
                return;
            }
        }
    }
}