// pages/detail/detail.js
const util = require("../../utils/util.js");
const addr = require("../../utils/addr.js");
const http = require('../../utils/http.js');
const WxParse = require('../../utils/wxParse/wxParse.js');
const animation = require("../../utils/animation.js");
const page = require("../../utils/pageRequest.js");
const tools = require("../../utils/tools.js");
import { core}  from "../../utils/core.js";
let _get = require('../../utils/lodash.get');
const app = getApp();
Page({
  /**
   * 页面的初始数据
   */
	data: {
		clientTel: '未绑定',//普通商品新增getphonenumber2018.3.7-->mxj

		no: [{
			content: '暂无介绍'
		}],
		pickspecification: [],//规格集合
		specificationdetail: [],//多规格后的价格
		//单价
		discountPrice: 0,
		totalCount: 1,
		showModalStatus: false,
		sel: false,
		attrSpacStr: [],
		self_arr: [],
		oldprice: 0,
		pid: '',
		condition: false, // 分享转发页面
		imSwitch: false,
		status: {},
		popinfo: {
			openWxShopMessage: false,
			headImg: '',
			nickName: '',
			modalClose: false,
			mark: false,
		}
	},

  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		let appid = app.globalData.appid;
		let that = this
		core.GetPageSetting().then(function (pageset) {
			var PageSetting = wx.getStorageSync("PageSetting");
			console.log("PageSetting", PageSetting)
			that.data.popinfo.openWxShopMessage = PageSetting.extraConfig.openWxShopMessage
			console.log("that.data.popinfo.openWxShopMessage", that.data.popinfo.openWxShopMessage)
			that.data.popinfo.nickName = PageSetting.appConfig.nick_name
			that.data.popinfo.headImg = PageSetting.appConfig.head_img
			that.data.popinfo.modalClose = false;
			that.data.popinfo.mark = wx.getStorageSync("mark");
			that.setData({
				popinfo: that.data.popinfo
			})

			util.setPageSkin(that);
		});



		let scene = options.scene || "";
		let [_array, status] = [[], that.data.status]
		if (scene) {
			_array = decodeURIComponent(options.scene)
			_array = _array.split("_")
		}
		let typeName = options.typeName || _array[2] || "";//类型判断
		let showprice = options.showprice || _array[3] == 1 ? 'true' : 'false' || "";//是否显示价格
		let pid = options.id || _array[0];//详情Id
		let salesManRecordId = _array[1] || 0//分销id

		salesManRecordId = Number(salesManRecordId)

		getApp().GetStoreConfig(function (config) {
			if (config && config.funJoinModel) {
				if (salesManRecordId > 0) {
					that.data.status.senceOpen = 1
					that.data.status._sellShow = true
					// var sellShow = 'true'
				} else {
					config.funJoinModel.productQrcodeSwitch == true ? that.data.status.senceOpen = 1 : that.data.status.senceOpen = 0
					options.sellShow == 'true' ? that.data.status._sellShow = true : that.data.status._sellShow = false
					// var sellShow = options.sellShow || "";//是否分销					
				}
				that.setData({
					imSwitch: (config.funJoinModel.imSwitch && config.kfInfo),
				});
			}
		});

		app.getUserInfo(res => {
			page.memberInfo(res.UserId, that).then(data => {
				page.detailsRequest(pid, that)
				if (typeName) {
					if (typeName == 'buy') {
						if (that.data.status._sellShow) {
							// status._sellShow = true
							this.data.sellId = wx.getStorageSync("salesManId") || app.globalData.salesManId

							if (scene) {
								that.data.salesManRecordId = salesManRecordId
							} else {
								page.getRecordId(that.data.sellId, pid).then(data => {
									if (data.isok) {
										that.data.salesManRecordId = data.obj
									} else {
										that.data.salesManRecordId = 0
										tools.ShowMsg(data.msg)
									}
								})
								that.data.status.senceOpen = 1
							}


						} else {
							// status._sellShow = false
							that.data.salesManRecordId = salesManRecordId
						}
						status._buyShow = true
						page.shopCartData(that)
					} else {
						status._buyShow = false
					}
				} else {
					status._buyShow = undefined
				}
				if (showprice == "true") {
					status._showPrice = true
				} else {
					status._showPrice = false
				}
				salesManRecordId ? page.BindRelationShip(pid, salesManRecordId, res.UserId) : ''
				that.setData({ status: that.data.status, clientTel: wx.getStorageSync('userInfo').TelePhone })

				that.data.pid = pid
				that.data.typeName = typeName
				that.data.showprice = showprice
			})
		})



	},

	// 立即购买跳转订单页面
	orderGo: function (cartid) {
		let that = this
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
		that.data.datajson = jsonstr
		that.setData({ showModalStatus: false })
		wx.navigateTo({
			url: '../orderList/orderList?discountTotal=' + that.data.discountTotal + "&datajson=" + that.data.datajson + "&goodCarIdStr=" + cartid,
		})
	},
	// 弹起购物车选择
	shopCarShow: function (e) {
		let that = this
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		let [id, addshop, gobuy] = [Number(e.currentTarget.id), false, false]
		let pickspecification = that.data.pickspecification
		for (let i = 0, valPic; valPic = pickspecification[i++];) {
			for (let j = 0, keyPic; keyPic = valPic.items[j++];) {
				if (keyPic.sel) {
					keyPic.sel = false
				}
			}
		}
		switch (id) {
			case 0:
				addshop = true; gobuy = false;
				break;
			case 1:
				addshop = false; gobuy = true;
				break;
		}
		that.setData({
			specInfo: "",
			totalCount: 1,
			gobuy: gobuy,
			addshop: addshop,
			oldprice: that.data.msg.priceStr,
			pickspecification: pickspecification,
			discountTotal: that.data.msg.discountPricestr,
		})
		animation.utilUp("open", this);
	},
	//隐藏购物车弹窗
	hiddenShow: function (e) {
		animation.utilUp("close", this)
	},
	// 选择商品属性点击事件
	chooseFunc: function (e) {
		let ds = e.currentTarget.dataset
		let [parentindex, childindex] = [ds.parentindex, ds.childindex]
		let [stock, chooseprice, oldprice, discountTotal] = [0, 0, 0, 0]
		let [attrSpacStr, specInfo, template, spec, pick] = [[], [], [], this.data.specificationdetail, this.data.pickspecification]
		let currentList = pick[parentindex];
		let self = pick[parentindex].items[childindex]
		let key = "pickspecification[" + parentindex + "]"

		if (currentList.items.length > 0) {
			currentList.items.forEach(function (obj, i) {
				obj.id != self.id ? obj.sel = false : obj.sel = !obj.sel;
			})
		}

		for (let i = 0, val; val = pick[i++];) {
			for (let j = 0, valKey; valKey = val.items[j++];) {
				if (valKey.sel) {
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
		// 从specificationdetail拿取相对应的价格以及库存
		template = spec.find(f => f.id == attrSpacStr)
		if (template) {
			stock = template.stock
			chooseprice = template.discountPrice
			oldprice = parseFloat(template.price).toFixed(2)
			this.data.discountPrice = parseFloat(chooseprice).toFixed(2)
		}

		discountTotal = parseFloat(chooseprice || this.data.msg.discountPrice).toFixed(2)
		this.setData({
			stock: stock,
			totalCount: 1,//切换选择规格时重置选择数量
			oldprice: oldprice,
			specInfo: specInfo,
			[key]: currentList,
			chooseprice: chooseprice,
			attrSpacStr: attrSpacStr,
			discountTotal: discountTotal,
		})
	},
	//购物车防空逻辑
	addShopCartFunc: function (e) {
		let that = this
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		let [attrSpacStr, specInfo, qty] = [that.data.attrSpacStr, that.data.specInfo, that.data.totalCount]
		let [pickspecification, specificationdetail] = [that.data.pickspecification, that.data.specificationdetail]
		let templath_id = specificationdetail.find(k => k.id == attrSpacStr)
		//多规格产品
		if (pickspecification.length) {
			if (attrSpacStr == '' || attrSpacStr == undefined) {
				// 未选择任何产品提醒
				tools.showLoadToast("请选择商品规格")
				return;
			}
			// 每项选择选择判断
			if (templath_id == undefined) {
				tools.showLoadToast("请选择规格")
				return;
			}
			else {
				if (that.data.msg.stockLimit) {
					if (templath_id.stock) {
						// 添加购物车
						if (that.data.addshop) {
							let para = {
								attrSpacStr: attrSpacStr,
								SpecInfo: specInfo,
								qty: qty,
								newCartRecord: 0,
								fpage: that
							}
							// 添加成功提醒
							tools.showToast("添加成功")
							page.addShopCartRequest(para)
							animation.utilUp("close", that)
						}
						// 否则立即购买
						else {
							let para = {
								attrSpacStr: attrSpacStr,
								SpecInfo: specInfo,
								qty: qty,
								newCartRecord: 1,
								fpage: that
							}
							page.addShopCartRequest(para)
						}
					}
					else {
						// 库存不足提醒
						tools.showLoadToast("亲,库存不足")

					}
				}
				else {
					// 添加购物车
					if (that.data.addshop) {
						// 添加成功提醒
						tools.showToast("添加成功")
						let para = {
							attrSpacStr: attrSpacStr,
							SpecInfo: specInfo,
							qty: qty,
							newCartRecord: 0,
							fpage: that
						}
						page.addShopCartRequest(para)
						animation.utilUp("close", that)
					}
					// 否则立即购买
					else {
						let para = {
							attrSpacStr: attrSpacStr,
							SpecInfo: specInfo,
							qty: qty,
							newCartRecord: 1,
							fpage: that
						}
						page.addShopCartRequest(para)
					}
				}

			}
		}
		//无规格产品
		else {
			if (that.data.msg.stockLimit) {
				if (Number(that.data.msg.stock) < Number(that.data.totalCount)) {
					tools.showLoadToast("亲,库存不足");
					return;
				}
			}

			// 添加购物车
			if (that.data.addshop) {
				// 添加成功提醒
				tools.showToast("添加成功")
				let para = {
					attrSpacStr: "",
					SpecInfo: "",
					qty: qty,
					newCartRecord: 0,
					fpage: that
				}
				animation.utilUp("close", that)
				page.addShopCartRequest(para)
			}
			// 否则立即购买
			else {
				let para = {
					attrSpacStr: "",
					SpecInfo: "",
					qty: qty,
					newCartRecord: 1,
					fpage: that
				}
				page.addShopCartRequest(para)
			}
		}
	},
	// 点击事件 进入编辑状态后 “+”号 增加商品数量
	addFunc: function () {
		let [count, stock] = [this.data.totalCount, this.data.stock]
		if (this.data.pickspecification.length != 0 && this.data.attrSpacStr.length == 0) {
			tools.showLoadToast("请选择商品规格")
			return
		}
		else {
			if (this.data.msg.stockLimit == true) { //当前商品是否被限制库存了，默认是true限制，false不限制
				if (count < stock) {
					count++
				}
				else {
					tools.showLoadToast("亲,库存不足");
					return;
				}
			}
			else {
				count++
			}
		}

		if (this.data.pickspecification.length) {
			var discountTotal = parseFloat(this.data.chooseprice * count).toFixed(2)
		}
		else {
			discountTotal = parseFloat(this.data.msg.discountPrice * count).toFixed(2)
		}

		this.setData({
			totalCount: count,
			discountTotal: discountTotal,
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
		if (this.data.pickspecification.length != 0) {
			var discountTotal = parseFloat(this.data.chooseprice * count).toFixed(2)
		} else {
			discountTotal = parseFloat(this.data.msg.discountPrice * count).toFixed(2)
		}
		this.setData({
			totalCount: count,
			discountTotal: discountTotal,
		})
	},
	preview: function (e) {
		let that = this
		let slider = e.currentTarget.dataset.slider
		let img = e.currentTarget.dataset.img
		let index = e.currentTarget.id
		if (slider) {
			util.preViewShow(slider[index], slider)
		}
		else {
			let urls = []
			urls.push(img)
			util.preViewShow(img, urls)
		}
	},

  /**
   * 跳转功能
   */
	// 跳转预约表单
	yuyueGoto: function () {
		tools.goNewPage('../subscribe/subscribe?pid=' + this.data.pid + '&name=' + this.data.msg.name + "&form=true")
	},

	// 跳转到我的购物车
	templGoto: function () {
		tools.goNewPage('../shoppingCart/shoppingCart')
	},
  /**
     * 用户点击右上角分享
     */
	onShareAppMessage: function () {
		let that = this
		let imgUrl = ""
		if (that.data.msg.slideimgs.length == 0) {
			imgUrl = that.data.msg.img
		} else {
			imgUrl = that.data.msg.slideimgs[0]
		}

		return {
			title: that.data.msg.name,
			path: 'pages/detail/detail?scene=' + that.data.pid + "_" + that.data.salesManRecordId + "_" + that.data.status.senceOpen + "&typeName=" + that.data.typeName + "&showprice=" + that.data.showprice,
			imageUrl: imgUrl,
			success: function (res) {
				tools.showToast("转发成功")
				if (that.data.status._sellShow) {
					page.updateRecordId(that.data.salesManRecordId)
				}
			}
		}

	},

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
	onPullDownRefresh: function () {
		let that = this
		tools.showLoadToast("正在刷新")
		setTimeout(function () {
			tools.showToast("刷新成功")
			page.detailsRequest(that.data.pid, that)
		}, 1000)
		wx.stopPullDownRefresh()
	},


	// 获取用户手机号码
	getPhoneNumber: function (e) {
		var that = this
		app.globalData.telEncryptedData = e.detail.encryptedData
		app.globalData.telIv = e.detail.iv
		app.globalData.isgetTel = 1
		app.getUserInfo(function (res) {
			if (res.TelePhone != '未绑定') {
				that.setData({ clientTel: res.TelePhone })
			}
		})
	},

	shareBtn: function () {
		var that = this;
		that.inite3(that.data.salesManRecordId);
		// if (that.data.status._sellShow) {
		// 	page.getRecordId(that.data.sellId, that.data.pid).then(data => {
		// 		if (data.isok) {
		// 			that.data.salesManRecordId = data.obj
		// 		} else {
		// 			tools.ShowMsg(data.msg)
		// 		}
		// 	})
		// } else {
		// 	that.inite3(0);
		// }
	},

  //请求二维码
  inite3: function (sallId) {
    var that = this
    wx.request({
      url: addr.Address.GetProductQrcode,
      data: {
        appId: app.globalData.appid,
        pid: that.data.pid,
        showQrcode: that.data.status.senceOpen,
        version: 2,
        recordId: sallId,
        typeName: that.data.typeName,
        showprice: that.data.showprice,
      },
      method: "GET",
      header: {
        'content-type': 'application/json' // 默认值
      },
      success: function (data) {
        var qrcode = _get(data,"data.dataObj.qrCode","");
        var goodimg = _get(that.data,"msg.img","")
        if (qrcode&&qrcode.indexOf('https')==-1){
          qrcode = qrcode.replace(/^http/, "https")
        }
        if (goodimg&&goodimg.indexOf('https')==-1){
          goodimg = goodimg.replace(/^http/, "https");
        }
        
        if (data.data.isok) {
          that.data.condition = true
          that.setData({ condition: true, })
          wx.downloadFile({
            url: qrcode, //下载小程序码图片
            success: function (res0) {
              wx.downloadFile({
                url: goodimg,   //下载商品图片
                success: function (res) {
                  var windowWidth = wx.getSystemInfoSync().windowWidth  // 屏幕宽度
                  var windowHeight = wx.getSystemInfoSync().windowHeight // 屏幕高度
                  var context = wx.createCanvasContext('sharingCanvas')
                  var bgImg = '/image/a38.png'
                  var objImgUrl = res.tempFilePath
                  var code = res0.tempFilePath
                  //对比较长的标题进行字符分割，每行显示9个，最多显示三行
                  var title = that.data.msg.name
                  var title1 = title.substr(0, 9)
                  var title2 = title.substr(9, 9)
                  var title3 = title.substr(18, 9)
                  var oldtitle = "原价"
                  var newtitle = "现价"
                  var money = "￥"
                  var oldprice = "￥" + that.data.msg.priceStr
                  var length = oldprice.length
                  var discountTotal = that.data.msg.discountPrice
                  var tip = "长按查看商品"
                  //在画布上进行绘制
                  context.drawImage(objImgUrl, windowWidth * 0.05, 0, windowWidth * 0.8, windowWidth * 0.8) // 画商品图片
                  context.setFillStyle('white')
                  context.fillRect(windowWidth * 0.05, windowWidth * 0.8, windowWidth * 0.8, windowHeight * 0.27)  //画图片下方的区域
                  context.drawImage(code, windowWidth * 0.52, windowWidth * 0.82, windowWidth * 0.27, windowWidth * 0.27)  //画小程序码
                  //画商品的标题
                  //第一行标题
                  context.setFontSize(16)
                  context.setFillStyle("#333333")
                  context.fillText(title1, windowWidth * 0.1, windowWidth * 0.88)
                  //第二行标题
                  context.fillText(title2, windowWidth * 0.1, windowWidth * 0.93)
                  //第三行标题
                  context.fillText(title3, windowWidth * 0.1, windowWidth * 0.98)
                  //画商品的原价
                  context.setFontSize(14)
                  context.setFillStyle("#9C9C9C")
                  context.fillText(oldtitle, windowWidth * 0.1, windowWidth * 1.05)
                  context.fillText(oldprice, windowWidth * 0.2, windowWidth * 1.05)
                  //商品的现价
                  context.fillText(newtitle, windowWidth * 0.1, windowWidth * 1.12)
                  context.setFontSize(22)
                  context.setFillStyle("#FF6700")                               
                  context.fillText(discountTotal, windowWidth * 0.25, windowWidth * 1.12)
                  //
                  context.setFontSize(12)
                  context.setFillStyle("#FF6700")
                  context.fillText(tip, windowWidth * 0.56, windowWidth * 1.12)
                  context.setFontSize(14)
                  context.setFillStyle("#FF6700")
                  context.fillText(money, windowWidth * 0.2, windowWidth * 1.12)  

									context.draw()

								}
							})
						}
					})
				}
			}, fail: function () {
				tools.showToast("获取信息出错")
			}

		})


	},


	//关闭分享页面
	cancelSharing: function () {
		this.setData({ condition: !this.data.condition, })
	},


	// 保存画布的图片
	canvasToTempFilePath: function (e) {
		let that = this
		wx.canvasToTempFilePath({
			x: 0,
			y: 0,
			width: wx.getSystemInfoSync().windowWidth * 0.9,
			height: wx.getSystemInfoSync().windowHeight * 0.8,
			destWidth: 650,
			destHeight: 880,
			canvasId: 'sharingCanvas',
			success: function (res) {
				wx.saveImageToPhotosAlbum({
					filePath: res.tempFilePath,
					success(res) {

						if (that.data.status._sellShow) {
							page.updateRecordId(that.data.salesManRecordId).then(data => {
								console.log("打印数据", data)
							})
						}

						wx.showToast({
							title: '图片保存成功',
						})


					}
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
		that.data.popinfo.modalClose = true
		that.setData({
			popinfo: that.data.popinfo
		})
	},

})