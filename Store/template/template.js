var addr = require("../utils/addr.js");


function formatDuring(mss) {
	var days = parseInt(mss / (1000 * 60 * 60 * 24));
	var hours = parseInt((mss % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
	var minutes = parseInt((mss % (1000 * 60 * 60)) / (1000 * 60));
	var seconds = (mss % (1000 * 60)) / 1000;
	return days + "天" + hours + "小时" + minutes + "分钟" + seconds + "秒";
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

function showmodal(title, content, showCancel, dosth) {
	wx.showModal({
		title: title,
		content: content,
		showCancel: showCancel,
		confirmColor: '#fe536f',
		success: function (res) {
			if (res.confirm) {
				if (dosth == 1) {
					goBackPage(2)
				}
			}
		}
	})
}

function stopPullDown() {
	setTimeout(function () {
		wx.showToast({
			title: '刷新成功',
			icon: 'success'
		})
		wx.stopPullDownRefresh()
	}, 1000)
}

function makePhoneCall(phoneNumber) {
	wx.makePhoneCall({
		phoneNumber: phoneNumber,
	})
}

// 水印
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
			userid: getApp().globalData.userInfo.UserId
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok) {
				that.data.vipcard = res.data.obj

				var timestamp = res.data.obj.timestamp
				var carid = res.data.obj.cardId
				var signature = res.data.obj.signature
				var cardext = { 'code': '', 'openid': '', 'timestamp': timestamp, 'signature': signature }
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
					// content: '您已经是该店会员卡。可以返回微信从卡包中查看！',
					content: res.data.msg,
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
// 通过cardId领取微信会员卡，得到code
function GetWxCardCode(that) {
	wx.request({
		url: addr.Address.GetWxCardCode,
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId
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
			console.log('上传会员卡code成功')
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
		},
		method: "POST",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			console.log(res)
			console.log('更新会员卡信息成功')
		},
		fail: function () {
			console.log('更新会员卡信息出错')
		}
	})
}


// 获取会员信息
function GetVipInfo(that, isBook, cb) {
	wx.request({
		url: addr.Address.GetVipInfo,
		data: {
			appid: getApp().globalData.appid,
			uid: getApp().globalData.userInfo.UserId,
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok == true) {
				getApp().globalData.levelid = res.data.model.levelid
				res.data.model.PriceSum = parseFloat(res.data.model.PriceSum).toFixed(2)
				that.setData({ model: res.data.model })
				if (isBook == 1) {
					cb('success')
				}
			}
		},
		fail: function () {
			console.log('获取不了会员信息')
		}
	})
}

// 获取储值列表
function getSaveMoneySetList(that) {
	wx.request({
		url: addr.Address.getSaveMoneySetList,
		data: {
			appid: getApp().globalData.appid,
			openid: getApp().globalData.userInfo.openId,
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok == true) {
				that.setData({ saveMoneySetList: res.data.saveMoneySetList })
			}
		},
		fail: function () {
			console.log('获取不了储值列表')
		}
	})
}

// 获取储值金额
function getSaveMoneySetUser(that) {
	wx.request({
		url: addr.Address.getSaveMoneySetUser,
		data: {
			appid: getApp().globalData.appid,
			openid: getApp().globalData.userInfo.openId,
		},
		method: "GET",
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			if (res.data.isok == true) {
				that.setData({ saveMoneySetUser: res.data.saveMoneySetUser })
			}
		},
		fail: function () {
			console.log('获取不了储值金额')
		}
	})
}

// 请求预充值
function addSaveMoneySet(that, saveMoneySetId, cb) {
	wx.request({
		url: addr.Address.addSaveMoneySet,
		data: {
			appid: getApp().globalData.appid,
			openid: getApp().globalData.userInfo.openId,
			saveMoneySetId: saveMoneySetId,
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded'
		},
		success: function (res) {
			if (res.data.isok == true) {
				cb(res)
			}
		},
		fail: function () {
			console.log('预充值请求失败')
		}
	})
}

// 查询消费记录
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
				for (var i = 0; i < res.data.saveMoneyUserLogList.length; i++) {
					res.data.saveMoneyUserLogList[i].ChangeNote = res.data.saveMoneyUserLogList[i].ChangeNote.replace('【充值】', '')
					res.data.saveMoneyUserLogList[i].ChangeNote = res.data.saveMoneyUserLogList[i].ChangeNote.replace('【消费】', '')
					res.data.saveMoneyUserLogList[i].ChangeNote = res.data.saveMoneyUserLogList[i].ChangeNote.replace('【退款】', '')
				}
				that.setData({ saveMoneyUserLogList: res.data.saveMoneyUserLogList })
			}
		},
		fail: function () {
			console.log('查询消费记录失败')
		}
	})
}

// 提交备用formId
function commitFormId(formid, that) {
	wx.request({
		url: addr.Address.commitFormId, //仅为示例，并非真实的接口地址
		data: {
			appid: getApp().globalData.appid,
			openid: getApp().globalData.userInfo.openId,
			formid: formid
		},
		method: "POST",
		header: {
			'content-type': 'application/json' // 默认值
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


module.exports = {
	// 请求接口
	GetAgentConfigInfo: GetAgentConfigInfo,
	GetVipInfo: GetVipInfo,
	GetCardSign: GetCardSign,
	GetWxCardCode: GetWxCardCode,
	SaveWxCardCode: SaveWxCardCode,
	UpdateWxCard: UpdateWxCard,
	// 方法(wxAPI)
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
	getSaveMoneySetList: getSaveMoneySetList,
	getSaveMoneySetUser: getSaveMoneySetUser,
	addSaveMoneySet: addSaveMoneySet,
	getSaveMoneySetUserLogList: getSaveMoneySetUserLogList,
	commitFormId: commitFormId,
}