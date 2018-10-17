// pages/group/grouporderinfo.js
var template = require('../../template/template.js');
var addr = require("../../utils/addr.js");
var util = require("../../utils/util.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		isgroup: 0,
	},
	copy_ordernums: function () {
		template.copy(this.data.postdata.goodOrder.OrderNum)
	},
	makephonecall: function () {
		template.makePhoneCall('dsads')
	},
	navigateTo: function (e) {
		template.goNewPage('../group/joingroupinfo?groupid=' + e.currentTarget.dataset.groupid)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		that.data.isshareGroup = options.isshareGroup != undefined ? options.isshareGroup : 0
		that.setData({ isgroup: options.isgroup == 1 ? '1' : '0' })
		that.getMiniappGoodsOrderById(options.orderid)
	},
	getMiniappGoodsOrderById: function (orderId) {
		var that = this
		wx.request({
			url: addr.Address.getMiniappGoodsOrderById,
			data: {
				AppId: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				orderId: orderId
			},
			method: "GET",
			header: {
				'content-type': 'application/json'
			},
			success: function (res) {
				if (res.data.isok == 1) {
					res.data.data.groupendtime = res.data.data.groupendtime.replace(/-/g, '/');
					res.data.data.goodOrderDtl[0].orderDtl.CreateDateStr = template.ChangeDateFormatNew(res.data.data.goodOrderDtl[0].orderDtl.CreateDate)
					that.setData({ postdata: res.data.data })
				}
			},
			fail: function () {
				console.log("查询订单详情失败")
			}
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
		var that = this
		setTimeout(function () {
			that.data.timeInter = setInterval(function () {
				if (that.data.postdata != null) {
					var nowTime = new Date().getTime()
					var endtime = ((new Date(that.data.postdata.groupendtime)).getTime() - nowTime)
					that.data.postdata.endtime = template.formatDuring(endtime)
					that.setData({ postdata: that.data.postdata })
				}
			}, 1000)
		}, 1000)
	},

	/**
	 * 生命周期函数--监听页面隐藏
	 */
	onHide: function () {
		clearInterval(this.data.timeInter)
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
	gotopay: function (e) {
		var that = this
		var payid = e.currentTarget.dataset.payid
		var orderId = e.currentTarget.dataset.orderid
		var newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: payid,
			'type': 1,
		}
		util.PayOrder(payid, newparam, {
			failed: function () {
				wx.showToast({ title: '您取消了支付', icon: 'loading' })
			},
			success: function (res) {
				if (res == "wxpay") {
				} else if (res == "success") {
					wx.showToast({
						title: '支付成功',
						duration: 500
					})
					wx.redirectTo({
						url: '../group/grouporderinfo?orderid=' + orderId + '&isgroup=1',
					})
				}
			}
		})
	},
	/**
	 * 用户点击右上角分享
	 */
	onShareAppMessage: function () {
		return {
			title: this.data.postdata.goodOrderDtl[0].goodname,
			imageUrl: this.data.postdata.goodOrderDtl[0].goodImgUrl,
			path: 'pages/group/joingroupinfo?groupid=' + this.data.postdata.goodsOrder.GroupId + '&isshareGroup=1'
		}
	}
})