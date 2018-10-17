// pages/orderInfo/orderInfo.js
var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var tool = require('../../template/Food2.0.js');
var template = require('../../template/template.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {

	},
	// 联系商家
	makePhonecall: function () {
		template.makePhoneCall(app.globalData.TelePhone)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		if (options.scene != undefined) {
			var orderId = options.scene
		} else {
			var orderId = parseInt(app.globalData.dbOrder)
		}
		that.setData({ logoimg: app.globalData.logoimg, FoodsName: app.globalData.FoodsName, TablesNo: app.globalData.TablesNo, TelePhone: app.globalData.TelePhone, orderId: orderId })
		if (app.globalData.userInfo.openId == undefined) {
			app.new_login(function (e) {
				tool.getMiniappGoodsOrderById(that, orderId)
			})
		} else {
			tool.getMiniappGoodsOrderById(that, orderId)
		}
	},



	// 取消订单-1 申请退款-2 确认送达5
	changeState: function (e) {
		var that = this
		var index = parseInt(e.currentTarget.id)
		var state = e.currentTarget.dataset.stateid
		if (state == -1) { var content = '是否确认取消订单？' }
		else if (state == -2) { var content = '是否申请退款？' }
		else { var content = '是否确认送达？' }
		wx.showModal({
			title: '提示',
			content: content,
			success: function (res) {
				if (res.confirm) {
					template.UpdateWxCard(that)
					tool.updateMiniappGoodsOrderState(index, state, function (cb) {
						if (cb == '更新成功') {
							tool.getMiniappGoodsOrderById(that, that.data.orderId)
						} else {
							wx.showModal({
								title: '提示',
								content: cb,
								showCancel: false,
								success: function (res0) {
									if (res0.confirm) {
										wx.switchTab({ url: '../orderList/orderList', })
									}
								}
							})
						}
					})
				} else if (res.cancel) {
					console.log('用户点击取消')
				}
			}
		})
	},
	// 二次付款
	gotoPay: function (e) {
		var that = this
		var payid = e.currentTarget.dataset.payid
		var orderid = e.currentTarget.dataset.orderid
		app.globalData.dbOrder = orderid
		var newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: payid,
			'type': 1,
		}
		util.PayOrder(payid, newparam, {
			failed: function () {
				wx.showModal({
					title: '提示',
					content: '支付失败，客户取消支付',
					success: function (res) {
						if (res.confirm) {
							tool.updateMiniappGoodsOrderState(orderid, 0, function (res) { })
						}
					}
				})
			},
			success: function (res) {
				if (res == "wxpay") {
				} else if (res == "success") {
					wx.showToast({
						title: '支付成功',
						duration: 500
					})
					wx.redirectTo({ url: '../paySuccess/paySuccess', })
					// app.goBackPage(1)
				}
			}
		})
	},
})