import Promise from 'es6-promise.min.js';
var addr = require("addr");

var isdebug = true;
var requestParameter = {
  url: "",
  data: {},
  method: 'GET',
  header: {
    "content-type": "application/x-www-form-urlencoded"
  },

}
var http = {
  //异步请求
  post: function (url, data) {
    return new Promise(function (resolve, reject) {
      wx.request(Object.assign({}, requestParameter, {
        url,
        data,
        method: "POST",
        fail: function (e) {
          isdebug && console.log(`请求 ${_url} 失败！\r\n 错误信息：${e.errMsg}`);
          resolve("");
        },
        success: function (e) {
          if (e.statusCode == 200) {
            resolve(e.data);
          }
          else {
            isdebug && console.log(e);
            resolve("");
          }
        }
      }))
    });

  },
  get: function (url, data) {
    return new Promise(function (resolve, reject) {
      wx.request(Object.assign({}, requestParameter, {
        url,
        data,
        fail: function (e) {
          isdebug && console.log(`请求 ${_url} 失败！\r\n 错误信息：${e.errMsg}`);
          resolve("");
        },
        success: function (e) {
          if (e.statusCode == 200) {
            resolve(e.data);
          }
          else {
            isdebug && console.log(e);
            resolve("");
          }
        }
      }))
    });
  },

};

var core = {
  GetVipInfo: function (parameters) {
    return http.post(addr.Address.GetVipInfo, parameters)
  },
  GetStoreInfo: function (parameters) {
    return http.post(addr.Address.GetStoreInfo, parameters)
  },
  GetTechnicianInfo: function (parameters) {
    return http.post(addr.Address.GetTechnicianInfo, parameters)
  },
  GetContactList: function (parameters) {
    return http.post(addr.Address.GetContactList, parameters);
  },
  AddContact: function (parameters) {
    return http.post(addr.Address.AddContact, parameters)
  },
  GetHistory: function (parameters) {
    return http.post(addr.Address.GetHistory, parameters);
  },
  upload: function (filetype = "img") {
    return new Promise(function (resolve, reject) {
      if (filetype == "img") {
        wx.chooseImage({
          success: function (res) {
            var uploadCount = 0;
            var uploadImgs = [];
            var tempFilePaths = res.tempFilePaths;
            function uploadOne() {
              wx.showLoading({
                title: '上传中...',
              })
              const uploadTask = wx.uploadFile({
                url: addr.Address.Upload,
                filePath: tempFilePaths[uploadCount],
                name: 'file',
                formData: {
                  filetype: filetype,
                },
                success: function (res) {
                  console.log(res);
                  var data = JSON.parse(res.data);
                  if (data.result) {
                    //resolve(data.msg);
                    uploadCount += 1;
                    console.log("上传成功", data.msg);
                    uploadImgs.push(data.msg);
                  }
                  else {
                    console.log("上传失败", data);
                    resolve("");
                  }
                  if (uploadCount < tempFilePaths.length) {
                    uploadOne();
                  }
                  else {
                    console.log("上传完毕", uploadImgs);
                    resolve(uploadImgs);
                  }
                },
                complete: function () {
                  wx.hideLoading();
                }
              })
              uploadTask.onProgressUpdate((res) => {
                wx.showLoading({
                  title: '上传中' + res.progress + "%",
                })
              })
            }
            uploadOne();
          }
        })
      }
    });
  },
  changeunreadmsg: function (unreadmsg, unreadmsgcount) {
    var app = getApp();
    app.globalData.unreadmsg = unreadmsg
    app.globalData.unreadmsgcount = unreadmsgcount;

    wx.setStorage({
      key: "unreadmsg",
      data: unreadmsg,
    })
    wx.setStorage({
      key: "unreadmsgcount",
      data: unreadmsgcount,
    })
  },
  Getaid: function (appid) {
    let aid = wx.getStorageSync("aid");
    if (aid) {
      return Promise.resolve(aid);
    }
    return new Promise(function (resolve, reject) {
      http.post(addr.Address.Getaid, { appid }).then(function (data) {
        if (data && data.isok) {
          wx.setStorageSync("aid", data.msg)
          resolve(data.msg)
        }
        else {
          resolve("")
        }
      });
    });

  },

  GetPageSetting: function () {
    let app = getApp();
    let that=this;
    var PageSetting = wx.getStorageSync("PageSetting");
    if (PageSetting) {
      return Promise.resolve(PageSetting);
    }
    return new Promise(function (resolve, reject) {
      that.Getaid(app.globalData.appid).then(function (aid) {
        http.post(addr.Address.GetPageSetting, { aid}).then(function(data){
          if (data.isok) {
            wx.setStorageSync("PageSetting", data)
            resolve(data);
          }
          else{
            resolve("");
          }
          
        });
      });
    });

  },
};

module.exports = {
  http,
  core,
}