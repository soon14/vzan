//app.js
var util = require("./utils/util.js");
var api = require("./utils/network.js");
var addr = require("./utils/addr.js");
var imgresouces = require("./utils/imgresouces.js");
var C_Enum = require("./public/C_Enum.js");
let { WeToast } = require('src/wetoast.js')    // 返回构造函数，变量名可自定义
App({
  onLaunch: function () {

    //调用API从本地缓存中获取数据
    var that = this;

    //第三方平台配置
    var exconfig = wx.getExtConfigSync()
    if (exconfig != undefined) {
      that.globalData.appid = exconfig.appid
      that.globalData.areaCode = exconfig.areaCode
    }
    var logs = wx.getStorageSync('logs') || []
    logs.unshift(Date.now())
    wx.setStorageSync('logs', logs)
    this.getUserInfo({
      success: function (res) {
        that.globalData.user = res
      }
    })
  },

  globalData: {
    isclearItem5:0,
    ispayOk: 0,
    dizhiId: '',//地址id
    TablesNo: -999,//桌台号(外卖==-999)
    orderid: 0,//订单id
    TelePhone: 0,//商家联系号码
    addressInfo: '请选择定位信息',//定位详细位置
    weidu: 0,//纬度
    jingdu: 0,//经度
    ShippingFeeStr: 0,//配送费
    OutSideStr: 0,//起送价
    FoodsName: "",//商店名字
    logoimg: "",//商店头像
    TheShop: 99,//堂食判断
    TakeOut: 99,//外卖判断
    dbOrder: 0,// 订单id，paysuccess页面用
    // typeid分类
    id: 0,
    typeid: 0,
    change: 1,
    user: [],
    //userInfo: {unionId:"oW2wBwXbX3lXBHRqQubA8sq-Gkcc"},
    userInfo: { IsValidTelePhone: 0, TelePhone: "未绑定" },
    isLogin: false,
    citysubid: 0,
    // 微信配置信息
    session_key: '',
    openid: '',
    appsr: "",
    //旧
    //  appid: "wx95a525ef2e44492f",
    //  appsr: "fbdb9ae499d15101ef0cc92f1f2bf4fe",
    // areaCode: 110228,//密云县测试
    // areaCode: 110107,//石景山正式
    //同城优惠券
    // appid: "wx238f1fdb91e27c56",
    // appsr: "3e0ad0785a863744da222055955a381d",
    // areaCode: 110228,//密云县测试
    // areaCode: 110107,//石景山正式
    //模板开发
    // appid: "wx95a525ef2e44492f",
    // appsr: "95db16838a6520bbb4fd0fb280b3186d",
    // areaCode: 110228,
    //模板开发Q逗
    appid: "wx9cb1d8be83da075b",
    areaCode: 110228,
    //同城分类信息
    // appid: "wxbfa9d9b358118fa4",
    // appsr: "6c7127c583d86aa387285e11b5780273",
    // areaCode: 110228,//密云县测试
    // areaCode: 110107,//石景山正式

    // 配置
    //Host: 'https://cityapi.vzan.com/',
    Host: 'https://txiaowei.vzan.com/',
    isDebug: false,
    pageSize: 10, //

    // content:name,     
    // 微赞平台信息
    uid: '',
    platformUserInfo: '',
    //店铺客服电话
    customerphone: "",
    cityname: '',
    // 同城信息
    //用户的名字和用户的图像
  },
  WeToast, // 后面可以通过app.WeToast访问
  imgresouces,
  C_Enum,

  getUserInfo: function (cb) {
    console.log('getUserInfo执行')
    var that = this
    if (that.globalData.userInfo && that.globalData.userInfo.unionId) {
      console.log('getUserInfo执行回调')
      typeof cb == "function" && cb(that.globalData.userInfo)
    } else {
      //调用登录接口
      wx.login({
        success: function (res) {
          console.log('login res.code ' + res.code)
          wx.getUserInfo({
            withCredentials: true,
            success: function (data) {
              console.log('getUserInfo登录成功')
              that.login(res.code, data.encryptedData, data.signature, data.iv, cb);
            },
            fail: function () {
              // fail
              wx.showModal({
                title: '提示',
                showCancel: false,
                confirmText: '知道啦',
                content: '授权失败啦!\n请退出后在微信小程序中移除[微赞同城信息],再重扫二维码或直接搜索[微赞同城信息]选择允许!',
                success: function (res) {
                  if (res.confirm) {
                    console.log('用户点击确定')
                  }
                }
              })
            },
            complete: function () {
              console.log('getUserInfo执行complete')
            }
          })
        },
      })
    }
  },
  //登录
  login: function (code, encryptedData, signature, iv, cb) {
    wx.showToast({
      title: '加载中',
      icon: 'loading',
      duration: 10000
    })
    // console.log(code)
    // console.log(encryptedData)
    // console.log(signature)
    // console.log(iv)
    var that = this;
    // var currentData = this.data
    console.log('准备调用loginByThirdPlatform接口')
    wx.request({
      url: addr.Address.loginByThirdPlatform,
      data: {
        code: code,
        data: encryptedData,
        signature: signature,
        iv: iv,
        appid: that.globalData.appid,
      },
      method: "Get",
      success: function (data) {
        if (data.data.result) {
          var json = data.data.obj
          console.log('loginByThirdPlatform接口返回 unionId - ' + json.unionId)
          that.globalData.userInfo = {
            avatarUrl: json.avatarUrl,
            city: json.city,
            country: json.country,
            gender: json.gender,
            language: json.language,
            nickName: json.nickName,
            // nickName:'我修改的名字',
            openId: json.openId,
            province: json.province,
            sessionId: json.sessionId,
            unionId: json.unionId,
            TelePhone: (json.tel == null || json.tel == '' ? "未绑定" : json.tel),

            IsValidTelePhone: json.IsValidTelePhone,
            //unioId: 'oW2wBwTJhtXdeOgB7S9nuvEJoDls',
            //unionId: 'oW2wBwWwbBttylGPQdRUW46yc3UY',
            // unionId: "oW2wBwUp5QndTGk3yCs1iYfQXYsQ",
          };
          console.log('json = ' + json);
          wx.hideToast()
          // currentData.header.title = that.globalData.userInfo.nickName
          // currentData.header.title = '我是名字'                
          // currentData.header.image = that.globalData.userInfo.avatarUrl
          // that.setData(currentData)
          // that.storeListRequest(that.globalData.userInfo.unionId, 1)

          typeof cb == "function" && cb(that.globalData.userInfo)
        } else {
          console.log('登录失败 data - ' + data.data)
          wx.showModal({
            title: '提示',
            content: data.data.msg,
          })
          wx.hideToast()
          // currentData.haveMoreData = false
          // that.setData(currentData)
        }
      },
      fail: function (data) {
        console.log(data)
      },
      complete: function (data) {
        console.log(data)
      }
    })
  },
  getUserInforef: function (cb) {
    wx.getUserInfo({
      withCredentials: true,
      success: function (data) {
        if (data.encryptedData == undefined || data.encryptedData == '') {
          this.getUserInforef(cb)
        }
        else {
          this.login(res.code, data.encryptedData, data.signature, data.iv, cb)
        }
      },
      fail: function () {
        // fail
        wx.showModal({
          title: '提示',
          showCancel: false,
          confirmText: '知道啦',
          content: '授权失败啦!\n请退出后在微信小程序中移除,再重扫二维码或直接搜索选择允许!',
          success: function (res) {
            if (res.confirm) {
              console.log('用户点击确定')
            }
          }
        })
      },
      complete: function () {
        console.log('getUserInfo执行complete')
        // complete
      }
    })
  },
  pictureTap: function (e) {
    var src = e.currentTarget.dataset.src;
    wx.previewImage({
      urls: [src]
    })
  },
  pictureTaps: function (url, urls) {
    wx.previewImage({
      current: url,
      urls: urls,
    })
  },
  ShowMsg: function (msg) {
    wx.showModal({
      title: '提示',
      content: msg,
      showCancel: false,
    })
  },
  ShowMsgAndUrl: function (msg, url, mtype = 0) {
    var that = this
    wx.showModal({
      title: '提示',
      showCancel: false,
      content: msg,
      success: function (res) {
        if (res.confirm) {
          if (mtype == 0) {
            that.goNewPage(url)
          } else if (mtype == 1) {
            that.goBackPage(1)
          }
        }
      }
    })
  },
  ShowConfirm: function (msg, callfunction) {
    var that = this
    wx.showModal({
      title: '提示',
      content: msg,
      success: function (e) {
        if (e.confirm) {
          callfunction(e)
        }
      }
    })
  },
  showToast: function (msg) {
    wx.showToast({
      title: msg,
    })
  },
  showLoading: function (msg) {
    wx.showLoading({
      title: msg,
    })
  },
  //刷新页面
  reloadpage: function (param = '', index = 0) {
    var pages = getCurrentPages()
    var coupon = pages[index]
    coupon.onLoad(param)
  },
  //刷新页面
  reloadpagebyurl: function (param = '', url = '') {
    var pages = getCurrentPages()
    if (pages.length > 0) {
      for (var i in pages) {
        var page = pages[i]
        if (page.route == url) {
          page.onLoad(param)
          break
        }
      }
    }
  },
  //手机号码是否通过验证
  checkphone: function (param = '') {
    if (this.globalData.userInfo.IsValidTelePhone == 0) {
      if (param != '1') {
        wx: wx.showModal({
          title: '提示',
          content: '为了保障您数据的安全，请先进行手机号验证',
          success: function (res) {
            if (res.confirm) {
              wx.navigateTo({
                url: '../bind_mobile/bind_mobile',
              })
            }
          },
        })
      }
    }
    else {
      return true
    }
    return false
  },
  //跳转新页面
  goNewPage: function (url) {
    wx.navigateTo({
      url: url,
    })
  },
  //跳转选项卡页
  goBarPage: function (url) {
    wx.switchTab({
      url: url,
    })
  },
  //返回上几页
  goBackPage: function (delta) {
    wx.navigateBack({
      delta: delta
    })
  },
  onHide: function () {
    // getApp().globalData.addressInfo = '请选择定位信息'
    // console.log('我是app.js的onHide')
  },
})