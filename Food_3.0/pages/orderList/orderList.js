// pages/orderList/orderList.js
var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var app = getApp();
var tool = require('../../template/Food2.0.js');
var template = require('../../template/template.js')
Page({

  /**
   * 页面的初始数据
   */
	data: {
		pageIndex: 1,
		pageIndex1: 2,
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		wx.showLoading({ title: '页面加载中', })
		app.new_login(function (e) {
			tool.getMiniappGoodsOrder(that, that.data.pageIndex, 0, 0, 1)
		})
	},
	// 跳转到订单详情页
	navtoOrderinfo: function (e) {
		app.globalData.dbOrder = e.currentTarget.id
		wx.navigateTo({ url: '../orderInfo/orderInfo' })
	},
  /**
   * 生命周期函数--监听页面显示
   */
	onShow: function () {
		// tool.getMiniappGoodsOrder(this, 0, 0, 2, 1)
	},
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
	onPullDownRefresh: function () {
		tool.getMiniappGoodsOrder(this, 0, 0, 2, 0)
		template.stopPullDown()
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		tool.getMiniappGoodsOrder(this, this.data.pageIndex, 1, 99, 0)
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
							tool.getMiniappGoodsOrder(that, 0, 0, 2, 0)
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
		app.globalData.isOrderlist = 1
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
					// tool.getMiniappGoodsOrder(that, 0, 0, 2, 0)
				}
			}
		})
	}
})