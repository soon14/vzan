// pages/index/share.js
import { http, addr, tools, shareModel } from "../../modules/core.js";
var app = getApp();
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
    var that = this;
    var id = options.id || 0;
    if (id <= 0) {
      tools.backHome();
      return;
    }
    app.getUserInfo().then(function (user) {

      http.postAsync(addr.GetUserPageInfo, { id: id, uid: user.uid }).then(function (data) {
        if (data.obj.content != "" && typeof data.obj.content == "string") {
          data.obj.content = JSON.parse(data.obj.content);
        }
        that.setData({
          pageModel: data.obj
        });
      });
    });
    tools.createPageQRCode("shareCanvas", id)
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
  saveQRCode: function () {
    wx.canvasToTempFilePath({
      canvasId: 'shareCanvas',
      success: function (res) {
        wx.saveImageToPhotosAlbum({
          filePath: res.tempFilePath,
          success: function () {
            tools.alert("图片已经保存在您的相册，快去分享朋友圈吧！", "保存成功", null, null, false);
            setTimeout(function () {
              tools.backHome();
            }, 1500);
          }
        });
      }
    })

  },
  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {
    var that = this;
    var pageModel = that.data.pageModel;
    if (!pageModel) {
      tools.alert("页面不存在");
      return;
    }
    var model = JSON.parse(JSON.stringify(shareModel));
    var pageContent = pageModel.content;
    model.title = pageContent.pageTitle;
    model.path = "/pages/index/pagePreview?id=" + pageModel.id
    model.imgurl = tools.getPageImg(pageContent.coms);
    return tools.share(model, function () {
      wx.showToast({
        title: '分享成功',
      })
      setTimeout(function () {
        tools.backHome()
      }, 1500);
    });
  }
})