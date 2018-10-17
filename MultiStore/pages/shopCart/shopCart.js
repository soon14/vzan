// pages/shopCart/shopCart.js
const app = getApp();
const tools = require('../../utils/tools.js')
const animation = require("../../utils/animation.js");
const pageRequest = require("../../utils/public-request.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    goundcartindex: -1,
    selectedAllStatus: false,
    total: 0,
    shopNum: 0,
    showWho: false,
    onloadShow: false,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    that.data.options = options
    let showChoose = app.globalData.location.showLocation
    that.setData({ showChoose })

  },
  //onload数据封装&&授权问题
  canUser: function (options) {
    let that = this
    let storeId = 0
    if (Object.keys(options).length == 0) {
      pageRequest.getGoodsCarData(app.globalData.currentPage.Id, that)
      that.setData({
        selfStatus: app.globalData.selfShow,
        cityStatus: app.globalData.cityShow,
        expressStatus: app.globalData.expressShow
      })
      that.exPressFun()
    }
    else {
      // 选择门店自取
      if (options.id) {
        app.globalData.pageShow = true
        app.globalData.pageLocation = false
        pageRequest.getStoreById(Number(options.id), that)
        pageRequest.getGoodsCarData(Number(options.id), that)
      }
      //选择配送
      else {
        app.globalData.pageShow = false
        app.globalData.pageLocation = true
        app.globalData.lat = options.lat
        app.globalData.lng = options.lng
        app.globalData.address = options.address
        Promise.all([pageRequest.getStoresAfter(options.lat, options.lng, that)]).then(values => {
          if (values[0].code == 2) {
            storeId = values[0].BoosStoreId
          } else {
            storeId = values[0].obj.Id
          }
          pageRequest.getGoodsCarData(storeId, that)
        })
      }
    }
    that.setData({ currentPage: app.globalData.currentPage, _userShow: true })

  },
  // 配送费
  exPressFun: function () {
    this.setData({
      cityPrice: app.globalData.cityPrice,
      expressPrice: app.globalData.expressPrice,
      takeStartPrice: app.globalData.takeStartPrice
    })
  },
  // 地址选择
  changGoto: function (e) {
    tools.goNewPage("../addressSelect/addressSelect?condition=" + e.currentTarget.dataset.id)
  },
  // 弹出编辑
  showEditorFunc: function (e) {
    let [id, goodlist, ds] = [e.currentTarget.id, this.data.goodlist, e.currentTarget.dataset]
    let i = ds.parentindex
    if (this.data.goundcartindex > 0) {
      tools.showToast("还在编辑状态")
    }
    else {
      animation.utilRight(this)
      goodlist[i].showModalStatus2 = true
      this.setData({ goodlist: goodlist })
      this.data.good_key = i
      this.data.goundcartindex = 2;
    }
  },
  // 收回编辑
  updateEditorFunc: function (e) {
    console.log(e)
    let [goodlist, index, updateCar] = [this.data.goodlist, e.currentTarget.id, []]
    goodlist[index].showModalStatus2 = false
    if (goodlist[index].Count == 0) {
      goodlist[index].Count = 1
    }
    let goodsCarModel = {
      Id: goodlist[index].Id,
      SpecIds: goodlist[index].SpecIds,
      SpecInfo: goodlist[index].SpecInfo,
      Count: goodlist[index].Count, //数量
    }
    animation.utilRight(this)
    updateCar.push(goodsCarModel)
    pageRequest.updateOrDelete(0, updateCar, this)
    this.setData({ goodlist: goodlist, total: 0, shopNum: 0 })
    this.data.goundcartindex = -1;
    tools.showToast("更新成功")
  },
  // 从购物车删除商品
  deleteFunc: function (e) {
    let that = this
    let [goodlist, ds] = [that.data.goodlist, e.currentTarget.dataset]
    let i = ds.parentindex
    wx.showModal({
      title: '提示',
      content: '亲，确认要删除该商品吗',
      success: res => {
        if (res.confirm) {
          let goodsCarModel = [{
            Id: goodlist[i].Id,
            SpecIds: goodlist[i].SpecIds,
            SpecInfo: goodlist[i].SpecInfo,
            Count: goodlist[i].Count
          }]
          pageRequest.updateOrDelete(-1, goodsCarModel, that)
          that.data.goundcartindex = -1
        }
      }
    })
  },
  // ++
  addViewFunc: function (e) {
    let that = this
    let [goodlist, ds] = [that.data.goodlist, e.currentTarget.dataset]
    let i = ds.parentindex
    if (goodlist[i].goodsMsg.stockLimit) {
      if (goodlist[i].specificationdetail.length) {
        let templateModel = goodlist[i].specificationdetail.find(f => f.id == goodlist[i].SpecIds)
        if (templateModel) {
          if (goodlist[i].Count < templateModel.stock) {
            goodlist[i].Count++
          }
          else {
            tools.showToast("库存不足")
          }
        }
      }
      else {
        if (goodlist[i].Count < goodlist[i].goodsMsg.stock) {
          goodlist[i].Count++
        }
        else {
          tools.showToast("库存不足")
        }
      }
    }
    else {
      goodlist[i].Count++
    }
    that.setData({ goodlist: goodlist })
  },
  //--
  lessViewFunc: function (e) {
    let that = this
    let [goodlist, ds] = [that.data.goodlist, e.currentTarget.dataset]
    var i = ds.parentindex
    if (goodlist[i].Count > 1) {
      goodlist[i].Count--
    }
    else {
      tools.showToast("不能再减少了哦")
    }
    this.setData({ goodlist: goodlist })
  },
  // input框输入数量
  setValueFunc: function (e) {
    console.log(e)
    let that = this
    let [goodlist, ds, value] = [that.data.goodlist, e.currentTarget.dataset, e.detail.value]
    let i = ds.parentindex
    if (value) {
      if (goodlist[i].goodsMsg.stockLimit) {
        if (goodlist[i].goodsMsg.specificationdetail) {
          let templateModel = goodlist[i].specificationdetail.find(f => f.id == goodlist[i].SpecIds)
          if (templateModel) {
            value <= templateModel.stock ? goodlist[i].Count = value : tools.showToast("超过库存")
          }
        }
        else {
          value <= goodlist[i].goodsMsg.stock ? goodlist[i].Count = value : tools.showToast("超过库存")
        }
      }
      else {
        goodlist[i].Count = value
      }
    }
    that.setData({ goodlist: goodlist })
  },
  // 单个选择
  itemSelectFunc: function (e) {
    console.log(e)
    let [parentindex, goodlist] = [e.currentTarget.dataset.parentindex, this.data.goodlist]
    let templateModel = goodlist[parentindex].goodsMsg
    let key = "goodlist[" + parentindex + "].goodsMsg.sel"
    if (this.data.goundcartindex > 0) {
      tools.showToast("您还在编辑状态")
      return
    }
    this.setData({
      [key]: !templateModel.sel,
    })
    this.totalFunct()
  },
  // 全选
  allSelectFunc: function (e) {
    var selectedAllStatus = this.data.selectedAllStatus;
    let goodlist = this.data.goodlist;
    if (this.data.goundcartindex > 0) {
      tools.showToast("您还在编辑状态")
      return
    }
    selectedAllStatus = !selectedAllStatus;
    for (var i in goodlist) { goodlist[i].goodsMsg.sel = selectedAllStatus }
    this.setData({ selectedAllStatus: selectedAllStatus, goodlist: goodlist })
    this.totalFunct()
  },
  // 计算总价
  totalFunct: function () {
    let [goodlist, total, shopNum, totalOldprice] = [this.data.goodlist, 0, 0, 0]
    for (var i = 0; i < goodlist.length; i++) {
      if (goodlist[i].goodsMsg.sel) {
        total += goodlist[i].priceStr * goodlist[i].Count
        totalOldprice += goodlist[i].originalPriceStr * goodlist[i].Count
        shopNum++
      }
    }
    total = parseFloat(total).toFixed(2)
    totalOldprice = parseFloat(totalOldprice).toFixed(2)
    this.setData({
      goodlist: goodlist,
      total: total,
      shopNum: shopNum
    })
    this.data.totalOldprice = totalOldprice
  },
  // 选择商品属性
  showShopcarFunc: function (e) {
    let ds = e.currentTarget.dataset
    let [i, currentStatu] = [ds.parentindex, ds.statu]
    let msg = this.data.goodlist[i]
    this.setData({
      msg: msg,
      newPrice: msg.priceStr,
      oldPrice: msg.originalPriceStr,
      specInfo: msg.SpecInfo,
      totalCount: msg.Count,
    })
    animation.utilUp(currentStatu, this)
  },
  //隐藏
  hideFunc: function (e) {
    let currentStatu = e.currentTarget.dataset.statu
    animation.utilUp(currentStatu, this);
    this.setData({
      newPrice: this.data.msg.discountPricestr,
      oldPrice: this.data.msg.price,
    })
  },
  // 选择商品属性点击事件
  chooseFunc: function (e) {
    console.log(e)
    let ds = e.currentTarget.dataset
    let [attrSpacStr, specInfo, parentindex, childindex] = [[], [], ds.p, ds.c]
    let [spec, pick, newPrice, oldPrice] = [this.data.msg.specificationdetail, this.data.msg.pickspecification, this.data.msg.priceStr, this.data.msg.originalPriceStr]
    let [currentList, self] = [pick[parentindex], pick[parentindex].items[childindex]]
    let key = "msg.pickspecification[" + parentindex + "]"
    if (currentList.items.length > 0) {
      currentList.items.forEach(function (obj, i) {
        obj.id != self.id ? obj.sel = false : obj.sel = !obj.sel
      });
    }
    for (let i = 0, val; val = pick[i++];) {
      for (let j = 0, valKey; valKey = val.items[j++];) {
        if (valKey.sel) {
          let parentName = val.name
          let childName = valKey.name
          let specName = parentName + ":" + childName
          specInfo.push(specName)
          attrSpacStr.push(valKey.id)
        }
      }
    }
    //拼接id及名字
    attrSpacStr = attrSpacStr.join("_")
    specInfo = specInfo.join(" ")
    let templateFind = spec.find(f => f.id == attrSpacStr)
    if (templateFind) {
      newPrice = templateFind.discountPricestr
      oldPrice = templateFind.price
      this.data.computePrice = newPrice
      this.data.stock = templateFind.stock
    }
    this.setData({
      [key]: currentList,
      newPrice: newPrice,
      oldPrice: oldPrice,
      totalCount: 1,//切换选择规格时重置选择数量
      specInfo: specInfo,
    })
    this.data.attrSpacStr = attrSpacStr
  },
  // +
  addFunc: function (e) {
    let [msg, SpecIds, totalCount] = [this.data.msg, this.data.attrSpacStr, this.data.totalCount]
    let template = msg.specificationdetail.find(a => a.id == SpecIds)
    if (template) {
      var nownumber = template.stock
      if (totalCount < nownumber) {
        totalCount++
      }
      else {
        tools.showToast("不能再添加哦")
      }
    }
    else {
      totalCount++
    }
    this.setData({ totalCount: totalCount })
  },
  // -
  lessFunc: function (e) {
    let [msg, totalCount] = [this.data.msg, this.data.totalCount]
    if (totalCount > 1) {
      totalCount--
    } else {
      tools.showToast("不能再减少了哦")
    }
    this.setData({ totalCount: totalCount })
  },
  // 确定
  shopCarFunc: function (e) {
    let [goodlist, specificationdetail, msg] = [this.data.goodlist, this.data.msg.specificationdetail, this.data.msg]
    let templateModel = specificationdetail.find(f => f.id == this.data.attrSpacStr)
    if (templateModel == undefined) {
      tools.showToast("请选择")
      return;
    }
    else {
      if (templateModel.stock == 0) {
        tools.showToast("库存不足")
        return;
      } else {
        goodlist[this.data.good_key].SpecIds = this.data.attrSpacStr
        goodlist[this.data.good_key].SpecInfo = this.data.specInfo
        goodlist[this.data.good_key].Count = this.data.totalCount
        goodlist[this.data.good_key].Price = msg.goodsMsg.discountPricestr
      }
    }
    this.setData({ goodlist: goodlist })
    animation.utilUp("close", this)
  },
  // 支付
  goPlayFunc: function (e) {
    console.log(e)
    let that = this
    let [goodCarIdStr, discount, totalCount, goodlist] = [[], 0, 0, that.data.goodlist]
    if (that.data.goundcartindex > 0) {
      tools.showToast("您还在编辑状态")
      return
    }
    else {
      for (let i in goodlist) {
        if (goodlist[i].goodsMsg.sel == true) {
          goodCarIdStr.push(goodlist[i].Id)
          discount = goodlist[i].discount
          totalCount += goodlist[i].Count
        }
      }
      if (that.data.shopNum == 0) {
        tools.showToast("请选择商品")
        return;
      } else {
        goodCarIdStr = goodCarIdStr.join(",")
        tools.goNewPage("../order/order?goodCarIdStr=" + goodCarIdStr + "&totalCount=" + totalCount + "&totalPrice=" + that.data.total + "&discount=" + discount + "&totalOldprice=" + that.data.totalOldprice)
      }
    }
    pageRequest.commitFormId(e.detail.formId)
  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
    let that = this
    wx.getSetting({
      success(res) {
        if (res.authSetting['scope.userLocation']) {
          app.getLocationInfo().then(res => {
            let [lat, lng, address] = [res.latitude, res.longitude, ""]
            // 获取当前定位
            Object.keys(app.globalData.address).length == 0 ? pageRequest.getMapAddress(lat, lng, that) : address = app.globalData.address
          })
          that.setData({ showChoose: true })
        }
      }
    })
    that.canUser(that.data.options);
    that.setData({ selectedAllStatus: false, total: 0, shopNum: 0 })
  },


  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast("正在刷新")
    that.canUser(that.data.options);
    setTimeout(res => { tools.showToast("刷新成功"); wx.stopPullDownRefresh() }, 1000)
  },
})