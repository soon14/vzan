const util = require("../../utils/util.js");
const addr = require("../../utils/addr.js");
const app = getApp()
Page({


  data: {
    navTitle: "积分记录",
    listViewModel: {
      pageindex: 1,
      pagesize: 10,
      list: [],
      ispost: false,
      loadall: false,
    },
    orderType: ["购买商品", "积分兑换", "储值充值"],
    imgSrc: [
      "http://j.vzan.cc/miniapp/img/enterprise/integral-record-01.png", 
      "http://j.vzan.cc/miniapp/img/enterprise/integral-record-03.png", 
      "http://j.vzan.cc/miniapp/img/enterprise/integral-record-02.png"
      ],
  },
  onLoad: function (options) {
    let navTitle = this.data.navTitle;
    util.navBarTitle(navTitle);
    this.GetUserIntegralLogs()
  },
  GetUserIntegralLogs: function () {
    let that = this;
    let vm = that.data.listViewModel
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    wx.request({
      url: addr.Address.GetUserIntegralLogs,
      data: {
        appId: app.globalData.appid,
        userId: app.globalData.userInfo.UserId,
        pageindex: vm.pageindex,
        pagesize: vm.pagesize
      },
      method: "POST",
      header: {
        'content-type': 'application/x-www-form-urlencoded'
      },
      success: function (res) {
        vm.ispost = false;
        if (res.data.isok) {
          if (res.data.model.listLog.length >= vm.pagesize) {
            vm.pageindex += 1;
          }
          else {
            vm.loadall = true;
          }
          if (res.data.model.listLog.length > 0) {
            vm.list = vm.list.concat(res.data.model.listLog);
          }
          let integralRecordList = vm.list
          //res.data.model.listLog
          that.setData({
            integralRecordList: integralRecordList
          })
        }
      },
      fail: function (res) {
        wx.showModal({
          title: '获取积分信息失败',
          content: res.data.msg,
        })
      }
    })
  },
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
    let that = this
    wx.showToast({
      title: '正在刷新',
      duration: 1000,
      icon: "loading",
      success: function () {
        Object.assign(that.data.listViewModel, { pageindex: 1, list: [], ispost: false, loadall: false });
        that.GetUserIntegralLogs()
      }
    })
    setTimeout(function () {
      wx.showToast({
        title: '刷新成功',
        duration: 500,
        icon: "success"
      })
    wx.stopPullDownRefresh()
    }, 1000)
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    this.GetUserIntegralLogs()
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  }
})