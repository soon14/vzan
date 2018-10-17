// pages/my/orderList/orderList.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    header: [{
        content: '全部',
        state: -999,
        groupState: -999,
        commentState: -999
      },
      {
        content: '待付款',
        state: 0,
        groupState: -999,
        commentState: -999
      },
      {
        content: '待分享',
        state: -999,
        groupState: 0,
        commentState: -999
      },
      {
        content: '待发货',
        state: 1,
        groupState: -999,
        commentState: -999
      },
      {
        content: '待收货',
        state: 2,
        groupState: -999,
        commentState: -999
      },
      // { content: '待评价', state: 4, groupState: -999, commentState: 1 },
      {
        content: '交易成功',
        state: 4,
        groupState: -999,
        commentState: -999
      },
    ],
    headerType: 0,
    requestData: { //请求订单列表数据
      pageIndex: 1,
      state: -999,
      groupstate: -999,
      commentstate: -999
    },
  },
  confirm_getGood: function(e) {
    var that = this
    wx.showModal({
      title: '提示',
      content: '是否确认收货',
      success: function(res) {
        if (res.confirm) {
          var data = {}
          data.aid = app.globalData.aid
          data.storeId = e.currentTarget.dataset.storeid
          data.orderId = e.currentTarget.dataset.orderid
          http.pRequest(addr.OrderSuccess, data, function(callback) {
            template.showtoast('操作成功', 'success')
            var _rd = that.data.requestData
            _rd.pageIndex = 1
            that.OrderList(that, _rd, 0)
          })
        }
      }
    })

  },
  pintuaninfo_nt: function(e) {
    var groupid = e.currentTarget.dataset.groupid
    var storeid = e.currentTarget.dataset.storeid
    template.gonewpage('/pages/shopping/pintuanInfo/pintuanInfo?groupid=' + groupid + '&storeid=' + storeid)
  },
  orderinfo_nt: function(e) {
    var orderid = e.currentTarget.dataset.orderid
    var storeid = e.currentTarget.dataset.storeid
    template.gonewpage('/pages/shopping/orderInfo/orderInfo?orderid=' + orderid + '&storeid=' + storeid)
  },
  payinfo_nt: function(e) {
    var orderid = e.currentTarget.dataset.orderid
    var storeid = e.currentTarget.dataset.storeid
    var data = {}
    data.aid = app.globalData.aid
    data.storeId = storeid
    data.orderId = orderid
    http.pRequest(addr.PayAgain, data, function(callback) {
      var _i = callback.data.obj
      app.globalData.myAddress.addressinfo = _i.orderDetail.address
      app.globalData.myAddress.consignee = _i.orderDetail.consignee
      app.globalData.myAddress.mobile = _i.orderDetail.phone
      var shopcarData = {
        goodName: _i.goodsInfo.name,
        Price: _i.orderDetail.moneyStr,
        discountPrice: 0,
        reducePrice: (_i.specification ? Number(_i.specification.groupPrice / 100).toFixed(2) : ''),
        Number: _i.orderDetail.count,
        Stock: _i.goodsInfo.stock,
        imageUrl: _i.goodsInfo.img,
        specInfo: (_i.specification ? _i.specification.name : ''),
        Sort: [],
        Spec: [],
        Sortid: [],
        SpecinfoId: '',
        stockLimit: _i.goodsInfo.stockLimit,
        storeId: _i.goodsInfo.storeId,
        gid: _i.goodsInfo.id
      }
      var actionId = {
        cityMordersId: _i.cityMordersId,
        groupId: _i.groupId,
        groupState: (_i.groupCount - _i.entrantCount > 0 ? 0 : 1),
        orderid: orderid
      }
      template.gonewpage('/pages/shopping/payInfo/payInfo?shopcarData=' + JSON.stringify(shopcarData) + '&actionId=' + JSON.stringify(actionId))
    })
  },
  changeType: function(e) {
    var index = e.currentTarget.id
    this.setData({
      headerType: index
    })

    var data = e.currentTarget.dataset
    data.pageIndex = 1
    this.OrderList(this, data, 0)
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    that.setData({
      headerType: options.id || 0
    })
    app.login(function() {
      var data = JSON.parse(options.dataset)
      data.pageIndex = 1
      that.OrderList(that, data, 0)
    })
  },
  OrderList: function(that, data, isreachBottom) { //isreachBottom 0刷新 1加载更多
    wx.showNavigationBarLoading();
    data.utoken = app.globalData.utoken
    data.aid = app.globalData.aid
    data.pageSize = 6
    wx.request({
      url: addr.OrderList,
      data: data,
      method: "GET",
      header: {
        "content-type": "application/x-www-form-urlencoded"
      },
      success: function(res) {
        if (res.data.code == 1) {
          that.data.requestData = data
          wx.hideNavigationBarLoading();
          if (isreachBottom == 0) {
            that.setData({
              orderList: res.data.obj.list
            })
            that.data.requestData.pageIndex = ++data.pageIndex
          } else {
            if (res.data.obj.list.length != 0) {
              that.data.orderList = that.data.orderList.concat(res.data.obj.list)
              that.setData({
                orderList: that.data.orderList,
              })
              that.data.requestData.pageIndex = ++data.pageIndex
            }
          }
        }
      },
      fail: function() {
        console.log("查询订单列表出错")
      }
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

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {
    var data = this.data.requestData
    this.OrderList(this, data, 1)
  },
})