// pages/store/applyEnter/applyEnter.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    show_applys_modal: false,
    isbind: false,
    fuserId: 0,

    xieyi_1: true,
    xieyi_2: true
  },
  choose_xieyi: function(e) {
    var id = e.currentTarget.id
    if (id == 0) {
      this.setData({
        xieyi_1: !this.data.xieyi_1
      })
    } else {
      this.setData({
        xieyi_2: !this.data.xieyi_2
      })
    }
  },
  applyenter: function(_u) { //申请入驻
    var that = this
    if (that.data.userInfo.IsValidTelePhone == 0) {
      wx.showModal({
        title: '提示',
        content: '请先同意授权手机号码',
        showCancel: false,
        confirmText: '好的'
      })
      return
    }
    if (!that.data.xieyi_1 || !that.data.xieyi_2) {
      wx.showModal({
        title: '提示',
        content: '请先阅读并勾选同意《拼享惠商家入驻协议》和《拼享惠代理商协议》',
        showCancel: false,
        confirmText: '好的'
      })
      return
    }
    var g = getApp().globalData;
    app.globalData.userInfo = null;
    app.login(function(g) {
      that.setData({
        userInfo: g.userInfo
      })
      http.pRequest(addr.ApplyIn, {
        fuserId: that.data.fuserId,
        aId: g.aid
      }, function(callback) {
        if (callback.data.obj > 0) {
          that.setData({
            show_applys_modal: true
          })
          that.data.storeid = callback.data.obj
        }
      })
    })
  },
  check_xieyi: function(e) {
    var id = e.currentTarget.id
    if (id == 0) {
      var url = 'https://wtapi.vzan.com/dist/shangjiaruzhu.docx'
    } else {
      var url = 'https://wtapi.vzan.com/dist/dailixieyi.docx'
    }
    wx.showLoading({
      title: '页面加载中...',
    })
    wx.downloadFile({
      url: url,
      success: function(res) {
        var filePath = res.tempFilePath
        wx.hideLoading()
        wx.openDocument({
          filePath: filePath,
        })
      }
    })
  },
  my_nt: function() {
    template.switchtab('/pages/my/my')
  },
  hidden_apply_modal: function() {
    this.setData({
      show_applys_modal: false
    })
  },
  getphonenumber: function(e) { //绑定手机号码
    var that = this;
    var _e = e.detail;
    var g = getApp().globalData;
    _e.signature = g.userInfo.UserId
    console.log(_e)
    if (_e.encryptedData && _e.iv) {
      wx.login({
        success: function(res) {
          app.loginByThirdPlatform(res.code, _e.encryptedData, _e.signature, _e.iv, function(cb) {
            if (cb) {
              app.login(function() {
                that.setData({
                  userInfo: g.userInfo
                })
              })
            }
          }, 1)
        }
      });
    } else {
      //拒绝授权
      template.gonewpage("/pages/my/bindphone/bindphone");
    }
  },
  addAgent_gopay: function() {
    var that = this
    if (that.data.userInfo.IsValidTelePhone == 0) {
      wx.showModal({
        title: '提示',
        content: '请先同意授权手机号码',
        showCancel: false,
        confirmText: '好的'
      })
      return
    }
    if (!that.data.xieyi_1 || !that.data.xieyi_2) {
      wx.showModal({
        title: '提示',
        content: '请先阅读并勾选同意《拼享惠商家入驻协议》和《拼享惠代理商协议》',
        showCancel: false,
        confirmText: '好的'
      })
      return
    }
    if (that.data.agent_peizhi != null) {
      wx.showModal({
        title: '提示',
        content: '您已经是代理商了，可以返回个人中心从‘推广代理’入口中查看代理详情',
        confirmText: '个人中心',
        success: function(res) {
          if (res.confirm) {
            template.switchtab('/pages/my/my')
          }
        }
      })
      return
    }
    http.pRequest(addr.PayAgent, {
      aid: app.globalData.aid,
      fuserId: that.data.fuserId
    }, function(callback) {
      http.PayOrderNew(callback.data.obj.cityMordersId, function(callback) {
        if (callback == -1) {
          wx.showModal({
            title: '提示',
            content: '尊敬的用户，您取消了支付！',
            confirmText: '返回首页',
            success: function(res) {
              if (res.confirm) {
                template.switchtab('/pages/index/index')
              }
            }
          })
        } else {
          wx.showModal({
            title: '提示',
            content: '开通成功！可以返回个人中心从‘推广代理’入口中查看代理详情。',
            confirmText: '个人中心',
            showCancel: false,
            success(res) {
              if (res.confirm) {
                template.switchtab('/pages/my/my')
              }
            }
          })
        }
      })
    })
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    this.data.fuserId = options.fuserId || options.scene || 0
    if (this.data.fuserId != 0) { //兼容初版生成的邀请二维码
      if (this.data.fuserId.substring(1, 2) == '_') {
        this.data.fuserId = this.data.fuserId.split('_')[1]
      }
    }
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
    var that = this
    app.globalData.userInfo = null;
    app.login(function(g) {
      console.log(g)
      var data = {}
      data.aid = app.globalData.aid
      data.storeId = g.userInfo.StoreId
      http.gRequest(addr.PlatFormInfo, data, function(callback) {
        that.setData({
          dianpu_peizhi: callback.data.obj
        })
      })
      //绑定手机页面，绑定成功isbind为true，当前页面绑定手机则直接请求；isInvite 0申请入驻 1开通代理
      if (g.userInfo.IsValidTelePhone == 1 && that.data.isbind && that.data.isInvite == 0) {
        that.applyenter(g.userInfo)
      }
      that.setData({
        userInfo: g.userInfo
      })

      http.gRequest(addr.IsAgent, {}, function(callback) { //判断当前用户是否已是代理
        that.setData({
          agent_peizhi: callback.data.obj.agent
        })
      })

      http.gRequest(addr.GetFuserId, { //查询fuserid
        aid: app.globalData.aid,
        fuserId: that.data.fuserId
      }, function(callback) {
        that.data.fuserId = callback.data.obj.fuserId
        if (that.data.fuserId != 0) { //根据fuserid查询用户名字，用作页面显示
          http.gRequest(addr.GetAgentInfo, {
            userId: that.data.fuserId
          }, function(callback) {
            that.setData({
              title: callback.data.obj.userInfo.NickName
            })
          })
        }
      })

    })
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
  onShareAppMessage: function() {
    var that = this
    var shareTitle = '门店业绩想翻倍，就用拼享惠'
    var shareDesc = '申请加入拼享惠'
    var fuserId = (that.data.fuserId > 0 ? that.data.fuserId : that.data.userInfo.UserId)
    var sharePath = 'pages/store/applyEnter/applyEnter?fuserId=' + fuserId
    var imageUrl = 'http://i.vzan.cc/image/jpg/2018/7/23/15330231288f2fec92411099489b69dee49db7.jpg'
    return {
      title: shareTitle,
      desc: shareDesc,
      path: sharePath,
      imageUrl: imageUrl,
      success: function() {
        console.log(sharePath)
      }
    }
  },
})