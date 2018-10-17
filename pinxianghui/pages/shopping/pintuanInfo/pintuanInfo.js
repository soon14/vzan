// pages/shopping/pintuanInfo/pintuanInfo.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    show_moreUser: false, //拼团人数太多进行隐藏
    invite_modal: false, //邀请好友弹窗
    cardbottom_btn: [{
        fontcss: 'dzicon icon-icon_fenxiang-1',
        content: '分享好友',
        bgcolor: 'bgc9e40',
        id: 2
      },
      {
        fontcss: 'dzicon icon-Circleoffriends_pengyou',
        content: '分享朋友圈',
        bgcolor: 'bgc040',
        id: 1
      },
      {
        fontcss: 'dzicon icon-Choice_xuanze',
        content: '保存图片',
        bgcolor: 'bgc6b',
        id: 0
      },
    ],


    item: [{
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN'
      },
      {
        userlogo: 'http://j.vzan.cc/miniapp/img/group2/icon_blankuser.png',
        username: 'MAXIAOJIAN'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN'
      },
    ], //每组都push一个问号图片进去，wxml页面使用 || 去渲染
    item2: [{
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装彩妆十件套装彩妆十件套装彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
      {
        img: 'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
        topic: '彩妆十件套装',
        price: '1540',
        sellnums: '240'
      },
    ],
  },
  goodInfo_nt: function() {
    template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + this.data.pintuanInfo.goodsInfo.id)
  },
  showmore_User: function() {
    this.setData({
      show_moreUser: !this.data.show_moreUser
    })
  },
  storeindex_nt: function() {
    var storeid = this.data.pintuanInfo.goodsInfo.storeId
    template.gonewpage('/pages/store/storeIndex/storeIndex?storeid=' + storeid + '&fromPintuan=1')
  },
  goodinfo_nt: function(e) {
    var gid = e.currentTarget.dataset.gid
    var groupid = e.currentTarget.dataset.groupid
    template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + gid + '&groupid=' + groupid)
  },
  orderinfo_nt: function(e) {
    var orderid = e.currentTarget.dataset.orderid
    var storeid = e.currentTarget.dataset.storeid
    template.gonewpage('/pages/shopping/orderInfo/orderInfo?orderid=' + orderid + '&storeid=' + storeid)
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    if (options.scene) {
      var idArray = options.scene.split('_')
    }
    var data = {}
    data.storeId = options.storeid || idArray[0]
    data.groupId = options.groupid || idArray[1]
    that.data.requestData = data
    that.data.isPayInfo = options.ispayInfo || 0
    console.log(options)
  },
  beginload: function(data) {
    var that = this
    var g = getApp().globalData;
    http.gRequest(addr.GroupDetail, data, function(callback) {
      var _d = callback.data.obj
      if (_d.groupDetail.state == -1) { //分享了出去的团又交易失败的处理
        wx.showModal({
          title: '提示',
          content: '该团已失效！',
          showCancel: false,
          confirmText: '返回首页',
          success: function(res) {
            if (res.confirm) {
              template.switchtab('/pages/index/index')
            }
          }
        })
        return
      }
      _d.groupDetail.endtime = _d.groupDetail.endtime.replace(/-/g, '/')
      var findjoin = _d.users.find(f => f.userid == g.userInfo.UserId)
      if (findjoin) {
        _d.isjoin = true
      }
      for (let i = 0; i < _d.groupDetail.groupCount; i++) {
        if (i > _d.groupDetail.entrantCount - 1) {
          _d.users.push({})
        }
        _d.users[i].noneLogo = 'http://j.vzan.cc/miniapp/img/group2/icon_blankuser.png'
      }
      that.setData({
        pintuanInfo: _d
      })
      var et = new Date(_d.groupDetail.endtime).getTime()
      var nt = new Date().getTime()
      var lessTime = et - nt
      if (lessTime > 0) {
        that.data.interval = setInterval(function() {
          lessTime = lessTime - 1000
          let endtimeStr = template.formatDuring(lessTime)
          that.setData({
            'pintuanInfo.groupDetail.endtimeStr': endtimeStr
          })
          console.log(endtimeStr)
        }, 1000)
      }
      if (that.data.isPayInfo == 1) {
        that.show_invite_modal()
      }
      that.data.requestData = data
      if (_d.groupDetail.state > 0 && _d.groupDetail.state != 3) {
        wx.showModal({
          title: '提示',
          content: (_d.groupDetail.state == 1 ? '等待团友确认收货，系统自动返现。' : '拼团成功，系统正在返现，5分钟内到帐，请留意微信退款通知。'),
          showCancel: false
        })
      }

      var order_data = {}
			order_data.orderId = _d.orderId
			order_data.storeId = _d.goodsInfo.storeId
			order_data.aid = app.globalData.aid
			http.gRequest(addr.OrderDetail, order_data, function(callback) {
        that.setData({
          orderInfo: callback.data.obj
        })
      })
    })

  },
  show_invite_modal: function() {
    var that = this
    var _rd = that.data.requestData
    var data = {}
    data.aid = app.globalData.aid
    data.type = 3
    data.scene = _rd.storeId + '_' + _rd.groupId
    http.gRequest(addr.GetQrCode, data, function(callback) {
      var _url = callback.data.obj.url
      that.draw_canvas(_url)
    })
  },
  hidden_invite_modal: function() {
    this.setData({
      invite_modal: !this.data.invite_modal
    })
  },
  draw_canvas: function(_url) {
    var that = this
    wx.downloadFile({
      url: _url.replace(/^http:/, "https:"),
      success: function(res) {
        var goodQrCode = res.tempFilePath
        const ctx = wx.createCanvasContext('groupShare_Card')
        const wW = wx.getSystemInfoSync().windowWidth * 0.72 * 0.8
        const wH = wx.getSystemInfoSync().windowHeight * 0.36
        var phoneType = wx.getSystemInfoSync().model
        ctx.setFillStyle('#fff')
        ctx.fillRect(0, 0, wW, wH)
        ctx.drawImage(goodQrCode, wW * 0.1, (phoneType == 'iPhone X' ? wH * 0.1 : 0), wW * 0.8, wW * 0.8) //二维码
        ctx.textAlign = 'center'
        ctx.setFillStyle('#666666')
        ctx.setFontSize(14)
        ctx.fillText('面对面扫码拼单', wW * 0.5, wH * 0.92)
        ctx.draw()
        if (that.data.pintuanInfo.groupDetail.state == 0) {
          that.setData({
            invite_modal: true
          })
        }
      }
    })

  },
  // 保存画布图片
  sharecard_action: function(e) {
    var index = e.currentTarget.id
    const wW = wx.getSystemInfoSync().windowWidth * 0.72 * 0.8
    const wH = wx.getSystemInfoSync().windowHeight * 0.36
    template.canvasToTempFilePath(index, 'groupShare_Card', wW, wH, this)
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
    template.checksetting(that)
    var g = getApp().globalData;
    app.login(function() {
      that.data.requestData.aid = g.aid
      that.setData({
        userInfo: g.userInfo
      })
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
    clearInterval(this.data.interval)
    this.beginload(this.data.requestData)
    template.stoppulldown()
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {

  },
  onShareAppMessage: function() {
    var that = this
    var _g = that.data.pintuanInfo
    var shareTitle = _g.goodsInfo.name
    var shareDesc = '快来和我一齐拼单，拼单立返' + (_g.goodsInfo.groupPrice / 100) + '元哦！'
    var sharePath = 'pages/shopping/pintuanInfo/pintuanInfo?storeid=' + _g.goodsInfo.storeId + '&groupid=' + _g.groupDetail.id
    var imageUrl = _g.goodsInfo.img
    return {
      title: shareTitle,
      desc: shareDesc,
      path: sharePath,
      imageUrl: imageUrl,
    }
  },
})