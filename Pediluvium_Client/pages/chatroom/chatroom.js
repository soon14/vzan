// pages/chatroom/chatroom.js
var app = getApp();
// var userInfo = wx.getStorageSync('userInfo');
var appid = app.globalData.appid;
Page({

  /**
   * 页面的初始数据
   */
  data: {
    connectTime: 0,//连接次数
    isreconnect: true,
    headImgs: [],
    msgs: [],
    isfocus: false,
    msgModel: {
      appId: appid,
      tuserId: 0,
      headImg: '',
      command: 0,//0:聊天消息，1:实时在线人数
      msgtype: 0,//消息类型 (-1:系统消息离开，0:文本，1:图像, 2:语音, 3:短视频，4:系统消息进入,5:获取在线客户)
      msg: ''
    },
    connected: false//连接状态
  },
  inputContext: function (e) {
    this.data.msgtext = e.detail.value
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this;
    that.setData({ userInfo: wx.getStorageSync('userInfo') });
    
    that.data.msgModel.headImg = that.data.userInfo.avatarUrl;
    that.data.msgModel.msgtype = 4;
    that.data.msgModel.msg = '欢迎' + that.data.userInfo.nickName + '进入大厅';
    that.data.msgModel.enterPage = 'im';
    that.sendSocketMessage();
  },
  onShow:function(){
    var that = this;
    wx.onSocketMessage(function (res) {
      console.log('收到服务器内容：' + res.data)
      var resData = JSON.parse(res.data);
      if (resData.msgtype === 5) {
        that.setData({ headImgs: resData.userList });
      } else {
        that.data.msgs=that.data.msgs.concat(resData.msg);
        that.setData({ msgs: that.data.msgs });
      }
      console.log(resData);
    })
  },
  //
  onUnload: function () {
    var that = this;
    that.data.msgModel.headImg = that.data.userInfo.avatarUrl;
    that.data.msgModel.msgtype = -1;
    that.data.msgModel.msg = that.data.userInfo.nickName + '离开大厅';
    that.data.msgModel.enterPage = 'chatroom';
    that.sendSocketMessage();

  },
  sendMsg: function () {
    var that = this;
    that.data.msgModel.msg = that.data.msgtext;
    that.data.msgModel.msgtype = 0;
    that.data.msgModel.enterPage = 'chatroom';
    that.sendSocketMessage();
    that.setData({ msgtext: '' });
  },
  sendSocketMessage: function () {
    var that = this;
    wx.sendSocketMessage({
      data: JSON.stringify(that.data.msgModel),
      success: function () {
        //that.data.msgs.push(that.data.msgModel);
        //that.setData({ msgs: that.data.msgs });
      },
      fail: function () {
        wx.showToast({ title: '发送失败', icon: 'none' });
      }
    })
  },
  entervistorList: function () {
    wx.navigateTo({
      url: '../vistorList/vistorList',
    })
  }
})