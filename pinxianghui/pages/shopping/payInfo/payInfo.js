// pages/shopping/payInfo/payInfo.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    payType: 0, //支付方式
    remark: '', //订单备注信息
    paysuccess_modal: false, //支付成功弹窗
    ispay: false, //是否已经进行过支付，控制当页面的样式
    actionId: {}, //订单id和下单id对象，如果不为空，则使用该对象的数据去发起支付
    money: 1, //储值余额金额
  },
  show_shareCode: function() {
    this.setData({
      paysuccess_modal: !this.data.paysuccess_modal,
      invite_modal: !this.data.invite_modal
    })
  },
  makephonecall: function() {
    var _s = this.data.storeInfo.storeInfo
    wx.makePhoneCall({
      phoneNumber: _s.phone,
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this
    var _s = JSON.parse(options.shopcarData)
    that.setData({
      shopcarData: _s,
      actionId: (options.actionId ? JSON.parse(options.actionId) : {})
    })
    app.login(function() {
      http.gRequest(addr.StoreInfo, {
        id: _s.storeId
      }, function(callback) {
        that.setData({
          storeInfo: callback.data.obj
        })
      })
      http.gRequest(addr.AddressList, {}, function(callback) {
        var data = callback.data.obj

        var findDefaultAddress = data.find(f => f.isDefault == 1)
        for (let i = 0; i < data.length; i++) {
          data[i].addressinfo = data[i].province + data[i].city + data[i].district + data[i].address
          data[i].peisong_type = 0
        }
        if (findDefaultAddress) {
          that.setData({
            userAddress: findDefaultAddress
          })
        }
      })
    })
  },
  choose_address: function() {
    if (Object.keys(this.data.actionId).length != 0) {
      template.showtoast('已下单，不能修改信息了！', 'none')
      return
    }
    var storeid = this.data.storeInfo.storeInfo.id
    wx.navigateTo({
      url: '/pages/my/Address/myAddress?storeid=' + storeid,
    })
  },
  reduce_num: function() {
    var _s = this.data.shopcarData
    var _acId = this.data.actionId
    if (_acId.cityMordersId) {
      template.showtoast('已下单，不能修改数量！', 'none')
      return
    }
    if (_s.Number > 1) {
      _s.Number--
    } else {
      template.showtoast('亲，不可以再减了！', 'none')
    }
    this.setData({
      shopcarData: _s
    })
  },
  add_num: function() {
    var _acId = this.data.actionId
    if (_acId.cityMordersId) {
      template.showtoast('已下单，不能修改数量！', 'none')
      return
    }
    var _s = this.data.shopcarData
    if (_s.stockLimit) {
      if (_s.Number < _s.Stock) {
        _s.Number++
      } else {
        template.showtoast('亲，库存不足够多了！', 'none')
      }
    } else {
      _s.Number++
    }
    this.setData({
      shopcarData: _s
    })
  },
  inputremark: function(e) {
    if (Object.keys(this.data.actionId).length != 0) {
      template.showtoast('已下单，修改信息无效！', 'none')
      return
    }
    this.setData({
      remark: e.detail.value
    })
  },
  gopay: function(e) { //发起支付
    var that = this
    var g = getApp().globalData;
    var _s = e.currentTarget.dataset.item
    var _a = that.data.userAddress
    if (!_a) {
      template.showtoast('请选择地址', 'none')
      return
    }
    var data = {}
    data.sourcetype = (_s.storeId == app.globalData.sourcestoreid ? 1 : 0); //进入小程序途径 0平台 1扫码
    data.aid = g.aid;
    data.storeId = _s.storeId //店铺id 
    data.groupId = _s.groupId || 0 //团id 0开团 >0参团
    data.goodsId = _s.gid //商品id
    data.specificationId = _s.SpecinfoId //规格拼接串
    data.count = _s.Number //购买数量
    data.sendway = _a.peisong_type //0商家配送 1到店自取 2面对面交易
    data.payway = that.data.payType //支付方式 0微信支付 1余额支付 2线下支付（暂时只有微信支付）
    data.consignee = _a.consignee //收货人名字
    data.phone = _a.mobile //手机号码
    data.address = (_a.peisong_type == 0 ? _a.addressinfo : (_a.peisong_type == 2 ? '' : _a.Id)) //地址详情
    data.buyerRemark = that.data.remark //订单备注信息

    if (Object.keys(that.data.actionId).length != 0) { //如果在该页面调起过支付，则使用旧citymodelId调起支付
      var _d = that.data.actionId
      var data = {}
      data.aid = app.globalData.aid
      data.storeId = _s.storeId
      data.orderId = _d.orderid
      http.pRequest(addr.PayAgain, data, function(callback) {
        var orderid = callback.data.obj.cityMordersId
        http.PayOrderNew(orderid, function(cb) {
          if (cb == 1) {
            if (_d.groupState == 0) { //如果开团或拼团未成团则弹窗，若满人则直接跳转页面
              that.setData({
                paysuccess_modal: true,
                ispay: true
              })
            } else {
              template.gonewpagebyrd('/pages/shopping/pintuanInfo/pintuanInfo?groupid=' + _d.groupId + '&storeid=' + that.data.shopcarData.storeId + '&ispayInfo=1')
            }
          }
        })
      })
    } else { //下单并调起支付
      http.pRequest(addr.AddOrder, JSON.stringify(data), function(callback) {
        console.log(callback)
        var _d = callback.data.obj
        that.setData({
          actionId: _d
        })
        var orderid = _d.cityMordersId
        http.PayOrderNew(orderid, function(cb) {
          if (cb == 1) {
            if (_d.groupState == 0) { //如果开团或拼团未成团则弹窗，若满人则直接跳转页面
              that.setData({
                paysuccess_modal: true,
                ispay: true
              })
            } else {
              template.gonewpagebyrd('/pages/shopping/pintuanInfo/pintuanInfo?groupid=' + _d.groupId + '&storeid=' + that.data.shopcarData.storeId + '&ispayInfo=1')
            }
          }
        })
      })
    }
  },
  invitepintuan_nt: function() {
    template.gonewpagebyrd('/pages/shopping/pintuanInfo/pintuanInfo?groupid=' + this.data.actionId.groupId + '&storeid=' + this.data.shopcarData.storeId + '&ispayInfo=1')
  },
  show_psmodal: function() {
    var _ac = this.data.actionId
    var storeid = this.data.storeInfo.storeInfo.id
    this.setData({
      paysuccess_modal: !this.data.paysuccess_modal
    })
    template.gonewpagebyrd('/pages/shopping/orderInfo/orderInfo?orderid=' + _ac.orderid + '&storeid=' + storeid)
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function() {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function() {
    var useraddress = app.globalData.myAddress
    if (Object.keys(useraddress).length != 0) {
      this.setData({
        userAddress: useraddress
      })
    }
  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function() {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function() {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function() {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {

  },
})