var net = require('../../utils/net');
var address = require('../../utils/address');
var util = require('../../utils/util');

var WxParse = require('../../wxParse/wxParse.js');

var app = getApp();
Page({
  data: {
    articles: [],
    canLoadMore: true,
    errorMsg: null,
    pageIndex: 1
  },
  onLoad: function(options) {
    app.IsShowBottomLogo(this)
    // 页面初始化 options为页面跳转所带来的参数
    console.log(options);
    this.loadDataFormNet()
  },
  onPullDownRefresh: function() {
    this.data.pageIndex = 1
    this.setData({
      articles: [],
      canLoadMore: true,
      errorMsg: null
    })
    this.loadDataFormNet(false)
    app.IsShowBottomLogo(this)
    wx.stopPullDownRefresh()
  },
  loadDataFormNet: function(isShowLoading = true) {
    var that = this
    var params = {
      appid: app.globalData.appid,
      pageIndex: that.data.pageIndex,
      level: 5,
      pageSize: 10
    }
    if (isShowLoading) {
      util.showLoadingDialog("正在加载...")
    }
    net.POST(address.Address.GET_MODEL_DATA, params, {
      success: function(res, msg) {
        util.hideLoadingDialog()
        util.stopPullDownRefresh()
        if (res.data.length < 10) {
          that.setData({
            canLoadMore: false,
            errorMsg: msg,
            articles: that.data.articles.concat(res.data),
          })
          return
        }



        console.log(res);
        if (Object.prototype.toString.call(res.data) == "[object Array]") {
          res.data.forEach(function(item, index) {
            item.Content = WxParse.wxParse('Content', 'html', item.Content, that, 5)
          });
        }
        that.data.pageIndex++
          that.setData({
            articles: that.data.articles.concat(res.data),
          })
      },
      failure: function(fail) {
        util.hideLoadingDialog()
        util.stopPullDownRefresh()
        console.log(fail);
        that.setData({
          errorMsg: fail
        })
      }
    })
  },
  nextPage: function() {
    if (this.data.canLoadMore) {
      this.loadDataFormNet()
    }
  },
  onReachBottom: function() {
    this.nextPage()
  },
  onItemClick: function(e) {
    wx.navigateTo({
      url: '../detail/detail?id=' + e.currentTarget.dataset.index + '&isProduct=1'
    })
  },
  onShareAppMessage: function() {
    return {
      path: '/pages/product/product'
    }
  },
})