//app.js

import { http, addr, tools } from "/modules/core.js";
App({
  onLaunch: function () {
    // this.getUserInfo().then(data => {
    //   console.log(data)
    // }, data => {
    //   tools.alert(data);
    // });
    this.getAppConfigInfo();
  },
  APPID :"wx6f902b2567efaf58",
  KEY_USERINFO: "userInfo",
  globalData: {
    userInfo: null,
    telEncryptedData:"",

  },
  getUser:function(){
    return wx.getStorageSync(this.KEY_USERINFO);
  },
  getUserInfo: function () {
    var that = this;
    return new Promise(function (resolve, reject) {
      var uinfo = wx.getStorageSync(that.KEY_USERINFO);
      if (uinfo) {
        that.globalData.userInfo = uinfo;
        resolve(uinfo);
      }
      else {
        var _code = "";
        that.wxlogin().then(function (res) {
          _code = res.code;
          return that.wxGetUserInfo()
        }).then(function (data) {
          return that.login(_code, data.encryptedData, data.signature, data.iv)
        }, function (data) {
          console.log(data.errMsg);
          if (data.errMsg == "getUserInfo:fail auth deny") {
            tools.alert("必须授权才能使用小未工坊");
          }
        }).then(function (data) {
          if (!data)
            return;
          if (data && data.result) {
            var json = data.obj
            var uinfo = {
              avatarUrl: json.avatarUrl,
              nickName: json.nickName,
              openId: json.openId,
              uid: json.userid,
              phone: json.tel||"",
            };
            that.globalData[that.KEY_USERINFO] = uinfo
            wx.setStorage({
              key: that.KEY_USERINFO,
              data: uinfo
            })
            resolve(uinfo);
          }
          else {
            reject(data.msg);
          }
        });
      }
    });
  },
  wxlogin: function () {
    var p = new Promise(function (resolve, reject) {
      wx.login({
        success: function (res) {
          resolve(res);
        },
        fail: function () {
          reject("wx.login 失败");
        }
      })
    });
    return p;
  },
  wxGetUserInfo: function () {
    var p = new Promise(function (resolve, reject) {
      wx.getUserInfo({
        withCredentials: true,
        success: function (res_userinfo) {
          console.log(res_userinfo)
          resolve(res_userinfo);
        },
        fail: function (e) {
          console.log("wx.getUserInfo fail", e)
          reject(e);
        }
      })
    });
    return p;
  },
  // (code, encryptedData, signature, iv, cb, isphonedata) {
  login: function (code, encryptedData, signature, iv, isphonedata) {
    var that=this;
    return http.getAsync(addr.loginByThirdPlatform, {
      code: code,
      iv: iv,
      data: encryptedData,
      signature: signature,
      appid: that.APPID,
      needappsr: 1,
      isphonedata: isphonedata||0
    });
  },
  getAppConfig:function(){
    return wx.getStorageSync("appConfig");
  },
  getAppConfigInfo:function(){
    var that=this;
    http.postAsync(addr.GetAppConfig, { appid: that.APPID}).then(function(data){
      if (data.result){
        if (typeof data.msg == "string" && data.msg!=""){
          data.msg=JSON.parse(data.msg);
          wx.setStorage({
            key: 'appConfig',
            data: data.msg,
          })
        }
        
      }
    });
  }

})