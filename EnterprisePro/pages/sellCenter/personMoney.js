// pages/sellCenter/personMoney.js
const app = getApp();
const util = require("../../utils/util.js");
const tools = require("../../utils/tools.js");
const page = require("../../utils/pageRequest.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    showMoney: false
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    app.getUserInfo(res => {
      page.getSaleMan(res.UserId).then(data => {
        if (data.isok) {
          this.setData({ vm: data.obj })
        } else {
          tools.ShowMsg(data.msg)
        }
      })
    })
    util.setPageSkin(this);
  },
  goMoney: function () {
    this.setData({ showMoney: true })
  },
  setInput: function (e) {
    console.log(e)
    let price = Number(e.detail.value)
    let nowPrice = parseFloat(this.data.vm.useCashStr)
    if (price > nowPrice) {
      this.setData({ price: "", text: "请输入可提现金额范围" })
      return
    } else {
      this.data.price = price
      this.setData({ text: "" })
    }
  },
  postCash: function () {
    let that = this
    if (that.data.price == undefined) {
      tools.ShowMsg("请输入可提现金额")
      return;
    }
    wx.showLoading({
      title: '加载中..',
      mask: true,
      success: function () {

        page.postCash(that.data.price).then(data => {
          if (data.isok) {
            tools.goNewPage("../sellCenter/personSuccess?price=" + that.data.price)
          } else {
            tools.ShowMsg(data.msg)
          }
          wx.hideLoading();
        })

      }
    })
  },
  goRecord: function () {
    tools.goNewPage("../sellCenter/record")
  },

})