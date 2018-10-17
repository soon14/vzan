var addr = require("addr.js");
var app = getApp();
var http = {
  //异步请求
  postAsync: function (_url, _data) {
    // console.log("请求：" + _url);
    return new Promise(function (resolve, reject) {
      wx.request({
        url: _url,
        data: _data || {},
        method: 'POST',
        header: {
          "content-type": "application/x-www-form-urlencoded"
        }, // 设置请求的 header
        success: function (res) {
          // success
          if (res.statusCode == 200) {

            resolve(res.data);
          }
          else
            reject(res);
        },
        fail: function (e) {
          // console.log(e);
          let _str = `请求 ${_url} 失败！
          错误信息：${e.errMsg}`;
          wx.showModal({
            title: '提示',
            content: _str,
            showCancel: false,
            success: function (res) {

            }
          })
          reject("");
        }
      })
    });

  },
  getAsync: function (_url, _data) {
    // console.log("请求：" + _url);
    return new Promise(function (resolve, reject) {
      wx.request({
        url: _url,
        data: _data || {},
        method: 'GET',
        header: {
          'content-type': 'application/json'
        },
        success: function (res) {
          // success
          if (res.statusCode == 200) {
            // console.log(res.data);
            resolve(res.data);
          }
          else
            reject(res);
        },
        fail: function (e) {
          // console.log(e);
          let _str = `请求 ${_url} 失败！
          错误信息：${e.errMsg}`;
          wx.showModal({
            title: '提示',
            content: _str,
            showCancel: false,
            success: function (res) {

            }
          })
          reject("");
        }
      })
    });

  },
  // JSON请求
  postJsonAsync: function (_url, _data) {
    // console.log("请求：" + _url);
    return new Promise(function (resolve, reject) {
      wx.request({
        url: _url,
        data: _data || {},
        method: 'POST',
        header: {
          'content-type': "application/json"
        }, // 设置请求的 header
        success: function (res) {
          // success
          if (res.statusCode == 200) {

            resolve(res.data);
          }
          else
            reject(res);
        },
        fail: function (e) {
          // console.log(e);
          let _str = `请求 ${_url} 失败！
          错误信息：${e.errMsg}`;
          wx.showModal({
            title: '提示',
            content: _str,
            showCancel: false,
            success: function (res) {

            }
          })
          reject("");
        }
      })
    });
  },
}
module.exports = http;