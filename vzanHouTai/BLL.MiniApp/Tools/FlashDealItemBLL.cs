//using AutoMapper;
using BLL.MiniApp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Tools;
using DAL.Base;
using Entity.MiniApp.Ent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Tools
{
    public class FlashDealItemBLL : BaseMySql<FlashDealItem>
    {
        #region 单例模式
        private static FlashDealItemBLL _singleModel;
        private static readonly object SynObject = new object();

        private FlashDealItemBLL()
        {

        }

        public static FlashDealItemBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FlashDealItemBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 用户已订阅活动，模板消息提醒，缓存Key（{0}=flashItemId,{1}=userId）
        /// </summary>
        private const string _subscribeMark = "subscribe_flashItem_{0}_{1}";
        /// <summary>
        /// 用户订阅活动，模板消息版本，缓存版本Key（{0}=flashDealId）
        /// </summary>
        private const string _subscribeVer = "subscribeVer_flashDeal_{0}";

        public bool AddItem(FlashDealItem item, FlashDeal deal)
        {
            return AddItems(new List<FlashDealItem> { item }, deal);
        }

        public bool AddItems(List<FlashDealItem> newItems, FlashDeal deal)
        {
            TransactionModel tranModel = new TransactionModel();
            newItems.ForEach((item) =>
            {
                item.State = 1;
                item.DealId = deal.Id;
                item.Aid = deal.Aid;
                tranModel.Add(BuildAddSql(item));
            });
            return ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
        }

        public bool CheckAvailable(EntGoods good)
        {
            return CheckAvailable(good.id, good.aid);
        }

        private bool CheckAvailable(int sourceId,int aid)
        {
            string whereSql = BuildWhereSql(sourceId: sourceId, Aid: aid);
            return GetCount(whereSql) > 0;
        }

        public bool DelItems(List<FlashDealItem> items, string appendSql = null)
        {
            TransactionModel tranModel = new TransactionModel();
            items.ForEach(item => { item.State = -1; tranModel.Add(BuildUpdateSql(item, "State")); });
            if(!string.IsNullOrWhiteSpace(appendSql))
            {
                tranModel.Add(appendSql);
            }
            bool result = ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            if (result)
            {
                //删除用户订阅
                Task.Factory.StartNew(() =>
                {
                    SubscribeMessageBLL.SingleModel.DeleteSubscribe(items);
                });
            }
            return result;
        }

        public bool DelItem(FlashDealItem item)
        {
            bool result = DelItems(new List<FlashDealItem>() { item });
            if(result)
            {
                //删除用户订阅
                Task.Factory.StartNew(() =>
                {
                    SubscribeMessageBLL.SingleModel.DeleteSubscribe(item);
                });
            }
            return result;
        }

        public bool ExpireItem(List<FlashDealItem> items, string appendSql = null)
        {
            TransactionModel tranModel = new TransactionModel();
            items.ForEach(item => { item.State = 0; tranModel.Add(BuildUpdateSql(item, "State")); });
            if (!string.IsNullOrWhiteSpace(appendSql))
            {
                tranModel.Add(appendSql);
            }
            return ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
        }

        public bool UpdateItem(FlashDealItem item)
        {
            return Update(item, "Title,OrigPrice,DealPrice,Discount,Specs");
        }

        public bool CheckInputVaild(FlashDealItem item, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (item.DealPrice <= 0)
            {
                errorMsg = "请正确填写秒杀物品的秒杀价格";
                return false;
            }
            if(string.IsNullOrWhiteSpace(item.Title))
            {
                errorMsg = "秒杀物品标题不能为空";
                return false;
            }
            List<FlashItemSpec> itemSpec = null;
            if(!string.IsNullOrWhiteSpace(item.Specs))
            {
                try { itemSpec = JsonConvert.DeserializeObject<List<FlashItemSpec>>(item.Specs); } catch { }
            }
            if (itemSpec?.Count > 0)
            {
                FlashItemSpec invalidInput = itemSpec.FirstOrDefault(spec => spec.DealPrice <= 0);
                if (invalidInput != null)
                {
                    errorMsg = $"请正确设置{invalidInput.Name}的秒杀价格";
                    return false;
                }
            }
            return true;
        }

        public bool CheckRepeatItem(XcxAppAccountRelation authData, List<FlashDealItem> newItems)
        {
            string sourceIds = string.Join(",", newItems.Where(item => item.SourceId > 0).Select(item => item.SourceId));
            if (string.IsNullOrWhiteSpace(sourceIds))
            {
                return false;
            }
            string whereSql = BuildWhereSql(Aid: authData.Id, sourceIds: sourceIds, state: 1);
            return GetCount(whereSql) > 0;
        }

        public bool AddSubscribeMark(FlashDealItem flashItem, int userId)
        {
            int currVer = GetSubscribeCacheVer(flashItem.DealId);
            return RedisUtil.Set<int?>(GetSubsribeMarkKey(flashItem.Id, userId), currVer, TimeSpan.FromDays(7));
        }

        public bool CheckSubscribeMark(FlashDealItem flashItem, int userId)
        {
            int currVer = GetSubscribeCacheVer(flashItem.DealId);
            int? subscribeVer = RedisUtil.Get<int?>(GetSubsribeMarkKey(flashItem.Id, userId));
            return subscribeVer.HasValue && subscribeVer.Value == currVer; 
        }

        public int GetSubscribeCacheVer(int flashDealId)
        {
            return RedisUtil.GetVersion(GetSubsribeVerKey(flashDealId));
        }

        public void UpdateSubscribeCacheVer(int flashDealId)
        {
            RedisUtil.SetVersion(GetSubsribeVerKey(flashDealId));
        }

        private string GetSubsribeVerKey(int flashDealId)
        {
            return string.Format(_subscribeVer, flashDealId);
        }

        private string GetSubsribeMarkKey(int flashDealId, int userId)
        {
            return string.Format(_subscribeMark, flashDealId, userId);
        }

        public List<FlashDealItem> GetByDealId(int dealId)
        {
            return GetByDealIds(dealId.ToString());
        }

        public List<FlashDealItem> GetByDealIds(string dealIds)
        {
            string whereSql = BuildWhereSql(dealIds);
            return GetList(whereSql);
        }

        public FlashDealItem GetBySourceIdAid(int sourceId,int Aid, int? state = null)
        {
            string whereSql = BuildWhereSql(sourceId: sourceId, Aid: Aid, state: state);
            return GetModel(whereSql);
        }

        public FlashDealItem GetBySourceIdAid(int sourceId, int Aid, int flashDealId)
        {
            string whereSql = BuildWhereSql(sourceId: sourceId, Aid: Aid, dealIds: flashDealId.ToString());
            return GetModel(whereSql);
        }

        public List<FlashDealItem> GetByAid(int Aid)
        {
            string whereSql = BuildWhereSql(Aid: Aid);
            return GetList(whereSql);
        }

        public List<FlashDealItem> GetUsingItem(int Aid)
        {
            string whereSql = BuildWhereSql(Aid: Aid, state: 1);
            return GetList(whereSql);
        }

        //public FlashDealItem GetByEntGood(EntGoods good)
        //{
        //    return GetBySourceIdAid(sourceId: good.id, Aid: good.aid, state: (int)FlashItemState.使用中);
        //}

        public EntGoods GetFlashDealPrice(EntGoods good,FlashDealItem flashItem)
        {
            if (flashItem == null || flashItem.Id == 0)
            {
                return good;
            }

            List<FlashItemSpec> flashItemSpecs = flashItem.GetSpecs();
            List<EntGoodsAttrDetail> formatAttr = good.GASDetailList?.Where(attr => flashItemSpecs.Exists(spec => spec.Id == attr.id)).ToList();
            formatAttr?.ForEach(attr =>
            {
                FlashItemSpec spec = flashItemSpecs.Find(item => item.Id == attr.id);
                attr.discountPrice = float.Parse((spec.DealPrice * 0.01).ToString("0.00"));
                attr.originalPrice = float.Parse((spec.OrigPrice * 0.01).ToString("0.00"));
            });
            good.specificationdetail = JsonConvert.SerializeObject(formatAttr);
            good.originalPrice = float.Parse((flashItem.OrigPrice * 0.01).ToString("0.00"));
            good.discountPrice = float.Parse((flashItem.DealPrice * 0.01).ToString("0.00"));
            return good;
        }

        public EntGoodsCart GetFlashDealPrice(EntGoodsCart cartItem, FlashDealItem flashItem)
        {
            if (flashItem == null || flashItem.Id == 0)
            {
                return cartItem;
            }
            FlashItemSpec flashItemSpec = null;
            if (!string.IsNullOrWhiteSpace(cartItem.SpecIds))
            {
                flashItemSpec = flashItem.GetSpecs().FirstOrDefault(spec => spec.Id == cartItem.SpecIds);
            }
            if (flashItemSpec != null)
            {
                cartItem.Price = flashItemSpec.DealPrice;
                cartItem.originalPrice = flashItemSpec.OrigPrice;
            }
            else
            {
                cartItem.Price = flashItem.DealPrice;
                cartItem.originalPrice = flashItem.OrigPrice;
            }
            //折扣最低0.01元
            //cartItem.Price = cartItem.Price <= 0 ? 1 : cartItem.Price;
            return cartItem;
        }

        public List<FlashDealItem> GetCountByFlashId(string flashIds)
        {
            string whereSql = BuildWhereSql(dealIds: flashIds);
            return GetList(whereSql, 9999, 1, "DealId");
        }

        private string BuildWhereSql(string dealIds = null, int? sourceId = null, string sourceIds = null ,int? Aid = null, bool isGetDel = false, int? state = null)
        {
            List<string> whereSql = new List<string>();
            if(state.HasValue)
            {
                whereSql.Add($"State = {state.Value}");
            }
            else if(!isGetDel)
            {
                whereSql.Add("State >= 0");
            }
            if(!string.IsNullOrWhiteSpace(dealIds))
            {
                whereSql.Add($"DealId in ({dealIds})");
            }
            if(sourceId.HasValue)
            {
                whereSql.Add($"SourceId = {sourceId.Value}");
            }
            if(Aid.HasValue)
            {
                whereSql.Add($"Aid = {Aid.Value}");
            }
            if(!string.IsNullOrWhiteSpace(sourceIds))
            {
                whereSql.Add($"SourceId IN ({sourceIds})");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
