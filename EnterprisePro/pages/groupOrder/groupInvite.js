var http = require("../../utils/http.js");
var addr = require("../../utils/addr.js");
var tools = require("../../utils/tools.js");
var util = require("../../utils/util.js");
var WxParse = require("../../utils/wxParse/wxParse.js");
var app = getApp();

Page({

  /**
   * 页面的初始数据
   */
  data: {
    tab: [
      { name: "商品详情", sel: true },
      { name: "拼团规则", sel: false },
    ],
    //距离结束
    fromTheEnd: {
      dd: "00",
      hh: "00",
      mm: "00",
      ss: "00",
    }
  },
  // 拨打电话
  makephonecall: function () {
    wx.makePhoneCall({
      phoneNumber: app.globalData.customerphone,
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var _gsid = options.gsid || 0;
    if (_gsid <= 0) {
      wx.navigateBack({
        delta: 1,
      })
    }

    var that = this;

    tools
      .getUserInfo()
      .then(function (user) {
        that.setData({
          "vm.uinfo": user,
          "vm.gsid": _gsid,
        });
        that.init();
      })
  },
  init: function () {
    var that = this;
    var _vm = that.data.vm;
    http
      .postAsync(addr.Address.GetInvitePageData, { appId: app.globalData.appid, gsid: _vm.gsid, })
      .then(function (data) {
        console.log(data);
        var _g = data.postdata
        //判断当前用户是否已经在团之中
        var isingroup = false;
        if (_g.GroupUserList.length > 0) {
          var obj = _g.GroupUserList.find(function (item) {
            return item.Id == _vm.uinfo.UserId
          });
          isingroup = (obj == undefined ? false : true);
        }
        //转换富文本
        _g.Description = WxParse.wxParse('Description', 'html', _g.Description||"", that, 5)

        if (_g.GroupUserList.length >= 4) {
          _g.GroupUserList = _g.GroupUserList.slice(0, 4);
          _g.NeedNum_fmt = 0;
        }
        else {
          if (_g.NeedNum + _g.GroupUserList.length <= 4) {
            _g.NeedNum_fmt = _g.NeedNum;
          }
          else {
            _g.NeedNum_fmt = 4 - _g.GroupUserList.length;
          }

        }

        that.setData({
          "isingroup": isingroup,
          "vm.groupDetail": _g,
        });
        if (_g.MState == 1) {
          tools.initEndClock(_g.ValidDateStart, _g.ValidDateEnd, that);
        }
      });
  },
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
  clidkAddGroup: function (e) {
    var _groupid = e.currentTarget.dataset.groupid;
    var _vm = this.data.vm;
    var url = '/pages/groupOrder/groupOrder?groupid=' + _groupid + "&isGroup=1&isGHead=0&gsid=" + _vm.gsid
    console.log(url);
    wx.navigateTo({
      url: url,
    })
  },
  clickGroupItem:function(e){
    var _groupid = e.currentTarget.dataset.groupid;
    wx.navigateTo({
      url: '/pages/groupDetail/groupDetail?id=' + _groupid,
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
  onShareAppMessage: function (res) {
    console.log(res);
    var group = res.target.dataset.group;

    return tools.share(group);
  }
})