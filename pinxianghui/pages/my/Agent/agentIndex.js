// pages/my/Agent/agentIndex.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    share_code: false
  },
  showshare_code: function() {
    var that = this
    var data = {}
    data.aid = app.globalData.aid
    // data.storeId = that.data.userInfo.StoreId
    data.type = 4
    data.scene = this.data.userInfo.UserId
    http.gRequest(addr.GetQrCode, data, function(callback) {
      that.setData({
        codeUrl: callback.data.obj.url,
        share_code: true
      })
    })
  },
  hideshare_code: function() {
    this.setData({
      share_code: false
    })
  },
  copy_url: function() {
    wx.setClipboardData({
      data: 'https://pan.baidu.com/s/1WzsMkWFnIwMsrQR1twrsfw',
    })
  },
  tixian_nt: function() {
    template.gonewpage('/pages/store/store_tixian/store_tixian?storeid=' + this.data.userInfo.StoreId + '&isAgent=1')
  },
  tixianRecord_nt: function() {
    template.gonewpage('/pages/store/store_tixian/tixian_record?storeid=' + this.data.userInfo.StoreId + '&applyType=5')
  },
  agentCount_mt: function(e) {
    template.gonewpage('/pages/my/Agent/agentCount?requestType=' + e.currentTarget.id)
  },
  download_code: function() {
    var that = this
    wx.downloadFile({
      url: that.data.codeUrl.replace(/http:/, "https:"),
      success: function(res) {
        var imageUrl = res.tempFilePath
        that.save_img(imageUrl)
      }
    })
  },
  save_img: function(url) {
    var that = this
    wx.saveImageToPhotosAlbum({
      filePath: url,
      success(res) {
        template.showtoast('保存图片成功', 'success')
      },
      fail: function(res) {
        if (res.errMsg = 'saveImageToPhotosAlbum:fail auth deny') {
          that.setData({
            AlbumUnset: true
          })
        }
      }
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    this.beginload()
  },
  beginload: function() {
    var that = this
    app.login(function(g) {
      that.setData({
        userInfo: g.userInfo
      })

      http.gRequest(addr.GetAgentIncome, {}, function(callback) {
        that.setData({
          myAgentInfo: callback.data.obj
        })
      })

      var data = {}
      data.aid = app.globalData.aid
      data.storeId = g.userInfo.StoreId
      http.gRequest(addr.PlatFormInfo, data, function(callback) {
        that.setData({
          dianpu_peizhi: callback.data.obj
        })
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
    this.beginload()
    template.stoppulldown()
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function(e) {
    var that = this
    var index = e.target.id
    var shareTitle = '门店业绩想翻倍，就用拼享惠'
    var shareDesc = '申请加入拼享惠'
    // isInvite = ' + index + 
    var sharePath = 'pages/store/applyEnter/applyEnter?fuserId=' + that.data.userInfo.UserId
    // var imageUrl = 'http://i.vzan.cc/image/jpg/2018/7/10/1629075d49d315347144fda4680bafb01b4b34.jpg'
    var imageUrl = 'http://i.vzan.cc/image/jpg/2018/7/23/15330231288f2fec92411099489b69dee49db7.jpg'
    return {
      title: shareTitle,
      desc: shareDesc,
      path: sharePath,
      imageUrl: imageUrl,
      success: function() {
        console.log(sharePath)
      }
    }
  }
})