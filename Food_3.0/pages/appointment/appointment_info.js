// pages/appointment_info/appointment_info.js
var tool = require('../../template/Food2.0.js');
var template = require('../../template/template.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {

	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		this.setData({ storeAddress: app.globalData.storeAddress.storeaddress })
		tool.GetReservation(this)
		tool.GetReserveMenu(this)
	},
	show_listmodal: function () {
		this.setData({ listmodal: !this.data.listmodal })
	},
	cancelbook: function (e) {
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		template.commitFormId(formId, that)

		wx.showModal({
			title: '提示',
			content: '是否确认取消预约呢？',
			success: function (res) {
				if (res.confirm) {
					tool.resetappoint()
					tool.CancelResevation(that)
				}
			},
		})
	},
	openlocation: function () {
		tool.openLocation(app.globalData.storeAddress.storeLat, app.globalData.storeAddress.storeLng, 14)
	},
	navigate_home: function (e) {
		getApp().globalData.appoint_Id = 0
		tool.resetappoint()
		wx.navigateBack({ delta: 1 })
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
		tool.GetReservation(this)
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