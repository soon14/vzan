// pages/editorAddress/editorAddress.js
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
		getLocation: '点击选择位置',
		addInfo: '',
		allInfo: [],
		openId: '',
		Id: 0,
		// 省市区选择器
		// region: ['广东省', '广州市', '海珠区'],
		region: ['', '', ''],
		regionstr: "请选择所在地区",
		customItem: '全部',
		nickname: "",
		phone: "",
		address: "",
		// 街道选择器
		array: ['请选择', '中国', '巴西', '日本'],
		index: 0,
		// 入口循环
		// inputAddress: ['收货人', '联系电话', '所在地区', '街道'],
	},
	inputaddinfo: function (e) {
		this.setData({ addInfo: e.detail.value })
	},
	inputnickname: function (e) {
		this.setData({ nickname: e.detail.value })
	},
	inputphone: function (e) {
		this.setData({ phone: e.detail.value })
	},
	inputaddress: function (e) {
		this.setData({ address: e.detail.value })
	},
	// 省市区选择器
	bindRegionChange: function (e) {
		console.log('picker发送选择改变，携带值为', e.detail.value)
		this.setData({
			region: e.detail.value,
			regionstr: e.detail.value[0] + " " + e.detail.value[1] + " " + e.detail.value[2]
		})
	},
	// 街道选择器
	bindPickerChange: function (e) {
		console.log('picker发送选择改变，携带值为', e.detail.value)
		this.setData({
			index: e.detail.value
		})
	},
	formSubmit: function (e) {
		console.log('form发生了submit事件，携带数据为：', e.detail.value)
		var name = e.detail.value.name
		var phone = e.detail.value.phone
		var regionstr = this.data.regionstr
		var region = this.data.region
		var address = e.detail.value.address + this.data.addInfo
		var id = this.data.Id
		if (id == '' || id == 'undefined') {
			id = 0
		}
		if (name.trim().length <= 0) {
			app.ShowMsg("请输入收货人")
			return
		}
		if (phone.trim().length <= 0) {
			app.ShowMsg("请输入联系电话")
			return
		}
		if (region[0] == "全部" && region[1] == "全部" && region[2] == "全部") {
			app.ShowMsg("请选择地区")
			return
		}
		// if (regionstr.trim().length <= 0 || regionstr =="请选择所在地区") {
		//   app.ShowMsg("请选择地区")
		//   return
		// }
		if (address.trim().length <= 0 || address.trim() == '点击选择位置') {
			app.ShowMsg("请输入详细地址")
			return
		}

		this.saveAddress(id, region[0], region[1], region[2], name, phone, address)
	},
	saveAddress: function (id, Province, CityCode, AreaCode, name, phone, address) {
		if (app.globalData.weidu == '' || app.globalData.jingdu == '') {
			var Lat = this.data.allInfo.Lat
			var Lng = this.data.allInfo.Lng
		} else {
			var Lat = app.globalData.weidu
			var Lng = app.globalData.jingdu
		}
		var data = {
			Id: id, Province: '全部', CityCode: '全部', AreaCode: '全部', NickName: name, TelePhone: phone, Address: address, Lat: Lat, Lng: Lng,
		}
		var url = addr.Address.AddOrEditMyAddressDefault
		var param = {
			appid: app.globalData.appid,
			openid: app.globalData.userInfo.openId,
			addressjson: JSON.stringify(data),
		}
		var method = "POST"
		this.requestData(url, param, method, function (e) {
			if (e.data.isok == 1) {
				app.showToast("保存成功")
				app.goBackPage(1)
			} else {
				app.showToast("请填写正确信息")
			}
		})
	},
	requestData: function (url, params, method, callback) {
		wx.request({
			url: url,
			method: method,
			head: {
				'Content-Type': 'application/json'
			},
			data: params,
			success: function (success) {
				// app.goBackPage(1)
				callback(success)
			},
			fail: function (fail) {
				onError("网络请求异常", fail.statusCode, that);
			},
		});
	},
	//删除地址
	DeleteAddress: function (e) {
		var id = e.currentTarget.id
		var that = this
		var url = addr.Address.deleteMyAddress
		var param = {
			appid: app.globalData.appid,
			// appId: 307,
			openid: app.globalData.userInfo.openId,
			AddressId: that.data.Id
		}
		var method = "POST"
		app.ShowConfirm("确定删除该地址？", function (res) {
			that.requestData(url, param, method, function (e) {
				app.showToast(e.data.msg)
				if (e.data.isok == 1) {
					app.goBackPage(1)
				}
			})
		})

	},
	chooseLocal: function () {
		var that = this
		var address = that.data.address
		wx.chooseLocation({
			success: function (res) {
				var weidu = res.latitude
				var jingdu = res.longitude
				var addressInfo = res.address
				var address = res.address
				app.globalData.weidu = weidu
				app.globalData.jingdu = jingdu
				app.globalData.addressInfo = addressInfo
				that.setData({
					getLocation: address
				})
				// that.inite3(weidu, jingdu)
			},
		})
	},
  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this
		// if (options.addressInfo != '') {
		// 	this.setData({ address: options.addressInfo })
		// }
		this.setData({ Id: options.Id })
		if (app.globalData.openid == '') {
			app.getUserInfo(function (e) {
				that.setData({ userinfo: app.globalData.userInfo, openId: e.openId })
			})
		} else {
			that.setData({ userinfo: app.globalData.userInfo })
		}
		if (that.data.Id != '') {
			that.inite(that.data.Id)
		}
		// if (options.addressjson != undefined) {
		//   var address = JSON.parse(options.addressjson)
		//   if (address != undefined && address != null) {
		//     var region = [address.Province, address.CityCode, address.AreaCode]
		//     var regionstr = address.Province + " " + address.CityCode + " " + address.AreaCode
		//     that.setData({
		//       region: region,
		//       regionstr: regionstr,
		//       Id: address.Id,
		//       nickname: address.NickName,
		//       phone: address.TelePhone,
		//       address: address.Address
		//     })
		//   }
		// }

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
		var url = "pages/myAddress/myAddress"
		app.reloadpagebyurl("", url)
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

	},
	//获取我的地址列表
	inite: function (addressId) {
		var that = this
		wx.request({
			url: addr.Address.GetMyAddress,
			data: {
				AppId: app.globalData.appid,
				// AppId: 307,
				openid: app.globalData.userInfo.openId,
				isDefault: 0,
				addressId: addressId,
			},
			method: "GET",
			header: {
				'content-type': 'application/json' // 默认值
			},
			success: function (res) {
				if (res.data.isok == 1) {
					that.setData({
						allInfo: res.data.postdata.address,
						nickname: res.data.postdata.address.NickName,
						phone: res.data.postdata.address.TelePhone,
						address: res.data.postdata.address.Address,
						getLocation: res.data.postdata.address.Address,
						// regionstr: (res.data.postdata.address.Province + ' ' + res.data.postdata.address.CityCode + ' ' + res.data.postdata.address.AreaCode)
						// regionstr: ('全部' + ' ' + '全部' + ' ' + '全部')
					})
					console.log('1', res)
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
})