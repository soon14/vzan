//index.js
//获取应用实例
const app = getApp()
const HOST = "http://localhost:8096/"; //正式
const req = {
  json: {
    url: "",
    data: {},
    method: 'GET',
    header: {
      'content-type': "application/json"
    },
  },
  urlencoded: {
    url: "",
    data: {},
    method: 'GET',
    header: {
      "content-type": "application/x-www-form-urlencoded"
    },
  }
};
const addr = {
  wxLogin: HOST + 'openid',
};
const http = {
  //异步请求
  postJson: function(url, data) {
    return new Promise(function(resolve, reject) {
      wx.request(Object.assign({}, req.json, {
        url,
        data,
        method: "POST",
        fail: function(e) {
          // isdebug && console.log(`请求 ${_url} 失败！\r\n 错误信息：${e.errMsg}`);
          resolve("");
        },
        success: function(e) {
          if (e.statusCode == 200) {
            resolve(e.data);
          } else {
            // isdebug && console.log(e);
            resolve("");
          }
        }
      }))
    });
  },
  //异步请求
  post: function(url, data) {
    return new Promise(function(resolve, reject) {
      wx.request(Object.assign({}, req.urlencoded, {
        url,
        data,
        method: "POST",
        fail: function(e) {
          // isdebug && console.log(`请求 ${_url} 失败！\r\n 错误信息：${e.errMsg}`);
          resolve("");
        },
        success: function(e) {
          if (e.statusCode == 200) {
            resolve(e.data);
          } else {
            // isdebug && console.log(e);
            resolve("");
          }
        }
      }))
    });
  },
  get: function(url, data) {
    return new Promise(function(resolve, reject) {
      wx.request(Object.assign({}, req.urlencoded, {
        url,
        data,
        fail: function(e) {
          // isdebug && console.log(`请求 ${_url} 失败！\r\n 错误信息：${e.errMsg}`);
          resolve("");
        },
        success: function(e) {
          if (e.statusCode == 200) {
            resolve(e.data);
          } else {
            // isdebug && console.log(e);
            resolve("");
          }
        }
      }))
    });
  },

};
Page({
  data: {
    motto: 'Hello World',
    userInfo: {},
    hasUserInfo: false,
    canIUse: wx.canIUse('button.open-type.getUserInfo')
  },
  //事件处理函数
  bindViewTap: function() {
    wx.navigateTo({
      url: '../logs/logs'
    })
  },
  onLoad: function() {
    if (app.globalData.userInfo) {
      this.setData({
        userInfo: app.globalData.userInfo,
        hasUserInfo: true
      })
    } else if (this.data.canIUse) {
      // 由于 getUserInfo 是网络请求，可能会在 Page.onLoad 之后才返回
      // 所以此处加入 callback 以防止这种情况
      app.userInfoReadyCallback = res => {
        this.setData({
          userInfo: res.userInfo,
          hasUserInfo: true
        })
      }
    } else {
      // 在没有 open-type=getUserInfo 版本的兼容处理
      wx.getUserInfo({
        success: res => {
          app.globalData.userInfo = res.userInfo
          this.setData({
            userInfo: res.userInfo,
            hasUserInfo: true
          })
        }
      })
    }
  },
  getUserInfo: function(e) {
    wx.login({
      success: function(res) {
        if (res.code) {
          http.post(addr.wxLogin, {
            code: res.code
          }).then(data => {
            console.log(data)
          })

        } else {
          console.log('登录失败！' + res.errMsg)
        }
      }
    })



    console.log(e)
    app.globalData.userInfo = e.detail.userInfo
    this.setData({
      userInfo: e.detail.userInfo,
      hasUserInfo: true
    })
  }
})