using DAL.Base;
using Entity.MiniApp.Fds;
using System;
using System.Linq;

namespace BLL.MiniApp.Fds
{
    public class FoodFreightTemplateBLL : BaseMySql<FoodFreightTemplate>
    {
        #region 单例模式
        private static FoodFreightTemplateBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodFreightTemplateBLL()
        {

        }

        public static FoodFreightTemplateBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodFreightTemplateBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 获取运费
        /// </summary>
        /// <param name="freightTemplateId"></param>
        /// <param name="buyNum"></param>
        /// <returns></returns>
        public int GetFoodFreightTemplateCost(int freightTemplateId, int buyNum)
        {
            var freightMoney = 0;
            var freigthTemplat = freightTemplateId > 0 ? GetModel(freightTemplateId) ?? new FoodFreightTemplate() : new FoodFreightTemplate();
            if (freigthTemplat.Id > 0)
            {
                if (buyNum > freigthTemplat.BaseCount)
                {
                    freightMoney = freigthTemplat.BaseCost + (buyNum - freigthTemplat.BaseCount) * freigthTemplat.ExtraCost;
                }
                else
                {
                    freightMoney = freigthTemplat.BaseCost;
                }
            }
            return freightMoney;
        }

        /// <summary>
        /// 运费模板是否被引用
        /// </summary>
        /// <param name="ftid"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public bool IsReferred(int ftid, int foodId)
        {
            var freightIds = FoodGoodsBLL.SingleModel.GetList($"FoodId={foodId} and State >=0").Select(m => m.FreightIds).ToList();
            if (!freightIds.Any()) return false;
            var freightIdList = string.Join(",", freightIds).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return freightIdList.Distinct().Contains(ftid.ToString());
        }
    }
}
