var http = require("../../utils/http.js");
var tools = require("../../utils/tools.js");
var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var WxParse = require("../../utils/wxParse/wxParse.js");
const animation = require("../../utils/animation.js");
const page = require("../../utils/pageRequest.js");
var app = getApp();

var timer_countdown = null;
var isEndClock = null;
import { core } from "../../utils/core.js";
Page({

  /**
   * 页面的初始数据
   */
	data: {
		clientTel: 0,
		groupstate: '',
		currentGroupImg: 1,
		//skudata
		pickspecification: [],//规格集合
		specificationdetail: [],//多规格后的价格
		msg: [],//商品详情
		//单价
		discountPrice: 0,
		totalCount: 1,
		showModalStatus: false,
		sel: false,
		attrSpacStr: [],
		self_arr: [],
		addshop: false,
		gobuy: false,
		logoShow: false,//购物车是否有商品
		oldprice: 0,
		isgroup: 0,
		showmoreGroup: false,
		titletype: 0,
		imSwitch: false,
		popinfo: {
			openWxShopMessage: false,
			headImg: '',
			nickName: '',
			modalClose: false,
			mark: false,
		}
	},
	change_titletype: function (e) {
		this.setData({ titletype: e.currentTarget.id })
	},
	// 获取用户手机号码
	getPhoneNumber: function (e) {
		var that = this
		console.log(e.detail.errMsg)
		console.log(e.detail.iv)
		console.log(e.detail.encryptedData)
		app.globalData.telEncryptedData = e.detail.encryptedData
		app.globalData.telIv = e.detail.iv
		app.globalData.isgetTel = 1
		app.getUserInfo(function (res) {
			if (res.TelePhone != '未绑定') {
				that.setData({ clientTel: res.TelePhone })
			}
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {

		let that = this
		core.GetPageSetting().then(function (pageset) {
			var PageSetting = wx.getStorageSync("PageSetting");
			that.data.popinfo.openWxShopMessage = PageSetting.extraConfig.openWxShopMessage
			that.data.popinfo.nickName = PageSetting.appConfig.nick_name
			that.data.popinfo.headImg = PageSetting.appConfig.head_img
			that.data.popinfo.modalClose = false;
			that.data.popinfo.mark = wx.getStorageSync("mark");
			that.setData({
				popinfo: that.data.popinfo
			})

			util.setPageSkin(that);
		});

		app.getUserInfo(function (uinfo) {
			that.setData({ clientTel: uinfo.TelePhone })
		})

		var pageIndex = Number(options.pageIndex) || 0;
		that.setData({
			pid: options.pid,
			pageIndex: pageIndex,
		});

		that.specificationRequest()


		getApp().GetStoreConfig(function (config) {
			if (config && config.funJoinModel) {
				that.setData({
					imSwitch: config.funJoinModel.imSwitch && config.kfInfo
				});
			}
		});
	},
	navtoG2Detail: function (e) {
		tools.goNewPage('../group2/groupDetail?pid=' + e.currentTarget.dataset.goodid + '&groupid=' + e.currentTarget.dataset.groupid)
	},
	showmore: function () {
		this.setData({ showmoreGroup: !this.data.showmoreGroup })
	},
	// 初始化
	specificationRequest: function () {
		let that = this
		http.getAsync(
			addr.Address.GetGoodInfo,
			{
				pid: that.data.pid,
				levelid: app.globalData.vipInfo.levelid || "",
				version: 2,
			})
			.then(function (data) {
				if (data.isok == true) {
					let msg = data.msg

					var GroupSponsorList = msg.EntGroups.GroupSponsorList
					for (let z = 0; z < GroupSponsorList.length; z++) {
						if (GroupSponsorList[z].SponsorUserId == wx.getStorageSync('userInfo').UserId) {
							GroupSponsorList.splice(z, 1)
							z--
						}
					}

					let slideimgs = data.msg.slideimgs.split(",")
					if (msg.pickspecification != '') {
						var pickspecification = JSON.parse(msg.pickspecification)
						for (let i = 0, val; val = pickspecification[i++];) {
							for (let j = 0, key; key = val.items[j++];) {
								key.sel = that.data.sel
							}
						}
					}
					if (msg.specificationdetail != '') {
						var specificationdetail = JSON.parse(msg.specificationdetail)
					}
					app.globalData.IsDistribution = msg.IsDistribution
					app.globalData.DistributionMoney = msg.DistributionMoney
					msg.discountPrice = parseFloat(msg.discountPrice).toFixed(2)
					let discountTotal = parseFloat(msg.discountPrice).toFixed(2);
					if (msg.goodtype == 1) {
						discountTotal = msg.discountGroupPrice
					}
					that.data.aid = msg.aid
					that.data.discountPrice = msg.price//初始折扣价格
					that.data.specificationdetail = specificationdetail//属性
					// 替换富文本标签 控制样式
					data.msg.description = data.msg.description.replace(/[<]br[/][>]/g, '<div style=\"height:20px\"></div>')
					data.msg.description = data.msg.description.replace(/&nbsp;/g, '<span style=\"margin-left:16rpx;\"></span>')
					data.msg.description = data.msg.description.replace(/[<][/]p[>][<]p[>]/g, '<div></div>')
					that.setData({
						GroupSponsorList: GroupSponsorList,
						stock: msg.stock,//初始库存
						discountTotal: discountTotal,//初始弹窗价格
						oldprice: (msg.price).toFixed(2),//原始价格
						pickspecification: pickspecification,
						msg: msg,
						slideimgs: slideimgs,
						slideimgs_fmt: data.msg.slideimgs_fmt.split("|"),
						article: WxParse.wxParse('article', 'html', data.msg.description, that, 5),
					})
					//动态改标题
					let tmpTitle = '' + data.msg.name;
					//utils.navBarTitle(tmpTitle)
					//utils.setPageSink(that, app.globalData.pages[that.data.isIndex1].skin);
				}
			})
	},
	// 查看轮播大图
	previewSwiper: function (e) {
		var index = e.currentTarget.id;
		var that = this;
		wx.previewImage({
			current: this.data.slideimgs[index],
			urls: this.data.slideimgs
		})
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
		var that = this
		setTimeout(function () {
			that.data.Timeinter = setInterval(function () {
				if (that.data.GroupSponsorList.length > 0) {
					var nowTime = new Date().getTime()
					for (var i = 0; i < that.data.GroupSponsorList.length; i++) {
						var endtimeList = ((new Date(that.data.GroupSponsorList[i].ShowEndTime)).getTime() - nowTime)
						that.data.GroupSponsorList[i].endtimeList = tools.formatDuring(endtimeList)
					}
					that.setData({ GroupSponsorList: that.data.GroupSponsorList })
				}
			}, 1000)
		}, 1000)
		//this.initGroupInfo();
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
		clearInterval(this.data.Timeinter)
	},

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
	onPullDownRefresh: function () {

	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {

	},
  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {
		var that = this
		return {
			title: that.data.msg.name,
			path: 'pages/group/groupDetail?pid=' + that.data.pid,
			imageUrl: that.data.msg.img
		}
	},

	initGroupInfo: function () {
		var that = this;
		var groupid = Number(that.data.id) || 0;
		if (groupid <= 0) {
			wx.showModal({
				title: '错误提示',
				content: '团购不存在！',
			})
			return;
		}


		http
			.postAsync(addr.Address.GetGoodInfo,
			{
				appId: app.globalData.appid,
				pid: groupid,
			})
			.then(function (data) {
				console.log(data);
				if (!data.isok) {
					tools.alert("信息", data.msg);
					return;
				}

				var _p = data.msg;
				//选取前5个用户
				// if (_g.GroupUserList && _g.GroupUserList.length > 0) {
				//   _g.GroupUserList = _g.GroupUserList.slice(0, 5)
				// }
				//选取两个可以参加的团
				// if (_g.GroupSponsorList && _g.GroupSponsorList.length > 0) {
				//   _g.GroupSponsorList = _g.GroupSponsorList.slice(0, 2)
				// }
				//转换富文本
				_p.description = WxParse.wxParse('description', 'html', _p.description || "", that, 5)

				_p.EntGroups.ValidDateStartStr = _p.EntGroups.ValidDateStartStr.replace(/-/g, "/");
				_p.EntGroups.ValidDateEndStr = _p.EntGroups.ValidDateEndStr.replace(/-/g, "/");

				//距离结束倒计时
				//tools.initEndClock(_p.EntGroups.ValidDateStartStr, _p.EntGroups.ValidDateEndStr, that);
				if (_p.slideimgs != "") {
					_p.ImgList = _p.slideimgs.split(",");
				}
				//保存
				that.setData({
					groupdetail: _p,
				});
				//that.initCountDown();
			});
	},
	//点击：商品详情/拼团规则
	clickTab: function (e) {
		this.data.tab.forEach(function (o, i) {
			o.sel = false;
		});
		this.data.tab[e.currentTarget.dataset.index].sel = true;
		var key = "tab[" + e.currentTarget.dataset.index + "].sel";
		this.setData({
			tab: this.data.tab
		});

	},

	getUserInfo: function () {
		var _user = app.globalData.userInfo;
		return new Promise(function (resolve, reject) {
			if (!_user.UserId) {
				app.getUserInfo(function (uinfo) {
					resolve(uinfo);
				})
			}
			else {
				resolve(_user);
			}
		})

	},
	hiddenShow: function (e) {
		animation.utilUp("close", this)
		for (var i = 0; i < this.data.pickspecification.length; i++) {
			for (var j = 0; j < this.data.pickspecification[i].items.length; j++) {
				this.data.pickspecification[i].items[j].sel = false
			}
		}
		this.setData({ specInfo: "", pickspecification: this.data.pickspecification, totalCount: 1 })
	},
	//一键拼团
	clidkAddGroup: function (e) {
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, this)
		var _groupid = e.currentTarget.dataset.groupid;
		// wx.navigateTo({
		//   url: '/pages/groupOrder/groupOrder?groupid=' + _groupid + "&isGroup=1&isGHead=1&gsid=0" + "&isIndex1=" + this.data.isIndex1,
		// })
		var that = this;
		that.data.joinGroup = 1
		animation.utilUp("open", this);
		var that = this;
		this.setData({
			gobuy: true,
			addshop: false,
			isgroup: 1,
			groupid: 0,
			goodtype: that.data.msg.goodtype,
			discountTotal: that.data.msg.EntGroups.GroupPriceStr,
			oldprice: that.data.msg.EntGroups.OriginalPriceStr,
			chooseprice: that.data.msg.EntGroups.GroupPriceStr
		})


	},
	//单独购买
	clidkAddGroupSingle: function (e) {
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, this)
		var _groupid = e.currentTarget.dataset.groupid;
		// wx.navigateTo({
		//   url: '/pages/groupOrder/groupOrder?groupid=' + _groupid + "&isGroup=0&isGHead=0&gsid=0" + "&isIndex1=" + this.data.isIndex1,
		// })
		animation.utilUp("open", this);
		var that = this;
		that.data.joinGroup = 0
		this.setData({
			gobuy: true,
			addshop: false,
			isgroup: 0,
			groupid: 0,
			goodtype: that.data.msg.goodtype,
			discountTotal: that.data.msg.priceStr,
			oldprice: that.data.msg.EntGroups.OriginalPriceStr,
			chooseprice: that.data.msg.priceStr
		})
	},
	joinGroup: function (e) {
		var _ds = e.currentTarget.dataset.group;
		var _groupid = _ds.GroupId;
		var _gsid = _ds.Id;
		wx.navigateTo({
			url: '/pages/groupOrder/groupInvite?groupid=' + _groupid + "&isGroup=1&isGHead=0&gsid=" + _gsid,
		})
	},
	initCountDown: function () {
		var that = this;
		if (this.data.groupdetail.GroupSponsorList != null) {
			var list = this.data.groupdetail.GroupSponsorList;
			if (list.length > 0) {
				for (var i = list.length - 1; i >= 0; i--) {
					if (list[i].MState == 1) {

					}

					var timespan = tools.getTimeSpan(list[i].ShowEndTime);
					if (timespan <= 0) {
						list.splice(i, 1)
					}
					else {
						var timeFormatArray = tools.formatMillisecond(timespan);
						var timeFormat = "";
						// if (timeFormatArray[0] > 0) {
						//   timeFormat += timeFormatArray[0] + '天';
						// }
						timeFormat += timeFormatArray[1] + ':' + timeFormatArray[2] + ':' + timeFormatArray[3];
						list[i].countdown = timeFormat;
					}



				}
				that.setData({
					"groupdetail.GroupSponsorList": list
				});

				timer_countdown = setTimeout(function () {
					that.initCountDown();
				}, 1000);

			}

		}
	},
	changeGroupImage: function (e) {
		this.setData({
			currentGroupImg: Number(e.detail.current) + 1
		});
	},
	// 选择商品属性点击事件
	chooseFunc: function (e) {
		console.log(e)
		let ds = e.currentTarget.dataset
		let [attrSpacStr, specInfo, parentindex, childindex, specificationdetail, pickspecification] = [[], [], ds.parentindex, ds.childindex, this.data.specificationdetail, this.data.pickspecification]
		let currentList = this.data.pickspecification[parentindex];
		let self = this.data.pickspecification[parentindex].items[childindex]
		let key = "pickspecification[" + parentindex + "]"
		currentList.items.length > 0 ?
			(
				currentList.items.forEach(function (obj, i) {
					obj.id != self.id ? obj.sel = false : obj.sel = !obj.sel;
				})
			) : "";
		for (let i = 0, val; val = pickspecification[i++];) {
			for (let j = 0, valKey; valKey = val.items[j++];) {
				if (valKey.sel == true) {
					attrSpacStr.push(valKey.id)
					let [parentName, childName] = [val.name, valKey.name]
					let specName = parentName + ":" + childName
					specInfo.push(specName)
				}
			}
		}
		//拼接id及名字
		attrSpacStr = attrSpacStr.join("_")
		specInfo = specInfo.join(" ")
		var that = this;
		// 从specificationdetail拿取相对应的价格以及库存
		// for (let k = 0, spec; spec = specificationdetail[k++];) {
		//   if (attrSpacStr == spec.id) {
		//     var stock = spec.stock
		//     let oldpriceChoose = spec.price
		//     var chooseprice = spec.discountPrice;
		//     var oldprice = parseFloat(oldpriceChoose).toFixed(2)

		//     if(that.data.msg.goodtype==1){
		//       chooseprice = spec.discountGroupPrice;
		//       oldprice = spec.discountGroupPrice;
		//     }
		//     this.data.discountPrice = parseFloat(chooseprice).toFixed(2)
		//   }
		// }
		var spec = specificationdetail.find(p => p.id == attrSpacStr);
		var chooseprice = that.data.discountPrice;
		if (spec) {
			var stock = spec.stock
			// var chooseprice = spec.discountPrice;
			// var oldprice = parseFloat(spec.price).toFixed(2)
			if (that.data.msg.goodtype == 1 && that.data.isgroup == 1) {
				that.data.chooseprice = parseFloat(spec.groupPrice).toFixed(2)
			}
			else {
				that.data.chooseprice = that.data.msg.priceStr
			}
			that.data.oldprice = that.data.msg.EntGroups.OriginalPriceStr
			// this.data.discountPrice = parseFloat(chooseprice).toFixed(2)
		}

		// var def_price = this.data.msg.discountPrice;
		// if (that.data.msg.goodtype == 1 && that.data.isgroup == 1) {
		// 	def_price = that.data.msg.discountGroupPrice;
		// }

		// var discountTotal = parseFloat(chooseprice).toFixed(2)
		this.setData({
			[key]: currentList,
			// discountTotal: discountTotal,
			attrSpacStr: attrSpacStr,
			specInfo: specInfo,
			stock: stock,
			totalCount: 1,//切换选择规格时重置选择数量
			chooseprice: that.data.chooseprice,
			oldprice: that.data.oldprice,
			discountTotal: that.data.chooseprice
		})
	},
	// 点击事件 进入编辑状态后 “+”号 增加商品数量
	addFunc: function () {
		let [count, stock] = [this.data.totalCount, this.data.stock]
		var ischooseAll = 0
		for (var i = 0; i < this.data.attrSpacStr.length; i++) {
			if (this.data.attrSpacStr[i] == '_') {
				++ischooseAll
			}
		}
		if (this.data.pickspecification.length != ischooseAll + 1 && this.data.pickspecification.length > 0) {
			tools.showToast("请选择商品规格")
			return
		} else {
			if (this.data.msg.stockLimit == true) { //当前商品是否被限制库存了，默认是true限制，false不限制
				if (count < stock) {
					count++
				} else {
					count = this.data.stock
					tools.showToast("亲,库存不足")
				}
			} else {
				count++
			}
		}


		// if (this.data.pickspecification.length != 0) {
		// 	var discountTotal = parseFloat(this.data.chooseprice * count).toFixed(2)
		// } else {
		// 	discountTotal = parseFloat(this.data.msg.discountPrice * count).toFixed(2)
		// }
		this.data.discountTotal = parseFloat(this.data.chooseprice * count).toFixed(2)
		this.setData({
			totalCount: count,
			discountTotal: this.data.discountTotal,
		})
	},
	// 点击事件 进入编辑状态后 “-”号 减小商品数量
	lessFunc: function (e) {
		let [count, stock] = [this.data.totalCount, this.data.stock]
		if (this.data.msg.stockLimit == true) { //最外层判断有没有库存限制
			if (count > 1) {
				count--
			} else {
				tools.showToast("亲,不要再减啦")
				count = 1
			}
		} else {
			if (count > 1) {
				count--
			} else {
				count = 1
			}
		}
		// if (this.data.pickspecification.length != 0) {
		// 	var discountTotal = parseFloat(this.data.chooseprice * count).toFixed(2)
		// } else {
		// 	discountTotal = parseFloat(this.data.msg.discountPrice * count).toFixed(2)
		// }
		this.data.discountTotal = parseFloat(this.data.chooseprice * count).toFixed(2)

		this.setData({
			totalCount: count,
			discountTotal: this.data.discountTotal,
		})
	},
	//购物车防空逻辑
	addShopCartFunc: function (e) {
		let that = this
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		let [currentStatu, attrSpacStr, specInfo, qty, pickspecification, specificationdetail] = [e.currentTarget.dataset.statu, that.data.attrSpacStr, that.data.specInfo, that.data.totalCount, that.data.pickspecification, that.data.specificationdetail]
		let templath_id = specificationdetail.find(k => k.id == attrSpacStr)
		if (pickspecification.length != 0) {
			if (attrSpacStr == '' || attrSpacStr == undefined) {
				// 未选择任何产品提醒
				tools.showToast("请选择商品规格")
				return;
			}
			// 每项选择选择判断
			if (templath_id == undefined) {
				tools.showToast("请选择规格")
				return;
			}
			else {
				if (that.data.msg.stockLimit == true) {
					if (templath_id.stock != 0) {
						animation.utilUp(currentStatu, that)
						// 添加购物车
						if (that.data.addshop == true) {
							// 添加成功提醒
							tools.showToast("添加成功")
							that.addShopCartRequest(attrSpacStr, specInfo, qty, 0)
						}
						// 否则立即购买
						else {
							that.addShopCartRequest(attrSpacStr, specInfo, qty, 1)
						}
					}
					else {
						// 库存不足提醒
						tools.showToast("亲,库存不足")
					}
				} else {
					animation.utilUp(currentStatu, that)
					// 添加购物车
					if (that.data.addshop == true) {
						// 添加成功提醒
						tools.showToast("添加成功")
						that.addShopCartRequest(attrSpacStr, specInfo, qty, 0)
					}
					// 否则立即购买
					else {
						that.addShopCartRequest(attrSpacStr, specInfo, qty, 1)
					}
				}

			}
		} else {
			// 添加购物车
			if (that.data.addshop == true) {
				// 添加成功提醒
				tools.showToast("添加成功")
				animation.utilUp(currentStatu, that)
				that.addShopCartRequest("", "", qty, 0)
			}
			// 否则立即购买
			else {
				that.addShopCartRequest(attrSpacStr, specInfo, qty, 1)
			}
		}
	},
	// 购物车添加是否成功请求
	addShopCartRequest: function (attrSpacStr, SpecInfo, qty, newCartRecord) {
		var that = this
		http
			.postAsync(addr.Address.addGoodsCarData,
			{
				appId: app.globalData.appid,
				openid: app.globalData.userInfo.openId,
				goodid: that.data.pid,
				attrSpacStr: attrSpacStr,
				SpecInfo: SpecInfo || '',
				qty: qty,
				newCartRecord: newCartRecord,
				levelid: app.globalData.vipInfo.levelid || "",
				isgroup: that.data.joinGroup
			})
			.then(function (data) {
				if (data.isok == 1) {
					if (that.data.gobuy == true) {
						let datas = []
						if (that.data.chooseprice == undefined) {
							that.data.chooseprice = that.data.msg.discountPrice
						}
						datas.push({
							ImgUrl: that.data.msg.img,
							Count: that.data.totalCount,
							oldPrice: that.data.oldprice,
							SpecInfo: that.data.specInfo,
							Introduction: that.data.msg.name,
							discount: that.data.msg.discount,
							discountPrice: (Number(that.data.chooseprice) || 0).toFixed(2),
							goodid: that.data.pid
						})
						let jsonstr = JSON.stringify(datas)

						var parameters = [];
						if (Number(that.data.chooseprice) * that.data.msg.discount / 100 < 0.01) {
							if (that.data.isgroup == 0) {
								parameters.push("discountTotal=" + that.data.totalCount * 0.01)
							} else {
								parameters.push("discountTotal=" + that.data.discountTotal)
							}
						} else {
							parameters.push("discountTotal=" + (parseFloat(Number(Number(that.data.chooseprice) * that.data.msg.discount / 100).toFixed(2)) * that.data.totalCount).toFixed(2))
						}
						parameters.push("datajson=" + jsonstr);
						parameters.push("goodCarIdStr=" + data.cartid);
						parameters.push("isIndex1=" + that.data.isIndex1);
						parameters.push("isgroup=" + that.data.isgroup);
						parameters.push("groupid=" + that.data.groupid);
						parameters.push("goodtype=" + that.data.goodtype);
						parameters.push("headerReducePrice=" + that.data.msg.EntGroups.HeadDeductStr);
						parameters.push("vipdiscount=" + that.data.msg.discount);
						animation.utilUp("close", that)
						that.setData({ totalCount: 1 })
						tools.goNewPage('../orderList/group2_orderList?' + parameters.join("&") + '&joinGroup=' + that.data.joinGroup)
					}
				} else {
					wx.showToast({
						title: data.msg,
					})
				}
			})
	},
	gochat: function () {
		tools.gochat();
	},
	//商品详情弹出框的关闭按钮
	modalClose: function () {
		var that = this
		console.log("关闭商品详情弹出框")
		that.data.popinfo.modalClose = true
		that.setData({
			popinfo: that.data.popinfo
		})
	},
})