var util = require('../../utils/util.js')
var address = require('../../utils/address.js')
var net = require('../../utils/net.js')
var app = getApp()
Page({
    data: {},
    onLoad: function (options) {
      app.IsShowBottomLogo(this)
        // 页面初始化 options为页面跳转所带来的参数
        this.request()
    },
   
    request: function (isShowLoading=true) {
        var that = this
        var params = {
            appid: app.globalData.appid,
            level: 6,
            pageSize: 100,
            pageIndex:1,
        }
        if (isShowLoading) {
            util.showLoadingDialog("正在加载...")
        }
        net.POST(address.Address.GET_MODEL_DATA, params, {
            success: function (res, msg) {
                util.hideLoadingDialog()
                util.stopPullDownRefresh()
                if (util.isOptStrNull(res) || util.isOptStrNull(res.data)) {
                    that.setData({
                        canLoadMore: false,
                        errorMsg: msg
                    })
                    return
                }
                console.log(res);
                that.setData({
                    Title: res.data[0].Title,
                    articles: res.data[0].MiniappdevelopmentList,
                    canLoadMore: false,
                    errorMsg: "没有更多数据"
                })
            },
            failure: function (fail) {
                util.hideLoadingDialog()
                util.stopPullDownRefresh()
                console.log(fail);
                that.setData({
                    errorMsg: fail
                })
            }
        })
    },
    onPullDownRefresh: function () {
        this.request(false)
				app.IsShowBottomLogo(this)
				wx.stopPullDownRefresh()
    },
    onShareAppMessage: function () {
      return {
        path: '/pages/develop/develop'
      }
    }
})