var WxParse = require('../../wxParse/wxParse.js');
var net = require('../../utils/net.js');
var util = require('../../utils/util.js');
var address = require('../../utils/address.js');
Page({
	data: {
		errorMsg: '',
		title_: '',
		date_: '',
		content_: '',
		imgUrl_: ''
	},
	onLoad: function (options) {
		if (options.isProduct != undefined) { this.setData({ isProduct: options.isProduct }) }
		// 页面初始化 options为页面跳转所带来的参数
		if (!util.isOptStrNull(options.id)) {
			console.log(options)
			this.setData({
				id: options.id,
				title_: options.title_,
				date_: options.date_,
				content_: options.content_,
				imgUrl_: options.imgUrl_
			})
			this.loadDataFormNet()
		} else {
			this.setData({
				Content: options.content,
			})
		}
	},
	loadDataFormNet: function () {
		var that = this
		var params = { id: that.data.id }
		util.showLoadingDialog("正在加载...")
		net.POST(address.Address.GET_MODEl_INFO_BY_ID, params, {
			success: function (res, msg) {
				util.hideLoadingDialog()
				if (util.isOptStrNull(res) || util.isOptStrNull(res.data)) {
					that.setData({
						errorMsg: msg
					})
					return
				}

				// 替换富文本标签 控制样式
				res.data[0].Content = res.data[0].Content.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
				res.data[0].Content = res.data[0].Content.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
				res.data[0].Content = res.data[0].Content.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
				res.data[0].Content = WxParse.wxParse('article', 'html', res.data[0].Content, that, 5)


				that.setData({
					headerImg: res.data[0].ImgUrl,
					navTitle: res.data[0].Title,
					content: res.data[0].Content
				})
				wx.setNavigationBarTitle({ title: res.data[0].Title })
				// that.data.content = res.data[0].Content
				// WxParse.wxParse('article', 'html', res.data[0].Content, that, 0, {
				//     onError: function (e) {
				//         console.log(e);
				//         if (that.data.article == "undefined" || util.isOptStrNull(that.data.article)) {
				//             that.setData({
				//                 errorMsg: "加载失败" + '，请点击重试'
				//             });
				//         }
				//     }
				// });
			},
			failure: function (fail) {
				util.hideLoadingDialog()
				console.log(fail);
				that.setData({
					errorMsg: fail
				})
			}
		})
	},
	onShareAppMessage: function () {
		var title_ = this.data.title_
		var date_ = this.data.date_
		var content_ = this.data.content_
		var imgUrl_ = this.data.imgUrl_ || this.data.headerImg
		if (!util.isOptStrNull(this.data.id)) {
			return {
				title: this.data.navTitle,
				path: '/pages/detail/detail?id=' + this.data.id + '&isProduct=' + this.data.isProduct
				+ '&title_=' + title_
				+ '&content_=' + content_
				// + '&imgUrl_=' + imgUrl_
				+ '&date_=' + date_,
				// imageUrl: imgUrl_
			}
		}
	},

})