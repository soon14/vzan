// pages/addMyAddress/addMyAddress.js
const app = getApp();
const tools = require("../../utils/tools.js");
const pageRequest = require("../../utils/public-request.js");
const animation = require("../../utils/animation.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {

  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
  },
  // 检测用户是否授权
  canUser: function (options) {
    let that = this
    pageRequest.getMyaddress(that, 0)

  },

  /**
  * 页面跳转
  * 1添加地址
  * 2管理地址
  */
  addressGoto: function () {
    tools.goNewPage("../addressEdit/addressEdit")
  },
  managerGoto: function () {
    tools.goNewPage("../addressChange/addressChange")
  },
  // 选择地址
  selectFunc: function (e) {
    let that = this
    let ds = e.currentTarget.dataset
    let [lat, lng] = [ds.lat, ds.lng]

    pageRequest.getAddStore(lat, lng).then(res => {

      if (res.obj) {
        that._goodState(res.obj.Id)
      }
      else {
        pageRequest.getAddExpre().then(data => {
          if (data.isOpen) {
            that._goodState(data.info.Id)
          }
          else {
            tools.showLoadToast("暂无配送门店")
          }
        })
      }
    })


    that.data.addressId = ds.id
  },
  // 详情&&购物车
  _goodState: function (storeId) {
    let that = this
    let para = {
      specId: "",
      goodId: 0,
      storeId: 0,
      cartIds: 0,
    }
    wx.showLoading({
      title: '加载中..',
      mask: true,
      success: function () {
        // 详情页
        if (getCurrentPages().length == 4 && getCurrentPages()[1].route == "pages/details/details") {
          para.storeId = storeId
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
                tools.showLoadToast("正在加载")
                animation.utilUp("open", that)
              } else {
                that.storeGoto()
              }
              that.setData({ list: menuList })
            }
            wx.hideLoading()
          })
        }
        else {
          // 购物车页
          para.storeId = storeId
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
                that.storeGoto()
              }
              that.setData({ list: listModel })
              wx.hideLoading()
            }
          })


        }
      }
    })
  },
  // 当选择继续购买时
  confirmFunc: function () {
    animation.utilUp("close", this)
    pageRequest.getMyAddressDefault(this, this.data.addressId)
    tools.goBack()
  },
  hideFunc: function () {
    animation.utilUp("close", this)
  },
  // 选择提示
  storeGoto: function () {
    let that = this
    tools._clickMoadl('确认选择该地址吗？').then(res => {
      if (res.confirm) {
        pageRequest.getMyAddressDefault(that, that.data.addressId);
        tools.goBack()
      }
    })
  },
  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
    this.canUser(this.data.options)
  },
  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast("正在刷新")
    pageRequest.getMyaddress(that, 0);
    setTimeout(res => { tools.showToast("刷新成功"); wx.stopPullDownRefresh() }, 1000)
  },


})