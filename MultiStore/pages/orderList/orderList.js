// pages/orderList/orderList.js
const app = getApp();
const pageRequest = require("../../utils/public-request.js");
const tools = require('../../utils/tools.js')
Page({

  /**
   * 页面的初始数据
   */
  data: {
    goodListViewModal: {
      pageindex: 1,
      pagesize: 5,
      list: [],
      ispost: false,
      loadall: false,
    },
    status: [
      { context: "全部", id: -999 },
      { context: "待自取", id: 8 },
      { context: "待发货", id: 1 },
      { context: "待收货", id: 2 },
      { context: "已完成/退款", id: 0 },
    ],
    img: ["http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg", "/image/1.png", "/image/2.png", "/image/1.png",],
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let condition = 0
    if (Object.keys(options).length == 0) {
      condition = -999
      pageRequest.orderRequest(condition, this)
    } else {
      condition = options.condition
      pageRequest.orderRequest(condition, this)
    }
    this.setData({ condition: condition })
  },
  // choose
  changFunc: function (e) {
    let that = this
    tools.resetFunc(that)
    that.setData({ condition: e.currentTarget.dataset.id })
    pageRequest.orderRequest(e.currentTarget.dataset.id, that)
    wx.pageScrollTo({ scrollTop: 0 })
  },
  // pay function
  gotopay: function (e) {
    let newparam = {
      openId: app.globalData.userInfo.openId,
      orderid: e.currentTarget.dataset.orderid,
      'type': 1,
      dbOrderId: e.currentTarget.dataset.dborder
    }
    pageRequest.paySec(newparam)
    pageRequest.commitFormId(e.detail.formId)
  },
  //拨打电话
  phoneFunc: function () { tools.phone(app.globalData.storeData.TelePhone) },
  // 申请退款
  orderFunc: function (e) {
    let that = this
    let para = {
      storeId: e.currentTarget.dataset.storeid,
      orderId: e.currentTarget.dataset.dborder
    }
    pageRequest.commitFormId(e.detail.formId)
    tools._clickMoadl('确认申请退款吗?').then(res => {
      if (res.confirm) {
        pageRequest.outOrder(para).then(data => {
          if (data.isok) {
            tools.resetFunc(that)
            pageRequest.orderRequest(that.data.condition, that)
          }
        })
      }
    })
  },

  // 取消订单&&确认收货
  canCleFunc: function (e) {
    let that = this
    let [state, content, id] = [0, "", e.currentTarget.dataset.id]
    if (id == 1) { state = -1; content = "确认取消该订单吗?" }
    else if (id == 4) { state = 3; content = "确认送达吗?" }
    else { state = 3; content = "确认收货吗？" }
    let para = {
      storeId: e.currentTarget.dataset.storeid,
      orderId: e.currentTarget.dataset.dborder,
      state: state
    }
    wx.showModal({
      title: '提示',
      content: content,
      success: res => {
        if (res.confirm) {
          pageRequest.changeOrder(para).then(function () {
            tools.resetFunc(that)
            pageRequest.orderRequest(that.data.condition, that)
            pageRequest.commitFormId(e.detail.formId)
          })
        }
        else {
          return;
        }
      }
    })

  },
  // 订单详情
  orderGoto: function (e) {
    let dborder = e.currentTarget.dataset.dborder
    tools.goNewPage("../orderStatus/orderStatus?storeId=" + app.globalData.currentPage.Id + "&dbOrderId=" + dborder)
  },
  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
  },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast('正在刷新')
    setTimeout(res => { tools.showToast("刷新成功"); pageRequest.orderRequest(that.data.condition, that) }, 1000)
    wx.stopPullDownRefresh()
  },
  // 返回顶部
  backFunc: function () {
    wx.pageScrollTo({
      scrollTop: 0
    })
  },
  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    pageRequest.orderRequest(this.data.condition, this)
  },


})