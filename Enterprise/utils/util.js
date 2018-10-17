import Promise from './es6-promise.min.js';
// var crypt = require("./crypt.js");
var addr = require("addr");
Date.prototype.Format = function (fmt) { //author: meizz 
  var o = {
    "M+": this.getMonth() + 1,                 //月份 
    "d+": this.getDate(),                    //日 
    "H+": this.getHours(),                   //小时 
    "m+": this.getMinutes(),                 //分 
    "s+": this.getSeconds(),                 //秒 
    "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
    "S": this.getMilliseconds()             //毫秒 
  };

  if (/(y+)/.test(fmt)) {
    fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
  }
  for (var k in o)
    if (new RegExp("(" + k + ")").test(fmt))
      fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
  return fmt;
}
// 点击放大
function preViewShow(current, urls) {
  wx.previewImage({
    current: current,
    urls: urls,
  })
}
// 复制
function copy(data) {
  var that = this
  wx.setClipboardData({
    data: data,
    success: function (res) {
      wx.getClipboardData({
        success: function (res) {
          wx.showToast({
            title: '复制成功',
          })
        }
      })
    }
  })
}
//动态改顶部兰标题
function navBarTitle(tmpTitle) {
  wx.setNavigationBarTitle({
    title: tmpTitle,
    success: function () {

    },
    complete: function () {

    }
  });
}
//时间戳
function ChangeDateFormat(val) {
  if (val != null) {
    var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", ""), 10));
    //月份为0-11，所以+1，月份 小时，分，秒小于10时补个0
    var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
    var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
    var hour = date.getHours() < 10 ? "0" + date.getHours() : date.getHours();
    var minute = date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
    var second = date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds();
    var dd = date.getFullYear() + "-" + month + "-" + currentDate + " " + hour + ":" + minute + ":" + second;
    // console.log(dd)
    return dd;
  }
  return "";
}
// 主题颜色改变
var skinList = [
  { name: "蓝色", type: "skin_blue", color: "#ffffff", bgcolor: "#218CD7", sel: true },
  { name: "粉色", type: "skin_pink", color: "#ffffff", bgcolor: "#FF5A9B", sel: false },
  { name: "绿色", type: "skin_green", color: "#ffffff", bgcolor: "#1ACC8E", sel: false },
  { name: "红色", type: "skin_red", color: "#ffffff", bgcolor: "#fe525f", sel: false },
  { name: "白色", type: "skin_white", color: "#000000", bgcolor: "#ffffff", sel: false },
  { name: "黑色", type: "skin_black1", color: "#ffffff", bgcolor: "#3a393f", sel: false },
  { name: "红色1", type: "skin_red1", color: "#ffffff", bgcolor: "#f51455", sel: false },
  { name: "红色2", type: "skin_red2", color: "#ffffff", bgcolor: "#e7475e", sel: false },
  { name: "红色3", type: "skin_red3", color: "#ffffff", bgcolor: "#f65676", sel: false },

  { name: "橙色1", type: "skin_orange1", color: "#ffffff", bgcolor: "#f7ad0a", sel: false },
  { name: "橙色2", type: "skin_orange2", color: "#ffffff", bgcolor: "#f79d2d", sel: false },
  { name: "橙色3", type: "skin_orange3", color: "#ffffff", bgcolor: "#f9c134", sel: false },
  { name: "橙色4", type: "skin_orange4", color: "#ffffff", bgcolor: "#f78500", sel: false },
  { name: "橙色5", type: "skin_orange5", color: "#ffffff", bgcolor: "#ef7030", sel: false },
  { name: "橙色6", type: "skin_orange6", color: "#ffffff", bgcolor: "#f05945", sel: false },

  { name: "绿色1", type: "skin_green1", color: "#ffffff", bgcolor: "#99cd4e", sel: false },
  { name: "绿色2", type: "skin_green2", color: "#ffffff", bgcolor: "#7dc24b", sel: false },
  { name: "绿色3", type: "skin_green3", color: "#ffffff", bgcolor: "#31b96e", sel: false },
  { name: "紫色1", type: "skin_purple1", color: "#ffffff", bgcolor: "#6c49b8", sel: false },
  { name: "紫色2", type: "skin_purple2", color: "#ffffff", bgcolor: "#86269b", sel: false },
  { name: "蓝色1", type: "skin_blue1", color: "#ffffff", bgcolor: "#4472ca", sel: false },
  { name: "蓝色2", type: "skin_blue2", color: "#ffffff", bgcolor: "#5e7ce2", sel: false },
  { name: "蓝色3", type: "skin_blue3", color: "#ffffff", bgcolor: "#1098f7", sel: false },
  { name: "蓝色4", type: "skin_blue4", color: "#ffffff", bgcolor: "#558ad8", sel: false },
  { name: "蓝色5", type: "skin_blue5", color: "#ffffff", bgcolor: "#2a93d4", sel: false }
];
function setPageSkin(fpage) {
  var pages = getApp().globalData.pages;
  if (!pages) {
    pages = wx.getStorageSync("PageSetting") || "";
    if (pages) {
      pages = pages.msg.pages;
      if (typeof pages == "string") {
        pages = JSON.parse(pages);
      }
    }
  }
  var skinIndex = 0;
  if (pages && pages.length > 0) {
    skinIndex = pages[0].skin;
  }
  wx.setNavigationBarColor({
    frontColor: skinList[skinIndex].color,
    backgroundColor: skinList[skinIndex].bgcolor,
  })
  fpage.setData({
    currentSkin: skinList[skinIndex].type
  });
}
// 提交备用formId
function commitFormId(formid, that) {
  let userInfo = wx.getStorageSync("userInfo")
  wx.request({
    url: addr.Address.commitFormId, //仅为示例，并非真实的接口地址
    data: {
      appid: getApp().globalData.appid,
      openid: userInfo.openId,
      formid: formid
    },
    method: "POST",
    header: {
      'content-type': 'application/json' // 默认值
    },
    success: function (res) {
      console.log('commitFormId', res)
    },
    fail: function () {
      console.log('提交备用formid出错')
    }
  })
}
// 判断版本库
function getSystem() {
  let ver1 = parseFloat(wx.getSystemInfoSync().SDKVersion)
  let ver2 = 1.5
  if (ver1 < ver2 || wx.getSystemInfoSync().SDKVersion == undefined) {
    wx.showModal({
      title: '提示',
      content: '当前微信版本过低，无法使用该功能，请升级到最新微信版本后重试',
      showCancel: false,
      success(res) {
        if (res.confirm) {
          wx.redirectTo({
            url: '/pages/errorpage/errorpage',
          })
        }
      }
    })
    return;
  }
}
module.exports = {
  copy,
  getSystem,
  commitFormId,
  setPageSkin,
  navBarTitle,
  preViewShow,
  ChangeDateFormat,
}

