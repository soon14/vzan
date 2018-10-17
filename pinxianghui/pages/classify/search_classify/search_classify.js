// pages/classify/search_classify/search_classify.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    typeid: 0,
    header_name: '全部',
    storeid: 0,
  },
  change_typeid: function(e) {
    var id = e.currentTarget.dataset.typeid
    var name = e.currentTarget.dataset.headername
    wx.setNavigationBarTitle({
      title: name
    })
    this.setData({
      typeid: id,
      header_name: name
    })
  },
  searchGoods_nt: function(e) {
    var fid = e.currentTarget.dataset.fid
    var gid = e.currentTarget.dataset.gid
    if (gid) {
      var url = '/pages/classify/search_goods/search_goods?gid=' + gid + '&storeid=' + this.data.storeid
    } else {
      var url = '/pages/classify/search_goods/search_goods?storeid=' + this.data.storeid
    }
    template.gonewpage(url)
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    if (options.storeid) {
      that.data.storeid = options.storeid
    }
    var g = getApp().globalData;
    app.login(function() {
      http.gRequest(addr.CategoryList, {
        aId: g.aid,
        storeId: options.storeid || 0
      }, function(callback) {
        var _s = callback.data.obj

        var fidArray = []
        fidArray =
          _s.filter(f => {
            return f.fId == 0
          })
        for (let i = 0; i < fidArray.length; i++) {
          fidArray[i].sortArray =
            _s.filter(f => {
              return f.fId == fidArray[i].id
            })
        }

        that.setData({
          fenleiList: _s,
          fidArray: fidArray
        })
      })
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