var utils = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var http = require('../../utils/http.js');
var pages = require("../../utils/pageRequest.js")
var tools = require("../../utils/tools")
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    ispay: 0,
    ismyOrder: 1,//用来判断立减金的入口，默认值是1（弹）。
    smoney: false,
    postdata: [],//订单详情数据大集合
    OrderDetail: [],//砍价订单商品详情
  },
  // 复制订单
  copy: function () {
    utils.copy(this.data.postdata.goodOrder.OrderNum)
  },
  // 获取立减金
  getreducemoney: function () {
    tools.goNewPage('../getsmoney/getsmoney')
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    if (options.ispay != undefined) { that.setData({ ispay: options.ispay }) }
    that.data.orderId = options.orderId
    that.checkDetails()
    utils.setPageSkin(that);
    http.postAsync(addr.Address.GetTableNoQrCode, {
      appid: app.globalData.appid,
      orderId: options.orderId
    }).then(function (data) {
      if (data.isok) {
        that.setData({
          "hxqrcode": data.Msg
        });
      }
    });
  },
  cancelsmoney: function () {
    this.setData({ smoney: !this.data.smoney })
  },
  // 查询详情单
  checkDetails: function () {
    var that = this
    http.getAsync(
      addr.Address.getMiniappGoodsOrderById,
      {
        appid: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        // openid: wx.getStorageSync('userInfo').openId,
        orderId: that.data.orderId
      }
    ).then(function (data) {
      if (data.isok == 1) {
        var postdata = data.postdata
        postdata.freightPrice = parseFloat(postdata.freightPrice).toFixed(2)
        postdata.goodAllPrice = parseFloat(postdata.buyPrice - postdata.freightPrice).toFixed(2)
        for (var i = 0; i < postdata.goodOrderDtl.length; i++) {
          postdata.goodOrderDtl[i].price = parseFloat(postdata.goodOrderDtl[i].price).toFixed(2)
        }
        if (postdata.goodOrderDtl[0].orderDtl.State == 1 && postdata.goodOrder.BuyMode == 1 && app.globalData.reductionCart != null && that.data.ispay == 1) {
          that.data.smoney = true
        } else {
          that.data.smoney = false
        }
        that.setData({
          smoney: that.data.smoney,
          postdata: postdata
        })
      }
    })
  },
  homeGoto: function () {
    wx.reLaunch({
      url: '../index/index',
    })
  },

  // 微信二次付款
  gotopay: function (e) {
    var that = this
    var oradid = that.data.postdata.goodOrder.OrderId
    var entgoodorderid = that.data.postdata.goodOrder.Id;
    pages.goPayFuc(oradid, entgoodorderid)
  },


})