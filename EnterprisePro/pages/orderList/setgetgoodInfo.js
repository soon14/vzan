// pages/orderList/setgetgoodInfo.js
var app = getApp()
const util = require("../../utils/util.js")
const tools = require("../../utils/tools.js")
Page({

  /**
   * 页面的初始数据
   */
  data: {
    name: '',
    phone: '',
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    util.setPageSkin(this);
    this.setData({ name: app.globalData.getWayinfo.name, phone: app.globalData.getWayinfo.phone })
  },
  inputname: function (e) {
    app.globalData.getWayinfo.name = e.detail.value
  },
  inputphone: function (e) {
    app.globalData.getWayinfo.phone = e.detail.value
  },
  submit: function () {
    if (app.globalData.getWayinfo.name == undefined || app.globalData.getWayinfo.phone == undefined || app.globalData.getWayinfo.name == '' || app.globalData.getWayinfo.phone == '') {
      wx.showToast({ title: '请填写完整信息', icon: 'loading' })
      return
    } else {
      tools.showToast('提交成功')
      setTimeout(function () {
        tools.goBackPage(1)
      }, 1000)
    }
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