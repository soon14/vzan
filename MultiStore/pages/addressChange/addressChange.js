// pages/addressChange/addressChange.js
const app = getApp();
const tools = require("../../utils/tools.js");
const pageRequest = require("../../utils/public-request.js")
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
  // 删除
  deleteFunc: function (e) {
    let that = this
    wx.showModal({
      title: '提示',
      content: '确定删除吗？',
      success: res => {
        if (res.confirm) { pageRequest.deleteAddress(that, e.currentTarget.dataset.id) }
        else { return }
      }
    })
  },
  /**
   * 页面跳转
   */
  addressGoto: function () {
    tools.goNewPage("../addressEdit/addressEdit")
  },
  editGoto: function (e) {
    tools.goNewPage("../addressEdit/addressEdit?id=" + e.currentTarget.dataset.id)
  },
  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
    pageRequest.getMyaddress(this, 0)
  },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    tools.showLoadToast("正在刷新")
    setTimeout(res => { tools.showToast("刷新成功"); pageRequest.getMyaddress(this, 0); wx.stopPullDownRefresh() }, 1000)
  },
})