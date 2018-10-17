var util = require("../../utils/util.js");

var addr = require("../../utils/addr.js");
const http = require('../../utils/http.js');
var tools = require("../../utils/tools.js");
var app = getApp();


var timer_countdown = null;

Page({

  /**
   * 页面的初始数据
   */
  data: {
    groupState: [
      { name: "进行中", state: 1, sel: true },
      { name: "已结束", state: 2, sel: false },
      { name: "全部", state: 0, sel: false }
    ],
    groupStateName: {
      "1": "进行中",
      "2": "已结束",
      "0": "全部"
    },
    vm: {
      ispost: false,
      loadall: false,
      list: [],
      state: 1,
      pagesize: 10,
      pageindex: 1,
    },
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this;
    app.getUserInfo(function (e) {
      that.loadMore();
    })
  },
  loadMore: function () {
    var that = this, vm = this.data.vm, user = vm.uinfo;
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost) {
      that.setData({
        "vm.ispost": true
      })
    }
    wx.setNavigationBarTitle({
      title: that.data.groupStateName[vm.state.toString()] + '拼团',
    })
    http
      .postAsync(addr.Address.GetGroupList,
      {
        appId: app.globalData.appid,
        state: vm.state,
        pageIndex: vm.pageindex
      })
      .then(function (data) {
        console.log(data);

        if (data.isok) {
          if (data.postdata.length < vm.pagesize) {
            vm.loadall = true;
          }

          if (data.postdata.length > 0) {
            if (vm.pageindex <= 1) {
              vm.list = data.postdata
            }
            else {
              vm.list = vm.list.concat(data.postdata);
            }

            that.initCountDown();

          }
          vm.ispost = false;
          that.setData({
            "vm": vm
          });

        }
        else {
          tools.alert("提示", data.msg);
        }


      });

  },
  //初始化拼团列表
  initGroupList: function () {
    var that = this;
    http
      .postAsync(addr.Address.GetGroupListPage, { appId: app.globalData.appid })
      .then(function (data) {
        that.data.vm.list = [];
        that.setData({ "vm.list": data.postdata });
      });
  },
  clickGroupItem: function (e) {
    var _groupid = e.currentTarget.dataset.groupid;
    var _g = e.currentTarget.dataset.group;
    if (_g.State == 1) {
      wx.navigateTo({
        url: '/pages/groupDetail/groupDetail?id=' + _groupid,
      })
    }

  },
  changeGroupListState: function (e) {
    var that = this, ds = e.currentTarget.dataset, vm = this.data.vm;
    that.resetVM({ state: ds.state });
    that.loadMore();

  },
  resetVM: function (source) {
    var vm = this.data.vm;
    vm.ispost = false;
    vm.loadall = false;
    vm.pageindex = 1;
    vm.list = [];
    if (source) {
      Object.assign(vm, source);
    }
    this.setData({
      "vm": vm
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
    var that = this;
    var vm = this.data.vm;
    this.resetVM({ state: vm.state });
    that.loadMore();
    setTimeout(function () {
      wx.stopPullDownRefresh();
    }, 1500);
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

  },
  //初始化倒计时
  initCountDown: function () {
    var vm = this.data.vm, that = this;
    if (vm.list.length > 0) {
      for (var i = vm.list.length - 1; i >= 0; i--) {
        if (vm.list[i].State == -1) {
          var timespan = tools.getTimeSpan(vm.list[i].ValidDateStart);
          if (timespan <= 0) {
            vm.list.splice(i, 1)
          }
          else {
            var timeFormatArray = tools.formatMillisecond(timespan);
            var timeFormat = "";
            if (timeFormatArray[0] > 0) {
              timeFormat += timeFormatArray[0] + '天';
            }
            timeFormat += timeFormatArray[1] + '时' + timeFormatArray[2] + '分' + timeFormatArray[3] + '秒';
            vm.list[i].countdown = timeFormat;
          }

        }
      }
      that.setData({
        "vm": vm
      });

      timer_countdown = setTimeout(function () {
        that.initCountDown();
      }, 1000);
    }

  },
  goto: function (e) {
    var url = e.currentTarget.dataset.url;

  }
})