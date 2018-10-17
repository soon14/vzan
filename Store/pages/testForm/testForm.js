// pages/testForm/testForm.js
Page({

  /**
   * 页面的初始数据
   */
  data: {

  },
  formSubmit: function (e) {
    var that = this
    var vipwindow = this.data.vipwindow
    // for (var i = 0; i < 3; i++) {
      wx.request({
        url: 'http://testwtApi.vzan.com/apimiappstores/testCommitForm',
        data: {
          FormId: 1
        },
        method: "GET",
        header: {
          'content-type': "application/json"
        },
        success: function (res) {
          console.log('我是formid爸爸', e)
          console.log('我是formid爸爸+i', e.detail.formId)
        },
        fail: function (e) {

          console.log('一键分享获取失败')
        }
      })
    // }
  },
  chooseAddress: function () {
    var that = this
    wx.chooseAddress({
      success: function (res) {
        that.formSubmit(e)
      }
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {

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
  onShareAppMessage: function () {

  }
})