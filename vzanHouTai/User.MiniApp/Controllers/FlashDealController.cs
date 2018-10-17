//using AutoMapper;
using BLL.MiniApp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Tools;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Filters;
using Utility;

namespace User.MiniApp.Controllers
{
    //[RoutePrefix("FlashDeal")]
    //[Route("{action=Admin}")]
    [LoginFilter(parseAuthDataTo: "authData")]
    [RouteAuthCheck]
    public class FlashDealController : baseController
    {


        /// <summary>
        /// 管理秒杀活动
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Admin(XcxAppAccountRelation authData)
        {
            if (authData == null)
                return View("PageError", new Return_Msg() { Msg = "无效参数!", code = "500" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(authData.TId);
            bool isAvailable = false;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={authData.VersionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.ComsConfig))
                {
                    functionList.ComsConfigModel = JsonConvert.DeserializeObject<ComsConfig>(functionList.ComsConfig);
                }
                isAvailable = functionList.ComsConfigModel?.FlashDeal == 0;
            }
            ViewBag.appId = authData.Id;
            ViewBag.PageType = xcxTemplate.Type;
            ViewBag.isAvailable = isAvailable;
            ViewBag.versionId = authData.VersionId;
            return View();
        }

        /// <summary>
        /// 获取秒杀活动列表
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult Get(XcxAppAccountRelation authData = null, DateTime? queryBegin = null, DateTime? queryEnd = null, string title = null, int? state = null, int pageIndex = 1, int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                title = StringHelper.ReplaceSqlKeyword(title);
            }

            List<FlashDeal> deals = FlashDealBLL.SingleModel.GetListByPara(Aid: authData.Id, title: title, state: state, queryBegin: queryBegin, queryEnd: queryEnd, pageIndex: pageIndex, pageSize: pageSize);
            if (deals?.Count > 0)
            {
                string flashDealIds = string.Join(",", deals.Select(deal => deal.Id));

                List<FlashDealItem> items = FlashDealItemBLL.SingleModel.GetCountByFlashId(flashDealIds);
                deals.ForEach(deal => { deal.ItemCount = items.Count(item => item.DealId == deal.Id); });
            }

            int recordCount = 0;
            //if (pageIndex == 1)
            //{
            recordCount = FlashDealBLL.SingleModel.GetCountByPara(Aid: authData.Id, title: title, queryBegin: queryBegin, queryEnd: queryEnd);
            //  }
            return ReturnJson(isOk: true, message: "获取成功", data: new
            {
                deals = deals,
                total = recordCount,
                pageCount = Utility.Paging.PageInfo.GetPageCount(recordCount, pageSize)
            });
        }

        /// <summary>
        /// 新增秒杀活动
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Add(XcxAppAccountRelation authData = null, FlashDeal inputDeal = null, List<FlashDealItem> inputItem = null)
        {
            if (inputDeal == null || inputItem == null || inputItem.Count == 0)
            {
                return ReturnJson(message: "请完善填写秒杀设置");
            }

            string errorMsg = string.Empty;
            if (!FlashDealBLL.SingleModel.CheckInputVaild(inputDeal, out errorMsg))
            {
                return ReturnJson(message: errorMsg);
            }

            if (inputItem.Exists(item => !FlashDealItemBLL.SingleModel.CheckInputVaild(item, out errorMsg)))
            {
                return ReturnJson(message: errorMsg);
            }
            inputDeal.Aid = authData.Id;
            inputItem.ForEach(item => { item.Aid = authData.Id; });

            var result = FlashDealBLL.SingleModel.AddNewDeal(inputDeal, inputItem);

            return ReturnJson(isOk: result, message: result ? "添加成功" : "添加失败");
        }

        /// <summary>
        /// 更新秒杀活动
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(XcxAppAccountRelation authData, FlashDeal updateDeal = null)
        {
            if (updateDeal == null)
            {
                return ReturnJson(message: "请完善填写秒杀设置");
            }

            string errorMsg = string.Empty;
            if (!FlashDealBLL.SingleModel.CheckInputVaild(updateDeal, out errorMsg))
            {
                return ReturnJson(message: errorMsg);
            }
            if (!FlashDealBLL.SingleModel.Editable(FlashDealBLL.SingleModel.GetModel(updateDeal.Id)))
            {
                return ReturnJson(message: "非法操作");
            }

            bool result = FlashDealBLL.SingleModel.UpdateDeal(updateDeal);

            return ReturnJson(isOk: result, message: result ? "更新成功" : "更新失败");
        }

        /// <summary>
        /// 上架秒杀活动
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult onShelf(XcxAppAccountRelation authData, int? flashDealId = null)
        {
            if (!flashDealId.HasValue || flashDealId.Value <= 0)
            {
                return ReturnJson(message: "非法参数");
            }

            FlashDeal updateDeal = FlashDealBLL.SingleModel.GetModel(flashDealId.Value);
            if (!FlashDealBLL.SingleModel.CheckAuth(authData, updateDeal) || !FlashDealBLL.SingleModel.CheckOnShelf(updateDeal))
            {
                return ReturnJson(message: "非法操作");
            }
            if (updateDeal.End <= DateTime.Now)
            {
                return ReturnJson(message: "活动结束日期小于当前时间，请重新调整");
            }

            bool result = false;
            if (updateDeal.Begin <= DateTime.Now)
            {
                result = FlashDealBLL.SingleModel.StartNow(updateDeal);
            }
            else
            {
                result = FlashDealBLL.SingleModel.UpdateOnShelf(updateDeal);
            }
            return ReturnJson(isOk: result, message: result ? "更新成功" : "更新失败");
        }

        /// <summary>
        /// 上架秒杀活动
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult offShelf(XcxAppAccountRelation authData, int? flashDealId = null)
        {
            if (!flashDealId.HasValue || flashDealId.Value <= 0)
            {
                return ReturnJson(message: "非法参数");
            }

            string errorMsg = string.Empty;
            FlashDeal updateDeal = FlashDealBLL.SingleModel.GetModel(flashDealId.Value);
            if (!FlashDealBLL.SingleModel.CheckAuth(authData, updateDeal) || FlashDealBLL.SingleModel.CheckOnShelf(updateDeal))
            {
                return ReturnJson(message: "非法操作");
            }

            bool result = FlashDealBLL.SingleModel.UpdateOffShelf(updateDeal);

            return ReturnJson(isOk: result, message: result ? "更新成功" : "更新失败");
        }

        /// <summary>
        /// 开始秒杀活动
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Start(XcxAppAccountRelation authData, int? flashDealId = null)
        {
            if (!flashDealId.HasValue || flashDealId.Value <= 0)
            {
                return ReturnJson(message: "非法参数");
            }

            string errorMsg = string.Empty;
            FlashDeal updateDeal = FlashDealBLL.SingleModel.GetModel(flashDealId.Value);
            if (!FlashDealBLL.SingleModel.CheckAuth(authData, updateDeal) || !FlashDealBLL.SingleModel.CheckForStart(updateDeal))
            {
                return ReturnJson(message: "非法操作");
            }

            bool result = FlashDealBLL.SingleModel.StartNow(updateDeal);

            return ReturnJson(isOk: result, message: result ? "更新成功" : "更新失败");
        }

        /// <summary>
        /// 删除秒杀活动
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(XcxAppAccountRelation authData, int flashDealId = 0)
        {
            if (flashDealId <= 0)
            {
                return ReturnJson(message: "无效参数_flashDealId");
            }

            string errorMsg = string.Empty;
            FlashDeal deleteDeal = FlashDealBLL.SingleModel.GetModel(flashDealId);
            if (!FlashDealBLL.SingleModel.Editable(deleteDeal))
            {
                return ReturnJson(message: "不可删除");
            }

            bool result = FlashDealBLL.SingleModel.DeleteDeal(deleteDeal);

            return ReturnJson(isOk: result, message: result ? "更新成功" : "更新失败");
        }

        /// <summary>
        /// 获取秒杀物品
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetItem(XcxAppAccountRelation authData, int flashDealId = 0)
        {
            if (flashDealId <= 0)
            {
                return ReturnJson(message: "无效秒杀活动ID");
            }
            List<FlashDealItem> flashItems = FlashDealItemBLL.SingleModel.GetByDealId(flashDealId);
            if (flashItems.Exists(item => item.Aid != authData.Id))
            {
                return ReturnJson(message: "非法操作（秒杀商品）");
            }
            return ReturnJson(isOk: true, data: flashItems, message: "获取成功");
        }

        /// <summary>
        /// 获取秒杀物品
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult SelectItem(XcxAppAccountRelation authData, int flashDealId = 0)
        {
            object Items = null;
            int? pageType = XcxTemplateBLL.SingleModel.GetModel(authData.TId)?.Type;

            //参与活动中的物品

            List<FlashDealItem> usingItem = FlashDealItemBLL.SingleModel.GetUsingItem(authData.Id).FindAll(item => item.DealId != flashDealId);

            switch (pageType)
            {
                case (int)TmpType.小程序专业模板:
                    List<EntGoods> entProducts = EntGoodsBLL.SingleModel.GetListByFlashDealId(appId: authData.Id, pageIndex: 1, pageSize: 99999, flashDealId: flashDealId);
                    //List<FlashItemForSelect> formatItem = Mapper.Map<List<EntGoods>, List<FlashItemForSelect>>(entProducts);
                    //formatItem.ForEach(item => item.disabled = usingItem.Exists(used => item.SourceId == used.SourceId));
                    //Items = formatItem;
                    //Items = itemBLL.ConvertSelectItem(entProducts);
                    Items = entProducts?.Select(item => new
                    {
                        Id = item.id,
                        SourceId = item.id,//来源数据ID（必须）
                        Title = item.name,//标题（必须）
                        OrigPrice = item.price,//原价（必须）
                        DealPrice = item.price,//秒杀价（必须）
                        Specs = EntGoodsBLL.SingleModel.FormatSpecFlashDeal(good: item),//多规格
                        Stock = 1, // 库存（停用）
                        disabled = usingItem.Exists(used => item.id == used.SourceId)//是否可选（必须）
                    });
                    break;
            }

            return ReturnJson(isOk: true, message: "获取成功", data: Items);
        }

        /// <summary>
        /// 新增秒杀物品
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddItem(XcxAppAccountRelation authData, int flashDealId = 0, List<FlashDealItem> newItem = null)
        {
            if (newItem == null)
            {
                return ReturnJson(message: "请完善填写秒杀商品设置");
            }

            FlashDeal belongDeal = FlashDealBLL.SingleModel.GetModel(flashDealId);
            if (belongDeal == null)
            {
                return ReturnJson(message: "秒杀活动不存在");
            }
            if (!FlashDealBLL.SingleModel.Editable(belongDeal))
            {
                return ReturnJson(message: "秒杀活动未下架，不可编辑");
            }


            string errorMsg = string.Empty;
            if (newItem.Exists(item => !FlashDealItemBLL.SingleModel.CheckInputVaild(item, out errorMsg)))
            {
                return ReturnJson(message: errorMsg);
            }
            if (FlashDealItemBLL.SingleModel.CheckRepeatItem(newItems: newItem, authData: authData))
            {
                return ReturnJson(message: "添加失败，活动商品重复");
            }

            bool result = FlashDealItemBLL.SingleModel.AddItems(newItems: newItem, deal: belongDeal);

            return ReturnJson(isOk: result, message: result ? "添加成功" : "添加失败");
        }


        /// <summary>
        /// 更新秒杀物品
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateItem(XcxAppAccountRelation authData, FlashDealItem dealItem = null)
        {
            if (dealItem == null || dealItem.DealId <= 0)
            {
                return ReturnJson(message: "请完善填写秒杀商品设置");
            }

            FlashDeal belongDeal = FlashDealBLL.SingleModel.GetModel(dealItem.DealId);
            if (belongDeal == null)
            {
                return ReturnJson(message: "秒杀活动数据异常:NULL");
            }
            if (!FlashDealBLL.SingleModel.Editable(belongDeal))
            {
                return ReturnJson(message: "秒杀活动未下架，不可编辑");
            }


            string errorMsg = string.Empty;
            if (!FlashDealItemBLL.SingleModel.CheckInputVaild(dealItem, out errorMsg))
            {
                return ReturnJson(message: errorMsg);
            }

            bool result = false;
            if (dealItem.Id > 0)
            {
                result = FlashDealItemBLL.SingleModel.UpdateItem(dealItem);
            }
            else
            {
                result = FlashDealItemBLL.SingleModel.AddItem(item: dealItem, deal: belongDeal);
            }

            return ReturnJson(isOk: result, message: result ? "添加成功" : "添加失败");
        }

        /// <summary>
        /// 删除秒杀物品
        /// </summary>
        /// <param name="authData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteItem(XcxAppAccountRelation authData, int flashDealId = 0, List<FlashDealItem> deleteItem = null)
        {
            if (deleteItem == null)
            {
                return ReturnJson(message: "请完善填写秒杀商品设置");
            }

            FlashDeal belongDeal = FlashDealBLL.SingleModel.GetModel(flashDealId);
            if (belongDeal == null)
            {
                return ReturnJson(message: "秒杀活动不存在");
            }
            if (!FlashDealBLL.SingleModel.Editable(belongDeal))
            {
                return ReturnJson(message: "秒杀活动未下架，不可编辑");
            }

            if (FlashDealItemBLL.SingleModel.GetByDealId(flashDealId)?.Count == 1)
            {
                return ReturnJson(message: "不可删除，活动需要至少一个秒杀商品");
            }
            string errorMsg = string.Empty;
            if (deleteItem.Exists(item => item.Aid != belongDeal.Aid || item.DealId != belongDeal.Id))
            {
                return ReturnJson(message: "非法操作");
            }

            bool result = FlashDealItemBLL.SingleModel.DelItems(items: deleteItem);

            return ReturnJson(isOk: result, message: result ? "删除成功" : "删除失败");
        }
    }
}