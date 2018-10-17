// pages/setAddress/setAddress.js
var addr = require("../../utils/addr.js");
var app = getApp();
var tool = require('../../template/Food2.0.js');
Page({

  /**
   * 页面的初始数据
   */
	data: {

		addressInfo: '',//详细地址
		ifismeis: 5
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		if (options.isMe != undefined) {
			that.setData({ ifismeis: parseInt(options.isMe) })
		}
		app.new_login(function (e) {
			that.setData({ addressInfo: app.globalData.addressInfo })
			tool.GetMyAddress(that)
		})
	},
	// 定位
	chooseLocation: function () {
		var that = this
		wx.chooseLocation({
			success: function (res) {
				var weidu = res.latitude
				var jingdu = res.longitude
				var addressInfo = res.address
				app.globalData.weidu = weidu
				app.globalData.jingdu = jingdu
				app.globalData.addressInfo = addressInfo
				console.log("显示的是addressInfo")
				console.log(addressInfo)
				that.setData({
					addressInfo: addressInfo
				})
			},
		})
	},
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
	onReady: function () {

	},
	// 跳转到收货地址编辑页面
	navtoEditorAddress: function (e) {
		wx.navigateTo({ url: '../editorAddress/editorAddress?Id=' + e.currentTarget.id })
	},
  /**
   * 生命周期函数--监听页面显示
   */
	onShow: function () {
		tool.GetMyAddress(this)
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
	navtoupdateOrder: function (e) {
		var dizhiId = e.currentTarget.id
		var ifismeis = this.data.ifismeis
		app.globalData.dizhiId = dizhiId
		if (ifismeis == 5) {
			wx.navigateBack({ url: '../updateOrder/updateOrder' })
		}
	},
  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	},

})