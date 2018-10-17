// pages/Me/Me.js
var template = require('../../template/template.js')
var tool = require('../../template/Pediluvium.js')
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		unreadmsgcount: 0,
	},
	navo_webview: function () {
		wx.navigateTo({
			url: '/pages/Me/web_view?id=' + this.data.AgentConfig.QrcodeId,
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
	},
	// 改变技师状态
	changenowState: function (e) {
		var that = this
		wx.showActionSheet({
			itemList: ['空闲', '上钟', '休息中'],
			success: function (res) {
				if (res.tapIndex != undefined) {
					if (res.tapIndex == 2) {
						tool.ChangeTechnicianState(that, 3)
					} else {
						tool.ChangeTechnicianState(that, res.tapIndex)
					}
				}
			},
			fail: function (res) {
				console.log(res.errMsg)
			}
		})
	},
	goEditmyinfo: function () {
		template.goNewPage('../editMyinfo/editMyinfo')
	},
	openContact: function () {
		template.goNewPage('../im/contact')
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
		var app = getApp();
		var that = this;
		template.GetAgentConfigInfo(that)
		app.getUserInfo(function (e) {
			wx.hideLoading()
			tool.GetTechInfo(that, 0)
		})
		wx.getStorage({
			key: 'unreadmsgcount',
			success: function (res) {
				var num = Number(res.data) || 0;
				if (num > 0) {

					wx.showTabBarRedDot({
						index: 3,
					})
				}
				else {
					wx.hideTabBarRedDot({
						index: 3,
					})

				}
				that.setData({
					"unreadmsgcount": num
				});
			},
		})
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
		tool.GetTechInfo(this)
		template.stopPullDown()
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {

	},

})