// pages/goodsList/goodsList.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var canvas = require("../../utils/canvas.js");
var app = getApp();
Page({

	/**
	 * 页面的初始数据
	 */
	data: {
		showCanvas: true,//分享按钮显隐
		IsOpen: 0,//一键分享
		ShareImg: '',//一键分享图片
		// 商品列表
		goods: [],
		value: '',
		goodname: "",
		orderbyid: 0,
		condition: true,
		condition1: true,
	},
	navTogoodList: function (e) {
		var id = e.currentTarget.id
		wx.navigateTo({
			url: '../goodList/goodList?id=' + id
		})
	},
	/**
	 * 生命周期函数--监听页面加载
	 */
	onLoad: function (options) {
		// 获取商品列表
		this.GetGoodsList(this.data.goodname)
		this.GetShareImg()
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
	// 关键字搜索商品
	searchGood: function () {
		// 获取商品列表
		this.GetGoodsList()
	},
	// 搜索综合
	searchAll: function () {
		this.data.orderbyid = 0
		this.GetGoodsList('')
	},
	value: function (e) {
		var value1 = e.detail.value
		this.data.goodname = value1
		this.GetGoodsList(value1)
	},
	// 按销量排序
	orderbySold: function () {
		var condition = this.data.condition
		if (this.data.orderbyid == 5) {
			this.data.orderbyid = 6
			this.setData({ condition: false })
		}
		else {
			this.data.orderbyid = 5
			this.setData({ condition: true })
		}
		this.GetGoodsList('')
	},
	// 按价格排序
	orderbyPrice: function () {
		var condition1 = this.data.condition1
		if (this.data.orderbyid == 1) {
			this.data.orderbyid = 2
			this.setData({ condition1: false })
		}
		else {
			this.data.orderbyid = 1
			this.setData({ condition1: true })
		}
		this.GetGoodsList('')
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
		var that = this
		return {
			title: app.globalData.shareConfig.StoreName,
			path: '/pages/index/index',
			imageUrl: app.globalData.shareConfig.ADImg[0].url,
			success: function (res) {
				app.showToast("转发成功")
			}
		}
	},
	// 获取商品列表
	GetGoodsList: function (goodname) {
		var that = this
		wx.request({
			url: addr.Address.getClassify,
			data: {
				appid: app.globalData.appid,
				typeid: 0,
				pageindex: 1,
				pagesize: 10,
				orderbyid: that.data.orderbyid,
				goodname: goodname,
				levelid: app.globalData.levelid
			},
			method: "GET",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok == 1)
					var goods = res.data.postdata.goodslist
				for (var i = 0; i < goods.length; i++) {
					if (goods[i].PriceStr.length > 7) {
						goods[i].PriceStrCount = parseFloat(goods[i].PriceStr / 10000).toFixed(2)
						goods[i].discountPricestrCount = parseFloat(goods[i].discountPricestr / 10000).toFixed(2)
					}
				}
				that.setData({
					goods: goods,
					sorts: res.data.postdata.goodsTypeList
				})
				that.data.pageindex++
				that.data.loading = 0
			},
			fail: function () {
				console.log("获取首页出错")
				wx.showToast({
					title: '获取首页出错',
				})
			}
		},
		)
	},
	// 跳转到分享页面
	navtoShare: function () {
		canvas.Drawcanvas(this)
		this.setData({ showCanvas: !this.data.showCanvas })
	},
	// 关闭分享图片
	onCancle: function (e) {
		this.setData({
			showCanvas: (!this.data.showCanvas)
		})
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
	//获取一键分享信息
	GetShareImg: function () {
		var that = this
		wx.request({
			url: addr.Address.GetShareImg,
			data: {
				AppId: app.globalData.appid
			},
			method: "GET",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				app.globalData.shareConfig = res.data.obj
				that.setData({
					IsOpen: res.data.obj.IsOpen
				})
			},
			fail: function (e) {
				console.log('一键分享获取失败')
			}
		})
	},
})