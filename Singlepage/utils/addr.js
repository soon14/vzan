const HOST = "https://wtApi.vzan.com/";
//  const HOST = "http://testwtApi.vzan.com/";

function Address() { }
Address.WebViewURL ="http://www.xiaochengxu.com.cn/mobile/mobileReg",
Address.WxLogin = HOST + "apiMiappEnterprise/WxLogin" //微赞第三方平台登录
Address.GetPageMsg = HOST + "apiMiappPage/GetPageMsg"//获取第二版接口
Address.SetForm = HOST + "apiMiappPage/SetForm"// 提交表单
Address.GetAgentConfigInfo = HOST + "apiMiappPage/GetAgentConfigInfo"// 水印
Address.GetVaildCode = HOST + "webview/GetVaildCode"//获取验证码
Address.SaveUserInfo = HOST + "webview/SaveUserInfo"//注册账号

module.exports = { Address: Address, }