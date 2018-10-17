// pages/shopping/goodInfo/goodInfo.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var WxParse = require('../../../script/wxParse/wxParse.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    bottom_btn: [{
        fontcss: 'dzicon icon-shouye- cb5 t32 l44',
        content: ''
      },
      {
        fontcss: 'dzicon icon-icon_wodeguanzhu cb5 t40',
        content: '关注'
      },
      {
        fontcss: 'dzicon icon-WeChat_weixin cb5 t40',
        content: '客服'
      },
    ],
    showgoodInfo: false, //购物车弹窗
    showshareCard: false, //商品分享码弹窗
    shopcarData: { //购物车
      goodName: '',
      Price: 0,
      discountPrice: 0,
      reducePrice: 0,
      Number: 1,
      Stock: 0,
      imageUrl: '',
      specInfo: '请选择属性',
      Sort: [],
      Spec: [],
      Sortid: [],
      SpecinfoId: '',
      stockLimit: false,
    },

    aprase: [{
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN',
        data: '04-18 颜色分类：粉色 尺寸：S[标准码]',
        content: '裙子很好看，穿起来很仙，仙，仙出屎仙出屎仙出屎仙出屎仙出屎仙出屎'
      },
      {
        userlogo: '/image/ky.png',
        username: 'MAXIAOJIAN',
        data: '04-18 颜色分类：粉色 尺寸：S[标准码]',
        content: '裙子很好看，穿起来很仙，仙，仙出屎'
      },
    ], //评价
    goodtuijian: [{
        gimg: '/image/ky.png',
        gcontent: '卡西欧200米防水',
        price: '988.88'
      },
      {
        gimg: '/image/ky.png',
        gcontent: '条纹中裙潮流范',
        price: '988.88'
      },
      {
        gimg: '/image/ky.png',
        gcontent: '美图M9照亮你的美图M9照亮你的',
        price: '988.88'
      },
    ], //店长推荐
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
    ], //热销商品
  },
  go_applyEnter: function() {
    template.gonewpage('/pages/store/applyEnter/applyEnter')
  },
  swiper_previewImg: function(e) {
    let imageArray = this.data.pintuanInfo.imgAlbumList
    let img = e.currentTarget.dataset.url
    template.previewsingleimage(img, imageArray)
  },
  wxParseImgTap: function(e) {
    let src = e.currentTarget.dataset.src;
    template.previewsingleimage(src)
  },
  index_nt: function(e) {
    var that = this
    var index = e.currentTarget.id
    var _g = that.data.pintuanInfo
    var title = '关注成功'
    if (index == 0) {
      if (that.data.bottom_btn[0].content == '首页') {
        template.switchtab('/pages/index/index')
      } else {
        template.gonewpage('/pages/store/storeIndex/storeIndex?storeid=' + _g.storeId)
      }
    } else if (index == 1) {
      var data = that.data.goodLikesInfo
      data.likeId = _g.id
      if (data.id > 0) {
        title = '已取消关注'
      }
      http.pRequest(addr.AddLikes, data, function(callback) {
        template.showtoast(title, 'success')
        that.beginload(_g.id)
      })
    } else {

    }
  },
  store_nt: function(e) {
    template.gonewpage('/pages/store/storeIndex/storeIndex?storeid=' + e.currentTarget.dataset.storeid)
  },
  showgoodinfo: function() {
    this.setData({
      showgoodInfo: !this.data.showgoodInfo
    })
  },
  showshare_card: function() {
    var that = this
    var data = {}
    data.aid = app.globalData.aid
    // data.storeId = that.data.storeInfo.id
    data.type = 1
    data.scene = that.data.pintuanInfo.id + '_' + that.data.pintuanInfo.storeId
    http.gRequest(addr.GetQrCode, data, function(callback) {
      var _url = callback.data.obj.url
      that.draw_goodshareCard(_url)
    })
  },
  hideshare_card: function() {
    this.setData({
      showshareCard: false
    })
  },
  draw_goodshareCard: function(_url) { //画布
    var that = this
    var _g = that.data.pintuanInfo
    wx.downloadFile({
      url: _url.replace(/http:/, "https:"),
      success: function(res) {
        var storeQrCode = res.tempFilePath
        var grouprice = Number(_g.groupPrice / 100)

        const ctx = wx.createCanvasContext('gshare_Card')
        const wW = wx.getSystemInfoSync().windowWidth * 0.74
        const wH = wx.getSystemInfoSync().windowHeight * 0.5
        const phoneType = wx.getSystemInfoSync().model

        ctx.setFillStyle('#fff')
        ctx.fillRect(1, 0, wW, wH)
        drawText(_g.name, wW * 0.5, wH * 0.02, wW * 0.8); // 商品名字
        ctx.drawImage(storeQrCode, wW * 0.18, (phoneType == 'iPhone X' ? wH * 0.24 : wH * 0.19), wW * 0.64, wW * 0.58) //二维码
        //单买价
        ctx.setFontSize(14)
        ctx.textAlign = 'center'
        ctx.setFillStyle('#666666')
        ctx.fillText('单买价', wW * 0.25, wH * 0.82)
        ctx.setFillStyle('#666666')
        ctx.fillText('拼团立返', wW * 0.75, wH * 0.82)
        //价格
        ctx.textAlign = 'center'
        ctx.font = "bold 13px Arial"
        ctx.setFillStyle('#000')
        ctx.setFontSize(16)
        ctx.fillText('￥' + _g.priceStr, wW * 0.25, wH * 0.88)
        ctx.setFillStyle('#000')
        ctx.setFontSize(16)
        ctx.fillText('￥' + grouprice, wW * 0.75, wH * 0.88)

        ctx.draw()
        that.setData({
          showshareCard: true
        })
        //文字转行
        function drawText(t, x, y, w) {
          var chr = t.split("");
          var temp = "";
          var row = [];
          ctx.textAlign = 'center';
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
            ctx.fillText(row[0], wW * 0.5, wH * 0.02 + 16);
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
  },
  save_goodImg: function() {
    const phonePixel = wx.getSystemInfoSync().pixelRatio
    const wW = wx.getSystemInfoSync().windowWidth * 0.74 * phonePixel
    const wH = wx.getSystemInfoSync().windowHeight * 0.5 * phonePixel
    template.canvasToTempFilePath(0, 'gshare_Card', wW, wH, this)
  },
  payinfo_nt: function() {
    var _s = this.data.shopcarData
    if (_s.Number == 0) {
      template.showtoast('库存不足！', 'none')
      return
    }
    if (this.checkChoose(_s.Spec) && (_s.Spec.length == _s.Sort.length)) {
      template.gonewpagebyrd('/pages/shopping/payInfo/payInfo?shopcarData=' + JSON.stringify(_s))
    } else {
      template.showtoast('请选择规格', 'none')
    }
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    if (options.scene) {
      for (var key in options.scene) {
        if (options.scene[key] == '_') {
          var scene = options.scene.split('_')[0]
          break
        } else {
          var scene = options.scene
        }
      }
      app.globalData.sourcestoreid = options.scene.split('_')[1]
      app.globalData.sourcetime = new Date().getTime()
    }

    app.login(function(g) {
      that.beginload(options.gid || scene)
      that.setData({
        'shopcarData.groupId': options.groupid || 0,
        userInfo: g.userInfo,
        'bottom_btn[0].content': (options.scene ? '店铺' : '首页')
      })
    })
  },
  beginload: function(gid) {
    var that = this
    http.gRequest(addr.GoodInfo, {
      id: gid
    }, function(callback) {
      var _g = callback.data.obj.goodInfo
      var _s = that.data.shopcarData
      var _l = callback.data.obj.goodLikesInfo
      var _b = that.data.bottom_btn
      _g.pinGoodsCount = callback.data.obj.pinGoodsCount
      _g.pinUserCount = callback.data.obj.pinUserCount
      _g.imgAlbumList.unshift(_g.img)
      _g.specificationdetailArray = JSON.parse(_g.specificationdetail || '[]')
      _g.pickspecificationArray = JSON.parse(_g.pickspecification || '[]')
      for (let i = 0; i < _g.pickspecificationArray.length; i++) {
        _s.Sort.push(_g.pickspecificationArray[i].name)
      }
      _s.stockLimit = _g.stockLimit
      _s.storeId = _g.storeId
      _s.goodName = _g.name
      _s.gid = _g.id
      _s.Price = _g.priceStr
      if (_g.SpecificationDetailList.length == 0) {
        _s.imageUrl = _g.img
        _s.Stock = _g.stock
        _s.stockLimit = _g.stockLimit
        _s.Price = _g.priceStr
        _s.reducePrice = (_g.groupPrice / 100).toFixed(2)
        _s.specInfo = ''
      }
      _g.fuwenben = WxParse.wxParse('description', 'html', _g.description, that, 5)

      if (_l.id > 0) {
        _b[1].fontcss = 'dzicon icon-aixin cf0033 t32 l46'
      } //渲染红心
      else {
        _b[1].fontcss = 'dzicon icon-icon_wodeguanzhu cb5 t40'
      }
      that.setData({
        pintuanInfo: _g, //商品详情
        goodLikesInfo: _l, //关注对象
        bottom_btn: _b, //底部按钮
        shopcarData: _s
      })
      //请求店铺详情
      http.gRequest(addr.StoreInfo, {
        id: _g.storeId
      }, function(callback) {
        callback.data.obj.storeInfo.goodsCount = callback.data.obj.goodsCount
        callback.data.obj.storeInfo.pinGoodsCount = callback.data.obj.pinGoodsCount
        that.setData({
          storeInfo: callback.data.obj.storeInfo,
        })
      }, that)

    })
  },
  getUserInfo: function(e) {
    var that = this
    var _e = e.detail
    var g = getApp().globalData;
    if (_e.encryptedData && _e.iv && _e.signature) {
      wx.login({
        success: function(res) {
          app.loginByThirdPlatform(res.code, _e.encryptedData, _e.signature, _e.iv, function(cb) {
            if (cb) {
              app.login(function() {
                that.setData({
                  userInfo: g.userInfo
                })
              })
            }
          }, 0)
        }
      })
    }
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
    var _g = that.data.pintuanInfo
    var shareTitle = _g.name
    var shareDesc = ''
    var sharePath = 'pages/shopping/goodInfo/goodInfo?gid=' + _g.id
    var imageUrl = _g.img
    return {
      title: shareTitle,
      desc: shareDesc,
      path: sharePath,
      imageUrl: imageUrl,
    }
  },
  checkChoose: function(array) {
    var result = true
    for (let i = 0; i < array.length; i++) {
      if (array[i] == '') {
        result = false
        return
      }
    }
    return result
  },
  chooseLabels: function(e) { //选择规格
    var that = this
    let _e = e.currentTarget.dataset
    let sort_lineid = _e.sort_lineid
    let spec_lineid = _e.spec_lineid
    let sortname = _e.sortname
    let sortinfo = _e.sortinfo
    let specid = _e.specid
    let _pA = that.data.pintuanInfo.pickspecificationArray
    let _sA = that.data.pintuanInfo.specificationdetailArray
    let _s = that.data.shopcarData
    for (var i = 0; i < _pA[sort_lineid].items.length; i++) {
      if (i == spec_lineid) {
        if (_pA[sort_lineid].items[i].ischoose == true) {
          _pA[sort_lineid].items[i].ischoose = false //如果已经选定过 反选
          _s.Spec[sort_lineid] = ''
        } else {
          _pA[sort_lineid].items[i].ischoose = true
          _s.Spec[sort_lineid] = sortinfo
        }
      } else {
        _pA[sort_lineid].items[i].ischoose = false
      }
    }
    _s.Sortid[sort_lineid] = specid
    if (that.checkChoose(_s.Spec)) {
      if (_s.Sort.length == _s.Spec.length) {
        // 规则尺寸显示
        _s.specInfo = ''
        _s.SpecinfoId = ''
        for (var z = 0; z < _s.Spec.length; z++) {
          _s.specInfo += _s.Sort[z] + ':' + _s.Spec[z] + ' '
          _s.SpecinfoId += _s.Sortid[z] + (z == _s.Sortid.length - 1 ? '' : '_')
        }
        //遍历对应价格
        var fgood = _sA.find(f => f.id == _s.SpecinfoId)
        if (fgood) {
          _s.Price = (fgood.price / 100).toFixed(2)
          _s.reducePrice = (fgood.groupPrice / 100).toFixed(2)
          _s.Stock = fgood.stock
          _s.Number = (fgood.stock == 0 && that.data.shopcarData.stockLimit ? '0' : '1')
          _s.imageUrl = fgood.imgUrl || that.data.pintuanInfo.img
        }
      }
    } else {
      _s.specInfo = '请选择属性' //规格值没选满还原默认显示
      _s.SpecinfoId = ''
      _s.Price = that.data.pintuanInfo.priceStr
    }

    that.setData({
      pintuanInfo: that.data.pintuanInfo,
      shopcarData: _s
    })
    console.log('choose labels dataset', _e)
  },
  reduce_num: function() {
    var _s = this.data.shopcarData
    if (_s.Number > 1) {
      _s.Number--
    } else {
      template.showtoast('亲，不可以再减了！', 'none')
    }
    this.setData({
      shopcarData: _s
    })
  },
  add_num: function() {
    var _s = this.data.shopcarData
    if (_s.stockLimit) {
      if (_s.Number < _s.Stock) {
        _s.Number++
      } else {
        template.showtoast('亲，库存不足够多了！', 'none')
      }
    } else {
      _s.Number++
    }
    this.setData({
      shopcarData: _s
    })
  },
})