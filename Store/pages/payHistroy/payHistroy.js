// pages/payHistroy/payHistroy.js
var addr = require("../../utils/addr.js");
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
  data: {
    saveMoneyUserLogList: [],// 数据集合
    item: [
      { condition: '购买商品', name: '冲50送300', yue: '975', shijian: '2017.09.11 9:00:22', jieguo: '-315' },
      { condition: '购买商品', name: '购买商品', yue: '975', shijian: '2017.09.11 9:00:22', jieguo: '+505' },
    ]
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    that.inite()
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

  },
  // 获取储值记录列表
  inite: function () {
    var that = this
    wx.request({
      url: addr.Address.getSaveMoneySetUserLogList, //仅为示例，并非真实的接口地址
      data: {
        appid: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
      },
      method: "GET",
      header: {
        'content-type': 'application/x-www-form-urlencoded' // 默认值
      },
      success: function (res) {
        if (res.data.isok == 1) {
          var saveMoneyUserLogList = res.data.saveMoneyUserLogList
          for (var i = 0; i < saveMoneyUserLogList.length; i++) {
            saveMoneyUserLogList[i].ChangeMoneyStr = (parseFloat(saveMoneyUserLogList[i].ChangeMoney) / 100).toFixed(2)
            saveMoneyUserLogList[i].AfterMoneyStr = (parseFloat(saveMoneyUserLogList[i].AfterMoney) / 100).toFixed(2)
          }
          that.setData({
            saveMoneyUserLogList: saveMoneyUserLogList
          })
          console.log(saveMoneyUserLogList)
        }
      },
      fail: function () {
        console.log('获取不了会员信息')
      }
    })
  },
})