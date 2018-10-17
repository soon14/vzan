using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Core.MiniApp.DTO
{
    public class LoginResult
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public string authToken { get; set; }
    }

    /// <summary>
    /// 编辑店铺DTO
    /// </summary>
    public class EditStore
    {
        /// <summary>
        /// 头像/LOGO
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "店铺LOGO不能为空")]
        [StringLength(maximumLength: 300, MinimumLength = 1, ErrorMessage = "店铺LOGO不能为空")]
        public string Logo { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "店铺名称不能为空")]
        [StringLength(maximumLength: 300, MinimumLength = 1, ErrorMessage = "店铺名称不能为空")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 登陆名
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "店铺登陆名不能为空")]
        [StringLength(maximumLength: 12, MinimumLength = 6, ErrorMessage = "店铺登陆名必须大于6个字符，并小于12个字符")]
        public string Login { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 启用时间（店铺启用 大于 该日期）
        /// </summary>
        [DataType(DataType.Date)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "启用时间不能为空")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime? Begin { get; set; } = null;

        /// <summary>
        /// 过期时间（店铺过期 小于 该日期）
        /// </summary>
        [DataType(DataType.Date)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "过期时间不能为空")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime? Expire { get; set; } = null;
    }

    public class EditProduct
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "商品名称不能为空")]
        [StringLength(maximumLength: 300, MinimumLength = 1, ErrorMessage = "商品名称不能为空")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 图片
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "商品图片不能为空")]
        [StringLength(maximumLength: 300, MinimumLength = 1, ErrorMessage = "商品图片不能为空")]
        public string Img { get; set; } = string.Empty;
        /// <summary>
        /// 人气值
        /// </summary>
        public int Hit { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "必须选择商品分类")]
        public int CategoryId { get; set; }
        /// <summary>
        /// 打印标签
        /// </summary>
        public int PrintTagId { get; set; }
        /// <summary>
        /// 是否外卖商品
        /// </summary>
        [Required(ErrorMessage = "外卖设置不能为空")]
        public bool? IsTakeOut { get; set; }
        /// <summary>
        /// 售价
        /// </summary>
        [Range(minimum: 0.01d, maximum: double.MaxValue, ErrorMessage = "售价必须大于零")]
        public double Price { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        [Range(minimum: 0.01d, maximum: double.MaxValue, ErrorMessage = "原价必须大于零")]
        public double OriginalPrice { get; set; }
        /// <summary>
        /// 打包费用
        /// </summary>
        [Range(minimum: 0d, maximum: double.MaxValue, ErrorMessage = "打包费用必须大于，或等于零")]
        public double PackingFee { get; set; }
        /// <summary>
        /// 每日库存
        /// </summary>
        public int DailySupply { get; set; }
        /// <summary>
        /// 月销量
        /// </summary>
        public int Sale { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 是否上架
        /// </summary>
        [Required(ErrorMessage = "上架状态不能为空")]
        public bool? Display { get; set; }
        /// <summary>
        /// 排序，越大越靠前
        /// </summary>
        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "排序字段必须大于或等于零，最大为9位数")]
        public int Sort { get; set; }
        /// <summary>
        /// 商品属性
        /// </summary>
        public List<EditProductAttr> Attrs { get; set; }
    }

    public class EditProductAttr
    {
        public int Id { get; set; }
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "属性不能为空")]
        public int AttrId { get; set; }
        public int AttrTypeId { get; set; }
        public string Option { get; set; }
        [Range(minimum: 0d, maximum: double.MaxValue, ErrorMessage = "价格必须大于或等于零")]
        public double Price { get; set; }
    }

    public class EditAttbute
    {
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "属性分类不能为空")]
        public int AttrTypeId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "店铺名称不能为空")]
        [StringLength(maximumLength: 150, MinimumLength = 1, ErrorMessage = "店铺名称不能为空")]
        public string Name { get; set; }
        [MinLength(length:1,ErrorMessage ="至少添加一个选项"), Required(ErrorMessage = "至少添加一个选项")]
        public string[] Option { get; set; }
        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "排序字段必须大于或等于零，最大为9位数")]
        public int Sort { get; set; }
    }

    public class EditAttrType
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "属性分类名称不能为空")]
        [StringLength(maximumLength: 150, MinimumLength = 1, ErrorMessage = "属性分类名称不能为空")]
        public string Name { get; set; }
        [Required(ErrorMessage = "启用设置不能为空")]
        public bool? Enable { get; set; }
    }

    public class EditCategory
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "分类名称不能为空")]
        [StringLength(maximumLength: 150, MinimumLength = 1, ErrorMessage = "分类名称不能为空")]
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "排序字段必须大于或等于零，最大为9位数")]
        public int Sort { get; set; }
        [Required(ErrorMessage = "显示设置不能为空")]
        public bool Display { get; set; }
    }

    public class EditBaseConfig: DishBaseConfig
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "店铺名称不能为空")]
        [StringLength(maximumLength: 150, MinimumLength = 1, ErrorMessage = "店铺名称不能为空")]
        public string storename { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "店铺LOGO不能为空")]
        [StringLength(maximumLength: 150, MinimumLength = 1, ErrorMessage = "店铺LOGO不能为空")]
        public string logo { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
    }
}