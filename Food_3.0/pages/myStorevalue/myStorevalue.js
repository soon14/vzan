// pages/myStorevalue/myStorevalue.js
var addr = require("../../utils/addr.js");
var util = require("../../utils/util.js");
var template = require("../../template/template.js");
var tool = require("../../template/Food2.0.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
	},
	// 跳转到我的账单记录
	navtomypayList: function () {
		wx.navigateTo({
			url: '../mypayList/mypayList',
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		this.setData({ userimg: app.globalData.userInfo.avatarUrl })
		template.GetVipInfo(this, function () { })
		template.getSaveMoneySetList(this)
		template.getSaveMoneySetUser(this)//获取储值余额
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
		template.getSaveMoneySetList(this)
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



	saveMoney: function (e) {
		template.addSaveMoneySet(this, e.currentTarget.id)
	},




	wxpaymoney: function (oradid) {
		var that = this
		var oradid = oradid
		var newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: oradid,
			'type': 1,
		}
		util.PayOrder(oradid, newparam, {
			failed: function (res) {
				wx.showToast({ title: '您取消了支付！', icon: 'loading' })
			},
			success: function (res) {
				if (res == "wxpay") {
				} else if (res == "success") {
					wx.showToast({
						title: '支付成功',
						duration: 500
					})
					template.GetVipInfo(that, function () { })
					template.getSaveMoneySetUser(that) //获取储值余额
				}
			}
		})
	},
})