var addr = require("../utils/addr.js");

// 复制
function copy(data) {
	var that = this
	wx.setClipboardData({
		data: data,
		success: function (res) {
			wx.getClipboardData({
				success: function (res) {
					wx.showToast({
						title: '复制成功',
					})
				}
			})
		}
	})
}

function ChangeDateFormatNew(val) {
	if (val != null) {
		var date = new Date(parseInt(val.replace("/Date(", "").replace(")-", ""), 10));
		var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
		var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
		var hour = date.getHours();
		var minute = date.getMinutes();
		var second = date.getSeconds();
		var dd = date.getFullYear() + "/" + month + "/" + currentDate + " " + hour + ":" + minute + ":" + second;
		return dd;
	}
	return "";
}

// var app = getApp();
//异步请求
function postAsync(_url, _data) {
	return new Promise(function (resolve, reject) {
		// console.log("////////////////////");
		// console.log(app.globalData.userInfo.openId);
		wx.request({
			url: _url,
			data: _data || {},
			method: 'POST',
			header: {
				"content-type": "application/x-www-form-urlencoded"
			}, // 设置请求的 header
			success: function (res) {
				// success
				if (res.statusCode == 200) {

					resolve(res.data);
				}
				else
					reject(res);
			},
			fail: function (e) {
				let _str = `请求 ${_url} 失败！
          错误信息：${e.errMsg}`;
				wx.showModal({
					title: '提示',
					content: _str,
					showCancel: false,
					success: function (res) {

					}
				})
				reject("");
			}
		})
	})
}


function formatDuring(mss) {
	if (mss < 0) {
		return "00:00:00";
	} else {
		var days = parseInt(mss / (1000 * 60 * 60 * 24));
		var hours = parseInt((mss % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
		var minutes = parseInt((mss % (1000 * 60 * 60)) / (1000 * 60));
		var seconds = (mss % (1000 * 60)) / 1000;
		if (hours == 0) {
			return minutes + "分钟" + Math.round(seconds) + "秒";
		} else if (days == 0) {
			return hours + "小时" + minutes + "分钟" + Math.round(seconds) + "秒";
		} else {
			return days + "天" + hours + "小时" + minutes + "分钟" + Math.round(seconds) + "秒";
		}
	}
}

	function previewImageList(index, photoList) {
		var previewImage = photoList[index]
		wx.previewImage({
			current: previewImage,
			urls: photoList
		})
	}



  //跳转新页面
function goNewPage(url) {
	if (getCurrentPages().length >= 5) {
		wx.redirectTo({
			url: url,
		})
	} else {
		wx.navigateTo({
			url: url,
		})
	}
  }

function goTabPage(url) {
	wx.switchTab({
		url: url,
	})
}

function goBackPage(delta) {
	wx.navigateBack({
		delta: delta
	})
}

function goNewPageByRd(url) {
	wx.redirectTo({
		url: url,
	})
}

function goNewPageByRl(url) {
	wx.reLaunch({
		url: url,
	})
}

function showtoast(title, icon) {
	wx.showToast({
		title: title,
		icon: icon
	})
}

function showmodal(title, content, confirmText, showCancel) {
	wx.showModal({
		title: title,
		content: content,
		confirmText: confirmText,
		showCancel: showCancel,
		confirmColor: '#f20033',
		success: function (res) {
			if (res.confirm) {

			}
		}
	})
}

function stopPullDown() {
	wx.showNavigationBarLoading()
	setTimeout(function () {
		wx.showToast({
			title: '刷新成功',
			icon: 'success'
		})
		wx.hideNavigationBarLoading()
		wx.stopPullDownRefresh()
	}, 1000)
}

function makePhoneCall(phoneNumber) {
	wx.makePhoneCall({
		phoneNumber: phoneNumber,
	})
}

// 获取水印开关
function GetAgentConfigInfo(that) {
	wx.request({
		url: addr.Address.GetAgentConfigInfo,
		data: {
			appid: getApp().globalData.appid,
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
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
}




// 获取小程序店铺卡套
function GetCardSign(that) {
	wx.request({
		url: addr.Address.GetCardSign,
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId,
			type: 1
		},
		method: "GET",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			if (res.data.isok) {
				that.data.vipcard = res.data.obj

				var timestamp = res.data.obj.timestamp
				var carid = res.data.obj.cardId
				var signature = res.data.obj.signature
				var cardext = { 'timestamp': timestamp, 'signature': signature }
				cardext = JSON.stringify(cardext)
				// 领卡
				wx.addCard({
					cardList: [
						{
							cardId: carid,
							cardExt: cardext
						}
					],
					success: function (res) {
						console.log(res.cardList) // 卡券添加结果
						wx.showToast({
							title: '领取成功',
						})
						that.setData({ iscloseBtn: 0 })
						SaveWxCardCode(res.cardList[0].code)
					},
					fail: function (res) {
						console.log(res)
					}
				})
			} else {
				wx.showModal({
					title: '亲爱的会员',
					content: '您已经是该店会员卡。可以返回微信从卡包中查看！',
					showCancel: false,
					confirmText: '我知道了'
				})
			}
		},
		fail: function () {
			console.log('获取小程序店铺卡套出错')
		}
	})
}
// 保存提交领卡后得到的code
function SaveWxCardCode(code) {
	wx.request({
		url: addr.Address.SaveWxCardCode,
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId,
			code: code,
			type: 1
		},
		method: "POST",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			UpdateWxCard(this)
			console.log('上传code成功',res)
		},
		fail: function () {
			console.log('上传code出错')
		}
	})
}

// 通过cardId领取微信会员卡，得到code
function GetWxCardCode(that) {
	wx.request({
		url: addr.Address.GetWxCardCode,
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId,
			type: 1
		},
		method: "POST",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok) {
				if (res.data.obj == null) {
					var a = 1
				} else {
					var a = 0
				}
				that.setData({
					iscloseBtn: a,
					havecard: res.data.obj
				})
			}
		},
		fail: function () {
			console.log('判断是否领过卡出错')
		}
	})
}
// 保存提交领卡后得到的code
function SaveWxCardCode(code) {
	wx.request({
		url: addr.Address.SaveWxCardCode,
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId,
			code: code
		},
		method: "POST",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			UpdateWxCard(this)
			console.log('上传会员卡code成功',res)
		},
		fail: function () {
			console.log('上传会员卡code出错')
		}
	})
}
// 用户领卡并且提交code后,在消费完成后,请求同步到微信卡包接口更新会员信息
function UpdateWxCard(that) {
	wx.request({
		url: addr.Address.UpdateWxCard,
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId,
			type: 1
		},
		method: "POST",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok == true) {
				console.log(res)
				console.log('更新会员卡信息成功')
			}
		},
		fail: function () {
			console.log('更新会员卡信息出错')
		}
	})
}



// 获取会员信息
function GetVipInfo(that, cb) {
	wx.request({
		url: addr.Address.GetVipInfo,
		data: {
			appid: getApp().globalData.appid,
			uid: getApp().globalData.userInfo.UserId,
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok) {
				getApp().globalData.levelid = res.data.model.levelid
				res.data.model.PriceSum = (parseFloat(res.data.model.PriceSum) / 100).toFixed(2)
				that.setData({
					model: res.data.model,
				})
				cb('isok')
			}
		},
		fail: function () {
			console.log('获取不了会员信息')
		}
	})
}

function addSaveMoneySet(that, saveMoneySetId) {
	wx.request({
		url: addr.Address.addSaveMoneySet,
		data: {
			appid: getApp().globalData.appid,
			openid: getApp().globalData.userInfo.openId,
			saveMoneySetId: saveMoneySetId
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok) {
				var orderid = res.data.orderid
				that.wxpaymoney(orderid)
			}
		},
		fail: function () {
			console.log('请求预充值失败')
		}
	})
}


function getSaveMoneySetList(that) {
	wx.request({
		url: addr.Address.getSaveMoneySetList,
		data: {
			appid: getApp().globalData.appid,
			openid: getApp().globalData.userInfo.openId
		},
		method: "GET",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok) {
				that.setData({
					saveMoneySetList: res.data.saveMoneySetList,
				})
			}
		},
		fail: function () {
			console.log('获取储值列表失败')
		}
	})
}

// 获取储值余额
function getSaveMoneySetUser(that) {
	wx.request({
		url: addr.Address.getSaveMoneySetUser,
		data: {
			appid: getApp().globalData.appid,
			openId: getApp().globalData.userInfo.openId,
		},
		method: "GET",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {
				that.setData({ AccountMoneyStr: res.data.saveMoneySetUser.AccountMoneyStr })
				getApp().globalData.AccountMoneyStr = res.data.saveMoneySetUser.AccountMoneyStr
			}
		},
		fail: function () {
			console.log('获取储值余额信息')
		}
	})
}



function getSaveMoneySetUserLogList(that) {
	wx.request({
		url: addr.Address.getSaveMoneySetUserLogList,
		data: {
			appid: getApp().globalData.appid,
			openid: getApp().globalData.userInfo.openId,
		},
		method: "GET",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == 1) {
				var saveMoneyUserLogList = res.data.saveMoneyUserLogList
				for (var i = 0; i < saveMoneyUserLogList.length; i++) {
					saveMoneyUserLogList[i].ChangeMoneyStr = (parseFloat(saveMoneyUserLogList[i].ChangeMoney) / 100).toFixed(2)
					saveMoneyUserLogList[i].AfterMoneyStr = (parseFloat(saveMoneyUserLogList[i].AfterMoney) / 100).toFixed(2)
				}
				that.setData({
					saveMoneyUserLogList: saveMoneyUserLogList
				})
				console.log(saveMoneyUserLogList)
			}
		},
		fail: function () {
			console.log('获取储值记录列表失败')
		}
	})
}

// 提交备用formId
function commitFormId(formid, that) {
	wx.request({
		url: addr.Address.commitFormId,
		data: {
			appid: getApp().globalData.appid,
			openid: getApp().globalData.userInfo.openId,
			formid: formid
		},
		method: "POST",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			console.log('commitFormId', res)
		},
		fail: function () {
			wx.showToast({
				title: '默认地址出错',
			})
		}
	})
}

// 立减金
function GetReductionCard(that, couponsId, orderId, state, cb) {
	wx.request({
		url: addr.Address.GetReductionCard,
		data: {
			orderType: 1,
			couponsId: couponsId,
			orderId: orderId,
			userId: getApp().globalData.userInfo.UserId,
			openId: getApp().globalData.userInfo.openId
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (data) {
			var data = data.data
			wx.setStorageSync("coupon", data.coupon)
			if (data.coupon != null) {
				data.coupon.StartUseTimeStr = data.coupon.StartUseTimeStr.replace(/[.]/g, '/')
				data.coupon.EndUseTimeStr = data.coupon.EndUseTimeStr.replace(/[.]/g, '/')

				that.setData({
					coupon: data.coupon,
					userList: data.userList
				})
			}
			if (data.isok) {
				if (state == 0) { //getsmoney
					for (var i = 0; i < data.coupon.SatisfyNum; i++) {
						that.data.userLogo.push({
							HeadImgUrl: ''
						})
					}
					for (var j = 0; j < data.userList.length; j++) {
						that.data.userLogo[j].HeadImgUrl = data.userList[j].HeadImgUrl
					}
					that.setData({ userLogo: that.data.userLogo })
				}

				if (state == 1) { //click--invitegetsmoney
					if (data.coupon.SatisfyNum == data.userList.length) {

						if (data.userInfo == null) {
							wx.showToast({
								title: '该活动已满员',
								icon: 'loading'
							})
							return
						} else {
							var finduserid = data.userList.find(f => f.Id == data.userInfo.Id)

							if (finduserid) {
								wx.showToast({
									title: '领取成功'
								})
								setTimeout(function () {
									var coupon = JSON.stringify(that.data.coupon)
									wx.redirectTo({
										url: '/pages/addCoupon/usersmoney?orderId=' + getApp().globalData.orderid
									})
								}, 1000)
							} else {
								wx.showToast({
									title: '该活动已被领取'
								})
							}
						}

					} else {
						wx.redirectTo({
							url: '/pages/addCoupon/getsmoney'
						})
					}
				}

				if (state == 2) { //onload--invitegetsmoney 
					if (data.userInfo == null) {
						wx.showToast({
							title: '该活动已满员',
							icon: 'loading'
						})
						return
					}
					var finduserid = data.userList.find(f => f.Id == data.userInfo.Id)
					if (data.coupon.SatisfyNum > data.userList.length) {
						wx.redirectTo({
							url: '../addCoupon/getsmoney'
						})
						return
					}
					if (finduserid) {
						if (getCurrentPages()[getCurrentPages().length - 1].route != "pages/addCoupon/usersmoney") {
							wx.redirectTo({
								url: '../addCoupon/usersmoney?coupon=' + JSON.stringify(data.coupon)
							})
						}
					}

				}
			}
			else {
				if (data.coupon == null) {
					wx.showToast({ title: '该活动已取消', icon: 'loading' })
					return
				}
				if (data.userList == undefined && data.msg == "立减金已放送完毕,请关注下次的立减金优惠哦") {
					wx.redirectTo({
						url: '../addCoupon/usersmoney?coupon=' + JSON.stringify(wx.getStorageSync("coupon")) + '&isok=-1'
					})
					// that.setData({
					// 	isGet: true,
					// 	msg: data.msg,
					// })
					return;
				}
				if (data.userList == undefined && data.msg == "你已超过领取限制") {
					that.setData({
						isGet: true,
						msg: data.msg,
					})
					return;
				}
				wx.showToast({
					title: data.msg,
					icon: 'loading'
				})
			}
		}
	})
}


// 查询正在参与的立减金活动
function GetReductionCardList(that) {
	wx.request({
		url: addr.Address.GetReductionCardList,
		data: {
			userId: getApp().globalData.userInfo.UserId,
			openId: getApp().globalData.userInfo.openId,
			aid: getApp().globalData.aid,
			storeId: getApp().globalData.storeId,
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (data) {
			var data = data.data
			if (data.isok) {
				for (var i = 0; i < data.coupons.length; i++) {
					data.coupons[i].StartUseTimeStr = data.coupons[i].StartUseTimeStr.replace(/[.]/g, '/')
					data.coupons[i].EndUseTimeStr = data.coupons[i].EndUseTimeStr.replace(/[.]/g, '/')
				}
				that.setData({ couponsList: data.coupons })
			}
		}
	})
}

function GetMyCouponList(postData, isreachbottom) {
	return postAsync(addr.Address.GetMyCouponList, postData);
}

function GetStoreCouponList(that, goodstype) {
	wx.request({
		url: addr.Address.GetStoreCouponList,
		data: {
			appId: getApp().globalData.appid,
			userId: getApp().globalData.userInfo.UserId,
			goodstype: goodstype
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			that.setData({ couponList: res.data.postdata })
			wx.hideLoading()
		},
		fail: function () {
			wx.showToast({
				title: '获取优惠券列表出错', icon: 'loading'
			})
		}
	})
}

function GetCoupon(that, couponId) {
	wx.request({
		url: addr.Address.GetCoupon,
		data: {
			appId: getApp().globalData.appid,
			userId: getApp().globalData.userInfo.UserId,
			couponId: couponId
		},
		method: "POST",
		header: {
			"content-type": "application/x-www-form-urlencoded"
		},
		success: function (res) {
			if (res.data.isok == true) {
				wx.startPullDownRefresh()
				stopPullDown()
			}
			wx.showModal({
				title: '提示',
				content: res.data.msg,
				showCancel: false,
			})
		},
		fail: function () {
			wx.showToast({
				title: '获取优惠券列表出错', icon: 'loading'
			})
		}
	})
}

module.exports = {
	postAsync: postAsync, //异步请求
	ChangeDateFormatNew: ChangeDateFormatNew,
	// 请求接口
	GetAgentConfigInfo: GetAgentConfigInfo,
	GetVipInfo: GetVipInfo,
	GetCardSign: GetCardSign,
	GetWxCardCode: GetWxCardCode,
	SaveWxCardCode: SaveWxCardCode,
	UpdateWxCard: UpdateWxCard,
	GetReductionCard: GetReductionCard,
	GetReductionCardList: GetReductionCardList,
	GetMyCouponList: GetMyCouponList,
	GetStoreCouponList: GetStoreCouponList,
	GetCoupon: GetCoupon,

	// 方法(wxAPI)
	copy: copy,
	formatDuring: formatDuring,
	previewImageList: previewImageList,
	goNewPage: goNewPage,
	goTabPage: goTabPage,
	goBackPage: goBackPage,
	goNewPageByRd: goNewPageByRd,
	goNewPageByRl: goNewPageByRl,
	showtoast: showtoast,
	showmodal: showmodal,
	stopPullDown: stopPullDown,
	makePhoneCall: makePhoneCall,
	addSaveMoneySet: addSaveMoneySet,
	getSaveMoneySetList: getSaveMoneySetList,
	getSaveMoneySetUser: getSaveMoneySetUser,
	getSaveMoneySetUserLogList: getSaveMoneySetUserLogList,
	commitFormId: commitFormId
}