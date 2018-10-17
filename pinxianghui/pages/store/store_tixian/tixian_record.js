// pages/store/store_tixian/tixian_record.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    requestData: {
      storeId: 0,
      pageIndex: 1,
      pageSize: 20,
      isreachBottom: 0,
      applyType: 3, //5：代理收益 3：商家收益
    },
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    that.data.requestData.applyType = options.applyType
		wx.setNavigationBarTitle({
			title: (options.applyType==5?'代理收益提现记录':'提现记录'),
		})
    var _rd = that.data.requestData
    _rd.storeId = options.storeid
    app.login(function() {
      _rd.aid = app.globalData.aid
      that.beginload(_rd)
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
  beginload: function(data) {
    var that = this
    http.gRequest(addr.DrawCashRecord, data, function(callback) {
      var _l = callback.data.obj.list
      if (callback.data.code == 1) {
        that.data.requestData = data
        wx.hideNavigationBarLoading();
        if (data.isreachBottom == 0) {
          that.setData({
            tixian_record: _l
          })
          that.data.requestData.pageIndex = ++data.pageIndex
        } else {
          if (_l.length != 0) {
            that.data.tixian_record = that.data.tixian_record.concat(res.data.obj.list)
            that.setData({
              tixian_record: that.data.tixian_record,
            })
            that.data.requestData.pageIndex = ++data.pageIndex
          }
        }
      }
    })
  },
})