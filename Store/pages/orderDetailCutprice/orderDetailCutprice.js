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
    payType:0,//支付方式 1是微信支付 2是余额支付
    goodmoney: 0,
    // 所有信息
    allOrder: [],
    // 头部字段
    goodOrder: [],
    goodInfo: [],
    // 商品详情
    goodOrderDtl: [],
    stateRemark: '',
    orderId: 0,
    openId: 0,
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    var openId = that.data.openId
    that.setData({ orderId: options.orderId,payType:app.globalData.payType })
    console.log(that.data.orderId)
    app.getUserInfo(function (e) {
      that.setData({
        openId: e.openId
      })
      that.inite()
    })
    console.log(options)
  },
  // 初始化
  inite: function () {
    var that = this
    wx.request({
      url: addr.Address.checkorderInfo,
      data: {
        appid: app.globalData.appid,
        openid: that.data.openId,
        orderId: that.data.orderId
      },
      method: "GET",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        if (res.data.isok == 1) {
          var goodInfo = []
          for (var i = 0; i < res.data.postdata.goodOrderDtl.length; i++) {
            goodInfo.push(res.data.postdata.goodOrderDtl[i])
          }
          that.setData({
            goodOrder: res.data.postdata.goodOrder,
            goodOrderDtl: res.data.postdata.goodOrderDtl,
            stateRemark: res.data.postdata.stateRemark,
            allOrder: res.data.postdata,
            goodInfo: goodInfo
          })
        }
        var goodmoney1 = parseFloat(res.data.postdata.buyPrice) - parseFloat(res.data.postdata.freightPrice).toFixed(2)
        var goodmoney = parseFloat(goodmoney1).toFixed(2)
        that.setData({ goodmoney: goodmoney })
        console.log(res.data.postdata)
      },
      fail: function () {
        console.log("获取首页出错")
        wx.showToast({
          title: '获取首页出错',
        })
      }
    })
  },
  makephoneCall: function () {
    var phone = app.globalData.customerphone
    if (phone == "") {
      this.GetStoreConfig()
    }
    else {
      wx.makePhoneCall({
        phoneNumber: phone,
      })
    }
    // wx.makePhoneCall({
    //   phoneNumber: phone
    // })
    console.log(phone)
  },
  //获取店铺配置
  GetStoreConfig: function () {
    var that = this
    var url = addr.Address.GetStoreConfig
    var param = {
      appid: app.globalData.appid,
    }
    var method = "GET"
    network.requestData(url, param, method, function (e) {
      if (e.data.isok == 1) {
        var phone = e.data.postdata.store.TelePhone
        app.globalData.customerphone = phone
        wx.makePhoneCall({
          phoneNumber: phone,
        })
      }
    })
  },
  // 复制订单号
  copylistNums: function () {
    wx.setClipboardData({
      data: this.data.goodOrder.OrderNum,
      success: function (res) {
        wx.getClipboardData({
          success: function (res) {
            // console.log(res.data) // data
            wx.showToast({
              title: '复制成功',
              duration: 500
            })
          }
        })
      }
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

  }
})