// pages/myAddress/myAddress.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    openId: '',
    Address: [
      // { NickName: '袁大宝', TelePhone: '183964655', IsDefault: 0, Province: "", CityCode: "", AreaCode:"", Address: '广东省广州市天河区东圃一马路汇报商业街A栋B区B019A栋B区B019A栋B区B019A栋B区B019' },
    ],
    selectAddress: 0,
  },
  // 跳转到地址编辑页
  navToeditorAddress: function (e) {
    var id = e.currentTarget.id
    var address = this.data.Address.find(f => f.Id == id)
    var url = "../editorAddress/editorAddress"
    if (address != undefined && address != null) {
      var addressjson = JSON.stringify(address)
      url = '../editorAddress/editorAddress?addressjson=' + addressjson

    }
    app.goNewPage(url)
  },
  //初始化
  inite: function () {
    var that = this
    var url = addr.Address.GetMyAddress
    var param = {
      appid: app.globalData.appid,
      openid: app.globalData.userInfo.openId,
      addressId: 0,
    }
    var method = "GET"
    network.requestData(url, param, method, function (e) {
      if (e.data.isok == 1) {
        that.setData({
          Address: e.data.postdata.addressList
        })
      }
    })
  },

  // 选择默认地址
  setChoose: function (e) {
    var index = e.currentTarget.id
    var Address = this.data.Address
    if (Address != undefined && Address != null && Address.length > 0) {
      for (var i = 0; i < Address.length; i++) {
        var model = Address[i]
        if (model.Id == index) {
          model.IsDefault = 1
        }
        else {
          model.IsDefault = 0
        }
      }
    }
    this.setData(this.data)

    var url = addr.Address.setMyAddressDefault
    var param = {
      appid: app.globalData.appid,
      openid: app.globalData.userInfo.openId,
      addressId: index
    }
    var method = "post"
    network.requestData(url, param, method, function (e) {
      console.log(e)
    })
    wx.showToast({
      title: '设置成功',
      duration: 1000
    })
    setTimeout(function () {
      wx.navigateBack({
        delta: 1
      })
    }, 1000)

  },

  //删除地址
  DeleteAddress: function (e) {
    var id = e.currentTarget.id
    var that = this
    var url = addr.Address.deleteMyAddress
    var param = {
      appid: app.globalData.appid,
      openid: app.globalData.userInfo.openId,
      AddressId: id
    }
    var method = "get"
    app.ShowConfirm("确定删除该地址？", function (res) {
      network.requestData(url, param, method, function (e) {
        app.showToast(e.data.msg)
        if (e.data.isok == 1) {
          that.inite()
        }
      })
    })

  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    if (app.globalData.userInfo != undefined && app.globalData.userInfo.openId != undefined) {
      that.inite()
    }
    else {
      app.getUserInfo(function () {
        that.inite()
      })
    }
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
  onShareAppMessage: function () {

  },
})