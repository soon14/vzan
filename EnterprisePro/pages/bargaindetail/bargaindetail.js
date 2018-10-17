// pages/bargaindetail/bargaindetail.js
const addr = require("../../utils/addr.js");
const util = require("../../utils/util.js");
const http = require('../../utils/http.js');
const page = require("../../utils/pageRequest.js")
const tools = require("../../utils/tools.js")
const app = getApp()
import { core } from "../../utils/core.js";
Page({

  /**
   * 页面的初始数据
   */
  data: {
    haveCreatOrder: false,//用户是否下过单
    homeClose: true,
    username: '',//用作显示自砍圆圈名字
    avatarUrl: '',//用作显示自砍圆圈头像
    BargainedUserName: '',//帮砍人名字
    qrcode: '',//点击帮砍生成的带参二维码
    buid: 0,//砍价商品领取记录Id
    buid1: 0,//帮砍砍价商品领取记录Id
    isCut: 0,//是否从其他页面直接点击砍价 0否 1真
    nowTime: 0,//现实时间的实时毫秒数
    cutprice: 0,//砍价完成的随机价格
    scrollTop: 0,//页面元素滚动高度
    singleprice: 0,//进入页面时当前价格
    obj: [],//返回obj
    DescImgList: [],//obj级下
    ImgList: [],//obj级下
    Id: 0,//商品id
    isFriend: 0,//判断是否带参，(好友) 0是自砍 1是好友
    condition0: false, //帮好友砍价成功弹窗
    condition1: false,//自砍成功弹窗
    condition2: false,//邀请朋友砍刀弹窗
    condition0_1: 0,//帮砍开始砍价按钮隐藏
    condition0_2: 0,//自砍邀请好友按钮
    choose: 0,
    isTimeto: 1,//自砍倒计时
    imgUrls: [ // 砍价描述图片
      'http://img02.tooopen.com/images/20150928/tooopen_sy_143912755726.jpg',
      'http://img06.tooopen.com/images/20160818/tooopen_sy_175866434296.jpg',
      'http://img06.tooopen.com/images/20160818/tooopen_sy_175833047715.jpg'
    ],
    item: [ //砍价排行返回的内容
      { userimg: '/image/test.png', username: '吴亦凡', condition: '砍至最低', payed: false },
      { userimg: '/image/test.png', username: '吴亦凡', condition: '32.56', payed: true },
      { userimg: '/image/test.png', username: '吴亦凡', condition: '8.36', payed: true },
      { userimg: '/image/test.png', username: '吴亦凡', condition: '8.36', payed: false },
    ],
    imSwitch:false,
  },
  a: function () {
    return
  },
  // 点击查看大图
  previewImageAPI: function (e) {
    let img = e.currentTarget.dataset.img
    let imgUrls = []
    imgUrls.push(img)
    util.preViewShow(img, imgUrls)
  },
  // 砍价描述 砍价排行选择km
  changeChoose: function (e) {
    var index = e.currentTarget.id
    this.setData({ choose: index })
  },
  // 帮好友砍价
  beginBargain: function () {
    var that = this
    if (that.data.isFriend == 1) {
      var buid = that.data.buid1
    } else {
      var buid = that.data.buid
    }
    that.cutpriceRequest(buid, 1)
    // this.setData({ condition0: !this.data.condition0 })
  },
  // 自砍 !
  bargainMyself: function () {
    var that = this
    if (that.data.haveCreatOrder == true) {
      wx.showModal({
        title: '提示',
        content: '您已经下过单了，请进入砍价单查看详情！',
        confirmText: '去看看',
        showCancel: false,
        success: function (res) {
          if (res.confirm) {
            tools.goNewPage('../mycutprice/mycutprice')
          }
        }
      })
    } else {
      that.AddBargainRequest(that.data.Id, app.globalData.userInfo.nickName, 0)
    }
  },
  // 我也要玩
  bargainMyself0: function () {
    var that = this
    this.setData({ isFriend: 0 })
  },
  // 帮好友砍价取消砍价叉叉
  cancelBargain: function () {
    this.setData({ condition0: !this.data.condition0, condition0_1: 1 })
  },
  // 自砍取消叉叉
  hiddenCondition0: function () {
    this.setData({ condition1: !this.data.condition1 })
  },
  // 不帮好友砍价点击 “我也要玩” 按钮 
  changeCondition_1: function () {
    this.setData({ condition0: !this.data.condition0 })
  },
  // 自砍点击 “请好友帮看一刀” 按钮
  changeCondition_2: function () {
    this.setData({ condition1: !this.data.condition1, condition0_2: 1 })
  },
  //!
  inviteBargain1: function (e) {
    var index = e.currentTarget.id
    var that = this
    that.inite3(index, 1)
  },
  //!
  inviteBargain: function (e) {
    var index = e.currentTarget.id
    var that = this
    if (that.data.haveCreatOrder == true) {
      wx.showModal({
        title: '提示',
        content: '您已经下过单了，请进入砍价单查看详情！',
        confirmText: '去看看',
        showCancel: false,
        success: function (res) {
          if (res.confirm) {
            tools.goNewPage('../mycutprice/mycutprice')
          }
        }
      })
    } else {
      that.inite3(index, 0)
    }

  },
  // 取消好友帮砍弹窗叉叉
  cancelMyselfBargain: function () {
    this.setData({ condition2: !this.data.condition2, })
  },
  // 自砍进入倒计时的按钮提示
  bargainTimeload: function () {
    tools.ShowMsg('自砍倒计时中！')
  },
  // 活动已结束
  bargainTimeout: function (e) {
    tools.ShowMsg('活动已结束！')
  },
  // 商品已售罄
  bargainSoldout: function () {
    tools.ShowMsg('亲，该商品已售罄！')
  },
  // 活动未开始
  bargainTimeunout: function () {
    tools.ShowMsg('莫慌，活动未开始！')
  },
  // 跳转到砍价列表
  navtoMycutprice: function () {
    tools.goNewPage('../mycutprice/mycutprice')
  },
  // 返回主页
  pagesGoto: function (e) {
    tools.goLaunch('../index/index')
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    if (options.scene) {
      // 判断是否从带参二维码进入
      var sceneArray = decodeURIComponent(options.scene)
      if (sceneArray != undefined) {
        var sceneArray = sceneArray.split("_")
        that.data.buid1 = sceneArray[0]
        that.setData({
          // buid1: sceneArray[0],
          isFriend: sceneArray[2]
        })
      }
    }
    if (options.Id == undefined) {
      var Id = sceneArray[1]
      var isCut = 0 //好友砍
    } else {
      var Id = options.Id
      var isCut = 1 //自砍
    }
    that.data.isCut = isCut
    // that.setData({ isCut: isCut })
    if (app.globalData.userInfo.openId == undefined || app.globalData.userInfo.openId == "") {
      app.getUserInfo(function (e) {
        that.barDetailRequest(Id)
        that.data.Id = Id
        that.setData({
          // Id: Id,
          username: app.globalData.userInfo.nickName,
          avatarUrl: app.globalData.userInfo.avatarUrl
        })
      })
    } else {
      that.barDetailRequest(Id)
      that.data.Id = Id
      that.setData({
        // Id: Id,
        username: app.globalData.userInfo.nickName,
        avatarUrl: app.globalData.userInfo.avatarUrl
      })
    }
    
    getApp().GetStoreConfig(function (config) {
      if (config && config.funJoinModel) {
        that.setData({
          imSwitch: config.funJoinModel.imSwitch && config.kfInfo
        });
      }
    });
    core.GetPageSetting().then(function (pageset) {
      
      util.setPageSkin(that);
    });
  },
  // 圆圈进度条
  canvasCircle: function (a, b) {
    var cxt_arc = wx.createCanvasContext('canvasArc');
    cxt_arc.setLineWidth(2);
    cxt_arc.setStrokeStyle('#f7f7f7');
    cxt_arc.setLineCap('round')
    cxt_arc.beginPath();
    cxt_arc.arc(64, 64, 60, 0, 2 * Math.PI, false);
    cxt_arc.stroke();
    cxt_arc.setLineWidth(2);
    cxt_arc.setStrokeStyle('#f20033');
    cxt_arc.setLineCap('round')
    cxt_arc.beginPath();
    cxt_arc.arc(64, 64, 60, 1.5 * Math.PI, 2 * Math.PI * (a / b) + 1.5 * Math.PI, false);
    cxt_arc.stroke();
    cxt_arc.draw();
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {
    var that = this
    setInterval(function () { // 倒计时
      that.setData({
        nowTime: new Date().getTime()
      })
    }, 1000)
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
    this.data.obj = []
    this.barDetailRequest(this.data.Id)
    tools.showToast('刷新成功')
    wx.stopPullDownRefresh()
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },
  onPageScroll: function (e) {
    var that = this
    var scrollTop = e.scrollTop
    that.setData({ scrollTop: scrollTop })
  },
  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {
    return {
      title: '最低' + this.data.obj.FloorPriceStr + '元，原价' + this.data.obj.OriginalPriceStr + '元，等你来砍，能砍多少看你本事了',
      imageUrl: this.data.obj.ImgUrl,
      path: '/pages/bargaindetail/bargaindetail?scene=' + this.data.buid + '_' + this.data.Id + '_' + 1
    }
		console.log(this.data.buid + '_' + this.data.Id + '_' + 1)
  },
  // 砍价详情
  barDetailRequest: function (Id) {
    let that = this
    http.getAsync(
      addr.Address.GetBargain,
      {
        appid: app.globalData.appid,
        Id: Id,
        UserId: app.globalData.userInfo.UserId
      })
      .then(function (data) {
        if (data.isok) {
          let [date, obj, singleprice, BargainUserList, BargainRecordUserList, buid] = [new Date(), data.obj, 0, data.obj.BargainUserList, data.obj.BargainRecordUserList, 0]
          let findBuid = BargainRecordUserList.find(b => b.BargainUserId = app.globalData.userInfo.UserId)
          let nowtime = date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + date.getDate() + " " + date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds()
          findBuid ? buid = findBuid.BUId : buid = 0
          for (var j = 0, val; val = BargainRecordUserList[j++];) {
            if (val.BargainUserId == app.globalData.userInfo.UserId) {
              var buid = val.BUId
              break
            }
          }
          for (var i = 0, valKey; valKey = BargainUserList[i++];) {
            if (valKey.UserId == app.globalData.userInfo.UserId && valKey.Id == buid) {
              singleprice = valKey.CurrentPriceStr
              break
            }
          }
          obj.EndDate = that.ChangeDateFormat(obj.EndDate)
          obj.StartDate = that.ChangeDateFormat(obj.StartDate)
          that.data.isCut == 0 ? that.beginBargain() : "";
          that.data.haveCreatOrder = data.haveCreatOrder
          //that.canvasCircle(obj.OriginalPriceStr - singleprice, obj.OriginalPriceStr)//圆圈进度条
          var percent = ((Number(obj.OriginalPriceStr) - singleprice) / Number(obj.OriginalPriceStr)) * 100;
          if (percent > 100)
            percent = 100;
          that.setData({
            buid: buid,
            obj: obj,
            DescImgList: data.obj.DescImgList,
            ImgList: data.obj.ImgList,
            BargainUserList: BargainUserList,
            singleprice: singleprice,
            percent: percent,

          })
        } else {
          wx.showModal({
            title: '提示',
            content: data.msg,
            showCancel: false,
            success: function (res) {
              if (res.confirm) {
                tools.goBackPage(1)
              }
            }
          })
        }
      })

  },
  // 申请砍价
  AddBargainRequest: function (Id, UserName, isbuy) {
    let that = this
    http.postAsync(
      addr.Address.AddBargainUser,
      {
        Id: Id,
        UserId: app.globalData.userInfo.UserId,
        UserName: UserName
      }).then(function (data) {
        if (data.isok) {
          if (isbuy == 0) { //自砍
            // var buid = data.buid
            // that.setData({ buid: data.buid })
            that.cutpriceRequest(data.buid, 0)
          }
          if (isbuy == 1) {
            // var buid = data.buid
            that.getBargainRequest(data.buid)
          }
        }
      })
  },
  // 开始砍价
  cutpriceRequest: function (buid, isFriend) {
    var that = this
    var condition0 = that.data.condition0
    http.postAsync(
      addr.Address.cutprice,
      {
        UserId: app.globalData.userInfo.UserId,
        buid: buid
      })
      .then(function (data) {
        if (isFriend == 1) {
          condition0 = true
        } else {
          condition0 = false
        }
        if (data.msg == '请求成功') {
          var record = data.record
          that.setData({
            condition1: !that.data.condition1,
            cutprice: data.cutprice,
            isCut: 1,
            condition0: condition0,
            BargainedUserName: data.BargainedUserName
          })
          that.barDetailRequest(that.data.Id)
        }
        else if (data.msg == '您已经帮他砍过了，刷新试试') {
          tools.showLoadToast("亲，已经砍过啦")
          that.setData({ condition0_1: 1, })
        }
        else if (data.msg == '已砍至底价！') {
          if (that.data.isFriend == 1) {
            wx.showModal({
              title: '提示',
              content: '您朋友的商品已经砍至最低价！',
              showCancel: false,
              success: function (res) {
                if (res.confirm) {
                  that.setData({ condition0_1: 1 })
                }
              }
            })
          }
          else {
            tools.showLoadToast("已砍至最低价")
          }
        }
        else if (data.msg == '已下单不能再砍') {
          if (that.data.isFriend == 1) {
            wx.showModal({
              title: '提示',
              content: '您的朋友已下单了，不能帮砍了哦',
              showCancel: false,
              success: function (res) {
                if (res.confirm) {
                  that.setData({ condition0_1: 1 })
                }
              }
            })
          }
          else {
            tools.showLoadToast("已下单不能再砍")
          }
        }
        else if (data.msg == '已帮他砍过了') {
          tools.showLoadToast("您已帮他砍过了")
          that.setData({ isFriend: 0 })
        }
        else if (data.msg == '砍价状态有误！') {
          tools.showLoadToast("砍价状态有误")
        }
        else if (data.msg == '商品已售罄！') {
          wx.showModal({
            title: '提示',
            content: '亲，该商品已售罄！请点击确认返回首页',
            showCancel: false,
            confirmText: '确认',
            success: function (res) {
              if (res.confirm) {
                wx.switchTab({
                  url: '../index/index',
                })
              }
            }
          })
        }
        else {
          var timeArray = []
          if (data.obj == 0) {
            var content = '已自砍成功,自砍倒计时1分钟！'
          } else {
            timeArray = JSON.stringify(data.obj).split(".")
            var mintues = parseInt(parseInt(timeArray[1]) * 0.6)
            if (timeArray.length == 2 && parseInt(timeArray[0]) != 0) {
              var content = '已自砍成功,' + timeArray[0] + '小时' + mintues + '分钟' + '之后才能继续自砍'
            } else if (timeArray.length == 2 && parseInt(timeArray[0]) == 0) {
              var content = '已自砍成功,' + mintues + '分钟' + '之后才能继续自砍'
            } else {
              var content = '已自砍成功,' + timeArray[0] + '小时' + '之后才能继续自砍'
            }
          }
          wx.showModal({
            title: '提示',
            content: content,
            showCancel: false,
            success: function (res) {
              if (res.confirm) {
                that.setData({ isFriend: 0 })
              }
            }
          })
        }
      })

  },
  ChangeDateFormat: function (val) {
    if (val != null) {
      var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", "").replace("-", "/"), 10));
      //月份为0-11，所以+1，月份小于10时补个0
      var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
      var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
      var hour = date.getHours();
      var minute = date.getMinutes();
      var second = date.getSeconds();
      var dd = date.getFullYear() + "/" + month + "/" + currentDate + " " + hour + ":" + minute + ":" + second;
      // console.log(dd)
      return dd;
    }
    return "";
  },

  // 请求二维码
  inite3: function (bId, isCondition) {
    var that = this
    console.log("进入请求二维码")
    wx.request({
      url: addr.Address.GetShareCutPrice, //仅为示例，并非真实的接口地址
      data: {
        UserId: app.globalData.userInfo.UserId,
        buid: that.data.buid,
        appId: app.globalData.appid,
        bId: bId,
      },
      method: "POST",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function (data) {
        var isCut = that.data.isCut
        if (data.data.isok) {
          if (isCondition == 1) {
            that.setData({ condition2: !that.data.condition2, condition1: !that.data.condition1, qrcode: data.data.qrcode })
          } else {
            that.setData({ condition2: !that.data.condition2, qrcode: data.data.qrcode })
          }

          wx.downloadFile({
            url: data.data.qrcode.replace(/http/, "https"), //下载二维码图片
            success: function (res0) {
              // 下载大图
              wx.downloadFile({
                url: that.data.obj.ImgUrl_thumb.replace(/http/, "https"), //下载砍价商品大图
                success: function (res) {
                  var windowWidth = wx.getSystemInfoSync().windowWidth //屏幕宽度
                  var windowHeight = wx.getSystemInfoSync().windowHeight //屏幕高度
                  var context = wx.createCanvasContext('firstCanvas')
                  var bgImg = '/image/a38.png'
                  var objImgUrl = res.tempFilePath
                  var code = res0.tempFilePath //先下载二维码 返回的tempFilePath用作canvas拼图（只能本地图片）
                  var bottomprice = '最低' + that.data.obj.FloorPriceStr + '元，原价' + that.data.obj.OriginalPriceStr + '元，等你来砍'
                  var bottomprice1 = '能砍多少看你本事了'
                  context.drawImage(bgImg, 0, 0, windowWidth * 0.9, windowHeight * 0.75); //大背景图
                  context.drawImage(objImgUrl, windowWidth * 0.1, windowHeight * 0.1, windowWidth * 0.70, windowHeight * 0.19); //商品大图
                  context.drawImage(code, windowWidth * 0.18, windowHeight * 0.45, windowWidth * 0.25, windowHeight * 0.17); //二维码
                  context.setFontSize(13)
                  context.setFillStyle('#fbb47b')
                  context.fillText(bottomprice, windowWidth * 0.17, windowHeight * 0.42) //第一行文字
                  context.fillText(bottomprice1, windowWidth * 0.29, windowHeight * 0.45) //第二行文字
                  context.draw()
                }
              })
            }
          })
        }
      },
      fail: function () {
        tools.showToast("获取信息出错")
      }
    })
  },
  // 现价购买按钮
  buybynowPrice: function (e) {
    var that = this
    // 提交备用formId
    var formId = e.detail.formId
    util.commitFormId(formId, that)
    var index = e.currentTarget.id
    that.AddBargainRequest(index, app.globalData.userInfo.nickName, 1)
  },

  //现价购买
  getBargainRequest: function (buid) {
    let that = this
    http.postAsync(
      addr.Address.GetBargainUser,
      {
        buid: buid,
        userid: app.globalData.userInfo.UserId
      })
      .then(function (data) {
        if (data.isok) {
          tools.goNewPage('../orderListCutprice/orderListCutprice?BName=' + data.obj.BName + '&Freight=' + data.obj.Freight + '&ImgUrl=' + data.obj.ImgUrl + '&curPrcie=' + data.obj.curPrcie + '&buid=' + buid)
        } else {
          wx.showModal({
            title: '提示',
            content: '您已经下过单了，请进入砍价单查看详情！',
            confirmText: '去看看',
            showCancel: false,
            success: function (res) {
              if (res.confirm) {
                tools.goNewPage('../mycutprice/mycutprice')
              }
            }
          })
        }
      })
  },

  // 保存画布的图片
  canvasToTempFilePath: function (e) {
    wx.canvasToTempFilePath({
      x: 0,
      y: 0,
      width: wx.getSystemInfoSync().windowWidth * 0.9,
      height: wx.getSystemInfoSync().windowHeight * 0.75,
      destWidth: 650,
      destHeight: 880,
      canvasId: 'firstCanvas',
      success: function (res) {
        wx.saveImageToPhotosAlbum({
          filePath: res.tempFilePath,
          success(res) {
            if (e.currentTarget.id == 0) {
              wx.showToast({
                title: '图片保存成功',
              })
            }
            if (e.currentTarget.id == 1) {
              wx.showModal({
                title: '提示',
                content: '保存已保存成功！您可以用该图片去分享朋友圈哦',
                showCancel: false
              })
            }
          }
        })
      }
    })
  },
  gochat: function () {
    tools.gochat();
  }
})