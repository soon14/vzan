// const HOST = "http://testwtApi.vzan.com/";
//const HOST = "https://wtApi.vzan.com/";
const HOST = "http://api2.vzan.com/";
function Address() {

}


Address.loginByThirdPlatform = HOST + "apiMiappStores/CheckUserLoginNoappsr"; //登陆微赞后台
Address.WxLogin = HOST + "apiMiappStores/WxLogin"; //登陆微赞后台

Address.GetFoodsDetail = HOST + "apiMiappFoodsNew/GetFoodsDetail"// 店铺信息

Address.GetGoodsTypeList = HOST + "apiMiappFoods/GetGoodsTypeList"// 首页商品分类信息

Address.GetGoodsList = HOST + "apiMiappFoods/GetGoodsList"//首页商品信息

Address.GetGoodsDtl = HOST + "apiMiappFoods/GetGoodsDtl"// 获取商品详情

Address.getGoodsCarData = HOST + "apiMiappFoods/getGoodsCarData"// 获取购物车列表

Address.GetMyAddress = HOST + "apiMiappFoods/GetMyAddress"// 获取我的收货地址

Address.AddOrEditMyAddressDefault = HOST + "apiMiappFoods/AddOrEditMyAddressDefault"// 新增收货地址

Address.getMiniappGoodsOrder = HOST + "apiMiappFoods/getMiniappGoodsOrder"// 获取我的订单

Address.deleteMyAddress = HOST + "apiMiappFoods/deleteMyAddress"// 删除我的收货地址

Address.addGoodsCarData = HOST + "apiMiappFoods/addGoodsCarData"// 提交商品到购物车

Address.AddPayOrder_Store = HOST + "apiMiappFoodsNew/addMiniappGoodsOrder"//下单

// Address.PayOrder = HOST + "apiMiappFoods/PayOrder"// 支付
Address.PayOrder = HOST + "Inherit/PayOrderNew"// 支付

Address.updateMiniappGoodsOrderState = HOST + "apiMiappFoodsNew/updateMiniappGoodsOrderState"// 更新订单状态

Address.getMiniappGoodsOrderById = HOST + "apiMiappFoodsNew/getMiniappGoodsOrderById"// 查看订单

Address.GetDistanceForFood = HOST + "apiMiappFoods/GetDistanceForFood"// 查看配送距离

Address.testMuBang = HOST + "apiMiappFoods/testMuBang"// 表单测试

Address.GetAgentConfigInfo = HOST + "apiMiappPage/GetAgentConfigInfo"// 是否显示水印

Address.GetVipInfo = HOST + "apiMiappStores/GetVipInfo"// 获取会员信息

Address.getSaveMoneySetList = HOST + "apiMiappSaveMoney/getSaveMoneySetList"// 获取储值列表

Address.addSaveMoneySet = HOST + "apiMiappSaveMoney/addSaveMoneySet"// 请求预充值

Address.getSaveMoneySetUserLogList = HOST + "apiMiappSaveMoney/getSaveMoneySetUserLogList"// 获取储值记录列表

Address.getSaveMoneySetUser = HOST + "apiMiappSaveMoney/getSaveMoneySetUser"// 获取储余额

Address.buyOrderbySaveMoney = HOST + "apiMiappStores/buyOrderbySaveMoney"// 更改支付方式

Address.commitFormId = HOST + "apiMiappStores/commitFormId"//提交虚拟formId

Address.GetCardSign = HOST + "apiMiappStores/GetCardSign"//获取小程序店铺卡套

Address.GetWxCardCode = HOST + "apiMiappStores/GetWxCardCode"//通过cardId领取微信会员卡,得到code

Address.SaveWxCardCode = HOST + "apiMiappStores/SaveWxCardCode"//保存提交领卡后得到的code

Address.UpdateWxCard = HOST + "apiMiappStores/UpdateWxCard"//用户领卡并且提交code后,在消费完成后,请求同步到微信卡包接口更新会员信息

Address.GetReductionCard = HOST + "apiMiniAppCoupons/GetReductionCard"//使用立减金

Address.GetReductionCardList = HOST + "apiMiniAppCoupons/GetReductionCardList"//查询正在参与的立减金活动

Address.GetMyCouponList = HOST + "apiMiniAppCoupons/GetMyCouponList"//获取我的优惠券列表
Address.GetStoreCouponList = HOST + "apiMiniAppCoupons/GetStoreCouponList"//获取店铺优惠券列表
Address.GetCoupon = HOST + "apiMiniAppCoupons/GetCoupon"//领取优惠券


Address.PutSortQueueMsg = HOST + "Inherit/PutSortQueueMsg"//申请拿号
Address.GetUserInSortQueuesPlanMsg = HOST + "Inherit/GetUserInSortQueuesPlanMsg"//获取当前队列位置信息
Address.CancelSortQueue = HOST + "Inherit/CancelSortQueue"//取消排队

Address.GetDadaFreight = HOST + "apiMiniAppDistribution/GetDadaFreight"//查询配送费


Address.AddReservation = HOST + "apiMiappFoods/AddReservation"//预约下单
Address.GetReserveMenu = HOST + "apiMiappFoods/GetReserveMenu"//获取已经支付的预约菜单
Address.GetReservation = HOST + "apiMiappFoods/GetReservation"//获取预约详情
Address.CancelResevation = HOST + "apiMiappFoods/CancelResevation"//获取预约详情
Address.GetReserveMenuPay = HOST + "apiMiappFoods/GetReserveMenuPay"//获取到店扫码支付菜单

/* 拼团 */
Address.GetGroupDetail = HOST + "apiMiniAppEntGroup/GetGroupDetail"//查看拼团详情
Address.GetMyGroupList = HOST + "apiMiniAppEntGroup/GetMyGroupList"//查看我拼团列表
Address.GetGoodsDtl = HOST + "apiMiappFoods/GetGoodsDtl"//查看团详情
Address.AddPayOrder = HOST + "apiMiappFoods/AddPayOrder"//支付拼团
/* end */
Address.GetTableNo = HOST + "apiMiappFoods/GetTableNo"//获取桌台号名称








module.exports = {
	Address: Address,
}