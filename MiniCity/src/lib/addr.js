//const HOST = "http://testwtapi.vzan.com/";
const HOST = "https://wtApi.vzan.com/";//正式
var addr = {
  loginByThirdPlatform: HOST + "apiMiappEnterprise/CheckUserLoginNoappsr", //登陆微赞后台
  WxLogin: HOST + "apiMiappEnterprise/WxLogin",
  GetAgentConfigInfo: HOST + "apiMiappEnterprise/GetAgentConfigInfo", //水印
  Getaid: HOST + "apiMiappEnterprise/Getaid", // 根据小程序的appid查询aid
  GetVipInfo: HOST + "apiMiappEnterprise/GetVipInfo", // 获取会员信息
  Upload : HOST + "apiim/Upload", //上传
  PayOrder: HOST + "apiMiappEnterprise/PayOrder",
  ApiCity:HOST+"apiCity/getCitySetting",
  GetMsgList:HOST+"apiCity/getMsgList",
  AddPayOrder:HOST+"apiCity/AddPayOrder",
  GetRuleList:HOST+"apiCity/getRuleList",
  GetMsgDetail:HOST+"apiCity/getMsgDetail", 
  AddReportMsg:HOST+"apiCity/AddReportMsg",
  AddMsgViewFavoriteShare:HOST+"apiCity/AddMsgViewFavoriteShare", 
  GetCityUserMsgList:HOST+"apiCity/getCityUserMsgList",
  GetUnReadUserMsgCount:HOST+"apiCity/getUnReadUserMsgCount",
  GetMsgCode:HOST+"apiCity/getMsgCode",
  SaveUserPhone:HOST+"apiCity/saveUserPhone",
  GetMsgByUserId:HOST+"apiCity/getMsgByUserId", 
  GetListMyFavoriteMsg:HOST+"apiCity/getListMyFavoriteMsg",
  DelMyFavoriteOrMyMsg:HOST+"apiCity/delMyFavoriteOrMyMsg",
  SaveCityStoreUser:HOST+"apiCity/SaveCityStoreUser",
  GetCityStoreUserPhone:HOST+"apiCity/GetCityStoreUserPhone",
  GetCityReviewSetting:HOST+"apiCity/getCityReviewSetting",
  GetVaildCode:HOST+"webview/GetVaildCode",//获取验证码
  SaveUserInfo:HOST+"webview/SaveUserInfo",//注册账号
  AddComment:HOST+"apiCity/AddComment",
  GetMsgComment:HOST+"apiCity/GetMsgComment",
  DeleteMsgComment:HOST+"apiCity/DeleteMsgComment",
};

export default addr;
