// pages/store/store.js
var http = require('../../script/pinxianghui.js');
var template = require('../../script/template.js');
var addr = require('../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    isgoIndex: false, //是否显示返回首页按钮
    storeCode: false, //显示店铺二维码
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
  },
  go_myproduct: function() {
    template.gonewpage('/pages/my/mgr/product/product?storeid=' + this.data.storeInfo.storeInfo.id)
  },
  go_applyEnter: function() {
    template.switchtab('/pages/index/index')
  },
  hide_storecode: function() {
    this.setData({
      storeCode: false
    })
  },
  makephonecall: function() {
    wx.makePhoneCall({
      phoneNumber: this.data.pingtai_peizhi.kfPhone,
    })
  },
  nt_tixian: function() {
    template.gonewpage('/pages/store/store_tixian/store_tixian?storeid=' + this.data.storeInfo.storeInfo.id)
  },
  tixianRecord_nt: function() {
    template.gonewpage('/pages/store/store_tixian/tixian_record?storeid=' + this.data.storeInfo.storeInfo.id + '&applyType=3')
  },
  edit_storeIndex_nt: function() {
    var _s = this.data.storeInfo.storeInfo
    var requestData = {
      id: _s.id,
      storeName: _s.storeName,
      phone: _s.phone,
      logo: _s.logo
    }
    template.gonewpage('/pages/store/edit_storeIndex/edit_storeIndex?requestData=' + JSON.stringify(requestData))
  },
  makePhoneCall: function() {
    wx.makePhoneCall({
      phoneNumber: this.data.storeInfo.storeInfo.phone,
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    //请求店铺详情
    app.login(function() {
      that.beginload(options.storeid)
    })
    if (getCurrentPages()[0].route == 'pages/store/store') {
      that.setData({
        isgoIndex: true
      })
    }
  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function() {

  },
  beginload: function(storeid) {
    var that = this
    http.gRequest(addr.StoreInfo, {
      id: storeid
    }, function(callback) {
      that.setData({
        storeInfo: callback.data.obj,
      })
    })
    http.gRequest(addr.PlatFormInfo, {
      aid: app.globalData.aid
    }, function(callback) {
      that.setData({
        pingtai_peizhi: callback.data.obj
      })
    })
  },
  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function(options) {
    template.checksetting(this)
    if (this.data.storeInfo) {
      var _s = this.data.storeInfo.storeInfo
      this.beginload(_s.id)
    }
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
    var that = this
    var _s = that.data.storeInfo.storeInfo
    var shareTitle = _s.storeName
    var shareDesc = ''
    var sharePath = 'pages/store/storeIndex/storeIndex?storeid=' + _s.id
    var imageUrl = _s.logo
    return {
      title: shareTitle,
      desc: shareDesc,
      path: sharePath,
      imageUrl: imageUrl,
    }
  },
  show_storeCode: function() {
    var that = this
    var _s = that.data.storeInfo.storeInfo
    wx.showLoading({
      title: '图片生成中....'
    })
    var data = {}
    data.aid = app.globalData.aid
    // data.storeId = _s.id
    data.type = 2
    data.scene = _s.id
    http.gRequest(addr.GetQrCode, data, function(callback) {
      var _url = callback.data.obj.url
      that.loadcanvas(_url)
    })
  },
  loadcanvas: function(_url) { //画布
    var that = this
    var _s = that.data.storeInfo
    var store_logo = ''
    if ((_s.storeInfo.logo.substring(0, 20) == 'https://wx.qlogo.cn/') || !_s.storeInfo.logo) {
      store_logo = 'http://i.vzan.cc/image/jpg/2018/7/23/15330231288f2fec92411099489b69dee49db7.jpg'
    } else {
      store_logo = _s.storeInfo.logo
    }
    wx.downloadFile({
      url: _url.replace(/http:/, "https:"),
      success: function(res0) {

        wx.downloadFile({
          url: (store_logo + '?x-oss-process=image/resize,l_100,image/circle,r_100/format,png').replace(/http:/, "https:"),
          success: function(res) {
            var storeQrCode = res0.tempFilePath
            var storeLogo = res.tempFilePath
            const ctx = wx.createCanvasContext('shopCard')
            const wW = wx.getSystemInfoSync().windowWidth * 0.72
            const wH = wx.getSystemInfoSync().windowHeight * 0.5
            const phoneType = wx.getSystemInfoSync().model

            // 灰色背景
            ctx.setFillStyle('#fff')
            ctx.fillRect(1, 0, wW, wH)
            ctx.setFillStyle('#f2f2f2')
            ctx.fillRect(1, 0, wW, wH * 0.24)
            ctx.drawImage(storeLogo, wW * 0.06, wH * 0.03, wH * 0.18, wH * 0.18) //店铺logo
            ctx.drawImage(storeQrCode, wW * 0.18, (phoneType == 'iPhone X' ? wH * 0.32 : wH * 0.28), wW * 0.64, wW * 0.58) //二维码
            // 店铺名称
            ctx.setFontSize(15)
            ctx.setFillStyle('#000')
            drawText((_s.storeInfo.storeName || app.globalData.userInfo.nickName), (phoneType == 'iPhone X' ? wW * 0.34 : wW * 0.3), wH * 0.03, wW * 0.6)
            // 店铺基本信息
            ctx.setFontSize(11)
            ctx.setFillStyle('#000')
            ctx.fillText('商品数量:' + _s.goodsCount, (phoneType == 'iPhone X' ? wW * 0.34 : wW * 0.3), wH * 0.2)
            // 长按识别小程序码进去店铺
            ctx.setFontSize(13)
            ctx.textAlign = 'center'
            ctx.setFillStyle('#666666')
            ctx.fillText('长按识别小程序码进去店铺', wW * 0.5, wH * 0.90)
            ctx.draw();
            wx.hideLoading()
            that.setData({
              storeCode: true
            })

            //文字转行
            function drawText(t, x, y, w) {
              var chr = t.split("");
              var temp = "";
              var row = [];
              ctx.textAlign = 'left';
              ctx.setFontSize(14);
              ctx.setFillStyle('#333333');
              for (var a = 0; a < chr.length; a++) {
                if (ctx.measureText(temp).width < w) {;
                } else {
                  row.push(temp);
                  temp = "";
                }
                temp += chr[a];
              }
              row.push(temp);
              if (row.length == 1) {
                ctx.fillText(row[0], (phoneType == 'iPhone X' ? wW * 0.34 : wW * 0.3), wH * 0.09);
              } else if (row.length == 2) {
                for (var b = 0; b < row.length; b++) {
                  ctx.fillText(row[b], x, y + (b + 1) * 16);
                }
              } else {
                row[1] = row[1].replace(row[1].substring(row[1].length - 2, row[1].length), '....')
                for (var b = 0; b < 2; b++) {
                  ctx.fillText(row[b], x, y + (b + 1) * 16);
                }
              }
            }
          }
        })
      }
    })
  },
  // 保存画布图片
  sharecard_action: function(e) {
    var index = e.currentTarget.id
    const phonePixel = wx.getSystemInfoSync().pixelRatio
    const wW = wx.getSystemInfoSync().windowWidth * 0.72 * phonePixel
    const wH = wx.getSystemInfoSync().windowHeight * 0.5 * phonePixel
    template.canvasToTempFilePath(index, 'shopCard', wW, wH, this)
  },
})