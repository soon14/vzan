// pages/book/book.js
var template = require('../../template/template.js')
var tool = require("../../template/Pediluvium_Client.js")
var util = require('../../utils/util.js')
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		animationData: {},//弹窗动画
		tid: 0,//技师id
		chooseBegin: false,
		command: {},
		dataChoose: 0,
	},
	Alter: function () {
		var that = this
		wx.showModal({
			title: '付费提示',
			content: '送ta花儿后可查看相册哦！',
			cancelText: '下次吧',
			confirmText: '送花朵',
			confirmColor: '#fe536f',
			success: function (res) {
				if (res.confirm) {
					tool.payGift(that, 1, that.data.tid, function (e) {
						if (e.data.isok == 1) {
							var newparam = {
								openId: app.globalData.userInfo.openId,
								orderid: e.data.orderid,
								'type': 1,
							}
							util.PayOrder(e.data.orderid, newparam, {
								failed: function (res) {
									template.showmodal('提示', '您取消了支付该订单！', false)
								},
								success: function (res) {
									if (res == "wxpay") {
										console.log(res)
									} else if (res == "success") {
										template.UpdateWxCard(that)
										tool.GetTechnicianInfo(that, that.data.tid)
										template.showtoast('支付成功', 'success')
									}
								}
							})
						}
					})
				} else if (res.cancel) {
					console.log('用户点击取消')
				}
			}
		})
	},
	projectinfo: function (e) {
		template.goNewPage('../projectinfo/projectinfo?id=' + e.currentTarget.id)
	},

	gobooklist: function (e) {
		template.commitFormId(e.detail.formId) //提交备用formid
		if (this.data.command.timeT == undefined || this.data.command.timeT == '') {
			template.showtoast('请选择预约时间', 'loading')
			return
		} else {
			this.setData({ chooseBegin: !this.data.chooseBegin, dataChoose: 0 })

			this.data.command.time = this.data.command.dataT + this.data.command.timeT
			var command = JSON.stringify(this.data.command)
			template.goNewPage('../booklist/booklist?command=' + command)
		}
	},
	/**
	 * 生命周期函数--监听页面加载
	 */
	onLoad: function (options) {
		this.data.tid = options.id
		tool.GetTechnicianInfo(this, options.id)
		tool.GetStoreInfo(this)
	},
	unshowBook: function () {
		this.action()

		delete this.data.command.timeT
		this.setData({ chooseBegin: !this.data.chooseBegin, dataChoose: 0, animationData: {} })
	},
	// 开始预约
	book: function (e) {
		tool.GetDateTable(this, this.data.tid, 0) //默认请求索引0的时间列

		var index = e.currentTarget.dataset.pid
		this.data.command.Tid = this.data.info.id
		this.data.command.Sid = this.data.info.serviceList[index].id
		this.data.command.dataT = this.data.data.ReservationTime[0].date
		this.data.command.sexT = this.data.info.sex
		this.data.command.ageT = this.data.info.age
		this.data.command.nameT = this.data.info.jobNumber
		this.data.command.projectT = this.data.info.serviceList[index].name
		this.data.command.priceT = this.data.info.serviceList[index].discountPricestr

		this.setData({ chooseBegin: !this.data.chooseBegin, command: this.data.command })

		this.action()
	},

	chooseDate: function (e) {
		tool.GetDateTable(this, this.data.tid, e.currentTarget.id) //请求对应索引的时间列

		delete this.data.command.timeT
		this.data.command.dataT = this.data.data.ReservationTime[e.currentTarget.id].date
		this.setData({ dataChoose: e.currentTarget.id, command: this.data.command })
	},

	chooseTime: function (e) {

		// 选择时间附到command中
		var index = e.currentTarget.id
		var timeList = this.data.timeList

		var timeT = timeList[index].time
		for (var i = 0; i < timeList.length; i++) {
			if (i == index) {
				timeList[i].state = true
			} else {
				timeList[i].state = false
			}
		}

		this.data.command.timeT = timeT
		this.setData({ timeList: this.data.timeList, command: this.data.command })
	},

	previewImg: function (e) {
		template.previewImageList(e.currentTarget.id, this.data.info.photoList)
	},

	action: function () {// 动画
		var that = this;
		var windowHeight = wx.getSystemInfoSync().windowHeight
		var animation = wx.createAnimation({
			duration: 200,
			timingFunction: 'ease',
		})
		animation.translateY(-windowHeight * 0.65).step();
		that.animation = animation
		that.setData({
			animationData: animation.export()
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
		delete this.data.command.timeT
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
		tool.GetTechnicianInfo(this, this.data.tid)
		tool.GetStoreInfo(this)
		template.stopPullDown()
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