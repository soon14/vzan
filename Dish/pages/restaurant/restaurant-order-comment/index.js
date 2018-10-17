const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
var app = getApp()
Page({
	data: {
		score_arr: [
			{
				'val': 1,
				'ischeck': true
			},
			{
				'val': 2,
				'ischeck': true
			},
			{
				'val': 3,
				'ischeck': true
			},
			{
				'val': 4,
				'ischeck': true
			},
			{
				'val': 5,
				'ischeck': true
			}
		],
		this_order_id: 0,
		oinfo: [],
		glo_is_load: true,
		img_count_limit: 5,
		this_img_i: 0,
		this_img_max: 0,
		postimg: [],
		submitIsLoading: false,
		buttonIsDisabled: false,
		this_score_val: 5
	},
	onLoad: function (options) {
		var that = this;
		var order_id = options.oid;
		that.setData({ this_order_id: order_id })
		//请求订单详情
		requestUtil.post(_DuoguanData.OrderInfo, { oid: that.data.this_order_id }, (info) => {
			that.setData({ oinfo: info.info, glo_is_load: false });
		}, that, {});
	},
	set_score_bind: function (e) {
		var that = this;
		var max_val = e.currentTarget.id;
		var datas = that.data.score_arr;
		for (var i = 0; i < datas.length; i++) {
			if (i < max_val) {
				datas[i].ischeck = true
			} else {
				datas[i].ischeck = false
			}
		}
		that.setData({
			score_arr: datas,
			this_score_val: max_val
		});
	},
	//删除
	del_pic_bind: function (e) {
		var that = this
		var index = e.currentTarget.id;
		var datas = that.data.postimg;
		datas.splice(index, 1)
		that.setData({
			postimg: datas
		})
	},
	//上传图片
	chooseimg_bind: function () {
		var that = this
		var img_lenth = that.data.postimg.length
		var sheng_count = that.data.img_count_limit - img_lenth
		if (sheng_count <= 0) {
			wx.showModal({
				title: '提示',
				content: '对不起，最多可上传五张图片',
				showCancel: false
			})
			return false
		}
		wx.chooseImage({
			count: sheng_count, // 默认9
			sizeType: ['compressed'], // 可以指定是原图还是压缩图，默认二者都有
			sourceType: ['album', 'camera'], // 可以指定来源是相册还是相机，默认二者都有
			success: function (res) {
				wx.showLoading({
					title: '正在上传...',
					mask: true
				})
				var tempFilePaths = res.tempFilePaths
				var j = 0
				function upload() {
					wx.uploadFile({
						url: _DuoguanData.Upload,
						filePath: tempFilePaths[j],
						name: 'file',
						formData: {
							filetype: 'img',
						},
						success: function (cb) {
							cb = JSON.parse(cb.data)
							that.setData({
								postimg: that.data.postimg.concat(cb.msg)
							})
							j++
							if (j < tempFilePaths.length) {
								upload()
							} else {
								wx.hideLoading()
							}
						},
						fail: function (res) {
							template.showtoast('上传失败', 'loading')
						}
					})
				}
				upload()
			}
		})
		// wx.chooseImage({
		// 	count: sheng_count,
		// 	sizeType: ['compressed'], // 可以指定是原图还是压缩图，默认二者都有
		// 	sourceType: ['album', 'camera'], // 可以指定来源是相册还是相机，默认二者都有
		// 	success: function (res) {
		// 		// 返回选定照片的本地文件路径列表，tempFilePath可以作为img标签的src属性显示图片
		// 		that.setData({
		// 			postimg: that.data.postimg.concat(res.tempFilePaths)
		// 		})
		// 	}
		// })
	},

	//发表评论
	formSubmit: function (e) {
		var that = this;
		var t_data = e.detail.value;
		that.setData({ buttonIsDisabled: true, submitIsLoading: true });
		var imgarray = ''
		for (let i = 0; i < that.data.postimg.length; i++) {
			imgarray += that.data.postimg[i] + ';'
		}
		requestUtil.post(_DuoguanData.postComment, { storeId: wx.getStorageSync('dish_id'), aid: wx.getStorageSync('aid'), oid: that.data.this_order_id, fval: that.data.this_score_val, fcon: e.detail.value.post_content, imgs: imgarray }, (info) => {
			if (info) {
				wx.showToast({
					title: '发表成功',
					icon: 'success'
				})
				setTimeout(function () {
					wx.navigateBack({
						delta: 1
					})
				}, 500)
			}
			that.setData({ oinfo: info, glo_is_load: false });
		}, that, {
				completeAfter: function () {
					that.setData({ buttonIsDisabled: false, submitIsLoading: false });
				}
			});
	},





	imgUploadTime: function () {
		var that = this
		var this_img_len = that.data.this_img_i
		var this_img_max_len = that.data.this_img_max
		if (this_img_len < this_img_max_len) {
			var requsetData = {};
			requsetData.pid = that.data.this_comment_id;
			requestUtil.upload(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanDish/OrderApi/imgUpload.html', that.data.postimg[this_img_len], 'file', requsetData, (info) => {
				that.setData({ this_img_i: that.data.this_img_i + 1 });
				that.imgUploadTime();
			}, this, {});
		} else {
			wx.hideToast();
			wx.showModal({
				title: '提示',
				content: '评价成功',
				showCancel: false,
				success: function (res) {
					wx.redirectTo({
						url: '../restaurant-order-list/index'
					});
				}
			});
		}
	},
	//商品点赞
	goods_zan_bind: function (e) {
		var goods_id = e.currentTarget.id;
	}
})