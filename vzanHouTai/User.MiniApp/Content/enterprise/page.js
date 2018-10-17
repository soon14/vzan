define([
    'pagedata',
], function (pagedata) {
    var mixin = {
        el: "#app",
        beforeCreated: function () {

        },
        created: function () {
            this.utils = utils;
            this.Init();
        },
        mounted: function () {
            console.log("mounted");
            var that = this;

            $(".app-fields-normal").sortable({
                axis: "y",
                items: "> div:not(.js-not-sortable)",
                cursor: "move",
                //revert: true,
                start: function (e, t) {

                    t.item.data("startPos", t.item.index());
                },
                stop: function (t, n) {

                    var _currentPage = that.selPage();
                    if (_currentPage && _currentPage.coms.length >= 0) {
                        var preindex = n.item.data("startPos"), currentindex = n.item.index();

                        if (n.item.hasClass("app-com")) {
                            n.item.remove();
                            if (that.PickCom(n.item.attr("comname"))) {
                                console.log("preindex:" + preindex + ",currentindex:" + currentindex);
                                //删除
                                var dragelement = _currentPage.coms.splice(preindex, 1)[0];
                                //插入
                                _currentPage.coms.splice(currentindex, 0, dragelement);
                                _currentPage.coms.forEach(function (o, i) {
                                    o.sortindex = i;
                                });
                            }
                            return;
                        }



                        //删除
                        var dragelement = _currentPage.coms.splice(preindex, 1)[0];
                        //插入
                        _currentPage.coms.splice(currentindex, 0, dragelement);
                        _currentPage.coms.forEach(function (o, i) {
                            o.sortindex = i;
                        });
                    }
                    $(".app-fields-normal").sortable("cancel");
                }
            })

            $("#pageslist").sortable({
                axis: "y",
                items: "> div",
                cursor: "move",
                start: function (e, t) {
                    t.item.data("startPos", t.item.index());
                },
                stop: function (t, n) {

                    if (that.pages.length > 0) {
                        var preindex = n.item.data("startPos"), currentindex = n.item.index();
                        var dragelement = that.pages.splice(preindex, 1)[0];
                        that.pages.splice(currentindex, 0, dragelement);
                    }
                    console.log(preindex, currentindex);
                    that.selPageIndex = currentindex;
                    $("#pageslist").sortable("cancel");

                }
            })

            $(".app-com").draggable({
                connectToSortable: ".app-fields-normal",
                helper: "clone",
                addClasses: false,
                //revert: "invalid",
                //snap: true,
                stop: function (event, ui) {

                }
            });
            $(".app-coms,#pageslist,.app-fields-normal,#pageslist").disableSelection();
            //初始化上传组件
            this.initUploader('img');
            this.initUploader('video');
            this.initUploader('audio');
            this.SyncData();

            this.shopLogo_url = this.GetShoperLogoUrl(); //获取小程序logo
            this.UpdateWxShopLogoUrl(); //获取小程序logo
        },
        data: pagedata,
        watch: {
            "selComIndex": function (val, oldVal) {
                console.log("index change", val, oldVal);
                var that = this;
                if (val == -1)
                    return;


                if (this.selCom().type == "richtxt") {
                    if (that.comeditor != null) {
                        that.comeditor.setContent(app.selCom().content);
                    }
                }
                if (this.selCom().type == "news") {
                    that.$nextTick(function () {
                        if (that.$refs.newspager) {
                            that.$refs.newspager.typeid = this.selCom().typeid;
                        }
                    });
                }
                if (this.selCom().type == "cutprice") {
                    Vue.nextTick()
                        .then(function () {
                            if (that.$refs.cutpricepager) {
                                that.$refs.cutpricepager.current_page = 1;
                                that.$refs.cutpricepager.GetCutPrice();
                            }
                        })
                }
                if (this.selCom().type == "joingroup") {
                    Vue.nextTick()
                        .then(function () {
                            if (that.$refs.joingrouppager) {
                                that.$refs.joingrouppager.current_page = 1;
                                that.$refs.joingrouppager.GetJoinGroup();
                            }
                        })
                }
                if (this.selCom().type == "entjoingroup") {
                    Vue.nextTick()
                        .then(function () {
                            if (that.$refs.entjoingrouppager) {
                                that.$refs.entjoingrouppager.current_page = 1;
                                that.$refs.entjoingrouppager.GetEntJoinGroup();
                            }
                        })
                }
                if (this.selCom().type == "good" ||
                    this.selCom().type == "live" ||
                    this.selCom().type == "img") {
                    that.$nextTick(function () {
                        if (that.$refs.goodpager) {
                            that.$refs.goodpager.current_page = 1;
                            that.$refs.goodpager.GetProducts();
                        }
                    });
                }
                if (this.selCom().type == "coupons") {
                    that.$nextTick(function () {
                        if (that.$refs.couponspager) {
                            that.$refs.couponspager.current_page = 1;
                            that.$refs.couponspager.GetCoupons();
                        }
                    })

                }
                if (this.selCom().type == "bottomnav") {
                    if (bottomNavSync != null) {
                        clearInterval(bottomNavSync);
                    }
                    bottomNavSync = setInterval(function () {
                        that.SyncBottomNav(that.selCom());
                    }, 1000);

                }
                else {
                    clearInterval(bottomNavSync);
                }

                if (this.selCom().type == "flashdeal") {
                    that.getFlashDeals(function (flashdeals) {
                        that.flashDeals = flashdeals;
                        
                    });
                }
                

            },
            "comeditor": function () {
                console.log("comeditor change");
            },
        },
        computed: {
            normalpages: function () {
                return this.pages.filter(function (item) {
                    return (!("def_name" in item) || ("def_name" in item && item.def_name == ""));
                })
            },
            canComLink: function () {
                return this.selCom() && (this.selCom().type == 'slider' || this.selCom().type == 'imgnav' || this.selCom().type == 'img' || this.selCom().type == 'magicCube' || this.selCom().type == 'bottomnav');
            }
        },
        methods: {
            findGoods: function () {
                var that = this;
                that.$nextTick(function () {
                    if (that.$refs.goodpager) {
                        that.$refs.goodpager.current_page = 1;
                        that.$refs.goodpager.GetProducts();
                    }
                });
            },
            findCoupons: function () {
                var that = this;
                that.$nextTick(function () {
                    if (that.$refs.couponspager) {
                        that.$refs.couponspager.current_page = 1;
                        that.$refs.couponspager.GetCoupons();
                    }
                });
            },
            BuildEditor: function () {
                console.log("BuildEditor");
                var that = this;
                if (comeditor != null) {
                    UE.delEditor("editor");
                }

                //编辑器
                comeditor = UE.getEditor('editor', {
                    //, 'insertvideo',
                    toolbars: [
                        ['source', '|', 'undo', 'redo', '|', 'bold', ' italic', ' underline', 'strikethrough', '|', 'forecolor', 'backcolor', '|', 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify'],
                        ['paragraph', 'fontsize', ' |', 'insertorderedlist', 'insertunorderedlist', 'horizontal', 'autotypeset', 'pasteplain'],
                        ['simpleupload', 'insertimage']
                    ],
                    //imageUrl: "/CityUpload/Image?cityid=" + aid,
                    autoHeight: false
                });

                comeditor.ready(function () {

                    if (that.selCom()) {
                        if (that.selCom().type == "richtxt") {
                            var _txt = that.selCom().content
                            comeditor.setContent(_txt);
                        }
                    }
                });
                function asyncContent() {
                    if (that.selCom() && comeditor != null) {
                        if (that.selCom().type == "richtxt") {
                            that.selCom().content = comeditor.getContent();
                        }
                    }
                }
                comeditor.addListener("contentChange", function () {
                    setTimeout(function () {
                        asyncContent();
                    }, 1000);
                });

            },

            //把底部导航放到最后面
            SortPageCom: function () {
                var _currentPage = this.selPage();
                if (_currentPage && _currentPage.coms.length > 0) {
                    var _bottomnav_index = this.GetPageComIndex('bottomnav');
                    if (_bottomnav_index != -1) {
                        //删除
                        var dragelement = _currentPage.coms.splice(_bottomnav_index, 1)[0];
                        _currentPage.coms.push(dragelement);
                    }
                }
            },
            SortPage: function () {
                if (this.pages.length > 1) {
                    var _pageyuyue_index = this.pages.findIndex(function (item) {
                        return "def_name" in item && item.def_name == "产品预约";
                    });
                    if (_pageyuyue_index != -1) {
                        var _pageyuyue = this.pages.splice(_pageyuyue_index, 1)[0];
                        this.pages.push(_pageyuyue);
                    }
                }
                var _currentPage = this.selPage();
                if (_currentPage && _currentPage.coms.length > 0) {
                    var _bottomnav_index = this.GetPageComIndex('bottomnav');
                    if (_bottomnav_index != -1) {
                        //删除
                        var dragelement = _currentPage.coms.splice(_bottomnav_index, 1)[0];
                        _currentPage.coms.push(dragelement);
                    }

                }
            },
            SyncBottomNav: function (com) {
                //console.log(com)
                if (com && com.type == "bottomnav") {
                    //var com_clone = JSON.parse(JSON.stringify(com));
                    if (this.normalpages.length > 0) {
                        this.normalpages.forEach(function (page_item) {
                            if (page_item.coms.length > 0) {
                                for (var i = page_item.coms.length - 1; i >= 0; i--) {
                                    var _current_com = page_item.coms[i];
                                    if (_current_com.type == "bottomnav") {
                                        page_item.coms.splice(i, 1, com);
                                        console.log("SyncBottomNav ok")
                                        break;
                                    }
                                }

                            }
                        });
                    }
                }
                else {
                    if (bottomNavSync != null) {
                        clearInterval(bottomNavSync);
                    }
                }
            },

            Init: function () {
                //初始化组件
                this.InitCom();
                this.SortPage();
                if (this.normalpages.length == 0) {
                    this.InitPage();
                    this.SortPage();
                    //给每个页面添加底部导航组件
                    for (var i = 0; i < this.normalpages.length; i++) {
                        var _cpage = this.normalpages[i];
                        if (pagetype != 26)
                            _cpage.coms.push(this.CreateCom("bottomnav"));
                    }

                }

                this.selPageIndex = 0;
                if (this.selPage() && this.selPage().coms.length > 0) {
                    this.SortPageCom();
                }
                //初始化皮肤
                this.skinList[this.normalpages[0].skin || 0].sel = true;
               

            },
            InitCom: function () {
                //给产品列表组件添加两个默认产品分类
                if (this.goodtypelist.length > 0 && pagetype != 26) {
                    this.coms.goodlist.goodCat = this.coms.goodlist.goodCat.concat(this.goodtypelist.slice(0, 3));
                }
                //给轮播图添加三个默认项
                var _new_slider_item1 = $.extend(true, {}, sliderItem);
                //var _new_slider_item2 = $.extend(true, {}, sliderItem);
                //var _new_slider_item3 = $.extend(true, {}, sliderItem);
                this.coms.slider.items = this.coms.slider.items.concat(_new_slider_item1);
                //给图片导航添加4个默认项
                var _new_imgnav_item1 = $.extend(true, {}, navItem);
                var _new_imgnav_item2 = $.extend(true, {}, navItem);
                var _new_imgnav_item3 = $.extend(true, {}, navItem);
                var _new_imgnav_item4 = $.extend(true, {}, navItem);
                this.coms.imgnav.navlist = this.coms.imgnav.navlist.concat(_new_imgnav_item1, _new_imgnav_item2, _new_imgnav_item3, _new_imgnav_item4);

                if (this.coms.magicCube) {
                    //魔方图片添加默认项
                    var _new_magicCube_item1 = $.extend(true, {}, magicItem);
                    var _new_magicCube_item2 = $.extend(true, {}, magicItem);
                    var _new_magicCube_item3 = $.extend(true, {}, magicItem);
                    var _new_magicCube_item4 = $.extend(true, {}, magicItem);
                    this.coms.magicCube.items = this.coms.magicCube.items.concat(_new_magicCube_item1, _new_magicCube_item2, _new_magicCube_item3, _new_magicCube_item4);
                }

                if (pagetype != 26) {


                    //给表单添加默认项
                    var _newformitem1 = $.extend(true, {}, formItem.text);
                    _newformitem1.name = "姓名";
                    _newformitem1.placeholder = "请输入您的名字";
                    var _newformitem2 = $.extend(true, {}, formItem.number);
                    _newformitem2.name = "号码";
                    _newformitem2.placeholder = "请输入您的手机号码";
                    var _newformitem3 = $.extend(true, {}, formItem.radio);
                    _newformitem3.name = "请选择您想代理的产品";

                    var _newformitem3_item1 = $.extend(true, {}, formMutiSelectItem);
                    var _newformitem3_item2 = $.extend(true, {}, formMutiSelectItem);
                    var _newformitem3_item3 = $.extend(true, {}, formMutiSelectItem);
                    var _newformitem3_item4 = $.extend(true, {}, formMutiSelectItem);
                    _newformitem3_item1.name = "";
                    _newformitem3_item2.name = "";
                    _newformitem3_item3.name = "";
                    _newformitem3_item4.name = "";
                    _newformitem3.items = _newformitem3.items.concat(_newformitem3_item1, _newformitem3_item2, _newformitem3_item3, _newformitem3_item4);
                    this.coms.form.title = "表单名称";
                    this.coms.form.items = this.coms.form.items.concat(_newformitem1, _newformitem2, _newformitem3);

                    //给单选项添加两个默认项
                    var _new_radio_item1 = $.extend(true, {}, formMutiSelectItem);
                    var _new_radio_item2 = $.extend(true, {}, formMutiSelectItem);
                    formItem.radio.items = formItem.radio.items.concat(_new_radio_item1, _new_radio_item2);

                    //给产品预约添加默认项
                    var yyformitem1 = $.extend(true, {}, formItem.text);
                    yyformitem1.name = "姓名";
                    yyformitem1.placeholder = "请输入您的姓名";
                    var yyformitem2 = $.extend(true, {}, formItem.number);
                    yyformitem2.name = "手机号";
                    yyformitem2.placeholder = "请输入您的手机号码";
                    var yyformitem3 = $.extend(true, {}, formItem.sex);
                    var yyformitem4 = $.extend(true, {}, formItem.number);
                    yyformitem4.name = "年龄";
                    yyformitem4.placeholder = "请输入您的年龄";
                    var yyformitem5 = $.extend(true, {}, formItem.text);
                    yyformitem5.name = "地址";
                    yyformitem5.placeholder = "请输入您的地址";
                    var yyformitem6 = $.extend(true, {}, formItem.text);
                    yyformitem6.name = "备注";
                    yyformitem6.placeholder = "不超过20字";
                    this.coms.yyform.items = this.coms.yyform.items.concat(yyformitem1, yyformitem2, yyformitem3, yyformitem4, yyformitem5, yyformitem6);
                }
                //给外卖组件添加两个默认产品分类
                //if (this.goodtypelist.length > 0 && pagetype != 26) {
                //    this.coms.takeout.goodCat = this.coms.takeout.goodCat.concat(this.goodtypelist.slice(0, 3));
                //}
            },
            InitPage: function () {
                //如果未设置过，默认生成5个页面
                var page1 = $.extend(true, {}, pageItem);
                var page2 = $.extend(true, {}, pageItem);
                var page3 = $.extend(true, {}, pageItem);
                var page4 = $.extend(true, {}, pageItem);
                page1.name = "首页";
                page1.sel = true;
                page2.name = "产品中心";
                page3.name = "客户案例";
                page4.name = "关于我们";

                if (pagetype == 26) {
                    this.pages = this.pages.concat(page1);
                }
                else {
                    this.pages = this.pages.concat(page1, page2, page3, page4);
                }


                //默认选中第一页
                this.selPageIndex = 0;
            },
            clickGoodCatNav: function () {
                if (this.selCom()) {
                    this.selCom().isShowGoodSearch = false;
                    this.selCom().isShowGoodPriceSort = false;
                    this.selCom().isShowFilter = false;
                    this.selCom().isShowGoodSaleCountSort = false;
                    if (!this.selCom().isShowGoodCatNav) {
                        this.selCom().GoodCatNavStyle = 1;
                    }
                }
            },
            clickNewsCatNav: function () {
                if (this.selCom()) {
                   
                    if (!this.selCom().isShowNewsNav) {
                        this.selCom().NewsNavStyle = 1;
                    }
                }
            },
            isSysPage: function (pageitem, pagename) {
                if (!pageitem) {
                    return;
                }
                if (pagename == undefined) {
                    return ("def_name" in pageitem && pageitem.def_name != "")
                }
                return "def_name" in pageitem && pageitem.def_name == pagename;
            },
            isCreated: function (comsitem, comsname) {
                if (!comsitem) {
                    return false;
                }
                var result = false;
                $.each(comsitem, function (i, item) {
                    if (item.name == comsname && item.type != 'sex') {
                        result = true;
                        return false;
                    } else if (item.type == 'sex' && item.type == comsname) {
                        result = true;
                        return false;
                    } else {
                        result = false;
                    }
                })
                return result;
            },
            ChangeSkin: function (index) {
               
                this.skinList.forEach(function (o, i) {
                    o.sel = false;
                });
                this.skinList[index].sel = true;
                if (this.normalpages.length > 0) {
                    this.pages.forEach(function (o) {
                        o.skin = index;
                    });
                }
            },
            //创建组件
            CreateCom: function (comname) {
                if (!this.selPage()) {
                    console.warn("请选择要添加组件的页面");
                    return;
                }
                var that = this;

                var _newcom = null;
                //创建底部导航组件
                if (comname == 'bottomnav') {
                    if (this.normalpages.length > 0) {
                        var firstBottomNav = this.normalpages[0].coms.find(function (item) {
                            return item.type == "bottomnav";
                        });
                        if (firstBottomNav != null) {
                            return JSON.parse(JSON.stringify(firstBottomNav));
                        }
                    }
                    var _bottomnav_com = pagedata.coms.bottomnav;
                    _bottomnav_com.navlist = [];

                    this.normalpages.filter(function (item) {
                        return (!("def_name" in item) || ("def_name" in item && item.def_name == ""));
                    }).forEach(function (o, i) {
                        var _newitem = $.extend(true, {}, navItem);
                        _newitem.name = o.name;
                        _newitem.img = ""
                        _newitem.url = -1,
                            _bottomnav_com.navlist.push(_newitem);
                    });

                    if (_bottomnav_com.navlist.length > MAX_BOTTOMNAVITEMCOUNT) {
                        _bottomnav_com.navlist = _bottomnav_com.navlist.splice(0, MAX_BOTTOMNAVITEMCOUNT);
                    }
                    for (var i = 0; i < _bottomnav_com.navlist.length; i++) {
                        switch (i) {
                            case 0:
                                _bottomnav_com.navlist[i].sel = true;
                                _bottomnav_com.navlist[i].icon = "icon-home2";
                                break;
                            case 1:
                                _bottomnav_com.navlist[i].icon = "icon-show1";
                                break;
                            case 2:
                                _bottomnav_com.navlist[i].icon = "icon-news2";
                                break;
                            case 3:
                                _bottomnav_com.navlist[i].icon = "icon-personal1";
                                break;
                            case 4:
                                _bottomnav_com.navlist[i].icon = "icon-news2";
                                break;
                        }
                    }

                    var _bottomnav_com = $.extend(true, {}, pagedata.coms.bottomnav);

                    if (_bottomnav_com.navlist.length > 0) {
                        _bottomnav_com.navlist[0].sel = true;
                    }
                    _newcom = JSON.parse(JSON.stringify(_bottomnav_com))
                }
                else if (comname == 'contactShopkeeper') {
                    //contactShopkeeper = pagedata.coms.contactShopkeeper;
                    ////获取当前用户小程序头像
                    //contactShopkeeper.shopLogo_url = this.GetShoperLogoUrl();
                    if (pagedata.coms.contactShopkeeper) {
                        _newcom = JSON.parse(JSON.stringify(pagedata.coms.contactShopkeeper));
                        _newcom.shopLogo_url = that.shopLogo_url;
                    }

                }
                else {
                    _newcom = JSON.parse(JSON.stringify(pagedata.coms[comname]));
                }
                return _newcom;
            },
            selPage: function () {
                if (this.selPageIndex < 0)
                    return false;
                return this.pages[this.selPageIndex];
            },
            selCom: function () {
                if (!this.selPage())
                    return false;
                var _selpage = this.selPage();
                if (_selpage.selComIndex >= 0) {
                    if (_selpage.selComIndex > _selpage.coms.length - 1) {
                        _selpage.selComIndex = 0;
                    }

                    //兼容旧数据，如果不存在预约开关则初始化预约开关
                    if ((_selpage.coms[_selpage.selComIndex].type == 'goodlist' || _selpage.coms[_selpage.selComIndex].type == 'good') && !("subscribeSwitch" in _selpage.coms[_selpage.selComIndex])) {
                        Vue.set(_selpage.coms[_selpage.selComIndex], 'subscribeSwitch', false);
                    }
                    return _selpage.coms[_selpage.selComIndex]
                }
                else
                    return false;
            },
            PickImage: function (uploadcom, index) {
                this.uploadCom = uploadcom;
                this.uploadIndex = index;
                $('#pcfileUpload').click();
            },
            uploadImage(event) {
                var that = this;
                var file = event.target;
                if (file.files.length > 0 && file.files[0].size / (1024 * 1024) > 10) {
                    layer.msg("请上传小于10M的图片", { time: 1500 });
                    return;
                }
                else if (file.files[0].size == 0) {
                    console.log("没有选择文件");
                    return;
                }
                $.ajaxFileUpload(
                    {
                        url: '/Upload/UploadImg', //用于文件上传的服务器端请求地址
                        secureuri: false, //是否需要安全协议，一般设置为false
                        data: {},
                        fileElementId: "pcfileUpload", //文件上传域的ID
                        dataType: 'json', //返回值类型 一般设置为json
                        timeout: 120 * 1000,
                        success: function (data, status)  //服务器成功响应处理函数
                        {
                            if (that.uploadCom && that.uploadIndex != -1) {
                                that.uploadCom[that.uploadIndex].img = data.Path;
                                that.uploadCom[that.uploadIndex].icon = "";
                            }
                            else if (that.uploadCom && that.uploadCom.type == "kefu") {
                                //that.uploadCom.img = data.Path;
                                Vue.set(that.uploadCom, "img", data.Path);
                            }
                            else if (that.uploadCom && that.uploadCom.type == "contactShopkeeper") {
                                //that.uploadCom.img = data.Path;
                                Vue.set(that.uploadCom, "iconUrl", data.Path);
                            }
                            else if (that.uploadCom && that.uploadCom.type == "img") {
                                Vue.set(that.uploadCom, "imgurl", data.Path);
                            }
                            //else if (that.uploadCom && that.uploadCom.type == "magicCube") {
                            //    Vue.set(that.uploadCom, "imgUrl", data.Path);
                            //}
                            else if (that.uploadCom && that.uploadCom.type == "video") {
                                Vue.set(that.uploadCom, "poster", data.Path);
                            }
                        },
                        error: function (data, status, e)//服务器响应失败处理函数
                        {
                            //$('#pcfileUpload').removeAttr("clickindex");

                        },
                        complete: function () {

                            $('#pcfileUpload').change(function (e) {
                                that.uploadImage(e);
                            });
                            that.uploadCom = null;
                            that.uploadIndex = -1;
                        }
                    })
            },
            PickDefaultIcon: function (uploadcom, index) {
                this.uploadCom = uploadcom;
                this.uploadIndex = index;
                $("#ChooseDefaultIconModal").modal("show");
            },
            /*********************底部导航选择字体图标**********************/
            ChooseDefaultIconItem: function (iconitem) {
                if (this.selCom()) {
                    this.selIconfont = iconitem;
                }
            },
            ChooseDefaultIconItemOK: function () {
                if (this.selCom() && this.selIconfont != "") {
                    if (this.selCom().type == "makecall"
                        || this.selCom().type == "kefu"
                        || this.selCom().type == "share") {
                        this.selCom().icon = this.selIconfont;
                    }
                    else {
                        if (this.uploadIndex >= 0) {
                            this.selCom().navlist[this.uploadIndex].icon = this.selIconfont;
                            this.selCom().navlist[this.uploadIndex].img = "";
                        }
                        else {
                            this.uploadCom.icon = this.selIconfont;
                            this.uploadCom.img = "";
                        }
                    }
                    this.selIconfont = "";
                }
                $("#ChooseDefaultIconModal").modal("hide");
            },
            AddNewsTypeOK: function () {
                var that = this;
                var _typename = $.trim(that.editptypeitem.name);
                if (_typename.length == 0 || _typename.length > 10) {
                    layer.msg("分类名称不能为空，且不能超过10个字", { time: 1000 });
                    return;
                }
                $.post("/MiappEnterprise/newstype", $.extend(that.editptypeitem, { appId: aid }), function (data) {
                    if (typeof data == "object") {
                        if (data.isok) {
                            $('#CreateNewsTypeModal').modal('hide');
                            layer.msg("保存成功", { time: 1000 });
                            if (ptypeitem.id == 0) {
                                that.newstypelist.push(data.msg);
                            }
                            that.editptypeitem = JSON.parse(JSON.stringify(ptypeitem));
                        }
                        else {
                            layer.msg(data.msg, { time: 1000 });
                        }
                    }
                    else {
                        layer.msg(data.msg, { time: 1000 });
                    }
                });
            },
            PickVideo: function (uploadcom) {
                //this.uploadCom = uploadcom;
                $('#pcfileUploadVideo').click();
                //this.
                //this.uploader.start();
            },
            PickAudio: function (uploadcom) {
                this.uploadCom = uploadcom;
                $('#pcfileUploadAudio').click();
            },

            //上传视频
            uploadVideo: function () {
                var that = this;
                var file = event.target;
                if (file.files.length > 0 && file.files[0].size / (1024 * 1024) > 20) {
                    layer.msg("请上传小于20M的视频", { time: 1500 });
                    return;
                }
                var _type = file.files[0].type;
                if (_type != "video/mp4") {
                    layer.msg("只支持：mp4格式", { time: 1500 });
                    return;
                }

                $.ajaxFileUpload(
                    {
                        url: '/Upload/UploadVedio?id=' + aid, //用于文件上传的服务器端请求地址
                        //url: "http://mp4-vod.oss-cn-hangzhou.aliyuncs.com/" + msg.NewFileName + "?uploadId=" + msg.UploadId,//partNumber=" + opts.formData.partNumber + "&
                        secureuri: false, //是否需要安全协议，一般设置为false
                        data: {},
                        fileElementId: "pcfileUploadVideo", //文件上传域的ID
                        dataType: 'json', //返回值类型 一般设置为json
                        timeout: 120 * 1000,
                        success: function (data, status)  //服务器成功响应处理函数
                        {

                            console.log(data, status);
                            if (!data.isok) {
                                layer.msg(data.msg, { time: 1500 });
                                return;
                            }
                            if (that.uploadCom) {
                                if (that.uploadCom.type == "video") {
                                    that.uploadCom.src = data.soucepath
                                }
                            }

                        },
                        error: function (data, status, e)//服务器响应失败处理函数
                        {

                        },
                        complete: function () {

                            $('#pcfileUploadVideo').change(function (e) {
                                that.uploadVideo(e);
                            });
                            that.uploadCom = null;
                        }
                    })

            },
            uploadAudio: function () {
                var that = this;
                var file = event.target;
                if (file.files.length > 0 && file.files[0].size / (1024 * 1024) > 20) {
                    layer.msg("请上传小于20M的音频", { time: 1500 });
                    return;
                }
                var _type = file.files[0].type;
                if (_type && _type != "audio/mp3") {
                    layer.msg("只支持：mp3格式", { time: 1500 });
                    return;
                }
                $.ajaxFileUpload(
                    {
                        url: '/Upload/UploadVoiceOnly', //用于文件上传的服务器端请求地址
                        secureuri: false, //是否需要安全协议，一般设置为false
                        data: {},
                        fileElementId: "pcfileUploadAudio", //文件上传域的ID
                        dataType: 'json', //返回值类型 一般设置为json
                        timeout: 120 * 1000,
                        success: function (data, status)  //服务器成功响应处理函数
                        {
                            console.log(data, status);
                            if (!data.isok) {
                                layer.msg(data.msg, { time: 1500 });
                                return;
                            }
                            if (that.uploadCom) {
                                if (that.uploadCom.type == "bgaudio") {
                                    that.uploadCom.src = data.Path
                                }
                            }

                        },
                        error: function (data, status, e)//服务器响应失败处理函数
                        {


                        },
                        complete: function () {

                            $('#pcfileUploadAudio').change(function (e) {
                                that.uploadAudio(e);
                            });
                            that.uploadCom = null;
                        }
                    })
            },
            AddPage: function () {

                var _newpage = $.extend(true, {}, pageItem);
                _newpage.name = "新建页";
                if (this.normalpages.length > 0) {
                    _newpage.skin = this.normalpages[0].skin;
                }
                this.pages.push(_newpage);
                this.SortPage();
                var _selindex = this.pages.findLastIndex(function (item) {
                    return (!("def_name" in item) || item.def_name == "");
                });
                this.PickPage(_selindex);

                this.pages[_selindex].coms.push(this.CreateCom("bottomnav"));

            },
            PickPage: function (index) {

                if (this.isSysPage(this.pages[index])) {
                    return;
                }
                this.pages.forEach(function (o, i) {
                    o.sel = false;
                })
                this.pages[index].sel = true;
                this.selPageIndex = index;
                this.selComIndex = -1;

            },
            RemovePage: function (pageIndex) {
                var that = this;
                if (this.normalpages.length == 1) {
                    layer.msg("删除失败，至少需要有一个页面！");
                    return;
                }
                layer.confirm('您确定要删除当前页面吗？删除后该页面将无法恢复，请谨慎操作！', function (index) {
                    //do something

                    that.pages.splice(pageIndex, 1)

                    var _selindex = that.pages.findLastIndex(function (item) {
                        return (!("def_name" in item) || item.def_name == "");
                    });
                    if (_selindex != -1) {
                        that.PickPage(_selindex);
                    }
                    layer.close(index);
                });




            },
            CopyPage: function (index) {
                var _newpage = $.extend(true, {}, this.pages[index]);
                _newpage.sel = false;
                this.pages.push(_newpage);
                this.SortPage();
            },
            PickCom: function (key) {
                console.log(key);
                if (key == "unable") {
                    layer.msg("请升级更高版本才能开启此功能！", {
                        time: 1000
                    });
                    return false;
                }
                var that = this;
                var _selpage = this.selPage();
                if (!_selpage) {
                    layer.msg("请选择要设计的页面！", {
                        time: 1000
                    });
                    return false;
                }
                if (key == "goodlist") {
                    if (this.GetPageComCount('goodlist') == 1) {
                        layer.msg("一个页面只能添加一个“产品列表”组件！");
                        return false;
                    }
                    else if ((this.GetPageComCount() > 0 && this.GetPageComCount('bottomnav') == 0) ||
                        (this.GetPageComCount() > 1 && this.GetPageComCount('bottomnav') == 1)) {
                        layer.msg("“产品列表”组件只能和“底部导航”共存！");
                        return false;
                    }

                }
                else if (key == "newslist") {
                    if (this.GetPageComCount('newslist') == 1) {
                        layer.msg("一个页面只能添加一个“资讯列表”组件！");
                        return false;
                    }
                    else if ((this.GetPageComCount() > 0 && this.GetPageComCount('bottomnav') == 0) ||
                        (this.GetPageComCount() > 1 && this.GetPageComCount('bottomnav') == 1)) {
                        layer.msg("“资讯列表”组件只能和“底部导航”共存！");
                        return false;
                    }

                }


                else if (key == "bottomnav") {
                    if (this.GetPageComCount('bottomnav') == 1) {
                        layer.msg("一个页面只能添加一个“底部导航”组件！");
                        return false;
                    }
                }
                else if (this.GetPageComCount('newslist') == 1) {
                    layer.msg("有“资讯列表”的页面除了“底部导航”不能再添加其他组件！");
                    return false;
                }
                else if (this.GetPageComCount('goodlist') == 1) {
                    layer.msg("有“产品列表”的页面除了“底部导航”不能再添加其他组件！");
                    return false;
                }
                else if (key == "kefu" && this.GetPageComCount('kefu') == 1) {
                    layer.msg("一个页面只能添加一个客服组件！");
                    return false;
                }
                else if (key == "bgaudio" && this.GetPageComCount('bgaudio') >= 1) {
                    layer.msg("一个页面只能添加一个背景音乐组件！");
                    return false;
                }
                else if (key == "makecall" && this.GetPageComCount('makecall') == 1) {
                    layer.msg("一个页面只能添加一个拨打电话组件！");
                    return false;
                }
                else if (key == "form" && this.GetPageComCount('form') == 1) {
                    layer.msg("一个页面只能添加一个表单组件！");
                    return false;
                }
                else if (key == "imgnav" && this.GetPageComCount('imgnav') == 2 && pagetype == 26) {
                    layer.msg("一个页面只能添加一个8个分类导航！");
                    return false;
                } else if (key == "slider" && this.GetPageComCount('slider') == 1 && pagetype == 26) {
                    layer.msg("一个页面只能添加一个轮播图！");
                    return false;
                } else if (key == "good" && this.GetPageComCount('good') == 1 && pagetype == 26) {
                    layer.msg("一个页面只能添加一个产品组件！");
                    return false;
                } else if (key == "share" && this.GetPageComCount('share') >= 1) {
                    layer.msg("一个页面只能添加一个分享转发组件！");
                    return false;
                } else if (key == "contactShopkeeper" && this.GetPageComCount('contactShopkeeper') >= 1) {
                    layer.msg("一个页面只能添加一个联系店主组件！");
                    return false;
                }
                //else if (key == "takeout") {
                //    if (this.GetPageComCount('takeout') == 1) {
                //        layer.msg("一个页面只能添加一个外卖组件！");
                //        return;
                //    }
                //    else if (this.GetPageComCount() > 0) {
                //        layer.msg("外卖组件只能独立存在");
                //        return;
                //    }

                //}
                //else if (this.GetPageComCount('takeout') == 1) {
                //    layer.msg("有外卖组件的页面不能再添加其他组件！");
                //    return;
                //}
                _selpage.coms.push(this.CreateCom(key));
                that.SortPageCom();

                return true;

            },

            //选择全部资讯分类
            PickAllNewsCat: function () {
                if (!this.selCom())
                    return;
                console.log(this.selCom().pickAllNewsCat);
                for (var i = 0; i < this.news2typelist.length; i++) {
                    this.news2typelist[i].sel = this.selCom().pickAllNewsCat;
                }
            },
            PickNewsCatOK: function () {
                if (!this.selCom())
                    return;
                var _selcom = this.selCom();
                _selcom.newsCat = [];
                for (var i = 0; i < this.news2typelist.length; i++) {
                    this.news2typelist[i].sel && _selcom.newsCat.push(this.news2typelist[i]);
                }
                console.log(_selcom.newsCat);
                _selcom.newsCat = _selcom.newsCat.sort(sortorder);//按照排序字段排序
                $("#AddNewsCatModal").modal("hide");
            },
            ShowAddNewsCatModal: function () {
                $("#AddNewsCatModal").modal("show");
                var that = this;
                if (this.selCom()) {
                    that.news2typelist.forEach(function (item) {
                        item.sel = false;
                    });

                    var com = this.selCom();
                    if (com.type == "newslist" && com.newsCat.length > 0 && that.news2typelist.length > 0) {


                        for (var i = 0; i < that.news2typelist.length; i++) {
                            for (var j = 0; j < com.newsCat.length; j++) {
                                if (that.news2typelist[i].id == com.newsCat[j].id) {
                                    that.news2typelist[i].sel = true;
                                }
                            }
                        }
                    }
                }
            },
            //删除选择的资讯分类
            RemoveNewsCatItem: function (index) {
                if (!this.selCom())
                    return;
                var _selcom = this.selCom();
                _selcom.newsCat.splice(index, 1);
            },
            ShowAddGoodCatModal: function () {
                $("#AddGoodCatModal").modal("show");
                var that = this;
                if (this.selCom()) {
                    that.goodtypelist.forEach(function (item) {
                        item.sel = false;
                    });

                    var com = this.selCom();
                    if (com.type == "goodlist" && com.goodCat.length > 0 && that.goodtypelist.length > 0) {


                        for (var i = 0; i < that.goodtypelist.length; i++) {
                            for (var j = 0; j < com.goodCat.length; j++) {
                                if (that.goodtypelist[i].id == com.goodCat[j].id) {
                                    that.goodtypelist[i].sel = true;
                                }
                            }
                        }
                    }
                }
            },
           
            //选择全部产品分类
            PickAllGoodCat: function () {
                if (!this.selCom())
                    return;
                console.log(this.selCom().pickallgoodcat);
                for (var i = 0; i < this.goodtypelist.length; i++) {
                    this.goodtypelist[i].sel = this.selCom().pickallgoodcat;
                }
            },
            PickGoodCatOK: function () {
                if (!this.selCom())
                    return;
                var _selcom = this.selCom();
                _selcom.goodCat = [];
                for (var i = 0; i < this.goodtypelist.length; i++) {
                    this.goodtypelist[i].sel && _selcom.goodCat.push(this.goodtypelist[i]);
                }
                console.log(_selcom.goodCat);
                _selcom.goodCat = _selcom.goodCat.sort(sortorder);//按照排序字段排序
                $("#AddGoodCatModal").modal("hide");
            },
            PickGoodExtCatOK: function () {
                if (!this.selCom())
                    return;
                var _selcom = this.selCom();
                _selcom.goodExtCat = [];
                var selItems = this.goodexttypeslist.filter(function (item) {
                    return item.sel;
                });

                _selcom.goodExtCat = selItems;
                $("#AddGoodExtTypes").modal("hide");
            },
            PickCutPrice: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    item.sel = !item.sel
                }
            },
            PickCutPriceOK: function () {
                var sellist = this.cutpriceModel.list.filter(function (_item) {
                    return _item.sel;
                });
                var _selcom = this.selCom();
                if (_selcom) {
                    if (_selcom.items.length > 0) {
                        for (var i = 0; i < sellist.length; i++) {
                            var _exists = _selcom.items.find(function (ele) {
                                return ele.id == sellist[i].Id
                            });
                            if (_exists == undefined) {
                                _selcom.items.push({
                                    id: sellist[i].Id,
                                    name: sellist[i].BName,
                                    img: sellist[i].ImgUrl
                                });
                            }
                        }
                    }
                    else {
                        _selcom.items = _selcom.items.concat(sellist.map(function (item) {
                            return {
                                id: item.Id,
                                name: item.BName,
                                img: item.ImgUrl
                            }
                        }));
                    }
                }


                this.cutpriceModel.list.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#AddCutPriceModal").modal("hide");
            },

            PickJoinGroup: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    item.sel = !item.sel
                }
            },
            PickJoinGroupOK: function () {
                var sellist = this.joingroupModel.list.filter(function (_item) {
                    return _item.sel;
                });
                var _selcom = this.selCom();
                if (_selcom) {
                    if (_selcom.items.length > 0) {
                        for (var i = 0; i < sellist.length; i++) {
                            var _exists = _selcom.items.find(function (ele) {
                                return ele.id == sellist[i].Id
                            });
                            if (_exists == undefined) {
                                _selcom.items.push({
                                    id: sellist[i].Id,
                                    name: sellist[i].GroupName,
                                    img: sellist[i].ImgUrl
                                });
                            }
                        }
                    }
                    else {
                        _selcom.items = _selcom.items.concat(sellist.map(function (item) {
                            return {
                                id: item.Id,
                                name: item.GroupName,
                                img: item.ImgUrl
                            }
                        }));
                    }
                }


                this.joingroupModel.list.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#AddJoinGroupModal").modal("hide");
            },
            PickEntJoinGroup: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    item.sel = !item.sel
                }
            },
            PickEntJoinGroupOK: function () {
                var sellist = this.entjoingroupModel.list.filter(function (_item) {
                    return _item.sel;
                });
                var _selcom = this.selCom();
                if (_selcom) {
                    if (_selcom.items.length > 0) {
                        for (var i = 0; i < sellist.length; i++) {
                            var _exists = _selcom.items.find(function (ele) {
                                return ele.id == sellist[i].Id
                            });
                            if (_exists == undefined) {
                                _selcom.items.push({
                                    id: sellist[i].EntGoodsId,
                                    name: sellist[i].Name,
                                    img: sellist[i].ImgUrl,
                                    GroupPrice: sellist[i].GroupPriceStr,
                                    SaleNum: sellist[i].SaleNum
                                });
                            }
                        }
                    }
                    else {
                        _selcom.items = _selcom.items.concat(sellist.map(function (item) {
                            return {
                                id: item.EntGoodsId,
                                name: item.Name,
                                img: item.ImgUrl,
                                GroupPrice: item.GroupPriceStr,
                                SaleNum: item.SaleNum
                            }
                        }));
                    }
                }


                this.entjoingroupModel.list.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#AddEntJoinGroupModal").modal("hide");
            },
            GetFilterTypes: function (comitem) {
                if (comitem && !comitem.isShowGoodCatNav && comitem.isShowFilter) {
                    if (comitem.isShowGoodPriceSort) {
                        return comitem.goodExtCat.slice(0, 2);
                    }
                    else {
                        return comitem.goodExtCat.slice(0, 3);
                    }
                }
                else
                    return [];
            },
            //删除选择的产品分类
            RemoveGoodCatItem: function (index) {
                if (!this.selCom())
                    return;
                var _selcom = this.selCom();
                _selcom.goodCat.splice(index, 1);
            },
            //删除扩展分类
            RemoveGoodCatExtItem: function (index) {
                if (!this.selCom())
                    return;
                var _selcom = this.selCom();
                _selcom.goodExtCat.splice(index, 1);
            },
            GetPageCom: function (comname) {
                if (!this.selPage())
                    return false;
                return this.selPage().coms.find(function (obj) {
                    return obj.type == comname;
                });
            },
            GetPageComCount: function (comname) {
                if (!this.selPage())
                    return 0;
                comname = comname || "";
                if (comname == "") {
                    return this.selPage().coms.length;
                }
                return this.selPage().coms.filter(function (obj) {
                    return obj.type == comname;

                }).length;
            },
            GetPageComIndex: function (comname) {
                if (!this.selPage())
                    return -1;
                return this.selPage().coms.findIndex(function (obj) {
                    return obj.type == comname;
                });
            },
            //删除组件
            RemovePageComByIndex: function (index) {
                if (index < 0)
                    return;
                if (this.selPage() && this.selPage().coms.length > 0) {
                    this.selPage().coms.splice(index, 1);
                    //如果删除的是当前选中的组件
                    if (index == this.selPage().selComIndex) {
                        this.selPage().selComIndex = -1;
                    }

                    //更新 [商品详情推送开关]
                    this.DetectionWxShopMessageState();
                }

            },
            EditCom: function (index, event) {
                var that = this;
                this.selPage().selComIndex = index;
                this.selComIndex = index;


                if (this.selCom().type == "richtxt") {
                    setTimeout(function () {
                        that.BuildEditor();
                    }, 500);
                }
                var nodeele = $(event.target).closest(".app-field");
                //$(".app-sidebar-wrap").css({ "margin-top": nodeele.position().top + 'px' });

            },
            /************************底部导航******************************/
            RemoveBottomNavByIndex: function (index) {
                if (this.selCom()) {
                    var _current_bottomnav = this.selCom();
                    if (_current_bottomnav.navlist.length <= 2) {
                        layer.msg("最少要有两个导航", { time: 1500 });
                        return;
                    }
                    _current_bottomnav.navlist.splice(index, 1);
                }
            },
            AddButtomNavItem: function () {
                if (this.selCom()) {
                    var _current_bottomnav = this.selCom();
                    if (_current_bottomnav.navlist.length >= MAX_BOTTOMNAVITEMCOUNT) {
                        layer.msg("最多只能设置" + MAX_BOTTOMNAVITEMCOUNT + "个导航", { time: 1500 });
                        return;
                    }
                    var _newitem = $.extend(true, {}, navItem);
                    _newitem.name = "新建项";
                    _newitem.img = "";
                    _newitem.url = -1;
                    _newitem.icon = "icon-news2";
                    _current_bottomnav.navlist.push(_newitem);



                }
            },
            /************************轮播图******************************/
            AddSliderItem: function () {
                if (this.selCom()) {
                    var _currentCom = this.selCom();
                    if (_currentCom.items.length >= 10) {
                        layer.msg("最多只能设置10张轮播图", { time: 1500 });
                        return;
                    }
                    var _newitem = $.extend(true, {}, sliderItem);
                    _newitem.url = -1,
                        _currentCom.items.push(_newitem);
                }
            },
            RemoveSliderByIndex: function (index) {
                if (this.selCom()) {
                    var _currentCom = this.selCom();
                    if (_currentCom.items.length <= 1) {
                        layer.msg("最少要有1张轮播图", { time: 1500 });
                        return;
                    }
                    _currentCom.items.splice(index, 1);
                }
            },
            /************************图片导航******************************/
            RemoveImgNavByIndex: function (index) {
                if (this.selCom()) {
                    var _current_com = this.selCom();
                    if (_current_com.navlist.length <= 2 && pagetype != 26) {
                        layer.msg("最少要有两个导航", { time: 1500 });
                        return;
                    }
                    if (_current_com.navlist.length <= 1 && pagetype == 26) {
                        this.RemovePageComByIndex(this.selPage().selComIndex)
                        return;
                    }

                    _current_com.navlist.splice(index, 1);
                }
            },
            AddImgNavItem: function () {
                if (this.selCom()) {
                    var _current_com = this.selCom();
                    if (_current_com.navlist.length >= 4 && pagetype != 26) {
                        layer.msg("最多只能设置4个导航", { time: 1500 });
                        return;
                    }
                    var _newitem = $.extend(true, {}, navItem);
                    _newitem.name = "新建项";
                    _newitem.img = ""
                    _newitem.url = -1;
                    _current_com.navlist.push(_newitem);
                }
            },
            /************************产品*****************************/
            GetGoodTypeName: function (typeid) {
                if (couponstypelist.length > 0) {
                    var _item = couponstypelist.find(function (item) {
                        item.id = typeid
                    })
                    if (_item != undefined) {
                        return _item.name;
                    }
                }

            },
            PickGood: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    if (_selcom.type == 'img' || _selcom.type == "slider" || _selcom.type == "imgnav" || _selcom.type == "magicCube") {
                        var sellist = this.goodModel.list.forEach(function (_item) {
                            _item.sel = false;
                        });
                        this.goodModel.list[index].sel = true;
                    }
                    else {
                        item.sel = !item.sel
                    }
                }
            },
            PickGoodOK: function () {
                var sellist = this.goodModel.list.filter(function (_item) {
                    return _item.sel;
                });


                var _selcom = this.selCom();
                if (_selcom) {
                    if (_selcom.type == "img" || _selcom.type == "slider" || _selcom.type == "imgnav" || _selcom.type == "magicCube" || _selcom.type == "bottomnav") {
                        if (sellist.length > 0) {
                            var selitem = sellist[0];
                            var _citem = null;

                            var _citem = _selcom.type == "img" ? _selcom : (_selcom.type == "imgnav" || _selcom.type == "bottomnav") ? _selcom.navlist[_selcom.editindex] : _selcom.items[_selcom.editindex];

                            _citem.items = [];
                            Vue.set(_citem.items, 0, {
                                id: selitem.id,
                                name: selitem.name || "",
                                showprice: selitem.showprice,
                                plabelstr_array: selitem.plabelstr_array,
                                img: selitem.img || "",
                                price: selitem.price,
                                unit: selitem.unit
                            });
                        }
                    }
                    else {
                        if (_selcom.items.length > 0) {
                            for (var i = 0; i < sellist.length; i++) {
                                var _exists = _selcom.items.find(function (ele) {
                                    return ele.id == sellist[i].id
                                });
                                if (_exists == undefined) {
                                    _selcom.items.push({
                                        id: sellist[i].id,
                                        name: sellist[i].name || "",
                                        showprice: sellist[i].showprice,
                                        plabelstr_array: sellist[i].plabelstr_array,
                                        img: sellist[i].img || "",
                                        price: sellist[i].price,
                                        unit: sellist[i].unit
                                    });
                                }
                            }
                        }
                        else {
                            _selcom.items = _selcom.items.concat(sellist.map(function (item) {
                                return {
                                    id: item.id,
                                    name: item.name || "",
                                    showprice: item.showprice,
                                    plabelstr_array: item.plabelstr_array,
                                    img: item.img,
                                    price: item.price,
                                    unit: item.unit
                                }
                            }));
                        }
                    }
                }


                this.goodModel.list.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#AddGoodModal").modal("hide");

            },

            PickGoodType: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    var sellist = this.goodtypelist.forEach(function (_item) {
                        _item.sel = false;
                    });
                    this.goodtypelist[index].sel = true;
                }
            },
            PickGoodMinType: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    var sellist = this.goodmintypelist.forEach(function (_item) {
                        _item.sel = false;
                    });
                    this.goodmintypelist[index].sel = true;
                }
            },
            PickEntGroup: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    var sellist = this.entjoingroupModel.list.forEach(function (_item) {
                        _item.sel = false;
                    });
                    this.entjoingroupModel.list[index].sel = true;
                }
            },
            PickLinkCutPrice: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    var sellist = this.cutpriceModel.list.forEach(function (_item) {
                        _item.sel = false;
                    });
                    this.cutpriceModel.list[index].sel = true;
                }
            },
            PickLinkFlashDealPrice: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    var sellist = this.flashDealsModel.list.forEach(function (_item) {
                        _item.sel = false;
                    });
                    this.flashDealsModel.list[index].sel = true;
                }
            },
            //选择团购
            PickLinkJoinGroup: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    var sellist = this.joingroupModel.list.forEach(function (_item) {
                        _item.sel = false;
                    });
                    this.joingroupModel.list[index].sel = true;
                }

            },
            //选择产品分类
            PickGoodTypeOK: function () {
                var sellist = this.goodtypelist.filter(function (_item) {
                    return _item.sel;
                });

                var _selcom = this.selCom();
                if (_selcom) {
                    var _citem = _selcom.type == "img" ? _selcom : (_selcom.type == "imgnav" || _selcom.type == "bottomnav") ? _selcom.navlist[_selcom.editindex] : _selcom.items[_selcom.editindex];
                    if (sellist.length > 0) {
                        var selitem = sellist[0];
                        if (_citem.itemstype == undefined)
                            _citem.itemstype = [];

                        Vue.set(_citem.itemstype, 0, {
                            id: selitem.id,
                            name: selitem.name || "",
                        });
                        _citem.url = selitem.id;
                    }
                }

                this.goodtypelist.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#AddGoodTypeModal").modal("hide");

            },
            PickGoodMinTypeOK: function () {
                var sellist = this.goodmintypelist.filter(function (_item) {
                    return _item.sel;
                });

                var _selcom = this.selCom();
                if (_selcom) {
                    var _citem = _selcom.type == "img" ? _selcom : (_selcom.type == "imgnav" || _selcom.type == "bottomnav") ? _selcom.navlist[_selcom.editindex] : _selcom.items[_selcom.editindex];
                    if (sellist.length > 0) {
                        var selitem = sellist[0];
                        if (_citem.itemstype == undefined)
                            _citem.itemstype = [];

                        Vue.set(_citem.itemstype, 0, {
                            id: selitem.id,
                            name: selitem.name || "",
                        });
                        _citem.url = selitem.id;
                    }
                }

                this.goodmintypelist.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#AddGoodMinTypeModal").modal("hide");

            },
            //确定选择的拼团
            PickEntGroupOK: function () {
                var sellist = this.entjoingroupModel.list.filter(function (_item) {
                    return _item.sel;
                });

                var _selcom = this.selCom();
                if (_selcom) {
                    var _citem = _selcom.type == "img" ? _selcom : (_selcom.type == "imgnav" || _selcom.type == "bottomnav") ? _selcom.navlist[_selcom.editindex] : _selcom.items[_selcom.editindex];
                    if (sellist.length > 0) {
                        var selitem = sellist[0];
                        if (_citem.itemstype == undefined)
                            _citem.itemstype = [];

                        Vue.set(_citem.itemstype, 0, {
                            id: selitem.EntGoodsId,
                            name: selitem.Name || "",
                        });
                        _citem.url = selitem.Id;
                    }
                }

                this.goodtypelist.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#AddEntGroupModal").modal("hide");
            },
            //确定选择的砍价
            PickLinkCutPriceOK: function () {
                var sellist = this.cutpriceModel.list.filter(function (_item) {
                    return _item.sel;
                });

                var _selcom = this.selCom();
                if (_selcom) {
                    var _citem = _selcom.type == "img" ? _selcom : (_selcom.type == "imgnav" || _selcom.type == "bottomnav") ? _selcom.navlist[_selcom.editindex] : _selcom.items[_selcom.editindex];
                    if (sellist.length > 0) {
                        var selitem = sellist[0];
                        if (_citem.itemstype == undefined)
                            _citem.itemstype = [];

                        Vue.set(_citem.itemstype, 0, {
                            id: selitem.Id,
                            name: selitem.BName || "",
                        });
                        _citem.url = selitem.Id;
                    }
                }

                this.cutpriceModel.list.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#LinkAddCutPriceModal").modal("hide");
            },
            //确定选择的砍价
            PickLinkFlashDealsOK: function () {
                var sellist = this.flashDealsModel.list.filter(function (_item) {
                    return _item.sel;
                });

                var _selcom = this.selCom();
                if (_selcom) {
                    var _citem = _selcom.type == "img" ? _selcom : (_selcom.type == "imgnav" || _selcom.type == "bottomnav") ? _selcom.navlist[_selcom.editindex] : _selcom.items[_selcom.editindex];
                    if (sellist.length > 0) {
                        var selitem = sellist[0];
                        if (_citem.itemstype == undefined)
                            _citem.itemstype = [];

                        Vue.set(_citem.itemstype, 0, {
                            id: selitem.Id,
                            name: selitem.Title || "",
                        });
                        _citem.url = selitem.Id;
                    }
                }

                this.flashDealsModel.list.forEach(function (o, i) {
                    if (sellist[0]&& o.Id != sellist[0].Id) {
                        o.sel = false;
                    }
                  
                });
                $("#LinkAddFlashDealsModal").modal("hide");
            },
            GetFlashDeals: function () {
                var that = this;
                return $.get("/FlashDeal/Get", { appId: aid,pageIndex: that.current_page, pageSize: 10 }, function (data) {
                   // console.log(data);
                    that.flashDealsModel.list = data.dataObj.deals;
                    that.pagercount = data.dataObj.pageCount;

                }, 'json');
            },
            //格式化倒计时（距离活动开始时间）
            getRemainTime: function (faultDate, completeTime) {
                var days = 0;
                var hours = 0;
                var minutes = 0
                var stime = Date.parse(faultDate);
                var etime = Date.parse(completeTime);
                if (new Date(stime).getTime() < new Date(etime).getTime()) {
                    var usedTime = etime - stime;  //两个时间戳相差的毫秒数  
                    days = Math.floor(usedTime / (24 * 3600 * 1000));
                    //计算出小时数  
                    var leave1 = usedTime % (24 * 3600 * 1000);    //计算天数后剩余的毫秒数  
                    hours = Math.floor(leave1 / (3600 * 1000));
                    //计算相差分钟数  
                    var leave2 = leave1 % (3600 * 1000);        //计算小时数后剩余的毫秒数  
                    minutes = Math.floor(leave2 / (60 * 1000));
                }
                var time = String.raw`${days}天${hours}时${minutes > 0 ? minutes +1: 0}分`;
                return time;
            },
            //获取活动倒计时
            getCountDown: function (beginString) {
                return this.getRemainTime(new Date(Date.now()), new Date(beginString));
            },
            //确定选择的团购
            PickLinkJoinGroupOK: function () {
                var sellist = this.joingroupModel.list.filter(function (_item) {
                    return _item.sel;
                });

                var _selcom = this.selCom();
                if (_selcom) {
                    var _citem = _selcom.type == "img" ? _selcom : (_selcom.type == "imgnav" || _selcom.type == "bottomnav") ? _selcom.navlist[_selcom.editindex] : _selcom.items[_selcom.editindex];
                    if (sellist.length > 0) {
                        var selitem = sellist[0];
                        if (_citem.itemstype == undefined)
                            _citem.itemstype = [];

                        Vue.set(_citem.itemstype, 0, {
                            id: selitem.Id,
                            name: selitem.GroupName || "",
                        });
                        _citem.url = selitem.Id;
                    }
                }

                this.joingroupModel.list.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#LinkAddGroupModal").modal("hide");
            },




            /************************优惠券*****************************/
            PickCoupons: function (item, index) {
                var _selcom = this.selCom();
                if (_selcom) {
                    item.sel = !item.sel
                }
            },
            PickCouponsOK: function () {
                var sellist = this.couponsModel.list.filter(function (_item) {
                    return _item.sel;
                });


                var _selcom = this.selCom();
                if (_selcom) {
                    if (_selcom.items.length > 0) {
                        for (var i = 0; i < sellist.length; i++) {
                            var _exists = _selcom.items.find(function (ele) {
                                return ele.id == sellist[i].Id
                            });
                            if (_exists == undefined) {
                                _selcom.items.push({
                                    id: sellist[i].Id,
                                    CouponName: sellist[i].CouponName || "",
                                    LimitMoney: sellist[i].LimitMoneyStr,
                                    CouponWay: sellist[i].CouponWay,
                                    Money: sellist[i].MoneyStr,
                                    StartUseTime: sellist[i].StartUseTimeStr,
                                    EndUseTime: sellist[i].EndUseTimeStr,
                                    ValDay: sellist[i].ValDay,
                                    ValType: sellist[i].ValType,
                                    sel: sellist[i].sel
                                });
                            }
                        };
                    }
                    else {
                        _selcom.items = _selcom.items.concat(sellist.map(function (item) {
                            return {
                                id: item.Id,
                                CouponName: item.CouponName || "",
                                LimitMoney: item.LimitMoneyStr,
                                Money: item.MoneyStr,
                                CouponWay: item.CouponWay,
                                StartUseTime: item.StartUseTimeStr,
                                EndUseTime: item.EndUseTimeStr,
                                ValDay: item.ValDay,
                                ValType: item.ValType,
                                sel: item.sel
                            }
                        }));

                    }
                }


                this.couponsModel.list.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#AddCouponsModal").modal("hide");

            },
            /************************表单*****************************/
            AddRadioItem: function (formitem) {
                if (formitem.items.length >= 10) {
                    layer.msg("最多只能设置10个选项");
                    return;
                }
                var _newradio_item = $.extend(true, {}, formMutiSelectItem);
                formitem.items.push(_newradio_item);
            },
            AddFormItem: function (com, type) {
                if (com.items.length >= 10) {
                    layer.msg("最多只能设置10个控件");
                    return;
                }
                var _item = $.extend(true, {}, formItem[type]);
                com.items.push(_item);
            },
            /************************地图*****************************/
            ChooseLocationOK: function () {
                var _location = mapframe.window.document.getElementById("poi_cur").value;
                var _zoom = mapframe.window.document.getElementById("zoomlevel").innerHTML;
                if (_location == "") {
                    layer.msg("您还未选择地理位置！");
                    return;
                }
                _location = _location.split(",");
                var _selMap = this.selCom();
                _selMap.latitude = parseFloat(_location[0]);
                _selMap.longitude = parseFloat(_location[1]);
                _selMap.scale = parseInt(_zoom) || 15;

                console.log(_location);
                $("#ChooseLocationModal").modal("hide");
            },
            /************************资讯*****************************/
            ChangeNewsListByType: function (typeid) {
                if (this.$refs.newspager) {
                    this.$refs.newspager.typeid = typeid;
                }
            },
            ChooseNewsOK: function () {
                var sellist = this.newsModel.list.filter(function (_item) {
                    return _item.sel;
                });
                var _selcom = this.selCom();
                if (_selcom) {
                    if (_selcom.list.length > 0) {
                        for (var i = 0; i < sellist.length; i++) {
                            var _exists = _selcom.list.find(function (ele) {
                                return ele.id == sellist[i].id
                            });
                            if (_exists == undefined) {
                                _selcom.list.push({
                                    id: sellist[i].id,
                                    title: sellist[i].title || ""
                                });
                            }
                        }


                    }
                    else {
                        _selcom.list = _selcom.list.concat(sellist.map(function (item) {
                            return {
                                id: item.id,
                                title: item.title,
                            }
                        }));
                    }
                }

                this.newsModel.list.forEach(function (o, i) {
                    o.sel = false;
                });
                $("#ChooseNewsModal").modal("hide");
            },

            RemoveNewsItem: function (index) {
                if (!this.selCom())
                    return;
                var _selcom = this.selCom();
                _selcom.list.splice(index, 1);
            },

            PickNewsContent: function (typeid) {
                $("#ChooseNewsModal").modal("show");
                this.ChangeNewsListByType(typeid);

            },
            /************************魔方图片*****************************/
            selectedStyle: function (index) {
                var _selcom = this.selCom();
                _selcom.style = index;
                //for (var i = 0; i < this.magicCubeSetType.length; i++) {
                //    this.magicCubeSetType[i].styleBorder = false;
                //}
                this.magicCubeSetType[index].styleBorder = index;
            },
            selectPages: function (index) {
                var _selcom = this.selCom();
                _selcom.pagesIndex = index;
                if (_selcom.items[index].img == "") {
                    layer.msg("请先上传图片")
                }

            },
            /************************保存*****************************/
            savepage: function () {
                if (this.ispost) {
                    layer.msg("数据提交中，请勿重复提交！", { time: 1000 });
                    return;
                }
                if (this.normalpages.length <= 0) {
                    layer.msg("至少要有一个页面！");
                    return;
                }

                for (var i = 0; i < this.normalpages.length; i++) {
                    var _cpage = this.normalpages[i];
                    if (_cpage.coms.length == 0) {
                        layer.msg("页面：“" + _cpage.name + "”至少要有一个组件！");
                        return;
                    }
                    if ($.trim(_cpage.name).length == 0) {
                        layer.msg("页面名称不能为空！");
                        return;
                    }
                    for (var j = 0; j < _cpage.coms.length; j++) {
                        var _ccom = _cpage.coms[j];
                        if (_ccom.type == "bottomnav") {
                            if (_ccom.navlist.length < 2) {

                            }
                        }
                        else if (_ccom.type == "video") {
                            if (_ccom.src == "" || _ccom.src.indexOf("http") == -1) {
                                layer.msg("第" + (i + 1) + "个页面的第" + (j + 1) + "个组件【视频组件】没有设置视频！");
                                return;
                            }
                            if (_ccom.src.lastIndexOf('.') == -1) {
                                layer.msg("视频组件只支持.mp4！");
                                return;
                            }
                            var _videoext = _ccom.src.substr(_ccom.src.lastIndexOf('.'));
                            if (_ccom.src.lastIndexOf('.') == -1 || (_videoext != ".m3u8" && _videoext != ".mp4")) {
                                layer.msg("视频组件只支持.mp4！");
                                return;
                            }
                        }
                        else if (_ccom.type == "form") {
                            if ($.trim(_ccom.title).length > 20) {
                                layer.msg("表单名称不能超过20个字！");
                                return;
                            }
                            for (var fi = 0; fi < _ccom.items.length; fi++) {
                                var _cformitem = _ccom.items[fi];
                                if (_cformitem.name.length > 20 || _cformitem.name.length <= 0) {
                                    layer.msg("表单控件名称不能为空且不能超过20个字！");
                                    return;
                                }
                                if (_cformitem.type == "radio" && (_cformitem.items.length > 10 || _cformitem.items.length == 0)) {
                                    layer.msg("单选项“" + _cformitem.name + "”不能为空且不能超过10个！");
                                    return;
                                }
                            }
                        }
                        else if (_ccom.type == "news") {
                            if ($.trim(_ccom.title).length > 10 || $.trim(_ccom.title).length <= 0) {
                                layer.msg("资讯组件标题不能为空，且不能超过10个字！");
                                return;
                            }
                            if (_ccom.typeid == 0) {
                                layer.msg("请选择资讯组件的数据来源！");
                                return;
                            }
                            if (_ccom.listmode == "pick" && _ccom.list.length <= 0) {
                                layer.msg("第" + (i + 1) + "个页面的第" + (j + 1) + "个组件【内容资讯】选择了指定数量，但没有添加资讯，保存失败！");
                                return;
                            }
                        }
                        else if (_ccom.type == "live") {
                            if ($.trim(_ccom.vzliveurl).length <= 0) {
                                layer.msg("第" + (i + 1) + "个页面的第" + (j + 1) + "个组件【直播组件】没有设置直播地址，保存失败！");
                                return;
                            }
                        }
                        else if (_ccom.type == "img") {
                            if (_ccom.urltype == "p" && _ccom.items.length == 0) {
                                layer.msg("第" + (i + 1) + "个页面的第" + (j + 1) + "个组件【图片组件】选择了跳转到产品，但是没有选择产品！");
                                return;
                            }
                        }
                        else if (_ccom.type == "imgnav") {

                        }
                        else if (_ccom.type == "good") {
                            if (_ccom.items.length == 0) {
                                layer.msg("第" + (i + 1) + "个页面的第" + (j + 1) + "个组件【产品组件】没有选择产品！");
                                return;
                            }
                        }
                        else if (_ccom.type == "joingroup") {
                            if (_ccom.items.length == 0) {
                                layer.msg("第" + (i + 1) + "个页面的第" + (j + 1) + "个组件【拼团】没有选择拼团活动！");
                                return;
                            }
                        }
                        else if (_ccom.type == "cutprice") {
                            if (_ccom.items.length == 0) {
                                layer.msg("第" + (i + 1) + "个页面的第" + (j + 1) + "个组件【砍价】没有选择砍价活动！");
                                return;
                            }
                        }
                    }
                }

                var syncmainsite = this.syncmainsite ? 1 : 0;//同步小未案例
                var saveAlert = _layer.confirm('确定保存数据？', {
                    btn: ['确定', '取消'] //按钮
                }, function () {
                    //var pageModels = pagedata.pages;
                    //pageModels.forEach(function (pageitem) {
                    //    pageitem.coms = JSON.parse(pageitems.coms);
                    //})
                    $.post("/MiappEnterprise/pageset", { appId: aid, pagetype: pagetype, storeId: storeId, templateid: templateid, act: "save", pages: JSON.stringify(pagedata.pages), syncmainsite: syncmainsite, extraConfig: JSON.stringify(extraConfig), MeConfigJson: JSON.stringify(meConfig) }, null, "json")
                        .then(function (data) {
                            _layer.closeAll();
                            if (data.isok) {
                                templateid = 0;
                                //parent.window.location.reload();
                            }

                            layer.msg(data.msg, { time: 2000 });


                        })
                        .fail(function (data) {
                            layer.alert(data.msg);
                        });
                }, function () {
                });

            },
            /********初始化上传组件*********/
            initUploader: function (type) {
                var that = this;
                var options = {
                    browse_button: 'browse_file_' + type,
                    url: 'http://oss.aliyuncs.com',
                    flash_swf_url: 'http://j.vzan.cc/dz/content/aliosslib/lib/lib/plupload-2.1.2/js/Moxie.swf',
                    silverlight_xap_url: 'http://j.vzan.cc/dz/content/aliosslib/lib/lib/plupload-2.1.2/js/Moxie.xap',
                    filters: {

                    },
                    multi_selection: false

                };
                switch (type) {
                    case 'img':

                        options.filters = {

                            mime_types: [
                                {
                                    title: "图片文件",
                                    extensions: "jpg,jpeg,gif,png"
                                },
                            ],
                            //max_file_size: '5mb'
                        }
                        break;
                    //case 'magicCube':

                    //    options.filters = {

                    //        mime_types: [
                    //            {
                    //                title: "图片文件",
                    //                extensions: "jpg,jpeg,gif,png"
                    //            },
                    //        ],
                    //        //max_file_size: '5mb'
                    //    }
                    //    break;
                    case 'video':
                        options.filters = {

                            mime_types: [ //只允许上传图片和zip文件
                                {
                                    title: "视频文件",
                                    extensions: "mp4,m3u8",
                                },
                            ],
                            //max_file_size: '20mb'
                        }

                        break;
                    case 'audio':
                        options.filters = {

                            mime_types: [ //只允许上传图片和zip文件
                                {
                                    title: "音频文件",
                                    extensions: "mp3",
                                },
                            ],
                            //max_file_size: '10mb'
                        }


                        break;
                }
                //如果还有当前类型的上传组件实例的话则先把其销毁
                if (that.uploader[type].instance) {
                    console.log('destroy uploader :' + type);
                    that.uploader[type].instance.destroy();
                }
                that.uploader[type].instance = new plupload.Uploader(options);

                that.uploader[type].instance.init();

                that.uploader[type].instance.bind('BeforeUpload', function (d, f) {


                    //取到文件后缀名

                    var fileName = f.name;
                    var ext = fileName.substring(fileName.lastIndexOf('.') + 1, fileName.length).toLocaleLowerCase();

                    //转成Mb
                    var fileSize = f.size / 1024 / 1024;
                    var flag = true;

                    switch (type) {
                        case 'img':
                            if (!['jpg', 'png', 'gif', 'jpeg'].contains(ext)) {
                                layer.msg('请上传正确的图片格式，后缀名可为：jpg,png,gif,jpeg');
                                flag = false;
                            }
                            if (fileSize > MAX_IMG_SIZE) {
                                layer.msg('文件最大支持 ' + MAX_IMG_DSIZE + 'Mb');
                                flag = false;

                            }
                            break;
                        case 'audio':
                            if (!['mp3'].contains(ext)) {
                                layer.msg('请上传正确的音频格式文件，后缀名可为：mp3');
                                flag = false;

                            }
                            if (fileSize > MAX_AUDIO_SIZE) {
                                layer.msg('文件最大支持 ' + MAX_AUDIO_SIZE + 'Mb');
                                flag = false;

                            }
                            break;
                        case 'video':
                            if (!['mp4', 'm3u8'].contains(ext)) {
                                layer.msg('请上传正确的视频格式文件，后缀名可为：mp4,m3u8');
                                flag = false;

                            }
                            if (fileSize > MAX_VIDEO_SIZE) {
                                layer.msg('文件最大支持 ' + MAX_VIDEO_SIZE + 'Mb');
                                flag = false;
                            }
                            break;
                    }
                    //如果在上传中遇到文件类型和文件大小不正确的错误时重新初始上传组件
                    if (!flag) {
                        that.initUploader(type);
                    }

                    that.uploader.token.key += '.' + ext;
                    that.uploader[type].instance.setOption(that.uploader.token);
                });

                that.uploader[type].instance.bind('FilesAdded', function (uploader, files) {
                    $('#start_upload_' + type).click();
                });
                that.uploader[type].instance.bind('UploadProgress', function (uploader, file) {

                    that.uploader[type].element.innerHTML = '上传进度 - ' + file.percent + "%";
                });
                that.uploader[type].instance.bind('UploadComplete', function (d, f, e) {
                    that.uploader[type].element.innerHTML = getBtnHTML(type);
                    //console.log(that.uploader.filePath);
                    if (that.uploadCom) {
                        var fileName = f[0].name;
                        var fileExt = fileName.substring(fileName.lastIndexOf('.') + 1, fileName.length);
                        var filePath = that.uploader[type].filePath + '.' + fileExt;
                        //一个组件内部子元素索引大于0的只有图片组件
                        if (that.uploadIndex != -1) {
                            if (that.uploadCom[that.uploadIndex].imgurl != undefined) {
                                that.uploadCom[that.uploadIndex].imgurl = filePath;
                            } else if (that.uploadCom[that.uploadIndex].img != undefined) {
                                that.uploadCom[that.uploadIndex].img = filePath;
                            }
                        } else {
                            if (that.uploadCom.type == "bgaudio") {
                                that.uploadCom.src = filePath;
                            }
                            else if (that.uploadCom.type == 'img') {
                                if (that.uploadCom.imgurl != undefined) {
                                    that.uploadCom.imgurl = filePath;
                                } else if (that.uploadCom.img != undefined) {
                                    that.uploadCom.img = filePath;
                                }
                            }
                            else if (that.uploadCom.type == 'magicCube') {
                                if (that.uploadCom.imgurl != undefined) {
                                    that.uploadCom.imgurl.push(filePath);
                                } else if (that.uploadCom.img != undefined) {
                                    that.uploadCom.img = filePath;
                                }
                            }
                            else if (that.uploadCom.type == 'kefu') {
                                Vue.set(that.uploadCom, "img", filePath);
                            }
                            else if (that.uploadCom.type == 'live') {
                                Vue.set(that.uploadCom, "img", filePath);
                            }
                            else if (that.uploadCom && that.uploadCom.type == "contactShopkeeper") {
                                Vue.set(that.uploadCom, "iconUrl", filePath);
                            }
                            else if (that.uploadCom.type == 'video') {

                                if (type == 'img') {
                                    that.uploadCom.poster = filePath;
                                } else {
                                    that.uploadCom.src = filePath;
                                }
                            }
                            else {
                                if (that.uploadCom.imgurl != undefined) {
                                    that.uploadCom.imgurl = filePath;
                                } else if (that.uploadCom.img != undefined) {
                                    that.uploadCom.img = filePath;
                                }
                            }

                        }
                    }
                    //上传完成后重新初始上传组件
                    that.initUploader(type);

                });
                //获取上传按钮源HTML
                function getBtnHTML(type) {
                    switch (type) {
                        case 'img':
                            return '<i class="layui-icon">&#xe67c;</i>上传图片'
                        case 'video':
                            return '<i class="layui-icon">&#xe67c;</i>上传视频'
                        case 'audio':
                            return '<i class="layui-icon">&#xe67c;</i>上传音频'
                        default:
                            return '';
                    }
                }
            },

            /*********开始上传文件***********/
            uploadFile: function (type) {
                var that = this;
                $.get('/upload/initupload', {
                    type: type
                }).then(function (data) {
                    var filePath = data.dir + data.key;
                    that.uploader[type].filePath = data.host + '/' + filePath;

                    var multipartParams = {
                        //因为不知道生成随机文件名的规则，所以，存储原始文件名
                        'key': filePath,
                        'policy': data.policy,
                        'OSSAccessKeyId': data.accessid,
                        'success_action_status': '200',
                        'signature': data.signature
                    };
                    that.uploader.token = multipartParams;
                    that.uploader[type].instance.setOption({
                        'url': data.host,
                        'multipart_params': multipartParams
                    });
                    that.uploader[type].instance.start();
                }).fail(function (data) {
                });

            },
            pickImage: function (uploadcom, index, e) {
                this.uploadCom = uploadcom;
                this.uploadIndex = index;

                if (!e.target.classList.contains('layui-icon')) {
                    this.uploader.img.element = e.target;
                } else {
                    this.uploader.img.element = e.target.parentElement;
                }


                $('#browse_file_img').click();
            },
            pickVideo: function (uploadcom, e) {
                this.uploadCom = uploadcom;
                if (!e.target.classList.contains('layui-icon')) {
                    this.uploader.video.element = e.target;
                } else {
                    this.uploader.video.element = e.target.parentElement;
                }
                $('#browse_file_video').click();
            },
            pickAudio: function (uploadcom, e) {
                this.uploadCom = uploadcom;
                if (!e.target.classList.contains('layui-icon')) {
                    this.uploader.audio.element = e.target;
                } else {
                    this.uploader.audio.element = e.target.parentElement;
                }
                $('#browse_file_audio').click();
            },
            /*同步产品分类*/
            SyncData: function () {
                var d = this.normalpages;
                try {
                    if (d && d.length > 0) {
                        for (var i = 0; i < d.length; i++) {
                            var _cur_page = d[i];
                            if (_cur_page && _cur_page.coms.length > 0) {
                                for (var j = 0; j < length; j++) {
                                    var _cur_com = _cur_page.coms[j];
                                    if (_cur_com.type == "goodlist" && ('goodCat' in _cur_com)) {

                                    }
                                }
                            }
                        }
                    }
                }
                catch (ex) {
                    consol.log(ex);
                }
            },
            //通过微赞直播地址获取直播拉流地址
            getLiveAddress: function () {
                var _selcom = this.selCom();
                var that = this;
                if (_selcom && _selcom.type == "live") {
                    if (!_selcom.vzliveurl) {
                        layer.msg("请先输入微赞直播地址！");
                        return;
                    }
                    var result = /http:\/\/vzan.com\/live\/tvchat-(\d+).*/gi.exec(_selcom.vzliveurl);
                    if (!result) {
                        layer.msg("请输入正确的微赞直播地址！");
                        return;
                    }
                    var tpid = result[1];
                    $.post("http://tliveapi.vzan.com/VZLive/GetLivePlay", { tpid: tpid }).then(function (res) {
                        if (res.isok) {
                            if (res.dataObj.playurl != "") {
                                _selcom.url = res.dataObj.playurl;
                                that.savepage();
                            }
                            else {
                                layer.msg("保存失败，直播未开始！");
                            }

                        }
                        else {
                            layer.msg("保存失败， 请检查直播地址是否正确！");
                            return;
                        }
                    });
                }
            },
            //一键复制小未案例
            CopyTemplate: function () {
                var that = this;
                layer.confirm('复制小未案例数据将会替换原有数据，是否确定？', {
                    btn: ['确定', '取消'] //按钮
                }, function () {
                    $.post("/enterprisepro/CopyTemplate", { appId: aid }, function (data) {
                        //console.log(data);
                        if (data.isok) {

                            that.pages = JSON.parse(data.data);
                        } else {
                            layer.msg(data.msg);
                        }
                    })
                    layer.closeAll();
                });

                //console.log(that.pages);
                //console.log(aid);
                //layer.msg("ddd");

            },

            //检索联系商家数据
            SearchingContactShopkeeper: function (item) {

                if (!item.openTel && !item.openService) {
                    item.openTel = true;
                    layer.msg("必须开启一个按钮,默认开启 [拨打电话]");
                }

                if (!item.pageShow && !item.iconShow) {
                    item.pageShow = true;
                    layer.msg("必须开启一种显示方式,默认开启 [页面展示]");
                }

                //悬浮图标只能在开启了相应按钮后才能开启



                if (!item.openTel || !item.iconShow) item.openTelSuspend = false;
                else item.openTelSuspend = true;

                if (!item.openService || !item.iconShow) item.openServiceSuspend = false;
                else item.openServiceSuspend = true;


                this.DetectionWxShopMessageState();
            },

            //若所有的 联系店主的 在线客服都没有开启,那么同步关闭掉全局的 '商品详情推送开关'
            DetectionWxShopMessageState: function () {
                var isOpenService = false;
                var pages = pagedata.pages;
                $(pages).each(function (index, page) {
                    $(page.coms).each(function (index, com) {
                        if (com.type == "contactShopkeeper") {
                            if (com.openService) {
                                isOpenService = true;
                            }
                        }
                    });
                });
                if (!isOpenService) {
                    this.extraConfig.openWxShopMessage = false;
                }
            },

            //同步 联系店主 LOGO至商家最新授权
            UpdateWxShopLogoUrl: function () {
                var that = this;

                var pages = pagedata.pages;
                $(pages).each(function (index, page) {
                    $(page.coms).each(function (index, com) {
                        if (com.type == "contactShopkeeper") {
                            com.headImg = that.shopLogo_url;
                        }
                    });
                });
            },

            //根据aId获取小程序头像,若获取失败返回默认头像
            GetShoperLogoUrl: function () {
                var logUrl = 'http://j.vzan.cc/dz/content/images/Enterprisepro/xwLogo.png';

                $.ajax({
                    type: "post",
                    url: "/common/GetShoperLogoUrl",
                    data: { appId: aid },
                    async: false,
                    success: function (data) {
                        if (data.isok) {
                            logUrl = data.dataObj.headImg;
                        }
                    }
                });

                return logUrl;
            },

            //获取当前页面开关
            GetCurPageContactShopkeeperSwitch: function (switchName) {
                if (!this.selPage())
                    return false;

                var isOpen = false;
                $(this.selPage().coms).each(function (index, obj) {
                    if (obj.type == "contactShopkeeper") {
                        if (switchName == "openTelSuspend") {
                            isOpen = obj.openTelSuspend;
                        }
                        if (switchName == "openServiceSuspend") {
                            isOpen = obj.openServiceSuspend;
                        }
                    }
                });
                return isOpen;
            },

            //获取当前页开启的悬浮图标数量
            GetPageRightToolsCount: function () {
                var toolsCount = 0;
                if (this.GetCurPageContactShopkeeperSwitch('openTelSuspend')) {
                    toolsCount++;
                }
                if (this.GetCurPageContactShopkeeperSwitch('openServiceSuspend')) {
                    toolsCount++;
                }
                if (this.GetPageCom('bgaudio')) {
                    toolsCount++;
                }
                if (this.GetPageCom('share')) {
                    toolsCount++;
                }
                return toolsCount;
            },

            //链接功能
            ChangeLinkFunctionByType: function (current, event) {
                var url = current.url;
                var furl = current.furl;
                var urltype = current.urltype;
                if (urltype != 2) {
                    return;
                }
                //if (!(furl == undefined || furl == 4 || url == -1 || furl == -1)) {
                //    event.currentTarget.value = -1;
                //    current.furl = -1;
                //    current.url = -1;
                //    layer.msg("该功能不可与页面共同跳转");
                //    return false;
                //}
                if (furl != 4) {
                    current.url = -1;
                }
            },
            getFlashDeals: function (callback) {
                $.get('/FlashDeal/Get', { appId: aid, pageIndex:1,pageSize:9999,}).then(function (result) {
                    if (!result.isok) {
                        layer.msg(result.Msg);
                        return;
                    }
                    callback(result.dataObj.deals);
                }).fail(function (result) {
                    alert(String.raw`error:${result.statusText}`);
                });
            }
        }
    }
    return mixin;
});

function sortorder(a, b) {
    return b.sort - a.sort
}