// pages/book/book.js
var template = require("../../template/template.js")
var tool = require("../../template/Pediluvium_Client.js")
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
  data: {
    Tsex: 0,// 0：全部，1：男，2：女（默认值：0）
    Tage: 0,// 0：不排序， 1：升序， 2：降序（默认值：0）
    Tcount: 0,// 0：不排序， 1：升序， 2：降序（默认值：0）

    Stype: 0,//0：全部（默认值：0）
    Sprice: 0,//0：不排序， 1：升序， 2：降序（默认值：0）
    Scount: 0,//0：不排序， 1：升序， 2：降序（默认值：0）

    pageIndex: 1,
    pageIndexS: 1,
    // 轮播图属性
    indicatorDots: true,
    autoplay: true,
    interval: 5000,
    duration: 1000,
    typeChoose: 0, //0选技师 1选项目
    chooseProject: 0,
    chooseSex: ['不限性别', '男', '女'],  //选性别条件表
    typeList: [], //选项目条件表
  },
	nato_webview: function () {
		wx.navigateTo({
			url: '/pages/me/web_view?id=' + this.data.AgentConfig.QrcodeId,
		})
	},
  navtohome: function () {
    template.goNewPage('../home/home')
  },
  previewImg: function (e) {
    tool.previewStoreimg(e, this.data.data.photoList)
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this
    app.getUserInfo(function (e) {
      template.GetVipInfo(that, 1, function (e) {
        if (e = 'success') {
          tool.GetStoreInfo(that)
          tool.GetTechnicianList(that, 1, 6, 0, 0, 0, 0)
          tool.GetServiceList(that, 1, 6, 0, 0, 0, 0)
        }
      })
      template.GetAgentConfigInfo(that)
      that.setData({ userInfo: e })
    })
  },

  setChoose: function (e) {
    this.setData({ typeChoose: e.currentTarget.id })
  },

  artificerInfo: function () {
    template.goNewPage('../artificerInfo/artificerInfo')
  },

  projectinfo: function (e) {
    template.goNewPage('../projectinfo/projectinfo?id=' + e.currentTarget.id)
  },
  goNewPage: function (e) {
    template.goNewPage('../artificerInfo/artificerInfo?id=' + e.currentTarget.id)
  },
  clickToChat: function (e) {
    var ds = e.currentTarget.dataset;
    var userid = ds.userid;
    var nickname = (ds.nickname || "").replace(/\s/gi, "");
    var headimg = ds.headimg;
    wx.navigateTo({
      url: '../im/chat?userid=' + userid + "&nickname=" + nickname + "&headimg=" + headimg,
    })
  },
  getLogin: function (e) {
    let that = this
    let _g = e.detail
    if (_g.errMsg == "getUserInfo:fail auth deny") {
      return;
    }
    wx.login({
      success: function (res) {
        let vm = {
          iv: _g.iv,
          code: res.code,
          data: _g.encryptedData,
          signature: _g.signature,
          isphonedata: 0,
        }
        app.login_old(vm, function (data) {
          that.setData({ userInfo: data })
        })
      }
    })
  },

  // 选技师筛选
  orderby_Tsex: function (e) {
    this.setData({ Tsex: e.detail.value })
    tool.GetTechnicianList(this, 1, 6, this.data.Tsex, this.data.Tage, this.data.Tcount, 0)
  },
  orderby_Tage: function () {
    if (this.data.Tage == 0) {
      this.setData({ Tage: 1 })
    } else if (this.data.Tage == 1) {
      this.setData({ Tage: 2 })
    } else {
      this.setData({ Tage: 0 })
    }
    tool.GetTechnicianList(this, 1, 6, this.data.Tsex, this.data.Tage, this.data.Tcount, 0)
  },
  orderby_Tcount: function () {
    if (this.data.Tcount == 0) {
      this.setData({ Tcount: 1 })
    } else if (this.data.Tcount == 1) {
      this.setData({ Tcount: 2 })
    } else {
      this.setData({ Tcount: 0 })
    }
    tool.GetTechnicianList(this, 1, 6, this.data.Tsex, this.data.Tage, this.data.Tcount, 0)
  },
  // 选项目筛选
  orderby_Stype: function (e) {
    this.setData({ chooseProject: e.detail.value, Stype: this.data.typeList[e.detail.value].id })
    tool.GetServiceList(this, 1, 6, this.data.Stype, this.data.Sprice, this.data.Scount, 0)
  },
  orderby_Sprice: function () {
    if (this.data.Sprice == 0) {
      this.setData({ Sprice: 1 })
    } else if (this.data.Sprice == 1) {
      this.setData({ Sprice: 2 })
    } else {
      this.setData({ Sprice: 0 })
    }
    tool.GetServiceList(this, 1, 6, this.data.Stype, this.data.Sprice, this.data.Scount, 0)
  },
  orderby_Scount: function () {
    if (this.data.Scount == 0) {
      this.setData({ Scount: 1 })
    } else if (this.data.Scount == 1) {
      this.setData({ Scount: 2 })
    } else {
      this.setData({ Scount: 0 })
    }
    tool.GetServiceList(this, 1, 6, this.data.Stype, this.data.Sprice, this.data.Scount, 0)
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

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
    var that = this
    template.GetVipInfo(that, 1, function (e) {
      if (e = 'success') {
        tool.GetStoreInfo(that)
        tool.GetTechnicianList(that, 1, 6, 0, 0, 0, 0)
        tool.GetServiceList(that, 1, 6, 0, 0, 0, 0)
      }
    })
    template.GetAgentConfigInfo(that)
    that.setData({ Tsex: 0, Tage: 0, Tcount: 0, Stype: 0, Sprice: 0, Scount: 0, chooseProject: 0 })
    template.stopPullDown()
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    if (this.data.typeChoose == 0) {
      tool.GetTechnicianList(this, this.data.pageIndex, 6, this.data.Tsex, this.data.Tage, this.data.Tcount, 1)
    } else {
      tool.GetServiceList(this, this.data.pageIndexS, 6, this.data.Stype, this.data.Sprice, this.data.Scount, 1)
    }
  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  }
})