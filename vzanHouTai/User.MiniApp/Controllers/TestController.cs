using BLL.MiniApp;
using BLL.MiniApp.cityminiapp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.CrmApi;
using BLL.MiniApp.Ent;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Plat;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Plat;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace User.MiniApp.Controllers
{
    public class TestController : baseController
    {

        
        
        
        
        
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetAPPTest()
        {
            //new PlatStatisticalFlowConfigBLL().RunService();
            return Json(new { isok = true, msg = "ok"}, JsonRequestBehavior.AllowGet);
        }
        // GET: Test
        public ActionResult GetUserIntegral()
        {
            var listUserIntegral = ExchangeUserIntegralBLL.SingleModel.GetList();
            return Json(new { isok = true, msg = "ok", obj = listUserIntegral }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserIntegralLog(int appId = 0, int userId = 0, int orderId = 0)
        {
            string strWhere = string.Empty;
            if (appId > 0 && userId > 0 && orderId > 0)
                strWhere = $"appId={appId} and userId={userId} and orderId={orderId}";

            var listUserIntegralLog = ExchangeUserIntegralLogBLL.SingleModel.GetList(strWhere);
            return Json(new { isok = true, msg = "ok", obj = listUserIntegralLog }, JsonRequestBehavior.AllowGet);

        }
        
        public ActionResult GetExchangeRuleList(int appId = 0, int state = 0)
        {
            string strWhere = $"state={state}";
            if (appId > 0)
                strWhere += $" and appId={appId} ";

            List<ExchangeRule> listRule = ExchangeRuleBLL.SingleModel.GetList(strWhere);
            return Json(new { isok = true, msg = "ok", obj = listRule }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetVipInsteadCardList(string phone, string code)
        {
            string codeKey = $"CITY_BINDPHONE_{phone}";
            RedisUtil.Set<string>(codeKey, code.ToString());
            return Json(new { isok = true, msg = "ok", code = RedisUtil.Get<string>(codeKey) }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetCityMsgList(int aid = 0, int state = 0)
        {
            string strWhere = $"state={state}";
            if (aid > 0)
                strWhere += $" and aid={aid} ";

            List<CityMsg> list = CityMsgBLL.SingleModel.GetList(strWhere);
            return Json(new { isok = true, msg = "ok", obj = list }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult getCity_UserFavoriteMsg(int aid, int msgId, int userId, int actionType)
        {
            var model = CityUserFavoriteMsgBLL.SingleModel.GetModel($"aid={aid} and userId={userId} and msgId={msgId} and state=0 and actionType={actionType}");
            return Json(new { isok = true, msg = "ok", obj = model }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetGoodCar(int orderId)
        {
            var cartModelList = EntGoodsCartBLL.SingleModel.GetOrderDetail(orderId);
            var order = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
            return Json(new { isok = true, msg = "ok", obj = cartModelList, order= order }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetDraApply()
        {
            List<DrawCashApply> listDrawCashApply = DrawCashApplyBLL.SingleModel.GetList();
            List<DrawCashApplyLog> logs =  DrawCashApplyLogBLL.SingleModel.GetList();
            return Json(new { isok = true, msg = "ok", obj = listDrawCashApply, log = logs }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetSalesManRecord(int appId = 0, int userId = 0, int goodsId = 0)
        {
            string strWhere = "";
            if (appId >= 0)
            {
                strWhere += $"appId={appId}";
            }
            List<SalesManRecord> list = new SalesManRecordBLL().GetList(strWhere);
            SalesManRecordUser salesManRecordUser = SalesManRecordUserBLL.SingleModel.GetModel($"userId={userId} and goodsId={goodsId} and appId={appId}");
            SalesManRecordUser model = model = SalesManRecordUserBLL.SingleModel.GetModel($" DATE_ADD(UpdateTime,INTERVAL protected_time Day)>now() and userId={userId} and goodsId={goodsId} and appId={appId}");//判断 产品-用户-是否在分销员保护期内
            return Json(new { isok = true, msg = "ok", obj = new { listSalesManRecord = list, salesManRecordUser = salesManRecordUser, model = model } }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetTest()
        {
            List<SalesManRecordUser> list = SalesManRecordUserBLL.SingleModel.GetList();
         
            return Json(new { isok = true, msg = "成功", obj = new { SalesManRecordUserList = list } }, JsonRequestBehavior.AllowGet);
          
        }

        public ActionResult AddFunctionList(int TemplateType,int VersionId,string VersionName,int Price)
        {
            FunctionList functionList = new FunctionList();
            functionList.TemplateType = TemplateType;
            functionList.VersionId = VersionId;
            functionList.VersionName = VersionName;
            functionList.Price = Price;
            functionList.AddTime = DateTime.Now;
            functionList.UpdateTime = DateTime.Now;
            functionList.PageConfig = JsonConvert.SerializeObject(new PageConfig());
            functionList.ComsConfig = JsonConvert.SerializeObject(new ComsConfig());
            functionList.ProductMgr = JsonConvert.SerializeObject(new ProductMgr());
            functionList.StoreConfig = JsonConvert.SerializeObject(new StoreConfig());
            functionList.NewsMgr = JsonConvert.SerializeObject(new NewsMgr());
            functionList.MessageMgr = JsonConvert.SerializeObject(new MessageMgr());
            functionList.MarketingPlugin = JsonConvert.SerializeObject(new MarketingPlugin());
            functionList.OperationMgr = JsonConvert.SerializeObject(new OperationMgr());
            functionList.FuncMgr = JsonConvert.SerializeObject(new FuncMgr());
          int id=Convert.ToInt32(FunctionListBLL.SingleModel.Add(functionList));
            return Json(new { isok = true, msg = "成功", obj = id }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFunc(int id)
        {
            // List<FunctionList> list = new FunctionListBLL().GetList();
            List<VersionType> listVersionType = XcxTemplateBLL.SingleModel.GetRealPriceVersionTemplateList(22, id);
            return Json(new { isok = true, msg = "成功", obj = listVersionType }, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult UpdateEntGoodType(int appId)
        {
            string sql = $"update entgoodtype set parentid=-1 where aid={appId}";
           int i= SqlMySql.ExecuteNonQuery(Utility.dbEnum.MINIAPP.ToString(), System.Data.CommandType.Text, sql, null);
            return Json(new { isok = true, msg = "成功", obj = i }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetPlatMyCard(int aid)
        //{
        //    int count = 0;
        //    List<PlatMyCard> list = new PlatMyCardBLL().GetDataList(aid, ref count);
        //    return Json(new { isok = true, msg = "成功", obj = list }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetPlatMsg(int aid)
        {
          
            List<PlatMsg> list = PlatMsgBLL.SingleModel.GetList($"aid={aid}");
            return Json(new { isok = true, msg = "成功", obj = list }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPlatStore(int aid)
        {

            List<PlatStore> list = PlatStoreBLL.SingleModel.GetList($"BindPlatAid={aid}");
            List<PlatStoreRelation> listPlatStoreRelation = PlatStoreRelationBLL.SingleModel.GetList($"aid={aid}");
            return Json(new { isok = true, msg = "成功", obj = new {ListStore=list, listPlatStoreRelation= listPlatStoreRelation } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 导出代理商Excel
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public ActionResult ToSaveExecel()
        {
            
            
            DataTable table = new DataTable();
            table.Columns.AddRange(new[]
            {
                        new DataColumn("ID"),
                        new DataColumn("accountid"),
                        new DataColumn("登录账号"),
                        new DataColumn("手机号"),
                        new DataColumn("代理商名称"),
                        new DataColumn("代理商类型"),
                        new DataColumn("代理区域数量"),
                        new DataColumn("代理区域"),
                        new DataColumn("上级代理商(账号)"),
                        new DataColumn("域名类型"),
                        new DataColumn("网站域名"),
                        new DataColumn("网站状态"),
                        new DataColumn("添加时间"),
                        new DataColumn("修改时间"),
                        new DataColumn("总消费"),
                        new DataColumn("预存款"),
                        new DataColumn("状态")
                    });
            string msg = "";
            try
            {
                int pageIndex = 0;
                int pageSize = 100;
                List<C_Area> arealist = C_AreaBLL.SingleModel.GetList();
                do
                {
                    pageIndex++;
                    var list = AgentinfoBLL.SingleModel.GetListExcelData(pageIndex, pageSize);
                    if (list == null || list.Count <= 0)
                    {
                        break;
                    }

                    //代理区域列表
                    var agentids = string.Join(",", list.Select(s => s.id).Distinct());
                    var agentarealist = AgentAreaBLL.SingleModel.GetList($"agentid in ({agentids})");
                    var accountids = "'" + string.Join("','", list.Select(s => s.useraccountid)) + "'";
                    var accountinfos = AccountBLL.SingleModel.GetListByAccoundId(accountids);
                    var distributions = DistributionBLL.SingleModel.GetList($"useraccountid in ({accountids})");
                    foreach (Agentinfo item in list)
                    {
                        //代理区域数量
                        if (agentarealist != null && agentarealist.Count > 0 && arealist != null && arealist.Count > 0)
                        {
                            List<AgentArea> tempagentarea = agentarealist.Where(w => w.AgentId == item.id).ToList();
                            if (tempagentarea != null && tempagentarea.Count() > 0)
                            {
                                item.areacount = tempagentarea.Count();
                                item.areaname = "";
                                foreach (var a in tempagentarea)
                                {
                                    var areaname = arealist.FirstOrDefault(s => s.Code == a.AreaCode);
                                    var cityname = arealist.FirstOrDefault(s => s.Code == a.CityCode);
                                    var province = arealist.FirstOrDefault(s => s.Code == a.ProvinceCode);
                                    item.areaname += " " + (province != null ? (province.Name + (cityname != null ? "-" + cityname.Name + (areaname != null ? "-" + areaname.Name : "") : "")) : "");
                                }
                            }
                        }

                        Account model = accountinfos?.FirstOrDefault(w => w.Id.ToString() == item.useraccountid);
                        if (model != null)
                        {
                            item.loginid = model.LoginId;
                            item.phone = model.ConsigneePhone;
                        }

                        Distribution distribution = distributions.FirstOrDefault(d => d.useraccountid == item.useraccountid);
                        if (distribution != null)
                        {
                            item.username = distribution.name;
                            var parentAgentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                            if (parentAgentinfo != null)
                            {
                                var parentAccount = AccountBLL.SingleModel.GetModel($"id='{parentAgentinfo.useraccountid}'");
                                if (parentAccount != null)
                                {
                                    item.parentAgent = parentAccount.LoginId;
                                }
                            }
                        }

                        DataRow row = table.NewRow();
                        row["ID"] = item.id;
                        row["accountid"] = item.useraccountid;
                        row["登录账号"] = item.loginid;
                        row["手机号"] = item.phone;
                        row["代理商名称"] = item.username;
                        row["代理商类型"] = item.userLevel == 1 ? "二级代理" : item.userLevel == 0 ? "一级代理" : "未知类型";
                        row["代理区域数量"] = item.areacount;
                        row["代理区域"] = item.areaname;
                        row["上级代理商(账号)"] = item.parentAgent;
                        row["网站域名"] = item.domian;
                        row["域名类型"] = item.DomainTypeStr;
                        row["网站状态"] = item.WebStateStr;
                        row["添加时间"] = item.addtime;
                        row["修改时间"] = item.updateitme;
                        row["总消费"] = item.sumcost;
                        row["预存款"] = item.deposit / 100;
                        row["状态"] = item.state == -1 ? "已停用" : "正常";

                        table.Rows.Add(row);
                    }
                    break;
                } while (true);

                ExcelHelper<Agentinfo>.Out2Excel(table, "代理商详情"); //导出
            }
            catch (Exception ex)
            {
                msg=ex.Message;
            }
            return Content(msg);
        }


        public ActionResult TBPlatStoreTime()
        {
            
            
            TransactionModel transaction = new TransactionModel();
           List<PlatStore> list= PlatStoreBLL.SingleModel .GetList("addtime='0001-01-01 00:00:00'",1000,1,"Id");
            int i = 0;
            foreach(PlatStore item in list)
            {
                PlatStoreRelation platStoreRelation= PlatStoreRelationBLL.SingleModel.GetModel($"StoreId={item.Id}");
                if (platStoreRelation != null)
                {
                    item.AddTime = platStoreRelation.AddTime;
                    item.UpdateTime = platStoreRelation.UpdateTime;
                    transaction.Add(PlatStoreBLL.SingleModel .BuildUpdateSql(item, "AddTime,UpdateTime"));
                    i++;
                }
            }
            if(transaction.sqlArray!=null&& transaction.sqlArray.Length > 0)
            {
                if (PlatStoreBLL.SingleModel .ExecuteTransactionDataCorect(transaction.sqlArray))
                {
                    return Content($"ok={i}");
                }

                return Content($"error={i}");
            }
            else
            {
                return Content($"ok:没有数据");
            }

        }





    }
}