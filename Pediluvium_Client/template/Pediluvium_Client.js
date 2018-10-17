var addr = require("../utils/addr.js");
var template = require("../template/template.js");
var util = require("../utils/util.js");


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
		imageArray.push(imgList[i].filepath)
	}
	var previewImage = imageArray[index]
	console.log(previewImage)
	wx.previewImage({
		current: previewImage,
		urls: imageArray
	})

}

function GetStoreInfo(that, ischeckSaveMoney) { //传1就做储值开关判断，不传则没有动作
	wx.request({
		url: addr.Address.GetStoreInfo,
		data: {
			appid: getApp().globalData.appid,
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				if (res.data.LogoList.length != 0) {
					res.data.data.shopLogo = res.data.LogoList[0].filepath
				}
				res.data.data.photoList = res.data.photoList
				res.data.data.noontime = res.data.data.ShopTime.split(' - ')
				that.setData({ data: res.data.data })
				if (ischeckSaveMoney == 1) {
					that.setData({
						canSaveMoneyFunction: res.data.data.switchModel.canSaveMoneyFunction,
					})
				}
			}
		},
		fail: function (res) {
			console.log('请求店铺信息失败')
		}
	})
}

// isReachBootm 0否 1触底加载更多
function GetTechnicianList(that, pageIndex, pageSize, sex, age, count, isReachBootm) {
	wx.request({
		url: addr.Address.GetTechnicianList,
		data: {
			appid: getApp().globalData.appid, //必填
			pageIndex: pageIndex,
			pageSize: pageSize,
			sex: sex,	//0：全部，1：男，2：女（默认值：0）
			age: age,	//0：不排序， 1：升序， 2：降序（默认值：0）
			count: count,	//0：不排序， 1：升序， 2：降序（默认值：0）
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				var technicianList = res.data.technicianList
				for (var i = 0; i < technicianList.length; i++) {
					var state = technicianList[i].stateName
					if (state == '空闲') {
						technicianList[i].bgColor = '#59e988'
					} else if (state == '将下钟') {
						technicianList[i].bgColor = '#FC7D86'
					} else if (state == '上钟') {
						technicianList[i].bgColor = '#ffb266'
					} else {
						technicianList[i].bgColor = '#6daafd'
					}
					technicianList[i].photoList = technicianList[i].photo.split(',')
				}
				if (isReachBootm == 0) {
					that.setData({ technicianList: technicianList })
					that.data.pageIndex = 2
				} else { //触底加载更多
					if (res.data.technicianList.length > 0) {
						that.data.technicianList = that.data.technicianList.concat(technicianList)
						that.setData({ technicianList: that.data.technicianList })
						++that.data.pageIndex
					}
				}
			}
		},
		fail: function (res) {
			console.log('请求技师列表失败')
		}
	})
}

function GetTechnicianInfo(that, id) {
	wx.request({
		url: addr.Address.GetTechnicianInfo,
		data: {
			appid: getApp().globalData.appid, //必填
			id: id,
			uid: getApp().globalData.userInfo.UserId,
			levelid: getApp().globalData.levelid
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				res.data.info.photoList = res.data.info.photo.split(',')
				that.setData({ info: res.data.info, state: res.data.state })
			}
		},
		fail: function (res) {
			console.log('请求技师详情失败')
		}
	})
}


function GetServiceList(that, pageIndex, pageSize, type, price, count, isReachBootm) {
	wx.request({
		url: addr.Address.GetServiceList,
		data: {
			appid: getApp().globalData.appid, //必填
			pageIndex: pageIndex,
			pageSize: pageSize,
			type: type,	//0：全部（默认值：0）
			price: price,	//0：不排序， 1：升序， 2：降序（默认值：0）
			count: count,	//0：不排序， 1：升序， 2：降序（默认值：0）
			levelid: getApp().globalData.levelid
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				if (isReachBootm == 0) {
					var typeList_0 = { id: 0, aid: 2000, name: '全部', sort: 2, state: 1, type: 3, count: 0, storeId: 19, sel: false } //增加一个全部选项 id==0
					res.data.typeList.splice(0, 0, typeList_0)
					that.setData({ goodsList: res.data.goodsList, typeList: res.data.typeList })
					that.data.pageIndexS = 2
				} else {
					if (res.data.goodsList.length > 0) {
						that.data.goodsList = that.data.goodsList.concat(res.data.goodsList)
						that.setData({ goodsList: that.data.goodsList, typeList: res.data.typeList })
						++that.data.pageIndexS
					}
				}
			}
		},
		fail: function (res) {
			console.log('请求服务列表失败')
		}
	})
}

function GetServiceInfo(that, id) {
	wx.request({
		url: addr.Address.GetServiceInfo,
		data: {
			appid: getApp().globalData.appid,
			id: id,
			levelid: getApp().globalData.levelid
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				for (var i = 0; i < res.data.technicianList.length; i++) {
					res.data.technicianList[i].serviceCount = res.data.technicianList[i].baseCount //传回来字段不相同，为适应模版
					var state = res.data.technicianList[i].stateName
					if (state == '空闲') {
						res.data.technicianList[i].bgColor = '#59e988'
					} else if (state == '将下钟') {
						res.data.technicianList[i].bgColor = '#FC7D86'
					} else if (state == '上钟') {
						res.data.technicianList[i].bgColor = '#ffb266'
					} else {
						res.data.technicianList[i].bgColor = '#6daafd'
					}
				}
				that.setData({ serviceInfo: res.data.serviceInfo, technicianList: res.data.technicianList })
			}
		},
		fail: function (res) {
			console.log('请求服务详情失败')
		}
	})
}

function GetDateTable(that, tid, days) {
	wx.request({
		url: addr.Address.GetDateTable,
		data: {
			appid: getApp().globalData.appid,
			tid: tid,
			days: days //日期下标
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				that.setData({
					timeList: res.data.data
				})
			}
		},
		fail: function (res) {
			console.log('请求时间列失败')
		}
	})
}

function CancelPay(tid, servicetime, dbOrder, cb) {
	wx.request({
		url: addr.Address.CancelPay,
		data: {
			appid: getApp().globalData.appid,
			tid: tid,
			servicetime: servicetime,
			dbOrder: dbOrder
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				cb('更新成功')
			}
		},
		fail: function (res) {
			console.log('时间列解库失败')
		}
	})
}

function GetOrderRecord(that, pageIndex, pageSize, state, isReachBottom, ordertype) {
	wx.request({
		url: addr.Address.GetOrderRecord,
		data: {
			appid: getApp().globalData.appid,
			uid: getApp().globalData.userInfo.UserId,
			pageIndex: pageIndex,
			pageSize: pageSize,
			state: state,
			ordertype: ordertype
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				if (isReachBottom == 0) {
					that.setData({ list: res.data.list })
					that.data.pageIndex = 2
				} else {
					if (res.data.list.length > 0) {
						that.data.list = that.data.list.concat(res.data.list)
						that.setData({ list: that.data.list })
						++that.data.pageIndex
					}
				}
			}
		},
		fail: function (res) {
			console.log('请求预约列表失败')
		}
	})
}

function payGift(that, payWay, tid, cb) {
	wx.request({
		url: addr.Address.payGift,
		data: {
			appid: getApp().globalData.appid,
			uid: getApp().globalData.userInfo.UserId,
			payWay: payWay,
			tid: tid,
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				cb(res)
			}
		},
		fail: function (res) {
			console.log('赠送鲜花下单失败')
		}
	})
}

function GiftList(that, pageIndex, pageSize, isReachBottom) {
	wx.request({
		url: addr.Address.GiftList,
		data: {
			appid: getApp().globalData.appid,
			uid: getApp().globalData.userInfo.UserId,
			pageIndex: pageIndex,
			pageSize: pageSize,
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				if (isReachBottom == 0) {
					that.setData({ list: res.data.list, recordCount: res.data.recordCount, sum: res.data.sum })
					that.data.pageIndex = 2
				} else {
					if (res.data.list.length > 0) {
						that.data.list = that.data.list.concat(res.data.list)
						that.setData({ list: that.data.list, recordCount: res.data.recordCount, sum: res.data.sum })
						++that.data.pageIndex
					}
				}
			}
		},
		fail: function (res) {
			console.log('请求送花记录列表失败')
		}
	})
}


module.exports = {
	// 微信接口
	openLocation: openLocation,
	previewStoreimg: previewStoreimg,
	// 足浴客户端api
	GetStoreInfo: GetStoreInfo,
	GetTechnicianList: GetTechnicianList,
	GetServiceList: GetServiceList,
	GetTechnicianInfo: GetTechnicianInfo,
	GetServiceInfo: GetServiceInfo,
	GetDateTable: GetDateTable,
	CancelPay: CancelPay,
	GetOrderRecord: GetOrderRecord,
	payGift: payGift,
	GiftList: GiftList,
}