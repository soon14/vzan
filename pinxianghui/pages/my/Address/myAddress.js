// pages/my/Address/myAddress.js
var http = require('../../../script/pinxianghui.js');
var template = require('../../../script/template.js');
var addr = require('../../../utils/addr.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
	data: {
		peisong_type: [
			{ content: '商家配送', id: 0 },
			{ content: '到店自取', id: 1 },
			{ content: '面对面交易', id: 2 }
		],
		typeid: 0, //0商家配送 1到店自取 2面对面交易
		storeid: 0,
		ischooseAddress: false,//从下单页面进入true点击返回 从我的入口false点击无反应
		ziqu: {
			name: '',
			phone: '',
		},
	},
	choose_type: function (e) {
		var that = this
		var index = e.currentTarget.dataset.typeid
		var name = e.currentTarget.dataset.name

		if (index == 0) {
			this.beginload(0)
		}
		if (index == 1) {
			this.beginload(1)
		}
		that.setData({ typeid: index })
	},

	addAddress_byhand: function (e) {
		var item = JSON.stringify(e.currentTarget.dataset.item)
		if (item) {
			var url = '/pages/my/Address/editmyAddress?item=' + item
		} else {
			var url = '/pages/my/Address/editmyAddress'
		}
		template.gonewpage(url)
	},
	addAddress_byWx: function () {
		var that = this
		var g = getApp().globalData;
		wx.chooseAddress({
			success: function (res) {
				var data = {}
				data.id = 0 //地址id 新增地址0 编辑地址！=0
				data.userId = g.userInfo.UserId//用户userid
				data.consignee = res.userName//用户名字
				data.mobile = res.telNumber//手机号码
				data.province = res.provinceName//省
				data.city = res.cityName//市
				data.district = res.countyName//区
				data.address = res.detailInfo//详细地址
				data.state = 1
				data.isDefault = 0
				http.gRequest(addr.addAddress, data, function (callback) {
					template.showtoast('添加成功', 'success')
					that.beginload(0)
				})
			}
		})
	},
	delAddress: function (e) {
		var that = this
		var id = e.currentTarget.dataset.addressid
		wx.showModal({
			title: '提示',
			content: '是否确认删除该地址？',
			success: function (res) {
				if (res.confirm) {
					http.gRequest(addr.deleteAddress, { id: id }, function () {
						template.showtoast('删除成功', 'success')
						that.beginload(0)
					})
				}
			},
		})
	},
	set_addressDefault: function (e) {
		console.log(e.currentTarget.dataset.item)
		var g = getApp().globalData;
		var that = this
		var _d = e.currentTarget.dataset.item
		var data = {}
		data.id = _d.id //地址id 新增地址0 编辑地址！=0
		data.userId = g.userInfo.UserId//用户userid
		data.consignee = _d.consignee//用户名字
		data.mobile = _d.mobile//手机号码
		data.province = _d.province//省
		data.city = _d.city//市
		data.district = _d.district//区
		data.address = _d.address//详细地址
		data.state = _d.state
		data.isDefault = 1
		http.gRequest(addr.addAddress, data, function (callback) {
			that.beginload(0)
			template.showtoast('设置成功', 'success')
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		if (options.storeid) {
			http.gRequest(addr.StoreInfo, { id: options.storeid }, function (callback) {
				var setting = callback.data.obj.storeInfo.setting
				var _pt = that.data.peisong_type
				if (setting.openZq == 0) {
					_pt.splice(1, 1)
					that.setData({ peisong_type: _pt })
				}
			})
			that.setData({
				storeid: (options.storeid ? options.storeid : 0),
				ischooseAddress: true,
			})
			wx.setNavigationBarTitle({ title: '选择配送方式' })
		} else {
			wx.setNavigationBarTitle({ title: '收货地址' })
		}
	},
	inputName: function (e) {
		app.globalData.ziquInfo.name = e.detail.value
	},
	inputPhone: function (e) {
		app.globalData.ziquInfo.phone = e.detail.value
	},
	chooseAddress: function (e) {
		var that = this
		var _zq = app.globalData.ziquInfo
		var item = e.currentTarget.dataset.item
		item.peisong_type = that.data.typeid
		if (that.data.ischooseAddress) { //如果是到店自取 信息未完善则进行提示
			if (that.data.typeid == 1) {
				item.consignee = _zq.name
				item.mobile = _zq.phone
				item.addressinfo = item.address
				if (!_zq.name || !_zq.phone) {
					template.showtoast('请填写信息', 'none')
					return
				}
			}
			app.globalData.myAddress = item
			template.goback(1)
		}
	},
	choosePeisongType: function () {
		app.globalData.myAddress.peisong_type = 2
		template.goback(1)
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
		this.beginload(this.data.typeid)
		if (Object.keys(app.globalData.ziquInfo).length > 0) {
			this.setData({ ziqu: app.globalData.ziquInfo })
		}
	},
	beginload: function (typeid) {
		var that = this
		if (typeid == 0) {
			http.gRequest(addr.AddressList, {}, function (callback) {
				var data = callback.data.obj
				for (let i = 0; i < data.length; i++) {
					data[i].addressinfo = data[i].province + data[i].city + data[i].district + data[i].address
				}
				that.setData({ addressList: data })
			})
		} else {
			var data = {}
			data.aid = app.globalData.aid
			data.storeId = that.data.storeid
			http.gRequest(addr.ZqStoreList, data, function (callback) {
				that.setData({ storeAddressList: callback.data.obj.placeList })
			})
		}
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

	},

  /**
   * 页面上拉触底事件的处理函数
   */
	onReachBottom: function () {

	},
})