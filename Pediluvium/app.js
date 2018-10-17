//app.js
var aldstat = require("./utils/san.js");
var util = require("./utils/util.js");
var api = require("./utils/network.js");
var addr = require("./utils/addr.js");
var imgresouces = require("./utils/imgresouces.js");
var C_Enum = require("./public/C_Enum.js");
let { WeToast } = require('src/wetoast.js')    // 返回构造函数，变量名可自定义

/**im begin**/
import { core } from './utils/core';
let _get = require('./utils/lodash.get');
let reConnectTimer = null;
let isConnecting = false;//ws是否正在连接中
/**im end**/

App({
	onLaunch: function () {
		var that = this;
		var exconfig = wx.getExtConfigSync()  //第三方平台配置
		if (exconfig != undefined) {
			that.globalData.appid = exconfig.appid
			that.globalData.areaCode = exconfig.areaCode
		}
		this.globalData.unreadmsg = wx.getStorageSync("unreadmsg") || {};
		var unreadmsgcount = 0;
		for (var key in this.globalData.unreadmsg) {
			unreadmsgcount += this.globalData.unreadmsg[key];
		}
		this.globalData.unreadmsgcount = unreadmsgcount;
		wx.setStorageSync("unreadmsgcount", unreadmsgcount)

	},

	globalData: {
		technicianId: 0,// 技师id
		storeId: 0,//店铺id
		userInfo: { IsValidTelePhone: 0, TelePhone: "未绑定" },
		isLogin: false,
		citysubid: 0,

		// 微信配置信息
		session_key: '',
		openid: '',
		appsr: "",
		appid: "",
		areaCode: 110228,

		// 配置
		Host: 'https://txiaowei.vzan.com/',
		isDebug: false,
		pageSize: 10, //

		// content:name,     
		// 微赞平台信息
		uid: '',
		platformUserInfo: '',
		cityname: '',

		/**im begin**/
		ws: false,//websocket链接
		msgQueue: [],
		unreadmsg: {},//key=fuserid_tuserid,value=count
		unreadmsgcount: 0,
		/**img end**/
	},
	WeToast, // 后面可以通过app.WeToast访问
	imgresouces,
	C_Enum,

	onHide: function () {//退出小程序终止背景音乐
		wx.stopBackgroundAudio()
	},
	getUserInfo: function (cb) {
		console.log('getUserInfo执行')
		var that = this
		var _userInfo = wx.getStorageSync('userInfo')
		if (_userInfo && _userInfo.openId && _userInfo.TelePhone != null && _userInfo.TelePhone != '未绑定') {
			that.globalData.userInfo = _userInfo;
			console.log('getUserInfo执行回调')
			var phone = _get(that.globalData, "userInfo.TelePhone", "");
			var appid = _get(that.globalData, "appid", "");
			if (phone != "" && appid != "") {
				core.GetTechInfo({ telePhoneNumber: phone, appid: appid, }).then(function (res) {
					if (res) {
						that.globalData.fuserInfo = res;
						that.connectSocket();
						typeof cb == "function" && cb(_userInfo)
					}
				});
			}
			else {
				typeof cb == "function" && cb(_userInfo)
			}
			wx.hideToast()
		} else {
			//调用登录接口
			wx.login({
				success: function (res) {
					that.WxLogin(res.code, cb)
					// console.log('login res.code ' + res.code)
					// wx.getUserInfo({
					// 	withCredentials: true,
					// 	success: function (data) {
					// 		console.log('getUserInfo登录成功')
					// 		that.login(res.code, data.encryptedData, data.signature, data.iv, cb);
					// 	},
					// 	fail: function (data) {

					// 		wx.hideLoading()
					// 		wx.showModal({
					// 			title: '提示',
					// 			showCancel: false,
					// 			confirmText: '确定',
					// 			content: '授权后，才能继续使用。',
					// 			success: function (res) {

					// 				if (res.confirm) {
					// 					console.log('用户点击确定')
					// 					wx.openSetting({
					// 						success: function (setres) {

					// 						}
					// 					});
					// 				}
					// 			}
					// 		})
					// 	},
					// 	complete: function () {
					// 		console.log('getUserInfo执行complete')
					// 	}
					// })
				},
			})
		}
	},

	//新版第三方登录
	WxLogin: function (code, cb) {
		wx.showToast({
			title: '正在登录....',
			icon: 'loading',
			duration: 10000
		})
		var that = this;
		wx.request({
			url: addr.Address.WxLogin,
			data: {
				code: code,
				appid: getApp().globalData.appid,
				needappsr: 0,
			},
			header: {
				'content-type': 'application/x-www-form-urlencoded'
			},
			method: "POST",
			success: function (data) {
				console.log(data)
				var obj = data.data
				if (obj.isok) {
					wx.startPullDownRefresh()
					obj.dataObj.UserId = obj.dataObj.Id
					obj.dataObj.openId = obj.dataObj.OpenId
					obj.dataObj.nickName = obj.dataObj.NickName
					obj.dataObj.avatarUrl = obj.dataObj.HeadImgUrl
					that.globalData.userInfo = obj.dataObj
					wx.setStorageSync("userInfo", obj.dataObj)
					typeof cb == "function" && cb(that.globalData.userInfo)
				} else {
					wx.showModal({
						title: '提示',
						content: obj.Msg,
						showCancel: false,
					})
				} wx.hideToast()
			},
			fail: function (data) {
				console.log(obj)
			},
		})
	},

	//登录
	login: function (code, encryptedData, signature, iv, cb) {
		wx.showToast({
			title: '加载中',
			icon: 'loading',
			duration: 10000
		})
		console.log(code)
		console.log(encryptedData)
		console.log(signature)
		console.log(iv)
		var that = this;
		// var currentData = this.data
		console.log('准备调用loginByThirdPlatform接口')
		wx.request({
			url: addr.Address.loginByThirdPlatform,
			data: {
				code: code,
				data: encryptedData,
				signature: signature,
				iv: iv,
				appid: that.globalData.appid,
				needappsr: 0
			},
			method: "Get",
			success: function (data) {
				if (data.data.result) {
					var json = data.data.obj
					console.log('loginByThirdPlatform接口返回 unionId - ' + json.unionId)
					that.globalData.userInfo = {
						UserId: json.userid,
						avatarUrl: json.avatarUrl,
						city: json.city,
						country: json.country,
						gender: json.gender,
						language: json.language,
						nickName: json.nickName,
						// nickName:'我修改的名字',s
						openId: json.openId,
						province: json.province,
						sessionId: json.sessionId,
						unionId: json.unionId,
						TelePhone: (json.tel == null || json.tel == '' ? "未绑定" : json.tel),

						IsValidTelePhone: json.IsValidTelePhone,
						//unioId: 'oW2wBwTJhtXdeOgB7S9nuvEJoDls',
						//unionId: 'oW2wBwWwbBttylGPQdRUW46yc3UY',
						// unionId: "oW2wBwUp5QndTGk3yCs1iYfQXYsQ",
					};
					if (!json.tel) {
						wx.redirectTo({
							url: '/pages/index/index',
						})
						return;
					}

					console.log('json = ' + json);
					wx.setStorage({
						key: "userInfo",
						data: that.globalData.userInfo
					})
					wx.hideToast()
					// currentData.header.title = that.globalData.userInfo.nickName
					// currentData.header.title = '我是名字'                
					// currentData.header.image = that.globalData.userInfo.avatarUrl
					// that.setData(currentData)
					// that.storeListRequest(that.globalData.userInfo.unionId, 1)

					var phone = _get(that.globalData, "userInfo.TelePhone", "");
					var appid = _get(that.globalData, "appid", "");
					core.GetTechInfo({ telePhoneNumber: phone, appid: appid }).then(function (res) {
						if (res) {
							that.globalData.fuserInfo = res;
							that.connectSocket();

						}
						typeof cb == "function" && cb(that.globalData.userInfo)
					});
				} else {
					console.log('登录失败 data - ' + data.data)
					wx.showModal({
						title: '提示',
						content: data.data.msg,
					})
					wx.hideToast()
					// currentData.haveMoreData = false
					// that.setData(currentData)
				}

			},
			fail: function (data) {
				console.log(data)
			},
			complete: function (data) {
				console.log(data)
			}
		})
	},
	getUserInforef: function (cb) {
		wx.getUserInfo({
			withCredentials: true,
			success: function (data) {
				if (data.encryptedData == undefined || data.encryptedData == '') {
					this.getUserInforef(cb)
				}
				else {
					this.login(res.code, data.encryptedData, data.signature, data.iv, cb)
				}
			},
			fail: function () {
				// fail
				wx.showModal({
					title: '提示',
					showCancel: false,
					confirmText: '知道啦',
					content: '授权失败啦!\n请退出后在微信中移除该小程序,再重扫二维码或直接搜索该小程序!',
					success: function (res) {
						if (res.confirm) {
							console.log('用户点击确定')
						}
					}
				})
			},
			complete: function () {
				console.log('getUserInfo执行complete')
				// complete
			}
		})
	},
	pictureTap: function (e) {
		var src = e.currentTarget.dataset.src;
		wx.previewImage({
			urls: [src]
		})
	},
	pictureTaps: function (url, urls) {
		wx.previewImage({
			current: url,
			urls: urls,
		})
	},
	ShowMsg: function (msg) {
		wx.showModal({
			title: '提示',
			content: msg,
			showCancel: false,
		})
	},
	ShowMsgAndUrl: function (msg, url, mtype = 0) {
		var that = this
		wx.showModal({
			title: '提示',
			showCancel: false,
			content: msg,
			success: function (res) {
				if (res.confirm) {
					if (mtype == 0) {
						that.goNewPage(url)
					} else if (mtype == 1) {
						that.goBackPage(1)
					}
				}
			}
		})
	},
	showToast: function (msg) {
		wx.showToast({
			title: msg,
		})
	},
	//刷新页面
	reloadpage: function (param = '', index = 0) {
		var pages = getCurrentPages()
		var coupon = pages[index]
		coupon.onLoad(param)
	},
	//刷新页面
	reloadpagebyurl: function (param = '', url = '') {
		var pages = getCurrentPages()
		if (pages.length > 0) {
			for (var i in pages) {
				var page = pages[i]
				if (page.route == url) {
					page.onLoad(param)
					break
				}
			}
		}
	},
	//手机号码是否通过验证
	checkphone: function (param = '') {
		if (this.globalData.userInfo.IsValidTelePhone == 0) {
			if (param != '1') {
				wx: wx.showModal({
					title: '提示',
					content: '为了保障您数据的安全，请先进行手机号验证',
					success: function (res) {
						if (res.confirm) {
							wx.navigateTo({
								url: '../bind_mobile/bind_mobile',
							})
						}
					},
				})
			}
		}
		else {
			return true
		}
		return false
	},
	//跳转新页面
	goNewPage: function (url) {
		wx.navigateTo({
			url: url,
		})
	},
	//跳转选项卡页
	goBarPage: function (url) {
		wx.switchTab({
			url: url,
		})
	},
	//返回上几页
	goBackPage: function (delta) {
		wx.navigateBack({
			delta: delta
		})
	},

	//IM
	connectSocket: function () {
		var that = this;
		var globaldata = that.globalData;
		var appid = _get(globaldata, "appid");
		var fuserid = _get(globaldata, "fuserInfo.userid");
		if (!appid || !fuserid) {
			console.log("连接失败：", { appid, fuserid });
			return;
		}

		if (globaldata.ws || isConnecting) {
			console.log("连接失败：", { ws: globaldata.ws, isConnecting });
			return;
		}

		isConnecting = true;

		wx.connectSocket({
			//url: 'ws://47.93.7.128:9527/?appId=' + appid + '&userId=' + fuserid + '&fuserType=1',
			url: 'wss://dzwss.xiaochengxu.com.cn/?appId=' + appid + '&userId=' + fuserid + '&fuserType=1',
			//url: 'wss://dzwss.vzan.com:9527/?appId=' + appid + '&userId=' + fuserid + '&fuserType=1',
			header: {
				'content-type': 'application/json'
			},
			method: "GET"
		});
		console.log("ws connecting...");

		wx.onSocketOpen(function (res) {
			wx.hideLoading()
			console.log('WebSocket连接已打开！', res);
			globaldata.ws = true;
			isConnecting = false;
			if (reConnectTimer) {
				clearTimeout(reConnectTimer);
				reConnectTimer = null;
			}
			//重连后，自动重发发送失败的消息
			for (var i = 0; i < that.globalData.msgQueue.length; i++) {
				that.sendMessage(that.globalData.msgQueue[i])
			}
			that.globalData.msgQueue = [];

		});
		wx.onSocketError(function (res) {
			console.log('WebSocket连接打开失败，请检查！', res)
			globaldata.ws = false;
			isConnecting = false;
		});

		wx.onSocketClose(function (res) {
			console.log('WebSocket 已关闭！', res)
			globaldata.ws = false;
			isConnecting = false;
			that.reConnect();
		});
		//接收消息
		wx.onSocketMessage(function (res) {
			console.log('收到服务器内容：' + res.data)
			var msg = res.data;
			if (typeof res.data == "string")
				msg = JSON.parse(res.data);

			//判断当前在哪个页面
			var pages = getCurrentPages();
			var currentPage = pages[pages.length - 1];
			var fuser = currentPage.data.fuserInfo;
			var tuser = currentPage.data.tuserInfo;
			//聊天页面
			if (currentPage.route == "pages/im/chat") {
				var list = currentPage.data.vm.list;
				//如果消息是当前联系人发来的
				if (msg.fuserId == fuser.userid && msg.tuserId == tuser.userid ||//我发的
					msg.fuserId == tuser.userid && msg.tuserId == fuser.userid) {//发给我的

					list.push(msg);
					currentPage.setData({
						"vm.list": list,
						"vm.lastids": msg.ids,
					});
				}
				else {
					that.markUnreadMsg(msg);
				}
			}
			//联系人页面 只有技师才会被动添加联系人，因为他是被动接收用户的消息
			else if (currentPage.route == "pages/im/contact") {

				var list = currentPage.data.vm.list;
				//查找给我发消息的那个人
				var contactIndex = list.findIndex(function (obj) {
					return msg.fuserId == obj.tuserId;
				});
				if (contactIndex == -1) {
					list.unshift({
						"Id": 0,
						"appId": msg.appId,
						"aId": 0,
						"storeId": 0,
						"fuserId": msg.tuserId,//技师ID
						"tuserId": msg.fuserId,//用户ID
						"state": 0,
						"fuserType": 1,
						"extra": "",
						"tuserHeadImg": msg.fheadImg, "tuserNicename": msg.fnickName,
						"message": {
							msgType: msg.msgType,
							msg: msg.msg,
							sendDate: msg.sendDate,
						}
					});
					currentPage.setData({
						"vm.list": list
					});
				}
				that.markUnreadMsg(msg);
			}
			//如果是其他页面，tab显示小红点
			else if (currentPage.route == "pages/Me/Me") {
				wx.showTabBarRedDot({
					index: 3,
				})
				var key = msg.fuserId + "_" + msg.tuserId + "_" + msg.tuserType //用户发给技师的
				var currentUnreadmsgcount = _get(that.globalData.unreadmsg, key, 0);
				currentUnreadmsgcount += 1;
				that.globalData.unreadmsg[key] = currentUnreadmsgcount;

				var unreadmsgcount = 0;
				for (var item in that.globalData.unreadmsg) {
					unreadmsgcount += that.globalData.unreadmsg[item]
				}
				currentPage.setData({
					"unreadmsgcount": unreadmsgcount
				});

				core.changeunreadmsg(that.globalData.unreadmsg, unreadmsgcount);

			}
			else {

				wx.showTabBarRedDot({
					index: 3,
				})
				var key = msg.fuserId + "_" + msg.tuserId + "_" + msg.tuserType //技师给用户发的
				var currentUnreadmsgcount = _get(that.globalData.unreadmsg, key, 0);
				currentUnreadmsgcount += 1;
				that.globalData.unreadmsg[key] = currentUnreadmsgcount;

				var unreadmsgcount = 0;
				for (var item in that.globalData.unreadmsg) {
					unreadmsgcount += that.globalData.unreadmsg[item]
				}
				core.changeunreadmsg(that.globalData.unreadmsg, unreadmsgcount);
			}

		})
	},
	reConnect: function () {
		console.log("开始重连");
		var that = this;
		if (reConnectTimer) {
			clearTimeout(reConnectTimer);
			reConnectTimer = null;
		}
		reConnectTimer = setTimeout(function () {
			that.connectSocket();
		}, 3000);
	},
	//发
	sendMessage: function (msg) {
		if (typeof msg == "object")
			msg = JSON.stringify(msg);
		console.log(msg);
		var that = this;
		var globaldata = that.globalData;
		if (globaldata.ws) {
			wx.sendSocketMessage({
				data: msg
			})
		}
		else {
			that.globalData.msgQueue.push(msg);
		}
	},
	//只标记联系人列表里的未读消息
	markUnreadMsg: function (msg) {
		var that = this;
		var pages = getCurrentPages();
		var currentPage = getCurrentPages().find(p => p.route == "pages/im/contact");
		if (currentPage == undefined)
			return;
		var list = currentPage.data.vm.list;
		//查找给我发消息的那个人
		var contactIndex = list.findIndex(function (obj) {
			return msg.fuserId == obj.tuserId;
		});
		if (contactIndex != -1) {
			if (msg.msgType === 0) {
				list[contactIndex].message = {
					msgType: msg.msgType,
					msg: msg.msg,
					sendDate: msg.sendDate,
				};
				var unreadmsgItem = _get(list[contactIndex], "unreadnum", 0);
				unreadmsgItem += 1;
				list[contactIndex].unreadnum = unreadmsgItem;
				if (unreadmsgItem > 99) {
					list[contactIndex].unreadnum_fmt = "99+";
				}
				else {
					list[contactIndex].unreadnum_fmt = unreadmsgItem;
				}


			}

			currentPage.setData({
				"vm.list": list
			});

			var key = msg.fuserId + "_" + msg.tuserId + "_" + msg.tuserType //技师给用户发的
			var msgcount = _get(that.globalData.unreadmsg, key, 0);
			msgcount += 1;
			that.globalData.unreadmsg[key] = msgcount;
			that.globalData.unreadmsgcount += 1;

			//永久存储
			wx.setStorage({
				key: 'unreadmsgcount',
				data: that.globalData.unreadmsgcount,
			})
			wx.setStorage({
				key: 'unreadmsg',
				data: that.globalData.unreadmsg,
			})
		}
		wx.showTabBarRedDot({
			index: 3,
		})
	},
})