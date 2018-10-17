// pages/storeInfo/storeInfo.js
// pages/index/index.js
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
    Logo:'',
    food:[],
    item: { image: '../../image/test.png', storename: '乐凯撒比萨（花城汇店）', sold: '596', txt: '尊敬的客官大人，今天是台风天气，为了更好地服务每一位顾客，请提前一个小时预订餐，如若餐厅送餐有延误，或有服务和品质不满的地方请致电020-38326733小乐都十分愿意为您解答，感谢您的体谅，谢谢！' },
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    that.inite()
  },
// 联系商家
  makePhonecall:function(){
wx.makePhoneCall({
  phoneNumber: this.data.food.TelePhone,
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
  onShareAppMessage: function () {

  },
  inite: function (e) {
    var that = this
    wx.request({
      url: addr.Address.GetFoodsDetail,
      data: {
        AppId: app.globalData.appid,
        // AppId: 307
      },
      method: "GET",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function (res) {
        if (res.data.isok == 1) {
          that.setData({
            food: res.data.postdata.food,
            Logo: res.data.postdata.Logo
          })
        }
      },
      fail: function () {
        console.log("获取信息出错")
        wx.showToast({
          title: '获取信息出错',
        })
      }
    })
  },
})