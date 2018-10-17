// pages/share/share.js
Page({

  /**
   * 页面的初始数据
   */
  data: {
    ShareImg: '',
    tempFilePath: '',
  },
  // 保存图片
  savephoto: function () {
    var that = this
    if (that.data.tempFilePath == '') {

      wx.downloadFile({
        url: this.data.ShareImg,
        success: function (res) {
          that.setData({ tempFilePath: res.tempFilePath })
        }
      })
    }
    // 保存图片到相册
    wx.saveImageToPhotosAlbum({
      filePath: that.data.tempFilePath,
      success(res) {
        wx.showToast({
          title: '保存成功',
        })
      },
      fail(res) {
        wx.showModal({
          title: '图片下载中，请再次点击保存。',
          content: that.data.tempFilePath,
          showCancel: false,
          // content: '1'
        })
      }
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    var ShareImg = options.ShareImg.replace(/http/, "https")
    that.setData({
      ShareImg: ShareImg
    })
    // 保存图片到本地
    wx.downloadFile({
      url: this.data.ShareImg, //仅为示例，并非真实的资源
      success: function (res) {
        that.setData({ tempFilePath: res.tempFilePath })
      }
    })
  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {

  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function () {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function () {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function (res) {
    return {
      path: '/pages/index/index'
    }
  }
})