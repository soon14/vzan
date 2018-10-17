using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utility;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Entity.MiniApp.Pin
{
    #region 产品
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinGoods
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aId { get; set; } = 0;



        [SqlField]
        public int storeId { get; set; } = 0;
        public string storeName { get; set; } = string.Empty;
        /// <summary>
        /// 状态 1=上架，0=下架，-1=删除 枚举 PinEnums.GoodsState
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 审核状态，默认待审核 枚举：PinEnums.GoodsAuditState
        /// </summary>
        [SqlField]
        public int auditState { get; set; } = 1;

        public string auditStateStr {
            get {
              return  Enum.GetName(typeof(PinEnums.GoodsAuditState), auditState);
            }
        }
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 99;

        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "产品名称不能为空")]
        [MinLength(length: 1, ErrorMessage = "产品名称最少1个字")]
        [MaxLength(length: 50, ErrorMessage = "产品名称最多50个字")]
        public string name { get; set; } = string.Empty;


        [SqlField]
        [Range(1, int.MaxValue, ErrorMessage = "请选择一级分类")]
        public int cateIdOne { get; set; } = 0;
        public string cateNameOne { get; set; } = string.Empty;
        /// <summary>
        /// 分类ID
        /// </summary>
        [SqlField]
        [Range(1, int.MaxValue, ErrorMessage = "请选择二级分类")]
        public int cateId { get; set; } = 0;

        public string cateName { get; set; } = string.Empty;
        /// <summary>
        /// 属性类型ID 
        /// </summary>
        [SqlField]
        public int attrTypeId { get; set; } = 0;
        /// <summary>
        /// 标签，可多选，逗号分隔
        /// </summary>
        [SqlField]
        public string labels { get; set; } = string.Empty;

        public List<string> labelList
        {
            get
            {
                return labels.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
        /// <summary>
        /// 产品主图
        /// </summary>
        [SqlField]
        public string img { get; set; } = string.Empty;

        /// <summary>
        /// 多图，逗号分隔
        /// </summary>
        [SqlField]
        public string imgAlbum { get; set; } = string.Empty;

        public List<string> imgAlbumList
        {
            get
            {
                if (string.IsNullOrEmpty(imgAlbum))
                    imgAlbum = "";
                return imgAlbum.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
        /// <summary>
        /// 单位
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "产品单位不能为空")]
        [MinLength(length: 1, ErrorMessage = "单位最少1个字")]
        [MaxLength(length: 3, ErrorMessage = "单位最多3个字")]
        public string unit { get; set; } = string.Empty;

        /// <summary>
        /// 原价，单位分
        /// </summary>
        [SqlField]
        public int originalPrice { get; set; } = 0;

        /// <summary>
        /// 价格，单位分
        /// </summary>
        [SqlField]
        public int price { get; set; } = 0;
        public string priceStr
        {
            get
            {
                return (price * 0.01).ToString();
            }
        }
        /// <summary>
        /// 库存
        /// </summary>
        [SqlField]
        public int stock { get; set; } = 0;

        /// <summary>
        /// 是否限制库存
        /// </summary>
        [SqlField]
        public bool stockLimit { get; set; } = false;

        /// <summary>
        /// 虚拟销量
        /// </summary>
        [SqlField]
        public int virtualSales { get; set; } = 0;

        /// <summary>
        /// 销量
        /// </summary>
        [SqlField]
        public int sales { get; set; } = 0;

        [SqlField]
        [AllowHtml]
        [Required(AllowEmptyStrings = false, ErrorMessage = "产品描述不能为空")]
        public string description { get; set; } = string.Empty;

        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;

        [SqlField]
        public DateTime updateTime { get; set; } = DateTime.Now;

        #region 
        /// <summary>
        /// 购买模式 0=普通商品，1=拼团
        /// </summary>
        [SqlField]
        public int buyMode { get; set; } = 1;

        /// <summary>
        /// 拼团返现金额
        /// </summary>
        [SqlField]
        public int groupPrice { get; set; } = 0;

        /// <summary>
        /// 团长减价
        /// </summary>
        [SqlField]
        public int groupHeadReduce { get; set; } = 0;

        /// <summary>
        /// 成团人数限制
        /// </summary>
        [SqlField]
        public int groupUserLimit { get; set; } = 2;

        /// <summary>
        /// 每用户限购 0=不限制
        /// </summary>
        [SqlField]
        public int groupUserBuyLimit { get; set; } = 0;

        /// <summary>
        /// 有效时长，单位小时
        /// </summary>
        [SqlField]
        public int groupValidTime { get; set; }
        #endregion


        /// <summary>
        /// 产品规格，详情 保存规格+价格+库存
        /// </summary>
        [SqlField]
        public string specificationdetail { get; set; } = string.Empty;

        /// <summary>
        /// 规格详情列表
        /// </summary>
        public List<SpecificationDetailModel> SpecificationDetailList
        {
            get
            {
                if (!string.IsNullOrEmpty(specificationdetail))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<List<SpecificationDetailModel>>(specificationdetail);
                    }
                    catch (Exception)
                    {
                        return new List<SpecificationDetailModel>();
                    }
                } 
                else
                    return new List<SpecificationDetailModel>();
            }
        }
        /// <summary>
        /// 选择的规格对象，用于编辑的时候还原
        /// </summary>
        [SqlField]
        public string pickspecification { get; set; } = string.Empty;

        /// <summary>
        /// 已拼数量
        /// </summary>
        public int pinGoodsCount { get; set; } = 0;

        /// <summary>
        /// 首页显示排序 越大越靠前
        /// </summary>
        [SqlField]
        public int IndexRank { get; set; } = 99;
        

        /// <summary>
        /// 运费模板ID
        /// </summary>
        [SqlField]
        public int FreightTemplate { get; set; } = 0;

        /// <summary>
        /// 评价数量
        /// </summary>
        [SqlField]
        public int CommentCount { get; set; } = 0;

        /// <summary>
        /// 商品属性
        /// </summary>
        [SqlField]
        public string Attrbute { get; set; }

        public PinGoodAttrbute AttrbuteModel => GetAttrbute();

        public PinGoodAttrbute GetAttrbute()
        {
            return string.IsNullOrWhiteSpace(Attrbute) ? new PinGoodAttrbute() : JsonConvert.DeserializeObject<PinGoodAttrbute>(Attrbute);
        }

        public void SetAttrbute(string key, object value)
        {
            Type configType = typeof(PinGoodAttrbute);
            object newValue = Convert.ChangeType(value, configType.GetProperty(key).PropertyType);
            PinGoodAttrbute updateAttrbute = GetAttrbute();
            configType.GetProperty(key).SetValue(updateAttrbute, newValue);
            Attrbute = JsonConvert.SerializeObject(updateAttrbute);
        }
    }
    #endregion
}

public class PinGoodAttrbute
{
    public int Weight { get; set; }
}


/// <summary>
/// 单个规格详情类
/// </summary>
public class SpecificationDetailModel
{
    /// <summary>
    /// 规格ID，单个，多个组合 如 1_2
    /// </summary>
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// 规格名称，单个，多个组合 如  颜色:红色,尺码:X
    /// </summary>
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// 产品价格，分
    /// </summary>
    public int price { get; set; } = 0;
    public string priceStr
    {
        get
        {
            return (price * 0.01).ToString("0.00");
        }
    }
    /// <summary>
    /// 返现金额，分
    /// </summary>
    public int groupPrice { get; set; } = 0;

    /// <summary>
    /// 库存
    /// </summary>
    public int stock { get; set; } = 0;

    /// <summary>
    /// 产品图片
    /// </summary>
    public string imgUrl { get; set; } = string.Empty;

}