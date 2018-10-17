//const HOST = "http://testwtapi.vzan.com/";
const HOST = "https://wtApi.vzan.com/";
//const HOST = "http://api-mx.vzan.com/";
var addr = {
  Upload: HOST + "apiWorkShop/Upload",
  loginByThirdPlatform: HOST + "apiMiappEnterprise/CheckUserLoginNoappsr", //登陆微赞后台
  WxLogin: HOST + "apiMiappEnterprise/WxLogin", //新登陆接口
  Getaid: HOST + "apiMiappEnterprise/Getaid", // 根据小程序的appid查询aid
  GetVipInfo: HOST + "apiMiappEnterprise/GetVipInfo", // 获取会员信息
  index: HOST + "apiMiappBussiness/index",
  GetRecordByDate: HOST + "apiMiappBussiness/GetRecordByDate",
  login: HOST + "apiMiappBussiness/Login",
  GetStoreList: HOST + "apiMiappBussiness/GetStoreList",
  SendUserAuthCode: HOST + "apiMiappFootbath/SendUserAuthCode", //获取验证码
  GetOrderCount: HOST + "apiMiappBussiness/GetOrderCount", //获取不同条件下各个类型的订单数量
  GetOrderList: HOST + "apiMiappBussiness/GetOrderList",
  UpdteOrderState: HOST + "apiMiappBussiness/UpdteOrderState",
  GetOrderByTableNo: HOST + "apiMiappBussiness/GetOrderByTableNo",
  GetGoodsTypes: HOST + "apiMiappBussiness/GetGoodsTypes", //获取商品分类
  GetGoodsTypesAll: HOST + "apiMiappBussiness/GetGoodsTypesAll",
  GoodType: HOST + "apiMiappBussiness/GoodType",
  GoodAttr: HOST + "apiMiappBussiness/GoodAttr",
  GetEntGoodsList: HOST + "apiMiappBussiness/GetEntGoodsList", //获取商品列表
  GetEntGoodsInfo: HOST + "apiMiappBussiness/GetEntGoodsInfo", //获取单个产品详情
  SaveEntGoodsInfo: HOST + "apiMiappBussiness/SaveEntGoodsInfo", //保存/修改 产品
  UpdateState: HOST + "apiMiappBussiness/UpdateState",
  GetVipList: HOST + "apiMiappBussiness/GetVipList",
  GetVipLevel: HOST + "apiMiappBussiness/GetVipLevel",
  ChangeSaveMoney: HOST + "apiMiappBussiness/ChangeSaveMoney",
  EditViplevel: HOST + "apiMiappBussiness/EditViplevel",
  GetStoreInfo: HOST + "apiMiappBussiness/GetStoreInfo",
  SaveStoreInfo: HOST + "apiMiappBussiness/SaveStoreInfo",
  GetMessageCount: HOST + "apiMiappBussiness/GetMessageCount",
  ReadMessage: HOST + "apiMiappBussiness/ReadMessage",
  GetMessageList: HOST + "apiMiappBussiness/GetMessageList",
  //私信
  GetContactList: HOST + "apiim/GetProContactList",
  AddContact: HOST + "apiim/AddProContact",
  GetHistory: HOST + "apiim/GetHistory",
  Upload: HOST + "apiim/Upload",
  GetConnectInfo: HOST + "apiim/GetConnectInfo",
  Logout: HOST + "apiMiappBussiness/Logout",
  richtexturl: "https://wtapi.vzan.com/webview/RichText",
  //richtexturl:"http://testwtapi.vzan.com/webview/RichText",
  //产品二维码
  GetProductQrcode: HOST + "apiMiappBussiness/GetProductQrcode",
  getDeliveryCompany: HOST + 'apiMiniAppTools/GetDeliveryCompany',

  //阿拉丁统计
  getAppInfo: HOST + 'api/base/getAppInfo',
};

export default addr;
