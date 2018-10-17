var WxParse = require('../../../../script/wxParse/wxParse.js');
import {
  core,
  vm,
  http
} from "../../../../utils/core";


Page({

  /**
   * 页面的初始数据
   */
  data: {
    goodTypes: [],
    goodLabels: [],
    goodAttrList: [],
    goodUnits: [],
    goodExtTypes: [],
    batchStock: "", //批量设置时输入的库存
    batchPrice: "", //批量设置时输入的价格
    batchGroupPrice: "", //批量设置时输入的返现
    scrollToId: "",
    tabSelectedIndex: 0,
    ptypeSelectedId: 0,
    description: "",
    descriptionNodes: {},
    goodTypeModel: {},
    attrModel: {},
    editAttrModel: {},
    edit_type_list: 0, //当前选择的是大类还是小类的tab
    edit_attr_list: 0, //当前选择的是规格还会规格值的tab
    $toast: {
      show: false,
      msg: "",
      inMess: ""
    },
    tab: [{
      name: "分类"
    }, {
      name: "详情"
    }, {
      name: "规格"
      //{ name: "标签" },
      //{ name: "参数" }
    }],

    vm: JSON.parse(JSON.stringify(vm)),
    goodTypesIndex: -1,
    p: {},
    pickspecification: [],
    keyword: "",
    id: 0,
    ispost: false,
    saving: false,
    show_attr: false, //选择规格弹窗
    show_batch: false, //批量设置弹窗
    show_edit_typelist: false, //编辑分类-分类列表
    show_edit_attrlist: false, //编辑规格-规格列表
    show_addtype: false, //添加大类弹窗
    show_addattr: false,
    attrTable_fmt: [],

    //计算属性
    firstLevel: [],
    firstLevelId: 0,
    //bigType: [],
    firstAttrId: 0,
    attrs: [],
    //firstAttr: [],
    attr_pick_nums: 0,
    pickspecification_fmt: [],
    canSaveType: false,
    canSaveAttr: false,
    canSetPrice: true,

  },


  inputAttrPrice: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var index = Number(ds.index);
    var key = "attrTable_fmt[" + index + "].price";
    that.setData({
      [key]: e.detail.value
    });
  },
  inputAttrGroupPrice: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var index = Number(ds.index);
    var key = "attrTable_fmt[" + index + "].groupPrice";
    that.setData({
      [key]: e.detail.value
    });
  },
  inputAttrStock: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var index = Number(ds.index);
    var key = "attrTable_fmt[" + index + "].stock";
    that.setData({
      [key]: e.detail.value
    });
  },
  bindGoodTypePickerChange: function(e) {
    var that = this;
    that.setData({
      "goodTypeModel.fId": that.data.firstLevel[Number(e.detail.value)].id,
      "goodTypeModel.selindex": Number(e.detail.value)
    });
  },
  bindGoodAttrPickerChange: function(e) {
    var that = this;
    that.setData({
      "attrModel.fId": that.firstAttr()[Number(e.detail.value)].id,
      "attrModel.selindex": Number(e.detail.value),
    });
  },
  firstAttr: function() {
    return this.data.goodAttrList.filter(t => t.fId == 0);
  },

  //点击 大类，小类
  change_edit_type_list: function(e) {

    this.setData({
      "edit_type_list": Number(e.currentTarget.dataset.type)
    });
  },
  change_edit_attr_list: function(e) {
    var tabindex = Number(e.currentTarget.dataset.tabindex);
    var that = this;
    that.setData({
      "edit_attr_list": tabindex,
    });
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function(options) {
    var that = this;
    that.setData({
      "id": options.id || 0,
      "storeid": options.storeid || 0,
    });




    //初始化
    // that.setData({
    //   "ispost": true,
    //   "goodTypes": [],
    //   "p": {},
    //   "pickspecification": [],
    // })

    if (that.data.id == 0) {
      wx.setNavigationBarTitle({
        title: "添加产品"
      });
    } else {
      wx.setNavigationBarTitle({
        title: "修改产品"
      });
    }
    var app = getApp();
    app.login(function(g) {

      that.loadTypes(g, function() {
        that.loadData(g);
      });


    });
  },
  loadTypes: function(g, callback) {
    var that = this;
    wx.showLoading({
      title: "加载中"
    });
    //初始化数据
    //产品分类
    var req_typsall = core.GetGoodsTypesAll({
      utoken: g.utoken,
      aid: g.aid,
      storeid: that.data.storeid
    }).then(function(alltypes) {
      //分类
      if (alltypes && alltypes.code == 1) {
        var types = alltypes.obj;
        //重建分类
        var level1 = types.goodTypes.filter(function(p) {
          return p.fId == 0;
        });
        level1.forEach(function(p) {
          p.children = types.goodTypes.filter(function(item) {
            return item.fId == p.id;
          });
        });
        var ptypeSelectedId = 0;
        var firstLevelId = 0;
        if (level1.length > 0) {
          ptypeSelectedId = level1[0].id;
          firstLevelId = level1[0].id;
        }

        var firstAttrId = 0;
        if (types.goodAttrList.length > 0) {

          types.goodAttrList.forEach(p => {
            if (p.fId == 0) {
              p.children = types.goodAttrList.filter(child => {
                return child.fId == p.id;
              });
            }
          });

          types.goodAttrList.forEach(function(p) {
            if (p.children) {
              p.children.forEach(function(pp) {
                pp.sel = false;
              });
            }
          });
          var firstLevelGoodAttr = types.goodAttrList.filter(t => t.fId == 0);
          if (firstLevelGoodAttr.length > 0) {
            firstAttrId = firstLevelGoodAttr[0].id;
          }
        }

        that.setData({
          "ptypeSelectedId": ptypeSelectedId,
          "goodTypes": types.goodTypes,
          "firstLevel": level1,
          "firstLevelId": firstLevelId,
          "firstAttrId": firstAttrId,
          "goodLabels": types.goodLabels,
          "goodAttrList": types.goodAttrList,
          "goodUnits": types.goodUnits,
          "goodTypeModel": types.goodTypeModel,
          "editAttrModel": types.attrModel,
          "attrModel": types.attrModel
        });
      }
      //如果是创建产品
      if (that.data.id == 0) {
        that.setData({
          "p": types.goodModel,
        });
      }


      if (callback)
        callback();

      wx.hideLoading();
    });
  },
  loadData: function(g) {
    var that = this;
    var d = that.data;

    if (that.data.id > 0) {
      //产品详情
      var req_goodinfo = core.GoodInfo({
        utoken: g.utoken,
        id: that.data.id
      }).then(function(result) {
        let good = result;
        if (good && good.code == 1 && typeof good.obj == "object") {
          //如果是修改产品，默认进 详情
          //that.tabSelectedIndex = 1;
          var temp_good = good.obj.goodInfo;
          //初始化分类
          //this.initTypes();
          //初始化规格
          var temp_attr = temp_good.pickspecification;
          if (temp_attr) {
            temp_attr = JSON.parse(temp_attr);
            that.data.pickspecification = temp_attr;
            temp_good.pickspecification = temp_attr;
            temp_good.specificationdetail = JSON.parse(temp_good.specificationdetail || '[]');
            that.initGoodsAttr(that.data.goodAttrList, temp_good);
          }

          //初始化分类
          that.data.goodTypes.forEach(tt => {
            tt.sel = false;
          });
          that.data.goodTypes.forEach(tt => {
            if (tt.id == temp_good.cateIdOne) {
              tt.sel = true;
            }
            if (tt.id == temp_good.cateId) {
              tt.sel = true;
            }
          });
          temp_good.originalPrice = temp_good.originalPrice / 100;
          temp_good.price = temp_good.price / 100;
          temp_good.groupPrice = temp_good.groupPrice / 100;

          var temp_description = temp_good.description;
          var nodes = WxParse.wxParse(
            "descriptionNodes",
            "html",
            temp_description || "",
            that,
            5
          );
          that.setData({
            "description": temp_description,
            "descriptionNodes": nodes
          });

          that.setData({
            "tabSelectedIndex": 1,
            "pickspecification_fmt": that.pickspecification_fmt(temp_attr),
            "p": temp_good,
            "ptypeSelectedId": temp_good.cateIdOne,
            "goodTypes": that.data.goodTypes,
          });
          that.buildTable();
        }

      });


    } else {
      that.setData({
        "tabSelectedIndex": 0
      });
    }

    //初始化产品分类
    //await this.loadTypes();
  },
  pickspecification_fmt: function(pickspecification) {
    if (pickspecification && pickspecification.length > 0) {
      pickspecification.forEach(p => {
        if (p.items.length > 0) {
          p.childrenNames = p.items
            .map(p => {
              return p.name;
            })
            .join("，");
        }
      });
    }
    return pickspecification;
  },
  initGoodsAttr: function(attrList, goods) {
    attrList.forEach(a => {
      goods.pickspecification.forEach(p => {
        if (p.items) {
          p.items.forEach(pa => {
            if (a.id == pa.id) {
              a.sel = true;
            }
          });
        }
      });
    });
    this.setData({
      "goodAttrList": attrList
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

    var temp_description = core.getStorageSync("temp_p_description_" + that.data.id);
    if (temp_description != "" && temp_description != "error") {
      var nodes = WxParse.wxParse(
        "descriptionNodes",
        "html",
        temp_description || "",
        that,
        5
      );
      that.setData({
        "description": temp_description,
        "descriptionNodes": nodes
      });
    }
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
    var that = this;
    wx.removeStorageSync("temp_p_description_" + that.data.id)
  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function() {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function() {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function() {

  },
  changeTab: function(e) {
    this.setData({
      "tabSelectedIndex": Number(e.currentTarget.dataset.index)
    });
  },


  buildTable: function() {
    var _t = [];
    var that = this;
    this.data.pickspecification.forEach(function(o) {
      if (o.items.length > 0) {
        _t.push(o.items);
      }
    });
    var _result = [];
    if (_t.length > 1) {
      _result = this.discarts.apply(this, _t);
    } else {
      _result = _t[0];
    }
    //创建用于显示的对象
    var table_detail = [];
    if (this.data.pickspecification.length > 1) {
      _result.forEach(function(p) {
        table_detail.push({
          id: p.map(function(i) {
            return i.id;
          }).join("_"),
          name: p.map(function(i) {
            return i.name;
          }).join("-"),
          //originalPrice: 0,
          price: 0,
          groupPrice: 0,
          stock: 0,
          imgUrl: ""
        });
      });
    } else if (this.data.pickspecification.length == 1) {
      _result.forEach(function(p) {
        table_detail.push({
          id: p.id,
          name: p.name, //that.pickspecification[0].name+"-"+
          originalPrice: 0,
          price: 0,
          groupPrice: 0,
          stock: 0,
          imgUrl: ""
        });
      });
    }
    that.data.attrTable_fmt = table_detail;
    //将规格值显示出来
    if (
      that.data.p &&
      that.data.p.specificationdetail instanceof Array &&
      that.data.attrTable_fmt &&
      that.data.attrTable_fmt.length > 0
    ) {
      that.data.p.specificationdetail.forEach(a => {
        that.data.attrTable_fmt.forEach(aa => {
          if (a.id.toString() === aa.id.toString()) {
            aa.price = a.price / 100;
            aa.groupPrice = a.groupPrice / 100;
            aa.stock = a.stock;
            aa.imgUrl = a.imgUrl;
          }
        });
      });
    }

    that.setData({
      "attrTable_fmt": table_detail,
    });
    return _result;
  },
  discarts: function() {
    //笛卡尔积
    var twodDscartes = function twodDscartes(a, b) {
      var ret = [];
      for (var i = 0; i < a.length; i++) {
        for (var j = 0; j < b.length; j++) {
          ret.push(ft(a[i], b[j]));
        }
      }
      return ret;
    };
    var ft = function ft(a, b) {
      if (!(a instanceof Array)) a = [a];
      var ret = a.slice(0);
      ret.push(b);
      return ret;
    };
    //多个一起做笛卡尔积
    return function(data) {
      var len = data.length;
      if (len == 0) return [];
      else {
        var r = data[0];
        for (var i = 1; i < len; i++) {
          r = twodDscartes(r, data[i]);
        }
        return r;
      }
    }(arguments.length > 1 ? arguments : arguments[0]);
  },

  //方法
  //添加规格
  add_attr: function(e) {
    var ds = e.currentTarget.dataset;
    var id = Number(ds.id);
    var name = ds.name;
    var fid = Number(ds.fid);

    var that = this;
    that.setData({
      "show_addattr": true,
      "attrModel.id": id,
      "attrModel.name": name,
      "attrModel.fId": fid,
      "attrModel.selindex": that.firstAttr().findIndex(function(t) {
        return t.id == fid;
      }) || 0,
    });
  },


  //编辑|隐藏 分类
  edit_typelist: function() {
    var that = this;
    this.setData({
      "show_edit_typelist": !that.data.show_edit_typelist
    })
  },

  //编辑|隐藏 规格
  eidt_attrlist: function eidt_attrlist() {
    var that = this;
    this.setData({
      "show_edit_attrlist": !that.data.show_edit_attrlist
    })
  },
  editContent: function editContent(e) {
    var ds = e.currentTarget.dataset;
    var id = ds.id;
    wx.navigateTo({
      url: "richtxt_edit?id=" + id
    });
  },
  input_attr_name(e) {

    var val = core.trim(e.detail.value);
    this.setData({
      "attrModel.name": val,
      "canSaveAttr": val.length > 0.
    });
  },
  hide_addattr: function() {
    this.initAttrModel();
  },
  initAttrModel: function() {
    var that = this;
    that.setData({
      "show_addattr": false,
      "attrModel.id": 0,
      "attrModel.name": "",
      "attrModel.fId": 0,
    });
  },
  add_good_attr_ok() {

    var that = this;
    wx.showLoading({
      title: "提交中...",
      mask: true
    });
    var g = getApp().globalData;
    var model = JSON.parse(JSON.stringify(that.data.attrModel));

    core.GoodAttr(g.utoken, "", model).then(function(result) {
      wx.hideLoading();
      if (result.code == 1) {

        that.loadTypes(g);
        that.initAttrModel();

        var msg = model.id > 0 ? "修改成功" : "添加成功";
        wx.showToast({
          title: msg,
          icon: "success",
          duration: 2000
        });
      } else {
        core.alert(result.msg);
      }
    });
  },
  //删除商品图片
  del_p_img: function() {
    var that = this;
    that.setData({
      "p": that.data.p
    });
  },
  choose_p_img: function() {
    var that = this;
    core.upload("img").then(function(urlArray) {
      that.data.p.img = urlArray[0];
      that.setData({
        "p": that.data.p
      });
    });
  },
  del_p_slideimg: function(index) {
    var that = this;
    that.data.p.imgAlbumList.splice(index, 1);
    that.setData({
      "p.imgAlbumList": that.data.p.imgAlbumList
    });
  },
  choose_slideimg: function() {
    var that = this;
    var maxchoose = 9 - that.data.p.imgAlbumList.length;
    if (that.data.p.imgAlbumList.length >= 9) {
      core.alert("最多只能上传9张图片，如需上传其他图片，请删除后再添加！");
      return;
    }
    core.upload("img", maxchoose).then(function(urlArray) {
      that.data.p.imgAlbumList = that.data.p.imgAlbumList.concat(urlArray);
      that.setData({
        "p.imgAlbumList": that.data.p.imgAlbumList
      });
    });
  },
  scrollToType: function(e) {
    var that = this;
    var id = e.currentTarget.dataset.id;
    this.data.goodTypes.forEach(t => {
      t.sel = false;
    });
    this.setData({
      goodTypes: that.data.goodTypes,
      scrollToId: "ptype-" + id,
      ptypeSelectedId: id
    });
  },
  chooseType: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var id = Number(ds.id);
    var sel = Boolean(ds.sel);
    var fid = ds.fid;
    if (fid !== that.data.ptypeSelectedId) {
      wx.showModal({
        title: '提示',
        content: '请选择对应大类下的小类',
      })
      return;
    }
    this.data.goodTypes.forEach(p => {
      p.sel = false;
    });
    this.data.goodTypes.find(function(t) {
      return t.id == id;
    }).sel = !sel;
    this.setData({
      "goodTypes": that.data.goodTypes
    });
  },
  choose_type_ok: function() {
    var that = this;
    if (this.data.goodTypes.filter(function(t) {
        return t.sel;
      }).length < 1) {
      wx.showModal({
        title: '提示',
        content: '请选择产品分类',
      })
      return;
    }
    that.setData({
      "tabSelectedIndex": 1,
    });
  },

  //填写产品名称
  input_p_name: function(event) {
    var that = this;
    that.setData({
      "p.name": event.detail.value,
    });
  },

  //是否限制库存
  click_p_stockLimit: function(e) {
    var that = this;
    if (that.data.p.stockLimit) {
      wx.showModal({
        title: "提示",
        content: "不限制库存将清空现在设置的库存数!",
        success: function success(res) {
          if (res.confirm) {
            that.data.p.stock = 0;
            that.data.p.stockLimit = !that.data.p.stockLimit;
            that.data.attrTable_fmt.forEach(function(a) {
              a.stock = 0;
            });
            that.setData({
              "p": that.data.p,
            });
          }
        }
      });
    } else {
      that.data.p.stockLimit = !that.data.p.stockLimit;
      that.setData({
        "p": that.data.p,
      });
    }
  },
  input_p_stock: function(e) {
    var that = this;
    that.setData({
      "p.stock": parseInt(e.detail.value) || 0
    });
  },

  //虚拟销量
  input_p_virtualSalesCount: function(e) {
    var that = this;
    that.setData({
      "p.virtualSales": parseInt(e.detail.value) || 0
    });
  },
  input_p_price: function(e) {
    var that = this;
    that.setData({
      "p.price": e.detail.value
    });
  },
  input_p_groupPrice: function(e) {
    var that = this;
    that.setData({
      "p.groupPrice": e.detail.value
    });
  },
  input_p_originalPrice: function(e) {
    var that = this;
    that.setData({
      "p.originalPrice": e.detail.value
    });
  },
  input_p_unit: function(e) {
    var that = this;
    that.setData({
      "p.unit": e.detail.value
    });
  },

  //点击设置规格
  click_set_attr: function() {
    this.setData({
      "tabSelectedIndex": 2,
    });
  },

  //点击添加规格
  show_attr_modal: function() {
    //重置所有属性的sel
    var that = this;

    //查询出所有已选择的属性
    var attr_sel = [];
    if (that.data.pickspecification.length > 0) {
      that.data.pickspecification.forEach(function(p) {
        if (p.items && p.items.length > 0) {
          attr_sel = attr_sel.concat(p.items);
        }
      });
    }
    //查询出所有属性值
    var attrval = [];
    if (that.data.goodAttrList.length > 0) {
      that.data.goodAttrList.forEach(function(pp) {
        if (pp.children && pp.children.length > 0) {
          attrval = attrval.concat(pp.children);
        }
      });
    }

    //自动选中已选的属性
    attr_sel.forEach(function(p) {
      attrval.forEach(function(pp) {
        if (p.id == pp.id) {
          pp.sel = true;
        }
      });
    });


    that.setData({
      "goodAttrList": that.data.goodAttrList,
      "pickspecification": that.data.pickspecification,
      "show_attr": true,
      "attr_pick_nums": that.attr_pick_nums(),
    });
  },

  //隐藏选择属性弹窗
  hide_attr_modal: function() {
    this.setData({
      "show_attr": false,
    });
  },

  //点击属性
  choose_attr: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var index = Number(ds.index);
    var index2 = Number(ds.indexs);

    this.data.goodAttrList[index].children[index2].sel = !this.data.goodAttrList[index].children[index2].sel;
    that.setData({
      "goodAttrList": that.data.goodAttrList,
      "attr_pick_nums": that.attr_pick_nums(),
    });
  },

  //确定选择属性
  choose_attr_ok: function() {
    var pickspecification_sel = [];
    var that = this;
    //将选中的属性添加到p.pickspecification
    if (that.data.goodAttrList && that.data.goodAttrList.length > 0) {
      //查询选中的一级分类
      var level1 = that.data.goodAttrList.filter(function(p) {
        return p.children && p.children.length > 0 && p.children.filter(function(pp) {
          return pp.sel;
        }).length > 0;
      });
      if (level1.length > 4) {
        core.alert("最多只能选择4种规格");
        return;
      }
      if (level1 && level1.length > 0) {
        level1.forEach(function(p) {
          pickspecification_sel.push({
            id: p.id,
            name: p.name,
            fId: p.parentid,
            items: p.children.filter(function(pp) {
              return pp.sel;
            })
          });
        });
      }
    }
    if (pickspecification_sel.length > 40) {
      core.alert("最多只能选择40种规格值组合");
      return;
    }

    that.setData({
      "pickspecification": pickspecification_sel,
      "pickspecification_fmt": that.pickspecification_fmt(pickspecification_sel),
      "show_attr": false,
    });
    that.buildTable();
  },
  remove_attr: function(index) {
    var that = this;
    wx.showModal({
      title: "提示",
      content: "删除后将重新建立规格组合，价格、库存、图片将会清空，请谨慎操作",
      success: function success(res) {
        if (res.confirm) {
          that.data.pickspecification.splice(index, 1);
          that.buildTable();
          that.setData({
            "pickspecification": that.data.pickspecification,
            "pickspecification_fmt": that.pickspecification_fmt(that.data.pickspecification),
          });
        } else if (res.cancel) {}
      }
    });
  },
  show_batch_modal: function() {
    this.setData({
      show_batch: true,
    });
  },

  //隐藏批量设置弹窗
  hide_batch_modal: function() {
    this.setData({
      show_batch: false,
    });
  },
  input_batch_price: function(e) {

    this.setData({
      batchPrice: e.detail.value,
    });
  },
  input_batch_group_rice: function(e) {
    this.setData({
      batchGroupPrice: e.detail.value,
    });
  },
  input_batch_stock: function(e) {
    this.setData({
      batchStock: e.detail.value
    });
  },

  //确定批量设置
  input_batch_ok: function() {
    var that = this;
    var price = core.safeNum(that.data.batchPrice) || 0;
    var groupPrice = core.safeNum(that.data.batchGroupPrice) || 0;
    var stock = core.safeNum(that.data.batchStock) || 0;

    if (price < 0) {
      that.setData({
        batchPrice: 0,
      });
      core.alert("请输入价格");
      return;
    }
    if (groupPrice < 0) {
      core.alert("返现金额输入错误");
      that.setData({
        batchGroupPrice: 0,
      });
      return;
    }
    if (groupPrice > price) {
      core.alert("返现金额不能大于单买价");
      that.setData({
        batchGroupPrice: 0,
      });
      return;
    }
    if (stock < 0) {
      core.alert("库存输入错误");
      that.setData({
        batchStock: 0,
      });
      return;
    }
    if (that.data.attrTable_fmt.length > 0) {
      that.data.attrTable_fmt.forEach(function(p) {
        p.price = price;
        p.groupPrice = groupPrice;
        p.stock = stock;

      });
    }

    that.setData({
      show_batch: false,
      attrTable_fmt: that.data.attrTable_fmt,
    });
  },

  //点击选择图片
  pickImg: function(e) {
    var that = this;
    var index = Number(e.currentTarget.dataset.index);
    core.upload("img").then(function(urlArray) {
      var key = "attrTable_fmt[" + index + "].imgUrl"
      that.setData({
        [key]: urlArray[0],
      });
    });
  },

  del_good_type: function(e) {
    var that = this;
    var g = getApp().globalData;
    var id = e.currentTarget.dataset.id;
    wx.showModal({
      title: "提示",
      content: "删除后不可恢复，确认删除吗？",
      success: function(res) {
        if (res.confirm) {
          var model = JSON.parse(JSON.stringify(that.data.goodTypeModel));
          model.id = id;
          core.GoodType(g.utoken, "del", model).then(function(result) {

            if (result.code == 1) {
              that.loadTypes(g);
            }
            if (result.msg) {
              wx.showModal({
                title: '提示',
                content: result.msg,
              })
            }
          });
        }
      }
    });
  },
  del_good_attr: function(e) {
    var that = this;
    var g = getApp().globalData;
    var id = e.currentTarget.dataset.id;
    wx.showModal({
      title: "提示",
      content: "删除后不可恢复，确认删除吗？",
      success: function(res) {
        if (res.confirm) {
          var model = JSON.parse(JSON.stringify(that.data.attrModel));
          model.id = id;
          core.GoodAttr(g.utoken, "del", model).then(function(result) {

            if (result.code == 1) {
              that.loadTypes(g);
            }
            if (result.msg) {
              wx.showModal({
                title: '提示',
                content: result.msg,
              })
            }
          });
        }
      }
    });
  },
  update_good_type: function(e) {
    var ds = e.currentTarget.dataset;
    var id = Number(ds.id);
    var fid = Number(ds.fid);
    var name = ds.name;
    var that = this;
    that.setData({
      "show_addtype": true,
      "goodTypeModel.id": id,
      "goodTypeModel.fId": fid,
      "goodTypeModel.name": name,
      "canSaveType": name.length > 0.
    });

    if (fid > 0) {
      that.data.goodTypeModel.selindex = that.data.firstLevel.findIndex(t => {
        return t.id == fid;
      });
    } else {
      that.data.goodTypeModel.selindex = 0;
    }

    that.setData({
      "goodTypeModel.selindex": that.data.goodTypeModel.selindex,
    });
  },
  update_good_attr: function(e) {
    var ds = e.currentTarget.dataset;
    var id = Number(ds.id);
    var fid = Number(ds.fid);
    var name = ds.name;
    var that = this;

    that.setData({
      "show_addattr": true,
      "attrModel.id": id,
      "attrModel.fId": fid,
      "attrModel.name": name,
      "canSaveAttr": name.length > 0.
    });
    if (fid > 0) {
      that.data.attrModel.selindex = that.firstAttr().findIndex(t => {
        return t.id == fid;
      });
    } else {
      that.data.attrModel.selindex = 0;
    }

    that.setData({
      "attrModel.selindex": that.data.attrModel.selindex,
    });
  },
  add_type: function(e) {
    var that = this;
    var ds = e.currentTarget.dataset;
    var id = Number(ds.id);
    var name = ds.name;
    var fid = Number(ds.fid);

    if (fid > 0) {
      that.data.goodTypeModel.selindex = that.data.firstLevel.findIndex(t => {
        return t.id == fid;
      });
    } else {
      that.data.goodTypeModel.selindex = 0;
    }
    that.setData({
      "show_addtype": true,
      "goodTypeModel.id": id,
      "goodTypeModel.fId": fid,
      "goodTypeModel.name": name,
      "goodTypeModel.selindex": that.data.goodTypeModel.selindex
    });
  },
  hide_addtype: function() {
    this.initTypeModel();
  },
  initTypeModel: function() {
    var that = this;
    that.setData({
      "show_addtype": false,
      "goodTypeModel.id": 0,
      "goodTypeModel.name": "",
      "goodTypeModel.fId": that.data.firstLevel.length > 0 ? 0 : -1
    });
  },
  input_type_name: function(e) {
    var val = core.trim(e.detail.value);
    this.setData({
      "goodTypeModel.name": val,
      "canSaveType": val.length > 0.
    });
  },
  add_good_type_ok: function() {
    var that = this;
    wx.showLoading({
      title: "提交中...",
      mask: true
    });
    var g = getApp().globalData;
    var model = JSON.parse(JSON.stringify(that.data.goodTypeModel));

    core.GoodType(g.utoken, "", model).then(function(result) {
      wx.hideLoading();
      if (result.code == 1) {

        that.loadTypes(g);
        that.initTypeModel();

        var msg = model.id > 0 ? "修改成功" : "添加成功";
        wx.showToast({
          title: msg,
          icon: "success",
          duration: 2000
        });
      } else {
        core.alert(result.msg);
      }
    });

  },
  attr_pick_nums: function() {
    var nums = 0;
    var that = this;
    if (that.data.goodAttrList) {
      that.data.goodAttrList.forEach(p => {
        if (p.children) {
          nums += p.children.filter(child => {
            return child.sel;
          }).length;
        }
      });
    }
    return nums;
  },
  //将数据临时保存
  saveTemp: function saveTemp() {
    /*var tempGoodInfo = core.getStorageSync("tempGoodInfo");
    if (tempGoodInfo == "error") {
      core.alert("保存失败，请重试");
      return;
    }*/
    var that = this;
    if (that.data.attrTable_fmt.length <= 0) {
      core.alert("请先设置产品规格");
      return;
    } else {
      // var result = core.setStorageSync("tempGoodInfo", {
      //   p: that.p,
      //   attrTable_fmt: that.attrTable_fmt
      // });
      // if (result == "error") {
      //   core.alert("保存失败，请重试");
      //   return;
      // }
      this.data.p.price = that.data.attrTable_fmt.sort(function(a, b) {
        return a.price - b.price;
      })[0].price;
      this.data.p.price = Number(this.data.p.price) || 0;
      if (this.data.p.price <= 0) {
        core.alert("产品价格不能小于0");
        return;
      }
      that.data.p.stock = 0;
      this.data.attrTable_fmt.forEach(function(t) {
        that.data.p.stock += Number(t.stock) || 0;
      });
      if (this.data.p.stockLimit && that.data.p.stock <= 0) {
        core.alert("选择限制产品库存时，产品库存不能为0");
        return;
      }
    }

    that.setData({
      "tabSelectedIndex": 1,
      "p": that.data.p,
      "canSetPrice": false,
    });
  },

  save: function() {
    var that = this;
    var g = getApp().globalData;
    if (that.data.saving) return;
    var model = JSON.parse(JSON.stringify(that.data.p));
    that.data.saving = true;
    wx.showLoading({
      mask: true,
      title: "保存中"
    });
    //aid
    model.aid = g.aid;
    //名称
    model.name = core.trim(model.name);
    if (model.name.length == "") {
      core.alert("请填写产品名称！");
      that.data.saving = false;
      wx.hideLoading();
      return;
    }
    if (model.name.length > 100) {
      core.alert("产品名称不能超过100个字");
      that.data.saving = false;
      wx.hideLoading();
      return;
    }

    //库存
    if (model.stockLimit) {
      if (!/^\d{0,7}$/.test(model.stock) || parseInt(model.stock) < 0) {
        core.alert("请输入产品库存,库存只能输入>=0的整数！");
        that.data.saving = false;
        wx.hideLoading();
        return;
      }
    }
    //图片
    if (model.img == "") {
      core.alert("请上传产品图片！");
      that.data.saving = false;
      wx.hideLoading();
      return;
    }
    //分类
    model.cateIdOne = that.data.ptypeSelectedId;
    model.cateId = that.data.goodTypes
      .filter(t => {
        return t.fId != 0 && t.sel;
      })
      .map(i => {
        return i.id;
      })[0];
    if (model.cateIdOne == 0 || model.cateId == 0) {
      core.alert("请选择产品分类！");
      that.data.saving = false;
      wx.hideLoading();
      return;
    }

    //标签

    //参数

    var saveAttrTable = JSON.parse(JSON.stringify(that.data.attrTable_fmt));
    //规格
    if (that.data.attrTable_fmt && that.data.attrTable_fmt.length > 0) {
      //创建副本

      //检查规格 价格，库存输入是否正确
      saveAttrTable.forEach(p => {});
      for (let index = 0; index < saveAttrTable.length; index++) {
        let p = saveAttrTable[index];
        if (!/^\d{1,}.?(\d{0,2})?$/.test(p.price) ||
          !/^\d{1,}.?(\d{0,2})?$/.test(p.groupPrice)
        ) {
          core.alert("产品价格请输入大于0的数字，最多两位小数！");
          that.data.saving = false;
          wx.hideLoading();
          return;
        }
      }

      if (saveAttrTable && saveAttrTable.length > 0) {
        saveAttrTable.forEach(function(obj) {
          obj.price = parseInt((Number(obj.price) || 0) * 100);
          obj.groupPrice = parseInt((Number(obj.groupPrice) || 0) * 100);
        });
      }
      model.specificationdetail = JSON.stringify(saveAttrTable);
      model.pickspecification = JSON.stringify(that.data.pickspecification);



      // var specificationkeys = [];
      // var _specification = [];
      // if (that.data.pickspecification.length > 0) {
      //   that.data.pickspecification.forEach(function(_item) {
      //     if (_item.items.length > 0) {
      //       //保存规格
      //       specificationkeys.push(_item.id);
      //       //保存规格值
      //       _item.items.forEach(function(_item_value) {
      //         _specification.push(_item_value.id);
      //       });
      //     }
      //   });
      // }
      // that.data.p.specificationkeys = specificationkeys.join(",");
      // that.data.p.specification = _specification.join(",");
    }
    //价格
    if (!/^\d*.?\d{0,2}$/.test(model.price)) {
      core.alert("产品价格请输入大于0的数字，最多两位小数！");
      that.data.saving = false;
      wx.hideLoading();
      return;
    } else if (model.price <= 0 || model.price > 1000000000) {
      core.alert("产品价格请输入大于0小于1000000000的数字，最多两位小数！");
      that.data.saving = false;
      wx.hideLoading();
      return;
    }
    //轮播图
    model.imgAlbum = model.imgAlbumList.join(",");
    //重量，单位kg
    //that.p.Weight = that.p.Weight * 1000;
    //运费模板
    //详情
    model.description = that.data.description || "";

    model.originalPrice = parseInt((Number(model.originalPrice) || 0) * 100);
    model.price = parseInt((Number(model.price) || 0) * 100);
    model.groupPrice = parseInt((Number(model.groupPrice) || 0) * 100);
    

    if (saveAttrTable && saveAttrTable.length > 0) {
      //选择最小价格作为产品的展示价格
      model.price = saveAttrTable.sort((a, b) => {
        return a.price - b.price;
      })[0].price;

      //最高返现
      model.groupPrice = saveAttrTable.sort(function(a, b) {
        return b.groupPrice - a.groupPrice;
      })[0].groupPrice;
    }

    if (model.groupPrice > model.price){
      core.alert("返现金额不能高于产品价格！");
      wx.hideLoading();
      return;
    }

    core.SaveGoodsInfo(g.utoken, "", model).then(function(result) {

      core.alert(result.msg);
      that.data.saving = false;
      wx.hideLoading();
      if (result.code == 1) {
        setTimeout(function() {
          that.back();
        }, 1000);
      }
    });

  },
  back: function() {
    var pages = getCurrentPages();
    var productPageIndex = pages.findIndex(p => p.route == "pages/my/mgr/product/product");
    if (productPageIndex == -1) {
      wx.redirectTo({
        url: "/pages/my/mgr/product/product"
      });
    } else {
      var backNum = pages.length - 1 - productPageIndex;
      wx.navigateBack({
        delta: backNum
      });
    }
  },
  cancel: function() {
    var that = this;
    wx.showModal({
      title: "提示",
      content: "确定退出么",
      success: function(res) {
        if (res.confirm) {
          that.back();
        }
      }
    });
  }
})