// pages/booklist/booklist.js
var template = require('../../template/template.js')
var tool = require('../../template/Pediluvium_Client.js')
var app = getApp()
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
		tool.GetStoreInfo(this, 0)
		this.setData({ command: JSON.parse(options.command) })
	},
	inpuNumber: function (e) {
		this.data.command.phoneNumber = e.detail.value
		this.setData({ command: this.data.command })
	},
	inputMessage: function (e) {
		this.data.command.message = e.detail.value
		this.setData({ command: this.data.command })
	},
	navtopay: function (e) {
		template.commitFormId(e.detail.formId) //提交备用formid
		console.log('command', this.data.command)
		if (this.data.command.phoneNumber == undefined || Number(this.data.command.phoneNumber.length != 11)) {
			template.showtoast('手机号码错误', 'loading')
			return
		} else if (this.data.data.switchModel.WriteDesc == true && this.data.command.message == undefined || this.data.command.message == '') {
			template.showtoast('请填写备注', 'loading')
			return
		} else {
			var command = JSON.stringify(this.data.command)
			template.goNewPage('../pay/pay?command=' + command)
		}
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