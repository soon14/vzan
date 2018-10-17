const util = require("../../utils/util.js");
const addr = require("../../utils/addr.js");
const app = getApp()
Page({

  data: {
    navTitle: "积分订单",
    listViewModel: {
      pageindex: 1,
      pagesize: 10,
      list: [],
      ispost: false,
      loadall: false,
    },
    state: ["", "", "待发货", "点击确认收货", "已完成"]
  },

  onLoad: function (options) {
    let navTitle = this.data.navTitle;
    util.navBarTitle(navTitle)
    this.GetExchangeActivityOrders()
  },
  confirmReceipt: function (e) {
    console.log(e)
    if (e.currentTarget.dataset.state == 3) {
      this.ConfirmReciveGood(e)
    }
  },
  //获取积分订单列表
  GetExchangeActivityOrders: function () {
    let that = this;
    let vm = that.data.listViewModel
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    wx.request({
      url: addr.Address.GetExchangeActivityOrders,
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
          if (res.data.model.listActivity.length >= vm.pagesize) {
            vm.pageindex += 1;
          }
          else {
            vm.loadall = true;
          }
          if (res.data.model.listActivity.length > 0) {
            vm.list = vm.list.concat(res.data.model.listActivity);
          }
          let integralOrderList = vm.list
          that.setData({
            integralOrderList: integralOrderList
          })
        }
      },
      fail: function (res) {
        wx.showModal({
          title: '获取订单信息失败',
          content: res.data.msg,
        })
      }
    })
  },
  //确认收货
  ConfirmReciveGood: function (e) {
    let that = this;
    wx.request({
      url: addr.Address.ConfirmReciveGood,
      data: {
        appId: app.globalData.appid,
        userId: app.globalData.userInfo.UserId,
        orderId: e.currentTarget.id
      },
      method: "POST",
      header: {
        'content-type': 'application/x-www-form-urlencoded'
      },
      success: function (res) {
        if (res.data.isok) {
            setTimeout(function () {
              wx.showToast({
                title: '收货成功',
                duration: 1200,
                icon: "success"
              })
            }, 800)
            Object.assign(that.data.listViewModel, { pageindex: 1, list: [], ispost: false, loadall: false });
            that.GetExchangeActivityOrders();
          }
        // }
      },
      fail: function (res) {
        wx.showModal({
          title: '获取订单信息失败',
          content: res.data.msg,
        })
      }
    })
  },

  onReady: function () {

  },


  onShow: function () {

  },

  onHide: function () {

  },

  onUnload: function () {

  },


  onPullDownRefresh: function () {
    let that = this
    wx.showToast({
      title: '正在刷新',
      duration: 1000,
      icon: "loading",
      success: function () {
        Object.assign(that.data.listViewModel, { pageindex: 1, list: [], ispost: false, loadall: false });
        that.GetExchangeActivityOrders()
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


  onReachBottom: function () {
    this.GetExchangeActivityOrders()
  },


  onShareAppMessage: function () {

  }
})