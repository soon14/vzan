using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Stores;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntFreightTemplateBLL : BaseMySql<EntFreightTemplate>
    {
        #region 单例模式
        private static EntFreightTemplateBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntFreightTemplateBLL()
        {

        }

        public static EntFreightTemplateBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntFreightTemplateBLL();
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
        public int GetFreightTemplateCost(int freightTemplateId, int buyNum)
        {
            var freightMoney = 0;
            var freigthTemplat = freightTemplateId > 0 ? GetModel(freightTemplateId) ?? new EntFreightTemplate() : new EntFreightTemplate();
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
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type">运费模板类型（筛选）：1:满减模板,0:计件模板</param>
        /// <param name="templateId">模板ID</param>
        /// <returns></returns>
        public List<EntFreightTemplate> GetListByAppId(int appId, int pageIndex = 1, int pageSize = 10, int type = 0,int templateId = 0)
        {
            var whereSql = BuildWhereSql(appId: appId, type: type, templateId: templateId);
            return GetList($"{whereSql} AND state >= 0", pageSize, pageIndex, "*", "ID DESC");
        }

        public string BuildWhereSql(int appId = 0,int type = 0,int templateId = 0)
        {
            var whereSql = string.Empty;
            if (appId > 0 && templateId >= 0)
            {
                whereSql = $"aid = {appId}";
            }
            if (type == 1 && templateId == 0)
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"FullDiscount > 0" : $"{whereSql} AND FullDiscount > 0";
            }
            else if(templateId == 0)
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"FullDiscount = 0" : $"{whereSql} AND FullDiscount = 0";
            }
            //-999:获取官方默认运费模板
            if(templateId == -999)
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"aid = {templateId}" : $"{whereSql} AND aid = {templateId}";
            }
            if (templateId > 0)
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"Id = {templateId}" : $"{whereSql} AND Id = {templateId}";
            }
            return whereSql;
        }

        public List<EntFreightTemplate> ConvertApiModel(List<EntFreightTemplate> templates)
        {
            string areaCodes = string.Join(",", templates.Where(template => !string.IsNullOrWhiteSpace(template.AreaCode)).Select(template => template.AreaCode));
            //去除重复Code
            areaCodes = string.Join(",", areaCodes.SplitStr(",").GroupBy(areaCode => areaCode).Select(areaCode => areaCode.First()));
            if (string.IsNullOrWhiteSpace(areaCodes))
            {
                return templates;
            }
            //获取选中区域
            List<C_Area> areaList = C_AreaBLL.SingleModel.GetList($"Code in ({areaCodes.TrimEnd(',')})");
            //获取选中区域省份
            var parentArea = areaList.FindAll(area => area.ParentCode > 0 && areaList.FindIndex(findParentArea => findParentArea.Code == area.ParentCode) == -1)
                                     .GroupBy(areaCode => areaCode.ParentCode)
                                     .Select(areaCode => areaCode.First().ParentCode);
            if (parentArea.Count() > 0)
            {
                var provinceCodes = string.Join(",", parentArea);
                areaList.AddRange(C_AreaBLL.SingleModel.GetList($"Code in ({provinceCodes.TrimEnd(',')})"));
            }
            //遍历组成区域名称列表
            templates.ForEach((template) =>
            {
                if (!string.IsNullOrWhiteSpace(template.AreaCode))
                {
                    var templateArea = template.AreaCode.SplitStr(",").Select(code => int.Parse(code));
                    var selectCity = areaList.Where(area => area.ParentCode > 0 && templateArea.Contains(area.Code));
                    var selectProvinces = areaList.Where(area => area.ParentCode == 0 && (templateArea.Contains(area.Code) || selectCity.Count(city => city.ParentCode == area.Code) > 0));
                    foreach (var province in selectProvinces)
                    {
                        var citys = selectCity.Where(area => area.ParentCode == province.Code);
                        var isSelectAll = citys.Count() == 0 || citys.Count() == C_AreaBLL.SingleModel.GetSubArea(province.Code)?.Count;
                        var citysName = isSelectAll ? "全省/市" : string.Join(",", citys.Select(area => area.Name));
                        template.AreaName = $"{template.AreaName};<span style=\"color:#286090\">{province.Name}</span>({citysName})".Trim(';');
                        template.AreaCode = isSelectAll ? $"{template.AreaCode},{province.Code}" : template.AreaCode;
                    }
                }
            });
            return templates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cartIds"></param>
        /// <returns>"运费|优惠信息|是否在配送范围"</returns>
        public FreightInfo GetFreightFee(StoreConfigModel config, string cartIds, string province, string city)
        {
            FreightInfo info = null;
            //返回统一运费
            if (config.FreightPriceSwitch)
            {
                info = new FreightInfo { Fee = config.FreightPrice, Msg = "无优惠", IsVaild = true };
                return info;
            }
            //获取模板（未启用模板时，返回默认官方模板）
            EntFreightTemplate template = config.FreightTemplateId <= 0 ? GetDefaultTemplate() : GetModel(config.FreightTemplateId);
            //送货地址是否在商家配送范围（默认模板：全国配送）
            bool isInDeliveryArea = CheckDeliveryArea(template: template, province: province, city: city);
            if (!isInDeliveryArea)
            {
                info = new FreightInfo { Fee = 0, Msg = "送货地址超出配送范围", IsVaild = false };
                return info;
            }
            //获取购物车商品
            var shopingItem = EntGoodsCartBLL.SingleModel.GetMyCartById(cartIds.Trim(','));
            if (shopingItem?.Count <= 0)
            {
                info = new FreightInfo { Fee = 0, Msg = "无效商品", IsVaild = false };
                return info;
            }
            //获取运费
            return GetFreightFeeByTemplate(template, shopingItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public FreightInfo GetFreightFeeByTemplate(EntFreightTemplate template, List<EntGoodsCart> shopingItem)
        {
            FreightInfo info = null;
            //满额免运费
            if (template.FullDiscount > 0)
            {
                var totalPrice = shopingItem.Sum(item => item.Count * item.Price);
                var discountPrice = totalPrice >= template.FullDiscount ? 0 : template.BaseCost;
                info = new FreightInfo
                {
                    Fee = discountPrice,
                    Msg = discountPrice == 0 ? "满减免运费" : $"离免运费，还差购买{(template.FullDiscount - totalPrice) * 0.01}元商品",
                    IsVaild = true
                };
            }
            //按件计费
            else if (template.BaseCost > 0 && template.BaseCount > 0 && template.ExtraCost > 0)
            {
                var itemCount = shopingItem.Sum(item => item.Count);
                var extraCount = itemCount - template.BaseCount;
                var fee = extraCount > 0 ? extraCount * template.ExtraCost + template.BaseCost: template.BaseCost;
                info = new FreightInfo
                {
                    Fee = fee,
                    Msg = extraCount <= 0 ? "无优惠" : $"购买超出{extraCount}将，以每件{template.ExtraCost * 0.01}元续费",
                    IsVaild = true
                };
            }
            else
            {
                info = new FreightInfo { Msg = "无效运费模板" };
            }
            return info;
        }

        public EntFreightTemplate GetDefaultTemplate()
        {
            return GetModel($"aid = {-999} AND IsDefault = 1");
        }

        public bool CheckDeliveryArea(EntFreightTemplate template, string province, string city)
        {
            if (string.IsNullOrWhiteSpace(template.AreaCode))
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(province) || string.IsNullOrWhiteSpace(city))
            {
                return false;
            }

            var result = false;
            var areaCode = template.AreaCode.SplitStr(",").Select(code => int.Parse(code)).GroupBy(code => code).Select(code => code.First());
            var areaCodeStr = string.Join(",", areaCode).Trim(',');
            var provinceCode = string.Join(",", areaCode.Select(code => C_AreaBLL.SingleModel.GetParentCityCode(code)).Where(code => code > 0).Concat(areaCode)).Trim(',');

            var provinceSql = $"SELECT count(name) FROM (SELECT `Name` FROM {C_AreaBLL.SingleModel.TableName} WHERE Code in({provinceCode}) AND ParentCode = 0) as T1 WHERE NAME LIKE '%{province}%'";
            var citySql = $@"SELECT count(name) 
                               FROM (SELECT `Name` 
                                       FROM {C_AreaBLL.SingleModel.TableName}
                                      WHERE (ParentCode in({areaCodeStr}) or Code in ({areaCodeStr})) AND ParentCode > 0) as T1 
                              WHERE NAME LIKE '%{city}%'";

            result = GetCountBySql(provinceSql) > 0 &&
                         GetCountBySql(citySql) > 0;
            return result;
        }

        ///// <summary>
        ///// 运费模板是否被引用
        ///// </summary>
        ///// <param name="ftid"></param>
        ///// <param name="storeId"></param>
        ///// <returns></returns>
        //public bool IsReferred(int ftid, int aid)
        //{
        //    var goodsBll = new MiniappEntGoodsBLL();
        //    var freightIds = goodsBll.GetList($"aid={aid} and State >=0").Select(m => m.FreightIds).ToList();
        //    if (!freightIds.Any()) return false;
        //    var freightIdList = string.Join(",", freightIds).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //    return freightIdList.Distinct().Contains(ftid.ToString());
        //}
    }
}
