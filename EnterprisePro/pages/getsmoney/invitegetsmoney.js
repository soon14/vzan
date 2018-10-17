// pages/getsmoney/invitegetsmoney.js
var tools = require('../../utils/tools.js')
var pages = require('../../utils/pageRequest.js')
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		console.log('我是options', options)
		that.data.couponsId = options.couponsId
		app.globalData.reductionCart.Id = options.couponsId
		that.data.dbOrder = options.orderId
		app.globalData.dbOrder = options.orderId
		that.data.isfirstIn = options.isfirstIn
		app.getUserInfo(function () {
			pages.GetReductionCard(that, options.couponsId, options.orderId, 2)
		})
		setTimeout(function () {
			if (that.data.coupon != null) {
				setInterval(function () {
					var nowTime = new Date().getTime()
					var endtime = ((new Date(that.data.coupon.EndUseTimeStr)).getTime() - nowTime)
					that.data.coupon.endtime = tools.formatDuring(endtime)
					that.setData({ coupon: that.data.coupon })
				}, 1000)
			}
		}, 1000)
	},

	getreducecard: function () {
		var that = this
		console.log('couponsId,dbOrder', that.data.couponsId, that.data.dbOrder)
		app.getUserInfo(function (e) {
			pages.GetReductionCard(that, that.data.couponsId, that.data.dbOrder, 1)
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
		var that = this
		return {
			title: '社交立减金',
			path: '/pages/getsmoney/invitegetsmoney?couponsId=' + that.data.couponsId + '&orderId=' + that.data.dbOrder
		}
	}
})