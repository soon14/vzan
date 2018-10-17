// pages/me/me.js
const app = getApp()
const tools = require("../../utils/tools.js");
const animation = require("../../utils/animation.js");
const pageRequest = require("../../utils/public-request.js");
const http = require("../../utils/http.js")
const addr = require("../../utils/addr.js")
Page({

  /**
   * 页面的初始数据
   */
  data: {
    showMode: [
      { img: "dzicon icon-daifukuan f47", context: "待自取", url: "../orderList/orderList?condition=8", id: 0 },
      { img: "dzicon icon-daifahuo f47", context: "待发货", url: "../orderList/orderList?condition=1", id: 1 },
      { img: "dzicon icon-daishouhuo f47", context: "待收货", url: "../orderList/orderList?condition=2", id: 2 },
      { img: "dzicon icon-yiwancheng f47", context: "已完成/退款", url: "../orderList/orderList?condition=0", id: 3 },
    ],
    orderMode: [
      { context: "收货地址", id: 0 },
      { context: "联系客服", id: 1 },
    ],
  },
  onShow: function () {
    let that = this
    wx.getSetting({
      success(res) {
        if (res.authSetting['scope.userLocation']) {
          that.setData({ showChoose: true })
        }else{
          that.setData({ showChoose: false })
        }
      }
    })

  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    let user = wx.getStorageSync("userInfo")
    let modelUser = wx.getStorageSync("model")
    that.setData({ user, modelUser, })
    pageRequest.agminapp(this)
  },
  makeMinapp(e) {
    let id = e.currentTarget.dataset.id
    wx.navigateTo({
      url: '/pages/index/minapp?id=' + id,
    })
  },
  // 会员权益弹窗
  showVipFunc: function (e) {
    let currentStatu = e.currentTarget.dataset.statu;
    animation.utilDown(currentStatu, this)
  },
  mePageGoto: function () { tools.goNewPage("../addressChange/addressChange") },
  moreOrderGoto: function () { tools.goNewPage("../orderList/orderList") },
  orderGoto: function (e) { tools.goNewPage(e.currentTarget.dataset.url) },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    wx.clearStorageSync()
    app.getUserInfo()
    setTimeout(res => {
      let user = wx.getStorageSync("userInfo")
      let modelUser = wx.getStorageSync("model")
      tools.showToast("刷新成功");
      wx.stopPullDownRefresh()
      that.setData({ user, modelUser })
    }, 1000)
  },
  wxlogin: function (e) {
    let that=this
    let _g = e.detail
    let appid = app.globalData.appid
    if (_g.errMsg == 'getUserInfo:fail auth deny') {
      return;
    }
    wx.login({
      success: function (res) {
        http.postAsync(addr.Address.loginByThirdPlatform, {
          appid,
          iv: _g.iv,
          code: res.code,
          data: _g.encryptedData,
          signature: _g.signature,
          isphonedata: 0,
        }).then(data => {
          if (data.result) {
            http.getAsync(addr.Address.GetVipInfo, {
              appid: appid,
              uid: data.obj.userid
            }).then(function (datar) {
              let userInfo = data.obj
              wx.clearStorageSync("userInfo")
              wx.setStorageSync("userInfo", userInfo)
              that.setData({ user: userInfo, model: datar.model })
              app.globalDat.userInfo = userInfo
            })
          }
        })
      },
      fail: function (res) {
      }
    })
  },
})