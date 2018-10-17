// pages/appointment/appointment_info.js
const addr = require('../../utils/addr.js');
const tools = require("../../utils/tools.js")
var util = require('../../utils/util.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		listmodal: false,
	},
	show_listmodal: function () {
		this.setData({ listmodal: !this.data.listmodal })
	},
	cancelbook: function (e) {
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)

		wx.showModal({
			title: '提示',
			content: '是否确认取消预约呢？',
			success: function (res) {
				if (res.confirm) {
					tools.resetappoint()
					that.CancelResevation(that)
				}
			},
		})
	},
	navigate_home: function (e) {
		// getApp().globalData.appoint_Id = 0
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, this)
		tools.resetappoint()
		wx.navigateBack({ delta: 1 })
	},
	openlocation: function () {
		wx.openLocation({
			latitude: wx.getStorageSync('StoreInfo').postData.storeInfo.Lat,
			longitude: wx.getStorageSync('StoreInfo').postData.storeInfo.Lng,
			scale: 14
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		this.setData({ storeInfo: wx.getStorageSync('StoreInfo').postData.storeInfo})
		// this.setData({ storeAddress: app.globalData.storeAddress.storeaddress })
		this.GetReservation(this)
		this.GetReserveMenu(this)
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
		this.GetReservation(this)
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

	GetReservation: function (that) {
		wx.request({
			url: addr.Address.GetReservation,
			data: {
				appId: app.globalData.appid,
				openId: app.globalData.userInfo.openId,
				reserveId: app.globalData.appoint_Id
			},
			method: "POST",
			header: {
				"content-type": "application/x-www-form-urlencoded"
			},
			success: function (res) {
				if (res.data.result) {
					if (res.data.obj.State == -4 || res.data.obj.State == 6 || res.data.obj.State == -1) {
						getApp().globalData.appoint_Id = 0
						tools.resetappoint()
						setTimeout(function () {
							wx.stopPullDownRefresh()
							wx.showToast({ title: '更新成功', })
						}, 500)
					}
					that.setData({ data: res.data.obj })
				}
			},
			fail: function () {
				console.log("查询预约id出错")
			}
		})
	},
	CancelResevation: function (that) {
		wx.request({
			url: addr.Address.CancelResevation,
			data: {
				appId: getApp().globalData.appid,
				openId: getApp().globalData.userInfo.openId,
				reserveId: getApp().globalData.appoint_Id
			},
			method: "POST",
			header: {
				"content-type": "application/x-www-form-urlencoded"
			},
			success: function (res) {
				if (res.data.result) {
					// getApp().globalData.appoint_Id = 0
					wx.showToast({ title: '取消成功', })
					setTimeout(function () {
						wx.navigateBack({ delta: 1 })
					}, 1000)
				} else {
					wx.showToast({ title: res.data.msg, icon: 'loading' })
				}
			},
			fail: function () {
				console.log("加入预约购物车出错")
			}
		})
	},
	GetReserveMenu: function (that) {
		wx.request({
			url: addr.Address.GetReserveMenu,
			data: {
				appId: app.globalData.appid,
				openId: app.globalData.userInfo.openId,
				reserveId: app.globalData.appoint_Id
			},
			method: "POST",
			header: {
				"content-type": "application/x-www-form-urlencoded"
			},
			success: function (res) {
				if (res.data.result) {
					var alldiscountprice = 0
					if (res.data.obj != null) {
						var orderInfo = res.data.obj
						for (var i = 0; i < res.data.obj.length; i++) {
							alldiscountprice += (res.data.obj[i].Price * res.data.obj[i].Count)
						}
					}
					that.setData({ oderInfo: orderInfo, alldiscountprice: (alldiscountprice).toFixed(2) })
				}
			},
			fail: function () {
				console.log("查询预约菜单出错")
			}
		})
	}

})