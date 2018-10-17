// pages/group/mygroupList.js
var tool = require('../../template/Food2.0.js');
var template = require('../../template/template.js');
var util = require("../../utils/util.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		headtype: [
			{ content: '全部', id: 0 },
			{ content: '进行中', id: 1 },
			{ content: '已成功', id: 2 }
		],
		choosetype: 0,
		pageindex: 1,


	},
	navigateTo: function (e) {
		var index = e.currentTarget.id //0参团详情 1参团详情 
		if (index == 1) {
			template.goNewPage('../group/joingroupinfo?groupid=' + e.currentTarget.dataset.groupid)
		}
		if (index == 0) {
			template.goNewPage('../group/grouporderinfo?orderid=' + e.currentTarget.dataset.orderid + '&isgroup=1')
		}
	},
	changetype: function (e) {
		this.setData({ isall: false })
		tool.GetMyGroupList(this, e.currentTarget.id, 1, 0)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		wx.showLoading({ title: '数据加载中....', })
		tool.GetMyGroupList(this, 0, 1, 0)
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
		tool.GetMyGroupList(this, 0, 1, 0)
		template.stopPullDown()
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		tool.GetMyGroupList(this, this.data.choosetype, this.data.pageindex, 1)
	},

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function (res) {
		var group = res.target.dataset.group
		return {
			title: group.goodList[0].goodsMsg.name,
			imageUrl: group.goodList[0].goodsMsg.img,
			path: 'pages/group/joingroupinfo?groupid=' + group.groupid
		}
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
					template.goNewPage('../group/grouporderinfo?orderid=' + orderId + '&isgroup=1')
				}
			}
		})
	}
})