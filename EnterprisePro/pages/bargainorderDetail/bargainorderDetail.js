var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
const page = require("../../utils/pageRequest.js")
const tools = require("../../utils/tools.js")
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    cutpriceAllprice: 0,//砍价总购物价格
    goodmoney: 0,
    stateRemark: '',
    orderId: 0,
    openId: 0,
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    this.data.orderId = options.orderId
    this.inite1()
    page.pageShare(this)
    util.setPageSkin(this);
  },
  // 查询微信支付的单子
  inite: function () {
    var that = this
    wx.request({
      url: addr.Address.checkorderInfo,
      data: {
        appid: app.globalData.appid,
        openid: app.globalData.openId,
        orderId: that.data.orderId
      },
      method: "GET",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        if (res.data.isok == 1) {
          var goodInfo = []
          for (var i = 0; i < res.data.postdata.goodOrderDtl.length; i++) {
            goodInfo.push(res.data.postdata.goodOrderDtl[i])
          }
          that.setData({
            goodOrder: res.data.postdata.goodOrder,
            goodOrderDtl: res.data.postdata.goodOrderDtl,
            stateRemark: res.data.postdata.stateRemark,
            allOrder: res.data.postdata,
            goodInfo: goodInfo
          })
        }
        var goodmoney1 = parseFloat(res.data.postdata.buyPrice) - parseFloat(res.data.postdata.freightPrice).toFixed(2)
        var goodmoney = parseFloat(goodmoney1).toFixed(2)
        that.setData({ goodmoney: goodmoney })
      },
      fail: function () {
        wx.showToast({
          title: '获取首页出错',
        })
      }
    })
  },
  // 砍价详情
  inite1: function () {
    var that = this
    wx.request({
      url: addr.Address.GetOrderDetail,
      data: {
        AppId: app.globalData.appid,
        UserId: app.globalData.userInfo.UserId,
        buid: that.data.orderId
      },
      method: "POST",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        if (res.data.isok) {
          var WxAddress = res.data.obj.WxAddress.WxAddress
          WxAddress = JSON.parse(WxAddress)
          var OrderDetail = res.data.obj.OrderDetail
          OrderDetail.BuyTimeStr1 = util.ChangeDateFormatNew(OrderDetail.BuyTime)
          OrderDetail.CreateDateStr = util.ChangeDateFormatNew(OrderDetail.CreateDate)
          OrderDetail.CreateOrderTimeStr = util.ChangeDateFormatNew(OrderDetail.CreateOrderTime)
          OrderDetail.SendGoodsTimeStr = util.ChangeDateFormatNew(OrderDetail.SendGoodsTime)
          OrderDetail.StartDateStr = util.ChangeDateFormatNew(OrderDetail.StartDate)
          OrderDetail.ConfirmReceiveGoodsTimeStr = util.ChangeDateFormatNew(OrderDetail.ConfirmReceiveGoodsTime)
          OrderDetail.outOrderDateStr = util.ChangeDateFormatNew(OrderDetail.outOrderDate)
          var cutpriceAllprice = (Number(OrderDetail.GoodsFreightStr) + Number(OrderDetail.CurrentPriceStr)).toFixed(2)
          that.setData({
            cutpriceAllprice: cutpriceAllprice,
            OrderDetail: OrderDetail,
            WxAddress: WxAddress,
          })
        }
      },
      fail: function () {
        wx.showToast({
          title: '获取首页出错',
        })
      }
    })
  },
  // 复制订单号
  copylistNums: function (e) {
    let [index, data] = [e.currentTarget.id, 0]
    if (index == 0) {
      data = this.data.OrderDetail.OrderId
    } else {
      data = this.data.goodOrder.OrderNum
    }
    util.copy(data)
  },
  // 返回主页
  siwchtoIndex: function () {
    wx.redirectTo({
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

	/**
	 * 用户点击右上角分享
	 */
  onShareAppMessage: function () {
    let that = this
    return {
      title: that.data.shareTitle,
      path: '/pages/bargainorderDetail/bargainorderDetail?isIndex1=' + that.data.isIndex1 + "&orderId=" + this.data.orderId,
      imageUrl: that.data.shareImage,
      success: function (res) {
        tools.showToast("转发成功")
      }
    }
  }
})