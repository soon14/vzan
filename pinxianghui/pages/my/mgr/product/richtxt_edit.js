// pages/my/mgr/product/richtxt_edit.js
import {
  core,
  vm,
  http,
  addr,
} from "../../../../utils/core";


Page({

  /**
   * 页面的初始数据
   */
  data: {

  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    let that = this;
    if (options.id) {
      that.setData({
        "id": options.id,
        "webViewURL": addr.richtexturl + "?type=pin&id=" + options.id + "#wechat_redirect"
      });
    }
  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function() {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function() {
    
  },
  saveContent: function(e) {
    var that = this;
    if (e.detail && e.detail.data.length > 0) {
      core.setStorageSync("temp_p_description_" + that.data.id, e.detail.data[0].txt);
    }
  },
  reload: function() {
    this.vm = JSON.parse(JSON.stringify(vm));

    setTimeout(() => {
      wx.stopPullDownRefresh();
    }, 300);
  },
  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function() {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function() {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function() {
    this.reload();
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function() {

  }
})