const app = getApp();
const http = require("http.js");
const addr = require("addr.js");
const util = require("util.js");
const WxParse = require('wxParse/wxParse.js');
const tools = require("tools.js")
// 内容组件
let listViewModal_arr = {}
//产品列表
let listviewModel_goods_arr = {}

// 各控件功能
let pageFunc = {
  a: function(currentCom, isIndex1, j, resolve, fpage) {
    let that = fpage
    if (currentCom.type == "bottomnav") {
      for (let i = 0, len = currentCom.navlist.length; i < len; i++) {
        currentCom.navlist[i].type = 'bottom'
      }
    }
    if (currentCom.type == "magicCube") {
      for (let i = 0, len = currentCom.items.length; i < len; i++) {
        currentCom.items[i].type = 'magic'
      }
    }
    if (currentCom.type == "imgnav") {
      for (let i = 0, len = currentCom.navlist.length; i < len; i++) {
        currentCom.navlist[i].type = 'imgnav'

      }
    }
    if (currentCom.type == "slider") {
      currentCom.current = 0
      for (let i = 0, len = currentCom.items.length; i < len; i++) {
        currentCom.items[i].type = 'slider'
      }
    }
    //视频
    if (currentCom.type == "video") {
      currentCom.sel = false
    }
    //产品
    if (currentCom.type == "good") {
      page.goodRequest(isIndex1, j, currentCom, that)
    }
    //地图
    if (currentCom.type == "map") {
      currentCom.icon = 'http://j.vzan.cc/miniapp/img/enterprise/location2.png'
    }
    //富文本
    if (currentCom.type == "richtxt" && typeof currentCom.content == "string") {
      // 替换富文本标签 控制样式
      currentCom.content = currentCom.content.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
      currentCom.content = currentCom.content.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
      currentCom.content = currentCom.content.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
      currentCom.content = WxParse.wxParse('content', 'html', currentCom.content || "", that, 5);
    }
    //内容咨询
    if (currentCom.type == "news") {
      if (currentCom.listmode == "all" || (currentCom.listmode == 'pick' && currentCom.list.length == 0)) {
        page.allNewsRequest(currentCom.typeid, isIndex1, j, currentCom, that)
      } else {
        page.someNewsRequest(isIndex1, j, currentCom, that);
      }
    }
    // 表单
    if (currentCom.type == "form") {
      currentCom.items.forEach(function(o, i) {
        if (o.type == 'radio') {
          let array = []
          for (let v in o.items) {
            array.push(o.items[v].name)
            o.array = array
          }
        }
      })

    }

    // 背景音乐
    if (currentCom.type == 'bgaudio') {
      let src1 = currentCom.src
      let isPlay = true
      if (app.globalData._bgFirst) {
        wx.playBackgroundAudio({
          dataUrl: src1
        })
        app.globalData._bgFirst = false
      }
      that.setData({
        isPlay: !isPlay
      })
      that.data.src1 = src1
      resolve(src1)
    }
    // 产品列表
    if (currentCom.type == "goodlist") {
      // 是否有分类
      if (currentCom.goodCat.length) {
        var typeid = []
        for (let i = 0; i < currentCom.goodCat.length; i++) {
          if (currentCom.goodCat[i].name == '全部') {
            currentCom.goodCat.splice(i, 1)
          }
          typeid.push(currentCom.goodCat[i].id)
        }

        // 当导航条开启时
        if (currentCom.isShowGoodCatNav) {
          if (currentCom.goodCat.length == 1) {
            typeid = typeid.join(",")
          } else {
            typeid = typeid.join(",")
            if (currentCom.goodCat[0].name != '全部') {
              currentCom.goodCat.unshift({
                id: typeid,
                name: "全部"
              })
            }
          }
        }
        //导航条 关闭时
        else {
          typeid = typeid.join(",")
        }
        that.setData({
          condition: typeid
        })
      }
      // 没有分类则查询全部
      else {
        typeid = ""
      }
      tools.reset(that.data.goodListViewModal)
      page.goodsListRequest(typeid, "", "", "", that)
      that.data.goodExtCat = currentCom.goodExtCat
    }
  }
}
let page = {
  // 根据小程序的appid查询aid渲染页面 
  aidRequest: function(fpage) {
    let that = fpage
    let aid = wx.getStorageSync("aid");
    if (aid) {
      page.pagesRequest(aid, that)
    } else {
      http
        .getAsync(
          addr.Address.Getaid, {
            appid: app.globalData.appid
          })
        .then(function(data) {
          if (data.isok) {
            page.pagesRequest(data.msg, that)
            wx.setStorageSync("aid", data.msg)
          }
        })
    }
  },
  pagesRequest: function(aid, fpage) {
    let isIndex1 = 0
    if (fpage.data.isIndex1) {
      isIndex1 = fpage.data.isIndex1
    } else {
      isIndex1 = 0
    }
    let PageSetting = wx.getStorageSync("PageSetting");
    if (PageSetting) {

      page.pageset(fpage, JSON.parse(PageSetting.msg.pages), isIndex1);
      page.pageData(aid).then(data => {
        if (data.isok) {
          if (PageSetting.msg.updatetime != data.msg.updatetime) {
            page.pageset(fpage, JSON.parse(data.msg.pages), isIndex1);
            wx.setStorageSync("PageSetting", data)
          }

        }

      })
    } else {
      page.pageData(aid).then(data => {
        if (data.isok) {
          page.pageset(fpage, JSON.parse(data.msg.pages), isIndex1);
          wx.setStorageSync("PageSetting", data)
        }

      })
    }
  },
  // 同步数据
  SyncPages: function(aid, isIndex1, fpage) {
    var PageSetting = wx.getStorageSync("PageSetting");
    page.pageData(aid).then(data => {
      if (data.isok) {
        page.pageset(fpage, JSON.parse(data.msg.pages), isIndex1);
      }
      wx.setStorageSync("PageSetting", data)
    })
  },
  //pages数据
  pageset: function(fpage, pages, isIndex1) {
    let that = fpage
    return new Promise(function(resolve, reject) {
      app.globalData.pages = pages;
      wx.showLoading({
        title: '加载中...',
        mask: true,
        success: function(res) {
          //删除产品预约
          for (let i = 0; i < pages.length; i++) {
            if (pages[i].def_name == "产品预约") {
              pages.splice(i, 1)
            }
          }
          for (let j = 0, valKey = pages[isIndex1].coms.length; j < valKey; j++) {
            let currentCom = pages[isIndex1].coms[j]
            pageFunc.a(currentCom, isIndex1, j, resolve, that)
          }
          // 悬浮按钮开关
          page.statusFunc(pages, isIndex1, that)
          that.setData({
            currentPage: pages[isIndex1]
          })
          if (pages) {
            util.setPageSkin(that);
            util.navBarTitle(pages[isIndex1].name)
          }
          wx.hideLoading()
        }
      })
    })
  },
  // 各种状态判断
  statusFunc: function(pages, isIndex1, fpage) {
    let that = fpage
    let template = pages[isIndex1].coms
    let status = that.data.status
    let [bootom, yuyuetemplate, makecalltemplate, customertemplate, goodtemplate, goodlisttemplate] = [
      template.find(f => f.type == 'bottomnav'),
      template.find(f => f.subscribeSwitch == true),
      template.find(f => f.type == "makecall"),
      template.find(f => f.type == "kefu"),
      template.find(f => f.type == "good"),
      template.find(f => f.type == "goodlist"),
    ]
    //背景音乐
    for (let i = 0, val; val = pages[i++];) {
      let _musicTemp = val.coms.find(k => k.type == "bgaudio")
      if (_musicTemp) {
        status._bgmusic = true
        break;
      }
    }
    // 主页
    if (bootom == undefined && isIndex1 != 0) {
      status._homeClose = true
    } else {
      status._homeClose = false
    }

    // 客服
    if (customertemplate) {
      status.cIcon = customertemplate.icon
      status._customer = true
    } else {
      status._customer = false
    }

    // 预约按钮
    if (yuyuetemplate && (goodtemplate || goodlisttemplate)) {
      status._yuyueShow = true
    } else {
      status._yuyueShow = false
    }
    // 电话
    if (makecalltemplate) {
      status.pIcon = makecalltemplate.icon;
      status.pNumber = makecalltemplate.phone;
      status._makecall = true
    } else {
      status._makecall = false;
    }

    //产品列表
    if (goodlisttemplate) {
      status._goodsShow = true
      that.data.goodlisttemplate = goodlisttemplate
    } else {
      status._goodsShow = false
    }
    that.setData({
      status: status
    })
  },
  // 去水印接口
  logoRequest: function(fpage) {
    var that = fpage
    var AgentConfig = wx.getStorageSync("AgentConfig");
    if (AgentConfig) {
      that.setData({
        AgentConfig: AgentConfig
      })
    } else {
      http.getAsync(
          addr.Address.GetAgentConfigInfo, {
            appid: app.globalData.appid
          })
        .then(function(data) {
          if (data.isok == 1) {
            if (data.AgentConfig.isdefaul == 0) {
              data.AgentConfig.LogoText = data.AgentConfig.LogoText.split(' ')
            } else {
              data.AgentConfig.LogoText = data.AgentConfig.LogoText
            }
            that.setData({
              AgentConfig: data.AgentConfig
            })
            wx.setStorageSync("AgentConfig", data.AgentConfig)
          }
        })
    }
  },

  //获取产品列表请求
  goodsListRequest: function(typeid, pricesort, exttypes, search, fpage) {
    let that = fpage
    return new Promise(function(resolve, reject) {
      const vm = that.data.goodListViewModal;
      let currentPage = app.globalData.pages[that.data.isIndex1];
      if (vm.ispost || vm.loadall)
        return;
      if (!vm.ispost)
        that.setData({
          "goodListViewModal.ispost": true
        });
      http.getAsync(
          addr.Address.GetGoodsList, {
            aid: wx.getStorageSync("aid"),
            typeid: typeid,
            pageindex: vm.pageindex,
            pagesize: vm.pagesize,
            pricesort: pricesort,
            exttypes: exttypes + "",
            search: search,
            goodShowType: currentPage.coms[0].goodShowType || ""
          })
        .then(function(data) {
          vm.ispost = false;
          if (data.isok == 1) {
            data.postdata.goodslist.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
            data.postdata.goodslist.length > 0 ? vm.list = vm.list.concat(data.postdata.goodslist) : "";
            that.setData({
              "goodListViewModal": vm
            })
          }
          resolve(data)
        })
    })
  },
  // 产品列表筛选参数
  getExt: function(fpage) {
    let that = fpage
    wx.showLoading({
      title: '加载中...',
      mask: true,
      success: function() {
        http.getAsync(
            addr.Address.GetExtTypes, {
              aid: wx.getStorageSync("aid")
            })
          .then(function(data) {
            if (data.isok == true) {
              let exttypes = data.msg
              let goodExtCat = that.data.goodExtCat
              let extTypes_fmt = [];
              for (let i = 0, val; val = goodExtCat[i++];) {
                let template = exttypes.filter(f => f.ParentId == val.TypeId)
                extTypes_fmt.push({
                  item: val,
                  child: template
                });
              }
              that.setData({
                extTypes_fmt: extTypes_fmt
              })
              wx.hideLoading()
            }
          })
      }
    })
  },
  // 表单
  formRequest: function(formdatajson, comename, fpage) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    let vm = that.data.listViewModal_form
    // 报名请求
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    http
      .postAsync(
        addr.Address.SaveUserForm, {
          uid: userInfo.userid,
          formdatajson: formdatajson,
          aid: wx.getStorageSync("aid"),
          comename: comename,
        })
      .then(function(data) {
        vm.ispost = false; //请求完毕，关闭请求开关
        that.setData({
          typed: '',
          startDate: ''
        })
        if (data.isok == 1) {
          tools.showToast("提交成功")
        } else {
          tools.showToast("提交失败")
        }
      })
  },
  //产品组件
  goodRequest: function(pageindex, comindex, currentCom, fpage) {
    let that = fpage
    let [viewmodel, _goodids] = [{},
      []
    ];
    let news_com_key = pageindex + "_" + comindex;
    currentCom.items.forEach(function(o, i) {
      _goodids.push(o.id)
    })
    let _postids = _goodids.join(",")
    if (_goodids.length > 0) {
      http.getAsync(
          addr.Address.GetGoodsByids, {
            ids: _postids,
            goodShowType: currentCom.goodShowType
          })
        .then(function(data) {
          if (data.isok) {
            viewmodel.list = data.msg
            viewmodel.ids = _postids
            viewmodel.btnType = currentCom.btnType
            listviewModel_goods_arr[news_com_key] = viewmodel;
            that.setData({
              listviewModel_goods_arr: listviewModel_goods_arr
            })
          }
        })
    } else {
      that.setData({
        showGoodText: "暂无数据"
      })
    }
  },
  // 全部新闻
  allNewsRequest: function(typeid, pageindex, comindex, currentCom, fpage) {
    let that = fpage
    let viewmodel = {}
    let news_com_key = pageindex + "_" + comindex;
    http.getAsync(
        addr.Address.GetNewsList, {
          aid: wx.getStorageSync("aid"),
          typeid: typeid,
          pageindex: 0,
          pagesize: 0,
          liststyle: currentCom.liststyle,
        })
      .then(function(data) {
        if (data.isok == true) {
          viewmodel.list = data.data;
          currentCom.listmode == 'pick' && currentCom.list.length == 0 && currentCom.num > 0 ? viewmodel.list = data.data.slice(0, currentCom.num) : "";

          // 时间戳转换
          viewmodel.list.forEach(function(o, i) {
            o.addtime = util.ChangeDateFormat(o.addtime)
            delete o.content;
          })

          listViewModal_arr[news_com_key] = viewmodel;
          that.setData({
            listViewModal_arr: listViewModal_arr
          })
        }
      })
  },
  // 选择新闻/
  someNewsRequest: function(pageindex, comindex, currentCom, fpage) {
    let that = fpage
    let [_newsid, viewmodel] = [
      [], {}
    ];
    let news_com_key = pageindex + "_" + comindex;
    currentCom.list.forEach(function(o, i) {
      _newsid.push(o.id)
    })
    let _newstids = _newsid.join(",");
    if (_newsid.length > 0) {
      http.getAsync(
          addr.Address.GetNewsInfoByids, {
            ids: _newstids,
            liststyle: currentCom.liststyle,
          })
        .then(function(data) {
          if (data.isok == true && data.msg.length > 0) {
            viewmodel.list = data.msg.slice(0, currentCom.num);
            viewmodel.ids = _newstids
            let _temp_newids = [];

            // 时间戳转换
            viewmodel.list.forEach(function(o, i) {
              o.addtime = util.ChangeDateFormat(o.addtime)
              delete o.content;
            })
            listViewModal_arr[news_com_key] = viewmodel;
            that.setData({
              listViewModal_arr: listViewModal_arr
            })
          }
        });
    }
  },
  // 内容资讯详情
  contentDetail: function(id, fpage) {
    let that = fpage
    http.getAsync(
        addr.Address.GetNewsInfo, {
          id: id,
          version: 2,
        })
      .then(function(data) {
        if (data.isok == true) {
          data.msg.slideimgs_fmt = data.msg.slideimgs_fmt.split("|")
          data.msg.slideimgs = data.msg.slideimgs.split(",")
          // 时间戳转换
          data.msg.addtime = util.ChangeDateFormat(data.msg.addtime)
          // 替换富文本标签 控制样式
          data.msg.content = data.msg.content.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
          data.msg.content = data.msg.content.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
          data.msg.content = data.msg.content.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
          let msg = data.msg
          that.setData({
            msg: msg,
            article: WxParse.wxParse('article', 'html', data.msg.content, that, 5),
          })
          // 动态改标题
          util.navBarTitle(data.msg.title)
        }
      })
  },
  //查看内容咨询更多列表
  contentMore: function(ids, typeid, fpage) {
    let that = fpage;
    let vm = that.data.listViewModel
    //如果正在请求或者已经获取了所有数据 停止执行
    if (vm.ispost || vm.loadall)
      return;
    //如果没有请求  打开请求开关
    if (!vm.ispost)
      vm.ispost = true;
    //如果是查询 选择的内容
    if (ids) {
      http
        .getAsync(addr.Address.GetNewsInfoByids, {
          ids: ids,
          pageindex: vm.pageindex,
          pagesize: vm.pagesize,
        })
        .then(function(data) {
          if (data.isok) {
            //设置值
            vm.ispost = false; //请求完毕，关闭请求开关
            vm.loadall = true; //因为是查询选择的内容，一次查询完毕不需要再分页查询
            vm.list = data.msg; //list

            // 时间戳转换 对数据进行格式化
            data.msg.forEach(function(o, i) {
              o.addtime = util.ChangeDateFormat(o.addtime)
            })
            //保存值
            that.setData({
              "listViewModel": vm
            })
          }
        })
    }
    //查询所有
    else {
      http
        .getAsync(addr.Address.GetNewsList, {
          aid: wx.getStorageSync("aid"),
          typeid: typeid,
          pageindex: vm.pageindex,
          pagesize: vm.pagesize
        })
        .then(function(data) {
          if (data.isok) {
            vm.ispost = false; //请求完毕，关闭请求开关
            //格式化数据
            data.data.forEach(function(o, i) {
              o.addtime = util.ChangeDateFormat(o.addtime)
            })
            //更改状态数据
            if (data.data.length >= vm.pagesize) {
              vm.pageindex += 1;
            } else {
              vm.loadall = true;
            }
            if (data.data.length > 0) {
              vm.list = vm.list.concat(data.data);
            }
            that.setData({
              "listViewModel": vm
            })
          }
        })
    }

  },
  //预约列表
  subMore: function(fpage) {
    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    let vm = that.data.listViewModel
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    http
      .getAsync(
        addr.Address.GetSubscribeFormDetail, {
          aid: wx.getStorageSync("aid"),
          uid: userInfo.userid,
          pageindex: vm.pageindex,
          pagesize: vm.pagesize
        })
      .then(function(data) {
        vm.ispost = false; //请求完毕，关闭请求开关
        if (data.isok) {
          //更改状态数据
          if (data.list.length >= vm.pagesize) {
            vm.pageindex += 1;
          } else {
            vm.loadall = true;
          }
          for (var i = 0; i < data.list.length; i++) {
            data.list[i].formdatajson = (data.list[i].formdatajson || "").split(",")
            data.list[i].remark = JSON.parse(data.list[i].remark)
          }
          if (data.list.length > 0) {
            vm.list = vm.list.concat(data.list);
          }
          that.setData({
            "listViewModel": vm
          })
          wx.setStorageSync("listViewModel", vm)
        }
      })
  },
  // 预约表单请求判断
  pageData: function(aid) {
    return new Promise(function(resolve, reject) {
      http.getAsync(
          addr.Address.GetPageSetting, {
            aid: aid
          })
        .then(function(data) {
          resolve(data)
        })
    })
  },
  //提交表单
  submitForm: function(formdatajson, remark, fpage) {


    let that = fpage
    let userInfo = wx.getStorageSync("userInfo")
    let vm = that.data.listFrom
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    http
      .postAsync(
        addr.Address.SaveSubscribeForm, {
          aid: wx.getStorageSync("aid"),
          uid: userInfo.userid,
          formdatajson: formdatajson,
          remark: remark,
        })
      .then(function(data) {


        if (data.isok == 1) {
          tools.showToast("提交成功")
          setTimeout(res => {
            tools.goBackPage(1)
          }, 2000)
        } else {
          tools.showToast("提交失败")
        }
      })
  },



  // 产品详情页请求
  detailsRequest: function(pid, fpage, leveId) {
    let that = fpage
    http.getAsync(
        addr.Address.GetGoodInfo, {
          pid: pid,
          version: 2,
        })
      .then(function(data) {
        if (data.isok) {
          let msg = data.msg
          let [specificationdetail, pickspecification, discountTotal] = [
            [],
            [], 0
          ]
          //保存商品
          console.log("商品的名称", msg.name)
          msg.slideimgs = msg.slideimgs.split(",")
          msg.slideimgs_fmt = msg.slideimgs_fmt.split("|")
          if (msg.pickspecification) {
            pickspecification = JSON.parse(msg.pickspecification)
            for (let i = 0, val; val = pickspecification[i++];) {
              for (let j = 0, key; key = val.items[j++];) {
                key.sel = false
              }
            }
          }
          if (msg.specificationdetail) {
            specificationdetail = JSON.parse(msg.specificationdetail)
          }
          msg.discountPrice = parseFloat(msg.discountPrice).toFixed(2)
          discountTotal = parseFloat(msg.discountPrice).toFixed(2)
          // 替换富文本标签 控制样式
          msg.description = msg.description.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
          msg.description = msg.description.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
          msg.description = msg.description.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
          that.setData({
            msg: msg,
            stock: msg.stock, //初始库存
            discountTotal: discountTotal, //初始弹窗价格
            oldprice: (msg.price).toFixed(2), //原始价格
            pickspecification: pickspecification,
            article: WxParse.wxParse('article', 'html', msg.description, that, 5),
          })
          that.data.discountPrice = msg.price //初始折扣价格
          that.data.specificationdetail = specificationdetail //属性
          //动态改标题
          util.navBarTitle(msg.name)
        }
      })
  },
}
module.exports = page