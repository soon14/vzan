// pages/classify/search_goods/search_goods.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    showSearch: true,
    searchHistroy: [],
    good_vm: {
      storeId: 0,
      cateIdOne: 0,
      cateId: 0,
      pageSize: 20,
      pageIndex: 1,
      kw: '',
    },
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    var cateId = options.gid || 0
    if (options.storeid) {
      that.data.good_vm.storeId = options.storeid
    }
    app.login(function() {
      that.beginload(cateId)
    })

    //搜索历史
    var searchHistroy = wx.getStorageSync('searchHistroy')
    if (searchHistroy) {
      this.setData({
        searchHistroy: searchHistroy
      })
    }
  },
  nt_goodinfo: function(e) {
    var gid = e.currentTarget.dataset.gid
    template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + gid)
  },
  set_search: function() {
    var _sh = wx.getStorageSync('searchHistroy')
    this.setData({
      showSearch: !this.data.showSearch,
      searchHistroy: (_sh ? _sh : [])
    })
  },
  inputkw: function(e) {
    this.setData({
      'good_vm.kw': e.detail.value,
    })
  },
  cancel_search: function() {
    this.data.good_vm.kw = ''
    this.search_good()
    this.setData({
      good_vm: this.data.good_vm,
    })
  },
  clear_sh: function() {
    wx.removeStorageSync('searchHistroy')
  },
  search_good: function(e) {
    var that = this
    var kw = (e && e.currentTarget.dataset.kw ? e.currentTarget.dataset.kw : that.data.good_vm.kw)
    that.data.good_vm.pageIndex = 1
    that.set_storage(kw) //保存历史搜索记录
    that.beginload(0, kw)
    that.setData({
      'good_vm.kw': kw,
      showSearch: false
    })
  },
  beginload: function(cateId, kw) {
    var that = this
    var _g = that.data.good_vm
    if (cateId) {
      _g.cateId = Number(cateId)
    }
    if (kw) {
      _g.kw = kw
    }
    http.GoodList(that, _g, 0)
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
    var _g = this.data.good_vm
    http.GoodList(this, _g, 1)
  },

  set_storage: function(kw) {
    if (!kw) {
      return
    }
    var _sh = this.data.searchHistroy
    //添加到缓存历史搜索记录当中
    if (_sh.length > 0) {
      var findkw = _sh.find(f => f == kw)
      if (!findkw) {
        _sh.push(kw)
        wx.setStorageSync('searchHistroy', _sh)
      }
    } else {
      _sh.push(kw)
      wx.setStorageSync('searchHistroy', _sh)
    }
  },
})