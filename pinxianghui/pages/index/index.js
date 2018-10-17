// pages/index/index.js
var http = require('../../script/pinxianghui.js');
var template = require('../../script/template.js');
var addr = require('../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    islog: 0, //debug参数
    btnLoading: false,
    isall: false, //是否已经加载所有商品数据
    swiper_dots: false,
    good_vm: {
      storeId: 0,
      cateIdOne: 0,
      cateId: 0,
      pageSize: 20,
      pageIndex: 1,
      kw: '',
    },
  },
  storeInfo_nt: function(e) {
    var storeid = e.currentTarget.dataset.storeid
    template.gonewpage('/pages/store/storeIndex/storeIndex?storeid=' + storeid)
  },
  biaoganStore_nt: function() {
    template.gonewpage('/pages/my/applyModel/modelStore')
  },
  go_applyEnter: function() {
    template.gonewpage('/pages/store/applyEnter/applyEnter')
  },
  nt_goodinfo: function(e) {
    var dataset = e.currentTarget.dataset
    if (dataset.url) {
      template.gonewpage(dataset.url)
    } else {
      template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + dataset.gid)
    }
  },
  classify_nt: function(e) {
    var fid = e.currentTarget.dataset.fid
    var scroll_into = e.currentTarget.id
    app.globalData.scroll_into = scroll_into
    app.globalData.fId = fid
    template.switchtab('/pages/classify/classify')
  },
  searchClassify_nt: function() {
    template.gonewpage('/pages/classify/search_classify/search_classify')
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    if (options.scene) {
      that.data.islog = options.scene
    }
    that.beginload()
  },
  loadmore_goodS: function() {
    this.setData({
      btnLoading: true
    })
    http.GoodList(this, this.data.good_vm, 1)
  },
  beginload: function() {
    var that = this
    var _g = that.data.good_vm
    app.login(function(g) {
      http.gRequest(addr.CategoryList, { //分类列表
        aId: g.aid
      }, function(callback) {
        var _s = callback.data.obj
        var fenleiList = []
        for (let i = 0; i < _s.length; i++) {
          if (_s[i].fId == 0) {
            fenleiList.push(_s[i])
          }
        }
        that.setData({
          fenleiList: fenleiList
        })

        var data = {}
        data.aid = app.globalData.aid
        data.storeId = 0
        data.islog = that.data.islog
        http.gRequest(addr.PlatFormInfo, data, function(callback) {
          that.setData({
            Swiper: callback.data.obj.picList,
            swiper_dots: callback.data.obj.picList.length > 1 ? true : false
          })
        })

        http.GoodList(that, _g, 0) //商品列表

        http.gRequest(addr.BiaoganStoreList, {
          aid: app.globalData.aid,
          pageIndex: 1,
          pageSize: 9
        }, function(callback) {
          that.setData({
            BiaoganList: callback.data.obj.list
          })
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
    this.data.good_vm.pageIndex = 1
    this.beginload()
    template.stoppulldown()
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {
    this.loadmore_goodS()
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function() {

  },
})