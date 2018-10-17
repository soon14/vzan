// pages/setAddress/setAddress.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {

    addressInfo: '',//详细地址
    openId: '',
    // 地址模板
    addressList: [],
    item: [
      { location: '啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊', nickname: '李四', phone: '18825030524' },
      { location: '啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊', nickname: '李四', phone: '18825030524' },
    ],
    ifismeis: 5
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    if (options.isMe != undefined) {
      that.setData({ ifismeis: parseInt(options.isMe) })
    }
    if (app.globalData.openid == '') {
      app.getUserInfo(function (e) {
        that.setData({ openId: e.openId, addressInfo: app.globalData.addressInfo })
        that.inite()
      })
    }
  },
  // 定位
  chooseLocation: function () {
    var that = this
    wx.chooseLocation({
      success: function (res) {
        var weidu = res.latitude
        var jingdu = res.longitude
        var addressInfo = res.address
        app.globalData.weidu = weidu
        app.globalData.jingdu = jingdu
        app.globalData.addressInfo = addressInfo
        that.setData({
          addressInfo: addressInfo
        })
      },
    })
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },
  // 跳转到收货地址编辑页面
  navtoEditorAddress: function (e) {
    var Id = e.currentTarget.id
    wx.navigateTo({
      url: '../editorAddress/editorAddress?Id=' + Id + '&addressInfo=' + this.data.addressInfo,
    })
  },
  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
    this.inite()
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
  navtoupdateOrder: function (e) {
    var dizhiId = e.currentTarget.id
    var ifismeis = this.data.ifismeis
    app.globalData.dizhiId = dizhiId
    if (ifismeis == 5) {
      wx.navigateBack({
        url: '../updateOrder/updateOrder',
      })
    }
  },
  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  },
  //获取我的地址列表
  inite: function (e) {
    var that = this
    wx.request({
      url: addr.Address.GetMyAddress,
      data: {
        AppId: app.globalData.appid,
        openid: that.data.openId,
        isDefault: 0,
        addressId: '',
      },
      method: "GET",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function (res) {
        if (res.data.isok == 1) {
          that.setData({
            addressList: res.data.postdata.addressList
          })
          console.log('1', res)
        }
      },
      fail: function () {
        console.log("获取菜类分类出错")
        wx.showToast({
          title: '获取菜类出错',
        })
      }
    })
  },
})