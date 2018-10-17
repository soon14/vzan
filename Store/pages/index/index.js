var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var canvas = require("../../utils/canvas.js");
var mulpicker = require("../../public/mulpicker.js");
var http = require("../../utils/http.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		showCanvas: true,//分享按钮显隐
		IsBargainOpen: 2,//判断砍价是否开启
		model: [],//vip信息
		nowTime: 0,//获取目前系统的时间毫秒
		item1: [
			{
				img: '/image/test.png', yuanjia: '980.00', xianjia: '900.00', jieshushijian: '1', neirong: '华为P932G版手机华为P932G版手机华为P932G版手机华为P932G版手机华为P932G版手机华为P932G版手机', shengyu: '30'
			},
			{
				img: '/image/test.png', yuanjia: '980.00', xianjia: '900.00', jieshushijian: '活动结束', neirong: '的撒的撒的撒过分个人', shengyu: '30'
			},
			{
				img: '/image/test.png', yuanjia: '980.00', xianjia: '900.00', jieshushijian: '-1', neirong: '爱上大声地', shengyu: '30'
			},
		],
		IsOpen: 0,//一键分享
		ShareImg: '',//一键分享图片
		dots: false,
		sort: [],// 导航分类 swiper翻页效果
		sort1: [],
		sort2: [],
		sort3: [],
		sort4: [],
		// 商品模板
		goods: [],
		// 轮播图
		imgUrls: [],
		// 轮播图属性
		indicatorDots: false,
		autoplay: true,
		interval: 4000,
		duration: 1000,
		pageindex: 1,
		loading: 0,
	},
	navo_webview: function () {
		app.goNewPage('/pages/me/web_view?id=' + this.data.AgentConfig.QrcodeId)
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
	// 跳转到商品搜索页面
	navTogoodsList: function () {
		wx.navigateTo({
			url: '../goodsList/goodsList'
		})
	},
	// 跳转到分类底部导航栏
	switchtoClassify: function () {
		wx.switchTab({
			url: '../classify/classify',
		})
	},
	// 已结束按钮
	showTimeout: function () {
		wx.showToast({
			title: '活动已结束',
			icon: 'loading',
			duration: 1000
		})
	},
	// 已结束按钮
	showLoading: function () {
		wx.showToast({
			title: '莫慌，还没开始',
			icon: 'loading',
			duration: 1000
		})
	},
	// 商品已售罄
	showSoldout: function () {
		wx.showToast({
			title: '商品已售罄',
			icon: 'loading',
			duration: 1000
		})
	},
	// 跳转到砍价列表
	navtoAllbargain: function () {
		wx.navigateTo({
			url: '../allbargain/allbargain',
		})
	},
	// 点击查看大图
	previewImage: function (e) {
		var imageArray = this.data.imgUrls
		var index = e.currentTarget.id
		var previewImage = imageArray[index]
		console.log(previewImage)
		wx.previewImage({
			current: previewImage,
			urls: imageArray
		})
	},
	navtoBargaindetail: function (e) {
		var loadingT = e.currentTarget.dataset.loadingt
		var Id = e.currentTarget.id
		wx.navigateTo({
			url: '../bargaindetail/bargaindetail?Id=' + Id,
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		setInterval(function () { // 倒计时
			that.setData({
				nowTime: new Date().getTime()
			})
		}, 1000)
		// getApp().onekeyshare
		app.getUserInfo(function (e) {
			that.GetAgentConfigInfo()
			that.inite4()
			that.requestTitle()
			that.inite()
			that.inite3()
			that.inite5()
			that.GetShareImg(this)
			that.inite6()
			that.initGroupList()
		})
	},
	// 点击商品跳转到详情页
	navToGoodList: function (e) {
		var id = e.currentTarget.id
		wx.navigateTo({
			url: '../goodList/goodList?id=' + id
		})
	},
	// 跳转到分享页面
	navtoShare: function () {
		canvas.Drawcanvas(this)
		this.setData({ showCanvas: !this.data.showCanvas })
	},
	// 跳转到分类页面
	navToclassify: function (e) {
		var that = this
		var id = e.currentTarget.id
		app.globalData.typeid = id
		app.globalData.change = 1
		wx.switchTab({
			url: '../classify/classify'
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
		this.GetStoreConfig()
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
		this.data.pageindex = 1
		this.data.goods = []
		this.initGroupList()
		this.inite()
		this.inite3()
		this.inite4()
		this.inite5()
		this.GetShareImg(this)
		this.GetStoreConfig()
		this.requestTitle()
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
		if (this.data.loading == 0) {
			this.data.loading = 1
			this.inite2(0, this.data.pageindex, this.data.levelid, 1)
		}
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
	//初始化
	inite: function (e) {
		var that = this
		wx.request({
			url: addr.Address.getIndexinformation,
			data: {
				appid: app.globalData.appid
			},
			method: "GET",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				var typelist = []
				var typelist1 = []
				var typelist2 = []
				var typelist3 = []
				var typelist4 = []
				if (res.data.isok == 1) {
					for (var i = 0; i <= 9; i++) {
						if (res.data.postdata.typelist[i] != undefined) {
							typelist.push(res.data.postdata.typelist[i])
						}
					}
					for (var i = 10; i <= 19; i++) {
						if (res.data.postdata.typelist[i] != undefined) {
							typelist1.push(res.data.postdata.typelist[i])
						}
					}
					for (var i = 20; i <= 29; i++) {
						if (res.data.postdata.typelist[i] != undefined) {
							typelist2.push(res.data.postdata.typelist[i])
						}
					}
					for (var i = 30; i <= 39; i++) {
						if (res.data.postdata.typelist[i] != undefined) {
							typelist3.push(res.data.postdata.typelist[i])
						}
					}
					for (var i = 40; i <= 49; i++) {
						if (res.data.postdata.typelist[i] != undefined) {
							typelist4.push(res.data.postdata.typelist[i])
						}
					}
					that.setData({
						imgUrls: res.data.postdata.dataimgs,
						sort: typelist,
						sort1: typelist1,
						sort2: typelist2,
						sort3: typelist3,
						sort4: typelist4
					})
				}
			},
			fail: function (e) {
				console.log(e)
				wx.showToast({
					title: '获取首页出错',
				})
			}
		}
		)
	},
	// 获取首页商品
	inite2: function (typeid, pageindex, levelid, isReachbottom) { //isReachbottom=0正常 1上拉加载更多
		var that = this
		wx.request({
			url: addr.Address.getClassify,
			data: {
				levelid: levelid,
				appid: app.globalData.appid,
				typeid: typeid,
				pageindex: pageindex,
				pagesize: 10,
				orderbyid: 0
			},
			method: "GET",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok == 1)
					for (var i = 0; i < res.data.postdata.goodslist.length; i++) {
						if (res.data.postdata.goodslist[i].PriceStr.length > 7) {
							res.data.postdata.goodslist[i].PriceStrCount = parseFloat(res.data.postdata.goodslist[i].PriceStr / 10000).toFixed(2)
							res.data.postdata.goodslist[i].discountPricestrCount = parseFloat(res.data.postdata.goodslist[i].discountPricestr / 10000).toFixed(2)
						}
					}
				if (isReachbottom == 0) {
					var goods = res.data.postdata.goodslist
					that.data.pageindex++
				}
				if (isReachbottom == 1) {
					var goods = that.data.goods
					if (res.data.postdata.goodslist.length > 0) {
						goods = goods.concat(res.data.postdata.goodslist)
						that.data.pageindex++
					}
				}
				that.setData({
					goods: goods,
					sorts: res.data.postdata.goodsTypeList
				})
				that.data.loading = 0
			},
			fail: function () {
				console.log("获取首页出错")
				wx.showToast({
					title: '获取首页出错',
				})
			}
		})
	},
	// 获取砍价列表
	inite3: function () {
		var that = this
		wx.request({
			url: addr.Address.GetBargainList, //仅为示例，并非真实的接口地址
			data: {
				appid: app.globalData.appid,
				pageIndex: 1,
				pageSize: 4,
				IsEnd: -1
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				var bargainList = res.data
				for (var i = 0; i < bargainList.length; i++) {
					bargainList[i].EndDate = that.ChangeDateFormat(bargainList[i].EndDate)
					bargainList[i].StartDate = that.ChangeDateFormat(bargainList[i].StartDate)
				}
				that.setData({ bargainList: bargainList })
			},
			fail: function () {
				console.log("获取信息出错")
				wx.showToast({
					title: '获取信息出错',
				})
			}
		})
	},
	requestTitle: function (isShowLoading = true) {
		var that = this
		wx.request({
			url: addr.Address.GetImg,
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
				console.log("获取首页出错")
			},
			complete: function () {
				wx.stopPullDownRefresh()
				util.hideNavigationBarLoading()
			}
		})
	},
	// 砍价按钮
	bargainButton: function (e) {
		var Id = e.currentTarget.id
		wx.navigateTo({
			url: '../bargaindetail/bargaindetail?Id=' + Id + '&isCut=1',
		})
	},
	ChangeDateFormat: function (val) {
		if (val != null) {
			var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", "").replace("-", "/"), 10));
			//月份为0-11，所以+1，月份小于10时补个0
			var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
			var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
			var hour = date.getHours();
			var minute = date.getMinutes();
			var second = date.getSeconds();
			var dd = date.getFullYear() + "/" + month + "/" + currentDate + " " + hour + ":" + minute + ":" + second;
			return dd;
		}
		return "";
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
				} else {
					return
				}
			},
			fail: function (e) {
				console.log('一键分享获取失败')
			}
		})
	},
	// 获取会员信息
	inite4: function (e) {
		var that = this
		wx.request({
			url: addr.Address.GetVipInfo, //仅为示例，并非真实的接口地址
			data: {
				appid: app.globalData.appid,
				uid: app.globalData.userInfo.UserId,
			},
			method: "POST",
			header: {
				'content-type': 'application/x-www-form-urlencoded' // 默认值
			},
			success: function (res) {
				if (res.data.isok) {
					that.setData({
						model: res.data.model,
						levelid: res.data.model.levelid
					})
					var levelid = res.data.model.levelid
					app.globalData.levelid = res.data.model.levelid
					that.inite2(0, 1, app.globalData.levelid, 0)
				}
			},
			fail: function () {
				console.log('获取不了会员信息')
			}
		})
	},
	// 查询砍价是否开启
	inite5: function (e) {
		var that = this
		wx.request({
			url: addr.Address.GetBargainOpenState, //仅为示例，并非真实的接口地址
			data: {
				appid: app.globalData.appid,
			},
			method: "GET",
			header: {
				'content-type': 'application/x-www-form-urlencoded' // 默认值
			},
			success: function (res) {
				if (res.data.isok) {
					that.setData({
						IsBargainOpen: res.data.obj.IsBargainOpen
					})
				}
			},
			fail: function () {
				console.log('获取不了砍价开关')
			}
		})
	},
	// 获取储值余额
	inite6: function (e) {
		var that = this
		wx.request({
			url: addr.Address.getSaveMoneySetUser, //仅为示例，并非真实的接口地址
			data: {
				appid: app.globalData.appid,
				openId: app.globalData.userInfo.openId,
			},
			method: "GET",
			header: {
				'content-type': 'application/x-www-form-urlencoded' // 默认值
			},
			success: function (res) {
				if (res.data.isok == true) {
					app.globalData.myMoeny = res.data.saveMoneySetUser.AccountMoneyStr
				}
			},
			fail: function () {
				console.log('获取不了会员信息')
			}
		})
	},
	//初始化拼团列表
	initGroupList: function () {
		var that = this;
		http
			.postAsync(addr.Address.GetGroupListPage, { appId: app.globalData.appid })
			.then(function (data) {
				that.setData({ "vmGrouplist.list": data.postdata });
			});
	},
	clickGroupItem: function (e) {
		var _groupid = e.currentTarget.dataset.groupid;
		wx.navigateTo({
			url: '/pages/groupDetail/groupDetail?id=' + _groupid,
		})
	},
	//获取店铺配置
	GetStoreConfig: function () {
		var that = this
		var url = addr.Address.GetStoreConfig
		var param = {
			appid: app.globalData.appid,
		}
		var method = "GET"
		network.requestData(url, param, method, function (e) {
			if (e.data.isok == 1) {
				var phone = e.data.postdata.store.TelePhone
				app.globalData.canSaveMoneyFunction = e.data.postdata.store.funJoinModel.canSaveMoneyFunction
				app.globalData.customerphone = phone
			}
		})
	},
	// 水印
	GetAgentConfigInfo: function (e) {
		var that = this
		wx.request({
			url: addr.Address.GetAgentConfigInfo, //仅为示例，并非真实的接口地址
			data: {
				appid: app.globalData.appid,
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					if (res.data.AgentConfig.isdefaul == 0) {
						res.data.AgentConfig.LogoText = res.data.AgentConfig.LogoText.split(' ')
					} else {
						res.data.AgentConfig.LogoText = res.data.AgentConfig.LogoText
					}
					that.setData({
						AgentConfig: res.data.AgentConfig,
					})
				}
			},
			fail: function () {
				console.log('获取不了水印')
			}
		})
	},
})