// pages/index/index.js
var template = require('../../template/template.js');
var tool = require('../../template/Food2.0.js');
var util = require("../../utils/util.js");
var addr = require("../../utils/addr.js");
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {

		openState: {},
		shopcartList: [],
		alldiscountprice: '0.00',//购物车总价，需要给个默认值
		specidArray: [],//临时存储规格id
		sizeArray: [],//临时规格属性名字
		size: '',
		specnameArray: [],//临时存储规格分类+名字
		shopcartlength: 0,//购物车物品数量，需要给个默认值0

		chooselabelslength: 0,//判断选择规格属性是否已满选

		specinfo: '',
		attrspacstr: '',


		typeid: 0,
		goodsName: '',
		shopType: 1,//购买方式(0点餐/1外卖)
		pageindex: 1,//分页
		isall: false,

		showshopcart: false,//购物车内容弹窗
		shownotice: false,//店铺公告弹窗
		showgoodinfo: false,//商品详情弹窗
		chooseLocal: false,//选择定位弹窗 false已定位 true未定位
		goodinfoModal: false,//添加商品(有规则商品弹窗)
		couponsList: [],//是否存进行中立减金
		TablesNo: -999,//外卖-999 堂食!=-999
		addressInfo: '未定位',

	},
	previewGoodimg: function (e) {
		let previewArray = []
		previewArray.push(e.currentTarget.dataset.url)
		template.previewImageList(0, previewArray)
	},
	chooseGoodType: function (e) {
		wx.pageScrollTo({ scrollTop: 0, })
		this.data.pageindex = 1 //选择类别还原pageindex分页=1
		if (e.currentTarget.dataset.id == 0) { this.data.goodsName = '' }
		tool.GetGoodsList(this, e.currentTarget.dataset.id, this.data.goodsName, this.data.shopType, 1, 0)
	},
	searchGood: function (e) {
		wx.pageScrollTo({ scrollTop: 0, })
		this.data.pageindex = 1 //选择类别还原pageindex分页=1
		this.data.goodsName = e.detail.value
		tool.GetGoodsList(this, this.data.typeid, this.data.goodsName, this.data.shopType, 1, 0)
	},
	shownotice: function () {
		this.setData({ shownotice: !this.data.shownotice })
	},
	showgoodinfo: function (e) {
		var goodinfo = {}
		goodinfo.url = e.currentTarget.dataset.img //商品详情大图
		goodinfo.name = e.currentTarget.dataset.name //商品详情名称
		goodinfo.introduction = e.currentTarget.dataset.introduction //商品详情介绍
		this.setData({ showgoodinfo: !this.data.showgoodinfo, goodinfo: goodinfo })
	},
	chooseLocation: function () {
		tool.chooseLocation(this)
	},
	submit_formid: function (e) {
		// 提交备用formId
		template.commitFormId(e.detail.formId, this)
	},
	addgood: function (e) { //页面加号
		var that = this
		var goodslist = that.data.goodslist
		var index = e.currentTarget.id
		var foodinfo = that.data.goodslist[e.currentTarget.id]
		if (foodinfo.attrList.length > 0) { //有规格商品则弹窗
			foodinfo.good.nowbuynums = 1
			that.data.index = index
			that.setData({ goodinfoModal: !that.data.goodinfoModal })


		} else { //无规格商品则直接+
			if (goodslist[index].good.Stock > goodslist[index].good.carCount) {//判断库存是否足够
				goodslist[index].good.carCount++

				var findgoodid = that.data.shopcartList.find(f => f.goodid == goodslist[index].good.Id)//无规格商品直接遍历商品id，相同则直接+数量，否则push
				if (findgoodid) {
					findgoodid.buynums++
				} else {
					that.data.shopcartList.push({ goodname: goodslist[index].good.GoodsName, buynums: goodslist[index].good.carCount, size: '', attrSpacStr: '', SpecInfo: '', newCartRecord: 1, goodid: goodslist[index].good.Id, discountprice: goodslist[index].good.discountPricestr, price: goodslist[index].good.PriceStr, discount: goodslist[index].good.discount, isPackin: goodslist[index].good.isPackin }) //如果没有重复的话，push到购物车数组里边
				}

			} else {
				template.showtoast('库存不足！', 'loading')
			}
		}


		tool.countallprice(that)//计算购物车所有金额
		that.setData({ foodinfo: foodinfo, goodslist: goodslist, shopcartList: that.data.shopcartList })
		console.log(this.data.goodslist[e.currentTarget.id])
	},
	chooselabels: function (e) { //规格弹窗选择规格
		var attrname = e.currentTarget.dataset.attrname //大类别名称
		var specname = e.currentTarget.dataset.specname //规格名称
		var specid = e.currentTarget.dataset.specid //规格id
		var lineid = e.currentTarget.dataset.lineid //大类别所在下标
		var index = e.currentTarget.dataset.index //规格所在下标

		this.data.specidArray[lineid] = specid
		this.data.sizeArray[lineid] = specname
		this.data.specnameArray[lineid] = attrname + ':' + specname
		var foodinfo = this.data.foodinfo


		for (var i = 0; i < foodinfo.attrList[0].Value[lineid].SpecList.length; i++) {
			if (i == index) {

				if (foodinfo.attrList[0].Value[lineid].SpecList[i].ischoose == true) {
					foodinfo.attrList[0].Value[lineid].SpecList[i].ischoose = false//如果已经选定过 反选
				} else {
					foodinfo.attrList[0].Value[lineid].SpecList[i].ischoose = true
				}

			} else {
				foodinfo.attrList[0].Value[lineid].SpecList[i].ischoose = false
			}
		}
		var chooselabelslength = 0
		for (var u = 0; u < foodinfo.attrList[0].Value.length; u++) {
			for (var h = 0; h < foodinfo.attrList[0].Value[u].SpecList.length; h++) {
				if (foodinfo.attrList[0].Value[u].SpecList[h].ischoose == true) {
					chooselabelslength++
					this.data.chooselabelslength = chooselabelslength
				}
			}
		}


		if (this.data.chooselabelslength == foodinfo.attrList[0].Value.length) {
			var allspecid = ''
			this.data.size = ''
			for (var z = 0; z < this.data.specidArray.length; z++) {
				allspecid += this.data.specidArray[z] + '_'
				this.data.size += this.data.sizeArray[z] + ' '
				var findchooseprice = foodinfo.good.GASDetailList.find(f => f.id == allspecid)
				if (findchooseprice) {
					if (foodinfo.good.nowbuynums > findchooseprice.count) {
						foodinfo.good.nowbuynums = findchooseprice.count
					}
					foodinfo.good.Stock = findchooseprice.count
					foodinfo.good.modalchooseprice = findchooseprice.discountPricestr
					this.data.foodinfo.good.modalallprice = (this.data.foodinfo.good.modalchooseprice * this.data.foodinfo.good.nowbuynums).toFixed(2) //计算弹窗商品总金额
				}
			}
		}

		this.setData({ foodinfo: foodinfo })

	},


	modaladdnums: function (e) { //选择规格弹窗增加数量
		if (this.data.specidArray.length == 0) {
			wx.showToast({ title: '请选择规格', icon: 'loading' })
			return
		}
		if (this.data.foodinfo.good.nowbuynums < this.data.foodinfo.good.Stock) {
			this.data.foodinfo.good.nowbuynums++ //弹窗数量++
			this.data.foodinfo.good.modalallprice = (this.data.foodinfo.good.modalchooseprice * this.data.foodinfo.good.nowbuynums).toFixed(2) //计算弹窗商品总金额
			this.setData({ foodinfo: this.data.foodinfo, goodslist: this.data.goodslist })
		} else {
			wx.showToast({ title: '库存不足', icon: 'loading' })
		}
	},

	addgood_modalsure: function () { //选择规格弹窗确定按钮
		var foodinfo = this.data.foodinfo
		if (foodinfo.good.nowbuynums == 0) {
			wx.showToast({ title: '请选择数量', icon: 'loading' })
			return
		}
		if (this.data.chooselabelslength != foodinfo.attrList[0].Value.length) {
			wx.showToast({ title: '请选择规格', icon: 'loading' })
			return
		}
		var specinfo = ''
		var attrspacstr = ''
		for (var i = 0; i < this.data.specidArray.length; i++) {
			specinfo += this.data.specnameArray[i] + ' '
			attrspacstr += this.data.specidArray[i] + '_'
		}
		var findspecidAray = this.data.shopcartList.find(s => s.attrSpacStr == attrspacstr && s.goodid == foodinfo.good.Id)

		if (findspecidAray) {
			findspecidAray.buynums += foodinfo.good.nowbuynums
		}

		else {

			var findinfo = foodinfo.good.GASDetailList.find(f => f.id == attrspacstr)//有规格商品先拼接规格，在遍历
			this.data.shopcartList.push({ goodname: foodinfo.good.GoodsName, buynums: foodinfo.good.nowbuynums, size: this.data.size, attrSpacStr: attrspacstr, SpecInfo: specinfo, newCartRecord: 1, goodid: foodinfo.good.Id, discountprice: findinfo.discountPricestr, price: findinfo.priceStr, discount: findinfo.discount, isPackin: foodinfo.good.isPackin }) //如果没有重复的话，push到购物车数组里边

		}
		var goodslist_carCount = 0
		for (var j = 0; j < this.data.shopcartList.length; j++) {
			if (this.data.shopcartList[j].goodid == foodinfo.good.Id) {
				goodslist_carCount += this.data.shopcartList[j].buynums
			}
		}
		this.data.goodslist[this.data.index].good.carCount = goodslist_carCount//列表数量

		tool.countallprice(this)
		this.data.specidArray = [], this.data.specnameArray = [], this.data.size = ''
		this.setData({ goodinfoModal: !this.data.goodinfoModal, shopcartList: this.data.shopcartList, goodslist: this.data.goodslist })
		this.data.chooselabelslength = 0 //还原选择规格长度判断		
	},

	cancelchoosegood: function () { //选择规格弹窗取消按钮
		this.data.chooselabelslength = 0, this.data.size = '', this.data.specidArray = [], this.data.specnameArray = []
		this.setData({ goodinfoModal: !this.data.goodinfoModal })
	},
	showshopcartmodal: function () { //显示购物车弹窗
		this.setData({ showshopcart: !this.data.showshopcart })
	},
	clearShopcartList: function () {
		tool.clearShopcartList(this)
	},
	shopcartmodal_reduce: function (e) {//购物车弹窗‘-’
		var findgoodslist_carCount = this.data.goodslist.find(f => f.good.Id == e.currentTarget.dataset.goodid)
		if (findgoodslist_carCount) {
			findgoodslist_carCount.good.carCount--
		}
		this.data.shopcartList[e.currentTarget.id].buynums--
		tool.countallprice(this)
		if (this.data.shopcartList[e.currentTarget.id].buynums < 1) {
			this.data.shopcartList.splice(e.currentTarget.id, 1)
		}
		this.setData({ shopcartList: this.data.shopcartList, goodslist: this.data.goodslist })
	},
	shopcartmodal_add: function (e) { //购物车弹窗‘+’
		var findgoodslist_carCount = this.data.goodslist.find(f => f.good.Id == e.currentTarget.dataset.goodid)
		if (findgoodslist_carCount) {
			findgoodslist_carCount.good.carCount++
		}
		this.data.shopcartList[e.currentTarget.id].buynums++
		tool.countallprice(this)
		this.setData({ shopcartList: this.data.shopcartList, goodslist: this.data.goodslist })
	},
	gotopay: function () {//去结算
		if (this.data.shopcartList.length > 0) {
			wx.navigateTo({
				url: '../updateOrder/updateOrder?shopcartList=' + JSON.stringify(this.data.shopcartList) + '&allprice=' + this.data.allprice + '&alldiscountprice=' + this.data.alldiscountprice + '&dishwarefee=' + this.data.dishwarefee,
			})
		} else {
			wx.showToast({ title: '请选择商品', icon: 'loading' })
		}
	},
	_couponGo: function () {
		wx.navigateTo({ url: '../addCoupon/mysmoney', })
	},


	/**
	 * 生命周期函数--监听页面加载
	 */
	onLoad: function (options) {
		var that = this
		console.log(that.data.ShippingFeeStr)
		if (decodeURIComponent(options.scene) == "undefined") { //外卖
			app.globalData.TablesNo = -999
			wx.setStorageSync('isNewTableNo', 'false')
			var shoptype = 1
			wx.setNavigationBarTitle({ title: '外卖', })

			if (wx.getStorageSync('addr') != "" && wx.getStorageSync('msg') != "") { //外卖是否定位
				console.log("外卖已经定位")
				app.globalData.addressInfo = wx.getStorageSync('addr')
				that.setData({ msg: wx.getStorageSync('msg'), addressInfo: wx.getStorageSync('addr') })
			} else {
				console.log("外卖没有定位")
				that.setData({
					chooseLocal: true,
				})
			}
		} else { //扫码点餐 
			app.globalData.TablesNo = that.data.TablesNo
			tool.GetReserveMenuPay(that, function (cb) {
				if (cb > 0) {
					that.gotoPay(that, cb)
				} else {
					wx.showModal({ title: '提示', content: '您有预约订单商家仍未接单！可提示商家进行接单。', showCancel: false, confirmText: '知道了' })
				}
			})
			var code = decodeURIComponent(options.scene)
			var Pagetitle = ''
			var isNewCode = 2
			for (var key in code) {
				if (code[key] == '#') {
					var Newcode = code.split('#')
					isNewCode = 1
					break
				}
			}
			if (isNewCode == 1) {
				wx.setStorageSync('isNewTableNo', 'true')
				tool.GetTableNo(that, Newcode[1], function (cb) {
					if (cb.data.isok) {
						that.data.TablesNo = app.globalData.TablesNo = Newcode[1]
						Pagetitle = cb.data.dataObj.name
						wx.setNavigationBarTitle({ title: '餐台：' + '（' + Pagetitle + '）', })
					}
				})
			} else {
				wx.setStorageSync('isNewTableNo', 'false')
				that.data.TablesNo = app.globalData.TablesNo = code
				Pagetitle = code
				wx.setNavigationBarTitle({ title: '自助点餐' + '(' + Pagetitle + '号桌)', })
			}

			var shoptype = 0

		}

		that.setData({ shopType: shoptype, TablesNo: that.data.TablesNo, ShippingFeeStr: app.globalData.ShippingFeeStr })
		app.new_login(function (e) {

			template.GetAgentConfigInfo(that)
			tool.GetGoodsTypeList(that)
			template.GetVipInfo(that, function () {
				tool.GetFoodsDetail(that, 1)
				tool.GetGoodsList(that, 0, '', that.data.shopType, 1, 0)
			})
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
		template.GetReductionCardList(this)
		console.log("监听页面显示")
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
		// var that = this
		// template.GetReductionCardList(that)
		// template.GetVipInfo(that, function () {
		// 	tool.GetGoodsList(that, 0, '', that.data.shopType, 1, 0)
		// })
		// tool.GetGoodsTypeList(that)
		// tool.GetFoodsDetail(that, 1)
		// that.setData({ shopcartList: [], alldiscountprice: '0.00', shopcartlength: 0 })
		// that.data.pageindex = 1

		// template.stopPullDown()
	},

	/**
	 * 页面上拉触底事件的处理函数
	 */
	onReachBottom: function () {
		tool.GetGoodsList(this, this.data.typeid, this.data.goodsName, this.data.shopType, this.data.pageindex, 1)
	},

	/**
	 * 用户点击右上角分享
	 */
	onShareAppMessage: function () {

	},

	onPageScroll: function (e) {
		if (e.scrollTop > 155) { this.setData({ isup: 1 }); }
		if (e.scrollTop <= 1) { this.setData({ isup: 0 }); }
	},


	gotoPay: function (that, orderid) { //预约点餐扫码付款
		var newparam = {
			openId: app.globalData.userInfo.openId,
			orderid: orderid,
			'type': 1,
		}
		util.PayOrder(orderid, newparam, {
			failed: function () {
				wx.showToast({ title: '请重新扫码支付', icon: 'loading' })
			},
			success: function (res) {
				if (res == "wxpay") {
				} else if (res == "success") {
					wx.showToast({ title: '支付成功' })
				}
			}
		})
	}
})