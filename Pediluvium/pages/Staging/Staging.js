// pages/Staging/Staging.js
var app = getApp()
var template = require('../../template/template.js')
var tool = require('../../template/Pediluvium.js')
Page({

  /**
   * 页面的初始数据
   */
	data: {
		typeid: 2,
		orderState: [
			{ txt: '待服务', id: 2, nums: 0 },
			{ txt: '已预约', id: 1, nums: 0 },
			{ txt: '已结束', id: 0, nums: 0 },
		],
		pageindex: 1,
		orderArray: [],
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		app.getUserInfo(function (e) {
			console.log(e)
			tool.GetTechInfo(that, 4);
			setTimeout(function () {

				setInterval(function () {
					var nowTime = new Date().getTime()
					var orderArray = that.data.orderArray

					for (var i = 0; i < orderArray.length; i++) {
						if (orderArray[i].state == 4) {
							var oldTime = ((new Date(orderArray[i].serviceTime)).getTime() - nowTime)
							orderArray[i].lesstime = template.formatDuring(oldTime)
						}
						if (orderArray[i].state == 5) {
							var oldTime_1 = ((new Date(orderArray[i].serviceEndTime)).getTime() - nowTime)
							orderArray[i].lesstime = template.formatDuring(oldTime_1)
						}
					}

					that.setData({ orderArray: orderArray })
				}, 1000)

			}, 1000)

		})
	},
	changeType: function (e) {
		wx.pageScrollTo({ scrollTop: 0, })
		this.setData({ typeid: e.currentTarget.id })
		tool.GetMyOrder(this, e.currentTarget.id, 1, 0)
	},
	changeorder: function (e) {
		var that = this
		if (e.currentTarget.id == 5) {
			var content = '是否确认上钟？'
		} else {
			var content = '是否确认下钟？'
		}
		wx.showModal({
			title: '是否确认',
			content: content,
			success: function (res) {
				if (res.confirm) {
					tool.UpdateOrderState(that, e.currentTarget.dataset.orderid, e.currentTarget.id)
				}
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
		tool.GetMyOrderCount(this)
		tool.GetMyOrder(this, this.data.typeid, 1, 0)
		template.stopPullDown()
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		tool.GetMyOrder(this, this.data.typeid, this.data.pageindex, 1)
	},

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	}
})