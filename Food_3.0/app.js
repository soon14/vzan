//app.js
var aldstat = require("./utils/san.js");
var util = require("./utils/util.js");
var api = require("./utils/network.js");
var addr = require("./utils/addr.js");
var imgresouces = require("./utils/imgresouces.js");
var C_Enum = require("./public/C_Enum.js");
let { WeToast } = require('src/wetoast.js')    // 返回构造函数，变量名可自定义
App({
	onLaunch: function (options) {
		console.log('onlaunch', options)
		//调用API从本地缓存中获取数据
		var that = this;
		//第三方平台配置
		var exconfig = wx.getExtConfigSync()
		if (exconfig != undefined) {
			that.globalData.appid = exconfig.appid
			that.globalData.areaCode = exconfig.areaCode
		}
		// this.getUserInfo({
		// 	success: function (res) {
		// 		that.globalData.user = res
		// 	}
		// })

	},

	globalData: {
		// 预约点餐
		appointMsg: {
			datatime: '选择就餐日期',
			time: '选择就餐时间',
			numsindex: 0,
			msg: '',
			name: '',
			phonenumber: '',
		},
		appoint_numsArray: ['选择就餐人数'],
		appoint_Id: 0,//预约id
		appoint_paynow: 1,//预约点餐 扫码付款0 预约付款1
		appoint_shopcartlist: [],
		appoint_shopcartlength: 0,
		appoint_alldiscountprice: 0,
		appoint_goodslist: [],
		//预约点餐 end

		reduction: {},//立减金对象
		// 客户上传手机号码准备
		isgetTel: 0,
		telEncryptedData: 0,
		telIv: 0,

		storeConfig: [],//店铺配置对象
		storeAddress: {},//店铺地址 经纬度
		aid: 0, storeId: 0,
		canSaveMoneyFunction: false,//储值开关
		isOrderlist: 0,//用做卡订单页确认支付又取消得操作
		AccountMoneyStr: 0,//储值余额 用于判断默认支付方式
		payType: 1,//默认1是微信支付 2是储值支付
		levelid: 0,//会员等级
		shareConfig: [],//分享设置 用于 util/canvas.js
		isclearItem5: 0,
		ispayOk: 0,
		dizhiId: '',//地址id
		TablesNo: -999,//桌台号(外卖==-999)
		orderid: 0,//citymodelid 用作二次付款
		TelePhone: 0,//商家联系号码
		addressInfo: '未定位',//定位详细位置
		weidu: 0,//纬度
		jingdu: 0,//经度
		authMark: false, //用户开始是不是授权地理位置获取
		address: "", //用户的地址
		DistributionWay: 1, //商家配送为1，达达配送为2
		distributionprice: 0, //配送费，单位是分
		ShippingFeeStr: '',//配送费
		OutSideStr: 0,//起送价
		FoodsName: "",//商店名字
		logoimg: "",//商店头像
		TheShop: 99,//堂食判断
		TakeOut: 99,//外卖判断
		dbOrder: 0,// 订单id，paysuccess页面用
		// typeid分类
		id: 0,
		typeid: 0,
		change: 1,
		user: [],
		//userInfo: {unionId:"oW2wBwXbX3lXBHRqQubA8sq-Gkcc"},
		userInfo: { IsValidTelePhone: 0, TelePhone: "未绑定" },
		isLogin: false,
		citysubid: 0,
		// 微信配置信息
		session_key: '',
		openid: '',
		appsr: "",
		appid: "",
		isDebug: false,
		uid: '',
		platformUserInfo: '',
		//店铺客服电话
		customerphone: "",
		cityname: '广州',
	},
	WeToast, // 后面可以通过app.WeToast访问
	imgresouces,
	C_Enum,


	getUserInfo: function (cb) {
		var that = this
		var _userInfo = wx.getStorageSync('userInfo')
		console.log(_userInfo)
		if (_userInfo && _userInfo.openId && this.globalData.isgetTel == 0) {
			that.globalData.userInfo = _userInfo;
			typeof cb == "function" && cb(_userInfo)
		} else {
			if (that.globalData.isgetTel == 0) {
				that.new_login(function (res) {
					if (res) {
						callback(res)
					}
				})
			} else {
				wx.login({
					success: function (res) {
						that.login(res.code, that.globalData.telEncryptedData, that.globalData.userInfo.UserId, that.globalData.telIv, cb, 1)
					}
				})
			}
			//调用登录接口
			// wx.login({
			// 	success: function (res) {
			// 		console.log('login res.code ' + res.code)
			// 		if (getApp().globalData.isgetTel == 0) { //如果isgetTel为0则正常登录，否则执行else 上传手机号码
			// 			wx.getUserInfo({
			// 				withCredentials: true,
			// 				success: function (data) {
			// 					console.log('getUserInfo登录成功')
			// 					that.login(res.code, data.encryptedData, data.signature, data.iv, cb);
			// 				},
			// 				fail: function () {
			// 					wx.showModal({
			// 						title: '提示',
			// 						showCancel: false,
			// 						confirmText: '知道啦',
			// 						content: '授权失败啦!\n请退出后在微信小程序中移除该小程序,再重扫二维码或直接搜索该小程序!',
			// 						success: function (res) {
			// 							if (res.confirm) {
			// 								console.log('用户点击确定')
			// 							}
			// 						}
			// 					})
			// 				},
			// 				complete: function () {
			// 					console.log('getUserInfo执行complete')
			// 				}
			// 			})
			// 		} else {
			// 			if (that.globalData.telEncryptedData == undefined || that.globalData.telIv == undefined) {
			// 				return
			// 			} else {
			// 				that.login(res.code, that.globalData.telEncryptedData, that.globalData.userInfo.UserId, that.globalData.telIv, cb, 1)
			// 			}
			// 		}
			// 	},
			// })
		}
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
				appid: that.globalData.appid || getApp().globalData.appid,
				isphonedata: isphonedata,
			},
			method: "Get",
			success: function (data) {
				if (data.data.result) {
					var json = data.data.obj
					console.log('loginByThirdPlatform接口返回 unionId - ' + json.unionId)
					that.globalData.userInfo = {
						avatarUrl: json.avatarUrl,
						city: json.city,
						UserId: json.userid,
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
					var msg = (data.data.msg == '序列化类型为“System.Reflection.RuntimeModule”的对象时检测到循环引用。' ? '系统繁忙，请重新操作。' : data.data.msg)
					console.log('登录失败 data - ' + data.data)
					wx.showModal({
						title: '提示',
						content: msg,
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

	new_login: function (callback) {
		var that = this
		var _userinfo = wx.getStorageSync('userInfo')
		that.globalData.userInfo = _userinfo
		if (_userinfo) {
			wx.setStorageSync('userInfo', _userinfo)
			callback(_userinfo)
		} else {
			wx.login({
				success: function (res) {
					that.WxLogin(res.code, getApp().globalData.appid, function (cb) {
						if (cb) {
							getApp().globalData.userInfo = cb.dataObj
							callback(cb)
						}
					});
				}
			})
		}
	},
	//第三方登录
	WxLogin: function (code, appid, cb) {
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
				console.log(data)
				var data = data.data
				if (data.isok) {
					data.dataObj.UserId = data.dataObj.Id
					data.dataObj.openId = data.dataObj.OpenId
					data.dataObj.nickName = data.dataObj.NickName
					data.dataObj.avatarUrl = data.dataObj.HeadImgUrl
					that.globalData.userInfo = data.dataObj
					wx.setStorageSync("userInfo", data.dataObj)
					wx.setStorageSync("utoken", data.dataObj.loginSessionKey)
					cb(data)
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
	ShowMsg: function (msg) {
		wx.showModal({
			title: '提示',
			content: msg,
			showCancel: false,
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
	//在确定是达达培东的情况下，请求获取运费
	getFee: function (lng, lat, address) {
		console.log("在确定是达达配送的情况下，请求获取运费")
		var that = this
		console.log("获取达达运费")
		console.log(that.globalData.jingdu, "显示经度")
		console.log(address)
		//获取城市 
		wx.request({
			url: addr.Address.GetDadaFreight,
			data: {
				cityname: "广州",
				appid: that.globalData.appid,
				openid: that.globalData.userInfo.openId,
				lat: lat,
				lnt: lng,
				acceptername: "用户",
				accepterphone: "158xxxxxxxx",
				address: that.globalData.cityname,
			},
			method: "POST",
			header: {
				'content-type': 'application/x-www-form-urlencoded'
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.setData({
						ShippingFeeStr: res.data.dataObj.deliverFee,

					});
					this.globalData.ShippingFeeStr = res.data.dataObj.deliverFee
					console.log("打印ShippingFeeStr的值")
					console.log(this.globalData.ShippingFeeStr)

				}
			},
			fail: function () {
				console.log("获取配送费失败")
			}
		})
	},
})