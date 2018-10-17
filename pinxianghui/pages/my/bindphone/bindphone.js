// pages/my/bindphone/bindphone.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    timeOutCode: 60,
    phoneNumber: "",
    phoneCode: ""
  },
  inputNumber(e) {
    this.setData({
      phoneNumber: e.detail.value
    })
  },
  inputCode(e) {
    this.setData({
      phoneCode: e.detail.value
    })
  },
  // 获取验证码
  getCode: function() {
    var that = this
    if (that.data.phoneNumber.length != 11) {
      wx.showToast({
        title: '手机有误',
        icon: 'loading'
      })
      return
    }
    var data = {}
    data.tel = that.data.phoneNumber
    data.appid = app.globalData.appid
    data.type = 10
    http.pRequest(addr.SendSMS, data, function(callback) {
      // 倒计时
      let timer = setInterval(function() {
        that.data.timeOutCode--
          if (that.data.timeOutCode == 0) {
            that.setData({
              timeOutCode: 60
            })
            clearInterval(timer)
          }
        that.setData({
          timeOutCode: that.data.timeOutCode
        })
      }, 1000)
    })
  },
  // 注册
  bindphone: function() {
    var that = this
    if (that.data.phoneCode == '' || that.data.phoneNumber.length != 11) {
      wx.showToast({
        title: '信息有误',
        icon: 'loading'
      })
      return
    }
    var data = {}
    data.tel = that.data.phoneNumber
    data.code = that.data.phoneCode
    data.appid = app.globalData.appid
    http.pRequest(addr.ValidateSMS, data, function(callback) {
      wx.showToast({
        title: '注册成功',
      })
      getCurrentPages()[getCurrentPages().length - 2].data.isbind = true
      setTimeout(() => {
        wx.navigateBack({
          delta: 1
        })
      }, 500)
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {

  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function() {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function() {

  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function() {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function() {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function() {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {

  },
})