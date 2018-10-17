// pages/store/myFollow/myFollow.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
Page({

  /**
   * 页面的初始数据
   */
	data: {
		header: [
			{ content: '商品', id: 0 },
			{ content: '店铺', id: 1 },
		],
		headerType: 0,

		item1: [
			{ slogo: '/image/ky.png', sname: '天猫超市', snums: '30', sold: '11' }
		]
	},
	changeType: function (e) {
		var index = e.currentTarget.dataset.id
		this.setData({ headerType: this.data.header[index].id })
		this.beginload(index)
	},
	goodinfo_nt: function (e) {
		var gid = e.currentTarget.dataset.gid
		template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + gid)
	},
	store_nt: function (e) {
		var storeid = e.currentTarget.dataset.storeid
		template.gonewpage('/pages/store/storeIndex/storeIndex?storeid=' + storeid)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		this.beginload(0)
	},
	beginload: function (search_type) {
		var that = this
		var data = {};
    var g=getApp().globalData;
    data.aId = g.aid;
		data.type = search_type
		http.gRequest(addr.LikesList, data, function (callback) {
			that.setData({ likeList: callback.data.obj })
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