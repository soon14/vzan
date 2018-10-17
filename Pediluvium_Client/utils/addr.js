const HOST = "https://wtApi.vzan.com/";
// const HOST = "http://testwtApi.vzan.com/";
//const HOST="http://lztapi.vzan.com/";
function Address() {

}

Address.loginByThirdPlatform = HOST + "apiMiappFootbath/CheckUserLoginNoappsr" //登陆微赞后台
Address.WxLogin = HOST + "apiMiappFootbath/WxLogin" //登陆微赞后台
//支付模块
Address.AddPayOrder_Store = HOST + "apimiappfootbath/ReserveService" //足浴的下单接口
Address.AddPayOrder = HOST + "apimiappfootbath/AddPayOrder"
Address.PayOrder = HOST + "apiMiappEnterprise/PayOrder"

//储值模块
Address.getSaveMoneySetList = HOST + "apiMiappSaveMoney/getSaveMoneySetList"//获取充值列表
Address.getSaveMoneySetUser = HOST + "apiMiappSaveMoney/getSaveMoneySetUser"//获取储值金额
Address.addSaveMoneySet = HOST + "apiMiappSaveMoney/addSaveMoneySet"//充值
Address.getSaveMoneySetUserLogList = HOST + "apiMiappSaveMoney/getSaveMoneySetUserLogList"//消费记录

// 会员卡模块
Address.GetWxCardCode = HOST + "apiMiappStores/GetWxCardCode?type=3"//会员卡请求
Address.GetCardSign = HOST + "apiMiappStores/GetCardSign?type=3"//获取会员卡Sign(签名)
Address.SaveWxCardCode = HOST + "apiMiappStores/SaveWxCardCode?type=3"//保存code提交
Address.UpdateWxCard = HOST + "apiMiappStores/UpdateWxCard?type=3"//更新会员卡

//提交虚拟formId
Address.commitFormId = HOST + "apiMiappStores/commitFormId"

Address.CancelPay = HOST + "apiMiappfootbath/CancelPay"// 取消支付回调

Address.GetVipInfo = HOST + "apiMiappfootbath/GetVipInfo"// 是否显示水印

Address.GetAgentConfigInfo = HOST + "apiMiappPage/GetAgentConfigInfo"// 是否显示水印

Address.GetStoreInfo = HOST + "apimiappfootbath/GetStoreInfo"// 店铺信息

Address.GetTechnicianList = HOST + "apimiappfootbath/GetTechnicianList"// 技师列表

Address.GetTechnicianInfo = HOST + "apimiappfootbath/GetTechnicianInfo"// 技师详情

Address.GetServiceList = HOST + "apimiappfootbath/GetServiceList"// 项目列表

Address.GetServiceInfo = HOST + "apimiappfootbath/GetServiceInfo"// 项目详情

Address.GetDateTable = HOST + "apimiappfootbath/GetDateTable"// 根据技师id以及日期查询可预约时间列表

Address.GetOrderRecord = HOST + "apiMiappfootbath/GetOrderRecord"// 订单列表

Address.payGift = HOST + "apiMiappfootbath/payGift"// 送花下单

Address.GiftList = HOST + "apiMiappfootbath/GiftList"// 送花记录

//im
Address.GetContactList = HOST + "apiim/GetContactList" //获取联系人列表
Address.AddContact = HOST + "apiim/AddContact" //添加联系人
Address.GetHistory = HOST + "apiim/GetHistory" //获取历史消息
Address.Upload = HOST + "apiim/Upload" //上传

//'我也要做小程序' 
Address.GetVaildCode = HOST + "webview/GetVaildCode"//获取验证码
Address.SaveUserInfo = HOST + "webview/SaveUserInfo"//注册账号


module.exports = {
	Address: Address,
}