// pages/orderList/orderList.js
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
		openId: '',
		postdata: [],
		// pageIndex0: 1,
		pageIndex: 1,
		pageIndex1: 2,
		postdata: [],
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		app.getUserInfo(function (e) {
			that.setData({ userinfo: app.globalData.userInfo, openId: e.openId })
			that.inite(that.data.pageIndex, 0, 0, 1)
		})
	},
	// 取消订单
	cancelOrder: function (e) {
		var that = this
		var index = e.currentTarget.id
		wx.showModal({
			title: '提示',
			content: '确定要取消订单吗？',
			success: function (res) {
				if (res.confirm) {
					that.inite1(index, -1)
				} else if (res.cancel) {
					console.log('用户点击取消')
				}
			}
		})
	},
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
	onReady: function () {

	},
	// 申请退款
	cancelpayMoeny: function (e) {
		var that = this
		var index = parseInt(e.currentTarget.id)
		wx.showModal({
			title: '提示',
			content: '确定申请退款？',
			success: function (res) {
				if (res.confirm) {
					that.inite1(index, -2)
				} else if (res.cancel) {
					console.log('用户点击取消')
				}
			}
		})

	},
	// 确认送达
	orderisOk: function (e) {
		var that = this
		var index = parseInt(e.currentTarget.id)
		wx.showModal({
			title: '提示',
			content: '确认送达？',
			success: function (res) {
				if (res.confirm) {
					util.UpdateWxCard(that)
					that.inite1(index, 5)
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
		// var orderId = parseInt(app.globalData.orderid)
		var oradid = e.currentTarget.id
		var newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: oradid,
			'type': 1,
		}
		util.PayOrder(oradid, newparam, {
			failed: function () {
				wx.showModal({
					title: '提示',
					content: '支付失败，客户取消支付',
				})
			},
			success: function (res) {
				if (res == "wxpay") {
				} else if (res == "success") {
					wx.showToast({
						title: '支付成功',
						duration: 500
					})
					that.inite(0, 0, 2, 0)
				}
			}
		})
	},
	// 跳转到订单详情页
	navtoOrderinfo: function (e) {
		var orderid = e.currentTarget.id
		app.globalData.orderid = orderid
		wx.navigateTo({
			url: '../orderInfo/orderInfo'
		})
	},
  /**
   * 生命周期函数--监听页面显示
   */
	onShow: function () {
		this.inite(0, 0, 2, 1)
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
		var that = this
		that.inite(0, 0, 2, 0)
		setTimeout(function () {
			wx.stopPullDownRefresh()
			that.setData({
				condition: false
			})
		}, 1000)
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		this.inite(this.data.pageIndex, 1, 99)
	},

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	},
	// 获取我的订单
	inite: function (pageIndex, isFirst, isRefresh, isonShow) {
		var that = this
		var postdata = that.data.postdata
		if (isRefresh == 2) {
			pageIndex = 1
			that.data.pageIndex1 = 2
		} else {
			pageIndex = that.data.pageIndex
		}
		wx.request({
			url: addr.Address.getMiniappGoodsOrder,
			data: {
				AppId: app.globalData.appid,
				openid: that.data.openId,
				State: 10,
				pageIndex: pageIndex,
				pageSize: 5
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					for (var i = 0; i < res.data.postdata.length; i++) {
						for (var j = 0; j < res.data.postdata[i].orderList.length; j++) {
							res.data.postdata[i].orderList[j].BuyPrice = Number(res.data.postdata[i].orderList[j].BuyPrice).toFixed(2)
						}
					}
					if (isRefresh == 2 && isonShow == 0) {
						wx.showToast({
							title: '刷新成功',
						})
					}
					if (isFirst == 1) {
						if (res.data.postdata != '') {
							if (res.data.postdata[0].year == postdata[0].year) {
								postdata[0].orderList = (postdata[0].orderList).concat(res.data.postdata[0].orderList)
								that.data.pageIndex1++
							}
						} else {
							return
						}
					} else {
						postdata = res.data.postdata
					}
					that.setData({
						postdata: postdata,
						pageIndex: that.data.pageIndex1
					})
					console.log('1', res)
				}
			},
			fail: function () {
				console.log("获取订单列表出错")
				wx.showToast({
					title: '获取订单列表出错',
				})
			}
		})
	},
	// 取消已付款订单
	inite1: function (orderId, State) {
		var that = this
		wx.request({
			url: addr.Address.updateMiniappGoodsOrderState,
			data: {
				AppId: app.globalData.appid,
				openid: that.data.openId,
				orderId: orderId,
				State: State
			},
			method: "POST",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.inite(0, 0, 2)
				}
			},
			fail: function () {
				console.log("更新订单状态出错")
				wx.showToast({
					title: '更新订单状态出错',
				})
			}
		})
	},
})