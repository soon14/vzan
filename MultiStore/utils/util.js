import Promise from './es6-promise.min.js';
var addr = require("addr");
const tools = require("tools");
//时间对比函数，如果a>b返回1，如果a<b返回-1，相等返回0
function compareDateFormat(a, b) {
  var dateA = new Date(a.replace(/\-/g, "\/"));
  var dateB = new Date(b.replace(/\-/g, "\/"));
  if (isNaN(dateA) || isNaN(dateB)) return null;
  if (dateA > dateB) return 1;
  if (dateA < dateB) return -1;
  return 0;
}

//时间对比函数，如果a>b返回1，如果a<b返回-1，相等返回0
function compareDateFormatstr(a, b) {
  var dateA = new Date(a);
  var dateB = new Date(b);
  if (isNaN(dateA) || isNaN(dateB)) return null;
  if (dateA > dateB) return 1;
  if (dateA < dateB) return -1;
  return 0;
}

function compareTime(originTime, targetTime) {
  var argA = originTime.split(":");
  var argB = targetTime.split(":");
  console.log(argA[0]);
  var dateA = new Date(); // 创建 Date 对象。
  var dateB = new Date(); // 创建 Date 对象。
  dateA.setHours(argA[0], (argA[1] + '').indexOf('0') == 0 && argA[1].length > 1 ? argA[1].substring(1) : argA[1], 0);  // 设置 UTC 小时，分钟，秒。
  dateB.setHours(argB[0], (argB[1] + '').indexOf('0') == 0 && argB[1].length > 1 ? argB[1].substring(1) : argB[1], 30);  // 设置 UTC 小时，分钟，秒。
  if (isNaN(dateA) || isNaN(dateB)) return null;
  if (dateA > dateB) return 1;
  if (dateA < dateB) return -1;
  return 0;
}
//格式化时间
function formatTime(unixtime) {
  var year = date.getFullYear()
  var month = date.getMonth() + 1
  var day = date.getDate()
  var hour = date.getHours()
  var minute = date.getMinutes()
  var second = date.getSeconds()

  if (formatstring == null || formatstring == undefined) {
    return [year, month, day].map(formatNumber).join('/') + ' ' + [hour, minute, second].map(formatNumber).join(':')
  }
  else if (formatstring == "yyyy.MM.dd HH:mm") {
    return [year, month, day].map(formatNumber).join('.') + ' ' + [hour, minute].map(formatNumber).join(':')
  }
  else if (formatstring == "yyyy-MM-dd") {
    return [year, month, day].map(formatNumber).join('-')
  }
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
function ChangeDateFormatNew(val) {
  if (val != null) {
    var date = new Date(parseInt(val.replace("/Date(", "").replace(")-", ""), 10));
    var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
    var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
    var hour = date.getHours();
    var minute = date.getMinutes();
    var second = date.getSeconds();
    var dd = date.getFullYear() + "/" + month + "/" + currentDate + " " + hour + ":" + minute + ":" + second;
    return dd;
  }
  return "";
}
//时间间隔,a>b,返回天数，小时数，分钟数
function timeDiff(a, b) {
  var timespan = a - b;
  var days = parseInt(timespan / 3600000 / 24);
  var hoursspan = timespan - days * 24 * 3600000;
  var hours = parseInt(hoursspan > 0 ? hoursspan / 3600000 : 0);
  var minutesspan = hoursspan - hours * 3600000;
  var minutes = parseInt(minutesspan > 0 ? minutesspan / 60000 : 0);
  return [days, hours, minutes];
}
function cutstr(str, len) {
  var str_length = 0;
  var str_len = 0;
  var str_cut = new String();
  str_len = str.length;
  for (var i = 0; i < str_len; i++) {
    var a = str.charAt(i);
    str_length++;
    if (escape(a).length > 4) {
      //中文字符的长度经编码之后大于4  
      str_length++;
    }
    str_cut = str_cut.concat(a);
    if (str_length >= len) {
      str_cut = str_cut.concat("...");
      return str_cut;
    }
  }
  //如果给定字符串小于指定长度，则返回源字符串；  
  if (str_length < len) {
    return str;
  }
}
function formatNumber(n) {
  n = n.toString()
  return n[1] ? n : '0' + n
}
function log(msg) {
  var isDebug = getApp().globalData.isDebug;
  if (isDebug) {
    console.log(msg);
  }
}
function isFunction(value) {
  if (typeof (value) == "function") {
    return true;
  } else {
    return false;
  }
}

/**
 *
 * @param o 遍历对象中的属性，不存在则添加到对象o中
 * @param n
 */
function extendObject(o, n) {
  for (var p in n) {
    if (n.hasOwnProperty(p) && (!o.hasOwnProperty(p)))
      o[p] = n[p];
  }
}

function objToStr(obj) {
  var str = "";
  for (var p in obj) { // 方法
    if (typeof (obj[p]) == "function") {
      // obj [ p ]() ; //调用方法

    } else if (obj[p] != undefined && obj[p] != null) { // p 为属性名称，obj[p]为对应属性的值
      str += p + "=" + obj[p] + "&";
    }
  }
  return str;
}

/** 判断对象是否为空 */
function isOptStrNull(str) {
  if (str == undefined || str == null || str == '' || str == 'null' || str == '[]' || str == '{}') {
    return true
  } else {
    return false;
  }
}

function resizeimg(imgurl, width, height) {
  if (imgurl == null || imgurl == undefined || imgurl == "")
    return "";
  if (imgurl.indexOf("//i.vzan.cc/") > -1 && imgurl.indexOf("?x-oss-process") < 0) {
    if (!width) {
      imgurl += "?x-oss-process=image/resize,limit_0,m_fill,h_" + height + "/format,";
    }
    if (!height) {
      imgurl += "?x-oss-process=image/resize,limit_0,m_fill,w_" + width + "/format,";
    }
    if (width > 0 && height > 0) {
      imgurl += "?x-oss-process=image/resize,limit_0,m_fill,w_" + width + ",h_" + height + "/format,";
    }
    return imgurl += "gif";
  }
  else {
    return imgurl;
  }
}

//转译特殊字符
function jsonReplaceToJSONString(json) {
  var JSONString = JSON.stringify(json)
  JSONString = JSONString.replace(/\&/g, "%26");
  JSONString = JSONString.replace(/\?/g, "%3F");
  return JSONString
}

function JSONStringReplaceToJson(JSONString) {
  var json = JSONString.replace(/\%26/g, "&");
  json = json.replace(/\%3F/g, "?");
  return json
}

// PayOrder
function PayOrder(param, pay_callback) {
  var that = this
  wx.showToast({
    title: '加载中...',
    icon: 'loading',
    duration: 2000,
  })
  wx.showNavigationBarLoading()
  wx.request({
    url: addr.Address.PayOrder,
    data: {
      openId: param.openId,
      orderid: param.orderid,
      'type': param.type,
    },
    method: 'POST',
    header: {
      'content-type': 'application/x-www-form-urlencoded',
    },
    success: function (res) {
      if (res.data.result == true) {
        var obj = res.data.obj
        var jsObj = JSON.parse(obj)
        // if()
        getApp().globalData.reduction = res.data.extdata.reductionCart
        //发起支付
        pay_callback.success("wxpay")
        wxpay(jsObj, {
          failed: function () {
            pay_callback.failed("failed")
          },
          success: function () {
            pay_callback.success("success")
          }
        })
      } else {
        pay_callback.failed("failed")
      }
    },
  })
}
/* 支付   */
function wxpay(param, callback) {
  var taht = this
  console.log(param)
  wx.requestPayment({
    appId: param.appId,
    timeStamp: param.timeStamp,
    nonceStr: param.nonceStr,
    package: param.package,
    signType: param.signType,
    paySign: param.paySign,
    success: function (res) {
      callback.success("success")
      delete callback.success;
    },
    fail: function (res) {
      callback.failed("failed");
      delete callback.failed;
    },
    complete: function (res) {
      if (("failed" in callback) && res.errMsg == 'requestPayment:cancel') { //支付取消
      }
    }
  })
}
module.exports = {
  log,
  cutstr,
  timeDiff,
  PayOrder,
  objToStr,
  resizeimg,
  isFunction,
  formatTime,
  compareTime,
  extendObject,
  isOptStrNull,
  ChangeDateFormat,
  compareDateFormat,
  ChangeDateFormatNew,
  compareDateFormatstr,
  JSONStringReplaceToJson,
  jsonReplaceToJSONString,
}
