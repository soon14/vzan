// pages/my/Agent/agentCount.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
		item:5,
    requestData: {
      pageIndex: 1,
      pageSize: 10,
      type: 0,
      isreachBottom: 0, //0初始化 1触底加载
    }
  },
  makephonecall: function(e) {
    var phone = e.currentTarget.dataset.phone
    wx.makePhoneCall({
      phoneNumber: phone,
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    var data = that.data.requestData
    data.type = options.requestType
    wx.setNavigationBarTitle({
      title: (options.requestType == 0 ? '我的代理' : '下级商户'),
    })
    that.beginload(data, 0)
  },
  beginload: function(data) {
    var that = this
    http.gRequest(addr.GetIncomeList, data, function(callback) {
      var _ol = callback.data.obj

      if (callback.data.code == 1) {
        that.data.requestData = data
        wx.hideNavigationBarLoading();
        if (data.isreachBottom == 0) {
          that.setData({
            incomeList: _ol,
          })
          that.data.requestData.pageIndex = ++data.pageIndex
        } else {
          if (_ol.list.length != 0) {
						that.data.incomeList.list = that.data.incomeList.list.concat(callback.data.obj.list)
            that.setData({
              incomeList: that.data.incomeList,
            })
            that.data.requestData.pageIndex = ++data.pageIndex
          }
        }
        that.setData({
          requestData: data
        })
      }
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
    var _rd = this.data.requestData
    _rd.isreachBottom = 1
    this.beginload(_rd)
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function() {

  }
})