const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
var app = getApp();
Page({
	data: {
		dish_data: [],
		this_address_data: null,
		this_address_str: '请选择地址',
		this_u_lat: 0,
		this_u_lng: 0,
		this_dish_id: 0,
		this_show_type: 0,
		this_edit_type: 'add',
		this_fapiao_leixing: 1,
		this_fapiao_info: null,
		wx_q_data: {}
	},
	onLoad: function (op) {
		this.setData({ isUserSetting: op.isUserSetting ? op.isUserSetting : 0 });
		this.getMylist();
	},
	getMylist: function () {
		var that = this;
		var rq_data = {};
		rq_data.dish_id = that.data.this_dish_id;
		requestUtil.post(_DuoguanData.getAddressList, rq_data, (info) => {
			that.setData({ this_address_data: info });
		}, this, {});

	},
	//选择地图
	choose_addresss_bind: function (e) {
		var that = this;
		wx.chooseLocation({
			success: function (res) {
				that.setData({ this_address_str: res.address, this_u_lat: res.latitude, this_u_lng: res.longitude });
			},
			fail: function (res) {
				console.log(res)
			}
		})
	},
	//新增发票
	fapiao_add_bind: function (e) {
		var that = this
		if (e.currentTarget.dataset.id == 0) {
			this.setData({ this_show_type: 1, this_address_str: '请选择地址', this_u_lat: 0, this_u_lng: 0 });
		} else {
			// that.fapiao_formSubmit
			wx.chooseAddress({
				success: function (res) {
					that.data.this_fapiao_leixing = 0 //选择添加微信地址不返回性别，此时性别为0（未知）
					var q = that.data.wx_q_data
					q.consignee = res.userName
					q.mobile = res.telNumber
					q.address = res.provinceName + res.cityName + res.countyName + res.detailInfo
					q.u_lat = 0
					q.u_lng = 0
					q.buchong = ''
					that.fapiao_formSubmit(e)
				}
			})
		}
	},
	//取消新增
	quxiao_add_bind: function () {
		this.setData({ this_show_type: 0, this_edit_type: 'add', this_fapiao_leixing: 1, this_fapiao_info: null });
	},
	//切换类型
	chang_fapiao_leixing_bind: function () {
		this.setData({ this_fapiao_leixing: this.data.this_fapiao_leixing == 1 ? 2 : 1 });
	},
	//更新发票
	fapiao_formSubmit: function (e) {
		var that = this;
		var q_data = e.detail.value || that.data.wx_q_data;
		q_data.u_sex = that.data.this_fapiao_leixing;
		if (that.data.this_fapiao_info != null) {
			q_data.id = that.data.this_fapiao_info.id;
		}
		requestUtil.post(_DuoguanData.addAddress, q_data, (info) => {
			that.setData({ this_show_type: 0, this_edit_type: 'add', this_fapiao_leixing: 1, this_fapiao_info: null });
			that.getMylist();
		}, this, {});
	},
	//编辑发票
	fapiao_edit_bind: function (e) {
		var that = this;
		var fapiao_id = e.currentTarget.id;
		that.data.fapiao_id = fapiao_id
		requestUtil.post(_DuoguanData.getAddresssInfo, { id: fapiao_id }, (info) => {
			console.log(info)
			that.setData({ this_fapiao_info: info, this_address_str: info.address, this_u_lat: info.u_lat, this_u_lng: info.u_lng, this_fapiao_leixing: info.u_sex, this_edit_type: 'edit', this_show_type: 1 });
		}, this, {});
	},
	//删除发票
	delete_fapiao_bind: function () {
		var that = this;
		var fapiao_id = that.data.this_fapiao_info.id;
		requestUtil.post(_DuoguanData.deleteAddress, { id: fapiao_id }, (info) => {
			that.setData({ this_show_type: 0, this_edit_type: 'add', this_fapiao_leixing: 1, this_fapiao_info: null });
			that.getMylist();
		}, this, {});
	},
	//选择收货地址
	select_fapiao_bind: function (e) {
		var fapiao_data = e.currentTarget.dataset;
		wx.setStorageSync('dish_select_address_id', fapiao_data.id);
		wx.navigateBack({ delta: 1 });
	}
})