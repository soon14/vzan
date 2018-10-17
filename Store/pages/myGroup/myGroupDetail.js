
var http = require("../../utils/http.js");
var addr = require("../../utils/addr.js");
var tools = require("../../utils/tools.js");
var util = require("../../utils/util.js");
var WxParse = require("../../utils/wxParse/wxParse.js");
var app = getApp();

Page({

	/**
	 * 页面的初始数据
	 */
	data: {
		tab: [
			{ name: "商品详情", sel: true },
			{ name: "拼团规则", sel: false },
		],
		//距离结束
		fromTheEnd: {
			dd: "00",
			hh: "00",
			mm: "00",
			ss: "00",
		}
	},

	/**
	 * 生命周期函数--监听页面加载
	 */
	onLoad: function (options) {
		var gsid = options.gsid || 0, that = this;
		if (gsid == 0) {
			wx.navigateBack({
				delta: 1
			})
			return;
		}


		this.data.gsid = gsid;
		tools.getUserInfo().then(function (user) {
			that.setData({
				"vm.uinfo": user
			});
			that.init();
		})

	},
	init: function () {
		var that = this, vm = this.data.vm;
		http.postAsync(addr.Address.GetMyGroupDetail, {
			appId: app.globalData.appid,
			userId: vm.uinfo.UserId,
			groupsponId: that.data.gsid
		}).then(function (data) {
			console.log(data);
			if (data.isok) {
				var _g = data.groupdetail
        //转换富文本
        _g.Description = WxParse.wxParse('Description', 'html', _g.Description, that, 5)

        if (_g.GroupUserList.length >= 4) {
          _g.GroupUserList = _g.GroupUserList.slice(0, 4);
          _g.NeedNum_fmt = 0;
        }
        else {
          if (_g.NeedNum + _g.GroupUserList.length <= 4) {
            _g.NeedNum_fmt = _g.NeedNum;
          }
          else {
            _g.NeedNum_fmt = 4 - _g.GroupUserList.length;
          }

        }

				that.setData({
					"vm.groupDetail": _g,
				});

				if (data.groupdetail.MState == 1) {
					tools.initEndClock(_g.ValidDateStart, _g.ValidDateEnd, that);
				}


			}
		});
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
	// 拨打电话
	makephonecall: function () {
		wx.makePhoneCall({
			phoneNumber: app.globalData.customerphone,
		})
	},
	// 返回主页
	siwchtoIndex: function () {
		wx.switchTab({
			url: '../index/index',
		})
	},
	/**
	 * 用户点击右上角分享
	 */
	onShareAppMessage: function (res) {
		console.log(res);
		var _g = res.target.dataset.group;
		var _user = this.data.vm.uinfo;
		var _path = '/pages/groupOrder/groupInvite?gsid=' + _g.GroupSponsorId + '&shareuid=' + _user.UserId;
		var _title = `￥${_g.DiscountPrice / 100}元就能购买${_g.GroupName},一起来拼团吧！`;
		console.log(_path);
		return {
			title: _title,
			path: _path,
			imageUrl: _g.ImgUrl,
			success: function (res) {
				// 转发成功
				tools.tips("转发成功");
			},
			fail: function (res) {

			}
		}
	},
	//点击：商品详情/拼团规则
	clickTab: function (e) {
		this.data.tab.forEach(function (o, i) {
			o.sel = false;
		});
		this.data.tab[e.currentTarget.dataset.index].sel = true;
		var key = "tab[" + e.currentTarget.dataset.index + "].sel";
		this.setData({
			tab: this.data.tab
		});

	},
})