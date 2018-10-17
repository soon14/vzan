// pages/bookrecord/bookrecord.js
var template = require('../../template/template.js')
var tool = require('../../template/Pediluvium_Client.js')
Page({

  /**
   * 页面的初始数据
   */
	data: {
		ordertype: 0, //待使用1 --- else 0
		pageIndex: 1,
		typeChoose: [
			{ content: '全部', sortid: -1 },
			{ content: '待使用', sortid: 4 },
			{ content: '已完成', sortid: 6 }
		],
		sortId: -1,
	},
	choosesortId: function (e) {
		wx.pageScrollTo({
			scrollTop: 0,
		})
		this.setData({ sortId: e.currentTarget.id })
		if (e.currentTarget.id == 4) {
			tool.GetOrderRecord(this, 1, 6, this.data.sortId, 0, 1)
			this.data.ordertype = 1
		} else {
			tool.GetOrderRecord(this, 1, 6, this.data.sortId, 0, 0)
			this.data.ordertype = 0
		}
	},
	projectInfo: function (e) {
		template.goNewPage('../projectinfo/projectinfo?id=' + e.currentTarget.id)
	},
	artificerInfo: function (e) {
		template.goNewPage('../artificerInfo/artificerInfo?id=' + e.currentTarget.id)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		tool.GetOrderRecord(this, this.data.pageIndex, 6, this.data.sortId, 0, 0)
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
		tool.GetOrderRecord(this, 1, 6, this.data.sortId, 0, this.data.ordertype)
		template.stopPullDown()
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		tool.GetOrderRecord(this, this.data.pageIndex, 6, this.data.sortId, 1, this.data.ordertype)
	},

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	}
})