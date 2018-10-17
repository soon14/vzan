function fontSize() {
    var deviceWidth = $(document).width();
    if (deviceWidth > 1920) {
        deviceWidth = 1920;
        //console.log(deviceWidth);
    }
    var fontSize = deviceWidth / 19.2;
    $("html").css("fontSize", fontSize);
}
fontSize();
$(window).resize(function () {
    fontSize();
});

Vue.use(VueLazyload, {
    preLoad: 1.3,
    attempt: 1
});
var indexDl = new Vue({
    el: "#indexDl",
    data: {
        templateNum: 0,
        solutionNum: 0,
        pdCaseLabelNum: 0,
        merit: [
            { title: "流畅", text: "使用阿里云服务器，加载速度有保证，减低流失率", iconSrc: "http://j.vzan.cc/dz/content/newhome/image/indexDl/merit-01.png" },
            { title: "自由", text: "精选丰富组件，自由布局，满足99%的设计需求", iconSrc: "http://j.vzan.cc/dz/content/newhome/image/indexDl/merit-02.png" },
            { title: "专业", text: "自定义跳转页面，直达商品详情，提升潜在销售量", iconSrc: "http://j.vzan.cc/dz/content/newhome/image/indexDl/merit-03.png" }
        ],
        templateMaket: [
            { name: "单页版", text: "单页版是基础版本，是现在市面上最强大的单页版，支持轮播图，视频，背景音乐，自定义表单等组件。可随意拉拽，进行可视化界面设计，适用于所有行业。通过附近展示功能，可以定位店铺位置，为店铺带来大量客流，是商家网络名片的最佳选择。", func: [{ icon: "-0.54rem -1.54rem", text: "轮播图" }, { icon: "-5.94rem -1rem", text: "富文本" }, { icon: "-4.32rem -1rem", text: "图片" }, { icon: "-6.48rem -1rem", text: "自定义表单" }, { icon: "-3.24rem -1rem", text: "视频展示" }, { icon: "-5.4rem -1rem", text: "背景音乐" }, { icon: "-3.78rem -1rem", text: "地理导航" }] },
            { name: "企业版", text: "企业版为广大中小型企业提供一个简单高效的在线推广工具，助力商家抢占线上流量红利。", func: [{ icon: "-4.86rem -1rem", text: "公司简介" }, { icon: "-5.94rem -1.54rem", text: "产品展示" }, { icon: "0 -2.08rem", text: "动态展示" }, { icon: "-5.94rem -1.54rem", text: "发展历程" }, { icon: "-0.54rem -2.08rem", text: "联系方式" }] },
            { name: "电商版", text: "电商版为零售提供最先进的行业解决方案，帮助零售业打破自身局限，搭建微信端电商销售体系，实现流量聚合。会员系统和营销插件能有效为商户带来流量和热度。砍价和拼团的等营销功能引导老用户主动带来新客户。除此之外，还可以连接云打印机，支付完成即出小票，应用场景从线上到线下无缝衔接。", func: [{ icon: "-3.24rem -1.54rem", text: "图片导航" }, { icon: "-0.54rem -1.54rem", text: "轮播图" }, { icon: "-1.62rem -1.54rem", text: "多属性设置" }, { icon: "-2.7rem -1.54rem", text: "商品排序" }, { icon: "-1.08rem -1.54rem", text: "消息模板" }, { icon: "0 -1.54rem", text: "会员管理系统" }, { icon: "-2.16rem -1.54rem", text: "砍价" }] },
            { name: "餐饮版", text: "餐饮版为实体餐饮门店提供一个简单高效的在线营销工具，助力商家抢占线上流量的红利。能完美整合会员，外卖，线上支付，营销工具等餐厅经营过程中所需所有功能。扎根于微信的9亿用户流量，借助会员卡折扣，砍价等营销工具吸引线上用户到店消费。再通过多样化的营销促成客户进行二次消费，让老客户主动为商家带来新的客流。", func: [{ icon: "-0.54rem -1rem", text: "增加营销" }, { icon: "-1.08rem -1rem", text: "增加效率" }, { icon: "-1.62rem -1rem", text: "增加转换成交率" }, { icon: "-2.7rem -1rem", text: "餐厅会员" }, { icon: "-2.16rem -1rem", text: "点餐系统" }, { icon: "0 -1rem", text: "连接打印机" }] },
            { name: '行业版', text: '行业版，适用于所有行业。内置14种功能组件，其中包括视频，轮播图，背景音乐，在线客服等，用户可以根据不同行业的场景需求，自由选择搭配组件。 目前已有"餐饮，家政，酒店，ktv，美容美发美甲，健身房，建材，教育，汽车，房地产，宠物......"等十几个行业模板案例。', func: [{ icon: '-6.48rem -1.54rem', text: '功能组件' }, { icon: '-4.32rem -1.54rem', text: '视觉操作' }, { icon: '-3.78rem -1.54rem', text: '一键分享' }, { icon: '-5.4rem -1.54rem', text: '线上预约' }, { icon: '-4.86rem -1.54rem', text: '一键发布' }, { icon: '-5.94rem -1.54rem', text: '数据分析' }] },
            { name: '专业版', text: '专业版，具备行业版的自定义特点，内置16种功能组件，其中包括视频，轮播图，背景音乐，在线客服，以及电商直播。商家可以根据行业的需求选择组件，自由搭配。行业拥有丰富的营销插件——会员卡储值，会员折扣，砍价，拼团等。可以选择多种电商形式——内容电商，直播电商，社交电商。', func: [{ icon: '-6.48rem -1.54rem', text: '功能组件' }, { icon: '-6.48rem -1rem', text: '内容资讯' }, { icon: '-3.78rem -1.54rem', text: '一键分享' }, { icon: '-5.4rem -1.54rem', text: '线上预约' }, { icon: '-4.86rem -1.54rem', text: '快速发布' }, { icon: '-3.24rem -1rem', text: '直播插件' }, { icon: '0 -1.54rem', text: '会员系统' }, { icon: '-0.54rem -1rem', text: '营销插件' }] },
        ],
        solution: [
            { name: "汽车4s", icon: ["-7rem 0", "-1rem 0"], text: "4s店小程序对行业资源整合进行深入思考，就“成交客户满意度调研，客户服务，车友信息交流”3个关键点，进行深入开发。", func: ["店铺管理", "附近展示", "动态表单", "会员管理", "在线客服", "信息资讯", "线上支付", "数据汇总"] },
            { name: "房地产", icon: ["-3.5rem -0.5rem", "-5rem 0"], text: "房地产小程序具有房产中介必需的基础功能，提高企业服务效率，实现了服务与人的连接。", func: ["店铺管理", "楼盘资讯", "视频展示", "在线预约", "会员管理", "数据汇总"] },
            { name: "健身房", icon: ["-2.5rem -0.5rem", "-4rem 0"], text: "健身房小程序链接商家和客户，帮助商家通过店铺的高品质内容来吸引微信9亿级活跃用户流量，帮助提高用户的身体数值，提高商家的曝光率和交易额。", func: ["店铺管理", "资讯发布", "教练在线", "预约到店", "会员管理", "数据汇总"] },
            { name: "家政", icon: ["-1.5rem -0.5rem", "-3rem 0"], text: "家政小程序深入家政行业，经过调研分析市场需求，旨在解决家政公司的痛点，提供了一整套行业方法，帮助商家获取精准客户，高效管理家政人员，利用会员管理系统维护老客户，引导他们再次消费。", func: ["店铺管理", "在线客服", "线上预约", "支付便捷", "会员管理", "数据汇总"] },
            { name: "电商", icon: ["-3rem -0.5rem", "-4.5rem 0"], text: "电商版未程序为零售提供最先进的行业解决方案，帮助零售业打破自身局限，搭建微信端电商销售体系，实现流量聚合。", func: ["图片导航", "轮播图", "多属性设置", "商品排序", "消息模板", "会员管理", "砍价", "连接云打印机"] },
            { name: "KTV", icon: ["-2rem -0.5rem", "-3.5rem 0"], text: "KTV小程序旨在用数据和产品帮助线下KTV商家，口碑KTV行业解决方案链接商家和用户，围绕商家，不仅给店铺带来微信9亿活跃用户的流量，预约包厢、地图导航，在线支付等智慧KTV的解决方案。", func: ["会员管理", "营销推广", "门店管理", "店铺管理", "资讯内容", "自助点餐", "在线预约", "数据汇总"] },
            { name: "美容美发", icon: ["0 -0.5rem", "-1.5rem 0"], text: "美容美发小程序可以让消费者需要更多的行业信息，帮助他进门店及手艺人的选择，并且当办理了会员服务，可以获取更多品质的会员服务，根据会员等级享受折扣权限。", func: ["店铺管理", "线上预约", "支付便捷", "会员管理", "销售推广", "门店管理"] },
            { name: "家装建材", icon: ["-0.5rem -0.5rem", "-2rem 0"], text: "家装建材小程序，场景囊括线上到线下，致力为家装公司提供一套，高效便捷的装修行业解决方案。", func: ["店铺管理", "装修案例", "工地地图", "线上预约", "会员管理", "专家在线", "支付便捷", "家装商城"] },
            { name: "教育培训", icon: ["-1rem -0.5rem", "-2.5rem 0"], text: "教育培训小程序，可以帮助机构塑造品牌形象全面展示机构荣誉，学习环境，师资力量，课程内容帮助机构更好招收学员，服务客户，增加互动。", func: ["招生问题", "管理问题", "家校互动问题"] },
            { name: "医美", icon: ["-6.5rem 0", "-0.5rem 0"], text: "医美小程序为医美机构带来什么产品细节展示，一键预约，选择美容师，美容产品商城，线上结算支付，会员系统管理，附近5公里展示，数据汇总统计，在线客服。", func: ["店铺管理", "咨讯内容", "美容师预约", "线上商城", "在线客服", "会员管理"] },
            { name: "宠物", icon: ["-4rem -0.5rem", "-5.5rem 0"], text: "宠物小程序可以快速让顾客了解自己想了解的信息，通过视频直观了解宠物状况，也可以主动咨询在线客服，极大方便了客户，也方便了商家管理和盈利。", func: ["店铺管理", "宠物资讯", "视频展示", "在线预约", "在线客服", "会员系统", "数据汇总", "宠物商城"] },
            { name: "餐饮", icon: ["-6rem 0", "0 0"], text: "餐饮小程序为实体餐饮门店提供一个简单高效的在线营销工具，助力商家抢占线上流量的红利。", func: ["增加营收", "增加效率", "流量成交率", "餐厅会员", "销售活动", "点餐系统", "连接打印机"] },
        ],
        caseLabel: [{ TName: "全部", Id: 0 }, ],
        solutionIndex: 1,
        solutionParams: {
            tagName: "汽车4s",
            pageIndex: 1,
            pageSize: 4,
            userAccountId: userAccountId,
        },
        style: "cover",
        solutionCase: [],
        dlsData: {
            companyName: "",
            logo: "",
            description: "",
            keyWords: "",
            ICPNumber: "",
            telephone: "",
            email: "",
            address: "",
            aboutUs: [],
            banner: [],
            bannerBtnToggle: false
        },
        information: {
            name: "",
            phone: "",
            content: ""
        },
        caseParams: {
            tid: -999,
            pageIndex: 1,
            pageSize: 8,
            userAccountId: userAccountId,
        },
        productCase: {
            caseList: [],
            warn: true,
            showWarnText: false,
            warnText: "",
        },
        videoUrl: "",
    },

    mounted() {
        this.getSolutionCaseList();
        this.getCaseXcxTemplate();
        this.getProductCase();
        //  this.getAboutUsData();
    },
    methods: {
        getTpDetails: function (i) {
            indexDl.templateNum = i;
        },
        getSolutionDetails: function (i) {
            indexDl.solutionNum = i;
            indexDl.solutionParams.tagName = indexDl.solution[i].name;
            //indexDl.solutionIndex = i + 1;
            indexDl.getSolutionCaseList();
        },
        getProductDetails: function (i) {
            indexDl.pdCaseLabelNum = i;
            if (i > 0) {
                indexDl.caseParams.tid = indexDl.caseLabel[i].Id;
                indexDl.caseParams.pageIndex = 1;
            } else if (i === 0) {
                indexDl.caseParams.tid = -999;
            }
            indexDl.getProductCase();
        },
        getMoreCase: function () {
            indexDl.productCase.caseList = [];
            indexDl.caseParams.pageIndex = indexDl.caseParams.pageIndex + 1;
            indexDl.getProductCase();
        },
        getSolutionCaseList: function () {
            var that = this
            $.ajax({
                type: "POST",
                url: "/agentmanager/GetAgentinfoCaseList",
                data: that.solutionParams,
                success: function (res) {
                    if (res.isok) {
                        indexDl.solutionCase = res.dataObj.list;
                    } else {
                        console.log(res.Msg);
                    }
                },
                error: function (err) {
                    console.log("GetCaseList:err");
                }
            })
        },
        getCaseXcxTemplate: function () {
            var that = this
            $.ajax({
                type: "POST",
                url: "/agentmanager/GetCaseXcxTemplate",
                data: { userAccountId: userAccountId },
                success: function (res) {
                    if (res.isok) {
                        that.caseLabel = that.caseLabel.concat(res.dataObj);
                    } else {
                        console.log(res.Msg);
                    }
                },
                error: function (err) {
                    console.log(err);
                }
            })
        },
        getProductCase: function () {
            var that = this
            $.ajax({
                type: "POST",
                url: "/agentmanager/GetAgentinfoCaseList",
                data: that.caseParams,
                success: function (res) {
                    if (res.isok) {
                        var resList = res.dataObj.list;
                        var pageIndex = indexDl.caseParams.pageIndex;
                        var pageSize = indexDl.caseParams.pageSize;
                        indexDl.productCase.caseList = resList;
                        //indexDl.productCase.warn = resList.length > pageSize-1 ? true : false;
                        indexDl.productCase.showWarnText = resList.length === 0 ? true : false
                        if (resList.length === 0) {
                            indexDl.productCase.warn = false;
                            if (pageIndex === 1) {
                                indexDl.productCase.warnText = "暂无案例";
                            } else if (pageIndex > 1) {
                                indexDl.productCase.warnText = "没有更多案例了"
                            }
                        } else if (resList.length > 0) {
                            indexDl.productCase.warn = true;
                        }

                    } else {
                        console.log(res.Msg);
                    }
                },
                error: function (err) {
                    console.log(err);
                }
            })

            //$.ajax({
            //    type: "POST",
            //    url: "/dzhome/GetCaseList ",
            //    data: {
            //        id: that.productCase.id,
            //        pageIndex: that.productCase.pageIndex,
            //        pageSize: that.productCase.pageSize,
            //    },
            //    success: function (data) {
            //        if (data != undefined && data != null) {
            //            if (data.isok) {
            //                indexDl.productCase.caseList = data.cases;
            //                if (data.cases.length != 0) {
            //                    indexDl.productCase.warn = false;
            //                } else {
            //                    indexDl.productCase.warn = true;
            //                }
            //            }
            //        }
            //    },
            //    error: function (err) {
            //        console.log("GetCaseList:err");
            //    }
            //})

        },
        playVideo: function () {
            indexDl.videoUrl = "http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/video/20180205/ef92ff22b9fd4c899c072b1c2d3731df.mp4";
        },
        submitInformation: function () {
            var mask = layer.load(0);
            $.ajax({
                type: "POST",
                url: "/agentmanager/SetAgentWebSiteQuetion",
                data: {
                    telephone: indexDl.information.phone,
                    userName: indexDl.information.name,
                    question: indexDl.information.content,
                },
                success: function (data) {
                    layer.close(mask)
                    if (data != undefined && data != null) {
                        layer.msg(data.msg);
                        indexDl.information.phone = "";
                        indexDl.information.name = "";
                        indexDl.information.content = ""
                    }
                },
                error: function (err) {
                    console.log("err");
                }
            })
        }

    }
})
window.onload = function () {

    //导航条定位内容
    var arrOffsetTop = [
        $('#bannerSwiper').offset().top,
        $('.template-maket').offset().top,
        $('.solution').offset().top,
        $('.product-case').offset().top,
        $('.about-us').offset().top,
        $('.contact-us').offset().top,
    ];
    $(".lis").click(function () {
        $('body, html').animate({ scrollTop: arrOffsetTop[$(this).attr("data-index")] - $("#nav").height() }, 500);
    });



    var bannerSwiper = new Swiper('#bannerSwiper', {
        direction: 'horizontal',
        //loop: true,
        autoplay: 3000,
        autoplayDisableOnInteraction: false,
        speed: 800,
        prevButton: '.swiper-button-prev',
        nextButton: '.swiper-button-next',
        grabCursor: true,
        pagination: '.swiper-pagination',
        paginationType: 'custom',//自定义分页器
        paginationCustomRender: function (swiper, current, total) {
            var customPaginationHtml = "";
            for (var i = 0; i < total; i++) {
                //判断哪个分页器此刻应该被激活
                if (i == (current - 1)) {
                    customPaginationHtml += '<span class="swiper-pagination-customs swiper-pagination-customs-active"></span>';
                } else {
                    customPaginationHtml += '<span class="swiper-pagination-customs"></span>';
                }
            }
            return customPaginationHtml;
        }
    })
    var aboutSwiper = new Swiper('#aboutSwiper', {
        direction: 'horizontal',
        //loop: true,
        //autoplay: 3000,
        autoplayDisableOnInteraction: false,
        speed: 800,
        prevButton: '.swiper-button-prev',
        nextButton: '.swiper-button-next',
        grabCursor: true,
        pagination: '.swiper-pagination',
        paginationType: 'custom',//自定义分页器
        paginationCustomRender: function (swiper, current, total) {
            var customPaginationHtml = "";
            for (var i = 0; i < total; i++) {
                //判断哪个分页器此刻应该被激活
                if (i == (current - 1)) {
                    customPaginationHtml += '<span class="swiper-pagination-customs swiper-pagination-customs-active"></span>';
                } else {
                    customPaginationHtml += '<span class="swiper-pagination-customs"></span>';
                }
            }
            return customPaginationHtml;
        }
    })
}
