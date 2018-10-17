// pages/user/address/index.js
import dg from '../../../utils/dg.js';
import request from '../../../utils/requestUtil.js';
import _DuoguanData, { duoguan_host_api_url as API_HOST, getAddressList as getAddressList } from '../../../utils/data.js';
const baseUrl = getAddressList;

import $ from '../../../utils/underscore.js';
import listener from '../../../utils/listener.js';

Page({

	/**
	 * 页面的初始数据
	 */
	data: {
		isAli: dg.os.isAlipay(), // 是否为ali小程序，默认为是
		isCallback: false, // 是否为回调页面，默认为否 
		isUseLocation: false, // 是否使用经纬度，默认为否 
		showPage: 'list', // list列表 form表单
		// 列表页面使用的数据
		// listUrl: '/AddressApi/info',
		listUrl: '',
		list: [],
		pageNumber: 1, // 分页参数
		pageSize: 20,
		hasMore: true,
		isShowLoading: false,
		// form表单使用的数据
		buttonIsDisabled: false,
		// url: '/AddressApi/info',
		url: '',
		id: 0, // 默认为0表示添加操作
		label: 1,
		longitude: 0, // 地图上获取的经度
		latitude: 0, // 地图上获取的纬度
		callbackAddress: {}, // 回调之后的地址信息
		address: '', // 除去详细地址之后的地址
		pickerIndex: 1, // 电话选择国家，索引默认为0，即中国
		pickerData: [
			{
				'country': '其它',
				'phone_country_area_code': '0',
				'placeholder': '国家区号+您的手机号码',
			},
			{
				'country': '中国',
				'phone_country_area_code': '86',
				'placeholder': '请输入您的手机号码',
			},
		], // 电话选择国家数据
	},

	/**
	 * 生命周期函数--监听页面加载
	 */
	onLoad: function (options) {
		let isUseLocation = (typeof (options.isUseLocation) == "undefined") ? false : (options.isUseLocation == 'true' ? true : false);
		this.setData({
			isCallback: (typeof (options.isCallback) != "undefined") ? true : false,
			isUseLocation: isUseLocation,
		});

		this.setPageTitle('list');

		this.initialize(options);
	},

	/**
	 * 生命周期函数--监听页面卸载
	 */
	onUnload: function () {
		if (this.data.isCallback) {
			let info = this.data.callbackAddress;
			listener.fireEventListener('address.choose.confirm', [info]);
		}
	},

	/**
	 * 页面相关事件处理函数--监听用户下拉动作
	 */
	onPullDownRefresh: function () {
		// 防止表单页面下拉刷新数据
		if (this.data.showPage == 'form') {
			dg.stopPullDownRefresh();
			return false;
		}

		this.setData({
			list: [],
			pageNumber: 1
		})

		let options = {}
		let search = {}
		// 需要分页的调用
		options = {
			pageNumber: 1,
			pageSize: this.data.pageSize,
			hasMore: true,
			url: this.data.listUrl,
			search: search,
		}
		this.reachBottom(options)
	},

	/**
	 * 页面上拉触底事件的处理函数
	 */
	onReachBottom: function () {
		let options = {}
		let search = {}
		// 需要分页的调用
		options = {
			pageNumber: this.data.pageNumber,
			pageSize: this.data.pageSize,
			hasMore: this.data.hasMore,
			url: this.data.listUrl,
			search: search,
		}
		this.reachBottom(options)
	},

	/**
	 * 用户点击右上角分享
	 */
	//   onShareAppMessage: function () {

	//   },

	// 以下为列表页面使用的方法

	/**
	 * 初始化
	 */
	initialize: function (options) {
		// 需要分页的调用
		options = {
			pageNumber: 1,
			pageSize: this.data.pageSize,
			hasMore: true,
			url: this.data.listUrl,
			search: [],
		}
		this.reachBottom(options)
	},

	/**
	 * 触底分页
	 */
	reachBottom: function (options) {
		// 分页加载通用模版
		if (!options.hasMore) {
			this.setData({ isShowLoading: false })
			dg.stopPullDownRefresh();
			return false
		}
		let requestUrl = baseUrl + options.url
		let requestData = { _p: options.pageNumber, _r: options.pageSize, search: options.search }
		request.post(requestUrl, requestData, (data) => {
			let orginData = this.data.list
			data = data || []
			if (data.length != 0) {
				$(data).map((item) => {
					// 数据处理
					return item
				})
			}
			orginData = (options.pageNumber == 1) ? (data || []) : orginData.concat(data || []);
			// 调整默认地址在最上面
			orginData = $(orginData).sortBy(function (item) {
				return -item.is_default;
			})
			this.setData({
				isShowLoading: false,
				hasMore: (data.length < this.data.pageSize) ? false : true,
				pageNumber: options.pageNumber + 1,
				list: orginData,
				nodata: orginData.length == 0 ? false : true,
			})
		}, this, {
				isShowLoading: false,
				completeAfter: (e) => {
					dg.stopPullDownRefresh();
				}
			})
	},

	/**
	 * 处理苹果手机 下拉刷新失败的情况
	 */
	refresh: function () {
		this.setData({
			list: [],
			pageNumber: 1
		})

		let options = {}
		let search = {}
		// 需要分页的调用
		options = {
			pageNumber: 1,
			pageSize: this.data.pageSize,
			hasMore: true,
			url: this.data.listUrl,
			search: search,
		}
		this.reachBottom(options)
	},

	/**
	 * 其它模块选择地址
	 */
	radioChange: function (e) {
		const index = e.detail.value;
		const item = this.data.list[index];
		if (this.data.isCallback) {
			this.setData({
				callbackAddress: item
			});
			if (this.data.isUseLocation) {
				if (item.longitude * 1 < 0.01) {
					dg.alert("此地址无经纬度，编辑或重选", null, '温馨提示');
					return false;
				} else if (item.qqmap_address.data == 'invalid') {
					dg.alert("此地址请先编辑或重选", null, '温馨提示');
					return false;
				}
			}
			dg.navigateBack();
		} else { // 设为默认地址
			let id = item.id;
			let requestUrl = baseUrl;
			let requestData = { id: id };
			request.post(requestUrl, requestData, (info) => {
				this.refresh(); // 模拟下拉刷新
			}, this, { isShowLoading: false });
			return false;
		}
	},

	/**
	 * 新增
	 */
	add: function (e) {
		this.setPageTitle('add');

		this.setData({
			showPage: 'form',
			id: 0,
			name: '',
			gender: 1,
			mobile: '',
			address: '',
			detail_info: '',
			address: '',
			label: 1,
			postcode: '',
		});
	},

	/**
	 * 编辑
	 */
	edit: function (e) {
		this.setPageTitle('edit');

		let id = e.currentTarget.dataset.id;
		this.setData({
			showPage: 'form',
			id: id,
		})
		let values = { id: id }
		let requestUrl = baseUrl + this.data.url
		let requestData = values
		request.post(requestUrl, requestData, (info) => {
			info.province_list = [];
			info.city_list = [];
			info.area_list = [];

			let pickerIndex = 0;
			$(this.data.pickerData).map((item, index) => {
				// 兼容处理
				if (item.phone_country_area_code == info.phone_country_area_code && info.phone_country_area_code == 0) {
					info.pickerIndex = 0;
					return item
				}
			});

			this.setData({
				...info,
			})
		}, this)
	},

	// 以下为表单页面使用的方法

	/**
	 * 选择App的收货地址
	 * 
	 * @date 2017-12-29
	 * @todo 支付宝小程序此时没有相应的接口
	 */
	chooseAppAddress: function (e) {
		let _this = this;
		wx.chooseAddress({
			success: function (res) {
				let item = {
					name: res.userName,
					mobile: res.telNumber,
					postcode: res.postalCode,
					address: res.provinceName + res.cityName + res.countyName,
					all_address: res.provinceName + res.cityName + res.countyName + res.detailInfo,
					detail_info: res.detailInfo
				};
				if (_this.data.isCallback) {
					_this.setData({
						callbackAddress: item
					});
					dg.navigateBack();
				}
			},
			fail: function (res) {
				if (res.errMsg.indexOf('deny') !== -1 || res.errMsg.indexOf('denied') > 0) {
					dg.confirm("是否重新授权获取通讯地址？", function (res) {
						if (res.confirm) {
							wx.openSetting({});
						}
					}, '授权失败');
				}
			}
		})
	},

	/**
	 * 取消
	 */
	cancel: function (e) {
		this.setPageTitle('list');

		this.setData({
			showPage: 'list',
			pickerIndex: 1,
			id: 0,
		});
	},

	/**
	 * 删除
	 */
	remove: function (e) {
		let _this = this;
		let id = e.currentTarget.dataset.id;
		dg.confirm("确定要删除收货地址吗？", function (res) {
			if (res.confirm) {
				_this.deleting(id)
			}
		}, '删除提示');
	},

	/**
	 * 删除请求
	 */
	deleting: function (id) {
		let values = [];
		values['id'] = id;
		values['request_method'] = "DELETE"; // 删除请求

		this.setData({
			buttonIsDisabled: true,
		})
		// let requestUrl = baseUrl + this.data.url;
		let requestUrl = baseUrl;
		let requestData = values;
		request.post(requestUrl, requestData, (info) => {
			// 不做处理
			let data = data;
			// 提交是否成功
			if (data == "success") {
				dg.showToast({
					title: '删除成功',
					icon: 'success',
					duration: 2000,
				})
			}
			this.setData({
				showPage: 'list',
				buttonIsDisabled: false,
				id: 0,
				name: '',
				gender: 1,
				mobile: '',
				address: '',
				detail_info: '',
				label: 1,
				postcode: '',
			})

			this.refresh(); // 模拟下拉刷新
		}, this, {
				isShowLoading: false, completeAfter: function (res) {
					this.setData({
						buttonIsDisabled: false,
					});
				}
			});
	},

	/**
	* 选择性别
	*/
	chooseGender: function (e) {
		let gender = e.currentTarget.dataset.gender || 1;
		this.setData({
			gender: gender
		})
	},

	/**
	 * 选择地址标签
	 */
	chooseLabel: function (e) {
		let label = e.currentTarget.dataset.label || 1;
		this.setData({
			label: label
		})
	},

	/**
	 * 设置当前页面的标题
	 */
	setPageTitle: function (type) {
		let title = "";
		if (type == 'add') {
			title = "新增收货地址";
		} else if (type == 'edit') {
			title = "编辑收货地址";
		} else if (type == 'list') {
			title = this.data.isCallback ? "选择收货地址" : "我的地址";
		}

		dg.setNavigationBarTitle({ title: title });
	},

	/**
	 * form表单提交
	 */
	formSubmit: function (e) {
		let values = e.detail.value;

		values.gender = this.data.gender;
		values.label = this.data.label;

		values.longitude = this.data.longitude * 1;
		values.latitude = this.data.latitude * 1;
		if (values.longitude < 0.01 || values.latitude < 0.01) {
			dg.showToast({
				title: '经纬度未获取',
				icon: 'none',
				duration: 2000,
			});
			return false;
		}
		values.is_use_location = 1; // 强制获取经纬度
		values.address = this.data.address;
		values.phone_country_area_code = this.data.pickerData[this.data.pickerIndex].phone_country_area_code; // 电话国家代码

		if (this.data.id == 0) { // 添加操作
			values.request_method = "POST";
		} else { // 编辑操作
			values.request_method = "PUT";
			values.id = this.data.id;
		}

		this.setData({
			buttonIsDisabled: true,
		})

		// let requestUrl = baseUrl + this.data.url
		let requestUrl = baseUrl 
		let requestData = values
		request.post(requestUrl, requestData, (info) => {
			// 不做处理
			let data = info;
			// 提交是否成功
			if (data.length == 0) {
				this.setData({
					buttonIsDisabled: false,
				})
				return false;
			} else {
				dg.showToast({
					title: '提交成功',
					icon: 'success',
					duration: 2000,
				})
				this.setData({
					showPage: 'list',
					id: 0,
					pickerIndex: 1,
					buttonIsDisabled: false,
				})

				this.refresh(); // 模拟下拉刷新
			}
		}, this, {
				isShowLoading: false, completeAfter: function (res) {
					this.setData({
						buttonIsDisabled: false,
					})
				}
			});
	},

	/**
	 * 选择省份
	 */
	selectRegionProvince: function (e) {
		let value = e.detail.value;
		let id = this.data.province_list[value]['id'];
		// let requestUrl = baseUrl + '/RegionApi/getCityParam';
		let requestUrl = baseUrl;
		let requestUrlData = { id: id };
		request.post(requestUrl, requestUrlData, (params) => {
			this.setData({
				...params,
				province_index: value,
			})
		}, this, {
				isShowLoading: false, completeAfter: (data) => {
					let e = { detail: { value: 0 } };
					this.selectRegionCity(e);
				}
			})
	},

	/**
	 * 选择城市
	 */
	selectRegionCity: function (e) {
		let value = e.detail.value;
		let id = this.data.city_list[value]['id'];
		// let requestUrl = baseUrl + '/RegionApi/getAreaParam';
		let requestUrl = baseUrl ;
		let requestUrlData = { id: id };
		request.post(requestUrl, requestUrlData, (params) => {
			this.setData({
				...params,
				city_index: value,
			})
		}, this, {
				isShowLoading: false, completeAfter: (data) => {
					let e = { detail: { value: 0 } };
					this.selectRegionArea(e);
				}
			})
	},

	/**
	 * 选择城市区域
	 */
	selectRegionArea: function (e) {
		let value = e.detail.value;
		let id = this.data.area_list[value]['id'];
		this.setData({
			area_index: value,
		});
	},

	/**
	 * 地图上选点获取经纬度
	 */
	location: function (e) {
		let that = this;
		dg.chooseLocation({
			fail: function (res) {
				if (that.data.isAli) {
					// @2018-03-29 支付宝目前不需要授权
				} else { // 微信
					if (res.errMsg.indexOf("auth") != -1) {
						dg.confirm("请授权用户地理位置", function (res) {
							if (res.confirm) {
								wx.openSetting({
									success: (res) => {
										if (res.authSetting) {
											let title = '';
											if (res.authSetting['scope.userLocation']) {
												title = "请在地图上选点";
											} else {
												title = "授权失败";
											}
											dg.showToast({
												title: title,
												icon: "none",
											});
										}
									}
								});
							}
						}, "温馨提示");
					}
				}
			},
			success: function (res) {
				let address = that.data.address;
				address = res.address || "";
				let detail_info = that.data.detail_info;
				if (res.name) {
					detail_info = res.name;
				}
				that.setData({
					longitude: res.longitude,
					latitude: res.latitude,
					address: address,
					detail_info: detail_info,
				});

				if (address == "") { // 没有地址的情况
					dg.alert("请在补充信息中填写完整的地址");
				}
			}
		});
	},

	/**
	 * 电话选择国家时的picker事件
	 */
	pickerChange: function (e) {
		let index = e.detail.value;
		this.setData({
			pickerIndex: index,
		});
	},
})