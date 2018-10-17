// pages/myOrder/myOrder.js
var util = require("../../utils/util.js");
const tools = require("../../utils/tools.js")
var addr = require("../../utils/addr.js");
const http = require('../../utils/http.js');
var pages = require("../../utils/pageRequest.js")
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    condition: 10, // 标题颜色，默认为10
    choose: [ //标题
      { title: '全部订单', state: 10 },
      { title: '待付款', state: 0 },
      { title: '待发货', state: 1 },
      { title: '待收货', state: 2 },
      { title: '已完成/退款', state: 3 }
    ],
    array: ['微信支付', '储值支付'],//选择付款模式
    index1: 2,//选择付款模式下标
    orderId: 0,
    state: 10,
    listViewModel: {
      pageindex: 1,
      pagesize: 10,
      list: [],
      ispost: false,
      loadall: false,
    },
    allList: [],
  },



  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    if (options.state != undefined) {
      that.orderRequest(options.state)
    } else {
      that.orderRequest(that.data.state)
    }
    util.setPageSkin(that);
  },
  // 获取订单页面请求
  orderRequest: function (state) {
    var that = this
    var vm = that.data.listViewModel
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    http
      .getAsync(
      addr.Address.getMiniappGoodsOrder,
      {
        appId: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        State: state,
        pageindex: vm.pageindex,
        pagesize: vm.pagesize
      })
      .then(function (data) {
        vm.ispost = false;//请求完毕，关闭请求开关
        if (data.isok == true) {
          //更改状态数据
          if (data.postdata.length >= vm.pagesize) {
            vm.pageindex += 1;
          }
          else {
            vm.loadall = true;
          }
          if (data.postdata.length > 0) {
            vm.list = vm.list.concat(data.postdata);
          }
          that.setData({
            "listViewModel": vm,
            condition: state
          })
        }
      })
  },
  // 更改订单状态
  changeStatusRequest: function (goodsorderid, state) {
    var that = this
    http.postAsync(
      addr.Address.updateMiniappGoodsOrderState,
      {
        appid: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        orderId: goodsorderid,
        State: state
      }).then(function (data) {
        if (data.isok == 1) {
          wx.showToast({
            title: '收货成功',
            icon: 'success',
            duration: 1500
          })
          Object.assign(that.data.listViewModel, { pageindex: 1, list: [], ispost: false, loadall: false })
          that.orderRequest(that.data.condition)
        }
      })
  },
  // 查看订单详情
  orderGoto: function (e) {
    var index = e.currentTarget.id
    wx.navigateTo({
      url: '../orderDetail/orderDetail?orderId=' + index
    })
  },
  // 跳转到商品详情页
  detailGoto: function (e) {
    var index = e.currentTarget.id
    wx.navigateTo({
      url: '../detail/detail?id=' + index + '&typeName=buy'
    })
  },
  // 查看不同状态
  statusFunc: function (e) {
    var that = this
    var index = e.currentTarget.id
    Object.assign(that.data.listViewModel, { pageindex: 1, list: [], ispost: false, loadall: false })
    that.orderRequest(index)
    wx.pageScrollTo({
      scrollTop: 0
    })
  },
  //确认收货
  goodsFunc: function (e) {
    var that = this
    var index = e.currentTarget.dataset.pid
    wx.showModal({
      title: '提示',
      content: '是否确认收货？',
      success: function (res) {
        if (res.confirm) {
          that.changeStatusRequest(index, 3) //待收货-->已完成
          pages.updateCardRequest()
        } else if (res.cancel) {
          return
        }
      }
    })

  },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    var that = this
    tools.showLoadToast("正在刷新")
    setTimeout(function () {
      tools.showToast("刷新成功")
      Object.assign(that.data.listViewModel, { pageindex: 1, list: [], ispost: false, loadall: false })
      that.orderRequest(that.data.condition)
    }, 1000)
    wx.stopPullDownRefresh()
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    var that = this
    that.orderRequest(that.data.condition)
  },


  // 微信二次付款
  gotopay: function (e) {
    let oradid = e.currentTarget.id
    let entgoodstorderid = e.currentTarget.dataset.pid
    pages.goPayFuc(oradid, entgoodstorderid)
  },




})