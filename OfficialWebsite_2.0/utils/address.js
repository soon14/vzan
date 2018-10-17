
var IS_DEBUG = false
var DOMAIN = ''

if (IS_DEBUG) {
	// DOMAIN = "https://txiaowei.vzan.com/"
	DOMAIN = "http://testwtApi.vzan.com/"
}
else {
	// DOMAIN = "https://cityapi.vzan.com/"
	DOMAIN = "https://wtApi.vzan.com/"
	// DOMAIN = "http://testwtApi.vzan.com/"
}

var Address = {
	GET_MODEL_DATA: DOMAIN + 'apiMiapp/GetModelData',  // 参数appid,level
	GET_MODEl_INFO_BY_ID: DOMAIN + 'apiMiapp/GetModelInfoById', // 参数id
	GET_MODEl_IMG: DOMAIN + 'apiMiapp/GetImg', // 参数id
	GET_BOTTOM_LOGO: DOMAIN + 'apiMiapp/GetAgentConfigInfo', // 底部logo
	GetVaildCode: DOMAIN + "webview/GetVaildCode",//获取验证码
	SaveUserInfo: DOMAIN + "webview/SaveUserInfo",//注册账号
}

module.exports = {
	Address: Address
} 