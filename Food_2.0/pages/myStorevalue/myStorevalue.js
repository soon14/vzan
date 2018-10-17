// pages/myStorevalue/myStorevalue.js
var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		AccountMoneyStr: 0,//储值余额
		saveMoneySetList: [],//储值列表
		model: [],//会员权益集合
		// 储值列表
		item: [
			{ txt: '储值', chong: '100.10', song: '999.99' },
			{ txt: '储值', chong: '100.10', song: '999.99' },
			{ txt: '储值', chong: '100.10', song: '999.99' },
			{ txt: '储值', chong: '100.10', song: '999.99' },
			{ txt: '储值', chong: '100.10', song: '999.99' },
			{ txt: '储值', chong: '100.10', song: '999.99' },
		]
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
		util.GetVipInfo(this)
		this.getSaveMoneySetList()
		util.getSaveMoneySetUser(this)//获取储值余额
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
		this.getSaveMoneySetList()
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

	// 获取储值列表
	getSaveMoneySetList: function () {
		var that = this
		wx.request({
			url: addr.Address.getSaveMoneySetList, //仅为示例，并非真实的接口地址
			data: {
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId
			},
			method: "GET",
			header: {
				'content-type': 'application/x-www-form-urlencoded' // 默认值
			},
			success: function (res) {
				if (res.data.isok) {
					that.setData({
						saveMoneySetList: res.data.saveMoneySetList,
					})
				}
			},
			fail: function () {
				console.log('获取不了会员信息')
			}
		})
	},
	saveMoney: function (e) {
		var index = e.currentTarget.id
		this.addSaveMoneySet(index)
	},
	// 请求预充值
	addSaveMoneySet: function (saveMoneySetId) {
		var that = this
		wx.request({
			url: addr.Address.addSaveMoneySet, //仅为示例，并非真实的接口地址
			data: {
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				saveMoneySetId: saveMoneySetId
			},
			method: "POST",
			header: {
				'content-type': 'application/x-www-form-urlencoded' // 默认值
			},
			success: function (res) {
				if (res.data.isok) {
					var orderid = res.data.orderid
					that.wxpaymoney(orderid)
				}
			},
			fail: function () {
				console.log('获取不了会员信息')
			}
		})
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
				wx.showModal({
					title: '提示',
					content: res
				})
			},
			success: function (res) {
				if (res == "wxpay") {
				} else if (res == "success") {
					wx.showToast({
						title: '支付成功',
						duration: 500
					})
					util.GetVipInfo(that)
					util.getSaveMoneySetUser(that) //获取储值余额
				}
			}
		})
	},
})