//app.js
var aldstat = require("./utils/san.js");
var util = require("./utils/util.js");
var api = require("./utils/network.js");
var addr = require("./utils/addr.js");
var tools = require("./utils/tools.js");
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
			that.globalData.areaCode = exconfig.areaCode
		}
		//var logs = wx.getStorageSync('logs') || []
		//logs.unshift(Date.now())
		//wx.setStorageSync('logs', logs)
		// wx.getUserInfo({
		//   success: function (res) {
		//     that.globalData.user = res
		//   }
		// })
	},

	globalData: {
		// 客户上传手机号码准备
		isgetTel: 0,
		telEncryptedData: "",
		telIv: "",


		canSaveMoneyFunction: false,//储值开关
		shareConfig: [],//分享设置集合 用于share.wxml
		newparam: [],//正常商品余额转微信支付的订单数据
		orderTwiceId: 0,//正常商品余额转微信支付的订单号
		myMoeny: 10,//储值余额
		windowWidth: 0,//屏幕宽度
		windowHeight: 0,//屏幕高度（去除标题栏）
		sumprice: 0,//余额支付成功后跳转的支付成功页面的价格
		payType: 1,//支付类型 1是微信支付 2是余额支付
		levelid: 0,//商家会员等级
		userId: 0,
		// 一键分享
		// onekeyshare:[],
		dbOrder: 0, // 订单id，paysuccess页面用
		// typeid分类
		id: 0,
		typeid: -1,
		change: 1,
		user: [],
		userInfo: { IsValidTelePhone: 0, TelePhone: "未绑定" },
		isLogin: false,
		citysubid: 0,
		// 微信配置信息
		session_key: '',
		openid: '',
		appid: "wx9cb1d8be83da075b",
		areaCode: 110228,

		// 配置
		isDebug: false,
		pageSize: 10, //

		// content:name,     
		// 微赞平台信息
		uid: '',
		platformUserInfo: '',
		//店铺客服电话
		customerphone: "",
		cityname: '',
		// 同城信息
		//用户的名字和用户的图像
	},
	WeToast, // 后面可以通过app.WeToast访问
	imgresouces,
	C_Enum,

	getUserInfo: function (cb) {
		console.log('getUserInfo执行')
		var that = this
		var _userInfo = wx.getStorageSync('userInfo')
		if (_userInfo && _userInfo.openId) {
			that.globalData.userInfo = _userInfo;
			console.log('getUserInfo执行回调')
			typeof cb == "function" && cb(_userInfo)
		} else {
			//调用登录接口
			wx.login({
				success: function (res) {
					console.log('login res.code ' + res.code)
					tools.WxLogin(
						that.globalData.appid,
						res.code,
					).then(function (result) {
						if (result.code == 1 && result.dataObj) {

							console.log(result);
							that.globalData.userInfo = {
								UserId: result.dataObj.Id,
								avatarUrl: result.dataObj.HeadImgUrl,
								city: "",
								country: "",
								gender: result.dataObj.Sex,
								language: "",
								nickName: result.dataObj.NickName,
								openId: result.dataObj.OpenId,
								province: "",
								sessionId: "",
								unionId: result.dataObj.UnionId,
								TelePhone: (result.dataObj.TelePhone == null || result.dataObj.TelePhone == '' ? "未绑定" : result.dataObj.TelePhone),
								IsValidTelePhone: result.dataObj.IsValidTelePhone,
							};
							wx.setStorage({
								key: "userInfo",
								data: that.globalData.userInfo
							})

							typeof cb == "function" && cb(that.globalData.userInfo)

						}
						else {
							return "";
						}

					});
				},
			})
		}
	},
	getPhoneNumber: function (cb) {
		var that = this;
		wx.login({
			success: function (res) {
				if (that.globalData.telEncryptedData && that.globalData.telIv) {
					that.login(res.code, that.globalData.telEncryptedData, that.globalData.userInfo.UserId, that.globalData.telIv, cb, 1)
				}
			}
		})

	},
	//登录
	login: function (code, encryptedData, signature, iv, cb, isphonedata) {
		wx.showToast({
			title: '加载中',
			icon: 'loading',
			duration: 10000
		})
		var that = this;
		console.log('准备调用loginByThirdPlatform接口')
		wx.request({
			url: addr.Address.loginByThirdPlatform,
			data: {
				code: code,
				data: encryptedData,
				signature: signature,
				iv: iv,
				appid: that.globalData.appid,
				isphonedata: isphonedata,
			},
			method: "Get",
			success: function (data) {
				if (data.data.result) {
					var json = data.data.obj
					console.log('loginByThirdPlatform接口返回 unionId - ' + json.unionId)
					that.globalData.userInfo = {
						UserId: json.userid,
						avatarUrl: json.avatarUrl,
						city: json.city,
						country: json.country,
						gender: json.gender,
						language: json.language,
						nickName: json.nickName,
						openId: json.openId,
						province: json.province,
						sessionId: json.sessionId,
						unionId: json.unionId,
						TelePhone: (json.tel == null || json.tel == '' ? "未绑定" : json.tel),

						IsValidTelePhone: json.IsValidTelePhone,
					};
					wx.setStorage({
						key: "userInfo",
						data: that.globalData.userInfo
					})

					console.log('json = ' + json);
					wx.hideToast()

					typeof cb == "function" && cb(that.globalData.userInfo)
				} else {
					console.log('登录失败 data - ' + data.data)
					wx.showModal({
						title: '提示',
						content: data.data.msg,
					})
					wx.hideToast()
				}
			},
			fail: function (data) {
				console.log(data)
			},
			complete: function (data) {
				console.log(data)
			}
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
	ShowConfirm: function (msg, callfunction) {
		var that = this
		wx.showModal({
			title: '提示',
			content: msg,
			success: function (e) {
				if (e.confirm) {
					callfunction(e)
				}
			}
		})
	},
	showToast: function (msg) {
		wx.showToast({
			title: msg,
		})
	},
	showLoading: function (msg) {
		wx.showLoading({
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
wx.getSystemInfo({
	success: function (res) {
		getApp().globalData.windowWidth = res.windowWidth
		getApp().globalData.windowHeight = res.windowHeight
	}
})