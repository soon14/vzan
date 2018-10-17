// pages/pay/pay.js
var tool = require("../../template/Pediluvium_Client.js")
var template = require("../../template/template.js")
var util = require("../../utils/util.js")
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		payway: 1,
		canSaveMoneyFunction: false,
	},
	choosepayway: function (e) {
		this.setData({ payway: e.currentTarget.id })
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		tool.GetStoreInfo(this, 1)
		this.setData({ command: JSON.parse(options.command) })
	},

	book_to_pay: function (e) {
		var that = this
		template.commitFormId(e.detail.formId) //提交备用formid
		var command = that.data.command
		getApp().globalData.priceT = command.priceT
		var param = {
			appid: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			userId: app.globalData.userInfo.UserId,
			tid: command.Tid,
			serviceId: command.Sid,
			serviceTime: command.time,
			phone: command.phoneNumber,
			payWay: that.data.payway,
			message: command.message
		}
		util.AddOrder(param, function (refun, command) {
			if (refun == 'failed') {
				var command = JSON.parse(command)
				tool.CancelPay(command.tid, command.serviceTime, app.globalData.orderid, function (cb) {
					if (cb == '更新成功') {
						wx.showModal({
							title: '提示',
							content: '您取消了支付该预约订单',
							showCancel: false,
							confirmColor: '#fe536f',
							success: function (res) {
								if (res.confirm) {
									template.UpdateWxCard(that)
									setTimeout(function () {
										template.goBackPage(2)
									}, 500)
								}
							}
						})
					}
				})
			} else {
				template.goNewPage('../paysuccess/paysuccess?payway=' + that.data.payway)
			}
		})

	},

	cancel_book: function (e) {
		template.commitFormId(e.detail.formId) //提交备用formid
		template.showmodal('提示', '是否取消预约返回技师主页？', true, 1)
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