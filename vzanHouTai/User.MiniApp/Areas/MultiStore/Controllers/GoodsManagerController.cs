using BLL.MiniApp;
using BLL.MiniApp.Ent;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Controllers;
using User.MiniApp.Model;
using Utility;
using Utility.IO;
using Newtonsoft.Json;
using User.MiniApp.Areas.MultiStore.Filters;
using Newtonsoft.Json.Converters;
using Entity.MiniApp.Model;
using Entity.MiniApp;

namespace User.MiniApp.Areas.MultiStore.Controllers
{
    public class GoodsManagerController : enterpriseproController
    {
        public GoodsManagerController()
        {
            this.PageType = 26;
        }
        // GET: MultiStore/GoodsManager
        public ActionResult Index()
        {
            return View();
        }
        [MiniApp.Filters.RouteAuthCheck]
        public override ActionResult pedit()
        {
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            int goodtype = Context.GetRequestInt("goodtype", (int)EntGoodsType.普通产品);
            EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(id);
            ViewBag.goodtype = goodtype;
            if (goodModel == null)
                goodModel = new EntGoods() { goodtype = goodtype, EntGroups = new EntGroupsRelation() };
            else
            {
                var entGroups = EntGroupsRelationBLL.SingleModel.GetModel($"EntGoodsId={goodModel.id}");
                if (entGroups != null)
                {
                    goodModel.EntGroups = entGroups;
                }
                else
                {
                    goodModel.EntGroups = new EntGroupsRelation();
                }

                ViewBag.goodtype = goodModel.goodtype;
            }
            return View(goodModel);
        }
        /// <summary>
        /// 门店产品管理
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult subgoodlist(int appId, int pageIndex = 1, int pageSize = 20)
        {
            ViewModel<SubStoreGoodsView> vm = new ViewModel<SubStoreGoodsView>();

            string search = Context.GetRequest("search", "");
            int plabels = Context.GetRequestInt("plabels", 0);
            int ptype = Context.GetRequestInt("ptype", 0);
            int ptag = Context.GetRequestInt("ptag", -1);
            int storeid = Context.GetRequestInt("storeId", 0);
            string isPost = Context.GetRequest("isPost",string.Empty);

            string strwhere = $"aid={appId} and substate=1 and state=1 and StoreId={storeid} and goodtype={(int)EntGoodsType.普通产品}";

            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (search.Trim() != "")
            {
                strwhere += $" and name like @name ";
                parameters.Add(new MySqlParameter("@name", $"%{search}%"));
            }
            if (plabels > 0)
            {
                strwhere += $" and  FIND_IN_SET(@plabels,plabels)";
                parameters.Add(new MySqlParameter("@plabels", plabels));
            }
            if (ptype > 0)
            {
                strwhere += $" and FIND_IN_SET (@ptype,ptypes) ";
                parameters.Add(new MySqlParameter("@ptype", ptype));
            }
            if (ptag > -1)
            {
                strwhere += $" and subtag={ptag} ";
            }
            string selSql = $"select * from substoregoodsview where {strwhere} limit {(pageIndex - 1) * pageSize},{pageSize}";
            DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, selSql, parameters.ToArray()).Tables[0];
            vm.DataList = DataHelper.ConvertDataTableToList<SubStoreGoodsView>(dt);
            string countSql = $"select count(0) from substoregoodsview where {strwhere}";
            vm.TotalCount = Convert.ToInt32(SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(), CommandType.Text, countSql, parameters.ToArray()));
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            vm.DataList.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.ptypes))
                {

                    string sql = $"SELECT GROUP_CONCAT(`name`) from entgoodtype where FIND_IN_SET(id,@ptypes)";
                    x.ptypestr = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                        CommandType.Text, sql,
                        new MySqlParameter[] { new MySqlParameter("@ptypes", x.ptypes) }).ToString();
                }

                if (!string.IsNullOrEmpty(x.plabels))
                {
                    string sql = $"SELECT group_concat(name) from entgoodlabel where FIND_IN_SET(id,@plabels)";
                    x.plabelstr = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(),
                        CommandType.Text, sql,
                        new MySqlParameter[] { new MySqlParameter("@plabels", x.plabels) }).ToString();
                }


            });
            vm.CurrentUserRoles = UserRoleBLL.SingleModel.GetCurrentUserRoles(dzuserId,appId,storeid);
            if (isPost == "isPost")
            {
                ViewModel<EntGoods> entgoodView = new ViewModel<EntGoods>();
                entgoodView.TotalCount = vm.TotalCount;
                entgoodView.PageIndex = vm.PageIndex;
                entgoodView.PageSize = vm.PageSize;
                vm.DataList.ForEach(p =>
                {
                    entgoodView.DataList.Add(new EntGoods()
                    {
                        id=p.Pid,
                        name=p.name,
                        img=p.img,
                        price=p.price,
                        unit=p.unit
                    });
                });
                return Content(JsonConvert.SerializeObject(entgoodView, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
            }
                
            return View(vm);
        }

        /// <summary>
        /// 门店拼团管理
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult subgrouplist(int appId, int pageIndex = 1, int pageSize = 20)
        {
            ViewModel<SubStoreGoodsView> vm = new ViewModel<SubStoreGoodsView>();

            string search = Context.GetRequest("search", "");
            int plabels = Context.GetRequestInt("plabels", 0);
            int ptype = Context.GetRequestInt("ptype", 0);
            int ptag = Context.GetRequestInt("ptag", -1);
            int storeid = Context.GetRequestInt("storeId", 0);
            string isPost = Context.GetRequest("isPost", string.Empty);

            string strwhere = $"aid={appId} and substate=1 and state=1 and StoreId={storeid} and goodtype={(int)EntGoodsType.拼团产品}";

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
            string selSql = $"select * from substoregoodsview where {strwhere} limit {(pageIndex - 1) * pageSize},{pageSize}";
            DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, selSql, parameters.ToArray()).Tables[0];
            vm.DataList = DataHelper.ConvertDataTableToList<SubStoreGoodsView>(dt);
            string countSql = $"select count(0) from substoregoodsview where {strwhere}";
            vm.TotalCount = Convert.ToInt32(SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(), CommandType.Text, countSql, parameters.ToArray()));
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            vm.DataList.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.ptypes))
                {

                    string sql = $"SELECT GROUP_CONCAT(`name`) from entgoodtype where FIND_IN_SET(id,@ptypes)";
                    x.ptypestr = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                        CommandType.Text, sql,
                        new MySqlParameter[] { new MySqlParameter("@ptypes", x.ptypes) }).ToString();
                }

                if (!string.IsNullOrEmpty(x.plabels))
                {
                    string sql = $"SELECT group_concat(name) from entgoodlabel where FIND_IN_SET(id,@plabels)";
                    x.plabelstr = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(),
                        CommandType.Text, sql,
                        new MySqlParameter[] { new MySqlParameter("@plabels", x.plabels) }).ToString();
                }
            });
            vm.CurrentUserRoles = UserRoleBLL.SingleModel.GetCurrentUserRoles(dzuserId, appId, storeid);
            if (isPost == "isPost")
            {
                ViewModel<EntGoods> entgoodView = new ViewModel<EntGoods>();
                entgoodView.TotalCount = vm.TotalCount;
                entgoodView.PageIndex = vm.PageIndex;
                entgoodView.PageSize = vm.PageSize;
                vm.DataList.ForEach(p =>
                {
                    entgoodView.DataList.Add(new EntGoods()
                    {
                        id = p.Pid,
                        name = p.name,
                        img = p.img,
                        price = p.price,
                        unit = p.unit,
                        goodtype = p.goodtype,
                    });
                });
                return Content(JsonConvert.SerializeObject(entgoodView, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
            }

            //获取拼团原价和团购价
            vm.DataList = EntGroupsRelationBLL.SingleModel.GetMStoreGroup(vm.DataList);

            return View(vm);
        }

        /// <summary>
        /// 添加门店产品
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ActionResult AddStoreProduct(int appId = 0, int storeId = 0)
        {
            string ids = Context.GetRequest("ids", string.Empty);
            if (appId == 0 || storeId == 0 || ids == "")
            {
                return Json(new { isok = false, msg = "非法请求" });
            }
            string[] idsArray = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (idsArray.Length <= 0)
                return Json(new { isok = false, msg = "请选择要添加的产品" });

            SubStoreEntGoods subGoodsModel;
            List<string> addSqlList = new List<string>();
            foreach (string id in idsArray)
            {
                int pid = 0;
                if (int.TryParse(id, out pid))
                {
                    EntGoods goodModel = EntGoodsBLL.SingleModel.GetModel(pid);
                    if (goodModel != null&&!SubStoreEntGoodsBLL.SingleModel.Exists($"pid={pid} and aid={appId} and storeid={storeId} and SubState=1"))
                    {
                        subGoodsModel = new SubStoreEntGoods()
                        {
                            Aid = goodModel.aid,
                            Pid = goodModel.id,
                            StoreId = storeId,
                            SubSpecificationdetail = goodModel.specificationdetail,
                            SubState = 1,
                            SubTag = 1,
                            SubsalesCount = 0,
                            SubStock = goodModel.stock,
                        };
                        addSqlList.Add(SubStoreEntGoodsBLL.SingleModel.BuildAddSql(subGoodsModel));

                        #region 拼团
                        if(goodModel.goodtype== (int)EntGoodsType.拼团产品)
                        {
                            EntGroupsRelation entgroups = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(goodModel.id,goodModel.aid);
                            if(entgroups==null)
                            {
                                return Json(new { isok = false, msg = "团商品已失效！" });
                            }
                            entgroups.AddTime = DateTime.Now;
                            entgroups.RId = goodModel.aid;
                            entgroups.EntGoodsId = goodModel.id;//占位
                            entgroups.StoreId = storeId;
                            string addsql = EntGroupsRelationBLL.SingleModel.BuildAddSql(entgroups);
                            //获取最新插入的多门店产品ID
                            //addsql = addsql.Replace("-9999", "(select last_insert_id())");
                            //log4net.LogHelper.WriteInfo(this.GetType(),addsql);
                            addSqlList.Add(addsql);
                        }
                        
                        #endregion
                    }
                }
            }
            if (SubStoreEntGoodsBLL.SingleModel.ExecuteTransaction(addSqlList.ToArray()))
            {
                return Json(new { isok = true, msg = $"选择{idsArray.Length}个产品，添加成功{addSqlList.Count}个" });
            }
            else
            {
                return Json(new { isok = false, msg = "添加失败！" });
            }

        }

        /// <summary>
        /// 编辑门店产品
        /// </summary>
        /// <returns></returns>
        public ActionResult subgoodedit()
        {
            int id = Context.GetRequestInt("id", 0);
            int subid = Context.GetRequestInt("subid", 0);
            if (id <= 0 || subid <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            EntGoods entModelOld = EntGoodsBLL.SingleModel.GetModel(id);
            if (entModelOld == null || entModelOld.state == 0 || entModelOld.tag == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "总店产品不可用!", code = "500" });
            }
            SubStoreEntGoods subModel = SubStoreEntGoodsBLL.SingleModel.GetModel(subid);
            if (subModel == null || subModel.SubState == 0)
            {
                if (subModel.SubTag == 1)
                {
                    return View("PageError", new Return_Msg() { Msg = "下架后才可以编辑!", code = "500" });
                }
                return View("PageError", new Return_Msg() { Msg = "产品不可用!", code = "500" });
            }
            
            entModelOld.specificationdetail = subModel.SubSpecificationdetail;
            entModelOld.stock = subModel.SubStock;
            entModelOld.sort = subModel.SubSort;

            //拼团
            if(entModelOld.goodtype==(int)EntGoodsType.拼团产品)
            {
                EntGroupsRelation group = EntGroupsRelationBLL.SingleModel.GetModelByGroupGoodType(entModelOld.id,entModelOld.aid);
                if(group==null)
                {
                    return View("PageError", new Return_Msg() { Msg = "拼团不可编辑!", code = "500" });
                }
                entModelOld.EntGroups = group;
            }
            return View(entModelOld);
        }
        /// <summary>
        /// 编辑门店产品-保存
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult subgoodedit(int appId, EntGoods model)
        {
            string act = Context.GetRequest("act", "");
            int storeId = Context.GetRequestInt("storeId", 0);
            int subid = Context.GetRequestInt("subid", 0);
            int pid = Context.GetRequestInt("id", 0);
            if (act == "batch")
            {
                int actval = Context.GetRequestInt("actval", -1);
                if (actval < 0 || storeId < 0)
                {
                    return Json(new { isok = true, msg = "非法参数" });
                }
                string ids = Context.GetRequest("ids", string.Empty);
                if (string.IsNullOrEmpty(ids))
                {
                    return Json(new { isok = true, msg = "请先选择产品" });
                }
                string sql = $"update substoreentgoods set SubTag=@tag where StoreId={storeId} and find_in_set(id,@ids)>0";

                SqlMySql.ExecuteTransaction(EntGoodsBLL.SingleModel.connName, sql, CommandType.Text,
                    new MySqlParameter[] {
                        new MySqlParameter("@ids",ids),
                        new MySqlParameter("@tag",actval)
                    });
                return Json(new { isok = true, msg = "设置成功" });
            }
            else if (act == "del")
            {

                SubStoreEntGoods subModel = SubStoreEntGoodsBLL.SingleModel.GetModel(subid);
                if (subModel == null)
                {
                    return Json(new { isok = false, msg = "产品不存在" });
                }
                subModel.SubState = 0;
                var TranModel = new TransactionModel();
                TranModel.Add(SubStoreEntGoodsBLL.SingleModel.BuildUpdateSql(subModel, "SubState"));
                bool result = SubStoreEntGoodsBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray);
                return Json(new { isok = result, msg = "删除" + (result ? "成功" : "失败") });
            }
            else if (act == "tag")
            {
                int tag = Context.GetRequestInt("tag", -1);
                if (pid <= 0 || tag < 0)
                {
                    return Json(new { isok = false, msg = "参数错误" });
                }
                SubStoreEntGoods subModel = SubStoreEntGoodsBLL.SingleModel.GetModel(subid);
                if (subModel == null || subModel.SubState == 0)
                {
                    return Json(new { isok = false, msg = "产品不存在或已删除" });
                }
                EntGoods entGood = EntGoodsBLL.SingleModel.GetModel(pid);
                if (entGood == null || entGood.state == 0)
                {
                    return Json(new { isok = false, msg = "总店产品不存在或已删除" });
                }
                if (tag != -1)
                {
                    //如果总店产品下架，分店下架的产品不能再上架
                    if (entGood.tag == 0 && subModel.SubTag == 0 && tag == 1)
                    {
                        return Json(new { isok = false, msg = "总店产品已下架，分店产品不能再上架" });
                    }
                    subModel.SubTag = tag;


                    var TranModel = new TransactionModel();
                    TranModel.Add(SubStoreEntGoodsBLL.SingleModel.BuildUpdateSql(subModel, "SubTag"));
                    string actionName = (tag == 0 ? "下架" : "上架");
                    bool result = SubStoreEntGoodsBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray);
                    return Json(new { isok = result, msg = actionName + (result ? "成功" : "失败") });
                }
                return Json(new { isok = false, msg = "操作失败！" });
            }
            else if (act == "edit")
            {
                SubStoreEntGoods subModel = SubStoreEntGoodsBLL.SingleModel.GetModel(subid);
                if (subModel == null || subModel.SubState == 0 )
                {
                    return Json(new { isok = true, msg = "产品不可用" });
                }
                EntGoods entModelOld = EntGoodsBLL.SingleModel.GetModel(model.id);
                if (entModelOld == null || entModelOld.state == 0 || entModelOld.tag == 0)
                {

                    return Json(new { isok = true, msg = "总店产品不可用" });
                }

                var TranModel = new TransactionModel();
                TranModel.Add(EntGoodsBLL.SingleModel.GetSyncSql(entModelOld, model, subModel));
                bool reault= SubStoreEntGoodsBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray);

                if (reault)
                {
                    return Json(new { isok = true, msg = "修改成功" });
                }
            }
            return Json(new { isok = false, msg = "操作失败" });
        }
        
    }
}