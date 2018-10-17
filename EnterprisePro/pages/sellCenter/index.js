// pages/sellCenter/index.js
const app = getApp();
const util = require("../../utils/util.js");
const tools = require("../../utils/tools.js");
const page = require("../../utils/pageRequest.js");
const WxParse = require('../../utils/wxParse/wxParse.js');
Page({

  /**
   * 页面的初始数据
   */
  data: {
    phoneShow: false,
    showBtn: true,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    this.configInfo()
    
  },

  /**
   * 分销首页配置
   * SalesMan:如果为NULL表示没有成为分销员，否则再根据其状态state判断 -1被清退删除了，0待审核，1审核不通过，2审核通过
   */
  configInfo: function () {
    let that = this
    app.getUserInfo(res => {
      page.getSaleConfig().then(data => {
        wx.showLoading({
          title: '加载中',
          mask: true,
          success: function () {
            if (data.isok) {
              let _config = data.obj
              if (_config.RecruitPlan.description != null) {
                _config.content = _config.RecruitPlan.description.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
                _config.content = _config.RecruitPlan.description.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
                _config.content = _config.RecruitPlan.description.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
                _config.content = WxParse.wxParse('content', 'html', _config.RecruitPlan.description, that, 5)
                that.setData({ vm: _config, })
              }
              else {
                wx.showModal({
                  title: '提示',
                  content: "未配置页面",
                  showCancel: false,
                  success: function (res) {
                    if (res.confirm) {
                      tools.goBackPage(1)
                    }
                  }
                })
              }
              if (_config.SalesManManager.allow_recruit == 0) {
                that.setData({ "showBtn": false })
                wx.hideLoading()
                return;
              }
              if (_config.SalesMan == null) {
                _config.showBtn = true
                _config.text = "成为分销员"
              } 
              else {
                let state = _config.SalesMan.state
                switch (state) {
                  case -1:
                    wx.redirectTo({ url: '../sellCenter/person' })
                    break;
                  case 0:
                    _config.text = "待审核"
                    _config.showBtn = false
                    break;
                  case 1:
                    _config.text = "待审核"
                    _config.showBtn = false
                    break;
                  case 2:
                    wx.redirectTo({ url: '../sellCenter/person' })
                    app.globalData.salesManId = _config.SalesMan.Id
                    wx.setStorageSync("salesManId", _config.SalesMan.Id)
                    break;
                }
              }
              that.setData({ vm: _config, })
            }
            else {
              if (data.msg = "还未进行分销设置") {
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
            }
            wx.hideLoading();
          },
        })

      })
      util.setPageSkin(that);
    })
  },
  // 申请分销事件
  getApply: function (e) {
    let that = this
    let status = e.currentTarget.dataset.s
    let phoneShow = that.data.phoneShow
    if (status) {
      wx.showModal({
        title: '申请成为分销员',
        content: "申请提交后，商家将会进行审核，审核通过后即可成为本店的分销员",
        success: res => {
          if (res.confirm) {
            phoneShow = true
          } else {
            phoneShow = false
          }
          that.setData({ phoneShow, })
        }
      })
    }
  },
  // 获取输入号码
  getInput: function (e) {
    this.data.phoneNumber = e.detail.value
  },
  // 电话点击事件
  phoneClick: function (e) {
    let id = Number(e.target.id)
    switch (id) {
      case 0:
        this.setData({ phoneShow: false })
        break;
      case 1:
        page.postApply(this.data.phoneNumber).then(data => {
          if (data.isok) {
            tools.showToast(data.msg)
            this.setData({ phoneShow: false })
            this.configInfo()
          } else {
            tools.ShowMsg(data.msg)
          }
        })
        break;
    }

  },
  onPullDownRefresh() {
    this.configInfo()
    wx.stopPullDownRefresh();
  }
})