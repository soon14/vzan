// pages/me/me.js
var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var app = getApp();
var template = require('../../template/template.js');
var tool = require('../../template/Food2.0.js');
Page({

  /**
   * 页面的初始数据
   */
	data: {
		islogin: false,
		canSaveMoneyFunction: false,//储值开关
		iscloseBtn: 0,//判断领取会员卡按钮开关 0关 1开
		AccountMoneyStr: 0,//储值余额
		isOnload: 0,//控制onwho
		isshowVip: false,// 会员卡弹窗
		takeout: 99,
		TelePhone: 0,
		TablesNo: 0,
	},
	navo_webview: function () {
		wx.navigateTo({
			url: '/pages/me/web_view?id=' + this.data.AgentConfig.QrcodeId,
		})
	},
	getUserInfo: function (e) {
		var that = this
		var _e = e.detail
		if (e.detail.errMsg != 'getUserInfo:fail auth deny') {
			wx.login({
				success: function (res) {
					app.login(res.code, _e.encryptedData, _e.signature, _e.iv, function (cb) {
						that.setData({ userinfo: wx.getStorageSync('userInfo'), islogin: true })
					}, 0)
				}
			})
		} else {
			wx.showModal({
				title: '提示',
				content: '你拒绝了登录授权，请再次点击登录进行操作。',
				showCancel: false
			})
		}
	},
	// 领取微信会员卡
	getvipCard: function () {
		var that = this
		template.GetCardSign(that)
	},
	// 跳转到储值有礼页面
	navtomyStorevalue: function () {
		wx.navigateTo({ url: '../myStorevalue/myStorevalue', })
	},
	// 跳转到我的地址页面
	navtoAddress: function () {
		wx.navigateTo({ url: '../setAddress/setAddress?openId=' + this.data.userinfo.openId + '&isMe=1', })
	},
	navtoMycoupon: function () {
		wx.navigateTo({ url: '../me/mycoupon', })
	},
	navtomygrouplist: function () {
		wx.navigateTo({
			url: '../group/mygroupList',
		})
	},
	// 会员卡弹窗
	showVipmodal: function () {
		this.setData({ isshowVip: !this.data.isshowVip })
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		var takeout = app.globalData.TakeOut
		that.data.TelePhone = app.globalData.TelePhone
		app.new_login(function (e) {
			if (e) {
				template.GetWxCardCode(that) //判断是否已领取过卡
				template.getSaveMoneySetUser(that)//获取储值余额
				template.GetAgentConfigInfo(that) //刷新水印
				template.GetVipInfo(that, function () { })
				that.setData({ takeout: takeout, TablesNo: app.globalData.TablesNo })
			}
		})
	},
	// 联系客服
	makephoneCall: function () {
		template.makePhoneCall(this.data.TelePhone)
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
		template.UpdateWxCard(this)
		tool.GetFoodsDetail(this, 2)
		if (this.data.isOnload != 0) {
			template.getSaveMoneySetUser(this) //获取储值余额
			template.GetAgentConfigInfo(this) //刷新水印
			template.GetVipInfo(this, function () { })
		}
		this.data.isOnload++
		this.setData({ userinfo: wx.getStorageSync('userInfo'), islogin: wx.getStorageSync('userInfo').avatarUrl == null ? false : true })
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
		tool.GetFoodsDetail(this, 2)
		template.GetWxCardCode(this) //判断领取卡卷按钮
		template.UpdateWxCard(this)
		template.getSaveMoneySetUser(this)//获取储值余额
		template.GetAgentConfigInfo(this) //刷新水印
		template.GetVipInfo(this, function () { })
		setTimeout(function () {
			wx.showToast({
				title: '刷新成功',
			})
			wx.stopPullDownRefresh()
		}, 500);

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