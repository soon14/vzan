// pages/im/im.js
var template = require("../../template/template.js");
import { core } from '../../utils/core';
let _get = require('../../utils/lodash.get');
//import Promise from '../../utils/es6-promise.min.js';

var vmDefault = {
  list: [],
  ispost: false,
  loadall: false,
  pageindex: 1,
  pagesize: 20,
};
Page({

  /**
   * 页面的初始数据
   */
  data: {
    vm: JSON.parse(JSON.stringify(vmDefault))
  },
  enterchatroom: function () {
    wx.navigateTo({
      url: '../chatroom/chatroom',
    })
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {

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
    var that = this,
      app = getApp();


    app.getUserInfo(function (e) {
      console.log(e);
      core.GetTechInfo({ telePhoneNumber: e.TelePhone }).then(function (res) {
        if (res) {
          that.setData({
            techid: res.userid
          });
          that.loadMore();
        }
      });

    })
  },
  loadMore: function () {
    var that = this,
      d = this.data,
      vm = d.vm,
      app = getApp();
    if (vm.ispost || vm.loadall)
      return;
    vm.ispost = true;

    core.GetContactList({
      appId: app.globalData.appid,
      fuserId: that.data.techid,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
      fuserType: 1,
    }).then(function (res) {
      console.log(res);
      if (res && res.data) {
        vm.list = res.data;
      }
      vm.ispost = false;
      vm.loadall = true;


      //显示未读消息数
      if (vm.list.length > 0) {
        var unreadmsgArray = app.globalData.unreadmsg;
        if (unreadmsgArray) {
          for (var i = 0, ii = vm.list.length; i < ii; i++) {
            var key = vm.list[i].tuserId + "_" + vm.list[i].fuserId + "_" + vm.list[i].fuserType;
            var unreadmsgItem = _get(unreadmsgArray, key, 0);
            vm.list[i].unreadnum = unreadmsgItem;
            if (unreadmsgItem > 99) {
              vm.list[i].unreadnum_fmt = "99+";
            }
            else {
              vm.list[i].unreadnum_fmt = unreadmsgItem;
            }

          }
        }
      }
      that.setData({
        vm
      });

    });
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
    //wx.stopPullDownRefresh();

    this.setData({
      vm: JSON.parse(JSON.stringify(vmDefault))
    });
    this.loadMore();
    template.stopPullDown();
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  // onShareAppMessage: function () {

  // }
  clickToCaht: function (e) {
    var ds = e.currentTarget.dataset;
    var nickname = (ds.nickname || "").replace(/\s/gi, "");
    var headimg = ds.headimg;
    var index = ds.index;
    var that = this;
    var app=getApp();
    var fuserid = app.globalData.fuserInfo.userid;//技师ID


    var key = "vm.list[" + index + "].unreadnum";
    that.setData({
      [key]: 0,
    });
    var unreadmsg = app.globalData.unreadmsg;
    var unreadmsgcount = app.globalData.unreadmsgcount;
    if (unreadmsg) {
      var key = ds.userid + "_" + fuserid + "_1";
      var preCount = unreadmsg[key];//原某一个会话未读数
      unreadmsg[key] = 0;

      unreadmsgcount = unreadmsgcount - preCount;//总未读数-当前会话未读数
      if (unreadmsgcount <= 0) {
        unreadmsgcount = 0;
        wx.hideTabBarRedDot({
          index: 3,
        });
      }

      core.changeunreadmsg(unreadmsg, unreadmsgcount);
    }

    wx.navigateTo({
      url: 'chat?userid=' + ds.userid + "&nickname=" + nickname + "&headimg=" + headimg,
    })
  },

})