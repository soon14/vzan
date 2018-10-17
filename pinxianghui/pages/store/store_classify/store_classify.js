// pages/classify/classify.js
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
    isme: 0, //渲染假底部导航栏 0店铺主页 1店铺搜索 2我的
    good_vm: {
      storeId: 0,
      cateIdOne: 0,
      cateId: 0,
      pageSize: 20,
      pageIndex: 1,
      kw: '',
    },


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
  url_nt: function(e) {
    var index = e.currentTarget.dataset.id
    var _d = this.data.good_vm
    if (index == 0) {
      template.gonewpagebyrd('/pages/store/storeIndex/storeIndex?storeid=' + _d.storeId)
    } else if (index == 1) {
      if (getCurrentPages()[getCurrentPages().length - 1].route == "pages/store/store_classify/store_classify") {
        return
      }
      template.gonewpagebyrd('/pages/store/store_classify/store_classify?storeid=' + _d.storeId)
    } else {
      template.gonewpagebyrd('/pages/store/store_my/store_my?storeid=' + _d.storeId)
    }
  },
  search_goods: function(e) {
    var that = this
    var g = getApp().globalData;
    var _g = that.data.good_vm
    var fId = e.currentTarget.dataset.typeid
    app.globalData.fId = fId
    _g.cateIdOne = fId
    _g.aId = g.aid;
    _g.pageIndex = 1
    http.GoodList(that, _g, 0)
    that.setData({
      typeid: fId
    })
  },
  searchClassify_nt: function() {
		template.gonewpage('/pages/classify/search_classify/search_classify?storeid=' + this.data.good_vm.storeId)
  },
  nt_goodinfo: function(e) {
    var gid = e.currentTarget.dataset.gid
    template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + gid)
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    var _g = that.data.good_vm
    if (getCurrentPages()[getCurrentPages().length - 1].route == "pages/store/store_classify/store_classify") {
      that.setData({
        isme: 1
      })
    }
    http.gRequest(addr.CategoryList, {
      aId: app.globalData.aid,
      storeId: options.storeid
    }, function(callback) {
      var _s = callback.data.obj
      _s.sort(function(a, b) {
        return b.sort - a.sort
      }).sort
      _g.storeId = options.storeid
      http.GoodList(that, _g, 0)
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
    // var findPageIndex = getCurrentPages().findIndex(f => f.route == 'pages/store/store_classify/store_classify')
    // if (findPageIndex && getCurrentPages().length > 2) {
    //   setTimeout(function() {
    //     template.goback(getCurrentPages().length - findPageIndex - 1)
    //   }, 500)
    // } else {
      var fId = app.globalData.fId
      var _g = that.data.good_vm
      if (fId) {
        that.setData({
          typeid: fId
        })
        _g.cateIdOne = fId
        _g.pageIndex = 1
        http.GoodList(that, _g, 0)
      }
    // }

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