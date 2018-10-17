// pages/valuepage/valuepage.js
var template = require("../../template/template.js");
var app = getApp();
var util = require("../../utils/util.js");
Page({

  /**
   * 页面的初始数据
   */
	data: {
	},
	payhistroy: function () {
		template.goNewPage('../payhistroy/payhistroy')
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		template.getSaveMoneySetList(this)
		template.getSaveMoneySetUser(this)
	},
	go_pay: function (e) {
		var that = this
		var id = e.currentTarget.id
		template.addSaveMoneySet(that, id, function (e) {
			if (e.data.isok == true) {
				var newparam = {
					openId: app.globalData.userInfo.openId,
					orderid: e.data.orderid,
					'type': 1,
				}
				util.PayOrder(e.data.orderid, newparam, {
					failed: function (res) {
						if (res == 'failed') {
							template.showmodal('提示', '您取消了支付', false)
						}
					},
					success: function (res) {
						if (res == "wxpay") {
							console.log(res)
						} else if (res == "success") {
							template.getSaveMoneySetUser(that)
							template.UpdateWxCard(that)
						}
					}
				})
			}
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
		template.getSaveMoneySetUser(this)
		template.getSaveMoneySetList(this)
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