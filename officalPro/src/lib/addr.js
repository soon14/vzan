// const HOST = "http://testwtapi.vzan.com/"; //测试
const HOST = "https://wtApi.vzan.com/";//正式/
// const HOST = "http://hyqTestApi.vzan.com/";
// const HOST = "http://apiwx.vzan.com/";


var addr = {
    WxLogin: HOST + "apiMiappEnterprise/WxLogin", //登录微赞后台
    loginByThirdPlatform: HOST + "apiMiappEnterprise/CheckUserLoginNoappsr", //登录微赞后台
    Upload: HOST + "apiim/Upload", //上传
    Getaid: HOST + "apiMiappEnterprise/Getaid", // 根据小程序的appid查询aid
    /*********************产品******************************************* */
    GetGoodsCategoryLevel:HOST+"apiQiye/GetGoodsCategoryLevel",//产品类别配置
    GetGoodsCategory:HOST+"apiQiye/GetGoodsCategory",//获取产品类别
    GetGoodsList:HOST+"apiQiye/GetGoodsList",//产品列表
    GetGoodInfo:HOST+"apiQiye/GetGoodInfo",//产品详情
    /*********************名片******************************************* */
    GetUserIsBind:HOST+"apiQiye/GetUserIsBind",//获取该用户是否绑定员工名片
    BindWorkIDByUserId:HOST+"apiQiye/BindWorkIDByUserId",//将用户通过员工码绑定名片
    JieBindWorkIDByUserId:HOST+"apiQiye/JieBindWorkIDByUserId",//.解除该用户绑定的员工名片
    PostMsg:HOST+"apiQiye/PostMsg",//.发动态
    DelPostMsg:HOST+"apiQiye/DelPostMsg",//.删除我的动态
    GetListPostMsg:HOST+"apiQiye/GetListPostMsg",//.获取动态列表
    GetEmployee:HOST+"apiQiye/GetEmployee",//.获取名片详情
    AddFavorite:HOST+"apiQiye/AddFavorite",//.点赞、关注、浏览、私信、收藏量
    GetDataList:HOST+"apiQiye/GetDataList",//.获取名片数据雷达
    EditEmployee:HOST+"apiQiye/EditEmployee",//.修改编辑名片
    GetEmployeeQrcode:HOST+"apiQiye/GetEmployeeQrcode",//.获取名片码
    GetMyListEmployee:HOST+"apiQiye/GetMyListEmployee",//名片列表
    CustomerBindEmployee:HOST+"apiQiye/CustomerBindEmployee",//客户绑定客服
    GetCustomerList:HOST+"apiQiye/GetCustomerList",//员工获取客户数据列表
    EditeCustomerDesc:HOST+"apiQiye/EditeCustomerDesc",//编辑客户备注
     /*********************订单相关******************************************* */
     AddGoodsCarData:HOST+"apiQiye/AddGoodsCarData",//.添加商品至购物车
     GetGoodsCarList:HOST+"apiQiye/GetGoodsCarList",//.购物车列表
     UpdateOrDeleteGoodsCarData:HOST+"apiQiye/UpdateOrDeleteGoodsCarData",//修改或删除购物车商品
     GetFreightFee:HOST+"apiQiye/GetFreightFee",//获取运费信息
     AddPayOrder:HOST+"apiQiye/AddPayOrder",//生成订单
     GetOrderList:HOST+"apiQiye/GetOrderList",//获取订单信息列表
     GetOrderInfo:HOST+"apiQiye/GetOrderInfo",//.获取订单信息
     UpdateOrderState:HOST+"apiQiye/UpdateOrderState",//修改订单状态
     EditUserAddress: HOST + "apiMiappEnterprise/EditUserAddress", //添加，修改，设置取消默认 收货地址
     GetUserAddress: HOST + "apiMiappEnterprise/GetUserAddress", //查询用户收货地址
     DeleteUserAddress: HOST + "apiMiappEnterprise/DeleteUserAddress", //删除用户地址
     commitFormId: HOST + "apiMiappStores/commitFormId", //提交虚拟formId
     PayOrder: HOST + "apiMiappEnterprise/PayOrder", // 微信支付订单
     GetStoreInfo: HOST + "apiQiye/GetStoreInfo", // 获取店铺信息
     /*************************企业信息***************************************** */
     GetQiyeInfo:HOST+"apiQiye/GetQiyeInfo",//企业信息
     GetCompanyNews:HOST+"apiQiye/GetCompanyNews",//企业资讯
     GetCompanyNewsDetail:HOST+"apiQiye/GetCompanyNewsDetail",//企业资讯详情
     GetDevelopmentDataList:HOST+"apiQiye/GetDevelopmentDataList",//发展历程
    /****************************私信****************************************** */
    GetContactList: HOST + "apiim/GetProContactList", //获取联系人列表
    AddContact: HOST + "apiim/AddProContact", //添加联系人
    GetHistory: HOST + "apiim/GetHistory", //获取历史消息
    GetEmployeeMessage:HOST + "apiQiye/GetEmployeeMessage", //获取客户与员工聊天记录
}

export default addr
