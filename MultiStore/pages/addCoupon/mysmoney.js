// pages/getsmoney/mysmoney.js
const app = getApp()
const tools = require('../../utils/tools.js')
const pageRequest = require("../../utils/public-request.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {

  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    pageRequest.GetReductionCardList(that, app.globalData.currentPage.Id)

    setTimeout(function () {
      var couponsList = that.data.couponsList
      if (couponsList != null) {
        setInterval(function () {
          for (var i = 0; i < couponsList.length; i++) {
            var nowTime = new Date().getTime()
            var endtime = ((new Date(couponsList[i].EndUseTimeStr)).getTime() - nowTime)
            couponsList[i].endtime = tools.formatDuring(endtime)
            that.setData({ couponsList: couponsList })
          }
        }, 1000)
      }
    }, 1000)
  },
  navtoinvitesmoney: function (e) {
    app.globalData.reduction.Id = e.currentTarget.id
    app.globalData.dbOrderId = e.currentTarget.dataset.orderid
    wx.navigateTo({ url: '/pages/addCoupon/getsmoney' })
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
  onShareAppMessage: function (e) {
    var that = this
    return {
      title: '社交立减金',
      path: '/pages/addCoupon/invitegetsmoney?couponsId=' + e.target.id + '&orderId=' + e.target.dataset.idx
    }
  }
})