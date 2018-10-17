var net = require('../../utils/net.js');
var util = require('../../utils/util.js');
var app = getApp()
Page({
  data: {
    currentPageIndex: 1,
    // netStateBean: new net.netStateBean(),
    isShowQRCode: false,
    userInfo: {},
    backimg: '',
  },
  navo_webview: function() {
    wx.navigateTo({
      url: '../web_view/web_view?id=' + this.data.AgentConfig.QrcodeId
    })
  },
  onLoad: function() {
    app.IsShowBottomLogo(this)
    // 页面初始化 options为页面跳转所带来的参数
    this.setData({
      windowWidth: app.globalData.windowWidth,
      windowHeight: app.globalData.windowHeight,
      pixelRatio: app.globalData.pixelRatio,
    })
    this.loadDataFormNet()
  },
  set_isShowQRCode: function() {
    this.setData({
      isShowQRCode: false
    })
  },
  onItemClick: function(e) {
    var row = e.target.dataset.index;
    var cell = this.data.items[row]
    var that = this
    switch (row) {
      case 0:
        that.data.isShowQRCode = !that.data.isShowQRCode
        that.setData(that.data)
        break
      case 1:
        wx.openLocation({
          latitude: that.data.userInfo.latitude,
          longitude: that.data.userInfo.longitude,
          scale: 18
        })
        break
      case 2:
        wx.makePhoneCall({
          phoneNumber: cell.title
        })
        break
    }
  },
  loadDataFormNet: function() {
    var that = this
    var params = {
      appid: app.globalData.appid,
      level: '7'
    }
    util.showLoadingDialog()
    net.POST(net.Address.GET_MODEL_DATA, params, {
      success: function(res, code) {
        util.hideLoadingDialog()
        util.stopPullDownRefresh()
        var items = [{
            image: '/images/QRCode.png',
            title: '扫一扫，直接聊',
          },
          {
            image: '/images/location.png',
            title: '选择定位区域',
          },
          {
            image: '/images/phone.png',
            title: '暂无信息~',
          },
        ]
        var userInfo = {}
        var backimg = ''
        if (res != null && res.data.length > 0) {
          var object = res.data[0]

          backimg = object.LitleImgUrl
          if (!util.isOptStrNull(object)) {
            items[1].title = object.Address
            items[2].title = object.mobile
            if (!util.isOptStrNull(object.AddressPoint)) {
              var points = object.AddressPoint.split(",") || ['0', '0'];
            }
            var userInfo = {
              avatarUrl: '../../images/icon.png',
              nickName: object.Title ? object.Title : "暂无信息",
              location: object.Address,
              latitude: Number(points[1]),
              longitude: Number(points[0]),
              phone: object.mobile,
              codeImg: object.ImgUrl
            }
          }
        }

        that.setData({
          items: items,
          userInfo: userInfo,
          backimg: backimg,
        })
      },
      failure: function(error) {
        console.log(error)
        util.hideLoadingDialog()
        util.stopPullDownRefresh()
      }
    })
  },
  onShareAppMessage: function() {
    return {
      path: '/pages/mine/mine'
    }
  },
  onShow: function() {
    if (util.isOptStrNull(this.data.items)) {
      this.loadDataFormNet()
    }
  },
  onPullDownRefresh: function() {
    app.IsShowBottomLogo(this)
    wx.stopPullDownRefresh()
  },
  previewImg: function(e) {
    var imageArray = []
    imageArray.push(e.currentTarget.dataset.img)
    var previewImage = imageArray[0]
    console.log(previewImage)
    wx.previewImage({
      current: previewImage,
      urls: imageArray
    })

  },
})