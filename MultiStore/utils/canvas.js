var addr = require("addr.js");
var app = getApp();

var canvas = {
	data: {
		imgCanvas: ''
	},
	inite3: function () {
		var that = this
		var imgCanvas = that.data.imgCanvas
		var Qrcode = that.data.Qrcode
		var StoreName = that.data.StoreName
		var StoreContent = that.data.StoreContent
		var freeStyle = that.data.freeStyle
		var Logo = that.data.Logo
		wx.request({
			url: addr.Address.GetShare, //仅为示例，并非真实的接口地址
			data: {
				appId: app.globalData.appid,
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				imgCanvas = res.data.obj.ADImg[0].url
				Logo = res.data.obj.Logo[0].url + "?x-oss-process=image/circle,r_100/format,png"
				Qrcode = res.data.obj.Qrcode
				StoreName = res.data.obj.StoreName
				StoreContent = res.data.obj.ADTitle
				freeStyle = res.data.obj.StyleType
				// 样式1
				if (freeStyle == 0) {
					wx.downloadFile({
						url: Qrcode.replace(/http/, "https"), //下载二维码图片
						success: function (res0) {
							// 下载大图
							wx.downloadFile({
								url: imgCanvas.replace(/http/, "https"), //下载砍价商品大图
								success: function (res) {
									var windowWidth = wx.getSystemInfoSync().windowWidth * 0.85 //画布宽度 以px为单位
									var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
									var context = wx.createCanvasContext('firstCanvas')
									var topic = '/image/11.png'
									var ImgUrl = res.tempFilePath
									var code = res0.tempFilePath
									var title = StoreName
									var content = StoreContent;
									var bottomText = "点赞科技提供技术支持"
									context.setFillStyle('white')
									context.fillRect(0, 0, windowWidth, windowHeight)
									// 背景图
									context.drawImage(topic, 0, 0, windowWidth, windowHeight);
									// 店铺图片
									context.drawImage(ImgUrl, windowWidth * 0.13, windowHeight * 0.14, windowWidth * 0.75, windowHeight * 0.30);
									// 二维码
									context.drawImage(code, windowWidth * 0.18, windowHeight * 0.635, windowHeight * 0.16, windowHeight * 0.16);
									// 店铺名称
									context.setFontSize(14)
									context.setFillStyle('#333333')
									context.fillText(title, windowWidth * 0.13, windowHeight * 0.5)
									// 内容
									context.setFontSize(12)
									context.setFillStyle('#978A8A')
									context.fillText(content, windowWidth * 0.13, windowHeight * 0.58)
									// 水印
									context.setFontSize(8)
									context.setFillStyle('#E8D9D9')
									context.fillText(bottomText, windowWidth * 0.37, windowHeight * 0.98)
									context.draw()


								}
							})
						}
					})
				}
				// 样式6
				if (freeStyle == 5) {
					wx.downloadFile({
						url: Qrcode.replace(/http/, "https"), //下载二维码图片
						success: function (res0) {
							// 下载大图
							wx.downloadFile({
								url: imgCanvas.replace(/http/, "https"), //下载砍价商品大图
								success: function (res) {
									var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87  //画布宽度 以px为单位
									var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
									var context = wx.createCanvasContext('firstCanvas')
									var topic = '/image/12.png'
									var ImgUrl = res.tempFilePath
									var code = res0.tempFilePath
									var title = StoreName
									var content = StoreContent;
									var bottomText = "点赞科技提供技术支持"
									context.setFillStyle('white')
									context.fillRect(0, 0, windowWidth, windowHeight)
									// 背景图
									context.drawImage(topic, 0, 0, windowWidth, windowHeight);
									// 店铺图片
									context.drawImage(ImgUrl, windowWidth * 0.14, windowHeight * 0.03, windowWidth * 0.72, windowHeight * 0.33);
									// 二维码
									context.drawImage(code, windowWidth * 0.33, windowHeight * 0.61, windowHeight * 0.25, windowHeight * 0.25);
									// 店铺名称
									context.setFontSize(14)
									context.setFillStyle('#333333')
									context.fillText(title, windowWidth * 0.125, windowHeight * 0.45)
									// 内容
									context.setFontSize(12)
									context.setFillStyle('#978A8A')
									context.fillText(content, windowWidth * 0.125, windowHeight * 0.52)
									// 水印
									context.setFontSize(8)
									context.setFillStyle('#E8D9D9')
									context.fillText(bottomText, windowWidth * 0.38, windowHeight * 0.97)
									context.draw()
								}
							})
						}
					})
				}
				// 样式5
				if (freeStyle == 4) {
					wx.downloadFile({
						url: Qrcode.replace(/http/, "https"), //下载二维码图片
						success: function (res0) {
							// 下载大图
							wx.downloadFile({
								url: imgCanvas.replace(/http/, "https"), //下载砍价商品大图
								success: function (res3) {
									wx.downloadFile({
										url: Logo.replace(/http/, "https"),
										success: function (res) {
											var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
											var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
											var context = wx.createCanvasContext('firstCanvas')
											var topic = '/image/13.png'
											var ImgUrl = res3.tempFilePath
											var code = res0.tempFilePath
											var title = StoreName
											var content = StoreContent;
											var logo = res.tempFilePath
											var bottomText = "点赞科技提供技术支持"
											context.setFillStyle('white')
											context.fillRect(0, 0, windowWidth, windowHeight)
											// 背景图
											context.drawImage(topic, 0, 0, windowWidth, windowHeight);
											// logo
											context.drawImage(logo, windowWidth * 0.43, windowHeight * 0.048, windowHeight * 0.1, windowHeight * 0.1);
											// 店铺广告图片
											context.drawImage(ImgUrl, windowWidth * 0.13, windowHeight * 0.18, windowWidth * 0.75, windowHeight * 0.30);
											// 二维码
											context.drawImage(code, windowWidth * 0.23, windowHeight * 0.698, windowHeight * 0.16, windowHeight * 0.16);
											// 店铺名称
											context.setFontSize(14)
											context.setFillStyle('#333333')
											context.fillText(title, windowWidth * 0.13, windowHeight * 0.53)
											// 内容
											context.setFontSize(12)
											context.setFillStyle('#978A8A')
											context.fillText(content, windowWidth * 0.13, windowHeight * 0.59)
											// 水印
											context.setFontSize(8)
											context.setFillStyle('#E8D9D9')
											context.fillText(bottomText, windowWidth * 0.37, windowHeight * 0.98)
											context.draw()
										}
									})
								}
							})
						}
					})
				}
				// 样式4
				if (freeStyle == 3) {
					wx.downloadFile({
						url: Qrcode.replace(/http/, "https"), //下载二维码图片
						success: function (res0) {
							// 下载大图
							wx.downloadFile({
								url: imgCanvas.replace(/http/, "https"), //下载砍价商品大图
								success: function (res3) {
									wx.downloadFile({
										url: Logo.replace(/http/, "https"),
										success: function (res) {
											var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
											var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
											var context = wx.createCanvasContext('firstCanvas')
											var topic = '/image/14.png'
											var ImgUrl = res3.tempFilePath
											var code = res0.tempFilePath
											var title = StoreName
											var content = "扫码进入店铺";
											var logo = res.tempFilePath
											var bottomText = "点赞科技提供技术支持"
											context.setFillStyle('white')
											context.fillRect(0, 0, windowWidth, windowHeight)
											// 背景图
											context.drawImage(topic, 0, 0, windowWidth, windowHeight);
											// logo
											context.drawImage(logo, windowWidth * 0.68, windowHeight * 0.18, windowHeight * 0.1, windowHeight * 0.1);
											// 店铺广告图片
											context.drawImage(ImgUrl, windowWidth * 0.059, windowHeight * 0.089, windowWidth * 0.5, windowHeight * 0.83);
											// 二维码
											context.drawImage(code, windowWidth * 0.63, windowHeight * 0.6, windowHeight * 0.16, windowHeight * 0.16);
											// 店铺名称
											context.setFontSize(9)
											context.setFillStyle('#333333')
											context.fillText(title, windowWidth * 0.655, windowHeight * 0.16)
											// 内容
											context.setFontSize(12)
											context.setFillStyle('#dacaca')
											context.fillText(content, windowWidth * 0.63, windowHeight * 0.80)
											// 水印
											context.setFontSize(8)
											context.setFillStyle('#E8D9D9')
											context.fillText(bottomText, windowWidth * 0.35, windowHeight * 0.98)
											context.draw()
										}
									})
								}
							})
						}
					})
				}
				// 样式7
				if (freeStyle == 6) {
					wx.downloadFile({
						url: Qrcode.replace(/http/, "https"), //下载二维码图片
						success: function (res0) {
							// 下载大图
							wx.downloadFile({
								url: imgCanvas.replace(/http/, "https"), //下载砍价商品大图
								success: function (res3) {
									wx.downloadFile({
										url: Logo.replace(/http/, "https"),
										success: function (res) {
											var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
											var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
											var context = wx.createCanvasContext('firstCanvas')
											var topic = '/image/15.png'
											var ImgUrl = res3.tempFilePath
											var code = res0.tempFilePath
											var title = StoreName
											var content = StoreContent;
											var logo = res.tempFilePath
											var bottomText = "点赞科技提供技术支持"
											context.setFillStyle('white')
											context.fillRect(0, 0, windowWidth, windowHeight)
											// 背景图
											context.drawImage(topic, 0, 0, windowWidth, windowHeight);
											// logo
											// context.drawImage(logo, windowWidth * 0.65, windowHeight * 0.18, windowHeight * 0.1, windowHeight * 0.1);
											// 店铺广告图片
											context.drawImage(ImgUrl, windowWidth * 0.0001, windowHeight * 0.0001, windowWidth, windowHeight * 0.7);
											// 二维码
											context.drawImage(code, windowWidth * 0.16, windowHeight * 0.72, windowHeight * 0.18, windowHeight * 0.18);
											// 店铺名称
											// context.setFontSize(14)
											// context.setFillStyle('#333333')
											// context.fillText(title, windowWidth * 0.6, windowHeight * 0.38)
											// 内容
											// context.setFontSize(12)
											// context.setFillStyle('#978A8A')
											// context.fillText(content, windowWidth * 0.6, windowHeight * 0.45)
											// 水印
											context.setFontSize(8)
											context.setFillStyle('#E8D9D9')
											context.fillText(bottomText, windowWidth * 0.37, windowHeight * 0.98)
											context.draw()
										}
									})
								}
							})
						}
					})
				}
				// 样式3
				if (freeStyle == 1) {
					wx.downloadFile({
						url: Qrcode.replace(/http/, "https"), //下载二维码图片
						success: function (res0) {
							// 下载大图
							wx.downloadFile({
								url: imgCanvas.replace(/http/, "https"), //下载砍价商品大图
								success: function (res) {
									var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
									var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
									var context = wx.createCanvasContext('firstCanvas')
									var topic = '/image/16.png'
									var ImgUrl = res.tempFilePath
									var code = res0.tempFilePath
									var title = StoreName
									var content = StoreContent;
									var bottomText = "点赞科技提供技术支持"
									context.setFillStyle('white')
									context.fillRect(0, 0, windowWidth, windowHeight)
									// 背景图
									context.drawImage(topic, 0, 0, windowWidth, windowHeight);
									// 店铺图片
									context.drawImage(ImgUrl, windowWidth * 0.068, windowHeight * 0.09, windowWidth * 0.867, windowHeight * 0.35);
									// 二维码
									context.drawImage(code, windowWidth * 0.18, windowHeight * 0.648, windowWidth * 0.23, windowHeight * 0.16);
									// 店铺名称
									context.setFontSize(14)
									context.setFillStyle('#333333')
									context.fillText(title, windowWidth * 0.15, windowHeight * 0.50)
									// 内容
									context.setFontSize(12)
									context.setFillStyle('#978A8A')
									context.fillText(content, windowWidth * 0.15, windowHeight * 0.58)
									// 水印
									context.setFontSize(8)
									context.setFillStyle('#E8D9D9')
									context.fillText(bottomText, windowWidth * 0.37, windowHeight * 0.98)
									context.draw()
								}
							})
						}
					})
				}
				// 样式2
				if (freeStyle == 2) {
					wx.downloadFile({
						url: Qrcode.replace(/http/, "https"), //下载二维码图片
						success: function (res0) {
							// 下载大图
							wx.downloadFile({
								url: imgCanvas.replace(/http/, "https"), //下载砍价商品大图
								success: function (res3) {
									wx.downloadFile({
										url: Logo.replace(/http/, "https"),
										success: function (res) {
											var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
											var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
											var context = wx.createCanvasContext('firstCanvas')
											var topic = '/image/17.png'
											var ImgUrl = res3.tempFilePath
											var code = res0.tempFilePath
											var title = StoreName
											var content = StoreContent;
											var logo = res.tempFilePath
											var bottomText = "点赞科技提供技术支持"
											context.setFillStyle('white')
											context.fillRect(0, 0, windowWidth, windowHeight)
											// 背景图
											context.drawImage(topic, 0, 0, windowWidth, windowHeight);
											// logo
											context.drawImage(logo, windowWidth * 0.43, windowHeight * 0.07, windowHeight * 0.105, windowHeight * 0.105);
											// 店铺广告图片
											context.drawImage(ImgUrl, windowWidth * 0.13, windowHeight * 0.18, windowWidth * 0.75, windowHeight * 0.30);
											// 二维码
											context.drawImage(code, windowWidth * 0.23, windowHeight * 0.675, windowHeight * 0.16, windowHeight * 0.16);
											// 店铺名称
											context.setFontSize(14)
											context.setFillStyle('#333333')
											context.fillText(title, windowWidth * 0.15, windowHeight * 0.54)
											// 内容
											context.setFontSize(12)
											context.setFillStyle('#978A8A')
											context.fillText(content, windowWidth * 0.15, windowHeight * 0.63)
											// 水印
											context.setFontSize(8)
											context.setFillStyle('#E8D9D9')
											context.fillText(bottomText, windowWidth * 0.37, windowHeight * 0.98)
											context.draw()
										}
									})
								}
							})
						}
					})
				}
			},
			fail: function () {
				console.log("获取信息出错")
				wx.showToast({
					title: '获取信息出错',
				})
			}
		})
	},



}
module.exports = canvas;