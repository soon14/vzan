const app = getApp();
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
const WxParse = require('../../../wxParse/wxParse.js');
Page({
	data: {
		tabTit: 1,
		this_dish_id: 0,
		this_table_id: 0,
		this_type_text: '点餐',
		this_order_type: 1,
		this_is_ziqu: 0,
		this_order_id: 0,
		scrollDown: false,
		cart_list_isshow: false,
		dish_data: [],
		cart_list: [],
		dish_yingye_status_text: '未营业',
		dish_yingye_status_val: 2,
		dish_button_status: true,
		submitIsLoading: false,
		guigeIsShow: false,
		this_goods_attr_all_price: 0,
		goods_attr_select: {},
		goods_specification: '',
		goods_a_info: {},
		this_g_cate_key: null,
		this_g_good_key: null,
		is_goods_show_status: false,
		this_goods_show_info: null,
		this_cate_data_id: 0,
		this_cate_data_name: '商品',
		this_cate_data_goods: null,
		this_search_isshow: false,
		this_search_key: '',

		title_sw_id: 'scroll_to_0', //控制点餐列表交互 勿删
		sw_id: 'scroll_to_0',//控制点餐列表交互 勿删
	},
	go_dish_index_bind: function () {
		wx.switchTab({
			url: '/pages/restaurant/restaurant-home/index',
			fail: function () {
				wx.navigateTo({
					url: '/pages/restaurant/restaurant-home/index',
				})
			}
		})
	},
	//搜索
	search_goods_bind: function () {
		this.setData({ this_search_isshow: this.data.this_search_isshow ? false : true, this_search_key: '' });
	},
	go_goods_search_bind: function (e) {
		this.setData({ this_search_key: e.detail.value });
		this.get_goods_list_bycid(0, 0)
		// if (e.detail.value != '') {
		// 	this.onShow();
		// }
	},
	//获取商品详情
	goods_info_bind: function (e) {
		var that = this;
		//动态获取商品规格
		requestUtil.post(_DuoguanData.getGoodsInfo, { goods_id: e.currentTarget.id }, (info) => {
			WxParse.wxParse('g_description', 'html', info.g_description, that, 5);
			that.setData({ this_goods_show_info: info, is_goods_show_status: true });
		}, this, { isShowLoading: true });
	},
	close_goods_show_status: function () {
		this.setData({ is_goods_show_status: false });
	},
	huodong_info_bind: function () {
		var that = this;
		wx.navigateTo({
			url: '../restaurant-active/index?&dish_id=' + that.data.this_dish_id
		});
	},
	//领券页面
	huodong_quan_info_bind: function () {
		var that = this;
		wx.navigateTo({
			url: '../restaurant-juan/index?&dish_id=' + that.data.this_dish_id
		});
	},
	//订单
	go_user_order_bind: function (e) {
		wx.navigateTo({
			url: '/pages/restaurant/restaurant-order-list/index'
		});
	},
	tabSubBind: function (e) {
		var that = this;
		var this_target = e.target.id;
		that.setData({
			tabTit: this_target
		});
	},
	//导航
	get_location_bind: function () {
		var that = this;
		var loc_lat = that.data.dish_data.dish_info.dish_gps_lat;
		var loc_lng = that.data.dish_data.dish_info.dish_gps_lng;
		wx.openLocation({
			latitude: parseFloat(loc_lat),
			longitude: parseFloat(loc_lng),
			scale: 18,
			name: that.data.dish_data.dish_info.dish_name,
			address: that.data.dish_data.dish_info.dish_address
		});
	},
	//电话
	call_phone_bind: function () {
		var that = this;
		wx.makePhoneCall({
			phoneNumber: that.data.dish_data.dish_info.dish_con_mobile
		});
	},
	//选择规格
	guige_select_bind: function (e) {
		var that = this;
		that.data.index = e.currentTarget.dataset.index
		var this_g_goods_id = e.currentTarget.id;
		that.setData({ goods_attr_select: {}, this_g_cate_key: null, this_g_good_key: null });
		//动态获取商品规格
		requestUtil.post(_DuoguanData.getOneGoodsAttr, { goods_id: this_g_goods_id }, (info) => {
			that.setData({
				goods_specification: info.good_attr,
				goods_a_info: info.good_a_info,
				guigeIsShow: true,
				this_g_cate_key: e.currentTarget.dataset.ckey || 0,
				this_g_good_key: e.currentTarget.dataset.gkey
			});
		}, this, {});
	},
	attr_select_clost_bind: function () {
		this.setData({ guigeIsShow: false, goods_attr_select: {}, this_g_cate_key: null, this_g_good_key: null });
	},
	//属性选择
	select_attr_bind: function (e) {
		var that = this;
		var this_attr_id = e.currentTarget.id;
		var this_attr_name = e.currentTarget.dataset.type;
		var this_attr_label = e.currentTarget.dataset.label;
		var this_attr_price = e.currentTarget.dataset.price;
		var datas = that.data.goods_specification;
		var this_spec_price = 0;
		var a_datas = that.data.goods_attr_select;
		var g_datas = that.data.goods_a_info;
		for (var i = 0; i < datas.length; i++) {
			if (datas[i].name == this_attr_name) {
				a_datas[datas[i].name] = null;
				for (var j = 0; j < datas[i].values.length; j++) {
					datas[i].values[j].ischeck = false;
					if (datas[i].values[j].id == this_attr_id) {
						datas[i].values[j].ischeck = true;
						a_datas[datas[i].name] = {
							's_id': this_attr_id,
							's_label': this_attr_label,
							's_price': this_attr_price
						}

					}
				}
			}
		}
		let goods_attr_all_price = 0;
		for (var si in a_datas) {
			goods_attr_all_price = goods_attr_all_price + a_datas[si].s_price;
		}
		if (goods_attr_all_price > 0) {
			g_datas.shop_price = goods_attr_all_price;
		}
		that.setData({
			goods_specification: datas,
			goods_attr_select: a_datas,
			goods_a_info: g_datas
		})
	},
	onLoad: function (options) {
		var that = this;
		console.log('餐桌二维码参数', options)
		var post_id = options.dish_id || 0;
		var table_id = options.table_id || 0;
		if (table_id > 0) {
			requestUtil.post(_DuoguanData.getDishTableInfo, { table_id: table_id }, (info) => {
				wx.setNavigationBarTitle({
					title: info.table_name,
				})
			})
		} else {
			wx.setNavigationBarTitle({
				title: '店内',
			})
		}
		var is_ziqu = options.is_ziqu || 0;
		var order_id = options.order_id || 0;
		//order_type 1店内 2外卖
		var order_type = options.order_type || 1;
		//设置类型缓存
		wx.setStorageSync('global_order_type' + post_id, order_type || 1);
		if (table_id != undefined) {
			that.setData({ this_table_id: table_id });
		} else {
			that.setData({ this_table_id: 0 });
		}
		//设置缓存
		wx.setStorageSync('dish_id', post_id)
		wx.setStorageSync('global_table_id' + post_id, table_id);
		that.setData({ this_dish_id: post_id, this_order_type: order_type, this_search_key: '', this_is_ziqu: is_ziqu, this_order_id: order_id });
	},
	onShow: function () {
		var that = this;
		that.setData({ submitIsLoading: false, this_search_isshow: false, showgotab: getCurrentPages().length == 1 ? true : false });
		let rdata = {};
		rdata.order_type = that.data.this_order_type;
		requestUtil.post(_DuoguanData.getDishInfo, rdata, (info) => {

			for (let i = 0; i < info.comment_list.length; i++) {
				info.comment_list[i].addTime = requestUtil.ChangeDateFormat(info.comment_list[i].addTime)
			}

			that.setData({
				dish_data: info,
				classifySeleted: info.dish_cate[0].id,
				dish_yingye_status_val: info.dish_info.is_yingye_status
			});
			if (that.data.this_order_type == 2) {
				that.setData({ this_type_text: info.dish_info.dish_waimai_text || '外卖' });
				wx.setNavigationBarTitle({
					title: info.dish_info.dish_name
				});
			} else {
				that.setData({ this_type_text: info.dish_info.dish_diannei_text || '店内' });
			}
			if (info.dish_info.is_yingye_status == 1) {
				//没有开启配送
        // if (that.data.this_order_type == 2 && info.dish_info.ps_open_status == 0) {
        //   that.setData({ dish_yingye_status_text: '未开启配送', dish_button_status: true });
        // }
        // else{
          that.setData({ dish_yingye_status_text: '选好了', dish_button_status: false });
        //}
			} else {
				that.setData({ dish_yingye_status_text: '未营业', dish_button_status: true });
			}
      
      
			let cart_data = wx.getStorageSync('this_dish_cart_' + that.data.this_dish_id) || {};
			// that.get_new_card_data(cart_data);
			//获取商品列表
			that.get_goods_list_bycid(info.dish_cate[0].id);
		}, this, {});
	},
	setRect: function (e) {
		if (this.data.this_cate_data_goods) {
			var f_number = e.detail.scrollTop + (this.data.huodong_quan_jiner > 0 ? -80 : 0)
			var find_classifySeleted = this.data.this_cate_data_goods.find(f => f.all_scroll > f_number)
			if (find_classifySeleted) {
				this.setData({ classifySeleted: find_classifySeleted.id, title_sw_id: find_classifySeleted.sw_name })
			}
		}
	},
	//获取商品列表
	get_goods_list_bycid: function (cate_id, is_loading = 1) {
		let base_card_data = wx.getStorageSync('this_dish_cart_' + this.data.this_dish_id) || {};
		let base_card_data_arr = Object.keys(base_card_data);
		var that = this;
		var requestData = {};
		requestData.dish_id = that.data.this_dish_id;
		requestData.cate_id = 0;
		// requestData.cate_id = cate_id;
		requestData.pageSize = 1000;
		// requestData.pageSize = 10;
		requestData.pageIndex = 1;
		requestData.search_key = that.data.this_search_key;
		requestData.order_type = that.data.this_order_type;
		requestUtil.post(_DuoguanData.getGoodsListByCateId, requestData, (info) => {

			if (info != null && info.length != 0) {
				for (let i = 0; i < info.length; i++) {
					info[i].cart_goods_number = 0
					for (var bj = 0; bj < base_card_data_arr.length; bj++) {
						if (base_card_data[base_card_data_arr[bj]].goods_id == info[i].id) {
							info[i].cart_goods_number += base_card_data[base_card_data_arr[bj]].cart_goods_number;
						}
					}

					// 优化内容

					var _cate = that.data.dish_data.dish_cate
					for (let z = 0; z < _cate.length; z++) {
						_cate[z].goodsList = []
						_cate[z].all_scroll = 0
						_cate[z].sw_name = 'scroll_to_' + z
						for (let x = 0; x < info.length; x++) {
							if (_cate[z].id == info[x].cate_id) {
								_cate[z].goodsList.push(info[x])
							}
						}
						_cate[z].sw_number = 105 * Number(_cate[z].goodsList.length) + 46
						for (let x = 0; x < z + 1; x++) {
							_cate[z].all_scroll += _cate[x].sw_number
						}
					}

					/*
					if (base_card_data[info[i].id] != undefined) {
							info[i].cart_goods_number = base_card_data[info[i].id].cart_goods_number;
					}*/
				}
				let cart_data = wx.getStorageSync('this_dish_cart_' + that.data.this_dish_id) || {};
				that.setData({ this_cate_data_goods: _cate, this_cate_data_id: cate_id, this_search_key: '' });
				that.get_new_card_data(cart_data)
			}
		}, that, { isShowLoading: is_loading ? true : false });
	},
	//分类筛选
	select_cate_goods_bind: function (e) {
		var cate_key = e.currentTarget.dataset.key;

		this.setData({ this_cate_data_name: this.data.dish_data.dish_cate[cate_key].title, classifySeleted: this.data.dish_data.dish_cate[cate_key].id, sw_id: 'scroll_to_' + cate_key });
		// this.get_goods_list_bycid(this.data.dish_data.dish_cate[cate_key].id);
	},
	//显示隐藏购物车
	cart_list_show_bind: function () {
		var that = this;
		if (that.data.cart_list_isshow == true) {
			that.setData({
				cart_list_isshow: false
			});
		} else {
			that.setData({
				cart_list_isshow: true
			});
		}
	},
	//添加购物车数量
	bind_cart_number_jia: function (e) {
		var that = this;
		let index = (that.data.index != undefined ? that.data.index : e.currentTarget.dataset.index)
		let this_goods_list = this.data.this_cate_data_goods;
		let this_goods_key = e.currentTarget.dataset.key;
		let this_goods_info = this_goods_list[this_goods_key].goodsList[index];
		let this_goods_attr = this.data.goods_attr_select;
		let this_goods_attr_key = Object.keys(this_goods_attr);

		let get_goods_attr = '';
		let get_goods_attr_id = '';
		let get_goods_attr_price = 0;
		if (this_goods_info.goods_specification.length > 0) {
			if (this_goods_attr_key.length == 0) {
				wx.showModal({
					title: '提示',
					content: "对不起，请选择商品属性",
					showCancel: false
				});
				return;
			}
			for (var gi = 0; gi < this_goods_info.goods_specification.length; gi++) {
				if (this_goods_attr[this_goods_info.goods_specification[gi].name] == undefined) {
					wx.showModal({
						title: '提示',
						content: "对不起，请选择商品【" + this_goods_info.goods_specification[gi].name + "】",
						showCancel: false
					});
					return;
				}
			}
			for (var si = 0; si < this_goods_attr_key.length; si++) {
				get_goods_attr = get_goods_attr + this_goods_attr_key[si] + ':' + this_goods_attr[this_goods_attr_key[si]].s_label + ' ';
				get_goods_attr_id = get_goods_attr_id + this_goods_attr[this_goods_attr_key[si]].s_id + ',';
				get_goods_attr_price = get_goods_attr_price + this_goods_attr[this_goods_attr_key[si]].s_price;
			}
		}
		this_goods_info.cart_goods_number += 1;
		let all_goods_price = that.returnFloat(parseFloat(get_goods_attr_price) + parseFloat(this_goods_info.shop_price));
		let base_card_data = wx.getStorageSync('this_dish_cart_' + this.data.this_dish_id) || {};
		let cart_data = base_card_data[this_goods_info.id + get_goods_attr_id] || {};
		if (Object.keys(cart_data).length > 0) {
			cart_data = {
				'goods_id': this_goods_info.id,
				'goods_name': this_goods_info.g_name,
				'cart_goods_number': base_card_data[this_goods_info.id + get_goods_attr_id].cart_goods_number + 1,
				'goods_price': get_goods_attr_price > 0 ? get_goods_attr_price : this_goods_info.shop_price,
				//'goods_price': all_goods_price,
				'goods_attr': get_goods_attr,
				'goods_attr_id': get_goods_attr_id,
				'goods_cate_id': this_goods_info.cate_id
			};
		} else {
			cart_data = {
				'goods_id': this_goods_info.id,
				'goods_name': this_goods_info.g_name,
				'cart_goods_number': 1,
				'goods_price': get_goods_attr_price > 0 ? get_goods_attr_price : this_goods_info.shop_price,
				//'goods_price': all_goods_price,
				'goods_attr': get_goods_attr,
				'goods_attr_id': get_goods_attr_id,
				'goods_cate_id': this_goods_info.cate_id
			};
		}

		base_card_data[this_goods_info.id + get_goods_attr_id] = cart_data;
		this.setData({ 'this_cate_data_goods': this_goods_list });
		this.get_new_card_data(base_card_data);
		this.attr_select_clost_bind();
		wx.setStorageSync('this_dish_cart_' + this.data.this_dish_id, base_card_data);
		that.data.index = undefined
	},
	bind_cart_number_jia_one: function (e) {
		let that = this;
		let cart_item_id = e.currentTarget.id;
		let base_card_data = wx.getStorageSync('this_dish_cart_' + this.data.this_dish_id) || {};

		let cart_data = base_card_data[cart_item_id] || {};
		let cdata_key_arr = Object.keys(cart_data);
		if (cdata_key_arr.length > 0) {
			cart_data = {
				'goods_id': cart_data.goods_id,
				'goods_name': cart_data.goods_name,
				'cart_goods_number': cart_data.cart_goods_number + 1,
				'goods_price': cart_data.goods_price,
				'goods_attr': cart_data.goods_attr,
				'goods_attr_id': cart_data.goods_attr_id,
				'goods_cate_id': cart_data.goods_cate_id
			};
			base_card_data[cart_item_id] = cart_data;

			this.get_new_card_data(base_card_data);
			wx.setStorageSync('this_dish_cart_' + this.data.this_dish_id, base_card_data);
			that.get_goods_list_bycid(that.data.this_cate_data_id, 0);
		}
	},
	//减少购物车数量
	bind_cart_number_jian: function (e) {
		let index = (this.data.index != undefined ? this.data.index : e.currentTarget.dataset.index)
		let this_goods_list = this.data.this_cate_data_goods;
		let this_goods_key = e.currentTarget.dataset.key;
		let this_goods_info = this_goods_list[this_goods_key].goodsList[index];
		this_goods_info.cart_goods_number -= 1;

		let base_card_data = wx.getStorageSync('this_dish_cart_' + this.data.this_dish_id) || {};
		if (this_goods_info.cart_goods_number <= 0) {
			delete base_card_data[this_goods_info.id];
		} else {
			let cart_data = base_card_data[this_goods_info.id] || {};
			cart_data = {
				'goods_id': this_goods_info.id,
				'goods_name': this_goods_info.g_name,
				'cart_goods_number': this_goods_info.cart_goods_number,
				'goods_price': this_goods_info.shop_price,
				'goods_attr': '',
				'goods_attr_id': '',
				'goods_cate_id': this_goods_info.cate_id
			};
			base_card_data[this_goods_info.id] = cart_data;
		}
		this.setData({ 'this_cate_data_goods': this_goods_list });
		this.get_new_card_data(base_card_data);
		wx.setStorageSync('this_dish_cart_' + this.data.this_dish_id, base_card_data);
		this.data.index = undefined
	},
	bind_cart_number_jian_one: function (e) {
		let that = this;
		let cart_item_id = e.currentTarget.id;
		let base_card_data = wx.getStorageSync('this_dish_cart_' + this.data.this_dish_id) || {};
		let cart_data = base_card_data[cart_item_id] || {};
		let cdata_key_arr = Object.keys(cart_data);
		if (cdata_key_arr.length > 0) {
			if (cart_data.cart_goods_number - 1 <= 0) {
				delete base_card_data[cart_item_id];
			} else {
				cart_data = {
					'goods_id': cart_data.goods_id,
					'goods_name': cart_data.goods_name,
					'cart_goods_number': cart_data.cart_goods_number - 1,
					'goods_price': cart_data.goods_price,
					'goods_attr': cart_data.goods_attr,
					'goods_attr_id': cart_data.goods_attr_id,
					'goods_cate_id': cart_data.goods_cate_id
				};
				base_card_data[cart_item_id] = cart_data;
			}

			this.get_new_card_data(base_card_data);
			wx.setStorageSync('this_dish_cart_' + this.data.this_dish_id, base_card_data);
			that.get_goods_list_bycid(that.data.this_cate_data_id, 0);
		}
	},

	//清空购物车
	cart_delete_bind: function () {
		var that = this;
		wx.showModal({
			title: '提示',
			content: "确认要清空购物车吗",
			confirmText: "确定",
			cancelText: "取消",
			success: function (res) {
				if (res.confirm == true) {
					let base_card_data = {};
					that.get_new_card_data(base_card_data);
					let info = that.data.this_cate_data_goods;
					if (info != null) {
						for (let i = 0; i < info.length; i++) {
							for (let z = 0; z < info[i].goodsList.length; z++) {
								info[i].goodsList[z].cart_goods_number = 0;
							}
						}
					}
					that.setData({ this_cate_data_goods: info });
					wx.setStorageSync('this_dish_cart_' + that.data.this_dish_id, base_card_data);
				} else {

				}
			}
		});
	},
	//获取最新购物车数据
	get_new_card_data: function (cdata) {
		var that = this;
		var _gL = that.data.this_cate_data_goods
		var c_all_g_number = 0;
		var c_all_g_price = 0;
		var c_all_g_yunfei = 0;
		var cdata_key_arr = Object.keys(cdata);
		if (cdata_key_arr.length > 0) {
			for (let i = 0; i < cdata_key_arr.length; i++) {
				c_all_g_number += cdata[cdata_key_arr[i]].cart_goods_number;
				c_all_g_price += cdata[cdata_key_arr[i]].cart_goods_number * cdata[cdata_key_arr[i]].goods_price;
			}
		}
		for (let z = 0; z < _gL.length; z++) {
			_gL[z].cartLength = 0
			for (let x = 0; x < cdata_key_arr.length; x++) {
				if (_gL[z].id == cdata[cdata_key_arr[x]].goods_cate_id) {
					_gL[z].cartLength += cdata[cdata_key_arr[x]].cart_goods_number
				}
			}
		}
		that.setData({
			cart_list: cdata,
			all_g_number: c_all_g_number,
			all_g_price: that.returnFloat(c_all_g_price),
			all_g_yunfei: c_all_g_yunfei,
			all_chajia: that.returnFloat(that.data.dish_data.dish_info.waimai_limit_jiner - c_all_g_price),
			this_cate_data_goods: _gL
		});
		if (c_all_g_number == 0) {
			that.setData({ cart_list_isshow: false });
		}
	},
	//下单
	goods_order_bind: function () {
		var that = this;
		var this_order_type = that.data.this_order_type;
		let base_card_data = wx.getStorageSync('this_dish_cart_' + that.data.this_dish_id) || {};
		base_card_data = JSON.stringify(base_card_data);
		let requestData = {};
		requestData.cdata = [];
		requestData.cdata = base_card_data;
		requestData.dish_id = that.data.this_dish_id;
		requestUtil.post(_DuoguanData.AddDishCart, requestData, (info) => {
			that.comfirm_goods_order();
		}, that, {});
		//添加购物车
	},
	comfirm_goods_order: function () {
		var that = this;
		//是否需要手机验证
		if (that.data.dish_data.dish_info.dish_is_sms_check == 1) {
			//读取用户手机号是否已验证
			if (!wx.getStorageSync('userInfo').TelePhone) {
				//跳转到手机认证
				wx.navigateTo({
					url: '../phone-binding/index'
				});
				return false;
			} else {
				that.setData({ submitIsLoading: true });
				wx.navigateTo({
					url: '../restaurant-order/index?dish_id=' + that.data.this_dish_id + '&table_id=' + wx.getStorageSync('global_table_id' + that.data.this_dish_id) + '&order_type=' + that.data.this_order_type + '&order_id=' + that.data.this_order_id + '&is_ziqu=' + that.data.this_is_ziqu
				});
			}
		} else {
			that.setData({ submitIsLoading: true });
			wx.navigateTo({
				url: '../restaurant-order/index?dish_id=' + that.data.this_dish_id + '&table_id=' + wx.getStorageSync('global_table_id' + that.data.this_dish_id) + '&order_type=' + that.data.this_order_type + '&order_id=' + that.data.this_order_id + '&is_ziqu=' + that.data.this_is_ziqu
			});
		}
	},
	//图片放大
	img_max_bind: function (e) {
		var that = this;
		var img_max_url = e.currentTarget.dataset.url;
		var this_img_key = e.currentTarget.dataset.key;
		var all_img_num = that.data.dish_data.comment_list[this_img_key].imgsList.length;
		var durls = [];
		for (var i = 0; i < all_img_num; i++) {
			durls[i] = that.data.dish_data.comment_list[this_img_key].imgsList[i];
		}
		wx.previewImage({
			current: img_max_url,
			urls: durls
		})
	},
	//下拉刷新
	onPullDownRefresh: function () {
		var that = this;
		that.setData({
			submitIsLoading: false,
		})
		that.onShow();
		setTimeout(() => {
			wx.stopPullDownRefresh()
		}, 1000);
	},
	onShareAppMessage: function () {
		var that = this
		return {
			title: that.data.dish_data.dish_info.dish_name,
			desc: that.data.dish_data.dish_info.dish_jieshao,
			path: 'pages/restaurant/restaurant-single/index?dish_id=' + that.data.this_dish_id + '&order_type=' + that.data.this_order_type
		}
	},
	returnFloat: function (value) {
		var value = Math.round(parseFloat(value) * 100) / 100;
		return value;
	}


})