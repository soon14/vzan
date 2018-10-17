//index.js
var net = require("../../utils/net.js")
var util = require("../../utils/util.js")
//获取应用实例
var app = getApp()
var WxParse = require('../../wxParse/wxParse.js');
Page({
	data: {
		title: '',
		introductInfo: {
			introduct: '',
			dataNode: ''
		},
		imgUrls: [
			// "../../images/png_05.png",
			// "../../images/png_04.png",
			// "../../images/png_03.png",
			// "../../images/png_02.png",
			// "../../images/png_01.png"
		]
	},
    navo_webview: function () {
        wx.navigateTo({
            url: '../web_view/web_view?id=' + this.data.AgentConfig.QrcodeId
        })
    },
	onLoad: function () {
		app.IsShowBottomLogo(this)

		this.requestTitle()
		console.log(net.MessageCode.SuccessCode.NORMAL)
		console.log(net.Address.GET_MODEL_DATA)
		this.request()
	},
	onPullDownRefresh: function () {
		this.request(false)
		app.IsShowBottomLogo(this)
		wx.stopPullDownRefresh()
	},
	request: function (isShowLoading = true) {
		var that = this
		var params = {
			appid: app.globalData.appid,
			level: '1'
		}
		if (isShowLoading) {
			util.showLoadingDialog()
		}
		var request = new net.GET(net.Address.GET_MODEL_DATA, params, {
			success: function (data, code) {
				util.hideLoadingDialog()
				util.stopPullDownRefresh()
				if (code == net.MessageCode.SuccessCode.NORMAL) {
					var object = data.data[0]
					var title = object.Title
					var introduct = object.Content
					var imgUrls = object.ImgUrl.split(',')
					// 替换富文本标签 控制样式
					introduct = introduct.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
					introduct = introduct.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
					introduct = introduct.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
					that.data.introductInfo.dataNode = WxParse.wxParse('article', 'html', introduct, that, 5),
					
					that.data.title = title
					that.data.imgUrls = imgUrls
					that.setData(that.data)
					app.globalData.title = title
					console.log(data)
				}
				else {
					console.log("no data")
				}
			},

			failure: function (error) {
				console.log(error)
				util.hideLoadingDialog()
				util.stopPullDownRefresh()
			}
		})
	},
	onShareAppMessage: function () {
		return {
			title: this.data.title,
			path: '/pages/index/index'
		}
	},

	requestTitle: function (isShowLoading = true) {
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

						if (name != null && name != undefined) {
							wx.setNavigationBarTitle({
								title: name.Value
							})
						}
					}
				}
			},
			fail: function (e) {
				console.log(e)
			},
			complete: function () {
				wx.stopPullDownRefresh()
				util.hideNavigationBarLoading()
			}
		})
	},
})
