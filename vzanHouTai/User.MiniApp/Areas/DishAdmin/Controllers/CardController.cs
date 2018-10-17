using BLL.MiniApp;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Helper;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using User.MiniApp.Areas.DishAdmin.Filters;
using User.MiniApp.Model;
using Utility.IO;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{
    /// <summary>
    /// 会员卡
    /// </summary>
    [LoginFilter]
    public class CardController : Controller
    {
        
        
        

        private readonly DishReturnMsg _result;
        public CardController()
        {
            _result = new DishReturnMsg();
            _result.msg = "网络异常";

            
            
            
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 会员卡管理
        /// </summary>
        /// <returns></returns>
        public ActionResult CardList(int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string nickname = "", string u_name = "", string u_phone = "", string number = "")
        {
            DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aId, storeId);
            if (store == null)
            {
                _result.code = 500;
                _result.msg = "门店不存在";
                return View("PageError", _result);
            }
            int recordCount = 0;
            ViewModel<DishVipCard> vm = new ViewModel<DishVipCard>();
            vm.DataList = DishVipCardBLL.SingleModel.GetVipCardList(storeId, pageIndex, pageSize, nickname, u_name, u_phone, number, out recordCount);
            vm.TotalCount = recordCount;
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            vm.storeId = storeId;
            vm.aId = aId;
            return View(vm);
        }

        /// <summary>
        /// 会员卡资料编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult CardEdit(int id = 0, int aId = 6901757, int storeId = 8, string act = "")
        {
            if (act != "save")
            {
                if (id <= 0)
                {
                    _result.code = 500;
                    _result.msg = "参数错误 id_error";
                    return View("PageError", _result);
                }
                EditModel<DishVipCard> model = new EditModel<DishVipCard>();
                model.DataModel = DishVipCardBLL.SingleModel.GetVipCardById_StoreId(id, storeId);
                model.aId = aId;
                model.storeId = storeId;
                return View(model);
            }
            else
            {
                if (id <= 0)
                {
                    _result.msg = "参数错误 id_error";
                    return Json(_result);
                }
                string u_name = Context.GetRequest("u_name", string.Empty);
                string u_phone = Context.GetRequest("u_phone", string.Empty);
                DishVipCard card = DishVipCardBLL.SingleModel.GetVipCardById_StoreId(id, storeId);
                if (card == null)
                {
                    _result.msg = "不存在此会员卡";
                    return Json(_result);
                }
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(card.uid);
                if (userInfo == null)
                {
                    _result.msg = "不存在此用户";
                    return Json(_result);
                }
                card.u_name = u_name;
                if (!DishVipCardBLL.SingleModel.Update(card, "u_name"))
                {
                    _result.msg = "保存失败";
                    return Json(_result);
                }
                userInfo.TelePhone = u_phone;
                if (!C_UserInfoBLL.SingleModel.Update(userInfo, "telephone"))
                {
                    _result.msg = "手机号保存失败";
                    return Json(_result);
                }
                _result.code = 1;
                _result.msg = "保存成功";
                return Json(_result);
            }
        }

        /// <summary>
        /// 会员卡资金记录
        /// </summary>
        /// <returns></returns>
        public ActionResult CardLog(int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20)
        {
            DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aId, storeId);
            if (store == null)
            {
                _result.code = 500;
                _result.msg = "门店不存在";
                return View("PageError", _result);
            }
            if (id <= 0)
            {
                _result.code = 500;
                _result.msg = "参数错误 id_error";
                return View("PageError", _result);
            }
            DishVipCard card = DishVipCardBLL.SingleModel.GetVipCardById_StoreId(id, storeId); 
            int recordCount = 0;
            ViewModel<DishCardAccountLog> vm = new ViewModel<DishCardAccountLog>();
            vm.DataList = DishCardAccountLogBLL.SingleModel.GetRecordLogList(storeId, card.uid, pageIndex+1, pageSize, out recordCount);
            vm.TotalCount = recordCount;
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            vm.storeId = storeId;
            vm.aId = aId;
            return View(vm);
        }

        /// <summary>
        /// 会员管理
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberList(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string keyword = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {

                ViewModel<C_UserInfo> vm = new ViewModel<C_UserInfo>();
                if (storeId > 0)
                {
                    XcxAppAccountRelation relModel = XcxAppAccountRelationBLL.SingleModel.GetModelById(aId);
                    if (relModel == null)
                    {
                        return Content("小程序不存在或已过期");
                    }
                    string filterSql = "1=1";
                    List<MySql.Data.MySqlClient.MySqlParameter> parameters = new List<MySql.Data.MySqlClient.MySqlParameter>();
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        filterSql += $" and NickName like @NickName";
                        parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@NickName", Utils.FuzzyQuery(keyword.ToLower())));
                    }
                    if (relModel != null&&!string.IsNullOrEmpty(relModel.AppId))
                    {
                        filterSql += $" and appid='{relModel.AppId}'";
                        vm.DataList = C_UserInfoBLL.SingleModel.GetListBySql($"select * from C_UserInfo where {filterSql} order by AddTime desc limit {pageIndex * pageSize},{pageSize}", parameters.ToArray());
                        vm.PageIndex = pageIndex;
                        vm.PageSize = pageSize;
                        vm.TotalCount = C_UserInfoBLL.SingleModel.GetCount(filterSql, parameters.ToArray());
                        vm.aId = aId;
                        vm.storeId = storeId;
                    }
                    
                }
                return View(vm);
            }
            return Json(_result, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 充值
        /// </summary>
        /// <returns></returns>
        public ActionResult Recharge(int id = 0, int aId = 0, int storeId = 0, string act = "", double account_money = 0, string account_info = "")
        {
            if (act != "save")
            {
                EditModel<DishVipCard> model = new EditModel<DishVipCard>();
                model.aId = aId;
                model.storeId = storeId;
                model.DataModel = DishVipCardBLL.SingleModel.GetVipCardById_StoreId(id, storeId);
                if (model.DataModel == null)
                {
                    _result.code = 500;
                    _result.msg = "会员卡不存在";
                    return View("PageError", _result);
                }
                return View(model);
            }
            else
            {
                if (account_money == 0)
                {
                    _result.msg = "充值金额不能为0";
                    return Json(_result);
                }
                if (id <= 0)
                {
                    _result.msg = "参数错误 id_error";
                    return Json(_result);
                }
                DishVipCard card = DishVipCardBLL.SingleModel.GetVipCardById_StoreId(id, storeId);
                if (card == null)
                {
                    _result.msg = "不存在此会员卡";
                    return Json(_result);
                }
                card.account_balance += account_money;
                if (DishVipCardBLL.SingleModel.Update(card, "account_balance"))
                {
                    _result.code = 1;
                    _result.msg = "修改成功";
                    DishCardAccountLogBLL.SingleModel.AddRecordLog(card, account_money, account_info);
                    return Json(_result);
                }
                else
                {
                    _result.msg = "修改失败";
                    return Json(_result);
                }
            }
        }

        /// <summary>
        /// 基本配置
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult Config(string act = "", DishVipCardSetting cardSetting = null)
        {
            int aid = Context.GetRequestInt("aId", 0);
            if (aid <= 0)
            {
                _result.code = 500;
                _result.msg = "参数错误";
                return View("PageError", _result);
            }
            int storeId = Context.GetRequestInt("storeId", 0);
            if (storeId <= 0)
            {
                _result.code = 500;
                _result.msg = "参数错误!";
                return View("PageError", _result);
            }
            if (act != "save")
            {
                DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    _result.code = 500;
                    _result.msg = "门店不存在";
                    return View("PageError", _result);
                }
                cardSetting = DishVipCardSettingBLL.SingleModel.GetModelByStoreId(store.id);
                if (cardSetting == null)
                {
                    cardSetting = new DishVipCardSetting();
                    cardSetting.aid = store.aid;
                    cardSetting.storeId = store.id;
                    cardSetting.id = Convert.ToInt32(DishVipCardSettingBLL.SingleModel.Add(cardSetting));
                    if (cardSetting.id < 0)
                    {
                        _result.code = 500;
                        _result.msg = "数据错误";
                        return View("PageError", _result);
                    }
                }
                return View(cardSetting);
            }
            else
            {
                if (aid <= 0)
                {
                    _result.msg = "参数错误";
                    return Json(_result);
                }

                if (storeId <= 0)
                {
                    _result.msg = "参数错误!";
                    Json(_result);
                }

                DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    _result.msg = "门店不存在";
                    return Json(_result);
                }
                if (cardSetting == null)
                {
                    _result.msg = "参数错误";
                    return Json(_result);
                }
                if (cardSetting.id <= 0)
                {
                    _result.msg = "参数错误!id error";
                    return Json(_result);
                }
                DishVipCardSetting model = DishVipCardSettingBLL.SingleModel.GetModelByStoreId_Id(store.id, cardSetting.id);
                if (model == null)
                {
                    _result.msg = "数据错误";
                    return Json(_result);
                }
                model.card_info = cardSetting.card_info;
                model.card_open_status = cardSetting.card_open_status;
                bool isSuccess = DishVipCardSettingBLL.SingleModel.Update(model, "card_info,card_open_status");
                if (isSuccess)
                {
                    _result.code = 1;
                    _result.msg = "保存成功";
                    return Json(_result);
                }
                else
                {
                    _result.code = 0;
                    _result.msg = "保存失败";
                    return Json(_result);
                }
            }

        }

        /// <summary>
        /// 充值配置
        /// </summary>
        /// <returns></returns>
        public ActionResult Chong(string act = "", List<double> rc_man = null, List<double> rc_song = null)
        {
            int aid = Context.GetRequestInt("aId", 0);
            if (aid <= 0)
            {
                _result.code = 500;
                _result.msg = "参数错误";
                return View("PageError", _result);
            }
            int storeId = Context.GetRequestInt("storeId", 0);
            if (storeId <= 0)
            {
                _result.code = 500;
                _result.msg = "参数错误!";
                return View("PageError", _result);
            }
            if (act != "save")
            {
                DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    _result.code = 500;
                    _result.msg = "门店不存在";
                    return View("PageError", _result);
                }
                DishVipCardSetting cardSetting = DishVipCardSettingBLL.SingleModel.GetModelByStoreId(store.id);
                if (cardSetting == null)
                {
                    cardSetting = new DishVipCardSetting();
                    cardSetting.aid = store.aid;
                    cardSetting.storeId = store.id;
                    cardSetting.id = Convert.ToInt32(DishVipCardSettingBLL.SingleModel.Add(cardSetting));
                    if (cardSetting.id < 0)
                    {
                        _result.code = 500;
                        _result.msg = "数据错误";
                        return View("PageError", _result);
                    }
                }
                if (!string.IsNullOrEmpty(cardSetting.rechargeConfigJson))
                {
                    cardSetting.rechargeConfig = JsonConvert.DeserializeObject<List<RechargeConfig>>(cardSetting.rechargeConfigJson);
                }
                return View(cardSetting);
            }
            else
            {
                if (aid <= 0)
                {
                    _result.msg = "参数错误";
                    return Json(_result);
                }

                if (storeId <= 0)
                {
                    _result.msg = "参数错误!";
                    Json(_result);
                }

                DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    _result.msg = "门店不存在";
                    return Json(_result);
                }
                int id = Context.GetRequestInt("id", 0);
                if (id <= 0)
                {
                    _result.msg = "参数错误!id error";
                    return Json(_result);
                }
                DishVipCardSetting model = DishVipCardSettingBLL.SingleModel.GetModelByStoreId_Id(store.id, id);
                if (model == null)
                {
                    _result.msg = "数据错误";
                    return Json(_result);
                }
                model.rechargeConfig = GetRechargeConfig(rc_man, rc_song);
                model.rechargeConfigJson = JsonConvert.SerializeObject(model.rechargeConfig);
                bool isSuccess = DishVipCardSettingBLL.SingleModel.Update(model, "rechargeConfigJson");
                if (isSuccess)
                {
                    _result.code = 1;
                    _result.msg = "保存成功";
                    return Json(_result);
                }
                else
                {
                    _result.code = 0;
                    _result.msg = "保存失败";
                    return Json(_result);
                }
            }
        }

        private List<RechargeConfig> GetRechargeConfig(List<double> rc_man, List<double> rc_song)
        {
            List<RechargeConfig> rechargeList = null;
            if (rc_man == null || rc_man.Count <= 0) return rechargeList;
            rechargeList = new List<RechargeConfig>();
            for (int i = 0; i < rc_man.Count; i++)
            {
                RechargeConfig recharge = new RechargeConfig();
                recharge.rc_man = rc_man[i];
                if (rc_song != null && i < rc_song.Count)
                {
                    recharge.rc_song = rc_song[i];
                }
                rechargeList.Add(recharge);
            }
            return rechargeList;
        }


        public void Export(int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string nickname = "", string u_name = "", string u_phone = "", string number = "")
        {
            DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aId, storeId);
            if (store == null)
            {
                Response.Write("<script>alert('门店不存在');window.opener=null;window.close();</script>");
            }
            int recordCount = 0;
            ViewModel<DishVipCard> vm = new ViewModel<DishVipCard>();
            vm.DataList = DishVipCardBLL.SingleModel.GetVipCardList(storeId, pageIndex, pageSize, nickname, u_name, u_phone, number, out recordCount);
            string filename = $"表单导出";
            if (vm.DataList != null && vm.DataList.Count > 0)
            {
                
                DataTable exportTable = new DataTable();
                exportTable = ExportExcelBLL.GetDishVipData(vm.DataList);
                ExcelHelper<DishVipCard>.Out2Excel(exportTable, filename);//导出
            }
            else
            {
                Response.Write("<script>alert('查无数据');window.opener=null;window.close();</script>");
                return;
            }
        }

    }
}