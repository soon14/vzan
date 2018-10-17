var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		payType: 0,//1是微信支付 2是储值支付
		buyMode: 0,//用来判断用什么方法查询订单详情 正常商品无论用什么支付方式都是用旧接口，砍价商品用砍价接口查询
		dbOrder: 0,
		orderId: 0,
		totalMoeny: 0,
		loading: 0,
		pageindex: 0,
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var dbOrder = app.globalData.dbOrder
		this.setData({ totalMoeny: options.totalMoney, orderId: options.orderId, dbOrder: dbOrder, payType: app.globalData.payType })
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

	},

})