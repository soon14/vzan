// pages/shopping/orderInfo/orderInfo.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
		item:3,
    getCode: false,
    haveConfirm: false,
  },
  copy_ordernum: function() {
    wx.setClipboardData({
      data: this.data.orderInfo.orderDetail.outTradeNo,
      success: function(res) {
        template.showtoast('复制成功', 'success')
      }
    })
  },
  storeInfo_nt: function() {
    template.gonewpage('/pages/store/storeIndex/storeIndex?storeid=' + this.data.requestData.storeId + '&fromPintuan=1')
  },
  goodInfo_nt: function() {
    template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + this.data.orderInfo.goodsInfo.id)
  },
  makephonecall: function() {
    var _o = this.data.orderInfo
    if (!_o.storeInfo.phone) {
      template.showtoast('该商家没有设置联系电话哦！', 'none')
      return
    }
    wx.makePhoneCall({
      phoneNumber: _o.storeInfo.phone,
    })
  },
  show_getcode: function() {
    this.setData({
      getCode: !this.data.getCode
    })
  },
  pintuaninfo_nt: function(e) {
    var groupid = e.currentTarget.dataset.groupid
    var storeid = e.currentTarget.dataset.storeid
    template.gonewpage('/pages/shopping/pintuanInfo/pintuanInfo?groupid=' + groupid + '&storeid=' + storeid)
  },
  confirm_getGood: function() {
    var that = this

    wx.showModal({
      title: '提示',
      content: '是否确认收货',
      success: function(res) {
        if (res.confirm) {
          var _ai = that.data.actionId
          var data = {}
          data.aid = app.globalData.aid
          data.storeId = _ai.storeId
          data.orderId = _ai.orderId
          http.pRequest(addr.OrderSuccess, data, function(callback) {
            that.data.haveConfirm = true
            template.showtoast('操作成功', 'success')
            that.beginload(data)
          })
        }
      }
    })

  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    var data = {}
    data.storeId = options.storeid
    data.orderId = options.orderid
    that.data.requestData = data
  },
  beginload: function(data) {
    var that = this
    var g = getApp().globalData;
    http.gRequest(addr.OrderDetail, data, function(callback) {
      var _d = callback.data.obj
      _d.groupDetail.endTimeStr = _d.groupDetail.endTimeStr.replace(/-/g, '/')
      if (!_d.users) {
        _d.users = []
      }
      var findjoin = _d.users.find(f => f.userid == g.userInfo.UserId)
      if (findjoin) {
        _d.isjoin = true
      }
      _d.users = _d.users || []
      for (let i = 0; i < _d.groupDetail.groupCount; i++) {
        if (i > _d.groupDetail.entrantCount - 1) {
          _d.users.push({})
        }
        _d.users[i].noneLogo = 'http://j.vzan.cc/miniapp/img/group2/icon_blankuser.png'
      }
      that.setData({
        orderInfo: _d,
        actionId: data
      })


      var et = new Date(_d.groupDetail.endTimeStr).getTime()
      var nt = new Date().getTime()
      var lessTime = et - nt
      if (lessTime > 0) {
        that.data.interval = setInterval(function() {
          lessTime = lessTime - 1000
          let endtimeStr = template.formatDuring(lessTime)
          that.setData({
            'orderInfo.groupDetail.endtimeStr': endtimeStr
          })
          console.log(endtimeStr)
        }, 1000)
      }
      if (that.data.haveConfirm && _d.groupDetail.state > 0) {
        wx.showModal({
          title: '提示',
          content: (_d.groupDetail.state == 1 ? '等待团友确认收货，系统自动返现。' : '拼团成功，系统正在返现，5分钟内到帐，请留意微信退款通知。'),
          showCancel: false
        })
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
    var that = this
    app.login(function(g) {
      that.data.requestData.aid = app.globalData.aid
      that.beginload(that.data.requestData)
    })
  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function() {
    clearInterval(this.data.interval)
  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function() {
    clearInterval(this.data.interval)
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
})