// pages/paySuccess/paySuccess.js
var app = getApp();
var template = require('../../template/template.js');
Page({

  /**
   * 页面的初始数据
   */
	data: {
		showReduction: false,//立减金弹窗
	},
	// 跳转到订单详情
	navtoOrderinfo: function () {
		wx.redirectTo({
			url: '../orderInfo/orderInfo'
		})
	},
	_coupFunc: function (e) {
		switch (Number(e.currentTarget.id)) {
			case 0:
				this.setData({ showReduction: false })
				break;
			case 1:
				template.goNewPage('../addCoupon/getsmoney')
				break;
		}
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		if (app.globalData.reduction != null && app.globalData.payType == 1) {
			this.setData({ showReduction: true })
		}
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