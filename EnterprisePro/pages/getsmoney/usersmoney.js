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
		pages.GetReductionCard(that, app.globalData.reductionCart.Id, app.globalData.dbOrder, 2)



		setTimeout(function () {
			if (that.data.coupon != null) {
				setInterval(function () {
					var nowTime = new Date().getTime() //现在的时候戳
					if (new Date(that.data.coupon.StartUseTimeStr).getTime() > nowTime) {
						that.data.coupon.endtime = '活动仍未开始'
					} else {
						var endtime = ((new Date(that.data.coupon.EndUseTimeStr)).getTime() - nowTime)
						that.data.coupon.endtime = tools.formatDuring(endtime)
					}

					that.setData({ coupon: that.data.coupon })
				}, 1000)
			}
		}, 1000)
	},
	swichtoindex: function () {
		wx.reLaunch({ url: '../index/index' })
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

	}
})