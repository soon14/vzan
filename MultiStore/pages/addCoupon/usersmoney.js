// pages/getsmoney/invitegetsmoney.js
const app = getApp()
const tools = require('../../utils/tools.js')
const pageRequest = require("../../utils/public-request.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {},

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    if (options && options.coupon) {
      let coupon = JSON.parse(options.coupon)
      setTimeout(function () {
        if (coupon != null) {
          setInterval(function () {
            var nowTime = new Date().getTime() //现在的时候戳
            if (new Date(coupon.StartUseTimeStr).getTime() > nowTime) {
              coupon.endtime = '活动仍未开始'
            } else {
              var endtime = ((new Date(coupon.EndUseTimeStr)).getTime() - nowTime)
              coupon.endtime = tools.formatDuring(endtime)
            }

            that.setData({ coupon: coupon })
          }, 1000)
        }
      }, 1000)
    }
  },
  swichtoindex: function () {
    wx.reLaunch({ url: '../index/index' })
  },

})