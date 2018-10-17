// pages/paymymoney/paymymoney.js
var addr = require("../../utils/addr.js");
var app = getApp()
var util = require("../../utils/util.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    AccountMoneyStr: 0,//储值余额
    user: [],
    saveMoneySetList: [],//储值列表
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    app=getApp();
    app.getUserInfo(function (res) {
      that.inite()
      that.inite2()
      that.setData({ user: res })
    });
  },
  navitopayHistroy: function () {
    wx.navigateTo({
      url: '../payHistroy/payHistroy',
    })
  },
  // 马上重置
  gotopay: function (e) {
    var that = this
    var index = e.currentTarget.id
    that.inite1(index)
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
  // 充值项目列表
  inite: function (e) {
    var that = this
    wx.request({
      url: addr.Address.getSaveMoneySetList, //仅为示例，并非真实的接口地址
      data: {
        appid: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
      },
      method: "GET",
      header: {
        'content-type': 'application/x-www-form-urlencoded' // 默认值
      },
      success: function (res) {
        if (res.data.isok) {
          that.setData({
            saveMoneySetList: res.data.saveMoneySetList
          })
        }
      },
      fail: function () {
        console.log('获取不了会员信息')
      }
    })
  },
  // 预充值
  inite1: function (saveMoneySetId) {
    var that = this
    wx.request({
      url: addr.Address.addSaveMoneySet, //仅为示例，并非真实的接口地址
      data: {
        appid: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        saveMoneySetId: saveMoneySetId
      },
      method: "POST",
      header: {
        'content-type': 'application/x-www-form-urlencoded' // 默认值
      },
      success: function (res) {
        if (res.data.isok) {
          var orderid = res.data.orderid
          that.wxpaymoney(orderid)
        }
      },
      fail: function () {
        console.log('获取不了会员信息')
      }
    })
  },
  wxpaymoney: function (oradid) {
    var that = this
    // var orderId = parseInt(app.globalData.orderid)
    var oradid = oradid
    var newparam = {
      openId: app.globalData.userInfo.openId,
      orderid: oradid,
      'type': 1,
    }
    util.PayOrder(oradid, newparam, {
      failed: function () {
        wx.showModal({
          title: '提示',
          content: '支付失败，客户取消支付',
        })
      },
      success: function (res) {
        if (res == "wxpay") {
        } else if (res == "success") {
          wx.showToast({
            title: '支付成功',
            duration: 500
          })
          that.inite()
          that.inite2()
        }
      }
    })
  },
  // 获取储值余额
  inite2: function (e) {
    var that = this
    wx.request({
      url: addr.Address.getSaveMoneySetUser, //仅为示例，并非真实的接口地址
      data: {
        appid: app.globalData.appid,
        openId: app.globalData.userInfo.openId,
      },
      method: "GET",
      header: {
        'content-type': 'application/x-www-form-urlencoded' // 默认值
      },
      success: function (res) {
        if (res.data.isok == true) {
          that.setData({
            AccountMoneyStr: res.data.saveMoneySetUser.AccountMoneyStr
          })
        }
      },
      fail: function () {
        console.log('获取不了会员信息')
      }
    })
  },
})