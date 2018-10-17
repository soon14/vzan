// pages/getFlower/getFlower.js
var tool = require('../../template/Pediluvium.js')
var template = require('../../template/template.js')
Page({

  /**
   * 页面的初始数据
   */
	data: {
		typeid: 0,//控制头部选择类型样式
		item: [//头部选择类型
			{ txt: '全部', id: 0 },
			{ txt: '今天', id: 1 },
			{ txt: '昨天', id: 2 },
			{ txt: '最近7天', id: 3 },
			{ txt: '最近30天', id: 4 },
		],
		pageindex: 1,
	},
	// 数据类型
	changeType: function (e) {
		this.setData({ typeid: e.currentTarget.id })
		tool.GetMyGifts(this, e.currentTarget.id, 1, 0)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		tool.GetMyGifts(this, 0, 1, 0)
		tool.GetMyGiftsCount(this)
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
		tool.GetMyGiftsCount(this)		
		tool.GetMyGifts(this, this.data.typeid, 1, 0)
		template.stopPullDown()
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		tool.GetMyGifts(this, this.data.typeid, this.data.pageindex, 1)
	},

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	}
})