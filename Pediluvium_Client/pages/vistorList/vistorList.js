// pages/vistorList/vistorList.js
var app = getApp();
var appid = app.globalData.appid;
Page({

  /**
   * 页面的初始数据
   */
  data: {
    msgModel: {
      appId: '',
      tuserId: 0,
      headImg: '',
      msg: ''
    },
    userList: []
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this;
    app.getUserInfo(function () {
      setTimeout(function () {
        that.sendSocketMessage();
      }, 500)
    })
    wx.onSocketMessage(function (res) {
      console.log('收到服务器内容：' + res.data)
      var resData = JSON.parse(res.data);
      console.log(resData);
      that.setData({ userList: resData.userList });
    })
  },
  sendSocketMessage: function () {
    var msg = { appid: appid, enterPage: 'vistorList' };
    var that = this;
    wx.sendSocketMessage({
      data: JSON.stringify(msg),
      success: function () {
      },
      fail: function () {
        wx.showToast({ title: '请求失败', icon: 'none' });
      }
    })
  },
  PrivateChat:function(){
    wx.navigateTo({
      url: '../privatechat/privatechat',
    })
  }
})