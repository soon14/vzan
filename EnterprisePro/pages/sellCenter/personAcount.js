

// pages/sellCenter/personAcount.js
const app = getApp();
const util = require("../../utils/util.js");
const tools = require("../../utils/tools.js");
const page = require("../../utils/pageRequest.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    state: [
      { txt: "全部", id: "0" },
      { txt: "未失效", id: "1" },
      { txt: "已失效", id: "2" }
    ],
    priceState: [
      { txt: "佣金", id: "0" },
      { txt: "最新", id: "1" },
      { txt: "价格", id: "2" }
    ],
    vmCount: {
      list: [],
      ispost: false,
      loadall: false,
      pageIndex: 1,
      pageSize: 10,
      state: 0,
      goodsName: "",
      sortType: 0,
      showType: 0,
    },
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let showId = Number(options.id)
    if (options) {
      this.getAcount(showId, this.data.vmCount)
    }
    this.setData({ showId, condition: 0 })
    util.setPageSkin(this);
  },
  select: function (e) {
    console.log(e)
    let vmCount = this.data.vmCount
    Object.assign(vmCount, { pageIndex: 1, list: [], ispost: false, loadall: false })
    vmCount.state = e.currentTarget.dataset.id
    vmCount.sortType = e.currentTarget.dataset.id
    this.getAcount(this.data.showId, vmCount)
    this.setData({ condition: e.currentTarget.dataset.id })
  },
  getAcount: function (showId, vmCount) {
    let that = this
    app.getUserInfo(res => {
      // 获取累计客户
      if (showId == 0) {
        wx.showLoading({
          title: '加载中',
          mask: true,
          success: function () {
            if (vmCount.ispost || vmCount.loadall)
              return;
            if (!vmCount.ispost)
              that.setData({ "vmCount.ispost": true });
            page.getRecordUser(vmCount).then(data => {
              vmCount.list = []
              vmCount.ispost = false
              if (data.isok) {
                data.obj.RecordCount >= vmCount.pageIndex ? vmCount.pageIndex += 1 : vmCount.loadall = true;
                data.obj.RecordCount > 0 ? vmCount.list = vmCount.list.concat(data.obj.SalesManRecordUserList) : "";
                that.setData({ vmCount, })
              } else {
                tools.ShowMsg(data.msg)
              }
              wx.hideLoading()
            })
          }
        })
        util.navBarTitle("累计客户")
      }
      //推广产品
      else if (showId == 1) {
        if (vmCount.ispost || vmCount.loadall)
          return;
        if (!vmCount.ispost)
          that.setData({ "vmCount.ispost": true });
        page.getSaleGoodsList(vmCount).then(data => {
          vmCount.ispost = false
          if (data.isok) {
            data.obj.SalesmanGoodsList.length >= vmCount.pageIndex ? vmCount.pageIndex += 1 : vmCount.loadall = true;
            data.obj.SalesmanGoodsList.length > 0 ? vmCount.list = vmCount.list.concat(data.obj.SalesmanGoodsList) : "";
            vmCount.showType = data.obj.showType
            that.setData({ vmCount, })
          } else {
            tools.ShowMsg(data.msg)
          }
        })
        util.navBarTitle("推广商品")
      }
      else {
        if (vmCount.ispost || vmCount.loadall)
          return;
        if (!vmCount.ispost)
          that.setData({ "vmCount.ispost": true });
        page.getRecoderOrder(res.UserId, vmCount).then(data => {
          vmCount.ispost = false
          if (data.isok) {
            if (data.obj.List != null) {
              data.obj.List.length >= vmCount.pageIndex ? vmCount.pageIndex += 1 : vmCount.loadall = true;
              data.obj.List.length > 0 ? vmCount.list = vmCount.list.concat(data.obj.List) : "";
            }
            that.setData({ vmCount })
          } else {
            tools.ShowMsg(data.msg)
          }
        })
        util.navBarTitle("推广订单")
      }
    })
  },
  search: function (e) {
    console.log(e)
    let vmCount = this.data.vmCount
    Object.assign(vmCount, { pageIndex: 1, list: [], ispost: false, loadall: false })
    vmCount.goodsName = e.detail.value
    this.getAcount(this.data.showId, this.data.vmCount)
  },
  goPro: function (e) {
    let id = e.currentTarget.dataset.id
 
    let url =
      wx.redirectTo({
        url: "../detail/detail?id=" + id + "&typeName=buy" + "&showprice=true" + "&sellShow=true" + "&showQrcode=1",
      })
  },
  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    this.getAcount(this.data.showId, this.data.vmCount)
  },


})