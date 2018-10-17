using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace User.MiniApp.Model
{
    public class DashBoardViewModel
    {
        /// <summary>
        /// 小程序访问数据
        /// </summary>
        public XCXDataModel<FWResultModel> FWData { get; set; }
        /// <summary>
        /// 小程序概况数据
        /// </summary>
        public XCXDataModel<GKResultModel> GKData { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public int StoreId { get; set; }
        /// <summary>
        /// 小程序appId
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 权限表
        /// </summary>
        public int RId { get; set; }
        /// <summary>
        /// 小程序二维码
        /// </summary>
        public string XcxCodeImgUrl { get; set; }
        public bool IsOk { get; set; }
        public string Msg { get; set; }
        /// <summary>
        /// 多门店模板门店数据
        /// </summary>
        public List<FootBath> Footbathlist { get; set; }
    }

    public class MiniAppStoreData
    {
        /// <summary>
        /// 销售金额(不包含充值)
        /// </summary>
        public int storeincome { get; set; }
        /// <summary>
        /// 会员充值金额
        /// </summary>
        public int vippaysum { get; set; }
        /// <summary>
        /// 新增会员数量
        /// </summary>
        public int usersum { get; set; }
        /// <summary>
        /// 累计会员数量
        /// </summary>
        public int vipsum { get; set; }
        /// <summary>
        /// 商品销售排行
        /// </summary>
        public List<MiniAppStoreGoods> salegoodsorderby { get; set; }
        /// <summary>
        /// 商品分类销售情况
        /// </summary>
        public List<MiniAppStoreGoods> salegoods { get; set; }
        /// <summary>
        /// 商品分类销售情况
        /// </summary>
        public int salesum { get; set; }
    }
}