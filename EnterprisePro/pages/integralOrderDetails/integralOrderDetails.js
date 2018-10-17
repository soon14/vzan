// pages/orderList/orderList.js
const util = require("../../utils/util.js");
const tools = require("../../utils/tools.js");
const addr = require("../../utils/addr.js");
const http = require('../../utils/http.js');
const app = getApp();
Page({

  data: {
    sum: 0,  // 运费
    discountTotal: 0,    // 总价格
    goodsDetails: {},
    totalMoney: "",
    payType: ['微信支付'],
    payText: "",
    // Address:{

    // }
  },
	/**
	 * 生命周期函数--监听页面加载
	 */
  onLoad: function (options) {
    let that = this
    let goodsData = JSON.parse(options.goodsData)
    let freight = Number(goodsData.freight)
    let priceStr = Number(goodsData.priceStr)
    let totalMoney = ((freight).add(priceStr)).toFixed(2)
    this.setData({
      totalMoney: totalMoney,
      goodsDetails: goodsData
    })
    if (goodsData.currentClassify == 0) {
      that.setData({
        payText: "立即兑换"
      })
    } else if (goodsData.currentClassify == 1) {
      that.setData({
        payText: "立即支付"
      })
    }
  },

  //默认地址请求
  AddressRequest: function () {
    var that = this
    http.postAsync(
      addr.Address.GetUserWxAddress,
      {
        appId: app.globalData.appid,
        UserId: app.globalData.userInfo.UserId
      }).then(function (data) {
        if (data.isok == true) {

          var AddressStr = JSON.parse(data.obj.WxAddress.WxAddress)
          that.setData({
            Address: AddressStr
          })
        }
      })
  },
  // 选择我的地址页
  addressGoto: function () {
    let that = this
    wx.chooseAddress({ // 调用微信接口获取地址
      success: function (res) {
        that.setData({ Address: res })
      }
    })
  },
  inputMessage: function (e) {
    let messIput = e.detail.value
    this.data.messIput = messIput
  },
  // 支付请求
  payRequest: function () {
    let that = this
    if (this.data.Address != undefined) {
      let Address = {
        userName: this.data.Address.userName,
        nationalCode: this.data.Address.nationalCode,
        telNumber: this.data.Address.telNumber,
        postalCode: this.data.Address.postalCode,
        provinceName: this.data.Address.provinceName,
        cityName: this.data.Address.cityName,
        countyName: this.data.Address.countyName,
        detailInfo: this.data.Address.detailInfo
      }
      let AddressStr = JSON.stringify(Address)
      http
        .postAsync(
        addr.Address.AddExchangeActivityOrder,
        {
          userId: app.globalData.userInfo.UserId,
          activityId: that.data.goodsDetails.goodsId,
          address: AddressStr,
          appId: app.globalData.appid,
        })
        .then(function (data) {
          if (data.isok) {
            if (that.data.goodsDetails.currentClassify == 0) {
              setTimeout(function () {
                tools.goNewPage('../integralSuccessful/integralSuccessful')
              }, 1000)
            } else if (that.data.goodsDetails.currentClassify == 1) {
              that.wxPayFunc(data.obj)
            }
          } else {
            tools.showToast(data.msg)
          }
        })
    } else {
      tools.showToast("请填写收货地址")
    }

  },
  // 调用微信支付
  wxPayFunc: function (orderId) {
    let that = this
    let oradid = orderId
    let newparam = {
      openId: app.globalData.userInfo.openId,
      orderid: oradid,
      'type': 1,
    }
    util.PayOrder(newparam, {
      failed: function () {
        wx.showToast({
          title: '您取消了支付',
          duration: 1000,
          icon: "loading"
        })
      },
      success: function (res) {
        if (res == "wxpay") {
        } else if (res == "success") {
          tools.showToast("支付成功")
          setTimeout(function () {
            tools.goNewPage('../integralSuccessful/integralSuccessful')
          }, 1000)
        }
      }
    })
  },



})