// pages/orderList/orderList.js
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
		clientTel: 0,
		amount: 0,
		openId: '',
		// 判断购物车是否为空
		isok: 2,
		// 商品模板
		goodsList: [],
		// 是否全选状态
		selectedAllStatus: false,
		// 总金额
		total: 0,
		goodsAmount: 0,
		// 购买的商品数量
		shoplenght: 0,
		// 选择商品属性显隐
		setNature: false,
		nature: [
			{
				sort: "尺码",
				id: 1,
				size: [
					{ id: 11, name: 'L', },
					{ id: 11, name: 'M', }
				]
			},
		],
		// 商品分类名
		typename: [],
		// 商品属性
		goodInfo: [],
		index: '',
		// 编辑商品
		Goodid: 0,
		// 查看全部商品
		Goods: [],
		goodsAttrList: [],
		menus: [],
		tempInventory: 0,
		tempprice: 0,
		// 选择商品属性
		groupindex1: 0,
		groupindex2: 0,
		groupindex3: 0,
		allgroupindex: '',
		// 第一项属性名字 规格
		firstName: '',
		firstInfo: '',
		// 第二项属性名字 规格
		secondName: '',
		secondInfo: '',
		// 第三项属性名字 规格
		thirdInfo: '',
		thirdName: '',
		//选择的规格属性
		selectAttrInfo: "请选择规格属性",
		//总价
		sumprice: 0,
		//单价
		singleprice: 0,
		singlecount: 0,
		buyamount: 0,
		editeId: 0,
		goundcartindex: -1,
		GoodsState: 0,
		State: 0
	},

	// 更新购物车
	updateGood: function () {
		var index = this.data.goundcartindex
		var models = this.data.goodsList
		for (var i = 0; i < models[index].GoodsCar.length; i++) {
			var model = models[index].GoodsCar[i]
			if (model.Id == this.data.editeId) {
				model.SpecIds = this.data.allgroupindex
				model.SpecInfo = this.data.selectAttrInfo
				model.Count = this.data.buyamount
				break
			}
		}

		this.setData({
			setNature: !this.data.setNature,
			goodsList: this.data.goodsList
		})
	},
	// 获取用户手机号码
	getPhoneNumber: function (e) {
		var that = this
		app.globalData.telEncryptedData = e.detail.encryptedData
		app.globalData.telIv = e.detail.iv
		app.globalData.isgetTel = 1

		app.getPhoneNumber(function (res) {
			wx.showToast({
				title: '获取成功',
			})
			console.log("getPhoneNumber", res);
			if (res.TelePhone != '未绑定') {
				that.setData({ clientTel: res.TelePhone })
			}
		});
	},
	//  点击事件 商品是否被点中
	setChoose: function (e) {
		var GoodsState = this.data.GoodsState
		var State = this.data.State
		if (this.data.goundcartindex >= 0) {
			app.showToast("您还在编辑状态")
			return
		}
		var shoplenght = this.data.shoplenght
		var item = this.data.goodsList
		var ids = e.target.id.split('_')
		var index = ids[0]
		var index2 = ids[1]
		this.setData({
			index: index,
			index2: index2,
		})
		item[index].GoodsCar[index2].choose = !item[index].GoodsCar[index2].choose
		var tempmodel = item.find(f => f.GoodsCar.find(d => d.choose == undefined || d.choose == false) != undefined)
		var selectedAllStatus = true
		if (tempmodel != undefined) {
			selectedAllStatus = false
		}
		this.setData({
			goodsList: item,
			selectedAllStatus: selectedAllStatus
		})
		var menus = []
		for (var i = 0; i < this.data.goodsList.length; i++) {
			var tempitem = this.data.goodsList[i]
			if (tempitem != undefined && tempitem.GoodsCar.length > 0) {
				for (var j = 0; j < tempitem.GoodsCar.length; j++) {
					var item = tempitem.GoodsCar[j]
					if (item.choose == true) {
						menus.push(item)
					}
				}
			}
		}
		this.data.menus = menus
		this.setData({
			shoplenght: menus.length,
			goodsAmount: menus.length,
			GoodsState: menus.GoodsState,
			State: menus.State
		})
		console.log('!', menus)
		this.sum();
	},
	// 选择商品属性点击事件
	setgoodInfo: function (e) {
		var pid = e.currentTarget.dataset.pid
		console.log(pid)
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
		// 获取第一条属性 
		this.data.firstName = ""
		this.data.firstInfo = ""
		if (this.data.groupindex1 > 0) {
			var FirstInfo = this.data.goodsAttrList[0].SpecList.find(f => f.Id == this.data.groupindex1)
			this.data.firstInfo = FirstInfo.SpecName
			this.data.firstName = this.data.goodsAttrList[0].AttrName
		}
		// 获取第二条条属性 
		this.data.secondName = ""
		this.data.secondInfo = ""
		if (this.data.groupindex2 > 0) {
			var SecondInfo = this.data.goodsAttrList[1].SpecList.find(f => f.Id == this.data.groupindex2)
			this.data.secondInfo = SecondInfo.SpecName
			this.data.secondName = this.data.goodsAttrList[1].AttrName
		}
		// 获取第三条属性 
		this.data.thirdName = ""
		this.data.thirdInfo = ""
		if (this.data.groupindex3 > 0) {
			var ThirdInfo = this.data.goodsAttrList[2].SpecList.find(f => f.Id == this.data.groupindex3)
			this.data.thirdInfo = ThirdInfo.SpecName
			this.data.thirdName = this.data.goodsAttrList[2].AttrName
		}

		// 获取商品属性 allgroupindex
		this.data.allgroupindex = ""
		this.data.selectAttrInfo = ""

		this.data.allgroupindex = ""
		if (this.data.groupindex1 > 0) {
			this.data.allgroupindex = this.data.groupindex1 + "_"
			this.data.selectAttrInfo = this.data.firstName + ":" + this.data.firstInfo + "  "
		}
		if (this.data.groupindex2 > 0) {
			this.data.allgroupindex += this.data.groupindex2 + "_"
			this.data.selectAttrInfo += this.data.secondName + ":" + this.data.secondInfo + "  "
		}
		if (this.data.groupindex3 > 0) {
			this.data.allgroupindex += this.data.groupindex3 + "_"
			this.data.selectAttrInfo += this.data.thirdName + ":" + this.data.thirdInfo + "  "
		}
		if (this.data.goodsAttrList.length > 0) {
			var findprice = this.data.goodsList[this.data.index].GoodsCar[this.data.indextwo].goodsMsg.GASDetailList.find(f => f.id == this.data.allgroupindex)
			if (findprice) {
				this.data.sumprice = parseFloat(findprice.priceStr).toFixed(2)
				this.data.allyuanjia = parseFloat(findprice.priceStr * this.data.buyamount).toFixed(2)
				this.data.alldiscountprice = parseFloat(findprice.discountPricestr).toFixed(2)
				this.data.allzhekou = parseFloat(findprice.discountPricestr * this.data.buyamount).toFixed(2)
				this.data.discount = findprice.discount
			}
		}
		this.setData(this.data)
	},
	// 点击全选
	setAllchoose: function (e) {
		if (this.data.goundcartindex >= 0) {
			app.showToast("您还在编辑状态")
			return
		}
		var shoplenght = 0
		// 环境中目前已选状态
		var selectedAllStatus = this.data.selectedAllStatus;
		// 取反操作
		selectedAllStatus = !selectedAllStatus;
		// 购物车数据，关键是处理choose值
		var carts = this.data.goodsList;
		// 遍历
		for (var i = 0; i < carts.length; i++) {
			for (var j = 0; j < carts[i].GoodsCar.length; j++) {
				carts[i].GoodsCar[j].choose = selectedAllStatus
			}
			shoplenght += carts[i].GoodsCar.length
		}
		if (!selectedAllStatus) {
			shoplenght = 0
		}

		this.setData({
			selectedAllStatus: selectedAllStatus,
			goodsList: carts,
			shoplenght: shoplenght
		})
		this.setData({ shoplenght: shoplenght })
		this.sum();
	},
	// 计算总金额
	sum: function () {
		var carts = this.data.goodsList;
		// 计算总金额
		var total = 0;
		var yuanjia = 0;
		var shoplenght = 0;
		var menus = []
		for (var i = 0; i < carts.length; i++) {
			for (var j = 0; j < carts[i].GoodsCar.length; j++) {
				if (carts[i].GoodsCar[j].choose) {
					yuanjia += carts[i].GoodsCar[j].goodsMsg.PriceStr * carts[i].GoodsCar[j].Count
					total += carts[i].GoodsCar[j].goodsMsg.discountPricestr * carts[i].GoodsCar[j].Count
					// if (carts[i].GoodsCar[j].goodsMsg.discount == 100) {
					// 	total += carts[i].GoodsCar[j].PriceStr * carts[i].GoodsCar[j].Count;
					// } else {
					// 	total += carts[i].GoodsCar[j].goodsMsg.discountPricestr * carts[i].GoodsCar[j].Count;
					// }
					shoplenght += carts[i].GoodsCar[j].Count
					menus.push(carts[i].GoodsCar[j])
				}
			}
			this.setData({
				yuanjia: yuanjia,
				total: total,
				shoplenght: shoplenght,
				menus: menus
			})
		}
		var newValue = parseInt(total * 100);
		total = newValue / 100.00;
		// 写回经点击修改后的数组
		this.setData({
			goodsList: carts,
			total: total
		});
	},
	// 选择商品属性
	setNature: function (e) {
		var attrid = e.currentTarget.dataset.attrid
		var attrids = e.currentTarget.dataset.attrid.split("_")
		if (attrids.length > 0) {
			this.data.groupindex1 = attrids[0]
		}
		if (attrids.length > 1) {
			this.data.groupindex2 = attrids[1]
		}
		if (attrids.length > 2) {
			this.data.groupindex3 = attrids[2]
		}
		this.data.editeId = e.currentTarget.dataset.cid
		this.data.selectAttrInfo = e.currentTarget.dataset.specinfo
		this.data.buyamount = e.currentTarget.dataset.count
		this.data.allgroupindex = attrid
		var that = this
		var Goods = that.data.Goods
		var goodsAttrList = that.data.goodsAttrList
		var index = that.data.index
		var goodsList = that.data.goodsList
		var good = goodsList[index].GoodsCar[e.currentTarget.id].GoodsId
		that.data.indextwo = e.currentTarget.id
		this.data.setNature = !that.data.setNature
		this.data.Goodid = good
		that.setData(this.data)
		wx.request({
			url: addr.Address.getgoodInfo,
			data: {
				appid: app.globalData.appid,
				goodsid: that.data.Goodid,
				levelid: app.globalData.levelid
			},
			method: "GET",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok == 1) {
					var findinfo = that.data.goodsList[that.data.index].GoodsCar[that.data.indextwo].goodsMsg
					if (res.data.postdata.goodsAttrList.length > 0) {
						var findprice = findinfo.GASDetailList.find(f => f.id == attrid)
						if (findprice) {
							that.data.singlecount = findprice.count
							that.data.sumprice = parseFloat(findprice.priceStr).toFixed(2)
							that.data.allyuanjia = parseFloat(findprice.priceStr * that.data.buyamount).toFixed(2)
							that.data.alldiscountprice = parseFloat(findprice.discountPricestr).toFixed(2)
							that.data.allzhekou = parseFloat(findprice.discountPricestr * that.data.buyamount).toFixed(2)
							that.data.discount = findprice.discount
						}
					} else {
						that.data.singlecount = findinfo.Stock
						that.data.sumprice = parseFloat(findinfo.PriceStr).toFixed(2)
						that.data.allyuanjia = parseFloat(findinfo.PriceStr * that.data.buyamount).toFixed(2)
						that.data.alldiscountprice = parseFloat(findinfo.discountPricestr).toFixed(2)
						that.data.allzhekou = parseFloat(findinfo.discountPricestr * that.data.buyamount).toFixed(2)
						that.data.discount = findinfo.discount
					}

					that.setData({
						Goods: res.data.postdata.goodsdetail,
						goodsAttrList: res.data.postdata.goodsAttrList,
						singleprice: that.data.singleprice,
						singlecount: that.data.singlecount,
						sumprice: that.data.sumprice,
						allyuanjia: that.data.allyuanjia,
						alldiscountprice: that.data.alldiscountprice,
						allzhekou: that.data.allzhekou,
						discount: that.data.discount,
					})
				}
				console.log('12321321321321321', that.data.goodsAttrList)
				console.log(res)
			},
			fail: function () {
				console.log("获取首页出错")
				wx.showToast({
					title: '获取首页出错',
				})
			}
		})
	},

	// // 从购物车删除商品
	delShop: function (e) {
		var that = this
		var ids = e.target.id.split('_')
		var index = ids[0]
		var index2 = ids[1]
		var goodsList = that.data.goodsList
		var good = goodsList[index].GoodsCar[index2].Id
		wx.request({
			url: addr.Address.updateOrDeleteGoodsCarData,
			data: {
				openid: that.data.openId,
				appid: app.globalData.appid,
				goodsCarModel: [{ Id: good }],
				function: -1
			},
			method: "POST",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				app.getUserInfo(function (e) {
					that.setData({
						openId: e.openId,
						goodsList: [],
						goundcartindex: -1
					})
					that.inite()
				})
			},
			fail: function () {
				console.log("获取购物车信息出错")
			}
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {

	},
	// 点击事件 商品进入编辑状态
	setEditor: function (e) {
		var item = this.data.goodsList
		var index = e.currentTarget.id
		var state = e.currentTarget.dataset.state
		for (var i = 0; i < item.length; i++) {
			if (index == i) {
				item[i].editor = true
			}
			else {
				item[i].editor = false
			}
		}

		this.data.goundcartindex = index
		if (state) {
			var models = this.data.goodsList[index].GoodsCar
			var updatas = []
			for (var i = 0; i < models.length; i++) {
				var model = models[i]

				updatas.push({
					Id: model.Id,
					SpecIds: model.SpecIds,
					SpecInfo: model.SpecInfo,
					Count: model.Count,
				})
			}
			var that = this
			var param = {
				openid: this.data.openId,
				appid: app.globalData.appid,
				goodsCarModel: updatas,
				function: 0
			}
			var url = addr.Address.updateOrDeleteGoodsCarData
			var method = "post"

			network.requestData(url, param, method, function (e) {
				app.showToast("编辑成功")
				that.data.goundcartindex = -1
				that.inite()
			})
		}
		else {
			var carts = this.data.goodsList;
			// 遍历
			for (var i = 0; i < carts.length; i++) {
				for (var j = 0; j < carts[i].GoodsCar.length; j++) {
					carts[i].GoodsCar[j].choose = false
				}
			}
			this.setData({
				goodsList: item,
				index: index,
				selectAttrInfo: "",
				sumprice: 0,
				singleprice: 0,
				singlecount: 0,
				goodsList: carts,
			})
		}

		return
	},
	// 已选菜单 item 的+
	increaseListArray: function (e) {
		var that = this

		var item = that.data.goodsList
		var ids = e.target.id.split('_')
		var index = ids[0]
		var index2 = ids[1]
		item[index].GoodsCar[index2].Count++
		that.setData({
			goodsList: item
		})
		console.log(item)

		that.sum();
	},
	// 已选菜单 item 的-
	reduceListArray: function (e) {
		var item = this.data.goodsList
		var ids = e.target.id.split('_')
		var index = ids[0]
		var index2 = ids[1]
		if ((item[index].GoodsCar[index2].Count) > 1) {
			item[index].GoodsCar[index2].Count--
		} else {
			wx.showToast({
				title: '受不了了，宝贝不能再减少了哦',
			})
		}
		this.setData({
			goodsList: item,
		})
		this.sum();
	},
	// input框输入数量
	setValue: function (e) {
		var amount = this.data.amount
		var ids = e.target.id.split('_')
		var index = ids[0]
		var index2 = ids[1]
		var value = e.detail.value
		var item = this.data.goodsList
		var value1 = item[index].GoodsCar[index2].Count
		// console.log(item[index].GoodsCar[index2])
		for (var i = 0; i < item[index].GoodsCar[index2].goodsMsg.GASDetailList.length; i++) {
			var chooseamount = item[index].GoodsCar[index2].goodsMsg.GASDetailList.find(a => a.id == item[index].GoodsCar[index2].SpecIds)
			if (chooseamount != undefined) {
				amount = chooseamount.count
			}
			// console.log(amount)
			// console.log(item[index].GoodsCar[index2].Count)
		}
		// console.log(index,index2)
		// 将数值与状态写回
		if (value < 1) { value++ }
		if (value > amount) { value = amount }
		this.setData({
			value1: value,
		})
		item[index].GoodsCar[index2].Count = value;
		this.sum();
	},

	// 点击事件 进入编辑状态后 “+”号 增加商品数量
	valueAdd: function (e) {
		var value = this.data.buyamount
		if (value < this.data.singlecount) {
			value++
		} else {
			value = this.data.singlecount
		}
		this.setData({
			buyamount: value,
			allyuanjia: ((this.data.sumprice * value).toFixed(2)),
			allzhekou: ((this.data.alldiscountprice * value).toFixed(2)),
		})
	},
	// 点击事件 进入编辑状态后 “-”号 减小商品数量
	valueReduce: function (e) {
		var value = this.data.buyamount
		if (value <= 1) {
			value = 1
		} else {
			value = value - 1
		}
		this.setData({
			buyamount: value,
			allyuanjia: ((this.data.sumprice * value).toFixed(2)),
			allzhekou: ((this.data.alldiscountprice * value).toFixed(2)),
		})
	},
	// input框输入数量
	setValue2: function (e) {
		var value = e.detail.value
		if (value <= 1) {
			value = 1
		}
		else if (value >= this.data.singlecount) {
			value = this.data.singlecount
		}
		this.setData({
			buyamount: value,
			sumprice: ((this.data.singleprice * value).toFixed(2)),
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

		var that = this;
		app.getUserInfo(function (e) {

			if (e.avatarUrl) {
				that.setData({ amount: 0, goodsAmount: 0, goodsList: [], selectedAllStatus: false, shoplenght: 0, total: 0, menus: [] })
				that.setData({
					openId: e.openId,
					clientTel: app.globalData.userInfo.TelePhone
				})
				that.inite()
				that.sum();
			}
			else {
				wx.showModal({
					title: '提示',
					content: '请先登录',
					success: function (res) {
						if (res.confirm) {
							wx.switchTab({
								url: '/pages/me/me',
							})
						}
					}
				})
			}

		})
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
		var typeid = this.data.typeid
		app.reloadpagebyurl(typeid, "../classify/classify")
	},

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
	onPullDownRefresh: function () {
		wx.showToast({
			title: '正在加载',
			icon: 'loading',
			duration: 1500
		})

		wx.stopPullDownRefresh()
		// wx.hideToast()
		this.inite()
	},
	// 点击商品内容跳转到商品详情
	navTogoodList: function (e) {
		var ids = e.currentTarget.id.split('_')
		var index = ids[0]
		var index2 = ids[1]
		var item = this.data.goodsList
		var goodsId = this.data.goodsList[index].GoodsCar[index2].GoodsId
		wx.navigateTo({
			url: '../goodList/goodList?id=' + goodsId,
		})
	},
	// 点击商品分类跳转到分类页面
	navToclassify: function (e) {
		var that = this
		var index = e.currentTarget.id
		var item = that.data.goodsList

		app.globalData.typeid = item[index].typeid
		app.globalData.change = 1
		wx.switchTab({
			url: '../classify/classify',
		})
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

	},
	//初始化
	inite: function (e) {
		var that = this
		wx.request({
			url: addr.Address.getCarInfo,
			data: {
				appid: app.globalData.appid,
				openid: that.data.openId,
				levelid: app.globalData.levelid
			},
			method: "GET",
			header: {
				'content-type': "application/json"
			},
			success: function (res) {
				if (res.data.isok == 1) {
					for (var i = 0; i < res.data.postdata.length; i++) {
						for (var j = 0; j < res.data.postdata[i].GoodsCar.length; j++) {
							res.data.postdata[i].GoodsCar[j].originalPrice = parseFloat(res.data.postdata[i].GoodsCar[j].originalPrice / 100).toFixed(2)
						}
					}
					that.setData({
						goodsList: res.data.postdata,
						isok: res.data.isok
					})
					console.log('aaaaaaaaaaaaaaab', that.data.goodsList)
				} else {
					if (res.length <= 0) {
						that.setData({
							goodsList: []
						})
					}
				}
			},
			fail: function () {
				console.log("访问购物车出错")
				wx.showToast({
					title: '访问出错',
				})
			}
		})
	},
	//隐藏
	hiddenShow: function () {
		this.setData({
			setNature: !this.data.setNature
		})
	},
	checkgood: function (e) {
		var that = this

		var menu = that.data.menus
		var goodCarIdStr = ""
		var datas = []
		for (var i = 0; i < menu.length; i++) {

			var item = menu[i]
			goodCarIdStr += item.Id + ","
			datas.push({ ImgUrl: item.goodsMsg.ImgUrl, SpecInfo: item.SpecInfo, Introduction: item.goodsMsg.GoodsName, Price: item.PriceStr, Count: item.Count, undiscountPrice: item.goodsMsg.PriceStr, discount: item.goodsMsg.discount })
		}
		if (!datas.length == 0) {
			wx.request({
				url: addr.Address.checkGood,
				data: {
					appid: app.globalData.appid,
					openid: app.globalData.userInfo.openId,
					goodCarIdStr: goodCarIdStr
				},
				method: "GET",
				header: {
					'content-type': "application/json"
				},
				success: function (res) {

					if (res.data.isok == 1) {
						that.navToOrderList(e, datas, goodCarIdStr)
					}
					else {
						wx.showModal({
							title: '提示',
							content: '该商品已下架，请重新选择商品',
						})
						var total = that.data.total
						var shoplenght = that.data.shoplenght
						that.data.menus = []
						that.setData({
							total: 0,
							shoplenght: 0
						})
						that.inite()
					}
				},
				fail: function () {
					console.log("")
					wx.showToast({
						title: '提交订单失败',
					})
				}
			})
		} else {
			wx.showToast({
				title: '请选择商品',
			})
		}
		// 提交模拟formId
		var formId = e.detail.formId
		util.commitFormId(formId, that)
	},
	// 跳转到结算页面
	navToOrderList: function (e, datas, goodCarIdStr) {
		var that = this
		if (this.data.menus.length == 0) {
			wx.showToast({
				title: '请选择商品~',
				icon: 'loading',
				duration: 1000
			})
		}
		// else {

		// if (!(that.data.goodsList.goodsdetail.State >= 0 && that.data.goodsList.goodsdetail.IsSell == 1)) {
		//   wx.showModal({
		//     title: '提示',
		//     content: '该商品已下架或售罄',
		//     success: function (res) {
		//       if (res.confirm) {
		//         wx.navigateBack({
		//           delta: 1
		//         })
		//       } else if (res.cancel) {
		//         wx.navigateBack({
		//           delta: 1
		//         })
		//       }
		//     }
		//   })
		// }
		// else {
		// if (that.data.State == 0 || that.data.GoodsState == 0) {
		wx.showModal({
			title: '是否确认提交订单吗？',
			success: function (res) {
				if (res.confirm) {
					console.log('用户点击确定')

					var jsonstr = JSON.stringify(datas)

					wx.navigateTo({
						url: '../orderList/orderList?totalMoney=' + that.data.yuanjia + "&datajson=" + jsonstr + "&goodCarIdStr=" + goodCarIdStr + '&youhui=' + that.data.total

						// url: '../orderList/orderList?totalMoney=' + that.data.tempprice * that.data.buyamount + "&datajson=" + jsonstr + "&SpecInfo=" + SpecInfo + "&qty=" + buyNums + "&goodid=" + goodid + "&attrSpacStr=" + attrSpacStr + '&youhui=' + that.data.sumprice
					})
				} else if (res.cancel) {
					console.log('用户点击取消')
				}
			}
		})
		// }
		// }
		// console.log()
	},

})