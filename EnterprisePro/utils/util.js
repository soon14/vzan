import Promise from './es6-promise.min.js';
// var crypt = require("./crypt.js");
var addr = require("addr");

// 点击放大
function preViewShow(current, urls) {
	wx.previewImage({
		current: current,
		urls: urls,
	})
}
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
//动态改顶部兰标题
function navBarTitle(tmpTitle) {
	wx.setNavigationBarTitle({
		title: tmpTitle,
		success: function () {

		},
		complete: function () {

		}
	});
}
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
function formatTime(unixtime) {
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

//时间戳
function ChangeDateFormat(val) {
	if (val != null) {
		var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", ""), 10));
		//月份为0-11，所以+1，月份 小时，分，秒小于10时补个0
		var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
		var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
		var hour = date.getHours() < 10 ? "0" + date.getHours() : date.getHours();
		var minute = date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
		var second = date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds();
		var dd = date.getFullYear() + "-" + month + "-" + currentDate + " " + hour + ":" + minute + ":" + second;
		// console.log(dd)
		return dd;
	}
	return "";
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


function resizeimg(imgurl, width, height) {
	if (imgurl == null || imgurl == undefined || imgurl == "")
		return "";
	if (imgurl.indexOf("//i.vzan.cc/") > -1 && imgurl.indexOf("?x-oss-process") < 0) {
		if (!width) {
			imgurl += "?x-oss-process=image/resize,limit_0,m_fill,h_" + height + "/format,";
		}
		if (!height) {
			imgurl += "?x-oss-process=image/resize,limit_0,m_fill,w_" + width + "/format,";
		}
		if (width > 0 && height > 0) {
			imgurl += "?x-oss-process=image/resize,limit_0,m_fill,w_" + width + ",h_" + height + "/format,";
		}
		return imgurl += "gif";
	}
	else {
		return imgurl;
	}
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
			// 'content-type': 'application/json',
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
function AddOrder(param, refun, reparam) {
	var that = this
	wx.showNavigationBarLoading()
	wx.showToast({
		title: '加载中...',
		icon: 'loading',
		duration: 10000
	})
	//生成订单
	wxPayRequst(param, {
		failed: function (res) {
			setTimeout(function () {
				wx.hideToast()
			}, 500)
			console.log("生成订单失败")
			wx.hideNavigationBarLoading()
			refun(reparam, 0)
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
function PayOrder(param, pay_callback) {
	var that = this
	wx.request({
		url: addr.Address.PayOrder,
		data: {
			openId: param.openId,
			orderid: param.orderid,
			aid: wx.getStorageSync('aid'),
			'type': param.type,
		},
		method: 'POST',
		header: {
			'content-type': 'application/x-www-form-urlencoded',
		},
		success: function (res) {
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
					},
				})
			} else {
				pay_callback.failed("failed")
			}
		},
	})
}
/* 支付   */
function wxpay(param, callback) {
	var taht = this
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
			delete callback.success;
		},
		fail: function (res) {
			callback.failed("failed");
			delete callback.failed;
		},
		complete: function (res) {
			if (("failed" in callback) && res.errMsg == 'requestPayment:cancel') { //支付取消
				// wx.showModal({
				//   title: '提示',
				//   content: '您取消了支付该订单！',
				//   showCancel:false,
				//   success: function (res) {
				//     if (res.errMsg) {

				//     }
				//   }
				// })
			}

			// if (res.errMsg == 'requestPayment:ok') { //成功
			//   wx.redirectTo({
			//     url: '../orderDetail/orderDetail?orderId=' + getApp().globalData.dbOrder + "&isIndex1=" + getApp().globalData.isIndex1,
			//   })
			// }
			// console.log("pay complete")
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
function ShowPath(path) {
	wx.showModal({
		title: '页面路径',
		content: path,
		cancelText: "取消",
		confirmText: "复制",
		success: function (e) {
			if (e.confirm) {
				wx.setClipboardData({
					data: path,
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
		}
	})
}

// 主题颜色改变
var skinList = [
	{ name: "蓝色", type: "skin_blue", color: "#ffffff", bgcolor: "#218CD7", sel: true },
	{ name: "粉色", type: "skin_pink", color: "#ffffff", bgcolor: "#FF5A9B", sel: false },
	{ name: "绿色", type: "skin_green", color: "#ffffff", bgcolor: "#1ACC8E", sel: false },
	{ name: "红色", type: "skin_red", color: "#ffffff", bgcolor: "#fe525f", sel: false },
	{ name: "白色", type: "skin_white", color: "#000000", bgcolor: "#ffffff", sel: false },
	{ name: "黑色", type: "skin_black1", color: "#ffffff", bgcolor: "#3a393f", sel: false },
	{ name: "红色1", type: "skin_red1", color: "#ffffff", bgcolor: "#f51455", sel: false },
	{ name: "红色2", type: "skin_red2", color: "#ffffff", bgcolor: "#e7475e", sel: false },
	{ name: "红色3", type: "skin_red3", color: "#ffffff", bgcolor: "#f65676", sel: false },

	{ name: "橙色1", type: "skin_orange1", color: "#ffffff", bgcolor: "#f7ad0a", sel: false },
	{ name: "橙色2", type: "skin_orange2", color: "#ffffff", bgcolor: "#f79d2d", sel: false },
	{ name: "橙色3", type: "skin_orange3", color: "#ffffff", bgcolor: "#f9c134", sel: false },
	{ name: "橙色4", type: "skin_orange4", color: "#ffffff", bgcolor: "#f78500", sel: false },
	{ name: "橙色5", type: "skin_orange5", color: "#ffffff", bgcolor: "#ef7030", sel: false },
	{ name: "橙色6", type: "skin_orange6", color: "#ffffff", bgcolor: "#f05945", sel: false },

	{ name: "绿色1", type: "skin_green1", color: "#ffffff", bgcolor: "#99cd4e", sel: false },
	{ name: "绿色2", type: "skin_green2", color: "#ffffff", bgcolor: "#7dc24b", sel: false },
	{ name: "绿色3", type: "skin_green3", color: "#ffffff", bgcolor: "#31b96e", sel: false },
	{ name: "紫色1", type: "skin_purple1", color: "#ffffff", bgcolor: "#6c49b8", sel: false },
	{ name: "紫色2", type: "skin_purple2", color: "#ffffff", bgcolor: "#86269b", sel: false },
	{ name: "蓝色1", type: "skin_blue1", color: "#ffffff", bgcolor: "#4472ca", sel: false },
	{ name: "蓝色2", type: "skin_blue2", color: "#ffffff", bgcolor: "#5e7ce2", sel: false },
	{ name: "蓝色3", type: "skin_blue3", color: "#ffffff", bgcolor: "#1098f7", sel: false },
	{ name: "蓝色4", type: "skin_blue4", color: "#ffffff", bgcolor: "#558ad8", sel: false },
	{ name: "蓝色5", type: "skin_blue5", color: "#ffffff", bgcolor: "#2a93d4", sel: false }
];

function setPageSkin(fpage) {
	var pages = getApp().globalData.pages;
	if (!pages) {
		pages = wx.getStorageSync("PageSetting") || "";
		if (pages) {
			pages = pages.msg.pages;
			if (typeof pages == "string") {
				pages = JSON.parse(pages);
			}
		}
	}
	var skinIndex = 0;
	if (pages && pages.length > 0) {
		skinIndex = pages[0].skin;
	}
	wx.setNavigationBarColor({
		frontColor: skinList[skinIndex].color,
		backgroundColor: skinList[skinIndex].bgcolor,
	})
	fpage.setData({
		currentSkin: skinList[skinIndex].type
	});
}

// 获取默认地址
function GetUserWxAddress(that) {
	wx.request({
		url: addr.Address.GetUserWxAddress, //仅为示例，并非真实的接口地址
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId
		},
		method: "GET",
		header: {
			'content-type': 'application/json' // 默认值
		},
		success: function (res) {
			if (res.data.isok) {
				var WxAddress = res.data.obj.WxAddress.WxAddress
				if (WxAddress.length != 0) {
					WxAddress = JSON.parse(WxAddress)
					that.setData({ Address: WxAddress })
				}
			}
		},
		fail: function () {
			wx.showToast({
				title: '默认地址出错',
			})
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
			console.log('提交备用formid出错')
		}
	})
}
// 获取小程序店铺卡套
function GetCardSign(that) {
	wx.request({
		url: addr.Address.GetCardSign, //仅为示例，并非真实的接口地址
		data: {
			appid: getApp().globalData.appid,
			userid: getApp().globalData.userInfo.UserId
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
			userid: getApp().globalData.userInfo.UserId
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
			code: code
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
		},
		method: "POST",
		header: {
			'content-type': 'application/json' // 默认值
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

////////////////////////////////start 新支付，统一调用该方法
function AddOrderNew(param, refun, reparam) {
	var that = this
	wx.showNavigationBarLoading()
	wx.showToast({
		title: '加载中...',
		icon: 'loading',
		duration: 10000
	})
	//生成订单
	wxPayRequstNew(param, {
		failed: function (res) {
			console.log("生成订单失败")

			wx.hideNavigationBarLoading()
			refun(res, 0)
		},
		success: function (res, oradid) {
			console.log(res)
			if (res == "wxpay") {
				//发起支付
				wx.hideNavigationBarLoading()
				setTimeout(function () {
					wx.hideToast()
				}, 100)
			}
			else if (res == "success") {
				refun(res, oradid)
			}

		},
		failed: function (res) {
			if (res == "failed") {
				refun(res)
			}
		}
	})
}

//微信支付统
function wxPayRequstNew(param, callback) {
	var that = this
	var url = addr.Address.AddPayOrderNew
	wx.request({
		url: url,
		data: param,
		method: 'POST',
		header: {
			'content-type': 'application/x-www-form-urlencoded',
		},
		success: function (res) {
			if (res.data.result) {
				var oradid = res.data.obj
				var newparam = {
					openId: param.openid,
					orderid: oradid,
					'type': 1,
				}
				//储值支付
				if (param.paytype == 1) {
					if (oradid > 0) {
						callback.success("success", oradid)
					}
					else {
						wx.showModal({
							title: '提示',
							content: res.data.msg,
							showCancel: false
						})
						callback.failed("failed")
					}
				}
				//微信支付
				else {
					PayOrderNew(oradid, newparam, {
						failed: function () {
							callback.failed("failed")
						},
						success: function (res) {
							callback.success(res, oradid)
						}
					})
				}

			} else {
				wx.showModal({
					title: '提示',
					content: res.data.msg,
					showCancel: false
				})
				callback.failed("failed")
			}
		}
	})

}

function PayOrderNew(orderid, param, pay_callback) {
	var that = this
	wx.request({
		url: addr.Address.PayOrderNew,
		data: param,
		method: 'POST',
		header: {
			'content-type': 'application/x-www-form-urlencoded',
		},
		success: function (res) {
			console.log(res.data.obj)
			if (res.data.result) {
				var obj = res.data.obj
				var jsObj = JSON.parse(obj)
				//发起支付
				pay_callback.success("wxpay")
				wxpay(jsObj, {
					failed: function () {
						pay_callback.failed("failed")
					},
					success: function (res) {
						pay_callback.success("success")
					}
				})
			} else {
				pay_callback.failed("failed")
			}
		}
	})
}
// 判断版本库
function getSystem() {
	let ver1 = parseFloat(wx.getSystemInfoSync().SDKVersion)
	let ver2 = 1.5
	if (ver1 < ver2 || wx.getSystemInfoSync().SDKVersion == undefined) {
		wx.showModal({
			title: '提示',
			content: '当前微信版本过低，无法使用该功能，请升级到最新微信版本后重试',
			showCancel: false,
			success(res) {
				if (res.confirm) {
					wx.redirectTo({
						url: '/pages/errorpage/errorpage',
					})
				}
			}
		})
		return;
	}
}
module.exports = {
	UpdateWxCard: UpdateWxCard,
	SaveWxCardCode: SaveWxCardCode,
	GetWxCardCode: GetWxCardCode,
	GetCardSign: GetCardSign,
	GetUserWxAddress: GetUserWxAddress,
	commitFormId: commitFormId,
	formatTime: formatTime,
	isFunction: isFunction,
	objToStr: objToStr,
	isOptStrNull: isOptStrNull,
	compareDateFormat: compareDateFormat,
	compareDateFormatstr: compareDateFormatstr,
	compareTime: compareTime,
	extendObject: extendObject,
	timeDiff: timeDiff,
	resizeimg: resizeimg,
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
	ShowPath: ShowPath,
	ChangeDateFormat,
	PayOrder: PayOrder,
	navBarTitle,
	commitFormId,
	AddOrderNew,
	copy,
	preViewShow,
	ChangeDateFormatNew,
	getSystem,
	setPageSkin,
}
