var template = require('../../../../script/template.js');
import {
  core,
  vm,
  http
} from "../../../../utils/core";

var queryModel = {
  utoken: '',
  aId: 0,
  storeId: 0,
  cateIdOne: 0,
  cateId: 0,
  kw: '',
  pageSize: 20,
  pageIndex: 1,
  storeOwner: 1,
};
var toast = {
  show: false,
  mask: false,
  msg: "",
}

Page({

  /**
   * 页面的初始数据
   */
  data: {
    storeId: 0,
    vm: JSON.parse(JSON.stringify(vm)),
    query: JSON.parse(JSON.stringify(queryModel)),
    categoryList: [],
    selectedTypeId: 0,
    showshareCard: false, //商品分享码弹窗
    pintuanInfo: {},

    $toast: JSON.parse(JSON.stringify(toast)),
  },
  open_document: function() {
    wx.showLoading({
      title: '页面加载中...',
    })
    wx.downloadFile({
      url: 'https://wtapi.vzan.com/dist/shuoming.doc',
      // url: 'http://j.vzan.cc/miniapp/doc/pindoc.docx',
      success: function(res) {
        var filePath = res.tempFilePath
        wx.hideLoading()
        wx.openDocument({
          filePath: filePath,
        })
      }
    })
  },
  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this;
    that.setData({
      "query.storeId": Number(options.storeid) || 0,
    });
  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function() {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function() {
    var that = this;
    template.checksetting(that)
    that.setData({
      "vm": JSON.parse(JSON.stringify(vm))
    });
    this.loadMore();
  },

  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function() {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function() {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function() {
    this.reload();
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {

  },
  inputkw: function(e) {
    this.setData({
      "query.kw": e.detail.value
    });
  },
  search: function() {
    this.data.vm = JSON.parse(JSON.stringify(vm));
    this.loadMore();
  },
  pickType: function(e) {
    this.setData({
      "selectedTypeId": e.currentTarget.dataset.type
    });
    this.data.vm = JSON.parse(JSON.stringify(vm));
    this.loadMore();
  },
  loadMore: function() {
    var that = this;
    var vm = that.data.vm;
    var q = that.data.query;

    if (vm.ispost || vm.loadall) return;
    if (vm.storeId == 0)
      return;
    that.setData({
      "vm.ispost": true
    })
    wx.showNavigationBarLoading();
    var app = getApp();
    app.login(function(g) {

      if (that.data.categoryList.length <= 0) {
        core.CategoryList({
          utoken: g.utoken,
          aId: g.aid
        }).then(function(result) {
          if (result && result.code == 1) {
            that.setData({
              "categoryList": result.obj
            });
          }
        });
      }

      q.utoken = g.utoken;
      q.aId = g.aid;
      q.pageSize = vm.pagesize;
      q.pageIndex = vm.pageindex;
      q.cateIdOne = that.data.selectedTypeId;

      core.GoodList(q).then(function(result) {
        if (result && result.code == 1) {
          let key = "vm.list[" + vm.pageindex + "]";
          if (result.obj.length >= vm.pagesize) {
            vm.pageindex += 1;
          } else {
            vm.loadall = true;
          }
          that.setData({
            [key]: result.obj,
            "vm.pageindex": vm.pageindex,
            "vm.loadall": vm.loadall
          });
        } else {
          wx.showModal({
            title: '提示',
            content: result.msg,
            showCancel: false,
            success: function(res) {
              if (res.confirm) {
                template.goback(1)
              }
            }
          })
        }
        that.setData({
          "vm.ispost": false
        })
        wx.hideNavigationBarLoading();
      });
    })
  },
  reload: function() {
    this.data.vm = JSON.parse(JSON.stringify(vm));
    this.loadMore();
    setTimeout(function() {
      wx.stopPullDownRefresh();
    }, 500);
  },
  updateGoods: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var postData = that.data.vm.list[ds.indexf][Number(ds.indexs)];
    postData.state = postData.state == 1 ? 0 : 1;
    var g = getApp().globalData;
    core.EditGoods(g.utoken, postData).then(function(result) {
      if (result && result.code == 1) {

        that.data.vm = JSON.parse(JSON.stringify(vm));
        that.loadMore();
        that.setData({
          "$toast": {
            show: true,
            mask: false,
            msg: result.msg
          }
        });
        setTimeout(function() {
          that.setData({
            '$toast.show': false
          });
        }, 1500);
      } else {
        wx.showModal({
          title: '提示',
          content: result.msg,
        })
      }
    });
  },
  deleteGoods: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var postData = that.data.vm.list[ds.indexf][Number(ds.indexs)];
    wx.showModal({
      title: '提示',
      content: '确定删除吗？',
      success: function(res) {
        if (res.confirm) {
          postData.state = -1;
          var g = getApp().globalData;
          core.EditGoods(g.utoken, postData).then(function(result) {
            if (result && result.code == 1) {
              wx.showToast({
                title: '删除成功',
              })
              that.data.vm = JSON.parse(JSON.stringify(vm));
              that.loadMore();
            } else {
              wx.showModal({
                title: '提示',
                content: result.msg,
              })
            }
          });
        } else if (res.cancel) {

        }
      }
    })

  },
  editGoods: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var postData = that.data.vm.list[ds.indexf][Number(ds.indexs)];
    wx.navigateTo({
      url: 'product_edit?id=' + postData.id + "&storeid=" + postData.storeId,
    })
  },
  getCode: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var id = ds.id;
    var indexf = Number(ds.indexf);
    var indexs = Number(ds.indexs);
    var g = getApp().globalData;
    var q = {
      utoken: g.utoken,
      aid: g.aid,
      storeId: that.data.query.storeId,
      type: 1,
      scene: id,
    };
    that.setData({
      "pintuanInfo": that.data.vm.list[indexf][indexs],
    });
    core.GetQrCode(q).then(function(result) {
      console.log(result);
      if (result.code == 1) {
        that.setData({
          "showshareCard": true,
        });

        that.draw_goodshareCard(result.obj.url)
      } else {
        wx.showModal({
          title: '提示',
          content: result.msg + ",小程序发布成功后才可以生成二维码",
        })
      }
    });
  },
  hideshare_card: function() {
    var that = this;
    that.setData({
      showshareCard: false,
    });
  },
  draw_goodshareCard: function(_url) { //画布
    var that = this
    var _g = that.data.pintuanInfo
    wx.downloadFile({
      url: _url.replace(/^http:/, "https:"),
      success: function(res) {
        var storeQrCode = res.tempFilePath
        var grouprice = Number(_g.groupPrice / 100)

        const ctx = wx.createCanvasContext('gshare_Card')
        const wW = wx.getSystemInfoSync().windowWidth * 0.74
        const wH = wx.getSystemInfoSync().windowHeight * 0.5
        const phoneType = wx.getSystemInfoSync().model


        ctx.setFillStyle('#fff')
        ctx.fillRect(1, 0, wW, wH)
        drawText(_g.name, wW * 0.5, wH * 0.02, wW * 0.8); // 商品名字
        ctx.drawImage(storeQrCode, wW * 0.18, (phoneType == 'iPhone X' ? wH * 0.24 : wH * 0.19), wW * 0.64, wW * 0.58) //二维码
        //单买价
        ctx.setFontSize(14)
        ctx.textAlign = 'center'
        ctx.setFillStyle('#666666')
        ctx.fillText('单买价', wW * 0.25, wH * 0.82)
        ctx.setFillStyle('#666666')
        ctx.fillText('拼团立返', wW * 0.75, wH * 0.82)
        //价格
        ctx.textAlign = 'center'
        ctx.font = "bold 13px Arial"
        ctx.setFontSize(16)
        ctx.setFillStyle('#000')
        ctx.fillText('￥' + _g.priceStr, wW * 0.25, wH * 0.88)
        ctx.setFontSize(16)
        ctx.setFillStyle('#000')
        ctx.fillText('￥' + grouprice, wW * 0.75, wH * 0.88)

        ctx.draw()
        that.setData({
          showshareCard: true
        })
        //文字转行
        function drawText(t, x, y, w) {
          var chr = t.split("");
          var temp = "";
          var row = [];
          ctx.textAlign = 'center';
          ctx.setFontSize(14);
          ctx.setFillStyle('#333333');
          for (var a = 0; a < chr.length; a++) {
            if (ctx.measureText(temp).width < w) {;
            } else {
              row.push(temp);
              temp = "";
            }
            temp += chr[a];
          }
          row.push(temp);
          if (row.length == 1) {
            ctx.fillText(row[0], wW * 0.5, wH * 0.02 + 16);
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
        }
      },
      fail: function(e) {
        console.log(e);
      }
    })
  },
  save_goodImg: function() {
    const phonePixel = wx.getSystemInfoSync().pixelRatio
    const wW = wx.getSystemInfoSync().windowWidth * phonePixel * 0.74
    const wH = wx.getSystemInfoSync().windowHeight * phonePixel * 0.5
    template.canvasToTempFilePath(0, 'gshare_Card', wW, wH, this)
  },
  addnew: function() {
    var that = this;
    wx.navigateTo({
      url: "product_edit?id=0&storeid=" + that.data.query.storeId
    });
  },
})