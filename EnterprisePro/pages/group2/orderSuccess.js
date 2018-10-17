// pages/group2.0/orderSuccess.js
const addr = require("../../utils/addr.js");
var http = require("../../utils/http.js");
var util = require("../../utils/util.js");
var tools = require('../../utils/tools.js');

var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {

	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		this.checkDetails(options.gOrderid)
		util.setPageSkin(that);
		this.setData({ isgroup: options.isgroup })
		setTimeout(function () {
			setInterval(function () {
				if (that.data.postdata != null) {
					var nowTime = new Date().getTime()
					var endtime = ((new Date(that.data.postdata.groupendtime)).getTime() - nowTime)
					that.data.postdata.endtime = tools.formatDuring(endtime)
					that.setData({ postdata: that.data.postdata })
				}
			}, 1000)
		}, 1000)

	},

	// 查询详情单
	checkDetails: function (gOrderid) {
		var that = this
		http.getAsync(
			addr.Address.getMiniappGoodsOrderById,
			{
				appid: app.globalData.appid,
				openid: app.globalData.userInfo.openId || wx.getStorageSync('userInfo').openId,
				orderId: gOrderid
			}
		).then(function (data) {
			if (data.isok == 1) {
				data.postdata.groupendtime = data.postdata.groupendtime.replace(/-/g, '/');
				if (data.postdata.groupstate == 1) {
					that.setTitle('开团成功')
				} else if (data.postdata.groupstate == 0) {
					that.setTitle('待付款')
				} else if (data.postdata.groupstate == 2) {
					that.setTitle('拼团成功')
				} else {
					that.setTitle('开团失败')
				}

				that.setData({ postdata: data.postdata })
			}
			console.log('订单id,团id', gOrderid, data.postdata.goodOrder.GroupId)
		})
	},
	// 复制订单号
	copyOrdernum: function () {
		util.copy(this.data.postdata.goodOrder.OrderNum)
	},
	goGroupDetail: function () {
		tools.goNewPage('../myGroup2/myGroupDetail?groupid=' + this.data.postdata.goodOrder.GroupId)
	},
	setTitle: function (title) {
		wx.setNavigationBarTitle({ title: title })
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
		var that = this
		return {
			title: that.data.postdata.goodOrderDtl[0].goodname,
			imageUrl: that.data.postdata.goodOrderDtl[0].goodImgUrl,
			path: 'pages/myGroup2/myGroupDetail?groupid=' + that.data.postdata.goodOrder.GroupId + '&aid=' + wx.getStorageSync('PageSetting').msg.aid
		}
	}
})