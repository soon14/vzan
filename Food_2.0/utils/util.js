import Promise from './es6-promise.min.js';
// var crypt = require("./crypt.js");
var addr = require("addr");
var app = getApp();

//时间对比函数，如果a>b返回1，如果a<b返回-1，相等返回0
function compareDateFormat(a, b) {
	var dateA = new Date(a.replace(/\-/g, "\/"));
	var dateB = new Date(b.replace(/\-/g, "\/"));
	if (isNaN(dateA) || isNaN(dateB)) return null;
	if (dateA > dateB) return 1;
	if (dateA < dateB) return -1;
	return 0;
}

//时间对比函数，如果a>b返回1，如果a<b返回-1，相等返回0
function compareDateFormatstr(a, b) {
	var dateA = new Date(a);
	var dateB = new Date(b);
	if (isNaN(dateA) || isNaN(dateB)) return null;
	if (dateA > dateB) return 1;
	if (dateA < dateB) return -1;
	return 0;
}

function compareTime(originTime, targetTime) {
	var argA = originTime.split(":");
	var argB = targetTime.split(":");
	console.log(argA[0]);
	var dateA = new Date(); // 创建 Date 对象。
	var dateB = new Date(); // 创建 Date 对象。
	dateA.setHours(argA[0], (argA[1] + '').indexOf('0') == 0 && argA[1].length > 1 ? argA[1].substring(1) : argA[1], 0);  // 设置 UTC 小时，分钟，秒。
	dateB.setHours(argB[0], (argB[1] + '').indexOf('0') == 0 && argB[1].length > 1 ? argB[1].substring(1) : argB[1], 30);  // 设置 UTC 小时，分钟，秒。
	if (isNaN(dateA) || isNaN(dateB)) return null;
	if (dateA > dateB) return 1;
	if (dateA < dateB) return -1;
	return 0;
}
//格式化时间
function formatTime(date, formatstring) {
	var year = date.getFullYear()
	var month = date.getMonth() + 1
	var day = date.getDate()

	var hour = date.getHours()
	var minute = date.getMinutes()
	var second = date.getSeconds()

	if (formatstring == null || formatstring == undefined) {
		return [year, month, day].map(formatNumber).join('/') + ' ' + [hour, minute, second].map(formatNumber).join(':')
	}
	else if (formatstring == "yyyy.MM.dd HH:mm") {
		return [year, month, day].map(formatNumber).join('.') + ' ' + [hour, minute].map(formatNumber).join(':')
	}
	else if (formatstring == "yyyy-MM-dd") {
		return [year, month, day].map(formatNumber).join('-')
	}
}

//时间间隔,a>b,返回天数，小时数，分钟数
function timeDiff(a, b) {
	var timespan = a - b;
	var days = parseInt(timespan / 3600000 / 24);
	var hoursspan = timespan - days * 24 * 3600000;
	var hours = parseInt(hoursspan > 0 ? hoursspan / 3600000 : 0);
	var minutesspan = hoursspan - hours * 3600000;
	var minutes = parseInt(minutesspan > 0 ? minutesspan / 60000 : 0);
	return [days, hours, minutes];
}

// 登陆必要参数
// function primaryLoginArgs(params={}) {
//     var unionid = wx.getStorageSync('unionId')
//     var sysInfo = wx.getSystemInfoSync()
//     var versionCode = "1.0"
//     var deviceType = sysInfo.model
//     deviceType = deviceType.toLowerCase().indexOf("iphone") > -1 ? "iPhone" : deviceType
//     console.log("设备", deviceType, deviceType.toLowerCase(), deviceType.toLowerCase().indexOf("iphone"))
//     var timestamp = (new Date()).getTime()
//     var sign = crypt.getVerifyModel(unionid, versionCode, deviceType, timestamp);
//     var verifyModel = {};
//     verifyModel.deviceType = deviceType //"ios9.0"
//     verifyModel.timestamp = timestamp + ""// 1479174892808
//     verifyModel.uid = unionid//"oW2wBwUJF_7pvDFSPwKfSWzFbc5o"
//     verifyModel.versionCode = versionCode + ""//"1.0"
//     verifyModel.sign = sign//"817AF07823E5CF86031A8A34FB593D1EC12A5499D66EBA10E7C4B6D034EF1C67A9C8FE9FF2A33F82"
//
//     var data = {
//         "deviceType": verifyModel.deviceType, "timestamp": verifyModel.timestamp, "UnionId": verifyModel.uid,
//         "uid": verifyModel.uid, "versionCode": verifyModel.versionCode, "sign": verifyModel.sign
//     }
//     extendObject(data, params);
//     return data;
// }

/** 
     * js截取字符串，中英文都能用 
     * @param str：需要截取的字符串 
     * @param len: 需要截取的长度 
     */
function cutstr(str, len) {
	var str_length = 0;
	var str_len = 0;
	var str_cut = new String();
	str_len = str.length;
	for (var i = 0; i < str_len; i++) {
		var a = str.charAt(i);
		str_length++;
		if (escape(a).length > 4) {
			//中文字符的长度经编码之后大于4  
			str_length++;
		}
		str_cut = str_cut.concat(a);
		if (str_length >= len) {
			str_cut = str_cut.concat("...");
			return str_cut;
		}
	}
	//如果给定字符串小于指定长度，则返回源字符串；  
	if (str_length < len) {
		return str;
	}
}

function formatNumber(n) {
	n = n.toString()
	return n[1] ? n : '0' + n
}

function log(msg) {
	var isDebug = getApp().globalData.isDebug;
	if (isDebug) {
		console.log(msg);
	}
}

function isFunction(value) {
	if (typeof (value) == "function") {
		return true;
	} else {
		return false;
	}
}

/**
 *
 * @param o 遍历对象中的属性，不存在则添加到对象o中
 * @param n
 */
function extendObject(o, n) {
	for (var p in n) {
		if (n.hasOwnProperty(p) && (!o.hasOwnProperty(p)))
			o[p] = n[p];
	}
}

function objToStr(obj) {
	var str = "";
	for (var p in obj) { // 方法
		if (typeof (obj[p]) == "function") {
			// obj [ p ]() ; //调用方法

		} else if (obj[p] != undefined && obj[p] != null) { // p 为属性名称，obj[p]为对应属性的值
			str += p + "=" + obj[p] + "&";
		}
	}
	return str;
}

/** 判断对象是否为空 */
function isOptStrNull(str) {
	if (str == undefined || str == null || str == '' || str == 'null' || str == '[]' || str == '{}') {
		return true
	} else {
		return false;
	}
}

//加载对话框的显示和隐藏
function showLoadingDialog() {
	wx.showToast({
		title: "加载中",
		mask: true,
		icon: 'loading',
		duration: 10000
	})

}

function hideLoadingDialog() {
	wx.hideToast();
}

function showNavigationBarLoading() {
	wx.showNavigationBarLoading();
}

function hideNavigationBarLoading() {
	wx.hideNavigationBarLoading();
}

/**
 * 模态框
 */
function showModal(title = "提示", content = "暂不支持", showCancel = false, confirmText = "确定") {
	return new Promise((resolve, reject) => {
		wx.showModal({
			"title": title,
			"content": content,
			"showCancel": showCancel,
			"confirmText": confirmText,
			"confirmColor": "#ff5d38",
			"success": function (res) {
				if (res.confirm) {
					resolve(res)
				} else {
					reject(res)
				}
			},
			"fail": function (res) {
				reject(res)
			}
		})
	})
		;
}

/**
 * 从本地相册选择图片或使用相机拍照。
 */
function chooseImage(count, sizeType = ['original', 'compressed'], sourceType = ['album', 'camera']) {
	return new Promise((resolve, reject) => {
		wx.chooseImage({
			count: count, // 默认9
			sizeType: sizeType, // 可以指定是原图还是压缩图，默认二者都有
			sourceType: sourceType, // 可以指定来源是相册还是相机，默认二者都有
			success: function (res) {
				// 返回选定照片的本地文件路径列表，tempFilePath可以作为img标签的src属性显示图片
				resolve(res);
			},
			fail: function (res) {
				reject(res);
			}
		});
	})
		;
}

/**
 * 预览图片
 * @param index  当前显示图片的链接 的index
 * @param urls
 */
function previewImage(urls, index = 0) {
	wx.previewImage({
		current: urls[index], // 当前显示图片的http链接
		urls: urls // 需要预览的图片http链接列表
	})
}

/*
 * 用于Toast的bean对象
 * see(showToast)
 */
function Remind() {
	this.showRemind = false
	this.message = '暂无内容'
}
// 自定义Toast
function showToast(that) {
	var bean = that.data.remindBean;
	if (!bean.showRemind) {
		bean.showRemind = true;
		that.setData(that.data);
		setTimeout(function () {
			that.data.remindBean.showRemind = false
			that.setData(that.data)
		}, 2500);
	}
}

function getRequest(url, params) {
	wx.request({
		url: url,
		data: params,
		header: {
			'content-type': 'application/json'
		},
		success: function (res) {
			console.log(res.data)
		},
		fail: function (res) {

		},
		complete: function (res) {

		}
	})
}


function postRequest(url, params) {

	return new Promise((resolve, reject) => {
		wx.request({
			url: url,
			data: params,
			method: 'POST',
			header: {
				'content-type': 'application/json'
			},
			success: function (res) {
				resolve(res);
			},

			fail: function (res) {
				// console.log("failure")
				reject(res);
			},
			complete: function (res) {

			}
		})
	})
}
//转译特殊字符
function jsonReplaceToJSONString(json) {
	var JSONString = JSON.stringify(json)
	JSONString = JSONString.replace(/\&/g, "%26");
	JSONString = JSONString.replace(/\?/g, "%3F");
	return JSONString
}

function JSONStringReplaceToJson(JSONString) {
	var json = JSONString.replace(/\%26/g, "&");
	json = json.replace(/\%3F/g, "?");
	return json
}
//微信支付统
function wxPayRequst(param, callback) {
	var that = this
	wx.request({
		url: addr.Address.AddPayOrder,
		data: {
			itemid: param.itemid,
			paytype: param.paytype,
			extype: param.extype,
			extime: param.extime,
			// extime: 1,
			openId: param.openId,
			quantity: param.quantity,
			areacode: param.areacode,
			appid: getApp().globalData.appid
		},
		method: 'POST',
		header: {
			'content-type': 'application/x-www-form-urlencoded',
		},
		success: function (res) {
			if (res.data.result) {
				var oradid = res.data.obj
				PayOrder(oradid, param, {
					failed: function () {
						callback.failed("failed")
					},
					success: function (res) {
						if (res == "wxpay") {
							callback.success("wxpay")
						} else if (res == "success") {
							callback.success("success")
						}
					}
				})
			} else {
				callback.failed("failed")
			}
		}
	})
}


//微信支付统
function wxPayRequst_Store(param, callback) {
	var that = this
	var url = addr.Address.AddPayOrder_Store  //下单 addMiniappGoodsOrder
	wx.request({
		url: url,
		data: param,
		method: 'POST',
		header: {
			'content-type': 'application/x-www-form-urlencoded',
		},
		success: function (res) {
			if (res.data.isok > 0) {
				getApp().globalData.dbOrder = res.data.dbOrder
				var oradid = res.data.orderid
				var dbOrder = res.data.dbOrder
				var newparam = {
					openId: param.openid,
					orderid: oradid,
					'type': getApp().globalData.payType,
				}
				if (getApp().globalData.payType == 2) { //余额支付
					getApp().globalData.orderid = dbOrder //保存orderid方便查询
					wx.showToast({
						title: '支付成功',
					})
					wx.redirectTo({
						url: '../paySuccess/paySuccess'
					})
					UpdateWxCard(this)
					return
				}
				//如果paytype是1的话调用微信支付
				PayOrder(oradid, newparam, {
					failed: function () {
						callback.failed("failed")
					},
					success: function (res) {
						getApp().globalData.orderid = dbOrder
						if (res == "wxpay") {
							callback.success("wxpay")
						} else if (res == "success") {
							callback.success("success")
						}
					}
				})
			} else {
				wx.showModal({
					title: '提示',
					content: res.data.msg + '不能购买！',
					success: function (res) {
						if (res.confirm) {
							wx.navigateBack({
								delta: 1
							})
						} else if (res.cancel) {
							wx.navigateBack({
								delta: 1
							})
						}
					}
				})
				callback.failed("failed")
			}
		}
	})

}

function AddOrder(param, refun, reparam) {
	var that = this
	wx.showNavigationBarLoading()
	wx.showToast({
		title: '加载中...',
		icon: 'loading',
		duration: 10000
	})
	//生成订单
	wxPayRequst_Store(param, {
		failed: function (res) {
			// setTimeout(function () {
			//   wx.hideToast()
			// }, 500)

			console.log("生成订单失败")

			wx.hideNavigationBarLoading()
			refun(res, 0)
		},
		success: function (res) {
			console.log(res)
			if (res == "wxpay") {
				//发起支付
				wx.hideNavigationBarLoading()
				setTimeout(function () {
					wx.hideToast()
				}, 100)
			} else if (res == "success") {
				refun(reparam, 1)
			}
		}
	})
}

// PayOrder
function PayOrder(orderid, param, pay_callback) {
	var that = this
	wx.request({
		url: addr.Address.PayOrder,
		data: param,
		method: 'POST',
		header: {
			'content-type': 'application/x-www-form-urlencoded',
		},
		success: function (res) {
			console.log(res.obj)
			if (res.data.result == true) {
				var obj = res.data.obj
				var jsObj = JSON.parse(obj)
				//发起支付
				pay_callback.success("wxpay")
				wxpay(jsObj, {
					failed: function () {
						pay_callback.failed("failed")
					},
					success: function () {
						pay_callback.success("success")
					}
				})
			} else {
				// pay_callback.failed("failed")
				pay_callback.failed(res.data.msg)
			}
		}
	})
}
/* 支付   */
function wxpay(param, callback = "function") {
	var taht = this
	var orderId = getApp().globalData.orderid
	console.log(param)
	wx.requestPayment({
		appId: param.appId,
		timeStamp: param.timeStamp,
		nonceStr: param.nonceStr,
		package: param.package,
		signType: param.signType,
		paySign: param.paySign,
		success: function (res) {
			callback.success("success")
			getApp().globalData.ispayOk = 1
		},
		fail: function (res) { //原因 微信支付失败回调大部分机型不触发 2017.11
			console.log('支付失败', res)
			if (getApp().globalData.isOrderlist == 0) {
				wx.showModal({
					title: '提示',
					content: '支付失败',
					success: function (res) {
						if (res.confirm) {
							inite2(orderId, 0, {
								failed: function (res) {
									console.log('更改订单状态失败')
								},
								success: function (res) {
									wx.navigateBack({
										delta: 1
									})
								}
							})
						} else if (res.cancel) {
							inite2(orderId, 0, {
								failed: function (res) {
									console.log('更改订单状态失败')
								},
								success: function (res) {
									wx.navigateBack({
										delta: 1
									})
								}
							})
						}
					}
				})
			} else {
				wx.showModal({
					title: '提示',
					content: '支付失败',
					success: function (res) {
						if (res.confirm) {
							inite2(orderId, 0, {
								failed: function (res) {
									console.log('更改订单状态失败')
								},
								success: function (res) {
								}
							})
						} else if (res.cancel) {
							inite2(orderId, 0, {
								failed: function (res) {
									console.log('更改订单状态失败')
								},
								success: function (res) {
								}
							})
						}
					}
				})
			}
		},
		complete: function (res) {
			UpdateWxCard(this)
			console.log(res)
			// complete
			console.log("pay complete")
		}
	})
}
// 更改订单的状态
function inite2(orderId, State) {
	var that = this
	wx.request({
		url: addr.Address.updateMiniappGoodsOrderState,
		data: {
			AppId: getApp().globalData.appid,
			openid: getApp().globalData.userInfo.openId,
			orderId: orderId,
			State: State
		},
		method: "POST",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			if (res.data.isok == 1) {
				wx.navigateBack({
					delta: 1
				})
				console.log("更新订单状态成功")
			}
		},
		fail: function () {
			console.log("更新订单状态出错")
			wx.showToast({
				title: '更新订单状态出错',
			})
		}
	})
}

function raw1(args) {
	var keys = Object.keys(args)
	keys = keys.sort()
	var newArgs = {}
	keys.forEach(function (key) {
		newArgs[key] = args[key]
	})
	var str = ""
	for (var k in newArgs) {
		str += '&' + k + '=' + newArgs[k]
	}
	str = str.substr(1)
	return str
}

function makePhoneCall(phoneNumber) {
	wx.makePhoneCall({
		phoneNumber: phoneNumber,
		fail: function () {
			this.showModal('提示', '号码有误 , 请重试 !')
		}
	})
}

// 获取水印开关
function GetAgentConfigInfo(that) {
	wx.request({
		url: addr.Address.GetAgentConfigInfo, //仅为示例，并非真实的接口地址
		data: {
			appid: getApp().globalData.appid,
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
}

// 获取会员信息
function GetVipInfo(that) {
	wx.request({
		url: addr.Address.GetVipInfo, //仅为示例，并非真实的接口地址
		data: {
			appid: getApp().globalData.appid,
			uid: getApp().globalData.userInfo.UserId,
		},
		method: "POST",
		header: {
			'content-type': 'application/x-www-form-urlencoded' // 默认值
		},
		success: function (res) {
			if (res.data.isok) {
				getApp().globalData.levelid = res.data.model.levelid
				res.data.model.PriceSum = (parseFloat(res.data.model.PriceSum) / 100).toFixed(2)
				that.setData({
					model: res.data.model,
					goodslist: res.data.model.levelInfo.goodslist
				})
			}
		},
		fail: function () {
			console.log('获取不了会员信息')
		}
	})
}
// 获取储值余额
function getSaveMoneySetUser(that) {
	wx.request({
		url: addr.Address.getSaveMoneySetUser, //仅为示例，并非真实的接口地址
		data: {
			appid: getApp().globalData.appid,
			openId: getApp().globalData.userInfo.openId,
		},
		method: "GET",
		header: {
			'content-type': 'application/x-www-form-urlencoded' // 默认值
		},
		success: function (res) {
			if (res.data.isok == true) {
				that.setData({ AccountMoneyStr: res.data.saveMoneySetUser.AccountMoneyStr })
				getApp().globalData.AccountMoneyStr = res.data.saveMoneySetUser.AccountMoneyStr
			}
		},
		fail: function () {
			console.log('获取不了会员信息')
		}
	})
}
// 获取小程序店铺卡套
function GetCardSign(that) {
	wx.request({
		url: addr.Address.GetCardSign, //仅为示例，并非真实的接口地址
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
// 通过cardId领取微信会员卡，得到code
function GetWxCardCode(that) {
	wx.request({
		url: addr.Address.GetWxCardCode, //仅为示例，并非真实的接口地址
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId,
			type: 1
		},
		method: "POST",
		header: {
			'content-type': 'application/json' // 默认值
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
		url: addr.Address.SaveWxCardCode, //仅为示例，并非真实的接口地址
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId,
			code: code,
			type: 1
		},
		method: "POST",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			UpdateWxCard(this)
			console.log('上传code成功')
		},
		fail: function () {
			console.log('上传code出错')
		}
	})
}
// 用户领卡并且提交code后,在消费完成后,请求同步到微信卡包接口更新会员信息
function UpdateWxCard(that) {
	wx.request({
		url: addr.Address.UpdateWxCard, //仅为示例，并非真实的接口地址
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId,
			type: 1
		},
		method: "POST",
		header: {
			'content-type': 'application/json' // 默认值
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
	commitFormId: commitFormId,
	GetCardSign: GetCardSign,
	GetWxCardCode: GetWxCardCode,
	SaveWxCardCode: SaveWxCardCode,
	UpdateWxCard: UpdateWxCard,
	getSaveMoneySetUser: getSaveMoneySetUser,
	GetVipInfo: GetVipInfo,
	GetAgentConfigInfo: GetAgentConfigInfo,
	inite2: inite2,
	formatTime: formatTime,
	log: log,
	isFunction: isFunction,
	objToStr: objToStr,
	isOptStrNull: isOptStrNull,
	compareDateFormat: compareDateFormat,
	compareDateFormatstr: compareDateFormatstr,
	compareTime: compareTime,
	extendObject: extendObject,
	timeDiff: timeDiff,


	hideLoadingDialog: hideLoadingDialog,
	showLoadingDialog: showLoadingDialog,
	showNavigationBarLoading: showNavigationBarLoading,
	hideNavigationBarLoading: hideNavigationBarLoading,
	showModal: showModal,
	showToast: showToast,
	Remind: Remind,
	cutstr: cutstr,

	// API
	chooseImage: chooseImage,
	previewImage: previewImage,
	makePhoneCall: makePhoneCall,
	jsonReplaceToJSONString: jsonReplaceToJSONString,
	JSONStringReplaceToJson: JSONStringReplaceToJson,
	wxPayRequst: wxPayRequst,
	AddOrder: AddOrder,
	PayOrder: PayOrder,
}
