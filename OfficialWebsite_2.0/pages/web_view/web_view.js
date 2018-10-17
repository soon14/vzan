// pages/me/web_view.js
var addr = require("../../utils/address.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    submitType: ['成为代理商', '开发小程序'],
    typeid: 0,
    agentqrcodeid: 0, //分销推广id
    phoneNumber: '', //手机号码
    Code: '', //手机验证码
    psw: '',
    intervalTime: 60,
  },
  chooseType: function(e) {
    this.setData({
      typeid: e.currentTarget.dataset.id
    })
  },
  inputPhone: function(e) {
    this.data.phoneNumber = e.detail.value
  },
  inputCode: function(e) {
    this.data.Code = e.detail.value
  },
  inputPassword: function(e) {
    this.data.psw = e.detail.value
  },

  // 获取手机验证码
  GetVaildCode: function() {
    var that = this
    if (that.data.phoneNumber.length != 11) {
      wx.showToast({
        title: '手机有误',
        icon: 'loading'
      })
      return
    }
    wx.request({
      url: addr.Address.GetVaildCode,
      data: {
        phonenum: that.data.phoneNumber,
        type: 1,
        agentqrcodeid: that.data.agentqrcodeid
      },
      method: "POST",
      header: {
        'content-type': "application/x-www-form-urlencoded"
      },
      success: function(res) {
        if (res.data.isok) {
          // 倒计时
          let timer = setInterval(function() {
            that.data.intervalTime--
              if (that.data.intervalTime == 0) {
                that.setData({
                  intervalTime: 60
                })
                clearInterval(timer)
              }
            that.setData({
              intervalTime: that.data.intervalTime
            })
          }, 1000)
        } else {
          wx.showModal({
            title: '温馨提示',
            content: res.data.Msg,
            showCancel: false,
            confirmText: '我知道了'
          })
        }
      },
    })
  },
  // 注册
  SaveUserInfo: function() {
    var _t = this.data
    if (_t.Code == '' || _t.phoneNumber.length != 11) {
      wx.showToast({
        title: '信息有误',
        icon: 'loading'
      })
      return
    }
    wx.request({
      url: addr.Address.SaveUserInfo,
      data: {
        password: _t.psw,
        phone: _t.phoneNumber,
        code: _t.Code,
        agentqrcodeid: _t.agentqrcodeid,
        opentype: _t.typeid,
        appid: app.globalData.appid
      },
      method: "POST",
      header: {
        'content-type': "application/x-www-form-urlencoded"
      },
      success: function(res) {
        if (res.data.isok == true) {
          wx.showToast({
            title: '注册成功',
          })
          setTimeout(() => {
            wx.navigateBack({
              delta: 1
            })
          }, 500)
        } else {
          wx.showModal({
            title: '温馨提示',
            content: res.data.Msg,
            showCancel: false,
            confirmText: '我知道了'
          })
        }
      },
    })
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    this.data.agentqrcodeid = options.id
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