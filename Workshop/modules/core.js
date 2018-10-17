require("polyfill.js")
//地址
const HOST = "https://wtapi.vzan.com/"
//const HOST = "http://api2.vzan.com/";
const DEFAULT_SHARE_IMG = "/img/share.jpg";


var savePost = false;

var addr = {
  loginByThirdPlatform: HOST + "apiWorkShop/CheckUserLoginNoappsr",
  GetAppConfig: HOST + "apiWorkShop/GetAppConfig",
  GetUserPage: HOST + "apiWorkShop/GetUserPage",//获取用户所有自定义页面
  GetUserPageInfo: HOST + "apiWorkShop/GetUserPageInfo",//获取单个页面的数据
  Upload: HOST + "apiWorkShop/Upload",
  SavePage: HOST + "apiWorkShop/SavePage",
  DelPage: HOST + "apiWorkShop/DelPage",
  GetShareQRCode: HOST + "apiWorkShop/GetShareQRCode",
  SendPhoneAuth: HOST + "apiWorkShop/senduserauth",
  Submitauth: HOST + "apiWorkShop/Submitauth",
  SaveFormData: HOST + "apiWorkShop/SaveFormData",
  GetUserForm: HOST + "apiWorkShop/GetUserForm",
  SaveFeedback: HOST + "/apiMiappEnterprise/SaveFeedback",
}

//请求
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
            console.log(res.data);
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

  }
}

//工具
var tools = {
  alert: function (content, title, success, cancel, showCancel = true) {
    wx.showModal({
      title: title || "提示",
      content: content || "",
      showCancel: showCancel,
      success: function (res) {
        if (res.confirm) {
          success && success();
        } else if (res.cancel) {
          cancel && cancel();
        }
      }
    })
  },
  //单张上传
  upload: function (filetype = "img") {
    return new Promise(function (resolve, reject) {
      if (filetype == "img") {
        wx.chooseImage({
          count: 1,
          success: function (res) {
            var tempFilePaths = res.tempFilePaths;
            wx.showLoading({
              title: '上传中...',
            })
            const uploadTask = wx.uploadFile({
              url: addr.Upload,
              filePath: tempFilePaths[0],
              name: 'file',
              formData: {
                filetype: filetype,
              },
              success: function (res) {
                console.log(res);
                var data = JSON.parse(res.data);
                if (data.result) {
                  resolve(data.msg);
                }
                else {
                  reject(data.msg);
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
        })
      }
      else if (filetype == "video") {
        wx.chooseVideo({
          success: function (res) {
            var tempFilePaths = res.tempFilePath;
            wx.showLoading({
              title: '上传中...',
            })
            const uploadTask = wx.uploadFile({
              url: addr.Upload,
              filePath: tempFilePaths,
              name: 'file',
              formData: {
                filetype: filetype,
              },
              success: function (res) {
                console.log(data);
                var data = JSON.parse(res.data);
                if (data.result) {
                  resolve(data.msg);
                }
                else {
                  reject(data.msg);
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
        })
      }
    });
  },
  uploadVideo: function () {
    return new Promise(function (resolve, reject) {
      wx.chooseVideo({
        success: function (res) {
          var tempFilePaths = res.tempFilePaths;
          wx.showLoading({
            title: '上传中...',
          })
          const uploadTask = wx.uploadFile({
            url: addr.Upload,
            filePath: tempFilePaths[0],
            name: 'file',
            success: function (res) {
              console.log(data);
              var data = JSON.parse(res.data);
              if (data.result) {
                resolve(data.msg);
              }
              else {
                reject(data.msg);
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
      })
    });
  },
  savePage: function (that, callback) {
    if (savePost) {
      wx.showToast({
        title: '提交中，请勿重复提交。',
      })
      return;
    }

    getApp().getUserInfo().then(function (user) {
      var pageModel = JSON.parse(JSON.stringify(that.data.vm));
      if (pageModel.content.pageTitle == "") {
        tools.alert("请填写页面标题");
        return;
      }
      var coms = pageModel.content.coms;
      for (var i = 0; i < coms.length; i++) {
        if (coms[i].type == 'tel' && !/^1[\d]{10}$/g.test(coms[i].content)) {
          tools.alert("请填写正确的手机号码");
          return;
        }
        else if (coms[i].type == 'form') {
          for (var j = 0; j < coms[i].items.length; j++) {
            if (coms[i].items[j].type == "radio") {
              if (coms[i].items[j].items.length == 0) {
                tools.alert("请为单选项 设置 选项");
                return;
              }
              else {
                coms[i].items[j].items.forEach(function (radioItem) {
                  radioItem.sel = false;
                });
              }
              coms[i].items[j].value = "";
            }
            else {
              coms[i].items[j].value = "";
            }
          }
        }
        else if (coms[i].type == 'video') {
          coms[i].autoplay = false;
        }
      }
      pageModel.uid = user.uid;
      pageModel.content = JSON.stringify(pageModel.content);

      if (!savePost)
        savePost = true;

      http.postAsync(addr.SavePage, pageModel).then(function (data) {
        if (!data.result) {
          tools.alert(data.msg);

        }
        if (callback) {
          callback(data.obj);
        }
        savePost = false;
      });
    });

  },
  backHome: function (callback) {
    var pages = getCurrentPages();
    if (pages.find(p => { return p.route === "pages/index/index" })) {
      wx.navigateBack({
        delta: pages.length + 1,
        success: function () {
          if (callback) {
            callback();
          }
        }
      })
    }
    else {
      wx.redirectTo({
        url: '/pages/index/index',
        success: function () {
          if (callback) {
            callback();
          }
        }
      })
    }
  },
  getPageQRCode: function (pageid) {
    return http.postAsync(addr.GetShareQRCode, { id: pageid });
  },
  getFormPhone: function (content) {
    if (typeof content != "object") {
      return "";
    }
    for (var key in content) {
      if (/1[\d]{10}/.test(content[key])) {
        return content[key]
      }
    }
    return "";
  },
  makePhoneCall: function (phone) {
    if (phone == "")
      return;
    wx.makePhoneCall({
      phoneNumber: phone,
    })
  },
  share: function (shareModel, callback) {
    if (shareModel.title == "") {
      shareModel.title = "小未工坊";
      shareModel.path = "/pages/index/index"
      shareModel.imgurl = "https://j.vzan.cc/miniapp/img/workshop/share.jpg";
    }
    return {
      title: shareModel.title,
      path: shareModel.path,
      imageUrl: shareModel.imgurl,
      success: function (res) {
        if (callback) {
          callback();
        }
      },
      fail: function (res) {

      }
    }
  },
  //获取组件里第一张图片，没有返回默认图片
  getPageImg: function (coms) {
    var result = DEFAULT_SHARE_IMG;
    if (!coms)
      return result;
    for (var i = 0; i < coms.length; i++) {
      var cur_com = coms[i];
      if (cur_com.type == 'slider') {
        var itemWithImg = cur_com.items.find(p => p.src != "");
        if (itemWithImg) {
          return itemWithImg.src;
        }
      }
      else if (cur_com.type == 'img' && cur_com.src != "") {
        return cur_com.src;
      }
    }
    return result;

  },
  getPageDes: function (coms) {
    var result = "";
    if (!coms)
      return result;
    for (var i = 0; i < coms.length; i++) {
      var cur_com = coms[i];
      if (cur_com.type == 'txt' && cur_com.content != "") {
        return cur_com.content;
      }
    }
    return result;
  },
  createPageQRCode: function (bindId, parameter) {
    wx.getSystemInfo({
      success: function (res) {
        var pxtorpx = 750 / res.screenWidth;
        var rpxtopx = res.screenWidth / 750;

        const ctx = wx.createCanvasContext('shareCanvas')
        ctx.drawImage("/img/sharebg.png", 0, 0, 670 * rpxtopx, 820 * rpxtopx)

        tools.getPageQRCode(parameter).then(function (data) {
          console.log(data);
          if (data.result) {
            wx.downloadFile({
              //url: 'https://i.vzan.cc/image/jpg/2017/11/20/214411697bf8fa549046419a645c768019b5c3.jpg?x-oss-process=image/resize,limit_0,m_fill,w_315,h_315/format,gif/quality,q_80',
              url: data.msg.replace("http://","https://"),
              success: function (res) {
                if (res.statusCode === 200) {
                  ctx.drawImage(res.tempFilePath, 245 * rpxtopx, 395 * rpxtopx, 200 * rpxtopx, 200 * rpxtopx)
                  ctx.draw()
                }
              }
            })

          }
          else {
            tools.alert(data.msg);
          }

        });
      }
    })
  },
  delPage: function (pageid) {
   return http.postAsync(addr.DelPage,{id:pageid})
  }
}

//model
var listvm = {
  pageSize: 10,
  pageIndex: 1,
  count: 0,
  list: [],
  state: 1,
  loadAll: false,
  isPost: false,
  filterForm: 0,
  beginTime: "",
  endTime: ""
}

var formMutiSelectItem = {
  name: "",
  sel: false,
  placeholder: "请输入选择项"
};

var formItem = {
  "text": {
    type: "text",
    placeholder: "点击以输入文本",
    name: "",
    value: "",
  },
  "number": {
    type: "number",
    name: "",
    value: "",
  },
  "radio": {
    type: 'radio',
    items: [],
    name: "",
    value: "",
  },
  "checkbox": {
    type: 'checkbox',
    items: [],
    name: "",
    value: "",
  },
  "time": {
    type: "time",
    name: "",
    value: "",
  },
  "date": {
    type: "date",
    name: "",
    value: "",
  },
  "sex": {
    type: 'radio',
    items: ["男", "女"],
    name: "性别",
    value: "",
  }
}

var coms = {
  slider: {
    type: "slider",
    name: "轮播图",
    items: [{ src: "" }, { src: "" }, { src: "" }],
  },
  img: {
    type: "img",
    name: "图片",
    src: "",
  },
  txt: {
    type: "txt",
    name: "文本",
    content: "",
  },
  tel: {
    type: "tel",
    name: "电话",
    content: "",
  },
  video: {
    type: "video",
    name: "视频",
    poster: "",
    src: ""
  },
  form: {
    type: "form",
    name: "表单",
    title: "",
    items: []
  }
}
var pageData = {
  id: 0,
  uid: 0,
  state: 0,
  content: {
    pageTitle: "",
    coms: [
      Object.assign({}, coms.slider)
    ]
  }
}

var shareModel = {
  title: "",
  path: "",
  imgurl: "",
};

module.exports = {
  addr,
  http,
  tools,
  listvm,
  pageData,
  formItem,
  coms,
  formMutiSelectItem,
  shareModel,
}