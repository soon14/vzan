var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    payType: 0,//1是微信支付 2是储值支付
    dbOrder: 0,
    totalMoeny: 0,
    // 导航分类
    sort: [
      { sortIcon: '../../image/a5.png', sortContext: '导航分类' },
      { sortIcon: '../../image/a5.png', sortContext: '导航' },
      { sortIcon: '../../image/a5.png', sortContext: '包' },
      { sortIcon: '../../image/a5.png', sortContext: '导航分类' },
    ],
    // 商品模板
    goods: [
      { goodImg: '../../image/test.png', goodContent: '啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊', goodPrice: '1683', sold: '523' },

      { goodImg: '../../image/test.png', goodContent: '啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊', goodPrice: '1683', sold: '523' },

      { goodImg: '../../image/test.png', goodContent: '啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊', goodPrice: '1683', sold: '523' },
    ],
    // 轮播图
    imgUrls: [
      'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
      'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
      'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg'
    ],
    // 轮播图属性
    indicatorDots: false,
    autoplay: false,
    interval: 5000,
    duration: 1000,
    loading: 0,
    pageindex: 0,
    buyMode: 0,//用来判断用什么方法查询订单详情 正常商品无论用什么支付方式都是用旧接口，砍价商品用砍价接口查询
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var dbOrder = app.globalData.dbOrder
    this.setData({ totalMoeny: options.totalMoney, orderId: options.orderId, dbOrder: dbOrder, payType: app.globalData.payType, buyMode: options.buyMode })
    this.inite2()
    console.log('!', options)
    console.log('我是商品号', dbOrder)


  },
  // 跳转到订单详情
  navToorderDetail: function () {
    var dbOrder = this.data.dbOrder
    wx.redirectTo({
      url: '../orderDetail/orderDetail?orderId=' + dbOrder + '&buyMode=' + this.data.buyMode,
    })
  },
  // 跳转到首页
  switchIndex: function () {
    wx.switchTab({
      url: '../index/index',
    })
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {

  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function () {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function () {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },
  navTogoodList: function (e) {
    console.log(e)
    wx.redirectTo({
      url: '../goodList/goodList?id=' + e.currentTarget.id,
    })
  },
  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  },

  // 获取首页商品
  inite2: function (typeid, pageindex) {
    var that = this
    wx.request({
      url: addr.Address.getClassify,
      data: {
        appid: app.globalData.appid,
        typeid: typeid,
        pageindex: pageindex,
        pagesize: 4,
        orderbyid: 0,
        levelid: app.globalData.levelid
      },
      method: "GET",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        if (res.data.isok == 1) {
          var goods = that.data.goods
          if (res.data.postdata.goodslist.length > 0) {
            goods = goods.concat(res.data.postdata.goodslist)
          }
          that.setData({
            goods: res.data.postdata.goodslist,
          })
          that.data.pageindex++
          that.data.loading = 0
        }
        console.log(res)
      },
      fail: function () {
        console.log("获取商品列表出错")
        wx.showToast({
          title: '获取商品列表失败',
        })
      }
    })
  },
})