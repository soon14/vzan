// pages/myOrder/myOrder.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
var C_Enum = app.C_Enum;
Page({

  /**
   * 页面的初始数据
   */
	data: {
		array: ['微信支付', '储值支付'],//选择付款模式
		index1: 2,//选择付款模式下标
		condition: 10, // 标题颜色，默认为10
		orderId: 0,
		year: '2017',
		choose: [
			{ title: '全部订单', state: 10 },
			{ title: '待付款', state: 0 },
			{ title: '待发货', state: 3 },
			{ title: '待收货', state: 5 },
			{ title: '已完成', state: 6 }],
		// 全部订单 condition== 0 时
		allList: [
		],
		pageIndex: 1,
		pageSize: 6,
		bottommsg: "",
		state: 10,
		Loading: 0,//是否在加载数据中，1：加载中，0没有加载
		//屏幕高度
		sliderHeight: 600,
	},
	// 选择付款模式
	bindPickerChange: function (e) {
		var that = this
		var index = e.currentTarget.id
		if (e.detail.value == 0) {
			var orderId = that.data.allList[0].orderList[index].cityMorderId
		} else {
			var orderId = that.data.allList[0].orderList[index].Id
		}
		var BuyPrice = that.data.allList[0].orderList[index].BuyPrice
		that.setData({ BuyPrice: BuyPrice })
		getApp().globalData.dbOrder = orderId
		var orderList = that.data.allList[0].orderList
		var oradid = orderId
		that.setData({
			index1: e.detail.value
		})
		if (e.detail.value == 0) {
			var newparam = {
				openId: app.globalData.userInfo.openId,
				orderid: oradid,
				'type': 1,
			}
			util.PayOrder(oradid, newparam, {
				failed: function (res) {
					wx.showModal({
						title: '提示',
						// content: '支付失败，客户取消支付',
						content: res.data.msg,
					})
				},
				success: function (res) {
					if (res == "wxpay") {

					} else if (res == "success") {
						wx.showToast({
							title: '支付成功',
							duration: 500
						})
						app.globalData.payType = 1
						wx.navigateTo({
							url: '../paysuccess/paysuccess?totalMoney=' + that.data.BuyPrice
						})
					}
				}
			})
		} else {
			wx.showModal({
				title: '提示',
				content: '亲，是否使用余额支付？',
				success: function (res) {
					if (res.confirm) {
						app.globalData.payType = 2
						that.inite3(oradid)
					} else if (res.cancel) {
						console.log('用户点击取消')
					}
				}
			})
		}
	},
	// 改变标题颜色
	changedColor: function (e) {
		if (this.data.Loading == 0) {
			this.data.Loading = 1
			var state = e.currentTarget.dataset.state
			var index = e.currentTarget.id
			this.setData({
				condition: index
			})
			this.data.allList = []
			this.setData(this.data)
			this.data.pageIndex = 1
			this.inite(state, this.data.pageIndex)
		}
	},

	//初始化
	inite: function (state, pageIndex) {
		app.showLoading("加载...")
		var that = this
		var url = addr.Address.getMiniappGoodsOrder
		var param = {
			appid: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			State: state,
			pageIndex: pageIndex,
			pageSize: this.data.pageSize,
		}
		this.data.state = state
		var method = "GET"
		network.requestData(url, param, method, function (e) {
			if (e.data.isok == 1) {
				if (e.data.postdata.length > 0) {
					var allListold = that.data.allList
					var allList = e.data.postdata
					for (var i = 0; i < allList.length; i++) {
						for (var j = 0; j < allList[i].orderList.length; j++) {
							allList[i].orderList[j].BuyPrice = Math.round(allList[i].orderList[j].BuyPrice * 100) / 100
						}

						var tempmodel = allListold.find(f => f.year == allList[i].year)
						if (tempmodel != undefined) {
							for (var j = 0; j < allListold.length; j++) {
								if (allListold[j].year == allList[i].year) {
									allListold[j].orderList = allListold[j].orderList.concat(allList[i].orderList)
									break
								}

							}
						}
						else {
							allListold.push(allList[i])
						}
					}
					that.setData({
						allList: allListold,
						pageIndex: ++that.data.pageIndex
					})
				}
				else {
					wx.hideLoading()
					that.data.Loading = 0
					return
				}
				console.log(e)
			}
			wx.hideLoading()
			that.data.Loading = 0
		})
	},
	//初始化
	inite1: function (state, pageIndex) {
		app.showLoading("加载...")
		var that = this
		that.data.allList = []
		var url = addr.Address.getMiniappGoodsOrder
		var param = {
			appid: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			State: state,
			pageIndex: pageIndex,
			pageSize: this.data.pageSize,
		}
		this.data.state = state
		var method = "GET"
		network.requestData(url, param, method, function (e) {
			if (e.data.isok == 1) {
				if (e.data.postdata.length > 0) {
					var allListold = that.data.allList
					var allList = e.data.postdata
					for (var i = 0; i < allList.length; i++) {
						for (var j = 0; j < allList[i].orderList.length; j++) {
							allList[i].orderList[j].BuyPrice = Math.round(allList[i].orderList[j].BuyPrice * 100) / 100
						}

						var tempmodel = allListold.find(f => f.year == allList[i].year)
						if (tempmodel != undefined) {
							for (var j = 0; j < allListold.length; j++) {
								if (allListold[j].year == allList[i].year) {
									allListold[j].orderList = allListold[j].orderList.concat(allList[i].orderList)
									break
								}

							}
						}
						else {
							allListold.push(allList[i])
						}


					}
					that.setData({
						allList: e.data.postdata,
						pageIndex: ++that.data.pageIndex,
					})
				}
				else {
					that.setData({
						allList: [],
						pageIndex: 0,
					})
				}
				console.log(e)
			}
			wx.hideLoading()
			that.data.Loading = 0
		})
		that.setData({ condition: 3 })
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		// ”我的“页面入口
		if (app.globalData.canSaveMoneyFunction) {
			this.data.array = this.data.array
		} else {
			this.data.array = this.data.array.splice(0, 1)
		}
		this.setData({
			array: this.data.array,
			condition: options.condition
		})
		var state = options.state
		var that = this
		// 获取系统信息
		wx.getSystemInfo({
			success: function (res) {
				that.setData({
					sliderHeight: res.windowHeight,
				});
			}
		});

		this.data.allList = []
		this.data.pageIndex = 1
		this.data.Loading = 1
		if (app.globalData.userInfo != undefined && app.globalData.userInfo.openId != undefined) {
			that.inite(that.data.state, that.data.pageIndex)
		}
		else {
			app.getUserInfo(function () {
				that.inite(that.data.state, that.data.pageIndex)
			})
		}
	},
	// 跳转到订单详情页面
	navToorderDetail: function (e) {
		this.setData({
			orderId: e.currentTarget.id
		})
		app.globalData.payType = 1
		console.log(this.data.allList)
		wx.navigateTo({
			url: '../orderDetail/orderDetail?orderId=' + this.data.orderId + '&buyMode=2'
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
	goUrl: function (e) {
		var that = this
		var index = e.currentTarget.id
		var orderId = that.data.allList[0].orderList[index].Id
		getApp().globalData.dbOrder = orderId
		console.log(e)
		var orderList = that.data.allList[0].orderList
		console.log(orderList)
		if (orderList[index].StateTitle == '未付款' && orderList[index].StateTitle != '待发货') {
		}
		if (orderList[index].StateTitle == '待收货') {
		// if (orderList[index].StateTitle == '正在配送') {
			wx.showModal({
				title: '提示',
				content: '是否确认收货',
				success: function (res) {
					util.UpdateWxCard(that)
					if (res.confirm) {
						var orderid = orderList[index].Id
						wx.request({
							url: addr.Address.updateMiniappGoodsOrderState,
							data: {
								appid: app.globalData.appid,
								openid: app.globalData.userInfo.openId,
								State: 6,
								orderId: orderid
							},
							method: "POST",
							header: {
								'content-type': "application/json"
							},
							success: function (res) {
								if (res.data.isok == 1) {
									// that.data.allList = []
									that.inite1(5, 1)
									console.log('!!!0', res)
								}
							},
							fail: function () {
								console.log("确认收货失败")
								wx.showToast({
									title: '确认收货失败',
								})
							}
						})
					} else if (res.cancel) {
						console.log('用户点击取消')
					}
				}
			})
		}
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
		this.data.allList = []
		that.inite(that.data.state, 1)
		setTimeout(function () {
			wx.showToast({
				title: '刷新成功',
			})
			wx.stopPullDownRefresh()
		}, 1000)
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		if (this.data.Loading == 0) {
			this.data.Loading = 1
			this.inite(this.data.state, this.data.pageIndex)
		}
	},

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	},
	// 更改商品的支付方式
	inite3: function (orderid) {
		var that = this
		wx.request({
			url: addr.Address.updateOrderBuyMode,
			data: {
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				orderid: orderid,
				buyMode: 2
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.inite2(orderid)
				}
			},
			fail: function () {
				console.log("获取首页出错")
				wx.showToast({
					title: '获取首页出错',
				})
			}
		},
		)
	},
	// 余额二次付款
	inite2: function (goodsorderid) {
		var that = this
		wx.request({
			url: addr.Address.buyOrderbySaveMoney,
			data: {
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				goodsorderid: goodsorderid
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok == 1) {
					wx.navigateTo({
						url: '../paysuccess/paysuccess?totalMoney=' + that.data.BuyPrice,
					})
				}
			},
			fail: function () {
				console.log("获取首页出错")
				wx.showToast({
					title: '获取首页出错',
				})
			}
		},
		)
	},
})