const app = getApp()
const tools = require("../../utils/tools.js");
const animation = require("../../utils/animation.js");
const pageRequest = require("../../utils/public-request.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    Mode: [
      { mode: "配送上门", id: 0 },
      { mode: "门店自取", id: 1 },
    ],
    showToast: false,
    condition: 0,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    this.onloadFunc(options)
  },
  //封装onload
  onloadFunc: function (options) {
    let that = this
    let [lat, lng, showLocation] = [app.globalData.location.latitude, app.globalData.location.longitude, false]
    lat && lng ? pageRequest.getMapAddress(lat, lng, that) : "";
    if (options && Object.keys(options).length) {
      if (options.condition == "1") {
        that.setData({ condition: options.condition })
        that.data.cartIdStr = options.cartIdStr
        that.data.pCondition = options.condition
      }
      that.chooseRequest(lat, lng, options.condition)
    }
    if (options && options.detailMenu != "undefined" && options.detailMenu != undefined) {
      that.data.detailMenu = JSON.parse(options.detailMenu)
    }
    that.data.options = options
  },
  // 收货地址&&门店列表请求
  chooseRequest: function (latitude, longitude, condition) {
    let that = this
    if (condition == 0) {
      pageRequest.getNearAddress(latitude, longitude, that)
    }
    else {
      pageRequest.getStoresList(latitude, longitude, 1, that)
    }

  },
  // 切换选择
  changModeFunc: function (e) {
    let that = this
    let [condition, latitude, longitude] = [that.data.condition, 0, 0]
    e == undefined ? condition = 0 : condition = e.currentTarget.dataset.id
    wx.getSetting({
      success: (data) => {
        //授权时
        if (data.authSetting["scope.userLocation"] && data.authSetting["scope.userInfo"]) {
          if (that.data.latitude && that.data.longitude) {
            latitude = that.data.latitude;
            longitude = that.data.longitude
          }
          else {
            latitude = app.globalData.location.latitude;
            longitude = app.globalData.location.longitude
          }
          that.chooseRequest(latitude, longitude, condition)
        }
      }
    })
    that.setData({ condition: condition })

  },
  //选择位置位置
  chooseLocFunc: function (e) {
    let that = this
    let showopen=false
    let condition = that.data.condition
    //授权时
    wx.chooseLocation({
      success: res => {
        let address = res.address + res.name
        that.setData({ address: address, condition: condition })
        that.data.latitude = res.latitude
        that.data.longitude = res.longitude
        that.chooseRequest(res.latitude, res.longitude, condition)
      },
      fail: res => {
        if (res.errMsg == "chooseLocation:fail auth deny") {
          wx.showModal({
            title: '提示',
            content: '亲，该功能需要授权',
            success: res => {
              if (res.confirm) {
                // wx.openSetting()
              }
            }
          })
        }
      }
    })

  },

  // 选择该门店
  storeFunc: function (e) {
    let that = this
    let ds = e.currentTarget.dataset
    let [storeId, selecthomeid] = [ds.id, ds.homeid]
    that.data.storeName = ds.shopname
    if (getCurrentPages().length == 2) {
      that.storeGoto(storeId, selecthomeid)
    }
    else {
      that._goodState(storeId, selecthomeid)
    }
  },
  // 详情&&购物车
  _goodState: function (storeId, selecthomeid) {
    let that = this
    let para = {
      specId: "",
      goodId: 0,
      storeId: storeId,
      cartIds: 0,
    }
    wx.showLoading({
      title: '加载中..',
      mask: true,
      success: function () {
        pageRequest.getStoreByIdNum(storeId, that).then(res => {
          // 详情页
          if (getCurrentPages().length == 4 && getCurrentPages()[1].route == "pages/details/details") {
            para.goodId = getCurrentPages()[2].data.menuList[0].id
            para.specId = getCurrentPages()[2].data.menuList[0].SpecInfo
            pageRequest.getGoodStock(para).then(data => {
              if (data.isok) {
                let menuList = []
                menuList.push(data.goodModel)
                menuList[0].GoodName = data.goodModel.name
                menuList[0].goodsMsg = { img: data.goodModel.img }
                menuList[0].SpecInfo = getCurrentPages()[2].data.menuList[0].SpecInfo
                menuList[0].Count = getCurrentPages()[2].data._orderSync.totalCount
                app.globalData.goodState = data.goodState
                menuList[0].GoodsState = data.goodState
                if (menuList[0].GoodsState == 3) {
                  animation.utilUp("open", that)
                } else {
                  that.storeGoto(storeId, selecthomeid)
                }
                that.setData({ list: menuList })
              }
              wx.hideLoading()
            })
          }
          else {
            para.cartIds = getCurrentPages()[1].data.goodsId
            pageRequest.getGoodsCarDataByIds(para).then(data => {
              if (data.isok) {
                let [listModel, list] = [[], data.data]
                let template = list.find(f => f.GoodsState == 3)
                for (let i = 0, key; key = list[i++];) {
                  if (key.GoodsState == 3) {
                    listModel.push(key)
                  }
                }
                if (template != undefined) {
                  animation.utilUp("open", that)
                }
                else {
                  that.storeGoto(storeId, selecthomeid)
                }
                that.setData({ list: listModel })
                wx.hideLoading()
              }
            })

          }
        })
      }
    })
  },






  //选择该地址 
  addrFunc: function (e) {
    let that = this
    let ds = e.currentTarget.dataset
    let [address, lat, lng, id] = [ds.address, ds.lat, ds.lng, ds.id]
    pageRequest.getAddStore(lat, lng, that).then(res => {
      if (res.code == 1) { that.addBacGoto(address, lat, lng, id) }
      else {
        pageRequest.getAddExpre().then(data => {
          if (data.isOpen) {
            that.addBacGoto(address, lat, lng, id)
          } else {
            that.setData({ showToast: true, toast: "附近门店暂不支持配送" })
            setTimeout(data => { that.setData({ showToast: false }) }, 800)
          }
        })
      }
    })
  },
  // 门店自取选择提示
  storeGoto: function (storeId, homeid) {
    let that = this
    wx.showModal({
      title: '提示',
      content: '确认选择该门店吗(门店自取)？',
      success: res => {
        if (res.confirm) {
          if (getCurrentPages().length == 2 && getCurrentPages()[0].route == 'pages/index/index') {
            tools.goLaunch("../index/index?id=" + storeId)
          }
          else if (getCurrentPages().length == 2 && getCurrentPages()[0].route == 'pages/classify/classify') {
            tools.goLaunch("../classify/classify?id=" + storeId + "&homeid=" + homeid)
          }
          else if (getCurrentPages().length == 4 && getCurrentPages()[2].route == 'pages/order/order' || getCurrentPages().length == 3 && getCurrentPages()[1].route == 'pages/order/order') {
            tools.goBack()
          }
          else {
            tools.goLaunch("../shopCart/shopCart?id=" + storeId + "&homeid=" + homeid)
          }
        }
        else { return; }
      }
    })
  },
  //地址选择返回相对应页面
  addBacGoto: function (address, lat, lng, id) {
    if (getCurrentPages().length == 4 && getCurrentPages()[2].route == 'pages/order/order' || getCurrentPages().length == 3 && getCurrentPages()[1].route == 'pages/order/order') {
      return;
    }
    wx.showModal({
      title: '提示',
      content: '确认选择该地址吗(配送上门)？',
      success: res => {
        if (res.confirm) {
          if (getCurrentPages()[0].route == 'pages/index/index') {
            tools.goLaunch("../index/index?address=" + address + "&lat=" + lat + "&lng=" + lng)
          }
          else if (getCurrentPages()[0].route == "pages/classify/classify") {
            tools.goLaunch("../classify/classify?address=" + address + "&lat=" + lat + "&lng=" + lng)
          }
          else {
            tools.goLaunch("../shopCart/shopCart?address=" + address + "&lat=" + lat + "&lng=" + lng)
          }
          pageRequest.getMyAddressDefault(this, id)
        }
        else {
          return;
        }
      }
    })
  },
  // 继续选择
  confirmFunc: function () { let that = this; animation.utilUp("close", that); tools.goBack() },
  // 重新选择
  hideFunc: function () { animation.utilUp("close", this) },
  //地图引导
  openLocaFunc: function (e) {
    let ds = e.currentTarget.dataset
    let [lat, lng, name, address] = [ds.lat, ds.lng, ds.add, ds.address]
    tools.openMap(lat, lng, name, address)
  },
  //添加地址
  addressGoto: function () {
    let [lat, lng] = [app.globalData.currentPage.Lat, app.globalData.currentPage.Lng]
    if (this.data.latitude && this.data.longitude) {
      lat = this.data.latitude
      lng = this.data.longitude
    }
    else {
      lat = app.globalData.currentPage.Lat
      lng = app.globalData.currentPage.Lng
    }
    tools.goNewPage("../addressEdit/addressEdit?address=" + this.data.address + "&lat=" + lat + "&lng=" + lng)
    this.data.latitude = lat
    this.data.longitude = lng
    this.data.addShow = true
  },
  // 拨打电话
  phoneFunc: function (e) {
    if (e.currentTarget.dataset.phone == '') { tools.showToast("暂无联系电话"); return }
    tools.phone(e.currentTarget.dataset.phone)
  },
  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {

  },
})