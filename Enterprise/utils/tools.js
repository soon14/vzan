
//工具类
var addr = require("addr.js");
var http = require("http.js");
var tools = {

  // 重置条件
  reset: function (list) {
    Object.assign(list, { pageindex: 1, list: [], ispost: false, loadall: false, exttypes: "", pricesort: "" })
  },
  // 返回页面顶部
  pageTop: function (that) {
    wx.pageScrollTo({
      scrollTop: 0,
    })
  },
  // 工具类showtoast
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
      duration: 1000,
    })
  },
  showLoadToast: function (msg) {
    wx.showToast({
      title: msg,
      duration: 1000,
      icon: "loading"
    })
  },
  //跳转新页面
  goNewPage: function (url) {
    if (getCurrentPages().length >= 5) {
      wx.redirectTo({
        url: url,
      })
    } else {
      wx.navigateTo({
        url: url,
      })
    }
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
  //重启动
  goLaunch: function (url) {
    wx.reLaunch({
      url: url
    })
  },
  // 拨打电话
  phoneFunc: function (phoneNumber) {
    if (phoneNumber) {
      wx.makePhoneCall({
        phoneNumber: phoneNumber,
      })
    } else {
      tools.showToast("未设置电话")
    }
  },
  // 打开地图
  mapFunc: function (lat, lng) {
    wx.openLocation({
      latitude: lat,
      longitude: lng,
      scale: 28
    })
  },
};
module.exports = tools;