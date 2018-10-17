// pages/details/details.js
const app = getApp();
const pageRequest = require("../../utils/public-request.js");
const tools = require('../../utils/tools.js')
const animation = require("../../utils/animation.js");
const http = require("../../utils/http.js")
const addr = require("../../utils/addr.js")
Page({

  /**
   * 页面的初始数据
   */
  data: {
    indicatorDots: false,
    autoplay: true,
    interval: 5000,
    duration: 1000,
    totalCount: 1,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    let userInfo = wx.getStorageSync("userInfo")
    that.setData({ userInfo })
    app.globalData.currentPage.HomeId == 0 ? pageRequest.getGoodInfo(that, options.pid, "") : pageRequest.getGoodInfo(that, options.pid, app.globalData.currentPage.Id)
    that.data.pid = options.pid

  },
  // 弹出购物车
  addShopFunc: function (e) {
    let [currentStatu, msg, pick, showWho] = [e.currentTarget.dataset.statu, this.data.msg, this.data.msg.pickspecification, e.currentTarget.dataset.show]
    if (pick.length) {
      for (let i = 0, val; val = pick[i++];) {
        for (let j = 0, key; key = val.items[j++];) {
          key.sel ? key.sel = false : ""
        }
      }
    }
    msg.pickspecification = pick
    showWho == "false" ? showWho = false : showWho = true
    this.setData({
      msg: msg,
      specInfo: '',
      totalCount: 1,
      showWho: showWho,
      oldPrice: msg.price,
      newPrice: msg.discountPricestr,
      selfStatus: app.globalData.selfShow,
      cityStatus: app.globalData.cityShow,
      expressStatus: app.globalData.expressShow,
      takeStartPrice: app.globalData.takeStartPrice,
    })
    this.data.specId = ""
    this.data.stock = msg.stock
    animation.utilUp(currentStatu, this);
    pageRequest.commitFormId(e.detail.formId)
  },
  getlogin: function (e) {
    let that = this
    let _g = e.detail
    let appid = app.globalData.appid
    console.log(_g)
    if (_g.errMsg =='getUserInfo:fail auth deny'){
      return;
    }
    wx.login({
      success: function (res) {
        http.postAsync(addr.Address.loginByThirdPlatform, {
          appid,
          iv: _g.iv,
          code: res.code,
          data: _g.encryptedData,
          signature: _g.signature,
          isphonedata: 0,
        }).then(data => {
          if (data.result) {
            let userInfo = data.obj
            wx.clearStorageSync("userInfo")
            that.setData({ userInfo })
            wx.setStorageSync("userInfo", userInfo)
            app.globalDat.userInfo = userInfo
          }
        })
      },
      fail: function (res) {
      }
    })
  },
  //隐藏
  hideFunc: function (e) {
    let currentStatu = e.currentTarget.dataset.statu
    animation.utilUp(currentStatu, this);
    this.setData({ newPrice: this.data.msg.discountPricestr, oldPrice: this.data.msg.price })
  },
  //选中规格
  chooseFunc: function (e) {
    let that = this
    let [ds, msg] = [e.currentTarget.dataset, that.data.msg]
    let [pick, spec, newPrice, oldPrice] = [msg.pickspecification, msg.specificationdetail, msg.discountPricestr, msg.price]
    let [pIndex, cIndex, specInfo, specId] = [ds.p, ds.c, [], []]
    let [currentPick, self] = [pick[pIndex], pick[pIndex].items[cIndex]]
    let key = "msg.pickspecification[" + pIndex + "]"
    if (currentPick.items.length > 0) {
      currentPick.items.forEach(function (obj, i) {
        obj.id != self.id ? obj.sel = false : obj.sel = !obj.sel;
      })
    }
    for (let i = 0, val; val = pick[i++];) {
      for (let j = 0, keyVal; keyVal = val.items[j++];) {
        if (keyVal.sel == true) {
          let specName = val.name + ":" + keyVal.name
          specInfo.push(specName)
          specId.push(keyVal.id)
        }
      }
    }
    specId = specId.join("_")
    let templeSpec = spec.find(f => f.id == specId)
    if (templeSpec) {
      oldPrice = templeSpec.price
      newPrice = templeSpec.discountPricestr
      that.data.computePrice = newPrice
      that.data.stock = templeSpec.stock
    } else {
      that.data.stock = msg.stock
    }
    that.data.specId = specId
    that.setData({
      [key]: currentPick,
      specInfo: specInfo,
      newPrice: newPrice,
      oldPrice: oldPrice,
      totalCount: 1
    })
    console.log(e)
  },
  //加号Func
  addFunc: function () {
    let [count, stock, addNewPrice, pick, spec] = [this.data.totalCount, this.data.stock, 0, this.data.msg.pickspecification, this.data.msg.specificationdetail]
    if (this.data.msg.stockLimit) {
      if (count < stock) {
        count++
      } else {
        count = this.data.stock || 1;
        tools.showToast("亲,库存不足")
      }
    }
    else {
      count++
    }
    if (pick.length) {
      // 有规格时
      let templatePrice = spec.find(k => k.id == this.data.specId)
      if (templatePrice) { addNewPrice = parseFloat(this.data.computePrice * count || this.data.msg.computePrice).toFixed(2) }
      else { return; }
    }
    // 无规格时
    else {
      addNewPrice = parseFloat(this.data.msg.discountPricestr * count || this.data.msg.discountPricestr).toFixed(2)
    }
    this.setData({ totalCount: count, newPrice: addNewPrice })
  },
  //减号
  lessFunc: function () {
    let [lessPrice, count, stock, pick] = [0, this.data.totalCount, this.data.stock, this.data.msg.pickspecification]
    if (this.data.msg.stockLimit) {
      if (count > 1) {
        count--
      }
      else {
        tools.showToast("亲,不要再减啦");
        count = 1
      }
    }
    else {
      if (count > 1) {
        count--
      }
      else {
        count = 1
      }
    }
    if (pick != 0) { lessPrice = parseFloat(this.data.computePrice * count).toFixed(2) }
    else { lessPrice = parseFloat(this.data.msg.discountPrice * count).toFixed(2) }
    this.setData({ totalCount: count, newPrice: lessPrice })
  },
  // 购物车&&购买按钮
  shopCarFunc: function (e) {
    let that = this
    let formid = e.detail.formId
    that.keepFunc(formid)
  },
  // 防空验证
  keepFunc: function (formid) {
    let that = this
    let msg = that.data.msg
    let [attrSpacStr, pick, spec] = [that.data.specId, msg.pickspecification, msg.specificationdetail]
    let para = {
      goodId: msg.id,
      attrSpacStr: that.data.specId,
      SpecInfo: that.data.specInfo,
      qty: that.data.totalCount,
      newCartRecord: 0,
      fpage: that
    }
    if (msg.stockLimit) {
      if (pick.length) {
        let templateFind = spec.find(a => a.id == attrSpacStr)
        if (attrSpacStr == "") { tools.showToast("请选择商品规格"); return }
        else if (templateFind == undefined) { tools.showToast("未选择完"); return }
        else {
          templateFind.stock ? that.goShopModeFunc(formid, para) : tools.showToast("库存不足")
        }
      }
      else {
        msg.stock ? that.goShopModeFunc(formid, para) : tools.showToast("库存不足")
      }
    }
    else {
      if (pick.length) {
        let templateFind = spec.find(a => a.id == attrSpacStr)
        if (attrSpacStr == "") { tools.showToast("请选择商品规格"); return }
        else if (templateFind == undefined) { tools.showToast("未选择完"); return }
        else {
          that.goShopModeFunc(formid, para)
        }
      }
      else {
        that.goShopModeFunc(formid, para)
      }
    }
  },

  // 添加购物车或立即购买
  goShopModeFunc: function (formid, para) {
    let that = this
    if (that.data.showWho) {
      that.orderGoto(formid)
    }
    else {
      pageRequest.addGoodsCarData(para).then(data => {
        if (data.isok) {
          tools.showToast("添加成功")
        } else {
          tools.showToast(data.msg)
        }
      })
    }
    animation.utilUp("close", this);
  },

  // 查看购物车
  shopCarGoto: function () {
    tools.goLaunch('../shopCart/shopCart')
  },
  // 图片点击放大
  preViewShow: function (e) {
    console.log(e)
    let [current, urls, currentPage] = [e.currentTarget.dataset.img, [], this.data.msg]
    if (currentPage.slideimgs != "") {
      for (let i in currentPage.slideimgs) { urls.push(currentPage.slideimgs[i]) }
    }
    else {
      urls.push(current)
    }
    tools.preViewShow(current, urls)
  },

  //立即购买
  orderGoto: function (formid) {
    if (this.data.msg.pickspecification == 0) { this.data.computePrice = this.data.msg.discountPricestr }
    let totalOldprice = parseFloat(this.data.oldPrice * this.data.totalCount).toFixed(2)
    let menu = {
      id: this.data.msg.id,
      Count: this.data.totalCount,
      SpecInfo: this.data.specInfo,
      GoodName: this.data.msg.name,
      attrSpacStr: this.data.specId,
      priceStr: this.data.computePrice,
      goodsMsg: { img: this.data.msg.img },
      originalPriceStr: this.data.oldPrice,
    }
    let menuList = []
    menuList.push(menu)
    tools.goNewPage("../order/order?menuList=" + JSON.stringify(menuList) + "&totalCount=" + this.data.totalCount + "&totalPrice=" + this.data.newPrice + "&discount=" + this.data.msg.discount + "&totalOldprice=" + totalOldprice)
    pageRequest.commitFormId(formid)
  },
  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {
    let that = this
    let [msg, image] = [that.data.msg, msg.img]
    msg.slideimgs != "" ? image = msg.slideimgs[0] : image = msg.img
    return {
      title: msg.name,
      imageUrl: image,
      path: '/pages/details/details?pid=' + that.data.pid,
      success: res => { tools.showToast("转发成功") },
      fail: res => { tools.showToast("转发失败") }
    }
  }
})