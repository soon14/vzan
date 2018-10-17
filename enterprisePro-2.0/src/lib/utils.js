import _get from './lodash.get';
const skin = [{
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
  } catch (e) {}
  try {
    m += s2.split(".")[1].length
  } catch (e) {}
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
  } catch (e) {}
  try {
    t2 = arg2.toString().split(".")[1].length
  } catch (e) {}
  r1 = Number(arg1.toString().replace(".", ""))
  r2 = Number(arg2.toString().replace(".", ""))
  return (r1 / r2) * Math.pow(10, t2 - t1);
}
Number.prototype.div = function (arg) {
  return accDiv(this, arg);
}
/**
 * @param {请求}
 */
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
/**
 * @param {时间转换}
 */
const timeTools = {
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
    if (timeTools.getType(time) == timeTools.typeEnum.string) {
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
    var len = String(time).length
    var sliceField = len > 2 ? len : 2
    return ('0' + time).slice(-sliceField)
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
  formTimeSpan() {
    let nowDate = new Date()
    let _g = {
      years: [],
      months: this.timeSpan(12),
      days: this.timeSpan(31),
      hours: this.timeSpan(23),
      mins: this.timeSpan(60),
      nowYear: nowDate.getFullYear(),
      nowMonth: nowDate.getMonth() + 1,
      nowDay: nowDate.getDate(),
      nowHour: nowDate.getHours(),
      nowMin: nowDate.getMinutes(),
    }
    for (let i = 1990, len = nowDate.getFullYear() + 1; i <= len; i++) {
      _g.years.push(i);
    }
    // 未满足大于10时添加0
    _g.nowMonth = this.timeAdd(_g.nowMonth)
    _g.nowDay = this.timeAdd(_g.nowDay)
    _g.nowHour = this.timeAdd(_g.nowHour)
    _g.nowMin = this.timeAdd(_g.nowMin)
    // indexOf寻找当前时间下标
    let value = [
      _g.years.indexOf(_g.nowYear),
      _g.months.indexOf(_g.nowMonth),
      _g.days.indexOf(_g.nowDay),
      _g.hours.indexOf(_g.nowHour),
      _g.mins.indexOf(_g.nowMin)
    ];
    let vm = {
      value,
      years: _g.years,
      months: _g.months,
      days: _g.days,
      hours: _g.hours,
      mins: _g.mins,
    };
    return vm;
  },
  //赋值
  timeSpan(num) {
    let data = []
    for (let i = 0; i <= num; i++) {
      i < 10 ? i = "0" + i : ""
      data.push(i);
    }
    return data
  },
  //补零
  timeAdd(time) {
    time < 10 ? time = "0" + time : time = time
    return time
  }
};
const canvasTools = {
  async pathStatus(_g, w, h) {
    let vm = {}
    vm.status = _g.StyleType
    switch (vm.status) {
      case 0:
        vm.bg = await this.downFile("https://wtapi.vzan.com/img/share1.png")
        vm.xstore = w * 0.045;
        vm.ystore = h * 0.031;
        vm.wstore = w * 0.91;
        vm.hstore = w * 0.97;


        vm.xng = w * 0.045;
        vm.yng = h * 0.6;
        vm.wng = w * 0.91;
        vm.hng = w * 0.18;

        vm.xname = w * 0.1;
        vm.yname = h * 0.65;

        vm.xcon = w * 0.1;
        vm.ycon = h * 0.7;

        vm.xqrcode = w * 0.15;
        vm.yqrcode = h * 0.75;
        vm.wqrcode = w * 0.23;
        vm.hqrcode = w * 0.23;

        vm.xtxt = w * 0.19;
        vm.ytxt = h * 0.95;
        vm.xtxt1 = w * 0.65;
        vm.ytxt1 = h * 0.95;


        break;
      case 1:
        vm.bg = await this.downFile("https://wtapi.vzan.com/img/share2.png")
        vm.xicon = w * 0.4;
        vm.yicon = 0;
        vm.wicon = w * 0.2;
        vm.hicon = w * 0.2;

        vm.xstore = w * 0.11;
        vm.ystore = h * 0.17;
        vm.wstore = w * 0.78;
        vm.hstore = w * 0.78;

        vm.xng = w * 0.11;
        vm.yng = h * 0.63;
        vm.wng = w * 0.78;
        vm.hng = w * 0.14;

        vm.xname = w * 0.15;
        vm.yname = h * 0.67;

        vm.xcon = w * 0.15;
        vm.ycon = h * 0.71;

        vm.xqrcode = w * 0.15;
        vm.yqrcode = h * 0.75;
        vm.wqrcode = w * 0.23;
        vm.hqrcode = w * 0.23;

        vm.xtxt = w * 0.26;
        vm.ytxt = h * 0.95;
        vm.xtxt1 = w * 0.72;
        vm.ytxt1 = h * 0.95;

        break;

      case 2:
        vm.bg = await this.downFile("https://wtapi.vzan.com/img/share3.png")
        vm.xstore = w * 0.14;
        vm.ystore = h * 0.10;
        vm.wstore = w * 0.72;
        vm.hstore = w * 0.72;

        vm.xng = w * 0.14;
        vm.yng = h * 0.485;
        vm.wng = w * 0.72;
        vm.hng = w * 0.18;

        vm.xname = w * 0.18;
        vm.yname = h * 0.53;

        vm.xcon = w * 0.18;
        vm.ycon = h * 0.59;

        vm.xqrcode = w * 0.2;
        vm.yqrcode = h * 0.71;
        vm.wqrcode = w * 0.21;
        vm.hqrcode = w * 0.21;

        vm.xtxt = w * 0.23;
        vm.ytxt = h * 0.9;
        vm.xtxt1 = w * 0.61;
        vm.ytxt1 = h * 0.9;
        break;

      case 3:
        vm.bg = await this.downFile("https://wtapi.vzan.com/img/share4.png")
        vm.xstore = w * 0.16;
        vm.ystore = h * 0.05;
        vm.wstore = w * 0.68;
        vm.hstore = w * 0.68;

        vm.xng = w * 0.16;
        vm.yng = h * 0.423;
        vm.wng = w * 0.68;
        vm.hng = w * 0.16;

        vm.xname = w * 0.2;
        vm.yname = h * 0.47;

        vm.xcon = w * 0.2
        vm.ycon = h * 0.51;

        vm.xqrcode = w * 0.355;
        vm.yqrcode = h * 0.66;
        vm.wqrcode = w * 0.3;
        vm.hqrcode = w * 0.3;

        vm.xtxt = w * 0.43;
        vm.ytxt = h * 0.95;
        break;
      case 4:
        vm.bg = await this.downFile("https://wtapi.vzan.com/img/share5.png")
        vm.xstore = 0;
        vm.ystore = 0;
        vm.wstore = w;
        vm.hstore = w * 1.05;
        vm.xqrcode = w * 0.15;
        vm.yqrcode = h * 0.79;
        vm.wqrcode = w * 0.23;
        vm.hqrcode = w * 0.23;
        break;
      case 5:
        vm.bg = await this.downFile("https://wtapi.vzan.com/img/barShare.png")
        vm.xstore = w * 0.075;
        vm.ystore = h * 0.053;
        vm.wstore = w * 0.85;
        vm.hstore = w * 0.85;

        vm.xng = w * 0.075;
        vm.yng = h * 0.53;
        vm.wng = w * 0.85;
        vm.hng = w * 0.18;

        vm.xname = w * 0.1;
        vm.yname = h * 0.58;

        vm.xcon = w * 0.1;
        vm.ycon = h * 0.64;

        vm.xqrcode = w * 0.15;
        vm.yqrcode = h * 0.71;
        vm.wqrcode = w * 0.23;
        vm.hqrcode = w * 0.23;

        vm.xtxt = w * 0.19;
        vm.ytxt = h * 0.92;
        vm.xtxt1 = w * 0.61;
        vm.ytxt1 = h * 0.92;
        break;
      case 6:
        vm.bg = await this.downFile("https://wtapi.vzan.com/img/share1.png")
        vm.xstore = w * 0.045;
        vm.ystore = h * 0.031;
        vm.wstore = w * 0.91;
        vm.hstore = w * 0.97;

        vm.xng = w * 0.045;
        vm.yng = h * 0.6;
        vm.wng = w * 0.91;
        vm.hng = w * 0.18;

        vm.xname = w * 0.1;
        vm.yname = h * 0.65;

        vm.xcon = w * 0.1;
        vm.ycon = h * 0.7;

        vm.xqrcode = w * 0.15;
        vm.yqrcode = h * 0.75;
        vm.wqrcode = w * 0.23;
        vm.hqrcode = w * 0.23;

        vm.xtxt = w * 0.19;
        vm.ytxt = h * 0.95;
        vm.xtxt1 = w * 0.65;
        vm.ytxt1 = h * 0.95;
        break;
    }
    vm.qrcode = await this.downFile(_g.Qrcode ? _g.Qrcode.replace(/^http:/, "https:") : '', );
    vm.img = await this.downFile(_g.ADImg.length ? _g.ADImg[0].url.replace(/^http:/, "https:") : '', );
    vm.icon = vm.status == 1 ? await this.downFile(_g.Logo.length ? _g.Logo[0].url.replace(/^http:/, "https:") + "?x-oss-process=image/resize,l_100,image/circle,r_100/format,png" : "", ) : ""
    vm.name = _g.StoreName
    vm.content = _g.ADTitle
    return vm
  },
  async pathCanvas(qrCode, target) {
    let m = {}
    let tmp = _get(target, 'vm', '')
    let _ms = _get(target.vm, 'goodInfo', '')
    if (tmp) {
      if (tmp.type == 'good') {
        m.title = tmp.name
        m.img = tmp.img_fmt
        m.disprice = tmp.priceStr
        m.discount = tmp.discount
        m.price = tmp.originalPrice
      } else {
        m.img = tmp.img
        m.title = tmp.name
        m.discount = tmp.discount
        m.disprice = tmp.EntGroups.GroupPriceStr
        m.price = tmp.EntGroups.OriginalPriceStr
      }
    } else {
      m.discount = 99
      m.img = _ms.img
      m.title = _ms.name
      m.price = _ms.priceStr
      m.disprice = _ms.discountPricestr
    }
    m.img = await this.downFile(m.img.replace(/^http:/, "https:"));
    m.qrcode = await this.downFile(qrCode.replace(/^http:/, "https:"))
    return m
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
}
/**
 * @param {工具类型}
 */
const un = {
  icon(currentCom, index) {
    let vm = {}
    let share = currentCom.find(f => f.type == 'share')
    let bootom = currentCom.find(f => f.type == 'bottomnav')
    let phone = currentCom.find(f => f.type == 'contactShopkeeper')
    let shop = currentCom.find(f => f.btnType == 'buy' && (f.type == 'good' || f.type == 'goodlist'))
    let sub = currentCom.find(f => f.btnType == 'yuyue' && (f.type == 'good' || f.type == 'goodlist'))
    vm.sub = sub ? true : false
    vm.shop = shop ? true : false
    vm.share = share ? true : false
    vm.homeStatus = bootom == undefined && index != 0 ? true : false
    if (phone) {
      vm.suspend = phone.openServiceSuspend ? true : false
      vm.custom = phone.serverType
      if (phone.openTelSuspend) {
        vm.phone = {
          show: true,
          tel: phone.phoneNum
        }
      }
    }
    vm.bgmusic = true
    return vm;
  },
}
module.exports = {
  un,
  req,
  skin,
  timeTools,
  canvasTools
}
