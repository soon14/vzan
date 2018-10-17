var _jsonsort = require('../../../utils/common');
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
var app = getApp();
Page({
	data: {
		this_options: {},
		this_dish_id: 0,
		this_dish_info: '',
		this_dish_type: '',
		glo_is_load: false,
		this_is_card_open: false,
		this_is_user_card: 0,
		this_user_card_info: null,
		this_open_mini: 0,
		cPagelength: 0,//当前页面栈数

		shareArray: [
			{ content: '保存图片', font: 'icon-Choice_xuanze', bgcolor: '#FF6B6B' },
			{ content: '发朋友圈', font: 'icon-Circleoffriends_pengyou', bgcolor: "#FFD040" },
			{ content: '分享给朋友', font: 'icon-Forward_fenxiang', bgcolor: "#FF9E40" },
		],
		showShareCard: false,
		shareCardSetting: {},
	},
	getuserInfo: function (e) {
		var that = this
		var _e = e.detail
		if (_e.encryptedData && _e.iv && _e.signature) {
			wx.login({
				success: function (res) {
					app.loginByThirdPlatform(res.code, _e.encryptedData, _e.signature, _e.iv, function (cb) {
						if (cb) {
							that.setData({
								userInfo: wx.getStorageSync('userInfo')
							})
						}
					}, 0)
				}
			})
		} else {
			wx.showModal({
				title: '提示',
				content: '尊贵的客户，获取会员卡需先授权头像和微信名称哦',
				showCancel: false,
				confirmText: '我知道了'
			})
		}
	},
	share_card: function (e) {
		let that = this
		let data = (Object.keys(that.data.shareCardSetting).length == 0 ? e.currentTarget.dataset : that.data.shareCardSetting)
		const ctx = wx.createCanvasContext('shareCard')
		const wW = wx.getSystemInfoSync().windowWidth
		const wH = wx.getSystemInfoSync().windowHeight
		wx.showLoading({ title: '图片生成中...' })
		downImage(data.logo, data.code, function (cb) {
			if (cb) {
				// 灰色背景
				ctx.setFillStyle('#fff')
				ctx.fillRect(0, 0, wW, wH)
				ctx.setFillStyle('#444444')
				ctx.fillRect(0, 0, wW, wH * 0.22)
				// 店铺logo
				ctx.drawImage(data.logoPath, wW * 0.04, wH * 0.04, wH * 0.14, wH * 0.14)
				// 店铺名字
				drawText(data.name, wW * 0.3, wH * 0.02, wW * 0.44);
				// 店铺分数
				for (let p = 0; p < 5; p++) {
					if (p < Number(data.score)) {
						drawStar(wW * 0.01, wW * 0.02, wW * 0.32 + (p * 18), wH * 0.12, true);
					} else {
						drawStar(wW * 0.01, wW * 0.02, wW * 0.32 + (p * 18), wH * 0.12, false);
					}
				}
				// 人均消费
				ctx.setFontSize(13);
				ctx.setFillStyle('white');
				ctx.fillText('人均:￥' + data.price, wW * 0.61, wH * 0.13);
				// 店铺标签
				drawLabels(data.labels, wW * 0.3, wH * 0.14, wH * 0.28)
				// 二维码
				ctx.drawImage(data.codePath, wW * 0.12, wH * 0.22, wH * 0.37, wH * 0.42)
				ctx.draw();

				that.data.shareCardSetting = data
				that.setData({ showShareCard: true })
				wx.hideLoading()
			}
		})
		//文字转行
		function drawText(t, x, y, w) {
			var chr = t.split("");
			var temp = "";
			var row = [];
			ctx.setFontSize(16);
			ctx.setFillStyle('white');
			for (var a = 0; a < chr.length; a++) {
				if (ctx.measureText(temp).width < w) {
					;
				}
				else {
					row.push(temp);
					temp = "";
				}
				temp += chr[a];
			}
			row.push(temp);
			if (row.length == 1) {
				ctx.fillText(row[0], wW * 0.3, wH * 0.08);
			} else {
				for (var b = 0; b < row.length; b++) {
					ctx.fillText(row[b], x, y + (b + 1) * 20);
				}
			}
		};
		// 画星星
		function drawStar(r, R, x, y, fill) {
			ctx.beginPath();
			for (let i = 0; i < 5; i++) {
				ctx.lineTo(Math.cos((18 + i * 72) / 180 * Math.PI) * R + x, -Math.sin((18 + i * 72) / 180 * Math.PI) * R + y);
				ctx.lineTo(Math.cos((52 + i * 72) / 180 * Math.PI) * r + x, -Math.sin((52 + i * 72) / 180 * Math.PI) * r + y);
			}
			ctx.strokeStyle = fill ? "#fea512" : "#fff";
			ctx.fillStyle = fill ? "#fea512" : "#fff";
			ctx.closePath();
			ctx.fill();
			ctx.stroke();
		};
		function drawLabels(labelsArray, x, y, w) {
			let _l = labelsArray;
			var _s = ''
			for (let i = 0; i < _l.length; i++) {
				_s += '"' + _l[i] + '"  '
			}
			var chr = _s.split("");
			var temp = "";
			var row = [];
			ctx.setFontSize(13);
			ctx.setFillStyle('#fea512');
			for (var a = 0; a < chr.length; a++) {
				if (ctx.measureText(temp).width < w) {
					;
				}
				else {
					row.push(temp);
					temp = "";
				}
				temp += chr[a];
			}
			row.push(temp);
			if (row.length == 1) {
				ctx.fillText(row[0], x, y + 16);
			} else if (row.length == 2) {
				for (var b = 0; b < row.length; b++) {
					ctx.fillText(row[b], x, y + (b + 1) * 16);
				}
			} else {
				row[1] = row[1].replace(row[1].substring(row[1].length - 2, row[1].length), '....')
				for (var b = 0; b < 2; b++) {
					ctx.fillText(row[b], x, y + (b + 1) * 16);
				}
			}
		};
		function downImage(logo, code, cb) {
			if (Object.keys(that.data.shareCardSetting).length != 0) {
				cb('isok')
			} else {
				wx.downloadFile({
					url: logo.replace(/http/, "https"), //下载店铺logo
					success: (res) => {
						data.logoPath = res.tempFilePath

						wx.downloadFile({
							url: code.replace(/http/, "https"), //下载店铺二维码
							success: (res_1) => {
								data.codePath = res_1.tempFilePath
								cb('isok')
							}
						})
					}
				})
			}
		}
	},
	unshowShareCard: function () {
		this.setData({ showShareCard: false })
	},
	doSomething: function (e) { //0保存图片 1发朋友圈 2分享给朋友
		var id = e.currentTarget.id
		var wW = wx.getSystemInfoSync().windowWidth
		var wH = wx.getSystemInfoSync().windowHeight
		if (id != 2) {
			wx.canvasToTempFilePath({
				x: 0,
				y: 0,
				width: wW * 0.84,
				height: wH * 0.62,
				destWidth: wW * 0.84,
				destHeight: wH * 0.62,
				canvasId: 'shareCard',
				success: function (res) {
					wx.getSetting({
						success: function (cb) {
							console.log(res)
							if (cb.authSetting["scope.writePhotosAlbum"] == undefined) {
								saveImage(res.tempFilePath, id)
							} else {
								if (!cb.authSetting["scope.writePhotosAlbum"]) {
									wx.showModal({
										title: '提示',
										content: '您拒绝过授权访问相册，请进入下一步设置启动"保存到相册"。',
										showCancel: false,
										confirmText: '下一步',
										success() {
											wx.openSetting({})
										}
									})
								} else {
									saveImage(res.tempFilePath, id)
								}
							}
						}
					})
				}
			})
		};
		function saveImage(filepath, id) {
			wx.saveImageToPhotosAlbum({
				filePath: filepath,
				success(res) {
					if (id == 0) {
						wx.showToast({
							title: '图片保存成功',
						})
					}
					if (id == 1) {
						wx.showModal({
							title: '提示',
							content: '保存已保存成功！您可以用该图片去分享朋友圈哦',
							showCancel: false
						})
					}
				}
			})
		}
	},
	webview_formsubmit: function (e) {
		var that = this;
		wx.navigateTo({
			url: '../webview_jianjie/index?dish_id=' + that.data.this_dish_id
		});
	},
	openmini_formsubmit: function (e) {
		let dinfo = this.data.this_dish_info;
		wx.navigateToMiniProgram({
			appId: dinfo.dish_tomini_appid,
			path: dinfo.dish_tomini_appurl,
			success(res) {

			},
			fail: function (res) {
				console.log(res);
			}
		})
	},
	diancan_formsubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		that.insertFormID(form_id);

		if (that.data.this_dish_info.dish_info.dish_is_diannei == 1) {
			if (that.data.this_dish_info.dish_info.dish_is_rcode_open == 1) {
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
					url: '../restaurant-single/index?dish_id=' + that.data.this_dish_id + '&order_type=1&is_ziqu=0'
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
	//自提
	ziqu_formsubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		that.insertFormID(form_id);

		if (that.data.this_dish_info.dish_info.dish_is_ziqu == 1) {
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
	go_dish_index_bind: function (e) {

		wx.switchTab({
			url: e.currentTarget.dataset.url,
			fail: function () {
				wx.navigateTo({
					url: e.currentTarget.dataset.url,
				})
			}
		})
	},
	//预订
	yuding_formsubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		that.insertFormID(form_id);
		if (that.data.this_dish_info.dish_info.dish_is_yuding == 1) {
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
		if (that.data.this_dish_info.dish_info.dish_is_paidui == 1) {
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
		var _u = wx.getStorageSync('userInfo')
		requestUtil.post(_DuoguanData.commitFormId, { formid: form_id, appid: getApp().globalData.appid, openid: _u.openId || _u.OpenId }, (info) => {

		}, that, { isShowLoading: false });
	},
	//外卖
	waimai_formsubmit: function (e) {
		var that = this;
		var form_id = e.detail.formId;
		that.insertFormID(form_id);
		if (that.data.this_dish_info.dish_info.dish_is_waimai == 1) {
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
	go_user_card_info: function (e) {
		wx.navigateTo({
			url: "../restaurant-card/index?dish_id=" + this.data.this_dish_id + '&savemoney=' + e.currentTarget.id
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
		var loc_lat = that.data.this_dish_info.dish_info.dish_gps_lat;
		var loc_lng = that.data.this_dish_info.dish_info.dish_gps_lng;
		wx.openLocation({
			latitude: parseFloat(loc_lat),
			longitude: parseFloat(loc_lng),
			scale: 18,
			name: that.data.this_dish_info.dish_info.dish_name,
			address: that.data.this_dish_info.dish_info.dish_address
		});
	},
	//电话
	call_phone_bind: function () {
		var that = this;
		wx.makePhoneCall({
			phoneNumber: that.data.this_dish_info.dish_info.dish_con_mobile
		});
	},
	onLoad: function (options) {
		var that = this;
		console.log(options)
		wx.setStorageSync('dish_id', options.dish_id)
		that.setData({
			cPagelength: getCurrentPages().length,
			this_options: options,
			this_dish_id: options.dish_id,
			userInfo: wx.getStorageSync('userInfo')
		});
	},
	onShow: function () {
		wx.hideToast();
		this.loadSingleDishData();
	},
	loadSingleDishData: function () {
		var that = this;
		requestUtil.post(_DuoguanData.getDishInfo, { dish_id: that.data.this_dish_id }, (info) => {
			wx.setStorageSync("dish_ischeck_mobile", info.dish_is_sms_check || 0);

			if (info.dish_info.dish_fuwu) {
				info.dish_info.dish_fuwu = info.dish_info.dish_fuwu.replace(/[，]/g, ',')
				info.dish_info.dish_fuwuList = info.dish_info.dish_fuwu.split(',')
			}
			info.dish_comment_fenshu = Math.round(info.dish_comment_fenshu)
			info.dish_comment_fenshu = Math.ceil(info.dish_comment_fenshu)

			info.dish_info.dish_yingye_time_text_diannei = ''
			for (var i = 0; i < info.dish_info.open_time.length; i++) {
				info.dish_info.dish_yingye_time_text_diannei += (info.dish_info.open_time[i].dish_open_btime + '-' + info.dish_info.open_time[i].dish_open_etime + ';')
			}
			info.dish_info.dish_yingye_time_text_waimai = ''
			for (var z = 0; z < info.dish_info.wm_time.length; z++) {
				info.dish_info.dish_yingye_time_text_waimai += (info.dish_info.wm_time[z].dish_open_wm_btime + '-' + info.dish_info.wm_time[z].dish_open_wm_etime + ';')
			}

			that.setData({ this_dish_info: info, glo_is_load: false, this_open_mini: 0 });
			wx.setNavigationBarTitle({ title: info.dish_info.dish_name });
			wx.hideToast();
			if (info.dish_info.dish_is_open_tomini == 1) {
				that.setData({ this_open_mini: 1 });
				return false;
			}
			//验证用户是否已领取会员卡
			if (info.dish_info.card_open_status == 1) {
				that.setData({ this_is_card_open: true });
				that.check_user_is_card();
			} else {
				that.setData({ this_is_card_open: false });
			}

		}, that, { isShowLoading: false });
	},
	check_user_is_card: function () {
		var that = this;
		var requestData = {};
		requestData.module_name = 'DuoguanDish';
		requestData.shop_id = that.data.this_dish_id;
		requestUtil.post(_DuoguanData.userIsCard, requestData, (info) => {
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
		requestUtil.post(_DuoguanData.getCardInfo, requestData, (info) => {
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
	//图片放大
	img_max_bind: function (e) {
		var that = this;
		wx.previewImage({ current: e.target.dataset.url, urls: that.data.this_dish_info.dish_info.dish_shijing });
	},
	img_max_bind_zz: function (e) {
		var that = this;
		wx.previewImage({ current: e.target.dataset.url, urls: that.data.this_dish_info.dish_info.dish_zizhi });
	},
	//下拉刷新
	onPullDownRefresh: function () {
		var that = this;
		that.loadSingleDishData();
		setTimeout(() => {
			wx.stopPullDownRefresh()
		}, 1000);
	},
	onShareAppMessage: function () {
		var that = this;
		that.setData({ showShareCard: false })
		var shareTitle = that.data.this_dish_info.dish_info.dish_name;
		var shareDesc = that.data.this_dish_info.dish_info.dish_jieshao;
		var sharePath = 'pages/restaurant/restaurant-home-info/index?d_type=single&dish_id=' + that.data.this_dish_id;
		return {
			title: shareTitle,
			desc: shareDesc,
			path: sharePath,
			success: function (res) {
				that.share_card()
			}
		}
	},
})