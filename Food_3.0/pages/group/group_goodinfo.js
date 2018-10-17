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
		select_condition: 0,
		showmoregroup: false,
		shopcart: false,
		isshareGroup: 0,//是否从分享进入的拼团详情 0否 1是
		groupUserlist: [
			{ img: '/image/test.png', lack: '1', lesstime: '03:45:20', name: 'MAXIAOJIAN1' },
			{ img: '/image/test.png', lack: '1', lesstime: '03:45:2001634165321', name: 'MAXIAOJIAN' },
		],


		openState: {},
		shopcartList: [],
		alldiscountprice: '0.00',//购物车总价，需要给个默认值
		specidArray: [],//临时存储规格id
		sizeArray: [],//临时规格属性名字
		size: '请选择规格',
		specnameArray: [],//临时存储规格分类+名字
		shopcartlength: 0,//购物车物品数量，需要给个默认值0

		chooselabelslength: 0,//判断选择规格属性是否已满选

		specinfo: '',
		attrspacstr: '',

		isgroup: 0,//是否拼团 0单独购买 1拼团
	},
	navito_joingroupinfo :function(e){
		template.goNewPage('../group/joingroupinfo?groupid=' + e.currentTarget.dataset.groupid)
	},
	navito_groupgoodinfo: function (e) {
		template.goNewPage('../group/group_goodinfo?groupid=' + e.currentTarget.dataset.goodid + '&isshareGroup=1' + '&GID=' + e.currentTarget.dataset.gid)
	},
	showshopcart: function (e) {
		var isgroup = e.currentTarget.dataset.isgroup
		if (isgroup == 1) {
			this.data.miniappFoodGoods.chooseprice = Number(this.data.miniappFoodGoods.discountGroupPrice) / 100
			this.data.miniappFoodGoods.singlediscountprice = Number(this.data.miniappFoodGoods.discountGroupPrice) / 100
		} else {
			this.data.miniappFoodGoods.chooseprice = this.data.miniappFoodGoods.discountPricestr
			this.data.miniappFoodGoods.singlediscountprice = this.data.miniappFoodGoods.discountPricestr
		}
		if (this.data.goodsAttrList.length == 0) {
			this.data.miniappFoodGoods.EntGroups.OriginalPriceStr = this.data.miniappFoodGoods.PriceStr
		}

		// 还原相关属性
		this.data.specidArray = [], this.data.sizeArray = [], this.data.specnameArray = [], this.data.chooselabelslength = 0,
			this.data.miniappFoodGoods.count = this.data.miniappFoodGoods.Stock,
			this.data.miniappFoodGoods.nowbuynums = 1
		for (var i = 0; i < this.data.goodsAttrList.length; i++) {
			for (var j = 0; j < this.data.goodsAttrList[i].SpecList.length; j++) {
				this.data.goodsAttrList[i].SpecList[j].ischoose = false
			}
		}


		// end
		this.setData({ shopcart: !this.data.shopcart, isgroup: isgroup != undefined ? isgroup : 0, miniappFoodGoods: this.data.miniappFoodGoods, goodsAttrList: this.data.goodsAttrList, size: '请选择规格' })
	},
	showmore: function () {
		this.setData({ showmoregroup: !this.data.showmoregroup })
	},
	go_home: function () {
		wx.switchTab({ url: '../home/home', })
	},
	change_select: function (e) {
		this.setData({ select_condition: e.currentTarget.id })
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
	chooselabels: function (e) { //规格弹窗选择规格
		var attrname = e.currentTarget.dataset.attrname //大类别名称
		var specname = e.currentTarget.dataset.specname //规格名称
		var specid = e.currentTarget.dataset.specid //规格id
		var lineid = e.currentTarget.dataset.lineid //大类别所在下标
		var index = e.currentTarget.dataset.index //规格所在下标

		this.data.specidArray[lineid] = specid
		this.data.sizeArray[lineid] = specname
		this.data.specnameArray[lineid] = attrname + ':' + specname
		var foodinfo = this.data.goodsAttrList


		for (var i = 0; i < foodinfo[lineid].SpecList.length; i++) {
			if (i == index) {

				if (foodinfo[lineid].SpecList[i].ischoose == true) {
					foodinfo[lineid].SpecList[i].ischoose = false//如果已经选定过 反选
				} else {
					foodinfo[lineid].SpecList[i].ischoose = true
				}

			} else {
				foodinfo[lineid].SpecList[i].ischoose = false
			}
		}
		var chooselabelslength = 0
		for (var u = 0; u < foodinfo.length; u++) {
			for (var h = 0; h < foodinfo[u].SpecList.length; h++) {
				if (foodinfo[u].SpecList[h].ischoose == true) {
					chooselabelslength++
				}
			}
		}
		this.data.chooselabelslength = chooselabelslength


		if (this.data.chooselabelslength == foodinfo.length) {
			var allspecid = ''
			this.data.size = ''
			for (var z = 0; z < this.data.specidArray.length; z++) {
				allspecid += this.data.specidArray[z] + '_'
				this.data.size += this.data.sizeArray[z] + ' '
				var findchooseprice = this.data.miniappFoodGoods.GASDetailList.find(f => f.id == allspecid)
				if (findchooseprice) {
					this.data.miniappFoodGoods.count = findchooseprice.count
					this.data.miniappFoodGoods.EntGroups.OriginalPriceStr = (findchooseprice.originalPrice / 100).toFixed(2)
					this.data.miniappFoodGoods.chooseprice = this.data.isgroup == 0 ? Number(findchooseprice.discountPricestr).toFixed(2) : (findchooseprice.discountGroupPrice / 100).toFixed(2)
					this.data.miniappFoodGoods.EntGroups.OriginalPriceStr = this.data.isgroup == 0 ? (findchooseprice.price / 100) : (findchooseprice.groupPrice / 100)
					this.data.miniappFoodGoods.singleprice = this.data.isgroup == 0 ? (findchooseprice.price / 100) : (findchooseprice.groupPrice / 100)

					this.data.miniappFoodGoods.singlediscountprice = this.data.isgroup == 0 ? Number(findchooseprice.discountPricestr).toFixed(2) : (findchooseprice.discountGroupPrice / 100).toFixed(2)
					if (findchooseprice.count > 0) {
						this.data.miniappFoodGoods.nowbuynums = 1
					} else {
						this.data.miniappFoodGoods.nowbuynums = 0
					}
				}
			}
		}
		this.setData({ miniappFoodGoods: this.data.miniappFoodGoods, goodsAttrList: this.data.goodsAttrList, size: this.data.size })

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
		if (this.data.goodsAttrList.length > 0 && this.data.specidArray.length != this.data.goodsAttrList.length) {
			wx.showToast({ title: '请选择规格', icon: 'loading' })
		} else {

			if (this.data.miniappFoodGoods.nowbuynums > 1) {
				this.data.miniappFoodGoods.nowbuynums--
				this.data.miniappFoodGoods.chooseprice = Number(this.data.miniappFoodGoods.singlediscountprice) * this.data.miniappFoodGoods.nowbuynums
				this.data.miniappFoodGoods.EntGroups.OriginalPriceStr = Number(this.data.miniappFoodGoods.singleprice) * this.data.miniappFoodGoods.nowbuynums
				this.setData({ miniappFoodGoods: this.data.miniappFoodGoods })
			} else {
				wx.showToast({ title: '不能再减了哦', icon: 'loading' })
			}

		}
	},
	shopcartmodal_add: function (e) { //购物车弹窗‘+’
		if (this.data.goodsAttrList.length > 0 && this.data.specidArray.length != this.data.goodsAttrList.length) {
			wx.showToast({ title: '请选择规格', icon: 'loading' })
		} else {

			if (this.data.miniappFoodGoods.nowbuynums < this.data.miniappFoodGoods.count) {
				this.data.miniappFoodGoods.nowbuynums++
				this.data.miniappFoodGoods.chooseprice = Number(this.data.miniappFoodGoods.singlediscountprice) * this.data.miniappFoodGoods.nowbuynums
				this.data.miniappFoodGoods.EntGroups.OriginalPriceStr = Number(this.data.miniappFoodGoods.singleprice) * this.data.miniappFoodGoods.nowbuynums
				this.setData({ miniappFoodGoods: this.data.miniappFoodGoods })
			} else {
				wx.showToast({ title: '不能再加了哦', icon: 'loading' })
			}

		}
	},
	gotopay: function () {//去结算
		var good = this.data.miniappFoodGoods
		if (Number(this.data.GID) == 0) {
			if (this.data.isgroup == 1 && (good.nowbuynums + (good.EntGroups.GroupSize - 1)) > good.Stock) {
				wx.showModal({
					title: '提示',
					content: '该拼团商品库存不足以开团，请选择其他规格！',
					showCancel: false
				})
				return
			}
		}

		if (this.data.chooselabelslength == this.data.goodsAttrList.length) {

			// 添加入购物车
			this.data.shopcartList = []
			var specinfo = ''
			var attrspacstr = ''
			for (var i = 0; i < this.data.specidArray.length; i++) {
				specinfo += this.data.specnameArray[i] + ' '
				attrspacstr += this.data.specidArray[i] + '_'
			}
			this.data.shopcartList.push({ goodname: good.GoodsName, buynums: good.nowbuynums, size: this.data.size, attrSpacStr: attrspacstr, SpecInfo: specinfo, newCartRecord: 1, goodid: good.EntGroups.EntGoodsId, discountprice: good.chooseprice, price: good.EntGroups.OriginalPriceStr, discount: good.discount, isgroup: this.data.isgroup, HeadDeductStr: good.EntGroups.HeadDeductStr, GID: this.data.GID })
			var allprice = Number(this.data.shopcartList[0].price)
			var alldiscountprice = Number(this.data.shopcartList[0].discountprice)

			var url = '../group/group_updateOrder'
			wx.navigateTo({
				url: url + '?shopcartList=' + JSON.stringify(this.data.shopcartList) + '&allprice=' + allprice + '&alldiscountprice=' + alldiscountprice + '&isshareGroup=' + this.data.isshareGroup,
			})
		} else {
			wx.showToast({ title: '请选择规格', icon: 'loading' })
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
		app.new_login(function (res) {
			template.GetVipInfo(that, function (cb) {
				if (cb == 'isok') {
					tool.GetFoodsDetail(that, 4, function () { })
					tool.GetGoodsDtl(that, options.groupid)
				}
			})
		})
		that.data.GID = options.GID != undefined ? options.GID : 0
		that.setData({ isshareGroup: options.isshareGroup || 0 })
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
			that.data.timeInter = setInterval(function () {
				if (that.data.GroupSponsorList.length > 0) {
					var nowTime = new Date().getTime()
					for (var i = 0; i < that.data.GroupSponsorList.length; i++) {
						var endtimeList = ((new Date(that.data.GroupSponsorList[i].ShowEndTime)).getTime() - nowTime)
						that.data.GroupSponsorList[i].endtimeList = template.formatDuring(endtimeList)
					}
					that.setData({ GroupSponsorList: that.data.GroupSponsorList })
				}
			}, 1000)
		}, 1000)
	},

  /**
   * 生命周期函数--监听页面隐藏
   */
	onHide: function () {
		clearInterval(this.data.timeInter)
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

		template.stopPullDown()
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
		return {
			title: this.data.miniappFoodGoods.GoodsName,
			path: 'pages/group/group_goodinfo?groupid=' + this.data.miniappFoodGoods.Id,
			imageUrl: this.data.miniappFoodGoods.ImgUrl
		}
	},

	onPageScroll: function (e) {

	},


})