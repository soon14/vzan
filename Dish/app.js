//app.js
var _DuoguanData = require('./utils/data.js');
var listener = require('./utils/listener.js');
var aldstat = require("./utils/san.js");
App({
  onLaunch: function() {
    var that = this;
    var exconfig = wx.getExtConfigSync()
    if (exconfig != undefined) {
      that.globalData.appid = exconfig.appid
    }
    if (wx.getStorageSync('userInfo') && wx.getStorageSync('aid')) {
      wx.hideToast()
    } else {
      this.login()
    }
  },
  login: function() {
    var that = this
    wx.login({
      success: function(res) {
        that.WxLogin(res.code, getApp().globalData.appid);
        that.getAid()
      }
    })
  },
  //新版第三方登录
  WxLogin: function(code, appid) {
    wx.showToast({
      title: '正在登录....',
      icon: 'loading',
      duration: 10000
    })
    var that = this;
    wx.request({
      url: _DuoguanData.WxLogin,
      data: {
        code: code,
        appid: appid,
        needappsr: 0,
      },
      header: {
        'content-type': 'application/x-www-form-urlencoded'
      },
      method: "POST",
      success: function(data) {
        console.log(data)
        var data = data.data
        if (data.isok) {
          wx.startPullDownRefresh()
          data.dataObj.UserId = data.dataObj.Id
          data.dataObj.openId = data.dataObj.OpenId
          data.dataObj.nickName = data.dataObj.NickName
          data.dataObj.avatarUrl = data.dataObj.HeadImgUrl
          that.globalData.userInfo = data.dataObj
          wx.setStorageSync("userInfo", data.dataObj)
          wx.setStorageSync("utoken", data.dataObj.loginSessionKey)
        } else {
          wx.showModal({
            title: '提示',
            content: data.data.Msg,
          })
        }
        wx.hideToast()
      },
      fail: function(data) {
        console.log(data)
      },
    })
  },
  // ͨ获取店铺aid
  getAid: function() {
    var that = this;
    wx.request({
      url: _DuoguanData.getAid,
      data: {
        appid: getApp().globalData.appid,
      },
      header: {
        'content-type': 'application/x-www-form-urlencoded'
      },
      method: "POST",
      success: function(data) {
        if (data.data.code == 1) {
          wx.setStorageSync('aid', data.data.info)
        } else {
          wx.showModal({
            title: '提示',
            content: data.data.info,
            showCancel: false
          })
					return					
        }
      },
      fail: function(data) {
        console.log(data)
      },
    })
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
      url: _DuoguanData.loginByThirdPlatform,
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
          that.globalData.userInfo = {
            HeadImgUrl: json.avatarUrl,
            avatarUrl: json.avatarUrl,
            city: json.city,
            UserId: json.userid,
            country: json.country,
            gender: json.gender,
            language: json.language,
            NickName: json.nickName,
            nickName: json.nickName,
            openId: json.openId,
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

          console.log('json = ' + json);
          wx.hideToast()
          typeof cb == "function" && cb(that.globalData.userInfo)
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
  globalData: {
    appid: '',
  }
})