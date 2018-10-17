// pages/my/applyModel/applyModel.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    is_apply: false,
  },
  applyBiaogan: function() {
    var that = this
    http.pRequest(addr.ApplyBiaogan, {
      aid: app.globalData.aid
    }, function(callback) {
      that.setData({
        is_apply: true
      })
    })
  },
  copy_wx: function() {
    wx.setClipboardData({
      data: 'pinxianghui123',
    })
  },
  hide_isApply: function() {
    template.goback(1)
    // this.setData({
    //   is_apply: false
    // })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    app.login(function() {

    })
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