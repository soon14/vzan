const HOST = "http://testwtapi.vzan.com/"; //测试
// const HOST = "https://wtApi.vzan.com/";//正式

var addr = {
  WxLogin: HOST + 'apiPlat/WxLogin',
  Upload: HOST + "apiim/Upload", //上传 
  GetConfig: HOST + "apiPlat/GetConfig",//首页轮播图以及推荐商家配置
  GetStoreCategoryLevel: HOST +"apiPlat/GetStoreCategoryLevel",//平台店铺类别配置级别
  GetStoreCategory: HOST +"apiPlat/GetStoreCategory",//平台店铺类别
  GetStoreList: HOST +"apiPlat/GetStoreList",//获取店铺列表 
  GetStoreDetail: HOST +"apiPlat/GetStoreDetail",//获取店铺详情
  GetGoodsList: HOST +"apiPlat/GetGoodsList",//获取当前店铺产品列表
  GetStoreGoods: HOST +"apiPlat/GetStoreGoods",//优选商城
  GetGoodInfo: HOST +"apiPlat/GetGoodInfo",//获取产品详情
  AddGoodsCarData: HOST + 'apiPlat/AddGoodsCarData', //添加购物车数据 
  GetFreightFee: HOST + 'apiPlat/GetFreightFee', //获取运费 
  GetOrderInfo: HOST + 'apiPlat/GetOrderInfo', //获取订单信息
  UpdateOrderState: HOST + 'apiPlat/UpdateOrderState', //修改订单状态
  GetOrderList: HOST + 'apiPlat/GetPlatOrderList', //获取订单列表  
       
  /*****************收货地址*****************/
  GetUserAddress: HOST + 'Inherit/GetUserAddress', //获取用户地址列表
  EditUserAddress: HOST + 'Inherit/EditUserAddress', //编辑收货地址
  ChangeUserAddressState: HOST + 'Inherit/changeUserAddressState', //设置默认
  DeleteUserAddress: HOST + 'Inherit/DeleteUserAddress', //删除地址

  GetStoreCodeImg: HOST +"apiPlat/GetStoreCodeImg",//获取独立小程序二维码
  GetMsgTypeList: HOST + "/apiPlat/GetMsgTypeList",//获取信息分类
  GetMsgByUserId: HOST + "apiPlat/GetMsgByUserId",//获取我的信息列表
  GetMsgList: HOST + "apiPlat/GetMsgList", //获取信息列表 
  GetMsgDetail: HOST + "apiPlat/getMsgDetail",//获取帖子详情  
  GetRuleList: HOST + "apiPlat/GetRuleList",//帖子置顶规则
  AddPayOrder: HOST + "apiPlat/AddPayOrder",//发布帖子
  PayOrder: HOST + "apiMiappEnterprise/PayOrder", // 微信支付订单
  DelMyFavoriteOrMyMsg: HOST + "apiPlat/DelMyFavoriteOrMyMsg", //删除帖子
  GetListMyFavoriteMsg: HOST + "apiPlat/GetListMyFavoriteMsg", //获取用户收藏的帖子信息列表
  GetCityReviewSetting: HOST + "apiPlat/GetCityReviewSetting",//获取帖子审核规则
  AddComment: HOST + "apiPlat/AddComment",//发表评论
  GetMsgComment: HOST + "apiPlat/GetMsgComment",//获取评论列表
  DeleteMsgComment:HOST +"apiPlat/DeleteMsgComment",//删除评论
  GetOtherFavoriteList: HOST + "/apiPlat/GetOtherFavoriteList",//获取点赞列表
  AddFavorite: HOST + "apiPlat/AddFavorite", //更新帖子/名片/评论/商品的 浏览量 收藏数 分享数量
  CheckUserLoginNoappsr: HOST + 'apiPlat/CheckUserLoginNoappsr', //手机登入解密
  senduserauth: HOST + 'apiPlat/senduserauth', //发生验证码
  Submitauth: HOST + 'apiPlat/Submitauth', //提交验证
  AddMyCard: HOST + 'apiPlat/AddMyCard', //编辑名片
  GetMyCard: HOST + 'apiPlat/GetMyCard', //获取名片  
  GetOtherFavoriteList: HOST + 'apiPlat/GetOtherFavoriteList', //获取我的动态内容
  GetIndustryList: HOST + 'apiPlat/GetIndustryList', //行业列表
  loginByThirdPlatform: HOST + 'apiMiappEnterprise/CheckUserLoginNoappsr',
  Getaid: HOST + "apiMiappEnterprise/Getaid", // 根据小程序的appid查询aid ,
  GetConnectionsList: HOST + 'apiPlat/GetConnectionsList',//获取人脉圈数据,
  GetAreaList: HOST + 'apiPlat/GetAreaList',//获取地区列表,
  RichText: HOST + 'webview/PlatStoreDescriptionRichText', //店铺介绍富文本编辑
  AddStore: HOST + 'apiPlat/AddStore', //添加商铺
  GetActivityLog: HOST + 'apiPlat/GetActivityLog', //获得活动轨迹 
  GetRadarData: HOST + 'apiPlat/GetRadarData', //获取数据雷达 
  GetMyCardCodeUrl: HOST + 'apiPlat/GetMyCardCodeUrl', //获取名片码
  ApplyStoreApp: HOST + 'apiPlat/ApplyStoreApp', //申请小程序
  GetMyCouponList: HOST + 'apiMiniAppCoupons/GetMyCouponList', //获取优惠卷
  GetStoreCouponList: HOST + '/apiMiniAppCoupons/GetStoreCouponList', //获取店铺优惠卷
  GetCoupon: HOST + 'apiMiniAppCoupons/GetCoupon', //领取优惠卷
  GetStoreCodeImg: HOST + 'apiPlat/GetStoreCodeImg', //获取店铺二维码
  /*************私信系列*****************/
  GetContactList: HOST + "apiim/GetProContactList", //获取联系人列表
  AddContact: HOST + "apiim/AddProContact", //添加联系人
  GetHistory: HOST + "apiim/GetHistory", //获取历史消息
  /*********************验证名片是否违规*************************/
  CheckCardState: HOST + '/apiPlat/CheckCardState'
} 

export default addr
