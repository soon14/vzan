// pages/my/my.js
var http = require('../../script/pinxianghui.js');
var template = require('../../script/template.js');
var addr = require('../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    entry_order: [{
        fontcss: 'dzicon icon-icon_daifukuan- eo_icon t44 l44',
        content: '待付款',
        red_point: '0',
        state: 0,
        groupState: -999,
        commentState: -999
      },
      {
        fontcss: 'dzicon icon-icon_daifenxiang eo_icon t42 l44',
        content: '待分享',
        red_point: '0',
        state: -999,
        groupState: 0,
        commentState: -999
      },
      {
        fontcss: 'dzicon icon-icon_daifahuo- eo_icon t46 l44',
        content: '待发货',
        red_point: '0',
        state: 1,
        groupState: -999,
        commentState: -999
      },
      {
        fontcss: 'dzicon icon-icon_daishouhuo- eo_icon t42 l44',
        content: '待收货',
        red_point: '0',
        state: 2,
        groupState: -999,
        commentState: -999
      },
      // { fontcss: 'dzicon icon-icon_wodepingjia- eo_icon t44 l44', content: '待评价', red_point: '0', state: 4, groupState: -999, commentState: 1 },
    ],
    entry_list: [{
        fontcss: 'dzicon icon-icon_youhuiquan- el-0',
        content: '优惠券',
        red_point: '',
        id: 0,
        isopen: false,
				getuserInfo:false
      },
      {
        fontcss: 'dzicon icon-icon_wodepingjia- el-1',
        content: '我的评价',
        red_point: '',
        id: 1,
        isopen: false,
				getuserInfo: false	
      },
      {
        fontcss: 'dzicon icon-icon_shouhuodizhi- el-2',
        content: '收货地址',
        red_point: '',
        url: '/pages/my/Address/myAddress',
        id: 2,
        isopen: true,
				getuserInfo: false
      },
      {
        fontcss: 'dzicon icon-icon_wodezixun el-3',
        content: '我的咨询',
        red_point: '',
        id: 3,
        isopen: false,
				getuserInfo: false
      },
      {
        fontcss: 'dzicon icon-icon_kefu el-4',
        content: '客服管理',
        red_point: '',
        id: 4,
        isopen: false,
				getuserInfo: false
      },
      {
        fontcss: 'dzicon icon-icon_dianpudingdan el-5 t42',
        content: '店铺订单',
        red_point: '',
        url: '',
        id: 5,
        isopen: true,
				getuserInfo: false
      },
      {
        fontcss: 'dzicon icon-icon_shangpinguanli el-6',
        content: '商品管理',
        red_point: '',
        url: '',
        id: 6,
        isopen: true,
				getuserInfo: true
      },
      {
        fontcss: 'dzicon icon-icon_wodedianpu el-7',
        content: '我的店铺',
        red_point: '',
        url: '',
        id: 7,
        isopen: true,
				getuserInfo: true
      },
      {
        fontcss: 'dzicon icon-Officialnetwork_guanwang el-8',
        content: '免费入驻',
        red_point: '',
        url: '/pages/store/applyEnter/applyEnter',
        id: 8,
        isopen: true,
				getuserInfo: true
      },
      {
        fontcss: 'dzicon icon-icon_wodeguanzhu el-9',
        content: '我的关注',
        red_point: '',
        url: '/pages/store/myFollow/myFollow',
        id: 9,
        isopen: true,
				getuserInfo: false
      },
      {
        fontcss: 'dzicon icon-icon_qingchuhuancun el-10',
        content: '清除缓存',
        red_point: '',
        id: 10,
        isopen: false,
				getuserInfo: false
      },
      {
        fontcss: 'dzicon icon-pinglun1 el-10 t38',
        content: '我的评论',
        red_point: '',
        id: 11,
        isopen: false,
				getuserInfo: false
      },
      {
        fontcss: 'dzicon icon-icon_tuiguangdaili- el-5 t38',
        content: '推广代理',
        red_point: '',
        url: '',
        id: 12,
        isopen: false,
				getuserInfo: true
      },
      {
        fontcss: 'dzicon icon-biaogan_biaogan- el-5 t42',
        content: '标杆计划',
        red_point: 'new',
        url: '',
        id: 13,
        isopen: false,
				getuserInfo: true
      },
    ]
  },
  entryList_nt: function(e) {
    var _url = e.currentTarget.dataset.url
    var _id = e.currentTarget.dataset.id
    if (_id == 10) {
      wx.clearStorageSync()
      template.showtoast('清除成功', 'success')
    }
    if (_url) {
      template.gonewpage(_url)
    }
  },
  orderlist_nt: function(e) {
    var index = e.currentTarget.id
    var dataset = e.currentTarget.dataset
    template.gonewpage('/pages/my/orderList/orderList?id=' + index + '&dataset=' + JSON.stringify(dataset))
  },
  getUserInfo: function(e) {
    var that = this
    var _e = e.detail
    var g = getApp().globalData;
    if (_e.encryptedData && _e.iv && _e.signature) {
      wx.login({
        success: function(res) {
          app.loginByThirdPlatform(res.code, _e.encryptedData, _e.signature, _e.iv, function(cb) {
            if (cb) {
              app.login(function() {
                that.setData({
                  userInfo: g.userInfo
                })
								template.showtoast('已刷新','none')
              })
            }
          }, 0)
        }
      })
    }
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {

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

      var _u = g.userInfo;
      var _e = that.data.entry_list
      // _e[4].isopen = (_u.StoreId == 0 ? false : true)
      _e[5].isopen = (_u.StoreId == 0 ? false : true)
      _e[6].isopen = (_u.StoreId == 0 ? false : true)
      _e[7].isopen = (_u.StoreId == 0 ? false : true)
      _e[8].isopen = (_u.StoreId == 0 ? true : false)
      _e[13].isopen = (_u.StoreId == 0 ? false : true)
      that.setData({
        userInfo: _u,
        entry_list: _e,
        'entry_list[5].url': '/pages/store/orderList/orderIndex?storeid=' + g.userInfo.StoreId,
        'entry_list[6].url': '/pages/my/mgr/product/product?storeid=' + g.userInfo.StoreId,
        'entry_list[7].url': '/pages/store/store?storeid=' + g.userInfo.StoreId,
        'entry_list[12].url': '/pages/my/Agent/agentIndex?storeid=' + g.userInfo.StoreId,
				'entry_list[13].url': '/pages/my/applyModel/applyModel?storeid=' + g.userInfo.StoreId,
      })

      app.login(function(g) {
        http.gRequest(addr.IsAgent, {}, function(callback) {
          var agent = callback.data.obj.agent
          that.setData({
            'entry_list[12].isopen': agent == null ? false : true
          })
        })
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
    this.onShow();
    template.stoppulldown()
  },
})