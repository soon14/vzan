//index.js
import { http, addr, tools, listvm, shareModel } from "../../modules/core.js";
//获取应用实例
const app = getApp()

Page({

  /**
   * 页面的初始数据
   */
  data: {
    hasPage: true,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    wx.hideShareMenu({

    })
    var vm = Object.assign({}, listvm)
    this.setData({ state: options.state});
    vm.state = options.state;
    this.setData({ vm });
    this.loadMore();
  },
  loadMore: function () {
    var that = this;
    var vm = this.data.vm;
    if (vm.isPost || vm.loadAll)
      return;
    app.getUserInfo().then(function (user) {
      vm.isPost = true;
      http.getAsync(addr.GetUserPage, {
        uid: user.uid,
        state: vm.state,
        pageSize: vm.pageSize,
        pageIndex: vm.pageIndex
      }).then(function (data) {
        setTimeout(function () {
          wx.stopPullDownRefresh();
        }, 500);
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
      });
    });


  },
  editPage: function (e) {
    wx.navigateTo({
      url: 'pageset?id=' + e.currentTarget.dataset.id,
    })
  },
  delPage: function (e) {
    var ds = e.currentTarget.dataset;
    var id = ds.id;
    var index = ds.index;
    var that = this;
    wx.showModal({
      title: '提示',
      content: '确定删除吗',
      success: function (d) {
        console.log(d);
        if (d.confirm) {
          http.postAsync(addr.DelPage, { id: id }).then(function (res) {
            console.log(res);
            if (res.result) {
              var list = that.data.vm.list;
              list.splice(index, 1);
              that.setData({
                "vm.list": list,
              });
            }
            else {
              tools.alert(res.msg);
            }
          });
        }
      },
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
    var vm = Object.assign({}, listvm)
    vm.state =this.data.state;
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
  onShareAppMessage: function (e) {
    var model = JSON.parse(JSON.stringify(shareModel));
    console.log(e);
    var ds = e.target.dataset;
    model.title = ds.model.content.pageTitle;
    model.path = "/pages/index/pagePreview?id=" + ds.model.id
    model.imgurl = tools.getPageImg(ds.model.content.coms);
    tools.share(model, function () {
      wx.showToast({
        title: '分享成功',
      })
    });
  },
  puhlishPage: function (e) {
    var ds = e.currentTarget.dataset;
    var id = ds.id;
    wx.navigateTo({
      url: 'pagePreview?id=' + id + '&frompage=temp',
    })
  }
})
