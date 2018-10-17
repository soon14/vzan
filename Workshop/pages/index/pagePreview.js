// pages/index/pagePreview.js
var WxParse = require("../../modules/wxParse/wxParse.js");
import { http, addr, tools, pageData } from "../../modules/core.js";
var app = getApp();
var ispost = false;
Page({

  /**
   * 页面的初始数据
   */
  data: {
    formSubmit: false,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var id = options.id || 0;
    var frompage = options.frompage || "";
    var vm = Object.assign({}, pageData);
    vm.id = id;
    this.setData({ id, vm, frompage });
    this.init();
  },
  init: function () {
    var vm = Object.assign({}, pageData);
    var id = this.data.vm.id;
    var that = this;
    app.getUserInfo().then(function (user) {
      that.setData({ user });
      http.postAsync(addr.GetUserPageInfo, { id: id, uid: user.uid }).then(function (data) {
        if (data.obj.content != "" && typeof data.obj.content == "string") {
          data.obj.content = JSON.parse(data.obj.content);
          var page = data.obj.content;
          if (page.pageTitle != "") {
            wx.setNavigationBarTitle({
              title: page.pageTitle,
            })
          }
          data.obj.content.coms.forEach(function (item) {
            if (item.type == "slider") {
              item.show = !item.items.every(function (slider_item) {
                return slider_item.src == "";
              });
            }
          });
        }
        that.setData({ vm: data.obj });
      });
    });

  },
  bindDateChange: function (e) {
    var ds = e.currentTarget.dataset;
    var comIndex = ds.comIndex;
    var itemIndex = ds.itemIndex;
    var val = e.detail.value;
    var key = "vm.content.coms[" + comIndex + "].items[" + itemIndex + "].value";
    this.setData({
      [key]: val
    });
  },
  bindRadioChange: function (e) {
    var ds = e.currentTarget.dataset;
    var comIndex = ds.comIndex;
    var itemIndex = ds.itemIndex;

    var key = "vm.content.coms[" + comIndex + "].items[" + itemIndex + "].value";
    var val = this.data.vm.content.coms[comIndex].items[itemIndex].items[e.detail.value].name;
    this.setData({
      [key]: val
    });
  },
  submitForm: function (e) {
    console.log(e);
    var that = this;
    if (that.data.vm.id <= 0) {
      tools.alert("页面不存在！");
      return;
    }
    var user = app.getUser();
    if (user == "") {
      tools.alert("请先登录！");
      return;
    }
    var formdata = e.detail.value;
    for (var key in formdata) {
      if (formdata[key] == "") {
        tools.alert("请填写" + key);
        return;
      }
    }
    if (ispost)
      return;
    ispost = true;
    var saveModel = {
      id: 0,
      pageid: that.data.vm.id,
      userid: user.uid,
      content: JSON.stringify(formdata),
      state: 0,
    }
    http.postAsync(addr.SaveFormData, saveModel).then(function (data) {
      console.log(data);
      if (data.result) {
        wx.showToast({
          title: data.msg,
        })
        setTimeout(function () {
          that.setData({
            formSubmit: true
          });
        }, 1500);
      }

    });
  },
  save: function (e) {
    var ds = e.currentTarget.dataset;
    var state = ds.state;
    var that = this;
    that.data.vm.state = state;
    tools.savePage(that, function (pageid) {

      //保存草稿=返回首页
      if (state == 0) {
        wx.showToast({
          title: '保存成功',
        })
        setTimeout(function () {
          tools.backHome()
        }, 300);
      }
      //发布=跳转到分享页
      else if (state == 1) {
        wx.showToast({
          title: '发布成功',
        })
        setTimeout(function () {
          wx.navigateTo({
            url: 'share?id=' + pageid,
          })
        }, 500);
      }
    });
  },
  //继续编辑
  editAgain: function () {
    var that = this;
    tools.backHome(function () {
      wx.navigateTo({
        url: '/pages/index/pageset?id=' + that.data.vm.id,
      })
    })
  },
  playVideo: function (e) {
    var ds = e.currentTarget.dataset;
    var comIndex = ds.comIndex;
    var key = "vm.content.coms[" + comIndex + "].autoplay";
    this.setData({
      [key]: true,
    });
    console.log(comIndex);
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
    setTimeout(function () {
      var vm = Object.assign({}, pageData);

      vm.id = that.data.id;
      that.setData({ vm });
      that.init();
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
  makecall: function (e) {
    var phone = e.currentTarget.dataset.phone;
    console.log(phone);
    wx.makePhoneCall({
      phoneNumber: phone
    })
  },
})