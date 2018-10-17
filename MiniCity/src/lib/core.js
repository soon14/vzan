import wepy from "wepy";
import addr from "./addr.js";
import { wxParse } from "./wxParse/wxParse";
var isdebug = true;
var vm = {
  list: [],
  ispost: false, 
  loadall: false,
  pagesize: 20,
  pageindex: 1
};
var requestParameter = { 
  url: "",
  data: {},
  method: "GET",
  header: {
    "content-type": "application/x-www-form-urlencoded"
  }
};
const scopeList = {
  userInfo: "scope.userInfo", //wx.getUserInfo	    用户信息
  userLocation: "scope.userLocation", //wx.chooseLocation	地理位置
  address: "scope.address", //wx.chooseAddress	    通讯地址
  invoiceTitle: "scope.invoiceTitle", //wx.chooseInvoiceTitle发票抬头
  werun: "scope.werun", //wx.getWeRunData	    微信运动步数
  record: "scope.record", //wx.startRecord	    录音功能
  writePhotosAlbum: "scope.writePhotosAlbum", //wx.saveImageToPhotosAlbum, wx.saveVideoToPhotosAlbum	保存到相册
  camera: "scope.camera" //摄像头
};
 

var http = {
  //异步请求
  post: function (url, data) {
    return new Promise(function (resolve, reject) {
      wx.request(
        Object.assign({}, requestParameter, {
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
        })
      );
    });
  },
  get: function (url, data) {
    return new Promise(function (resolve, reject) {
      wx.request(
        Object.assign({}, requestParameter, {
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
        })
      );
    });
  }
};

var core = {
  /**********************************封装工具类**********************************************************/
  extend(target, src) {
    for (var key in src) {
      target[key] = src[key]
    }
    return target
  },
  //动态改顶部兰标题
  setPageTitle: function (tmpTitle) {
    wx.setNavigationBarTitle({
      title: tmpTitle
    });
  },
  // 图片点击放大
  preViewShow: function (current, urls) {
    wx.previewImage({
      current: current,
      urls: urls
    });
  },
  // 打开地图
  openMap: function (lat, lng) {
    wx.openLocation({
      latitude: lat,
      longitude: lng,
      scale: 28
    });
  },
  //时间戳
  ChangeDateFormat: function (val) {
    if (val != null) {
      var date = new Date(
        parseInt(val.replace("/Date(", "").replace(")/", ""), 10)
      );
      //月份为0-11，所以+1，月份 小时，分，秒小于10时补个0
      var month =
        date.getMonth() + 1 < 10
          ? "0" + (date.getMonth() + 1)
          : date.getMonth() + 1;
      var currentDate =
        date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
      var hour = date.getHours() < 10 ? "0" + date.getHours() : date.getHours();
      var minute =
        date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
      var second =
        date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds();
      var dd =
        date.getFullYear() +
        "-" +
        month +
        "-" +
        currentDate +
        " " +
        hour +
        ":" +
        minute +
        ":" +
        second;
      // console.log(dd)
      return dd;
    }
    return "";
  },
  //加载中
  loading: function (msg) {
    var title = msg ? msg : "";
    wx.showLoading({
      title: title
    });
  },
  //显示弹窗
  showToast: function (msg, icon, duration) {
    // var icon = icon ? icon : "success";
    // var duration = duration ? duration : 1000;
    return new Promise(function (resolve, reject) {
      wx.showToast({
        title: msg,
        icon: icon || "success",
        duration: duration || 1000,
        success: function (res) {
          resolve(res)
        }
      })
    })
  },
  //显示提示框
  showModal: function (msg) {
    return new Promise(function (resolve, reject) {
      wx.showModal({
        title: "提示",
        showCancel: false,
        content: msg || "",
        success: res => {
          resolve(res);
        }
      })
    })

  },
  /****************************************请求接口*************************************************/
  //动态添加轮播图小圆点
  getSwiperLength: function (arr, length) {
    for (var i = 1; i < length; i++) {
      arr.push("");
    }
  },
  //点赞、收藏、浏览
  addMsgViewFavoriteShare: function (appid, msgId, type, userid) {
    return new Promise(function (resolve, reject) {
      http
        .post(addr.AddMsgViewFavoriteShare, {
          appId: appid,
          msgId: msgId,
          actionType: type,
          userId: userid,
        })
        .then(data => {
          resolve(data)
        });
    })
  },
  //底部水印
  GetAgentConfigInfo:function(){
    let appid = wepy.$instance.globalData.appid;
     return http.post(addr.GetAgentConfigInfo,{
        appId:appid,
     })
  },
  // 普通支付// PayOrder
  PayOrder: async function (param) {
    let userInfo = await core.getUserInfo()
    let app = wepy.$instance;
    let aid = app.globalData.appid;
    return http.post(
      addr.PayOrder, {
        aid: aid,
        openId: userInfo.openId,
        orderid: param.orderid,
        'type': param.type,
      })
  },
  /* 支付 */
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
  shareAppMsg: function (e, list, navList) {
    var title, imgUrl, app, sharePath;
    var app = wepy.$instance;
    //var that = this;
    if (e.from === "menu") {
      title = navList.nav[navList.index].name;
      sharePath = "/pages/index";
    } else if (e.from === "button") {
      let id = e.target.dataset.id;
      let index = e.target.dataset.index;
      let defaultImg = "http://j.vzan.cc/miniapp/img/MiniCity/shareDefault.jpg";
      title = list[index].msgDetail;
      imgUrl = list[index].imgList[0] ? list[index].imgList[0] : defaultImg;
      sharePath = "/pages/classifyDetails?msgId=" + id;
    }
    return {
      title: title,
      imageUrl: imgUrl,
      path: sharePath,
      success: function (res) {
        if (e.from === "button") {
          let index = e.target.dataset.index;
          let id = e.target.dataset.id;
          core.addMsgViewFavoriteShare(app.globalData.appid, id, 2);
          list[index].ShareCount++;
          this.$apply();
        }
        core.showToast("分享成功", "success", 1000);
      },
      fail: function (res) {
        console.log(res);
      }
    };
  },
  //获取验证码
  GetVaildCode:function(param){
    let _param = {
      type:1,
      phonenum:param.phone,
      agentqrcodeid:param.agentqrcodeid,
    }
    return http.post(addr.GetVaildCode,_param);
  },
  //注册账号
  SaveUserInfo:function(param){
    return http.post(addr.SaveUserInfo,param);
  },
  /**********************************封装wx begin**********************************************************/
  //检查登陆状态
  checkSession: function () {
    return new Promise(function (resolve, reject) {
      // resolve(true) //电脑上使用
      wx.checkSession({
        success: function () {
          resolve(true);
        },
        fail: function () {
          resolve(false);
        }
      });
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
    });
  },
  wxLogin: async function (code) {
    let appid = wepy.$instance.globalData.appid;
    let _g = await http.post(addr.WxLogin, {
      code,
      appid,
      needappsr: 0,
    })
    if (_g.isok) {
      _g.dataObj.userid = _g.dataObj.Id
      _g.dataObj.openId = _g.dataObj.OpenId
      _g.dataObj.avatarUrl = _g.dataObj.HeadImgUrl
      _g.dataObj.nickName = _g.dataObj.NickName
      return _g.dataObj;
    }
    return '';
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
          resolve(false);
        }
      });
    });
  },
  openSetting: function () {
    wx.showModal({
      title: "提示",
      confirmText: "设置",
      showCancel: false,
      content: "未授权，请先前往授权",
      success: function (res) {
        console.log(res)
        isdebug && console.log(res.confirm);
        if (res.confirm) {
          wx.openSetting({
            success: function (setres) { }
          });
        }
      }
    });
  },
  getStorage: function (key) {
    return new Promise(function (resolve, reject) {
      wx.getStorage({
        key,
        success: function (res) {
          resolve(res.data);
        },
        fail: function () {
          resolve("");
        }
      });
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
  //检查某个权限是否允许授权
  checkAuth: async function (scope) {
    let authSetting = await this.getSetting(scope);
    if (!authSetting[scope]) {
      return false;
    } else {
      return true;
    }
  },

  /**********************************封装wx end**********************************************************/
  // getAid: async function() {
  //   var aid = await this.getStorage("aid");
  //   var app = wepy.$instance;
  //   var appid = app.globalData.appid;

  //   if (!aid) {
  //     var aidInfo = await http.post(addr.Getaid, {
  //       appid
  //     });
  //     if (aidInfo && aidInfo.isok) {
  //       try {
  //         wx.setStorageSync("aid", aidInfo.msg);
  //       } catch (e) {}
  //       return aidInfo.msg;
  //     }
  //     return "";
  //   } else return aid;
  // },
  getCurrentStore: function () {
    var app = wepy.$instance;
    var currentStore = app.globalData.currentStore;
    if (currentStore) {
      return currentStore;
    } else {
      currentStore = wx.getStorageSync("currentStore");
      app.globalData.currentStore = currentStore;
      return currentStore;
    }
  },
  loginByThirdPlatform: function (
    appid,
    code,
    encryptedData,
    signature,
    iv,
    isphonedata
  ) {
    return http.post(addr.loginByThirdPlatform, {
      code: code,
      data: encryptedData,
      signature: signature,
      iv: iv,
      appid: appid,
      isphonedata: isphonedata
    });
  },
  getUserInfo: async function () {
    let userInfo = await core.getStorage("userInfo")
    if (userInfo) {
      return userInfo;
    } else {
      let code = await core.login();
      if (code) {
        let info = await core.wxLogin(code);
        wx.setStorageSync("userInfo", info);
        //await core.getVipInfo()
        return info;
      }
      return "";
    }
  },
  //用户授权
  async userImpower(user) {
    let app = wepy.$instance;
    let code = await core.login();
    let info = await core.loginByThirdPlatform(
      app.globalData.appid,
      code,
      user.encryptedData,
      user.signature,
      user.iv,
      0
    );
    if (info.result) {
      wx.removeStorageSync("userInfo")
      wx.setStorageSync("userInfo", info.obj);
      return info.obj
    }
  },
  // getUserInfo: async function () {
  //   let that = this;
  //   var app = wepy.$instance;
  //   var userInfo = await this.getStorage("userInfo");
  //   var sessionStatus = await core.checkSession();
  //   if (sessionStatus && userInfo != "") {
  //     return userInfo;
  //   } else {
  //     var code = await core.login();
  //     if (code) {
  //       //这一步会要求授权，如果拒绝授权，encData=false
  //       var encData = await core.getBaseUserInfo();
  //       if (encData) {
  //         var result = await that.loginByThirdPlatform(
  //           app.globalData.appid,
  //           code,
  //           encData.encryptedData,
  //           encData.signature,
  //           encData.iv,
  //           0
  //         );
  //         if (result && result.result) {
  //           wx.setStorage({
  //             key: "userInfo",
  //             data: result.obj
  //           });
  //           return result.obj;
  //         }
  //         return "";
  //       }
  //       if (!await that.checkAuth(scopeList.userInfo)) {
  //         //core.openSetting();
  //         return "";
  //       }
  //       userInfo = "";
  //     }
  //     return userInfo;
  //   }
  // },
  //会员信息
  getVipInfo: async function (uid) {
    let userInfo = await this.getUserInfo();
    let app = wepy.$instance;
    let appid = app.globalData.appid;
    let vipInfo = "";
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
    return "";
  },
  loginByPhone: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo) {
      return http.post(addr.login, {
        appid,
        userid: userInfo.userid,
        phone: userInfo.tel
      });
    }

    return "";
  },
  index: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    let currentStore = this.getCurrentStore();
    if (userInfo) {
      return http.post(addr.index, {
        appid,
        userid: userInfo.userid,
        storeAppId: currentStore.AppId
      });
    }
    return "";
  },
  indexRecord: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo) {
      return http.post(addr.GetRecordByDate, {
        appid,
        userid: userInfo.userid
      });
    }

    return "";
  },
  GetStoreList: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo) {
      return http.post(addr.GetStoreList, {
        appid,
        userid: userInfo.userid
      });
    }
    return "";
  },
  //添加评论
  AddComment:async function(_param){
    var appid  = wepy.$instance.globalData.appid;
    var userInfo = await core.getUserInfo();
    var param = {
      appId:appid,
      userId:userInfo.userid,
      Id:_param.Id,
      CommentDetail:_param.commentDetail,
    }
    return http.post(addr.AddComment,param);
  },
  //获取评论列表
  GetMsgComment:async function(_param){
    var appid  = wepy.$instance.globalData.appid;
    var userInfo = await core.getUserInfo();
    var param ={
      appId:appid,
      userId:userInfo.userid,
    }
    var dataParam = core.extend(param,_param)
    return http.get(addr.GetMsgComment,dataParam);
  },
  //删除评论
  DeleteMsgComment:function(id){
    var appid  = wepy.$instance.globalData.appid;
    var param ={
      appId:appid,
      Id:id,
    }
    return http.post(addr.DeleteMsgComment,param);
  }
};

module.exports = {
  http,
  core,
  vm,
};
