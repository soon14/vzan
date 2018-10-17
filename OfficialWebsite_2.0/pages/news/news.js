var net = require('../../utils/net');
var address = require('../../utils/address');
var util = require('../../utils/util');
var dateFormat = require('./DateFormat');
var app = getApp();
Page({
    data: {articles: [], currentPageIndex: 1, canLoadMore: true, errorMsg: null},
    onLoad: function (options) {
      app.IsShowBottomLogo(this)
        // 页面初始化 options为页面跳转所带来的参数
        console.log(options);
        this.loadDataFormNet()
    },
    navo_webview: function () {
        wx.navigateTo({
            url: '../web_view/web_view?id=' + this.data.AgentConfig.QrcodeId
        })
    },
   
    onPullDownRefresh: function () {
        this.setData({
            currentPageIndex: 1, canLoadMore: true, errorMsg: null
        })
        this.loadDataFormNet(false)
				app.IsShowBottomLogo(this)
				wx.stopPullDownRefresh()
    },
    loadDataFormNet: function (isShowLoading=true) {
        var that = this
        var params = {
            appid: app.globalData.appid,
            level: 8,
            pageIndex: that.data.currentPageIndex
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
                var data = that.data.currentPageIndex == 1 ? [] : that.data.articles
                for (var i = 0; i < res.data.length; i++) {
                    var item = res.data[i];
                    item.Lastdate = dateFormat.dateParse(item.Lastdate);
                    item.Createdate = dateFormat.dateParse(item.Createdate);
                }
                that.setData({
                    articles: data.concat(res.data),
                    currentPageIndex: that.data.currentPageIndex + 1
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
    onReachBottom: function () {
        if (this.data.canLoadMore) {
            this.loadDataFormNet()
        }
    },
    onItemClick: function (e) {
        var item = this.data.articles[e.currentTarget.dataset.index]
        var imgUrl = item.ImgUrl || item.LitleImgUrl || '/images/ic_error_state.png'
        var date = item.Createdate||item.Lastdate
        wx.navigateTo({
            url: '../detail/detail?id=' + item.Id 
            + '&imgUrl_=' + imgUrl 
            + '&title_=' + item.Title
            + '&content_=' + item.Content
            + '&date_=' + date
        })
    },
    onShareAppMessage: function () {
      return {
        path: '/pages/news/news'
      }
    }
})