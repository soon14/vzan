// pages/my/Address/editmyAddress.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		user: {
			id: 0,
			state: 1,
			consignee: '', //姓名
			mobile: '', //电话
			province: '',//省
			city: '',//市
			district: '',//区
			address: '',//详细地址
			isDefault: 0,//是否默认地址 0false 1true
			// region: ['', '', ''], //省市区
			lat: '', //经度
			lng: '', //纬度
		},
		region: ['', '', ''],
	},
	inputUsername: function (e) {
		this.setData({ 'user.consignee': e.detail.value })
	},
	inputUserphone: function (e) {
		this.setData({ 'user.mobile': e.detail.value })
	},
	bindRegionChange: function (e) {
		console.log('picker发送选择改变，携带值为', e.detail.value)
		this.setData({
			'user.province': e.detail.value[0],
			'user.city': e.detail.value[1],
			'user.district': e.detail.value[2]
		})
	},
	inputBuchong: function (e) {
		this.setData({ 'user.address': e.detail.value })
	},
	save_myaddress: function () {
		var that = this
    var g = getApp().globalData;
		var _d = that.data.user
		var data = {}
		data.id = _d.id //地址id 新增地址0 编辑地址！=0
    data.userId = g.userInfo.UserId//用户userid
		data.consignee = _d.consignee//用户名字
		data.mobile = _d.mobile//手机号码
		data.province = _d.province//省
		data.city = _d.city//市
		data.district = _d.district//区
		data.address = _d.address//详细地址
		data.state = _d.state
		data.isDefault = _d.isDefault
		http.gRequest(addr.addAddress, data, function (callback) {
			template.showtoast('添加成功', 'success')
			setTimeout(() => {
				template.goback(1)
			}, 500)
		})
	},
	delAddress: function () {
		var that = this
		var _d = that.data.user
		if (_d.id > 0) {
			wx.showModal({
				title: '提示',
				content: '是否确认删除该地址？',
				success: function (res) {
					if (res.confirm) {
						http.gRequest(addr.deleteAddress, { id: _d.id }, function () {
							template.showtoast('删除成功', 'success')
							setTimeout(() => {
								template.goback(1)
							}, 500)
						})
					}
				},
			})
		}
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		if (options.item) {
			this.setData({ user: JSON.parse(options.item) })
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