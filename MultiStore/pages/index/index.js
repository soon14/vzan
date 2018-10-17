//index.js
const app = getApp();
const pageRequest = require("../../utils/public-request.js");
const tools = require('../../utils/tools.js')


Page({
  data: {

  },
  onShow: function () {
    let that=this
    wx.getSetting({
      success(res) {
        if (res.authSetting['scope.userLocation']) {
          that.setData({ showChoose: true })
          that.onloadFunc(that.data.options)
        }
      }
    })

  },
  onLoad: function (options) {
    this.onloadFunc(options)
    pageRequest.agminapp(this)
  },
  makeMinapp(e) {
    let id=e.currentTarget.dataset.id
    wx.navigateTo({
      url: '/pages/index/minapp?id=' + id,
    })
  },
  //onload数据封装
  onloadFunc: function (options) {
    let that = this
    that.data.options = options
    // 获取aid
    pageRequest.getAid(that)
    // 获取用户定位
    app.getLocationInfo().then(res => {
      let [lat, lng, address] = [res.latitude, res.longitude, ""]

      if (Object.keys(options).length == 0) {
        if (app.globalData.pageShow == false && app.globalData.pageLocation == false) {
          pageRequest.getStoresAfter(lat, lng, that)
        }
        else if (app.globalData.pageShow == true && app.globalData.pageLocation == false) {
          pageRequest.getStoreById(Number(app.globalData.currentPage.Id), that)
        }
        else {
          pageRequest.getStoresAfter(app.globalData.lat, app.globalData.lng, that)
        }
        // 获取当前定位
        Object.keys(app.globalData.address).length == 0 ? pageRequest.getMapAddress(lat, lng, that) : address = app.globalData.address
      }
      else {
        // 选择门店自取
        if (options.id) {
          pageRequest.getStoreById(Number(options.id), that)
        }
        //配送
        else {
          address = options.address;
          app.globalData.address = options.address;
          app.globalData.lat = options.lat
          app.globalData.lng = options.lng
          pageRequest.getStoresAfter(options.lat, options.lng, that)
        }
      }
      let showChoose = app.globalData.location.showLocation
      that.setData({ address: address, showChoose })
    })
  },
  // 切换地址
  addressGoto: function (e) {
    tools.goNewPage("../addressSelect/addressSelect?condition=" + e.currentTarget.dataset.id)
  },
  //分类导航跳转
  classifyGoto: function (e) {
    app.globalData.condition = e.currentTarget.dataset.condition
    tools.goSwitch('../classify/classify')
  },
  // 产品详情
  detailsGoto: function (e) {
    tools.goNewPage("../details/details?pid=" + e.currentTarget.dataset.pid)
  },
  // 立减金
  _couponGo: function () {
    tools.goNewPage('../addCoupon/mysmoney')
  },
  // 图片点击放大
  preViewShow: function (e) {
    let ds = e.currentTarget.dataset
    let [img, imgls, urls] = [ds.img, ds.imglist, []]
    for (let i in imgls) {
      urls.push(imgls[i].img)
    }
    tools.preViewShow(img, urls)
  },
  //下拉刷新
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast("正在刷新")
    setTimeout(res => { tools.showToast("刷新成功"); that.onloadFunc(that.data.options); wx.stopPullDownRefresh() }, 1000)
  },
  // 分享
  onShareAppMessage: function () {
    return {
      title: app.globalData.StoreName,
      path: '/pages/index/index',
      success: res => { tools.showToast("转发成功") },
      fail: res => { tools.showToast("转发失败") }
    }
  },


})
