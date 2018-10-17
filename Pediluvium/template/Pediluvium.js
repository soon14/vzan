var addr = require("../utils/addr.js")
var template = require("../template/template.js")
let _get = require('../utils/lodash.get');
var app = getApp()


function SendUserAuthCode(that, telePhoneNumber) {
	wx.request({
		url: addr.Address.SendUserAuthCode,
		data: {
			telePhoneNumber: telePhoneNumber
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {

			} else {
				template.showtoast('系统繁忙！！！', 'loading')
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '获取验证码失败',
				content: res.data.msg,
			})
		}
	})
}

function BindPhoneNumber(that, telePhoneNumber, verificationCode) {
	wx.request({
		url: addr.Address.BindPhoneNumber,
		data: {
			appId: app.globalData.appid,
			openId: app.globalData.userInfo.openId || wx.getStorageSync('userInfo').openId,
			telePhoneNumber: telePhoneNumber,
			verificationCode: verificationCode,
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {
				template.goTabPage('../Staging/Staging')
			} else {
				template.showmodal('绑定失败', res.data.Msg, false, 0)
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '绑定手机号码失败',
				content: res.data.msg,
			})
		}
	})
}

function GetTechInfo(that, isntMe) {
	var phone = _get(app.globalData, "userInfo.TelePhone", "");
	var appid = _get(app.globalData, "appid", "");
	if (phone == "" || appid == "")
		return;
	wx.request({
		url: addr.Address.GetTechInfo,
		data: {
			telePhoneNumber: phone,
			appid: appid
		},
		method: "GET",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {
				app.globalData.technicianId = res.data.dataObj.userInfo.id
				app.globalData.storeId = res.data.dataObj.store.Id
				that.setData({ dataObj: res.data.dataObj })
				if (isntMe == 0) {
					if (app.globalData.userInfo.TelePhone == '未绑定') {
						that.setData({ pageCondition: 0 })
					} else {
						//template.showtoast('绑定成功！', 'success')
						// setTimeout(function () {
						// 	template.goTabPage('../Staging/Staging')
						// }, 1500)
					}
				}
				if (isntMe == 4) {
					GetMyOrder(that, 2, 1, 0)
					GetMyOrderCount(that)
				}
				if (isntMe == 5) {
					app.globalData.id = res.data.dataObj.userInfo.id
					that.setData({
						imgLogo: res.data.dataObj.userInfo.headImg,
						username: res.data.dataObj.userInfo.jobNumber,
						sign: res.data.dataObj.userInfo.desc,
						imgArray: res.data.dataObj.photos
					})
				}
			} else {
				if (res.data.code == 3 && (app.globalData.userInfo.TelePhone != '未绑定' || app.globalData.userInfo.TelePhone != null)) {
					that.setData({ pageCondition: 2 })
				} else {
					that.setData({ pageCondition: 0 })
				}
				if (res.data.Msg) {
					wx.showModal({
						title: '提示',
						content: res.data.Msg,
						showCancel: false,
						success: function (res) {
							if (res.confirm) {
								wx.reLaunch({
									url: '/pages/index/index',
								})
							}
						}
					})

				}
				console.log('GetTechInfo', res)
				console.log('phone', app.globalData.userInfo.TelePhone)
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '获取用户信息失败',
				content: res.data.msg,
			})
		}
	})
}

function GetGiftsOrderDescList(that) {
	wx.request({
		url: addr.Address.GetGiftsOrderDescList,
		data: {
			storeId: app.globalData.storeId
		},
		method: "GET",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {
				that.setData({ rankingArray: res.data.dataObj })
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '获取收花排行版失败',
				content: res.data.msg,
			})
		}
	})
}

function GetMyGiftsCount(that) {
	wx.request({
		url: addr.Address.GetMyGiftsCount,
		data: {
			technicianId: app.globalData.technicianId,
			getGiftsRecordType: 0
		},
		method: "GET",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {
				that.setData({ allMsg: res.data.dataObj })
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '获取我的收花总信息失败',
				content: res.data.msg,
			})
		}
	})
}

function GetMyGifts(that, getGiftsRecordType, pageIndex, isReachBottom) {
	wx.request({
		url: addr.Address.GetMyGifts,
		data: {
			technicianId: app.globalData.technicianId,
			getGiftsRecordType, getGiftsRecordType,
			pageIndex: pageIndex,
			pageSize: 6
		},
		method: "GET",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {
				if (isReachBottom == 0) {
					that.setData({ rankingArray: res.data.dataObj })
					that.data.pageindex = 2
				} else {
					if (res.data.dataObj.length > 0) {
						that.data.rankingArray = that.data.rankingArray.concat(res.data.dataObj)
						that.setData({ rankingArray: that.data.rankingArray })
						++that.data.pageindex
					}
				}
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '获取我的收花信息失败',
				content: res.data.msg,
			})
		}
	})
}

function ChangeTechnicianState(that, state) {
	wx.request({
		url: addr.Address.ChangeTechnicianState,
		data: {
			id: that.data.dataObj.userInfo.id,
			state, state
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {
				GetTechInfo(that)
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '修改状态失败',
				content: res.data.msg,
			})
		}
	})
}

function SaveUserInfo(that, userInfo, cb) {
	wx.request({
		url: addr.Address.SaveUserInfo,
		data: {
			userInfo: userInfo
		},
		method: "POST",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok == true) {
				cb(res)
			} else {
				template.showmodal('提示', res.data.Msg, false, 1)
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '修改信息失败',
				content: res.data.msg,
			})
			cb(res)
		}
	})
}

function GetMyOrder(that, getOrderType, pageIndex, isReachBottom) {
	if (app.globalData.technicianId == 0) {
		return
	}
	wx.request({
		url: addr.Address.GetMyOrder,
		data: {
			technicianId: app.globalData.technicianId,
			getOrderType: getOrderType,
			pageIndex: pageIndex,
			pageSize: 6
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok == true) {
				for (var i = 0; i < res.data.dataObj.length; i++) {
					res.data.dataObj[i].serviceEndTime = res.data.dataObj[i].serviceEndTime.replace(/-/g, '/')
					res.data.dataObj[i].serviceTime = res.data.dataObj[i].serviceTime.replace(/-/g, '/')
				}
				if (isReachBottom == 0) {
					that.setData({ orderArray: res.data.dataObj })
					that.data.pageindex = 2
				} else {
					if (res.data.dataObj.length > 0) {
						that.data.orderArray = that.data.orderArray.concat(res.data.dataObj)
						that.setData({ orderArray: that.data.orderArray })
						++that.data.pageindex
					}
				}
			} else {
				template.showmodal('提示', res.data.Msg, false, 1)
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '订单查询失败',
				content: res.data.msg,
			})
			cb(res)
		}
	})
}

function GetMyOrderCount(that) {
	if (app.globalData.technicianId == 0) {
		return
	}
	wx.request({
		url: addr.Address.GetMyOrderCount,
		data: {
			technicianId: app.globalData.technicianId,
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok == true) {
				that.data.orderState[0].nums = res.data.dataObj.waitOrderCount
				that.data.orderState[1].nums = res.data.dataObj.reservedOrderCount
				that.data.orderState[2].nums = res.data.dataObj.complateOrderCount
				that.setData({ orderState: that.data.orderState })
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '订单总数据查询失败',
				content: res.data.msg,
			})
			cb(res)
		}
	})
}

function UpdateOrderState(that, orderId, orderState) {
	wx.request({
		url: addr.Address.UpdateOrderState,
		data: {
			orderId: orderId,
			orderState: orderState,
			technicianId: app.globalData.technicianId
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {
				template.showtoast('更新成功', 'success')
				setTimeout(function () {
					GetMyOrder(that, 2, 1, 0)
				}, 1500)
			}
		},
		fail: function (res) {
			wx.showModal({
				title: '订单总数据查询失败',
				content: res.data.msg,
			})
			cb(res)
		}
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

module.exports = {
	// 微信接口
	previewStoreimg: previewStoreimg,
	// 足浴api
	SendUserAuthCode: SendUserAuthCode,
	BindPhoneNumber: BindPhoneNumber,
	GetTechInfo: GetTechInfo,
	GetGiftsOrderDescList: GetGiftsOrderDescList,
	GetMyGiftsCount: GetMyGiftsCount,
	GetMyGifts: GetMyGifts,
	ChangeTechnicianState: ChangeTechnicianState,
	SaveUserInfo: SaveUserInfo,
	GetMyOrder: GetMyOrder,
	GetMyOrderCount: GetMyOrderCount,
	UpdateOrderState: UpdateOrderState,
}