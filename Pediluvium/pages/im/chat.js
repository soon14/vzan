// pages/im/chat.js
import { core } from '../../utils/core';
var addr = require("../../utils/addr");
let _get = require('../../utils/lodash.get');
let ispost = false;


var vmDefault = {
  list: [],
  ispost: false,
  loadall: false,
  pageindex: 1,
  pagesize: 20,
  lastid: 0,
  lastids: "",
};

Page({

  /**
   * 页面的初始数据
   */
  data: {
    msg: "",
    showMore: false,
    vm: JSON.parse(JSON.stringify(vmDefault)),
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this;
    var app = getApp();
    var userid = _get(options, "userid");
    var nickname = _get(options, "nickname");
    var headimg = _get(options, "headimg");

    if (userid) {
      this.setData({
        "tuserInfo": {
          userid,
          nickname,
          headimg
        }
      });
      wx.setNavigationBarTitle({
        title: nickname,
      })
    }
    else {
      wx.navigateBack({
        delta: 1,
      })
      return;
    }

    app.getUserInfo(function (e) {
      that.setData({
        "userInfo": e
      });

      var fuserInfo = _get(app.globalData, "fuserInfo", "");
      if (fuserInfo == "") {
        return;
      }
      that.setData({
        "fuserInfo": {
          userid: fuserInfo.userid,//技师ID
          nickname: fuserInfo.nickname,
          headimg: fuserInfo.headimg
        }
      });
      //加载历史消息
      that.loadMore(0);
    })

  },
  loadMore: function () {
    var app = getApp(),
      d = this.data,
      vm = d.vm,
      that = this;

    if (vm.ispost || vm.loadall)
      return;

    vm.ispost = true;

    core.GetHistory({
      appId: app.globalData.appid,
      fuserId: d.fuserInfo.userid,
      tuserId: d.tuserInfo.userid,
      id: vm.lastid,
      fuserType: 1,
    }).then(function (res) {
      console.log(res);
      if (res && res.isok) {
        if (res.data.length < vm.pagesize) {
          vm.loadall = true
        }
        else {
          vm.loadall = false;
        }
        if (res.data.length > 0) {
          vm.list = res.data.concat(vm.list);
          if (vm.lastid === 0) {
            vm.lastid = vm.list[vm.list.length - 1].Id;
            vm.lastids = vm.list[vm.list.length - 1].ids;
          }
          else {
            vm.lastid = vm.list[0].Id;
            vm.lastids = vm.list[0].ids;
          }
        }
      }
      else {
        vm.loadall = true
      }
      vm.ispost = false;
      that.setData({
        vm
      });
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
    var app=getApp();
    if (!app.globalData.ws) {
      app.reConnect();
    }

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
  // onShareAppMessage: function () {

  // }
  clickChat: function () {
    this.hideMore();
  },
  hideMore: function () {
    this.setData({
      showMore: false,
    });
  },
  toggleMore: function () {
    this.setData({
      showMore: !this.data.showMore,
    });
  },
  inputTxtMsg: function (e) {
    var txt = e.detail.value || "";
    if (txt.replace(/\s/gi, "").length == 0) {
      txt = txt.replace(/\s/gi, "");
    }
    this.setData({
      "msg": txt,
    });
  },
  clickToSend: function () {
    var that = this;
    var d = this.data;
    var txt = d.msg;

    txt = txt.replace(/\s/gi, "");
    if (txt.length == 0) {
      wx.showModal({
        content: '不能发送空白消息',
        showCancel: false,
      })
      return;
    }
    this.sendMsg(0, txt);

  },
  sendMsg: function (msgtype, msg) {
    var that = this;
    var app = getApp();
    var ws = _get(app, "globalData.ws") || false;
    var fuserid = that.data.fuserInfo.userid;  //_get(app, "globalData.userInfo.UserId");
    var appid = _get(app, "globalData.appid");
    var tuserid = that.data.tuserInfo.userid;

    if (!ws) {
      app.reConnect();
      wx.showLoading({
        title: '连接中',
      })
      return;
    }
    if (!fuserid || !appid || !tuserid)
      return;

    var msg = {
      appId: appid,
      tuserId: tuserid,
      enterPage: "pages/im/chat",
      fuserId: fuserid,
      msgType: msgtype,
      tuserType: 0,//tuserId类型， 0：普通用户，1:技师
      msg: msg,
      ids: "",
      tempid: fuserid + '_' + new Date().getTime(),//临时ID用来判断消息是否发送成功
    };

    console.log(JSON.stringify(msg));

    wx.sendSocketMessage({
      data: JSON.stringify(msg),
      success: function () {
        console.log("send ok");
      },
      fail: function (res) {
        console.log(res);
        app.globalData.msgQueue.push(msg);
      },
      complete: function () {

        that.setData({
          "msg": "",
        });
      }
    })

  },
  clickImgBtn: function () {
    var that = this;
    core.upload().then(function (res) {
      console.log(res);
      if (res && (Object.prototype.toString.call(res) === "[object Array]") && res.length > 0) {
        for (var i = 0; i < res.length; i++) {
          that.sendMsg(1, res[i]);
        }
      }
    });
  },
  previewImage: function (e) {
    var imgurl = e.currentTarget.dataset.imgurl;
    var urls = [];
    var msglist = _get(this.data, "vm.list");
    if (msglist) {
      urls = msglist.filter(function (obj) {
        return obj.msgType == 1;
      });
      urls = urls.map(function (obj) {
        return obj.msg;
      });
    }
    if (imgurl) {
      wx.previewImage({
        current: imgurl, // 当前显示图片的http链接
        urls: urls // 需要预览的图片http链接列表
      })
    }
  }
})