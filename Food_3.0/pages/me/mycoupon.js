// pages/me/mycoupon.js
var template = require('../../template/template.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		couponState: [
			{ name: "未使用", value: 0 },
			{ name: "已使用", value: 1 },
			{ name: "已过期", value: 2 }
		],
		state: 0,
		pageindex: 1,
		couponList: [],
		ismyCoupon: true, //区别模版
		isall: false,//是否已经加载所有
	},
	go_home: function () {
		wx.switchTab({ url: '../home/home', })
	},
	go_Couponcenter: function () {
		wx.navigateTo({ url: '../me/getCouponcenter', })
	},
	choosestate: function (e) {
		this.setData({ state: e.currentTarget.dataset.state, isall: false })
		this.data.pageindex = 1
		this.loadmyCoupon(e.currentTarget.dataset.state, 0)
	},
	loadmyCoupon: function (state, isreachbottom) {
		var that = this
		template.GetMyCouponList({
			appId: app.globalData.appid,
			userId: app.globalData.userInfo.UserId,
			pageIndex: that.data.pageindex,
			state: state,
			goodsId: '',
		}).then(function (res) {
			if (res.isok == true) {
				if (isreachbottom == 1) {
					if (res.postdata.length > 0) {
						that.data.couponList = that.data.couponList.concat(res.postdata)
						that.data.pageindex++
					} else {
						that.data.couponList = that.data.couponList
						that.data.isall = true
					}
				}

				else {
					that.data.pageindex++
					that.data.couponList = res.postdata
				}
				that.setData({ couponList: that.data.couponList, isall: that.data.isall })
				wx.hideLoading()
			}
		});
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		wx.showLoading({ title: '数据加载中...', })
		this.loadmyCoupon(this.data.state, 0)
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
		this.setData({ state: 0, isall: false })
		this.data.pageindex = 1
		this.loadmyCoupon(0, 0)
		template.stopPullDown()
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		this.loadmyCoupon(this.data.state, 1)
	},

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	}
})