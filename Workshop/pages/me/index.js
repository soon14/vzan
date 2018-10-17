// pages/me/index.js
var app = getApp();
import { http, addr, tools } from "../../modules/core.js";
Page({

  /**
   * 页面的初始数据
   */
  data: {
    appConfig: "",
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this;
    that.setData({
      appConfig: app.getAppConfig()
    });
    app.getUserInfo().then(data => {
      that.setData({
        KEY_USERINFO: app.KEY_USERINFO,
        vm: {
          [app.KEY_USERINFO]: data
        }
      });
    }, data => {
      that.setData({
        KEY_USERINFO: app.KEY_USERINFO,
        vm: {
          [app.KEY_USERINFO]: null
        }
      });
    });
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
  mypage: function () {
    tools.backHome();
  },
  mytemppage: function () {
    wx.navigateTo({
      url: '/pages/index/pagelist?state=0',
    })
  },
  myFormPage:function(){
   wx.navigateTo({
     url: '/pages/index/formPage',
   })
  }
  /**
   * 用户点击右上角分享
   */
  // onShareAppMessage: function () {

  // }
})