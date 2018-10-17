//多店铺版本
require.config({
    baseUrl: "../../Content/enterprise/",
　　});
require([
    'pagedata',
    'page',
], function (pagedata, mixin) {
    delete pagedata.coms.goodlist;
     delete pagedata.coms.kefu;
    //delete pagedata.coms.richtxt;
    delete pagedata.coms.bgaudio;
    delete pagedata.coms.makecall;
    delete pagedata.coms.map;
    delete pagedata.coms.img;
    delete pagedata.coms.news;
    delete pagedata.coms.video;
    delete pagedata.coms.form;
    delete pagedata.coms.bottomnav;
    pagedata.coms.good.comHead = "热卖中";
    pagedata.urltypelist = [
   { id: -1, name: "不跳转" },
   { id: 4, name: "产品分类" }
    ]
    $.extend(true, pagedata.pages, pages);

    

    var app = new Vue({
        mixins: [mixin],
        created: function () {
            if (pages.length <= 0) {
                this.PickCom("slider");
                this.PickCom("imgnav");
                this.PickCom("good");
            }
            this.normalpages[0].skin = 12;
           
        },
        mounted: function () {

        }
    });
});

