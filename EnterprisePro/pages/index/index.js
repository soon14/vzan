// pages/index/index.js
const app = getApp();
const util = require("../../utils/util.js");
const tools = require("../../utils/tools.js");
const canvas = require('../../utils/canvas.js');
const page = require("../../utils/pageRequest.js");
const animation = require("../../utils/animation.js");
const pickerFile = require('../../template/picker/js/picker_datetime.js')

var listViewModal = {
	pageindex: 1,
	pagesize: 10,
	list: [],
	ispost: false,
	loadall: false,
	ids: '',
	btnType: '',
}
var _goodListViewModal = {
	pageindex: 1,
	pagesize: 10,
	list: [],
	ispost: false,
	loadall: false,
	pricesort: "",
	exttypes: "",
	search: "",
	text: "价格",
	price: [
		{ context: "价格不限", id: "0" },
		{ context: "价格由高到低", id: "1" },
		{ context: "价格由低到高", id: "2" },
	],
	inshow: 0
};
// 各状态判断

Page({
	data: {
		currentPage: null,
		isIndex1: 0, //点击跳转
		showCanvas: true, //分享卡片开关
		// 表单组件
		listViewModal_form: {
			pageindex: 1,
			pagesize: 10,
			list: {},
			ispost: false,
			loadall: false,
		},
		// 产品列表组件
		goodListViewModal: JSON.parse(JSON.stringify(_goodListViewModal)),
		extId: [],
		search: "",
		pricesort: "",
		// 各状态判断
		status: {
			_homeClose: false,
			_shareShow: false,
			_customer: false,
			_buyShow: false,
			_yuyueShow: false,
			_makecall: false,
			_goodsShow: false,
			_bgmusic: false,
			_haveReductionmoney: false,
			_shopRed: false,
			sIcon: "",
			cIcon: "",
			pIcon: "",
			pNumber: "",
			showModalStatus: false,//排序动画
			showMadalFilterStatus: false, //筛选动画
			kipperMark: false,
			openTelSuspend: false,  //开启电话图标悬浮
			openServiceSuspend: false,  //开启客服图标悬浮
			contactPhone: "",  //联系店主组件的拨打电话号码
			openWxShopMessage: false, //联系店主的详情推送框
			headImg: "",
			nickName: "",
		}
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		let that = this
		console.log('storecodeid', getApp().globalData.storecodeid)
		let PageSetting = wx.getStorageSync("PageSetting");
		let pageShare = wx.getStorageSync("pageShare");
		let ver2 = 1.5
		let ver1 = parseFloat(wx.getSystemInfoSync().SDKVersion)
		if (ver1 < ver2 || wx.getSystemInfoSync().SDKVersion == undefined) {
			return;
		}
    /*
    分享进来的没有授权信息所以必须要执行一遍授权方法
    产品列表下拉刷新，会再次调用onLoad但是没有传options所以，下面必须判断options
    */
		if (options && options.isIndex1) {
			that.setData({ isIndex1: options.isIndex1 || 0 })
		}
		//用户授权
		that.canUser()
		//底部水印
		page.logoRequest(that)
		//自定义分享
		if (pageShare) {
			that.data.pageShare = pageShare
		} else {
			page.pageShare(that).then(data => {
				that.data.pageShare = data.obj
			})
		}
		// 时间选择动画
		that.datetimePicker = new pickerFile.pickerDatetime({
			page: this,
			animation: 'slide',
			duration: 200,
		});
	},
	// 用户授权
	canUser: function () {
		let that = this
		app.getUserInfo(res => {
			wx.getSetting({
				success: (data) => {
					//授权时
					if (data.authSetting["scope.userInfo"]) {
						page.memberInfo(res.UserId, that).then(data => {
							page.aidRequest(that)
							page.GetStoreInfo(that, 1)
						})
					} else {
						page.aidRequest(that)
					}
					that.data.authSetting = data.authSetting["scope.userInfo"]
				}
			})
		})
	},
	// 图片等跳转内页
	goNewPage: function (e) {
		let that = this
		let ds = e.currentTarget.dataset
		let index = e.currentTarget.id
		let [isIndex1, items, img, btnType, templateType] = [ds.id, ds.items, ds.img, ds.btntype, ds.type]
		if (templateType == 'img') {//如果是图片组件
			if (ds.urltype == -1) {//不跳转，点击查看大图
				let imgs = []; imgs.push(img)
				util.preViewShow(img, imgs)
			}
			else if (ds.urltype == 0) {//更换index渲染内容
				if (ds.url >= 0) {
					wx.redirectTo({
						url: '../index/index?isIndex1=' + ds.url,
					})
				} else {
					let imgs = []; imgs.push(img)
					util.preViewShow(img, imgs)
				}
			}
			else if (ds.urltype == 1) { //跳转到小程序
				wx.navigateToMiniProgram({
					appId: ds.appid,
					path: ds.path,
					success(res) {
						console.log(res)
					},
					fail(err) {
						console.log(err)
						wx.showModal({
							title: "提示",
							content: "跳转失败",
							showCancel: false,
						})
						return
					}
				})
			}
			else if (ds.urltype == 2) {//链接更换index渲染内容或者扫码购物
				if (Number(ds.furl) != -1) { //新增扫码购物代码
					if (Number(ds.furl) == 4) { //如果是扫码购物
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
								} else {//扫码成功操作
									wx.showToast({ title: '扫码成功', })
									getApp().globalData.storecodeid = res.path.split('?scene=')[1]
									if (ds.url != -1) {
										wx.redirectTo({
											url: '../index/index?isIndex1=' + ds.url,
										})
										// tools.goNewPage('../index/index?isIndex1=' + ds.url)
									}

								}
							}
						})
					} else {
						tools.goNewPage('/' + ds.furl)
					}
				} else { //原图片跳转代码
					if (ds.url == -1) {
						let imgs = []; imgs.push(img)
						util.preViewShow(img, imgs)
					} else {
						tools.goNewPage('../index/index?isIndex1=' + ds.url)
					}
				}
			}
			else {//产品详情页
				let [productId, showprice] = [items[0].id, items[0].showprice]
				let url = '../detail/detail?id=' + productId + "&typeName=" + btnType + "&showprice=" + showprice
				tools.goNewPage(url)
			}
		}
		else if (templateType == 'slider') {
			let item = items[Number(index)];
			if (item.urltype === undefined) {
				if (isIndex1 != -1 && isIndex1 != "") {
					tools.goNewPage('../index/index?isIndex1=' + isIndex1);
				}
			}
			else {
				switch (item.urltype) {
					//不跳转
					case -1:
						let imgs = []
						imgs.push(item.img)
						util.preViewShow(item.img, imgs)
						break;
					//跳转到页面
					case 0:
						if (item.url !== -1 && item.url !== "") {
							tools.goNewPage('../index/index?isIndex1=' + item.url);
						}
						break;
					//跳转到小程序
					case 1:
						wx.navigateToMiniProgram({
							appId: item.appid,
							path: item.path,
							//extraData: {},  //跳转小程序传递的参数对象
							// envVersion: 'trial', //默认跳转正式版
							success(res) {
								console.log(res)
							},
							fail(err) {
								console.log(err)
								wx.showModal({
									title: "提示",
									content: "跳转失败",
								})
							}
						})
						break;
				}
			}
		}
		else {
			let sliderImgs = []
			let sliderimg = items[index].img
			for (let i = 0, valImg; valImg = items[i++];) {
				sliderImgs.push(valImg.img)
			}
			isIndex1 != -1 && isIndex1 != "" ?
				tools.goNewPage('../index/index?isIndex1=' + isIndex1) :
				util.preViewShow(sliderimg, sliderImgs)
		}

	},
	// 页面跳转
	pagesGoto: function (e) {
		let that = this
		let ds = e.currentTarget.dataset
		let isIndex1 = parseInt(e.currentTarget.id) // page页数
		if (isIndex1 === -1 && ds.furl == -1 && ds.urltype != 1 || isIndex1 === '' && ds.furl == '' && ds.urltype != 1) {
			tools.showLoadToast("未设置跳转")
			return;
		} else {
			if (ds.type == 'imgnav') {//图片

				if (ds.urltype == -1) {//不跳转，点击查看大图
					wx.showToast({ title: '未设置跳转', icon: 'loading' })
					return
				}
				else if (ds.urltype == 0) {//更换index渲染内容
					if (isIndex1 >= 0) {
						wx.redirectTo({
							url: '../index/index?isIndex1=' + isIndex1,
						})
					} else {
						wx.showToast({ title: '未设置跳转', icon: 'loading' })
						return
					}
				}
				else if (ds.urltype == 1) { //跳转到小程序
					wx.navigateToMiniProgram({
						appId: ds.appid,
						path: ds.path,
						success(res) {
							console.log(res)
						},
						fail(err) {
							console.log(err)
							wx.showModal({
								title: "提示",
								content: "跳转失败",
								showCancel: false,
							})
							return
						}
					})
				}
				else if (ds.urltype == 2) {//链接更换index渲染内容或者扫码购物
					if (Number(ds.furl) != -1) { //新增扫码购物代码
						if (Number(ds.furl) == 4) { //如果是扫码购物
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
									} else {//扫码成功操作
										wx.showToast({ title: '扫码成功', })
										getApp().globalData.storecodeid = res.path.split('?scene=')[1]
										if (isIndex1 != -1) {
											wx.redirectTo({
												url: '../index/index?isIndex1=' + isIndex1,
											})
										}

									}
								}
							})
						} else {
							tools.goNewPage('/' + ds.furl)
						}
					} else { //原图片跳转代码
						if (ds.url == -1) {
							let imgs = []; imgs.push(img)
							util.preViewShow(img, imgs)
						} else {
							tools.goNewPage('../index/index?isIndex1=' + isIndex1)
						}
					}
				}

			} else { //底部导航栏
				if (ds.type == 'bottomnav') {
					page.pageset(that, app.globalData.pages, isIndex1).then(data => { //图片或者底部导航
						if (app.globalData._bgFirst == false) {
							wx.playBackgroundAudio({ dataUrl: data })
						}
					})
					that.setData({
						currentPage: app.globalData.pages[isIndex1], // 根据isindex1跳转相对页面
						isIndex1: isIndex1,
					})
				} else {//魔方图片
					if (ds.urltype == -1) {//不跳转，点击查看大图
						wx.showToast({ title: '未设置跳转', icon: 'loading' })
						return
					}
					else if (ds.urltype == 0) {//更换index渲染内容
						if (isIndex1 >= 0) {
							wx.redirectTo({
								url: '../index/index?isIndex1=' + isIndex1,
							})
						} else {
							wx.showToast({ title: '未设置跳转', icon: 'loading' })
							return
						}
					}
					else if (ds.urltype == 1) { //跳转到小程序
						wx.navigateToMiniProgram({
							appId: ds.appid,
							path: ds.path,
							success(res) {
								console.log(res)
							},
							fail(err) {
								console.log(err)
								wx.showModal({
									title: "提示",
									content: "跳转失败",
									showCancel: false,
								})
								return
							}
						})
					}
					else if (ds.urltype == 2) {//链接更换index渲染内容或者扫码购物
						if (Number(ds.furl) != -1) { //新增扫码购物代码
							if (Number(ds.furl) == 4) { //如果是扫码购物
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
										} else {//扫码成功操作
											wx.showToast({ title: '扫码成功', })
											getApp().globalData.storecodeid = res.path.split('?scene=')[1]
											if (isIndex1 != -1) {
												wx.redirectTo({
													url: '../index/index?isIndex1=' + isIndex1,
												})
											}

										}
									}
								})
							} else {
								tools.goNewPage('/' + ds.furl)
							}
						}
						else { //原图片跳转代码
							if (ds.url == -1) {
								let imgs = []; imgs.push(img)
								util.preViewShow(img, imgs)
							} else {
								wx.redirectTo({
									url: '../index/index?isIndex1=' + isIndex1,
								})
							}
						}
					} else {
						wx.reLaunch({ url: '../index/index', })//返回主页home标志
					}
				}

			}
		}
	},
	// 各组件跳转
	templGoto: function (e) {
		let that = this
		let [ds, Id] = [e.currentTarget.dataset, Number(e.currentTarget.id)]
		let liveId = []
		let url = ""
		switch (Id) {
			case 0:
				that.data.currentPage.coms[ds.comindex].items.forEach(function (o, i) {
					liveId.push(o.id)
				});
				liveId = liveId.join(",");
				url = '../live/live?pageindex=' + ds.pageindex + "&comindex=" + ds.comindex + "&ids=" + liveId;
				break; // 直播
			case 1:
				url = '../contentAD/contentAD?typeid=' + ds.typeid + "&ids=" + ds.ids + "&mr=true";
				break; // 内容咨询查看更多
			case 2:
				url = '../contentAD/contentAD?id=' + e.currentTarget.dataset.id + "&dl=true";
				break; // 内容咨询详情

			case 3:
				let showQrcode = 0
				if (app.globalData.storeConfig.funJoinModel.productQrcodeSwitch) {
					showQrcode = 1
				}
				url = '../detail/detail?id=' + ds.id + "&typeName=" + ds.name + "&showprice=" + ds.showprice + "&showQrcode=" + showQrcode;
				break; // 产品详情
			case 4:
				url = '../shoppingCart/shoppingCart?isIndex1=';
				break; // 购物车
			case 5:
				url = '../subscribe/subscribe?pid=' + ds.id + '&name=' + ds.name + "&form=true";
				break; // 预约跳转
			case 6:
				url = '../subscribe/subscribe?sublist=true';
				break; //查看表单
			case 7:
				url = '../me/me?aid=' + wx.getStorageSync("aid");
				break;
			case 8:
				url = '../bargaindetail/bargaindetail?Id=' + ds.id;
				break; //砍价详情
			case 9:
				url = '../groupDetail/groupDetail?id=' + ds.group;
				break; //拼团
			default:
				url = '../getsmoney/mysmoney'
				break;
		}
		tools.goNewPage(url)
	},
	// 排序
	priceSortFunc: function (e) {
		let that = this;
		let [id, ds] = [Number(e.currentTarget.id), e.currentTarget.dataset]
		let [index, changText] = [ds.id, ds.content]
		tools.reset(that.data.goodListViewModal)
		let currStatus = "open"
		let condition = that.data.condition
		let pricesort = that.data.pricesort
		switch (id) {
			case 0:
				currStatus = "close"
				page.goodsListRequest(condition, pricesort, "", "", that);
				break;
			case 1:
				currStatus = "close"
				pricesort = "desc"
				page.goodsListRequest(condition, pricesort, "", "", that);
				break;
			case 2:
				currStatus = "close"
				pricesort = "asc"
				page.goodsListRequest(condition, pricesort, "", "", that);
				break;
			case 3:
				currStatus = "open"
				break;
			case 4:
				currStatus = "close"
				break;
		}
		that.setData({
			"goodListViewModal.text": changText,
			"goodListViewModal.inshow": index,
		})
		that.data.pricesort = pricesort
		animation.utilDown(currStatus, that);
	},
	//筛选
	fiFterFunc: function (e) {
		let that = this
		let extTypes_fmt = that.data.extTypes_fmt
		let [id, ds] = [Number(e.currentTarget.id), e.currentTarget.dataset]
		let [parentindex, childindex, sel] = [ds.parentindex, ds.childindex, ds.sel]
		let key = "extTypes_fmt[" + parentindex + "].child[" + childindex + "].sel"
		let condition = that.data.condition
		switch (id) {
			case 0:
				page.getExt(that)
				animation.utilFilter("open", that);
				break; //弹出
			case 1:
				animation.utilFilter("close", that);
				break; //收回
			case 2:
				tools.reset(that.data.goodListViewModal)
				page.goodsListRequest(condition, "", that.data.extId, "", that)
				that.data.extId = []
				animation.utilFilter("close", that)
				break; //确定
			case 3:
				for (let i = 0, val; val = extTypes_fmt[i++];) {
					for (let j = 0, key; key = val.child[j++];) {
						key.sel == true ? key.sel = false : "";
					}
				}
				that.data.extId = []
				that.setData({ extTypes_fmt: extTypes_fmt })
				break; //重置
			case 4:
				let template = extTypes_fmt[parentindex].child[childindex]
				that.setData({ [key]: !template.sel })
				if (template.sel) {
					let [parentId, childId] = [template.ParentId, template.TypeId]
					let exttypesId = parentId + "-" + childId
					that.data.extId.push(exttypesId)
				}
				break; //选择
		}
	},
	//搜索 分类导航
	proFunc: function (e) {
		let that = this
		let id = Number(e.currentTarget.id)
		let search = that.data.search
		if (e.currentTarget.dataset.id != undefined) {
			var condition = e.currentTarget.dataset.id
		} else {
			condition = that.data.condition
		}
		tools.reset(that.data.goodListViewModal)
		switch (id) {
			case 0:
				search = e.detail.value
				page.goodsListRequest(condition, "", "", search, that)
				break;
			case 1:
				wx.showLoading({
					title: '加载中...',
					mask: true,
					success: function () {
						Promise.all([page.goodsListRequest(condition, "", "", "", that)]).then(function (data) {
							if (data[0].isok == 1) {
								wx.hideLoading()
							}
						})
					}
				})
				that.setData({ condition: condition })
				break;
			case 2:
				wx.pageScrollTo({ scrollTop: 0 })
				break;
		}
		that.data.search = search
	},
	// 下拉加载更多
	onReachBottom: function () {
		let that = this
		page.goodsListRequest(that.data.condition, that.data.pricesort, that.data.extId, that.data.search, that)
	},
	// 上拉刷新
	onPullDownRefresh: function () {
		let that = this
		let app = getApp();
		wx.removeStorageSync("aid");
		wx.removeStorageSync("AgentConfig");
		wx.removeStorageSync("PageSetting");
		wx.removeStorageSync("pageShare");
		tools.showLoadToast("正在刷新")
		that.onLoad();
		that.onShow();
		setTimeout(res => {
			tools.showToast("刷新成功");
			wx.stopPullDownRefresh()
		}, 1500)
	},
	// 各组件功能
	pageFunc: function (e) {
		let that = this
		let ds = e.currentTarget.dataset
		let id = Number(e.currentTarget.id)
		switch (id) {
			case 0:
				tools.mapFunc(ds.lat, ds.lng)
				break; // 地图处理
			case 1:
				tools.phoneFunc(ds.phone)
				break; // 拨打电话
			case 2:
				canvas.inite3()
				that.setData({ showCanvas: (!that.data.showCanvas) })
				break; // 分享按钮
			case 3:
				that.setData({ showCanvas: (!that.data.showCanvas) })
				break; //收回
			case 4:
				let template = that.data.currentPage.coms[ds.childindex].sel
				let key = "currentPage.coms[" + ds.childindex + "].sel"
				that.setData({ [key]: !template })
				break; //视频
			case 5:
				that.datetimePicker.setPicker('startDate');
				break; //表单时间选择
			case 6:
				that.setData({ pickIndex: parseInt(e.detail.value) })
				break; //表单picker值
			case 7:
				tools.phoneFunc(ds.phone)   //联系店主拨打电话
				break;
		}
	},
	// 音乐背景播放
	playAudioFunc: function (e) {
		let that = this
		let [isPlay, src] = [true, that.data.src1]
		wx.playBackgroundAudio({ dataUrl: src })
		that.setData({ isPlay: !isPlay })
	},
	// 音乐背景暂停播放
	stopAudioFunc: function () {
		wx.stopBackgroundAudio()
		this.setData({ isPlay: !this.data.isPlay })
	},
	// 表单提交，提交真是姓名和手机号码
	sumbitFormFuc: function (e) {
		let that = this
		let [detail, comename] = [JSON.stringify(e.detail.value), e.detail.target.dataset.name]
		let showModalUser = false
		if (that.data.authSetting) {
			for (let key in e.detail.value) {
				if (e.detail.value[key] == '') {
					tools.showToast("信息未填写完整")
					return
				}
			}
			page.formRequest(detail, comename, that)
			showModalUser = false
		} else {
			showModalUser = true
		}
		that.setData({ showModalUser: showModalUser })
	},
	//拒绝授权时弹窗
	userFunc: function (e) {
		let id = Number(e.target.id)
		switch (id) {
			case 0:
				this.setData({
					showModalUser: false
				})
				break;
			case 1:
				wx.openSetting({
					success: (data) => {
						this.canUser()
						data.authSetting = { "scope.userInfo": true }
						this.setData({ showModalUser: false })
					}
				})
				break;
		}
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
			success: data => {
				wx.saveImageToPhotosAlbum({
					filePath: data.tempFilePath,
					success(res) {
						if (e.currentTarget.id == 0) {
							tools.showToast("图片保存成功")
						} else {
							tools.ShowMsg('保存已保存成功！您可以用该图片去分享朋友圈哦')
						}
					}
				})
			}
		})
	},
  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {
		let that = this
		let title = ""
		let imageUrl = ""
		if (that.data.pageShare.ADTitle && that.data.pageShare.ADImg.length) {
			title = that.data.pageShare.ADTitle
			imageUrl = that.data.pageShare.ADImg[0].url
		}
		return {
			title: title,
			path: '/pages/index/index?isIndex1=' + that.data.isIndex1,
			imageUrl: imageUrl,
			success: function (res) {
				tools.showToast("转发成功")
			}
		}
	},
	//领取优惠券
	getCoupon: function (e) {
		if (!app.globalData.userInfo) {
			wx.showModal({
				title: '提示',
				content: '请先登录！',
			})
			return;
		}
		console.log(e)
		var ds = e.currentTarget.dataset;
		var id = ds.id;
		var that = this;
		tools.GetCoupon({
			appId: app.globalData.appid,
			couponId: id,
			userId: app.globalData.userInfo.UserId
		})
			.then(function (res) {
				wx.showModal({
					title: '提示',
					content: res.msg,
				})
				console.log(res);
				if (res.isok) {
					//that.reLoad();
				}
			});
	},
  /**
*用户点击搜索框
*/
	goodsSearch: function (e) {
		console.log("用户点击了搜索框")
		wx.navigateTo({
			// url:'../orderList/group2_orderList'
			url: '../goodsSearch/goodsSearch'
		})

	},
	//跳转我的优惠券
	goMyCoupons: function () {
		tools.goNewPage("../me/mycoupon")
	},
	//关闭联系店主的商品详情框
	closeDetail: function (e) {
		console.log("关闭联系店主的商品详情框")
		var that = this;
		that.data.status.kipperMark = true;
		console.log(that.data.status.kipperMark)
		that.onLoad();
	},

})