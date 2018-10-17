// pages/mypayList/mypayList.js
var template = require('../../template/template.js');
var addr = require("../../utils/addr.js");
var app = getApp()
Page({

  /**
   * 页面的初始数据
   */
	data: {
	},

	onLoad: function (options) {
		template.getSaveMoneySetUserLogList(this)
	},


})