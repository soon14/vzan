const HOST = "http://testwtApi.vzan.com/";
// const HOST = "https://wtApi.vzan.com/";

module.exports = {
  loginByThirdPlatform: HOST + "apiMiappStores/CheckUserLoginNoappsr", //微赞第三方平台登录 旧版 用作更新头像和绑定手机号码
  PayOrderNew: HOST + "Inherit/PayOrderNew", //使用citymodelId调用微信支付

  GoodList: HOST + "api/apiPin/GoodList", //获取产品列表 √
  GoodInfo: HOST + "api/apiPin/GoodInfo", //获取产品详情 √
  CategoryList: HOST + "api/apiPin/CategoryList", //获取所有产品分类 √
  StoreInfo: HOST + "api/apiPin/StoreInfo", //获取店铺详情 √
  CommentList: HOST + "api/apiPin/CommentList", //评论列表 X--一期不做
  ApplyIn: HOST + "api/apiPin/ApplyIn", //申请入驻 √
  UserStore: HOST + "api/apiPin/UserStore", //获取用户店铺 
  EditStore: HOST + "api/apiPin/EditStore", //编辑门店信息 √
  addAddress: HOST + "api/apiPin/addAddress", //添加、修改、设置默认地址 √
  AddressList: HOST + "api/apiPin/AddressList", //获取用户所有收货地址 √
  deleteAddress: HOST + "api/apiPin/deleteAddress", //删除用户地址 √
  ZqStoreList: HOST + "api/apiPin/ZqStoreList", //门店的自取地址 √
  LikesList: HOST + "api/apiPin/LikesList", //获取我的收藏 √
  AddLikes: HOST + "api/apiPin/AddLikes", //添加收藏 √
  AddOrder: HOST + "api/apiPin/AddOrder", //创建订单 √
  PayAgain: HOST + "api/apiPin/PayAgain", //二次支付 √
  CheckOrder: HOST + "api/apiPin/CheckOrder", //用于检查订单超时服务
  GroupDetail: HOST + "api/apiPin/GroupDetail", //获取拼团详情 √
  OrderDetail: HOST + "api/apiPin/OrderDetail", //获取订单详情 √
  OrderList: HOST + "api/apiPin/OrderList", //订单列表 √
  OrderSuccess: HOST + "api/apiPin/OrderSuccess", //确认收货 √
  IsAgent: HOST + "api/apiPin/IsAgent", //验证是否代理 √
  PayAgent: HOST + "api/apiPin/PayAgent", //代理付费 √
  GetAgentIncome: HOST + "api/apiPin/GetAgentIncome", //获取收益详情 √
  GetIncomeList: HOST + "api/apiPin/GetIncomeList", //查看收益列表 √
  CashDetail: HOST + "api/apiPin/CashDetail", //查询可提现金额 √
  GetAgentInfo: HOST + "api/apiPin/GetAgentInfo", //根据userid查询对应用户信息 √
  ReloadUserInfo: HOST + "api/apiPin/ReloadUserInfo", //同步头像昵称 √
  ApplyBiaogan: HOST + "api/apiPin/ApplyBiaogan", //申请标杆店铺 √ 
	BiaoganStoreList: HOST + "api/apiPin/BiaoganStoreList", //标杆店铺列表 √
	GetFuserId: HOST + "api/apiPin/GetFuserId", //获取分享代理fuserid √

  //商家端
  StoreOrderList: HOST + "api/apiPin/StoreOrderList", //商家查看订单列表 √
  UpdateOrderState: HOST + "api/apiPin/UpdateOrderState", //商家更改订单状态 √
  GetGoodsTypesAll: HOST + "api/apiPin/GetGoodsTypesAll",
  GoodType: HOST + "api/apiPin/GoodType", //产品分类管理
  UploadFile: HOST + "apiim/Upload",
  GoodAttr: HOST + 'api/apiPin/GoodAttr',
  SaveGoodsInfo: HOST + 'api/apiPin/SaveGoodsInfo',
  richtexturl: HOST + "webview/PinRichText",

  PlatFormInfo: HOST + "api/apiPin/PlatFormInfo", //查询后台配置 √
  AddApply: HOST + "api/apiPin/AddApply", //发起提现 √
  DrawCashRecord: HOST + "api/apiPin/DrawCashRecord", //提现记录 √
  GetQrCode: HOST + "api/apiPin/GetQrCode", //获取二维码 √
  OrderTotal: HOST + "api/apiPin/OrderTotal", //商家查看订单列表数量 √
  getAid: HOST + "api/apiPin/getAid", //通过appid获取aid √
  WxLogin: HOST + "api/apiPin/WxLogin", //微赞第三方平台登录 √
  GetUserInfo: HOST + "api/apiPin/GetUserInfo", //通过utoken查询用户信息 X--wxlogin接口有返回 
  SendSMS: HOST + "api/apiPin/SendSMS", //发送短信验证码 √
  ValidateSMS: HOST + "api/apiPin/ValidateSMS", //提交验证短信码 √
  Upload: HOST + "api/apiPin/Upload", //上传图片 √
  UserStore: HOST + 'api/apiPin/UserStore', //查询用户开通的店铺
  EditGoods: HOST + 'api/apiPin/EditGoods', //修改，添加产品
}