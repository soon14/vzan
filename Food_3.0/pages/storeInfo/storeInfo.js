// pages/storeInfo/storeInfo.js
var template = require('../../template/template.js');
var tool = require('../../template/Food2.0.js');
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		ShippingFeeStr: '',
		storeImgs: [
			'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
			'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
			'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
			'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
		],
		couponList: []

	},
	getcoupon: function (e) {
		template.GetCoupon(this, e.currentTarget.dataset.couponid)
	},
	makePhoneCall: function () {
		template.makePhoneCall(this.data.data.food.TelePhone)
	},
	navigation: function () {
		tool.openLocation(this.data.data.food.Lat, this.data.data.food.Lng, 14)
	},
	previewStoreImgs: function (e) {
		tool.previewStoreimg(e, this.data.data.storeImgs)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		that.setData({
			ShippingFeeStr: app.globalData.ShippingFeeStr,
		})
		app.new_login(function (e) {
			tool.GetFoodsDetail(that, 3)
			template.GetStoreCouponList(that, -1)
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
		tool.GetFoodsDetail(this, 3)
		template.GetStoreCouponList(this, -1)
		template.stopPullDown()
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