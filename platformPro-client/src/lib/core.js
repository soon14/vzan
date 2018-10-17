import wepy from "wepy";
import addr from "./addr.js"
import _get from './lodash.get';
import {
  wxParse
} from './wxParse/wxParse';
var isEndClock = null;
var timer_countdown = null;
var isdebug = false;
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


/**********************************封装工具类**********************************************************/
var tools = {
  //设置主题颜色
  async setThemeColor(vm) {
    var cacheTheme = this.cacheGlobalData('theme')
    if (cacheTheme) {
      this.setBarColor(cacheTheme.color, cacheTheme.bgcolor)
      vm.currentSkin = cacheTheme.type
      return
    }
    var theme = null //匹配skinList元素
    var result = await core.getStoreDetail()
    this.handleResult(result, (res) => { 
      var color = res.platStore.ColorTxt
      theme = skinList.find((val) => {
        return val.bgcolor == color
      })
      console.log(theme)
      if (!theme) this.freeToast('匹配不到主题色', 'none')
      this.setBarColor(theme.color, theme.bgcolor)
    })
    this.cacheGlobalData('theme', theme)
    vm.currentSkin = theme.type
    vm.$apply()
  },
  //设置底头颜色
  setBarColor(color, bgColor) {
    wx.setNavigationBarColor({
      backgroundColor: bgColor,
      frontColor: color
    })
  },
  loadMoreData(vm, pageSize) {
    var concat_ = function (targetKey, src) {
      pageSize = pageSize || 8
      if (src.length > 0) {
        vm[targetKey] = vm[targetKey].concat(src)
        vm.pageIndex > 1 && src.length < pageSize && (vm.loadAll = true)
      } else {
        vm.pageIndex > 1 && (vm.loadAll = true)
      }
    }

    var handleReachBottom = async function (countKey, cb) {
      if (vm[countKey] > pageSize && !vm.loadAll) {
        vm.loadMore = true
        vm.pageIndex++
          await cb.call(vm)
        vm.loadMore = false
        vm.$apply()
      }
    }

    return {
      concat_,
      handleReachBottom
    }
  },
  /**
   * 缓存全局数据， 传单个key返回缓存值， 传key，value记忆缓存新值
   * @param {String} key 
   * @param {Mix} value 
   */
  cacheGlobalData(key, value) {
    var gloabal = wepy.$instance.globalData
    return gloabal[key] || (gloabal[key] = value)
  },
  /**
   * 输出结果控制流程
   * @param {Object} result 
   * @param {Function} callback 
   */
  handleResult(result, callback, errCallBack) {
    //新接口
    if ('isok' in result) {
      result.isok ?
        typeof callback === 'function' && callback(result.dataObj) :
        (
          typeof errCallBack === 'function' && errCallBack(result),
          console.log(result.Msg, '接口错误信息')
        )
    }
    //旧接口
    if ('errcode' in result) {
      Number(result.errcode) <= -1 ?
        (
          typeof errCallBack === 'function' && errCallBack(),
          console.log(result.msg, '接收拦截器错误信息')
        ) :
        typeof callback === 'function' && callback(result)
    }
  },
  //回调函数必须是一个箭头函数
  handleRegister(flag, success, failure) {
    if (flag) {
      typeof success === 'function' && success()
    } else {
      typeof failure === 'function' && failure()
      wx.setStorageSync('need-register', true)
      wx.switchTab({
        url: '/pages/join/join-index/index'
      })
    }
  },
  //检查用户是否注册或完善名片
  async checkRegister() {
    var isRegister = false
    var isCompleteCard = false
    var cacheResult = wx.getStorageSync('check-register')
    if (cacheResult) {
      return cacheResult
    }
    var result = await core.getMyCard();
    tools.handleResult(result, res => {
      isRegister = true;
      res.Address && (isCompleteCard = true)
    });
    result = {
      isRegister,
      isCompleteCard
    }
    wx.setStorageSync('check-register', result)
    return result
  },
  getLocation() {
    return new Promise(resolve => {
      wx.getLocation({
        success: (res) => {
          resolve(res)
        },
        fail: async () => {
          tools.showModalCancle('获取位置信息失败')
        }
      })
    })
  },
  //扩展一个对象
  extend(target, src) {
    for (var key in src) {
      target[key] = src[key]
    }
    return target
  },
  // 检查更新
  updateMiniapp() {
    const _update = wx.getUpdateManager()
    _update.onCheckForUpdate(function (res) {
      console.log(res.hasUpdate)
    })
    _update.onUpdateReady(function () {
      wx.showModal({
        title: '更新提示',
        content: '新版本已经准备好，是否重启应用？',
        success: function (res) {
          if (res.confirm) {
            // 新的版本已经下载好，调用 applyUpdate 应用新版本并重启
            _update.applyUpdate()
          }
        }
      })
    })
    _update.onUpdateFailed(function () {
      // 新的版本下载失败
      wx.showModal({
        title: '更新提示',
        content: '新版本下载失败，请手动删除小程序',
        showCancel: false
      })
    })
  },
  //微信版本检查
  getSystem: function () {
    let ver1 = parseFloat(wx.getSystemInfoSync().SDKVersion)
    let ver2 = 1.5
    if (ver1 < ver2 || wx.getSystemInfoSync().SDKVersion == undefined) {
      wx.showModal({
        title: '提示',
        content: '当前微信版本过低，无法使用该功能，请升级到最新微信版本后重试',
        showCancel: false,
        success(res) {}
      })
    }
  },
  // 重置数组
  resetArray: function (vm) {
    Object.assign(vm, {
      ispost: false,
      loadall: false,
      list: [],
      pageindex: 1
    })
    return vm
  },
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
              tools.showLoadingNo('上传中...')
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
  uploadImg(tempImg, index) {
    return new Promise(function (resolve, reject) {
      wx.uploadFile({
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
        }
      })
    });
  },
  /**
   urltype
   -1 点击图片放大
   0  跳转 target：_blank 新页面打开  _self 当前页面打开
   1  跳转小程序
   2 跳转功能 furl:-1时点击放大 string路径时跳转  furl：4扫码功能（根据url来跳转)
   3 跳转产品详情页
   4 跳转产品列表组件
   5 跳转拼团详情页
   6 跳转砍价详情页
   7 跳转团购详情页
   */
  imgPlay: async function (type, _g, targetPage) {
    let that = targetPage
    let vm = _get(that.$parent, "vm_goodsList")
    vm.loadall = false
    vm.ispost = false
    vm.pageindex = 1
    that.$parent.vm_goodsList = vm
    that.$apply()
    let current = _g.imgurl || _g.img;
    if (_g.urltype == undefined && (type == 'bottomnav' || type == "imgnav")) {
      if (_g.url == -1) {
        that.$emit("ShowToast", "未设置跳转")
        return;
      }
      that.$emit("bottomGo", _g.url)
      return;
    }
    if (_g.urltype == undefined && (type == 'img' || type == "slider" || type == 'magicCube')) {
      tools.preViewShow(current)
      return;
    }
    let _p = await core.getPageSetting()
    switch (_g.urltype) {
      case -1:
        if (type == 'img' || type == 'slider' || type == 'magicCube') {
          tools.preViewShow(current);
        } else {
          that.$emit("ShowToast", "未设置跳转")
        }
        break;
      case 0:
        if (_g.url == -1) {
          that.$emit("ShowToast", "未设置跳转")
        } else {
          if (_g.target == '_blank') {
            if (_p.pages[_g.url].coms[0].type == "goodlist") {
              that.$emit("goodProGo", _p.pages[_g.url])
            } else {
              if (getCurrentPages().length >= 2) {

                tools.goRedirecto("/pages/index/index?currentPageIndex=" + _g.url)
              } else {
                tools.goNewPage("/pages/index/index?currentPageIndex=" + _g.url)
              }
            }
          } else {
            that.$emit("bottomGo", _g.url)
          }
        }
        break;
      case 1:
        let item = {
          appid: _g.appid,
          path: _g.path,
        }
        tools.goNewMiniapp(item)
        break;
      case 2:
        if (_g.furl == -1) {
          if (type == 'img' || type == 'slider' || type == 'magicCube') {
            tools.preViewShow(current);
          } else {
            that.$emit("ShowToast", "未设置跳转")
          }
          return;
        }
        if (_g.furl != '' && typeof (_g.furl) == 'string' && _g.url == -1) {
          tools.goNewPage("/" + _g.furl)
          return;
        }
        if (_g.furl == 4) {
          tools.sceneQrcode(Number(_g.url))
          return;
        }
        break;
      case 3:
        if (_g.items.length == 0) {
          that.$emit("ShowToast", "未设置产品")
          return;
        }
        let goodpara = {
          id: _g.items[0].id,
          btn: _g.btnType,
          showprice: _g.items[0].showprice
        }
        tools.goNewPage("/pages/good/good?para=" + JSON.stringify(goodpara))
        break;
      case 4:
        if (_g.itemstype.length == 0) {
          if (type == 'img' || type == 'slider' || type == 'magicCube') {
            tools.preViewShow(current);
          } else {
            that.$emit("ShowToast", "未设置跳转")
          }
          return;
        }
        let key = _p.pages
        let _more = key.some(k => k.coms[0].type == 'goodlist')
        if (_more === false) {
          that.$emit("ShowToast", "当前小程序未设置产品列表")
          return;
        }

        let array = tools.resetNomore(key, _g)
        if (array.length) {
          array[0].coms[0].goodCatId = _g.itemstype[0].id
          that.$emit("goodProGo", array[0])
        } else {
          that.$emit("ShowToast", "产品列表暂无该分类")
        }

        break;
      case 5:
        let g2url = "/pages/group2/group2?id="
        tools.pathGo(_g, g2url, that)
        break;
      case 6:
        let burl = "/pages/bargain/bargain?id="
        tools.pathGo(_g, burl, that)
        break;
      case 7:
        let gurl = "/pages/group/group?id="
        tools.pathGo(_g, gurl, that)
        break;
    }
  },
  // 去重
  resetNomore: function (array, _g) {
    let n = [];
    for (let i = 0; i < array.length; i++) {
      if (n.indexOf(array[i]) == -1 && array[i].coms[0].type == "goodlist") {
        let _val = array[i].coms[0].goodCat.some(k => k.id == _g.itemstype[0].id)
        if (_val) {
          n.push(array[i]);
        }
      }
    }
    return n;
  },
  pathGo: function (_g, url, targetPage) {
    if (_g.itemstype.length == 0) {
      targetPage.$emit("ShowToast", "未设置跳转")
      return;
    }
    tools.goNewPage(url + _g.itemstype[0].id)
  },
  //主题色改变
  setPageSkin: async function (targetPage) {
    let currentPage = await core.getStorage("pages");
    if (currentPage == "") {
      currentPage = await core.getPageSetting()
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
  //倒计时
  TimeShow: function (startDateStr, endDateStr, type) {
    let [starShow, endShow, timeInter] = [false, false, "00:00:00"]
    let end = ""
    let star = ""
    if (type == 'bargain') {
      end = endDateStr.replace(/-/g, "/")
      star = startDateStr.replace(/-/g, "/")
    } else {
      end = endDateStr.replace(/[.]/g, "/")
      star = startDateStr.replace(/[.]/g, "/")
    }
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
      tools.loading("未设置电话")
    }
  },

  //跳转小程序
  goNewMiniapp: function (item) {
    wx.navigateToMiniProgram({
      appId: item.appid,
      path: item.path,
      success(res) {
        // console.log(res)
      },
      fail(err) {
        tools.showModalCancle("跳转失败")
      }
    })
  },
  //扫码
  sceneQrcode: function (url) {
    wx.scanCode({
      onlyFromCamera: true,
      success: async res => {
        // console.log(res)
        if (res.path == undefined) {
          tools.showModalCancle("亲，该二维码有误")

        } else { //扫码成功操作
          await tools.wxToast('扫码成功')
          if (url != -1) {
            tools.goRedirecto("/pages/index/index?currentPageIndex=" + url)
          }

        }
      }
    })
  },
  //页面回到顶部
  onPageScroll: function (duration) {
    wx.pageScrollTo({
      scrollTop: 0,
      duration: duration || 0
    });
  },
  //动态改顶部兰标题
  setPageTitle: function (tmpTitle) {
    wx.setNavigationBarTitle({
      title: tmpTitle,
    });
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
  showLoading: function (msg) {
    wx.showLoading({
      title: msg || '加载中...',
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

  //跳转tabbar页
  goTabBar: function (url) {
    wx.switchTab({
      url
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
            tools.wxToast("复制成功")
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
  //自定义弹窗
  freeToast: function (msg, icon, duration) {
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
  // 上传图片
  chooseImg() {
    return new Promise(function (resolve, reject) {


      wx.chooseImage({
        count: 8, // 默认9
        sizeType: ["original", "compressed"], // 可以指定是原图还是压缩图，默认二者都有
        sourceType: ['album', 'camera'], // 可以指定来源是相册还是相机，默认二者都有
        success: async function (res) {
          // 返回选定照片的本地文件路径列表，tempFilePath可以作为img标签的src属性显示图片
          resolve(res)
        }
      })
    })
  },
  //获取图片信息
  getImageInfo(src) {
    return new Promise(resolve => {
      wx.getImageInfo({
        src: src,
        success: (res) => {
          resolve(res.path)
        },
        fail: () => {
          console.log('获取图片信息失败')
        }
      })
    })
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
        openId: userInfo.OpenId,
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
  //控制微信支付逻辑
  async handleWxPay(option_) {
    //default settings
    var option = {
      orderid: 0,
      success: null,
      fail: null
    };

    //an argument of object for PayOrder
    var params = {}

    //extend
    option = tools.extend(option, option_)

    //ready for PayOrder
    params.orderid = option.orderid;
    params["type"] = 1;

    //get the data Object for wxpay
    var data = await pay.PayOrder(params)
    params = JSON.parse(data.obj)
    // get the result of wxpay
    var result = await pay.wxpay(params)
    //handle wxpay log
    if (result.errMsg == "requestPayment:ok") {
      //success callback tips: must use arrow function
      typeof option.success === 'function' && option.success(data.extdata)
    } else {
      //fail callback tips: must use arrow function
      typeof option.fail === 'function' && option.fail()
    }
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
    let priceStr = (Number(shouldPay).sub(Number(joinPrice))).toFixed(2)
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
            // isdebug && console.log(e);
            resolve("");
          }
        }
      }))
    });
  },

};
/**********************************各接口请求**********************************************************/
var core = {
  //检查登陆状态
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
      // _g.dataObj.userid = _g.dataObj.Id
      // _g.dataObj.openId = _g.dataObj.OpenId
      // _g.dataObj.avatarUrl = _g.dataObj.HeadImgUrl
      // _g.dataObj.nickName = _g.dataObj.NickName
      return _g.dataObj;
    }
    return '';
  },
  getUserInfo: async function (forceRefresh) {
    let userInfo = wepy.$instance.globalData._userInfo_;
    let isSessionValid = await core.checkSession();
    if (isSessionValid && userInfo && !forceRefresh) {
      return userInfo;
    } else {
      let code = await core.login();
      if (code) {
        let info = await core.wxLogin(code);
        wepy.$instance.globalData._userInfo_ = info
        return info;
      }
      return "";
    }
  },

  openSetting: function () {
    wx.showModal({
      title: "提示",
      confirmText: "去授权",
      showCancel: false,
      content: "未授权，请先授权",
      success: function (res) {
        // isdebug && console.log(res.confirm);
        if (res.confirm) {
          wx.openSetting({
            success: function (setres) {
              wx.hideLoading()
            }
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
    let authSetting = await core.getSetting(scope);
    if (!authSetting[scope]) {
      return false
    } else {
      return true;
    }

  },
  /**********************************封装wx end**********************************************************/
  getAid: async function () {
    var aid = await core.getStorage("aid");
    var app = wepy.$instance;
    var appid = app.globalData.appid;
    if (!aid) {
      var aidInfo = await http.post(addr.Getaid, {
        appid
      });
      if (aidInfo && aidInfo.isok) {
        try {
          core.setStorage("aid", aidInfo.msg)
        } catch (e) {}
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
    let aid = await core.getAid();
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
    let aid = await core.getAid();
    let updatetimeInfo = await http.post(addr.GetPageSettingUpdateTime, {
      aid
    });
    if (updatetimeInfo && updatetimeInfo.isok)
      return updatetimeInfo.msg;
    else
      return new Date().getTime();
  },
  getPageSetting: async function () {
    let app = wepy.$instance
    let pages = app.globalData.pages

    let updatetime = await core.getPageSettingUpdateTime();
    if (pages == "" || pages.updatetime != updatetime) {
      pages = await core.getPageConfig()
      if (pages && pages.isok) {
        if (typeof pages.msg.pages == "string") {
          pages.msg.pages = JSON.parse(pages.msg.pages);
        }
        app.globalData.pages = pages.msg
        return pages.msg;
      }
      return "";
    } else {
      return pages;
    }

  },
  getGoodsByids: async function (ids, com) {
    let app = wepy.$instance;
    let ShowType = com.goodShowType || ""
    let levelid = 0;
    let vipInfo = await core.getVipInfo();
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
    let aid = await core.getAid();
    return http.post(
      addr.GetGroupByIds, {
        ids,
        aid
      });
  },
  //重组数据 
  resetMsgTypeList(dataList, number) {
    let list = [];
    let count = number; //每页需要几个图标
    let pageSize = Math.ceil(dataList.length / count);
    if (pageSize == 1) {
      list.push(dataList);
    } else if (pageSize > 1) {
      for (let i = 0; i < pageSize; i++) {
        let arr = [];
        let start = i * count;
        let end = (i + 1) * count;
        arr = dataList.slice(start, end);
        list.push(arr);
      }
    }
    return list;
  },
  //选择节点
  selectNode(selector) {
    let query = wx.createSelectorQuery();
    query.select(selector).boundingClientRect();
    return new Promise(function (resolve, reject) {
      query.exec(function (res) {
        resolve(res);
      })
    })
  },
  /************************************************产品********************************************************/
  //获得店铺详情
  getStoreDetail: async function () {
    var appId = wepy.$instance.globalData.appid;
    var user = await core.getUserInfo();
    return http.get(addr.GetStoreDetail, {
      appId,
      //userId: userInfo.Id,
      // myCardId,
      sessionKey: user.loginSessionKey,
      type: 2,
    })
  },
  //更新 帖子/名片/评论/商品的 浏览量 收藏数 分享数量以及新增用户的收藏
  async countUpData(id, actionType, dataType, aid) {
    let app = wepy.$instance;
    let user = await core.getUserInfo()
    return http.post(addr.AddFavorite, {
      aid: aid,
      userId: user.Id,
      sessionKey: user.loginSessionKey,
      appId: app.globalData.appid,
      othercardid: id, //帖子或名片id
      actionType: actionType, //功能类型 收藏 = 0, 点赞 = 1, 关注 = 2, 看过 = 3, 私信 = 4
      datatype: dataType //数据类型 帖子 = 0, 商品 = 1, 评论 = 2, 名片=3,店铺 = 4
    })
  },
  //我也要做小程序
  GetAgentConfigInfo: function () {
    let appid = wepy.$instance.globalData.appid;
    return http.post(addr.GetAgentConfigInfo, {
      appId: appid,
    })
  },
  //产品类别
  async GetGoodsCategoryLevel() {
    let app = wepy.$instance;
    let user = await core.getUserInfo()
    return new Promise(function (resolve, reject) {
      http.get(addr.GetGoodsCategoryLevel, {
        appId: app.globalData.appid,
        sessionKey: user.loginSessionKey,
      }).then(data => {
        if (data.isok) {
          resolve(data.dataObj.Level);
        } else {
          tools.showModal(data.Msg);
          reject();
        }
      })
    })
  },
  //产品对应类别的分类
  async GetGoodsCategory(_param) {
    let appid = wepy.$instance.globalData.appid;
    let user = await core.getUserInfo()
    let defaultParam = {
      appId: appid,
      sessionKey: user.loginSessionKey,
      isFirstType: 0, //0:大类,1:小类,其他:选单独大类时填写
      parentId: "", //单独大类的id
    }
    let param = tools.extend(defaultParam, _param);
    //debugger
    return http.get(addr.GetGoodsCategory, param);
  },
  //获取产品列表
  async GetGoodsList(_param) {
    let appId = wepy.$instance.globalData.appid;
    let aid = await core.getAid();
    let user = await core.getUserInfo()
    let defaultParam = {
      appId,
      aid,
      sessionKey: user.loginSessionKey,
      typeid: "", //String 大类id的集合 | 小类id的集合
      isFirstType: "", //大类传0,小类不传
      pricesort: "", //是否按价格排序asc|desc
      search: "", //搜索的关键字
    }
    let param = tools.extend(defaultParam, _param);
    return http.get(addr.GetGoodsList, param);
  },
  //获取产品详情
  async GetGoodInfo(pid) {
    let appId = wepy.$instance.globalData.appid;
    let user = await core.getUserInfo();
    return http.get(addr.GetGoodInfo, {
      appId,
      userId: user.Id,
      sessionKey: user.loginSessionKey,
      pid,
    })
  },
  /************************************************私信************************************************** */
  connectSocket: async function () {
    var that = this;
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    var globaldata = app.globalData;
    var appid = app.globalData.appid || "";
    var fuserid = userInfo.Id || ""

    console.log(fuserid)

    if (appid == "" || fuserid == "")
      return;

    if (globaldata.ws || isConnecting)
      return;

    isConnecting = true;

    wx.connectSocket({
      //fuserType：用户身份  0：普通用户 2：商家
      url: 'wss://dzwss.xiaochengxu.com.cn?appId=' + appid + '&userId=' + fuserid + '&fuserType=' + 0,
      header: {
        'content-type': 'application/json'
      },
      method: "GET"
    });

    console.log("ws connecting...");

    wx.onSocketOpen(function (res) {
      // console.log('ws is open', res);
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
      if (currentPage.route == "pages/connected/private-letter/index") {
        var list = currentPage.data.vm.list;
        console.log('接收')
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
    // console.log("开始重连");
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
      console.log('发送')
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
    core.setStorage("unreadmsg", unreadmsg)
    core.setStorage("unreadmsgcount", unreadmsgcount)
  },
  /**
   * 获得联系列表
   * @param {Number} pageindex 页码
   * @param {Number} pagesize 条目
   */
  getContactList: async function (pageindex, pagesize) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    return http.post(addr.GetContactList, {
      appId: app.globalData.appid,
      fuserId: userInfo.Id,
      pageIndex: pageindex || 1,
      pageSize: pagesize || 5,
      fuserType: 0,
      ver: 1,
    })
  },
  /**
   * 增加联系人
   * @param {Number} userid 他人的userid
   */
  addContact: async function (userid) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    return http.post(addr.AddContact, {
      appId: app.globalData.appid,
      fuserId: userInfo.Id,
      tuserId: userid
    })
  },
  /**
   * 获得聊天记录
   * @param {Number} userid 他人的userid
   * @param {String} lastid 最新一条标记
   */
  getHistory: async function (userid, lastid) {
    let app = wepy.$instance
    let userInfo = await core.getUserInfo()
    let info = await http.post(addr.GetHistory, {
      appId: app.globalData.appid,
      fuserId: userInfo.Id,
      tuserId: userid,
      id: lastid || 0,
      fuserType: 0,
      ver: 1,
    })
    return info
  },
  // gochat: async function () {
  //   let store = await core.getStoreConfig()
  //   if (store && store.funJoinModel) {
  //     if (store.kfInfo && store.funJoinModel.imSwitch) {
  //       var userid = store.kfInfo.uid;
  //       var nickname = (store.kfInfo.nickName || "").replace(/\s/gi, "");
  //       var headimg = store.kfInfo.headImgUrl;
  //       tools.goNewPage('/pages/im/chat?userid=' + userid + "&nickname=" + nickname + "&headimg=" + headimg)
  //     } else {
  //       tools.showModalCancle('商家已关闭在线客服')
  //     }
  //   }
  // },
  /**
   * 生成订单
   * @param {Number} paytype  购买类型 1 微信支付 2 储值支付
   * @param {String} jsondata 订单请求数据
   * @param {Number} buyprice 购买价格
   */
  async addPayOrder(paytype, jsondata, buyprice) {
    var user = await core.getUserInfo()
    return http.post(addr.AddPayOrder, {
      paytype,
      jsondata,
      buyprice,
      sessionkey: user.loginSessionKey,
      appid: wepy.$instance.globalData.appid,
      ordertype: 3001021,
      userId: user.Id
    })
  },
  //添加到购物车
  async addGoodsCarData(option) {
    var user = await core.getUserInfo()
    var defaultSetting = {
      userid: user.Id, //用户Id
      sessionkey: user.loginSessionKey,
      attrspacstr: '', //规格ID
      specinfo: '', //商品规格 
      specimg: '', //规格图片
      goodid: 0, //商品ID
      qty: 0, //数量
      gotobuy: 0, //1：立即购买，0：添加到购物车
    }
    return http.post(addr.AddGoodsCarData, tools.extend(defaultSetting, option))
  },
  /**
   * 修改购物车
   * @param {String} goodsCarModel 购物车数据集
   * @param {Number} type 0跟新 -1删除
   */
  async updateOrDeleteGoodsCarData(cartstr, type) {
    var user = await core.getUserInfo()
    return http.post(addr.UpdateOrDeleteGoodsCarData, {
      sessionkey: user.loginSessionKey,
      userid: user.Id,
      cartstr,
      type
    })
  },
  /**
   * 获取购物车列表
   * @param {Number} pageIndex 页码
   * @param {Number} pageSize 页数
   */
  async getGoodsCarList(pageIndex, pageSize) {
    var user = await core.getUserInfo()
    return http.post(addr.GetGoodsCarList, {
      userid: user.Id,
      sessionkey: user.loginSessionKey,
      pageIndex: pageIndex || 1,
      pageSize: pageSize || 8
    })
  },
  /**
   * 获取运费
   * @param {String} province 省
   * @param {String} city 城市
   * @param {String} goodcartids 购物车ID 格式：1，2，3
   */
  async getFreightFee(province, city, goodcartids) {
    var user = await core.getUserInfo()
    return http.post(addr.GetFreightFee, {
      sessionkey: user.loginSessionKey,
      userid: user.Id,
      province,
      city,
      goodcartids
    })
  },
  /** 
   * Number
   * @param {Number} state 
   *   已取消 = -1,
   *   待付款 = 0,
   *   待发货 = 1,
   *   待自取 = 2,
   *   待收货 = 3,
   *   已完成 = 4 
   * @param {Number} pageindex 页码
   * @param {Number} pagesize 页数
   */
  async getOrderList(state, pageindex, pagesize) {
    var user = await core.getUserInfo()
    return http.post(addr.GetOrderList, {
      sessionkey: user.loginSessionKey,
      userid: user.Id,
      state,
      pageindex: pageindex || 1,
      pagesize: pagesize || 8
    })
  },
  /**
   * 获取订单信息
   * @param {Number} id 订单Id
   */
  async getOrderInfo(id) {
    var user = await core.getUserInfo()
    return http.post(addr.GetOrderInfo, {
      sessionkey: user.loginSessionKey,
      userid: user.Id,
      id
    })
  },
  /**
   * 获取用户地址列表
   */
  async getUserAddress() {
    var user = await core.getUserInfo()
    return http.post(addr.GetUserAddress, {
      userid: user.Id
    })
  },
  /**
   * 编辑用户地址
   * @param {Object} model  地址模型 传id=0时添加
   */
  editUserAddress(model) {
    return http.post(addr.EditUserAddress, model)
  },
  /**
   * 改变地址默认状态
   * @param {*} id 地址Id
   * @param {*} isdefault 1 默认 0取消默认
   */
  async changeUserAddressState(id, isdefault) {
    var user = await core.getUserInfo()
    return http.post(addr.ChangeUserAddressState, {
      id,
      isdefault,
      userid: user.Id
    })
  },
  /**
   * 删除地址
   * @param {*} id 
   */
  deleteUserAddress(id) {
    return http.post(addr.DeleteUserAddress, {
      id
    })
  },
  //修改订单状态  id-订单ID  state：-1 取消订单  4：确认收货
  async updateOrderState(id, state) {
    var user = await core.getUserInfo()
    return http.post(addr.UpdateOrderState, {
      sessionkey: user.loginSessionKey,
      userid: user.Id,
      id,
      state
    })
  },
  //删除订单
  async cancleOrder(orderid) {
    var result = await tools.showModal('确定删除订单吗？')
    if (result.confirm) {
      tools.showLoading('删除中')
      var data = await core.updateOrderState(orderid, -1)
      tools.handleResult(data, async () => {
        wx.hideLoading()
        await tools.freeToast('删除成功', 'success', 500)
        tools.goRedirecto('/pages/my/my-order/index')
      }, () => {
        wx.hideLoading()
        tools.freeToast('删除失败', 'none', 500)
      })
    }
  },
  //确认收货
  async confirmOrder(orderid) {
    var result = await tools.showModal('确定收货吗？')
    if (result.confirm) {
      tools.showLoading('处理中')
      var data = await core.updateOrderState(orderid, 4)
      tools.handleResult(data, async () => {
        wx.hideLoading()
        await tools.freeToast('收货成功', 'success', 500)
        tools.goNewPage('/pages/my/my-order/goodOlt?orderid=' + orderid)
      }, (err) => {
        wx.hideLoading()
        tools.freeToast(err.Msg, 'none', 500)
      })
    }
  },
  //获取储值(获取储值)
  async getSaveMoneySetUser() {
    var user = await core.getUserInfo()
    return http.get(addr.GetSaveMoneySetUser, {
      appid: wepy.$instance.globalData.appid,
      openid: user.OpenId
    })
  },
  //获取充值列表
  async getSaveMoneySetList() {
    return http.get(addr.GetSaveMoneySetList, {
      appid: wepy.$instance.globalData.appid
    })
  },
  //获取账单记录
  async getSaveMoneySetUserLogList() {
    var user = await core.getUserInfo()
    return http.get(addr.GetSaveMoneySetUserLogList, {
      appid: wepy.$instance.globalData.appid,
      openid: user.OpenId
    })
  },
  //充值
  async addSaveMoneySet(id) {
    var user = await core.getUserInfo()
    return http.post(addr.AddSaveMoneySet, {
      appid: wepy.$instance.globalData.appid,
      openid: user.OpenId,
      saveMoneySetId: id
    })
  },
  //获取会员信息（获取消费）
  async getVipInfo() {
    var user = await core.getUserInfo()
    return http.post(addr.GetVipInfo, {
      uid: user.Id,
      appid: wepy.$instance.globalData.appid
    })
  },
  /**
   * 获取我的优惠卷
   * @param {Object} option 
   * state -> 0：未使用，1：已使用，2：已过期，3：已失效（默认0）
   */
  async getMyCouponList(option) {
    var user = await core.getUserInfo()
    var settings = {
      pageIndex: 1,
      state: 0,
      goodsId: 0, //商品id（使用时）
      appId: wepy.$instance.globalData.appid,
      userId: user.Id
    }
    return http.post(addr.GetMyCouponList, tools.extend(settings, option))
  },
  /**
   * 获取店铺优惠卷列表
   * @param {int} goodstype 
   * 是否指定商品有优惠（默认-1：查全部，0：全部商品，大于0（商品ID）：指定商品）
   */
  async getStoreCouponList(shopAppid, goodstype) {
    var user = await core.getUserInfo()
    return http.post(addr.GetStoreCouponList, {
      appId: shopAppid,
      userId: user.Id,
      goodstype: goodstype || -1
    })
  },
  /**
   * 领取优惠卷
   * @param {int} couponId 优惠卷Id
   */
  async getCoupon(couponId) {
    var user = await core.getUserInfo()
    return http.post(addr.GetCoupon, {
      appId: wepy.$instance.globalData.appid,
      userId: user.Id,
      couponId: couponId
    })
  },
  //更新用户信息
  async updateUserInfo(nickName, imgUrl) {
    var user = await core.getUserInfo()
    return http.post(addr.UpdateUserInfo, {
      sessionKey: user.loginSessionKey,
      userid: user.Id,
      imgurl: imgUrl,
      nickname: nickName
    })
  },
  //发送后台验证码
  GetVaildCode: function (param) {
    let _param = {
      type: 1,
      phonenum: param.phone,
      agentqrcodeid: param.agentqrcodeid,
    }
    return http.post(addr.GetVaildCode, _param);
  },
  //注册小未账号
  SaveUserInfo: function (param) {
    return http.post(addr.SaveUserInfo, param);
  },
  /********************会员卡系列******************************* */
  //更新会员卡
  updateWxCard: async function (targetPage) {
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo()
    let info = await http.post(addr.UpdateWxCard, {
      appid: app.globalData.appid,
      UserId: userInfo.Id,
      type: 5
    })
    if (info.msg == "还未生成会员卡(请到后台设置同步微信会员卡)") {
      return;
    } else {
      await core.getWxCard(targetPage)
    }
  },
  // 会员卡请求
  getWxCard: async function (targetPage) {
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo()
    let info = await http.get(addr.GetWxCardCode, {
      appid: app.globalData.appid,
      UserId: userInfo.Id,
      type: 5
    })
    let wxCard = false
    if (info.isok) {
      info.obj == null ? wxCard = true : wxCard = false
    }
    targetPage.vm.wxCard = wxCard
    targetPage.$apply()
    await core.setStorage("myVm", targetPage.vm)
  },
  // 获取会员卡Sign(签名)
  getCardSign: async function () {
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo()
    return http.get(addr.GetCardSign, {
      appid: app.globalData.appid,
      UserId: userInfo.Id,
      type: 5
    })
  },
  // 提交code到服务器
  saveWxCard: async function (code, targetPage) {
    let app = wepy.$instance;
    let userInfo = await core.getUserInfo()
    let info = await http.post(addr.SaveWxCardCode, {
      appid: app.globalData.appid,
      UserId: userInfo.Id,
      code: code,
      type: 5
    })
    if (info.isok) {
      console.log('领取成功')
      await core.updateWxCard(targetPage)
    }
  },
  /*****************************社交立减金*************************************/
  //领取立减金
  getReductionCardV2: async function (vm) {
    var userInfo = await core.getUserInfo()
    var aid = await core.getAid()
    return http.post(
      addr.GetReductionCardV2, {
        userId: userInfo.Id,
        aid: aid,
        orderId: vm.orderid,
        couponsId: vm.couponsid,
        sessionKey: userInfo.loginSessionKey
      })
  },
  //获取未领取立减金列表
  getReductionCardListV2: async function (storeId) {
    var userInfo = await core.getUserInfo()
    var aid = await core.getAid()
    let _r = await http.post(
      addr.GetReductionCardListV2, {
        storeId,
        userId: userInfo.Id,
        aid,
        sessionKey: userInfo.loginSessionKey
      })
    if (_r.isok) {
      return _r.dataObj
    } else { 
      tools.showModalCancle(_r.Msg)
      return ""
    }
  }
  /*****************************社交立减金*************************************/
};



module.exports = {
  http,
  core,
  pay,
  tools,
}
