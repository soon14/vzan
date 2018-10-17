 const HOST = "http://testwtapi.vzan.com/"; //测试
// const HOST = "https://wtApi.vzan.com/";//正式
// const HOST = "http://hyqapi.vzan.com/";
// const ws='ws://118.126.93.46:9528/'//私信测试
const ws='wss://dzwss.xiaochengxu.com.cn/'

var addr = {
  ws:ws,
  live: HOST + "apiMiappEnterprise/GetLivePlay",
  WxLogin: HOST + "apiMiappEnterprise/WxLogin", //登录微赞后台
  loginByThirdPlatform: HOST + "apiMiappEnterprise/CheckUserLoginNoappsr", //登录微赞后台
  GetGoodsList: HOST + "apiMiappEnterprise/GetGoodsList", //获取产品列表
  GetPageSetting: HOST + "apiMiappEnterprise/GetPageSetting", // 获取企业版object
  GetPageSettingUpdateTime: HOST + "apiMiappEnterprise/GetPageSettingUpdateTime", //
  SaveUserForm: HOST + "apiMiappEnterprise/SaveUserForm", // 表单提交
  GetGoodInfo: HOST + "apiMiappEnterprise/GetGoodInfo", // 单个产品信息
  Getaid: HOST + "apiMiappEnterprise/Getaid", // 根据小程序的appid查询aid
  SaveSubscribeForm: HOST + "apiMiappEnterprise/SaveSubscribeForm", //保存产品预约信息
  GetNewsList: HOST + "apiMiappEnterprise/GetNewsList", //新闻
  GetNewsInfo: HOST + "apiMiappEnterprise/GetNewsInfo", //单个新闻详情
  GetGoodsByids: HOST + "apiMiappEnterprise/GetGoodsByids", //产品更新
  GetNewsInfoByids: HOST + "apiMiappEnterprise/GetNewsInfoByids", // 新闻更新
  GetExtTypes: HOST + "/apiMiappEnterprise/GetExtTypes", //自定义分类
  GetAgentConfigInfo: HOST + "apiMiappEnterprise/GetAgentConfigInfo", //去水印接口
  GetShare: HOST + "apiMiappEnterprise/GetShare", //分享按钮
  GetSubscribeFormDetail: HOST + "apiMiappEnterprise/GetSubscribeFormDetail", // 查看预约单
  getGoodsCarData_new: HOST + "/apiMiappEnterprise/getGoodsCarData_new", //无分类添加购物车
  addGoodsCarData: HOST + "apiMiappEnterprise/addGoodsCarData", // 添加商品到购物车
  updateOrDeleteGoodsCarData: HOST + "apiMiappEnterprise/updateOrDeleteGoodsCarData", // 编辑 / 删除 购物车(仅删除 / 更改数量, 规格)
  addMiniappGoodsOrder: HOST + "apiMiappEnterprise/addMiniappGoodsOrder", // 生成订单
  PayOrder: HOST + "apiMiappEnterprise/PayOrder", // 微信支付订单
  getMiniappGoodsOrder: HOST + "apiMiappEnterprise/getMiniappGoodsOrder", // 获取我的订单
  getMiniappGoodsOrderById: HOST + "apiMiappEnterprise/getMiniappGoodsOrderById", // 查看订单详情
  updateOrDeleteGoodsCarDataBySingle: HOST + "apiMiappEnterprise/updateOrDeleteGoodsCarDataBySingle", //更新订单状态
  GetStoreInfo: HOST + "apiMiappEnterprise/GetStoreInfo", // 店铺配置
  GetBargainList: HOST + "apiMiniAppTools/GetBargainList?BargainType=1", // 砍价列表
  GetBargain: HOST + "apiMiniAppTools/GetBargain?BargainType=1", // 砍价商品详情
  AddBargainUser: HOST + "apiMiniAppTools/AddBargainUser?BargainType=1", // 申请砍价
  cutprice: HOST + "apiMiniAppTools/cutprice?BargainType=1", // 开始砍价
  GetBargainUserList: HOST + "apiMiniAppTools/GetBargainUserList?BargainType=1", // 获取我的砍价订单
  GetShareCutPrice: HOST + "apiMiniAppTools/GetShareCutPrice?BargainType=1", //获取砍价分享二维码
  GetBargainRecordList: HOST + "apiMiniAppTools/GetBargainRecordList?BargainType=1", //查询砍价记录
  GetBargainUser: HOST + "apiMiniAppTools/GetBargainUser?BargainType=1", // 砍价现价购买按钮
  GetBargainOpenState: HOST + "apiMiniAppTools/GetBargainOpenState?BargainType=1", // 判断砍价是否开启
  AddBargainOrder: HOST + "apiMiniAppTools/AddBargainOrder?BargainType=1", // 下单
  GetUserWxAddress: HOST + "apiMiniAppTools/GetUserWxAddress", //查询默认地址
  commitFormId: HOST + "apiMiappStores/commitFormId", //提交虚拟formId
  deleteLastFormId:HOST+"apiMiappStores/deleteLastFormId",
  GetOrderDetail: HOST + "apiMiniAppTools/GetOrderDetail", // 查看砍价的订单详情
  ConfirmReceive: HOST + "apiMiniAppTools/ConfirmReceive", // 砍价确认收货接口
  GetGroupListPage: HOST + 'apiMiniAppGroup/GetGroupListPage', //获取首页拼团
  GetGroupList: HOST + 'apiMiniAppGroup/GetGroupList', //拼团列表
  GetGroupDetail: HOST + 'apiMiniAppGroup/GetGroupDetail', //获取指定的拼团商品详情
  AddGroup: HOST + 'apiMiniAppGroup/AddGroup', //一键拼团
  AddPayOrderNew: HOST + "apiMiappStores/AddPayOrderNew", //小程序新支付接口地址
  GetPaySuccessGroupDetail: HOST + 'apiMiniAppGroup/GetPaySuccessGroupDetail', //获取下单成功详情
  GetMyGroupList: HOST + "apiMiniAppGroup/GetMyGroupList", //我的拼团单
  GetMyGroupDetail: HOST + "apiMiniAppGroup/GetMyGroupDetail", //参团详情
  GetInvitePageData: HOST + "apiMiniAppGroup/GetInvitePageData", //邀请页面
  GetGroupOrderDetail: HOST + "apiMiniAppGroup/GetOrderDetail", //拼团订单详情
  RecieveGoods: HOST + "apiMiniAppGroup/RecieveGoods", //确认收货
  CancelPay: HOST + "apiMiniAppGroup/CancelPay", //取消支付减库存
  GetVipInfo: HOST + "apiMiappEnterprise/GetVipInfo", // 获取会员信息
  getSaveMoneySetList: HOST + "apiMiappSaveMoney/getSaveMoneySetList", //获取充值列表
  getSaveMoneySetUser: HOST + "apiMiappSaveMoney/getSaveMoneySetUser", //显示储值金额
  addSaveMoneySet: HOST + "apiMiappSaveMoney/addSaveMoneySet", //充值
  GetPayLogList: HOST + "apiMiappSaveMoney/GetPayLogList", //充值记录
  updateMiniappGoodsOrderState: HOST + "apiMiappEnterprise/updateMiniappGoodsOrderState", //更新订单状态
  GetGroupByIds: HOST + "apiMiappEnterprise/GetGroupByIds", //拼团列表显示
  GetWxCardCode: HOST + "apiMiappStores/GetWxCardCode", //会员卡请求
  GetCardSign: HOST + "apiMiappStores/GetCardSign", //获取会员卡Sign(签名)
  SaveWxCardCode: HOST + "apiMiappStores/SaveWxCardCode", //保存code提交
  UpdateWxCard: HOST + "apiMiappStores/UpdateWxCard", //更新会员卡
  PayByStoredvalue: HOST + "apiMiappEnterprise/PayByStoredvalue", //储值支付
  StoredvalueOrderInfo: HOST + "apiMiappEnterprise/StoredvalueOrderInfo", //获取储值支付订单详情
  /****************优惠券*********************/
  GetMyCouponList: HOST + "apiMiniAppCoupons/GetMyCouponList", //获取我的优惠券列表
  GetStoreCouponList: HOST + "apiMiniAppCoupons/GetStoreCouponList", //获取店铺优惠券列表
  GetCoupon: HOST + "apiMiniAppCoupons/GetCoupon", //领取优惠券
  /****************积分中心*********************/
  GetExchangeActivityList: HOST + "apiMiappEnterprise/GetExchangeActivityList", //获取积分商城活动列表
  GetExchangeActivity: HOST + "apiMiappEnterprise/GetExchangeActivity", //获取积分商品详情
  AddExchangeActivityOrder: HOST + "apiMiappEnterprise/AddExchangeActivityOrder", //兑换积分商品
  GetStoreRules: HOST + "apiMiappEnterprise/GetStoreRules", //获取积分规则
  GetUserIntegral: HOST + "apiMiappEnterprise/GetUserIntegral", //获取用户积分
  GetExchangeActivityOrders: HOST + "apiMiappEnterprise/GetExchangeActivityOrders", //获取积分订单列表
  GetUserIntegralLogs: HOST + "apiMiappEnterprise/GetUserIntegralLogs", //获取积分记录
  ConfirmReciveGood: HOST + "apiMiappEnterprise/ConfirmReciveGood", //确认收货
  GetReductionCard: HOST + "apiMiniAppCoupons/GetReductionCard", //使用立减金
  GetReductionCardList: HOST + "apiMiniAppCoupons/GetReductionCardList", //查询正在参与的立减金活动
  /****************新版拼团*********************/
  GetentGroupDetail: HOST + "apiMiniAppEntGroup/GetGroupDetail", //参团详情2.0
  GetMyGroupList2: HOST + "apiMiniAppEntGroup/GetMyGroupList", //我的拼团单2.0
  GetEntGroupByIds: HOST + "apiMiappEnterprise/GetEntGroupByIds", //产品更新
  GetAddressByIp: HOST + "apiMiappEnterprise/GetAddressByIp", //根据ip查询地址
  GetUserAddress: HOST + "apiMiappEnterprise/GetUserAddress", //根据用户id查询收货地址
  EditUserAddress: HOST + "apiMiappEnterprise/EditUserAddress", //添加，修改，设置取消默认 收货地址
  DeleteUserAddress: HOST + "apiMiappEnterprise/DeleteUserAddress", //删除用户地址
  changeUserAddressState: HOST + "apiMiappEnterprise/changeUserAddressState", //设置取消默认 收货地址
  GetFreightFee: HOST + "apiMiappEnterprise/GetFreightFee", //运费模板
  /****************分销中心*********************/
  GetMiniAppSaleManConfig: HOST + "apiMiappEnterprise/GetMiniAppSaleManConfig", //获取小程序分销配置以及当前用户是否成为分销员了
  ApplySalesman: HOST + "apiMiappEnterprise/ApplySalesman", //申请成为分销员
  GetSalesManRecord: HOST + "apiMiappEnterprise/GetSalesManRecord", //获取推广分享记录Id
  UpdateSalesManRecord: HOST + "apiMiappEnterprise/UpdateSalesManRecord", //更新推广分享记录状态 默认更新为可用 state:1
  GetSalesmanGoodsList: HOST + "apiMiappEnterprise/GetSalesmanGoodsList", //.获取分销产品
  GetSalesManRecordOrder: HOST + "apiMiappEnterprise/GetSalesManRecordOrder", //获取分销推广订单
  GetSalesManRecordUser: HOST + "apiMiappEnterprise/GetSalesManRecordUser", //获取分销员累计的客户
  BindRelationShip: HOST + "apiMiappEnterprise/BindRelationShip", //建立分销员与客户的关系  当用户点击从分销市场分享出去的商品链接
  GetSalesManUserInfo: HOST + "apiMiappEnterprise/GetSalesManUserInfo", //获取分销员相关信息 各个人分销中心
  DrawCashApply: HOST + "apiMiappEnterprise/DrawCashApply", //申请提现'
  GetDrawCashApplyList: HOST + "apiMiappEnterprise/GetDrawCashApplyList", //获取分销员提现记录
  GetProductQrcode: HOST + "apiEnterprise/GetProductQrcode", //获取二维码
  GetSaleManRelationList: HOST + "apiMiappEnterprise/GetSaleManRelationList", //二级分销
  //排队
  GetSortQueueSwitch: HOST + "Inherit/GetSortQueueSwitch", //排队拿号功能是否开启
  PutSortQueueMsg: HOST + "Inherit/PutSortQueueMsg", //申请拿号
  GetUserInSortQueuesPlanMsg: HOST + "Inherit/GetUserInSortQueuesPlanMsg", //获取当前队列位置信息
  CancelSortQueue: HOST + "Inherit/CancelSortQueue", //取消排队

  //im
  GetContactList: HOST + "apiim/GetProContactList", //获取联系人列表
  AddContact: HOST + "apiim/AddProContact", //添加联系人
  GetHistory: HOST + "apiim/GetHistory", //获取历史消息
  Upload: HOST + "apiim/Upload", //上传

  /****************物流信息*********************/
  LogisticsInfo: HOST + 'apiMiappEnterprise/GetOrderDeliveryFeed',

  /****************申請售後*********************/
  submitReturnGoodAppeal: HOST + 'apiMiappEnterprise/updateMiniappGoodsOrderState',
  getDeliveryCompany: HOST + 'apiMiniAppTools/GetDeliveryCompany',
  getReturnGoodDetail: HOST + 'apiMiappEnterprise/GetReutrnOrderInfo',

  GetFunctionList: HOST + "apiMiappEnterprise/GetFunctionList", //版本功能相关开关
  GetVersonId: HOST + "apiMiappEnterprise/GetVersonId", //版本

  /*******************官网************************** */
  SendUserAdvisory: HOST + "apiMiniAppGw/SendUserAdvisory",
  SendUserAuthCode: HOST + "apiMiniAppGw/GetVaildCode",
  SaveUserInfo: HOST + "apiMiniAppGw/SaveUserInfo",

  /******************评论**********************************/
  AddGoodsComment: HOST + "apiEnterprise/AddGoodsComment",
  GetGoodsCommentList: HOST + "apiEnterprise/GetGoodsCommentList",
  GetUserGoodsCommentList: HOST + "apiEnterprise/GetUserGoodsCommentList",
  PointsGoodsComment: HOST + "apiEnterprise/PointsGoodsComment",
  /******************二级分类**********************************/
  GetSecondGoodTypeList: HOST + "apiMiappEnterprise/GetSecondGoodTypeList",
  GetGoodTypeList: HOST + "apiMiappEnterprise/GetGoodTypeList",
  /******************绑定手机号码**********************************/
  senduserauth: HOST + "apiMiappEnterprise/senduserauth",
  Submitauth: HOST + "apiMiappEnterprise/Submitauth",
  GetVaildCode: HOST + "webview/GetVaildCode", //获取验证码
  SaveUserInfo: HOST + "webview/SaveUserInfo", //注册账号 
  GetTableNoQrCode: HOST + "apiMiappEnterprise/GetTableNoQrCode", //核销二维码

  addPayContentOrder: HOST + "apiMiappEnterprise/addPayContentOrder", // 付费内容
  /************************秒杀********************************* */
  GetFlashDeal: HOST + "apiMiappEnterprise/GetFlashDeal", //获取秒杀组件
  GetFlashItem: HOST + "apiMiappEnterprise/GetFlashItem", //秒杀详情
  AddFlashSubscribe: HOST + "apiMiappEnterprise/AddFlashSubscribe", //秒杀提醒


  GetUserPlayCard: HOST + "apiMiappEnterprise/GetUserPlayCard", //获取用户签到信息
  PlayCard: HOST + "apiMiappEnterprise/PlayCard", //用户点击签到
  AddQrCodeScanRecord: HOST + "apiMiappEnterprise/AddQrCodeScanRecord",
  UpdateNewsPV: HOST + "apiMiappEnterprise/UpdateNewsPV",
  
  GetAllVipRights: HOST + "apiMiappEnterprise/GetAllVipRights",//获取会员所有权益

  GetFullReductionByAid:HOST+"apiMiniAppTools/GetFullReductionByAid",//满减s
  GetBargainFreightFee:HOST+"apiMiappEnterprise/GetBargainFreightFee",//获取砍价运费-接口
  GetOrderRecordCount:HOST+"apiMiappEnterprise/GetOrderRecordCount",//订单数量
  GetStorePickPlace:HOST+"apiMiappEnterprise/GetStorePickPlace",//店铺
}

export default addr
