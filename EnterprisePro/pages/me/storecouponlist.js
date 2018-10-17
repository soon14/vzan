// pages/me/storecouponlist.js

const pages = require('../../utils/pageRequest.js');
const addr = require("../../utils/addr.js");
const http = require("../../utils/http.js");
const tools = require("../../utils/tools.js");
let app = getApp();

Page({

  /**
   * 页面的初始数据
   */
  data: {
    vmStorecoupon: {
      list: [],
      ispost: false,
      loadall: false,
      pageindex: 1,
      pagesize: 10,
      listname:"storecoupon",
    },
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    this.loadMore();
  },
  loadMore: function (callback) {
    var that = this;
    var d = that.data;
    var vm = that.data.vmStorecoupon;
    if (vm.ispost || vm.loadall)
      return;

    if (!vm.ispost) {
      this.setData({
        "vmStorecoupon.ispost": true,
      });
    }

    tools.GetStoreCouponList({
      appId: app.globalData.appid,
      goodstype: -1,
      userId: app.globalData.userInfo.UserId
    }).then(function (res) {
      console.log(res);
      if (res.isok) {
        if (res.postdata.length >= vm.pagesize) {
          vm.pageindex += 1;
        }
        else {
          vm.loadall = true;
        }
        vm.list = vm.list.concat(res.postdata);
        vm.ispost = false;
      }
      that.setData({
        vmStorecoupon: vm
      })

      if (callback) {
        callback();
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
    this.reLoad();
  },
  reLoad: function () {
    this.setData({
      vmStorecoupon: {
        list: [],
        ispost: false,
        loadall: false,
        pageindex: 1,
        pagesize: 10,
        listname: "storecoupon",
      },
    });
    this.loadMore(function () {
      setTimeout(function () {
        wx.stopPullDownRefresh();
      }, 500);
    });
  },
  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    var vm = this.data.vmStorecoupon;
    if (!vm.ispost && !vm.loadall){
      //this.loadMore();
    }
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  },
  getCoupon: function (e) {
    if (!app.globalData.userInfo) {
      wx.showModal({
        title: '提示',
        content: '请先登录！',
      })
      return;
    }
    console.log(e)
    var ds = e.currentTarget.dataset;
    var id = ds.id;
    var that = this;
    tools.GetCoupon({
      appId: app.globalData.appid,
      couponId: id,
      userId: app.globalData.userInfo.UserId
    })
      .then(function (res) {
        wx.showModal({
          title: '提示',
          content: res.msg,
        })
        console.log(res);
        if (res.isok) {
          //that.reLoad();
        }
      });
  }
})