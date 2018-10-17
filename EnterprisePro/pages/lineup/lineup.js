// pages/appointment_info/appointment_info.js
const page = require("../../utils/pageRequest.js");
const util = require("../../utils/util.js");

var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		phonenumber: 0,
	},
	inputPhonenumber: function (e) {
		this.data.phonenumber = e.detail.value
	},
	formaline: function (e) {
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		if (that.data.phonenumber != 0) {
			page.PutSortQueueMsg(that, that.data.phonenumber, function (cb) {
				if (cb == 'true') {
					page.GetUserInSortQueuesPlanMsg(that)
				}
			})
		} else {
			wx.showToast({ title: '信息未完善', icon: 'loading' })
		}
	},
	cancelqueue: function (e) {
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		wx.showModal({
			title: '提示',
			content: '是否确认取消排队？',
			success: function (res) {
				if (res.confirm) {
					page.CancelSortQueue(that, that.data.dataObj.sortQueue.id, function (cb) {
						if (cb == 'true') {
							page.GetUserInSortQueuesPlanMsg(that)
						}
					})
				}
			}
		})
	},
	refreshqueue: function (e) {
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, this)
		page.GetUserInSortQueuesPlanMsg(this)
		wx.showToast({ title: '刷新成功', duration: 500, })
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
		var that = this
		page.GetUserInSortQueuesPlanMsg(that)


		that.setData({ storeAddress: wx.getStorageSync('StoreInfo').postData.storeInfo.Address })
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
				if (that.data.isonOrder == true) {
					page.GetUserInSortQueuesPlanMsg(that)
				}
			}, 5000)
		}, 5000)
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

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	}
})