
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
    //调用API从本地缓存中获取数据
    var that = this;

    //第三方平台配置
    var exconfig = wx.getExtConfigSync()
    if (exconfig != undefined) {
      that.globalData.appid = exconfig.appid
      that.globalData.areaCode = exconfig.areaCode
    }
    else {
      console.log("exconfig", exconfig);
      return;
    }
    this.globalData.unreadmsg = wx.getStorageSync("unreadmsg") || {};
    var unreadmsgcount = 0;
    for (var key in this.globalData.unreadmsg) {
      unreadmsgcount += this.globalData.unreadmsg[key];
    }
    this.globalData.unreadmsgcount = unreadmsgcount;
    wx.setStorageSync("unreadmsgcount", unreadmsgcount)
    if (this.globalData.unreadmsgcount > 0) {
      wx.showTabBarRedDot({
        index: 2,
      })
    }
  },

  globalData: {
    // 客户上传手机号码准备
    isgetTel: 0,
    telEncryptedData: 0,
    telIv: 0,
    orderid: 0,//订单id 用于取消预约支付cancelpay接口
    levelid: 0,
    priceT: 0,//预约价格 应用于pay->paysuccess

    /**im begin**/
    ws: false,//websocket链接
    msgQueue: [],
    unreadmsg: {},
    unreadmsgcount: 0,
    /**img end**/
  },
  WeToast, // 后面可以通过app.WeToast访问
  imgresouces,
  C_Enum,

  getUserInfo: function (cb) {
    console.log('getUserInfo执行')
    var that = this
    wx.showLoading({
      title: '加载中'
    })
    var _userInfo = wx.getStorageSync('userInfo')
    if (_userInfo) {
      that.globalData.userInfo = _userInfo;
      that.globalData.fuserInfo = {
        userid: _userInfo.UserId,
        nickname: _userInfo.nickName,
        headimg: _userInfo.avatarUrl
      };
      console.log('getUserInfo执行回调')
      typeof cb == "function" && cb(_userInfo)
      that.connectSocket();
      wx.hideLoading()
    } else {
      //调用登录接口
      wx.login({
        success: function (res) {
          that.login(res.code, cb);

        },
      })
    }
  },
  login: function (code, cb) {
    wx.showLoading({
      title: '加载中',
      icon: 'loading',
    })
    let that = this;
    let appid = that.globalData.appid
    wx.request({
      url: addr.Address.WxLogin,
      data: {
        code,
        appid,
        needappsr: 0,
      },
      method: "POST",
      header: {
        "content-type": "application/x-www-form-urlencoded"
      }, // 设置请求的 header
      success: function (data) {
        if (data.data.isok) {
          var json = data.data.dataObj
          that.globalData.userInfo = {
            UserId: json.Id,
            avatarUrl: json.HeadImgUrl,
            nickName: json.NickName,
            openId: json.OpenId,
            sessionId: json.loginSessionKey,
            unionId: json.UnionId,
          };
          that.globalData.fuserInfo = {
            userid: json.Id,
            nickname: json.NickName,
            headimg: json.HeadImgUrl
          };
          wx.setStorage({
            key: "userInfo",
            data: that.globalData.userInfo
          })
          wx.hideLoading()
          typeof cb == "function" && cb(that.globalData.userInfo)
        } else {
          wx.hideLoading()
          wx.showModal({
            title: '提示',
            content: data.data.Msg,
          })
        }
        that.connectSocket();
      },
      fail: function (data) {
        console.log(data)
        wx.hideLoading()
      },
      complete: function (data) {
        console.log(data)
        wx.hideLoading()
      }
    })
  },
  
  login_old: function (vm, cb){
    wx.showLoading({
      title: '加载中',
      icon: 'loading',
    })
    let that = this;
    let appid = that.globalData.appid
    wx.clearStorageSync("userInfo")
    wx.request({
      url: addr.Address.loginByThirdPlatform,
      data: {
        appid,
        iv: vm.iv,
        code: vm.code,
        data: vm.data,
        signature: vm.sign,
        isphonedata: vm.phone
      },
      method: "POST",
      header: {
        "content-type": "application/x-www-form-urlencoded"
      }, // 设置请求的 header
      success: function (data) {
        if (data.data.result) {
          var json = data.data.obj
          that.globalData.userInfo = {
            UserId: json.userid,
            avatarUrl: json.avatarUrl,
            nickName: json.nickName,
            openId: json.openId,
            sessionId: json.sessionId,
            unionId: json.unionId,
          };
          that.globalData.fuserInfo = {
            userid: json.userid,
            nickname: json.nickName,
            headimg: json.avatarUrl
          };
          wx.setStorage({
            key: "userInfo",
            data: that.globalData.userInfo
          })
          wx.hideLoading()
          typeof cb == "function" && cb(that.globalData.userInfo)
        } else {
          wx.hideLoading()
          wx.showModal({
            title: '提示',
            content: data.data.msg,
          })
        }
        that.connectSocket();
      },
      fail: function (data) {
        wx.hideLoading()
      },
      complete: function (data) {
        wx.hideLoading()
      }
    })
  },

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
      //url: 'ws://47.93.7.128:9527/?appId=' + appid + '&userId=' + fuserid + '&fuserType=0',
      url: 'wss://dzwss.xiaochengxu.com.cn/?appId=' + appid + '&userId=' + fuserid + '&fuserType=0',

      //url: 'wss://dzwss.vzan.com:9527/?appId=' + appid + '&userId=' + fuserid + '&fuserType=0',
      header: {
        'content-type': 'application/json'
      },
      method: "GET"
    });
    console.log("ws connecting...");

    wx.onSocketOpen(function (res) {
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
      //联系人页面
      else if (currentPage.route == "pages/im/contact") {
        that.markUnreadMsg(msg);
      }
      else {

        wx.showTabBarRedDot({
          index: 2,
        })
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

    wx.showTabBarRedDot({
      index: 2,
    })
  },
})