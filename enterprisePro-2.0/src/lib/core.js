import wepy from "wepy";
import addr from "./addr.js";
import _get from './lodash.get';
import {
  un,
  req,
  skin,
  timeTools,
  canvasTools
} from "./utils.js";
import {
  wxParse
} from './wxParse/wxParse';
let reConnectTimer = null;
let isConnecting = false; //ws是否正在连接中
let isFirst = true;
var _index = -1; //页面跳转切换防止多次点击
var compare = function (x, y) { //比较函数
  if (x.sort < y.sort) {
    return 1;
  } else if (x.sort > y.sort) {
    return -1;
  } else {
    if (x.id < y.id) {
      return -1;
    } else if (x.id > y.id) {
      return 1
    } else {
      return 0;
    }

  }
};
/**********************************封装商品事件**********************************************************/
var pro = {
  //普通商品选择事件
  choosePro(_pro, p, c, type, isgroup) {
    let specId = []
    let specInfo = []
    let pick = _pro.pickspecification
    let spec = _pro.GASDetailList
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
    _pro.specId = specId.join("_")
    _pro.specInfo = specInfo.join(" ")
    _pro.totalCount = 1
    let specTemp = spec.find(f => f.id == _pro.specId)
    switch (type) {
      case 'good':
        _pro = this.goodSel(specTemp, _pro)
        break;
      case 'miaosha':
        _pro = this.miaoShaSel(specTemp, _pro)
        break;
      case 'group':
        _pro = this.groupSel(specTemp, _pro, isgroup)
        break;
    }
    _pro.pickspecification[p] = currentList
    return _pro;
  },
  /** 
   * @param {goodSel 普通商品选择} 
   * @param {miaoShaSel 秒杀商品选择 }
   * @param {groupSel 拼团商品选择}
   */
  goodSel(temp, g) {
    if (temp) {
      g.stock = temp.stock
      g.selImg = temp.imgUrl ? temp.imgUrl : g.img
      g.itemPrice = parseFloat(temp.price).toFixed(2) //订单页单个商品显示
      g.danMaiPrice = parseFloat(temp.price).toFixed(2) //总额
      g.yuanJiaPrice = parseFloat(temp.originalPrice).toFixed(2) //原价
    } else {
      g.selImg = g.img
      g.itemPrice = g.priceStr
      g.danMaiPrice = g.priceStr
      g.yuanJiaPrice = parseFloat(g.originalPrice).toFixed(2) //原价
    }
    return g;
  },
  miaoShaSel(temp, g) {
    if (temp) {
      g.stock = temp.stock
      g.selImg = temp.imgUrl ? temp.imgUrl : g.img
      g.itemPrice = parseFloat(temp.discountPrice).toFixed(2) //订单页单个商品显示
      g.danMaiPrice = parseFloat(temp.discountPrice).toFixed(2) //总额
      g.yuanJiaPrice = parseFloat(temp.price).toFixed(2) //原价
    } else {
      g.itemPrice = g.discountPricestr
      g.danMaiPrice = g.discountPricestr
      g.selImg = g.img
      g.yuanJiaPrice = parseFloat(g.price).toFixed(2) //原价
    }
    return g
  },
  groupSel(temp, g, isgroup) {
    if (temp) {
      g.stock = temp.stock
      g.selImg = temp.imgUrl ? temp.imgUrl : g.img
      isgroup == 1 ? g.groupPrice = parseFloat(temp.groupPrice).toFixed(2) : g.danMaiPrice = parseFloat(temp.price).toFixed(2)
      g.itemPrice = isgroup == 1 ? g.groupPrice : g.danMaiPrice
      g.yuanJiaPrice = parseFloat(temp.originalPrice).toFixed(2)
    } else {
      g.selImg = g.img
      g.danMaiPrice = g.priceStr
      g.yuanJiaPrice = g.originalPrice
      g.stock = g.GASDetailList[0].stock
      g.groupPrice = g.GASDetailList[0].groupPrice
      g.itemPrice = isgroup == 1 ? g.groupPrice : g.danMaiPrice
    }
    return g;
  },
  //普通商品加事件
  addPro(_pro, type, target, isgroup) {
    let count = _pro.totalCount
    let price = 0
    // 有规格
    if (_pro.pickspecification.length) {
      let _goodTemp = _pro.GASDetailList.find(f => f.id == _pro.specId)
      if (_goodTemp == undefined) {
        tools.ShowToast("请先选择规格", target)
        return;
      } else {
        if (type == 'good') {
          price = _goodTemp.price
        } else if (type == 'miaosha') {
          price = _goodTemp.discountPrice
        } else {
          isgroup == 1 ? price = _goodTemp.groupPrice : price = _goodTemp.price
        }
        if (_pro.stockLimit) {
          if (count < _goodTemp.stock) {
            count++;
          } else {
            tools.ShowToast("库存不足", target)
          }
        } else {
          count++
        }
      }
    } else {
      if (type == 'good') {
        price = _pro.price
      } else if (type == 'miaosha') {
        price = _pro.discountPrice
      } else {
        Number(isgroup) == 1 ? price = (_pro.EntGroups.GroupPrice).div(100) : price = _pro.price
      }
      if (_pro.stockLimit) {
        if (count < _pro.stock) {
          count++
        } else {
          tools.ShowToast("库存不足", target)
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
  lessPro(_pro) {
    let count = _pro.totalCount
    if (count > 1) {
      count--
    }
    return count;
  },
  // 重置数据
  resetPro(_pro, display, type, isgroup) {
    switch (type) {
      case "good":
        _pro.itemPrice = parseFloat(_pro.priceStr).toFixed(2);
        _pro.danMaiPrice = parseFloat(_pro.priceStr).toFixed(2);
        _pro.yuanJiaPrice = parseFloat(_pro.originalPrice).toFixed(2); //原价
        break;
      case "miaosha":
        _pro.itemPrice = parseFloat(_pro.discountPricestr).toFixed(2);
        _pro.danMaiPrice = parseFloat(_pro.discountPricestr).toFixed(2);
        _pro.yuanJiaPrice = _pro.priceStr //原价
        break;
      case "group":
        _pro.isgroup = Number(isgroup)
        _pro.danMaiPrice = _pro.priceStr //单买
        _pro.yuanJiaPrice = _pro.EntGroups.OriginalPriceStr //原价
        _pro.groupPrice = _pro.GASDetailList.length ? _pro.GASDetailList[0].groupPrice : _pro.EntGroups.GroupPriceStr //拼团价
        _pro.itemPrice = _pro.isgroup == 1 ? _pro.groupPrice : _pro.danMaiPrice
        break;
    }
    _pro.stock = _pro.stockStr;
    _pro.specInfo = "";
    _pro.specId = "";
    _pro.totalCount = 1;
    _pro.display = display;
    _pro.priceStr = parseFloat(_pro.price).toFixed(2);

    for (let i = 0, len = _pro.pickspecification.length; i < len; i++) {
      for (let j = 0, key = _pro.pickspecification[i].items.length; j < key; j++) {
        _pro.pickspecification[i].items[j].sel = false;
      }
    }
    return _pro
  },
  //订单页面所需数据
  orderPro(_pro, type, isgroup) {
    let _g = {}
    let price = 0
    let oriprice = 0
    _g.list = [];
    if (type == 'good') {
      price = _pro.itemPrice
      oriprice = _pro.originalPrice
    } else {
      price = _pro.itemPrice
      oriprice = _pro.yuanJiaPrice
    }
    _g.list.push({
      ImgUrl: _pro.selImg,
      oldPrice: oriprice,
      SpecInfo: _pro.specInfo,
      Introduction: _pro.name,
      discount: _pro.discount,
      discountPrice: price,
      goodid: _pro.id,
      Count: _pro.totalCount,
      type: type
    });
    _g.goodid = _pro.id;
    _g.totalCount = _pro.totalCount;
    _g.totalPrice = _pro.discountPricestr;
    if (type == 'good' || type == 'miaosha') {
      _g.totalPrice = _pro.danMaiPrice;
    } else {
      _g.totalPrice = isgroup == 1 ? Number(Number(_pro.yuanJiaPrice).mul(Number(_pro.totalCount))).toFixed(2) : _pro.danMaiPrice
    }

    return _g
  },
  // 封装优惠卷选择事件
  useCoupon(list, _good, index) {
    let pickCoupon = null
    //如果选择的是指定商品优惠券，判断当前订单列表里的商品是否符合使用条件
    let selCoupon = list[index];
    if (selCoupon.GoodsIdStr != "") {
      var specifiedGood = selCoupon.GoodsIdStr.split(',');
      //筛选出可优惠的产品
      var filterGood = _good.filter(function (item, index) {
        return specifiedGood.includes((item.goodid).toString());
      });
      //计算优惠商品的总价格 会员打折后的总价
      var totalPrice = 0;
      if (filterGood.length > 0) {
        filterGood.forEach(function (curValue) {
          totalPrice += (Number(curValue.discountPrice) || 0).mul(Number(curValue.Count))
        })
      }
      /*
      如果没有符合的指定商品
      或者指定商品的价格没有达到优惠标准
      */
      if (filterGood.length == 0) {
        tools.showModal("订单中没有优惠券指定的商品！", false)
        return pickCoupon;
      } else if (selCoupon.LimitMoney > 0 && totalPrice * 100 < selCoupon.LimitMoney) {
        tools.showModal('指定商品满' + selCoupon.Money_fmt + '元才能使用此优惠券！', false)
        return pickCoupon;
      }
    }
    return selCoupon
  },

};
/**********************************封装工具类**********************************************************/
var tools = {
  // 检查更新
  updateMiniapp() {
    const _update = wx.getUpdateManager()
    _update.onCheckForUpdate(function (res) {
      // console.log(res.hasUpdate)
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
    let ver2 = 1.9
    if (ver1 < ver2 || wx.getSystemInfoSync().SDKVersion == undefined) {
      tools.showModal('当前微信版本过低，无法使用该功能，请升级到最新微信版本后重试', false)
    } else {
      tools.updateMiniapp()
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
              tools.showLoading()
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
        complete: function () { }
      })
      uploadTask.onProgressUpdate((res) => {
        tools.loading('上传中' + res.progress + "%")
      })
    });
  },
  /**
    @param { urltype}
    @return { -1 点击图片放大}
    @return { 0  跳转 target：_blank 新页面打开  _self 当前页面打开}
    @return { 1  跳转小程序}
    @return { 2 跳转功能 furl:-1时点击放大 string路径时跳转  furl：4扫码功能（根据url来跳转)}
    @return { 3 跳转产品详情页}
    @return { 4 跳转产品列表组件}
    @return { 5 跳转拼团详情页}
    @return { 6 跳转砍价详情页}
    @return { 7 跳转团购详情页}
   */
  pathNav: async function (type, _g, target, index) {
    let app = wepy.$instance
    let current = _g.imgurl || _g.img;
    let _pageIndex = index
    let _urltype = _get(_g, 'urltype', '')
    let _p = _get(app.globalData, 'pages', '') || await core.getPageSetting();
    //兼容旧版
    if (_urltype === '') {
      tools.oldGo(type, _g, current, target)
    } else {
      switch (Number(_urltype)) {
        case -1:
          tools.pathMethod(type, current)
          break;
        case 0:
          tools.pathPage(_g, _p, _pageIndex, target)
          break;
        case 1:
          tools.goNewMiniapp(_g.appid, _g.path)
          break;
        case 2:
          tools.pathFunc(_g, type, current)
          break;
        case 3:
          if (_g.items.length == 0) {
            tools.loading("未设置产品")
            return;
          }
          tools.pathGood(_g.items[0].id, _g.btnType, _g.items[0].showprice)
          break;
        case 4:
          tools.pathGoodCat(_g, target)
          break;
        case 5:
          tools.pathGo(_g, "/pages/group2/group2?id=")
          break;
        case 6:
          tools.pathGo(_g, "/pages/bargain/bargain?id=")
          break;
        case 7:
          tools.pathGo(_g, "/pages/group/group?id=")
          break;
        case 8:
          tools.pathGoodSmallCat(_g, target)
          break;
        case 9:
          tools.pathMiaoSha(_g)
          break;
      }
    }
  },
  /**
   * @method oldGo 兼容旧版跳转
   */
  oldGo: function (type, _g, current, target) {
    switch (type) {
      case 'bottomnav' || "imgnav":
        if (_g.url == -1) {
          tools.loading("未设置跳转")
          return;
        }
        tools.showLoading()
        core.renderPage(target, Number(_g.url));
        tools.onPageScroll();
        break;
      case "img" || "slider" || "magicCube":
        tools.preViewShow(current)
        break;
    }
  },

  /**
   * @method  pathGood 产品详情跳转
   * @method  pathMethod 相同动作
   * @method pathGoodCat 产品大类跳转
   * @method pathGoodSmallCat 产品小类跳转
   * @method pathGo 营销插件详情页跳转
   * @method pathPage 页面跳转
   * @method pathFunc 功能
   * @method pathMiaoSha 秒杀
   */
  pathGo: function (_g, url) {
    if (_g.itemstype.length == 0) {
      tools.loading("未设置跳转")
      return;
    }
    tools.goNewPage(url + _g.itemstype[0].id)
  },
  pathGood: function (id, btn, showprice, sale) {
    let goodpara = {
      id,
      btn,
      showprice,
      sale,
    }
    tools.goNewPage("/pages/good/good?para=" + JSON.stringify(goodpara))
  },
  pathMethod: function (type, current) {
    if (type == 'img' || type == 'slider' || type == 'magicCube') {
      tools.preViewShow(current);
    } else {
      tools.loading("未设置跳转")
    }
  },
  pathGoodCat: function (_g, target) {
    let vm = {}
    let app = wepy.$instance
    let switchInfo = app.globalData.switchInfo
    vm.typeid = _g.itemstype[0].id
    vm.title = _g.itemstype[0].name
    vm.isFirstType = switchInfo.SeondTypeOpen ? 0 : '';
    vm.goods = {
      showBig: switchInfo.SeondTypeOpen ? true : false,
      goodShowType: 'small',
      btnType: _g.btnType || 'buy',
      isShowPrice: true,
    }
    target.$preload("vm", vm);
    target.$navigate("/pages/good/goodProLst");
  },
  pathGoodSmallCat: function (_g, target) {
    let vm = {}
    vm.typeid = _g.itemstype[0].id
    vm.title = _g.itemstype[0].name
    vm.isFirstType = ""
    vm.goods = {
      showBig: false,
      goodShowType: 'small',
      btnType: _g.btnType,
      isShowPrice: true,
    }
    target.$preload("vm", vm);
    target.$navigate("/pages/good/goodProLst");
  },
  pathPage: function (_g, _p, _pageIndex, target) {
    if (Number(_index) == Number(_pageIndex)) {
      return;
    }
    if (_g.url >= _p.pages.length) {
      tools.loading("未设置跳转")
      return;
    }
    if (_g.url == -1) {
      tools.loading("未设置跳转")
    } else {
      if (_g.target == '_blank') {
        _index = -1
        getCurrentPages().length >= 8 ? tools.goRedirecto("/pages/index/index?currentPageIndex=" + _g.url) : tools.goNewPage("/pages/index/index?currentPageIndex=" + _g.url)
      } else {
        tools.showLoading()
        _index = _pageIndex === '' ? -1 : _pageIndex
        core.renderPage(target, Number(_g.url));
        tools.onPageScroll();
      }
    }
  },
  pathFunc: function (_g, type, current) {
    if (_g.furl == -1) {
      tools.pathMethod(type, current)
      return;
    }
    if (_g.furl != '' && typeof (_g.furl) == 'string') {
      tools.goNewPage("/" + _g.furl)
      return;
    }
    if (_g.furl == 4) {
      tools.sceneQrcode(Number(_g.url))
      return;
    }
  },
  pathMiaoSha: function (_g) {
    let id = _g.itemstype[0].id
    tools.goNewPage("/pages/miaoSha/more?id=" + id)
  },
  //主题色改变
  setPageSkin: async function (target) {
    let skinIndex = 0
    let app = wepy.$instance
    let currentPage = _get(app.globalData, 'pages', '') || await core.getPageSetting();
    let pages = _get(currentPage, "pages", "")
    if (pages) {
      skinIndex = pages[0].skin
    }
    wx.setNavigationBarColor({
      frontColor: skin[skinIndex].color,
      backgroundColor: skin[skinIndex].bgcolor,
    })
    target.currentSkin = skin[skinIndex].type
    target.$apply()
  },
  //倒计时
  TimeShow: function (startDateStr, endDateStr) {
    let timeInter = "00:00:00"
    let end = endDateStr
    let star = startDateStr
    let timeQueue = ['days', 'hours', 'min', 'seconds']
    let starTime = (new Date(star)) - (new Date()); //计算剩余的毫秒数
    if (starTime > 0) {
      timeQueue = timeTools.getRemainTimeQueue(starTime).join(':') //根据剩余毫秒数转换对应日期数组
    } else {
      let endTime = (new Date(end)) - (new Date()); //计算剩余的毫秒数
      if (endTime <= 0) {
        timeQueue = "00:00:00"
      } else {
        timeQueue = timeTools.getRemainTimeQueue(endTime).join(':') //根据剩余毫秒数转换对应日期数组
      }
    }
    timeInter = timeQueue
    return timeInter
  },
  //拨打电话
  phoneFunc: function (phoneNumber) {
    if (phoneNumber) {
      wx.makePhoneCall({
        phoneNumber,
      })
    } else {
      tools.loading("未设置电话")
    }
  },
  //跳转小程序
  goNewMiniapp: function (appId, path) {
    wx.navigateToMiniProgram({
      path,
      appId,
      success(res) { },
      fail(err) {
        tools.showModal(err, false)
      }
    })
  },
  //扫码
  sceneQrcode: function (url) {
    let app = wepy.$instance.globalData
    wx.scanCode({
      onlyFromCamera: true,
      success: async res => {
        if (res.path == undefined) {
          tools.showModal("亲，该二维码有误", false)

        } else { //扫码成功操作
          app.storecodeid = res.path.split('?scene=')[1]
          await tools.loading('扫码成功', 'success')
          if (url != -1) {
            tools.goRedirecto("/pages/index/index?currentPageIndex=" + url)
          }

        }
      }
    })
  },
  //页面回到顶部
  onPageScroll: function () {
    wx.pageScrollTo({
      scrollTop: 0,
      duration: 300,
    });
  },
  //动态改顶部兰标题
  setPageTitle: function (title) {
    wx.setNavigationBarTitle({
      title,
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
  //loading加载
  showLoading: function (msg) {
    let ms = msg || '加载中...';
    wx.showLoading({
      title: ms,
      mask: true,
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
  // 复制
  copy: function (data) {
    wx.setClipboardData({
      data: data,
      success: function (res) {
        wx.getClipboardData({
          success: function (res) {
            tools.loading("复制成功", 'success')
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
  showModal: function (msg, _bools) {
    if (_bools == undefined) {
      _bools = true
    }
    return new Promise(function (resolve, reject) {
      wx.showModal({
        title: "提示",
        content: msg,
        showCancel: _bools,
        success: res => {
          resolve(res)
        }
      })
    })
  },
  //显示提示框
  ShowToast: function (msg, target) {
    target.toast.show = true;
    target.toast.msg = msg;
    target.$apply();
    setTimeout(() => {
      target.toast.show = false;
      target.$apply();
    }, 1000);
  },
  loading: function (msg, _i) {
    wx.showToast({
      title: msg,
      icon: _i || "loading",
      duration: 1000
    })
  },
  ChangeDateFormat: function (val) {
    if (val != null) {
      var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", ""), 10));
      //月份为0-11，所以+1，月份 小时，分，秒小于10时补个0
      var month = timeTools.patchTime(date.getMonth() + 1);
      var currentDate = timeTools.patchTime(date.getDate());
      var hour = timeTools.patchTime(date.getHours());
      var minute = timeTools.patchTime(date.getMinutes());
      var second = timeTools.patchTime(date.getSeconds());
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
  //统一富文本转换
  richChange(desc, target) {
    if (desc) {
      desc = desc.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>').replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>').replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
      return wxParse("description", "html", desc || "", target, 5);
    } else {
      return "";
    }
  },
  //统一授权
  async getRnUser(e, type) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, "userInfo", "")
    if (type && type === 1 && e.detail.errMsg.includes('getPhoneNumber:fail user deny')) {
      return userInfo;
    }
    if (e.detail.errMsg.includes('getUserInfo:fail auth deny')) {
      return userInfo;
    }
    let vm = {
      iv: e.detail.iv,
      data: e.detail.encryptedData,
    }
    let user = await core.loginUserInfo(vm)
    return user
  },
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
  // 地理位置授权
  getLocation() {
    return new Promise(function (resolve, reject) {
      wx.getLocation({
        type: 'wgs84',
        success: res => {
          resolve(res)
        },
        fail: res => {
          tools.showModal("若要使用地位,请点击右上角‘关于小程序’进行相关设置", false)
          resolve('')
        },
        complete: function () { }
      })
    })
  },
};
/**********************************支付**********************************************************/
var pay = {
  // 普通支付// PayOrder
  PayOrder: async function (param) {
    let app = wepy.$instance
    let aid = _get(app.globalData, 'aid', '') || await core.getAid()
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo()
    let payInfo = await http.post(addr.PayOrder, {
      aid: aid,
      openId: userInfo.OpenId,
      orderid: param.orderid,
      'type': param.type,
    })
    if (payInfo.result) {
      let jsObj = JSON.parse(payInfo.obj);
      let wxpay = await pay.wxpay(jsObj)
      return wxpay;
    } else {
      if (payInfo.obj) {
        let msg = payInfo.obj.split('"')
        if (msg[7].includes('mch_id参数格式错误') || payInfo.obj.includes("mch_id参数长度有误")) {
          await tools.showModal("商户秘钥错误", false)
        } else {
          await tools.showModal(payInfo.msg, false)
        }
      }
      return ""
    }
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
  // 拼团下单
  AddOrderNew: async function (param) {
    let app = wepy.$instance
    let appid = app.globalData.appid
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.AddPayOrderNew, {
      appId: appid,
      openid: userInfo.OpenId,
      userId: userInfo.Id,
      ordertype: param.order,
      paytype: param.paytype,
      jsondata: param.jsondata
    })
  },
  //formid
  deleteLastFormId: function () {
    let app = wepy.$instance
    http.post(addr.deleteLastFormId, {
      appid: app.globalData.appid,
      openid: app.globalData.userInfo.OpenId
    })
  },
};
/**********************************请求**********************************************************/
var http = {
  //异步请求
  postJson: function (url, data) {
    return new Promise(function (resolve, reject) {
      wx.request(Object.assign({}, req.json, {
        url,
        data,
        method: "POST",
        fail: function (e) {
          // isdebug && console.log(`请求 ${_url} 失败！\r\n 错误信息：${e.errMsg}`);
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
  //异步请求
  post: function (url, data) {
    return new Promise(function (resolve, reject) {
      wx.request(Object.assign({}, req.urlencoded, {
        url,
        data,
        method: "POST",
        fail: function (e) {
          // isdebug && console.log(`请求 ${_url} 失败！\r\n 错误信息：${e.errMsg}`);
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
  get: function (url, data) {
    return new Promise(function (resolve, reject) {
      wx.request(Object.assign({}, req.urlencoded, {
        url,
        data,
        fail: function (e) {
          // isdebug && console.log(`请求 ${_url} 失败！\r\n 错误信息：${e.errMsg}`);
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
  senduserauth: async function (tel) { //获取手机验证码
    let app = wepy.$instance
    let appid = app.globalData.appid
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.senduserauth, {
      tel: tel,
      sendType: 8,
      appid: appid,
      openId: userInfo.OpenId,
    })
  },
  Submitauth: async function (tel, authCode) { //绑定手机号码
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.Submitauth, {
      tel: tel,
      openId: userInfo.OpenId,
      authCode: authCode
    })
  },
  //needappsr0 是第三方， 1是个人
  loginUserInfo: async function (vm) {
    let app = wepy.$instance
    let appId = app.globalData.appid;
    let checkSession = await tools.checkSession()
    let userInfo = _get(app.globalData, 'userInfo', '')
    if (userInfo == '' || !checkSession) {
      let code = await core.login();
      userInfo = await core.wxLogin(code)
    }
    let info = await http.post(addr.loginByThirdPlatform, {
      appId,
      iv: vm.iv,
      data: vm.data,
      userId: userInfo.Id,
    })
    if (info.isok) {
      let headImg = _get(info.dataObj, "HeadImgUrl", "")
      let headName = _get(info.dataObj, "NickName", "")
      if (headImg == null) {
        headImg = ''
      }
      if (headName == null) {
        headName = ''
      }
      if (headImg == '' || headName == '') {
        info.dataObj.newUser = true
      } else {
        info.dataObj.newUser = false
      }
      app.globalData.userInfo = info.dataObj

      return info.dataObj;
    } else {
      tools.showModal(info.Msg, false)
    }
  },
  //newUser为新用户 false表已授权
  wxLogin: async function (code) {
    let appId = wepy.$instance.globalData.appid;
    let info = await http.post(addr.WxLogin, {
      code,
      appId,
      needAppsr: 1,
    })
    if (info.isok) {
      let headImg = _get(info.dataObj, "HeadImgUrl", "")
      let headName = _get(info.dataObj, "NickName", "")
      if (headImg == null) {
        headImg = ''
      }
      if (headName == null) {
        headName = ''
      }
      if (headImg == '' || headName == '') {
        info.dataObj.newUser = true
      } else {
        info.dataObj.newUser = false
      }
      return info.dataObj;
    } else {
      tools.showModal(info.Msg, false)
      wx.hideLoading()
    }
  },
  getUserInfo: async function () {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, "userInfo", "")
    if (userInfo) {
      let checkSession = tools.checkSession()
      if (checkSession) {
        return userInfo;
      }
    }
    let code = await core.login();
    if (code) {
      let info = await core.wxLogin(code);
      app.globalData.userInfo = info
      return info;
    }
  },
  //获取code
  login: function () {
    return new Promise(function (resolve, reject) {
      wx.login({
        success: function (res) {
          resolve(res.code);
        }
      });
    })
  },
  /**********************************封装wx end**********************************************************/
  getAid: async function () {
    let app = wepy.$instance
    let aid = _get(app.globalData, "aid", '')
    let appid = app.globalData.appid;
    await core.getVipInfo()
    core.appSwitch()
    if (aid) {
      return aid
    } else {
      let info = await http.post(addr.Getaid, {
        appid
      });
      if (info.isok) {
        app.globalData.aid = info.msg
        return info.msg;
      }
    }
  },
  /**
   * @method getVipInfo 会员信息
   */
  getVipInfo: async function () {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo()
    let vipinfo = _get(app.globalData, 'vipInfo', '')
    if (vipinfo != '') {
      return vipinfo;
    }
    let info = await http.get(addr.GetVipInfo, {
      appid: app.globalData.appid,
      uid: userInfo.Id
    })
    if (info.isok) {
      let valueMoney = await core.valueMoney();
      info.model.valueMoney = valueMoney.saveMoneySetUser.AccountMoneyStr
      app.globalData.vipInfo = info.model;
      return info.model;
    } else {
      return "";
    }
  },
  /**
   * @method formId 模板消息提交
   */
  formId: async function (formid) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, "userInfo", '') || await core.getUserInfo();
    http.post(addr.commitFormId, {
      appid: app.globalData.appid,
      openid: userInfo.OpenId,
      formid: formid
    })
  },
  //获取储值余额
  valueMoney: async function () {
    let app = wepy.$instance
    let userInfo = app.globalData.userInfo
    return http.get(addr.getSaveMoneySetUser, {
      appid: app.globalData.appid,
      openid: userInfo.OpenId,
    })
  },
  //产品组件
  getGoodByids: async function (ids, levelid, ShowType) {

    let data = await http.post(addr.GetGoodsByids, {
      ids,
      levelid: levelid || "",
      goodShowType: ShowType || '',
    })
    if (data.isok) {
      return data.msg
    } else {
      return ""
    }
  },
  //产品列表组件
  getGoodList: async function (vm, com) {
    let app = wepy.$instance
    if (typeof (vm.typeid) == 'object') {
      vm.typeid = vm.typeid.join(",")
    }
    let aid = app.globalData.aid
    let vipInfo = app.globalData.vipInfo
    let goodShowType = _get(com, 'goodShowType', '')
    return http.get(addr.GetGoodsList, {
      aid,
      goodShowType,
      typeid: vm.typeid,
      search: vm.search || '',
      exttypes: vm.exttypes || '',
      pagesize: vm.pagesize,
      pricesort: vm.pricesort || '',
      pageindex: vm.pageindex,
      levelid: vipInfo.levelid || '',
      isFirstType: vm.isFirstType,
      saleCountSort: vm.saleCountSort || '',
    })
  },
  //拼团1.0
  groupInfo: async function (groupid) {
    let app = wepy.$instance
    return http.post(addr.GetGroupDetail, {
      appId: app.globalData.appid,
      groupId: groupid,
    })
  },
  //获取二级分类
  getGoodType: function (ids) {
    return http.post(addr.GetGoodTypeList, {
      appid: wepy.$instance.globalData.appid,
      ids: ids
    })
  },
  getPageConfig: async function () {
    let app = wepy.$instance
    let aid = _get(app.globalData, 'aid', '') || await core.getAid()
    return http.post(addr.GetPageSetting, {
      aid
    });
  },
  getPageSetting: async function () {
    let app = wepy.$instance
    let pages = _get(app.globalData, 'pages', '') || await core.getPageConfig();
    if (pages.isok) {
      if (typeof (pages.msg.pages) === "string") {
        pages.msg.pages = JSON.parse(pages.msg.pages);
      }
      // 排除“产品预约”页面 不需要显示
      for (let i = 0, len = pages.msg.pages.length; i < len; i++) {
        if (pages.msg.pages[i].def_name.includes("产品预约")) {
          pages.msg.pages.splice(i, 1)
        }
      }
      app.globalData.appConfig = pages
      app.globalData.pages = pages.msg
      return pages.msg;
    } else {
      return pages
    }
  },
  //悬浮按钮
  iconStatus: async function (currentCom, pageIndex, target) {
    let vm = {}
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo()
    vm = await un.icon(currentCom, pageIndex)
    vm.userInfo = userInfo
    vm.showState = false
    vm.backUp = false
    if (pageIndex == 0) {
      let reduction = await core.getReductionLst(0)
      vm.reduction = reduction.length ? true : false
    }

    target.vm_com_icon = vm
    target.$apply()
  },
  comPonents: function (currentCom, pageIndex, target) {
    for (let i = 0, len = currentCom.length; i < len; i++) {
      let vm = currentCom[i]
      switch (vm.type) {
        case "form":
          vm.items.forEach(function (o, i) {
            if (o.type == 'radio') {
              let array = []
              for (let i = 0, len = o.items.length; i < len; i++) {
                array.push(o.items[i].name)
              }
              o.array = array
            }
          })
          break;
        case "goodlist":
          vm.GoodCatNavStyle == '6' ? core.getGoodLstType(vm, target) : core.getGoodLst(vm, pageIndex, target)
          break;
      }
    }
  },
  renderPage: async function (target, pageIndex) {
    let app = wepy.$instance
    let aid = _get(app.globalData, 'aid', '') || await core.getAid()
    http.post(addr.GetPageSettingUpdateTime, {
      aid
    }).then(async updatetime => {
      let pageSetting = _get(app.globalData, 'pages', '')
      if (pageSetting == '' || updatetime.msg != pageSetting.updatetime) {
        app.globalData.pages = ''
        pageSetting = await core.getPageSetting()
      }
      if (typeof (pageSetting.msg) == 'string' && pageSetting.msg.includes('小程序没有设置页面') || pageSetting.pages.length == 0) {
        tools.showModal("小程序未配置页面", false)
        wx.hideLoading()
        return;
      }
      if (pageIndex < 0 || pageIndex > pageSetting.pages.length) {
        tools.showModal("页面不存在", false)
        wx.hideLoading()
        return;
      }
      let meArray = _get(pageSetting, "MeConfig", "")
      let currentPage = pageSetting.pages[pageIndex];
      if (meArray) {
        target.vm_com_bottom = meArray
      }
      target.currentPage = currentPage;
      target.currentPageIndex = pageIndex;
      target.$apply();
      Promise.all([
        tools.setPageTitle(currentPage.name),
        tools.setPageSkin(target),
        core.comPonents(currentPage.coms, pageIndex, target),
        core.iconStatus(currentPage.coms, pageIndex, target),
        core.getCoupShow(pageIndex, target),
        core.logoSwitch(target)
      ])
      wx.hideLoading()
      // 当重复点击或者切换时检测loadall，为-1时不用再次请求
      var loadall = _get(target.vm_com_status, pageIndex.toString(), -1)
      if (loadall != -1) {
        return;
      }
      for (let comIndex = 0, len = currentPage.coms.length; comIndex < len; comIndex++) {
        core.renderCom(pageIndex, comIndex, currentPage.coms[comIndex], target);
      }
      // 页面加载完loadall=true
      _index = -1
      var loadall = _get(target.vm_com_status, pageIndex.toString(), -1)
      loadall == -1 ? target.vm_com_status[pageIndex.toString()] = true : '';
      target.$apply();
    })

  },
  renderCom: function (pageIndex, comIndex, currentCom, target) {
    let _array = []
    let key = pageIndex + "_" + comIndex
    switch (currentCom.type) {
      case "slider":
        currentCom.current = 0
        break;
      case "good":
        _array.push(core.goodRequest(key, currentCom, target))
        break;
      case "news":
        _array.push(core.allNews(key, currentCom, target))
        break;
      case "live":
        let vm = {
          list: currentCom.items.slice(0, 3),
          num: currentCom.items.length
        }
        target.vm_com_live[key] = vm;
        break;
      case "cutprice":
        _array.push(core.bargain(key, currentCom, target))
        break;
      case "richtxt":
        target.vm_com_rich[key] = tools.richChange(currentCom.content, target);
        break;
      case "joingroup":
        _array.push(core.joingroupRequest(key, currentCom, target))
        break;
      case "entjoingroup":
        _array.push(core.getEngroupIds(key, currentCom, target))
        break;
      case "bgaudio":
        if (currentCom.src) {
          let pause = _get(wepy.$instance.globalData, 'musicPause', true)
          if (pause) {
            currentCom.playAudio = true
            currentCom.scrollShow = false
            wx.playBackgroundAudio({
              dataUrl: currentCom.src
            })
          }
        }
        break;
      case "flashdeal":
        _array.push(core.getMiaoSha(key, currentCom.flashDealId, target))
        break;
      case "newslist":
        let ids = []
        if (currentCom.newsCat.length == 0) {
          return;
        }
        for (let i = 0, len = currentCom.newsCat.length; i < len; i++) {
          ids.push(currentCom.newsCat[i].id)
        }
        if (ids.length) {
          ids = ids.join(',')
        }
        target.vm_com_newlst.ids = ids
        target.vm_com_newlst.ids_array = ids
        _array.push(core.getNewsLst(target.vm_com_newlst, target))
        break;
    }
    Promise.all(_array)
    target.$apply();
  },
  /**
   * @param {getGoodLstType 产品列表优先显示分类}
   * @param {getGoodLst  产品列表}
   * @param {_gCat全局是否有goodcat值}
   * @param {_firType是否开启二级分类 大类传0 小类传空}
   * @param {showMore是否显示二级分类按钮}
   * @param {showgoodAll是否显示全部分类 单个不显示 多个显示} 
   */
  getGoodLstType: async function (_g, target) {
    let id = []
    let showfirst = true
    _g.goodCat.forEach(function (o, i) {
      id.push(o.id)
    })
    let ids = id.join(',')
    let data = await core.getGoodType(ids)
    if (data.isok) {
      if (_g.goodCat[0].parentId != 0) {
        for (let i = 0, len = data.dataObj.length; i < len; i++) {
          data.dataObj[0].SecondGoodTypes.push(data.dataObj[i].FirstGoodType)
        }
        showfirst = false
      } else {
        showfirst = true
      }
      target.vm_com_classify.list = data.dataObj
      target.vm_com_classify.showfirst = showfirst
      target.$apply()
    }
  },
  getGoodLst: function (current, pageIndex, target) {
    let id = []
    let app = wepy.$instance
    let showBIG = app.globalData.showBIG
    let goodCat = app.globalData.goodCat[pageIndex]
    let vm = tools.resetArray(target.vm_com_goodLst)
    let firtype = _get(current.goodCat[0], 'parentId', "");
    vm.goodCatArray[pageIndex] = current.goodCat
    if (firtype === 0) {
      vm.showMore = true
      if (goodCat && goodCat.length == 1) {
        vm.isFirstType = showBIG ? 0 : ''
      } else {
        if (showBIG) {
          vm.isFirstType = 0
        } else {
          vm.isFirstType = goodCat && goodCat.length > 1 ? '' : 0;
        }
      }
      vm.goodCat[pageIndex] = goodCat && goodCat.length ? goodCat : current.goodCat;
    } else {
      vm.isFirstType = ''
      vm.showMore = false
      vm.goodCat[pageIndex] = current.goodCat
    }
    if (vm.goodCat[pageIndex].length) {
      vm.goodCat[pageIndex].forEach(function (o, i) {
        id.push(o.id)
      })
      vm.goodCat[pageIndex] = vm.goodCat[pageIndex].sort(compare)
    }
    vm.showgoodAll = vm.goodCat[pageIndex].length == 1 ? false : true
    vm.typeid = id
    vm.alltypeid = id.join(",")
    core.getGoodsListRequest(vm, current, target)
    app.globalData.goodCat = []
  },
  // 优惠券弹窗&&积分弹窗
  getCoupShow: async function (pageIndex, target) {
    let app = wepy.$instance
    let coupHidden = _get(app.globalData, 'coupHidden', '')
    if (coupHidden) {
      return;
    }
    if (Number(pageIndex)) {
      return;
    }
    let coupfloat = await core.getStoreCoup(1, 5);
    if (coupfloat.postdata.length) {
      for (let i = 0, l = coupfloat.postdata.length; i < l; i++) {
        coupfloat.postdata[i].isGet = false;
        coupfloat.postdata[i].coupBtnText = "领取";
      }
      target.coupHidden = coupfloat.postdata.length > 0 ? true : false;
      target.vm_com_coupList = coupfloat.postdata;
      target.$apply();
      app.globalData.coupHidden = target.coupHidden
    }
    let showsigninFloat = _get(app.globalData, 'showsigninFloat', '');
    if (showsigninFloat) {
      return;
    }
    if (target.coupHidden == false) {
      core.showSignin(target)
    }
  },
  //秒杀
  getMiaoSha: async function (key, id, target) {
    let vm = {}
    let app = wepy.$instance;
    http.post(addr.GetFlashDeal, {
      appId: app.globalData.appid,
      openId: app.globalData.userInfo.OpenId,
      flashDealIds: id,
    }).then(info => {
      if (info.isok) {
        if (info.dataObj) {
          vm = info.dataObj[0]
          vm.more = true
          if (vm.State == 0 || vm.State == 3) {
            vm.show = false
            target.vm_com_miaosha[key] = vm
            target.$apply()
            return;
          }
          vm.show = true
          if (vm.Item.length > 4) {
            vm.Item.splice(4, vm.Item.length)
          }
          for (let i = 0, len = vm.Item.length; i < len; i++) {
            if (vm.Item[i].Stock != 0) {
              vm.Item[i].saleRate = ((Number(vm.Item[i].Stock).div((Number(vm.Item[i].Sale).add(Number(vm.Item[i].Stock))))).mul(100)).toFixed(0)
            } else {
              vm.Item[i].saleRate = 0
            }
          }
          target.vm_com_miaosha[key] = vm
          target.$apply()
          core.miaoShaCountDown(vm, target)
        } else {
          vm.show = false
          target.vm_com_miaosha[key] = vm
          target.$apply()
        }
      }
    })
  },
  // 秒杀更多
  getMiaoShaMore: function (id, target) {
    let app = wepy.$instance;
    let vm = {}
    http.post(addr.GetFlashDeal, {
      appId: app.globalData.appid,
      openId: app.globalData.userInfo.OpenId,
      flashDealIds: id,
    }).then(data => {
      if (data.isok) {
        if (data.dataObj) {
          vm = data.dataObj[0]
          vm.more = false
          vm.description = vm.description.split('\n')
          for (let i = 0, len = vm.Item.length; i < len; i++) {
            if (vm.Item[i].Stock != 0) {
              vm.Item[i].saleRate = (Number(vm.Item[i].Stock).div((Number(vm.Item[i].Sale).add(Number(vm.Item[i].Stock))))).mul(100).toFixed(0)
            } else {
              vm.Item[i].saleRate = 0
            }
          }

          target.vm = vm
          target.$apply()
          core.miaoShaCountDown(vm, target)
        }
      }
    })
  },
  //秒杀提醒
  miaoShaTip: function (id, target, type, miaoshaid, key) {
    let app = wepy.$instance
    http.post(addr.AddFlashSubscribe, {
      appId: app.globalData.appid,
      openId: app.globalData.userInfo.OpenId,
      flashItemId: id,
    }).then(data => {
      if (data.isok) {
        tools.ShowToast('已设模板消息请您留意微信消息', target)
        clearInterval(target.miaoShaCutDown)
        if (Number(type) == 1) {
          core.getMiaoSha(key, miaoshaid, target)
        } else {
          core.getMiaoShaMore(miaoshaid, target)
        }
      }
    })
  },
  // 秒杀倒计时
  miaoShaCountDown: function (vm, target) {
    target.miaoShaCutDown = setInterval(res => {
      let startShow = false
      let timeFormatArray = []
      let starTime = timeTools.getTimeSpan(vm.Begin);
      let endTime = timeTools.getTimeSpan(vm.End);
      if (starTime > 0) {
        startShow = true
        timeFormatArray = timeTools.formatMillisecond(starTime);
        timeFormatArray.push(startShow)
      } else {
        startShow = false
        if (endTime <= 0) {
          timeFormatArray = ['00', '00', '00', '00']
          clearInterval(target.miaoShaCutDown)
        } else {
          timeFormatArray = timeTools.formatMillisecond(endTime);
        }
      }
      timeFormatArray.push(startShow)
      vm.countDownArray = timeFormatArray
      target.vm = vm
      target.$apply()
    }, 1000);
  },
  //产品组件
  goodRequest: async function (key, currentCom, target) {
    let goodidsArray = []
    currentCom.items.forEach(function (o, i) {
      goodidsArray.push(o.id)
    })
    if (goodidsArray.length > 0) {
      let app = wepy.$instance
      let vipinfo = _get(app.globalData, 'vipInfo', '') || await core.getVipInfo();
      let ShowType = _get(currentCom, 'goodShowType', "")
      let tmp = await core.getGoodByids(goodidsArray.join(','), vipinfo.levelid, ShowType)
      if (tmp) {
        let vm = {
          list: tmp
        }
        target.vm_com_good[key] = vm;
        target.$apply();
      }
    }
  },
  //产品列表组件
  getGoodsListRequest: async function (vm, currentCom, target) {
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    if (typeof (vm.typeid) == 'object') {
      vm.typeid = vm.typeid.join(",")
    }
    let app = wepy.$instance
    let aid = app.globalData.aid
    let vipInfo = _get(app.globalData, 'vipInfo', '') || await core.getVipInfo()
    http.get(addr.GetGoodsList, {
      aid,
      typeid: vm.typeid,
      search: vm.search,
      exttypes: vm.exttypes,
      pagesize: vm.pagesize,
      pricesort: vm.pricesort,
      pageindex: vm.pageindex,
      isFirstType: vm.isFirstType,
      levelid: _get(vipInfo, 'levelid', ''),
      goodShowType: _get(currentCom, 'goodShowType', ''),
      saleCountSort: vm.saleCountSort,
    }).then(glistInfo => {
      vm.ispost = false;
      if (glistInfo.isok == 1) {
        vm.list[vm.pageindex] = glistInfo.postdata.goodslist
        glistInfo.postdata.goodslist.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true
        target.vm_com_goodLst = vm
        target.$apply()
      }
    })
  },
  //产品列表筛选
  goodFifter: async function () {
    let aid = wepy.$instance.globalData.aid
    return http.post(addr.GetExtTypes, {
      aid,
    })
  },
  //拼团组件
  joingroupRequest: async function (key, currentCom, target) {
    let ids = [];
    let aid = wepy.$instance.globalData.aid
    currentCom.items.forEach(function (o, i) {
      ids.push(o.id)
    })
    if (ids.length > 0) {
      http.post(addr.GetGroupByIds, {
        aid,
        ids: ids.join(","),
      }).then(data => {
        if (data) {
          let vm = {}
          vm.list = data.postdata;
          target.vm_com_group[key] = vm;
          target.$apply();
        }
      })
    }
  },
  //砍价组价请求
  bargain: async function (key, currentCom, target) {
    let _goodids = [];
    let app = wepy.$instance
    let user = app.globalData.userInfo
    currentCom.items.forEach(function (o, i) {
      _goodids.push(o.id)
    })
    let _postids = _goodids.join(",")
    if (_goodids.length > 0) {
      http.get(addr.GetBargainList, {
        appid: app.globalData.appid,
        ids: _postids,
      }).then(data => {
        if (data.length > 0) {
          data.forEach(function (_cutprice_item) {
            _cutprice_item.startDateStr = _cutprice_item.startDateStr.replace(/-/g, '/');
            _cutprice_item.endDateStr = _cutprice_item.endDateStr.replace(/-/g, '/');
          });
          target.vm_com_bargain[key] = data
          target.vm_com_user = user
          target.$apply()
          core.bargainCount(data, target, key)
        }
      })
    }
  },
  bargainCount: function (barlist, target, key) {
    target.barCount = setInterval(async () => {
      target.vm_com_bargain[key] = await core.barCountDown(barlist, target);
      target.$apply();
    }, 1000);
  },
  // // 砍价倒计时
  barCountDown: function (data) {
    if (data.length > 0) {
      for (var j = data.length - 1; j >= 0; j--) {
        var dataItem = data[j]
        if (dataItem.RemainNum == 0) {
          dataItem.time = ['00', '00', '00', '00']
        } else {
          let starTime = timeTools.getTimeSpan(dataItem.startDateStr);
          let endTime = timeTools.getTimeSpan(dataItem.endDateStr);
          if (starTime > 0) {
            var timeFormatArray = timeTools.formatMillisecond(starTime);
            dataItem.time = timeFormatArray
          } else {
            if (endTime <= 0) {
              dataItem.time = ['00', '00', '00', '00']
            } else {
              var timeFormatArray = timeTools.formatMillisecond(endTime);
              dataItem.time = timeFormatArray
            }
          }
        }
      }
    }
    return data
  },
  //拼团2.0
  getEngroupIds: async function (key, currentCom, target) {
    let ids = []
    currentCom.items.forEach(function (o, i) {
      ids.push(o.id)
    });
    let aid = wepy.$instance.globalData.aid
    if (ids.length > 0) {
      http.get(addr.GetEntGroupByIds, {
        aid,
        ids: ids.join(","),
      }).then(data => {
        if (data.isok) {
          let vm = {
            list: data.postdata
          }
          let dataKey = "vm_com_group2." + key;
          target.setData({
            [dataKey]: vm,
          });
        }
      })
    }
  },
  /***************************表单 预约***********************************************/
  formRequest: async function (formdatajson, comename) {
    let app = wepy.$instance
    let aid = _get(app.globalData, 'aid', '') || await core.getAid()
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo()
    return http.post(addr.SaveUserForm, {
      aid,
      uid: userInfo.Id,
      formdatajson: formdatajson,
      comename: comename,
      storecodeid: _get(app.globalData, 'storecodeid', 0),
    })
  },
  //提交表单
  submitForm: async function (vm, target) {
    wx.showNavigationBarLoading();
    let app = wepy.$instance
    let aid = _get(app.globalData, 'aid', '') || await core.getAid()
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo()
    http.post(addr.SaveSubscribeForm, {
      aid,
      uid: userInfo.Id,
      remark: vm.remark,
      formId: vm.formId,
      buyMode: vm.buyMode,
      formdatajson: vm.datajson,
    }).then(async data => {
      if (data.isok) {
        if (vm.buyMode == 1) {
          if (target.vm.goodInfo.price == 0) {
            tools.loading("预约成功", 'success')
            wx.hideNavigationBarLoading()
            return;
          }
          let newparam = {
            orderid: data.obj.citymorderId,
            type: 1
          };
          let wxpay = await pay.PayOrder(newparam);
          if (wxpay != '' && wxpay.errMsg.includes("requestPayment:ok")) {
            tools.loading("预约成功", 'success')
            setTimeout(() => {
              tools.goBack(1)
            }, 3000);
          } else {
            await pay.deleteLastFormId()
            tools.loading("取消支付");
          }
          wx.hideNavigationBarLoading()
        } else {
          let showTip = await tools.showModal('是否使用储值支付')
          if (showTip.confirm) {
            setTimeout(() => {
              tools.loading("预约成功", 'success')
              wx.hideNavigationBarLoading()
              setTimeout(() => {
                tools.goBack(1)
              }, 3000);
            }, 1500);
          }
        }
      } else {
        tools.showModal(data.msg, false)
        wx.hideNavigationBarLoading()
      }
    })

  },
  //预约列表
  subMore: async function (target) {
    let vm = target.vm;
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    let app = wepy.$instance;
    let aid = _get(app.globalData, 'aid', '') || await core.getAid();
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    http.get(addr.GetSubscribeFormDetail, {
      aid,
      uid: userInfo.Id,
      pageindex: vm.pageindex,
      pagesize: vm.pagesize
    }).then(subInfo => {
      vm.ispost = false;
      if (subInfo.isok) {
        var len = subInfo.list.length
        len >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true
        len > 0 ? vm.list = [...vm.list, ...subInfo.list] : ""
        for (var i = 0; i < len; i++) {
          var listItem = subInfo.list[i]
          listItem.formdatajson = (listItem.formdatajson || "").split(",")
          listItem.remark = JSON.parse(listItem.remark)
          for (let j = 0, leng = listItem.formdatajson.length;j<leng;j++){
            listItem.formdatajson[j]=listItem.formdatajson[j].split(':')
          }
        }
        target.vm = vm
        target.$apply()
      }
    })
  },
  // 资讯列表
  getNewsLst: async function (vm, target) {
    let aid = wepy.$instance.globalData.aid
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = false
    http.get(addr.GetNewsList, {
      aid,
      typeid: vm.ids,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
      keyMsg: vm.search,
    }).then(data => {
      vm.ispost = false
      if (data.isok) {
        data.data.forEach(function (o, i) {
          o.addtime = tools.ChangeDateFormat(o.addtime)
        })
        data.data.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        data.data.length > 0 ? vm.list = [...vm.list, ...data.data] : '';
        target.vm_com_newlst = vm
        target.$apply()
      }
    })
  },
  // 全部资讯
  allNews: async function (key, currentCom, target) {
    let vm_news = {};
    let typeid = currentCom.typeid;
    let aid = wepy.$instance.globalData.aid
    http.get(addr.GetNewsList, {
      aid,
      typeid,
      liststyle: currentCom.liststyle,
    }).then(allNewsInfo => {
      if (allNewsInfo && allNewsInfo.isok) {
        vm_news.list = allNewsInfo.data;
        if (allNewsInfo.data.length >= 5) {
          vm_news.showmore = true
        } else {
          vm_news.showmore = false
        }
        // 时间戳转换
        vm_news.list.forEach(function (o, i) {
          o.addtime = tools.ChangeDateFormat(o.addtime)
          o.content = []
        })
        target.vm_com_news[key] = vm_news;
        target.$apply();
      }
    })

  },
  // 选择资讯
  chooseNews: async function (key, currentCom, target) {
    let _newsid = []
    let ids = []
    for (let i = 0, len = currentCom.list.length; i < len; i++) {
      if (len < 4) {
        _newsid.push(currentCom.list[i].id)
      }
      ids.push(currentCom.list[i].id)
    }
    let _newstids = _newsid.join(",");
    if (_newsid.length > 0) {
      http.get(addr.GetNewsInfoByids, {
        ids: _newstids,
        liststyle: currentCom.liststyle,
      }).then(data => {
        if (data && data.isok && data.msg.length > 0) {
          let vm = {
            ids: ids,
            list: data.msg,
          }
          // 时间戳转换
          vm.list.forEach(function (o, i) {
            o.addtime = tools.ChangeDateFormat(o.addtime)
            o.content = []
          })
          target.vm_com_news[key] = vm;
          target.$apply();
        }
      })
    }
  },
  /**
   * 咨询详情
   * @param {vm.ispay} 是否付费
   * @param {vm.ispaid}是否购买过
   * @param {playVideo}视频控制显隐
   * @param {playAudio}音频播放
   * @param{playContent}文章
   * @param {payMask}支付弹窗
   * @param {selIndex}选择支付方式
   */
  getNewInfo: async function (id, target) {
    let app = wepy.$instance
    http.get(addr.GetNewsInfo, {
      id,
      appid: _get(app.globalData, "appid", ""),
      openId: _get(app.globalData.userInfo, "OpenId", ""),
      version: 2,
    }).then(async data => {
      if (data.isok) {
        let tmp = data.msg;
        tmp.imgurl_fmt = tmp.img_fmt;
        if (tmp.slideimgs_fmt && tmp.slideimgs != "") {
          tmp.slideimgs_fmt = tmp.slideimgs_fmt.split("|");
          tmp.slideimgs = tmp.slideimgs.split(",");
        }
        if (tmp.payinfo) {
          if (Number(tmp.payinfo.PayAmount) == 0) {
            tmp.ispaid = true
          }
          // 文章收费
          if (Number(tmp.contenttype) == 0) {
            tmp.playContent = tmp.ispay && tmp.ispaid == false ? true : false
          }

          tmp.payinfo.PayAmount = ((tmp.payinfo.PayAmount).div(100)).toFixed(2)
        }
        tmp.playVideo = false
        tmp.playAudio = false
        tmp.isShowBtn = false
        tmp.payMask = false
        tmp.selIndex = 0
        tmp.userInfo = app.globalData.userInfo
        tmp.valuemoney = app.globalData.vipInfo.valueMoney
        tmp.RecommendedItem ? target.getHotList(tmp.RecommendedItem) : ""
        // 时间戳转换
        tmp.addtime = tools.ChangeDateFormat(tmp.addtime);
        // 替换富文本标签 控制样式
        tmp.content_fmt = tools.richChange(tmp.content, target);
        target.vm = tmp;
        target.$apply();
        tools.setPageTitle(tmp.title);
        core.UpdateNewsPV(id, target)
      } else {
        await tools.showModal("页面不存在", false)
        tools.goBack(1)
      }
    })
  },
  addShopCar: async function (para) {
    let app = wepy.$instance;
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.addGoodsCarData, {
      qty: para.count,
      goodid: para.pid,
      isgroup: para.isgroup,
      SpecInfo: para.SpecInfo,
      openid: userInfo.OpenId,
      attrSpacStr: para.specId,
      newCartRecord: para.record,
      appId: app.globalData.appid,
      SpecImg: _get(para, 'img', ""),
    })
  },
  /***************************购物车请求***********************************************/
  shopCarList: async function (target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let vipInfo = _get(app.globalData, 'vipInfo', '') || await core.getVipInfo();
    http.get(addr.getGoodsCarData_new, {
      appId: app.globalData.appid,
      openid: userInfo.OpenId,
      levelid: vipInfo.levelid || 0,
    }).then(data => {
      if (data.isok == 1) {
        var len = Object.keys(data.postdata).length
        if (len) {
          for (let i = 0; i < len; i++) {
            var postdataItem = data.postdata[i]
            if (postdataItem.goodsMsg.pickspecification) {
              postdataItem.goodsMsg.pickspecification = JSON.parse(postdataItem.goodsMsg.pickspecification)
              for (let j = 0, jen = postdataItem.goodsMsg.pickspecification.length; j < jen; j++) {
                for (let k = 0, ken = postdataItem.goodsMsg.pickspecification[j].items.length; k < ken; k++) {
                  postdataItem.goodsMsg.pickspecification[j].items[k].sel = false
                }
              }
            }
          }
        } else {
          data.postdata = []
        }
        target.vm.list = data.postdata
        target.$apply()
      } else {
        tools.showModal(data.msg, false)
      }
    })
  },
  //function:0为编辑-1为删除
  update: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.postJson(addr.updateOrDeleteGoodsCarData, {
      openid: userInfo.OpenId,
      appid: app.globalData.appid,
      function: vm.date,
      goodsCarModel: vm.model,
    })
  },
  //店铺配置
  getStoreConfig: async function () {
    let app = wepy.$instance;
    let wxServer = false
    let pages = _get(app.globalData.pages, "pages", "") || await core.getPageSetting();
    for (let i = 0, len = pages.length; i < len; i++) {
      let temp = pages[i].coms.find(f => f.type.includes("contactShopkeeper"))
      if (temp) {
        if (temp.openService) {
          wxServer = temp.serverType.includes("wxServer") ? true : false;
        }
        break;
      } else {
        wxServer = false
      }
    }
    let storeConfig = _get(app.globalData, 'storeConfig', '')
    if (storeConfig) {
      return storeConfig
    } else {
      let storeConfig = await http.get(addr.GetStoreInfo, {
        appId: app.globalData.appid
      });
      if (storeConfig.isok) {
        storeConfig.postData.storeInfo.funJoinModel.wxServer = wxServer
        app.globalData.storeConfig = storeConfig.postData
        return storeConfig.postData;
      }
    }
  },
  //下单
  addMinOrder: async function (vm) {
    let app = wepy.$instance
    let aid = _get(app.globalData, 'aid', '') || await core.getAid();
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.addMiniappGoodsOrder, {
      aid: aid,
      appId: app.globalData.appid,
      openid: userInfo.OpenId,
      goodCarIdStr: vm.carId,
      wxaddressjson: vm.address,
      orderjson: vm.order,
      buyMode: vm.buyMode, //1是微信支付  2是储值支付
      getWay: vm.getWay, //1是商家（快递）  0到店自取
      isgroup: vm.isgroup,
      groupid: vm.groupid,
      goodtype: vm.goodtype,
      couponlogid: vm.couponlogid,
      zqstoreName: _get(vm, 'storename', ''),
      salesManRecordId: _get(vm, 'salesManRecordId', 0),
      storecodeid: _get(app.globalData, 'storecodeid', 0),
      discountType: _get(vm, 'discountType', 0),
      flashItemID: _get(vm, 'flashDealId', ''),
      zqStoreId: _get(vm, "zqStoreId", "")
    })
  },
  /***************************订单详情***********************************************/
  orderDtl: async function (orderId, target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let info = await http.get(addr.getMiniappGoodsOrderById, {
      appid: app.globalData.appid,
      openid: userInfo.OpenId,
      orderId: orderId
    })
    if (info.isok == 1) {
      var vm = info.postdata
      for (let i = 0, len = vm.goodOrderDtl.length; i < len; i++) {
        var goodOrderDtlItem = vm.goodOrderDtl[i]
        goodOrderDtlItem.ImgUrl = goodOrderDtlItem.goodImgUrl
        goodOrderDtlItem.Introduction = goodOrderDtlItem.goodname
        goodOrderDtlItem.SpecInfo = goodOrderDtlItem.orderDtl.SpecInfo
        goodOrderDtlItem.discountPrice = goodOrderDtlItem.orderDtl.priceStr
        goodOrderDtlItem.oldPrice = goodOrderDtlItem.orderDtl.originalPriceStr
        goodOrderDtlItem.Count = goodOrderDtlItem.orderDtl.Count
        goodOrderDtlItem.type = 'good'
      }
      target.vm_order = vm
      target.$apply()
    }
  },
  /***************************订单列表***********************************************/
  minOlt: async function (target) {
    let vm = target.vm
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    http.get(addr.getMiniappGoodsOrder, {
      appId: app.globalData.appid,
      openid: userInfo.OpenId,
      State: vm.state,
      pageindex: vm.pageindex,
      pagesize: vm.pagesize
    }).then(orderInfo => {
      vm.ispost = false; //请求完毕，关闭请求开关
      if (orderInfo.isok) {
        //更改状态数据
        orderInfo.postdata.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        orderInfo.postdata.length > 0 ? vm.list = [...vm.list, ...orderInfo.postdata] : '';
        target.vm = vm
        target.condition = vm.state
        target.$apply();
      }
    })
  },
  // 更改订单状态
  oltState: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.updateMiniappGoodsOrderState, {
      appid: app.globalData.appid,
      openid: userInfo.OpenId,
      orderId: vm.orderId,
      State: vm.state
    })
  },
  /**
   * @method bargainDlt 砍价详情
   * @param {data.haveCreatOrder 是否下单 false为未下单，true已下单}
   * @param {vm.scene是否来自帮砍}
   * @param {isFriend 0是自己 ，1是帮砍}
   */

  bargainDlt: async function (Id, target, scene) {
    let vm = {}
    let app = wepy.$instance;
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo()
    let store = _get(app.globalData, 'storeConfig', '') || await core.getStoreConfig()
    http.get(addr.GetBargain, {
      UserId: userInfo.Id,
      appid: app.globalData.appid,
      Id: Id,
    }).then(async data => {
      if (data.isok) {
        vm.obj = data.obj
        if (vm.obj.stateStr.includes('已失效')) {
          await tools.showModal("该砍价已失效", false)
          tools.goBack(1)
          return;
        }

        if (vm.obj.startDateStr && vm.obj.endDateStr) {
          vm.obj.startDateStr = vm.obj.startDateStr.replace(/-/g, "/")
          vm.obj.startDateStr = vm.obj.endDateStr.replace(/-/g, "/")
        }
        vm.num = 4 //初始化帮砍更多
        vm.userInfo = userInfo
        vm.haveCreatOrder = data.haveCreatOrder
        target.vm = vm
        target.$apply()
        if (scene.length) {
          vm.buid = scene[0]
          let temp = vm.obj.BargainUserList.find(f => f.Id == vm.buid)
          if (temp) {
            vm.bargainUImg = temp.ShopLogoUrl
            vm.bargainUname = temp.ShopName
            temp.State != 8 ? vm.currentPrice = temp.CurrentPriceStr : '';
          }
          core.cutPrice(vm.buid, target, 1, Id, scene)
          if (Number(vm.currentPrice) == Number(vm.obj.FloorPriceStr)) {
            vm.precent = '100%'
          } else {
            vm.precent = core.compBarPrice(vm.obj.OriginalPriceStr, vm.currentPrice)
          }
        } else {
          vm.bargainUImg = userInfo.HeadImgUrl
          vm.bargainUname = userInfo.NickName
          for (let i = 0, len = vm.obj.BargainUserList.length; i < len; i++) {
            if (userInfo.Id == vm.obj.BargainUserList[i].UserId && vm.obj.BargainUserList[i].State != 8) {
              vm.currentPrice = vm.obj.BargainUserList[i].CurrentPriceStr
            }
          }
          if (Number(vm.currentPrice) == Number(vm.obj.FloorPriceStr)) {
            vm.precent = '100%'
          } else {
            vm.precent = core.compBarPrice(vm.obj.OriginalPriceStr, vm.currentPrice)
          }
          if (vm.haveCreatOrder) {
            vm.isFriend = 0
            let showModal = await tools.showModal("您已经下过单了，请进入砍价单查看详情!");
            showModal.confirm ? target.$navigate("/pages/bargain/bargainList") : '';
          } else {
            core.addBargain(Id, target, scene)
          }
        }
        //倒计时
        target.barCutDown = setInterval(async () => {
          target.vm.time = await tools.TimeShow(vm.obj.startDateStr, vm.obj.startDateStr);
          target.vm.time = target.vm.time.split(":")
          target.$apply();
        }, 1000);
      } else {
        await tools.showModal(data.msg, false)
        tools.goBack(1)
      }
    })
  },

  // 申请砍价
  addBargain: async function (Id, target, scene) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo()
    http.post(addr.AddBargainUser, {
      Id: Id,
      UserId: userInfo.Id,
      UserName: userInfo.NickName
    }).then(data => {
      if (data.isok) {
        core.cutPrice(data.buid, target, 0, Id, scene)
        target.vm.buid = data.buid
        target.$apply()
      } else {
        tools.showModal(data.msg, false)
      }

    })
  },
  // 开始砍价
  cutPrice: async function (buid, target, isFriend, Id, scene) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let vm_bn = target.vm
    http.post(addr.cutprice, {
      UserId: userInfo.Id,
      buid: buid
    }).then(data => {
      let vm = {}
      switch (data.code) {
        case -1:
          vm_bn.isFriend = isFriend
          tools.showModal(data.msg, false)
          break;
        case 0:
          if (data.obj == 0) {
            vm.content = '您已砍过,自砍倒计时1分钟！'
          } else {
            vm.timeArray = JSON.stringify(data.obj).split(".")
            if (vm.timeArray.length == 1) {
              vm.timeArray[1] = 0
            } else {
              vm.timeArray[1] = parseInt(parseInt(vm.timeArray[1]) * 0.6)
            }
            vm.content = '您已砍过,' + vm.timeArray[0] + '小时' + vm.timeArray[1] + '分钟' + '之后才能继续自砍'
          }
          tools.showModal(vm.content, false)
          vm_bn.isFriend = 0
          vm_bn.barShow = false
          break;
        case 1:
          tools.showModal(data.msg, false)
          vm_bn.isFriend = 1
          break;
        case 2:
          userInfo.NickName == data.BargainedUserName ? vm_bn.isFriend = 0 : vm_bn.isFriend = 1;
          vm_bn.currentPrice = data.price
          vm_bn.precent = core.compBarPrice(vm_bn.obj.OriginalPriceStr, data.price)
          vm_bn.BargainedUserName = data.BargainedUserName
          vm_bn.barShow = true
          vm_bn.cutprice = data.cutprice
          target.vm = vm_bn
          target.bargainShow = true
          break;
      }
      target.vm = vm_bn
      target.$apply()
    })

  },
  compBarPrice: function (orinalPrice, currentPrice) {
    let _cutPrice = Number(orinalPrice).sub(Number(currentPrice))
    let _oPrice = Number(orinalPrice).sub(_cutPrice)
    let _newPrice = 100 - (Number(_oPrice).div(Number(orinalPrice)).mul(100))
    return parseFloat(_newPrice).toFixed(2)
  },
  //获取砍价单列表
  bargainList: async function (target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let vm = target.vm;
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    http.post(addr.GetBargainUserList, {
      appId: app.globalData.appid,
      UserId: userInfo.Id,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
      State: vm.state,
    }).then(info => {
      vm.ispost = false; //请求完毕，关闭请求开关
      if (info.isok) {
        for (let i = 0, len = info.obj.length; i < len; i++) {
          if (info.obj[i].GoodsFreightStr == '') {
            info.obj[i].GoodsFreightStr = '0.00'
          }
          info.obj[i].CreateDateStr = info.obj[i].CreateDateStr.split(" ");
        }
        info.obj.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        info.obj.length > 0 ? vm.list = [...vm.list, ...info.obj] : '';
        target.vm = vm
        target.$apply();
      }
    })


  },
  //查看砍价记录
  barShare: async function (buid, target) {
    let info = await http.post(addr.GetBargainRecordList, {
      buid: buid,
      pageIndex: 1,
      pageSize: 100,
    })
    if (info.isok) {
      let barShare = info.obj
      for (var i = 0, len = barShare.length; i < len; i++) {
        barShare[i].CreateDate = tools.ChangeDateFormat(barShare[i].CreateDate)
      }
      target.vm.HelpCut = barShare
      target.$apply()
    } else {
      tools.showModal(info.msg, false)
    }
  },
  // 获取默认地址
  getAddress: async function () {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let info = await http.get(addr.GetUserWxAddress, {
      appid: app.globalData.appid,
      userid: userInfo.Id
    })
    let address = info.obj.WxAddress.WxAddress
    if (address) {
      address = JSON.parse(address)
      return address
    } else {
      return ""
    }
  },
  // 砍价下单
  addBarOrder: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.AddBargainOrder, {
      AppId: app.globalData.appid,
      UserId: userInfo.Id,
      buid: vm.buid,
      address: vm.address,
      Remark: vm.Remark,
      PayType: vm.PayType
    })
  },
  //现价购买
  getBarPrice: async function (buid, target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    http.post(addr.GetBargainUser, {
      buid: buid,
      userid: userInfo.Id
    }).then(async data => {
      if (data.isok) {
        let vm = {
          list: [],
          totalCount: 1,
          fee: data.obj.Freight,
          buid: buid,
          totalPrice: data.obj.curPrcie,
          originalPrice: data.obj.curPrcie,
        }
        vm.list.push({
          ImgUrl: data.obj.ImgUrl,
          oldPrice: "",
          SpecInfo: "",
          Introduction: data.obj.BName,
          discount: 100,
          discountPrice: data.obj.curPrcie,
          Count: 1
        });
        target.$preload("vm_order", vm);
        target.$navigate("/pages/bargain/bargainOrder");
      } else {
        let showModal = await tools.showModal("您已经下过单了，请进入砍价单查看详情!");
        if (showModal.confirm) {
          target.$navigate("/pages/bargain/bargainList");
        }
      }
    })
  },
  //砍价邀请分享
  getShare: async function (vm, target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    tools.showLoading()
    http.post(addr.GetShareCutPrice, {
      appId: app.globalData.appid,
      UserId: userInfo.Id,
      buid: vm.buid,
      bId: vm.bId,
    }).then(info => {
      if (info.isok) {
        target.vm.qrcode = info.qrcode
        target.$apply();
        canvas.barCanvas(info.qrcode, target)
      } else {
        tools.showModal(info.msg, false)
        wx.hideLoading()
      }
    })
  },
  //砍价订单详情
  getBarOlt: async function (buid, target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let info = await http.post(addr.GetOrderDetail, {
      buid,
      AppId: app.globalData.appid,
      UserId: userInfo.Id,
    })
    let vm = info.obj.OrderDetail
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
    vm.GetWay = 1
    vm.QtyCount = 1 //数量
    vm.BuyMode = vm.PayType //支付方式
    vm.OrderNum = vm.OrderId //订单号
    vm.PayDateStr = vm.BuyTimeStr //支付时间
    vm.Address = vm.AddressDetail
    vm.OrderId = vm.CityMordersId
    vm.Message = vm.Remark || null
    vm.AccepterTelePhone = vm.TelNumber
    vm.AccepterName = vm.AddressUserName
    vm.OnlyGoodsMoney = vm.CurrentPriceStr //价格
    vm.CreateDateStr = vm.CreateOrderTimeStr //下单时间
    vm.BuyPriceStr = vm.PayAmount //价格
    vm.DistributeDateStr = tools.ChangeDateFormat(vm.SendGoodsTime) //发货时间：
    vm.AcceptDateStr = tools.ChangeDateFormat(vm.ConfirmReceiveGoodsTime) //成交时间
    target.vm_order.freightPrice = parseFloat(Number(vm.FreightFee).div(100)).toFixed(2) //运费
    target.vm_order.goodOrder = vm
    target.vm_order.goodOrderDtl = []
    target.vm_order.goodOrderDtl.push({
      ImgUrl: vm.ImgUrl,
      Introduction: vm.BName,
      discountPrice: vm.CurrentPriceStr,
      discount: 100,
      Count: 1
    })
    target.$apply()
  },
  //确认收货
  confirmBar: async function (buid, target, type) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let info = await http.post(addr.ConfirmReceive, {
      buid: buid,
      userid: userInfo.Id,
      appId: app.globalData.appid
    })
    if (info.isok) {
      if (type == 0) {
        target.vm = tools.resetArray(target.vm)
        await core.bargainList(target)
      } else {
        await core.getBarOlt(buid, target)
      }
      tools.loading("收货成功", 'success')
    } else {
      tools.showModal(info.msg, false)
    }
  },
  /****************************************** 我的页面*******************************************/
  updateWxCard: async function (target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    http.post(addr.UpdateWxCard, {
      appid: app.globalData.appid,
      UserId: userInfo.Id,
      type: 2
    }).then(info => {
      if (info.msg.includes("还未生成会员卡(请到后台设置同步微信会员卡)")) {
        return;
      } else {
        core.getWxCard(userInfo.Id, target)
      }
    })
  },
  // 会员卡请求
  getWxCard: function (UserId, target) {
    let app = wepy.$instance;
    http.get(addr.GetWxCardCode, {
      appid: app.globalData.appid,
      UserId: UserId,
      type: 2
    }).then(info => {
      let wxCard = false
      if (info.isok) {
        info.obj == null ? wxCard = true : wxCard = false
      } else {
        wxCard = false
      }
      target.vm.wxCard = wxCard
      target.$apply()
    })
  },
  // 获取会员卡Sign(签名)
  getCardSign: async function () {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.get(addr.GetCardSign, {
      appid: app.globalData.appid,
      UserId: userInfo.Id,
      type: 2
    })
  },
  // 提交code到服务器
  saveWxCard: async function (code, target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    http.post(addr.SaveWxCardCode, {
      appid: app.globalData.appid,
      UserId: userInfo.Id,
      code: code,
      type: 2
    }).then(info => {
      if (info.isok) {
        core.updateWxCard(target)
      }
    })
  },
  /****************************************** 储值*******************************************/
  getPrice: async function () {
    let app = wepy.$instance;
    let info = await http.get(addr.getSaveMoneySetList, {
      appid: app.globalData.appid
    })
    if (info.isok) {
      let _g = info.saveMoneySetList
      for (let i = 0, len = _g.length; i < len; i++) {
        _g[i].sel = false
      }
      return _g
    } else {
      return ""
    }
  },
  //充值列表
  getSaveList: async function (target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let info = await core.getPrice();
    target.vm_save.price = info;
    target.vm_save.user = userInfo
    target.$apply();
  },
  //充值请求
  addSavePrice: async function (saveMoneySetId) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.addSaveMoneySet, {
      appid: app.globalData.appid,
      openid: userInfo.OpenId,
      saveMoneySetId: saveMoneySetId
    })
  },
  // 历史充值列表
  getMoneyRec: async function (target) {
    let app = wepy.$instance
    let vm = target.vm_record
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    tools.showLoading()
    let info = await http.post(addr.GetPayLogList, {
      appid: app.globalData.appid,
      openid: userInfo.OpenId,
      pageindex: vm.pageindex,
      pagesize: vm.pagesize
    })
    if (info.isok) {
      vm.ispost = false
      vm.list[vm.pageindex] = info.dataObj
      target.vm_record = vm
      info.dataObj.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
      target.$apply();
    } else {
      tools.showModal(info.Msg, false)
    }
    wx.hideLoading()
  },
  /****************************************** 团购*******************************************/
  initGroupInfo: async function (groupId, target) {
    let app = wepy.$instance
    let store = _get(app.globalData, 'storeConfig', '') || await core.getStoreConfig();
    if (!groupId) {
      await tools.showModal("团购不存在！", false)
      await tools.goBack(1)
      return;
    }
    let groupInfo = await core.groupInfo(groupId)
    if (!groupInfo.isok) {
      await tools.showModal(groupInfo.msg, false)
      await tools.goBack(1)
      return;
    }
    var _g = groupInfo.groupdetail;
    _g.slideimgs = []
    _g.slideimgs_fmt = []
    for (let i = 0, len = _g.ImgList.length; i < len; i++) {
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
    _g.content_fmt = tools.richChange(_g.Description, target);
    _g.ValidDateStart = _g.ValidDateStart.replace(/-/g, "/");
    _g.ValidDateEnd = _g.ValidDateEnd.replace(/-/g, "/");
    _g.imswitch = store.storeInfo.funJoinModel.imSwitch
    //保存
    target.vm_group = _g
    target.$apply()
  },
  groupTime: function (startDateStr, endDateStr) {
    let [groupstate, timeInter] = [0, "00:00:00"]
    let end = endDateStr.replace(/-/g, "/")
    let star = startDateStr.replace(/-/g, "/")
    let timeQueue = ['days', 'hours', 'min', 'seconds']
    let starTime = (new Date(star)) - (new Date()); //计算剩余的毫秒数
    let fromTheEnd_txt = ""
    if (starTime > 0) {
      timeQueue = timeTools.getRemainTimeQueue(starTime) //根据剩余毫秒数转换对应日期数组
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
      timeQueue = timeTools.getRemainTimeQueue(endTime) //根据剩余毫秒数转换对应日期数组
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
  initCountDown: function (vm_group) {
    if (vm_group.GroupSponsorList != null) {
      var list = vm_group.GroupSponsorList;
      if (list.length > 0) {
        for (var i = list.length - 1; i >= 0; i--) {
          var timespan = timeTools.getTimeSpan(list[i].ShowEndTime);

          if (timespan <= 0) {
            list.splice(i, 1)
          } else {
            var timeFormatArray = timeTools.formatMillisecond(timespan);
            var timeFormat = "";
            timeFormat += timeFormatArray[0] + ":" + timeFormatArray[1] + ':' + timeFormatArray[2] + ':' + timeFormatArray[3];
            list[i].countdown = timeFormat;
          }
        }
        return list
      } else {
        return ""
      }
    }
  },
  groupLstCountDowm: function (vm) {
    let list = vm
    var timespan = timeTools.getTimeSpan(list.ShowDate);
    if (timespan <= 0) {
      list.splice(i, 1)
    } else {
      var timeFormatArray = timeTools.formatMillisecond(timespan);
      var timeFormat = "";

      timeFormat += timeFormatArray[1] + ':' + timeFormatArray[2] + ':' + timeFormatArray[3];
      list.countdown = timeFormat;
    }
    return list

  },
  // 确认收货
  groupRece: function (id) {
    return http.post(addr.RecieveGoods, {
      guid: id
    })
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
    http.post(addr.CancelPay, {
      guid: guid,
      appId: app.globalData.appid
    })
  },
  //支付成功
  paySuccess: function (_d) {
    let app = wepy.$instance
    return http.post(addr.GetPaySuccessGroupDetail, {
      appId: app.globalData.appid,
      gsid: _d.gsid,
      orderid: _d.orderid,
      paytype: _d.paytype,
    })
  },
  //订单详情
  getOlt: function (guid) {
    let app = wepy.$instance
    return http.get(addr.GetGroupOrderDetail, {
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
        tools.loading("转发成功", 'success')
      },
    }
  },
  //拼团列表
  getGroupList: async function (target) {
    let app = wepy.$instance
    let vm = target.vm
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let info = await http.post(addr.GetMyGroupList, {
      appId: app.globalData.appid,
      userId: userInfo.Id,
      t: vm.state,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
    })
    if (info.isok) {
      vm.ispost = false;
      target.vm.list[vm.pageindex] = info.postdata
      info.postdata.length < vm.pagesize ? vm.loadall = true : vm.pageindex += 1
      target.$apply()
    } else {
      tools.showModal(info.msg, false)
    }
  },
  //参团详情
  myGroupDlt: async function (id) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.GetMyGroupDetail, {
      appId: app.globalData.appid,
      userId: userInfo.Id,
      groupsponId: id,
    })
  },
  groupInvite: async function (id, target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let info = await http.post(addr.GetInvitePageData, {
      appId: app.globalData.appid,
      gsid: id,
    })
    if (info.isok) {
      var isingroup = false;
      let _g = info.postdata
      _g.content_fmt = tools.richChange(_g.Description, target);
      if (_g.GroupUserList.length > 0) {
        var obj = _g.GroupUserList.find(function (item) {
          return item.Id == userInfo.Id
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
      target.isingroup = isingroup
      target.vm_dlt = _g
      target.$apply()
      let that = target
      that.cutDown = setInterval(async res => {
        let start = _g.ValidDateStart
        let end = _g.ValidDateEnd
        that.time = await core.groupTime(start, end)
        that.$apply()
      }, 1000)
    } else {
      tools.showModal(info.msg, false)
    }
  },
  //进行中的拼团列表
  groupIng: async function (target) {
    let app = wepy.$instance
    let vm = target.vm
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    let info = await http.post(addr.GetGroupList, {
      appId: app.globalData.appid,
      state: vm.state,
      pageIndex: vm.pageindex
    })
    if (info.isok) {
      vm.ispost = false;
      target.vm.list[vm.pageindex] = info.postdata
      info.postdata.length < vm.pagesize ? vm.loadall = true : vm.pageindex += 1
      target.$apply()
    } else {
      tools.showModal(info.msg, false)
    }
  },
  /***********************************************拼团2.0**************************************************************/
  getMinOrderId: async function (id) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.get(addr.getMiniappGoodsOrderById, {
      appid: app.globalData.appid,
      openid: userInfo.OpenId,
      orderId: id
    })
  },
  //拼团2.0列表
  getEntGroup: async function (target) {
    let app = wepy.$instance
    let vm = target.vm
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    http.post(addr.GetMyGroupList2, {
      appId: app.globalData.appid,
      userId: userInfo.Id,
      state: vm.state,
      pageIndex: vm.pageindex
    }).then(data => {
      if (data.isok == 1) {
        vm.ispost = false;
        if (data.postdata) {
          data = core.group2State(data)
          data.postdata.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
          data.postdata.length > 0 ? vm.list = [...vm.list, ...data.postdata] : '';
        }
        target.vm = vm
        target.$apply()
      } else {
        tools.showModal(data.msg, false)
      }
    })
  },
  //拼团订单列表状态
  group2State: function (vm) {
    for (let i = 0, len = vm.postdata.length; i < len; i++) {
      let tmp = vm.postdata[i]
      if (tmp.state == 3) {
        tmp.showMark = '已完成';
        tmp.state = 4;
      }
      if (tmp.groupstate == 0 && tmp.state == 0) {
        tmp.showMark = '待付款';
        tmp.state = 0;
      }
      if (tmp.groupstate == 2 && tmp.state == 1) {
        tmp.showMark = '待发货';
        tmp.state = 5;
      }
      if (tmp.groupstate == 2 && tmp.state == 8) {
        tmp.showMark = "待发货";
        tmp.state = 5
      }
      if (tmp.groupstate == -4 && tmp.state == -4) {
        tmp.showMark = "未成团";
        tmp.state = 1;
      }
      if (tmp.groupstate == 0 && tmp.state == -1) {
        tmp.showMark = "订单失效";
        tmp.state = 6;
      }
      if (tmp.groupstate == 1 && tmp.state == -1) {
        tmp.showMark = "订单失效";
        tmp.state = 6;
      }
      if (tmp.groupstate == 2 && tmp.state == -1) {
        tmp.showMark = "订单失效";
        tmp.state = 6
      }
      if (tmp.groupstate == 1 && tmp.state == 8) {
        tmp.showMark = "未成团";
        tmp.state = 1
      }
      if (tmp.groupstate == 1 && tmp.state == 1) {
        tmp.showMark = "未成团";
        tmp.state = 1
      }
      if (tmp.groupstate == 2 && tmp.state == 2) {
        tmp.showMark = "待收货";
        tmp.state = 9
      }
      if (tmp.groupstate == 2 && tmp.state == -4) {
        tmp.showMark = "退款成功";
        tmp.state = 7
      }
      if (tmp.groupstate == -4 && tmp.state == -1) {
        tmp.showMark = "已过期";
        tmp.state = 8
      }
    }
    return vm
  },
  //更改状态
  async groupConfrim(vm, target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    http.post(addr.updateMiniappGoodsOrderState, {
      appid: app.globalData.appid,
      openid: userInfo.OpenId,
      orderId: vm.Id,
      State: vm.state
    }).then(async data => {
      if (data.isok == 1) {
        target.vm = tools.resetArray(target.vm)
        if (vm.state == 3) {
          await tools.loading("收货成功", 'success')
        } else {
          await tools.loading("取消订单成功", 'success')
        }
        await core.getEntGroup(target);
      } else {
        tools.showModal(data.msg, false)
      }
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
        tools.loading("转发成功", 'success')
      },
    }
  },
  /***********************************************收货地址选择**************************************************************/
  getAddresslt: async function (target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let store = _get(app.globalData, 'storeConfig', '') || await core.getStoreConfig();
    let info = await http.post(addr.GetUserAddress, {
      userId: userInfo.Id
    })
    if (info.isok) {
      let _g = info.data
      for (let i = 0, len = _g.length; i < len; i++) {
        _g[i].address = _g[i].province + _g[i].city + _g[i].district + _g[i].street
      }
      if (store.storeInfo.funJoinModel.openExpress == false && (store.storeInfo.funJoinModel.openInvite || store.storeInfo.funJoinModel.openToStoreConsume)) {
        if (store.storeInfo.funJoinModel.openInvite) {
          target.selIndex = 1
        } else {
          target.selIndex = 6
        }
        let ln = await tools.getLocation()
        if (ln) {
          target.vm_more.lat = ln.latitude
          target.vm_more.lng = ln.longitude
          core.GetStorePickPlace(target.vm_more, target)
          target.$apply();
        } else {
          core.GetStorePickPlace(target.vm_more, target)
        }
      }
      target.vm_addr.express = _g
      target.vm_addr.selflst = store
      target.$apply()
    } else {
      tools.showModal(info.msg, false)
    }
  },
  GetStorePickPlace: function (vm, target) {
    let app = wepy.$instance
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost) {
      vm.ispost = true
    }
    http.get(addr.GetStorePickPlace, {
      appId: app.globalData.appid,
      lng: _get(vm, "lng", ""),
      lat: _get(vm, "lat", ""),
      pageIndex: _get(vm, "pageIndex", ""),
      pageSize: _get(vm, "pageSize", ""),
    }).then(data => {
      if (data.isok) {
        vm.ispost = false;
        data.dataObj.placeList.length < vm.pageSize ? vm.loadall = true : vm.pageIndex += 1
        data.dataObj.placeList.length > 0 ? vm.list = [...vm.list, ...data.dataObj.placeList] : '';
        target.vm_more = vm
        target.$apply()
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
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
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
  //运费模板
  getFreight: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.GetFreightFee, {
      appId: app.globalData.appid,
      openId: userInfo.OpenId,
      goodCartIds: vm.cartid,
      province: _get(vm, 'province', ""),
      city: _get(vm, 'city', ""),
      flashItemId: _get(vm, "flashId", ""),
      isgroup: _get(vm, "isgroup", ""),
      groupid: _get(vm, "groupid", ""),
      discountType: _get(vm, "discountType", ""),
      couponlogid: _get(vm, "couponlogid", "")
    })
  },
  getLive: function (url) {
    let result = /https?:\/\/vzan.com\/live\/tvchat-(\d+).*/gi.exec(url);
    if (!result) {
      tools.showModal("播放地址不正确", false)
      return;
    }
    let tpid = result[1];
    return http.post(addr.live, {
      tpid: tpid
    })
  },
  /**************************************优惠券*********************************************/
  getCoup: async function (_pro, target) {
    let app = wepy.$instance
    let vm = target.vmMycoupon
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    if (_pro != '') {
      vm.goodsInfo = JSON.stringify(_pro.list.map(function (item, index) {
        return {
          goodid: item.goodid,
          totalprice: Number(item.discountPrice) * item.Count * 100
        }
      }))
    }
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost) {
      vm.ispost = true
    }
    vm.goodsId = _pro.goodid
    let coupInfo = await http.post(addr.GetMyCouponList, {
      state: vm.state,
      goodsId: _get(vm, "goodsId", ""),
      goodsInfo: _get(vm, "goodsInfo", ""),
      pageIndex: vm.pageindex,
      userId: userInfo.Id,
      appId: app.globalData.appid,
    })
    if (coupInfo.isok) {
      vm.ispost = false;
      coupInfo.postdata.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true
      vm.list = [...vm.list, ...coupInfo.postdata]
      target.vmMycoupon = vm
      target.$apply()
    }
  },
  // 领取优惠券
  getCoupon: async function (id) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.GetCoupon, {
      appId: app.globalData.appid,
      userId: userInfo.Id,
      couponId: id,
    })
  },
  //领券中心
  getStoreCoup: function (showTip, state) {
    let app = wepy.$instance
    let userInfo = app.globalData.userInfo
    let coupState = state
    return http.post(addr.GetStoreCouponList, {
      appId: app.globalData.appid,
      goodstype: -1,
      userId: userInfo.Id,
      IsShowTip: showTip || "",
      state: coupState
    })
  },
  /**************************************立减金*********************************************/
  getReduction: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.GetReductionCard, {
      userId: userInfo.Id,
      openId: userInfo.OpenId,
      orderId: vm.orderid,
      couponsId: vm.couponsid
    })
  },
  getReductionLst: async function (type, target) {
    let app = wepy.$instance
    let aid = _get(app.globalData, "aid", "");
    let userInfo = _get(app.globalData, "userInfo", "");
    let store = _get(app.globalData, 'storeConfig', '') || await core.getStoreConfig();
    let data = await http.post(addr.GetReductionCardList, {
      aid,
      userId: userInfo.Id,
      openId: userInfo.OpenId,
      storeId: store.storeInfo.Id,
    })
    if (data.isok) {
      if (Number(type) == 0) {
        return data.coupons
      } else {
        target.vm = data.coupons;
        target.$apply();
      }
    }
  },
  /**************************************积分商城*********************************************/
  getInterInfo: async function () {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.GetUserIntegral, {
      appId: app.globalData.appid,
      userId: userInfo.Id
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
    return http.post(addr.GetStoreRules, {
      appId: app.globalData.appid,
    })
  },
  //积分记录
  interRecord: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.GetUserIntegralLogs, {
      appId: app.globalData.appid,
      userId: userInfo.Id,
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
  //积分下单 way: 默认为0快递发货 1到店消费 2到店自取
  interOrder: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.AddExchangeActivityOrder, {
      userId: userInfo.Id,
      appId: app.globalData.appid,
      activityId: vm.id,
      address: vm.address,
      way: vm.way,
    })
  },
  //订单详情
  interLst: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.GetExchangeActivityOrders, {
      userId: userInfo.Id,
      appId: app.globalData.appid,
      pageindex: vm.pageindex,
      pagesize: vm.pagesize,
    })
  },
  // 积分确认收货
  interConfirm: async function (id) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.ConfirmReciveGood, {
      userId: userInfo.Id,
      appId: app.globalData.appid,
      orderId: id
    })
  },
  /**********************************************分销中心***************************************************** */
  getMiniSale: async function (id) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.get(addr.GetMiniAppSaleManConfig, {
      UserId: userInfo.Id,
      appId: app.globalData.appid,
      parentSalesManId: id,
    })
  },
  postApply: async function (phone, id) { //申请成为分销员
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.ApplySalesman, {
      UserId: userInfo.Id,
      appId: app.globalData.appid,
      TelePhone: phone,
      parentSalesManId: id,
    })
  },
  getSaleInfo: async function () {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.get(addr.GetSalesManUserInfo, {
      UserId: userInfo.Id,
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
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let pageindex = _get(vm, 'pageindex', 1)
    let pageSize = _get(vm, 'pagesize', 10)
    return http.get(addr.GetSalesManRecordOrder, {
      appId: app.globalData.appid,
      UserId: userInfo.Id,
      pageIndex: pageindex,
      pageSize: pageSize,
    })
  },
  // 累计客户
  getSaleManRecord: async function (vm) {
    let app = wepy.$instance
    let state = _get(vm, 'state', 0)
    let pageindex = _get(vm, 'pageindex', 1)
    let pageSize = _get(vm, 'pagesize', 10)
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.get(addr.GetSalesManRecordUser, {
      appId: app.globalData.appid,
      UserId: userInfo.Id,
      pageIndex: pageindex,
      pageSize: pageSize,
      state: state
    })
  },
  // 绑定分销关系Id
  bindSale: function (goodsid) {
    let app = wepy.$instance
    return http.get(addr.GetSalesManRecord, {
      appId: app.globalData.appid,
      goodsId: goodsid || '',
      salesManId: app.globalData.saleId,
    })
  },
  // 分享绑定
  bindShip: async function (goodsId, record) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.BindRelationShip, {
      appId: app.globalData.appid,
      userId: userInfo.Id,
      goodsId: goodsId,
      salesManRecordId: record,
    })
  },
  //更新推广分享记录状态 默认更新为可用 state=1
  updateRecordId: function (salesManRecordId) {
    let app = wepy.$instance
    http.post(addr.UpdateSalesManRecord, {
      appId: app.globalData.appid,
      salesManRecordId: salesManRecordId,
      state: 1,
    })
  },
  applyCash: async function (drawCashMoney) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.DrawCashApply, {
      appId: app.globalData.appid,
      UserId: userInfo.Id,
      drawCashMoney,
    })
  },
  // 二级分销
  GetSaleManRelationList: function (vm) {
    let app = wepy.$instance
    return http.post(addr.GetSaleManRelationList, {
      appId: app.globalData.appid,
      saleManId: vm.saleManId,
      pageSize: vm.pagesize,
      pageIndex: vm.pageindex,
    })
  },
  cashRecordlst: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.get(addr.GetDrawCashApplyList, {
      appId: app.globalData.appid,
      UserId: userInfo.Id,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
    })
  },
  //储值支付
  payByStore: async function (pickCoupon, paymoney, calmoney, money_coupon, money_vip, payway, discountType) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let vipinfo = _get(app.globalData, 'vipInfo', '') || await core.getVipInfo();
    return http.post(addr.PayByStoredvalue, {
      appid: app.globalData.appid,
      userId: userInfo.Id,
      openId: userInfo.OpenId,
      levelid: vipinfo.levelid,
      couponid: pickCoupon == null ? 0 : pickCoupon.Id,
      money: paymoney * 100, //输入金额
      money_cal: calmoney, //支付金额
      money_coupon: money_coupon,
      money_vip: money_vip,
      payway: payway, //（储值支付：2，微信支付：1）
      discountType,
    })
  },
  // 储值支付成功
  payByStoreSuccess: async function (orderid) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.StoredvalueOrderInfo, {
      orderid: orderid,
      openId: userInfo.OpenId,
    })
  },
  /************************************************私信************************************************** */
  connectSocket: async function () {
    let app = wepy.$instance
    if (app.globalData.wxState) {
      return;
    }
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    if (app.globalData.ws || isConnecting) {
      return;
    }
    isConnecting = true;
    wx.connectSocket({
      url: addr.ws + '?appId=' + app.globalData.appid + '&userId=' + userInfo.Id + '&isFirst=' + isFirst,
      header: {
        'content-type': 'application/json'
      },
      method: "GET"
    });
    wx.onSocketOpen(function (res) {
      app.globalData.ws = true;
      isConnecting = false;

      if (reConnectTimer) {
        clearTimeout(reConnectTimer);
        reConnectTimer = null;
      }

      //重连后，自动重发发送失败的消息
      if (app.globalData.msgQueue.length > 0) {
        for (var i = 0, len = app.globalData.msgQueue.length; i < len; i++) {
          core.sendMessage(app.globalData.msgQueue[i])
        }
        app.globalData.msgQueue = [];
        wx.hideLoading();
      }
      setInterval(() => {
        var msg = {
          appId: _get(app.globalData, "appid"),
          fuserId: userInfo.Id,
          tuserId: "",
          enterPage: "",
          msgType: 2,
          tuserType: 0, //
          msg: "",
          ids: "",
          tempid: userInfo.Id + '_' + new Date().getTime(), //临时ID用来判断消息是否发送成功
          isChat: 2,
        };
        core.sendMessage(msg)
      }, 120000)
    });

    wx.onSocketError(function (res) {
      app.globalData.ws = false;
      isConnecting = false;
    });

    wx.onSocketClose(function (res) {
      isFirst = false;
      app.globalData.ws = false;
      isConnecting = false;
      core.reConnect();
    });

    //接收消息
    wx.onSocketMessage(function (res) {
      let msg = res.data;
      let pages = getCurrentPages();
      let currentPage = pages[pages.length - 1];
      let fuser = currentPage.data.fuserInfo;
      let tuser = currentPage.data.tuserInfo;
      if (typeof msg == "string") {
        msg = JSON.parse(msg);
      }
      //聊天页面
      if (currentPage.route == "pages/im/chat") {
        let list = currentPage.data.vm.list;
        if (msg.fuserId == fuser.userid && msg.tuserId == tuser.userid || msg.fuserId == tuser.userid && msg.tuserId == fuser.userid) {
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
    let app = wepy.$instance
    if (app.globalData.wxState) {
      return;
    }
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
    if (typeof msg == "object") {
      msg = JSON.stringify(msg);
    }
    if (app.globalData.ws) {
      wx.sendSocketMessage({
        data: msg
      })
    } else {
      app.globalData.msgQueue.push(msg);
    }
  },
  getContactList: async function (vm, target) {
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    http.post(addr.GetContactList, {
      appId: app.globalData.appid,
      fuserId: userInfo.Id,
      pageIndex: vm.pageindex,
      pageSize: vm.pagesize,
      fuserType: 0,
      ver: 1,
    }).then(data => {
      if (data.isok) {
        vm.ispost = false;
        data.data.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true
        vm.list = [...vm.list, ...data.data]
        target.vm = vm
        target.$apply()
      } else {
        tools.showModal(data.msg, false)
      }
    })
  },
  AddContact: async function (userid) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    http.post(addr.AddContact, {
      appId: app.globalData.appid,
      fuserId: userInfo.Id,
      tuserId: userid
    })
  },
  getHistory: async function (userid, vm, target) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
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

        vm.list = info.data.concat(vm.list);
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
    target.vm = vm
    target.$apply()
  },
  gochat: async function () {
    let app = wepy.$instance
    let store = _get(app.globalData, 'storeConfig', '') || await core.getStoreConfig();
    if (store.storeInfo && store.storeInfo.funJoinModel) {
      if (store.storeInfo.kfInfo && store.storeInfo.funJoinModel.imSwitch) {
        var userid = _get(store.storeInfo.kfInfo, 'uid', "");
        var nickname = _get(store.storeInfo.kfInfo, 'nickName', "").replace(/\s/gi, "");
        var headimg = _get(store.storeInfo.kfInfo, "headImgUrl", "");
        tools.goNewPage('/pages/im/chat?userid=' + userid + "&nickname=" + nickname + "&headimg=" + headimg)
      } else {
        tools.showModal('商家已关闭在线客服', false)
      }
    }
  },
  /***********************************功能开关************************************** */
  appSwitch: async function () {
    let app = wepy.$instance
    let appid = app.globalData.appid
    let info = await http.get(addr.GetFunctionList, {
      appid,
    })
    if (info.isok) {
      for (let i = 0, len = info.dataObj.list.length; i < len; i++) {
        if (info.dataObj.list[i].title.includes("我的预约单")) {
          info.dataObj.list[i].url = "/pages/good/goodSubLst"
        }
      }
      app.globalData.switchInfo = info.dataObj
      return info.dataObj
    } else {
      app.globalData.switchInfo = ''
      return ''
    }
  },
  // 版本0 旗舰版 1尊享版 2高级版 3基础版
  getVerson: async function () {
    let appid = wepy.$instance.globalData.appid
    let _g = await http.get(addr.GetVersonId, {
      appid,
    })
    if (_g.isok) {
      return _g.dataObj
    } else {
      return ""
    }
  },
  // 底部水印
  logoSwitch: async function (target) {
    let app = wepy.$instance
    let appid = app.globalData.appid;
    let logo = app.globalData.logo
    if (logo) {
      target.vm_com_logo = logo
      target.$apply()
    } else {
      let info = await http.get(addr.GetAgentConfigInfo, {
        appid,
      })
      if (info.isok == 1) {
        let data = info.AgentConfig;
        data.LogoText = data.isdefaul == 0 ? data.LogoText.split(' ') : data.LogoText;
        data.openPath = await core.getVerson();
        app.globalData.logo = data
        target.vm_com_logo = data
        target.$apply()
      } else {
        tools.showModal(info.msg, false)
      }
    }
  },
  /*****************************************官网**************************************** */
  sendUser: function (phone, name) {
    return http.post(addr.SendUserAdvisory, {
      Phone: phone,
      username: name,
      source: 1,
      type: 5,
    })
  },
  getPhoneCode: function (phone) {
    return http.post(addr.SendUserAuthCode, {
      phonenum: phone,
      type: 1,
    })
  },
  getUserRegi: function (_g) {
    return http.post(addr.SaveUserInfo, {
      phone: _g.phone,
      password: _g.password,
      code: _g.code,
      address: _g.address,
      sourcefrom: "小程序",
      agentqrcodeid: 0
    })
  },
  /****************************************************评论********************************************************************/
  postValue: async function (vm, target) {
    let app = wepy.$instance
    let appid = app.globalData.appid
    let userInfo = await core.getUserInfo();
    http.post(addr.AddManyGoodsComment, {
      appid,
      userId: userInfo.Id,
      orderId: vm.orderid, //订单id
      goodsType: vm.goodsType, //0：普通产品，1：拼团产品，2：砍价产品
      listJson: JSON.stringify(vm.list),
      sessionkey: _get(userInfo, 'loginSessionKey', ""),
    }).then(data => {
      if (data.isok) {
        target.vm.showMask = true
        target.$apply()
      } else {
        tools.showModal(data.Msg, false)
      }
    })
  },
  //获取列表评论我的
  getGoodsValue: async function (vm) {
    let app = wepy.$instance
    let appid = app.globalData.appid
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.GetUserGoodsCommentList, {
      appid,
      userid: userInfo.Id,
      pageIndex: _get(vm, 'pageindex', 1),
      pageSize: _get(vm, 'pagesize', 10),
      haveimg: _get(vm, 'sel', -1),
    })
  },
  //获取指定商品评论
  getShowGoodValue: async function (vm) {
    let app = wepy.$instance
    let appid = app.globalData.appid
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.GetGoodsCommentList, {
      appid,
      userid: userInfo.Id,
      goodsid: _get(vm, 'goodsid', '') || vm,
      pageIndex: _get(vm, 'pageindex', 1),
      pageSize: _get(vm, 'pagesize', 10),
      haveimg: _get(vm, 'sel', -1),
    })
  },
  //点赞
  pointValue: async function (id) {
    let app = wepy.$instance
    let appid = app.globalData.appid
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.PointsGoodsComment, {
      id,
      appid,
      userid: userInfo.Id,
    })
  },
  //获取验证码
  GetVaildCode: function (param) {
    let _param = {
      type: 1,
      phonenum: param.phone,
      agentqrcodeid: param.agentqrcodeid,
    }
    return http.post(addr.GetVaildCode, _param);
  },
  //注册账号
  SaveUserInfo: function (param) {
    return http.post(addr.SaveUserInfo, param);
  },
  //核销二维码
  GetTableNoQrCode: function (orderid) {
    let appid = wepy.$instance.globalData.appid
    return http.post(addr.GetTableNoQrCode, {
      appid,
      orderid,
      bussinessAppid: "wxe569b2c80b845da7",
    });
  },
  // 付费支付
  payCtOrder: async function (vm) {
    let app = wepy.$instance
    let _g = await http.post(addr.addPayContentOrder, {
      appId: app.globalData.appid,
      openId: app.globalData.userInfo.OpenId,
      contentId: vm.id,
      buyMode: vm.mode,
    })
    if (_g.isok) {
      return _g.dataObj
    } else {
      tools.showModal(_g.Msg, false)
      return false
    }
  },
  // 秒杀详情
  miaoShaDeail: function (pid, target) {
    let app = wepy.$instance
    http.post(addr.GetFlashItem, {
      appId: app.globalData.appid,
      openId: app.globalData.userInfo.OpenId,
      flashItemID: pid,
    }).then(async data => {
      if (data.isok) {
        let vm = data.dataObj
        vm.goodInfo.selImg = vm.goodInfo.img;
        if (vm.goodInfo.pickspecification) {
          vm.goodInfo.pickspecification = JSON.parse(vm.goodInfo.pickspecification);
          for (let i = 0, len = vm.goodInfo.pickspecification.length; i < len; i++) {
            for (let j = 0, key = vm.goodInfo.pickspecification[i].length; j < key; j++) {
              vm.goodInfo.pickspecification[i].items[j].sel = false
            }
          }
        }
        let store = _get(app.globalData, 'storeConfig', '') || await core.getStoreConfig();
        vm.store = store.storeInfo.funJoinModel
        vm.content_fmt = vm.goodInfo.description.replace(/[<]br[/][>]/g, '<div style="height:20px"></div>');
        vm.content_fmt = vm.goodInfo.description.replace(/&nbsp;/g, '<span style="margin-left:16rpx;"></span>');
        vm.content_fmt = vm.goodInfo.description.replace(/[<][/]p[>][<]p[>]/g, "<div></div>");
        vm.content_fmt = tools.richChange(vm.goodInfo.description, target);
        vm.goodInfo.danMaiPrice = vm.goodInfo.discountPricestr; //用于计算单买价
        vm.goodInfo.yuanJiaPrice = vm.goodInfo.priceStr; //用于计算原价
        vm.goodInfo.itemPrice = vm.goodInfo.discountPricestr; //初始单个产品价格
        vm.goodInfo.stockStr = vm.goodInfo.stock; //初始库存
        vm.goodInfo.totalCount = 1;
        vm.goodInfo.specId = ""; //初始选择分类Id
        vm.goodInfo.type = 'good'
        vm.userInfo = app.globalData.userInfo
        tools.setPageTitle(vm.goodInfo.name);
        target.vm = vm
        target.$apply()

        target.miaoDeal = setInterval(res => {
          let timeFormatArray = []
          let starTime = timeTools.getTimeSpan(vm.flashPayInfo.end);
          timeFormatArray = timeTools.formatMillisecond(starTime);
          target.vm.countDownArray = timeFormatArray
          target.$apply()
        }, 1000)


      } else {
        await tools.showModal(data.Msg, false)
        tools.goBack(1)
      }
    })
  },
  //首页积分签到弹窗
  showSignin: async function (target) {
    let app = wepy.$instance
    let store = _get(app.globalData, 'storeConfig', '') || await core.getStoreConfig();
    let signinRules = store.storeInfo.funJoinModel.PlayCardConfigModel;
    if (signinRules) {
      let showPage = signinRules.ShowPage;
      target.signinRules = signinRules;
      if (showPage == 1) {
        target.showsigninFloat = true;
        target.signinRules = signinRules;
        app.globalData.showsigninFloat = target.showsigninFloat;
        core.getUserPlayCard(target);
      }
      target.$apply();
    }
  },
  async storeInfo(_this) {
    let app = wepy.$instance
    let store = _get(app.globalData, 'storeConfig', '') || await core.getStoreConfig();
    _this.signinRules = store.storeInfo.funJoinModel.PlayCardConfigModel;
    _this.$apply();
  },
  //获取用户签到信息
  async getUserPlayCard(_this) {
    let app = wepy.$instance;
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.get(addr.GetUserPlayCard, {
      appId: app.globalData.appid,
      userId: userInfo.Id,
    }).then(data => {
      if (data.isok) {
        if (data.dataObj) {
          data.dataObj.listPlayCardLog.forEach((item, index) => {
            item['dateStr'] = _this.singinDate[index];
          });
          _this.userSignin = data.dataObj;
          _this.$apply();
        }
      } else {
        tools.showModal(data.Msg, false);
      }
    })
  },
  //用户点击签到
  async playCard(_this) {
    let app = wepy.$instance;
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.PlayCard, {
      appId: app.globalData.appid,
      userId: userInfo.Id,
    }).then(data => {
      if (data.isok) {
        data.dataObj.listPlayCardLog.forEach((item, index) => {
          item['dateStr'] = _this.singinDate[index];
        });
        _this.userSignin = data.dataObj;
        _this.$apply();
      } else {
        tools.showModal(data.Msg, false);
      }
    })
  },
  //记录二维码进入
  async addQrCodeScanRecord(qrcodeId) {
    if (qrcodeId == '') {
      return;
    }
    var app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    return http.post(addr.AddQrCodeScanRecord, {
      appId: app.globalData.appid,
      openId: userInfo.OpenId,
      qrCodeId: qrcodeId
    })
  },
  UpdateNewsPV: function (id, target) {
    http.post(addr.UpdateNewsPV, {
      id,
    }).then(data => {
      if (data.isok) {
        target.vm.Pv = target.vm.Pv + 1
        target.$apply()
      }
    })
  },
  //获取所有会员权益
  GetAllVipRights: async function (target) {
    let app = wepy.$instance
    let vipinfo = _get(app.globalData, 'vipInfo', '') || await core.getVipInfo();
    http.post(addr.GetAllVipRights, {
      appId: app.globalData.appid,
    }).then(async data => {
      if (data.isok) {
        for (let i = 0, len = data.dataObj.length; i < len; i++) {
          if (vipinfo.levelInfo.level == data.dataObj[i].level) {
            if (i + 1 == data.dataObj.length) {
              target.vm.index = data.dataObj.length
            } else {
              target.vm.index = i + 1
            }
          }
        }
        target.vm.list = data.dataObj
        target.vm.info = vipinfo
        target.$apply()
      } else {
        await tools.showModal(data.Msg, false)
        tools.goBack(1)
      }
    })
  },
  //满减
  async GetFullReductionByAid() {
    let aid = _get(wepy.$instance.globalData, 'aid', '')
    let info = await http.post(addr.GetFullReductionByAid, {
      aid
    })
    if (info.isok) {
      if (info.dataObj) {
        return info.dataObj;
      } else {
        return null;
      }
    }
  },
  //获取砍价运费
  GetBargainFreightFee: async function (vm) {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', "") || await core.getUserInfo();
    let info = await http.post(addr.GetBargainFreightFee, {
      city: vm.city,
      buid: vm.build,
      province: vm.province,
      openId: userInfo.OpenId,
      appId: app.globalData.appid,
    })
    if (info.isok) {
      return info.dataObj
    } else {
      return "";
    }
  },
  //订单数量
  GetOrderRecordCount: async function () {
    let app = wepy.$instance
    let userInfo = _get(app.globalData, 'userInfo', "") || await core.getUserInfo();
    let info = await http.get(addr.GetOrderRecordCount, {
      openid: userInfo.OpenId,
      appId: app.globalData.appid,
    })
    if (info.isok) {
      return info.opostdata;
    } else {
      return "";
    }
  },
  getGoodInfo: async function (pid) {
    let app = wepy.$instance;
    let vipInfo = _get(app.globalData, 'vipInfo', '') || await core.getVipInfo();
    let userInfo = _get(app.globalData, 'userInfo', '') || await core.getUserInfo();
    let data = await http.get(addr.GetGoodInfo, {
      pid,
      version: 2,
      levelid: vipInfo.levelid,
      appid: app.globalData.appid,
    })
    if (data.isok) {
      let vm = data.msg;
      vm.userInfo = userInfo;
      vm.hotGoods = data.hotGoods
      vm.imgurl = data.msg.img;
      vm.selImg = data.msg.img;
      vm.imgurl_fmt = data.msg.img_fmt;
      if (vm.slideimgs && vm.slideimgs_fmt) {
        vm.slideimgs = vm.slideimgs.split(",");
        vm.slideimgs_fmt = vm.slideimgs_fmt.split("|");
      }
      if (vm.pickspecification) {
        vm.pickspecification = JSON.parse(vm.pickspecification);
        for (let i = 0, len = vm.pickspecification.length; i < len; i++) {
          for (let j = 0, key = vm.pickspecification[i].length; j < key; j++) {
            vm.pickspecification[i].items[j].sel = false
          }
        }
      }
      vm.content_fmt = tools.richChange(vm.description, this);
      tools.setPageTitle(vm.name);
      return vm;
    } else {
      await tools.showModal(data.msg, false);
      tools.goBack(1)
    }
  },
  //取消订单
  CancelOrder: async function (orderId) {
    let userInfo = await core.getUserInfo();
    let info = await http.post(addr.CancelOrder, {
      orderId,
      sessionkey: _get(userInfo, 'loginSessionKey', ""),
      State: 19,
    })
    tools.showModal(info.Msg, false)
    if (info.isok) {
      return true
    } else {
      return false
    }
  },
  //预约表单配置 
  getSub: async function () {
    let aid = _get(wepy.$instance.globalData, "aid", "");
    let info = await http.get(addr.GetSubscribePageSetting, {
      aid
    })
    if (info.isok) {
      info.page = JSON.parse(info.page)
      //因后台接口调整
      info.page.coms[0].openpay = info.funJoinModel.OpenYuyuePay
      info.page.coms[0].paycount = info.funJoinModel.YuyuePayCount
      info.page.coms[0].paytype = info.funJoinModel.YuyuePayType
      return info.page.coms[0]
    } else {
      return ""
    }
  },
};
/**********************************画布**********************************************************/
var canvas = {
  getShare: function () {
    let app = wepy.$instance;
    let width = wx.getSystemInfoSync().windowWidth * 0.87
    let height = wx.getSystemInfoSync().windowHeight * 0.75
    let ctx = wx.createCanvasContext('firstCanvas')
    http.get(addr.GetShare, {
      appId: app.globalData.appid
    }).then(async shareInfo => {
      if (shareInfo.isok) {
        let _v = await canvasTools.pathStatus(shareInfo.obj, width, height)
        app.globalData.adImg = shareInfo.obj.ADImg.length ? shareInfo.obj.ADImg[0].url : '';
        app.globalData.adTitle = shareInfo.obj.ADTitle
        ctx.drawImage(_v.bg.tempFilePath, 0, 0, width, height); //背景图
        _v.status == 1 ? ctx.drawImage(_v.icon.tempFilePath, _v.xicon, _v.yicon, _v.w_icon, _v.hicon) : '';
        ctx.drawImage(_v.img.tempFilePath, _v.xstore, _v.ystore, _v.wstore, _v.hstore); //店铺图片
        ctx.drawImage(_v.qrcode.tempFilePath, _v.xqrcode, _v.yqrcode, _v.wqrcode, _v.hqrcode); //二维码
        if (_v.status != 4) {
          ctx.setFillStyle('rgba(0, 0, 0, 0.2)')
          ctx.fillRect(_v.xng, _v.yng, _v.wng, _v.hng)
          ctx.setFontSize(16) //店铺名
          ctx.setFillStyle('#ffffff')
          ctx.fillText(_v.name, _v.xname, _v.yname)
          ctx.setFontSize(13) //广告语
          ctx.fillText(_v.content, _v.xcon, _v.ycon)
          ctx.setFontSize(10)
          if (_v.status != 3) {
            ctx.setFillStyle('#DACACA')
            ctx.fillText('长按进入店铺', _v.xtxt, _v.ytxt)
          } else {
            ctx.setFillStyle('#ffffff')
          }
          ctx.fillText('长按二维码', _v.xtxt1, _v.ytxt1)
        }
        ctx.draw()
      } else {
        tools.showModal(shareInfo.msg, false)
      }
    })

  },
  getQrcode: function (g, target) {
    let app = wepy.$instance
    http.get(addr.GetProductQrcode, {
      version: 2,
      pid: g.pid,
      showQrcode: 1,
      typeName: g.type,
      recordId: g.recordId,
      showprice: g.showprice,
      appId: app.globalData.appid,
      productType: g.protype || 0,
      flashItemId: g.flashId || '',
    }).then(async info => {
      if (info.isok) {
        let vm = await canvasTools.pathCanvas(info.dataObj.qrCode, target)
        let w = wx.getSystemInfoSync().windowWidth * 0.87 // 屏幕宽度
        let h = wx.getSystemInfoSync().windowHeight * 0.75 // 屏幕高度
        let ctx = wx.createCanvasContext('firstCanvas')
        ctx.setFillStyle('white')
        ctx.fillRect(0, 0, w, h)
        ctx.drawImage(vm.img.tempFilePath, 0, 0, w, w)
        ctx.drawImage(vm.qrcode.tempFilePath, w * 0.64, h * 0.74, w * 0.23, w * 0.23)
        ctx.setFontSize(12)
        ctx.setFillStyle("#FF6700")
        ctx.fillText("长按查看商品", w * 0.65, h * 0.96)

        ctx.setFontSize(14)
        ctx.setFillStyle("#333333")
        ctx.fillText(vm.title.substr(0, 9), w * 0.1, h * 0.77)
        ctx.fillText(vm.title.substr(9, 9), w * 0.1, h * 0.81)
        ctx.fillText(vm.title.substr(18, 9), w * 0.1, h * 0.85)
        if (vm.discount != 100) {
          ctx.setFontSize(14)
          ctx.setFillStyle("#9C9C9C")
          ctx.fillText("原价", w * 0.1, h * 0.91)
          ctx.fillText("￥" + vm.price, w * 0.2, h * 0.91)
        }
        ctx.fillText("现价", w * 0.1, h * 0.96)
        ctx.setFontSize(20)
        ctx.setFillStyle("#FF6700")
        ctx.fillText(vm.disprice, w * 0.25, h * 0.96)
        ctx.setFontSize(14)
        ctx.setFillStyle("#FF6700")
        ctx.fillText("￥", w * 0.2, h * 0.96)
        ctx.draw()
        target.showCanvas = true;
        target.$apply();
      } else {
        tools.showModal(info.Msg, false)
      }
      wx.hideLoading();
    })
  },
  barCanvas: async function (qrcode, target) {
    let file1 = await canvasTools.downFile(qrcode.replace(/^http:/, "https:"));
    let file2 = await canvasTools.downFile('https://j.vzan.cc/miniapp/img/enterprise/bn-share.png');
    let w = wx.getSystemInfoSync().windowWidth;
    let h = wx.getSystemInfoSync().windowHeight;
    let ctx = wx.createCanvasContext('bargainCanvas');
    let _txt = "￥"
    let _txt2 = target.vm.obj.FloorPriceStr
    ctx.drawImage(file2.tempFilePath, 0, 0, w * 0.8, h * 0.74); //大背景图
    ctx.drawImage(file1.tempFilePath, w * 0.25, h * 0.25, w * 0.3, w * 0.3); //二维码
    ctx.setFontSize(25)
    ctx.setFillStyle('white')
    ctx.fillText(_txt, w * 0.23, h * 0.17) //第一行文字
    ctx.setFontSize(35)
    ctx.setFillStyle('white')
    ctx.fillText(_txt2, w * 0.29, h * 0.17) //第一行文字
    ctx.draw()
    wx.hideLoading()
    target.vm.barShow = false
    target.barCanvas = true
    target.$apply()
  },
  getSellCanvas: async function (saleManId, target) {
    let app = wepy.$instance
    let bindRecord = await core.bindSale(saleManId);
    tools.showLoading()
    http.get(addr.GetProductQrcode, {
      saleManId,
      storeSale: 1,
      recordId: bindRecord.obj,
      appId: app.globalData.appid,
    }).then(async data => {
      if (data.isok) {
        let qrcode = await canvasTools.downFile(data.dataObj.qrCode.replace(/^http:/, "https:"));
        let w = wx.getSystemInfoSync().windowWidth
        let h = wx.getSystemInfoSync().windowHeight
        let ctx = wx.createCanvasContext('sellCanvas')
        ctx.setFillStyle('white')
        ctx.fillRect(0, 0, w, h)
        ctx.setFontSize(16)
        ctx.setFillStyle("#333333")
        ctx.setTextAlign('center')
        ctx.fillText(target.vm.nickName, w * 0.38, h * 0.07)
        ctx.drawImage(qrcode.tempFilePath, w * 0.13, h * 0.12, w * 0.5, w * 0.5)
        ctx.draw()
        target.record = bindRecord.obj
        target.showMask = true
        target.$apply()
      } else {
        tools.showModal(data.Msg, false)
      }
      wx.hideLoading()
    })
  },

  getSellQrcode: function (saleManId, target) {
    let app = wepy.$instance
    tools.showLoading()
    http.get(addr.GetProductQrcode, {
      saleManId,
      applySale: 1,
      appId: app.globalData.appid,
    }).then(async data => {
      if (data.isok) {
        let qrcode = await canvasTools.downFile(data.dataObj.qrCode.replace(/^http:/, "https:"));
        let w = wx.getSystemInfoSync().windowWidth
        let h = wx.getSystemInfoSync().windowHeight
        let ctx = wx.createCanvasContext('sellCanvas')
        ctx.setFillStyle('white')
        ctx.fillRect(0, 0, w, h)
        ctx.setFontSize(16)
        ctx.setFillStyle("#333333")
        ctx.setTextAlign('center')
        ctx.fillText(target.vm.nickName, w * 0.38, h * 0.07)
        ctx.drawImage(qrcode.tempFilePath, w * 0.13, h * 0.12, w * 0.5, w * 0.5)
        ctx.draw()
        target.showMask = true
        target.$apply()
      } else {
        tools.showModal(data.Msg, false)
      }
      wx.hideLoading()
    })
  },


};

module.exports = {
  http,
  core,
  pay,
  canvas,
  tools,
  pro,
}
