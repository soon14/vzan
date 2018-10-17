// pages/orderInfo/orderInfo.js
var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    logoimg: '',
    FoodsName: '',
    postdata: [],
    goodOrder: [],
    goodOrderDtl: [],
    TablesNo: 0,
    TelePhone: 0,
    orderId: 0,
    item: [
      { name: '蔓越草莓', nums: '2', price: '30' },
      { name: '美式精选', nums: '1', price: '24' },
      { name: '美式精选', nums: '1', price: '24' },
      { name: '美式精选', nums: '1', price: '24' },
      { name: '美式精选', nums: '1', price: '24' },

    ]
  },
  // 联系商家
  makePhonecall: function () {
    wx.makePhoneCall({
      phoneNumber: app.globalData.TelePhone,
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var orderId = parseInt(app.globalData.orderid)
    this.setData({ logoimg: app.globalData.logoimg, FoodsName: app.globalData.FoodsName, TablesNo: app.globalData.TablesNo, TelePhone: app.globalData.TelePhone, orderId: orderId })
    this.inite(orderId)
  },
  // 取消订单
  cancelOrder: function (e) {
    var that = this
    var index = parseInt(e.currentTarget.id)
    wx.showModal({
      title: '提示',
      content: '是否确认取消订单',
      success: function (res) {
        if (res.confirm) {
          that.inite1(index, -1)
        } else if (res.cancel) {
          console.log('用户点击取消')
        }
      }
    })
  },
  // 二次付款
  gotoPay: function (e) {
    var that = this
    // var orderId = parseInt(app.globalData.orderid)
    var oradid = e.currentTarget.id
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
          app.goBackPage(1)
        }
      }
    })
  },
  // 申请退款
  cancelpayMoeny: function (e) {
    var that = this
    var index = parseInt(e.currentTarget.id)
    wx.showModal({
      title: '提示',
      content: '是否确认申请退款？',
      success: function (res) {
        if (res.confirm) {
          that.inite1(index, -2)
        } else if (res.cancel) {
          console.log('用户点击取消')
        }
      }
    })
  },
  // 确认送达
  orderisOk: function (e) {
    var that = this
    var index = parseInt(e.currentTarget.id)
    wx.showModal({
      title: '提示',
      content: '是否确认取消订单',
      success: function (res) {
        if (res.confirm) {
          that.inite1(index, 5)
        } else if (res.cancel) {
          console.log('用户点击取消')
        }
      }
    })
  },
  // 取消已付款订单
  inite1: function (orderId, State) {
    var that = this
    wx.request({
      url: addr.Address.updateMiniappGoodsOrderState,
      data: {
        AppId: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        orderId: orderId,
        State: State
      },
      method: "POST",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function (res) {
        if (res.data.isok == 1) {
          that.inite(that.data.orderId)
        } else {
          wx.showModal({
            title: '提示',
            content: res.data.msg,
            showCancel: false,
            success: function (res) {
              if (res.confirm) {
                wx.switchTab({
                  url: '../orderList/orderList',
                })
              }
            }
          })
        }
      },
      fail: function () {

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

  },
  //查询订单详情
  inite: function (orderId) {
    var that = this
    wx.request({
      url: addr.Address.getMiniappGoodsOrderById,
      data: {
        AppId: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        orderId: orderId
      },
      method: "GET",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function (res) {
        if (res.data.isok == 1) {
          that.setData({
            postdata: res.data.postdata,
            goodOrder: res.data.postdata.goodOrder,
            goodOrderDtl: res.data.postdata.goodOrderDtl
          })
        }
      },
      fail: function () {
        console.log("获取订单详情出错")
        wx.showToast({
          title: '获取订单详情出错',
        })
      }
    })
  },
})