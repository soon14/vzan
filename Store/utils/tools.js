if (!Date.prototype.Format) {
  Date.prototype.Format = function (fmt) {
    var o = {
      "M+": this.getMonth() + 1,                 //月份 
      "d+": this.getDate(),                    //日 
      "H+": this.getHours(),                   //小时 
      "m+": this.getMinutes(),                 //分 
      "s+": this.getSeconds(),                 //秒 
      "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
      "S": this.getMilliseconds()             //毫秒 
    };

    if (/(y +)/.test(fmt)) {
      fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    for (var k in o)
      if (new RegExp("(" + k + ")").test(fmt))
        fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
  }
}


//工具类
var addr = require("addr.js");
var http = require("http.js");


var isEndClock = null;

var app = getApp();
var tools = {
  //类型
  typeEnum: {
    "data": "[object Date]",
    "object": "[object Object]",
    "number": "[object Number]",
    "string": "[object String]",
    "boolean": "[object Boolean]"
  },
  //弹窗
  alert: function (title, content, successCallback, cancelCallback) {
    wx.showModal({
      title: title || '提示',
      content: content || "",
      success: function (res) {
        if (successCallback) {
          successCallback();
        }
        if (cancelCallback) {
          cancelCallback();
        }
      }
    })
  },
  tips: function (title) {
    wx.showToast({
      title: title,
      icon: 'success',
      duration: 1500
    })
  },
  getUserInfo: function () {
    app=getApp();
    var _user = app.globalData.userInfo;
    return new Promise(function (resolve, reject) {
      if (!_user.UserId) {
        app.getUserInfo(function (uinfo) {
          resolve(uinfo);
        })
      }
      else {
        resolve(_user);
      }
    })

  },
  updateUserAddress: function (_userid, _WxAddress) {
    http
      .postAsync(addr.Address.UpdateUserWxAddress,
      {
        UserId: _userid,
        WxAddress: _WxAddress
      });
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
  formatMillisecond: function (millisecond) {
    var days = Math.floor(millisecond / (1000 * 60 * 60 * 24));
    var hours = Math.floor((millisecond % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    var minutes = Math.floor((millisecond % (1000 * 60 * 60)) / (1000 * 60));
    var seconds = Math.floor((millisecond % (1000 * 60)) / 1000);
    if (days.toString().length < 2)
      days = "0" + days;

    if (hours.toString().length < 2)
      hours = "0" + hours;

    if (minutes.toString().length < 2)
      minutes = "0" + minutes;

    if (seconds.toString().length < 2)
      seconds = "0" + seconds;

    return [days, hours, minutes, seconds];
  },
  initEndClock: function (begin, end, whereData) {
    var that = whereData;
    var _begin = begin.replace(/-/g, "/");
    var _end = end.replace(/-/g, "/");
    var now = new Date().getTime();
    var begin = new Date(_begin).getTime();
    var end = new Date(_end).getTime();
    
    //未开始
    if (begin - now > 0) {
      var _dd = 0, _hh = 0, _mm = 0, _ss = 0;
      var totalSecond = parseInt((begin - now) / 1000);
      var totalMinute = parseInt(totalSecond / 60);
      var totalHour = parseInt(totalMinute / 60);

      _dd = Math.floor(totalHour / 24);
      _hh = Math.floor(totalHour % 24);
      _mm = Math.floor(totalMinute % 60);
      _ss = Math.floor(totalSecond % 60);
      _dd = _dd < 10 ? "0" + _dd : _dd;
      _hh = _hh < 10 ? "0" + _hh : _hh;
      _mm = _mm < 10 ? "0" + _mm : _mm;
      _ss = _ss < 10 ? _ss = "0" + _ss : _ss;
      var _fromTheEnd = {
        dd: _dd,
        hh: _hh,
        mm: _mm,
        ss: _ss,
      }

      
      that.setData({
        fromTheEnd: _fromTheEnd,
        fromTheEnd_txt: "距离开始",
        groupstate: -1
      });
      isEndClock = setTimeout(function () {
        tools.initEndClock(_begin, _end, that);
      }, 1000);
    }
    //开始中
    else if (begin - now < 0 && end - now > 0) {
      var _dd = 0, _hh = 0, _mm = 0, _ss = 0;
      var totalSecond = parseInt((end - now) / 1000);
      var totalMinute = parseInt(totalSecond / 60);
      var totalHour = parseInt(totalMinute / 60);

      _dd = Math.floor(totalHour / 24);
      _hh = Math.floor(totalHour % 24);
      _mm = Math.floor(totalMinute % 60);
      _ss = Math.floor(totalSecond % 60);
      _dd = _dd < 10 ? "0" + _dd : _dd;
      _hh = _hh < 10 ? "0" + _hh : _hh;
      _mm = _mm < 10 ? "0" + _mm : _mm;
      _ss = _ss < 10 ? _ss = "0" + _ss : _ss;
      var _fromTheEnd = {
        dd: _dd,
        hh: _hh,
        mm: _mm,
        ss: _ss,
      }
      that.setData({
        fromTheEnd: _fromTheEnd,
        fromTheEnd_txt: "距离结束",
        groupstate: 1
      });
      isEndClock = setTimeout(function () {
        tools.initEndClock(_begin, _end, that);
      }, 1000);
    }
    //结束
    else if (now - end >= 0) {
      if (isEndClock != null) {
        clearTimeout(isEndClock);
        isEndClock = null;
      }
      that.setData({
        fromTheEnd_txt: "已结束",
        groupstate: 0
      });
    }
  },
  copyData: function (data) {
    var that = this;
    wx.setClipboardData({
      data: data,
      success: function (res) {
        that.tips("复制成功");
      }
    })
  },
  share: function (group) {
    var _g = group;
    var _path = '/pages/groupOrder/groupInvite?gsid=' + _g.GroupSponsorId;
    var _title = `￥${_g.DiscountPrice / 100}元就能购买${_g.GroupName},一起来拼团吧！`;
    console.log(_path);
    return {
      title: _title,
      path: _path,
      imageUrl: _g.ImgUrl,
      success: function (res) {
        // 转发成功
        tools.tips("转发成功");
      },
      fail: function (res) {

      }
    }
  },
  WxLogin: function (appid, code) {
    return http.postAsync(addr.Address.WxLogin, {
      code: code,
      appid: appid
    });
  },
};
module.exports = tools;