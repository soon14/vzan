var addr = require("../../utils/addr.js")
var app = getApp()
var util = require("../../utils/util.js");
var canvas = require("../../utils/canvas.js");
// pages/home/home.js
Page({

  /**
   * 页面的初始数据
   */
	data: {
		showtime: '',
		isonload: 0,
		first: 0,
		opentime: [
			{ date: '一', condition: false },
			{ date: '二', condition: false },
			{ date: '三', condition: false },
			{ date: '四', condition: false },
			{ date: '五', condition: false },
			{ date: '六', condition: false },
			{ date: '日', condition: false },
		],
		openDate: [],//营业时间 缩写
		postdata: [],//首页信息大集合
		sliderImgs: [],//轮播图集合
		// swiper
		indicatorDots: false,
		autoplay: true,
		interval: 3000,
		duration: 1000,
		storeImgs: [],//门店图集合
		isOnload: 0,//检测是否第一次进入 控制onshow
		AgentConfig: [],//水印开关集合
		showCanvas: true,//分享按钮显隐
		item: [    // 门店图标
			{ txt: '扫码点餐', iconPath: '/image/a29.png', id: 1 },
			{ txt: '点外卖', iconPath: '/image/a30.png', id: 2 },
			{ txt: '会员卡', iconPath: '/image/a31.png', id: 3 },
			{ txt: '储值有礼', iconPath: '/image/a32.png', id: 4 },
			{ txt: '推荐好友', iconPath: '/image/a33.png', id: 5 },
		]
	},
	// 保存画布的图片
	canvasToTempFilePath: function (e) {
		wx.canvasToTempFilePath({
			x: 0,
			y: 0,
			width: 650,
			height: 880,
			destWidth: 650,
			destHeight: 880,
			canvasId: 'firstCanvas',
			success: function (res) {
				console.log(res.tempFilePath)
				wx.saveImageToPhotosAlbum({
					filePath: res.tempFilePath,
					success(res) {
						if (e.currentTarget.id == 0) {
							wx.showToast({
								title: '图片保存成功',
							})
						}
						if (e.currentTarget.id == 1) {
							wx.showModal({
								title: '提示',
								content: '保存已保存成功！您可以用该图片去分享朋友圈哦',
								showCancel: false
							})
						}
					}
				})
			}
		})
	},
	// 首页大图标动作
	doSomething: function (e) {
		var that = this
		var index = e.currentTarget.id
		if (index == 1) {
			wx.scanCode({
				onlyFromCamera: true,
				success: (res) => {
					console.log(res)
					if (res.path == undefined) {
						wx.showModal({
							title: '提示',
							content: '亲，该二维码有误！',
							showCancel: false
						})
					} else {
						wx.navigateTo({
							url: '/' + res.path,
						})
					}
				}
			})
		} else if (index == 2) {
			wx.navigateTo({
				url: '../index/index',
			})
		} else if (index == 3) {
			wx.switchTab({
				url: '../me/me',
			})
		} else if (index == 4) {
			wx.navigateTo({
				url: '../myStorevalue/myStorevalue',
			})
		} else {
			canvas.Drawcanvas()
			that.setData({ showCanvas: !that.data.showCanvas })
		}
	},
	// 关闭分享图片
	onCancle: function (e) {
		this.setData({
			showCanvas: (!this.data.showCanvas)
		})
	},
	// 点击查看轮播大图
	previewSliderImgs: function (e) {
		var imageArray = this.data.sliderImgs
		var index = e.currentTarget.id
		var previewImage = imageArray[index]
		console.log(previewImage)
		wx.previewImage({
			current: previewImage,
			urls: imageArray
		})
	},
	// 点击查看门店大图
	previewStoreImgs: function (e) {
		var imageArray = this.data.storeImgs
		var index = e.currentTarget.id
		var previewImage = imageArray[index]
		console.log(previewImage)
		wx.previewImage({
			current: previewImage,
			urls: imageArray
		})
	},
	// 点击详情地址查看地图
	openLocation: function (e) {
		var Lat = e.currentTarget.dataset.lat
		var Lng = e.currentTarget.dataset.lng
		wx.openLocation({
			latitude: Lat,
			longitude: Lng,
			scale: 14
		})
	},
	// 拨打店铺电话
	makePhoneCall: function (e) {
		var phoneNumber = e.currentTarget.id
		wx.makePhoneCall({
			phoneNumber: phoneNumber,
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {

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
		// if (this.data.isOnload != 0) {
		// 	util.GetAgentConfigInfo(this) //刷新水印
		// }
		// this.data.isOnload++
		var that = this
		if (app.globalData.userInfo.UserId == undefined) {
			app.getUserInfo(function (e) {
				util.GetAgentConfigInfo(that) //刷新水印
				that.GetFoodsDetail()
				util.GetVipInfo(that)
			})
		} else {
			util.GetAgentConfigInfo(that) //刷新水印
			that.GetFoodsDetail()
			util.GetVipInfo(that)
		}
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
		this.data.first = 0
		this.data.isonload = 0
		util.GetVipInfo(this)
		util.GetAgentConfigInfo(this) //刷新水印
		this.GetFoodsDetail() //店铺配置
		setTimeout(function () {
			wx.showToast({
				title: '刷新成功',
			})
			wx.stopPullDownRefresh()
		}, 1500)
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
		if (this.data.postdata.food.funJoinModel.shareConfig.ADTitle != null && this.data.postdata.food.funJoinModel.shareConfig.ADImg != []) {
			return {
				title: this.data.postdata.food.funJoinModel.shareConfig.ADTitle,
				path: '/pages/home/home',
				imageUrl: this.data.postdata.food.funJoinModel.shareConfig.ADImg[0],
				success: function (res) {
					//转发成功
				}
			}
		}
	},
	// 获取首页显示数据
	GetFoodsDetail: function (e) {
		var that = this
		wx.request({
			url: addr.Address.GetFoodsDetail,
			data: {
				AppId: app.globalData.appid,
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.data.openDate = []
					// var opendate = (res.data.postdata.food.OpenDateStr).split('周')
					// if (that.data.isonload == 0) {
					// 	for (var v = 0; v < that.data.opentime.length; v++) {
					// 		that.data.opentime[v].condition = false
					// 	}
					// 	that.data.isonload++
					// }
					// for (var i = 0; i < opendate.length; i++) {
					// 	var template = that.data.opentime.find(f => f.date == opendate[i])
					// 	if (template) {
					// 		template.condition = true
					// 	}
					// }
					// for (var j = 0; j < that.data.opentime.length; j++) {
					// if (that.data.opentime[j].condition == true) {
					// if (that.data.first == 0) {
					// that.data.openDate += '周' + that.data.opentime[j].date
					// that.data.openDate.push('周' + that.data.opentime[j].date)
					// that.data.first++
					// }
					// }
					// else {
					// if (that.data.opentime[j + 1].condition != false) {
					// that.data.openDate += '至周' + that.data.opentime[j - 1].date + ','
					// that.data.openDate.push('周' + that.data.opentime[j - 1].date)
					// that.data.first = 0
					// }
					// }
					// if (that.data.opentime[j].date == '日') {
					// that.data.openDate += '至周' + that.data.opentime[j].date + ','
					// that.data.openDate.push('周' + that.data.opentime[j].date)
					// that.data.first = 0
					// }
					// }
					// for (var l = 1; l < that.data.openDate.length; l++) {
					// 	if (that.data.openDate[l - 1] != that.data.openDate[l]) {
					// 		that.data.showtime += that.data.openDate[l - 1] + '至' + that.data.openDate[l] + ','
					// 		l++
					// 	} else {
					// 		that.data.showtime += that.data.openDate[l] + ','
					// 		l++
					// 	}
					// }
					//判断首页开关 *特殊 外卖与堂食开关
					if (res.data.postdata.food.funJoinModel.takeOut) {
						var TakeOut = true
					} else {
						var TakeOut = false
					}
					if (res.data.postdata.food.funJoinModel.theShop) {
						var TheShop = true
					} else {
						var TheShop = false
					}
					if (res.data.postdata.food.funJoinModel.saveMoney == true && res.data.postdata.food.funJoinModel.canSaveMoneyFunction == true) {
						var savemoney = true
					} else {
						var savemoney = false
					}
					that.data.item[0].condition = TheShop
					that.data.item[1].condition = TakeOut
					that.data.item[2].condition = res.data.postdata.food.funJoinModel.vipCard
					that.data.item[3].condition = savemoney
					that.data.item[4].condition = res.data.postdata.food.funJoinModel.theShard
					that.setData({
						// showtime: that.data.showtime,
						// opentime: that.data.opentime,
						// openDate: that.data.openDate,
						postdata: res.data.postdata,
						sliderImgs: res.data.postdata.sliderImgUrls,
						storeImgs: res.data.postdata.storeImgs,
						item: that.data.item
					})
					app.globalData.canSaveMoneyFunction = res.data.postdata.food.funJoinModel.canSaveMoneyFunction
					app.globalData.TelePhone = res.data.postdata.food.TelePhone
					app.globalData.shareConfig = res.data.postdata.food.funJoinModel.shareConfig
					// console.log(res.data.postdata.food.OpenDateStr)
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
})