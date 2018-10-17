// pages/index/txtComEdit.js
var WxParse = require("../../modules/wxParse/wxParse.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    comIndex: -1,
    itemIndex: -1,
    backUrl: "",
    content: "",
    comType: "txt",
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var d = Object.assign(this.data, options);
    this.setData({ ...d });
  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
    this.initCom();
  },
  //取值
  initCom: function () {
    var d = this.data;
    d.comIndex = Number(d.comIndex);
    d.itemIndex = Number(d.itemIndex);

    var pages = getCurrentPages();
    var fromPage = pages.find(p => { return p.route === d.backUrl });
    if (!fromPage)
      return;
    var comKey = "";
    if (d.itemIndex != -1)
      d.conetnt = fromPage.data.vm.content.coms[d.comIndex].items[d.itemIndex].content
    else {
      d.content = fromPage.data.vm.content.coms[d.comIndex].content;
      d.comType = fromPage.data.vm.content.coms[d.comIndex].type;
    }

    this.setData({
      content: d.content,
      comType: d.comType
    });
    setTimeout(function () {
      fromPage.data.isnav = false;
    }, 1000);
  },
  //赋值
  save: function (e) {
    var d = this.data;
    var pages = getCurrentPages();
    var fromPage = pages.find(p => { return p.route === d.backUrl });

    var comKey = "", comKey_fmt = "";
    if (d.itemIndex != -1) {
      comKey = "vm.content.coms[" + d.comIndex + "].items[" + d.itemIndex + "].content";
      comKey_fmt = "vm.content.coms[" + d.comIndex + "].items[" + d.itemIndex + "].content_fmt";
    }
    else {
      comKey = "vm.content.coms[" + d.comIndex + "].content";
      comKey_fmt = "vm.content.coms[" + d.comIndex + "].content_fmt";
    }

    var value = e.detail.value.content;
    var value_fmt = WxParse.wxParse('content', 'html', value)

    wx.navigateBack({
      delta: 1,
      success: function () {
        fromPage.setData({
          [comKey]: value,
          [comKey_fmt]: value_fmt
        });
      }
    })
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

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  }
})