// pages/home/home.js
var template = require("../../template/template.js")
var tool = require("../../template/Pediluvium_Client.js")
Page({

  /**
   * 页面的初始数据
   */
	data: {
	},
	makephonecall: function () {
		template.makePhoneCall(this.data.data.TelePhone)
	},
	navAddress: function (e) {
		tool.openLocation(e.currentTarget.dataset.lat, e.currentTarget.dataset.lng, 14)
	},
	previewImg: function (e) {
		tool.previewStoreimg(e, this.data.data.photoList)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {

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
		tool.GetStoreInfo(this)
		template.GetAgentConfigInfo(this)
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
		tool.GetStoreInfo(this)
		template.GetAgentConfigInfo(this)
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