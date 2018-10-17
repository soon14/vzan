using BLL.MiniApp.Ent;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Tools
{
    public class FlashDealBLL : BaseMySql<FlashDeal>
    {
        #region 单例模式
        private static FlashDealBLL _singleModel;
        private static readonly object SynObject = new object();

        private FlashDealBLL()
        {

        }

        public static FlashDealBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FlashDealBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 新增秒杀活动
        /// </summary>
        /// <param name="newDeal">秒杀活动</param>
        /// <param name="newItem">秒杀物品</param>
        /// <returns></returns>
        public bool AddNewDeal(FlashDeal newDeal, List<FlashDealItem> newItem)
        {
            int newId = 0;
            newDeal.AddTime = DateTime.Now;
            newDeal.UpdateTime = DateTime.Now;
            if (!int.TryParse(Add(newDeal).ToString(), out newId) || newId == 0)
            {
                return false;
            }
            newDeal.Id = newId;
            newItem.ForEach((item) =>
            {
                item.DealId = newId;
                item.Aid = newDeal.Aid;
            });
            return FlashDealItemBLL.SingleModel.AddItems(newItem, newDeal);
        }

        /// <summary>
        /// 更新秒杀活动
        /// </summary>
        /// <param name="deal"></param>
        /// <returns></returns>
        public bool UpdateDeal(FlashDeal deal)
        {
            deal.UpdateTime = DateTime.Now;
            bool result = Update(deal, "Title,Banner,Begin,End,AmountLimit,OrderLimit,UpdateTime,Discount,Description");
            if(result)
            {
                Task.Factory.StartNew(() =>
                {
                    SubscribeMessageBLL.SingleModel.DeleteSubscribe(deal);
                    FlashDealItemBLL.SingleModel.UpdateSubscribeCacheVer(deal.Id);
                });
            }
            return result;
        }

        private bool UpdateState(FlashDeal deal, FlashDealState state)
        {
            deal.State = (int)state;
            deal.UpdateTime = DateTime.Now;
            return Update(deal, "State,UpdateTime");
        }

        private bool UpdateState(List<FlashDeal> deal, FlashDealState state)
        {
            TransactionModel _tranModel = new TransactionModel();
            deal.ForEach(item =>
            {
                item.State = (int)state;
                _tranModel.Add(BuildUpdateSql(item, "State"));
            });
            return ExecuteTransactionDataCorect(_tranModel.sqlArray, _tranModel.ParameterArray);
        }

        public bool UpdateOffShelf(FlashDeal deal)
        {
            bool result = UpdateState(deal, FlashDealState.已下架);
            if(result)
            {
                //活动下架，清空活动开始提醒
                Task.Factory.StartNew(() =>
                {
                    SubscribeMessageBLL.SingleModel.DeleteSubscribe(deal);
                    FlashDealItemBLL.SingleModel.UpdateSubscribeCacheVer(deal.Id);
                });
            }
            return result;
        }

        public bool UpdateOnShelf(FlashDeal deal)
        {
            bool result = UpdateState(deal, FlashDealState.已上架);
            if (result)
            {
                //活动上架，恢复推送“秒杀开始通知”
                Task.Factory.StartNew(() =>
                {
                    SubscribeMessageBLL.SingleModel.DeleteSubscribe(deal);
                    FlashDealItemBLL.SingleModel.UpdateSubscribeCacheVer(deal.Id);
                });
            }
            return result;
        }

        public bool StartNow(FlashDeal deal)
        {
            deal.State = (int)FlashDealState.已开始;
            deal.UpdateTime = DateTime.Now;
            deal.Begin = DateTime.Now;
            bool result = Update(deal, "State,Begin,UpdateTime");
            if(result)
            {
                Task.Factory.StartNew(() =>
                {
                    SubscribeMessageBLL.SingleModel.UpdateSendTime(deal);
                });
            }
            return result;
        }

        public bool BeginDeals(List<FlashDeal> deals)
        {
            return UpdateState(deals, FlashDealState.已开始);
        }

        /// <summary>
        /// 删除秒杀活动
        /// </summary>
        /// <param name="deal"></param>
        /// <returns></returns>
        public bool DeleteDeal(FlashDeal deal)
        {
            return UpdateDealToInvalid(deal, FlashDealState.已删除);
        }

        public bool ExpireByManual(FlashDeal expireDeal)
        {
            return UpdateDealToInvalid(expireDeal, FlashDealState.已结束);
        }

        public bool ExpireByServer(FlashDeal expireDeal)
        {
            return UpdateDealToInvalid(expireDeal, FlashDealState.已结束);
        }

        private bool UpdateDealToInvalid(FlashDeal deal, FlashDealState updateState)
        {
            deal.State = (int)updateState;

            string delDealSql = BuildUpdateSql(deal, "State");
            
            List<FlashDealItem> dealItems = FlashDealItemBLL.SingleModel.GetByDealId(deal.Id);

            bool result = true;
            if(updateState == FlashDealState.已结束)
            {
                //停用/释放秒杀商品
                result = FlashDealItemBLL.SingleModel.ExpireItem(items: dealItems, appendSql: delDealSql);
            }
            else if(updateState == FlashDealState.已删除)
            {
                //删除秒杀物品
                result = FlashDealItemBLL.SingleModel.DelItems(items: dealItems, appendSql: delDealSql);
            }
            if(result && updateState == FlashDealState.已删除)
            {
                //删除用户订阅
                Task.Factory.StartNew(() =>
                {
                    SubscribeMessageBLL.SingleModel.DeleteSubscribe(dealItems);
                });
            }
            return result;
        }

        public bool CheckInputVaild(FlashDeal input, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (input.Begin >= input.End)
            {
                errorMsg = "开始时间必须小于结束时间";
                return false;
            }
            if (string.IsNullOrWhiteSpace(input.Title))
            {
                errorMsg = "标题不能为空";
                return false;
            }
            if (input.AmountLimit <= 0)
            {
                errorMsg = "限购数量必须大于零";
                return false;
            }
            if (input.OrderLimit < 0)
            {
                errorMsg = "用户参与设置异常";
                return false;
            }
            return true;
        }

        public bool CheckAuth(XcxAppAccountRelation authData, FlashDeal flashDeal)
        {
            return authData.Id == flashDeal.Aid;
        }

        /// <summary>
        /// 是否允许下架
        /// </summary>
        /// <param name="flashDeal"></param>
        /// <returns></returns>
        public bool CheckOffShelf(FlashDeal flashDeal)
        {
            return flashDeal.State != (int)FlashDealState.已上架 && flashDeal.State != (int)FlashDealState.已开始;
        }

        /// <summary>
        /// 是否允许上架
        /// </summary>
        /// <param name="flashDeal"></param>
        /// <returns></returns>
        public bool CheckOnShelf(FlashDeal flashDeal)
        {
            return flashDeal.State == (int)FlashDealState.已下架;
        }

        /// <summary>
        /// 是否允许开始
        /// </summary>
        /// <param name="flashDeal"></param>
        /// <returns></returns>
        public bool CheckForStart(FlashDeal flashDeal)
        {
            return flashDeal.State == (int)FlashDealState.已上架 || flashDeal.State == (int)FlashDealState.已下架;
        }

        public bool Editable(FlashDeal deal)
        {
            return deal.State == (int)FlashDealState.已下架;
        }

        public FlashDealPayInfo GetFlashDealPayment(FlashDealItem dealItem, int userId)
        {
            FlashDealPayInfo payment = new FlashDealPayInfo();
            FlashDeal deal = GetModel(dealItem.DealId);
            switch (deal.State)
            {
                case (int)FlashDealState.已删除:
                    payment.Info = "秒杀活动已删除，不可支付";
                    break;
                case (int)FlashDealState.已下架:
                    payment.Info = "秒杀活动已下架，不可支付";
                    break;
                case (int)FlashDealState.已结束:
                    payment.Info = "秒杀活动已结束，不可支付";
                    break;
                default:
                    if (deal.OrderLimit > 0 && FlashDealPaymentBLL.SingleModel.isPaid(dealItem, userId))
                    {
                        payment.Info = "已参与过秒杀该商品，不可支付";
                    }
                    else if (deal.End <= DateTime.Now)
                    {
                        payment.Info = "秒杀活动已结束，不可支付";
                    }
                    else
                    {
                        payment.IsPay = true;
                        payment.Info = "秒杀活动正常，可支付";
                    }
                    break;
            }
            payment.FlashItemId = dealItem.Id;
            payment.FlashDealId = deal.Id;
            return payment;
        }

        public List<FlashDeal> GetExpireDeal(int amount)
        {
            string whereSql = BuildWhereSql(queryEnd: DateTime.Now, state: (int)FlashDealState.已开始);
            return GetList(strWhere: whereSql, PageIndex: 1, PageSize: amount);
        }

        public List<FlashDeal> GetWaitForStart(int amount)
        {
            string whereSql = BuildWhereSql(IsGetStarted: true, state: (int)FlashDealState.已上架);
            return GetList(whereSql, amount, 1);
        }

        public List<FlashDeal> GetByAidAndFlashIds(int Aid, string flashIds)
        {
            string whereSql = BuildWhereSql(Aid: Aid);
            return GetList($"({whereSql}) AND ID in({flashIds})");
        }

        public List<FlashDeal> GetListByPara(int? Aid = null, string title = null, int? state = null, DateTime? queryBegin = null, DateTime? queryEnd = null, int pageIndex = 1, int pageSize = 10)
        {
            string whereSql = BuildWhereSql(Aid: Aid, title: title, state: state, queryBegin: queryBegin, queryEnd: queryEnd);
            return GetList(whereSql, pageSize, pageIndex, "*", "ID DESC");
        }

        public int GetCountByPara(int? Aid = null, string title = null, DateTime? queryBegin = null, DateTime? queryEnd = null)
        {
            string whereSql = BuildWhereSql(Aid: Aid, title: title, queryBegin: queryBegin, queryEnd: queryEnd);
            return GetCount(whereSql);
        }

        /// <summary>
        /// 序列化为专业版接口数据
        /// </summary>
        /// <returns></returns>
        public List<object> FormatForEnt(List<FlashDeal> flashDeals,int userId = 0)
        {
            string flashDealIds = string.Join(",", flashDeals.Select(item => item.Id));
            
            List<FlashDealItem> flashItem = FlashDealItemBLL.SingleModel.GetByDealIds(flashDealIds);
            string flashItemIds = string.Join(",", flashItem.Select(item => item.SourceId));
            List<EntGoods> itemsDetail = EntGoodsBLL.SingleModel.GetListByIds(flashItemIds);

            
            List<object> formatFlashDeal = new List<object>();
            flashDeals.ForEach(flashDeal =>
            {
                List<FlashDealItem> thisDealItem = flashItem.FindAll(item => item.DealId == flashDeal.Id);
                List<object> formatItem = new List<object>();
                thisDealItem.ForEach(item =>
                {
                    EntGoods detail = itemsDetail.FirstOrDefault(sourceItem => sourceItem.id == item.SourceId);
                    int stock = detail.stock;
                    if (detail.GASDetailList?.Count > 0)
                    {
                        stock += detail.GASDetailList.Sum(spec => spec.stock);
                    }
                    if(item.GetSpecs()?.Count>0)
                    {
                        //显示秒杀规格最低价
                        item.DealPrice = item.GetSpecs().OrderBy(spec => spec.DealPrice).First().DealPrice;
                    }
                    formatItem.Add(new
                    {
                        FlashItemId = item.Id,
                        GoodId = detail.id,
                        Title = detail.name,
                        Img = detail.img,
                        OrigPrice = double.Parse((item.OrigPrice * 0.01).ToString()),
                        DealPrice = double.Parse((item.DealPrice * 0.01).ToString()),
                        Sale = FlashDealPaymentBLL.SingleModel.GetSaleCountByEnt(item.Id),
                        Stock = stock,
                        StockLimit = detail.stockLimit,
                        IsNotify = FlashDealItemBLL.SingleModel.CheckSubscribeMark(item, userId),
                        SubVer = RedisUtil.Get<int?>(string.Format("subscribe_flashItem_{0}_{1}", item.Id, userId)),
                        Ver = FlashDealItemBLL.SingleModel.GetSubscribeCacheVer(item.DealId)
                    });
                });
                formatFlashDeal.Add(new
                {
                    Id = flashDeal.Id,
                    State = flashDeal.State,
                    Title = flashDeal.Title,
                    Banner = flashDeal.Banner,
                    Begin = flashDeal.Begin,
                    End = flashDeal.End,
                    Item = formatItem,
                    description = flashDeal.Description,
                });
            });
            return formatFlashDeal;
        }

        private string BuildWhereSql(int? Aid = null, string title = null, int? state = null, DateTime? queryBegin = null, DateTime? queryEnd = null, bool IsGetStarted = false, bool isGetDelete = false)
        {
            List<string> whereSql = new List<string>();
            if (Aid.HasValue)
            {
                whereSql.Add($"Aid = {Aid}");
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                whereSql.Add($"Title LIKE '%{title}%'");
            }
            if (state.HasValue)
            {
                whereSql.Add($"State = {state.Value}");
            }
            if (queryBegin.HasValue)
            {
                whereSql.Add($"Begin >= '{queryBegin.Value.ToString()}'");
            }
            if (queryEnd.HasValue)
            {
                whereSql.Add($"End <= '{queryEnd.Value.ToString()}'");
            }
            if (IsGetStarted)
            {
                whereSql.Add($"Begin <= '{DateTime.Now.ToString()}'");
            }
            if (!isGetDelete)
            {
                whereSql.Add($"State != {(int)FlashDealState.已删除}");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
