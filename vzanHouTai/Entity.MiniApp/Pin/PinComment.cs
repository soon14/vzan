using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Entity.MiniApp.Pin
{
    /// <summary>
    /// 评论
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinComment
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        [SqlField]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "小程序ID输入错误")]
        public int AId { get; set; } = 0;

        [SqlField]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "店铺ID输入错误")]
        public int StoreId { get; set; } = 0;

        /// <summary>
        /// 订单ID
        /// </summary>
        [SqlField]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "订单ID输入错误")]
        public int OrderId { get; set; } = 0;

        /// <summary>
        /// 产品ID，拼享惠没有购物车，可以直接保存产品ID便于查询
        /// </summary>
        [SqlField]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "产品ID输入错误")]
        public int GoodsId { get; set; } = 0;

        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "用户ID输入错误")]
        public int UserId { get; set; } = 0;

        /// <summary>
        /// 评价等级 1=差评，2=中评，3=好评
        /// </summary>
        [SqlField]
        [RegularExpression(@"^[1-3]$", ErrorMessage = "评价等级输入错误")]
        public int RateLevel { get; set; } = 3;

        public string RateLevelStr
        {
            get
            {
                switch (RateLevel)
                {
                    case 1:
                        return "差评";
                    case 2:
                        return "中评";
                    case 3:
                        return "好评";
                    default:
                        return "";
                }
            }
        }
        /// <summary>
        /// 描述评分 1=很差，2=差，3=一般，4=好，5=非常好
        /// </summary>
        [SqlField]
        [RegularExpression(@"^[1-5]$", ErrorMessage = "描述评分输入错误")]
        public int DescriptionScore { get; set; } = 5;

        /// <summary>
        /// 物流评分
        /// </summary>
        [SqlField]
        [RegularExpression(@"^[1-5]$", ErrorMessage = "物流评分输入错误")]
        public int LogisticsScore { get; set; } = 5;

        /// <summary>
        /// 服务态度评分
        /// </summary>
        [SqlField]
        [RegularExpression(@"^[1-5]$", ErrorMessage = "服务态度评分输入错误")]
        public int ServicesScore { get; set; } = 5;


        /// <summary>
        /// 赞数量
        /// </summary>
        [SqlField]
        public int PraiseCount { get; set; } = 0;

        /// <summary>
        /// 评价内容
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "评价内容不能为空")]
        [StringLength(maximumLength: 300, MinimumLength = 1, ErrorMessage = "评价内容最少1个字，最多300个字")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 是否匿名
        /// </summary>
        [SqlField]
        public bool IsAnonymous { get; set; } = false;

        /// <summary>
        /// 状态 1=显示，-1=删除
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;

        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;

        public string AddTimeStr
        {
            get
            {
                return AddTime.ToString("yyyy.MM.dd");
            }
        }


        /// <summary>
        /// 图片，多图用逗号分割
        /// </summary>
        [SqlField]
        public string Imgs { get; set; } = string.Empty;

        /// <summary>
        /// 用于显示
        /// </summary>
        public List<string> ImgsList
        {
            get
            {
                return string.IsNullOrEmpty(Imgs) ? new List<string>() : Imgs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = string.Empty;

        /// <summary>
        /// 头像
        /// </summary>
        public string UserPhoto { get; set; } = string.Empty;

        /// <summary>
        /// 产品规格，用于显示
        /// </summary>
        public string AttrName { get; set; } = string.Empty;


        /// <summary>
        /// 是否已经评论
        /// </summary>
        public bool IsComment { get; set; } = false;

        /// <summary>
        /// 评价的产品
        /// </summary>
        public PinGoods Goods { get; set; }

        public string GoodName { get; set; } = string.Empty;
        public string GoodImgUrl { get; set; } = string.Empty;
    }
}
