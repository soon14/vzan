var addr = require('../utils/addr.js');
var template = require('../script/template.js');
var app = getApp();

module.exports = {
  gRequest: function(_url, _data, callback, that) {
    wx.showNavigationBarLoading();
    var g = getApp().globalData;
    _data.utoken = g.utoken;
    wx.request({
      url: _url,
      data: _data || {},
      method: 'GET',
      header: {
        "content-type": "application/x-www-form-urlencoded"
      },
      success: function(res) {
        wx.hideNavigationBarLoading();
        if (res.data.code == 1) {
          callback(res);
        } else if (res.data.code == 2) { //如果登录过期
          g.utoken = "";
          app.login(function() {
            re_gRequest(_url, _data, function(res) {
              callback(res)
            })
          })
        } else {
          if (res.data.msg == '不是代理') {
            return
          }
          wx.showModal({
            title: '提示',
            content: res.data.MessageDetail || res.data.msg,
            showCancel: false,
            confirmText: (res.data.msg == '该商品未通过审核，去看看其他商品吧' ? '去首页' : '确定'),
            success: function(res0) {
              if (res0.confirm && (res.data.msg == '门店不可用' || res.data.msg == '该商品未通过审核，去看看其他商品吧')) {
                wx.switchTab({
                  url: '/pages/index/index',
                })
              }
            }
          })
        }
        console.log(_url, _data, res)
      },
    })

    function re_gRequest(_url, _data, callback) {
      wx.request({
        url: _url,
        data: _data || {},
        method: 'GET',
        header: {
          "content-type": "application/x-www-form-urlencoded"
        },
        success: function(res) {
          if (res.data.code == 1) {
            callback(res);
          }
        }
      })
    }

  },

  pRequest: function(_url, _data, callback) {
    wx.showNavigationBarLoading();
    var g = getApp().globalData;
    var utoken = g.utoken;
    wx.request({
      url: _url + '?utoken=' + utoken,
      data: _data || {},
      method: 'POST',
      header: {
        "content-type": "application/json"
      },
      success: function(res) {
        wx.hideNavigationBarLoading();
        if (res.data.code == 1) {
          callback(res);
        } else if (res.data.code == 2) { //如果登录过期

          g.utoken = "";
          app.login(function() {
            re_pRequest(_url, _data, function(res) {
              callback(res)
            })
          })
        } else {
          wx.showModal({
            title: '提示',
            content: res.data.MessageDetail || res.data.msg,
            showCancel: false
          })
        }
        console.log(_url, _data, res)
      }
    })

    function re_pRequest(_url, _data, callback) {
      wx.request({
        url: _url,
        data: _data || {},
        method: 'POST',
        header: {
          "content-type": "application/json"
        },
        success: function(res) {
          if (res.data.code == 1) {
            callback(res);
          }
        }
      })
    }
  },


  GoodList: function(that, data, isreachBottom) { //isreachBottom 0刷新 1加载更多
    wx.showNavigationBarLoading();
    data.utoken = getApp().globalData.utoken
    data.aId = getApp().globalData.aid
    wx.request({
      url: addr.GoodList,
      data: data,
      method: "GET",
      header: {
        "content-type": "application/x-www-form-urlencoded"
      },
      success: function(res) {
        if (res.data.code == 1) {
          var _GL = []
          wx.hideNavigationBarLoading();
          if (isreachBottom == 0) {
            _GL.push(res.data.obj)
            that.setData({
              _GL: _GL
            })
            that.data.good_vm.pageIndex = ++data.pageIndex
          } else {
            if (res.data.obj.length != 0) {
              that.data._GL.push(res.data.obj)
              var length = that.data._GL.length
              var _GL = '_GL' + '[' + Number(length - 1) + ']'
              that.setData({
                [_GL]: that.data._GL[Number(length - 1)],
              })
              that.data.good_vm.pageIndex = ++data.pageIndex

            }
            that.setData({
              btnLoading: false,
              isall: (res.data.obj.length == 0 ? true : false),
            })
          }
          that.setData({
            scroll_into: getApp().globalData.scroll_into
          })
        }
      },
      fail: function() {
        console.log("查询商品列表出错")
      }
    })
  },



  // 发起支付
  PayOrderNew: function(orderid, cb) {
    var that = this;
    var g = getApp().globalData;
    wx.request({
      url: addr.PayOrderNew,
      data: {
        openId: g.userInfo.openId,
        orderid: orderid,
        'type': 1,
      },
      method: 'POST',
      header: {
        'content-type': 'application/x-www-form-urlencoded',
      },
      success: function(res) {
        if (res.data.result) {
          that.wxpay(res.data.obj, function(callback) {
            cb(callback)
          })
        } else {
          wx.showModal({
            title: '支付失败',
            content: res.data.msg,
            showCancel: false
          })
        }
      }
    })
  },
  wxpay: function(param, callback) {
    var that = this
    param = JSON.parse(param)
    wx.requestPayment({
      appId: param.appId,
      timeStamp: param.timeStamp,
      nonceStr: param.nonceStr,
      package: param.package,
      signType: param.signType,
      paySign: param.paySign,
      success: function(res) {
        callback('1')
      },
      fail: function(res) {
        callback('-1')
      },
      complete: function(res) {
        console.log('pay complete', res)
        if (res.errMsg == "requestPayment:fail cancel") {
          callback('-1')
          // template.gonewpage('/pages/shopping/orderInfo/orderInfo') //如果取消支付则不进行任何操作
        }
      }
    })
  },

}