//app.js
var aldstat = require("./utils/san.js");
var util = require("./utils/util.js");
var api = require("./utils/network.js");
var addr = require("./utils/addr.js");
var imgresouces = require("./utils/imgresouces.js");
var C_Enum = require("./public/C_Enum.js");
let { WeToast } = require('src/wetoast.js')    // 返回构造函数，变量名可自定义
App({
	onLaunch: function () {
		//调用API从本地缓存中获取数据
		var that = this;

		//第三方平台配置
		var exconfig = wx.getExtConfigSync()
		if (exconfig != undefined) {
			that.globalData.appid = exconfig.appid
		}
		that.login()
	},

	globalData: {
		userInfo: { IsValidTelePhone: 0, TelePhone: "未绑定" }
	},
	WeToast, // 后面可以通过app.WeToast访问
	imgresouces,
	C_Enum,

	onHide: function () {//退出小程序终止背景音乐
		wx.stopBackgroundAudio()
	},
	login: function () {
		var that = this
		wx.login({
			success: function (res) {
				that.WxLogin(res.code, getApp().globalData.appid);
			}
		})
	},
	//新版第三方登录
	WxLogin: function (code, appid) {
		wx.showToast({
			title: '正在登录....',
			icon: 'loading',
			duration: 10000
		})
		var that = this;
		wx.request({
			url: addr.Address.WxLogin,
			data: {
				code: code,
				appid: appid,
				needappsr: 0,
			},
			header: {
				'content-type': 'application/x-www-form-urlencoded'
			},
			method: "POST",
			success: function (data) {
				var data = data.data
				if (data.isok) {
					wx.setStorageSync("userInfo", data.dataObj)
				} else {
					wx.showModal({
						title: '提示',
						content: data.data.Msg,
					})
				} wx.hideToast()
			},
			fail: function (data) {
				console.log(data)
			},
		})
	},
	pictureTap: function (e) {
		var src = e.currentTarget.dataset.src;
		wx.previewImage({
			urls: [src]
		})
	},
	pictureTaps: function (url, urls) {
		wx.previewImage({
			current: url,
			urls: urls,
		})
	},
	ShowMsg: function (msg) {
		wx.showModal({
			title: '提示',
			content: msg,
			showCancel: false,
		})
	},
	ShowMsgAndUrl: function (msg, url, mtype = 0) {
		var that = this
		wx.showModal({
			title: '提示',
			showCancel: false,
			content: msg,
			success: function (res) {
				if (res.confirm) {
					if (mtype == 0) {
						that.goNewPage(url)
					} else if (mtype == 1) {
						that.goBackPage(1)
					}
				}
			}
		})
	},
	showToast: function (msg) {
		wx.showToast({
			title: msg,
		})
	},
	//刷新页面
	reloadpage: function (param = '', index = 0) {
		var pages = getCurrentPages()
		var coupon = pages[index]
		coupon.onLoad(param)
	},
	//刷新页面
	reloadpagebyurl: function (param = '', url = '') {
		var pages = getCurrentPages()
		if (pages.length > 0) {
			for (var i in pages) {
				var page = pages[i]
				if (page.route == url) {
					page.onLoad(param)
					break
				}
			}
		}
	},
	//手机号码是否通过验证
	checkphone: function (param = '') {
		if (this.globalData.userInfo.IsValidTelePhone == 0) {
			if (param != '1') {
				wx: wx.showModal({
					title: '提示',
					content: '为了保障您数据的安全，请先进行手机号验证',
					success: function (res) {
						if (res.confirm) {
							wx.navigateTo({
								url: '../bind_mobile/bind_mobile',
							})
						}
					},
				})
			}
		}
		else {
			return true
		}
		return false
	},
	//跳转新页面
	goNewPage: function (url) {
		wx.navigateTo({
			url: url,
		})
	},
	//跳转选项卡页
	goBarPage: function (url) {
		wx.switchTab({
			url: url,
		})
	},
	//返回上几页
	goBackPage: function (delta) {
		wx.navigateBack({
			delta: delta
		})
	},
})