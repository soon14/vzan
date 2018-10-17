// pages/classify/classify.js
var http = require('../../script/pinxianghui.js');
var template = require('../../script/template.js');
var addr = require('../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    typeid: 0,
    good_vm: {
      storeId: 0,
      cateIdOne: 0,
      cateId: 0,
      pageSize: 20,
      pageIndex: 1,
      kw: '',
    },
    scroll_into: 's_0_0',


    goods: [{
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装彩妆十件套装彩妆十件套装彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
    ],
  },
  search_goods: function(e) {
    var that = this
    var g = getApp().globalData;
    var _g = that.data.good_vm
    var fId = e.currentTarget.dataset.typeid
    app.globalData.fId = fId
    app.globalData.scroll_into = e.currentTarget.id
    _g.cateIdOne = fId
    _g.aId = g.aid;
    _g.pageIndex = 1
    http.GoodList(that, _g, 0)
    that.setData({
      typeid: fId
    })
  },
  searchClassify_nt: function() {
    template.gonewpage('/pages/classify/search_classify/search_classify')
  },
  nt_goodinfo: function(e) {
    var gid = e.currentTarget.dataset.gid
    template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + gid)
  },
  go_applyEnter: function() {
    template.gonewpage('/pages/store/applyEnter/applyEnter')
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    var _g = that.data.good_vm
    http.gRequest(addr.CategoryList, {
      aId: app.globalData.aid
    }, function(callback) {
      var _s = callback.data.obj
      if (app.globalData.fId == 0) {
        http.GoodList(that, _g, 0)
      }
      that.setData({
        fenleiList: _s
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
    var that = this
    var fId = app.globalData.fId
    var _g = that.data.good_vm
    if (fId > 0) {
      that.setData({
        typeid: fId
      })
      _g.cateIdOne = fId
      _g.pageIndex = 1
      http.GoodList(that, _g, 0)
    }
    // that.setData({ scroll_into: app.globalData.scroll_into })
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
    http.GoodList(this, _g, 1)
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {
    var _g = this.data.good_vm
    http.GoodList(this, _g, 1)
  },
})