// pages/historyStored/historyStored.js
const tools = require("../../utils/tools.js")
const util = require("../../utils/util.js")
const page = require("../../utils/pageRequest.js")
const app = getApp();
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
    page.getHistoryRequest(this)
    util.setPageSkin(that);
  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast("正在加载")
    setTimeout(function () {
      tools.showToast("刷新成功")
      page.getHistoryRequest(that)
    }, 1000)
    wx.stopPullDownRefresh()
  },


})