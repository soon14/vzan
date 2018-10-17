using DAL.Base;
using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Stores;
using Entity.MiniApp;
using BLL.MiniApp.Ent;
using Utility;
using Core.MiniApp.Common;
using Entity.MiniApp.PlatChild;
using BLL.MiniApp.PlatChild;
using Entity.MiniApp.Plat;
using BLL.MiniApp.Plat;
using Newtonsoft.Json;
using BLL.MiniApp.Qiye;
using Entity.MiniApp.Qiye;
using BLL.MiniApp.Pin;
using Entity.MiniApp.Pin;
using System.Data;
using MySql.Data.MySqlClient;

namespace BLL.MiniApp.Tools
{
    public class DeliveryTemplateBLL : BaseMySql<DeliveryTemplate>
    {
        #region 单例模式
        private static DeliveryTemplateBLL _singleModel;
        private static readonly object SynObject = new object();

        private DeliveryTemplateBLL()
        {

        }

        public static DeliveryTemplateBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DeliveryTemplateBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public string GetDeliveryTemplateName(int templateId)
        {
            return base.GetModel(templateId)?.Name;
        }


        /// <summary>
        /// 通过运费模板名称获取模板Id
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public int GetDeliveryTemplateId(int aid, string templateName)
        {
            string sql = $"SELECT id from DeliveryTemplate where aid={aid} and Name=@templateName and State=0";
           
            object id = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                  CommandType.Text, sql,
                  new MySqlParameter[] { new MySqlParameter("@templateName", templateName) });
            if (id != DBNull.Value)
            {
                return Convert.ToInt32(id);
            }
            return 0;

        }



        public int AddTemplate(int appId, DeliveryTemplate newTemplate)
        {
            newTemplate.UpdateTime = DateTime.Now;
            newTemplate.State = 0;
            newTemplate.Id = 0;
            newTemplate.Aid = appId;
            int newId = 0;
            object result = Add(newTemplate);
            if (int.TryParse(result.ToString(), out newId) && newId > 0)
            {
                return newId;
            }
            return 0;
        }

        public bool UpdateTemplate(int templateId, DeliveryTemplate newTemplate)
        {
            newTemplate.UpdateTime = DateTime.Now;
            newTemplate.Id = templateId;
            return Update(newTemplate, "Base,BaseCost,DeliveryRegion,Extra,ExtraCost,IsFree,Name,UnitType,UpdateTime,EnableDiscount,Discount");
        }

        public bool UpdateRegionTemplate(int templateId, int aid, List<DeliveryTemplate> regionTemplate)
        {
            TransactionModel tran = new TransactionModel();
            regionTemplate.ForEach((template) =>
            {
                template.Aid = aid;
                template.ParentId = templateId;
                template.UpdateTime = DateTime.Now;
                template.DeliveryRegionSub = template.DeliveryRegionSub ?? string.Empty;
                if (template.Id > 0)
                {
                    tran.Add(BuildUpdateSql(template, "Base,BaseCost,Extra,ExtraCost,IsFree,UnitType,DeliveryRegion,DeliveryRegionSub,UpdateTime"));
                }
                else
                {
                    tran.Add(BuildAddSql(template));
                }
            });
            bool result = ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            return result;
        }

        public bool DeleteTemplate(DeliveryTemplate delTemplate)
        {
            delTemplate.State = -1;
            return Update(delTemplate, "State");
        }

        public bool CheckRegionCode(string regionCodes)
        {
            return regionCodes.SplitStr(",").TrueForAll(regionCode => StringHelper.IsNumber(regionCode));
        }

        /// <summary>
        /// 获取管理运费模板列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<DeliveryTemplate> GetByAid(int appId, int? storeId = null, int pageIndex = 1, int pageSize = 10, int? unitType = null)
        {
            string whereSql = BuildWhereSql(appId: appId, unitType: unitType, storeId: storeId);
            return GetList(whereSql, pageSize, pageIndex);
        }

        /// <summary>
        /// 通过ID数组获取运费模板
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<DeliveryTemplate> GetTemplateByIds(string ids)
        {
            string whereSql = BuildWhereSql(ids: ids);
            return GetList(whereSql);
        }

        public List<DeliveryTemplate> GetRegionTemplate(int ParentId)
        {
            string whereSql = BuildWhereSql(parentId: ParentId);
            return GetList(whereSql);
        }

        public DeliveryDiscount GetDiscount(int aid, int amount)
        {
            DeliveryConfig config = DeliveryConfigBLL.SingleModel.GetStoreConfig(aid);
            bool hasDiscount = config != null && config.Attr.DiscountEnable && config.Attr.Discount <= amount;
            return new DeliveryDiscount
            {
                HasDiscount = hasDiscount,
                DiscountPrice = hasDiscount ? 0 : amount,
                DiscountInfo = hasDiscount ? $"购买金额满{config.Attr.Discount * 0.01}元，减免运费 " : null,
            };
        }

        /// <summary>
        /// 构建WhereSql语句
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public string BuildWhereSql(int appId = 0, int? storeId = null, string ids = null, int? unitType = null, int? parentId = null)
        {
            string whereSql = "State = 0";
            if (appId > 0)
            {
                whereSql = $"{whereSql} AND Aid ={appId}";
            }
            if (!string.IsNullOrWhiteSpace(ids))
            {
                whereSql = $"{whereSql} AND Id in ({ids})";
            }
            if (unitType.HasValue && Enum.IsDefined(typeof(DeliveryUnit), unitType.Value))
            {
                whereSql = $"{whereSql} AND UnitType = {unitType.Value}";
            }
            if (parentId.HasValue)
            {
                whereSql = $"{whereSql} AND ParentId = {parentId.Value}";
            }
            else
            {
                whereSql = $"{whereSql} AND (ParentId is null OR ParentId = 0)";
            }
            if(storeId.HasValue)
            {
                whereSql = $"{whereSql} AND StoreId = {storeId.Value}";
            }
            return whereSql;
        }

        /// <summary>
        /// 获取商品运费（通用业务）
        /// </summary>
        /// <param name="productInfo">产品信息</param>
        /// <param name="provinces">配送省份</param>
        /// <param name="city">配送市区</param>
        /// <param name="sumMethod">运费结算规则</param>
        /// <returns></returns>
        public DeliveryFeeResult GetDeliveryFeeCommon(List<DeliveryProduct> productInfo, string provinces, string city, DeliveryFeeSumMethond sumMethod)
        {
            DeliveryFeeResult result = new DeliveryFeeResult();
            if (string.IsNullOrWhiteSpace(provinces) || string.IsNullOrWhiteSpace(city))
            {
                result.Message = "无效配送地址";
                return result;
            }

            //获取商品
            if (productInfo == null || productInfo.Count == 0)
            {
                result.Message = "无效产品信息";
                return result;
            }

            //获取运费模板ID
            string templateIds = string.Join(",", productInfo.Select(product => product.TemplateId).Where(templateId => templateId > 0));
            if (string.IsNullOrWhiteSpace(templateIds))
            {
                //没有选择运费模板，默认零元
                result.InRange = true;
                result.Message = "没有产品使用运费模板，返回零元";
                return result;
            }

            List<DeliveryTemplate> productTemplates = GetTemplateByIds(templateIds);
            if (productTemplates == null || productTemplates.Count == 0)
            {
                //没运费模板数据，已删除，默认零元
                result.InRange = true;
                result.Message = "查无运费模板，或许已删除，默认返回零元";
                return result;
            }

            DeliveryFeeSumBLL sumFeeMethod = new DeliveryFeeSumBLL(sumMethod);

            foreach(DeliveryTemplate template in productTemplates)
            {
                List<DeliveryProduct> products = productInfo.FindAll(item => item.TemplateId == template.Id);
                if(template.IsFree == 1)
                {
                    //全国包邮
                    result.Fee = 0;
                    result.InRange = true;
                    result.Message += $"[{template.Name}]模板，设置为全国包邮，[{string.Join(",",products.Select(item => item.Name))}]商品运费为零；";
                    continue;
                }

                DeliveryTemplate region = MatchDeliveryRegion(template, provinces, city);
                if (region == null)
                {
                    //检查超出配送区域
                    result.ErrorId = products.First().Id;
                    result.Message = $"[{products.First().Name}]不在配送区域内";
                    return result;
                }
                if (region.IsFree == 1)
                {
                    //部分区域设置包邮
                    result.Fee = 0;
                    result.InRange = true;
                    result.Message += $"[{string.Join(",", products.Select(item => item.Name))}]配送到[{provinces}][{city}]为包邮地区；";
                    continue;
                }

                //购买单位
                int unit = 0;
                int padUnit = 0;
                string unitName = string.Empty;
                switch(template.UnitType)
                {
                    case (int)DeliveryUnit.件数:
                        unitName = "件";
                        unit = products.Sum(item => item.Count);
                        break;
                    case (int)DeliveryUnit.重量:
                        unitName = "g";
                        //购买重量 = 数量 x 单件重量
                        unit = products.Sum(item => item.Count * item.Weight);
                        //补余，如：购买300g，不足1kg，追加700kg
                        int extraUnit = unit - region.Base;
                        if (extraUnit > 0 && region.Extra > 0 && region.ExtraCost > 0)
                        {
                            padUnit = (int)Math.Ceiling((double)extraUnit / region.Extra) * region.Extra - extraUnit;
                        }
                        break;
                    default:
                        continue;
                }

                //新建运费
                DeliveryFeeSum sum = new DeliveryFeeSum
                {
                    Base = region.Base,
                    BaseCost = region.BaseCost,
                    Extra = region.Extra,
                    ExtraCost = region.ExtraCost,
                    BuyUnit = unit,
                    PadUnit = padUnit,
                    UnitType = template.UnitType
                };

                //叠加运费
                sumFeeMethod.AddTotal(sum);
                result.Message += string.Join(";", products.Select(item =>
                {
                    int buyUnit = template.UnitType == (int)DeliveryUnit.件数 ? item.Count : item.Count * item.Weight;
                    return string.Join("", new string[]
                    {
                        $"[{item.Name}]（购买{buyUnit}{unitName}）:",
                        $"{sum.Base}{unitName} 内 {sum.BaseCost * 0.01}元，",
                        $"运费增加{sum.ExtraCost * 0.01}元；",
                    });
                }));
            }

            int totalFee = 0;
            result.InRange = true;
            try
            {
                totalFee = sumFeeMethod.GetTotal();
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.InRange = false;
            }
            result.Fee = totalFee;
            return result;
        }

        public DeliveryFeeResult GetFreightInfo(PinGoods good, int buyCount, string province, string city)
        {
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(good.aId, good.storeId);
            if(store == null)
            {
                return new DeliveryFeeResult { InRange = true, Message = "店铺不存在" };
            }
            if (!store.setting.freightSwitch)
            {
                return new DeliveryFeeResult { InRange = true, Message = "未开启运费模板功能，默认零元运费" };
            }

            DeliveryFeeSumMethond sumMethod;
            if (!Enum.TryParse(store.setting.freightSumRule.ToString(), out sumMethod))
            {
                return new DeliveryFeeResult { Message = "运费规则设置异常" };
            }

            DeliveryProduct product = new DeliveryProduct
            {
                Count = buyCount,
                Id = good.id,
                Name = good.name,
                TemplateId = good.FreightTemplate,
                Weight = good.GetAttrbute().Weight,
            };

            DeliveryFeeResult result = GetDeliveryFeeCommon(new List<DeliveryProduct> { product }, province, city, sumMethod);
            if (result.Fee > 0)
            {
                result.Message = $"[{sumMethod}]{result.Message}";
            }
            return result;
        }

        /// <summary>
        /// 小未平台独立模板运费
        /// </summary>
        /// <param name="goodcartids"></param>
        /// <param name="aid"></param>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public DeliveryFeeResult GetPlatFee(string goodcartids, int aid, string province, string city, ref string msg)
        {
            
            
            DeliveryFeeResult deliueryResult = new DeliveryFeeResult();
            
            //购物车
            List<PlatChildGoodsCart> cartlist = PlatChildGoodsCartBLL.SingleModel.GetListByIds(goodcartids);
            if (cartlist == null || cartlist.Count <= 0)
            {
                msg = "运费：购物车数据为空";
                return deliueryResult;
            }

            //商品
            string goodsid = string.Join(",", cartlist.Select(s => s.GoodsId));
            List<PlatChildGoods> goodslist = PlatChildGoodsBLL.SingleModel.GetListByIds(goodsid);
            if (goodslist == null || goodslist.Count <= 0)
            {
                msg = "运费：找不到商品数据";
                return deliueryResult;
            }
            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(aid);
            if (store == null)
            {
                msg = "运费：无效店铺";
                return deliueryResult;
            }
            DeliveryFeeSumMethond sumMethod;
            PlatStoreSwitchModel config = string.IsNullOrWhiteSpace(store.SwitchConfig) ? new PlatStoreSwitchModel() : JsonConvert.DeserializeObject<PlatStoreSwitchModel>(store.SwitchConfig);
            
            if (config.enableDeliveryTemplate)
            {
                if (!Enum.TryParse(config.deliveryFeeSumMethond.ToString(), out sumMethod))
                {
                    msg = "运费：无效运费模板";
                    return deliueryResult;
                }

                List<DeliveryProduct> productInfo = new List<DeliveryProduct>();
                foreach (PlatChildGoods gooditem in goodslist)
                {
                    List<PlatChildGoodsCart> tempcartlist = cartlist.Where(w => w.GoodsId == gooditem.Id).ToList();
                    if (tempcartlist == null || tempcartlist.Count <= 0)
                    {
                        msg = "运费：无效购物车数据";
                        return deliueryResult;
                    }
                    productInfo.Add(new DeliveryProduct
                    {
                        TemplateId = gooditem.TemplateId,
                        Name = gooditem.Name,
                        Count = tempcartlist.Sum(s => s.Count),
                    });
                }
                deliueryResult = GetDeliveryFeeCommon(productInfo, province, city, sumMethod);
            }

            if (deliueryResult == null)
            {
                msg = "运费：无效运费模板";
            }
            else if (!deliueryResult.InRange)
            {
                msg = string.IsNullOrEmpty(deliueryResult.Message) ? "不在配送范围内" : deliueryResult.Message;
            }

            return deliueryResult;
        }

        /// <summary>
        /// 企业智推版运费
        /// </summary>
        /// <param name="goodcartids"></param>
        /// <param name="aid"></param>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public DeliveryFeeResult GetQiyeFee(string goodcartids, int aid, string province, string city, ref string msg)
        {
            
            
            DeliveryFeeResult deliueryResult = new DeliveryFeeResult();
            
            //购物车
            List<QiyeGoodsCart> cartList = QiyeGoodsCartBLL.SingleModel.GetListByIds(goodcartids);
            if (cartList == null || cartList.Count <= 0)
            {
                msg = "运费：购物车数据为空";
                return deliueryResult;
            }

            //商品
            string goodsIds = string.Join(",", cartList.Select(s => s.GoodsId));
            List<QiyeGoods> goodsList = QiyeGoodsBLL.SingleModel.GetListByIds(goodsIds);
            if (goodsList == null || goodsList.Count <= 0)
            {
                msg = "运费：找不到商品数据";
                return deliueryResult;
            }
            QiyeStore store = QiyeStoreBLL.SingleModel.GetModelByAId(aid);
            if (store == null)
            {
                msg = "运费：无效店铺";
                return deliueryResult;
            }
            DeliveryFeeSumMethond sumMethod;
            PlatStoreSwitchModel config = string.IsNullOrWhiteSpace(store.SwitchConfig) ? new PlatStoreSwitchModel() : JsonConvert.DeserializeObject<PlatStoreSwitchModel>(store.SwitchConfig);

            if (config.enableDeliveryTemplate)
            {
                if (!Enum.TryParse(config.deliveryFeeSumMethond.ToString(), out sumMethod))
                {
                    msg = "运费：无效运费模板";
                    return deliueryResult;
                }

                List<DeliveryProduct> productInfo = new List<DeliveryProduct>();
                foreach (QiyeGoods goodItem in goodsList)
                {
                    List<QiyeGoodsCart> tempCartList = cartList.Where(w => w.GoodsId == goodItem.Id).ToList();
                    if (tempCartList == null || tempCartList.Count <= 0)
                    {
                        msg = "运费：无效购物车数据";
                        return deliueryResult;
                    }
                    productInfo.Add(new DeliveryProduct
                    {
                        TemplateId = goodItem.TemplateId,
                        Name = goodItem.Name,
                        Count = tempCartList.Sum(s => s.Count),
                    });
                }
                deliueryResult = GetDeliveryFeeCommon(productInfo, province, city, sumMethod);
                if (deliueryResult == null)
                {
                    msg = "运费：无效运费模板";
                }
                else if (!deliueryResult.InRange)
                {
                    msg = deliueryResult.Message;
                }
            }
            return deliueryResult;
        }

        /// <summary>
        /// 获取商品运费（专业版：通过购物车商品）
        /// </summary>
        /// <param name="goodCarts"></param>
        /// <param name="sumMethod"></param>
        /// <returns></returns>
        public DeliveryFeeResult GetDeliveryFeeSum(List<EntGoodsCart> goodCarts, string provinces, string city, DeliveryFeeSumMethond sumMethod)
        {
            DeliveryFeeResult result = new DeliveryFeeResult();
            if (string.IsNullOrWhiteSpace(provinces) || string.IsNullOrWhiteSpace(city))
            {
                result.InRange = false;
                result.Message = "无效配送地址";
                return result;
            }

            //获取商品
            string goodsId = string.Join(",", goodCarts.Select(good => good.FoodGoodsId));
            if (string.IsNullOrWhiteSpace(goodsId))
            {
                result.InRange = false;
                return result;
            }

            List<EntGoods> goods = EntGoodsBLL.SingleModel.GetListByIds(goodsId);
            List<DeliveryProduct> computedProduct = goodCarts.Where(item => goods.FindIndex(good => good.id == item.FoodGoodsId) > -1).Select(item => {
                //商品
                EntGoods good = goods.Find(thisGood => thisGood.id == item.FoodGoodsId);
                return new DeliveryProduct
                {
                    Name = good.name,
                    Count = item.Count,
                    TemplateId = good.TemplateId,
                    Weight = good.Weight,
                    Amount = item.Price,
                    Id = good.id
                };
            }).ToList();
            return GetDeliveryFeeCommon(productInfo: computedProduct, provinces: provinces, city: city, sumMethod: sumMethod);
        }

        public DeliveryFeeResult GetDeliveryFee(BargainUser bargainOrder,string province, string city, DeliveryFeeSumMethond sumMethod, DeliveryConfig config = null)
        {
            if (config == null)
            {
                config = DeliveryConfigBLL.SingleModel.GetConfig(new Bargain { Id = bargainOrder.BId });
            }
            DeliveryProduct formatProduct = new DeliveryProduct
            {
                Count = 1,
                Name = bargainOrder.BName,
                TemplateId = config.Attr.DeliveryTemplateId,
                Weight = config.Attr.Weight,
                Amount = bargainOrder.CurrentPrice,
            };

            DeliveryFeeResult result = GetDeliveryFeeCommon(new List<DeliveryProduct> { formatProduct }, province, city, sumMethod);
            if(result.InRange)
            {
                DeliveryDiscount discount = GetDiscount(bargainOrder.aid, bargainOrder.CurrentPrice);
                result.Fee = discount.HasDiscount ? discount.DiscountPrice : result.Fee;
                result.Message = discount.HasDiscount ? discount.DiscountInfo : result.Message;
            }
            return result;
        }

        /// <summary>
        /// 匹配配送区域运费设置
        /// </summary>
        /// <param name="template"></param>
        /// <param name="provinceName"></param>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public DeliveryTemplate MatchDeliveryRegion(DeliveryTemplate template, string provinceName, string cityName)
        {
            //直辖市特殊处理
            string zhiXiaShiCityName = "";
            HashSet<string> zhiXiaShi = new HashSet<string> {
                "北京市",
                "天津市",
                "上海市",
                "重庆市",
            };

            //匹配区域数据
            C_Area userProvince = C_AreaBLL.SingleModel.GetModel($"ParentCode = 0 AND Name = '{provinceName}'");
            //TODO 有名字相似的城市 用like会导致不准确

            C_Area userCity = C_AreaBLL.SingleModel.GetModel($"ParentCode > 0 AND (Name = '{cityName}' {zhiXiaShiCityName})");

            if (userProvince != null && zhiXiaShi.Contains(provinceName))
            {
                userCity = new C_Area
                {
                    ParentCode = userProvince.Code,
                    Name=userProvince.Name,
                    State=1,
                    Code=userProvince.Code,
                };
            };

            if (userProvince == null || userCity == null)
            {
                //匹配失败：数据库不存在用户输入地区
                //log4net.LogHelper.WriteInfo(this.GetType(), $"无法匹配用户地址：省（{provinceName}{userProvince?.Code}）,市（{cityName}{userCity?.ParentCode}）");
                return null;
            }

            //匹配配送区域
            List<DeliveryTemplate> deliveryRegion = GetRegionTemplate(template.Id);
            foreach (var region in deliveryRegion)
            {
                //在配送范围内
                if (CheckDeliveryArea(region, userProvince.Name, userCity.Name))
                {
                    //log4net.LogHelper.WriteInfo(this.GetType(), $"276:找到匹配区域({Newtonsoft.Json.JsonConvert.SerializeObject(region)})");
                    //返回配送区域运费设置
                    return region;
                }
            }
            //旧运费模板数据兼容（返回母模板运费设置）
            if (deliveryRegion == null || deliveryRegion.Count == 0)
            {
                if (template.IsFree == 1)
                {
                    //全国包邮
                    return template;
                }
                if (string.IsNullOrWhiteSpace(template.DeliveryRegion))
                {
                    //全国配送
                    return template;
                }
                if (CheckDeliveryRegion(template, userProvince.Code, userCity.Code))
                {
                    //在配送范围内
                    return template;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查配送范围
        /// </summary>
        /// <param name="region"></param>
        /// <param name="provinceCode"></param>
        /// <param name="cityCode"></param>
        /// <returns></returns>
        public bool CheckDeliveryRegion(DeliveryTemplate region, int provinceCode, int cityCode)
        {
            //获取全部区域码，并删除重复
            List<int> allAreaCode = region.DeliveryRegion.ConvertToIntList(',');
            //省份区域码
            IEnumerable<int> deliveryProvince = allAreaCode.Select(code => GetParentCode(code)).GroupBy(code => code).Select(code => code.First());
            //市级区域码
            IEnumerable<int> deliveryCity = allAreaCode.Where(code => !deliveryProvince.Contains(code));

            if (!deliveryProvince.Contains(provinceCode))
            {
                //log4net.LogHelper.WriteInfo(this.GetType(), $"314:{string.Join(",", deliveryProvince)}");
                //省份不在配送范围内
                return false;
            }
            if (region.ParentId > 0)
            {
                //新版配送区域
                List<string> rejectRegion = region.DeliveryRegionSub?.SplitStr(",") ?? new List<string>();
                if (rejectRegion.IndexOf(cityCode.ToString()) > -1)
                {
                    //log4net.LogHelper.WriteInfo(this.GetType(), $"346:{string.Join(",", deliveryCity)}");
                    //市区在拒绝配送范围
                    return false;
                }
            }
            else if (!deliveryCity.Contains(cityCode))
            {
                //log4net.LogHelper.WriteInfo(this.GetType(), $"353:{string.Join(",", deliveryCity)}");
                //log4net.LogHelper.WriteInfo(this.GetType(), $"354:{cityCode}");
                //市区在配送范围内
                return false;
            }

            return true;
        }

        public bool CheckDeliveryArea(DeliveryTemplate template, string province, string city)
        {
            //为空，默认全国配送
            if (string.IsNullOrWhiteSpace(template.DeliveryRegion))
            {
                return true;
            }
            //为空，地址错误
            if (string.IsNullOrWhiteSpace(province) || string.IsNullOrWhiteSpace(city))
            {
                return false;
            }

            //获取并筛选，删除重复areaCode
            IEnumerable<int> areaCodes = template.DeliveryRegion.SplitStr(",").Select(code => int.Parse(code)).GroupBy(code => code).Select(code => code.First());
            //获取并筛选，删除重复parentCode
            Func<int, int> getParentCode = (areaCode) =>
            {
                return (int)Math.Floor((double)(areaCode / 10000)) * 10000;
            };
            IEnumerable<int> parentCode = areaCodes.Select(code => getParentCode(code)).GroupBy(code => code).Select(code => code.First());
            areaCodes = areaCodes.Where(code => !parentCode.Contains(code));

            //数据库匹配地址
            string provinceCodeStr = string.Join(",", parentCode).Trim(',');
            string cityCodeStr = string.Join(",", areaCodes).Trim(',');

            //定位省份
            bool locateProvice = !string.IsNullOrWhiteSpace(provinceCodeStr);
            if (locateProvice)
            {
                //log4net.LogHelper.WriteInfo(this.GetType(), $"393：{provinceCodeStr}");
                locateProvice = C_AreaBLL.SingleModel.GetCount($"Code in ({provinceCodeStr}) AND Name LIKE '%{province}%'") > 0;
                //log4net.LogHelper.WriteInfo(this.GetType(), $"393：Code in ({ provinceCodeStr}) AND Name LIKE '%{province}%'");
            }
            //定位城市
            bool locateCity = !string.IsNullOrWhiteSpace(cityCodeStr) || (locateProvice && string.IsNullOrWhiteSpace(cityCodeStr));
            if (locateCity && !string.IsNullOrWhiteSpace(cityCodeStr))
            {
                locateCity = C_AreaBLL.SingleModel.GetCount($"Code in ({cityCodeStr}) AND ParentCode in({provinceCodeStr},0) AND Name LIKE '%{city}%'") > 0;
                //log4net.LogHelper.WriteInfo(this.GetType(), $"400：{cityCodeStr},{$"Code in ({cityCodeStr}) AND ParentCode in({provinceCodeStr},0) AND Name LIKE '%{city}%'"}");
            }

            return locateCity && locateProvice;
        }

        /// <summary>
        /// 获取省份区域码算法
        /// </summary>
        /// <param name="areaCode"></param>
        /// <returns></returns>
        public int GetParentCode(int areaCode)
        {
            return (int)Math.Floor((double)(areaCode / 10000)) * 10000;
        }

        public bool HasRepeatRegion(DeliveryTemplate currRegion, DeliveryTemplate newRegion)
        {
            if (currRegion.Id == newRegion.Id)
            {
                return true;
            }
            //当前区域码
            List<int> currCodes = currRegion.DeliveryRegion.ConvertToIntList(',');
            //新增区域码
            List<int> newCodes = newRegion.DeliveryRegion.ConvertToIntList(',');
            //寻找重复区域码
            return newCodes.Exists(code => currCodes.Contains(code));
        }

        public DeliveryTemplate ConvertUnitType(DeliveryTemplate template, DeliveryUnit unitType)
        {
            //更新配送单位
            double newUnit = 0;
            switch (unitType)
            {
                case DeliveryUnit.件数:
                    newUnit = 0.001;
                    break;
                case DeliveryUnit.重量:
                    newUnit = 1000;
                    break;
                default:
                    break;
            }
            template.Base = (int)Math.Floor(template.Base * newUnit);
            template.Extra = (int)Math.Floor(template.Extra * newUnit);
            return template;
        }
    }

    public class DeliveryFeeSumBLL
    {
        public DeliveryFeeSumBLL(DeliveryFeeSumMethond SumFeeMethod)
        {
            this._SumFeeMethod = SumFeeMethod;
        }

        private List<DeliveryFeeSum> _totalSum { get; set; } = new List<DeliveryFeeSum>();

        private DeliveryFeeSumMethond _SumFeeMethod { get; set; }

        /// <summary>
        /// 累计运费模型
        /// </summary>
        /// <param name="totalSum">现有计费</param>
        /// <param name="feeModel">新增计费</param>
        /// <param name="sumMethod">计算规则</param>
        /// <returns></returns>
        public bool AddTotal(DeliveryFeeSum feeModel)
        {
            //寻找的相同运费模板设置
            int infoIndex = _totalSum.FindIndex(info => info.Base == feeModel.Base &&
                                                        info.BaseCost == feeModel.BaseCost &&
                                                        info.Extra == feeModel.Extra &&
                                                        info.ExtraCost == feeModel.ExtraCost &&
                                                        info.UnitType == feeModel.UnitType);
            bool isHasSame = infoIndex > -1;
            if (isHasSame)
            {
                //合并购买单位，不重复计算
                _totalSum[infoIndex].BuyUnit += feeModel.BuyUnit;
                return true;
            }

            //有赞运费计费规则：累计不同模板运费
            if (_SumFeeMethod == DeliveryFeeSumMethond.有赞)
            {
                //累计运费
                _totalSum.Add(feeModel);
                return true;
            }

            //淘宝运费计费规则：只算一个最高的首运费
            if (_SumFeeMethod == DeliveryFeeSumMethond.淘宝)
            {
                //log4net.LogHelper.WriteInfo(this.GetType(), $"714{JsonConvert.SerializeObject(feeModel)}");
                //log4net.LogHelper.WriteInfo(this.GetType(), $"715{JsonConvert.SerializeObject(_totalSum)}");
                //累计运费
                if (_totalSum.Count == 0)
                {
                    _totalSum.Add(feeModel);
                    return true;
                }

                //获取最高的首运费
                DeliveryFeeSum highestBaseCost = _totalSum.OrderByDescending(total => total.BaseCost).First();
                //log4net.LogHelper.WriteInfo(this.GetType(), $"720：{JsonConvert.SerializeObject(highestBaseCost)}");
                if (highestBaseCost.BaseCost >= feeModel.BaseCost)
                {
                    //低于最高首运费，每件以续费价格计算
                    feeModel.BaseCost = feeModel.ExtraCost;
                    feeModel.Base = 0;
                }
                else
                {
                    //高于最高首运费，覆盖当前最高首运费
                    _totalSum.ForEach(total => total.BaseCost = total.ExtraCost);
                }
                _totalSum.Add(feeModel);
            }

            return false;
        }

        /// <summary>
        /// 累计运费
        /// </summary>
        /// <param name="sumModel">运费模型</param>
        /// <returns></returns>
        public int GetTotal()
        {
            int totalFee = 0;
            foreach (var model in _totalSum)
            {
                double extraUnit = model.Extra > 0 ? Math.Max(model.BuyUnit - model.Base + model.PadUnit, 0) / model.Extra : 0;
                totalFee += model.BaseCost + (int)Math.Ceiling(extraUnit * model.ExtraCost);
            }
            return totalFee;
        }
    }
}
