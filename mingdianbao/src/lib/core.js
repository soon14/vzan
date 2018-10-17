import wepy from "wepy";
import addr from "./addr.js"
import _get from './lodash.get';
import {
  wxParse
} from './wxParse/wxParse';
var isEndClock = null;
var timer_countdown = null;
var isdebug = true;
let reConnectTimer = null;
let isConnecting = false; //ws是否正在连接中
let isFirst = true;
var requestParameter = {
  url: "",
  data: {},
  method: 'GET',
  header: {
    "content-type": "application/x-www-form-urlencoded"
  },
}
var requestParameterjson = {
  url: "",
  data: {},
  method: 'GET',
  header: {
    'content-type': "application/json"
  },
}
const scopeList = {
  "userInfo": "scope.userInfo", //wx.getUserInfo	    用户信息
  "userLocation": "scope.userLocation", //wx.chooseLocation	地理位置
  "address": "scope.address", //wx.chooseAddress	    通讯地址
  "invoiceTitle": "scope.invoiceTitle", //wx.chooseInvoiceTitle发票抬头
  "werun": "scope.werun", //wx.getWeRunData	    微信运动步数
  "record": "scope.record", //wx.startRecord	    录音功能
  "writePhotosAlbum": "scope.writePhotosAlbum", //wx.saveImageToPhotosAlbum, wx.saveVideoToPhotosAlbum	保存到相册
  "camera": "scope.camera", //摄像头
}
const skinList = [{
  name: "蓝色",
  type: "skin_blue",
  color: "#ffffff",
  bgcolor: "#218CD7",
  sel: true
},
{
  name: "粉色",
  type: "skin_pink",
  color: "#ffffff",
  bgcolor: "#FF5A9B",
  sel: false
},
{
  name: "绿色",
  type: "skin_green",
  color: "#ffffff",
  bgcolor: "#1ACC8E",
  sel: false
},
{
  name: "红色",
  type: "skin_red",
  color: "#ffffff",
  bgcolor: "#fe525f",
  sel: false
},
{
  name: "白色",
  type: "skin_white",
  color: "#000000",
  bgcolor: "#ffffff",
  sel: false
},
{
  name: "黑色",
  type: "skin_black1",
  color: "#ffffff",
  bgcolor: "#3a393f",
  sel: false
},
{
  name: "红色1",
  type: "skin_red1",
  color: "#ffffff",
  bgcolor: "#f51455",
  sel: false
},
{
  name: "红色2",
  type: "skin_red2",
  color: "#ffffff",
  bgcolor: "#e7475e",
  sel: false
},
{
  name: "红色3",
  type: "skin_red3",
  color: "#ffffff",
  bgcolor: "#f65676",
  sel: false
},

{
  name: "橙色1",
  type: "skin_orange1",
  color: "#ffffff",
  bgcolor: "#f7ad0a",
  sel: false
},
{
  name: "橙色2",
  type: "skin_orange2",
  color: "#ffffff",
  bgcolor: "#f79d2d",
  sel: false
},
{
  name: "橙色3",
  type: "skin_orange3",
  color: "#ffffff",
  bgcolor: "#f9c134",
  sel: false
},
{
  name: "橙色4",
  type: "skin_orange4",
  color: "#ffffff",
  bgcolor: "#f78500",
  sel: false
},
{
  name: "橙色5",
  type: "skin_orange5",
  color: "#ffffff",
  bgcolor: "#ef7030",
  sel: false
},
{
  name: "橙色6",
  type: "skin_orange6",
  color: "#ffffff",
  bgcolor: "#f05945",
  sel: false
},

{
  name: "绿色1",
  type: "skin_green1",
  color: "#ffffff",
  bgcolor: "#99cd4e",
  sel: false
},
{
  name: "绿色2",
  type: "skin_green2",
  color: "#ffffff",
  bgcolor: "#7dc24b",
  sel: false
},
{
  name: "绿色3",
  type: "skin_green3",
  color: "#ffffff",
  bgcolor: "#31b96e",
  sel: false
},
{
  name: "紫色1",
  type: "skin_purple1",
  color: "#ffffff",
  bgcolor: "#6c49b8",
  sel: false
},
{
  name: "紫色2",
  type: "skin_purple2",
  color: "#ffffff",
  bgcolor: "#86269b",
  sel: false
},
{
  name: "蓝色1",
  type: "skin_blue1",
  color: "#ffffff",
  bgcolor: "#4472ca",
  sel: false
},
{
  name: "蓝色2",
  type: "skin_blue2",
  color: "#ffffff",
  bgcolor: "#5e7ce2",
  sel: false
},
{
  name: "蓝色3",
  type: "skin_blue3",
  color: "#ffffff",
  bgcolor: "#1098f7",
  sel: false
},
{
  name: "蓝色4",
  type: "skin_blue4",
  color: "#ffffff",
  bgcolor: "#558ad8",
  sel: false
},
{
  name: "蓝色5",
  type: "skin_blue5",
  color: "#ffffff",
  bgcolor: "#2a93d4",
  sel: false
}
];
// 浮点数求和
function accAdd(arg1, arg2) {
  var r1, r2, m;
  try {
    r1 = arg1.toString().split(".")[1].length
  } catch (e) {
    r1 = 0
  }
  try {
    r2 = arg2.toString().split(".")[1].length
  } catch (e) {
    r2 = 0
  }
  m = Math.pow(10, Math.max(r1, r2))
  return (arg1 * m + arg2 * m) / m
}
Number.prototype.add = function (arg) {
  return accAdd(arg, this);
}
// 浮点数相减
function accSubtr(arg1, arg2) {
  var r1, r2, m, n;
  try {
    r1 = arg1.toString().split(".")[1].length
  } catch (e) {
    r1 = 0
  }
  try {
    r2 = arg2.toString().split(".")[1].length
  } catch (e) {
    r2 = 0
  }
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
  var m = 0,
    s1 = arg1.toString(),
    s2 = arg2.toString();
  try {
    m += s1.split(".")[1].length
  } catch (e) { }
  try {
    m += s2.split(".")[1].length
  } catch (e) { }
  return Number(s1.replace(".", "")) * Number(s2.replace(".", "")) / Math.pow(10, m)
}
Number.prototype.mul = function (arg) {
  return accMul(arg, this);
}
// 浮点数相除
function accDiv(arg1, arg2) {
  var t1 = 0,
    t2 = 0,
    r1, r2;
  try {
    t1 = arg1.toString().split(".")[1].length
  } catch (e) { }
  try {
    t2 = arg2.toString().split(".")[1].length
  } catch (e) { }
  r1 = Number(arg1.toString().replace(".", ""))
  r2 = Number(arg2.toString().replace(".", ""))
  return (r1 / r2) * Math.pow(10, t2 - t1);
}
Number.prototype.div = function (arg) {
  return accDiv(this, arg);
}

var tools = {
  typeEnum: {
    //由data改成date mark
    "date": "[object Date]",
    "object": "[object Object]",
    "number": "[object Number]",
    "string": "[object String]",
    "boolean": "[object Boolean]"
  },
  getType: function (obj) {
    return Object.prototype.toString.call(obj)
  },
  getTimeSpan: function (time) {
    if (tools.getType(time) == tools.typeEnum.string) {
      time = time.replace(/-/g, "/");
    }
    time = new Date(time).getTime();
    var now = new Date().getTime();
    if (time - now <= 0)
      return 0;
    else
      return time - now
  },
  /**
   * 时间补零 mark
   * @param {Number} time 时间元素
   * @return {String} 补零后的时间元素
   */
  patchTime: function (time) {
    return ('0' + time).slice(-2)
  },
  /**
   * 获取剩余时间 mark
   * @param {Number} time 时间轴
   * @return {Array} 包含格式化时间元素的数组[日，时，分，秒]
   */
  getRemainTimeQueue: function (time) {
    var days_ = parseInt(time / 1000 / 60 / 60 / 24)
    var hours_ = parseInt(time / 1000 / 60 / 60 % 24)
    var min_ = parseInt(time / 1000 / 60 % 60)
    var seconds_ = parseInt(time / 1000 % 60)
    return [
      this.patchTime(days_),
      this.patchTime(hours_),
      this.patchTime(min_),
      this.patchTime(seconds_)
    ]
  },
  //mark
  formatMillisecond: function (millisecond) {
    var days = Math.floor(millisecond / (1000 * 60 * 60 * 24));
    var hours = Math.floor((millisecond % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    var minutes = Math.floor((millisecond % (1000 * 60 * 60)) / (1000 * 60));
    var seconds = Math.floor((millisecond % (1000 * 60)) / 1000);

    return [
      this.patchTime(days),
      this.patchTime(hours),
      this.patchTime(minutes),
      this.patchTime(seconds)
    ];
  },
  //上传图片
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
                url: addr.Upload,
                filePath: tempFilePaths[uploadCount],
                name: 'file',
                formData: {
                  filetype: filetype,
                },
                success: function (res) {
                  console.log(res);
                  var data = JSON.parse(res.data);
                  if (data.result) {
                    uploadCount += 1;
                    console.log("上传成功", data.msg);
                    uploadImgs.push(data.msg);
                  } else {
                    console.log("上传失败", data);
                    resolve("");
                  }
                  if (uploadCount < tempFilePaths.length) {
                    uploadOne();
                  } else {
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
  //富文本图片点击放大
  richImg: function (src) {
    let list = []
    list.push(src)
    core.preViewShow(src, list)
  }
};
/**********************************支付**********************************************************/
var pay = {
  // 普通支付// PayOrder
  PayOrder: async function (param) {
    let userInfo = await core.getUserInfo()
    let aid = await core.getAid();
    return http.post(
      addr.PayOrder, {
        aid: aid,
        openId: userInfo.openId,
        orderid: param.orderid,
        'type': param.type,
      })
  },
  /* 支付   */
  wxpay: function (param) {
    let app = wepy.$instance
    wx.showNavigationBarLoading()
    return new Promise(function (resolve, rejcet) {
      wx.requestPayment({
        appId: app.globalData.appid,
        timeStamp: param.timeStamp,
        nonceStr: param.nonceStr,
        package: param.package,
        signType: param.signType,
        paySign: param.paySign,
        success: function (res) {
          resolve(res)
        },
        fail: function (res) {
          resolve(res)
        },
        complete: function (res) {
          resolve(res)
        }
      })
    })
  },
  // 拼团支付
  AddOrderNew: async function (param) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    return http.post(
      addr.AddPayOrderNew, {
        appId: app.globalData.appid,
        openid: userInfo.openId,
        userId: userInfo.userid,
        ordertype: param.order,
        paytype: param.paytype,
        jsondata: param.jsondata
      })
  },
  //价钱计算
  payMoney: function (_price, _dis, _num, _freight, joinPrice) {
    let discount = Number(_dis).div(100)
    let price = Number(_price).mul(discount)
    let reg = String(price).indexOf(".") + 1
    if (reg > 0) {
      price = Math.round(price * 100) / 100
    }
    let allPay = Number(price).mul(_num)
    let shouldPay = Number(allPay).add(Number(_freight))
    let priceStr = Number(shouldPay).sub(Number(joinPrice))
    return priceStr;
  },
};

/**********************************请求**********************************************************/
var http = {
  //异步请求
  postJson: function (url, data) {
    return new Promise(function (resolve, reject) {
      wx.request(Object.assign({}, requestParameterjson, {
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
          } else {
            isdebug && console.log(e);
            resolve("");
          }
        }
      }))
    });
  },
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
          } else {
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
          } else {
            isdebug && console.log(e);
            resolve("");
          }
        }
      }))
    });
  },

};
/**********************************画布**********************************************************/
var canvas = {
  getShare: async function () {
    let app = wepy.$instance
    let [imgUrl, qrcode, storeIcon] = ['', '', ""]
    let shareInfo = await core.getShareInfo()
    if (shareInfo.isok) {
      let [bg, StoreName, StoreContent, freeStyle, bottomText] = ["", shareInfo.obj.StoreName, shareInfo.obj.ADTitle, shareInfo.obj.StyleType, "小未程序"]
      shareInfo.obj.Logo.length ? storeIcon = shareInfo.obj.Logo[0].url + "?x-oss-process=image/circle,r_100/format,png" : ""
      shareInfo.obj.ADImg.length ? imgUrl = shareInfo.obj.ADImg[0].url.replace(/http/, "https") : imgUrl = "http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg"
      shareInfo.obj.Qrcode != "" ? qrcode = shareInfo.obj.Qrcode : core.showModalCancle("请上传小程序码")
      //共同属性
      let width = wx.getSystemInfoSync().windowWidth * 0.87
      let height = wx.getSystemInfoSync().windowHeight * 0.75
      let ctx = wx.createCanvasContext('firstCanvas')
      // 店铺大小位置
      let [w_store, h_store, x_store, y_store] = [0, 0, 0, 0]
      //二维码
      let [w_qrcode, h_qrcode, x_qrcode, y_qrcode] = [0, 0, 0, 0]
      //店铺名
      let [x_name, y_name] = [0, 0]
      //内容
      let [x_con, y_con] = [0, 0]
      // 水印
      let [x_logo, y_logo] = [0, 0]
      let file3 = {}
      let [x_icon, y_icon, w_icon, h_icon] = [0, 0, 0, 0]
      // 店铺logo
      if (freeStyle == 2 || freeStyle == 3 || freeStyle == 4) {
        file3 = await core.downFile(storeIcon)
      }
      let file1 = await core.downFile(qrcode);
      if (file1.statusCode == 200) {
        let file2 = await core.downFile(imgUrl);
        if (file2.statusCode == 200) {
          switch (freeStyle) {
            case 0:
              let filebg = await core.downFile("https://j.vzan.cc/miniapp/img/sharebg/1.png")
              bg = filebg.tempFilePath;
              x_store = width * 0.13;
              y_store = height * 0.14;
              w_store = width * 0.75;
              h_store = height * 0.30;
              x_qrcode = width * 0.18;
              y_qrcode = height * 0.635;
              w_qrcode = height * 0.16;
              h_qrcode = height * 0.16;
              x_name = width * 0.13;
              y_name = height * 0.5;
              x_con = width * 0.13;
              y_con = height * 0.58;
              x_logo = width * 0.45;
              y_logo = height * 0.98
              break;
            case 1:
              let filebg1 = await core.downFile("http://j.vzan.cc/miniapp/img/sharebg/2.png")
              bg = filebg1.tempFilePath;

              x_store = width * 0.068;
              y_store = height * 0.09;
              w_store = width * 0.867;
              h_store = height * 0.35;

              x_qrcode = width * 0.18;
              y_qrcode = height * 0.648;
              w_qrcode = width * 0.23;
              h_qrcode = height * 0.16;

              x_name = width * 0.15;
              y_name = height * 0.5;

              x_con = width * 0.15;
              y_con = height * 0.58;

              x_logo = width * 0.45;
              y_logo = height * 0.98
              break;
            case 2:
              let filebg2 = await core.downFile("http://j.vzan.cc/miniapp/img/sharebg/3.png")
              bg = filebg2.tempFilePath;
              x_store = width * 0.13;
              y_store = height * 0.18;
              w_store = width * 0.75;
              h_store = height * 0.30;

              x_qrcode = width * 0.23;
              y_qrcode = height * 0.675;
              w_qrcode = height * 0.16;
              h_qrcode = height * 0.16;

              x_name = width * 0.15;
              y_name = height * 0.54;

              x_con = width * 0.15;
              y_con = height * 0.63;

              x_logo = width * 0.45;
              y_logo = height * 0.98

              x_icon = width * 0.43;
              y_icon = height * 0.07;
              w_icon = height * 0.105;
              h_icon = height * 0.105;

              break;
            case 3:
              let filebg3 = await core.downFile("http://j.vzan.cc/miniapp/img/sharebg/4.png")
              bg = filebg3.tempFilePath;
              x_store = width * 0.059;
              y_store = height * 0.089;
              w_store = width * 0.5;
              h_store = height * 0.83;

              x_qrcode = width * 0.63;
              y_qrcode = height * 0.6;
              w_qrcode = height * 0.16;
              h_qrcode = height * 0.16;

              x_name = width * 0.655;
              y_name = height * 0.16;

              x_con = width * 0.63;
              y_con = height * 0.80;

              x_logo = width * 0.45;
              y_logo = height * 0.98

              x_icon = width * 0.68;
              y_icon = height * 0.18;
              w_icon = height * 0.1;
              h_icon = height * 0.1;

              break;
            case 4:
              let filebg4 = await core.downFile("http://j.vzan.cc/miniapp/img/sharebg/5.png")
              bg = filebg4.tempFilePath;
              x_store = width * 0.13;
              y_store = height * 0.18;
              w_store = width * 0.75;
              h_store = height * 0.30;

              x_qrcode = width * 0.23;
              y_qrcode = height * 0.698;
              w_qrcode = height * 0.16;
              h_qrcode = height * 0.16;

              x_name = width * 0.13;
              y_name = height * 0.53;

              x_con = width * 0.13;
              y_con = height * 0.59;

              x_logo = width * 0.45;
              y_logo = height * 0.98

              x_icon = width * 0.43;
              y_icon = height * 0.048;
              w_icon = height * 0.1;
              h_icon = height * 0.1;

              break;
            case 5:
              let filebg5 = await core.downFile("http://j.vzan.cc/miniapp/img/sharebg/6.png")
              bg = filebg5.tempFilePath;
              x_store = width * 0.14;
              y_store = height * 0.03;
              w_store = width * 0.72;
              h_store = height * 0.33;

              x_qrcode = width * 0.33;
              y_qrcode = height * 0.61;
              w_qrcode = height * 0.25;
              h_qrcode = height * 0.25;

              x_name = width * 0.125;
              y_name = height * 0.45;

              x_con = width * 0.125;
              y_con = height * 0.52;

              x_logo = width * 0.45;
              y_logo = height * 0.97
              break;
            case 6:
              let filebg6 = await core.downFile("http://j.vzan.cc/miniapp/img/sharebg/7.png")
              bg = filebg6.tempFilePath;
              x_store = width * 0.0001;
              y_store = height * 0.0001;
              w_store = width;
              h_store = height * 0.7;

              x_qrcode = width * 0.16;
              y_qrcode = height * 0.72;
              w_qrcode = height * 0.18;
              h_qrcode = height * 0.18;

              x_logo = width * 0.45;
              y_logo = height * 0.98;
              break;
          }
          ctx.setFillStyle('white')
          ctx.fillRect(0, 0, width, height)
          // 背景图
          ctx.drawImage(bg, 0, 0, width, height);
          //店铺logo
          if (freeStyle == 2 || freeStyle == 3 || freeStyle == 4) {
            ctx.drawImage(file3.tempFilePath, x_icon, y_icon, w_icon, h_icon)
          }
          //店铺图片
          ctx.drawImage(file2.tempFilePath, x_store, y_store, w_store, h_store);
          // 二维码
          ctx.drawImage(file1.tempFilePath, x_qrcode, y_qrcode, w_qrcode, h_qrcode);
          if (freeStyle != 6) {
            // 店铺名称
            ctx.setFontSize(14)
            ctx.setFillStyle('#333333')
            ctx.fillText(StoreName, x_name, y_name)
            // 内容
            ctx.setFontSize(12)
            ctx.setFillStyle('#978A8A')
            ctx.fillText(StoreContent, x_con, y_con)
          }
          // 水印
          ctx.setFontSize(8)
          ctx.setFillStyle('#E8D9D9')
          ctx.fillText(bottomText, x_logo, y_logo)
          ctx.draw()
        }
      }
    }
  },
  getQrcode: async function (vm, targetPage) {
    let app = wepy.$instance
    await http.get(addr.GetProductQrcode, {
      appId: app.globalData.appid,
      showQrcode: 1,
      version: 2,
      pid: vm.pid,
      recordId: vm.recordId,
      typeName: vm.type,
      showprice: vm.showprice
    }).then(async data => {
      let qrcode = await core.downFile(data.dataObj.qrCode);
      if (qrcode.statusCode == 200) {
        let Img = await core.downFile(targetPage.goodVm.img);
        if (Img.statusCode == 200) {
          let width = wx.getSystemInfoSync().windowWidth // 屏幕宽度
          let height = wx.getSystemInfoSync().windowHeight // 屏幕高度
          let ctx = wx.createCanvasContext('firstCanvas')
          var objImgUrl = Img.tempFilePath
          var code = qrcode.tempFilePath
          var title = targetPage.goodVm.name
          var title1 = title.substr(0, 9)
          var title2 = title.substr(9, 9)
          var title3 = title.substr(18, 9)
          var oldtitle = "原价"
          var newtitle = "现价"
          var money = "￥"
          var oldprice = "￥" + targetPage.goodVm.priceStr
          var discountTotal = targetPage.goodVm.discountPrice
          var tip = "长按查看商品"
          ctx.drawImage(objImgUrl, width * 0.05, 0, width * 0.8, width * 0.8) // 画商品图片
          ctx.setFillStyle('white')
          ctx.fillRect(width * 0.05, width * 0.8, width * 0.8, height * 0.27) //画图片下方的区域
          ctx.drawImage(code, width * 0.52, width * 0.82, width * 0.27, width * 0.27) //画小程序码
          ctx.setFontSize(16)
          ctx.setFillStyle("#333333")
          ctx.fillText(title1, width * 0.1, width * 0.88)
          ctx.fillText(title2, width * 0.1, width * 0.93)
          ctx.fillText(title3, width * 0.1, width * 0.98)
          ctx.setFontSize(14)
          ctx.setFillStyle("#9C9C9C")
          ctx.fillText(oldtitle, width * 0.1, width * 1.05)
          ctx.fillText(oldprice, width * 0.2, width * 1.05)
          ctx.fillText(newtitle, width * 0.1, width * 1.12)
          ctx.setFontSize(22)
          ctx.setFillStyle("#FF6700")
          ctx.fillText(discountTotal, width * 0.25, width * 1.12)
          ctx.setFontSize(12)
          ctx.setFillStyle("#FF6700")
          ctx.fillText(tip, width * 0.56, width * 1.12)
          ctx.setFontSize(14)
          ctx.setFillStyle("#FF6700")
          ctx.fillText(money, width * 0.2, width * 1.12)
          ctx.draw()
        }
      }

    })
  },
};
var core = {
  /**********************************封装工具类**********************************************************/
  //主题色改变
  setPageSkin: async function (targetPage) {
    let currentPage = await this.getStorage("pages");
    if (currentPage == "") {
      currentPage = await this.getPageSetting()
    }
    var skinIndex = 0;
    if (currentPage.pages && currentPage.pages.length > 0) {
      skinIndex = currentPage.pages[0].skin;
    }
    wx.setNavigationBarColor({
      frontColor: skinList[skinIndex].color,
      backgroundColor: skinList[skinIndex].bgcolor,
    })
    targetPage.currentSkin = skinList[skinIndex].type
    targetPage.$apply()
  },
  //普通商品选择事件
  choosePro(goodVm, p, c) {
    let [specId, specInfo, pick, spec] = [
      [],
      [], goodVm.pickspecification, goodVm.GASDetailList
    ]
    let [currentList, self] = [pick[p], pick[p].items[c]]
    //判断选择哪个分类
    if (currentList.items.length > 0) {
      currentList.items.forEach(function (obj, i) {
        obj.id != self.id ? obj.sel = false : obj.sel = !obj.sel;
      })
    }
    //sel为true获取specId和分类信息
    for (let i = 0, val; val = pick[i++];) {
      for (let j = 0, valKey; valKey = val.items[j++];) {
        if (valKey.sel) {
          let [parentName, childName] = [val.name, valKey.name]
          let specName = parentName + ":" + childName
          specId.push(valKey.id)
          specInfo.push(specName)
        }
      }
    }
    goodVm.specId = specId.join("_")
    goodVm.specInfo = specInfo.join(" ")
    goodVm.totalCount = 1
    let specTemp = spec.find(f => f.id == goodVm.specId)
    //获取库存 原价以及折扣价
    if (specTemp) {
      goodVm.stock = specTemp.stock
      goodVm.priceStr = parseFloat(specTemp.price).toFixed(2)
      goodVm.discountPricestr = specTemp.discountPricestr
      goodVm.starPricedis = specTemp.discountPricestr
    } else {
      goodVm.starPricedis = parseFloat(goodVm.discountPrice).toFixed(2)
      goodVm.priceStr = parseFloat(goodVm.price).toFixed(2)
      goodVm.discountPricestr = parseFloat(goodVm.discountPrice).toFixed(2)
    }
    return {
      goodVm,
      currentList
    }
  },
  //普通商品加事件
  addPro(vm_sec, targetPage) {
    let _g = vm_sec
    let count = _g.totalCount
    let price = 0
    // 有规格
    if (_g.pickspecification.length) {
      let sp_id = _g.GASDetailList.find(f => f.id == _g.specId)
      if (sp_id == undefined) {
        core.ShowToast("请先选择规格", targetPage)
        return;
      } else {
        price = sp_id.discountPrice
        if (_g.stockLimit) {
          if (count < sp_id.stock) {
            count++;
          } else {
            core.ShowToast("库存不足", targetPage)
          }
        } else {
          count++
        }
      }
    } else {
      price = _g.discountPrice
      if (_g.stockLimit) {
        if (count < _g.stock) {
          count++
        } else {
          core.ShowToast("库存不足", targetPage)
        }
      } else {
        count++
      }
    }
    return {
      count,
      price
    }
  },
  //普通商品减事件
  lessPro(vm_sec, targetPage) {
    let count = vm_sec.totalCount
    if (count > 1) {
      count--
    } else {
      count = 1
      core.ShowToast("亲,不要再减啦", targetPage)
    }
    return count;
  },
  //倒计时
  //mark
  TimeShow: function (startDateStr, endDateStr) {
    let [starShow, endShow, timeInter] = [false, false, "00:00:00"]
    let end = endDateStr.replace(/-/g, "/")
    let star = startDateStr.replace(/-/g, "/")
    let timeQueue = ['days', 'hours', 'min', 'seconds']
    let starTime = (new Date(star)) - (new Date()); //计算剩余的毫秒数
    if (starTime > 0) {
      timeQueue = tools.getRemainTimeQueue(starTime) //根据剩余毫秒数转换对应日期数组
      starShow = true
      endShow = false
    } else {
      let endTime = (new Date(end)) - (new Date()); //计算剩余的毫秒数
      if (endTime <= 0) {
        return {
          timeInter: "00:00:00",
          starShow: false,
          endShow: false
        };
      }
      timeQueue = tools.getRemainTimeQueue(endTime) //根据剩余毫秒数转换对应日期数组
      endShow = true
      starShow = false
    }
    timeInter = timeQueue.join(':')
    return {
      timeInter,
      starShow,
      endShow
    }
  },
  //拨打电话
  phoneFunc: function (phoneNumber) {
    if (phoneNumber) {
      wx.makePhoneCall({
        phoneNumber: phoneNumber,
      })
    } else {
      this.loading("未设置电话")
    }
  },
  //跳转小程序
  goNewMiniapp: function (item) {
    wx.navigateToMiniProgram({
      appId: item.appid,
      path: item.path,
      success(res) {
        console.log(res)
      },
      fail(err) {
        core.showModalCancle("跳转失败")
      }
    })
  },
  //扫码
  sceneQrcode: function (url) {
    wx.scanCode({
      onlyFromCamera: true,
      success: async res => {
        console.log(res)
        if (res.path == undefined) {
          core.showModalCancle("亲，该二维码有误")

        } else { //扫码成功操作
          await core.wxToast('扫码成功')
          if (url != -1) {
            core.goRedirecto("/pages/index/index?currentPageIndex=" + url)
          }

        }
      }
    })
  },
  //页面回到顶部
  onPageScroll: function () {
    wx.pageScrollTo({
      scrollTop: 0,
      duration: 0
    });
  },
  //动态改顶部兰标题
  setPageTitle: function (tmpTitle) {
    wx.setNavigationBarTitle({
      title: tmpTitle,
    });
  },
  // 图片点击放大
  preViewShow: function (current) {
    let urls = []
    urls.push(current)
    wx.previewImage({
      current: urls[0],
      urls: urls,
    })
  },
  // 打开地图
  openMap: function (vm) {
    wx.openLocation({
      latitude: vm.lat,
      longitude: vm.lng,
      name: vm.name,
      scale: 28
    })
  },
  //loading加载
  showLoading: function () {
    wx.showLoading({
      title: '加载中...',
      mask: true,
    })
  },
  showLoadingNo: function () {
    wx.showLoading({
      title: '加载中...',
    })
  },
  //页面跳转
  goNewPage: function (url) {
    wx.navigateTo({
      url
    })
  },
  //返回
  goBack: function (delta) {
    wx.navigateBack({
      delta: delta
    })
  },
  //消除中间栈层
  goRedirecto: function (url) {
    wx.redirectTo({
      url: url,
    })
  },
  //重启动
  goLaunch: function (url) {
    wx.reLaunch({
      url: url
    })
  },
  // 复制
  copy: function (data) {
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
  //地址选择
  chooseAddress: function () {
    return new Promise(function (resolve, reject) {
      wx.chooseAddress({
        success: function (res) {
          resolve(res)
        },
        fail: function (res) {
          resolve(res)
        }
      })
    })
  },
  //显示模态弹窗
  showModal: function (msg) {
    return new Promise(function (resolve, reject) {
      wx.showModal({
        title: "提示",
        content: msg,
        success: res => {
          resolve(res)
        }
      })
    })
  },
  //菜单栏
  showAction: function (item) {
    return new Promise(function (resolve, reject) {
      wx.showActionSheet({
        itemList: item,
        success: function (res) {
          resolve(res)
        }
      })
    })
  },
  showModalCancle: function (msg) {
    return new Promise(function (resolve, reject) {
      wx.showModal({
        title: "提示",
        content: msg,
        showCancel: false,
        success: res => {
          resolve(res)
        }
      })
    })
  },
  //显示提示框
  ShowToast: function (msg, targetPage) {
    targetPage.$toast.show = true;
    targetPage.$toast.msg = msg;
    targetPage.$apply();
    setTimeout(() => {
      targetPage.$toast.show = false;
      targetPage.$apply();
    }, 1000);
  },
  loading: function (msg) {
    wx.showToast({
      title: msg,
      icon: "loading",
      duration: 1000
    })
  },
  wxToast: function (msg) {
    wx.showToast({
      title: msg,
      duration: 1000
    })
  },
  //下载api封装
  downFile: function (url) {
    return new Promise(function (resolve, reject) {
      wx.downloadFile({
        url: url,
        success: function (res) {
          resolve(res)
        }
      })
    })
  },
  //时间戳
  //mark
  ChangeDateFormat: function (val) {
    if (val != null) {
      var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", ""), 10));
      //月份为0-11，所以+1，月份 小时，分，秒小于10时补个0
      var month = tools.patchTime(date.getMonth() + 1);
      var currentDate = tools.patchTime(date.getDate());
      var hour = tools.patchTime(date.getHours());
      var minute = tools.patchTime(date.getMinutes());
      var second = tools.patchTime(date.getSeconds());
      var dd = date.getFullYear() + "-" + month + "-" + currentDate + " " + hour + ":" + minute + ":" + second;
      return dd;
    }
    return "";
  },
  /**********************************封装wx begin**********************************************************/
  //检查登陆状态
  checkSession: function () {
    return new Promise(function (resolve, reject) {
      resolve(true)
      // wx.checkSession({
      //   success: function () {
      //     resolve(true);
      //   },
      //   fail: function () {
      //     resolve(false);
      //   }
      // })
    });
  },
  //获取code
  login: function () {
    return new Promise(function (resolve, reject) {
      wx.login({
        success: function (res) {
          if (res.code) {
            resolve(res.code);
          } else {
            resolve(false);
          }
        }
      });
    })
  },
  //基本信息，不包含opneid 
  getBaseUserInfo: function () {
    return new Promise(function (resolve, reject) {
      wx.getUserInfo({
        withCredentials: true,
        success: function (res) {
          resolve(res);
        },
        fail: function () {
          resolve(false)
        }
      })
    });
  },

  // openSetting: function () {
  //   wx.showModal({
  //     title: "提示",
  //     confirmText: "设置",
  //     showCancel: false,
  //     content: "授权后，才能继续使用。",
  //     success: function (res) {
  //       isdebug && console.log(res.confirm);
  //       if (res.confirm) {
  //         wx.openSetting({
  //           success: function (setres) {
  //             wx.hideLoading()
  //           }
  //         });
  //       }
  //     }
  //   });
  // },
  getStorage: function (key) {
    return new Promise(function (resolve, reject) {
      wx.getStorage({
        key,
        success: function (res) {
          resolve(res.data);
        },
        fail: function () {
          resolve("");
        },
      })
    });
  },
  getSetting: function (scope) {
    return new Promise(function (resolve, reject) {
      wx.getSetting({
        success: function (res) {
          resolve(res);
        },
        fail: function () {
          resolve({});
        }
      });
    });
  },
  //清除缓存
  clearStorage: function (key) {
    wx.removeStorage({
      key,
    })
  },
  //设置缓存
  setStorage: function (key, data) {
    wx.setStorage({
      key,
      data,
    })
  },
  //检查某个权限是否允许授权
  checkAuth: async function (scope) {
    let authSetting = await this.getSetting(scope);
    if (!authSetting[scope]) {
      return false
    } else {
      return true;
    }

  },
  /**********************************封装wx end**********************************************************/
  getAid: async function () {
    var aid = await this.getStorage("aid");
    var app = wepy.$instance;
    var appid = app.globalData.appid;

    if (!aid) {
      var aidInfo = await http.post(addr.Getaid, {
        appid
      });
      if (aidInfo && aidInfo.isok) {
        try {
          core.setStorage("aid", aidInfo.msg)
        } catch (e) { }
        return aidInfo.msg;
      }
      return "";
    } else
      return aid
  },
  loginByThirdPlatform: async function (appid, code, encryptedData, signature, iv, isphonedata) {
    return await http.post(addr.loginByThirdPlatform, {
      code: code,
      data: encryptedData,
      signature: signature,
      iv: iv,
      appid: appid,
      isphonedata: isphonedata
    })
  },

  // getUserInfo: async function () {
  //   let app = wepy.$instance;
  //   let userInfo = await core.getStorage("userInfo");
  //   let sessionStatus = await core.checkSession()

  //   if (sessionStatus && userInfo != "") {
  //     return userInfo;
  //   } else {
  //     let code = await core.login();
  //     if (code) {
  //       //这一步会要求授权，如果拒绝授权，encData=false
  //       let encData = await core.getBaseUserInfo();
  //       if (encData) {
  //         let result = await core.loginByThirdPlatform(
  //           app.globalData.appid,
  //           code,
  //           encData.encryptedData,
  //           encData.signature,
  //           encData.iv,
  //           0
  //         );

  //         if (result && result.result) {
  //           await core.setStorage("userInfo", result.obj)
  //           return result.obj;
  //         }
  //         userInfo = "";
  //       }
  //       if (!await core.checkAuth(scopeList.userInfo)) {
  //         core.openSetting();
  //       }
  //       userInfo = "";
  //     }
  //     return userInfo;
  //   }
  // },
  //会员信息
  getVipInfo: async function () {
    let vipInfo = "";
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo();
    let appid = app.globalData.appid;
    if (userInfo) {
      if (app.globalData.vipInfo) {
        return app.globalData.vipInfo;
      }
      vipInfo = await http.get(addr.GetVipInfo, {
        appid,
        uid: userInfo.userid
      });
      if (vipInfo && vipInfo.isok) {
        app.globalData.vipInfo = vipInfo.model;
      }
      return vipInfo.model;
    }
    return ""
  },
  getPageConfig: async function () {
    let aid = await this.getAid();
    return http.post(addr.GetPageSetting, {
      aid
    });
  },
  //分享
  getShareInfo: function () {
    let app = wepy.$instance
    return http.get(
      addr.GetShare, {
        appId: app.globalData.appid
      })
  },
  getPageSettingUpdateTime: async function () {
    let aid = await this.getAid();
    let updatetimeInfo = await http.post(addr.GetPageSettingUpdateTime, {
      aid
    });
    if (updatetimeInfo && updatetimeInfo.isok)
      return updatetimeInfo.msg;
    else
      return new Date().getTime();
  },
  getPageSetting: async function () {
    let pages = await this.getStorage("pages");
    let updatetime = await this.getPageSettingUpdateTime();
    if (pages == "" || pages.updatetime != updatetime) {
      pages = await core.getPageConfig()
      if (pages && pages.isok) {
        if (typeof pages.msg.pages == "string") {
          pages.msg.pages = JSON.parse(pages.msg.pages);
        }
        await core.setStorage("pages", pages.msg)
        return pages.msg;
      }
      return "";
    } else
      return pages;
  },
  getGoodsByids: async function (ids, com) {
    let app = wepy.$instance;
    let ShowType = com.goodShowType || ""
    let levelid = 0;
    let vipInfo = await this.getVipInfo();
    if (vipInfo) {
      levelid = vipInfo.levelid;
    }
    return http.post(
      addr.GetGoodsByids, {
        ids,
        levelid,
        goodShowType: ShowType
      })
  },
  getGroupByIds: async function (ids) {
    let aid = await this.getAid();
    return http.post(
      addr.GetGroupByIds, {
        ids,
        aid
      });
  },
  // 内容资讯请求
  getNews: async function (typeid, liststyle) {
    let aid = await this.getAid();
    return http.get(
      addr.GetNewsList, {
        aid,
        typeid,
        liststyle,
      })
  },
  // 选择内容资讯请求
  getNewsByids: async function (ids, liststyle) {
    return http.get(
      addr.GetNewsInfoByids, {
        ids,
        liststyle,
      })
  },
  //获取内容列表
  getConByids: async function (ids, vm) {
    return http.get(
      addr.GetNewsInfoByids, {
        ids,
        pageindex: vm.pageindex,
        pagesize: vm.pagesize,
      })
  },
  contentDetail: async function (id) {
    return new Promise(function (resolve, reject) {
      http.get(
        addr.GetNewsInfo, {
          id: id,
          version: 2,
        }).then(data => {
          resolve(data)
        })
    })
  },
  // formId
  formId: async function (formid) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()
    http.post(
      addr.commitFormId, {
        appid: app.globalData.appid,
        openid: userInfo.openId,
        formid: formid
      })
  },
  // 店铺配置
  GetStoreInfo: function () {
    let app = wepy.$instance
    return http.get(
      addr.GetStoreInfo, {
        appId: app.globalData.appid
      })
  },
  //获取储值余额
  valueMoney: async function () {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    return http.get(
      addr.getSaveMoneySetUser, {
        appid: app.globalData.appid,
        openid: userInfo.openId,
      })
  },
  //产品详情
  goodDetail: async function (pid) {

    let vipInfo = await core.getVipInfo();
    let levelid = vipInfo.levelid;
    return http.get(
      addr.GetGoodInfo, {
        pid,
        levelid,
        version: 2,
      })
  },
  //预约列表
  subList: async function (vm) {
    let aid = await this.getAid();
    let userInfo = await this.getUserInfo();
    return http
      .get(
      addr.GetSubscribeFormDetail, {
        aid,
        uid: userInfo.userid,
        pageindex: vm.pageindex,
        pagesize: vm.pagesize
      })
  },

  //拼团1.0
  groupInfo: async function (groupid) {
    let app = wepy.$instance
    return http
      .post(addr.GetGroupDetail, {
        appId: app.globalData.appid,
        groupId: groupid,
      })
  },
  //拼团2.0
  GetentGroupDetail: async function (groupid) {
    let app = wepy.$instance
    return http.get(
      addr.GetentGroupDetail, {
        appid: app.globalData.appid,
        groupid: groupid
      })
  },
  // 获取购物车
  shopCar: async function () {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo();
    let vipInfo = await core.getVipInfo();
    return http.get(
      addr.getGoodsCarData_new, {
        appId: app.globalData.appid,
        openid: userInfo.openId,
        levelid: vipInfo.levelid
      })
  },
  //悬浮按钮
  iconStatus: function (currentCom, pageIndex, targetPage) {
    let homeStatus = false
    let bootom = currentCom.find(f => f.type == 'bottomnav')
    if (bootom == undefined && pageIndex != 0) {
      homeStatus = true
    }
    for (let i = 0; i < currentCom.length; i++) {
      if (currentCom[i].type == 'form') {
        let _g = currentCom[i]
        _g.items.forEach(function (o, i) {
          if (o.type == 'radio') {
            let array = []
            for (let v in o.items) {
              array.push(o.items[v].name)
            }
            o.array = array
          }
        })
      }
    }
    targetPage.homeStatus = homeStatus
    targetPage.$apply()
  },
  renderPage: async function (targetPage, pageIndex) {
    let that = this;
    let pageSetting = await core.getPageSetting();
    //排除“产品预约”页面 不需要显示
    for (let i = 0; i < pageSetting.pages.length; i++) {
      if (pageSetting.pages[i].def_name == "产品预约") {
        this.setStorage("sub", pageSetting.pages[i].coms[0])
        pageSetting.pages.splice(i, 1)
      }
    }
    pageIndex = Number(pageIndex);
    if (pageIndex < 0 || pageIndex > pageSetting.pages.length) {
      return;
    }

    let currentPage = pageSetting.pages[pageIndex];
    targetPage.currentPageIndex = pageIndex;
    targetPage.currentPage = pageSetting.pages[pageIndex];
    await core.iconStatus(currentPage.coms, pageIndex, targetPage)
    //当重复点击或者切换时检测loadall，为-1时不用再次请求
    var loadall = _get(targetPage.pagesetStatus, pageIndex.toString(), -1)
    if (loadall != -1) {
      targetPage.$apply();
      core.setPageTitle(currentPage.name)
      return;
    }

    for (let comIndex = 0; comIndex < currentPage.coms.length; comIndex++) {
      await core.renderCom(pageIndex, comIndex, currentPage.coms[comIndex], targetPage);
    }
    // 页面加载完loadall=true
    var loadall = _get(targetPage.pagesetStatus, pageIndex.toString(), -1)
    if (loadall == -1) {
      targetPage.pagesetStatus[pageIndex.toString()] = true;
    }
    targetPage.$apply();
    core.setPageTitle(currentPage.name)
  },
  renderCom: async function (pageIndex, comIndex, currentCom, targetPage) {
    switch (currentCom.type) {
      case "good":
        await this.goodRequest(pageIndex, comIndex, currentCom, targetPage)
        break;
      case "video":
        currentCom.sel = "false"
        break;
      case "news":
        if (currentCom.listmode == "all" || (currentCom.listmode == 'pick' && currentCom.list.length == 0)) {
          await this.allNews(pageIndex, comIndex, currentCom, targetPage)
        } else {
          await this.chooseNews(pageIndex, comIndex, currentCom, targetPage);
        }
        break;
      case "live":
        let vm_live = {}
        let vm_live_key = pageIndex + "_" + comIndex
        vm_live = {
          list: currentCom.items.slice(0, 3),
          num: currentCom.items.length
        }
        targetPage.vm_com_live[vm_live_key] = vm_live;
        targetPage.$apply();
        break;
      case "cutprice":

        await this.bargain(pageIndex, comIndex, currentCom, targetPage) // 倒计时
        break;
      case "richtxt":
        //mark
        currentCom.content = currentCom.content
          .replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
          .replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
          .replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
        currentCom.content_fmt = wxParse('content_fmt', 'html', currentCom.content || "", targetPage, 5);
        targetPage.$apply();
        break;

      case "goodlist":
        let id = []
        if (currentCom.goodCat.length) {
          currentCom.goodCat.forEach(function (o, i) {
            id.push(o.id)
          })
        }
        targetPage.vm_goodsList.alltypeid = id.join(",")
        targetPage.vm_goodsList.typeid = id
        await this.getGoodsListRequest(targetPage.vm_goodsList, currentCom, targetPage)
        break;
      case "joingroup":
        await this.joingroupRequest(pageIndex, comIndex, currentCom, targetPage)
        break;
      case "entjoingroup":
        await this.getEngroupIds(pageIndex, comIndex, currentCom, targetPage)
        break;
      case "bgaudio":
        if (currentCom.src) {
          wx.playBackgroundAudio({
            dataUrl: currentCom.src
          })
        }
        break;
    }
  },
  /***************************主要组件请求***********************************************/
  //产品组件
  goodRequest: async function (pageIndex, comIndex, currentCom, targetPage) {
    let [vm, goodidsArray] = [{},
    []
    ]
    let key = pageIndex + "_" + comIndex;
    currentCom.items.forEach(function (o, i) {
      goodidsArray.push(o.id)
    })
    let goodids = goodidsArray.join(",")
    if (goodidsArray.length > 0) {
      let goodsInfo = await this.getGoodsByids(goodids, currentCom);
      if (goodsInfo && goodsInfo.isok) {
        vm.list = goodsInfo.msg;
        targetPage.vm_com_good[key] = vm;
        targetPage.$apply();
      }
    }
  },
  //产品列表组件
  getGoodsListRequest: async function (vm, currentCom, targetPage) {
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    this.showLoading() //加载圈
    let glistInfo = await this.getGoodList(vm, currentCom)
    if (glistInfo && glistInfo.isok) {
      vm.ispost = false;
      if (glistInfo.isok == 1) {
        vm.goods[vm.pageindex] = glistInfo.postdata.goodslist
        glistInfo.postdata.goodslist.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true
        targetPage.vm_goodsList = vm
        targetPage.$apply()
        wx.hideLoading(); //数据加载完毕隐藏
      }
    }
  },
  //产品列表筛选
  goodFifter: async function () {
    let aid = await core.getAid()
    return http.post(addr.GetExtTypes, {
      aid,
    })
  },
  //拼团组件
  joingroupRequest: async function (pageIndex, comIndex, currentCom, targetPage) {
    let [vm, ids] = [{},
    []
    ];
    let key = pageIndex + "_" + comIndex;
    currentCom.items.forEach(function (o, i) {
      ids.push(o.id)
    })
    if (ids.length > 0) {
      let data = await this.getGroupByIds(ids.join(","), currentCom);
      if (data) {
        vm.list = data.postdata;
        targetPage.vm_com_group[key] = vm;
        targetPage.$apply();
      }
    }
  },
  //砍价组价请求
  bargain: async function (pageIndex, comIndex, currentCom, targetPage) {
    let app = wepy.$instance
    let _goodids = [];
    let key = pageIndex + "_" + comIndex;
    currentCom.items.forEach(function (o, i) {
      _goodids.push(o.id)
    })
    let _postids = _goodids.join(",")
    if (_goodids.length > 0) {
      http.get(
        addr.GetBargainList, {
          appid: app.globalData.appid,
          ids: _postids,
        })
        .then(async function (data) {
          if (data.length > 0) {
            data.forEach(function (_cutprice_item) {
              _cutprice_item.startDateStr = _cutprice_item.startDateStr.replace(/-/g, '/');
              _cutprice_item.endDateStr = _cutprice_item.endDateStr.replace(/-/g, '/');
            });

            targetPage.vm_com_bargain[key] = data
            targetPage.$apply()

          }
        });
    }
  },
  // 砍价倒计时
  barCountDown: async function (data, targetPage) {
    //mark
    for (var j = data.length - 1; j >= 0; j--) {
      var dataItem = data[j]
      if (dataItem.RemainNum == 0) {
        dataItem.txt = "活动结束"
        dataItem.time = "00:00:00:00"
        dataItem.barImg = "http://j.vzan.cc/miniapp/img/enterprise/a43.png"
        dataItem.btn = "活动结束"
      } else {
        let starTime = await tools.getTimeSpan(dataItem.startDateStr);
        let endTime = await tools.getTimeSpan(dataItem.endDateStr);
        var timeFormat = "";
        if (starTime > 0) {
          dataItem.txt = "距离开始"
          dataItem.btn = "立即砍价"
          var timeFormatArray = await tools.formatMillisecond(starTime);
          timeFormat += timeFormatArray[0] + ":" + timeFormatArray[1] + ':' + timeFormatArray[2] + ':' + timeFormatArray[3];
          dataItem.time = timeFormat
          dataItem.barImg = "http://j.vzan.cc/miniapp/img/enterprise/a44.png"
        } else {
          if (endTime <= 0) {
            dataItem.txt = "活动结束"
            dataItem.time = "00:00:00:00"
            dataItem.btn = "活动结束"
            dataItem.barImg = "http://j.vzan.cc/miniapp/img/enterprise/a43.png"
          } else {
            dataItem.txt = "距离结束"
            dataItem.btn = "立即砍价"
            dataItem.barImg = "http://j.vzan.cc/miniapp/img/enterprise/a42.png"
            var timeFormatArray = await tools.formatMillisecond(endTime);
            timeFormat += timeFormatArray[0] + ":" + timeFormatArray[1] + ':' + timeFormatArray[2] + ':' + timeFormatArray[3];
            dataItem.time = timeFormat
          }
        }
      }

    }
    return data

  },
  //拼团2.0
  getEngroupIds: async function (pageindex, comindex, currentCom, targetPage) {
    let [vm, ids] = [{},
    []
    ];
    let key = pageindex + "_" + comindex;
    currentCom.items.forEach(function (o, i) {
      ids.push(o.id)
    });
    let groupId = ids.join(",")
    let aid = await this.getAid();
    if (ids.length > 0) {
      await http.get(
        addr.GetEntGroupByIds, {
          aid: aid,
          ids: groupId,
        })
        .then(function (data) {
          vm.list = data.postdata
          targetPage.vm_com_group2[key] = vm;
          targetPage.$apply();

        });
    }
  },
  /***************************表单 预约***********************************************/
  formRequest: async function (formdatajson, comename) {
    let listViewModal_form = {
      pageindex: 1,
      pagesize: 10,
      list: {},
      ispost: false,
      loadall: false,
    }
    let aid = await this.getAid();
    return new Promise(function (resolve, reject) {
      let app = wepy.$instance;
      let vm = listViewModal_form
      // 报名请求
      if (vm.ispost || vm.loadall)
        return;
      if (!vm.ispost)
        vm.ispost = true;
      http
        .post(
        addr.SaveUserForm, {
          uid: app.globalData.vipInfo.uid,
          formdatajson: formdatajson,
          aid: aid,
          comename: comename,
        })
        .then(function (data) {
          vm.ispost = false; //请求完毕，关闭请求开关
          resolve(data)
        })
    })
  },
  //提交表单
  submitForm: async function (formVm) {
    let aid = await this.getAid();
    let userInfo = await this.getUserInfo();
    http
      .post(
      addr.SaveSubscribeForm, {
        aid: aid,
        uid: userInfo.userid,
        formdatajson: formVm.datajson,
        remark: formVm.remark,
        formId: formVm.formId
      })
      .then(function (data) {
        if (data.isok) {
          core.wxToast("预约成功")
          setTimeout(res => {
            core.goBack(1)
          }, 2000)
        } else {
          core.showModalCancle(data.msg)
        }
      })
  },
  //预约列表
  subMore: async function (targetPage) {
    let vm = targetPage.vm_form
    let subList = await this.getStorage("subList")
    if (subList) {
      targetPage.vm_form = subList
      targetPage.$apply()
    } else {
      if (vm.ispost || vm.loadall)
        return;
      if (!vm.ispost)
        vm.ispost = true;
      this.showLoading()
      let subInfo = await this.subList(vm)
      vm.ispost = false;
      if (subInfo.isok) {
        //mark
        var len = subInfo.list.length
        len >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true
        len > 0 ? vm.list = vm.list.concat(subInfo.list) : ""
        for (var i = 0; i < len; i++) {
          var listItem = subInfo.list[i]
          listItem.formdatajson = (listItem.formdatajson || "").split(",")
          listItem.remark = JSON.parse(listItem.remark)
        }
        targetPage.vm_form = vm
        targetPage.$apply()
        this.setStorage("subList", vm)
      }
      wx.hideLoading()
    }
  },
  // 全部新闻
  allNews: async function (pageindex, comindex, currentCom, targetPage) {
    let vm_news = {};
    let key = pageindex + "_" + comindex;
    let id = currentCom.typeid;
    let allNewsInfo = await this.getNews(id, currentCom.liststyle);
    if (allNewsInfo && allNewsInfo.isok) {
      vm_news.list = allNewsInfo.data;
      currentCom.listmode == 'pick' && currentCom.list.length == 0 && currentCom.num > 0 ?
        vm_news.list = allNewsInfo.data.slice(0, currentCom.num) : "";
      // 时间戳转换
      vm_news.list.forEach(function (o, i) {
        o.addtime = core.ChangeDateFormat(o.addtime)
      })
      targetPage.vm_com_news[key] = vm_news;
      targetPage.$apply();
    }
  },
  // 选择新闻/
  chooseNews: async function (pageindex, comindex, currentCom, targetPage) {
    let [_newsid, viewmodel] = [
      [], {}
    ];
    let key = pageindex + "_" + comindex;
    currentCom.list.forEach(function (o, i) {
      _newsid.push(o.id)
    })
    let _newstids = _newsid.join(",");
    if (_newsid.length > 0) {
      let chooseNewsInfo = await this.getNewsByids(_newstids, currentCom.liststyle);
      if (chooseNewsInfo && chooseNewsInfo.isok && chooseNewsInfo.msg.length > 0) {
        viewmodel.list = chooseNewsInfo.msg.slice(0, currentCom.num);
        viewmodel.ids = _newstids;
        // 时间戳转换
        viewmodel.list.forEach(function (o, i) {
          o.addtime = core.ChangeDateFormat(o.addtime)
        })
        targetPage.vm_com_news[key] = viewmodel;
        targetPage.$apply();
      }
    }
  },
  /*************************** 产品详情页请求***********************************************/
  addShopCar: async function (para) {
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo();
    return http
      .post(addr.addGoodsCarData, {
        appId: app.globalData.appid,
        openid: userInfo.openId,
        goodid: para.pid,
        attrSpacStr: para.specId,
        SpecInfo: para.SpecInfo,
        qty: para.count,
        newCartRecord: para.record,
        isgroup: para.isgroup
      })
  },

  /***************************购物车请求***********************************************/
  shopCarList: async function (targetPage) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()
    let levelid = 0;
    let vipInfo = await this.getVipInfo();
    if (vipInfo) {
      levelid = vipInfo.levelid;
    }
    wx.showLoading({
      title: '加载中...',
      mask: true,
      success: function () {
        http
          .get(
          addr.getGoodsCarData_new, {
            appId: app.globalData.appid,
            openid: userInfo.openId,
            levelid: levelid,
          })
          .then(async function (data) {
            if (data.isok == 1) {
              //mark
              var len = Object.keys(data.postdata).length
              if (len) {
                for (let i = 0; i < len; i++) {
                  var postdataItem = data.postdata[i]
                  postdataItem.carIndex = false
                  if (postdataItem.goodsMsg.pickspecification) {
                    postdataItem.goodsMsg.pickspecification = JSON.parse(postdataItem.goodsMsg.pickspecification)
                    for (let j = 0, val; val = postdataItem.goodsMsg.pickspecification[j++];) {
                      for (let k = 0, key; key = val.items[k++];) {
                        key.sel = false
                      }
                    }
                  }
                }
              } else {
                data.postdata = []
              }
              targetPage.vm_shopList.list = data.postdata
              targetPage.$apply()
              wx.hideLoading()
            } else {
              await core.showModalCancle(data.msg)
              wx.hideLoading()
            }
          })
      }
    })
  },
  //function:0为编辑-1为删除
  update: async function (vm) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()
    return new Promise(function (resolve, rejce) {
      http.postJson(
        addr.updateOrDeleteGoodsCarData, {
          openid: userInfo.openId,
          appid: app.globalData.appid,
          function: vm.date,
          goodsCarModel: vm.model
        }
      ).then(data => {
        resolve(data)
      })
    })
  },
  //店铺配置
  getStoreConfig: async function () {
    let app = wepy.$instance;
    let storeConfig = ""
    if (app.globalData.storeConfig) {
      return app.globalData.storeConfig
    } else {
      storeConfig = await core.GetStoreInfo()
      if (storeConfig.isok) {
        app.globalData.storeConfig = storeConfig.postData.storeInfo
        return storeConfig.postData.storeInfo;
      }
    }
  },
  //下单
  addMinOrder: async function (vm) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()
    let aid = await this.getAid();
    return new Promise(function (resolve, reject) {
      http
        .post(addr.addMiniappGoodsOrder, {
          aid: aid,
          appId: app.globalData.appid,
          openid: userInfo.openId,
          goodCarIdStr: vm.carId,
          wxaddressjson: vm.address,
          orderjson: vm.order,
          buyMode: vm.buyMode,
          getWay: vm.getWay,
          isgroup: vm.isgroup,
          groupid: vm.groupid,
          goodtype: vm.goodtype,
          couponlogid: vm.couponlogid,
          salesManRecordId: vm.salesManRecordId || 0
        })
        .then(function (data) {
          resolve(data)
        })
    })
  },

  /***************************订单详情***********************************************/
  orderDtl: async function (orderId, targetPage) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()
    await http.get(
      addr.getMiniappGoodsOrderById, {
        appid: app.globalData.appid,
        openid: userInfo.openId,
        orderId: orderId
      }
    ).then(function (data) {
      if (data.isok == 1) {
        var vm = data.postdata
        //mark
        for (let i = 0, len = vm.goodOrderDtl.length; i < len; i++) {
          var goodOrderDtlItem = vm.goodOrderDtl[i]
          goodOrderDtlItem.ImgUrl = goodOrderDtlItem.goodImgUrl
          goodOrderDtlItem.Introduction = goodOrderDtlItem.goodname
          goodOrderDtlItem.SpecInfo = goodOrderDtlItem.orderDtl.SpecInfo
          goodOrderDtlItem.discountPrice = goodOrderDtlItem.orderDtl.priceStr
          goodOrderDtlItem.oldPrice = goodOrderDtlItem.orderDtl.originalPriceStr
          goodOrderDtlItem.Count = goodOrderDtlItem.orderDtl.Count
        }
        targetPage.vm_order = vm
        targetPage.$apply()
      }
    })
  },
  /***************************订单列表***********************************************/
  // 获取订单页面请求
  minOlt: async function (targetPage) {
    let vm = targetPage.vm_olt
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    await http
      .get(
      addr.getMiniappGoodsOrder, {
        appId: app.globalData.appid,
        openid: userInfo.openId,
        State: vm.state,
        pageindex: vm.pageindex,
        pagesize: vm.pagesize
      })
      .then(function (data) {
        vm.ispost = false; //请求完毕，关闭请求开关
        if (data.isok) {
          //更改状态数据
          if (data.postdata.length >= vm.pagesize) {
            vm.pageindex += 1;
          } else {
            vm.loadall = true;
          }
          if (data.postdata.length > 0) {
            vm.list = vm.list.concat(data.postdata);
          }
          targetPage.vm_olt = vm
          targetPage.condition = vm.state
          targetPage.$apply();
        }
      })
  },
  // 更改订单状态
  oltState: async function (vm) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()
    return new Promise(function (resolve, reject) {
      http.post(
        addr.updateMiniappGoodsOrderState, {
          appid: app.globalData.appid,
          openid: userInfo.openId,
          orderId: vm.orderId,
          State: vm.state
        }).then(function (data) {
          resolve(data)
        })
    })
  },
  /***************************砍价***********************************************/
  bargainDlt: async function (Id, buid, targetPage) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo();
    await http.get(
      addr.GetBargain, {
        UserId: userInfo.userid,
        appid: app.globalData.appid,
        Id: Id,
      })
      .then(function (data) {
        if (data.isok) {
          let singleprice = 0
          for (var i = 0, valKey; valKey = data.obj.BargainUserList[i++];) {
            if (valKey.UserId == userInfo.userid && valKey.State != 8) {
              singleprice = valKey.CurrentPriceStr
            }
          }
          let percent = ((Number(data.obj.OriginalPriceStr) - singleprice) / Number(data.obj.OriginalPriceStr)) * 100;
          if (percent > 100) {
            percent = 100;
          }
          if (buid == 0) {
            let findTemp = data.obj.BargainRecordUserList.find(f => f.BargainUserId == userInfo.userid)
            findTemp ? buid = findTemp.BUId : ""
          }
          data.haveCreatOrder == false && targetPage.shareId == 1 ? core.addBargain(data.obj.Id, buid, targetPage, 0) : targetPage.vm_bargain.isFriend = 0;
          //mark
          let vm_bargain = targetPage.vm_bargain
          vm_bargain.buid = buid
          vm_bargain.haveCreatOrder = data.haveCreatOrder
          vm_bargain.singleprice = singleprice
          vm_bargain.percent = percent
          vm_bargain.user = userInfo
          vm_bargain.list = data.obj
          targetPage.$apply()
        } else {
          core.showModalCancle(data.msg)
        }
      })
  },
  //获取砍价单列表
  bargainList: async function (targetPage) {
    let app = wepy.$instance;
    let vm = targetPage.vm_blt;
    let userInfo = await this.getUserInfo();

    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;

    await http.post(
      addr.GetBargainUserList, {
        appId: app.globalData.appid,
        UserId: userInfo.userid,
        pageIndex: vm.pageindex,
        pageSize: vm.pagesize,
        State: vm.state,
      }).then(data => {

        vm.ispost = false; //请求完毕，关闭请求开关
        if (data.isok) {
          for (let i = 0; i < data.obj.length; i++) {
            if (data.obj[i].GoodsFreightStr == '') {
              data.obj[i].GoodsFreightStr = '0.00'
            }
          }
          if (data.obj.length >= vm.pagesize) {
            vm.pageindex += 1;
          } else {
            vm.loadall = true;
          }
          if (data.obj.length > 0) {
            vm.list = vm.list.concat(data.obj);
          }
          targetPage.vm_blt = vm
          targetPage.condition = vm.state
          targetPage.$apply();
        }
      })
  },
  //查看砍价记录
  barShare: async function (buid, targetPage) {
    http.post(
      addr.GetBargainRecordList, {
        buid: buid,
        pageIndex: 1,
        pageSize: 100,
      }).then(data => {
        if (data.isok) {
          let barShare = data.obj
          for (var i = 0; i < barShare.length; i++) {
            barShare[i].CreateDate = this.ChangeDateFormat(barShare[i].CreateDate)
          }
          targetPage.barShare = barShare
          targetPage.$apply()
        } else {
          this.showModalCancle(data.msg)
        }
      })
  },
  // 申请砍价
  addBargain: async function (Id, buid, targetPage, click) {
    let userInfo = await this.getUserInfo();
    await http.post(
      addr.AddBargainUser, {
        Id: Id,
        UserId: userInfo.userid,
        UserName: userInfo.nickName
      }).then(function (data) {
        if (data.isok) {
          let cut_buid = 0
          buid == 0 ? cut_buid = data.buid : cut_buid = buid;
          targetPage.vm_bargain.buid = cut_buid
          core.cutPrice(cut_buid, Id, targetPage, click)
        }
      })
  },
  // 开始砍价
  cutPrice: async function (buid, Id, targetPage, click) {
    let userInfo = await this.getUserInfo();
    let selfShow = false
    //mark
    var vm_bargain = targetPage.vm_bargain
    await http.post(
      addr.cutprice, {
        UserId: userInfo.userid,
        buid: buid
      })
      .then(async function (data) {
        switch (data.code) {
          case -1:
            core.showModalCancle(data.msg)
            click == 1 ? vm_bargain.isFriend = 0 : vm_bargain.isFriend = 1
            if (vm_bargain.isFriend == 0) {
              await core.bargainDlt(Id, 0, targetPage)
            }
            break;
          case 0:
            let [timeArray, content] = [
              [], ""
            ]
            if (data.obj == 0) {
              content = '您已砍过,自砍倒计时1分钟！'
            } else {
              timeArray = JSON.stringify(data.obj).split(".")
              let mintues = 0
              let time = timeArray[0]
              if (timeArray.length == 1) {
                mintues = 0
              } else {
                mintues = parseInt(parseInt(timeArray[1]) * 0.6)
              }
              content = '您已砍过,' + time + '小时' + mintues + '分钟' + '之后才能继续自砍'
            }
            core.showModalCancle(content)
            vm_bargain.isFriend = 0
            vm_bargain.selfShow = false
            break;
          case 1:
            core.showModalCancle(data.msg)
            click == 1 ? vm_bargain.isFriend = 0 : vm_bargain.isFriend = 1
            if (vm_bargain.isFriend == 0) {
              await core.bargainDlt(Id, 0, targetPage)
            }
            break;
          case 2:
            vm_bargain.isFriend = data.isFriend
            vm_bargain.BargainedUserName = data.BargainedUserName
            vm_bargain.cutprice = data.cutprice
            vm_bargain.selfShow = !selfShow
            if (data.isFriend == 0) {
              await core.bargainDlt(Id, 0, targetPage)
            }
            break;

        }
        targetPage.$apply();

      })
  },
  // 获取默认地址
  getAddress: async function (targetPage) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo();
    http.get(
      addr.GetUserWxAddress, {
        appid: app.globalData.appid,
        userid: userInfo.userid
      }).then(data => {
        let address = data.obj.WxAddress.WxAddress
        address = JSON.parse(address)
        targetPage.vm_goods.address = address
        targetPage.$apply()
      })
  },
  // 砍价下单
  addBarOrder: async function (vm) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo();
    return new Promise(function (resolve, reject) {
      http.post(
        addr.AddBargainOrder, {
          AppId: app.globalData.appid,
          UserId: userInfo.userid,
          buid: vm.buid,
          address: vm.address,
          Remark: vm.Remark,
          PayType: vm.PayType
        }).then(data => {
          resolve(data)
        })
    })
  },
  //现价购买
  getBarPrice: async function (buid) {
    let userInfo = await this.getUserInfo();
    return new Promise(function (resolve, reject) {
      http.post(
        addr.GetBargainUser, {
          buid: buid,
          userid: userInfo.userid
        })
        .then(function (data) {
          resolve(data)
        })
    })
  },
  //砍价邀请分享
  getShare: async function (vm, targetPage) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()
    core.showLoading()
    await http.post(
      addr.GetShareCutPrice, {
        appId: app.globalData.appid,
        UserId: userInfo.userid,
        buid: vm.buid,
        bId: vm.bId,
      }).then(async function (data) {
        if (data.isok) {
          await core.barCanvas(data.qrcode, targetPage.vm_bargain.list.ImgUrl, targetPage)
        }
      })
  },
  //canvas
  barCanvas: async function (qrcode, Imgurl, targetPage) {
    let q1 = qrcode.replace(/http/, "https")
    let q2 = Imgurl.replace(/http/, "https")
    let q3 = 'https://j.vzan.cc/miniapp/img/enterprise/a38.png'
    let file1 = await core.downFile(q1);
    let file3 = await core.downFile(q3)
    if (file1.statusCode == 200) {
      let file2 = await core.downFile(q2);
      if (file2.statusCode == 200) {
        let windowWidth = wx.getSystemInfoSync().windowWidth
        let windowHeight = wx.getSystemInfoSync().windowHeight
        let context = wx.createCanvasContext('firstCanvas')
        let bgImg = file3.tempFilePath
        let objImgUrl = file2.tempFilePath
        let code = file1.tempFilePath
        let bottomprice = '最低' + targetPage.vm_bargain.list.FloorPriceStr + '元，原价' + targetPage.vm_bargain.list.OriginalPriceStr + '元'
        let bottomprice1 = '等你来砍能砍多少看你本事了'
        context.drawImage(bgImg, 0, 0, windowWidth * 0.87, windowHeight * 0.75); //大背景图
        context.drawImage(objImgUrl, windowWidth * 0.1, windowHeight * 0.1, windowWidth * 0.70, windowHeight * 0.22); //商品大图
        context.drawImage(code, windowWidth * 0.18, windowHeight * 0.50, windowWidth * 0.24, windowHeight * 0.16); //二维码
        context.setFontSize(13)
        context.setFillStyle('#fbb47b')
        context.fillText(bottomprice, windowWidth * 0.17, windowHeight * 0.45) //第一行文字
        context.fillText(bottomprice1, windowWidth * 0.17, windowHeight * 0.48) //第二行文字
        context.draw()
        wx.hideLoading()
      }
    }
  },
  //砍价订单详情
  getBarOlt: async function (buid, targetPage) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    await http.post(
      addr.GetOrderDetail, {
        AppId: app.globalData.appid,
        UserId: userInfo.userid,
        buid: buid
      }).then(data => {
        let vm = data.obj.OrderDetail
        switch (vm.State) {
          case 3:
            vm.State = -4
            break;
          case 5:
            vm.State = 0;
            break;
          case 6:
            vm.State = 2;
            break;
          case 7:
            vm.State = 1
            break;
          case 8:
            vm.State = 3
            break;
        }

        vm.AccepterName = vm.AddressUserName
        vm.AccepterTelePhone = vm.TelNumber
        vm.Address = vm.AddressDetail

        targetPage.vm_order.goodOrder = vm
        //mark
        var goodOrder = targetPage.vm_order.goodOrder

        goodOrder.QtyCount = 1 //数量
        goodOrder.OnlyGoodsMoney = vm.CurrentPriceStr //价格
        goodOrder.BuyPriceStr = (Number(vm.CurrentPriceStr).add(Number(vm.GoodsFreightStr))).toFixed(2) //价格
        goodOrder.OrderNum = vm.OrderId //订单号
        goodOrder.CreateDateStr = vm.CreateOrderTimeStr //下单时间
        goodOrder.BuyMode = vm.PayType //支付方式
        goodOrder.PayDateStr = vm.BuyTimeStr //支付时间
        goodOrder.DistributeDateStr = core.ChangeDateFormat(vm.SendGoodsTime) //发货时间：
        goodOrder.AcceptDateStr = core.ChangeDateFormat(vm.ConfirmReceiveGoodsTime) //成交时间
        goodOrder.GetWay = 1
        goodOrder.OrderId = vm.CityMordersId

        targetPage.vm_order.freightPrice = vm.GoodsFreightStr //运费
        targetPage.vm_order.goodOrderDtl = []
        targetPage.vm_order.goodOrderDtl.push({
          ImgUrl: vm.ImgUrl,
          Introduction: vm.BName,
          discountPrice: vm.CurrentPriceStr,
          discount: 100,
          Count: 1
        })

        targetPage.$apply()
      })
  },
  //确认收货
  confirmBar: async function (buid, targetPage, type) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    await http.post(addr.ConfirmReceive, {
      buid: buid,
      userid: userInfo.userid,
      appId: app.globalData.appid
    }).then(async data => {
      if (data.isok) {
        if (type == 0) {
          var vm_blt = targetPage.vm_blt
          vm_blt.pageindex = 1
          vm_blt.loadall = false
          vm_blt.ispost = false
          vm_blt.list = []
          await this.bargainList(targetPage)
        } else {
          await this.getBarOlt(buid, targetPage)
        }
        core.wxToast("收货成功")
      } else {
        core.showModalCancle(data.msg)
      }
    })
  },
  /****************************************** 我的页面*******************************************/
  updateWxCard: async function (targetPage) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()
    await http.post(
      addr.UpdateWxCard, {
        appid: app.globalData.appid,
        UserId: userInfo.userid,
        type: 2
      }).then(function (data) {
        if (data.msg == "还未生成会员卡(请到后台设置同步微信会员卡)") {
          return;
        } else {
          core.getWxCard(userInfo.userid, targetPage)
        }
      })
  },
  // 会员卡请求
  getWxCard: async function (UserId, targetPage) {
    let app = wepy.$instance;

    await http.get(
      addr.GetWxCardCode, {
        appid: app.globalData.appid,
        UserId: UserId,
        type: 2
      }).then(function (data) {
        let wxCard = false

        if (data.isok) {
          if (data.obj == null) {
            wxCard = true
          } else {
            wxCard = false
          }
        } else {
          wxCard = false
        }

        targetPage.vm_com_me.wxCard = wxCard
        targetPage.$apply()
      })
  },
  // 获取会员卡Sign(签名)
  getCardSign: async function () {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    return new Promise(function (resolve, reject) {
      http.get(
        addr.GetCardSign, {
          appid: app.globalData.appid,
          UserId: userInfo.userid,
          type: 2
        })
        .then(function (data) {
          resolve(data)
        })
    })
  },
  // 提交code到服务器
  saveWxCard: async function (code, targetPage) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    await http.post(
      addr.SaveWxCardCode, {
        appid: app.globalData.appid,
        UserId: userInfo.userid,
        code: code,
        type: 2
      })
      .then(function (data) {
        if (data.isok) {
          core.updateWxCard(targetPage)
        }
      })
  },
  /****************************************** 储值*******************************************/
  //获取储值余额
  getSaveUser: async function (targetPage) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    await http
      .get(
      addr.getSaveMoneySetUser, {
        appid: app.globalData.appid,
        openid: userInfo.openId
      })
      .then(function (data) {
        if (data.isok) {
          targetPage.vm_save.user.saveMoney = data.saveMoneySetUser.AccountMoneyStr
          targetPage.$apply();
        } else {
          core.showModalCancle(data.msg)
        }
      })
  },
  //充值列表
  getSaveList: async function (targetPage) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    await http.get(
      addr.getSaveMoneySetList, {
        appid: app.globalData.appid
      }).then(data => {
        if (data.isok) {
          targetPage.vm_save.price = data.saveMoneySetList;
          targetPage.vm_save.user = userInfo
          targetPage.$apply();
        } else {
          core.showModalCancle(data.msg)
        }
      })
  },
  //充值请求
  addSavePrice: async function (saveMoneySetId) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    return new Promise(function (resolve, reject) {
      http
        .post(
        addr.addSaveMoneySet, {
          appid: app.globalData.appid,
          openid: userInfo.openId,
          saveMoneySetId: saveMoneySetId
        }).then(function (data) {
          resolve(data)
        })
    })
  },
  // 历史充值列表
  getMoneyRec: async function (targetPage) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    wx.showLoading({
      title: '加载中',
      mask: true,
      success: function () {
        http.get(
          addr.getSaveMoneySetUserLogList, {
            appid: app.globalData.appid,
            openid: userInfo.openId,
          }).then(function (data) {
            if (data.isok == 1) {
              targetPage.vm_record = data.saveMoneyUserLogList
              targetPage.$apply();
            } else {
              core.showModalCancle(data.msg)
            }
            wx.hideLoading()
          })
      }
    })
  },
  /****************************************** 团购*******************************************/
  initGroupInfo: async function (groupId, targetPage) {
    if (!groupId) {
      await core.showModalCancle("团购不存在！")
      await core.goBack(1)
      return;
    }

    let groupInfo = await this.groupInfo(groupId)

    if (!groupInfo.isok) {
      await core.showModalCancle(groupInfo.msg)
      await core.goBack(1)
      return;
    }

    var _g = groupInfo.groupdetail;
    _g.slideimgs = []
    _g.slideimgs_fmt = []

    for (let i = 0; i < _g.ImgList.length; i++) {
      _g.slideimgs_fmt.push(_g.ImgList[i].thumbnail)
      _g.slideimgs.push(_g.ImgList[i].filepath)
    }

    //选取前5个用户
    if (_g.GroupUserList && _g.GroupUserList.length > 0) {
      _g.GroupUserList = _g.GroupUserList.slice(0, 5)
    }

    //选取两个可以参加的团
    if (_g.GroupSponsorList && _g.GroupSponsorList.length > 0) {
      _g.GroupSponsorList = _g.GroupSponsorList.slice(0, 2)
    }

    //转换富文本
    _g.content_fmt = wxParse('content_fmt', 'html', _g.Description || "", targetPage, 5);
    _g.ValidDateStart = _g.ValidDateStart.replace(/-/g, "/");
    _g.ValidDateEnd = _g.ValidDateEnd.replace(/-/g, "/");

    //保存
    targetPage.vm_group = _g
    targetPage.$apply()
  },
  groupTime: function (startDateStr, endDateStr) {
    let [groupstate, timeInter] = [0, "00:00:00"]
    let end = endDateStr.replace(/-/g, "/")
    let star = startDateStr.replace(/-/g, "/")
    let timeQueue = ['days', 'hours', 'min', 'seconds']
    let starTime = (new Date(star)) - (new Date()); //计算剩余的毫秒数
    let fromTheEnd_txt = ""

    if (starTime > 0) {
      timeQueue = tools.getRemainTimeQueue(starTime) //根据剩余毫秒数转换对应日期数组
      fromTheEnd_txt = "距离开始"
      groupstate = -1
    } else {
      let endTime = (new Date(end)) - (new Date()); //计算剩余的毫秒数

      if (endTime <= 0) {
        fromTheEnd_txt = "已结束"
        return {
          timeInter: "00:00:00",
          fromTheEnd_txt,
          groupstate: 0,
        };
      }

      timeQueue = tools.getRemainTimeQueue(endTime) //根据剩余毫秒数转换对应日期数组
      fromTheEnd_txt = "距离结束"
      groupstate = 1

    }

    timeInter = timeQueue[0] + "天" + timeQueue[1] + "时" + timeQueue[2] + "分" + timeQueue[3] + "秒"

    return {
      timeInter,
      groupstate,
      fromTheEnd_txt,
    }
  },
  initCountDown: async function (vm_group) {
    if (vm_group.GroupSponsorList != null) {
      var list = vm_group.GroupSponsorList;

      if (list.length > 0) {
        for (var i = list.length - 1; i >= 0; i--) {
          var timespan = await tools.getTimeSpan(list[i].ShowEndTime);

          if (timespan <= 0) {
            list.splice(i, 1)
          } else {
            var timeFormatArray = await tools.formatMillisecond(timespan);
            var timeFormat = "";

            timeFormat += timeFormatArray[1] + ':' + timeFormatArray[2] + ':' + timeFormatArray[3];
            list[i].countdown = timeFormat;
          }
        }
        return list
      }
    }
  },
  // 拼团下单
  addGroup: function (_Jsondata) {
    return http.post(addr.AddGroup, {
      Jsondata: JSON.stringify(_Jsondata)
    });
  },
  //取消支付
  canPay: function (guid) {
    let app = wepy.$instance
    http.post(
      addr.CancelPay, {
        guid: guid,
        appId: app.globalData.appid
      })
  },
  //支付成功
  paySuccess: function (_d) {
    let app = wepy.$instance
    return http.post(
      addr.GetPaySuccessGroupDetail, {
        appId: app.globalData.appid,
        gsid: _d.gsid,
        orderid: _d.orderid,
        paytype: _d.paytype,
      })
  },
  //订单详情
  getOlt: function (guid) {
    let app = wepy.$instance
    return http.get(
      addr.GetGroupOrderDetail, {
        appId: app.globalData.appid,
        guid: guid,
      })
  },
  // 拼团分享
  groupShare: function (group) {
    var _g = group;
    var _path = '/pages/group/groupInvite?id=' + _g.GroupSponsorId;
    var _title = `￥${_g.DiscountPrice / 100}元就能购买${_g.GroupName},一起来拼团吧！`;
    return {
      title: _title,
      path: _path,
      imageUrl: _g.ImgUrl,
      success: function (res) {
        // 转发成功
        core.wxToast("转发成功")
      },
    }
  },
  //拼团列表
  getGroupList: async function (targetPage) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()
    let vm = targetPage.vm

    if (vm.ispost || vm.loadall)
      return;

    if (!vm.ispost)
      vm.ispost = true;

    await http.post(
      addr.GetMyGroupList, {
        appId: app.globalData.appid,
        userId: userInfo.userid,
        t: vm.state,
        pageIndex: vm.pageindex
      }).then(data => {
        if (data.isok) {
          let group = {}
          vm.ispost = false;

          for (let i = 0; i < data.postdata.length; i++) {
            data.postdata[i].ShowEndTime = data.postdata[i].ShowDate
          }

          setInterval(async res => {
            group.GroupSponsorList = data.postdata
            core.initCountDown(group)
            targetPage.$apply()
          }, 1000)

          targetPage.vm.list[vm.pageindex] = data.postdata

          if (data.postdata.length < vm.pagesize) {
            vm.loadall = true;
          } else {
            vm.pageindex += 1;
          }

          targetPage.$apply()
        } else {
          core.showModalCancle(data.msg)
        }
      })
  },
  //参团详情
  myGroupDlt: async function (id) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    return http.post(
      addr.GetMyGroupDetail, {
        appId: app.globalData.appid,
        userId: userInfo.userid,
        groupsponId: id,
      })
  },
  groupInvite: async function (id, targetPage) {
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo()

    await http.post(
      addr.GetInvitePageData, {
        appId: app.globalData.appid,
        gsid: id,
      }).then(data => {
        if (data.isok) {
          var isingroup = false;
          let _g = data.postdata
          _g.content_fmt = wxParse('content_fmt', 'html', _g.Description || "", targetPage, 5);

          if (_g.GroupUserList.length > 0) {
            var obj = _g.GroupUserList.find(function (item) {
              return item.Id == userInfo.userid
            });
            isingroup = (obj == undefined ? false : true);
          }

          if (_g.GroupUserList.length >= 4) {
            _g.GroupUserList = _g.GroupUserList.slice(0, 4);
            _g.NeedNum_fmt = 0;
          } else {
            if (_g.NeedNum + _g.GroupUserList.length <= 4) {
              _g.NeedNum_fmt = _g.NeedNum;
            } else {
              _g.NeedNum_fmt = 4 - _g.GroupUserList.length;
            }
          }

          targetPage.isingroup = isingroup
          targetPage.vm_dlt = _g
          targetPage.$apply()

          let that = targetPage
          that.cutDown = setInterval(async res => {
            let start = _g.ValidDateStart
            let end = _g.ValidDateEnd
            that.time = await core.groupTime(start, end)
            that.$apply()
          }, 1000)
        } else {
          core.showModalCancle(data.msg)
        }
      })
  },
  //进行中的拼团列表
  groupIng: async function (targetPage) {
    let app = wepy.$instance
    let vm = targetPage.vm

    if (vm.ispost || vm.loadall)
      return;

    if (!vm.ispost)
      vm.ispost = true;

    await http.post(
      addr.GetGroupList, {
        appId: app.globalData.appid,
        state: vm.state,
        pageIndex: vm.pageindex
      }).then(data => {
        if (data.isok) {
          vm.ispost = false;

          targetPage.vm.list[vm.pageindex] = data.postdata
          data.postdata.length < vm.pagesize ? vm.loadall = true : vm.pageindex += 1
          targetPage.$apply()
        } else {
          core.showModalCancle(data.msg)
        }
      })
  },
  /***********************************************拼团2.0**************************************************************/
  getMinOrderId: async function (id) {
    let app = wepy.$instance;
    let userInfo = await this.getUserInfo()

    return http.get(
      addr.getMiniappGoodsOrderById, {
        appid: app.globalData.appid,
        openid: userInfo.openId,
        orderId: id
      })
  },
  //拼团2.0列表
  getEntGroup: async function (targetPage) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()
    let vm = targetPage.vm

    if (vm.ispost || vm.loadall)
      return;

    if (!vm.ispost)
      vm.ispost = true;

    await http.post(
      addr.GetMyGroupList2, {
        appId: app.globalData.appid,
        userId: userInfo.userid,
        state: vm.state,
        pageIndex: vm.pageindex
      }).then(data => {
        if (data.isok == 1) {
          vm.ispost = false;
          if (data.postdata != null) {
            //mark
            for (var i = 0, len = data.postdata.length; i < len; i++) {

              var postdataItem = data.postdata[i]

              if (postdataItem.state == 3) {
                postdataItem.state = 4
              }
              if (postdataItem.groupstate == 2 && postdataItem.state == 1) {
                postdataItem.state = 5
              }
              if (postdataItem.groupstate == -4 && postdataItem.state == -4) {
                postdataItem.state = 1
              }
              if (postdataItem.groupstate == 0 && postdataItem.state == -1) {
                postdataItem.state = 6
              }
              if (postdataItem.groupstate == 1 && postdataItem.state == -1) {
                postdataItem.state = 6
              }
              if (postdataItem.groupstate == 1 && postdataItem.state == 8) {
                postdataItem.state = 1
              }
              if (postdataItem.groupstate == 2 && postdataItem.state == 8) {
                postdataItem.state = 5
              }
            }
            targetPage.vm.list[vm.pageindex] = data.postdata
            data.postdata.length < vm.pagesize ? vm.loadall = true : vm.pageindex += 1
          } else {
            vm.loadall = true
            targetPage.vm.list[vm.pageindex] = null
          }
          targetPage.$apply()
        } else {
          core.showModalCancle(data.msg)
        }
      })
  },
  //更改状态
  async groupConfrim(vm) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()

    return http.post(
      addr.updateMiniappGoodsOrderState, {
        appid: app.globalData.appid,
        openid: userInfo.openId,
        orderId: vm.Id,
        State: vm.state
      })
  },
  // 拼团分享
  group2Share: function (vm) {
    var _path = '/pages/group2/group2Share?id=' + vm.id;
    var _title = vm.name;
    var _img = vm.img;

    return {
      title: _title,
      path: _path,
      imageUrl: _img,
      success: function (res) {
        // 转发成功
        core.wxToast("转发成功")
      },
    }
  },
  /***********************************************收货地址选择**************************************************************/
  getAddresslt: async function (targetPage) {
    let userInfo = await this.getUserInfo()
    http.post(
      addr.GetUserAddress, {
        userId: userInfo.userid
      }).then(data => {
        if (data.isok) {
          let _g = data.data
          for (let i = 0; i < _g.length; i++) {
            _g[i].address = _g[i].province + _g[i].city + _g[i].district + _g[i].street
          }
          targetPage.vm_addr = _g
          targetPage.$apply()
        } else {
          core.showModalCancle(data.msg)
        }
      })
  },
  //删除
  delAddress: function (id) {
    return http.post(
      addr.DeleteUserAddress, {
        id: id
      })
  },
  //保存
  saveAddress: async function (vm) {
    let userInfo = await this.getUserInfo()

    return http.post(
      addr.EditUserAddress, {
        id: vm.id,
        userid: userInfo.userid,
        isdefault: vm.isdefault,
        contact: vm.contact,
        phone: vm.phone,
        province: vm.province,
        city: vm.city,
        district: vm.district,
        street: vm.street,
      })

  },
  //运费模板
  getFreight: async function (vm) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()

    return http.post(
      addr.GetFreightFee, {
        appId: app.globalData.appid,
        openId: userInfo.openId,
        goodCartIds: vm.cartid,
        province: vm.province,
        city: vm.city,
      })
  },
  /**************************************直播间*********************************************/
  getLive: function (url) {
    return new Promise(function (resolve, reject) {
      let result = /https?:\/\/vzan.com\/live\/tvchat-(\d+).*/gi.exec(url);

      if (!result) {
        core.showModalCancle("直播地址不正确")
        return;
      }

      let tpid = result[1];

      http.post(
        addr.live, {
          tpid: tpid
        }).then(data => {
          resolve(data)
        })
    })
  },
  /**************************************优惠券*********************************************/
  getCoup: async function (vm) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(
      addr.GetMyCouponList, {
        appId: app.globalData.appid,
        userId: userInfo.userid,
        pageIndex: vm.pageindex,
        state: vm.state,
        goodsId: vm.goodsId,
        goodsInfo: vm.goodsInfo,
      })
  },
  // 领取优惠券
  getCoupon: async function (id) {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()

    return http.post(addr.GetCoupon, {
      appId: app.globalData.appid,
      userId: userInfo.userid,
      couponId: id,
    })
  },
  //领券中心
  getStoreCoup: async function () {
    let app = wepy.$instance
    let userInfo = await this.getUserInfo()

    return http.post(
      addr.GetStoreCouponList, {
        appId: app.globalData.appid,
        goodstype: -1,
        userId: userInfo.userid
      })
  },
  /**************************************立减金*********************************************/
  getReduction: async function (vm) {
    let userInfo = await this.getUserInfo()

    return http.post(
      addr.GetReductionCard, {
        userId: userInfo.userid,
        openId: userInfo.openId,
        orderId: vm.orderid,
        couponsId: vm.couponsid
      })
  },
  getReductionLst: async function () {
    let aid = await core.getAid();
    let userInfo = await core.getUserInfo()
    let store = await core.getStoreConfig()

    return http.post(
      addr.GetReductionCardList, {
        userId: userInfo.userid,
        openId: userInfo.openId,
        aid,
        storeId: store.Id,
      })
  },
  /**************************************积分商城*********************************************/
  getInterInfo: async function () {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(
      addr.GetUserIntegral, {
        appId: app.globalData.appid,
        userId: userInfo.userid
      })
  },
  // 积分商品列表
  interPro: async function (vm) {
    let app = wepy.$instance

    return http.post(addr.GetExchangeActivityList, {
      appId: app.globalData.appid,
      type: vm.type,
      pageindex: vm.pageindex,
      pagesize: vm.pagesize
    })
  },
  //积分规则
  interRule: async function () {
    let app = wepy.$instance

    return http.post(
      addr.GetStoreRules, {
        appId: app.globalData.appid,
      })
  },
  //积分记录
  interRecord: async function (vm) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(addr.GetUserIntegralLogs, {
      appId: app.globalData.appid,
      userId: userInfo.userid,
      pageindex: vm.pageindex,
      pagesize: vm.pagesize,
    })
  },
  //详情
  interDlt: async function (id) {
    let app = wepy.$instance

    return http.post(addr.GetExchangeActivity, {
      appId: app.globalData.appid,
      id: id
    })
  },
  //积分下单
  interOrder: async function (vm) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(addr.AddExchangeActivityOrder, {
      userId: userInfo.userid,
      appId: app.globalData.appid,
      activityId: vm.id,
      address: vm.address
    })
  },
  //订单详情
  interLst: async function (vm) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(addr.GetExchangeActivityOrders, {
      userId: userInfo.userid,
      appId: app.globalData.appid,
      pageindex: vm.pageindex,
      pagesize: vm.pagesize,
    })
  },
  // 积分确认收货
  interConfirm: async function (id) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(addr.ConfirmReciveGood, {
      userId: userInfo.userid,
      appId: app.globalData.appid,
      orderId: id
    })
  },
  /**********************************************分销中心***************************************************** */
  getMiniSale: async function () {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.get(addr.GetMiniAppSaleManConfig, {
      UserId: userInfo.userid,
      appId: app.globalData.appid,
    })
  },
  postApply: async function (phone) { //申请成为分销员
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(
      addr.ApplySalesman, {
        UserId: userInfo.userid,
        appId: app.globalData.appid,
        TelePhone: phone
      })
  },
  getSaleInfo: async function () {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.get(addr.GetSalesManUserInfo, {
      UserId: userInfo.userid,
      appId: app.globalData.appid,
    })
  },
  // 推广产品
  getSaleLst: function (vm) {
    let app = wepy.$instance

    return http.get(addr.GetSalesmanGoodsList, {
      appId: app.globalData.appid,
      goodsName: vm.search,
      sortType: vm.state,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
    })
  },
  // 累计订单
  getSaleRecord: async function (vm) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.get(addr.GetSalesManRecordOrder, {
      appId: app.globalData.appid,
      UserId: userInfo.userid,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
    })
  },
  // 累计客户
  getSaleManRecord: async function (vm) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.get(addr.GetSalesManRecordUser, {
      appId: app.globalData.appid,
      UserId: userInfo.userid,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
      state: vm.state
    })
  },
  // 绑定分销关系Id
  bindSale: function (goodsid) {
    let app = wepy.$instance

    return http.get(addr.GetSalesManRecord, {
      appId: app.globalData.appid,
      goodsId: goodsid,
      salesManId: app.globalData.saleId,
    })
  },
  // 分享绑定
  bindShip: async function (goodsId, record) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(addr.BindRelationShip, {
      appId: app.globalData.appid,
      userId: userInfo.userid,
      goodsId: goodsId,
      salesManRecordId: record,
    })
  },
  //更新推广分享记录状态 默认更新为可用 state=1
  updateRecordId: function (salesManRecordId) {
    let app = wepy.$instance
    http.post(
      addr.UpdateSalesManRecord, {
        appId: app.globalData.appid,
        salesManRecordId: salesManRecordId,
        state: 1,
      })
  },
  applyCash: async function (drawCashMoney) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(addr.DrawCashApply, {
      appId: app.globalData.appid,
      UserId: userInfo.userid,
      drawCashMoney,
    })
  },
  cashRecordlst: async function (vm) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.get(addr.GetDrawCashApplyList, {
      appId: app.globalData.appid,
      UserId: userInfo.userid,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
    })
  },

  //储值支付
  payByStore: async function (pickCoupon, paymoney, calmoney, money_coupon, money_vip) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let vipinfo = await core.getVipInfo()

    return http.post(addr.PayByStoredvalue, {
      appid: app.globalData.appid,
      userId: userInfo.userid,
      openId: userInfo.openId,
      levelid: vipinfo.levelid,
      couponid: pickCoupon == null ? 0 : pickCoupon.Id,
      money: paymoney * 100, //输入金额
      money_cal: calmoney, //支付金额
      money_coupon: money_coupon,
      money_vip: money_vip,
    })
  },
  // 储值支付成功
  payByStoreSuccess: async function (orderid) {
    let userInfo = await core.getUserInfo()

    return http.post(addr.StoredvalueOrderInfo, {
      orderid: orderid,
      openId: userInfo.openId,
    })
  },
  /************************************************私信************************************************** */
  connectSocket: async function () {
    var that = this;
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    var globaldata = app.globalData;
    var appid = app.globalData.appid || "";
    var fuserid = userInfo.userid || ""

    if (appid == "" || fuserid == "")
      return;

    if (globaldata.ws || isConnecting)
      return;

    isConnecting = true;

    wx.connectSocket({
      //fuserType：用户身份  0：普通用户 2：商家
      url: 'wss://dzwss.xiaochengxu.com.cn/?appId=' + appid + '&userId=' + fuserid + '&isFirst=' + isFirst,
      header: {
        'content-type': 'application/json'
      },
      method: "GET"
    });

    console.log("ws connecting...");

    wx.onSocketOpen(function (res) {
      console.log('ws is open', res);
      globaldata.ws = true;
      isConnecting = false;

      if (reConnectTimer) {
        clearTimeout(reConnectTimer);
        reConnectTimer = null;
      }

      //重连后，自动重发发送失败的消息
      for (var i = 0; i < globaldata.msgQueue.length; i++) {
        that.sendMessage(globaldata.msgQueue[i])
      }

      globaldata.msgQueue = [];
    });

    wx.onSocketError(function (res) {
      console.log('WebSocket连接打开失败，请检查！', res)
      globaldata.ws = false;
      isConnecting = false;
    });

    wx.onSocketClose(function (res) {
      isFirst = false;
      console.log('WebSocket 已关闭！', res)
      globaldata.ws = false;
      isConnecting = false;
      core.reConnect();
    });

    //接收消息
    wx.onSocketMessage(function (res) {
      console.log('收到服务器内容：' + res.data)
      var msg = res.data;

      if (typeof res.data == "string")
        msg = JSON.parse(res.data);

      //判断当前在哪个页面
      var pages = getCurrentPages();
      var currentPage = pages[pages.length - 1];
      var fuser = currentPage.data.fuserInfo;
      var tuser = currentPage.data.tuserInfo;

      //聊天页面
      if (currentPage.route == "pages/im/chat") {
        var list = currentPage.data.vm.list;

        //如果消息是当前联系人发来的
        if (msg.fuserId == fuser.userid && msg.tuserId == tuser.userid || //我发的
          msg.fuserId == tuser.userid && msg.tuserId == fuser.userid) { //发给我的

          list.push(msg);
          currentPage.setData({
            "vm.list": list,
            "vm.lastids": msg.ids,
          });
        } else {
          core.markUnreadMsg(msg);
        }
      }
      //联系人页面
      else if (currentPage.route == "pages/im/contact") {
        core.markUnreadMsg(msg);
      } else {
        var key = msg.fuserId + "_" + msg.tuserId + "_" + msg.tuserType //技师给用户发的
        var currentUnreadmsgcount = _get(globaldata.unreadmsg, key, 0);
        currentUnreadmsgcount += 1;
        globaldata.unreadmsg[key] = currentUnreadmsgcount;

        var unreadmsgcount = 0;
        for (var item in globaldata.unreadmsg) {
          unreadmsgcount += globaldata.unreadmsg[item]
        }

        core.changeunreadmsg(globaldata.unreadmsg, globaldata.unreadmsgcount);
      }

    })
  },
  reConnect: function () {
    console.log("开始重连");
    if (reConnectTimer) {
      clearTimeout(reConnectTimer);
      reConnectTimer = null;
    }
    reConnectTimer = setTimeout(function () {
      core.connectSocket();
    }, 3000);
  },
  //发
  sendMessage: function (msg) {
    let app = wepy.$instance
    if (typeof msg == "object")
      msg = JSON.stringify(msg);
    var globaldata = app.globalData;
    if (globaldata.ws) {
      wx.sendSocketMessage({
        data: msg
      })
    } else {
      app.globalData.msgQueue.push(msg);
    }
  },
  //只标记联系人列表里的未读消息
  markUnreadMsg: function (msg) {
    var that = this;
    let app = wepy.$instance
    var pages = getCurrentPages();
    var currentPage = getCurrentPages().find(p => p.route == "pages/im/contact");

    if (currentPage != null) {
      var list = currentPage.data.vm.list;

      //查找给我发消息的那个人
      var contactIndex = list.findIndex(function (obj) {
        return msg.fuserId == obj.tuserId;
      });

      if (contactIndex != -1) {
        list[contactIndex].message = {
          msgType: msg.msgType,
          msg: msg.msgType == 1 ? "[图片]" : msg.msg,
          sendDate: msg.sendDate,
        };
        var unreadmsgItem = _get(list[contactIndex], "unreadnum", 0);
        unreadmsgItem += 1;
        list[contactIndex].unreadnum = unreadmsgItem;
        if (unreadmsgItem > 99) {
          list[contactIndex].unreadnum_fmt = "99+";
        } else {
          list[contactIndex].unreadnum_fmt = unreadmsgItem;
        }
        currentPage.setData({
          "vm.list": list
        });
      }
    }

    var key = msg.fuserId + "_" + msg.tuserId + "_" + msg.tuserType //技师给用户发的
    var currentUnreadmsgcount = _get(app.globalData.unreadmsg, key, 0);
    currentUnreadmsgcount += 1;
    app.globalData.unreadmsg[key] = currentUnreadmsgcount;

    var unreadmsgcount = 0;

    for (var item in app.globalData.unreadmsg) {
      unreadmsgcount += app.globalData.unreadmsg[item]
    }


    core.changeunreadmsg(that.globalData.unreadmsg, unreadmsgcount);
  },
  changeunreadmsg: function (unreadmsg, unreadmsgcount) {
    var app = wepy.$instance
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
  getContactList: async function (vm) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    return http.post(addr.GetContactList, {
      appId: app.globalData.appid,
      fuserId: userInfo.userid,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
      fuserType: 0,
      ver: 1,
    })
  },
  AddContact: async function (userid) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    http.post(addr.AddContact, {
      appId: app.globalData.appid,
      fuserId: userInfo.userid,
      tuserId: userid
    })
  },
  getHistory: async function (userid, vm, targetPage) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()

    if (vm.ispost || vm.loadall)
      return;

    vm.ispost = true;
    http.post(addr.GetHistory, {
      appId: app.globalData.appid,
      fuserId: userInfo.userid,
      tuserId: userid,
      id: vm.lastid,
      fuserType: 0,
      ver: 1,
    }).then(res => {
      if (res && res.isok) {
        res.data.length < vm.pagesize ? vm.loadall = true : vm.loadall = false;
        if (res.data.length > 0) {
          vm.list = res.data.concat(vm.list);
          if (vm.lastid === 0) {
            vm.lastid = vm.list[0].Id;
            vm.lastids = vm.list[vm.list.length - 1].ids;
          } else {
            vm.lastid = vm.list[0].Id;
            vm.lastids = vm.list[0].ids;
          }
        }
      } else {
        vm.loadall = true
      }
      vm.ispost = false;
      targetPage.vm = vm
      targetPage.$apply()

    })
  },
  gochat: async function () {
    let store = await core.getStoreConfig()
    if (store && store.funJoinModel) {
      if (store.kfInfo && store.funJoinModel.imSwitch) {
        var userid = store.kfInfo.uid;
        var nickname = (store.kfInfo.nickName || "").replace(/\s/gi, "");
        var headimg = store.kfInfo.headImgUrl;
        wx.navigateTo({
          url: '/pages/im/chat?userid=' + userid + "&nickname=" + nickname + "&headimg=" + headimg,
        })
      } else {
        core.showModalCancle('商家已关闭在线客服')
      }
    }
  },
  /************************************************排队取号************************************************** */
  PutSortQueueMsg: async function (phone) {
    let aId = await core.getAid()
    let store = await core.getStoreConfig()
    let userInfo = await core.getUserInfo()
    let app = wepy.$instance

    return http.post(addr.PutSortQueueMsg, {
      appid: app.globalData.appid,
      aId,
      storeId: store.Id,
      userId: userInfo.userid,
      pCount: 0,
      telePhone: phone,
      pageType: 22,
    })
  },
  // 是否开启排队
  GetUserInSortQueuesPlanMsg: async function (targetPage) {
    let userInfo = await core.getUserInfo()
    let app = wepy.$instance
    let aId = await core.getAid()
    let store = await core.getStoreConfig()

    http.post(addr.GetUserInSortQueuesPlanMsg, {
      appid: app.globalData.appid,
      aId,
      storeId: store.Id,
      userId: userInfo.userid
    }).then(data => {
      if (data.isok == true) {
        data.code == 0 ? targetPage.isonOrder = false : targetPage.isonOrder = true
        data.dataObj.address = store.Address
        targetPage.dataObj = data.dataObj
        data.code > 0 ? targetPage.numsindex = data.dataObj.sortQueue.pCount : targetPage.numsindex = 0
        targetPage.$apply()
      }
    })
  },
  CancelSortQueue: async function (sortId) {
    let app = wepy.$instance
    let aId = await core.getAid()
    let store = await core.getStoreConfig()

    return http.post(addr.CancelSortQueue, {
      aId,
      appid: app.globalData.appid,
      storeId: store.Id,
      sortId: sortId
    })
  },

  /***********************************功能开关************************************** */
  appSwitch: function () {
    let appid = wepy.$instance.globalData.appid
    return http.get(addr.GetFunctionList, {
      appid,
    })
  },
  // 底部水印
  logoSwitch: async function (targetPage) {
    let appid = wepy.$instance.globalData.appid;
    let logo = await core.getStorage("logo");
    if (logo) {
      targetPage.vm_com_logo = logo
      targetPage.$apply()
    } else {
      http.get(addr.GetAgentConfigInfo, {
        appid,
      }).then(async data => {
        if (data.isok == 1) {
          if (data.AgentConfig.isdefaul == 0) {
            data.AgentConfig.LogoText = data.AgentConfig.LogoText.split(' ')
          } else {
            data.AgentConfig.LogoText = data.AgentConfig.LogoText
          }
          targetPage.vm_com_logo = data.AgentConfig
          targetPage.$apply()
          await core.setStorage("logo", data.AgentConfig)
        } else {
          core.showModalCancle(data.msg)
        }
      })
    }
  },
  /***********************************电商************************************** */
  GetGoodsList: async function (vm) {
    let app = wepy.$instance
    let vipInfo = await this.getVipInfo();
    return http.get(addr.GetGoodsList, {
      levelid: vipInfo.levelid,
      appid: app.globalData.appid,
      typeid: vm.typeid,
      pageindex: vm.pageindex,
      pagesize: 10,
      orderbyid: 0,
    })
  },
  GetHomeConfig: async function (vm) {
    return new Promise(function (resolve, reject) {
      let urlArray = []
      let app = wepy.$instance
      http.get(addr.GetHomeConfig, {
        appid: app.globalData.appid,
      }).then(data => {
        if (data.isok > 0) {
          let typelist = data.postdata.typelist;
          let j = Math.ceil(typelist.length / 10);
          let swiperImgarray = data.postdata.dataimgs;
          let swiperTypearray = [];
          let item = [];
          for (var z = 1; z < j; z++) {
            for (var i = 0; i <= typelist.length; i++) {
              item.push(typelist[i]);
              if (i == z * 9 || i == typelist.length) {
                swiperTypearray.push(item);
                item = [];
              }
            }
          }
          urlArray.swiperImgarray = swiperImgarray
          urlArray.swiperTypearray = swiperTypearray
          resolve(urlArray)
        }
      })
    })
  },
  GetStoreConfig: async function (vm) {
    let app = wepy.$instance
    if (!wx.getStorageSync('store')) {
      http.get(addr.GetStoreConfig, {
        appid: app.globalData.appid,
      }).then(data => {
        if (data.isok > 0) {
          wx.setStorageSync('store', data.postdata.store);
        }
      })
    }
  },
  GetGroupListPage: async function (vm) {
    let app = wepy.$instance
    return http.get(addr.GetGroupListPage, {
      appid: app.globalData.appid,
    })
  },
};

module.exports = {
  http,
  core,
  pay,
  canvas,
  tools,
}
