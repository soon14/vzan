

var http = {
  get: function (_url, _data, callback) {
    console.log("请求：" + _url);
    wx.request({
      url: _url,
      data: _data || {},
      method: 'GET', // OPTIONS, GET, HEAD, POST, PUT, DELETE, TRACE, CONNECT
      header: {
        "content-type": "application/x-www-form-urlencoded"
      }, // 设置请求的 header
      success: function (res) {
        // success
        callback(res);
      }
    })
  },
  post: function (_url, _data, callback) {
    console.log("请求：" + _url);
    wx.showNavigationBarLoading();
    wx.request({
      url: _url,
      data: _data || {},
      method: 'POST', // OPTIONS, GET, HEAD, POST, PUT, DELETE, TRACE, CONNECT
      header: {
        "content-type": "application/x-www-form-urlencoded"
      }, // 设置请求的 header
      success: function (res) {
        wx.hideNavigationBarLoading();
        // success
        callback(res);
      }
    })
  },
  //异步请求
  postAsync: function (_url, _data) {
    console.log("请求：" + _url);
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
          console.log(e);
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
    console.log("请求：" + _url);
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
            resolve(res.data);
          }
          else
            reject(res);
        },
        fail: function (e) {
          console.log(e);
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
          let _str = `错误信息：网络连接超时`;
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