using BLL.MiniApp;
using System.Collections.Generic;
using System.Web.Mvc;
using Core.MiniApp;
using System;
using Entity.MiniApp;
using Utility;
using Newtonsoft.Json;
using Utility.IO;
using DAL.Base;

namespace User.MiniApp.Controllers
{
    public class dadamanageController : baseController
    {
        private DadaOrderBLL _dadaOrderBll;
        
        
        private readonly DadaApi _dadaApi;

        /// <summary>
        /// 实例化对象
        /// </summary>
        public dadamanageController()
        {
            _dadaApi = new DadaApi();
            _dadaOrderBll = new DadaOrderBLL();
            
        }

        #region 测试
        public ActionResult Test()
        {
            string ip = WebHelper.GetIP();
            if (ip != "121.33.238.82")
            {
                return View("PageError", new Return_Msg() { Msg = ip + "IP不对!", code = "404" });
            }

           // object tempceshi = new { client_id = "272445895350440", order_id = "123456780", order_status = 5, cancel_reason = "没有配送员接单", cancel_from = 2, update_time = 1519885778, signature = "08baa67b531ddf656209f15499b8d6ed", dm_id = 666, dm_name = "测试达达", dm_mobile = "13546670420" };
           //string signature = _dadaapi.GetSignatureR(tempceshi);

            return View();
        }

        /// <summary>
        /// 注册商户
        /// </summary>
        /// <returns></returns>
        public ActionResult RegistMerchant()
        {
            DadaMerchant data = new DadaMerchant() { city_name = "上海", contact_name = "测试", contact_phone = "18718463808", email = "caihuaxing@yeah.net", enterprise_address = "浦东新区", enterprise_name = "点赞科技", mobile = "18718463808" };
            //object data = new { city_name = "广州", contact_name = "测试", contact_phone = "18718463808", email = "caihuaxing@yeah.net", enterprise_address = "天河区", enterprise_name = "点赞科技", mobile = "18718463808"};
            //object data = new { contact_name = "agent" , enterprise_name="上海", enterprise_address = "天河区", city_name = "ceshi", contact_phone = "18718463808", email = "caihuaxing@yeah.net", mobile = "18718463808"};
            _dadaApi._sourceid = "";
            string json = _dadaApi.PostParamJson(data);
            string url = _dadaApi._merchantapi;
            string result = WxHelper.DoPostJson(url, json);

            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 创建门店
        /// </summary>
        /// <returns></returns>
        public ActionResult AddShop()
        {
            string sourceid = Context.GetRequest("sourceid", "73753");
            _dadaApi._sourceid = sourceid;
            List<DadaShop> data = new List<DadaShop>() { new DadaShop(){
                station_name = "花门店3",
        origin_shop_id= "hua003",
        area_name="天河区",
        station_address= "广州",
        contact_name= "xxx",
        city_name= "广州",
        business= 1,
        lng= 121.515014,
        phone= "13012345678",
        lat= 31.229081},
        new DadaShop(){
            station_name= "花门店4",
        origin_shop_id= "hua004",
        area_name= "天河区",
        station_address= "广州",
        contact_name= "xxx",
        business= 1,
        lng= 121.515014,
        city_name= "广州",
        phone= "13012345678",
        lat= 31.229081,
        username= "13012345678"
        }
            };

            string json = _dadaApi.PostParamJson(data);
            string url = _dadaApi._shopapi;
            string result = WxHelper.DoPostJson(url, json);

            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <returns></returns>
        public ActionResult AddOrder()
        {
            string sourceid = Context.GetRequest("sourceid", "73753");
            string shop_no = Context.GetRequest("shop_no", "11047059");
            string order_id = Context.GetRequest("order_id", "");
            int raddorder = Context.GetRequestInt("raddorder", 0);

            _dadaApi._sourceid = sourceid;
            _dadaApi._shop_no = shop_no;

            DadaOrder data = new DadaOrder() { shop_no = _dadaApi._shop_no, origin_id = order_id, city_code ="123456", cargo_price =20.01f, is_prepay =1, expected_fetch_time =_dadaApi.GetTimeStamp(), receiver_name ="ceshi", receiver_address ="ceshi", receiver_lat =20.12548, receiver_lng =120.548565, callback = _dadaApi._ordercallback, insurance_fee=0, receiver_phone="18718463809", receiver_tel="18718463809", tips=1, info= "测试", cargo_type=1, cargo_weight =1.1, cargo_num =1, invoice_title ="", deliver_locker_code ="", pickup_locker_code ="", origin_mark ="", origin_mark_no ="", is_finish_code_needed =0, delay_publish_time =0};

            string json = _dadaApi.PostParamJson(data);
            string url = _dadaApi._addorderapi;
            if(raddorder==1)
            {
                url = _dadaApi._readdorderapi;
            }
            else if(raddorder==2)
            {
                url = _dadaApi._querydeliverfeeorderapi;
            }
            string result = WxHelper.DoPostJson(url, json);
            if(!string.IsNullOrEmpty(result))
            {
                DadaApiReponseModel<ResultReponseModel> reposemodel = JsonConvert.DeserializeObject<DadaApiReponseModel<ResultReponseModel>>(result);
                if(raddorder==2)
                {
                    RedisUtil.Set("dada_key_"+ order_id, reposemodel.result.deliveryNo, TimeSpan.FromMinutes(3));
                }
            }
            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 追加订单
        /// </summary>
        /// <returns></returns>
        public ActionResult AppendOrder()
        {
            string order_id = Context.GetRequest("order_id", "");
            string shop_no = Context.GetRequest("shop_no", "11047059");
            object data = new { shop_no= shop_no, transporter_id=18,order_id= order_id };
            string url = _dadaApi._appendorderapi;

            string json = _dadaApi.PostParamJson(data);
            string result = WxHelper.DoPostJson(url, json);

            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 添加消费
        /// </summary>
        /// <returns></returns>
        public ActionResult AddTip()
        {
            string order_id = Context.GetRequest("order_id", "");
            decimal tips = Convert.ToDecimal(Context.GetRequest("tips","0"));
            string city_code = Context.GetRequest("city_code", "");
            object data = new { order_id = order_id, tips = tips, city_code = city_code };
            string url = _dadaApi._addtip;

            string json = _dadaApi.PostParamJson(data);
            string result = WxHelper.DoPostJson(url, json);

            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询追加配送员
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTransporterList()
        {
            string shop_no = Context.GetRequest("shop_no", "11047059");
            object data = new { shop_no = shop_no };
            string url = _dadaApi._transporterapi;

            string json = _dadaApi.PostParamJson(data);
            string result = WxHelper.DoPostJson(url, json);

            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改订单状态
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateOrderState()
        {
            string order_id = Context.GetRequest("order_id", "");
            int merchantid = Context.GetRequestInt("merchantid", 0);
            int state = Context.GetRequestInt("state", -1);
            int cancel_reason_id = Context.GetRequestInt("cancel_reason_id", 0);
            string cancel_reason = Context.GetRequest("cancel_reason", string.Empty);
            
            if(state==-1)
            {
                return Json(new { isok=false,msg="订单状态出错"}, JsonRequestBehavior.AllowGet);
            }

            object data =new object();
            string url = string.Empty;
            switch (state)
            {
                case 0://取消订单
                    data = new { order_id = order_id, cancel_reason_id = 1, cancel_reason = cancel_reason };
                    url = _dadaApi._cancelorderapi;
                    break;
                case 1://查询订单
                    data = new { order_id = order_id };
                    url = _dadaApi._orderqueryapi;
                    break;
                case 2://接收订单
                    data = new { order_id = order_id };
                    url = _dadaApi._acceptorderapi;
                    break;
                case 3://完成取货
                    data = new { order_id = order_id };
                    url = _dadaApi._fetchgoodapi;
                    break;
                case 4://完成订单
                    data = new { order_id = order_id };
                    url = _dadaApi._finishorderapi;
                    break;
                case 5://订单过期
                    data = new { order_id = order_id };
                    url = _dadaApi._expireorderapi;
                    break;
                case 6://取消追加订单
                    data = new { order_id = order_id };
                    url = _dadaApi._cancelappendorderapi;
                    break;
                case 7://执行预发单后发单
                    string deliveryNo = RedisUtil.Get<string>("dada_key_" + order_id);
                    data = new { deliveryNo = deliveryNo };
                    url = _dadaApi._addafterqueryorderapi;
                    break;
            }

            string json = _dadaApi.PostParamJson(data);
            string result = WxHelper.DoPostJson(url, json);
            //if(!string.IsNullOrEmpty(result))
            //{
            //    DadaApiReponseModel<ResultReponseModel> resultmodel = JsonConvert.DeserializeObject<DadaApiReponseModel<ResultReponseModel>>(result);
            //    return Json(new { isok = true, msg = resultmodel.msg }, JsonRequestBehavior.AllowGet);
            //}

            return Json(new { isok = false, msg = result }, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 获取取消订单原因
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelOrderReasons()
        {
            object data = "";
            string json = _dadaApi.PostParamJson(data);
            string url = _dadaApi._cancelorderreasonsapi;
            string result = WxHelper.DoPostJson(url, json);
            return Json(new { isok = true, dada = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询城市
        /// </summary>
        /// <returns></returns>
        public ActionResult CityCodeList()
        {
            object data = "";
            string json = _dadaApi.PostParamJson(data);
            log4net.LogHelper.WriteInfo(this.GetType(), json);

            string url = _dadaApi._citycodelistapi;
            log4net.LogHelper.WriteInfo(this.GetType(), url);

            string result = WxHelper.DoPostJson(url, json);
            //string result = HttpHelper.DoPostJson(url, json);
            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult notis(OrderReponseModel orderform)
        {
            log4net.LogHelper.WriteInfo(this.GetType(), "达达配送回调json:" + JsonConvert.SerializeObject(orderform));
            object ten3 = new { client_id = orderform.client_id, order_id = orderform.order_id, update_time = orderform.update_time };
            string signature = _dadaApi.GetSignatureR(ten3);
            if(orderform.signature != signature)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "达达配送回调签名错误json:" + JsonConvert.SerializeObject(orderform));
            }
            else
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "达达配送回调签名正确json:" + JsonConvert.SerializeObject(orderform));
            }
            
            return Content("success");
        }
        #endregion

        public ActionResult managerconfig()
        {
            string ip = WebHelper.GetIP();
            if (ip != "121.33.238.82")
            {
                return View("PageError", new Return_Msg() { Msg = ip + "IP不对!", code = "404" });
            }

            //int appId = Context.GetRequestInt("appId", 0);
            //int Id = Context.GetRequestInt("Id", 0);
            //int type = Context.GetRequestInt("type", 0);

            ////商户号
            //DadaMerchant merchant = new DadaMerchant();
            //DadaRelation relationmodel = _dadarelationBll.GetModelByRid(appId);
            //if(relationmodel!=null)
            //{
            //    merchant = _dadamerchantBll.GetModel(relationmodel.merchantid);
            //}

            ////门店
            //List<DadaShop> shoplist = new List<DadaShop>();
            //if(merchant.id>0)
            //{
            //    shoplist = _dadashopBll.GetList();
            //}

            return View();
        }

        public ActionResult SaveData()
        {
            string merchantid = Context.GetRequest("merchantid", string.Empty);
            string companyname = Context.GetRequest("companyname", string.Empty);
            int rid = Context.GetRequestInt("rid", 0);
            string shopno = Context.GetRequest("shopno", string.Empty);
            if(string.IsNullOrEmpty(merchantid))
            {
                return Json(new { isok = false, msg = "商户号不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(companyname))
            {
                return Json(new { isok = false, msg = "企业名称不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(shopno))
            {
                return Json(new { isok = false, msg = "店铺编号不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (rid<=0)
            {
                return Json(new { isok = false, msg = "权限表ID不能小于0" }, JsonRequestBehavior.AllowGet);
            }

            DadaRelation dadarelation = DadaRelationBLL.SingleModel.GetModelByRid(rid);
            if(dadarelation!=null)
            {
                return Json(new { isok = false, msg = "该权限ID已经有关联商户号" }, JsonRequestBehavior.AllowGet);
            }

            DadaMerchant dadamerchant = DadaMerchantBLL.SingleModel.GetModelByMId(merchantid);
            if(dadamerchant!=null)
            {
                return Json(new { isok = false, msg = "该商户号已经存在" }, JsonRequestBehavior.AllowGet);
            }
            dadamerchant = new DadaMerchant() { sourceid = merchantid};

            DadaShop dadashop = DadaShopBLL.SingleModel.GetModelByShopNo(shopno);
            if (dadashop != null)
            {
                return Json(new { isok = false, msg = "该门店已经存在" }, JsonRequestBehavior.AllowGet);
            }

            int mid = Convert.ToInt32(DadaMerchantBLL.SingleModel.Add(dadamerchant)); 
            if(mid>0)
            {
                dadarelation = new DadaRelation() { merchantid = mid,rid=rid,state = 0,addtime=DateTime.Now};
                int relationid = Convert.ToInt32(DadaRelationBLL.SingleModel.Add(dadarelation));

                dadashop = new DadaShop() { origin_shop_id = shopno,merchantid = mid };
                int shopid = Convert.ToInt32(DadaShopBLL.SingleModel.Add(dadashop));
                if(relationid>0 && shopid>0)
                {
                    return Json(new { isok = true, msg = "保存成功" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isok=false,msg="保存失败"},JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取取消订单原因
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCancelOrderReasons()
        {
            string merchantid = Context.GetRequest("merchantid",string.Empty);
            _dadaApi._sourceid = merchantid;
            return Json(new { isok = true, dada = _dadaApi.GetCancelOrderReasonList() }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 修改订单状态
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateDadaOrderState()
        {
            string order_id = Context.GetRequest("order_id", "");
            string merchantid = Context.GetRequest("merchantid", string.Empty);
            int state = Context.GetRequestInt("state", -1);
            int cancel_reason_id = Context.GetRequestInt("cancel_reason_id", 0);
            string cancel_reason = Context.GetRequest("cancel_reason", string.Empty);

            if (string.IsNullOrEmpty(merchantid))
            {
                return Json(new { isok = false, msg = "找不到商户号" }, JsonRequestBehavior.AllowGet);
            }
            if (state == -1)
            {
                return Json(new { isok = false, msg = "订单状态出错" }, JsonRequestBehavior.AllowGet);
            }

            //DadaOrderRelation orelationmodel = _dadaOrderRelationBll.GetModelUOrderNo(order_id);
            //if (orelationmodel == null)
            //{
            //    return Json(new { isok = false, msg = "找不到系统订单，请刷新重试" }, JsonRequestBehavior.AllowGet);
            //}

            DadaOrder ordermodel = _dadaOrderBll.GetModelByOrderNo(order_id);
            if (ordermodel == null || !(ordermodel.state == (int)DadaOrderEnum.待接单 || ordermodel.state == (int)DadaOrderEnum.待取货) )
            {
                return Json(new { isok = false, msg = "找不到系统订单，请刷新重试" }, JsonRequestBehavior.AllowGet);
            }

            _dadaApi._sourceid = merchantid;
            object data = new object();
            string url = string.Empty;
            switch (state)
            {
                case 0://取消订单
                    data = new { order_id = order_id, cancel_reason_id = cancel_reason_id, cancel_reason = cancel_reason };
                    url = _dadaApi._cancelorderapi;
                    break;
            }

            string json = _dadaApi.PostParamJson(data);
            string result = WxHelper.DoPostJson(url, json);
            if (!string.IsNullOrEmpty(result))
            {
                DadaApiReponseModel<ResultReponseModel> resultmodel = JsonConvert.DeserializeObject<DadaApiReponseModel<ResultReponseModel>>(result);
                if(resultmodel.status=="success")
                {
                    ordermodel.state = (int)DadaOrderEnum.取消中;
                    ordermodel.update_time = DateTime.Now;
                    if(!_dadaOrderBll.Update(ordermodel, "state"))
                    {
                        return Json(new { isok = false, msg = "更新订单状态失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { isok = true, msg = resultmodel.msg }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = false, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
        }
    }
}