// pages/updateOrder/updateOrder.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		clientTel: 0,//用户手机号码，用于渲染
		isOncepull: 0,//控制去支付按钮 防止多次点击
		youhui: 0,
		yuanjia: 0,
		wayToPay: ['微信支付', '储值支付'],
		index: 0,//选择支付模式下标 0是微信 1是储值支付
		address: [],
		paymoeny: 0,//支付的总价格
		orderid: 0,//订单id
		ShippingFeeStr: 0,//配送费
		OutSideStr: 0,//起送价
		lengthCounr: 0,
		Message: "",
		TablesNo: '',
		item5: [],
		logoimg: '',
		FoodsName: '',
		OrderType: '',//0 堂食 / 1 外卖
		allprice: 0,
		goodCarIdStr: '',//购物车id串
		addressId: 0,
		item: [
			{ name: '蔓越草莓', nums: '2', price: '30' },
			{ name: '美式精选', nums: '1', price: '24' },
			{ name: '美式精选', nums: '1', price: '24' },
			{ name: '美式精选', nums: '1', price: '24' },
			{ name: '美式精选', nums: '1', price: '24' },
		],
		msg: '',
	},
	// 选择支付模式
	bindPickerChange: function (e) {
		this.setData({
			index: e.detail.value
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		that.setData({
			TablesNo: app.globalData.TablesNo, ShippingFeeStr: app.globalData.ShippingFeeStr, isOncepull: 0, clientTel: app.globalData.userInfo.TelePhone
		})
		var logoimg = app.globalData.logoimg
		var ShippingFeeStr = parseFloat(app.globalData.ShippingFeeStr).toFixed(2)
		var OutSideStr = app.globalData.OutSideStr
		var FoodsName = app.globalData.FoodsName
		var item5 = JSON.parse(options.item5)
		var allprice = parseFloat(options.allprice).toFixed(2)
		var yuanjia = 0
		for (var i = 0; i < item5.length; i++) {
			item5[i].yuanjia = Number(item5[i].yuanjia)
			yuanjia += Number(item5[i].yuanjia * item5[i].nums)
		}
		var paymoeny = (parseFloat(ShippingFeeStr) + parseFloat(allprice)).toFixed(2)
		if (app.globalData.TablesNo == -999) { //外卖
			if (Number(app.globalData.AccountMoneyStr) > Number(paymoeny) && app.globalData.canSaveMoneyFunction == true) {
				var index = 1
				app.globalData.payType = 2 //储值支付
			} else {
				var index = 0
				app.globalData.payType = 1 //微信支付
			}
			var youhui = (Number(yuanjia) - Number(paymoeny) + Number(ShippingFeeStr)).toFixed(2)
		} else {  //堂食
			if (Number(app.globalData.AccountMoneyStr) > Number(allprice) && app.globalData.canSaveMoneyFunction == true) { //堂食情况下，如果储值金额>买单金额 则默认选择储值金额 同上
				var index = 1
				app.globalData.payType = 2
			} else {
				var index = 0
				app.globalData.payType = 1
			}
			var youhui = (Number(yuanjia) - Number(allprice) + Number(ShippingFeeStr)).toFixed(2)
		}
		if (app.globalData.canSaveMoneyFunction == true) {
			this.data.wayToPay = this.data.wayToPay
		} else {
			this.data.wayToPay = this.data.wayToPay.splice(0, 1)
		}
		yuanjia = parseFloat(yuanjia).toFixed(2)
		this.setData({
			wayToPay: this.data.wayToPay,
			yuanjia: yuanjia,
			youhui: youhui,
			item5: item5,
			paymoeny: paymoeny,
			logoimg: logoimg,
			FoodsName: FoodsName,
			allprice: allprice,
			index: index
		})
	},
	// 跳转到选择地址页面
	navTosetAddress: function () {
		var that = this
		wx.navigateTo({
			url: '../setAddress/setAddress',
		})
	},
	// 订单备注
	inputMessage: function (e) {
		var Message = this.data.Message
		this.setData({ Message: e.detail.value })
	},
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
	onReady: function () {

	},

  /**
   * 生命周期函数--监听页面显示
   */
	onShow: function (e) {
		if (app.globalData.dizhiId != '') {
			var addressId = parseInt(app.globalData.dizhiId)
			this.inite1(addressId)
		}

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

	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {

	},

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	},
	// 获取用户手机号码
	getPhoneNumber: function (e) {
		var that = this
		console.log(e.detail.errMsg)
		console.log(e.detail.iv)
		console.log(e.detail.encryptedData)
		app.globalData.telEncryptedData = e.detail.encryptedData
		app.globalData.telIv = e.detail.iv
		app.globalData.isgetTel = 1
		app.getUserInfo(function (res) {
			if (res.TelePhone != '未绑定') {
				that.setData({ clientTel: res.TelePhone })
			}
		})
	},
	// 把商品加入购物车
	gotoPay: function (e) {
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		var isclearItem5 = 1
		app.globalData.isclearItem5 = isclearItem5
		var item5 = that.data.item5
		var lengthCounr = that.data.lengthCounr
		var goodCarIdStr = that.data.goodCarIdStr
		if (lengthCounr == 0 && that.data.isOncepull == 0) {
			that.data.isOncepull = 1
			for (var i = 0; i < item5.length; i++) {
				//  循环请求
				wx.request({
					url: addr.Address.addGoodsCarData,
					data: {
						appId: app.globalData.appid,
						openid: app.globalData.userInfo.openId,
						goodid: item5[i].goodsid,
						attrSpacStr: item5[i].attrSpacStr,
						SpecInfo: item5[i].SpecInfo,
						GoodsNumber: item5[i].nums,
						newCartRecord: 1
					},
					method: "POST",
					header: {
						'content-type': 'application/json' // 默认值
					},
					success: function (res) {
						if (res.data.isok == 1) {
							if (goodCarIdStr == '') {
								goodCarIdStr = res.data.cartid
							} else {
								goodCarIdStr = goodCarIdStr + ',' + res.data.cartid
							}
							lengthCounr++
							that.setData({
								goodCarIdStr: goodCarIdStr,
								lengthCounr: lengthCounr
							})
							if (lengthCounr == item5.length) {
								if (that.data.index == 0) { //微信支付
									app.globalData.payType = 1
									that.PayOrder()
								} else { //储值支付
									wx.showModal({
										title: '提示',
										content: '是否确认使用储值支付？',
										success: function (res) {
											if (res.confirm) {
												app.globalData.payType = 2
												that.PayOrder()
											} else if (res.cancel) {
												that.data.isOncepull = 2
											}
										}
									})
								}
							}
						}
						console.log('~', goodCarIdStr)
					},
					fail: function () {
						console.log("提交购物车失败")
						wx.showToast({
							title: '提交购物车失败',
						})
					}
				})
			}
		} else {
			if (that.data.isOncepull == 2) {
				wx.showModal({
					title: '提示',
					content: '请返回上一页面再次提交订单',
					showCancel: false,
					success: function (res) {
						if (res.confirm) {
							app.goBackPage(1)
						}
					}
				})
			}
		}
	},
	showToast: function () {
		wx.showToast({
			title: '请选择地址',
			icon: 'loading'
		})
	},
	showToast1: function () {
		wx.showModal({
			title: '提示',
			content: '该地址在配送范围外，请重新选择！',
		})
	},
	// 支付
	PayOrder: function () {
		var that = this
		var lengthCounr = that.data.lengthCounr
		var orderid = that.data.orderid
		if (that.data.TablesNo == -999) {
			var OrderType = 1
		} else {
			var OrderType = 0
		}
		if (OrderType == 1) {
			var AddressId = parseInt(app.globalData.dizhiId)
		} else {
			var AddressId = 0
		}
		var orderjson = {
			AddressId: AddressId,
			Message: that.data.Message,
			TablesNo: that.data.TablesNo,
			OrderType: OrderType
		}
		var orderjson = JSON.stringify(orderjson)
		if (app.globalData.payType == 1) {
			var buyModeId = 1
		} else {
			var buyModeId = 2
		}
		var param = {
			appid: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			goodCarIdStr: that.data.goodCarIdStr,
			orderjson: orderjson,
			buyMode: buyModeId
		}
		util.AddOrder(param, function (e) {
			if (e == "failed") {
				wx.hideLoading()
			}
			else {
				wx.redirectTo({
					url: '../paySuccess/paySuccess'
				})
			}
		}, "")
		that.setData({ lengthCounr: 99 })
	},
	//获取我的地址列表
	inite1: function (addressId) {
		var that = this
		wx.request({
			url: addr.Address.GetMyAddress,
			data: {
				AppId: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				isDefault: 0,
				addressId: addressId,
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.inite2(res.data.postdata.address.Lat, res.data.postdata.address.Lng)
					that.setData({
						address: res.data.postdata.address
					})
				}
			},
			fail: function () {
				console.log("获取我的地址列表出错")
				wx.showToast({
					title: '获取我的地址列表出错',
				})
			}
		})
	},
	// 查询配送距离
	inite2: function (lat, lng) {
		var that = this
		wx.request({
			url: addr.Address.GetDistanceForFood,
			data: {
				AppId: app.globalData.appid,
				lat: lat,
				lng: lng
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.msg != '配送范围内') {
					wx.showModal({
						title: '提示',
						content: '该地址在配送范围外，请重新选择！',
					})
				}
			},
			fail: function () {
				console.log("查询出错")
				wx.showToast({
					title: '查询出错',
				})
			}
		})
	},
	// 储值支付
	buyOrderbySaveMoney: function () {
		var that = this
		wx.request({
			url: addr.Address.buyOrderbySaveMoney,
			data: {
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				goodsorderid: that.data.goodCarIdStr
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.PayOrder()
				}
			},
			fail: function () {
				console.log("支付失败")
				wx.showToast({
					title: '获取首页出错',
				})
			}
		})
	},
})