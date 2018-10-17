// pages/launch/launch.js
var net = require("../../utils/net.js")
var util = require("../../utils/util.js")
var app = getApp()
Page({
	data: {
		imgurl: ''
	},
	onLoad: function (options) {
		this.request()
		// 页面初始化 options为页面跳转所带来的参数
		setTimeout(function () {
			wx.switchTab({
				url: '../index/index'
			})
		}, 3000)
	},
	onReady: function () {
		// 页面渲染完成
	},
	onShow: function () {
		// 页面显示
	},
	onHide: function () {
		// 页面隐藏
	},
	onUnload: function () {
		// 页面关闭
	},
	request: function (isShowLoading = true) {
		var that = this

		wx.request({
			url: net.Address.GET_MODEl_IMG,
			data: {
				appid: app.globalData.appid,
			},
			method: "GET",
			header: {
				'content-type': "application/json"
			},
			//下拉刷新 
			success: function (res) {
				if (res.data.isok == 1) {
					var object = res.data.data
					if (res.data.data.length > 0) {
						var name = object.find(f => f.Param == "nparam")
						var url = object.find(f => f.Param == "img")
						if (url != undefined) {
							that.data.imgurl = url.Value
						}
						that.setData(that.data)

						if (name != null && name != undefined) {
							wx.setNavigationBarTitle({
								title: name.Value
							})
						}
					}
				}
			},
			fail: function (e) {
				console.log("获取首页出错")

				console.log(e)
			},
			complete: function () {
				wx.stopPullDownRefresh()
				util.hideNavigationBarLoading()
			}
		})
	},
})