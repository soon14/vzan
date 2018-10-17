// pages/store/orderList/orderIndex.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		entry_list: [
			{ fontcss: 'dzicon icon-icon_dianpudingdan icon_base el-3', logourl: '', content: '全部订单', nums: '0', css: 'ml20 h100 bg_bottom_line', state: -999, datetype: -999 },
			{ fontcss: '', logourl: 'http://i.vzan.cc/image/png/2018/6/25/1524008a89d62e405b4bc4b7e88c178d5b6ade.png', content: '7日订单', nums: '0', css: 'ml20 h100 bg_bottom_line', state: -999, datetype: 1 },
			{ fontcss: '', logourl: 'http://i.vzan.cc/image/png/2018/6/25/152420a73d3a9cc5304046ae764b116304d05b.png', content: '今日订单', nums: '0', css: 'ml20 h100', state: -999, datetype: 2 },
			{ fontcss: 'dzicon icon-icon_daishouhuo- icon_base el-6', logourl: '', content: '待发货', nums: '0', css: 'pl20 bg_bottom_line bg_bt10 h110', state: 1, datetype: -999 },
			{ fontcss: 'dzicon icon-icon_daifahuo- icon_base el-6', logourl: '', content: '待收货', nums: '0', css: 'ml20 bg_bottom_line h100', state: 2, datetype: -999 },
			{ fontcss: 'dzicon icon-icon_daiziqu icon_base el-6', logourl: '', content: '待自取', nums: '0', css: 'ml20 h100', state: 3, datetype: -999 },
			{ fontcss: '', logourl: 'http://i.vzan.cc/image/png/2018/6/25/15244205134367cd384ab79e158cadcccc994e.png', content: '交易成功', nums: '0', css: 'pl20 bg_bt20 h120', state: 4, datetype: -999 },
		],
	},

	orderlist_nt: function (e) {
		var state = e.currentTarget.dataset.state
		var datetype = e.currentTarget.dataset.datetype
		template.gonewpage('/pages/store/orderList/orderList?state=' + state + '&datetype=' + datetype)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		app.login(function () {
			var data = {}
			data.aid = app.globalData.aid
			data.storeId = options.storeid
			http.gRequest(addr.OrderTotal, data, function (callback) {
				var _e = that.data.entry_list
				var _n = callback.data.obj
				_e[0].nums = _n.total
				_e[1].nums = _n.weekTotal
				_e[2].nums = _n.todayTotal
				_e[3].nums = _n.sendTotal
				_e[4].nums = _n.receiveTotal
				_e[5].nums = _n.zqTotal
				_e[6].nums = _n.successTotal
				that.setData({ entry_list: _e })
			})
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
})