// pages/index/authPhone.js
var app = getApp();
import { http, addr, tools } from "../../modules/core.js";
var timer = null;
Page({

  /**
   * 页面的初始数据
   */
  data: {
    phone: "",
    code: "",
    serverCode: "",
    countDown: 60,
    ispost: false,
    ispostAuth: false,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {

  },
  inputPhone: function (e) {
    this.setData({
      phone: e.detail.value
    });
  },
  inputCode: function (e) {
    this.setData({
      code: e.detail.value
    });
  },
  startCountDown: function () {
    var that = this;
    that.data.countDown -= 1;
    if (that.data.countDown < 1) {
      clearTimeout(timer);
      timer = null;
      that.setData({
        countDown: 60,
        ispost: false,
      });
      return;
    }
    else {
      that.setData({
        countDown: that.data.countDown,
        ispost: false,
      });
      timer = setTimeout(function () {
        that.startCountDown();
      }, 1000);
    }

  },
  getAuthCode: function () {
    var that = this;
    var d = this.data;
    if (!/^1[\d]{10}$/g.test(d.phone)) {
      tools.alert("请输入正确的手机号码", "提示");
      return;
    }
    var uinfo = app.getUser();
    if (uinfo == "") {
      tools.alert("请在设置中开启允许获取用户信息", "提示");
      return;
    }
    if (d.ispost)
      return;
    if (!d.ispost)
      d.ispost = true;

    http.postAsync(addr.SendPhoneAuth, { tel: d.phone, openId: uinfo.openId, sendType: 8, appid: app.APPID })
      .then(function (data) {
        console.log(data);
        if (data.isok) {
          that.setData({
            serverCode: data.dataObj
          });
          that.startCountDown();
        }
        else {
          tools.alert(data.Msg);
          d.ispost = false;
        }
      });
  },
  submitAuthCode: function () {
    var d = this.data;
    var uinfo = app.getUser();
    if (uinfo == "") {
      tools.alert("请在设置中开启允许获取用户信息", "提示");
      return;
    }
    if (d.code == "") {
      tools.alert("请输入验证码", "提示");
      return;
    }
    if (d.serverCode != "" && d.code != d.serverCode) {
      tools.alert("验证码输入错误", "提示");
      return;
    }
    if (d.ispostAuth)
      return;
    if (!d.ispostAuth)
      d.ispostAuth = true;

    http.postAsync(addr.Submitauth, { tel: d.phone, openId: uinfo.openId, authCode: d.code })
      .then(function (data) {
        if (data.isok) {
          wx.showToast({
            title: '绑定成功！',
          })
          setTimeout(function () {
            wx.removeStorage({
              key: 'userInfo',
              success: function (res) {
                wx.reLaunch({
                  url: 'index'
                })
              },
            })
          }, 1500);

        }
        else {
          tools.alert(data.Msg);
          d.ispostAuth = false;
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