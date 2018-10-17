// pages/projectinfo/projectinfo.js
var template = require('../../template/template.js')
var tool = require("../../template/Pediluvium_Client.js")
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
		chooseid: 0,
		infoid: 0,
	},
	chooseid: function (e) {
		this.setData({ chooseid: e.currentTarget.id })
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		this.data.infoid = options.id
		this.setData({ command: this.data.command,userInfo:wx.getStorageSync('userInfo') })
   
		tool.GetStoreInfo(this)
		tool.GetServiceInfo(this, options.id)
	},
	goNewPage: function (e) {
		template.goNewPage('../artificerInfo/artificerInfo?id=' + e.currentTarget.id)
	},
  getLogin: function (e) {
    let that = this
    let _g = e.detail
    if (_g.errMsg == "getUserInfo:fail auth deny") {
      return;
    }
    wx.login({
      success: function (res) {
        let vm = {
          iv: _g.iv,
          code: res.code,
          data: _g.encryptedData,
          signature: _g.signature,
          isphonedata: 0,
        }
        app.login_old(vm, function (data) {
          that.setData({ userInfo: data })
        })
      }
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
		tool.GetStoreInfo(this)
		tool.GetServiceInfo(this, this.data.infoid)
		template.stopPullDown()
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
  clickToChat: function (e) {
    console.log(e);
    var ds = e.currentTarget.dataset;
    var userid = ds.userid;
    var nickname = (ds.nickname || "").replace(/\s/gi, "");
    var headimg = ds.headimg;
    wx.navigateTo({
      url: '../im/chat?userid=' + userid + "&nickname=" + nickname + "&headimg=" + headimg,
    })
  },
})