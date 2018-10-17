// pages/watermark/watermark.js
import { http, addr } from "../../modules/core.js";
Page({

  /**
   * 页面的初始数据
   */
	data: {
		phonenumber: '',//手机号码
		username: '',//联系人名字
		btnState: 0,

		about: [
			'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
			'/image/1.png',
			'/image/1.png',
			'/image/1.png',
			'/image/1.png',
		],
		solve: [
			{
				a: [{ topic: '专业版', content: '专业版，具备行业版的自定义特点，内置16种功能组件，其中包括视频，轮播图，背景音乐，在线客服，以及电商直播。商家可以根据行业的需求选择组件，自由搭配。行业拥有丰富的营销插件——会员卡储值，会员折扣，砍价，拼团等。可以做成多种电商形式——内容电商，直播电商，社交电商' }],
				b: [
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
				]
			},
			{
				a: [{ topic: '行业版', content: '行业版，适用于所有行业。内置14种功能组件，其中包括视频，轮播图，背景音乐，在线客服等，用户可以根据不同行业的场景需求，自由选择搭配组件。目前已有“餐饮，家政，酒店，ktv，美容美发美甲，健身房，建材，教育，汽车，房地产，宠物……”等十几个行业模板案例。' }],
				b: [
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
				]
			},
			{
				a: [{ topic: '餐饮版', content: '餐饮版，类似于美团、大众点评，可实现线上外卖，扫码点餐功能，生成点餐小程序，实现一桌一码，极大方便了菜品订单管理。连接后厨打印机，无需服务员参与，下单自动出票，能有效提高工作效率。随着“附近的小程序”功能的推出，小程序成为餐饮行业重要的客户流量渠道。' }],
				b: [
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
				]
			},
			{
				a: [{ topic: '电商版', content: '电商版，可实现商品展示，排序，下单，功能齐全，可通过微信二维码直接进入，支持会员卡储值，会员卡折扣，砍价等营销插件，模块化设置，商家只需简单设置即可拥有自己的商城，省时省力！除此之外，还支持连接云打印机，支付完成就能打出小票。从线上到线下无缝衔接。' }],
				b: [
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
				]
			},
			{
				a: [{ topic: '企业版', content: '企业官网版相当于企业的网络名片，可以有效增加企业的曝光度。不但能满足企业宣传的需求，同时还可以辅助企业销售。企业可以利用小程序来展示产品、案例、宣传企业文化、发布资讯。' }],
				b: [
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
				]
			},
			{
				a: [{ topic: '单页版', content: '市面上功能最完善的单页版，附近5公里自动展示，本地商家的网络名片。' }],
				b: [
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
					'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
				]
			}
		]
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {

	},

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
	onReady: function () {

	},
	// 查看关于我们大图集
	previewAbout: function (e) {
		var imageArray = e.currentTarget.dataset.about
		var index = e.currentTarget.id
		console.log(imageArray)
		var previewImage = imageArray[index]
		console.log(previewImage)
		wx.previewImage({
			current: previewImage,
			urls: imageArray
		})
	},
	// 输入手机号码
	input_number: function (e) {
		this.setData({ phonenumber: e.detail.value })
	},
	// 输入联系人名字
	input_name: function (e) {
		this.setData({ username: e.detail.value })
	},
	// 联系电话
	phoneCall: function () {
		wx.makePhoneCall({
			phoneNumber: this.data.number,
		})
	},

	// 打开地图
	getLocation: function (e) {
		wx.openLocation({
			latitude: Number(e.currentTarget.dataset.lat),
			longitude: Number(e.currentTarget.dataset.lng),
			scale: 14
		})
	},
	submit_form: function () {
        var that = this
		var data = {
			username: that.data.username,
			phone: that.data.phonenumber,
			source: 1,
			type: 5
		}
		if (that.data.username == '' || Number(that.data.phonenumber.length != 11)) {
			wx.showToast({
				title: '信息有误',
				icon: 'loading'
			})
		} else {
            http.post(addr.SaveFeedback, data, function (res) {
				if (res.data.msg == '发送成功') {
					that.setData({ username: '', phonenumber: '', btnState: 1 })
					wx.showToast({
						title: '提交成功',
						icon: 'success'
					})
				}
			})
		}
	},
	unsubmit: function () {
		wx.showToast({
			title: '已成功预约',
			icon: 'loading'
		})
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