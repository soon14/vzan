// pages/stored/stored.js]
const tools = require("../../utils/tools.js")
const page = require("../../utils/pageRequest.js")
const utils = require("../../utils/util.js")
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
    page.userOfmoneyRequest(app.globalData.userInfo.openId, this)
    this.setData({ user: app.globalData.userInfo })
    page.moneyListRequest(this)
    utils.setPageSkin(this);
  },

  //充值请求
  getMoneyRequest: function (e) {
    var that = this
    var saveMoneySetId = e.currentTarget.dataset.id
    page.moneyRequest(saveMoneySetId).then(data => {
      if (data.isok) {
        that.wxpaymoney(data.orderid)
      }
    })
  },
  wxpaymoney: function (oradid) {
    let that = this
    var newparam = {
      openId: app.globalData.userInfo.openId,
      orderid: oradid,
      'type': 1,
    }
    utils.PayOrder(newparam, {
      failed: function () {
        tools.showLoadToast("您取消了支付")
      },
      success: function (res) {
        if (res == "wxpay") {
        } else if (res == "success") {
          tools.showToast("支付成功")
          page.userOfmoneyRequest(app.globalData.userInfo.openId, that)
        }
      }
    })
  },

  hisStoredGoto: function () {
    tools.goNewPage('../historyStored/historyStored')
  },


  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    var that = this
    tools.showLoadToast("正在刷新")
    setTimeout(res => {
      tools.showToast("刷新成功")
      page.moneyListRequest(that)
    }, 1000)
    wx.stopPullDownRefresh()
  },

})