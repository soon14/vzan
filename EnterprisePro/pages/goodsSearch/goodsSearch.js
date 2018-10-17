
var addr = require("../../utils/addr.js");
var util = require("../../utils/util.js");
var http = require("../../utils/http.js");
const tools = require("../../utils/tools.js");

Page({
  /*
  **生命周期函数，监听页面的加载
  */

  onLoad: function (options) {
    var that = this;
    var app = getApp();
    app.GetStoreConfig(function (storeConfig) {
      if (storeConfig && ('searchKeyword') in storeConfig.funJoinModel) {
        that.setData({
          "hm.hotSearch": storeConfig.funJoinModel.searchKeyword
        });
      }
    });
    wx.getStorage({
      key: 'searchData',
      success: function (res) {
        if (res.errMsg =="getStorage:ok"){
          that.setData({
            "hm.searchData": res.data
          });
        }
        console.log(res)
      },
    })

  },

  data: {
    inputfocus: true,
    inputValue: "",
    StorageFlag: true,  //搜索记录标志位
    unitFlag: "",
    vm: {
      ispost: false,
      loadall: false,
      list: [],
      goodtype: 0,
      pageindex: 1,
      pagesize: 20,
    },
    hm: {
      hotSearch: [],
      searchData: []
    },

  },
  focusInput: function () {
    this.setData({
      inputfocus: true,
    });
  },
  blurInput: function () {
    this.setData({
      inputfocus: false,
    });
  },
  /*
  **获取用户的输入信息
  */
  inputSearch: function (e) {
    var that = this;
    var d = that.data;
    this.setData({
      inputValue: e.detail.value
    });
  },

  /*
 **清除用户的历史记录
 */
  clearHistory: function (e) {

    var that = this;
    wx.removeStorageSync('searchData');
    this.setData({
      "hm.searchData": [],
    });

    this.onLoad();

  },
  clickSearch: function () {
    this.setData({
      vm: {
        ispost: false,
        loadall: false,
        list: [],
        goodtype: 0,
        pageindex: 1,
        pagesize: 20
      }
    });
    this.goodsSearch();
  },
  clearInputValue:function(){
    this.setData({
      inputValue:"",
    });
  },
  /*
  **对用户的输入信息进行搜索
  */
  goodsSearch: function (e) {

    var that = this;
    var d = this.data;
    var vm = d.vm;
    if (d.inputValue != '') {

      //将搜索词存进本地缓存中
      var that = this;
      var searchData = wx.getStorageSync('searchData') || [];
      var index = searchData.findIndex(p => p == d.inputValue);
      if (index==-1){
        searchData.unshift(d.inputValue);
      }
      
      if (searchData.length > 10) {
        searchData = searchData.slice(0, 10);
      }
      wx.setStorageSync('searchData', searchData);
      that.setData({
        "hm.searchData": searchData
      });



      if (vm.ispost || vm.loadall)
        return;
      vm.ispost = true;
      http
        .postAsync(addr.Address.GetGoodsList,
        {
          aid: wx.getStorageSync("aid"),
          goodtype: 0,
          pageindex: vm.pageindex,
          pagesize: vm.pagesize,
          search: d.inputValue,
        })
        .then(function (res) {
          if (res.isok == 1) {
            if (res.postdata.goodslist.length < vm.pagesize) {
              vm.loadall = true;
            }
            else {
              vm.loadall = false;
              vm.pageindex += 1;
            }

            vm.list = vm.list.concat(res.postdata.goodslist);
            vm.ispost = false;
            that.setData({
              "vm": vm,
            });
          }
        });
      this.onLoad();
    }
  },

  historyTap: function (e) {
    var that = this;
    var d = this.data;
    var hisName = e.currentTarget.dataset.keyword;
    this.setData({
      vm: {
        ispost: false,
        loadall: false,
        list: [],
        pageindex: 1,
        pagesize: 20,
        goodtype: 0,
      },
      inputValue: hisName
    });

    this.goodsSearch();

  },
  onReachBottom: function () {

    this.goodsSearch();
  }
})

