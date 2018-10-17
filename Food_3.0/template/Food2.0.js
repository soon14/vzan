var addr = require("../utils/addr.js");
var template = require("../template/template.js");
var util = require("../utils/util.js");
//var inde = require("../pages/index/index.js");
var app = getApp()

function action(that, content) {
	that.setData({ animationData: {} })
	var duration = content.length / 3 * 1000
	var animation = wx.createAnimation({
		duration: duration,
		timingFunction: 'linear',
	})
	that.animation = animation
	var tX = content.length * 13
	animation.translateX(-tX, 0).step()
	animation.translateX(220, 0).step({ duration: 0 })
	that.setData({
		animationData: animation.export()
	})
}

function openLocation(latitude, longitude, scale) {
	wx.openLocation({
		latitude: latitude,
		longitude: longitude,
		scale: scale
	})
}

function previewStoreimg(e, imgList) {
	var index = e.currentTarget.id
	var imageArray = []
	for (var i = 0; i < imgList.length; i++) {
		imageArray.push(imgList[i])
	}
	var previewImage = imageArray[index]
	console.log(previewImage)
	wx.previewImage({
		current: previewImage,
		urls: imageArray
	})
}


function canvasToTempFilePath(index) {
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
					if (index == 0) {
						wx.showToast({
							title: '图片保存成功',
						})
					}
					if (index == 1) {
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
}

function GetFoodsDetail(that, isHome, cb) { //0为home.js调用，1为index.js调用
	wx.request({
		url: addr.Address.GetFoodsDetail,
		data: {
			AppId: app.globalData.appid,
			openId: app.globalData.userInfo.openId
		},
		method: "GET",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			if (res.data.isok == true) {
				//判断首页开关 *特殊 外卖与堂食开关
				that.setData({
					jingdu: res.data.data.food.Lng,

				})
				if (res.data.data.food.funJoinModel.takeOut) {
					var TakeOut = true
				} else {
					var TakeOut = false
				}
				if (res.data.data.food.funJoinModel.theShop) {
					var TheShop = true
				} else {
					var TheShop = false
				}
				if (res.data.data.food.funJoinModel.saveMoney == true && res.data.data.food.funJoinModel.canSaveMoneyFunction == true) {
					var savemoney = true
				} else {
					var savemoney = false
				}
				if (res.data.data.reserveId == 0 && isHome == 0) {
					resetappoint() //如果预约id是0，则重置预约信息
				}
				app.globalData.storeConfig = res.data.data.food //店铺配置对象
				app.globalData.appoint_Id = res.data.data.reserveId
				app.globalData.storeAddress.storeaddress = res.data.data.food.Address
				app.globalData.storeAddress.storeLat = res.data.data.food.Lat
				app.globalData.storeAddress.storeLng = res.data.data.food.Lng
				app.globalData.aid = res.data.data.food.appId
				app.globalData.storeId = res.data.data.food.Id
				app.globalData.canSaveMoneyFunction = res.data.data.food.funJoinModel.canSaveMoneyFunction
				app.globalData.TelePhone = res.data.data.food.TelePhone
				app.globalData.shareConfig = res.data.data.food.funJoinModel.shareConfig
				app.globalData.OutSideStr = res.data.data.food.OutSideStr
				app.globalData.TheShop = res.data.data.food.TheShop
				app.globalData.TakeOut = res.data.data.food.TakeOut
				app.globalData.logoimg = res.data.data.Logo
				app.globalData.FoodsName = res.data.data.food.FoodsName
				app.globalData.jingdu = res.data.data.food.Lng
				app.globalData.address = res.data.data.food.Address
				app.globalData.DistributionWay = res.data.data.food.DistributionWay
				app.globalData.reservationSwitch = res.data.data.food.funJoinModel.reservationSwitch
				if (res.data.data.food.DistributionWay == 1) {
					app.globalData.ShippingFeeStr = res.data.data.food.ShippingFeeStr
				} else {
					GetDadaFreight(that, wx.getStorageSync('weidu'), wx.getStorageSync('jingdu'), wx.getStorageSync('addr'))
				}
				if (isHome == 0) {
					that.data.typeIcon[0].condition = TheShop
					that.data.typeIcon[1].condition = TakeOut
					that.data.typeIcon[2].condition = res.data.data.food.funJoinModel.vipCard
					that.data.typeIcon[3].condition = savemoney
					that.data.typeIcon[4].condition = res.data.data.food.funJoinModel.theShard
					that.data.typeIcon[5].condition = res.data.data.food.funJoinModel.sortQueueShowSwitch
					that.data.typeIcon[6].condition = res.data.data.food.funJoinModel.reservationShowSwitch
					var entryCount = 0
					for (let m = 0; m < that.data.typeIcon.length; m++) {
						if (that.data.typeIcon[m].condition) {
							entryCount++
						}
					}
					that.setData({
						entryCount: entryCount,
						food: res.data.data.food,
						sliderImgs: res.data.data.sliderImgUrls,
						storeImgs: res.data.data.storeImgs,
						typeIcon: that.data.typeIcon
					})
					cb('ok0')
				}
				if (isHome == 1 || isHome == 6) {
					var food = res.data.data.food

					var t = food.getOpenTimeList
					var nowtime = new Date('2018/06/12 ' + new Date().getHours() + ':' + new Date().getMinutes() + ':00').getTime()

					for (let l = 0; l < t.length; l++) {
						var st = new Date('2018/06/12 ' + t[l].StartTime + ':00').getTime()
						var et = new Date('2018/06/12 ' + t[l].EndTime + ':00').getTime()
						if (st > et) {
							var et = new Date('2018/06/13 ' + t[l].EndTime + ':00').getTime()
						}

						if (st < nowtime && nowtime < et) {
							t[l].isopen = true
						} else {
							t[l].isopen = false
						}
					}

					if (food.openState == 1) {
						//判断堂食
						if (food.TheShop == 1 && that.data.TablesNo != -999) {
							if (t[2].isopen) {
								that.data.openState.state = 1
								that.data.openState.msg = '营业中'
							} else {
								that.data.openState.state = 0
								that.data.openState.msg = '休息中'
							}
						}
						//判断外卖
						if (food.TakeOut == 1 && that.data.TablesNo == -999) {
							if (t[1].isopen == true) {
								if (wx.getStorageSync('msg') == '配送范围内') {
									that.data.openState.state = 1
									that.data.openState.msg = '营业中'
								} else {
									that.data.openState.state = 0
									that.data.openState.msg = '配送范围外'
								}
							} else {
								that.data.openState.state = 0
								that.data.openState.msg = '休息中'
							}
						}
					}

					else {
						that.data.openState.state = 0
						that.data.openState.msg = '商家休息中'
					}

					that.setData({
						openState: that.data.openState,
						food: res.data.data.food,
						chooseLocal: that.data.chooseLocal,
					})
				}
				if (isHome == 2) {
					that.setData({ canSaveMoneyFunction: res.data.data.food.funJoinModel.canSaveMoneyFunction })
				}
				if (isHome == 3) {
					that.setData({ data: res.data.data })
				}
			}
		},
		fail: function () {
			console.log("获取店铺装修信息失败")
		}
	})

}

function GetDadaFreight(that, lng, lat, address) {
	var _that = that
	wx.request({
		url: addr.Address.GetDadaFreight,
		data: {
			cityname: app.globalData.cityname,
			appid: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			lat: lng,
			lnt: lat,
			acceptername: "用户",
			accepterphone: "158xxxxxxxx",
			address: address,
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded' // 默认值

		},
		success: function (res) {
			if (res.data.isok == 1) {
				app.globalData.ShippingFeeStr = res.data.dataObj.deliverFee
				app.globalData.distributionprice = res.data.dataObj.deliverFeeInt
				_that.setData({
					ShippingFeeStr: res.data.dataObj.deliverFee,
					distributionprice: res.data.dataObj.deliverFeeInt,
				});
			}
		},
		fail: function () {
			console.log("获取达达配送费失败")
		}
	})
}

function updateMiniappGoodsOrderState(orderId, State, cb) {
	var that = this
	wx.request({
		url: addr.Address.updateMiniappGoodsOrderState,
		data: {
			AppId: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			orderId: orderId,
			State: State
		},
		method: "POST",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			if (res.data.isok == 1) {
				cb('更新成功')
			} else {
				cb(res.data.msg)
			}
		},
		fail: function () {
			console.log("更新订单状态出错")
			cb('更新失败')
		}
	})
}

function getMiniappGoodsOrderById(that, orderId) {
	wx.request({
		url: addr.Address.getMiniappGoodsOrderById,
		data: {
			AppId: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			orderId: orderId
		},
		method: "GET",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			if (res.data.isok == 1) {
				var yuanjia = 0
				for (var i = 0; i < res.data.data.goodOrderDtl.length; i++) {
					yuanjia += res.data.data.goodOrderDtl[i].orderDtl.originalPrice * res.data.data.goodOrderDtl[i].orderDtl.Count
				}
				res.data.data.buyPrice = Number(res.data.data.buyPrice).toFixed(2)
				var allgoodprice = (Number(res.data.data.buyPrice) - Number(res.data.data.goodsOrder.PcakinPriceStr)).toFixed(2)
				var youhui = Number(yuanjia / 100) + Number(res.data.data.freightPrice) - Number(allgoodprice)
				that.setData({
					youhui: (youhui).toFixed(2),
					yuanjia: Number(yuanjia / 100).toFixed(2),
					postdata: res.data.data,
					goodOrder: res.data.data.goodsOrder,
					goodOrderDtl: res.data.data.goodOrderDtl
				})
			}
		},
		fail: function () {
			console.log("查询订单详情失败")
		}
	})
}


function getMiniappGoodsOrder(that, pageIndex, isFirst, isRefresh, isonShow) {
	var postdata = that.data.postdata
	if (isRefresh == 2) {
		pageIndex = 1
		that.data.pageIndex1 = 2
	} else {
		pageIndex = that.data.pageIndex
	}
	wx.request({
		url: addr.Address.getMiniappGoodsOrder,
		data: {
			AppId: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			State: 10,
			pageIndex: pageIndex,
			pageSize: 5
		},
		method: "GET",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			if (res.data.isok == 1) {
				for (var i = 0; i < res.data.postdata.length; i++) {
					for (var j = 0; j < res.data.postdata[i].orderList.length; j++) {
						res.data.postdata[i].orderList[j].BuyPrice = Number(res.data.postdata[i].orderList[j].BuyPrice).toFixed(2)
					}
				}
				if (isRefresh == 2 && isonShow == 0) {
					wx.showToast({
						title: '刷新成功',
					})
				}
				if (isFirst == 1) {
					if (res.data.postdata != '') {
						if (res.data.postdata[0].year == postdata[0].year) {
							postdata[0].orderList = (postdata[0].orderList).concat(res.data.postdata[0].orderList)
							that.data.pageIndex1++
						}
					} else {
						return
					}
				} else {
					postdata = res.data.postdata
				}
				that.setData({
					postdata: postdata,
					pageIndex: that.data.pageIndex1
				})
				wx.hideLoading()
			}
		},
		fail: function () {
			console.log("获取订单列表出错")
		}
	})
}

function GetMyAddress(that) {
	wx.request({
		url: addr.Address.GetMyAddress,
		data: {
			AppId: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			isDefault: 0,
			addressId: '',
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				that.setData({
					addressList: res.data.postdata.addressList
				})
			}
		},
		fail: function () {
			console.log("获取地址列表出错")
		}
	})
}

function GetGoodsTypeList(that) {
	wx.request({
		url: addr.Address.GetGoodsTypeList,
		data: {
			AppId: app.globalData.appid,
			pagesize: 100,
			pageindex: 1,
		},
		method: "GET",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			if (res.data.isok == 1) {
				that.setData({
					goodsTypeList: res.data.postdata.goodsTypeList,
				})
				wx.hideLoading()
			}
		},
		fail: function () {
			console.log("获取菜类分类出错")
		}
	})
}

function GetGoodsList(that, typeid, goodsName, shopType, pageindex, isReachBottom, goodtype) {
	wx.request({
		url: addr.Address.GetGoodsList,
		data: {
			AppId: app.globalData.appid,
			typeid: typeid,
			goodsName: goodsName,
			shopType: shopType,
			pageindex: pageindex,
			pagesize: 6,
			levelid: app.globalData.levelid,
			goodtype: goodtype || ''
		},
		method: "GET",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			if (res.data.isok == 1) {
				for (var i = 0; i < that.data.shopcartList.length; i++) {
					var findgoodid = res.data.postdata.goodslist.find(f => f.good.Id == that.data.shopcartList[i].goodid)
					if (findgoodid) {
						findgoodid.good.carCount += that.data.shopcartList[i].buynums
					}
				}


				if (isReachBottom == 0) {
					if (res.data.postdata.goodslist.length == 0) { var isall = true } else { var isall = false }
					that.setData({
						goodslist: res.data.postdata.goodslist,
						typeid: typeid,
						isall: isall
					})
					++that.data.pageindex
				} else {
					if (res.data.postdata.goodslist.length > 0) {
						that.data.goodslist = that.data.goodslist.concat(res.data.postdata.goodslist)
						that.setData({
							goodslist: that.data.goodslist,
							typeid: typeid
						})
						++that.data.pageindex
					} else {
						that.setData({ isall: true })
					}
				}
				wx.hideLoading()
			}
		},
		fail: function () {
			console.log("获取菜类分类出错")
		}
	})
}

// 定位
function chooseLocation(that) {
	console.log("进入food2")

	//判断用户之前是不是拒绝授权获取地理位置
	// wx.getSetting({
	//   success: (res) => {
	//     if (res.authSetting['scope.userLocation'] != undefined && res.authSetting['scope.userLocation'] != true) {
	//       wx.showModal({
	//         title: '是否授权当前位置',
	//         content: '需要获取您的地理位置，请确认授权，否则地图功能将无法使用',
	//         success: function (res) {
	//           if (res.cancel) {
	//             console.log("用户不进行授权");
	//           } else if (res.confirm) {
	//             wx.openSetting({
	//               success: function (data) {
	//                 if (data.authSetting["scope.userLocation"] == true) {
	//                   console.log("授权成功");
	//                 }
	//               }
	//             })

	//           }
	//         }
	//       })
	//     }
	//   }
	// })



	wx.chooseLocation({
		success: function (res) {
			wx.setStorageSync("addr", res.name)
			wx.setStorageSync("weidu", res.latitude)
			wx.setStorageSync("jingdu", res.longitude)
			app.globalData.weidu = res.latitude
			app.globalData.jingdu = res.longitude
			app.globalData.addressInfo = res.name
			app.globalData.address = res.address
			that.setData({
				addressInfo: res.name,
				chooseLocal: false,
				jingdu: res.longitude,
				weidu: res.latitude,
			})

			if (app.globalData.DistributionWay == 2) {
				var city = getApp().globalData.address.match(/省(\S*)市/);
				var cityname = city[1]
				that.setData({
					cityname: cityname
				})
			}
			GetDistanceForFood(that, res.latitude, res.longitude)
			//如果是达达配送就要请求运费

			if (app.globalData.DistributionWay == 2) {
				GetDadaFreight(that, res.latitude, res.longitude, res.address)
			}
		},
	})

}



// 查询配送距离
function GetDistanceForFood(that, lat, lng) {
	wx.request({
		url: addr.Address.GetDistanceForFood,
		data: {
			AppId: app.globalData.appid,
			lat: lat,
			lng: lng
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.msg == '配送范围内') {
				if (that.data.food.TakeOut == 1 && that.data.food.openState == 1 && that.data.food.getOpenTimeList[1].isopen) {
					that.data.openState.state = 1
					that.data.openState.msg = '营业中'
				} else {
					that.data.openState.state = 0
					that.data.openState.msg = '休息中'
				}
			} else {
				that.data.openState.msg = res.data.msg
			}
			that.setData({
				openState: that.data.openState
			})
			console.log(that.data.food.getOpenTimeList)
			wx.setStorageSync('msg', res.data.msg)
		},
	})
}

function countallprice(that) {//计算购物车的所有钱
	var shopcartList = that.data.shopcartList
	var allprice = 0
	var alldiscountprice = 0
	var shopcartlength = 0
	var dishwarefee = 0
	for (var i = 0; i < shopcartList.length; i++) {
		if (shopcartList[i].isPackin == 1) {
			dishwarefee += Number(getApp().globalData.storeConfig.PackinFeeStr) * Number(shopcartList[i].buynums)
		}
		allprice += Number(shopcartList[i].price) * Number(shopcartList[i].buynums)
		alldiscountprice += Number(shopcartList[i].discountprice) * Number(shopcartList[i].buynums)
		shopcartlength += shopcartList[i].buynums
	}
	that.setData({ allprice: (allprice).toFixed(2), alldiscountprice: (alldiscountprice).toFixed(2), shopcartlength: shopcartlength, dishwarefee: (dishwarefee).toFixed(2) })
}

function clearShopcartList(that) { //清空购物车
	if (that.data.shopcartList.length > 0) {
		wx.showModal({
			title: '提示',
			content: '是否清空购物车？',
			success: function (res) {
				if (res.confirm) {
					that.data.pageindex = 1
					GetGoodsList(that, 0, '', that.data.shopType, 1, 0)
					that.setData({
						shopcartList: [],
						allprice: 0,
						alldiscountprice: '0.00',
						typeid: 0,
						goodsName: '',
						shopcartlength: 0,
						dishwarefee: 0,
					})
				} else if (res.cancel) {
					console.log('用户点击取消')
				}
			}
		})
	} else {
		return
	}
}

function GetUserInSortQueuesPlanMsg(that) {
	wx.request({
		url: addr.Address.GetUserInSortQueuesPlanMsg,
		data: {
			appid: getApp().globalData.appid,
			aId: getApp().globalData.aid,
			storeId: getApp().globalData.storeId,
			userId: getApp().globalData.userInfo.UserId
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == true) {
				that.setData({ isonOrder: res.data.code == 0 ? false : true, dataObj: res.data.dataObj, numsindex: res.data.code > 0 ? res.data.dataObj.sortQueue.pCount : 0 })

			}
		},
		fail: function () {
			console.log("获取当前队列位置信息出错")
		}
	})
}

function PutSortQueueMsg(that, pCount, telePhone) {
	wx.request({
		url: addr.Address.PutSortQueueMsg,
		data: {
			appid: getApp().globalData.appid,
			aId: getApp().globalData.aid,
			storeId: getApp().globalData.storeId,
			userId: getApp().globalData.userInfo.UserId,
			pCount: pCount,
			telePhone: telePhone
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == true) {
				wx.showToast({ title: '取号成功！', })
				GetUserInSortQueuesPlanMsg(that)
			} else {
				wx.showModal({ title: '提示', content: res.data.Msg })
			}
		},
		fail: function () {
			console.log("申请取号出错")
		}
	})
}

function CancelSortQueue(that, sortId) {
	wx.request({
		url: addr.Address.CancelSortQueue,
		data: {
			appid: getApp().globalData.appid,
			aId: getApp().globalData.aid,
			storeId: getApp().globalData.storeId,
			sortId: sortId
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == true) {
				wx.showToast({ title: res.data.Msg })
				GetUserInSortQueuesPlanMsg(that)
			}
		},
		fail: function () {
			console.log("取消排队出错")
		}
	})
}

function appoint_addGoodsCarData(that, goodid, attrSpacStr, SpecInfo, GoodsNumber, cb) {
	wx.request({
		url: addr.Address.addGoodsCarData,
		data: {
			appId: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			goodid: goodid,
			attrSpacStr: attrSpacStr,
			SpecInfo: SpecInfo,
			GoodsNumber: GoodsNumber,
			newCartRecord: 1,
			isReservation: true
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == true) {
				cb(res.data.cartid)
			}
		},
		fail: function () {
			console.log("加入预约购物车出错")
		}
	})
}

function GetReserveMenu(that) {
	wx.request({
		url: addr.Address.GetReserveMenu,
		data: {
			appId: app.globalData.appid,
			openId: app.globalData.userInfo.openId,
			reserveId: app.globalData.appoint_Id
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == 1) {
				var alldiscountprice = 0
				if (res.data.data != null) {
					var orderInfo = res.data.data
					for (var i = 0; i < res.data.data.length; i++) {
						alldiscountprice += (res.data.data[i].Price * res.data.data[i].Count)
					}
					that.setData({ oderInfo: orderInfo, alldiscountprice: (alldiscountprice).toFixed(2) })
				} else {
					that.setData({ alldiscountprice: (alldiscountprice).toFixed(2) })
				}
			}
		},
		fail: function () {
			console.log("查询预约菜单出错")
		}
	})
}

function GetReservation(that) {
	wx.request({
		url: addr.Address.GetReservation,
		data: {
			appId: app.globalData.appid,
			openId: app.globalData.userInfo.openId,
			reserveId: app.globalData.appoint_Id
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == 1) {
				if (res.data.data.State == -5 || res.data.data.State == 5 || res.data.data.State == -1) {
					getApp().globalData.appoint_Id = 0
					resetappoint()
				}
				that.setData({ data: res.data.data })
			}
		},
		fail: function () {
			console.log("查询预约id出错")
		}
	})
}


function AddReservation(that, dinningTime, seats, userName, contact, note, orderjson, goodCarIdStr, buyMode, cb) {
	wx.request({
		url: addr.Address.AddReservation,
		data: {
			appId: getApp().globalData.appid,
			openId: getApp().globalData.userInfo.openId,
			dinningTime: dinningTime,
			seats: seats,
			userName: userName,
			contact: contact,
			note: note,
			orderjson: orderjson,
			goodCarIdStr: goodCarIdStr,
			isgroup: 0,
			goodtype: 0,
			buyMode: buyMode,
			isReservation: true
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			app.globalData.appoint_Id = res.data.reserveId
			if (res.data.isok == true) {
				cb(res.data)
			} else {
				wx.showModal({
					title: '提示',
					content: '亲爱的客户，由于您的储值余额不足，已自动帮您预约好餐桌，请准时到达店铺并进行扫码点餐。',
					showCancel: false,
					confirmText: '知道了',
					success: function (res) {
						setTimeout(function () {
							wx.redirectTo({ url: '../appointment/appointment_info' })
						}, 1000)
					},
				})
			}
		},
		fail: function () {
			console.log("加入预约购物车出错")
		}
	})
}

function CancelResevation(that) {
	wx.request({
		url: addr.Address.CancelResevation,
		data: {
			appId: getApp().globalData.appid,
			openId: getApp().globalData.userInfo.openId,
			reserveId: getApp().globalData.appoint_Id
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == 1) {
				getApp().globalData.appoint_Id = 0
				wx.showToast({ title: '取消成功', })
				setTimeout(function () {
					wx.navigateBack({ delta: 1 })
				}, 1000)
			} else {
				wx.showToast({ title: res.data.msg, icon: 'loading' })
			}
		},
		fail: function () {
			console.log("加入预约购物车出错")
		}
	})
}

function GetReserveMenuPay(that, cb) {
	wx.request({
		url: addr.Address.GetReserveMenuPay,
		data: {
			appId: getApp().globalData.appid,
			openId: getApp().globalData.userInfo.openId,
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == true) {
				if (res.data.data.isAccepet != undefined) {
					if (res.data.data.isAccepet == true) {
						cb(res.data.data.orderid)
					} else {
						cb(-1)
					}
				}
			}
		},
		fail: function () {
			console.log("加入预约购物车出错")
		}
	})
}

// 取消预约 后台退款 重置预约信息
function resetappoint() {
	getApp().globalData.appoint_goodslist = []
	getApp().globalData.appoint_shopcartlist = []
	getApp().globalData.appoint_alldiscountprice = 0
	getApp().globalData.appoint_paynow = 1
	getApp().globalData.appoint_shopcartlength = 0
	getApp().globalData.appointMsg = {
		datatime: '选择就餐日期',
		time: '选择就餐时间',
		numsindex: 0,
		msg: '',
		name: '',
		phonenumber: '',
	}
	console.log('重置预约信息')
}

function GetGroupDetail(that, groupid) {
	wx.request({
		url: addr.Address.GetGroupDetail,
		data: {
			appId: getApp().globalData.appid,
			groupid: groupid
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == 1) {
				// 拼团用户头像
				var userImg = []
				for (var i = 0; i < res.data.postdata.GroupUserList.length; i++) {
					userImg.push(res.data.postdata.GroupUserList[i].HeadImgUrl)
					var length = i
				}
				if ((length || 0) < res.data.postdata.GroupSize) {
					for (var j = 0; j < res.data.postdata.GroupSize - res.data.postdata.GroupUserList.length; j++) {
						userImg.push('')
					}
				}
				// end

				// 判断该用户有否参与过该团
				var finduserId = res.data.postdata.GroupUserList.find(f => f.Id == getApp().globalData.userInfo.UserId)
				if (finduserId) {
					var haveJoin = true
				} else {
					var haveJoin = false
				}

				for (var z = 0; z < res.data.GroupSponsorList.length; z++) {
					var ifjoined = res.data.GroupSponsorList[z].GroupUserList.find(f => f.Id == getApp().globalData.userInfo.UserId)
					if (ifjoined) {
						res.data.GroupSponsorList[z].haveJoin = true
					} else {
						res.data.GroupSponsorList[z].haveJoin = false
					}
				}
				// end

				// 参团详情页面控制按钮显示 
				if (res.data.postdata.GroupUserList.length == res.data.postdata.GroupSize) {
					res.data.postdata.groupstate = 2
				} else {
					res.data.postdata.groupstate = 1
				}
				// end

				res.data.postdata.StartDateStr = template.ChangeDateFormatNew(res.data.postdata.StartDate)
				res.data.postdata.EndDateStr = template.ChangeDateFormatNew(res.data.postdata.EndDate)

				if (res.data.postdata.GroupSize)
					that.setData({
						userImg: userImg,
						GroupSponsorList: res.data.GroupSponsorList,
						postdata: res.data.postdata,
						haveJoin: haveJoin,
						isSponsor: res.data.postdata.SponsorUserId == getApp().globalData.userInfo.UserId ? true : false
					})
			}
		},
		fail: function () {
			console.log("查询拼团详情出错")
		}
	})
}

function GetMyGroupList(that, state, pageIndex, isreachBottom) {
	wx.request({
		url: addr.Address.GetMyGroupList,
		data: {
			appId: getApp().globalData.appid,
			userId: getApp().globalData.userInfo.UserId,
			state: state,
			pageIndex: pageIndex
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == 1) {
				if (isreachBottom == 0) {
					if (res.data.postdata == null) { var isall = true } else { var isall = false }
					that.setData({
						postdata: (res.data.postdata == null ? [] : res.data.postdata),
						choosetype: state,
						isall: isall
					})
					that.data.pageindex = ++pageIndex
				} else {
					if (res.data.postdata != null) {
						that.data.postdata = that.data.postdata.concat(res.data.postdata)
						that.setData({
							postdata: that.data.postdata,
							choosetype: state,
						})
						that.data.pageindex = ++pageIndex
					} else {
						that.setData({ isall: true })
					}
				}
				wx.hideLoading()
			}
		},
		fail: function () {
			console.log("查询拼团列表出错")
		}
	})
}


function GetGoodsDtl(that, goodsid) {
	wx.request({
		url: addr.Address.GetGoodsDtl,
		data: {
			appId: getApp().globalData.appid,
			goodsid: goodsid,
			levelid: getApp().globalData.levelid
		},
		method: "GET",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == 1) {

				if (res.data.postdata.goodsAttrList.length == 0) {
					res.data.postdata.miniappFoodGoods.singleprice = res.data.postdata.miniappFoodGoods.PriceStr
				}
				res.data.postdata.miniappFoodGoods.nowbuynums = 1
				res.data.postdata.miniappFoodGoods.count = res.data.postdata.miniappFoodGoods.Stock

				// 如果有其他用户正在开团，剔除自己的团
				var GroupSponsorList = res.data.postdata.miniappFoodGoods.EntGroups.GroupSponsorList
				for (let z = 0; z < GroupSponsorList.length; z++) {
					var finduserId = GroupSponsorList[z].GroupUserList.find(f => f.Id == getApp().globalData.userInfo.UserId)
					if (finduserId) {
						GroupSponsorList.splice(z, 1)
						z--
					}
				}
				for (let x = 0; x < GroupSponsorList.length; x++) {
					GroupSponsorList[x].haveJoin = false
				}
				// end

				that.setData({
					GroupSponsorList: GroupSponsorList,
					goodsAttrList: res.data.postdata.goodsAttrList,
					miniappFoodGoods: res.data.postdata.miniappFoodGoods
				})
			}
		},
		fail: function () {
			console.log("查询团详情出错")
		}
	})
}

function GetTableNo(that, tableNo, cb) {
	wx.request({
		url: addr.Address.GetTableNo,
		data: {
			appid: getApp().globalData.appid,
			tableNo: tableNo,
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			cb(res)
		},
		fail: function () {
			console.log("获取桌台号错误")
		}
	})
}

module.exports = {
	// 微信接口
	openLocation: openLocation,
	previewStoreimg: previewStoreimg,
	canvasToTempFilePath: canvasToTempFilePath,

	action: action,
	chooseLocation: chooseLocation,
	// 餐饮2.0api
	countallprice: countallprice,
	clearShopcartList: clearShopcartList,
	resetappoint: resetappoint,

	GetFoodsDetail: GetFoodsDetail,
	updateMiniappGoodsOrderState: updateMiniappGoodsOrderState,
	getMiniappGoodsOrderById: getMiniappGoodsOrderById,
	getMiniappGoodsOrder: getMiniappGoodsOrder,
	GetMyAddress: GetMyAddress,
	GetGoodsTypeList: GetGoodsTypeList,
	GetGoodsList: GetGoodsList,
	GetDistanceForFood: GetDistanceForFood,
	GetUserInSortQueuesPlanMsg: GetUserInSortQueuesPlanMsg,
	PutSortQueueMsg: PutSortQueueMsg,
	CancelSortQueue: CancelSortQueue,
	appoint_addGoodsCarData: appoint_addGoodsCarData,
	GetReserveMenu: GetReserveMenu,
	GetReservation: GetReservation,
	AddReservation: AddReservation,
	CancelResevation: CancelResevation,
	GetReserveMenuPay: GetReserveMenuPay,
	GetGroupDetail: GetGroupDetail,
	GetMyGroupList: GetMyGroupList,
	GetGoodsDtl: GetGoodsDtl,
	GetTableNo: GetTableNo,
}