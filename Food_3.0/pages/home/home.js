// pages/home/home.js
var tool = require('../../template/Food2.0.js');
var template = require('../../template/template.js');
var canvas = require("../../utils/canvas.js");
var addr = require("../../utils/addr.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		islogin: false,
		animationData: {},//走马灯动画
		typeIcon: [    // 门店图标
			{ txt: '扫码点餐', iconPath: 'http://j.vzan.cc/miniapp/img/lijianjin/a29.png', id: 1 },
			{ txt: '点外卖', iconPath: 'http://j.vzan.cc/miniapp/img/lijianjin/a30.png', id: 2 },
			{ txt: '会员卡', iconPath: 'http://j.vzan.cc/miniapp/img/lijianjin/a31.png', id: 3 },
			{ txt: '储值有礼', iconPath: 'http://j.vzan.cc/miniapp/img/lijianjin/a32.png', id: 4 },
			{ txt: '推荐好友', iconPath: 'http://j.vzan.cc/miniapp/img/lijianjin/a33.png', id: 5 },
			{ txt: '排队拿号', iconPath: 'http://j.vzan.cc/miniapp/img/lijianjin/sortQueue.png', id: 6 },
			{ txt: '预约点餐', iconPath: 'http://j.vzan.cc/miniapp/img/lijianjin/reservation.png', id: 7 },
		],
		showCanvas: true,

		item: ['会员折扣'],
		couponList: [],
		ShippingFeeStr: '',
		shopcartList: [],
		pageindex: 1
	},
	navo_webview: function () {
		wx.navigateTo({
			url: '/pages/me/web_view?id=' + this.data.AgentConfig.QrcodeId,
		})
	},
	getUserInfo: function (e) {
		var that = this
		var _e = e.detail
		if (e.detail.errMsg != 'getUserInfo:fail auth deny') {
			wx.login({
				success: function (res) {
					app.login(res.code, _e.encryptedData, _e.signature, _e.iv, function (cb) {
						that.setData({ islogin: true })
					}, 0)
				}
			})
		} else {
			wx.showModal({
				title: '提示',
				content: '你拒绝了登录授权，请再次点击登录进行操作。',
				showCancel: false
			})
		}
	},
	go_groupinfo: function (e) {
		template.goNewPage('../group/group_goodinfo?groupid=' + e.currentTarget.dataset.groupid)
	},
	gostoreinfo: function () {
		template.goNewPage('../storeInfo/storeInfo')
	},
	makePhoneCall: function () {
		template.makePhoneCall(this.data.food.TelePhone)
	},
	navigation: function () {
		tool.openLocation(this.data.food.Lat, this.data.food.Lng, 14)
	},
	onCancle: function (e) {
		this.setData({ showCanvas: (!this.data.showCanvas) })
	},
	// 保存画布图片
	canvasToTempFilePath: function (e) {
		wx.getSetting({
			success: function (res) {
				console.log(res)
				if (res.authSetting["scope.writePhotosAlbum"] == undefined) {
					tool.canvasToTempFilePath(e.currentTarget.id)
				} else {
					if (!res.authSetting["scope.writePhotosAlbum"]) {
						wx.showModal({
							title: '提示',
							content: '您拒绝过授权访问相册，请进入下一步设置启动"保存到相册"。',
							showCancel: false,
							confirmText: '下一步',
							success() {
								wx.openSetting({})
							}
						})
					} else {
						tool.canvasToTempFilePath(e.currentTarget.id)
					}
				}
			}
		})
	},
	previewImage: function (e) {
		tool.previewStoreimg(e, this.data.sliderImgs)
	},
	getcoupon: function (e) {
		template.GetCoupon(this, e.currentTarget.dataset.couponid)
	},
	go_mycoupon: function () {
		wx.navigateTo({
			url: '../me/mycoupon',
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		app.new_login(function (e) {
			tool.GetGoodsList(that, 0, '', 0, 1, 0, 1)
			template.GetStoreCouponList(that, -1)
			template.GetAgentConfigInfo(that)
			template.GetVipInfo(that, function () { })
			template.getSaveMoneySetUser(that)
			if (e) {
				tool.GetFoodsDetail(that, 0, function (res) {
					if (res == 'ok0') {
						tool.action(that, that.data.food.Notice)
						setInterval(function () {
							tool.action(that, that.data.food.Notice)
						}, that.data.food.Notice.length / 3 * 1000)
					}
				})
			}
			that.setData({ islogin: wx.getStorageSync('userInfo').avatarUrl == null ? false : true })
		})

		if (options != undefined) {//从模版消息跳转到小程序
			if (options.lat != undefined && options.lng != undefined) {
				setTimeout(function () {
					tool.openLocation(Number(options.lat), Number(options.lng), 14)
				}, 2000)
			}
		}

	},
	/**
	*获取达达运费
	**/
	getFee: function (lat, lnt, address) {
		var that = this
		//获取城市 
		var city = address.match(/省(\S*)市/);
		getApp().globalData.cityname = city[1]
		wx.request({
			url: addr.Address.GetDadaFreight,
			data: {
				cityname: app.globalData.cityname,
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				lat: lat,
				lnt: lnt,
				acceptername: "用户",
				accepterphone: "158xxxxxxxx",
				address: '海珠',
			},
			method: "POST",
			header: {
				'content-type': 'application/x-www-form-urlencoded'
			},
			success: function (res) {
				console.log("測試")
				console.log(res)
				if (res.data.isok == 1) {
					that.setData({
						ShippingFeeStr: res.data.dataObj.deliverFee,
						distributionprice: res.data.dataObj.deliverFeeInt,
					});
					app.globalData.ShippingFeeStr = res.data.dataObj.deliverFee
					app.globalData.distributionprice = res.data.dataObj.deliverFeeInt
				}
			},
			fail: function (res) {
				console.log('请求达达配送运费失败', res)
			}
		})
	},

	/**
	 * 生命周期函数--监听页面初次渲染完成
	 */
	onReady: function () {


	},

	/**
	 * 生命周期函数--监听页面显示
	 */
	onShow: function () {
		// var that = this
		// if (app.globalData.appoint_Id == 0) {
		// 	tool.GetFoodsDetail(that, 0, function (res) {
		// 		if (res == 'ok0') {
		// 			tool.action(that, that.data.food.Notice)
		// 			setInterval(function () {
		// 				tool.action(that, that.data.food.Notice)
		// 			}, that.data.food.Notice.length / 3 * 1000)
		// 		}
		// 	})
		// }
		// that.setData({
		//     ShippingFeeStr: app.globalData.ShippingFeeStr
		// })
	},

	/**
	 * 生命周期函数--监听页面隐藏
	 */
	onHide: function () {

	},

	/**
	 * 生命周期函数--监听页面卸载
	 */
	onUnload: function () {

	},

	/**
	 * 页面相关事件处理函数--监听用户下拉动作
	 */
	onPullDownRefresh: function () {
		this.data.pageindex = 1
		this.onLoad()
		template.stopPullDown()
	},

	/**
	 * 页面上拉触底事件的处理函数
	 */
	onReachBottom: function () {
		tool.GetGoodsList(this, 0, '', 0, this.data.pageindex, 1, 1)
	},

	/**
	 * 用户点击右上角分享
	 */
	onShareAppMessage: function () {
		if (this.data.food.funJoinModel.shareConfig.ADTitle != null && this.data.food.funJoinModel.shareConfig.ADImg != []) {
			return {
				title: this.data.food.funJoinModel.shareConfig.ADTitle,
				path: '/pages/home/home',
				imageUrl: this.data.food.funJoinModel.shareConfig.ADImg[0]
			}
		}
	},
	// 首页大图标动作
	doSomething: function (e) {
		var that = this
		var index = e.currentTarget.id
		if (index == 1) {
			wx.scanCode({
				onlyFromCamera: true,
				success: (res) => {
					console.log(res)
					if (res.path == undefined) {
						wx.showModal({
							title: '提示',
							content: '亲，该二维码有误！',
							showCancel: false
						})
					} else {
						wx.navigateTo({ url: '/' + res.path, })
					}
				}
			})
		} else if (index == 2) {
			wx.navigateTo({ url: '../index/index' })
		} else if (index == 3) {
			wx.switchTab({ url: '../me/me' })
		} else if (index == 4) {
			wx.navigateTo({ url: '../myStorevalue/myStorevalue' })
		} else if (index == 5) {
			canvas.Drawcanvas()
			that.setData({ showCanvas: !that.data.showCanvas })
		} else if (index == 6) {
			wx.navigateTo({ url: '../lineup_index/lineup_index' })
		} else {
			if (app.globalData.reservationSwitch == true) {
				if (app.globalData.appoint_Id > 0) {
					wx.navigateTo({ url: '../appointment/appointment_info' })
				} else {
					wx.navigateTo({ url: '../appointment/appointment' })
				}
			} else {
				wx.showModal({
					title: '提示',
					content: '该商家未开启预约点餐功能！',
					showCancel: false
				})
			}

		}
	},
	commintformid: function (e) {
		// 提交备用formId
		var formId = e.detail.formId
		template.commitFormId(formId, this)
	},
})