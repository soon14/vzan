var http = require("../../utils/http.js");
var addr = require("../../utils/addr.js");
var tools = require("../../utils/tools.js");
var util = require("../../utils/util.js");
var app = getApp();

Page({

  /**
   * 页面的初始数据
   */
  data: {

  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var _gsid = options.gsid || 0;
    var _orderid = options.orderid || 0;
    var _paytype = options.paytype || -1;
    if ( _orderid == 0 || _paytype == -1) {
      wx.navigateBack({
        delta: 1,
      })
    }

    this.setData({
      gsid: _gsid,
      orderid: _orderid,
      paytype: _paytype,
    });
    this.init();

  },
  init: function () {
    var that = this;
    var _d = that.data;
    http.postAsync(addr.Address.GetPaySuccessGroupDetail, {
      appId: app.globalData.appid,
      gsid: _d.gsid,
      orderid: _d.orderid,
      paytype: _d.paytype,
    }).then(function (data) {
      console.log(data);
      if (data.isok) {
        var _g = data.postdata;

        if (_d.gsid>0){
          if (_g.GroupUserList.length >= 4) {
            _g.GroupUserList = _g.GroupUserList.slice(0, 4);
            _g.NeedNum_fmt = 0;
          }
          else {
            if (_g.NeedNum + _g.GroupUserList.length <= 4) {
              _g.NeedNum_fmt = _g.NeedNum;
            }
            else{
              _g.NeedNum_fmt = 4 - _g.GroupUserList.length;
            }
            
          }

        }
        

        that.setData({
          "vm": data.postdata,
        });

        if (that.data.gsid > 0) {
          tools.initEndClock(_g.ValidDateStart, _g.ValidDateEnd, that);
        }
      }
    });
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
    wx.showLoading({
      title: '加载中',
    })
    this.init();
    setTimeout(function () {
      wx.hideLoading()
      wx.stopPullDownRefresh();
    }, 1000);
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function (res) {
    var group = res.target.dataset.group;

    return tools.share(group);
  }
})