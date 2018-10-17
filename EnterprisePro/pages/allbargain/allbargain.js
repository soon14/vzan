// pages/allbargain/allbargain.js
var addr = require("../../utils/addr.js");
const tools = require("../../utils/tools.js")
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
  data: {
    pageindexRB: 2,//上拉加载更多的页面
    nowTime: 0,//现实时间的实时毫秒数
    bargainList: [],
    item: ['进行中', '已结束', '全部'],//头部
    changeTypeNums: 2,//改变头部的状态
    item1: [
      {
        img: '/image/test.png', yuanjia: '980.00', xianjia: '900.00', jieshushijian: '1', neirong: '华为P932G版手机华为P932G版手机华为P932G版手机华为P932G版手机华为P932G版手机华为P932G版手机', shengyu: '30'
      },
      {
        img: '/image/test.png', yuanjia: '980.00', xianjia: '900.00', jieshushijian: '活动结束', neirong: '的撒的撒的撒过分个人', shengyu: '30'
      },
      {
        img: '/image/test.png', yuanjia: '980.00', xianjia: '900.00', jieshushijian: '-1', neirong: '爱上大声地', shengyu: '30'
      },
    ]
  },
  // 头部选择时间
  changeType: function (e) {
    var index = e.currentTarget.id
    this.setData({ changeTypeNums: index, pageindexRB: 2 })
    if (index == 0) {
      var typeId = 0
    } else if (index == 1) {
      var typeId = 1
    } else {
      var typeId = -1
    }
    this.inite(typeId, 1, 0)
  },
  navtoBargaindetail: function (e) {
    var Id = e.currentTarget.id
    tools.goNewPage('../bargaindetail/bargaindetail?Id=' + Id)
    // wx.navigateTo({
    //   url: '../bargaindetail/bargaindetail?Id=' + Id,
    // })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    that.inite(-1, 1, 0)

    setInterval(function () { // 倒计时
      that.setData({
        nowTime: new Date().getTime()
      })
    }, 1000)
  },
  // 砍价按钮
  bargainButton: function (e) {
    var Id = e.currentTarget.id
    tools.goNewPage('../bargaindetail/bargaindetail?Id=' + Id + '&isCut=1')
    // wx.navigateTo({
    // 	url: '../bargaindetail/bargaindetail?Id=' + Id + '&isCut=1',
    // })
  },
  showLoading: function () {
    tools.showLoadToast('倒计时中')
  },
  showTimeout: function () {
    tools.showLoadToast('活动已结束')
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
    var that = this
    that.inite(-1, 1, 2)
    wx.stopPullDownRefresh()
    wx.showToast({
      title: '刷新成功',
    })
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    var that = this
    if (that.data.changeTypeNums == 2) {
      var index = -1
    }
    if (that.data.changeTypeNums == 1) {
      var index = 1
    }
    if (that.data.changeTypeNums == 0) {
      var index = 0
    }
    that.inite(index, that.data.pageindexRB, 1)
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  },
  // 获取砍价列表
  inite: function (IsEnd, pageIndex, isReachBootm) {
    var that = this
    wx.request({
      url: addr.Address.GetBargainList, //仅为示例，并非真实的接口地址
      data: {
        appid: app.globalData.appid,
        // appId: 'wxdb77fc8440a79656',
        pageIndex: pageIndex,
        pageSize: 3,
        IsEnd: IsEnd
      },
      method: "GET",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function (res) {
        var changeTypeNums = that.data.changeTypeNums
        var pageindexRB = that.data.pageindexRB
        var bargainList = that.data.bargainList
        var resdata = res.data
        // if (res.data.length == 0) {
        //   return
        // }
        for (var i = 0; i < res.data.length; i++) {
          res.data[i].EndDate = that.ChangeDateFormat(res.data[i].EndDate)
          res.data[i].StartDate = that.ChangeDateFormat(res.data[i].StartDate)
        }
        if (isReachBootm == 1) {
          if (res.data.length != 0) {
            bargainList = bargainList.concat(resdata)
          }
        } else {
          bargainList = res.data
        }
        if (isReachBootm == 1) {
          pageindexRB = pageindexRB + 1
        }
        if (isReachBootm == 2) {
          changeTypeNums = 2
          pageindexRB = 2
        }
        that.setData({ bargainList: bargainList, changeTypeNums: changeTypeNums, pageindexRB: pageindexRB })


      },
      fail: function () {

        wx.showToast({
          title: '获取信息出错',
        })
      }
    })
  },
  ChangeDateFormat: function (val) {
    if (val != null) {
      var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", "").replace("-", "/"), 10));
      //月份为0-11，所以+1，月份小于10时补个0
      var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
      var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
      var hour = date.getHours();
      var minute = date.getMinutes();
      var second = date.getSeconds();
      var dd = date.getFullYear() + "/" + month + "/" + currentDate + " " + hour + ":" + minute + ":" + second;
      // console.log(dd)
      return dd;
    }
    return "";
  },

})