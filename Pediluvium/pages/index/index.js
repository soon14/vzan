// pages/index/index.js
var util = require("../../utils/util.js");
var template = require('../../template/template.js');
var tool = require('../../template/Pediluvium.js')
var intervalid
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		pageCondition: 0,//0未绑定手机号码 1绑定手机号码ing 2绑定了手机但未绑定微信 ，若以上均完善则跳转至staging
		phone: '',
		code: '',
		Reciprocal: '',
		reducetime: 60,
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		if (options.pageCondition == undefined) {
			// wx.showLoading({
			// 	title: '加载中'
			// })
			// app.getUserInfo(function (e) {
			// 	wx.hideLoading()
			// 	tool.GetTechInfo(that, 0)
			// })
		} else {
			that.setData({ pageCondition: options.pageCondition })
		}
	},
	// 输入手机号码
	phoneinput: function (e) {
		this.setData({ phone: e.detail.value })
	},
	codeinput: function (e) {
		this.setData({ code: e.detail.value })
	},
	getCode: function () {
		var that = this
		if (that.data.phone == '') {
			template.showtoast('请输入手机号码', 'loading')
			return false;
		}
		if (that.data.phone.length != 11) {
			template.showtoast('手机号码有误', 'loading')
			return false;
		}
		if (that.data.reducetime != 60) {
			return false;
		}
		intervalid = setInterval(function () {
			if (that.data.reducetime == 1) {
				clearInterval(intervalid);
				that.data.reducetime = 60;
				that.setData({ Reciprocal: '' })
				return false;
			}
			that.data.reducetime--;
			that.setData({ Reciprocal: that.data.reducetime + 's' })
		}, 1000)
		tool.SendUserAuthCode(that, that.data.phone)
	},
	setpageCondition: function () {
		template.goNewPageByRd('../index/index?pageCondition=1')
	},
	binduserphone: function () {
		if (this.data.phone == '' || this.data.code == '') {
			return
		} else {
			tool.BindPhoneNumber(this, this.data.phone, this.data.code)
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
		var that = this
		wx.showLoading({
			title: '加载中'
		})
		app.GetUserInfo(function (e) {
			wx.hideLoading()
			tool.GetTechInfo(that, 0)
		})
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