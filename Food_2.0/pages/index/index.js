// pages/index/index.js
var util = require("../../utils/util.js");
var network = require("../../utils/network.js");
var addr = require("../../utils/addr.js");
var mulpicker = require("../../public/mulpicker.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		isAllgoods:false,
		pageindex: 1,
		clientTel: 0,//判断客户手机号码是否已绑定 用左渲染
		scrollTop: true,//监听页面滚动高度
		margintop: false,//用作控制顶部动画
		singlePricestr: 0,//原价 用于传值updateorder
		userInfo: [],//用户信息
		AgentConfig: [],//水印开关集合
		InfoIntroduction: '',//商品介绍
		Infoimg: '',//商品详情大图
		Infoname: '',//商品详情名字
		showAdresstips: false,//控制提示选择收货地址框显隐
		menuInfo: false,//商品详情弹窗
		storeInfo: false,//店铺公告弹窗
		edtiorNums: 0,
		edtiorNums1: 0,
		isfinNums: 1,
		inputvalue: '',
		ispayOk: 99,
		FirstInfoCount: 0,
		shopCartlength: 0,//购物车长度
		TablesNo: 0,
		msg: '',//判断是否在配送范围内字段
		goodsid: 0,//菜品id
		shopCar: [],//购物车缓存数据set
		logoimg: '',
		food: [],
		goodsTypeList: [],
		choose0: 0,//左侧scrowllview选择
		goodslist: [],
		goodslist_1: [],//作为还原数组
		goodslist_2: [],//作为还原数组
		chooseNum: [],
		chooseNums: 0,
		initenums: 0,
		openId: '',
		goodsAttrList: [],
		miniappFoodGoods: [],
		allnums: 0,//购物车的商品总数
		allprice: 0,//购物车总价
		// itemprice:0,//购物车里面单个物品的数量*单价
		groupindex1: 0,
		groupindex2: 0,
		groupindex3: 0,
		allgroupindex: '',//所有属性数字串
		selectAttrInfo: '',//所有属性中文
		firstName: '',
		firstInfo: '',    // 第一项属性名字 规格
		secondName: '',
		secondInfo: '',// 第二项属性名字 规格
		thirdInfo: '',// 第三项属性名字 规格
		thirdName: '',
		singleprice: 0,
		goodname: '',//点击单个菜品的名字
		Stock: 0,//商品的库存
		index: 0,//下标
		arrow: 0,//标记
		shopType: 999,
		// 左侧分类
		item1: [],
		// 右侧商品
		item2: [],
		// 显隐框商品
		item3: [],
		// 显隐框商品规格
		item4: [],
		// 购物车数据
		item5: [],
		item5_1: [],
		item5_2: [],
		nums: 0,//已选择商品的数量
		condition: false,//购物车显隐
		condition1: false,//选择商品规格显隐
		// switchC: 2,//1为堂食，2为外卖
		addressInfo: '',
		isFirstClick: true,
		goodsName: '',//用作查询商品的字段
		ka: 0,
		isallNotice: false,
	},
	// 改变condition的值
	setCondition: function (hiddencondition) {
		var allnums = 0
		var allprice = 0
		var item5 = this.data.item5
		var item5_1 = this.data.item5_1
		var item5_2 = this.data.item5_2
		var isFirstClick = this.data.isFirstClick
		if (isFirstClick) {
			for (var i = 0; i < item5.length; i++) {
				if (item5_1.length != 0) {
					item5[i].price = (Number(item5[i].nums) * Number(this.data.item5[i].price))
					allnums += item5[i].nums
					allprice += (item5[i].price)
				}
			}
		} else {
			if (item5.length != 0 && isFirstClick == false) {
				for (var i = 0; i < item5.length; i++) {
					if (this.data.item5_1.length != 0) {
						if (typeof (this.data.item5[i].price) == "string") {
							item5[i].price = (Number(this.data.item5[i].price) * Number(item5[i].nums))
						}
					}
				}
				this.setData({
					item5_1: item5
				})
			}
			for (var i = 0; i < item5.length; i++) {
				allnums += item5[i].nums
				allprice += (item5[i].price)
			}
		}
		allprice = parseFloat(allprice).toFixed(2)

		if (this.data.condition == false) {
			if (hiddencondition == 1) {
				var condition = false
			} else {
				var condition = true
			}
			this.setData({
				condition: condition,
				allprice: allprice,
				allnums: allnums,
				item5: item5,
			})
			if (item5.length != 0) {
				this.setData({ isFirstClick: false })
			}
		}
	},
	// 把item5还原
	setCondition2: function () {
		var item5 = this.data.item5
		this.setData({
			condition: !this.data.condition,
		})
	},
	// 改变condition1的值
	setCondition1: function () {
		var shopCartlength = this.data.shopCartlength
		var index = this.data.index
		var arrow = this.data.arrow
		var edtiorNums = this.data.edtiorNums
		var edtiorNums1 = this.data.edtiorNums1
		var goodslist = this.data.goodslist
		if (goodslist[index].attrList.length > 0) {
			if (edtiorNums > edtiorNums1) {
				var EdtNums = edtiorNums - edtiorNums1
				shopCartlength = shopCartlength - EdtNums
				goodslist[index].good.carCount = goodslist[index].good.carCount - EdtNums
			}
			if (edtiorNums1 > edtiorNums) {
				var EdtNums = edtiorNums1 - edtiorNums
				shopCartlength = shopCartlength + EdtNums
				goodslist[index].good.carCount = goodslist[index].good.carCount + EdtNums
			}
		}
		var allgroupindex = this.data.allgroupindex
		if (this.data.item5.length == 0) {
			shopCartlength = 0
			this.data.goodslist[this.data.chooseNums].good.carCount = 0
		} else {
			shopCartlength = shopCartlength
		}
		this.setData({
			goodslist: goodslist,
			condition1: !this.data.condition1,
			shopCartlength: shopCartlength,
			arrow: 0,
			edtiorNums: 0,
			edtiorNums1: 0,
			firstInfo: "",
			secondInfo: "",
			thirdInfo: "",
			groupindex1: "",
			groupindex2: "",
			groupindex3: "",
			allgroupindex: "",
			selectAttrInfo: "",
		})
	},
	// 监听页面滚动高度
	onPageScroll: function (e) {
		if (e.scrollTop > 175 && this.data.goodslist.length > 4) {
			if (this.data.scrollTop == true) {
				this.setData({ scrollTop: false })
			}
		} else {
			if (this.data.scrollTop == false) {
				this.setData({ scrollTop: true })
			}
		}
	},
	// 改变changeChoose0的值
	changeChoose0: function (e) {
		var index = e.currentTarget.id
		this.data.shopindex = index
		this.data.pageindex = 1
		wx.pageScrollTo({ scrollTop: 0, })
		this.setData({ choose0: index })
		this.GetGoodsList(index, this.data.inputvalue, this.data.shopType, this.data.pageindex, 0)
	},
	// 商品详情弹窗
	changemenuInfo: function (e) {
		var Infoimg = e.currentTarget.dataset.img //商品详情大图
		var Infoname = e.currentTarget.dataset.name //商品详情名字
		var InfoIntroduction = e.currentTarget.dataset.introduction//商品详情介绍
		this.setData({ menuInfo: !this.data.menuInfo, Infoimg: Infoimg, Infoname: Infoname, InfoIntroduction: InfoIntroduction })
	},
	// 跳转到个人中心
	switchTome: function () {
		wx.switchTab({
			url: '../me/me',
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		var addressInfo = app.globalData.addressInfo
		util.getSaveMoneySetUser(that) //获取储值余额
		util.GetAgentConfigInfo(that) //刷新水印
		var shopType = that.data.shopType
		// 通过二维码获取桌位号
		var scene = decodeURIComponent(options.scene)
		if (scene == "undefined") {
			app.globalData.TablesNo = -999
		} else {
			app.globalData.TablesNo = scene
		}
		if (app.globalData.TablesNo == -999) {
			shopType = 1
		} else {
			shopType = 0
		}
		if (wx.getStorageSync('addr') != "" && wx.getStorageSync('msg') != "") {
			app.globalData.addressInfo = wx.getStorageSync('addr')
			that.setData({ msg: wx.getStorageSync('msg') })
		}
		console.log(wx.getStorageSync('weidu')) //缓存下来的纬度
		console.log(wx.getStorageSync('jingdu')) //缓存下来的经度

		if (app.globalData.userInfo.openId == undefined && app.globalData.userInfo.nickName == undefined) {
			app.getUserInfo(function (e) {
				that.setData({ openId: e.openId, addressInfo: addressInfo, TablesNo: app.globalData.TablesNo, shopType: shopType, userInfo: app.globalData.userInfo, clientTel: app.globalData.userInfo.TelePhone })
				that.inite()//获取首页显示数据
				that.inite1()//获取分类列表
				that.GetGoodsList(0, that.data.goodsName, that.data.shopType, that.data.pageindex, 0) //首页菜单列表
			})
		} else {
			that.setData({ addressInfo: addressInfo, TablesNo: app.globalData.TablesNo, shopType: shopType, userInfo: app.globalData.userInfo, clientTel: app.globalData.userInfo.TelePhone })
			that.inite()
			that.inite1()
			that.GetGoodsList(0, that.data.goodsName, that.data.shopType, that.data.pageindex, 0)
		}
		if (that.data.TablesNo == -999) {
			wx.setNavigationBarTitle({
				title: '外卖',
			})
		} else {
			wx.setNavigationBarTitle({
				title: '自助点餐' + '(' + that.data.TablesNo + '号桌)',
			})
		}
		if (app.globalData.addressInfo == '请选择定位信息' && app.globalData.TablesNo == -999) {
			that.setData({ showAdresstips: !that.data.showAdresstips })
		}
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
	resting: function () {
		wx.showToast({
			title: '商家休息中',
			icon: 'loading'
		})
	},
	// 跳转到商店详情页
	navtostoreInfo: function () {
		wx.navigateBack({
			url: '../home/home',
		})
	},
	Localless: function () {
		wx.showToast({
			title: '请先进行定位',
			icon: 'loading'
		})
	},
	Outdistance: function () {
		wx.showToast({
			title: '配送范围外',
			icon: 'loading'
		})
	},
	Stockless: function () {
		if (this.data.food.openState == 0) {
			wx.showToast({
				title: '商家休息中',
				icon: 'loading'
			})
		} else {
			wx.showToast({
				title: '该商品库存不足',
				icon: 'loading'
			})
		}
	},
	// 商店公告弹窗
	showStoreinfo: function () {
		this.setData({ storeInfo: !this.data.storeInfo })
	},
	// 选择对应商品规格 
	chooseInfo: function (e) {
		var that = this
		if (e.currentTarget.id != '') {
			var shopCartlength = that.data.shopCartlength
			var goodslist = that.data.goodslist
			var item5 = that.data.item5
			var chooseNum = that.data.chooseNum
			var index = e.currentTarget.id
			var Id = e.target.dataset.id
			var goodname = goodslist[index].good.GoodsName
			var good = this.data.goodslist[index].good
			var GASDetailList = goodslist[index].good.GASDetailList
			if (GASDetailList.length > 0) {
				if (good.carCount == 0) {
					goodslist[index].good.carCount++
					shopCartlength++
				}
				that.setData({
					singleprice: goodslist[index].good.discountPricestr,
					goodslist: goodslist,
					chooseNums: index,
					condition1: !this.data.condition1,
					goodname: goodname,
					goodsid: Id,
					index: index,
					shopCartlength: shopCartlength
				})
			} else {
				that.addNums(index)
			}
			that.setCondition(1)
		} else {
			// 提交备用formId
			var formId = e.detail.formId
			util.commitFormId(formId, that)
		}
		console.log('我是顺序id', index)
		console.log('我是菜品id', Id)
		console.log('我是菜品名字', goodname)
		console.log(this.data.goodslist[index])
	},
	// 选择对应商品规格
	RchooseInfo: function (e) {
		var that = this
		var goodslist = that.data.goodslist
		var item5 = that.data.item5
		var chooseNum = that.data.chooseNum
		var index = e.currentTarget.id
		var Id = e.target.dataset.id
		var goodname = goodslist[index].good.GoodsName
		var good = this.data.goodslist[index].good
		var GASDetailList = goodslist[index].good.GASDetailList
		// chooseNum.push(index)
		if (GASDetailList.length > 0) {
			that.setData({
				chooseNums: index,
				condition1: !this.data.condition1,
				goodname: goodname,
				goodsid: Id,
				index: index
			})
		} else {
			that.RaddNums(index)
		}
		that.setCondition(1)
		console.log('我是顺序id', index)
		console.log('我是菜品id', Id)
		console.log('我是菜品名字', goodname)
		console.log(this.data.goodslist)
	},
	// 选择商品属性点击事件
	setChoose: function (e) {

		var pid = e.currentTarget.dataset.pid
		// 商品对应id
		var cid = e.currentTarget.id
		if (pid == 0) {
			this.data.groupindex1 = cid
		}
		else if (pid == 1) {
			this.data.groupindex2 = cid
		}
		else {
			this.data.groupindex3 = cid
		}

		var tempdata = this.data.goodslist[this.data.chooseNums].attrList[0].Value[pid].SpecList
		if (this.data.groupindex1 > 0 && pid == 0) {
			// 获取第一条属性 
			this.data.firstName = ""
			this.data.firstInfo = ""
			// var FirstInfo = this.data.goodsAttrList[0].SpecList.find(f => f.Id == this.data.groupindex1)
			var FirstInfo = tempdata.find(f => f.Id == this.data.groupindex1)
			this.data.firstInfo = FirstInfo.SpecName
			this.data.firstName = this.data.goodslist[this.data.chooseNums].attrList[0].Value[pid].AttrName
		}

		if (this.data.groupindex2 > 0 && pid == 1) {
			// 获取第二条条属性 
			this.data.secondName = ""
			this.data.secondInfo = ""
			var SecondInfo = tempdata.find(f => f.Id == this.data.groupindex2)
			this.data.secondInfo = SecondInfo.SpecName
			this.data.secondName = this.data.goodslist[this.data.chooseNums].attrList[0].Value[pid].AttrName
		}

		if (this.data.groupindex3 > 0 && pid == 2) {
			// 获取第三条属性 
			this.data.thirdName = ""
			this.data.thirdInfo = ""
			var ThirdInfo = tempdata.find(f => f.Id == this.data.groupindex3)
			this.data.thirdInfo = ThirdInfo.SpecName
			this.data.thirdName = this.data.goodslist[this.data.chooseNums].attrList[0].Value[pid].AttrName
		}

		// 获取商品属性 allgroupindex 
		var allgroupindex = this.data.allgroupindex
		var selectAttrInfo = this.data.selectAttrInfo

		var tempdata1 = this.data.goodslist[this.data.chooseNums].good.GASDetailList

		if (this.data.groupindex1 > 0) {
			allgroupindex = this.data.groupindex1 + "_"
			selectAttrInfo = this.data.firstName + ":" + this.data.firstInfo + " "
		}
		if (this.data.groupindex2 > 0) {
			allgroupindex += this.data.groupindex2 + "_"
			selectAttrInfo += this.data.secondName + ":" + this.data.secondInfo + " "
		}
		if (this.data.groupindex3 > 0) {
			allgroupindex += this.data.groupindex3 + "_"
			selectAttrInfo += this.data.thirdName + ":" + this.data.thirdInfo + " "
		}
		if (this.data.goodslist[this.data.chooseNums].good.GASDetailList.length > 0) {
			var attrmodel = this.data.goodslist[this.data.chooseNums].good.GASDetailList.find(d => d.id == allgroupindex)
			if (attrmodel != undefined) {
				this.data.singleprice = attrmodel.discountPricestr //读出对应规格下的折扣价格
				this.data.singlePricestr = attrmodel.priceStr //读出对应规格下的价格
				if (this.data.buyamount > attrmodel.count) {
					this.data.buyamount = attrmodel.count
				}
			}
		}
		console.log(this.data)
		console.log('我是attrSpacStr', allgroupindex)
		console.log('我是SpecInfo', selectAttrInfo)
		this.setData({ allgroupindex: allgroupindex, selectAttrInfo: selectAttrInfo })
		this.setData(this.data)
	},
	// 把添加的菜品暂时缓存到购物车
	setStorageCar: function () {
		var edtiorNums = this.data.edtiorNums
		var edtiorNums1 = this.data.edtiorNums1
		var index = this.data.index
		var goodslist = this.data.goodslist
		var firstInfo = this.data.firstInfo
		var secondInfo = this.data.secondInfo
		var thirdInfo = this.data.thirdInfo
		var groupindex1 = this.data.groupindex1
		var groupindex2 = this.data.groupindex2
		var groupindex3 = this.data.groupindex3
		var allgroupindex = this.data.allgroupindex
		var selectAttrInfo = this.data.selectAttrInfo
		var GASDetailList = this.data.goodslist[this.data.chooseNums].good.GASDetailList
		var attrListLength = this.data.goodslist[this.data.chooseNums].attrList[0].Value.length
		var isInfo
		if (attrListLength == 1) { isInfo = this.data.firstInfo == '' || this.data.goodslist[this.data.chooseNums].good.carCount == 0 }
		if (attrListLength == 2) { isInfo = this.data.firstInfo == '' || this.data.secondInfo == '' || this.data.goodslist[this.data.chooseNums].good.carCount == 0 }
		if (attrListLength == 3) { isInfo = this.data.firstInfo == '' || this.data.secondInfo == '' || this.data.thirdInfo == '' || this.data.goodslist[this.data.chooseNums].good.carCount == 0 }
		if (isInfo) {
			wx.showToast({
				icon: 'loading',
				title: '选择规格或数量',
			})
		}
		else {
			var shopCartlength = 0
			var item5 = this.data.item5
			if (item5.length > 0) {
				for (var i = 0; i < item5.length; i++) {
					if (item5[i].name == this.data.goodname && item5[i].SpecInfo == this.data.selectAttrInfo && item[i].goodsid == this.data.goodslist[index].Id) {
						item5.splice(i, 1)
					}
				}
			}
			var findStock = goodslist[Number(this.data.index)].good.GASDetailList.find(f => f.id == allgroupindex)
			if (findStock.count == 0) {
				wx.showToast({
					title: '库存不足',
					icon: 'loading'
				})
				return
			}
			// shopCartlength++
			var price = parseFloat(this.data.singleprice).toFixed(2)
			item5.push({ name: this.data.goodname, size: firstInfo + ' ' + secondInfo + ' ' + thirdInfo, price: price, nums: this.data.goodslist[this.data.chooseNums].good.carCount, goodsid: this.data.goodsid, attrSpacStr: this.data.allgroupindex, SpecInfo: this.data.selectAttrInfo, newCartRecord: 1, chooseNums: this.data.chooseNums, yuanjia: this.data.singlePricestr })
			this.setData({
				edtiorNums: 0,
				edtiorNums1: 0,
				item5: item5,
				item5_1: item5,
				condition1: !this.data.condition1,
				firstInfo: "",
				secondInfo: "",
				thirdInfo: "",
				groupindex1: "",
				groupindex2: "",
				groupindex3: "",
				singleprice: 0,
				allgroupindex: "",
				selectAttrInfo: "",
			})
			goodslist[index].good.carCount = 0
			if (item5.length > 0) {
				for (var i = 0; i < item5.length; i++) {
					shopCartlength += item5[i].nums
					if (goodslist[index].good.GoodsName == item5[i].name) {
						goodslist[index].good.carCount += item5[i].nums
					}
				}
				this.setCondition(1)
				this.setData({ shopCartlength: shopCartlength, goodslist: goodslist, isfinNums: 0 })
			}
		}
	},
	// 清空购物车
	clearItem5: function () {
		var that = this
		var shopCartlength = that.data.shopCartlength
		var isFirstClick = that.data.isFirstClick
		var allprice = that.data.allprice
		if (that.data.item5.length > 0) {
			wx.showModal({
				title: '提示',
				content: '是否清空购物车？',
				success: function (res) {
					if (res.confirm) {
						var item5 = []
						var goodslist = that.data.goodslist
						that.setData({
							goodslist: that.data.goodslist_1,
							item5: item5,
							isFirstClick: true,
							allprice: 0,
							shopCartlength: 0
						})
					} else if (res.cancel) {
						console.log('用户点击取消')
					}
				}
			})
		}
	},
	// 增加规格弹窗的数量“+”
	addNums: function (chooseNums) {
		var typeofa = 0
		var shopCartlength = this.data.shopCartlength
		var item5 = this.data.item5
		var goodslist = this.data.goodslist
		var edtiorNums = this.data.edtiorNums
		// var arrow = this.data.arrow
		// if (arrow == 0) {
		//   this.setData({ goodslist_2: goodslist })
		// }
		var goodslist_2 = this.data.goodslist
		if (typeof (chooseNums) == 'string') {
			chooseNums = chooseNums
			var good = this.data.goodslist[chooseNums].good
			typeofa = 1
		}
		if (typeof (chooseNums) == 'object') {
			chooseNums = chooseNums.currentTarget.id
		}
		// var initenums = goodslist[this.data.chooseNums].good.carCount
		goodslist[chooseNums].good.carCount++
		var nums = 0
		nums++
		shopCartlength++
		if (goodslist[chooseNums].good.carCount > goodslist[chooseNums].good.Stock) {
			wx.showModal({
				title: '提示',
				content: '不能大于库存',
				success: function (res) {
					if (res.confirm) {
						goodslist[chooseNums].good.carCount--
					} else if (res.cancel) {
						goodslist[chooseNums].good.carCount--
					}
				}
			})
			return
		}
		edtiorNums++
		if (typeofa == 1) {
			if (item5.length > 0) {
				for (var i = 0; i < item5.length; i++) {
					if (item5[i].name == good.GoodsName && item5[i].goodsid == good.Id) {
						item5.splice(i, 1)
					}
				}
			}
			item5.push({ name: good.GoodsName, size: this.data.firstInfo + ' ' + this.data.secondInfo + ' ' + this.data.thirdInfo, price: good.discountPricestr, nums: good.carCount, goodsid: good.Id, attrSpacStr: this.data.allgroupindex, SpecInfo: this.data.selectAttrInfo, newCartRecord: 1, chooseNums: chooseNums, yuanjia: good.PriceStr })
			if (item5.length > 0) {
				for (var i = 0; i < item5.length; i++) {
					if (item5[i].nums == 0) {
						item5.splice(i, 1)
					}
				}
			}
		}
		this.setCondition(1)
		console.log('我是对应的good数组', goodslist[chooseNums].good)
		this.setData({
			shopCartlength: shopCartlength,
			item5: item5,
			item5_1: item5,
			goodslist: goodslist,
			nums: nums,
			edtiorNums: edtiorNums,
			// arrowms,
			// arrow: 1
			// initenums: initenums
		})
	},
	// 增加规格弹窗的数量“-”
	RaddNums: function (chooseNums) {
		var typeofa = 0
		var edtiorNums1 = this.data.edtiorNums1
		var item5 = this.data.item5
		var shopCartlength = this.data.shopCartlength
		var goodslist = this.data.goodslist
		var goodslist_2 = this.data.goodslist
		if (typeof (chooseNums) == 'string') {
			chooseNums = chooseNums
			var good = this.data.goodslist[chooseNums].good
			typeofa = 1
		}
		if (typeof (chooseNums) == 'object') {
			chooseNums = chooseNums.currentTarget.id
		}
		if (goodslist[chooseNums].good.carCount > 0) {
			goodslist[chooseNums].good.carCount--
		} else {
			wx.showToast({
				title: '选择数量不能小于0',
			})
		}
		var nums = 0
		nums++
		shopCartlength--
		edtiorNums1++
		if (typeofa == 1) {
			if (item5.length > 0) {
				for (var i = 0; i < item5.length; i++) {
					if (item5[i].name == good.GoodsName && item5[i].goodsid == good.Id) {
						item5.splice(i, 1)
					}
				}
			}
			item5.push({ name: good.GoodsName, size: this.data.firstInfo + ' ' + this.data.secondInfo + ' ' + this.data.thirdInfo, price: good.discountPricestr, nums: good.carCount, goodsid: good.Id, attrSpacStr: this.data.allgroupindex, SpecInfo: this.data.selectAttrInfo, newCartRecord: 1, chooseNums: chooseNums, yuanjia: good.PriceStr })
			if (item5.length > 0) {
				for (var i = 0; i < item5.length; i++) {
					if (item5[i].nums == 0) {
						item5.splice(i, 1)
					}
				}
			}
		}
		this.setCondition(1)
		console.log('我是对应的good数组', goodslist[chooseNums].good)
		this.setData({
			edtiorNums1: edtiorNums1,
			shopCartlength: shopCartlength,
			item5: item5,
			item5_1: item5,
			goodslist: goodslist,
			nums: nums,
		})
	},
	// 增加购物车单商品的数量
	addShopcarSingleNums: function (e) {
		var goodslist = this.data.goodslist
		var shopCartlength = this.data.shopCartlength
		var allprice = 0
		var index = e.currentTarget.id
		var item5 = this.data.item5
		goodslist[item5[index].chooseNums].good.carCount++
		item5[index].nums++
		shopCartlength++
		if (goodslist[item5[index].chooseNums].good.carCount > goodslist[item5[index].chooseNums].good.Stock) {
			wx.showModal({
				title: '提示',
				content: '不能大于库存',
				success: function (res) {
					if (res.confirm) {
						item5[index].nums--
						goodslist[item5[index].chooseNums].good.carCount--
					} else if (res.cancel) {
						item5[index].nums--
						goodslist[item5[index].chooseNums].good.carCount--
					}
				}
			})
			return
		}
		item5[index].price = (Number(item5[index].price) + (Number(item5[index].price) / Number(item5[index].nums - 1))).toFixed(2)
		for (var i = 0; i < item5.length; i++) {
			if (typeof (item5[i].price == "string")) {
				item5[i].price = Number(item5[i].price)
			}
			allprice += item5[i].price
		}
		allprice = parseFloat(allprice).toFixed(2)
		if (item5.length > 0) {
			for (var i = 0; i < item5.length; i++) {
				if (item5[i].nums == 0) {
					item5.splice(i, 1)
				}
			}
		}
		this.setData({ item5: item5, goodslist: goodslist, allprice: allprice, shopCartlength: shopCartlength })
	},
	// 提交订单
	update: function (e) {
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, this)
		for (var i = 0; i < this.data.item5.length; i++) {
			this.data.item5[i].price = Number(this.data.item5[i].price)
		}
		if (this.data.item5.length > 0 && (Number(this.data.allprice) > Number(this.data.food.OutSideStr)) && this.data.TablesNo == -999) {
			var item5 = JSON.stringify(this.data.item5)
			wx.navigateTo({
				url: '../updateOrder/updateOrder?item5=' + item5 + '&allprice=' + this.data.allprice,
			})
			return
		}
		if (!(this.data.item5.length > 0) && this.data.TablesNo == -999 && this.data.allprice == 0) {
			wx.showModal({
				title: '提示',
				content: '请添加购物',
				showCancel: false
			})
			return
		}
		if (this.data.item5.length > 0 && this.data.TablesNo == -999 && (Number(this.data.allprice) < Number(this.data.food.OutSideStr)) && this.data.allprice != 0) {
			wx.showToast({
				title: '不能低于起送价',
			})
			return
		}

		if (this.data.item5.length > 0) {
			var item5 = JSON.stringify(this.data.item5)
			wx.navigateTo({
				url: '../updateOrder/updateOrder?item5=' + item5 + '&allprice=' + this.data.allprice,
			})
			return
		}
		if (!(this.data.item5.length > 0) && this.data.TablesNo != -999) {
			wx.showModal({
				title: '提示',
				content: '请添加购物',
				showCancel: false
			})
			return
		}
	},
	// 减少购物车单商品的数量
	RaddShopcarSingleNums: function (e) {
		var goodslist = this.data.goodslist
		var shopCartlength = this.data.shopCartlength
		var allprice = 0
		var index = e.currentTarget.id
		var item5 = this.data.item5
		goodslist[item5[index].chooseNums].good.carCount--
		item5[index].nums--
		shopCartlength--
		item5[index].price = (Number(item5[index].price) - (Number(item5[index].price) / Number(item5[index].nums + 1))).toFixed(2)

		for (var i = 0; i < item5.length; i++) {
			if (typeof (item5[i].price == "string")) {
				item5[i].price = Number(item5[i].price)
			}
			allprice += item5[i].price
		}
		var price = item5[index].price
		price = parseFloat(price).toFixed(2)
		allprice = parseFloat(allprice).toFixed(2)
		if (item5.length > 0) {
			for (var i = 0; i < item5.length; i++) {
				if (item5[i].nums == 0) {
					item5.splice(i, 1)
				}
			}
		}
		this.setData({ item5: item5, goodslist: goodslist, allprice: allprice, shopCartlength: shopCartlength })
	},

	// 定位
	chooseLocation: function () {
		var that = this
		wx.chooseLocation({
			success: function (res) {
				var weidu = res.latitude
				var jingdu = res.longitude
				var addressInfo = res.name
				that.saveLocation("addr", addressInfo)
				that.saveLocation("weidu", weidu)
				that.saveLocation("jingdu", jingdu)
				app.globalData.weidu = weidu
				app.globalData.jingdu = jingdu
				app.globalData.addressInfo = addressInfo
				that.setData({
					addressInfo: addressInfo,
					showAdresstips: false
				})
				console.log(res)
				that.inite3(weidu, jingdu)
			},
		})
	},
	saveLocation: function (key, data) {
		var that = this
		wx.setStorage({
			key: key, //调用获取缓存接口
			data: data,
			success: function (res) {
				console.log(res)
			}
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
		if (this.data.TablesNo == -999) {
			wx.setNavigationBarTitle({
				title: '外卖',
			})
		} else {
			wx.setNavigationBarTitle({
				title: '自助点餐' + '(' + this.data.TablesNo + '号桌)',
			})
		}
		var ispayOk = app.globalData.ispayOk
		var orderId = app.globalData.orderid
		var item5 = this.data.item5
		var allprice = this.data.allprice
		var shopCartlength = this.data.shopCartlength
		// 支付失败回调
		if (ispayOk == 0) {
			this.inite4(orderId, 0)
		}
		if (app.globalData.isclearItem5 == 1) {
			this.inite1()
			this.GetGoodsList(0, '', this.data.shopType, this.data.pageindex, 0)
			this.setData({ item5: [], allprice: 0, shopCartlength: 0 })
			app.globalData.isclearItem5 = 0
		}
		var addressInfo = app.globalData.addressInfo
		this.setData({ addressInfo: addressInfo })
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
		that.data.pageindex = 1
		var inputvalue = that.data.inputvalue
		var item5 = that.data.item5
		var shopCartlength = that.data.shopCartlength
		var allprice = that.data.allprice
		that.setData({ choose0: 0, inputvalue: '', item5: [], allprice: 0, shopCartlength: 0 })
		that.inite()
		util.GetVipInfo(that)
		util.GetAgentConfigInfo(that) //刷新水印
		that.inite1()
		setTimeout(function () {
			wx.stopPullDownRefresh()
			that.GetGoodsList(0, '', that.data.shopType, 1, 0)
			wx.showToast({
				title: '店铺状态已更新'
			})
		}, 1000)
	},


  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {
		this.GetGoodsList(this.data.shopindex, this.data.goodsName, this.data.shopType, this.data.pageindex, 1)
	},
	// 根据名字搜索菜单列表
	searchGood: function (e) {
		var goodName = e.detail.value
		var inputvalue = e.detail.value
		this.setData({ goodsName: goodName, inputvalue: inputvalue })
		this.GetGoodsList(0, this.data.goodsName, this.data.shopType, 1, 0)
	},
  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	},
	// 获取首页显示数据
	inite: function (e) {
		var that = this
		wx.request({
			url: addr.Address.GetFoodsDetail,
			data: {
				AppId: app.globalData.appid,
				// AppId: 307
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					var TheShop = res.data.postdata.food.TheShop
					var TakeOut = res.data.postdata.food.TakeOut
					var logoimg = res.data.postdata.Logo
					var FoodsName = res.data.postdata.food.FoodsName
					var TelePhone = res.data.postdata.food.TelePhone
					var ShippingFeeStr = res.data.postdata.food.ShippingFeeStr
					var OutSideStr = res.data.postdata.food.OutSideStr
					that.setData({
						logoimg: logoimg,
						FoodsName: FoodsName,
						food: res.data.postdata.food,
						TheShop: TheShop,//堂食
						TakeOut: TakeOut,//外卖
					})

					app.globalData.TelePhone = TelePhone
					app.globalData.ShippingFeeStr = ShippingFeeStr
					app.globalData.OutSideStr = OutSideStr
					app.globalData.TheShop = TheShop
					app.globalData.TakeOut = TakeOut
					app.globalData.logoimg = logoimg
					app.globalData.FoodsName = FoodsName
				}
			},
			fail: function () {
				console.log("获取信息出错")
				wx.showToast({
					title: '获取信息出错',
				})
			}
		})
	},
	// 菜单分类列表
	inite1: function (e) {
		var that = this
		wx.request({
			url: addr.Address.GetGoodsTypeList,
			data: {
				AppId: app.globalData.appid,
				pagesize: 100,
				pageindex: 1,
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.setData({
						goodsTypeList: res.data.postdata.goodsTypeList
					})
				}
			},
			fail: function () {
				console.log("获取菜类分类出错")
				wx.showToast({
					title: '获取菜类出错',
				})
			}
		})
	},
	// 首页菜单列表
	GetGoodsList: function (typeid, goodsName, shopType, pageindex, isReachBottom) {
		var that = this
		wx.request({
			url: addr.Address.GetGoodsList,
			data: {
				AppId: app.globalData.appid,
				typeid: typeid,
				goodsName: goodsName,
				shopType: shopType,
				pageindex: pageindex,
				pagesize: 6,
				levelid: app.globalData.levelid
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.data.shopindex = typeid
					if (isReachBottom == 0) {
						if (res.data.postdata.goodslist.length < 4) {
							that.data.scrollTop = true
						}
						that.setData({
							isAllgoods:false,
							scrollTop: that.data.scrollTop,
							goodslist: res.data.postdata.goodslist,
							goodslist_1: res.data.postdata.goodslist
						})
						++that.data.pageindex
					} else {
						if (res.data.postdata.goodslist.length > 0) {
							that.data.goodslist = that.data.goodslist.concat(res.data.postdata.goodslist)
							that.data.goodslist_1 = that.data.goodslist_1.concat(res.data.postdata.goodslist)
							that.setData({
								goodslist: that.data.goodslist,
								goodslist_1: that.data.goodslist_1
							})
							++that.data.pageindex
						}else{
							that.setData({isAllgoods:true})
						}
					}
				}
			},
			fail: function () {
				console.log("获取菜类分类出错")
				wx.showToast({
					title: '获取菜类出错',
				})
			}
		})
	},
	// 查询配送距离
	inite3: function (lat, lng) {
		var that = this
		wx.request({
			url: addr.Address.GetDistanceForFood,
			data: {
				AppId: app.globalData.appid,
				lat: lat,
				lng: lng
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				that.setData({
					msg: res.data.msg
				})
				that.saveLocation("msg", res.data.msg)
			},
			fail: function () {
				console.log("查询出错")
				wx.showToast({
					title: '查询出错',
				})
			}
		})
	},
	// 更改订单的状态
	inite4: function (orderId, State) {
		var that = this
		wx.request({
			url: addr.Address.updateMiniappGoodsOrderState,
			data: {
				AppId: getApp().globalData.appid,
				openid: getApp().globalData.userInfo.openId,
				orderId: orderId,
				State: State
			},
			method: "POST",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					wx.navigateBack({
						delta: 1
					})
					console.log("更新订单状态成功")
				}
			},
			fail: function () {
				console.log("更新订单状态出错")
				wx.showToast({
					title: '更新订单状态出错',
				})
			}
		})
	}
})