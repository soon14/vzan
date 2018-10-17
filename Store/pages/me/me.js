// pages/me/me.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    AgentConfig: [],//水印开关
    canSaveMoneyFunction: false,//储值开关
    iscloseBtn: 0,//判断领取会员卡按钮开关 0关 1开
    vipcard: [],//卡套集合
    havecard: [],//若为null则显示取卡按钮
    AccountMoneyStr: 0,//储值余额
    PriceSum: 0,//累计消费金额
    model: [],//会员信息
    vipwindow: false,
    user: [],    // 微信头像和微信名字
    // 待付款 待发货 待收货 已完成
    classify: [
      { icon: '../../image/a12.png', txt: '待付款', state: 0 },
      { icon: '../../image/a13.png', txt: '待发货', state: 3 },
      { icon: '../../image/a14.png', txt: '待收货', state: 5 },
      { icon: '../../image/a15.png', txt: '已完成', state: 6 },
    ],
    // 我的订单 收货地址 优惠券 我的奖品 常见问题 联系客服
    entry: ['我的订单', '收货地址', '优惠券', '我的奖品', '常见问题', '联系客服'],

    menu_entry: [
      { title: "我的订单", url: "../myOrder/myOrder?condition=0", id: 0 },
      { title: "我的拼团", url: "../myGroup/myGroup", id: 3 },
      { title: "我的砍价单", url: "../mycutprice/mycutprice", id: 4 },
      //  { title: "常见问题", url: "../myAddress/myAddress" },
      { title: "收货地址", url: "../myAddress/myAddress", id: 1 },
      { title: "联系客服", url: "", id: 2 },
    ]
  },
	navo_webview:function(){
		app.goNewPage('/pages/me/web_view?id=' + this.data.AgentConfig.QrcodeId)
	},
  // 待付款 待发货 待收货 已完成页面跳转
  navTomyOrder: function (e) {
    var index = Number(e.currentTarget.id)  //转格式为数字
    var state = e.currentTarget.dataset.state

    var index = index + 1
    var url = "../myOrder/myOrder?condition=" + index + "&state=" + state

    app.goNewPage(url)

  },
  navtopaymymoney: function () {
    wx.navigateTo({
      url: '../paymymoney/paymymoney',
    })
  },
  // 会员权益弹窗
  showVipmodal: function () {
    this.setData({ vipwindow: !this.data.vipwindow })
  },
  // 我的订单 收货地址 优惠券 我的奖品 常见问题 联系客户
  navTomyAddress: function (e) {
    var index = Number(e.currentTarget.id)  //转格式为数字
    var url = e.currentTarget.dataset.url
    if (index == 2) {
      var phone = app.globalData.customerphone
      if (phone == "") {
        this.GetStoreConfig()
        wx.makePhoneCall({
          phoneNumber: app.globalData.customerphone,
        })
      }
      else {
        wx.makePhoneCall({
          phoneNumber: phone,
        })
      }
    }
    else if (index == 1) {
      wx.chooseAddress({
        success: function (res) {
          console.log('调用地址成功', res)
        }
      })
    }
    else {
      app.goNewPage(url)
    }
  },
  //获取店铺配置
  GetStoreConfig: function () {
    var that = this
    var url = addr.Address.GetStoreConfig
    var param = {
      appid: app.globalData.appid,
    }
    var method = "GET"
    network.requestData(url, param, method, function (e) {
      if (e.data.isok == 1) {
        that.setData({ canSaveMoneyFunction: e.data.postdata.store.funJoinModel.canSaveMoneyFunction })
        app.globalData.canSaveMoneyFunction = e.data.postdata.store.funJoinModel.canSaveMoneyFunction
        var phone = e.data.postdata.store.TelePhone
        app.globalData.customerphone = phone
      }
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    if (app.globalData.userInfo.openId == undefined || app.globalData.userInfo.openId == "") {
      app.getUserInfo(function (e) {
        that.GetStoreConfig()
        that.inite3()
        that.inite4()
        // 获取微信头像和微信名字
        that.setData({
          user: app.globalData.userInfo
        })
      })
    }
    else {
      that.GetStoreConfig()
      util.GetWxCardCode(that) //判断是否已领取过卡
      this.inite2()
      // 获取微信头像和微信名字
      this.setData({
        user: app.globalData.userInfo
      })
    }
  },
  // 水印
  inite2: function (e) {
    var that = this
    wx.request({
      url: addr.Address.GetAgentConfigInfo, //仅为示例，并非真实的接口地址
      data: {
        appid: app.globalData.appid,
      },
      method: "GET",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function (res) {
        if (res.data.isok == 1) {
          if (res.data.AgentConfig.isdefaul == 0) {
            res.data.AgentConfig.LogoText = res.data.AgentConfig.LogoText.split(' ')
          } else {
            res.data.AgentConfig.LogoText = res.data.AgentConfig.LogoText
          }
          that.setData({
            AgentConfig: res.data.AgentConfig,
          })
        }
      },
      fail: function () {
        console.log('获取不了水印')
      }
    })
  },
  // 获取会员信息
  inite3: function (e) {
    var that = this
    wx.request({
      url: addr.Address.GetVipInfo, //仅为示例，并非真实的接口地址
      data: {
        appid: app.globalData.appid,
        uid: app.globalData.userInfo.UserId,
      },
      method: "POST",
      header: {
        'content-type': 'application/x-www-form-urlencoded' // 默认值
      },
      success: function (res) {
        if (res.data.isok) {
          var PriceSum = (parseFloat(res.data.model.PriceSum) / 100).toFixed(2)
          that.setData({
            model: res.data.model,
            PriceSum: PriceSum
          })
        }
      },
      fail: function () {
        console.log('获取不了会员信息')
      }
    })
  },
  // 获取储值余额
  inite4: function (e) {
    var that = this
    wx.request({
      url: addr.Address.getSaveMoneySetUser, //仅为示例，并非真实的接口地址
      data: {
        appid: app.globalData.appid,
        openId: app.globalData.userInfo.openId,
      },
      method: "GET",
      header: {
        'content-type': 'application/x-www-form-urlencoded' // 默认值
      },
      success: function (res) {
        if (res.data.isok == true) {
          that.setData({ AccountMoneyStr: res.data.saveMoneySetUser.AccountMoneyStr })
        }
      },
      fail: function () {
        console.log('获取不了会员信息')
      }
    })
  },
  // 跳转到官网页
  waterMarker: function () {
    wx.navigateTo({
      url: '../watermark/watermark',
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
    this.inite2()
    this.inite3()
    this.inite4()
    this.GetStoreConfig()
    util.UpdateWxCard(this)
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
  // 领取微信会员卡
  getvipCard: function () {
    var that = this
    util.GetCardSign(that)
  },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    this.GetStoreConfig()
    util.GetWxCardCode(this) //判断领取卡卷按钮
    this.inite2()
    this.inite3()
    this.inite4()
    util.UpdateWxCard(this)
    wx.stopPullDownRefresh()
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
  userlogin: function (e) {
    console.log(e);
    var data = e.detail;
    var that = this;
    if (data && data.encryptedData) {
      wx.login({
        success: function (res) {
          getApp().login(res.code, data.encryptedData, data.signature, data.iv, function (callback) {
            console.log(callback);
            that.setData({
              user: callback
            });
            wx.showToast({
              title: '登陆成功',
            })
          });
        }
      })
    }
    else {
      wx.showModal({
        title: '提示',
        content: '请允许获取：用户信息',
        showCancel:false,
        success: function (res) {
          if (res.confirm) {
            wx.openSetting({

            })
          }
        }
      })

    }

  }
})