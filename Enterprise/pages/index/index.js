// pages/index/index.js
const app = getApp();
const util = require("../../utils/util.js");
const tools = require("../../utils/tools.js");
const page = require("../../utils/pageRequest.js");
const animation = require("../../utils/animation.js");
const pickerFile = require('../../template/picker/js/picker_datetime.js')

var listViewModal = {
  pageindex: 1,
  pagesize: 10,
  list: [],
  ispost: false,
  loadall: false,
  ids: '',
  btnType: '',
}
var _goodListViewModal = {
  pageindex: 1,
  pagesize: 10,
  list: [],
  ispost: false,
  loadall: false,
  pricesort: "",
  exttypes: "",
  search: "",
  text: "价格",
  price: [
    { context: "价格不限", id: "0" },
    { context: "价格由高到低", id: "1" },
    { context: "价格由低到高", id: "2" },
  ],
  inshow: 0
};
// 各状态判断

Page({
  data: {
    currentPage: null,
    isIndex1: 0, //点击跳转
    // 表单组件
    listViewModal_form: {
      pageindex: 1,
      pagesize: 10,
      list: {},
      ispost: false,
      loadall: false,
    },
    // 产品列表组件
    goodListViewModal: JSON.parse(JSON.stringify(_goodListViewModal)),
    extId: [],
    search: "",
    pricesort: "",
    // 各状态判断
    status: {
      _homeClose: false,
      _shareShow: false,
      _customer: false,
      _buyShow: false,
      _yuyueShow: false,
      _makecall: false,
      _goodsShow: false,
      _bgmusic: false,
      _haveReductionmoney: false,
      _shopRed: false,
      sIcon: "",
      cIcon: "",
      pIcon: "",
      pNumber: "",
      showModalStatus: false,//排序动画
      showMadalFilterStatus: false, //筛选动画
      kipperMark: false,
      openTelSuspend: false,  //开启电话图标悬浮
      openServiceSuspend: false,  //开启客服图标悬浮
      contactPhone: "",  //联系店主组件的拨打电话号码
      openWxShopMessage: false, //联系店主的详情推送框
      headImg: "",
      nickName: "",
    },
    pickIndex: {},
    time: {}
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    let isIndex1 = options && options.isIndex1 ? options.isIndex1 : that.data.isIndex1
    that.setData({ isIndex1 })
    //用户授权
    page.aidRequest(that)
    //底部水印
    page.logoRequest(that)
    // 时间选择动画
    that.datetimePicker = new pickerFile.pickerDatetime({
      page: this,
      animation: 'slide',
      duration: 200,
    });
  },


  componNav: function (e) {
    let _g = e.currentTarget.dataset.items
    console.log(_g)
    let [img, imgs] = ['', []]
    let _pages = wx.getStorageSync('PageSetting')
    let index = Number(_g.urltype)
    if (index > 0) {
      return;
    }
    img = _g.imgurl || _g.img
    imgs.push(img)
    switch (index) {
      case -1:
        if (_g.type == 'img' || _g.type == 'slider' || _g.type == 'magic') {
          util.preViewShow(img, imgs)
        } else {
          tools.showLoadToast("未设置跳转")
        }
        break;
      case 0:
        if (_g.url == -1) {
          if (_g.type == 'img' || _g.type == 'slider' || _g.type == 'magic') {
            util.preViewShow(img, imgs)
          } else {
            tools.showLoadToast("未设置跳转")
          }
        } else {
          if (_g.target == '_blank') {
            tools.goNewPage("../index/index?isIndex1=" + _g.url)
          } else {
            page.pageset(this, JSON.parse(_pages.msg.pages), _g.url)
            this.setData({ isIndex1: _g.url })
          }
        }
    }
  },
  makeMinapp: function (e) {
    let id = e.currentTarget.dataset.id
    wx.navigateTo({
      url: "/pages/index/minapp?id=" + id,
    })
  },
  goHome: function () {
    wx.reLaunch({
      url: '/pages/index/index?isIndex1=0',
    })
  },
  // 各组件跳转
  templGoto: function (e) {
    let that = this
    let [ds, Id] = [e.currentTarget.dataset, Number(e.currentTarget.id)]
    let liveId = []
    let url = ""
    switch (Id) {
      case 1:
        url = '../contentAD/contentAD?typeid=' + ds.typeid + "&ids=" + ds.ids + "&mr=true";
        break; // 内容咨询查看更多
      case 2:
        url = '../contentAD/contentAD?id=' + e.currentTarget.dataset.id + "&dl=true";
        break; // 内容咨询详情
      case 3:
        url = '../detail/detail?id=' + ds.id + "&typeName=" + ds.name + "&showprice=" + ds.showprice
        break; // 产品详情
      case 5:
        url = '../subscribe/subscribe?pid=' + ds.id + '&name=' + ds.name + "&form=true";
        break; // 预约跳转
      case 6:
        url = '../subscribe/subscribe?sublist=true';
        break; //查看表单
    }
    tools.goNewPage(url)
  },
  // 排序
  priceSortFunc: function (e) {
    let that = this;
    let [id, ds] = [Number(e.currentTarget.id), e.currentTarget.dataset]
    let [index, changText] = [ds.id, ds.content]
    tools.reset(that.data.goodListViewModal)
    let currStatus = "open"
    let condition = that.data.condition
    let pricesort = that.data.pricesort
    switch (id) {
      case 0:
        currStatus = "close"
        page.goodsListRequest(condition, pricesort, "", "", that);
        break;
      case 1:
        currStatus = "close"
        pricesort = "desc"
        page.goodsListRequest(condition, pricesort, "", "", that);
        break;
      case 2:
        currStatus = "close"
        pricesort = "asc"
        page.goodsListRequest(condition, pricesort, "", "", that);
        break;
      case 3:
        currStatus = "open"
        break;
      case 4:
        currStatus = "close"
        break;
    }
    that.setData({
      "goodListViewModal.text": changText,
      "goodListViewModal.inshow": index,
    })
    that.data.pricesort = pricesort
    animation.utilDown(currStatus, that);
  },
  //筛选
  fiFterFunc: function (e) {
    let that = this
    let extTypes_fmt = that.data.extTypes_fmt
    let [id, ds] = [Number(e.currentTarget.id), e.currentTarget.dataset]
    let [parentindex, childindex, sel] = [ds.parentindex, ds.childindex, ds.sel]
    let key = "extTypes_fmt[" + parentindex + "].child[" + childindex + "].sel"
    let condition = that.data.condition
    switch (id) {
      case 0:
        page.getExt(that)
        animation.utilFilter("open", that);
        break; //弹出
      case 1:
        animation.utilFilter("close", that);
        break; //收回
      case 2:
        tools.reset(that.data.goodListViewModal)
        page.goodsListRequest(condition, "", that.data.extId, "", that)
        that.data.extId = []
        animation.utilFilter("close", that)
        break; //确定
      case 3:
        for (let i = 0, val; val = extTypes_fmt[i++];) {
          for (let j = 0, key; key = val.child[j++];) {
            key.sel == true ? key.sel = false : "";
          }
        }
        that.data.extId = []
        that.setData({ extTypes_fmt: extTypes_fmt })
        break; //重置
      case 4:
        let template = extTypes_fmt[parentindex].child[childindex]
        that.setData({ [key]: !template.sel })
        if (template.sel) {
          let [parentId, childId] = [template.ParentId, template.TypeId]
          let exttypesId = parentId + "-" + childId
          that.data.extId.push(exttypesId)
        }
        break; //选择
    }
  },
  //搜索 分类导航
  proFunc: function (e) {
    let that = this
    let id = Number(e.currentTarget.id)
    let search = that.data.search
    if (e.currentTarget.dataset.id != undefined) {
      var condition = e.currentTarget.dataset.id
    } else {
      condition = that.data.condition
    }
    tools.reset(that.data.goodListViewModal)
    switch (id) {
      case 0:
        search = e.detail.value
        page.goodsListRequest(condition, "", "", search, that)
        break;
      case 1:
        wx.showLoading({
          title: '加载中...',
          mask: true,
          success: function () {
            Promise.all([page.goodsListRequest(condition, "", "", "", that)]).then(function (data) {
              if (data[0].isok == 1) {
                wx.hideLoading()
              }
            })
          }
        })
        that.setData({ condition: condition })
        break;
      case 2:
        wx.pageScrollTo({ scrollTop: 0 })
        break;
    }
    that.data.search = search
  },
  // 下拉加载更多
  onReachBottom: function () {
    let that = this
    if (that.data.currentPage.coms[0].type == 'goodlist') {
      page.goodsListRequest(that.data.condition, that.data.pricesort, that.data.extId, that.data.search, that)
    }
  },
  // 上拉刷新
  onPullDownRefresh: function () {
    let that = this
    let app = getApp();
    wx.removeStorageSync("aid");
    wx.removeStorageSync("AgentConfig");
    wx.removeStorageSync("PageSetting");
    tools.showLoadToast("正在刷新")
    that.onLoad();
    that.onShow();
    setTimeout(res => {
      tools.showToast("刷新成功");
      wx.stopPullDownRefresh()
    }, 1500)
  },
  // 各组件功能
  pageFunc: function (e) {
    let that = this
    let ds = e.currentTarget.dataset
    let id = Number(e.currentTarget.id)
    switch (id) {
      case 0:
        tools.mapFunc(ds.lat, ds.lng)
        break; // 地图处理
      case 1:
        tools.phoneFunc(ds.phone)
        break; // 拨打电话
      case 4:
        let template = that.data.currentPage.coms[ds.childindex].sel
        let key = "currentPage.coms[" + ds.childindex + "].sel"
        that.setData({ [key]: !template })
        break; //视频
      case 7:
        tools.phoneFunc(ds.phone)   //联系店主拨打电话
        break;
    }
  },
  selTime: function (e) {
    this.datetimePicker.setPicker('startDate');
  },
  selPick: function (e) {
    let _d = e.currentTarget
    let pickIndex = this.data.pickIndex
    pickIndex[_d.id] = parseInt(e.detail.value)
    this.setData({ pickIndex })
  },
  // 音乐背景播放
  playAudioFunc: function (e) {
    let that = this
    let [isPlay, src] = [true, that.data.src1]
    wx.playBackgroundAudio({ dataUrl: src })
    that.setData({ isPlay: !isPlay })
  },
  // 音乐背景暂停播放
  stopAudioFunc: function () {
    wx.stopBackgroundAudio()
    this.setData({ isPlay: !this.data.isPlay })
  },
  // 表单提交，提交真是姓名和手机号码
  sumbitFormFuc: function (e) {
    let that = this
    let [detail, comename] = [JSON.stringify(e.detail.value), e.detail.target.dataset.name]
    for (let key in e.detail.value) {
      if (e.detail.value[key] == '') {
        tools.showToast("信息未填写完整")
        return
      }
    }
    page.formRequest(detail, comename, that)
  },
  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {
    let that = this
    let title = ""
    let imageUrl = ""
    return {
      title: title,
      path: '/pages/index/index?isIndex1=' + that.data.isIndex1,
      imageUrl: imageUrl,
      success: function (res) {
        tools.showToast("转发成功")
      }
    }
  },





})