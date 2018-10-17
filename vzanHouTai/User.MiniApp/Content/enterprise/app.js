//行业版
require.config({
    baseUrl: "../../Content/enterprise/",
　　});
require([
    'pagedata',
    'page',
], function (pagedata, mixin) {

    pagedata.coms.magicCube = {
        type: "magicCube",
        name: "魔方图片",
        items: [],
        imgSpacing: 5,
        style: 0,
        pagesIndex: 0,
    };
    pagedata.coms.spacing = {
        type: "spacing",
        name: "间距",
        items: [],
        title: "间距设置",
        color: "black",
        spacing: 10, //默认最小值是10，最大值100
    };
    //广告组件
    pagedata.coms.ad = {
        type: "ad",
        name: "广告",
        unitid: "",//广告id
    }

    pagedata.urltypelist = [
    { id: -1, name: "不跳转" },
    { id: 0, name: "跳转到页面" }
    //{ id: 1, name: "跳转到小程序" },
    //{ id: 3, name: "产品详情页" },
    //{ id: 4, name: "产品分类" }
    ]
    $.extend(true, pagedata.pages, pages);
   
    var app = new Vue(mixin);
    console.log("ok");
});

