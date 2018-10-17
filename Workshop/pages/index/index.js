//index.js
import { http, addr, tools, listvm, shareModel } from "../../modules/core.js";
//获取应用实例
const app = getApp()

Page({

  /**
   * 页面的初始数据
   */
  data: {
    popQRCodeShow: false,
    phoneAuth: true,
    user: null,
    hasData: true,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    //wx.hideShareMenu({})
    this.init();
  },
  init: function () {
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
      that.setData({ user });
      if (user.phone == "") {
        that.setData({ phoneAuth: false });
        return;
      }
      that.setData({
        "vm.isPost": true
      });

      http.getAsync(addr.GetUserPage, {
        uid: user.uid,
        pageSize: vm.pageSize,
        pageIndex: vm.pageIndex
      }).then(function (data) {
        if (data.result) {
          if (data.obj.length > 0) {
            data.obj.forEach(p => {
              if (p.content != "") {
                p.content = JSON.parse(p.content);
                p.imgurl = tools.getPageImg(p.content.coms);
                p.des = tools.getPageDes(p.content.coms);
              }
            });
          }
          else {
            if (vm.pageIndex == 1) {
              that.setData({
                hasData: false,
              });
            }
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
        that.setData({ vm, phoneAuth: true });
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
    var that = this;
    wx.showModal({
      title: '提示',
      content: '确定要删除吗？',
      success: function (res) {
        if (res.confirm) {
          tools.delPage(ds.id).then(function (data) {
            wx.showToast({
              title: data.msg,
            })
            if (data.result) {
              var list = that.data.vm.list;
              list.splice(ds.index, 1);
              that.setData({
                "vm.list": list
              });
            }
          });
        }
      },
    })
  },
  viewPage: function (e) {
    wx.navigateTo({
      url: 'pagePreview?id=' + e.currentTarget.dataset.id,
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
    //wx.hideShareMenu({})
    this.init();
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
    this.init();
    setTimeout(function () {
      wx.stopPullDownRefresh()
    }, 500);
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },
  getPhoneNumber: function (e) {
    var that = this;
    console.log(e);
    if (e.detail.errMsg == "getPhoneNumber:fail user deny") {
      wx.navigateTo({
        url: 'authPhone',
      })
      return;
    }
    wx.login({
      success: function (res) {

        app.login(res.code, e.detail.encryptedData, that.data.user.uid, e.detail.iv, 1).then(function (data) {
          console.log(data);
          if (data && data.result) {
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
        });
      }
    })

  },
  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function (e) {
    var model = JSON.parse(JSON.stringify(shareModel));
    console.log(e);

    if (e.target) {
      var ds = e.target.dataset;
      model.title = ds.model.content.pageTitle;
      model.path = "/pages/index/pagePreview?id=" + ds.model.id
      model.imgurl = tools.getPageImg(ds.model.content.coms);
    }
    return tools.share(model, function () {
      wx.showToast({
        title: '分享成功',
      })
    });
  },
  showPageQRCode: function (e) {
    var id = e.currentTarget.dataset.id;
    tools.createPageQRCode("shareCanvas", id);

    this.setData({
      popQRCodeShow: true,
    });

    wx.setNavigationBarColor({
      frontColor: '#ffffff',
      backgroundColor: '#b1b1b1',
    })
  },
  closeQRCodePop: function () {
    wx.setNavigationBarColor({
      frontColor: '#000000',
      backgroundColor: '#ffffff',
    })
    this.setData({
      popQRCodeShow: false,
    });

  }
})
