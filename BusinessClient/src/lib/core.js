import wepy from "wepy";
import addr from "./addr.js";
import _get from './lodash.get';
import {
  wxParse
} from "./wxParse/wxParse";
var isdebug = true;
var vm = {
  list: [],
  ispost: false,
  loadall: false,
  pagesize: 10,
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

/*im begin*/
var isEndClock = null;
var timer_countdown = null;
var isdebug = true;
let reConnectTimer = null;
let isConnecting = false; //ws是否正在连接中
let isFirst = true;
/*im end*/

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
}
];

var http = {
  //异步请求
  post: function (url, data, isjson) {
    var arg = arguments;
    return new Promise(function (resolve, reject) {
      let p = JSON.parse(JSON.stringify(requestParameter));
      if (isjson) {
        p.header["content-type"] = "application/json";
      }
      wx.request(
        Object.assign({}, p, {
          url,
          data,
          method: "POST",
          fail: function (e) {
            isdebug && console.log(e, data);
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
            isdebug && console.log(`请求 ${url} 失败！\r\n 错误信息：${e.errMsg}`);
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
  trim: function (input) {
    if (input) {
      return input.replace(/\s+/gm, "");
    }
    return "";
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
        date.getMonth() + 1 < 10 ?
          "0" + (date.getMonth() + 1) :
          date.getMonth() + 1;
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
  /**********************************封装wx begin**********************************************************/
  alert: function (content, title, ok, showCancel) {
    wx.showModal({
      title: title || "提示",
      content: content,
      showCancel: showCancel || true,
      success: function (res) {
        if (res.confirm) {
          if (ok) {
            ok();
          }
        } else if (res.cancel) {

        }
      }
    });
  },
  //检查登陆状态
  checkSession: function () {
    return new Promise(function (resolve, reject) {
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
  openSetting: function () {
    wx.showModal({
      title: "提示",
      confirmText: "设置",
      showCancel: false,
      content: "授权后，才能继续使用。",
      success: function (res) {
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
  getStorageSync: function (key) {
    try {
      var value = wx.getStorageSync(key)
      return value;
    } catch (e) {
      return "error";
    }
  },
  setStorageSync: function (key, value) {
    try {
      wx.setStorageSync(key, value)
    } catch (e) {
      return "error";
    }
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
  // getAid: function() {
  //   let currentStore =this.getCurrentStore();
  //   if (currentStore == "") {
  //     return;
  //   }
  //   return
  // },
  getCurrentStore: function () {
    return wx.getStorageSync("currentStore");
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
  WxLogin: function (appid, code) {
    return http.post(addr.WxLogin, {
      code: code,
      appid: appid
    });
  },
  getUserInfo: async function () {
    let that = this;
    var app = wepy.$instance;
    var userInfo = await this.getStorage("userInfo");
    var sessionStatus = await core.checkSession();
    if (userInfo) {
      //sessionStatus &&
      return userInfo;
    } else {
      var code = await core.login();
      if (code) {
        //这一步会要求授权，如果拒绝授权，encData=false
        var result = await that.WxLogin(
          app.globalData.appid,
          code,
        );
        if (result.code == 1 && result.dataObj) {
          wx.setStorageSync("userInfo", result.dataObj);
          return result.dataObj;
        } else {
          return "";
        }
        // if (!await that.checkAuth(scopeList.userInfo)) {
        //   core.openSetting();
        //   return "";
        // }
      }
    }
    return userInfo;
  },
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
        uid: userInfo.Id
      });
      if (vipInfo && vipInfo.isok) {
        app.globalData.vipInfo = vipInfo.model;
      }
      return vipInfo.model;
    }
    return "";
  },
  loginByPhone: async function (code, phone, userType) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo) {
      return http.post(addr.login, {
        appid,
        userid: userInfo.Id,
        phone: phone,
        userType,
        verificationCode: code
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
        userid: userInfo.Id,
        storeAppId: currentStore.AppId
      });
    }
    return "";
  },
  indexRecord: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") {
      return;
    }
    if (userInfo) {
      return http.post(addr.GetRecordByDate, {
        appid,
        userid: userInfo.Id,
        storeAppId: currentStore.AppId
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
        userid: userInfo.Id
      });
    }
    return "";
  },
  SendUserAuthCode: function (telePhoneNumber) {
    return http.post(addr.SendUserAuthCode, {
      telePhoneNumber
    });
  },
  GetOrderCount: async function (orderType, selType, value) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetOrderCount, {
      appid,
      orderType,
      selType,
      value,
      userid: userInfo.Id,
      storeAppId: currentStore.AppId
    });
  },
  GetGoodsTypes: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetGoodsTypes, {
      appid,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId
    });
  },
  GetGoodsTypesAll: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetGoodsTypesAll, {
      appid,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId
    });
  },
  GetEntGoodsList: async function (goodsType, pageIndex, pageSize, goodsName) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetEntGoodsList, {
      appid,
      goodsType,
      pageIndex,
      pageSize,
      goodsName,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId
    });
  },
  GetEntGoodsInfo: async function (id) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetEntGoodsInfo, {
      appid,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId,
      id
    });
  },
  SaveEntGoodsInfo: async function (p) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;

    return http.post(addr.SaveEntGoodsInfo, Object.assign(p, {
      appid,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId,
    }));
  },
  UpdateState: async function (act, tag, goodsId) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.UpdateState, {
      appid,
      storeAppId: currentStore.AppId,
      userId: userInfo.Id,
      act,
      tag,
      goodsId
    });
  },
  GetOrderList: async function (
    orderType,
    selType,
    value,
    pageIndex,
    pageSize,
    state,
    dateType
  ) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetOrderList, {
      appid,
      orderType,
      selType,
      value,
      pageIndex,
      pageSize,
      state,
      dateType,
      userid: userInfo.Id,
      storeAppId: currentStore.AppId
    });
  },
  GetMessageCount: async function (type) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetMessageCount, {
      appid,
      userid: userInfo.Id,
      storeAppId: currentStore.AppId
    });
  },
  GetVipList: async function (pageIndex, pageSize, searchValue, levelid) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetVipList, {
      appid,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId,
      pageIndex,
      pageSize,
      searchValue,
      levelid
    });
  },
  GetVipLevel: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetVipLevel, {
      appid,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId
    });
  },
  EditViplevel: async function (viprid, levelId) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.EditViplevel, {
      appid,
      storeAppId: currentStore.AppId,
      userId: userInfo.Id,
      viprid,
      levelId
    });
  },

  ChangeSaveMoney: async function (vipuid, saveMoney) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.ChangeSaveMoney, {
      appid,
      storeAppId: currentStore.AppId,
      userId: userInfo.Id,
      vipuid,
      saveMoney
    });
  },
  GetStoreInfo: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetStoreInfo, {
      appid,
      storeAppId: currentStore.AppId,
      userId: userInfo.Id
    });
  },
  SaveStoreInfo: async function (storeModel) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.SaveStoreInfo, {
      appid,
      storeAppId: currentStore.AppId,
      userId: userInfo.Id,
      storeModel: JSON.stringify(storeModel)
    });
  },
  GetOrderByTableNo: async function (type, tableNo) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetOrderByTableNo, {
      appid,
      type,
      kw: tableNo,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId
    });
  },
  UpdteOrderState: async function (orderType, orderId, state, oldState, remark, attachData) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.UpdteOrderState, {
      appid,
      orderType,
      orderId,
      state,
      oldState,
      remark,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId,
      attachData
    });
  },
  GetMessageList: async function (type, pageIndex, pageSize) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetMessageList, {
      appid,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId,
      type,
      pageIndex,
      pageSize
    });
  },
  ReadMessage: async function (type, orderId) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.ReadMessage, {
      appid,
      userId: userInfo.Id,
      storeAppId: currentStore.AppId,
      type,
      orderId
    });
  },
  /**
   * 默认只可选择一张图片，可以设置传多张
   * 返回值是图片url的数组
   */
  upload: function (filetype = "img", count = 1) {
    return new Promise(function (resolve, reject) {
      if (filetype == "img") {
        wx.chooseImage({
          count, //默认只能传一张
          success: function (res) {
            var uploadCount = 0;
            var uploadImgs = [];
            var tempFilePaths = res.tempFilePaths;

            function uploadOne() {
              wx.showLoading({
                mask: true,
                title: "上传中..."
              });
              const uploadTask = wx.uploadFile({
                url: addr.Upload,
                filePath: tempFilePaths[uploadCount],
                name: "file",
                formData: {
                  filetype: filetype
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
              });
              uploadTask.onProgressUpdate(res => {
                wx.showLoading({
                  mask: true,
                  title: "上传中" + res.progress + "%"
                });
              });
            }
            uploadOne();
          }
        });
      }
    });
  },
  GetConnectInfo: function (aid, phone) {
    return http.post(addr.GetConnectInfo, {
      aid,
      phone
    });
  },
  changeunreadmsg: function (unreadmsg, unreadmsgcount) {
    var app = wepy.$instance;
    app.globalData.unreadmsg = unreadmsg;
    app.globalData.unreadmsgcount = unreadmsgcount;

    wx.setStorage({
      key: "unreadmsg",
      data: unreadmsg
    });
    wx.setStorage({
      key: "unreadmsgcount",
      data: unreadmsgcount
    });
  },

  /***************** im begin ****************************/
  connectSocket: async function () {
    var that = this;
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo();
    var globaldata = app.globalData;
    var appid = app.globalData.appid || "";
    var fuserid = userInfo.Id || "";
    if (appid == "" || fuserid == "") return;
    if (globaldata.ws || isConnecting) return;
    isConnecting = true;

    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;

    let aid = currentStore.Id;
    let phone = userInfo.TelePhone;
    let appuser = await this.GetConnectInfo(aid, phone);
    if (!appuser.isok) {
      return;
    }


    wx.connectSocket({
      //fuserType：用户身份  0：普通用户 2：商家
      url: "wss://dzwss.xiaochengxu.com.cn/?appId=" +
        appuser.dataObj.appId +
        "&userId=" +
        appuser.dataObj.userId +
        "&isFirst=" +
        isFirst,
      header: {
        "content-type": "application/json"
      },
      method: "GET"
    });
    console.log("ws connecting...");
    wx.onSocketOpen(function (res) {
      console.log("ws is open", res);
      globaldata.ws = true;
      isConnecting = false;
      if (reConnectTimer) {
        clearTimeout(reConnectTimer);
        reConnectTimer = null;
      }
      //重连后，自动重发发送失败的消息
      for (var i = 0; i < globaldata.msgQueue.length; i++) {
        that.sendMessage(globaldata.msgQueue[i]);
      }
      globaldata.msgQueue = [];
    });
    wx.onSocketError(function (res) {
      console.log("WebSocket连接打开失败，请检查！", res);
      globaldata.ws = false;
      isConnecting = false;
    });

    wx.onSocketClose(function (res) {
      isFirst = false;
      console.log("WebSocket 已关闭！", res);
      globaldata.ws = false;
      isConnecting = false;
      core.reConnect();
    });
    //接收消息
    wx.onSocketMessage(function (res) {
      console.log("收到服务器内容：" + res.data);
      var msg = res.data;
      if (typeof res.data == "string") msg = JSON.parse(res.data);

      //判断当前在哪个页面
      var pages = getCurrentPages();
      var currentPage = pages[pages.length - 1];
      var fuser = currentPage.data.fuserInfo;
      var tuser = currentPage.data.tuserInfo;
      //聊天页面
      if (currentPage.route == "pages/im/chat") {
        var list = currentPage.data.vm.list;
        //如果消息是当前联系人发来的
        if (
          (msg.fuserId == fuser.userid && msg.tuserId == tuser.userid) || //我发的
          (msg.fuserId == tuser.userid && msg.tuserId == fuser.userid)
        ) {
          //发给我的

          list.push(msg);
          currentPage.setData({
            "vm.list": list,
            "vm.lastids": msg.ids
          });
        } else {
          core.markUnreadMsg(msg);
        }
      } else if (currentPage.route == "pages/im/contact") {
        //联系人页面
        core.markUnreadMsg(msg);
      } else {
        var key = msg.fuserId + "_" + msg.tuserId + "_" + msg.tuserType; //技师给用户发的
        var currentUnreadmsgcount = _get(globaldata.unreadmsg, key, 0);
        currentUnreadmsgcount += 1;
        globaldata.unreadmsg[key] = currentUnreadmsgcount;

        var unreadmsgcount = 0;
        for (var item in globaldata.unreadmsg) {
          unreadmsgcount += globaldata.unreadmsg[item];
        }

        core.changeunreadmsg(globaldata.unreadmsg, globaldata.unreadmsgcount);
      }
    });
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
    let app = wepy.$instance;
    if (typeof msg == "object") msg = JSON.stringify(msg);
    var globaldata = app.globalData;
    if (globaldata.ws) {
      wx.sendSocketMessage({
        data: msg
      });
    } else {
      app.globalData.msgQueue.push(msg);
    }
  },
  //只标记联系人列表里的未读消息
  markUnreadMsg: function (msg) {
    var that = this;
    let app = wepy.$instance;
    var pages = getCurrentPages();
    var currentPage = getCurrentPages().find(
      p => p.route == "pages/im/contact"
    );
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
          sendDate: msg.sendDate
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
    var key = msg.fuserId + "_" + msg.tuserId + "_" + msg.tuserType; //技师给用户发的
    var currentUnreadmsgcount = _get(app.globalData.unreadmsg, key, 0);
    currentUnreadmsgcount += 1;
    app.globalData.unreadmsg[key] = currentUnreadmsgcount;

    var unreadmsgcount = 0;
    for (var item in app.globalData.unreadmsg) {
      unreadmsgcount += app.globalData.unreadmsg[item];
    }

    core.changeunreadmsg(app.globalData.unreadmsg, unreadmsgcount);
  },
  changeunreadmsg: function (unreadmsg, unreadmsgcount) {
    var app = wepy.$instance;
    app.globalData.unreadmsg = unreadmsg;
    app.globalData.unreadmsgcount = unreadmsgcount;

    wx.setStorage({
      key: "unreadmsg",
      data: unreadmsg
    });
    wx.setStorage({
      key: "unreadmsgcount",
      data: unreadmsgcount
    });
  },
  AddContact: async function (userid) {
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo();
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    let aid = currentStore.Id;
    let phone = userInfo.TelePhone;
    let appuser = await this.GetConnectInfo(aid, phone);
    if (!appuser.isok) {
      return;
    }

    http.post(addr.AddContact, {
      appId: app.globalData.appid,
      fuserId: appuser.dataObj.userId,
      tuserId: userid
    });
  },
  getHistory: async function (fuserid,tuserid, vm, targetPage) {
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo();
    if (vm.ispost || vm.loadall) return;

    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    let aid = currentStore.Id;
    let phone = userInfo.TelePhone;
    let appuser = await this.GetConnectInfo(aid, phone);


    vm.ispost = true;
    http
      .post(addr.GetHistory, {
        appId: app.globalData.appid,
        fuserId:fuserid,
        tuserId: tuserid,
        id: vm.lastid,
        fuserType: 0,
        ver: 1
      })
      .then(res => {
        if (res && res.isok) {
          res.data.length < vm.pagesize ?
            (vm.loadall = true) :
            (vm.loadall = false);
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
          vm.loadall = true;
        }
        vm.ispost = false;
        targetPage.vm = vm;
        targetPage.$apply();
      });
  },

  GetContactList: async function (pageIndex, pageSize) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;

    let aid = currentStore.Id;
    let phone = userInfo.TelePhone;
    let appuser = await this.GetConnectInfo(aid, phone);
    if (appuser.isok && appuser.dataObj) {
      return http.post(addr.GetContactList, {
        appid,
        fuserId: appuser.dataObj.userId,
        storeAppId: currentStore.AppId,
        pageIndex,
        pageSize,
        fuserType: 0,
        ver: 1
      });
    } else {
      return {
        isok: false,
        Msg: '未设置客服'
      }
    }

  },
  getContactList: async function (vm) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;

    let aid = currentStore.Id;
    let phone = userInfo.TelePhone;
    let appuser = await this.GetConnectInfo(aid, phone);
    return http.post(addr.GetContactList, {
      appid,
      fuserId: appuser.dataObj.userId,
      storeAppId: currentStore.AppId,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
      fuserType: 0,
      ver: 1,
    })
  },
  /***************** im end ****************************/
  Logout: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    //let currentStore = await this.getStorage("currentStore");
    //if (currentStore == "") return;
    return http.post(addr.Logout, {
      appid,
      userid: userInfo.Id,
      //storeAppId: currentStore.AppId,
    });

  },
  GetGoodInfo: async function (id) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.GetGoodInfo, {
      appid,
      userid: userInfo.Id,
      storeAppId: currentStore.AppId,
      id
    });
  },
  GoodType: async function (model) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    var obj = Object.assign(model, {
      appid: appid,
      userid: userInfo.Id,
      storeAppId: currentStore.AppId,
    });
    console.log(obj);
    return http.post(addr.GoodType, obj);
  },
  GoodAttr: async function (model) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    var obj = Object.assign(model, {
      appid: appid,
      userid: userInfo.Id,
      storeAppId: currentStore.AppId,
    });
    console.log(obj);
    return http.post(addr.GoodAttr, obj);
  },
  GetProductQrcode: async function (pid) {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    if (currentStore == "") return;
    //appId=wx3aa0870b24142168&pid=161817&recordId=0&typeName=buy&showprice=0&productType=0
    var obj = {
      appid: currentStore.AppId,
      userid: userInfo.Id,
      pid,
      recordId: 0,
      typeName: 'buy',
      showprice: 1,
      productType: 0,
    };
    return http.post(addr.GetProductQrcode, obj);
  },
  getDeliveryCompany: async function () {
    return http.post(addr.getDeliveryCompany, {});
  },
  ViewDeliveryFeed: async function () {
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    let userInfo = await core.getUserInfo();
    if (userInfo == "") return;
    let currentStore = await this.getStorage("currentStore");
    if (currentStore == "") return;
    return http.post(addr.ViewDeliveryFeed, {

    });
  },
  getAppInfo:function(appid){
    return http.get(addr.getAppInfo,{appid:appid});
  },
};


module.exports = {
  http,
  core,
  vm
};
