// pages/detail/detail.js
const util = require("../../utils/util.js");
const addr = require("../../utils/addr.js");
const http = require('../../utils/http.js');
const WxParse = require('../../utils/wxParse/wxParse.js');
const animation = require("../../utils/animation.js");
const page = require("../../utils/pageRequest.js");
const tools = require("../../utils/tools.js");
const app = getApp();
Page({
  /**
   * 页面的初始数据
   */
  data: {
    no: [{
      content: '暂无介绍'
    }],
    discountPrice: 0,

  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    let typeName = options.typeName || "";//类型判断
    let showprice = options.showprice || "";//是否显示价格
    let pid = options.id || _array[0];//详情Id
    if (typeName == 'true') {
      that.setData({ showBtn: true })
    } else {
      that.setData({ showBtn: false })
    }
    console.log("typeName=" + typeName)
    page.detailsRequest(pid, that)
    that.data.pid = pid
    that.data.typeName = typeName
    that.data.showprice = showprice
    util.setPageSkin(that);


  },







  preview: function (e) {
    let that = this
    let slider = e.currentTarget.dataset.slider
    let img = e.currentTarget.dataset.img
    let index = e.currentTarget.id
    if (slider) {
      util.preViewShow(slider[index], slider)
    }
    else {
      let urls = []
      urls.push(img)
      util.preViewShow(img, urls)
    }
  },

  /**
   * 跳转功能
   */
  // 跳转预约表单
  yuyueGoto: function () {
    tools.goNewPage('../subscribe/subscribe?pid=' + this.data.pid + '&name=' + this.data.msg.name + "&form=true")
  },


  /**
     * 用户点击右上角分享
     */
  onShareAppMessage: function () {
    let that = this
    let imgUrl = ""
    if (that.data.msg.slideimgs.length == 0) {
      imgUrl = that.data.msg.img
    } else {
      imgUrl = that.data.msg.slideimgs[0]
    }

    return {
      title: that.data.msg.name,
      path: 'pages/detail/detail?id=' + that.data.pid + "&typeName=" + that.data.typeName + "&showprice=" + that.data.showprice,
      imageUrl: imgUrl,
      success: function (res) {
        tools.showToast("转发成功")
      }
    }

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast("正在刷新")
    setTimeout(function () {
      tools.showToast("刷新成功")
      page.detailsRequest(that.data.pid, that)
    }, 1000)
    wx.stopPullDownRefresh()
  },










})