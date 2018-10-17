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
// 砍价
let listviewModel_cutprice_arr = {}
//拼团
let listviewModel_joingroup_arr = {}
//拼团2.0
let listviewModel_entjoingroup_arr = {}
// 各控件功能
let pageFunc = {
	a: function (currentCom, isIndex1, j, resolve, fpage) {
		let that = fpage
		//视频
		if (currentCom.type == "video") {
			currentCom.sel = false
		}
		//产品
		if (currentCom.type == "good") {
			page.goodRequest(isIndex1, j, currentCom, that)
		}
		//砍价
		if (currentCom.type == "cutprice") {
			page.bargainRequest(isIndex1, j, currentCom, that)
		}
		//拼团
		if (currentCom.type == "joingroup") {
			page.joinRequest(isIndex1, j, currentCom, that)
		}
		//拼团2.0
		if (currentCom.type == "entjoingroup") {
			page.joinRequest_2(isIndex1, j, currentCom, that)
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
			}
			else {
				page.someNewsRequest(isIndex1, j, currentCom, that);
			}
		}
		// 表单
		if (currentCom.type == "form") {
			currentCom.items.forEach(function (o, i) {
				if (o.type == 'radio') {
					let array = []
					for (let v in o.items) {
						array.push(o.items[v].name)
						o.array = array
					}
				}
			})
			that.setData({ pickIndex: 0 })
		}
		// 直播
		if (currentCom.type == "live") {
			let [liveList, listviewModel_liveList_arr] = [currentCom.items, {}]
			let news_live_key = isIndex1 + "_" + j
			liveList = {
				list: currentCom.items.slice(0, 3),
				num: currentCom.items.length
			}
			listviewModel_liveList_arr[news_live_key] = liveList
			that.setData({ listviewModel_liveList_arr: listviewModel_liveList_arr })
		}
		// 背景音乐
		if (currentCom.type == 'bgaudio') {
			let src1 = currentCom.src
			let isPlay = true
			if (app.globalData._bgFirst) {
				wx.playBackgroundAudio({ dataUrl: src1 })
				app.globalData._bgFirst = false
			}
			that.setData({ isPlay: !isPlay })
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
							currentCom.goodCat.unshift({ id: typeid, name: "全部" })
						}
					}
				}
				//导航条 关闭时
				else {
					typeid = typeid.join(",")
				}
				that.setData({ condition: typeid })
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
	aidRequest: function (fpage) {
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
				.then(function (data) {
					if (data.isok) {
						page.pagesRequest(data.msg, that)
						wx.setStorageSync("aid", data.msg)
					}
				})
		}
	},
	pagesRequest: function (aid, fpage) {
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
		}
		else {
			page.pageData(aid).then(data => {
				if (data.isok) {
					page.pageset(fpage, JSON.parse(data.msg.pages), isIndex1);
					wx.setStorageSync("PageSetting", data)
				}

			})
		}
	},
	// 同步数据
	SyncPages: function (aid, isIndex1, fpage) {
		var PageSetting = wx.getStorageSync("PageSetting");
		page.pageData(aid).then(data => {
			if (data.isok) {
				page.pageset(fpage, JSON.parse(data.msg.pages), isIndex1);
			}
			wx.setStorageSync("PageSetting", data)
		})
	},
	//pages数据
	pageset: function (fpage, pages, isIndex1) {
		let that = fpage
		return new Promise(function (resolve, reject) {
			app.globalData.pages = pages;
			wx.showLoading({
				title: '加载中...',
				mask: true,
				success: function (res) {
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
					that.setData({ currentPage: pages[isIndex1] })
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
	statusFunc: function (pages, isIndex1, fpage) {
		let that = fpage
		let template = pages[isIndex1].coms
		let status = that.data.status
		let [bootom, buytemplate, yuyuetemplate, makecalltemplate, customertemplate, sharetemplate, goodtemplate, goodlisttemplate, tempCutprice, contacttemplate] = [
			template.find(f => f.type == 'bottomnav'),
			template.find(f => f.btnType == 'buy'),
			template.find(f => f.btnType == "yuyue"),
			template.find(f => f.type == "makecall"),
			template.find(f => f.type == "kefu"),
			template.find(f => f.type == 'share'),
			template.find(f => f.type == "good"),
			template.find(f => f.type == "goodlist"),
			template.find(f => f.type == "cutprice"),
			template.find(f => f.type == "contactShopkeeper"),
		]
		//背景音乐
		for (let i = 0, val; val = pages[i++];) {
			let _musicTemp = val.coms.find(k => k.type == "bgaudio")
			if (_musicTemp) {
				status._bgmusic = true
				break;
			}
		}

		//联系店主的电话图标
		if (contacttemplate) {
			status.openTelSuspend = contacttemplate.openTelSuspend
			status.contactPhone = contacttemplate.phoneNum
			wx.setStorageSync("mark", true)
		} else {
			status.openTelSuspend = false
			//wx.setStorageSync("mark", false)

		}

		for (let i = 0, val; val = pages[i++];) {
			let _musicTemp = val.coms.find(k => k.type == "bgaudio")
			if (_musicTemp) {
				status._bgmusic = true
				break;
			}
		}


		//联系店主客服图标
		if (contacttemplate) {
			status.openServiceSuspend = contacttemplate.openServiceSuspend
		} else {
			status.openServiceSuspend = false
			status.openWxShopMessage = false

		}

    // 主页
    if (bootom == undefined && isIndex1 != 0) {
      status._homeClose = true
    } else {
      status._homeClose = false
    }
    //  分享按钮
    if (sharetemplate) {
      status.sIcon = sharetemplate.icon
      status._shareShow = true
    } else {
      status._shareShow = false
    }
    // 客服
    if (customertemplate) {
      status.cIcon = customertemplate.icon
      status._customer = true
    } else {
      status._customer = false
    }
    // 购物车图标
    if (buytemplate && (goodtemplate || goodlisttemplate)) {
      status._buyShow = true
      //page.shopCartData(that)
    } else {
      status._buyShow = false;
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
    // 倒计时
    if (tempCutprice) {
      setInterval(res => {
        that.setData({ nowTime: new Date().getTime() })
      }, 1000);
    }
    //产品列表
    if (goodlisttemplate) {
      status._goodsShow = true
      that.data.goodlisttemplate = goodlisttemplate
    } else {
      status._goodsShow = false
    }
    that.setData({ status: status })
  },
  // 去水印接口
  logoRequest: function (fpage) {
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
        .then(function (data) {
          if (data.isok == 1) {
            if (data.AgentConfig.isdefaul == 0) {
              data.AgentConfig.LogoText = data.AgentConfig.LogoText.split(' ')
            } else {
              data.AgentConfig.LogoText = data.AgentConfig.LogoText
            }
            that.setData({ AgentConfig: data.AgentConfig })
            wx.setStorageSync("AgentConfig", data.AgentConfig)
          }
        })
    }
  },
  // 自定义分享转发页面
  pageShare: function (fpage) {
    let that = fpage
    return new Promise(function (resolve, reject) {
      http.getAsync(
        addr.Address.GetShare, {
          appId: app.globalData.appid
        })
        .then(function (data) {
          if (data.isok) {
            app.globalData.shareObj = data.obj
            that.data.shareTitle = data.obj.ADTitle
            if (data.obj.ADImg.length != 0) {
              data.obj.ADImg[0].url = data.obj.ADImg[0].url
              that.data.shareImage = data.obj.ADImg[0].url
            }
            wx.setStorageSync("pageShare", data.obj)
            resolve(data)
          }
        })
    })
  },
  //获取产品列表请求
  goodsListRequest: function (typeid, pricesort, exttypes, search, fpage) {
    let that = fpage
    return new Promise(function (resolve, reject) {
      const vm = that.data.goodListViewModal;
      let currentPage = app.globalData.pages[that.data.isIndex1];
      if (vm.ispost || vm.loadall)
        return;
      if (!vm.ispost)
        that.setData({ "goodListViewModal.ispost": true });
      http.getAsync(
        addr.Address.GetGoodsList, {
          aid: wx.getStorageSync("aid"),
          typeid: typeid,
          pageindex: vm.pageindex,
          pagesize: vm.pagesize,
          pricesort: pricesort,
          exttypes: exttypes + "",
          search: search,
          levelid: wx.getStorageSync("levelid"),
          goodShowType: currentPage.coms[0].goodShowType
        })
        .then(function (data) {
          vm.ispost = false;
          if (data.isok == 1) {
            data.postdata.goodslist.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
            data.postdata.goodslist.length > 0 ? vm.list = vm.list.concat(data.postdata.goodslist) : "";
            that.setData({ "goodListViewModal": vm })
          }
          resolve(data)
        })
    })
  },
  // 产品列表筛选参数
  getExt: function (fpage) {
    let that = fpage
    wx.showLoading({
      title: '加载中...',
      mask: true,
      success: function () {
        http.getAsync(
          addr.Address.GetExtTypes, {
            aid: wx.getStorageSync("aid")
          })
          .then(function (data) {
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
              that.setData({ extTypes_fmt: extTypes_fmt })
              wx.hideLoading()
            }
          })
      }
    })
  },
  // 表单
  formRequest: function (formdatajson, comename, fpage) {
    let that = fpage
    let vm = that.data.listViewModal_form
    // 报名请求
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    http
      .postAsync(
      addr.Address.SaveUserForm, {
        uid: app.globalData.userInfo.UserId,
        formdatajson: formdatajson,
        aid: wx.getStorageSync("aid"),
        comename: comename,
          // 扫码购物
        StoreCodeId: getApp().globalData.storecodeid
          // #end
      })
      .then(function (data) {
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
  // 判断购物车列是否有商品
  newshopcartRequest: function (fpage) {
    var that = fpage
    wx.showLoading({
      title: '加载中...',
      mask: true,
      success: function () {
        var shopCartList = that.data.shopCartList
        var currentStatu = false
        var showModalStatus2 = false
        http
          .getAsync(
          addr.Address.getGoodsCarData_new, {
            appId: app.globalData.appid,
            openid: app.globalData.userInfo.openId,
            levelid: app.globalData.vipInfo.levelid
          })
          .then(function (data) {
            if (data.isok == 1) {
              shopCartList = data.postdata
              var goodsList = data.postdata
              for (var i = 0; i < goodsList.length; i++) {
                var currentList = goodsList[i]
                currentList.showModalStatus2 = showModalStatus2
                currentList.Price = parseFloat(currentList.Price / 100).toFixed(2)
                currentList.originalPrice = parseFloat(currentList.originalPrice / 100).toFixed(2)
                if (currentList.goodsMsg.pickspecification != '') {
                  currentList.goodsMsg.pickspecification = JSON.parse(currentList.goodsMsg.pickspecification)
                }
                currentList.goodsMsg.specificationdetail = JSON.parse(currentList.goodsMsg.specificationdetail)
              }
              wx.hideLoading()
            }
            that.setData({ goodsList: goodsList })
          })
      }
    })
		// 主页
		if (bootom == undefined && isIndex1 != 0) {
			status._homeClose = true
		} else {
			status._homeClose = false
		}
		//  分享按钮
		if (sharetemplate) {
			status.sIcon = sharetemplate.icon
			status._shareShow = true
		} else {
			status._shareShow = false
		}
		// 客服
		if (customertemplate) {
			status.cIcon = customertemplate.icon
			status._customer = true
		} else {
			status._customer = false
		}
		// 购物车图标
		if (buytemplate && (goodtemplate || goodlisttemplate)) {
			status._buyShow = true
			// page.shopCartData(that)
		} else {
			status._buyShow = false;
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
		// 倒计时
		if (tempCutprice) {
			setInterval(res => {
				that.setData({ nowTime: new Date().getTime() })
			}, 1000);
		}
		//产品列表
		if (goodlisttemplate) {
			status._goodsShow = true
			that.data.goodlisttemplate = goodlisttemplate
		} else {
			status._goodsShow = false
		}
		that.setData({ status: status })
	},
	// 去水印接口
	logoRequest: function (fpage) {
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
				.then(function (data) {
					if (data.isok == 1) {
						if (data.AgentConfig.isdefaul == 0) {
							data.AgentConfig.LogoText = data.AgentConfig.LogoText.split(' ')
						} else {
							data.AgentConfig.LogoText = data.AgentConfig.LogoText
						}
						that.setData({ AgentConfig: data.AgentConfig })
						wx.setStorageSync("AgentConfig", data.AgentConfig)
					}
				})
		}
	},
	// 自定义分享转发页面
	pageShare: function (fpage) {
		let that = fpage
		return new Promise(function (resolve, reject) {
			http.getAsync(
				addr.Address.GetShare, {
					appId: app.globalData.appid
				})
				.then(function (data) {
					if (data.isok) {
						app.globalData.shareObj = data.obj
						that.data.shareTitle = data.obj.ADTitle
						if (data.obj.ADImg.length != 0) {
							data.obj.ADImg[0].url = data.obj.ADImg[0].url
							that.data.shareImage = data.obj.ADImg[0].url
						}
						wx.setStorageSync("pageShare", data.obj)
						resolve(data)
					}
				})
		})
	},
	//获取产品列表请求
	goodsListRequest: function (typeid, pricesort, exttypes, search, fpage) {
		let that = fpage
		return new Promise(function (resolve, reject) {
			const vm = that.data.goodListViewModal;
			let currentPage = app.globalData.pages[that.data.isIndex1];
			if (vm.ispost || vm.loadall)
				return;
			if (!vm.ispost)
				that.setData({ "goodListViewModal.ispost": true });
			http.getAsync(
				addr.Address.GetGoodsList, {
					aid: wx.getStorageSync("aid"),
					typeid: typeid,
					pageindex: vm.pageindex,
					pagesize: vm.pagesize,
					pricesort: pricesort,
					exttypes: exttypes + "",
					search: search,
					levelid: wx.getStorageSync("levelid"),
					goodShowType: currentPage.coms[0].goodShowType
				})
				.then(function (data) {
					vm.ispost = false;
					if (data.isok == 1) {
						data.postdata.goodslist.length >= vm.pagesize ? vm.pageindex += 1 : vm.loadall = true;
						data.postdata.goodslist.length > 0 ? vm.list = vm.list.concat(data.postdata.goodslist) : "";
						that.setData({ "goodListViewModal": vm })
					}
					resolve(data)
				})
		})
	},
	// 产品列表筛选参数
	getExt: function (fpage) {
		let that = fpage
		wx.showLoading({
			title: '加载中...',
			mask: true,
			success: function () {
				http.getAsync(
					addr.Address.GetExtTypes, {
						aid: wx.getStorageSync("aid")
					})
					.then(function (data) {
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
							that.setData({ extTypes_fmt: extTypes_fmt })
							wx.hideLoading()
						}
					})
			}
		})
	},
	// 表单
	formRequest: function (formdatajson, comename, fpage) {
		let that = fpage
		let vm = that.data.listViewModal_form
		// 报名请求
		if (vm.ispost || vm.loadall)
			return;
		if (!vm.ispost)
			vm.ispost = true;
		http
			.postAsync(
			addr.Address.SaveUserForm, {
				uid: app.globalData.userInfo.UserId,
				formdatajson: formdatajson,
				aid: wx.getStorageSync("aid"),
				comename: comename,
				// 扫码购物
				StoreCodeId: getApp().globalData.storecodeid
				// #end
			})
			.then(function (data) {
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
	// 判断购物车列是否有商品
	newshopcartRequest: function (fpage) {
		var that = fpage
		wx.showLoading({
			title: '加载中...',
			mask: true,
			success: function () {
				var shopCartList = that.data.shopCartList
				var currentStatu = false
				var showModalStatus2 = false
				http
					.getAsync(
					addr.Address.getGoodsCarData_new, {
						appId: app.globalData.appid,
						openid: app.globalData.userInfo.openId,
						levelid: app.globalData.vipInfo.levelid
					})
					.then(function (data) {
						if (data.isok == 1) {
							shopCartList = data.postdata
							var goodsList = data.postdata
							for (var i = 0; i < goodsList.length; i++) {
								var currentList = goodsList[i]
								currentList.showModalStatus2 = showModalStatus2
								currentList.Price = parseFloat(currentList.Price / 100).toFixed(2)
								currentList.originalPrice = parseFloat(currentList.originalPrice / 100).toFixed(2)
								if (currentList.goodsMsg.pickspecification != '') {
									currentList.goodsMsg.pickspecification = JSON.parse(currentList.goodsMsg.pickspecification)
								}
								currentList.goodsMsg.specificationdetail = JSON.parse(currentList.goodsMsg.specificationdetail)
							}
							wx.hideLoading()
						}
						that.setData({ goodsList: goodsList })
					})
			}
		})

	},
	//获取储值余额
	userOfmoneyRequest: function (openid, fpage) {
		var that = fpage
		http
			.getAsync(
			addr.Address.getSaveMoneySetUser, {
				appid: app.globalData.appid,
				openid: openid
			})
			.then(function (data) {
				if (data.isok) {
					let saveMoney = parseFloat(data.saveMoneySetUser.AccountMoneyStr).toFixed(2)
					that.setData({ saveMoney: saveMoney })
				}
			})
	},
	// 二次支付
	goPayFuc: function (citymorderId, entgoodorderid) {
		var newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: citymorderId,
			'type': 1,
		}
		util.PayOrder(newparam, {
			success: function (res) {
				if (res == "wxpay") { } else if (res == "success") {
					tools.showToast("支付成功")
					setTimeout(function () {
						wx.redirectTo({
							url: '../orderDetail/orderDetail?orderId=' + entgoodorderid + "&ispay=1",
						})
					}, 1000)
				}
			},
			failed: function () {
				tools.showLoadToast("您取消了支付")
			},
		})
	},
	//拼团组价请求
	joinRequest: function (pageindex, comindex, currentCom, fpage) {
		let that = fpage
		let [viewmodel, _goodids] = [{}, []];
		let news_com_key = pageindex + "_" + comindex;
		currentCom.items.forEach(function (o, i) {
			_goodids.push(o.id)
		})
		let _postids = _goodids.join(",")
		if (_goodids.length > 0) {
			http.getAsync(
				addr.Address.GetGroupByIds, {
					aid: wx.getStorageSync("aid"),
					ids: _postids,
				})
				.then(function (data) {
					viewmodel.list = data.postdata;
					listviewModel_joingroup_arr[news_com_key] = viewmodel;
					that.setData({ listviewModel_joingroup_arr: listviewModel_joingroup_arr })
				});
		}
	},
	//拼团2组价请求
	joinRequest_2: function (listArray, j, currentCom, fpage) {
		let that = fpage
		let viewmodel = {};
		let _goodids = []
		let [comindex, pageindex] = [j, that.data.isIndex1];
		let news_com_key = pageindex + "_" + comindex;
		currentCom.items.forEach(function (o, i) {
			_goodids.push(o.id)
		})
		let _postids = _goodids.join(",")
		if (_goodids.length > 0) {
			http.getAsync(
				addr.Address.GetEntGroupByIds, {
					aid: wx.getStorageSync('aid'),
					ids: _postids,
				})
				.then(function (data) {
					viewmodel.list = data.postdata;
					listviewModel_entjoingroup_arr[news_com_key] = viewmodel;
					that.setData({ "listviewModel_entjoingroup_arr": listviewModel_entjoingroup_arr })
				});
		}
	},
	//砍价组价请求
	bargainRequest: function (pageindex, comindex, currentCom, fpage) {
		let that = fpage
		let [viewmodel, _goodids] = [{}, []];
		let news_com_key = pageindex + "_" + comindex;
		currentCom.items.forEach(function (o, i) {
			_goodids.push(o.id)
		})
		let _postids = _goodids.join(",")
		if (_goodids.length > 0) {
			http.getAsync(
				addr.Address.GetBargainList, {
					appid: app.globalData.appid,
					ids: _postids,
				})
				.then(function (data) {
					if (data.length > 0) {
						data.forEach(function (_cutprice_item) {
							_cutprice_item.startDateStr = _cutprice_item.startDateStr.replace(/-/g, '/');
							_cutprice_item.endDateStr = _cutprice_item.endDateStr.replace(/-/g, '/');
						});
					}
					viewmodel.list = data;
					listviewModel_cutprice_arr[news_com_key] = viewmodel;
					that.setData({ listviewModel_cutprice_arr: listviewModel_cutprice_arr })
				});
		}
	},
	//产品组件
	goodRequest: function (pageindex, comindex, currentCom, fpage) {
		let that = fpage
		let [viewmodel, _goodids] = [{}, []];
		let news_com_key = pageindex + "_" + comindex;
		currentCom.items.forEach(function (o, i) {
			_goodids.push(o.id)
		})
		let _postids = _goodids.join(",")
		if (_goodids.length > 0) {
			http.getAsync(
				addr.Address.GetGoodsByids, {
					ids: _postids,
					levelid: app.globalData.vipInfo.levelid,
					goodShowType: currentCom.goodShowType
				})
				.then(function (data) {
					if (data.isok) {
						viewmodel.list = data.msg
						// for (let i in viewmodel.list) {
						//   if (viewmodel.list[i].tag == 0) {
						//     viewmodel.list[i].soldNone = true
						//   } else {
						//     viewmodel.list[i].soldNone = false
						//   }
						// }
						viewmodel.ids = _postids
						viewmodel.btnType = currentCom.btnType
						listviewModel_goods_arr[news_com_key] = viewmodel;
						that.setData({ listviewModel_goods_arr: listviewModel_goods_arr })
					}
				})
		}
		else {
			that.setData({ showGoodText: "暂无数据" })
		}
	},
	// 全部新闻
	allNewsRequest: function (typeid, pageindex, comindex, currentCom, fpage) {
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
			.then(function (data) {
				if (data.isok == true) {
					viewmodel.list = data.data;
					currentCom.listmode == 'pick' && currentCom.list.length == 0 && currentCom.num > 0 ?
						viewmodel.list = data.data.slice(0, currentCom.num) : "";
					// 时间戳转换
					viewmodel.list.forEach(function (o, i) {
						o.addtime = util.ChangeDateFormat(o.addtime)
					})
					listViewModal_arr[news_com_key] = viewmodel;
					that.setData({ listViewModal_arr: listViewModal_arr })
				}
			})
	},
	// 选择新闻/
	someNewsRequest: function (pageindex, comindex, currentCom, fpage) {
		let that = fpage
		let [_newsid, viewmodel] = [[], {}];
		let news_com_key = pageindex + "_" + comindex;
		currentCom.list.forEach(function (o, i) {
			_newsid.push(o.id)
		})
		let _newstids = _newsid.join(",");
		if (_newsid.length > 0) {
			http.getAsync(
				addr.Address.GetNewsInfoByids, {
					ids: _newstids,
					liststyle: currentCom.liststyle,
				})
				.then(function (data) {
					if (data.isok == true && data.msg.length > 0) {
						viewmodel.list = data.msg.slice(0, currentCom.num);
						viewmodel.ids = _newstids
						let _temp_newids = [];
						// 时间戳转换
						viewmodel.list.forEach(function (o, i) {
							o.addtime = util.ChangeDateFormat(o.addtime)
						})
						listViewModal_arr[news_com_key] = viewmodel;
						that.setData({ listViewModal_arr: listViewModal_arr })
					}
				});
		}
	},
	// 会员信息
	memberInfo: function (uid, fpage) {
		return new Promise(function (resolve, reject) {
			let that = fpage
			http.getAsync(
				addr.Address.GetVipInfo, {
					appid: app.globalData.appid,
					uid: uid,
				})
				.then(function (data) {
					if (data.isok == true) {
						app.globalData.vipInfo = {
							model: data.model,
							levelid: data.model.levelid,
						}
						wx.setStorageSync("levelid", data.model.levelid)
						resolve(data)
					}
				})
		})
	},
	// 会员信息,拼团2.0调用
	memberInfo_2: function (callback) {
		var that = this
		http.getAsync(
			addr.Address.GetVipInfo, {
				appid: app.globalData.appid,
				uid: app.globalData.userInfo.UserId,
			})
			.then(function (data) {
				if (data.isok == true) {
					var model = data.model
					var levelid = model.levelid
					app.globalData.vipInfo = {
						model: model,
						levelid: levelid,
					}
					if (callback && typeof callback == "function") {
						callback(model);
					}
				}
			})
	},
	// 内容资讯详情
	contentDetail: function (id, fpage) {
		let that = fpage
		http.getAsync(
			addr.Address.GetNewsInfo, {
				id: id,
				version: 2,
			})
			.then(function (data) {
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
	contentMore: function (ids, typeid, fpage) {
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
				.then(function (data) {
					if (data.isok) {
						//设置值
						vm.ispost = false; //请求完毕，关闭请求开关
						vm.loadall = true; //因为是查询选择的内容，一次查询完毕不需要再分页查询
						vm.list = data.msg; //list

						// 时间戳转换 对数据进行格式化
						data.msg.forEach(function (o, i) {
							o.addtime = util.ChangeDateFormat(o.addtime)
						})
						//保存值
						that.setData({ "listViewModel": vm })
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
				.then(function (data) {
					if (data.isok) {
						vm.ispost = false; //请求完毕，关闭请求开关
						//格式化数据
						data.data.forEach(function (o, i) {
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
						that.setData({ "listViewModel": vm })
					}
				})
		}

  },
  //预约列表
  subMore: function (fpage) {
    let that = fpage
    let vm = that.data.listViewModel
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    http
      .getAsync(
      addr.Address.GetSubscribeFormDetail, {
        aid: wx.getStorageSync("aid"),
        uid: app.globalData.userInfo.UserId,
        pageindex: vm.pageindex,
        pagesize: vm.pagesize
      })
      .then(function (data) {
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
          that.setData({ "listViewModel": vm })
          wx.setStorageSync("listViewModel", vm)
        }
      })
  },
  // 预约表单请求判断
  pageData: function (aid) {
    return new Promise(function (resolve, reject) {
      http.getAsync(
        addr.Address.GetPageSetting, {
          aid: aid
        })
        .then(function (data) {
          resolve(data)
        })
    })
  },
  //提交表单
  submitForm: function (formdatajson, remark, fpage) {
    let that = fpage
    let vm = that.data.listFrom
    if (vm.ispost || vm.loadall)
      return;
    if (!vm.ispost)
      vm.ispost = true;
    http
      .postAsync(
      addr.Address.SaveSubscribeForm, {
        aid: wx.getStorageSync("aid"),
        uid: app.globalData.userInfo.UserId,
        formdatajson: formdatajson,
        remark: remark,
      })
      .then(function (data) {
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
  //会员卡更新
  updateCardRequest: function (UserId, fpage) {
    let that = fpage
    http.postAsync(
      addr.Address.UpdateWxCard, {
        appid: app.globalData.appid,
        UserId: UserId,
        type: 2
      }).then(function (data) {
        if (data.msg == "还未生成会员卡(请到后台设置同步微信会员卡)") {
          return;
        } else {
          page.getWXcardRequest(UserId, that)
        }
      })
  },
  // 会员卡请求
  getWXcardRequest: function (UserId, fpage) {
    var that = fpage
    http.getAsync(
      addr.Address.GetWxCardCode, {
        appid: app.globalData.appid,
        UserId: UserId,
        type: 2
      }).then(function (data) {
        let vipCard = false
        if (data.isok) {
          if (data.obj == null) {
            vipCard = true
          } else {
            vipCard = false
          }
        } else {
          vipCard = false
        }
        that.setData({
          vipCard: vipCard
        })
      })
  },
  // 获取会员卡Sign(签名)
  getCardSign: function (UserId, fpage) {
    let that = fpage
    return new Promise(function (resolve, reject) {
      http.getAsync(
        addr.Address.GetCardSign, {
          appid: app.globalData.appid,
          UserId: UserId,
          type: 2
        })
        .then(function (data) {
          resolve(data)
        })
    })
  },
  // 提交code到服务器
  saveCodeRequest: function (code, fpage) {
    let that = fpage
    http.postAsync(
      addr.Address.SaveWxCardCode, {
        appid: app.globalData.appid,
        UserId: app.globalData.userInfo.UserId,
        code: code,
        type: 2
      })
      .then(function (data) {
        if (data.isok) {
          page.updateCardRequest(app.globalData.userInfo.UserId, that)
        }
      })
  },
  // 直播组件产品
  goodsRequest: function (ids, fpage) {
    return new Promise(function (resolve, reject) {
      let that = fpage
      http.getAsync(
        addr.Address.GetGoodsByids, {
          ids: ids,
          levelid: app.globalData.vipInfo.levelid,
          // goodShowType: currentCom.goodShowType
        })
        .then(function (data) {
          resolve(data)
        })
    })
  },
  // 直播地址转换
  getLiveAddress: function (url, fpage) {
    return new Promise(function (resolve, reject) {
      let that = fpage;
      let result = /https?:\/\/vzan.com\/live\/tvchat-(\d+).*/gi.exec(url);
      if (!result) {
        tools.showToast("直播地址不正确")
        return;
      }
      let tpid = result[1];
      http.postAsync(
        addr.Address.live, {
          tpid: tpid
        })
        .then(function (data) {
          resolve(data)
        })
    });
  },
  // 直播添加购物车
  addToCartRequest: function (para) {
    let that = para.fpage
    http
      .postAsync(
      addr.Address.addGoodsCarData, {
        appId: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        goodid: para.goodid,
        attrSpacStr: para.attrSpacStr,
        SpecInfo: para.SpecInfo,
        qty: para.qty,
        newCartRecord: para.newCartRecord
      }
      ).then(function (data) {
        if (data.isok == 1) {
          //page.shopCartData(that)
        } else {
          tools.showToast(data.msg)
        }
      })
  },
  // 判断购物车是否有商品
  shopCartData: function (fpage) {
    let that = fpage
    var shopCartList = []
    http
      .getAsync(
      addr.Address.getGoodsCarData, {
        appId: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        levelid: app.globalData.vipInfo.levelid
      })
      .then(function (data) {
        if (data.isok == 1) {
          let [shopCartList, shopRed] = [data.postdata, false]
          shopCartList != '' ? shopRed = true : shopRed = false
          if (getCurrentPages().length == 1 && getCurrentPages()[0].route == 'pages/index/index') {
            shopCartList = []
          }
          that.setData({
            shopCartList: shopCartList,
            "status._shopRed": shopRed
          })
        }
      })
  },
  // 编辑/删除 购物车(仅删除/更改数量, 规格)请求
  updateShopCar: function (Id) {
    http
      .postAsync(
      addr.Address.updateOrDeleteGoodsCarDataBySingle, {
        appId: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        Id: Id,
        Count: "",
        Function: -1
      })
  },
  // 充值列表请求
  moneyListRequest: function (fpage) {
    let that = fpage
    wx.showLoading({
      title: '加载中',
      mask: true,
      success: function () {
        http
          .getAsync(
          addr.Address.getSaveMoneySetList, {
            appid: app.globalData.appid
          })
          .then(function (data) {
            if (data.isok) {
              var saveMoneySetList = data.saveMoneySetList
              that.setData({
                saveMoneySetList: saveMoneySetList
              })
            } else {
              wx.showToast({
                title: data.msg,
              })
            }
            wx.hideLoading()
          })
      }
    })

  },
  moneyRequest: function (saveMoneySetId) {
    return new Promise(function (resolve, reject) {
      http
        .postAsync(
        addr.Address.addSaveMoneySet, {
          appid: app.globalData.appid,
          openid: app.globalData.userInfo.openId,
          saveMoneySetId: saveMoneySetId
        }).then(function (data) {
          resolve(data)
        })
    })
  },
  // 产品详情页请求
  detailsRequest: function (pid, fpage, leveId) {
    let that = fpage
    http.getAsync(
      addr.Address.GetGoodInfo, {
        pid: pid,
        levelid: wx.getStorageSync("levelid"),
        version: 2,
      })
      .then(function (data) {
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
          app.globalData.IsDistribution = msg.IsDistribution
          app.globalData.DistributionMoney = msg.DistributionMoney
          //动态改标题
          util.navBarTitle(msg.name)
        }
      })
  },
  // 产品详情页购物车添加是否成功请求
  addShopCartRequest: function (para) {
    let that = para.fpage
    // return new Promise(function (resolve, reject) {
    http
      .postAsync(addr.Address.addGoodsCarData, {
        appId: app.globalData.appid,
        openid: app.globalData.userInfo.openId,
        goodid: that.data.pid,
        attrSpacStr: para.attrSpacStr,
        SpecInfo: para.SpecInfo,
        qty: para.qty,
        newCartRecord: para.newCartRecord,
      })
      .then(function (data) {
        if (data.isok == 1) {
          if (that.data.gobuy) {
            that.orderGo(data.cartid)
          } else {
           // page.shopCartData(that)
          }
        } else {
          tools.showToast(data.msg)
        }
      })
    // })
  },
  // 历史充值列表
  getHistoryRequest: function (fpage) {
    let that = fpage
    wx.showLoading({
      title: '加载中',
      mask: true,
      success: function () {
        http.getAsync(
          addr.Address.getSaveMoneySetUserLogList, {
            appid: app.globalData.appid,
            openid: app.globalData.userInfo.openId,
          }).then(function (data) {
            if (data.isok) {
              let saveMoneyUserLogList = data.saveMoneyUserLogList
              that.setData({
                saveMoneyUserLogList: saveMoneyUserLogList
              })
            } else {
              tools.showLoadToast(data.msg)
            }
            wx.hideLoading()
          })
      }
    })
  },
  // 店铺配置
  GetStoreInfo: function (that, isIndex) {
    http.getAsync(
      addr.Address.GetStoreInfo, {
        appId: app.globalData.appid
      })
      .then(function (data) {
        if (data.isok == true) {
          app.globalData.storeConfig = data.postData.storeInfo;
          setTimeout(function () {
            page.GetReductionCardList(that, data.postData.storeInfo.Id, )
          }, 1000)
          console.log("isIndex的值是",isIndex)
          if (isIndex == 0) {
            if (data.postData.storeInfo.funJoinModel.openInvite && that.data.getWay == 0) {
            
              that.data.Address.storeaddress = data.postData.storeInfo.Address
              console.log("页面配置中storeaddress的值", that.data.Address.storeaddress)
            }

						that.setData({
							storeInfo: data.postData.storeInfo,
							Address: that.data.Address
						})
					}
					if (isIndex == 1) {
						wx.setStorageSync('StoreInfo', data)
					}
				}
			})
	},
	// 立减金
	GetReductionCard: function (that, couponsId, orderId, state, cb) {
		http.postAsync(
			addr.Address.GetReductionCard, {
				couponsId: couponsId,
				userId: app.globalData.userInfo.UserId,
				orderId: orderId,
				openId: app.globalData.userInfo.openId
			})
			.then(function (data) {
				if (data.coupon != null) {
					data.coupon.StartUseTimeStr = data.coupon.StartUseTimeStr.replace(/[.]/g, '/')
					data.coupon.EndUseTimeStr = data.coupon.EndUseTimeStr.replace(/[.]/g, '/')
					that.setData({
						coupon: data.coupon,
						userList: data.userList
					})
				}
				if (data.isok == true) {

					if (state == 0) { //getsmoney
						for (var i = 0; i < data.coupon.SatisfyNum; i++) {
							that.data.userLogo.push({
								HeadImgUrl: ''
							})
						}
						for (var j = 0; j < data.userList.length; j++) {
							that.data.userLogo[j].HeadImgUrl = data.userList[j].HeadImgUrl
						}
						that.setData({
							userLogo: that.data.userLogo
						})
					}

					if (state == 1) { //click--invitegetsmoney
						if (data.coupon.SatisfyNum == data.userList.length) {

							if (data.userInfo == null) {
								wx.showToast({
									title: '该活动已满员',
									icon: 'loading'
								})
								return
							} else {
								var finduserid = data.userList.find(f => f.Id == data.userInfo.Id)

								if (finduserid) {
									wx.showToast({
										title: '领取成功'
									})
									setTimeout(function () {
										var coupon = JSON.stringify(that.data.coupon)
										wx.redirectTo({
											url: '/pages/getsmoney/usersmoney?orderId=' + app.globalData.dbOrder
										})
									}, 1000)
								} else {
									wx.showToast({
										title: '该活动已被领取'
									})
								}
							}

						} else {
							wx.redirectTo({
								url: '/pages/getsmoney/getsmoney'
							})
						}
					}

					if (state == 2) { //onload--invitegetsmoney 
						if (data.userInfo == null) {
							wx.showToast({
								title: '该活动已满员',
								icon: 'loading'
							})
							return
						}
						var finduserid = data.userList.find(f => f.Id == data.userInfo.Id)
						if (data.coupon.SatisfyNum > data.userList.length) {
							wx.redirectTo({
								url: '../getsmoney/getsmoney'
							})
							return
						}
						if (finduserid) {
							if (getCurrentPages()[getCurrentPages().length - 1].route != "pages/getsmoney/usersmoney") {
								wx.redirectTo({
									url: '../getsmoney/usersmoney'
								})
							}
						}
					}

				} else {
					if (data.coupon == null) {
						wx.showToast({
							title: '该活动已取消',
							icon: 'loading'
						})
						return
					}
					if (data.userList == undefined) {
						that.setData({
							isGet: true,
							msg: data.msg
						})
					}
					wx.showToast({
						title: data.msg,
						icon: 'loading'
					})
					console.log(data)
				}
			})
	},
	// 查询正在参与的立减金活动
	GetReductionCardList: function (that, storeId) {
		http.postAsync(
			addr.Address.GetReductionCardList, {
				userId: app.globalData.userInfo.UserId,
				openId: app.globalData.userInfo.openId,
				aid: wx.getStorageSync("aid"),
				storeId: storeId,
			})
			.then(function (data) {
				if (data.isok == true) {
					for (var i = 0; i < data.coupons.length; i++) {
						data.coupons[i].StartUseTimeStr = data.coupons[i].StartUseTimeStr.replace(/[.]/g, '/')
						data.coupons[i].EndUseTimeStr = data.coupons[i].EndUseTimeStr.replace(/[.]/g, '/')
					}
					if (data.coupons.length == 0) {
						that.setData({
							"status._haveReductionmoney": false,
							couponsList: data.coupons
						})
					} else {
						that.setData({
							"status._haveReductionmoney": true,
							couponsList: data.coupons
						})
					}
				}
			})
	},
	/****************分销中心*********************/
	getSaleConfig: function () {//.获取小程序分销配置以及当前用户是否成为分销员了
		return new Promise(function (resolve, reject) {
			http.getAsync(
				addr.Address.GetMiniAppSaleManConfig, {
					appId: app.globalData.appid,
					UserId: app.globalData.userInfo.UserId,
				})
				.then(function (data) {
					resolve(data)
				})
		})
	},
	postApply: function (phone) {//申请成为分销员
		return new Promise(function (resolve, reject) {
			http.postAsync(
				addr.Address.ApplySalesman, {
					appId: app.globalData.appid,
					UserId: app.globalData.userInfo.UserId,
					TelePhone: phone
				})
				.then(function (data) {
					resolve(data)
				})
		})
	},
	getRecordUser: function (vm) {//获取分销员累计的客户
		return new Promise(function (resolve, reject) {
			http.getAsync(
				addr.Address.GetSalesManRecordUser, {
					appId: app.globalData.appid,
					UserId: app.globalData.userInfo.UserId,
					pageIndex: vm.pageIndex,
					pageSize: vm.pageSize,
					state: vm.state
				})
				.then(function (data) {

					resolve(data)
				})
		})
	},
	getSaleGoodsList: function (vm) {//获取分销产品
		return new Promise(function (resolve, reject) {
			http.getAsync(
				addr.Address.GetSalesmanGoodsList, {
					appId: app.globalData.appid,
					goodsName: vm.goodsName,
					sortType: vm.sortType,
					pageIndex: vm.pageIndex,
					pageSize: vm.pageSize,
				})
				.then(function (data) {
					resolve(data)
				})
		})
	},
	getRecordId: function (salesManId, goodsId) {//获取推广分享记录Id
		return new Promise(function (resolve, reject) {
			http.getAsync(
				addr.Address.GetSalesManRecord, {
					appId: app.globalData.appid,
					salesManId: salesManId,
					goodsId: goodsId,
				}).then(function (data) {
					resolve(data)
				})
		})
	},
	updateRecordId: function (salesManRecordId) {//更新推广分享记录状态 默认更新为可用 state=1
		return new Promise(function (resolve, reject) {
			http.postAsync(
				addr.Address.UpdateSalesManRecord, {
					appId: app.globalData.appid,
					salesManRecordId: salesManRecordId,
					state: 1,
				}).then(function (data) {
					resolve(data)
				})
		})
	},
	BindRelationShip: function (goodsId, salesManRecordId, userid) {//建立分销员-产品-客户三者之间的关系  当用户点击从分销市场分享出去的商品链接
		return new Promise(function (resolve, reject) {
			http.postAsync(
				addr.Address.BindRelationShip, {
					appId: app.globalData.appid,
					salesManRecordId: salesManRecordId,
					userId: userid,
					goodsId: goodsId,
				}).then(function (data) {
					resolve(data)
				})
		})
	},
	getSaleMan: function (UserId) {//.获取分销员相关信息 各个人分销中心
		return new Promise(function (resolve, reject) {
			http.getAsync(
				addr.Address.GetSalesManUserInfo, {
					appId: app.globalData.appid,
					UserId: UserId,
				}).then(data => {
					resolve(data)
				})
		})
	},
	getRecoderOrder: function (UserId, vm) {//.获取分销推广订单
		return new Promise(function (resolve, reject) {
			http.getAsync(
				addr.Address.GetSalesManRecordOrder, {
					appId: app.globalData.appid,
					UserId: UserId,
					pageIndex: vm.pageIndex,
					pageSize: vm.pageSize,
				}).then(data => {
					resolve(data)
				})
		})
	},
	postCash: function (drawCashMoney) {//申请提现
		return new Promise(function (resolve, reject) {
			http.postAsync(
				addr.Address.DrawCashApply, {
					appId: app.globalData.appid,
					UserId: app.globalData.userInfo.UserId,
					drawCashMoney,
				}).then(data => {
					resolve(data)
				})
		})
	},
	cashList: function (vm) {//获取分销员提现记录
		return new Promise(function (resolve, reject) {
			http.getAsync(
				addr.Address.GetDrawCashApplyList, {
					appId: app.globalData.appid,
					UserId: app.globalData.userInfo.UserId,
					pageIndex: vm.pageIndex,
					pageSize: vm.pageSize,
				}).then(data => {
					resolve(data)
				})
		})
	},

	GetUserInSortQueuesPlanMsg: function (that) {
		wx.request({
			url: addr.Address.GetUserInSortQueuesPlanMsg,
			data: {
				appid: getApp().globalData.appid,
				aId: wx.getStorageSync('aid'),
				storeId: wx.getStorageSync('StoreInfo').postData.storeInfo.Id,
				userId: getApp().globalData.userInfo.UserId
			},
			method: "POST",
			header: {
				"content-type": "application/x-www-form-urlencoded"
			},
			success: function (res) {
				if (res.data.isok == true) {
					that.setData({ isonOrder: res.data.code == 0 ? false : true, dataObj: res.data.dataObj, numsindex: res.data.code > 0 ? res.data.dataObj.sortQueue.pCount : 0 })
				}
			},
			fail: function () {
				console.log("获取当前队列位置信息出错")
			}
		})
	},

	PutSortQueueMsg: function (that, telePhone, cb) {
		wx.request({
			url: addr.Address.PutSortQueueMsg,
			data: {
				appid: getApp().globalData.appid,
				aId: wx.getStorageSync('aid'),
				storeId: wx.getStorageSync('StoreInfo').postData.storeInfo.Id,
				userId: getApp().globalData.userInfo.UserId,
				pCount: 0,
				telePhone: telePhone,
				pageType: 22,
			},
			method: "POST",
			header: {
				"content-type": "application/x-www-form-urlencoded"
			},
			success: function (res) {
				if (res.data.isok == true) {
					wx.showToast({ title: '取号成功！', })
					cb('true')
				} else {
					wx.showModal({ title: '提示', content: res.data.Msg })
				}
			},
			fail: function () {
				console.log("申请取号出错")
			}
		})
	},

	CancelSortQueue: function (that, sortId, cb) {
		wx.request({
			url: addr.Address.CancelSortQueue,
			data: {
				appid: getApp().globalData.appid,
				aId: wx.getStorageSync('aid'),
				storeId: wx.getStorageSync('StoreInfo').postData.storeInfo.Id,
				sortId: sortId
			},
			method: "POST",
			header: {
				"content-type": "application/x-www-form-urlencoded"
			},
			success: function (res) {
				if (res.data.isok == true) {
					wx.showToast({ title: res.data.Msg })
					cb('true')
				}
			},
			fail: function () {
				console.log("取消排队出错")
			}
		})
	},
}
module.exports = page