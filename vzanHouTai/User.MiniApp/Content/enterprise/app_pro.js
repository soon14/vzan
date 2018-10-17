//专业版
require.config({
    baseUrl: "../../Content/enterprise/",
});
require([
    'pagedata',
    'page',
], function (pagedata, mixin) {

    MAX_BOTTOMNAVITEMCOUNT = 4;
    MAX_IMG_SIZE = 20;//单位M
    MAX_AUDIO_SIZE = 20;
    MAX_VIDEO_SIZE = 200;

    delete pagedata.coms.goodlist.subscribeSwitch;
    pagedata.coms.goodlist.btnType = "";
    pagedata.coms.goodlist.isShowPrice = true;



    delete pagedata.coms.good.subscribeSwitch;
    pagedata.coms.good.btnType = "";
    pagedata.coms.good.title = "热门推荐";

    pagedata.coms.share = {
        type: "share",
        name: "分享转发",
        icon: "icon-Recommend5",
        img: "",
    };

    pagedata.coms.newslist = {
        type: "newslist",
        name: "资讯列表", //组件名字
        newsShowType: "normal", //产品显示方式 默认详情列表
        isShowNewsNav: false, //是否显示分类导航
        NewsNavStyle: 1,//分类导航样式
        isShowNewsSearch: false,//是否显示搜索框
        newsCat: [], //选择的资讯分类
        pickAllNewsCat: false//是否选中所有分类
    };

    pagedata.coms.live = {
        type: "live",
        name: "直播购物",
        title: "",
        img: "",
        des: "",
        url: "",
        vzliveurl: "",//微赞直播网址
        items: []
    };

    pagedata.coms.cutprice = {
        type: "cutprice",
        name: "砍价",
        items: [],
        title: "砍价活动"
    };

    pagedata.coms.joingroup = {
        type: "joingroup",
        name: "团购",
        items: [],
        title: "团购活动"
    };

    pagedata.coms.entjoingroup = {
        type: "entjoingroup",
        name: "拼团",
        items: [],
        title: "拼团活动",
        displayMode: "",//显示方式：default||"",big,small
    };


    pagedata.coms.coupons = {
        type: "coupons",
        name: "优惠券",
        items: [],
        title: "店铺优惠券",
        couponsShowType: "one",
    };
    pagedata.coms.spacing = {
        type: "spacing",
        name: "间距",
        items: [],
        title: "间距设置",
        color: "black",
        spacing: 10, //默认最小值是10，最大值100
    };
    pagedata.coms.magicCube = {
        type: "magicCube",
        name: "魔方图片",
        items: [],
        imgSpacing: 5,
        style: 0,
        pagesIndex: 0,
    };
    pagedata.coms.search = {
        type: "search",
        name: "产品搜索",
        style: 1,//1：方形，2：圆角，3：方形+按钮，4：放大镜+按钮
        bgStyle: 1, //背景色 1：白色,2：皮肤颜色
        placeholder: '商品搜索',
    };
    pagedata.coms.contactShopkeeper = {
        type: "contactShopkeeper",
        name: "联系店主",
        shopLogo_url: 'http://j.vzan.cc/dz/content/images/Enterprisepro/xwLogo.png',//商家logo路径
        openTel: true, //是否开启电话联系
        openTelSuspend: false,//开启电话图标悬浮 -为兼容保留
        phoneNum: '',//电话号码
        openService: true, //是否开启在线客服
        openServiceSuspend: false,//开启在线客服图标悬浮 -为兼容保留
        txt: "欢迎光临本店,我是店主",
        pageShow: true, //页面展示
        iconShow: false, //浮标展示
        serverType: "wxServer",//客服类型： wxServer.微信客服 miniappServer.小程序客服(我们自己的客服组件)
        iconType: 0,//0=小程序图标，1=自定义图标
        iconUrl:"",//自定义图标路径
    };
    //广告组件
    pagedata.coms.ad = {
        type: "ad",
        name: "广告",
        unitid: "",//广告id
    }
    //pagedata.coms.takeout = {
    //    type: "takeout",
    //    name: "外卖",
    //    styleType: 1,//样式
    //    showStoreInfo: true,//显示门店信息
    //    goodCat:[]//产品分类列表
    //}

    pagedata.urltypelist = [
        { id: -1, name: "不跳转" },
        { id: 0, name: "跳转到页面" },
        { id: 1, name: "跳转到小程序" },
        { id: 2, name: "跳转到功能" },
        { id: 3, name: "产品详情页" },
        { id: 4, name: "产品分类" },
        { id: 5, name: "拼团详情页" },
        { id: 6, name: "砍价详情页" },
        { id: 7, name: "团购详情页" },
         { id: 8, name: "产品小类" },
         { id: 9, name: "秒杀活动" },
     
    ]

    delete pagedata.coms.kefu;
    delete pagedata.coms.makecall;

    $.extend(true, pagedata.pages, pages);

    var app = new Vue({
        mixins: [mixin]
    });
});