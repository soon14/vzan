const HOST = "https://wtApi.vzan.com/";
// const HOST = "http://testwtApi.vzan.com/";

function Address() {

}
Address.WxLogin = HOST + "apiMiappEnterprise/WxLogin" //微赞第三方平台登录
Address.loginByThirdPlatform = HOST + "apiSinglePage/CheckUserLoginNoappsr" //登陆微赞后台
Address.GetAgentConfigInfo = HOST + "apiMiappPage/GetAgentConfigInfo"// 是否显示水印
Address.Upload = HOST + "apiWorkShop/Upload"// 上传图片
Address.SendUserAuthCode = HOST + "apiMiappFootbath/SendUserAuthCode"// 获取验证码
Address.BindPhoneNumber = HOST + "apiMiappFootbath/BindPhoneNumber"// 绑定手机号码
Address.GetUserInfo = HOST + "apiMiappFootbath/GetUserInfo"// 获取用户信息
Address.GetGiftsOrderDescList = HOST + "apiMiappFootbath/GetGiftsOrderDescList"// 送花排行榜
Address.GetMyGiftsCount = HOST + "apiMiappFootbath/GetMyGiftsCount"// 送花总数据
Address.GetMyGifts = HOST + "apiMiappFootbath/GetMyGifts"// 我收到的花
Address.ChangeTechnicianState = HOST + "apiMiappFootbath/ChangeTechnicianState"// 修改技师状态
Address.SaveUserInfo = HOST + "apiMiappFootbath/SaveUserInfo"// 修改技师状态
Address.GetMyOrder = HOST + "apiMiappFootbath/GetMyOrder"// 订单查询
Address.GetMyOrderCount = HOST + "apiMiappFootbath/GetMyOrderCount"// 订单总数据
Address.UpdateOrderState = HOST + "apiMiappFootbath/UpdateOrderState"// 订单状态修改
//im
Address.GetContactList = HOST + "apiim/GetContactList" //获取联系人列表
Address.AddContact = HOST + "apiim/AddContact" //添加联系人
Address.GetHistory = HOST + "apiim/GetHistory" //获取历史消息
Address.Upload = HOST + "apiim/Upload" //上传
Address.GetTechInfo = HOST + "apiMiappFootbath/TechnicianLogin"//获取技师信息

//'我也要做小程序'
Address.GetVaildCode = HOST + "webview/GetVaildCode"//获取验证码
Address.SaveUserInfo = HOST + "webview/SaveUserInfo"//注册账号
module.exports = {
	Address: Address,
}