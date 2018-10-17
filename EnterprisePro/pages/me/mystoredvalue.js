// pages/me/mystoredvalue.js
const page = require('../../utils/pageRequest.js')
const tools = require("../../utils/tools.js")
let app = getApp();


Page({

  /**
   * 页面的初始数据
   */
  data: {

  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {

  },
  init: function () {
    let that = this;
    let openid = wx.getStorageSync("userInfo").openId
    page.userOfmoneyRequest(openid, that);
  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
    this.init();
  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast('刷新中')
    setTimeout(function () {
      tools.showToast("刷新成功")
      that.init();
      wx.stopPullDownRefresh();
    }, 800);
  },

})