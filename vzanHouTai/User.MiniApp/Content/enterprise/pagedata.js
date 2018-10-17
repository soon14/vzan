define([
    'common',
], function () {
    var pagedata = {
        //皮肤列表
        //skinList: [

        //    { name: "蓝色", type: "blue", color: "#218CD7", sel: false },
        //    { name: "粉色", type: "pink", color: "#FF5A9B", sel: false },
        //    { name: "绿色", type: "green", color: "#1ACC8E", sel: false },
        //    { name: "红色", type: "red", color: "#FE525F", sel: false },
        //    { name: "白色", type: "white", color: "#ffffff", sel: false },
        //],
        skinList: [
            { name: "蓝色", type: "skin_blue", color: "#ffffff", bgcolor: "#218CD7", sel: false },
            { name: "粉色", type: "skin_pink", color: "#ffffff", bgcolor: "#FF5A9B", sel: false },
            { name: "绿色", type: "skin_green", color: "#ffffff", bgcolor: "#1ACC8E", sel: false },
            { name: "红色", type: "skin_red", color: "#ffffff", bgcolor: "#fe525f", sel: false },
            { name: "白色", type: "skin_white", color: "#000000", bgcolor: "#ffffff", sel: false },

            { name: "黑色", type: "skin_black1", color: "#ffffff", bgcolor: "#3a393f", sel: false },
            { name: "红色1", type: "skin_red1", color: "#ffffff", bgcolor: "#f51455", sel: false },
            { name: "红色2", type: "skin_red2", color: "#ffffff", bgcolor: "#e7475e", sel: false },
            { name: "红色3", type: "skin_red3", color: "#ffffff", bgcolor: "#f65676", sel: false },

            { name: "橙色1", type: "skin_orange1", color: "#ffffff", bgcolor: "#f7ad0a", sel: false },
            { name: "橙色2", type: "skin_orange2", color: "#ffffff", bgcolor: "#f79d2d", sel: false },
            { name: "橙色3", type: "skin_orange3", color: "#ffffff", bgcolor: "#f9c134", sel: false },
            { name: "橙色4", type: "skin_orange4", color: "#ffffff", bgcolor: "#f78500", sel: false },
            { name: "橙色5", type: "skin_orange5", color: "#ffffff", bgcolor: "#ef7030", sel: false },
            { name: "橙色6", type: "skin_orange6", color: "#ffffff", bgcolor: "#f05945", sel: false },

            { name: "绿色1", type: "skin_green1", color: "#ffffff", bgcolor: "#99cd4e", sel: false },
            { name: "绿色2", type: "skin_green2", color: "#ffffff", bgcolor: "#7dc24b", sel: false },
            { name: "绿色3", type: "skin_green3", color: "#ffffff", bgcolor: "#31b96e", sel: false },
            { name: "紫色1", type: "skin_purple1", color: "#ffffff", bgcolor: "#6c49b8", sel: false },
            { name: "紫色2", type: "skin_purple2", color: "#ffffff", bgcolor: "#86269b", sel: false },
            { name: "蓝色1", type: "skin_blue1", color: "#ffffff", bgcolor: "#4472ca", sel: false },
            { name: "蓝色2", type: "skin_blue2", color: "#ffffff", bgcolor: "#5e7ce2", sel: false },
            { name: "蓝色3", type: "skin_blue3", color: "#ffffff", bgcolor: "#1098f7", sel: false },
            { name: "蓝色4", type: "skin_blue4", color: "#ffffff", bgcolor: "#558ad8", sel: false },
            { name: "蓝色5", type: "skin_blue5", color: "#ffffff", bgcolor: "#2a93d4", sel: false }
        ],
        defaultIcon: vz_defaulticon,
        item: "",
        extraConfig: extraConfig,
        pagercount: 1,
        ispost: false,
        goodtypelist: goodtypelist,
        goodmintypelist: goodmintypelist,
        goodexttypeslist: _goodexttypeslist,
        newstypelist: _newstypelist,
        news2typelist: _news2typelist,
        pages: [],
        syncmainsite: syncmainsite,
        coms: {
            "bottomnav": {
                type: "bottomnav",
                name: "底部导航",
                navlist: [],//数组元素为navItem类型

            },
            "imgnav": {
                type: "imgnav",
                name: "图片导航",
                navlist: [],
            },
            "slider": {
                type: "slider",
                name: "轮播图",
                items: [],

            },
            "goodlist": {
                type: "goodlist",
                name: "产品列表", //组件名字
                goodShowType: "normal", //产品显示方式
                isShowGoodCatNav: false, //是否显示分类导航
                GoodCatNavStyle: 4,//分类导航样式
                isShowGoodSearch: false,//是否显示搜索框
                isShowGoodPriceSort: false,//是否显示价格排序
                isShowFilter: false,//是否显示筛选
                isShowGoodSaleCountSort: false,//是否显示销量排序
                filterTypeList: [],//筛选项目
                goodCat: [], //选择的产品分类
                goodExtCat: [],//选择的扩展分类
                pickallgoodcat: false,//是否选中所有分类
                subscribeSwitch: false,//是否开启预约
                isShowPrice: true,
            },
            "good": {
                type: "good",
                name: "产品",
                items: [],
                goodShowType: "small",
                subscribeSwitch: false,//是否开启预约
                isShowPrice: true,
            },
            "form": {
                type: "form",
                name: "表单",
                title: "",
                items: [],
                formstyle: 1,//1,2,3三种样式 默认为1
            },
            "richtxt": {
                type: "richtxt",
                name: "富文本",
                content: "",

            },
            "video": {
                type: "video",
                name: "视频",
                src: "",
                sourcetype: 'neturl',//资源类型：neturl=网络地址，src=上传的视频地址 默认是网络地址
                poster: "",
                autoplay: false,
            },
            "bgaudio": {
                type: "bgaudio",
                name: "背景音乐",
                src: "",
                sourcetype: 'neturl',
                autoplay: true,
            },
            "kefu": {
                type: "kefu",
                name: "在线客服",
                position: 'right',
                top: 150,
                icon: "icon-contact1",
                msgtype: "txt",
                txt: "",
                img: "",
            },
            "makecall": {
                type: "makecall",
                name: "拨打电话",
                txt: '拨打电话',
                phone: '',
                icon: "icon-contact3",
            },
            "map": {
                type: "map",
                name: "地图",
                txt: "地图引导语",
                latitude: 0,//纬度
                longitude: 0,//经度
                scale: 15,//缩放等级
                showmap: false
            },
            "img": {
                type: "img",
                name: "图片",
                imgurl: "",
                url: -1,
                urltype: -1,//-1=不跳转，0=跳转到页面，1=跳转到小程序，2=跳转链接功能，3=产品详情页，4=产品分类，5=拼团详情页，6=砍价详情页
                furl: "-1",//链接功能跳转
                appid: "",
                path: "",
                items: [],
                itemstype: [],
                btnType: "",// ""=不显示|yuyue=预约|buy =购买
                target: "_blank",
            },
            "yyform": {
                type: "yyform",
                name: "产品预约",
                title: "",
                items: [],
            },
            "news": {
                type: "news",
                name: "内容资讯",
                title: "",
                titlestyle: 3,
                listmode: 'all',//all:全部，pick:手动选择 只显示选择的
                list: [],
                liststyle: 1,
                typeid: 0,
                num: 4,//显示条数 默认是4 如果为0表示显示全部

            },
            "coupons": {
                type: "coupons",
                name: "优惠券",
                items: [],
                couponsShowType: "one",
            },
            "flashdeal": {
                type: "flashdeal",
                name: "秒杀",
                items: [],
                showAll: true,
                showItemCount: 0,
                flashDealId: 0,
                title: "秒杀"
            },
            //"magicCube": {
            //    type: "magicCube",
            //    name: "魔方图片",
            //    items: [],
            //    imgSpacing:5,
            //    style: 0,
            //    pagesIndex: 0,
            //},
        },
        //魔方图片样式
        magicCubeSetType: [
            { title: "1行2个", magicImg: "/Content/enterprise/img/pageset/magic-01.png" },
            { title: "1行3个", magicImg: "/Content/enterprise/img/pageset/magic-02.png" },
            { title: "1行4个", magicImg: "/Content/enterprise/img/pageset/magic-03.png" },
            { title: "2左2右", magicImg: "/Content/enterprise/img/pageset/magic-04.png" },
            { title: "1左2右", magicImg: "/Content/enterprise/img/pageset/magic-05.png" },
            { title: "1上2下", magicImg: "/Content/enterprise/img/pageset/magic-06.png" },
        ],
        //产品显示方式
        goodShowType: {
            "big": {
                demoImg: "http://j.vzan.cc/dz/content/miniapEnterprise/img/pageset/good_big.jpg",
            },
            "small": {
                demoImg: "http://j.vzan.cc/dz/content/miniapEnterprise/img/pageset/good_small.jpg",
            },
            "normal": {
                demoImg: "http://j.vzan.cc/dz/content/miniapEnterprise/img/pageset/good_normal.jpg",
            },
            "scroll": {
                demoImg: "http://j.vzan.cc/dz/content/miniapEnterprise/img/pageset/good_scroll.jpg",
            }
        },
        //产品分类导航显示方式
        goodCatType: {
            "top": {
                demoImg: "http://j.vzan.cc/dz/content/miniapEnterprise/img/pageset/img/good_cat_top.jpg",
            },
            "left": {
                demoImg: "http://j.vzan.cc/dz/content/miniapEnterprise/img/pageset/img/good_cat_left.jpg",
            }
        },
        selPageIndex: -1,
        selComIndex: -1,
        uploadCom: null,
        selIconfont: "",
        uploadIndex: -1,
        goodModel: {
            list: [],
            psize: 10,
            pindex: 1,
            search: '',
            goodType: 0,
            allcount: 0,
        },
        ptypeModel: {
            list: [],
            psize: 10,
            pindex: 1,
            allcount: 0,
        },
        newsModel: {
            list: [],
            psize: 10,
            pindex: 1,
            allcount: 0,
        },
        cutpriceModel: {
            list: [],
            psize: 10,
            pindex: 1,
            allcount: 0,
        },
        joingroupModel: {
            list: [],
            psize: 10,
            pindex: 1,
            allcount: 0,
        },
        entjoingroupModel: {
            list: [],
            psize: 10,
            pindex: 1,
            allcount: 0,
        },
        formName_fmt: {
            "text": "文本",
            "number": "数字",
            "radio": "单选",
            "checkbox": "多选",
            "date": "日期",
            "sex": "性别",
        },
        uploader: {
            tempFilePath: null,
            img: {
                instalce: null,
                filePath: '',
                element: null
            },
            video: {
                instalce: null,
                filePath: '',
                element: null
            },
            audio: {
                instalce: null,
                filePath: '',
                element: null
            }
        },
        couponsModel: {
            list: [],
            psize: 10,
            pindex: 1,
            search: '',
            couponsType: 0,
            allcount: 0,
        },
        urltypelist: [
            { id: -1, name: "不跳转" },
            { id: 0, name: "跳转到页面" },
        ],
        flashDeals: [],
        flashDealsModel: {
            list: [],
            psize: 10,
            pindex: 1,
            allcount: 0,
        },
        meConfig: window.meConfig,
    };

    /*如果组件改动了属性，这个方法用来修正旧数据*/
    if (pages && pages.length > 0) {
        pages.forEach(function (page_item) {
            if (page_item.coms.length > 0) {
                page_item.coms.forEach(function (com_item) {
                    if (com_item.type == "goodlist" ||
                        com_item.type == "good") {

                        if (!("isShowPrice" in com_item)) {
                            com_item.isShowPrice = true;
                        }
                    }
                    else if (com_item.type == "img") {
                        if (!("urltype" in com_item)) {
                            com_item.urltype = "page";
                        }
                        if (!("btnType" in com_item)) {
                            com_item.btnType = "";
                        }
                    }
                });
            }
        });
    }


    return pagedata;
});