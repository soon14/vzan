// pages/live/live.js
const http = require('../../utils/http.js');
const addr = require("../../utils/addr.js");
const animation = require("../../utils/animation.js")
const page = require("../../utils/pageRequest.js")
const tools = require("../../utils/tools.js")
const app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    num: 1,
    selectedAllStatus: false,
    total: "0.00",
    // 第一层显隐
    showModalStatus: false,
    //第一层内页切换
    showModalStatus2: false,
    //第二层显隐
    showModalStatus3: false,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    this.data.ids = options.ids
    this.data.comindex = options.comindex
    this.data.pageindex = options.pageindex
    if (options && options.pages) {
      this.data.pages = options.pages
    }
    else {
      this.data.pages = app.globalData.pages
    }
    this.liveRequest(this.data.pages)

  },
  /**
   * 接口请求
   */
  liveRequest: function (pages) {
    let that = this
    let [pageindex, comindex] = [that.data.pageindex, that.data.comindex]
    if (pages[pageindex].coms[comindex].type == "live") {
      let url = pages[pageindex].coms[comindex].vzliveurl
      page.getLiveAddress(url, that).then(data => {
        if (data.isok) {
          let liveUrl = ""
          let pageImg = ""
          data.dataObj.state == -1 || data.dataObj.state == 2 ? tools.showToast(data.Msg) : ""
          if (data.dataObj.playurl) {
            liveUrl = data.dataObj.playurl
            pageImg = data.dataObj.cover
          }
          else {
            pageImg = pages[pageindex].coms[comindex].img
          }
          that.setData({
            liveUrl: liveUrl,
            pageImg: pageImg
          })
        }
      })
      that.liveGoodsList()
    }
  },

  // 底部产品
  liveGoodsList: function () {
    let that = this
    page.goodsRequest(that.data.ids, that).then(data => {
      if (data.isok) {
        that.setData({ product: data.msg })
      }
    })
  },
  //弹出单个产品
  liveGoods: function (ids) {
    let that = this
    page.goodsRequest(ids, that).then(data => {
      if (data.isok) {
        if (data.msg[0].pickspecification) {
          data.msg[0].pickspecification = JSON.parse(data.msg[0].pickspecification)
          for (let i = 0, pic; pic = data.msg[0].pickspecification[i++];) {
            for (let j = 0, picKey; picKey = pic.items[j++];) {
              picKey.sel = false
            }
          }
        }
        that.setData({
          liveData: data.msg[0],
          discountPrice: data.msg[0].discountPrice,
        })
        that.data.liveId = data.msg[0].id
      }
    })
  },

  // 立即购买请求
  buyRequest: function () {
    let that = this
    let menu = that.data.liveData
    let datas = []
    let total = that.data.discountPrice * that.data.num
    http
      .postAsync(
      addr.Address.addGoodsCarData,
      {
        appId: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        goodid: that.data.liveId,
        qty: that.data.num,
        newCartRecord: 0
      }
      ).then(function (data) {
        if (data.isok == 1) {
          let cartid = data.cartid
          datas.push({
            Count: that.data.num,
            Introduction: menu.name,
            SpecInfo: that.data.specInfo,
            oldPrice: menu.price,
            discountPrice: that.data.discountPrice,
            discount: menu.discount,
            ImgUrl: menu.img,
          })
					
          let jsonstr = JSON.stringify(datas)
					that.data.datajson = jsonstr
          tools.goNewPage('../orderList/orderList?datajson=' + jsonstr + "&goodCarIdStr=" + cartid + "&discountTotal=" + total)

        } else {
          tools.showToast(data.msg)
        }
      })
  },
  /**
   * 功能
   */
  // 第一层滚动产品列表
  firstScrollFunc: function (e) {
    let that = this
    let ds = e.currentTarget.dataset
    let [id, currentStatu] = [ds.id, ds.statu]
    currentStatu == "open" ? page.shopCartData(that) : ""
    that.liveGoods(id)
    animation.utilUp(currentStatu, that)
  },
  // 选择规格
  selectFunc: function (e) {
    console.log(e)
    let ds = e.currentTarget.dataset
    let liveData = this.data.liveData
    let [attrSpacStr, specInfo, template, priceCount, stock, discountPrice] = [[], [], [], 0, 0, this.data.discountPrice]
    let [parentindex, childindex, spec, pick] = [ds.parentindex, ds.childindex, liveData.GASDetailList, liveData.pickspecification]
    let key = "liveData.pickspecification[" + parentindex + "]"
    let currentList = pick[parentindex];
    let self = pick[parentindex].items[childindex]

    if (currentList.items.length > 0) {
      currentList.items.forEach(function (obj, i) {
        obj.id != self.id ? obj.sel = false : obj.sel = !obj.sel
      })
    }
    // 循环拿出sel为true的id以及name
    for (let i = 0, lKey; lKey = pick[i++];) {
      for (let j = 0, valKey; valKey = lKey.items[j++];) {
        if (valKey.sel) { attrSpacStr.push(valKey.id); specInfo.push(valKey.name) }
      }
    }
    //拼接id及名字
    attrSpacStr = attrSpacStr.join("_")
    specInfo = specInfo.join(" ")
    template = spec.find(k => k.id == attrSpacStr)
    if (template) {
      priceCount = template.discountPricestr
      discountPrice = template.discountPricestr
      stock = template.stock
    }
    this.setData({
      [key]: currentList,
      discountPrice: discountPrice,
      num: 1
    })
    this.data.price = priceCount
    this.data.stock = stock
    this.data.specInfo = specInfo
    this.data.attrSpacStr = attrSpacStr
  },
  /* 第一层点击减号 */
  bindMinusFunc: function () {
    let num = this.data.num;
    if (num > 1) {
      num--;
    } else {
      tools.showToast("不能再减了，亲")
    }
    this.setData({ num: num });
  },
  /* 第一层点击加号 */
  bindPlusFunc: function () {
    let num = this.data.num;
    let attrSpacStr = this.data.attrSpacStr
    let pic = this.data.liveData.pickspecification
    let spec = this.data.liveData.GASDetailList
    if (this.data.liveData.stockLimit) {
      if (this.data.liveData.stock == 0) {
        tools.showToast("亲，该商品已售罄!")
        num = 1
      }
      else {
        if (pic.length) {
          let tempId = spec.find(f => f.id == attrSpacStr)
          if (tempId) {
            if (num < tempId.stock) {
              num++;
            } else {
              tools.showToast("库存不足")
            }
          }
          else {
            num = 1
          }
        } else {
          if (num < this.data.liveData.stock) {
            num++
          }
          else {
            tools.showToast("库存不足")
          }
        }
      }

    } else {
      num++
    }

    this.setData({ num: num });
  },
  //横向切换
  secondScrollFunc: function (e) {
    let [that, id] = [this, e.currentTarget.dataset.id]
    that.liveGoods(id)
    animation.utilRow("open", that)
    that.setData({ num: 1 })
    that.data.attrSpacStr = []
    that.data.specInfo = []
  },
  // 防空验证
  verdictCartFunc: function (e) {
    let that = this
    let [mode_Id, attrSpacStr] = [e.currentTarget.id, that.data.attrSpacStr]
    let [spec, pick] = [that.data.liveData.GASDetailList, that.data.liveData.pickspecification]
    let templateSpec = spec.find(k => k.id == attrSpacStr)
    let para = {
      goodid: that.data.liveId,
      attrSpacStr: attrSpacStr,
      SpecInfo: that.data.specInfo,
      qty: that.data.num,
      newCartRecord: 0,
      fpage: that
    }
    if (spec.length) {
      if (templateSpec) {
        if (mode_Id == 0) {
          tools.showToast('添加成功')
          page.addToCartRequest(para)
        }
        else {
          that.buyRequest()
        }
      }
      else {
        tools.showToast("请选择商品规格")
      }
    }
    else {
      if (mode_Id == 0) {
        tools.showToast('添加成功')
        page.addToCartRequest(para)
      }
      else {
        that.buyRequest()
      }
    }
  },
  //购物车弹窗
  showShopFunc: function (e) {
    let that = this
    let currentStatu = e.currentTarget.dataset.statu;
    currentStatu == "open" ? page.shopCartData(that) : ""
    animation.utilSecond(currentStatu, that)
    that.setData({ selectedAllStatus: false, total: "0.00" })
  },
  // 删除单个购物车商品
  deleteCartFunc: function (e) {
    let that = this
    let proId = e.currentTarget.dataset.id
    console.log(e)
    wx.showModal({
      title: '提示',
      content: '亲，确认要删除该商品吗',
      success: res => {
        if (res.confirm) {
          page.updateShopCar(proId)
          setTimeout(res => { tools.showToast('删除成功'); page.shopCartData(that) }, 500)
        }
      }
    })
  },
  /* 第二层减号 */
  bindMinusCart: function (e) {
    let that = this
    let ds = e.currentTarget.dataset
    let [i, j, CartItems] = [ds.idf, ds.ids, that.data.shopCartList]
    if (CartItems[i].GoodsCar[j].Count > 1) {
      CartItems[i].GoodsCar[j].Count--
    } else {
      tools.showToast('不能再减了，亲')
    }
    this.setData({ shopCartList: CartItems });
    that.totalFunc()
  },
  /* 第二层加号 */
  bindPlusCart: function (e) {
    let that = this
    let ds = e.currentTarget.dataset
    let [i, j, CartItems] = [ds.idf, ds.ids, that.data.shopCartList]
    let count = CartItems[i].GoodsCar[j].Count
    let SpecIds = CartItems[i].GoodsCar[j].SpecIds
    let template = CartItems[i].GoodsCar[j].goodsMsg.GASDetailList.find(s => s.id == SpecIds)
    if (CartItems[i].GoodsCar[j].goodsMsg.stockLimit) {
      if (template) {
        let nownumber = template.stock
        if (CartItems[i].GoodsCar[j].Count < nownumber) {
          CartItems[i].GoodsCar[j].Count++
        } else {
          tools.showToast('不能再添加哦')
        }
      }
      else {
        if (CartItems[i].GoodsCar[j].Count < CartItems[i].GoodsCar[j].goodsMsg.stock) {
          CartItems[i].GoodsCar[j].Count++
        }
        else {
          tools.showToast('不能再添加哦')
        }
      }
    }
    else {
      CartItems[i].GoodsCar[j].Count++
    }

    console.log(e)
    that.setData({ shopCartList: CartItems })
    that.totalFunc()
  },
  // 计算总价格
  totalFunc: function () {
    let that = this
    let [total, menus, CarItems] = [0, [], that.data.shopCartList]
    for (let i = 0, carKey; carKey = CarItems[i++];) {
      for (let j = 0, carValue; carValue = carKey.GoodsCar[j++];) {
        if (carValue.choose) {
          total += Number(carValue.goodsMsg.discountPricestr).mul(carValue.Count)
          menus.push(carValue)
        }
      }
    }

    let totalitem = total.toFixed(2)
    that.setData({
      total: totalitem,
      menus: menus
    })
  },
  // 单条选择
  selectItemFunc: function (e) {
    let that = this
    let ds = e.currentTarget.dataset
    let [i, j, CartItems] = [ds.idf, ds.ids, that.data.shopCartList]
    CartItems[i].GoodsCar[j].choose = !CartItems[i].GoodsCar[j].choose
    let temp = CartItems.find(f => f.GoodsCar.find(d => d.choose == undefined || d.choose == false) != undefined)
    let selectedAllStatus = true
    if (temp != undefined) {
      selectedAllStatus = false
    }
    that.setData({
      shopCartList: CartItems,
      selectedAllStatus: selectedAllStatus,
    })
    that.totalFunc()

  },
  // 全选
  selectAllFunc: function () {
    let that = this
    let selectedAllStatus = that.data.selectedAllStatus;
    selectedAllStatus = !selectedAllStatus;
    let CarItems = that.data.shopCartList
    for (let i = 0; i < CarItems.length; i++) {
      for (let j = 0; j < CarItems[i].GoodsCar.length; j++) {
        CarItems[i].GoodsCar[j].choose = selectedAllStatus
      }
    }
    that.setData({
      selectedAllStatus: selectedAllStatus,
      shopCartList: CarItems,
    })
    that.totalFunc()
  },

  // 跳转订单详情
  navOrderFunc: function () {
    let that = this
    let [goodCarIdStr, datas, menu] = ["", [], that.data.menus]
    if (that.data.total == 0) {
      tools.showToast('请选择商品')
      return
    }
    for (let i = 0; i < menu.length; i++) {
      let item = menu[i]
      goodCarIdStr += item.Id + "," //购物车ID串
      datas.push({
        Count: item.Count,
        oldPrice: item.Price,
        SpecInfo: item.SpecInfo,
        ImgUrl: item.goodsMsg.img,
        Introduction: item.goodsMsg.name,
        discountPrice: item.goodsMsg.discountPrice,
      })
    }
    let jsonstr = JSON.stringify(datas)
    tools.goNewPage('../orderList/orderList?discountTotal=' + that.data.total + "&datajson=" + jsonstr + "&goodCarIdStr=" + goodCarIdStr)
  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {
    let that = this
    tools.showLoadToast("正在刷新")
    setTimeout(res => { tools.showToast("刷新成功"); that.liveRequest() }, 1000)
    wx.stopPullDownRefresh()
  },



  onShareAppMessage: function () {
    var that = this
    return {
      title: that.data.shareTitle,
      path: 'pages/live/live?pageindex=' + that.data.pageindex + "&comindex=" + that.data.comindex + "&ids=" + that.data.ids + "&pages=" + that.data.pages,
      imageUrl: that.data.shareImage,
      success: function (res) {
        tools.showToast("转发成功")
      }
    }
  },

})