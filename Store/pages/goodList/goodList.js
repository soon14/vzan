// pages/goodsList/goodsList.js
var WxParse = require('../../utils/wxParse/wxParse.js');
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    clientTel: 0,//客户电话，用于判定是否已绑定
    yuanjia: 0,//商品原价
    // 立即购买
    menus: [],
    Array: [],
    // 商品详情页面id
    goodId: '',
    // 轮播图属性
    indicatorDots: false,
    autoplay: false,
    interval: 5000,
    duration: 1000,
    imgUrls: [],
    // 商品详情
    good: [],
    // 尺码 颜色
    goodsAttrList: [],
    // 正品保障 闪电发货 专业售后 七天退货
    icon: [
      // { icon: '../../image/a11.png', txt: '正品保障' },
      // { icon: '../../image/a11.png', txt: '闪电发货' },
      // { icon: '../../image/a11.png', txt: '专业售后' },
      // { icon: '../../image/a11.png', txt: '七天退货' },
    ],
    cardid: 0,
    // 评价模板
    appraise: [
      {
        imgUrls:
        ['http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
          '../../image/test.png',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg'
        ],
        content: [
          {
            userimg: '../../image/test.png', username: '2号车厢', visitTime: '4小时前', prase: '9', context: '我不会说太多暖心的情话，但是我会做好每一件小事我不会说太多暖心的情话，但是我会做好每一件小事我不会说太多暖心的情话，但是我会做好每一件小事我不会说太多暖心的情话，但是我会做好每一件小事我不会说太多暖心的情话，但是我会做好每一件小事我不会说太多暖心的情话，但是我会做好每一件小事我不会说太多暖心的情话，但是我会做好每一件小事我不会说太多暖心的情话，但是我会做好每一件小事,然后一直陪在你身边。'
          }
        ]
      },

      {
        imgUrls:
        ['http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
          'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
          '../../image/test.png',
        ],
        content: [
          {
            userimg: '../../image/test.png', username: '2号车厢', visitTime: '4小时前', prase: '9', context: 'asd,然后一直陪在你身边。'
          }
        ]
      },
    ],
    // 选择商品属性显隐
    setNature: false,
    setNature1: false,
    setNature2: false,
    groupindex1: 0,
    groupindex2: 0,
    groupindex3: 0,
    allgroupindex: '',
    openId: '',
    // 默认库存
    defaultInventory: 0,
    // 选择规格后商品的库存
    tempInventory: 0,
    // 商品对应价格
    tempprice: 0,
    // 第一项属性名字 规格
    firstName: '',
    firstInfo: '',
    // 第二项属性名字 规格
    secondName: '',
    secondInfo: '',
    // 第三项属性名字 规格
    thirdInfo: '',
    thirdName: '',
    // 商品购买数量
    buyamount: 1,
    goods1: [],

    //选择的规格属性
    selectAttrInfo: "请选择规格属性",
    //总价
    sumprice: 0,
    //单价
    singleprice: 0,
    singlecount: 0,
  },
  // 返回主页
  siwchtoIndex: function () {
    wx.switchTab({
      url: '../index/index',
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    that.setData({ goodId: options.id })
    console.log('`', options)

    app.getUserInfo(function (e) {
      // 获取商品详情id
      that.setData({
        userInfo: app.globalData.userInfo,
        goodId: options.id,
        openId: e.openId,
        clientTel: app.globalData.userInfo.TelePhone
      })
      that.inite()
    })
    util.GetUserWxAddress(that)
  },
  //初始化
  inite: function (e) {
    var that = this
    wx.request({
      url: addr.Address.getgoodInfo,
      data: {
        appid: app.globalData.appid,
        goodsid: that.data.goodId,
        levelid: app.globalData.levelid,
        uid: app.globalData.userInfo.UserId
      },
      method: "GET",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        if (res.data.isok == 1) {
          if (!(res.data.postdata.goodsdetail.State >= 0 && res.data.postdata.goodsdetail.IsSell == 1)) {
            wx.showModal({
              title: '提示',
              content: '该商品已下架或售罄',
              success: function (res) {
                if (res.confirm) {
                  wx.navigateBack({
                    delta: 1
                  })
                } else if (res.cancel) {
                  wx.navigateBack({
                    delta: 1
                  })
                }
              }
            })
          }

          //规格属性
          // var selectAttrInfo = ""
          // if (res.data.postdata.goodsAttrList.length > 0) {
          //   for (var i = 0; i < res.data.postdata.goodsAttrList.length; i++) {
          //     selectAttrInfo += res.data.postdata.goodsAttrList[i].AttrName + "  "
          //   }
          // }
          var sumprice = res.data.postdata.goodsdetail.discountPricestr
          var yuanjia = res.data.postdata.goodsdetail.PriceStr
          // var singleprice = res.data.postdata.goodsdetail.PriceStr
          var singleprice = res.data.postdata.goodsdetail.discountPricestr
          var singlecount = (res.data.postdata.goodsdetail.Inventory) - (res.data.postdata.goodsdetail.IsSell)
          // 使用h5模板
          WxParse.wxParse('postDescription', 'html', res.data.postdata.goodsdetail.Description, that);
          that.setData({
            yuanjia: yuanjia,
            imgUrls: res.data.postdata.imgattent,
            good: res.data.postdata.goodsdetail,
            undiscountprice: res.data.postdata.goodsdetail.PriceStr,
            discount: res.data.postdata.goodsdetail.discount,
            Array: res.data.postdata,
            goodsAttrList: res.data.postdata.goodsAttrList,
            tempInventory: res.data.postdata.goodsdetail.Inventory,
            tempprice: res.data.postdata.goodsdetail.PriceStr,
            defaultInventory: res.data.postdata.goodsdetail.Stock,
            // selectAttrInfo: selectAttrInfo,
            sumprice: sumprice,
            singleprice: singleprice,
            singlecount: singlecount,
          })
        }
        console.log('~~~~~~~~~~~~~~~~~~~~~~`', res)
      },
      fail: function () {
        console.log("获取商品详情出错")
        wx.showToast({
          title: '获取商品详情出错',
        })
      }
    },
    )

    this.GetGoodsList(0)
  },
  // 点击查看轮播大图
  previewImage: function (e) {
    var imageArray = this.data.imgUrls
    var index = e.currentTarget.id
    var previewImage = imageArray[index]
    wx.previewImage({
      current: previewImage,
      urls: imageArray
    })
  },
  // 点击查看用户评价图
  previewImage1: function (e) {
    console.log(e.target.id[0])
    console.log(e.target.id[1])
    var index = e.target.id[0]
    var index2 = e.target.id[1]
    var imgs = this.data.appraise[index].imgUrls
    wx.previewImage({
      current: imgs[index2],
      urls: imgs
    })

  },
  // 选择商品属性点击事件
  setChoose: function (e) {
    console.log(this.data.good)
    var pid = e.currentTarget.dataset.pid
    // 商品对应id
    var cid = e.currentTarget.id
    if (pid == 0) {
      this.data.groupindex1 = cid
    }
    else if (pid == 1) {
      this.data.groupindex2 = cid
    }
    else {
      this.data.groupindex3 = cid
    }
    // 获取第一条属性 
    this.data.firstName = ""
    this.data.firstInfo = ""
    if (this.data.groupindex1 > 0) {
      var FirstInfo = this.data.goodsAttrList[0].SpecList.find(f => f.Id == this.data.groupindex1)
      this.data.firstInfo = FirstInfo.SpecName
      this.data.firstName = this.data.goodsAttrList[0].AttrName
    }
    // 获取第二条条属性 
    this.data.secondName = ""
    this.data.secondInfo = ""
    if (this.data.groupindex2 > 0) {
      var SecondInfo = this.data.goodsAttrList[1].SpecList.find(f => f.Id == this.data.groupindex2)
      this.data.secondInfo = SecondInfo.SpecName
      this.data.secondName = this.data.goodsAttrList[1].AttrName
    }
    // 获取第三条属性 
    this.data.thirdName = ""
    this.data.thirdInfo = ""
    if (this.data.groupindex3 > 0) {
      var ThirdInfo = this.data.goodsAttrList[2].SpecList.find(f => f.Id == this.data.groupindex3)
      this.data.thirdInfo = ThirdInfo.SpecName
      this.data.thirdName = this.data.goodsAttrList[2].AttrName
    }

    // 获取商品属性 allgroupindex
    this.data.allgroupindex = ""
    this.data.selectAttrInfo = ""
    if (this.data.groupindex1 > 0) {
      this.data.allgroupindex = this.data.groupindex1 + "_"
      this.data.selectAttrInfo = this.data.firstName + ":" + this.data.firstInfo + "  "
    }
    if (this.data.groupindex2 > 0) {
      this.data.allgroupindex += this.data.groupindex2 + "_"
      this.data.selectAttrInfo += this.data.secondName + ":" + this.data.secondInfo + "  "
    }
    if (this.data.groupindex3 > 0) {
      this.data.allgroupindex += this.data.groupindex3 + "_"
      this.data.selectAttrInfo += this.data.thirdName + ":" + this.data.thirdInfo + "  "
    }
    if (this.data.good.GASDetailList.length > 0) {
      var attrmodel = this.data.good.GASDetailList.find(d => d.id == this.data.allgroupindex)
      if (attrmodel != undefined) {
        this.data.tempInventory = attrmodel.count
        this.data.tempprice = attrmodel.priceStr
        this.data.singleprice = attrmodel.discountPricestr
        this.data.undiscountprice = attrmodel.priceStr
        this.data.singlecount = attrmodel.count
        this.data.discount = attrmodel.discount
        this.data.sumprice = parseFloat(attrmodel.discountPricestr * this.data.buyamount).toFixed(2)
        if (this.data.buyamount > attrmodel.count) {
          this.data.buyamount = attrmodel.count
        }
      }
    }
    this.setData(this.data)
  },
  // 选择商品属性
  setNature: function (e) {
    this.setData({
      // 选择属性框显隐
      setNature: !this.data.setNature,
      setNature1: !this.data.setNature,
    })
  },
  // 添加购物车
  addshoppingCar: function (e) {
    var that = this
    that.setData({
      setNature2: false,
      setNature: true,
      setNature1: true
    })
    return
    var good = that.data.good
    var Array = that.data.Array
    var Name1 = that.data.firstName
    var Name2 = that.data.secondName
    var Name3 = that.data.thirdName
    var Info1 = that.data.firstInfo
    var Info2 = that.data.secondInfo
    var Info3 = that.data.thirdInfo
    // 数量
    var buyNums = that.data.buyamount
    // good下标 
    var attrSpacStr = that.data.allgroupindex
    // goodId
    var goodid = that.data.goodId
    var openid = that.data.openId
    // if (Name1 != '' | Name2 == '' | Name3 == '') {
    //   var SpecInfo = Name1 + ":" + Info1
    // }
    // if (Name1 != '' | Name2 != '' | Name3 == '') {
    //   var SpecInfo = Name1 + ":" + Info1 + " " + Name2 + ":" + Info2
    // }
    // if (Name1 != '' | Name2 != '' | Name3 != '') {
    //   var SpecInfo = Name1 + ":" + Info1 + " " + Name2 + ":" + Info2 + " " + Name3 + ":" + Info3
    // }
    if (Name1 != '') {
      var SpecInfo = Name1 + ":" + Info1
    }
    if (Name2 != '') {
      var SpecInfo = Name1 + ":" + Info1 + " " + Name2 + ":" + Info2
    }
    if (Name3 != '') {
      var SpecInfo = Name1 + ":" + Info1 + " " + Name2 + ":" + Info2 + " " + Name3 + ":" + Info3
    }
    if (Array.goodsAttrList.length > 0 && Array != undefined) {
      if (that.data.selectAttrInfo == '请选择规格属性') {
        if (this.data.setNature) {
          wx.showToast({
            title: '请选择商品规格',
            icon: 'loading'
          })
        }

      }
    }
    {
      // 请求把商品加入购物车
      wx.request({
        url: addr.Address.addGoods,
        data: {
          appid: app.globalData.appid,
          openid: openid,
          goodid: goodid,
          attrSpacStr: attrSpacStr,
          SpecInfo: SpecInfo,
          qty: buyNums
        },
        method: "POST",
        header: {
          'content-type': 'application/json'
        },
        success: function (res) {
          if (res.data.isok == 1) {
            wx.showToast({
              title: '添加成功',
              icon: 'success',
              duration: 2000
            })
            setTimeout(function () {
              that.setData({
                selectAttrInfo: "请选择规格属性",
                // 选择属性框显隐
                setNature: false,
                Info1: '',
              })
            }, 1000)

          } else if (res.data.msg == '数量必须大于0') {
            wx.showToast({
              title: '数量必须大于0',
              icon: 'loading'
            })
          }
          else {
            wx.showToast({
              title: '请填写规格属性值',
              icon: 'loading'
            })
          }
        },
      })
    }
  },
  // 添加购物车
  addshoppingCar1: function (e) {
    var that = this
    var good = that.data.good
    var Array = that.data.Array
    var Name1 = that.data.firstName
    var Name2 = that.data.secondName
    var Name3 = that.data.thirdName
    var Info1 = that.data.firstInfo
    var Info2 = that.data.secondInfo
    var Info3 = that.data.thirdInfo
    // 数量
    var buyNums = that.data.buyamount
    // good下标 
    var attrSpacStr = that.data.allgroupindex
    // goodId
    var goodid = that.data.goodId
    var openid = that.data.openId
    if (Name1 != '') {
      var SpecInfo = Name1 + ":" + Info1
    }
    if (Name2 != '') {
      var SpecInfo = Name1 + ":" + Info1 + " " + Name2 + ":" + Info2
    }
    if (Name3 != '') {
      var SpecInfo = Name1 + ":" + Info1 + " " + Name2 + ":" + Info2 + " " + Name3 + ":" + Info3
    }
    if (Array.goodsAttrList.length > 0 && Array != undefined) {
      if (that.data.selectAttrInfo == '请选择规格属性') {
        if (this.data.setNature) {
          wx.showToast({
            title: '请选择商品规格',
            icon: 'loading'
          })
        }
        that.setData({
          setNature: true,
          setNature1: true,
          setNature2: false
        })
        return
      }
    }
    {
      // 请求把商品加入购物车
      wx.request({
        url: addr.Address.addGoods,
        data: {
          appid: app.globalData.appid,
          openid: openid,
          goodid: goodid,
          attrSpacStr: attrSpacStr,
          SpecInfo: SpecInfo,
          qty: buyNums
        },
        method: "POST",
        header: {
          'content-type': 'application/json'
        },
        success: function (res) {
          if (res.data.isok == 1) {
            wx.showToast({
              title: '添加成功',
              icon: 'success',
              duration: 2000
            })
            setTimeout(function () {
              that.setData({
                selectAttrInfo: that.data.selectAttrInfo,
                // 选择属性框显隐
                setNature: false,
                Info1: '',
              })
            }, 1000)

          } else if (res.data.msg == '数量必须大于0') {
            wx.showToast({
              title: '数量必须大于0',
              icon: 'loading'
            })
          } else {
            wx.showToast({
              title: '请填写规格属性值',
              icon: 'loading'
            })
          }
        },
      })
    }
  },
  // 点击查看详情大图
  previewImage2: function (e) {
    var imageArray = this.data.Array.descImgList2   //声明data数据
    var index = e.currentTarget.id     //声明对应图片的id
    console.log(imageArray)          //后台输出imagesArray先查看结构
    var previewImage = imageArray[index]     //imageArray[index]指定数组里任意一张,用id判断
    console.log(previewImage)
    wx.previewImage({
      current: previewImage, //当前显示图片的http链接
      urls: imageArray //需要预览的图片http链接列表
    })

  },
  formSubmit1: function () {

  },
  // 立即购买
  gotoBuy: function (e) {
    var that = this
    var Array = that.data.Array
    var good = that.data.good
    var Name1 = that.data.firstName
    var Name2 = that.data.secondName
    var Name3 = that.data.thirdName
    var Info1 = that.data.firstInfo
    var Info2 = that.data.secondInfo
    var Info3 = that.data.thirdInfo
    // 数量
    var buyNums = that.data.buyamount
    // good下标 
    var attrSpacStr = that.data.allgroupindex
    // goodId
    var goodid = that.data.goodId
    var openid = that.data.openId
    if (Array.goodsAttrList.length == 1) {
      var SpecInfo = Name1 + ":" + Info1
      if (Info1 == '') {
        wx.showToast({
          title: '请选择规格属性',
          icon: 'loading'
        })
        return
      }
    }
    if (Array.goodsAttrList.length == 2) {
      var SpecInfo = Name1 + ":" + Info1 + " " + Name2 + ":" + Info2
      if (Info1 == '' || Info2 == '') {
        wx.showToast({
          title: '请选择规格属性',
          icon: 'loading'
        })
        return
      }
    }
    if (Array.goodsAttrList.length == 3) {
      var SpecInfo = Name1 + ":" + Info1 + " " + Name2 + ":" + Info2 + " " + Name3 + ":" + Info3
      if (Info1 == '' || Info2 == '' || Info3 == '') {
        wx.showToast({
          title: '请选择规格属性',
          icon: 'loading'
        })
        return
      }
    }
    setTimeout(function () {
      that.setData({
        setNature: false,
      })
    }, 2000)
    this.data.SpecInfo = SpecInfo
    this.data.buyNums = buyNums


    // 提交备用formId,客户手机号码
    var formId = e.detail.formId
    console.log('我是formid', formId)
    util.commitFormId(formId, that)
    that.navToOrderList(SpecInfo, buyNums, goodid, attrSpacStr)
  },
  getPhoneNumber: function (e) {
    var that = this
    app.globalData.telEncryptedData = e.detail.encryptedData
    app.globalData.telIv = e.detail.iv
    app.globalData.isgetTel = 1
    app.getPhoneNumber(function (res) {
      wx.showToast({
        title: '获取成功',
      })
      console.log("getPhoneNumber", res);
      if (res.TelePhone != '未绑定') {
        that.setData({ clientTel: res.TelePhone })
        that.setData({ username: app.globalData.userInfo.nickName, avatarUrl: app.globalData.userInfo.avatarUrl, clientTel: app.globalData.userInfo.TelePhone })
      }
    });

  },
  // 跳转到结算页面
  navToOrderList: function (SpecInfo, buyNums, goodid, attrSpacStr) {
    var that = this
    var item = that.data.good
    var goodCarIdStr = that.data.cardid
    var nowprice = parseFloat(that.data.tempprice).toFixed(2) //对应规格下的原价
    var tempprice = nowprice * that.data.buyNums
    // if (item.GASDetailList.length == 0) {
    //   goodCarIdStr = goodCarIdStr
    // }
    // PriceStr
    var datas = []
    datas.push({ ImgUrl: item.ImgUrl, SpecInfo: that.data.SpecInfo, Introduction: item.Introduction, Price: that.data.singleprice, undiscountPrice: that.data.undiscountprice, discount: item.discount, Count: that.data.buyNums })
    var jsonstr = JSON.stringify(datas)
    if (that.data.buyamount == 0) {
      wx.showToast({
        title: '数量不可为0',
        icon: 'loading'
      })
      return
    }
    wx.navigateTo({
      url: '../orderList/orderList?totalMoney=' + that.data.tempprice * that.data.buyamount + "&datajson=" + jsonstr + "&SpecInfo=" + SpecInfo + "&qty=" + buyNums + "&goodid=" + goodid + "&attrSpacStr=" + attrSpacStr + '&youhui=' + that.data.sumprice
    })
  },

  //立即购买按钮
  nowbuy: function (e) {
    var that = this
    // 提交备用formId
    var formId = e.detail.formId
    console.log('我是formid', formId)
    util.commitFormId(formId, that)
    var buyNums = that.data.buyamount
    var good = that.data.good
    var setNature = that.data.setNature
    var setNature1 = that.data.setNature1
    var setNature2 = that.data.setNature2
    that.setData({
      setNature1: false,
      setNature: true,
      setNature2: true
    })
  },
  // 客服拨打电话
  makePhonecall: function () {
    var phone = app.globalData.customerphone
    if (phone == "") {
      this.GetStoreConfig()
    }
    else {
      wx.makePhoneCall({
        phoneNumber: phone,
      })
    }
  },
  // 点击事件 进入编辑状态后 “+”号 增加商品数量
  valueAdd: function (e) {
    var value = this.data.buyamount
    if (value < this.data.singlecount) {
      value++
    } else {
      value = this.data.singlecount
      wx.showModal({
        title: '提示',
        content: '亲，该商品最多只有这么多了！',
        showCancel: true,
      })
    }
    this.setData({
      buyamount: value,
      allundiscountprice: ((this.data.undiscountprice * value).toFixed(2)),
      sumprice: ((this.data.singleprice * value).toFixed(2)),
    })
  },
  // 点击事件 进入编辑状态后 “-”号 减小商品数量
  valueReduce: function (e) {
    var value = this.data.buyamount
    if (this.data.singlecount > 0) {
      if (value <= 1) {
        value = 1
      } else {
        value = value - 1
      }
      this.setData({
        buyamount: value,
        allundiscountprice: ((this.data.undiscountprice * value).toFixed(2)),
        sumprice: ((this.data.singleprice * value).toFixed(2)),
      })
    } else {
      return
    }
  },
  // input框输入数量
  setValue: function (e) {
    var value = e.detail.value
    if (value <= 1) {
      value = 1
    }
    else if (value >= this.data.singlecount) {
      value = this.data.singlecount
    }
    this.setData({
      buyamount: value,
      sumprice: ((this.data.singleprice * value).toFixed(2)),
    })
  },
  //隐藏
  hiddenShow: function () {
    this.setData({
      setNature: !this.data.setNature
    })
  },
  //更多推荐
  gotogoodlist: function () {
    var url = "../classify/classify"
    app.goBarPage(url)
  },
  // 获取商品列表
  GetGoodsList: function (typeid) {
    var that = this
    wx.request({
      url: addr.Address.getClassify,
      data: {
        appid: app.globalData.appid,
        typeid: typeid,
        pageindex: 1,
        pagesize: 4,
        levelid: app.globalData.levelid
      },
      method: "GET",
      header: {
        'content-type': "application/json"
      },
      success: function (res) {
        if (res.data.isok == 1) {
          var goodslist = res.data.postdata.goodslist
          var goods1 = []
          if (goodslist != undefined && goodslist.length > 0) {
            for (var i = 0; i < goodslist.length; i++) {
              var model = goodslist[i]
              goods1.push({ Id: model.Id, goodImg: model.ImgUrl, goodContent: model.GoodsName, goodPrice: model.discountPricestr, sold: model.salesCount })
            }
          }

          that.setData({
            goods1: goods1,
          })
        }
      },
      fail: function () {
        console.log("获取首页出错")
        wx.showToast({
          title: '获取首页出错',
        })
      }
    },
    )
  },
  // 点击商品跳转到详情页
  GotoGoodSDetail: function (e) {
    // console.log(this.data.goods)
    var id = e.currentTarget.id
    wx.navigateTo({
      url: '../goodList/goodList?id=' + id
    })
  },
  //获取店铺配置
  GetStoreConfig: function () {
    var that = this
    var url = addr.Address.GetStoreConfig
    var param = {
      appid: app.globalData.appid,
    }
    var method = "GET"
    network.requestData(url, param, method, function (e) {
      if (e.data.isok == 1) {
        var phone = e.data.postdata.store.TelePhone
        app.globalData.customerphone = phone
        wx.makePhoneCall({
          phoneNumber: phone,
        })
      }
    })
  },
  loginCheck: function () {
    wx.showModal({
      title: '提示',
      content: '请先登录',
      success: function (res) {
        if (res.confirm) {
          wx.switchTab({
            url: '/pages/me/me',
          })
        }
      }
    })
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function (e) {

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