// pages/orderStatus/orderStatus.js
const app = getApp();
const pageRequest = require("../../utils/public-request.js");
const tools = require('../../utils/tools.js')
const util = require("../../utils/util.js")

Page({
  /**
   * 页面的初始数据
   */
  data: {
    _seClose: false
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let _couShow = false
    let _sRlist = ""
    if (options && options.reduction) {
      _sRlist = JSON.parse(options.reduction)
    }
    let para = {
      storeId: options.storeId,
      orderId: options.dbOrderId,
      fpage: this
    }
    pageRequest.getOrderDetial(para).then(data => {
      if (data.isok) {
        if (_sRlist && _sRlist.RemNum > 0 && (data.orderInfo.State == 8 || data.orderInfo.State == 1 || data.orderInfo.State == 11)) {
          _couShow = true
        } else {
          _couShow = false
        }
        this.setData({ _couShow: _couShow })
        this.data.storeId = options.storeId
        this.data.dbOrderId = options.dbOrderId
        this.data._seClose = true
      }
    })
  },
  onShow: function () {
    if (this.data._seClose) {
      this.setData({ "_couShow": false })
    }
  },
  //立减金
  _coupFunc: function (e) {
    let id = Number(e.target.id)
    switch (id) {
      case 0:
        this.setData({ _couShow: false })
        break;
      case 1:
        tools.goNewPage('../addCoupon/getsmoney')
        break;
    }
  },
  // 复制订单号
  _copyFunc: function (e) {
    let num = e.currentTarget.dataset.id
    tools.copy(num)
  },
  // 申请退款
  _refuFunc: function () {
    let that = this
    let para = {
      storeId: that.data.storeId,
      orderId: that.data.dbOrderId,
      fpage: that
    }
    tools._clickMoadl('确认申请退款吗?').then(res => {
      if (res.confirm) {
        pageRequest.outOrder(para).then(data => {
          if (data.isok) {
            setTimeout(function () { pageRequest.getOrderDetial(para) }, 500)
          }
        })
      }
    })
  },
  // 取消订单&&确认收货
  _sPageFuc: function (e) {
    let that = this
    let [state, content, id] = [0, "", Number(e.currentTarget.dataset.id)]
    switch (id) {
      case 1:
        state = -1;
        content = "确认取消该订单吗?";
        break;
      case 4:
        state = 3;
        content = "确认送达吗?"
        break;
      default:
        state = 3;
        content = "确认收货吗？"
        break;
    }
    let para = {
      storeId: that.data.storeId,
      orderId: that.data.dbOrderId,
      state: state,
      fpage: that
    }
    tools._clickMoadl(content).then(res => {
      if (res.confirm) {
        pageRequest.changeOrder(para).then(data => {
          pageRequest.getOrderDetial(para)
        })
      }
    })
  },
  //拨打电话&&返回首页&&二次支付
  _sUseFunc: function (e) {
    let id = Number(e.currentTarget.id)
    let newparam = {
      openId: app.globalData.userInfo.openId,
      orderid: e.currentTarget.dataset.orderid,
      'type': 1,
      dbOrderId: e.currentTarget.dataset.dborder
    }
    switch (id) {
      case 0:
        tools.phone(app.globalData.currentPage.TelePhone);
        break;
      case 1:
        tools.goLaunch("../index/index");
        break;
      case 2:
        pageRequest.paySec(newparam)
        break;
    }
  },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    let para = {
      storeId: that.data.storeId,
      orderId: that.data.dbOrderId,
      fpage: that
    }
    tools.showLoadToast("正在刷新")
    setTimeout(res => { tools.showToast("刷新成功"); pageRequest.getOrderDetial(para); wx.stopPullDownRefresh() }, 1000)
  },

})