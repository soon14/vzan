using Entity.MiniApp.Dish;
using BLL.MiniApp.Dish;
using User.MiniApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.IO;
using BLL.MiniApp;
using Entity.MiniApp;
using Core.MiniApp;
using User.MiniApp.Areas.DishAdmin.Filters;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{
    [LoginFilter]
    public class PeiSongController : Controller
    {
        private readonly DishReturnMsg _result;
        
        public PeiSongController()
        {
            _result = new DishReturnMsg();
            
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 基本设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Config(string act, int aId,int storeId,DishStore model=null)
        {
            
            //显示
            if (string.IsNullOrEmpty(act))
            {
                model = DishStoreBLL.SingleModel.GetModelByAid_Id(aId, storeId);
                if (model == null)
                    model = new DishStore();
                EditModel<DishStore> em = new EditModel<DishStore>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    bool updateResult = DishStoreBLL.SingleModel.Update(model, "ps_open_status,ps_type");
                    _result.msg = updateResult ? "修改成功" : "修改失败";
                    _result.code = updateResult ? 1 : 0;
                }
                return Json(_result, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 本店-配送员配送
        /// </summary>
        /// <returns></returns>
        public ActionResult SelfPeiSong(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishTransporter> vm = new ViewModel<DishTransporter>();
                vm.DataList = DishTransporterBLL.SingleModel.GetListBySql($"select * from DishTransporter where state<>-1 and aid={aId} and storeid={storeId} order by sort desc");
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
                        _result.msg = "参数错误";
                    else
                    {
                        DishTransporter updateModel = DishTransporterBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishTransporterBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                                _result.msg = "删除失败";
                        }
                        else
                            _result.msg = "删除失败,数据不存在或已删除";
                    }

                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 编辑本店配送员
        /// </summary>
        /// <returns></returns>
        public ActionResult SelfPeiSongEdit(string act = "", int id = 0, int aId = 0, int storeId = 0, DishTransporter model = null, int fid = 0)
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
                    model = new DishTransporter();
                else
                {
                    model = DishTransporterBLL.SingleModel.GetModel(id);
                    if (model == null)
                        return Content("分类不存在");
                }
                EditModel<DishTransporter> em = new EditModel<DishTransporter>();
                em.DataModel = model;
                em.aId = aId;
                em.storeId = storeId;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    if (id == 0)
                    {
                        int newid = Convert.ToInt32(DishTransporterBLL.SingleModel.Add(model));
                        _result.msg = newid > 0 ? "添加成功" : "添加失败";
                        _result.code = newid > 0 ? 1 : 0;
                    }
                    else
                    {
                        bool updateResult = DishTransporterBLL.SingleModel.Update(model);
                        _result.msg = updateResult ? "修改成功" : "修改失败";
                        _result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(_result);
        }


        /// <summary>
        /// 达达配送配置
        /// </summary>
        /// <returns></returns>

        public ActionResult DaDaPeiSongConfig(int aId,string act,int storeId, DadaMerchant model=null)
        {
            //int id = Context.GetRequestInt("id", 0);
            //int aId = Context.GetRequestInt("aId", 0);
            //int storeId = Context.GetRequestInt("storeId", 0);
            //string act = Context.GetRequest("act", string.Empty);
            //string mobile = Context.GetRequest("mobile", string.Empty);
            //string city_name = Context.GetRequest("city_name", string.Empty);
            //int source_id = Context.GetRequestInt("source_id", 0);
            //string origin_shop_id = Context.GetRequest("origin_shop_id", string.Empty);
            //string enterprise_name = Context.GetRequest("enterprise_name", string.Empty);

            //参数验证
            if (aId <= 0 )
            {
                _result.msg = "参数错误";
                return Json(_result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                EditModel<DadaMerchant> em = new EditModel<DadaMerchant>();
                em.appId = aId;
                em.storeId = storeId;
                em.aId = aId;
                DadaMerchant dadamerchantmodel = DadaMerchantBLL.SingleModel.GetModelByRId(aId, storeId);
                if (dadamerchantmodel != null && dadamerchantmodel.id>0)
                {
                    DadaShop dadashopmodel = DadaShopBLL.SingleModel.GetModelByMId(dadamerchantmodel.id);
                    if (dadashopmodel == null)
                    {
                        return Content("店铺信息失效!");
                    }
                    dadamerchantmodel.origin_shop_id = dadashopmodel.origin_shop_id;
                }
                List<DadaCity> citylist = DadaCityBLL.SingleModel.GetList();
                dadamerchantmodel.CityList = citylist;
                em.DataModel = dadamerchantmodel;

                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    if(model==null)
                    {
                        _result.msg = "参数出错";
                        return Json(_result);
                    }
                    if(string.IsNullOrEmpty(model.mobile))
                    {
                        _result.msg = "请输入商户手机号";
                        return Json(_result);
                    }
                    if (string.IsNullOrEmpty(model.city_name) || model.city_name =="0")
                    {
                        _result.msg = "请选择城市";
                        return Json(_result);
                    }
                    if (string.IsNullOrEmpty(model.sourceid))
                    {
                        _result.msg = "请输入您在达达注册的商户ID";
                        return Json(_result);
                    }
                    if (string.IsNullOrEmpty(model.origin_shop_id))
                    {
                        _result.msg = "请输入您在达达添加的门店编号";
                        return Json(_result);
                    }
                    if (string.IsNullOrEmpty(model.enterprise_name))
                    {
                        _result.msg = "请输入您在达达注册的商户企业全称";
                        return Json(_result);
                    }
                    //添加
                    if (model.id == 0)
                    {
                        DadaMerchant merchantmodel = DadaMerchantBLL.SingleModel.GetModelByMId(model.sourceid);
                        if(merchantmodel!=null)
                        {
                            _result.msg = "该商户ID已被绑定，请重新输入";
                            return Json(_result);
                        }
                        DadaShop dadashopmodel = DadaShopBLL.SingleModel.GetModelByShopNo(model.origin_shop_id);
                        if(dadashopmodel!=null)
                        {
                            _result.msg = "该门店编号已被绑定，请重新输入";
                            return Json(_result);
                        }
                        bool success = DadaMerchantBLL.SingleModel.AddDadaMerchant(model,aId, storeId);
                        _result.msg = success ? "保存成功" : "保存失败";
                        _result.code = success ? 1 : 0;
                    }
                    //修改
                    else
                    {
                        string msg = "";
                        bool updateResult = DadaMerchantBLL.SingleModel.UpdateDadaMerchant(model,ref msg);
                        _result.msg = updateResult ? "保存成功" : "保存失败";
                        _result.code = updateResult ? 1 : 0;
                    }
                }
            }
            return Json(_result);
        }


        /// <summary>
        /// 快跑者配送配置
        /// </summary>
        /// <returns></returns>

        public ActionResult KPZPeiSongConfig(int aId=0, string act="", int storeId=0, KPZStoreRelation model = null)
        {
            //参数验证
            if (aId <= 0)
            {
                _result.msg = "参数错误";
                return Json(_result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                EditModel<KPZStoreRelation> em = new EditModel<KPZStoreRelation>();
                em.appId = aId;
                em.storeId = storeId;
                em.aId = aId;
                KPZStoreRelation storemodel = KPZStoreRelationBLL.SingleModel.GetModelBySidAndAid(aId, storeId);
                if (storemodel == null )
                {
                    storemodel = new KPZStoreRelation();
                }
                em.DataModel = storemodel;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    if (model == null)
                    {
                        _result.msg = "参数出错";
                        return Json(_result);
                    }
                    if (string.IsNullOrEmpty(model.TelePhone))
                    {
                        _result.msg = "请输入商户手机号";
                        return Json(_result);
                    }
                    if (string.IsNullOrEmpty(model.TeamToken))
                    {
                        _result.msg = "请输入与您合作的快跑者团队的token";
                        return Json(_result);
                    }
                    bool success = KPZStoreRelationBLL.SingleModel.AddStore(aId, storeId,model.TeamToken,model.TelePhone);
                    _result.code = success ? 1 : 0;
                    _result.msg = success ? "保存成功" : "保存失败";
                }
            }
            return Json(_result);
        }

        /// <summary>
        /// UU跑腿配置
        /// </summary>
        /// <returns></returns>
        public ActionResult UUPeiSongConfig(int aId = 0, string act = "", int storeId = 0,UUCustomerRelation model = null)
        {
            //参数验证
            if (aId <= 0)
            {
                _result.msg = "参数错误";
                return Json(_result);
            }
            //显示
            if (string.IsNullOrEmpty(act))
            {
                EditModel<UUCustomerRelation> em = new EditModel<UUCustomerRelation>();
                em.appId = aId;
                em.storeId = storeId;
                em.aId = aId;
                UUCustomerRelation storemodel = UUCustomerRelationBLL.SingleModel.GetModelByAid(aId, storeId,0);
                if (storemodel == null)
                {
                    storemodel = new UUCustomerRelation();
                }
                em.DataModel = storemodel;
                return View(em);
            }
            else
            {
                if (act == "edit")
                {
                    if (model == null)
                    {
                        _result.msg = "参数出错";
                        return Json(_result);
                    }
                    if (string.IsNullOrEmpty(model.Phone))
                    {
                        _result.msg = "请输入商户手机号";
                        return Json(_result);
                    }
                    if (string.IsNullOrEmpty(model.PhoneCode))
                    {
                        _result.msg = "请输入验证码";
                        return Json(_result);
                    }
                    UUBaseResult result = UUApi.BindUserSubmit(model.Phone,model.PhoneCode,model.CityName,model.CountyName);
                    if(result!=null && result.return_code=="ok"&&!string.IsNullOrEmpty(result.openid))
                    {
                        model.OpenId = result.openid;
                        model.UpdateTime = DateTime.Now;
                        if(model.Id<=0)
                        {
                            model.State = 0;
                            model.AId = aId;
                            model.StoreId = storeId;
                            model.AddTime = DateTime.Now;
                            UUCustomerRelationBLL.SingleModel.Add(model);
                        }
                        else
                        {
                            UUCustomerRelationBLL.SingleModel.Update(model);
                        }
                        _result.msg = "注册成功";
                        _result.code = 1;
                    }
                }
            }
            return Json(_result);
        }

        /// <summary>
        /// UU跑腿发送验证码
        /// </summary>
        /// <param name="aId"></param>
        /// <returns></returns>
        public ActionResult UUSendPhoneCode(int aId = 0,string phone="",int storeId=0)
        {
            //参数验证
            if (aId <= 0)
            {
                _result.msg = "参数错误";
                return Json(_result);
            }

            if(string.IsNullOrWhiteSpace(phone))
            {
                _result.msg = "请输入手机号码";
                return Json(_result);
            }

            UUBaseResult uuResult = UUApi.BindUserApply(phone);
            if(uuResult!=null)
            {
                _result.msg = uuResult.return_msg;
                _result.code = uuResult.return_code == "ok" ? 1 : 0;
            }
            else
            {
                _result.msg = "uu接口异常";
                _result.code = 0;
            }

            return Json(_result);
        }


        /// <summary>
        /// 解绑UU跑腿
        /// </summary>
        /// <param name="aId"></param>
        /// <returns></returns>
        public ActionResult UUCancelBind(int aId = 0,int storeId = 0,int id=0)
        {
            //参数验证
            if (aId <= 0 || id<=0)
            {
                _result.msg = "参数错误";
                return Json(_result);
            }

            UUCustomerRelation model = UUCustomerRelationBLL.SingleModel.GetModel(id);
            if(model==null)
            {
                _result.msg = "请刷新重试";
                return Json(_result);
            }
            if(model.AId!=aId || model.StoreId!=storeId)
            {
                _result.msg = "无效权限";
                return Json(_result);
            }
            
            UUBaseResult uuResult = UUApi.CancelBind(model.OpenId);
            if (uuResult != null)
            {
                _result.msg = uuResult.return_msg;
                if(uuResult.return_code=="ok")
                {
                    model.State = -1;
                    UUCustomerRelationBLL.SingleModel.Update(model,"state");
                    _result.code = 1;
                }
            }
            else
            {
                _result.msg = "uu接口异常";
                _result.code = 0;
            }

            return Json(_result);
        }
    }
}