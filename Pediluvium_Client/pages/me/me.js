// pages/me/me.js
var template = require("../../template/template.js")
var tool = require("../../template/Pediluvium_Client.js")
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		iscloseBtn: 0,//判断领取会员卡按钮开关 0关 1开
		userInfo: [],//用户信息
		isshowVip: false,
		entry: [
			{ content: '预约记录', id: 0, url: '../bookrecord/bookrecord' },
			{ content: '送花记录', id: 1, url: '../flowerrecord/flowerrecord' },
			{ content: '联系客服', id: 2 },
			{ content: '门店地址', id: 3 },
		]
	},
	nato_webview: function () {
		wx.navigateTo({
			url: '/pages/me/web_view?id=' + this.data.AgentConfig.QrcodeId,
		})
	},
	goNewPage: function (e) {
		var index = e.currentTarget.id
		if (index == 0 || index == 1) {
			template.goNewPage(e.currentTarget.dataset.url)
		} else if (index == 2) {
			template.makePhoneCall(this.data.data.TelePhone)
		} else {
			tool.openLocation(this.data.data.Lat, this.data.data.Lng, 14)
		}
	},
	valuepage: function () {
		template.goNewPage('../valuepage/valuepage')
	},
	showvipModal: function () {
		this.setData({ isshowVip: !this.data.isshowVip })
	},
	getvipCard: function () {
		template.GetCardSign(this)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		template.GetWxCardCode(this) //判断是否已领取过卡
		this.setData({ userInfo: app.globalData.userInfo })
	},
	getLogin: function (e) {
		let that = this
		let _g = e.detail
		if (_g.errMsg == "getUserInfo:fail auth deny") {
			return;
		}
		wx.login({
			success: function (res) {
				let vm = {
					iv: _g.iv,
					code: res.code,
					data: _g.encryptedData,
					signature: _g.signature,
					isphonedata: 0,
				}
				app.login_old(vm, function (data) {
					that.setData({ userInfo: data })
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
		template.UpdateWxCard(this)
		template.getSaveMoneySetUser(this)
		template.GetVipInfo(this, 0)
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
		template.GetWxCardCode(this) //判断是否已领取过卡
		template.UpdateWxCard(this)
		template.getSaveMoneySetUser(this)
		template.GetVipInfo(this, 0)
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