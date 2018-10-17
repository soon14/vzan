// pages/index/formDetail.js
import { http, addr, tools, listvm, shareModel } from "../../modules/core.js";
var app = getApp();
var today_begin = new Date().Format("yyyy-MM-dd 00:00:00");
var today_end = new Date().Format("yyyy-MM-dd 23:59:59");
var yesterday = new Date();
yesterday.setDate(yesterday.getDate() - 1);
var yesterday_begin = yesterday.Format("yyyy-MM-dd 00:00:00");
var yesterday_end = yesterday.Format("yyyy-MM-dd 23:59:59");

var recent7day = new Date();
recent7day.setDate(recent7day.getDate() - 7);
var recent7day_begin = recent7day.Format("yyyy-MM-dd 00:00:00");

var recent30day = new Date();
recent30day.setMonth(recent30day.getMonth() - 1);
var recent30day_begin = recent30day.Format("yyyy-MM-dd 00:00:00");

Page({

  /**
   * 页面的初始数据
   */
  data: {
    filterList: [
      { name: "今天", begin: today_begin, end: today_end, sel: true },
      { name: "昨天", begin: yesterday_begin, end: yesterday_end, sel: false },
      { name: "最近七天", begin: recent7day_begin, end: today_end, sel: false },
      { name: "最近一月", begin: recent30day_begin, end: today_end, sel: false },
    ],
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var id = options.id || 0;
    if (id <= 0) {
      tools.alert("页面不存在");
      return;
    }
    this.setData({ id });
    wx.hideShareMenu({})
    this.init();
    this.getPageInfo();
  },
  init: function () {
    var d = this.data;
    var vm = Object.assign({}, listvm);
    var filterItem = d.filterList.find(p => p.sel);
    var today_begin = filterItem.begin;
    var today_end = filterItem.end;
    vm.beginTime = today_begin;
    vm.endTime = today_end;

    this.setData({ vm, filterList: d.filterList });
    this.loadMore();
  },
  loadMore: function () {
    var that = this;
    var vm = this.data.vm;
    if (vm.isPost || vm.loadAll)
      return;
    app.getUserInfo().then(function (user) {
      that.setData({ user });
      if (user.phone == "") {
        that.setData({ phoneAuth: false });
        return;
      }
      that.setData({
        "vm.isPost": true
      });

      http.getAsync(addr.GetUserForm, {
        uid: user.uid,
        pageid: that.data.id,
        pageSize: vm.pageSize,
        pageIndex: vm.pageIndex,
        beginTime: vm.beginTime,
        endTime: vm.endTime,
      }).then(function (data) {
        wx.stopPullDownRefresh();
        if (data.result) {
          vm.count = data.msg;
          if (data.obj.length > 0) {
            data.obj.forEach(p => {
              if (p.content != "") {
                p.content = JSON.parse(p.content);
                p.phone = tools.getFormPhone(p.content);
              }
            });
          }
          else {
            if (vm.pageIndex == 1) {
              that.setData({
                hasData: false,
              });
            }
          }
          vm.list = vm.list.concat(data.obj);
          if (data.obj.length >= vm.pageSize) {
            vm.pageIndex += 1;
          }
          else
            vm.loadAll = true;


        }
        else
          tools.alert(data.msg);
        vm.isPost = false;
        that.setData({ vm, phoneAuth: true });
      });
    });


  },
  getPageInfo: function () {
    var that = this;
    var d = this.data;
    var user = app.getUser();
    http.postAsync(addr.GetUserPageInfo, { id: d.id, uid: user.uid }).then(function (data) {
      if (data.obj.content != "" && typeof data.obj.content == "string") {
        data.obj.content = JSON.parse(data.obj.content);
        that.setData({ pageInfo: data.obj });
      }
    });
  },
  changeFilter: function (e) {
    var d = this.data;
    var vm = d.vm;
    var index = e.currentTarget.dataset.index;
    var selItem = d.filterList[index];
    d.filterList.forEach(function (obj) {
      obj.sel = false;
    });
    d.filterList[index].sel = true;
    this.setData({
      filterList: d.filterList
    });

    var vm = Object.assign({}, listvm)
    var today_begin = selItem.begin;
    var today_end = selItem.end;
    vm.beginTime = today_begin;
    vm.endTime = today_end;
    this.setData({ vm });
    this.loadMore();
  },
  makePhoneCall: function (e) {
    var phone = e.currentTarget.dataset.phone;
    tools.makePhoneCall(phone);
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
    setTimeout(function(){
      that.init();
    },1500);
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    var d=this.data;
    var vm=d.vm;
    vm.pageIndex+=1;
    this.loadMore();
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  }
})