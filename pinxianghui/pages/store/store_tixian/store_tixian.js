// pages/store/store_tixian/store_tixian.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    is_tixian: false,
    unshow_tips: true,
    isAgent: 0, //0店铺提现 1代理商提现
    tixian_type: [{
        fontcss: 'dzicon icon-icon_erweima-1 icon_base eo_0',
        content: '店内扫码交易（手续费0%）',
        ischoose: true,
        isopen: false,
        money: 0,
        id: 4
      },
      {
        fontcss: 'dzicon icon-icon_jiaoyipingtai- icon_base eo_1',
        content: '平台交易（手续费0%）',
        ischoose: false,
        isopen: false,
        money: 0,
        id: 3
      },
      {
        fontcss: 'dzicon icon-icon_dailishouyi- icon_base eo_2',
        content: '代理收益（手续费0%）',
        ischoose: true,
        isopen: false,
        money: 0,
        id: 5
      },
    ],
    requestData: {
      cash: 0, //提现金额
      drawCashWay: 0, //提现方式 1银行卡 0微信 现仅支微信
      cashStr: 0, //对应的可提现金额
      applyType: 0, //applyType:（0：平台提现，1：店内扫码提现，2代理收益提现
    }
  },
  copy_wx: function() {
    wx.setClipboardData({
      data: 'dailimin99',
    })
  },
  hide_istixian: function() {
    this.setData({
      unshow_tips: false
    })
  },
  nt_tixianRecord: function() {
    var _rd = this.data.requestData
    var applyType = (_rd.applyType == 5 ? 5 : 3)
    template.gonewpagebyrd('/pages/store/store_tixian/tixian_record?storeid=' + _rd.storeId + '&applyType=' + applyType)
  },
  choose_tixianType: function(e) {
    var that = this
    var index = e.currentTarget.id
    var id = e.currentTarget.dataset.id
    let _t = this.data.tixian_type
    for (let i = 0; i < _t.length; i++) {
      if (i == index) {
        that.count_tixianMoney(_t[i].fee / 100, _t[i].money)
        that.data.requestData.applyType = id
        that.data.requestData.cashStr = _t[i].money
        that.data.requestData.cash = _t[i].money
        _t[i].ischoose = true
        that.setData({
          fee: _t[i].fee
        })
      } else {
        _t[i].ischoose = false
      }
    }
    that.setData({
      tixian_type: _t,
      requestData: that.data.requestData,
    })
  },
  goback_nt: function() {
    template.goback(1)
  },
  input_tixianMoney: function(e) {
    var _rd = this.data.requestData
    if (Number(e.detail.value) > Number(_rd.cashStr)) {
      this.setData({
        'requestData.cash': 0
      })
    } else {
      this.setData({
        'requestData.cash': e.detail.value,
      })
      this.count_tixianMoney(this.data.fee / 100, e.detail.value)
    }
  },
  count_tixianMoney: function(fee, actuallyMoney) {
    var tixian_fee = ((Number(actuallyMoney) * Number(fee)).toFixed(2) < '0.00' ? '0.01' : (Number(actuallyMoney) * Number(fee)).toFixed(2))
    var actually_money = (Number(actuallyMoney) - Number(tixian_fee)).toFixed(2)
    this.setData({
      tixian_fee: tixian_fee,
      actually_money: actually_money
    })
  },
  application_tixian: function() {
    var that = this
    var _rd = that.data.requestData
    if (Number(_rd.cash) < Number(that.data.tixian_peizhi.minTxMoney)) {
      wx.showModal({
        title: '提示',
        content: '金额低于' + that.data.tixian_peizhi.minTxMoney + '时不可提现。',
        showCancel: false
      })
      return
    }
    if (Number(_rd.cash) <= 0) {
      template.showtoast('请输入正确的金额数', 'none')
      return
    }
    var data = _rd
    data.aid = app.globalData.aid
    data.cash = data.cash * 100
    http.pRequest(addr.AddApply, data, function(callback) {
      that.setData({
        is_tixian: !that.data.is_tixian
      })
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    that.setData({
      isAgent: options.isAgent || 0,
      'tixian_type[0].isopen': options.isAgent ? false : true,
      'tixian_type[1].isopen': options.isAgent ? false : true,
      'tixian_type[2].isopen': options.isAgent ? true : false
    })
    that.data.requestData.storeId = Number(options.storeid)
    app.login(function() {
      var data = {}
      data.aid = app.globalData.aid
      data.storeId = options.storeid
      that.beginload(data)
    })
  },
  beginload: function(data) {
    var that = this
    http.gRequest(addr.PlatFormInfo, data, function(callback) {
      var _t = callback.data.obj
      var txType = that.data.tixian_type
      txType[0].content = '店内扫码交易（手续费' + Number(_t.qrcodeServiceFee) + '%）'
      txType[1].content = '平台交易（手续费' + Number(_t.serviceFee) + '%）'
      txType[2].content = '代理收益（手续费' + Number(_t.agentServiceFee) + '%）'
      txType[0].fee = Number(_t.qrcodeServiceFee)
      txType[1].fee = Number(_t.serviceFee)
      txType[2].fee = Number(_t.agentServiceFee)

      http.gRequest(addr.CashDetail, {
        aid: app.globalData.aid
      }, function(callback) {
        var _m = callback.data.obj
        txType[0].money = _m.qrcodeCash
        txType[1].money = _m.cash
        txType[2].money = _m.agentCash

        that.setData({
          tixian_peizhi: _t,
          tixian_type: that.data.tixian_type,
          'requestData.cash': that.data.isAgent == 1 ? _m.agentCash : _m.qrcodeCash,
          'requestData.cashStr': that.data.isAgent == 1 ? _m.agentCash : _m.qrcodeCash,
          'requestData.applyType': that.data.isAgent == 1 ? 5 : 4,
          fee: that.data.isAgent == 1 ? that.data.tixian_type[2].fee : that.data.tixian_type[0].fee
        })
        var i = (that.data.isAgent == 1 ? 2 : 0)
        that.count_tixianMoney(that.data.tixian_type[i].fee / 100, that.data.tixian_type[i].money)
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

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function() {

  }
})