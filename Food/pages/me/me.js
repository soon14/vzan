// pages/me/me.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    userinfo: [],
    openId: '',
    takeout: 99,
    TelePhone: 0,
    TablesNo: 0,
  },
  // 跳转到我的地址页面
  navtoAddress: function () {
    wx.navigateTo({
      url: '../setAddress/setAddress?openId=' + this.data.openId + '&isMe=1',
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    var takeout = app.globalData.TakeOut
    var TelePhone = app.globalData.TelePhone
    // if (app.globalData.openId == '') {
    app.getUserInfo(function (e) {
      that.setData({ userinfo: app.globalData.userInfo, openId: e.openId, takeout: takeout, TelePhone: TelePhone, TablesNo: app.globalData.TablesNo })
    })
    // }
  },
  // 联系客服
  makephoneCall: function () {
    wx.makePhoneCall({
      phoneNumber: this.data.TelePhone,
    })
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {

  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function () {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function () {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  },
  formSubmit: function (e) {
    var formid = e.detail.formId
    wx.request({
      url: addr.Address.testMuBang,
      data: {
        AppId: app.globalData.appid,
        openid: getApp().globalData.userInfo.openId,
        formid: formid
      },
      method: "GET",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function (res) {
        if (res.data.isok == 1) {
          console.log('我是res.data.a',res.data.a)
          console.log('我是res.data.b',res.data.b)
          console.log('我是formid', formid)
        }
      },
      fail: function () {
        console.log("获取菜类分类出错")
        wx.showToast({
          title: '获取菜类出错',
        })
      }
    })
  },
})