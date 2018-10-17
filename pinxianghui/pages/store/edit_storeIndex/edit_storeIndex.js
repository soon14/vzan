// pages/store/edit_storeIndex/edit_storeIndex.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
import {
  core
} from '../../../utils/core.js';
Page({

  /**
   * 页面的初始数据
   */
  data: {
    requestData: {
      id: 0,
      storeName: '',
      phone: '',
      logo: '',
    }
  },
  inputName: function(e) {
    this.setData({
      'requestData.storeName': e.detail.value
    })
  },
  inputPhone: function(e) {
    this.setData({
      'requestData.phone': e.detail.value
    })
  },
  uploadImg: function() {
    var that = this
    app.login(function() {

      core.upload('img', 1).then(function(callback) {
        that.setData({
          'requestData.logo': callback[0]
        })
      })

    })
  },
  delImg: function() {
    this.setData({
      'requestData.logo': ''
    })
  },
  save_storeMsg: function() {
    var that = this
    var _rd = that.data.requestData
    if (!_rd.logo || !_rd.storeName || !_rd.phone) {
      template.showtoast('信息未完善', 'none')
      return
    }
    http.pRequest(addr.EditStore, _rd, function(callback) {
      template.showtoast('保存成功', 'success')
      template.goback(1)
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var _rd = JSON.parse(options.requestData)
    if (_rd.logo.substring(0, 20) == 'https://wx.qlogo.cn/') {
      _rd.logo = ''
    }
    this.setData({
      requestData: _rd
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

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function() {

  }
})