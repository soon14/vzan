const addr = require("../../utils/addr.js");
const http = require("../../utils/http.js");
const tools = require("../../utils/tools.js");
var QQMapWX = require('../../utils/qqmap-wx.js');
var qqmapsdk;
var app = getApp();
//我的地址
var vm = {
	ispost: false,
	loadall: false,
	list: [],
	psize: 20,
	pindex: 1,
}
//编辑地址
var vmAddrInfo = {
	id: 0,
	userid: 0,
	isdefault: 0,
	contact: "",
	phone: "",
	province: "",
	city: "",
	district: "",
	street: "",
}

//请求开关
var ispost = false;
// pages/me/address.js
Page({

  /**
   * 页面的初始数据
   */
	data: {
		userLocationAuth: false,//用户是否允许地理位置授权
		showChooseAddr: false,//是否显示选择地址
		showEditAddr: false,//是否显示编辑地址
		poisList: [],//附近低点
		vm: JSON.parse(JSON.stringify(vm)),
		editAddrInfo: JSON.parse(JSON.stringify(vmAddrInfo)),
		editIndex: -1,
		canSave: false,
		editState: "add",//编辑状态：add=添加,edit=修改
	},


  /**
   * 生命周期函数--监听页面加载
   */
	onLoad: function (options) {
		var that = this;
		console.log("vmAddrInfo", vmAddrInfo)
		wx.getSetting({
			success(res) {
				console.log(res);
				if (!res.authSetting['scope.userLocation']) {
					wx.authorize({
						scope: 'scope.userLocation',
						success: function (res) {
							console.log(res);
							that.setData({ "userLocationAuth": true });
						},
						fail: function (res) {
							console.log(res);
							that.setData({ "userLocationAuth": false });
						},
					})
				}
				else {
					that.setData({ "userLocationAuth": true });
				}
			}
		})
		http.postAsync(addr.Address.GetAddressByIp).then(function (res) {
			console.log(res);
			if (typeof res == "string")
				return;
			if (res.message != "query ok")
				return;
			var addrInfo = res.result.ad_info;

			// vmAddrInfo.province = addrInfo.province;
			// vmAddrInfo.city = addrInfo.city;
			// vmAddrInfo.district = addrInfo.district;

			that.setData({
				"editAddrInfo": JSON.parse(JSON.stringify(vmAddrInfo))
			});
		});
		qqmapsdk = new QQMapWX({
			key: 'P7TBZ-CMBKX-GAE4J-TA42W-XAGWV-6XBHG' // 必填
		});
		this.loadMore();
	},
	loadMore: function (callback) {
		var that = this;
		var d = that.data;
		var vm = that.data.vm;
		if (vm.ispost || vm.loadall)
			return;

		if (!vm.ispost) {
			this.setData({
				"vm.ispost": true,
			});
		}
		var app = getApp();
		app.getUserInfo(function (res) {
			console.log(res);
			that.setData({
				userInfo: res
			});
			tools
				.GetUserAddress({
					userId: that.data.userInfo.UserId
				})
				.then(function (res) {
					console.log(res);
					if (res.isok) {
						// if (res.postdata.length >= vm.pagesize) {
						//   vm.pageindex += 1;
						// }
						// else {
						//   vm.loadall = true;
						// }
						//vm.list = vm.list.concat(res.postdata);
						vm.list = res.data;
						vm.ispost = false;
						vm.loadall = true;
					}
					that.setData({
						vm: vm
					})
					// console.log("vm的值是", vm.list.length)
					// app.globalData.addressLength = vm.list.length
					// //将长度放在缓存中
					// wx.setStorageSync('addressLength', app.globalData.addressLength)
					// console.log("app.globalData.addressLength", app.globalData.addressLength)
					// var value = wx.getStorageSync('addressLength')
					// console.log("打印出收货地址的长度", value)


					// // var index = that.currentTarget.dataset.index
					// // console.log("1打印出当前的设置的地址编号", index)
					// // console.log(that.data.vm.list[index])

					if (callback) {
						callback();
					}
				});
		});

	},
	bindRegionChange: function (e) {
		console.log(e);
		var val = e.detail.value;
		console.log(val)
		this.data.editAddrInfo.province = val[0]
		this.data.editAddrInfo.city = val[1]
		this.data.editAddrInfo.district = val[2]
		console.log("editAddrInfo", this.data.editAddrInfo)
		this.setData({
			editAddrInfo: this.data.editAddrInfo
		});
		//this.checkInput();
	},
	//手动添加地址
	addself: function () {

		this.setData({
			editAddrInfo: JSON.parse(JSON.stringify(vmAddrInfo)),
			editIndex: -1,
			editState: 'add',
			showEditAddr: true,
		});
	},
	addwx: function () {
		// wx.getSetting({
		//   success: (res) => {
		//     console.log(res);
		//     //如果没有授权
		//     if (!res.authSetting['scope.address']) {

		//       wx.showModal({
		//         title: '提示',
		//         content: '您需要先授权,才能使用此功能',
		//         confirmText: "去设置",
		//         success: function (modelres) {

		//           if (modelres.confirm) {
		//             wx.openSetting({
		//               success: (data) => {
		//                 data.authSetting = { 
		//                   "scope.address": true,
		//                   }
		//               }
		//             })
		//           }
		//         }
		//       })
		//     }
		//     else {
		//       wx.chooseAddress({
		//         success: function (res) {
		//           console.log(res)
		//         },
		//         fail: function (res) {
		//           console.log(res);

		//         }
		//       })
		//     }
		//   }
		// })

		var that = this;
		wx.chooseAddress({
			success: function (res) {
				console.log("微信请求")
				console.log(res)
				var postData = JSON.parse(JSON.stringify(vmAddrInfo));
				postData.userid = that.data.userInfo.UserId;
				postData.contact = res.userName;
				postData.phone = res.telNumber;
				postData.province = res.provinceName;
				postData.city = res.cityName;
				postData.district = res.countyName;
				postData.street = res.detailInfo;
				postData.zipcode = res.postalCode;
				that.setData({
					"editAddrInfo": postData
				});

				that.saveAddr();
			},
			fail: function (res) {
				console.log("微信请求失败")
				console.log(res);

			}
		})

		console.log("显示id", JSON.parse(JSON.stringify(vmAddrInfo)).district)



		//
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
		// this.setData({
		//   vm:JSON.parse(JSON.stringify(vm))
		// });
		// this.loadMore();
		var that = this
		console.log("that.data.vm.list", that.data.vm.list)

	},
	hideClickAddrDetail: function () {
		this.setData({
			"showChooseAddr": false,
		});
	},

	clickAddrDetail: function () {
		var that = this;
		var editAddrInfo = that.data.editAddrInfo;
		var region = editAddrInfo.province + editAddrInfo.city + editAddrInfo.district;

		if (region === "") {
			wx.showModal({
				title: '提示',
				content: '请先选择地区',
			})
			return;
		}
		if (this.data.showChooseAddr)
			return;
		this.setData({ "showChooseAddr": true });
		// wx.getLocation({
		//   type: 'wgs84',
		//   success: function (res) {
		//     var latitude = res.latitude
		//     var longitude = res.longitude
		//     var speed = res.speed
		//     var accuracy = res.accuracy



		//   }
		// })
		qqmapsdk.reverseGeocoder({
			// location: {
			//   latitude: latitude,
			//   longitude: longitude
			// },
			get_poi: 1,
			policy: 2,
			//address_format: 'short',
			radius: 1000,
			success: function (res) {
				console.log(res);
				if (typeof res == "string") {
					return;
				}
				if (res.status !== 0) {
					return;
				}
				var poisList = res.result.pois;

				that.setData({
					"poisList": poisList
				});
			},
			fail: function (res) {
				console.log(res);
			},

		});
	},
	//选择一个地址
	chooseAddr: function (e) {
		console.log(e);
		var ds = e.currentTarget.dataset;
		var index = ds.index;
		var addrItem = this.data.poisList[index];
		var that = this;
		var editAddrInfo = that.data.editAddrInfo;
		var region = editAddrInfo.province + editAddrInfo.city + editAddrInfo.district;
		var address = addrItem.address.replace(region, "");
		this.setData({
			"editAddrInfo.street": address,
			showChooseAddr: false,
		});
		// this.checkInput();
	},

	//关闭编辑地址
	closeEdit: function () {
		this.setData({
			"showEditAddr": false,
			"showEditAddr": false,
		});
	},

	//输入详细地址
	inputDistrict: function (e) {
		this.setData({
			"editAddrInfo.street": e.detail.value,
			showChooseAddr: false,
		});
		// this.checkInput();
	},
	//输入联系人
	inputContact: function (e) {
		this.setData({
			"editAddrInfo.contact": e.detail.value,
		});
		//  this.checkInput();
	},
	//输入手机号
	inputPhone: function (e) {
		this.setData({
			"editAddrInfo.phone": e.detail.value,
		});
		//this.checkInput();
	},

	checkInput: function () {
		console.log("进入检查地址信息")
		var that = this;
		var d = that.data;
		var info = that.data.editAddrInfo;
		console.log(info)
		//联系人不能空
		if (info.concat && info.concat.replace(/\s/g, "").length == 0) {
			console.log("联系人信息为空")
			wx.showModal({
				title: '提示',
				content: '联系人信息不能为空',
			})
			that.setData({ canSave: false });
			return;
		}
		//手机11位
		if (!/^1[\d]{10}$/.test(info.phone)) {
			console.log("手机信息")
			wx.showModal({
				title: '提示',
				content: '手机号码应该为11位',
			})

			that.setData({ canSave: false });
			return;
		}
		//区域
		if (!info.province || !info.city || !info.district) {
			console.log("区域信息")
			that.setData({ canSave: false });
			wx.showModal({
				title: '提示',
				content: '地区信息不能为空',
			})
			return;
		}
		//详细地址不能空
		if (!info.street || info.street.replace(/\s/g, "").length == 0) {
			that.setData({ canSave: false });
			wx.showModal({
				title: '提示',
				content: '详细信息不能为空',
			})
			return;
		}

		that.setData({ canSave: true });
	},
	//保存地址
	saveAddr: function () {
		var that = this;

		this.checkInput();


		if (ispost)
			return;

		if (!ispost)
			ispost = true;
		// wx.showLoading({
		//   title: '提交中',
		// })
		if (this.data.userInfo && this.data.userInfo.UserId) {
			var postData = this.data.editAddrInfo;
			if (postData.id == 0) {
				postData.userid = that.data.userInfo.UserId;
			}
			tools
				.EditUserAddress(postData)
				.then(function (res) {
					console.log("手动添加地址成功")
					console.log(res);
					if (res.isok) {
						//that.reload();
						// wx.showToast({
						//   title: res.msg,
						// })
						var list = that.data.vm.list;
						if (postData.id == 0) {
							postData.id = res.data;
							list.push(postData);
						}
						else {
							list[that.data.editIndex] = postData;
						}

						//如果当前就一个地址，把第一个设为默认
						if (list.length == 1) {
							console.log("打印list", list)
							list[0].isdefault = 1;
							that.changeDefault(0)
						}

						that.setData({
							"vm.list": list,
							editIndex: -1,
							showEditAddr: false,
							showChooseAddr: false,
						});


					}
					// else {
					//   wx.showModal({
					//     title: '操作失败',
					//     content: res.msg,
					//   })
					// }
					ispost = false;
				});
		}
		else {
			wx.showModal({
				title: '提示',
				content: '请先登录！',
			})
		}

	},
	//更改默认
	changeDefault: function (e) {
		console.log("进入设置默认", e)
		var that = this;
		var index = -1;
		if (typeof e == "object") {
			console.log("object")
			index = e.currentTarget.dataset.index
			console.log(that.data.vm.list[index])
			app.globalData.defaultAddress = that.data.vm.list[index]
			// //将默认设置的地址存在缓存中
			// wx.setStorageSync('defaultAddress', that.data.vm.list[index])
			// var value = wx.getStorageSync('defaultAddress')

		}
		else if (typeof e == "number") {
			console.log("number")
			index = e;
		}
		if (index === -1 || this.data.vm.list.length <= 0)
			return;

		var selectItem = this.data.vm.list[index];

		tools
			.changeUserAddressState({
				id: selectItem.id,
				userid: this.data.userInfo.UserId,
				isdefault: 1
			})
			.then(function (res) {
				console.log(res);
				if (res.isok) {
					that.data.vm.list.forEach(function (obj) {
						obj.isdefault = 0;
					});
					that.data.vm.list[index].isdefault = 1;
					that.setData({
						"vm.list": that.data.vm.list
					});
					console.log("list的值", that.data.vm.list)
				}
				else {
					wx.showModal({
						title: '提示',
						content: res.msg,

					})
				}
			});
	},
	reload: function () {
		var that = this;
		that.setData({
			vm: JSON.parse(JSON.stringify(vm)),
			editAddrInfo: JSON.parse(JSON.stringify(vmAddrInfo)),
			showChooseAddr: false,
			showEditAddr: false,
		});
		that.loadMore();
		console.log("reload后的数值", that.data.vm.list)
	},
	//点击编辑
	editAddress: function (e) {
		var index = e.currentTarget.dataset.index;
		var selectItem = this.data.vm.list[index];
		var editAddrInfo = this.data.editAddrInfo;
		this.setData({
			editState: 'edit',
			editIndex: index,
			showEditAddr: true,
			editAddrInfo: selectItem,
			canSave: true,
		});
	},
	delAddress: function (e) {
		var index = e.currentTarget.dataset.index;
		var selectItem = this.data.vm.list[index];
		var that = this;
		wx.showModal({
			content: '确定要删除该地址吗？',
			success: function (res) {
				if (res.confirm) {
					tools
						.DeleteUserAddress({ id: selectItem.id })
						.then(function (d) {
							if (d.isok) {
								var list = that.data.vm.list;
								list.splice(index, 1);
								//如果删除的是默认地址 删除后将第一个作为默认地址
								if (list.length > 0 && !list.find(function (item) { return item.isdefault == 1 })) {
									list[0].isdefault = 1;
									that.changeDefault(0);
								}
								// else {

								//   wx.removeStorageSync('keyaddressLength')
								//   wx.removeStorageSync('defaultAddress')

								// }
								that.setData({
									"vm.list": list
								});

							}
							else {
								wx.showModal({
									title: '提示',
									content: d.msg,
								})
							}
						});
				}
			}
		})
	},


	turn: function (e) {
		var that = this

		//将当前选择的保存在shippingAddress中
		var index = e.currentTarget.dataset.index;
		app.globalData.shippingAddress = that.data.vm.list[index];
		getCurrentPages()[getCurrentPages().length - 2].data.requestMark = true
		wx.navigateBack()

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

  /**
   * 用户点击右上角分享
   */
	onShareAppMessage: function () {

	}
})