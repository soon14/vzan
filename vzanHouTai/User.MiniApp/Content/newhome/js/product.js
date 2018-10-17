; (function () {
    var vm = new Vue({
        el: "#page-content",
        data: {

            singlepage: [{
                title: "单页版",
                description: "点赞单页版是点赞科技的基础版本，是现在市面上最强大的单页版，支持轮播图，视频，背景音乐，自定义表单等组件。支持随意拉拽，可视化界面设计。适用于所有行业，通过附近展示功能，可以定位店铺位置，为店铺带来大量客流，是商家网络名片的最佳选择。",
                bg_img: "/Content/newhome/image/page_bg.png",
                list_1: {
                    title: "轮播图",
                    img: "/Content/newhome/image/log-banner.png",
                    content: "banner是一种广告展示形式，banner一般是放在小程序的顶部位置，在用户浏览小程序信息的同时，吸引用户对于焦点信息的关注",
                    bg_showImg: "/Content/newhome/image/banner_show.png"
                },
                list_2: {
                    title: "富文本文本",
                    img: "/Content/newhome/image/logo-richtxt.png",
                    content: "文本不用说是人们相互间交流的工具，小程序的重要信息也是以文本为主。文本能准确的表达信息的内容和含义。富文本支持格式和图文，信息独特，排版优美的小程序文本很具吸引力。",
                    bg_showImg: "/Content/newhome/image/rich-show.png"
                },
                list_3: {
                    title: "图片",
                    img: "/Content/newhome/image/img_bg.png",
                    content: "图像在小程序中具有提供信息，展示形象，美化网页，表达小程序独特风格的重要作用",
                    bg_showImg: "/Content/newhome/image/img_bg.png"
                },
                list_4: {
                    title: "自定义表单",
                    img: "/Content/newhome/image/logo-form.png",
                    content: "企业用户可以通过自定义表单，根据自己的实际情况独立进行业务配置，来进行个性化、行业化定制。可以实现各个行业的在线预约需求。还支持导出表单，方便商家管理报名数据。",
                    bg_showImg: "/Content/newhome/image/form-show.png"
                },







            }],

            more: [
                { img: "/Content/newhome/image/logo-video.png", title: "视频展示", content_1: "视频是目前最生动有趣的内容触达手段", content_2: "通过视频，可以全面展示企业和商家的特色与风格", content_3: "更容易让人接受" },
                { img: "/Content/newhome/image/logo-music.png", title: "背景音乐", content_1: "音乐具有极大的情绪感染力和情感传达能力", content_2: "如果能在小程序中听到与小程序风格相同", content_3: "风格一致的音乐，自然就会多停留驻足" },
                { img: "/Content/newhome/image/logo-map.logo.png", title: "地图导航", content_1: "帮助用户直接找到商家或企业的营业地址", content_2: "可以调用手机里的导航", content_3: "能适应不同受众的用户习惯" }
            ],

            show: [
                { img: "/Content/newhome/image/logo_pro1.png", title: "营销角度：增加营收", content_1: "引客流：会员折扣", content_2: "提客单：菜品介绍", content_3: "顾客回流：会员储值" },
                {
                    img: "/Content/newhome/image/logo_pro2.png", title: "服务角度：增加效率", content_1: "加速开台：在线预定", content_2: "加速翻台：扫码点菜", content_3: "加速清台：在线支付，云打印，即出小票"
                },
                {
                    img: "/Content/newhome/image/logo_pro3.png", title: "流量：增加转化成交率", content_1: "流量搜集：41个流量入口", content_2: "流量承接：可以对接公众号", content_3: "流量运营：大数据分析精准营销"
                }
            ],
            manager: [
                { img: "/Content/newhome/image/pro_sys.png", title: "餐厅会员系统", content_1: "低成本建立会员档案", content_2: "自定义会员等级和商品折扣", content_3: "促进顾客回流" },
                {
                    img: "/Content/newhome/image/pro_active.png", title: "餐厅营销活动", content_1: "会员营销帮助商家轻松实现多场景", content_2: "多对象，多类型的营销", content_3: "极大提升营销效率"
                },
                {
                    img: "/Content/newhome/image/pro_eat.png", title: "点餐系统", content_1: "由点赞餐饮版后台生成分桌二维码", content_2: "提供给商家张贴餐桌上", content_3: "便于消费者到店扫码享受便捷点单的服务"
                },
                {
                    img: "/Content/newhome/image/pro_in.png", title: "连接打印机", content_1: "点餐结束后", content_2: "后厨与前台即出小票", content_3: "极大提高点单效率"
                },
            ],
            eclist_1: [
                { img: "/Content/newhome/image/logo_nav.png", title: "图片导航", context: "用形象美观的图标对商品做出分类导航，支持图标和名称自定义。引导消费者找到心仪商品", },
                { img: "/Content/newhome/image/logo_ecbanner.png", title: "轮播图", context: "banner是一种广告展示形式，banner一般是放在小程序的顶部位置，在用户浏览小程序信息的同时，吸引用户对于焦点信息的关注", }
            ],
            eclist_2: [
                { img: "/Content/newhome/image/logo_spec1.png", title: "多属性设置", context: "支持多属性设置，展示商品规格，消费者根据需求现在喜爱的商品，商品多属性选择，让消费者使用更加便捷", },
                { img: "/Content/newhome/image/logo_spec2.png", title: "商品排序", context: "支持多级分类，多类多级化，支持自定义排序，根据商家的运营和活动需要，置顶商品，精确推广商品", }
            ],
            moreFunc: [
                { img: "/Content/newhome/image/logo_mess.png", title: "消息模板", context: " 对消费者进行消息推送，根据订单的状态自动推送消息模板给顾客微信增加互动频率" },
                {
                    img: "/Content/newhome/image/logo_vip.png", title: "会员管理系统", context: " 对接微信卡包，支持会员卡储值、折扣商品SKU、，会员等级权益独立管理各元素组件支持自定义还能直接跳转到小程序，公众号！！！！"
                },
                { img: "/Content/newhome/image/logo_kan.png", title: "砍价", context: " 砍价已经成为线上商城必不可少的营销手段。可以用低价吸引了真实用户的同时也达到了吸粉的作用" },
                { img: "/Content/newhome/image/logo_da.png", title: "连接云打印机", context: " 下单结束后，立即打出小票，极大提高订单管理效率。" },
                { img: "/Content/newhome/image/logo_map.png", title: "附近5公里自动展示", context: " 相当于定点投放广告到方圆5公里的用户微信里" },
            ],

            basis: [
                { img: "/Content/newhome/image/logo_basis_1.png", title: "店铺管理", context_1: "展示营业执照、门店环境照片", context_2: " 可提供的服务、营业时间、地址等信息" },
                { img: "/Content/newhome/image/logo_basis_2.png", title: "附近展示，精准引流", context_1: "汽车服务维修保养一般“就近原则”", context_2: " 使用点赞4s店小程序，可以在方圆5公里的手机", context_3: "里自动出现，帮助商家找到精准客户" },
                {
                    img: "/Content/newhome/image/logo_basis_3.png", title: "动态表单，在线调研", context_1: "自定义动态表单", context_2: " 能对成交客户进行阶段性调研", context_3: "方便商户进行总结规划"
                },
                {
                    img: "/Content/newhome/image/logo_basis_4.png", title: "会员管理，顾客回流", context_1: "支持会员储值，会员卡折扣", context_2: " 会员卡放入微信卡包", context_3: "直接跳转到小程序里，用户使用更方便"
                },
                {
                    img: "/Content/newhome/image/logo_basis_5.png", title: "在线客服，贴身服务", context_1: "支持在线客服功能", context_2: " 客户能在小程序中随时得到问题解答", context_3: "进一步提高服务质量"
                },
                {
                    img: "/Content/newhome/image/logo_basis_6.png", title: "信息共享，资讯更新", context_1: "提供给车友获取信息的好去处", context_2: " 商家也可以把4s店的最新活动", context_3: "产品更新分享给车友"
                },
                {
                    img: "/Content/newhome/image/logo_basis_7.png", title: "便捷支付，灵活结算", context_1: "提供了线上商城，增加4s店的服务项目", context_2: " 客户可以直接使用微信进行支付", context_3: "增加店铺营收"
                },
                {
                    img: "/Content/newhome/image/logo_basis_8.png", title: "数据汇总", context_1: "基于小程序的用户大数据", context_2: " 为商家提供浏览过小程序的用户画像", context_3: "通过用户分析，访问分析，订单分析", context_4: "精准营销和运营，庞大数据来推动门店发展"
                },
            ]




        }
    })
})()
