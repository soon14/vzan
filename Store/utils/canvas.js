var app = getApp();

var canvas = {
	Drawcanvas: function () {
		var that = this
		var Qrcode = app.globalData.shareConfig.Qrcode //二维码
		if (app.globalData.shareConfig.ADImg.length == 0) {
			var imgCanvas = Qrcode.replace(/http/, "https")
		} else {
			imgCanvas = app.globalData.shareConfig.ADImg[0].url.replace(/http/, "https")
		}
		var StoreName = app.globalData.shareConfig.StoreName
		if (app.globalData.shareConfig.ADTitle != null) {
			var StoreContent = app.globalData.shareConfig.ADTitle
		} else {
			var StoreContent = ''
		}
		var freeStyle = app.globalData.shareConfig.StyleType
		var Logo = app.globalData.shareConfig.Logo[0].url + "?x-oss-process=image/circle,r_100/format,png"
		// 样式一
		if (freeStyle == 0) {
			wx.downloadFile({
				url: Qrcode.replace(/http/, "https"), //下载二维码图片
				success: function (res0) {
					// 下载大图
					wx.downloadFile({
						url: imgCanvas, //下载砍价商品大图
						success: function (res) {
							var windowWidth = wx.getSystemInfoSync().windowWidth * 0.85 //画布宽度 以px为单位
							var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
							var context = wx.createCanvasContext('firstCanvas')
							var topic = '/image/11.png'
							if (app.globalData.shareConfig.ADImg.length == 0) {
								var ImgUrl = '/image/replacewhite.png'
							} else {
								var ImgUrl = res.tempFilePath
							}
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
							context.drawImage(code, windowWidth * 0.19, windowHeight * 0.635, 75, 70);
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
		// 样式二
		if (freeStyle == 5) {
			wx.downloadFile({
				url: Qrcode.replace(/http/, "https"), //下载二维码图片
				success: function (res0) {
					// 下载大图
					wx.downloadFile({
						url: imgCanvas, //下载砍价商品大图
						success: function (res) {
							var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87  //画布宽度 以px为单位
							var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
							var context = wx.createCanvasContext('firstCanvas')
							var topic = '/image/12.png'
							if (app.globalData.shareConfig.ADImg.length == 0) {
								var ImgUrl = '/image/replacewhite.png'
							} else {
								var ImgUrl = res.tempFilePath
							}
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
							context.drawImage(code, windowWidth * 0.36, windowHeight * 0.62, 85, 80);
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
		// 样式三
		if (freeStyle == 4) {
			wx.downloadFile({
				url: Qrcode.replace(/http/, "https"), //下载二维码图片
				success: function (res0) {
					// 下载大图
					wx.downloadFile({
						url: imgCanvas, //下载砍价商品大图
						success: function (res3) {
							wx.downloadFile({
								url: Logo.replace(/http/, "https"),
								success: function (res) {
									var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
									var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
									var context = wx.createCanvasContext('firstCanvas')
									var topic = '/image/13.png'
									if (app.globalData.shareConfig.ADImg.length == 0) {
										var ImgUrl = '/image/replacewhite.png'
									} else {
										var ImgUrl = res3.tempFilePath
									}
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
									context.drawImage(code, windowWidth * 0.23, windowHeight * 0.698, 70, 65);
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
		// 样式四
		if (freeStyle == 3) {
			wx.downloadFile({
				url: Qrcode.replace(/http/, "https"), //下载二维码图片
				success: function (res0) {
					// 下载大图
					wx.downloadFile({
						url: imgCanvas, //下载砍价商品大图
						success: function (res3) {
							wx.downloadFile({
								url: Logo.replace(/http/, "https"),
								success: function (res) {
									var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
									var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
									var context = wx.createCanvasContext('firstCanvas')
									var topic = '/image/14.png'
									if (app.globalData.shareConfig.ADImg.length == 0) {
										var ImgUrl = '/image/replacewhite.png'
									} else {
										var ImgUrl = res3.tempFilePath
									}
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
									context.drawImage(code, windowWidth * 0.63, windowHeight * 0.56, 75, 70);
									// 店铺名称
									context.setFontSize(12)
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
		// 样式五
		if (freeStyle == 6) {
			wx.downloadFile({
				url: Qrcode.replace(/http/, "https"), //下载二维码图片
				success: function (res0) {
					// 下载大图
					wx.downloadFile({
						url: imgCanvas, //下载砍价商品大图
						success: function (res3) {
							wx.downloadFile({
								url: Logo.replace(/http/, "https"),
								success: function (res) {
									var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
									var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
									var context = wx.createCanvasContext('firstCanvas')
									var topic = '/image/15.png'
									if (app.globalData.shareConfig.ADImg.length == 0) {
										var ImgUrl = '/image/replacewhite.png'
									} else {
										var ImgUrl = res3.tempFilePath
									}
									var code = res0.tempFilePath
									var title = StoreName
									var content = StoreContent;
									var logo = res.tempFilePath
									var bottomText = "点赞科技提供技术支持"
									context.setFillStyle('white')
									context.fillRect(0, 0, windowWidth, windowHeight)
									// 背景图
									context.drawImage(topic, 0, 0, windowWidth, windowHeight);
									// 店铺广告图片
									context.drawImage(ImgUrl, windowWidth * 0.0001, windowHeight * 0.0001, windowWidth, windowHeight * 0.7);
									// 二维码
									context.drawImage(code, windowWidth * 0.16, windowHeight * 0.72, 78, 70);
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
		// 样式六
		if (freeStyle == 1) {
			wx.downloadFile({
				url: Qrcode.replace(/http/, "https"), //下载二维码图片
				success: function (res0) {
					// 下载大图
					wx.downloadFile({
						url: imgCanvas, //下载砍价商品大图
						success: function (res) {
							var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
							var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
							var context = wx.createCanvasContext('firstCanvas')
							var topic = '/image/16.png'
							if (app.globalData.shareConfig.ADImg.length == 0) {
								var ImgUrl = '/image/replacewhite.png'
							} else {
								var ImgUrl = res.tempFilePath
							}
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
							context.drawImage(code, windowWidth * 0.20, windowHeight * 0.648, 65, 60);
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
		// 样式七
		if (freeStyle == 2) {
			wx.downloadFile({
				url: Qrcode.replace(/http/, "https"), //下载二维码图片
				success: function (res0) {
					// 下载大图
					wx.downloadFile({
						url: imgCanvas, //下载砍价商品大图
						success: function (res3) {
							wx.downloadFile({
								url: Logo.replace(/http/, "https"),
								success: function (res) {
									var windowWidth = wx.getSystemInfoSync().windowWidth * 0.87 //画布宽度 以px为单位
									var windowHeight = wx.getSystemInfoSync().windowHeight * 0.75 //画布高度 以px为单位
									var context = wx.createCanvasContext('firstCanvas')
									var topic = '/image/17.png'
									if (app.globalData.shareConfig.ADImg.length == 0) {
										var ImgUrl = '/image/replacewhite.png'
									} else {
										var ImgUrl = res3.tempFilePath
									}
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
									context.drawImage(code, windowWidth * 0.23, windowHeight * 0.675, 75, 70);
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
}
module.exports = canvas;