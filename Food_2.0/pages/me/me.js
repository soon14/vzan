// pages/me/me.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		AgentConfig:[],//水印开关集合
		canSaveMoneyFunction: false,//储值开关
		iscloseBtn: 0,//判断领取会员卡按钮开关 0关 1开
		vipcard: [],//卡套集合
		havecard: [],//若为null则显示取卡按钮
		goodslist: [],//优惠商品名称
		AccountMoneyStr: 0,//储值余额
		isOnload: 0,//控制onwho
		model: [],//会员权益集合
		isshowVip: false,// 会员卡弹窗
		userinfo: [],
		openId: '',
		takeout: 99,
		TelePhone: 0,
		TablesNo: 0,
	},
	// 领取微信会员卡
	getvipCard: function () {
		var that = this
		util.GetCardSign(that)
	},
	// 跳转到储值有礼页面
	navtomyStorevalue: function () {
		wx.navigateTo({
			url: '../myStorevalue/myStorevalue',
		})
	},
	// 跳转到我的地址页面
	navtoAddress: function () {
		wx.navigateTo({
			url: '../setAddress/setAddress?openId=' + this.data.openId + '&isMe=1',
		})
	},
	// 会员卡弹窗
	showVipmodal: function () {
		this.setData({ isshowVip: !this.data.isshowVip })
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		var takeout = app.globalData.TakeOut
		var TelePhone = app.globalData.TelePhone
		app.getUserInfo(function (e) {
			util.GetWxCardCode(that) //判断是否已领取过卡
			util.getSaveMoneySetUser(that)//获取储值余额
			util.GetAgentConfigInfo(that) //刷新水印
			util.GetVipInfo(that)
			that.setData({ userinfo: app.globalData.userInfo, openId: e.openId, takeout: takeout, TelePhone: TelePhone, TablesNo: app.globalData.TablesNo})
		})
	},
	// 联系客服
	makephoneCall: function () {
		wx.makePhoneCall({
			phoneNumber: this.data.TelePhone,
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
		util.UpdateWxCard(this)
		this.GetFoodsDetail()
		if (this.data.isOnload != 0) {
			util.getSaveMoneySetUser(this) //获取储值余额
			util.GetAgentConfigInfo(this) //刷新水印
			util.GetVipInfo(this)
		}
		this.data.isOnload++
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
		this.GetFoodsDetail()
		util.GetWxCardCode(this) //判断领取卡卷按钮
		util.UpdateWxCard(this)
		util.getSaveMoneySetUser(this)//获取储值余额
		util.GetAgentConfigInfo(this) //刷新水印
		util.GetVipInfo(this)
		setTimeout(function () {
			wx.showToast({
				title: '刷新成功',
			})
			wx.stopPullDownRefresh()
		}, 500);

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

	},
	// 获取首页显示数据
	GetFoodsDetail: function (e) {
		var that = this
		wx.request({
			url: addr.Address.GetFoodsDetail,
			data: {
				AppId: app.globalData.appid,
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.setData({
						canSaveMoneyFunction: res.data.postdata.food.funJoinModel.canSaveMoneyFunction
					})
				}
			},
			fail: function () {
				console.log("获取信息出错")
				wx.showToast({
					title: '获取信息出错',
				})
			}
		})
	},
})