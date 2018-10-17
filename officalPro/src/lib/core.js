import wepy from "wepy";
import addr from "./addr.js"
import _get from './lodash.get';
import {
  wxParse
} from './wxParse/wxParse';
import {
  Promise
} from "core-js";
let reConnectTimer = null;
let isConnecting = false; //ws是否正在连接中
let isFirst = true;
let isdebug = false;
let settime = ""; //私信计时
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

var tools = {
  getAuth: function () {
    return new Promise(function (resolve, reject) {
      wx.getSetting({
        success: (res) => {
          if (Object.keys(res.authSetting).length) {
            if (res.authSetting['scope.writePhotosAlbum']) {
              resolve(false)
            } else {
              resolve(true)
            }
          } else {
            resolve(false)
          }

        }
      })
    })
  },
  getLocation: function () {
    return new Promise(function (resolve, reject) {
      wx.getLocation({
        type: 'wgs84',
        success: function (res) {
          resolve(res)
        },
        fail: function (res) {
          resolve(res)
        }
      })
    })
  },
  chooseLocation: function () {
    return new Promise(function (resolve, reject) {
      wx.chooseLocation({
        success: res => {
          resolve(res)
        },
        fail: res => {
          resolve(res)
        }
      })
    });
  },
  showModal: function (msg, _bool) {
    return new Promise(function (resolve, reject) {
      wx.showModal({
        title: "提示",
        content: msg,
        showCancel: _bool,
        success: res => {
          resolve(res)
        }
      })
    })
  },
  loading: function (msg) {
    wx.showToast({
      title: msg,
      icon: "loading",
      duration: 1000
    })
  },
  wxShowToast: function (msg) {
    wx.showToast({
      title: msg,
      icon: "success",
      duration: 1000
    })
  },
  //显示提示框
  ShowToast: function (msg, targetPage) {
    targetPage.toast.show = true;
    targetPage.toast.msg = msg;
    targetPage.$apply();
    setTimeout(() => {
      targetPage.toast.show = false;
      targetPage.$apply();
    }, 1000);
  },
  // 图片上传/
  upLoadimg(tempImg, index) {
    return new Promise(function (resolve, reject) {
      const uploadTask = wx.uploadFile({
        url: addr.Upload,
        filePath: tempImg[index],
        name: "file",
        formData: {
          filetype: 'img'
        },
        success: function (res) {
          resolve(res)
        },
        fail: function (res) {
          resolve(res)
        },
        complete: function () {}
      })
      uploadTask.onProgressUpdate((res) => {
        tools.loading('上传中' + res.progress + "%")
      })
    });
  },
  chooseImage(count) {
    return new Promise(function (resolve, reject) {
      wx.chooseImage({
        count: count, // 默认9
        sizeType: ["original", "compressed"], // 可以指定是原图还是压缩图，默认二者都有
        sourceType: ['album', 'camera'], // 可以指定来源是相册还是相机，默认二者都有
        success: async function (res) {
          // 返回选定照片的本地文件路径列表，tempFilePath可以作为img标签的src属性显示图片
          resolve(res)
        }
      })
    })
  },
  resetArray(array) {
    Object.assign(array, {
      ispost: false,
      loadall: false,
      ids: '',
      list: [],
      pageindex: 1,
      search: "",
      pricesort: "",
      isFirstType: "",
      saleCountSort: "",
    })
    return array
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
      delta
    })
  },
  //消除中间栈层
  goRedirecto: function (url) {
    wx.redirectTo({
      url,
    })
  },
  //重启动
  goLaunch: function (url) {
    wx.reLaunch({
      url
    })
  },
  //动态改顶部兰标题
  setPageTitle: function (title) {
    wx.setNavigationBarTitle({
      title,
    });
  },
  setPageSkin: function (ftColor, bgColor) {
    wx.setNavigationBarColor({
      frontColor: ftColor,
      backgroundColor: bgColor,
    })
  },
  // 图片点击放大
  preViewShow: function (current, imglst) {
    let urls = []
    imglst ? urls = imglst : urls.push(current)
    wx.previewImage({
      current: current,
      urls: urls,
    })
  },
  //拨打电话
  makePhone: async function (number, type) {
    wx.makePhoneCall({
      phoneNumber: number
    })
    if (type != 'nosend') {
      core.onShareUp('客户拨打了你的电话，离成交更进一步了哦')
    }
  },

  // 打开地图
  openMap: function (vm) {
    wx.openLocation({
      latitude: vm.lat,
      longitude: vm.lng,
      name: vm.name,
      address: vm.address,
      scale: 28
    })
  },
  //保存号码
  addPhone: function (vm) {
    return new Promise(function (resolve, reject) {
      wx.addPhoneContact({
        firstName: vm.name,
        mobilePhoneNumber: vm.phone,
        organization: vm.company,
        success: res => {
          resolve(res)
        },
        fail: res => {
          resolve(res)
        }
      })
    })
  },
  // 复制
  copy: function (data) {
    wx.setClipboardData({
      data: data,
      success: function (res) {
        wx.getClipboardData({
          success: function (res) {
            tools.wxShowToast("复制成功")
          }
        })
      }
    })
  },
  //时间戳
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
  patchTime: function (time) {
    var len = String(time).length
    var sliceField = len > 2 ? len : 2
    return ('0' + time).slice(-sliceField)
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
  //私信
  upload: function (filetype = "img") {
    return new Promise(function (resolve, reject) {
      if (filetype == "img") {
        wx.chooseImage({
          success: function (res) {
            var uploadCount = 0;
            var uploadImgs = [];
            var tempFilePaths = res.tempFilePaths;

            function uploadOne() {
              tools.loading('上传中...')
              const uploadTask = wx.uploadFile({
                url: addr.Upload,
                filePath: tempFilePaths[uploadCount],
                name: 'file',
                formData: {
                  filetype: filetype,
                },
                success: function (res) {
                  var data = JSON.parse(res.data);
                  if (data.result) {
                    uploadCount += 1;
                    uploadImgs.push(data.msg);
                  } else {
                    resolve("");
                  }
                  if (uploadCount < tempFilePaths.length) {
                    uploadOne();
                  } else {
                    resolve(uploadImgs);
                  }
                },
                complete: function () {
                  wx.hideLoading();
                }
              })
              uploadTask.onProgressUpdate((res) => {
                tools.loading('上传中' + res.progress + "%")
              })
            }
            uploadOne();
          }
        })
      }
    });
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
};
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
            resolve("");
          }
        }
      }))
    });
  },

};
var core = {
  getAid: async function () {
    let app = wepy.$instance
    let aid = _get(app.globalData, "aid", '')
    let appid = app.globalData.appid;
    if (aid) {
      return aid
    } else {
      let aidInfo = await http.post(addr.Getaid, {
        appid
      });
      if (aidInfo.isok) {
        app.globalData.aid = aidInfo.msg
        return aidInfo.msg;
      }
    }
  },
  // 登陆
  login: function () {
    return new Promise(function (resolve, reject) {
      wx.login({
        success: function (res) {
          resolve(res.code);
        }
      });
    })
  },
  // 检查登陆
  checkSession: function () {
    return new Promise(function (resolve, reject) {
      wx.checkSession({
        success: function () {
          console.log('有效', '检查登陆状态')
          resolve(true);
        },
        fail: function () {
          console.log('失效', '检查登陆状态')
          resolve(false);
        }
      })
    });
  },
  getUserInfo: async function () {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '')
    let checkSession = await core.checkSession()
    if (checkSession) {
      if (userInfo) {
        app.globalData.userInfo = userInfo
        return userInfo
      } else {
        userInfo = await core.loginInfo()
        app.globalData.userInfo = userInfo
        return userInfo
      }
    } else {
      userInfo = await core.loginInfo()
      app.globalData.userInfo = userInfo
      return userInfo
    }
  },
  loginInfo: async function () {
    let app = wepy.$instance;
    let code = await core.login()
    if (code) {
      let info = await http.post(addr.WxLogin, {
        code,
        needappsr: 0,
        appid: app.globalData.appid,
      })
      if (info.dataObj.HeadImgUrl == null && info.dataObj.NickName == null) {
        info.dataObj.newUser = true
      } else {
        info.dataObj.newUser = false
      }
      return info.dataObj
    }
  },
  loginUserInfo: async function (vm) {
    let app = wepy.$instance
    let appid = app.globalData.appid;
    let _user = await http.post(addr.loginByThirdPlatform, {
      appid,
      iv: vm.iv,
      code: vm.code,
      data: vm.data,
      signature: vm.sign,
      isphonedata: vm.phone,
      needappsr:0
    })
    if (_user.result) {
      if (_user.obj.avatarUrl == null && _user.obj.nickName == null) {
        _user.obj.newUser = true
      } else {
        _user.obj.newUser = false
      }
      _user.obj.TelePhone = _user.obj.tel
      _user.obj.OpenId = _user.obj.openId
      _user.obj.Id = _user.obj.userid
      _user.obj.HeadImgUrl = _user.obj.avatarUrl
      _user.obj.NickName = _user.obj.nickName
      app.globalData.userInfo = _user.obj
      return _user.obj;
    }
  },
  /**
   * GetGoodsCategoryLevel是否开启二级分类
   * GetGoodsCategory分类列表数据
   * @param {data.dataObj.Level} 1表示只开启了一级分类,表示按照小类显示  ;2表示开启了两级,
   * 获取大类传1 获取小类传2 指定小类传3 以及parentid
   * @return {产品操作}
   */
  GetGoodsCategoryLevel: async function (target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    http.post(addr.GetGoodsCategoryLevel, {
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey
    }).then(data => {
      if (data.isok) {
        if (data.dataObj.Level == 1) {
          core.GetGoodsCategory(2, 0, target)
        } else {
          core.GetGoodsCategory(1, 0, target)
          core.GetGoodsCategory(2, 0, target)
        }
      }
    })
  },
  GetGoodsCategory: async function (isFirstType, parentId, target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    http.post(addr.GetGoodsCategory, {
      appId: app.globalData.appid,
      isFirstType: isFirstType,
      parentId: parentId || 0,
      sessionkey: userInfo.loginSessionKey
    }).then(async data => {
      if (data.isok) {
        if (isFirstType == 1) {
          target.vm.firstType = data.dataObj.list
        }
        if (isFirstType == 2 || isFirstType == 3) {
          target.vm.secondType = data.dataObj.list
        }
        target.$apply()
      }
    })
  },
  /**
   * GetGoodsList产品列表
   * GetGoodInfo产品详情
   * @param {search:搜索字段，typeid：小类拼接id或大类拼接id，pricesort：价格排序，isFirstType：大类传0，小类传空值，saleCountSort：销量排序}
   * @return {产品操作}
   */
  GetGoodsList: async function (vm, target) {
    let app = wepy.$instance
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.GetGoodsList, {
      aid,
      appId: app.globalData.appid,
      search: vm.search,
      typeid: vm.ids,
      pricesort: vm.pricesort,
      isFirstType: vm.isFirstType,
      saleCountSort: vm.saleCountSort,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
      sessionkey: userInfo.loginSessionKey
    }).then(async data => {
      if (data.isok) {
        vm.ispost = false
        data.dataObj.goodslist.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true
        data.dataObj.goodslist.length > 0 ? vm.list = vm.list.concat(data.dataObj.goodslist) : ""
        if (data.dataObj.goodslist.length == 0) {
          vm.noGoodlst = true
        } else {
          vm.noGoodlst = false
        }
        target.goodsLst = vm
        target.$apply()
        if (app.globalData.chat.product) {
          return
        }
        core.onShareUp('查看了产品，快问问客户想要啥')
        app.globalData.chat.product = true
      }
    })
  },
  GetGoodInfo: async function (pid, target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    http.post(addr.GetGoodInfo, {
      pid,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey
    }).then(async data => {
      if (data.isok) {
        let _v = data.dataObj
        if (_v.Slideimgs) {
          _v.Slideimgs = _v.Slideimgs.split(',')
        }
        if (_v.Description) {
          _v.content = _v.Description.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>').replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>').replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
          _v.content = wxParse('content', 'html', _v.content || "", target, 8);
        }
        if (_v.PickSpec) {
          _v.PickSpec = JSON.parse(_v.PickSpec)
        }
        target.vm = _v
        target.$apply()
        tools.setPageTitle(_v.Name)
      } else {
        await tools.showModal(data.Msg, false)
        tools.goBack(1)
      }
    })
  },

  /**
   * GetUserIsBind该用户是否绑定员工名片
   * @param {}
   * @return {名片操作}
   */
  GetUserIsBind: async function (target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.GetUserIsBind, {
      aid,
      userId: userInfo.Id,
      sessionkey: userInfo.loginSessionKey,
      appId: app.globalData.appid,
    }).then(data => {

      if (data.isok) {
        tools.setPageTitle('我的名片')
        tools.setPageSkin("#ffffff", '#444444')
        target.vm.showPerson = true
        Promise.all([core.GetEmployee(data.dataObj, target), core.getMess(target.vmCustomer, target)])
      } else {
        tools.setPageTitle('名片夹')
        tools.setPageSkin("#000000", '#ffffff')
        target.vm.showPerson = false
        core.GetMyListEmployee(target.vmEmp, target)
      }
      target.$apply()
    })
  },
  // 将用户通过员工码绑定名片
  BindWorkIDByUserId: async function (wordid, target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.BindWorkIDByUserId, {
      aid,
      workId: wordid,
      userId: userInfo.Id,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(data => {
      if (data.isok) {
        tools.showModal(data.Msg, false)
        target.vm.showPerson = true
        target.personMask = false
        target.$apply()
        tools.setPageTitle('我的名片')
        tools.setPageSkin("#ffffff", '#444444')
        Promise.all([core.GetEmployee(data.dataObj, target), core.getMess(target.vmCustomer, target)])
      } else {
        tools.showModal(data.Msg, false)
      }
    })
  },
  //聊天记录
  getMess: async function (vm, target) {
    let userInfo = await core.getUserInfo()
    http.post(addr.GetEmployeeMessage, {
      userId: userInfo.Id,
      sessionkey: userInfo.loginSessionKey,
      name: vm.name || "",
      pageIndex: 1,
      pageSize: 5,
    }).then(data => {
      if (data.isok) {
        target.vmCustomer = data.dataObj.list
        target.$apply()
      }
    })
  },
  //获取名片详情
  GetEmployee: async function (id, target, type) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.GetEmployee, {
      aid,
      employeeId: id,
      userId: userInfo.Id,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(data => {
      if (data.isok) {
        data.dataObj.Location = data.dataObj.Location.split(",")
        target.vm.personInfo = data.dataObj
        target.vm.userInfo = userInfo
        target.$apply()
        app.globalData.cardDlt = data.dataObj //全局名片详情
        if (type) {
          core.onShareUp('客户查看了你的名片，主动出击吧')
        }
      } else {
        tools.showModal(data.Msg, false)
      }
    })
  },



  //员工获取客户
  cardCustomerList: async function (target) {
    let _g = {}
    let userInfo = await core.getUserInfo()
    http.post(addr.GetCustomerList, {
      userId: userInfo.Id,
      sessionkey: userInfo.loginSessionKey,
      name: "",
      pageIndex: 1,
      pageSize: 4,
    }).then(data => {
      if (data.isok) {
        _g.count = data.dataObj.count
        _g.list = data.dataObj.list
        target.vmCustomer = _g
        target.$apply()
      }
    })
  },
  //员工获取客户
  GetCustomerList: async function (vm, target) {
    if (vm.ispost || vm.loadall)
      return
    if (!vm.ispost)
      vm.ispost = true
    let userInfo = await core.getUserInfo()
    http.post(addr.GetCustomerList, {
      userId: userInfo.Id,
      sessionkey: userInfo.loginSessionKey,
      name: vm.name || "",
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
    }).then(data => {
      vm.ispost = false;
      if (data.isok) {
        vm.count = data.dataObj.count
        data.dataObj.list.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        data.dataObj.list.length > 0 ? vm.list = vm.list.concat(data.dataObj.list) : "";

      } else {
        vm.loadall = true
      }
      target.vmCustomer = vm
      target.$apply()
    })
  },
  //解除绑定
  JieBindWorkIDByUserId: async function (target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.JieBindWorkIDByUserId, {
      aid,
      userId: userInfo.Id,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(data => {
      if (data.isok) {
        tools.setPageTitle('名片夹')
        tools.setPageSkin("#000000", '#ffffff')
        target.vm.showPerson = false
        target.vm.personInfo = {}
        target.vmEmp = tools.resetArray(target.vmEmp)
        core.GetMyListEmployee(target.vmEmp, target)
        target.$apply()
      }
    })
  },
  //获取动态
  GetListPostMsg: async function (target, userid) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.GetListPostMsg, {
      aid,
      userId: userid || userInfo.Id,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(async data => {
      if (data.isok) {
        target.vm = data.dataObj.list
        target.$apply()
      } else {
        await tools.showModal(data.Msg, false)
        tools.goBack(1)
      }
    })
  },
  //发布动态
  PostMsg: async function (mess, imglst) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.PostMsg, {
      aid,
      userId: userInfo.Id,
      msgDetail: mess,
      imgs: imglst,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(async data => {
      if (data.isok) {
        await tools.showModal(data.Msg, false)
        tools.goRedirecto("/pages/card/dongTaiMore")
      } else {
        tools.showModal(data.Msg, false)
      }
    })
  },
  //删除动态
  DelPostMsg: async function (id, target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.DelPostMsg, {
      id,
      aid,
      userId: userInfo.Id,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(data => {
      if (data.isok) {
        tools.wxShowToast(data.Msg)
        core.GetListPostMsg(target)
      } else {
        tools.showModal(data.Msg, false)
      }
    })
  },
  /**点赞、关注、浏览、私信、收藏量
   * @param {actiontype:0收藏 1点赞 2关注 3看过 4私信 5评论 6转发}
   * @return {datatype:0帖子 1商品 2评论 3名片 4店铺;othercardid:操作id}
   */
  AddFavorite: async function (vm, target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.AddFavorite, {
      aid,
      actiontype: vm.action,
      userid: userInfo.Id,
      othercardid: vm.id,
      datatype: vm.type,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,

    }).then(data => {
      if (data.isok) {
        if (vm.action == 1 && vm.type == 3) {
          if (data.dataObj == 0) {
            target.vm.personInfo.DzCount = target.vm.personInfo.DzCount + 1
            target.vm.personInfo.IsDzed = true
            core.onShareUp('给你点了个赞，继续努力哟')
          } else {
            target.vm.personInfo.DzCount = target.vm.personInfo.DzCount - 1
            target.vm.personInfo.IsDzed = false
          }
        }
        target.$apply()
      }
    })
  },
  // 获取名片数据雷达
  GetDataList: async function (id, target) {
    let app = wepy.$instance
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    let userInfo = await core.getUserInfo()
    http.post(addr.GetDataList, {
      aid,
      employeeId: id,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(async data => {
      if (data.isok) {
        target.vm = data.dataObj
        target.vm.cardDlt = app.globalData.cardDlt
        target.$apply()
      } else {
        await tools.showModal(data.Msg, false)
        tools.goBack(1)
      }
    })
  },
  // 修改编辑名片
  EditEmployee: async function (vm) {
    let app = wepy.$instance
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    let userInfo = await core.getUserInfo()
    http.post(addr.EditEmployee, {
      aid,
      name: vm.name,
      avatar: vm.avatar,
      employeeId: vm.id,
      phone: vm.phone,
      wxAccount: vm.wxAccount,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(async data => {
      if (data.isok) {
        let tip = await tools.showModal(data.Msg, true)
        if (tip.confirm) {
          tools.goBack(1)
        }
      } else {
        tools.showModal(data.Msg, false)
      }
    })
  },
  //名片码
  GetEmployeeQrcode: async function (vm, target) {
    let app = wepy.$instance
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    wx.showLoading({
      title: '加载中..',
    })
    let userInfo = await core.getUserInfo()
    http.post(addr.GetEmployeeQrcode, {
      aid,
      employeeId: vm.Id,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(async data => {
      if (data.isok) {


        let systemInfo = await core.getSystemInfo()
        //用户头像
        let img1 = vm.Avatar.replace(/^http:/, "https:") + "?x-oss-process=image/resize,l_100,image/circle,r_100/format,png";
        let userlogo = await tools.downFile(img1);
        // //二维码
        let img2 = data.dataObj.replace(/^http:/, "https:")
        let qrcode = await tools.downFile(img2)
        let bg = await tools.downFile("https://j.vzan.cc/miniapp/img/qxCard.png")
        let screenWidth = systemInfo.screenWidth * systemInfo.pixelRatio
        let screenHeight = systemInfo.screenHeight * systemInfo.pixelRatio
        let pixelRatio = systemInfo.pixelRatio
        let w = Math.floor(core.getRelativeValue(750, 630, screenWidth) / pixelRatio)
        let h = Math.floor(core.getRelativeValue(1334, 932, screenHeight) / pixelRatio)
        let ctx = wx.createCanvasContext('firstCanvas')



        ctx.drawImage(bg.tempFilePath, 0, 0, w, h)
        ctx.drawImage(userlogo.tempFilePath, w * 0.05, h * 0.03, w * 0.2, w * 0.2)
        ctx.drawImage(qrcode.tempFilePath, w * 0.2, h * 0.5, w * 0.6, w * 0.6)



        ctx.setFontSize(16)
        ctx.setFillStyle("#000000")
        ctx.fillText(vm.Name, w * 0.3, h * 0.09)

        ctx.setFontSize(13)
        ctx.setFillStyle("#777777")
        ctx.fillText(vm.DepartMentName + "-" + vm.Job, w * 0.3, h * 0.14)

        ctx.setFontSize(13)
        ctx.setFillStyle("#999999")
        ctx.fillText("手机", w * 0.05, h * 0.27)

        ctx.setFontSize(13)
        ctx.setFillStyle("#333")
        ctx.fillText(vm.Phone, w * 0.05, h * 0.32)

        ctx.setFontSize(13)
        ctx.setFillStyle("#999999")
        ctx.fillText("公司", w * 0.05, h * 0.4)

        ctx.setFontSize(13)
        ctx.setFillStyle("#333333")
        ctx.fillText(vm.CompanyName, w * 0.05, h * 0.45)




        ctx.draw()
        wx.hideLoading()
      } else {
        wx.hideLoading()
        await tools.showModal(data.Msg, false)
        tools.goBack(1)
      }
    })
  },

  getSystemInfo: function () {

    return new Promise((resolve) => {
      wx.getSystemInfo({
        success: (res) => {
          resolve(res)
        },
        fail: () => {
          resolve(false)
        }
      })
    })
  },
  //获取相对值
  getRelativeValue(mon, son, relMon) {
    var relSon = son * relMon / mon
    return Math.floor(relSon)
  },
  //名片列表
  GetMyListEmployee: async function (vm, target) {
    if (vm.ispost || vm.loadall)
      return
    if (!vm.ispost)
      vm.ispost = true
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.GetMyListEmployee, {
      aid,
      appId: app.globalData.appid,
      userId: userInfo.Id,
      sessionkey: userInfo.loginSessionKey,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
    }).then(data => {
      vm.ispost = false
      if (data.isok) {
        data.dataObj.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        data.dataObj.length > 0 ? vm.list = vm.list.concat(data.dataObj) : "";
        // 时间戳
        for (let i = 0, len = data.dataObj.length; i < len; i++) {
          data.dataObj[i].UpdateTime = tools.ChangeDateFormat(data.dataObj[i].UpdateTime)
        }
        target.vmEmp = vm
        target.$apply()
      }
    })
  },
  //绑定客服
  CustomerBindEmployee: async function () {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let data = await http.post(addr.CustomerBindEmployee, {
      userId: userInfo.Id,
      sessionkey: userInfo.loginSessionKey,
    })
    if (data.isok) {
      app.globalData.storeKefu = data.dataObj
      return data.dataObj
    } else {
      return ""
    }
  },
  /**
   * 
   * @param {}
   * @return {订单}
   */
  AddGoodsCarData: async function (vm) {

    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    return http.post(addr.AddGoodsCarData, {
      sessionkey: userInfo.loginSessionKey,
      appId: app.globalData.appid,
      attrSpacStr: vm.specId || '',
      specInfo: vm.specInfo || '',
      specImg: vm.specImg,
      goodId: vm.goodid,
      userId: userInfo.Id,
      qty: vm.count,
      gotoBuy: vm.gotoBuy
    })
  },
  // 设置地址
  EditUserAddress: async function (vm) {
    let userInfo = await core.getUserInfo()
    return http.post(addr.EditUserAddress, {
      id: vm.id,
      userid: userInfo.Id,
      isdefault: vm.isdefault,
      contact: vm.contact,
      phone: vm.phone,
      province: vm.province,
      city: vm.city,
      district: vm.district,
      street: vm.street,
    })
  },
  //添加地址
  getAddresslt: async function (target) {
    let userInfo = await core.getUserInfo()
    http.post(addr.GetUserAddress, {
      userId: userInfo.Id
    }).then(info => {
      if (info.isok) {
        let _g = info.data
        for (let i = 0, len = _g.length; i < len; i++) {
          _g[i].address = _g[i].province + _g[i].city + _g[i].district + _g[i].street
        }
        target.vm.express = _g
        target.$apply()
      } else {
        tools.showModalCancle(info.msg)
      }
    })
  },
  //删除地址
  delAddress: function (id) {
    return http.post(
      addr.DeleteUserAddress, {
        id: id
      })
  },
  //获取购物车商品
  GetGoodsCarList: async function (_v, target) {
    if (_v.ispost || _v.loadall)
      return;
    if (!_v.ispost)
      _v.ispost = true
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    http.post(addr.GetGoodsCarList, {
      sessionkey: userInfo.loginSessionKey,
      appId: app.globalData.appid,
      userId: userInfo.Id,
      pageIndex: _v.pageindex,
      pageSize: _v.pagesize,
    }).then(data => {
      _v.ispost = false
      if (data.isok) {
        data.dataObj.carList.length >= _v.pagesize ? _v.pageindex += 1 : _v.loadall = true;
        data.dataObj.carList.length > 0 ? _v.list = _v.list.concat(data.dataObj.carList) : "";
        for (let i = 0, len = data.dataObj.carList.length; i < len; i++) {
          data.dataObj.carList[i].sel = false
        }
      } else {
        _v.loadall = true;
        target.vm = _v
      }
      target.$apply()
    })
  },
  //删除更新购物车
  UpdateOrDeleteGoodsCarData: async function (vm, target) {
    let userInfo = await core.getUserInfo()

    http.post(addr.UpdateOrDeleteGoodsCarData, {
      sessionkey: userInfo.loginSessionKey,
      userId: userInfo.Id,
      type: vm.type, //0更新 -1删除
      cartStr: JSON.stringify(vm.cartStr)
    }).then(data => {
      if (data.isok) {
        tools.wxShowToast(data.Msg)
        tools.resetArray(target.vm)
        core.GetGoodsCarList(target.vm, target)
      }
    })
  },
  //获取运费
  GetFreightFee: async function (vm) {
    let userInfo = await core.getUserInfo()
    return http.post(addr.GetFreightFee, {
      sessionkey: userInfo.loginSessionKey,
      userId: userInfo.Id,
      province: vm.province,
      city: vm.city,
      goodCartIds: vm.ids
    })
  },
  //获取订单详情
  GetOrderInfo: async function (id, target) {
    let userInfo = await core.getUserInfo()
    http.post(addr.GetOrderInfo, {
      id,
      sessionkey: userInfo.loginSessionKey,
      userId: userInfo.Id,
    }).then(data => {
      if (data.isok) {
        data.dataObj.list = data.dataObj.CartList
        target.vm = data.dataObj
        target.$apply()
      }
    })
  },
  //订单操作
  UpdateOrderState: async function (id, state, target, type, o_type) {
    let userInfo = await core.getUserInfo()
    http.post(addr.UpdateOrderState, {
      id,
      state,
      userId: userInfo.Id,
      sessionkey: userInfo.loginSessionKey,
    }).then(data => {
      if (data.isok) {
        tools.showModal(data.Msg, false)
        if (type = 'confirm') {
          if (o_type == 'olt') {
            core.GetOrderInfo(id, target)
          } else {
            target.vm = tools.resetArray(target.vm)
            core.GetOrderList(target.vm, target)
          }
        } else {
          if (o_type == 'olt') {
            core.GetOrderInfo(id, target)
          } else {
            target.vm = tools.resetArray(target.vm)
            core.GetOrderList(target.vm, target)
          }
        }
      }
    })
  },
  //获取店铺
  GetStoreInfo: async function (target) {
    let app = wepy.$instance
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    let userInfo = await core.getUserInfo()
    http.post(addr.GetStoreInfo, {
      aid,
      sessionkey: userInfo.loginSessionKey,
    }).then(data => {
      if (data.isok) {
        target.storeInfo = data.dataObj
        target.storeInfo.name = ""
        target.storeInfo.phone = ""
        target.$apply()
      }
    })
  },
  /**
   * 
   * @param {state：-1已取消；0待付款 1待发货 2待自取 3待收货 4已完成}
   * @return {订单列表}
   */
  GetOrderList: async function (vm, target) {
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    let userInfo = await core.getUserInfo()
    http.post(addr.GetOrderList, {
      state: vm.state,
      userId: userInfo.Id,
      sessionkey: userInfo.loginSessionKey,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
    }).then(async data => {
      vm.ispost = false
      if (data.isok) {
        data.dataObj.list.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        data.dataObj.list.length > 0 ? vm.list = [...vm.list, ...data.dataObj.list] : "";
        target.vm = vm
        target.$apply()
      } else {
        await tools.showModal(data.Msg, false)
        tools.goBack(1)
      }
    })
  },
  /**
   * 
   * @param {}
   * @return {企业相关}
   */
  GetQiyeInfo: async function (target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.GetQiyeInfo, {
      aid,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
    }).then(data => {
      if (data.isok) {
        data.dataObj.content = data.dataObj.Description.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>').replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>').replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
        data.dataObj.content = wxParse('Description', 'html', data.dataObj.Description || "", target, 8);
        data.dataObj.Location = data.dataObj.Location.split(',')
        tools.setPageTitle(data.dataObj.StoreName)
        tools.setPageSkin("#000000", "#ffffff");
        target.vm = data.dataObj
        target.$apply()
        if (app.globalData.chat.index) {
          return
        }
        core.onShareUp('客户查看了官网，对公司很有兴趣哦')
        app.globalData.chat.index = true
      }
    })
  },
  GetDevelopmentDataList: async function (vm, target) {
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.GetDevelopmentDataList, {
      aid,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
      pageIndex: vm.pageindex || 1,
      pageSize: vm.pagesize || 9,
    }).then(data => {
      if (data.isok) {
        vm.ispost = false
        data.dataObj.data.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        data.dataObj.data.length > 0 ? vm.list = [...vm.list, ...data.dataObj.data] : "";
        vm.userInfo = userInfo
        target.dev = vm
        target.$apply()
      }
    })
  },
  // formId
  formId: async function (formid) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    http.post(addr.commitFormId, {
      appid: app.globalData.appid,
      openid: userInfo.OpenId,
      formid: formid
    })
  },
  /**
   * 
   * @param {GetCompanyNews列表   GetCompanyNewsDetail详情}
   * @return {咨询内容}
   */
  GetCompanyNews: async function (vm, target) {
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let aid = _get(app.globalData, "aid", "")
    if (aid == '') {
      aid = await core.getAid()
    }
    http.post(addr.GetCompanyNews, {
      aid,
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
      pageIndex: vm.pageindex || 1,
      pageSize: vm.pagesize || 9,
    }).then(data => {
      if (data.isok) {
        vm.ispost = false
        data.dataObj.data.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        data.dataObj.data.length > 0 ? vm.list = [...vm.list, ...data.dataObj.data] : "";

        target.vm = vm
        target.$apply()
      }
    })
  },
  GetCompanyNewsDetail: async function (id, target) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    http.post(addr.GetCompanyNewsDetail, {
      appId: app.globalData.appid,
      sessionkey: userInfo.loginSessionKey,
      id: id
    }).then(async data => {
      if (data.isok) {
        if (data.dataObj.Content) {
          data.dataObj.content = data.dataObj.Content.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>').replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>').replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
          data.dataObj.content = wxParse('content', 'html', data.dataObj.Content || "", target, 8);
        }
        target.vm = data.dataObj
        target.$apply()
        tools.setPageTitle(data.dataObj.Title)
      } else {
        await tools.showModal(data.Msg, false)
        tools.goBack(1)
      }
    })
  },

  /**
   * @param {私信}
   */
  EditeCustomerDesc: async function (vm, target) {
    let userInfo = await core.getUserInfo()
    http.post(addr.EditeCustomerDesc, {
      id: vm.id,
      desc: vm.desc,
      phoneDesc: vm.phone,
      sessionkey: userInfo.loginSessionKey,
    }).then(data => {
      if (data.isok) {
        tools.wxShowToast("成功")
        target.vmCustomer = tools.resetArray(target.vmCustomer)
        core.GetCustomerList(target.vmCustomer, target)
        target.showChange = false
        target.personId = 0
        target.$apply()
      }
    })
  },
  //聊天记录
  GetEmployeeMessage: async function (vm, target) {
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    let userInfo = await core.getUserInfo()
    http.post(addr.GetEmployeeMessage, {
      userId: userInfo.Id,
      sessionkey: userInfo.loginSessionKey,
      name: vm.name || "",
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
    }).then(data => {
      vm.ispost = false
      if (data.isok) {

        data.dataObj.list.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        data.dataObj.list.length > 0 ? vm.list = [...vm.list, ...data.dataObj.list] : "";
        target.vm = vm
        target.$apply()
      }
    })
  },
  connectSocket: async function () {
    var that = this;
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '')
    if (userInfo == '') {
      userInfo = await core.getUserInfo()
    }
    var globaldata = app.globalData;
    var appid = globaldata.appid || "";
    var fuserid = userInfo.Id || ""

    if (appid == "" || fuserid == "")
      return;

    if (globaldata.ws || isConnecting)
      return;

    isConnecting = true;

    wx.connectSocket({
      //fuserType：用户身份  0：普通用户 2：商家
      url: 'wss://dzwss.xiaochengxu.com.cn?appId=' + appid + '&userId=' + fuserid + '&isFirst=' + isFirst,
      header: {
        'content-type': 'application/json'
      },
      method: "GET"
    });
    wx.onSocketOpen(function (res) {
      globaldata.ws = true;
      isConnecting = false;

      if (reConnectTimer) {
        clearTimeout(reConnectTimer);
        reConnectTimer = null;
      }

      //重连后，自动重发发送失败的消息
      for (var i = 0, len = globaldata.msgQueue.length; i < len; i++) {
        that.sendMessage(globaldata.msgQueue[i])
      }

      globaldata.msgQueue = [];
    });

    wx.onSocketError(function (res) {
      globaldata.ws = false;
      isConnecting = false;
    });

    wx.onSocketClose(function (res) {
      isFirst = false;
      globaldata.ws = false;
      isConnecting = false;
      core.reConnect();
    });

    //接收消息
    wx.onSocketMessage(function (res) {
      var msg = res.data

      if (typeof msg == "string") {
        msg = JSON.parse(msg);
      }
      let currentPage = getCurrentPages()[getCurrentPages().length - 1]
      let pageRoute = currentPage.route
      if (pageRoute == 'pages/index/index' || pageRoute == 'pages/goods/goods' || pageRoute == 'pages/news/news' || pageRoute == 'pages/card/card') {
        if (msg.msgType == 0) {
          msg.showMask = true
          currentPage.setData({
            chatLst: msg
          })
          clearTimeout(settime)
          settime = setTimeout(() => {
            msg.showMask = false
            currentPage.setData({
              chatLst: msg
            })
          }, 10000);
          return;
        }
        if (msg.msgType == 1) {
          msg.showMask = true
          currentPage.setData({
            chatLst: msg
          })
          clearTimeout(settime)
          settime = setTimeout(() => {
            msg.showMask = false
            currentPage.setData({
              chatLst: msg
            })
          }, 10000);
          return;
        }
        if (msg.msgType == 4 && msg.fuserId != userInfo.Id) {
          msg.showMask = true
          currentPage.setData({
            chatLst: msg
          })
          clearTimeout(settime)
          settime = setTimeout(() => {
            msg.showMask = false
            currentPage.setData({
              chatLst: msg
            })
          }, 10000);
          return;
        }
        if (msg.msgType == 3 && msg.fuserId != userInfo.Id) {
          msg.showMask = true
          currentPage.setData({
            chatLst: msg
          })
          clearTimeout(settime)
          settime = setTimeout(() => {
            msg.showMask = false
            currentPage.setData({
              chatLst: msg
            })
          }, 10000);
          core.sendMsg(fuserid, msg.fuserId, "您好，请问有什么可以帮到您？", 4)
        }
      }
      if (pageRoute == "pages/chat/chatLine") {
        var fuser = currentPage.data.fuserInfo;
        var tuser = currentPage.data.tuserInfo;
        var list = currentPage.data.vm.list;

        //如果消息是当前联系人发来的
        if (msg.fuserId == fuser.userid && msg.tuserId == tuser.userid || msg.fuserId == tuser.userid && msg.tuserId == fuser.userid) { //发给我的

          list.push(msg);
          currentPage.setData({
            "vm.list": list,
            "vm.lastids": msg.ids,
          });
        }
      }



    })
  },
  reConnect: function () {
    if (reConnectTimer) {
      clearTimeout(reConnectTimer);
      reConnectTimer = null;
    }
    reConnectTimer = setTimeout(function () {
      core.connectSocket();
    }, 3000);
  }, //发
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

  changeunreadmsg: function (unreadmsg, unreadmsgcount) {
    var app = wepy.$instance
    app.globalData.unreadmsg = unreadmsg
    app.globalData.unreadmsgcount = unreadmsgcount;
    wx.setStorageSync("unreadmsg", unreadmsg)
    wx.setStorageSync("unreadmsgcount", unreadmsgcount)
  },
  AddContact: async function (userid) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '')
    if (userInfo == '') {
      userInfo = await core.getUserInfo()
    }
    http.post(addr.AddContact, {
      appId: app.globalData.appid,
      fuserId: userInfo.Id,
      tuserId: userid
    })
  },
  getHistory: async function (userid, vm, targetPage) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '')
    if (userInfo == '') {
      userInfo = await core.getUserInfo()
    }
    if (vm.ispost || vm.loadall)
      return;
    vm.ispost = true;
    let info = await http.post(addr.GetHistory, {
      appId: app.globalData.appid,
      fuserId: userInfo.Id,
      tuserId: userid,
      id: vm.lastid,
      fuserType: 0,
      ver: 1,
    })
    if (info && info.isok) {
      info.data.length < vm.pagesize ? vm.loadall = true : vm.loadall = false;
    
      if (info.data.length > 0) {
        vm.list = [...info.data, ...vm.list]
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
  },
  gochat: async function (vm) {
    tools.goNewPage('/pages/chat/chatLine?userid=' + vm.id + "&nickname=" + vm.name + "&headimg=" + vm.img)
  },
  sendMsg(myuserid, orderuserid, msg, msgtype, isChat) {
    var app = wepy.$instance;
    var ws = _get(app, "globalData.ws") || false;
    var appid = _get(app, "globalData.appid");
    if (!ws) {
      core.reConnect()
    }
    /**
     * @param {msgType 0 文字，1 图片}
     * @param {fuserId 自己id}
     * @param {tuserId 对方id}
     */
    var msg = {
      appId: appid,
      fuserId: myuserid,
      tuserId: orderuserid,
      enterPage: "pages/chat/chatLine",
      msgType: msgtype,
      tuserType: 0,
      msg,
      ids: "",
      tempid: myuserid + '_' + new Date().getTime(), //临时ID用来判断消息是否发送成功
      isChat: isChat || 0,
    };
    wx.sendSocketMessage({
      data: JSON.stringify(msg),
      success: function () {},
      fail: function (res) {
        app.globalData.msgQueue.push(msg);
      },
      complete: function () {}
    })
  },
  // 系统消息
  async onShareUp(msg) {
    let app = wepy.$instance
    let o_userid = _get(app.globalData, 'storeKefu', "")
    let userInfo = _get(app.globalData, "userInfo", "")

    if (o_userid == '') {
      o_userid = await core.CustomerBindEmployee()
    }
    if (userInfo == '') {
      userInfo = await core.getUserInfo()
    }
    if (o_userid != null && userInfo.Id != o_userid.UserId && o_userid.UserId != 0) {
      core.sendMsg(userInfo.Id, o_userid.UserId, msg, 3)
    }
  },

};

var pro = {
  resetPro(_v) {
    _v.itemPrice = parseFloat(_v.Price).toFixed(2)
    _v.danMaiPrice = parseFloat(_v.Price).toFixed(2)
    _v.yuanJiaPrice = _v.OriginalPriceStr
    _v.specImg = _v.Img
    _v.stock_new = _v.Stock
    _v.count = 1
    if (_v.PickSpec.length) {
      for (let i = 0, len = _v.PickSpec.length; i < len; i++) {
        for (let j = 0, key = _v.PickSpec[i].items.length; j < key; j++) {
          _v.PickSpec[i].items[j].sel = false
        }
      }
    }
    return _v
  },
  choosePro(_v, _pindex, _cindex) {
    let specId = []
    let specInfo = []
    let pick = _v.PickSpec
    let spec = _v.GASDetailList
    let [currentList, self] = [pick[_pindex], pick[_pindex].items[_cindex]]
    if (currentList.items.length > 0) {
      currentList.items.forEach(function (obj, i) {
        obj.Id != self.Id ? obj.sel = false : obj.sel = !obj.sel;
      })
    }
    for (let i = 0, len = pick.length; i < len; i++) {
      for (let j = 0, key = pick[i].items.length; j < key; j++) {
        if (pick[i].items[j].sel) {
          let [parentName, childName] = [pick[i].Name, pick[i].items[j].Name]
          let specName = parentName + ":" + childName
          specId.push(pick[i].items[j].Id)
          specInfo.push(specName)
        }
      }
    }
    _v.specId = specId.join("_")
    _v.specInfo = specInfo.join(" ")
    _v.count = 1
    let specTemp = spec.find(f => f.Id == _v.specId)
    if (specTemp) {
      _v.itemPrice = parseFloat(specTemp.Price).toFixed(2)
      _v.danMaiPrice = parseFloat(specTemp.Price).toFixed(2)
      _v.yuanJiaPrice = parseFloat(specTemp.OriginalPrice).toFixed(2)
      _v.specImg = specTemp.ImgUrl || _v.Img
      _v.stock_new = specTemp.Stock
    } else {
      _v.itemPrice = parseFloat(_v.Price).toFixed(2)
      _v.danMaiPrice = parseFloat(_v.Price).toFixed(2)
      _v.yuanJiaPrice = parseFloat(_v.OriginalPrice).toFixed(2)
      _v.specImg = _v.Img
      _v.stock_new = _v.Stock
    }
    _v.PickSpec[_pindex] = currentList
    return _v;
  },
  addPro(_v, target) {
    let count = _v.count
    if (_v.PickSpec.length) {
      let specTemp = _v.GASDetailList.find(f => f.Id == _v.specId)
      if (specTemp == undefined) {
        tools.ShowToast("请先选择规格", target)
        count = 1
        return count;
      } else {
        if (_v.StockLimit) {
          if (count < specTemp.Stock) {
            count++;
          } else {
            tools.ShowToast("库存不足", target)
          }
        } else {
          count++
        }
      }
    } else {
      if (_v.StockLimit) {
        if (count < _v.Stock) {
          count++;
        } else {
          tools.ShowToast("库存不足", target)
        }
      } else {
        count++
      }
    }
    return count;
  },
  lessPro(_v, target) {
    let count = _v.count
    if (count > 1) {
      count--
    } else {
      count = 1
    }
    return count;
  },
  setPro(_v, num, target) {
    let count = _v.count
    if (_v.PickSpec.length) {
      let specTemp = _v.GASDetailList.find(f => f.Id == _v.specId)
      if (specTemp == undefined) {
        tools.ShowToast("请先选择规格", target)
        count = 1
        return count;
      } else {
        if (_v.StockLimit) {
          if (num <= specTemp.Stock) {
            count = num;
          } else {
            count = 1
            tools.ShowToast("库存不足", target)
          }
        } else {
          count = num;
        }
      }
    } else {
      if (_v.StockLimit) {
        if (num <= _v.Stock) {
          count = num;
        } else {
          count = 1
          tools.ShowToast("库存不足", target)
        }
      } else {
        count = num;
      }
    }
    return count;
  }
};
var pay = {
  /**
   * @param {paytype:1微信支付 2储值支付 ；jsondata支付信息string；buyprice：支付价格}
   */
  AddPayOrder: async function (vm) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let info = await http.post(addr.AddPayOrder, {
      sessionkey: userInfo.loginSessionKey,
      userId: userInfo.Id,
      appId: app.globalData.appid,
      ordertype: 3001024,
      paytype: vm.paytype,
      jsondata: JSON.stringify(vm.jsondata),
      buyprice: vm.price,
    })
    if (info.isok) {
      await pay.payInfo(info.dataObj.orderid, info.dataObj.dbOrder)
    } else {
      tools.showModal(info.Msg, false)
    }
  },
  payInfo: async function (orderid, dbOrder, type) {
    let app = wepy.$instance
    let aid = _get(app.globalData, 'aid', '')
    if (aid == '') {
      aid = await core.getAid()
    }
    let userInfo = _get(app.globalData, 'userInfo', '')
    if (userInfo == '') {
      userInfo = await core.getUserInfo()
    }
    http.post(addr.PayOrder, {
      aid,
      orderid,
      openId: userInfo.OpenId,
      'type': '1',
    }).then(async data => {
      wx.showNavigationBarLoading()
      data.obj = JSON.parse(data.obj)
      let _pay = await pay.wxpay(data.obj)
      if (_pay.errMsg == "requestPayment:ok") {
        tools.loading('支付成功')
      } else {
        tools.loading('您取消了支付')
      }
      setTimeout(() => {
        if (type == 'lst') {
          tools.goNewPage("/pages/goods/goodsOlt?id=" + dbOrder)
        } else {
          tools.goRedirecto("/pages/goods/goodsOlt?id=" + dbOrder)
        }
      }, 1000);
      wx.hideNavigationBarLoading()
    })
  },
  /* 支付   */
  wxpay: function (param) {
    let app = wepy.$instance
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
}


module.exports = {
  tools,
  core,
  pro,
  pay
}
