
/// <summary>
/// Member转移过来的
/// </summary>
namespace Entity.MiniApp
{
    /// <summary>
    /// 用户的软件信息
    /// </summary>
    public class MemberProductInfo
    {
        /// <summary>
        /// 商品的基本信息
        /// </summary>
        public Product _Product { get; set; }

        /// <summary>
        /// 商品的时间信息
        /// </summary>
        public MemberProduct _MemberProduct { get; set; }


        /// <summary>
        /// 购买记录
        /// </summary>
        //public OrderProduct _OrderProduct { get; set; }
    }
}
