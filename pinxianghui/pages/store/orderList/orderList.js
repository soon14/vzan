// pages/store/orderList/orderList.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    searchVal: ['订单号', '取货码', '商品', '收货人', '手机号'],
    chooseType: false,
    headerChoose: [{
        content: '全部',
        id: 0,
        state: -999,
        datetype: -999
      },
      {
        content: '待发货',
        id: 1,
        state: 1,
        datetype: -999
      },
      {
        content: '待收货',
        id: 2,
        state: 2,
        datetype: -999
      },
      {
        content: '待自取',
        id: 3,
        state: 3,
        datetype: -999
      },
      {
        content: '交易成功',
        id: 4,
        state: 4,
        datetype: -999
      },
    ],
    headerType: 0,
    requestData: { //请求列表数据
      state: 0,
      pageIndex: 1,
      pageSize: 6,
      seltype: 1,
      val: '',
      dateType: -999,
      isreachBottom: 0, //0非触底 1触底加载
    },
  },
  makephonecall: function(e) {
    var phone = e.currentTarget.dataset.phone
    wx.makePhoneCall({
      phoneNumber: phone,
    })
  },
  choosetype: function(e) { //点击更改订单查询条件
    this.setData({
      chooseType: !this.data.chooseType
    })
    if (e.currentTarget.id) {
      this.setData({
        'requestData.seltype': e.currentTarget.id
      })
    }
  },
  changeType: function(e) { //点击查询订单状态
    var that = this
    var dataset = e.currentTarget.dataset
    var index = dataset.id
    var state = dataset.state
    var datatype = dataset.datatype
    var _rd = this.data.requestData
    _rd.pageIndex = 1
    _rd.state = state
    _rd.dateType = datatype
    _rd.isreachBottom = 0
    _rd.val = ''
    that.beginload(_rd)

  },
  input_goodsName: function(e) {
    this.setData({
      'requestData.val': e.detail.value
    })
  },
  search_good: function(e) {
    var _rd = this.data.requestData
    _rd.pageIndex = 1
    _rd.isreachBottom = 0
    this.beginload(_rd)
  },
  control_good: function(e) {
    var that = this
    var _d = e.currentTarget.dataset.item
    var index = e.currentTarget.dataset.id
    var content = (index == -1 ? '是否确认取消订单并退款？' : (index == 1 ? '是否确认给买家发货？' : '是否确认买家已取货？'))
    wx.showModal({
      title: '提示',
      content: content,
      success: function(res) {
        if (res.confirm) {
          var data = {}
          data.aid = app.globalData.aid
          data.storeId = _d.storeId
          data.orderId = _d.id
          data.state = index
          http.pRequest(addr.UpdateOrderState, data, function(callback) {
            template.showtoast('操作成功', 'success')
            var _rd = that.data.requestData
            _rd.isreachBottom = 0
            _rd.pageIndex = 1
            that.beginload(_rd)
          })
        }
      }
    })
  },
  tuikuan_good: function() {

  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    app.login(function() {
      that.data.requestData.aid = app.globalData.aid
      var data = that.data.requestData
      data.state = options.state
      data.dateType = options.datetype
      that.beginload(that.data.requestData)
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
    var that = this
    var _rd = that.data.requestData
    _rd.isreachBottom = 1
    that.beginload(_rd)
  },
  beginload: function(data) {
    var that = this
    data.aid = app.globalData.aid
    http.gRequest(addr.StoreOrderList, data, function(callback) {
      var _ol = callback.data.obj
      if (callback.data.code == 1) {
        that.data.requestData = data
        wx.hideNavigationBarLoading();
        if (data.isreachBottom == 0) {
          that.setData({
            storeOrderList: _ol
          })
          that.data.requestData.pageIndex = ++data.pageIndex
        } else {
          if (_ol.length != 0) {
            that.data.storeOrderList = that.data.storeOrderList.concat(callback.data.obj)
            that.setData({
              storeOrderList: that.data.storeOrderList,
            })
            that.data.requestData.pageIndex = ++data.pageIndex
          }
        }
        that.setData({
          headerType: data.state,
          requestData: data
        })
      }
    })
  },
})