// pages/appointment_info/appointment_info.js
var template = require('../../template/template.js');
var tool = require('../../template/Food2.0.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		numsArray: ['选择就餐人数'], numsindex: 0,
		phonenumber: 0,
	},
	bindPickerChange: function (e) {
		this.setData({ numsindex: e.detail.value })
	},
	inputPhonenumber: function (e) {
		this.data.phonenumber = e.detail.value
	},
	formaline: function (e) {
		// 提交备用formId
		var formId = e.detail.formId
		template.commitFormId(formId, this)
		if (this.data.numsindex != 0 || this.data.phonenumber != 0) {
			tool.PutSortQueueMsg(this, this.data.numsArray[this.data.numsindex], this.data.phonenumber)
		} else {
			wx.showToast({ title: '信息未完善', icon: 'loading' })
		}
	},
	cancelqueue: function (e) {
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		template.commitFormId(formId, that)
		wx.showModal({
			title: '提示',
			content: '是否确认取消排队？',
			success: function (res) {
				if (res.confirm) {
					tool.CancelSortQueue(that, that.data.dataObj.sortQueue.id)
				}
			}
		})
	},
	refreshqueue: function (e) {
		// 提交备用formId
		var formId = e.detail.formId
		template.commitFormId(formId, this)
		tool.GetUserInSortQueuesPlanMsg(this)
		wx.showToast({ title: '刷新成功', duration: 500, })
	},
	openlocation: function () {
		tool.openLocation(app.globalData.storeAddress.storeLat, app.globalData.storeAddress.storeLng, 14)
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		tool.GetUserInSortQueuesPlanMsg(that)




		for (var numsindex = 1; numsindex < 15; numsindex++) {
			that.data.numsArray.push(numsindex)
		}
		that.setData({ numsArray: that.data.numsArray, storeAddress: app.globalData.storeAddress.storeaddress })
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
					tool.GetUserInSortQueuesPlanMsg(that)
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