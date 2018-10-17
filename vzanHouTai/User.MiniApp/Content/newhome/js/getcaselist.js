; (function () {
    var pagedata = {
        templateList: [],
        initTemplateIndex: 0,
    };
    $.extend(true, pagedata, initData);
    Vue.use(VueLazyload)

    var app = new Vue({
        el: "#app",
        data: {

            optionList: ["汽车4S", "房地产", "健身房", "家政", "电商", "KTV", "美容美发", "家装建材", "教育培训", "医美", "宠物", "汽车美容", "餐饮"],
            pageMore: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/dy/logo-video.png?v1", title: "视频展示", content_1: "视频是目前最生动有趣的内容触达手段", content_2: "通过视频，可以全面展示企业和商家的特色与风格", content_3: "更容易让人接受" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/dy/logo-music.png?v1", title: "背景音乐", content_1: "音乐具有极大的情绪感染力和情感传达能力", content_2: "如果能在小程序中听到与小程序风格相同", content_3: "风格一致的音乐，自然就会多停留驻足" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/dy/logo-map.logo.png?v1", title: "地图导航", content_1: "帮助用户直接找到商家或企业的营业地址", content_2: "可以调用手机里的导航", content_3: "能适应不同受众的用户习惯" }
            ],
            show: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/cy/logo-catering1.png?v1", title: "营销角度：增加营收", content_1: "引客流：会员折扣", content_2: "提客单：菜品介绍", content_3: "顾客回流：会员储值" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/cy/logo-catering2.png?v1", title: "服务角度：增加效率", content_1: "加速开台：在线预定", content_2: "加速翻台：扫码点菜", content_3: "加速清台：在线支付，云打印，即出小票" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/cy/logo-catering3.png?v1", title: "流量：增加转化成交率", content_1: "流量搜集：41个流量入口", content_2: "流量承接：可以对接公众号", content_3: "流量运营：大数据分析精准营销" }
            ],
            manager: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/pro_sys.png?v1", title: "餐厅会员系统", content_1: "低成本建立会员档案", content_2: "自定义会员等级和商品折扣", content_3: "促进顾客回流" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/pro_active.png?v1", title: "餐厅营销活动", content_1: "会员营销帮助商家轻松实现多场景", content_2: "多对象，多类型的营销", content_3: "极大提升营销效率" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/pro_eat.png?v1", title: "点餐系统", content_1: "由小未餐饮版后台生成分桌二维码", content_2: "提供给商家张贴餐桌上", content_3: "便于消费者到店扫码享受便捷点单的服务" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/pro_in.png?v1", title: "连接打印机", content_1: "点餐结束后", content_2: "后厨与前台即出小票", content_3: "极大提高点单效率" },
            ],
            eclist_1: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/ds/logo_nav.png?v1", title: "图片导航", context: "用形象美观的图标对商品做出分类导航，支持图标和名称自定义。引导消费者找到心仪商品", },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/ds/logo_ecbanner.png?v1", title: "轮播图", context: "banner是一种广告展示形式，banner一般是放在小程序的顶部位置，在用户浏览小程序信息的同时，吸引用户对于焦点信息的关注", }
            ],
            eclist_2: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/ds/logo_spec1.png?v1", title: "多属性设置", context: "支持多属性设置，展示商品规格，消费者根据需求选择适合的商品属性，让消费者使用更加便捷", },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/ds/logo_spec2.png?v1", title: "商品排序", context: "支持多级分类，多类多级化，支持自定义排序，根据商家的运营和活动需要，置顶商品，精确推广商品", }
            ],
            moreFunc: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/ds/logo_mess.png?v1", title: "消息模板", context: " 对消费者进行消息推送，根据订单的状态自动推送消息模板给顾客微信增加互动频率" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/ds/logo_vip.png?v1", title: "会员管理系统", context: " 对接微信卡包，支持会员卡储值，会员折扣，商品SKU，会员等级权益独立管理，各元素组件支持自定义。还能直接跳转到小程序，公众号。" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/ds/logo_kan.png?v1", title: "砍价", context: " 砍价已经成为线上商城必不可少的营销手段。用低价吸引了真实用户的同时也达到了吸粉的作用" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/ds/logo_da.png?v1", title: "连接云打印机", context: " 下单结束后，立即打出小票，极大提高订单管理效率。" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/ds/logo_map.png?v1", title: "附近5公里自动展示", context: " 相当于定点投放广告到方圆5公里的用户微信里" },
            ],
            pro: [
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-01.png?v1", title: "在线商城", context: "内置14种功能组件，根据需求来选择搭配组件" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-02.png?v1", title: "知识付费", context: "随意拖拽组件完成小程序界面设计，提高搭建效率" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-03.png?v1", title: "电商直播", context: "自带7种分享样式图，一键转发朋友圈" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-04.png?v1", title: "二级分销", context: "提前预订，顾客不用排队等待，商家合理排版" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-05.png?v1", title: "客户管理", context: "小未程序无需打包下载，一键完成代码升级" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-06.png?v1", title: "视频资讯", context: "基于小程序的用户大数据，为商家提供用户画像分析方便商家进行精准营销和运营。" },
            ],
            proNbFunc: [
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/pro-function01.png?v1", title: "拼团优惠", context: "会员卡储值，会员折扣" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/pro-function02.png?v1", title: "限时秒杀", context: "砍价营销，促销利器" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/pro-function03.png?v1", title: "签到积分", context: "多人拼团，分享好友，疯狂裂变" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/pro-function01.png?v1", title: "积分商城", context: "会员卡储值，会员折扣" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/pro-function02.png?v1", title: "优惠券", context: "砍价营销，促销利器" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/pro-function03.png?v1", title: "社交立减金", context: "多人拼团，分享好友，疯狂裂变" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/pro-function01.png?v1", title: "砍价优惠", context: "会员卡储值，会员折扣" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/pro-function02.png?v1", title: "多会员折扣", context: "砍价营销，促销利器" },
            ],
            proOther: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-01.png?v1", title: "在线客服", },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-02.png?v1", title: "联系我们", },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-03.png?v1", title: "业务预约", },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-04.png?v1", title: "预存充值", },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-05.png?v1", title: "数据分析", },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-06.png?v1", title: "地图导航", },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-06.png?v1", title: "图片魔方", },
                { img: "http://j.vzan.cc/dz/content/newhome/image/templateMarket/zy/icon-pro-06.png?v1", title: "排队叫号", },
            ],
            basis: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/car4s/car4s-logo-1.png?v1", title: "店铺管理", context_1: "展示营业执照、门店环境照片", context_2: " 可提供的服务、营业时间、地址等信息" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/car4s/car4s-logo-2.png?v1", title: "附近展示，精准引流", context_1: "汽车服务维修保养一般“就近原则”", context_2: " 使用小未4s店小程序，可以在方圆5公里的手机里自动出现，帮助商家找到精准客户" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/car4s/car4s-logo-3.png?v1", title: "动态表单，在线调研", context_1: "自定义动态表单", context_2: " 能对成交客户进行阶段性调研", context_3: "方便商户进行总结规划" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/car4s/car4s-logo-4.png?v1", title: "会员管理，顾客回流", context_1: "支持会员储值，会员卡折扣", context_2: " 会员卡放入微信卡包", context_3: "直接跳转到小程序里，用户使用更方便" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/car4s/car4s-logo-5.png?v1", title: "在线客服，贴身服务", context_1: "支持在线客服功能", context_2: " 客户能在小程序中随时得到问题解答", context_3: "进一步提高服务质量" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/car4s/car4s-logo-6.png?v1", title: "信息共享，资讯更新", context_1: "提供给车友获取信息的好去处", context_2: " 商家也可以把4s店的最新活动", context_3: "和产品更新分享给车友" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/car4s/car4s-logo-7.png?v1", title: "便捷支付，灵活结算", context_1: "提供了线上商城，增加4s店的服务项目", context_2: " 客户可以直接使用微信进行支付", context_3: "增加店铺营收" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/car4s/car4s-logo-8.png?v1", title: "数据汇总", context_1: "基于小程序的用户大数据", context_2: " 为商家提供浏览过小程序的用户画像", context_3: "通过用户分析，访问分析，订单分析", context_4: "精准营销和运营，庞大数据来推动门店发展" },
            ],
            realty: [
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/realty/realty-function-01.png?v1", title: "店铺管理", context_1: "线下健身机构环境", context_2: "服务项目，优惠信息同步展示到线上店铺" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/realty/realty-function-02.png?v1", title: "楼盘资讯", context_1: "可以及时展示楼盘的资讯", context_2: "中介把自己的信息直接传达给客户", context_3: "并且不需要借助第三方平台" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/realty/realty-function-03.png?v1", title: "视频展示", context_1: "用最直观的方式来展示房子的状况", context_2: "有利于客户最快找到心仪的房子" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/realty/realty-function-04.png?v1", title: "在线预约", context_1: "提前预约现场看房", context_2: "能提高效率" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/realty/realty-function-05.png?v1", title: "会员管理", context_1: "低成本建立档案", context_2: "为在小程序找到房源的提供及时服务", context_3: "再次关顾时，可以自定义折扣，增加粘性" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/realty/realty-function-06.png?v1", title: "数据汇总", context_1: "基于小程序的用户数据，为商家提供浏览过小程序的用户画像，可以通过用户分析", context_2: "访问分析，订单分析，精准营销和运营", context_3: "用庞大数据来推动门店发展" },
            ],
            gymnasium: [
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/gymnasium/gymnasium-function-01.png?v1", title: "店铺管理", context_1: "线下健身机构环境", context_2: "服务项目，优惠信息同步展示到线上店铺" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/gymnasium/gymnasium-function-02.png?v1", title: "资讯发布", context_1: "发布健身相关干货文章 ", context_2: "帮助引导线上流量到店消费" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/gymnasium/gymnasium-function-03.png?v1", title: "教练在线", context_1: "随时随地都能咨询专业问题", context_2: "打破空间和时间限制", context_3: "拉近健身机构和客户的距离" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/gymnasium/gymnasium-function-04.png?v1", title: "预约到店", context_1: "用户可自由预约到店时间", context_2: "预约教练预约健身课程" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/gymnasium/gymnasium-function-05.png?v1", title: "会员管理", context_1: "支持会员卡储值、折扣", context_2: "低成本建立健身会员档案", context_3: "自定义会员等级和商品折扣，促进顾客回流" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/gymnasium/gymnasium-function-06.png?v1", title: "数据汇总", context_1: "基于小程序的用户数据，为商家提供浏览过小程序的用户画像，可以通过用户分析", context_2: "访问分析，订单分析，精准营销和运营", context_3: "用庞大数据来推动门店发展" },
            ],
            homemaking: [
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/homemaking/homemaking-function-01.png?v1", title: "店铺管理", context_1: "展示营业执照、门店环境照片", context_2: "可提供的服务、营业时间、地址等信息" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/homemaking/homemaking-function-02.png?v1", title: "在线客服", context_1: "第一时间对接意向客户" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/homemaking/homemaking-function-03.png?v1", title: "线上拓客，灵活预约", context_1: "顾客在线预约家政服务员和服务类目", context_2: "商家及时做出人员安排，提高工作效率", context_3: "还通过附近展示，获取本地的精准用户" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/homemaking/homemaking-function-04.png?v1", title: "便捷支付，灵活结算", context_1: "实时更新订单状态", context_2: "可以导出表单", context_3: "方便商家进行管理结算" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/homemaking/homemaking-function-05.png?v1", title: "会员管理，顾客回流", context_1: "支持会员储值，会员卡折扣", context_2: "会员卡放入微信卡包", context_3: "直接跳转到小程序里，用户使用更方便" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/homemaking/homemaking-function-06.png?v1", title: "数据汇总", context_1: "基于小程序的用户数据，为商家提供浏览过小程序的用户画像，可以通过用户分析", context_2: "访问分析，订单分析，精准营销和运营", context_3: "用庞大数据来推动门店发展" },
            ],
            ktv: [
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/ktv/ktv-function-01.png?v1", title: "店铺管理", context_1: "展示营业执照、门店环境照片", context_2: "可提供的服务、营业时间、地址等信息" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/ktv/ktv-function-02.png?v1", title: "资讯内容", context_1: "资讯内容组件支持视频和富文本", context_2: "可以完美展示ktv近期的活动", context_3: "是宣传企业文化和推广的一大利器" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/ktv/ktv-function-03.png?v1", title: "自助点餐点酒水", context_1: "为商家提供手机点单功能", context_2: "提高服务效率" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/ktv/ktv-function-04.png?v1", title: "在线预定包厢", context_1: "为店铺带来订单", context_2: "为商家提供包厢预订服务" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/ktv/ktv-function-05.png?v1", title: "数据汇总", context_1: "基于小程序的用户大数据", context_2: "为商家提供数据分析", context_3: "方便商家进行精准营销和运营" },
            ],
            hairdressing: [
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/hairdressing/hairdressing-function-01.png?v1", title: "店铺管理", context_1: "展示营业执照、门店环境照片", context_2: "可提供的服务、营业时间、地址等信息" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/hairdressing/hairdressing-function-02.png?v1", title: "线上拓客，灵活预约", context_1: "在线预约服务，到店时间，发型设计师", context_2: "商家合理排班。提高工作效率", context_3: "通过附近展示吸引精准客户到店消费" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/hairdressing/hairdressing-function-03.png?v1", title: "便捷支付，灵活结算", context_1: "线上选好设计和发型款式直接微信支付", context_2: "商户直观地了解订单状态", context_3: "导出订单，方便进行结算" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/hairdressing/hairdressing-function-04.png?v1", title: "会员管理，顾客回流", context_1: "支持会员储值，折扣", context_2: "会员卡放入微信卡包，可直接跳转到小程序里", context_3: "促进顾客回流" },
            ],
            decoration: [
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/decoration/decoration-function-01.png?v1", title: "店铺管理", context_1: "展示营业执照、门店环境照片", context_2: "可提供的服务、营业时间、地址等信息" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/decoration/decoration-function-02.png?v1", title: "精选装修案例展示", context_1: "自主维护，轻松发布资讯", context_2: "把每个客户案例都变成自己的营销工具" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/decoration/decoration-function-03.png?v1", title: "工地地图，谈单利器", context_1: "精确定位店铺位置", context_2: "引导客户到店洽谈" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/decoration/decoration-function-04.png?v1", title: "线上拓客，灵活预约", context_1: "通过附近展示，精确覆盖半径5公里", context_2: "顾客在线预约装修服务和时间", context_3: "直接预约设计师，提高工作效率" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/decoration/decoration-function-05.png?v1", title: "会员管理，顾客回流", context_1: "支持会员储值、折扣", context_2: "会员卡放入微信卡包可直接跳转到小程序里", context_3: "用户使用更加方便" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/decoration/decoration-function-06.png?v1", title: "专家在线", context_1: "随时随地都能咨询装修问题", context_2: "打破空间和时间限制", context_3: "给客户提供一个和专家交流的平台" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/decoration/decoration-function-07.png?v1", title: "便捷支付，灵活结算", context_1: "客户在线上选好设计和装修风格", context_2: "可以直接微信支付，简单便捷", context_3: "商户直可以导出订单，方便进行结算" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/decoration/decoration-function-08.png?v1", title: "家装商城", context_1: "能展示家居产品", context_2: "有利于客户购买风格一致的搭配家具", context_3: "也能增加商家的收入，实现利人利己" },
            ],
            education: [
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/education/icon-education01.png?v1", title: "招生问题", context_1: "小程序推广成本低，效果好", context_2: "提供一个展示机构品牌机构形象的平台", context_3: "给出转化用户的解决方案" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/education/icon-education02.png?v1", title: "管理问题", context_1: "会员管理系统", context_2: "导出报名表单", context_3: "解决机构管理问题" },
                 { img: "http://j.vzan.cc/dz/content/newhome/image/solution/education/icon-education03.png?v1", title: "家校互动问题", context_1: "在线客服，随时沟通", context_2: "内容资讯分发，分享校内活动", context_3: "在家就能了解培训机构的动态" },
            ],
            medicine_01: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/medicine/medicine-fun-01.png?v1", title: "店铺管理", context_1: "展示营业执照、门店环境照片", context_2: "可提供的服务、营业时间、地址等信息" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/medicine/medicine-fun-02.png?v1", title: "咨询内容", context_1: "咨询内容组件支持视频和富文本", context_2: "可以完美展示医美机构近期的活动", context_3: "是宣传和推广的一大利器" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/medicine/medicine-fun-03.png?v1", title: "美容师预约", context_1: "可以通过小程序，顾客线上提前预约美容师和时间，到店消费不等待" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/medicine/medicine-fun-04.png?v1", title: "线上商城", context_1: "展示并出售美容产品，为医美机构带来更多盈利。引导新老顾客再次消费" },
            ],
            medicine_02: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/medicine/medicine-support-01.png?v1", title: "在线客服", context_1: "通过小程序，就能在线上对美容师进行咨询", context_2: "进一步了解医美细节" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/medicine/medicine-support-02.png?v1", title: "会员管理系统", context_1: "对接微信卡包，支持会员卡储值、折扣、商品SKU，会员等级权益独立管理，各元素组件支持自定义。", context_2: "还能直接跳转到小程序，公众号!!!" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/medicine/medicine-support-03.png?v1", title: "美容师预约", context_1: "商户可以基于小程序的用户数据，为商家提供浏览过小程序的用户，分析用户的画像，年龄，性别等详情，方便商家进行精准营销和运营" },
            ],
            pet: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/pet/pet-fun-01.png?v1", title: "店铺管理", context_1: "展示营业执照、门店环境照片", context_2: "可提供的服务、营业时间、地址等信息" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/pet/pet-fun-02.png?v1", title: "宠物资讯", context_1: "可以用来展示宠物店的近期活动，店长分享小秘诀，分享宠物趣事，加强顾客和店家的交流和互动，增加用户留存时间" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/pet/pet-fun-03.png?v1", title: "视频展示", context_1: "用视频来展示宠物的状态", context_2: "更加具有真实性和趣味性" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/pet/pet-fun-04.png?v1", title: "在线预约", context_1: "用户可以提前预约到店时间和服务", context_2: "带着宠物到店消费，无需等待" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/pet/pet-fun-05.png?v1", title: "在线客服", context_1: "贴心的宠物管家", context_2: "随时为顾客解答宠物的相关问题" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/pet/pet-fun-06.png?v1", title: "会员系统", context_1: "低成本建立宠物店会员档案", context_2: "自定义会员等级和折扣，促进顾客回流" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/pet/pet-fun-07.png?v1", title: "数据汇总", context_1: "商户可以基于小程序的用户大数据,为商家提供用户分析，访问分析，订单分析,精准营销和运营，用庞大数据来推动门店发展" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/pet/pet-fun-08.png?v1", title: "宠物商城", context_1: "在线出售宠物，用具和宠物食品", context_2: "可以在宠物的详情页面附上视频和图片" },
            ],
            autoBeauty: [
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/autoBeauty/autoBeauty-fun-01.png?v1", title: "店铺管理", context_1: "上传营业执照、门店环境照片", context_2: "可提供的服务、营业时间等开店基本的信息", context_3: "能展示门店环境、地址等信息" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/autoBeauty/autoBeauty-fun-02.png?v1", title: "咨询发布", context_1: "发布相关汽车美容的知识", context_2: "汽车美容店的最近活动，引导线上流量到店消费" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/autoBeauty/autoBeauty-fun-03.png?v1", title: "在线客服", context_1: "随时随地都能咨询汽车美容专业问题，打破空间和时间限制，拉近汽车美容店家和客户的距离" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/autoBeauty/autoBeauty-fun-04.png?v1", title: "预约到店", context_1: "用户可自由预约到店时间，预约服务", context_2: "不用排队等待，浪费宝贵时间" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/autoBeauty/autoBeauty-fun-05.png?v1", title: "会员管理", context_1: "支持会员卡储值、折扣", context_2: "低成本建立健身会员档案", context_3: "自定义会员等级和商品折扣，促进顾客回流" },
                { img: "http://j.vzan.cc/dz/content/newhome/image/solution/autoBeauty/autoBeauty-fun-06.png?v1", title: "数据汇总", context_1: "会员营销数据+用户数据", context_2: "为商家提供用户分析，访问分析，订单分析", context_3: "精准营销和运营，庞大数据来推动门店发展" },
            ],
            optionNum: 0,
            optionName: "汽车4S",
            versionsId: "4",
            hashId: "",
            recordCount: 0,
            selectTemplateIndex: 0,
            selectTagIndex: 0,
            xcxTemplates: pagedata.templateList,
            cases: [],
            tags: [],
            pageIndex: 1,
            pageSize: 8,
            tagId: 0,
            id: 0,
            loadmore: false,
            scrollLoading: false,
            selTemplateIndex: pagedata.initTemplateIndex,
        },
        watch: {
            mytemplates: {
                handler: function (nums) {
                    nums.forEach(function (o, i) {
                        switch (o.Type) {
                            case 1: o.iosimg = "s-gw.png"; break; //官网
                            case 4: o.iosimg = "s-dy.png"; break; //单页版
                            case 6: o.iosimg = "s-ds.png"; break; //电商版
                            case 8: o.iosimg = "s-cy.png"; break; //餐饮版
                            case 12: o.iosimg = "s-jc.png"; break; //行业版
                            case 22: o.iosimg = "s-gj.png"; break; //专业版
                            default: o.iosimg = "s-gj.png"; break; //专业版
                        }
                    })

                },
                deep: true
            }
        },
        methods: {
            toggleOption: function (i, e) {
                this.optionNum = i
                this.optionName = app.xcxTemplates[i].TName;//e.target.innerText;
                this.versionsId = app.xcxTemplates[i].Id;//e.target.getAttribute("data-versionsId");
            },
            toggleSolution: function (i, e, id) {
                $.each(app.xcxTemplates, function (index, value) {
                    if (id == value.Id) {
                        app.optionNum = index;
                    }
                });
                // this.optionNum = i
                this.optionName = e.target.innerText;
                this.versionsId = e.target.getAttribute("data-versionsId");
            },
            showCurrentSolution: function () {
                var hash = decodeURIComponent(window.location.hash.substring(1));
                this.optionName = hash;
                if (!hash) {
                    this.optionName = "汽车4S";
                } else {
                    this.optionName = hash;
                }
                var that = this;
                $.each(that.xcxTemplates, function (index, value) {
                    if (27 == value.Id) {
                        that.optionNum = index;
                    }
                });
                this.id = 27;
                this.GetCaseList();
            },
            showCurrentVersions: function () {
                var hash = window.location.hash.substring(1);
                if (!hash) {
                    this.versionsId = "4";
                } else {
                    this.versionsId = hash;
                }
            },
            GetDataByTid: function (index) {
                this.selTemplateIndex = index;
                if (index >= 0) {
                    this.id = this.xcxTemplates[index].Id;
                } else {
                    this.id = 0;
                }
                this.tagId = 0;
                this.pageIndex = 1;

                this.GetCaseList();
            },
            GetDataByTagId: function (index) {
                if (index >= 0) {
                    this.tagId = this.tags[index].id;
                } else {
                    this.tagId = 0;
                }
                this.pageIndex = 1;
                this.GetCaseList();
            },
            LoadMoreData: function () {
                this.pageIndex++;
                this.loadmore = true;
                this.GetCaseList();
            },
            GetCaseList: function () {
                var layerIndex = layer.load(2);
                $.post("/dzhome/GetCaseList", { id: this.id, tagId: this.tagId, pageIndex: this.pageIndex, pageSize: this.pageSize }, function (data) {
                    layer.close(layerIndex);
                    if (data.isok) {
                        app.recordCount = data.recordCount;
                        if (app.loadmore) {
                            app.loadmore = false;
                            app.scrollLoading = false;

                            app.cases = app.cases.concat(data.cases);
                            app.tags = data.tags;
                        } else {
                            app.cases = data.cases;
                            app.tags = data.tags;
                        }
                    } else {
                        layer.msg(data.msg);
                    }
                }, "json")
            },
            //试用模板
            TestTemplate: function (tid) {
                var formdata = { tid: tid };
                $.post("/dzhome/TestTemplate", formdata, function (data) {
                    layer.msg(data.Msg);
                    if (data.isok) {
                        setTimeout(function () {
                            window.location.reload();
                        }, 1000)
                    }
                    if (data.code == "-10") {
                        window.location.href = "/dzhome/login";
                    }
                }, "json")
            },
        },
        created: function () {
            this.GetDataByTid(this.selTemplateIndex);
        },
        mounted: function () {
            var currentPathname = window.location.pathname.split("/").pop();
            if (currentPathname == "templateMarket" || currentPathname == "solution") {
                this.showCurrentVersions();
                this.showCurrentSolution();
            }
        }
    })
    var isscroll = true;
    if (window.location.pathname == "/dzhome" || "/dzhome/newHome") {
        isscroll = false;
    }

    $(window).scroll(function () {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();
        //			滑到底部加载更多问题
        if ((scrollTop + windowHeight > (scrollHeight - 50)) && app.cases != null && app.recordCount > app.cases.length && !app.scrollLoading && isscroll) {
            app.scrollLoading = true;
            app.LoadMoreData();
        }
    });

})()





