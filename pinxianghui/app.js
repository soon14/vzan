//app.js
var addr = require('/utils/addr.js');
var aldstat = require("./utils/san.js");
import {
  core,
  vm,
  http
} from "./utils/core";
App({
  onLaunch: function() {
    var that = this;
    var exconfig = wx.getExtConfigSync()
    if (exconfig != undefined) {
      that.globalData.appid = exconfig.appid
    }
  },
  //全局变量
  globalData: {
    appid: '',
    userInfo: null,
    aid: 0,
    utoken: "",
    sourcestoreid: 0, //扫码进入的门店id
    sourcetime: 0, //source倒计时 半小时sourcestoreid字段归0
    myAddress: {}, //收货地址
    ziquInfo: {}, //到店自取姓名和名字
    fId: 0, //底部导航栏‘分类’默认fId
    scroll_into: 's_0_0', //控制classify页面header scrollview 体验问题
  },
  login: function(callback) {
    var that = this
    if (that.globalData.userInfo && that.globalData.aid && that.globalData.utoken) {
      wx.hideToast()
      callback(that.globalData);
    } else {
      wx.login({
        success: function(res) {
          that.WxLogin(res.code, function(cb_2) {
            if (cb_2) {
              callback(cb_2);
            }
          });
        }
      })
    }
  },
  //新版第三方登录
  WxLogin: function(code, cb_2) {
    wx.showToast({
      title: '正在登录....',
      icon: 'loading',
      duration: 10000
    })
    var that = this;
    //获取aid
    var req_getAid = http.get(addr.getAid, {
      appid: that.globalData.appid
    });
    //获取userInfo
    var req_login = http.get(addr.WxLogin, {
      code: code,
      appid: that.globalData.appid,
      needappsr: 0,
      storeId: 0
    });
    Promise.all([req_getAid, req_login]).then(function(results) {
      //保存aid
      if (results[0].code == 1)
        that.globalData.aid = results[0].obj;

      if (results[1].code == 1) {
        var t = results[1].dataObj;
        //保存userInfo
        that.globalData.userInfo = {
          UserId: t.Id,
          openId: t.OpenId,
          nickName: t.NickName,
          avatarUrl: t.HeadImgUrl,
          isNewUser: t.HeadImgUrl ? false : true,
          IsValidTelePhone: t.IsValidTelePhone,
          StoreId: t.StoreId
        }
        //保存utoken
        that.globalData.utoken = t.loginSessionKey;
      } else {
        wx.showModal({
          title: '提示',
          content: results[1].Msg,
          showCancel: false,
        })
      }
      cb_2 && cb_2(that.globalData);
      wx.hideToast();
    });
  },
  //微信登录
  loginByThirdPlatform: function(code, encryptedData, signature, iv, cb, isphonedata) {
    wx.showToast({
      title: '加载中',
      icon: 'loading',
      duration: 10000
    })
    var that = this;
    console.log('准备调用loginByThirdPlatform接口')
    wx.request({
      url: addr.loginByThirdPlatform,
      data: {
        code: code,
        data: encryptedData,
        signature: signature,
        iv: iv,
        appid: that.globalData.appid || getApp().globalData.appid,
        isphonedata: isphonedata,
      },
      method: "Get",
      success: function(data) {
        if (data.data.result) {
          var json = data.data.obj
          console.log('loginByThirdPlatform接口返回 unionId - ' + json.unionId)
          var userInfo = {
            UserId: json.userid,
            openId: json.openId,
            nickName: json.nickName,
            avatarUrl: json.avatarUrl,
            isNewUser: json.avatarUrl ? false : true,
            IsValidTelePhone: json.IsValidTelePhone,
            StoreId: that.globalData.userInfo.StoreId
          };
          that.globalData.userInfo = userInfo
          console.log('json = ' + json);
          wx.hideToast()
          typeof cb == "function" && cb(userInfo)
        } else {
          var msg = (data.data.msg == '序列化类型为“System.Reflection.RuntimeModule”的对象时检测到循环引用。' ? '系统繁忙，请重新操作。' : data.data.msg)
          console.log('登录失败 data - ' + data.data)
          wx.showModal({
            title: '提示',
            content: msg,
          })
          wx.hideToast()
        }
      },
      fail: function(data) {
        console.log(data)
      },
      complete: function(data) {
        console.log(data)
      }
    })
  },

  onShow: function() {
    var that = this
    that.globalData.interval = setInterval(() => {
      if ((that.globalData.sourcetime + 1800000) < new Date().getTime()) {
        that.globalData.sourcestoreid = 0
      }
    }, 1000)
  },
  onHide: function() {
    clearInterval(this.globalData.interval)
  },
})