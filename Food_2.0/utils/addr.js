const HOST = "http://testwtApi.vzan.com/";
//const HOST = "https://wtApi.vzan.com/";
function Address() {

}

// Address.loginByThirdPlatform = "http://txiaowei.vzan.com/apiQuery/CheckUserLogin"; //登陆微赞后台


Address.flowStatistics = "http://vzan.cc/p/i";//流量统计的URL
Address.otherInvitation = "http://vzan.cc/liveajax/app_guest_topics";//邀请直播列表
Address.uploadUrl = "http://www.vzan.cc/wap/attachment";//上传文件
Address.createMinisnsByApp = "http://www.vzan.com/wap/AddMinisnsByApp";//申请论坛
Address.getMiniSnsListByApp = "http://www.vzan.com/Wap/GetMiniSnsListByApp";//论坛列表


Address.loginByThirdPlatform = HOST + "apiMiappStores/CheckUserLoginNoappsr"; //登陆微赞后台
Address.getStoreList = HOST + "apiQuery/GetStoreList"; //首页获取店铺列表

Address.DeleteStore = HOST + "apiQuery/DeleteStore";  // 删除商铺接口(参数:storeid，areaid)
Address.uploadImage = HOST + "apiQuery/uploadImageFromPost";  // 上传文件
Address.GetStorecoupon = HOST + "apiQuery/GetStorecoupon";  // 优惠券查询 (参数：unionid，storeid，pageindex，type:优惠券类型  1代金券 0是优惠券)
Address.GetStoreHalfOff = HOST + "apiQuery/GetStoreHalfOff";  // 五折优惠卡查询 (参数：unionid，storeid，pageIndex，)
Address.GetStoreStrategy = HOST + "apiQuery/GetStoreStrategy";  // 获取店铺攻略(参数：pageIndex，storeId)
Address.GetStrategyArticle = HOST + "apiQuery/GetStrategyArticle";  // 获取店铺攻略详情(参数：strategyId)
Address.DeleteStrategy = HOST + "apiQuery/DeleteStrategy";  // 删除攻略(参数：strategyId，storeid)
Address.GetStoreClass = HOST + "apiQuery/GetStoreClass";  // 店铺分类(参数：id（父级ID）)
Address.storeInviteGen = HOST + "apiQuery/storeInviteGen";  // 生成店铺转让邀请密匙(参数：unionid storeid)
Address.storeinvited = HOST + "apiQuery/storeinvited";  // 验证店铺转让密匙(参数：areaid，unionid，timestamp，invitekey，storeid)
Address.storeaccept = HOST + "apiQuery/storeaccept";  // 确认转让店铺接口(参数：areaid，unionId，inviteUnionid，timestamp，invitekey，storeid（unionId-转让用户unionId，inviteUnionid邀请用户unionId）)
//minisnsId（论坛ID），unionid，articleId（仅修改时），strategyId（仅修改时），storeId，AreaCode，title，txtContent,choosedType(选中版块ID)，ImgurlList（图片地址集合）,lat(经度)，lng（纬度），speed（移动速度），address(地址),acy（是否检查优酷地址, 100: 问答免费看答案），top_money（置顶费用），all_money（发帖费用），zd_mode（1全站置顶 2板块置顶）
Address.AddStoreStrategy = HOST + "apiQuery/AddStoreStrategy";
// 添加代金券
Address.AddStoreCoupon = HOST + "apiQuery/AddStoreCoupon";
// 添加五折优惠卡
Address.AddHaltOff = HOST + "apiQuery/AddHaltOff";
// 店铺版块
Address.GetArtType = HOST + "apiQuery/GetArtType";
// 生成邀请核销员的token 参数（areaid，storeid）
Address.InviteClerksGen = HOST + "apiQuery/InviteClerksGen";
// 验证key （areaId，unionid，inviteUnionid, storeid，timestamp，invitekey）
Address.InvitedClerksCheck = HOST + "apiQuery/InvitedClerksCheck";
// 同意成为核销员 （id，unionid，inviteUnionid, storeid，timestamp，invitekey）//id 为areaid
Address.InviteClerksAccept = HOST + "apiQuery/InviteClerksAccept";
// 获取核销员列表 （unionid，storeId）
Address.GetClerkList = HOST + "apiQuery/GetClerkList";
// 修改店铺
Address.UpdateStore = HOST + "apiQuery/UpdateStore";

// 获取店铺分类
Address.getStreet = HOST + "apiQuery/getStreet";
// 获取地区
Address.getArea = HOST + "apiQuery/getArea";
// 删除图片
Address.deleteImage = HOST + "apiQuery/DeleteStoreImage";
//获取店铺详情 GetStoreDetail StoreID:店铺ID unionid
Address.getStoreDetail = HOST + "apiQuery/getStoreDetail";
//获取店铺商品
Address.Getstoregoodslist = HOST + "apiQuery/Getstoregoodslist";
//获取店铺动态
Address.GetStrategylist = HOST + "apiQuery/GetStrategylist";
//获取店铺招牌信息
Address.Getstorerecruit = HOST + "apiQuery/Getstorerecruit";
//获取店铺拼团
Address.GetStoreGroup = HOST + "apiQuery/GetStoreGroup";
//获取店铺同诚卡
Address.GetStoreHalfs = HOST + "apiQuery/GetStoreHalfs";




//(添加评论) unionid comment（评论数据）JSON格式：{"Id": int,  "StoreId": int, "Content": string, "ParentId":int} 
Address.AddStoreComment = HOST + "apiQuery/AddStoreComment";
//（获取店铺评论）unionid -String- id (店铺ID) –int- pageIndex –int- pageSize –int-  get
Address.GetStoreComments = HOST + "apiQuery/GetStoreComment";
// （删除评论）POST unionid id (评论ID) –int-
Address.DeleteComment = HOST + "apiQuery/DeleteComment";
//获取最多100条子评论
Address.GetMoreSubComment = HOST + "apiQuery/GetMoreSubComment";
// AddComplaint  unionid -String- complaint (举报数据) JSON格式：{"StoreId": int, "Content": string }
Address.AddComplaint = HOST + "apiQuery/AddComplaint";
//CheckStoreConcern （检查关注）参数：unionid，storeid
Address.CheckStoreConcern = HOST + "apiQuery/CheckStoreConcern";
//AddStoreConcern（关注\取消关注） 参数：unionid，storeid
Address.AddStoreConcern = HOST + "apiQuery/AddStoreConcern";
//GetNearStore  （获取附件商铺）-GET 参数：storeid,unionid
Address.GetNearStore = HOST + "apiQuery/GetNearStore";

//GetPayInfo (店铺VIP列表)itemid -int-（店铺ID）paytype-int-（支付类型：店铺VIP为204）unonid-string
Address.GetPayInfo = HOST + "apiQuery/GetPayInfo";
//AddPayOrder 
Address.AddPayOrder = HOST + "apiQuery/AddPayOrder";
//优惠券退款
Address.RefundCoupon = HOST + "apiQuery/RefundCoupon";

// PayOrder openid
Address.PayOrder = HOST + "apiQuery/PayOrder";

// 我的优惠券列表
Address.GetHomeCouponList = HOST + "apiQuery/GetHomeCouponList";

// 优惠券详情
Address.GetCouponDetail = HOST + "apiQuery/GetAscdetail";

//获取首页轮播图
Address.CarouselList = HOST + "apiQuery/GetHomeCarouselList";
//
//获取首页本地商家的数据
Address.localStore = HOST + "apiQuery/GetHomeStoreList";

//获取首页抢优惠的数据
Address.getDis = HOST + "apiQuery/GetHomeCouponList";

//获取城市列表
Address.getLocation = HOST + "apiQuery/GetCityList";
//获取用户地理位置
Address.GetCurrentCityList = HOST + "apiQuery/GetCurrentCityList";

//没有同城编码则根据经纬度获取所在同城城市名或区县名
Address.GetCityAreaName = HOST + "apiQuery/GetCityAreaName";

//获取根据用户输入的城市名来对当前城市同城进行一个查询
Address.GetFindCityInfo = HOST + 'apiQuery/GetFindCityList';//GetFindCityInfo

//测试用户Unionid
//Address.Unionid='oW2wBwXbX3lXBHRqQubA8sq-Gkcc';

//免费领取优惠券
Address.BuyCoupon = HOST + "apiQuery/BuyCoupon";


//获取我的优惠券
Address.GetUserCouponList = HOST + "apiQuery/GetUserCouponList";//GetUserCoupon

//获取用户购买优惠券详情
Address.GetCoupdetail = HOST + "apiQuery/GetCoupdetail";
//获取优惠券二维码图片
Address.Gqrcode = HOST + "apiQuery/Gqrcode";
//检查优惠券二维码是否已被扫描
Address.CheckCouponUsed = HOST + "apiQuery/CheckCouponUsed"
//优惠券订单详情
Address.GetMucqrCode = HOST + "apiQuery/GetMucqrCode"
//根据arreacode查找同城
Address.GetCityInfo = HOST + "apiQuery/GetCityInfo"
//优惠券下单
Address.AddPayCouponOrder = HOST + "apiQuery/AddPayCouponOrder"
//发送验证码
Address.Senduserauth = HOST + "apiQuery/Senduserauth"
//提交验证
Address.Submitauth = HOST + "apiQuery/Submitauth"

////////////////////////////--微赞同城信息小程序接口
//首页
Address.GetHomeInfoData = HOST + "apiQuery/GetHomeInfoData"
//热门推荐
Address.getpushpost = HOST + "apiQuery/getpushpost"
//帖子列表
Address.GetPostList = HOST + "apiQuery/GetPostList"
//加载拼车列表数据
Address.getcarpoolbottom = HOST + "apiQuery/getcarpoolbottom"
//加载列表数据
Address.gettplbottom = HOST + "apiQuery/gettplbottom"
//帖子详情
Address.GetPostDetail = HOST + "apiQuery/GetPostDetail"
//获取评论
Address.GetComment = HOST + "apiQuery/GetComment"
//添加帖子评论
Address.AddComment = HOST + "apiQuery/AddComment"
//举报发帖
Address.addcomplaints = HOST + "apiQuery/addcomplaints"
//获取帖子分类
Address.getposttypeconfig = HOST + "apiQuery/getposttypeconfig"
//修改个人中心
Address.UpdateUserInfo = HOST + "apiQuery/UpdateUserInfo"
//获取我的发布
Address.GetMyPublish = HOST + "apiQuery/GetMyPublish"
//继续付款
Address.continuepostpay = HOST + "apiQuery/continuepostpay"
//根据区域id获取发帖付费信息列表
Address.getchargetypeinfolistpaytype = HOST + "apiQuery/getchargetypeinfolistpaytype"
//发帖
Address.publish = HOST + "apiQuery/publish"
//添加帖子
Address.addpost = HOST + "apiQuery/addpost"
//删除帖子
Address.delpost = HOST + "apiQuery/delpost"
//修改已付费帖子状态
Address.passpost = HOST + "apiQuery/passpost"
//刷新帖子状态
Address.refreshpost = HOST + "apiQuery/refreshpost"
//获取广告图
Address.GetImg = HOST + "apiQuery/GetImg"

// 获取分类页面信息
Address.get_post_select_item = HOST + "apiQuery/get_post_select_item"

// 点餐接口

Address.GetFoodsDetail = HOST + "apiMiappFoods/GetFoodsDetail"// 店铺信息

Address.GetGoodsTypeList = HOST + "apiMiappFoods/GetGoodsTypeList"// 首页商品分类信息

Address.GetGoodsList = HOST + "apiMiappFoods/GetGoodsList"//首页商品信息

Address.GetGoodsDtl = HOST + "apiMiappFoods/GetGoodsDtl"// 获取商品详情

Address.getGoodsCarData = HOST + "apiMiappFoods/getGoodsCarData"// 获取购物车列表

Address.GetMyAddress = HOST + "apiMiappFoods/GetMyAddress"// 获取我的收货地址

Address.AddOrEditMyAddressDefault = HOST + "apiMiappFoods/AddOrEditMyAddressDefault"// 新增收货地址

Address.getMiniappGoodsOrder = HOST + "apiMiappFoods/getMiniappGoodsOrder"// 获取我的订单

Address.deleteMyAddress = HOST + "apiMiappFoods/deleteMyAddress"// 删除我的收货地址

Address.addGoodsCarData = HOST + "apiMiappFoods/addGoodsCarData"// 提交商品到购物车

// Address.addMiniappGoodsOrder = HOST + "apiMiappFoods/addMiniappGoodsOrder"//购物车生成订单

Address.AddPayOrder_Store = HOST + "apiMiappFoods/addMiniappGoodsOrder"//下单

Address.PayOrder = HOST + "apiMiappFoods/PayOrder"// 支付

Address.updateMiniappGoodsOrderState = HOST + "apiMiappFoods/updateMiniappGoodsOrderState"// 更新订单状态

Address.getMiniappGoodsOrderById = HOST + "apiMiappFoods/getMiniappGoodsOrderById"// 查看订单

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

module.exports = {
	Address: Address,
}