// pages/newShopCart/newShopCart.js
var page = require("../../utils/pageRequest.js")
var animation = require("../../utils/animation.js")
var http = require('../../utils/http.js');
var addr = require("../../utils/addr.js")
var utils = require("../../utils/util.js")
const tools = require("../../utils/tools.js")
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
  data: {
    goundcartindex: -1,
    sel: false,
    showModalStatus: false,
    pickspecification: [],//规格集合
    specificationdetail: [],//多规格后的价格
    selectedAllStatus: false,
    addshop: true,
    total: 0,
    shopNum: 0,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    page.newshopcartRequest(this)
    utils.setPageSkin(this);
  },
  //更新购物车请求
  updateRequest: function (updateCar, editorType) {
    var that = this
    http.postJsonAsync(
      addr.Address.updateOrDeleteGoodsCarData,
      {
        openid: app.globalData.userInfo.openId,
        appid: app.globalData.appid,
        function: editorType,
        goodsCarModel: updateCar
      }
    ).then(function (data) {
      if (data.isok == 1) {
        //page.newshopcartRequest(that)
        that.data.goundcartindex = -1
        that.setData({
          total: 0,
          shopNum: 0,
          selectedAllStatus: false
        })
      }
    })
  },
  // 弹出编辑
  showEditorFunc: function (e) {
    var id = e.currentTarget.id
    var goodsList = this.data.goodsList
    var ds = e.currentTarget.dataset
    var i = ds.parentindex
    if (this.data.goundcartindex > 0) {
      tools.showToast("还在编辑状态")
    } else {
      goodsList[i].showModalStatus2 = true
      animation.utilRight(this)
      this.data.goundcartindex = 2
      this.data.good_key = i
      this.setData({
        goodsList: goodsList,
      })
    }
  },
  // 收回编辑
  updateEditorFunc: function (e) {
    console.log(e)
    var goodsList = this.data.goodsList
    var index = e.currentTarget.id
    var updateCar = []
    goodsList[index].showModalStatus2 = false
    var goodsCarModel = ({
      Id: goodsList[index].Id,
      SpecIds: goodsList[index].SpecIds,
      SpecInfo: goodsList[index].SpecInfo,
      Count: goodsList[index].Count, //数量

    })
    updateCar.push(goodsCarModel)
    this.updateRequest(updateCar, 0)
    tools.showToast("更新成功")
    animation.utilRight(this)
    this.setData({
      goodsList: goodsList
    })
  },
  // 从购物车删除商品
  deleteFunc: function (e) {
    var that = this
    var goodsList = that.data.goodsList
    var ds = e.currentTarget.dataset
    var i = ds.parentindex
    console.log(e)
    wx.showModal({
      title: '提示',
      content: '亲，确认要删除该商品吗',
      success: function (res) {
        if (res.confirm) {
          var goodsCarModel = [{
            Id: goodsList[i].Id,
            SpecIds: goodsList[i].SpecIds,
            SpecInfo: goodsList[i].SpecInfo,
            Count: goodsList[i].Count, //数量
          }]
          that.updateRequest(goodsCarModel, -1)
        } else if (res.cancel) {
          console.log('用户点击取消')
        }
      }
    })
  },
  // ++
  addViewFunc: function (e) {
    var that = this
    var goodsList = that.data.goodsList
    var ds = e.currentTarget.dataset
    var i = ds.parentindex
    if (goodsList[i].goodsMsg.stockLimit == true) {
      if (goodsList[i].goodsMsg.specificationdetail.length != 0) {
        var specificationdetail = goodsList[i].goodsMsg.specificationdetail
        var templateModel = specificationdetail.find(f => f.id == goodsList[i].SpecIds)
        if (templateModel != undefined) {
          if (goodsList[i].Count < templateModel.stock) {
            goodsList[i].Count++
          } else {
            tools.showToast("超过库存")
          }
        }
      } else {
        if (goodsList[i].Count < goodsList[i].goodsMsg.stock) {
          goodsList[i].Count++
        } else {
          tools.showToast("超过库存")
        }
      }
    } else {
      goodsList[i].Count++
    }
    that.setData({
      goodsList: goodsList
    })
  },
  //--
  lessViewFunc: function (e) {
    var that = this
    var goodsList = that.data.goodsList
    var ds = e.currentTarget.dataset
    var i = ds.parentindex
    var j = ds.childindex
    if (goodsList[i].Count > 1) {
      goodsList[i].Count--
    } else {
      tools.showToast("不能再减少了哦")
    }
    this.setData({
      goodsList: goodsList,
    })
  },
  // input框输入数量
  setValueFunc: function (e) {
    console.log(e)
    var that = this
    var goodsList = that.data.goodsList
    var ds = e.currentTarget.dataset
    var i = ds.parentindex
    var j = ds.childindex
    var value = e.detail.value
    if (value != undefined) {
      if (goodsList[i].goodsMsg.stockLimit == true) {
        if (goodsList[i].goodsMsg.specificationdetail != "") {
          var specificationdetail = goodsList[i].goodsMsg.specificationdetail
          var templateModel = specificationdetail.find(f => f.id == goodsList[i].SpecIds)
          if (templateModel != undefined) {
            if (value <= templateModel.stock) {
              goodsList[i].Count = value
            } else {
              tools.showToast("超过库存")
            }
          }
        } else {
          if (value <= goodsList[i].goodsMsg.stock) {
            goodsList[i].Count = value
          } else {
            tools.showToast("超过库存")
          }
        }
      }
    }
    that.setData({
      goodsList: goodsList
    })
  },
  // 单个选择
  itemSelectFunc: function (e) {
    console.log(e)
    var parentindex = e.currentTarget.dataset.parentindex
    var goodsList = this.data.goodsList
    var templateModel = goodsList[parentindex].goodsMsg
    var key = "goodsList[" + parentindex + "].goodsMsg.sel"
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
    var goodsList = this.data.goodsList;
    if (this.data.goundcartindex > 0) {
      tools.showToast("您还在编辑状态")
      return
    }
    selectedAllStatus = !selectedAllStatus;
    for (var i in goodsList) {
      goodsList[i].goodsMsg.sel = selectedAllStatus
    }
    this.setData({
      selectedAllStatus: selectedAllStatus,
      goodsList: goodsList,
    })
    this.totalFunct()
  },
  totalFunct: function () {
    var goodsList = this.data.goodsList;
    var total = 0;
    var shopNum = 0;
    for (var i = 0; i < goodsList.length; i++) {
      if (goodsList[i].goodsMsg.sel == true) {
        total += goodsList[i].Price * goodsList[i].Count
        shopNum++
      }
    }
    var total = parseFloat(total).toFixed(2)
    this.setData({
      goodsList: goodsList,
      total: total,
      shopNum: shopNum
    })
  },
  // 选择商品属性
  showShopcarFunc: function (e) {
    var ds = e.currentTarget.dataset
    var i = ds.parentindex
    var currentStatu = e.currentTarget.dataset.statu;
    var msg = this.data.goodsList[i]
    var pickspecification = msg.goodsMsg.pickspecification
    if (pickspecification.length == 0) {
      tools.showToast("暂无规格可选")
      return;
    }
    var specificationdetail = msg.goodsMsg.specificationdetail
    var stock = specificationdetail[0].stock
    for (var k in pickspecification) {
      for (var j in pickspecification[k].items) {
        pickspecification[k].items[j].sel = this.data.sel
      }
    }
    msg.img = msg.goodsMsg.img
    msg.stockLimit = msg.goodsMsg.stockLimit
    msg.discount = msg.goodsMsg.discount
    var oldprice = parseFloat(msg.originalPrice).toFixed(2)
    var discountTotal = parseFloat(specificationdetail[0].discountPrice).toFixed(2)
    var specInfo = msg.SpecInfo
    this.data.parentindex = ds.parentindex
    this.data.specificationdetail = msg.goodsMsg.specificationdetail
    animation.utilUp(currentStatu, this)
    this.setData({
      msg: msg,
      pickspecification: pickspecification,
      discountTotal: discountTotal,
      stock: stock,
      oldprice: oldprice,
      specInfo: specInfo,
      totalCount: 1
    })
  },
  //隐藏
  hiddenShow: function (e) {
    animation.utilUp("close", this)
  },
  // 选择商品属性点击事件
  chooseFunc: function (e) {
    console.log(e)
    var attrSpacStr = []
    var specInfo = []
    var ds = e.currentTarget.dataset
    var parentindex = ds.parentindex
    var childindex = ds.childindex
    var specificationdetail = this.data.msg.goodsMsg.specificationdetail
    var pickspecification = this.data.pickspecification
    var currentList = this.data.pickspecification[parentindex];
    var self = this.data.pickspecification[parentindex].items[childindex]
    if (currentList.items.length > 0) {
      currentList.items.forEach(function (obj, i) {
        if (obj.id != self.id) {
          obj.sel = false;
        }
        else {
          obj.sel = !obj.sel;
        }
      });
    }
    var key = "pickspecification[" + parentindex + "]"
    for (var i = 0; i < pickspecification.length; i++) {
      for (var j = 0; j < pickspecification[i].items.length; j++) {
        if (pickspecification[i].items[j].sel == true) {
          attrSpacStr.push(pickspecification[i].items[j].id)
          var parentName = pickspecification[i].name
          var childName = pickspecification[i].items[j].name
          var specName = parentName + ":" + childName
          specInfo.push(specName)
        }
      }
    }
    //拼接id及名字
    attrSpacStr = attrSpacStr.join("_")
    specInfo = specInfo.join(" ")
    // 从specificationdetail拿取相对应的价格以及库存
    for (var k in specificationdetail) {
      if (attrSpacStr == specificationdetail[k].id) {
        var chooseprice = specificationdetail[k].discountPrice
        var chooseOldPrice = specificationdetail[k].price
        this.data.discountPrice = parseFloat(chooseprice).toFixed(2)
        var stock = specificationdetail[k].stock
      }
    }
    var discountTotal = parseFloat(chooseprice || this.data.msg.goodsMsg.discountPrice).toFixed(2)
    var oldprice = parseFloat(chooseOldPrice || this.data.msg.originalPrice).toFixed(2)
    this.data.attrSpacStr = attrSpacStr//拼接的id
    this.data.msg.goodsMsg.discountPricestr = discountTotal
    this.data.msg.SpecInfo = specInfo
    this.setData({
      [key]: currentList,
      discountTotal: discountTotal,
      stock: stock,
      totalCount: 1,//切换选择规格时重置选择数量
      specInfo: specInfo,
      oldprice: oldprice
    })
  },
  // +
  addFunc: function (e) {
    var msg = this.data.msg
    var SpecIds = this.data.attrSpacStr //当前商品的规格
    var totalCount = this.data.totalCount
    var template = msg.goodsMsg.GASDetailList.find(a => a.id == SpecIds)
    if (template) {
      var nownumber = template.stock
      if (totalCount < nownumber) {
        totalCount++
      } else {
        tools.showToast("不能再添加哦")
      }
    } else {
      totalCount++
    }
    msg.Count = totalCount
    this.setData({
      msg: msg,
      totalCount: totalCount
    })
  },
  // -
  lessFunc: function (e) {
    var msg = this.data.msg
    var totalCount = this.data.totalCount
    if (totalCount > 1) {
      totalCount--
    } else {
      tools.showToast("不能再减少了哦")
    }
    msg.Count = totalCount
    this.setData({
      msg: msg,
      totalCount: totalCount
    })
  },
  // 更新购物车
  addShopCartFunc: function (e) {
    var goodsList = this.data.goodsList
    var specificationdetail = this.data.specificationdetail
    var currentStatu = e.currentTarget.dataset.statu;
    var msg = this.data.msg
    var templateModel = specificationdetail.find(f => f.id == this.data.attrSpacStr)
    if (templateModel == undefined) {
      tools.showToast("请选择")
      return;
    } else {
      if (templateModel.stock == 0) {
        tools.showToast("库存不足")
        return;
      } else {
        goodsList[this.data.good_key].SpecIds = this.data.attrSpacStr
        goodsList[this.data.good_key].SpecInfo = msg.SpecInfo
        goodsList[this.data.good_key].Count = this.data.totalCount
        goodsList[this.data.good_key].Price = msg.goodsMsg.discountPricestr
      }
    }
    this.setData({
      msg: msg,
      goodsList: goodsList
    })
    animation.utilUp(currentStatu, this)
  },
  // 支付
  goPlayFunc: function (e) {
    var that = this
    var formId = e.detail.formId
    utils.commitFormId(formId, that)
    var ImgUrl = []
    var datas = []
    var goodCarIdStr = ""
    var goodsList = that.data.goodsList
    if (that.data.goundcartindex > 0) {
      tools.showToast("您还在编辑状态")
      return
    } else {
      for (var i in goodsList) {
        if (goodsList[i].goodsMsg.sel == true) {
          goodCarIdStr += goodsList[i].Id + ","
          datas.push({
            ImgUrl: goodsList[i].goodsMsg.img,
            SpecInfo: goodsList[i].SpecInfo,
            Introduction: goodsList[i].goodsMsg.name,
            discountPrice: goodsList[i].Price,
            Count: goodsList[i].Count,
            oldPrice: goodsList[i].originalPrice,
            discount: goodsList[i].discount,
            goodid: goodsList[i].goodsMsg.id
          })
        }
      }

      if (datas.length == 0) {
        tools.showToast("请选择商品")
        return;
      } else {
        var jsonstr = JSON.stringify(datas)
        var discountTotal = that.data.total
        that.data.datajson = JSON.stringify(datas)
        wx.navigateTo({
          url: '../orderList/orderList?discountTotal=' + discountTotal + "&datajson=" + that.data.datajson  + "&goodCarIdStr=" + goodCarIdStr
        })
      }
    }
  },


  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {

  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function () {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function () {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    //page.newshopcartRequest(this)
    tools.showToast("刷新成功")
    this.setData({
      total: 0,
      shopNum: 0,
      selectedAllStatus: false,
      goundcartindex: -1
    })
    wx.stopPullDownRefresh()
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  }
})