; (function () {
    $(function () {
        var star = null;
        new Vue({
            el: "#shopList",
            data: {
                optionList: [
                    "最新", "最热", "交通", "体育", "教育", "公益",
                    "医疗", "工具", "IT科技", "富媒体", "商家服务", "商家自营",
                    "快递邮政", "房地产", "生活服务", "物流",
                    "健身", "美容美发", "读书"
                ],
                itemsData: {
                    pageIndex: 1,
                    pageSize: 16,
                    keys: "最新",
                    type: ""
                },
                itemList: [],
                starSum: 5,
                num: 0,
                isObj: false,
                tplIndex: null,
                minAppCode: false
            },
            mounted() {
                this.toggleOption(this.num, function () { })

            },
            methods: {

                toggleOption:function(i, f) {

                    this.itemList = [];
                    this.num = i;

                    if (i == 0 || i == 1) {
                        this.itemsData.pageIndex = 1
                        this.itemsData.type = i;
                        this.getNewestItems(f)
                    } else {
                        this.itemsData.keys = this.optionList[i];
                        this.getMiniAppItems(f);
                    }

                },
                getMore:function() {
                    if (this.num == 0 || this.num == 1) {

                        this.itemsData.pageIndex++;
                        this.itemsData.type = this.num;
                        this.getNewestItems()

                    } else {
                        this.itemsData.pageIndex++;
                        this.itemsData.keys = this.optionList[this.num];
                        this.getMiniAppItems();

                    }
                },
                getNewestItems:function(f) {
                    var miniAppLoad = layer.load(2);
                    var that = this;
                    $.ajax({
                        type: "GET",
                        url: "http://testwtapi.vzan.com/apiMiniAppGw/RangeApp",
                        data: "pageIndex=" + that.itemsData.pageIndex + "&pageSize=" + that.itemsData.pageSize + "&type=" + that.itemsData.type,
                        success: function (data) {
                            layer.close(miniAppLoad)
                            if (data != undefined && data != null && data.obj.length > 0) {
                                if (data.isok) {
                                    that.isObj = false;
                                    for (var i = 0; i < data.obj.length; i++) {
                                        data.obj[i].Tags = data.obj[i].Tags.slice(0, 2);
                                        data.obj[i].startNum = data.obj[i].startNum / that.starSum * 100;
                                    }
                                    that.itemList = that.itemList.concat(data.obj);


                                } else if (that.num <= 7 && data.obj.length == 0) {
                                    layer.msg("没有更多内容")

                                } else if (that.num > 7 && data.obj.length == 0) {
                                    that.isObj = true;
                                }
                                if (f) {
                                    f();
                                }
                            }
                        },
                        error: function () {
                            console.log("error");
                        }
                    })

                },
                getMiniAppItems:function(f) {
                    var miniAppLoad = layer.load(2);
                    var that = this;
                    $.ajax({
                        type: "GET",
                        url: "http://testwtapi.vzan.com/apiMiniAppGw/Index",
                        data: "pageIndex=" + that.itemsData.pageIndex + "&pageSize=" + that.itemsData.pageSize + "&keys=" + that.itemsData.keys,
                        success: function (data) {
                            layer.close(miniAppLoad)
                            if (data != undefined && data != null && data.obj.length > 0) {
                                if (data.isok) {
                                    that.isObj = false;
                                    for (var i = 0; i < data.obj.length; i++) {
                                        data.obj[i].Tags = data.obj[i].Tags.slice(0, 2);
                                        data.obj[i].startNum = data.obj[i].startNum / that.starSum * 100;
                                    }
                                    that.itemList = that.itemList.concat(data.obj);

                                }
                            } else if (that.num <= 7 && data.obj.length == 0) {
                                layer.msg("没有更多内容")

                            } else if (that.num > 7 && data.obj.length == 0) {
                                that.isObj = true;
                            }
                            if (f) {
                                f();
                            }
                        },
                        error: function () {
                            console.log("error");
                        }
                    })

                },
                showCode:function(i) {
                    this.tplIndex = i;
                    this.minAppCode = true;
                },
                hideCode:function(i) {
                    this.tplIndex = null;
                    this.minAppCode = false;
                }
            },
            computed: {

            }

        })

    })
})()
