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
    orderStateName: {
      "-4": "已过期",
      "-3": "退款中",
      "-2": "已退款",
      "-1": "已收货",
      "0": "待发货",
      "1": "已发货",
    }
  },
	// 拨打电话
	makephonecall: function () {
		wx.makePhoneCall({
			phoneNumber: app.globalData.customerphone,
		})
	},
	// 返回主页
	siwchtoIndex: function () {
		wx.switchTab({
			url: '../index/index',
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var guid = options.guid || 0;
    if (guid == 0) {
      wx.navigateBack({
        delta: 1
      })
    }
    this.data.guid = guid;
    this.setData({
      "guid": guid
    });
    this.init();
  },
  init: function () {
    var _d = this.data;
    var that = this;

    http
      .postAsync(addr.Address.GetGroupOrderDetail,
      {
        appId: app.globalData.appid,
        guid: _d.guid,
      })
      .then(function (data) {
        console.log(data);
        if (data.isok) {
          var _result = data.postdata;
          var _goodprice=0;
          if (_result.isGroup){
            _goodprice = _result.DiscountPrice * _result.num/100;
          }
          else{
            _goodprice = _result.OriginalPrice * _result.num/100;
          }
          _goodprice = parseFloat(_goodprice).toFixed(2);
          that.setData({
            "vm": data.postdata,
            "vm.goodprice": _goodprice
          });
        }

      });
  },
  copyData: function (e) {
    var _d = e.currentTarget.dataset.ordernum;
    if (_d) {
      tools.copyData(_d);
    }
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
    this.init();
    wx.showLoading({
      title: '加载中',
    })
    setTimeout(function () {
      wx.hideLoading();
      wx.stopPullDownRefresh()
    }, 500);
  },
  // 返回主页
  siwchtoIndex: function () {
    wx.switchTab({
      url: '../index/index',
    })
  },
  // 拨打电话
  makephonecall: function () {
    wx.makePhoneCall({
      phoneNumber: app.globalData.customerphone,
    })
  },
  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },
  changeOrderStatus: function (e) {
    var that = this;
    var _guid = that.data.guid;
    wx.showModal({
      title: '提示',
      content: '确认收货吗？',
      success: function (res) {
        if (res.confirm) {
          http.postAsync(addr.Address.RecieveGoods, { guid: _guid })
            .then(function (data) {
              tools.alert("提示", data.msg, function () {
                that.init();
              });
            });
        } else if (res.cancel) {

        }
      }
    })

  }
})