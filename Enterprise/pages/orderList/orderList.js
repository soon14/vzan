// pages/orderList/orderList.js
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
    gooddetaiSpecInfo: "",  //商品详情也传值
    gooddetailqty: 0,
    gooddetailattrSpacStr: '',
    gooddetailgoodid: '',
    Address: [],// 默认地址
    openId: '',
    sum: 0,  // 运费
    totalMoney: 0,    // 总价格
    amount: 0,    // 共amount件
    nums: 0,    // 商品数量
    chooseModel: [],  // 配送选择器属性
    array1: ['微信支付', '余额支付'],//支付方式
    array: [],
    index: 0,
    index1: 0,//支付方式下标 0使用微信支付 1使用余额支付
    goodCarIdStr: "",    //购物车Ids,以，分开
    goods: [
      { ImgUrl: '', SpecInfo: "", Introduction: "", Price: 1.01, Count: 0 }
    ],
    sumprice: 0
  },
  // 选择配送
  bindPickerChange: function (e) {
    console.log('picker发送选择改变，携带值为', e.detail.value)
    console.log(this.data.chooseModel)
    this.setData({
      index: e.detail.value,
      sumprice: (parseFloat(this.data.totalMoney) + parseFloat(this.data.chooseModel[e.detail.value].sum)).toFixed(2)
    })
    console.log(this.data.chooseModel[this.data.index])
  },
  // 跳转到我的地址页
  navTomyAddress: function () {
    var that = this
    // wx.navigateTo({
    //   url: '../myAddress/myAddress',
    // })
    wx.chooseAddress({ // 调用微信接口获取地址
      success: function (res) {
        that.setData({ Address: res })
      }
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    console.log('~~~~我是options~~~~~~', options)
    var amount = that.data.amount

    console.log('传进购物车的数据', options)
    if (options.datajson != undefined) {
      var gooddetaiSpecInfo = options.SpecInfo
      var gooddetailqty = options.qty
      var gooddetailattrSpacStr = options.attrSpacStr
      var gooddetailgoodid = options.goodid
      this.data.gooddetailqty = gooddetailqty
      this.data.gooddetaiSpecInfo = gooddetaiSpecInfo
      this.data.gooddetailattrSpacStr = gooddetailattrSpacStr
      this.data.gooddetailgoodid = gooddetailgoodid
      var menu = JSON.parse(options.datajson)
      var goodCarIdStr = options.goodCarIdStr
      var nums = menu[0].Count
      var goodtotalMoney = 0
      for (var i = 0; i < menu.length; i++) {
        amount += menu[i].Count
        goodtotalMoney += parseFloat(menu[i].Price) * parseFloat(menu[i].Count)
      }
      var goodtotalmoney = parseFloat(goodtotalMoney).toFixed(2)
      console.log(menu)
      that.setData({
        goods: menu,
        nums: nums,
        goodtotalMoney: goodtotalmoney,
        totalMoney: options.totalMoney,
        amount: amount,
        goodCarIdStr: goodCarIdStr,
      })
    }
    if (app.globalData.userInfo.openId != undefined && app.globalData.userInfo.openId != "") {
      // that.inite() 取消获取地址接口
      if (this.data.gooddetailgoodid != undefined) {
        that.inite2()
      }
      else {
        that.inite1()
      }
    }
    else {
      app.getUserInfo(function (e) {
        that.setData({
          openId: e.openId
        })
        // that.inite() 取消获取地址接口
        if (that.data.gooddetailgoodid != undefined) {
          that.inite2()
        }
        else {
          that.inite1()
        }
      })
    }
  },
  checkgooddetail: function (e) {
    var that = this
    if (that.data.index1 == 0) {
      app.globalData.payType = 1 //1是默认微信支付
      var SpecInfo = that.data.gooddetaiSpecInfo
      if (this.data.gooddetailgoodid != undefined) {
        if (SpecInfo == 'undefined') {
          SpecInfo = ''
        } else {
          SpecInfo = that.data.gooddetaiSpecInfo
        }
        // 请求把商品加入购物车
        wx.request({
          url: addr.Address.addGoods,
          data: {
            appid: app.globalData.appid,
            openid: app.globalData.userInfo.openId,
            goodid: this.data.gooddetailgoodid,
            attrSpacStr: this.data.gooddetailattrSpacStr,
            SpecInfo: SpecInfo,
            qty: this.data.gooddetailqty,
            newCartRecord: 1
          },
          method: "POST",
          header: {
            'content-type': 'application/json'
          },
          success: function (res) {
            if (res.data.isok == 1) {
              console.log('~~~~~~~~~~~~~~~~123123~~~~~', res)
              that.data.goodCarIdStr = res.data.cartid
              that.checkgood(e)
            }
            else {
              wx.showModal({
                title: '提示',
                content: '该商品已售罄',
              })
            }
          },
        })
      }
      else {
        this.checkgood(e)
      }
    }
    // 余额支付
    else {
      wx.showModal({
        title: '提示',
        content: '是否确认进行余额支付？',
        success: function (res) {
          if (res.confirm) {
            app.globalData.payType = 2 //2是余额支付
            var SpecInfo = that.data.gooddetaiSpecInfo
            app.globalData.sumprice = that.data.sumprice
            if (that.data.gooddetailgoodid != undefined) {
              if (SpecInfo == 'undefined') {
                SpecInfo = ''
              } else {
                SpecInfo = that.data.gooddetaiSpecInfo
              }
              // 请求把商品加入购物车
              wx.request({
                url: addr.Address.addGoods,
                data: {
                  appid: app.globalData.appid,
                  openid: app.globalData.userInfo.openId,
                  goodid: that.data.gooddetailgoodid,
                  attrSpacStr: that.data.gooddetailattrSpacStr,
                  SpecInfo: SpecInfo,
                  qty: that.data.gooddetailqty,
                  newCartRecord: 1
                },
                method: "POST",
                header: {
                  'content-type': 'application/json'
                },
                success: function (res) {
                  if (res.data.isok == 1) {
                    console.log('~~~~~~~~~~~~~~~~123123~~~~~', res)
                    that.data.goodCarIdStr = res.data.cartid
                    that.checkgood(e)
                  }
                  else {
                    wx.showModal({
                      title: '提示',
                      content: '该商品已售罄',
                    })
                  }
                },
              })
            }
            else {
              this.checkgood(e)
            }
          } else if (res.cancel) {
            console.log('用户点击取消')
          }
        }
      })
    }
  },

  // 结算
  Pay: function () {
    if (this.data.loading == 1) {
      return
    }
    this.data.loading = 1
    var that = this
    var address = {
      userName: that.data.Address.userName,
      postalCode: that.data.Address.postalCode,
      provinceName: that.data.Address.provinceName,
      cityName: that.data.Address.cityName,
      countyName: that.data.Address.countyName,
      detailInfo: that.data.Address.detailInfo,
      telNumber: that.data.Address.telNumber
    }
    address = JSON.stringify(address)
    var order = {
      FreightTemplateId: that.data.chooseModel[that.data.index].Id,
      Message: that.data.Message,
      Remark: '',
    }
    var orderjson = JSON.stringify(order)
    if (app.globalData.payType == 1) {
      var buyModeId = ''
    } else {
      buyModeId = 2
    }
    var param = {
      appid: app.globalData.appid,
      openid: app.globalData.userInfo.openId,
      goodCarIdStr: that.data.goodCarIdStr,
      orderjson: orderjson,
      wxaddressjson: address,
      buyMode: buyModeId
    }
    util.AddOrder(param, function (e) {
      if (e == "failed") {
        wx.hideLoading()
      }
      else {
        wx.navigateTo({
          url: '../paysuccess/paysuccess?totalMoney=' + that.data.sumprice,
        })
      }
      // this.data.loading = 0
      that.data.loading = 0
    }, "")
  },
  // 输入留言说明
  inputMessage: function (e) {
    this.setData({
      Message: e.detail.value
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
    // this.inite()
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
  // 提示请选择地址
  showtoastAddaddress: function () {
    wx.showModal({
      title: '提示',
      content: '亲，您忘记了选择送货地址哦',
      showCancel: false
    })
  },
  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  },
  // 选择支付方式
  changePayway: function (e) {
    console.log('picker发送选择改变，携带值为', e.detail.value)
    this.setData({
      index1: e.detail.value,
    })
  },
  //初始化获取默认地址
  // inite: function (e) {
  //   var that = this
  //   wx.request({
  //     url: addr.Address.GetMyAddress,
  //     data: {
  //       appid: app.globalData.appid,
  //       openid: app.globalData.userInfo.openId,
  //       addressId: 0,
  //       isDefault: 1
  //     },
  //     method: "GET",
  //     header: {
  //       'content-type': "application/json"
  //     },
  //     success: function (res) {
  //       if (res.data.isok == 1) {
  //         that.setData({
  //           Address: res.data.postdata.address
  //         })
  //         console.log(res.data.postdata.address)
  //       }
  //     },
  //     fail: function () {
  //       console.log("获取默认地址出错")
  //       wx.showToast({
  //         title: '获取默认地址出错',
  //       })
  //     }
  //   },
  //   )
  // },
  //初始化获取运费模板
  inite1: function (e) {
    var that = this
    var goodCarIdStr = this.data.goodCarIdStr
    wx.request({
      url: addr.Address.getorderInfo,
      data: {
        appid: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        goodCarIdStr: goodCarIdStr
      },
      method: "GET",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        var array = []
        var chooseModel = []
        if (res.data.isok == 1) {
          for (var i = 0; i < res.data.postdata.length; i++) {
            var model = res.data.postdata[i]
            array.push(model.Name)
          }
          // var sumprice = parseFloat(that.data.totalMoney + (res.data.postdata[that.data.index].sum)).toFixed(2)
          that.setData({
            array: array,
            chooseModel: res.data.postdata,
            sumprice: (parseFloat(that.data.totalMoney) + parseFloat(res.data.postdata[that.data.index].sum)).toFixed(2)
          })
        }
        console.log('!!!!!!!!!!!!!!!!!!!!!!', res)

        // console.log(res.data.postdata)
      },
      fail: function () {
        console.log("获取首页出错")
        wx.showToast({
          title: '获取首页出错',
        })
      }
    },
    )
  },
  inite2: function (e) {
    var that = this
    var goodCarIdStr = this.data.goodCarIdStr
    wx.request({
      url: addr.Address.getOrderGoodsBuyPriceByGoodsIds,
      data: {
        appid: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        goodId: this.data.gooddetailgoodid,
        attrSpacStr: this.data.gooddetailattrSpacStr,
        qty: this.data.gooddetailqty,
      },
      method: "GET",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        var array = []
        var chooseModel = []
        if (res.data.isok == 1) {
          for (var i = 0; i < res.data.postdata.length; i++) {
            var model = res.data.postdata[i]
            array.push(model.Name)
          }
          var sumprice = (parseFloat(that.data.totalMoney) + parseFloat(res.data.postdata[that.data.index].sum)).toFixed(2)
          that.setData({
            array: array,
            chooseModel: res.data.postdata,
            sumprice: sumprice

          })
        }
        console.log('!!!!!!!!!!!!!!!!!!!!!!', res)

        // console.log(res.data.postdata)
      },
      fail: function () {
        console.log("获取首页出错")
        wx.showToast({
          title: '获取首页出错',
        })
      }
    },
    )
  },
  // 储值支付
  inite3: function (e) {
    var that = this
    wx.request({
      url: addr.Address.getBuyModeList,
      data: {
        appid: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        Money: that.data.sumprice
      },
      method: "POST",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        if (res.data.isok == 1) {

        }
      },
      fail: function () {
        console.log("支付失败")
        wx.showToast({
          title: '获取首页出错',
        })
      }
    },
    )
  },
  checkgood: function (e) {
    var goodCarIdStr = this.data.goodCarIdStr
    var that = this
    wx.request({
      url: addr.Address.checkGood,
      data: {
        appid: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        goodCarIdStr: goodCarIdStr
      },
      method: "GET",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        if (res.data.isok == 1) {
          that.Pay(e)
        }
        else {
          wx.showModal({
            title: '提示',
            content: res.data.msg,
          })
          // that.inite() 取消获取地址接口
        }

      },
      fail: function () {
        console.log("")
        wx.showToast({
          title: '提交订单失败',
        })
      }
    })
  },
})