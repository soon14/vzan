var MAX_BOTTOMNAVITEMCOUNT = 5,
    MAX_IMG_SIZE = 10,//单位M
    MAX_AUDIO_SIZE = 10,
    MAX_VIDEO_SIZE = 20;


//分页组件
var vuepager = Vue.component("vue-pager", {
    template: `<div class="vuepager" v-show="show">
            <div class ="pagerlist">
                <span class="jump" v-bind:class="{disabled:pstart}" v-on:click="jumpPage(--current_page)">上一页</span>
                <span v-show="current_page>5" class="jump" v-on:click="jumpPage(1)">1</span>
                <span class="ellipsis" v-show="efont">...</span>
                <span  class="jump" v-for="(item,index) in indexs" v-bind:class="{bgprimary:current_page==item}" v-on:click="jumpPage(item)">{{item}}</span>
                <span class="ellipsis" v-show="ebehind">...</span>
                <span :class="{disabled:pend}" class="jump" v-on:click="jumpPage(++current_page)">下一页</span>
                <span v-show="current_page<pagercount-4" class="jump" v-on:click="jumpPage(pagercount)">{{pagercount}}</span>
            </div>
        </div>
        `,
    props: ['name'],
    data: function () {
        return {
            current_page: 1, //当前页
            pagercount: 1, //总页数
            changePage: '',//跳转页
            list: [],
            nowIndex: 0,
            typeid: 0,
            pagerkey: ""
        }
    },
    mounted: function () {
        console.log("vue-pager mounted " + this.name);
        if (this.name == "goods") {
            this.GetProducts();
        }
        else if (this.name == "news") {
            this.GetNews();
        }
        else if (this.name == "cutprice") {
            this.GetCutPrice();
        }
        else if (this.name == "joingroup") {
            this.GetJoinGroup();
        }
        else if (this.name == "entjoingroup") {
            this.GetEntJoinGroup();
        }
        else if (this.name == "coupons") {
            this.GetCoupons();
        }
        else if (this.name == "flashdeals") {
            this.GetFlashDeals();
        }
        //this.GetEntJoinGroup();
    },
    computed: {
        show: function () {
            return this.pagercount && this.pagercount != 1;
        },
        pstart: function () {
            return this.current_page == 1;
        },
        pend: function () {
            return this.current_page == this.pagercount;
        },
        efont: function () {
            if (this.pagercount <= 7) return false;
            return this.current_page > 5
        },
        ebehind: function () {
            if (this.pagercount <= 7) return false;
            var nowAy = this.indexs;
            return nowAy[nowAy.length - 1] != this.pagercount;
        },
        indexs: function () {

            var left = 1,
                right = this.pagercount,
                ar = [];
            if (this.pagercount >= 7) {
                if (this.current_page > 5 && this.current_page < this.pagercount - 4) {
                    left = Number(this.current_page) - 3;
                    right = Number(this.current_page) + 3;
                } else {
                    if (this.current_page <= 5) {
                        left = 1;
                        right = 7;
                    } else {
                        right = this.pagercount;

                        left = this.pagercount - 6;
                    }
                }
            }
            while (left <= right) {
                ar.push(left);
                left++;
            }
            return ar;
        },

    },
    created: function () {
        console.log("vue-pager created " + this.name);
    },
    watch: {
        "typeid": function (newVal, oldVal) {
            console.log(newVal + "-" + oldVal);
            if (this.name == "news" && this.typeid != 0) {
                this.current_page = 1;
                this.GetNews();
            }
        },
        "pagerkey": function (newVal, oldVal) {
            if (this.name == "goods") {
                this.current_page = 1;
                this.GetProducts();
            }
            else if (this.name == "news") {
                this.current_page = 1;
                this.GetNews();
            }
        },
    },
    methods: {
        jumpPage: function (id) {
            this.current_page = id;
            if (this.name == "goods") {
                this.GetProducts();
            }
            else if (this.name == "news") {
                this.GetNews();
            }
            else if (this.name == "cutprice") {
                this.GetCutPrice();
            }
            else if (this.name == "joingroup") {
                this.GetJoinGroup();
            }
            else if (this.name == "entjoingroup") {
                this.GetEntJoinGroup();
            }
            else if (this.name == "coupons") {
                this.GetCoupons();
            } else if (this.name == "flashdeals") {
                this.GetFlashDeals();
            }
        },
        GetProducts: function (index) {
            var that = this;
            if (pagetype == 26 && HomeId > 0) {
                //表示多门店版本 门店产品
                return $.post("/MultiStore/GoodsManager/subgoodlist", { appId: aid, storeId: storeId, isPost: "isPost", pageIndex: that.current_page, pageSize: 10, search: that.$parent.goodModel.search, ptype: that.$parent.goodModel.goodType }, function (data) {

                    that.$parent.goodModel.list = data.DataList;
                    that.pagercount = data.PageCount;



                }, 'json');
            }
            return $.post("/" + dirname + "/plist", { appId: aid, pageIndex: that.current_page, pageSize: 10, search: that.$parent.goodModel.search, goodType: that.$parent.goodModel.goodType, }, function (data) {

                that.$parent.goodModel.list = data.DataList;
                that.pagercount = data.PageCount;
            }, 'json');

        },
        GetNews: function () {
            var that = this;
            if (this.typeid == 0) {
                return;
            }
            return $.post("/" + dirname + "/newslist", { appId: aid, pageIndex: that.current_page, pageSize: 10, typeid: that.typeid }, function (data) {

                that.$parent.newsModel.list = data.DataList;

                that.pagercount = data.PageCount;

            }, 'json');
        },
        GetCutPrice: function () {
            var that = this;
            return $.post("/" + dirname + "/Bargain", { appId: aid, actionType: 1, pageIndex: that.current_page, pageSize: 10 }, function (data) {
                console.log(data);
                that.$parent.cutpriceModel.list = data.DataList;
                that.pagercount = data.PageCount;

            }, 'json');
        },
        GetFlashDeals: function () {
            var that = this;
            return $.get("/FlashDeal/Get", { appId: aid, pageIndex: that.current_page, pageSize: 10 }, function (data) {
                console.log(data);
                that.$parent.flashDealsModel.list = data.dataObj.deals;
                that.pagercount = data.dataObj.pageCount;

            }, 'json');
           
        },
   
        GetJoinGroup: function () {
            var that = this;
            return $.post("/" + dirname + "/GetGroupListPage", { appId: aid, actionType: 1, pageIndex: that.current_page, pageSize: 10 }, function (data) {
                console.log(data);
                that.$parent.joingroupModel.list = data.DataList;
                that.pagercount = data.PageCount;

            }, 'json');
        },
        GetEntJoinGroup: function () {
            var that = this;
            return $.post("/" + dirname + "/GetEntGroupListPage", { appId: aid, actionType: 1, pageIndex: that.current_page, pageSize: 10 }, function (data) {
                console.log(data);
                that.$parent.entjoingroupModel.list = data.DataList;
                that.pagercount = data.PageCount;

            }, 'json');
        },
        GetCoupons: function () {
            var that = this;
            return $.post("/enterprise/coupinlist", { appId: aid, storeId: storeId, couponname: that.$parent.couponsModel.search, pageIndex: that.current_page, pageSize: 10 }, function (data) {
                that.$parent.couponsModel.list = data.DataList;
                that.pagercount = data.PageCount;
                var couponsList = that.$parent.couponsModel.list;

                for (var i = 0; i < couponsList.length; i++) {
                    couponsList[i].LimitMoneyStr = couponsList[i].LimitMoneyStr.split(".")[0];
                    couponsList[i].StartUseTimeStr = couponsList[i].StartUseTimeStr.split(" ")[0];
                    couponsList[i].EndUseTimeStr = couponsList[i].EndUseTimeStr.split(" ")[0];
                    if (couponsList[i].CouponWay == 0) {
                        couponsList[i].MoneyStr = couponsList[i].MoneyStr.split(".")[0];
                    }
                    Vue.set(that.$parent.couponsModel.list[i], 'sel', false)
                }
            }, 'json')
        },
    },

});
//富文本组件
var entcom_richtxt_default = Vue.component("entcom-richtxt-default", {
    template: "#entcom-richtxt-default-tpl"
});
//组件跳转
var comlink = Vue.component('comlink', {
    template: '#comlink-template',
    props: [
        'comItem',
        'comIndex',
        //'urltype',
        //'furl',
        //'url',
        //'appid',
        //'path',
        //'btnType',
        //'items',
        //'itemstype',
        //'target',
    ],
    data: function () {
        return {
            urltype_sub: -1,
            url_sub: -1,
            furl_sub: -1,
            appid_sub: "",
            path_sub: "",
            btnType_sub: "",
            target_sub: "_blank",
         
        }
    },
    mounted: function () {
        this.urltype_sub = this.comItem.urltype;
        this.url_sub = this.comItem.url;
        this.furl_sub = this.comItem.furl;
        this.appid_sub = this.comItem.appid;
        this.path_sub = this.comItem.path;
        this.target_sub = this.comItem.target;
        this.btnType_sub = this.comItem.btnType;
    },
    computed: {

    },
    methods: {
        selCom: function () {
            return this.$parent.selCom();
        },
        //获取赋值对象，
        getAssignmentObject: function () {
            var currentCom = this.selCom();
            if (currentCom.type == "magicCube") {
                return this.selCom().items[this.selCom().pagesIndex];
            }
            else if (currentCom.type == "imgnav" || currentCom.type =="bottomnav") {
                return this.selCom().navlist[this.comIndex];
            }
            else if (currentCom.type == "slider" ) {
                return this.selCom().items[this.comIndex];
            }
            else if (currentCom.type == "img") {
                return this.selCom()
            }
        },
        //选择跳转类型
        ChangeLinkUrlType: function () {
            this.getAssignmentObject().urltype = this.urltype_sub;
            this.getAssignmentObject().itemstype = [];
            if (this.comItem.urltype == 0 || (this.comItem.urltype == 2 && this.comItem.furl == 4)) {

            }
            else {
                this.getAssignmentObject().furl = -1;
            }

            if (this.comItem.urltype == 9) {
                this.$parent.GetFlashDeals();
            }

      

        },

        //选择页面/选择跳转功能
        ChangeLinkPage: function () {

            if (this.comItem.urltype == 0 || (this.comItem.urltype == 2 && this.comItem.furl == 4)) {
                this.getAssignmentObject().url = this.url_sub;
            }
            else {
                this.getAssignmentObject().url = - 1;
            }
            this.getAssignmentObject().furl = this.furl_sub;
        },
        //填写小程序appid
        ChangeLinkAppid: function () {
            this.getAssignmentObject().appid = this.appid_sub;
        },
        //填写小程序path
        ChangeLinkPath: function () {
            this.getAssignmentObject().path = this.path_sub;
        },
        ShowGoodModal: function (index) {
            this.selCom().editindex = index;
            $("#AddGoodModal").modal("show");

        },
        ShowGoodTypeModal: function (index) {
            this.selCom().editindex = index;
            $("#AddGoodTypeModal").modal("show");
        },
        ShowGoodMinTypeModal: function (index) {
            this.selCom().editindex = index;
            $("#AddGoodMinTypeModal").modal("show");
        },
        ShowEntGroupModal: function (index) {
            this.selCom().editindex = index;
            $("#AddEntGroupModal").modal("show");
        },
        ShowLinkCutPriceModal: function (index) {
            this.selCom().editindex = index;
            $("#LinkAddCutPriceModal").modal("show");
        },
        ShowLinkGroupModal: function (index) {
            this.selCom().editindex = index;
            $("#LinkAddGroupModal").modal("show");
        },
        ShowLinkFlashDealsModal: function (index) {
            this.selCom().editindex = index;
            $("#LinkAddFlashDealsModal").modal("show");
        },
    },
    watch: {
        "comItem.urltype": function (newVal, oldVal) {
            this.urltype_sub = newVal;
            //this.urltype_sub = newVal.urltype;
            //this.url_sub = newVal.url;
            //this.furl_sub = newVal.furl;
            //this.appid_sub = newVal.appid;
            //this.path_sub = newVal.path;

        },
        "comItem.url": function (newVal, oldVal) {
            this.url_sub = newVal;
        },
        "comItem.furl": function (newVal, oldVal) {
            this.furl_sub = newVal;
        },
        "comItem.appid": function (newVal, oldVal) {
            this.appid_sub = newVal;
        },
        "comItem.path": function (newVal, oldVal) {
            this.path_sub = newVal;
        },
        "btnType_sub": function (newVal, oldVal) {
            this.getAssignmentObject().btnType = newVal;
        },
        "target_sub": function (newVal, oldVal) {
            this.getAssignmentObject().target = newVal;
        },



        "comItem.btnType": function (newVal, oldVal) {
            this.btnType_sub = newVal;
        },
        "comItem.target": function (newVal, oldVal) {
            this.target_sub = newVal;
        }
    }
})

var uploadhand = null;
//组件使用的百度编辑器
var comeditor = null;
//底部导航同步定时器
var bottomNavSync = null;

const def_good_img_big = 'http://j.vzan.cc/dz/content/miniapEnterprise/img/pageset/blank_315x315.png';

//底部导航项对象
var navItem = {
    "name": "",
    "img": "",
    "url": -1,
    "sel": false,
    icon: "",
    furl: "-1",//链接跳转
    urltype: -1,//-1=不跳转，0=跳转到页面，1=跳转到小程序，2=跳转链接功能，3=产品详情页，4=产品分类，5=拼团详情页，6=砍价详情页
    appid: "",
    itemstype: [],//分类
    items: [],//产品详情
    btnType: "",// ""=不显示|yuyue=预约|buy =购买
    path: "",
    target: "_self",
};
//page项对象
var pageItem = {
    skin: 0,
    name: "",
    sel: false,
    target: "_self",
    coms: [],
    selComIndex: -1,
    def_name: '',
    img: '',
}
//轮播图项对象
var sliderItem = {
    furl: "-1",//链接跳转
    img: '',
    target: '_blank',
    url: -1,
    sel: false,
    urltype: -1,//-1=不跳转，0=跳转到页面，1=跳转到小程序，2=跳转链接功能，3=产品详情页，4=产品分类，5=拼团详情页，6=砍价详情页
    appid: "",
    itemstype: [],//分类
    items: [],//产品详情
    btnType: "",// ""=不显示|yuyue=预约|buy =购买
    path: "",
};

//魔方图片项对象
var magicItem = {
    "furl": "-1",//链接跳转
    "name": "",
    "img": "",
    "url": -1,
    "sel": false,
    urltype: -1,//-1=不跳转，0=跳转到页面，1=跳转到小程序，2=跳转链接功能，3=产品详情页，4=产品分类，5=拼团详情页，6=砍价详情页
    appid: "",
    itemstype: [],//分类
    items: [],//产品详情
    btnType: "",// ""=不显示|yuyue=预约|buy =购买
    path: "",

    target: "_blank",
};


var formMutiSelectItem = {
    name: "",
    placeholder: "请输入选择项"
};

var formItem = {
    "text": {
        type: "text",
        placeholder: "",
        name: "",
    },
    "number": {
        type: "number",
        name: "",
    },
    "radio": {
        type: 'radio',
        items: [],
        name: "",
    },
    "checkbox": {
        type: 'checkbox',
        items: [],
        name: "",
    },
    "time": {
        type: "time",
        name: "",
    },
    "date": {
        type: "date",
        name: "",
    },
    "sex": {
        type: 'radio',
        items: ["男", "女"],
        name: "性别",
    }

}