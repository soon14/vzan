const http = require("http.js");
const addr = require("addr.js");
const WxParse = require('wxParse/wxParse.js');
const app = getApp();
const tools = require('tools.js');
const util = require('util.js');
const animation = require("animation.js");

let pageFunc = {
  //页面配置
  a: function (currentPage, fpage) {
    let that = fpage
    for (let i = 0, val; val = currentPage.StoreMaterialPages[i++];) {
      if (val.type == "richtxt") {
        // 替换富文本标签 控制样式
        val.content = val.content.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
        val.content = val.content.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
        val.content = val.content.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
        val.content = WxParse.wxParse('content', 'html', val.content || "", that, 5);
      }
    }
  },
  //配送方式共同数据
  b: function (getApp, currentPage, fpage) {
    let that = fpage
    let page = currentPage.takeOutWayModel
    let [cityStatus, expressStatus, selfStatus] = [page.cityService.IsOpen, page.GetExpressdelivery.IsOpen, page.selfTake.IsOpen]
    getApp.storeId = currentPage.Id
    getApp.homeid = currentPage.HomeId
    getApp.selfStatus = selfStatus
    getApp.cityStatus = cityStatus
    getApp.expressStatus = expressStatus
    getApp.currentPage = currentPage
    if (getCurrentPages.length == 0 && getCurrentPages()[0].route == "pages/index/index") {
      pageFunc.c(currentPage, fpage)
    }
    that.setData({ currentPage: currentPage })
    pageRequest.GetReductionCardList(that, currentPage.Id)
  },
  // 获取产品ids
  c: function (currentPage, fpage) {
    let that = fpage
    let goodtemplate = currentPage.StoreMaterialPages.find(d => d.type == "good");
    if (goodtemplate) {
      let _goodids = []
      goodtemplate.items.forEach(function (o, i) {
        _goodids.push(o.id)
      })
      _goodids = _goodids.join(",")
      pageRequest.getGoodsByids(_goodids, that)
    }
  },

}

let pageRequest = {
  // 获取aid
  getAid: function (fpage) {
    let that = fpage
    let aid = wx.getStorageSync("aid");
    if (aid) {
      return;
    } else {
      http.getAsync(addr.Address.Getaid, {
        appid: app.globalData.appid
      }).then(function (data) {
        if (data.isok) {
          app.globalData.aid = data.msg
          wx.setStorageSync("aid", data.msg)
        }
      })
    }
  },
  //1.门店或总店是否开启同城配送
  getStoresAfter: function (lat, lng, fpage) {
    let that = fpage
    return new Promise(function (resolve, reject) {
      http.getAsync(addr.Address.GetStores, {
        appid: app.globalData.appid,
        lat: lat,
        lng: lng,
        type: 0
      }).then(function (data) {
        let getApp = app.globalData
        if (data.obj) {
          let [obj, currentPage] = [data.obj, []]
          currentPage = obj
          currentPage.StoreMaterialPages = (JSON.parse(currentPage.StoreMaterialPages))[0].coms
          pageFunc.a(currentPage, that)
          pageFunc.b(getApp, currentPage, that)
          getApp.selfShow = false
          getApp.expressShow = false
          getApp.cityShow = currentPage.takeOutWayModel.cityService.IsOpen
          getApp.cityPrice = currentPage.takeOutWayModel.cityService.FreeFrightCost
          getApp.takeStartPrice = currentPage.takeOutWayModel.cityService.TakeStartPrice
        }
        else {
          getApp.cityShow = false
          pageRequest.getStoreExpresAfter(that, lat, lng)
        }
        that.setData({ cityStatus: getApp.cityShow })
        resolve(data);
      })
    });
  },
  //2.总店是否开启快递配送
  getStoreExpresAfter: function (fpage, lat, lng) {
    let that = fpage
    http
      .getAsync(
      addr.Address.GetStoreExpresState,
      {
        appid: app.globalData.appid,
      })
      .then(function (data) {
        let getApp = app.globalData
        if (data.isOpen) {
          let [obj, currentPage] = [data.info, []]
          currentPage = obj
          currentPage.StoreMaterialPages = (JSON.parse(currentPage.StoreMaterialPages))[0].coms
          pageFunc.a(currentPage, that)
          pageFunc.b(getApp, currentPage, that)
          getApp.cityShow = false
          getApp.selfShow = false
          getApp.expressShow = currentPage.takeOutWayModel.GetExpressdelivery.IsOpen
          getApp.expressPrice = currentPage.takeOutWayModel.GetExpressdelivery.FreeFrightCost
        }
        else {
          getApp.expressShow = false
          pageRequest.selfStoresAfter(lat, lng, that)
        }
        that.setData({ expressStatus: getApp.expressShow })
      })
  },
  //3. 门店以及总店是否开启自取
  selfStoresAfter: function (lat, lng, fpage) {
    let that = fpage
    http
      .getAsync(
      addr.Address.GetStores,
      {
        appid: app.globalData.appid,
        lat: lat,
        lng: lng,
        type: 1
      })
      .then(function (data) {
        let getApp = app.globalData
        if (data.obj) {
          let [obj, currentPage] = [data.obj, []]
          currentPage = obj
          currentPage.StoreMaterialPages = (JSON.parse(currentPage.StoreMaterialPages))[0].coms
          pageFunc.a(currentPage, that)
          pageFunc.b(getApp, currentPage, that)
          getApp.cityShow = false
          getApp.expressShow = false
          getApp.selfShow = currentPage.takeOutWayModel.selfTake.IsOpen
        } else {
          getApp.selfShow = false
          pageRequest.closeById(data.BoosStoreId, that)
        }
        that.setData({ selfStatus: getApp.selfShow })
      })
  },
  //4.当所有配送方式关闭是显示打样
  closeById: function (storeId = 0, fpage) {
    let that = fpage
    http
      .getAsync(
      addr.Address.GetStoreById,
      {
        appid: app.globalData.appid,
        storeId: storeId
      }).then(function (data) {
        let [obj, currentPage, getApp] = [data.obj, [], app.globalData]
        currentPage = obj
        currentPage.StoreMaterialPages = (JSON.parse(currentPage.StoreMaterialPages))[0].coms
        pageFunc.a(currentPage, that)
        //后台默认状态（不可修改）
        getApp.selfStatus = false
        getApp.cityStatus = false
        getApp.expressStatus = false
        //前端修改状态
        getApp.cityShow = false
        getApp.selfShow = false
        getApp.expressShow = false
        getApp.currentPage = currentPage
        // 获取产品ids
        pageFunc.c(currentPage, that)
        pageRequest.GetReductionCardList(that, currentPage.Id)
        that.setData({
          selfStatus: false,
          cityStatus: false,
          expressStatus: false,
          currentPage: currentPage,
        })
      })
  },
  //5.addressSelect页面选择门店列表请求
  getStoresList: function (lat, lng, type, fpage) {
    let that = fpage
    http
      .getAsync(
      addr.Address.GetStores,
      {
        appid: app.globalData.appid,
        lat: lat,
        lng: lng,
        actionType: "list",
        type: type
      })
      .then(function (data) {
        let showStore = false
        data.code == 1 ? showStore = true : showStore = false
        that.setData({
          storeList: data.obj,
          showStore: showStore,
        })
      })
  },
  //6.addressSelect页面附近地址列表请求
  getNearAddress: function (lat, lng, fpage) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    http.getAsync(addr.Address.GetNearMyAddress, {
      appid: app.globalData.appid,
      userid: userInfo.userid,
      lat: lat,
      lng: lng
    }).then(function (data) {
      let showMyAddress = false
      if (data.obj) {
        let msg = data.obj
        msg == 0 ? showMyAddress = false : showMyAddress = true
        for (var i = 0, val; val = msg[i++];) {
          val.Address = val.Address + val.Name
        }
        that.setData({ msg: msg })
      }
      else {
        showMyAddress = false
      }
      that.setData({ showMyAddress: showMyAddress })
    })
  },
  //7.首页选择门店
  getStoreById: function (storeId, fpage) {
    let that = fpage
    return new Promise(function (resolve, reject) {
      http
        .getAsync(
        addr.Address.GetStoreById,
        {
          appid: app.globalData.appid,
          storeId: storeId
        }).then(function (data) {
          let [obj, currentPage] = [data.obj, []]
          currentPage = obj
          currentPage.StoreMaterialPages = (JSON.parse(currentPage.StoreMaterialPages))[0].coms
          pageFunc.a(currentPage, that)
          app.globalData.selfShow = true
          app.globalData.cityShow = false
          app.globalData.expressShow = false
          app.globalData.currentPage = currentPage
          pageRequest.GetReductionCardList(that, currentPage.Id)
          // 获取产品ids
          pageFunc.c(currentPage, that)
          that.setData({
            currentPage: currentPage,
            selfStatus: true,
            cityStatus: false,
            expressStatus: false,
          })
          resolve(data)
        })
    })
  },
  //8.选择门店
  getStoreByIdNum: function (storeId, fpage) {
    let that = fpage
    return new Promise(function (resolve, reject) {
      http
        .getAsync(
        addr.Address.GetStoreById,
        {
          appid: app.globalData.appid,
          storeId: storeId
        }).then(function (data) {
          let obj = data.obj
          app.globalData.currentPage = obj
          resolve(data)
        })
    })
  },
  //9.获取当前定位
  getMapAddress: function (lat, lng, fpage) {
    let that = fpage
    http
      .getAsync(
      addr.Address.getTXMapAddress,
      {
        lat: lat,
        lng: lng,
      })
      .then(function (data) {
        if (data.isok == 1) {
          app.globalData.address = data.address
          that.setData({ address: data.address })
        }
      })
  },
  //10.获取我的收货地址
  getMyaddress: function (fpage, addressId) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    wx.showLoading({
      title: '加载中...',
      mask: true,
      success: function () {
        http.getAsync(
          addr.Address.GetMyAddress,
          {
            appId: app.globalData.appid,
            openId: userInfo.openId,
            addressId: addressId
          })
          .then(function (data) {
            if (data.isok) {
              let addressList = data.data
              for (var i = 0, val; val = addressList[i++];) {
                val.Address = val.Address + val.Name
              }
              if (addressId != 0) {
                let msg = data.data.address
                let [address, name, phone, detailAddress] = [msg.Address, msg.NickName, msg.TelePhone, msg.Name]
                that.data.Lat = msg.Lat
                that.data.Lng = msg.Lng
                that.data.id = msg.Id
                that.setData({
                  name: name,
                  phone: phone,
                  address: address,
                  detailAddress: detailAddress,
                })
              }
              that.setData({ addressList: addressList })
            }
            wx.hideLoading()
          })
      }
    })
  },
  //11.设置为默认选择地址
  getMyAddressDefault: function (fpage, addressId) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    http.postAsync(
      addr.Address.SetMyAddressDefault, {
        appId: app.globalData.appid,
        openId: userInfo.openId,
        addressId: addressId
      })
  },
  //12.删除地址
  deleteAddress: function (fpage, addressId) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    http.postAsync(
      addr.Address.DeleteMyAddress, {
        appId: app.globalData.appid,
        openId: userInfo.openId,
        addressId: addressId
      }).then(function (data) {
        if (data.isok) {
          tools.showToast("删除成功")
          setTimeout(function () {
            pageRequest.getMyaddress(that, 0)
          }, 1000)
        }
      })
  },
  //13.用户拒绝授权时获取门店信息
  rejectStoresList: function (fpage) {
    let that = fpage
    http
      .getAsync(
      addr.Address.GetStores,
      {
        appid: app.globalData.appid,
        actionType: "list",
      })
      .then(function (data) {
        let showStore = false
        if (data.code == 1) {
          let obj = data.obj
          showStore = true
          for (let i = 0, val; val = obj[i++];) {
            val.SwitchConfig = JSON.parse(val.SwitchConfig)
          }

          that.setData({
            obj: obj,
          })
        } else {
          showStore = false
        }
        that.setData({
          showStore: showStore,
        })
      })
  },
  //14.用户拒绝授权时获取附近地址
  rejectNearAddress: function (fpage) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    http.getAsync(addr.Address.GetNearMyAddress, {
      appid: app.globalData.appid,
      userid: userInfo.userid,
    }).then(function (data) {
      let showMyAddress = false
      if (data.obj != undefined) {
        let msg = data.obj
        msg == [] ? showMyAddress = false : showMyAddress = true
        for (var i = 0, val; val = msg[i++];) {
          val.Address = val.Address + val.Name
        }
        that.setData({
          msg: msg
        })
      } else {
        showMyAddress = false
      }
      that.setData({
        showMyAddress: showMyAddress
      })
    })
  },
  //15. 获取产品详情
  getGoodInfo: function (fpage, pid, storeId) {
    let that = fpage
    http.postAsync(
      addr.Address.GetGoodInfo, {
        aid: wx.getStorageSync("aid"),
        pid: pid,
        storeId: storeId,
        levelid: wx.getStorageSync("levelid")
      })
      .then(function (data) {
        if (data.isok) {
          let msg = data.msg
          if (msg.slideimgs != '') {
            msg.slideimgs = msg.slideimgs.split(",")
          }
          if (msg.description != '') {
            // 替换富文本标签 控制样式
            msg.description = msg.description.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
            msg.description = msg.description.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
            msg.description = msg.description.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
          }
          //分类对应信息
          if (msg.specificationdetail != '') {
            msg.specificationdetail = JSON.parse(msg.specificationdetail)
          }
          // 产品分类 
          if (msg.pickspecification != '') {
            msg.pickspecification = JSON.parse(msg.pickspecification)
            for (let i = 0; i < msg.pickspecification.length; i++) {
              for (let j = 0; j < msg.pickspecification[i].items.length; j++) {
                msg.pickspecification[i].items[j].sel = false
              }
            }
          }
          that.setData({
            msg: msg,
            pla: msg.plabelstr_array,
            article: WxParse.wxParse('article', 'html', msg.description, that, 5),
          })
          tools.navBarTitle(msg.name)
        }
      })
  },

  //17.获取首页产品组件
  getGoodsByids: function (ids, fpage) {
    let that = fpage
    http.postAsync(
      addr.Address.GetGoodsByids,
      {
        ids: ids,
        levelid: wx.getStorageSync("levelid"),
        storeId: app.globalData.currentPage.Id,
        homeId: app.globalData.currentPage.HomeId,
      })
      .then(function (data) {
        if (data.isok) {
          let proList = data.msg
          that.setData({
            proList: proList
          })
        }
      })
  },
  //18.获取购物车信息
  getGoodsCarData: function (storeId, fpage) {
    let that = fpage
    let showModalStatus2 = false
    let userInfo = wx.getStorageSync("userInfo")
    wx.showLoading({
      title: '加载中...',
      mask: true,
      success: function () {
        http.getAsync(addr.Address.GetGoodsCarData, {
          appId: app.globalData.appid,
          openId: userInfo.openId,
          storeId: storeId,
          levelid: wx.getStorageSync("levelid")
        }).then(function (data) {
          if (data.isok) {
            let goodlist = data.data
            for (let i in goodlist) {
              goodlist[i].img = goodlist[i].goodsMsg.img
              goodlist[i].unit = goodlist[i].goodsMsg.unit
              goodlist[i].showModalStatus2 = false
              if (goodlist[i].goodsMsg.pickspecification != '') {
                goodlist[i].pickspecification = JSON.parse(goodlist[i].goodsMsg.pickspecification)
                goodlist[i].specificationdetail = JSON.parse(goodlist[i].goodsMsg.specificationdetail)
                for (let k = 0; k < goodlist[i].pickspecification.length; k++) {
                  for (let j = 0; j < goodlist[i].pickspecification[k].items.length; j++) {
                    goodlist[i].pickspecification[k].items[j].sel = false
                  }
                }
              }
            }
            that.setData({ goodlist: goodlist })
            wx.hideLoading()
          }
        })
      }
    })

  },
  // 19.更新购物车
  updateOrDelete: function (typeFunction, goodsCarModel, fpage) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    http.postJsonAsync(
      addr.Address.UpdateOrDeleteGoodsCarData, {
        appId: app.globalData.appid,
        openId: userInfo.openId,
        function: typeFunction,
        goodsCarModel: goodsCarModel,
      }).then(function (data) {
        if (data.isok) {
          pageRequest.getGoodsCarData(app.globalData.currentPage.Id, that)
        }
      })
  },
  // 20.生成订单
  addGoodsOrder: function (para) {
    let that = para.fpage
    let userInfo = wx.getStorageSync("userInfo")
    return new Promise(function (resolve, reject) {
      http.postAsync(
        addr.Address.AddMiniappGoodsOrder,
        {
          goodCarIdStr: para.goodCarIdStr,
          effGoodCarIdStr: para.effGoodCarIdStr,
          orderjson: para.orderjson,
          orderType: para.orderType,
          buyMode: para.buyMode,
          appId: app.globalData.appid,
          openId: userInfo.openId,
          lat: para.lat,
          lng: para.lng,
          couponlogid: para.couponlogid
        }).then(function (data) {
          if (data.isok) {
            resolve(data)
          } else {
            tools.showModalFalse(data.msg)
            reject(data)
          }
        })
    })
  },
  //21.提交formid
  commitFormId: function (formid) {
    let userInfo = wx.getStorageSync("userInfo")
    http.postAsync(
      addr.Address.commitFormId, {
        appid: app.globalData.appid,
        openid: userInfo.openId,
        formid: formid,
      }).then(function (data) {
        console.log("nihao泽涛:" + data.msg + "__" + formid)
      })
  },
  //22. 订单详情请求
  getOrderDetial: function (para) {
    let that = para.fpage
    let userInfo = wx.getStorageSync("userInfo")
    return new Promise(function (resolve, reject) {
      http.postAsync(addr.Address.GetOrderDetial, {
        appId: app.globalData.appid,
        openId: userInfo.openId,
        storeId: para.storeId,
        orderId: para.orderId
      }).then(function (data) {
        if (data.isok) {
          data.storeInfo.takeOutWayModel = JSON.parse(data.storeInfo.TakeOutWayConfig)
          let parm = data.orderInfo
          let menuList = parm.goodsCarts
          let discount = 100
          for (let i in menuList) {
            menuList[i].GoodName = menuList[i].goodsMsg.name
            discount = menuList[i].goodsMsg.discount
          }
          let _orDetail = {
            user: {
              name: parm.AccepterName,
              address: parm.Address,
              phone: parm.AccepterTelePhone,
              orderId: parm.OrderId,
              dborderId: parm.Id
            },
            price: {
              totalOld: (Number(parm.GoodsMoney).sub(Number(parm.FreightPriceStr))).toFixed(2),//商品金额
              discountPrice: parm.ReducedPriceStr,//折扣金额
              BuyPriceStr: parm.BuyPriceStr,//合计
              FreightPriceStr: parm.FreightPriceStr,//快递费
              QtyCount: parm.QtyCount
            },
            order: {
              orderNum: parm.OrderNum,//订单编号
              createDateStr: parm.CreateDateStr,//下单时间
              buyModeStr: parm.BuyModeStr,//支付方式
              payDateStr: parm.PayDateStr,//支付时间
              multiStoreGetWayStr: parm.MultiStoreGetWayStr,//配送方式
              Message: parm.Message,//留言
              AcceptDateStr: parm.AcceptDateStr
            },
            state: parm.State,
            store: data.storeInfo,
            menuList: menuList,
            discount: discount
          }
          that.setData({ _orDetail })
        }
        resolve(data)
      })
    })
  },
  //23.申请退款
  outOrder: function (para) {
    let userInfo = wx.getStorageSync("userInfo")
    return new Promise(function (resolve, reject) {
      http.getAsync(addr.Address.OutOrder,
        {
          appId: app.globalData.appid,
          openid: userInfo.openId,
          storeId: para.storeId,
          orderId: para.orderId,

        }).then(function (data) {
          if (data.isok) {
            tools.showToast("已申请退款")
          } else {
            tools.showToast(data.msg)
          }
          resolve(data)
        })
    })
  },
  //24.取消订单
  changeOrder: function (para) {
    let userInfo = wx.getStorageSync("userInfo")
    return new Promise(function (resolve, reject) {
      http.getAsync(addr.Address.ChangeOrder, {
        appId: app.globalData.appid,
        openId: userInfo.openId,
        storeId: para.storeId,
        orderId: para.orderId,
        state: para.state
      }).then(function (data) {
        if (data.isok) {
          tools.showToast(data.msg)
        } else {
          tools.showToast(data.msg)
        }
        resolve(data)
      })
    })
  },
  //25.二次支付
  paySec: function (newparam) {
    util.PayOrder(newparam, {
      success: function (res) {
        if (res == "wxpay") {
        } else if (res == "success") {
          wx.showToast({
            title: '支付成功',
            duration: 1000,
            icon: "loading"
          })
          setTimeout(function () {
            tools.goNewPage("../orderStatus/orderStatus?storeId=" + app.globalData.currentPage.Id + "&dbOrderId=" + newparam.dbOrderId + "&reduction=" + JSON.stringify(app.globalData.reduction))
          }, 1000)
        }
      },
      failed: function () {
        wx.showToast({
          title: '您取消了支付',
          duration: 1000,
          icon: "loading"
        })
      },
    })
  },
  //26. 订单列表
  orderRequest: function (state, fpage) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    const vm = that.data.goodListViewModal
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true
    that.setData({ "goodListViewModal.ispost": true });
    http.postAsync(addr.Address.GetOrderList, {
      appId: app.globalData.appid,
      openId: userInfo.openId,
      storeId: app.globalData.currentPage.Id,
      pageindex: vm.pageindex,
      pagesize: vm.pagesize,
      state: state,
    }).then(function (data) {
      vm.ispost = false;
      if (data.isok) {
        for (let i = 0; i < data.orderList.length; i++) {
          data.orderList[i].goodsCart = data.orderList[i].goodsCarts
          data.orderList[i].Count = data.orderList[i].goodsCart.length
          for (let j = 0; j < data.orderList[i].goodsCarts.length; j++) {
            if (data.orderList[i].goodsCarts.length >= 4) {
              data.orderList[i].goodsCarts = data.orderList[i].goodsCarts.slice(0, 3)
            }
          }
        }
        // /更改状态数据
        data.orderList.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
        data.orderList.length > 0 ? vm.list = vm.list.concat(data.orderList) : "";
        that.setData({ "goodListViewModal": vm })
      }
    })


  },
  //27.添加商品至购物车
  addGoodsCarData: function (para) {
    let userInfo = wx.getStorageSync("userInfo")
    return new Promise(function (resolve, reject) {
      http.postAsync(addr.Address.AddGoodsCarData, {
        appId: app.globalData.appid,
        openId: userInfo.openId,
        goodId: para.goodId,
        attrSpacStr: para.attrSpacStr,
        SpecInfo: para.SpecInfo,
        qty: para.qty,
        newCartRecord: para.newCartRecord
      }).then(function (data) {
        resolve(data)
      })
    })
  },
  // 28.产品分类请求
  classifyRequest: function (fpage) {
    let that = fpage
    let typesList = []
    http.postAsync(addr.Address.GetGoodTypes, {
      aid: wx.getStorageSync("aid")
    }).then(function (data) {
      if (data.isok) {
        typesList = data.data
        let templateMode = typesList.find(f => f.name == '全部')
        if (templateMode == undefined) {
          typesList.unshift({ id: "", name: "全部" })
        } else {
          return;
        }
        that.setData({
          typesList: typesList,
        })

      } else {
        app.showToast(data.msg)
      }
    })
  },
  //29.产品请求
  proRequest: function (typeid, storeid, fpage) {
    let that = fpage
    const vm = that.data.goodListViewModal
    return new Promise(function (resolve, reject) {
      if (vm.ispost || vm.loadall)
        return;
      if (!vm.ispost)
        vm.ispost = true
      that.setData({ "goodListViewModal.ispost": true });
      //获取产品列表
      http.postAsync(addr.Address.GetGoodsList, {
        aid: wx.getStorageSync("aid"),
        typeid: typeid,
        pageindex: vm.pageindex,
        pagesize: vm.pagesize,
        storeid: storeid,
        levelid: wx.getStorageSync("levelid")
      })
        .then(function (data) {
          vm.ispost = false;
          if (data.isok) {
            // /更改状态数据
            data.msg.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
            data.msg.length > 0 ? vm.list = vm.list.concat(data.msg) : "";
            that.setData({ "goodListViewModal": vm })
          }
          resolve(data)
        })
    })
  },
  //30.保存地址
  saveAddress: function (addressJson, fpage) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    let vm = that.data.listViewModal
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      that.setData({
        "listViewModal.ispost": true
      })
    http
      .postJsonAsync(
      addr.Address.AddOrEditMyAddress,
      {
        appId: app.globalData.appid,
        openId: userInfo.openId,
        addressJson: addressJson
      })
      .then(function (data) {
        if (data.isok == true) {
          tools.showToast("保存成功")
          setTimeout(function () {
            wx.navigateBack({
              delta: 1,
              success: function () {
                vm.ispost = false;//请求完毕，关闭请求开关
                that.setData({ "listViewModal": vm })
              },
            })
          }, 1000)
        }
      })
  },
  // 31.获取购物车已有商品
  getGoodsCarDataByIds: function (para) {
    let userInfo = wx.getStorageSync("userInfo")
    return new Promise(function (resolve, reject) {
      http.getAsync(addr.Address.GetGoodsCarDataByIds, {
        appId: app.globalData.appid,
        openId: userInfo.openId,
        storeId: para.storeId,
        cartIds: para.cartIds
      }).then(function (data) {
        resolve(data)
      })
    })
  },
  //32.详情页面商品
  getGoodStock: function (para) {
    return new Promise(function (resolve, reject) {
      http.getAsync(
        addr.Address.GetGoodStockState,
        {
          appid: app.globalData.appid,
          goodId: para.goodId,
          specId: para.specId,
          storeId: para.storeId,
          levelid: wx.getStorageSync("levelid")
        }).then(function (data) {
          resolve(data)
        })
    })
  },
  //33.用户拒绝授权时获取当前定位
  rejectLocation: function () {
    return new Promise(function (resolve, reject) {
      http
        .getAsync(addr.Address.GetLocation)
        .then(function (data) {
          app.globalData.location.latitude = data.obj.result.location.lat
          app.globalData.location.longitude = data.obj.result.location.lng
          resolve(data)
        })
    })
  },
  //34.(addressSelect)地址选择判断是否有门店
  getAddStore: function (lat, lng) {
    return new Promise(function (resolve, reject) {
      http
        .getAsync(
        addr.Address.GetStores,
        {
          appid: app.globalData.appid,
          lat: lat,
          lng: lng,
          type: 0
        })
        .then(function (data) {
          resolve(data);
        })
    });
  },
  //2.总店是否开启快递配送
  getAddExpre: function () {
    return new Promise(function (resolve, reject) {
      http
        .getAsync(
        addr.Address.GetStoreExpresState,
        {
          appid: app.globalData.appid,
        })
        .then(function (data) {
          resolve(data);
        })
    });
  },
  //35.获取指定默认地址
  defaultOrder: function () {
    let userInfo = wx.getStorageSync("userInfo")
    return new Promise(function (resolve, reject) {
      http
        .getAsync(
        addr.Address.GetMyAddress,
        {
          appId: app.globalData.appid,
          openId: userInfo.openId,
          isDefault: 1
        }).then(function (data) {
          resolve(data)
        })
    })
  },
  // 立减金
  GetReductionCard: function (that, couponsId, orderId, state, cb) {
    let userInfo = wx.getStorageSync("userInfo")
    http.postAsync(addr.Address.GetReductionCard, {
      couponsId: couponsId,
      orderId: orderId,
      userId: userInfo.userid,
      openId: userInfo.openId,
    }).then(function (data) {
      if (data.coupon != null) {
        data.coupon.StartUseTimeStr = data.coupon.StartUseTimeStr.replace(/[.]/g, '/')
        data.coupon.EndUseTimeStr = data.coupon.EndUseTimeStr.replace(/[.]/g, '/')

        that.setData({
          coupon: data.coupon,
          userList: data.userList
        })
      }
      if (data.isok) {
        if (state == 0) { //getsmoney
          for (var i = 0; i < data.coupon.SatisfyNum; i++) {
            that.data.userLogo.push({
              HeadImgUrl: ''
            })
          }
          for (var j = 0; j < data.userList.length; j++) {
            that.data.userLogo[j].HeadImgUrl = data.userList[j].HeadImgUrl
          }
          that.setData({ userLogo: that.data.userLogo })
        }

        if (state == 1) { //click--invitegetsmoney
          if (data.coupon.SatisfyNum == data.userList.length) {

            if (data.userInfo == null) {
              wx.showToast({
                title: '该活动已满员',
                icon: 'loading'
              })
              return
            } else {
              var finduserid = data.userList.find(f => f.Id == data.userInfo.Id)

              if (finduserid) {
                wx.showToast({
                  title: '领取成功'
                })
                setTimeout(function () {
                  var coupon = JSON.stringify(that.data.coupon)
                  wx.redirectTo({
                    url: '/pages/addCoupon/usersmoney?orderId=' + app.globalData.dbOrderId
                  })
                }, 1000)
              } else {
                wx.showToast({
                  title: '该活动已被领取'
                })
              }
            }

          } else {
            wx.redirectTo({
              url: '/pages/addCoupon/getsmoney'
            })
          }
        }

        if (state == 2) { //onload--invitegetsmoney 
          if (data.userInfo == null) {
            wx.showToast({
              title: '该活动已满员',
              icon: 'loading'
            })
            return
          }
          var finduserid = data.userList.find(f => f.Id == data.userInfo.Id)
          if (data.coupon.SatisfyNum > data.userList.length) {
            wx.redirectTo({
              url: '../addCoupon/getsmoney'
            })
            return
          }
          if (finduserid) {
            if (getCurrentPages()[getCurrentPages().length - 1].route != "pages/addCoupon/usersmoney") {
              wx.redirectTo({
                url: '../addCoupon/usersmoney?coupon=' + JSON.stringify(data.coupon)
              })
            }
          }

        }
        wx.setStorageSync("coupon", data.coupon)
      }
      else {
        if (data.coupon == null) {
          tools._clickMoadlNo('该活动已取消').then(data => {
            tools.goLaunch("../index/index")
          })

          return
        }
        if (data.userList == undefined && data.msg == "立减金已放送完毕,请关注下次的立减金优惠哦") {
          wx.redirectTo({
            url: '../addCoupon/usersmoney?coupon=' + JSON.stringify(wx.getStorageSync("coupon"))
          })
          that.setData({
            isGet: true,
            msg: data.msg,
          })
          return;
        }
        if (data.userList == undefined && data.msg == "你已超过领取限制") {
          that.setData({
            isGet: true,
            msg: data.msg,
          })
          return;
        }
        wx.showToast({
          title: data.msg,
          icon: 'loading'
        })
      }
    })
  },

  // 查询正在参与的立减金活动
  GetReductionCardList: function (that, storeId) {
    let userInfo = wx.getStorageSync("userInfo")
    http.postAsync(
      addr.Address.GetReductionCardList, {
        userId: userInfo.userid,
        openId: userInfo.openId,
        aid: wx.getStorageSync("aid"),
        storeId: storeId,
      })
      .then(function (data) {
        if (data.isok) {
          for (var i = 0; i < data.coupons.length; i++) {
            data.coupons[i].StartUseTimeStr = data.coupons[i].StartUseTimeStr.replace(/[.]/g, '/')
            data.coupons[i].EndUseTimeStr = data.coupons[i].EndUseTimeStr.replace(/[.]/g, '/')
          }
          that.setData({ couponsList: data.coupons })
        }
      })
  },
  //我也要做小程序
  agminapp: function (fpage) {
    let appid = app.globalData.appid
    http.postAsync(addr.Address.GetAgentConfigInfo, {
      appid,
    }).then(data => {
      if (data.isok == 1) {
        fpage.setData({ agconfig: data.AgentConfig })
      }
    })
  },
}

module.exports = pageRequest