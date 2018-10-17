using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Entity.Base;
using Utility;


namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 菜品
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishGood
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 人气值
        /// </summary>
        [SqlField]
        public int g_renqi { get; set; } = 1;

        /// <summary>
        /// 菜品名称
        /// </summary>
        [SqlField]
        public string g_name { get; set; } = string.Empty;

        /// <summary>
        /// 分类ID
        /// </summary>
        [SqlField]
        public int cate_id { get; set; } = 0;

        public string cat_name { get; set; } = string.Empty;

        /// <summary>
        /// 打印标签ID
        /// </summary>
        [SqlField]
        public int g_print_tag { get; set; } = 0;

        /// <summary>
        /// 菜品图片
        /// </summary>
        [SqlField]
        public string img { get; set; } = string.Empty;

        /// <summary>
        /// 是否外卖
        /// </summary>
        [SqlField]
        public int is_waimai { get; set; } = 1;

        /// <summary>
        /// 价格
        /// </summary>
        [SqlField]
        public double shop_price { get; set; } = 0.00d;

        /// <summary>
        /// 原价
        /// </summary>
        [SqlField]
        public double market_price { get; set; } = 0.00d;

        /// <summary>
        /// 打包费
        /// </summary>
        [SqlField]
        public double dabao_price { get; set; } = 0.00d;

        /// <summary>
        /// 每日库存 -1为不限制
        /// </summary>
        [SqlField]
        public int day_kucun { get; set; } = -1;

        /// <summary>
        /// 描述
        /// </summary>
        [SqlField]
        [AllowHtml]
        public string g_description { get; set; } = string.Empty;

        /// <summary>
        /// 单位
        /// </summary>
        [SqlField]
        public string g_danwei { get; set; } = string.Empty;

        /// <summary>
        /// 状态
        /// 1=上架,0=下架,-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 排序，越大越靠前
        /// </summary>
        [SqlField]
        public int is_order { get; set; } = 99;

        /// <summary>
        /// 类型ID
        /// </summary>
        [SqlField]
        public int goods_type { get; set; } = 0;

        /// <summary>
        /// 赞数
        /// </summary>
        [SqlField]
        public int goods_like_num { get; set; } = 0;

        [SqlField]
        public DateTime add_time { get; set; } = DateTime.Now;
        public string add_time_str
        {
            get
            {
                return add_time.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        [SqlField]
        public DateTime update_time { get; set; }

        /// <summary>
        /// 总销量
        /// </summary>
        [SqlField]
        public int sale_all_num { get; set; } = 0;

        /// <summary>
        /// 月销量
        /// </summary>
        [SqlField]
        public int yue_xiaoliang { get; set; } = 0;

        /// <summary>
        /// 日销量
        /// </summary>
        [SqlField]
        public int sale_day_num { get; set; } = 0;

        /// <summary>
        /// 菜品属性
        /// </summary>
        public List<DishGoodAttr> attr { get; set; } = new List<DishGoodAttr>();

        public List<good_attr> goods_specification { get; set; } = new List<good_attr>();
        public bool sel { get; set; } = false;
    }

    /// <summary>
    /// 比较产品属性
    /// </summary>
    public class DishGoodAttrComparer : IEqualityComparer<DishGoodAttr>
    {
        public bool Equals(DishGoodAttr x, DishGoodAttr y)
        {
            return x.value == y.value && x.attr_id == y.attr_id ;
        }
        public int GetHashCode(DishGoodAttr obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;
            if (string.IsNullOrEmpty(obj.value))
            {
                obj.value = "";
            }
            int hashValue = obj.value.GetHashCode();
            int hashAttrId = obj.attr_id.GetHashCode();
            //int hashGoodsId = obj.goods_id.GetHashCode();
            return hashValue^ hashAttrId;
        }
    }
    /// <summary>
    /// 比较产品属性
    /// </summary>
    public class DishGoodAttrComparer2 : IEqualityComparer<DishGoodAttr>
    {
        public bool Equals(DishGoodAttr x, DishGoodAttr y)
        {
            return x.id == y.id;
        }
        public int GetHashCode(DishGoodAttr obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;
            if (string.IsNullOrEmpty(obj.value))
            {
                obj.value = "";
            }
            int hashValue = obj.id.GetHashCode();// obj.value.GetHashCode();
            //int hashAttrId = obj.attr_id.GetHashCode();
            //int hashGoodsId = obj.goods_id.GetHashCode();
            return hashValue;//^ hashAttrId;
        }
    }

    /// <summary>
    /// 产品属性
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishGoodAttr
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        [SqlField]
        public int goods_id { get; set; } = 0;

        /// <summary>
        /// 属性类型ID
        /// </summary>
        [SqlField]
        public int attr_type_id { get; set; } = 0;

        /// <summary>
        /// 属性ID
        /// </summary>
        [SqlField]
        public int attr_id { get; set; } = 0;

        /// <summary>
        /// 价格
        /// </summary>
        [SqlField]
        public double price { get; set; } = 0.00d;

        /// <summary>
        /// 属性值
        /// </summary>
        [SqlField]
        public string value { get; set; } = string.Empty;

        public string attr_name { get; set; } = string.Empty;
    }


    public class good_attr
    {
        public int attr_type { get; set; } = -1;
        public string name { get; set; } = string.Empty;
        public List<good_attr_value> values { get; set; } = new List<good_attr_value>();
    }
    public class good_attr_value
    {
        public double format_price { get; set; } = .00d;
        public int id { get; set; } = 0;
        public bool ischeck { get; set; } = false;
        public string label { get; set; } = string.Empty;
        public double price { get; set; } = .00d;
    }
}
