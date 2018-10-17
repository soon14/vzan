// pages/appointment/appointment.js
var app = getApp();
var util = require('../../utils/util.js');
var http = require('../../utils/http.js');
var addr = require('../../utils/addr.js');
var tools = require("../../utils/tools.js");

Page({

  /**
   * 页面的初始数据
   */
	data: {
		shopcartList: [],//已经点餐的购物车集合

		listmodal: false,
		appointMsg: {},//预约点餐信息集合
		numsArray: ['选择预约人数'],

		goodCarIdStr: '',//购物车id串
	},
	bindDateChange: function (e) {
		app.globalData.appointMsg.datatime = e.detail.value
		this.setData({ appointMsg: app.globalData.appointMsg })
	},
	bindTimeChange: function (e) {
		app.globalData.appointMsg.time = e.detail.value
		this.setData({ appointMsg: app.globalData.appointMsg })
	},
	bindPickerChange: function (e) {
		app.globalData.appointMsg.numsindex = e.detail.value
		this.setData({ appointMsg: app.globalData.appointMsg })
	},
	inputmessage: function (e) {
		app.globalData.appointMsg.msg = e.detail.value
		this.setData({ appointMsg: app.globalData.appointMsg })
	},
	inputname: function (e) {
		app.globalData.appointMsg.name = e.detail.value
		this.setData({ appointMsg: app.globalData.appointMsg })
	},
	inputphonenumber: function (e) {
		app.globalData.appointMsg.phonenumber = e.detail.value
		this.setData({ appointMsg: app.globalData.appointMsg })
	},
	show_listmodal: function () {
		this.setData({ listmodal: !this.data.listmodal })

	},
	go_appointmentindex: function () {
		if (this.data.shopcartList.length > 0) { this.setData({ listmodal: !this.data.listmodal }) } //跳转之前把弹窗隐藏回去
		wx.navigateTo({ url: '../appointment/appointment_index' })
	},
	/**
	 * 生命周期函数--监听页面加载
	 */
	onLoad: function (options) {


		this.data.AccountMoneyStr = options.AccountMoneyStr

		this.data.appointMsg.stardate = new Date().getFullYear() + '-' + new Date().getMonth() + '-' + new Date().getDate()
		for (var numsindex = 1; numsindex < 15; numsindex++) {
			this.data.numsArray.push(numsindex)
		}
		app.globalData.appoint_numsArray = this.data.numsArray
		this.setData({ numsArray: this.data.numsArray, appointMsg: app.globalData.appointMsg })
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

		this.setData({ shopcartList: app.globalData.appoint_shopcartlist, shopcartlength: app.globalData.appoint_shopcartlength, alldiscountprice: app.globalData.appoint_alldiscountprice, goodslist: app.globalData.appoint_shopcartlist })
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
	goappoint: function (e) {//预约按钮
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		var itemList = ['微信支付', '储值支付']
		if (Number(that.data.AccountMoneyStr) < Number(that.data.alldiscountprice)) {
			itemList.splice(1, 1)
		}
		var _a = app.globalData.appointMsg
		var _g = that.data.shopcartList
		that.data.goodCarIdStr = ''//初始化购物车id串		
		if (_a.datatime == '选择预约日期' || _a.name == '' || _a.numsindex == 0 || _a.phonenumber == '' || _a.time == '选择预约时间' || _g.length == 0) {
			wx.showToast({ title: '信息未填写完整', icon: 'loading' })
			return
		}
		var orderjson = {
			AddressId: 0,
			Message: '',
			TablesNo: "0",
			OrderType: 0,
		}

		var j = 0
		for (var i = 0; i < _g.length; i++) {
			that.appoint_addGoodsCarData(that, _g[i].goodid, _g[i].attrSpacStr, _g[i].SpecInfo, _g[i].buynums, function (cb) {
				j++
				if (that.data.goodCarIdStr == '') {
					that.data.goodCarIdStr = cb
				} else {
					that.data.goodCarIdStr = that.data.goodCarIdStr + ',' + cb
				}

				if (app.globalData.appoint_paynow == 1) { //预约支付
					if (cb > 0 && j == _g.length) {
						wx.showActionSheet({
							itemList: itemList,
							success: function (res) {
								if (res.tapIndex == 0) {
									app.globalData.payType = 1
									that.AddReservation(that, _a.datatime + ' ' + _a.time, that.data.numsArray[_a.numsindex], _a.name, _a.phonenumber, _a.msg, JSON.stringify(orderjson), that.data.goodCarIdStr, 1, function (cb) {
										if (cb.isok == 1) {
											var newparam = {
												openId: app.globalData.userInfo.openId,
												orderid: cb.orderid,
												aid: wx.getStorageSync('aid'),
												'type': 1,
											}
											// cb.dbOrder, 
											util.PayOrder(newparam, {
												failed: function (res) {
													wx.showToast({ title: '您取消了支付！', icon: 'loading' })
													that.CancelResevation()
												},
												success: function (res) {
													if (res == "wxpay") {
														console.log(res)
													} else if (res == "success") {
														template.showtoast('预约成功', 'success')
														setTimeout(function () {
															wx.redirectTo({ url: '../appointment/appointment_info' })
														}, 1000)
													}
												}
											})


										}
									})

								} else {
									wx.showModal({
										title: '提示',
										content: '是否确认使用储值支付？',
										success: function (res) {
											if (res.confirm) {
												app.globalData.payType = 2

												that.AddReservation(that, _a.datatime + ' ' + _a.time, that.data.numsArray[_a.numsindex], _a.name, _a.phonenumber, _a.msg, JSON.stringify(orderjson), that.data.goodCarIdStr, 2, function (cb) {
													if (cb.isok == 1) {
														wx.showToast({ title: '预约成功！' })
														setTimeout(function () {
															wx.redirectTo({ url: '../appointment/appointment_info' })
														}, 1000)
													}
												})
											}
										}
									})
								}
							},
						})
					}
				}


				if (app.globalData.appoint_paynow == 0) { //到店扫码支付
					if (cb > 0 && j == _g.length) {
						that.AddReservation(that, _a.datatime + ' ' + _a.time, that.data.numsArray[_a.numsindex], _a.name, _a.phonenumber, _a.msg, JSON.stringify(orderjson), that.data.goodCarIdStr, 1, function (cb) {
							if (cb.isok == 1) {
								wx.showToast({ title: '预约成功！' })
								setTimeout(function () {
									wx.redirectTo({ url: '../appointment/appointment_info' })
								}, 1000)
							}
						})
					}
				}




			})
		}
	},


	//请求商品加入购物车
	appoint_addGoodsCarData: function (that, goodid, attrSpacStr, SpecInfo, GoodsNumber, cb) {
		wx.request({
			url: addr.Address.appoint_addGoodsCarData,
			data: {
				appId: app.globalData.appid,
				openId: app.globalData.userInfo.openId,
				goodId: goodid,
				attrSpacStr: attrSpacStr,
				SpecInfo: SpecInfo,
				qty: GoodsNumber,
				newCartRecord: 1,
				isReservation: true
			},
			method: "POST",
			header: {
				"content-type": "application/x-www-form-urlencoded"
			},
			success: function (res) {
				if (res.data.isok == true) {
					cb(res.data.cartid)
				}
			},
			fail: function () {
				console.log("加入预约购物车出错")
			}
		})
	},


	AddReservation: function (that, dinningTime, seats, userName, contact, note, orderjson, goodCarIdStr, buyMode, cb) {
		wx.request({
			url: addr.Address.AddReservation,
			data: {
				appId: getApp().globalData.appid,
				openId: getApp().globalData.userInfo.openId,
				reserveTime: dinningTime,
				seats: seats,
				userName: userName,
				contact: contact,
				note: note,
				orderjson: orderjson,
				goodCarIdStr: goodCarIdStr,
				isgroup: 0,
				goodtype: 0,
				buyMode: buyMode,
			},
			method: "POST",
			header: {
				"content-type": "application/x-www-form-urlencoded"
			},
			success: function (res) {
				app.globalData.appoint_Id = res.data.reserveId
				if (res.data.isok == true) {
					cb(res.data)
				} else {
					wx.showModal({
						title: '提示',
						content: res.data.msg,
						showCancel: false,
						confirmText: '知道了',
					})
				}
			},
			fail: function () {
				console.log("加入预约购物车出错")
			}
		})
	},
	CancelResevation: function (that) {
		wx.request({
			url: addr.Address.CancelResevation,
			data: {
				appId: getApp().globalData.appid,
				openId: getApp().globalData.userInfo.openId,
				reserveId: getApp().globalData.appoint_Id
			},
			method: "POST",
			header: {
				"content-type": "application/x-www-form-urlencoded"
			},
			success: function (res) {
				if (res.data.result) {
					// getApp().globalData.appoint_Id = 0
					tools.resetappoint()
					setTimeout(function () {
						wx.navigateBack({ delta: 1 })
					}, 500)
				}
			},
			fail: function () {
				console.log("加入预约购物车出错")
			}
		})
	},

})