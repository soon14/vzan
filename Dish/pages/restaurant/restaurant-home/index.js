const app = getApp();
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
const QQMapWX = require('../../../utils/qqmap-wx-jssdk.min.js');// 引入SDK核心类
const qqmapsdk = new QQMapWX({ key: '7DWBZ-XEW6R-HGGWQ-WZYXJ-FZUAV-MBBDY' });// 实例化API核心类

Page({
	data: {
		this_options: {},
		this_dish_config_info: {},
		this_dish_id: 0,
		this_dish_info: '',
		this_dish_type: '',
		this_cate_id: 0,
		this_dish_cate_data: '',
		this_dish_cate_num: 0,
		this_ruzhu_show_status: true,
		this_latitude_data: 0,
		this_longitude_data: 0,
		this_search_key: '',
		glo_is_load: true,
		this_page_size: 1,
		this_page_num: 10,
		dish_sort_type: 1,
		dish_list_data: [],
		is_loadmore: true,
		this_is_card_open: false,
		this_is_user_card: 0,
		this_user_card_info: null,
		this_quan_list: null,
		quan_is_show: true
	},
	//隐藏弹优惠券
	bind_qun_hidden: function () {
		this.setData({ quan_is_show: false });
	},
	//领取优惠券
	go_lingqu_quan_bind: function () {
		var that = this;
		wx.navigateTo({
			url: '../get-redbag/index',
			success: function () {
				that.setData({ quan_is_show: false });
			}
		});

	},
	//扫码
	shop_saoma_bind: function () {
		wx.scanCode({
			success: (res) => {
				if (res.path) {
					wx.navigateTo({
						url: '/' + res.path
					});
				}
			}
		})
	},
	dish_info_bind: function (e) {
		var that = this;
		wx.setStorageSync("dish_id", e.currentTarget.id || 0);
		let t_index = e.currentTarget.dataset.index;
		let t_dish_info = that.data.dish_list_data.index_dish_list[t_index];
		if (t_dish_info.dish_is_open_tomini == 1) {
			wx.navigateToMiniProgram({
				appId: t_dish_info.dish_tomini_appid,
				path: t_dish_info.dish_tomini_appurl
			});
			return;
		}

		wx.navigateTo({
			url: '../restaurant-home-info/index?dish_id=' + e.currentTarget.id
		});
	},
	webview_formsubmit: function (e) {
		var that = this;
		wx.navigateTo({
			url: '../webview_jianjie/index?dish_id=' + that.data.this_dish_id
		});
	},
	diancan_formsubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		that.insertFormID(form_id);
		if (that.data.this_dish_info.dish_is_diannei == 1) {
			if (that.data.this_dish_info.dish_is_rcode_open == 1) {
				wx.scanCode({
					success: (res) => {
						//添加到扫码日志
						// requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanDish/Api/addScodeLog.html', { path: res.path }, (info) => {

						// }, that, { isShowLoading: false });
						if (res.path) {
							wx.navigateTo({
								url: '/' + res.path
							});
						}
					}
				});
			} else {
				wx.navigateTo({
					url: '../restaurant-single/index?dish_id=' + that.data.this_dish_id + '&order_type=1'
				});
			}
		} else {
			wx.showModal({
				title: '提示',
				content: "对不起，暂不支持店内点餐",
				showCancel: false
			});
			return;
		}
	},
	go_dish_index_bind: function () {
		wx.switchTab({
			url: '/pages/restaurant/restaurant-home/index'
		})
	},
	//自提
	ziqu_formsubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		that.insertFormID(form_id);

		if (that.data.this_dish_info.dish_is_ziqu == 1) {
			wx.navigateTo({
				url: '../restaurant-single/index?dish_id=' + that.data.this_dish_id + '&order_type=1&is_ziqu=1'
			});
		} else {
			wx.showModal({
				title: '提示',
				content: "对不起，暂不支持自提",
				showCancel: false
			});
			return;
		}
	},
	//预订
	yuding_formsubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		that.insertFormID(form_id);
		if (that.data.this_dish_info.dish_is_yuding == 1) {
			wx.navigateTo({
				url: '../restaurant-reserve/index?dish_id=' + that.data.this_dish_id
			});
		} else {
			wx.showModal({
				title: '提示',
				content: "对不起，暂不支持预定",
				showCancel: false
			});
			return;
		}
	},
	//排队
	paidui_formsubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		that.insertFormID(form_id);
		if (that.data.this_dish_info.dish_is_paidui == 1) {
			wx.navigateTo({
				url: '../paidui/index?dish_id=' + that.data.this_dish_id
			});
		} else {
			wx.showModal({
				title: '提示',
				content: "对不起，暂不支持排队",
				showCancel: false
			});
			return;
		}
	},
	insertFormID: function (form_id) {
		var that = this;
		requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php?s=/addon/DuoguanUser/Api/addUserFormId.html', { form_id: form_id }, (info) => {

		}, that, { isShowLoading: false });
	},
	//外卖
	waimai_formsubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		that.insertFormID(form_id);
		if (that.data.this_dish_info.dish_is_waimai == 1) {
			wx.navigateTo({
				url: '../restaurant-single/index?dish_id=' + that.data.this_dish_id + '&order_type=2'
			});
		} else {
			wx.showModal({
				title: '提示',
				content: "对不起，暂不支持外卖",
				showCancel: false
			});
			return;
		}
	},
	//入驻
	shop_ruzhu_bind: function () {
		wx.navigateTo({
			url: '../restaurant-ruzhu/index'
		});
	},
	//转账
	zhuanzhang_bind: function (e) {
		wx.navigateTo({
			url: '../pay/index?dish_id=' + e.currentTarget.id
		});
	},
	//通用跳转
	go_nav_url_base: function (e) {
		wx.navigateTo({ url: e.currentTarget.dataset.url });
	},
	//日志
	go_card_log_bind: function () {
		var that = this;
		wx.navigateTo({
			url: '../restaurant-card-log/index?dish_id=' + that.data.this_dish_id
		});
	},
	//我的会员卡
	go_user_card_info: function () {
		wx.navigateTo({
			url: "../restaurant-card/index?dish_id=" + this.data.this_dish_id
		});
	},
	//导航
	get_location_bind: function () {
		wx.showToast({
			title: '地图加载中',
			icon: 'loading',
			duration: 10000,
			mask: true
		});
		var that = this;
		var loc_lat = that.data.this_dish_info.dish_gps_lat;
		var loc_lng = that.data.this_dish_info.dish_gps_lng;
		wx.openLocation({
			latitude: parseFloat(loc_lat),
			longitude: parseFloat(loc_lng),
			scale: 18,
			name: that.data.this_dish_info.dish_name,
			address: that.data.this_dish_info.dish_address
		});
	},
	//电话
	call_phone_bind: function () {
		var that = this;
		wx.makePhoneCall({
			phoneNumber: that.data.this_dish_info.dish_con_mobile
		});
	},
	//分类
	dish_category_click: function (e) {
		this.setData({ this_cate_id: e.currentTarget.id, this_search_key: '' });
		this.loadIndexMuchData();
	},
	onLoad: function (options) {
		var that = this;
		that.setData({
			this_options: options
		});

		//判断门店类型　单门店直接跳转　多门店显示首页
		requestUtil.post(_DuoguanData.getDishConfig, {}, (info) => {
			var data_options = that.data.this_options;
			that.setData({ this_ruzhu_show_status: info.dish_rz_open, this_dish_config_info: info });
			if (data_options.d_type == undefined) {
				if (info.dish_type == 0) {
					that.setData({
						this_dish_type: 'single',
						this_dish_id: info.dish_id
					});
					that.loadSingleDishData();
				} else if (info.dish_type == 2) {
					that.setData({
						this_dish_type: 'much'
					});
					that.loadMuchDishData();
				}
			} else {
				var d_type = data_options.d_type;
				if (d_type == 'single') {
					if (data_options.dish_id != undefined && data_options.dish_id > 0) {
						that.setData({
							this_dish_type: 'single',
							this_dish_id: data_options.dish_id
						});
						that.loadSingleDishData();
					} else {
						that.setData({
							this_dish_type: 'much'
						});
						that.loadMuchDishData();
					}
				} else if (d_type == 'much') {
					that.setData({
						this_dish_type: 'much'
					});
					that.loadMuchDishData();
				} else {
					that.setData({
						this_dish_type: 'much'
					});
					that.loadMuchDishData();
				}
			}
		}, this, { isShowLoading: false });
	},
	onShow: function () {
		wx.hideToast();
		//var that = this;
	},
	loadSingleDishData: function () {
		var that = this;
		requestUtil.post(_DuoguanData.getDishInfo, { dish_id: that.data.this_dish_id }, (info) => {
			that.setData({ this_dish_info: info, glo_is_load: false });
			wx.setNavigationBarTitle({
				title: info.dish_name
			});
			//验证用户是否已领取会员卡
			if (info.card_open_status == 1) {
				that.setData({ this_is_card_open: true });
				that.check_user_is_card();
			} else {
				that.setData({ this_is_card_open: false });
			}
			// requestUtil.post(_DuoguanData.GetShareInfo, { mmodule: 'duoguan_dish' }, (info) => {
			// 	this.setData({ shareInfo: info });
			// });
		}, that, { isShowLoading: false });
	},
	check_user_is_card: function () {
		var that = this;
		var requestData = {};
		requestData.module_name = 'DuoguanDish';
		requestData.shop_id = that.data.this_dish_id;
		requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanMemberCard/Api/userIsCard.html', requestData, (info) => {
			that.setData({ this_is_user_card: info });
			if (info == 1) {
				//读取会员卡信息
				that.get_user_card_info();
			}
		}, that, { isShowLoading: false });
	},
	get_user_card_info: function () {
		var that = this;
		var requestData = {};
		requestData.module_name = 'DuoguanDish';
		requestData.shop_id = that.data.this_dish_id;
		requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanMemberCard/Api/getCardInfo.html', requestData, (info) => {
			if (info.is_new == 1) {
				wx.showModal({
					title: '提示',
					content: "会员卡领取成功",
					showCancel: false
				});
			}
			that.setData({ this_is_user_card: 1, this_user_card_info: info });
		}, that, { isShowLoading: true });
	},
	loadMuchDishData: function () {
		var that = this;
		if (that.data.this_dish_type != 'much') {
			return false;
		}
		var latitude = 0;
		var longitude = 0;
		// that.loadIndexMuchData();
		//获取位置信息
		wx.getLocation({
			type: 'gcj02',
			success: function (res) {
				latitude = res.latitude;
				longitude = res.longitude;
				that.setData({ this_latitude_data: latitude, this_longitude_data: longitude });
			},
			complete: function () {
				that.loadIndexMuchData(latitude, longitude);
			}
		});
	},
	loadIndexMuchData: function () {
		var that = this;
		var requestData = {};
		requestData.ws_lat = that.data.this_latitude_data;
		requestData.ws_lng = that.data.this_longitude_data;
		requestData.sort_type = that.data.dish_sort_type;
		requestData.cate_id = that.data.this_cate_id;
		requestData.keywords = that.data.this_search_key,
			requestUtil.post(_DuoguanData.getDishList, requestData, (info) => {

				for (let i = 0; i < info.index_dish_list.length; i++) {
					info.index_dish_list[i].stars = Math.round(info.index_dish_list[i].stars)
					info.index_dish_list[i].stars = Math.ceil(info.index_dish_list[i].stars)
					if (info.index_dish_list[i].ismain > 0) {
						wx.redirectTo({
							url: '/pages/restaurant/restaurant-home-info/index?dish_id=' + info.index_dish_list[i].id,
						})
						return
					}
				}

				if (info.index_dish_list == null) {
					that.setData({ is_loadmore: false });
				} else {
					if (info.index_dish_list.length < that.data.this_page_num) {
						that.setData({ is_loadmore: false });
					}
				}
				if (info.index_cate_list != null) {
					that.setData({ this_dish_cate_num: info.index_cate_list.length });
				}
				that.setData({ dish_list_data: info, this_dish_cate_data: info.index_cate_list, glo_is_load: false });
				wx.hideToast();


				//获取平台优惠券
				// requestUtil.post(_DuoguanData.GetQuanList, { limit_num: 3 }, (info) => {
				// 	that.setData({ this_quan_list: info.qlist });
				// });
			}, that, { isShowLoading: false });
	},
	swiper_top_bind: function (e) {
		var that = this;
		wx.navigateTo({
			url: e.currentTarget.dataset.url
		});
	},
	dish_search_bind: function (e) {
		this.data.this_cate_id = 0
		wx.showToast({
			title: '加载中',
			icon: 'loading',
			duration: 10000
		});
		this.loadIndexMuchData();
	},
	chang_search_val_bind: function (e) {
		var s_key = e.detail.value;
		this.setData({ this_search_key: s_key });
	},
	//排序
	datasort_bind: function (e) {
		var that = this;
		var this_target = e.currentTarget.id;
		wx.showToast({
			title: '加载中',
			icon: 'loading',
			duration: 10000
		});
		that.setData({
			dish_sort_type: this_target,
			is_loadmore: true,
			this_page_size: 1
		});
		that.loadIndexMuchData();
	},
	//图片放大
	img_max_bind: function (e) {
		var that = this;
		wx.previewImage({ current: e.target.dataset.url, urls: that.data.this_dish_info.dish_shijing_arr });
	},
	img_max_bind_zz: function (e) {
		var that = this;
		wx.previewImage({ current: e.target.dataset.url, urls: that.data.this_dish_info.dish_zizhi_arr });
	},
	//下拉刷新
	onPullDownRefresh: function () {
		var that = this;
		that.setData({ this_cate_id: 0, this_search_key: '', this_page_size: 1, is_loadmore: true, });
		that.onLoad(that.data.this_options);
		setTimeout(() => {
			wx.stopPullDownRefresh();
		}, 1000);
	},
	onReachBottom: function (e) {
		var that = this;
		wx.showNavigationBarLoading();
		if (that.data.is_loadmore == false) {
			wx.hideNavigationBarLoading();
			return false;
		}
		var requestData = {};
		requestData.pagesize = that.data.this_page_size + 1;
		requestData.pagenum = that.data.this_page_num;
		requestData.ws_lat = that.data.this_latitude_data;
		requestData.ws_lng = that.data.this_longitude_data;
		requestData.sort_type = that.data.dish_sort_type;
		requestData.keywords = that.data.this_search_key;
		requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php?s=/addon/DuoguanDish/Api/getDishList.html', requestData, (info) => {
			wx.hideNavigationBarLoading();
			if (info.index_dish_list == null) {
				that.setData({ is_loadmore: false });
			} else {
				if (info.index_dish_list.length < that.data.this_page_num) {
					that.setData({ is_loadmore: false });
				}
				var this_new_dish_data = that.data.dish_list_data;
				this_new_dish_data.index_dish_list = this_new_dish_data.index_dish_list.concat(info.index_dish_list);
				that.setData({ dish_list_data: this_new_dish_data, this_page_size: requestData.pagesize, glo_is_load: false });
			}

		}, this, { isShowLoading: false });
	},
	onShareAppMessage: function () {
		var that = this;
		var shareTitle = '智慧餐厅';
		var shareDesc = '小程序智慧餐厅,扫一扫即可点餐,无需服务员的参与，自动出单';
		var sharePath = 'pages/restaurant/restaurant-home/index';
		if (that.data.this_dish_type == 'single') {
			shareTitle = that.data.this_dish_info.dish_name;
			shareDesc = that.data.this_dish_info.dish_jieshao;
			sharePath = 'pages/restaurant/restaurant-home/index?d_type=single&dish_id=' + that.data.this_dish_id;
		} else {
			shareTitle = that.data.shareInfo.title;
			shareDesc = that.data.shareInfo.desc;
			sharePath = 'pages/restaurant/restaurant-home/index';
		}
		return {
			title: shareTitle,
			desc: shareDesc,
			path: sharePath
		}
	},
})