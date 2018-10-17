using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entity.MiniApp.Pin;
using User.MiniApp.Model;
using Core.MiniApp;
using User.MiniApp.Areas.Pin.Filters;
using MySql.Data.MySqlClient;
using System.Text;
using BLL.MiniApp.Pin;

namespace User.MiniApp.Areas.Pin.Controllers
{
    /// <summary>
    /// 产品管理
    /// 分类管理
    /// </summary>
    [LoginFilter]
    public class ProductController : BaseController
    {
        #region 分类
        public ActionResult Category(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "", int fid = 0, int state = 1)
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                string filterSql = $"state<>-1 and aid={aId} and storeid={storeId} and fId={fid}";
                ViewModel<PinCategory> vm = new ViewModel<PinCategory>();
                vm.DataList = PinCategoryBLL.SingleModel.GetList(filterSql, pageSize, pageIndex, "*", "sort desc");
                vm.TotalCount = PinCategoryBLL.SingleModel.GetCount(filterSql);
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.aId = aId;
                vm.storeId = storeId;
                return View(vm);
            }
            else
            {
                //删除
                if (act == "del")
                {
                    if (id <= 0)
                        result.msg = "参数错误";
                    else
                    {
                        PinCategory updateModel = PinCategoryBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = PinCategoryBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                result.code = 1;
                                result.msg = "删除成功";
                            }
                            else
                                result.msg = "删除失败";
                        }
                        else
                            result.msg = "删除失败,对象不存在或已删除";
                    }

                }
                else if (act == "sort")
                {
                    bool updateResult = PinCategoryBLL.SingleModel.UpdateSortBatch(sortData, aId);
                    result.code = updateResult ? 1 : 0;
                    result.msg = updateResult ? "排序成功" : "排序失败";
                }
                else if (act == "updateState")
                {
                    PinCategory model = PinCategoryBLL.SingleModel.GetModel(id);
                    bool updateResult = false;
                    if (model != null)
                    {
                        model.state = state;
                        updateResult = PinCategoryBLL.SingleModel.Update(model, "state");
                    }
                    result.code = updateResult ? 1 : 0;
                    result.msg = updateResult ? "设置成功" : "设置失败";

                }
            }
            return Json(result);
        }

        public ActionResult CategoryEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, PinCategory model = null, int fid = 0)
        {
            //参数验证
            if (id < 0 || aId <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (id == 0)
                    model = new PinCategory();
                else
                {
                    model = PinCategoryBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("对象不存在");
                }
                EditModel<PinCategory> em = new EditModel<PinCategory>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    ModelState.Remove("sort");
                    if (!ModelState.IsValid)
                    {
                        result.code = 0;
                        result.msg = this.ErrorMsg();
                        return Json(result);
                    }
                    if (id == 0)
                    {
                        if (PinCategoryBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                        {
                            result.code = 0;
                            result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                            return Json(result);
                        }
                        int newid = Convert.ToInt32(PinCategoryBLL.SingleModel.Add(model));
                        result.msg = newid > 0 ? "添加成功" : "添加失败";
                        result.code = newid > 0 ? 1 : 0;
                    }
                    else
                    {
                        if (PinCategoryBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name and id<>{id}", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                        {
                            result.code = 0;
                            result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                            return Json(result);
                        }
                        bool updateResult = PinCategoryBLL.SingleModel.Update(model);
                        result.msg = updateResult ? "修改成功" : "修改失败";
                        result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(result);
        }

        public ActionResult getSubCategory(int fid)
        {
            List<PinCategory> list = PinCategoryBLL.SingleModel.GetListBySql($"select * from PinCategory where fid={fid} order by sort desc");
            if (list != null && list.Count > 0)
            {
                result.obj = list;
                result.code = 1;
            }
            return Json(result);
        }
        #endregion

        #region 产品
        public ActionResult Good(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "", string name = "", string shopName = "", string shopTel = "", int cateIdOne = 0, int cateId = 0, int auditState = -10,string orderBy= "indexrank",string orderMode= "desc")
        {
            if (string.IsNullOrEmpty(act))
            {
                pageIndex -= 1;
                if (pageIndex < 0)
                    pageIndex = 0;
                

                StringBuilder filterSql = new StringBuilder();
                ViewModel<PinGoods> vm = new ViewModel<PinGoods>();
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                filterSql.Append($" and g.aId = {aId} ");

                if (!string.IsNullOrEmpty(name))
                {
                    filterSql.Append(" and g.name like @name ");
                    parameters.Add(new MySqlParameter("@name", Utils.FuzzyQuery(name)));
                }
                if (!string.IsNullOrEmpty(shopName))
                {
                    filterSql.Append(" and s.storename like @shopName ");
                    parameters.Add(new MySqlParameter("@shopName", Utils.FuzzyQuery(shopName)));
                }
                if (!string.IsNullOrEmpty(shopTel))
                {
                    filterSql.Append(" and s.phone like @shopTel ");
                    parameters.Add(new MySqlParameter("@shopTel", Utils.FuzzyQuery(shopTel)));
                }
                if (cateIdOne > 0)
                {
                    filterSql.Append(" and g.cateIdOne = @cateIdOne ");
                    parameters.Add(new MySqlParameter("@cateIdOne", cateIdOne));
                }
                else if (cateIdOne == 0)
                {
                    filterSql.Append(" and g.cateIdOne not in(SELECT id from pincategory where state=-1 and fid=0) and g.cateId not in(select id from pincategory  where state=-1 and fid<>0) ");
                }
                if (cateId > 0)
                {
                    filterSql.Append(" and g.cateId = @cateId ");
                    parameters.Add(new MySqlParameter("@cateId", cateId));
                }
                if (auditState != -10)
                {
                    filterSql.Append(" and g.auditState = @auditState ");
                    parameters.Add(new MySqlParameter("@auditState", auditState));
                }

                string sql = $"SELECT g.* from pingoods g " +
                    $" inner JOIN pinstore s " +
                    $" on " +
                    $" g.storeid=s.id " +
                    filterSql +
                    $" and s.state=1 " +
                    $" and g.state<>-1 ";
                string orderSql = $" order by  g.{orderBy} {orderMode},g.id desc limit {pageIndex * pageSize},{pageSize}";

                vm.DataList = PinGoodsBLL.SingleModel.GetListBySql(sql + orderSql, parameters.ToArray());
                vm.TotalCount = PinGoodsBLL.SingleModel.GetCountBySql(sql.Replace("g.*", "count(0)"), parameters.ToArray());
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.aId = aId;
                vm.storeId = storeId;
                return View(vm);
            }
            else
            {
                bool updateResult = false;

                if (act == "updateIndexRank")
                {
                    PinGoods goodModel = PinGoodsBLL.SingleModel.GetModel(id);
                    if (goodModel == null)
                    {
                        result.msg = "产品不存在";
                        return Json(result);
                    }
                    int rank = Utility.IO.Context.GetRequestInt("value", 0);
                    goodModel.IndexRank = rank;
                    updateResult = PinGoodsBLL.SingleModel.Update(goodModel, "IndexRank");
                }
                else if (act == "updateAuditState")
                {
                    PinGoods goodModel = PinGoodsBLL.SingleModel.GetModel(id);
                    if (goodModel == null)
                    {
                        result.msg = "产品不存在";
                        return Json(result);
                    }
                    int state = Utility.IO.Context.GetRequestInt("state", 0);
                    goodModel.auditState = state;
                    if (state == (int)PinEnums.GoodsAuditState.已拒绝)
                    {
                        goodModel.state = 0;
                    }
                    updateResult = PinGoodsBLL.SingleModel.Update(goodModel, "auditState");
                }
                else if (act == "sort")
                {
                    updateResult = PinGoodsBLL.SingleModel.UpdateRankBatch(sortData);
                }
                result.code = updateResult ? 1 : 0;
                result.msg = updateResult ? "设置成功" : "设置失败";
                return Json(result);
            }
        }
        public ActionResult GoodInfo(int id = 0, int aId = 0)
        {
            if (id <= 0)
            {
                return Content("非法请求");
            }
            ViewModel<PinGoods> vm = new ViewModel<PinGoods>();
            vm.DataModel = PinGoodsBLL.SingleModel.GetModel(id);
            vm.aId = aId;
            return View(vm);
        }
        #endregion
    }
}