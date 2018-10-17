// const HOST = "http://testwtapi.vzan.com/";   //测试
// const HOST ="http://lztapi.vzan.com";
const HOST = "https://wtApi.vzan.com/";//正式
function Address() {

}
Address.GetAgentConfigInfo = HOST + "apiMiappEnterprise/GetAgentConfigInfo";
Address.WxLogin = HOST + "apiMiniAppMultiStore/WxLogin";
//登陆微赞后台
Address.loginByThirdPlatform = HOST + "apiMiniAppMultiStore/CheckUserLoginNoappsr";
//获取aid
Address.Getaid = HOST + "apiMiappEnterprise/Getaid";
//产品分类
Address.GetGoodTypes = HOST + "apiMiniAppMultiStore/GetGoodTypes";
//获取产品列表
Address.GetGoodsList = HOST + "apiMiniAppMultiStore/GetGoodsList";
//
Address.GetGoodInfo = HOST + "apiMiniAppMultiStore/GetGoodInfo";
//根据选择地址的经纬度,获取同城配送范围内最近一家的门店
Address.GetStores = HOST + "apiMiniAppMultiStore/GetStores";
//门店
Address.GetStoreById = HOST + "apiMiniAppMultiStore/GetStoreById";
//显示附近地址
Address.GetNearMyAddress = HOST + "apiMiniAppMultiStore/GetNearMyAddress"
//快递配送
Address.GetStoreExpresState = HOST + "apiMiniAppMultiStore/GetStoreExpresState";
//添加编辑地址
Address.AddOrEditMyAddress = HOST + "apiMiniAppMultiStore/AddOrEditMyAddress";
// 获取我的收货地址
Address.GetMyAddress = HOST + "apiMiniAppMultiStore/GetMyAddress";
// 设定默认收货地址
Address.SetMyAddressDefault = HOST + "apiMiniAppMultiStore/SetMyAddressDefault";
// 删除我的收货地址
Address.DeleteMyAddress = HOST + "apiMiniAppMultiStore/DeleteMyAddress";
//默认地址
Address.getTXMapAddress = HOST + "apiMiappFoods/getTXMapAddress";
// 获取首页产品组件
Address.GetGoodsByids = HOST + "apiMiappEnterprise/GetGoodsByids";
//用户拒绝授权时获取当前定位
Address.GetLocation = HOST + "apiMiniAppMultiStore/GetLocation";
//会员信息
Address.GetVipInfo = HOST + "apiMiniAppMultiStore/GetVipInfo";
//获取购物车信息
Address.GetGoodsCarData = HOST + "apiMiniAppMultiStore/GetGoodsCarData";
//添加商品至购物车
Address.AddGoodsCarData = HOST + "apiMiniAppMultiStore/AddGoodsCarData";
//从购物车 删除商品/更新数量
Address.UpdateOrDeleteGoodsCarData = HOST + "apiMiniAppMultiStore/UpdateOrDeleteGoodsCarData";
//获取指定的购物车记录
Address.GetGoodsCarDataByIds = HOST + "apiMiniAppMultiStore/GetGoodsCarDataByIds";
//判定购物车商品在当前门店是否有库存
Address.SearchGoodCarStockForStore = HOST + "apiMiniAppMultiStore/SearchGoodCarStockForStore";
// 生成订单
Address.AddMiniappGoodsOrder = HOST + "apiMiniAppMultiStore/AddMiniappGoodsOrder";
// 微信支付订单
Address.PayOrder = HOST + "Inherit/PayOrderNew";
// 获取订单列表
Address.GetOrderList = HOST + "apiMiniAppMultiStore/GetOrderList";
//单个产品获取cartid
Address.GetGoodStockState = HOST + "apiMiniAppMultiStore/GetGoodStockState";
// 提交formid
Address.commitFormId = HOST + "apiMiappStores/commitFormId";
// 获取订单明细
Address.GetOrderDetial = HOST + "apiMiniAppMultiStore/GetOrderDetial";
// 申请退款
Address.OutOrder = HOST + "apiMiniAppMultiStore/OutOrder";
// 取消订单
Address.ChangeOrder = HOST + "apiMiniAppMultiStore/ChangeOrder";
//使用立减金
Address.GetReductionCard = HOST + "apiMiniAppCoupons/GetReductionCard";
//查询正在参与的立减金活动
Address.GetReductionCardList = HOST + "apiMiniAppCoupons/GetReductionCardList";
/****************优惠券*********************/
Address.GetMyCouponList = HOST + "apiMiniAppCoupons/GetMyCouponList";//获取我的优惠券列表
Address.GetStoreCouponList = HOST + "apiMiniAppCoupons/GetStoreCouponList";//获取店铺优惠券列表
Address.GetCoupon = HOST + "apiMiniAppCoupons/GetCoupon";//领取优惠券

Address.GetVaildCode = HOST + "webview/GetVaildCode";//获取验证码
Address.SaveUserInfo = HOST + "webview/SaveUserInfo"//注册账号
module.exports = { Address: Address }