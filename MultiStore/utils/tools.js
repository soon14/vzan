// const pageRequest = require("public-request.js");

// 浮点数求和
function accAdd(arg1, arg2) {
  var r1, r2, m;
  try { r1 = arg1.toString().split(".")[1].length } catch (e) { r1 = 0 }
  try { r2 = arg2.toString().split(".")[1].length } catch (e) { r2 = 0 }
  m = Math.pow(10, Math.max(r1, r2))
  return (arg1 * m + arg2 * m) / m
}
Number.prototype.add = function (arg) {
  return accAdd(arg, this);
}
// 浮点数相减
function accSubtr(arg1, arg2) {
  var r1, r2, m, n;
  try { r1 = arg1.toString().split(".")[1].length } catch (e) { r1 = 0 }
  try { r2 = arg2.toString().split(".")[1].length } catch (e) { r2 = 0 }
  m = Math.pow(10, Math.max(r1, r2));
  //动态控制精度长度
  n = (r1 >= r2) ? r1 : r2;
  return Number(((arg1 * m - arg2 * m) / m).toFixed(n));
}
Number.prototype.sub = function (arg) {
  return accSubtr(this, arg);
}

// 浮点数相乘
function accMul(arg1, arg2) {
  var m = 0, s1 = arg1.toString(), s2 = arg2.toString();
  try { m += s1.split(".")[1].length } catch (e) { }
  try { m += s2.split(".")[1].length } catch (e) { }
  return Number(s1.replace(".", "")) * Number(s2.replace(".", "")) / Math.pow(10, m)
}
Number.prototype.mul = function (arg) {
  return accMul(arg, this);
}
// 浮点数相除
function accDiv(arg1, arg2) {
  var t1 = 0, t2 = 0, r1, r2;
  try { t1 = arg1.toString().split(".")[1].length } catch (e) { }
  try { t2 = arg2.toString().split(".")[1].length } catch (e) { }
  r1 = Number(arg1.toString().replace(".", ""))
  r2 = Number(arg2.toString().replace(".", ""))
  return (r1 / r2) * Math.pow(10, t2 - t1);
}
Number.prototype.div = function (arg) {
  return accDiv(this, arg);
}
// 主题颜色改变
var skinList = [
  { name: "蓝色", type: "skin_blue", color: "#000000", bgcolor: "#ffffff", sel: true },
  { name: "粉色", type: "skin_pink", color: "#ffffff", bgcolor: "#FF5A9B", sel: false },
  { name: "绿色", type: "skin_green", color: "#ffffff", bgcolor: "#1ACC8E", sel: false },
  { name: "红色", type: "skin_red", color: "#ffffff", bgcolor: "#fe525f", sel: false },
];

var http = require("http.js");
var addr = require("addr.js");
var tools = {

  // 打开地图引导
  openMap: function (latitude, longitude, name, address) {
    wx.openLocation({
      latitude: latitude,
      longitude: longitude,
      scale: 28,
      name: name,
      address: address
    })
  },
  //拨打电话
  phone: function (phoneNumber) {
    wx.makePhoneCall({
      phoneNumber: phoneNumber
    })
  },
  //图片 点击放大
  preViewShow: function (current, urls) {
    wx.previewImage({
      current: current,
      urls: urls,
    })
  },
  // 复制
  copy: function (data) {
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
  },
  //动态改顶部兰标题
  navBarTitle: function (tmpTitle) {
    wx.setNavigationBarTitle({
      title: tmpTitle,
      success: function () {
        console.log("" + tmpTitle);
      },
      complete: function () {
        console.log("动态修改微信小程序的页面标题-complete");
      }
    });
  },
  // 改变NavigationBar颜色
  setPageSink: function (fpage, skinIndex) {
    wx.setNavigationBarColor({
      frontColor: skinList[skinIndex].color,
      backgroundColor: skinList[skinIndex].bgcolor,
      animation: {
        duration: 400,
        timingFunc: 'easeIn'
      }
    })
    fpage.setData({
      currentSkin: skinList[skinIndex].type
    });
  },
  _clickMoadl: function (msg) {
    return new Promise(function (resolve, reject) {
      wx.showModal({
        title: '提示',
        content: msg,
        success: res => {
          resolve(res)
        }
      })
    })
  },
  _clickMoadlNo: function (msg) {
    return new Promise(function (resolve, reject) {
      wx.showModal({
        title: '提示',
        content: msg,
        showCancel: false,
        success: res => {
          resolve(res)
        }
      })
    })
  },
  //showModal
  showModal: function (msg) {
    wx.showModal({
      title: '提示',
      content: msg,
    })
  },
  // 不带取消按钮modal提示框
  showModalFalse: function (msg) {
    wx.showModal({
      title: '提示',
      content: msg,
      showCancel: false,
    })
  },
  // 修改确定按钮modal提示
  showModalConfig: function (confirm, msg) {
    wx.showModal({
      title: '提示',
      showCancel: false,
      confirmText: confirm,
      content: msg,
    })
  },
  // 提示框
  showToast: function (msg) {
    wx.showToast({
      title: msg,
    })
  },
  // loading提示框
  showLoadToast: function (msg) {
    wx.showToast({
      title: msg,
      duration: 1000,
      icon: "loading"
    })
  },
  //跳转新页面
  goNewPage: function (url) {
    wx.navigateTo({
      url: url,
    })
  },
  //重启动
  goLaunch: function (url) {
    wx.reLaunch({
      url: url
    })
  },
  //重定向跳转
  goRedito: function (url) {
    wx.redirectTo({
      url: url
    })
  },
  //返回上一页
  goBack: function () {
    wx.navigateBack({
      delta: 1
    })
  },
  // tabar页跳转
  goSwitch: function (url) {
    wx.switchTab({
      url: url,
    })
  },
  // promise回调
  callBack: function (tag) {
    return new Promise(function (resolve, reject) {
      if (tag) {
        resolve()
      } else {
        reject()
      }
    })
  },

  // 页面刷新
  onPull: function (fpage) {
    let that = fpage
    wx.showToast({
      title: "正在刷新",
      icon: "loading",
      duration: 1000
    })
    setTimeout(function () {
      wx.showToast({
        title: "刷新成功",
        duration: 1000
      })
      that.onLoad()
      wx.stopPullDownRefresh()
    }, 1000)
  },
  // 重置条件
  resetFunc(fpage) {
    let that = fpage
    Object.assign(that.data.goodListViewModal, { pageindex: 1, list: [], ispost: false, loadall: false });
  },

	/*
倒计时
  appId
  couponId
  userId
  */
  formatDuring: function (mss) {
    if (mss < 0) {
      return "00:00:00";
    } else {
      var days = parseInt(mss / (1000 * 60 * 60 * 24));
      var hours = parseInt((mss % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      var minutes = parseInt((mss % (1000 * 60 * 60)) / (1000 * 60));
      var seconds = (mss % (1000 * 60)) / 1000;
      if (hours == 0) {
        return minutes + "分钟" + Math.round(seconds) + "秒";
      } else if (days == 0) {
        return hours + "小时" + minutes + "分钟" + Math.round(seconds) + "秒";
      } else {
        return days + "天" + hours + "小时" + minutes + "分钟" + Math.round(seconds) + "秒";
      }
    }
  },
  GetMyCouponList: function (postData) {
    return http.postAsync(addr.Address.GetMyCouponList, postData);
  },
}
module.exports = tools