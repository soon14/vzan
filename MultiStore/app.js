//app.js
var aldstat = require("./utils/ald-stat/san.js");
var util = require("./utils/util.js");
var http = require("./utils/http.js");
var addr = require("./utils/addr.js");
const tools = require('./utils/tools.js');
App({
  onLaunch: function () {
    var that = this;
    //第三方平台配置
    var exconfig = wx.getExtConfigSync()
    if (exconfig != undefined) {
      that.globalData.appid = exconfig.appid
    }
  },
  onShow: function () {
    this.getUserInfo()
  },
  chooseMode: 1,
  globalData: {
    location: {},
    userInfo: {},
    isgetTel: 0,
    telEncryptedData: 0,
    telIv: 0,
    //后台默认状态
    cityStatus: false,
    expressStatus: false,
    selfStatus: false,
    //前端更改状态
    cityShow: false,
    expressShow: false,
    selfShow: false,
    //页面切换状态判断
    pageShow: false,
    aid: 0,
    condition: 0,
    address: {},
    pageLocation: false,
    condition: '',
    currentPage: {},
    cityOrder: false,
    expressOrder: false,
    takeStartPrice: 0,//起步价
    cityPrice: 0,//单笔满免运费
    expressPrice: 0,//单笔满免运费
    reduction: {},
    dbOrderId: 0
  },
  // WeToast,

  // 获取用户定位
  getLocationInfo: function () {
    let that = this
    return new Promise(function (resolve, reject) {
      wx.getLocation({
        type: 'wgs84',
        success: function (res) {
          var latitude = res.latitude
          var longitude = res.longitude
          that.globalData.location = {
            latitude: latitude,
            longitude: longitude,
            showLocation: true
          }

          resolve(res)
        },
        fail: function (res) {
          that.globalData.location = {
            showLocation: false
          }
          resolve(res)
        },
        complete: function () {
        }

      })
    })
  },
  getUserInfo: function (cb) {
    let that = this
    let userInfo = wx.getStorageSync("userInfo")
    if (userInfo) {
      wx.setStorageSync("userInfo", userInfo)
    } else {
      //调用登录接口
      wx.login({
        success: function (res) {
          that.login(res.code)
        },
        fail: function (res) {
        }
      })
    }
  },
  //登录
  login: function (code) {
    let that = this;
    let app = that.globalData
    let appid = app.appid
    tools.showLoadToast('加载中')
    http.postAsync(addr.Address.WxLogin, {
      code,
      appid,
      needappsr: 0,
    }).then(function (data) {
      data.dataObj.userid = data.dataObj.Id
      data.dataObj.openId = data.dataObj.OpenId
      data.dataObj.nickName = data.dataObj.NickName
      data.dataObj.avatarUrl = data.dataObj.HeadImgUrl
      app.userInfo = data.dataObj
      wx.setStorageSync("userInfo", data.dataObj)
      that.getVipInfo(data.dataObj.Id)
    })
  },
  //16.获取会员信息
  getVipInfo: function (uid) {
    let that = this
    let levelid = wx.getStorageSync("levelid")
    if (levelid) {
      return;
    }
    else {
      http.getAsync(addr.Address.GetVipInfo, {
        appid: that.globalData.appid,
        uid: uid
      }).then(function (data) {
        wx.setStorageSync("levelid", data.model.levelid)
        wx.setStorageSync("model", data.model)
      })
    }
  },


})