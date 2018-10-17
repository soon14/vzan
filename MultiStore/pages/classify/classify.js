// pages/classify/classify.js
const app = getApp()
const tools = require("../../utils/tools.js");
const pageRequest = require("../../utils/public-request.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    // 产品列表组件
    goodListViewModal: {
      pageindex: 1,
      pagesize: 5,
      list: [],
      ispost: false,
      loadall: false,
    },
    condition: 0,
    proStoreId: 0,
    onClassify: false,
    claStatus: false,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    this.onloadFunc(options)
    
  },

  //onload数据封装
  onloadFunc: function (options) {
    let that = this;
    let proStoreId = that.data.proStoreId
    that.data.options = options
    let address = ""
    if (Object.keys(options).length == 0) {
      address = app.globalData.address
      that.setData({
        cityStatus: app.globalData.cityShow,
        selfStatus: app.globalData.selfShow,
        expressStatus: app.globalData.expressShow
      })
    }
    else {
      // 选择门店自取
      if (options.id) {
        app.globalData.pageShow = true
        app.globalData.pageLocation = false
        pageRequest.getStoreById(Number(options.id), that)
      }
      else {
        //配送
        address = options.address
        app.globalData.pageShow = false
        app.globalData.pageLocation = true
        app.globalData.lat = options.lat
        app.globalData.lng = options.lng
        app.globalData.address = options.address
        that.data.claStatus = true//地址选择后产品storeid请求
      }
    }
    let showChoose = app.globalData.location.showLocation
    that.setData({ address: address, currentPage: app.globalData.currentPage, showChoose})
    pageRequest.classifyRequest(that)
  },
  // 分类选择
  typesFunc: function (e) {
    let that = this
    let condition = ""
    let proStoreId = that.data.proStoreId
    e == undefined ? condition = "" : condition = e.currentTarget.dataset.id
    tools.resetFunc(that)
    wx.showLoading({
      title: '加载中...',
      mask: true,
      success: function () {
        pageRequest.proRequest(condition, proStoreId, that).then(data => {
          wx.hideLoading()
        })
      }
    })
    that.setData({ condition: condition, scrolltop: 0 })
  },
  // 产品详情
  detailsGoto: function (e) {
    tools.goNewPage("../details/details?pid=" + e.currentTarget.dataset.pid)
  },
  // 切换地址
  addressGoto: function (e) {
    tools.goNewPage("../addressSelect/addressSelect?condition=" + e.currentTarget.dataset.id)
  },
  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
    let that = this
    console.log(app)
    let proStoreId = that.data.proStoreId
    let condition = app.globalData.condition
    // 地址选择
    if (that.data.claStatus) {
      Promise.all([pageRequest.getStoresAfter(app.globalData.lat, app.globalData.lng, that)]).then(values => {
        if (values[0].code == 1) {
          values[0].obj.HomeId == 0 ? proStoreId = 0 : proStoreId = values[0].obj.Id
        }
        else {
          proStoreId = 0
        }
        pageRequest.proRequest("", proStoreId, that)
        that.data.proStoreId = proStoreId
      })
      that.data.claStatus = false
    }
    else {
      // 门店自取选择
      if (that.data.options.homeid) {
        Number(that.data.options.homeid) ? proStoreId = that.data.options.id : proStoreId = 0
      }
      else {
        app.globalData.currentPage.HomeId == 0 ? proStoreId = 0 : proStoreId = app.globalData.currentPage.Id
      }
      tools.resetFunc(that)
      pageRequest.proRequest(condition, proStoreId, that)
      that.data.proStoreId = proStoreId
    }
    that.setData({ condition: condition })
    app.globalData.condition = ""
    wx.getSetting({
      success(res) {
        if (res.authSetting['scope.userLocation']) {
          that.setData({ showChoose: true })
          that.onloadFunc(that.data.options)
        }
      }
    })
  },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast("正在刷新")                                                                                                                                                                             
    setTimeout(res => {
      tools.showToast("刷新成功")
      that.typesFunc()
      wx.stopPullDownRefresh()
    }, 1000)
  },
  // 返回顶部
  backFunc: function () {
    this.setData({
      scrolltop: 0
    })
  },
  /**
   * 页面上拉触底事件的处理函数
   */
  onLoadMore: function () {
    pageRequest.proRequest(this.data.condition, this.data.proStoreId, this)
  },

  // 分享
  onShareAppMessage: function () {
    return {
      title: app.globalData.StoreName,
      path: '/pages/classify/classify',
      success: res => { tools.showToast("转发成功") },
      fail: res => { tools.showToast("转发失败") }
    }
  },
})