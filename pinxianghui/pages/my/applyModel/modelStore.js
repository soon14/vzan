// pages/my/applyModel/modelStore.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    requestData: {
      pageIndex: 1
    }
  },
  storeInfo_nt: function(e) {
    var storeid = e.currentTarget.dataset.storeid
    template.gonewpage('/pages/store/storeIndex/storeIndex?storeid=' + storeid)
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var _rd = this.data.requestData
    this.BiaoganStoreList(this, _rd, 0)
  },
  BiaoganStoreList: function(that, data, isreachBottom) { //isreachBottom 0刷新 1加载更多
    wx.showNavigationBarLoading();
    data.utoken = getApp().globalData.utoken
    data.aid = getApp().globalData.aid
    wx.request({
      url: addr.BiaoganStoreList,
      data: data,
      method: "GET",
      header: {
        "content-type": "application/x-www-form-urlencoded"
      },
      success: function(res) {
        if (res.data.code == 1) {
          that.data.requestData = data
          wx.hideNavigationBarLoading();
          if (isreachBottom == 0) {
            that.setData({
              BiaoganList: res.data.obj.list
            })
            that.data.requestData.pageIndex = ++data.pageIndex
          } else {
            if (res.data.obj.list.length != 0) {
              that.data.BiaoganList = that.data.BiaoganList.concat(res.data.obj.list)
              that.setData({
                BiaoganList: that.data.BiaoganList,
              })
              that.data.requestData.pageIndex = ++data.pageIndex
            }
          }
        }
      },
      fail: function() {
        console.log("查询标杆店铺出错")
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
    this.BiaoganStoreList(this, this.data.requestData, 1)
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function() {

  }
})