// pages/store/storeIndex/storeIndex.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    fromPintuan: 0, //0没有限制 1限制
    typeid: 0, //分类id 
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
    storeShow: true, //true店铺已开放 false店铺未开放
    storeCode: false, //分享店铺二维码
    isme: 0, //渲染假底部导航栏 0店铺主页 1店铺搜索 2我的
    good_vm: { //请求商品列表
      storeId: 0,
      cateIdOne: 0,
      cateId: 0,
      pageSize: 20,
      pageIndex: 1,
      kw: '',
    },

    goods: [{
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
    hotsell: [{
        userlogo: '/image/ky.png',
        username: '阿萨德撒打打打d阿萨德',
        lesstime: '00:40:30',
        gimg: '/image/ky.png',
        price: '154'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN',
        lesstime: '00:40:30',
        gimg: '/image/ky.png',
        price: '154'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN',
        lesstime: '00:40:30',
        gimg: '/image/ky.png',
        price: '154'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN',
        lesstime: '00:40:30',
        gimg: '/image/ky.png',
        price: '154'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN',
        lesstime: '00:40:30',
        gimg: '/image/ky.png',
        price: '154'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN',
        lesstime: '00:40:30',
        gimg: '/image/ky.png',
        price: '154'
      },
    ],
  },
  go_applyEnter: function() {
    template.gonewpage('/pages/store/applyEnter/applyEnter')
  },
  searchClassify_nt: function() {
    template.gonewpage('/pages/classify/search_classify/search_classify?storeid=' + this.data.storeInfo.id)
  },
  nt_goodinfo: function(e) {
    var gid = e.currentTarget.dataset.gid
    template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + gid)

    // if (getCurrentPages().length > 1) {
    //   if (getCurrentPages()[getCurrentPages().length - 1].route == getCurrentPages()[getCurrentPages().length - 2].route) {
    //     template.gonewpagebyrd('/pages/shopping/goodInfo/goodInfo?gid=' + gid)
    //   } else {
    //     template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + gid)
    //   }
    // } else {
    //   template.gonewpage('/pages/shopping/goodInfo/goodInfo?gid=' + gid)
    // }
  },
  addlike_store: function() {
    var that = this
    var _s = that.data.storeInfo
    var title = '收藏成功'
    var data = that.data.storeLikesInfo
    data.likeId = _s.id
    if (data.id > 0) {
      title = '已取消收藏'
    }

    http.pRequest(addr.AddLikes, data, function(callback) {
      template.showtoast(title, 'success')
      that.data.good_vm.pageIndex = 1
      that.beginload(_s.id)
    })
  },
  index_nt: function() {
    template.switchtab('/pages/index/index')
  },
  url_nt: function(e) {
    var index = e.currentTarget.dataset.id
    var storeid = this.data.storeInfo.id
    if (index == 0) {
      if (getCurrentPages()[getCurrentPages().length - 1].route == "pages/store/storeIndex/storeIndex") {
        return
      }
      template.gonewpagebyrd('/pages/store/storeIndex/storeIndex?storeid=' + storeid)
    } else if (index == 1) {
      template.gonewpagebyrd('/pages/store/store_classify/store_classify?storeid=' + storeid)
    } else {
      template.gonewpagebyrd('/pages/store/store_my/store_my?storeid=' + storeid)
    }
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    if (getCurrentPages()[getCurrentPages().length - 1].route == "pages/store/storeIndex/storeIndex") {
      that.setData({
        isme: 0
      })
    }
    app.login(function() {
      that.beginload(options.storeid || options.scene)
      if (options.scene) {
        app.globalData.sourcestoreid = options.scene
        app.globalData.sourcetime = new Date().getTime()
      }
      that.data.fromPintuan = options.fromPintuan || 0
    })
  },
  beginload: function(storeid) {
    var that = this
    var g = getApp().globalData;
    http.gRequest(addr.StoreInfo, {
      id: storeid
    }, function(callback) {
      var _s = callback.data.obj.storeInfo
      var _l = callback.data.obj.storeLikesInfo
      _s.goodsCount = callback.data.obj.goodsCount
      _s.pinGoodsCount = callback.data.obj.pinGoodsCount
      if (_l.id > 0) {
        _s.guanzhuCss = 'cf0033'
      } else {
        _s.guanzhuCss = 'c666'
      }
      that.setData({
        storeInfo: _s, //店铺详情
        storeLikesInfo: _l //店铺收藏标记
      })

      http.gRequest(addr.CategoryList, {
        aId: g.aid,
        storeId: _s.id
      }, function(callback) {
        let _f = callback.data.obj
        var _g = that.data.good_vm
        _g.storeId = _s.id
        http.GoodList(that, _g, 0)
        that.setData({
          fenleiList: _f
        })
      })

    }, that)
  },
  search_goods: function(e) {
    var that = this
    var _g = that.data.good_vm
    _g.pageIndex = 1
    _g.storeId = that.data.storeInfo.id
    _g.cateIdOne = e.currentTarget.dataset.typeid
    http.GoodList(that, _g, 0)
    this.setData({
      typeid: e.currentTarget.dataset.typeid
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
    template.checksetting(this)
    // var findPageIndex = getCurrentPages().findIndex(f => f.route == 'pages/store/storeIndex/storeIndex')
    // if (findPageIndex && getCurrentPages().length > 2 && this.data.fromPintuan == 0 && getCurrentPages()[findPageIndex].route != 'pages/store/storeIndex/storeIndex') {
    //   setTimeout(function() {
    //     template.goback(getCurrentPages().length - findPageIndex - 1)
    //   }, 500)
    // }
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
    var _g = this.data.good_vm
    http.GoodList(this, _g, 1)
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function() {
    var that = this
    var _s = that.data.storeInfo
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
  // 保存画布图片
  sharecard_action: function(e) {
    var index = e.currentTarget.id
    const phonePixel = wx.getSystemInfoSync().pixelRatio
    const wW = wx.getSystemInfoSync().windowWidth * 0.72 * phonePixel
    const wH = wx.getSystemInfoSync().windowHeight * 0.5 * phonePixel
    template.canvasToTempFilePath(index, 'shopCard', wW, wH, this)
  },
  hide_storecode: function() {
    this.setData({
      storeCode: false
    })
  },
  show_storecode: function() {
    var that = this
    wx.showLoading({
      title: '图片生成中....'
    })
    var data = {}
    data.aid = app.globalData.aid
    // data.storeId = that.data.storeInfo.id
    data.type = 2
    data.scene = that.data.storeInfo.id
    http.gRequest(addr.GetQrCode, data, function(callback) {
      var _url = callback.data.obj.url
      that.loadcanvas(_url)
    })
  },
  loadcanvas: function(_url) { //画布
    var that = this
    var _s = that.data.storeInfo
    var store_logo = ''
    if ((_s.logo.substring(0, 20) == 'https://wx.qlogo.cn/') || !_s.logo) {
      store_logo = 'http://i.vzan.cc/image/jpg/2018/7/23/15330231288f2fec92411099489b69dee49db7.jpg'
    } else {
      store_logo = _s.logo
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
            drawText((_s.storeName || app.globalData.userInfo.nickName), (phoneType == 'iPhone X' ? wW * 0.34 : wW * 0.3), wH * 0.03, wW * 0.6)
            // 店铺基本信息
            ctx.setFontSize(12)
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
})