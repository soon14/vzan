//app.js
var aldstat = require("./utils/ald-stat/san.js");
const http = require("./utils/http.js");
const addr = require("./utils/addr.js");
const utils=require("./utils/util.js")

App({
  onLaunch: function (options) {
    var that = this;
    var exconfig = wx.getExtConfigSync()  //第三方平台配置
    if (exconfig != undefined) {
      that.globalData.appid = exconfig.appid
      that.globalData.areaCode = exconfig.areaCode
    }
  },
  onShow: function () {
    utils.getSystem()
    this.getUserInfo()
  },
  // 推出小程序停止播放
  onHide: function () {
    wx.stopBackgroundAudio()
  },

  globalData: {
    pages: '',
    _bgFirst:true,
  },

  getUserInfo: function () {
    let that = this
    //调用登录接口
    let userInfo = wx.getStorageSync("userInfo")
    if (userInfo) {
      wx.setStorageSync("userInfo", userInfo)
    } else {
      wx.login({
        success: function (res) {
          that.login(res.code)
        },
        fail: function (res) {
          wx.showModal({
            title: '提示',
            content: res.errMsg,
          })
        }
      })
    }
  },
  //登录
  login: function (code) {
    let appid = this.globalData.appid
    wx.showToast({
      title: '加载中',
      icon: 'loading',
      duration: 3000
    })
    http.postAsync(addr.Address.WxLogin, {
      appid,
      code: code,
      needappsr: 0,
    }).then(data => {
      if (data.isok) {
        data.dataObj.openId = data.dataObj.OpenId
        data.dataObj.userid = data.dataObj.Id
        wx.setStorageSync("userInfo", data.dataObj)
      }
    })
  },








})