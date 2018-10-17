const HOST = "http://testwtapi.vzan.com/";

var addr = {
  loginByThirdPlatform: HOST + "apiMiappStores/CheckUserLoginNoappsr",//登陆微赞后台

  GetHomeConfig: HOST + "apiMiappStores/GetHomeConfig",// 获取首页配置
  GetGoodsList: HOST + "apiMiappStores/GetGoodsList",// 获取分类页面
  GetStoreDetail: HOST + "apiMiappStores/GetStoreDetail",// 获取商品详情
  addGoodsCarData: HOST + "apiMiappStores/addGoodsCarData",// 添加购物车
  getGoodsCarData: HOST + "apiMiappStores/getGoodsCarData",// 查询购物车
  GetMyAddress: HOST + "apiMiappStores/GetMyAddress",// 我的收获地址
  AddOrEditMyAddressDefault: HOST + "apiMiappStores/AddOrEditMyAddressDefault",// 编辑或添加收获地址
  setMyAddressDefault: HOST + "apiMiappStores/setMyAddressDefault",// 设置默认地址
  deleteMyAddress: HOST + "apiMiappStores/deleteMyAddress",// 删除地址
  getMiniappGoodsOrder: HOST + "apiMiappStores/getMiniappGoodsOrder",// 我的订单
  GetStoreConfig: HOST + "apiMiappStores/GetStoreConfig",// 店铺配置
  getOrderGoodsBuyPriceByCarIds: HOST + "apiMiappStores/getOrderGoodsBuyPriceByCarIds",// 获取运费模板
  addMiniappGoodsOrder: HOST + "apiMiappStores/addMiniappGoodsOrder",//下单
  updateOrDeleteGoodsCarData: HOST + "apiMiappStores/updateOrDeleteGoodsCarData",//删除购物车
  getMiniappGoodsOrderById: HOST + "apiMiappStores/getMiniappGoodsOrderById",// 查看订单详情
  updateMiniappGoodsOrderState: HOST + "apiMiappStores/updateMiniappGoodsOrderState",// 更新订单状态
  checkGood: HOST + "apiMiappStores/checkGood",
  getOrderGoodsBuyPriceByGoodsIdsHOST: HOST + "apiMiappStores/getOrderGoodsBuyPriceByGoodsIds",
  GetImg: HOST + "apiMiappStores/GetImg",//获取广告图

  GetShare: HOST + "apiMiappEnterprise/GetShare?sharetype=1",// 获取一键分享
  GetAgentConfigInfo: HOST + "apiMiappPage/GetAgentConfigInfo",// 是否显示水印
  GetVipInfo: HOST + "apiMiappStores/GetVipInfo",// 获取会员信息
  commitFormId: HOST + "apiMiappStores/commitFormId",//提交虚拟formId

  //砍价
  GetBargainList: HOST + "apiMiniAppTools/GetBargainList",// 砍价列表
  GetBargain: HOST + "apiMiniAppTools/GetBargain",// 砍价商品详情
  AddBargainUser: HOST + "apiMiniAppTools/AddBargainUser",// 申请砍价
  cutprice: HOST + "apiMiniAppTools/cutprice",// 开始砍价
  GetBargainUserList: HOST + "apiMiniAppTools/GetBargainUserList",// 获取我的砍价订单
  GetShareCutPrice: HOST + "apiMiniAppTools/GetShareCutPrice",//获取砍价分享二维码
  GetBargainRecordList: HOST + "apiMiniAppTools/GetBargainRecordList",//查询砍价记录
  GetBargainUser: HOST + "apiMiniAppTools/GetBargainUser",// 砍价现价购买按钮
  GetBargainOpenState: HOST + "apiMiniAppTools/GetBargainOpenState",// 判断砍价是否开启
  AddBargainOrder: HOST + "apiMiniAppTools/AddBargainOrder",// 砍价下单
  ConfirmReceive: HOST + "apiMiniAppTools/ConfirmReceive",// 砍价确认收货接口
  GetOrderDetail: HOST + "apiMiniAppTools/GetOrderDetail",// 查看砍价的订单详情

  getSaveMoneySetList: HOST + "apiMiappSaveMoney/getSaveMoneySetList",//充值列表
  addSaveMoneySet: HOST + "apiMiappSaveMoney/addSaveMoneySet",// 请求预充值
  getSaveMoneySetUserLogList: HOST + "apiMiappSaveMoney/getSaveMoneySetUserLogList",// 获取储值记录表
  getSaveMoneySetUser: HOST + "apiMiappSaveMoney/getSaveMoneySetUser",// 获取储值余额

  getBuyModeList: HOST + "apiMiappStores/getBuyModeList",// 余额支付
  updateOrderBuyMode: HOST + "apiMiappStores/updateOrderBuyMode",// 更改二次支付方式->余额二次支付
  buyOrderbySaveMoney: HOST + "apiMiappStores/buyOrderbySaveMoney",// 余额二次支付

  //拼团
  GetGroupListPage: HOST + '/apiMiniAppGroup/GetGroupListPage',//获取首页拼团数据
  GetGroupList: HOST + '/apiMiniAppGroup/GetGroupList',//拼团列表
  GetGroupDetail: HOST + '/apiMiniAppGroup/GetGroupDetail',//获取指定的拼团商品详情
  AddGroup: HOST + '/apiMiniAppGroup/AddGroup',//一键拼团
  AddPayOrderNew: HOST + "apiMiappStores/AddPayOrderNew",//小程序新支付接口地址
  PayOrderNew: HOST + "apiMiappStores/PayOrderNew",
  GetUserWxAddress: HOST + "apiMiniAppTools/GetUserWxAddress",//查询默认地址
  UpdateUserWxAddress: HOST + "apiMiniAppTools/UpdateUserWxAddress",
  GetPaySuccessGroupDetail: HOST + 'apiMiniAppGroup/GetPaySuccessGroupDetail',//获取下单成功详情
  GetMyGroupList: HOST + "apiMiniAppGroup/GetMyGroupList",//我的拼团单
  GetMyGroupDetail: HOST + "apiMiniAppGroup/GetMyGroupDetail",//参团详情
  GetInvitePageData: HOST + "apiMiniAppGroup/GetInvitePageData",//邀请页面
  GetOrderDetail: HOST + "apiMiniAppGroup/GetOrderDetail",//拼团订单详情
  RecieveGoods: HOST + "apiMiniAppGroup/RecieveGoods",//确认收货

  // 会员卡
  GetCardSign: HOST + "apiMiappStores/GetCardSign",//获取小程序店铺卡套
  GetWxCardCode: HOST + "apiMiappStores/GetWxCardCode",//通过cardId领取微信会员卡,得到code
  SaveWxCardCode: HOST + "apiMiappStores/SaveWxCardCode",//保存提交领卡后得到的code
  UpdateWxCard: HOST + "apiMiappStores/UpdateWxCard",//用户领卡并且提交code后,在消费完成后,请求同步到微信卡包接口更新会员信息
  CancelPay: HOST + "apiMiniAppGroup/CancelPay",//取消支付减库存
}

export default addr
