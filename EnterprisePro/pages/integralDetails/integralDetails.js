const util = require("../../utils/util.js");
const addr = require("../../utils/addr.js");
const WxParse = require('../../utils/wxParse/wxParse.js')
const app = getApp();

Page({

  data: {
    indicatorDots: true,
    autoplay: true,
    interval: 3000,
    duration: 500,
    classify: 0,
    GoodsDetails: {},
    btnContent: "",
    integralType: "",
    goodsId: ""
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    this.GetExchangeActivity(options.id)
    this.data.integralType = options.currentClassify
    this.data.goodsId = options.id
    if (this.data.integralType == 0) {
      that.setData({
        btnContent: "立即兑换"
      })
    } else if (this.data.integralType == 1) {
      that.setData({
        btnContent: "立即购买"
      })
    }
    this.setData({
      integralType: options.currentClassify
    })
  },
  //轮播点击放大
  previewSwiperImg:function(e){
    let that = this
    wx.previewImage({
      current: that.data.GoodsDetails.imgs[e.target.id],
      urls: that.data.GoodsDetails.imgs
    })
  },
  //购买或兑换积分商品
  goodsBuy: function () {
    let goodsMenu = {
      currentClassify: this.data.integralType,
      goodsId: this.data.goodsId,
      goodsImg: this.data.GoodsDetails.activityimg,
      goodsTitle: this.data.GoodsDetails.activityname,
      freight: this.data.GoodsDetails.freightStr,
      integral: this.data.GoodsDetails.integral,
      priceStr: this.data.GoodsDetails.priceStr,
      originalPriceStr: this.data.GoodsDetails.originalPriceStr
    }
    let goodsData = JSON.stringify(goodsMenu)
    wx.navigateTo({
      url: "../integralOrderDetails/integralOrderDetails?goodsData=" + goodsData
    })
  },
  GetExchangeActivity: function (id) {
    let that = this;
    wx.request({
      url: addr.Address.GetExchangeActivity,
      data: {
        appId: app.globalData.appid,
        id: id
      },
      method: "POST",
      header: {
        'content-type': 'application/x-www-form-urlencoded'
      },
      success: function (res) {
        if (res.data.isok) {
          console.log(res.data.obj)
          // 替换富文本标签 控制样式
          res.data.obj.description = res.data.obj.description.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
          res.data.obj.description = res.data.obj.description.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
          res.data.obj.description = res.data.obj.description.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
          that.setData({
            GoodsDetails: res.data.obj,
            article: WxParse.wxParse('article', 'html', res.data.obj.description, that, 5),
          })
          let navTitle = res.data.obj.activityname
          util.navBarTitle(navTitle)

        }
      },
      fail: function (res) {
        wx.showModal({
          title: '获取用户信息失败',
          content: res.data.msg,
        })
      }
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
    let that = this
    wx.showToast({
      title: '正在刷新',
      duration: 1000,
      icon: "loading",
      success: function () {
        that.GetExchangeActivity()
      }
    })
    setTimeout(function () {
      wx.showToast({
        title: '刷新成功',
        duration: 800,
        icon: "success"
      })
    wx.stopPullDownRefresh()
    }, 1000)
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  }
})