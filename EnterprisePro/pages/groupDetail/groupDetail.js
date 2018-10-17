const http = require('../../utils/http.js');
var tools = require("../../utils/tools.js");
var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var WxParse = require("../../utils/wxParse/wxParse.js");
var app = getApp();

var timer_countdown = null;
var isEndClock = null;

import { core } from "../../utils/core.js";
Page({

  /**
   * 页面的初始数据
   */
  data: {
    clientTel: 0,
    tab: [
      { name: "商品详情", sel: true },
      { name: "拼团规则", sel: false },
    ],
    groupstate: '',
    //距离结束
    fromTheEnd: {
      dd: "00",
      hh: "00",
      mm: "00",
      ss: "00",
    },
    imSwitch: false,
 
  },
  // 获取用户手机号码
  getPhoneNumber: function (e) {
    var that = this
    app.globalData.telEncryptedData = e.detail.encryptedData
    app.globalData.telIv = e.detail.iv
    app.globalData.isgetTel = 1
    app.getUserInfo(function (res) {
      if (res.TelePhone != '未绑定') {
        that.setData({ clientTel: res.TelePhone })
      }
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this;
    app.getUserInfo(function (uinfo) {
      that.setData({ clientTel: uinfo.TelePhone })
    })

    var _groupid = options.id;
    this.data.groupid = _groupid;
    util.setPageSkin(that);
    
    getApp().GetStoreConfig(function (config) {
      if (config && config.funJoinModel) {
        that.setData({
          imSwitch: (config.funJoinModel.imSwitch && config.kfInfo)
        });
      }
    });

    core.GetPageSetting().then(function (pageset) {
     

      util.setPageSkin(that);
    });
  },
  // 查看轮播大图
  previewSwiper: function (e) {
    var imageArray = []
    var index = e.currentTarget.id
    for (var i = 0; i < this.data.groupdetail.ImgList.length; i++) {
      imageArray.push(this.data.groupdetail.ImgList[i].filepath)
    }

    var previewImage = imageArray[index]
    wx.previewImage({
      current: previewImage,
      urls: imageArray
    })
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
    this.initGroupInfo();
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

  },
  initGroupInfo: function () {
    var that = this;
    var groupid = that.data.groupid;
    if (!groupid) {
      wx.showModal({
        title: '错误提示',
        content: '团购不存在！',
      })
      return;
    }


    http
      .postAsync(addr.Address.GetGroupDetail,
      {
        appId: app.globalData.appid,
        groupId: groupid,
      })
      .then(function (data) {

        if (!data.isok) {
          tools.alert("信息", data.msg);
          wx.navigateBack({
            delta: 1
          })
          return;
        }
        var _g = data.groupdetail;
        //选取前5个用户
        if (_g.GroupUserList && _g.GroupUserList.length > 0) {
          _g.GroupUserList = _g.GroupUserList.slice(0, 5)
        }
        //选取两个可以参加的团
        if (_g.GroupSponsorList && _g.GroupSponsorList.length > 0) {
          _g.GroupSponsorList = _g.GroupSponsorList.slice(0, 2)
        }
        //转换富文本
        _g.Description = WxParse.wxParse('Description', 'html', _g.Description || "", that, 5)

        _g.ValidDateStart = _g.ValidDateStart.replace(/-/g, "/");
        _g.ValidDateEnd = _g.ValidDateEnd.replace(/-/g, "/");

        //距离结束倒计时
        tools.initEndClock(_g.ValidDateStart, _g.ValidDateEnd, that);

        //保存
        that.setData({ groupdetail: _g });
        that.initCountDown();
      });
  },
  //点击：商品详情/拼团规则
  clickTab: function (e) {
    this.data.tab.forEach(function (o, i) {
      o.sel = false;
    });
    this.data.tab[e.currentTarget.dataset.index].sel = true;
    var key = "tab[" + e.currentTarget.dataset.index + "].sel";
    this.setData({
      tab: this.data.tab
    });

  },

  getUserInfo: function () {
    var _user = app.globalData.userInfo;
    return new Promise(function (resolve, reject) {
      if (!_user.UserId) {
        app.getUserInfo(function (uinfo) {
          resolve(uinfo);
        })
      }
      else {
        resolve(_user);
      }
    })

  },
  //一键拼团
  clidkAddGroup: function (e) {
    // 提交备用formId
    var formId = e.detail.formId
    util.commitFormId(formId, this)
    var _groupid = e.currentTarget.dataset.groupid;
    wx.navigateTo({
      url: '/pages/groupOrder/groupOrder?groupid=' + _groupid + "&isGroup=1&isGHead=1&gsid=0"
    })
  },
  //单独购买
  clidkAddGroupSingle: function (e) {
    // 提交备用formId
    var formId = e.detail.formId
    util.commitFormId(formId, this)
    var _groupid = e.currentTarget.dataset.groupid;
    wx.navigateTo({
      url: '/pages/groupOrder/groupOrder?groupid=' + _groupid + "&isGroup=0&isGHead=0&gsid=0"
    })
  },
  joinGroup: function (e) {
    var _ds = e.currentTarget.dataset.group;
    var _groupid = _ds.GroupId;
    var _gsid = _ds.Id;
    wx.navigateTo({
      url: '/pages/groupOrder/groupInvite?groupid=' + _groupid + "&isGroup=1&isGHead=0&gsid=" + _gsid,
    })
  },
  initCountDown: function () {
    var that = this;
    if (this.data.groupdetail.GroupSponsorList != null) {
      var list = this.data.groupdetail.GroupSponsorList;
      if (list.length > 0) {
        for (var i = list.length - 1; i >= 0; i--) {
          if (list[i].MState == 1) {

          }

          var timespan = tools.getTimeSpan(list[i].ShowEndTime);
          if (timespan <= 0) {
            list.splice(i, 1)
          }
          else {
            var timeFormatArray = tools.formatMillisecond(timespan);
            var timeFormat = "";
            // if (timeFormatArray[0] > 0) {
            //   timeFormat += timeFormatArray[0] + '天';
            // }
            timeFormat += timeFormatArray[1] + ':' + timeFormatArray[2] + ':' + timeFormatArray[3];
            list[i].countdown = timeFormat;
          }



        }
        that.setData({
          "groupdetail.GroupSponsorList": list
        });

        timer_countdown = setTimeout(function () {
          that.initCountDown();
        }, 1000);

      }

    }
  },
  gochat: function () {
    tools.gochat();
  },


})