// pages/editorAddress/editorAddress.js
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
    Id: 0,
    // 省市区选择器
    // region: ['广东省', '广州市', '海珠区'],
    region: ['全部', '广州市', '海珠区'],
    regionstr: "请选择所在地区",
    customItem: '全部',
    nickname:"",
    phone:"",
    address:"",
    // 街道选择器
    array: ['请选择', '中国', '巴西', '日本'],
    index: 0,
    // 入口循环
    // inputAddress: ['收货人', '联系电话', '所在地区', '街道'],
  },
  // 省市区选择器
  bindRegionChange: function (e) {
    console.log('picker发送选择改变，携带值为', e.detail.value)
    this.setData({
      region: e.detail.value,
      regionstr: e.detail.value[0] + " " + e.detail.value[1] + " " + e.detail.value[2]
    })
  },
  // 街道选择器
  bindPickerChange: function (e) {
    console.log('picker发送选择改变，携带值为', e.detail.value)
    this.setData({
      index: e.detail.value
    })
  },
  formSubmit: function (e) {
    console.log('form发生了submit事件，携带数据为：', e.detail.value)
    var name = e.detail.value.name
    var phone = e.detail.value.phone
    var regionstr = this.data.regionstr
    var region = this.data.region
    var address = e.detail.value.address
    var id = this.data.Id
    if (name.trim().length <= 0) {
      app.ShowMsg("请输入收获人")
      return
    }
    if (phone.trim().length <= 0) {
      app.ShowMsg("请输入联系电话")
      return
    }
    if (region[0] == "全部" || region[1] == "全部" || region[2] == "全部") {
      app.ShowMsg("请选择地区")
      return
    }
    // if (regionstr.trim().length <= 0 || regionstr =="请选择所在地区") {
    //   app.ShowMsg("请选择地区")
    //   return
    // }
    if (address.trim().length <= 0) {
      app.ShowMsg("请输入详细地址")
      return
    }

    this.saveAddress(id, region[0], region[1], region[2], name, phone, address)
  },
  saveAddress: function (id, Province, CityCode, AreaCode, name, phone, address) {
    var data = { Id: id, Province: Province, CityCode: CityCode, AreaCode: AreaCode, NickName: name, TelePhone: phone, Address: address }
    var url = addr.Address.AddOrEditMyAddressDefault
    var param = {
      appid: app.globalData.appid,
      openid: app.globalData.userInfo.openId,
      addressjson: JSON.stringify(data)
    }
    var method = "POST"
    this.requestData(url, param, method, function (e) {
      if (e.data.isok == 1) {
        app.showToast("保存成功")
        app.goBackPage(1)
      }
    })
  },
  requestData: function (url, params, method, callback) {
    wx.request({
      url: url,
      method: method,
      head: {
        'Content-Type': 'application/json'
      },
      data: params,
      success: function (success) {
        callback(success)
      },
      fail: function (fail) {
        onError("网络请求异常", fail.statusCode, that);
      },
    });
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
      that.requestData(url, param, method, function (e) {
        app.showToast(e.data.msg)
        if (e.data.isok == 1) {
          app.goBackPage(1)
        }
      })
    })

  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    if (options.addressjson!=undefined)
    {
      var address = JSON.parse(options.addressjson)
      if (address != undefined && address != null) {
        var region = [address.Province, address.CityCode, address.AreaCode]
        var regionstr = address.Province + " " + address.CityCode + " " + address.AreaCode
        this.setData({
          region: region,
          regionstr: regionstr,
          Id: address.Id,
          nickname: address.NickName,
          phone: address.TelePhone,
          address: address.Address
        })
      }
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
    var url = "pages/myAddress/myAddress"
    app.reloadpagebyurl("", url)
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

  }
})