// pages/appointment/appointment_index.js
const http = require('../../utils/http.js');
const addr = require('../../utils/addr.js');
const tools = require('../../utils/tools.js');
const util = require('../../utils/util.js');

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
		shopType: 1,//购买方式(0点餐/1外卖)
		pageindex: 1,//分页
		isall: false,

		shownotice: false,//店铺公告弹窗
		showgoodinfo: false,//商品详情弹窗
		goodinfoModal: false,//添加商品(有规则商品弹窗)
		showappointmentlist: false,//预约购物的订单列表
	},
	chooseGoodType: function (e) {
		wx.pageScrollTo({ scrollTop: 0, })
		this.data.pageindex = 1 //选择类别还原pageindex分页=1
		this.GetGoodsList(e.currentTarget.dataset.id, 1, 0)
	},

	showappointmentlist: function () {
		this.setData({ showappointmentlist: !this.data.showappointmentlist })
	},
	submit_formid: function (e) {
		// 提交备用formId
		that.commitFormId(e.detail.formId, this)
	},
	addgood: function (e) { //页面加号
		var that = this
		// 提交备用formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)
		var goodslist = that.data.goodslist
		var index = e.currentTarget.id
		var foodinfo = that.data.goodslist[e.currentTarget.id]
		if (foodinfo.GASDetailList.length > 0) { //有规格商品则弹窗
			foodinfo.nowbuynums = 1
			that.data.index = index
			that.setData({ goodinfoModal: !that.data.goodinfoModal })


		} else { //无规格商品则直接+
			if (goodslist[index].stock > goodslist[index].carCount) {//判断库存是否足够
				goodslist[index].carCount++

				var findgoodid = that.data.shopcartList.find(f => f.goodid == goodslist[index].id)//无规格商品直接遍历商品id，相同则直接+数量，否则push
				if (findgoodid) {
					findgoodid.buynums++
				} else {
					that.data.shopcartList.push({ goodname: goodslist[index].name, buynums: goodslist[index].carCount, size: '', attrSpacStr: '', SpecInfo: '', goodid: goodslist[index].id, discountprice: goodslist[index].discountPricestr, price: goodslist[index].priceStr, discount: goodslist[index].discount }) //如果没有重复的话，push到购物车数组里边
				}

			} else {
				that.showtoast('库存不足！', 'loading')
			}
		}


		that.countallprice()//计算购物车所有金额
		that.setData({ foodinfo: foodinfo, goodslist: goodslist, shopcartList: that.data.shopcartList })
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


		for (var i = 0; i < foodinfo.pickspecificationArray[lineid].items.length; i++) {
			if (i == index) {

				if (foodinfo.pickspecificationArray[lineid].items[i].ischoose == true) {
					foodinfo.pickspecificationArray[lineid].items[i].ischoose = false//如果已经选定过 反选
				} else {
					foodinfo.pickspecificationArray[lineid].items[i].ischoose = true
				}

			} else {
				foodinfo.pickspecificationArray[lineid].items[i].ischoose = false
			}
		}
		var chooselabelslength = 0
		for (var u = 0; u < foodinfo.pickspecificationArray.length; u++) {
			for (var h = 0; h < foodinfo.pickspecificationArray[u].items.length; h++) {
				if (foodinfo.pickspecificationArray[u].items[h].ischoose == true) {
					chooselabelslength++
					this.data.chooselabelslength = chooselabelslength
				}
			}
		}


		if (this.data.chooselabelslength == foodinfo.pickspecificationArray.length) {
			var allspecid = ''
			this.data.size = ''
			for (var z = 0; z < this.data.specidArray.length; z++) {
				allspecid += this.data.specidArray[z] + (z == this.data.specidArray.length - 1 ? '' : '_')
				this.data.size += this.data.sizeArray[z] + ' '
				var findchooseprice = foodinfo.GASDetailList.find(f => f.id == allspecid)
				if (findchooseprice) {
					if (foodinfo.nowbuynums > findchooseprice.stock) {
						foodinfo.nowbuynums = findchooseprice.stock
					}
					foodinfo.stock = findchooseprice.stock
					foodinfo.modalchooseprice = findchooseprice.discountPricestr
					this.data.foodinfo.modalallprice = (this.data.foodinfo.modalchooseprice * this.data.foodinfo.nowbuynums).toFixed(2) //计算弹窗商品总金额
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
		if (this.data.foodinfo.nowbuynums < this.data.foodinfo.stock) {
			this.data.foodinfo.nowbuynums++ //弹窗数量++
			this.data.foodinfo.modalallprice = (this.data.foodinfo.modalchooseprice * this.data.foodinfo.nowbuynums).toFixed(2) //计算弹窗商品总金额
			this.setData({ foodinfo: this.data.foodinfo, goodslist: this.data.goodslist })
		} else {
			wx.showToast({ title: '库存不足', icon: 'loading' })
		}
	},

	addgood_modalsure: function () { //选择规格弹窗确定按钮
		var foodinfo = this.data.foodinfo
		if (foodinfo.nowbuynums == 0) {
			wx.showToast({ title: '请选择数量', icon: 'loading' })
			return
		}
		if (this.data.chooselabelslength != foodinfo.pickspecificationArray.length) {
			wx.showToast({ title: '请选择规格', icon: 'loading' })
			return
		}
		var specinfo = ''
		var attrspacstr = ''
		for (var i = 0; i < this.data.specidArray.length; i++) {
			specinfo += this.data.specnameArray[i] + ' '
			attrspacstr += this.data.specidArray[i] + (i == this.data.specidArray.length - 1 ? '' : '_')
		}
		var findspecidAray = this.data.shopcartList.find(s => s.attrSpacStr == attrspacstr && s.goodid == foodinfo.id)

		if (findspecidAray) {
			findspecidAray.buynums += foodinfo.nowbuynums
		}

		else {

			var findinfo = foodinfo.GASDetailList.find(f => f.id == attrspacstr)//有规格商品先拼接规格，在遍历
			this.data.shopcartList.push({ goodname: foodinfo.name, buynums: foodinfo.nowbuynums, size: this.data.size, attrSpacStr: attrspacstr, SpecInfo: specinfo, goodid: foodinfo.id, discountprice: findinfo.discountPricestr, price: findinfo.price, discount: findinfo.discount }) //如果没有重复的话，push到购物车数组里边

		}
		var goodslist_carCount = 0
		for (var j = 0; j < this.data.shopcartList.length; j++) {
			if (this.data.shopcartList[j].goodid == foodinfo.id) {
				goodslist_carCount += this.data.shopcartList[j].buynums
			}
		}
		this.data.goodslist[this.data.index].carCount = goodslist_carCount//列表数量

		this.countallprice()
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
		this.clearShopcartList()
	},
	shopcartmodal_reduce: function (e) {//购物车弹窗‘-’
		var findgoodslist_carCount = this.data.goodslist.find(f => f.id == e.currentTarget.dataset.goodid)
		if (findgoodslist_carCount) {
			findgoodslist_carCount.carCount--
		}
		this.data.shopcartList[e.currentTarget.id].buynums--
		this.countallprice()
		if (this.data.shopcartList[e.currentTarget.id].buynums < 1) {
			this.data.shopcartList.splice(e.currentTarget.id, 1)
		}
		this.setData({ shopcartList: this.data.shopcartList, goodslist: this.data.goodslist })
	},
	shopcartmodal_add: function (e) { //购物车弹窗‘+’
		var findgoodslist_carCount = this.data.goodslist.find(f => f.id == e.currentTarget.dataset.goodid)
		if (findgoodslist_carCount) {
			findgoodslist_carCount.carCount++
		}
		this.data.shopcartList[e.currentTarget.id].buynums++
		this.countallprice()
		this.setData({ shopcartList: this.data.shopcartList, goodslist: this.data.goodslist })
	},
	go_appointment: function (e) {
		app.globalData.appoint_shopcartlist = this.data.shopcartList
		app.globalData.appoint_shopcartlength = this.data.shopcartlength
		app.globalData.appoint_alldiscountprice = this.data.alldiscountprice
		app.globalData.appoint_goodslist = this.data.goodslist
		app.globalData.appoint_paynow = e.currentTarget.id
		wx.navigateBack({
			url: '../appointment/appointment_index'
		})
	},
	/**
	 * 生命周期函数--监听页面加载
	 */
	onLoad: function (options) {
		var that = this

		util.setPageSkin(that);
		if ((app.globalData.appoint_shopcartlist.length) > 0) {
			that.setData({ shopcartList: app.globalData.appoint_shopcartlist, shopcartlength: app.globalData.appoint_shopcartlength, alldiscountprice: app.globalData.appoint_alldiscountprice, goodslist: app.globalData.appoint_goodslist })
		}
		that.GetReserveGoodClass() //获取预约商品分类
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
		if (this.data.shopcartList.length > 0) {
			app.globalData.appoint_paynow = 1
		}
		app.globalData.appoint_shopcartlist = this.data.shopcartList
		app.globalData.appoint_shopcartlength = this.data.shopcartlength
		app.globalData.appoint_alldiscountprice = this.data.alldiscountprice
		app.globalData.appoint_goodslist = this.data.goodslist
	},

	/**
	 * 页面相关事件处理函数--监听用户下拉动作
	 */
	onPullDownRefresh: function () {
		// tool.GetGoodsTypeList(this)
		// tool.GetFoodsDetail(this, 1)
		// this.setData({ shopcartList: [], alldiscountprice: '0.00', shopcartlength: 0 })
		// this.data.pageindex = 1
		// tool.GetGoodsList(this, 0, '', this.data.shopType, 1, 0)
		// template.stopPullDown()
	},

	/**
	 * 页面上拉触底事件的处理函数
	 */
	onReachBottom: function () {
		this.GetGoodsList(this.data.typeid, this.data.pageindex)
	},

	/**
	 * 用户点击右上角分享
	 */
	onShareAppMessage: function () {

	},

	onPageScroll: function (e) {

	},
	// 获取预约购物商品分类
	GetReserveGoodClass: function () {
		var that = this
		wx.showLoading({
			title: '加载中',
			mask: true,
			success: function () {
				http.postAsync(
					addr.Address.GetReserveGoodClass, {
						appId: app.globalData.appid,
						openId: app.globalData.userInfo.openId,
					}).then(function (data) {
						if (data.result) {
							var allSortId = ''
							for (let i = 0; i < data.obj.length; i++) {
								allSortId += data.obj[i].id + ','
							}
							let goodSort = [{ name: '全部', id: allSortId }]

							that.setData({
								goodSort: goodSort.concat(data.obj)
							})
							that.GetGoodsList(goodSort.concat(data.obj)[0].id, 1, 0)
						} else {
							tools.showLoadToast(data.msg)
						}
						wx.hideLoading()
					})
			}
		})
	},
	// 获取预约商品
	GetGoodsList: function (typeid, pageindex, isReachBottom) { //isreachbottom 0否 1是
		var that = this
		wx.showLoading({
			title: '加载中',
			mask: true,
			success: function () {
				http.getAsync(
					addr.Address.GetGoodsList, {
						aid: wx.getStorageSync("aid"),
						typeid: typeid,
						pageindex: pageindex,
						pagesize: 6,
						pricesort: '',
						exttypes: '',
						search: '',
						levelid: wx.getStorageSync("levelid"),
						goodShowType: 'normal'
					}).then(function (data) {
						if (data.isok == 1) {

							wx.hideLoading()
							for (var i = 0; i < data.postdata.goodslist.length; i++) {
								data.postdata.goodslist[i].carCount = 0
								data.postdata.goodslist[i].pickspecificationArray = JSON.parse(data.postdata.goodslist[i].pickspecification)
							}

							for (var i = 0; i < that.data.shopcartList.length; i++) {
								var findgoodid = data.postdata.goodslist.find(f => f.id == that.data.shopcartList[i].goodid)
								if (findgoodid) {
									findgoodid.carCount += that.data.shopcartList[i].buynums
								}
							}

							if (isReachBottom == 0) {
								that.setData({
									goodslist: data.postdata.goodslist,
									typeid: typeid
								})
								++that.data.pageindex
							} else {
								if (data.postdata.goodslist.length > 0) {
									that.data.goodslist = that.data.goodslist.concat(data.postdata.goodslist)
									++that.data.pageindex
								} else {
									return
								}
								that.setData({
									goodslist: that.data.goodslist,
									typeid: typeid
								})
							}

						} else {
							tools.showLoadToast(data.msg)
						}
						wx.hideLoading()
					})
			}
		})
	},
	showtoast: function (title, icon) {
		wx.showToast({
			title: title,
			icon: icon
		})
	},
	countallprice: function () {//计算购物车的所有钱
		var that = this
		var shopcartList = that.data.shopcartList
		var allprice = 0
		var alldiscountprice = 0
		var shopcartlength = 0
		for (var i = 0; i < shopcartList.length; i++) {
			allprice += Number(shopcartList[i].price) * Number(shopcartList[i].buynums)
			alldiscountprice += Number(shopcartList[i].discountprice) * Number(shopcartList[i].buynums)
			shopcartlength += shopcartList[i].buynums
		}
		that.setData({ allprice: (allprice).toFixed(2), alldiscountprice: (alldiscountprice).toFixed(2), shopcartlength: shopcartlength })
	},
	clearShopcartList: function () { //清空购物车
		var that = this
		if (that.data.shopcartList.length > 0) {
			wx.showModal({
				title: '提示',
				content: '是否清空购物车？',
				success: function (res) {
					if (res.confirm) {
						that.data.pageindex = 1
						// GetGoodsList(that, 0, '', that.data.shopType, 1, 0)
						that.setData({
							shopcartList: [],
							allprice: 0,
							alldiscountprice: '0.00',
							typeid: 0,
							goodsName: '',
							shopcartlength: 0,
						})
					} else if (res.cancel) {
						console.log('用户点击取消')
					}
				}
			})
		} else {
			return
		}
	}
})