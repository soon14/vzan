// pages/index/formPage.js
import { http, addr, tools, listvm, shareModel } from "../../modules/core.js";
const app = getApp()
Page({

  /**
   * 页面的初始数据
   */
  data: {

  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    wx.hideShareMenu({

    })
    var vm = Object.assign({}, listvm)
    this.setData({ vm });
    this.loadMore();
  },
  loadMore: function () {
    var that = this;
    var vm = this.data.vm;
    if (vm.isPost || vm.loadAll)
      return;
    app.getUserInfo().then(function (user) {

      that.setData({
        "vm.isPost": true
      });
      http.getAsync(addr.GetUserPage, {
        uid: user.uid,
        state: vm.state,
        pageSize: vm.pageSize,
        pageIndex: vm.pageIndex,
        filterForm: 1,
      }).then(function (data) {
        if (data.result) {
          if (data.obj.length > 0) {
            data.obj.forEach(p => {
              if (p.content != "") {
                p.content = JSON.parse(p.content);
              }
              p.poster = tools.getPageImg(p.content.coms);
              p.des = tools.getPageDes(p.content.coms);
            });
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
        that.setData({ vm });
        wx.stopPullDownRefresh();
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
    var vm = Object.assign({}, listvm)
    this.setData({ vm });
    this.loadMore();
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