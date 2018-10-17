const HOST = "http://testwtapi.vzan.com/"; //测试
// const HOST = "https://wtApi.vzan.com/";//正式
  
var addr = {
  WxLogin: HOST + 'apiPlat/WxLogin',
  Upload: HOST + "apiim/Upload", //上传 
  GetPageSettingUpdateTime: HOST + "apiMiappEnterprise/GetPageSettingUpdateTime", //
  Getaid: HOST + "apiMiappEnterprise/Getaid", // 根据小程序的appid查询aid ,
  GetAgentConfigInfo: HOST + "apiMiappEnterprise/GetAgentConfigInfo", //获取我也要做小程序配置
  GetStoreDetail: HOST +"apiPlat/GetStoreDetail",//获取店铺详情
  AddFavorite: HOST + "apiPlat/AddFavorite", //更新帖子/名片/评论/商品的/店铺 浏览量 收藏数 分享数量
  GetGoodsCategoryLevel: HOST +"apiPlat/GetGoodsCategoryLevel",//产品类别配置级别
  GetGoodsCategory: HOST +"apiPlat/GetGoodsCategory",//产品店铺类别
  GetGoodsList: HOST +"apiPlat/GetGoodsList",//获取产品列表
  GetGoodInfo: HOST +"apiPlat/GetGoodInfo",//获取产品详情
  UpdateUserInfo: HOST + "/apiPlat/UpdateUserInfo", //更新用户信息

  /***************订单相关*********************/ 
  AddPayOrder: HOST + 'apiPlat/AddPayOrder', //生成订单
  AddGoodsCarData: HOST + 'apiPlat/AddGoodsCarData', //添加购物车数据
  UpdateOrDeleteGoodsCarData: HOST + 'apiPlat/UpdateOrDeleteGoodsCarData', //更新购物车数据
  GetGoodsCarList: HOST + 'apiPlat/GetGoodsCarList', //获取购物车列表
  GetFreightFee: HOST + 'apiPlat/GetFreightFee', //获取运费
  GetOrderList: HOST + 'apiPlat/GetOrderList', //获取订单列表  
  GetOrderInfo: HOST + 'apiPlat/GetOrderInfo', //获取订单信息
  UpdateOrderState: HOST + 'apiPlat/UpdateOrderState', //修改订单状态
   
  /*******************支付**************************/
  PayOrder: HOST + "apiMiappEnterprise/PayOrder", // 微信支付订单
  GetSaveMoneySetUser: HOST + 'apiMiappSaveMoney/getSaveMoneySetUser', //获取储值
  GetSaveMoneySetList: HOST + 'apiMiappSaveMoney/getSaveMoneySetList', //获取充值列表
  AddSaveMoneySet: HOST + 'apiMiappSaveMoney/addSaveMoneySet', //充值
  GetSaveMoneySetUserLogList: HOST + 'apiMiappSaveMoney/getSaveMoneySetUserLogList', //获取账单记录
   
  /********************会员***************************/
  GetVipInfo: HOST + 'apiMiappStores/GetVipInfo', //获取会员信息
  UpdateWxCard: HOST + "apiMiappStores/UpdateWxCard", //更新会员卡
  GetWxCardCode: HOST + "apiMiappStores/GetWxCardCode", //会员卡请求
  GetCardSign: HOST + "apiMiappStores/GetCardSign", //获取会员卡Sign(签名)
  SaveWxCardCode: HOST + "apiMiappStores/SaveWxCardCode", //保存code提交
  UpdateWxCard: HOST + "apiMiappStores/UpdateWxCard", //更新会员卡
  CheckUserLoginNoappsr: HOST + 'apiPlat/CheckUserLoginNoappsr', //授权解密
  senduserauth: HOST + 'apiPlat/senduserauth', //发生验证码
  Submitauth: HOST + 'apiPlat/Submitauth', //提交验证

  /********************优惠卷***************************** */
  GetMyCouponList: HOST + 'apiMiniAppCoupons/GetMyCouponList', //获取优惠卷
  GetStoreCouponList: HOST + '/apiMiniAppCoupons/GetStoreCouponList', //获取店铺优惠卷
  GetCoupon: HOST + 'apiMiniAppCoupons/GetCoupon', //领取优惠卷
  
  /*****************收货地址*****************/
  GetUserAddress: HOST + 'Inherit/GetUserAddress', //获取用户地址列表
  EditUserAddress: HOST + 'Inherit/EditUserAddress', //编辑收货地址
  ChangeUserAddressState: HOST + 'Inherit/changeUserAddressState', //设置默认
  DeleteUserAddress: HOST + 'Inherit/DeleteUserAddress', //删除地址

  /*******************注册小未账号**********************/
  GetVaildCode: HOST + "webview/GetVaildCode",//获取验证码
  SaveUserInfo: HOST + "webview/SaveUserInfo",//注册账号

  /*************私信系列*****************/
  GetContactList: HOST + "apiim/GetProContactList", //获取联系人列表
  AddContact: HOST + "apiim/AddProContact", //添加联系人
  GetHistory: HOST + "apiim/GetHistory", //获取历史消息,

  /***************社交立减金******************/
  GetReductionCardV2: HOST + 'apiMiniAppCoupons/GetReductionCardV2',
  GetReductionCardListV2: HOST + 'apiMiniAppCoupons/GetReductionCardListV2'
}   

export default addr
