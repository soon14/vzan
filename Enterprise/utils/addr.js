 const HOST = "http://testwtapi.vzan.com/";   //测试
// const HOST = "http://hyqTestApi.vzan.com/";
//const HOST = "https://wtApi.vzan.com/";//正式
// const HOST = "http://pp.vzan.com/";//局域网
//const HOST = "http://api2.vzan.com/";//正式
function Address() {
}

// Address.loginByThirdPlatform = "http://txiaowei.vzan.com/apiQuery/CheckUserLogin"; //登陆微赞后台

// 直播接口
Address.WxLogin = HOST + "apiMiappEnterprise/WxLogin";
Address.live = HOST + "apiMiappEnterprise/GetLivePlay";

Address.loginByThirdPlatform = HOST + "apiMiappEnterprise/CheckUserLoginNoappsr"; //登陆微赞后台

Address.GetGoodsList = HOST + "apiMiappEnterprise/GetGoodsList"//获取产品列表

Address.GetPageSetting = HOST + "apiMiappEnterprise/GetPageSetting"// 获取企业版object

Address.SaveUserForm = HOST + "apiMiappEnterprise/SaveUserForm"// 表单提交

Address.GetGoodInfo = HOST + "apiMiappEnterprise/GetGoodInfo"// 单个产品信息

Address.Getaid = HOST + "apiMiappEnterprise/Getaid"// 根据小程序的appid查询aid

Address.SaveSubscribeForm = HOST + "apiMiappEnterprise/SaveSubscribeForm"//保存产品预约信息

Address.GetNewsList = HOST + "apiMiappEnterprise/GetNewsList"//新闻

Address.GetNewsInfo = HOST + "apiMiappEnterprise/GetNewsInfo"//单个新闻详情

Address.GetGoodsByids = HOST + "apiMiappEnterprise/GetGoodsByids"//产品更新

Address.GetNewsInfoByids = HOST + "apiMiappEnterprise/GetNewsInfoByids"// 新闻更新

Address.GetExtTypes = HOST + "/apiMiappEnterprise/GetExtTypes"//自定义分类

Address.GetAgentConfigInfo = HOST + "apiMiappEnterprise/GetAgentConfigInfo"//去水印接口

Address.GetShare = HOST + "apiMiappEnterprise/GetShare"//分享按钮

Address.GetSubscribeFormDetail = HOST + "apiMiappEnterprise/GetSubscribeFormDetail"// 查看预约单

Address.getGoodsCarData = HOST + "apiMiappEnterprise/getGoodsCarData"// 查询购物车
Address.getGoodsCarData_new = HOST + "/apiMiappEnterprise/getGoodsCarData_new"//无分类添加购物车

Address.addGoodsCarData = HOST + "apiMiappEnterprise/addGoodsCarData"// 添加商品到购物车

Address.updateOrDeleteGoodsCarData = HOST + "apiMiappEnterprise/updateOrDeleteGoodsCarData"// 编辑 / 删除 购物车(仅删除 / 更改数量, 规格)

Address.addMiniappGoodsOrder = HOST + "apiMiappEnterprise/addMiniappGoodsOrder"// 生成订单

Address.getOrderGoodsBuyPriceByCarIds = HOST + "apiMiappEnterprise/getOrderGoodsBuyPriceByCarIds"// 获取运费模板

Address.PayOrder = HOST + "apiMiappEnterprise/PayOrder"// 微信支付订单
// Address.PayOrder = HOST + "Inherit/PayOrderNew"// 微信支付订单

Address.getMiniappGoodsOrder = HOST + "apiMiappEnterprise/getMiniappGoodsOrder"// 获取我的订单

Address.getMiniappGoodsOrderById = HOST + "apiMiappEnterprise/getMiniappGoodsOrderById"// 查看订单详情

Address.updateOrDeleteGoodsCarDataBySingle = HOST + "apiMiappEnterprise/updateOrDeleteGoodsCarDataBySingle"//更新订单状态

Address.GetStoreConfig = HOST + "apiMiappStores/GetStoreConfig"// 店铺配置

Address.GetBargainList = HOST + "apiMiniAppTools/GetBargainList?BargainType=1"// 砍价列表

Address.GetBargain = HOST + "apiMiniAppTools/GetBargain?BargainType=1"// 砍价商品详情

Address.AddBargainUser = HOST + "apiMiniAppTools/AddBargainUser?BargainType=1"// 申请砍价

Address.cutprice = HOST + "apiMiniAppTools/cutprice?BargainType=1"// 开始砍价

Address.GetBargainUserList = HOST + "apiMiniAppTools/GetBargainUserList?BargainType=1"// 获取我的砍价订单

Address.GetShareCutPrice = HOST + "apiMiniAppTools/GetShareCutPrice?BargainType=1"//获取砍价分享二维码

Address.GetBargainRecordList = HOST + "apiMiniAppTools/GetBargainRecordList?BargainType=1"//查询砍价记录

Address.GetBargainUser = HOST + "apiMiniAppTools/GetBargainUser?BargainType=1"// 砍价现价购买按钮

Address.GetBargainOpenState = HOST + "apiMiniAppTools/GetBargainOpenState?BargainType=1"// 判断砍价是否开启

Address.AddBargainOrder = HOST + "apiMiniAppTools/AddBargainOrder?BargainType=1"// 下单

Address.GetUserWxAddress = HOST + "apiMiniAppTools/GetUserWxAddress" //查询默认地址

Address.commitFormId = HOST + "apiMiappStores/commitFormId"//提交虚拟formId

Address.GetOrderDetail = HOST + "apiMiniAppTools/GetOrderDetail"// 查看砍价的订单详情

Address.ConfirmReceive = HOST + "apiMiniAppTools/ConfirmReceive"// 砍价确认收货接口


Address.GetGroupListPage = HOST + '/apiMiniAppGroup/GetGroupListPage'//获取首页拼团

Address.GetGroupList = HOST + '/apiMiniAppGroup/GetGroupList'//拼团列表

Address.GetGroupDetail = HOST + '/apiMiniAppGroup/GetGroupDetail'//获取指定的拼团商品详情

Address.AddGroup = HOST + '/apiMiniAppGroup/AddGroup'//一键拼团

Address.AddPayOrderNew = HOST + "apiMiappStores/AddPayOrderNew"//小程序新支付接口地址

Address.PayOrderNew = HOST + "apiMiappStores/PayOrderNew";

Address.UpdateUserWxAddress = HOST + "apiMiniAppTools/UpdateUserWxAddress"

Address.GetPaySuccessGroupDetail = HOST + 'apiMiniAppGroup/GetPaySuccessGroupDetail'//获取下单成功详情

Address.GetMyGroupList = HOST + "apiMiniAppGroup/GetMyGroupList"//我的拼团单

Address.GetMyGroupDetail = HOST + "apiMiniAppGroup/GetMyGroupDetail"//参团详情

Address.GetInvitePageData = HOST + "apiMiniAppGroup/GetInvitePageData"//邀请页面

Address.GetGroupOrderDetail = HOST + "apiMiniAppGroup/GetOrderDetail"//拼团订单详情

Address.RecieveGoods = HOST + "apiMiniAppGroup/RecieveGoods"//确认收货

Address.CancelPay = HOST + "apiMiniAppGroup/CancelPay" //取消支付减库存

Address.GetVipInfo = HOST + "/apiMiappEnterprise/GetVipInfo"// 获取会员信息

Address.getSaveMoneySetList = HOST + "apiMiappSaveMoney/getSaveMoneySetList"//获取充值列表

Address.getSaveMoneySetUser = HOST + "apiMiappSaveMoney/getSaveMoneySetUser"//显示储值金额

Address.addSaveMoneySet = HOST + "apiMiappSaveMoney/addSaveMoneySet"//充值

Address.getSaveMoneySetUserLogList = HOST + "apiMiappSaveMoney/getSaveMoneySetUserLogList"//充值记录


Address.updateMiniappGoodsOrderState = HOST + "apiMiappEnterprise/updateMiniappGoodsOrderState"//更新订单状态

Address.GetGroupByIds = HOST + "apiMiappEnterprise/GetGroupByIds"//拼团列表显示


Address.GetWxCardCode = HOST + "apiMiappStores/GetWxCardCode"//会员卡请求

Address.GetCardSign = HOST + "apiMiappStores/GetCardSign"//获取会员卡Sign(签名)

Address.SaveWxCardCode = HOST + "apiMiappStores/SaveWxCardCode"//保存code提交

Address.UpdateWxCard = HOST + "apiMiappStores/UpdateWxCard"//更新会员卡

Address.PayByStoredvalue = HOST + "apiMiappEnterprise/PayByStoredvalue"//储值支付

Address.StoredvalueOrderInfo = HOST + "apiMiappEnterprise/StoredvalueOrderInfo"//获取储值支付订单详情

/****************优惠券*********************/
Address.GetMyCouponList = HOST + "apiMiniAppCoupons/GetMyCouponList"//获取我的优惠券列表
Address.GetStoreCouponList = HOST + "apiMiniAppCoupons/GetStoreCouponList"//获取店铺优惠券列表
Address.GetCoupon = HOST + "apiMiniAppCoupons/GetCoupon"//领取优惠券

/****************积分中心*********************/
Address.GetExchangeActivityList = HOST + "apiMiappEnterprise/GetExchangeActivityList"//获取积分商城活动列表
Address.GetExchangeActivity = HOST + "apiMiappEnterprise/GetExchangeActivity"//获取积分商品详情
Address.AddExchangeActivityOrder = HOST + "apiMiappEnterprise/AddExchangeActivityOrder"//兑换积分商品
Address.GetStoreRules = HOST + "apiMiappEnterprise/GetStoreRules"//获取积分规则
Address.GetUserIntegral = HOST + "apiMiappEnterprise/GetUserIntegral"//获取用户积分
Address.GetExchangeActivityOrders = HOST + "apiMiappEnterprise/GetExchangeActivityOrders"//获取积分订单列表
Address.GetUserIntegralLogs = HOST + "apiMiappEnterprise/GetUserIntegralLogs"//获取积分记录
Address.ConfirmReciveGood = HOST + "apiMiappEnterprise/ConfirmReciveGood"//确认收货

Address.GetStoreInfo = HOST + "apiMiappEnterprise/GetStoreInfo" //获取店铺配置

Address.GetReductionCard = HOST + "apiMiniAppCoupons/GetReductionCard" //使用立减金
Address.GetReductionCardList = HOST + "apiMiniAppCoupons/GetReductionCardList" //查询正在参与的立减金活动

/****************新版拼团*********************/
Address.GetentGroupDetail = HOST + "apiMiniAppEntGroup/GetGroupDetail" //参团详情2.0
Address.GetMyGroupList2 = HOST + "apiMiniAppEntGroup/GetMyGroupList"//我的拼团单2.0
Address.GetEntGroupByIds = HOST + "apiMiappEnterprise/GetEntGroupByIds"//产品更新

Address.GetAddressByIp = HOST + "apiMiappEnterprise/GetAddressByIp" //根据ip查询地址
Address.GetUserAddress = HOST + "apiMiappEnterprise/GetUserAddress" //根据用户id查询收货地址
Address.EditUserAddress = HOST + "apiMiappEnterprise/EditUserAddress" //添加，修改，设置取消默认 收货地址
Address.DeleteUserAddress = HOST + "apiMiappEnterprise/DeleteUserAddress" //删除用户地址
Address.changeUserAddressState = HOST + "apiMiappEnterprise/changeUserAddressState" //设置取消默认 收货地址
/****************分销中心*********************/
Address.GetMiniAppSaleManConfig = HOST + "apiMiappEnterprise/GetMiniAppSaleManConfig"//获取小程序分销配置以及当前用户是否成为分销员了
Address.ApplySalesman = HOST + "apiMiappEnterprise/ApplySalesman"//申请成为分销员
Address.GetSalesManRecord = HOST + "apiMiappEnterprise/GetSalesManRecord"//获取推广分享记录Id
Address.UpdateSalesManRecord = HOST + "apiMiappEnterprise/UpdateSalesManRecord"//更新推广分享记录状态 默认更新为可用 state=1
Address.GetSalesmanGoodsList = HOST + "apiMiappEnterprise/GetSalesmanGoodsList"//.获取分销产品
Address.GetSalesManRecordOrder = HOST + "apiMiappEnterprise/GetSalesManRecordOrder"//获取分销推广订单
Address.GetSalesManRecordUser = HOST + "apiMiappEnterprise/GetSalesManRecordUser"//获取分销员累计的客户
Address.GetUserAddress = HOST + "apiMiappEnterprise/GetUserAddress"//查询用户收货地址
Address.BindRelationShip = HOST + "apiMiappEnterprise/BindRelationShip"//建立分销员与客户的关系  当用户点击从分销市场分享出去的商品链接
Address.GetSalesManUserInfo = HOST + "apiMiappEnterprise/GetSalesManUserInfo"//获取分销员相关信息 各个人分销中心
Address.DrawCashApply = HOST + "apiMiappEnterprise/DrawCashApply"//申请提现'
Address.GetDrawCashApplyList = HOST + "apiMiappEnterprise/GetDrawCashApplyList"//获取分销员提现记录
/*****************达达配送****************************/
Address.GetDadaFreight = HOST + "apiMiniAppDistribution/GetDadaFreight";//计算运费

/********************计算运费************************************ */
// Address.GetFreightFee = "http://apiwx.vzan.com/" +"apiMiappEnterprise/GetFreightFee" ;
Address.GetFreightFee = HOST + "apiMiappEnterprise/GetFreightFee";  //运费模板

Address.GetTableNoQrCode = HOST + "apiMiappEnterprise/GetTableNoQrCode"


//im
Address.GetContactList = HOST + "apiim/GetProContactList" //获取联系人列表
Address.AddContact = HOST + "apiim/AddProContact" //添加联系人
Address.GetHistory = HOST + "apiim/GetHistory" //获取历史消息
Address.Upload = HOST + "apiim/Upload" //上传

Address.GetProductQrcode = HOST + "apiEnterprise/GetProductQrcode"    //获取二维码

//排队
Address.GetSortQueueSwitch = HOST + "Inherit/GetSortQueueSwitch"//排队拿号功能是否开启
Address.PutSortQueueMsg = HOST + "Inherit/PutSortQueueMsg"//申请拿号
Address.GetUserInSortQueuesPlanMsg = HOST + "Inherit/GetUserInSortQueuesPlanMsg"//获取当前队列位置信息
Address.CancelSortQueue = HOST + "Inherit/CancelSortQueue"//取消排队
//预约购物
Address.GetReserveGoodClass = "http://apiwx.vzan.com/" + "apiMiappEnterprise/GetReserveGoodClass"//获取预约购物商品分类
Address.GetVaildCode = HOST + "webview/GetVaildCode"//获取验证码
Address.SaveUserInfo = HOST + "webview/SaveUserInfo"//注册账号


module.exports = {
  Address: Address,
}