// pages/classify/classify.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var canvas = require("../../utils/canvas.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		showCanvas: true,//分享按钮显隐
		ishave: 0,//上拉加载更多参数
		IsOpen: 0,//一键分享
		ShareImg: '',//一键分享图片
		// 分类
		sorts: [],
		// selected: 0,
		// 商品模板
		goods: [],
		index: 0,
		index1: 0,//默认产品id -1全部
		pageindex: 0,
		loading: 0,
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		if (app.globalData.typeid > 0) {
			this.setData({ index1: app.globalData.typeid })
			this.inite(app.globalData.typeid)
		}
		else {
			this.inite(0)
		}
		this.GetShareImg(this)
	},
	//初始化
	inite: function (typeid) {
		if (typeid != undefined) {
			this.GetGoodsList(typeid, 1, 1)
		}
		else {
			this.GetGoodsList(-1, 1, 0)
		}
	},
	// 获取更多商品
	getMoregoods: function () {
		if (this.data.loading == 0) {
			this.data.loading = 1
			this.GetGoodsList(this.data.index1, this.data.pageindex + 1, 0)
		}
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
	// 点击分类切换对应商品
	changeGoods: function (e) {
		console.log(e)
		this.data.pageindex = 0
		this.data.goods = []
		this.GetGoodsList(e.currentTarget.id, 1, 0)
		console.log('`', this.data.sorts)
	},
	// 点击商品跳转到详情页
	navToGoodList: function (e) {
		var id = e.currentTarget.id
		wx.navigateTo({
			url: '../goodList/goodList?id=' + id
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
		if (this.data.ishave == 0) {
			this.data.ishave++
		} else {
			this.data.goods = []
			this.GetShareImg(this)
			this.GetGoodsList(app.globalData.typeid, 1, 1)
			this.data.pageindex = 0
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
	GetGoodsList: function (typeid, pageindex, condition) {
		var that = this
		wx.request({
			url: addr.Address.getClassify,
			data: {
				appid: app.globalData.appid,
				typeid: typeid,
				pageindex: pageindex,
				pagesize: 6,
				levelid: app.globalData.levelid
			},
			method: "GET",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok == 1) {
					for (var i = 0; i < res.data.postdata.goodslist.length; i++) {
						if (res.data.postdata.goodslist[i].PriceStr.length > 7) {
							res.data.postdata.goodslist[i].PriceStrCount = parseFloat(res.data.postdata.goodslist[i].PriceStr / 10000).toFixed(2)
							res.data.postdata.goodslist[i].discountPricestrCount = parseFloat(res.data.postdata.goodslist[i].discountPricestr / 10000).toFixed(2)
						}
					}
					if (condition == 0) { //condition==0表示上拉加载更多
						var goods = that.data.goods
						if (res.data.postdata.goodslist.length > 0) {
							goods = goods.concat(res.data.postdata.goodslist)
						}
					} else {
						goods = res.data.postdata.goodslist
					}
					that.setData({
						goods: goods,
						sorts: res.data.postdata.goodsTypeList,
						typeid: typeid,
						index1: typeid
					})
					that.data.pageindex++
					that.data.loading = 0
					console.log(res)
				}
				app.globalData.typeid = typeid
			},
			fail: function () {
				console.log("获取分类出错")
				wx.showToast({
					title: '获取分类出错',
				})
			}
		},
		)
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
				if (res.data.isok) {
					app.globalData.shareConfig = res.data.obj
					that.setData({
						IsOpen: res.data.obj.IsOpen
					})
				}
			},
			fail: function (e) {
				console.log('一键分享获取失败')
			}
		})
	},
})