// pages/me/mycoupon.js
const page = require('../../utils/pageRequest.js');
const addr = require("../../utils/addr.js");
const http = require("../../utils/http.js");
const tools = require("../../utils/tools.js");
let app = getApp();


Page({

  /**
   * 页面的初始数据
   */
  data: {
    couponState: [
      { name: "未使用", value: 0, sel: true },
      { name: "已使用", value: 1, sel: false },
      { name: "已过期", value: 2, sel: false }
    ],
    vmStorecoupon: {
      list: [],
      ispost: false,
      loadall: false,
      pageindex: 1,
      pagesize: 10,
      state: 0,
      listname: "mycoupon",
    },
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    
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

    tools.GetMyCouponList({
      appId: app.globalData.appid,
      userId: app.globalData.userInfo.UserId,
      pageIndex: vm.pageindex,
      state: vm.state,
      goodsId:"",
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
  changeState: function (e) {
    var ds = e.currentTarget.dataset;
    var val = ds.value;
    var index = ds.index;
    var key = "couponState[" + index + "].sel";
    this.data.couponState.forEach(function (o) {
      o.sel = false;
    });
    this.data.couponState[index].sel=true;

    this.setData({
      "vmStorecoupon": {
        list: [],
        ispost: false,
        loadall: false,
        pageindex: 1,
        pagesize: 10,
        state: val,
        listname: "mycoupon",
      },
      "couponState": this.data.couponState,
    });
    console.log(val);
    this.loadMore();
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

    this.reLoad();
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
    var that=this;
    var lastState = that.data.vmStorecoupon.state;
    this.setData({
      vmStorecoupon: {
        list: [],
        ispost: false,
        loadall: false,
        pageindex: 1,
        pagesize: 10,
        state: lastState,
        listname: "mycoupon",
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
    if (!vm.ispost && !vm.loadall) {
      this.loadMore();
    }
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  },
  useCoupon:function(){
    wx.redirectTo({
      url: '/pages/index/index',
    })
  }
})