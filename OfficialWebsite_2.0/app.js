//app.js
var aldstat = require("./utils/san.js");
var addr = require("/utils/address.js")
App({
	onLaunch: function () {
		//调用API从本地缓存中获取数据
		//第三方平台配置
		var exconfig = wx.getExtConfigSync()
		if (exconfig != undefined) {
			this.globalData.appid = exconfig.appid
		}
		try {
			var res = wx.getSystemInfoSync()
			console.log(res.model)
			console.log(res.pixelRatio)
			console.log(res.windowWidth)
			console.log(res.windowHeight)
			this.globalData.windowWidth = res.windowWidth
			this.globalData.windowHeight = res.windowHeight
			this.globalData.pixelRatio = res.pixelRatio
		} catch (e) {
			// Do something when catch error
		}
	},
	globalData: {
		userInfo: null,
		title: "",
		// address: 'https://txiaowei.vzan.com/apiMiapp/GetModelData',  // 参数appid,level
		// dynamicDetail: 'https://txiaowei.vzan.com/apiMiapp/GetModelInfoById', // 参数id
		address: 'https://cityapi.vzan.com/apiMiapp/GetModelData',  // 参数appid,level
		dynamicDetail: 'https://cityapi.vzan.com/apiMiapp/GetModelInfoById', // 参数id
		// appid: 'wx61575c2a72a69def', 
		appid: 'wxc5d5d4fa13b0d27f',
	},
	IsShowBottomLogo: function (e) {
		var that = this
		wx.request({
			url: addr.Address.GET_BOTTOM_LOGO,
			data: {
				appid: that.globalData.appid,
			},
			method: "GET",
			header: {
				'content-type': "application/json"
			},
			//下拉刷新 
			success: function (res) {
				if (res.data.isok == 1) {
					if (res.data.AgentConfig.isdefaul == 0) {
						res.data.AgentConfig.LogoText = res.data.AgentConfig.LogoText.split(' ')
					} else {
						res.data.AgentConfig.LogoText = res.data.AgentConfig.LogoText
					}
					e.setData({
						AgentConfig: res.data.AgentConfig
					})
					console.log(res.data)
				}
			},
			fail: function (e) {
				console.log(e)
			},
			complete: function () {
			}
		})
	},
})