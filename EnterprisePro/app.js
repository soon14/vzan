//app.js
var aldstat = require("./utils/ald-stat/ald-stat.js");
const util = require("./utils/util.js");
const tools = require("./utils/tools.js");
const http = require("./utils/http.js");
const addr = require("./utils/addr.js");



/**im begin**/
import { core } from './utils/core';
let _get = require('./utils/lodash.get');
let reConnectTimer = null;
let isConnecting = false;//ws是否正在连接中
let isFirst = true;
/**im end**/

App({
	scene: "",
	fromAppid: "",
	onLaunch: function (options) {
		var that = this;
		var exconfig = wx.getExtConfigSync()  //第三方平台配置
		util.getSystem()
		if (exconfig != undefined) {
			that.globalData.appid = exconfig.appid
			that.globalData.areaCode = exconfig.areaCode
		}
		that.GetStoreConfig();
		// tools.GetDadaFreight({
		//   cityname:"",
		//   appid:"",
		//   openid:"",
		//   lat:0,
		//   lnt:0,
		//   acceptername:"",
		//   accepterphone:"",
		//   address:""
		// });
	},
	// 推出小程序停止播放
	onHide: function () {
		wx.stopBackgroundAudio()
	},

	globalData: {
		// 预约购物
		appointMsg: {
			datatime: '选择预约日期',
			time: '选择预约时间',
			numsindex: 0,
			msg: '',
			name: '',
			phonenumber: '',
		},
		appoint_numsArray: ['选择预约人数'],
		appoint_Id: 0,//预约id
		appoint_paynow: 3,//预约购物 扫码付款0 预约付款1
		appoint_shopcartlist: [],
		appoint_shopcartlength: 0,
		appoint_alldiscountprice: 0,
		appoint_goodslist: [],
		//预约购物 end
		storecodeid:0,//扫码购物id
		reductionCart: [],//立减金集合
		getWayinfo: {
			name: "",
			phone: "",
		},//上门自取参数
		isFirstPickTime: 0,
		IsDistribution: false, //判断购买普通商品时是否需要加传fromAppid和DistributionMoney
		DistributionMoney: 0,
		// 客户上传手机号码准备
		isgetTel: 0,
		telEncryptedData: 0,
		telIv: 0,
		userInfo: { IsValidTelePhone: 0, TelePhone: "未绑定" },
		pages: '',
		session_key: '', // 微信配置信息
		isIndex1: 0,//控制页面元素下标 
		openid: '',
		user: [],
		vipInfo: {},
		_bgFirst: true,
		storeConfig: "",
		defaultAddress: [],
		shippingAddress: [],  //用户选择的收货地址
		addressLength: 0,    //用户有默认的选择地址


		/**im begin**/
		ws: false,//websocket链接
		msgQueue: [],
		unreadmsg: {},
		unreadmsgcount: 0,
		/**img end**/
	},

	getUserInfo: function (cb) {
		let that = this
		let _userInfo = wx.getStorageSync('userInfo')
		//调用登录接口
		wx.login({
			success: function (res) {
				if (getApp().globalData.isgetTel == 0) { //如果isgetTel为0则正常登录，否则执行else 上传手机号码
					wx.getUserInfo({
						withCredentials: true,
						success: function (data) {
							if (_userInfo && _userInfo.openId && that.globalData.isgetTel == 0) {
								that.globalData.userInfo = _userInfo;

								//im begin
								try {
									that.globalData.fuserInfo = {
										userid: _userInfo.UserId,
										nickname: _userInfo.nickName,
										headimg: _userInfo.avatarUrl
									};
									that.connectSocket();
								}
								catch (ex) {
									console.log(ex);
								}
								//im end




								typeof cb == "function" && cb(_userInfo)
							}
							else {
								that.login(res.code, data.encryptedData, data.signature, data.iv, cb);
							}
						},
						fail: function () {
							typeof cb == "function" && cb(_userInfo)
						},
						complete: function () {

						}
					})
				}
				else {
					if (that.globalData.telEncryptedData == undefined || that.globalData.telIv == undefined) {
						return
					} else {
						that.login(res.code, that.globalData.telEncryptedData, that.globalData.userInfo.UserId, that.globalData.telIv, cb, 1)
					}
				}
			},
			fail: function (res) {
				wx.showModal({
					title: '提示',
					content: res.errMsg,
				})
			}
		})
	},
	//登录
	login: function (code, encryptedData, signature, iv, cb, isphonedata) {
		wx.showToast({
			title: '加载中',
			icon: 'loading',
			duration: 10000
		})
		var that = this;
		var currentData = this.data
		wx.request({
			url: addr.Address.loginByThirdPlatform,
			data: {
				code: code,
				data: encryptedData,
				signature: signature,
				iv: iv,
				appid: that.globalData.appid,
				isphonedata: isphonedata
			},
			method: "Get",
			success: function (data) {
				if (data.data.result) {
					var json = data.data.obj
					that.globalData.userInfo = {
						nickName: json.nickName,
						avatarUrl: json.avatarUrl,
						openId: json.openId,
						UserId: json.userid,
						city: json.city,
						country: json.country,
						gender: json.gender,
						language: json.language,
						iscityowner: json.iscityowner,
						province: json.province,
						sessionId: json.sessionId,
						unionId: json.unionId,
						TelePhone: (json.tel == null || json.tel == '' ? "未绑定" : json.tel),
						IsValidTelePhone: json.IsValidTelePhone,
					};
					wx.setStorage({
						key: "userInfo",
						data: that.globalData.userInfo
					})
					wx.hideToast()
					typeof cb == "function" && cb(that.globalData.userInfo)
				} else {
					wx.hideToast()
				}
			},
			fail: function (data) {
				wx.showToast({
					title: data.msg,
				})
			},
			complete: function (data) {
			}
		})
	},
	//获取店铺配置
	GetStoreConfig: function (callback) {

		var that = this
		// if (that.globalData.storeConfig) {
		//   callback(that.globalData.storeConfig);
		//   return;
		// }
		http.getAsync(addr.Address.GetStoreConfig, { appid: this.globalData.appid })
			.then(function (res) {
				if (res.isok) {
					that.globalData.canSaveMoneyFunction = res.postdata.store.funJoinModel.canSaveMoneyFunction;
					that.globalData.storeConfig = res.postdata.store;//funJoinModel
					if (callback) {
						callback(that.globalData.storeConfig);
					}
				}
			});
	},

	//im begin
	connectSocket: function () {



		var that = this;
		var globaldata = that.globalData;
		var appid = _get(globaldata, "appid") || "";
		var fuserid = _get(globaldata, "userInfo.UserId") || ""
		if (appid == "" || fuserid == "")
			return;
		if (globaldata.ws || isConnecting)
			return;
		isConnecting = true;

    wx.connectSocket({
      //fuserType：用户身份  0：普通用户 2：商家
      url: 'wss://dzwss.xiaochengxu.com.cn/?appId=' + appid + '&userId=' + fuserid + '&isFirst=' + isFirst,
      // url: 'ws://47.93.100.78:9528/?appId=' + appid + '&userId=' + fuserid + '&isFirst=' + isFirst,
      header: {
        'content-type': 'application/json'
      },
      method: "GET"
    });
    console.log("ws connecting...");

		wx.onSocketOpen(function (res) {
			console.log('ws is open', res);
			// if (isFirst) {
			//   setTimeout(function () {
			//     that.GetStoreConfig(function (config) {
			//       let kefuinfo = config.kfInfo;
			//       if (kefuinfo) {
			//         if (config && config.funJoinModel && config.funJoinModel.sayHello && kefuinfo.uid != fuserid) {
			//           wx.showModal({
			//             title: '提示',
			//             content: '您有1条未读消息',
			//             cancelText: "知道了",
			//             confirmText: "去看看",
			//             success: function (res) {
			//               if (res.confirm) {
			//                 tools.gochat();
			//               }
			//             }
			//           })
			//         }
			//       }

			//     });
			//   }, 2000)
			// }


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
			isFirst = false;
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
			//联系人页面
			else if (currentPage.route == "pages/im/contact") {
				that.markUnreadMsg(msg);
			}
			else {

				// wx.showTabBarRedDot({
				//   index: 2,
				// })
				var key = msg.fuserId + "_" + msg.tuserId + "_" + msg.tuserType //技师给用户发的
				var currentUnreadmsgcount = _get(that.globalData.unreadmsg, key, 0);
				currentUnreadmsgcount += 1;
				that.globalData.unreadmsg[key] = currentUnreadmsgcount;

				var unreadmsgcount = 0;
				for (var item in that.globalData.unreadmsg) {
					unreadmsgcount += that.globalData.unreadmsg[item]
				}

				core.changeunreadmsg(that.globalData.unreadmsg, that.globalData.unreadmsgcount);
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
		if (currentPage != null) {
			var list = currentPage.data.vm.list;
			//查找给我发消息的那个人
			var contactIndex = list.findIndex(function (obj) {
				return msg.fuserId == obj.tuserId;
			});
			if (contactIndex != -1) {
				list[contactIndex].message = {
					msgType: msg.msgType,
					msg: msg.msgType == 1 ? "[图片]" : msg.msg,
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
				currentPage.setData({
					"vm.list": list
				});
			}
		}

		var key = msg.fuserId + "_" + msg.tuserId + "_" + msg.tuserType //技师给用户发的
		var currentUnreadmsgcount = _get(that.globalData.unreadmsg, key, 0);
		currentUnreadmsgcount += 1;
		that.globalData.unreadmsg[key] = currentUnreadmsgcount;

		var unreadmsgcount = 0;
		for (var item in that.globalData.unreadmsg) {
			unreadmsgcount += that.globalData.unreadmsg[item]
		}


		core.changeunreadmsg(that.globalData.unreadmsg, unreadmsgcount);

		// wx.showTabBarRedDot({
		//   index: 2,
		// })
	},
	//im end



})