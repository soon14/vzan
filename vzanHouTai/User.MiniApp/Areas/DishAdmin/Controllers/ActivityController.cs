using BLL.MiniApp.Dish;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Model;
using Newtonsoft.Json;
using User.MiniApp.Areas.DishAdmin.Filters;
using Entity.MiniApp.Tools;
using BLL.MiniApp.Tools;
using Entity.MiniApp;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{


    /// <summary>
    /// 活动管理
    /// </summary>
    [LoginFilter]
    public class ActivityController : Controller
    {
        private readonly DishReturnMsg _result;

        public ActivityController()
        {
            _result = new DishReturnMsg();
        }
        /// <summary>
        /// 首单立减
        /// </summary>
        /// <returns></returns>
        public ActionResult Shou(string act = "", int aId = 0, int storeId = 0, DishGaojiConfig model = null)
        {
            EditModel<DishGaojiConfig> em = new EditModel<DishGaojiConfig>();
            if (string.IsNullOrEmpty(act))
            {
                DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
                if (store != null && !string.IsNullOrEmpty(store.gaojiConfigJson))
                {
                    em.DataModel = JsonConvert.DeserializeObject<DishGaojiConfig>(store.gaojiConfigJson);
                }
                else
                    em.DataModel = new DishGaojiConfig();
            }
            else if (act == "edit")
            {
                DishStore store = DishStoreBLL.SingleModel.GetModel($"id={storeId} and aid={aId}");
                if (store != null)
                {
                    DishGaojiConfig storeGojiConfig = null;
                    if (!string.IsNullOrEmpty(store.gaojiConfigJson))
                        storeGojiConfig = JsonConvert.DeserializeObject<DishGaojiConfig>(store.gaojiConfigJson);
                    else
                        storeGojiConfig = new DishGaojiConfig();

                    storeGojiConfig.huodong_shou_isopen = model.huodong_shou_isopen;
                    storeGojiConfig.huodong_shou_jiner = model.huodong_shou_jiner;
                    store.gaojiConfigJson = JsonConvert.SerializeObject(storeGojiConfig);
                    if (DishStoreBLL.SingleModel.Update(store))
                    {
                        _result.code = 1;
                        _result.msg = "设置成功";
                    }
                }
                return Json(_result);
            }
            em.aId = aId;
            em.storeId = storeId;
            return View(em);
        }

        /// <summary>
        /// 促销管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Cu(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                string filterSql = $"state<>-1 and aid={aId} and storeid={storeId}";
                ViewModel<DishActivity> vm = new ViewModel<DishActivity>();
                vm.DataList = DishActivityBLL.SingleModel.GetList(filterSql, pageSize, pageIndex, "*", "q_order desc");
                vm.TotalCount = DishActivityBLL.SingleModel.GetCount(filterSql);
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
                    {
                        _result.code = 0;
                        _result.msg = "参数错误";
                    }
                    else
                    {
                        DishActivity updateModel = DishActivityBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishActivityBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                            {
                                _result.code = 0;
                                _result.msg = "删除失败";
                            }
                        }
                        else
                        {
                            _result.code = 0;
                            _result.msg = "删除失败,活动不存在";
                        }
                    }

                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 促销编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult CuEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, DishActivity model = null)
        {
            //参数验证
            if (id < 0 || aId <= 0 || storeId <= 0)
            {
                _result.msg = "参数错误";
                return Json(_result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                if (id == 0)
                {
                    model = new DishActivity();
                }
                else
                {
                    model = DishActivityBLL.SingleModel.GetModel(id);
                    if (model == null)
                    {
                        return Content("活动不存在");
                    }
                }
                EditModel<DishActivity> em = new EditModel<DishActivity>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    //添加
                    if (id == 0)
                    {
                        int newid = Convert.ToInt32(DishActivityBLL.SingleModel.Add(model));
                        _result.msg = newid > 0 ? "添加成功" : "添加失败";
                        _result.code = newid > 0 ? 1 : 0;

                    }
                    //修改
                    else
                    {
                        bool updateResult = DishActivityBLL.SingleModel.Update(model);
                        _result.msg = updateResult ? "修改成功" : "修改失败";
                        _result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(_result);
        }
        /// <summary>
        /// 社交立减金
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="state"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionResult reductionCardList(int aid = 0, int storeId = 0, int state = -1, int pageSize = 10, int pageIndex = 1, string name = "")
        {

            List<Coupons> list = new List<Coupons>();
            list = CouponsBLL.SingleModel.GetCouponList(name, state, storeId, aid, TicketType.立减金, pageSize, pageIndex, "addtime desc");
            if (list != null && list.Count > 0)
            {
                string couponids = string.Join(",", list.Select(s => s.Id).Distinct());
                List<CouponLog> loglist = CouponLogBLL.SingleModel.GetList($"couponid in ({couponids}) and state!=4");

                foreach (Coupons item in list)
                {
                    item.StateStr = CouponsBLL.SingleModel.GetStateName(item);
                    //领取记录
                    List<CouponLog> temploglist = loglist?.Where(w => w.CouponId == item.Id).ToList();
                    if (temploglist != null && temploglist.Count > 0)
                    {
                        //已领取份数
                        int orderCount = temploglist.GroupBy(g => g.FromOrderId).Count();

                        //库存
                        item.RemNum = item.CreateNum - orderCount;
                        item.CouponNum = temploglist.Count;
                        var tempuserlist = temploglist.GroupBy(g => g.UserId).ToList();
                        //多少人领取
                        item.PersonNum = tempuserlist != null && tempuserlist.Count > 0 ? tempuserlist.Count : 0;
                        //已使用
                        List<CouponLog> tempuselist = temploglist.Where(w => w.State == 1).ToList();
                        item.UseNum = tempuselist != null && tempuselist.Count > 0 ? tempuselist.Count : 0;
                    }
                    else
                    {
                        //库存
                        item.RemNum = item.CreateNum;
                    }
                }
            }

            ViewBag.couponstate = state;
            ViewBag.couponname = name;
            ViewModel<Coupons> vm = new ViewModel<Coupons>();
            vm.DataList = list;
            vm.TotalCount = CouponsBLL.SingleModel.GetCouponListCount(name, state, storeId, aid, TicketType.立减金);
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            vm.aId = aid;
            vm.storeId = storeId;
            return View(vm);
        }
        /// <summary>
        /// 立减金
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="coupons"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public ActionResult ReductionCardEdit(int aid = 0, int storeId = 0, int id = 0)
        {
            Coupons model = null;
            if (id > 0)
            {
                model = CouponsBLL.SingleModel.GetModel(id);
                if (model == null)
                {
                    return Content("数据不存在");
                }
                if (!string.IsNullOrEmpty(model.GoodsIdStr))
                {
                    List<DishGood> goodsList = DishGoodBLL.SingleModel.GetList($"id in ({model.GoodsIdStr}) and state=1");
                    if (goodsList != null && goodsList.Count > 0)
                    {
                        model.SelectGoods = new List<object>();
                        foreach (var goods in goodsList)
                        {
                            model.SelectGoods.Add(new { goods.id, goods.g_name, goods.add_time_str,goods.img });
                        }
                    }
                }
            }
            else
            {
                model = new Coupons();
                model.appId = aid;
                model.StoreId = storeId;
                model.TicketType = (int)TicketType.立减金;
            }
            return View(model);
        }

        public ActionResult GetGoodsList(int aid = 0, int storeId = 0, int pageSize = 8, int pageIndex = 1)
        {
            if (aid <= 0 || storeId <= 0)
            {
                _result.msg = "参数错误";
                return Json(_result);
            }
            int recordCount = 0;
            List<DishGood> goodsList = DishGoodBLL.SingleModel.GetListByCondition(aid, storeId, pageSize, pageIndex, 1, out recordCount);
            _result.code = 1;
            _result.obj = new { list = goodsList, recordCount };
            return Json(_result);
        }
    }
}