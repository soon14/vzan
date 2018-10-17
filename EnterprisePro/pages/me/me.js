// pages/me/me.js
const app = getApp();
const util = require("../../utils/util.js");
const tools = require("../../utils/tools.js")
const page = require("../../utils/pageRequest.js")
const animation = require("../../utils/animation.js")

Page({

  /**
   * 页面的初始数据
   */
	data: {
		// 待付款 待发货 待收货 已完成
		classify: [
			{ icon: 'dzicon icon-daifukuan f47', txt: '待付款', state: 0 },
			{ icon: 'dzicon icon-daifahuo f47', txt: '待发货', state: 1 },
			{ icon: 'dzicon icon-daishouhuo f47', txt: '待收货', state: 2 },
			{ icon: 'dzicon icon-yiwancheng f47', txt: '已完成', state: 3 },
		],
		menu_entry: [
			{ title: "我的订单", url: "../myOrder/myOrder?condition=0" },
			{ title: "分销中心", url: "../sellCenter/index" },
			{ title: "我的购物车", url: "../shoppingCart/shoppingCart?" },
			{ title: "我的团购", url: "../myGroup/myGroup?" },
			{ title: "我的拼团", url: "../myGroup2/myGroup?isIndex1=", id: 4 },
			{ title: "我的砍价单", url: '../mycutprice/mycutprice? ' },
			{ title: "我的预约单", url: '../subscribe/subscribe?sublist=true' },
			// { title: "收货地址", url: "" },
			{ title: "收货地址", url: "../me/address" },
			{ title: "优惠券", url: "mycoupon?" },
			{ title: "积分中心", url: "../integralMall/integralMall?" },
			{ title: "预约购物", url: "" }
			//{ title: "客服", url: "../im/contact" },
		],
		skipJump: [
			{ title: "储值余额", url: "mystoredvalue?" },
			{ url: '../stored/stored? ' }
		],
		homeClose: true,
		vipcard: false,
		saveMoney: "0.00",
		pricestr: "0.00",
		showModalUser: false,
		status: { showModalStatus: false }
	},
	submitFormid: function (e) {
		var formId = e.detail.formId
		util.commitFormId(formId, this)
	},
  /**
  * 接口请求
  */
	onLoad: function (options) {
		let that = this
		page.logoRequest(that)
		util.setPageSkin(that);
		getApp().GetStoreConfig(function (config) {
			if (config && config.funJoinModel) {
				if (config.funJoinModel.imSwitch && config.kfInfo) {
					that.data.menu_entry.push({ title: "客服", url: "../im/contact" })
					that.data.funJoinModel = config.funJoinModel
					that.setData({
						"menu_entry": that.data.menu_entry
					});
				}
				// if (config.funJoinModel.reserveSwitch && config.funJoinModel.openInvite) {
				// 	that.data.menu_entry.push({ title: "预约购物", url: "" })
				// }
			}
		});
	},
	// 返回首页
	pagesGoto: function (e) {
		tools.goBackPage(1)
	},
	// 待付款...等跳转
	statusGoto: function (e) {
		let [index, state] = [Number(e.currentTarget.id), e.currentTarget.dataset.state]
		let url = "../myOrder/myOrder?condition=" + index + "&state=" + state
		tools.goNewPage(url)
	},
	//跳转
	addressGoto: function (e) {
		let url = e.currentTarget.dataset.url
		var appointmentUrl = Number(getApp().globalData.appoint_Id) != 0 && getApp().globalData.appoint_Id != 'null' && getApp().globalData.appoint_Id != null ?
			'../appointment/appointment_info' :
			'../appointment/appointment?AccountMoneyStr=' + this.data.saveMoney
		if (url == '') {
			if (this.data.funJoinModel.reserveSwitch && this.data.funJoinModel.openInvite) {
				tools.goNewPage(appointmentUrl)
			} else {
				if (Number(getApp().globalData.appoint_Id) > 0) {
					tools.goNewPage(appointmentUrl)
				} else {
					wx.showModal({
						title: '提示',
						content: '商家未开启预约购物功能！',
						showCancel: false
					})
					return
				}
			}
		} else {
			tools.goNewPage(url)
		}
	},

	// 会员权益弹窗
	showVipFunc: function (e) {
		let currentStatu = e.currentTarget.dataset.statu;
		animation.utilDown(currentStatu, this)
	},
	// 领取会员卡
	getWxCarFunc: function () {
		var that = this
		page.getCardSign(wx.getStorageSync("userInfo").UserId, that).then(data => {
			let cardext = { 'signature': data.obj.signature, 'timestamp': data.obj.timestamp }
			cardext = JSON.stringify(cardext)
			wx.addCard({
				cardList: [
					{
						cardId: data.obj.cardId,
						cardExt: cardext
					}
				],
				success: function (res) {
					console.log(res.cardList)
					page.saveCodeRequest(res.cardList[0].code, that)
				}
			})

		})
	},
	//拒绝授权时弹窗
	userFunc: function (e) {
		console.log(e)
		let id = Number(e.target.id)
		switch (id) {
			case 0:
				tools.goBackPage(1);
				break;
			case 1:
				wx.openSetting({
					success: (data) => {
						data.authSetting = { "scope.userInfo": true }
						this.setData({ showModalUser: false })
					}
				})
				break;
		}
	},
  /**
   * 生命周期函数--监听页面显示
   */
	onShow: function () {
		let that = this
		wx.getSetting({
			success: (res) => {
				if (res.authSetting["scope.userInfo"]) {
					let userInfo = wx.getStorageSync("userInfo")
					let memberInfo = wx.getStorageSync("memberInfo")
					if (userInfo && memberInfo) {
						that.setData({ model: memberInfo, user: userInfo })
						that.SyncUser()
					}
					else {
						that.getUserSeting()
					}
				}
				else {
					that.setData({ showModalUser: true })
					wx.hideLoading();
				}
			}
		})
	},
	//用户授权
	getUserSeting: function () {
		let that = this
		wx.showLoading({
			title: '加载中...',
			mask: true,
			success: function (load) {
				app.getUserInfo(res => {
					page.memberInfo(res.UserId, that).then(data => {
						getApp().globalData.appoint_Id = data.model.reservation
						that.setData({ model: data.model, user: res })
						wx.setStorageSync("memberInfo", data.model)
						wx.hideLoading();
					});
					page.userOfmoneyRequest(res.openId, that)
					page.updateCardRequest(res.UserId, that)
				});
			}
		})
	},
	//同步数据
	SyncUser: function () {
		let that = this
		app.getUserInfo(res => {
			page.memberInfo(res.UserId, that).then(data => {
				getApp().globalData.appoint_Id = data.model.reservation
				wx.setStorageSync("memberInfo", data.model)
			});
			page.userOfmoneyRequest(res.openId, that)
			page.updateCardRequest(res.UserId, that)
		});
	},
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
	onPullDownRefresh: function () {
		var that = this
		tools.showLoadToast("正在刷新")
		setTimeout(res => {
			tools.showToast("刷新成功")
			page.memberInfo(app.globalData.userInfo.UserId, that).then(data => {
				if (data.model.reservation == 0 || data.model.reservation == null) {
					tools.resetappoint()
				}
				getApp().globalData.appoint_Id = data.model.reservation
				that.setData({ model: data.model, pricestr: data.model.pricestr, user: res })
			});
			page.userOfmoneyRequest(app.globalData.userInfo.openId, that)
			page.updateCardRequest(app.globalData.userInfo.UserId, that)
		}, 1000)
		wx.stopPullDownRefresh()
	},
})