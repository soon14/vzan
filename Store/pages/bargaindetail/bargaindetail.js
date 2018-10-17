// pages/bargaindetail/bargaindetail.js
var addr = require("../../utils/addr.js");
var tool = require('../../template/template.js');
var util = require("../../utils/util.js");
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		clientTel: 0,//手机号码
		username: '',//用作显示自砍圆圈名字
		avatarUrl: '',//用作显示自砍圆圈头像
		BargainedUserName: '',//帮砍人名字
		qrcode: '',//点击帮砍生成的带参二维码
		buid: 0,//砍价商品领取记录Id
		buid1: 0,//帮砍砍价商品领取记录Id
		isCut: 0,//是否从其他页面直接点击砍价 0否 1真
		nowTime: 0,//现实时间的实时毫秒数
		cutprice: 0,//砍价完成的随机价格
		scrollTop: 0,//页面元素滚动高度
		singleprice: 0,//进入页面时当前价格
		obj: [],//返回obj
		DescImgList: [],//obj级下
		ImgList: [],//obj级下
		Id: 0,//商品id
		isFriend: 0,//判断是否带参，(好友) 0是自砍 1是好友
		condition0: false, //帮好友砍价成功弹窗
		condition1: false,//自砍成功弹窗
		condition2: false,//邀请朋友砍刀弹窗
		condition0_1: 0,//帮砍开始砍价按钮隐藏
		condition0_2: 0,//自砍邀请好友按钮
		choose: 0,
		isTimeto: 1,//自砍倒计时
		imgUrls: [ // 砍价描述图片
			'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
			'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
			'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg'
		],
		item: [ //砍价排行返回的内容
			{ userimg: '/image/test.png', username: '吴亦凡', condition: '砍至最低', payed: false },
			{ userimg: '/image/test.png', username: '吴亦凡', condition: '32.56', payed: true },
			{ userimg: '/image/test.png', username: '吴亦凡', condition: '8.36', payed: true },
			{ userimg: '/image/test.png', username: '吴亦凡', condition: '8.36', payed: false },
		]
	},
	getPhoneNumber: function (e) {
		var that = this
		console.log(e.detail.errMsg)
		console.log(e.detail.iv)
		console.log(e.detail.encryptedData)
		app.globalData.telEncryptedData = e.detail.encryptedData
		app.globalData.telIv = e.detail.iv
		app.globalData.isgetTel = 1
		app.getPhoneNumber(function (res) {
			if (res.TelePhone != '未绑定') {
				that.setData({ clientTel: res.TelePhone })
			}
		})
	},
	// 点击查看大图
	previewImageAPI: function (e) {
		var imgArray = this.data.DescImgList
		for (var i = 0; i < imgArray.length; i++) {
			var imageArray = []  //声明data数据
		}
		var index = e.currentTarget.id     //声明对应图片的id
	},
	// 拨打电话
	makePhonecall: function () {
		wx.makePhoneCall({
			phoneNumber: app.globalData.customerphone,
		})
	},
	// 砍价描述 砍价排行选择km
	changeChoose: function (e) {
		var index = e.currentTarget.id
		this.setData({ choose: index })
	},
	// 帮好友砍价
	beginBargain: function () {
		var that = this
		if (that.data.isFriend == 1) {
			var buid = that.data.buid1
		} else {
			var buid = that.data.buid
		}
		that.inite2(buid, 1)
		// this.setData({ condition0: !this.data.condition0 })
	},
	// 自砍 !
	bargainMyself: function () {
		var that = this
		if (that.data.haveCreatOrder == true) {
			wx.showModal({
				title: '提示',
				content: '您已经下过单了，请进入砍价单查看详情！',
				confirmText: '去看看',
				showCancel: false,
				success: function (res) {
					if (res.confirm) {
						tool.goNewPage('../mycutprice/mycutprice')
					}
				}
			})
		} else {
			that.inite1(that.data.Id, app.globalData.userInfo.nickName, 0)
		}
	},
	// 我也要玩
	bargainMyself0: function () {
		var that = this
		// that.inite1(that.data.Id, app.globalData.userInfo.nickName)
		this.setData({ isFriend: 0 })
	},
	// 帮好友砍价取消砍价叉叉
	cancelBargain: function () {
		this.setData({ condition0: !this.data.condition0, condition0_1: 1 })
	},
	// 自砍取消叉叉
	hiddenCondition0: function () {
		this.setData({ condition1: !this.data.condition1 })
	},
	// 不帮好友砍价点击 “我也要玩” 按钮 
	changeCondition_1: function () {
		this.setData({ condition0: !this.data.condition0 })
	},
	// 自砍点击 “请好友帮看一刀” 按钮
	changeCondition_2: function () {
		this.setData({ condition1: !this.data.condition1, condition0_2: 1 })
	},
	//!
	inviteBargain1: function (e) {
		var index = e.currentTarget.id
		var that = this
		that.GetShareCutPrice(index, 1)
	},
	//!
	inviteBargain: function (e) {
		var index = e.currentTarget.id
		var that = this
		if (that.data.haveCreatOrder == true) {
			wx.showModal({
				title: '提示',
				content: '您已经下过单了，请进入砍价单查看详情！',
				confirmText: '去看看',
				showCancel: false,
				success: function (res) {
					if (res.confirm) {
						tool.goNewPage('../mycutprice/mycutprice')
					}
				}
			})
		} else {
			that.GetShareCutPrice(index, 0)
		}
	},
	// 取消好友帮砍弹窗叉叉
	cancelMyselfBargain: function () {
		this.setData({ condition2: !this.data.condition2, })
	},
	// 自砍进入倒计时的按钮提示
	bargainTimeload: function () {
		wx.showModal({
			title: '提示',
			content: '莫慌，自砍倒计时中！',
			showCancel: false,
		})
	},
	// 活动已结束
	bargainTimeout: function (e) {
		wx.showModal({
			title: '提示',
			content: '活动已结束！',
			showCancel: false,
		})
	},
	// 商品已售罄
	bargainSoldout: function () {
		wx.showModal({
			title: '提示',
			content: '亲，该商品已售罄！',
			showCancel: false,
		})
	},
	// 活动未开始
	bargainTimeunout: function () {
		wx.showModal({
			title: '提示',
			content: '莫慌，活动未开始！',
			showCancel: false,
		})
	},
	// 跳转到砍价列表
	navtoMycutprice: function () {
		wx.navigateTo({
			url: '../mycutprice/mycutprice',
		})
	},
	// 返回主页
	siwchtoIndex: function () {
		wx.switchTab({
			url: '../index/index',
		})
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		// 判断是否从带参二维码进入
		var sceneArray = decodeURIComponent(options.scene)
		if (sceneArray != undefined) {
			var sceneArray = sceneArray.split("_")
			that.setData({ buid1: sceneArray[0], isFriend: sceneArray[2] })
		}

		if (options.Id == undefined) {
			var Id = sceneArray[1]
			var isCut = 0 //好友砍
		} else {
			var Id = options.Id
			var isCut = 1 //自砍
		}
		that.setData({ isCut: isCut })
		if (app.globalData.userInfo.openId == undefined || app.globalData.userInfo.openId == "") {
			app.getUserInfo(function (e) {
				that.inite(Id)
				that.GetStoreConfig()
				that.setData({ Id: Id, username: app.globalData.userInfo.nickName, avatarUrl: app.globalData.userInfo.avatarUrl, clientTel: app.globalData.userInfo.TelePhone })
			})
		} else {
			that.inite(Id)
			that.GetStoreConfig()
			that.setData({ Id: Id, username: app.globalData.userInfo.nickName, avatarUrl: app.globalData.userInfo.avatarUrl, clientTel: app.globalData.userInfo.TelePhone })
		}
	},
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
	onReady: function () {
		var that = this
		setInterval(function () { // 倒计时
			that.setData({
				nowTime: new Date().getTime()
			})
		}, 1000)
	},
	// 保存画布的图片
	canvasToTempFilePath: function (e) {
		wx.canvasToTempFilePath({
			x: 0,
			y: 0,
			width: app.globalData.windowWidth * 0.9,
			height: app.globalData.windowHeight * 0.75,
			destWidth: 650,
			destHeight: 880,
			canvasId: 'firstCanvas',
			success: function (res) {
				// console.log(res.tempFilePath)
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

  /**
   * 生命周期函数--监听页面卸载
   */
	onUnload: function () {

	},

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
	onPullDownRefresh: function () {
		this.data.obj = []
		this.inite(this.data.Id)
		wx.showToast({
			title: '刷新成功',
			icon: 'success'
		})
		wx.stopPullDownRefresh()
	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {

	},
	onPageScroll: function (e) {
		var that = this
		var scrollTop = e.scrollTop
		that.setData({ scrollTop: scrollTop })
	},
  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {
		return {
			title: '最低' + this.data.obj.FloorPriceStr + '元，原价' + this.data.obj.OriginalPriceStr + '元，等你来砍，能砍多少看你本事了',
			path: '/pages/bargaindetail/bargaindetail?scene=' + this.data.buid + '_' + this.data.Id + '_' + 1
		}
	},
	// 砍价详情
	inite: function (Id) {
		var that = this
		wx.request({
			url: addr.Address.GetBargain, //仅为示例，并非真实的接口地址
			data: {
				appid: app.globalData.appid,
				Id: Id,
				UserId: app.globalData.userInfo.UserId
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				var t = new Date()//  获取系统时间2017-10-19 14:37:00
				var nowtime = t.getFullYear() + "-" + (t.getMonth() + 1) + "-" + t.getDate() + " " + t.getHours() + ":" + t.getMinutes() + ":" + t.getSeconds()
				var obj = res.data.obj
				var singleprice = 0
				var BargainUserList = res.data.obj.BargainUserList
				var BargainRecordUserList = res.data.obj.BargainRecordUserList
				var findBuid = BargainRecordUserList.find(b => b.BargainUserId = app.globalData.userInfo.UserId)
				if (findBuid) {
					var buid = findBuid.BUId
				} else {
					var buid = 0
				}
				for (var j = 0; j < BargainRecordUserList.length; j++) {
					if (BargainRecordUserList[j].BargainUserId == app.globalData.userInfo.UserId) {
						var buid = BargainRecordUserList[j].BUId
						break
					}
				}
				for (var i = 0; i < BargainUserList.length; i++) {
					if (BargainUserList[i].UserId == app.globalData.userInfo.UserId && BargainUserList[i].Id == buid) {
						singleprice = BargainUserList[i].CurrentPriceStr
						break
					}
				}
				obj.EndDate = that.ChangeDateFormat(obj.EndDate)
				obj.StartDate = that.ChangeDateFormat(obj.StartDate)
				that.data.haveCreatOrder = res.data.haveCreatOrder
				if (that.data.isCut == 0) {
					that.beginBargain()
				}
				var percent = ((Number(obj.OriginalPriceStr) - singleprice) / Number(obj.OriginalPriceStr)) * 100;
				if (percent > 100)
					percent = 100;
				// 保存数据
				that.setData({
					buid: buid,
					obj: obj,
					DescImgList: res.data.obj.DescImgList,
					ImgList: res.data.obj.ImgList,
					BargainUserList: BargainUserList,
					singleprice: singleprice,
					percent: percent,
				})
				console.log(res)
			},
			fail: function () {
				console.log("获取信息出错")
				wx.showToast({
					title: '获取信息出错',
				})
			}
		})
	},
	// 申请砍价
	inite1: function (Id, UserName, isbuy) {
		var that = this
		wx.request({
			url: addr.Address.AddBargainUser, //仅为示例，并非真实的接口地址
			data: {
				Id: Id,
				UserId: app.globalData.userInfo.UserId,
				UserName: UserName
			},
			method: "POST",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == true) {
					if (isbuy == 0) { //自砍
						var buid = res.data.buid
						that.setData({ buid: res.data.buid })
						that.inite2(buid, 0)
					}
					if (isbuy == 1) {
						var buid = res.data.buid
						that.inite4(buid)
					}
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
	// 开始砍价
	inite2: function (buid, isFriend) {
		var that = this
		var condition0 = that.data.condition0
		wx.request({
			url: addr.Address.cutprice, //仅为示例，并非真实的接口地址
			data: {
				UserId: app.globalData.userInfo.UserId,
				buid: buid
			},
			method: "POST",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (isFriend == 1) {
					condition0 = true
				} else {
					condition0 = false
				}

				if (res.data.msg == '请求成功') {
					var record = res.data.record
					that.setData({ condition1: !that.data.condition1, cutprice: res.data.cutprice, isCut: 1, condition0: condition0, BargainedUserName: res.data.BargainedUserName })
					that.inite(that.data.Id)
				} else if (res.data.msg == '您已经帮他砍过了，刷新试试') {
					wx.showToast({
						title: '亲，已经砍过啦',
						icon: 'loading'
					})
					that.setData({ condition0_1: 1, })
				} else if (res.data.msg == '已砍至底价！') {
					if (that.data.isFriend == 1) {
						wx.showModal({
							title: '提示',
							content: '您朋友的商品已经砍至最低价！',
							showCancel: false,
							success: function (res) {
								if (res.confirm) {
									that.setData({ condition0_1: 1 })
								}
							}
						})
					} else {
						wx.showToast({
							title: '已砍至最低价',
							icon: 'loading'
						})
					}
				} else if (res.data.msg == '已下单不能再砍') {
					if (that.data.isFriend == 1) {
						wx.showModal({
							title: '提示',
							content: '您的朋友已下单了，不能帮砍了哦',
							showCancel: false,
							success: function (res) {
								if (res.confirm) {
									that.setData({ condition0_1: 1 })
								}
							}
						})
					} else {
						wx.showToast({
							title: '已下单不能再砍',
							icon: 'loading'
						})
					}
				} else if (res.data.msg == '已帮他砍过了') {
					wx.showToast({
						title: '您已帮他砍过了',
						icon: 'loading'
					})
					that.setData({ condition0_1: 1, })
				}
				else if (res.data.msg == '砍价状态有误！') {
					wx.showToast({
						title: '砍价状态有误',
						icon: 'loading'
					})
				} else if (res.data.msg == '商品已售罄！') {
					wx.showModal({
						title: '提示',
						content: '亲，该商品已售罄！请点击确认返回首页',
						showCancel: false,
						confirmText: '确认',
						success: function (res) {
							if (res.confirm) {
								wx.switchTab({
									url: '../index/index',
								})
							}
						}
					})
				} else {
					var timeArray = []
					if (res.data.obj == 0) {
						var content = '已自砍成功,自砍倒计时1分钟！'
					} else {
						timeArray = JSON.stringify(res.data.obj).split(".")
						var mintues = parseInt(parseInt(timeArray[1]) * 0.6)
						if (timeArray.length == 2 && parseInt(timeArray[0]) != 0) {
							var content = '已自砍成功,' + timeArray[0] + '小时' + mintues + '分钟' + '之后才能继续自砍'
						} else if (timeArray.length == 2 && parseInt(timeArray[0]) == 0) {
							var content = '已自砍成功,' + mintues + '分钟' + '之后才能继续自砍'
						} else {
							var content = '已自砍成功,' + timeArray[0] + '小时' + '之后才能继续自砍'
						}
					}
					wx.showModal({
						title: '提示',
						content: content,
						showCancel: false,
						success: function (res) {
							if (res.confirm) {
								that.setData({ isFriend: 0 })
							}
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
			// console.log(dd)
			return dd;
		}
		return "";
	},

	// 请求二维码
	GetShareCutPrice: function (bId, isCondition) {
		var that = this
		wx.request({
			url: addr.Address.GetShareCutPrice,
			data: {
				UserId: app.globalData.userInfo.UserId,
				buid: that.data.buid,
				appId: app.globalData.appid,
				bId: bId,
			},
			method: "POST",
			header: {
				'content-type': 'application/json'
			},
			success: function (res) {
				var isCut = that.data.isCut
				if (res.data.isok) {
					if (isCondition == 1) {
						that.setData({ condition2: !that.data.condition2, condition1: !that.data.condition1, qrcode: res.data.qrcode })
					} else {
						that.setData({ condition2: !that.data.condition2, qrcode: res.data.qrcode })
					}

					wx.downloadFile({
						url: res.data.qrcode.replace(/http/, "https"), //下载二维码图片
						success: function (res0) {
							// 下载大图
							wx.downloadFile({
								url: that.data.obj.ImgUrl_thumb.replace(/http/, "https"), //下载砍价商品大图
								success: function (res) {
									var windowWidth = app.globalData.windowWidth //屏幕宽度
									var windowHeight = app.globalData.windowHeight //屏幕高度
									var context = wx.createCanvasContext('firstCanvas')
									var bgImg = '/image/a38.png'
									var objImgUrl = res.tempFilePath
									var code = res0.tempFilePath //先下载二维码 返回的tempFilePath用作canvas拼图（只能本地图片）
									var bottomprice = '最低' + that.data.obj.FloorPriceStr + '元，原价' + that.data.obj.OriginalPriceStr + '元，等你来砍'
									var bottomprice1 = '能砍多少看你本事了'
									context.drawImage(bgImg, 0, 0, windowWidth * 0.9, windowHeight * 0.75); //大背景图
									context.drawImage(objImgUrl, windowWidth * 0.1, windowHeight * 0.1, windowWidth * 0.70, windowHeight * 0.19); //商品大图
									context.drawImage(code, windowWidth * 0.18, windowHeight * 0.45, windowWidth * 0.25, windowHeight * 0.17); //二维码
									context.setFontSize(13)
									context.setFillStyle('#fbb47b')
									context.fillText(bottomprice, windowWidth * 0.17, windowHeight * 0.42) //第一行文字
									context.fillText(bottomprice1, windowWidth * 0.29, windowHeight * 0.45) //第二行文字
									context.draw()
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
	//获取店铺配置
	GetStoreConfig: function () {
		var that = this
		wx.request({
			url: addr.Address.GetStoreConfig,
			data: {
				appId: app.globalData.appid
			},
			method: "GET",
			header: {
				'content-type': 'application/json'
			},
			success: function (res) {
				var isCut = that.data.isCut
				if (res.data.isok == 1) {
					var phone = res.data.postdata.store.TelePhone
					app.globalData.customerphone = phone
				}
			},
			fail: function () {
				console.log("获取电话出错")
				wx.showToast({
					title: '获取电话出错',
				})
			}
		})
	},
	// 现价购买按钮
	buybynowPrice: function (e) {
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		console.log('我是formid', formId)
		var index = e.currentTarget.id
		that.inite1(index, app.globalData.userInfo.nickName, 1)
	},

	//现价购买
	inite4: function (buid) {
		var that = this
		var vipwindow = this.data.vipwindow
		wx.request({
			url: addr.Address.GetBargainUser,
			data: {
				buid: buid,
				userid: app.globalData.userInfo.UserId
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok) {
					wx.navigateTo({
						url: '../orderListCutprice/orderListCutprice?BName=' + res.data.obj.BName + '&Freight=' + res.data.obj.Freight + '&ImgUrl=' + res.data.obj.ImgUrl + '&curPrcie=' + res.data.obj.curPrcie + '&buid=' + buid,
					})
				} else {
					wx.showModal({
						title: '提示',
						content: '您已经下过单了，请进入砍价单查看详情！',
						confirmText: '去看看',
						showCancel: false,
						success: function (res) {
							if (res.confirm) {
								wx.navigateTo({
									url: '../mycutprice/mycutprice',
								})
							}
						}
					})
				}
			},
			fail: function (e) {

				console.log('一键分享获取失败')
			}
		})
	},
})