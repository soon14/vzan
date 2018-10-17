const util = require("../../utils/util.js");
const addr = require("../../utils/addr.js");
const app = getApp();
const tools=require("../../utils/tools.js")
Page({

  data: {
    navTitle: "积分中心",
    integralRule: false,
    currentClassify: 0,
    listViewModel: {
      pageindex: 1,
      pagesize: 10,
      list: [],
      ispost: false,
      loadall: false,
    },
    integralRuleContent: [],
    userIntegral: "",
    integralOrder:0
  },

  onLoad: function (options) {
    let navTitle = this.data.navTitle;
    util.navBarTitle(navTitle)

    // this.GetUserIntegral();
  },
  toggleClassify: function (e) {
    this.setData({
      currentClassify: e.currentTarget.id
    });
    Object.assign(this.data.listViewModel, { pageindex: 1, list: [], ispost: false, loadall: false });
    this.GetExchangeActivityList();
  },
  openIntegralRule: function () {
    let integralRule = !this.data.integralRule
    this.GetStoreRules()
    this.setData({
      integralRule: integralRule
    })
  },
  openGoodsDetails: function (e) {
    let mark = e.currentTarget.dataset.mark
    let currentClassify = this.data.currentClassify
    wx.navigateTo({
      url: "../integralDetails/integralDetails?id=" + mark + "&currentClassify=" + currentClassify
    })
  },
  openIntegralRecord: function () {
    tools.goNewPage("../integralRecord/integralRecord")
  },
  openIntegralOrder: function () {
    tools.goNewPage("../integralOrder/integralOrder")
  },
  //获取用户积分及代发货数量
  getUserIntegral: function () {
    let that = this;
    wx.request({
      url: addr.Address.GetUserIntegral,
      data: {
        appId: app.globalData.appid,
        userId: app.globalData.userInfo.UserId
      },
      method: "POST",
      header: {
        'content-type': 'application/x-www-form-urlencoded'
      },
      success: function (res) {
        if (res.data.isok) {
          let userIntegral = res.data
          that.setData({
            userIntegral: userIntegral.obj.integral,
            integralOrder: userIntegral.sendCount
          })
        }
      },
      fail: function (res) {
        wx.showModal({
          title: '获取用户信息失败',
          content: res.data.msg,
        })
      }
    })
  },
  // 请求商品数据
  GetExchangeActivityList: function () {
    let that = this;
    let vm = that.data.listViewModel
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    wx.request({
      url: addr.Address.GetExchangeActivityList,
      data: {
        appId: app.globalData.appid,
        type: that.data.currentClassify,
        pageindex: vm.pageindex,
        pagesize: vm.pagesize
      },
      method: "POST",
      header: {
        'content-type': 'application/x-www-form-urlencoded'
      },
      success: function (res) {
        if (res.data.isok) {
          if (res.data.model.ExchangeActivityList.length >= vm.pagesize) {
            vm.pageindex += 1;
          }
          else {
            vm.loadall = true;
          }
          if (res.data.model.ExchangeActivityList.length > 0) {
            vm.list = vm.list.concat(res.data.model.ExchangeActivityList);
          }
          let mallList = vm.list
          that.setData({
            mallList: mallList
          })
        }
      },
      fail: function (res) {
        wx.showModal({
          title: '获取商品失败',
          content: res.data.msg,
        })
      }
    })
  },
  //获取积分规则
  GetStoreRules: function () {
    let that = this;
    wx.request({
      url: addr.Address.GetStoreRules,
      data: {
        appId: app.globalData.appid,
      },
      method: "POST",
      header: {
        'content-type': 'application/x-www-form-urlencoded'
      },
      success: function (res) {
        if (res.data.isok) {
          let ruleContent = res.data.obj
          that.setData({
            integralRuleContent: ruleContent
          })
        }
      },
      fail: function (res) {
        wx.showModal({
          title: '获取用户信息失败',
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
    this.GetExchangeActivityList();
    this.getUserIntegral();
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
        that.GetExchangeActivityList()
        that.getUserIntegral()
      }
    })
    setTimeout(function () {
      wx.showToast({
        title: '刷新成功',
        duration: 1000,
        icon: "success"
      })
    wx.stopPullDownRefresh()
    }, 1000)
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    this.GetExchangeActivityList();
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  }
})