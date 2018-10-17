// pages/order/order.js
const app = getApp();
const pageRequest = require("../../utils/public-request.js");
const tools = require('../../utils/tools.js')
const util = require("../../utils/util.js")

Page({
  /**
   * 页面的初始数据
   */
  data: {
    _orderSync: {
      totalCount: 0,
      showBan: false,
      totalPrice: 0,
      shopName: "",
      addressName: "选择收货地址",
      addressId: 0,
      showText: "",
      price: [
        { totalModel: "商品总额", price: "￥569.00" },
        { totalModel: "优惠金额", price: "￥0.00" },
        { totalModel: "配送费", price: "￥0.00" },
      ],
      selfStatus: false,
      cityStatus: false,
      expressStatus: false,
    },
    pickCoupon: null,
    couponsShow: false,
    vmMycoupon: {
      list: [],
      ispost: false,
      loadall: false,
      pageindex: 1,
      pagesize: 10,
      state: 0,
      listname: "pickmycoupon",
    },
    couponlogid: 0,
    wxPlay: true,
    gpWxPlay: true
  },
  // 跳转选择地址
  chooseGoto: function () {
    tools.goNewPage("../addMyAddress/addMyAddress")
  },
  // 跳转门店选择
  storeGoto: function () {
    tools.goNewPage("../addressSelect/addressSelect?condition=1")
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
   
  },
  //详情页请求
  getGoodStock: function (para) {
    let that = this
    pageRequest.getGoodStock(para).then(data => {
      if (data.isok) {
        let [showBan, menuList, _disPrice, _oldPrce] = [false, [], 0, 0]
        menuList.push(data.goodModel)
        menuList[0].FoodGoodsId = data.goodModel.id
        menuList[0].GoodName = data.goodModel.name
        menuList[0].originalPriceStr = data.goodModel.priceStr
        menuList[0].D = data.goodModel.discountPricestr
        menuList[0].goodsMsg = { img: data.goodModel.img }
        menuList[0].GoodsState = data.goodState
        _disPrice = (Number(menuList[0].priceStr).mul(Number(getCurrentPages()[1].data.totalCount))).toFixed(2)
        _oldPrce = (Number(menuList[0].originalPriceStr).mul(Number(getCurrentPages()[1].data.totalCount))).toFixed(2)
        menuList[0].Count = getCurrentPages()[1].data.totalCount
        menuList[0].SpecInfo = getCurrentPages()[1].data.specInfo
        menuList[0].attrSpacStr = getCurrentPages()[1].data.specId
        that._exPressFun(_disPrice, _oldPrce)
        data.goodState == 3 ? showBan = true : showBan = false
        that.setData({
          menuList: menuList,
          "_orderSync.totalCount": getCurrentPages()[1].data.totalCount,
          "_orderSync.showBan": showBan
        })
        that.loadMyCoupon()
      }
    })
  },

  // 购物车请求
  getGoodsCarDataByIds: function (para) {
    let that = this
    pageRequest.getGoodsCarDataByIds(para).then(data => {
      if (data.isok) {
        let [menuList, status, showBan, totalCount, _disPrice, _oldPrce, tolPrice, oldPrice] = [data.data, [], false, 0, 0, 0, 0, 0]
        for (let i = 0, key; key = menuList[i++];) {
          if (key.GoodsState == 3) {
            status.push(key)
          }
          else {
            totalCount += key.Count
          }
        }
        status.length == menuList.length ? showBan = true : showBan = false
        _disPrice = Number(getCurrentPages()[0].data.total).toFixed(2)
        _oldPrce = Number(getCurrentPages()[0].data.totalOldprice).toFixed(2)
        that._exPressFun(_disPrice, _oldPrce)
        that.setData({
          menuList: menuList,
          "_orderSync.showBan": showBan,
          "_orderSync.totalCount": totalCount,
        })
        that.loadMyCoupon()
      }
    })
  },


  // 详情商品生成catid、
  addGoodsCarData: function () {
    let that = this
    let buyMode = 1
    let para = {
      goodId: that.data.menuList[0].FoodGoodsId,
      attrSpacStr: that.data.menuList[0].attrSpacStr,
      SpecInfo: that.data.menuList[0].SpecInfo,
      qty: that.data.menuList[0].Count,
      newCartRecord: 1,
    }
    if (that.data.wxPlay) {
      pageRequest.addGoodsCarData(para).then(data => {
        if (data.isok) {
          that._logStorage(data.cartid, that.data.couponlogid)
          that.data.wxPlay = false
        }
      })
    }
  },
  // 去支付
  payGoto: function (e) {
    let that = this
    let [menuList, buyMode, effGoodCarIdStr] = [this.data.menuList, 1, []]
    if (that.data.showBan) {
      return;
    }
    if (that.data._orderSync.cityStatus || that.data._orderSync.expressStatus) {
      if (that.data._orderSync.addressName == '选择收货地址') {
        tools.showLoadToast("忘记填写地址啦")
        return;
      }
    }
    // 商品详情
    if (getCurrentPages().length == 3 && getCurrentPages()[1].route == "pages/details/details") {
      that.addGoodsCarData()
    }
    // 购物车
    else {
      that._logStorage(that.data.goodsId, that.data.couponlogid)
    }
    pageRequest.commitFormId(e.detail.formId)
  },

  //详情&&购物车
  _logStorage: function (goodCarIdStr, couponlogid) {
    let that = this
    let wxPlay = true
    let [menuList, buyMode, effGoodCarIdStr] = [that.data.menuList, 1, []]
    if (buyMode == 1) {
      let [AddressId, lat, lng] = [0, "", ""]
      // 当自取时addressid不传
      if (that.data._orderSync.cityStatus == false && that.data._orderSync.expressStatus == false && that.data._orderSync.selfStatus == true) {
        AddressId = 0
        lat = app.globalData.currentPage.Lat
        lng = app.globalData.currentPage.Lng
      } else {
        AddressId = that.data._orderSync.addressId
        lat = ""
        lng = ""
      }
      let orderJson = {
        AddressId: AddressId,
        storeId: app.globalData.currentPage.Id,
        Message: that.data.message
      }
      orderJson = JSON.stringify(orderJson)
      if (getCurrentPages().length == 2 && getCurrentPages()[0].route == 'pages/shopCart/shopCart') {
        for (let i in menuList) {
          if (menuList[i].GoodsState != 3) {
            effGoodCarIdStr.push(menuList[i].Id)
          }
        }
        effGoodCarIdStr = effGoodCarIdStr.join(",")
      }
      else {
        effGoodCarIdStr = goodCarIdStr
      }

      let para = {
        goodCarIdStr: goodCarIdStr,
        effGoodCarIdStr: effGoodCarIdStr,
        orderjson: orderJson,
        orderType: that.data.orderType,
        buyMode: buyMode,
        lat: lat,
        lng: lng,
        fpage: that,
        couponlogid: couponlogid
      }

      if (that.data.gpWxPlay) {
        pageRequest.addGoodsOrder(para).then(function (data) {
          if (data.isok) {
            if (data.orderId == 0) {
              tools.showLoadToast('支付成功')
              app.globalData.reduction = data.reductionCart
              app.globalData.dbOrderId = data.dbOrderId
              tools.goRedito("../orderStatus/orderStatus?storeId=" + app.globalData.currentPage.Id + "&dbOrderId=" + data.dbOrderId + "&reduction=" + JSON.stringify(data.reductionCart))
            } else {
              that.wxPayFunc(data.orderId, data.dbOrderId, data.reductionCart)
            }
            that.data.gpWxPlay = false
          }
        })
      }
    }
  },
  // 调用微信支付
  wxPayFunc: function (oradid, dbOrderId, reduction) {
    let that = this
    let newparam = {
      openId: app.globalData.userInfo.openId,
      orderid: oradid,
      'type': 1,
    }
    // 立减金
    console.log("立减金=" + reduction)
    console.log("orderId=" + dbOrderId)
    app.globalData.reduction = reduction
    app.globalData.dbOrderId = dbOrderId
    util.PayOrder(newparam, {
      failed: function () {
        if (typeof (that.data._orderSync.totalPrice) == "string" && that.data._orderSync.totalPrice != "0.00") {
          tools.showLoadToast('您取消了支付')
          tools.goRedito("../orderStatus/orderStatus?storeId=" + app.globalData.currentPage.Id + "&dbOrderId=" + dbOrderId + "&reduction=" + JSON.stringify(reduction))
        }
      },
      success: function (res) {
        if (res == "wxpay") {
        } else if (res == "success") {
          tools.showLoadToast('支付成功')
          tools.goRedito("../orderStatus/orderStatus?storeId=" + app.globalData.currentPage.Id + "&dbOrderId=" + dbOrderId + "&reduction=" + JSON.stringify(reduction))
        }
      },
    })
  },
  //获取留言
  setMessageFunc: function (e) { this.data.message = e.detail.value },


  //请求方式
  _orderPage: function () {
    let that = this
    let storeId = 0
    // 配送
    if (app.globalData.cityShow || app.globalData.expressShow) {
      //获取当前指定收货地址
      pageRequest.defaultOrder().then(data => {
        let [lat, lng] = ["", ""]
        if (data.isok) {
          let address = data.data.address
          let addressName = address.Address + address.Name
          lat = address.Lat
          lng = address.Lng
          that.setData({ "_orderSync.addressId": data.data.address.Id, "_orderSync.addressName": addressName })
        }
        else {
          lat = app.globalData.currentPage.Lat
          lng = app.globalData.currentPage.Lng
        }
        //配送方式判断以及店铺id
        pageRequest.getAddStore(lat, lng).then(ct => {
          let [cityMode, expressMode, selfMode] = [false, false, false]
          // 附近是否有门店配送
          if (ct.obj) {
            cityMode = true
            that.showActionFunc(cityMode, expressMode, selfMode)
            storeId = ct.obj.Id
          }
          // 否则开启检查总店是否有快递配送
          else {
            pageRequest.getAddExpre().then(ex => {
              if (ex.isOpen) {
                cityMode = false
                expressMode = true
              } else {
                cityMode = false
                expressMode = false
                selfMode = true
              }
              that.showActionFunc(cityMode, expressMode, selfMode)
            })
            storeId = ct.BoosStoreId
          }
          that._goMoede(storeId)
        })
      })

    }
    // 自取
    else {
      pageRequest.getStoreByIdNum(app.globalData.currentPage.Id, that).then(res => {
        that._goMoede(res.obj.Id)
        that.showActionFunc(false, false, true)
      })
    }
  },
  // 进入页面判断
  _goMoede: function (storeId) {
    let that = this
    // 详情页
    if (getCurrentPages().length == 3 && getCurrentPages()[1].route == "pages/details/details") {
      let para = {
        specId: getCurrentPages()[1].data.specId,
        goodId: getCurrentPages()[1].data.msg.id,
        storeId: storeId
      }
      that.getGoodStock(para)
    }
    // 购物车页
    else {
      if (getCurrentPages().length == 2 && getCurrentPages()[0].route == 'pages/shopCart/shopCart') {
        let goodsId = []
        let goodlist = getCurrentPages()[0].data.goodlist
        for (let i = 0, key; key = goodlist[i++];) {
          if (key.goodsMsg.sel) {
            goodsId.push(key.Id)
          }
        }
        let para = {
          cartIds: goodsId.join(","),
          storeId: storeId
        }
        goodsId = goodsId.join(",")
        that.getGoodsCarDataByIds(para)
        that.data.goodsId = goodsId
      }

    }
  },
  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {
    let that = this
    that._orderPage()
    wx.getSetting({
      success(res) {
        if (res.authSetting['scope.userLocation']) {
          that.setData({ chooseShow: true })
        }else{
          that.setData({ chooseShow: false })
        }
      }
    })
  
  },
  // 配送方式
  showActionFunc: function (cityMode, expressMode, selfMode) {
    let [showText, orderType] = ["", 0]
    if (cityMode) {
      showText = "同城配送"
      orderType = 2
    }
    else if (cityMode == false && expressMode) {
      showText = "快递配送"
      orderType = 3
    }
    else if (cityMode == false && expressMode == false && selfMode) {
      showText = "到店自取"
      orderType = 1
    }
    this.setData({ "_orderSync.showText": showText })
    this.data.orderType = orderType
  },
  // 运费判断
  _exPressFun: function (_allPrice, _allOldPrice) {
    let that = this
    let _app = app.globalData
    let [cityMode, expressMode, selfMode, expressPrice, price] = [_app.cityShow, _app.expressShow, _app.selfShow, 0, that.data._orderSync.price]
    // 同城
    if (cityMode) {
      Number(_allPrice) >= _app.cityPrice ? expressPrice = 0 : expressPrice = _app.currentPage.takeOutWayModel.cityService.TakeFright
    }
    // 快递
    else if (cityMode == false && expressMode) {
      Number(_allPrice) >= _app.expressPrice ? expressPrice = 0 : expressPrice = _app.currentPage.takeOutWayModel.GetExpressdelivery.TakeFright
    }
    //自取
    else if (cityMode == false && expressMode == false && selfMode) {
      expressPrice = 0
    }
    let totalPrice = (Number(_allPrice).add(Number(expressPrice))).toFixed(2)
    price[0].price = "￥" + _allOldPrice
    price[1].price = "￥" + ((Number(_allOldPrice)).sub(Number(_allPrice))).toFixed(2)
    price[2].price = "￥" + expressPrice
    that.setData({
      "_orderSync.price": price,
      "_orderSync.selfStatus": selfMode,
      "_orderSync.cityStatus": cityMode,
      "_orderSync.totalPrice": totalPrice,
      "_orderSync.expressStatus": expressMode,
      "_orderSync.shopName": app.globalData.currentPage.StoreName,
    })
  },
  // 立减金
  loadMyCoupon: function () {
    var that = this;
    var d = that.data;
    var vm = that.data.vmMycoupon;
    if (vm.ispost || vm.loadall)
      return;

    if (!vm.ispost) {
      this.setData({
        "vmMycoupon.ispost": true,
      });
    }
    var goodsid = "";
    if (this.data.menuList) {
      goodsid = this.data.menuList.map(p => p.FoodGoodsId).join(',');
    }

    tools.GetMyCouponList({
      appId: app.globalData.appid,
      userId: app.globalData.userInfo.userid,
      pageIndex: vm.pageindex,
      state: vm.state,
      goodsId: goodsid,
      goodsInfo: JSON.stringify(that.data.menuList.map(function (item, index) {
        return {
          goodid: item.FoodGoodsId,
          totalprice: Number(item.priceStr) * item.Count * 100
        }
      }))
    }).then(function (res) {
      console.log(res);
      if (res.isok) {
        if (res.postdata.length >= vm.pagesize) {
          vm.pageindex += 1;
        }
        else {
          vm.loadall = true;
        }
        vm.list = vm.list.concat(res.postdata);
        vm.ispost = false;
      }
      that.setData({ vmMycoupon: vm })
    });
  },

  useMyCoupon: function (e) {
    var ds = e.currentTarget.dataset;
    var selCoupon = this.data.vmMycoupon.list[ds.index];
    this.data.couponlogid = selCoupon.Id
    //如果选择的是指定商品优惠券，判断当前订单列表里的商品是否符合使用条件
    if (selCoupon.GoodsIdStr != "") {
      var specifiedGood = selCoupon.GoodsIdStr.split(',');

      //筛选出可优惠的产品
      var filterGood = this.data.menuList.filter(function (item, index) {
        return specifiedGood.includes((item.FoodGoodsId).toString());
      });

      //计算优惠商品的总价格 会员打折后的总价
      var totalPrice = 0;
      if (filterGood.length > 0) {
        filterGood.forEach(function (curValue) {
          totalPrice += (Number(curValue.priceStr) || 0).mul(Number(curValue.Count))
        })
      }
      console.log(filterGood);
			/*
			如果没有符合的指定商品
			或者指定商品的价格没有达到优惠标准
			*/
      if (filterGood.length == 0) {
        wx.showModal({
          title: '提示',
          content: '订单中没有优惠券指定的商品！',
        })
        this.setData({
          pickCoupon: null,
        });
        return;
      }
      else if (selCoupon.LimitMoney > 0 && totalPrice * 100 < selCoupon.LimitMoney) {
        wx.showModal({
          title: '提示',
          content: '指定商品满' + selCoupon.Money_fmt + '元才能使用此优惠券！',
        });
        this.setData({
          pickCoupon: null,
        });
        return;
      }
    }
    this.setData({ pickCoupon: selCoupon });
    this.pickCouponOK();
  },



  calMoney: function () {
    var money = (Number(this.data._orderSync.totalPrice) || 0) * 100;
    //运费
    var freight = (Number(this.data._orderSync.price[2].price) || 0) * 100;
    //先减去运费
    money = money - freight;


    var calMoney = money;
    var money_coupon = 0;//优惠的钱


    var d = this.data;
    var coupon = d.pickCoupon;
    if (money > 0) {

      //如果使用了优惠券
      if (coupon != null) {
        //全部商品
        if (coupon.GoodsIdStr == "") {
          if (coupon.LimitMoney <= 0 || money >= coupon.LimitMoney) {
            coupon.Money = Number(coupon.Money);
            //指定金额 - 优惠
            if (coupon.CouponWay == 0) {
              calMoney = (money).sub(coupon.Money);
              money_coupon = (coupon.Money).div(100);
            }
            //折扣 * 折扣
            else if (coupon.CouponWay == 1) {
              var p = (coupon.Money).div(100).mul(10).div(100);
              calMoney = (money).mul(p);
              var coupon_p = (1).sub(p);
              money_coupon = (money).mul(coupon_p).div(100);
            }

            if (calMoney < 0) {
              calMoney = 0;
            }
          }
          else {
            wx.showModal({
              title: '提示',
              content: '未达到优惠券使用条件！',
            });
            this.setData({
              pickCoupon: null,
            });
            return;
          }
        }
        //指定产品
        else {


          /*如果没有优惠券中指定的商品 提示不可用 */
          var specifiedGood = coupon.GoodsIdStr.split(',');
          var canUse = this.data.menuList.some(function (item, index) {
            return specifiedGood.includes((item.FoodGoodsId).toString());
          });
          if (!canUse) {
            wx.showModal({
              title: '提示',
              content: '不符合优惠券使用条件！',
            });
            this.setData({
              pickCoupon: null,
            });
            return;
          }


          //筛选出可优惠的产品
          var filterGood = this.data.menuList.filter(function (item, index) {
            return specifiedGood.includes((item.FoodGoodsId).toString());
          });

          //计算指定商品的价格之和
          //优惠券的金额都是分为单位 
          //商品的价格以元为单位
          var sumMoney = 0;
          filterGood.forEach(function (item, index) {
            sumMoney += (Number(item.priceStr)).mul(100).mul(Number(item.Count));
          });


          //如果满足使用条件
          if (coupon.LimitMoney <= 0 || sumMoney >= coupon.LimitMoney) {
            coupon.Money = Number(coupon.Money);
            //指定金额 - 优惠
            if (coupon.CouponWay == 0) {
              money_coupon = coupon.Money//(sumMoney).sub(coupon.Money);
            }
            //折扣 * 折扣
            else if (coupon.CouponWay == 1) {
              //重新计算价格


              var p = (coupon.Money).div(100).mul(10).div(100);
              var coupon_p = (1).sub(p);
              money_coupon = (sumMoney).mul(coupon_p)// (sumMoney).sub((sumMoney).mul(coupon_p));
            }
            if (sumMoney < 0) {
              sumMoney = 0;
            }
            if (money_coupon < 0) {
              money_coupon = 0;
            }
            //优惠券优惠的最大金额就是指定商品的总价
            if (sumMoney - money_coupon < 0) {
              money_coupon = sumMoney;
            }
            calMoney = (money).sub(money_coupon);

            if (calMoney < 0) {
              calMoney = 0;
            }


            //优惠券减掉的钱
            money_coupon = (money_coupon).div(100);
          }
        }

      }

    }
    //再把运费加回来
    calMoney = (calMoney).add(freight);
    let _orderSync = this.data._orderSync
    // _orderSync.price[0].price = (calMoney).div(100).toFixed(2);
    _orderSync.totalPrice = (calMoney).div(100).toFixed(2)
    this.setData({
      "_orderSync": _orderSync,
      //totalPrice: money_cal_fmt,
      // money_cal: calMoney,
      // money_cal_fmt: money_cal_fmt,
      money_coupon: money_coupon.toFixed(2),
    });
  },
  chooseCoupons: function () {
    this.setData({ couponsShow: !this.data.couponsShow });
  },
  reachCouponBottom: function () {
    var vm = this.data.vmMycoupon;
    if (!vm.ispost && !vm.loadall) {
      this.loadMyCoupon();
    }
  },
  notuseCoupon: function () {
    this.setData({
      "pickCoupon": null,
      "couponsShow": false,
    });
  },
  pickCouponOK: function () {
    this.calMoney();
    this.setData({
      "couponsShow": false,
    });
  },
  //選擇支付方式
  payModeFunc: function (e) {
    let [buyMode, payModeShow, condition] = [0, "", 0]
    payModeShow = "微信支付"; buyMode = 1
    // if (condition) { payModeShow = "微信支付"; buyMode = 1 }
    // else { payModeShow = "储值支付"; buyMode = 2 }
    this.setData({ condition: condition, payModeShow: payModeShow })
    this.data.buyMode = buyMode
  },
})